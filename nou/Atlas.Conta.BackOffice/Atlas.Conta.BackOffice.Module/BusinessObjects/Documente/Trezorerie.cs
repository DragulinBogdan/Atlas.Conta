using Atlas.Conta.BackOffice.Module.UI;
using DevExpress.ExpressApp.DC;
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
    [XafDisplayName("Instrument de plată")]
    public virtual TipInstrumentPlata TipInstrument { get; set; }
    [XafDisplayName("Număr extras")]
    public virtual string NumarExtras { get; set; }
    [XafDisplayName("Dată extras")]
    public virtual DateOnly? DataExtras { get; set; }

    // Contrapartida (latura care NU e contul propriu) — cheia invariantului de
    // imperechere: plata stinge doar documente pe care apare același repartitor.
    // Metodă, nu proprietate: nu e stare, iar XAF n-are ce căuta pe ea (XAF0033).
    public abstract Guid GetContrapartidaId();

    // Piciorul PROPRIU al documentului (latura care NU e contrapartidă):
    // predatorul la Plata, primitorul la Incasare. Derivat din contrapartidă,
    // ca să existe o singură declarație de direcție per tip.
    public Guid GetContPropriuId() => PredatorId == GetContrapartidaId() ? PrimitorId : PredatorId;

    // Viramentul intern (F7-D1, transferul 581): contrapartida NU e un
    // partener/angajat, ci al DOILEA cont propriu. Discriminarea e a datelor
    // (tipul repartitorului de pe latură), nu a unui tip de document nou —
    // header, linii și ciclu de viață sunt identice cu ale unei plăți normale.
    //
    // Se cer AMBELE laturi conturi proprii, nu doar contrapartida: un draft cu
    // laturile inversate (plată de la un partener CĂTRE casă) are contrapartida
    // ContPropriu fără să fie virament, iar predicatul e consumat de căi care
    // rulează ÎNAINTE de validare (affordance-ul din API, dimensiunile notei) —
    // „gardul care tace devine capcană" (lecțiile F5/F6).
    public bool EsteVirament(DevExpress.ExpressApp.IObjectSpace os) =>
        os.GetObjectByKey<Repartitor>(PredatorId) is ContPropriu
        && os.GetObjectByKey<Repartitor>(PrimitorId) is ContPropriu;

    // Trezoreria e stingătorul „clasic" (31d): o SINGURĂ contrapartidă (latura
    // non-ContPropriu), cu plafonul = totalul brut al plății/încasării. Cum
    // toate stingerile ei merg către aceeași contrapartidă, plafonul per
    // contrapartidă e identic cu plafonul global de dinainte — comportamentul
    // trezoreriei nu se schimbă.
    //
    // Viramentul NU stinge și nu poate fi stins (F7-D5): banii nu părăsesc
    // patrimoniul, deci nicio datorie/creanță nu se închide, iar contrapartida
    // unei plăți normale (partener/angajat) nu apare niciodată pe laturile lui.
    // `null` = „tipul nu stinge nimic", exact contractul bazei — de aici motorul
    // știe să nu creeze imperecherea automată a laturii pereche.
    public override IReadOnlyDictionary<Guid, decimal> CapacitateStingere(DevExpress.ExpressApp.IObjectSpace os) =>
        EsteVirament(os)
            ? null
            : new Dictionary<Guid, decimal> { [GetContrapartidaId()] = Motor.ImperechereService.Total(os, ID) };

    // Cealaltă jumătate a lui F7-D5, IMPUSĂ nu doar afirmată: viramentul nu
    // închide nicio datorie/creanță, deci nu poate sta nici pe rolul de document
    // STINS. Argumentul „contrapartida unei plăți normale nu apare pe laturile
    // lui" nu ține: `NotaContabila` aduce contrapartide ARBITRARE din
    // repartitorii expliciți ai liniilor (49a), deci o notă cu repartitorul =
    // contul propriu al viramentului ar trece invariantul de contrapartidă și ar
    // stinge un picior — blocându-i definitiv anularea/stornarea, iar clientul
    // ascunde panoul de stingeri pe virament (F7-D8) ⇒ operatorul n-ar vedea DE CE.
    public override bool PoateFiStins(DevExpress.ExpressApp.IObjectSpace os) => !EsteVirament(os);

    // Dimensiunea Repartitor a notei (F7-D5b): pe virament AMBELE laturi
    // primesc contul propriu AL PICIORULUI. Default-ul bazei (debit←Predator,
    // credit←Primitor) pune pe fiecare rând contrapartida laturii — corect când
    // contrapartida e un partener, dar la un virament între două conturi pe
    // același sintetic (două bănci, ambele 5121 — analiticul se derivă din
    // dimensiuni, decizia 10) ieșirea lui A s-ar atribui lui B și invers:
    // soldul per cont propriu ar ieși exact inversat. Precedentul mecanismului:
    // Decont (32c) și DescarcareGestiune (37a).
    public override Guid RepartitorImplicitDebit(DevExpress.ExpressApp.IObjectSpace os) =>
        EsteVirament(os) ? GetContPropriuId() : base.RepartitorImplicitDebit(os);
    public override Guid RepartitorImplicitCredit(DevExpress.ExpressApp.IObjectSpace os) =>
        EsteVirament(os) ? GetContPropriuId() : base.RepartitorImplicitCredit(os);

    // Latura pereche a viramentului (F7-D4): Plata → Incasare, Incasare → Plata.
    // Contract, nu `is`/`switch` pe tip în clasa de bază (invariantul II).
    protected abstract DocumentTrezorerie CreeazaPereche(DevExpress.ExpressApp.IObjectSpace os);

    // Cele două picioare ale tranzitului 581 sunt confirmate de documente
    // diferite, la date diferite (foaia de vărsământ azi, extrasul mâine), deci
    // viramentul e o PERECHE, nu un document unic: la operarea primului picior
    // motorul generează draftul celuilalt. Nu e alegere de profil (convenția
    // tranzitului implică două picioare, punct) ⇒ hook de tip, nu PoliticaConex
    // — care ar cere două rânduri simetrice și n-are gard contra recursiei.
    public override Document GenereazaSecundar(DevExpress.ExpressApp.IObjectSpace os) {
        // Gardul de recursie, local și explicit: latura pereche e ea însăși un
        // virament, deci fără el operarea ei ar genera un al treilea document,
        // la infinit.
        if (Autogenerat || !EsteVirament(os))
            return null;

        var pereche = CreeazaPereche(os);
        pereche.Data = Data;
        pereche.TipInstrument = TipInstrument;
        // Laturile se copiază CA ATARE, NEinversate (F7-D1): predator = contul
        // sursă, primitor = contul destinație pe ambele documente; direcția o
        // poartă tipul (Plata = banii pleacă, Incasare = banii sosesc).
        pereche.PredatorId = PredatorId;
        pereche.PrimitorId = PrimitorId;
        // NU se copiază: Numar (server-owned, se asignează din seria proprie la
        // operarea perechii) și NumarExtras/DataExtras (fiecare picior are
        // extrasul lui).
        foreach (var s in Detalii) {
            var d = os.CreateObject<DocumentTrezorerieDetaliu>();
            d.Document = pereche;
            d.TipMaterialId = s.TipMaterialId;
            d.Valoare = s.Valoare;
            // `ValoareTva` intră în `Document.Total` (Σ Valoare + ValoareTva —
            // P1/36a), deci se clonează ODATĂ cu valoarea: pe calea API e mereu 0
            // (WriteDto-ul trezoreriei nu-l atinge), dar în UI-ul XAF câmpul e
            // vizibil și editabil pe linia de trezorerie — fără clonare perechea
            // ar ieși cu alt total decât sursa, fără niciun semnal.
            d.ValoareTva = s.ValoareTva;
            d.AngajamentId = s.AngajamentId;
            d.PreiaDimensiuni(s.DimensiuniCulese());
        }
        return pereche;
    }

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        foreach (var d in Detalii)
            if (d.Valoare <= 0)
                erori.Add("Fiecare linie poartă o valoare pozitivă (defalcarea sumei plătite/încasate).");

        // Virament către sine: cele două picioare ar posta 581 = 581 pe același
        // cont propriu, iar dimensiunea implicită n-ar mai distinge nimic.
        if (PredatorId != Guid.Empty && PredatorId == PrimitorId)
            erori.Add("Predatorul și primitorul sunt același repartitor — un virament cere două conturi proprii diferite.");

        // Cuplajul laturi ↔ natura liniilor (F7-D3), verificat în AMBELE
        // sensuri. Fără el, un virament cules din greșeală cu Tipul obișnuit de
        // trezorerie ar cădea pe regula GENERICĂ a tipului și ar posta
        // „cont destinație = cont sursă" pe FIECARE picior — dublă postare
        // tăcută. Cuplajul se formulează pe `NaturaClasa` (cod), nu pe coduri de
        // Clasă/Tip sau simboluri de cont: zero scurgere de profil în clasă.
        //
        // Naturile se preîncarcă pe FK-uri (25b — apelantul nu garantează lazy
        // loading), exact ca în motor.
        var idsTip = Detalii.Select(d => d.TipMaterialId).Distinct().ToList();
        var naturi = os.GetObjectsQuery<TipMaterial>()
            .Where(t => idsTip.Contains(t.ID))
            .Select(t => new { t.ID, t.Clasa.Natura })
            .ToDictionary(t => t.ID, t => t.Natura);
        // Tipul lipsă (linie neculeasă) e refuzat deja de baza `Document` — aici
        // nu e virament, deci cade natural pe ramura „linie care nu e virament".
        bool EsteLinieVirament(DocumentDetaliu d) =>
            naturi.TryGetValue(d.TipMaterialId, out var n) && n == NaturaClasa.Virament;

        var esteVirament = EsteVirament(os);
        var liniiVirament = Detalii.Where(EsteLinieVirament).ToList();
        if (esteVirament && liniiVirament.Count != Detalii.Count)
            erori.Add("Contrapartida e un cont propriu (virament intern) — toate liniile trebuie să fie de virament.");
        if (!esteVirament && liniiVirament.Count > 0)
            erori.Add("Liniile de virament intern cer contrapartidă cont propriu (casă/bancă) — corectați latura sau Tipul liniilor.");

        // Oglinda refuzului de pe DSC/FCL (38c): fără regulă de contare potrivită
        // pe Tip sau pe natura Virament, potrivirea ar cădea la final pe regula
        // GENERICĂ a tipului (TipMaterial null + NaturaFiltru null) și ar posta
        // din nou „destinație = sursă" pe ambele picioare, fără niciun zgomot.
        if (liniiVirament.Count > 0) {
            var tipDoc = Motor.MotorOperare.GasesteTipDocument(os, this);
            var reguli = os.GetObjectsQuery<RegulaContare>()
                .Where(r => r.TipDocumentId == tipDoc.ID)
                .Select(r => new { r.TipMaterialId, r.NaturaFiltru, r.SemnFiltru }).ToList();
            foreach (var d in liniiVirament) {
                // OGLINDA trebuie să rămână FIDELĂ potrivirii din motor, altfel
                // gardul devine decor: motorul filtrează ÎNTÂI pe `SemnFiltru`
                // (`MotorOperare`: `r.SemnFiltru == null || r.SemnFiltru == semn`),
                // și abia din supraviețuitori alege Tip exact → NaturaFiltru →
                // generic. Pe trezorerie `Cantitate` e 0, deci semnul e 0 și
                // ORICE rând cu SemnFiltru ±1 iese din joc. `RegulaContare` e dată
                // editabilă în XAF: un semn pus din greșeală pe rândul VIR ar
                // trece de un gard care ignoră semnul, iar motorul ar cădea pe
                // regula generică a tipului — exact dubla postare tăcută pe care
                // gardul există s-o prevină.
                var semn = Math.Sign(d.Cantitate);
                var candidati = reguli.Where(r => r.SemnFiltru == null || r.SemnFiltru == semn);
                if (!candidati.Any(r => r.TipMaterialId == d.TipMaterialId
                        || (r.TipMaterialId == null && r.NaturaFiltru == NaturaClasa.Virament)))
                    erori.Add("Linia de virament nu are regulă de contare potrivită (cont de tranzit = cont propriu) — "
                        + "adăugați rândul de politică (sau rulați updater-ul).");
            }
        }
    }
}

// Predator = ContPropriu (sursa banilor), primitor = beneficiarul.
// [TipDetaliu] se declară pe fiecare derivată (atributul e Inherited=false).
[TipDetaliu(typeof(DocumentTrezorerieDetaliu))]
public class Plata : DocumentTrezorerie {
    public override Guid GetContrapartidaId() => PrimitorId;

    protected override DocumentTrezorerie CreeazaPereche(DevExpress.ExpressApp.IObjectSpace os) =>
        os.CreateObject<Incasare>();

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        if (os.GetObjectByKey<Repartitor>(PredatorId) is not ContPropriu)
            erori.Add("Predatorul plății este contul propriu (casă/bancă) din care se plătește.");
        // Contrapartida acceptă și un cont propriu (F7-D3): viramentul intern
        // e o plată către al doilea cont propriu, nu un tip de document aparte.
        if (os.GetObjectByKey<Repartitor>(PrimitorId) is not (Partener or Angajat or ContPropriu))
            erori.Add("Primitorul plății este beneficiarul — un partener, un angajat (avans) sau un cont propriu (virament intern).");
    }
}

// Predator = plătitorul, primitor = ContPropriu (destinația banilor).
[TipDetaliu(typeof(DocumentTrezorerieDetaliu))]
public class Incasare : DocumentTrezorerie {
    public override Guid GetContrapartidaId() => PredatorId;

    protected override DocumentTrezorerie CreeazaPereche(DevExpress.ExpressApp.IObjectSpace os) =>
        os.CreateObject<Plata>();

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        // Contrapartida acceptă și un cont propriu (F7-D3) — vezi Plata.
        if (os.GetObjectByKey<Repartitor>(PredatorId) is not (Partener or Angajat or ContPropriu))
            erori.Add("Predatorul încasării este plătitorul — un partener, un angajat sau un cont propriu (virament intern).");
        if (os.GetObjectByKey<Repartitor>(PrimitorId) is not ContPropriu)
            erori.Add("Primitorul încasării este contul propriu (casă/bancă) în care se încasează.");
    }
}

// DIM-2 (decizia 54e, inventar §2, Î1/Î2): frunza UNICĂ a defalcării PLT+INC —
// semantica liniei e identică (defalcarea sumei, 31a), iar plata autogenerată
// clonează dimensiunile liniilor FCT, deci frunza poartă reuniunea FCT.
// Obligativitatea e politică per profil (PoliticaValidare/DimensiuniObligatorii,
// decizia 54d): la bugetar cerute ca date, la privat opționale — nimic hardcodat.
public class DocumentTrezorerieDetaliu : DocumentDetaliu {
    public virtual Guid? CodEconomicId { get; set; }
    [XafDisplayName("Cod economic")]
    public virtual CodEconomic CodEconomic { get; set; }

    public virtual Guid? SursaFinantareId { get; set; }
    [XafDisplayName("Sursă de finanțare")]
    public virtual SursaFinantare SursaFinantare { get; set; }

    public virtual Guid? CodFunctionalId { get; set; }
    [XafDisplayName("Cod funcțional")]
    public virtual CodFunctional CodFunctional { get; set; }

    public virtual Guid? ProiectId { get; set; }
    [XafDisplayName("Proiect")]
    public virtual Proiect Proiect { get; set; }

    public override Dimensiuni DimensiuniCulese() => new() {
        CodEconomicId = CodEconomicId, SursaFinantareId = SursaFinantareId,
        CodFunctionalId = CodFunctionalId, ProiectId = ProiectId
    };
    public override void PreiaDimensiuni(Dimensiuni s) {
        CodEconomicId = s.CodEconomicId; SursaFinantareId = s.SursaFinantareId;
        CodFunctionalId = s.CodFunctionalId; ProiectId = s.ProiectId;
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
