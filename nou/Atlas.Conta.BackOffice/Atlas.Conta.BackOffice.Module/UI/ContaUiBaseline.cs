using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.DXF.Core.Views;
using Atlas.DXF.Core.Views.Discovery;
using Atlas.DXF.Core.Views.Fluent;
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
// Layout-ul DetailView-urilor de document e TOT aici, declarativ: `.Layout(...)`
// (Atlas.DXF 26.1.3.9) — API AUTORITAR, aplicat de `UiLayoutUpdater` chiar în
// generatorul de layout, deci grupurile declarate ÎNLOCUIESC arborele generat.
// A înlocuit `[DetailViewLayout]` pe proprietăți + `LayoutDocumenteUpdater`
// (GATE XAF D12). ATENȚIE la confuzia clasică: `.Section()/.Group()/.Tabs()` de
// pe `DetailView(...)` rămân ADITIVE (adaugă doar ce lipsește din arbore) —
// pe un view real sunt no-op. Captions-urile câmpurilor stau tot pe proprietăți
// ([XafDisplayName]), ca să fie identice în ListView și DetailView.
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
        LayoutDocumente(registry);
        DetaliuGeneric(registry);
        FacturaIntrare(registry);
        Nir(registry);
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
        registry.For<NirDetaliu>().HideForeignKeys();               // ProdusId (F5-D1)
        registry.For<FacturaIesireDetaliu>().HideForeignKeys();     // ProdusId
        registry.For<DecontDetaliu>().HideForeignKeys();            // ContDebitId/ContCreditId/RepartitorDebitId/RepartitorCreditId
        registry.For<DescarcareGestiuneDetaliu>().HideForeignKeys();// LinieSursaId
        registry.For<NotaContabilaDetaliu>().HideForeignKeys();     // ContDebitId/ContCreditId/RepartitorDebitId/RepartitorCreditId
        registry.For<AsamblareDetaliu>().HideForeignKeys();          // fără FK propriu — declarația ține convenția pe derivată

        // Tipuri în afara ierarhiei de documente:
        registry.For<RegistruStoc>().HideForeignKeys();             // LotId/RepartitorId/DocumentId/DetaliuId
        // Coloana `Lot` iese din ListView-ul registrului de stoc (review advers
        // D6): de când `Lot` are DefaultProperty, afișarea ei evaluează
        // `Eticheta`, care citește `Produs` LAZY — pe 282k rânduri asta e N+1 per
        // pagină randată, exact tiparul pentru care s-a introdus
        // `ConfigureDimensiuniEager` la 41c. Registrul rămâne complet (produs,
        // gestiune, cantitate, valoare); identitatea lotului se citește pe
        // documentul sursă sau, la pasul 5, în proiecții (42c). Nomenclatorul de
        // Loturi și lookup-urile păstrează eticheta — acolo e chiar rostul ei, iar
        // seturile sunt mărginite de căutare.
        registry.For<RegistruStoc>()
            .ListView(nameof(RegistruStoc) + ListView, _ => { })
            .Column(r => r.Lot, c => c.Index = -1);
        // DIM-3: dimensiunile registrului și ale regulii de contare sunt FK-uri
        // PLATE cu navigație pereche — convenția HideForeignKeys le acoperă pe
        // toate (fostul bloc de path-uri nested ale owned-ului a murit).
        registry.For<RegistruContabil>().HideForeignKeys();         // ContDebitId/ContCreditId/DocumentId/DetaliuId + Debit*/Credit*
        registry.For<RegulaContare>().HideForeignKeys();            // TipDocumentId/TipMaterialId/Cont* + Comun*/Override*
        registry.For<Lot>().HideForeignKeys();                      // ProdusId/GestiuneId (LinieIntrareId orfan → rămâne)
        registry.For<Imperechere>().HideForeignKeys();              // DocumentStingatorId/DocumentId
    }

    // Layout-ul DetailView-urilor de document (GATE XAF D12), declarat autoritar.
    //
    // Compunerea e BAZĂ-ÎNTÂI: grupurile declarate pe `Document` vin primele,
    // cele ale derivatei se adaugă DUPĂ. De aici containerul `Antet`: grupurile
    // proprii facturii (Scadență/Plată/Altele, Livrare) se declară NESTED în el,
    // cu același id, deci se concatenează ÎNĂUNTRU — între identificarea
    // documentului și grid, unde le voia ordinea de culegere. Fără el ar fi
    // aterizat după „Stare & totaluri". `Antet` n-are caption (e doar ordine).
    //
    // Ordinea de pe ecran: identificare → câmpuri proprii tipului → liniile →
    // stare & totaluri (`Total` e confruntarea cu hârtia înainte de operare, D5,
    // deci stă sub grid). Membrii nedeclarați (proprietățile celorlalte 10 tipuri
    // de document) NU dispar: `UiLayoutUpdater` îi mătură într-un grup final —
    // exact plasa de siguranță pentru „am adăugat o proprietate și am uitat-o".
    // Membrii ascunși (HideMembers/HideForeignKeys) declarați aici s-ar sări cu
    // log; nu declarăm niciunul (TethysId, CHITANTA_* — 31e — rămân în schemă).
    static void LayoutDocumente(UiBaselineRegistry registry) {
        registry.ForHierarchy<Document>()
            .Layout(l => l
                .Group("Antet", null, g => g
                    .Group("GrupDocument", "Document", d => d
                        .Item(x => x.Numar)
                        .Item(x => x.Data)
                        .Item(x => x.Predator)
                        .Item(x => x.Primitor)))
                .Group("GrupDetalii", "Detalii", g => g
                    // Colecția se plasează pe NUME (selectorul tipizat nu exprimă
                    // membrii de colecție); grupul propriu oglindește convenția
                    // XAF `{item}_Group` — un grid într-o cutie comună citește prost.
                    .Item(nameof(Document.Detalii)))
                .Group("GrupStare", "Stare & totaluri", g => g
                    .Item(x => x.Stare)
                    .Item(x => x.DataOperare)
                    .Item(x => x.DocumentSursa)
                    .Item(x => x.Autogenerat)
                    .Item(x => x.Total)));

        registry.For<FacturaIntrare>()
            .Layout(l => l
                .Group("Antet", null, g => g
                    .Group("GrupScadenta", "Scadență & PV", d => d
                        .Item(x => x.DataScadenta)
                        .Item(x => x.NumarPV)
                        .Item(x => x.DataPV))
                    // Fostul grup DECONT_* — parametrii plății autogenerate (31e).
                    .Group("GrupPlata", "Plată", d => d
                        .Item(x => x.GenereazaPlata)
                        .Item(x => x.PlataContPropriu)
                        .Item(x => x.PlataNumar)
                        .Item(x => x.PlataData)
                        .Item(x => x.PlataTipInstrument))
                    .Group("GrupAltele", "Altele", d => d
                        .Item(x => x.CodCpv)
                        .Item(x => x.Valuta)
                        .Item(x => x.Curs))));

        registry.For<FacturaIesire>()
            .Layout(l => l
                .Group("Antet", null, g => g
                    .Group("GrupLivrare", "Livrare", d => d
                        .Item(x => x.DataScadenta)
                        .Item(x => x.GestiuneDescarcare))));
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
        // Coloanele proprii, după identificarea documentului (vezi ListaRoot):
        // scadența/PV întâi, apoi câmpurile de curs și grupul plății.
        ListaRoot<FacturaIntrare>(registry)
            .Column(d => d.DataScadenta, c => c.Index = 10)
            .Column(d => d.NumarPV, c => c.Index = 11)
            .Column(d => d.DataPV, c => c.Index = 12)
            .Column(d => d.CodCpv, c => c.Index = 13)
            .Column(d => d.Valuta, c => c.Index = 14)
            .Column(d => d.Curs, c => c.Index = 15)
            .Column(d => d.GenereazaPlata, c => c.Index = 16)
            .Column(d => d.PlataContPropriu, c => c.Index = 17)
            .Column(d => d.PlataNumar, c => c.Index = 18)
            .Column(d => d.PlataData, c => c.Index = 19)
            .Column(d => d.PlataTipInstrument, c => c.Index = 20);

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
            // `Valoare` e REZULTAT (preț × cantitate, prin regimul TipTva) — se
            // recalculează la culegere (D5) și `PregatesteOperare` o rescrie
            // necondiționat la operare. Editabilă, ar invita operatorul s-o
            // „corecteze", iar valoarea tastată s-ar pierde fără mesaj (review
            // advers D7). `ValoareTva` RĂMÂNE editabilă — acolo overrideul e
            // deliberat păstrat de motor (36a: factura bate rotunjirea noastră).
            .Column(d => d.Valoare, c => { c.Index = 7; c.AllowEdit = false; })
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
            // Idem `Valoare` (review advers D7): rezultat, nu culegere.
            if (dv.Items[nameof(DocumentDetaliu.Valoare)] is IModelCommonMemberViewItem valoare)
                valoare.AllowEdit = false;
        });
    }

    // Review advers F5-F1: felia 5 a făcut din ecranul XAF de NIR o cale VIE de
    // culegere (`DocumenteLoturiCulegereController` e țintit pe `Document`, iar
    // `NirDetaliu` are acum Produs + PretUnitar) — dar fără oglinda blocului de
    // mai sus lotul rămânea EDITABIL lângă ele, adică exact bypass-ul închis pe
    // FCT la GOL 1. Concret: operatorul culege produs și preț ȘI alege din
    // nomenclator lotul unei recepții anterioare; serviciul de culegere vede lot
    // străin și tace (gardul F5-D3), produsul și prețul rămân inerte fără niciun
    // mesaj, iar operarea adaugă +cantitate pe un lot DEJA în stoc, evaluat la
    // prețul LUI — marfă re-recepționată, invizibilă pentru gardianul de sold
    // (mișcarea e pozitivă) și o notă `3xx = 401` fără legătură cu hârtia
    // furnizorului. Gardul rămâne corect; aici i se închide capcana.
    static void Nir(UiBaselineRegistry registry) {
        var entitate = registry.For<NirDetaliu>();
        entitate.HideMembers(d => d.TipMaterialId, d => d.LotId, d => d.TipTvaId, d => d.AngajamentId);
        entitate.ListView(nameof(NirDetaliu) + ListView, _ => { })
            // Ordinea coloanelor = ordinea de culegere: produsul dă Tipul și naște
            // lotul recepției manuale.
            .Column(d => d.Produs, c => c.Index = 0)
            .Column(d => d.TipMaterial, c => c.Index = 1)
            // Lotul e al mecanismului (născut din produs + gestiunea primitoare pe
            // recepția manuală, MOȘTENIT de pe factură pe clona conexă): read-only
            // pe ambele cazuri — pe conex nici nu e al liniei (F5-D4).
            .Column(d => d.Lot, c => { c.Index = 2; c.AllowEdit = false; })
            .Column(d => d.Cantitate, c => c.Index = 3)
            .Column(d => d.PretUnitar, c => c.Index = 4)
            // `Valoare` e REZULTAT (F5-D6): `PregatesteOperare` o rescrie
            // necondiționat, pe ambele ramuri. Editabilă, valoarea tastată s-ar
            // pierde fără mesaj — review advers D7, aceeași concluzie ca pe FCT.
            .Column(d => d.Valoare, c => { c.Index = 5; c.AllowEdit = false; })
            .Column(d => d.DataExpirare, c => c.Index = 6)
            .Column(d => d.LotFabricatie, c => c.Index = 7)
            // NIR-ul nu culege TVA (F5-D5): pe clona conexă `TipTva` e informativ
            // (clonat din factură), `ValoareTva` e mereu 0 — a le arăta la culegere
            // ar sugera că se completează aici.
            .Column(d => d.TipTva, c => c.Index = -1)
            .Column(d => d.ValoareTva, c => c.Index = -1);
        // Dialogul de New/edit al colecției (40a) — acolo AllowEdit-ul coloanei nu
        // ajunge, iar bypass-ul ar rămâne deschis pe cealaltă cale.
        entitate.DetailView(nameof(NirDetaliu) + "_DetailView", dv => {
            if (dv.Items[nameof(NirDetaliu.Lot)] is IModelCommonMemberViewItem lot)
                lot.AllowEdit = false;
            if (dv.Items[nameof(DocumentDetaliu.Valoare)] is IModelCommonMemberViewItem valoare)
                valoare.AllowEdit = false;
        });
    }

    static void FacturaIesire(UiBaselineRegistry registry) {
        ListaRoot<FacturaIesire>(registry)
            .Column(d => d.DataScadenta, c => c.Index = 10)
            .Column(d => d.GestiuneDescarcare, c => c.Index = 11);

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
            // Rezultat, nu culegere — vezi nota de pe FCT (review advers D7).
            .Column(d => d.Valoare, c => { c.Index = 7; c.AllowEdit = false; })
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
    //
    // Indicii bazei sunt 0–4 și NU ajung singuri: coloanele proprii derivatei
    // păstrează indicii generați (tot 0..n), iar grila le sortează INTERCALAT
    // (Scadență, Număr, Dată, Număr PV, Dată PV, Predator, …) — găsit la smoke-ul
    // migrării de layout, cu simptomul din nota de mai sus doar pe jumătate
    // rezolvat. De aceea apelantul continuă lanțul cu propriile coloane, de la
    // 10 în sus; „gaura" 5–9 lasă loc unor coloane comune viitoare.
    static ListViewFluent<T> ListaRoot<T>(UiBaselineRegistry registry) where T : Document
        => registry.For<T>()
            .ListView(typeof(T).Name + ListView, _ => { })
            .Column(d => d.Numar, c => c.Index = 0)
            .Column(d => d.Data, c => c.Index = 1)
            .Column(d => d.Predator, c => c.Index = 2)
            .Column(d => d.Primitor, c => c.Index = 3)
            .Column(d => d.Stare, c => c.Index = 4)
            .Column(d => d.Total, c => c.Index = -1);

    // F6-D10 (lecția F5-F1, aplicată preventiv): declararea `ILinieCareNasteLot`
    // face din ecranul LDI o cale VIE de culegere — `DocumenteLoturiCulegereController`
    // e țintit pe `Document`. Cele două direcții culeg lucruri DIFERITE, iar
    // câmpurile celeilalte sunt capcane: pe plus lotul e al mecanismului (născut
    // din produs + gestiunea inventariată), pe minus produsul și prețul de
    // evaluare sunt inerte. Comutarea o face `[Appearance]` de pe frunză;
    // ordinea coloanelor de aici e ordinea de culegere a plusului.
    static void ListaDiferenteInventar(UiBaselineRegistry registry) {
        var entitate = registry.For<ListaDiferenteInventarDetaliu>();
        entitate.HideMembers(d => d.ProdusId, d => d.TipMaterialId, d => d.LotId, d => d.TipTvaId, d => d.AngajamentId);
        entitate.ListView(nameof(ListaDiferenteInventarDetaliu) + ListView, _ => { })
            .Column(d => d.Directie, c => c.Index = 0)
            .Column(d => d.TipMaterial, c => c.Index = 1)
            .Column(d => d.Produs, c => c.Index = 2)
            .Column(d => d.Lot, c => c.Index = 3)
            .Column(d => d.Cantitate, c => c.Index = 4)
            .Column(d => d.PretEvaluare, c => c.Index = 5)
            // Rezultat, nu culegere: `PregatesteOperare` o rescrie necondiționat
            // pe ambele direcții — vezi nota de pe FCT (review advers D7).
            .Column(d => d.Valoare, c => { c.Index = 6; c.AllowEdit = false; })
            .Column(d => d.DataExpirare, c => c.Index = 7)
            .Column(d => d.LotFabricatie, c => c.Index = 8)
            // LDI n-are semantică de TVA — ascunde coloanele moștenite din bază.
            .Column(d => d.TipTva, c => c.Index = -1)
            .Column(d => d.ValoareTva, c => c.Index = -1);
        // Dialogul de New/edit al colecției (40a) — acolo AllowEdit-ul coloanei nu
        // ajunge; `Lot`/`Produs` NU se blochează aici: editabilitatea lor e pe
        // direcție ([Appearance] de pe frunză), nu necondiționată ca la FCT/NIR.
        entitate.DetailView(nameof(ListaDiferenteInventarDetaliu) + "_DetailView", dv => {
            if (dv.Items[nameof(DocumentDetaliu.Valoare)] is IModelCommonMemberViewItem valoare)
                valoare.AllowEdit = false;
        });
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
