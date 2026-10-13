using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub;

public static partial class Materializare {
    /// <summary>Refuzurile retragerii și ale stornării la orice dată, pe corecțiile de valoare ale loturilor documentului; null = liber.</summary>
    public static (string Anulare, string Stornare) MotiveCorectiiValorice(IObjectSpace os, Document doc) {
        var proprii = os.GetObjectsQuery<Postare>().Where(p => p.DocumentId == doc.ID && p.FelUnitate == N.FelUnitate.Lot).ToList();
        if (FapteValorice(os, proprii) is not { } fapte)
            return (null, null);
        return (Motiv(() => N.CorectieValorica.LaRetragere(fapte, doc.ID)),
            Motiv(() => N.CorectieValorica.LaStornare(fapte, doc.ID, DateOnly.MaxValue)));

        static string Motiv(Action gard) {
            try { gard(); return null; }
            catch (N.RefuzException e) { return Contractare.Mesaj(e.Refuz); }
        }
    }

    static IReadOnlyList<N.CompensareLot> CompensariLaStornare(
            IObjectSpace os, Document doc, IEnumerable<Postare> aleDocumentului, DateOnly data) {
        if (FapteValorice(os, aleDocumentului) is not { } fapte)
            return [];
        try { return N.CorectieValorica.LaStornare(fapte, doc.ID, data); }
        catch (N.RefuzException e) { throw new OperareException(Contractare.Mesaj(e.Refuz)); }
    }

    static List<N.FaptPostat> FapteValorice(IObjectSpace os, IEnumerable<Postare> aleDocumentului) {
        var loturi = aleDocumentului.Where(p => p.FelUnitate == N.FelUnitate.Lot && p.Unitate != null)
            .Select(p => p.Unitate.Value).Distinct().ToList();
        return Citiri.CorectiiValorice.Exista(os, loturi) ? Citiri.CorectiiValorice.Fapte(os, loturi) : null;
    }

    // 119 (b, d): câte o `Operare` pe fiecare corecție cu alocare activă pe beneficiarul inversat.
    static void ScrieCompensari(IObjectSpace os, IReadOnlyList<N.CompensareLot> compensari, DateOnly data) {
        if (compensari.Count == 0)
            return;
        var ids = compensari.Select(c => c.Radacina.Postare.Cauza.Document).Distinct().ToList();
        var corectii = os.GetObjectsQuery<Document>().Where(d => ids.Contains(d.ID)).ToList();
        BlocheazaDocumente(os, [.. corectii]);
        var rotunjire = new N.Rotunjire(Scara.ConventieBani);
        foreach (var corectie in corectii.OrderBy(d => d.ID)) {
            if (corectie.Stare != StareDocument.Operat)
                throw new OperareException($"{N.Coduri.CvProvenienta}: Corecția {corectie.Numar} are alocări active fără să fie operată.");
            var ale = compensari.Where(c => c.Radacina.Postare.Cauza.Document == corectie.ID)
                .OrderBy(c => c.Radacina.ScrisLa).ThenBy(c => c.Radacina.Ordinal).ToList();
            var contract = N.Motor.Opereaza(new N.Declaratie(corectie.ID, data,
                [.. ale.Select(N.CorectieValorica.Miscarea)], [.. ale.Select(N.CorectieValorica.Decizia)],
                [new N.PerioadaDeschisa(data.Year, data.Month)]), rotunjire);
            if (!contract.EsteAcceptat)
                throw new OperareException(string.Join("\n", Mesaje(contract.Refuzuri)));
            Scrie(os, corectie.ID, contract.Tranzactii.Single(),
                Explicatie.Din(contract, corectie.Declarant().GetType().Name).Scrie());
        }
    }
}
