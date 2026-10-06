using Atlas.Conta.BackOffice.Module.UI;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// DEC (06): justificarea avansurilor / cheltuielilor unui titular — predator =
// Angajat (titularul), primitor = unitatea internă care primește justificarea.
// Fără reguli de stoc; contarea per linie: debit din contul Tipului (cheltuiala
// aleasă) sau explicit pe linie, credit = contul de avans al titularului (542).
// Lanțul avans → decont → regularizare se leagă prin imperechere (decizia 31d).
[TipDetaliu(typeof(DecontDetaliu))]
public class Decont : Document, IDocumentCuPV, IDocumentFiscalPrimit {
    [DevExpress.ExpressApp.DC.XafDisplayName("Exigibilitate TVA")]
    public virtual DateOnly? DataExigibilitate { get; set; }
    [DevExpress.ExpressApp.DC.XafDisplayName("Data primirii")]
    public virtual DateOnly? DataPrimire { get; set; }

    public override Declaratii.IDeclarant Declarant() => Declaratii.DeclarantDecont.Instanta;
    public override Declaratii.ContractLaturi Laturi() =>
        new(Declaratii.Latura.Externa, Declaratii.Latura.Interna);

    // Rolul de STINS (F19-D16): decontul lasă un sold CREDITOR pe contul
    // titularului (cheltuiala lui, pe care i-o datorăm) — se stinge debitând,
    // adică exact cu plata/avansul către angajat (lanțul probat la 32b).
    public override SensStingere? SensDeStins(DevExpress.ExpressApp.IObjectSpace os) =>
        SensStingere.Datorie;

    [XafDisplayName("Număr PV")]
    public virtual string NumarPV { get; set; }
    [XafDisplayName("Dată PV")]
    public virtual DateOnly? DataPV { get; set; }

    public override void PregatesteOperare(DevExpress.ExpressApp.IObjectSpace os) =>
        CalculeazaValori(os, Detalii, pastreazaTvaCules: true);

    public override bool CuTva() => true;
    public override IReadOnlySet<string> IntrariBaza() => intrariBaza;
    static readonly IReadOnlySet<string> intrariBaza = IntrariBazaCu(nameof(DecontDetaliu.PretUnitar));

    // Cantitatea e pro-formă: lipsa ei înseamnă o bucată.
    public override decimal? BazaLinie(DevExpress.ExpressApp.IObjectSpace os, DocumentDetaliu linie) =>
        linie is DecontDetaliu d ? d.PretUnitar * (d.Cantitate == 0 ? 1 : d.Cantitate) : null;

    protected override void CalculeazaLinie(DocumentDetaliu linie, decimal baza, Motor.ContextTva tva, bool pastreazaTvaCules) {
        if (linie.Cantitate == 0)
            linie.Cantitate = 1;
        base.CalculeazaLinie(linie, baza, tva, pastreazaTvaCules);
    }

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        // Clasificația bugetară per linie a migrat în PoliticaValidare (32d →
        // 3d): regulă de profil, aplicată de motor înaintea acestui hook.
        foreach (var d in Detalii)
            if (d.Valoare <= 0)
                erori.Add("Fiecare linie de decont poartă o valoare pozitivă (cheltuiala justificată).");
    }
}

// Trăsătura PROPRIE a tipului (06, decizia 15 nuanțată): postarea explicită pe
// linie — cont și repartitor, ambele laturi — ca date de primă clasă, NU ca
// mecanism generic de override. Contractul ILinieCuPostareExplicita e citit de
// motor înaintea rezolvării declarative; câmpurile sunt opționale — nerezolvat
// rămâne pe seama regulii (debit din Tip, credit din titular).
// F8-D2: aderarea la `ILinieCuPretUnitar` e pură declarație (fără coloană nouă).
public class DecontDetaliu : DocumentDetaliu, ILinieCuPostareExplicita, ILinieCuPretUnitar {
    public virtual string Descriere { get; set; }
    // Cota și regimul vin din TipTva (bază, P1).
    [XafDisplayName("Preț unitar")]
    public virtual decimal PretUnitar { get; set; }

    // Postarea explicită pe linie alege din planul mare (nomenclator mare —
    // lookup standard; SmartLookup revertat, decizia 40d/gate).
    //
    // NOTĂ (review F8): `ContDebit == ContCredit` pe aceeași linie postează un
    // rând X = X — o notă nulă, care nu mișcă niciun sold. NU se refuză, din
    // aceeași rațiune ca la contul SUMATOR ales explicit (F8-D14): postarea
    // explicită e trăsătura tipului, iar operatorul care alege un cont anume îl
    // primește; ce e afordanță (ce se OFERĂ în lookup) se rafinează în client,
    // nu se transformă în interdicție de motor. Dacă vreodată devine cerință,
    // locul e `ValideazaOperare`, nu culegerea.
    public virtual Guid? ContDebitId { get; set; }
    [XafDisplayName("Cont debit")]
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    public virtual Cont ContDebit { get; set; }
    public virtual Guid? ContCreditId { get; set; }
    [XafDisplayName("Cont credit")]
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    public virtual Cont ContCredit { get; set; }
    public virtual Guid? RepartitorDebitId { get; set; }
    [XafDisplayName("Repartitor debit")]
    public virtual Repartitor RepartitorDebit { get; set; }
    public virtual Guid? RepartitorCreditId { get; set; }
    [XafDisplayName("Repartitor credit")]
    public virtual Repartitor RepartitorCredit { get; set; }

    // DIM-2 (decizia 54c, inventar §2): clasificația economică a cheltuielii
    // justificate (politica de tip cere angajament SAU cod economic).
    public virtual Guid? CodEconomicId { get; set; }
    [XafDisplayName("Cod economic")]
    public virtual CodEconomic CodEconomic { get; set; }

    public override Dimensiuni DimensiuniCulese() => new() { CodEconomicId = CodEconomicId };
    public override void PreiaDimensiuni(Dimensiuni s) => CodEconomicId = s.CodEconomicId;
}
