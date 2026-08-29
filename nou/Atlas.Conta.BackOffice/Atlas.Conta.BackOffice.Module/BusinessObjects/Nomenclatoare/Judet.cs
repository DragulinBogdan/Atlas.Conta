using Atlas.DXF.Core.Appearance.Attributes;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using System.ComponentModel.DataAnnotations;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// NOMENCLATORUL DE JUDEȚE (felia 15, D15-D1) — subdiviziunile României din
// ISO 3166-2:RO, așa cum le listează schema SAF-T (`RO_SAFT_SchemaDefCod`,
// foaia `ISO3166-1A2 - RO Dept Codes`): 41 de județe + municipiul București.
//
// DE CE nomenclator, și nu constantă în cod ca `TariUe`: județul are o
// DENUMIRE de afișat și e FK pe `Partener` (lookup în UI, `Region` în SAF-T,
// coloană în e-Factura). `TariUe` rămâne constantă tocmai fiindcă e o mulțime
// de coduri fără denumire și fără referință — aici e invers.
//
// DE CE trei coduri: fiecare sursă îl scrie altfel și toate trei intră în
// felie. `Cod` = ISO 3166-2 („RO-CJ") — forma cerută de SAF-T și cheia de
// idempotență a seed-ului. `CodAuto` = indicativul de înmatriculare („CJ") —
// exact ce întoarce ANAF în `scod_JudetAuto`/`dcod_JudetAuto`. `CodCnp` =
// codul de județ din CNP (12 = Cluj, 40 = București, 51/52 =
// Călărași/Giurgiu) — cheia pe care o poartă 1C în `CodJudet`. Conversiile
// stau în `JudeteRo`, funcții PURE, testabile fără bază.
//
// DE CE în NUCLEU, nu în pachetul de profil: împărțirea administrativă a țării
// nu e a planului de conturi — bugetarul are aceleași 42 de rânduri.
//
// DE CE `[ForbidCRUD]` ca `RandD300`: județele sunt LEGE (ordin ISO/ANAF), nu
// configurare. Seed-ul RESCRIE câmpurile ne-cheie pe `Cod` existent, deci e
// singura lor autoritate; un județ redenumit cu mâna ar produce o adresă
// SAF-T care arată corect și e respinsă la validare. Din același motiv e
// read-only în OData (56).
[NavigationItem("Nomenclatoare")]
[XafDefaultProperty(nameof(Denumire))]
[ForbidCRUD("ListView", "DetailView")]
public class Judet : BaseObject, ICuCautare {
    // Codul ISO 3166-2, cu prefixul de țară: „RO-CJ", „RO-B". Unic (index
    // filtrat pe `GCRecord = 0` în DbContext) și cheia seed-ului.
    [MaxLength(5)]
    public virtual string Cod { get; set; }

    // Denumirea oficială, cu diacritice („Bistrița-Năsăud", „Dâmbovița") —
    // 35 e lungimea câmpului `Region` din `AddressStructure`.
    [MaxLength(35)]
    public virtual string Denumire { get; set; }

    // Indicativul auto: „CJ", „B" — `Cod` fără prefixul „RO-".
    [MaxLength(2)]
    [XafDisplayName("Indicativ auto")]
    public virtual string CodAuto { get; set; }

    // Codul de județ din CNP (pozițiile 8–9): 1..39 în ordinea alfabetică
    // DINAINTE de 1981 (Călărași și Giurgiu nu existau), 40 București,
    // 41–46 sectoarele Capitalei (nu se modelează: nu sunt județe),
    // 51 Călărași, 52 Giurgiu.
    [XafDisplayName("Cod CNP")]
    public virtual int CodCnp { get; set; }

    // F20-D1 — coloana GENERATĂ de căutare fără diacritice; valoarea e a
    // BAZEI de date (vezi `Cautare` / `ICuCautare`), EF n-o scrie niciodată.
    [XafDisplayName("Căutare")]
    [VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
    public virtual string Cautare { get; set; }
}
