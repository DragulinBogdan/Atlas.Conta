import type { components } from '../../generated/api-types';
import { ia, posteaza, pune, sterge } from '../../nucleu/http';
import { storeRemote } from '../../nucleu/dxStore';
import { azi } from '../../nucleu/zi';

// `api.ts` DE MÂNĂ, cu tipurile GENERATE (43d) — șablonul DEC/LDI. Ce e PROPRIU
// feliei NTC stă tot aici, ca să nu se împrăștie prin JSX:
//   • `Numar` NU se culege: NTC are politică de numerotare („NTC-") în AMBELE
//     profiluri ⇒ seria e a OPERĂRII, server-owned, și nici nu există în
//     `NtcWriteDto` (F19-D6);
//   • **LINIA E POSTAREA** (32a): cele patru FK-uri ale postării explicite +
//     `Valoare` culeasă direct. Nu există lanț de valori (nici cantitate, nici
//     preț, nici TVA — F19-D7/D8), deci nu există nici valoare recalculată de
//     server la salvare: ce trimite operatorul e ce se postează;
//   • **negativul e PERMIS** (note storno, ca cele aduse de importul 1C); zero
//     îl refuză TIPUL la operare, iar clientul NU duplică refuzul (o a doua
//     sursă a aceleiași reguli e exact ce evită 42a);
//   • rolul de STINGĂTOR (48b): nota operată compensează. Candidații se CER
//     (`candidati`), nu se ghicesc — plafonul e per (contrapartidă × SENS), iar
//     contrapartidele notei trăiesc pe LINII, deci proiecția generică de rest
//     (o singură contrapartidă, pe latură) nu poate răspunde singură.
//
// Laturile sunt repartitori INTERNI (contrapartidele sunt pe linii), nefiltrate:
// autoritatea e motorul, nu lookup-ul (F6-D8).

type Scheme = components['schemas'];

export type NtcRead = Scheme['NtcReadDto'];
export type NtcWrite = Scheme['NtcWriteDto'];
export type NtcLinieRead = Scheme['NtcLinieReadDto'];
export type NtcLinieWrite = Scheme['NtcLinieWriteDto'];
export type NtcCandidati = Scheme['NtcCandidatiDto'];
export type NtcContrapartida = Scheme['NtcContrapartidaDto'];
export type OperareRezultat = Scheme['OperareRezultatDto'];

// Numele SCHEMELOR OpenAPI, ca stringuri, pentru `campMeta` (required/maxLength).
export const SCHEMA_ANTET = 'NtcWriteDto';
export const SCHEMA_LINIE = 'NtcLinieWriteDto';
// Tipurile de METADATA (entitățile XAF) — sursa captions-urilor. Linia e frunza
// (`NotaContabilaDetaliu`): descrierea și cele patru FK-uri ale postării
// explicite sunt ale ei.
export const TIP_ANTET = 'NotaContabila';
export const TIP_LINIE = 'NotaContabilaDetaliu';

const BAZA = '/api/ntc';

export const ntc = {
  citeste: (id: string) => ia<NtcRead>(`${BAZA}/${id}`),
  creeaza: (dto: NtcWrite) => posteaza<NtcRead>(BAZA, dto),
  actualizeaza: (id: string, dto: NtcWrite) => pune<NtcRead>(`${BAZA}/${id}`, dto),
  sterge: (id: string) => sterge(`${BAZA}/${id}`),

  // Dry-run (43 §2, stratul 2 autoritar): calculează + validează, fără
  // materializare. 200 cu listă goală = trece toți gardienii.
  valideaza: (id: string) => posteaza<{ Erori?: string[] | null }>(`${BAZA}/${id}/valideaza`)
    .then((r) => r.Erori ?? []),

  opereaza: (id: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/opereaza`),
  anuleaza: (id: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/anuleaza`),
  storneaza: (id: string, data: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/storneaza`, { Data: data }),

  // Panoul de compensare (F19-D10): UN apel întoarce, per (contrapartidă ×
  // sens), plafonul rezolvat de `NotaContabila.CapacitateStingere`, cât s-a
  // consumat deja (`ImperechereService.AsignatFataDe` — ACEEAȘI funcție pe care
  // o cheamă validarea) și rândurile `DocumenteCuRest` ale jumătății. Nu e o
  // grilă remote: lista e plafonată server-side (100 per jumătate) tocmai ca să
  // încapă într-un răspuns.
  candidati: (id: string) => ia<NtcCandidati>(`${BAZA}/${id}/candidati`),

  // Cheia grilei e `Id` (ReadDto), nu `ID` (entitatea) — DTO-ul e contractul.
  storeLista: () => storeRemote(BAZA, 'Id'),
};

export function antetGol(): NtcWrite {
  return { Data: azi(), Linii: [] };
}

// Linie NOUĂ: `Valoare` pornește de la 0 ca orice câmp numeric al culegerii —
// zero e refuzat de TIP la operare (F19-D8), nu de client.
export function linieGoala(): NtcLinieWrite {
  return { Valoare: 0 };
}

// ReadDto → WriteDto: EXACT câmpurile pe care operatorul are voie să le trimită.
// Ce lipsește de aici e server-owned prin OMISIUNE, nu prin uitare: `Numar`
// (seria „NTC-", asignată la materializare), `Stare`, `Total` și
// etichetele/affordances (proiecții de citire).
export function spreWrite(citit: NtcRead): NtcWrite {
  return {
    Data: citit.Data,
    PredatorId: citit.PredatorId,
    PrimitorId: citit.PrimitorId,
    Linii: (citit.Linii ?? []).map((l) => ({
      Id: l.Id,
      TipMaterialId: l.TipMaterialId,
      Descriere: l.Descriere,
      ContDebitId: l.ContDebitId,
      ContCreditId: l.ContCreditId,
      RepartitorDebitId: l.RepartitorDebitId,
      RepartitorCreditId: l.RepartitorCreditId,
      CodEconomicId: l.CodEconomicId,
      Valoare: l.Valoare,
    })),
  };
}
