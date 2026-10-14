# 3 — Mapare EF Core

- Data: 2026-07-22
- Stare: amendată de 89
- Execuție: (TPT → TPH cu discriminatorul mapat `ClrType`, 2026-09-18; frunzele și `[TipDetaliu]` rămân)
- Arii: documente, motor, verificare

## Regula durabilă

**EF Core TPT** pentru header și pentru detaliile derivate (frunzele există
unde schema diferă; declarația per document = `[TipDetaliu]`, 40a).

---

**Mapare EF Core: TPT (table-per-type)** pentru header. Pentru detalii:
   de verificat la inventar dacă derivatele sunt necesare deloc (diferențele
   pot fi doar de obligativitate, nu de schemă → bază pură + validare).
