import { ia } from '../../nucleu/http';

// Id-ul unui TipMaterial după COD, pentru precompletarea Tipului liniei de
// trezorerie. F3-D7: e DEFAULT DE CULEGERE, nu validare — lookup-ul rămâne
// NEFILTRAT: liniile autogenerate din factură poartă Tipul facturii-sursă
// (302/628…), iar o linie clonată nu are voie să devină necorectabilă în ecran.
//
// Două coduri azi, pe același mecanism (F7-D8):
//   • `TRZ` — tehnicul plății/încasării obișnuite (convenția 31a);
//   • `VIR` — viramentul intern, unde Natura liniei (Virament) e exact
//     discriminarea de care depinde contarea prin 581 (F7-D2/F7-D3). Ce se
//     întâmplă dacă Tipul e greșit NU e treaba ecranului: motorul refuză
//     explicit necorelarea laturi ↔ natura liniilor.
//
// Se caută O SINGURĂ dată pe sesiune PER COD, pe ușa OData deja deschisă (43f)
// — nu există endpoint de „constante ale profilului" și nu se inventează unul:
// `TipMaterial` e nomenclator, iar `Cod == "…"` e exact ce spune seed-ul.
const cereri = new Map<string, Promise<string | undefined>>();

export function idTip(cod: string): Promise<string | undefined> {
  const memorat = cereri.get(cod);
  if (memorat) return memorat;
  const cerere = ia<{ value?: { ID?: string }[] }>(
    `/api/odata/TipMaterial?$filter=${encodeURIComponent(`Cod eq '${cod}'`)}&$select=ID&$top=1`)
    .then((r) => r.value?.[0]?.ID)
    // Un eșec nu se memorează: precompletarea lipsește o dată (operatorul alege
    // Tipul manual), dar următoarea linie încearcă din nou.
    .catch(() => { cereri.delete(cod); return undefined; });
  cereri.set(cod, cerere);
  return cerere;
}
