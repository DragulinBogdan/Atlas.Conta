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
                    if (!registruRaportat) {
                        registruRaportat = true;
                        erori.Add("Registrele (stoc/contabil) se scriu doar de motor, la operare — "
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
            }
        }
        if (erori.Count > 0)
            throw new OperareException(string.Join("\n", erori.Distinct()));
    }

    // (a) Documentul: se culege cât e Draft. Starea e SERVER-OWNED — tranzițiile
    // le face doar motorul, în ObjectSpace-ul lui non-secured.
    static void VerificaDocument(IObjectSpace os, Document doc, ICollection<string> erori) {
        if (os.IsNewObject(doc)) {
            if (doc.Stare != StareDocument.Draft)
                erori.Add($"Un document nou se creează în starea Draft, nu „{doc.Stare}” "
                    + "— operarea îi schimbă starea.");
            return;
        }
        var originala = StareOriginala(os, doc) ?? doc.Stare;
        if (originala != StareDocument.Draft) {
            erori.Add($"Documentul {Eticheta(doc)} nu mai e Draft (starea „{originala}”) — "
                + "nu se mai modifică și nu se șterge. Anulați operarea sau stornați-l.");
            return;
        }
        if (!os.IsDeletedObject(doc) && doc.Stare != originala)
            erori.Add($"Starea documentului {Eticheta(doc)} o schimbă doar motorul "
                + "(Operează / Anulează operarea / Stornează).");
    }

    // (a) Liniile urmează starea documentului-gazdă (registrele s-au scris din
    // ele — gardianul de UI din 40c, aici de FOND, pe orice cale de scriere).
    static void VerificaLinie(IObjectSpace os, DocumentDetaliu linie, ICollection<string> erori) {
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

    // (c) Imperecherea: logica migrată din `ImperechereController.OnCommitting`
    // (decizia 31d/41d) — New validat prin invarianții serviciului, Edit refuzat
    // (re-validarea sumei ar cere excluderea propriului rând), Delete liber
    // (link fără registre proprii; gardianul de anulare/storno din motor există).
    static void VerificaImperechere(IObjectSpace os, Imperechere imperechere, ICollection<string> erori) {
        if (os.IsDeletedObject(imperechere))
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

    // Starea de dinaintea modificării, din evidența EF (OriginalValues) — o
    // scriere pe `Stare` nu-și poate ascunde propria urmă. `SecuredEFCoreObjectSpace`
    // derivă din `EFCoreObjectSpace` (DevExpress.EntityFrameworkCore.Security\
    // Security\SecuredEFCoreObjectSpace.cs:57), deci `DbContext` e disponibil pe
    // ambele. Null = nu se poate determina (alt provider) — apelantul cade pe
    // valoarea curentă.
    static StareDocument? StareOriginala(IObjectSpace os, Document doc) {
        if (os is not EFCoreObjectSpace efCore)
            return null;
        var entry = efCore.DbContext.Entry(doc);
        if (entry.State is EntityState.Detached or EntityState.Added)
            return null;
        return entry.OriginalValues[nameof(Document.Stare)] as StareDocument?;
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
