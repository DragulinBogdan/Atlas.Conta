import { campMeta, valoriEnum } from '../../nucleu/campMeta';

// Bucățile pe care le împart ecranele de politică. Nimic „inteligent": compuneri
// de etichetă și liste de opțiuni. Identitatea coloanelor rămâne cod, în fiecare
// ecran (43a).

export function codSiDenumire(e: Record<string, unknown>): string {
  if (!e) return '';
  const cod = e.Cod == null ? '' : String(e.Cod);
  const denumire = e.Denumire == null ? '' : String(e.Denumire);
  return cod && denumire ? `${cod} — ${denumire}` : cod || denumire;
}

export function simbolSiDenumire(e: Record<string, unknown>): string {
  if (!e) return '';
  const simbol = e.Simbol == null ? '' : String(e.Simbol);
  const denumire = e.Denumire == null ? '' : String(e.Denumire);
  return simbol && denumire ? `${simbol} — ${denumire}` : simbol || denumire;
}

export function etichetaTipTva(e: Record<string, unknown>): string {
  if (!e) return '';
  const cota = e.Cota == null ? null : Number(e.Cota);
  return `${codSiDenumire(e)}${cota == null ? '' : ` (${cota}%)`}`;
}

// Afișarea unei coloane de FK din navigația adusă cu `$expand`.
//
// De ce nu se lasă pe seama `<Lookup>`-ului coloanei, care are deja harta
// valoare → etichetă: harta lui e populată doar din rândurile ÎNCĂRCATE ale
// sursei, iar sursa tipurilor de TVA e filtrată pe `Activ`. Un rând vechi care
// referă un tip retras ar fi rămas GOL în grilă — exact rândul despre care
// operatorul are nevoie să afle.
//
// De ce funcție și nu selector-string (`"TipDocument.Cod"`): DevExtreme reține
// textul ales în editor (`displayValueMap`) și îl preferă oricărei alte afișări.
// Cu un selector de o singură proprietate, rândul tocmai editat ar fi arătat
// „FCT — Factură intrare" iar vecinii lui „FCT" — aceeași coloană, două
// ortografii, după cine a fost atins în sesiunea asta. Textul trebuie deci
// compus la fel ca `displayExpr`-ul lookup-ului.
//
// SORTAREA nu se pierde din cauza asta: coloana primește `calculateSortValue`
// ca STRING (`"TipDocument.Cod"`), iar `getSortDataSourceParameters` îl preferă
// afișării — string-ul ajunge pe sârmă ca `$orderby=TipDocument/Cod`. Un
// selector-FUNCȚIE acolo ar fi sortat tăcut doar pagina încărcată.
export function afisareNav(
  navigatie: string, compune: (element: Record<string, unknown>) => string,
): (rand: Record<string, unknown>) => string {
  return (rand) => {
    const element = rand?.[navigatie] as Record<string, unknown> | null | undefined;
    return element ? compune(element) : '';
  };
}

// Caption-ul unui membru, din `metadata.json`. Schema OpenAPI cerută e cea a
// ENTITĂȚII OData (nu convenția `…WriteDto` a documentelor): politicile se scriu
// prin OData, deci schema care le descrie poartă chiar numele tipului.
export function captionPolitica(tip: string, membru: string): string {
  return campMeta(tip, membru, tip).caption;
}

// Sursa unui `<Lookup>` de coloană peste un enum. Valorile circulă pe sârmă ca
// STRING (numele membrului), etichetele vin din dump (`[XafDisplayName]`).
//
// `etichetaGol` adaugă un rând cu valoarea `null` pentru enum-urile NULLABLE:
// pe `ClasaFiscala` nulul nu e „lipsă", e regula „orice clasă" (F23-D2), iar o
// celulă goală ar fi arătat-o ca pe o valoare neintrodusă. Cheia hărții interne
// a lookup-ului e obținută prin coerciție la string, deci `null` se potrivește.
export type OptiuneLookup = { valoare: string | number | null; label: string };

export function optiuniEnum(enumerare: string, etichetaGol?: string): OptiuneLookup[] {
  const lista: OptiuneLookup[] = valoriEnum(enumerare).map((v) => ({ valoare: v.valoare, label: v.label }));
  return etichetaGol === undefined ? lista : [{ valoare: null, label: etichetaGol }, ...lista];
}
