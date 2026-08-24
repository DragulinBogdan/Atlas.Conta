import type { components } from '../../generated/api-types';
import { ia, posteaza, pune, sterge } from '../../nucleu/http';
import { storeRemote } from '../../nucleu/dxStore';
import { azi } from '../../nucleu/zi';

// `api.ts` DE MÂNĂ, cu tipurile GENERATE (43d) — șablonul FCT/LDI. Ce e PROPRIU
// feliei DEC stă tot aici, ca să nu se împrăștie prin JSX:
//
//   • `Numar` NU se culege: DEC are politică de numerotare („DEC-") în AMBELE
//     profiluri ⇒ seria e a OPERĂRII, server-owned, și nici nu există în
//     `DecontWriteDto` (F8-D3; invers față de FCT, unde numărul e al
//     furnizorului);
//   • **POSTAREA EXPLICITĂ PE LINIE** (`ILinieCuPostareExplicita`, 32a): cont și
//     repartitor, per latură, toate patru OPȚIONALE — trăsătura PROPRIE a
//     tipului, nu un mecanism generic. Ce rămâne gol cade pe regula de contare;
//     ce e cules bate rezolvarea declarativă. Clientul nu decide nimic din asta:
//     le culege și le trimite;
//   • **cantitatea e PRO-FORMĂ** (32d): 0 → 1, normalizat de SERVER la culegere
//     (F8-D2). Nu se normalizează în TS — se vede după Salvează;
//   • `Valoare`/`ValoareTva` sunt REZULTAT (`PretUnitar × Cantitate` prin
//     `TvaService`, F8-D2), deci nu intră în WriteDto — ca la FCT/FCL.
//
// Laturile: predatorul e TITULARUL (`Angajat`) care justifică avansul,
// primitorul e unitatea internă care primește justificarea (`UnitateInterna`
// sau `Gestiune` — validarea operării le acceptă pe amândouă).

type Scheme = components['schemas'];

export type DecRead = Scheme['DecontReadDto'];
export type DecWrite = Scheme['DecontWriteDto'];
export type DecLinieRead = Scheme['DecontLinieReadDto'];
export type DecLinieWrite = Scheme['DecontLinieWriteDto'];
export type OperareRezultat = Scheme['OperareRezultatDto'];

// Numele SCHEMELOR OpenAPI, ca stringuri, pentru `campMeta` (required/maxLength).
// Câmpurile doar-de-afișare ale antetului (`Numar`, `Stare`, `DataOperare`) nu
// există în WriteDto — caption-ul lor vine din `metadata.json`, iar lipsa din
// schemă îi lasă corect NEobligatorii.
export const SCHEMA_ANTET = 'DecontWriteDto';
export const SCHEMA_LINIE = 'DecontLinieWriteDto';
// Tipurile de METADATA (entitățile XAF) — sursa captions-urilor. Linia e frunza
// (`DecontDetaliu`): descrierea, prețul unitar, cele patru câmpuri de postare
// explicită și `CodEconomic` sunt ale ei.
export const TIP_ANTET = 'Decont';
export const TIP_LINIE = 'DecontDetaliu';

export const RUTA = '/dec';
const BAZA = '/api/dec';

export const dec = {
  citeste: (id: string) => ia<DecRead>(`${BAZA}/${id}`),
  creeaza: (dto: DecWrite) => posteaza<DecRead>(BAZA, dto),
  actualizeaza: (id: string, dto: DecWrite) => pune<DecRead>(`${BAZA}/${id}`, dto),
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

export function antetGol(): DecWrite {
  return { Data: azi(), Linii: [] };
}

// Linie nouă: cantitatea rămâne 0 și o normalizează SERVERUL la 1 (pro-formă,
// F8-D2). Un „1" pus aici ar fi o regulă de domeniu mutată în TS — și ar
// ascunde exact ce cere F8-D2 să se vadă.
export function linieGoala(): DecLinieWrite {
  return { Cantitate: 0, PretUnitar: 0 };
}

// ReadDto → WriteDto: EXACT câmpurile pe care operatorul are voie să le trimită.
// Ce lipsește de aici e server-owned prin OMISIUNE, nu prin uitare: `Numar`
// (seria „DEC-", asignată la materializare), `Stare`, `Valoare`, `Total` și
// etichetele/affordances (proiecții de citire).
//
// `ValoareTva` este DELIBERAT absentă (regula FCT/FCL, o singură sursă): pe
// sârmă ea înseamnă „override manual", iar re-trimiterea valorii citite ar
// îngheța TVA-ul — serverul n-ar mai recalcula la schimbarea cantității sau a
// prețului. Override-ul îl pune editorul de linie, doar când operatorul chiar
// atinge câmpul.
export function spreWrite(citit: DecRead): DecWrite {
  return {
    Data: citit.Data,
    PredatorId: citit.PredatorId,
    PrimitorId: citit.PrimitorId,
    NumarPV: citit.NumarPV,
    DataPV: citit.DataPV,
    Linii: (citit.Linii ?? []).map((l) => ({
      Id: l.Id,
      TipMaterialId: l.TipMaterialId,
      Descriere: l.Descriere,
      Cantitate: l.Cantitate,
      // `PretUnitar` e nullable în ReadDto (linie de tip BAZĂ — decont istoric),
      // dar non-nullable în contractul de scriere: 0 e valoarea onestă pentru
      // „frunza nu există", iar reconcilierea oricum refuză o astfel de linie
      // referită prin `Id`.
      PretUnitar: l.PretUnitar ?? 0,
      // Round-trip DELIBERAT (semantica F2): pe o linie existentă, lipsa lui
      // `TipTvaId` din payload e GOLIRE, nu „nu m-am pronunțat" — default-ul de
      // tip document se aplică doar liniilor noi.
      TipTvaId: l.TipTvaId,
      CodEconomicId: l.CodEconomicId,
      AngajamentId: l.AngajamentId,
      // Postarea explicită (32a) — trăsătura tipului, culeasă și dusă înapoi.
      ContDebitId: l.ContDebitId,
      ContCreditId: l.ContCreditId,
      RepartitorDebitId: l.RepartitorDebitId,
      RepartitorCreditId: l.RepartitorCreditId,
    })),
  };
}
