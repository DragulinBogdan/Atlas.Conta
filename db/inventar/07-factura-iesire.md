# Inventar legacy — FCT IESIRE (Factura de ieșire) → `FacturaIesire`

Identitate legacy: tip 12, cod `FCT IESIRE`; combo activ defa 47 (intern →
extern). Volum 2026: 28 documente, 22 linii CNOTE modul 1. Rama: direcție, nu
canon (decizia 21).

## Funcționalitate

Facturare către client. În acest deployment: **fără reguli de stoc** — pur
facturare/creanță (4111 = 7xx), fără descărcare de gestiune la facturare
(instituție publică: refacturări, chirii, servicii). Descărcarea de stoc la
vânzare de bunuri (dacă apare la alți clienți) ar fi document separat sau
reguli proprii — de decis la seed, nu moștenit.

## Familia de câmpuri: LIVRARE, nu RECEPTIE

Singura din setul țintă care folosește vizibil familia `PRET_LIVRARE` /
`VALOARE_LIVRARE` (`TOTALDOC` hardcodat pe `VALOARE_LIVRARE` pentru tip 12 —
00 §10). `VALOARE_RECEPTIE` există recaptionat „VALOARE" în paralel — dublă
familie pe aceeași linie, zgomot tipic al tabelei late; în modelul nou
`FacturaIesireDetaliu` are UN set de câmpuri de valoare.

## Config viu (defa 47)

- Poziție: `COD_CPV`, Clasă/Tip, `DENMAT`, UM, `CANTITATE` (obligatoriu),
  `PRET_UNITAR` → `PRET_LIVRARE` → `VALOARE_LIVRARE` (formule), `COTA_TVA`
  default 0, `DETALII_ANGAJAMENT`, `ContD` vizibil / **`ContC` obligatoriu**
  (simetric cu FCT intrare: contul de venit ales pe linie).
- Header: `NR_NOTA`/`DATA_NOTA`, NR_PV/DATA_PV (refolosiri),
  **`DATA_SCADENTA = data_docum + 30`** (formulă header — politică de scadență).
- Hardcodate (00 §10): exceptată de la obligativitatea angajamentului
  (clasificația bugetară e pe cheltuieli, nu pe venituri).

## Mapare spre modelul nou

- **`FacturaIesire`** (decizia 19: clasă separată de FacturaIntrare): client
  (`Partener` primitor), scadență cu politică de default, numerotare PROPRIE
  (serie fiscală!) — spre deosebire de FCT intrare unde numărul e al
  furnizorului. E-Factura/SPV pe emitere = cerință nouă de integrare (2026),
  inexistentă în legacy — de tratat la API, nu în model.
- Linia: descriere/Clasă-Tip, cantitate, preț, TVA, cont de venit (explicit sau
  politică per Clasă/Tip), dimensiuni de venit.
- Fără registru de stoc; doar registru contabil + imperechere cu încasările
  (decizia 17).
