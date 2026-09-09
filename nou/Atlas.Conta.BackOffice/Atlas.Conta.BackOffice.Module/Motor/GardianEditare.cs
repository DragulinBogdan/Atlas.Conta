using System.ComponentModel;
using System.Runtime.CompilerServices;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Saft;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Core;
using DevExpress.ExpressApp.EFCore;
using DevExpress.ExpressApp.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Atlas.Conta.BackOffice.Module.Motor;

// Gardianul transversal de scriere (decizia 42a / spike D4): O SINGURĂ sursă de
// reguli pentru toate tierele de UI. Regulile de mai jos nu depind de ecran, de
// controller sau de endpoint — trăiesc pe `Committing`-ul ObjectSpace-ului și
// se aplică oricui scrie prin el.
//
// ═══ De ce ESTE seam-ul corect (probe pe sursele DevExpress 26.1.3) ═══
//
// Distincția secured / non-secured NU se face aici prin vreun token sau flag:
// e STRUCTURALĂ, dată de FABRICA prin care s-a născut ObjectSpace-ul. XAF are
// trei familii de customizeri, invocate din trei locuri diferite:
//
//   DevExpress.ExpressApp\Services\Core\Internal\ObjectSpaceCustomizerService.cs
//     :60-65  OnObjectSpaceCreated          → IObjectSpaceCustomizer            (ASTA)
//     :66-71  OnNonSecuredObjectSpaceCreated→ INonSecuredObjectSpaceCustomizer
//     :72-77  OnUpdatingObjectSpaceCreated  → IUpdatingObjectSpaceCustomizer
//
// (1) OS-urile SECURED din `IObjectSpaceFactory` (WebApi: DataService/OData și
//     endpoint-urile noastre viitoare) trec prin
//     `Services\Core\Internal\ObjectSpaceFactory.cs:50-54` —
//     `objectSpaceCustomizerService.OnObjectSpaceCreated(objectSpace)`.
// (2) OS-urile View-urilor din Blazor vin din `XafApplication.CreateObjectSpace`
//     (`XafApplication.cs:2416-2427`), care apelează `OnObjectSpaceCreated`
//     (`XafApplication.cs:1144-1150`) → același `IObjectSpaceCustomizerService`.
//     (Calea alternativă Blazor `LegacyObjectSpaceFactoryWrapper` ajunge tot la
//     una dintre cele două — `Blazor\Services\XafApplicationFactory\
//     ObjectSpaceFactoryWrapper.cs:53-60`.)
// (3) OS-urile NON-SECURED — cele ale MOTORULUI (`OperareApi`) — vin din
//     `Services\Core\Internal\NonSecuredObjectSpaceFactory.cs:51-55`, care
//     invocă `OnNonSecuredObjectSpaceCreated`, adică ALTĂ interfață; la fel
//     `XafApplication.CreateLogonObjectSpace` (`XafApplication.cs:2450, 2463-2468`).
//     Gardianul NU se activează acolo — ușa de sistem rămâne deschisă exact
//     pentru motor, care are voie să scrie registre și să schimbe `Stare`.
// (4) OS-urile de UPDATE (ModuleUpdater/seed) merg pe
//     `NonSecuredObjectSpaceFactory.cs:56-60` → `OnUpdatingObjectSpaceCreated`,
//     tot altă interfață — seed-ul nu vede gardianul.
// (5) Căile STANDALONE (ModelCheck/Import1C/Migrare, pe
//     `EFCoreObjectSpaceProvider` direct, fără host/DI) n-au nici
//     `IObjectSpaceCustomizerService`, nici înregistrarea din `AddXaf` —
//     rămân integral neatinse.
//
// Costul: o înregistrare de serviciu în FIECARE host (extensia
// `AddContaGardianEditare` de mai jos) — modulele XAF nu pot înregistra servicii
// în DI, iar `XafApplication.ObjectSpaceCreated` (evenimentul) ar fi acoperit
// DOAR Blazor-ul, nu și OS-urile din `IObjectSpaceFactory` ale WebApi.
public sealed class GardianEditare : IObjectSpaceCustomizer {
    // Un ObjectSpace trece prin exact o fabrică, deci printr-un singur apel de
    // customizer; marcajul e plasă (chei slabe — nu ține OS-urile în viață).
    static readonly ConditionalWeakTable<IObjectSpace, object> abonate = new();
    static readonly object marcaj = new();

    // Strategia de securitate a CERERII curente, injectată (felia 22, F22-D3).
    // Nullable DELIBERAT: gardianul e înregistrat în DI-ul host-urilor, dar
    // aceeași clasă e folosită și acolo unde nu există securitate. Parametrul
    // opțional e onorat de containerul Microsoft — `CallSiteFactory` cade pe
    // `ParameterDefaultValue` când tipul nu e înregistrat — deci înregistrarea
    // rămâne cea din `AddContaGardianEditare`, fără fabrică proprie.
    readonly ISecurityStrategyBase securitate;

    public GardianEditare(ISecurityStrategyBase securitate = null) =>
        this.securitate = securitate;

    public void OnObjectSpaceCreated(IObjectSpace objectSpace) {
        if (objectSpace == null)
            return;
        lock (abonate) {
            if (abonate.TryGetValue(objectSpace, out _))
                return;
            abonate.Add(objectSpace, marcaj);
        }
        objectSpace.Committing += OnCommitting;
    }

    // Ordinea e conținutul deciziei F22-D3: ÎNTÂI dreptul, apoi domeniul.
    // Nu mai e statică fiindcă pasul zero are nevoie de strategia injectată.
    void OnCommitting(object sender, CancelEventArgs e) {
        if (sender is not IObjectSpace os)
            return;
        VerificaAcces(os);
        Verifica(os);
    }

    // ═══ Pasul ZERO: are voie? (felia 22, F22-D3) ═══
    //
    // De ce ÎNAINTEA domeniului. Mecanica DevExpress (26.1.3) e:
    // `BaseObjectSpace.CommitChanges` → `OnCommitting` (aici) → `DoCommit` →
    // `SaveChanges` → `SecurityStateManager.GetEntriesToSave`, adică verificarea
    // Create/Write/Delete per entitate. Verificarea EXISTĂ, dar rulează DUPĂ
    // noi — așa că un utilizator fără niciun drept de scriere primea mai întâi
    // refuzul nostru de DOMENIU (422 „Codul e obligatoriu…") și nu afla
    // niciodată că, de fapt, n-avea voie să scrie deloc (77k). Un 422 nu poate
    // ascunde un refuz de permisiune (F22-D1), deci întrebarea de drept se pune
    // prima.
    //
    // Ce NU face: nu înlocuiește verificarea din `SaveChanges`. Aceea rămâne
    // plasa — permisiuni pe MEMBRU, criterii pe obiect evaluate în alt punct al
    // ciclului, tot ce nu vedem noi din `ModifiedObjects`. Aici se rezolvă
    // ORDINEA față de domeniu, nu se mută securitatea.
    //
    // Fără strategie (ModelCheck, unelte standalone, orice host care nu
    // înregistrează `ISecurityStrategyBase`) pasul TACE: acolo nu există
    // utilizator, deci nu există întrebare de drept — nu „refuz din prudență".
    // La fel când strategia nu e una care poate răspunde la cereri de
    // permisiune (`SecurityDummy` implementează `IRequestSecurity`, dar nu
    // `IRequestSecurityStrategy`, și oricum răspunde `true` la orice).
    //
    // Utilizatorul ADMINISTRATIV trece întreg pasul: un rol cu
    // `IsAdministrative` produce `IsAdministratorPermission`
    // (`PermissionsExtractor.cs:51-53`), pe care `PermissionRequestProcessor`
    // îl citește ca `RoleType.AllowAllWithoutPermissions`
    // (`PermissionRequestProcessor.cs:671-672`) — orice operație e acordată.
    // Rolul `Administrators` al bazei e chiar `IsAdministrative = true`
    // (`Updater.CreateAdminRole`), deci `Admin` nu poate fi refuzat aici.
    void VerificaAcces(IObjectSpace os) {
        if (securitate is not IRequestSecurityStrategy cerinte)
            return;
        foreach (var obj in os.ModifiedObjects) {
            if (obj == null)
                continue;
            // Trei întrebări diferite, nu una: în XAF `Write` și `Delete` sunt
            // permisiuni DISTINCTE, iar `Create` se pune pe TIP (obiectul nou
            // încă nu are identitate pe care să se evalueze un criteriu).
            // Tipul se dezproxează: `CanCreate` primește un `Type`, iar
            // `FacturaIntrareProxy` nu e în modelul de securitate.
            if (os.IsNewObject(obj)) {
                // Create ȘI Write (review 80 M2): plasa DevExpress cere pe `Added`
                // Create + Write + Read (`SecurityStateManager.
                // CheckIsGrantedToSave` :94-96, :197-198). Un rol cu Create fără
                // Write ar fi trecut aici și ar fi picat în plasă cu textul ei
                // englezesc. Mesajul rămâne „crea" — dreptul lipsă e al creării
                // complete.
                var tip = Api.Refuzuri.TipReal(obj.GetType());
                if (!cerinte.CanCreate(tip, os) || !cerinte.CanWrite(os, (object)obj))
                    throw new Api.RefuzAcces(Api.OperatieAcces.Creare, tip);
            }
            else if (EsteSters(os, obj)) {
                // Supraîncărcarea pe INSTANȚĂ (cast explicit la `object`, ca în
                // gate-urile din controllere): permisiunea de ștergere poate
                // purta criteriu pe obiect. `PermissionRequestProcessor.
                // GetTypeToProcess` dezproxează el tipul pentru aceste
                // supraîncărcări.
                if (!cerinte.CanDelete(os, (object)obj))
                    throw new Api.RefuzAcces(Api.OperatieAcces.Stergere,
                        Api.Refuzuri.TipReal(obj.GetType()));
            }
            else if (!cerinte.CanWrite(os, (object)obj)) {
                throw new Api.RefuzAcces(Api.OperatieAcces.Modificare,
                    Api.Refuzuri.TipReal(obj.GetType()));
            }
        }
    }

    // Regulile de DOMENIU, în ordinea din contract (D4). Toate lipsurile se
    // cumulează și se raportează împreună, ca în motor (mesajele se despart pe
    // „\n").
    //
    // Rămâne STATICĂ și neatinsă de pasul zero (F22-D3): ModelCheck o cheamă
    // direct, pe ObjectSpace-uri fără securitate, ca să probeze regulile de
    // domeniu izolat de orice permisiune. Dreptul se verifică în `OnCommitting`,
    // înaintea ei — nu înăuntru.
    public static void Verifica(IObjectSpace os) {
        var erori = new List<string>();
        var registruRaportat = false;
        // Lista se materializează: ramura de PROVENIENȚĂ (F23-D4) SCRIE pe
        // obiectele parcurse (`DinSeed = false`), iar `ModifiedObjects` e o
        // vedere peste change tracker-ul EF — nu se enumeră în timp ce se scrie
        // în el. Nicio intrare nouă nu apare (proprietatea unui obiect deja
        // modificat), dar snapshot-ul face garanția explicită, nu presupusă.
        foreach (var obj in os.ModifiedObjects.Cast<object>().ToList()) {
            // (i) 77-r2 — orice nomenclator căutabil are cod și denumire.
            // ÎNAINTEA switch-ului: regula e a INTERFEȚEI, nu a unui tip, iar
            // `case Partener`/`case Cont` de mai jos ar înghiți potrivirea.
            if (obj is ICuCautare rand && !EsteSters(os, obj))
                VerificaCodDenumire(rand, erori);
            // (j) F23-D4 — proveniența, tot al INTERFEȚEI și tot înaintea
            // switch-ului: cele 17 tipuri `ICuProvenienta` n-au toate un `case`.
            if (obj is ICuProvenienta provenit && !EsteSters(os, obj))
                VerificaProvenienta(os, provenit, erori);
            switch (obj) {
                // (b) Registrele sunt append-only și EXCLUSIV ale motorului
                // (decizia 14): nimeni nu le scrie prin UI/API, nici măcar
                // administratorul. Un singur mesaj, oricâte rânduri ar fi.
                case RegistruStoc:
                case RegistruContabil:
                // Al treilea registru (felia 11) intră pe aceeași regulă: e scris
                // de motor în aceeași tranzacție cu celelalte două, iar jurnalele
                // de TVA sunt declarații — o editare directă ar fi exact genul de
                // „corecție" pe care append-only-ul o interzice.
                case RegistruTva:
                    if (!registruRaportat) {
                        registruRaportat = true;
                        erori.Add("Registrele (stoc/contabil/TVA) se scriu doar de motor, la operare — "
                            + "nu se creează, modifică sau șterg direct.");
                    }
                    break;
                case Document doc:
                    VerificaDocument(os, doc, erori);
                    break;
                case DocumentDetaliu linie:
                    VerificaLinie(os, linie, erori);
                    break;
                case Imperechere imperechere:
                    VerificaImperechere(os, imperechere, erori);
                    break;
                // (d) Nomenclatorul de conturi: `Parinte` nu poate închide un
                // ciclu (F13-D4 / restanța 67e).
                case Cont cont:
                    VerificaCont(os, cont, erori);
                    break;
                // (e) Nomenclatorul de parteneri: `Tara` e cod ISO-2 (felia 14,
                // D4-D1). Regula XAF n-o vede scrierea prin OData (55b) — aici
                // e a doua jumătate, pe aceeași ușă secured.
                case Partener partener:
                    VerificaPartener(os, partener, erori);
                    break;
                // (f) Societatea raportoare (felia 16, D16-D1): un singur rând,
                // județ doar pe RO, bază contabilă din lista ANAF.
                case Societate societate:
                    VerificaSocietate(os, societate, erori);
                    break;
                // (g) Produsul (felia 16, D16-D2): `CodNc` are exact 8 cifre.
                case Produs produs:
                    VerificaProdus(produs, erori);
                    break;
                // (h) Politica de mișcare SAF-T (felia 17, D17-D1): codul din
                // nomenclator, motivul obligatoriu la excluderea deliberată,
                // semnul din {−1, null, +1}.
                case PoliticaMiscareSaft politica:
                    VerificaPoliticaMiscareSaft(os, politica, erori);
                    break;
                // ── F23-D5: ușa OData se deschide pe politici, deci fiecare
                // politică deschisă își aduce invarianții AICI. Regulile XAF de
                // pe proprietăți rămân (sunt prezentarea), dar nu rulează pe API
                // (55b) — jumătatea de fond e pe ușa comună.
                case TipDocument tipDoc:
                    VerificaTipDocument(os, tipDoc, erori);
                    break;
                case TipTva tipTva:
                    VerificaTipTva(os, tipTva, erori);
                    break;
                case PoliticaTvaImplicit implicit_:
                    VerificaPoliticaTvaImplicit(os, implicit_, erori);
                    break;
                case PoliticaTva politicaTva:
                    VerificaPoliticaTva(politicaTva, erori);
                    break;
                case RegulaContare regulaContare:
                    VerificaRegulaContare(regulaContare, erori);
                    break;
                case RegulaStoc regulaStoc:
                    VerificaRegulaStoc(regulaStoc, erori);
                    break;
                case PoliticaNumerotare numerotare:
                    VerificaPoliticaNumerotare(numerotare, erori);
                    break;
                case PoliticaScadenta scadenta:
                    VerificaPoliticaScadenta(scadenta, erori);
                    break;
                case PoliticaInchidereTva inchidere:
                    VerificaPoliticaInchidereTva(inchidere, erori);
                    break;
                case MapareD300 mapareD300:
                    VerificaMapareD300(os, mapareD300, erori);
                    break;
                case MapareD394 mapareD394:
                    VerificaMapareD394(mapareD394, erori);
                    break;
            }
        }
        if (erori.Count > 0)
            throw new OperareException(string.Join("\n", erori.Distinct()));
    }

    // (a) Documentul: se culege cât e Draft. Starea e SERVER-OWNED — tranzițiile
    // le face doar motorul, în ObjectSpace-ul lui non-secured. Review-ul advers
    // al spike-ului (F3) a lărgit paza pe TOATE câmpurile stăpânite de motor:
    // `DataOperare`/`Autogenerat`/`DocumentSursaId` (mecanismul grupului conex —
    // un draft care „se declară" copil al altui document i-ar manipula anularea),
    // iar `Numar` la tipurile cu PoliticaNumerotare (`AsignaNumar` ONOREAZĂ un
    // număr pre-completat — corect la re-operare, în OS-ul motorului, dar din
    // secured ar ocoli seria). La tipurile FĂRĂ politică (FCT) numărul rămâne
    // culegere liberă.
    static void VerificaDocument(IObjectSpace os, Document doc, ICollection<string> erori) {
        if (os.IsNewObject(doc)) {
            if (doc.Stare != StareDocument.Draft)
                erori.Add($"Un document nou se creează în starea Draft, nu „{doc.Stare}” "
                    + "— operarea îi schimbă starea.");
            if (doc.Autogenerat || doc.DocumentSursaId != null || doc.DataOperare != null)
                erori.Add("Legătura de grup conex (Autogenerat/DocumentSursa) și DataOperare "
                    + "le scrie doar motorul.");
            if (!string.IsNullOrEmpty(doc.Numar) && AreNumerotare(os, doc))
                erori.Add($"Numărul documentului vine din seria tipului (PoliticaNumerotare) "
                    + "— nu se culege.");
            return;
        }
        var originale = Originale(os, doc);
        var stareOriginala = (originale?[nameof(Document.Stare)] as StareDocument?) ?? doc.Stare;
        if (stareOriginala != StareDocument.Draft) {
            erori.Add($"Documentul {Eticheta(doc)} nu mai e Draft (starea „{stareOriginala}”) — "
                + "nu se mai modifică și nu se șterge. Anulați operarea sau stornați-l.");
            return;
        }
        if (EsteSters(os, doc) || originale == null)
            return;
        if (doc.Stare != stareOriginala)
            erori.Add($"Starea documentului {Eticheta(doc)} o schimbă doar motorul "
                + "(Operează / Anulează operarea / Stornează).");
        if (!Equals(originale[nameof(Document.DataOperare)], doc.DataOperare)
                || !Equals(originale[nameof(Document.Autogenerat)], doc.Autogenerat)
                || !Equals(originale[nameof(Document.DocumentSursaId)], doc.DocumentSursaId))
            erori.Add($"Câmpurile de operare și de grup conex ale documentului {Eticheta(doc)} "
                + "(DataOperare, Autogenerat, DocumentSursa) le scrie doar motorul.");
        var numarOriginal = originale[nameof(Document.Numar)] as string;
        if (!string.Equals(numarOriginal ?? "", doc.Numar ?? "", StringComparison.Ordinal)
                && AreNumerotare(os, doc))
            erori.Add($"Numărul documentului {Eticheta(doc)} vine din seria tipului "
                + "(PoliticaNumerotare) — nu se editează.");
    }

    // (a) Liniile urmează starea documentului-gazdă (registrele s-au scris din
    // ele — gardianul de UI din 40c, aici de FOND, pe orice cale de scriere).
    // Review-ul advers (F2): gazda se verifică pe AMBELE capete — o linie nu se
    // mută între documente (re-parentarea unei linii de pe un Operat pe un Draft
    // ar lăsa registrele fără liniile-sursă, iar verificarea doar a gazdei NOI
    // ar fi lăsat-o să treacă).
    static void VerificaLinie(IObjectSpace os, DocumentDetaliu linie, ICollection<string> erori) {
        if (!os.IsNewObject(linie) && !EsteSters(os, linie)) {
            var documentIdOriginal = Originale(os, linie)?[nameof(DocumentDetaliu.DocumentId)] as Guid?;
            if (documentIdOriginal is Guid gazdaVeche && gazdaVeche != Guid.Empty
                    && gazdaVeche != linie.DocumentId) {
                erori.Add("O linie nu se mută între documente — ștergeți-o și "
                    + "creați-o pe documentul țintă.");
                return;
            }
        }
        // FK-ul scalar nu e încă fixat pe liniile noi (se completează la
        // SaveChanges), deci navigația e sursa primară; documentul lipsă =
        // linie nouă pe un draft nou, care se validează pe cont propriu.
        var parinte = linie.Document
            ?? (linie.DocumentId != Guid.Empty ? os.GetObjectByKey<Document>(linie.DocumentId) : null);
        if (parinte == null)
            return;
        var stare = StareOriginala(os, parinte) ?? parinte.Stare;
        if (stare != StareDocument.Draft)
            erori.Add($"Liniile documentului {Eticheta(parinte)} nu se mai modifică "
                + $"(starea „{stare}”) — anulați operarea sau stornați-l.");
    }

    // Tipul documentului are rând `PoliticaNumerotare`? — exact criteriul
    // `NumarPoliticaController` (numărul e al seriei, nu al culegerii). Tipurile
    // fără ancoră `TipDocument` (n-ar trebui să existe pe căile vii) nu blochează.
    static bool AreNumerotare(IObjectSpace os, Document doc) {
        try {
            var tip = MotorOperare.GasesteTipDocument(os, doc);
            return os.GetObjectsQuery<PoliticaNumerotare>().Any(p => p.TipDocumentId == tip.ID);
        }
        catch (OperareException) {
            return false;
        }
    }

    // (c) Imperecherea: logica migrată din `ImperechereController.OnCommitting`
    // (decizia 31d/41d) — New validat prin invarianții serviciului, Edit refuzat
    // (re-validarea sumei ar cere excluderea propriului rând), Delete liber
    // (link fără registre proprii; gardianul de anulare/storno din motor există).
    static void VerificaImperechere(IObjectSpace os, Imperechere imperechere, ICollection<string> erori) {
        if (EsteSters(os, imperechere))
            return;
        if (!os.IsNewObject(imperechere)) {
            erori.Add("Imperecherea nu se editează — șterge-o și creeaz-o din nou.");
            return;
        }
        // Limitare asumată (ca gardianul de sold, decizia 25f): două link-uri
        // NOI în același commit nu se văd reciproc la Σ ≤ rest.
        try {
            ImperechereService.ValideazaCreare(os,
                imperechere.DocumentStingator, imperechere.Document, imperechere.Suma);
        }
        catch (OperareException ex) {
            erori.Add(ex.Message);
        }
    }

    // (d) Ciclul din `Cont.Parinte` (F13-D4, restanța 67e). Planul de conturi e
    // un ARBORE: `BalantaPliata` (67) cumulează brutele în sus pe `Parinte`, iar
    // `Cont.Simbol`/`Sumator` presupun aceeași ierarhie. Un ciclu (A → B → A,
    // sau A pe el însuși) transformă orice parcurgere într-o buclă infinită.
    // Garda de VIZITARE din `BalantaPliata` rămâne (apărare în adâncime pentru
    // datele deja intrate), dar ea OPREȘTE tăcut; aici ciclul se REFUZĂ la
    // intrare, cu lanțul în mesaj — „un gard care tace devine capcană" (62f).
    //
    // Se verifică la fiecare scriere a unui `Cont` cu părinte (nu doar când
    // `ParinteId` s-a schimbat): un ciclu se poate închide și mutând CELĂLALT
    // capăt, iar lanțul unui plan sintetic are 3–4 niveluri — costul e neglijabil
    // față de riscul de a rata cazul.
    static void VerificaCont(IObjectSpace os, Cont cont, ICollection<string> erori) {
        if (EsteSters(os, cont))
            return;
        var parinte = ParinteleLui(os, cont);
        if (parinte == null)
            return;
        var lant = new List<Cont> { cont };
        var vizitate = new HashSet<Guid> { cont.ID };
        for (var pas = 0; pas < LimitaAscendenti; pas++) {
            lant.Add(parinte);
            if (!vizitate.Add(parinte.ID)) {
                // Ciclul poate să nu treacă prin contul scris (X → A → B → A):
                // atunci nu e „propriul strămoș", dar e tot un ciclu — și tot
                // pe scrierea asta se vede prima oară.
                erori.Add(parinte.ID == cont.ID
                    ? $"Contul {EtichetaCont(cont)} nu poate fi propriul strămoș: "
                        + $"lanțul {Lant(lant)}."
                    : $"Contul {EtichetaCont(cont)} are un ciclu pe lanțul de părinți: {Lant(lant)}.");
                return;
            }
            parinte = ParinteleLui(os, parinte);
            if (parinte == null)
                return;
        }
        // Limita depășită fără repetiție = tot refuz: fie lanțul e mai adânc
        // decât are sens un plan de conturi, fie ciclul e mai lung decât ce
        // apucăm să vizităm. În ambele cazuri parcurgerile din raportare n-ar
        // mai fi de încredere.
        erori.Add($"Contul {EtichetaCont(cont)} are un lanț de părinți mai lung de "
            + $"{LimitaAscendenti} niveluri — planul de conturi nu poate fi atât de adânc: "
            + $"{Lant(lant)} → …");
    }

    static void VerificaPartener(IObjectSpace os, Partener partener, ICollection<string> erori) {
        if (EsteSters(os, partener))
            return;
        // Setterul a normalizat deja (trim/majuscule, gol ⇒ RO) — ce rămâne
        // greșit e chiar greșit: „ROM", „R0", „Germania".
        if (!FormatTaraIso2.IsMatch(partener.Tara ?? ""))
            erori.Add($"Partenerul {partener.Denumire ?? partener.Cod} are țara „{partener.Tara}” — "
                + "se scrie ca un cod ISO de două litere (RO, DE, US).");

        // (felia 15, D15-D1) Județul e al adreselor din România: `Region` din
        // SAF-T se validează contra listei ISO 3166-2:RO, deci un județ pe o
        // adresă străină e o valoare care nu poate fi scrisă în niciun fișier.
        // Regula XAF pereche nu există: FK-ul nu poate purta `RuleRequiredField`
        // și nici o regulă de coerență între două proprietăți (40b) — gardianul e
        // singura jumătate, și e pe ambele uși secured (Blazor + OData).
        //
        // Navigația e sursa primară, FK-ul plasa: pe un partener NOU scalarul se
        // completează abia la `SaveChanges` (același tipar ca `ParinteleLui`).
        var areJudet = partener.Judet != null
            || (partener.JudetId is Guid judetId && judetId != Guid.Empty);
        if (areJudet && !string.Equals(partener.Tara, "RO", StringComparison.Ordinal))
            erori.Add($"Partenerul {partener.Denumire ?? partener.Cod} are județ, dar țara "
                + $"„{partener.Tara}” — județul e al adreselor din România (SAF-T validează "
                + "`Region` contra listei ISO 3166-2:RO). Ștergeți județul sau puneți țara RO.");

        // Timbrul sincronizării ANAF e SERVER-OWNED (D15-D1, în siajul lui
        // `Autogenerat`): îl scrie doar serviciul, în OS-ul lui non-secured (58c).
        // Pe ușa secured e refuzat în AMBELE forme — schimbat pe un partener
        // existent și pre-completat pe unul nou (un partener născut prin OData cu
        // timbru își fabrică provenienața: ar arăta ca verificat la ANAF fără să
        // fi fost niciodată).
        //
        // `InactivFiscal` e în ACEEAȘI familie (review F2): e statutul din
        // registrul ANAF al contribuabililor inactivi — canonic pe axa TVA
        // (D15-D3), scris DOAR de serviciu. Cules de mână ar fi o părere despre
        // registru, nu registrul; pe nou = doar default-ul (false).
        if (os.IsNewObject(partener)) {
            if (partener.DataSincronizareAnaf != null)
                erori.Add("Data sincronizării ANAF o scrie doar serviciul de sincronizare — "
                    + "nu se culege pe un partener nou.");
            if (partener.InactivFiscal)
                erori.Add("Statutul „inactiv fiscal” îl scrie doar serviciul de sincronizare ANAF — "
                    + "nu se culege pe un partener nou.");
            return;
        }
        var originale = Originale(os, partener);
        if (originale == null)
            return;
        if (!Equals(originale[nameof(Partener.DataSincronizareAnaf)], partener.DataSincronizareAnaf))
            erori.Add($"Data sincronizării ANAF a partenerului {partener.Denumire ?? partener.Cod} "
                + "o scrie doar serviciul de sincronizare (comanda „Sincronizează din ANAF”).");
        if (!Equals(originale[nameof(Partener.InactivFiscal)], partener.InactivFiscal))
            erori.Add($"Statutul „inactiv fiscal” al partenerului {partener.Denumire ?? partener.Cod} "
                + "îl scrie doar serviciul de sincronizare (comanda „Sincronizează din ANAF”).");
    }

    // (f) Societatea raportoare (felia 16, D16-D1). Trei reguli, toate de FOND
    // (nu de ecran), deci toate aici: UI-ul XAF, OData și orice cale secured
    // viitoare le primesc pe aceeași ușă.
    static void VerificaSocietate(IObjectSpace os, Societate societate, ICollection<string> erori) {
        if (EsteSters(os, societate))
            return;

        // (1) UN SINGUR RÂND. „Societatea care raportează" e unică prin definiție:
        // un al doilea rând ar face `IdSocietate` ambiguu, iar fișierul ar ieși
        // semnat cu un CUI ales la întâmplare dintre două. Unicitatea NU se poate
        // exprima ca index (n-ai pe ce coloană s-o pui), deci gardianul e singura
        // ei formă — și de-aia trebuie să fie AICI, nu într-un controller de UI:
        // POST-ul al doilea prin OData trece pe lângă orice ecran.
        //
        // Se numără DOAR pe obiectele noi: un rând existent editat nu-și pune
        // singur problema. Interogarea vede rândurile deja COMISE (`GCRecord = 0`
        // prin filtrul global), iar `os.ModifiedObjects` dă restul commit-ului
        // curent — două rânduri noi în același commit se prind pe a doua ramură.
        if (os.IsNewObject(societate)) {
            var comise = os.GetObjectsQuery<Societate>().Count();
            var noiInainte = os.ModifiedObjects.OfType<Societate>()
                .TakeWhile(s => !ReferenceEquals(s, societate))
                .Count(s => os.IsNewObject(s) && !EsteSters(os, s));
            if (comise + noiInainte > 0)
                erori.Add("Există deja o societate raportoare — baza are UNA singură "
                    + "(o bază per client). Editați rândul existent în loc să creați altul.");
        }

        // (2) Județul e al adreselor din România, exact ca la `Partener`: schema
        // SAF-T validează `Region` contra listei ISO 3166-2:RO, iar aici valoarea
        // ajunge și în `AuditFileRegion` din `Header`.
        var areJudet = societate.Judet != null
            || (societate.JudetId is Guid judetId && judetId != Guid.Empty);
        if (areJudet && !string.Equals(societate.Tara, "RO", StringComparison.Ordinal))
            erori.Add($"Societatea are județ, dar țara „{societate.Tara}” — județul e al adreselor "
                + "din România (SAF-T validează `Region` contra listei ISO 3166-2:RO). "
                + "Ștergeți județul sau puneți țara RO.");

        // Setterul a normalizat deja țara (trim/majuscule, gol ⇒ RO); ce rămâne
        // greșit e chiar greșit.
        if (!FormatTaraIso2.IsMatch(societate.Tara ?? ""))
            erori.Add($"Societatea are țara „{societate.Tara}” — se scrie ca un cod ISO de două "
                + "litere (RO, DE, US).");

        // (3) `TaxAccountingBasis` din lista ANAF. Validatorul DUK verifică
        // `AccountID`-urile contra planului pe care îl declară valoarea asta, deci
        // o valoare inventată nu produce „un câmp greșit", ci un fișier respins
        // integral. Lista trăiește pe tip (`Societate.BazeContabile`), nu aici:
        // e a schemei, nu a gardianului.
        if (!Societate.BazeContabile.Contains(societate.BazaContabila ?? "", StringComparer.Ordinal))
            erori.Add($"Baza contabilă „{societate.BazaContabila}” nu e una dintre valorile admise de "
                + $"SAF-T (`TaxAccountingBasis`): {string.Join(", ", Societate.BazeContabile)}.");
    }

    // (i) 77-r2 — `Cod`/`Simbol` și `Denumire` obligatorii pe orice nomenclator
    // care declară `ICuCautare` (aceeași declarație care îi dă coloana de
    // căutare — regula stă pe interfață, nu pe o listă de tipuri). Jumătatea
    // de DOMENIU a regulii: schema o repetă (NOT NULL + CHECK, ușa de sistem),
    // dar pe ușa secured mesajul trebuie să fie al câmpului, nu al
    // constraint-ului. `IsNullOrWhiteSpace`, ca la `PoliticaMiscareSaft`: un
    // cod de „  ” nu e cod. Smoke-ul F20 a arătat gaura (partener fără
    // denumire acceptat pe toate cele trei uși).
    static void VerificaCodDenumire(ICuCautare rand, ICollection<string> erori) {
        // Proxy-ul de change-tracking al EF e un tip dinamic derivat
        // (`PartenerProxy`) — mesajul poartă numele CLASEI de domeniu.
        var tip = rand.GetType();
        if (tip.Assembly.IsDynamic && tip.BaseType != null)
            tip = tip.BaseType;
        var (cod, denumire) = Cautare.Citeste(rand);
        var numeCod = Cautare.NumeCod(tip);
        var eticheta = !string.IsNullOrWhiteSpace(denumire) ? denumire
            : !string.IsNullOrWhiteSpace(cod) ? cod : "(nou)";
        if (numeCod != null && string.IsNullOrWhiteSpace(cod))
            erori.Add($"„{numeCod}” este obligatoriu pe {tip.Name} {eticheta}.");
        if (string.IsNullOrWhiteSpace(denumire))
            erori.Add($"„{Cautare.NumeDenumire}” este obligatorie pe {tip.Name} {eticheta}.");
    }

    // (g) Produsul (felia 16, D16-D2): `CodNc` are exact 8 cifre, sau e gol.
    //
    // A DOUA jumătate a lui `[RuleRegularExpression]` de pe proprietate, exact ca
    // `Partener.Tara`: regula XAF apără culegerea din Blazor, dar NU rulează pe
    // API (55b) — iar `Produs` e CRUD pe OData (nomenclator viu, F2-D4), deci
    // fără gardul ăsta un PUT ar fi putut scrie „ABC" în `ProductCommodityCode`.
    // Gol/null trece: produsul fără NC iese în fișier cu `0` + avertisment
    // agregat, nu se refuză la culegere.
    static void VerificaProdus(Produs produs, ICollection<string> erori) {
        var cod = produs.CodNc;
        if (!string.IsNullOrWhiteSpace(cod) && !FormatCodNc.IsMatch(cod))
            erori.Add($"Produsul {produs.Denumire ?? produs.Cod} are codul NC „{cod}” — "
                + "codul NC are exact 8 cifre (sau rămâne gol).");
    }

    // (h) Politica de mișcare SAF-T (felia 17, D17-D1). Trei reguli de FOND, pe
    // ușa comună: politica e editabilă (`[NavigationItem("Politici")]`, fără
    // `ForbidCRUD`), deci ajunge aici și din XAF, și de pe orice cale secured
    // viitoare — iar validarea XAF nu rulează pe API (55b).
    //
    // Ce NU e aici: unicitatea tripletei. Aceea E exprimabilă ca index (spre
    // deosebire de `Societate`), și e exprimată — două indexuri parțiale în
    // `BackOfficeDbContext`, fiindcă `Semn` e nullabil și în Postgres
    // `NULL <> NULL`. Violarea lor iese ca mesaj de domeniu prin
    // `ConstraintViolationTranslator` (39a), nu ca 23505 brut.
    static void VerificaPoliticaMiscareSaft(
            IObjectSpace os, PoliticaMiscareSaft politica, ICollection<string> erori) {
        if (EsteSters(os, politica))
            return;
        var eticheta = politica.TipDocument?.Cod ?? politica.TipDocument?.Denumire ?? "(fără tip)";

        // (1) Codul e din nomenclatorul legii. O valoare din afara listei nu e o
        // „coloană greșită": validatorul ANAF respinge fișierul ÎNTREG pe ea, deci
        // greșeala trebuie prinsă la culegere, nu la depunere.
        // `IsNullOrWhiteSpace`, nu `IsNullOrEmpty`: un cod de „  ” ar fi trecut
        // pe lângă AMBELE reguli — nici cod valid (ramura de mai jos îl sărea),
        // nici excludere deliberată (cea de mai jos îi cerea motiv doar dacă era
        // gol). Adică exact gaura pe care gardianul există să n-o lase (62f).
        if (!string.IsNullOrWhiteSpace(politica.CodMiscare) && !SaftReguli.EsteCodMiscare(politica.CodMiscare))
            erori.Add($"Politica de mișcare SAF-T pe {eticheta}/{politica.TipStoc} are codul "
                + $"„{politica.CodMiscare}”, care nu e în nomenclatorul D406 "
                + $"({string.Join(", ", SaftReguli.CoduriMiscare.Keys)}) — "
                + "un cod inventat face fișierul respins integral.");

        // (2) Codul gol e o AFIRMAȚIE („registrul ăsta nu se raportează"), și o
        // afirmație are nevoie de motiv: rândurile ei ies în `Excluse` deliberate,
        // iar acolo se citește tocmai motivul. Fără el, excluderea ar fi
        // indistinguibilă de o politică uitată la jumătate.
        if (string.IsNullOrWhiteSpace(politica.CodMiscare) && string.IsNullOrWhiteSpace(politica.Motiv))
            erori.Add($"Politica de mișcare SAF-T pe {eticheta}/{politica.TipStoc} n-are cod de mișcare — "
                + "asta EXCLUDE deliberat rândurile din declarație, deci cere un motiv scris "
                + "(el apare lângă cifrele excluse).");

        // (3) Semnul e −1, +1 sau „orice" (null). Aceeași axă ca
        // `RegulaContare.SemnFiltru`, deci aceeași formă de gard: un `2` ar face
        // politica să nu se potrivească NICIODATĂ, tăcut.
        if (politica.Semn is not (null or -1 or 1))
            erori.Add($"Politica de mișcare SAF-T pe {eticheta}/{politica.TipStoc} are semnul "
                + $"{politica.Semn} — semnul e −1 (ieșire), +1 (intrare) sau gol (orice semn).");
    }

    // ═══ F23-D4 — PROVENIENȚA: gardianul o STINGE, seed-ul o aprinde ═══
    //
    // `DinSeed` e un câmp server-owned INVERSAT. Pe celelalte (Autogenerat,
    // DataSincronizareAnaf, Numar) gardianul REFUZĂ scrierea; aici o scrie el:
    // o politică atinsă de om nu mai e a seed-ului, iar asta nu e o greșeală de
    // corectat, e un fapt de înregistrat. De-aia editarea nu se refuză — se
    // timbrează.
    //
    // Pe obiect NOU e invers, și acolo chiar e refuz: un rând creat prin OData
    // cu `DinSeed = true` și-ar fabrica proveniența — ar arăta în raportul de
    // profil (F23-D8) ca fiind al seed-ului, deși nimeni nu l-a seed-uit
    // vreodată. Exact tiparul lui `Partener.DataSincronizareAnaf` (72a).
    //
    // Pe ușa de SISTEM (seed, motor, Import1C, ModelCheck) nu rulează nimic din
    // toate astea — gardianul nu e activ acolo (vezi antetul clasei).
    static void VerificaProvenienta(IObjectSpace os, ICuProvenienta rand, ICollection<string> erori) {
        if (os.IsNewObject(rand)) {
            if (rand.DinSeed)
                erori.Add($"Câmpul „Din seed” al rândului {EtichetaTip(rand)} îl scrie seed-ul — "
                    + "un rând creat de utilizator nu-și declară singur proveniența.");
            return;
        }
        // Scriere IDEMPOTENTĂ: pe un rând deja manual nu se atinge nimic, deci
        // un PATCH care nu schimbă nimic nu produce a doua modificare (F23-D11:
        // „gardianul nu poate scrie `DinSeed` fără dublu commit").
        if (rand.DinSeed)
            rand.DinSeed = false;
    }

    // ═══ F23-D5 — invarianții politicilor deschise pe OData ═══

    // `TipDocument` e ANCORA (decizia 20): oglindește clasele 1:1, iar `Cod` și
    // `ClrType` sunt identitatea prin care motorul își găsește tipul
    // (`GasesteTipDocument`) și prin care politicile îl referă. Un rând NOU n-are
    // clasă în spate, deci ar fi o ancoră spre nimic; o schimbare de `Cod` sau
    // `ClrType` ar rupe tăcut rezoluția de tip a întregului motor. Ce RĂMÂNE
    // editabil e exact ce e politică: `Denumire` și `TipTvaImplicit`.
    static void VerificaTipDocument(IObjectSpace os, TipDocument tip, ICollection<string> erori) {
        if (EsteSters(os, tip)) {
            erori.Add($"Tipul de document {tip.Cod ?? tip.Denumire} e ancora clasei de document "
                + "(decizia 20) — nu se șterge; clasele de document sunt cod, nu date.");
            return;
        }
        if (os.IsNewObject(tip)) {
            erori.Add("Tipurile de document nu se creează din date — ancora oglindește clasele "
                + "de document 1:1 (decizia 20), iar un rând fără clasă în spate n-ar avea ce "
                + "documenta. Editați rândul existent.");
            return;
        }
        var originale = Originale(os, tip);
        if (originale != null) {
            if (!string.Equals(originale[nameof(TipDocument.Cod)] as string, tip.Cod, StringComparison.Ordinal))
                erori.Add($"Codul tipului de document ({originale[nameof(TipDocument.Cod)]}) e identitatea "
                    + "lui — politicile îl referă prin el, iar schimbarea l-ar rupe tăcut.");
            if (!string.Equals(originale[nameof(TipDocument.ClrType)] as string, tip.ClrType, StringComparison.Ordinal))
                erori.Add($"Clasa CLR a tipului de document {tip.Cod} ({originale[nameof(TipDocument.ClrType)]}) "
                    + "e legătura cu codul — o scrie release-ul, nu culegerea.");
        }
        VerificaTipTvaActiv(os, tip.TipTvaImplicitId ?? tip.TipTvaImplicit?.ID,
            $"ancora tipului de document {tip.Cod}", erori);
    }

    // `TipTva`: cota e procent, iar `Activ = false` pe un tip REFERIT ca implicit
    // ar fi un gard care tace (62f) — rezolvarea implicitelor sare treapta cu un
    // motiv pe care nimeni nu-l citește, iar operatorul ar culege altceva decât
    // spune configurația. Refuzul vine cu LISTA referințelor: „scoate-l de aici,
    // apoi dezactivează-l" e o instrucțiune, „nu se poate" nu e.
    static void VerificaTipTva(IObjectSpace os, TipTva tip, ICollection<string> erori) {
        if (EsteSters(os, tip))
            return;
        if (tip.Cota is < 0m or > 100m)
            erori.Add($"Cota tipului de TVA {tip.Cod ?? tip.Denumire} e {tip.Cota} — cota e un procent "
                + "între 0 și 100.");
        if (tip.Activ)
            return;
        // Doar TRANZIȚIA spre inactiv se verifică: un tip care era deja inactiv
        // rămâne editabil (altfel n-ai mai putea nici să-i corectezi denumirea).
        var eraActiv = os.IsNewObject(tip)
            || (Originale(os, tip)?[nameof(TipTva.Activ)] as bool?) == true;
        if (!eraActiv)
            return;
        var referinte = ReferinteImplicite(os, tip.ID);
        if (referinte.Count > 0)
            erori.Add($"Tipul de TVA {tip.Cod ?? tip.Denumire} nu poate fi dezactivat cât timp e implicit "
                + $"în: {string.Join("; ", referinte)}. Un implicit care țintește un tip inactiv ar fi "
                + "sărit tăcut la culegere. Schimbați întâi referințele.");
    }

    // Cele patru locuri în care un `TipTva` poate fi IMPLICIT (F23-D1/D2).
    // Interogări pe FK, fără navigații (25b); etichetele sunt lizibile fiindcă
    // mesajul trebuie să spună UNDE, nu doar CÂT.
    static List<string> ReferinteImplicite(IObjectSpace os, Guid tipTvaId) {
        var referinte = new List<string>();
        referinte.AddRange(os.GetObjectsQuery<TipDocument>()
            .Where(t => t.TipTvaImplicitId == tipTvaId).Select(t => t.Cod).ToList()
            .Select(c => $"ancora tipului de document {c}"));
        referinte.AddRange(os.GetObjectsQuery<PoliticaTvaImplicit>()
            .Where(p => p.TipTvaId == tipTvaId)
            .Select(p => new { p.TipDocument.Cod, p.ClasaFiscala }).ToList()
            .Select(p => $"politica {p.Cod} × {p.ClasaFiscala?.ToString() ?? "orice clasă"}"));
        referinte.AddRange(os.GetObjectsQuery<Partener>()
            .Where(p => p.TipTvaImplicitId == tipTvaId).Select(p => p.Cod).Take(10).ToList()
            .Select(c => $"partenerul {c}"));
        referinte.AddRange(os.GetObjectsQuery<Produs>()
            .Where(p => p.TipTvaImplicitId == tipTvaId).Select(p => p.Cod).Take(10).ToList()
            .Select(c => $"produsul {c}"));
        return referinte;
    }

    // Un implicit nu poate ținti un tip inactiv — regula geamănă a celei de mai
    // sus, pe cealaltă direcție (acolo se dezactivează ținta, aici se scrie
    // referința). Fără ea, gardul ar fi ocolibil în doi pași.
    static void VerificaTipTvaActiv(IObjectSpace os, Guid? tipTvaId, string unde, ICollection<string> erori) {
        if (tipTvaId is not Guid id || id == Guid.Empty)
            return;
        var tip = os.GetObjectsQuery<TipTva>().Where(t => t.ID == id)
            .Select(t => new { t.Cod, t.Activ }).FirstOrDefault();
        if (tip != null && !tip.Activ)
            erori.Add($"Tipul de TVA „{tip.Cod}” e inactiv și nu poate fi implicit ({unde}) — "
                + "rezolvarea l-ar sări tăcut la culegere.");
    }

    // `PoliticaTvaImplicit` (F23-D2): ținta activă + MESAJUL unicității.
    // Indexul o apără oricum (`NULLS NOT DISTINCT`, F23-D3), dar un `23505` brut
    // nu spune ce cheie s-a repetat; gardianul vorbește înaintea bazei, iar
    // constraint-ul rămâne plasa (60a).
    static void VerificaPoliticaTvaImplicit(
            IObjectSpace os, PoliticaTvaImplicit politica, ICollection<string> erori) {
        if (EsteSters(os, politica))
            return;
        var tipDocId = politica.TipDocument?.ID ?? politica.TipDocumentId;
        var eticheta = politica.TipDocument?.Cod ?? "(fără tip)";
        VerificaTipTvaActiv(os, politica.TipTvaId != Guid.Empty ? politica.TipTvaId : politica.TipTva?.ID,
            $"implicitul {eticheta} × {politica.ClasaFiscala?.ToString() ?? "orice clasă"}", erori);
        if (tipDocId == Guid.Empty)
            return;
        var clasa = politica.ClasaFiscala;
        var valabil = politica.ValabilDeLa;
        var duplicat = os.GetObjectsQuery<PoliticaTvaImplicit>()
            .Where(p => p.TipDocumentId == tipDocId)
            .Select(p => new { p.ID, p.ClasaFiscala, p.ValabilDeLa }).ToList()
            .Any(p => p.ID != politica.ID && p.ClasaFiscala == clasa && p.ValabilDeLa == valabil);
        if (duplicat)
            erori.Add($"Există deja un implicit de TVA pe {eticheta} × "
                + $"{politica.ClasaFiscala?.ToString() ?? "orice clasă"}"
                + (politica.ValabilDeLa == null ? " (dintotdeauna)" : $" de la {politica.ValabilDeLa:dd.MM.yyyy}")
                + " — două rânduri pe aceeași cheie ar face rezolvarea nedeterministă. "
                + "Editați rândul existent sau dați-i o dată de valabilitate diferită.");
    }

    // `PoliticaTva`: sursa Explicit fără cont e o contrapartidă care nu se poate
    // rezolva niciodată — rândul de TVA n-ar avea unde să cadă la operare.
    static void VerificaPoliticaTva(PoliticaTva politica, ICollection<string> erori) {
        if (politica.SursaContrapartida == SursaCont.Explicit
                && politica.ContrapartidaFallbackId == null && politica.ContrapartidaFallback == null)
            erori.Add($"Politica de TVA pe {politica.TipDocument?.Cod ?? "(fără tip)"} are sursa "
                + "contrapartidei „Explicit”, dar niciun cont — sursa explicită E contul.");
    }

    // `RegulaContare`: trei invarianți, toți despre potrivirea din motor.
    static void VerificaRegulaContare(RegulaContare regula, ICollection<string> erori) {
        var eticheta = regula.TipDocument?.Cod ?? "(fără tip)";
        if (regula.SursaContDebit == SursaCont.Explicit
                && regula.ContDebitId == null && regula.ContDebit == null)
            erori.Add($"Regula de contare pe {eticheta} are sursa contului debitor „Explicit”, "
                + "dar niciun cont — sursa explicită E contul.");
        if (regula.SursaContCredit == SursaCont.Explicit
                && regula.ContCreditId == null && regula.ContCredit == null)
            erori.Add($"Regula de contare pe {eticheta} are sursa contului creditor „Explicit”, "
                + "dar niciun cont — sursa explicită E contul.");
        // Aceeași formă ca `PoliticaMiscareSaft.Semn`: un `2` ar face regula să
        // nu se potrivească NICIODATĂ, tăcut.
        if (regula.SemnFiltru is not (null or -1 or 1))
            erori.Add($"Regula de contare pe {eticheta} are filtrul de semn {regula.SemnFiltru} — "
                + "semnul e −1, +1 sau gol (orice semn).");
        // Cele două trepte de potrivire sunt ALTERNATIVE (26c): TipMaterial exact
        // bate `NaturaFiltru`, care bate regula generică. O regulă cu amândouă
        // n-ar fi „mai specifică", ar fi doar de neînțeles — iar motorul o
        // potrivește oricum pe treapta exactă, ignorând natura.
        var areTipMaterial = regula.TipMaterialId != null || regula.TipMaterial != null;
        if (areTipMaterial && regula.NaturaFiltru != null)
            erori.Add($"Regula de contare pe {eticheta} are și tip de material, și filtru de natură — "
                + "cele două sunt trepte ALTERNATIVE de potrivire (tipul exact bate natura, care bate "
                + "regula generică). Păstrați una.");
    }

    // `RegulaStoc`: semnul E direcția mișcării; nu există „orice semn" aici (spre
    // deosebire de filtre), fiindcă regula SCRIE rândul, nu îl caută.
    static void VerificaRegulaStoc(RegulaStoc regula, ICollection<string> erori) {
        if (regula.Semn is not (-1 or 1))
            erori.Add($"Regula de stoc pe {regula.TipDocument?.Cod ?? "(fără tip)"} are semnul "
                + $"{regula.Semn} — semnul e −1 (ieșire) sau +1 (intrare).");
    }

    // `PoliticaNumerotare`: seria compune numărul server-owned (53b), iar un
    // contor sub 1 ar produce numere de document zero sau negative.
    //
    // DEVIERE DECLARATĂ de la F23-D5, pe măsurătoare: contractul cerea și
    // „`Format` nevid". `Format` e însă OPȚIONAL în motor — `MotorOperare:814`
    // citește `IsNullOrWhiteSpace(politica.Format)` ca „serie + număr" —, iar
    // `ContaSeeder.SeedNumerotare` nu-l scrie NICIODATĂ. Regula din contract ar
    // fi refuzat orice editare a oricărei politici de numerotare pe orice bază
    // seed-uită, adică gardianul ar fi respins rândurile propriului seed
    // (măsurat: toate cele 14 rânduri private și 9 bugetare).
    //
    // Ce rămâne, și e regula pe care contractul o voia de fapt: un `Format`
    // CULES trebuie să fie utilizabil. `string.Format` aruncă la OPERARE pe un
    // șablon cu indice inexistent („{2}") sau cu acoladă neînchisă — adică un
    // document care nu se mai poate opera, cu o excepție de framework în loc de
    // un mesaj. Se probează aici, pe valoarea culeasă.
    static void VerificaPoliticaNumerotare(PoliticaNumerotare politica, ICollection<string> erori) {
        var eticheta = politica.TipDocument?.Cod ?? "(fără tip)";
        if (string.IsNullOrWhiteSpace(politica.Serie))
            erori.Add($"Politica de numerotare pe {eticheta} n-are serie — numărul documentului se "
                + "compune din ea.");
        if (politica.UrmatorulNumar < 1)
            erori.Add($"Politica de numerotare pe {eticheta} are următorul număr {politica.UrmatorulNumar} "
                + "— numerotarea începe de la 1.");
        if (string.IsNullOrWhiteSpace(politica.Format))
            return;
        try {
            // Aceiași doi parametri ca la operare (`MotorOperare.AsignaNumar`):
            // {0} = numărul, {1} = seria.
            string.Format(politica.Format, 1, politica.Serie ?? "");
        }
        catch (FormatException) {
            erori.Add($"Politica de numerotare pe {eticheta} are formatul „{politica.Format}”, care nu se "
                + "poate compune — șablonul acceptă {0} (numărul) și {1} (seria). Un format greșit ar "
                + "opri operarea documentului, nu culegerea lui.");
        }
    }

    static void VerificaPoliticaScadenta(PoliticaScadenta politica, ICollection<string> erori) {
        if (politica.ZileDefault < 0)
            erori.Add($"Politica de scadență pe {politica.TipDocument?.Cod ?? "(fără tip)"} are "
                + $"{politica.ZileDefault} zile — scadența implicită nu poate fi în trecut față de document.");
    }

    // `PoliticaInchidereTva`: cele patru conturi sunt un SET. Serviciul cere
    // setul complet ca să genereze ceva (46c), deci o politică pe jumătate
    // culeasă nu e „în lucru", e un tip inert care arată configurat — exact
    // ambiguitatea pe care ecranul ITV o traduce azi în „ProfilInert" (79a).
    static void VerificaPoliticaInchidereTva(PoliticaInchidereTva politica, ICollection<string> erori) {
        var conturi = new[] {
            politica.ContDeductibilaId ?? politica.ContDeductibila?.ID,
            politica.ContColectataId ?? politica.ContColectata?.ID,
            politica.ContDePlataId ?? politica.ContDePlata?.ID,
            politica.ContDeRecuperatId ?? politica.ContDeRecuperat?.ID,
        };
        var completate = conturi.Count(c => c != null && c != Guid.Empty);
        if (completate is > 0 and < 4)
            erori.Add($"Politica de închidere TVA pe {politica.TipDocument?.Cod ?? "(fără tip)"} are "
                + $"{completate} din 4 conturi — închiderea cere setul COMPLET (deductibilă, colectată, "
                + "de plată, de recuperat). Completați-le pe toate sau goliți-le pe toate (tip inert).");
    }

    // `MapareD300`: aceleași două reguli ca atributele XAF de pe clasă, chemate
    // prin funcțiile lor statice — o regulă, două uși (F23-D5).
    static void VerificaMapareD300(IObjectSpace os, MapareD300 mapare, ICollection<string> erori) {
        if (EsteSters(os, mapare))
            return;
        var rand = mapare.Rand
            ?? (mapare.RandId != Guid.Empty ? os.GetObjectByKey<RandD300>(mapare.RandId) : null);
        if (!MapareD300.EsteDeOperatiuni(rand))
            erori.Add($"Maparea D300 țintește rd. {rand.Cod}, de fel „{rand.Fel}” — rândurile de total, "
                + "oglindă și extern se calculează, nu se alimentează din mapări.");
        if (!MapareD300.FaraAscendentMapat(os, mapare))
            erori.Add("Aceeași pereche (tip de TVA × sens) țintește deja un rând aflat pe aceeași "
                + "verticală „din care” cu cel ales — cifra ar intra de două ori în rândul-părinte și "
                + "în totalul lui. Păstrați o singură mapare pe verticală.");
    }

    static void VerificaMapareD394(MapareD394 mapare, ICollection<string> erori) {
        if (!MapareD394.TintaPermisa(mapare.Tip, mapare.Sens))
            erori.Add($"Maparea D394 țintește {mapare.Tip} pe {mapare.Sens} — pe livrare se mapează doar "
                + "L, V, LS, pe achiziție doar A, C, AS. AÎ se derivă din partener (TVA la încasare), "
                + "iar N n-are sursă în registrul de TVA.");
    }

    // Numele CLASEI de domeniu al unui rând, nu al proxy-ului de change tracking
    // (`RegulaContareProxy`) — aceeași dezproxare ca la `VerificaCodDenumire`.
    static string EtichetaTip(object rand) {
        var tip = rand.GetType();
        if (tip.Assembly.IsDynamic && tip.BaseType != null)
            tip = tip.BaseType;
        return tip.Name;
    }

    static readonly System.Text.RegularExpressions.Regex FormatCodNc =
        new(@"^\d{8}$", System.Text.RegularExpressions.RegexOptions.Compiled);

    static readonly System.Text.RegularExpressions.Regex FormatTaraIso2 =
        new("^[A-Z]{2}$", System.Text.RegularExpressions.RegexOptions.Compiled);

    // Adâncimea maximă a unui plan sintetic e de ordinul 4 (clasă → grupă → cont
    // sintetic gr. I → gr. II); 64 e „imposibil de atins legitim", nu o limită de
    // produs.
    const int LimitaAscendenti = 64;

    // FK-ul scalar nu e încă fixat pe obiectele NOI (se completează la
    // SaveChanges), exact ca la liniile de document mai sus: navigația e sursa
    // primară, FK-ul e plasa pentru obiectele deja materializate (părintele poate
    // fi tot nou, în ACELAȘI commit — `GetObjectByKey` îl întoarce din OS).
    static Cont ParinteleLui(IObjectSpace os, Cont cont) =>
        cont.Parinte
            ?? (cont.ParinteId is Guid id && id != Guid.Empty ? os.GetObjectByKey<Cont>(id) : null);

    static string Lant(IEnumerable<Cont> conturi) =>
        string.Join(" → ", conturi.Select(EtichetaCont));

    static string EtichetaCont(Cont cont) =>
        string.IsNullOrWhiteSpace(cont.Simbol) ? $"({cont.ID})" : cont.Simbol;

    // „Obiectul ăsta e pe cale să fie ȘTERS?" — răspuns valabil ÎN Committing.
    //
    // De ce NU `os.IsDeletedObject` singur (probă pe surse 26.1.3, găsită de
    // smoke-ul feliei de trezorerie): `EFCoreObjectSpace.IsDeletedObject`
    // (EFCoreObjectSpace.cs:375-386) întoarce true DOAR pentru `Detached` sau
    // pentru un tip cu ștergere amânată al cărui `GCRecord` e deja 1. Or
    // `GCRecord` îl pune `EFCoreDeferredDeletionInterceptor` în `SavingChanges`
    // (DeferredDeletion/EFCoreDeferredDeletionInterceptor.cs:95-120), adică DUPĂ
    // evenimentul `Committing` — deci în gardian entitatea e încă `Deleted` cu
    // `GCRecord` 0 și `IsDeletedObject` răspunde FALS. Consecința reală: o
    // ștergere de imperechere era raportată ca „editare" și refuzată (31d cere
    // ștergerea liberă), pe ORICE cale secured — UI-ul XAF și `api/imperecheri`.
    // Starea EF e sursa corectă aici; `IsDeletedObject` rămâne în paralel pentru
    // ștergerile deja materializate. Ambele prin API-ul PUBLIC `IObjectSpace`
    // (review F3-D1a): `IsObjectToDelete` = `GetEntityState(obj) == Deleted`
    // (EFCoreObjectSpace.cs:371-374), fără cast la tipul concret — corect și pe
    // providerii non-EF.
    static bool EsteSters(IObjectSpace os, object obj) =>
        os.IsObjectToDelete(obj) || os.IsDeletedObject(obj);

    // Starea de dinaintea modificării, din evidența EF (OriginalValues) — o
    // scriere pe `Stare` nu-și poate ascunde propria urmă.
    static StareDocument? StareOriginala(IObjectSpace os, Document doc) =>
        Originale(os, doc)?[nameof(Document.Stare)] as StareDocument?;

    // Valorile ORIGINALE ale entității, din evidența EF. `SecuredEFCoreObjectSpace`
    // derivă din `EFCoreObjectSpace` (DevExpress.EntityFrameworkCore.Security\
    // Security\SecuredEFCoreObjectSpace.cs:57), deci `DbContext` e disponibil pe
    // ambele. Null = nu se poate determina (alt provider / Detached / Added) —
    // apelantul cade pe valorile curente.
    static Microsoft.EntityFrameworkCore.ChangeTracking.PropertyValues Originale(
            IObjectSpace os, object obj) {
        if (os is not EFCoreObjectSpace efCore)
            return null;
        var entry = efCore.DbContext.Entry(obj);
        if (entry.State is EntityState.Detached or EntityState.Added)
            return null;
        return entry.OriginalValues;
    }

    static string Eticheta(Document doc) =>
        string.IsNullOrWhiteSpace(doc.Numar) ? $"({doc.Data:dd.MM.yyyy})" : doc.Numar;
}

// Înregistrarea seam-ului. Se apelează din `ConfigureServices` al FIECĂRUI host
// (Blazor.Server + WebApi) — modulele XAF nu pot înregistra servicii în DI.
// Ordinea față de `AddXaf`/`AddXafWebApi` e indiferentă: serviciul e rezolvat
// lazy, la prima creare de ObjectSpace
// (`ObjectSpaceCustomizerService.cs:53-54`, `serviceProvider.GetServices<...>()`).
public static class GardianStartupExtensions {
    public static IServiceCollection AddContaGardianEditare(this IServiceCollection services) {
        // Scoped + TryAddEnumerable = exact forma în care XAF își înregistrează
        // proprii customizeri (`ObjectSpaceCustomizerStartupExtensions.cs:65`).
        //
        // Constructorul cere `ISecurityStrategyBase` cu valoare implicită `null`
        // (F22-D3). Nu e nevoie de fabrică: containerul Microsoft rezolvă
        // parametrul din scope când tipul e înregistrat (ambele host-uri îl
        // înregistrează scoped — controllerele WebApi îl injectează deja) și
        // cade pe valoarea implicită când nu e. Scope-ul gardianului e același
        // cu al strategiei, deci strategia primită e a CERERII curente.
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IObjectSpaceCustomizer, GardianEditare>());
        return services;
    }
}
