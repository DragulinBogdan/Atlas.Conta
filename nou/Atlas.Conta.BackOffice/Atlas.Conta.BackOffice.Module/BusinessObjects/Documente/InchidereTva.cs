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
// de formă DescarcareService 37b), nu se culege. De aceea clasa n-are câmpuri
// proprii și niciun override: operarea/anularea/storno-ul = motorul standard.
//
// Ce NU face (design §6): închiderea NU închide perioada fiscală (GardianPerioada
// rămâne mecanism separat) și NU atinge 121 (închiderea de exercițiu e în afara
// scope-ului). Acțiunea UI de generare rămâne aditivă, la nevoie — felia 1C-d
// folosește consola (Import1C), ca precedentele Migrare/ModelCheck.
//
// [TipDetaliu] se re-declară: atributul e Inherited=false (UI/TipDetaliuAttribute).
[TipDetaliu(typeof(NotaContabilaDetaliu))]
public class InchidereTva : NotaContabila {
}
