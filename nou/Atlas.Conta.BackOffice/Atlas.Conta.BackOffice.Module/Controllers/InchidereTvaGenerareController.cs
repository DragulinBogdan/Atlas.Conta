using System.ComponentModel;
using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Itv;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Conta.BackOffice.Module.Controllers;

// Restanța 79-r1: „Generează închiderea" de TVA din XAF Blazor. Până aici un
// contabil putea genera doar din clientul React (felia 21) sau din consola
// Import1C — pe Blazor documentul ITV se vedea, se opera, se storna, dar nu se
// putea NAȘTE (comentariul „acțiunea UI rămâne aditivă" din `InchidereTva.cs`).
//
// Forma e cea a comenzii de pe API, nu una proprie XAF-ului (44/53: Blazor e
// vehiculul de iterație, nu un al doilea produs):
//
//   * PARAMETRII (an, lună, unitate internă) se culeg într-un dialog pe un
//     obiect NON-PERSISTENT (`GenerareInchidereTvaParametri`) — exact cererea
//     `GenerareItvRequestDto`, cu lookup-ul de unitate în loc de Guid. Un
//     `ParametrizedAction` cu o singură valoare (precedentul „Generează
//     descărcarea") n-ar fi putut culege și unitatea; două widget-uri în toolbar
//     ar fi fost o formă pe care n-o are niciun alt ecran.
//   * GATE-UL e cel de pe `POST api/itv/genereaza` (F21-D3, 80b): pe TIP —
//     comanda n-are subiect, îl PRODUCE — și cere Create ȘI Write (plasa
//     DevExpress le cere pe amândouă la salvarea unui obiect nou). Se ia pe
//     ObjectSpace-ul SECURIZAT al View-ului, ÎNAINTE de ușa non-secured; fraza
//     refuzului e cea din Module (`Refuzuri.FaraDrept`), aceeași ca pe REST.
//   * COMANDA e `InchidereTvaApply.Genereaza`, pe ușa NON-SECURED a motorului
//     (58c: serviciul scrie câmpuri server-owned și își însumează soldurile
//     peste registru — pe ușa filtrată cifra ar fi FALSĂ, nu goală, 73g). Nu
//     există o a doua cale de generare: ModelCheck exersează același Apply.
//   * REZULTATUL urmează contractul 79d: „nu s-a generat" cu MOTIV e un raport
//     adevărat (toast informativ, nimic scris), refuzul de domeniu e
//     `UserFriendlyException` (422-ul de pe API), iar draftul născut se
//     deschide în editare — fluxul conexului din `DocumentOperareController`.
//
// Ce NU face (deliberat): nu previzualizează în dialog (comanda raportează
// motivul cu cifrele lui la un click, fără să scrie nimic când nu poate) și nu
// regenerează (pe Blazor calea e ștergerea draftului + generarea din nou — un
// buton propriu ar fi al doilea apelant al lui `Regenereaza` fără cerere).
public class InchidereTvaGenerareController : ObjectViewController<ListView, InchidereTva> {
    // Aceleași margini ca pe API (`ItvController.AnMinim/AnMaxim`): un an
    // absurd e o cerere malformată, nu un refuz de domeniu — serviciul ar fi
    // aruncat din `new DateOnly(…)`.
    const int AnMinim = 2000, AnMaxim = 2100;

    readonly PopupWindowShowAction genereaza;

    public InchidereTvaGenerareController() {
        genereaza = new PopupWindowShowAction(this, "InchidereTva.Genereaza", PredefinedCategory.RecordEdit) {
            Caption = "Generează închiderea",
            ToolTip = "Generează draftul închiderii de TVA pentru luna aleasă, din soldurile registrului.",
            // Comanda n-are subiect (produce documentul): butonul e activ și pe
            // lista goală, ca `New`.
            SelectionDependencyType = SelectionDependencyType.Independent,
            AcceptButtonCaption = "Generează",
            CancelButtonCaption = "Renunță",
        };
        genereaza.CustomizePopupWindowParams += Genereaza_CustomizePopupWindowParams;
        genereaza.Execute += Genereaza_Execute;
    }

    void Genereaza_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e) {
        // Obiectul de parametri trăiește într-un `NonPersistentObjectSpace`;
        // lookup-ul de unitate are nevoie de un ObjectSpace PERSISTENT dedesubt
        // (`CompositeObjectSpace.AdditionalObjectSpaces`) — altfel „The
        // UnitateInterna type is not registered" la deschiderea dialogului.
        // Popularea e LOCALĂ dialogului, nu un hook global pe
        // `ObjectSpaceCreated`: e singurul obiect non-persistent al aplicației,
        // iar un hook global ar fi schimbat comportamentul tuturor OS-urilor
        // non-persistente ale modulelor DevExpress (rapoarte, dashboard-uri).
        // OS-ul suplimentar vine din `Application.CreateObjectSpace`, deci e
        // SECURIZAT: lista de unități e cea pe care operatorul are voie s-o vadă.
        var os = Application.CreateObjectSpace(typeof(GenerareInchidereTvaParametri));
        if (os is NonPersistentObjectSpace npos && npos.Owner is not CompositeObjectSpace) {
            npos.PopulateAdditionalObjectSpaces(Application);
            npos.AutoDisposeAdditionalObjectSpaces = true;
        }

        var parametri = os.CreateObject<GenerareInchidereTvaParametri>();
        // Default-ul lunii: luna care URMEAZĂ ultimei închideri vii (Draft/Operat)
        // — închiderile se generează cronologic (46c), deci asta e singura lună
        // pe care comanda o poate accepta după ea; fără nicio închidere, luna
        // trecută (închiderea se face după ce luna s-a terminat). Citită pe OS-ul
        // securizat al listei: un rând invizibil operatorului nu-i dictează luna.
        var ultima = ObjectSpace.GetObjectsQuery<InchidereTva>()
            .Where(d => d.Stare != StareDocument.Stornat)
            .OrderByDescending(d => d.Data)
            .Select(d => (DateOnly?)d.Data)
            .FirstOrDefault();
        var implicita = ultima is DateOnly d ? d.AddMonths(1) : DateOnly.FromDateTime(DateTime.Today).AddMonths(-1);
        parametri.An = implicita.Year;
        parametri.Luna = implicita.Month;
        // Precompletarea unității DOAR la exact un rând (F21-D4: „un default care
        // nu minte — cu două unități nu alege"). Prin OS-ul dialogului, ca
        // referința să aparțină spațiului compus în care trăiește lookup-ul.
        var unitati = os.GetObjectsQuery<UnitateInterna>().Take(2).ToList();
        if (unitati.Count == 1)
            parametri.Unitate = unitati[0];

        e.View = Application.CreateDetailView(os, parametri);
        e.View.Caption = "Generează închiderea de TVA";
    }

    void Genereaza_Execute(object sender, PopupWindowShowActionExecuteEventArgs e) {
        var parametri = (GenerareInchidereTvaParametri)e.PopupWindowViewCurrentObject;

        // Validarea cererii, ca 400-ul de pe API (toate erorile deodată — cine a
        // greșit luna vrea să afle tot ce e greșit). Explicită, nu prin reguli
        // de validare pe obiectul non-persistent: dialogul e al acțiunii, nu al
        // modulului de validare, iar fraza e aceeași ca pe sârmă.
        var erori = new List<string>();
        if (parametri.An < AnMinim || parametri.An > AnMaxim)
            erori.Add($"„An” trebuie să fie între {AnMinim} și {AnMaxim}.");
        if (parametri.Luna < 1 || parametri.Luna > 12)
            erori.Add("„Luna” trebuie să fie între 1 și 12 — închiderea de TVA se face pe o lună.");
        if (parametri.Unitate == null)
            erori.Add("Alegeți unitatea internă care închide luna.");
        if (erori.Count > 0)
            throw new UserFriendlyException(string.Join("\n", erori));

        // Gate-ul de CREARE pe TIP, pe OS-ul securizat, ÎNAINTEA ușii non-secured
        // (F21-D3 + 80b: Create ȘI Write — `ContaApiController.PoateCrea`).
        if (Application.Security is not IRequestSecurityStrategy cerinte
                || !cerinte.CanCreate(typeof(InchidereTva), ObjectSpace)
                || !cerinte.CanWrite(typeof(InchidereTva), ObjectSpace))
            throw new UserFriendlyException(Refuzuri.FaraDrept(OperatieAcces.Creare, typeof(InchidereTva)));

        var cerere = new GenerareItvRequestDto {
            An = parametri.An, Luna = parametri.Luna, UnitateId = parametri.Unitate.ID,
        };

        // Comanda, pe ușa NON-SECURED a motorului; `Apply`-ul comite singur,
        // condiționat pe existența draftului. Refuzul de domeniu (cronologia,
        // `TRZ` lipsă, unitatea ne-internă) e 422-ul de pe API — aici
        // `UserFriendlyException`, ca la toate comenzile XAF ale motorului.
        GenerareItvRezultatDto rezultat;
        var fabrica = Application.ServiceProvider.GetRequiredService<INonSecuredObjectSpaceFactory>();
        using (var osMotor = fabrica.CreateNonSecuredObjectSpace(typeof(InchidereTva))) {
            try {
                rezultat = InchidereTvaApply.Genereaza(osMotor, cerere);
            }
            catch (OperareException ex) {
                throw new UserFriendlyException(ex.Message);
            }
        }

        if (rezultat.DocumentId is not Guid documentId) {
            // Raport, nu eroare (79d): luna e deja închisă / n-are sold / profilul
            // e inert. Motivul iese cu eticheta lui din model (`[XafDisplayName]`
            // pe `MotivNegenerare` — aceeași sursă ca eticheta din React, 57f),
            // iar soldurile însoțesc verdictul unde există (pe profil inert sunt
            // `null`, nu 0 — acolo nu există conturi de arătat).
            Informeaza(Rezuma(cerere, rezultat), InformationType.Info);
            return;
        }

        // Draftul s-a născut în ALT DbContext: lista e stale, iar documentul se
        // deschide în editare (fluxul conexului, `DocumentOperareController`) —
        // operatorul îl verifică și îl operează separat. Prin ID, pe un OS de
        // View propriu: puntea între ObjectSpace-uri e ID-ul, nu instanța.
        // `TargetWindow.NewWindow`, nu `Default`/`Current` (probate în browser,
        // explicate pe sursa `BlazorMdiShowViewStrategy` 26.1.3): aplicația e MDI
        // cu tab-uri, iar `Default` cât timp dialogul e încă deschis devine
        // `NewModalWindow` (draftul apărea într-un al doilea popup, cu Save/Cancel
        // și fără comenzile motorului); `Current` pe frame-ul listei înlocuia
        // view-ul în tab-ul listei fără ca toolbar-ul tab-ului să se reconstruiască
        // (fără Operează/Stornează). `NewWindow` = `TryAddNewWindow` = tab nou cu
        // controllerele lui — exact ce face click-ul pe un rând al listei.
        ObjectSpace.Refresh();
        var osView = Application.CreateObjectSpace(typeof(InchidereTva));
        var draft = osView.GetObjectByKey<InchidereTva>(documentId);
        if (draft != null) {
            e.ShowViewParameters.CreatedView = Application.CreateDetailView(osView, draft);
            e.ShowViewParameters.TargetWindow = TargetWindow.NewWindow;
        }
        Informeaza(
            $"Închiderea de TVA {cerere.Luna:00}/{cerere.An} generată ca draft: "
            + $"transfer {rezultat.Transfer:N2}, de plată {rezultat.DePlata:N2}, de recuperat {rezultat.DeRecuperat:N2}.",
            InformationType.Success);
    }

    // Rezumatul unui „nu s-a generat": motivul, cifrele, documentul blocant.
    string Rezuma(GenerareItvRequestDto cerere, GenerareItvRezultatDto rezultat) {
        var motiv = Enum.TryParse<MotivNegenerare>(rezultat.Motiv, out var m)
            ? CaptionHelper.GetDisplayText(m)
            : rezultat.Motiv;
        var parti = new List<string> {
            $"Nu s-a generat nimic pentru {cerere.Luna:00}/{cerere.An}: {motiv}."
        };
        if (rezultat.Sold4426 is decimal s4426 && rezultat.Sold4427 is decimal s4427)
            parti.Add($"Solduri la sfârșitul lunii: deductibilă {s4426:N2}, colectată {s4427:N2}.");
        if (rezultat.InchidereVieId is Guid blocantId) {
            // Eticheta documentului blocant, ca în `PrevizualizareItvDto`: numărul
            // dacă e materializat (53b), altfel data.
            var blocant = ObjectSpace.GetObjectsQuery<InchidereTva>()
                .Where(d => d.ID == blocantId)
                .Select(d => new { d.Numar, d.Data, d.Stare })
                .FirstOrDefault();
            if (blocant != null)
                parti.Add($"Închiderea blocantă: {(string.IsNullOrWhiteSpace(blocant.Numar) ? blocant.Data.ToString("dd.MM.yyyy") : blocant.Numar)} ({blocant.Stare}).");
        }
        return string.Join(" ", parti);
    }

    void Informeaza(string mesaj, InformationType tip) =>
        Application.ShowViewStrategy.ShowMessage(new MessageOptions {
            Message = mesaj,
            Type = tip,
            Duration = 8000,
        });
}

// Parametrii dialogului de generare — cererea `GenerareItvRequestDto`, cu
// unitatea ca LOOKUP (referință la obiectul persistent, prin spațiul compus al
// dialogului). Non-persistent (`NonPersistentBaseObject`): trăiește cât
// dialogul, nu are tabel, nu intră în `metadata.json` (dump-ul ia doar spațiul
// `BusinessObjects`) și nu e o clasă de domeniu — de aceea stă lângă controller.
// `SetPropertyValue`, nu auto-proprietăți: altfel `INotifyPropertyChanged` tace
// și editorii dialogului nu văd precompletarea.
[DomainComponent]
[XafDisplayName("Generează închiderea de TVA")]
public class GenerareInchidereTvaParametri : NonPersistentBaseObject {
    int an;
    int luna;
    UnitateInterna unitate;

    // Fără separator de mii: editorul numeric implicit ar arăta „2,026”.
    [XafDisplayName("An")]
    [ModelDefault("EditMask", "d")]
    [ModelDefault("DisplayFormat", "{0:0}")]
    public int An {
        get => an;
        set => SetPropertyValue(ref an, value);
    }

    [XafDisplayName("Luna")]
    [ModelDefault("EditMask", "d")]
    [ModelDefault("DisplayFormat", "{0:0}")]
    public int Luna {
        get => luna;
        set => SetPropertyValue(ref luna, value);
    }

    [XafDisplayName("Unitatea internă")]
    public UnitateInterna Unitate {
        get => unitate;
        set => SetPropertyValue(ref unitate, value);
    }
}
