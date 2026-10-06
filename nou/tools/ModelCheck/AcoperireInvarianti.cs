using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
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
        new("PARTIDE", "CITIRE_PARTIDE_INCOMPLETE", PartidaFaraPartener),
        new("IMO-CAUZA", "fără cauză", FisaFaraLinie),
        new("IMO-ORIGINE", "fără cauză", FisaFaraOrigine),
        new("IMO-SUPORT", "fără cauză", FisaFaraSuport),
        new("EXPLICATIE-LIPSA", C.Citiri.Explicatii.Lipsa, ExplicatieStearsa),
        new("EXPLICATIE-REFERINTA", C.Citiri.Explicatii.Referinta, ExplicatieReferitaGresit),
        new("EXPLICATIE-STORNO", C.Citiri.Explicatii.Storno, InversaPeLotDiferita),
        new("EXPLICATIE-IESIRE", C.Citiri.Explicatii.Iesire, (os, db) => Rescrie(os, db, ValoareIesireDiferita)),
        new("EXPLICATIE-DECLARATA", C.Citiri.Explicatii.Iesire, (os, db) => Rescrie(os, db, Fara<N.ValoareDeclarata>)),
        new("EXPLICATIE-EVALUARE", C.Citiri.Explicatii.Evaluare, (os, db) => Rescrie(os, db, SoldCititDiferit)),
        new("EXPLICATIE-STINGERE", C.Citiri.Explicatii.Stingere, (os, db) => Rescrie(os, db, Fara<N.AlocareFifo>)),
        new("EXPLICATIE-MECANISM", C.Citiri.Explicatii.Mecanism, (os, db) => Rescrie(os, db, EvaluatDeclarat)),
        new("EXPLICATIE-DECLARANT", C.Citiri.Explicatii.Mecanism, (os, db) => Rescrie(os, db,
            e => EvaluatDeclarat(e) is { } declarata ? declarata with { Declarant = nameof(DeclarantReturFurnizor) } : null)),
        new("EXPLICATIE-SOLD-FIFO", C.Citiri.Explicatii.Stingere, (os, db) => Rescrie(os, db, e => SoldFifo(e, null))),
        new("EXPLICATIE-SOLD-FIFO-MIC", C.Citiri.Explicatii.Stingere, (os, db) => Rescrie(os, db, e => SoldFifo(e, N.Sold.Zero))),
        new("TRANSFER-CONT", C.Citiri.Invarianti.TransferNeconservat, DestinatieTransferPeAltCont),
        new("LINIE-BCS", C.Citiri.Explicatii.Linie, (os, db) => LinieDiferita<BonConsum>(os, db, N.FelTranzactie.Operare, v => v + 1m)),
        new("LINIE-BTR", C.Citiri.Explicatii.Linie, (os, db) => LinieDiferita<NotaTransfer>(os, db, N.FelTranzactie.Transfer, v => v + 1m)),
        new("LINIE-ASM", C.Citiri.Explicatii.Linie, (os, db) => LinieDiferita<Asamblare>(os, db, N.FelTranzactie.Transfer, v => v + 1m)),
        new("LINIE-SEMN-BCS", C.Citiri.Explicatii.Linie, (os, db) => LinieDiferita<BonConsum>(os, db, N.FelTranzactie.Operare, v => -v)),
        new("LINIE-SEMN-ASM", C.Citiri.Explicatii.Linie, (os, db) => LinieDiferita<Asamblare>(os, db, N.FelTranzactie.Transfer, v => -v)),
        new("IESIRE-SEMN-BTR", C.Citiri.Explicatii.Iesire, (os, db) => Rescrie(os, db, IesireCuSemnOpus, Purtatori<NotaTransfer>(os, N.FelTranzactie.Transfer))),
        new("IESIRE-SEMN-ASM", C.Citiri.Explicatii.Iesire, (os, db) => Rescrie(os, db, IesireCuSemnOpus, Purtatori<Asamblare>(os, N.FelTranzactie.Transfer))),
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

    static bool PartidaFaraPartener(IObjectSpace os, DbContext db) {
        var tinta = os.GetObjectsQuery<C.Postare>().Where(C.Citiri.Transformare.FaraContrapondere)
            .Where(p => p.Carte == N.Carte.Contabil && p.FelUnitate == N.FelUnitate.Partida && p.Partener != null)
            .Select(p => new { p.ID, p.Spatiu }).FirstOrDefault();
        if (tinta == null) return false;
        db.Set<C.Postare>().Where(p => p.ID == tinta.ID && p.Spatiu == tinta.Spatiu)
            .ExecuteUpdate(s => s.SetProperty(p => p.Partener, (Guid?)null));
        return true;
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

    // Explicațiile purtate de tranzacțiile de felul dat ale documentelor de tipul T.
    static IQueryable<C.Tranzactie> Purtatori<T>(IObjectSpace os, N.FelTranzactie fel) where T : Document =>
        os.GetObjectsQuery<C.Tranzactie>().Where(t => t.Explicatie != null && t.Fel == fel
            && os.GetObjectsQuery<T>().Any(d => d.ID == t.DocumentId));

    // D9-D3 (a): linia unei ieșiri evaluate primește altă valoare decât decizia ei.
    static bool LinieDiferita<T>(IObjectSpace os, DbContext db, N.FelTranzactie fel, Func<decimal, decimal> schimba) where T : Document {
        foreach (var json in Purtatori<T>(os, fel).OrderBy(t => t.ID).Select(t => t.Explicatie).ToList()) {
            if (C.Explicatie.Citeste(json).Decizii.OfType<N.ValoareIesire>().FirstOrDefault() is not { } iesire) continue;
            var valoare = db.Set<DocumentDetaliu>().Where(l => l.ID == iesire.Linie).Select(l => (decimal?)l.Valoare).FirstOrDefault();
            if (valoare is not decimal v || schimba(v) == v) continue;
            var noua = schimba(v);
            return db.Set<DocumentDetaliu>().Where(l => l.ID == iesire.Linie)
                .ExecuteUpdate(s => s.SetProperty(l => l.Valoare, noua)) > 0;
        }
        return false;
    }

    // Normalizarea pe latură e semnată: o decizie cu semnul opus nu trece drept aceeași ieșire.
    static C.Explicatie IesireCuSemnOpus(C.Explicatie e) {
        var tinta = e.Decizii.OfType<N.ValoareIesire>().FirstOrDefault(i => i.Valoare != 0m);
        return tinta == null ? null : e with {
            Decizii = [.. e.Decizii.Select(d => ReferenceEquals(d, tinta) ? tinta with { Valoare = -tinta.Valoare } : d)],
        };
    }

    static bool Rescrie(IObjectSpace os, DbContext db, Func<C.Explicatie, C.Explicatie> schimba, IQueryable<C.Tranzactie> purtatori = null) {
        foreach (var t in (purtatori ?? os.GetObjectsQuery<C.Tranzactie>().Where(t => t.Explicatie != null))
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

    // X-RI1: aceleași cifre, alt mecanism — ieșirile evaluate redenumite „declarate”, fără soldurile citite.
    static C.Explicatie EvaluatDeclarat(C.Explicatie e) => !e.Decizii.OfType<N.ValoareIesire>().Any() ? null : e with {
        Decizii = [.. e.Decizii.Select(d => d is N.ValoareIesire i
            ? new N.ValoareDeclarata(i.Linie, i.Unitate, i.Cantitate, i.Valoare, SurseValoare.Linie) : d)],
        Ipoteze = [.. e.Ipoteze.Where(i => i is not N.SoldUnitateCitit)],
    };

    static C.Explicatie SoldFifo(C.Explicatie e, N.Sold inlocuit) {
        var alocare = e.Decizii.OfType<N.AlocareFifo>().FirstOrDefault();
        var tinta = alocare == null ? null : e.Ipoteze.OfType<N.SoldUnitateCitit>()
            .FirstOrDefault(i => i.Unitate.Id == alocare.Unitate.Id && i.Unitate.Cont == alocare.Unitate.Cont);
        return tinta == null ? null : e with {
            Ipoteze = [.. e.Ipoteze.Where(i => inlocuit != null || !ReferenceEquals(i, tinta))
                .Select(i => ReferenceEquals(i, tinta) ? tinta with { Sold = inlocuit } : i)],
        };
    }

    // X-RI2: numai contul capătului de destinație al unui transfer pe lot.
    static bool DestinatieTransferPeAltCont(IObjectSpace os, DbContext db) {
        var tinta = os.GetObjectsQuery<C.Postare>()
            .Where(p => p.Tranzactie.Fel == N.FelTranzactie.Transfer && p.FelUnitate == N.FelUnitate.Lot
                && p.Carte == N.Carte.Contabil && p.Cantitate > 0m && p.Valoare != 0m)
            .Select(p => new { p.ID, p.Spatiu, p.Cont }).FirstOrDefault();
        if (tinta == null) return false;
        var altCont = os.GetObjectsQuery<Cont>().Where(c => c.ID != tinta.Cont).OrderBy(c => c.Simbol).Select(c => c.ID).First();
        db.Set<C.Postare>().Where(p => p.ID == tinta.ID && p.Spatiu == tinta.Spatiu)
            .ExecuteUpdate(s => s.SetProperty(p => p.Cont, altCont));
        return true;
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
        return true;
    }
}
