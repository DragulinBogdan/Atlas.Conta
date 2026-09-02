import type { components } from '../../generated/api-types';
import { ia, posteaza, sterge } from '../../nucleu/http';
import { storeRemote } from '../../nucleu/dxStore';
import { urlCu } from '../../nucleu/urlStare';

// `api.ts` DE MÂNĂ, cu tipurile GENERATE (43d) — dar felia ITV e deliberat
// ALTFEL decât cele treisprezece de dinaintea ei (76a, F21-D1): închiderea de
// TVA nu e un agregat CULES (nu există `PUT header + linii`), e rezultatul unui
// SERVICIU. De aceea aici nu găsești `WriteDto`, `antetGol`, `linieGoala` sau
// `spreWrite`: n-ar avea ce transporta. Verbele feliei sunt:
//
//   • `previzualizare(an, luna)` — dry-run-ul comenzii: ce s-ar închide, sau
//     DE CE nu se poate (`Motiv`). Nu scrie nimic.
//   • `genereaza(cerere)` — comanda. **Răspunde 200 și când nu s-a generat
//     nimic** (`DocumentId: null` + `Motiv`): e un RAPORT, ca `DscId = null` la
//     backorder (58) și ca lotul ANAF (72e). 422 iese doar pe refuz de DOMENIU
//     (cronologie, tip TRZ lipsă, unitate ne-internă).
//   • `regenereaza(id)` — șterge draftul și îl reface pe soldurile de ACUM;
//     refolosește unitatea draftului, deci n-o mai cere.
//   • restul = comenzile standard ale oricărui document (`OperareApi`).
//
// Ce NU e aici, și e important: nicio aritmetică. Cele trei linii ale
// închiderii (transfer / de plată / de recuperat) le calculează
// `InchidereTvaService.CalculeazaLinii` — ACEEAȘI funcție pe care o consumă
// generatorul, deci previzualizarea nu e o copie a calculului, e calculul (42c).
// Un `Math.min(sold4426, sold4427)` scris aici ar fi al doilea generator.

type Scheme = components['schemas'];

export type ItvRead = Scheme['ItvReadDto'];
export type ItvListRand = Scheme['ItvListDto'];
export type ItvLinieRead = Scheme['ItvLinieReadDto'];
export type PrevizualizareItv = Scheme['PrevizualizareItvDto'];
export type GenerareItvCerere = Scheme['GenerareItvRequestDto'];
export type GenerareItvRezultat = Scheme['GenerareItvRezultatDto'];
export type OperareRezultat = Scheme['OperareRezultatDto'];

// Tipul de METADATA (entitatea XAF) — sursa captions-urilor moștenite din
// `Document` (Numar/Data/Stare/Total/PredatorId). `An`, `Luna` și cele șase
// cifre ale închiderii NU sunt membri de entitate (le derivă DTO-ul din `Data`,
// respectiv din liniile și din registrul de la acea dată), deci ecranele le
// numesc EXPLICIT — nu prin `campMeta`, care ar avertiza în consolă și ar cădea
// pe numele câmpului.
export const TIP_ANTET = 'InchidereTva';
// Numele SCHEMELOR OpenAPI pentru `campMeta` (required/maxLength). Felia n-are
// schemă de SCRIERE a documentului: se citește `ItvReadDto` / `ItvListDto`.
export const SCHEMA_ANTET = 'ItvReadDto';
export const SCHEMA_LISTA = 'ItvListDto';

const BAZA = '/api/itv';

export const itv = {
  citeste: (id: string) => ia<ItvRead>(`${BAZA}/${id}`),

  // Raport, nu comandă (F21-D2b): aici `NeCronologica` e un MOTIV afișabil,
  // în timp ce `genereaza` pe aceeași lună refuză cu 422. Cele două nu se
  // contrazic — una spune „de ce nu", cealaltă refuză zgomotos să facă.
  previzualizare: (an: string | number, luna: string | number) =>
    ia<PrevizualizareItv>(urlCu(`${BAZA}/previzualizare`, { an: String(an), luna: String(luna) })),

  genereaza: (cerere: GenerareItvCerere) =>
    posteaza<GenerareItvRezultat>(`${BAZA}/genereaza`, cerere),

  regenereaza: (id: string) => posteaza<GenerareItvRezultat>(`${BAZA}/${id}/regenereaza`),

  sterge: (id: string) => sterge(`${BAZA}/${id}`),

  // Dry-run al motorului: 200 cu listă goală = trece toți gardienii, inclusiv
  // cel anti-stale al lui `InchidereTva.ValideazaOperare`.
  valideaza: (id: string) => posteaza<{ Erori?: string[] | null }>(`${BAZA}/${id}/valideaza`)
    .then((r) => r.Erori ?? []),

  opereaza: (id: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/opereaza`),
  anuleaza: (id: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/anuleaza`),
  storneaza: (id: string, data: string) =>
    posteaza<OperareRezultat>(`${BAZA}/${id}/storneaza`, { Data: data }),

  // Cheia grilei e `Id` (ReadDto), nu `ID` (entitatea) — DTO-ul e contractul.
  // Ordinea implicită (`Data` desc) o pune SERVERUL: rândurile sunt LUNI, iar
  // `Id`-ul e ordinea de inserare, deci după o regenerare draftul lunii vechi ar
  // sări în capul listei.
  storeLista: () => storeRemote(BAZA, 'Id'),
};
