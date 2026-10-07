using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Atlas.Conta.BackOffice.ModelCheck;
using Atlas.Conta.BackOffice.Module.Anaf;
using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Asm;
using Atlas.Conta.BackOffice.Module.Api.Bcs;
using Atlas.Conta.BackOffice.Module.Api.Btr;
using Atlas.Conta.BackOffice.Module.Api.Dec;
using Atlas.Conta.BackOffice.Module.Api.Dsc;
using Atlas.Conta.BackOffice.Module.Api.Dvi;
using Atlas.Conta.BackOffice.Module.Api.Fcl;
using Atlas.Conta.BackOffice.Module.Api.Amo;
using Atlas.Conta.BackOffice.Module.Api.Cas;
using Atlas.Conta.BackOffice.Module.Api.Fct;
using Atlas.Conta.BackOffice.Module.Api.Imo;
using Atlas.Conta.BackOffice.Module.Api.Itv;
using Atlas.Conta.BackOffice.Module.Api.Ldi;
using Atlas.Conta.BackOffice.Module.Api.Nir;
using Atlas.Conta.BackOffice.Module.Api.Ntc;
using Atlas.Conta.BackOffice.Module.Api.Perioade;
using Atlas.Conta.BackOffice.Module.Api.Pif;
using Atlas.Conta.BackOffice.Module.Api.Politici;
using Atlas.Conta.BackOffice.Module.Api.Rdc;
using Atlas.Conta.BackOffice.Module.Api.Rlf;
using Atlas.Conta.BackOffice.Module.Api.Trz;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.DatabaseUpdate;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Proiectii;
using Atlas.Conta.BackOffice.Module.Saft;
using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.EFCore;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DevExtreme.AspNet.Data;
using Microsoft.EntityFrameworkCore;
using SecurityPermissionPolicy = DevExpress.Persistent.Base.SecurityPermissionPolicy;
using SecurityPermissionState = DevExpress.Persistent.Base.SecurityPermissionState;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

// Felia 27, pasul 2a — soldurile materializate la închidere (`SOL-V*`) și cursa
// închidere ↔ operare pe calea REALĂ a comenzilor (`PER-C*`, forma F1 din spike
// A.0/A.2). Scena stă în 2031–2032, în afara tuturor celorlalte scene ale suitei.
static class VerificaSolduriPerioada {
    public static void Ruleaza(Suita s, bool privat) {
        const string Marcaj = "E2E-SOL";
        const int An = 2031;
        var eticheta = privat ? "privat" : "bugetar";
        // Tipul de material cu natura Stoc: planul bugetar n-are „371”.
        var codTipStoc = privat ? "371" : "302.01.00";
        DateOnly Zi(int luna, int zi) => new(An, luna, zi);
        DateOnly Ultima(int an, int luna) => new(an, luna, DateTime.DaysInMonth(an, luna));

        // ─────────── curățenia de scenă (purjă FIZICĂ, F13-D2) ───────────
        void CurataSol(IObjectSpace os) {
            // Snapshot-urile ÎNTÂI: FK-urile lor spre lot/repartitor/cont sunt
            // `Restrict`, deci un rând rămas ar bloca purja nomenclatoarelor.
            for (var luna = 1; luna <= 12; luna++) {
                SolduriService.Elimina(os, An, luna);
                SolduriService.Elimina(os, An + 1, luna);
            }
            var pj = new Purja(os);
            var docIds = os.GetObjectsQuery<Document>()
                .Where(d => d.Data >= new DateOnly(An, 1, 1) && d.Data <= new DateOnly(An + 1, 12, 31))
                .Select(d => d.ID).ToList();
            var produsIds = os.GetObjectsQuery<Produs>()
                .Where(p => p.Cod.StartsWith(Marcaj)).Select(p => p.ID).ToList();
            var lotIds = os.GetObjectsQuery<Lot>()
                .Where(l => produsIds.Contains(l.ProdusId)).Select(l => l.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>()
                .Where(d => docIds.Contains(d.DocumentId)).ToList());
            foreach (var doc in os.GetObjectsQuery<Document>()
                    .Where(d => docIds.Contains(d.ID)).OrderByDescending(d => d.DocumentSursaId != null))
                pj.Adauga(doc);
            pj.Adauga(os.GetObjectsQuery<Lot>()
                .Where(l => lotIds.Contains(l.ID)).ToList());
            pj.Adauga(os.GetObjectsQuery<Produs>()
                .Where(p => produsIds.Contains(p.ID)).ToList());
            var perioadeIds = os.GetObjectsQuery<PerioadaFiscala>()
                .Where(p => p.An == An || p.An == An + 1).Select(p => p.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<InchiderePerioada>()
                .Where(i => perioadeIds.Contains(i.PerioadaId)).ToList());
            pj.Adauga(os.GetObjectsQuery<PerioadaFiscala>()
                .Where(p => p.An == An || p.An == An + 1).ToList());
            pj.Adauga(os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(Marcaj)).ToList());
            pj.Adauga(os.GetObjectsQuery<CodEconomic>()
                .Where(c => c.Cod.StartsWith(Marcaj)).ToList());
            pj.Executa();
        }

        // ─────────── citirea snapshot-ului și recalculul de control ───────────
        (Guid, Guid?, Guid?, Guid?, Guid?, Guid?, Guid?, Guid?, Guid?, Guid?) CheieAtom(AtomContabil a) =>
            (a.ContId, a.RepartitorId, a.MaterialId, a.CodFunctionalId, a.CodEconomicId,
             a.SursaFinantareId, a.UnitateId, a.ProiectId, a.CentruCostId, a.GestiuneId);

        Dictionary<(Guid, Guid?, Guid?, Guid?, Guid?, Guid?, Guid?, Guid?, Guid?, Guid?), (decimal D, decimal C)>
                SnapshotContabil(IObjectSpace os, int an, int luna) =>
            os.GetObjectsQuery<SoldPerioadaContabil>().Where(s => s.An == an && s.Luna == luna)
                .Select(s => new {
                    s.ContId, s.RepartitorId, s.MaterialId, s.CodFunctionalId, s.CodEconomicId,
                    s.SursaFinantareId, s.UnitateId, s.ProiectId, s.CentruCostId, s.GestiuneId, s.Debit, s.Credit
                })
                .ToList()
                .ToDictionary(
                    s => (s.ContId, s.RepartitorId, s.MaterialId, s.CodFunctionalId, s.CodEconomicId,
                          s.SursaFinantareId, s.UnitateId, s.ProiectId, s.CentruCostId, s.GestiuneId),
                    s => (s.Debit, s.Credit));

        // Recalculul de control e LINQ pe `ContabilProiectii.Atomi`, nu SQL: dacă
        // ambele căi ar fi scrise la fel, proba n-ar mai proba nimic.
        Dictionary<(Guid, Guid?, Guid?, Guid?, Guid?, Guid?, Guid?, Guid?, Guid?, Guid?), (decimal D, decimal C)>
                AsteptatContabil(IObjectSpace os, DateOnly panaLa) =>
            ContabilProiectii.Atomi(os).Where(a => a.Data <= panaLa).ToList()
                .GroupBy(CheieAtom)
                .Select(g => new { g.Key, D = g.Sum(a => a.Debit), C = g.Sum(a => a.Credit) })
                .Where(x => x.D != 0m || x.C != 0m)
                .ToDictionary(x => x.Key, x => (x.D, x.C));

        Dictionary<(Guid LotId, Guid ContId, Guid ProdusId, Guid GestiuneId, DateOnly Deschisa), (decimal Cantitate, decimal Valoare)> SnapshotStoc(IObjectSpace os, int an, int luna) =>
            os.GetObjectsQuery<SoldPerioadaStoc>().Where(s => s.An == an && s.Luna == luna)
                .ToList().ToDictionary(s => (s.LotId, s.ContId, s.ProdusId, s.GestiuneId, s.Deschisa),
                    s => (s.Cantitate, s.Valoare));

        Dictionary<(Guid LotId, Guid ContId, Guid ProdusId, Guid GestiuneId, DateOnly Deschisa), (decimal Cantitate, decimal Valoare)> AsteptatStoc(IObjectSpace os, DateOnly panaLa) =>
            os.GetObjectsQuery<Atlas.Conta.BackOffice.Module.Cub.Postare>()
                .Where(p => p.Carte == Atlas.Conta.Nucleu.Carte.Contabil
                    && p.FelUnitate == Atlas.Conta.Nucleu.FelUnitate.Lot && p.Unitate != null
                    && p.Produs != null && p.Gestiune != null && p.Data <= panaLa)
                .ToList().GroupBy(p => (p.Unitate.Value, p.Cont, p.Produs.Value, p.Gestiune.Value))
                .Select(g => new { g.Key, Deschisa = g.Min(p => p.UnitateDeschisa ?? p.Data),
                    Cantitate = g.Sum(p => p.Cantitate),
                    Valoare = g.Sum(p => p.Latura == Atlas.Conta.Nucleu.Latura.Debit ? p.Valoare : -p.Valoare) })
                .Where(s => s.Cantitate != 0m || s.Valoare != 0m)
                .ToDictionary(s => (s.Key.Item1, s.Key.Item2, s.Key.Item3, s.Key.Item4, s.Deschisa),
                    s => (s.Cantitate, s.Valoare));

        bool EgalContabil(IObjectSpace os, int an, int luna) {
            var snap = SnapshotContabil(os, an, luna);
            var asteptat = AsteptatContabil(os, Ultima(an, luna));
            return snap.Count == asteptat.Count
                && snap.All(kv => asteptat.TryGetValue(kv.Key, out var a) && a == kv.Value)
                && asteptat.All(kv => snap.ContainsKey(kv.Key));
        }

        bool EgalStoc(IObjectSpace os, int an, int luna) {
            var snap = SnapshotStoc(os, an, luna);
            var asteptat = AsteptatStoc(os, Ultima(an, luna));
            return snap.Count == asteptat.Count
                && snap.All(kv => asteptat.TryGetValue(kv.Key, out var a) && a == kv.Value)
                && asteptat.All(kv => snap.ContainsKey(kv.Key));
        }

        int RanduriSnapshot(IObjectSpace os, int an, int luna) =>
            os.GetObjectsQuery<SoldPerioadaContabil>().Count(s => s.An == an && s.Luna == luna)
            + os.GetObjectsQuery<SoldPerioadaStoc>().Count(s => s.An == an && s.Luna == luna);

        string Referinte(IObjectSpace os) =>
            string.Join(", ", SolduriService.Referinte(os).Select(r => $"{r.Luna:00}/{r.An}"));

        // ─────────── cursa: a doua conexiune, `lock_timeout` pe comandă ───────────
        string CodPostgres(Exception e) {
            for (var ex = e; ex != null; ex = ex.InnerException)
                if (ex is Npgsql.PostgresException pg)
                    return pg.SqlState;
            return null;
        }
        // `null` = comanda a trecut; altfel SQLSTATE-ul (55P03 = lock_timeout).
        string CuLockTimeout(Action<IObjectSpace> comanda) {
            using var os = s.Provider.CreateObjectSpace();
            var db = ((EFCoreObjectSpace)os).DbContext.Database;
            db.OpenConnection();
            try {
                db.ExecuteSqlRaw("SET lock_timeout = '1500ms'");
                comanda(os);
                return null;
            }
            catch (Exception e) {
                return CodPostgres(e) ?? e.GetType().Name;
            }
            finally {
                db.CloseConnection();
            }
        }

        // ═════════════════════ precondiția ═════════════════════
        using (var os = s.Provider.CreateObjectSpace())
            CurataSol(os);

        using (var os = s.Provider.CreateObjectSpace()) {
            var perioade = os.GetObjectsQuery<PerioadaFiscala>().Count(p => p.An == An || p.An == An + 1);
            var inchise = os.GetObjectsQuery<PerioadaFiscala>().Count(p => p.Inchisa);
            var snapshoturi = os.GetObjectsQuery<SoldPerioadaContabil>().Count()
                + os.GetObjectsQuery<SoldPerioadaStoc>().Count();
            Console.WriteLine($"     MĂSURAT (SOL-V0/{eticheta}): {perioade} perioade în {An}–{An + 1}, "
                + $"{inchise} perioade închise în bază, {snapshoturi} rânduri de snapshot.");
            s.Check($"SOL-V0 ({eticheta}) precondiție: {An}–{An + 1} sunt libere, NICIO perioadă a bazei nu e închisă "
                + "și nu există niciun snapshot — altfel mulțimea perioadelor DE REFERINȚĂ probată mai jos ar fi "
                + "măsurată peste conținut străin",
                perioade == 0 && inchise == 0 && snapshoturi == 0);
        }

        // ═════════════════════ scena ═════════════════════
        Guid idGestA, idGestB, idFurnizor, idLot1, idLot2, idBtrB, idCodEc;
        using (var os = s.Provider.CreateObjectSpace()) {
            for (var luna = 1; luna <= 12; luna++) {
                var p = os.CreateObject<PerioadaFiscala>();
                p.An = An;
                p.Luna = luna;
            }
            var tipMat = os.FirstOrDefault<TipMaterial>(t => t.Cod == codTipStoc);
            var gestA = os.CreateObject<Gestiune>();
            gestA.Cod = Marcaj + "-GA";
            gestA.Denumire = "Gestiune solduri A";
            var gestB = os.CreateObject<Gestiune>();
            gestB.Cod = Marcaj + "-GB";
            gestB.Denumire = "Gestiune solduri B";
            var furnizor = os.CreateObject<Partener>();
            furnizor.Cod = Marcaj + "-F";
            furnizor.Denumire = "Furnizor solduri";
            furnizor.CodFiscal = "RO33333338";
            furnizor.InregistratTva = true;
            Produs Prod(string sufix) {
                var p = os.CreateObject<Produs>();
                p.Cod = Marcaj + sufix;
                p.Denumire = "Produs solduri" + sufix;
                p.UM = "BUC";
                p.TipMaterial = tipMat;
                return p;
            }
            var produs1 = Prod("-P1");
            var produs2 = Prod("-P2");
            // Dimensiunea CULEASĂ a scenei: pe profilul bugetar contul de furnizori
            // o cere (`VerificaDimensiuniObligatorii`), iar pe privat e opțională —
            // aceeași scenă, cheia snapshot-ului nenulă pe ambele profiluri.
            var codEc = os.CreateObject<CodEconomic>();
            codEc.Cod = Marcaj + "-CE";
            codEc.Denumire = "Cod economic solduri";
            os.CommitChanges();
            s.Check($"SOL — precondiție de profil ({eticheta}): tipul de material cu natura Stoc („{codTipStoc}”) e "
                + "în seed pe AMBELE profiluri, deci scena de stoc e aceeași",
                tipMat != null && tipMat.Clasa?.Natura == NaturaClasa.Stoc);

            // Ianuarie: NIR cu DOUĂ loturi în aceeași gestiune.
            var nir = os.CreateObject<NIR>();
            nir.Data = Zi(1, 5);
            nir.Predator = furnizor;
            nir.Primitor = gestA;
            var l1 = os.CreateObject<NirDetaliu>();
            l1.Document = nir; l1.TipMaterial = tipMat; l1.Cantitate = 10m; l1.PretUnitar = 10m;
            l1.CodEconomicId = codEc.ID;
            var lot1 = l1.CreeazaLot(os, produs1, gestA);
            var l2 = os.CreateObject<NirDetaliu>();
            l2.Document = nir; l2.TipMaterial = tipMat; l2.Cantitate = 20m; l2.PretUnitar = 15m;
            l2.CodEconomicId = codEc.ID;
            var lot2 = l2.CreeazaLot(os, produs2, gestA);
            os.CommitChanges();
            MotorOperare.Opereaza(os, nir);
            os.CommitChanges();

            idGestA = gestA.ID;
            idGestB = gestB.ID;
            idFurnizor = furnizor.ID;
            idLot1 = lot1.ID;
            idLot2 = lot2.ID;
            idCodEc = codEc.ID;
            idBtrB = Guid.Empty;
        }

        // ── PER-C1/PER-C2: cursa, înainte de orice închidere ──
        using (var os = s.Provider.CreateObjectSpace()) {
            var btrProba = os.CreateObject<NotaTransfer>();
            btrProba.Data = Zi(1, 20);
            btrProba.PredatorId = idGestA;
            btrProba.PrimitorId = idGestB;
            btrProba.NumarPV = Marcaj + "-C1";
            var linie = os.CreateObject<DocumentDetaliu>();
            linie.Document = btrProba;
            linie.TipMaterial = os.GetObjectByKey<Lot>(idLot2).Produs.TipMaterial;
            linie.LotId = idLot2;
            linie.Cantitate = 1m;
            os.CommitChanges();
            var idProba = btrProba.ID;

            using (var externa = new Npgsql.NpgsqlConnection(s.ConnectionString)) {
                externa.Open();
                using var txExterna = externa.BeginTransaction();
                using (var cmd = externa.CreateCommand()) {
                    cmd.CommandText = "SELECT \"ID\" FROM \"PerioadeFiscale\" "
                        + $"WHERE \"An\" = {An} AND \"Luna\" = 1 FOR UPDATE";
                    cmd.ExecuteNonQuery();
                }
                var refuz = CuLockTimeout(o => ComenziDocument.Sistem(o).Opereaza(idProba));
                Console.WriteLine($"     MĂSURAT (PER-C1/{eticheta}): operarea sub închidere „în curs” a ieșit cu "
                    + $"„{refuz ?? "<a trecut>"}”.");
                s.Check($"PER-C1 ({eticheta}) cu o închidere „în curs” care ține `FOR UPDATE` pe rândul perioadei, "
                    + "operarea AȘTEAPTĂ și cade pe `lock_timeout` (55P03) — gardianul citește prin `FOR SHARE` în "
                    + "tranzacția comenzii, deci documentul nu se poate strecura în perioada care se închide",
                    refuz == "55P03");
                txExterna.Rollback();
            }
            var dupa = CuLockTimeout(o => ComenziDocument.Sistem(o).Opereaza(idProba));
            s.Check($"PER-C2 ({eticheta}) după eliberarea lock-ului aceeași comandă TRECE — blocarea serializează "
                + "cursa, nu interzice operarea", dupa == null);

            using (var externa = new Npgsql.NpgsqlConnection(s.ConnectionString)) {
                externa.Open();
                using var txExterna = externa.BeginTransaction();
                using (var cmd = externa.CreateCommand()) {
                    cmd.CommandText = "SELECT \"ID\" FROM \"PerioadeFiscale\" "
                        + $"WHERE \"An\" = {An} AND \"Luna\" = 1 FOR SHARE";
                    cmd.ExecuteNonQuery();
                }
                var refuz = CuLockTimeout(o => PerioadaService.Inchide(o, An, 1, [], null, Marcaj));
                Console.WriteLine($"     MĂSURAT (PER-C3/{eticheta}): închiderea sub operare „în curs” a ieșit cu "
                    + $"„{refuz ?? "<a trecut>"}”.");
                s.Check($"PER-C3 ({eticheta}) simetric: cu o operare „în curs” care ține `FOR SHARE`, comanda de "
                    + "închidere cade pe `lock_timeout` la `FOR UPDATE` — `SUM`-ul soldurilor nu apucă să ruleze "
                    + "peste un registru care încă se scrie",
                    refuz == "55P03");
                txExterna.Rollback();
            }
            using (var o = s.Provider.CreateObjectSpace()) {
                s.Check($"PER-C4 ({eticheta}) închiderea eșuată pe lock NU a lăsat nimic în urmă: 01/{An} e tot "
                    + "deschisă, fără rând de istoric — tranzacția comenzii s-a anulat integral",
                    !o.FirstOrDefault<PerioadaFiscala>(p => p.An == An && p.Luna == 1).Inchisa
                    && !o.GetObjectsQuery<InchiderePerioada>().Any(i => i.De == Marcaj));
            }
        }

        // ── restul scenei: februarie (ieșiri + notă cu dimensiuni), martie (storno) ──
        using (var os = s.Provider.CreateObjectSpace()) {
            var lot1 = os.GetObjectByKey<Lot>(idLot1);
            var lot2 = os.GetObjectByKey<Lot>(idLot2);
            var tipMat = lot1.Produs.TipMaterial;
            var gestA = os.GetObjectByKey<Repartitor>(idGestA);
            var gestB = os.GetObjectByKey<Repartitor>(idGestB);

            // BTR-A: lotul 1 iese INTEGRAL din gestiunea A ⇒ cheia (lot1, A, tip)
            // ajunge la cantitate 0 ȘI valoare 0, deci trebuie să LIPSEASCĂ din snapshot.
            var btrA = os.CreateObject<NotaTransfer>();
            btrA.Data = Zi(2, 10);
            btrA.PredatorId = idGestA;
            btrA.PrimitorId = idGestB;
            btrA.NumarPV = Marcaj + "-A";
            var linA = os.CreateObject<DocumentDetaliu>();
            linA.Document = btrA; linA.TipMaterial = tipMat; linA.LotId = idLot1; linA.Cantitate = 10m;
            os.CommitChanges();
            MotorOperare.Opereaza(os, btrA);
            os.CommitChanges();

            // BTR-B: parțial pe lotul 2 — stornat în martie.
            var btrB = os.CreateObject<NotaTransfer>();
            btrB.Data = Zi(2, 20);
            btrB.PredatorId = idGestA;
            btrB.PrimitorId = idGestB;
            btrB.NumarPV = Marcaj + "-B";
            var linB = os.CreateObject<DocumentDetaliu>();
            linB.Document = btrB; linB.TipMaterial = tipMat; linB.LotId = idLot2; linB.Cantitate = 5m;
            os.CommitChanges();
            MotorOperare.Opereaza(os, btrB);
            os.CommitChanges();
            idBtrB = btrB.ID;

            // Notă contabilă cu dimensiuni pe AMBELE laturi: repartitori diferiți pe
            // debit și pe credit, plus dimensiunea culeasă a notei (cod economic).
            var dataNir = Zi(1, 5);
            var idNir = os.GetObjectsQuery<NIR>().Where(d => d.Data == dataNir && d.PredatorId == idFurnizor)
                .Select(d => d.ID).FirstOrDefault();
            var noteNir = CubScena.Note(os, idNir);
            if (noteNir.Any(p => p.Debit) && noteNir.Any(p => p.Credit)) {
                var ntc = os.CreateObject<NotaContabila>();
                ntc.Data = Zi(2, 25);
                ntc.PredatorId = idGestA;
                ntc.PrimitorId = idGestB;
                var linN = os.CreateObject<NotaContabilaDetaliu>();
                linN.Document = ntc;
                linN.TipMaterial = tipMat;
                linN.ContDebitId = noteNir.First(p => p.Debit).Cont;
                linN.ContCreditId = noteNir.First(p => p.Credit).Cont;
                linN.Valoare = 42.37m;
                linN.CodEconomicId = idCodEc;
                linN.RepartitorDebitId = idGestA;
                linN.RepartitorCreditId = idFurnizor;
                os.CommitChanges();
                var refuzNtc = s.Refuz(() => MotorOperare.Opereaza(os, ntc));
                if (refuzNtc != null) {
                    os.Delete(ntc.Detalii.ToList());
                    os.Delete(ntc);
                    os.CommitChanges();
                }
                Console.WriteLine($"     MĂSURAT (SOL/{eticheta}): nota cu dimensiuni pe ambele laturi — "
                    + $"{(refuzNtc == null ? "operată" : "refuzată: " + refuzNtc.Split('\n')[0])}.");
            }

            // Martie: storno-ul unui document din februarie, la data stornării.
            MotorOperare.Storneaza(os, os.GetObjectByKey<Document>(idBtrB), Zi(3, 10));
            os.CommitChanges();
        }

        // ═════ SOL-C: aceleași cifre cu și fără snapshot, consumator cu consumator ═════
        //
        // Fiecare consumator mutat pe `SolduriService` (F27-D3) se citește de DOUĂ
        // ori: cu scena DESCHISĂ (nicio perioadă închisă ⇒ calea de azi, integral
        // din registre) și după închideri (calea snapshot + rulaje). Rezultatele se
        // compară SERIALIZATE — la cent, la rând și la ordine; scala zecimală se
        // normalizează, ca o diferență de `numeric` să nu treacă drept diferență de
        // cifre. Comparația se face în DOUĂ momente: cu referința 01/{An} (fereastra
        // deschisă februarie–martie are rulaje reale) și cu referința 03/{An} (toată
        // scena e înăuntrul snapshot-ului).
        Guid idContStoc, idContTert;
        var ziNir = Zi(1, 5);
        using (var os = s.Provider.CreateObjectSpace()) {
            var idNir = os.GetObjectsQuery<NIR>().Where(d => d.Data == ziNir && d.PredatorId == idFurnizor)
                .Select(d => d.ID).First();
            var noteNir = CubScena.Note(os, idNir);
            idContStoc = noteNir.First(p => p.Debit).Cont;
            idContTert = noteNir.First(p => p.Credit).Cont;
        }
        Guid idProdus2;
        using (var os = s.Provider.CreateObjectSpace())
            idProdus2 = os.GetObjectByKey<Lot>(idLot2).ProdusId;

        string N(decimal v) => v.ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture);

        Dictionary<string, string> Citiri(IObjectSpace os) {
            var ds = Zi(2, 1);
            var de = Zi(3, 31);
            var c = new Dictionary<string, string>();
            string Bal(bool analitic, Guid? codEc = null) => string.Join("\n",
                ContabilProiectii.Balanta(os, ds, de, analitic, codEconomicId: codEc).ToList()
                    .OrderBy(r => r.ContSimbol ?? "", StringComparer.Ordinal).ThenBy(r => r.ContId)
                    .ThenBy(r => r.RepartitorId)
                    .Select(r => $"{r.ContSimbol}|{r.ContId}|{r.RepartitorId}|{N(r.InitialDebit)}|"
                        + $"{N(r.InitialCredit)}|{N(r.SoldInitialDebit)}|{N(r.SoldInitialCredit)}|"
                        + $"{N(r.RulajDebit)}|{N(r.RulajCredit)}|{N(r.SoldFinalDebit)}|{N(r.SoldFinalCredit)}"));
            c["balanta-sintetic"] = Bal(false);
            c["balanta-analitic"] = Bal(true);
            c["balanta-dimensiune"] = Bal(true, idCodEc);
            c["balanta-plan"] = string.Join("\n", ContabilProiectii.BalantaPlan(os, ds, de)
                .Select(r => $"{r.ContSimbol}|{r.ContId}|{r.ParinteId}|{r.Nivel}|{r.AreCopii}|"
                    + $"{r.AreMiscareProprie}|{N(r.InitialDebit)}|{N(r.InitialCredit)}|{N(r.RulajDebit)}|"
                    + $"{N(r.RulajCredit)}|{N(r.SoldFinalDebit)}|{N(r.SoldFinalCredit)}"));
            string Fisa(Guid contId, Guid? repartitor = null, bool faraRepartitor = false) => string.Join("\n",
                ContabilProiectii.FisaCont(os, contId, ds, de, repartitorId: repartitor,
                        repartitorNul: faraRepartitor).ToList()
                    .Select(r => $"{r.Id}|{r.Data:yyyy-MM-dd}|{r.Sens}|{N(r.Debit)}|{N(r.Credit)}|"
                        + $"{N(r.SoldCurent)}|{r.ContrapartidaSimbol}|{r.RepartitorDenumire}|"
                        + $"{r.DocumentId}|{r.Storno}"));
            c["fisa-cont-stoc"] = Fisa(idContStoc);
            c["fisa-cont-tert"] = Fisa(idContTert);
            // Filtrul de dimensiune trece PRIN rândul sintetic: snapshot-ul se
            // filtrează înăuntru, iar rândul poartă apoi coordonatele filtrului, ca
            // să treacă neatins prin filtrele nivelului (b). Ambele variante ale
            // santinelei: un repartitor anume și „fără repartitor".
            c["fisa-cont-filtrata"] = Fisa(idContTert, repartitor: idFurnizor);
            c["fisa-cont-fara-repartitor"] = Fisa(idContStoc, faraRepartitor: true);
            string Stoc(DateOnly? la) => string.Join("\n",
                StocProiectii.SoldStoc(os, la).ToList()
                    .OrderBy(r => r.LotId).ThenBy(r => r.RepartitorId)
                    .ThenBy(r => r.ContId).ThenBy(r => r.ProdusId)
                    .Select(r => $"{r.LotId}|{r.RepartitorId}|{r.ContId}|{r.ProdusId}|{N(r.Cantitate)}|{N(r.Valoare)}|"
                        + $"{r.ProdusCod}|{r.GestiuneDenumire}"));
            c["sold-stoc-azi"] = Stoc(null);
            c["sold-stoc-la-data"] = Stoc(Ultima(An, 2));
            // Lotul 1 a ieșit INTEGRAL din gestiunea A: cheia lui e cantitate 0 și
            // valoare 0, deci lipsește din listă pe AMBELE căi — din snapshot o taie
            // regula cheilor integral zero, din registru filtrul care o oglindește.
            c["sold-stoc-lot-golit"] = StocProiectii.SoldStoc(os).ToList()
                .Any(r => r.LotId == idLot1 && r.RepartitorId == idGestA) ? "PREZENT" : "absent";
            string Solduri(DateOnly la) => string.Join("\n",
                Atlas.Conta.BackOffice.Module.Cub.Citiri.Loturi.Cumulate(os, Atlas.Conta.BackOffice.Module.Cub.Citiri.CitireCumul.Integrala, la)
                    .Where(s => s.LotId == idLot1 || s.LotId == idLot2).ToList()
                    .OrderBy(s => s.LotId).ThenBy(s => s.GestiuneId).ThenBy(s => s.ContId)
                    .Select(s => $"{s.LotId}|{s.GestiuneId}|{s.ContId}|{N(s.Cantitate)}|{N(s.Valoare)}"));
            c["solduri-la-data"] = Solduri(Ultima(An, 2)) + "\n──\n" + Solduri(Ultima(An, 3));
            // Cheia golită INTEGRAL (lotul 1 din gestiunea A) și cea rămasă, plus o
            // dată istorică: „absentă din snapshot” trebuie să dea tot 0.
            decimal Sold(Guid lot, Guid gestiune, DateOnly? la = null) =>
                Atlas.Conta.BackOffice.Module.Cub.Citiri.Loturi.Cumulate(os, Atlas.Conta.BackOffice.Module.Cub.Citiri.CitireCumul.Integrala, la)
                    .Where(s => s.LotId == lot && s.GestiuneId == gestiune).ToList().Sum(s => s.Cantitate);
            c["sold-pe-cheie"] = N(Sold(idLot1, idGestA))
                + "|" + N(Sold(idLot2, idGestA))
                + "|" + N(Sold(idLot2, idGestA, Ultima(An, 2)))
                + "|" + N(Sold(idLot1, idGestB));

            var itv = InchidereTvaService.Solduri(os, idContStoc, idContTert, Ultima(An, 3));
            c["itv-solduri"] = $"{N(itv.Sold4426)}|{N(itv.Sold4427)}";
            // Gardianul 25d: o ieșire de aprilie PESTE sold trebuie refuzată cu EXACT
            // același text, iar una SUB sold trebuie să treacă — pe ambele căi.
            c["refuz-sold-negativ"] = CubScena.RefuzSold(os, idLot2, idGestA, idProdus2, idContStoc, Zi(4, 15), -50m) ?? "<a trecut>";
            c["sold-intermediar-sub-sold"] = CubScena.RefuzSold(os, idLot2, idGestA, idProdus2, idContStoc, Zi(4, 15), -3m) ?? "<a trecut>";
            return c;
        }

        string PrimaDiferenta(string a, string b) {
            var la = a.Split('\n');
            var lb = b.Split('\n');
            for (var i = 0; i < Math.Max(la.Length, lb.Length); i++) {
                var x = i < la.Length ? la[i] : "<lipsă>";
                var y = i < lb.Length ? lb[i] : "<lipsă>";
                if (x != y)
                    return $"rândul {i + 1}: fără snapshot „{x}” ≠ cu snapshot „{y}”";
            }
            return "<identice>";
        }

        Dictionary<string, string> citiriDeschis;
        using (var os = s.Provider.CreateObjectSpace())
            citiriDeschis = Citiri(os);
        Console.WriteLine($"     MĂSURAT (SOL-C/{eticheta}): citiri de referință pe scena DESCHISĂ — "
            + string.Join(", ", citiriDeschis.OrderBy(kv => kv.Key, StringComparer.Ordinal)
                .Select(kv => $"{kv.Key} {kv.Value.Split('\n').Length} rd.")) + ".");
        s.Check($"SOL-C0 ({eticheta}) precondiția comparației: citirile de pe scena deschisă sunt NEgoale "
            + "(refuzul gardianului de sold există ca text, fișa și balanța au rânduri) — altfel „identic” "
            + "ar fi adevărat prin vid",
            citiriDeschis["refuz-sold-negativ"].StartsWith("STOC_INSUFICIENT: Sold negativ")
            && citiriDeschis["sold-intermediar-sub-sold"] == "<a trecut>"
            && citiriDeschis["balanta-sintetic"].Length > 0 && citiriDeschis["fisa-cont-stoc"].Length > 0
            && citiriDeschis["fisa-cont-filtrata"].Length > 0
            && citiriDeschis["sold-stoc-azi"].Length > 0);
        s.Check($"SOL-C0b ({eticheta}) lotul consumat INTEGRAL din gestiunea A lipsește din `SoldStoc` deja pe scena "
            + "DESCHISĂ: cheia cu cantitate ȘI valoare zero nu mai e o poziție de stoc, nici din registru, nici din "
            + "snapshot — schimbarea de comportament e afirmată aici, nu dedusă din egalitatea de mai jos",
            citiriDeschis["sold-stoc-lot-golit"] == "absent");

        void ComparaCitirile(string moment) {
            Dictionary<string, string> acum;
            using (var os = s.Provider.CreateObjectSpace())
                acum = Citiri(os);
            foreach (var cheie in citiriDeschis.Keys.OrderBy(k => k, StringComparer.Ordinal)) {
                var egal = citiriDeschis[cheie] == acum[cheie];
                if (!egal)
                    Console.WriteLine($"     DIFERENȚĂ ({eticheta}, {moment}, {cheie}): "
                        + PrimaDiferenta(citiriDeschis[cheie], acum[cheie]));
                s.Check($"SOL-C ({eticheta}, {moment}) „{cheie}” iese IDENTIC cu și fără snapshot: consumatorul "
                    + "pornește de la ultima perioadă de referință, nu de la începutul registrului",
                    egal);
            }
        }

        // ═════════════════════ SOL-V1: lanțul și referințele ═════════════════════
        using (var os = s.Provider.CreateObjectSpace())
            s.InchideAcceptTot(os, An, 1, Marcaj);
        using (var os = s.Provider.CreateObjectSpace()) {
            s.Check($"SOL-V1a ({eticheta}) după închiderea lui 01/{An} există snapshot DOAR pentru ea, iar ea e "
                + $"singura perioadă de referință",
                RanduriSnapshot(os, An, 1) > 0 && Referinte(os) == $"01/{An}"
                && RanduriSnapshot(os, An, 2) == 0);
        }
        ComparaCitirile($"referința 01/{An}");
        using (var os = s.Provider.CreateObjectSpace())
            s.InchideAcceptTot(os, An, 2, Marcaj);
        using (var os = s.Provider.CreateObjectSpace()) {
            s.Check($"SOL-V1b ({eticheta}) închiderea lui 02/{An} mută referința: snapshot(01/{An}) DISPARE (nu e "
                + $"capăt de an), snapshot(02/{An}) apare — snapshot-ul există ⇔ perioada e DE REFERINȚĂ",
                RanduriSnapshot(os, An, 1) == 0 && RanduriSnapshot(os, An, 2) > 0
                && Referinte(os) == $"02/{An}");
        }
        using (var os = s.Provider.CreateObjectSpace())
            s.InchideAcceptTot(os, An, 3, Marcaj);
        using (var os = s.Provider.CreateObjectSpace()) {
            Console.WriteLine($"     MĂSURAT (SOL-V1/{eticheta}): referințe = [{Referinte(os)}]; "
                + $"snapshot 03/{An} = {RanduriSnapshot(os, An, 3)} rânduri.");
            s.Check($"SOL-V1c ({eticheta}) după 01 → 02 → 03 rămâne un SINGUR snapshot, al ultimei perioade închise; "
                + "lunile intermediare nu se păstrează (stocarea, nu timpul, e costul lui D3)",
                RanduriSnapshot(os, An, 1) == 0 && RanduriSnapshot(os, An, 2) == 0
                && RanduriSnapshot(os, An, 3) > 0 && Referinte(os) == $"03/{An}");
        }
        ComparaCitirile($"referința 03/{An}");

        // ═════════════════════ SOL-V2: egalitatea la cent ═════════════════════
        using (var os = s.Provider.CreateObjectSpace()) {
            var snap = SnapshotContabil(os, An, 3);
            var asteptat = AsteptatContabil(os, Ultima(An, 3));
            Console.WriteLine($"     MĂSURAT (SOL-V2/{eticheta}): snapshot contabil {snap.Count} chei, "
                + $"recalcul LINQ {asteptat.Count} chei.");
            s.Check($"SOL-V2a ({eticheta}) snapshot(03/{An}) contabil = `SUM` peste `ContabilProiectii.Atomi` pe cheia "
                + "COMPLETĂ a atomului (cont + 8 dimensiuni ale laturii), la cent și în AMBELE sensuri — inclusiv "
                + "rândurile de storno, care intră algebric (R-D7)",
                EgalContabil(os, An, 3));

            var snapStoc = SnapshotStoc(os, An, 3);
            var asteptatStoc = AsteptatStoc(os, Ultima(An, 3));
            Console.WriteLine($"     MĂSURAT (SOL-V2/{eticheta}): snapshot stoc {snapStoc.Count} chei, "
                + $"recalcul LINQ pe cub {asteptatStoc.Count} chei.");
            s.Check($"SOL-V2b ({eticheta}) snapshot(03/{An}) stoc = cub pe lot/cont/produs/gestiune și data deschiderii",
                EgalStoc(os, An, 3));

            var cheieGolita = snapStoc.Keys.Any(k => k.LotId == idLot1 && k.GestiuneId == idGestA);
            var soldGolit = Atlas.Conta.BackOffice.Module.Cub.Citiri.Loturi.Solduri(os, Ultima(An, 3))
                .FirstOrDefault(s => s.LotId == idLot1 && s.GestiuneId == idGestA);
            Console.WriteLine($"     MĂSURAT (SOL-V2c/{eticheta}): lotul golit integral are sold "
                + $"({soldGolit.Cantitate}, {soldGolit.Valoare}) și e {(cheieGolita ? "PREZENT" : "absent")} în snapshot.");
            s.Check($"SOL-V2c ({eticheta}) cheia lotului golit INTEGRAL din gestiunea A lipsește din snapshot: "
                + "cheile integral zero se omit, iar „absentă” și „zero” sunt același răspuns pentru consumator",
                !cheieGolita && soldGolit.Cantitate == 0m && soldGolit.Valoare == 0m);
        }

        // ═════════════════════ SOL-V3: decembrie rămâne ═════════════════════
        for (var luna = 4; luna <= 12; luna++)
            using (var os = s.Provider.CreateObjectSpace())
                s.InchideAcceptTot(os, An, luna, Marcaj);
        using (var os = s.Provider.CreateObjectSpace()) {
            var p = os.CreateObject<PerioadaFiscala>();
            p.An = An + 1;
            p.Luna = 1;
            os.CommitChanges();
        }
        using (var os = s.Provider.CreateObjectSpace())
            s.InchideAcceptTot(os, An + 1, 1, Marcaj);
        using (var os = s.Provider.CreateObjectSpace()) {
            var dec = SnapshotContabil(os, An, 12);
            var ian = SnapshotContabil(os, An + 1, 1);
            Console.WriteLine($"     MĂSURAT (SOL-V3/{eticheta}): referințe = [{Referinte(os)}]; "
                + $"snapshot 12/{An} = {dec.Count} chei, 01/{An + 1} = {ian.Count} chei.");
            s.Check($"SOL-V3 ({eticheta}) capătul de an RĂMÂNE referință: după închiderea lui 01/{An + 1} referințele "
                + $"sunt 12/{An} ȘI 01/{An + 1}, lunile 04–11 n-au snapshot, iar cele două snapshot-uri sunt "
                + "IDENTICE (nicio mișcare în ianuarie) — incrementala peste snapshot(P−1) dă exact `SUM`-ul",
                Referinte(os) == $"12/{An}, 01/{An + 1}"
                && Enumerable.Range(4, 8).All(l => RanduriSnapshot(os, An, l) == 0)
                && dec.Count > 0 && dec.Count == ian.Count
                && dec.All(kv => ian.TryGetValue(kv.Key, out var v) && v == kv.Value)
                && EgalContabil(os, An, 12) && EgalContabil(os, An + 1, 1));
        }

        // ═════════════════════ SOL-V4: redeschiderea ═════════════════════
        using (var os = s.Provider.CreateObjectSpace())
            PerioadaService.Redeschide(os, An + 1, 1, "probă: redeschid ianuarie", null, Marcaj);
        using (var os = s.Provider.CreateObjectSpace()) {
            s.Check($"SOL-V4a ({eticheta}) redeschiderea lui 01/{An + 1} îi ȘTERGE snapshot-ul și îl lasă intact pe "
                + $"cel al lui 12/{An} — care era deja referință ca capăt de an, deci n-are ce reconstrui",
                RanduriSnapshot(os, An + 1, 1) == 0 && RanduriSnapshot(os, An, 12) > 0
                && Referinte(os) == $"12/{An}");
        }
        using (var os = s.Provider.CreateObjectSpace())
            PerioadaService.Redeschide(os, An, 12, "probă: redeschid decembrie", null, Marcaj);
        using (var os = s.Provider.CreateObjectSpace()) {
            Console.WriteLine($"     MĂSURAT (SOL-V4/{eticheta}): referințe după redeschiderea lui 12/{An} = "
                + $"[{Referinte(os)}]; snapshot 11/{An} = {RanduriSnapshot(os, An, 11)} rânduri.");
            s.Check($"SOL-V4b ({eticheta}) redeschiderea lui 12/{An} îi șterge snapshot-ul și îl RECONSTRUIEȘTE pe al "
                + $"lui 11/{An} prin `SUM` integral (P−2 nu mai are din ce porni) — egal la cent cu recalculul",
                RanduriSnapshot(os, An, 12) == 0 && RanduriSnapshot(os, An, 11) > 0
                && Referinte(os) == $"11/{An}" && EgalContabil(os, An, 11) && EgalStoc(os, An, 11));
        }
        using (var os = s.Provider.CreateObjectSpace())
            s.InchideAcceptTot(os, An, 12, Marcaj);
        using (var os = s.Provider.CreateObjectSpace())
            s.InchideAcceptTot(os, An + 1, 1, Marcaj);
        using (var os = s.Provider.CreateObjectSpace())
            s.Check($"SOL-V4c ({eticheta}) re-închiderea 12/{An} → 01/{An + 1} readuce exact cele două referințe: "
                + "ciclul închidere → redeschidere → închidere e idempotent pe snapshot-uri",
                Referinte(os) == $"12/{An}, 01/{An + 1}"
                && EgalContabil(os, An, 12) && EgalContabil(os, An + 1, 1));

        // ═════════════════════ SOL-V5: reconstrucția ═════════════════════
        using (var os = s.Provider.CreateObjectSpace()) {
            var raport = SolduriService.Reconstruieste(os);
            Console.WriteLine($"     MĂSURAT (SOL-V5/{eticheta}): " + string.Join("; ", raport.Referinte.Select(r =>
                $"{r.Luna:00}/{r.An} contabil {r.ContabilExistente}→{r.ContabilRecalculate} ({r.ContabilDiferite} dif.), "
                + $"stoc {r.StocExistente}→{r.StocRecalculate} ({r.StocDiferite} dif.)")) + ".");
            s.Check($"SOL-V5a ({eticheta}) reconstrucția pe scena închisă raportează ZERO diferențe pe ambele "
                + "referințe, cu cifrele scrise explicit — raportul iese ȘI când totul e în regulă (35b)",
                raport.Referinte.Count == 2
                && raport.Referinte.All(r => r.ContabilDiferite == 0 && r.StocDiferite == 0
                    && r.DiferentaDebit == 0m && r.DiferentaCredit == 0m
                    && r.DiferentaCantitate == 0m && r.DiferentaValoare == 0m
                    && r.ContabilExistente == r.ContabilRecalculate
                    && r.StocExistente == r.StocRecalculate));
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            // Coruperea unui rând de snapshot: SQL brut, pe lângă orice cale a motorului.
            var afectate = ((EFCoreObjectSpace)os).DbContext.Database.ExecuteSql(
                FormattableStringFactory.Create(
                    "UPDATE \"SolduriPerioadaContabil\" SET \"Debit\" = \"Debit\" + 1 "
                    + "WHERE \"ID\" = (SELECT \"ID\" FROM \"SolduriPerioadaContabil\" "
                    + "WHERE \"An\" = {0} AND \"Luna\" = {1} ORDER BY \"ID\" LIMIT 1)", An + 1, 1));
            s.Check($"SOL-V5b ({eticheta}) premisă: un rând al referinței 01/{An + 1} a fost corupt cu +1 leu pe debit",
                afectate == 1);
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var raport = SolduriService.Reconstruieste(os);
            var stricata = raport.Referinte.Single(r => r.An == An + 1 && r.Luna == 1);
            var curata = raport.Referinte.Single(r => r.An == An && r.Luna == 12);
            Console.WriteLine($"     MĂSURAT (SOL-V5c/{eticheta}): 01/{An + 1} — {stricata.ContabilDiferite} rânduri "
                + $"diferite, Δdebit = {stricata.DiferentaDebit}.");
            s.Check($"SOL-V5c ({eticheta}) reconstrucția RAPORTEAZĂ exact rândul corupt (1 rând diferit, Δdebit = 1) "
                + "și doar pe referința atinsă — diferența se raportează, nu se ascunde (35b)",
                stricata.ContabilDiferite == 1 && stricata.DiferentaDebit == 1m && stricata.DiferentaCredit == 0m
                && stricata.ContabilExistente == stricata.ContabilRecalculate
                && curata.ContabilDiferite == 0 && curata.StocDiferite == 0);
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var raport = SolduriService.Reconstruieste(os);
            s.Check($"SOL-V5d ({eticheta}) al doilea apel dă zero: rescrierea din primul a repus snapshot-ul pe "
                + "registre, iar egalitatea la cent se întoarce",
                raport.Referinte.All(r => r.ContabilDiferite == 0 && r.StocDiferite == 0)
                && EgalContabil(os, An + 1, 1) && EgalStoc(os, An + 1, 1));
        }

        // ═════════════════════ SOL-V6: cheia pe profilul curent ═════════════════════
        using (var os = s.Provider.CreateObjectSpace()) {
            var snap = os.GetObjectsQuery<SoldPerioadaContabil>()
                .Where(s => s.An == An + 1 && s.Luna == 1)
                .Select(s => new {
                    s.RepartitorId, s.MaterialId, s.CodFunctionalId, s.CodEconomicId,
                    s.SursaFinantareId, s.UnitateId, s.ProiectId, s.CentruCostId
                }).ToList();
            var cuRepartitor = snap.Count(s => s.RepartitorId != null);
            var cuBugetare = snap.Count(s => s.CodFunctionalId != null || s.CodEconomicId != null
                || s.SursaFinantareId != null || s.UnitateId != null || s.ProiectId != null);
            Console.WriteLine($"     MĂSURAT (SOL-V6/{eticheta}): {snap.Count} chei, {cuRepartitor} cu repartitor, "
                + $"{cuBugetare} cu cel puțin o dimensiune bugetară.");
            s.Check($"SOL-V6 ({eticheta}) cheia snapshot-ului poartă dimensiunile LATURII, nu ale raportului: "
                + "rândurile scenei au repartitori diferiți pe debit față de credit, deci cheia completă e singura "
                + "din care orice rollup (cont, cont × repartitor, cont × dimensiuni) iese ADITIV",
                snap.Count > 0 && cuRepartitor > 0 && cuBugetare > 0);
        }

        // ═════════════════════ curățenia ═════════════════════
        using (var os = s.Provider.CreateObjectSpace())
            CurataSol(os);
        using (var os = s.Provider.CreateObjectSpace()) {
            var perioade = os.GetObjectsQuery<PerioadaFiscala>()
                .Count(p => p.An == An || p.An == An + 1);
            var snapshoturi = os.GetObjectsQuery<SoldPerioadaContabil>().Count()
                + os.GetObjectsQuery<SoldPerioadaStoc>().Count();
            var documente = os.GetObjectsQuery<Document>()
                .Count(d => d.Data >= new DateOnly(An, 1, 1) && d.Data <= new DateOnly(An + 1, 12, 31));
            s.Check($"SOL-V7 ({eticheta}) fără reziduu: nicio perioadă {An}–{An + 1}, niciun document și NICIUN rând "
                + "de snapshot rămase — scena e re-rulabilă identic",
                perioade == 0 && documente == 0 && snapshoturi == 0);
        }
    }
}
