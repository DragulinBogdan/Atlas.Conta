import type { components } from '../../generated/api-types';
import { ia, posteaza, pune, sterge } from '../../nucleu/http';
import { storeRemote } from '../../nucleu/dxStore';
import { azi } from '../../nucleu/zi';
import { urlCu } from '../../nucleu/urlStare';

// Agregat cules pe șablonul DVI (43d); `TipMaterialId` al liniei îl pune serverul din fișă (F26-D5).

type Scheme = components['schemas'];

export type PifRead = Scheme['PifReadDto'];
export type PifWrite = Scheme['PifWriteDto'];
export type PifLinieRead = Scheme['PifLinieReadDto'];
export type PifLinieWrite = Scheme['PifLinieWriteDto'];
export type PifListRand = Scheme['PifListDto'];
export type LiniiSursa = Scheme['LiniiSursaDto'];
export type LinieSursaCandidata = Scheme['LinieSursaCandidataDto'];
export type OperareRezultat = Scheme['OperareRezultatDto'];
export type FisaImobilizare = Scheme['FisaImobilizareDto'];
export type SituatieImobilizare = Scheme['SituatieImobilizareDto'];

// Numele SCHEMELOR OpenAPI, ca stringuri, pentru `campMeta` (required/maxLength).
export const SCHEMA_ANTET = 'PifWriteDto';
export const SCHEMA_LINIE = 'PifLinieWriteDto';
export const SCHEMA_LISTA = 'PifListDto';
// Tipurile de METADATA (entitățile XAF) — sursa captions-urilor.
export const TIP_ANTET = 'PunereInFunctiune';
export const TIP_LINIE = 'PunereInFunctiuneDetaliu';

const BAZA = '/api/pif';

export type CerereLiniiSursa = {
  dataStart: string;
  dataEnd: string;
  partenerId?: string | null;
  // Fără bifă, serverul sare candidații consumați integral (`Rest <= 0`).
  toate?: boolean;
};

export const pif = {
  citeste: (id: string) => ia<PifRead>(`${BAZA}/${id}`),
  creeaza: (dto: PifWrite) => posteaza<PifRead>(BAZA, dto),
  actualizeaza: (id: string, dto: PifWrite) => pune<PifRead>(`${BAZA}/${id}`, dto),
  sterge: (id: string) => sterge(`${BAZA}/${id}`),

  // Dry-run: 200 cu listă goală = trece toți gardienii.
  valideaza: (id: string) => posteaza<{ Erori?: string[] | null }>(`${BAZA}/${id}/valideaza`)
    .then((r) => r.Erori ?? []),

  opereaza: (id: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/opereaza`),
  anuleaza: (id: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/anuleaza`),
  storneaza: (id: string, data: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/storneaza`, { Data: data }),

  // Liniile de factură de clasă F care pot hrăni o punere în funcțiune (F26-D5).
  // Perioada e OBLIGATORIE (serverul refuză cu 400 fără ea); plicul spune prin
  // `MaiSunt` că lista s-a tăiat la plafon și perioada trebuie îngustată.
  liniiSursa: (cerere: CerereLiniiSursa) =>
    ia<LiniiSursa>(`${BAZA}/linii-sursa?${parametriLiniiSursa(cerere)}`),

  // Cheia grilei e `Id` (ReadDto), nu `ID` (entitatea) — DTO-ul e contractul.
  storeLista: () => storeRemote(BAZA, 'Id'),
};

// Fișa unei imobilizări la o dată — indiciile editorului de linie (banda
// clasificării, starea, situația de la care pornește o modernizare sau o
// revizuire). Citire proprie feliei: ecranul de nomenclator are a lui, iar
// felia PIF nu depinde de el.
export function citesteFisa(id: string, laData?: string | null): Promise<FisaImobilizare> {
  return ia<FisaImobilizare>(urlCu(`/api/imobilizari/${id}/fisa`, { laData: laData ?? null }));
}

// Parametrii ABSENȚI nu se trimit: un `partenerId=` gol ar pleca pe sârmă ca
// GUID invalid.
function parametriLiniiSursa(cerere: CerereLiniiSursa): string {
  const p = new URLSearchParams({ dataStart: cerere.dataStart, dataEnd: cerere.dataEnd });
  if (cerere.partenerId) p.set('partenerId', cerere.partenerId);
  if (cerere.toate) p.set('toate', 'true');
  return p.toString();
}

export function antetGol(): PifWrite {
  return { Data: azi(), Linii: [] };
}

// Linie NOUĂ: felul implicit e `Intrare` (cazul curent), iar cifrele inițiale
// pornesc de la 0 — serverul le refuză oriunde altundeva decât pe intrarea fără
// linie sursă, deci zero e singura valoare neutră.
export function linieGoala(): PifLinieWrite {
  return {
    Fel: 'Intrare',
    Valoare: 0,
    AmortizareInitiala: 0,
    AmortizareFiscalaInitiala: 0,
    LuniAmortizateInitial: 0,
  };
}

// ReadDto → WriteDto: EXACT câmpurile pe care operatorul are voie să le trimită.
// Ce lipsește e server-owned prin OMISIUNE: `Numar`, `Stare`, `Total`,
// `TipMaterialId` (al fișei), etichetele și afordanțele. Toate liniile trec
// înapoi: una absentă din PUT s-ar ȘTERGE (43c).
export function spreWrite(citit: PifRead): PifWrite {
  return {
    Data: citit.Data,
    PredatorId: citit.PredatorId,
    PrimitorId: citit.PrimitorId,
    Linii: (citit.Linii ?? []).map((l) => ({
      Id: l.Id,
      ImobilizareId: l.ImobilizareId,
      Fel: l.Fel,
      LinieSursaId: l.LinieSursaId,
      Valoare: l.Valoare,
      ValoareFiscala: l.ValoareFiscala,
      AmortizareInitiala: l.AmortizareInitiala,
      AmortizareFiscalaInitiala: l.AmortizareFiscalaInitiala,
      LuniAmortizateInitial: l.LuniAmortizateInitial,
      Metoda: l.Metoda,
      DurataLuni: l.DurataLuni,
      ValoareReziduala: l.ValoareReziduala,
      MetodaFiscala: l.MetodaFiscala,
      DurataFiscalaLuni: l.DurataFiscalaLuni,
      CategorieFiscala: l.CategorieFiscala,
      UtilizareExclusiva: l.UtilizareExclusiva,
    })),
  };
}
