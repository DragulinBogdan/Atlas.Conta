using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api.Pif;

// Agregatul punerii în funcțiune și proiecțiile ei (F26-D5/D10).
// CONTRACT DE APELANT: toate metodele rulează în ObjectSpace-ul SECURED al apelantului;
// `Aplica`/`Sterge` COMIT.
public static class PifApply {

    public static Guid Aplica(IObjectSpace os, Guid? id, PifWriteDto dto) {
        if (dto == null)
            throw new OperareException("Lipsește corpul cererii.");

        // F3-D5: tot ce poate refuza — inclusiv liniile — se rezolvă înaintea lui `CreateObject`.
        PunereInFunctiune doc = null;
        if (id is Guid existentId) {
            doc = Rezolva.Cere<PunereInFunctiune>(os, existentId, "Punerea în funcțiune");
            if (doc.Stare != StareDocument.Draft)
                throw new OperareException(
                    $"Documentul {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se mai modifică. "
                    + "Anulați operarea sau stornați-l.");
        }
        var predator = Rezolva.Cere<Repartitor>(os, dto.PredatorId, "Predatorul (unitatea internă)");
        var primitor = Rezolva.Cere<Repartitor>(os, dto.PrimitorId, "Primitorul (locul fișelor)");
        var rezolvate = RezolvaLinii(os, doc, dto.Linii ?? new List<PifLinieWriteDto>());
        doc ??= os.CreateObject<PunereInFunctiune>();

        doc.Data = dto.Data;
        doc.Predator = predator;
        doc.Primitor = primitor;

        MaterializeazaLinii(os, doc, rezolvate);

        os.CommitChanges();
        return doc.ID;
    }

    public static void Sterge(IObjectSpace os, Guid id) {
        var doc = Rezolva.Cere<PunereInFunctiune>(os, id, "Punerea în funcțiune");
        if (doc.Stare != StareDocument.Draft)
            throw new OperareException(
                $"Documentul {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se șterge. "
                + "Anulați operarea sau stornați-l.");

        os.Delete(doc.Detalii.ToList());
        os.Delete(doc);
        os.CommitChanges();
    }

    sealed record LinieRezolvata(PifLinieWriteDto Dto, PunereInFunctiuneDetaliu Existenta,
        Imobilizare Fisa, TipMaterial TipMaterial, DocumentDetaliu LinieSursa,
        FelLiniePif Fel, MetodaAmortizare? Metoda, MetodaAmortizare? MetodaFiscala,
        CategorieFiscala? Categorie);

    static List<LinieRezolvata> RezolvaLinii(IObjectSpace os, PunereInFunctiune doc,
            List<PifLinieWriteDto> linii) {
        var existente = doc?.Detalii.ToDictionary(d => d.ID) ?? new Dictionary<Guid, DocumentDetaliu>();
        var pastrate = new HashSet<Guid>();
        var fiseCerute = new HashSet<Guid>();
        var rezolvate = new List<LinieRezolvata>();

        foreach (var l in linii) {
            var fisa = Rezolva.Cere<Imobilizare>(os, l.ImobilizareId, "Fișa de imobilizare");
            if (!fiseCerute.Add(fisa.ID))
                throw new OperareException($"Fișa {Eticheta(fisa)} apare de două ori în cerere — "
                    + "un document poartă un singur eveniment per fișă.");
            // F26-D5: tipul liniei e al FIȘEI, nu al payload-ului.
            var tipMaterial = Rezolva.Cere<TipMaterial>(os, fisa.TipMaterialId,
                $"Tipul (contul/clasa) fișei {Eticheta(fisa)}");
            var linieSursa = Rezolva.Optional<DocumentDetaliu>(os, l.LinieSursaId, "Linia sursă de factură");
            var fel = ApiEnum.Membru<FelLiniePif>(l.Fel, "Felul liniei de punere în funcțiune");
            var metoda = ApiEnum.MembruOptional<MetodaAmortizare>(l.Metoda, "Metoda de amortizare");
            var metodaFiscala = ApiEnum.MembruOptional<MetodaAmortizare>(
                l.MetodaFiscala, "Metoda de amortizare fiscală");
            var categorie = ApiEnum.MembruOptional<CategorieFiscala>(l.CategorieFiscala, "Categoria fiscală");
            VerificaScara(l.Valoare, Scara.Bani, "Valoarea");
            VerificaScara(l.ValoareFiscala ?? 0m, Scara.Bani, "Valoarea fiscală");
            VerificaScara(l.AmortizareInitiala, Scara.Bani, "Amortizarea inițială");
            VerificaScara(l.AmortizareFiscalaInitiala, Scara.Bani, "Amortizarea fiscală inițială");
            VerificaScara(l.ValoareReziduala ?? 0m, Scara.Bani, "Valoarea reziduală");

            PunereInFunctiuneDetaliu existenta = null;
            if (l.Id is Guid linieId) {
                if (!existente.TryGetValue(linieId, out var gasita))
                    throw new OperareException($"Linia {linieId} nu aparține documentului "
                        + $"{(doc == null ? "cerut" : Eticheta(doc))}.");
                if (!pastrate.Add(linieId))
                    throw new OperareException($"Linia {linieId} apare de două ori în cerere.");
                if (gasita is not PunereInFunctiuneDetaliu aceeasi)
                    throw new OperareException($"Linia {linieId} nu e o linie de punere în funcțiune.");
                existenta = aceeasi;
            }
            rezolvate.Add(new LinieRezolvata(l, existenta, fisa, tipMaterial, linieSursa,
                fel, metoda, metodaFiscala, categorie));
        }

        // 43c: liniile existente absente din payload se ȘTERG.
        var sterse = existente.Values.Where(d => !pastrate.Contains(d.ID)).ToList();
        if (sterse.Count > 0)
            os.Delete(sterse);
        return rezolvate;
    }

    static void MaterializeazaLinii(IObjectSpace os, PunereInFunctiune doc,
            List<LinieRezolvata> rezolvate) {
        foreach (var r in rezolvate) {
            var l = r.Dto;
            var detaliu = r.Existenta;
            if (detaliu == null) {
                detaliu = os.CreateObject<PunereInFunctiuneDetaliu>();
                detaliu.Document = doc;
            }

            detaliu.Imobilizare = r.Fisa;
            detaliu.TipMaterial = r.TipMaterial;
            detaliu.Fel = r.Fel;
            detaliu.LinieSursa = r.LinieSursa;
            if (l.LinieSursaId == null)
                detaliu.LinieSursaId = null;
            detaliu.Cantitate = 1m;
            detaliu.Valoare = l.Valoare;
            detaliu.ValoareFiscala = l.ValoareFiscala ?? 0m;
            detaliu.AmortizareInitiala = l.AmortizareInitiala;
            detaliu.AmortizareFiscalaInitiala = l.AmortizareFiscalaInitiala;
            detaliu.LuniAmortizateInitial = l.LuniAmortizateInitial;
            detaliu.Metoda = r.Metoda;
            detaliu.DurataLuni = l.DurataLuni;
            detaliu.ValoareReziduala = l.ValoareReziduala;
            detaliu.MetodaFiscala = r.MetodaFiscala;
            detaliu.DurataFiscalaLuni = l.DurataFiscalaLuni;
            detaliu.CategorieFiscala = r.Categorie;
            detaliu.UtilizareExclusiva = l.UtilizareExclusiva;
        }
    }

    /// <summary>`null` dacă documentul nu există, nu e vizibil sau nu e o punere în funcțiune.</summary>
    public static PifReadDto Citeste(IObjectSpace os, Guid id) {
        var h = os.GetObjectsQuery<PunereInFunctiune>()
            .Where(d => d.ID == id)
            .Select(d => new {
                d.ID, d.Numar, d.Data, d.Stare, d.DataOperare,
                d.PredatorId, PredatorDenumire = d.Predator.Denumire,
                d.PrimitorId, PrimitorDenumire = d.Primitor.Denumire
            })
            .FirstOrDefault();
        if (h == null)
            return null;

        var linii = os.GetObjectsQuery<PunereInFunctiuneDetaliu>()
            .Where(l => l.DocumentId == id)
            .OrderBy(l => l.ID)
            .Select(l => new {
                l.ID, l.ImobilizareId,
                NumarInventar = l.Imobilizare.NumarInventar,
                ImobilizareDenumire = l.Imobilizare.Denumire,
                l.Fel, l.LinieSursaId,
                LinieSursaNumar = l.LinieSursa.Document.Numar,
                l.TipMaterialId, TipMaterialCod = l.TipMaterial.Cod,
                l.Valoare, l.ValoareFiscala,
                l.AmortizareInitiala, l.AmortizareFiscalaInitiala, l.LuniAmortizateInitial,
                l.Metoda, l.DurataLuni, l.ValoareReziduala,
                l.MetodaFiscala, l.DurataFiscalaLuni, l.CategorieFiscala, l.UtilizareExclusiva
            })
            .ToList();

        var faraImperecheri = !ApiProiectii.AreImperecheri(os, id);

        return new PifReadDto {
            Id = h.ID, Numar = h.Numar, Data = h.Data,
            Stare = h.Stare.ToString(), DataOperare = h.DataOperare,
            PredatorId = h.PredatorId, PredatorDenumire = h.PredatorDenumire,
            PrimitorId = h.PrimitorId, PrimitorDenumire = h.PrimitorDenumire,
            Total = linii.Sum(l => l.Valoare),
            PoateEdita = h.Stare == StareDocument.Draft,
            PoateOpera = h.Stare == StareDocument.Draft,
            PoateSterge = h.Stare == StareDocument.Draft,
            PoateAnula = h.Stare == StareDocument.Operat && faraImperecheri,
            PoateStorna = h.Stare == StareDocument.Operat && faraImperecheri,
            Linii = linii.Select(l => new PifLinieReadDto {
                Id = l.ID, ImobilizareId = l.ImobilizareId,
                NumarInventar = l.NumarInventar, ImobilizareDenumire = l.ImobilizareDenumire,
                Fel = l.Fel.ToString(),
                LinieSursaId = l.LinieSursaId, LinieSursaNumar = l.LinieSursaNumar,
                TipMaterialId = l.TipMaterialId, TipMaterialCod = l.TipMaterialCod,
                Valoare = l.Valoare, ValoareFiscala = l.ValoareFiscala,
                AmortizareInitiala = l.AmortizareInitiala,
                AmortizareFiscalaInitiala = l.AmortizareFiscalaInitiala,
                LuniAmortizateInitial = l.LuniAmortizateInitial,
                Metoda = l.Metoda?.ToString(), DurataLuni = l.DurataLuni,
                ValoareReziduala = l.ValoareReziduala,
                MetodaFiscala = l.MetodaFiscala?.ToString(), DurataFiscalaLuni = l.DurataFiscalaLuni,
                CategorieFiscala = l.CategorieFiscala?.ToString(),
                UtilizareExclusiva = l.UtilizareExclusiva
            }).ToList()
        };
    }

    public static IQueryable<PifListDto> Lista(IObjectSpace os) {
        var totaluri = os.GetObjectsQuery<DocumentDetaliu>()
            .GroupBy(l => l.DocumentId)
            .Select(g => new { DocumentId = g.Key, Total = g.Sum(x => x.Valoare), Numar = g.Count() });

        return from d in os.GetObjectsQuery<PunereInFunctiune>()
               join t in totaluri on d.ID equals t.DocumentId into agregat
               from t in agregat.DefaultIfEmpty()
               select new PifListDto {
                   Id = d.ID,
                   Numar = d.Numar,
                   Data = d.Data,
                   // 57a: enum → string în SQL, ca filtrarea și sortarea să rămână server-side.
                   Stare = d.Stare == StareDocument.Draft ? "Draft"
                       : d.Stare == StareDocument.Operat ? "Operat"
                       : "Stornat",
                   PredatorDenumire = d.Predator.Denumire,
                   PrimitorDenumire = d.Primitor.Denumire,
                   Total = (decimal?)t.Total ?? 0m,
                   NrLinii = (int?)t.Numar ?? 0
               };
    }

    public const int PlafonCandidati = 500;

    /// <summary>Liniile de factură operate, de clasă de imobilizări, cu restul lor neconsumat.</summary>
    /// <remarks>`plafon` e parametru doar pentru probă; `MaiSunt` = mai există linii de clasă F.</remarks>
    public static LiniiSursaDto LiniiSursa(IObjectSpace os, DateOnly dataStart, DateOnly dataEnd,
            Guid? partenerId, bool toate, int plafon = PlafonCandidati) {
        if (dataEnd < dataStart)
            throw new OperareException("Sfârșitul perioadei e înaintea începutului.");

        var query = from l in os.GetObjectsQuery<DocumentDetaliu>()
                    join f in os.GetObjectsQuery<FacturaIntrare>() on l.DocumentId equals f.ID
                    where f.Stare == StareDocument.Operat && f.Data >= dataStart && f.Data <= dataEnd
                        && l.TipMaterial.Clasa.Natura == NaturaClasa.Imobilizare
                        && (partenerId == null || f.PredatorId == partenerId)
                    orderby f.Data descending, f.Numar
                    select new {
                        l.ID, l.DocumentId, f.Numar, f.Data, f.PredatorId,
                        PartenerDenumire = f.Predator.Denumire,
                        TipMaterialCod = l.TipMaterial.Cod,
                        TipMaterialDenumire = l.TipMaterial.Denumire,
                        Descriere = (l as FacturaIntrareDetaliu).Produs.Denumire,
                        l.Cantitate, l.Valoare
                    };

        var randuri = query.Take(plafon + 1).ToList();
        var maiSunt = randuri.Count > plafon;
        if (maiSunt)
            randuri.RemoveAt(randuri.Count - 1);

        // Aceeași aritmetică a consumului ca gardianul de operare, pe lot (F26-D5).
        var consumat = PunereInFunctiune.ConsumatPeLiniiSursa(os,
            randuri.Select(r => r.ID).ToList(), null);

        var candidati = new List<LinieSursaCandidataDto>();
        foreach (var r in randuri) {
            var deja = consumat.GetValueOrDefault(r.ID);
            var rest = r.Valoare - deja;
            if (!toate && rest <= 0m)
                continue;
            candidati.Add(new LinieSursaCandidataDto {
                LinieId = r.ID, DocumentId = r.DocumentId,
                Numar = r.Numar, Data = r.Data,
                PartenerId = r.PredatorId, PartenerDenumire = r.PartenerDenumire,
                TipMaterialCod = r.TipMaterialCod, TipMaterialDenumire = r.TipMaterialDenumire,
                Descriere = r.Descriere,
                Cantitate = r.Cantitate, Valoare = r.Valoare,
                Consumat = deja, Rest = rest
            });
        }
        return new LiniiSursaDto { Candidati = candidati, MaiSunt = maiSunt };
    }

    static void VerificaScara(decimal valoare, int scara, string rol) {
        if (decimal.Round(valoare, scara) != valoare)
            throw new OperareException($"{rol} acceptă cel mult {scara} zecimale.");
        var limita = 1m;
        for (var i = 0; i < Scara.Precizie - scara; i++)
            limita *= 10m;
        if (Math.Abs(valoare) >= limita)
            throw new OperareException(
                $"{rol} depășește intervalul suportat ({Scara.Precizie - scara} cifre întregi).");
    }

    static string Eticheta(Document doc) =>
        string.IsNullOrWhiteSpace(doc.Numar) ? $"({doc.Data:dd.MM.yyyy})" : doc.Numar;

    static string Eticheta(Imobilizare fisa) =>
        string.IsNullOrWhiteSpace(fisa.NumarInventar) ? fisa.Denumire ?? "(fără număr)" : fisa.NumarInventar;
}
