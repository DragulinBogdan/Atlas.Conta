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

// ═══════════════════════════════════════════════════════════════════════════════
// Review advers felia 27, pasul 8b — probele `F27-R*`. Scena stă în 2036
// (12/2035 nedefinit ⇒ 01/2036 e capăt de lanț); niciun alt bloc nu atinge anul.
// Fiecare probă răspunde unui scenariu din spec (R1 cursa pe căile fără
// `ComenziDocument`, R2 redeschiderea cu rectificativă, R3 corecția cu conex/pereche/
// stingeri, R4 lotul consumat între `Data` și `DataInregistrare`, R5 eroarea
// materială cu partener schimbat, R6 partida inversată în P+1, R7 anularea după
// redeschidere, R8 dry-run-ul sub închidere, R9 sabotajul snapshot-ului, R11
// două închideri concurente, plus partea liberă: lanțul cu gol, perioada
// faptului nedefinită, ștergerea originalului desfăcut).
static class VerificaReviewF27 {
    public static void Ruleaza(Suita s, bool privat) {
        const string Marcaj = "E2E-R27";
        const int An = 2036;
        var eticheta = privat ? "privat" : "bugetar";
        var codTipStoc = privat ? "371" : "302.01.00";
        var codTipVenit = privat ? "704" : "751.01.00";
        var codContCasa = privat ? "5311" : "531.01.01";
        DateOnly Zi(int luna, int zi) => new(An, luna, zi);

        void CurataR27(IObjectSpace os) {
            for (var luna = 1; luna <= 12; luna++) {
                SolduriService.Elimina(os, An, luna);
                SolduriService.Elimina(os, An - 1, luna);
            }
            var pj = new Purja(os);
            pj.Adauga(os.GetObjectsQuery<DepunereDeclaratie>()
                .Where(d => d.Perioada / 100 == An).ToList());
            var docIds = os.GetObjectsQuery<Document>()
                .Where(d => d.Data >= new DateOnly(An - 1, 12, 1) && d.Data <= new DateOnly(An, 12, 31))
                .Select(d => d.ID).ToList();
            var produsIds = os.GetObjectsQuery<Produs>()
                .Where(p => p.Cod.StartsWith(Marcaj)).Select(p => p.ID).ToList();
            var lotIds = os.GetObjectsQuery<Lot>()
                .Where(l => produsIds.Contains(l.ProdusId)).Select(l => l.ID).ToList();
            foreach (var imp in os.GetObjectsQuery<Imperechere>()
                    .Where(i => docIds.Contains(i.DocumentId) || docIds.Contains(i.DocumentStingatorId))
                    .OrderByDescending(i => i.InverseazaId != null))
                pj.Adauga(imp);
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>()
                .Where(d => docIds.Contains(d.DocumentId)).ToList());
            // Corecțiile (FK `CorecteazaId`) și conexele (FK `DocumentSursaId`) înaintea originalelor.
            foreach (var doc in os.GetObjectsQuery<Document>()
                    .Where(d => docIds.Contains(d.ID))
                    .OrderByDescending(d => d.DocumentSursaId != null).ThenByDescending(d => d.CorecteazaId != null))
                pj.Adauga(doc);
            pj.Adauga(os.GetObjectsQuery<Lot>()
                .Where(l => lotIds.Contains(l.ID)).ToList());
            pj.Adauga(os.GetObjectsQuery<Produs>()
                .Where(p => produsIds.Contains(p.ID)).ToList());
            var perioadeIds = os.GetObjectsQuery<PerioadaFiscala>()
                .Where(p => p.An == An).Select(p => p.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<InchiderePerioada>()
                .Where(i => perioadeIds.Contains(i.PerioadaId)).ToList());
            pj.Adauga(os.GetObjectsQuery<PerioadaFiscala>()
                .Where(p => p.An == An).ToList());
            pj.Adauga(os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(Marcaj)).ToList());
            pj.Adauga(os.GetObjectsQuery<CodEconomic>()
                .Where(c => c.Cod.StartsWith(Marcaj)).ToList());
            pj.Executa();
        }

        string CodPg(Exception e) {
            for (var x = e; x != null; x = x.InnerException)
                if (x is Npgsql.PostgresException pg)
                    return pg.SqlState;
            return null;
        }

        // Comanda pe un OS propriu, cu `lock_timeout` scurt: null = a trecut, altfel
        // codul Postgres (55P03 = a așteptat un lock) sau mesajul refuzului.
        string SubLock(Action<IObjectSpace> comanda) {
            using var os = s.Provider.CreateObjectSpace();
            var db = ((EFCoreObjectSpace)os).DbContext.Database;
            db.OpenConnection();
            try {
                db.ExecuteSqlRaw("SET lock_timeout = '1500ms'");
                comanda(os);
                return null;
            }
            catch (OperareException e) {
                return "REFUZ: " + e.Message.Split('\n')[0];
            }
            catch (Exception e) {
                return CodPg(e) ?? e.GetType().Name;
            }
            finally {
                db.CloseConnection();
            }
        }

        Npgsql.NpgsqlTransaction LockExtern(Npgsql.NpgsqlConnection c, int luna, string mod) {
            c.Open();
            var tx = c.BeginTransaction();
            using var cmd = c.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = "SELECT \"ID\" FROM \"PerioadeFiscale\" "
                + $"WHERE \"An\" = {An} AND \"Luna\" = {luna} FOR {mod}";
            cmd.ExecuteNonQuery();
            return tx;
        }

        string Refuz27(Action a) {
            try { a(); return null; }
            catch (OperareException e) { return e.Message.Split('\n')[0]; }
        }

        string RefuzGardianR27(IObjectSpace os) {
            try { GardianEditare.Verifica(os); return null; }
            catch (OperareException e) { return e.Message; }
        }

        List<(string Tip, string Numar, decimal Total, decimal Ramas)> Rest27(IObjectSpace os, Guid contrapartida,
                DateOnly? laData = null) =>
            ImperecheriProiectii.DocumenteCuRest(os, contrapartida, null, laData)
                .ToList().OrderBy(r => r.Numar, StringComparer.Ordinal)
                .Select(r => (r.Tip, r.Numar, r.Total, Ramas: r.Rest)).ToList();

        Dictionary<Guid, decimal> Partide27(IObjectSpace os, int luna) =>
            os.GetObjectsQuery<PartidaDeschisa>().Where(p => p.An == An && p.Luna == luna)
                .Select(p => new { p.DocumentId, p.Rest }).ToList().Where(p => p.DocumentId != null).GroupBy(p => p.DocumentId.Value).ToDictionary(g => g.Key, g => g.Sum(p => p.Rest));

        List<(int An, int Luna)> CuSnapshot(IObjectSpace os) =>
            os.GetObjectsQuery<SoldPerioadaContabil>().Select(s => new { s.An, s.Luna }).Distinct().ToList()
                .Select(x => (x.An, x.Luna)).Union(
                os.GetObjectsQuery<SoldPerioadaStoc>().Select(s => new { s.An, s.Luna }).Distinct().ToList()
                .Select(x => (x.An, x.Luna))).Union(
                os.GetObjectsQuery<PartidaDeschisa>().Select(s => new { s.An, s.Luna }).Distinct().ToList()
                .Select(x => (x.An, x.Luna)))
                .OrderBy(x => x.An).ThenBy(x => x.Luna).ToList();

        using (var os = s.Provider.CreateObjectSpace())
            CurataR27(os);

        using (var os = s.Provider.CreateObjectSpace()) {
            var perioade = os.GetObjectsQuery<PerioadaFiscala>().Count(p => p.An == An || p.An == An - 1);
            var documente = os.GetObjectsQuery<Document>()
                .Count(d => d.Data >= new DateOnly(An - 1, 12, 1) && d.Data <= new DateOnly(An, 12, 31));
            var inchise = os.GetObjectsQuery<PerioadaFiscala>().Count(p => p.Inchisa);
            s.Check($"F27-R0 ({eticheta}) precondiție: {An} (și 12/{An - 1}) liber, nicio perioadă închisă în bază, "
                + "niciun snapshot — altfel cifrele de mai jos ar fi peste conținut străin",
                perioade == 0 && documente == 0 && inchise == 0 && CuSnapshot(os).Count == 0);
        }

        // ═════════════ nomenclatoarele scenei ═════════════
        Guid idFurnizor, idClientA, idClientB, idGestA, idGestB, idCasa, idProdus, idCodEc, idTipStoc, idTipVenit,
            idSediu, idTipTrz, idN21 = Guid.Empty;
        using (var os = s.Provider.CreateObjectSpace()) {
            foreach (var luna in new[] { 1, 2, 3, 5 }) {
                var p = os.CreateObject<PerioadaFiscala>();
                p.An = An;
                p.Luna = luna;
            }
            var furnizor = os.CreateObject<Partener>();
            furnizor.Cod = Marcaj + "-F";
            furnizor.Denumire = "Furnizor review 27";
            furnizor.CodFiscal = "RO33333340";
            furnizor.InregistratTva = true;
            var clientA = os.CreateObject<Partener>();
            clientA.Cod = Marcaj + "-CA";
            clientA.Denumire = "Client A review 27";
            clientA.CodFiscal = "RO44444443";
            clientA.InregistratTva = true;
            var clientB = os.CreateObject<Partener>();
            clientB.Cod = Marcaj + "-CB";
            clientB.Denumire = "Client B review 27";
            clientB.CodFiscal = "RO55555554";
            clientB.InregistratTva = true;
            var gestA = os.CreateObject<Gestiune>();
            gestA.Cod = Marcaj + "-GA";
            gestA.Denumire = "Gestiune A review 27";
            var gestB = os.CreateObject<Gestiune>();
            gestB.Cod = Marcaj + "-GB";
            gestB.Denumire = "Gestiune B review 27";
            var casa = os.CreateObject<ContPropriu>();
            casa.Cod = Marcaj + "-CS";
            casa.Denumire = "Casa review 27";
            casa.ContImplicit = os.FirstOrDefault<Cont>(c => c.Simbol == codContCasa);
            var tipStoc = os.FirstOrDefault<TipMaterial>(t => t.Cod == codTipStoc);
            var produs = os.CreateObject<Produs>();
            produs.Cod = Marcaj + "-P";
            produs.Denumire = "Produs review 27";
            produs.UM = "BUC";
            produs.TipMaterial = tipStoc;
            var codEc = os.CreateObject<CodEconomic>();
            codEc.Cod = Marcaj + "-CE";
            codEc.Denumire = "Cod economic review 27";
            os.CommitChanges();
            idFurnizor = furnizor.ID; idClientA = clientA.ID; idClientB = clientB.ID;
            idGestA = gestA.ID; idGestB = gestB.ID; idCasa = casa.ID; idProdus = produs.ID; idCodEc = codEc.ID;
            idTipStoc = tipStoc.ID;
            idTipVenit = os.FirstOrDefault<TipMaterial>(t => t.Cod == codTipVenit).ID;
            idSediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU").ID;
            idTipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ").ID;
            if (privat)
                idN21 = os.FirstOrDefault<TipTva>(t => t.Cod == "N21").ID;
            s.Check($"F27-R — precondiție de profil ({eticheta}): tipurile de stoc/venit/TRZ, sediul și contul casei "
                + "sunt în seed", tipStoc != null && casa.ContImplicit != null && idTipVenit != Guid.Empty);
        }

        // Fabricile scenei — fiecare pe OS-ul apelantului, fără commit.
        FacturaIesire Fcl(IObjectSpace os, string sufix, DateOnly data, DateOnly inreg, Guid client, decimal valoare) {
            var f = os.CreateObject<FacturaIesire>();
            f.Numar = Marcaj + sufix;
            f.Data = data;
            f.DataInregistrare = inreg;
            f.PredatorId = idSediu;
            f.PrimitorId = client;
            var l = os.CreateObject<FacturaIesireDetaliu>();
            l.Document = f;
            l.TipMaterialId = idTipVenit;
            l.Cantitate = 1m;
            l.PretUnitar = valoare;
            l.CodEconomicId = idCodEc;
            if (privat)
                l.TipTvaId = idN21;
            return f;
        }

        Incasare Inc(IObjectSpace os, DateOnly data, DateOnly inreg, Guid client, decimal valoare) {
            var i = os.CreateObject<Incasare>();
            i.Data = data;
            i.DataInregistrare = inreg;
            i.PredatorId = client;
            i.PrimitorId = idCasa;
            i.TipInstrument = TipInstrumentPlata.Chitanta;
            var l = os.CreateObject<DocumentTrezorerieDetaliu>();
            l.Document = i;
            l.TipMaterialId = idTipTrz;
            l.Valoare = valoare;
            l.CodEconomicId = idCodEc;
            return i;
        }

        // FCT cu linie de STOC (naște lot ⇒ conex NIR la operare), opțional cu plata autogenerată.
        FacturaIntrare FctStoc(IObjectSpace os, string sufix, DateOnly data, DateOnly inreg, decimal cantitate,
                decimal pret, bool cuPlata = false, DateOnly? plataData = null) {
            var f = os.CreateObject<FacturaIntrare>();
            f.Numar = Marcaj + sufix;
            f.Data = data;
            f.DataInregistrare = inreg;
            f.PredatorId = idFurnizor;
            f.PrimitorId = idGestA;
            if (cuPlata) {
                f.GenereazaPlata = true;
                f.PlataContPropriuId = idCasa;
                f.PlataData = plataData;
            }
            var l = os.CreateObject<FacturaIntrareDetaliu>();
            l.Document = f;
            l.TipMaterialId = idTipStoc;
            l.Cantitate = cantitate;
            l.PretUnitar = pret;
            l.CodEconomicId = idCodEc;
            if (privat)
                l.TipTvaId = idN21;
            l.CreeazaLot(os, os.GetObjectByKey<Produs>(idProdus), os.GetObjectByKey<Gestiune>(idGestA));
            return f;
        }

        NIR Nir(IObjectSpace os, DateOnly data, DateOnly inreg, decimal cantitate, decimal pret, out Lot lot) {
            var n = os.CreateObject<NIR>();
            n.Data = data;
            n.DataInregistrare = inreg;
            n.PredatorId = idFurnizor;
            n.PrimitorId = idGestA;
            var l = os.CreateObject<NirDetaliu>();
            l.Document = n;
            l.TipMaterialId = idTipStoc;
            l.Cantitate = cantitate;
            l.PretUnitar = pret;
            l.CodEconomicId = idCodEc;
            lot = l.CreeazaLot(os, os.GetObjectByKey<Produs>(idProdus), os.GetObjectByKey<Gestiune>(idGestA));
            return n;
        }

        NotaTransfer Btr(IObjectSpace os, string pv, DateOnly data, DateOnly inreg, Guid lotId, decimal cantitate) {
            var b = os.CreateObject<NotaTransfer>();
            b.Data = data;
            b.DataInregistrare = inreg;
            b.PredatorId = idGestA;
            b.PrimitorId = idGestB;
            b.NumarPV = Marcaj + pv;
            var l = os.CreateObject<DocumentDetaliu>();
            l.Document = b;
            l.TipMaterialId = idTipStoc;
            l.LotId = lotId;
            l.Cantitate = cantitate;
            return b;
        }

        // ═════════════ R4 — lotul consumat între `Data` și `DataInregistrare` (ianuarie deschis) ═════════════
        Guid idLotA, idLotB;
        using (var os = s.Provider.CreateObjectSpace()) {
            var nirA = Nir(os, Zi(1, 5), Zi(2, 5), 10m, 10m, out var lotA);
            var nirB = Nir(os, Zi(1, 20), Zi(1, 20), 10m, 20m, out var lotB);
            os.CommitChanges();
            MotorOperare.Opereaza(os, nirA);
            MotorOperare.Opereaza(os, nirB);
            idLotA = lotA.ID; idLotB = lotB.ID;

            var btrDevreme = Btr(os, "-BTR-10.01", Zi(1, 10), Zi(1, 10), lotA.ID, 3m);
            os.CommitChanges();
            var refuz = Refuz27(() => MotorOperare.Opereaza(os, btrDevreme));
            Console.WriteLine($"     MĂSURAT (F27-R4a/{eticheta}): ieșirea din lotul întârziat, înregistrată pe "
                + $"{Zi(1, 10):dd.MM.yyyy} → „{refuz ?? "<A TRECUT>"}”.");
            s.Check($"F27-R4a ({eticheta}) ieșirea din lotul NIR-ului întârziat (Data 05.01, înregistrat 05.02) "
                + "înregistrată pe 10.01 cade pe gardianul de sold — lotul nu există în evidență la ziua aceea",
                refuz != null && refuz.Contains("STOC_INSUFICIENT"));

            btrDevreme.DataInregistrare = Zi(2, 10);
            os.CommitChanges();
            MotorOperare.Opereaza(os, btrDevreme);
            var stoc = CubScena.Stoc(os, btrDevreme.ID);
            var la31Ian = StocProiectii.SoldStoc(os, Zi(1, 31)).ToList()
                .Where(s => s.LotId == lotA.ID || s.LotId == lotB.ID).ToList();
            Console.WriteLine($"     MĂSURAT (F27-R4b/{eticheta}): același BTR cu `Data` 10.01 și înregistrare 10.02 "
                + $"→ {stoc.Count} mișcări de stoc la {stoc.Select(r => r.Data).FirstOrDefault():dd.MM.yyyy}; "
                + $"soldul la 31.01: {string.Join(", ", la31Ian.Select(s => $"{(s.LotId == lotA.ID ? "A" : "B")}={s.Cantitate}"))}.");
            s.Check($"F27-R4b ({eticheta}) același BTR cu `Data` fizică 10.01 și înregistrare 10.02 TRECE și își scrie "
                + "registrele la 10.02 (consecința D4: documentul fizic poate preceda intrarea în evidență a lotului); "
                + "soldul la 31.01 arată doar lotul B (10), lotul A nu există încă în evidență",
                btrDevreme.Stare == StareDocument.Operat && CubScena.Stoc(os, btrDevreme.ID).All(p => p.Data == Zi(2, 10))
                && la31Ian.Count == 1 && la31Ian[0].LotId == lotB.ID && la31Ian[0].Cantitate == 10m);
        }

        // ═════════════ R3c — factura întârziată cu plata autogenerată (ambele perioade DESCHISE) ═════════════
        Guid idFctTarzie, idPlataTarzie;
        using (var os = s.Provider.CreateObjectSpace()) {
            var fct = FctStoc(os, "-FCT-TARZ", Zi(1, 10), Zi(1, 25), 1m, 100m, cuPlata: true, plataData: Zi(1, 15));
            os.CommitChanges();
            MotorOperare.Opereaza(os, fct);
            idFctTarzie = fct.ID;
            var plata = os.GetObjectsQuery<Plata>().FirstOrDefault(p => p.DocumentSursaId == fct.ID);
            idPlataTarzie = plata?.ID ?? Guid.Empty;
            var refuz = plata == null ? "<fără plată generată>" : Refuz27(() => MotorOperare.Opereaza(os, plata));
            Console.WriteLine($"     MĂSURAT (F27-R3c/{eticheta}): FCT `Data` 10.01, înregistrată 25.01, plata culeasă "
                + $"pe 15.01 → plata generată la {plata?.Data:dd.MM.yyyy} (înregistrare {s.Ziua(plata?.DataInregistrare ?? default)}), "
                + $"operarea ei → „{refuz ?? "<A TRECUT>"}”.");
            s.Check($"F27-R3c ({eticheta}) plata autogenerată a unei facturi înregistrate mai târziu decât ziua plății "
                + "SE OPEREAZĂ: data ei de înregistrare e max(data plății, înregistrarea facturii), deci imperecherea "
                + "automată nu mai precede intrarea facturii în evidență — fluxul normal al facturii întârziate (D4) "
                + "nu se mai blochează pe plata ei",
                plata != null && refuz == null);
        }

        // ═════════════ documentele lui ianuarie pentru R2/R3/R5/R6/R7 ═════════════
        Guid idFcl1, idFclA5, idFcl6, idInc6, idImp6, idFclX, idIncZ, idFctS, idFctT, idNirT, idFcl27, idInc27, idImp27,
            idConexFctS;
        using (var os = s.Provider.CreateObjectSpace()) {
            var fcl1 = Fcl(os, "-FCL1", Zi(1, 6), Zi(1, 6), idClientA, 300m);
            var fclA5 = Fcl(os, "-FCL-A5", Zi(1, 7), Zi(1, 7), idClientA, 100m);
            var fcl6 = Fcl(os, "-FCL6", Zi(1, 8), Zi(1, 8), idClientB, 100m);
            var inc6 = Inc(os, Zi(1, 12), Zi(1, 12), idClientB, 60m);
            var fclX = Fcl(os, "-FCL-X", Zi(1, 9), Zi(1, 9), idClientB, 40m);
            var incZ = Inc(os, Zi(1, 15), Zi(1, 15), idClientB, 30m);
            var fctS = FctStoc(os, "-FCT-S", Zi(1, 11), Zi(1, 11), 2m, 50m);
            var fctT = FctStoc(os, "-FCT-T", Zi(1, 13), Zi(1, 13), 3m, 30m);
            var fcl27 = Fcl(os, "-FCL-27", Zi(1, 14), Zi(1, 14), idClientA, 80m);
            var inc27 = Inc(os, Zi(1, 16), Zi(1, 16), idClientA, 80m);
            os.CommitChanges();
            foreach (var d in new Document[] { fcl1, fclA5, fcl6, inc6, fclX, incZ, fctS, fctT, fcl27, inc27 })
                MotorOperare.Opereaza(os, d);
            idFcl1 = fcl1.ID; idFclA5 = fclA5.ID; idFcl6 = fcl6.ID; idInc6 = inc6.ID; idFclX = fclX.ID; idIncZ = incZ.ID;
            idFctS = fctS.ID; idFctT = fctT.ID; idFcl27 = fcl27.ID; idInc27 = inc27.ID;
            idImp6 = ImperechereService.Imperecheaza(os, inc6, fcl6, 60m, null, Zi(1, 12)).ID;
            idImp27 = ImperechereService.Imperecheaza(os, inc27, fcl27, 80m, null, Zi(1, 16)).ID;
            // Conexul lui FCT-T (NIR-ul autogenerat) se OPEREAZĂ; al lui FCT-S rămâne draft.
            var nirT = os.GetObjectsQuery<NIR>().First(n => n.DocumentSursaId == fctT.ID);
            MotorOperare.Opereaza(os, nirT);
            idNirT = nirT.ID;
            var conexFctS = os.GetObjectsQuery<NIR>().FirstOrDefault(n => n.DocumentSursaId == fctS.ID);
            idConexFctS = conexFctS?.ID ?? Guid.Empty;
            s.Check($"F27-R — scena lui ianuarie ({eticheta}): 10 documente operate, două stingeri, conexul lui FCT-T "
                + "operat, al lui FCT-S draft",
                nirT.Stare == StareDocument.Operat
                && conexFctS is { Stare: StareDocument.Draft });
        }

        // ═════════════ R1/R8 — cursa pe căile care NU trec prin `ComenziDocument` ═════════════
        using (var externa = new Npgsql.NpgsqlConnection(s.ConnectionString)) {
            using var tx = LockExtern(externa, 1, "UPDATE");
            var imp = SubLock(o => ImperechereService.Imperecheaza(o, o.GetObjectByKey<Document>(idIncZ),
                o.GetObjectByKey<Document>(idFclX), 1m, null, Zi(1, 20)));
            var desfa = SubLock(o => ImperechereService.Desfa(o, idImp6, Zi(1, 21)));
            var corectie = SubLock(o => CorectieService.Corecteaza(o, idFclX, Zi(1, 22), MotivCorectie.FaptNou));
            // Subiectul dry-run-ului trebuie să fie un DRAFT: pe un document deja
            // operat `Valideaza` răspunde din starea lui, fără să atingă perioada.
            // Conexul lui FCT-S e singurul draft al scenei la ora asta.
            var idDraftDryRun = idConexFctS;
            var dryRun = SubLock(o => ComenziDocument.Sistem(o).Valideaza(idDraftDryRun));
            var reconstructie = SubLock(o => Atlas.Conta.BackOffice.Module.Api.Perioade.PerioadeApply.Reconstruieste(o));
            var gardian = SubLock(o => {
                var i = o.CreateObject<Imperechere>();
                i.DocumentStingator = o.GetObjectByKey<Document>(idIncZ);
                i.Document = o.GetObjectByKey<Document>(idFclX);
                i.Suma = 1m;
                i.Data = Zi(1, 20);
                GardianEditare.Verifica(o);
            });
            Console.WriteLine($"     MĂSURAT (F27-R1/{eticheta}) sub `FOR UPDATE` pe 01/{An}: imperechere → "
                + $"„{imp ?? "<a trecut>"}”, desfacere → „{desfa ?? "<a trecut>"}”, corecție → „{corectie ?? "<a trecut>"}”, "
                + $"dry-run → „{dryRun ?? "<a trecut>"}”, reconstrucție → „{reconstructie ?? "<a trecut>"}”, "
                + $"gardianul de Committing → „{gardian ?? "<a trecut>"}”.");
            s.Check($"F27-R1a ({eticheta}) imperecherea manuală, desfacerea și corecția AȘTEAPTĂ închiderea „în curs” "
                + "(toate trei iau tranzacția comenzii și `FOR SHARE` prin gardian)",
                imp == "55P03" && desfa == "55P03" && corectie == "55P03");
            s.Check($"F27-R8 ({eticheta}) dry-run-ul (`Valideaza`, fără tranzacție) ia și el `FOR SHARE` în autocommit, "
                + "deci așteaptă închiderea în loc să răspundă pe o stare care se schimbă — fără deadlock (nimeni nu "
                + "ține două rânduri)", dryRun == "55P03");
            s.Check($"F27-R1b ({eticheta}) reconstrucția soldurilor ia `FOR UPDATE` pe TOT lanțul ca primă instrucțiune, "
                + "deci AȘTEAPTĂ o închidere „în curs”: altfel închiderea ar comite după citirea referințelor, iar "
                + "pasul final al reconstrucției („șterge ce nu e referință”) ar fi șters snapshot-ul proaspăt al "
                + "perioadei tocmai închise ⇒ soldurile ar fi pornit tăcut de la zero",
                reconstructie == "55P03");
            s.Check($"F27-R1c ({eticheta}) gardianul de Committing al ușii securizate (XAF: imperecherea nouă) citește "
                + "`FOR SHARE` în AUTOCOMMIT — așteaptă închiderea, dar lock-ul se eliberează la sfârșitul "
                + "instrucțiunii, înainte de `SaveChanges` (fereastra e probată la F27-R1d)",
                gardian == "55P03");
            tx.Rollback();
        }

        // R1d — ușa XAF, în forma de după fix: culegerea dialogului nu se mai comite
        // pe ObjectSpace-ul SECURIZAT, ci devine comanda `Imperecheaza`. Gardianul
        // refuză orice creare CRUD directă (101), iar comanda de după închidere
        // ia rândul perioadei în tranzacția ei și cade.
        using (var osXaf = s.Provider.CreateObjectSpace()) {
            var i = osXaf.CreateObject<Imperechere>();
            i.DocumentStingator = osXaf.GetObjectByKey<Document>(idIncZ);
            i.Document = osXaf.GetObjectByKey<Document>(idFclX);
            i.Suma = 1m;
            i.Data = Zi(1, 20);
            var verdictGardian = RefuzGardianR27(osXaf);
            using (var osInchidere = s.Provider.CreateObjectSpace())
                s.InchideAcceptTot(osInchidere, An, 1, Marcaj);
            var refuzComanda = Refuz27(() => {
                using var osComanda = s.Provider.CreateObjectSpace();
                ImperechereService.Imperecheaza(osComanda,
                    osComanda.GetObjectByKey<Document>(idIncZ),
                    osComanda.GetObjectByKey<Document>(idFclX), 1m, null, Zi(1, 20));
            });
            using var osCitire = s.Provider.CreateObjectSpace();
            var scrise = osCitire.GetObjectsQuery<Imperechere>()
                .Count(x => x.DocumentId == idFclX && x.DocumentStingatorId == idIncZ);
            var partideIan = Partide27(osCitire, 1);
            var totalX = ImperechereService.Total(osCitire, idFclX);
            var raport = SolduriService.Reconstruieste(osCitire).Referinte.FirstOrDefault(r => r.Luna == 1);
            Console.WriteLine($"     MĂSURAT (F27-R1d/{eticheta}): gardianul → „{verdictGardian ?? "<a trecut>"}”, "
                + $"comanda după închidere → „{refuzComanda ?? "<A TRECUT>"}”, rânduri scrise {scrise}, "
                + $"partida FCL-X la închidere {partideIan.GetValueOrDefault(idFclX)} (total {totalX}), reconstrucția: {raport?.PartideDiferite} partide diferite.");
            s.Check($"F27-R1d ({eticheta}) ușa XAF nu mai comite imperecherea pe ObjectSpace-ul securizat: culegerea "
                + "este refuzată de gardian pe CRUD, iar comanda de după închidere e REFUZATĂ "
                + "— niciun rând datat în perioada închisă, partidele rămân cele scrise la închidere, reconstrucția "
                + "zero diferențe",
                verdictGardian?.Contains("IMPERECHERE_COMANDA_OBLIGATORIE") == true && refuzComanda != null && refuzComanda.Contains("închis")
                && scrise == 0 && partideIan.GetValueOrDefault(idFclX) == ImperechereService.Total(osCitire, idFclX)
                && raport != null && raport.PartideDiferite == 0);
        }

        // ═════════════ R2 — redeschiderea cu rectificativă deja emisă (privat) ═════════════
        if (privat) {
            Guid idFcl2, idFcl3;
            DateTime? primaInchidere;
            using (var os = s.Provider.CreateObjectSpace()) {
                primaInchidere = os.FirstOrDefault<PerioadaFiscala>(p => p.An == An && p.Luna == 1).InchisaPrimaOara;
                FiscalitateService.ConfirmaDepunerea(os, FormularFiscal.D394, An, 1,
                Atlas.Conta.BackOffice.Module.Cub.Citiri.Fiscale.Versiune(os, FormularFiscal.D394, new(An, 1, 1), new(An, 1, 31)), Marcaj);
                var fcl2 = Fcl(os, "-FCL2", Zi(1, 25), Zi(2, 3), idClientA, 200m);
                os.CommitChanges();
                MotorOperare.Opereaza(os, fcl2);
                idFcl2 = fcl2.ID;
                var rect = TvaProiectii.Rectificativa(os, An, 1);
                s.Check($"F27-R2a ({eticheta}) faptul colectat întârziat (Data 25.01, înregistrat 03.02) se declară în "
                    + "01 ⇒ rectificativa lui 01 are exact rândul lui",
                    rect.EsteRectificativa && rect.Randuri.Count == 1 && rect.Randuri[0].Baza == 200m);
            }
            using (var os = s.Provider.CreateObjectSpace())
                PerioadaService.Redeschide(os, An, 1, "review F27-R2", null, Marcaj);
            using (var os = s.Provider.CreateObjectSpace()) {
                var rectDeschisa = TvaProiectii.Rectificativa(os, An, 1);
                var fcl3 = Fcl(os, "-FCL3", Zi(1, 26), Zi(1, 26), idClientA, 400m);
                os.CommitChanges();
                MotorOperare.Opereaza(os, fcl3);
                idFcl3 = fcl3.ID;
                Console.WriteLine($"     MĂSURAT (F27-R2b/{eticheta}): pe perioada REDESCHISĂ rectificativa e "
                    + $"{rectDeschisa.EsteRectificativa} cu {rectDeschisa.Randuri.Count} rânduri.");
                s.Check($"F27-R2b ({eticheta}) OBSERVAȚIE: pe o perioadă redeschisă `Rectificativa` răspunde în "
                    + "continuare (reperul `InchisaPrimaOara` rămâne) — conținutul e „ce s-a scris după prima "
                    + "declarare”, indiferent de starea curentă",
                    rectDeschisa.EsteRectificativa && rectDeschisa.Randuri.Count == 1);
            }
            using (var os = s.Provider.CreateObjectSpace())
                s.InchideAcceptTot(os, An, 1, Marcaj);
            using (var os = s.Provider.CreateObjectSpace()) {
                var p = os.FirstOrDefault<PerioadaFiscala>(x => x.An == An && x.Luna == 1);
                var rect = TvaProiectii.Rectificativa(os, An, 1);
                var raport = SolduriService.Reconstruieste(os).Referinte.FirstOrDefault(r => r.Luna == 1);
                Console.WriteLine($"     MĂSURAT (F27-R2c/{eticheta}): `InchisaPrimaOara` {p.InchisaPrimaOara:HH:mm:ss.fff} "
                    + $"(prima: {primaInchidere:HH:mm:ss.fff}), rectificativa: {rect.Randuri.Count} rânduri, Σ bază "
                    + $"{rect.Randuri.Sum(r => r.Baza)}; reconstrucția: {raport?.ContabilDiferite} diferențe contabile.");
                s.Check($"F27-R2c ({eticheta}) după re-închidere reperul rămâne PRIMA închidere, iar rectificativa lui 01 "
                    + "= TOATE rândurile scrise după ea (întârziatul + cel operat în redeschidere): fiscal corect — "
                    + "declarația depusă o dată rămâne reperul; snapshot-ul re-închiderii e cel din registre",
                    p.InchisaPrimaOara == primaInchidere && rect.Randuri.Count == 2
                    && rect.Randuri.Sum(r => r.Baza) == 600m && raport != null && raport.ContabilDiferite == 0);
            }
        }

        // ═════════════ R7 — anularea unui document din P după redeschidere ═════════════
        using (var os = s.Provider.CreateObjectSpace())
            PerioadaService.Redeschide(os, An, 1, "review F27-R7", null, Marcaj);
        using (var os = s.Provider.CreateObjectSpace()) {
            var fclX = os.GetObjectByKey<Document>(idFclX);
            var refuz = Refuz27(() => MotorOperare.AnuleazaOperarea(os, fclX));
            s.Check($"F27-R7a ({eticheta}) redeschiderea permite anularea numai fără depunere fiscală confirmată",
                privat ? refuz?.Contains("TVA_DEJA_DECLARATA") == true && fclX.Stare == StareDocument.Operat
                    : refuz == null && fclX.Stare == StareDocument.Draft);
        }
        using (var os = s.Provider.CreateObjectSpace())
            s.InchideAcceptTot(os, An, 1, Marcaj);
        using (var os = s.Provider.CreateObjectSpace()) {
            var partide = Partide27(os, 1);
            var raport = SolduriService.Reconstruieste(os).Referinte.FirstOrDefault(r => r.Luna == 1);
            s.Check($"F27-R7b ({eticheta}) re-închiderea rescrie snapshot-ul și partidele FĂRĂ documentul anulat; "
                + "reconstrucția: zero diferențe",
                (privat ? partide.ContainsKey(idFclX) && !CubScena.FaraNote(os, idFclX)
                    : !partide.ContainsKey(idFclX) && CubScena.FaraNote(os, idFclX)) && raport != null
                && raport.ContabilDiferite == 0 && raport.StocDiferite == 0 && raport.PartideDiferite == 0);
        }

        // ═════════════ R3 — corecția cu conex / stingeri ═════════════
        using (var os = s.Provider.CreateObjectSpace()) {
            var refuzT = Refuz27(() => CorectieService.Corecteaza(os, idFctT, Zi(2, 10), MotivCorectie.FaptNou));
            var refuz27 = Refuz27(() => CorectieService.Corecteaza(os, idFcl27, Zi(2, 10), MotivCorectie.FaptNou));
            Console.WriteLine($"     MĂSURAT (F27-R3a/{eticheta}): corecția FCT-T (conex NIR operat) → „{refuzT}”; "
                + $"corecția FCL-27 (stinsă în ianuarie, închis) → „{refuz27 ?? "<A TRECUT>"}”.");
            s.Check($"F27-R3a ({eticheta}) FCT cu NIR conex OPERAT: corecția e refuzată ca la storno (conexul întâi) — "
                + "textul trimite la anularea/stornarea conexului",
                refuzT != null && refuzT.Contains("conexe"));
            s.Check($"F27-R3b ({eticheta}) documentul stins integral într-o perioadă ÎNCHISĂ se corectează: stingerea se "
                + "inversează automat la storno (rând invers), iar draftul corecției se naște",
                refuz27 == null
                && os.GetObjectsQuery<Imperechere>().Count(i => i.InverseazaId == idImp27) == 1
                && os.GetObjectsQuery<Document>().Any(d => d.CorecteazaId == idFcl27));
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var draftVechi = os.GetObjectsQuery<NIR>().FirstOrDefault(n => n.DocumentSursaId == idFctS);
            var idLinieVeche = os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == idFctS).Select(d => d.ID).First();
            var lotVechi = os.GetObjectsQuery<Lot>().First(l => l.LinieIntrareId == idLinieVeche);
            var (_, corectie) = CorectieService.Corecteaza(os, idFctS, Zi(2, 11), MotivCorectie.FaptNou);
            var draftSters = os.GetObjectsQuery<NIR>().Any(n => n.ID == draftVechi.ID);
            MotorOperare.Opereaza(os, corectie);
            var conexNou = os.GetObjectsQuery<NIR>().FirstOrDefault(n => n.DocumentSursaId == corectie.ID);
            var linieNoua = os.GetObjectsQuery<DocumentDetaliu>().First(d => d.DocumentId == corectie.ID);
            var lotNou = os.GetObjectsQuery<Lot>().FirstOrDefault(l => l.LinieIntrareId == linieNoua.ID);
            Console.WriteLine($"     MĂSURAT (F27-R3d/{eticheta}): conexul draft vechi {(draftSters ? "MAI EXISTĂ" : "șters")}, "
                + $"conex nou "
                + $"{(conexNou == null ? "NU" : $"da, {conexNou.Stare}, LotId {(conexNou.Detalii.FirstOrDefault()?.LotId == lotNou?.ID ? "= lotul nou" : "≠ lotul nou")}")}, "
                + $"lot nou {(lotNou == null ? "-" : $"{lotNou.Data:dd.MM.yyyy} @ {lotNou.PretUnitar}")}.");
            s.Check($"F27-R3d ({eticheta}) FCT cu conex DRAFT: corecția șterge draftul autogenerat, iar operarea corecției "
                + "generează un conex nou pe LOTUL RENĂSCUT (linia-mamă nouă, lotul finalizat la data înregistrării "
                + "corecției, prețul liniei) — fără NIR dublu",
                !draftSters && conexNou != null && conexNou.Stare == StareDocument.Draft && lotNou != null
                && lotNou.ID != lotVechi.ID && conexNou.Detalii.First().LotId == lotNou.ID
                && lotNou.Data == Zi(2, 11) && lotNou.PretUnitar == 50m);
        }

        // ═════════════ R5 — eroarea materială cu partener schimbat (D394, privat) ═════════════
        if (privat) {
            Guid idCorectieA5;
            using (var os = s.Provider.CreateObjectSpace()) {
                var (_, corectie) = CorectieService.Corecteaza(os, idFclA5, Zi(2, 12), MotivCorectie.EroareMateriala);
                corectie.PrimitorId = idClientB;
                os.CommitChanges();
                MotorOperare.Opereaza(os, corectie);
                idCorectieA5 = corectie.ID;
            }
            using (var os = s.Provider.CreateObjectSpace()) {
                var d394 = D394Proiectii.D394(os, Zi(1, 1), Zi(1, 31));
                var randA = d394.Operatiuni.Where(o => o.CuiP == "44444443").ToList();
                var randB = d394.Operatiuni.Where(o => o.CuiP == "55555554").ToList();
                Console.WriteLine($"     MĂSURAT (F27-R5/{eticheta}): D394 01/{An} — client A: "
                    + $"{string.Join("; ", randA.Select(r => $"{r.Tip} nrFact {r.NrFact} bază {r.Baza} tva {r.Tva}"))}; "
                    + $"client B: {string.Join("; ", randB.Select(r => $"{r.Tip} nrFact {r.NrFact} bază {r.Baza} tva {r.Tva}"))}; "
                    + $"rectificativă {d394.Rectificativa} cu {d394.DiferenteDeclarat.Count} diferențe.");
                s.Check($"F27-R5 ({eticheta}) corecția partenerului nu numără inversa tehnică: "
                    + "vechiul partener pierde factura anulată, noul partener primește o singură factură corectată",
                    d394.Rectificativa
                    && randA.Sum(r => r.Baza) == 980m && randA.Sum(r => r.NrFact) == 4
                    && randB.Sum(r => r.Baza) == 240m && randB.Sum(r => r.NrFact) == 3);
            }
        }

        // ═════════════ R6 — partida stinsă parțial în P, inversată în P+1 ═════════════
        using (var os = s.Provider.CreateObjectSpace()) {
            var partideIan = Partide27(os, 1);
            ImperechereService.Desfa(os, idImp6, Zi(2, 15));
            var laFeb = Rest27(os, idClientB, Zi(2, 28));
            var partideDupa = Partide27(os, 1);
            Console.WriteLine($"     MĂSURAT (F27-R6a/{eticheta}): partida FCL6 la 01 = {partideIan.GetValueOrDefault(idFcl6)}; "
                + $"după desfacere la 28.02: {string.Join(" | ", laFeb.Select(r => $"{r.Tip} {r.Numar}: {r.Total}/{r.Ramas}"))}; "
                + $"partida lui 01 după = {partideDupa.GetValueOrDefault(idFcl6)}.");
            var total6Cub = ImperechereService.Total(os, idFcl6);
            s.Check($"F27-R6a ({eticheta}) FCL 100 stinsă cu 60 în 01 (partidă 40); desfacerea din 02 (rând invers −60) "
                + "readuce restul la 100 ȘI face încasarea document cu rest 60, iar partida lui 01 rămâne 40",
                partideIan.GetValueOrDefault(idFcl6) == total6Cub - 60m && partideDupa.GetValueOrDefault(idFcl6) == total6Cub - 60m
                && laFeb.Any(r => r.Tip == "FCL" && r.Numar == Marcaj + "-FCL6" && r.Ramas == total6Cub)
                && laFeb.Any(r => r.Tip == "INC" && r.Ramas == 60m));
        }
        using (var os = s.Provider.CreateObjectSpace())
            s.InchideAcceptTot(os, An, 2, Marcaj);
        using (var os = s.Provider.CreateObjectSpace()) {
            var partideFeb = Partide27(os, 2);
            var raport = SolduriService.Reconstruieste(os).Referinte.FirstOrDefault(r => r.Luna == 2);
            s.Check($"F27-R6b ({eticheta}) închiderea lui 02 materializează partidele cu desfacerea (FCL6 100, INC 60), "
                + "reconstrucția: zero diferențe; 01 nu mai e referință (nu e decembrie)",
                partideFeb.GetValueOrDefault(idFcl6) == ImperechereService.Total(os, idFcl6) && partideFeb.GetValueOrDefault(idInc6) == 60m
                && raport != null && raport.PartideDiferite == 0 && !Partide27(os, 1).Any());
        }

        // ═════════════ R9 — snapshot-ul sabotat ═════════════
        using (var os = s.Provider.CreateObjectSpace()) {
            var db = ((EFCoreObjectSpace)os).DbContext.Database;
            decimal Initial(Atlas.Conta.BackOffice.Module.Cub.Citiri.CitireCumul citire) => ContabilProiectii.Balanta(os, Zi(3, 1), Zi(3, 31), analitic: true,
                citire: citire).ToList().Sum(r => r.InitialDebit + r.InitialCredit);
            var inainte = Initial(Atlas.Conta.BackOffice.Module.Cub.Citiri.CitireCumul.Integrala);
            var sters = db.ExecuteSql($"""
            DELETE FROM "SolduriPerioadaContabil" WHERE "ID" = (
              SELECT "ID" FROM "SolduriPerioadaContabil"
              WHERE "An" = {An} AND "Luna" = 2 AND ("Debit" <> 0 OR "Credit" <> 0)
              ORDER BY "Credit" DESC LIMIT 1)
            """);
            var dupa = Initial(Atlas.Conta.BackOffice.Module.Cub.Citiri.CitireCumul.Integrala);
            var vizibila = Initial(Atlas.Conta.BackOffice.Module.Cub.Citiri.CitireCumul.Vizibila);
            var raport = SolduriService.Reconstruieste(os).Referinte.FirstOrDefault(r => r.Luna == 2);
            Console.WriteLine($"     MĂSURAT (F27-R9/{eticheta}): Σ inițial (debit + credit) al balanței analitice pe 03 înainte "
                + $"{inainte}, după ștergerea unui rând de snapshot {dupa}; reconstrucția raportează "
                + $"{raport?.ContabilDiferite} diferențe (Δ debit {raport?.DiferentaDebit}).");
            s.Check($"F27-R9 ({eticheta}) OBSERVAȚIE: un rând de snapshot lipsă dă o balanță TĂCUT greșită (inițialul "
                + "scade), fără niciun semnal la citire; singura detecție e reconstrucția la cerere, care raportează "
                + "diferența (și repară)",
                sters == 1 && dupa < inainte && raport != null && raport.ContabilDiferite >= 1);
            s.Check($"F27-R9 ({eticheta}) citirea vizibilă (104b) nu atinge snapshot-ul: inițialul rămâne cel dinaintea ștergerii",
                vizibila == inainte);
        }

        // ═════════════ partea liberă: ștergerea originalului desfăcut; desfacerea în fereastra deschisă ═════════════
        using (var os = s.Provider.CreateObjectSpace()) {
            var fcl = Fcl(os, "-FCL-Y", Zi(3, 2), Zi(3, 2), idClientB, 50m);
            var inc = Inc(os, Zi(3, 3), Zi(3, 3), idClientB, 50m);
            os.CommitChanges();
            MotorOperare.Opereaza(os, fcl);
            MotorOperare.Opereaza(os, inc);
            var imp = ImperechereService.Imperecheaza(os, inc, fcl, 50m, null, Zi(3, 4));
            var invers = ImperechereService.Desfa(os, imp.ID, Zi(3, 5));
            s.Check($"F27-RL1 ({eticheta}) OBSERVAȚIE: `Desfa` acceptă și o imperechere din fereastra DESCHISĂ (contractul "
                + "o rezervă perioadei închise) — rezultatul e algebric corect, dar rămân două rânduri în loc de zero",
                invers.InverseazaId == imp.ID && ImperechereService.Ramas(os, fcl.ID) == ImperechereService.Total(os, fcl.ID));
            os.Delete(os.GetObjectByKey<Imperechere>(imp.ID));
            var refuzGardian = RefuzGardianR27(os);
            string refuzCommit;
            try { os.CommitChanges(); refuzCommit = null; }
            catch (Exception e) { refuzCommit = CodPg(e) ?? e.GetType().Name; }
            var ramasFcl = ImperechereService.Ramas(os, fcl.ID);
            var inversDupa = os.GetObjectsQuery<Imperechere>().Where(i => i.ID == invers.ID)
                .Select(i => new { i.InverseazaId, i.Suma }).FirstOrDefault();
            var ramasInc = ImperechereService.Ramas(os, inc.ID);
            var restProiectie = Rest27(os, idClientB).Where(r => r.Numar == Marcaj + "-FCL-Y").Select(r => r.Ramas).FirstOrDefault();
            Console.WriteLine($"     MĂSURAT (F27-RL2/{eticheta}): ștergerea originalului desfăcut — gardian "
                + $"„{refuzGardian ?? "<a trecut>"}”, commit „{refuzCommit ?? "<a trecut>"}”; rest FCL-Y {ramasFcl} "
                + $"(total {ImperechereService.Total(os, fcl.ID)}), rest INC {ramasInc}, proiecția {restProiectie}; rândul invers după: "
                + $"{(inversDupa == null ? "ȘTERS" : $"Suma {inversDupa.Suma}, InverseazaId {(inversDupa.InverseazaId == null ? "NULL" : "păstrat")}")}.");
            s.Check($"F27-RL2 ({eticheta}) ștergerea originalului unei imperecheri DESFĂCUTE e refuzată PE FOND, cu textul "
                + "ei („are un rând invers”), nu pe fixup-ul EF al rândului invers: altfel rândul invers ar fi rămas orfan "
                + "cu −50, iar restul de stins ar fi devenit total + 50 pe ambele documente",
                refuzGardian != null && refuzGardian.Contains("invers"));
        }

        // ═════════════ partea liberă: lanțul cu gol — 04 nedefinit, 05 se închide peste 03 ═════════════
        using (var os = s.Provider.CreateObjectSpace())
            s.InchideAcceptTot(os, An, 3, Marcaj);
        using (var os = s.Provider.CreateObjectSpace()) {
            var refuz = Refuz27(() => s.InchideAcceptTot(os, An, 5, Marcaj));
            var referinte = SolduriService.Referinte(os);
            var cuSnapshot = CuSnapshot(os).Where(x => x.An == An).ToList();
            Console.WriteLine($"     MĂSURAT (F27-RL3/{eticheta}): închiderea lui 05 cu 04 nedefinit → "
                + $"„{refuz ?? "<a trecut>"}”; referințe {string.Join(",", referinte.Select(r => r.Luna))}; "
                + $"snapshot-uri: {string.Join(",", cuSnapshot.Select(r => r.Luna))}.");
            s.Check($"F27-RL3 ({eticheta}) cu 04 NEDEFINIT (închis prin absență) 05 se închide, iar snapshot-ul lui 03 "
                + "DISPARE: închiderea elimină toate referințele de dinaintea lui P care nu sunt decembrie, nu doar "
                + "P−1 definit — „doar referințele au snapshot” rămâne adevărat fără reconstrucție",
                refuz == null && referinte.Select(r => r.Luna).SequenceEqual([5])
                && cuSnapshot.Select(r => r.Luna).SequenceEqual([5]));
            SolduriService.Reconstruieste(os);
            s.Check($"F27-RL3b ({eticheta}) reconstrucția lasă aceeași mulțime (doar 05)",
                CuSnapshot(os).Where(x => x.An == An).Select(r => r.Luna).SequenceEqual([5]));
        }

        // ═════════════ partea liberă: perioada faptului NEDEFINITĂ (privat) ═════════════
        if (privat)
            using (var os = s.Provider.CreateObjectSpace()) {
                using (var o2 = s.Provider.CreateObjectSpace())
                    PerioadaService.Redeschide(o2, An, 5, "review F27-RL4", null, Marcaj);
                var p6 = os.CreateObject<PerioadaFiscala>();
                p6.An = An; p6.Luna = 6;
                os.CommitChanges();
                var fclZ = Fcl(os, "-FCL-Z", new DateOnly(An - 1, 12, 20), Zi(6, 5), idClientA, 10m);
                os.CommitChanges();
                MotorOperare.Opereaza(os, fclZ);
                var fapteZCub = CubScena.Fapte(os, fclZ.ID);
                var rect = TvaProiectii.Rectificativa(os, An - 1, 12);
                Console.WriteLine($"     MĂSURAT (F27-RL4/{eticheta}): faptul din 12/{An - 1} (perioadă NEDEFINITĂ), "
                    + $"înregistrat în 06/{An}, colectat ⇒ declarat în "
                    + $"{fapteZCub.Select(f => $"{f.PerioadaLuna:00}/{f.PerioadaAn}").FirstOrDefault()}; "
                    + $"rectificativa pe 12/{An - 1}: {rect.EsteRectificativa} (reper {rect.ConfirmataLa?.ToString() ?? "null"}).");
                s.Check($"F27-RL4 ({eticheta}) `PerioadaFaptului` peste o perioadă NEDEFINITĂ cade pe perioada "
                    + "ÎNREGISTRĂRII, indiferent de politică: o lună care nu există în bază n-are reper de "
                    + "rectificativă și nu se închide, deci nu se poate declara acolo",
                    fapteZCub.Count > 0 && fapteZCub.All(f => f.PerioadaAn == An - 1 && f.PerioadaLuna == 12)
                    && !rect.EsteRectificativa);
            }

        // ═════════════ R11 — două închideri concurente ale aceleiași perioade ═════════════
        {
            const int luna = 5;
            using (var os = s.Provider.CreateObjectSpace())
                if (os.FirstOrDefault<PerioadaFiscala>(p => p.An == An && p.Luna == luna) is { Inchisa: true })
                    PerioadaService.Redeschide(os, An, luna, "review F27-R11", null, Marcaj);
            string[] rezultate = new string[2];
            // Acceptările și severitatea se pregătesc ÎNAINTE de curse: `InchideAcceptTot`
            // scrie politica pe ObjectSpace propriu, iar două fire care o rescriu
            // concurent ar proba altceva decât cursa închiderii.
            var severitateR11 = s.SeveritateItvLipsa();
            s.SeteazaSeveritateItvLipsa(SeveritateConstatare.Avertisment);
            string[] acceptateR11;
            using (var os = s.Provider.CreateObjectSpace())
                acceptateR11 = PerioadaService.Verifica(os, An, luna).Select(c => c.Cheie).ToArray();
            try {
                using var externa = new Npgsql.NpgsqlConnection(s.ConnectionString);
                using var tx = LockExtern(externa, luna, "UPDATE");
                var taskuri = Enumerable.Range(0, 2).Select(k => System.Threading.Tasks.Task.Run(() => {
                    using var os = s.Provider.CreateObjectSpace();
                    var db = ((EFCoreObjectSpace)os).DbContext.Database;
                    db.OpenConnection();
                    try {
                        db.ExecuteSqlRaw("SET lock_timeout = '20s'");
                        PerioadaService.Inchide(os, An, luna, acceptateR11, null, Marcaj + "-T" + k);
                        rezultate[k] = null;
                    }
                    catch (OperareException e) { rezultate[k] = "REFUZ: " + e.Message.Split('\n')[0]; }
                    catch (Exception e) { rezultate[k] = CodPg(e) ?? e.GetType().Name; }
                    finally { db.CloseConnection(); }
                })).ToArray();
                System.Threading.Thread.Sleep(700);
                tx.Rollback();
                System.Threading.Tasks.Task.WaitAll(taskuri);
            }
            finally {
                s.SeteazaSeveritateItvLipsa(severitateR11);
            }
            using (var os = s.Provider.CreateObjectSpace()) {
                var istoric = os.GetObjectsQuery<InchiderePerioada>().Count(i => i.De.StartsWith(Marcaj + "-T"));
                Console.WriteLine($"     MĂSURAT (F27-R11/{eticheta}): T0 → „{rezultate[0] ?? "<a trecut>"}”, "
                    + $"T1 → „{rezultate[1] ?? "<a trecut>"}”, rânduri de istoric {istoric}.");
                s.Check($"F27-R11 ({eticheta}) două închideri concurente ale lui {luna:00}/{An}: exact una trece, cealaltă "
                    + "așteaptă și cade CURAT pe „e deja închisă” (422), cu un singur rând de istoric",
                    rezultate.Count(r => r == null) == 1
                    && rezultate.Any(r => r != null && r.Contains("deja închisă")) && istoric == 1);
            }
        }

        // ═════════════ desfacerea scenei ═════════════
        using (var os = s.Provider.CreateObjectSpace()) {
            foreach (var luna in new[] { 6, 5, 3, 2, 1 })
                if (os.FirstOrDefault<PerioadaFiscala>(p => p.An == An && p.Luna == luna) is { Inchisa: true })
                    PerioadaService.Redeschide(os, An, luna, "review F27: desfacerea scenei", null, Marcaj);
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            CurataR27(os);
            s.Check($"F27-R — curățenie finală ({eticheta}): nicio perioadă, niciun document și niciun snapshot {An} rămase",
                !os.GetObjectsQuery<PerioadaFiscala>().Any(p => p.An == An)
                && !os.GetObjectsQuery<Document>().Any(d => d.Data >= new DateOnly(An - 1, 12, 1)
                    && d.Data <= new DateOnly(An, 12, 31))
                && !CuSnapshot(os).Any(x => x.An == An));
        }
    }
}
