using System.ComponentModel;
using System.Runtime.CompilerServices;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Core;
using DevExpress.ExpressApp.EFCore;
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

    static void OnCommitting(object sender, CancelEventArgs e) {
        if (sender is IObjectSpace os)
            Verifica(os);
    }

    // Regulile, în ordinea din contract (D4). Toate lipsurile se cumulează și se
    // raportează împreună, ca în motor (mesajele se despart pe „\n").
    public static void Verifica(IObjectSpace os) {
        var erori = new List<string>();
        var registruRaportat = false;
        foreach (var obj in os.ModifiedObjects) {
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
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IObjectSpaceCustomizer, GardianEditare>());
        return services;
    }
}
