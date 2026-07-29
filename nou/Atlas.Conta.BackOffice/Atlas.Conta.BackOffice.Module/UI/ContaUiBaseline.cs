using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.DXF.Core.Views;
using Atlas.DXF.Core.Views.Discovery;
using DevExpress.ExpressApp.Model;

namespace Atlas.Conta.BackOffice.Module.UI;

// Baseline de coloane (EntityFluent) pentru ListView-urile de detaliu: cele
// tipizate comutate de TipDetaliuViewUpdater + cel GENERIC al bazei
// (`Document_Detalii_ListView`, folosit de NIR/BTR/BCS/PLT/INC/RLF/RDC).
// Conținut: ordinea logică de culegere (Index) + ascunderea FK-urilor brute
// (zgomot) + câteva capacități view-scoped. Sunt DEFAULT-uri — diff-urile
// utilizatorului din Model Editor rămân prioritare (SetIfEmpty/SetIfDefault).
// Coloanele de TVA se ascund view-scoped (Index = -1) pe tipurile fără semantică
// de TVA (LDI/DSC) — membrul rămâne vizibil pe tipurile care îl folosesc.
//
// Layout-ul DetailView-urilor NU e aici: `[DetailViewLayout]` pe proprietăți +
// `LayoutDocumenteUpdater` pentru etichete (vezi comentariul din LayoutDocumente.cs
// pentru motivul de fond — `.Section()/.Group()` din EntityFluent nu poate
// re-aranja un layout deja generat). Captions-urile câmpurilor stau pe
// proprietăți ([XafDisplayName]) ca să fie identice în ListView și DetailView.
//
// SUMAR de grilă (sumă pe Valoare/ValoareTva) — NEIMPLEMENTAT deliberat:
// `IModelColumn.Summary` e citit doar de grila WinForms (docs DevExpress), iar în
// Blazor sumarul cere un ViewController pe `DxGridListEditor.GridSummary`
// (`ViewSummaryController` din Atlas.DXF e o acțiune interactivă, gated pe
// extenderul `AutoSummary` din assembly-ul Blazor, la care Module-ul nu are
// acces). Nevoia operatorului e acoperită de `Total` pe DetailView (D5).
public sealed class ContaUiBaseline : IUiBaselineProvider {
    // Sufixul id-ului ListView-ului implicit generat de XAF per clasă.
    const string ListView = "_ListView";

    public void Register(UiBaselineRegistry registry) {
        AscundeFkuriBrute(registry);
        DetaliuGeneric(registry);
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
        registry.For<FacturaIntrareDetaliu>().HideForeignKeys();    // ProdusId (GATE XAF D1)
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

    // ListView-ul GENERIC al colecției `Detalii` (id-ul nested generat de XAF din
    // clasa care DECLARĂ membrul: `Document_Detalii_ListView`, unul singur pentru
    // toată ierarhia). Îl folosesc tipurile fără detaliu derivat: NIR (conexul
    // FCT!), BTR, BCS, PLT, INC, RLF, RDC — ordinea de mai jos le prinde pe toate.
    // Dimensiunile/Angajamentul rămân după coloanele de bază (index nealocat).
    static void DetaliuGeneric(UiBaselineRegistry registry) {
        registry.For<DocumentDetaliu>()
            .ListView(nameof(Document) + "_" + nameof(Document.Detalii) + ListView, _ => { })
            .Column(d => d.TipMaterial, c => c.Index = 0)
            .Column(d => d.Lot, c => c.Index = 1)
            .Column(d => d.Cantitate, c => c.Index = 2)
            .Column(d => d.Valoare, c => c.Index = 3)
            .Column(d => d.TipTva, c => c.Index = 4)
            .Column(d => d.ValoareTva, c => c.Index = 5);
    }

    static void FacturaIntrare(UiBaselineRegistry registry) {
        // Header: câmpurile moarte (CHITANTA_* — 31e) și id-ul de import Tethys
        // dispar din view-uri, rămân în schemă (GATE XAF D12).
        registry.For<FacturaIntrare>()
            .HideMembers(d => d.GenereazaChitanta, d => d.ChitantaNumar, d => d.ChitantaData, d => d.TethysId);
        ListaRoot<FacturaIntrare>(registry);

        var entitate = registry.For<FacturaIntrareDetaliu>();
        entitate.HideMembers(d => d.TipMaterialId, d => d.LotId, d => d.TipTvaId, d => d.AngajamentId);
        entitate.ListView(nameof(FacturaIntrareDetaliu) + ListView, _ => { })
            // Produsul e PRIMUL: el dă Tipul (D3) și naște lotul (D2) — ordinea
            // coloanelor e ordinea de culegere.
            .Column(d => d.Produs, c => c.Index = 0)
            .Column(d => d.TipMaterial, c => c.Index = 1)
            // Lotul liniei de FCT e al mecanismului D2 (se naște din produs +
            // gestiunea primitoare, motorul îl finalizează): READ-ONLY view-scoped.
            // Alegerea manuală a unui lot străin era exact bypass-ul care rupea
            // TVA-ul de cost (GOL 1 al explorării) — se închide aici, nu la nivel
            // de membru: pe FCL/LDI/ASM/BTR lotul se CULEGE.
            .Column(d => d.Lot, c => { c.Index = 2; c.AllowEdit = false; })
            .Column(d => d.Cantitate, c => c.Index = 3)
            .Column(d => d.PretUnitar, c => c.Index = 4)
            .Column(d => d.TipTva, c => c.Index = 5)
            .Column(d => d.ValoareTva, c => c.Index = 6)
            .Column(d => d.Valoare, c => c.Index = 7)
            .Column(d => d.DataExpirare, c => c.Index = 8)
            .Column(d => d.LotFabricatie, c => c.Index = 9)
            .Column(d => d.CodCpv, c => c.Index = 10)
            // Al treilea număr pe aceeași linie (preț × cantitate, înaintea
            // regulilor de TVA) confundă la culegere: Valoare + Valoare TVA sunt
            // cele care se postează și se recalculează live (D5).
            .Column(d => d.ValoareReceptie, c => c.Index = -1);
        // Linia se culege și prin DetailView-ul propriu (dialogul de New/edit al
        // colecției — 40a), unde AllowEdit-ul coloanei nu ajunge: același lot
        // read-only și acolo, altfel bypass-ul rămâne deschis pe cealaltă cale.
        entitate.DetailView(nameof(FacturaIntrareDetaliu) + "_DetailView", dv => {
            if (dv.Items[nameof(FacturaIntrareDetaliu.Lot)] is IModelCommonMemberViewItem lot)
                lot.AllowEdit = false;
        });
    }

    static void FacturaIesire(UiBaselineRegistry registry) {
        ListaRoot<FacturaIesire>(registry);

        var entitate = registry.For<FacturaIesireDetaliu>();
        entitate.HideMembers(d => d.ProdusId, d => d.TipMaterialId, d => d.LotId, d => d.TipTvaId, d => d.AngajamentId);
        entitate.ListView(nameof(FacturaIesireDetaliu) + ListView, _ => { })
            .Column(d => d.Produs, c => c.Index = 0)
            // Pe FCL lotul e PIN-ul opțional (P2, 37d) — se culege, deci editabil.
            .Column(d => d.Lot, c => c.Index = 1)
            .Column(d => d.TipMaterial, c => c.Index = 2)
            .Column(d => d.Cantitate, c => c.Index = 3)
            .Column(d => d.PretUnitar, c => c.Index = 4)
            .Column(d => d.TipTva, c => c.Index = 5)
            .Column(d => d.ValoareTva, c => c.Index = 6)
            .Column(d => d.Valoare, c => c.Index = 7)
            .Column(d => d.Descriere, c => c.Index = 8)
            // Ca la FCT: preț × cantitate e un al treilea număr redundant lângă
            // Valoare / Valoare TVA.
            .Column(d => d.ValoareLivrare, c => c.Index = -1);
    }

    // ListView-ul ROOT al unui tip de document: identificarea documentului ÎNTÂI.
    // Găsit la smoke-ul UI al gate-ului: generatorul XAF așază coloanele derivatei
    // înaintea celor moștenite, deci lista de facturi începea cu Scadență / PV /
    // Cod CPV / grupul de plată, iar `Numar`, `Data`, `Predator`, `Primitor` erau
    // împinse în dreapta, în afara ecranului — un contabil nu-și găsea factura.
    //
    // `Document.Total` e [NotMapped] și enumerează Detalii ⇒ o coloană aici =
    // N+1 pe fiecare pagină (disciplina de hot-path, 35d; baza de smoke are 187k
    // documente). Se ascunde view-scoped (Index = -1, ca la coloanele de TVA fără
    // semantică) — pe DetailView rămâne, e câmpul cu care operatorul confruntă
    // hârtia înainte de operare (D5).
    static void ListaRoot<T>(UiBaselineRegistry registry) where T : Document {
        registry.For<T>()
            .ListView(typeof(T).Name + ListView, _ => { })
            .Column(d => d.Numar, c => c.Index = 0)
            .Column(d => d.Data, c => c.Index = 1)
            .Column(d => d.Predator, c => c.Index = 2)
            .Column(d => d.Primitor, c => c.Index = 3)
            .Column(d => d.Stare, c => c.Index = 4)
            .Column(d => d.Total, c => c.Index = -1);
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
