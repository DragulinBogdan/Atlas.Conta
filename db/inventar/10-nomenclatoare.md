# Inventar legacy — Nomenclatoare → Repartitori, Produs/Lot, Clasă/Tip, Plan de conturi

Rama: direcție, nu canon (decizia 21).

## 1. Repartitori (841 entități, 17 tipuri, clasificare m2m)

Distribuția clasificărilor: Furnizori 390, Gestiuni 103, Gestionar 8,
Clienți 7, Departament 6, Unități 4, Investiții/Salariați/Debitori câte 1.

**Matricea de suprapunere reală** (validează decizia 16):

| Suprapunere | Nr | Concluzie |
|---|---|---|
| Furnizori ∩ Clienți | 6 | rol contextual pe `Partener`, NU clase separate ✔ |
| Furnizori ∩ Debitori | 1 | idem — calități de sold pe `Partener` |
| Gestiuni ∩ Departament / Unități ∩ Departament / Gestiuni ∩ Unități | 1+1+1 | organizatoric = calități transversale, nu identități |

Nicio suprapunere Salariați∩Furnizori în date. Concluzie pentru modelul nou:
derivate TPT `Partener`, `Angajat`, `Gestiune`, `UnitateInterna` (+`ContPropriu`
din 09); Gestionar/Comisie/Departament/CentruCost/LocConsum = calități.
`REPARTITORI_TIPURI.CONT` (401/404, 411, 302/303…) = politică: cont implicit
per tip/calitate. Tabele satelit legacy (adrese, conturi bancare
`REPARTITORI_CONTURI` 427 rânduri, delegați) — de preluat pe entitățile noi.

## 2. Clasă/Tip: GEST_TIP_PRODUSE + GEST_TIP_MATERIAL

- **`GEST_TIP_PRODUSE`** (23, „Clasa") — clasifică NATURA liniei, nu doar
  marfa: Materiale, Produse, Servicii, Cheltuieli, Salarii, Mijloace Fixe,
  Obiecte de Inventar / în Folosință, Mărfuri, Custodie, Combustibil,
  Medicamente…, plus tehnice (TVA, Diferență Preț, Diferențe Curs, Cursanți).
  Regulile de stoc se agață de Clasă (tip stoc per clasă — vezi 02/03).
- **`GEST_TIP_MATERIAL`** (43, „Tipul") — nivelul de cont: denumirea CONȚINE
  contul („302.01.00: Materiale auxiliare", „303.02.00: … în folosință");
  fiecare Tip aparține unei Clase. Modelele de contare se agață de Tip.

Confirmă modelul din CLAUDE.md: Clasă ≈ natura operațională (dă registrul de
stoc), Tip ≈ maparea contabilă (dă conturile). În modelul nou ambele rămân
nomenclatoare-politică; curățare la seed: separarea claselor tehnice
(TVA/Diferențe) de cele de stoc — probabil enum de natură + nomenclator.

## 3. Produs / Lot: GEST_SUMATOR + GEST_GNMCL

- `gest_sumator` (7.459) = catalogul real („Produs" din decizia 13);
  `GEST_SUMATOR_FIELDS` definește cheile de grupare.
- `GEST_GNMCL` (4.133) = loturile (codmat): preț recepție, gestiune intrare,
  `id_document_intrare`/`id_document_receptie`, data_cod, data expirare, lot
  fabricație.
- Anomalie de semnal: sumatori > loturi (7.459 > 4.133) — catalogul are și
  intrări nefolosite/istorice; curățare la migrare (se migrează doar
  produsele cu sold sau referite de solduri de deschidere).

## 4. Plan de conturi: CPLAN (1.679)

- 1.569 sintetice pure (`TIP=S`, `BALANTA=S`), ~110 cu defalcare:
  E (execuție/funcțional) 70, R (repartitor) 10, combinații multi-dimensiune
  (BFE, FE, FER, RFE, BE, BFEPR…) — un cont poate cere MAI MULTE dimensiuni
  simultan.
- Confirmă deciziile 10/15: plan doar sintetic; flag-urile de defalcare devin
  date de validare „dimensiuni obligatorii per cont" (setul complet
  R/M/E/B/F/P + sursa de finanțare nouă).
- `CPLAN_DEFALCARE` (38) = explicațiile de defalcare; `FCTCONT` (D/C/B) și
  `SUMATOR` (însumare vs soldare pe părinți) — atribute de preluat pe planul
  nou.

## 5. Alte nomenclatoare de preluat

`PERIOADE_FISCALE` (închidere — pivotul gardienilor din decizia 14),
`casierie` (conturi proprii → 09), `JUDETE`/`LOCALITATI`/`TARI` (adrese),
`valuta_tip`/cursuri (valută pe FCT externe), `BG_PLAN_FUNCTIONAL`/
`BG_PLAN_ECONOMIC` (planurile bugetare — dimensiunile funcțional/economic),
`OI_PROIECTE`/`OI_UNITATI` (dimensiunile proiect/unitate).
