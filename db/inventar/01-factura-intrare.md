# Inventar legacy — FCT (Factura fiscală de intrare) → `FacturaIntrare`

Identitate legacy: `GEST_TIP_DOCUM` id 2, cod `FCT`; combo activ
`GEST_DEFA_DOCUM` id 6 (predator extern → primitor intern). Volum 2026:
1.474 documente (1.387 pe combo 6). Mecanica motorului: vezi `00-motor-postare.md`.

**Proveniență (confirmat proprietar):** facturile vin parțial/complet dintr-un
sistem extern (integrarea Tethys: `TethysId` pe header, `TethysDOCUMENTE`,
resursele `legacy/Tethys/*.xtr`). `FacturaIntrare` din modelul nou are nevoie
de o **cale de import** ca cetățean de primă clasă (creare draft din sursă
externă + culegere completare), nu doar culegere manuală.

## Config combo (politici → seed)

| Politică | Valoare |
|---|---|
| Document conex | NIR (tip 4), `TIP_DESCARCARE=0` (fără swap predator/primitor) |
| Numerotare | nr. furnizor (manual); NR/DATA_DOC_CONEX culese pentru NIR-ul generat |
| Reguli stoc | **niciuna** — factura nu mișcă stoc; intrarea o face NIR-ul conex |
| Modele note | 140 în `gest_defa_docum_nota_model` (defa 6) |
| Plată automată | opțional per document (`DECONT_GENERATE` + câmpurile `DECONT_*`) |
| Clasificație bugetară | obligatorie (`clasificatieObligatorie=1`): fiecare linie cere angajament sau cod economic |

## Câmpuri header vii (din `GEST_DEFA_DOCUM_DOCUMENT`, defa 6)

Structurale (bază, decizia 2): predator (furnizor), primitor (gestiune),
`NR_DOCUM`, `DATA_DOCUM`, `DATA_SCADENTA`, totaluri.

Vizibile specifice: `COD_CPV`, `NR_NOTA`, `DATA_NOTA`,
`DESCRIERE_CURS`→**NR_PV** (refolosit!), `DATA_EMITERE`→**Data PV** (refolosit!).

Formule header: totaluri `SUM(...)` peste poziții (re-evaluate la postare);
`TOTALDOC = Σ VALOARE_RECEPTIE_TVA` (hardcodat în SP, nu în config).

Grup `DECONT_*`: `DECONT_GENERATE`, `DECONT_COD_CB`, `DECONT_NRDOC`,
`DECONT_DATA`, `DECONT_TIPDOC`, `SE_GEN_CHITANTA`, `NR_CHITANTA`,
`DATA_CHITANTA` — parametrii plății automate (în modelul nou: parametri de
generare a documentului conex Plata, nu câmpuri pe factură).

## Câmpuri poziție vii (din `GEST_DEFA_DOCUM_ITEMSI`, defa 6)

| Câmp | Obs |
|---|---|
| `PRODUS` | default `'M'` |
| `ID_GEST_TIP_MATERIAL` / `TIPMAT` / `DENMAT` | Clasă/Tip + denumire |
| `UM` | default `'BUC'` |
| `CANTITATE` | obligatoriu, default `'1'` |
| `PRET_UNITAR`, `COTA_TVA` | culese |
| `PRET_RECEPTIE` → `PRET_RECEPTIE_TVA` → `TVA_RECEPTIE` → `VALOARE_RECEPTIE` → `VALOARE_RECEPTIE_TVA` | lanț de formule (precedența 1–5); `VALOARE_RECEPTIE_TVA` readonly |
| `DETALII_ANGAJAMENT` | descrierea angajamentului bugetar (readonly, calculat) |
| `ID_ANGAJAMENTE_DEFALCARE`, `COD_FUNCTIONAL`, `COD_ECONOMIC` | dimensiuni bugetare |
| `ContD` (readonly), **`ContC` (vizibil + OBLIGATORIU)** | pe FCT operatorul alege explicit contul creditor per linie |
| `DATA_EXPIRARE`, `LOT_FABRICATIE`, `CATEGORIE_GRUPARE`, `COD_CPV` | atribute de lot/achiziție |
| `CANTITATE_SUPLIMENTARA = CANTITATE × CONVERSIE_UM` | UM duală (configurata, nefolosită vizibil aici) |

Observație importantă: pe FCT, override-ul de linie `ContC` NU e hack marginal —
e fluxul principal (obligatoriu). În modelul nou echivalentul: alegerea contului
creditor devine fie politică per tip partener (`REPARTITORI_TIPURI.CONT`:
401/404), fie câmp explicit pe linia FacturaIntrare — de decis la modelul nou,
NU se rezolvă prin override de dimensiuni.

## Reguli de stoc

Niciuna pe combo 6. Lotul se creează la FCT în `GEST_GNMCL`
(`id_document_intrare` = factura), dar mișcarea cantitativă o înregistrează NIR-ul.

## Contare (note contabile)

- 140 modele pe defa 6: per `ID_GEST_TIP_MATERIAL` → `isnull(ContD_linie, cont
  clasă)` = `isnull(ContC_linie, 401.01.00 / 404.01.00 / 4011 ...)`, valoare =
  `VALOARE_RECEPTIE_TVA`, repartitor D/C = predator/primitor, dimensiuni de pe
  linie.
- `GEST_DEFA_NOTA_CONT` (gen. veche, defa 6): 302x/303x/205 = 401/404 — azi doar
  sursă de conturi implicite.
- CNOTE 2026: 629 linii cu `cod_document='FCT'`, modul 1 — factura postează.
- DESCHIS (00 §12.1): partajarea contării FCT vs NIR conex — de lămurit înainte
  de seed, risc dublă postare.

## Validări

- Hardcodate: cantitate negativă interzisă pe `produs='M'` (tip 2);
  angajament/cod economic obligatoriu per linie (clasificație).
- `GEST_TEMPLATE_VALIDARI`: nu are template pe defa 6 în 2026 (aprobările sunt pe
  alte combo-uri) — de confirmat pe restul deployment-urilor.

## Mapare spre modelul nou

- **Bază** (`Document`): predator/primitor, nr/dată, dată scadență, totaluri
  (calculate), stare.
- **`FacturaIntrare`**: NR_PV/Data PV (câmpuri proprii, redenumite corect),
  COD_CPV, scadență?, referință NIR conex, parametri generare plată.
- **`FacturaIntrareDetaliu`** (dacă rămân derivate pe detalii): lanțul de valori
  recepție e candidat la bază (intră în note/stoc via NIR); `ContC` de decis.
- **Politici seed**: conex FCT→NIR + filtru tipuri material; modele contare per
  Clasă/Tip; obligativitate clasificație bugetară.
