using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Validation;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Decizia 16: TPT — moștenire doar unde schema diferă și identitatea e exclusivă;
// calitățile transversale sunt flags, nu clase.
[NavigationItem("Nomenclatoare")]
[XafDefaultProperty(nameof(Denumire))]
public abstract class Repartitor : BaseObject {
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
