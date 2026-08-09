import type { components } from '../../generated/api-types';
import { ia, posteaza, pune, sterge } from '../../nucleu/http';
import { storeRemote } from '../../nucleu/dxStore';
import { azi } from '../../nucleu/zi';

// `api.ts` DE MÂNĂ, cu tipurile GENERATE (43d) — șablonul FCT. Ce e PROPRIU
// feliei FCL stă tot aici, ca să nu se împrăștie prin JSX:
//   • `Numar` NU se culege: FCL are politică de numerotare („FCL-", serie
//     fiscală) ⇒ server-owned — nici nu există în `FacturaIesireWriteDto`
//     (invers față de FCT, unde numărul e al furnizorului);
//   • linia poartă PINUL de lot (`LotId`) — cules, spre deosebire de FCT unde
//     lotul se naște server-side din produs (53a);
//   • singura dimensiune a frunzei e `CodEconomicId` (DIM-2);
//   • `ValoareTva` circulă doar ca OVERRIDE (vezi `spreWrite`), ca la FCT;
//   • BACKORDER-ul (F4-D3/D4): două verbe în plus — acoperirea per linie și
//     generarea manuală a descărcării. Ambele sunt ale SERVERULUI: clientul nu
//     calculează niciodată rest, acoperire sau spargere pe loturi.

type Scheme = components['schemas'];

export type FclRead = Scheme['FacturaIesireReadDto'];
export type FclWrite = Scheme['FacturaIesireWriteDto'];
export type FclLinieRead = Scheme['FacturaIesireLinieReadDto'];
export type FclLinieWrite = Scheme['FacturaIesireLinieWriteDto'];
export type DocumentCopil = Scheme['DocumentCopilDto'];
export type OperareRezultat = Scheme['OperareRezultatDto'];
export type RestNedescarcatRand = Scheme['RestNedescarcatRandDto'];
export type GenerareDescarcareRezultat = Scheme['GenerareDescarcareRezultatDto'];

// Numele SCHEMELOR OpenAPI, ca stringuri, pentru `campMeta` (required/maxLength).
export const SCHEMA_ANTET = 'FacturaIesireWriteDto';
export const SCHEMA_LINIE = 'FacturaIesireLinieWriteDto';
// Tipurile de METADATA (entitățile XAF) — sursa captions-urilor. Linia e frunza
// (`FacturaIesireDetaliu`): `Produs`, `PretUnitar`, `Descriere` și `CodEconomic`
// sunt ale ei (DIM-2).
export const TIP_ANTET = 'FacturaIesire';
export const TIP_LINIE = 'FacturaIesireDetaliu';

const BAZA = '/api/fcl';

export const fcl = {
  citeste: (id: string) => ia<FclRead>(`${BAZA}/${id}`),
  creeaza: (dto: FclWrite) => posteaza<FclRead>(BAZA, dto),
  actualizeaza: (id: string, dto: FclWrite) => pune<FclRead>(`${BAZA}/${id}`, dto),
  sterge: (id: string) => sterge(`${BAZA}/${id}`),

  // Dry-run (43 §2, stratul 2 autoritar): calculează + validează, fără
  // materializare. 200 cu listă goală = trece toți gardienii.
  valideaza: (id: string) => posteaza<{ Erori?: string[] | null }>(`${BAZA}/${id}/valideaza`)
    .then((r) => r.Erori ?? []),

  // Operarea FCL poate întoarce `ConexId` — descărcarea de gestiune generată de
  // motor în aceeași tranzacție (`GenereazaSecundar`, P2). Clientul o deschide
  // pe `/dsc/{id}`.
  opereaza: (id: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/opereaza`),
  anuleaza: (id: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/anuleaza`),
  storneaza: (id: string, data: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/storneaza`, { Data: data }),

  // BACKORDER (F4-D3): generarea MANUALĂ a descărcării pentru restul neacoperit
  // — geamănul acțiunii XAF. `DscId` null în răspuns nu e eroare: o comandă
  // nelivrată încă rămâne nelivrată.
  genereazaDescarcarea: (id: string, data: string) =>
    posteaza<GenerareDescarcareRezultat>(`${BAZA}/${id}/genereaza-descarcare`, { Data: data }),

  // Acoperirea per linie de stoc (F4-D4): TOATE liniile, inclusiv cele acoperite
  // integral — tabelul arată starea acoperirii, nu doar lipsa.
  restNedescarcat: (id: string) => ia<RestNedescarcatRand[]>(`${BAZA}/${id}/rest-nedescarcat`),

  // Cheia grilei e `Id` (ReadDto), nu `ID` (entitatea) — DTO-ul e contractul.
  storeLista: () => storeRemote(BAZA, 'Id'),
};

export function antetGol(): FclWrite {
  return { Data: azi(), Linii: [] };
}

export function linieGoala(): FclLinieWrite {
  return { Cantitate: 0, PretUnitar: 0 };
}

// ReadDto → WriteDto: exact câmpurile pe care operatorul are voie să le trimită.
// Restul (Numar, Stare, Valoare, Total, etichete, affordances) rămân ale
// serverului.
//
// `ValoareTva` este DELIBERAT absentă (ca la FCT): pe sârmă ea înseamnă
// „override manual", iar re-trimiterea valorii citite ar îngheța TVA-ul.
// Override-ul îl pune editorul de linie, doar când operatorul chiar atinge
// câmpul.
export function spreWrite(citit: FclRead): FclWrite {
  return {
    Data: citit.Data,
    PredatorId: citit.PredatorId,
    PrimitorId: citit.PrimitorId,
    DataScadenta: citit.DataScadenta,
    GestiuneDescarcareId: citit.GestiuneDescarcareId,
    Linii: (citit.Linii ?? []).map((l) => ({
      Id: l.Id,
      TipMaterialId: l.TipMaterialId,
      // „General!" — identitatea liniei de stoc (P2 §4/37d).
      ProdusId: l.ProdusId,
      // „Specific?" — pinul de lot: opțional, dar CULES, deci face round-trip.
      LotId: l.LotId,
      Descriere: l.Descriere,
      Cantitate: l.Cantitate,
      PretUnitar: l.PretUnitar,
      // Round-trip DELIBERAT (semantica feliei 2): pe o linie existentă, lipsa
      // lui `TipTvaId` din payload e GOLIRE, nu „nu m-am pronunțat" — default-ul
      // de tip document se aplică doar liniilor noi.
      TipTvaId: l.TipTvaId,
      CodEconomicId: l.CodEconomicId,
    })),
  };
}
