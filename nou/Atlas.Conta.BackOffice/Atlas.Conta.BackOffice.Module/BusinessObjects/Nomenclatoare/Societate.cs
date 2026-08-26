using System.ComponentModel.DataAnnotations;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// SOCIETATEA RAPORTOARE (felia 16, D16-D1) — entitatea care lipsea.
//
// DE CE există: SAF-T (D406) nu are „latura liberă". `TransactionLine` și
// `PaymentLine` cer AMBELE `CustomerID` ȘI `SupplierID`; ce nu e partener e
// raportorul, deci raportorul trebuie să aibă un identificator. `Header` cere
// în plus nume, CUI, adresă, contact și `TaxAccountingBasis`. Până azi nimic
// din toate astea nu exista în model: baza știa cine sunt PARTENERII, nu cine
// e ea însăși.
//
// DE CE nomenclator EDITABIL, și nu `SetareProfil`: modelul e bază-per-client
// (35d), iar antetul societății e DATE ale clientului (se mută sediul, se
// schimbă persoana de contact, se deschide alt cont bancar), nu o alegere
// înghețată la seed-uire. `SetareProfil` rămâne exact ce e (profil + convenția
// de rotunjire, 52a) — două lucruri cu perioade de viață diferite nu se pun în
// același rând.
//
// DE CE UN SINGUR RÂND: „societatea care raportează" e unică prin definiție —
// un al doilea rând ar face `IdSocietate` ambiguu, iar fișierul ar fi semnat
// cu un CUI ales la întâmplare. Unicitatea NU se poate exprima ca index (nu
// există „unic pe nimic"), deci trăiește în `GardianEditare.VerificaSocietate`,
// pe ambele uși secured (Blazor + OData) — al doilea POST cade acolo, cu mesaj
// de domeniu. Seed-ul creează rândul GOL dacă lipsește și NU-l rescrie
// niciodată (riscul 12 al contractului: `--forceUpdate` pe o bază cu societatea
// completată nu are voie s-o golească).
//
// DE CE adresa e aceeași cu a partenerului, câmp cu câmp: iese prin ACELAȘI
// `AddressStructure` din schemă. Lungimile sunt cele din `AdresaSaft.Lungimi`
// (reflecție pe `Partener`), iar D16-V1 probează egalitatea — o coloană lărgită
// aici și nu acolo ar produce un fișier respins la validare.
[NavigationItem("Configurare")]
[XafDefaultProperty(nameof(Denumire))]
public class Societate : BaseObject {
    // Numele raportorului din `Header/Company/Name`. 70 = `Name` din
    // `CompanyStructure` (aceeași lungime ca `Partener.Denumire` n-ar fi
    // adevărată: acolo n-avem `[MaxLength]`, aici o cere schema).
    [MaxLength(70)]
    public virtual string Denumire { get; set; }

    // CUI-ul, în forma NORMALIZATĂ (fără prefixul `RO`) — ca la partener (71b).
    // Prefixul se pune la GENERARE, unde regula îl cere: `RegistrationNumber`-ul
    // societății e `RO`+CUI când e înregistrată în scopuri de TVA, iar
    // `IdSocietate` e `00`+CUI întotdeauna. Două reguli diferite peste aceeași
    // cifră ⇒ cifra se stochează o dată, curată.
    [MaxLength(20)]
    [XafDisplayName("Cod fiscal")]
    public virtual string CodFiscal { get; set; }

    [XafDisplayName("Înregistrat în scopuri de TVA")]
    public virtual bool InregistratTva { get; set; }

    [MaxLength(20)]
    [XafDisplayName("Nr. registrul comerțului")]
    public virtual string RegistruComert { get; set; }

    // Cod ISO 3166-1 alpha-2, normalizat la scriere EXACT ca la `Partener`
    // (trim + majuscule, gol ⇒ RO): aceeași grafie, aceeași regulă, același
    // gardian. Practic e mereu RO (raportorul e rezident — nerezidenții sunt
    // explicit în afara feliei), dar câmpul există fiindcă `AddressStructure`
    // cere `Country` și fiindcă „RO" hardcodat în generator ar fi exact genul de
    // constantă pe care nimeni n-o mai găsește când apare primul caz contrar.
    [XafDisplayName("Țară")]
    [MaxLength(2)]
    [RuleRegularExpression("Societate_Tara_Iso2", DefaultContexts.Save, "^[A-Z]{2}$",
        CustomMessageTemplate = "Țara se scrie ca un cod ISO de două litere (RO, DE, US).")]
    public virtual string Tara {
        get => tara;
        set => tara = Partener.NormalizeazaTara(value);
    }
    string tara = "RO";

    // ---- Adresa (`AddressStructure`), aceleași 6 câmpuri și lungimi ca `Partener` ----

    [XafDisplayName("Stradă")]
    [MaxLength(70)]
    public virtual string Strada { get; set; }

    [XafDisplayName("Număr")]
    [MaxLength(18)]
    public virtual string Numar { get; set; }

    [XafDisplayName("Bloc, scară, etaj, ap.")]
    [MaxLength(70)]
    public virtual string DetaliiAdresa { get; set; }

    [XafDisplayName("Localitate")]
    [MaxLength(35)]
    public virtual string Localitate { get; set; }

    [XafDisplayName("Cod poștal")]
    [MaxLength(18)]
    public virtual string CodPostal { get; set; }

    // `Region` = ISO 3166-2, DOAR pe adresele din România (schema validează
    // valoarea contra listei RO). Gardul pereche e în `GardianEditare`, ca la
    // partener: FK-ul nu poate purta o regulă de coerență între proprietăți (40b).
    // Aici are un consumator în plus: `AuditFileRegion` din `Header` E chiar
    // județul societății.
    public virtual Guid? JudetId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Județ")]
    public virtual Judet Judet { get; set; }

    // ---- Contactul (`Header/Company/Contact`) ----
    // 35 = `Contact*Name` din `ContactInformationStructure`; `Telephone` 20 în
    // schemă, dar 35 e lungimea pe care o folosește `AddressStructure` pentru
    // câmpurile scurte, iar tăierea la generare rămâne oricum a scriitorului.
    // `Email` 70 = `Email` din aceeași structură.
    [MaxLength(35)]
    [XafDisplayName("Contact — nume")]
    public virtual string ContactNume { get; set; }

    [MaxLength(35)]
    [XafDisplayName("Contact — prenume")]
    public virtual string ContactPrenume { get; set; }

    [MaxLength(35)]
    public virtual string Telefon { get; set; }

    [MaxLength(70)]
    public virtual string Email { get; set; }

    // Contul bancar propriu care apare în `Header/Company/BankAccount`. FK către
    // `ContPropriu` (care poartă deja IBAN-ul și `EsteBanca`, decizia 31a) — NU
    // un IBAN copiat aici: conturile proprii sunt deja nomenclator, iar o a doua
    // copie a IBAN-ului s-ar desincroniza tăcut. Lookup-ul oferă doar băncile
    // (casieria n-are IBAN); e AFORDANȚĂ, nu validare (65).
    public virtual Guid? ContBancarId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [DataSourceCriteria("EsteBanca = True")]
    [XafDisplayName("Cont bancar")]
    public virtual ContPropriu ContBancar { get; set; }

    // `TaxAccountingBasis` din `Header`: care dintre cele 12 planuri de conturi
    // ale ANAF stă la baza raportării. Validatorul DUK verifică `AccountID`-urile
    // contra planului ales, deci valoarea nu e decorativă. String, nu enum:
    // valorile sunt ale ANAF (se pot adăuga la un ordin nou), iar lista admisă e
    // `BazeContabile` — verificată în gardian, nu în tipul C#.
    //
    // Default `A` = „Contabilitate financiară" (OMFP 1802), planul profilului
    // privat. Profilul bugetar N-ARE bază validă (planul instituțiilor publice
    // nu e printre cele 12) — SAF-T îi e neaplicabil (D16-D5), nu prost
    // configurat.
    [MaxLength(18)]
    [XafDisplayName("Bază contabilă (SAF-T)")]
    public virtual string BazaContabila { get; set; } = BazaContabilaImplicita;

    // Raportăm CNP-ul persoanelor fizice în identificatorul de partener?
    // `false` (default) ⇒ PF cu CNP iese cu `04`+cod intern, nu `03`+CNP.
    // E o decizie de CONFIDENȚIALITATE, luată la fel în aplicația legacy: CNP-ul
    // e dat cu caracter personal, iar declarația nu-l cere obligatoriu.
    // Politică a BAZEI (deci câmp), nu constantă în generator.
    [XafDisplayName("Raportează CNP-ul persoanelor fizice")]
    public virtual bool RaporteazaCnp { get; set; }

    // Cele 12 valori admise de `TaxAccountingBasis` (xlsx 16.02.2026, foaia
    // „1. Header", H.2 — DESCRIEREA). Publică: gardianul o verifică, D16-V1 o
    // probează, iar generatorul o va scrie ca atare în fișier.
    //
    //   A         — contabilitatea angajamentelor, partidă dublă + planul general
    //               (OMFP 1802) — exact planul profilului privat
    //   I         — Invoice Accounting: nerezidenți / contribuabilii cu decont
    //               SPECIAL de TVA
    //   IFRS      — partidă dublă + OMFP 2844/2016 (planul IFRS)
    //   BANK      — instituții de credit (planul pentru bănci)
    //   INSURANCE — societăți de asigurări
    //   NORMA39   — leasing/investiții financiare, IFRS pe Norma ASF 39/2015
    //   IFN       — instituții financiare nebancare (Reg. BNR 17/2015)
    //   NORMA36   — brokeraj de asigurări/reasigurări (Norma ASF 36/2015)
    //   NORMA14   — pensii private (Norma ASF 14/2015)
    //   ONG       — persoane juridice fără scop patrimonial (OMFP 3103/2017)
    //   ONGE      — ONG cu cod de TVA pentru activitate economică
    //   BNR6      — instituții de plată, societăți financiare nebancare (Ordin BNR 6)
    //
    // Constatare (nu decizie): coloana „Reguli de validare sintactică" a
    // aceluiași rând H.2 enumeră doar OPT valori (fără NORMA14/ONG/ONGE/BNR6) —
    // o contradicție internă a xlsx-ului. Lista de aici e cea din DESCRIERE, cea
    // completă; dacă DUK respinge vreuna, se măsoară la V3 și se taie atunci.
    public static readonly IReadOnlyList<string> BazeContabile = [
        "A", "I", "IFRS", "BANK", "INSURANCE", "NORMA39",
        "IFN", "NORMA36", "NORMA14", "ONG", "ONGE", "BNR6",
    ];

    public const string BazaContabilaImplicita = "A";
}
