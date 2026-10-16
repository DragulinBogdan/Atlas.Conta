using Atlas.Conta.BackOffice.Module.Api.Perioade;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;

namespace Atlas.Conta.BackOffice.Module.Controllers;

// Comenzile exercițiului (E3-D7) pe decembrie, cu aceleași dialoguri și același gate ca luna.
public partial class PerioadaFiscalaController {
    PopupWindowShowAction inchideExercitiul;
    PopupWindowShowAction redeschideExercitiul;

    void CreeazaExercitiul() {
        inchideExercitiul = new PopupWindowShowAction(this, "PerioadaFiscala.InchideExercitiul", PredefinedCategory.RecordEdit) {
            Caption = "Închide exercițiul",
            ToolTip = "Verifică exercițiul pe decembrie închis și îl închide.",
            AcceptButtonCaption = "Închide exercițiul",
            CancelButtonCaption = "Renunță",
        };
        inchideExercitiul.CustomizePopupWindowParams += (_, e) => {
            var decembrie = (PerioadaFiscala)View.CurrentObject;
            e.View = DialogInchidere(decembrie, os => PerioadeApply.VerificaExercitiul(os, decembrie.An),
                $"Închide exercițiul {decembrie.An}");
        };
        inchideExercitiul.Execute += InchideExercitiul_Execute;

        redeschideExercitiul = new PopupWindowShowAction(this, "PerioadaFiscala.RedeschideExercitiul", PredefinedCategory.RecordEdit) {
            Caption = "Redeschide exercițiul",
            ToolTip = "Redeschide ultimul exercițiu închis, cu motiv scris.",
            AcceptButtonCaption = "Redeschide exercițiul",
            CancelButtonCaption = "Renunță",
        };
        redeschideExercitiul.CustomizePopupWindowParams += (_, e) => {
            var os = Application.CreateObjectSpace(typeof(RedeschiderePerioadaParametri));
            e.View = Application.CreateDetailView(os, os.CreateObject<RedeschiderePerioadaParametri>());
            e.View.Caption = $"Redeschide exercițiul {((PerioadaFiscala)View.CurrentObject).An}";
        };
        redeschideExercitiul.Execute += RedeschideExercitiul_Execute;
    }

    void ActualizeazaExercitiul(PerioadaFiscala perioada) {
        inchideExercitiul.Enabled[CheieStare] = perioada is { Luna: 12, Inchisa: true, ExercitiuInchis: false };
        redeschideExercitiul.Enabled[CheieStare] = perioada is { Luna: 12, ExercitiuInchis: true };
    }

    void InchideExercitiul_Execute(object sender, PopupWindowShowActionExecuteEventArgs e) {
        var parametri = (InchiderePerioadaParametri)e.PopupWindowViewCurrentObject;
        var decembrie = (PerioadaFiscala)View.CurrentObject;
        var an = decembrie.An;
        var cerere = new InchidePerioadaRequestDto {
            Acceptate = parametri.Constatari.Where(c => c.Acceptata).Select(c => c.Cheie).ToArray(),
        };
        var rezultat = Comanda(decembrie,
            os => PerioadeApply.InchideExercitiul(os, an, cerere, UserId(), Application.Security?.UserName));
        Informeaza($"Exercițiul {an} a fost închis la {rezultat.La.ToLocalTime():dd.MM.yyyy HH:mm}"
            + (rezultat.Acceptari.Length == 0 ? "." : $", cu {rezultat.Acceptari.Length} constatări acceptate."));
    }

    void RedeschideExercitiul_Execute(object sender, PopupWindowShowActionExecuteEventArgs e) {
        var parametri = (RedeschiderePerioadaParametri)e.PopupWindowViewCurrentObject;
        var decembrie = (PerioadaFiscala)View.CurrentObject;
        var an = decembrie.An;
        var cerere = new RedeschidePerioadaRequestDto { Motiv = parametri.Motiv };
        var rezultat = Comanda(decembrie,
            os => PerioadeApply.RedeschideExercitiul(os, an, cerere, UserId(), Application.Security?.UserName));
        Informeaza($"Exercițiul {an} a fost redeschis la {rezultat.La.ToLocalTime():dd.MM.yyyy HH:mm}.");
    }
}
