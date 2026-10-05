using Atlas.Conta.BackOffice.Module.Saft;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Api;
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
    Guid Partida(Guid referinta, Guid partener) => N.Unitate.DeschidePartidaInitiala(contTert, partener, referinta, Ianuarie).Id;
    void Rest(string id, Guid referinta, Guid partener, decimal valoare, DateOnly? la = null) =>
        SoldPartida(id, Partida(referinta, partener), la ?? new DateOnly(An, 1, 31), -valoare);

    protected override void Executa() {
        Comanda(os => {
            var c = os.CreateObject<Cont>(); c.Simbol = Marcaj; c.Denumire = Marcaj; c.Functie = "C";
            c.UrmarestePartide = true; c.RolTert = RolTertCont.Furnizor; contTert = c.ID;
            if (Economic == null) { var e = os.CreateObject<CodEconomic>(); e.Cod = Marcaj; e.Denumire = Marcaj; Economic = e.ID; }
            ancora = os.GetObjectsQuery<Cont>().First(c => c.RolTert == RolTertCont.Niciunul && c.Simbol.StartsWith("891")).ID;
            os.GetObjectByKey<Partener>(Furnizor).ContImplicitId = c.ID;
            os.CommitChanges();
        });
        loturi = [Lot(4, 40), Lot(3, 60), Lot(2, 0)];
        solduri = [new(Cont(Stoc), N.Latura.Debit, 100, true), new(ancora, N.Latura.Credit, 100),
            new(contTert, N.Latura.Credit, 150, true), new(ancora, N.Latura.Debit, 150)];
        partide = [new(contTert, Furnizor, ref1, N.Latura.Credit, 60),
            new(contTert, Furnizor, ref2, N.Latura.Credit, 40), new(contTert, Client, ref1, N.Latura.Credit, 50)];
        Refuzuri(); ReviewIntrare(); Performanta();
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
        Task<string> aDoua;
        using (var a = Deschide()) using (var ta = TranzactieComanda.Incepe(a)) {
            Scrie(a); a.CommitChanges();
            aDoua = Task.Run(() => {
                try { Comanda(b => { using var tb = TranzactieComanda.Incepe(b); Scrie(b); b.CommitChanges(); tb.Commit(); }); return ""; }
                catch (OperareException e) { return e.Message; }
            });
            AsteaptaBlocare(a, aDoua, "SC-DES-05");
            ta.Commit();
        }
        Verifica("SC-DES-05", "două sesiuni: a doua deschidere așteaptă prima și e refuzată",
            aDoua.GetAwaiter().GetResult().Contains(C.Materializare.DeschidereExistenta));
        Verifica("SC-DES-05", "indexul unic al bazei refuză a doua tranzacție de deschidere", CuSpatiu(os => {
            var db = ((EFCoreObjectSpace)os).DbContext;
            using var tx = db.Database.BeginTransaction();
            try {
                db.Database.ExecuteSqlInterpolated(
                    $"""INSERT INTO "Tranzactie" ("ID", "Fel", "Data", "ScrisLa") VALUES ({Guid.NewGuid()}, 4, {Ianuarie}, now())""");
                return false;
            }
            catch (PostgresException e) { return e.SqlState == PostgresErrorCodes.UniqueViolation; }
            finally { tx.Rollback(); }
        }));
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
        var consumInitial = Consum(loturi[0].Lot, 2);
        Opereaza(consumInitial);
        SoldLot("SC-CIT-36", loturi[0].Lot, Magazie, new(An, 1, 31), 2, 20);
        Verifica("SC-CIT-36", "consumul folosește deschiderea fără recepție în registru", CuSpatiu(os =>
            !os.GetObjectsQuery<RegistruStoc>().Any(r => r.LotId == loturi[0].Lot && r.RepartitorId == Magazie && r.Cantitate > 0)));
        Storneaza(consumInitial, new(An, 1, 20));
        SoldLot("SC-CIT-36", loturi[0].Lot, Magazie, new(An, 1, 31), 4, 40);
        DeschidereInSaftS();
        Rest("SC-DES-02", ref1, Furnizor, 60); Rest("SC-DES-02", ref2, Furnizor, 40); Rest("SC-DES-02", ref1, Client, 50);
        ReviewStingeri(); ReviewConcurenta();
        var plata = Trezorerie(false, 20);
        Comanda(os => { var d = os.GetObjectByKey<Document>(plata.Id);
            d.Data = Ianuarie.AddDays(2); d.DataInregistrare = d.Data; os.CommitChanges(); });
        Opereaza(plata.Id);
        var dataPlata = Ianuarie.AddDays(2);
        void Stinge(Guid partida, decimal suma, DateOnly data, Guid? document = null) => Comanda(os => {
            using var tx = TranzactieComanda.Incepe(os);
            C.Materializare.Imperecheaza(os, os.GetObjectByKey<Document>(document ?? plata.Id), partida, suma, data);
            os.CommitChanges(); tx.Commit();
        });
        Refuza("SC-DES-09", () => Stinge(Partida(ref1, Furnizor), 61, dataPlata), C.Materializare.StingereDeschidereInvalida);
        var plataMare = Trezorerie(false, 70); Opereaza(plataMare.Id);
        Refuza("SC-DES-09", () => Stinge(Partida(ref1, Furnizor), 61, Ianuarie, plataMare.Id), C.Materializare.StingereDeschidereInvalida);
        Rest("SC-DES-09", ref1, Furnizor, 60);
        Refuza("SC-DES-09", () => Stinge(Partida(ref1, Client), 20, dataPlata), C.Materializare.StingereDeschidereInvalida);
        Refuza("SC-DES-09", () => Stinge(Partida(ref1, Furnizor), 20, Ianuarie.AddDays(-1)), "precedă deschiderea");
        string Original() => CuSpatiu(os => System.Text.Json.JsonSerializer.Serialize(os.GetObjectsQuery<C.Postare>()
            .Where(p => p.Tranzactie.Fel == N.FelTranzactie.Deschidere).OrderBy(p => p.ID)
            .Select(p => new { p.ID, p.Data, p.Cont, p.Carte, p.Latura, p.Valoare, p.Cantitate,
                p.Unitate, p.Partener, p.Gestiune, p.DocumentId, p.LinieId }).ToArray()));
        var initial = Original();
        Refuza("SC-DES-09", () => Stinge(Partida(ref1, Furnizor), 20, Ianuarie.AddDays(1)), C.Materializare.StingereDeschidereInvalida);
        Verifica("SC-DES-09", "data dintre deschidere și plată nu scrie transfer", !CuSpatiu(os => os.GetObjectsQuery<C.Postare>()
            .Any(p => p.DocumentId == plata.Id && p.Tranzactie.Fel == N.FelTranzactie.Transfer)));
        Stinge(Partida(ref1, Furnizor), 20, dataPlata);
        using (var os = Deschide()) {
            var r = os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == plata.Id && p.Tranzactie.Fel == N.FelTranzactie.Transfer).ToList();
            Verifica("SC-DES-03", "transfer exact pe debit: partidă inițială +20, partidă plată -20", r.Count == 2
                && r.All(p => p.Cont == contTert && p.Partener == Furnizor && p.Carte == N.Carte.Contabil && p.Latura == N.Latura.Debit)
                && r.Single(p => p.Valoare == 20).Unitate == Partida(ref1, Furnizor)
                && r.Single(p => p.Valoare == -20).Unitate != Partida(ref1, Furnizor));
        }
        Rest("SC-DES-03", ref1, Furnizor, 40); Rest("SC-DES-03", ref2, Furnizor, 40); Rest("SC-DES-03", ref1, Client, 50);
        PlataPePartidaInitiala(plata.Id, 1, 20);
        Refuza("SC-DES-09", () => Stinge(Partida(ref2, Furnizor), 1, dataPlata), C.Materializare.StingereDeschidereInvalida);
        ReturFaraPretDeIntrare();
        InchideIanuarie();
        Refuza("SC-DES-09", () => Stinge(Partida(ref2, Furnizor), 1, dataPlata), "închis");
        Storneaza(plata.Id, Februarie);
        Rest("SC-DES-03", ref1, Furnizor, 40); Rest("SC-DES-03", ref1, Furnizor, 60, Februarie);
        PlataPePartidaInitiala(plata.Id, 2, -20);
        Verifica("SC-DES-03", "originalul deschiderii păstrat", initial == Original());
    }


    // D9-A6 (I7): prețul de intrare al lotului inițial e ce a pus creatorul lotului.
    void ReturFaraPretDeIntrare() {
        if (!Privat) return;
        var lot = loturi[1].Lot;
        var pret = CuSpatiu(os => os.GetObjectByKey<Lot>(lot).PretUnitar);
        Comanda(os => { os.GetObjectByKey<Lot>(lot).PretUnitar = 0; os.CommitChanges(); });
        try {
            var retur = CuSpatiu(os => {
                var d = os.CreateObject<ReturFurnizor>(); d.Data = Ianuarie; d.PredatorId = Magazie; d.PrimitorId = Furnizor;
                var l = os.CreateObject<DocumentDetaliu>(); l.Document = d; l.Pozitie = 1;
                l.TipMaterialId = Tip(os, Stoc); l.LotId = lot; l.Cantitate = 1;
                os.CommitChanges(); return d.ID;
            });
            Opereaza(retur);
            ValoriLinii("SC-DES-22", "retur 1 din lotul inițial 3/60 fără preț de intrare", retur, 0);
            Verifica("SC-DES-22", "returul scoate cantitatea cu valoare zero; lotul rămâne 2/60", CuSpatiu(os => {
                var p = os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == retur && p.Unitate == lot).ToList();
                return p.Count == 1 && p[0].Cantitate == -1 && p[0].Valoare == 0;
            }));
            SoldLot("SC-DES-22", lot, Magazie, Ianuarie, 2, 60);
            Anuleaza(retur);
            SoldLot("SC-DES-22", lot, Magazie, Ianuarie, 3, 60);
        }
        finally { Comanda(os => { os.GetObjectByKey<Lot>(lot).PretUnitar = pret; os.CommitChanges(); }); }
    }

    void DeschidereInSaftS() {
        if (!Privat) return;
        var (s, l, deschidere) = CuSpatiu(os => (SaftProiectii.SaftStocuriPeCub(os, An, 1), SaftProiectii.SaftPeCub(os, An, 1),
            os.GetObjectsQuery<C.Tranzactie>().Single(t => t.Fel == N.FelTranzactie.Deschidere).ID));
        var jurnal = l.Jurnale.Single(j => j.JournalID == SaftProiectii.JurnalDeschidere).Tranzactii.SelectMany(t => t.Linii)
            .Where(x => x.AccountID == Stoc && x.DebitCreditIndicator == "D").Sum(x => x.Amount);
        Verifica("SC-SAFT-45", "deschiderea din lună intră în Opening-ul fiecărui lot, nu e mișcare; Opening pe 302 = jurnalul DESCHIDERE din GL",
            loturi.All(x => s.StocFizic.SingleOrDefault(e => e.LotId == x.Lot && e.RepartitorId == Magazie) is { } e
                && (e.OpeningQuantity, e.OpeningValue, e.ClosingQuantity, e.ClosingValue) == (x.Cantitate, x.Valoare, x.Cantitate, x.Valoare))
            && s.MiscariStoc.All(m => m.TranzactieId != deschidere) && s.Rezumat.StocIntrariDiferite == 0
            && jurnal == loturi.Sum(x => x.Valoare)
            && s.StocFizic.Where(e => loturi.Any(x => x.Lot == e.LotId)).Sum(e => e.OpeningValue) == jurnal);
    }

    void PlataPePartidaInitiala(Guid plata, int luna, decimal suma) {
        if (!Privat) return;
        var saft = CuSpatiu(os => SaftProiectii.SaftPeCub(os, An, luna));
        var p = saft.Plati.Where(x => x.DocumentId == plata).ToList();
        Verifica("SC-SAFT-30", $"luna {luna}: plata pe partida inițială are o linie D {suma} fără SourceDocumentID și avertismentul PlataPePartidaInitiala",
            p.Count == 1 && p[0].Linii.Count == 1
            && p[0].Linii[0] is { SourceDocumentID: null, TintaDocumentId: null, DebitCreditIndicator: "D" } l && l.PaymentLineAmount == suma
            && saft.Avertismente.Any(a => a.Cod == nameof(CodAvertismentSaft.PlataPePartidaInitiala)));
    }

    void ReviewStingeri() {
        Guid Nota(DateOnly? data = null) => CuSpatiu(os => {
            var d = os.CreateObject<NotaContabila>(); d.Data = data ?? Ianuarie; d.DataInregistrare = d.Data;
            d.PredatorId = Loc; d.PrimitorId = Loc;
            var l = os.CreateObject<NotaContabilaDetaliu>(); l.Document = d; l.TipMaterialId = Tip(os, "TRZ");
            l.ContDebitId = ancora; l.ContCreditId = contTert; l.RepartitorCreditId = Furnizor;
            l.Valoare = 20; l.CodEconomicId = Economic;
            os.CommitChanges(); return d.ID;
        });
        var nota = Nota(); Opereaza(nota);
        var plata = Trezorerie(false, 20); Opereaza(plata.Id);
        Imperecheaza(nota, plata.Id, 20, Ianuarie.AddDays(20));
        VerificaCitiri(plata.Id, 0, "SC-DES-11");
        Refuza("SC-DES-11", () => Comanda(os => {
            using var tx = TranzactieComanda.Incepe(os);
            C.Materializare.Imperecheaza(os, os.GetObjectByKey<Document>(plata.Id), Partida(ref1, Furnizor), 20, Ianuarie);
        }), "STINGERE_DESCHIDERE_INVALIDA");
        Rest("SC-DES-11", ref1, Furnizor, 60);
        var altaNota = Nota(Ianuarie.AddDays(20)); Opereaza(altaNota);
        Refuza("SC-DES-11", () => Comanda(os => { using var tx = TranzactieComanda.Incepe(os);
            C.Materializare.Imperecheaza(os, os.GetObjectByKey<Document>(plata.Id), os.GetObjectByKey<Document>(altaNota), 20, Ianuarie.AddDays(20));
        }), C.Transferuri.PartidaProprieInsuficienta);
        var alta = Trezorerie(false, 20); Opereaza(alta.Id);
        var data = Ianuarie.AddDays(20);
        foreach (var rest in new[] { 10, 0 }) {
            Comanda(os => { using var tx = TranzactieComanda.Incepe(os);
                C.Materializare.Imperecheaza(os, os.GetObjectByKey<Document>(alta.Id), Partida(ref2, Furnizor), 10, data);
                os.CommitChanges(); tx.Commit(); });
            VerificaCitiri(alta.Id, rest, "SC-DES-12");
        }
        Verifica("SC-DES-12", "stingerea inițială consumă și restul de domeniu al plății", CuSpatiu(os => ImperechereService.Ramas(os, alta.Id)) == 0);
        using (var os = Deschide()) {
            decimal RestLa(DateOnly zi) => Atlas.Conta.BackOffice.Module.Proiectii.ImperecheriProiectii.DocumenteCuRest(os, laData: zi)
                .Where(p => p.DocumentId == alta.Id).Select(p => (decimal?)p.Rest).SingleOrDefault() ?? 0;
            Verifica("SC-DES-12", "stingerea viitoare nu intră în listă", RestLa(data.AddDays(-1)) == 20);
            Verifica("SC-DES-12", "lista include stingerea la data ei", RestLa(data) == 0);
        }
        Refuza("SC-DES-12", () => Imperecheaza(altaNota, alta.Id, 20, data), "rest");
        Refuza("SC-DES-12", () => Comanda(os => { using var tx = TranzactieComanda.Incepe(os);
            C.Materializare.Imperecheaza(os, os.GetObjectByKey<Document>(altaNota), os.GetObjectByKey<Document>(alta.Id), 20, data);
        }), C.Transferuri.PartidaProprieInsuficienta);
        var stamp = Amprenta(alta.Id);
        Refuza("SC-DES-13", () => Comanda(os => { using var tx = TranzactieComanda.Incepe(os);
            MotorOperare.AnuleazaOperarea(os, os.GetObjectByKey<Document>(alta.Id)); }), "stingere");
        Refuza("SC-DES-14", () => Comanda(os => { using var tx = TranzactieComanda.Incepe(os);
            MotorOperare.Storneaza(os, os.GetObjectByKey<Document>(alta.Id), Ianuarie.AddDays(5)); }), "stingerii");
        Verifica("SC-DES-13/14", "refuzurile păstrează plata și transferul", stamp == Amprenta(alta.Id));
        Storneaza(alta.Id, data);
        Rest("SC-DES-14", ref2, Furnizor, 40);
        Verifica("SC-DES-14", "inversa eliberează atribuirea în citirea din cub", CuSpatiu(os => ImperechereService.Asignat(os, alta.Id)) == 0);
    }

    void VerificaCitiri(Guid document, decimal rest, string id) {
        using var os = Deschide();
        var detaliu = ImperechereService.Ramas(os, document);
        var lista = Atlas.Conta.BackOffice.Module.Proiectii.ImperecheriProiectii.DocumenteCuRest(os, laData: new DateOnly(An, 1, 31))
            .Where(p => p.DocumentId == document).Select(p => (decimal?)p.Rest).SingleOrDefault() ?? 0;
        using var tx = TranzactieComanda.Incepe(os);
        SolduriService.MaterializeazaPartide(os, An, 1);
        var sold = os.GetObjectsQuery<PartidaDeschisa>().Where(p => p.DocumentId == document && p.An == An && p.Luna == 1)
            .Select(p => (decimal?)p.Rest).SingleOrDefault() ?? 0;
        Verifica(id, "detaliu = listă = snapshot, inclusiv inversa", detaliu == rest && lista == rest && sold == rest);
    }

    void ReviewConcurenta() {
        foreach (var anulare in new[] { false, true }) {
            var plata = Trezorerie(false, 20); Opereaza(plata.Id);
            Task<string> concurent;
            using (var os = Deschide()) using (var tx = TranzactieComanda.Incepe(os)) {
                C.Materializare.Imperecheaza(os, os.GetObjectByKey<Document>(plata.Id), Partida(ref2, Furnizor), 20, Ianuarie);
                os.CommitChanges();
                concurent = Task.Run(() => {
                    try { Comanda(alt => { ((EFCoreObjectSpace)alt).DbContext.Database.SetCommandTimeout(10);
                        if (anulare) ComenziDocument.Sistem(alt).AnuleazaOperarea(plata.Id);
                        else ComenziDocument.Sistem(alt).Storneaza(plata.Id, Ianuarie); }); return ""; }
                    catch (OperareException e) { return e.Message; }
                });
                AsteaptaBlocare(os, concurent);
                tx.Commit();
            }
            var rezultat = concurent.GetAwaiter().GetResult();
            Verifica("SC-DES-21", anulare ? "anularea concurentă refuză stingerea comisă" : "storno concurent include transferul comis",
                anulare ? rezultat.Contains("stingere") : rezultat == "");
            if (anulare) Storneaza(plata.Id, Ianuarie);
            Rest("SC-DES-21", ref2, Furnizor, 40);
        }
        var alta = Trezorerie(false, 20); Opereaza(alta.Id);
        Task<string> stingere;
        using (var os = Deschide()) using (var tx = TranzactieComanda.Incepe(os)) {
            MotorOperare.Storneaza(os, os.GetObjectByKey<Document>(alta.Id), Ianuarie);
            stingere = Task.Run(() => {
                try { Comanda(alt => { using var t = TranzactieComanda.Incepe(alt);
                    ((EFCoreObjectSpace)alt).DbContext.Database.SetCommandTimeout(10);
                    C.Materializare.Imperecheaza(alt, alt.GetObjectByKey<Document>(alta.Id), Partida(ref2, Furnizor), 20, Ianuarie);
                    alt.CommitChanges(); t.Commit(); }); return ""; }
                catch (OperareException e) { return e.Message; }
            });
            AsteaptaBlocare(os, stingere);
            tx.Commit();
        }
        Verifica("SC-DES-21", "stingerea recitește starea după storno concurent", stingere.GetAwaiter().GetResult().Contains(C.Materializare.StingereDeschidereInvalida));
        Rest("SC-DES-21", ref2, Furnizor, 40);
    }

    void AsteaptaBlocare(IObjectSpace os, Task concurent, string id = "SC-DES-21") {
        var db = ((EFCoreObjectSpace)os).DbContext;
        var pid = ((NpgsqlConnection)db.Database.GetDbConnection()).ProcessID;
        string interogare = null;
        var asteptat = System.Diagnostics.Stopwatch.StartNew();
        while (asteptat.Elapsed < TimeSpan.FromSeconds(5) && !concurent.IsCompleted) {
            db.Database.ExecuteSqlRaw("SELECT pg_stat_clear_snapshot()");
            interogare = db.Database.SqlQuery<string>($"""
                SELECT query AS "Value" FROM pg_stat_activity WHERE {pid} = ANY(pg_blocking_pids(pid))
                """).FirstOrDefault();
            if (interogare != null) break;
            Thread.Sleep(20);
        }
        Verifica(id, $"comanda concurentă așteaptă blocajul scrierii înaintea citirii cubului ({interogare ?? "fără blocare observată"})",
            interogare == TranzactieComanda.BlocajScriere);
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
        Refuz("stoc fără marcaj", [solduri[0] with { Detaliat = false }, .. solduri.Skip(1)], [], null, C.Materializare.DeschidereDiferenta);
        Refuz("fără control", solduri.Skip(2).ToArray(), null, null, C.Materializare.DeschidereInvalida);
        Refuz("cont", [solduri[0] with { Cont = Guid.NewGuid() }, .. solduri.Skip(1)], null, null, C.Materializare.DeschidereInvalida);
        foreach (var l in new[] { loturi[0] with { Lot = Guid.NewGuid() }, loturi[0] with { Gestiune = Guid.NewGuid() },
            loturi[0] with { Cont = ancora }, loturi[0] with { Cantitate = -1 }, loturi[0] with { Valoare = -1 } })
            Refuz("lot", null, [l, loturi[1]], null, C.Materializare.DeschidereInvalida);
        Refuz("partener", null, null, [partide[0] with { Partener = Magazie }, .. partide.Skip(1)], C.Materializare.DeschidereInvalida);
        Refuz("coordonate pe control detaliat", [solduri[0] with { Valuta = Guid.NewGuid() }, .. solduri.Skip(1)], null, null, "totalul de control");
        Refuz("valută absentă", null, null, [partide[0] with { ValoareValuta = 20 }, .. partide.Skip(1)], C.Materializare.DeschidereInvalida);
        Refuz("referință", null, null, [partide[0] with { Referinta = Guid.Empty }, .. partide.Skip(1)], C.Materializare.DeschidereInvalida);
        Refuz("carte", [solduri[0] with { Carte = N.Carte.Fiscal }, .. solduri.Skip(1)], null, null, "nu sunt echilibrate");
        Comanda(os => { os.GetObjectByKey<Lot>(loturi[0].Lot).Data = Ianuarie.AddDays(1); os.CommitChanges(); });
        Refuz("dată lot", null, null, null, C.Materializare.DeschidereInvalida);
        Comanda(os => { os.GetObjectByKey<Lot>(loturi[0].Lot).Data = Ianuarie; os.CommitChanges(); });
    }

    void ReviewIntrare() {
        var plata = Trezorerie(false, 20); Opereaza(plata.Id);
        using (var os = Deschide()) using (var tx = TranzactieComanda.Incepe(os)) {
            var analiza = new N.Analiza(null, Economic, null, null, null, null);
            var valuta = Guid.NewGuid();
            Scrie(os, [new(contTert, N.Latura.Credit, 100), new(ancora, N.Latura.Debit, 100)], [],
                [new(contTert, Furnizor, ref1, N.Latura.Credit, 100) { Analiza = analiza, Valuta = valuta, ValoareValuta = 20 }]);
            var p = os.ModifiedObjects.OfType<C.Postare>().Single(p => p.Cont == contTert);
            Verifica("SC-DES-15", "partida păstrează analiza și 100 lei / 20 în valută, curs 5", p.CodEconomic == Economic
                && p.Valuta == valuta && p.ValoareValuta == 20 && p.Valoare == 100
                && C.Randuri.Citeste(p).Coordonate.Unitate.Raport(new N.Sold(0, 100, 0, -20)) == 5);
            os.CommitChanges();
            Refuza("SC-DES-15", () => C.Materializare.Imperecheaza(os, os.GetObjectByKey<Document>(plata.Id),
                Partida(ref1, Furnizor), 20, Ianuarie), "în valută");
            Verifica("SC-DES-15", "stingerea neacoperită nu adaugă transfer", !os.ModifiedObjects.OfType<C.Postare>().Any());
            var faraNavigatie = new C.Postare { ID = Guid.NewGuid() };
            try { C.Randuri.Citeste(faraNavigatie); Verifica("SC-DES-16", "lipsa navigației refuzată explicit", false); }
            catch (InvalidOperationException e) { Verifica("SC-DES-16", "lipsa navigației refuzată explicit", e.Message.Contains("fără document")); }
        }
        Storneaza(plata.Id, Ianuarie);
        using (var os = Deschide()) using (var tx = TranzactieComanda.Incepe(os)) {
            Refuza("SC-DES-17", () => Scrie(os,
                [new(contTert, N.Latura.Credit, 100, Carte: N.Carte.Fiscal), new(ancora, N.Latura.Debit, 100, Carte: N.Carte.Fiscal)], [],
                [new(contTert, Furnizor, ref1, N.Latura.Credit, 100, N.Carte.Fiscal)]), C.Materializare.DeschidereInvalida);
        }
        var flags = CuSpatiu(os => os.GetObjectByKey<Cont>(contTert).DimensiuniObligatorii);
        try {
            Comanda(os => { os.GetObjectByKey<Cont>(contTert).DimensiuniObligatorii = DimensiuneFlags.CodEconomic; os.CommitChanges(); });
            using var os = Deschide(); using var tx = TranzactieComanda.Incepe(os);
            Refuza("SC-DES-18", () => Scrie(os), "Cod economic");
            Verifica("SC-DES-18", "dimensiunea lipsă refuzată înaintea scrierii", !os.ModifiedObjects.OfType<C.Postare>().Any());
        } finally { Comanda(os => { os.GetObjectByKey<Cont>(contTert).DimensiuniObligatorii = flags; os.CommitChanges(); }); }
        var receptie = Receptioneaza(new LinieFctScena(5, 10));
        using (var os = Deschide()) using (var tx = TranzactieComanda.Incepe(os)) {
            Refuza("SC-DES-19", () => C.Materializare.Deschide(os, Ianuarie.AddDays(1), solduri, loturi, partide), "istoriei existente");
            var l = receptie.Linii[0];
            Refuza("SC-DES-20", () => Scrie(os, [new(Cont(Stoc), N.Latura.Debit, 50), new(ancora, N.Latura.Credit, 50)],
                [new(Cont(Stoc), l.Lot.Value, Magazie, 5, 50)], []), "deja mișcări");
        }
        var lotNou = Lot(5, 50);
        Comanda(os => { os.GetObjectByKey<Lot>(lotNou.Lot).LinieIntrareId = receptie.Linii[0].Id; os.CommitChanges(); });
        using (var os = Deschide()) using (var tx = TranzactieComanda.Incepe(os)) {
            Refuza("SC-DES-20", () => Scrie(os, [new(Cont(Stoc), N.Latura.Debit, 50), new(ancora, N.Latura.Credit, 50)], [lotNou], []), "Lot, cont");
        }
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
