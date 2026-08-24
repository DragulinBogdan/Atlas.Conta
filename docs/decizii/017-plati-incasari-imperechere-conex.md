# Decizia 17 — Plăți/încasări = tipuri de document

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §17

---

**Plăți/încasări = tipuri de document** (înlocuiesc registrul separat
bregistru/breg_p din legacy). **Imperecherea** (stingerea facturilor) =
entitate many-to-many plată↔factură cu sume parțiale (în legacy: m2m
gest_docum↔bregistru). **Document conex**: generare automată de document
legat cu link persistat (ex. NIR din Factură, deschis automat în editare)
— mecanism generic, de proiectat.
