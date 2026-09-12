using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Decizia 13: identificare specifică pe lot. Produs = catalogul (fost sumator);
// Lot = fost codmat, creat de linia de intrare, preț unitar fix.

[NavigationItem("Nomenclatoare")]
[XafDefaultProperty(nameof(Denumire))]
public class Produs : BaseObject, ICuCautare {
    // 77-r2 — vezi nota de pe `Repartitor.Cod`: regula e a lui `ICuCautare`,
    // atributele sunt prezentarea ei (OpenAPI/client + UI XAF).
    [System.ComponentModel.DataAnnotations.Required]
    [RuleRequiredField("Produs_Cod_Necesar", DefaultContexts.Save,
        CustomMessageTemplate = "Codul este obligatoriu.")]
    public virtual string Cod { get; set; }
    [System.ComponentModel.DataAnnotations.Required]
    [RuleRequiredField("Produs_Denumire_Necesara", DefaultContexts.Save,
        CustomMessageTemplate = "Denumirea este obligatorie.")]
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

    // Implicitul de TVA de SUBIECT (felia 23, F23-D1) — perechea celui de pe
    // `Partener`, cu rol OPUS: aici e purtătorul de COTĂ, nu de regim. Pâinea
    // rămâne 11% de la orice furnizor înregistrat, dar o livrare
    // intracomunitară e scutită indiferent ce produs conține — de-aia
    // `ImpliciteService.TipTva` lasă produsul să-și impună cota DOAR când
    // regimul lui coincide cu cel al partenerului, și cedează altfel.
    public virtual Guid? TipTvaImplicitId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Tip TVA implicit")]
    public virtual TipTva TipTvaImplicit { get; set; }

    // F20-D1 — coloana GENERATĂ de căutare fără diacritice; valoarea e a
    // BAZEI de date (vezi `Cautare` / `ICuCautare`), EF n-o scrie niciodată.
    [XafDisplayName("Căutare")]
    [VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
    public virtual string Cautare { get; set; }
}

[NavigationItem("Nomenclatoare")]
[XafDefaultProperty(nameof(Eticheta))]
public class Lot : BaseObject {
    // 85g — aceeași etichetă în SQL (liste server, lookup, căutare) și în C#.
    [NotMapped]
    [Calculated(ExpresieEticheta)]
    public string Eticheta => EtichetaLot(Produs?.Denumire, Data, PretUnitar);

    public const string ExpresieEticheta =
        "Iif(Data = #0001-01-01# And PretUnitar = 0,"
        + " Concat(IsNull(Produs.Denumire, '(produs nedefinit)'), ' (în culegere)'),"
        + " Concat(IsNull(Produs.Denumire, '(produs nedefinit)'), ' · ',"
        + " Iif(GetDay(Data) < 10, '0', ''), ToStr(GetDay(Data)), '.',"
        + " Iif(GetMonth(Data) < 10, '0', ''), ToStr(GetMonth(Data)), '.', ToStr(GetYear(Data)),"
        + " ' · ', ToStr(Round(PretUnitar, 4))))";

    public static string EtichetaLot(string produs, DateOnly data, decimal pretUnitar) {
        var denumire = produs ?? "(produs nedefinit)";
        return data == default && pretUnitar == 0m
            ? $"{denumire} (în culegere)"
            : $"{denumire} · {data:dd.MM.yyyy} · "
              + Math.Round(pretUnitar, 4, MidpointRounding.AwayFromZero).ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture);
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
