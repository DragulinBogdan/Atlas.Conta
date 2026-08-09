import type { components } from '../../generated/api-types';
import { ia, posteaza, pune, sterge } from '../../nucleu/http';
import { storeRemote } from '../../nucleu/dxStore';
import { azi } from '../../nucleu/zi';

// `api.ts` DE MÂNĂ, cu tipurile GENERATE (43d) — același șablon ca BTR. Ce e
// PROPRIU feliei FCT stă tot aici, ca să nu se împrăștie prin JSX:
//   • `Numar` e CULES (numărul furnizorului — FCT n-are politică de numerotare);
//   • `ValoareTva` circulă doar ca OVERRIDE (vezi `spreWrite`);
//   • grupul conex (`Copii[]`) e citit, niciodată scris.

type Scheme = components['schemas'];

export type FctRead = Scheme['FacturaIntrareReadDto'];
export type FctWrite = Scheme['FacturaIntrareWriteDto'];
export type FctLinieRead = Scheme['FacturaIntrareLinieReadDto'];
export type FctLinieWrite = Scheme['FacturaIntrareLinieWriteDto'];
export type DocumentCopil = Scheme['DocumentCopilDto'];
export type OperareRezultat = Scheme['OperareRezultatDto'];

// Numele SCHEMELOR OpenAPI, ca stringuri, pentru `useCampMeta` (required/
// maxLength). Aici, o dată — nu împrăștiate prin JSX.
export const SCHEMA_ANTET = 'FacturaIntrareWriteDto';
export const SCHEMA_LINIE = 'FacturaIntrareLinieWriteDto';
// Tipurile de METADATA (entitățile XAF) — sursa captions-urilor. Linia e frunza
// (`FacturaIntrareDetaliu`): dimensiunile și `PretUnitar` sunt ale ei (DIM-2).
export const TIP_ANTET = 'FacturaIntrare';
export const TIP_LINIE = 'FacturaIntrareDetaliu';

const BAZA = '/api/fct';

export const fct = {
  citeste: (id: string) => ia<FctRead>(`${BAZA}/${id}`),
  creeaza: (dto: FctWrite) => posteaza<FctRead>(BAZA, dto),
  actualizeaza: (id: string, dto: FctWrite) => pune<FctRead>(`${BAZA}/${id}`, dto),
  sterge: (id: string) => sterge(`${BAZA}/${id}`),

  // Dry-run (43 §2, stratul 2 autoritar): calculează + validează, fără
  // materializare. 200 cu listă goală = trece toți gardienii.
  valideaza: (id: string) => posteaza<{ Erori?: string[] | null }>(`${BAZA}/${id}/valideaza`)
    .then((r) => r.Erori ?? []),

  // Operarea FCT poate întoarce `ConexId` — NIR-ul generat de motor în aceeași
  // tranzacție (F2-D3). Clientul îl deschide pe `/nir/{id}`.
  opereaza: (id: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/opereaza`),
  anuleaza: (id: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/anuleaza`),
  storneaza: (id: string, data: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/storneaza`, { Data: data }),

  // Cheia grilei e `Id` (ReadDto), nu `ID` (entitatea) — DTO-ul e contractul.
  storeLista: () => storeRemote(BAZA, 'Id'),
};

export function antetGol(): FctWrite {
  return { Data: azi(), Linii: [] };
}

export function linieGoala(): FctLinieWrite {
  return { Cantitate: 0, PretUnitar: 0 };
}

// ReadDto → WriteDto: exact câmpurile pe care operatorul are voie să le trimită.
// Restul (Stare, Lot, Valoare, Total, etichete) rămân ale serverului.
//
// `Valuta`/`Curs` NU se culeg în ecran, dar se DUC ÎNAPOI: PUT-ul e agregatul
// întreg, deci ce lipsește din payload se șterge. Un import (Tethys) care le-a
// pus nu are voie să le piardă fiindcă un om a deschis documentul.
//
// `ValoareTva` este DELIBERAT absentă: pe sârmă ea înseamnă „override manual",
// iar re-trimiterea valorii citite ar îngheța TVA-ul (serverul n-ar mai
// recalcula când se schimbă cantitatea sau prețul). Override-ul îl pune editorul
// de linie, doar când operatorul chiar atinge câmpul.
//
// Câmpurile `Plata*` (F3-D5) sunt CULESE, deci circulă în ambele sensuri: ele
// descriu plata pe care motorul o va genera la operare (31e). `GenereazaPlata`
// e bifă, nu tri-state — absența ei din payload ar însemna „fals", iar asta e
// exact semantica bifei nebifate.
export function spreWrite(citit: FctRead): FctWrite {
  return {
    Numar: citit.Numar,
    Data: citit.Data,
    PredatorId: citit.PredatorId,
    PrimitorId: citit.PrimitorId,
    DataScadenta: citit.DataScadenta,
    NumarPV: citit.NumarPV,
    DataPV: citit.DataPV,
    CodCpv: citit.CodCpv,
    Valuta: citit.Valuta,
    Curs: citit.Curs,
    GenereazaPlata: citit.GenereazaPlata,
    PlataContPropriuId: citit.PlataContPropriuId,
    PlataNumar: citit.PlataNumar,
    PlataData: citit.PlataData,
    PlataTipInstrument: citit.PlataTipInstrument,
    Linii: (citit.Linii ?? []).map((l) => ({
      Id: l.Id,
      TipMaterialId: l.TipMaterialId,
      ProdusId: l.ProdusId,
      Cantitate: l.Cantitate,
      PretUnitar: l.PretUnitar,
      // Round-trip DELIBERAT (semantica pasului 2): pe o linie existentă, lipsa
      // lui `TipTvaId` din payload e GOLIRE, nu „nu m-am pronunțat" — default-ul
      // de tip document se aplică doar liniilor noi.
      TipTvaId: l.TipTvaId,
      DataExpirare: l.DataExpirare,
      LotFabricatie: l.LotFabricatie,
      CodCpv: l.CodCpv,
      AngajamentId: l.AngajamentId,
      CodEconomicId: l.CodEconomicId,
      SursaFinantareId: l.SursaFinantareId,
      CodFunctionalId: l.CodFunctionalId,
      ProiectId: l.ProiectId,
    })),
  };
}
