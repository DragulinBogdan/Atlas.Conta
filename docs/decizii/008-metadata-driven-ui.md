# Decizia 8 — Metadata-driven UI

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă

## Regula durabilă

**Metadata-driven UI** doar pe criteriul deciziei 4: build-time = OpenAPI→TS
+ captions; runtime = doar politici-date; layout-ul per tip e cod React
(rafinat de 42e — serializarea layout-ului a MURIT).

---

**Metadata-driven UI** (pipeline în două straturi, din arhitectura XAF/React):
   build-time OpenAPI→TS pentru tipare; runtime endpoint-uri custom de metadata
   serializate din XAF Application Model (captions, editor types, lookups,
   coloane ListView, layout DetailView, validări simple). Server-authoritative:
   metadata se filtrează prin securitate PE SERVER înainte de serializare.
