import type { components } from '../generated/api-types';
import { ia, posteaza, sterge } from './http';
import { storeRemote } from './dxStore';

// `api.ts` DE MÂNĂ pentru STINGERE (43d), dar în NUCLEU, nu într-o felie:
// resursa e a DOCUMENTULUI, nu a trezoreriei — același panou apare pe PLT, pe
// INC și pe FCT (și, când nota de compensare va veni prin API, pe NTC). Felia
// care-l montează îi dă doar identitatea: cine e documentul, cine e
// contrapartida și pe ce ROL stă (stinge / e stins).
//
// Contractul e cel din F3-D3: trei verbe. Nu există PUT — `Imperechere` nu e
// document (n-are Draft/Operat, deci nici agregat de reconciliat).

type Scheme = components['schemas'];

export type Stingeri = Scheme['StingeriDto'];
export type StingereRand = Scheme['StingereRandDto'];
export type ImperechereWrite = Scheme['ImperechereWriteDto'];
export type DocumentCuRest = Scheme['DocumentCuRestRand'];

const BAZA = '/api/imperecheri';

export const stingeri = {
  // Un singur apel pentru tot panoul: Total/Asignat/Rămas (calculate de
  // `ImperechereService` — TS nu însumează nimic) + rândurile cu partea opusă.
  citeste: (documentId: string) => ia<Stingeri>(`${BAZA}/${documentId}/stingeri`),

  creeaza: (dto: ImperechereWrite) => posteaza<{ Id?: string }>(BAZA, dto),

  // Liberă (31d): legătura n-are registre proprii, iar dispariția ei redeschide
  // anularea/stornarea ambelor documente (affordances oneste — F3-D2).
  sterge: (id: string) => sterge(`${BAZA}/${id}`),

  // Candidații de stins: proiecția de REST, filtrată pe contrapartidă ȘI pe
  // SENS ÎN proiecție (parametri, nu filtre DataSourceLoader — contrapartida e
  // o latură diferită per tip, iar sensul e funcție de TIPUL documentului stins,
  // deci polimorf). Grila rămâne remote peste el.
  //
  // `sens` NU se deduce aici (42c): e literalul server-computed
  // `StingeriDto.SensCandidati` (`Opus(SensDeStins)`), pasat ca atare. `null` ⇒
  // parametrul lipsește din URL și proiecția nu filtrează pe sens — exact
  // comportamentul de dinainte de F19-D16, pentru tipurile care nu declară sens.
  // O valoare necunoscută e refuzată de rută cu 400, nu ignorată tăcut (57a).
  storeCandidati: (contrapartidaId: string, sens?: string | null) =>
    storeRemote(
      `/api/proiectii/documente-cu-rest?contrapartidaId=${encodeURIComponent(contrapartidaId)}`
      + (sens ? `&sens=${encodeURIComponent(sens)}` : ''),
      'DocumentId'),
};

// Ruta feliei pentru un cod de tip (`CelalaltTip`, `DocumentCuRestRand.Tip`,
// `DocumentTip` din fișă/jurnal). Vocabular ÎNCHIS, scris explicit: tipurile
// fără felie de client (ITV…) rămân TEXT, nu link mort. Se extinde
// odată cu feliile, nu automat — DEC a ieșit din listă la felia 8, iar NTC,
// ASM, RLF și RDC la felia 19.
export function rutaTip(tip: string | null | undefined, id: string): string | null {
  switch (tip) {
    case 'FCT': return `/fct/${id}`;
    case 'FCL': return `/fcl/${id}`;
    case 'NIR': return `/nir/${id}`;
    case 'DSC': return `/dsc/${id}`;
    case 'PLT': return `/plt/${id}`;
    case 'INC': return `/inc/${id}`;
    case 'BTR': return `/btr/${id}`;
    case 'BCS': return `/bcs/${id}`;
    case 'LDI': return `/ldi/${id}`;
    case 'DEC': return `/dec/${id}`;
    // F19: nota de compensare devine link real în panourile existente (era
    // exact cazul numit în comentariul lui `Celalalt`), iar asamblarea în fișa
    // de cont și în jurnal.
    case 'NTC': return `/ntc/${id}`;
    case 'ASM': return `/asm/${id}`;
    // Retururile au ecrane proprii de la felia 19. NU sunt stingători și nu
    // apar în `DocumenteCuRest` (F19-D11) — dar apar în fișa de cont, în jurnal
    // și ca `CelalaltTip` al unei stingeri făcute pe altă cale (notă, import),
    // și acolo trebuie să fie link, nu text.
    case 'RLF': return `/rlf/${id}`;
    case 'RDC': return `/rdc/${id}`;
    default: return null;
  }
}
