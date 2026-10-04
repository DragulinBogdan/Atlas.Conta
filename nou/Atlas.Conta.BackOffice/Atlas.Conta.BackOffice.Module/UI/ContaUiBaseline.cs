using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.DXF.Core.Views;
using Atlas.DXF.Core.Views.Discovery;
using Atlas.DXF.Core.Views.Fluent;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;

namespace Atlas.Conta.BackOffice.Module.UI;

// Baseline-ul XAF al Contei (EntityFluent). Grilele de linii se declară pe
// ROLURI (106h): vocabularul o dată pe ierarhia `DocumentDetaliu`, tipul spune
// ce roluri poartă; rezultatele motorului sunt `ReadOnly` pe grilă și pe dialog
// deopotrivă. Excepțiile pe view (captions, o coloană ascunsă pe o singură
// grilă) rămân view-scoped și câștigă în fața rolurilor. Sunt DEFAULT-uri —
// diff-urile utilizatorului din Model Editor rămân prioritare.
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
    // 85c — grila de culegere nested nu se face peste o colecție server.
    static readonly Action<IModelListView> Culegere = lv => lv.DataAccessMode = CollectionSourceDataAccessMode.Client;
    // F26-D12 — grila nested pe care operatorul nu o culege.
    static readonly Action<IModelListView> ReadOnly = lv => {
        Culegere(lv);
        lv.AllowEdit = false;
        lv.AllowNew = false;
        lv.AllowDelete = false;
    };

    // Coloană pe o CALE (`Factura.Numar`) — selectorul tipizat al fluent-ului
    // Atlas.DXF exprimă doar membri direcți.
    static void ColoanaPeCale(IModelListView lv, string cale, int index, string caption) {
        var col = lv.Columns.FirstOrDefault(c => c.PropertyName == cale)
            ?? lv.Columns.AddNode<IModelColumn>(cale);
        col.PropertyName = cale;
        col.Index = index;
        col.Caption = caption;
    }

    public void Register(UiBaselineRegistry registry) {
        AscundeFkuriBrute(registry);
        LayoutDocumente(registry);
        LiniiPeRoluri(registry);
        FacturaIntrare(registry);
        Nir(registry);
        FacturaIesire(registry);
        ListaDiferenteInventar(registry);
        Decont(registry);
        DescarcareGestiune(registry);
        NotaContabila(registry);
        Asamblare(registry);
        Dvi(registry);
        Imobilizari(registry);
        Perioade(registry);
        ColoanaTip(registry);
        registry.For<DocumentDetaliu>().ListView(nameof(DocumentDetaliu) + "_LookupListView")
            .Column(d => d.Document, c => c.Index = 0)
            .Column(d => d.Pozitie, c => c.Index = 1)
            .Column(d => d.TipTva, c => c.Index = 2)
            .Column(d => d.Valoare, c => c.Index = 3);
        registry.For<FacturaIntrareDetaliu>().HideMembers(d => d.LinieAvansId);
        registry.For<FacturaIesireDetaliu>().HideMembers(d => d.LinieAvansId);
    }

    // 89 — „Tip” doar pe listele care amestecă tipuri; pe frunze e constant.
    static void ColoanaTip(UiBaselineRegistry registry) {
        registry.For<Document>()
            .ListView(nameof(Document) + ListView)
            .Column(d => d.ClrType, c => c.Index = 1);
        registry.For<DocumentTrezorerie>()
            .ListView(nameof(DocumentTrezorerie) + ListView)
            .Column(d => d.ClrType, c => c.Index = 1);
        registry.For<Repartitor>()
            .ListView(nameof(Repartitor) + ListView)
            .Column(r => r.ClrType, c => c.Index = 1)
            .ListView(nameof(Repartitor) + "_LookupListView")
            .Column(r => r.ClrType, c => c.Index = 1);
    }

    // F27-D1: lanțul se citește în ordine cronologică, nu în ordinea inserării;
    // istoricul e listă imbricată pe perioadă (registru, deci fără CRUD).
    static void Perioade(UiBaselineRegistry registry) {
        registry.For<PerioadaFiscala>()
            .ListView(nameof(PerioadaFiscala) + ListView)
            .Column(p => p.An, c => { c.Index = 0; c.SortIndex = 0; c.SortOrder = DevExpress.Data.ColumnSortOrder.Ascending; })
            .Column(p => p.Luna, c => { c.Index = 1; c.SortIndex = 1; c.SortOrder = DevExpress.Data.ColumnSortOrder.Ascending; })
            .Column(p => p.Inchisa, c => c.Index = 2)
            .Column(p => p.InchisaLa, c => c.Index = 3)
            .Column(p => p.InchisaPrimaOara, c => c.Index = -1);
        registry.For<InchiderePerioada>().HideForeignKeys();
        registry.For<InchiderePerioada>()
            .ListView(nameof(PerioadaFiscala) + "_" + nameof(PerioadaFiscala.Istoric) + ListView)
            .Column(i => i.La, c => { c.Index = 0; c.SortIndex = 0; c.SortOrder = DevExpress.Data.ColumnSortOrder.Ascending; })
            .Column(i => i.Fel, c => c.Index = 1)
            .Column(i => i.De, c => c.Index = 2)
            .Column(i => i.Motiv, c => c.Index = 3)
            .Column(i => i.Acceptari, c => c.Index = 4)
            .Column(i => i.Perioada, c => c.Index = -1);
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
        // LaturaPerecheId (F8-D6) e declarat pe nivelul ABSTRACT `DocumentTrezorerie`,
        // deci nu-l vede descoperirea pe ierarhia `Document` (ea citește doar
        // membrii bazei ei). `ForHierarchy` de aici descoperă membrii trezoreriei
        // și îi aplică pe Plata/Incasare prin asignabilitate — o singură
        // declarație pentru ambele frunze.
        registry.ForHierarchy<DocumentTrezorerie>().HideForeignKeys();
        registry.For<FacturaIntrareDetaliu>().HideForeignKeys();    // ProdusId (GATE XAF D1)
        registry.For<NirDetaliu>().HideForeignKeys();               // ProdusId (F5-D1)
        registry.For<FacturaIesireDetaliu>().HideForeignKeys();     // ProdusId
        registry.For<DecontDetaliu>().HideForeignKeys();            // ContDebitId/ContCreditId/RepartitorDebitId/RepartitorCreditId
        registry.For<DescarcareGestiuneDetaliu>().HideForeignKeys();// LinieSursaId
        registry.For<NotaContabilaDetaliu>().HideForeignKeys();     // ContDebitId/ContCreditId/RepartitorDebitId/RepartitorCreditId
        registry.For<AsamblareDetaliu>().HideForeignKeys();          // ProdusId (F19-D3)

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
            .Column(r => r.Lot, c => c.Index = -1)
            // 85b — `Document`/`DocumentDetaliu` n-au DefaultProperty: ar apărea ca GUID.
            .Column(r => r.Document, c => c.Index = -1)
            .Column(r => r.Detaliu, c => c.Index = -1);
        // DIM-3: dimensiunile registrului și ale regulii de contare sunt FK-uri
        // PLATE cu navigație pereche — convenția HideForeignKeys le acoperă pe
        // toate (fostul bloc de path-uri nested ale owned-ului a murit).
        registry.For<RegistruContabil>().HideForeignKeys();         // ContDebitId/ContCreditId/DocumentId/DetaliuId + Debit*/Credit*
        registry.For<RegistruContabil>()
            .ListView(nameof(RegistruContabil) + ListView, _ => { })
            // 85b — `Document`/`DocumentDetaliu` n-au DefaultProperty: ar apărea ca GUID.
            .Column(r => r.Document, c => c.Index = -1)
            .Column(r => r.Detaliu, c => c.Index = -1);
        registry.For<RegulaContare>().HideForeignKeys();            // TipDocumentId/TipMaterialId/Cont* + Comun*/Override*
        registry.For<RegistruTva>().HideForeignKeys();              // DocumentId/DetaliuId/PartenerId/TipTvaId
        // Navigațiile registrului de TVA ies din ListView, din același motiv ca
        // `RegistruStoc.Lot` de mai sus: sunt LAZY (registrul nu are AutoInclude
        // — vezi DbContext) iar el e de ordinul sutelor de mii de rânduri, deci
        // afișarea lor ar fi N+1 per pagină randată. Rândul rămâne complet
        // (data, sens, regim, cotă, bază, TVA); identitatea documentului și a
        // partenerului se citește în jurnale, unde join-ul e explicit (JT-D7).
        registry.For<RegistruTva>()
            .ListView(nameof(RegistruTva) + ListView, _ => { })
            .Column(r => r.Document, c => c.Index = -1)
            .Column(r => r.Detaliu, c => c.Index = -1)
            .Column(r => r.Partener, c => c.Index = -1)
            .Column(r => r.TipTva, c => c.Index = -1)
            // Perioada de declarare stă lângă data faptului: diferența dintre
            // ele e chiar ce arată F27-D5. `ScrisLa` e timestamp tehnic (reperul
            // rectificativei) — se citește în raport, nu în listă.
            .Column(r => r.PerioadaAn, c => c.Index = 1)
            .Column(r => r.PerioadaLuna, c => c.Index = 2)
            .Column(r => r.ScrisLa, c => c.Index = -1);
        registry.For<Lot>().HideForeignKeys();                      // ProdusId/GestiuneId (LinieIntrareId orfan → rămâne)
        registry.For<Imperechere>().HideForeignKeys();              // DocumentStingatorId/DocumentId/InverseazaId
        // F27-D8: imperecherea e fapt datat, iar rândul invers se vede ca atare —
        // lista lor e singurul loc din XAF unde desfacerea se citește.
        registry.For<Imperechere>()
            .ListView(nameof(Imperechere) + ListView)
            .Column(i => i.Data, c => { c.Index = 0; c.SortIndex = 0; c.SortOrder = DevExpress.Data.ColumnSortOrder.Descending; })
            .Column(i => i.DocumentStingator, c => c.Index = 1)
            .Column(i => i.Document, c => c.Index = 2)
            .Column(i => i.Suma, c => c.Index = 3)
            .Column(i => i.Inverseaza, c => c.Index = 4)
            .Column(i => i.Autogenerat, c => c.Index = 5);
        registry.For<RandD300>().HideForeignKeys();                 // ParinteId/OglindaAId
        registry.For<MapareD300>().HideForeignKeys();               // TipTvaId/RandId
        registry.For<MapareD394>().HideForeignKeys();               // TipTvaId
        registry.For<PoliticaMiscareSaft>().HideForeignKeys();      // TipDocumentId (felia 17, D17-D1)
        registry.For<PoliticaTvaImplicit>().HideForeignKeys();      // TipDocumentId/TipTvaId (felia 23, F23-D2)

        // Partenerul (felia 14, D4-D1): identitatea fiscală într-un grup propriu.
        // Layout-ul e declarat integral (bază-întâi nu se aplică — `Repartitor`
        // n-are layout propriu), ca membrii bazei să nu cadă în `Unplaced`.
        // FK-urile brute (`JudetId`, `ContImplicitId`) nu intră în grupuri și
        // ar cădea în grupul-mătură al layout-ului — se ascund (41b), lookup-ul e
        // navigația.
        registry.For<Partener>().HideForeignKeys();
        registry.For<Partener>()
            .Layout(l => l
                .Group("GrupIdentificare", "Identificare", g => g
                    .Item(x => x.Cod)
                    .Item(x => x.Denumire)
                    .Item(x => x.Activ)
                    .Item(x => x.Calitati)
                    .Item(x => x.ContImplicit))
                .Group("GrupFiscal", "Fiscal", g => g
                    .Item(x => x.CodFiscal)
                    .Item(x => x.RegistruComert)
                    .Item(x => x.TipPersoana)
                    .Item(x => x.Tara)
                    .Item(x => x.InregistratTva)
                    .Item(x => x.TvaLaIncasare)
                    // Cele două date ale REGISTRULUI ANAF, lângă statutul pe
                    // care îl confirmă. Read-only pe ORICE cale de UI prin
                    // `[ModelDefault("AllowEdit","False")]` pe proprietăți (ca
                    // `Document.Stare` — 53c): timbrul e server-owned, iar
                    // inactivarea fiscală e un fapt al ANAF, nu o bifă.
                    .Item(x => x.DataSincronizareAnaf)
                    .Item(x => x.InactivFiscal))
                // Al treilea grup (felia 15, D15-D1): adresa structurată, în
                // ordinea în care o cere `AddressStructure` din SAF-T și în
                // care o culege un om (strada → număr → detalii → localitate
                // → județ → cod poștal). `Tara` rămâne în Fiscal: e cheia de
                // clasificare D394, nu o linie de adresă.
                .Group("GrupAdresa", "Adresă", g => g
                    .Item(x => x.Strada)
                    .Item(x => x.Numar)
                    .Item(x => x.DetaliiAdresa)
                    .Item(x => x.Localitate)
                    .Item(x => x.Judet)
                    .Item(x => x.CodPostal)));

        // Societatea raportoare (felia 16, D16-D1) — același tipar ca partenerul:
        // FK-urile brute (`JudetId`, `ContBancarId`) se ascund (41b), iar layout-ul
        // e declarat integral, în ordinea în care SAF-T cere antetul: cine e →
        // unde e → cu cine se vorbește → cum se raportează.
        registry.For<Societate>().HideForeignKeys();
        registry.For<Societate>()
            .Layout(l => l
                .Group("GrupIdentificare", "Identificare", g => g
                    .Item(x => x.Denumire)
                    .Item(x => x.CodFiscal)
                    .Item(x => x.InregistratTva)
                    .Item(x => x.RegistruComert)
                    .Item(x => x.Tara))
                .Group("GrupAdresa", "Adresă", g => g
                    .Item(x => x.Strada)
                    .Item(x => x.Numar)
                    .Item(x => x.DetaliiAdresa)
                    .Item(x => x.Localitate)
                    .Item(x => x.Judet)
                    .Item(x => x.CodPostal))
                .Group("GrupContact", "Contact", g => g
                    .Item(x => x.ContactNume)
                    .Item(x => x.ContactPrenume)
                    .Item(x => x.Telefon)
                    .Item(x => x.Email)
                    .Item(x => x.ContBancar))
                // Cele două câmpuri care nu sunt „date despre firmă", ci alegeri
                // de RAPORTARE: care plan de conturi declarăm și dacă punem CNP-ul
                // în identificatorul persoanelor fizice. Grup propriu tocmai ca să
                // nu pară adresă sau contact.
                .Group("GrupRaportare", "Raportare SAF-T", g => g
                    .Item(x => x.BazaContabila)
                    .Item(x => x.RaporteazaCnp)));

        // Produsul (felia 16, D16-D2): FK-ul nou `UnitateMasuraId` intră în
        // convenția de ascundere; lookup-ul e navigația. Fără layout declarat —
        // `Produs` n-avea unul, iar cele două câmpuri noi cad firesc în arborele
        // generat, după `UM`.
        registry.For<Produs>().HideForeignKeys();
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
                        .Item(x => x.DataInregistrare)
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
                    .Item(x => x.Total))
                // F27-D6: legătura de corecție, o singură declarație pe BAZĂ
                // (câmpurile sunt ale ei). Read-only prin `ModelDefault` pe
                // model; grupul se ascunde pe documentele care nu corectează
                // nimic, prin `[Appearance]` pe `Document`.
                .Group("GrupCorectie", "Corecție", g => g
                    .Item(x => x.Corecteaza)
                    .Item(x => x.MotivCorectie)));

        registry.For<FacturaIntrare>()
            .Layout(l => l
                .Group("Antet", null, g => g
                    .Group("GrupScadenta", "Scadență & PV", d => d
                        .Item(x => x.DataPrimire)
                        .Item(x => x.DataExigibilitate)
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

    // Vocabularul rolurilor liniei (106h). Ordinea sloturilor e a ierarhiei; un tip
    // umple sau lasă gol câte un slot. Un rol al bazei pe care tipul nu-l poartă se
    // ascunde; un membru fără rol vine la coadă, în ordinea generată.
    static class Rol {
        public const string Directie = "Directie", Identitate = "Identitate", Provenienta = "Provenienta",
            Unitate = "Unitate", Cantitate = "Cantitate", Pret = "Pret", Tva = "Tva", Valori = "Valori",
            AtributeLot = "AtributeLot", Conturi = "Conturi", Parametri = "Parametri";
    }

    // Grila generică `Document_Detalii_ListView` (BCS/BTR/RLF/RDC) și `DocumentDetaliu_ListView`
    // (DVI) primesc exact vocabularul bazei; DVI își pune excepțiile pe view (vezi `Dvi`).
    static void LiniiPeRoluri(UiBaselineRegistry registry) {
        registry.ForHierarchy<DocumentDetaliu>().Columns(c => c
            .Slot(Rol.Directie)
            .Slot(Rol.Identitate, d => d.TipMaterial)
            .Slot(Rol.Provenienta)
            .Slot(Rol.Unitate, d => d.Lot)
            .Slot(Rol.Cantitate, d => d.Cantitate)
            .Slot(Rol.Pret)
            .Slot(Rol.Tva, d => d.TipTva, d => d.ValoareTva)
            .Slot(Rol.Valori, d => d.Valoare)
            .Slot(Rol.AtributeLot)
            .Slot(Rol.Conturi)
            .Slot(Rol.Parametri));

        registry.For<DocumentTrezorerieDetaliu>()
            .HideForeignKeys()
            .Columns(c => c.Drop(Rol.Unitate).Drop(Rol.Cantitate));
    }

    static void FacturaIntrare(UiBaselineRegistry registry) {
        // Câmpurile moarte (CHITANTA_*, 31e) și id-ul de import Tethys rămân în schemă (GATE XAF D12).
        registry.For<FacturaIntrare>()
            .HideMembers(d => d.GenereazaChitanta, d => d.ChitantaNumar, d => d.ChitantaData, d => d.TethysId);
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

        // Lookup-ul (DVI alege factura de import, D7) cere identificarea facturii, nu
        // detaliile plății; coloanele proprii s-ar intercala, deci se ascund.
        registry.For<FacturaIntrare>()
            .ListView(nameof(FacturaIntrare) + "_LookupListView")
            .Column(d => d.Numar, c => c.Index = 0)
            .Column(d => d.Data, c => c.Index = 1)
            .Column(d => d.Predator, c => c.Index = 2)
            .Column(d => d.Stare, c => c.Index = 3)
            .Column(d => d.Total, c => c.Index = -1)
            .Column(d => d.DataScadenta, c => c.Index = -1)
            .Column(d => d.NumarPV, c => c.Index = -1)
            .Column(d => d.DataPV, c => c.Index = -1)
            .Column(d => d.CodCpv, c => c.Index = -1)
            .Column(d => d.Valuta, c => c.Index = -1)
            .Column(d => d.Curs, c => c.Index = -1)
            .Column(d => d.GenereazaPlata, c => c.Index = -1)
            .Column(d => d.PlataContPropriu, c => c.Index = -1)
            .Column(d => d.PlataNumar, c => c.Index = -1)
            .Column(d => d.PlataData, c => c.Index = -1)
            .Column(d => d.PlataTipInstrument, c => c.Index = -1);

        registry.For<FacturaIntrareDetaliu>()
            .Columns(c => c
                .Slot(Rol.Identitate, d => d.Produs, d => d.TipMaterial)
                .Slot(Rol.Pret, d => d.PretUnitar)
                .Slot(Rol.AtributeLot, d => d.DataExpirare, d => d.LotFabricatie)
                .Slot(Rol.Parametri, d => d.CodCpv))
            // Lotul se naște din produs + gestiune (D2); `Valoare` e rezultatul regimului
            // de TVA (D5); `ValoareTva` rămâne culeasă (36a).
            .ReadOnly(d => d.Lot, d => d.Valoare)
            .ListView(nameof(FacturaIntrareDetaliu) + ListView, Culegere)
            .Column(d => d.ValoareReceptie, c => c.Index = -1);
    }

    static void Nir(UiBaselineRegistry registry) {
        registry.For<NirDetaliu>()
            .Columns(c => c
                .Slot(Rol.Identitate, d => d.Produs, d => d.TipMaterial)
                .Slot(Rol.Pret, d => d.PretUnitar)
                .Slot(Rol.AtributeLot, d => d.DataExpirare, d => d.LotFabricatie)
                .Drop(Rol.Tva))                                   // F5-D5
            .ReadOnly(d => d.Lot, d => d.Valoare)                 // F5-D4, F5-D6
            .ListView(nameof(NirDetaliu) + ListView, Culegere);
    }

    static void FacturaIesire(UiBaselineRegistry registry) {
        ListaRoot<FacturaIesire>(registry)
            .Column(d => d.DataScadenta, c => c.Index = 10)
            .Column(d => d.GestiuneDescarcare, c => c.Index = 11);

        registry.For<FacturaIesireDetaliu>()
            .Columns(c => c
                .Slot(Rol.Identitate, d => d.Produs, d => d.TipMaterial)
                .Slot(Rol.Pret, d => d.PretUnitar)
                .Slot(Rol.Parametri, d => d.Descriere))
            .ReadOnly(d => d.Valoare)                             // lotul e pin opțional, se culege (37d)
            .ListView(nameof(FacturaIesireDetaliu) + ListView, Culegere)
            .Column(d => d.ValoareLivrare, c => c.Index = -1);
    }

    // ListView-ul ROOT al unui tip de document: identificarea documentului ÎNTÂI.
    // Generatorul XAF pune coloanele derivatei înaintea celor moștenite și le
    // păstrează indicii, deci apelantul continuă cu ale lui de la 10 în sus;
    // `Total` e [NotMapped] peste `Detalii` (N+1 pe pagină, 35d) — pe DetailView rămâne (D5).
    static ListViewFluent<T> ListaRoot<T>(UiBaselineRegistry registry) where T : Document
        => registry.For<T>()
            .ListView(typeof(T).Name + ListView, _ => { })
            .Column(d => d.Numar, c => c.Index = 0)
            .Column(d => d.Data, c => c.Index = 1)
            .Column(d => d.Predator, c => c.Index = 2)
            .Column(d => d.Primitor, c => c.Index = 3)
            .Column(d => d.Stare, c => c.Index = 4)
            .Column(d => d.Total, c => c.Index = -1);

    // LDI/ASM: cele două direcții culeg lucruri diferite; comutarea câmpurilor e
    // `[Appearance]` pe frunză (F6-D10, F19-D13), nu blocaj aici.
    static void ListaDiferenteInventar(UiBaselineRegistry registry) {
        registry.For<ListaDiferenteInventarDetaliu>()
            .Columns(c => c
                .Slot(Rol.Directie, d => d.Directie)
                .Slot(Rol.Identitate, d => d.Produs, d => d.TipMaterial)
                .Slot(Rol.Pret, d => d.PretEvaluare)
                .Slot(Rol.AtributeLot, d => d.DataExpirare, d => d.LotFabricatie)
                .Drop(Rol.Tva))
            .ReadOnly(d => d.Valoare)
            .ListView(nameof(ListaDiferenteInventarDetaliu) + ListView, Culegere);
    }

    static void Decont(UiBaselineRegistry registry) {
        registry.For<DecontDetaliu>()
            .Columns(c => c
                .Slot(Rol.Identitate, d => d.TipMaterial, d => d.Descriere)
                .Drop(Rol.Unitate)
                .Slot(Rol.Pret, d => d.PretUnitar)
                .Slot(Rol.Conturi, d => d.ContDebit, d => d.ContCredit, d => d.RepartitorDebit, d => d.RepartitorCredit))
            .ReadOnly(d => d.Valoare)
            .ListView(nameof(DecontDetaliu) + ListView, Culegere);
    }

    static void DescarcareGestiune(UiBaselineRegistry registry) {
        registry.For<DescarcareGestiuneDetaliu>()
            .Columns(c => c
                .Slot(Rol.Identitate, d => d.LinieSursa, d => d.TipMaterial)
                .Drop(Rol.Tva))
            .ReadOnly(d => d.Valoare)
            .ListView(nameof(DescarcareGestiuneDetaliu) + ListView, Culegere)
            .Column(d => d.LinieSursa, c => c.Caption = "Linie sursă");
    }

    // Nota contabilă: linia E postarea, deci perechea de conturi o identifică (FAZA 1C §5).
    static void NotaContabila(UiBaselineRegistry registry) {
        registry.For<NotaContabilaDetaliu>()
            .Columns(c => c
                .Slot(Rol.Identitate, d => d.Descriere, d => d.ContDebit, d => d.ContCredit,
                    d => d.RepartitorDebit, d => d.RepartitorCredit)
                .Drop(Rol.Unitate).Drop(Rol.Cantitate).Drop(Rol.Tva))
            .ListView(nameof(NotaContabilaDetaliu) + ListView, Culegere);
    }

    static void Asamblare(UiBaselineRegistry registry) {
        registry.For<AsamblareDetaliu>()
            .Columns(c => c
                .Slot(Rol.Directie, d => d.Directie)
                .Slot(Rol.Identitate, d => d.Produs, d => d.TipMaterial)
                .Slot(Rol.Pret, d => d.PretEvaluare)
                .Slot(Rol.AtributeLot, d => d.DataExpirare, d => d.LotFabricatie)
                .Drop(Rol.Tva))
            .ReadOnly(d => d.Valoare)
            .ListView(nameof(AsamblareDetaliu) + ListView, Culegere);
    }

    // DVI (DVI-D7) folosește detaliul de BAZĂ pe ListView-ul propriu al clasei
    // (`DocumentDetaliu_ListView`), nu grila generică, ca excepțiile ei să nu atingă
    // BCS/BTR/RLF/RDC. `Valoare` e culeasă (valoarea în vamă), nu rezultat.
    static void Dvi(UiBaselineRegistry registry) {
        registry.For<DocumentDetaliu>()
            .ListView(nameof(DocumentDetaliu) + ListView, Culegere)
            .Column(d => d.Valoare, c => { c.Index = 2; c.Caption = "Valoare în vamă"; })
            .Column(d => d.TipTva, c => c.Index = 1)
            .Column(d => d.ValoareTva, c => c.Index = 3)
            // Declarația n-are stoc: cantitatea și lotul n-au semantică pe ea.
            .Column(d => d.Cantitate, c => c.Index = -1)
            .Column(d => d.Lot, c => c.Index = -1)
            // Gazda e chiar DetailView-ul pe care stă grila; `Document` n-are DefaultProperty (85b).
            .Column(d => d.Document, c => c.Index = -1);

        registry.For<DviFactura>().HideForeignKeys();               // DviId/FacturaId
        registry.For<DviFactura>()
            .ListView(nameof(BusinessObjects.Dvi) + "_" + nameof(BusinessObjects.Dvi.Facturi) + ListView, lv => {
                Culegere(lv);
                // Coloanele facturii pe CĂI IMBRICATE: `Document` n-are DefaultProperty (85b),
                // iar selectorul tipizat nu exprimă o cale.
                ColoanaPeCale(lv, "Factura.Numar", 0, "Număr factură");
                ColoanaPeCale(lv, "Factura.Data", 1, "Dată factură");
                ColoanaPeCale(lv, "Factura.Predator", 2, "Furnizor");
                ColoanaPeCale(lv, "Factura.Stare", 3, "Stare factură");
            })
            .Column(f => f.Factura, c => c.Index = -1)
            .Column(f => f.Dvi, c => c.Index = -1);

        // `Total (brut)` ar aduna baza cu taxa — pe declarație cifrele sunt `Baza` și `Taxa`.
        registry.For<BusinessObjects.Dvi>().HideMembers(nameof(Document.Total));

        // Panoul facturilor de import stă NESTED în grupul liniilor (același id ⇒
        // concatenare înăuntru), imediat sub grila declarației.
        registry.For<BusinessObjects.Dvi>()
            .Layout(l => l
                .Group("GrupDetalii", "Detalii", g => g
                    .Group("GrupFacturi", "Facturi de import", f => f
                        .Item(nameof(BusinessObjects.Dvi.Facturi))))
                .Group("GrupStare", "Stare & totaluri", g => g
                    .Item(x => x.Baza)
                    .Item(x => x.Taxa)));
    }

    // Imobilizările (F26-D12); registrul rămâne `Server` implicit, ca celelalte trei (85).
    static void Imobilizari(UiBaselineRegistry registry) {
        registry.For<Imobilizare>().HideForeignKeys();
        registry.For<Imobilizare>()
            .Layout(l => l
                .Group("Antet", null, g => g
                    .Group("GrupIdentificare", "Identificare", d => d
                        .Item(x => x.NumarInventar)
                        .Item(x => x.Denumire))
                    .Group("GrupClasificare", "Clasificare", d => d
                        .Item(x => x.TipMaterial)
                        .Item(x => x.Clasificare))
                    .Group("GrupLoc", "Loc & responsabilitate", d => d
                        .Item(x => x.Loc)
                        .Item(x => x.CentruCost)
                        .Item(x => x.CodEconomic)
                        .Item(x => x.Responsabil))
                    .Group("GrupStareFisa", "Stare", d => d
                        .Item(x => x.Stare)
                        .Item(x => x.DataPunereInFunctiune)
                        .Item(x => x.DataIesire))));

        registry.For<ClasificareImobilizari>()
            .ListView(nameof(ClasificareImobilizari) + ListView)
            .Column(c => c.Cod, c => c.Index = 0)
            .Column(c => c.Denumire, c => c.Index = 1)
            .Column(c => c.DurataMinAni, c => c.Index = 2)
            .Column(c => c.DurataMaxAni, c => c.Index = 3)
            .Column(c => c.Grupa, c => c.Index = 4);

        registry.For<PunereInFunctiuneDetaliu>()
            .HideForeignKeys()
            .Columns(c => c
                .Slot(Rol.Directie, d => d.Fel)
                .Slot(Rol.Identitate, d => d.Imobilizare, d => d.TipMaterial)
                .Slot(Rol.Provenienta, d => d.LinieSursa)
                .Drop(Rol.Unitate).Drop(Rol.Cantitate).Drop(Rol.Tva)
                .Slot(Rol.Valori, d => d.Valoare, d => d.ValoareFiscala)
                .Slot(Rol.Parametri, d => d.Metoda, d => d.DurataLuni, d => d.ValoareReziduala,
                    d => d.MetodaFiscala, d => d.DurataFiscalaLuni, d => d.CategorieFiscala, d => d.UtilizareExclusiva,
                    d => d.AmortizareInitiala, d => d.AmortizareFiscalaInitiala, d => d.LuniAmortizateInitial))
            .ListView(nameof(PunereInFunctiuneDetaliu) + ListView, Culegere)
            .Column(d => d.LinieSursa, c => c.Caption = "Linie sursă");

        registry.For<IesireImobilizareDetaliu>()
            .HideForeignKeys()
            .Columns(c => c
                .Slot(Rol.Directie, d => d.Fel)
                .Slot(Rol.Identitate, d => d.Imobilizare)
                .Drop(Rol.Unitate).Drop(Rol.Cantitate).Drop(Rol.Tva)
                .Slot(Rol.Conturi, d => d.ContDebit, d => d.ContCredit, d => d.RepartitorDebit,
                    d => d.RepartitorCredit, d => d.CodEconomic))
            .ListView(nameof(IesireImobilizareDetaliu) + ListView, ReadOnly);
        registry.For<IesireImobilizare>()
            .Layout(l => l
                .Group("Antet", null, g => g
                    .Group("GrupIesire", "Ieșire", d => d
                        .Item(x => x.Cauza))));

        registry.For<AmortizareLunaraDetaliu>()
            .HideForeignKeys()
            .Columns(c => c
                .Slot(Rol.Identitate, d => d.Imobilizare)
                .Drop(Rol.Unitate).Drop(Rol.Cantitate).Drop(Rol.Tva)
                .Slot(Rol.Valori, d => d.Valoare, d => d.ValoareFiscala, d => d.ValoareDeductibila)
                .Slot(Rol.Conturi, d => d.ContDebit, d => d.ContCredit, d => d.RepartitorDebit,
                    d => d.CentruCost, d => d.CodEconomic)
                .Slot(Rol.Parametri, d => d.Luni))
            .ListView(nameof(AmortizareLunaraDetaliu) + ListView, ReadOnly)
            .Column(d => d.Valoare, c => c.Caption = "Amortizare contabilă")
            .Column(d => d.RepartitorCredit, c => c.Index = -1);

        registry.For<PoliticaAmortizare>().HideForeignKeys();        // TipMaterialId/Cont*Id

        registry.For<RegistruImobilizari>().HideForeignKeys();
        registry.For<RegistruImobilizari>()
            .ListView(nameof(RegistruImobilizari) + ListView)
            .Column(r => r.Data, c => c.Index = 0)
            .Column(r => r.Imobilizare, c => c.Index = 1)
            .Column(r => r.Fel, c => c.Index = 2)
            .Column(r => r.Valoare, c => c.Index = 3)
            .Column(r => r.ValoareFiscala, c => c.Index = 4)
            .Column(r => r.Amortizare, c => c.Index = 5)
            .Column(r => r.AmortizareFiscala, c => c.Index = 6)
            .Column(r => r.AmortizareDeductibila, c => c.Index = 7)
            .Column(r => r.Luni, c => c.Index = 8)
            .Column(r => r.Repartitor, c => c.Index = 9)
            .Column(r => r.Storno, c => c.Index = 10)
            // 85b — `Document`/`DocumentDetaliu` n-au DefaultProperty: ar apărea ca GUID.
            .Column(r => r.Document, c => c.Index = -1)
            .Column(r => r.Detaliu, c => c.Index = -1);
    }
}
