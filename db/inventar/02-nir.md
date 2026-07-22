# Inventar legacy — NIR (Nota de intrare-recepție) → `NIR`

Identitate legacy: `GEST_TIP_DOCUM` id 4, cod `NIR`; combo activ
`GEST_DEFA_DOCUM` id 14 (predator extern → primitor intern). Volum 2026: 300
documente. De regulă **autogenerat din FCT/BF** (conex, `AUTOGENERAT=1`), poate
exista și cules manual. Mecanica motorului: vezi `00-motor-postare.md`.

## Config combo (politici → seed)

| Politică | Valoare |
|---|---|
| Document conex | none (NIR e capătul lanțului FCT→NIR) |
| Tipuri stoc pe laturi | stoc_pred=1, stoc_prim=1 (magazie) |
| Reguli stoc | **+1 pe primitor** (gestiunea primește), tip stoc per tip produs — vezi tabelul |
| Modele note | 23 în `gest_defa_docum_nota_model` (defa 14) + fallback țintă pentru FCT |
| Validări/aprobări | template pe defa 14: TIP_VALIDARE=3, funcțiune 3, 10 zile grație |

## Reguli de stoc (defa 14) — singura intrare în stoc din lanțul de cumpărare

| Tip produs (`ID_GEST_TIP_PRODUSE`) | Tip stoc | Semn | Latura |
|---|---|---|---|
| 1, 7, 19–25 (materiale/consumabile) | 1 — Stoc magazie | +1 | primitor |
| 9 | 32 — Stoc gratuit | +1 | primitor |
| 16 (obiecte în folosință) | 8 — Stoc folosință | +1 | primitor |
| 17 (mărfuri) | 30 — Stoc mărfuri | +1 | primitor |
| 18 (custodie) | 31 — Stoc custodie | +1 | primitor |

(regulile sunt dublate pe `SEMN_ITEMS=±1` ⇒ fără filtru efectiv pe semn;
există și un set identic pe defa 258 — combo fantomă, întrebarea 00 §12.3)

La postare lotul (`GEST_GNMCL`) primește `id_document_receptie` = NIR-ul.

## Câmpuri poziție vii (defa 14)

Identic structural cu FCT (vin prin clonare conex): `PRODUS`,
`TIPMAT` (caption „LOT/Clasa Material"), `DENMAT`, `ID_GEST_TIP_MATERIAL`, `UM`,
`CANTITATE`, `PRET_UNITAR`, `COTA_TVA`, lanțul
`PRET_RECEPTIE → PRET_RECEPTIE_TVA → TVA_RECEPTIE → VALOARE_RECEPTIE →
VALOARE_RECEPTIE_TVA` (aceleași formule, precedență 1–5).

Diferențe față de FCT: `ContD`/`ContC` NU sunt vizibile (contarea vine integral
din modele per Clasă/Tip); `DETALII_ANGAJAMENT` invizibil; fără grup `DECONT_*`.

## Câmpuri header vii (defa 14)

Structurale + `NR_NOTA`/`DATA_NOTA`; numărul NIR-ului vine din `NR_DOC_CONEX`
cules pe factură sau din numerotarea automată. Nimic refolosit semantic detectat.

## Contare (note contabile)

23 modele pe defa 14, per Clasă/Tip (`ID_GEST_TIP_MATERIAL`):
`isnull(ContD, 302x/303x per clasă)` = `isnull(ContC, 4011 / 5324.01)`,
valoare `VALOARE_RECEPTIE_TVA`, repartitori = predator/primitor, dimensiuni de
pe linie. Aceste modele servesc și ca fallback pentru documentele al căror conex
e NIR (mecanismul din `spGestNotaCulegere`).

CNOTE 2026 nu are `cod_document='NIR'` ⇒ în producție notele lanțului de
cumpărare se emit sub FCT (întrebarea deschisă 00 §12.1 — de confirmat exact
cine generează).

## Mapare spre modelul nou

- **`NIR`** (header): referință la `FacturaIntrare` sursă (link conex tipizat),
  gestiune primitoare; restul structural în bază.
- **Detaliu**: liniile NIR creează **Loturile** (decizia 13) — `Lot` se naște
  aici cu preț fix (`PRET_RECEPTIE_TVA`), gestiune, dată; cantitatea NIR e
  singurul +1 în registrul de stoc pentru cumpărări.
- **Politici seed**: reguli registru stoc (+1 primitor × tip produs → tip stoc);
  modele contare per Clasă/Tip; filtrul de tipuri material al conexului
  (`GEST_ITEMSI_TIP_MATERIAL` defa 14) decide ce linii de FCT ajung pe NIR.
- Tipurile de stoc legacy (magazie/consum/folosință/custodie/mărfuri/gratuit) =
  candidații direcți pentru **registrele tipizate** din decizia 14.
