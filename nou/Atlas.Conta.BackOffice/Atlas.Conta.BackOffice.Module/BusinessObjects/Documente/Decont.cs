using Atlas.Conta.BackOffice.Module.UI;
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
    public virtual string NumarPV { get; set; }
    public virtual DateOnly? DataPV { get; set; }

    // Creditul (contul de avans 542) se dimensionează pe TITULAR, nu pe
    // primitorul justificării — soldul avansurilor se ține per angajat;
    // convenția 00 §5 (credit←Primitor) rămâne default-ul celorlalte tipuri.
    public override Guid RepartitorImplicitCredit() => PredatorId;

    // Cantitatea e pro-formă (legacy: defaults 'BUC'/'1'); lanțul de valori
    // trăiește pe derivată (testul bazei §3) — capătul se materializează aici,
    // cu TVA-ul din TipTva (P1): bonul cu TVA deductibil justificat pe decont
    // postează 4426 = 542 prin PoliticaTva. `ValoareTva` nenulă culeasă se
    // păstrează (regula 36a uniformizată — decizia 48b): TVA-ul de pe bonul
    // justificat bate rotunjirea noastră, exact ca la FCT.
    public override void PregatesteOperare(DevExpress.ExpressApp.IObjectSpace os) {
        var tipuri = Motor.TvaService.IncarcaTipuri(os, Detalii);
        foreach (var d in Detalii.OfType<DecontDetaliu>()) {
            if (d.Cantitate == 0)
                d.Cantitate = 1;
            Motor.TvaService.CalculeazaValori(d, d.PretUnitar * d.Cantitate, tipuri, pastreazaTvaCules: true);
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
public class DecontDetaliu : DocumentDetaliu, ILinieCuPostareExplicita {
    public virtual string Descriere { get; set; }
    // Cota și regimul vin din TipTva (bază, P1).
    public virtual decimal PretUnitar { get; set; }

    // Postarea explicită pe linie alege din planul mare: match exact pe Simbol.
    public virtual Guid? ContDebitId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    public virtual Cont ContDebit { get; set; }
    public virtual Guid? ContCreditId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    public virtual Cont ContCredit { get; set; }
    public virtual Guid? RepartitorDebitId { get; set; }
    public virtual Repartitor RepartitorDebit { get; set; }
    public virtual Guid? RepartitorCreditId { get; set; }
    public virtual Repartitor RepartitorCredit { get; set; }
}
