import type { components } from '../../generated/api-types';
import { ia, posteaza, pune, sterge } from '../../nucleu/http';
import { storeRemote } from '../../nucleu/dxStore';
import { azi } from '../../nucleu/zi';

// `CasWriteDto` n-are linii: serverul le produce și PUT-ul le re-produce (F26-D6, 42c).

type Scheme = components['schemas'];

export type CasRead = Scheme['CasReadDto'];
export type CasWrite = Scheme['CasWriteDto'];
export type CasLinieRead = Scheme['CasLinieReadDto'];
export type CasListRand = Scheme['CasListDto'];
export type OperareRezultat = Scheme['OperareRezultatDto'];

// Numele SCHEMELOR OpenAPI pentru `campMeta` (required/maxLength).
export const SCHEMA_ANTET = 'CasWriteDto';
export const SCHEMA_LISTA = 'CasListDto';
// Tipurile de METADATA (entitățile XAF) — sursa captions-urilor.
export const TIP_ANTET = 'IesireImobilizare';
export const TIP_LINIE = 'IesireImobilizareDetaliu';

const BAZA = '/api/cas';

export const cas = {
  citeste: (id: string) => ia<CasRead>(`${BAZA}/${id}`),
  creeaza: (dto: CasWrite) => posteaza<CasRead>(BAZA, dto),
  actualizeaza: (id: string, dto: CasWrite) => pune<CasRead>(`${BAZA}/${id}`, dto),
  sterge: (id: string) => sterge(`${BAZA}/${id}`),

  valideaza: (id: string) => posteaza<{ Erori?: string[] | null }>(`${BAZA}/${id}/valideaza`)
    .then((r) => r.Erori ?? []),

  opereaza: (id: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/opereaza`),
  anuleaza: (id: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/anuleaza`),
  storneaza: (id: string, data: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/storneaza`, { Data: data }),

  storeLista: () => storeRemote(BAZA, 'Id'),
};

export function antetGol(): CasWrite {
  return { Data: azi(), Cauza: 'Casare', Fise: [] };
}

// ReadDto → WriteDto. `Fise` la citire e DERIVAT din linii, ordonat după
// numărul de inventar: o fișă care n-ar produce nicio linie n-ar apărea la
// re-citire, iar PUT-ul e agregatul întreg.
export function spreWrite(citit: CasRead): CasWrite {
  return {
    Data: citit.Data,
    Cauza: citit.Cauza,
    PredatorId: citit.PredatorId,
    PrimitorId: citit.PrimitorId,
    Fise: citit.Fise ?? [],
  };
}
