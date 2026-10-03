using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>INV-CUB pe faptele fiecărei scene, înaintea purjei; fiecare ramură fără probă proprie își ucide mutantul o dată per profil (102d).</summary>
static class AcoperireInvarianti {
    sealed record Mutant(string Ramura, string Fragment, Func<IObjectSpace, DbContext, bool> Aplica);

    static readonly Mutant[] mutanti = [
        new("DESCHIDERE-EGALA", null, (os, db) => RegistruIstoric(os, db) != null),
        new("DESCHIDERE", "CITIRE_DESCHIDERE_INCOMPLETA", RegistruIstoricDiferit),
        new("PARTIDE", "CITIRE_PARTIDE_INCOMPLETE", PartidaFaraPartener),
        new("POLITICA", "CITIRE_PARTIDE_POLITICA", TotalDecontareDiferit),
        new("IMO-FISA", "fără fișă pe cub", FisaPeAltaUnitate),
        new("IMO-CAUZA", "fără cauză", FisaFaraLinie),
        new("IMO-ORIGINE", "fără cauză", FisaFaraOrigine),
        new("IMO-SUPORT", "fără cauză", FisaFaraSuport),
        new("IMO-REGISTRU", "diferă de cub", RegistruImobilizariDiferit),
        new("EXPLICATIE-LIPSA", C.Citiri.Explicatii.Lipsa, ExplicatieStearsa),
        new("EXPLICATIE-REFERINTA", C.Citiri.Explicatii.Referinta, ExplicatieReferitaGresit),
        new("EXPLICATIE-STORNO", C.Citiri.Explicatii.Storno, InversaPeLotDiferita),
        new("EXPLICATIE-IESIRE", C.Citiri.Explicatii.Iesire, (os, db) => Rescrie(os, db, ValoareIesireDiferita)),
        new("EXPLICATIE-DECLARATA", C.Citiri.Explicatii.Iesire, (os, db) => Rescrie(os, db, Fara<N.ValoareDeclarata>)),
        new("EXPLICATIE-EVALUARE", C.Citiri.Explicatii.Evaluare, (os, db) => Rescrie(os, db, SoldCititDiferit)),
        new("EXPLICATIE-STINGERE", C.Citiri.Explicatii.Stingere, (os, db) => Rescrie(os, db, Fara<N.AlocareFifo>)),
    ];

    static readonly HashSet<string> ucise = [];
    public static int Scene { get; private set; }
    public static IEnumerable<string> Neucise => mutanti.Select(m => m.Ramura).Where(r => !ucise.Contains(r));

    public static void Reseteaza() { ucise.Clear(); Scene = 0; }

    public static void Verifica(IObjectSpace os, Action<string, string, bool> verifica) {
        Scene++;
        var refuz = Refuz(os);
        verifica("INV-CUB", "invarianții pe faptele scenei, înaintea purjei" + (refuz == null ? "" : " — " + refuz.Split('\n')[0]), refuz == null);
        if (refuz != null) return;
        var db = ((EFCoreObjectSpace)os).DbContext;
        foreach (var m in mutanti.Where(m => !ucise.Contains(m.Ramura))) {
            using var tx = db.Database.BeginTransaction();
            bool aplicat;
            try { aplicat = m.Aplica(os, db); }
            catch (Exception e) {
                verifica("INV-CUB-" + m.Ramura, "mutantul nu se aplică — " + e.Message.Split('\n')[0], false);
                aplicat = false;
            }
            if (aplicat) {
                refuz = Refuz(os);
                var ok = m.Fragment == null ? refuz == null : refuz?.Contains(m.Fragment, StringComparison.OrdinalIgnoreCase) == true;
                verifica("INV-CUB-" + m.Ramura, (m.Fragment == null ? "martorul trece" : $"mutantul refuză cu {m.Fragment}")
                    + (ok ? "" : $" — {refuz ?? "trecut"}"), ok);
                if (ok) ucise.Add(m.Ramura);
            }
            tx.Rollback();
            db.ChangeTracker.Clear();
        }
    }

    static string Refuz(IObjectSpace os) {
        try { C.Citiri.Invarianti.Verifica(os); return null; }
        catch (OperareException e) { return e.Message; }
    }

    static Guid? RegistruIstoric(IObjectSpace os, DbContext db) {
        var postari = C.Citiri.Contabil.Postari(os).Where(p => p.Tranzactie.Fel == N.FelTranzactie.Deschidere && p.Valoare != 0m)
            .Select(p => new { p.Cont, p.Latura, p.Valoare, p.Data }).ToList();
        if (postari.Count == 0) return null;
        var debite = postari.Where(p => p.Latura == N.Latura.Debit).Select(p => (p.Cont, Sold: p.Valoare)).ToArray();
        var credite = postari.Where(p => p.Latura == N.Latura.Credit).Select(p => (p.Cont, Sold: p.Valoare)).ToArray();
        Guid? primul = null;
        for (int i = 0, j = 0; i < debite.Length && j < credite.Length;) {
            var valoare = Math.Min(debite[i].Sold, credite[j].Sold);
            var id = Guid.NewGuid(); primul ??= id;
            db.Database.ExecuteSqlInterpolated($@"INSERT INTO ""RegistruContabil""
                (""ID"", ""Data"", ""ContDebitId"", ""ContCreditId"", ""Valoare"", ""Storno"")
                VALUES ({id}, {postari[0].Data}, {debite[i].Cont}, {credite[j].Cont}, {valoare}, false)");
            if ((debite[i].Sold -= valoare) == 0m) i++;
            if ((credite[j].Sold -= valoare) == 0m) j++;
        }
        return primul;
    }

    static bool RegistruIstoricDiferit(IObjectSpace os, DbContext db) {
        if (RegistruIstoric(os, db) is not Guid rand) return false;
        db.Database.ExecuteSqlInterpolated($@"UPDATE ""RegistruContabil"" SET ""Valoare"" = ""Valoare"" + 1 WHERE ""ID"" = {rand}");
        return true;
    }

    static bool PartidaFaraPartener(IObjectSpace os, DbContext db) {
        var tinta = os.GetObjectsQuery<C.Postare>().Where(C.Citiri.Transformare.FaraContrapondere)
            .Where(p => p.Carte == N.Carte.Contabil && p.FelUnitate == N.FelUnitate.Partida && p.Partener != null)
            .Select(p => new { p.ID, p.Spatiu }).FirstOrDefault();
        if (tinta == null) return false;
        db.Set<C.Postare>().Where(p => p.ID == tinta.ID && p.Spatiu == tinta.Spatiu)
            .ExecuteUpdate(s => s.SetProperty(p => p.Partener, (Guid?)null));
        return true;
    }

    static bool TotalDecontareDiferit(IObjectSpace os, DbContext db) {
        var tinta = os.GetObjectsQuery<Document>()
            .Where(d => (d is FacturaIntrare || d is FacturaIesire || d is Decont || d is ReturClient)
                && (d.Stare == StareDocument.Operat || d.Stare == StareDocument.Stornat))
            .Select(d => new { d.ID, d.TotalStingere }).FirstOrDefault();
        if (tinta == null) return false;
        var total = tinta.TotalStingere ?? 0m;
        var nou = total >= 0 ? total + 1 : total - 1;
        db.Set<Document>().Where(d => d.ID == tinta.ID).ExecuteUpdate(s => s.SetProperty(d => d.TotalStingere, nou));
        return true;
    }

    static bool FisaPeAltaUnitate(IObjectSpace os, DbContext db) {
        var tinta = os.GetObjectsQuery<RegistruImobilizari>().Select(r => new { r.DocumentId, r.ImobilizareId }).FirstOrDefault();
        if (tinta == null) return false;
        Guid? alta = Guid.NewGuid();
        return db.Set<C.Postare>().Where(p => p.FelUnitate == N.FelUnitate.Fisa
                && p.Unitate == tinta.ImobilizareId && p.DocumentId == tinta.DocumentId)
            .ExecuteUpdate(s => s.SetProperty(p => p.Unitate, alta)) > 0;
    }

    static bool FisaFaraLinie(IObjectSpace os, DbContext db) {
        var tinta = os.GetObjectsQuery<C.Postare>()
            .Where(p => p.FelUnitate == N.FelUnitate.Fisa && p.Carte != N.Carte.Contabil && p.LinieId != null)
            .Select(p => new { p.ID, p.Spatiu }).FirstOrDefault();
        if (tinta == null) return false;
        db.Set<C.Postare>().Where(p => p.ID == tinta.ID && p.Spatiu == tinta.Spatiu)
            .ExecuteUpdate(s => s.SetProperty(p => p.LinieId, (Guid?)null));
        return true;
    }

    static bool FisaFaraOrigine(IObjectSpace os, DbContext db) {
        var tinta = os.GetObjectsQuery<C.Postare>()
            .Where(p => p.FelUnitate == N.FelUnitate.Fisa && p.Carte != N.Carte.Contabil
                && p.Tranzactie.Fel == N.FelTranzactie.Storno && p.InversaDinId != null)
            .Select(p => new { p.ID, p.Spatiu }).FirstOrDefault();
        if (tinta == null) return false;
        db.Set<C.Postare>().Where(p => p.ID == tinta.ID && p.Spatiu == tinta.Spatiu)
            .ExecuteUpdate(s => s.SetProperty(p => p.InversaDinId, (Guid?)null).SetProperty(p => p.InversaDinSpatiu, (N.Spatiu?)null));
        return true;
    }

    static bool FisaFaraSuport(IObjectSpace os, DbContext db) {
        var tinta = os.GetObjectsQuery<C.Postare>()
            .Where(p => p.FelUnitate == N.FelUnitate.Fisa && p.Tranzactie.Fel == N.FelTranzactie.Transfer
                && p.Valoare != 0m && p.SuportId != null)
            .Select(p => new { p.ID, p.Spatiu }).FirstOrDefault();
        if (tinta == null) return false;
        db.Set<C.Postare>().Where(p => p.ID == tinta.ID && p.Spatiu == tinta.Spatiu)
            .ExecuteUpdate(s => s.SetProperty(p => p.SuportId, (Guid?)null).SetProperty(p => p.SuportSpatiu, (N.Spatiu?)null));
        return true;
    }

    static bool RegistruImobilizariDiferit(IObjectSpace os, DbContext db) {
        var tinta = os.GetObjectsQuery<RegistruImobilizari>().Select(r => (Guid?)r.ID).FirstOrDefault();
        if (tinta == null) return false;
        db.Set<RegistruImobilizari>().Where(r => r.ID == tinta)
            .ExecuteUpdate(s => s.SetProperty(r => r.Valoare, r => r.Valoare + 1));
        return true;
    }

    static bool Rescrie(IObjectSpace os, DbContext db, Func<C.Explicatie, C.Explicatie> schimba) {
        foreach (var t in os.GetObjectsQuery<C.Tranzactie>().Where(t => t.Explicatie != null)
                .OrderBy(t => t.ID).Select(t => new { t.ID, t.Explicatie }).ToList()) {
            if (schimba(C.Explicatie.Citeste(t.Explicatie)) is not { } schimbata) continue;
            var json = schimbata.Scrie();
            db.Set<C.Tranzactie>().Where(x => x.ID == t.ID).ExecuteUpdate(s => s.SetProperty(x => x.Explicatie, json));
            return true;
        }
        return false;
    }

    static C.Explicatie ValoareIesireDiferita(C.Explicatie e) {
        var tinta = e.Decizii.OfType<N.ValoareIesire>().FirstOrDefault();
        return tinta == null ? null : e with {
            Decizii = [.. e.Decizii.Select(d => ReferenceEquals(d, tinta) ? tinta with { Valoare = tinta.Valoare + 0.01m } : d)],
        };
    }

    static C.Explicatie SoldCititDiferit(C.Explicatie e) {
        var iesire = e.Decizii.OfType<N.ValoareIesire>().FirstOrDefault();
        var tinta = iesire == null ? null : e.Ipoteze.OfType<N.SoldUnitateCitit>()
            .FirstOrDefault(i => i.Unitate.Id == iesire.Unitate.Id && i.Unitate.Cont == iesire.Unitate.Cont);
        return tinta == null ? null : e with {
            Ipoteze = [.. e.Ipoteze.Select(i => ReferenceEquals(i, tinta)
                ? tinta with { Sold = tinta.Sold with { Debit = tinta.Sold.Debit + 1000m } } : i)],
        };
    }

    static C.Explicatie Fara<T>(C.Explicatie e) where T : N.Decizie =>
        e.Decizii.OfType<T>().Any() ? e with { Decizii = [.. e.Decizii.Where(d => d is not T)] } : null;

    static bool ExplicatieStearsa(IObjectSpace os, DbContext db) {
        var cuIesiri = os.GetObjectsQuery<C.Postare>().Where(p => p.FelUnitate == N.FelUnitate.Lot && p.Cantitate < 0m
            && p.Tranzactie.Explicatie != null).Select(p => (Guid?)p.TranzactieId).FirstOrDefault();
        return cuIesiri != null && db.Set<C.Tranzactie>().Where(t => t.ID == cuIesiri)
            .ExecuteUpdate(s => s.SetProperty(t => t.Explicatie, (string)null)) > 0;
    }

    static bool ExplicatieReferitaGresit(IObjectSpace os, DbContext db) {
        var referinta = os.GetObjectsQuery<C.Tranzactie>().Where(t => t.ExplicatieDinId != null)
            .Select(t => new { t.ID, t.DocumentId }).FirstOrDefault();
        if (referinta == null) return false;
        var strain = os.GetObjectsQuery<C.Tranzactie>().Where(t => t.Explicatie != null && t.DocumentId != referinta.DocumentId)
            .Select(t => (Guid?)t.ID).FirstOrDefault();
        return strain != null && db.Set<C.Tranzactie>().Where(t => t.ID == referinta.ID)
            .ExecuteUpdate(s => s.SetProperty(t => t.ExplicatieDinId, strain)) > 0;
    }

    static bool InversaPeLotDiferita(IObjectSpace os, DbContext db) {
        var tinta = os.GetObjectsQuery<C.Postare>()
            .Where(p => p.Tranzactie.Fel == N.FelTranzactie.Storno && p.FelUnitate == N.FelUnitate.Lot)
            .Select(p => new { p.ID, p.Spatiu, p.DocumentId, p.Unitate, p.Gestiune }).FirstOrDefault();
        if (tinta == null) return false;
        db.Set<C.Postare>().Where(p => p.ID == tinta.ID && p.Spatiu == tinta.Spatiu)
            .ExecuteUpdate(s => s.SetProperty(p => p.Cantitate, p => p.Cantitate + 1m));
        // Registrul primește aceeași abatere, ca mutantul să ajungă la ramura explicației, nu la acoperirea stocului.
        var rand = os.GetObjectsQuery<RegistruStoc>().Where(r => r.Storno && r.DocumentId == tinta.DocumentId
            && r.LotId == tinta.Unitate && r.RepartitorId == tinta.Gestiune
            && (r.TipStoc == TipStoc.Magazie || r.TipStoc == TipStoc.Marfuri || r.TipStoc == TipStoc.Folosinta)).Select(r => (Guid?)r.ID).FirstOrDefault();
        db.Set<RegistruStoc>().Where(r => r.ID == rand).ExecuteUpdate(s => s.SetProperty(r => r.Cantitate, r => r.Cantitate + 1m));
        return true;
    }
}
