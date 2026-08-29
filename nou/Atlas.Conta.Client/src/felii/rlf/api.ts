import type { components } from '../../generated/api-types';
import { ia, posteaza, pune, sterge } from '../../nucleu/http';
import { storeRemote } from '../../nucleu/dxStore';
import { azi } from '../../nucleu/zi';

// `api.ts` DE MÂNĂ, cu tipurile GENERATE (43d) — șablonul BCS (tip + lot +
// cantitate, fără frunză de linie), peste care RLF adaugă lanțul de TVA al
// FCT-ului. Ce e PROPRIU feliei:
//   • `Numar` NU se culege: RLF are politică de numerotare („RLF-") ⇒ seria e a
//     OPERĂRII, server-owned, și nici nu există în `RlfWriteDto` (F19-D6);
//   • TVA-ul E cules (F19-D7): RLF are `PoliticaTva` (Deductibil) și
//     `TipTvaImplicit = N21` — returul stornează o achiziție, deci și taxa ei;
//   • `Valoare` NU e în WriteDto și nu se culege NICIODATĂ: e a lotului
//     (`|cantitate| × PretUnitar`), materializată de server la culegere și încă
//     o dată la operare (F19-D8).
//
// ═══ Semnul: pozitiv pe draft, negativ pe operat — și de ce nu e inconsecvență ═══
// Culegerea e POZITIVĂ: cifra de pe nota de credit a furnizorului, ca s-o poată
// confrunta operatorul înainte de a opera. Semnarea storno e fapta OPERĂRII
// (`PregatesteOperare` neagă cantitatea, valoarea și TVA-ul — reprezentarea
// storno a modelului: valori negative pe corespondența ORIGINALĂ). Deci ReadDto
// întoarce pozitiv pe Draft și negativ pe Operat, iar ecranul AFIȘEAZĂ ce
// întoarce serverul, în ambele stări — nu „corectează" nimic (42c). Documentul
// operat e oricum read-only; un document ANULAT redevine Draft purtând încă
// semnele operării, iar Apply le normalizează înapoi la magnitudini la primul
// PUT (`Math.Abs`, idempotent).
//
// ═══ Golirea fiscală: culegerea NU prezice restul ═══
// RLF declară `IDocumentCuIesireFiscala` (75a): ieșirea care golește lotul NU
// preia soldul valoric rămas — suma returului e hârtia furnizorului, iar
// reziduul de cenți rămâne pe lot. Clientul nu are ce adăuga aici: nu calculează
// nicio valoare și nu arată nicio previziune de rest.

type Scheme = components['schemas'];

export type RlfRead = Scheme['RlfReadDto'];
export type RlfWrite = Scheme['RlfWriteDto'];
export type RlfLinieRead = Scheme['RlfLinieReadDto'];
export type RlfLinieWrite = Scheme['RlfLinieWriteDto'];
export type OperareRezultat = Scheme['OperareRezultatDto'];

// Numele SCHEMELOR OpenAPI, ca stringuri, pentru `campMeta` (required/maxLength).
export const SCHEMA_ANTET = 'RlfWriteDto';
export const SCHEMA_LINIE = 'RlfLinieWriteDto';
// Tipurile de METADATA (entitățile XAF) — sursa captions-urilor. Linia RLF n-are
// frunză proprie (F19-D5: retururile folosesc detaliul de BAZĂ), deci e
// `DocumentDetaliu`.
export const TIP_ANTET = 'ReturFurnizor';
export const TIP_LINIE = 'DocumentDetaliu';

const BAZA = '/api/rlf';

export const rlf = {
  citeste: (id: string) => ia<RlfRead>(`${BAZA}/${id}`),
  creeaza: (dto: RlfWrite) => posteaza<RlfRead>(BAZA, dto),
  actualizeaza: (id: string, dto: RlfWrite) => pune<RlfRead>(`${BAZA}/${id}`, dto),
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

export function antetGol(): RlfWrite {
  return { Data: azi(), Linii: [] };
}

export function linieGoala(): RlfLinieWrite {
  return { Cantitate: 0 };
}

// ReadDto → WriteDto: EXACT câmpurile pe care operatorul are voie să le trimită.
// Ce lipsește de aici e server-owned prin OMISIUNE, nu prin uitare: `Numar`
// (seria „RLF-", asignată la materializare), `Stare`, `Valoare` (a lotului),
// `Total` și etichetele/affordances (proiecții de citire).
//
// `ValoareTva` e DELIBERAT absentă, ca la FCT/FCL: pe sârmă ea înseamnă
// „override manual, bate rotunjirea" (36a), iar re-trimiterea valorii citite ar
// îngheța TVA-ul — serverul n-ar mai recalcula când se schimbă cantitatea sau
// lotul. Override-ul îl pune editorul de linie, doar când operatorul chiar
// atinge câmpul.
//
// `TipTvaId` FACE round-trip: pe o linie existentă lipsa lui din payload e
// GOLIRE deliberată, nu „nu m-am pronunțat" — implicitul tipului de document se
// aplică doar liniilor NOI.
export function spreWrite(citit: RlfRead): RlfWrite {
  return {
    Data: citit.Data,
    PredatorId: citit.PredatorId,
    PrimitorId: citit.PrimitorId,
    Linii: (citit.Linii ?? []).map((l) => ({
      Id: l.Id,
      TipMaterialId: l.TipMaterialId,
      LotId: l.LotId,
      // Cantitatea unui document ANULAT vine SEMNATĂ (fapta operării): se duce
      // înapoi așa cum a venit, iar Apply o normalizează la magnitudine. Clientul
      // nu-i schimbă semnul — ar fi al doilea adevăr despre o cifră a serverului.
      Cantitate: l.Cantitate,
      TipTvaId: l.TipTvaId,
    })),
  };
}
