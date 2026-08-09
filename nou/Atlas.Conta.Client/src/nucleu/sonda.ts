import { ia } from './http';

// Sondă de EXISTENȚĂ pe un set OData: răspunde DOAR la „id-ul ăsta e în setul
// ăsta?" (`$filter=ID eq …&$select=ID&$top=1`), nu citește entitatea — nu
// depinde de sintaxa de cheie și nu aduce date. Folosită unde ReadDto dă un id
// de `Repartitor` fără FELUL lui (distincția e a nomenclatoarelor, nu a
// documentului): deducerea laturii Partener/Angajat pe trezorerie, emitentul
// FCL în/în afara setului `UnitateInterna`.
//
// Eșecul NU e fatal: `false` înseamnă „nu e în set / nu știu", iar apelanții
// cad pe default-ul lor sigur (comutatorul manual, afișarea statică).
export async function existaInSet(entitate: string, id: string): Promise<boolean> {
  try {
    const r = await ia<{ value?: unknown[] }>(
      `/api/odata/${entitate}?$filter=${encodeURIComponent(`ID eq ${id}`)}&$select=ID&$top=1`);
    return (r.value?.length ?? 0) > 0;
  }
  catch {
    return false;
  }
}
