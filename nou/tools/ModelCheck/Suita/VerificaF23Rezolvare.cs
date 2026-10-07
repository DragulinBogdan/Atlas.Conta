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

// ---------------------------------------------------------------------------
// F23-V2 — REZOLVAREA (`ImpliciteService.TipTva`), pe funcția REALĂ
// ---------------------------------------------------------------------------
// Regimul e al PARTENERULUI, cota e a PRODUSULUI (F23-D2): de-aia rezolvarea are
// două picioare care se împacă la final, nu o listă de trepte care se
// scurtcircuitează la prima potrivire. Scena e proprie și purjată fizic; `data` e
// ziua de azi, ca rândul cu `ValabilDeLa` de mâine să poată fi probat în AMBELE
// sensuri (azi nu se aplică, mâine bate „dintotdeauna”).
static class VerificaF23Rezolvare {
    public static void Ruleaza(Suita s, bool privat) {
        const string Marcaj = "E2E-F23";
        var azi = DateOnly.FromDateTime(DateTime.Today);
        var maine = azi.AddDays(1);

        void CurataF23(IObjectSpace osC) {
            var pj = new Purja(osC);
            // Rândurile de politică ale scenei se recunosc după `ValabilDeLa` NEnul:
            // toate cele șase rânduri seed-uite sunt „dintotdeauna”.
            pj.Adauga(osC.GetObjectsQuery<PoliticaTvaImplicit>().Where(p => p.ValabilDeLa != null));
            pj.Adauga(osC.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(Marcaj)));
            pj.Adauga(osC.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(Marcaj)));
            pj.Executa();
        }

        using var os = s.Provider.CreateObjectSpace();
        CurataF23(os);

        var fcl = os.FirstOrDefault<TipDocument>(t => t.Cod == "FCL");
        var fct = os.FirstOrDefault<TipDocument>(t => t.Cod == "FCT");
        var btr = os.FirstOrDefault<TipDocument>(t => t.Cod == "BTR");
        // Ancorele profilului: privat N21, bugetar CAP21 — proba nu le scrie în
        // literă, le citește de pe tipul de document (29: motorul e agnostic la plan).
        string Cod(Guid? id) => id == null ? null
            : os.GetObjectsQuery<TipTva>().Where(t => t.ID == id.Value).Select(t => t.Cod).FirstOrDefault();
        var codAncoraFcl = Cod(fcl.TipTvaImplicitId);
        var codAncoraFct = Cod(fct.TipTvaImplicitId);

        Partener Part(string sufix, string tara, bool inregistrat, TipTva implicitTva = null) {
            var p = os.CreateObject<Partener>();
            p.Cod = Marcaj + sufix;
            p.Denumire = "Partener probă F23 " + sufix;
            p.TipPersoana = TipPersoana.Juridica;
            p.Tara = tara;
            p.InregistratTva = inregistrat;
            p.TipTvaImplicit = implicitTva;
            return p;
        }
        var pRo = Part("-RO", "RO", true);
        var pUe = Part("-UE", "DE", false);
        var pXu = Part("-XU", "US", false);
        os.CommitChanges();

        if (!privat) {
            // Bugetarul e neplătitor: zero rânduri de politică, ancora ajunge
            // (F23-D2). Absența se PROBEAZĂ, nu se presupune.
            var randuriB = os.GetObjectsQuery<PoliticaTvaImplicit>().Count();
            var rUe = ImpliciteService.TipTva(os, fct.ID, pUe.ID, null, azi);
            var rRo = ImpliciteService.TipTva(os, fct.ID, pRo.ID, null, azi);
            var rXu = ImpliciteService.TipTva(os, fct.ID, pXu.ID, null, azi);
            Console.WriteLine($"     MĂSURAT (F23-V2/bugetar): {randuriB} rânduri `PoliticaTvaImplicit`; "
                + $"ancora FCT = {codAncoraFct}; FCT×UE → {Cod(rUe.TipTvaId) ?? "<niciun tip>"}/{rUe.Sursa}; "
                + $"FCT×RO → {Cod(rRo.TipTvaId) ?? "<niciun tip>"}/{rRo.Sursa}; "
                + $"FCT×extraUE → {Cod(rXu.TipTvaId) ?? "<niciun tip>"}/{rXu.Sursa}.");
            s.Check("F23-V2 (bugetar) profilul n-are NICIO politică de implicit: totul e capitalizat, deci "
                + $"rezolvarea cade pe ancora tipului ({codAncoraFct}) indiferent de clasa fiscală a "
                + "partenerului — RO, UE și extra-UE dau ACEEAȘI cifră, din aceeași sursă",
                randuriB == 0
                && Cod(rUe.TipTvaId) == codAncoraFct && rUe.Sursa == SursaImplicit.Ancora
                && Cod(rRo.TipTvaId) == codAncoraFct && rRo.Sursa == SursaImplicit.Ancora
                && Cod(rXu.TipTvaId) == codAncoraFct && rXu.Sursa == SursaImplicit.Ancora);
            CurataF23(os);
            return;
        }

        // ---------------- Scena privată ----------------
        var n11 = os.FirstOrDefault<TipTva>(t => t.Cod == "N11");
        var n19 = os.FirstOrDefault<TipTva>(t => t.Cod == "N19");
        var pN19 = Part("-N19", "RO", true, n19);
        // Golurile închise de 83f/g: RO NEÎNREGISTRAT (clasa fiscală 2) și extra-UE
        // pe latura de ACHIZIȚIE — până la felia 24 amândouă cădeau pe ancora N21.
        var pRoNe = Part("-RONE", "RO", false);
        var produs = os.CreateObject<Produs>();
        produs.Cod = Marcaj + "-P";
        produs.Denumire = "Produs probă F23 (cotă redusă)";
        produs.UM = "BUC";
        produs.TipTvaImplicit = n11;
        // Rândul cu VALABILITATE: „de mâine, livrarea intracomunitară trece pe N11”.
        // Nu e realist fiscal — e proba MECANISMULUI cu care se schimbă cotele la o
        // dată (1 august 2025 e precedentul real).
        var randMaine = os.CreateObject<PoliticaTvaImplicit>();
        randMaine.TipDocument = fcl;
        randMaine.ClasaFiscala = ClasaFiscalaPartener.Ue;
        randMaine.ValabilDeLa = maine;
        randMaine.TipTva = n11;
        os.CommitChanges();

        var rezRo = ImpliciteService.TipTva(os, fcl.ID, pRo.ID, null, azi);
        var rezUe = ImpliciteService.TipTva(os, fcl.ID, pUe.ID, null, azi);
        var rezXu = ImpliciteService.TipTva(os, fcl.ID, pXu.ID, null, azi);
        var rezFara = ImpliciteService.TipTva(os, fcl.ID, null, null, azi);
        var rezInexistent = ImpliciteService.TipTva(os, fcl.ID, Guid.NewGuid(), null, azi);
        var rezFctUe = ImpliciteService.TipTva(os, fct.ID, pUe.ID, null, azi);
        var rezRoProdus = ImpliciteService.TipTva(os, fcl.ID, pRo.ID, produs.ID, azi);
        var rezUeProdus = ImpliciteService.TipTva(os, fcl.ID, pUe.ID, produs.ID, azi);
        var rezBtr = ImpliciteService.TipTva(os, btr.ID, pRo.ID, null, azi);
        var rezInactiv = ImpliciteService.TipTva(os, fcl.ID, pN19.ID, null, azi);
        var rezMaine = ImpliciteService.TipTva(os, fcl.ID, pUe.ID, null, maine);
        var rlf = os.FirstOrDefault<TipDocument>(t => t.Cod == "RLF");
        var rezFctXu = ImpliciteService.TipTva(os, fct.ID, pXu.ID, null, azi);
        var rezRlfXu = ImpliciteService.TipTva(os, rlf.ID, pXu.ID, null, azi);
        var rezFctRoNe = ImpliciteService.TipTva(os, fct.ID, pRoNe.ID, null, azi);
        var rezRlfRoNe = ImpliciteService.TipTva(os, rlf.ID, pRoNe.ID, null, azi);

        string Descrie(ImpliciteService.RezultatImplicit r) =>
            $"{Cod(r.TipTvaId) ?? "<niciun tip>"}/{r.Sursa}";
        Console.WriteLine($"     MĂSURAT (F23-V2/rezolvare, ancora FCL = {codAncoraFcl}): "
            + $"FCL×RO → {Descrie(rezRo)}; FCL×UE → {Descrie(rezUe)}; FCL×extraUE → {Descrie(rezXu)}; "
            + $"FCL×fără partener → {Descrie(rezFara)}; FCL×partener inexistent → {Descrie(rezInexistent)}; "
            + $"FCT×UE → {Descrie(rezFctUe)}; FCL×RO+produs N11 → {Descrie(rezRoProdus)}; "
            + $"FCL×UE+produs N11 → {Descrie(rezUeProdus)}; BTR×RO → {Descrie(rezBtr)}; "
            + $"FCL×partener cu implicit N19 (inactiv) → {Descrie(rezInactiv)}; "
            + $"FCL×UE la {maine:dd.MM.yyyy} → {Descrie(rezMaine)}.");

        s.Check("F23-V2 treptele de REGIM, în ordinea din F23-D2: partenerul înregistrat RO n-are rând de politică "
            + $"⇒ cade pe ancora tipului ({codAncoraFcl}); UE și extra-UE potrivesc rândurile seed-uite ⇒ SDD din "
            + "POLITICĂ (livrarea intracomunitară e scutită, art. 294); pe FCT aceeași clasă UE dă TI21 — sensul "
            + "e al tipului de document, nu al partenerului",
            Cod(rezRo.TipTvaId) == codAncoraFcl && rezRo.Sursa == SursaImplicit.Ancora
            && Cod(rezUe.TipTvaId) == "SDD" && rezUe.Sursa == SursaImplicit.Politica
            && Cod(rezXu.TipTvaId) == "SDD" && rezXu.Sursa == SursaImplicit.Politica
            && Cod(rezFctUe.TipTvaId) == "TI21" && rezFctUe.Sursa == SursaImplicit.Politica);

        s.Check("F23-V2 FĂRĂ ORACOL DE EXISTENȚĂ (80a): partenerul LIPSĂ de pe document și partenerul INEXISTENT "
            + "(un Guid aleator) dau același rezultat ȘI EXACT ACELAȘI motiv, șir cu șir — altfel un utilizator "
            + "fără drept pe nomenclator ar afla din indiciul de sub câmp că partenerul cerut există",
            rezFara.Motiv == rezInexistent.Motiv
            && Cod(rezFara.TipTvaId) == codAncoraFcl && rezFara.Sursa == SursaImplicit.Ancora
            && Cod(rezInexistent.TipTvaId) == codAncoraFcl && rezInexistent.Sursa == SursaImplicit.Ancora
            && rezFara.Motiv.Contains("Fără partener vizibil"));

        s.Check("F23-V2 ÎMPĂCAREA (F23-D2.3): produsul își impune COTA doar când regimul coincide — pe un partener "
            + "RO înregistrat (regim Normal, ca N11) câștigă N11 din PRODUS; pe un partener UE (SDD, regim "
            + "Scutit) câștigă SDD din POLITICĂ, fiindcă o livrare intracomunitară e scutită indiferent ce produs "
            + "conține",
            Cod(rezRoProdus.TipTvaId) == "N11" && rezRoProdus.Sursa == SursaImplicit.Produs
            && Cod(rezUeProdus.TipTvaId) == "SDD" && rezUeProdus.Sursa == SursaImplicit.Politica);

        s.Check("F23-V2 un tip de document fără ancoră și fără politică (BTR) nu inventează un implicit: `Niciuna`, "
            + "cu motiv — linia rămâne fără TVA, ceea ce e chiar adevărul (transferul n-are fapt de TVA)",
            rezBtr.TipTvaId == null && rezBtr.Sursa == SursaImplicit.Niciuna
            && !string.IsNullOrWhiteSpace(rezBtr.Motiv));

        s.Check("F23-V2 tipul INACTIV nu se alege niciodată (F23-D2.6): partenerul are implicit N19 (cotă istorică, "
            + "stinsă de seed), deci se sare TOATĂ treapta partenerului — rezultatul cade pe ancoră "
            + $"({codAncoraFcl}), iar motivul SPUNE că s-a sărit un tip inactiv",
            Cod(rezInactiv.TipTvaId) == codAncoraFcl && rezInactiv.Sursa == SursaImplicit.Ancora
            && rezInactiv.Motiv.Contains("INACTIV") && rezInactiv.Motiv.Contains("N19"));

        s.Check("F23-V2 `ValabilDeLa`: rândul cu dată NU se aplică înaintea ei (azi rămâne SDD, „dintotdeauna”), iar "
            + "de la data lui bate „dintotdeauna” la egalitate de clasă (mâine devine N11) — mecanismul cu care o "
            + "schimbare de cotă intră fără să rescrie facturile vechi",
            Cod(rezUe.TipTvaId) == "SDD"
            && Cod(rezMaine.TipTvaId) == "N11" && rezMaine.Sursa == SursaImplicit.Politica);

        Console.WriteLine($"     MĂSURAT (F24-V7/implicitele noi): FCT×extraUE → {Descrie(rezFctXu)}; "
            + $"RLF×extraUE → {Descrie(rezRlfXu)}; FCT×RO neînregistrat → {Descrie(rezFctRoNe)}; "
            + $"RLF×RO neînregistrat → {Descrie(rezRlfRoNe)}.");
        s.Check("F24-V7 golurile lui 81-r1 primesc RÂND, nu tăcere (83f/g): achiziția de la un neînregistrat RO "
            + "propune NIM (nu există fapt de TVA pe linie — decurge din CLASĂ), iar achiziția extra-UE propune "
            + "IMP (taxa se datorează în vamă, pe DVI). Amândouă din POLITICĂ, simetric pe FCT și pe RLF — "
            + "înainte cădeau pe ancora N21, plauzibilă pe orice linie și greșită pe amândouă",
            Cod(rezFctXu.TipTvaId) == "IMP" && rezFctXu.Sursa == SursaImplicit.Politica
            && Cod(rezRlfXu.TipTvaId) == "IMP" && rezRlfXu.Sursa == SursaImplicit.Politica
            && Cod(rezFctRoNe.TipTvaId) == "NIM" && rezFctRoNe.Sursa == SursaImplicit.Politica
            && Cod(rezRlfRoNe.TipTvaId) == "NIM" && rezRlfRoNe.Sursa == SursaImplicit.Politica);

        // `PartenerulDocumentului` — fără `is`/`switch` pe frunze (invariantul II).
        // Documentele NU se comit: proba e despre CITIREA laturilor, nu despre
        // persistență, iar FK-urile se pun direct ca să nu depindă de `SaveChanges`.
        Guid? partenerFcl, partenerBtr;
        using (var osDoc = s.Provider.CreateObjectSpace()) {
            var gestiuni = osDoc.GetObjectsQuery<Gestiune>().Select(g => g.ID).Take(2).ToList();
            var unitate = osDoc.GetObjectsQuery<UnitateInterna>().Select(u => u.ID).FirstOrDefault();
            var docFcl = osDoc.CreateObject<FacturaIesire>();
            docFcl.PredatorId = unitate;
            docFcl.PrimitorId = pRo.ID;
            partenerFcl = ImpliciteService.PartenerulDocumentului(osDoc, docFcl);
            var docBtr = osDoc.CreateObject<NotaTransfer>();
            docBtr.PredatorId = gestiuni.Count > 0 ? gestiuni[0] : Guid.NewGuid();
            docBtr.PrimitorId = gestiuni.Count > 1 ? gestiuni[1] : Guid.NewGuid();
            partenerBtr = ImpliciteService.PartenerulDocumentului(osDoc, docBtr);
            osDoc.Rollback();
        }
        Console.WriteLine($"     MĂSURAT (F23-V2/laturi): FCL (primitor = partener) → "
            + $"{(partenerFcl == pRo.ID ? "primitorul" : partenerFcl?.ToString() ?? "null")}; "
            + $"BTR (două gestiuni) → {partenerBtr?.ToString() ?? "null"}.");
        s.Check("F23-V2 `PartenerulDocumentului` pune întrebarea NOMENCLATORULUI („e `Partener` cel de pe latura "
            + "asta?”), nu documentului: pe o factură de ieșire întoarce primitorul, pe un transfer între două "
            + "gestiuni întoarce null — zero `is`/`switch` pe frunze (invariantul II)",
            partenerFcl == pRo.ID && partenerBtr == null);

        // ACEEAȘI întrebare, dar cu laturile MATERIALIZATE în ObjectSpace — adică
        // exact calea REST, unde `Apply` rezolvă FK-urile înainte, deci repartitorul e
        // deja în change tracker-ul EF. Proba de mai sus NU prindea defectul tocmai
        // fiindcă un id nematerializat nu găsește nimic de castat.
        //
        // Ce s-a măsurat pe host (F23 pas 2): cu forma dintâi
        // (`os.GetObjectByKey<Partener>(id) != null`), `BaseObjectSpace.GetObjectByKey<T>`
        // găsea proxy-ul de `UnitateInterna` și ARUNCA `InvalidCastException` în loc să
        // întoarcă null — adică 500 pe ORICE `POST`/`PUT` al celor cinci felii cu TVA,
        // fiindcă predatorul e intern pe FCL/RLF/RDC. De aceea întrebarea se pune prin
        // INTEROGARE pe nomenclator (`GetObjectsQuery<Partener>().Any(...)`): același
        // răspuns dorit — null pentru altă frunză ȘI pentru un partener invizibil
        // (80a) —, fără cast.
        Guid? partenerFclMaterializat, partenerFctMaterializat;
        Exception exceptieLaturi = null;
        using (var osDoc = s.Provider.CreateObjectSpace()) {
            partenerFclMaterializat = partenerFctMaterializat = null;
            try {
                // Materializarea e ESENȚA probei: obiectele, nu id-urile.
                var unitate = osDoc.GetObjectsQuery<UnitateInterna>().FirstOrDefault();
                var gestiune = osDoc.GetObjectsQuery<Gestiune>().FirstOrDefault();
                var partener = osDoc.GetObjectByKey<Partener>(pRo.ID);
                var docFcl = osDoc.CreateObject<FacturaIesire>();
                docFcl.PredatorId = unitate?.ID ?? Guid.Empty;
                docFcl.PrimitorId = partener?.ID ?? pRo.ID;
                partenerFclMaterializat = ImpliciteService.PartenerulDocumentului(osDoc, docFcl);
                var docFct = osDoc.CreateObject<FacturaIntrare>();
                docFct.PredatorId = partener?.ID ?? pRo.ID;
                docFct.PrimitorId = gestiune?.ID ?? Guid.Empty;
                partenerFctMaterializat = ImpliciteService.PartenerulDocumentului(osDoc, docFct);
            }
            catch (Exception ex) {
                exceptieLaturi = ex;
            }
            osDoc.Rollback();
        }
        Console.WriteLine("     MĂSURAT (F23-V2/laturi materializate): FCL (predator `UnitateInterna` ÎNCĂRCATĂ) → "
            + $"{(partenerFclMaterializat == pRo.ID ? "primitorul" : partenerFclMaterializat?.ToString() ?? "null")}; "
            + $"FCT (predator partener, primitor `Gestiune` ÎNCĂRCATĂ) → "
            + $"{(partenerFctMaterializat == pRo.ID ? "predatorul" : partenerFctMaterializat?.ToString() ?? "null")}; "
            + $"excepție: {exceptieLaturi?.GetType().Name ?? "niciuna"}.");
        s.Check("F23-V2 `PartenerulDocumentului` nu ARUNCĂ pe o latură de altă frunză, nici când repartitorul e deja "
            + "MATERIALIZAT în ObjectSpace (calea REST, unde `Apply` rezolvă FK-urile înainte): FCL cu predator "
            + "`UnitateInterna` → primitorul, FCT cu primitor `Gestiune` → predatorul. `GetObjectByKey<Partener>` "
            + "CASTA proxy-ul celeilalte frunze (`InvalidCastException` ⇒ 500 pe toate scrierile cu TVA, măsurat pe "
            + "host); întrebarea se pune acum prin interogare pe nomenclator",
            exceptieLaturi == null
            && partenerFclMaterializat == pRo.ID && partenerFctMaterializat == pRo.ID);

        CurataF23(os);
        s.Check("F23-V2 curățenie: scena purjată FIZIC (70e) — partenerii, produsul și rândul de politică cu "
            + "`ValabilDeLa` dispar, iar cele șase rânduri seed-uite rămân neatinse",
            !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(Marcaj))
            && !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(Marcaj))
            && !os.GetObjectsQuery<PoliticaTvaImplicit>().Any(p => p.ValabilDeLa != null)
            && os.GetObjectsQuery<PoliticaTvaImplicit>().Count() == 11);
    }
}
