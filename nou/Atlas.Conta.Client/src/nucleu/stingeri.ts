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

  // Candidații de stins: proiecția de REST, filtrată pe contrapartidă ÎN
  // proiecție (parametru, nu filtru DataSourceLoader — contrapartida e o latură
  // diferită per tip). Grila rămâne remote peste el.
  storeCandidati: (contrapartidaId: string) =>
    storeRemote(`/api/proiectii/documente-cu-rest?contrapartidaId=${encodeURIComponent(contrapartidaId)}`, 'DocumentId'),
};

// Ruta feliei pentru un cod de tip (`CelalaltTip`, `DocumentCuRestRand.Tip`).
// Vocabular ÎNCHIS, scris explicit: tipurile fără felie de client (RDC, NTC,
// ASM…) rămân TEXT, nu link mort. Se extinde odată cu feliile, nu automat —
// DEC a ieșit din listă la felia 8.
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
    default: return null;
  }
}
