import { ia } from '../../nucleu/http';

// F3-D7: `TRZ` e DEFAULT DE CULEGERE, nu validare. Linia unei plăți culese
// manual e defalcarea sumei, iar Tipul ei e tehnicul `TRZ` (convenția 31a) —
// dar lookup-ul rămâne NEFILTRAT: liniile autogenerate din factură poartă Tipul
// facturii-sursă (302/628…), iar o linie clonată nu are voie să devine
// necorectabilă în ecran.
//
// Se caută O SINGURĂ dată pe sesiune, pe ușa OData deja deschisă (43f) — nu
// există endpoint de „constante ale profilului" și nu se inventează unul:
// `TipMaterial` e nomenclator, iar `Cod == "TRZ"` e exact ce spune seed-ul.
let cerere: Promise<string | undefined> | null = null;

export function idTipTrz(): Promise<string | undefined> {
  cerere ??= ia<{ value?: { ID?: string }[] }>(
    `/api/odata/TipMaterial?$filter=${encodeURIComponent("Cod eq 'TRZ'")}&$select=ID&$top=1`)
    .then((r) => r.value?.[0]?.ID)
    // Un eșec nu se memorează: precompletarea lipsește o dată (operatorul alege
    // Tipul manual), dar următoarea linie încearcă din nou.
    .catch(() => { cerere = null; return undefined; });
  return cerere;
}
