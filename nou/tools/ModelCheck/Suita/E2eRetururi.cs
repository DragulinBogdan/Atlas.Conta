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

// ============ Scenariul e2e 1C-a: ReturFurnizor / ReturClient ============
// Corespondența de STORNO (design §7, rezoluția spike-ului): liniile se culeg
// POZITIVE, PregatesteOperare le semnează negativ, iar rândurile se postează
// pe corespondența ORIGINALĂ cu valori negative — FĂRĂ flag-ul `Storno`
// (ăla rămâne al meta-operației Storneaza: stornarea unui retur dă rânduri
// POZITIVE cu Storno=true). Singura extensie de motor e
// `RegulaContare.PastreazaSemn`. RDC = UN document cu linii pe două roluri
// (venit fără lot / cost cu lotul original), cu `Total` = doar venitul.
// Luna decembrie 2026 e nefolosită de celelalte blocuri.
static class E2eRetururi {
    public static void Ruleaza(Suita s) {
        {
            const string MarcajRet = "E2E-RET";

            void CurataRet(IObjectSpace os) {
                // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
                var pj = new Purja(os);
                var repIds = os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajRet)).Select(r => r.ID).ToList();
                var docs = os.GetObjectsQuery<Document>()
                    .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
                var docIds = docs.Select(d => d.ID).ToList();
                pj.Adauga(os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
                pj.Adauga(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
                pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
                foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
                    pj.Adauga(doc);
                pj.Adauga(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod.StartsWith(MarcajRet)).ToList());
                pj.Adauga(os.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(MarcajRet)).ToList());
                // Tipul „fără regulă" creat de proba de refuz intră în purja de scenă, pe marcaj (F13-D2).
                pj.Adauga(os.GetObjectsQuery<TipMaterial>().Where(t => t.Cod.StartsWith(MarcajRet)));
                pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajRet)).ToList());
                pj.Executa();
            }

            using (var os = s.Provider.CreateObjectSpace()) {
                CurataRet(os);

                var tip371 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "371");   // marfă (MF)
                var tip301 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "301");   // alt Tip de stoc (coerență)
                var tip707 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "707");   // venit din vânzarea mărfurilor
                var n21 = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
                var ti21 = os.FirstOrDefault<TipTva>(t => t.Cod == "TI21");   // F13-D1
                var tipRlf = os.FirstOrDefault<TipDocument>(t => t.Cod == "RLF");
                var tipRdc = os.FirstOrDefault<TipDocument>(t => t.Cod == "RDC");
                var cont401 = os.FirstOrDefault<Cont>(c => c.Simbol == "401");
                var cont4111 = os.FirstOrDefault<Cont>(c => c.Simbol == "4111");
                var cont4426 = os.FirstOrDefault<Cont>(c => c.Simbol == "4426");
                var cont4427 = os.FirstOrDefault<Cont>(c => c.Simbol == "4427");
                var cont371 = os.FirstOrDefault<Cont>(c => c.Simbol == "371");
                var cont607 = os.FirstOrDefault<Cont>(c => c.Simbol == "607");
                var cont707 = os.FirstOrDefault<Cont>(c => c.Simbol == "707");

                // --- Seed RLF/RDC privat ---
                var stocRlf = os.GetObjectsQuery<RegulaStoc>().Where(r => r.TipDocumentId == tipRlf.ID).ToList();
                var stocRdc = os.GetObjectsQuery<RegulaStoc>().Where(r => r.TipDocumentId == tipRdc.ID).ToList();
                s.Check("Seed RLF: ancoră + numerotare RLF-; stoc +1 pe PREDATOR (generic→Magazie, MF→Marfuri) — semnul liniei dă ieșirea",
                    tipRlf != null && tipRlf.ClrType == nameof(ReturFurnizor)
                    && os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocumentId == tipRlf.ID)?.Serie == "RLF-"
                    && stocRlf.Count == 2
                    && stocRlf.All(r => r.Latura == LaturaDocument.Predator && r.Semn == +1)
                    && stocRlf.Any(r => r.ClasaId == null && r.TipStoc == TipStoc.Magazie)
                    && stocRlf.Any(r => r.TipStoc == TipStoc.Marfuri));
                s.Check("Seed RDC: ancoră + numerotare RDC-; stoc −1 pe PRIMITOR (marfa REVINE pe lotul original)",
                    tipRdc != null && tipRdc.ClrType == nameof(ReturClient)
                    && os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocumentId == tipRdc.ID)?.Serie == "RDC-"
                    && stocRdc.Count == 2
                    && stocRdc.All(r => r.Latura == LaturaDocument.Primitor && r.Semn == -1)
                    && stocRdc.Any(r => r.ClasaId == null && r.TipStoc == TipStoc.Magazie)
                    && stocRdc.Any(r => r.TipStoc == TipStoc.Marfuri));
                var contareRlf = os.GetObjectsQuery<RegulaContare>().Where(r => r.TipDocumentId == tipRlf.ID).ToList();
                s.Check("Seed RLF: UN rând generic Natura=Stoc cu PastreazaSemn — 3xx (Tipul) = furnizor (fallback 401)",
                    contareRlf.Count == 1
                    && contareRlf[0] is { NaturaFiltru: NaturaClasa.Stoc, PastreazaSemn: true, SemnFiltru: null,
                        SursaContDebit: SursaCont.TipMaterial, SursaContCredit: SursaCont.RepartitorPrimitor }
                    && contareRlf[0].ContCreditId == cont401.ID && contareRlf[0].ContDebitId == null);
                var venitRdc = os.GetObjectsQuery<RegulaContare>()
                    .Where(r => r.TipDocumentId == tipRdc.ID && r.TipMaterialId == null).ToList();
                var costRdc = os.GetObjectsQuery<RegulaContare>()
                    .Where(r => r.TipDocumentId == tipRdc.ID && r.TipMaterialId == tip371.ID).ToList();
                s.Check("Seed RDC: rând generic de VENIT (Natura=Serviciu, PastreazaSemn) — client (fallback 4111) = contul Tipului, fără fallback",
                    venitRdc.Count == 1
                    && venitRdc[0] is { NaturaFiltru: NaturaClasa.Serviciu, PastreazaSemn: true, SemnFiltru: null,
                        SursaContDebit: SursaCont.RepartitorPredator, SursaContCredit: SursaCont.TipMaterial }
                    && venitRdc[0].ContDebitId == cont4111.ID && venitRdc[0].ContCreditId == null);
                s.Check("Seed RDC: costul REVINE — 6xx = 3xx per TipMaterial cu excepțiile profilului (607=371), PastreazaSemn",
                    costRdc.Count == 1
                    && costRdc[0] is { PastreazaSemn: true, SemnFiltru: null,
                        SursaContDebit: SursaCont.Explicit, SursaContCredit: SursaCont.TipMaterial }
                    && costRdc[0].ContDebitId == cont607.ID);
                var tvaRlf = os.FirstOrDefault<PoliticaTva>(p => p.TipDocumentId == tipRlf.ID);
                var tvaRdc = os.FirstOrDefault<PoliticaTva>(p => p.TipDocumentId == tipRdc.ID);
                s.Check("Seed: PoliticaTva pe retururi (RLF deductibil/contrapartidă primitor 401; RDC colectat/contrapartidă predator 4111) + TipTva implicit N21",
                    tvaRlf is { Directie: DirectieTva.Deductibil, SursaContrapartida: SursaCont.RepartitorPrimitor }
                    && tvaRlf.ContrapartidaFallbackId == cont401.ID
                    && tvaRdc is { Directie: DirectieTva.Colectat, SursaContrapartida: SursaCont.RepartitorPredator }
                    && tvaRdc.ContrapartidaFallbackId == cont4111.ID
                    && tipRlf.TipTvaImplicitId == n21.ID && tipRdc.TipTvaImplicitId == n21.ID);

                // --- Fixtures ---
                var furnizor = os.CreateObject<Partener>();
                furnizor.Cod = MarcajRet + "-FURN";
                furnizor.Denumire = "Furnizor probă retur";
                var client = os.CreateObject<Partener>();
                client.Cod = MarcajRet + "-CLI";
                client.Denumire = "Client probă retur";
                var gestiune = os.CreateObject<Gestiune>();
                gestiune.Cod = MarcajRet + "-G";
                gestiune.Denumire = "Gestiune probă retur";
                var produs = os.CreateObject<Produs>();
                produs.Cod = MarcajRet + "-P";
                produs.Denumire = "Marfă probă retur";
                produs.UM = "BUC";
                produs.TipMaterial = tip371;
                os.CommitChanges();

                List<PostareScena> StocCub(Document doc) => CubScena.Stoc(os, doc.ID).Where(p => !p.Storno).ToList();
                List<PostareScena> NoteCub(Document doc) => CubScena.Note(os, doc.ID).Where(p => !p.Storno).ToList();
                decimal SoldContCub(Cont cont) => CubScena.SoldCont(os, cont.ID);
                decimal SoldStocCub(Lot lot, DateOnly? la = null) =>
                    CubScena.Sold(os, lot.ID, gestiune.ID, tip371.ContImplicitId, la).Cantitate;

                // --- Contextul: NIR manual, 10 buc × 10 lei ---
                var nir = os.CreateObject<NIR>();
                nir.Data = new DateOnly(2026, 12, 2);
                nir.Predator = furnizor; nir.Primitor = gestiune;
                var linNir = os.CreateObject<DocumentDetaliu>();
                linNir.Document = nir; linNir.TipMaterial = tip371; linNir.Cantitate = 10m; linNir.Valoare = 100m;
                var lot = linNir.CreeazaLot(os, produs, gestiune);
                os.CommitChanges();
                MotorOperare.Opereaza(os, nir);
                s.Check("Precondiție retururi: lotul original pe Marfuri (10 buc × 10 lei)",
                    lot.PretUnitar == 10m && SoldStocCub(lot) == 10m);

                var sold4426InitialCub = SoldContCub(cont4426);
                var sold4427InitialCub = SoldContCub(cont4427);

                // --- RLF: 4 buc din lotul original, N21 (culegere POZITIVĂ) ---
                var dRlf = new DateOnly(2026, 12, 5);
                var rlf = os.CreateObject<ReturFurnizor>();
                rlf.Data = dRlf; rlf.Predator = gestiune; rlf.Primitor = furnizor;
                var linRlf = os.CreateObject<DocumentDetaliu>();
                linRlf.Document = rlf; linRlf.TipMaterial = tip371; linRlf.Lot = lot;
                linRlf.Cantitate = 4m; linRlf.TipTva = n21;
                os.CommitChanges();

                s.Check("RLF nu generează conex/secundar", MotorOperare.Opereaza(os, rlf) == null);
                s.Check("RLF → Operat + număr din politică (RLF-); linia culeasă pozitiv devine NEGATIVĂ (−4 / −40 / −8.4 TVA)",
                    rlf.Stare == StareDocument.Operat && rlf.Numar?.StartsWith("RLF-") == true
                    && linRlf.Cantitate == -4m && linRlf.Valoare == -40m && linRlf.ValoareTva == -8.4m);
                var stocRlfCub = StocCub(rlf);
                s.Check("RLF stoc: UN rând −4 / −40 pe Marfuri, în gestiunea predatoare (regula +1 × linia negativă)",
                    stocRlfCub.Count == 1
                    && stocRlfCub[0] is { Cantitate: -4m, Semnata: -40m }
                    && stocRlfCub[0].Cont == tip371.ContImplicitId && stocRlfCub[0].Gestiune == gestiune.ID
                    && SoldStocCub(lot) == 6m);
                var noteRlfCub = NoteCub(rlf);
                s.Check("RLF note: 371 = 401 cu −40 și 4426 = 401 cu −8.4, pe corespondența ORIGINALĂ și FĂRĂ flag Storno",
                    noteRlfCub.Nota(cont371.ID, cont401.ID, -40m)
                    && noteRlfCub.Nota(cont4426.ID, cont401.ID, -8.4m)
                    && noteRlfCub.All(p => p.Fel == N.FelTranzactie.Operare && p.Data == dRlf));
                s.Check("RLF: Total = brutul negativ (−48.4) — datoria către furnizor scade",
                    rlf.Total == -48.4m);
                s.Check("TVA deductibilă scade cu 8.4 (soldul 4426 după retur)",
                    SoldContCub(cont4426) - sold4426InitialCub == -8.4m);

                // --- Gardianul de sold: retur peste disponibil ---
                var pesteSold = os.CreateObject<ReturFurnizor>();
                pesteSold.Data = new DateOnly(2026, 12, 8);
                pesteSold.Predator = gestiune; pesteSold.Primitor = furnizor;
                var linPesteSold = os.CreateObject<DocumentDetaliu>();
                linPesteSold.Document = pesteSold; linPesteSold.TipMaterial = tip371; linPesteSold.Lot = lot;
                linPesteSold.Cantitate = 20m; linPesteSold.TipTva = n21;
                os.CommitChanges();
                s.CheckRefuza("Retur la furnizor peste sold (20 din 6) → refuzul gardianului de sold",
                    () => MotorOperare.Opereaza(os, pesteSold));
                s.Check("Retur refuzat — fără rânduri-fantomă (33d)",
                    CubScena.FaraStoc(os, pesteSold.ID)
                    && CubScena.FaraNote(os, pesteSold.ID));
                os.Delete(pesteSold.Detalii.ToList());
                os.Delete(pesteSold);
                os.CommitChanges();

                // --- RDC: UN document, linie de venit (fără lot) + linie de cost (lotul original) ---
                var dRdc = new DateOnly(2026, 12, 10);
                var rdc = os.CreateObject<ReturClient>();
                rdc.Data = dRdc; rdc.Predator = client; rdc.Primitor = gestiune;
                var linVenit = os.CreateObject<DocumentDetaliu>();
                linVenit.Document = rdc; linVenit.TipMaterial = tip707; linVenit.Valoare = 100m; linVenit.TipTva = n21;
                var linCost = os.CreateObject<DocumentDetaliu>();
                linCost.Document = rdc; linCost.TipMaterial = tip371; linCost.Lot = lot; linCost.Cantitate = 3m;
                // TipTva pus și pe linia de COST — nu e o ciudățenie de scenă, e EXACT
                // ce face calea de produs: `TipDocument.TipTvaImplicit` al lui RDC e
                // N21, iar culegerea îl punea pe ORICE linie nouă
                // culeasă în UI. Scena reproduce deci culegerea reală (review advers
                // D1 al feliei 11) — înainte de fix, linia asta intra în jurnalul de
                // TVA cu costul ca bază impozabilă.
                linCost.TipTva = n21;
                os.CommitChanges();

                MotorOperare.Opereaza(os, rdc);
                s.Check("RDC → Operat + număr RDC-; venitul semnat negativ (−100 / −21), costul negativ (−3 / −30), cantitatea de venit pro-formă pozitivă",
                    rdc.Stare == StareDocument.Operat && rdc.Numar?.StartsWith("RDC-") == true
                    && linVenit.Cantitate == 1m && linVenit.Valoare == -100m && linVenit.ValoareTva == -21m
                    && linCost.Cantitate == -3m && linCost.Valoare == -30m && linCost.ValoareTva == 0m);
                var stocRdcCub = StocCub(rdc);
                s.Check("RDC stoc: UN rând +3 / +30 pe Marfuri, în gestiunea primitoare — marfa REVINE pe lotul original (regula −1 × linia negativă)",
                    stocRdcCub.Count == 1
                    && stocRdcCub[0] is { Cantitate: 3m, Semnata: 30m }
                    && stocRdcCub[0].Cont == tip371.ContImplicitId
                    && stocRdcCub[0].Unitate == lot.ID && stocRdcCub[0].Gestiune == gestiune.ID
                    && SoldStocCub(lot) == 9m);
                var noteRdcCub = NoteCub(rdc);
                s.Check("RDC note: 4111 = 707 cu −100, 4111 = 4427 cu −21, 607 = 371 cu −30 — toate fără flag Storno",
                    noteRdcCub.Nota(cont4111.ID, cont707.ID, -100m)
                    && noteRdcCub.Nota(cont4111.ID, cont4427.ID, -21m)
                    && noteRdcCub.Nota(cont607.ID, cont371.ID, -30m)
                    && noteRdcCub.All(p => p.Fel == N.FelTranzactie.Operare && p.Data == dRdc));
                s.Check("RDC: Total = DOAR liniile de venit (−121) — costul e mișcare internă venit↔stoc, nu creanță",
                    rdc.Total == -121m
                    && rdc.Detalii.Sum(d => d.Valoare + d.ValoareTva) == -151m); // totalul „naiv" al bazei
                // ═══ REGRESIE, review advers D1 al feliei 11 ═══
                // Linia de cost NU e o operațiune taxabilă — e mișcare internă
                // venit↔stoc — dar culegerea îi pusese `TipTva` (implicitul tipului).
                // `PregatesteOperare` îi șterge acum IDENTITATEA fiscală, nu doar
                // valoarea, deci `RegistruTva` nu mai primește rând pentru ea.
                // Fără fix: jurnalul arăta pentru acest retur baza −130 (venitul −100
                // PLUS costul −30) cu TVA −21 — o bază umflată cu 30%, într-o cifră
                // care ajunge în D394. Iar cusătura JT-D6 NU putea s-o vadă:
                // contribuția liniei de cost la TVA e exact 0, deci proba se închidea
                // perfect pe un jurnal greșit. De-aia proba stă AICI, pe axa bazei.
                var fiscalRdcCub = CubScena.Fapte(os, rdc.ID);
                s.Check("REGRESIE D1 (felia 11): linia de COST a returului nu produce fapt fiscal — `PregatesteOperare` "
                    + "îi șterge `TipTva` (implicitul culegerii), nu doar `ValoareTva`. Registrul are UN singur rând, "
                    + "al liniei de VENIT, cu baza −100 și TVA −21 — nu −130/−21, cum ieșea înainte de fix",
                    linCost.TipTvaId == null
                    && fiscalRdcCub.Count == 1
                    && fiscalRdcCub[0].DetaliuId == linVenit.ID
                    && fiscalRdcCub[0] is { Baza: -100m, Tva: -21m, Sens: SensTva.Livrare });
                // 4427 e cont de PASIV: rândul −21 stă pe CREDIT, deci soldul creditor
                // (−SoldCont, unde SoldCont = debit − credit) scade cu 21.
                s.Check("TVA colectată (sold CREDITOR) scade cu 21 după retur",
                    -(SoldContCub(cont4427) - sold4427InitialCub) == -21m);

                // --- Idempotența semnării: anulare → re-operare ---
                var numarRdc = rdc.Numar;
                MotorOperare.AnuleazaOperarea(os, rdc);
                s.Check("Anulare directă RDC (frunză) → Draft, registre goale, stocul revine la 6",
                    rdc.Stare == StareDocument.Draft
                    && CubScena.FaraStoc(os, rdc.ID)
                    && CubScena.FaraNote(os, rdc.ID)
                    && SoldStocCub(lot) == 6m);
                MotorOperare.Opereaza(os, rdc);
                s.Check("Re-operare → același număr și ACELEAȘI valori (semnarea e idempotentă prin Abs)",
                    rdc.Stare == StareDocument.Operat && rdc.Numar == numarRdc
                    && linVenit.Valoare == -100m && linVenit.ValoareTva == -21m
                    && linCost.Cantitate == -3m && linCost.Valoare == -30m
                    && rdc.Total == -121m);

                // --- Refuzuri (obiecte throwaway, curățate imediat) ---
                void RefuzRet(string nume, Func<Document> fabrica) {
                    var doc = fabrica();
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

                ReturClient RdcNou() {
                    var doc = os.CreateObject<ReturClient>();
                    doc.Data = new DateOnly(2026, 12, 12);
                    doc.Predator = client; doc.Primitor = gestiune;
                    return doc;
                }
                DocumentDetaliu LinieRdc(ReturClient doc, TipMaterial tip, decimal cantitate, decimal valoare, Lot lotLinie) {
                    var d = os.CreateObject<DocumentDetaliu>();
                    d.Document = doc; d.TipMaterial = tip; d.Cantitate = cantitate; d.Valoare = valoare;
                    if (lotLinie != null)
                        d.Lot = lotLinie;
                    return d;
                }

                RefuzRet("RDC: linie de venit (fără lot) cu Tip de STOC → refuz (venitul poartă natura Serviciu)", () => {
                    var doc = RdcNou();
                    LinieRdc(doc, tip371, 1m, 100m, null);
                    return doc;
                });
                RefuzRet("RDC: linie de cost pe lotul creat de ea însăși → refuz (marfa revine pe lotul ORIGINAL)", () => {
                    var doc = RdcNou();
                    LinieRdc(doc, tip707, 1m, 100m, null);
                    var d = LinieRdc(doc, tip371, 2m, 0m, null);
                    d.CreeazaLot(os, produs, gestiune);
                    return doc;
                });
                RefuzRet("RDC: linie de cost cu Tip incoerent cu produsul lotului → refuz", () => {
                    var doc = RdcNou();
                    LinieRdc(doc, tip707, 1m, 100m, null);
                    LinieRdc(doc, tip301, 1m, 0m, lot); // lotul e al unui produs cu Tipul 371
                    return doc;
                });
                RefuzRet("RLF: laturi inversate (predator partener / primitor gestiune) → refuz", () => {
                    var doc = os.CreateObject<ReturFurnizor>();
                    doc.Data = new DateOnly(2026, 12, 12);
                    doc.Predator = furnizor; doc.Primitor = gestiune;
                    var d = os.CreateObject<DocumentDetaliu>();
                    d.Document = doc; d.TipMaterial = tip371; d.Lot = lot; d.Cantitate = 1m;
                    return doc;
                });
                RefuzRet("RLF: linie fără lot → refuz (returul descarcă lotul original)", () => {
                    var doc = os.CreateObject<ReturFurnizor>();
                    doc.Data = new DateOnly(2026, 12, 12);
                    doc.Predator = gestiune; doc.Primitor = furnizor;
                    var d = os.CreateObject<DocumentDetaliu>();
                    d.Document = doc; d.TipMaterial = tip371; d.Cantitate = 1m;
                    return doc;
                });
                // Fix post-review pas 4: un Tip de stoc creat ÎNTRE updater-e n-are
                // regulă derivată 607=3xx → fără refuz, stocul s-ar mișca fără notă
                // (exact defectul închis pe DSC la 38c). Lotul e „de deschidere"
                // (LinieIntrareId null, ca migrarea) ca să izoleze EXACT eroarea.
                var clasaMf = os.FirstOrDefault<ClasaProdus>(c => c.Cod == "MF");
                var tipNou = os.CreateObject<TipMaterial>();
                tipNou.Cod = MarcajRet + "-TIPX"; tipNou.Denumire = "Tip fără regulă (probă)"; tipNou.Clasa = clasaMf;
                var produsNou = os.CreateObject<Produs>();
                produsNou.Cod = MarcajRet + "-PRX"; produsNou.Denumire = "Produs Tip nou"; produsNou.UM = "BUC";
                produsNou.TipMaterial = tipNou;
                var lotNou = os.CreateObject<Lot>();
                lotNou.Produs = produsNou; lotNou.Gestiune = gestiune;
                lotNou.PretUnitar = 5m; lotNou.Data = new DateOnly(2026, 12, 1);
                os.CommitChanges();
                RefuzRet("RDC: linie de cost cu Tip FĂRĂ regulă de contare derivată → refuz (stocul nu se mișcă fără notă — 38c)", () => {
                    var doc = RdcNou();
                    LinieRdc(doc, tip707, 1m, 100m, null);
                    LinieRdc(doc, tipNou, 1m, 0m, lotNou);
                    return doc;
                });
                os.Delete(lotNou); os.Delete(produsNou); os.Delete(tipNou);
                os.CommitChanges();

                // F24-P8 (felia 24, track B): linia de COST a returului e NEGATIVĂ după
                // semnarea din `PregatesteOperare`, deci un `SemnFiltru = +1` pus pe
                // rândul exact al Tipului o ratează în motor. Gardul, care acum
                // întreabă `Potrivire`, o ratează la fel (64).
                var tipSemnRet = os.CreateObject<TipMaterial>();
                tipSemnRet.Cod = MarcajRet + "-TIPSEMN";
                tipSemnRet.Denumire = "Tip cu regulă pe semnul greșit (probă)";
                tipSemnRet.Clasa = clasaMf;
                var produsSemnRet = os.CreateObject<Produs>();
                produsSemnRet.Cod = MarcajRet + "-PRS"; produsSemnRet.Denumire = "Produs Tip semn"; produsSemnRet.UM = "BUC";
                produsSemnRet.TipMaterial = tipSemnRet;
                var lotSemnRet = os.CreateObject<Lot>();
                lotSemnRet.Produs = produsSemnRet; lotSemnRet.Gestiune = gestiune;
                lotSemnRet.PretUnitar = 5m; lotSemnRet.Data = new DateOnly(2026, 12, 1);
                var regulaSemnRet = os.CreateObject<RegulaContare>();
                regulaSemnRet.TipDocument = os.FirstOrDefault<TipDocument>(t => t.Cod == "RDC");
                regulaSemnRet.TipMaterial = tipSemnRet;
                regulaSemnRet.SemnFiltru = +1;
                regulaSemnRet.PastreazaSemn = true;
                regulaSemnRet.SursaContDebit = SursaCont.RepartitorPredator;
                regulaSemnRet.SursaContCredit = SursaCont.TipMaterial;
                os.CommitChanges();
                var rdcSemn = RdcNou();
                LinieRdc(rdcSemn, tip707, 1m, 100m, null);
                LinieRdc(rdcSemn, tipSemnRet, 1m, 0m, lotSemnRet);
                os.CommitChanges();
                s.Check("F24-P8 (RDC): regula de contare EXACTĂ pe Tip cu `SemnFiltru = +1` NU e o potrivire pentru linia "
                    + "de cost, negativă după semnarea din `PregatesteOperare` ⇒ refuzul 38c, ca și cum regula ar lipsi",
                    s.Refuz(() => MotorOperare.Opereaza(os, rdcSemn))
                        ?.Contains("nu are regulă de contare de cost pentru Tipul ei") == true);
                new Purja(os)
                    .Adauga(rdcSemn.Detalii.ToList())
                    .Adauga(rdcSemn)
                    .Adauga(lotSemnRet)
                    .Adauga(produsSemnRet)
                    .Adauga(regulaSemnRet)
                    .Adauga(tipSemnRet)
                    .Executa();
                // Fix post-review pas 4: Capitalizat pe venitul stornat n-are sens
                // economic și ar compunda brutul la re-operare — refuz la validare.
                var ned21Ret = os.FirstOrDefault<TipTva>(t => t.Cod == "NED21");
                RefuzRet("RDC: linie de venit cu regim Capitalizat (NED21) → refuz (semnarea ar compunda la re-operare)", () => {
                    var doc = RdcNou();
                    var d = LinieRdc(doc, tip707, 1m, 100m, null);
                    d.TipTva = ned21Ret;
                    return doc;
                });
                // Fix post-review: Capitalizat pe RLF ar umfla valoarea liniei peste
                // costul lotului (net × cotă) — identificarea specifică ruptă.
                RefuzRet("RLF: linie cu regim Capitalizat (NED21) → refuz (valoarea returului e costul lotului)", () => {
                    var doc = os.CreateObject<ReturFurnizor>();
                    doc.Data = new DateOnly(2026, 12, 12);
                    doc.Predator = gestiune; doc.Primitor = furnizor;
                    var d = os.CreateObject<DocumentDetaliu>();
                    d.Document = doc; d.TipMaterial = tip371; d.Lot = lot; d.Cantitate = 1m; d.TipTva = ned21Ret;
                    return doc;
                });

                // --- Storno pe RLF: rândurile inverse sunt POZITIVE cu Storno=true ---
                var dStorno = new DateOnly(2026, 12, 20);
                MotorOperare.Storneaza(os, rlf, dStorno);
                var stornoStocRlfCub = CubScena.Stoc(os, rlf.ID).Where(p => p.Storno).ToList();
                var stornoNoteRlfCub = CubScena.Note(os, rlf.ID).Where(p => p.Storno).ToList();
                s.Check("Storno pe RETUR: rândurile inverse sunt POZITIVE și poartă Storno=true (flag-ul rămâne al meta-operației)",
                    rlf.Stare == StareDocument.Stornat
                    && stornoStocRlfCub.Single() is { Cantitate: 4m, Semnata: 40m, Data.Day: 20 }
                    && stornoNoteRlfCub.All(p => p.Data == dStorno)
                    && stornoNoteRlfCub.Nota(cont371.ID, cont401.ID, 40m)
                    && stornoNoteRlfCub.Nota(cont4426.ID, cont401.ID, 8.4m));
                s.Check("Stocul revine după stornarea returului (10 − 4 + 3 + 4 = 13); TVA deductibilă revine la valoarea inițială",
                    SoldStocCub(lot, dStorno) == 13m
                    && SoldContCub(cont4426) - sold4426InitialCub == 0m);

                // --- F13-D1: RLF + TI păstrează autolichidarea (latura Deductibil) ---
                //
                // Perechea probei de pe FCL (blocul P1 privat): sensul taxării
                // inverse NU se citește din clasa documentului („e retur, deci n-are
                // TVA") ci din `PoliticaTva.Directie`. RLF stornează o ACHIZIȚIE —
                // politica lui e `Deductibil`, exact ca a FCT-ului — deci
                // autolichidarea rămâne întreagă, cu semnul storno al retururilor:
                // 4426 = 4427 cu −TVA. Dacă D1 ar fi fost scris pe tipul de document
                // în loc de latură, checkul ăsta ar fi picat.
                //
                // Scenă PROPRIE (NIR + lot separat), la coada blocului: soldurile de
                // mai sus sunt măsurate cu delte globale pe 4426/4427, iar o probă
                // nouă strecurată între ele le-ar fi mutat cifrele fără să spună de ce.
                var nirTi = os.CreateObject<NIR>();
                nirTi.Data = new DateOnly(2026, 12, 22);
                nirTi.Predator = furnizor; nirTi.Primitor = gestiune;
                var linNirTi = os.CreateObject<DocumentDetaliu>();
                linNirTi.Document = nirTi; linNirTi.TipMaterial = tip371; linNirTi.Cantitate = 5m; linNirTi.Valoare = 50m;
                var lotTi = linNirTi.CreeazaLot(os, produs, gestiune);
                os.CommitChanges();
                MotorOperare.Opereaza(os, nirTi);

                var dRlfTi = new DateOnly(2026, 12, 23);
                var rlfTi = os.CreateObject<ReturFurnizor>();
                rlfTi.Data = dRlfTi; rlfTi.Predator = gestiune; rlfTi.Primitor = furnizor;
                var linRlfTi = os.CreateObject<DocumentDetaliu>();
                linRlfTi.Document = rlfTi; linRlfTi.TipMaterial = tip371; linRlfTi.Lot = lotTi;
                linRlfTi.Cantitate = 2m; linRlfTi.TipTva = ti21;
                os.CommitChanges();
                MotorOperare.Opereaza(os, rlfTi);
                var noteRlfTiCub = NoteCub(rlfTi);
                s.Check("F13-D1 RLF + TI21: pe latura DEDUCTIBIL (achiziția stornată) autolichidarea RĂMÂNE — "
                    + "371 = 401 cu −20 ȘI 4426 = 4427 cu −4.2 — adică regula e per (regim × direcția politicii), "
                    + "nu per clasă de document; pe FCL aceeași pereche nu postează nimic",
                    noteRlfTiCub.Nota(cont371.ID, cont401.ID, -20m)
                    && noteRlfTiCub.Nota(cont4426.ID, cont4427.ID, -4.2m)
                    && linRlfTi.ValoareTva == -4.2m
                    && noteRlfTiCub.All(p => p.Fel == N.FelTranzactie.Operare && p.Data == dRlfTi));

                CurataRet(os);
                s.Check("Curățenie finală retururi (fără reziduuri e2e)",
                    !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajRet))
                    && !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(MarcajRet))
                    && !os.GetObjectsQuery<ReturFurnizor>().Any()
                    && !os.GetObjectsQuery<ReturClient>().Any());
            }
        }
    }
}
