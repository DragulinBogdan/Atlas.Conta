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

// ================= Scenariul e2e 1C-a: Asamblare (ASM) =================
// Kitting n→m pe stoc (design FAZA 1C §7): consumurile descarcă loturi
// EXISTENTE (preț lot × cantitate, pattern BCS), liniile de produs CREEAZĂ
// lot cu valoare explicită (PretEvaluare, ca LDI-plus), iar invariantul
// Σ produse = Σ consumuri ține valoarea în patrimoniu. Direcția explicită se
// materializează în SEMN (mecanismul LDI 28a) peste UN SINGUR set de reguli
// de stoc (+1 pe predator). Stocul se mișcă FĂRĂ notă contabilă (23c:
// marfă→marfă la sintetic = zgomot) — verificat explicit.
// Luna noiembrie 2026 e nefolosită de celelalte blocuri.
static class E2eAsamblare {
    public static void Ruleaza(Suita s) {
        {
            const string MarcajAsm = "E2E-ASM";

            void CurataAsm(IObjectSpace os) {
                // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
                var pj = new Purja(os);
                var repIds = os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajAsm)).Select(r => r.ID).ToList();
                var docs = os.GetObjectsQuery<Document>()
                    .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
                var docIds = docs.Select(d => d.ID).ToList();
                ProbeCub.Purjeaza(pj, os, docIds);
                pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
                foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
                    pj.Adauga(doc);
                pj.Adauga(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod.StartsWith(MarcajAsm)).ToList());
                pj.Adauga(os.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(MarcajAsm)).ToList());
                pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajAsm)).ToList());
                pj.Executa();
            }

            using (var os = s.Provider.CreateObjectSpace()) {
                CurataAsm(os);

                var tip371 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "371"); // marfă (MF)
                var tipAsm = os.FirstOrDefault<TipDocument>(t => t.Cod == "ASM");

                // --- Seed ASM privat ---
                var reguliStocAsm = os.GetObjectsQuery<RegulaStoc>().Where(r => r.TipDocumentId == tipAsm.ID).ToList();
                s.Check("Seed ASM: ancoră TipDocument + numerotare ASM-; nicio regulă de stoc (D9-D8)",
                    tipAsm != null && tipAsm.ClrType == nameof(Asamblare)
                    && os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocumentId == tipAsm.ID)?.Serie == "ASM-"
                    && reguliStocAsm.Count == 0);
                s.Check("Seed ASM: FĂRĂ reguli de contare (marfă→marfă la sintetic = zgomot — 23c) și fără politici de TVA/scadență/validare",
                    !os.GetObjectsQuery<RegulaContare>().Any(r => r.TipDocumentId == tipAsm.ID)
                    && os.FirstOrDefault<PoliticaTva>(p => p.TipDocumentId == tipAsm.ID) == null
                    && os.FirstOrDefault<PoliticaScadenta>(p => p.TipDocumentId == tipAsm.ID) == null
                    && os.FirstOrDefault<PoliticaValidare>(p => p.TipDocumentId == tipAsm.ID) == null
                    && os.FirstOrDefault<PoliticaConex>(p => p.TipDocumentSursaId == tipAsm.ID) == null);

                // --- Fixtures: furnizor, gestiunea de lucru, 2 componente + kitul ---
                var furnizor = os.CreateObject<Partener>();
                furnizor.Cod = MarcajAsm + "-FURN";
                furnizor.Denumire = "Furnizor probă asamblare";
                var gestiune = os.CreateObject<Gestiune>();
                gestiune.Cod = MarcajAsm + "-G";
                gestiune.Denumire = "Gestiune probă asamblare";
                Produs ProdusAsm(string sufix) {
                    var p = os.CreateObject<Produs>();
                    p.Cod = MarcajAsm + sufix;
                    p.Denumire = "Produs probă asamblare" + sufix;
                    p.UM = "BUC";
                    p.TipMaterial = tip371; // kitting pe marfă (ca la flax)
                    return p;
                }
                var produsA = ProdusAsm("-A");
                var produsB = ProdusAsm("-B");
                var produsKit = ProdusAsm("-KIT");
                os.CommitChanges();

                List<PostareScena> StocCub(Document doc) => CubScena.Stoc(os, doc.ID).Where(p => !p.Storno).ToList();
                decimal SoldCub(Lot lot, DateOnly? laData = null) =>
                    CubScena.Sold(os, lot.ID, gestiune.ID, tip371.ContImplicitId, laData).Cantitate;

                // --- NIR manual: loturile componentelor (10 × 5 lei, 4 × 10 lei) ---
                var dNir = new DateOnly(2026, 11, 2);
                var nir = os.CreateObject<NIR>();
                nir.Data = dNir; nir.Predator = furnizor; nir.Primitor = gestiune;
                var linNirA = os.CreateObject<DocumentDetaliu>();
                linNirA.Document = nir; linNirA.TipMaterial = tip371; linNirA.Cantitate = 10m; linNirA.Valoare = 50m;
                var lotA = linNirA.CreeazaLot(os, produsA, gestiune);
                var linNirB = os.CreateObject<DocumentDetaliu>();
                linNirB.Document = nir; linNirB.TipMaterial = tip371; linNirB.Cantitate = 4m; linNirB.Valoare = 40m;
                var lotB = linNirB.CreeazaLot(os, produsB, gestiune);
                os.CommitChanges();
                MotorOperare.Opereaza(os, nir);
                s.Check("Precondiție ASM: loturile componentelor pe Marfuri (A 10 buc × 5, B 4 buc × 10)",
                    lotA.PretUnitar == 5m && lotB.PretUnitar == 10m
                    && SoldCub(lotA) == 10m
                    && SoldCub(lotB) == 4m);

                // Fabrica de linii: rolul + cantitatea POZITIVĂ culese, lotul fie
                // existent (consum), fie născut pe linie (produs).
                AsamblareDetaliu LinieAsm(Asamblare doc, DirectieAsamblare directie, decimal cantitate,
                    Lot lot = null, decimal? pretEvaluare = null, Produs produsNou = null, Gestiune gestiuneLot = null) {
                    var d = os.CreateObject<AsamblareDetaliu>();
                    d.Document = doc;
                    d.TipMaterial = tip371;
                    d.Directie = directie;
                    d.Cantitate = cantitate;
                    d.PretEvaluare = pretEvaluare;
                    if (lot != null)
                        d.Lot = lot;
                    if (produsNou != null)
                        d.CreeazaLot(os, produsNou, gestiuneLot ?? gestiune);
                    return d;
                }

                // --- Asamblarea: 6 × A (30) + 2 × B (20) → 2 kituri × 25 (50) ---
                var dAsm = new DateOnly(2026, 11, 5);
                var asm = os.CreateObject<Asamblare>();
                asm.Data = dAsm; asm.Predator = gestiune; asm.Primitor = gestiune; // aceeași gestiune
                var consumA = LinieAsm(asm, DirectieAsamblare.Consum, 6m, lotA);
                var consumB = LinieAsm(asm, DirectieAsamblare.Consum, 2m, lotB);
                var produsKitLinie = LinieAsm(asm, DirectieAsamblare.Produs, 2m, pretEvaluare: 25m, produsNou: produsKit);
                var lotKit = produsKitLinie.Lot;
                os.CommitChanges();

                s.Check("Asamblarea nu generează conex/secundar", MotorOperare.Opereaza(os, asm) == null);
                s.Check("Operare → Operat + număr din politică (ASM-)",
                    asm.Stare == StareDocument.Operat && asm.Numar?.StartsWith("ASM-") == true);
                s.Check("Semnarea direcției: consumurile devin negative (−6/−30, −2/−20), produsul rămâne pozitiv (+2/+50)",
                    consumA.Cantitate == -6m && consumA.Valoare == -30m
                    && consumB.Cantitate == -2m && consumB.Valoare == -20m
                    && produsKitLinie.Cantitate == 2m && produsKitLinie.Valoare == 50m);
                var stocAsmCub = StocCub(asm);
                s.Check("Stoc: EXACT 3 rânduri (−6/−30 A, −2/−20 B, +2/+50 kit) pe Marfuri, în gestiunea de lucru",
                    stocAsmCub.Count == 3
                    && stocAsmCub.Single(p => p.Unitate == lotA.ID) is { Cantitate: -6m, Semnata: -30m }
                    && stocAsmCub.Single(p => p.Unitate == lotB.ID) is { Cantitate: -2m, Semnata: -20m }
                    && stocAsmCub.Single(p => p.Unitate == lotKit.ID) is { Cantitate: 2m, Semnata: 50m }
                    && stocAsmCub.All(p => p.Cont == tip371.ContImplicitId && p.Gestiune == gestiune.ID));

                s.Check("Lotul kitului finalizat de motor: PretUnitar = valoarea alocată / cantitate (25), data documentului",
                    lotKit.PretUnitar == 25m && lotKit.Data == dAsm
                    && SoldCub(lotKit) == 2m
                    && SoldCub(lotA) == 4m
                    && SoldCub(lotB) == 2m);

                // --- Refuzuri (fiecare pe obiecte throwaway, curățate imediat) ---
                void RefuzAsm(string nume, Repartitor predator, Repartitor primitor, Action<Asamblare> linii) {
                    var doc = os.CreateObject<Asamblare>();
                    doc.Data = new DateOnly(2026, 11, 8);
                    doc.Predator = predator;
                    doc.Primitor = primitor;
                    linii(doc);
                    os.CommitChanges();
                    s.CheckRefuza(nume, () => MotorOperare.Opereaza(os, doc));
                    s.Check(nume + " — fără rânduri-fantomă în ObjectSpace (33d)",
                        CubScena.FaraStoc(os, doc.ID)
                        && CubScena.FaraNote(os, doc.ID));
                    var idsLinii = doc.Detalii.Select(d => d.ID).ToList();
                    os.Delete(doc.Detalii.ToList());
                    os.Delete(os.GetObjectsQuery<Lot>()
                        .Where(l => l.LinieIntrareId != null && idsLinii.Contains(l.LinieIntrareId.Value)).ToList());
                    os.Delete(doc);
                    os.CommitChanges();
                }

                RefuzAsm("Invariantul valoric picat (2 kituri × 30 = 60 ≠ 50 consumate) → refuz", gestiune, gestiune, doc => {
                    LinieAsm(doc, DirectieAsamblare.Consum, 6m, lotA);
                    LinieAsm(doc, DirectieAsamblare.Consum, 2m, lotB);
                    LinieAsm(doc, DirectieAsamblare.Produs, 2m, pretEvaluare: 30m, produsNou: produsKit);
                });
                RefuzAsm("Consum pe lotul creat de linia proprie → refuz", gestiune, gestiune, doc =>
                    LinieAsm(doc, DirectieAsamblare.Consum, 1m, produsNou: produsKit));
                // Fix post-review: lotul produs de ALTĂ linie a aceluiași document
                // are prețul nefinalizat (0) la validare — consumul lui ar lăsa
                // valoare orfană în registrul de stoc, cu invariantul satisfăcut.
                RefuzAsm("Consum pe lotul PRODUS de altă linie a aceluiași document → refuz (lanțul de kitting = documente separate)", gestiune, gestiune, doc => {
                    LinieAsm(doc, DirectieAsamblare.Consum, 2m, lotA);
                    var produsNouLinie = LinieAsm(doc, DirectieAsamblare.Produs, 1m, pretEvaluare: 10m, produsNou: produsKit);
                    LinieAsm(doc, DirectieAsamblare.Consum, 1m, produsNouLinie.Lot);
                });
                RefuzAsm("Produs pe lot STRĂIN (refolosit, nu născut pe linie) → refuz", gestiune, gestiune, doc => {
                    LinieAsm(doc, DirectieAsamblare.Consum, 2m, lotB);
                    LinieAsm(doc, DirectieAsamblare.Produs, 2m, lotA, pretEvaluare: 10m);
                });
                RefuzAsm("Direcție nesetată (default-ul enum-ului nu e valid) → refuz", gestiune, gestiune, doc => {
                    var d = LinieAsm(doc, DirectieAsamblare.Consum, 1m, lotA);
                    d.Directie = default; // linie culeasă fără rol
                });
                RefuzAsm("Linie de produs fără preț de evaluare → refuz", gestiune, gestiune, doc =>
                    LinieAsm(doc, DirectieAsamblare.Produs, 2m, produsNou: produsKit));
                RefuzAsm("Consum peste sold (A are 4 buc) → refuzul gardianului de sold", gestiune, gestiune, doc => {
                    LinieAsm(doc, DirectieAsamblare.Consum, 100m, lotA);
                    LinieAsm(doc, DirectieAsamblare.Produs, 2m, pretEvaluare: 250m, produsNou: produsKit);
                });
                RefuzAsm("Latură care nu e gestiune (predator = furnizor) → refuz", furnizor, gestiune, doc => {
                    LinieAsm(doc, DirectieAsamblare.Consum, 2m, lotA);
                    LinieAsm(doc, DirectieAsamblare.Produs, 1m, pretEvaluare: 10m, produsNou: produsKit);
                });
                RefuzAsm("Linie de bază DocumentDetaliu («detaliu generic») pe asamblare → refuz", gestiune, gestiune, doc => {
                    var d = os.CreateObject<DocumentDetaliu>(); // NU AsamblareDetaliu
                    d.Document = doc;
                    d.TipMaterial = tip371;
                    d.Lot = lotA;
                    d.Cantitate = 1m;
                });

                // --- Dezasamblarea = ACELAȘI tip (1 consum → n produse) ---
                var dDez = new DateOnly(2026, 11, 10);
                var dez = os.CreateObject<Asamblare>();
                dez.Data = dDez; dez.Predator = gestiune; dez.Primitor = gestiune;
                var consumKit = LinieAsm(dez, DirectieAsamblare.Consum, 1m, lotKit);           // −25
                var produsA2 = LinieAsm(dez, DirectieAsamblare.Produs, 1m, pretEvaluare: 10m, produsNou: produsA);  // +10
                var produsB2 = LinieAsm(dez, DirectieAsamblare.Produs, 1m, pretEvaluare: 15m, produsNou: produsB);  // +15
                var lotA2 = produsA2.Lot;
                var lotB2 = produsB2.Lot;
                os.CommitChanges();
                MotorOperare.Opereaza(os, dez);
                s.Check("Dezasamblare (1 kit → 2 componente): Σ produse (10+15) = Σ consum (25); loturi noi la prețurile alocate",
                    dez.Stare == StareDocument.Operat && consumKit.Valoare == -25m
                    && lotA2.PretUnitar == 10m && lotB2.PretUnitar == 15m);
                s.Check("Stoc după dezasamblare: kit 1 rămas, loturile componente noi cu 1 buc fiecare",
                    SoldCub(lotKit) == 1m
                    && SoldCub(lotA2) == 1m
                    && SoldCub(lotB2) == 1m);

                // --- Corecție directă → re-operare → storno ---
                var numarDez = dez.Numar;
                MotorOperare.AnuleazaOperarea(os, dez);
                s.Check("Anulare directă (dezasamblarea e frunză) → Draft + registre goale",
                    dez.Stare == StareDocument.Draft
                    && CubScena.FaraStoc(os, dez.ID)
                    && SoldCub(lotKit) == 2m);
                MotorOperare.Opereaza(os, dez);
                s.Check("Re-operare → același număr, aceleași 3 rânduri (semnarea e idempotentă)",
                    dez.Stare == StareDocument.Operat && dez.Numar == numarDez
                    && consumKit.Cantitate == -1m && consumKit.Valoare == -25m);
                var dStorno = new DateOnly(2026, 11, 20);
                MotorOperare.Storneaza(os, dez, dStorno);
                var stornoDezCub = CubScena.Stoc(os, dez.ID).Where(p => p.Storno).ToList();
                s.Check("Storno → 3 rânduri inverse (Storno=true) la data stornării; soldurile revin (kit 2, componentele noi 0)",
                    dez.Stare == StareDocument.Stornat
                    && stornoDezCub.Count(p => p.Data == dStorno) == 3
                    && stornoDezCub.Sum(p => p.Cantitate) == -1m
                    && SoldCub(lotKit, dStorno) == 2m
                    && SoldCub(lotA2, dStorno) == 0m);

                CurataAsm(os);
                s.Check("Curățenie finală asamblare (fără reziduuri e2e)",
                    !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajAsm))
                    && !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(MarcajAsm))
                    && !os.GetObjectsQuery<Asamblare>().Any());
            }
        }
    }
}
