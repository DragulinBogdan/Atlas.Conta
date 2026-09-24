using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed class ScenariiDeschidere(Func<IObjectSpace> deschide, Action<string, bool> check, bool privat,
    Action<IObjectSpace, int, int> inchide) : ScenaDocumente(deschide, check, privat, inchide, "DES", 2017) {
    Guid contTert, ancora;
    readonly Guid ref1 = Guid.NewGuid(), ref2 = Guid.NewGuid();
    C.SoldInitial[] solduri;
    C.LotInitial[] loturi;
    C.PartidaInitiala[] partide;
    int numar;

    public new void Ruleaza() {
        try { base.Ruleaza(); }
        finally { using var os = Deschide(); var pj = new Purja(os);
            pj.Adauga(os.GetObjectsQuery<Cont>().Where(c => c.Simbol == Marcaj)); pj.Executa(); }
    }

    protected override void CurataCubSuplimentar(IObjectSpace os, Purja pj) {
        var conturi = os.GetObjectsQuery<Cont>().Where(c => c.Simbol == Marcaj).Select(c => c.ID);
        var ids = os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == null && conturi.Contains(p.Cont))
            .Select(p => p.TranzactieId).Distinct().ToList();
        pj.AdaugaCheie<C.Postare>(os.GetObjectsQuery<C.Postare>().Where(p => ids.Contains(p.TranzactieId)).Select(p => p.ID).ToList());
        pj.AdaugaCheie<C.Tranzactie>(ids);
    }

    C.LotInitial Lot(decimal q, decimal v) => CuSpatiu(os => {
        var p = os.CreateObject<Produs>(); p.Cod = Marcaj + "-" + ++numar; p.Denumire = p.Cod;
        p.UM = "BUC"; p.TipMaterialId = Tip(os, Stoc);
        var l = os.CreateObject<Lot>(); l.Produs = p; l.GestiuneId = Magazie; l.Data = Ianuarie;
        l.PretUnitar = q == 0 ? 0 : v / q; os.CommitChanges();
        return new C.LotInitial(Cont(Stoc), l.ID, Magazie, q, v);
    });

    Guid Scrie(IObjectSpace os, C.SoldInitial[] s = null, C.LotInitial[] l = null, C.PartidaInitiala[] p = null) =>
        C.Materializare.Deschide(os, Ianuarie, s ?? solduri, l ?? loturi, p ?? partide);
    Guid Partida(Guid referinta, Guid partener) => N.Unitate.DeschidePartida(contTert, partener, referinta, Ianuarie).Id;
    void Rest(string id, Guid referinta, Guid partener, decimal valoare, DateOnly? la = null) =>
        SoldPartida(id, Partida(referinta, partener), la ?? new DateOnly(An, 1, 31), -valoare);

    protected override void Executa() {
        Comanda(os => {
            var c = os.CreateObject<Cont>(); c.Simbol = Marcaj; c.Denumire = Marcaj; c.Functie = "C";
            c.UrmarestePartide = true; contTert = c.ID;
            ancora = os.GetObjectsQuery<Cont>().First(c => c.RolTert == RolTertCont.Niciunul && c.Simbol.StartsWith("891")).ID;
            os.GetObjectByKey<Partener>(Furnizor).ContImplicitId = c.ID;
            os.CommitChanges();
        });
        loturi = [Lot(4, 40), Lot(3, 60), Lot(2, 0)];
        solduri = [new(Cont(Stoc), N.Latura.Debit, 100, true), new(ancora, N.Latura.Credit, 100),
            new(contTert, N.Latura.Credit, 150, true), new(ancora, N.Latura.Debit, 150)];
        partide = [new(contTert, Furnizor, ref1, N.Latura.Credit, 60),
            new(contTert, Furnizor, ref2, N.Latura.Credit, 40), new(contTert, Client, ref1, N.Latura.Credit, 50)];
        Refuzuri(); Performanta();
        using (var os = Deschide()) using (var tx = TranzactieComanda.Incepe(os)) {
            Scrie(os, solduri.Take(2).ToArray(), loturi.Take(2).ToArray(), []);
            var r = os.ModifiedObjects.OfType<C.Postare>().ToArray();
            Verifica("SC-DES-01", "exemplul izolat: exact trei postări, D=C=100", r.Length == 3
                && r.Where(p => p.Latura == N.Latura.Debit).Sum(p => p.Valoare) == 100
                && r.Where(p => p.Latura == N.Latura.Credit).Sum(p => p.Valoare) == 100);
        }
        using (var os = Deschide()) using (var tx = TranzactieComanda.Incepe(os)) {
            Scrie(os); var cate = os.ModifiedObjects.OfType<C.Postare>().Count();
            Refuza("SC-DES-05", () => Scrie(os), C.Materializare.DeschidereExistenta);
            Verifica("SC-DES-05", "al doilea apel nu adaugă postări", os.ModifiedObjects.OfType<C.Postare>().Count() == cate);
        }
        using (var a = Deschide()) using (var b = Deschide())
        using (var ta = TranzactieComanda.Incepe(a)) using (var tb = TranzactieComanda.Incepe(b)) {
            Scrie(a); Scrie(b); a.CommitChanges(); ta.Commit();
            try { b.CommitChanges(); tb.Commit(); Verifica("SC-DES-05", "unicitate în bază", false); }
            catch (Exception e) when (CauzaPg(e) is { SqlState: PostgresErrorCodes.UniqueViolation }) {
                Verifica("SC-DES-05", "două sesiuni: a doua deschidere refuzată de indexul unic", true);
            }
        }
        Comanda(os => { using var tx = TranzactieComanda.Incepe(os);
            Refuza("SC-DES-05", () => Scrie(os), C.Materializare.DeschidereExistenta); });
        using (var os = Deschide()) {
            var r = os.GetObjectsQuery<C.Postare>().Where(p => p.Tranzactie.Fel == N.FelTranzactie.Deschidere).ToList();
            Verifica("SC-DES-01/02", "8 postări detaliate, D=C=250, stoc 100; fără sold bloc dublat", r.Count == 8
                && r.Where(p => p.Latura == N.Latura.Debit).Sum(p => p.Valoare) == 250
                && r.Where(p => p.Latura == N.Latura.Credit).Sum(p => p.Valoare) == 250
                && r.Where(p => p.Cont == Cont(Stoc)).Sum(p => p.Valoare) == 100
                && r.Where(p => p.Cont == contTert).All(p => p.Unitate != null && p.Partener != null));
            Verifica("SC-DES-08", "round-trip fără document fictiv", r.All(p => p.DocumentId == null && p.LinieId == null
                && C.Randuri.Citeste(p).Cauza.Document == Guid.Empty && C.Randuri.Citeste(p).Valoare == p.Valoare));
        }
        foreach (var l in loturi) {
            SoldLot("SC-DES-01/07", l.Lot, Magazie, Ianuarie, l.Cantitate, l.Valoare);
            SoldLot("SC-DES-08", l.Lot, Magazie, Ianuarie.AddDays(-1), 0, 0);
        }
        Rest("SC-DES-02", ref1, Furnizor, 60); Rest("SC-DES-02", ref2, Furnizor, 40); Rest("SC-DES-02", ref1, Client, 50);
        var plata = Trezorerie(false, 20); Opereaza(plata.Id);
        void Stinge(Guid partida, decimal suma, DateOnly data, Guid? document = null) => Comanda(os => {
            using var tx = TranzactieComanda.Incepe(os);
            C.Materializare.Imperecheaza(os, os.GetObjectByKey<Document>(document ?? plata.Id), partida, suma, data);
            os.CommitChanges(); tx.Commit();
        });
        Refuza("SC-DES-09", () => Stinge(Partida(ref1, Furnizor), 61, Ianuarie), C.Materializare.StingereDeschidereInvalida);
        var plataMare = Trezorerie(false, 70); Opereaza(plataMare.Id);
        Refuza("SC-DES-09", () => Stinge(Partida(ref1, Furnizor), 61, Ianuarie, plataMare.Id), C.Materializare.StingereDeschidereInvalida);
        Rest("SC-DES-09", ref1, Furnizor, 60);
        Refuza("SC-DES-09", () => Stinge(Partida(ref1, Client), 20, Ianuarie), C.Materializare.StingereDeschidereInvalida);
        Refuza("SC-DES-09", () => Stinge(Partida(ref1, Furnizor), 20, Ianuarie.AddDays(-1)), C.Materializare.StingereDeschidereInvalida);
        string Original() => CuSpatiu(os => System.Text.Json.JsonSerializer.Serialize(os.GetObjectsQuery<C.Postare>()
            .Where(p => p.Tranzactie.Fel == N.FelTranzactie.Deschidere).OrderBy(p => p.ID)
            .Select(p => new { p.ID, p.Data, p.Cont, p.Carte, p.Latura, p.Valoare, p.Cantitate,
                p.Unitate, p.Partener, p.Gestiune, p.DocumentId, p.LinieId }).ToArray()));
        var initial = Original();
        Stinge(Partida(ref1, Furnizor), 20, Ianuarie);
        using (var os = Deschide()) {
            var r = os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == plata.Id && p.Tranzactie.Fel == N.FelTranzactie.Transfer).ToList();
            Verifica("SC-DES-03", "transfer exact pe debit: partidă inițială +20, partidă plată -20", r.Count == 2
                && r.All(p => p.Cont == contTert && p.Partener == Furnizor && p.Carte == N.Carte.Contabil && p.Latura == N.Latura.Debit)
                && r.Single(p => p.Valoare == 20).Unitate == Partida(ref1, Furnizor)
                && r.Single(p => p.Valoare == -20).Unitate != Partida(ref1, Furnizor));
        }
        Rest("SC-DES-03", ref1, Furnizor, 40); Rest("SC-DES-03", ref2, Furnizor, 40); Rest("SC-DES-03", ref1, Client, 50);
        Refuza("SC-DES-09", () => Stinge(Partida(ref2, Furnizor), 1, Ianuarie), C.Materializare.StingereDeschidereInvalida);
        InchideIanuarie();
        Refuza("SC-DES-09", () => Stinge(Partida(ref2, Furnizor), 1, Ianuarie), "închis");
        Storneaza(plata.Id, Februarie);
        Rest("SC-DES-03", ref1, Furnizor, 40); Rest("SC-DES-03", ref1, Furnizor, 60, Februarie);
        Verifica("SC-DES-03", "originalul deschiderii păstrat", initial == Original());
    }

    void Refuzuri() {
        void Refuz(string nume, C.SoldInitial[] s, C.LotInitial[] l, C.PartidaInitiala[] p, string cod) {
            using var os = Deschide(); using var tx = TranzactieComanda.Incepe(os);
            Refuza("SC-DES-04/06/07 " + nume, () => Scrie(os, s, l, p), cod);
            Verifica("SC-DES-06", "refuzul nu adaugă entități de cub", !os.ModifiedObjects.OfType<C.Postare>().Any()
                && !os.ModifiedObjects.OfType<C.Tranzactie>().Any());
        }
        foreach (var v in new[] { 39m, 41m }) Refuz("diferență", null, [loturi[0] with { Valoare = v }, loturi[1]], null, C.Materializare.DeschidereDiferenta);
        Refuz("lipsă", null, [], null, C.Materializare.DeschidereDiferenta);
        Refuz("fără control", solduri.Skip(2).ToArray(), null, null, C.Materializare.DeschidereInvalida);
        Refuz("cont", [solduri[0] with { Cont = Guid.NewGuid() }, .. solduri.Skip(1)], null, null, C.Materializare.DeschidereInvalida);
        foreach (var l in new[] { loturi[0] with { Lot = Guid.NewGuid() }, loturi[0] with { Gestiune = Guid.NewGuid() },
            loturi[0] with { Cont = ancora }, loturi[0] with { Cantitate = -1 }, loturi[0] with { Valoare = -1 } })
            Refuz("lot", null, [l, loturi[1]], null, C.Materializare.DeschidereInvalida);
        Refuz("partener", null, null, [partide[0] with { Partener = Magazie }, .. partide.Skip(1)], C.Materializare.DeschidereInvalida);
        Refuz("referință", null, null, [partide[0] with { Referinta = Guid.Empty }, .. partide.Skip(1)], C.Materializare.DeschidereInvalida);
        Refuz("carte", [solduri[0] with { Carte = N.Carte.Fiscal }, .. solduri.Skip(1)], null, null, C.Materializare.DeschidereInvalida);
        Comanda(os => { os.GetObjectByKey<Lot>(loturi[0].Lot).Data = Ianuarie.AddDays(1); os.CommitChanges(); });
        Refuz("dată lot", null, null, null, C.Materializare.DeschidereInvalida);
        Comanda(os => { os.GetObjectByKey<Lot>(loturi[0].Lot).Data = Ianuarie; os.CommitChanges(); });
    }

    void Performanta() {
        int Citiri(int cate) {
            var l = Enumerable.Range(0, cate).Select(_ => Lot(1, 1)).ToArray();
            using var os = Deschide(); using var tx = TranzactieComanda.Incepe(os);
            NumaratorSql.Instanta.Reseteaza();
            Scrie(os, [new(Cont(Stoc), N.Latura.Debit, cate, true), new(ancora, N.Latura.Credit, cate)], l, []);
            return NumaratorSql.Instanta.Numar;
        }
        var mic = Citiri(2); var mare = Citiri(51);
        Verifica("SC-DES-10", $"2/51 poziții: {mic}/{mare} interogări", mic == mare && mic > 0 && mare <= 16);
    }

    static PostgresException CauzaPg(Exception e) => e as PostgresException ?? (e.InnerException == null ? null : CauzaPg(e.InnerException));
}
