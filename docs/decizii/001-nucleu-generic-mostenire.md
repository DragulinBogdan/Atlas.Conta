# Decizia 1 — Nucleu generic + moștenire, nu tabele complet separate

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă

## Regula durabilă

**Nucleu generic + moștenire, nu tabele separate.** `Document` +
`DocumentDetaliu` de bază, derivate per tip; motoarele de stoc și contabile
consumă DOAR baza.

---

**Nucleu generic + moștenire, nu tabele complet separate.**
   Clase de bază `Document` (header) și `DocumentDetaliu`, cu clase derivate
   per tip de document. Motoarele de stoc și contabile consumă DOAR clasa de bază.
