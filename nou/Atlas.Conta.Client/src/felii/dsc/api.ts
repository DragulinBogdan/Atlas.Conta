import type { components } from '../../generated/api-types';
import { ia, posteaza } from '../../nucleu/http';
import { storeRemote } from '../../nucleu/dxStore';

// DSC = CITIRE + COMENZI (F4-D2). Nu există `DscWriteDto` în contract, deci nu
// există nici aici: absența scrierii se vede în TIPURI, nu doar în documentație.
// Descărcarea se NAȘTE exclusiv prin `DescarcareService` — automat, în tranzacția
// operării facturii, sau prin comanda de backorder de pe ruta FCL. Liniile ei
// sunt rezultatul pickingului pe loturi (pin întâi, apoi FIFO), nu o culegere.

type Scheme = components['schemas'];

export type DscRead = Scheme['DscReadDto'];
export type DscLinieRead = Scheme['DscLinieReadDto'];
export type OperareRezultat = Scheme['OperareRezultatDto'];

export const SCHEMA_ANTET = 'DscReadDto';
export const SCHEMA_LINIE = 'DscLinieReadDto';
// Tipurile de METADATA (entitățile XAF) — sursa captions-urilor.
export const TIP_ANTET = 'DescarcareGestiune';
export const TIP_LINIE = 'DescarcareGestiuneDetaliu';

const BAZA = '/api/dsc';

export const dsc = {
  citeste: (id: string) => ia<DscRead>(`${BAZA}/${id}`),

  valideaza: (id: string) => posteaza<{ Erori?: string[] | null }>(`${BAZA}/${id}/valideaza`)
    .then((r) => r.Erori ?? []),

  opereaza: (id: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/opereaza`),
  anuleaza: (id: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/anuleaza`),
  storneaza: (id: string, data: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/storneaza`, { Data: data }),

  storeLista: () => storeRemote(BAZA, 'Id'),
};
