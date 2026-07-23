using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.DXF.Core.Views;
using Atlas.DXF.Core.Views.Discovery;

namespace Atlas.Conta.BackOffice.Module.UI;

// Baseline de coloane (EntityFluent) pentru cele 5 ListView-uri de detaliu
// tipizate comutate de TipDetaliuViewUpdater. Conținut: ordinea logică de culegere
// (Index) + ascunderea FK-urilor brute (zgomot). Sunt DEFAULT-uri — diff-urile
// utilizatorului din Model Editor rămân prioritare (SetIfEmpty/SetIfDefault).
// Coloanele de TVA se ascund view-scoped (Index = -1) pe tipurile fără semantică
// de TVA (LDI/DSC) — membrul rămâne vizibil pe tipurile care îl folosesc.
public sealed class ContaUiBaseline : IUiBaselineProvider {
    // Sufixul id-ului ListView-ului implicit generat de XAF per clasă.
    const string ListView = "_ListView";

    public void Register(UiBaselineRegistry registry) {
        FacturaIntrare(registry);
        FacturaIesire(registry);
        ListaDiferenteInventar(registry);
        Decont(registry);
        DescarcareGestiune(registry);
    }

    static void FacturaIntrare(UiBaselineRegistry registry) {
        var entitate = registry.For<FacturaIntrareDetaliu>();
        entitate.HideMembers(d => d.TipMaterialId, d => d.LotId, d => d.TipTvaId, d => d.AngajamentId);
        entitate.ListView(nameof(FacturaIntrareDetaliu) + ListView, _ => { })
            .Column(d => d.TipMaterial, c => c.Index = 0)
            .Column(d => d.Lot, c => c.Index = 1)
            .Column(d => d.Cantitate, c => c.Index = 2)
            .Column(d => d.PretUnitar, c => c.Index = 3)
            .Column(d => d.TipTva, c => c.Index = 4)
            .Column(d => d.ValoareTva, c => c.Index = 5)
            .Column(d => d.Valoare, c => c.Index = 6)
            .Column(d => d.DataExpirare, c => c.Index = 7)
            .Column(d => d.LotFabricatie, c => c.Index = 8)
            .Column(d => d.CodCpv, c => c.Index = 9);
    }

    static void FacturaIesire(UiBaselineRegistry registry) {
        var entitate = registry.For<FacturaIesireDetaliu>();
        entitate.HideMembers(d => d.ProdusId, d => d.TipMaterialId, d => d.LotId, d => d.TipTvaId, d => d.AngajamentId);
        entitate.ListView(nameof(FacturaIesireDetaliu) + ListView, _ => { })
            .Column(d => d.Produs, c => c.Index = 0)
            .Column(d => d.Lot, c => c.Index = 1)
            .Column(d => d.TipMaterial, c => c.Index = 2)
            .Column(d => d.Cantitate, c => c.Index = 3)
            .Column(d => d.PretUnitar, c => c.Index = 4)
            .Column(d => d.TipTva, c => c.Index = 5)
            .Column(d => d.ValoareTva, c => c.Index = 6)
            .Column(d => d.Valoare, c => c.Index = 7)
            .Column(d => d.Descriere, c => c.Index = 8);
    }

    static void ListaDiferenteInventar(UiBaselineRegistry registry) {
        var entitate = registry.For<ListaDiferenteInventarDetaliu>();
        entitate.HideMembers(d => d.TipMaterialId, d => d.LotId, d => d.TipTvaId, d => d.AngajamentId);
        entitate.ListView(nameof(ListaDiferenteInventarDetaliu) + ListView, _ => { })
            .Column(d => d.Directie, c => c.Index = 0)
            .Column(d => d.TipMaterial, c => c.Index = 1)
            .Column(d => d.Lot, c => c.Index = 2)
            .Column(d => d.Cantitate, c => c.Index = 3)
            .Column(d => d.PretEvaluare, c => c.Index = 4)
            .Column(d => d.Valoare, c => c.Index = 5)
            .Column(d => d.DataExpirare, c => c.Index = 6)
            .Column(d => d.LotFabricatie, c => c.Index = 7)
            // LDI n-are semantică de TVA — ascunde coloanele moștenite din bază.
            .Column(d => d.TipTva, c => c.Index = -1)
            .Column(d => d.ValoareTva, c => c.Index = -1);
    }

    static void Decont(UiBaselineRegistry registry) {
        var entitate = registry.For<DecontDetaliu>();
        entitate.HideMembers(
            d => d.TipMaterialId, d => d.LotId, d => d.TipTvaId, d => d.AngajamentId,
            d => d.ContDebitId, d => d.ContCreditId, d => d.RepartitorDebitId, d => d.RepartitorCreditId);
        entitate.ListView(nameof(DecontDetaliu) + ListView, _ => { })
            .Column(d => d.TipMaterial, c => c.Index = 0)
            .Column(d => d.Descriere, c => c.Index = 1)
            .Column(d => d.Cantitate, c => c.Index = 2)
            .Column(d => d.PretUnitar, c => c.Index = 3)
            .Column(d => d.TipTva, c => c.Index = 4)
            .Column(d => d.ValoareTva, c => c.Index = 5)
            .Column(d => d.Valoare, c => c.Index = 6)
            // Postarea explicită pe linie (ILinieCuPostareExplicita) — la coadă.
            .Column(d => d.ContDebit, c => c.Index = 7)
            .Column(d => d.ContCredit, c => c.Index = 8)
            .Column(d => d.RepartitorDebit, c => c.Index = 9)
            .Column(d => d.RepartitorCredit, c => c.Index = 10);
    }

    static void DescarcareGestiune(UiBaselineRegistry registry) {
        var entitate = registry.For<DescarcareGestiuneDetaliu>();
        entitate.HideMembers(d => d.LinieSursaId, d => d.TipMaterialId, d => d.LotId, d => d.TipTvaId, d => d.AngajamentId);
        entitate.ListView(nameof(DescarcareGestiuneDetaliu) + ListView, _ => { })
            .Column(d => d.LinieSursa, c => { c.Index = 0; c.Caption = "Linie sursă"; })
            .Column(d => d.TipMaterial, c => c.Index = 1)
            .Column(d => d.Lot, c => c.Index = 2)
            .Column(d => d.Cantitate, c => c.Index = 3)
            .Column(d => d.Valoare, c => c.Index = 4)
            // DSC nu poartă TVA (integral pe FCL) — ascunde coloanele din bază.
            .Column(d => d.TipTva, c => c.Index = -1)
            .Column(d => d.ValoareTva, c => c.Index = -1);
    }
}
