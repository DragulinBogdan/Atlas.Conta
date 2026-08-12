import type { components } from '../../generated/api-types';
import { ia, posteaza, pune, sterge } from '../../nucleu/http';
import { storeRemote } from '../../nucleu/dxStore';
import { azi } from '../../nucleu/zi';

// `api.ts` DE MÂNĂ, cu tipurile GENERATE (43d) — șablonul NIR. Ce e PROPRIU
// feliei LDI stă tot aici, ca să nu se împrăștie prin JSX:
//   • `Numar` NU se culege: LDI are politică de numerotare („LDI-") ⇒ seria e a
//     OPERĂRII, server-owned, și nici nu există în `LdiWriteDto` (F6-D4);
//   • **DIRECȚIA conduce linia**: un plus și un minus n-au aceleași câmpuri, iar
//     serverul aplică două contracte diferite pe aceeași frunză (F6-D3). Pe
//     minus se aplică pinul `LotId` și se GOLESC câmpurile plusului; pe plus
//     `ProdusId` naște lotul, iar `LotId` din payload se IGNORĂ (server-owned,
//     F6-D5) — de aceea round-trip-ul lui e inofensiv;
//   • fără TVA (F6-D5): LDI n-are `PoliticaTva` în niciun profil;
//   • `Valoare` e REZULTAT și e SEMNATĂ de server încă de la culegere (F6-D6):
//     minusul negativ, plusul pozitiv, deci `Total`-ul draftului arată efectul
//     NET al inventarului. Cantitatea rămâne pozitivă la culegere — semnarea ei
//     e fapta OPERĂRII (28a), iar pe un document operat ReadDto o dă semnată.
//
// Laturile: predatorul e GESTIUNEA INVENTARIATĂ (tot din ea se nasc loturile
// plusurilor — `GestiuneLoturiCulese`, F6-D2), primitorul e COMISIA de
// inventariere (intern cu calitatea `Comisie` — 28d).

type Scheme = components['schemas'];

export type LdiRead = Scheme['LdiReadDto'];
export type LdiWrite = Scheme['LdiWriteDto'];
export type LdiLinieRead = Scheme['LdiLinieReadDto'];
export type LdiLinieWrite = Scheme['LdiLinieWriteDto'];
export type OperareRezultat = Scheme['OperareRezultatDto'];

// Numele SCHEMELOR OpenAPI, ca stringuri, pentru `campMeta` (required/maxLength).
// Câmpurile doar-de-afișare ale antetului (`Numar`, `Stare`, `DataOperare`) nu
// există în WriteDto — caption-ul lor vine din `metadata.json`, iar lipsa din
// schemă îi lasă corect NEobligatorii.
export const SCHEMA_ANTET = 'LdiWriteDto';
export const SCHEMA_LINIE = 'LdiLinieWriteDto';
// Tipurile de METADATA (entitățile XAF) — sursa captions-urilor. Linia e frunza
// (`ListaDiferenteInventarDetaliu`): direcția, produsul, prețul de evaluare,
// atributele de lot și `CodEconomic` sunt ale ei (F6-D2 / DIM-2).
export const TIP_ANTET = 'ListaDiferenteInventar';
export const TIP_LINIE = 'ListaDiferenteInventarDetaliu';

const BAZA = '/api/ldi';

export const ldi = {
  citeste: (id: string) => ia<LdiRead>(`${BAZA}/${id}`),
  creeaza: (dto: LdiWrite) => posteaza<LdiRead>(BAZA, dto),
  actualizeaza: (id: string, dto: LdiWrite) => pune<LdiRead>(`${BAZA}/${id}`, dto),
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

export function antetGol(): LdiWrite {
  return { Data: azi(), Linii: [] };
}

// Linie NOUĂ fără direcție: enum-ul n-are default valid (28e) — operatorul o
// alege, iar ea decide restul câmpurilor.
export function linieGoala(): LdiLinieWrite {
  return { Cantitate: 0 };
}

// ReadDto → WriteDto: EXACT câmpurile pe care operatorul are voie să le trimită.
// Ce lipsește de aici e server-owned prin OMISIUNE, nu prin uitare: `Numar`
// (seria „LDI-", asignată la materializare), `Stare`, `Valoare`, `Total` și
// etichetele/affordances (proiecții de citire).
//
// `AngajamentId` face round-trip FĂRĂ editor (ca la NIR — restanța 62g): dacă
// linia îl are din altă cale (import, ecranul XAF), un PUT din client nu i-l
// șterge.
export function spreWrite(citit: LdiRead): LdiWrite {
  return {
    Data: citit.Data,
    PredatorId: citit.PredatorId,
    PrimitorId: citit.PrimitorId,
    Linii: (citit.Linii ?? []).map((l) => ({
      Id: l.Id,
      Directie: l.Directie,
      TipMaterialId: l.TipMaterialId,
      // Mecanismul lotului de PLUS (F6-D2), nu o etichetă.
      ProdusId: l.ProdusId,
      // Pinul lotului DESCĂRCAT, aplicat doar pe minus (F6-D5); pe plus îl
      // ignoră serverul, deci ecoul lui e inofensiv.
      LotId: l.LotId,
      Cantitate: l.Cantitate,
      PretEvaluare: l.PretEvaluare,
      DataExpirare: l.DataExpirare,
      LotFabricatie: l.LotFabricatie,
      CodEconomicId: l.CodEconomicId,
      AngajamentId: l.AngajamentId,
    })),
  };
}
