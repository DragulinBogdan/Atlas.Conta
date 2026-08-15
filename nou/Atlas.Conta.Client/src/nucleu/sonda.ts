import { useQuery } from '@tanstack/react-query';
import { ia } from './http';

// Sondă de EXISTENȚĂ pe un set OData: răspunde DOAR la „id-ul ăsta e în setul
// ăsta?" (`$filter=ID eq …&$select=ID&$top=1`), nu citește entitatea — nu
// depinde de sintaxa de cheie și nu aduce date. Folosită unde ReadDto dă un id
// de `Repartitor` fără FELUL lui (distincția e a nomenclatoarelor, nu a
// documentului): deducerea laturii Partener/Angajat pe trezorerie, emitentul
// FCL în/în afara setului `UnitateInterna`.
//
// Eșecul PROPAGĂ: „nu știu" trebuie să rămână distinct de „nu e în set". Un
// `false` inventat pe eroare de rețea ar fi un răspuns AFIRMATIV mincinos —
// exact ce promite să nu facă `useSonda` mai jos („undefined cât timp nu
// știm"). Apelanții cad pe default-ul lor sigur prin `data === undefined`
// (retry-ul e oprit global în `main.tsx`, deci eroarea nu se repetă).
export async function existaInSet(entitate: string, id: string): Promise<boolean> {
  const r = await ia<{ value?: unknown[] }>(
    `/api/odata/${entitate}?$filter=${encodeURIComponent(`ID eq ${id}`)}&$select=ID&$top=1`);
  return (r.value?.length ?? 0) > 0;
}

// Aceeași sondă, ca HOOK — cu cache-ul TanStack pe cheia `['sonda', entitate,
// id]` (forma pe care o folosea deja `FclDetaliu`, 61c). Contează pentru
// viramentul intern (F7-D8): pe ecranul de trezorerie aceeași întrebare („e
// `ContPropriu`?") o pun DOUĂ componente — selectorul de fel al contrapartidei
// și shell-ul, care decide forma ecranului. Cheia comună le face să împartă un
// SINGUR răspuns; fără ea ar pleca două cereri identice la fiecare document.
//
// Întoarce `undefined` cât timp răspunsul nu a venit (sau proba e oprită) —
// apelanții rămân pe default-ul lor care nu minte, nu pe un `false` inventat.
export function useSonda(
  entitate: string, id: string | null | undefined, activa = true): boolean | undefined {
  return useQuery({
    queryKey: ['sonda', entitate, id],
    queryFn: () => existaInSet(entitate, id!),
    enabled: activa && id != null,
    // Apartenența unui id la un nomenclator nu se schimbă în viața ecranului.
    staleTime: Infinity,
  }).data;
}
