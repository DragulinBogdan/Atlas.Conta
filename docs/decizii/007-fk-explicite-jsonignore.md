# Decizia 7 — Nav properties în Web API

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă

## Regula durabilă

**Nav properties în API**: FK-urile explicite, `JsonIgnore` pe navigații.

---

**Nav properties în Web API**: se expun cheile străine (FK) explicit și se
   aplică `JsonIgnore` pe proprietățile de navigație (pattern deja validat
   în producție).
