# Decizia 26 — Felia 3c-1 — NIR + FacturaIntrare + mecanismul conex

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §26

---

**Felia 3c-1 — NIR + FacturaIntrare + mecanismul conex — executată;
validată e2e în ModelCheck (FCT cu linie stoc + linie serviciu → NIR conex
→ operare → gardieni grup → storno lanț). Tranșările:**
(a) **Întrebarea 00 §13.1 închisă:** recepția CONTEAZĂ pe NIR (3xx =
furnizor, valoarea cu TVA capitalizat); FacturaIntrare postează DOAR
liniile care nu trec pe NIR (servicii/cheltuieli/imobilizări). Granița e
Natura clasei — aceeași care filtrează conexul; fără dublă postare.
(b) **Conturile regulii de contare se rezolvă declarativ** (testul bazei
§7.2): enum `SursaCont` per latură (Explicit / TipMaterial /
PartenerPredator / PartenerPrimitor), contul explicit al regulii = valoare
directă sau fallback. `TipMaterial.ContImplicit` (FK nou) = maparea
Clasă/Tip → cont ca DATE (decizia 4), seed-uită din simbol (Cod-ul Tipului
E simbol de cont — potrivire exactă, apoi tăierea segmentelor terminale:
302.02.00.2 → 302.02.00). `Partener.ContImplicit` particularizează
creditorul (404 la furnizorii de imobilizări), fallback 401.01.00.
(c) **Prioritate la potrivirea regulilor**: RegulaContare — TipMaterial
exact → `NaturaFiltru` (câmp nou) → generică; fără regulă = linia nu
contează pe acel tip de document. RegulaStoc — regula specifică pe Clasă
bate genericul (Clasa=null = orice Natura=Stoc) per latură. Seed NIR:
+1 primitor, generic→Magazie, G/OF/MF/MC→registrele proprii.
(d) **PoliticaConex.NaturaFiltru înlocuiește lista m2m de tipuri permise**
(filtrul de conținut e funcțional natura liniei — decizia 21). Generarea
conexului trăiește ÎN motor, în tranzacția operării sursei: draft
`Autogenerat` cu `DocumentSursa`, clonă header (`InverseazaLaturi`) +
liniile eligibile (lot/dimensiuni/valoare incluse); fără linii eligibile
⇒ nu se generează. `Opereaza` întoarce conexul; controller-ul îl deschide
în editare. Grup conex la anulare/storno: copiii Operați → refuz
(conservator, există din 3b); copiii DRAFT autogenerați se ȘTERG odată cu
anularea sursei (artefact al operării; re-operarea regenerează).
(e) **Lotul se naște la culegere pe linia FACTURII** pentru lanțul conex
(echivalentul legacy: GEST_GNMCL.id_document_intrare = FCT):
`DocumentDetaliu.CreeazaLot(os, produs, gestiune)`; NIR-ul conex preia
LotId, NIR-ul manual își creează loturile pe propriile linii; motorul
finalizează lotul la operarea documentului-mamă și copiază atributele
culese (`ILinieCuAtributeLot`: DataExpirare/LotFabricatie).
`Lot.LinieIntrareId` = coloană FĂRĂ constrângere FK (intenționat): FK pe
ambele sensuri linie↔lot = ciclu de inserție pe care EF nu-l sparge, iar
ObjectSpace-ul XAF comite totul într-un singur SaveChanges.
(f) Validările proprii tipului în `ValideazaOperare` — FCT: numărul
furnizorului obligatoriu (FCT NU are politică de numerotare), clasificație
bugetară per linie (angajament SAU cod economic), laturi Partener→Gestiune,
cantități pozitive, linia de stoc cu lot creat; NIR: laturi
Partener→Gestiune, lot per linie, lotul în gestiunea primitoare.
Reconfirmat 25b în hooks: interogările NU ating navigații lazy în timpul
enumerării (proiecție cu Select înainte de materializare).
(g) Seed self-healing: regulile de contare cu sursă Explicit și fără cont
debitor (imposibil de rezolvat — reziduu de evoluție de schemă) se șterg
și se recreează la updater.
