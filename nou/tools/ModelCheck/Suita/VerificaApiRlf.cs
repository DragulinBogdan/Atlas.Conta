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

// ================= Felia API RLF (F19, track 3) — E2E-API-RLF =================
// Returul la furnizor parcurs prin CONTRACTUL feliei: WriteDto →
// `ReturFurnizorApply.Aplica` → `Citeste`/`Lista` → dry-run → `ComenziDocument`.
// Endpoint-urile din host sunt transport peste EXACT acest cod, deci ce e verde
// aici e verde și pe sârmă.
//
// Rulează DOAR pe profilul PRIVAT (F19-D14): RLF are politici (numerotare, reguli
// de stoc, contare, TVA) doar acolo; pe bugetar tipul e inert.
//
// Ce exersează, în plus față de blocul de MOTOR (`E2E-RET`, care probează
// mecanica pe calea XAF):
//   * F19-D8: culegerea e POZITIVĂ (cifra de pe nota de credit a furnizorului),
//     iar semnarea rămâne a OPERĂRII — cusătura măsurată contra hook-ului însuși;
//   * lanțul de TVA la culegere (implicit pe linii NOI, recalcul pe declanșatori,
//     override pe regimurile cu TVA separat);
//   * ANCORA F18/F5 (`IDocumentCuIesireFiscala`): returul care GOLEȘTE cantitativ
//     lotul NU absoarbe soldul valoric rămas — reziduul rămâne pe lot, 401 nu-l
//     primește; probat pe calea API și pus FAȚĂ ÎN FAȚĂ cu un consum obișnuit pe
//     un lot geamăn, care ÎL absoarbe;
//   * riscul 6: idempotența semnării prin calea API (culegere → operare → anulare
//     → PUT → re-operare) — cine bate pe cine între `MaterializeazaValori` și
//     `PregatesteOperare`;
//   * refuzurile de payload și cele ale tipului, fără rânduri-fantomă și fără
//     serie consumată.
static class VerificaApiRlf {
    public static void Ruleaza(Suita s) {
        const string Marcaj = "E2E-API-RLF";
        using var os = s.Provider.CreateObjectSpace();

        var tip371 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "371");
        var tipRlf = os.FirstOrDefault<TipDocument>(t => t.Cod == "RLF");
        var n21 = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
        var ned21 = os.FirstOrDefault<TipTva>(t => t.Cod == "NED21");   // Capitalizat
        var sdd = os.FirstOrDefault<TipTva>(t => t.Cod == "SDD");       // Scutit
        var cont371 = os.FirstOrDefault<Cont>(c => c.Simbol == "371");
        var cont401 = os.FirstOrDefault<Cont>(c => c.Simbol == "401");
        var cont4426 = os.FirstOrDefault<Cont>(c => c.Simbol == "4426");

        var reguliRlf = os.GetObjectsQuery<RegulaStoc>().Where(r => r.TipDocumentId == tipRlf.ID).ToList();

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

        s.Check("Api RLF — precondiții de profil: ancora RLF cu seria „RLF-”, NICIO regulă de stoc (D9-D8) "
            + "și `PoliticaTva` DEDUCTIBIL cu `TipTvaImplicit = N21` — de aceea `TipTvaId`/"
            + "`ValoareTva` INTRĂ în DTO (F19-D7), spre deosebire de NTC/ASM",
            tipRlf != null && tipRlf.ClrType == nameof(ReturFurnizor)
            && os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocumentId == tipRlf.ID)?.Serie == "RLF-"
            && reguliRlf.Count == 0
            && os.FirstOrDefault<PoliticaTva>(p => p.TipDocumentId == tipRlf.ID)?.Directie == DirectieTva.Deductibil
            && tipRlf.TipTvaImplicitId == n21.ID
            && tip371 != null && ned21 != null && sdd != null);

        // ---------------- Scena ----------------
        var gest = os.CreateObject<Gestiune>();
        gest.Cod = Marcaj + "-G"; gest.Denumire = "Gestiune Api RLF";
        var furnizor = os.CreateObject<Partener>();
        furnizor.Cod = Marcaj + "-F"; furnizor.Denumire = "Furnizor Api RLF"; furnizor.CodFiscal = "RO44444448";
        // Clasa fiscală a partenerului decide REGIMUL implicit (83f): un furnizor RO
        // neînregistrat ar primi de acum NIM, nu cota standard — nota de credit de 21%
        // din scenă vine de la unul înregistrat, iar scena o SPUNE.
        furnizor.InregistratTva = true;
        var loc = os.CreateObject<UnitateInterna>();
        loc.Cod = Marcaj + "-LOC"; loc.Denumire = "Loc consum Api RLF"; loc.Calitati = CalitateRepartitor.LocConsum;
        Produs Prod(string sufix) {
            var p = os.CreateObject<Produs>();
            p.Cod = Marcaj + sufix; p.Denumire = "Produs Api RLF" + sufix; p.UM = "BUC";
            p.TipMaterial = tip371;
            return p;
        }
        var produsA = Prod("-A");
        var produsRest = Prod("-REST");
        var produsGeaman = Prod("-GEAMAN");
        os.CommitChanges();

        var dataLot = new DateOnly(2026, 9, 1);
        var dataRlf = new DateOnly(2026, 9, 10);

        Lot LotNou(Produs p, decimal cantitate, decimal valoare) {
            var nir = os.CreateObject<NIR>();
            nir.Data = dataLot; nir.Predator = furnizor; nir.Primitor = gest;
            var lin = os.CreateObject<DocumentDetaliu>();
            lin.Document = nir; lin.TipMaterial = tip371; lin.Cantitate = cantitate; lin.Valoare = valoare;
            var lot = lin.CreeazaLot(os, p, gest);
            os.CommitChanges();
            MotorOperare.Opereaza(os, nir);
            os.CommitChanges();
            return lot;
        }
        void Bcs(Lot lot, DateOnly data, decimal q) {
            var doc = os.CreateObject<BonConsum>();
            doc.Data = data; doc.Predator = gest; doc.Primitor = loc;
            var d = os.CreateObject<DocumentDetaliu>();
            d.Document = doc; d.TipMaterial = tip371; d.Lot = lot; d.Cantitate = q;
            os.CommitChanges();
            MotorOperare.Opereaza(os, doc);
            os.CommitChanges();
        }
        (decimal Cantitate, decimal Valoare) SoldCheieCub(Guid lotId) => CubScena.SoldCheie(os, lotId, gest.ID);
        List<PostareScena> NoteCub(Guid docId) => CubScena.Note(os, docId).Where(p => !p.Storno).ToList();
        IReadOnlyList<string> DryRun(Guid docId) {
            using var osDry = s.Provider.CreateObjectSpace();
            return ComenziDocument.Sistem(osDry).Valideaza(docId);
        }
        int SerieRlf() {
            using var o = s.Provider.CreateObjectSpace();
            return o.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "RLF").UrmatorulNumar;
        }
        // Cusătura F19-D8, MĂSURATĂ: ce SCRIE hook-ul de operare pe o linie, rulat pe
        // un ObjectSpace de unică folosință (contractul lui `PregatesteOperare`: el
        // SCRIE pe linii, deci nu are voie să atingă OS-ul scenei; nu se comite).
        (decimal Valoare, decimal Tva, decimal Cantitate) Hook(Guid docId, Guid linieId) {
            using var o = s.Provider.CreateObjectSpace();
            var d = o.GetObjectByKey<ReturFurnizor>(docId);
            d.PregatesteOperare(o);
            var l = d.Detalii.Single(x => x.ID == linieId);
            return (l.Valoare, l.ValoareTva, l.Cantitate);
        }

        var lotIntreg = LotNou(produsA, 10m, 100m);   // 10 × 10,00 — lot „cuminte”
        s.Check("Api RLF premisă: lotul de 10 × 10,00 se naște prin NIR operat, în gestiunea returului",
            lotIntreg.PretUnitar == 10m && SoldCheieCub(lotIntreg.ID) == (10m, 100m));

        // ═══════════ (A) Culegerea: valori POZITIVE + lanțul de TVA ═══════════
        var idRlf = ReturFurnizorApply.Aplica(os, null, new RlfWriteDto {
            Data = dataRlf, PredatorId = gest.ID, PrimitorId = furnizor.ID,
            Linii = { new RlfLinieWriteDto { TipMaterialId = tip371.ID, LotId = lotIntreg.ID, Cantitate = 4m } }
        });
        var cit = ReturFurnizorApply.Citeste(os, idRlf);
        var linie = cit.Linii.Single();
        s.Check("Api RLF (F19-D8): culegerea e POZITIVĂ — `Valoare` = round(4 × 10,00) = 40,00 și TVA-ul 21% = 8,40, "
            + "adică exact cifra de pe nota de credit a furnizorului; `Total` brut 48,40 pe DRAFT",
            linie.Cantitate == 4m && linie.Valoare == 40m && linie.ValoareTva == 8.4m && cit.Total == 48.4m);
        s.Check("Api RLF (F19-D7): `TipTvaImplicit` al tipului (N21) se aplică pe linia NOUĂ al cărei payload n-a dat "
            + "TipTva — seam-ul de culegere, nu motorul",
            linie.TipTvaId == n21.ID && linie.TipTvaCod == "N21" && linie.TipTvaCota == 21m);
        s.Check("Api RLF: header plat, FĂRĂ număr (seria „RLF-” e server-owned — F19-D6), cu denumirile ambelor laturi "
            + "și affordances de Draft",
            cit.Numar == null && cit.Stare == "Draft" && cit.Data == dataRlf
            && cit.PredatorId == gest.ID && cit.PredatorDenumire == gest.Denumire
            && cit.PrimitorId == furnizor.ID && cit.PrimitorDenumire == furnizor.Denumire
            && cit.PoateEdita && cit.PoateOpera && !cit.PoateAnula && !cit.PoateStorna);
        s.Check("Api RLF: linia proiectează plat Tipul, eticheta lotului și tipul de TVA (fără `as`-cast — RLF n-are "
            + "frunză, F19-D9)",
            linie.TipMaterialId == tip371.ID && linie.TipMaterialCod == tip371.Cod
            && linie.LotId == lotIntreg.ID
            && linie.LotEticheta != null && linie.LotEticheta.Contains(produsA.Denumire));
        var randLista = ReturFurnizorApply.Lista(os).Single(d => d.Id == idRlf);
        s.Check("Api RLF: Lista dă aceleași cifre ca agregatul (Total prin join pe agregat), starea tradusă în SQL",
            randLista.Stare == "Draft" && randLista.Total == 48.4m && randLista.Numar == null
            && randLista.PredatorDenumire == gest.Denumire && randLista.PrimitorDenumire == furnizor.Denumire);

        var hookInainte = Hook(idRlf, linie.Id);
        Console.WriteLine($"     MĂSURAT (Api RLF, cusătura F19-D8): culegerea a scris {linie.Valoare:N2} / "
            + $"{linie.ValoareTva:N2} pe cantitatea {linie.Cantitate:0.###}; `PregatesteOperare` scrie "
            + $"{hookInainte.Valoare:N2} / {hookInainte.Tva:N2} pe {hookInainte.Cantitate:0.###} — aceleași "
            + "magnitudini, semn opus.");
        s.Check("ANCORA F19-D8 (cusătură MĂSURATĂ, nu afirmată): TVA-ul și valoarea de la CULEGERE sunt EXACT ce "
            + "calculează `PregatesteOperare` înainte de semnare — formulele sunt gemene, nu două adevăruri; "
            + "diferența e doar semnul, care e al OPERĂRII (46e)",
            hookInainte.Valoare == -linie.Valoare && hookInainte.Tva == -linie.ValoareTva
            && hookInainte.Cantitate == -linie.Cantitate);

        // Round-trip: ReadDto → WriteDto → Apply.
        RlfWriteDto Rescriere(RlfReadDto d) => new() {
            Data = d.Data, PredatorId = d.PredatorId, PrimitorId = d.PrimitorId,
            Linii = d.Linii.Select(l => new RlfLinieWriteDto {
                Id = l.Id, TipMaterialId = l.TipMaterialId, LotId = l.LotId,
                Cantitate = l.Cantitate, TipTvaId = l.TipTvaId, ValoareTva = l.ValoareTva
            }).ToList()
        };
        ReturFurnizorApply.Aplica(os, idRlf, Rescriere(cit));
        s.Check("Api RLF: PUT de round-trip complet nu schimbă nimic (idempotent la nivel de agregat)",
            ReturFurnizorApply.Citeste(os, idRlf) is { Total: 48.4m, Linii.Count: 1 });

        // Override-ul manual de ValoareTva (36a) + semantica declanșatorilor (56).
        var cuOverride = Rescriere(ReturFurnizorApply.Citeste(os, idRlf));
        cuOverride.Linii[0].ValoareTva = 8.39m;
        ReturFurnizorApply.Aplica(os, idRlf, cuOverride);
        s.Check("Api RLF: override-ul manual de `ValoareTva` (8,39 — rotunjirea furnizorului) se persistă, iar `Total` "
            + "îl urmează",
            ReturFurnizorApply.Citeste(os, idRlf).Linii[0].ValoareTva == 8.39m
            && ReturFurnizorApply.Citeste(os, idRlf).Total == 48.39m);
        var faraDeclansator = Rescriere(ReturFurnizorApply.Citeste(os, idRlf));
        faraDeclansator.Linii[0].ValoareTva = null;   // clientul nu retrimite override-ul
        faraDeclansator.Data = dataRlf.AddDays(1);
        ReturFurnizorApply.Aplica(os, idRlf, faraDeclansator);
        s.Check("Api RLF (56): un PUT care nu atinge niciun DECLANȘATOR (cantitatea, lotul, TipTva) NU pierde "
            + "override-ul — clientul nu poate distinge „valoarea citită e override” de „e calculată”, deci n-o "
            + "retrimite",
            ReturFurnizorApply.Citeste(os, idRlf).Linii[0].ValoareTva == 8.39m);
        var cuDeclansator = Rescriere(ReturFurnizorApply.Citeste(os, idRlf));
        cuDeclansator.Linii[0].ValoareTva = null;
        cuDeclansator.Linii[0].Cantitate = 5m;
        ReturFurnizorApply.Aplica(os, idRlf, cuDeclansator);
        s.Check("Api RLF (56): schimbarea CANTITĂȚII e declanșator — baza devine 50,00, TVA-ul se recalculează din "
            + "cotă (10,50) și override-ul stale dispare",
            ReturFurnizorApply.Citeste(os, idRlf).Linii[0] is { Valoare: 50m, ValoareTva: 10.5m });
        var inapoi = Rescriere(ReturFurnizorApply.Citeste(os, idRlf));
        inapoi.Linii[0].ValoareTva = null;
        inapoi.Linii[0].Cantitate = 4m;
        ReturFurnizorApply.Aplica(os, idRlf, inapoi);
        s.Check("Api RLF: …și înapoi la 4 buc ⇒ 40,00 / 8,40 (valoarea nu se culege NICIODATĂ — e a lotului)",
            ReturFurnizorApply.Citeste(os, idRlf).Linii[0] is { Valoare: 40m, ValoareTva: 8.4m });

        // ═══════════ (B) Refuzurile de PAYLOAD (reconcilierea) ═══════════
        RlfWriteDto Payload(params RlfLinieWriteDto[] linii) => new() {
            Data = dataRlf, PredatorId = gest.ID, PrimitorId = furnizor.ID, Linii = linii.ToList()
        };
        RlfLinieWriteDto LinieValida() => new() {
            TipMaterialId = tip371.ID, LotId = lotIntreg.ID, Cantitate = 1m
        };
        s.CheckRefuza("Api RLF: Id de linie STRĂIN → refuz (agregatul nu adoptă linii din alt document)", () => {
            var l = LinieValida(); l.Id = Guid.NewGuid();
            ReturFurnizorApply.Aplica(os, idRlf, Payload(l));
        });
        s.CheckRefuza("Api RLF: același Id de linie de două ori → refuz", () => {
            var l = LinieValida(); l.Id = linie.Id;
            ReturFurnizorApply.Aplica(os, idRlf, Payload(l, l));
        });
        s.CheckRefuza("Api RLF: `TipMaterialId` absent din payload → refuz de DOMENIU, nu violare de FK NOT NULL", () => {
            var l = LinieValida(); l.TipMaterialId = Guid.Empty;
            ReturFurnizorApply.Aplica(os, idRlf, Payload(l));
        });
        s.CheckRefuza("Api RLF: lot inexistent → refuz de domeniu (rezolvarea pe navigație)", () => {
            var l = LinieValida(); l.LotId = Guid.NewGuid();
            ReturFurnizorApply.Aplica(os, idRlf, Payload(l));
        });
        s.CheckRefuza("Api RLF: tip de TVA inexistent → refuz de domeniu", () => {
            var l = LinieValida(); l.TipTvaId = Guid.NewGuid();
            ReturFurnizorApply.Aplica(os, idRlf, Payload(l));
        });
        s.CheckRefuza("Api RLF: cantitate în afara scării numeric(18,3) → refuz de domeniu, nu DbUpdateException", () => {
            var l = LinieValida(); l.Cantitate = 1.0001m;
            ReturFurnizorApply.Aplica(s.OsCuGardian(), idRlf, Payload(l));
        });
        s.CheckRefuza("Api RLF: `ValoareTva` în afara scării numeric(18,2) → refuz de domeniu", () => {
            var l = LinieValida(); l.ValoareTva = 1.001m;
            ReturFurnizorApply.Aplica(s.OsCuGardian(), idRlf, Payload(l));
        });
        s.CheckRefuza("Api RLF (56): override de `ValoareTva` pe un regim FĂRĂ TVA separat (SDD, scutit) → refuz — "
            + "`PregatesteOperare` l-ar șterge oricum la operare, deci acceptarea lui ar minți operatorul", () => {
            var l = LinieValida(); l.TipTvaId = sdd.ID; l.ValoareTva = 1m;
            ReturFurnizorApply.Aplica(os, idRlf, Payload(l));
        });
        s.CheckRefuza("Api RLF: latură inexistentă în nomenclatorul de repartitori → refuz de domeniu, înaintea "
            + "oricărei modificări a header-ului", () =>
            ReturFurnizorApply.Aplica(os, idRlf, new RlfWriteDto {
                Data = dataRlf, PredatorId = Guid.NewGuid(), PrimitorId = furnizor.ID, Linii = { LinieValida() }
            }));
        s.Check("Api RLF: …iar returul rămâne pe laturile lui și cu linia lui (niciun refuz n-a rescris nimic)",
            ReturFurnizorApply.Citeste(os, idRlf) is { PredatorId: var pId, Linii.Count: 1, Total: 48.4m }
            && pId == gest.ID);
        // Un Apply refuzat DUPĂ `CreateObject` lasă linia pe jumătate construită în
        // ObjectSpace-ul VIU al apelantului (pe HTTP el moare cu cererea — aici nu).
        // Reconcilierea următorului payload al ACELUIAȘI document o curăță: linia
        // fără Id în payload cade în `sterse`. Măsurat, nu presupus.
        ReturFurnizorApply.Aplica(os, idRlf, Rescriere(ReturFurnizorApply.Citeste(os, idRlf)));
        s.Check("Api RLF: un Apply refuzat nu lasă reziduu în agregat — următorul payload valid readuce documentul la "
            + "exact o linie, cu aceleași cifre",
            ReturFurnizorApply.Citeste(os, idRlf) is { Linii.Count: 1, Total: 48.4m }
            && os.GetObjectsQuery<DocumentDetaliu>().Count(d => d.DocumentId == idRlf) == 1);

        // ═══════════ (C) Refuzurile TIPULUI, prin calea API ═══════════
        var serieInainte = SerieRlf();
        Guid IdNou(Guid predator, Guid primitor, params RlfLinieWriteDto[] linii) =>
            ReturFurnizorApply.Aplica(os, null, new RlfWriteDto {
                Data = dataRlf, PredatorId = predator, PrimitorId = primitor, Linii = linii.ToList()
            });

        var idLaturi = IdNou(furnizor.ID, gest.ID, LinieValida());
        s.CheckRefuza("Api RLF: laturi INVERSATE (predator partener / primitor gestiune) → refuz al tipului",
            () => ComenziDocument.Sistem(os).Opereaza(idLaturi));
        s.Check("Api RLF: dry-run-ul spune ACELAȘI lucru înaintea comenzii (43b: autoritar e motorul)",
            DryRun(idLaturi).Any(e => e.Contains("gestiune")));
        s.Check("Api RLF: refuzul n-a lăsat rânduri-fantomă (33d)",
            CubScena.FaraStoc(os, idLaturi)
            && CubScena.FaraNote(os, idLaturi));
        ReturFurnizorApply.Sterge(os, idLaturi);

        var idFaraLot = IdNou(gest.ID, furnizor.ID,
            new RlfLinieWriteDto { TipMaterialId = tip371.ID, Cantitate = 1m });
        s.Check("Api RLF: linia fără lot e CULEASĂ (draftul are voie să fie incomplet), dar cu valoarea golită la 0 — "
            + "valoarea lotului scos de pe linie ar minți pe ecran (precedentul BCS)",
            ReturFurnizorApply.Citeste(os, idFaraLot).Linii[0] is { LotId: null, Valoare: 0m, ValoareTva: 0m });
        s.CheckRefuza("Api RLF: …iar operarea o refuză („returul descarcă LOTUL ORIGINAL” — decizia 13)",
            () => ComenziDocument.Sistem(os).Opereaza(idFaraLot));
        ReturFurnizorApply.Sterge(os, idFaraLot);

        var idCapitalizat = IdNou(gest.ID, furnizor.ID, new RlfLinieWriteDto {
            TipMaterialId = tip371.ID, LotId = lotIntreg.ID, Cantitate = 1m, TipTvaId = ned21.ID
        });
        s.CheckRefuza("Api RLF: regim `Capitalizat` (NED21) → refuz al MOTORULUI, nu al clientului (43b) — valoarea "
            + "returului e costul lotului, nu costul plus taxa",
            () => ComenziDocument.Sistem(os).Opereaza(idCapitalizat));
        ReturFurnizorApply.Sterge(os, idCapitalizat);

        var idPesteSold = IdNou(gest.ID, furnizor.ID, new RlfLinieWriteDto {
            TipMaterialId = tip371.ID, LotId = lotIntreg.ID, Cantitate = 999m
        });
        s.CheckRefuza("Api RLF: retur peste soldul lotului → refuzul gardianului de sold (25d), pe calea API",
            () => ComenziDocument.Sistem(os).Opereaza(idPesteSold));
        ReturFurnizorApply.Sterge(os, idPesteSold);

        s.Check("Api RLF (F19-D6 + GATE D6): niciun refuz n-a consumat seria „RLF-” — numărul se asignează abia la "
            + "MATERIALIZARE, deci un document refuzat nu lasă gaură în numerotare",
            SerieRlf() == serieInainte);
        Console.WriteLine("     MĂSURAT (Api RLF, gardul „lot propriu”): `ReturFurnizor.ValideazaOperare` refuză linia "
            + "al cărei lot are `LinieIntrareId == linia`, dar calea API NU poate ajunge acolo — liniile de RLF nu "
            + "declară `ILinieCareNasteLot`, deci `Aplica` nu naște niciun lot, iar `LotId` se poate lega doar la un "
            + "lot EXISTENT. Gardul rămâne al căilor directe (XAF/Import1C); declarat, nu sărit tăcut.");
        var liniiRlfScena = os.GetObjectsQuery<ReturFurnizor>()
            .Where(d => d.PredatorId == gest.ID || d.PrimitorId == furnizor.ID)
            .SelectMany(d => d.Detalii.Select(x => x.ID)).ToList();
        s.Check("Api RLF: nicio linie de retur nu a născut vreun lot pe toată scena (gardul „lot propriu” e "
            + "inatingibil prin API, prin construcție)",
            liniiRlfScena.Count > 0
            && !os.GetObjectsQuery<Lot>().Any(l => l.LinieIntrareId != null
                && liniiRlfScena.Contains(l.LinieIntrareId.Value)));

        // ═══════════ (D) Operarea: semnarea, stocul, notele ═══════════
        s.Check("Api RLF: dry-run-ul unui retur complet nu întoarce nicio eroare", DryRun(idRlf).Count == 0);
        ComenziDocument.Sistem(os).Opereaza(idRlf);
        var citOperat = ReturFurnizorApply.Citeste(os, idRlf);
        var linieOperata = citOperat.Linii.Single();
        s.Check("Api RLF: operarea SEMNEAZĂ (−4 / −40,00 / −8,40), consumă seria „RLF-”, iar ReadDto arată cifrele "
            + "semnate — fapta operării, pe un document oricum read-only (F19-D9)",
            citOperat.Stare == "Operat" && citOperat.Numar != null && citOperat.Numar.StartsWith("RLF-")
            && linieOperata.Cantitate == -4m && linieOperata.Valoare == -40m && linieOperata.ValoareTva == -8.4m
            && citOperat.Total == -48.4m
            && !citOperat.PoateEdita && !citOperat.PoateOpera && citOperat.PoateAnula && citOperat.PoateStorna);
        s.Check("Api RLF: stoc −4 / −40,00 în gestiunea PREDATOARE (regula +1 × linia negativă), soldul lotului scade "
            + "de la 10 la 6",
            CubScena.Stoc(os, idRlf).Count == 1
            && SoldCheieCub(lotIntreg.ID) == (6m, 60m));
        var noteRlfCub = NoteCub(idRlf);
        s.Check("Api RLF: note pe corespondența ORIGINALĂ, negative — `371 = 401` cu −40,00 și `4426 = 401` cu −8,40",
            noteRlfCub.Nota(cont371.ID, cont401.ID, -40m)
            && noteRlfCub.Nota(cont4426.ID, cont401.ID, -8.4m)
            && noteRlfCub.All(p => p.Fel == N.FelTranzactie.Operare));

        // ═══════════ (E) ANCORA F18/F5: golirea FISCALĂ nu absoarbe restul ═══════════
        // Lot „strâmb”: 3 buc intrate cu 30,01 ⇒ preț 10,003333. Două ieșiri de câte 1
        // (10,00 + 10,00) lasă 1 buc și 10,01 lei — adică un BAN peste `preț × 1`.
        Lot LotCuRest(Produs p) {
            var l = LotNou(p, 3m, 30.01m);
            Bcs(l, new DateOnly(2026, 9, 2), 1m);
            Bcs(l, new DateOnly(2026, 9, 3), 1m);
            return l;
        }
        var lotFiscal = LotCuRest(produsRest);
        var lotGeaman = LotCuRest(produsGeaman);
        s.Check("Api RLF premisă (scena golirii): ambele loturi „strâmbe” au preț 10,003333 și au rămas la 1 buc / "
            + "10,01 — un ban peste `preț × cantitate`",
            lotFiscal.PretUnitar == 10.003333m && lotGeaman.PretUnitar == 10.003333m
            && SoldCheieCub(lotFiscal.ID) == (1m, 10.00m)
            && SoldCheieCub(lotGeaman.ID) == (1m, 10.00m));

        var idFiscal = IdNou(gest.ID, furnizor.ID, new RlfLinieWriteDto {
            TipMaterialId = tip371.ID, LotId = lotFiscal.ID, Cantitate = 1m
        });
        var citFiscal = ReturFurnizorApply.Citeste(os, idFiscal);
        s.Check("ANCORA F18/F5 la CULEGERE: pe cheia pe care returul o va goli, culegerea prezice `q × preț` = 10,00 "
            + "și ATÂT — nu consultă soldul valoric și nu prezice golirea; calea API nu introduce un al doilea adevăr "
            + "despre valoarea ieșirii",
            citFiscal.Linii[0].Valoare == 10.00m && citFiscal.Linii[0].ValoareTva == 2.10m);
        ComenziDocument.Sistem(os).Opereaza(idFiscal);
        // Contrastul: un BON DE CONSUM pe lotul geamăn, în aceeași poziție, ABSOARBE.
        Bcs(lotGeaman, new DateOnly(2026, 9, 12), 1m);
        var soldFiscal = SoldCheieCub(lotFiscal.ID);
        var soldGeaman = SoldCheieCub(lotGeaman.ID);
        var noteFiscal = NoteCub(idFiscal);
        var catreFurnizor = noteFiscal.Rulaj(cont401.ID, N.Latura.Credit);
        Console.WriteLine($"     MĂSURAT (Api RLF, F18/F5): pe DOUĂ loturi identice (1 buc / 10,01) — returul "
            + $"(ieșire FISCALĂ) a scos {citFiscal.Linii[0].Valoare:N2} și a lăsat lotul la "
            + $"{soldFiscal.Cantitate:0.###} / {soldFiscal.Valoare:N2}; consumul obișnuit a ABSORBIT restul și a "
            + $"lăsat lotul la {soldGeaman.Cantitate:0.###} / {soldGeaman.Valoare:N2}. Contul 401 a primit "
            + $"{catreFurnizor:N2} (valoare + TVA), nu banul în plus.");
        s.Check("ANCORA F18/F5 (`IDocumentCuIesireFiscala`, replicată pe calea API): returul GOLEȘTE cantitativ cheia, "
            + "dar NU absoarbe soldul valoric rămas — postează 10,00 (hârtia furnizorului) și lasă pe lot reziduul de "
            + "0,01, raportat în SAF-T S; contul 401 primește 10,00 + 2,10 TVA, iar banul în plus NU cade pe el",
            soldFiscal == (0m, 0.00m)
            && noteFiscal.Nota(cont371.ID, cont401.ID, -10.00m)
            && noteFiscal.Nota(cont4426.ID, cont401.ID, -2.10m)
            && catreFurnizor == -12.10m);
        s.Check("ANCORA F18/F5 (contrastul care face cifra să însemne ceva): pe lotul GEAMĂN, în aceeași poziție, un "
            + "consum obișnuit — care NU e ieșire fiscală — preia tot soldul valoric rămas (F18-D2) și duce cheia la "
            + "0 / 0,00; diferența dintre cele două cifre e exact marker-ul de tip, nu o toleranță",
            SoldCheieCub(lotGeaman.ID) == (0m, 0m));

        // ═══════════ (F) Riscul 6: idempotența semnării prin calea API ═══════════
        var numarRlf = citOperat.Numar;
        ComenziDocument.Sistem(os).AnuleazaOperarea(idRlf);
        var citAnulat = ReturFurnizorApply.Citeste(os, idRlf);
        Console.WriteLine($"     MĂSURAT (Api RLF, riscul 6): după ANULARE documentul e „{citAnulat.Stare}” cu linia "
            + $"încă semnată de operare ({citAnulat.Linii[0].Cantitate:0.###} / {citAnulat.Linii[0].Valoare:N2} / "
            + $"{citAnulat.Linii[0].ValoareTva:N2}) — motorul nu de-semnează.");
        s.Check("Api RLF: anularea readuce Draft-ul, șterge registrele și întoarce soldul lotului la 10 — dar LASĂ "
            + "liniile semnate (motorul nu de-semnează; `Abs`-urile din `PregatesteOperare` fac re-operarea sigură)",
            citAnulat.Stare == "Draft" && citAnulat.Linii[0].Cantitate == -4m
            && CubScena.FaraStoc(os, idRlf)
            && CubScena.FaraNote(os, idRlf)
            && SoldCheieCub(lotIntreg.ID) == (10m, 100m));
        ReturFurnizorApply.Aplica(os, idRlf, Rescriere(citAnulat));
        var citRenormalizat = ReturFurnizorApply.Citeste(os, idRlf);
        Console.WriteLine($"     MĂSURAT (Api RLF, riscul 6): PUT-ul de round-trip al aceluiași ReadDto semnat a "
            + $"readus linia la {citRenormalizat.Linii[0].Cantitate:0.###} / {citRenormalizat.Linii[0].Valoare:N2} / "
            + $"{citRenormalizat.Linii[0].ValoareTva:N2}, iar `Total` la {citRenormalizat.Total:N2}.");
        s.Check("RISCUL 6, MĂSURAT (cine bate pe cine): pe DRAFT bate `MaterializeazaValori` — round-trip-ul unui "
            + "ReadDto SEMNAT (singurul lucru pe care clientul îl are după o anulare) readuce linia la forma de "
            + "CULEGERE, adică magnitudini pozitive, în loc să fie refuzat sau să lase cantitatea negativă lângă o "
            + "valoare pozitivă. La OPERARE bate hook-ul — și cele două nu se contrazic, fiindcă amândouă pleacă din "
            + "`Abs`",
            citRenormalizat.Linii[0].Cantitate == 4m && citRenormalizat.Linii[0].Valoare == 40m
            && citRenormalizat.Linii[0].ValoareTva == 8.4m && citRenormalizat.Total == 48.4m);
        ComenziDocument.Sistem(os).Opereaza(idRlf);
        var citReoperat = ReturFurnizorApply.Citeste(os, idRlf);
        s.Check("RISCUL 6: re-operarea după anulare + PUT dă EXACT aceleași cifre și același număr — semnul nu se "
            + "dublează (nici prin motor, nici prin Apply)",
            citReoperat.Numar == numarRlf && citReoperat.Linii[0].Cantitate == -4m
            && citReoperat.Linii[0].Valoare == -40m && citReoperat.Linii[0].ValoareTva == -8.4m
            && citReoperat.Total == -48.4m
            && SoldCheieCub(lotIntreg.ID) == (6m, 60m));

        // ═══════════ (G) Storno ═══════════
        var dStorno = new DateOnly(2026, 9, 20);
        s.Check("Api RLF: storno prin API → Stornat, rânduri INVERSE append-only (POZITIVE, cu flag `Storno`) la data "
            + "stornării; stocul revine la 10",
            ComenziDocument.Sistem(os).Storneaza(idRlf, dStorno).StareNoua == StareDocument.Stornat
            && CubScena.Note(os, idRlf).Any(p => p.Storno && p.Debit && p.Cont == cont371.ID && p.Valoare == 40m)
            && SoldCheieCub(lotIntreg.ID) == (10m, 100m));
        s.Check("Api RLF: documentul stornat nu mai are nicio afordanță de scriere",
            ReturFurnizorApply.Citeste(os, idRlf) is
                { Stare: "Stornat", PoateEdita: false, PoateOpera: false, PoateAnula: false, PoateStorna: false });
        s.CheckRefuza("Api RLF: PUT pe un document Stornat se refuză (pre-check de domeniu, înaintea gardianului)",
            () => ReturFurnizorApply.Aplica(os, idRlf, Rescriere(ReturFurnizorApply.Citeste(os, idRlf))));
        s.CheckRefuza("Api RLF: DELETE pe un document Stornat se refuză",
            () => ReturFurnizorApply.Sterge(os, idRlf));

        Curata();
        s.Check("Api RLF: curățenie finală (fără reziduuri e2e)",
            !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(Marcaj))
            && !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(Marcaj))
            && os.GetObjectByKey<ReturFurnizor>(idRlf) == null
            && os.GetObjectByKey<ReturFurnizor>(idFiscal) == null);
    }
}
