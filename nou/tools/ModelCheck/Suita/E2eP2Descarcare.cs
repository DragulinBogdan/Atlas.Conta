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

// ================= Scenariul e2e P2: descărcarea de gestiune (DSC) =================
// Magazinul online (design §2): FCL dictată de site (preț decuplat de cost,
// poziții fără stoc la facturare), identificare cu prioritate pe lot. Loturile
// vin din NIR-uri manuale la DATE DIFERITE — FIFO determinist pe Lot.Data (nu
// tie-break pe Lot.ID/Guid). FCL cu FIFO + pin (contenția pe același produs) +
// serviciu + poziție indisponibilă → DSC conex spart pe loturi la GENERARE
// (pin întâi) → operare DSC (cost 6xx=3xx ≠ vânzarea 7xx) → backorder
// (Genereaza direct = acțiunea manuală) → gardieni de grup + storno care
// redeschide restul → anularea cu draft îl șterge → default TipTva de culegere.
static class E2eP2Descarcare {
    public static void Ruleaza(Suita s) {
        const string MarcajDsc = "E2E-DSC";

        void CurataDsc(IObjectSpace os) {
            // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
            var pj = new Purja(os);
            var repIds = os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajDsc)).Select(r => r.ID).ToList();
            var docs = os.GetObjectsQuery<Document>()
                .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
            var docIds = docs.Select(d => d.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<Imperechere>()
                .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
            // Copiii autogenerați (DSC conex, fără Numar) întâi — DocumentSursa spre FCL.
            foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
                pj.Adauga(doc);
            pj.Adauga(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod.StartsWith(MarcajDsc)).ToList());
            pj.Adauga(os.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(MarcajDsc)).ToList());
            // Tipul creat în testul de defect 1 (nu e curățat prin markerul de doc).
            pj.Adauga(os.GetObjectsQuery<TipMaterial>().Where(t => t.Cod.StartsWith(MarcajDsc)).ToList());
            pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajDsc)).ToList());
            pj.Executa();
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            CurataDsc(os);

            var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
            var mag2 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG2");
            var sediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
            var tip371 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "371"); // marfă
            var tip345 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "345"); // produs finit
            var tip704 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "704"); // serviciu (VEN)
            var n21 = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
            var sdd = os.FirstOrDefault<TipTva>(t => t.Cod == "SDD");
            Cont ContSimbol(string simbol) => os.FirstOrDefault<Cont>(c => c.Simbol == simbol);
            var cont4111 = ContSimbol("4111");
            var cont4427 = ContSimbol("4427");
            var cont607 = ContSimbol("607");
            var cont711 = ContSimbol("711");
            var cont707 = ContSimbol("707");
            var cont701 = ContSimbol("701");

            // --- Seed P2 privat ---
            var reguliStocDsc = os.GetObjectsQuery<RegulaStoc>().Where(r => r.TipDocument.Cod == "DSC").ToList();
            s.Check("Seed DSC: −1 pe predator; generic→Magazie, MF→Marfuri (oglindește NIR/LDI privat)",
                reguliStocDsc.Count == 2 && reguliStocDsc.All(r => r.Latura == LaturaDocument.Predator && r.Semn == -1)
                && reguliStocDsc.Any(r => r.ClasaId == null && r.TipStoc == TipStoc.Magazie)
                && reguliStocDsc.Any(r => r.TipStoc == TipStoc.Marfuri));
            RegulaContare CostDsc(TipMaterial tip) => os.FirstOrDefault<RegulaContare>(r => r.TipDocument.Cod == "DSC" && r.TipMaterialId == tip.ID);
            s.Check("Seed DSC cost: 371→607=371, 345→711=345 (excepțiile profilului; credit = contul de stoc al Tipului)",
                CostDsc(tip371)?.ContDebit?.Simbol == "607" && CostDsc(tip371).SursaContCredit == SursaCont.TipMaterial
                && CostDsc(tip345)?.ContDebit?.Simbol == "711" && CostDsc(tip345).SursaContCredit == SursaCont.TipMaterial);
            RegulaContare VanzareFcl(TipMaterial tip) => os.FirstOrDefault<RegulaContare>(r => r.TipDocument.Cod == "FCL" && r.TipMaterialId == tip.ID);
            s.Check("Seed FCL vânzare: 371→707, 345→701 (debit RepartitorPrimitor/4111, credit VENITUL — nu contul de stoc)",
                VanzareFcl(tip371)?.ContCredit?.Simbol == "707" && VanzareFcl(tip371).SursaContDebit == SursaCont.RepartitorPrimitor
                && VanzareFcl(tip371).ContDebit?.Simbol == "4111"
                && VanzareFcl(tip345)?.ContCredit?.Simbol == "701");
            s.Check("Seed FCL: rândul NaturaInterzisa=Stoc șters la privat (descărcarea preia vânzarea din stoc — 37e)",
                os.FirstOrDefault<PoliticaValidare>(p => p.TipDocument.Cod == "FCL") == null);
            s.Check("Seed DSC: numerotare DSC-; ancora fără TipTvaImplicit; N21 default pe FCT/FCL/DEC, NIR/DSC null (37f)",
                os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "DSC")?.Serie == "DSC-"
                && os.FirstOrDefault<TipDocument>(t => t.Cod == "DSC")?.TipTvaImplicitId == null
                && os.FirstOrDefault<TipDocument>(t => t.Cod == "FCT")?.TipTvaImplicitId == n21.ID
                && os.FirstOrDefault<TipDocument>(t => t.Cod == "FCL")?.TipTvaImplicitId == n21.ID
                && os.FirstOrDefault<TipDocument>(t => t.Cod == "DEC")?.TipTvaImplicitId == n21.ID
                && os.FirstOrDefault<TipDocument>(t => t.Cod == "NIR")?.TipTvaImplicitId == null);

            // --- Setup: furnizor, client, produse A/B (marfă 371), C (produs finit 345) ---
            var furnizor = os.CreateObject<Partener>();
            furnizor.Cod = MarcajDsc + "-FURN";
            furnizor.Denumire = "Furnizor probă DSC";
            var client = os.CreateObject<Partener>();
            client.Cod = MarcajDsc + "-CL";
            client.Denumire = "Client probă DSC"; // fără ContImplicit → creanța pe fallback 4111
            Produs CreeazaProdus(string sufix, TipMaterial tip) {
                var p = os.CreateObject<Produs>();
                p.Cod = MarcajDsc + sufix;
                p.Denumire = "Produs probă DSC" + sufix;
                p.UM = "BUC";
                p.TipMaterial = tip;
                return p;
            }
            var produsA = CreeazaProdus("-A", tip371); // marfă
            var produsB = CreeazaProdus("-B", tip371); // marfă fără stoc inițial
            var produsC = CreeazaProdus("-C", tip345); // produs finit
            os.CommitChanges();

            List<PostareScena> NoteCub(Document doc) => CubScena.Note(os, doc.ID).Where(p => !p.Storno).ToList();

            // --- NIR-uri manuale la DATE diferite (FIFO determinist pe Lot.Data) ---
            var d1 = new DateOnly(2026, 4, 1);
            var d2 = new DateOnly(2026, 4, 5);
            var nir1 = os.CreateObject<NIR>();
            nir1.Data = d1; nir1.Predator = furnizor; nir1.Primitor = mag1;
            var linA1 = os.CreateObject<DocumentDetaliu>();
            linA1.Document = nir1; linA1.TipMaterial = tip371; linA1.Cantitate = 10m; linA1.Valoare = 50m;
            var lotA1 = linA1.CreeazaLot(os, produsA, mag1); // 5 lei/buc
            var linC1 = os.CreateObject<DocumentDetaliu>();
            linC1.Document = nir1; linC1.TipMaterial = tip345; linC1.Cantitate = 5m; linC1.Valoare = 40m;
            var lotC1 = linC1.CreeazaLot(os, produsC, mag1); // 8 lei/buc
            os.CommitChanges();
            MotorOperare.Opereaza(os, nir1);

            var nir2 = os.CreateObject<NIR>();
            nir2.Data = d2; nir2.Predator = furnizor; nir2.Primitor = mag1;
            var linA2 = os.CreateObject<DocumentDetaliu>();
            linA2.Document = nir2; linA2.TipMaterial = tip371; linA2.Cantitate = 10m; linA2.Valoare = 60m;
            var lotA2 = linA2.CreeazaLot(os, produsA, mag1); // 6 lei/buc (mai scump, mai nou)
            os.CommitChanges();
            MotorOperare.Opereaza(os, nir2);
            s.Check("Loturi de marfă finalizate la NET (A1=5, A2=6/buc, în Marfuri) + C1=8 (Magazie)",
                lotA1.PretUnitar == 5m && lotA2.PretUnitar == 6m && lotC1.PretUnitar == 8m
                && CubScena.Sold(os, lotA1.ID, mag1.ID, tip371.ContImplicitId).Cantitate == 10m
                && CubScena.Sold(os, lotA2.ID, mag1.ID, tip371.ContImplicitId).Cantitate == 10m
                && CubScena.Sold(os, lotC1.ID, mag1.ID, tip345.ContImplicitId).Cantitate == 5m);

            // --- Refuzuri prealabile ale culegerii FCL (General!+Specific?, §4) ---
            void RefuzFcl(string nume, Gestiune gest, Produs prod, Lot pin) {
                var f = os.CreateObject<FacturaIesire>();
                f.Data = new DateOnly(2026, 4, 8); f.Predator = sediu; f.Primitor = client;
                f.GestiuneDescarcare = gest;
                var d = os.CreateObject<FacturaIesireDetaliu>();
                d.Document = f; d.TipMaterial = tip371; d.Cantitate = 1m; d.PretUnitar = 10m; d.TipTva = n21;
                d.Produs = prod;
                if (pin != null) d.Lot = pin;
                os.CommitChanges();
                s.CheckRefuza(nume, () => MotorOperare.Opereaza(os, f));
                os.Delete(f.Detalii.ToList());
                os.Delete(f);
                os.CommitChanges();
            }
            RefuzFcl("Linie de stoc fără ProdusId → refuz (identitatea liniei = produsul)", mag1, null, null);
            RefuzFcl("FCL cu linii de stoc fără GestiuneDescarcare → refuz", null, produsA, null);
            RefuzFcl("Pin pe lot care NU aparține produsului liniei → refuz", mag1, produsA, lotC1);
            RefuzFcl("Pin pe lot fără sold în gestiunea de descărcare (MAG2 goală) → refuz «întâi transfer (BTR)»", mag2, produsA, lotA1);

            // ═══ 57f (104f): ștergerea de linie de draft, văzută de gardian ═══
            //
            // ModelCheck n-are gardianul înregistrat (provider standalone), deci proba
            // cheamă `GardianEditare.Verifica` din `Committing`-ul acestui ObjectSpace:
            // același obiect, aceeași fază.
            {
                var fSters = os.CreateObject<FacturaIesire>();
                fSters.Data = new DateOnly(2026, 4, 8);
                fSters.Predator = sediu; fSters.Primitor = client; fSters.GestiuneDescarcare = mag1;
                var lRamane = os.CreateObject<FacturaIesireDetaliu>();
                lRamane.Document = fSters; lRamane.TipMaterial = tip371; lRamane.Cantitate = 1m;
                lRamane.PretUnitar = 10m; lRamane.TipTva = n21; lRamane.Produs = produsA;
                var lSters = os.CreateObject<FacturaIesireDetaliu>();
                lSters.Document = fSters; lSters.TipMaterial = tip371; lSters.Cantitate = 2m;
                lSters.PretUnitar = 10m; lSters.TipTva = n21; lSters.Produs = produsA;
                os.CommitChanges();
                var idSters = lSters.ID;

                bool? peLista = null, dupaIsObjectToDelete = null;
                string refuzGardian = null;
                void LaCommittingSters(object _, System.ComponentModel.CancelEventArgs __) {
                    peLista = os.ModifiedObjects.OfType<FacturaIesireDetaliu>().Any(l => l.ID == idSters);
                    dupaIsObjectToDelete = os.IsObjectToDelete(lSters);
                    try { GardianEditare.Verifica(os); }
                    catch (OperareException e) { refuzGardian = e.Message.Split('\n')[0]; }
                }
                os.Committing += LaCommittingSters;
                os.Delete(lSters);
                os.CommitChanges();
                os.Committing -= LaCommittingSters;

                var maiEVizibila = os.GetObjectsQuery<FacturaIesireDetaliu>().Any(l => l.ID == idSters);
                var randuriFizice = ((DevExpress.ExpressApp.EFCore.EFCoreObjectSpace)os).DbContext.Database
                    .SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM \"DocumentDetalii\" WHERE \"ID\" = {idSters}")
                    .Single();
                s.Check("57f (104f): ștergerea unei linii de draft ajunge la `Committing` ca `Deleted`, "
                    + "`GardianEditare` o tace, iar după commit rândul dispare FIZIC",
                    peLista == true && dupaIsObjectToDelete == true
                    && refuzGardian == null && randuriFizice == 0 && !maiEVizibila);

                // Artefact de scenă ⇒ purjă FIZICĂ, ca restul curățeniilor (F13-D2).
                new Purja(os).Adauga(fSters.Detalii.ToList()).Adauga(lSters).Adauga(fSters).Executa();
            }

            // --- FCL: L1 FIFO(A,12), L2 pin(A,A2,5), L3 serviciu, L4 indisponibil(B,7), L5(C,3) ---
            var d3 = new DateOnly(2026, 4, 10);
            var fcl = os.CreateObject<FacturaIesire>();
            fcl.Data = d3; fcl.Predator = sediu; fcl.Primitor = client;
            fcl.GestiuneDescarcare = mag1;
            FacturaIesireDetaliu LinieFcl(TipMaterial tip, Produs prod, decimal cant, decimal pret, Lot pin = null) {
                var d = os.CreateObject<FacturaIesireDetaliu>();
                d.Document = fcl; d.TipMaterial = tip; d.Produs = prod; d.Cantitate = cant; d.PretUnitar = pret; d.TipTva = n21;
                if (pin != null) d.Lot = pin;
                return d;
            }
            var lFclA = LinieFcl(tip371, produsA, 12m, 10m);           // L1 FIFO → net 120
            var lFclApin = LinieFcl(tip371, produsA, 5m, 10m, lotA2);  // L2 pin A2 → net 50
            var lFclServ = LinieFcl(tip704, null, 1m, 100m);           // L3 serviciu → net 100
            var lFclB = LinieFcl(tip371, produsB, 7m, 9m);             // L4 indisponibil → net 63
            var lFclC = LinieFcl(tip345, produsC, 3m, 20m);            // L5 produs finit → net 60
            os.CommitChanges();

            var dscDraft = MotorOperare.Opereaza(os, fcl);

            // Notele FCL: venitul se postează ACUM (preț de vânzare), inclusiv pentru
            // poziția indisponibilă B; costul vine separat pe DSC.
            var noteFclCub = NoteCub(fcl);
            PostareScena VenitCub(FacturaIesireDetaliu l) => noteFclCub.Single(p => p.LinieId == l.ID && p.Credit && p.Cont != cont4427.ID);
            s.Check("FCL note: 4111=707 net pe marfă L1/L2/L4 (venitul se postează ACUM, inclusiv fără stoc)",
                noteFclCub.Nota(cont4111.ID, cont707.ID, 120m, lFclA.ID) && VenitCub(lFclA).Valoare == 120m
                && VenitCub(lFclApin).Cont == cont707.ID && VenitCub(lFclApin).Valoare == 50m
                && VenitCub(lFclB).Cont == cont707.ID && VenitCub(lFclB).Valoare == 63m);
            s.Check("FCL note: 4111=701 pe produsul finit L5 (60), 4111=704 pe serviciul L3 (100)",
                VenitCub(lFclC).Cont == cont701.ID && VenitCub(lFclC).Valoare == 60m
                && VenitCub(lFclServ).Cont == tip704.ContImplicitId && VenitCub(lFclServ).Valoare == 100m);
            s.Check("FCL: 4427 colectat per linie (N21); 10 note total (5 venit + 5 TVA)",
                noteFclCub.Where(p => p.Credit && p.Cont == cont4427.ID).Count(c => noteFclCub.Nota(cont4111.ID, cont4427.ID, c.Valoare, c.LinieId)) == 5);

            // Opereaza întoarce DSC-ul draft (secundar); spargerea pe loturi la GENERARE.
            var dsc = (DescarcareGestiune)dscDraft;
            var dd = dsc.Detalii.OfType<DescarcareGestiuneDetaliu>().ToList();
            s.Check("Conex DSC: draft autogenerat, DocumentSursa=FCL, predator=gestiune, primitor=client",
                dsc.Stare == StareDocument.Draft && dsc.Autogenerat && dsc.DocumentSursaId == fcl.ID
                && dsc.PredatorId == mag1.ID && dsc.PrimitorId == client.ID);
            var linL2 = dd.Where(x => x.LinieSursaId == lFclApin.ID).ToList();
            s.Check("Spargere PIN ÎNTÂI: L2 → 1 rând lot A2, 5 buc, cost 30",
                linL2.Count == 1 && linL2[0].LotId == lotA2.ID && linL2[0].Cantitate == 5m && linL2[0].Valoare == 30m);
            s.Check("Spargere FIFO: L1 → A1 10 buc (50) + A2 2 buc (12, din 10−5 rămase după pin)",
                dd.Count(x => x.LinieSursaId == lFclA.ID) == 2
                && dd.Any(x => x.LinieSursaId == lFclA.ID && x.LotId == lotA1.ID && x.Cantitate == 10m && x.Valoare == 50m)
                && dd.Any(x => x.LinieSursaId == lFclA.ID && x.LotId == lotA2.ID && x.Cantitate == 2m && x.Valoare == 12m));
            var linL5 = dd.Where(x => x.LinieSursaId == lFclC.ID).ToList();
            s.Check("Spargere: L5 → C1 3 buc (24); L4 indisponibil → nicio linie; total 4 rânduri, cost 116",
                linL5.Count == 1 && linL5[0].LotId == lotC1.ID && linL5[0].Cantitate == 3m && linL5[0].Valoare == 24m
                && !dd.Any(x => x.LinieSursaId == lFclB.ID) && dd.Count == 4 && dd.Sum(x => x.Valoare) == 116m);

            var resturi = DescarcareService.RestNedescarcat(os, fcl);
            s.Check("RestNedescarcat (cusătura §2.2): L4 rest 7, celelalte 0",
                resturi.Single(x => x.LinieId == lFclB.ID).RestNeacoperit == 7m
                && resturi.Where(x => x.LinieId != lFclB.ID).All(x => x.RestNeacoperit == 0m));

            // --- Operare DSC: cost 6xx=3xx (≠ vânzarea), −stoc pe gestiune, dim ambele laturi pe gestiune ---
            s.Check("DSC nu generează conex", MotorOperare.Opereaza(os, dsc) == null);
            s.Check("DSC operat cu număr din politică", dsc.Stare == StareDocument.Operat && dsc.Numar?.StartsWith("DSC-") == true);
            var noteDscCub = NoteCub(dsc);
            s.Check("Cost marfă: 607 = 371 la COST 92 (30+50+12) — decuplat de vânzarea 707 (233)",
                noteDscCub.Rulaj(cont607.ID, N.Latura.Debit) == 92m
                && noteDscCub.Where(p => p.Debit && p.Cont == cont607.ID).All(d => noteDscCub.Any(c => c.Credit
                    && c.Cont == tip371.ContImplicitId && c.LinieId == d.LinieId && c.Valoare == d.Valoare))
                && noteFclCub.Rulaj(cont707.ID, N.Latura.Credit) == 233m);
            s.Check("Cost produs finit: 711 = 345, 24; exact 4 note pe DSC",
                noteDscCub.Count(p => p.Debit && p.Cont == cont711.ID) == 1
                && noteDscCub.Nota(cont711.ID, tip345.ContImplicitId, 24m));
            s.Check("D9-A10 DSC: creditul de stoc poartă gestiunea predatoare, debitul de cost clientul primitor (T-D13 g); materialul din lot",
                noteDscCub.Where(p => p.Credit).All(p => p.Gestiune == mag1.ID)
                && noteDscCub.Where(p => p.Debit).All(p => p.Repartitor == client.ID)
                && noteDscCub.Where(p => p.Debit && p.Cont == cont607.ID).All(p => p.Produs == produsA.ID)
                && noteDscCub.Single(p => p.Debit && p.Cont == cont711.ID).Produs == produsC.ID);
            var stocDscCub = CubScena.Stoc(os, dsc.ID);
            s.Check("DSC stoc: −10 A1, −7 A2 (Marfuri), −3 C1 (Magazie), toate pe gestiune",
                stocDscCub.Where(p => p.Unitate == lotA1.ID).Sum(p => p.Cantitate) == -10m
                && stocDscCub.Where(p => p.Unitate == lotA2.ID).Sum(p => p.Cantitate) == -7m
                && stocDscCub.Where(p => p.Unitate == lotC1.ID).Sum(p => p.Cantitate) == -3m
                && stocDscCub.Where(p => p.Unitate == lotA1.ID || p.Unitate == lotA2.ID).All(p => p.Cont == tip371.ContImplicitId)
                && stocDscCub.Single(p => p.Unitate == lotC1.ID).Cont == tip345.ContImplicitId
                && stocDscCub.All(p => p.Gestiune == mag1.ID));

            // --- Backorder (§5): recepția produsului B, apoi Genereaza direct (acțiunea manuală) ---
            var d4 = new DateOnly(2026, 4, 15);
            var nir3 = os.CreateObject<NIR>();
            nir3.Data = d4; nir3.Predator = furnizor; nir3.Primitor = mag1;
            var linB1 = os.CreateObject<DocumentDetaliu>();
            linB1.Document = nir3; linB1.TipMaterial = tip371; linB1.Cantitate = 7m; linB1.Valoare = 28m;
            var lotB1 = linB1.CreeazaLot(os, produsB, mag1); // 4 lei/buc
            os.CommitChanges();
            MotorOperare.Opereaza(os, nir3);

            var dsc2 = DescarcareService.Genereaza(os, fcl, d4);
            os.CommitChanges();
            var db2 = dsc2?.Detalii.OfType<DescarcareGestiuneDetaliu>().SingleOrDefault();
            s.Check("Backorder: DSC₂ autogenerat DOAR pe linia B (7 buc, cost 28), DocumentSursa=FCL",
                dsc2 != null && dsc2.Autogenerat && dsc2.DocumentSursaId == fcl.ID && dsc2.Detalii.Count == 1
                && db2 != null && db2.LinieSursaId == lFclB.ID && db2.LotId == lotB1.ID && db2.Cantitate == 7m && db2.Valoare == 28m);
            s.Check("A doua apelare Genereaza → null (acoperirea completă; draftul contează)",
                DescarcareService.Genereaza(os, fcl, d4) == null);
            MotorOperare.Opereaza(os, dsc2);
            var n2Cub = NoteCub(dsc2);
            s.Check("DSC₂ operat: 607 = 371 la costul B (28)",
                n2Cub.Count == 2 && n2Cub.Nota(cont607.ID, tip371.ContImplicitId, 28m));

            // --- Gardieni de grup + storno care redeschide restul ---
            var d5 = new DateOnly(2026, 4, 20);
            s.CheckRefuza("Anularea FCL cu DSC operat → refuz", () => MotorOperare.AnuleazaOperarea(os, fcl));
            s.CheckRefuza("Stornarea FCL cu DSC operat → refuz", () => MotorOperare.Storneaza(os, fcl, d5));
            MotorOperare.Storneaza(os, dsc2, d5);
            s.Check("Storno DSC₂ → Stornat, B1 revenit în stoc",
                dsc2.Stare == StareDocument.Stornat
                && CubScena.Sold(os, lotB1.ID, mag1.ID, tip371.ContImplicitId, d5).Cantitate == 7m);
            var dsc3 = DescarcareService.Genereaza(os, fcl, d5);
            var db3 = dsc3?.Detalii.OfType<DescarcareGestiuneDetaliu>().SingleOrDefault();
            s.Check("Stornat NU acoperă: Genereaza redeschide restul L4 (DSC₃, B1 7 buc)",
                dsc3 != null && dsc3.Detalii.Count == 1 && db3 != null
                && db3.LinieSursaId == lFclB.ID && db3.LotId == lotB1.ID && db3.Cantitate == 7m);
            os.Delete(dsc3.Detalii.ToList());
            os.Delete(dsc3);
            os.CommitChanges();

            // --- Anularea cu draft: operarea unei FCL noi generează un DSC draft; anularea îl șterge ---
            var d6 = new DateOnly(2026, 4, 25);
            var fclMini = os.CreateObject<FacturaIesire>();
            fclMini.Data = d6; fclMini.Predator = sediu; fclMini.Primitor = client;
            fclMini.GestiuneDescarcare = mag1;
            var lMini = os.CreateObject<FacturaIesireDetaliu>();
            lMini.Document = fclMini; lMini.TipMaterial = tip371; lMini.Produs = produsA;
            lMini.Cantitate = 2m; lMini.PretUnitar = 10m; lMini.TipTva = n21;
            os.CommitChanges();
            var dscMini = MotorOperare.Opereaza(os, fclMini);
            s.Check("FCL₂ operată generează un DSC draft autogenerat",
                dscMini is DescarcareGestiune { Stare: StareDocument.Draft, Autogenerat: true }
                && os.GetObjectsQuery<DescarcareGestiune>().Any(x => x.DocumentSursaId == fclMini.ID));
            MotorOperare.AnuleazaOperarea(os, fclMini);
            s.Check("Anularea FCL₂ ȘTERGE DSC-ul draft autogenerat (gardianul existent); FCL₂ pe Draft",
                fclMini.Stare == StareDocument.Draft
                && !os.GetObjectsQuery<DescarcareGestiune>().Any(x => x.DocumentSursaId == fclMini.ID));

            // --- Datoria P1: default TipTva de CULEGERE (N21); culegerea explicită bate ---
            var fclTva = os.CreateObject<FacturaIesire>();
            fclTva.Data = d6; fclTva.Predator = sediu; fclTva.Primitor = client;
            var lNoTva = os.CreateObject<FacturaIesireDetaliu>();
            lNoTva.Document = fclTva; lNoTva.TipMaterial = tip704; lNoTva.Cantitate = 1m; lNoTva.PretUnitar = 10m;
            var lExplicit = os.CreateObject<FacturaIesireDetaliu>();
            lExplicit.Document = fclTva; lExplicit.TipMaterial = tip704; lExplicit.Cantitate = 1m; lExplicit.PretUnitar = 10m; lExplicit.TipTva = sdd;
            Atlas.Conta.BackOffice.Module.Culegere.CulegereDocument.Normalizeaza(os, fclTva);
            s.Check("Default TipTva: linia fără TipTva primește N21; linia cu SDD explicit rămâne neatinsă",
                lNoTva.TipTvaId == n21.ID && lExplicit.TipTvaId == sdd.ID);
            os.CommitChanges();

            // --- Defecte găsite la review advers (P2): validări noi de integritate ---
            // Fiecare pe obiecte throwaway, șterse imediat (tipul nou nu e prins de
            // markerul de doc al CurataDsc — se șterge explicit).

            // Defect 1: Tip de stoc creat DUPĂ seed → fără regulă de contare derivată.
            var tipNou = os.CreateObject<TipMaterial>();
            tipNou.Cod = MarcajDsc + "-TIPNOU";
            tipNou.Denumire = "Tip de stoc nou (post-seed, fără reguli)";
            tipNou.Clasa = tip371.Clasa; // MF (Natura=Stoc) — dar fără rând de vânzare/cost
            var produsNou = CreeazaProdus("-N", tipNou);
            os.CommitChanges();

            var fclDef1 = os.CreateObject<FacturaIesire>();
            fclDef1.Data = d6; fclDef1.Predator = sediu; fclDef1.Primitor = client; fclDef1.GestiuneDescarcare = mag1;
            var lFclDef1 = os.CreateObject<FacturaIesireDetaliu>();
            lFclDef1.Document = fclDef1; lFclDef1.TipMaterial = tipNou; lFclDef1.Produs = produsNou;
            lFclDef1.Cantitate = 1m; lFclDef1.PretUnitar = 10m; lFclDef1.TipTva = n21;
            os.CommitChanges();
            s.CheckRefuza("Defect 1 (FCL): linie de stoc cu Tip nou fără regulă de vânzare → refuz",
                () => MotorOperare.Opereaza(os, fclDef1));
            os.Delete(fclDef1.Detalii.ToList());
            os.Delete(fclDef1);
            os.CommitChanges();

            // Simetric pe DSC manual: Tip fără regulă de cost (ar mișca stoc fără notă).
            var lotNou = os.CreateObject<Lot>();
            lotNou.Produs = produsNou; lotNou.Gestiune = mag1; lotNou.PretUnitar = 5m; lotNou.Data = d1;
            var dscDef1 = os.CreateObject<DescarcareGestiune>();
            dscDef1.Data = d6; dscDef1.Predator = mag1; dscDef1.Primitor = client;
            var lDscDef1 = os.CreateObject<DescarcareGestiuneDetaliu>();
            lDscDef1.Document = dscDef1; lDscDef1.TipMaterial = tipNou; lDscDef1.Lot = lotNou; lDscDef1.Cantitate = 1m;
            os.CommitChanges();
            s.CheckRefuza("Defect 1 (DSC manual): linie cu Tip fără regulă de cost → refuz",
                () => MotorOperare.Opereaza(os, dscDef1));
            os.Delete(dscDef1.Detalii.ToList());
            os.Delete(dscDef1);
            os.Delete(lotNou);
            os.Delete(produsNou);
            os.Delete(tipNou);
            os.CommitChanges();

            // F24-P8 (felia 24, track B): axa pe care gardurile 38c NU o aveau — o
            // regulă de contare EXACTĂ pe Tip, dar cu `SemnFiltru` nepotrivit liniei,
            // e scoasă din joc de motor la TOATE nivelurile (F24-P1), deci linia ar
            // mișca stocul fără notă; gardul, care acum întreabă `Potrivire`, o vede
            // exact ca motorul (64).
            var tipSemn = os.CreateObject<TipMaterial>();
            tipSemn.Cod = MarcajDsc + "-TIPSEMN";
            tipSemn.Denumire = "Tip de stoc cu regulă pe semnul greșit";
            tipSemn.Clasa = tip371.Clasa;
            var produsSemn = CreeazaProdus("-S", tipSemn);
            RegulaContare RegulaSemnGresit(string codTip) {
                var regula = os.CreateObject<RegulaContare>();
                regula.TipDocument = os.FirstOrDefault<TipDocument>(t => t.Cod == codTip);
                regula.TipMaterial = tipSemn;
                regula.SemnFiltru = -1;
                regula.SursaContDebit = SursaCont.RepartitorPrimitor;
                regula.SursaContCredit = SursaCont.TipMaterial;
                return regula;
            }
            var regulaFclSemn = RegulaSemnGresit("FCL");
            var regulaDscSemn = RegulaSemnGresit("DSC");
            os.CommitChanges();

            var fclSemn = os.CreateObject<FacturaIesire>();
            fclSemn.Data = d6; fclSemn.Predator = sediu; fclSemn.Primitor = client; fclSemn.GestiuneDescarcare = mag1;
            var lFclSemn = os.CreateObject<FacturaIesireDetaliu>();
            lFclSemn.Document = fclSemn; lFclSemn.TipMaterial = tipSemn; lFclSemn.Produs = produsSemn;
            lFclSemn.Cantitate = 1m; lFclSemn.PretUnitar = 10m; lFclSemn.TipTva = n21;
            os.CommitChanges();
            s.Check("F24-P8 (FCL): regula de contare EXACTĂ pe Tip cu `SemnFiltru = −1` NU e o potrivire pentru linia "
                + "POZITIVĂ (motorul o exclude înaintea nivelurilor de specificitate) ⇒ refuzul 38c, ca și cum regula "
                + "ar lipsi — axa de semn pe care gardul n-o avea",
                s.Refuz(() => MotorOperare.Opereaza(os, fclSemn))
                    ?.Contains("nu are regulă de contare de vânzare pentru Tipul ei") == true);

            var lotSemn = os.CreateObject<Lot>();
            lotSemn.Produs = produsSemn; lotSemn.Gestiune = mag1; lotSemn.PretUnitar = 5m; lotSemn.Data = d1;
            var dscSemn = os.CreateObject<DescarcareGestiune>();
            dscSemn.Data = d6; dscSemn.Predator = mag1; dscSemn.Primitor = client;
            var lDscSemn = os.CreateObject<DescarcareGestiuneDetaliu>();
            lDscSemn.Document = dscSemn; lDscSemn.TipMaterial = tipSemn; lDscSemn.Lot = lotSemn; lDscSemn.Cantitate = 1m;
            os.CommitChanges();
            s.Check("F24-P8 (DSC manual): aceeași regulă cu semnul greșit ⇒ refuzul 38c de cost, nu o linie care mișcă "
                + "stocul fără notă",
                s.Refuz(() => MotorOperare.Opereaza(os, dscSemn))
                    ?.Contains("nu are regulă de contare de cost pentru Tipul ei") == true);

            // F13-D2: fixtura e artefact de scenă — purjă FIZICĂ.
            new Purja(os)
                .Adauga(fclSemn.Detalii.ToList())
                .Adauga(dscSemn.Detalii.ToList())
                .Adauga(fclSemn)
                .Adauga(dscSemn)
                .Adauga(lotSemn)
                .Adauga(produsSemn)
                .Adauga(regulaFclSemn)
                .Adauga(regulaDscSemn)
                .Adauga(tipSemn)
                .Executa();

            // Defect 4: produs de ALT Tip decât Tipul liniei (linie tip371 × produsC/345).
            RefuzFcl("Defect 4 (FCL): linie de stoc cu produs de alt Tip decât Tipul liniei → refuz", mag1, produsC, null);

            // Defect 7: linie de BAZĂ DocumentDetaliu (ne-derivată) pe FCL ar ocoli General!+Specific?.
            var fclDef7 = os.CreateObject<FacturaIesire>();
            fclDef7.Data = d6; fclDef7.Predator = sediu; fclDef7.Primitor = client;
            var lDef7 = os.CreateObject<DocumentDetaliu>(); // NU FacturaIesireDetaliu
            lDef7.Document = fclDef7; lDef7.TipMaterial = tip704; lDef7.Cantitate = 1m;
            os.CommitChanges();
            s.CheckRefuza("Defect 7 (FCL): linie de bază DocumentDetaliu (ne-derivată, «detaliu generic») → refuz",
                () => MotorOperare.Opereaza(os, fclDef7));
            os.Delete(fclDef7.Detalii.ToList());
            os.Delete(fclDef7);
            os.CommitChanges();

            // Defect 2: DSC manual (fără DocumentSursa) cu LinieSursaId spre o linie a
            // FCL-ului viu → refuz; ȘI RestNedescarcat filtrează pe DocumentSursa, deci
            // draftul STRĂIN nu otrăvește acoperirea L4 (rest 7 neschimbat) cât există.
            var poison = os.CreateObject<DescarcareGestiune>();
            poison.Data = d6; poison.Predator = mag1; poison.Primitor = client; // fără DocumentSursa
            var lPoison = os.CreateObject<DescarcareGestiuneDetaliu>();
            lPoison.Document = poison; lPoison.TipMaterial = tip371; lPoison.Lot = lotA1;
            lPoison.Cantitate = 7m; lPoison.LinieSursaId = lFclB.ID;
            os.CommitChanges();
            s.Check("Defect 2: draftul străin (alt DocumentSursa) NU otrăvește acoperirea — L4 rest rămâne 7",
                DescarcareService.RestNedescarcat(os, fcl).Single(x => x.LinieId == lFclB.ID).RestNeacoperit == 7m);
            s.CheckRefuza("Defect 2: DSC manual (fără DocumentSursa) cu LinieSursaId → refuz",
                () => MotorOperare.Opereaza(os, poison));
            os.Delete(poison.Detalii.ToList());
            os.Delete(poison);
            os.CommitChanges();

            CurataDsc(os);
            s.Check("Curățenie finală DSC (fără reziduuri e2e)",
                !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajDsc))
                && !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(MarcajDsc)));
        }
    }
}
