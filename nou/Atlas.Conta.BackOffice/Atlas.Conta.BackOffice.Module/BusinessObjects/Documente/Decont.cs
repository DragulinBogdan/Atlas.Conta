namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// DEC (06): justificarea avansurilor / cheltuielilor unui titular — predator =
// Angajat (titularul), primitor = unitatea internă care primește justificarea.
// Fără reguli de stoc; contarea per linie: debit din contul Tipului (cheltuiala
// aleasă) sau explicit pe linie, credit = contul de avans al titularului (542).
// Lanțul avans → decont → regularizare se leagă prin imperechere (decizia 31d).
public class Decont : Document, IDocumentCuPV {
    public virtual string NumarPV { get; set; }
    public virtual DateOnly? DataPV { get; set; }

    // Creditul (contul de avans 542) se dimensionează pe TITULAR, nu pe
    // primitorul justificării — soldul avansurilor se ține per angajat;
    // convenția 00 §5 (credit←Primitor) rămâne default-ul celorlalte tipuri.
    public override Guid RepartitorImplicitCredit() => PredatorId;

    // Cantitatea e pro-formă (legacy: defaults 'BUC'/'1'); lanțul de valori
    // trăiește pe derivată (testul bazei §3) — capătul se materializează aici.
    public override void PregatesteOperare(DevExpress.ExpressApp.IObjectSpace os) {
        foreach (var d in Detalii.OfType<DecontDetaliu>()) {
            if (d.Cantitate == 0)
                d.Cantitate = 1;
            d.RecalculeazaValoare();
        }
    }

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        if (os.GetObjectByKey<Repartitor>(PredatorId) is not Angajat)
            erori.Add("Predatorul decontului este titularul — un angajat.");
        if (os.GetObjectByKey<Repartitor>(PrimitorId) is not (UnitateInterna or Gestiune))
            erori.Add("Primitorul decontului este unitatea internă care primește justificarea.");
        foreach (var d in Detalii) {
            if (d.Valoare <= 0)
                erori.Add("Fiecare linie de decont poartă o valoare pozitivă (cheltuiala justificată).");
            // Ca la FCT (29b): clasificația bugetară pe linie rămâne hardcodată
            // până la validarea declarativă (3d).
            if (d.AngajamentId == null && d.Dimensiuni.CodEconomicId == null)
                erori.Add("Fiecare linie cere clasificație bugetară: angajament sau cod economic.");
        }
    }
}

// Trăsătura PROPRIE a tipului (06, decizia 15 nuanțată): postarea explicită pe
// linie — cont și repartitor, ambele laturi — ca date de primă clasă, NU ca
// mecanism generic de override. Contractul ILinieCuPostareExplicita e citit de
// motor înaintea rezolvării declarative; câmpurile sunt opționale — nerezolvat
// rămâne pe seama regulii (debit din Tip, credit din titular).
public class DecontDetaliu : DocumentDetaliu, ILinieCuPostareExplicita {
    public virtual string Descriere { get; set; }
    public virtual decimal PretUnitar { get; set; }
    public virtual decimal CotaTva { get; set; }

    public virtual Guid? ContDebitId { get; set; }
    public virtual Cont ContDebit { get; set; }
    public virtual Guid? ContCreditId { get; set; }
    public virtual Cont ContCredit { get; set; }
    public virtual Guid? RepartitorDebitId { get; set; }
    public virtual Repartitor RepartitorDebit { get; set; }
    public virtual Guid? RepartitorCreditId { get; set; }
    public virtual Repartitor RepartitorCredit { get; set; }

    public void RecalculeazaValoare() => Valoare = PretUnitar * Cantitate * (1 + CotaTva / 100m);
}
