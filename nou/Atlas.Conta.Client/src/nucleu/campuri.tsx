import { CheckBox, DateBox, NumberBox, SelectBox, TextBox } from 'devextreme-react';
import { CampShell } from './CampShell';
import { useCamp } from './formular';
import { valoriEnum } from './campMeta';
import { izolataZi } from './zi';

// Vocabularul de editoare (43a). Fiecare e o FAȚĂ SUBȚIRE peste `CampShell`:
// controlul de input + legătura la agregat. Identitatea editorului se scrie
// EXPLICIT în JSX (`<CampData<Antet> camp="Data" />`) — nu există nicăieri o
// hartă „câmp → editor" care s-o aleagă (registrul respins în 43a). Un editor
// nou = un fișier nou aici + un tag nou la locul folosirii; escape hatch prin
// construcție, zero cliff.
//
// Tiparea: componentele sunt generice peste DTO-ul editat, iar `camp` e
// `keyof T` — o greșeală de nume nu compilează.

export type PropsCamp<T extends object> = {
  camp: Extract<keyof T, string>;
  readOnly?: boolean;
  // Vezi `useCamp`: escapă declarată pentru câmpurile pe care serverul le lasă
  // nullable pe draft, dar le cere la operare. Implicit rămâne schema OpenAPI.
  obligatoriu?: boolean;
  // A doua escapă (vezi `useCamp`): felia numește câmpul în vocabularul ei
  // acolo unde caption-ul bazei e corect, dar prea abstract (PLT: predatorul E
  // contul propriu). Implicit rămâne `metadata.json`.
  eticheta?: string;
};

export function CampText<T extends object>({ camp, readOnly, obligatoriu, eticheta }: PropsCamp<T>) {
  const c = useCamp<string>(camp, readOnly, obligatoriu, eticheta);
  return (
    <CampShell meta={c.meta} eroare={c.eroare}>
      <TextBox
        value={c.valoare ?? ''}
        readOnly={c.readOnly}
        maxLength={c.meta.lungimeMaxima}
        // Regula F2/F3, uniformă pe orice widget DevExtreme: doar schimbările
        // operatorului (`e.event`) se propagă în formular (vezi `CampSelectie`).
        onValueChanged={(e) => { if (e.event) c.seteaza((e.value as string) || undefined); }}
      />
    </CampShell>
  );
}

// Datele circulă pe sârmă ca `DateOnly` ISO („2026-08-08") — se păstrează ca
// STRING în agregat: nicio conversie de fus orar nu are voie să atingă o dată
// contabilă.
export function CampData<T extends object>({ camp, readOnly, obligatoriu, eticheta }: PropsCamp<T>) {
  const c = useCamp<string>(camp, readOnly, obligatoriu, eticheta);
  return (
    <CampShell meta={c.meta} eroare={c.eroare}>
      <DateBox
        type="date"
        displayFormat="dd.MM.yyyy"
        value={c.valoare ?? null}
        readOnly={c.readOnly}
        onValueChanged={(e) => { if (e.event) c.seteaza(izolataZi(e.value)); }}
      />
    </CampShell>
  );
}

export function CampNumar<T extends object>({ camp, readOnly, obligatoriu, eticheta, zecimale = 3 }: PropsCamp<T> & { zecimale?: number }) {
  const c = useCamp<number>(camp, readOnly, obligatoriu, eticheta);
  return (
    <CampShell meta={c.meta} eroare={c.eroare}>
      <NumberBox
        value={c.valoare ?? undefined}
        readOnly={c.readOnly}
        format={`#,##0.${'#'.repeat(zecimale)}`}
        onValueChanged={(e) => { if (e.event) c.seteaza(e.value == null ? undefined : Number(e.value)); }}
      />
    </CampShell>
  );
}

// Bifă (`bool` pe sârmă). Valoarea implicită e `false`, nu `undefined`: un flag
// de comportament (FCT: „generează plata") nu are stare „nu m-am pronunțat" —
// serverul îl citește oricum ca fals.
export function CampBifa<T extends object>({ camp, readOnly, eticheta }: PropsCamp<T>) {
  const c = useCamp<boolean>(camp, readOnly, undefined, eticheta);
  return (
    <CampShell meta={{ ...c.meta, obligatoriu: false }}>
      <div className="camp__bifa">
        <CheckBox
          value={c.valoare ?? false}
          readOnly={c.readOnly}
          // Regula F2, valabilă pe ORICE widget DevExtreme: doar schimbările
          // OPERATORULUI (`e.event`) se propagă — schimbarea programatică
          // (seed-ul agregatului) ar re-raporta prin closure-ul VECHI al
          // contextului și ar șterge câmpurile abia scrise.
          onValueChanged={(e) => { if (e.event) c.seteaza(Boolean(e.value)); }}
        />
      </div>
    </CampShell>
  );
}

// Enum-ul (`TipInstrumentPlata`, …): pe sârmă circulă ca STRING — numele
// membrului, convenția `Stare` (F3-D1). Valorile și label-urile vin din
// `metadata.json`, adică din C#; clientul nu ține nicio listă proprie de
// instrumente de plată — o valoare nouă în enum apare aici la regenerarea
// dump-ului, fără atingerea acestui fișier.
// SelectBox peste o listă MICĂ dată de felie, nu peste un nomenclator OData
// (`Lookup`) și nu peste un enum din metadata (`CampSelectie`). Există pentru
// mulțimile pe care le calculează SERVERUL într-un endpoint propriu și care nu
// sunt nomenclator: candidații de latură pereche ai viramentului (F8-D12,
// `GET /api/{plt|inc}/candidati-pereche`) — o proiecție de domeniu, plafonată
// server-side, nu o entitate expusă prin OData.
//
// Contractul e deliberat sărac: felia aduce datele (TanStack Query, ca orice
// server-read — 43c) și le dă gata mapate; componenta nu știe să încarce nimic.
// Așa nu apare un al doilea mecanism de sursă de date lângă `Lookup`.
export function CampOptiuni<T extends object>(
  { camp, readOnly, obligatoriu, eticheta, optiuni, substitut, textFaraDate }: PropsCamp<T> & {
    optiuni: { valoare: string; label: string }[];
    // Ce înseamnă „nimic ales" — pe virament: „se generează automat" (F8-D12:
    // opțiunea goală E comportamentul normal, deci se NUMEȘTE, nu se lasă albă).
    substitut?: string;
    textFaraDate?: string;
  }) {
  const c = useCamp<string>(camp, readOnly, obligatoriu, eticheta);
  return (
    <CampShell meta={c.meta} eroare={c.eroare}>
      <SelectBox
        dataSource={optiuni}
        value={c.valoare ?? null}
        readOnly={c.readOnly}
        valueExpr="valoare"
        displayExpr="label"
        placeholder={substitut}
        searchEnabled
        searchExpr="label"
        noDataText={textFaraDate ?? 'Nimic găsit'}
        showClearButton={!c.meta.obligatoriu}
        // Regula F2/F3, uniformă pe orice widget DevExtreme.
        onValueChanged={(e) => { if (e.event) c.seteaza((e.value as string) ?? undefined); }}
      />
    </CampShell>
  );
}

export function CampSelectie<T extends object>(
  { camp, readOnly, obligatoriu, eticheta, enumerare }: PropsCamp<T> & { enumerare: string }) {
  const c = useCamp<string>(camp, readOnly, obligatoriu, eticheta);
  return (
    <CampShell meta={c.meta} eroare={c.eroare}>
      <SelectBox
        dataSource={valoriEnum(enumerare)}
        value={c.valoare ?? null}
        readOnly={c.readOnly}
        valueExpr="valoare"
        displayExpr="label"
        showClearButton={!c.meta.obligatoriu}
        // Regula F2 (vezi `Lookup`/`CampBifa`): doar `e.event` se propagă —
        // altfel seed-ul valorii declanșa un `seteaza` din closure-ul vechi
        // care RESETA agregatul abia încărcat (bug găsit la smoke F3: plata
        // autogenerată se deschidea cu laturile și liniile „dispărute").
        onValueChanged={(e) => { if (e.event) c.seteaza((e.value as string) ?? undefined); }}
      />
    </CampShell>
  );
}
