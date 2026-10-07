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

// ================= Felia API ASM (F19, track 2) — E2E-API-ASM =================
// Asamblarea parcursă prin CONTRACTUL feliei: WriteDto → `AsamblareApply.Aplica`
// → `Citeste`/`Lista` → `DistribuieValoarea` → dry-run → `ComenziDocument`.
// Endpoint-urile din host sunt transport peste EXACT acest cod, deci ce e verde
// aici e verde și pe sârmă.
//
// Proba API rulează pe privat; ScenariiAsm acoperă ambele profiluri (ASM-B4).
//
// Ce exersează, în plus față de blocul de MOTOR (`E2E-ASM`, care probează
// mecanica pe calea XAF):
//   * TESTUL-ANCORĂ al lui F19-D3: linia de produs culeasă prin Apply
//     (`ProdusId`) naște lotul pe linia proprie, în gestiunea PREDATORULUI; PUT
//     repetat nu naște al doilea lot; comutarea Produs→Consum șterge lotul
//     propriu nefinalizat și golește câmpurile produsului; consumul cu lot
//     pinuit rămâne NEATINS;
//   * `Valoare` SEMNATĂ la culegere ⇒ `Total` draft == `Diferenta` din ReadDto;
//   * ANCORA F19-D4 (închide 75-r1): `distribuie-valoarea` pe scena cu lot golit
//     cu rezidu — predicția == cifra pe care o SCRIE operarea, invariantul trece
//     exact, a doua rulare e idempotentă, reziduul de ban se plimbă pe linia cu
//     grila cea mai fină, iar cazul nereprezentabil (75-r4) se REFUZĂ cu cifra;
//   * refuzurile de payload și cele ale tipului, fără rânduri-fantomă și fără
//     serie consumată;
//   * riscul 3 al contractului: coerența Tip↔Produs la NAȘTERE, MĂSURATĂ.
static class VerificaApiAsm {
    public static void Ruleaza(Suita s) {
        const string Marcaj = "E2E-API-ASM";
        using var os = s.Provider.CreateObjectSpace();

        var tip371 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "371");
        var tipAsm = os.FirstOrDefault<TipDocument>(t => t.Cod == "ASM");
        // Al doilea Tip de stoc al profilului — pentru proba de coerență Tip↔Produs
        // (riscul 3). Se CAUTĂ, nu se presupune: dacă profilul n-are decât unul,
        // proba se sare explicit.
        var tipAltStoc = os.GetObjectsQuery<TipMaterial>()
            .Where(t => t.Clasa.Natura == NaturaClasa.Stoc && t.ID != tip371.ID)
            .OrderBy(t => t.Cod).FirstOrDefault();

        var reguliAsm = os.GetObjectsQuery<RegulaStoc>().Where(r => r.TipDocumentId == tipAsm.ID).ToList();

        void Curata() {
            // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
            var pj = new Purja(os);
            var repIds = os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(Marcaj)).Select(r => r.ID).ToList();
            var docIds = os.GetObjectsQuery<Document>()
                .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId))
                .Select(d => d.ID).ToList();
            var loturi = os.GetObjectsQuery<Lot>()
                .Where(l => l.Produs.Cod.StartsWith(Marcaj)).Select(l => l.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<Imperechere>()
                .Where(i => docIds.Contains(i.DocumentId) || docIds.Contains(i.DocumentStingatorId)).ToList());
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>()
                .Where(d => docIds.Contains(d.DocumentId)).ToList());
            foreach (var doc in os.GetObjectsQuery<Document>().Where(d => docIds.Contains(d.ID)).ToList()
                         .OrderByDescending(d => d.DocumentSursaId != null))
                pj.Adauga(doc);
            pj.Adauga(os.GetObjectsQuery<Lot>().Where(l => loturi.Contains(l.ID)).ToList());
            pj.Adauga(os.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(Marcaj)).ToList());
            pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(Marcaj)).ToList());
            pj.Executa();
        }
        Curata();

        s.Check("Api ASM — precondiții de profil: ancora ASM cu seria „ASM-”, NICIO regulă de stoc (D9-D8) "
            + "și NICIO politică de TVA (F19-D7: `TipTvaId`/`ValoareTva` ar fi cifră moartă în DTO)",
            tipAsm != null && tipAsm.ClrType == nameof(Asamblare)
            && os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocumentId == tipAsm.ID)?.Serie == "ASM-"
            && reguliAsm.Count == 0
            && os.FirstOrDefault<PoliticaTva>(p => p.TipDocumentId == tipAsm.ID) == null
            && !os.GetObjectsQuery<RegulaContare>().Any(r => r.TipDocumentId == tipAsm.ID)
            && tip371 != null);

        // ---------------- Scena ----------------
        // DOUĂ gestiuni DIFERITE: ASM permite laturi diferite („de regulă aceeași”),
        // iar lotul produsului trebuie să se nască în PREDATOR (F19-D3). Cu o
        // singură gestiune proba ar trece și cu hook-ul lipsă.
        var gA = os.CreateObject<Gestiune>();
        gA.Cod = Marcaj + "-GA"; gA.Denumire = "Gestiune Api ASM (se asamblează)";
        var gB = os.CreateObject<Gestiune>();
        gB.Cod = Marcaj + "-GB"; gB.Denumire = "Gestiune Api ASM (primitoare)";
        var furnizor = os.CreateObject<Partener>();
        furnizor.Cod = Marcaj + "-F"; furnizor.Denumire = "Furnizor Api ASM"; furnizor.CodFiscal = "RO44444447";
        var loc = os.CreateObject<UnitateInterna>();
        loc.Cod = Marcaj + "-LOC"; loc.Denumire = "Loc consum Api ASM"; loc.Calitati = CalitateRepartitor.LocConsum;
        Produs Prod(string sufix, TipMaterial tip = null) {
            var p = os.CreateObject<Produs>();
            p.Cod = Marcaj + sufix; p.Denumire = "Produs Api ASM" + sufix; p.UM = "BUC";
            p.TipMaterial = tip ?? tip371;
            return p;
        }
        var produsA = Prod("-A");
        var produsKit = Prod("-KIT");
        var produsKit2 = Prod("-KIT2");
        var produsAltTip = tipAltStoc == null ? null : Prod("-ALT", tipAltStoc);
        os.CommitChanges();

        var dataLot = new DateOnly(2026, 6, 1);
        var dataAsm = new DateOnly(2026, 6, 10);

        // Lotul „strâmb”: 3 bucăți intrate cu 30,02 ⇒ preț 10,006667 (26e).
        Lot LotNou(Produs p, decimal cantitate, decimal valoare) {
            var nir = os.CreateObject<NIR>();
            nir.Data = dataLot; nir.Predator = furnizor; nir.Primitor = gA;
            var lin = os.CreateObject<DocumentDetaliu>();
            lin.Document = nir; lin.TipMaterial = p.TipMaterial; lin.Cantitate = cantitate; lin.Valoare = valoare;
            var lot = lin.CreeazaLot(os, p, gA);
            os.CommitChanges();
            MotorOperare.Opereaza(os, nir);
            os.CommitChanges();
            return lot;
        }
        void Bcs(Lot lot, DateOnly data, decimal q) {
            var doc = os.CreateObject<BonConsum>();
            doc.Data = data; doc.Predator = gA; doc.Primitor = loc;
            var d = os.CreateObject<DocumentDetaliu>();
            d.Document = doc; d.TipMaterial = tip371; d.Lot = lot; d.Cantitate = q;
            os.CommitChanges();
            MotorOperare.Opereaza(os, doc);
            os.CommitChanges();
        }
        // Lotul „cu rest”: după două ieșiri de câte 1 (10,01 + 10,01) rămâne 1 bucată
        // și 10,00 lei, iar `preț × 1` ar scoate 10,01 — exact scena D18-V2.
        Lot LotCuRest(Produs p) {
            var lot = LotNou(p, 3m, 30.02m);
            Bcs(lot, new DateOnly(2026, 6, 2), 1m);
            Bcs(lot, new DateOnly(2026, 6, 3), 1m);
            return lot;
        }

        var lotIntreg = LotNou(produsA, 10m, 50m);      // 10 × 5,00 — lot „cuminte”
        (decimal Cantitate, decimal Valoare) SoldCheieCub(Guid lotId, Repartitor r) => CubScena.SoldCheie(os, lotId, r.ID);
        s.Check("Api ASM premisă: lotul „cuminte” de 10 × 5,00 se naște prin NIR operat, în gestiunea în care se "
            + "asamblează",
            lotIntreg.PretUnitar == 5m && SoldCheieCub(lotIntreg.ID, gA) == (10m, 50m));

        // Dry-run-ul își cere ObjectSpace-ul PROPRIU (contractul lui
        // `MotorOperare.Valideaza`: `PregatesteOperare` SCRIE pe linii).
        IReadOnlyList<string> DryRun(Guid docId) {
            using var osDry = s.Provider.CreateObjectSpace();
            return ComenziDocument.Sistem(osDry).Valideaza(docId);
        }
        // Seria se citește din BAZĂ, nu din cache-ul OS-ului scenei.
        int SerieAsm() {
            using var o = s.Provider.CreateObjectSpace();
            return o.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "ASM").UrmatorulNumar;
        }
        Lot LotulLiniei(Guid linieId) =>
            os.GetObjectsQuery<Lot>().FirstOrDefault(l => l.LinieIntrareId == linieId);
        int LoturiAleLiniei(Guid linieId) =>
            os.GetObjectsQuery<Lot>().Count(l => l.LinieIntrareId == linieId);

        // ═══════════ (A) Testul-ancoră F19-D3: culegerea care naște lotul ═══════════
        var write = new AsmWriteDto {
            Data = dataAsm, PredatorId = gA.ID, PrimitorId = gB.ID,
            Linii = {
                new AsmLinieWriteDto {
                    Directie = "Consum", TipMaterialId = tip371.ID, LotId = lotIntreg.ID, Cantitate = 4m
                },
                new AsmLinieWriteDto {
                    Directie = "Produs", TipMaterialId = tip371.ID, ProdusId = produsKit.ID, Cantitate = 2m,
                    PretEvaluare = 10m, DataExpirare = new DateOnly(2027, 1, 31), LotFabricatie = "SARJA-1"
                }
            }
        };
        var idAsm = AsamblareApply.Aplica(os, null, write);
        var cit = AsamblareApply.Citeste(os, idAsm);
        var lConsum = cit.Linii.Single(l => l.Directie == "Consum");
        var lProdus = cit.Linii.Single(l => l.Directie == "Produs");
        var lotNascut = LotulLiniei(lProdus.Id);
        Console.WriteLine($"     MĂSURAT (Api ASM ancora F19-D3): linia de produs {lProdus.Id} a născut lotul "
            + $"{lotNascut?.ID} în gestiunea „{lotNascut?.Gestiune?.Denumire}” (predator „{gA.Denumire}”, "
            + $"primitor „{gB.Denumire}”), preț {lotNascut?.PretUnitar:0.######} (nefinalizat).");
        s.Check("ANCORA F19-D3 (Api ASM): linia de PRODUS culeasă cu `ProdusId` naște lotul PE LINIA PROPRIE "
            + "(`LinieIntrareId`), în gestiunea PREDATORULUI — nu a primitorului (default-ul bazei ar fi dus lotul în "
            + "gestiunea greșită, iar operarea l-ar fi refuzat fără cale de reparare din UI)",
            lotNascut != null && lotNascut.GestiuneId == gA.ID && gA.ID != gB.ID
            && lotNascut.ProdusId == produsKit.ID && lProdus.LotId == lotNascut.ID
            && lotNascut.PretUnitar == 0m && lotNascut.Data == default);
        s.Check("Api ASM: header plat, FĂRĂ număr (seria „ASM-” e server-owned — F19-D6), cu denumirile ambelor laturi; "
            + "`PoateDistribui` e adevărat pe un Draft cu ambele roluri",
            cit.Numar == null && cit.Stare == "Draft" && cit.Data == dataAsm
            && cit.PredatorId == gA.ID && cit.PredatorDenumire == gA.Denumire
            && cit.PrimitorId == gB.ID && cit.PrimitorDenumire == gB.Denumire
            && cit.PoateEdita && cit.PoateOpera && !cit.PoateAnula && !cit.PoateStorna && cit.PoateDistribui);
        s.Check("Api ASM (F19-D8): `Valoare` e SEMNATĂ de la culegere — consum −round(4 × 5,00) = −20,00, produs "
            + "+round(2 × 10,00) = +20,00 — iar `Cantitate` rămâne POZITIVĂ până la operare (28a)",
            lConsum.Valoare == -20m && lProdus.Valoare == 20m
            && lConsum.Cantitate == 4m && lProdus.Cantitate == 2m);
        s.Check("ANCORA F19-D9: invariantul 46d iese calculat SERVER-SIDE (`SumaConsum`/`SumaProdus`/`Diferenta`, "
            + "magnitudini pozitive) — iar `Total`-ul draftului E diferența (Σ valorilor semnate), deci clientul nu "
            + "face aritmetică (42c)",
            cit.SumaConsum == 20m && cit.SumaProdus == 20m && cit.Diferenta == 0m && cit.Total == 0m
            && cit.Total == cit.Diferenta);
        s.Check("Api ASM: linia proiectează plat Tipul, produsul, eticheta lotului și atributele lui; lotul „în culegere” "
            + "se recunoaște ca atare în etichetă",
            lProdus.ProdusId == produsKit.ID && lProdus.ProdusCod == produsKit.Cod
            && lProdus.TipMaterialCod == tip371.Cod && lProdus.PretEvaluare == 10m
            && lProdus.DataExpirare == new DateOnly(2027, 1, 31) && lProdus.LotFabricatie == "SARJA-1"
            && lProdus.LotEticheta != null && lProdus.LotEticheta.Contains("în culegere")
            && lConsum.LotId == lotIntreg.ID && lConsum.ProdusId == null && lConsum.PretEvaluare == null);
        var randLista = AsamblareApply.Lista(os).Single(d => d.Id == idAsm);
        s.Check("Api ASM: Lista dă aceleași cifre ca agregatul (Total prin join pe agregat), starea tradusă în SQL",
            randLista.Stare == "Draft" && randLista.Total == 0m && randLista.Numar == null
            && randLista.PredatorDenumire == gA.Denumire && randLista.PrimitorDenumire == gB.Denumire);

        // Round-trip: ReadDto → WriteDto → Apply. `LotId`-ul liniei de PRODUS se
        // întoarce din ReadDto, deci payload-ul îl retrimite — el trebuie IGNORAT.
        AsmWriteDto Rescriere(AsmReadDto d) => new() {
            Data = d.Data, PredatorId = d.PredatorId, PrimitorId = d.PrimitorId,
            Linii = d.Linii.Select(l => new AsmLinieWriteDto {
                Id = l.Id, Directie = l.Directie, TipMaterialId = l.TipMaterialId,
                ProdusId = l.ProdusId, LotId = l.LotId, Cantitate = l.Cantitate,
                PretEvaluare = l.PretEvaluare, DataExpirare = l.DataExpirare, LotFabricatie = l.LotFabricatie,
                AngajamentId = l.AngajamentId
            }).ToList()
        };
        AsamblareApply.Aplica(os, idAsm, Rescriere(cit));
        var citRT = AsamblareApply.Citeste(os, idAsm);
        s.Check("ANCORA F19-D3: PUT repetat (round-trip complet al agregatului) NU naște al doilea lot — linia își "
            + "păstrează lotul, iar `LotId`-ul retrimis din ReadDto e IGNORAT pe direcția Produs (server-owned)",
            LoturiAleLiniei(lProdus.Id) == 1
            && citRT.Linii.Single(l => l.Id == lProdus.Id).LotId == lotNascut.ID
            && citRT.Linii.Count == 2 && citRT.Diferenta == 0m);

        // Un `LotId` STRĂIN trimis pe linia de produs nu are voie s-o re-lege.
        var payloadLotStrain = Rescriere(citRT);
        payloadLotStrain.Linii.Single(l => l.Id == lProdus.Id).LotId = lotIntreg.ID;
        AsamblareApply.Aplica(os, idAsm, payloadLotStrain);
        s.Check("Api ASM: `LotId` STRĂIN pe o linie de PRODUS se ignoră (lotul e server-owned) — linia rămâne pe lotul "
            + "ei, iar lotul altcuiva nu e adoptat",
            AsamblareApply.Citeste(os, idAsm).Linii.Single(l => l.Id == lProdus.Id).LotId == lotNascut.ID);

        // Comutarea Produs → Consum: lotul propriu NEFINALIZAT dispare, câmpurile
        // produsului se golesc PERSISTAT, iar consumul cu pin rămâne neatins.
        var idLotNascut = lotNascut.ID;
        var payloadComutare = Rescriere(AsamblareApply.Citeste(os, idAsm));
        var linieComutata = payloadComutare.Linii.Single(l => l.Id == lProdus.Id);
        linieComutata.Directie = "Consum";
        linieComutata.LotId = lotIntreg.ID;
        AsamblareApply.Aplica(os, idAsm, payloadComutare);
        var linieDupaComutare = os.GetObjectByKey<AsamblareDetaliu>(lProdus.Id);
        Console.WriteLine($"     MĂSURAT (Api ASM comutare): lotul propriu {idLotNascut} după Produs→Consum: "
            + $"{(os.GetObjectByKey<Lot>(idLotNascut) == null ? "ȘTERS" : "rămas")}; "
            + $"ProdusId={linieDupaComutare.ProdusId?.ToString() ?? "null"}, "
            + $"PretEvaluare={linieDupaComutare.PretEvaluare?.ToString() ?? "null"}.");
        s.Check("ANCORA F19-D3: comutarea Produs→Consum prin PUT ȘTERGE lotul propriu NEFINALIZAT (gardul `NasteLot`) și "
            + "GOLEȘTE persistat câmpurile produsului (F6-D3: „inert devine adevărat, nu doar afirmat”) — un produs "
            + "rămas pe o linie de consum ar fi citit de validarea de coerență, printr-un câmp pe care UI-ul nu-l arată",
            os.GetObjectByKey<Lot>(idLotNascut) == null
            && linieDupaComutare.ProdusId == null && linieDupaComutare.PretEvaluare == null
            && linieDupaComutare.DataExpirare == null && linieDupaComutare.LotFabricatie == null
            && linieDupaComutare.LotId == lotIntreg.ID);
        s.Check("ANCORA F19-D3: consumul cu lot PINUIT a rămas NEATINS peste toate PUT-urile (gardul de lot străin: "
            + "linia care n-a născut lotul nu i-l atinge)",
            os.GetObjectByKey<AsamblareDetaliu>(lConsum.Id).LotId == lotIntreg.ID);

        // ═══════════ (B) Refuzurile de PAYLOAD (reconcilierea) ═══════════
        AsamblareApply.Aplica(os, idAsm, new AsmWriteDto {
            Data = dataAsm, PredatorId = gA.ID, PrimitorId = gB.ID,
            Linii = {
                new AsmLinieWriteDto { Id = lConsum.Id, Directie = "Consum", TipMaterialId = tip371.ID,
                    LotId = lotIntreg.ID, Cantitate = 4m },
                new AsmLinieWriteDto { Id = lProdus.Id, Directie = "Produs", TipMaterialId = tip371.ID,
                    ProdusId = produsKit.ID, Cantitate = 2m, PretEvaluare = 10m }
            }
        });
        var citBaza = AsamblareApply.Citeste(os, idAsm);
        var lotRenascut = LotulLiniei(lProdus.Id);
        Console.WriteLine($"     MĂSURAT (Api ASM, comutarea inversă Consum→Produs): linia {lProdus.Id} a "
            + $"{(lotRenascut == null ? "RĂMAS FĂRĂ lot" : $"născut lotul {lotRenascut.ID}")}; pinul consumului "
            + $"(lotul {lotIntreg.ID}) a fost {(citBaza.Linii.Single(l => l.Id == lProdus.Id).LotId == lotIntreg.ID ? "PĂSTRAT" : "rupt")}.");
        s.Check("ANCORA F19-D3 (comutarea INVERSĂ, Consum→Produs — defect găsit și reparat în felie): pinul lotului "
            + "consumat se RUPE la culegere, ca linia să-și poată naște lotul propriu. Fără ruptură, gardul de lot "
            + "STRĂIN din `LoturiCulegereService` bloca nașterea, linia rămânea pe lotul consumului, iar documentul "
            + "devenea PERMANENT ne-operabil („lotul unei linii de produs se naște pe linia însăși”) — cu `Lot` "
            + "server-owned pe direcția Produs, adică fără nicio cale de reparare din ecran",
            lotRenascut != null && lotRenascut.LinieIntrareId == lProdus.Id
            && lotRenascut.ProdusId == produsKit.ID && lotRenascut.GestiuneId == gA.ID
            && citBaza.Linii.Single(l => l.Id == lProdus.Id).LotId == lotRenascut.ID
            && LoturiAleLiniei(lProdus.Id) == 1);
        AsmWriteDto Payload(params AsmLinieWriteDto[] linii) => new() {
            Data = dataAsm, PredatorId = gA.ID, PrimitorId = gB.ID, Linii = linii.ToList()
        };
        AsmLinieWriteDto LinieValida() => new() {
            Directie = "Consum", TipMaterialId = tip371.ID, LotId = lotIntreg.ID, Cantitate = 1m
        };

        s.CheckRefuza("Api ASM: rol de linie necunoscut → refuz de DOMENIU cu valorile acceptate enumerate "
            + "(parse pe NUME, la graniță, ÎNAINTE de `CreateObject`)", () => {
            var l = LinieValida(); l.Directie = "Iesire";
            AsamblareApply.Aplica(os, idAsm, Payload(l));
        });
        s.CheckRefuza("Api ASM: rol absent (null) → același refuz — enumerarea n-are membru 0, deci o linie fără rol "
            + "n-are ce fi", () => {
            var l = LinieValida(); l.Directie = null;
            AsamblareApply.Aplica(os, idAsm, Payload(l));
        });
        s.CheckRefuza("Api ASM: Id de linie STRĂIN → refuz (agregatul nu adoptă linii din alt document)", () => {
            var l = LinieValida(); l.Id = Guid.NewGuid();
            AsamblareApply.Aplica(os, idAsm, Payload(l));
        });
        s.CheckRefuza("Api ASM: același Id de linie de două ori → refuz", () => {
            var l = LinieValida(); l.Id = lConsum.Id;
            AsamblareApply.Aplica(os, idAsm, Payload(l, l));
        });
        s.CheckRefuza("Api ASM: `TipMaterialId` absent din payload → refuz de DOMENIU, nu violare de FK NOT NULL", () => {
            var l = LinieValida(); l.TipMaterialId = Guid.Empty;
            AsamblareApply.Aplica(os, idAsm, Payload(l));
        });
        s.CheckRefuza("Api ASM: lot inexistent pe o linie de consum → refuz de domeniu (rezolvarea pe navigație)", () => {
            var l = LinieValida(); l.LotId = Guid.NewGuid();
            AsamblareApply.Aplica(os, idAsm, Payload(l));
        });
        s.CheckRefuza("Api ASM: produs inexistent pe o linie de produs → refuz de domeniu", () =>
            AsamblareApply.Aplica(os, idAsm, Payload(new AsmLinieWriteDto {
                Directie = "Produs", TipMaterialId = tip371.ID, ProdusId = Guid.NewGuid(), Cantitate = 1m,
                PretEvaluare = 1m
            })));
        s.CheckRefuza("Api ASM: cantitate în afara scării numeric(18,3) → refuz de domeniu, nu DbUpdateException", () => {
            var l = LinieValida(); l.Cantitate = 1.0001m;
            AsamblareApply.Aplica(s.OsCuGardian(), idAsm, Payload(l));
        });
        s.CheckRefuza("Api ASM: preț de evaluare în afara scării numeric(18,6) → refuz de domeniu", () =>
            AsamblareApply.Aplica(s.OsCuGardian(), idAsm, Payload(new AsmLinieWriteDto {
                Directie = "Produs", TipMaterialId = tip371.ID, ProdusId = produsKit.ID, Cantitate = 1m,
                PretEvaluare = 1.0000001m
            })));
        s.CheckRefuza("Api ASM: latură inexistentă în nomenclatorul de repartitori → refuz de domeniu, înaintea oricărei "
            + "modificări a header-ului", () =>
            AsamblareApply.Aplica(os, idAsm, new AsmWriteDto {
                Data = dataAsm, PredatorId = Guid.NewGuid(), PrimitorId = gB.ID, Linii = { LinieValida() }
            }));
        s.Check("Api ASM: …iar asamblarea rămâne pe laturile ei (refuzul n-a rescris nimic)",
            AsamblareApply.Citeste(os, idAsm) is { PredatorId: var pId } && pId == gA.ID);

        AsamblareApply.Aplica(os, idAsm, Rescriere(citBaza));
        var dupaRefuzuri = AsamblareApply.Citeste(os, idAsm);
        Console.WriteLine($"     MĂSURAT (Api ASM, după refuzuri): {dupaRefuzuri.Linii.Count} linii "
            + $"[{string.Join(", ", dupaRefuzuri.Linii.Select(l => $"{l.Directie}:{l.Valoare:N2}"))}], "
            + $"Diferenta {dupaRefuzuri.Diferenta:N2}, loturi ale liniei de produs {LoturiAleLiniei(lProdus.Id)}.");
        s.Check("Api ASM: un Apply refuzat nu lasă reziduu în agregat — următorul payload valid readuce documentul la "
            + "exact două linii, cu aceleași cifre, și fără lot în plus",
            dupaRefuzuri.Diferenta == 0m && dupaRefuzuri.Linii.Count == 2
            && LoturiAleLiniei(lProdus.Id) == 1);

        // Linia de tip BAZĂ (ASM istoric/importat).
        var docIstoric = os.GetObjectByKey<Asamblare>(idAsm);
        var linieBaza = os.CreateObject<DocumentDetaliu>();   // NU AsamblareDetaliu
        linieBaza.Document = docIstoric; linieBaza.TipMaterial = tip371;
        linieBaza.Cantitate = 1m; linieBaza.Valoare = 7m;
        os.CommitChanges();
        var idLinieBaza = linieBaza.ID;
        var citCuBaza = AsamblareApply.Citeste(os, idAsm);
        s.Check("Api ASM: citirea merge pe BAZA detaliului (as-cast la frunză) — linia de tip BAZĂ APARE, cu câmpurile "
            + "frunzei NULE (inclusiv `Directie`), intră în `Total` dar NU în sumele invariantului (n-are rol) — de "
            + "aceea pe un document cu linii vechi `Total` (7,00) și `Diferenta` (0,00) diferă DELIBERAT",
            citCuBaza.Linii.Count == 3
            && citCuBaza.Linii.Single(l => l.Id == idLinieBaza) is { Directie: null, ProdusId: null, PretEvaluare: null }
            && citCuBaza.Total == 7m && citCuBaza.Diferenta == 0m
            && AsamblareApply.Lista(os).Single(d => d.Id == idAsm).Total == 7m);
        s.CheckRefuza("Api ASM: Id-ul unei linii de tip BAZĂ în payload → refuz acționabil („ștergeți-o și culegeți-o din nou”)",
            () => {
                var l = LinieValida(); l.Id = idLinieBaza;
                AsamblareApply.Aplica(os, idAsm, Payload(l));
            });
        s.CheckRefuza("Api ASM: `distribuie-valoarea` pe un document cu linii de tip vechi se refuză explicit (n-au rol, "
            + "deci n-au loc nici în consum, nici în produse)",
            () => AsamblareApply.DistribuieValoarea(os, s.Deschide, idAsm));
        s.CheckRefuza("Api ASM: motorul refuză o asamblare cu linie de tip BAZĂ („trebuie culeasă ca linie de asamblare”)",
            () => ComenziDocument.Sistem(os).Opereaza(idAsm));
        AsamblareApply.Aplica(os, idAsm, Rescriere(citBaza));
        s.Check("Api ASM: linia absentă din payload se ȘTERGE (reconciliere server-side) — linia de bază dispare, "
            + "agregatul revine la două linii",
            !os.GetObjectsQuery<DocumentDetaliu>().Any(d => d.ID == idLinieBaza)
            && AsamblareApply.Citeste(os, idAsm).Linii.Count == 2);

        // ═══════════ (C) Refuzurile TIPULUI, prin calea API ═══════════
        var serieInainte = SerieAsm();
        Guid IdNou(Guid predator, Guid primitor, params AsmLinieWriteDto[] linii) =>
            AsamblareApply.Aplica(os, null, new AsmWriteDto {
                Data = dataAsm, PredatorId = predator, PrimitorId = primitor, Linii = linii.ToList()
            });

        var idLaturi = IdNou(loc.ID, gB.ID,
            new AsmLinieWriteDto { Directie = "Consum", TipMaterialId = tip371.ID, LotId = lotIntreg.ID, Cantitate = 1m },
            new AsmLinieWriteDto { Directie = "Produs", TipMaterialId = tip371.ID, ProdusId = produsKit.ID,
                Cantitate = 1m, PretEvaluare = 5m });
        s.CheckRefuza("Api ASM: predator ne-Gestiune → refuz al tipului („asamblarea trăiește într-o gestiune”)",
            () => ComenziDocument.Sistem(os).Opereaza(idLaturi));
        s.Check("Api ASM: dry-run-ul spune ACELAȘI lucru înaintea comenzii (43b: autoritar e motorul)",
            DryRun(idLaturi).Any(e => e.Contains("gestiune")));
        AsamblareApply.Sterge(os, idLaturi);

        var idPretZero = IdNou(gA.ID, gB.ID,
            new AsmLinieWriteDto { Directie = "Consum", TipMaterialId = tip371.ID, LotId = lotIntreg.ID, Cantitate = 1m },
            new AsmLinieWriteDto { Directie = "Produs", TipMaterialId = tip371.ID, ProdusId = produsKit.ID,
                Cantitate = 1m });
        s.CheckRefuza("Api ASM: linie de produs FĂRĂ preț de evaluare → refuz al tipului („cere preț de evaluare pozitiv”)",
            () => ComenziDocument.Sistem(os).Opereaza(idPretZero));
        AsamblareApply.Sterge(os, idPretZero);

        s.Check("Api ASM (F19-D6 + GATE D6): niciun refuz n-a consumat seria „ASM-” — numărul se asignează abia la "
            + "MATERIALIZARE, deci un document refuzat nu lasă gaură în numerotare",
            SerieAsm() == serieInainte);

        // Consum dintr-un lot NĂSCUT DE ACELAȘI DOCUMENT (lanțul de kitting).
        var idLotFrate = AsamblareApply.Aplica(os, null, new AsmWriteDto {
            Data = dataAsm, PredatorId = gA.ID, PrimitorId = gA.ID,
            Linii = {
                new AsmLinieWriteDto { Directie = "Consum", TipMaterialId = tip371.ID, LotId = lotIntreg.ID, Cantitate = 1m },
                new AsmLinieWriteDto { Directie = "Produs", TipMaterialId = tip371.ID, ProdusId = produsKit.ID,
                    Cantitate = 1m, PretEvaluare = 5m }
            }
        });
        var citFrate = AsamblareApply.Citeste(os, idLotFrate);
        var lotFrate = citFrate.Linii.Single(l => l.Directie == "Produs").LotId.Value;
        var payloadFrate = Rescriere(citFrate);
        payloadFrate.Linii.Add(new AsmLinieWriteDto {
            Directie = "Consum", TipMaterialId = tip371.ID, LotId = lotFrate, Cantitate = 1m
        });
        AsamblareApply.Aplica(os, idLotFrate, payloadFrate);
        s.CheckRefuza("Api ASM: consumul unui lot produs de ACELAȘI document → refuz al tipului (lanțul de kitting = "
            + "documente separate, operate în ordine) — culegerea îl PERMITE, refuzul e al motorului, cu mesajul lui",
            () => ComenziDocument.Sistem(os).Opereaza(idLotFrate));
        AsamblareApply.Sterge(os, idLotFrate);

        // Riscul 3 al contractului, MĂSURAT: coerența Tip↔Produs la NAȘTERE.
        if (produsAltTip != null) {
            var idTipGresit = IdNou(gA.ID, gA.ID,
                new AsmLinieWriteDto { Directie = "Consum", TipMaterialId = tip371.ID, LotId = lotIntreg.ID, Cantitate = 1m },
                new AsmLinieWriteDto { Directie = "Produs", TipMaterialId = tip371.ID, ProdusId = produsAltTip.ID,
                    Cantitate = 1m, PretEvaluare = 5m });
            var citTipGresit = AsamblareApply.Citeste(os, idTipGresit);
            var linieTipGresit = citTipGresit.Linii.Single(l => l.Directie == "Produs").Id;
            var lotTipGresit = LotulLiniei(linieTipGresit);
            Console.WriteLine($"     MĂSURAT (Api ASM, riscul 3 — coerența Tip↔Produs la NAȘTERE): linia cu Tipul "
                + $"„{tip371.Cod}” și produsul „{produsAltTip.Cod}” (Tip „{tipAltStoc.Cod}”) e ACCEPTATĂ la culegere, "
                + $"lotul {lotTipGresit?.ID} SE NAȘTE pe draft (nefinalizat, fără rânduri de stoc), iar refuzul vine "
                + "de la operare — ca pe LDI, care are aceeași ordine (F6-F2).");
            s.Check("Api ASM (riscul 3, MĂSURAT): coerența Tip↔Produs nu e păzită la CULEGERE — lotul se naște pe draft "
                + "cu produsul de alt Tip; e NEFINALIZAT (preț 0, dată 0) și fără rânduri de registru, deci urma e "
                + "reparabilă, nu contabilă",
                lotTipGresit != null && lotTipGresit.ProdusId == produsAltTip.ID
                && lotTipGresit.PretUnitar == 0m && lotTipGresit.Data == default
                && CubScena.FaraMiscari(os, lotTipGresit.ID));
            s.CheckRefuza("Api ASM (riscul 3 / review M2): operarea refuză prin gardul EXPLICIT pe `ProdusId` — cel pe "
                + "care îl au toate tipurile care nasc loturi (LDI F6-F2, FCT, FCL, NIR) și care lipsea de pe ASM. "
                + "Mesajul numește PRODUSUL, câmp editabil, nu lotul: pe linia de produs `Lot` e server-owned și "
                + "read-only, deci vechiul refuz trimitea operatorul la un câmp pe care nu-l poate atinge (62f)",
                () => ComenziDocument.Sistem(os).Opereaza(idTipGresit));
            s.Check("Api ASM (review M2): refuzul e chiar cel al produsului, cuvânt cu cuvânt ca pe LDI — nu cel "
                + "TRANZITIV prin lot",
                DryRun(idTipGresit).Any(e =>
                    e == "Produsul liniei aparține altui Tip decât Tipul liniei — corectați Tipul sau produsul."));
            var idLotTipGresit = lotTipGresit.ID;
            var reparare = Rescriere(AsamblareApply.Citeste(os, idTipGresit));
            reparare.Linii.Single(l => l.Id == linieTipGresit).ProdusId = produsKit.ID;
            AsamblareApply.Aplica(os, idTipGresit, reparare);
            s.Check("Api ASM (riscul 3): urma se repară din UI — realegerea produsului MUTĂ lotul existent pe produsul "
                + "corect (sincronizarea lotului propriu), nu lasă un al doilea lot orfan",
                LoturiAleLiniei(linieTipGresit) == 1
                && os.GetObjectByKey<Lot>(idLotTipGresit) is { } lotReparat && lotReparat.ProdusId == produsKit.ID);
            AsamblareApply.Sterge(os, idTipGresit);
            s.Check("Api ASM (riscul 3): ștergerea draftului duce cu ea și lotul nefinalizat — nicio urmă",
                os.GetObjectByKey<Lot>(idLotTipGresit) == null);
        }

        // ═══════════ (D) ANCORA F19-D4: `distribuie-valoarea` (închide 75-r1) ═══════════
        AsamblareApply.Sterge(os, idAsm);

        // --- (D1) Capcana: consumul care GOLEȘTE lotul ia 10,00, nu 10,01 ---
        var lotRest = LotCuRest(produsA);
        s.Check("Api ASM (D18-V2 replicat): lotul „cu rest” are prețul 10,006667 și soldul 1 buc / 10,00 — `preț × 1` "
            + "rotunjit ar da 10,01, adică exact capcana pe care o repară F19-D4",
            lotRest.PretUnitar == 10.006667m && SoldCheieCub(lotRest.ID, gA) == (1m, 10.00m)
            && Scara.RotunjesteBani(1m * lotRest.PretUnitar) == 10.01m);

        var idCapcana = IdNou(gA.ID, gA.ID,
            new AsmLinieWriteDto { Directie = "Consum", TipMaterialId = tip371.ID, LotId = lotRest.ID, Cantitate = 1m },
            new AsmLinieWriteDto { Directie = "Produs", TipMaterialId = tip371.ID, ProdusId = produsKit.ID,
                Cantitate = 1m, PretEvaluare = 10.01m });   // evaluarea „naivă”: preț lot × cantitate
        var citCapcana = AsamblareApply.Citeste(os, idCapcana);
        var erCapcana = DryRun(idCapcana);
        Console.WriteLine($"     MĂSURAT (Api ASM, capcana 75-r1): la culegere `Diferenta` = {citCapcana.Diferenta:N2} "
            + $"(consum {citCapcana.SumaConsum:N2}, produs {citCapcana.SumaProdus:N2}) — documentul PARE echilibrat; "
            + $"dry-run-ul dă {erCapcana.Count} eroare/erori: {string.Join(" | ", erCapcana)}");
        s.Check("ANCORA 75-r1 (capcana, MĂSURATĂ): la culegere documentul pare ECHILIBRAT (`Diferenta` 0,00 — culegerea "
            + "prezice `preț × cantitate`), dar operarea îl REFUZĂ: consumul care golește cheia ia 10,00 (D18-D2), nu "
            + "10,01 — asta e capcana pe care operatorul n-o poate ieși din ecran fără comanda F19-D4",
            citCapcana.Diferenta == 0m && citCapcana.SumaConsum == 10.01m
            && erCapcana.Any(e => e.StartsWith(Atlas.Conta.BackOffice.Module.Declaratii.CoduriRefuz.AsamblareNebalansata)
                && e.Contains($"Consum C={10.00m}; produse P={10.01m}")));

        var distr = AsamblareApply.DistribuieValoarea(os, s.Deschide, idCapcana);
        var citDupa = AsamblareApply.Citeste(os, idCapcana);
        var linieProdusCapcana = citDupa.Linii.Single(l => l.Directie == "Produs");
        var erDupa = DryRun(idCapcana);
        Console.WriteLine($"     MĂSURAT (Api ASM, F19-D4): predicția consumului {distr.SumaConsum:N2}, repartizat pe "
            + $"produse {distr.SumaProdus:N2} (reziduu plimbat {distr.ReziduuPlimbat:N2}); preț nou "
            + $"{linieProdusCapcana.PretEvaluare:0.######}, valoare {linieProdusCapcana.Valoare:N2}; "
            + $"dry-run după distribuire: {erDupa.Count} erori.");
        s.Check("ANCORA F19-D4: comanda PREZICE valoarea consumului prin dry-run-ul MOTORULUI (deci prin regula golirii "
            + "D18-D2, nu printr-o a doua formulă) — 10,00, nu 10,01 — și rescrie prețul produsului la exact atât; "
            + "dry-run-ul de după trece FĂRĂ nicio eroare",
            distr.SumaConsum == 10.00m && distr.SumaProdus == 10.00m
            && linieProdusCapcana.PretEvaluare == 10m && linieProdusCapcana.Valoare == 10.00m
            && erDupa.Count == 0);
        s.Check("Api ASM (F19-D9, limita DECLARATĂ): după distribuire `Diferenta` din ReadDto iese −0,01, fiindcă "
            + "sumele proiecției sunt ale CULEGERII (`preț × cantitate` — 75a interzice culegerii să prezică golirea); "
            + "verdictul autoritar rămâne dry-run-ul, iar cifrele prezise ies din `AsmDistribuireDto`",
            citDupa.SumaConsum == 10.01m && citDupa.SumaProdus == 10.00m && citDupa.Diferenta == -0.01m);

        // Idempotența: a doua rulare, aceleași cifre.
        var distr2 = AsamblareApply.DistribuieValoarea(os, s.Deschide, idCapcana);
        var citDupa2 = AsamblareApply.Citeste(os, idCapcana);
        s.Check("ANCORA F19-D4 (idempotența): a doua rulare pe același document, cu același registru, dă EXACT aceleași "
            + "cifre — cheia de repartizare devine chiar valorile scrise de prima, iar prețul e normalizat ca funcție "
            + "a valorii finale",
            distr2.SumaConsum == distr.SumaConsum && distr2.SumaProdus == distr.SumaProdus
            && distr2.ReziduuPlimbat == 0m
            && citDupa2.Linii.Single(l => l.Directie == "Produs") is { PretEvaluare: 10m, Valoare: 10.00m });

        // Operarea: predicția == cifra pe care o SCRIE motorul.
        ComenziDocument.Sistem(os).Opereaza(idCapcana);
        var citOperat = AsamblareApply.Citeste(os, idCapcana);
        var lConsumOperat = citOperat.Linii.Single(l => l.Directie == "Consum");
        var lProdusOperat = citOperat.Linii.Single(l => l.Directie == "Produs");
        var lotKitNou = os.GetObjectByKey<Lot>(lProdusOperat.LotId.Value);
        Console.WriteLine($"     MĂSURAT (Api ASM, predicție vs operare): predicția {distr.SumaConsum:N2} — operarea a "
            + $"scris pe linia de consum {lConsumOperat.Valoare:N2}; lotul consumat "
            + $"{SoldCheieCub(lotRest.ID, gA).Cantitate:0.###}/{SoldCheieCub(lotRest.ID, gA).Valoare:N2}; lotul nou "
            + $"{lotKitNou.PretUnitar:0.######} la {lotKitNou.Data:dd.MM.yyyy}.");
        s.Check("ANCORA F19-D4 (cusătura MĂSURATĂ, nu afirmată): cifra pe care a PREZIS-O comanda (10,00) e EXACT "
            + "valoarea pe care a scris-o operarea pe linia de consum (−10,00) — deci predicția și motorul folosesc "
            + "aceeași regulă, nu două; invariantul 46d trece la ZERO, iar lotul consumat ajunge la 0 / 0,00",
            citOperat.Stare == "Operat" && lConsumOperat.Valoare == -distr.SumaConsum
            && lProdusOperat.Valoare == distr.SumaProdus
            && lConsumOperat.Valoare + lProdusOperat.Valoare == 0m
            && SoldCheieCub(lotRest.ID, gA) == (0m, 0m));
        s.Check("Api ASM: operarea SEMNEAZĂ cantitățile (consum −1, produs +1 — 28a), consumă seria „ASM-”, finalizează "
            + "lotul nou cu `PretUnitar = PretEvaluare` la data documentului și îi copiază atributele",
            lConsumOperat.Cantitate == -1m && lProdusOperat.Cantitate == 1m
            && citOperat.Numar != null && citOperat.Numar.StartsWith("ASM-")
            && lotKitNou.PretUnitar == 10m && lotKitNou.Data == dataAsm && lotKitNou.GestiuneId == gA.ID
            && SoldCheieCub(lotKitNou.ID, gA) == (1m, 10.00m));
        s.Check("Api ASM: asamblarea NU postează (zero `RegulaContare` — 23c: marfă→marfă la sintetic e zgomot); "
            + "affordance-ele ReadDto trec pe Operat, iar `PoateDistribui` cade",
            !citOperat.PoateEdita && !citOperat.PoateOpera && citOperat.PoateAnula && citOperat.PoateStorna
            && !citOperat.PoateDistribui);
        s.CheckRefuza("Api ASM: `distribuie-valoarea` pe un document care nu mai e Draft se refuză",
            () => AsamblareApply.DistribuieValoarea(os, s.Deschide, idCapcana));
        s.CheckRefuza("Api ASM: PUT pe un document Operat se refuză (pre-check de domeniu, înaintea gardianului)",
            () => AsamblareApply.Aplica(os, idCapcana, Rescriere(citOperat)));

        // --- (D2) Reziduul PLIMBAT pe mai multe linii de produs ---
        // Grila valorilor realizabile e cu atât mai groasă cu cât cantitatea e mai
        // mare (un pas de 1e-6 pe preț mișcă valoarea cu q × 1e-6): linia de 30.000
        // bucăți nu poate absorbi un ban, cea de 1 bucată poate.
        var lotRest2 = LotCuRest(produsA);
        var idMulti = IdNou(gA.ID, gA.ID,
            new AsmLinieWriteDto { Directie = "Consum", TipMaterialId = tip371.ID, LotId = lotRest2.ID, Cantitate = 1m },
            new AsmLinieWriteDto { Directie = "Produs", TipMaterialId = tip371.ID, ProdusId = produsKit.ID,
                Cantitate = 30000m, PretEvaluare = 0.0002m },     // pondere 6,00, grilă de 0,03
            new AsmLinieWriteDto { Directie = "Produs", TipMaterialId = tip371.ID, ProdusId = produsKit2.ID,
                Cantitate = 1m, PretEvaluare = 4.01m });          // pondere 4,01, grilă de 0,000001
        var distrMulti = AsamblareApply.DistribuieValoarea(os, s.Deschide, idMulti);
        var citMulti = AsamblareApply.Citeste(os, idMulti);
        var lGros = citMulti.Linii.Single(l => l.ProdusId == produsKit.ID);
        var lFin = citMulti.Linii.Single(l => l.ProdusId == produsKit2.ID);
        Console.WriteLine($"     MĂSURAT (Api ASM, reziduul plimbat): țintă {distrMulti.SumaConsum:N2}; ponderi culese "
            + $"6,00 + 4,01 = 10,01 ⇒ părți 5,99 + 4,01; linia de 30.000 buc realizează {lGros.Valoare:N2} "
            + $"(preț {lGros.PretEvaluare:0.######}, un pas de 1e-6 mișcă 0,03), linia de 1 buc absoarbe reziduul "
            + $"{distrMulti.ReziduuPlimbat:N2} ⇒ {lFin.Valoare:N2} (preț {lFin.PretEvaluare:0.######}).");
        s.Check("ANCORA F19-D4 (reziduul se PLIMBĂ): pe mai multe linii de produs repartizarea e proporțională cu "
            + "valoarea CULEASĂ, iar diferența de rotunjire se pune pe linia pe care un pas de 0,000001 o poate "
            + "absorbi — cea cu cantitatea cea mai mică, adică grila cea mai fină; Σ produselor == valoarea "
            + "consumului la CENT",
            distrMulti.SumaConsum == 10.00m && distrMulti.SumaProdus == 10.00m
            && distrMulti.ReziduuPlimbat == -0.01m
            && lGros.Valoare == 6.00m && lFin.Valoare == 4.00m
            && lGros.Valoare + lFin.Valoare == distrMulti.SumaConsum);
        s.Check("Api ASM: distribuirea pe mai multe linii e și ea idempotentă (a doua rulare nu mai plimbă nimic)",
            AsamblareApply.DistribuieValoarea(os, s.Deschide, idMulti) is
                { SumaProdus: 10.00m, ReziduuPlimbat: 0m }
            && AsamblareApply.Citeste(os, idMulti).Linii.Single(l => l.ProdusId == produsKit2.ID).Valoare == 4.00m);
        s.Check("Api ASM: documentul distribuit pe mai multe linii trece dry-run-ul FĂRĂ erori (invariantul 46d la zero)",
            DryRun(idMulti).Count == 0);
        ComenziDocument.Sistem(os).Opereaza(idMulti);
        s.Check("Api ASM: …și operează, cu ambele loturi finalizate la prețurile distribuite",
            AsamblareApply.Citeste(os, idMulti).Stare == "Operat"
            && SoldCheieCub(lotRest2.ID, gA) == (0m, 0m)
            && os.GetObjectsQuery<Lot>().Single(l => l.LinieIntrareId == lGros.Id).PretUnitar == 0.0002m);

        // --- (D3) Limita 75-r4: reziduu NEREPREZENTABIL ⇒ refuz cu CIFRA ---
        // Consum care NU golește (lot de 3, iese 1) ⇒ ținta rămâne 10,01, iar linia
        // de produs are 1.000.000 buc: un pas de 1e-6 pe preț mișcă valoarea cu 1,00
        // leu, deci grila e mai groasă decât banul.
        var lotIntreg2 = LotNou(produsA, 3m, 30.02m);
        var idNerepr = IdNou(gA.ID, gA.ID,
            new AsmLinieWriteDto { Directie = "Consum", TipMaterialId = tip371.ID, LotId = lotIntreg2.ID, Cantitate = 1m },
            new AsmLinieWriteDto { Directie = "Produs", TipMaterialId = tip371.ID, ProdusId = produsKit.ID,
                Cantitate = 1000000m, PretEvaluare = 0.00001m });
        var mesajNerepr = new List<string>();
        try { AsamblareApply.DistribuieValoarea(os, s.Deschide, idNerepr); }
        catch (OperareException ex) { mesajNerepr.Add(ex.Message); }
        Console.WriteLine($"     MĂSURAT (Api ASM, limita 75-r4): țintă 10,01 pe 1.000.000 buc ⇒ prețul reprezentabil "
            + $"cel mai apropiat (0,000010) realizează 10,00; refuzul: {string.Join(" ", mesajNerepr)}");
        s.Check("ANCORA F19-D4 (limita 75-r4, DECLARATĂ nu ascunsă): când niciun preț de 6 zecimale nu poate stinge "
            + "reziduul (cantități mari — grila valorilor devine mai groasă decât banul), comanda REFUZĂ cu CIFRA "
            + "reziduului (0,01), în loc să lase un ASM pe care operarea l-ar refuza oricum",
            mesajNerepr.Count == 1 && mesajNerepr[0].Contains("0,01") && mesajNerepr[0].Contains("10,01"));
        s.Check("Api ASM (75-r4): refuzul n-a scris nimic — prețul cules rămâne cel de dinainte",
            AsamblareApply.Citeste(os, idNerepr).Linii.Single(l => l.Directie == "Produs").PretEvaluare == 0.00001m);
        AsamblareApply.Sterge(os, idNerepr);

        // --- (D4) Refuzurile comenzii: documente pe care predicția n-are ce prezice ---
        var idFaraProdus = IdNou(gA.ID, gA.ID,
            new AsmLinieWriteDto { Directie = "Consum", TipMaterialId = tip371.ID, LotId = lotIntreg.ID, Cantitate = 1m });
        s.CheckRefuza("Api ASM: `distribuie-valoarea` fără nicio linie de PRODUS → refuz (n-are pe ce distribui)",
            () => AsamblareApply.DistribuieValoarea(os, s.Deschide, idFaraProdus));
        s.Check("Api ASM: …iar `PoateDistribui` o spunea deja în ReadDto (afordanță, nu surpriză)",
            !AsamblareApply.Citeste(os, idFaraProdus).PoateDistribui);
        AsamblareApply.Sterge(os, idFaraProdus);

        var idFaraConsum = IdNou(gA.ID, gA.ID,
            new AsmLinieWriteDto { Directie = "Produs", TipMaterialId = tip371.ID, ProdusId = produsKit.ID,
                Cantitate = 1m, PretEvaluare = 5m });
        s.CheckRefuza("Api ASM: `distribuie-valoarea` fără nicio linie de CONSUM → refuz (nu există valoare de distribuit)",
            () => AsamblareApply.DistribuieValoarea(os, s.Deschide, idFaraConsum));
        AsamblareApply.Sterge(os, idFaraConsum);

        var idConsumFaraLot = IdNou(gA.ID, gA.ID,
            new AsmLinieWriteDto { Directie = "Consum", TipMaterialId = tip371.ID, Cantitate = 1m },
            new AsmLinieWriteDto { Directie = "Produs", TipMaterialId = tip371.ID, ProdusId = produsKit.ID,
                Cantitate = 1m, PretEvaluare = 5m });
        s.CheckRefuza("Api ASM: `distribuie-valoarea` cu un consum FĂRĂ lot → refuz explicit — cifra prezisă s-ar "
            + "schimba imediat ce linia se completează, iar operatorul ar rămâne cu prețuri care par bune",
            () => AsamblareApply.DistribuieValoarea(os, s.Deschide, idConsumFaraLot));
        AsamblareApply.Sterge(os, idConsumFaraLot);

        var idMixt = IdNou(gA.ID, gA.ID,
            new AsmLinieWriteDto { Directie = "Consum", TipMaterialId = tip371.ID, LotId = lotIntreg.ID, Cantitate = 1m },
            new AsmLinieWriteDto { Directie = "Produs", TipMaterialId = tip371.ID, ProdusId = produsKit.ID,
                Cantitate = 1m, PretEvaluare = 5m },
            new AsmLinieWriteDto { Directie = "Produs", TipMaterialId = tip371.ID, ProdusId = produsKit2.ID,
                Cantitate = 1m });
        s.CheckRefuza("Api ASM: cheia de repartizare MIXTĂ (o linie de produs evaluată, alta nu) → refuz cu ce are "
            + "operatorul de făcut — ponderea 0 ar da preț 0, adică refuzul operării mutat cu un pas mai încolo",
            () => AsamblareApply.DistribuieValoarea(os, s.Deschide, idMixt));
        // …iar cu ZERO prețuri culese cheia devine CANTITATEA (F19-D4).
        var payloadCantitati = Rescriere(AsamblareApply.Citeste(os, idMixt));
        foreach (var l in payloadCantitati.Linii.Where(l => l.Directie == "Produs")) {
            l.PretEvaluare = null;
            l.Cantitate = l.ProdusId == produsKit.ID ? 3m : 1m;
        }
        AsamblareApply.Aplica(os, idMixt, payloadCantitati);
        var distrCantitati = AsamblareApply.DistribuieValoarea(os, s.Deschide, idMixt);
        var citCantitati = AsamblareApply.Citeste(os, idMixt);
        Console.WriteLine($"     MĂSURAT (Api ASM, cheia pe cantitate): țintă {distrCantitati.SumaConsum:N2} pe 3 + 1 "
            + $"bucăți ⇒ {citCantitati.Linii.Single(l => l.ProdusId == produsKit.ID).Valoare:N2} + "
            + $"{citCantitati.Linii.Single(l => l.ProdusId == produsKit2.ID).Valoare:N2}.");
        s.Check("ANCORA F19-D4 (cheia de rezervă): dacă NICIO linie de produs n-are valoare culeasă, repartizarea e "
            + "proporțională cu CANTITATEA — 5,00 pe 3 + 1 bucăți ⇒ 3,75 + 1,25, Σ la cent",
            distrCantitati.SumaConsum == 5.00m && distrCantitati.SumaProdus == 5.00m
            && citCantitati.Linii.Single(l => l.ProdusId == produsKit.ID).Valoare == 3.75m
            && citCantitati.Linii.Single(l => l.ProdusId == produsKit2.ID).Valoare == 1.25m);
        AsamblareApply.Sterge(os, idMixt);

        // ═══════════ (E) Anulare, re-operare, storno ═══════════
        var numarCapcana = citOperat.Numar;
        s.Check("Api ASM: anulare prin API → Draft, rândurile de stoc dispar, lotul consumat își recapătă soldul",
            ComenziDocument.Sistem(os).AnuleazaOperarea(idCapcana).StareNoua == StareDocument.Draft
            && CubScena.FaraStoc(os, idCapcana)
            && SoldCheieCub(lotRest.ID, gA) == (1m, 10.00m));
        ComenziDocument.Sistem(os).Opereaza(idCapcana);
        var citReoperat = AsamblareApply.Citeste(os, idCapcana);
        s.Check("Api ASM: re-operare după anulare — același număr, aceleași cifre (idempotența `Abs`-urilor din "
            + "`PregatesteOperare` + regula golirii, care recalculează pe registrul curent)",
            citReoperat.Numar == numarCapcana
            && citReoperat.Linii.Single(l => l.Directie == "Consum").Valoare == -10.00m
            && citReoperat.Linii.Single(l => l.Directie == "Produs").Valoare == 10.00m
            && SoldCheieCub(lotRest.ID, gA) == (0m, 0m));
        s.Check("Api ASM: storno prin API → Stornat, rânduri INVERSE append-only la data stornării (+1/+10,00 pe lotul "
            + "consumat, −1/−10,00 pe lotul produs)",
            ComenziDocument.Sistem(os).Storneaza(idCapcana, new DateOnly(2026, 7, 15)).StareNoua == StareDocument.Stornat
            && CubScena.Stoc(os, idCapcana).Count(p => p.Storno) == 2
            && SoldCheieCub(lotRest.ID, gA) == (1m, 10.00m));

        Curata();
        s.Check("Api ASM: curățenie finală (fără reziduuri e2e)",
            !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(Marcaj))
            && !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(Marcaj))
            && os.GetObjectByKey<Asamblare>(idCapcana) == null);
    }
}
