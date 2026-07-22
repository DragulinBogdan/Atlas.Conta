# Inventar legacy — LDI (Lista diferențe inventar) → `ListaDiferenteInventar`

Identitate legacy: tip 15, cod `LDI`; combo activ defa 270 (intern → intern).
Volum 2026: 7 documente. Rama: direcție, nu canon (decizia 21).

## Funcționalitate

Înregistrează diferențele constatate la inventariere: plusuri și minusuri pe
aceeași listă, direcția dată de **semnul cantității** liniei (singurul tip din
setul țintă unde filtrul `SEMN_ITEMS` chiar diferențiază — regulile legacy au
`SEMN=+1` pe predator pentru ambele semne de linie, deci plus adaugă, minus
scade prin cantitatea negativă).

## Reguli de stoc (defa 270)

+1 pe predator (gestiunea inventariată), tip stoc per tip produs
(1 magazie / 8 folosință / 31 custodie), cantitatea semnată dă direcția.
Zgomot: setul e duplicat integral pe cheiere veche (tip document, defa NULL) —
se ignoră (decizia 21).

## Contare

2 rânduri gen. veche pe combo-urile LDI + `ContD`/`ContC` vizibile pe linie.
Funcțional: plus la inventar = 3xx = 758/791 (venit), minus = 6xx = 3xx
(cheltuială), pe Clasă/Tip — de definit curat la seed; legacy-ul acoperea
cazurile prin override manual pe linie, dovadă că politica era incompletă.

## Câmpuri vii pe poziție (defa 270)

Clasă/Tip (`ID_GEST_TIP_MATERIAL`, `TIP_MATERIAL`, `TIPMAT`), `DENMAT`, UM,
`CANTITATE` (semnată), `PRET_UNITAR`, `VALOARE_RECEPTIE` (formulă), lot
(`LOT_FABRICATIE`, `DATA_EXPIRARE`), dimensiuni (`DETALII_ANGAJAMENT`),
`ContD`/`ContC`.

## Particularitate: plusul creează lot

Minusul descarcă un lot existent (ca un consum); **plusul creează lot nou**
(ca un NIR) — cu preț de evaluare cules. LDI e singurul tip bidirecțional pe
loturi din setul țintă; în modelul nou probabil două colecții de detalii sau
un detaliu cu direcție explicită (nu semn implicit pe cantitate) — de decis la
modelul nou (candidat: `DiferentaPlus`/`DiferentaMinus` ca sub-linii tipizate
sau enum `DirectieDiferenta` pe linie).

## Mapare spre modelul nou

- **`ListaDiferenteInventar`**: gestiune inventariată; comisie (repartitor cu
  calitate „Comisie" — vezi decizia 16) de verificat ca cerință.
- Registru: aceleași registre ca NIR/BCS, direcție per linie.
- Semnul pe cantitate → direcție explicită (elimină convenția implicită).
