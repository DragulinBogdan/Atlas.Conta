using Atlas.Conta.BackOffice.Module.UI;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// ITV (design FAZA 1C §6): închiderea lunară de TVA — al 13-lea derivat, care
// iese din amânarea 36f și devine forcing function-ul TVA-ului structural din P1
// (reconcilierea 4423/4424 pe an fiscal complet, felia 1C-d).
//
// ITV **E** o notă contabilă (moștenire concretă, nu tip paralel): linii cu
// postare explicită (ILinieCuPostareExplicita), laturi interne, fără reguli de
// stoc/contare, aceleași validări. Diferența e doar de PROVENIENȚĂ — draftul se
// GENEREAZĂ din soldurile registrului (`InchidereTvaService.Genereaza`, precedentul
// de formă DescarcareService 37b), nu se culege. Operarea/anularea/storno-ul =
// motorul standard, plus gardianul anti-stale de mai jos (review advers 1C-a).
//
// Ce NU face (design §6): închiderea NU închide perioada fiscală (GardianPerioada
// rămâne mecanism separat) și NU atinge 121 (închiderea de exercițiu e în afara
// scope-ului). Acțiunea UI de generare rămâne aditivă, la nevoie — felia 1C-d
// folosește consola (Import1C), ca precedentele Migrare/ModelCheck.
//
// [TipDetaliu] se re-declară: atributul e Inherited=false (UI/TipDetaliuAttribute).
[TipDetaliu(typeof(NotaContabilaDetaliu))]
public class InchidereTva : NotaContabila {
    // Gardianul anti-stale (review advers 1C-a, defect 4): draftul poartă
    // soldurile de la GENERARE; dacă între generare și operare au mai intrat
    // documente de TVA în lună, operarea ar posta valori vechi — luna nu s-ar
    // închide la 0, iar guard-ul „închidere vie" ar bloca regenerarea. Se
    // recalculează soldurile la data documentului și se cere potrivirea EXACTĂ
    // cu liniile (transfer + de plată = 4427; transfer + de recuperat = 4426);
    // precedentul de plasă: gardianul de sold care refuză alocarea învechită a
    // DSC-ului. Nepotrivire = anulați/ștergeți draftul și regenerați.
    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        var tipItv = Motor.MotorOperare.GasesteTipDocument(os, this);
        var politica = os.FirstOrDefault<PoliticaInchidereTva>(p => p.TipDocumentId == tipItv.ID);
        if (politica?.ContDeductibilaId == null || politica.ContColectataId == null
                || politica.ContDePlataId == null || politica.ContDeRecuperatId == null) {
            erori.Add("Închiderea de TVA cere politica de conturi (PoliticaInchidereTva) completă.");
            return;
        }
        var (sold4426, sold4427) = Motor.InchidereTvaService.Solduri(
            os, politica.ContDeductibilaId.Value, politica.ContColectataId.Value, Data);
        decimal Suma(Guid? debit, Guid? credit) => Detalii.OfType<NotaContabilaDetaliu>()
            .Where(d => d.ContDebitId == debit && d.ContCreditId == credit)
            .Sum(d => d.Valoare);
        var transfer = Suma(politica.ContColectataId, politica.ContDeductibilaId);
        var dePlata = Suma(politica.ContColectataId, politica.ContDePlataId);
        var deRecuperat = Suma(politica.ContDeRecuperatId, politica.ContDeductibilaId);
        if (transfer + dePlata != sold4427 || transfer + deRecuperat != sold4426)
            erori.Add($"Soldurile de TVA s-au schimbat de la generare (4426: {sold4426}, 4427: {sold4427} " +
                $"față de liniile documentului) — ștergeți draftul și regenerați închiderea.");
    }
}
