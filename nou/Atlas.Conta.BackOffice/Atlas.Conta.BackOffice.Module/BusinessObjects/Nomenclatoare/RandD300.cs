using Atlas.DXF.Core.Appearance.Attributes;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// NOMENCLATORUL RÂNDURILOR DECONTULUI DE TVA (felia 12, D3-D1) — corpul
// formularului **300**, așa cum îl fixează **OPANAF nr. 174/2026** (M.Of.
// 105/09.02.2026), în vigoare de la prima perioadă fiscală a anului 2026:
// 45 de rânduri numerotate + 10 sub-rânduri („din care") = 55 de poziții.
// Structura integrală, cu formulele și schema XML v12.0.0, e în
// `docs/api/d300-structura-2026.md`.
//
// DE CE nomenclator, și nu `switch` în cod pe `TipTva.Cod`: rândul de decont e o
// funcție de `(TipTva, Sens)`, iar `TipTva` e nomenclator EDITABIL — un client
// care își adaugă un tip propriu trebuie să-l poată așeza pe rând fără release
// (invariantul IV / decizia 4: structura e cod, politica e date). Un cod liber
// pe `TipTva` (ca `CodSafTLivrare`) n-ar fi mers: o pereche cade pe MAI MULTE
// rânduri (taxarea inversă internă e în rd. 12.1 colectată ȘI în rd. 26.1
// deductibilă), iar un string magic n-ar fi avut nicio validare. FK-ul către
// rândurile de aici o are.
//
// DE CE în NUCLEU, nu în pachetul de profil: formularul e al legii, nu al
// profilului contabil — bugetarul are aceleași 55 de rânduri, doar că fără
// nicio mapare (`RegistruTva` îi e gol, neavând `PoliticaTva`). Ce diferă per
// profil e POLITICA (`MapareD300`), nu nomenclatorul.
//
// DE CE `[ForbidCRUD]` ca registrele: rândurile sunt LEGE, nu configurare —
// se schimbă prin seed odată cu ordinul care le schimbă, nu din UI. Un rând
// șters sau redenumit cu mâna ar produce un decont care arată corect și e greșit.
// Din același motiv nomenclatorul e read-only în OData (56).
//
// O SINGURĂ VERSIUNE a formularului, cea în vigoare (restanța D3-r1): o perioadă
// din 2025 proiectată aici pune 19% pe rd. 16/33 — corect pentru forma 2026, nu
// pentru decontul depus atunci. Ecranul avertizează sub 2026-01-01; versionarea
// pe an fiscal intră când o cere un client.
[NavigationItem("Nomenclatoare")]
[XafDefaultProperty(nameof(Cod))]
[ForbidCRUD("ListView", "DetailView")]
public class RandD300 : BaseObject {
    // Numărul din formular, ca text: „9", „12.1", „26.1" — sub-rândurile nu sunt
    // numere. Unic (index în DbContext); cheia de idempotență a seed-ului.
    public virtual string Cod { get; set; }
    public virtual string Denumire { get; set; }
    public virtual SectiuneD300 Sectiune { get; set; }
    // Poziția în formular (1..55) — ordinea de afișare, independentă de `Cod`
    // (care nu se sortează alfabetic: „10" < „9", „12.1" între 12 și 12.2).
    public virtual int Ordine { get; set; }
    // Coloanele pe care rândul CHIAR le are în formular. Rd. 13/14/15/29 n-au
    // coloană TVA, rd. 27/28/31/32/34/36…45 n-au coloană de bază: proiecția lasă
    // acolo `null`, nu 0 — un ecran care afișează „0,00" într-o casetă
    // inexistentă minte (D3-D3).
    public virtual bool AreBaza { get; set; }
    public virtual bool AreTva { get; set; }
    public virtual FelRandD300 Fel { get; set; }

    // Rândul „din care" (3.1→3, 5.1→5, 7.1→7, 12.1/12.2→12, 20.1→20, 22.1→22,
    // 26.1/26.2→26, 29.1→29). Părintele își adună copiii, iar totalurile
    // însumează DOAR nivelul 0 — de aici vine gardul contra dublei numărări.
    public virtual Guid? ParinteId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    public virtual RandD300 Parinte { get; set; }

    // Rândul-sursă al oglinzii, DOAR pe `Fel = Oglinda`: zona deductibilă 20…23,
    // 26, 26.1, 26.2 e copia exactă a zonei colectate 5…8, 12, 12.1, 12.2
    // (taxarea inversă se colectează și se deduce în aceeași perioadă, iar
    // formularul cere egalitatea ca validare blocantă).
    public virtual Guid? OglindaAId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    public virtual RandD300 OglindaA { get; set; }
}
