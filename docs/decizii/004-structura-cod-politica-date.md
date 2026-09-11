# Decizia 4 — Structura devine cod, politica rămâne date

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă

## Regula durabilă

**Structura devine cod, politica rămâne date**: câmpurile per tip = clase +
validare declarativă; maparea Clasă/Tip → conturi, definițiile de stoc,
numerotarea, conexul, scadența, TVA-ul, validările per tip = tabele de
politică editabile fără release. Politica nu inventează comportament
(invariant IV).

---

**Structura devine cod, politica rămâne date:**
   - Devin cod (clase derivate + validare declarativă): definițiile de câmpuri
 per combinație tip/predator/primitor din tabelele de configurare.
   - Rămân date (tabele de politică editabile fără release): maparea contabilă
 Clasă/Tip → conturi, definițiile de stoc (semn × tip document × filtru).
