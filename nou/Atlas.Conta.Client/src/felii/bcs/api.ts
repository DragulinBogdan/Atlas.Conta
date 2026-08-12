import type { components } from '../../generated/api-types';
import { ia, posteaza, pune, sterge } from '../../nucleu/http';
import { storeRemote } from '../../nucleu/dxStore';
import { azi } from '../../nucleu/zi';

// `api.ts` DE MÂNĂ, cu tipurile GENERATE (43d) — șablonul BTR, cu care BCS-ul e
// geamăn ca formă de culegere (tip + lot + cantitate). Ce e PROPRIU feliei:
//   • `Numar` NU se culege: BCS are politică de numerotare („BCS-") ⇒ seria e a
//     OPERĂRII, server-owned, și nici nu există în `BcsWriteDto` (F6-D4);
//   • fără `NumarPV`/`DataPV`: bonul de consum nu e `IDocumentCuPV` (BTR e);
//   • fără TVA (F6-D5): BCS n-are `PoliticaTva` în niciun profil, deci un TVA
//     cules aici ar fi cifră moartă;
//   • `Valoare` e REZULTAT: serverul o materializează LA CULEGERE (F6-D6) din
//     prețul lotului ales — nu se calculează niciodată în TS.
//
// Laturile: predatorul e GESTIUNEA din care iese marfa, primitorul e LOCUL DE
// CONSUM (intern cu calitatea `LocConsum`) — consumul nu „dispare", rămâne pe
// responsabilul locului (27a). Lookup-ul nu filtrează pe calitate (F6-D8):
// autoritatea e motorul.

type Scheme = components['schemas'];

export type BcsRead = Scheme['BcsReadDto'];
export type BcsWrite = Scheme['BcsWriteDto'];
export type BcsLinieRead = Scheme['BcsLinieReadDto'];
export type BcsLinieWrite = Scheme['BcsLinieWriteDto'];
export type OperareRezultat = Scheme['OperareRezultatDto'];

// Numele SCHEMELOR OpenAPI, ca stringuri, pentru `campMeta` (required/maxLength).
export const SCHEMA_ANTET = 'BcsWriteDto';
export const SCHEMA_LINIE = 'BcsLinieWriteDto';
// Tipurile de METADATA (entitățile XAF) — sursa captions-urilor. Linia BCS n-are
// frunză proprie (DIM-2: nu culege nicio dimensiune), deci e detaliul de BAZĂ.
export const TIP_ANTET = 'BonConsum';
export const TIP_LINIE = 'DocumentDetaliu';

const BAZA = '/api/bcs';

export const bcs = {
  citeste: (id: string) => ia<BcsRead>(`${BAZA}/${id}`),
  creeaza: (dto: BcsWrite) => posteaza<BcsRead>(BAZA, dto),
  actualizeaza: (id: string, dto: BcsWrite) => pune<BcsRead>(`${BAZA}/${id}`, dto),
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

export function antetGol(): BcsWrite {
  return { Data: azi(), Linii: [] };
}

export function linieGoala(): BcsLinieWrite {
  return { Cantitate: 0 };
}

// ReadDto → WriteDto: EXACT câmpurile pe care operatorul are voie să le trimită.
// Ce lipsește de aici e server-owned prin OMISIUNE, nu prin uitare: `Numar`
// (seria „BCS-", asignată la materializare), `Stare`, `Valoare`, `Total` și
// etichetele/affordances (proiecții de citire).
export function spreWrite(citit: BcsRead): BcsWrite {
  return {
    Data: citit.Data,
    PredatorId: citit.PredatorId,
    PrimitorId: citit.PrimitorId,
    Linii: (citit.Linii ?? []).map((l) => ({
      Id: l.Id,
      TipMaterialId: l.TipMaterialId,
      LotId: l.LotId,
      Cantitate: l.Cantitate,
    })),
  };
}
