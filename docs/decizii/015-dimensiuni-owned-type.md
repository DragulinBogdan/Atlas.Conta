# Decizia 15 — Dimensiuni

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă; forma de stocare amendată de 54c (owned → plat pe frunze/registru)
- **Rezumat durabil**: `CLAUDE.md` §15

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
