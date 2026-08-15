import type { components } from '../../generated/api-types';
import { ia, posteaza, pune, sterge } from '../../nucleu/http';
import { storeRemote } from '../../nucleu/dxStore';
import { azi } from '../../nucleu/zi';

// NUCLEUL PARTAJAT al feliilor de trezorerie — oglinda exactă a lui F3-D1 pe
// server: acolo `TrezorerieApply` e generic pe `T : DocumentTrezorerie` și două
// controllere subțiri îi dau ruta; aici o singură fabrică de client și două
// felii subțiri (`felii/plt`, `felii/inc`) care-i dau ruta și IDENTITATEA
// (titlu, laturi, captions). DTO-urile sunt literalmente aceleași scheme
// (`TrezorerieWriteDto`), deci un al doilea exemplar de `spreWrite` ar fi fost
// copie caracter cu caracter.
//
// Ce e PROPRIU trezoreriei, față de FCT:
//   • **`Numar` NU e în WriteDto** — PLT/INC au PoliticaNumerotare ⇒ numărul e
//     server-owned, se afișează ce se citește (gardianul refuză culegerea);
//   • **`Valoare` e CULEASĂ pe linie** — nu există `PregatesteOperare` pe
//     trezorerie: linia e defalcarea sumei, nu cantitate × preț;
//   • `Cantitate`/`LotId`/`TipTvaId`/`ValoareTva` n-au semantică aici și nu
//     intră în contract.

type Scheme = components['schemas'];

export type TrzRead = Scheme['TrezorerieReadDto'];
export type TrzWrite = Scheme['TrezorerieWriteDto'];
export type TrzLinieRead = Scheme['TrezorerieLinieReadDto'];
export type TrzLinieWrite = Scheme['TrezorerieLinieWriteDto'];
export type DocumentCopil = Scheme['DocumentCopilDto'];
export type OperareRezultat = Scheme['OperareRezultatDto'];
// Celălalt picior al viramentului (F8-D11): `Pereche` = starea REZOLVATĂ (link
// propriu SAU cine mă arată), `CandidatPereche` = ce se poate declara la
// culegere.
export type LaturaPereche = Scheme['LaturaPerecheDto'];
export type CandidatPereche = Scheme['CandidatPerecheDto'];

// Schemele OpenAPI (required/maxLength) sunt COMUNE celor două rute: același
// DTO generic pe server. Tipul de METADATA (captions) diferă — `Plata` vs
// `Incasare` — și îl dă felia.
export const SCHEMA_ANTET = 'TrezorerieWriteDto';
export const SCHEMA_LINIE = 'TrezorerieLinieWriteDto';
// Linia e frunza UNICĂ a celor două tipuri (DIM-2, Î1): `DocumentTrezorerieDetaliu`.
export const TIP_LINIE = 'DocumentTrezorerieDetaliu';

export type ApiTrezorerie = ReturnType<typeof apiTrezorerie>;

export function apiTrezorerie(baza: string) {
  return {
    citeste: (id: string) => ia<TrzRead>(`${baza}/${id}`),
    creeaza: (dto: TrzWrite) => posteaza<TrzRead>(baza, dto),
    actualizeaza: (id: string, dto: TrzWrite) => pune<TrzRead>(`${baza}/${id}`, dto),
    sterge: (id: string) => sterge(`${baza}/${id}`),

    // Dry-run (43 §2, stratul 2 autoritar): calculează + validează, fără
    // materializare. 200 cu listă goală = trece toți gardienii.
    valideaza: (id: string) => posteaza<{ Erori?: string[] | null }>(`${baza}/${id}/valideaza`)
      .then((r) => r.Erori ?? []),

    opereaza: (id: string) => posteaza<OperareRezultat>(`${baza}/${id}/opereaza`),
    anuleaza: (id: string) => posteaza<OperareRezultat>(`${baza}/${id}/anuleaza`),
    storneaza: (id: string, data: string) => posteaza<OperareRezultat>(`${baza}/${id}/storneaza`, { Data: data }),

    // Picioarele care pot fi DECLARATE pereche pentru laturile curente (F8-D11):
    // tipul OPUS, aceleași laturi, fără pereche definitivă. Listă MICĂ, plafonată
    // server-side — de asta e un simplu `GET`, nu un store `DataSourceLoader`:
    // e sursa unui SelectBox, nu a unei grile.
    //
    // `exclusId` = documentul curent (nu se poate arăta pe sine); pe un document
    // NOU nu există încă, deci se omite.
    candidatiPereche: (predatorId: string, primitorId: string, exclusId?: string) => {
      const p = new URLSearchParams({ predatorId, primitorId });
      if (exclusId) p.set('exclusId', exclusId);
      return ia<CandidatPereche[]>(`${baza}/candidati-pereche?${p.toString()}`);
    },

    storeLista: () => storeRemote(baza, 'Id'),
  };
}

export function antetGol(): TrzWrite {
  return { Data: azi(), Linii: [] };
}

export function linieGoala(): TrzLinieWrite {
  return { Valoare: 0 };
}

// ReadDto → WriteDto. Spre deosebire de FCT, aici NU există câmpuri necaptate
// în ecran care să ceară round-trip defensiv (`Valuta`/`Curs` n-au corespondent
// în contractul trezoreriei): tot ce e în WriteDto se culege pe ecran, deci
// harta e completă prin construcție. Dacă contractul crește, câmpul nou se
// adaugă AICI odată cu editorul lui — altfel PUT-ul, care e agregatul întreg,
// l-ar șterge.
export function spreWrite(citit: TrzRead): TrzWrite {
  return {
    Data: citit.Data,
    PredatorId: citit.PredatorId,
    PrimitorId: citit.PrimitorId,
    TipInstrument: citit.TipInstrument,
    NumarExtras: citit.NumarExtras,
    DataExtras: citit.DataExtras,
    // Legătura DECLARATĂ de acest document (F8-D6): se scrie o singură parte,
    // deci round-trip-ul e obligatoriu — fără el, orice PUT al unui virament
    // legat i-ar ȘTERGE legătura, iar re-operarea ar genera un al treilea
    // document (exact gaura 64k pe care felia o închide).
    LaturaPerecheId: citit.LaturaPerecheId,
    Linii: (citit.Linii ?? []).map((l) => ({
      Id: l.Id,
      TipMaterialId: l.TipMaterialId,
      Valoare: l.Valoare,
      AngajamentId: l.AngajamentId,
      CodEconomicId: l.CodEconomicId,
      SursaFinantareId: l.SursaFinantareId,
      CodFunctionalId: l.CodFunctionalId,
      ProiectId: l.ProiectId,
    })),
  };
}
