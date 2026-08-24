# Decizia 8 — Metadata-driven UI

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §8

---

**Metadata-driven UI** (pipeline în două straturi, din arhitectura XAF/React):
   build-time OpenAPI→TS pentru tipare; runtime endpoint-uri custom de metadata
   serializate din XAF Application Model (captions, editor types, lookups,
   coloane ListView, layout DetailView, validări simple). Server-authoritative:
   metadata se filtrează prin securitate PE SERVER înainte de serializare.
