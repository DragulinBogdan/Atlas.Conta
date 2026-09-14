import type { components } from '../../generated/api-types';
import { ia, posteaza, sterge } from '../../nucleu/http';
import { storeRemote } from '../../nucleu/dxStore';
import { urlCu } from '../../nucleu/urlStare';

// Oglinda lui ITV: fără `WriteDto`; `genereaza` SCRIE ori de câte ori luna e liberă (79), 200 fără document = raport.

type Scheme = components['schemas'];

export type AmoRead = Scheme['AmoReadDto'];
export type AmoListRand = Scheme['AmoListDto'];
export type LinieAmo = Scheme['LinieAmoDto'];
export type PrevizualizareAmo = Scheme['PrevizualizareAmoDto'];
export type GenerareAmoCerere = Scheme['GenerareAmoRequestDto'];
export type GenerareAmoRezultat = Scheme['GenerareAmoRezultatDto'];
export type OperareRezultat = Scheme['OperareRezultatDto'];

// Tipul de METADATA (entitatea XAF). `An`, `Luna` și cele trei totaluri NU sunt
// membri de entitate — le derivă DTO-ul, deci ecranele le numesc EXPLICIT.
export const TIP_ANTET = 'AmortizareLunara';
export const TIP_LINIE = 'AmortizareLunaraDetaliu';
// Felia n-are schemă de SCRIERE a documentului: se citește `AmoReadDto`.
export const SCHEMA_ANTET = 'AmoReadDto';
export const SCHEMA_LISTA = 'AmoListDto';

const BAZA = '/api/amo';

export const amo = {
  citeste: (id: string) => ia<AmoRead>(`${BAZA}/${id}`),

  previzualizare: (an: string | number, luna: string | number) =>
    ia<PrevizualizareAmo>(urlCu(`${BAZA}/previzualizare`, { an: String(an), luna: String(luna) })),

  genereaza: (cerere: GenerareAmoCerere) =>
    posteaza<GenerareAmoRezultat>(`${BAZA}/genereaza`, cerere),

  regenereaza: (id: string) => posteaza<GenerareAmoRezultat>(`${BAZA}/${id}/regenereaza`),

  sterge: (id: string) => sterge(`${BAZA}/${id}`),

  valideaza: (id: string) => posteaza<{ Erori?: string[] | null }>(`${BAZA}/${id}/valideaza`)
    .then((r) => r.Erori ?? []),

  opereaza: (id: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/opereaza`),
  anuleaza: (id: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/anuleaza`),
  storneaza: (id: string, data: string) =>
    posteaza<OperareRezultat>(`${BAZA}/${id}/storneaza`, { Data: data }),

  // Cheia grilei e `Id` (ReadDto), nu `ID` (entitatea). Ordinea implicită
  // (`Data desc, Id desc`) o pune SERVERUL: rândurile sunt LUNI.
  storeLista: () => storeRemote(BAZA, 'Id'),
};
