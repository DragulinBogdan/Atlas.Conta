using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.DXF.Core.Views;
using Atlas.DXF.Core.Views.Discovery;

namespace Atlas.Conta.BackOffice.Module.UI;

// Baseline de coloane (EntityFluent) pentru cele 6 ListView-uri de detaliu
// tipizate comutate de TipDetaliuViewUpdater. Conținut: ordinea logică de culegere
// (Index) + ascunderea FK-urilor brute (zgomot). Sunt DEFAULT-uri — diff-urile
// utilizatorului din Model Editor rămân prioritare (SetIfEmpty/SetIfDefault).
// Coloanele de TVA se ascund view-scoped (Index = -1) pe tipurile fără semantică
// de TVA (LDI/DSC) — membrul rămâne vizibil pe tipurile care îl folosesc.
public sealed class ContaUiBaseline : IUiBaselineProvider {
    // Sufixul id-ului ListView-ului implicit generat de XAF per clasă.
    const string ListView = "_ListView";

    public void Register(UiBaselineRegistry registry) {
        AscundeFkuriBrute(registry);
        FacturaIntrare(registry);
        FacturaIesire(registry);
        ListaDiferenteInventar(registry);
        Decont(registry);
        DescarcareGestiune(registry);
        NotaContabila(registry);
        Asamblare(registry);
    }

    // Ascunderea generică a scalarilor `{Nav}Id` care au navigație pereche
    // (convenția HideForeignKeys, Atlas.DXF 26.1.3.6) — înlocuiește listele
    // manuale de HideMembers pe FK-uri (rămân, dedupe-ul le face inofensive).
    // Pe ierarhii se aplică prin asignabilitate: o declarație pe bază acoperă
    // toate derivatele; FK-urile PROPRII derivatelor cer declarație pe tip.
    static void AscundeFkuriBrute(UiBaselineRegistry registry) {
        // Bazele documentelor: Predator/Primitor/DocumentSursa (Document);
        // Document/TipMaterial/Lot/TipTva/Angajament (DocumentDetaliu). Owned-ul
        // Dimensiuni n-are scalar pereche → nu e atins.
        registry.ForHierarchy<Document>().HideForeignKeys();
        registry.ForHierarchy<DocumentDetaliu>().HideForeignKeys();

        // FK-uri proprii, nevăzute pe baza ierarhiei:
        registry.For<FacturaIntrare>().HideForeignKeys();           // PlataContPropriuId
        registry.For<FacturaIesire>().HideForeignKeys();            // GestiuneDescarcareId
        registry.For<FacturaIesireDetaliu>().HideForeignKeys();     // ProdusId
        registry.For<DecontDetaliu>().HideForeignKeys();            // ContDebitId/ContCreditId/RepartitorDebitId/RepartitorCreditId
        registry.For<DescarcareGestiuneDetaliu>().HideForeignKeys();// LinieSursaId
        registry.For<NotaContabilaDetaliu>().HideForeignKeys();     // ContDebitId/ContCreditId/RepartitorDebitId/RepartitorCreditId
        registry.For<AsamblareDetaliu>().HideForeignKeys();          // fără FK propriu — declarația ține convenția pe derivată

        // Tipuri în afara ierarhiei de documente:
        registry.For<RegistruStoc>().HideForeignKeys();             // LotId/RepartitorId/DocumentId/DetaliuId
        registry.For<RegistruContabil>().HideForeignKeys();         // ContDebitId/ContCreditId/DocumentId/DetaliuId
        registry.For<Lot>().HideForeignKeys();                      // ProdusId/GestiuneId (LinieIntrareId orfan → rămâne)
        registry.For<Imperechere>().HideForeignKeys();              // DocumentStingatorId/DocumentId

        // Expansiunea InDetailView a owned-urilor (Registre.cs) generează item-uri
        // și pentru scalarii FK INTERNI ai owned-ului, cu path nested
        // (DimensiuniDebit.RepartitorId) — HideForeignKeys pe owner nu-i vede
        // (descoperirea e pe membrii direcți), deci se ascund explicit.
        string[] fkDimensiuni = [
            nameof(Dimensiuni.RepartitorId), nameof(Dimensiuni.MaterialId),
            nameof(Dimensiuni.CodFunctionalId), nameof(Dimensiuni.CodEconomicId),
            nameof(Dimensiuni.SursaFinantareId), nameof(Dimensiuni.UnitateId),
            nameof(Dimensiuni.ProiectId), nameof(Dimensiuni.CentruCostId),
        ];
        foreach (var latura in new[] {
                     nameof(RegistruContabil.DimensiuniDebit),
                     nameof(RegistruContabil.DimensiuniCredit) })
            registry.For<RegistruContabil>()
                .HideMembers(fkDimensiuni.Select(fk => $"{latura}.{fk}").ToArray());
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

    // Nota contabilă (FAZA 1C §5): linia E postarea — conturile și repartitorii
    // per latură sunt câmpurile de culegere. Restul semanticii bazei (TipMaterial
    // convențional TRZ, lot, cantitate, TVA) nu se folosește pe notă și se ascunde.
    static void NotaContabila(UiBaselineRegistry registry) {
        var entitate = registry.For<NotaContabilaDetaliu>();
        entitate.HideMembers(
            d => d.TipMaterialId, d => d.LotId, d => d.TipTvaId, d => d.AngajamentId,
            d => d.ContDebitId, d => d.ContCreditId, d => d.RepartitorDebitId, d => d.RepartitorCreditId);
        entitate.ListView(nameof(NotaContabilaDetaliu) + ListView, _ => { })
            .Column(d => d.Descriere, c => c.Index = 0)
            .Column(d => d.ContDebit, c => c.Index = 1)
            .Column(d => d.ContCredit, c => c.Index = 2)
            .Column(d => d.RepartitorDebit, c => c.Index = 3)
            .Column(d => d.RepartitorCredit, c => c.Index = 4)
            .Column(d => d.Valoare, c => c.Index = 5)
            // Coloanele moștenite fără semantică pe notă.
            .Column(d => d.TipMaterial, c => c.Index = -1)
            .Column(d => d.Lot, c => c.Index = -1)
            .Column(d => d.Cantitate, c => c.Index = -1)
            .Column(d => d.TipTva, c => c.Index = -1)
            .Column(d => d.ValoareTva, c => c.Index = -1);
    }

    // Asamblarea (FAZA 1C §7): rolul liniei (consum/produs) e primul câmp de
    // culegere — restul e schema de stoc (lot, cantitate, preț de evaluare pe
    // liniile de produs). ASM nu poartă TVA (marfa se mută între loturi).
    static void Asamblare(UiBaselineRegistry registry) {
        var entitate = registry.For<AsamblareDetaliu>();
        entitate.HideMembers(d => d.TipMaterialId, d => d.LotId, d => d.TipTvaId, d => d.AngajamentId);
        entitate.ListView(nameof(AsamblareDetaliu) + ListView, _ => { })
            .Column(d => d.Directie, c => c.Index = 0)
            .Column(d => d.TipMaterial, c => c.Index = 1)
            .Column(d => d.Lot, c => c.Index = 2)
            .Column(d => d.Cantitate, c => c.Index = 3)
            .Column(d => d.PretEvaluare, c => c.Index = 4)
            .Column(d => d.Valoare, c => c.Index = 5)
            .Column(d => d.DataExpirare, c => c.Index = 6)
            .Column(d => d.LotFabricatie, c => c.Index = 7)
            // ASM nu poartă TVA — ascunde coloanele moștenite din bază.
            .Column(d => d.TipTva, c => c.Index = -1)
            .Column(d => d.ValoareTva, c => c.Index = -1);
    }
}
