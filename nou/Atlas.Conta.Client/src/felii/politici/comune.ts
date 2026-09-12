import { campMeta, valoriEnum } from '../../nucleu/campMeta';
import { CAMP_CAUTARE } from '../../nucleu/odata';
import { urlCu } from '../../nucleu/urlStare';

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

// Opțiunile editorului unui lookup de coloană care caută pe coloana GENERATĂ
// `Cautare` (F20-D1) — nomenclatoarele mari (`Cont`: 644 de rânduri private)
// n-au cum fi parcurse cu ochiul, iar normalizarea diacriticelor e a bazei, nu
// a widget-ului. Ajung la SelectBox prin `column.editorOptions`, pe care grila
// le extinde peste opțiunile pe care le compune ea (`renderFormEditorTemplate`).
export const editorCautare = { searchEnabled: true, searchExpr: CAMP_CAUTARE, searchTimeout: 400 };

// „Explică pe acest tip" (F24-D7): din rândul unei politici către panoul
// `/politici/explica`, precompletat. URL-ul E starea panoului (43c), deci
// puntea dintre cele două ecrane e un link, nu un canal propriu.
//
// Rândul dă tipul de document (codul, din navigația adusă cu `$expand` —
// ruta cere CODUL ancorei, nu id-ul) și, unde le are, tipul de material și
// filtrul de semn. Ce nu are, nu trimite: panoul se deschide precompletat pe
// atât cât știe rândul, iar restul îl alege operatorul. `null` = rândul nu
// duce nicăieri (fără cod de tip), deci nici butonul nu se oferă.
export function urlExplica(cod: unknown, rand?: { tipMaterial?: unknown; semn?: unknown }): string | null {
  const tip = cod == null ? '' : String(cod);
  if (!tip) return null;
  return urlCu('/politici/explica', {
    tip,
    tipMaterial: rand?.tipMaterial == null ? '' : String(rand.tipMaterial),
    semn: rand?.semn == null ? '' : String(rand.semn),
  });
}

// Codul din navigația unui rând (`TipDocument`, `TipDocumentSursa`) — aceeași
// sursă pe care o afișează coloana, nu un `byKey` în plus.
export function codNav(rand: Record<string, unknown>, navigatie: string): unknown {
  return (rand?.[navigatie] as Record<string, unknown> | null | undefined)?.Cod;
}
