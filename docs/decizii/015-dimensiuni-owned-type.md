# Decizia 15 — Dimensiuni

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă; forma de stocare amendată de 54c (owned → plat pe frunze/registru)

## Regula durabilă

**Dimensiunile** = setul R/M/CF/CE/F/U/P/CC (FK nullable fiecare); regula
de notă poartă `Comun`/`OverrideDebit`/`OverrideCredit`, rezolvarea
(coalesce) e generică în motor; linia de document poartă un set parțial,
rândul de registru unul complet, PER LATURĂ (25a). `DimensiuniObligatorii`
per cont = validare (33a). Forma de stocare: PLATĂ, pe frunze și registru
(54c — forma owned a murit). Override-urile pe linie din legacy
(ContD/ContC, RepD/RepC) NU se preiau.

---

**Dimensiuni: owned type `Dimensiuni`** (proprietate nullable + FK real
per dimensiune: Repartitor, Material, CodFunctional, CodEconomic,
SursaFinantare, Unitate, Proiect, CentruCost). Regula de notă poartă
`Comun` / `OverrideDebit` / `OverrideCredit`; rezolvarea (coalesce) e o
funcție generică în motor. Linia de document poartă `Dimensiuni` parțial;
rândul de registru/notă poartă unul complet rezolvat. Flag-urile de
defalcare din plan (R/M/E/B/F/P) devin date de validare: dimensiuni
obligatorii per cont. Override-urile de pe linia legacy (ContD/ContC,
RepD/RepC în GEST_ITEMSI — hack rapid, confirmat) NU se preiau; dacă
apare nevoie reală, adăugarea de override-uri pe linie e pur aditivă.
