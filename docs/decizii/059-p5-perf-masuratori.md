# Decizia 59 — Măsurarea de perf a proiecțiilor pe baza de import

- **Data**: 2026-08-09 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §59
- **Docs**: docs/api/p5-perf-masuratori.md

---

**Măsurarea de perf a proiecțiilor pe baza de import — executată; datoria
D-2a/D-3a + `PoateGeneraDescarcare` ÎNCHISĂ ca măsurătoare**
(`docs/api/p5-perf-masuratori.md` — metodă, cifre, EXPLAIN). Pe clona de
import (205k documente, 337k linii, 46k imperecheri), HTTP cald, parametrii
reali de grilă: `Citeste` FCT/FCL 54–95ms (D-2a neproblemă — mulțime
mărginită 0–2 copii; `PoateGeneraDescarcare` +~40ms doar pe drumul complet,
scurt-circuitul funcționează), listele cu join pe agregat 135–150ms,
`rest-nedescarcat` ~50ms. Singurul peste prag: **`DocumenteCuRest` ~410ms**
per încărcare de panou (count ~175ms + pagină ~225ms; dominanta =
`GroupAggregate` pe TOATE cele 337k linii — cost structural, nu index
lipsă; filtrul pe contrapartidă nu intră în agregate). VERDICT: acceptabil
pentru release (panoul se deschide la cerere, nu e hot path), dar e singura
proiecție cu creștere liniară în totalul datelor (~2s la ~5 ani de volum);
optimizarea țintită e documentată în raport (contrapartida împinsă în
agregate + fără `requireTotalCount` pe panou — ambele aditive) și se face
CÂND cifra o cere, nu preventiv.
