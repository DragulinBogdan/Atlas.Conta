using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Decizia 17 + 09/31: plățile/încasările sunt tipuri de document; BREGISTRU
// dispare — registrul de casă/bancă e registrul contabil al acestor documente.
// Laturile: ContPropriu ↔ Partener/Angajat; liniile sunt DEFALCAREA sumei
// (echivalentul BREG_P): Valoare culeasă direct + Dimensiuni, fără stoc/lot.
// Tipul liniei: tehnic „TRZ" la culegere manuală; liniile autogenerate din
// factură păstrează Tipul liniei sursă (poartă informația de defalcare).
public abstract class DocumentTrezorerie : Document {
    public virtual TipInstrumentPlata TipInstrument { get; set; }
    public virtual string NumarExtras { get; set; }
    public virtual DateOnly? DataExtras { get; set; }

    // Contrapartida (latura care NU e contul propriu) — cheia invariantului de
    // imperechere: plata stinge doar documente pe care apare același repartitor.
    // Metodă, nu proprietate: nu e stare, iar XAF n-are ce căuta pe ea (XAF0033).
    public abstract Guid GetContrapartidaId();

    // Trezoreria e stingătorul „clasic" (31d): o SINGURĂ contrapartidă (latura
    // non-ContPropriu), cu plafonul = totalul brut al plății/încasării. Cum
    // toate stingerile ei merg către aceeași contrapartidă, plafonul per
    // contrapartidă e identic cu plafonul global de dinainte — comportamentul
    // trezoreriei nu se schimbă.
    public override IReadOnlyDictionary<Guid, decimal> CapacitateStingere(DevExpress.ExpressApp.IObjectSpace os) =>
        new Dictionary<Guid, decimal> { [GetContrapartidaId()] = Motor.ImperechereService.Total(os, ID) };

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        foreach (var d in Detalii)
            if (d.Valoare <= 0)
                erori.Add("Fiecare linie poartă o valoare pozitivă (defalcarea sumei plătite/încasate).");
    }
}

// Predator = ContPropriu (sursa banilor), primitor = beneficiarul.
public class Plata : DocumentTrezorerie {
    public override Guid GetContrapartidaId() => PrimitorId;

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        if (os.GetObjectByKey<Repartitor>(PredatorId) is not ContPropriu)
            erori.Add("Predatorul plății este contul propriu (casă/bancă) din care se plătește.");
        if (os.GetObjectByKey<Repartitor>(PrimitorId) is not (Partener or Angajat))
            erori.Add("Primitorul plății este beneficiarul — un partener sau un angajat (avans).");
    }
}

// Predator = plătitorul, primitor = ContPropriu (destinația banilor).
public class Incasare : DocumentTrezorerie {
    public override Guid GetContrapartidaId() => PredatorId;

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        if (os.GetObjectByKey<Repartitor>(PredatorId) is not (Partener or Angajat))
            erori.Add("Predatorul încasării este plătitorul — un partener sau un angajat.");
        if (os.GetObjectByKey<Repartitor>(PrimitorId) is not ContPropriu)
            erori.Add("Primitorul încasării este contul propriu (casă/bancă) în care se încasează.");
    }
}

// Decizia 17: stingerea — m2m stingător↔document cu sume parțiale
// (GEST_DECONTARI). Nu e document (fără ciclu Draft/Operat): un rând e o
// legătură între două documente DEJA operate. Invarianții
// (motor/ImperechereService): ambele Operat, Σ imperecheri ≤ totalul
// documentului stins, contrapartida stingătorului apare pe el; `ramas` e
// calcul, nu coloană. Autogenerat = creată de motor la operarea unei plăți
// autogenerate (plata automată din factură — 00 §7).
//
// `DocumentStingator` e tipat `Document`, nu `DocumentTrezorerie` (decizia 48b
// — compensarea): rolul de stingător e declarat POLIMORF, prin
// `Document.CapacitateStingere`, iar azi îl poartă trezoreria și nota
// contabilă. Tipul FK-ului nu mai face filtrarea — o face validarea.
[NavigationItem("Documente")]
public class Imperechere : BaseObject {
    public virtual Guid DocumentStingatorId { get; set; }
    public virtual Document DocumentStingator { get; set; }
    public virtual Guid DocumentId { get; set; }
    public virtual Document Document { get; set; }
    public virtual decimal Suma { get; set; }
    // Marcaj de proveniență (creată de motor la plata autogenerată) — nu se culege
    // de operator, deci read-only în UI.
    [ModelDefault("AllowEdit", "False")]
    public virtual bool Autogenerat { get; set; }
}
