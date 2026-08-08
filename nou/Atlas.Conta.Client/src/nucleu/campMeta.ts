import metadataBrut from '../generated/metadata.json?raw';
import openapiBrut from '../generated/openapi.json?raw';

// `useCampMeta` — atributele unui câmp, din cele DOUĂ dump-uri comise (43a/43d).
// Metadata leagă ATRIBUTELE câmpului; IDENTITATEA editorului rămâne cod, scrisă
// explicit în JSX. Aici nu există nicio hartă „câmp → editor" — dacă ar exista,
// ar fi exact registrul respins în 43a.
//
//   • caption            ← `metadata.json` (reflecție pe Module: `[XafDisplayName]`,
//                           fallback numele; FK-ul `{Nav}Id` moștenește caption-ul
//                           navigației, ca `HideForeignKeys` în XAF);
//   • required/maxLength ← schema OpenAPI a DTO-ului (schema *e* contractul —
//                           43 §2 stratul 1, structural, nu poate drifta).
//
// De ce `?raw` + `JSON.parse`: importul JSON tipat ar face TypeScript să
// infereze tipul literal al unui document de 600 KB (compilare lentă, tipuri
// inutile). Costul asumat al spike-ului: documentul OpenAPI ÎNTREG ajunge în
// bundle ca text. Îngustarea (un dump derivat doar cu schemele feliilor) e
// aditivă și nu schimbă nimic din API-ul de aici.

type MembriMeta = Record<string, string>;
type TipMeta = { DefaultProperty: string | null; Membri: MembriMeta };
type Metadata = { Enumuri: Record<string, Record<string, string>>; Tipuri: Record<string, TipMeta> };

type ProprietateSchema = {
  type?: string;
  format?: string;
  nullable?: boolean;
  maxLength?: number;
  $ref?: string;
};
type Schema = { properties?: Record<string, ProprietateSchema>; required?: string[] };
type DocumentOpenApi = { components?: { schemas?: Record<string, Schema> } };

const metadata = JSON.parse(metadataBrut) as Metadata;
const scheme = (JSON.parse(openapiBrut) as DocumentOpenApi).components?.schemas ?? {};

export type CampMeta = {
  caption: string;
  obligatoriu: boolean;
  lungimeMaxima?: number;
  format?: string;
  tip?: string;
};

// Convenția implicită a feliei: entitatea `X` se scrie prin `XWriteDto`.
// Liniile o rup (`NotaTransferLinieWriteDto`) — de aceea `schema` e parametru.
function schemaImplicita(tip: string): string {
  return `${tip}WriteDto`;
}

export function campMeta(tip: string, membru: string, schema?: string): CampMeta {
  const caption = metadata.Tipuri[tip]?.Membri[membru];
  if (caption === undefined && import.meta.env.DEV)
    console.warn(`[campMeta] „${tip}.${membru}" nu există în metadata.json — rulați ModelCheck --dump-metadata.`);

  const s = scheme[schema ?? schemaImplicita(tip)];
  const p = s?.properties?.[membru];
  // Fără `required[]` în swagger (Module nu are context nullable activ), semnalul
  // structural e `nullable`: `Guid`/`DateOnly` non-nullable = obligatoriu,
  // `Guid?`/`string` = opțional. Lista `required` e respectată dacă apare.
  const obligatoriu = p != null
    && ((s?.required?.includes(membru) ?? false) || p.nullable !== true);

  return {
    caption: caption ?? membru,
    obligatoriu,
    lungimeMaxima: p?.maxLength,
    format: p?.format,
    tip: p?.type,
  };
}

// Hook prin nume și prin contract; nu ține stare (dump-urile sunt constante de
// build) — există ca să fie punctul unic prin care componentele cer atribute.
export function useCampMeta(tip: string, membru: string, schema?: string): CampMeta {
  return campMeta(tip, membru, schema);
}

// Display-ul lookup-urilor (43f): proprietatea de afișare a nomenclatorului.
export function defaultProperty(tip: string): string {
  return metadata.Tipuri[tip]?.DefaultProperty ?? 'ID';
}

// Label-ul unei valori de enum (`Stare`, `TipStoc` …) — serverul le pune pe
// sârmă ca STRING (numele membrului), niciodată ca număr.
export function labelEnum(enumerare: string, valoare: string | null | undefined): string {
  if (!valoare) return '';
  return metadata.Enumuri[enumerare]?.[valoare] ?? valoare;
}
