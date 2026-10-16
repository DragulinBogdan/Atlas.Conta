using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api.Irz;

/// <summary>
/// Comenzile închiderii conturilor de rezultat peste <see cref="InchidereRezultatService"/>. Previzualizarea,
/// generarea și regenerarea rulează pe ușa NON-SECURED (58c); ștergerea, pe cea securizată.
/// </summary>
public static class InchidereRezultatApply {
    public static IQueryable<IrzListDto> Lista(IObjectSpace os) {
        var totaluri = os.GetObjectsQuery<DocumentDetaliu>()
            .GroupBy(l => l.DocumentId)
            .Select(g => new { DocumentId = g.Key, Numar = g.Count(), Total = g.Sum(x => x.Valoare) });
        return from d in os.GetObjectsQuery<InchidereRezultat>()
               join t in totaluri on d.ID equals t.DocumentId into agregat
               from t in agregat.DefaultIfEmpty()
               select new IrzListDto {
                   Id = d.ID, Numar = d.Numar, Data = d.Data, An = d.Data.Year, Luna = d.Data.Month,
                   Stare = d.Stare == StareDocument.Draft ? "Draft"
                       : d.Stare == StareDocument.Operat ? "Operat"
                       : "Stornat",
                   DataOperare = d.DataOperare, UnitateDenumire = d.Predator.Denumire,
                   NumarLinii = (int?)t.Numar ?? 0, Total = (decimal?)t.Total ?? 0m,
               };
    }

    /// <summary>Null când documentul nu există, nu e vizibil sau nu e o închidere de rezultat.</summary>
    public static IrzReadDto Citeste(IObjectSpace os, Guid id) {
        var doc = RandDupaCheie.Ca<InchidereRezultat>(os, id);
        if (doc == null)
            return null;
        var linii = os.GetObjectsQuery<InchidereRezultatDetaliu>()
            .Where(l => l.DocumentId == id)
            .OrderBy(l => l.ID)
            .Select(l => new IrzLinieReadDto {
                Id = l.ID, ContDebitSimbol = l.ContDebit.Simbol, ContCreditSimbol = l.ContCredit.Simbol,
                SursaPeDebit = l.SursaPeDebit, Valoare = l.Valoare,
            })
            .ToList();
        var regim = RegimDocument.Calculeaza(os, doc);
        return new IrzReadDto {
            Id = doc.ID, Numar = doc.Numar, Data = doc.Data, An = doc.Data.Year, Luna = doc.Data.Month,
            Stare = doc.Stare.ToString(), DataOperare = doc.DataOperare,
            UnitateId = doc.PredatorId, UnitateDenumire = doc.Predator?.Denumire,
            Total = linii.Sum(l => l.Valoare),
            Stale = doc.Stare == StareDocument.Draft ? InchidereRezultatService.Stale(os, doc) : null,
            PoateOpera = regim.Poate(ComandaDocument.Opereaza),
            PoateAnula = regim.Poate(ComandaDocument.AnuleazaOperarea),
            PoateStorna = regim.Poate(ComandaDocument.Storneaza),
            PoateSterge = regim.Poate(RegimDocument.Sterge),
            PoateRegenera = regim.Poate(RegimDocument.Regenereaza),
            Linii = linii,
        };
    }

    public static PrevizualizareIrzDto Previzualizeaza(IObjectSpace os, int an, int luna) {
        var r = InchidereRezultatService.Previzualizeaza(os, an, luna);
        var ids = r.Linii.SelectMany(l => new[] { l.Sursa.Cont, l.ContDestinatie }).Distinct().ToList();
        var simboluri = os.GetObjectsQuery<Cont>().Where(c => ids.Contains(c.ID)).ToDictionary(c => c.ID, c => c.Simbol);
        return new PrevizualizareIrzDto {
            An = an, Luna = luna, Motiv = r.Motiv?.ToString(), InchidereVieId = r.InchidereVieId,
            Linii = r.Linii.Select(l => new IrzLinieDto {
                ContSursaSimbol = simboluri[l.Sursa.Cont], ContDestinatieSimbol = simboluri[l.ContDestinatie],
                SursaPeDebit = l.SursaPeDebit, Valoare = l.Valoare,
            }).ToList(),
        };
    }

    /// <summary>Commit condiționat de existența draftului.</summary>
    public static GenerareIrzRezultatDto Genereaza(IObjectSpace os, GenerareIrzRequestDto cerere) {
        if (cerere == null)
            throw new OperareException("Lipsește corpul cererii.");
        using var tx = TranzactieComanda.Incepe(os);
        var r = InchidereRezultatService.Incearca(os, cerere.An, cerere.Luna, cerere.UnitateId);
        DocumentApply.Generat(r.Document);
        if (r.Document != null)
            os.CommitChanges();
        tx.Commit();
        return Rezultat(r);
    }

    /// <summary>Înlocuiește draftul în aceeași tranzacție: întâi se calculează, apoi se șterge.</summary>
    public static GenerareIrzRezultatDto Regenereaza(IObjectSpace os, Guid id) {
        using var tx = TranzactieComanda.Incepe(os);
        var doc = Rezolva.Cere<InchidereRezultat>(os, id, "Închiderea conturilor de rezultat");
        if (doc.Stare != StareDocument.Draft)
            throw new OperareException(
                $"Închiderea {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se regenerează. "
                + "Anulați operarea sau stornați-o, apoi generați luna din nou.");
        var r = InchidereRezultatService.Incearca(os, doc.Data.Year, doc.Data.Month, doc.PredatorId, inlocuieste: doc.ID);
        DocumentApply.Generat(r.Document);
        os.Delete(doc.Detalii.ToList());
        os.Delete(doc);
        os.CommitChanges();
        tx.Commit();
        return Rezultat(r);
    }

    public static void Sterge(IObjectSpace os, Guid id) {
        var doc = Rezolva.Cere<InchidereRezultat>(os, id, "Închiderea conturilor de rezultat");
        if (doc.Stare != StareDocument.Draft)
            throw new OperareException(
                $"Închiderea {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se șterge. "
                + "Anulați operarea sau stornați-o.");
        os.Delete(doc.Detalii.ToList());
        os.Delete(doc);
        os.CommitChanges();
    }

    /// <summary>Draftul nu mai închide exact soldurile de acum; null pe Operat/Stornat.</summary>
    public static bool? Stale(IObjectSpace os, Guid id) {
        var doc = Rezolva.Cere<InchidereRezultat>(os, id, "Închiderea conturilor de rezultat");
        return doc.Stare == StareDocument.Draft ? InchidereRezultatService.Stale(os, doc) : null;
    }

    static GenerareIrzRezultatDto Rezultat(InchidereRezultatService.RezultatInchidere r) => new() {
        DocumentId = r.Document?.ID, Motiv = r.Motiv?.ToString(), InchidereVieId = r.InchidereVieId,
        NumarLinii = r.Linii.Count,
    };

    static string Eticheta(Document doc) =>
        string.IsNullOrWhiteSpace(doc.Numar) ? $"({doc.Data:dd.MM.yyyy})" : doc.Numar;
}
