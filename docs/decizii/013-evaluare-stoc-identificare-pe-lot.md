# Decizia 13 — Evaluare stoc

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §13

---

**Evaluare stoc: identificare specifică pe lot** (mecanism deliberat în
legacy prin codmat unic per intrare, păstrat și făcut first-class).
`Produs` (catalog, fost sumator) + `Lot` (fost codmat: creat de linia de
intrare, preț unitar fix, gestiune, dată). Liniile de ieșire referă lotul;
picking auto-FIFO în cadrul produs×gestiune, cu override manual.
O singură metodă de evaluare în motor — fără CMP/FIFO paralel.
