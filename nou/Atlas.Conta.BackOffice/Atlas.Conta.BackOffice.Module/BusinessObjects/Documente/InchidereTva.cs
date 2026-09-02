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
// scope-ului). Generarea are trei apelanți pe același serviciu: consola
// Import1C (1C-d), ruta `POST api/itv/genereaza` (felia 21) și acțiunea XAF
// „Generează închiderea" (`Controllers/InchidereTvaGenerareController.cs`,
// 79-r1) — toți prin `InchidereTvaApply`/`InchidereTvaService`, niciunul cu
// aritmetică proprie.
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
        var linii = new Motor.InchidereTvaService.LiniiInchidere(
            Suma(politica.ContColectataId, politica.ContDeductibilaId),
            Suma(politica.ContColectataId, politica.ContDePlataId),
            Suma(politica.ContDeRecuperatId, politica.ContDeductibilaId));
        // Criteriul e UNUL singur, partajat cu `Stale` din ReadDto (79 M4).
        if (!Motor.InchidereTvaService.LiniiPotrivescSoldurile(linii, sold4426, sold4427))
            erori.Add($"Soldurile de TVA s-au schimbat de la generare (4426: {sold4426}, 4427: {sold4427} " +
                $"față de liniile documentului) — ștergeți draftul și regenerați închiderea.");

        // Cronologia la OPERARE (review 79 F1): o închidere OPERATĂ pentru o lună
        // ulterioară a închis deja, cumulat, și soldurile acestei luni — a opera
        // acum draftul de față le-ar închide a doua oară (4423/4424 dublate, iar
        // reziduul negativ pe 4426/4427 ar fi mascat de `Max(0, …)` din `Solduri`).
        // Gardianul de la generare (`Analizeaza`, 2b/2c) oprește sensul celălalt;
        // acesta e perechea lui, pe ușa prin care trece ORICE operare (XAF, API,
        // consolă). Un draft ulterior NU blochează: el va deveni stale și îl
        // refuză gardianul de mai sus.
        var ulterioara = os.GetObjectsQuery<InchidereTva>()
            .Where(d => d.Data > Data && d.Stare == StareDocument.Operat)
            .OrderBy(d => d.Data)
            .Select(d => new { d.Numar, d.Data })
            .FirstOrDefault();
        if (ulterioara != null)
            erori.Add($"Există o închidere de TVA operată pentru o lună ulterioară ({ulterioara.Numar}, "
                + $"{ulterioara.Data:dd.MM.yyyy}) — a opera acum închiderea lunii {Data.Month:00}/{Data.Year} ar închide "
                + "aceleași solduri a doua oară. Stornați-o pe cea ulterioară sau ștergeți acest draft.");
    }
}
