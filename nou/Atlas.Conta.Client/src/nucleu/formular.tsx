import { createContext, useContext, useMemo, type ReactNode } from 'react';
import { campMeta, type CampMeta } from './campMeta';

// Starea de formular = agregatul WriteDto ca O SINGURĂ VALOARE locală (43c).
// `Formular` e doar conducta care o duce la editorii de câmp — nu e store, nu
// e global, moare cu ecranul. Nicio bibliotecă de formular: vezi §„verdict" din
// README — validarea autoritară vine de la server ca `string[]` neatașat de
// câmp, deci ce ar aduce RHF (resolvere, erori per câmp, uncontrolled perf) nu
// are cui folosi aici.

type StareFormular = {
  // Tipul de METADATA (entitatea XAF) — sursa captions-urilor.
  tip: string;
  // Schema OpenAPI a DTO-ului editat — sursa required/maxLength.
  schema: string;
  valoare: Record<string, unknown>;
  seteaza: (camp: string, v: unknown) => void;
  readOnly: boolean;
  // Erorile structurale se arată abia după prima încercare de trimitere:
  // un formular gol nu întâmpină operatorul cu roșu.
  aratErori: boolean;
};

const Context = createContext<StareFormular | null>(null);

export type PropsFormular<T extends object> = {
  tip: string;
  schema: string;
  valoare: T;
  onSchimba: (v: T) => void;
  readOnly?: boolean;
  aratErori?: boolean;
  children: ReactNode;
};

export function Formular<T extends object>(props: PropsFormular<T>) {
  const { tip, schema, valoare, onSchimba, readOnly = false, aratErori = false, children } = props;
  const stare = useMemo<StareFormular>(() => ({
    tip,
    schema,
    valoare: valoare as Record<string, unknown>,
    seteaza: (camp, v) => onSchimba({ ...valoare, [camp]: v }),
    readOnly,
    aratErori,
  }), [tip, schema, valoare, onSchimba, readOnly, aratErori]);
  return <Context.Provider value={stare}>{children}</Context.Provider>;
}

function useStareFormular(): StareFormular {
  const stare = useContext(Context);
  if (!stare)
    throw new Error('Editorii de câmp se folosesc doar în interiorul unui <Formular>.');
  return stare;
}

// Mesajele structurale sunt ȘABLONATE pe FELUL regulii, cu caption-ul
// interpolat (43a): un editor nou nu aduce stringuri noi.
function eroareStructurala(meta: CampMeta, valoare: unknown): string | null {
  const gol = valoare === undefined || valoare === null || valoare === ''
    || valoare === '00000000-0000-0000-0000-000000000000';
  if (meta.obligatoriu && gol)
    return `„${meta.caption}” este obligatoriu.`;
  if (meta.lungimeMaxima != null && typeof valoare === 'string' && valoare.length > meta.lungimeMaxima)
    return `„${meta.caption}” depășește ${meta.lungimeMaxima} caractere.`;
  return null;
}

export type LegaturaCamp<V> = {
  meta: CampMeta;
  valoare: V | undefined;
  seteaza: (v: V | undefined) => void;
  eroare: string | null;
  readOnly: boolean;
};

// Un editor tipizat = `useCamp` + controlul lui de input. Atât.
export function useCamp<V>(camp: string, readOnlyLocal = false): LegaturaCamp<V> {
  const stare = useStareFormular();
  const meta = campMeta(stare.tip, camp, stare.schema);
  const valoare = stare.valoare[camp] as V | undefined;
  return {
    meta,
    valoare,
    seteaza: (v) => stare.seteaza(camp, v),
    eroare: stare.aratErori ? eroareStructurala(meta, valoare) : null,
    readOnly: stare.readOnly || readOnlyLocal,
  };
}

// Verificarea stratului 1 pe TOT agregatul, înainte de a trimite: aceleași
// reguli structurale, aceleași mesaje. Stratul 2 (motorul) rămâne autoritatea.
export function eroriStructurale(
  tip: string, schema: string, valoare: Record<string, unknown>, campuri: string[]): string[] {
  return campuri
    .map((c) => eroareStructurala(campMeta(tip, c, schema), valoare[c]))
    .filter((e): e is string => e != null);
}
