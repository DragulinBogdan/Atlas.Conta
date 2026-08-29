import { useQuery } from '@tanstack/react-query';
import ODataStore from 'devextreme/data/odata/store';
import { when } from 'devextreme/core/utils/deferred';
import { cache } from './cache';
import { areMembru, tabelCautare } from './campMeta';
import { expiraSesiunea, token } from './auth';
import { ia } from './http';

// Singurul store OData al clientului (F20-D2). Până la felia 20 cablajul
// (`url` + JWT + 401) era duplicat DELIBERAT în `nucleu/Lookup` și în
// `felii/raportare/comune`, cu clauza scrisă acolo: „dacă apare a treia
// utilizare, extragerea se justifică". A apărut — și odată cu ea două lucruri
// pe care un store per widget nu le poate face:
//
//  1. **`byKey` prin cache.** Rezolvarea afișării unei valori deja salvate
//     (`SelectBox` cu `value` dar fără item încărcat) trece prin
//     `dataSource.loadSingle('ID', v)` → `store.byKey(v)`. Cu `ODataStore`
//     nativ per instanță asta e o cerere HTTP per WIDGET și per MONTARE:
//     ecranul NTC cerea de 8 ori aceeași `UnitateInterna`. Cheia
//     `['nomenclator', entitate, id]` cu `staleTime: Infinity` face rezolvarea
//     o dată pe SESIUNE, partajată între toate widget-urile și toate ecranele.
//  2. **Normalizarea căutării** (F20-D1): literalul din `contains(Cautare,…)`
//     se trece prin ACELAȘI tabel de diacritice pe care coloana generată îl
//     aplică în SQL, altfel „ștefan" n-ar găsi niciodată „STEFAN".
//
// Ce NU e aici: `dxStore.ts` (grilele REST prin `devextreme-aspnet-data`) — e
// altă conductă, cu alt protocol; nu se unifică pentru că amândouă „aduc date".

// ── normalizarea căutării (F20-D1.5) ────────────────────────────────────────

// Tabelul vine din `metadata.json`, nu din TS: o copie locală ar putea drifta
// de coloana generată, iar driftul s-ar vedea abia ca „lookup-ul nu găsește".
const TABEL = tabelCautare();

// Aceeași transformare pe care o face `Cautare` în SQL — `lower` întâi, apoi
// `translate`. Ordinea contează: tabelul n-are majuscule tocmai fiindcă
// lower-ul le-a rezolvat deja.
export function normalizeazaCautare(text: string): string {
  let rezultat = '';
  for (const caracter of text.toLowerCase()) {
    const i = TABEL.De.indexOf(caracter);
    rezultat += i >= 0 && i < TABEL.La.length ? TABEL.La[i] : caracter;
  }
  return rezultat;
}

// Numele coloanei de căutare. Entitățile care NU o au (`Lot`, `RandD300`) cad
// pe proprietatea lor de afișare — apelantul decide, aici doar se răspunde.
export const CAMP_CAUTARE = 'Cautare';

export function areCautare(entitate: string): boolean {
  return areMembru(entitate, CAMP_CAUTARE);
}

// Rescrierea literalului din `$filter`, în `beforeSend`.
//
// Ce generează DevExtreme pentru `searchExpr="Cautare"` (v4, cu
// `oDataFilterToLower` implicit `true`): `contains(tolower(Cautare),'text')`,
// cu textul DEJA trecut prin `toLowerCase()` al bibliotecii. Rescrierea face
// două lucruri, amândouă pe cheia numelui de coloană — deci un `$filter` cu mai
// mulți termeni (`Cautare` OR `CodFiscal`) își păstrează ceilalți termeni
// neatinși:
//
//   • `tolower(Cautare)` → `Cautare`: coloana e generată DEJA lowercase, iar
//     `lower()` peste ea ar fi doar muncă de server pe fiecare tastare;
//   • literalul → `normalizeazaCautare(literal)`.
//
// Apostroful se dublează în OData; regexul îl acceptă în literal (`''`) și
// re-escapează rezultatul, ca un text cu apostrof să nu rupă filtrul.
// Toate cele trei funcții de text ale OData, nu doar `contains`: un `FilterRow`
// cu `startswith` pe coloană ar fi trimis altfel `tolower(Cautare)` peste un
// literal cu diacritice ⇒ zero rânduri, tăcut (review F20 F4).
const FILTRU_CAUTARE = new RegExp(`(contains|startswith|endswith)\\((?:tolower\\()?${CAMP_CAUTARE}\\)?,'((?:[^']|'')*)'\\)`, 'g');

export function normalizeazaFiltru(filtru: string): string {
  return filtru.replace(FILTRU_CAUTARE, (_, functie: string, literal: string) => {
    const text = normalizeazaCautare(String(literal).replace(/''/g, "'"));
    return `${functie}(${CAMP_CAUTARE},'${text.replace(/'/g, "''")}')`;
  });
}

// ── store-ul ────────────────────────────────────────────────────────────────

type Request = { url: string; method?: string; params?: Record<string, unknown>; headers?: Record<string, string> };

// `ODataStore` e o clasă DevExtreme (`Class.inherit`), nu o clasă ES: extinderea
// se face cu `.inherit`, iar `this.callBase` e legat DOAR pe durata apelului
// sincron (`wrapOverridden` îl pune și îl scoate în `try/finally`). Cum `byKey`
// trece printr-o promisiune, implementarea de bază se capturează AICI, o dată,
// nu prin `callBase` dintr-un callback asincron — unde ar fi deja `undefined`.
type Amanat = {
  done: (f: (v: unknown) => void) => Amanat;
  fail: (f: (e: unknown) => void) => Amanat;
};
type StoreIntern = {
  _entitate: string;
  _byKeyImpl: (cheie: unknown, extra?: unknown) => Amanat;
};
const byKeyDeBaza = (ODataStore as unknown as { prototype: StoreIntern }).prototype._byKeyImpl;

const StoreCuCache = (ODataStore as unknown as {
  inherit: (membri: Record<string, unknown>) => new (optiuni: unknown) => unknown;
}).inherit({
  ctor(this: StoreIntern & { callBase: (o: unknown) => void }, optiuni: { entitate: string }) {
    this.callBase(optiuni);
    this._entitate = optiuni.entitate;
  },

  // Rezolvarea unei SINGURE valori, prin cache-ul comun. Cheia poartă și
  // `$expand`/`$select`-ul cerut de apelant: eticheta unui lot se compune din
  // `Produs.Denumire` (`expand={['Produs']}`), deci un răspuns cules fără expand
  // ar afișa „(produs nedefinit)" în locul denumirii. O cheie care ignoră
  // proiecția ar face exact genul de minciună tăcută pe care cache-ul trebuie
  // să n-o poată produce.
  _byKeyImpl(this: StoreIntern, cheie: unknown, extra?: unknown) {
    // `when(promisiune)` = `fromPromise` (exportat doar în JS, nu în `.d.ts`):
    // `_withLock` din `abstract_store` cere un Deferred cu `.done`/`.fail`, nu o
    // promisiune nativă.
    return when(cache.fetchQuery({
      queryKey: cheieNomenclator(this._entitate, cheie, cheiaProiectiei(extra)),
      staleTime: Infinity,
      queryFn: () => byKeyBrut(this, cheie, extra),
    }));
  },
});

// Cheia unei valori rezolvate. O poartă și `byKey`-ul widget-urilor, și
// citirile imperative de mai jos — de asta trebuie compusă într-un singur loc.
function cheieNomenclator(entitate: string, id: unknown, proiectie: string) {
  return ['nomenclator', entitate, String(id), proiectie];
}

// `expand`/`select` ale `loadOptions`-urilor cu care vine `byKey`. Formele sunt
// ale DevExtreme (string sau array); se serializează stabil, sortat, ca două
// lookup-uri cu aceeași proiecție scrisă în altă ordine să împartă cache-ul.
function cheiaProiectiei(extra: unknown): string {
  const o = extra as { expand?: unknown; select?: unknown } | null | undefined;
  const lista = (v: unknown) => (v == null ? [] : Array.isArray(v) ? v.map(String) : [String(v)]);
  const parti = [...lista(o?.expand), ...lista(o?.select)].sort();
  return parti.join(',');
}

// Cererea propriu-zisă — implementarea de BAZĂ a store-ului, fără trecerea prin
// cache. E singurul `queryFn` al cheii de mai sus: și override-ul de `byKey`, și
// `citesteNomenclator`, și `useNomenclator` intră prin ea. (Un `queryFn` care ar
// chema la rândul lui `fetchQuery` pe ACEEAȘI cheie s-ar aștepta pe sine.)
//
// 401-ul se tratează aici fiindcă `_addFailHandlers` — cel care cheamă
// `errorHandler`-ul store-ului — stă pe `byKey`, iar citirile imperative nu trec
// neapărat prin el. `expiraSesiunea` e idempotentă, deci dubla tratare pe calea
// widget-ului nu strică nimic.
function byKeyBrut<T>(store: StoreIntern, cheie: unknown, extra?: unknown): Promise<T> {
  return new Promise<T>((resolve, reject) => {
    byKeyDeBaza.call(store, cheie, extra)
      .done((v: unknown) => resolve(v as T))
      .fail((e: unknown) => {
        if ((e as { httpStatus?: number } | null)?.httpStatus === 401) expiraSesiunea();
        reject(e);
      });
  });
}

export type OptiuniStore = {
  // Tipurile GUID pentru serializarea filtrului (`Edm.Guid` fără apostrofuri).
  fieldTypes?: Record<string, 'Guid'>;
};

// Store-ul unei entități OData. O instanță per configurație de widget (ca până
// acum) — partajarea reală e a CACHE-ului de `byKey`, nu a obiectului.
export function storeOData(entitate: string, optiuni: OptiuniStore = {}): ODataStore {
  return new StoreCuCache({
    entitate,
    url: `/api/odata/${entitate}`,
    key: 'ID',
    keyType: 'Guid',
    version: 4,
    fieldTypes: optiuni.fieldTypes,
    // JWT-ul nu poate trece prin `http.ts` aici: cererea o face componenta
    // DevExtreme. Același token, același header, un singur loc — plus
    // normalizarea literalului de căutare, care e tot o proprietate a CERERII.
    beforeSend: (e: Request) => {
      e.headers = { ...e.headers, Authorization: `Bearer ${token() ?? ''}` };
      const filtru = e.params?.$filter;
      if (typeof filtru === 'string' && e.params)
        e.params.$filter = normalizeazaFiltru(filtru);
    },
    // Sesiunea expirată trebuie tratată la fel pe TOATE conductele, nu doar pe
    // `http.ts`: altfel lookup-urile rămân mute (listă goală) după expirare.
    errorHandler: (e: { httpStatus?: number }) => { if (e.httpStatus === 401) expiraSesiunea(); },
  }) as ODataStore;
}

// ── citirea unui rând de nomenclator, în afara unui widget ───────────────────

// Un store per entitate, ținut aici, pentru citirile care NU vin dintr-un
// widget: precompletarea unei linii (F20-D3), o etichetă într-un formular. Trec
// prin ACEEAȘI cheie de cache și prin aceeași deserializare ca lookup-urile.
// Dacă ar face un `fetch` propriu, cache-ul ar ajunge să țină pentru un id când
// forma DevExtreme (`ID` ca obiect `Guid`, datele ca `Date`), când JSON-ul brut,
// după cine a cerut primul; o citire partajată care întoarce două forme e mai
// rea decât două citiri.
const storeuriEtichete = new Map<string, StoreIntern>();

function storeEticheta(entitate: string): StoreIntern {
  let s = storeuriEtichete.get(entitate);
  if (!s) {
    s = storeOData(entitate) as unknown as StoreIntern;
    storeuriEtichete.set(entitate, s);
  }
  return s;
}

// O etichetă cerută de un formular și una cerută de un `SelectBox` costă
// ÎMPREUNĂ o cerere. Proiecția e goală („entitatea, fără expand") — forma pe
// care o cer precompletările.
export function citesteNomenclator<T = Record<string, unknown>>(entitate: string, id: string): Promise<T> {
  return cache.fetchQuery({
    queryKey: cheieNomenclator(entitate, id, ''),
    staleTime: Infinity,
    queryFn: () => byKeyBrut<T>(storeEticheta(entitate), id),
  });
}

export function useNomenclator<T = Record<string, unknown>>(
  entitate: string, id: string | null | undefined): T | undefined {
  return useQuery({
    queryKey: cheieNomenclator(entitate, id, ''),
    queryFn: () => byKeyBrut<T>(storeEticheta(entitate), id!),
    enabled: id != null,
    staleTime: Infinity,
  }).data;
}

// ── sonda de EXISTENȚĂ ──────────────────────────────────────────────────────
// Mutată aici din `nucleu/sonda.ts` (F20-D2): e a aceleiași uși OData. Semantica
// rămâne DISTINCTĂ de citire — „id-ul ăsta e în setul ăsta?" nu aduce entitatea
// și nu depinde de sintaxa de cheie — de aceea are cheie proprie de cache.

// Eșecul PROPAGĂ: „nu știu" trebuie să rămână distinct de „nu e în set". Un
// `false` inventat pe eroare de rețea ar fi un răspuns AFIRMATIV mincinos —
// exact ce promite să nu facă `useSonda` mai jos („undefined cât timp nu
// știm"). Apelanții cad pe default-ul lor sigur prin `data === undefined`
// (retry-ul e oprit global, deci eroarea nu se repetă).
export async function existaInSet(entitate: string, id: string): Promise<boolean> {
  const r = await ia<{ value?: unknown[] }>(
    `/api/odata/${entitate}?$filter=${encodeURIComponent(`ID eq ${id}`)}&$select=ID&$top=1`);
  return (r.value?.length ?? 0) > 0;
}

// Aceeași sondă, ca HOOK — cu cache-ul TanStack pe cheia `['sonda', entitate,
// id]` (forma pe care o folosea deja `FclDetaliu`, 61c). Contează pentru
// viramentul intern (F7-D8): pe ecranul de trezorerie aceeași întrebare („e
// `ContPropriu`?") o pun DOUĂ componente — selectorul de fel al contrapartidei
// și shell-ul, care decide forma ecranului. Cheia comună le face să împartă un
// SINGUR răspuns; fără ea ar pleca două cereri identice la fiecare document.
//
// Întoarce `undefined` cât timp răspunsul nu a venit (sau proba e oprită) —
// apelanții rămân pe default-ul lor care nu minte, nu pe un `false` inventat.
export function useSonda(
  entitate: string, id: string | null | undefined, activa = true): boolean | undefined {
  return useQuery({
    queryKey: ['sonda', entitate, id],
    queryFn: () => existaInSet(entitate, id!),
    enabled: activa && id != null,
    // Apartenența unui id la un nomenclator nu se schimbă în viața ecranului.
    staleTime: Infinity,
  }).data;
}
