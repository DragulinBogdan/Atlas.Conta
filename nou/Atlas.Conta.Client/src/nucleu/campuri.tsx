import { DateBox, NumberBox, TextBox } from 'devextreme-react';
import { CampShell } from './CampShell';
import { useCamp } from './formular';
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
};

export function CampText<T extends object>({ camp, readOnly, obligatoriu }: PropsCamp<T>) {
  const c = useCamp<string>(camp, readOnly, obligatoriu);
  return (
    <CampShell meta={c.meta} eroare={c.eroare}>
      <TextBox
        value={c.valoare ?? ''}
        readOnly={c.readOnly}
        maxLength={c.meta.lungimeMaxima}
        onValueChanged={(e) => c.seteaza((e.value as string) || undefined)}
      />
    </CampShell>
  );
}

// Datele circulă pe sârmă ca `DateOnly` ISO („2026-08-08") — se păstrează ca
// STRING în agregat: nicio conversie de fus orar nu are voie să atingă o dată
// contabilă.
export function CampData<T extends object>({ camp, readOnly, obligatoriu }: PropsCamp<T>) {
  const c = useCamp<string>(camp, readOnly, obligatoriu);
  return (
    <CampShell meta={c.meta} eroare={c.eroare}>
      <DateBox
        type="date"
        displayFormat="dd.MM.yyyy"
        value={c.valoare ?? null}
        readOnly={c.readOnly}
        onValueChanged={(e) => c.seteaza(izolataZi(e.value))}
      />
    </CampShell>
  );
}

export function CampNumar<T extends object>({ camp, readOnly, obligatoriu, zecimale = 3 }: PropsCamp<T> & { zecimale?: number }) {
  const c = useCamp<number>(camp, readOnly, obligatoriu);
  return (
    <CampShell meta={c.meta} eroare={c.eroare}>
      <NumberBox
        value={c.valoare ?? undefined}
        readOnly={c.readOnly}
        format={`#,##0.${'#'.repeat(zecimale)}`}
        onValueChanged={(e) => c.seteaza(e.value == null ? undefined : Number(e.value))}
      />
    </CampShell>
  );
}
