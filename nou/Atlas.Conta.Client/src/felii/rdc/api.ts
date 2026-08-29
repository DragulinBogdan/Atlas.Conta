import type { components } from '../../generated/api-types';
import { ia, posteaza, pune, sterge } from '../../nucleu/http';
import { storeRemote } from '../../nucleu/dxStore';
import { azi } from '../../nucleu/zi';

// `api.ts` DE MÂNĂ, cu tipurile GENERATE (43d). RDC e singurul document al
// clientului cu linii pe DOUĂ ROLURI în același agregat:
//   • **venit** (fără lot) — venitul stornat de pe factura originală: Tip de
//     venit, `Valoare` CULEASĂ, TVA;
//   • **marfă returnată** (cu lot) — marfa care revine pe lotul ORIGINAL: Tip de
//     stoc, cantitate, `Valoare` = costul lotului (nu se culege), FĂRĂ TVA
//     (serverul persistă `TipTvaId = null`).
//
// ═══ Rolul e o PREZENȚĂ pe sârmă, un comutator în editor ═══
// În model rolul E prezența lui `LotId` (F19-D12) — nu există enum și nu se
// adaugă unul. Traducerea în „Venit" / „Marfă returnată" trăiește în EDITOR
// (`RdcEditorLinie`), la graniță, exact ca traducerea inversă la salvare.
// Consecința pe care serverul o impune (riscul 5, închis în `ReturClientApply`):
// **rolul unei linii EXISTENTE nu se schimbă** — un PUT care adaugă sau scoate
// `LotId` de pe o linie salvată e REFUZAT, nu convertit. Calea legitimă e cea pe
// care agregatul o exprimă deja: se șterge linia și se culege din nou pe rolul
// dorit. De aceea comutatorul e activ DOAR pe liniile noi.
//
// ═══ Două totaluri, niciodată adunate ═══
// `Total` = DOAR liniile de venit (brutul care ajustează creanța — oglinda lui
// `ReturClient.Total`, virtual, și a lui `LiniiCreanta`); `TotalCost` = Σ
// liniilor cu lot. Amândouă vin din ReadDto. Un singur „Total" ar minți:
// documentul valorează „−121 creanță + −30 cost", nu −151.
//
// Ca RLF: `Numar` server-owned (seria „RDC-", F19-D6), culegere POZITIVĂ,
// semnarea storno lăsată OPERĂRII (F19-D8) — vezi nota de semn din `felii/rlf`.

type Scheme = components['schemas'];

export type RdcRead = Scheme['RdcReadDto'];
export type RdcWrite = Scheme['RdcWriteDto'];
export type RdcLinieRead = Scheme['RdcLinieReadDto'];
export type RdcLinieWrite = Scheme['RdcLinieWriteDto'];
export type OperareRezultat = Scheme['OperareRezultatDto'];

// Numele SCHEMELOR OpenAPI, ca stringuri, pentru `campMeta` (required/maxLength).
export const SCHEMA_ANTET = 'RdcWriteDto';
export const SCHEMA_LINIE = 'RdcLinieWriteDto';
// Tipurile de METADATA (entitățile XAF) — sursa captions-urilor. Linia RDC n-are
// frunză proprie (F19-D5), deci e detaliul de BAZĂ.
export const TIP_ANTET = 'ReturClient';
export const TIP_LINIE = 'DocumentDetaliu';

const BAZA = '/api/rdc';

export const rdc = {
  citeste: (id: string) => ia<RdcRead>(`${BAZA}/${id}`),
  creeaza: (dto: RdcWrite) => posteaza<RdcRead>(BAZA, dto),
  actualizeaza: (id: string, dto: RdcWrite) => pune<RdcRead>(`${BAZA}/${id}`, dto),
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

export function antetGol(): RdcWrite {
  return { Data: azi(), Linii: [] };
}

// Linie NOUĂ fără rol: `LotId` lipsă ar însemna deja „venit" pe sârmă, deci
// editorul ține rolul ca stare PROPRIE și îl cere explicit înainte de a lăsa
// linia să fie adăugată (același principiu ca direcția fără membru 0 a ASM).
export function linieGoala(): RdcLinieWrite {
  return { Cantitate: 0, Valoare: 0 };
}

// ReadDto → WriteDto: EXACT câmpurile pe care operatorul are voie să le trimită.
// Ce lipsește de aici e server-owned prin OMISIUNE, nu prin uitare: `Numar`
// (seria „RDC-"), `Stare`, `Total`/`TotalCost` și etichetele/affordances.
//
// `ValoareTva` e DELIBERAT absentă (ca la FCT/FCL/RLF): pe sârmă înseamnă
// „override manual" (36a), iar re-trimiterea valorii citite ar îngheța TVA-ul.
// `TipTvaId` FACE round-trip: pe o linie existentă lipsa lui e GOLIRE
// deliberată — implicitul se aplică doar liniilor noi.
//
// `Valoare` se duce înapoi așa cum a venit, pe AMBELE roluri: pe venit e cifra
// culeasă (serverul o folosește ca bază), pe linia de cost serverul o IGNORĂ (o
// recalculează din prețul lotului). Semnele unui document ANULAT se întorc ca
// atare — Apply le normalizează la magnitudini.
export function spreWrite(citit: RdcRead): RdcWrite {
  return {
    Data: citit.Data,
    PredatorId: citit.PredatorId,
    PrimitorId: citit.PrimitorId,
    Linii: (citit.Linii ?? []).map((l) => ({
      Id: l.Id,
      TipMaterialId: l.TipMaterialId,
      LotId: l.LotId,
      Cantitate: l.Cantitate,
      Valoare: l.Valoare,
      TipTvaId: l.TipTvaId,
    })),
  };
}
