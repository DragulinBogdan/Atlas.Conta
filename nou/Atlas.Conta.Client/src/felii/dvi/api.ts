import type { components } from '../../generated/api-types';
import { ia, posteaza, pune, sterge } from '../../nucleu/http';
import { storeRemote } from '../../nucleu/dxStore';
import { azi } from '../../nucleu/zi';

// `api.ts` DE MÂNĂ, cu tipurile GENERATE (43d) — șablonul FCT/NTC. Ce e PROPRIU
// feliei DVI stă tot aici:
//   • `Numar` e CULES: e MRN-ul declarației, nu o serie proprie (DVI n-are
//     politică de numerotare) — ca numărul furnizorului pe FCT;
//   • `ValoareTva` NU e override (ca la FCT), ci taxa DECLARATĂ în vamă: 0
//     înseamnă „se naște din cotă la operare", deci circulă în ambele sensuri;
//   • `FacturiIds` e agregatul ÎNTREG la fiecare salvare — serverul creează
//     lipsa și șterge plusul; panoul de facturi e o listă LOCALĂ de id-uri, nu
//     două verbe proprii;
//   • `Baza`/`Tva` vin de pe server (42c) și nu au variantă calculată în TS.
//     `Total (brut)` nu există pe DVI: baza plus taxa n-ar fi nici valoarea
//     vămuită, nici ce se datorează (DVI-D4).

type Scheme = components['schemas'];

export type DviRead = Scheme['DviReadDto'];
export type DviWrite = Scheme['DviWriteDto'];
export type DviLinieRead = Scheme['DviLinieReadDto'];
export type DviLinieWrite = Scheme['DviLinieWriteDto'];
export type DviFactura = Scheme['DviFacturaDto'];
export type FacturaCandidata = Scheme['FacturaCandidataDto'];
export type FacturiCandidate = Scheme['FacturiCandidateDto'];
export type OperareRezultat = Scheme['OperareRezultatDto'];

// Numele SCHEMELOR OpenAPI, ca stringuri, pentru `campMeta` (required/maxLength).
export const SCHEMA_ANTET = 'DviWriteDto';
export const SCHEMA_LINIE = 'DviLinieWriteDto';
export const SCHEMA_LISTA = 'DviListDto';
// Tipurile de METADATA (entitățile XAF) — sursa captions-urilor. Linia DVI stă
// pe BAZA `DocumentDetaliu` (DVI-D1), deci acolo se citesc etichetele ei.
export const TIP_ANTET = 'Dvi';
export const TIP_LINIE = 'DocumentDetaliu';

const BAZA = '/api/dvi';

export type CerereCandidati = {
  dataStart: string;
  dataEnd: string;
  partenerId?: string | null;
  toate?: boolean;
  // Declarația CURENTĂ, ca serverul să scoată din listă facturile deja legate.
  // Pe o declarație nouă, nesalvată, nu există — se omite.
  dviId?: string | null;
};

export const dvi = {
  citeste: (id: string) => ia<DviRead>(`${BAZA}/${id}`),
  creeaza: (dto: DviWrite) => posteaza<DviRead>(BAZA, dto),
  actualizeaza: (id: string, dto: DviWrite) => pune<DviRead>(`${BAZA}/${id}`, dto),
  sterge: (id: string) => sterge(`${BAZA}/${id}`),

  // Dry-run (43 §2, stratul 2 autoritar): calculează + validează, fără
  // materializare. 200 cu listă goală = trece toți gardienii.
  valideaza: (id: string) => posteaza<{ Erori?: string[] | null }>(`${BAZA}/${id}/valideaza`)
    .then((r) => r.Erori ?? []),

  opereaza: (id: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/opereaza`),
  anuleaza: (id: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/anuleaza`),
  storneaza: (id: string, data: string) => posteaza<OperareRezultat>(`${BAZA}/${id}/storneaza`, { Data: data }),

  // Facturile care se pot lega (DVI-D5). Perioada e OBLIGATORIE (serverul
  // refuză cu 400 fără ea) fiindcă ea mărginește interogarea; `toate` scoate
  // filtrul de clasă fiscală, iar plicul spune prin `MaiSunt` că lista s-a tăiat
  // la plafonul serverului și perioada trebuie îngustată.
  facturiCandidate: (cerere: CerereCandidati) =>
    ia<FacturiCandidate>(`${BAZA}/facturi-candidate?${parametriCandidati(cerere)}`),

  // Cheia grilei e `Id` (ReadDto), nu `ID` (entitatea) — DTO-ul e contractul.
  storeLista: () => storeRemote(BAZA, 'Id'),
};

// Parametrii ABSENȚI nu se trimit: un `partenerId=` gol ar pleca pe sârmă ca
// GUID invalid, iar `dviId=` gol ar cere excluderea legăturilor unei declarații
// inexistente (404).
function parametriCandidati(cerere: CerereCandidati): string {
  const p = new URLSearchParams({ dataStart: cerere.dataStart, dataEnd: cerere.dataEnd });
  if (cerere.partenerId) p.set('partenerId', cerere.partenerId);
  if (cerere.toate) p.set('toate', 'true');
  if (cerere.dviId) p.set('dviId', cerere.dviId);
  return p.toString();
}

export function antetGol(): DviWrite {
  return { Data: azi(), Linii: [], FacturiIds: [] };
}

// Linie NOUĂ: baza și taxa pornesc de la 0 ca orice câmp numeric al culegerii.
// Zero pe `Valoare` îl refuză TIPUL la operare; zero pe `ValoareTva` e legitim
// și înseamnă „calculeaz-o din cotă".
export function linieGoala(): DviLinieWrite {
  return { Valoare: 0, ValoareTva: 0 };
}

// ReadDto → WriteDto: EXACT câmpurile pe care operatorul are voie să le trimită.
// Ce lipsește e server-owned prin OMISIUNE: `Stare`, `Baza`, `Tva`, etichetele
// și afordanțele.
//
// `FacturiIds` se reconstituie din `Facturi[]` fiindcă PUT-ul e agregatul
// ÎNTREG: o legătură care n-ar ajunge înapoi în payload s-ar ȘTERGE.
export function spreWrite(citit: DviRead): DviWrite {
  return {
    Numar: citit.Numar,
    Data: citit.Data,
    PredatorId: citit.PredatorId,
    PrimitorId: citit.PrimitorId,
    Linii: (citit.Linii ?? []).map((l) => ({
      Id: l.Id,
      TipMaterialId: l.TipMaterialId,
      TipTvaId: l.TipTvaId,
      Valoare: l.Valoare,
      ValoareTva: l.ValoareTva,
    })),
    FacturiIds: (citit.Facturi ?? [])
      .map((f) => f.FacturaId)
      .filter((id): id is string => id != null),
  };
}
