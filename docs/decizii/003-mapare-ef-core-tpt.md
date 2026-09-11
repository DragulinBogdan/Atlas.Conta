# Decizia 3 — Mapare EF Core

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă

## Regula durabilă

**EF Core TPT** pentru header și pentru detaliile derivate (frunzele există
unde schema diferă; declarația per document = `[TipDetaliu]`, 40a).

---

**Mapare EF Core: TPT (table-per-type)** pentru header. Pentru detalii:
   de verificat la inventar dacă derivatele sunt necesare deloc (diferențele
   pot fi doar de obligativitate, nu de schemă → bază pură + validare).
