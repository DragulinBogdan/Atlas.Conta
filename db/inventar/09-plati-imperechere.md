# Inventar legacy — Casa/Bancă, plăți și imperechere → `Plata` / `Incasare` / `Imperechere`

Surse: `BREGISTRU`/`BREG_P`/`casierie`, `GEST_DECONTARI`,
`spDecontariObligatii`, `spDecontariPlatiIncasari`, `spGestValideazaDocument`
(§ plata automată), confirmările proprietarului. Volum 2026: BREGISTRU 1.151,
BREG_P 1.175, CNOTE modul 2 (OP/DCP/CEC) ~1.145 linii. Rama: direcție, nu
canon (decizia 21).

## 1. Modelul legacy

- **`casierie`** = nomenclatorul registrelor de numerar/bancă (`cod_cb`,
  `is_banca`) — casele și conturile proprii.
- **`BREGISTRU`** = registrul de casă/bancă: o înregistrare per operațiune, cu
  `INCASARI` XOR `PLATI` (direcția prin coloana populată), `TIPDOC`
  (OP/CEC/DCP/chitanță), `CONT_CSP` (contul propriu), `CODGEST` (partenerul),
  `NR_EXTRAS`/`DATA_EXTRAS` (extrasul bancar), `SOLD_INITIAL`, transferuri
  între registre (`TRANSFER`, `COD_TRANSFER`, `COD_CBT`).
- **`BREG_P`** = defalcarea bugetară a operațiunii (cod funcțional/economic,
  proiect, unitate) — echivalentul dimensiunilor pe plată.
- **Avans spre decontare** (confirmat): avansul și închiderea lui erau
  înregistrări în BREGISTRU **cu referință între ele** (`PARENT_COD`,
  `NR_DECONT`/`DATA_DECONT`); documentul DEC justifica cheltuielile.

## 2. Imperecherea (stingerea)

`GEST_DECONTARI(id_gest_docum, id_bregistru, SUMA, AUTOGENERAT)` — m2m
document↔operațiune de plată **cu sume parțiale**. Exact modelul din decizia 17.

Ecranul de imperechere consuma două liste simetrice:
- `spDecontariObligatii`: documentele plătibile (combo-uri eligibile prin
  `fnDocLichidare()`), cu `totaldoc`, `asignat` (Σ imperecheri), `ramas`,
  procent; direcția plată/încasare dedusă din intern/extern pe
  predator/primitor; grupul conex folosit ca identitate a obligației.
- `spDecontariPlatiIncasari`: operațiunile din BREGISTRU cu `asignat`/`ramas`.

Plus generarea automată la postarea FCT (00 §7): plata + imperecherea +
defalcarea pe poziții (`GEST_DEFALCARE_DECONTARI`) create dintr-un foc când
operatorul cere (`DECONT_GENERATE`).

## 3. Mapare spre modelul nou (decizia 17)

- **`Plata` / `Incasare`** = tipuri de document: predator/primitor tipizați —
  cont propriu (casierie/bancă) ↔ `Partener`/`Angajat`. Registrul de casă/bancă
  devine **registrul contabil al documentelor de plată** + un registru de sold
  per cont propriu; BREGISTRU ca tabelă separată dispare.
- **Contul propriu (casierie/bancă)** = entitate nouă în familia Repartitori
  (derivată `ContPropriu` sau calitate pe UnitateInterna — de decis la modelul
  nou; are atribute proprii: IBAN, trezorerie, is_banca ⇒ înclin spre derivată).
- **`Imperechere`** = entitate m2m plată↔document cu sumă (partajări parțiale),
  cu invariantele: Σ imperecheri ≤ totalul plății și ≤ totalul documentului;
  `ramas` devine calcul, nu coloană.
- **Avans → decont → regularizare**: avansul = `Plata` către angajat;
  `Decont` justifică; diferența = `Plata`/`Incasare` de regularizare; toate
  legate prin imperechere cu decontul/între ele (înlocuiește referințele
  `PARENT_COD`). Lanț explicit, nu convenție de coduri.
- **Extrasul bancar** (`NR_EXTRAS`, import `xml_trezor` — 1.342 rânduri!):
  legacy importa extrase de trezorerie. Cale de import pentru `Plata`/`Incasare`
  (simetric cu importul FCT, decizia 21) — de tratat la API.
- Dimensiunile din BREG_P → `Dimensiuni` pe documentul/liniile de plată
  (decizia 15); defalcarea multi-sursă (cofinanțare) acoperă cazul BREG_P cu
  mai multe felii per plată.

## 4. Întrebări închise / rămase

- Închis: fluxul avans-decont (referință BREGISTRU↔BREGISTRU, formalizat mai
  sus). Închis: imperecherea pe poziții există (`GEST_DEFALCARE_DECONTARI`) dar
  doar la generarea automată — de decis dacă modelul nou o păstrează
  (00 §13.3).
- Deschis: transferurile între registre proprii (`TRANSFER`/`COD_CBT`) —
  probabil pereche Plata+Incasare legate conex; de confirmat la modelul nou.
- Deschis: `ORDINE_PLATA`/`CONTARE_ORDINE_PLATA` (goale aici) — ordinele de
  plată ca flux separat pre-plată; de verificat dacă intră în faza 1.
