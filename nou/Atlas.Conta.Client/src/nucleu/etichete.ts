import { citesteNomenclator } from './odata';

// Precompletarea unei linii scrie DOUĂ lucruri: id-ul (care pleacă la server) și
// eticheta lui (care rămâne în client, pentru grila liniilor nesalvate —
// mecanismul 61b). Până la felia 20 le scriau două apeluri cu DOUĂ guard-uri
// diferite: `setLinie((prev) => prev.TipMaterialId ? prev : …)` citea starea
// PROASPĂTĂ, iar `setEtichete` de dedesubt citea `linie.TipMaterialId` din
// CLOSURE. Aceeași întrebare, două surse — deci două răspunsuri posibile, iar
// divergența s-ar fi văzut ca „grila arată alt Tip decât linia".
//
// F20-D3: o singură decizie („câmpul e gol?"), luată o dată, aplicată pe
// amândouă stările. Sursa etichetei e `citesteNomenclator` (cache-ul comun din
// F20-D2), nu un `$expand` imbricat pe lookup-ul de lot sau de produs: acolo
// costul ar cădea pe FIECARE tastare într-un nomenclator de sute de mii de
// rânduri, ca să folosim răspunsul o dată la o selecție.

export type EticheteTipMaterial = {
  TipMaterialCod?: string;
  TipMaterialDenumire?: string;
};

const text = (v: unknown) => (v == null ? '' : String(v));

type LinieCuTip = { TipMaterialId?: string | null };

export function precompleteazaTip<L extends LinieCuTip, E extends EticheteTipMaterial>(optiuni: {
  // Starea liniei AȘA CUM E ACUM. Ea dă verdictul „e gol?" — o singură dată,
  // înainte de orice `set*`. Câmpul nu se schimbă în același eveniment (lookup-ul
  // tocmai a scris ALT câmp: `ProdusId` sau `LotId`), deci citirea din render e
  // aceeași cu `prev`-ul updater-elor; ce nu mai poate face e să difere ÎNTRE
  // cele două scrieri.
  linie: L;
  // Tipul dedus din selecție (produsul ales, sau produsul lotului ales).
  tipId: string | undefined;
  setLinie: (f: (prev: L) => L) => void;
  setEtichete: (f: (prev: E) => E) => void;
}): void {
  const { linie, tipId, setLinie, setEtichete } = optiuni;
  // Precompletarea nu suprascrie NICIODATĂ o alegere a operatorului.
  if (!tipId || linie.TipMaterialId)
    return;

  // Update FUNCȚIONAL: `seteaza`-ul lookup-ului a rulat deja în același
  // eveniment (a scris `ProdusId`/`LotId`), iar un patch din closure l-ar pierde.
  setLinie((prev) => ({ ...prev, TipMaterialId: tipId }));

  // Eticheta vine ASINCRON, dar fără cerere în plus după prima: cheia
  // `['nomenclator','TipMaterial',id]` e aceeași pe care o folosesc lookup-urile
  // ca să-și rezolve afișarea. Verdictul e cel de mai sus, nu o a doua citire a
  // stării — între timp starea s-a schimbat deja.
  void citesteNomenclator<{ Cod?: unknown; Denumire?: unknown }>('TipMaterial', tipId).then(
    (t) => setEtichete((prev) => ({
      ...prev,
      TipMaterialCod: text(t?.Cod),
      TipMaterialDenumire: text(t?.Denumire),
    })),
    // Eticheta lipsă nu e un refuz de domeniu și nu blochează culegerea: linia
    // are id-ul corect, iar după Salvează grila arată ce spune ReadDto-ul
    // serverului. Nu se inventează nimic în locul ei.
    () => undefined,
  );
}
