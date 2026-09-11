# Decizia 16 — Repartitori

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă

## Regula durabilă

**Repartitori: TPT bază + derivate** doar unde schema diferă și identitatea
e exclusivă (Partener, Gestiune, Angajat, UnitateInterna, ContPropriu);
calitățile transversale (Gestionar, Comisie, LocConsum, CentruCost…) =
flags `Calitati`, nu clase. Furnizor/client = rol dat de poziția pe
document, o singură clasă `Partener`.

---

**Repartitori: TPT bază + derivate, cu regula:** moștenire doar unde
schema diferă și identitatea e exclusivă; calitățile transversale
(centru de cost, delegat, cursant etc.) = roluri/flags, nu clase derivate.
Furnizor/client = rol contextual dat de poziția pe document, o singură
clasă `Partener`. Dublare acceptată pentru cazuri exotice.
