using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api.Amo;

// Amortizarea lunară peste `AmortizareService`, oglinda lui `InchidereTvaApply` (F26-D7/D10).
// CONTRACT DE APELANT: `Lista`/`Sterge` pe ușa SECURED, restul pe cea NON-SECURED, cu
// autorizarea luată înainte (55b, 58c, 73g).
public static class AmoApply {

    // `An`/`Luna` ies din `Data` în SQL: sunt coloanele grilei, modelul nu le are ca date.
    public static IQueryable<AmoListDto> Lista(IObjectSpace os) {
        var totaluri = os.GetObjectsQuery<AmortizareLunaraDetaliu>()
            .GroupBy(l => l.DocumentId)
            .Select(g => new {
                DocumentId = g.Key,
                Numar = g.Count(),
                Contabil = g.Sum(x => x.Valoare),
                Fiscal = g.Sum(x => x.ValoareFiscala),
                Deductibil = g.Sum(x => x.ValoareDeductibila)
            });

        return from d in os.GetObjectsQuery<AmortizareLunara>()
               join t in totaluri on d.ID equals t.DocumentId into agregat
               from t in agregat.DefaultIfEmpty()
               select new AmoListDto {
                   Id = d.ID,
                   Numar = d.Numar,
                   Data = d.Data,
                   An = d.Data.Year,
                   Luna = d.Data.Month,
                   // 57a: enum → string în SQL, ca filtrarea și sortarea să rămână server-side.
                   Stare = d.Stare == StareDocument.Draft ? "Draft"
                       : d.Stare == StareDocument.Operat ? "Operat"
                       : "Stornat",
                   DataOperare = d.DataOperare,
                   // Ambele laturi sunt unitatea care înregistrează luna; se arată una.
                   UnitateDenumire = d.Predator.Denumire,
                   NrLinii = (int?)t.Numar ?? 0,
                   TotalContabil = (decimal?)t.Contabil ?? 0m,
                   TotalFiscal = (decimal?)t.Fiscal ?? 0m,
                   TotalDeductibil = (decimal?)t.Deductibil ?? 0m
               };
    }

    /// <summary>`null` dacă documentul nu există, nu e vizibil sau nu e o amortizare lunară.</summary>
    public static AmoReadDto Citeste(IObjectSpace os, Guid id) {
        var h = os.GetObjectsQuery<AmortizareLunara>()
            .Where(d => d.ID == id)
            .Select(d => new {
                d.ID, d.Numar, d.Data, d.Stare, d.DataOperare,
                d.PredatorId, UnitateDenumire = d.Predator.Denumire
            })
            .FirstOrDefault();
        if (h == null)
            return null;

        var linii = os.GetObjectsQuery<AmortizareLunaraDetaliu>()
            .Where(l => l.DocumentId == id)
            .OrderBy(l => l.Imobilizare.NumarInventar).ThenBy(l => l.ID)
            .Select(l => new {
                l.ID, l.ImobilizareId,
                NumarInventar = l.Imobilizare.NumarInventar,
                Denumire = l.Imobilizare.Denumire,
                l.TipMaterialId,
                Contabil = l.Valoare, Fiscal = l.ValoareFiscala, Deductibil = l.ValoareDeductibila,
                ContCheltuialaId = l.ContDebitId, ContCheltuialaSimbol = l.ContDebit.Simbol,
                ContAmortizareId = l.ContCreditId, ContAmortizareSimbol = l.ContCredit.Simbol,
                LocId = l.RepartitorDebitId, LocDenumire = l.RepartitorDebit.Denumire,
                l.CentruCostId, l.CodEconomicId
            })
            .ToList();

        // `Stale` doar pe Draft, cu criteriul gardianului pe previzualizarea care se
        // exclude pe sine (`inlocuieste: id`).
        bool? stale = null;
        if (h.Stare == StareDocument.Draft) {
            var analiza = AmortizareService.Previzualizeaza(os, h.Data.Year, h.Data.Month, id);
            var doc = os.GetObjectByKey<AmortizareLunara>(id);
            stale = !(analiza.Motiv == null && doc != null && doc.LiniileCorespund(analiza));
        }

        var faraImperecheri = !ApiProiectii.AreImperecheri(os, id);

        return new AmoReadDto {
            Id = h.ID, Numar = h.Numar, Data = h.Data,
            An = h.Data.Year, Luna = h.Data.Month,
            Stare = h.Stare.ToString(), DataOperare = h.DataOperare,
            UnitateId = h.PredatorId, UnitateDenumire = h.UnitateDenumire,
            TotalContabil = linii.Sum(l => l.Contabil),
            TotalFiscal = linii.Sum(l => l.Fiscal),
            TotalDeductibil = linii.Sum(l => l.Deductibil),
            Stale = stale,
            PoateOpera = h.Stare == StareDocument.Draft,
            PoateSterge = h.Stare == StareDocument.Draft,
            PoateRegenera = h.Stare == StareDocument.Draft,
            PoateAnula = h.Stare == StareDocument.Operat && faraImperecheri,
            PoateStorna = h.Stare == StareDocument.Operat && faraImperecheri,
            Linii = linii.Select(l => new LinieAmoDto {
                Id = l.ID, ImobilizareId = l.ImobilizareId,
                NumarInventar = l.NumarInventar, Denumire = l.Denumire,
                TipMaterialId = l.TipMaterialId,
                Contabil = l.Contabil, Fiscal = l.Fiscal, Deductibil = l.Deductibil,
                ContCheltuialaId = l.ContCheltuialaId, ContCheltuialaSimbol = l.ContCheltuialaSimbol,
                ContAmortizareId = l.ContAmortizareId, ContAmortizareSimbol = l.ContAmortizareSimbol,
                LocId = l.LocId, LocDenumire = l.LocDenumire,
                CentruCostId = l.CentruCostId, CodEconomicId = l.CodEconomicId
            }).ToList()
        };
    }

    /// <summary>Dry-run-ul generării: nu scrie nimic.</summary>
    public static PrevizualizareAmoDto Previzualizeaza(IObjectSpace os, int an, int luna) {
        var r = AmortizareService.Previzualizeaza(os, an, luna);
        var dto = new PrevizualizareAmoDto {
            An = an, Luna = luna,
            Motiv = r.Motiv?.ToString(),
            MotivEticheta = r.Motiv == null ? null : AmortizareService.Eticheta(r.Motiv.Value),
            BlocantId = r.BlocantId,
            Detaliu = r.Detaliu,
            Linii = Proiecteaza(os, r.Linii),
            TotalContabil = r.Linii.Sum(l => l.Contabil),
            TotalFiscal = r.Linii.Sum(l => l.Fiscal),
            TotalDeductibil = r.Linii.Sum(l => l.Deductibil)
        };
        if (r.BlocantId is Guid blocantId) {
            var blocant = os.GetObjectsQuery<AmortizareLunara>()
                .Where(d => d.ID == blocantId)
                .Select(d => new { d.Numar, d.Stare, d.Data })
                .FirstOrDefault();
            // 53b: draftul n-are încă număr — eticheta cade pe dată.
            dto.BlocantNumar = string.IsNullOrWhiteSpace(blocant?.Numar)
                ? blocant?.Data.ToString("dd.MM.yyyy")
                : blocant.Numar;
            dto.BlocantStare = blocant?.Stare.ToString();
        }
        return dto;
    }

    // 79: comanda SCRIE ori de câte ori luna e liberă. Commit condiționat pe existența draftului.
    public static GenerareAmoRezultatDto Genereaza(IObjectSpace os, GenerareAmoRequestDto cerere) {
        if (cerere == null)
            throw new OperareException("Lipsește corpul cererii.");
        var r = AmortizareService.Incearca(os, cerere.An, cerere.Luna, cerere.UnitateId);
        if (r.Document != null)
            os.CommitChanges();
        return Rezultat(r);
    }

    // Întâi se calculează, apoi se șterge: un refuz aruncă înaintea oricărui `Delete`,
    // iar `inlocuieste: id` scoate draftul de față din gardianul de lună ocupată (79d).
    public static GenerareAmoRezultatDto Regenereaza(IObjectSpace os, Guid id) {
        var doc = Rezolva.Cere<AmortizareLunara>(os, id, "Amortizarea lunară");
        if (doc.Stare != StareDocument.Draft)
            throw new OperareException(
                $"Amortizarea {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se regenerează. "
                + "Anulați operarea sau stornați-o, apoi generați luna din nou.");

        var r = AmortizareService.Incearca(os, doc.Data.Year, doc.Data.Month, doc.PredatorId,
            inlocuieste: doc.ID);

        os.Delete(doc.Detalii.ToList());
        os.Delete(doc);
        os.CommitChanges();
        return Rezultat(r);
    }

    public static void Sterge(IObjectSpace os, Guid id) {
        var doc = Rezolva.Cere<AmortizareLunara>(os, id, "Amortizarea lunară");
        if (doc.Stare != StareDocument.Draft)
            throw new OperareException(
                $"Amortizarea {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se șterge. "
                + "Anulați operarea sau stornați-o.");

        os.Delete(doc.Detalii.ToList());
        os.Delete(doc);
        os.CommitChanges();
    }

    // 42c: două interogări pe toată mulțimea, nu una per linie.
    static List<LinieAmoDto> Proiecteaza(IObjectSpace os, IReadOnlyList<LinieAmortizare> linii) {
        if (linii.Count == 0)
            return [];
        var conturiCerute = linii.SelectMany(l => new[] { l.ContCheltuialaId, l.ContAmortizareId })
            .Where(c => c != null).Select(c => c.Value).Distinct().ToList();
        var conturi = conturiCerute.Count == 0
            ? new Dictionary<Guid, string>()
            : os.GetObjectsQuery<Cont>().Where(c => conturiCerute.Contains(c.ID))
                .Select(c => new { c.ID, c.Simbol }).ToList()
                .ToDictionary(c => c.ID, c => c.Simbol);
        var locuriCerute = linii.Select(l => l.LocId).Distinct().ToList();
        var locuri = os.GetObjectsQuery<Repartitor>().Where(r => locuriCerute.Contains(r.ID))
            .Select(r => new { r.ID, r.Denumire }).ToList()
            .ToDictionary(r => r.ID, r => r.Denumire);

        return linii.Select(l => new LinieAmoDto {
            ImobilizareId = l.ImobilizareId,
            NumarInventar = l.NumarInventar, Denumire = l.Denumire,
            TipMaterialId = l.TipMaterialId,
            Contabil = l.Contabil, Fiscal = l.Fiscal, Deductibil = l.Deductibil,
            ContCheltuialaId = l.ContCheltuialaId,
            ContCheltuialaSimbol = l.ContCheltuialaId == null
                ? null : conturi.GetValueOrDefault(l.ContCheltuialaId.Value),
            ContAmortizareId = l.ContAmortizareId,
            ContAmortizareSimbol = l.ContAmortizareId == null
                ? null : conturi.GetValueOrDefault(l.ContAmortizareId.Value),
            LocId = l.LocId, LocDenumire = locuri.GetValueOrDefault(l.LocId),
            CentruCostId = l.CentruCostId, CodEconomicId = l.CodEconomicId
        }).ToList();
    }

    static GenerareAmoRezultatDto Rezultat(RezultatAmortizare r) =>
        new() {
            DocumentId = r.Document?.ID,
            Motiv = r.Motiv?.ToString(),
            MotivEticheta = r.Motiv == null ? null : AmortizareService.Eticheta(r.Motiv.Value),
            BlocantId = r.BlocantId,
            Detaliu = r.Detaliu
        };

    static string Eticheta(Document doc) =>
        string.IsNullOrWhiteSpace(doc.Numar) ? $"({doc.Data:dd.MM.yyyy})" : doc.Numar;
}
