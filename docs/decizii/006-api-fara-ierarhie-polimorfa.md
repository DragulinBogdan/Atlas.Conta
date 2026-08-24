# Decizia 6 — API-ul NU expune ierarhia polimorfic

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §6

---

**API-ul NU expune ierarhia polimorfic.** Endpoint-uri per tip de document
   (`/api/odata/NIR`, `/api/odata/BonConsum`, …) → DTO-uri TypeScript plate
   prin codegen OpenAPI→TypeScript. Opțional un endpoint read-only pe bază
   pentru registrul general de documente. Moștenirea e detaliu de persistență.
