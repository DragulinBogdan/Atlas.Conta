using System.ComponentModel.DataAnnotations;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Validation;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Decizia 16: TPT — moștenire doar unde schema diferă și identitatea e exclusivă;
// calitățile transversale sunt flags, nu clase.
[NavigationItem("Nomenclatoare")]
[XafDefaultProperty(nameof(Denumire))]
public abstract class Repartitor : BaseObject, ICuCautare {
    public virtual string Cod { get; set; }
    public virtual string Denumire { get; set; }
    public virtual CalitateRepartitor Calitati { get; set; }
    public virtual bool Activ { get; set; } = true;
    // Tranșarea (d) din testul bazei, lărgită la decizia 31: contul purtat de
    // repartitor intră în rezolvarea declarativă a regulilor de contare
    // (SursaCont.Repartitor*) pentru ORICE latură — partener 401/404/411,
    // cont propriu 5xx/770, angajat 542 (avansuri) — deci stă pe bază.
    public virtual Guid? ContImplicitId { get; set; }
    // Plan de conturi mare — lookup standard (SmartLookup revertat,
    // decizia 40d/gate).
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    public virtual Cont ContImplicit { get; set; }

    // F20-D1 — coloana GENERATĂ de căutare fără diacritice; valoarea e a
    // BAZEI de date (vezi `Cautare` / `ICuCautare`), EF n-o scrie niciodată.
    [XafDisplayName("Căutare")]
    [VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
    public virtual string Cautare { get; set; }
}

// Furnizor/client = rol contextual dat de poziția pe document (decizia 16).
public class Partener : Repartitor {
    public virtual string CodFiscal { get; set; }
    public virtual string RegistruComert { get; set; }

    // Identitatea FISCALĂ a partenerului (felia 14, D4-D1): exact cele patru
    // date pe care D394 le cere ca să clasifice rândul (tip_partener 1–4, AI) —
    // satelitul 34g (adrese/IBAN/delegați) NU se deschide aici. Stau pe frunză,
    // nu pe `Repartitor`: calitățile fiscale sunt ale partenerului, nu ale
    // gestiunii sau ale angajatului.
    [XafDisplayName("Tip persoană")]
    public virtual TipPersoana TipPersoana { get; set; } = TipPersoana.Juridica;

    // Cod ISO 3166-1 alpha-2, liber (nomenclatorul de țări vine cu adresa —
    // 34g), cu validare de FORMAT. Se normalizează la scriere (trim +
    // majuscule; gol ⇒ RO) ca aceeași țară să nu apară în două grafii — cheia
    // de clasificare a rândului D394 e valoarea asta. Regula XAF apără
    // culegerea din Blazor; pe API (unde validarea XAF nu rulează — 55b) o
    // apără `GardianEditare`.
    [XafDisplayName("Țară")]
    [FieldSize(2)]
    [RuleRegularExpression("Partener_Tara_Iso2", DefaultContexts.Save, "^[A-Z]{2}$",
        CustomMessageTemplate = "Țara se scrie ca un cod ISO de două litere (RO, DE, US).")]
    public virtual string Tara {
        get => tara;
        set => tara = NormalizeazaTara(value);
    }
    string tara = "RO";

    public static string NormalizeazaTara(string valoare) {
        var cod = valoare?.Trim().ToUpperInvariant();
        return string.IsNullOrEmpty(cod) ? "RO" : cod;
    }

    // „În România" e parte din semantică (fix 3 al review-ului advers): un
    // partener din DE înregistrat în DE NU se bifează aici; un DE cu cod RO
    // (înregistrare directă / reprezentant fiscal, art. 316) DA — și atunci e
    // tip 1 în D394, cu prefixul RO tăiat. „Înregistrat bate tot" în
    // `D394Proiectii.TipPartener`: PFA/II înregistrate sunt tot tip 1.
    [XafDisplayName("Înregistrat în scopuri de TVA în România")]
    public virtual bool InregistratTva { get; set; }

    // Furnizor în sistemul TVA la încasare: în D394, `A` devine `AI`. Doar
    // FLAG-ul (D4-D1) — mecanismul 4428/exigibilitatea (36f) nu intră.
    [XafDisplayName("TVA la încasare")]
    public virtual bool TvaLaIncasare { get; set; }

    // ADRESA STRUCTURATĂ (felia 15, D15-D1) — amendamentul la 71b: satelitul
    // 34g se deschide EXACT cât cere adresa, restul (contact, IBAN, delegați,
    // adrese multiple) rămâne acolo. Coloane PLATE pe frunză, toate nullable:
    // forma owned a murit (54c), iar `ModelCheck` creează `Partener` în 16
    // scene — niciun câmp de adresă nu poate fi obligatoriu în model.
    //
    // Lungimile sunt ale lui `AddressStructure` din SAF-T (xlsx 16.02.2026,
    // foaia „5. Structures"), nu alese de noi: peste ele fișierul e respins la
    // validare, deci tăierea se face la SCRIERE, cu avertisment (D15-D3), nu la
    // generare. `[MaxLength]` (DataAnnotations), NU `[FieldSize]`: doar prima
    // produce `character varying(n)` în migrația EF (constatare din F14).
    [XafDisplayName("Stradă")]
    [MaxLength(70)]
    public virtual string Strada { get; set; }

    // Numărul poștal e TEXT: „12A", „bis", „1-3" — nu un întreg.
    [XafDisplayName("Număr")]
    [MaxLength(18)]
    public virtual string Numar { get; set; }

    // `AdditionalAddressDetail` + `Building` pliate într-un singur câmp:
    // SAF-T le are separate, dar nicio sursă a noastră (ANAF `*detalii_Adresa`,
    // 1C `Field8`) nu le desparte, iar o coloană pe care n-o umple nimeni e o
    // coloană care minte.
    [XafDisplayName("Bloc, scară, etaj, ap.")]
    [MaxLength(70)]
    public virtual string DetaliiAdresa { get; set; }

    // `City` — OBLIGATORIU în SAF-T (împreună cu `Country`). Obligatoriu în
    // FIȘIER, nu în model: lipsa lui e o gaură raportată de felia SAF-T, nu un
    // refuz la culegerea unui partener.
    [XafDisplayName("Localitate")]
    [MaxLength(35)]
    public virtual string Localitate { get; set; }

    [XafDisplayName("Cod poștal")]
    [MaxLength(18)]
    public virtual string CodPostal { get; set; }

    // `Region` = codul ISO 3166-2, DOAR pentru adresele din România (schema
    // SAF-T validează valoarea contra listei RO). Gardul pereche
    // (`GardianEditare.VerificaPartener`) refuză județul pe o țară ≠ RO;
    // FK-ul nu poate purta regula XAF (40b).
    public virtual Guid? JudetId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Județ")]
    public virtual Judet Judet { get; set; }

    // Timbrul sincronizării din registrul ANAF — SERVER-OWNED (55a): îl scrie
    // doar serviciul, pe ușa non-secured (58c); `GardianEditare` îl refuză pe
    // cea secured, ca pe `Autogenerat`. Fără el, „datele astea de unde vin?" nu
    // are răspuns, iar `suprascrie` devine ireversibil fără urmă.
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Sincronizat ANAF la")]
    public virtual DateTime? DataSincronizareAnaf { get; set; }

    // `stare_inactiv.statusInactivi` din răspunsul ANAF. DOAR evidență: motorul
    // nu-l consultă și nicio postare nu depinde de el (consecințele fiscale ale
    // inactivării sunt o decizie a contabilului, nu a motorului). SERVER-OWNED
    // ca timbrul: `GardianEditare` îl refuză pe ușa secured (review F2).
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Inactiv fiscal (ANAF)")]
    public virtual bool InactivFiscal { get; set; }
}

public class Angajat : Repartitor {
    public virtual string Marca { get; set; }
}

public class Gestiune : Repartitor {
}

public class UnitateInterna : Repartitor {
}

// Casele și conturile proprii (legacy `casierie`) — decizia din 09 §3.
public class ContPropriu : Repartitor {
    public virtual string Iban { get; set; }
    public virtual bool EsteBanca { get; set; }
}
