using Atlas.DXF.Core.Appearance.Attributes;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using System.ComponentModel.DataAnnotations;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// NOMENCLATORUL UNITĂȚILOR DE MĂSURĂ (felia 16, D16-D2) — codurile UN/ECE
// Recomandările 20 și 21, exact lista pe care o publică ANAF în foaia
// `Unitati_masura` a schemei SAF-T.
//
// DE CE nomenclator, și nu constantă în cod ca `TariUe`: exact motivul lui
// `Judet` — codul are o DENUMIRE de afișat și e FK pe `Produs` (lookup în UI).
// Lista în sine trăiește tot în cod (`UnitatiMasuraUnEce`), fiindcă e LEGE și
// fiindcă `Rezolva` din `UnitatiMasuraRo` are nevoie de ea în conectoare,
// înainte de a atinge baza.
//
// DE CE în NUCLEU: unitățile de măsură nu sunt ale planului de conturi —
// bugetarul are aceleași 2.163 de rânduri. Ca `Judet` și `RandD300`.
//
// DE CE `[ForbidCRUD]`: lista e a UN/ECE, publicată de ANAF. Seed-ul e singura
// ei cale de scriere, deci trebuie să fie și singura ei autoritate — un cod
// inventat cu mâna ar produce un `UOMStandard` respins la validare, iar o
// denumire schimbată ar rămâne înghețată peste următoarea publicare. Din
// același motiv e read-only în OData (56).
[NavigationItem("Nomenclatoare")]
[XafDefaultProperty(nameof(Cod))]
[ForbidCRUD("ListView", "DetailView")]
public class UnitateMasura : BaseObject {
    // Codul UN/ECE („H87", „KGM", „MTQ"). Unic, index filtrat pe `GCRecord = 0`
    // (ca `Judet.Cod`), cheia de idempotență a seed-ului.
    //
    // 9 = `UnitOfMeasure` din schema SAF-T (`SAFshorttextType`-ul redus). Codurile
    // reale au 1–4 caractere azi; lungimea e a CÂMPULUI din fișier, nu a listei
    // curente — o publicare cu un cod de 5 caractere nu trebuie să ceară migrație.
    [MaxLength(9)]
    public virtual string Cod { get; set; }

    // Denumirea românească publicată de ANAF (traducere indicativă), cu
    // diacritice; unde lipsește, denumirea engleză (două rânduri din 2.163:
    // `TST`, `XAL`). 256 = `Description` din `UOMTableEntry`.
    [MaxLength(256)]
    public virtual string Denumire { get; set; }
}
