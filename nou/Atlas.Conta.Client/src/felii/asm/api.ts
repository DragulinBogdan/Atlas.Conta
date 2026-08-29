import type { components } from '../../generated/api-types';
import { ia, posteaza, pune, sterge } from '../../nucleu/http';
import { storeRemote } from '../../nucleu/dxStore';
import { azi } from '../../nucleu/zi';

// `api.ts` DE MÂNĂ, cu tipurile GENERATE (43d) — șablonul LDI, fiindcă ASM e
// GEAMĂNUL lui structural: direcție explicită pe linie, o ramură NAȘTE lot
// (Produs, ca plusul de inventar), cealaltă descarcă un lot EXISTENT (Consum, ca
// minusul). Ce e PROPRIU feliei:
//   • `Numar` NU se culege (seria „ASM-" e server-owned, consumată la
//     materializare — F19-D6); fără TVA (F19-D7: ASM n-are `PoliticaTva`);
//   • gestiunea lotului nou e a PREDATORULUI, nu a primitorului (F19-D3):
//     laturile POT diferi, deși de regulă sunt aceeași;
//   • documentul poartă un INVARIANT VALORIC (46d): Σ produse = Σ consumuri.
//     ReadDto duce `SumaConsum`/`SumaProdus`/`Diferenta` calculate SERVER-SIDE —
//     clientul le AFIȘEAZĂ, nu le compune (42c);
//   • comanda proprie `distribuie-valoarea` (F19-D4, închide 75-r1) rescrie
//     `PretEvaluare` pe liniile de produs ca invariantul să treacă EXACT,
//     prezicând valoarea consumului prin funcțiile motorului — inclusiv regula
//     golirii (75a), pe care culegerea NU are voie s-o prezică singură.
//
// `Valoare` e REZULTAT și e SEMNATĂ de server încă de la culegere (F19-D8):
// consumul negativ, produsul pozitiv — deci `Total`-ul draftului E diferența
// invariantului. Cantitatea rămâne pozitivă la culegere; semnarea ei e fapta
// OPERĂRII (28a).

type Scheme = components['schemas'];

export type AsmRead = Scheme['AsmReadDto'];
export type AsmWrite = Scheme['AsmWriteDto'];
export type AsmLinieRead = Scheme['AsmLinieReadDto'];
export type AsmLinieWrite = Scheme['AsmLinieWriteDto'];
export type AsmDistribuire = Scheme['AsmDistribuireDto'];
export type OperareRezultat = Scheme['OperareRezultatDto'];

// Numele SCHEMELOR OpenAPI, ca stringuri, pentru `campMeta` (required/maxLength).
export const SCHEMA_ANTET = 'AsmWriteDto';
export const SCHEMA_LINIE = 'AsmLinieWriteDto';
// Tipurile de METADATA (entitățile XAF) — sursa captions-urilor. Linia e frunza
// (`AsamblareDetaliu`): direcția, produsul, prețul de evaluare și atributele de
// lot sunt ale ei (F19-D3).
export const TIP_ANTET = 'Asamblare';
export const TIP_LINIE = 'AsamblareDetaliu';

const BAZA = '/api/asm';

export const asm = {
  citeste: (id: string) => ia<AsmRead>(`${BAZA}/${id}`),
  creeaza: (dto: AsmWrite) => posteaza<AsmRead>(BAZA, dto),
  actualizeaza: (id: string, dto: AsmWrite) => pune<AsmRead>(`${BAZA}/${id}`, dto),
  sterge: (id: string) => sterge(`${BAZA}/${id}`),

  // Dry-run (43 §2, stratul 2 autoritar): calculează + validează, fără
  // materializare. Pe ASM e și singurul verdict care ține cont de regula
  // golirii — cifrele culegerii nu o prezic (vezi `AsmReadDto.SumaConsum`).
  valideaza: (id: string) => posteaza<{ Erori?: string[] | null }>(`${BAZA}/${id}/valideaza`)
    .then((r) => r.Erori ?? []),

  opereaza: (id: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/opereaza`),
  anuleaza: (id: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/anuleaza`),
  storneaza: (id: string, data: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/storneaza`, { Data: data }),

  // Comanda proprie feliei (F19-D4). Întoarce cifrele deciziei (suma prezisă a
  // consumului, suma repartizată, reziduul plimbat) PLUS agregatul recitit.
  // Refuzurile ei sunt mesaje de DOMENIU (422) — inclusiv cazul 75-r4, în care
  // niciun pas de preț reprezentabil nu poate stinge reziduul: e un refuz
  // legitim, cu cifra, nu o eroare tehnică.
  distribuie: (id: string) => posteaza<AsmDistribuire>(`${BAZA}/${id}/distribuie-valoarea`),

  // Cheia grilei e `Id` (ReadDto), nu `ID` (entitatea) — DTO-ul e contractul.
  storeLista: () => storeRemote(BAZA, 'Id'),
};

export function antetGol(): AsmWrite {
  return { Data: azi(), Linii: [] };
}

// Linie NOUĂ fără direcție: enumerarea n-are membru 0 tocmai ca o linie fără rol
// cules să nu treacă drept ceva — o alege operatorul, și de ea atârnă restul.
export function linieGoala(): AsmLinieWrite {
  return { Cantitate: 0 };
}

// ReadDto → WriteDto: EXACT câmpurile pe care operatorul are voie să le trimită.
// Ce lipsește de aici e server-owned prin OMISIUNE, nu prin uitare: `Numar`
// (seria „ASM-"), `Stare`, `Valoare`, `Total`, sumele invariantului și
// etichetele/affordances (proiecții de citire).
//
// `LotId` face round-trip pe AMBELE direcții, ca la LDI: pe CONSUM e pinul
// cules, pe PRODUS serverul îl IGNORĂ (lotul liniei de produs se naște prin
// `LoturiCulegereService`, nu prin payload) — deci ecoul lui e inofensiv.
export function spreWrite(citit: AsmRead): AsmWrite {
  return {
    Data: citit.Data,
    PredatorId: citit.PredatorId,
    PrimitorId: citit.PrimitorId,
    Linii: (citit.Linii ?? []).map((l) => ({
      Id: l.Id,
      Directie: l.Directie,
      TipMaterialId: l.TipMaterialId,
      ProdusId: l.ProdusId,
      LotId: l.LotId,
      Cantitate: l.Cantitate,
      PretEvaluare: l.PretEvaluare,
      DataExpirare: l.DataExpirare,
      LotFabricatie: l.LotFabricatie,
      AngajamentId: l.AngajamentId,
    })),
  };
}
