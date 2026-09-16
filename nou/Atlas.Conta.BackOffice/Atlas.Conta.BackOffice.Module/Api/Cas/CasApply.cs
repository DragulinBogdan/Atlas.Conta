using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api.Cas;

// Ieșirea de imobilizări (F26-D6/D10): liniile le produce `AmortizareService.LiniiIesire`,
// ACELAȘI producător pe care îl cheamă gardianul de operare.
// CONTRACT DE APELANT: ObjectSpace-ul SECURED al apelantului; `Aplica`/`Sterge` COMIT.
public static class CasApply {

    public static Guid Aplica(IObjectSpace os, Guid? id, CasWriteDto dto) {
        if (dto == null)
            throw new OperareException("Lipsește corpul cererii.");

        // F3-D5: rezolvările ȘI calculul liniilor înaintea oricărui `CreateObject`.
        IesireImobilizare doc = null;
        if (id is Guid existentId) {
            doc = Rezolva.Cere<IesireImobilizare>(os, existentId, "Ieșirea de imobilizări");
            if (doc.Stare != StareDocument.Draft)
                throw new OperareException(
                    $"Documentul {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se mai modifică. "
                    + "Anulați operarea sau stornați-l.");
        }
        var cauza = ApiEnum.Membru<CauzaIesire>(dto.Cauza, "Cauza ieșirii");
        var predator = Rezolva.Cere<Repartitor>(os, dto.PredatorId, "Predatorul (locul fișelor)");
        var primitor = Rezolva.Cere<Repartitor>(os, dto.PrimitorId, "Primitorul (unitatea internă)");

        var cerute = new List<Guid>();
        foreach (var fisaId in dto.Fise ?? new List<Guid>()) {
            if (cerute.Contains(fisaId))
                throw new OperareException($"Fișa {fisaId} apare de două ori în cerere.");
            cerute.Add(fisaId);
        }

        var produse = new List<(Imobilizare Fisa, TipMaterial Tip, IReadOnlyList<LinieIesire> Linii)>();
        foreach (var fisaId in cerute) {
            var fisa = Rezolva.Cere<Imobilizare>(os, fisaId, "Fișa de imobilizare");
            var tip = Rezolva.Cere<TipMaterial>(os, fisa.TipMaterialId,
                $"Tipul (contul/clasa) fișei {Eticheta(fisa)}");
            var politica = os.FirstOrDefault<PoliticaAmortizare>(p => p.TipMaterialId == fisa.TipMaterialId);
            // Aceeași frază ca gardianul de operare (F26-D6).
            if (politica == null)
                throw new OperareException(
                    $"Tipul fișei {fisa.NumarInventar} n-are rând de politică de amortizare — "
                    + "conturile ieșirii vin exclusiv din ea.");
            produse.Add((fisa, tip, AmortizareService.LiniiIesire(os, fisaId, dto.Data, politica)));
        }

        doc ??= os.CreateObject<IesireImobilizare>();
        DocumentApply.AplicaDate(doc, dto.Data, dto.DataInregistrare);
        doc.Cauza = cauza;
        doc.Predator = predator;
        doc.Primitor = primitor;

        // `PUT` re-produce liniile: ele sunt situația fișelor la `Data`, nu culegere.
        var existente = doc.Detalii.ToList();
        if (existente.Count > 0)
            os.Delete(existente);

        foreach (var (fisa, tip, linii) in produse)
            foreach (var linie in linii) {
                var detaliu = os.CreateObject<IesireImobilizareDetaliu>();
                detaliu.Document = doc;
                detaliu.Imobilizare = fisa;
                detaliu.TipMaterial = tip;
                detaliu.Fel = linie.Fel;
                detaliu.Valoare = linie.Valoare;
                detaliu.Cantitate = 1m;
                detaliu.ContDebitId = linie.ContDebitId;
                detaliu.ContCreditId = linie.ContCreditId;
                // F26-D8: ambii repartitori = locul fișei.
                detaliu.RepartitorDebitId = fisa.LocId;
                detaliu.RepartitorCreditId = fisa.LocId;
                detaliu.CodEconomicId = fisa.CodEconomicId;
            }

        os.CommitChanges();
        return doc.ID;
    }

    public static void Sterge(IObjectSpace os, Guid id) {
        var doc = Rezolva.Cere<IesireImobilizare>(os, id, "Ieșirea de imobilizări");
        if (doc.Stare != StareDocument.Draft)
            throw new OperareException(
                $"Documentul {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se șterge. "
                + "Anulați operarea sau stornați-l.");

        os.Delete(doc.Detalii.ToList());
        os.Delete(doc);
        os.CommitChanges();
    }

    /// <summary>`null` dacă documentul nu există, nu e vizibil sau nu e o ieșire de imobilizări.</summary>
    public static CasReadDto Citeste(IObjectSpace os, Guid id) {
        var h = os.GetObjectsQuery<IesireImobilizare>()
            .Where(d => d.ID == id)
            .Select(d => new {
                d.ID, d.Numar, d.Data, d.DataInregistrare, d.Cauza, d.Stare, d.DataOperare,
                d.PredatorId, PredatorDenumire = d.Predator.Denumire,
                d.PrimitorId, PrimitorDenumire = d.Primitor.Denumire
            })
            .FirstOrDefault();
        if (h == null)
            return null;

        var linii = os.GetObjectsQuery<IesireImobilizareDetaliu>()
            .Where(l => l.DocumentId == id)
            .OrderBy(l => l.Imobilizare.NumarInventar).ThenBy(l => l.Fel)
            .Select(l => new {
                l.ID, l.ImobilizareId,
                NumarInventar = l.Imobilizare.NumarInventar,
                ImobilizareDenumire = l.Imobilizare.Denumire,
                l.Fel, l.Valoare,
                l.ContDebitId, ContDebitSimbol = l.ContDebit.Simbol,
                l.ContCreditId, ContCreditSimbol = l.ContCredit.Simbol
            })
            .ToList();

        var faraImperecheri = !ApiProiectii.AreImperecheri(os, id);

        return new CasReadDto {
            Id = h.ID, Numar = h.Numar, Data = h.Data,
            DataInregistrare = h.DataInregistrare,
            Cauza = h.Cauza.ToString(),
            Stare = h.Stare.ToString(), DataOperare = h.DataOperare,
            PredatorId = h.PredatorId, PredatorDenumire = h.PredatorDenumire,
            PrimitorId = h.PrimitorId, PrimitorDenumire = h.PrimitorDenumire,
            Total = linii.Sum(l => l.Valoare),
            Fise = linii.Select(l => l.ImobilizareId).Distinct().ToList(),
            PoateEdita = h.Stare == StareDocument.Draft,
            PoateOpera = h.Stare == StareDocument.Draft,
            PoateSterge = h.Stare == StareDocument.Draft,
            PoateAnula = h.Stare == StareDocument.Operat && faraImperecheri,
            PoateStorna = h.Stare == StareDocument.Operat && faraImperecheri,
            Linii = linii.Select(l => new CasLinieReadDto {
                Id = l.ID, ImobilizareId = l.ImobilizareId,
                NumarInventar = l.NumarInventar, ImobilizareDenumire = l.ImobilizareDenumire,
                Fel = l.Fel.ToString(), Valoare = l.Valoare,
                ContDebitId = l.ContDebitId, ContDebitSimbol = l.ContDebitSimbol,
                ContCreditId = l.ContCreditId, ContCreditSimbol = l.ContCreditSimbol
            }).ToList()
        };
    }

    public static IQueryable<CasListDto> Lista(IObjectSpace os) {
        var agregat = os.GetObjectsQuery<IesireImobilizareDetaliu>()
            .GroupBy(l => l.DocumentId)
            .Select(g => new {
                DocumentId = g.Key,
                Total = g.Sum(x => x.Valoare),
                NrFise = g.Select(x => x.ImobilizareId).Distinct().Count()
            });

        return from d in os.GetObjectsQuery<IesireImobilizare>()
               join t in agregat on d.ID equals t.DocumentId into grup
               from t in grup.DefaultIfEmpty()
               select new CasListDto {
                   Id = d.ID,
                   Numar = d.Numar,
                   Data = d.Data,
                   Cauza = d.Cauza == CauzaIesire.Casare ? "Casare"
                       : d.Cauza == CauzaIesire.Vanzare ? "Vanzare"
                       : "Lipsa",
                   // 57a: enum → string în SQL, ca filtrarea și sortarea să rămână server-side.
                   Stare = d.Stare == StareDocument.Draft ? "Draft"
                       : d.Stare == StareDocument.Operat ? "Operat"
                       : "Stornat",
                   PredatorDenumire = d.Predator.Denumire,
                   Total = (decimal?)t.Total ?? 0m,
                   NrFise = (int?)t.NrFise ?? 0
               };
    }

    static string Eticheta(Document doc) =>
        string.IsNullOrWhiteSpace(doc.Numar) ? $"({doc.Data:dd.MM.yyyy})" : doc.Numar;

    static string Eticheta(Imobilizare fisa) =>
        string.IsNullOrWhiteSpace(fisa.NumarInventar) ? fisa.Denumire ?? "(fără număr)" : fisa.NumarInventar;
}
