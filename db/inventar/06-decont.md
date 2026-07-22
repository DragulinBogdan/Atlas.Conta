# Inventar legacy — DEC (Decont) → `Decont`

Identitate legacy: tip 20, cod `DEC`; combo activ defa 289 (predator extern →
primitor „oricare"). Volum 2026: 15 documente (12 pe combo 289), 8+ linii CNOTE
modul 1. Rama: direcție, nu canon (decizia 21).

## Funcționalitate

Justificarea avansurilor / cheltuielilor făcute de un titular (angajat):
documentele-suport (bonuri, facturi mici) se înregistrează pe linii și se
postează pe conturi alese explicit. Legat de familia `spDecontari*`,
`SP_GET_DECONT`, `SP_JUSTIFICARI_DECONTARI` (de acoperit la capitolul
plăți/imperechere).

## Particularitatea definitorie: postare explicită pe linie

Pe defa 289, **`ContD`, `RepD`, `ContC`, `RepC` sunt toate vizibile pe linie**
— unicul tip din setul țintă cu override complet (cont + repartitor, ambele
laturi). Decontul legacy e de facto o notă contabilă manuală cu identitate de
document. Fără reguli de stoc (nu mișcă stoc), 4 rânduri gen. veche de conturi
implicite.

Nuanță pentru decizia 15 (consemnată și în 00 §13.4): specificarea explicită a
postării pe linie NU devine mecanism generic de override, dar e trăsătura
PROPRIE a tipului `Decont` — linia lui poartă contul (și eventual repartitorul)
țintă ca date de primă clasă.

## Câmpuri vii (defa 289)

Poziție: Clasă/Tip, `DENMAT` (descrierea cheltuielii), UM/`CANTITATE`
(defaults `'BUC'`/`'1'` — pro-forma), `PRET_UNITAR`, lanțul de valori cu TVA,
`DETALII_ANGAJAMENT` + dimensiuni bugetare, `ContD`/`RepD`/`ContC`/`RepC`.
Header: `NR_NOTA`/`DATA_NOTA` + refolosirile NR_PV/DATA_PV.

## Mapare spre modelul nou

- **`Decont`**: titular = `Angajat` (predator; caz direct pentru derivata
  `Angajat` din decizia 16), fără registru de stoc, doar registru contabil.
- Linia: descriere + valoare + cont țintă + dimensiuni; validare pe angajament.
- Fluxul complet (confirmat proprietar, detaliat în `09-plati-imperechere.md`):
  avansul și închiderea lui trăiau în BREGISTRU (casa/bancă) ca înregistrări cu
  referință între ele (`PARENT_COD`/`NR_DECONT`); decontul justifica
  cheltuielile. În modelul nou: avans = `Plata` către angajat, `Decont`
  justifică, diferența = `Plata`/`Incasare` de regularizare, toate legate prin
  imperechere — lanț explicit, nu convenție de coduri.
