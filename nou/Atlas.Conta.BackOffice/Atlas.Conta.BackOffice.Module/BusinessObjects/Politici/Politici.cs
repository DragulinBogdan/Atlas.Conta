using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Decizia 4/20/21: structura e cod, politica e date. Rândurile de aici se
// SEED-uiesc pe funcționalitate (nu se transcriu din config-ul legacy).

// Ancoră seed care oglindește clasele 1:1 (decizia 20) — doar FK + UI.
[NavigationItem("Politici")]
[XafDefaultProperty(nameof(Denumire))]
public class TipDocument : BaseObject {
    public virtual string Cod { get; set; }
    public virtual string Denumire { get; set; }
    // Numele CLR al clasei derivate corespunzătoare (ex. "FacturaIntrare").
    public virtual string ClrType { get; set; }

    // Datoria P1 (design §8): default TipTva per tip de document, aplicat la
    // CULEGERE (nu în motor) — pe ancoră, NU pe PoliticaTva (bugetarul n-are
    // rânduri PoliticaTva dar are nevoie de default CAP21, iar un rând
    // PoliticaTva doar-pentru-default ar activa pasul TVA din motor). Folosirea
    // vine în pașii următori.
    public virtual Guid? TipTvaImplicitId { get; set; }
    public virtual TipTva TipTvaImplicit { get; set; }
}

// Regula de alimentare a registrului de stoc: tip document × latură × filtru
// Clasă → tip stoc + semn (00 §4, curățat: filtrul SEMN_ITEMS moare).
[NavigationItem("Politici")]
public class RegulaStoc : BaseObject {
    public virtual Guid TipDocumentId { get; set; }
    public virtual TipDocument TipDocument { get; set; }
    public virtual LaturaDocument Latura { get; set; }
    public virtual Guid? ClasaId { get; set; }
    public virtual ClasaProdus Clasa { get; set; }
    public virtual TipStoc TipStoc { get; set; }
    public virtual int Semn { get; set; }
}

// Maparea contabilă: tip document × Clasă/Tip → cont D/C + dimensiuni
// (decizia 15: Comun / OverrideDebit / OverrideCredit).
// Potrivirea pe linie (motor): TipMaterial exact → NaturaFiltru → regula
// generică (ambele null). Fără regulă potrivită = linia nu contează pe acest
// tip de document (așa se împarte lanțul FCT/NIR fără dublă postare).
[NavigationItem("Politici")]
public class RegulaContare : BaseObject {
    public virtual Guid TipDocumentId { get; set; }
    public virtual TipDocument TipDocument { get; set; }
    public virtual Guid? TipMaterialId { get; set; }
    public virtual TipMaterial TipMaterial { get; set; }
    // Filtru pe natura Clasei liniei (ex. FCT contează DOAR non-stoc — recepția
    // o postează NIR-ul); mai slab decât potrivirea exactă pe TipMaterial.
    public virtual NaturaClasa? NaturaFiltru { get; set; }
    // Filtru pe semnul cantității liniei (inventar 05: LDI e singurul tip unde
    // semnul chiar diferențiază — plusul contează venit, minusul cheltuială).
    // Null = orice semn; valoarea postată se normalizează cu semnul filtrului
    // (linia de minus poartă valoare negativă, nota ei rămâne pozitivă).
    public virtual int? SemnFiltru { get; set; }
    // Corespondență de STORNO (FAZA 1C §7, rezoluția spike-ului): valoarea liniei
    // se postează CU SEMNUL EI, fără normalizarea `SemnFiltru`, pe corespondența
    // ORIGINALĂ a achiziției/vânzării. Retururile (RLF/RDC) culeg pozitiv, își
    // semnează liniile negativ la operare și postează minus pe aceeași latură —
    // exact convenția `Storneaza` a motorului (25d) și reprezentarea 1C.
    public virtual bool PastreazaSemn { get; set; }
    // Conturile se rezolvă declarativ per latură (testul bazei §7.2): sursa
    // indică nomenclatorul purtător (Tip material / partenerul unei laturi),
    // iar contul explicit de mai jos rămâne valoare directă sau fallback.
    public virtual SursaCont SursaContDebit { get; set; }
    public virtual SursaCont SursaContCredit { get; set; }
    // Contul explicit al regulii trăiește în planul mare — lookup standard
    // (SmartLookup revertat, decizia 40d/gate).
    public virtual Guid? ContDebitId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    public virtual Cont ContDebit { get; set; }
    public virtual Guid? ContCreditId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    public virtual Cont ContCredit { get; set; }
    // DIM-3 (decizia 54c): cele trei seturi de dimensiuni ale regulii = coloane
    // PLATE ([Column] conservă schema owned-ului) — editarea politicilor devine
    // XAF-nativă (FK-uri normale cu lookup). Motorul le citește ca value
    // object prin metodele Dimensiuni*() de la final.
    [Column("DimensiuniComun_RepartitorId")]
    public virtual Guid? ComunRepartitorId { get; set; }
    [XafDisplayName("Repartitor comun")]
    public virtual Repartitor ComunRepartitor { get; set; }
    [Column("DimensiuniComun_MaterialId")]
    public virtual Guid? ComunMaterialId { get; set; }
    [XafDisplayName("Material comun")]
    public virtual Produs ComunMaterial { get; set; }
    [Column("DimensiuniComun_CodFunctionalId")]
    public virtual Guid? ComunCodFunctionalId { get; set; }
    [XafDisplayName("Cod funcțional comun")]
    public virtual CodFunctional ComunCodFunctional { get; set; }
    [Column("DimensiuniComun_CodEconomicId")]
    public virtual Guid? ComunCodEconomicId { get; set; }
    [XafDisplayName("Cod economic comun")]
    public virtual CodEconomic ComunCodEconomic { get; set; }
    [Column("DimensiuniComun_SursaFinantareId")]
    public virtual Guid? ComunSursaFinantareId { get; set; }
    [XafDisplayName("Sursă finanțare comun")]
    public virtual SursaFinantare ComunSursaFinantare { get; set; }
    [Column("DimensiuniComun_UnitateId")]
    public virtual Guid? ComunUnitateId { get; set; }
    [XafDisplayName("Unitate comun")]
    public virtual Unitate ComunUnitate { get; set; }
    [Column("DimensiuniComun_ProiectId")]
    public virtual Guid? ComunProiectId { get; set; }
    [XafDisplayName("Proiect comun")]
    public virtual Proiect ComunProiect { get; set; }
    [Column("DimensiuniComun_CentruCostId")]
    public virtual Guid? ComunCentruCostId { get; set; }
    [XafDisplayName("Centru cost comun")]
    public virtual Repartitor ComunCentruCost { get; set; }

    [Column("DimensiuniOverrideDebit_RepartitorId")]
    public virtual Guid? OverrideDebitRepartitorId { get; set; }
    [XafDisplayName("Repartitor override debit")]
    public virtual Repartitor OverrideDebitRepartitor { get; set; }
    [Column("DimensiuniOverrideDebit_MaterialId")]
    public virtual Guid? OverrideDebitMaterialId { get; set; }
    [XafDisplayName("Material override debit")]
    public virtual Produs OverrideDebitMaterial { get; set; }
    [Column("DimensiuniOverrideDebit_CodFunctionalId")]
    public virtual Guid? OverrideDebitCodFunctionalId { get; set; }
    [XafDisplayName("Cod funcțional override debit")]
    public virtual CodFunctional OverrideDebitCodFunctional { get; set; }
    [Column("DimensiuniOverrideDebit_CodEconomicId")]
    public virtual Guid? OverrideDebitCodEconomicId { get; set; }
    [XafDisplayName("Cod economic override debit")]
    public virtual CodEconomic OverrideDebitCodEconomic { get; set; }
    [Column("DimensiuniOverrideDebit_SursaFinantareId")]
    public virtual Guid? OverrideDebitSursaFinantareId { get; set; }
    [XafDisplayName("Sursă finanțare override debit")]
    public virtual SursaFinantare OverrideDebitSursaFinantare { get; set; }
    [Column("DimensiuniOverrideDebit_UnitateId")]
    public virtual Guid? OverrideDebitUnitateId { get; set; }
    [XafDisplayName("Unitate override debit")]
    public virtual Unitate OverrideDebitUnitate { get; set; }
    [Column("DimensiuniOverrideDebit_ProiectId")]
    public virtual Guid? OverrideDebitProiectId { get; set; }
    [XafDisplayName("Proiect override debit")]
    public virtual Proiect OverrideDebitProiect { get; set; }
    [Column("DimensiuniOverrideDebit_CentruCostId")]
    public virtual Guid? OverrideDebitCentruCostId { get; set; }
    [XafDisplayName("Centru cost override debit")]
    public virtual Repartitor OverrideDebitCentruCost { get; set; }

    [Column("DimensiuniOverrideCredit_RepartitorId")]
    public virtual Guid? OverrideCreditRepartitorId { get; set; }
    [XafDisplayName("Repartitor override credit")]
    public virtual Repartitor OverrideCreditRepartitor { get; set; }
    [Column("DimensiuniOverrideCredit_MaterialId")]
    public virtual Guid? OverrideCreditMaterialId { get; set; }
    [XafDisplayName("Material override credit")]
    public virtual Produs OverrideCreditMaterial { get; set; }
    [Column("DimensiuniOverrideCredit_CodFunctionalId")]
    public virtual Guid? OverrideCreditCodFunctionalId { get; set; }
    [XafDisplayName("Cod funcțional override credit")]
    public virtual CodFunctional OverrideCreditCodFunctional { get; set; }
    [Column("DimensiuniOverrideCredit_CodEconomicId")]
    public virtual Guid? OverrideCreditCodEconomicId { get; set; }
    [XafDisplayName("Cod economic override credit")]
    public virtual CodEconomic OverrideCreditCodEconomic { get; set; }
    [Column("DimensiuniOverrideCredit_SursaFinantareId")]
    public virtual Guid? OverrideCreditSursaFinantareId { get; set; }
    [XafDisplayName("Sursă finanțare override credit")]
    public virtual SursaFinantare OverrideCreditSursaFinantare { get; set; }
    [Column("DimensiuniOverrideCredit_UnitateId")]
    public virtual Guid? OverrideCreditUnitateId { get; set; }
    [XafDisplayName("Unitate override credit")]
    public virtual Unitate OverrideCreditUnitate { get; set; }
    [Column("DimensiuniOverrideCredit_ProiectId")]
    public virtual Guid? OverrideCreditProiectId { get; set; }
    [XafDisplayName("Proiect override credit")]
    public virtual Proiect OverrideCreditProiect { get; set; }
    [Column("DimensiuniOverrideCredit_CentruCostId")]
    public virtual Guid? OverrideCreditCentruCostId { get; set; }
    [XafDisplayName("Centru cost override credit")]
    public virtual Repartitor OverrideCreditCentruCost { get; set; }

    public Dimensiuni DimensiuniComun() => new() {
        RepartitorId = ComunRepartitorId, MaterialId = ComunMaterialId,
        CodFunctionalId = ComunCodFunctionalId, CodEconomicId = ComunCodEconomicId,
        SursaFinantareId = ComunSursaFinantareId, UnitateId = ComunUnitateId,
        ProiectId = ComunProiectId, CentruCostId = ComunCentruCostId
    };
    public Dimensiuni DimensiuniOverrideDebit() => new() {
        RepartitorId = OverrideDebitRepartitorId, MaterialId = OverrideDebitMaterialId,
        CodFunctionalId = OverrideDebitCodFunctionalId, CodEconomicId = OverrideDebitCodEconomicId,
        SursaFinantareId = OverrideDebitSursaFinantareId, UnitateId = OverrideDebitUnitateId,
        ProiectId = OverrideDebitProiectId, CentruCostId = OverrideDebitCentruCostId
    };
    public Dimensiuni DimensiuniOverrideCredit() => new() {
        RepartitorId = OverrideCreditRepartitorId, MaterialId = OverrideCreditMaterialId,
        CodFunctionalId = OverrideCreditCodFunctionalId, CodEconomicId = OverrideCreditCodEconomicId,
        SursaFinantareId = OverrideCreditSursaFinantareId, UnitateId = OverrideCreditUnitateId,
        ProiectId = OverrideCreditProiectId, CentruCostId = OverrideCreditCentruCostId
    };
}

// Documentul conex (decizia 17, 00 §6): sursă → țintă + filtrul de conținut
// (ce linii trec pe documentul generat). Filtrul legacy enumera tipuri de
// material per defa (GEST_ITEMSI_TIP_MATERIAL); funcțional criteriul e natura
// liniei (pe NIR trec exact liniile purtătoare de stoc) — decizia 21: politica
// se definește pe funcționalitate, nu prin transcrierea listelor legacy.
[NavigationItem("Politici")]
public class PoliticaConex : BaseObject {
    public virtual Guid TipDocumentSursaId { get; set; }
    public virtual TipDocument TipDocumentSursa { get; set; }
    public virtual Guid TipDocumentTintaId { get; set; }
    public virtual TipDocument TipDocumentTinta { get; set; }
    // TIP_DESCARCARE legacy: ținta inversează laturile (predator ↔ primitor).
    public virtual bool InverseazaLaturi { get; set; }
    // Null = trec toate liniile; altfel doar cele cu Clasa.Natura potrivită.
    public virtual NaturaClasa? NaturaFiltru { get; set; }
}

// Scadența default per tip (inventar 07: `DATA_SCADENTA = data_docum + 30` era
// formulă de header în legacy — politică de scadență, nu structură). Motorul o
// aplică la operare pe IDocumentCuScadenta DOAR dacă scadența nu a fost culeasă.
[NavigationItem("Politici")]
public class PoliticaScadenta : BaseObject {
    public virtual Guid TipDocumentId { get; set; }
    public virtual TipDocument TipDocument { get; set; }
    public virtual int ZileDefault { get; set; }
}

// Obligativitățile per tip (felia 3d) — profil de VALIDARE, nu structură
// (decizia 29: regula bugetară „angajament SAU cod economic" e a profilului,
// nu a clasei de document; la privat rândurile pur și simplu lipsesc).
// Motorul o aplică generic înaintea hook-urilor proprii tipului.
[NavigationItem("Politici")]
public class PoliticaValidare : BaseObject {
    public virtual Guid TipDocumentId { get; set; }
    public virtual TipDocument TipDocument { get; set; }
    // Fiecare linie cere clasificație bugetară: angajament SAU cod economic
    // (fosta validare hardcodată pe FCT/DEC, extinsă pe PLT — 29b/31f/32d).
    public virtual bool CereClasificatieBugetara { get; set; }
    // Liniile cu această natură se refuză (FCL: în acest profil facturarea nu
    // descarcă gestiune — 30a; vânzarea din stoc = document propriu la nevoie).
    public virtual NaturaClasa? NaturaInterzisa { get; set; }
}

// Postarea TVA per tip de document (P1, design §4) — independentă de potrivirea
// regulii principale de contare: pe FCT liniile de stoc NU au regulă (netul
// postează pe NIR-ul conex), dar TVA-ul lor deductibil se postează pe FACTURĂ.
// Simetrică cu PoliticaScadenta/PoliticaValidare; fără rând = niciun rând TVA
// (profilul bugetar nu primește rânduri — zero schimbare de comportament).
[NavigationItem("Politici")]
public class PoliticaTva : BaseObject {
    public virtual Guid TipDocumentId { get; set; }
    public virtual TipDocument TipDocument { get; set; }
    public virtual DirectieTva Directie { get; set; }
    // Contrapartida rândului de TVA (401/542/411…): sursa declarativă
    // (repartitorul unei laturi) + fallback explicit, ca la RegulaContare.
    public virtual SursaCont SursaContrapartida { get; set; }
    public virtual Guid? ContrapartidaFallbackId { get; set; }
    public virtual Cont ContrapartidaFallback { get; set; }
}

// Conturile închiderii lunare de TVA (FAZA 1C §6) — DATE per profil, nu
// simboluri în motor (decizia 29: motorul e agnostic la plan). Rândul privat
// leagă 4426/4427/4423/4424; bugetarul nu primește rând, deci ITV rămâne tip
// inert acolo (ca DSC — `InchidereTvaService.Genereaza` întoarce null).
// Toate cele patru conturi sunt nullable în schemă (politică editabilă, culeasă
// în trepte), dar serviciul cere setul COMPLET ca să genereze ceva.
[NavigationItem("Politici")]
public class PoliticaInchidereTva : BaseObject {
    public virtual Guid TipDocumentId { get; set; }
    public virtual TipDocument TipDocument { get; set; }
    // 4426 — TVA deductibilă (sold debitor de închis).
    public virtual Guid? ContDeductibilaId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    public virtual Cont ContDeductibila { get; set; }
    // 4427 — TVA colectată (sold creditor de închis).
    public virtual Guid? ContColectataId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    public virtual Cont ContColectata { get; set; }
    // 4423 — TVA de plată (excedentul de colectată).
    public virtual Guid? ContDePlataId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    public virtual Cont ContDePlata { get; set; }
    // 4424 — TVA de recuperat (excedentul de deductibilă).
    public virtual Guid? ContDeRecuperatId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    public virtual Cont ContDeRecuperat { get; set; }
}

[NavigationItem("Politici")]
public class PoliticaNumerotare : BaseObject {
    public virtual Guid TipDocumentId { get; set; }
    public virtual TipDocument TipDocument { get; set; }
    public virtual string Serie { get; set; }
    public virtual int UrmatorulNumar { get; set; }
    public virtual string Format { get; set; }
}

// Așezarea unei operațiuni taxabile pe rândul decontului de TVA (felia 12,
// D3-D2): politica `(TipTva × Sens) → RandD300`, cu N rânduri per pereche.
//
// DE CE n rânduri și nu o coloană pe `TipTva` (ca `CodSafTLivrare`): taxarea
// inversă istorică pe achiziție (TI19) apare ȘI la rd. 16 (regularizări taxă
// colectată), ȘI la rd. 33 (regularizări taxă dedusă) — o coloană n-ar fi
// încăput. Iar rândurile formularului se schimbă cu legea (2026 a eliminat 13
// rânduri și a comprimat numerotarea), deci ținta trebuie să fie o FK
// verificabilă, nu un string magic.
//
// Perechea NEMAPATĂ nu e o eroare: un `TipTva` propriu al clientului, fără
// mapare, apare la prima proiecție în panoul „Operațiuni neincluse în decont"
// cu cifrele lui (D3-D4) — raportarea nu refuză ce operarea a acceptat. Ce se
// REFUZĂ e o mapare care țintește un rând `Total`/`Oglindă`/`Extern`: cifra ar
// fi suprascrisă tăcut de formulă la prima proiecție, adică exact „un gard care
// tace devine capcană" (62f).
[NavigationItem("Politici")]
public class MapareD300 : BaseObject {
    // Validare de culegere pe NAVIGAȚII, nu pe Guid-uri (decizia 40b:
    // `RuleRequiredField` nu poate sta pe un `Guid` nenullabil — o linie culeasă
    // fără tip ar produce un INSERT cu FK invalid).
    public virtual Guid TipTvaId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Tip TVA")]
    [RuleRequiredField("MapareD300_TipTva_Necesar", DefaultContexts.Save,
        CustomMessageTemplate = "Tipul de TVA este obligatoriu.")]
    public virtual TipTva TipTva { get; set; }

    // Latura pe care se aplică maparea — aceeași axă ca `RegistruTva.Sens`
    // (achiziție/livrare), fiindcă rândul de decont diferă per sens: N21 e rd. 9
    // pe livrare și rd. 24 pe achiziție.
    public virtual SensTva Sens { get; set; }

    public virtual Guid RandId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Rând D300")]
    [RuleRequiredField("MapareD300_Rand_Necesar", DefaultContexts.Save,
        CustomMessageTemplate = "Rândul de decont este obligatoriu.")]
    // AFORDANȚA lookup-ului (fix F7 al review-ului advers): lista oferă doar
    // rândurile care CHIAR se pot alimenta din mapări. Refuzul rămâne în
    // validare — filtrul e ce vede operatorul, nu ce garantează sistemul (65).
    //
    // Forma e `DataSourceCriteriaProperty`, nu `DataSourceCriteria("Fel = ...")`,
    // și asta e o constatare MĂSURATĂ, nu o preferință: limbajul de criterii NU
    // acceptă numele unui membru de enum („Criteria Language Syntax", secțiunea
    // Constants — `[Status] = Status.InProgress` e explicit greșit; se acceptă
    // doar valoarea întreagă `= 1` sau forma serializată
    // `##Enum#Namespace.Tip,Membru#`, care cere în plus `EnumProcessingHelper
    // .RegisterEnum`). Ambele ar fi mutat comparația din compilator într-un
    // string — exact ce refuză și comentariul regulii de mai jos.
    [DataSourceCriteriaProperty(nameof(CriteriuRandOperatiuni))]
    public virtual RandD300 Rand { get; set; }

    [NotMapped]
    [Browsable(false)]
    public CriteriaOperator CriteriuRandOperatiuni =>
        CriteriaOperator.FromLambda<RandD300>(r => r.Fel == FelRandD300.Operatiuni);

    // Gardianul D3-D2, ca regulă de fond (nu criteriu în limbaj de criterii:
    // comparația de enum se scrie o dată, în C#, și nu depinde de parsare).
    // Ascuns din UI — e purtătorul regulii, nu un câmp de cules.
    [NotMapped]
    [Browsable(false)]
    [RuleFromBoolProperty("MapareD300_RandOperatiuni", DefaultContexts.Save,
        CustomMessageTemplate = "Rândul de decont trebuie să fie de fel „Operațiuni” — "
            + "rândurile de total, oglindă și extern se calculează, nu se alimentează din mapări.")]
    public bool RandEsteDeOperatiuni => Rand == null || Rand.Fel == FelRandD300.Operatiuni;

    // A DOUA jumătate a gardului contra dublei numărări (fix F4 al review-ului
    // advers), geamăna celui din `ContaSeeder.VerificaD300`: aceeași pereche
    // `(TipTva, Sens)` nu poate ținti simultan un rând ȘI un ascendent al lui.
    //
    // Proiecția adună părintele ca „mapările lui DIRECTE + Σ copiii": o pereche
    // pusă și pe rd. 12.1, și pe rd. 12 intră de două ori în rd. 12 și de acolo
    // în totalul rd. 19 — un decont care trece validările ANAF și e greșit.
    // Seed-ul apără seed-ul; regula de aici apără CULEGEREA din XAF, unde
    // maparea e editabilă (politica e date, decizia 4).
    //
    // Verificarea e SIMETRICĂ: nu contează dacă rândul nou e copilul unei mapări
    // existente sau părintele ei — conflictul e al perechii, nu al ordinii în
    // care s-au cules cele două. Fraților li se dă voie (TI19/achiziție pe
    // rd. 16 ȘI rd. 33 e chiar cazul care a cerut nomenclatorul).
    [NotMapped]
    [Browsable(false)]
    [RuleFromBoolProperty("MapareD300_FaraAscendent", DefaultContexts.Save,
        CustomMessageTemplate = "Aceeași pereche (tip de TVA × sens) țintește deja un rând aflat pe "
            + "aceeași verticală „din care” cu cel ales — cifra ar intra de două ori în rândul-părinte "
            + "și în totalul lui. Păstrați o singură mapare pe verticală.")]
    public bool RandFaraAscendentMapat {
        get {
            var os = ((IObjectSpaceLink)this).ObjectSpace;
            var tipId = TipTva?.ID ?? TipTvaId;
            if (os == null || Rand == null || tipId == Guid.Empty)
                return true;
            var sens = Sens;
            // Doar mapările VII ale aceleiași perechi: una ștearsă logic nu mai
            // alimentează nimic, deci nu poate dubla nimic (F5).
            var altele = os.GetObjectsQuery<MapareD300>()
                .Where(m => m.TipTvaId == tipId && m.Sens == sens).ToList();
            foreach (var alta in altele) {
                if (ReferenceEquals(alta, this) || alta.Rand == null || alta.RandId == Rand.ID)
                    continue;
                if (EsteAscendent(alta.Rand, Rand) || EsteAscendent(Rand, alta.Rand))
                    return false;
            }
            return true;
        }
    }

    // „`posibil` e undeva pe lanțul de părinți al lui `rand`" — cu gardă de
    // ciclu, ca peste tot unde se urcă pe `Parinte` (67e).
    static bool EsteAscendent(RandD300 posibil, RandD300 rand) {
        var vazute = new HashSet<Guid>();
        for (var sus = rand.Parinte; sus != null && vazute.Add(sus.ID); sus = sus.Parinte)
            if (sus.ID == posibil.ID)
                return true;
        return false;
    }
}

// Așezarea unei operațiuni taxabile pe tipul de operațiune din secțiunea 2 a
// D394 (felia 14, D4-D2): politica `(TipTva × Sens) → TipOperatiuneD394`,
// aceeași logică de la D300 — perechea e DIRECȚIONALĂ (N21 pe livrare = L, pe
// achiziție = A), deci nu încape într-o coloană pe `TipTva` (fosta
// `CategorieD394`, moartă odată cu felia). Ținta e enum, nu nomenclator:
// `Int_tipOpSType` e stabil din 2016.
//
// O SINGURĂ mapare per pereche (index unic filtrat pe `GCRecord = 0`): un grup
// de registru nu poate fi pe două tipuri de operațiune — ar intra de două ori
// în sumele de control ale formularului. Perechea NEMAPATĂ nu e eroare: apare
// în panoul `Neincluse` cu cifrele ei (D4-D4). Ce se REFUZĂ e ținta `AI`/`N`:
// `AI` se DERIVĂ în cod (A × furnizor cu `TvaLaIncasare`) — mapată explicit,
// ar clasifica un furnizor cu sistem normal ca fiind la încasare —, iar `N`
// n-are sursă în registru (D4-r3) — mapată, ar promite o coloană goală.
[NavigationItem("Politici")]
public class MapareD394 : BaseObject {
    public virtual Guid TipTvaId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Tip TVA")]
    [RuleRequiredField("MapareD394_TipTva_Necesar", DefaultContexts.Save,
        CustomMessageTemplate = "Tipul de TVA este obligatoriu.")]
    public virtual TipTva TipTva { get; set; }

    // Aceeași axă ca `RegistruTva.Sens` și `MapareD300.Sens`.
    public virtual SensTva Sens { get; set; }

    [XafDisplayName("Tip operațiune D394")]
    public virtual TipOperatiuneD394 Tip { get; set; }

    // Gardianul D4-D2 ca regulă de fond (`RuleFromBoolProperty`, nu
    // `RuleCriteria`: limbajul de criterii nu acceptă numele unui membru de
    // enum — vezi comentariul de pe `MapareD300.Rand`). Ascuns din UI — e
    // purtătorul regulii, nu un câmp de cules. Geamănul lui la seed:
    // `ContaSeeder.VerificaD394`.
    [NotMapped]
    [Browsable(false)]
    [RuleFromBoolProperty("MapareD394_TintaPermisa", DefaultContexts.Save,
        CustomMessageTemplate = "Tipul de operațiune AÎ se derivă din partener (TVA la încasare), iar N "
            + "n-are sursă în registrul de TVA — se mapează doar L, A, V, C, LS, AS.")]
    public bool TintaEstePermisa => TintaPermisa(Tip);

    public static bool TintaPermisa(TipOperatiuneD394 tip) =>
        tip is TipOperatiuneD394.L or TipOperatiuneD394.A or TipOperatiuneD394.V
            or TipOperatiuneD394.C or TipOperatiuneD394.LS or TipOperatiuneD394.AS;
}
