using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Decizia 13: identificare specifică pe lot. Produs = catalogul (fost sumator);
// Lot = fost codmat, creat de linia de intrare, preț unitar fix.

[NavigationItem("Nomenclatoare")]
[XafDefaultProperty(nameof(Denumire))]
public class Produs : BaseObject, ICuCautare {
    public virtual string Cod { get; set; }
    public virtual string Denumire { get; set; }
    // Unitatea de măsură ca TEXT LIBER, cum a fost dintotdeauna. RĂMÂNE lângă
    // FK-ul de mai jos (felia 16, D16-D2) — TECH-DEBT MARCAT, cu prag: se
    // elimină când toate produsele au FK, nu înainte. Motivul e că migrația
    // rezolvă doar ce se poate rezolva fără să ghicească (`UnitatiMasuraRo`),
    // iar restul e text pe care doar un om îl poate traduce; ștergerea coloanei
    // acum ar arunca informația care spune CE trebuie corectat.
    public virtual string UM { get; set; }
    public virtual Guid? TipMaterialId { get; set; }
    public virtual TipMaterial TipMaterial { get; set; }

    // Unitatea de măsură NORMALIZATĂ, din nomenclatorul UN/ECE (felia 16,
    // D16-D2): `UOMBase`/`UOMStandard` din `Product` sunt obligatorii în SAF-T,
    // iar un string liber n-are cum să treacă validarea. Nullable: nomenclatorul
    // existent n-are cum să fie complet din prima, iar produsul fără UM iese în
    // fișier cu `H87` + avertisment agregat, nu blochează raportarea.
    public virtual Guid? UnitateMasuraId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Unitate de măsură (SAF-T)")]
    public virtual UnitateMasura UnitateMasura { get; set; }

    // Codul din Nomenclatorul Combinat (`ProductCommodityCode`, obligatoriu în
    // `Product`) — 8 cifre, exact. Fără nomenclator NC8 seed-uit (9.984 de
    // coduri care se schimbă anual): validarea de fond e a DUK-ului, aici e doar
    // FORMA. Regula permite gol/null — produsul fără NC iese cu `0` și
    // avertisment agregat (D16-D2), nu se refuză la culegere.
    [MaxLength(8)]
    [XafDisplayName("Cod NC")]
    [RuleRegularExpression("Produs_CodNc_8Cifre", DefaultContexts.Save, @"^\d{8}$",
        SkipNullOrEmptyValues = true,
        CustomMessageTemplate = "Codul NC are exact 8 cifre (sau rămâne gol).")]
    public virtual string CodNc { get; set; }

    // F20-D1 — coloana GENERATĂ de căutare fără diacritice; valoarea e a
    // BAZEI de date (vezi `Cautare` / `ICuCautare`), EF n-o scrie niciodată.
    [XafDisplayName("Căutare")]
    [VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
    public virtual string Cautare { get; set; }
}

[NavigationItem("Nomenclatoare")]
[XafDefaultProperty(nameof(Eticheta))]
public class Lot : BaseObject {
    // GATE XAF (D4): identitatea lizibilă a lotului — singurul tip țintă de lookup
    // care nu avea DefaultProperty, deci apărea „Castle.Proxies.LotProxy" pe
    // FCT/FCL/DSC/LDI/ASM (restanța 40d). Cele trei atribute care distING loturile
    // aceluiași produs: proveniența (data) și prețul de intrare (identificarea
    // specifică — decizia 13). NotMapped: nu e stare, e o proiecție de afișare.
    // Navigația Produs se citește lazy, cu guard — pe lookup-uri și grile de
    // nomenclator; DELIBERAT fără AutoInclude (loturile trec prin hot-path-ul
    // pickingului, unde eticheta nu se afișează niciodată).
    //
    // Lotul NĂSCUT LA CULEGERE (25c/26e) n-are încă nici dată, nici preț — le pune
    // motorul la operarea documentului-mamă. Până atunci eticheta ar arăta
    // „01.01.0001 · 0": pe draft se spune explicit că e în curs de culegere.
    //
    // EXCLUS din căutarea full-text (review advers D4): `FilterController` include
    // membrii NEpersistenți în criteriu (`IncludeNonPersistentMembers = true`, mod
    // implicit AllSearchableMembers), iar criteriul ajunge pe colecția EF Core, care
    // nu poate traduce un membru nemapat — orice literă tastată în caseta de
    // căutare a Loturilor sau în lookup-ul de lot (care pe colecții mari PORNEȘTE
    // gol, deci căutarea e singura cale) ar arunca. Numericele nemapate
    // preexistente (Total/ValoareReceptie) cad doar pe text convertibil la număr;
    // Eticheta e primul string nemapat, de aceea lovește la orice text.
    [NotMapped]
    [SearchMemberOptions(SearchMemberMode.Exclude)]
    public string Eticheta {
        get {
            var produs = Produs?.Denumire ?? "(produs nedefinit)";
            return Data == default && PretUnitar == 0
                ? $"{produs} (în culegere)"
                : $"{produs} · {Data:dd.MM.yyyy} · {PretUnitar:0.####}";
        }
    }

    public virtual Guid ProdusId { get; set; }
    public virtual Produs Produs { get; set; }
    // Preț fix la creare = Valoare/Cantitate de pe linia de intrare (testul bazei §3).
    public virtual decimal PretUnitar { get; set; }
    public virtual Guid GestiuneId { get; set; }
    public virtual Gestiune Gestiune { get; set; }
    public virtual DateOnly Data { get; set; }
    public virtual DateOnly? DataExpirare { get; set; }
    public virtual string LotFabricatie { get; set; }
    // Linia care a creat lotul (NIR / plus de inventar / raport de producție).
    // Coloană FĂRĂ constrângere FK (intenționat): linia își referă lotul prin
    // LotId, iar lotul linia-mamă — un FK real pe ambele sensuri ar face ciclu
    // de inserție (EF nu sparge cicluri, iar ObjectSpace-ul XAF comite totul
    // într-un singur SaveChanges). Provenința e întreținută de CreeazaLot/motor.
    public virtual Guid? LinieIntrareId { get; set; }
}
