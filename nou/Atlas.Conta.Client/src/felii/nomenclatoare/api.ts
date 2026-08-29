import { useQuery, type QueryClient } from '@tanstack/react-query';
import { ia, modifica, posteaza, sterge } from '../../nucleu/http';

// Conducta de SCRIERE a nomenclatoarelor (F20-D8). Primul ecran din client care
// scrie prin OData: documentele merg pe REST, ca agregat întreg (42d), fiindcă
// acolo există un agregat — antet + linii, reconciliate server-side. Un
// nomenclator n-are așa ceva: e o entitate plată, cu ușa ei deja deschisă
// (`options.BusinessObject<Partener>()`), iar a-i inventa un controller REST ar
// fi însemnat un al doilea drum către aceleași câmpuri, cu alte mesaje.
//
// Ce cere asta de la client, măsurat pe host înainte de a scrie un rând de UI:
//   • `POST api/odata/<Entitate>`        ⇒ 201 + entitatea, în corp;
//   • `PATCH api/odata/<Entitate>(<id>)` ⇒ 204 No Content, FĂRĂ `Prefer` și
//     fără `If-Match` (`DataController.Patch` întoarce `Updated(...)`);
//   • `DELETE api/odata/<Entitate>(<id>)`⇒ 200;
//   • refuzul gardianului                ⇒ `422 {Erori:[…]}` (F20-D7), tradus
//     de `http.ts` exact ca pe REST.
// Antiforgery: `DataController` poartă `[ValidateAntiForgeryToken]`, dar cererile
// cu `Authorization: Bearer` trec (probat pe host: 201/204/200) — nu e nevoie de
// niciun token în plus.

// Cheia OData a unui rând. GUID-ul se scrie NEÎNCADRAT în V4 (`Partener(0000…)`),
// nu `guid'…'` — forma V3 ar da 404.
export function caleRand(entitate: string, id: string): string {
  return `/api/odata/${entitate}(${id})`;
}

export function citesteRand<T>(entitate: string, id: string): Promise<T> {
  return ia<T>(caleRand(entitate, id));
}

export function creeazaRand<T>(entitate: string, dto: unknown): Promise<T> {
  return posteaza<T>(`/api/odata/${entitate}`, dto);
}

export function modificaRand(entitate: string, id: string, dto: unknown): Promise<void> {
  return modifica<void>(caleRand(entitate, id), dto);
}

export function stergeRand(entitate: string, id: string): Promise<void> {
  return sterge(caleRand(entitate, id));
}

// ── citirea rândului editat ─────────────────────────────────────────────────

// De ce o cheie PROPRIE și nu `['nomenclator', entitate, id]` (cache-ul lui
// `byKey` din F20-D2): acolo stă forma DESERIALIZATĂ de `ODataStore` — `ID` ca
// obiect `Guid` DevExtreme, datele ca `Date`. Formularul de aici trimite înapoi
// exact ce a citit, deci are nevoie de JSON-ul BRUT. Două forme sub aceeași
// cheie ar fi făcut cache-ul să întoarcă „când una, când alta", după cine a
// cerut primul — exact minciuna tăcută pe care F20-D2 a scris-o ca s-o evite.
//
// Consecința e că invalidarea are DOUĂ ținte (vezi `invalideaza`): rândul editat
// și eticheta lui din lookup-uri.
export function cheieRand(entitate: string, id: string | undefined) {
  return ['nomenclator-rand', entitate, id];
}

export function useRand<T>(entitate: string, id: string | undefined) {
  return useQuery({
    queryKey: cheieRand(entitate, id),
    queryFn: () => citesteRand<T>(entitate, id!),
    enabled: id != null,
  });
}

// Ce se invalidează după orice scriere: rândul (forma brută), eticheta lui în
// toate lookup-urile deschise (`['nomenclator', entitate, …]`, cheia lui
// `byKey`) și sonda de existență. Redenumirea unui partener trebuie să se vadă
// pe ecranul de document care tocmai îl afișează, nu abia la următorul refresh
// de pagină — cache-ul acela are `staleTime: Infinity`.
export function invalideaza(cache: QueryClient, entitate: string, id?: string): void {
  void cache.invalidateQueries({ queryKey: cheieRand(entitate, id) });
  void cache.invalidateQueries({ queryKey: ['nomenclator', entitate] });
  void cache.invalidateQueries({ queryKey: ['sonda', entitate] });
}

// ── corpul unei scrieri ─────────────────────────────────────────────────────

// PATCH-ul OData e o DELTĂ: ce lipsește din corp rămâne neatins pe server. Iar
// editorii de text ai clientului scriu `undefined` pe câmp golit
// (`(e.value as string) || undefined` — `campuri.tsx`), iar `JSON.stringify`
// aruncă `undefined`. Compunerea celor două ar fi dat exact bug-ul tăcut
// „operatorul șterge localitatea, salvează, localitatea rămâne".
//
// De aceea corpul se compune EXPLICIT din câmpurile pe care le stăpânește
// formularul, cu `undefined` ⇒ `null`: „câmpul ăsta e acum gol" pleacă pe sârmă
// ca golire, nu ca tăcere. Câmpurile server-owned nu sunt în listă, deci nu pot
// pleca deloc (gardianul le-ar refuza oricum — 422).
export function corpScriere<T extends object>(valoare: T, campuri: (keyof T & string)[]): Record<string, unknown> {
  const corp: Record<string, unknown> = {};
  for (const camp of campuri) {
    const v = valoare[camp];
    corp[camp] = v === undefined || v === '' ? null : v;
  }
  return corp;
}

// `ODataStore` deserializează `Edm.Guid` ca OBIECT `Guid` DevExtreme, nu ca
// string — pe rândurile venite prin grilă (nu prin `ia`) cheia trebuie adusă la
// forma de sârmă înainte de a intra într-o rută.
export function idRand(rand: unknown): string {
  return String((rand as { ID?: unknown } | null)?.ID ?? '');
}
