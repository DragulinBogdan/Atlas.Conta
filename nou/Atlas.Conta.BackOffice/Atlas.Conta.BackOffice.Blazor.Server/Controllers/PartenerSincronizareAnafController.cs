using Atlas.Conta.BackOffice.Module.Anaf;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Conta.BackOffice.Blazor.Server.Controllers;

// CALEA UMANĂ a feliei 15 (D15-D5): „Sincronizează din ANAF" pe `Partener`, în
// DetailView și în ListView cu selecție multiplă.
//
// DE CE stă în Blazor.Server și nu în Module (unde sunt toate celelalte
// controllere): acțiunea are nevoie de un `HttpClient` din DI, iar Module nu
// înregistrează servicii în niciun host — cablarea (`AddHttpClient`, URL-ul din
// `Anaf:PlatitorTvaUrl`) e a host-ului. Clientul React n-are ecran de partener
// (→ `lista-react.md`), deci asta rămâne singura cale de om până la pasul 3.
//
// DE CE fără `suprascrie`: riscul 2 al contractului — un lot cu suprascriere
// rescrie `Denumire` peste eticheta contabilului și e ireversibil. Butonul
// uman face DOAR umplerea golurilor și raportarea diferențelor; suprascrierea
// deliberată rămâne a comenzii REST (D15-D4), unde e un parametru explicit.
public class PartenerSincronizareAnafController : ObjectViewController<ObjectView, Partener> {
    // Plafonul din D15-D4: 500 de parteneri ≈ 5 loturi ≈ 5 s de așteptare
    // impusă de ANAF. Peste el, treaba e a Import1C/CLI (D15-D6), nu a unui
    // buton care ține circuitul blocat.
    public const int MaximSelectie = 500;

    readonly SimpleAction sincronizeaza;

    public PartenerSincronizareAnafController() {
        sincronizeaza = new SimpleAction(this, "Partener.SincronizeazaAnaf", PredefinedCategory.RecordEdit) {
            Caption = "Sincronizează din ANAF",
            ToolTip = "Completează din registrul ANAF `PlatitorTva` datele lipsă (adresă, nr. reg. com.) "
                + "și actualizează statutul de TVA. Valorile culese diferite se RAPORTEAZĂ, nu se rescriu.",
            // Enabled la una sau mai multe rânduri selectate: același buton
            // servește fișa deschisă și selecția din listă.
            SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects,
            ConfirmationMessage = "Interogați registrul ANAF pentru partenerii selectați?",
        };
        sincronizeaza.Execute += Sincronizeaza_Execute;
    }

    void Sincronizeaza_Execute(object sender, SimpleActionExecuteEventArgs e) {
        var selectati = e.SelectedObjects?.OfType<Partener>().ToList() ?? [];
        if (selectati.Count == 0)
            throw new UserFriendlyException("Selectați cel puțin un partener.");
        if (selectati.Count > MaximSelectie)
            throw new UserFriendlyException($"Selecția are {selectati.Count} parteneri, plafonul e "
                + $"{MaximSelectie} (ANAF acceptă 100 de coduri pe secundă). Pentru nomenclatorul întreg "
                + "se folosește conectorul Import1C.");

        // Gate de autorizare ÎNAINTEA ușii non-secured (spike F1, tiparul
        // `DocumentOperareController`/`FacturaIesireDescarcareController`): ușa
        // de mai jos e a SERVICIULUI, nu a utilizatorului. Ca la lotul REST
        // (D15-D4), refuzul e PER PARTENER — un rând fără drept iese cu motiv,
        // nu aruncă tot lotul.
        var cerinte = Application.Security as IRequestSecurityStrategy;
        var permise = new List<Partener>();
        var refuzate = new List<Sarit>();
        foreach (var p in selectati) {
            if (cerinte != null && IsGrantedExtensions.CanWrite(cerinte, ObjectSpace, (object)p))
                permise.Add(p);
            else
                refuzate.Add(new Sarit(p.ID, p.Denumire ?? p.Cod, "fără drept de scriere pe partener"));
        }
        if (permise.Count == 0)
            throw new UserFriendlyException("Nu aveți dreptul de scriere pe niciunul dintre partenerii selectați.");
        var ids = permise.Select(p => p.ID).ToList();

        var fabrica = Application.ServiceProvider.GetRequiredService<INonSecuredObjectSpaceFactory>();
        var client = Application.ServiceProvider.GetRequiredService<PlatitorTvaClient>();

        // RULARE SINCRONĂ, asumat: XAF Blazor e sincron (motivul migrării pe
        // React — `ObjectSpace` n-are async nativ), iar acțiunea trebuie să
        // întoarcă rezultatul în același `Execute`. `Task.Run` scoate lanțul
        // async de pe SynchronizationContext-ul rendererului ÎNAINTE de blocare:
        // altfel `GetAwaiter().GetResult()` pe firul circuitului ar aștepta o
        // continuare care așteaptă chiar firul acela. (Serviciul folosește și
        // `ConfigureAwait(false)` peste tot — a doua plasă, nu prima.)
        // ObjectSpace-ul se naște ÎNĂUNTRU: DbContext-ul rămâne al unui singur
        // fir, cel care îl folosește. Costul e ~1 s per 100 de parteneri, impus
        // de ANAF, cu butonul blocat — acceptabil pentru o comandă manuală.
        RezultatLot rezultat;
        try {
            rezultat = Task.Run(async () => {
                using var os = fabrica.CreateNonSecuredObjectSpace(typeof(Partener));
                return await SincronizareAnafService.SincronizeazaAsync(
                    os, client, ids, suprascrie: false, CancellationToken.None);
            }).GetAwaiter().GetResult();
        }
        catch (UserFriendlyException) {
            throw;
        }
        catch (Exception ex) {
            throw new UserFriendlyException($"Sincronizarea ANAF a eșuat: {ex.Message}", ex);
        }

        // Comanda a comis în ALT DbContext — View-ul e stale (statutul de TVA,
        // adresa, timbrul). Același `Refresh()` ca după motor.
        ObjectSpace.Refresh();

        Application.ShowViewStrategy.ShowMessage(new MessageOptions {
            Message = Rezuma(rezultat, refuzate),
            Type = rezultat.Erori.Count > 0 ? InformationType.Warning : InformationType.Success,
            Duration = 20000,
        });
    }

    // Câți parteneri primesc detaliu nominal înainte ca mesajul să devină un
    // zid de text. Peste prag rămân cifrele — lista integrală e treaba
    // rezultatului REST (D15-D4), care întoarce totul ca date.
    const int MaximDetalii = 20;

    static string Rezuma(RezultatLot rezultat, IReadOnlyList<Sarit> refuzate) {
        var gasiti = rezultat.Rezultate.Count(r => r.Gasit);
        var negasiti = rezultat.Rezultate.Count(r => !r.Gasit);
        var modificari = rezultat.Rezultate.Sum(r => r.Modificari.Count);
        var diferente = rezultat.Rezultate.Sum(r => r.Diferente.Count);
        var sarite = rezultat.Sarite.Count + refuzate.Count;
        var linii = new List<string> {
            $"ANAF: {gasiti} găsiți, {negasiti} negăsiți, {sarite} săriți — "
            + $"{modificari} modificări, {diferente} diferențe raportate.",
        };

        // Doar partenerii care au ceva de spus: cel actualizat fără schimbări nu
        // merită un rând (timbrul lui e vizibil pe fișă).
        var interesanti = rezultat.Rezultate
            .Where(r => r.Modificari.Count > 0 || r.Diferente.Count > 0 || r.Avertismente.Count > 0)
            .ToList();
        foreach (var r in interesanti.Take(MaximDetalii)) {
            var parti = new List<string>();
            foreach (var m in r.Modificari)
                parti.Add($"{m.Camp}: „{m.Vechi ?? ""}” → „{m.Nou}”");
            foreach (var d in r.Diferente)
                parti.Add($"{d.Camp}: cules „{d.Cules}” ≠ ANAF „{d.Anaf}”");
            parti.AddRange(r.Avertismente);
            linii.Add($"• {r.Eticheta} ({r.Cui}): {string.Join("; ", parti)}");
        }
        if (interesanti.Count > MaximDetalii)
            linii.Add($"• … și încă {interesanti.Count - MaximDetalii} parteneri cu modificări/diferențe.");

        foreach (var s in refuzate.Concat(rezultat.Sarite).Take(MaximDetalii))
            linii.Add($"• sărit — {s.Eticheta ?? s.Id.ToString()}: {s.Motiv}");
        if (sarite > MaximDetalii)
            linii.Add($"• … și încă {sarite - MaximDetalii} săriți.");

        foreach (var er in rezultat.Erori)
            linii.Add($"• eroare ANAF ({(er.Tranzitorie ? "tranzitorie, se poate relua" : "fatală")}) "
                + $"pe {er.Lot.Count} coduri: {er.Mesaj}");

        return string.Join("\n", linii);
    }
}
