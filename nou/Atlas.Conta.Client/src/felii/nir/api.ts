import type { components } from '../../generated/api-types';
import { ia, posteaza } from '../../nucleu/http';
import { storeRemote } from '../../nucleu/dxStore';

// NIR = CITIRE + COMENZI (F2-D3). Nu există `NirWriteDto` în contract, deci nu
// există nici aici: absența scrierii se vede în TIPURI, nu doar în documentație.
// NIR-ul de producție e clona conexă pe care motorul o generează la operarea
// facturii; culegerea manuală e felie separată, pur aditivă.

type Scheme = components['schemas'];

export type NirRead = Scheme['NirReadDto'];
export type NirLinieRead = Scheme['NirLinieReadDto'];
export type OperareRezultat = Scheme['OperareRezultatDto'];

export const SCHEMA_ANTET = 'NirReadDto';
export const SCHEMA_LINIE = 'NirLinieReadDto';
// Tipurile de METADATA (entitățile XAF) — sursa captions-urilor.
export const TIP_ANTET = 'NIR';
export const TIP_LINIE = 'NirDetaliu';

const BAZA = '/api/nir';

export const nir = {
  citeste: (id: string) => ia<NirRead>(`${BAZA}/${id}`),

  valideaza: (id: string) => posteaza<{ Erori?: string[] | null }>(`${BAZA}/${id}/valideaza`)
    .then((r) => r.Erori ?? []),

  opereaza: (id: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/opereaza`),
  anuleaza: (id: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/anuleaza`),
  storneaza: (id: string, data: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/storneaza`, { Data: data }),

  storeLista: () => storeRemote(BAZA, 'Id'),
};
