import type { components } from '../../generated/api-types';
import { ia, posteaza, pune, sterge } from '../../nucleu/http';
import { storeRemote } from '../../nucleu/dxStore';
import { azi } from '../../nucleu/zi';

// `api.ts` DE MÂNĂ, cu tipurile GENERATE (43d) — șablonul FCT/FCL. Ce e PROPRIU
// feliei NIR stă tot aici, ca să nu se împrăștie prin JSX:
//   • `Numar` NU se culege: NIR are politică de numerotare („NIR-") ⇒ seria e a
//     OPERĂRII, server-owned — nici nu există în `NirWriteDto` (invers față de
//     FCT, unde numărul e al furnizorului);
//   • NIR-ul NU culege TVA (F5-D5): fără `PoliticaTva` pasul TVA al motorului nu
//     se declanșează, iar un TVA cules aici ar fi cifră moartă în cel mai bun caz
//     și dublă postare când sosește factura. Clona conexă își păstrează
//     `TipTvaId` informativ — PUT-ul nu-l atinge, ReadDto continuă să-l arate;
//   • `LotId` e server-owned (F5-D4): pe liniile culese manual îl NAȘTE serverul
//     din `ProdusId`, iar pe clona conexă rămâne lotul facturii (lot STRĂIN);
//   • `Valoare` e REZULTAT, nu câmp cules: serverul o materializează la culegere
//     din prețul liniei sau din prețul lotului străin (F5-D6).

type Scheme = components['schemas'];

export type NirRead = Scheme['NirReadDto'];
export type NirWrite = Scheme['NirWriteDto'];
export type NirLinieRead = Scheme['NirLinieReadDto'];
export type NirLinieWrite = Scheme['NirLinieWriteDto'];
export type OperareRezultat = Scheme['OperareRezultatDto'];

// Numele SCHEMELOR OpenAPI, ca stringuri, pentru `campMeta` (required/maxLength).
// Câmpurile doar-de-afișare ale antetului (`Numar`, `Stare`, `DataOperare`,
// `Autogenerat`, `DocumentSursaId`) nu există în WriteDto — caption-ul lor vine
// din `metadata.json`, iar lipsa din schemă îi lasă corect NEobligatorii.
export const SCHEMA_ANTET = 'NirWriteDto';
export const SCHEMA_LINIE = 'NirLinieWriteDto';
// Tipurile de METADATA (entitățile XAF) — sursa captions-urilor. Linia e frunza
// (`NirDetaliu`): `Produs`, `PretUnitar`, atributele de lot și dimensiunile sunt
// ale ei (F5-D1 / DIM-2).
export const TIP_ANTET = 'NIR';
export const TIP_LINIE = 'NirDetaliu';

const BAZA = '/api/nir';

export const nir = {
  citeste: (id: string) => ia<NirRead>(`${BAZA}/${id}`),
  creeaza: (dto: NirWrite) => posteaza<NirRead>(BAZA, dto),
  actualizeaza: (id: string, dto: NirWrite) => pune<NirRead>(`${BAZA}/${id}`, dto),
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

export function antetGol(): NirWrite {
  return { Data: azi(), Linii: [] };
}

export function linieGoala(): NirLinieWrite {
  return { Cantitate: 0, PretUnitar: 0 };
}

// ReadDto → WriteDto: EXACT câmpurile pe care operatorul are voie să le trimită.
// Ce lipsește de aici e server-owned prin OMISIUNE, nu prin uitare:
//   • `Numar`      — seria „NIR-" se asignează la operare (F5-D8);
//   • `Stare`      — a motorului;
//   • `LotId`      — se naște din `ProdusId` la commit, sau e al facturii (F5-D4);
//   • `Valoare`    — rezultat, materializat de `NirApply` și rescris la operare;
//   • `Total`      — agregat de server;
//   • `TipTvaId`/`ValoareTva` — NIR-ul nu culege TVA (F5-D5); pe clona conexă ele
//     există și se AFIȘEAZĂ, dar nu circulă înapoi: absența lor din payload nu e
//     golire, fiindcă `NirApply` nici măcar nu atinge câmpurile;
//   • etichetele și affordances — proiecții de citire.
//
// `ProdusId`/`PretUnitar` fac round-trip și pe liniile cu lot STRĂIN (unde sunt
// null/0): serverul le ignoră acolo prin construcție — gardul F5-D3 lasă lotul
// moștenit neatins, iar valoarea vine din prețul LOTULUI.
export function spreWrite(citit: NirRead): NirWrite {
  return {
    Data: citit.Data,
    PredatorId: citit.PredatorId,
    PrimitorId: citit.PrimitorId,
    Linii: (citit.Linii ?? []).map((l) => ({
      Id: l.Id,
      TipMaterialId: l.TipMaterialId,
      // Produsul e MECANISMUL lotului pe recepția manuală (F5-D2), nu o etichetă.
      ProdusId: l.ProdusId,
      Cantitate: l.Cantitate,
      // Prețul de recepție: din el se naște `PretUnitar`-ul lotului (26e), de
      // aceea operarea îl cere pozitiv pe liniile care își nasc lotul (F5-D7b).
      PretUnitar: l.PretUnitar,
      AngajamentId: l.AngajamentId,
      DataExpirare: l.DataExpirare,
      LotFabricatie: l.LotFabricatie,
      CodEconomicId: l.CodEconomicId,
      SursaFinantareId: l.SursaFinantareId,
      CodFunctionalId: l.CodFunctionalId,
      ProiectId: l.ProiectId,
    })),
  };
}
