import type { components } from '../../generated/api-types';
import { ia, posteaza, pune, sterge } from '../../nucleu/http';
import { storeRemote } from '../../nucleu/dxStore';

// `api.ts` DE MÂNĂ, cu tipurile GENERATE (43d): suprafața feliei e mică și
// deliberată — un client generat ar dicta forma hook-urilor și tot n-ar ajuta
// `CustomStore`. Aici se vede și granița contractului: `Write ≠ Read` în
// TIPURI, deci compilatorul oprește mâna care le-ar încurca.

type Scheme = components['schemas'];

export type BtrRead = Scheme['NotaTransferReadDto'];
export type BtrWrite = Scheme['NotaTransferWriteDto'];
export type BtrLinieRead = Scheme['NotaTransferLinieReadDto'];
export type BtrLinieWrite = Scheme['NotaTransferLinieWriteDto'];
export type OperareRezultat = Scheme['OperareRezultatDto'];

// Numele SCHEMELOR OpenAPI, ca stringuri, pentru `useCampMeta` (required/
// maxLength). Aici, o dată — nu împrăștiate prin JSX.
export const SCHEMA_ANTET = 'NotaTransferWriteDto';
export const SCHEMA_LINIE = 'NotaTransferLinieWriteDto';
// Tipurile de METADATA (entitățile XAF) — sursa captions-urilor.
export const TIP_ANTET = 'NotaTransfer';
export const TIP_LINIE = 'DocumentDetaliu';

const BAZA = '/api/btr';

export const btr = {
  citeste: (id: string) => ia<BtrRead>(`${BAZA}/${id}`),
  creeaza: (dto: BtrWrite) => posteaza<BtrRead>(BAZA, dto),
  actualizeaza: (id: string, dto: BtrWrite) => pune<BtrRead>(`${BAZA}/${id}`, dto),
  sterge: (id: string) => sterge(`${BAZA}/${id}`),

  // Dry-run (43 §2, stratul 2 autoritar): calculează + validează, fără
  // materializare. 200 cu listă goală = trece toți gardienii.
  valideaza: (id: string) => posteaza<{ Erori?: string[] | null }>(`${BAZA}/${id}/valideaza`)
    .then((r) => r.Erori ?? []),

  opereaza: (id: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/opereaza`),
  anuleaza: (id: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/anuleaza`),
  storneaza: (id: string, data: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/storneaza`, { Data: data }),

  // Cheia grilei e `Id` (ReadDto), nu `ID` (entitatea) — DTO-ul e contractul.
  storeLista: () => storeRemote(BAZA, 'Id'),
};

export function antetGol(): BtrWrite {
  return { Data: azi(), Linii: [] };
}

export function azi(): string {
  const d = new Date();
  return `${d.getFullYear()}-${`${d.getMonth() + 1}`.padStart(2, '0')}-${`${d.getDate()}`.padStart(2, '0')}`;
}

// ReadDto → WriteDto: exact câmpurile pe care operatorul are voie să le trimită.
// Restul (Numar, Stare, Valoare, Total) rămân ale serverului.
export function spreWrite(citit: BtrRead): BtrWrite {
  return {
    Data: citit.Data,
    PredatorId: citit.PredatorId,
    PrimitorId: citit.PrimitorId,
    NumarPV: citit.NumarPV,
    DataPV: citit.DataPV,
    Linii: (citit.Linii ?? []).map((l) => ({
      Id: l.Id,
      TipMaterialId: l.TipMaterialId,
      LotId: l.LotId,
      Cantitate: l.Cantitate,
    })),
  };
}
