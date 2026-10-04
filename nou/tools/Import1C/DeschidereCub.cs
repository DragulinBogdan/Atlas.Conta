using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Import1C;

static partial class Deschidere {
    public const string ViewPartide = "PartidaDeschidere";
    public const string ViewPartenerMigrare = "PartenerMigrare";
    public const string CodPartenerGeneric = "MIGRARE-NEDEFINIT";
    public const string CheiePartenerGeneric = "-";
    public const string CheieFaraDocument = "-";

    public sealed record Control(string Simbol, N.Latura Latura, decimal Valoare);

    public sealed record PartidaSursa(string Cheie, string Simbol, string PartenerHex,
        string RefCheie, decimal Sold, string Descriere, Guid Partener, Guid Referinta);

    public sealed record RezultatPartide(
        IReadOnlyList<PartidaSursa> Partide, IReadOnlyList<PartidaInitiala> Intrare,
        Guid PartenerGeneric, int Pozitii, int PeConturiNeurmarite, int Nemapate,
        int FaraPartener, int PartenerRecuperatDinDocument, int FaraDocument, int Agregate,
        decimal SoldPeGeneric);

    public static string CheieSursa(string simbol, string partenerHex, string refCheie) =>
        $"{simbol}|{partenerHex ?? CheiePartenerGeneric}|{refCheie ?? CheieFaraDocument}";

    public static string RefCheie(string tipRef, string id) =>
        tipRef == null || id == null ? null : $"{tipRef}/{id}";

    // 107c: Guid determinist din cheia sursei, fără FK spre Document.
    static Guid ReferintaDin(string text) {
        var amprenta = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(text));
        var id = amprenta[..16];
        id[7] = (byte)((id[7] & 0x0F) | 0x80);
        return new Guid(id);
    }

    public static RezultatPartide Partide(IObjectSpaceProvider provider, ImportLaCerere laCerere,
            FlaxDb flax, IReadOnlyList<FlaxPozitieTert> pozitii, Func<string, string> mapeaza,
            IReadOnlyDictionary<string, Guid> plan, IReadOnlySet<string> urmarite,
            Action<string> avert, Action<string, bool> check) {
        var generic = AsiguraPartenerGeneric(provider);
        var nemapate = new List<FlaxPozitieTert>();
        var neurmarite = 0;
        var faraPartener = 0;
        var recuperate = 0;
        var faraDocument = 0;
        var agregate = new Dictionary<string, (string Simbol, string PartenerHex, string RefCheie,
            decimal Sold, string Descriere, Guid Partener)>(StringComparer.Ordinal);
        foreach (var p in pozitii) {
            var simbol = mapeaza(p.Cont);
            if (simbol == null) {
                nemapate.Add(p);
                continue;
            }
            if (!urmarite.Contains(simbol)) {
                neurmarite++;
                continue;
            }
            var refCheie = RefCheie(p.DocTipRef, p.DocId);
            if (refCheie == null)
                faraDocument++;
            Guid? partener = null;
            if (p.PartenerId != null)
                partener = laCerere.AsiguraPartener(p.PartenerId);
            if (partener == null) {
                faraPartener++;
                var dinDocument = flax.PartenerDocument(p.DocTipNume, p.DocId);
                if (dinDocument != null && laCerere.AsiguraPartener(dinDocument) is { } recuperat) {
                    recuperate++;
                    partener = recuperat;
                    avert($"Poziție de terț {p.Cont} {p.SoldIni:N2} fără partener în subconto — partenerul "
                        + $"recuperat din antetul documentului „{p.DocDesc}”.");
                }
            }
            var cheie = CheieSursa(simbol, p.PartenerId, refCheie);
            if (!agregate.TryGetValue(cheie, out var cur))
                cur = (simbol, p.PartenerId, refCheie, 0m, p.DocDesc ?? p.PartenerDesc, partener ?? generic);
            agregate[cheie] = (cur.Simbol, cur.PartenerHex, cur.RefCheie, cur.Sold + p.SoldIni, cur.Descriere, cur.Partener);
        }
        foreach (var p in nemapate.OrderByDescending(p => Math.Abs(p.SoldIni)))
            check($"Poziție de terț 1C {p.Cont} ({p.SoldIni:N2}) se mapează pe planul OMFP", false);

        var partide = new List<PartidaSursa>();
        var intrare = new List<PartidaInitiala>();
        foreach (var (cheie, a) in agregate.OrderBy(x => x.Key, StringComparer.Ordinal)) {
            if (a.Sold == 0m)
                continue;
            var referinta = ReferintaDin(a.RefCheie != null ? $"1C:{a.RefCheie}" : $"1C:{a.Simbol}|{a.PartenerHex ?? CheiePartenerGeneric}|fara-document");
            partide.Add(new PartidaSursa(cheie, a.Simbol, a.PartenerHex, a.RefCheie, a.Sold, a.Descriere, a.Partener, referinta));
            intrare.Add(new PartidaInitiala(plan[a.Simbol], a.Partener, referinta,
                a.Sold > 0 ? N.Latura.Debit : N.Latura.Credit, Math.Abs(a.Sold)));
        }
        var duplicate = intrare.GroupBy(i => (i.Cont, i.Partener, i.Referinta)).Count(g => g.Count() > 1);
        check($"Partidele inițiale au identitate (cont, partener, referință) distinctă ({duplicate} coliziuni)",
            duplicate == 0);
        return new RezultatPartide(partide, intrare, generic, pozitii.Count, neurmarite, nemapate.Count,
            faraPartener, recuperate, faraDocument, pozitii.Count - neurmarite - nemapate.Count - agregate.Count,
            partide.Where(p => p.Partener == generic).Sum(p => p.Sold));
    }

    static Guid AsiguraPartenerGeneric(IObjectSpaceProvider provider) {
        using var os = provider.CreateObjectSpace();
        var legaturi = Legaturi.Incarca(os, ViewPartenerMigrare);
        if (legaturi.TryGetValue(CodPartenerGeneric, out var existent)
                && os.GetObjectByKey<Partener>(existent) != null)
            return existent;
        var p = os.GetObjectsQuery<Partener>().FirstOrDefault(x => x.Cod == CodPartenerGeneric)
            ?? os.CreateObject<Partener>();
        p.Cod = CodPartenerGeneric;
        p.Denumire = "Partener nedefinit în sursa 1C (migrare)";
        os.CommitChanges();
        Legaturi.Leaga(os, ViewPartenerMigrare, CodPartenerGeneric, p.ID);
        os.CommitChanges();
        return p.ID;
    }

    public sealed record RezultatControale(IReadOnlyList<Control> Controale,
        IReadOnlyDictionary<string, decimal> DeclarateStoc, int ConturiStocFaraLot);

    public static RezultatControale Controale(IReadOnlyDictionary<string, decimal> net,
            IReadOnlyList<PartidaInitiala> partide, IReadOnlyList<LotInitial> loturi,
            IReadOnlyDictionary<Guid, string> simbolPeId, IReadOnlySet<string> urmarite,
            IReadOnlySet<string> conturiStoc, Action<string> avert, Action<string, bool> check) {
        var controale = new List<Control>();
        var declarate = new Dictionary<string, decimal>(StringComparer.Ordinal);
        var debitTert = partide.Where(p => p.Latura == N.Latura.Debit)
            .GroupBy(p => simbolPeId[p.Cont]).ToDictionary(g => g.Key, g => g.Sum(p => p.Valoare), StringComparer.Ordinal);
        var creditTert = partide.Where(p => p.Latura == N.Latura.Credit)
            .GroupBy(p => simbolPeId[p.Cont]).ToDictionary(g => g.Key, g => g.Sum(p => p.Valoare), StringComparer.Ordinal);
        var stoc = loturi.GroupBy(l => simbolPeId[l.Cont])
            .ToDictionary(g => g.Key, g => g.Sum(l => l.Valoare), StringComparer.Ordinal);
        var stocFaraLot = 0;
        foreach (var simbol in net.Keys.Union(debitTert.Keys).Union(creditTert.Keys).Union(stoc.Keys)
                     .OrderBy(s => s, StringComparer.Ordinal)) {
            var sold = net.GetValueOrDefault(simbol);
            if (urmarite.Contains(simbol)) {
                var d = debitTert.GetValueOrDefault(simbol);
                var c = creditTert.GetValueOrDefault(simbol);
                check($"Cont {simbol}: partidele inițiale D {d:N2} − C {c:N2} = soldul Balanței {sold:N2}",
                    Math.Abs(d - c - sold) < EpsV);
                if (d != 0m) controale.Add(new Control(simbol, N.Latura.Debit, d));
                if (c != 0m) controale.Add(new Control(simbol, N.Latura.Credit, c));
                continue;
            }
            if (conturiStoc.Contains(simbol) || stoc.ContainsKey(simbol)) {
                var scris = stoc.GetValueOrDefault(simbol);
                if (scris != 0m) controale.Add(new Control(simbol, N.Latura.Debit, scris));
                else if (sold != 0m) stocFaraLot++;
                var delta = scris - sold;
                if (delta != 0m) {
                    declarate[simbol] = delta;
                    avert($"Cont de stoc {simbol}: controlul deschiderii = Σ loturilor scrise {scris:N2}, "
                        + $"Balanța {sold:N2}; Δ {delta:N2} declarat „deschidere fără detaliu de lot” (M1-D6).");
                }
                continue;
            }
            if (sold != 0m)
                controale.Add(new Control(simbol, sold > 0 ? N.Latura.Debit : N.Latura.Credit, Math.Abs(sold)));
        }
        var sumaDebit = controale.Where(c => c.Latura == N.Latura.Debit).Sum(c => c.Valoare);
        var sumaCredit = controale.Where(c => c.Latura == N.Latura.Credit).Sum(c => c.Valoare);
        if (sumaCredit != 0m) controale.Add(new Control(Ancora, N.Latura.Debit, sumaCredit));
        if (sumaDebit != 0m) controale.Add(new Control(Ancora, N.Latura.Credit, sumaDebit));
        return new RezultatControale(controale, declarate, stocFaraLot);
    }

    public sealed record RezultatCub(bool Scrisa, Guid Tranzactie, int Postari, int PartideScrise,
        int LoturiScrise, int LegaturiNoi);

    public static RezultatCub Cub(IObjectSpaceProvider provider, DateOnly data,
            IReadOnlyList<Control> controale, IReadOnlyList<LotInitial> loturi,
            IReadOnlyList<PartidaInitiala> partide, IReadOnlyList<PartidaSursa> partideSursa,
            IReadOnlyDictionary<string, Guid> plan, Action<string> avert, Action<string, bool> check) {
        var solduri = controale.Select(c => new SoldInitial(plan[c.Simbol], c.Latura, c.Valoare)).ToList();
        Guid tranzactie;
        var scrisa = false;
        using (var os = provider.CreateObjectSpace()) {
            var existenta = os.GetObjectsQuery<Tranzactie>()
                .Where(t => t.Fel == N.FelTranzactie.Deschidere).Select(t => t.ID).ToList();
            if (existenta.Count > 0) {
                tranzactie = existenta[0];
                avert("Baza are deja tranzacția `Deschidere` a cubului — nu se rescrie (unică per bază); "
                    + "se verifică împotriva sursei.");
            }
            else {
                using var tx = TranzactieComanda.Incepe(os);
                tranzactie = Materializare.Deschide(os, data, solduri, loturi, partide);
                os.CommitChanges();
                tx.Commit();
                scrisa = true;
            }
        }

        var legaturiNoi = 0;
        using (var os = provider.CreateObjectSpace()) {
            var legaturi = Legaturi.Incarca(os, ViewPartide);
            foreach (var p in partideSursa) {
                var id = N.Unitate.DeschidePartidaInitiala(plan[p.Simbol], p.Partener, p.Referinta, data).Id;
                if (legaturi.TryGetValue(p.Cheie, out var existent)) {
                    if (existent != id)
                        check($"Legătura 1C:{ViewPartide}/{p.Cheie} indică partida {existent}, sursa dă {id}", false);
                    continue;
                }
                Legaturi.Leaga(os, ViewPartide, p.Cheie, id);
                legaturiNoi++;
            }
            os.CommitChanges();
        }

        int postari, partideScrise, loturiScrise;
        using (var os = provider.CreateObjectSpace()) {
            var ale = os.GetObjectsQuery<Postare>().Where(p => p.TranzactieId == tranzactie)
                .Select(p => new { p.Cont, p.Latura, p.Valoare, p.FelUnitate }).ToList();
            postari = ale.Count;
            partideScrise = ale.Count(p => p.FelUnitate == N.FelUnitate.Partida);
            loturiScrise = ale.Count(p => p.FelUnitate == N.FelUnitate.Lot);
            var simbolPeId = plan.ToDictionary(x => x.Value, x => x.Key);
            var cub = ale.GroupBy(p => (Simbol: simbolPeId[p.Cont], p.Latura))
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Valoare));
            var asteptat = controale.ToDictionary(c => (c.Simbol, c.Latura), c => c.Valoare);
            var diferente = cub.Keys.Union(asteptat.Keys)
                .Where(k => Math.Abs(cub.GetValueOrDefault(k) - asteptat.GetValueOrDefault(k)) >= EpsV)
                .ToList();
            foreach (var k in diferente)
                check($"  cub {k.Simbol} {k.Latura}: {cub.GetValueOrDefault(k):N2} = control {asteptat.GetValueOrDefault(k):N2}", false);
            check($"Cub: tranzacția Deschidere are {postari} postări pe {cub.Count} chei (cont, latură) = "
                + $"{controale.Count} controale, {diferente.Count} diferențe", diferente.Count == 0);
            check($"Cub: {partideScrise} partide inițiale = {partide.Count} din sursă; {loturiScrise} loturi = {loturi.Count}",
                partideScrise == partide.Count && loturiScrise == loturi.Count);
            try {
                Invarianti.Verifica(os);
                check("Cub: invarianții (INV-CUB) trec după deschidere", true);
            }
            catch (OperareException ex) {
                check($"Cub: invarianții (INV-CUB) după deschidere — {ex.Message.Split('\n')[0]}", false);
            }
        }
        return new RezultatCub(scrisa, tranzactie, postari, partideScrise, loturiScrise, legaturiNoi);
    }
}
