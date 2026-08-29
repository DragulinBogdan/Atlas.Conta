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
public class Decont : Document, IDocumentCuPV {
    // Rolul de STINS (F19-D16): decontul lasă un sold CREDITOR pe contul
    // titularului (cheltuiala lui, pe care i-o datorăm) — se stinge debitând,
    // adică exact cu plata/avansul către angajat (lanțul probat la 32b).
    public override SensStingere? SensDeStins(DevExpress.ExpressApp.IObjectSpace os) =>
        SensStingere.Datorie;

    [XafDisplayName("Număr PV")]
    public virtual string NumarPV { get; set; }
    [XafDisplayName("Dată PV")]
    public virtual DateOnly? DataPV { get; set; }

    // Creditul (contul de avans 542) se dimensionează pe TITULAR, nu pe
    // primitorul justificării — soldul avansurilor se ține per angajat;
    // convenția 00 §5 (credit←Primitor) rămâne default-ul celorlalte tipuri.
    public override Guid RepartitorImplicitCredit(DevExpress.ExpressApp.IObjectSpace os) => PredatorId;

    // Cantitatea e pro-formă (legacy: defaults 'BUC'/'1'); lanțul de valori
    // trăiește pe derivată (testul bazei §3) — capătul se materializează aici,
    // cu TVA-ul din TipTva (P1): bonul cu TVA deductibil justificat pe decont
    // postează 4426 = 542 prin PoliticaTva. `ValoareTva` nenulă culeasă se
    // păstrează (regula 36a uniformizată — decizia 48b): TVA-ul de pe bonul
    // justificat bate rotunjirea noastră, exact ca la FCT.
    public override void PregatesteOperare(DevExpress.ExpressApp.IObjectSpace os) {
        var tipuri = Motor.TvaService.IncarcaTipuri(os, Detalii);
        // Latura fiscală a tipului (F13-D1) — o dată per document, nu per linie.
        var directie = Motor.TvaService.DirectiePentru(os, this);
        foreach (var d in Detalii.OfType<DecontDetaliu>()) {
            if (d.Cantitate == 0)
                d.Cantitate = 1;
            Motor.TvaService.CalculeazaValori(d, d.PretUnitar * d.Cantitate, tipuri, directie, pastreazaTvaCules: true);
        }
    }

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        if (os.GetObjectByKey<Repartitor>(PredatorId) is not Angajat)
            erori.Add("Predatorul decontului este titularul — un angajat.");
        if (os.GetObjectByKey<Repartitor>(PrimitorId) is not (UnitateInterna or Gestiune))
            erori.Add("Primitorul decontului este unitatea internă care primește justificarea.");
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
// F8-D2: aderarea la `ILinieCuPretUnitar` e PURĂ DECLARAȚIE — `PretUnitar` există
// pe clasă din 3a, deci nicio coloană și nicio migrație. Consecința e seam-ul
// comun de calcul la culegere (`TvaService.CalculeazaLaCulegere`): pe tierul API
// îl apelează `DecontApply`, iar în XAF nimic nu se schimbă — controllerul de
// recalcul e gate-uit pe TIPUL DOCUMENTULUI
// (`RecalculCulegere.TipCuPretUnitarCules` = FCT + FCL, ecranele gate-ului), nu
// pe interfața liniei.
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
