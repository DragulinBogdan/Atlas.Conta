using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api.Dvi;

// Nucleul feliei DVI: reconcilierea agregatului (antet + linii + legături) și
// proiecțiile plate. ZERO ASP.NET — controllerul din host e transport, iar
// ModelCheck exersează același cod.
//
// CONTRACT DE APELANT: `Aplica`/`Sterge` rulează în ObjectSpace-ul SECURED al
// apelantului și COMIT — legăturile trebuie să treacă prin `GardianEditare`
// (DVI-D3), care e armat doar pe ușile securizate.
public static class DviApply {

    public static Guid Aplica(IObjectSpace os, Guid? id, DviWriteDto dto) {
        if (dto == null)
            throw new OperareException("Lipsește corpul cererii.");

        // Rezolvările ÎNAINTE de orice `CreateObject` (F3-D5): un refuz de după
        // ar lăsa un obiect orfan în ObjectSpace-ul VIU al apelantului, pe care
        // un commit ulterior l-ar persista.
        var predator = Rezolva.Cere<Repartitor>(os, dto.PredatorId, "Predatorul (biroul vamal)");
        var primitor = Rezolva.Cere<Repartitor>(os, dto.PrimitorId, "Primitorul (unitatea internă)");

        BusinessObjects.Dvi doc;
        if (id is Guid existentId) {
            doc = Rezolva.Cere<BusinessObjects.Dvi>(os, existentId, "Declarația vamală");
            if (doc.Stare != StareDocument.Draft)
                throw new OperareException(
                    $"Documentul {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se mai modifică. "
                    + "Anulați operarea sau stornați-l.");
        }
        else {
            doc = os.CreateObject<BusinessObjects.Dvi>();
        }

        doc.Numar = dto.Numar;
        doc.Data = dto.Data;
        doc.Predator = predator;
        doc.Primitor = primitor;

        ReconciliazaLinii(os, doc, dto.Linii ?? new List<DviLinieWriteDto>());
        ReconciliazaFacturi(os, doc, dto.FacturiIds ?? new List<Guid>());

        os.CommitChanges();
        return doc.ID;
    }

    public static void Sterge(IObjectSpace os, Guid id) {
        var doc = Rezolva.Cere<BusinessObjects.Dvi>(os, id, "Declarația vamală");
        if (doc.Stare != StareDocument.Draft)
            throw new OperareException(
                $"Documentul {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se șterge. "
                + "Anulați operarea sau stornați-l.");

        os.Delete(Legaturi(os, doc.ID));
        os.Delete(doc.Detalii.ToList());
        os.Delete(doc);
        os.CommitChanges();
    }

    // Liniile declarației stau pe `DocumentDetaliu` de BAZĂ (DVI-D1).
    static void ReconciliazaLinii(IObjectSpace os, BusinessObjects.Dvi doc, List<DviLinieWriteDto> linii) {
        var existente = doc.Detalii.ToDictionary(d => d.ID);
        var pastrate = new HashSet<Guid>();

        foreach (var l in linii) {
            // Tot ce poate REFUZA se rezolvă înaintea lui `CreateObject` (F3-D5).
            var tipMaterial = Rezolva.Cere<TipMaterial>(os, l.TipMaterialId, "Tipul (contul/clasa)");
            var tipTva = Rezolva.Optional<TipTva>(os, l.TipTvaId, "Tipul de TVA");
            VerificaScara(l.Valoare, Scara.Bani, "Valoarea în vamă");
            VerificaScara(l.ValoareTva, Scara.Bani, "Valoarea TVA");
            if (l.ValoareTva < 0)
                throw new OperareException("Valoarea TVA nu poate fi negativă.");

            DocumentDetaliu detaliu;
            if (l.Id is Guid linieId) {
                if (!existente.TryGetValue(linieId, out var existenta))
                    throw new OperareException(
                        $"Linia {linieId} nu aparține documentului {Eticheta(doc)}.");
                if (!pastrate.Add(linieId))
                    throw new OperareException($"Linia {linieId} apare de două ori în cerere.");
                detaliu = existenta;
            }
            else {
                detaliu = os.CreateObject<DocumentDetaliu>();
                detaliu.Document = doc;
            }

            detaliu.TipMaterial = tipMaterial;
            detaliu.TipTva = tipTva;
            if (l.TipTvaId == null)
                detaliu.TipTvaId = null;
            detaliu.Valoare = l.Valoare;
            detaliu.ValoareTva = l.ValoareTva;
        }

        var sterse = existente.Values.Where(d => !pastrate.Contains(d.ID)).ToList();
        if (sterse.Count > 0)
            os.Delete(sterse);
    }

    // DVI-D3: legăturile se scriu DOAR prin `FacturiIds`. Regulile (declarație
    // Draft, factură Operat, pereche unică, fără editare) sunt ale entității și
    // le ridică `GardianEditare` la commit — aici se scrie doar diferența.
    static void ReconciliazaFacturi(IObjectSpace os, BusinessObjects.Dvi doc, List<Guid> facturiIds) {
        var cerute = new List<Guid>();
        foreach (var facturaId in facturiIds) {
            if (cerute.Contains(facturaId))
                throw new OperareException($"Factura {facturaId} apare de două ori în cerere.");
            cerute.Add(facturaId);
        }

        var existente = Legaturi(os, doc.ID);
        // Aceeași disciplină ca la linii (F3-D5): facturile se rezolvă toate
        // înainte, ca un refuz să nu lase o legătură fără factură în ObjectSpace.
        var deLegat = cerute
            .Where(i => !existente.Any(l => l.FacturaId == i))
            .Select(i => Rezolva.Cere<FacturaIntrare>(os, i, "Factura de import"))
            .ToList();

        var deSters = existente.Where(l => !cerute.Contains(l.FacturaId)).ToList();
        if (deSters.Count > 0)
            os.Delete(deSters);

        foreach (var factura in deLegat) {
            var legatura = os.CreateObject<DviFactura>();
            legatura.Dvi = doc;
            legatura.Factura = factura;
        }
    }

    // Prin INTEROGARE, nu prin navigația `Facturi`: după o ștergere amânată
    // colecția încărcată încă poartă legătura stinsă, iar reconcilierea ar
    // considera-o existentă.
    static List<DviFactura> Legaturi(IObjectSpace os, Guid dviId) =>
        os.GetObjectsQuery<DviFactura>().Where(f => f.DviId == dviId).ToList();

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

    // ═══════════════════════ Citire ═══════════════════════

    // `null` dacă documentul nu există, nu e vizibil sau nu e o declarație vamală.
    public static DviReadDto Citeste(IObjectSpace os, Guid id) {
        var h = os.GetObjectsQuery<BusinessObjects.Dvi>()
            .Where(d => d.ID == id)
            .Select(d => new {
                d.ID, d.Numar, d.Data, d.Stare, d.DataOperare,
                d.PredatorId, PredatorDenumire = d.Predator.Denumire,
                d.PrimitorId, PrimitorDenumire = d.Primitor.Denumire
            })
            .FirstOrDefault();
        if (h == null)
            return null;

        var linii = os.GetObjectsQuery<DocumentDetaliu>()
            .Where(l => l.DocumentId == id)
            .OrderBy(l => l.ID)
            .Select(l => new {
                l.ID, l.TipMaterialId,
                TipMaterialCod = l.TipMaterial.Cod,
                TipMaterialDenumire = l.TipMaterial.Denumire,
                l.TipTvaId, TipTvaCod = l.TipTva.Cod, TipTvaDenumire = l.TipTva.Denumire,
                TipTvaCota = (decimal?)l.TipTva.Cota,
                l.Valoare, l.ValoareTva
            })
            .ToList();

        var legaturi = os.GetObjectsQuery<DviFactura>()
            .Where(f => f.DviId == id)
            .OrderBy(f => f.Factura.Data).ThenBy(f => f.Factura.Numar)
            .Select(f => new {
                f.FacturaId,
                f.Factura.Numar, f.Factura.Data, f.Factura.Stare,
                PartenerDenumire = f.Factura.Predator.Denumire
            })
            .ToList();
        var totaluriFacturi = Totaluri(os, legaturi.Select(f => f.FacturaId).ToList());

        var faraImperecheri = !ApiProiectii.AreImperecheri(os, id);

        return new DviReadDto {
            Id = h.ID, Numar = h.Numar, Data = h.Data,
            Stare = h.Stare.ToString(), DataOperare = h.DataOperare,
            PredatorId = h.PredatorId, PredatorDenumire = h.PredatorDenumire,
            PrimitorId = h.PrimitorId, PrimitorDenumire = h.PrimitorDenumire,
            Baza = linii.Sum(l => l.Valoare),
            Tva = linii.Sum(l => l.ValoareTva),
            PoateEdita = h.Stare == StareDocument.Draft,
            PoateOpera = h.Stare == StareDocument.Draft,
            PoateAnula = h.Stare == StareDocument.Operat && faraImperecheri,
            PoateStorna = h.Stare == StareDocument.Operat && faraImperecheri,
            Linii = linii.Select(l => new DviLinieReadDto {
                Id = l.ID,
                TipMaterialId = l.TipMaterialId,
                TipMaterialCod = l.TipMaterialCod,
                TipMaterialDenumire = l.TipMaterialDenumire,
                TipTvaId = l.TipTvaId, TipTvaCod = l.TipTvaCod,
                TipTvaDenumire = l.TipTvaDenumire, TipTvaCota = l.TipTvaCota,
                Valoare = l.Valoare, ValoareTva = l.ValoareTva
            }).ToList(),
            Facturi = legaturi.Select(f => new DviFacturaDto {
                FacturaId = f.FacturaId,
                Numar = f.Numar, Data = f.Data,
                PartenerDenumire = f.PartenerDenumire,
                Stare = f.Stare.ToString(),
                Valoare = totaluriFacturi.GetValueOrDefault(f.FacturaId)
            }).ToList()
        };
    }

    // `IQueryable` — DataSourceLoader îi pune deasupra filtrarea/sortarea/
    // paginarea clientului (43c). Cifrele vin prin JOIN pe agregat, nu prin
    // subquery corelat (42c).
    public static IQueryable<DviListDto> Lista(IObjectSpace os) {
        var totaluri = os.GetObjectsQuery<DocumentDetaliu>()
            .GroupBy(l => l.DocumentId)
            .Select(g => new {
                DocumentId = g.Key,
                Baza = g.Sum(x => x.Valoare),
                Tva = g.Sum(x => x.ValoareTva)
            });
        var legaturi = os.GetObjectsQuery<DviFactura>()
            .GroupBy(f => f.DviId)
            .Select(g => new { DviId = g.Key, Numar = g.Count() });

        return from d in os.GetObjectsQuery<BusinessObjects.Dvi>()
               join t in totaluri on d.ID equals t.DocumentId into agregat
               from t in agregat.DefaultIfEmpty()
               join f in legaturi on d.ID equals f.DviId into legate
               from f in legate.DefaultIfEmpty()
               select new DviListDto {
                   Id = d.ID,
                   Numar = d.Numar,
                   Data = d.Data,
                   Stare = d.Stare == StareDocument.Draft ? "Draft"
                       : d.Stare == StareDocument.Operat ? "Operat"
                       : "Stornat",
                   PredatorDenumire = d.Predator.Denumire,
                   Baza = (decimal?)t.Baza ?? 0m,
                   Tva = (decimal?)t.Tva ?? 0m,
                   NrFacturi = (int?)f.Numar ?? 0
               };
    }

    // Plafonul de pagină al candidaților, ca la orice listă
    // (`ContaApiController.Incarca`).
    public const int PlafonCandidati = 500;

    // Candidații de legat (DVI-D5): facturi de intrare OPERATE din perioadă,
    // implicit doar cele ale furnizorilor extra-UE. Clasa fiscală se calculează
    // în memorie — `ClasaFiscala.APartenerului` e funcția LEGII (F23-D2), nu o
    // coloană, deci filtrul ei nu se traduce în SQL, iar plafonul cade pe
    // interogare, ÎNAINTEA lui: `MaiSunt` spune exact asta.
    //
    // CONTRACT DE APELANT: `plafon` nu vine de pe sârmă — e parametru doar ca
    // proba să poată forța trunchierea pe o scenă mică.
    public static FacturiCandidateDto FacturiCandidate(IObjectSpace os,
            DateOnly dataStart, DateOnly dataEnd, Guid? partenerId, bool toate, Guid? dviId,
            int plafon = PlafonCandidati) {
        if (dataEnd < dataStart)
            throw new OperareException("Sfârșitul perioadei e înaintea începutului.");

        var legate = dviId is Guid dvi
            ? os.GetObjectsQuery<DviFactura>().Where(f => f.DviId == dvi)
                .Select(f => f.FacturaId).ToList()
            : new List<Guid>();

        var query = os.GetObjectsQuery<FacturaIntrare>()
            .Where(f => f.Stare == StareDocument.Operat
                && f.Data >= dataStart && f.Data <= dataEnd
                && !legate.Contains(f.ID));
        if (partenerId is Guid pid)
            query = query.Where(f => f.PredatorId == pid);

        var randuri = query
            .OrderByDescending(f => f.Data).ThenBy(f => f.Numar)
            .Select(f => new {
                f.ID, f.Numar, f.Data, f.PredatorId,
                PartenerDenumire = f.Predator.Denumire,
                TipPersoana = (TipPersoana?)(f.Predator as Partener).TipPersoana,
                Tara = (f.Predator as Partener).Tara,
                InregistratTva = (bool?)(f.Predator as Partener).InregistratTva
            })
            .Take(plafon + 1)
            .ToList();
        var maiSunt = randuri.Count > plafon;
        if (maiSunt)
            randuri.RemoveAt(randuri.Count - 1);

        var candidati = new List<FacturaCandidataDto>();
        foreach (var r in randuri) {
            // Predatorul care nu e `Partener` n-are clasă fiscală: rămâne null,
            // deci intră doar la `toate`.
            var clasa = r.TipPersoana == null
                ? (ClasaFiscalaPartener?)null
                : ClasaFiscala.APartenerului(r.TipPersoana.Value, r.Tara, r.InregistratTva ?? false);
            if (!toate && clasa != ClasaFiscalaPartener.ExtraUe)
                continue;
            candidati.Add(new FacturaCandidataDto {
                FacturaId = r.ID, Numar = r.Numar, Data = r.Data,
                PartenerId = r.PredatorId, PartenerDenumire = r.PartenerDenumire,
                ClasaFiscala = clasa?.ToString()
            });
        }

        var ids = candidati.Select(c => c.FacturaId).ToList();
        var totaluri = Totaluri(os, ids);
        var sugestii = TipuriDominante(os, ids);
        foreach (var c in candidati) {
            c.Valoare = totaluri.GetValueOrDefault(c.FacturaId);
            c.TipMaterialSugeratId = sugestii.GetValueOrDefault(c.FacturaId);
        }
        return new FacturiCandidateDto { Candidati = candidati, MaiSunt = maiSunt };
    }

    // `Document.Total` (BRUT) pentru o mulțime de documente, printr-un singur
    // agregat.
    static Dictionary<Guid, decimal> Totaluri(IObjectSpace os, IReadOnlyCollection<Guid> ids) {
        if (ids.Count == 0)
            return new Dictionary<Guid, decimal>();
        var cerute = ids.ToList();
        return os.GetObjectsQuery<DocumentDetaliu>()
            .Where(l => cerute.Contains(l.DocumentId))
            .GroupBy(l => l.DocumentId)
            .Select(g => new { DocumentId = g.Key, Total = g.Sum(x => x.Valoare + x.ValoareTva) })
            .ToList()
            .ToDictionary(t => t.DocumentId, t => t.Total);
    }

    // Tipul de material cu cea mai mare Σ `Valoare` pe liniile fiecărui document.
    static Dictionary<Guid, Guid> TipuriDominante(IObjectSpace os, IReadOnlyCollection<Guid> ids) {
        if (ids.Count == 0)
            return new Dictionary<Guid, Guid>();
        var cerute = ids.ToList();
        return os.GetObjectsQuery<DocumentDetaliu>()
            .Where(l => cerute.Contains(l.DocumentId))
            .GroupBy(l => new { l.DocumentId, l.TipMaterialId })
            .Select(g => new { g.Key.DocumentId, g.Key.TipMaterialId, Total = g.Sum(x => x.Valoare) })
            .ToList()
            .GroupBy(x => x.DocumentId)
            .ToDictionary(g => g.Key,
                g => g.OrderByDescending(x => x.Total).ThenBy(x => x.TipMaterialId).First().TipMaterialId);
    }
}
