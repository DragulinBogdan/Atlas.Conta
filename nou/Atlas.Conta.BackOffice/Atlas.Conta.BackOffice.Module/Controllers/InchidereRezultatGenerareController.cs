using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Irz;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Conta.BackOffice.Module.Controllers;

// „Generează închiderea” conturilor de rezultat (E3-D3), în forma lui `InchidereTvaGenerareController`
// (79-r1): gate de creare pe tip pe OS-ul securizat, apoi `InchidereRezultatApply` pe ușa motorului.
public class InchidereRezultatGenerareController : ObjectViewController<ListView, InchidereRezultat> {
    const int AnMinim = 2000, AnMaxim = 2100;

    public InchidereRezultatGenerareController() {
        var genereaza = new PopupWindowShowAction(this, "InchidereRezultat.Genereaza", PredefinedCategory.RecordEdit) {
            Caption = "Generează închiderea",
            ToolTip = "Generează draftul închiderii conturilor de rezultat pentru luna aleasă, din soldurile de la sfârșitul ei.",
            SelectionDependencyType = SelectionDependencyType.Independent,
            AcceptButtonCaption = "Generează",
            CancelButtonCaption = "Renunță",
        };
        genereaza.CustomizePopupWindowParams += Genereaza_CustomizePopupWindowParams;
        genereaza.Execute += Genereaza_Execute;
    }

    void Genereaza_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e) {
        var os = Application.CreateObjectSpace(typeof(GenerareInchidereLunaParametri));
        if (os is NonPersistentObjectSpace npos && npos.Owner is not CompositeObjectSpace) {
            npos.PopulateAdditionalObjectSpaces(Application);
            npos.AutoDisposeAdditionalObjectSpaces = true;
        }
        var parametri = os.CreateObject<GenerareInchidereLunaParametri>();
        var ultima = ObjectSpace.GetObjectsQuery<InchidereRezultat>()
            .Where(d => d.Stare != StareDocument.Stornat)
            .OrderByDescending(d => d.Data)
            .Select(d => (DateOnly?)d.Data)
            .FirstOrDefault();
        var implicita = ultima is DateOnly d ? d.AddMonths(1) : DateOnly.FromDateTime(DateTime.Today).AddMonths(-1);
        parametri.An = implicita.Year;
        parametri.Luna = implicita.Month;
        var unitati = os.GetObjectsQuery<UnitateInterna>().Take(2).ToList();
        if (unitati.Count == 1)
            parametri.Unitate = unitati[0];
        e.View = Application.CreateDetailView(os, parametri);
        e.View.Caption = "Generează închiderea conturilor de rezultat";
    }

    void Genereaza_Execute(object sender, PopupWindowShowActionExecuteEventArgs e) {
        var parametri = (GenerareInchidereLunaParametri)e.PopupWindowViewCurrentObject;
        var erori = new List<string>();
        if (parametri.An < AnMinim || parametri.An > AnMaxim)
            erori.Add($"„An” trebuie să fie între {AnMinim} și {AnMaxim}.");
        if (parametri.Luna < 1 || parametri.Luna > 12)
            erori.Add("„Luna” trebuie să fie între 1 și 12 — închiderea conturilor de rezultat se face pe o lună.");
        if (parametri.Unitate == null)
            erori.Add("Alegeți unitatea internă care închide luna.");
        if (erori.Count > 0)
            throw new UserFriendlyException(string.Join("\n", erori));

        if (Application.Security is not IRequestSecurityStrategy cerinte
                || !cerinte.CanCreate(typeof(InchidereRezultat), ObjectSpace)
                || !cerinte.CanWrite(typeof(InchidereRezultat), ObjectSpace))
            throw new UserFriendlyException(Refuzuri.FaraDrept(OperatieAcces.Creare, typeof(InchidereRezultat)));

        var cerere = new GenerareIrzRequestDto { An = parametri.An, Luna = parametri.Luna, UnitateId = parametri.Unitate.ID };
        GenerareIrzRezultatDto rezultat;
        var fabrica = Application.ServiceProvider.GetRequiredService<INonSecuredObjectSpaceFactory>();
        using (var osMotor = fabrica.CreateNonSecuredObjectSpace(typeof(InchidereRezultat))) {
            try {
                rezultat = InchidereRezultatApply.Genereaza(osMotor, cerere);
            }
            catch (OperareException ex) {
                throw new UserFriendlyException(ex.Message);
            }
        }

        if (rezultat.DocumentId is not Guid documentId) {
            Informeaza(Rezuma(cerere, rezultat), InformationType.Info);
            return;
        }
        ObjectSpace.Refresh();
        var osView = Application.CreateObjectSpace(typeof(InchidereRezultat));
        var draft = RandDupaCheie.Ca<InchidereRezultat>(osView, documentId);
        if (draft != null) {
            e.ShowViewParameters.CreatedView = Application.CreateDetailView(osView, draft);
            e.ShowViewParameters.TargetWindow = TargetWindow.NewWindow;
        }
        Informeaza($"Închiderea conturilor de rezultat {cerere.Luna:00}/{cerere.An} generată ca draft, "
            + $"cu {rezultat.NumarLinii} linii.", InformationType.Success);
    }

    string Rezuma(GenerareIrzRequestDto cerere, GenerareIrzRezultatDto rezultat) {
        var motiv = Enum.TryParse<MotivNegenerare>(rezultat.Motiv, out var m)
            ? CaptionHelper.GetDisplayText(m)
            : rezultat.Motiv;
        var text = $"Nu s-a generat nimic pentru {cerere.Luna:00}/{cerere.An}: {motiv}.";
        if (rezultat.InchidereVieId is Guid blocantId
                && ObjectSpace.GetObjectsQuery<InchidereRezultat>().Where(d => d.ID == blocantId)
                    .Select(d => new { d.Numar, d.Data, d.Stare }).FirstOrDefault() is { } blocant)
            text += $" Închiderea existentă: {(string.IsNullOrWhiteSpace(blocant.Numar) ? blocant.Data.ToString("dd.MM.yyyy") : blocant.Numar)} ({blocant.Stare}).";
        return text;
    }

    void Informeaza(string mesaj, InformationType tip) =>
        Application.ShowViewStrategy.ShowMessage(new MessageOptions { Message = mesaj, Type = tip, Duration = 8000 });
}
