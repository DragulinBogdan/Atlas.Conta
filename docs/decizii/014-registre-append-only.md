# Decizia 14 — Registre persistate, append-only

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §14

---

**Registre persistate, append-only.** Documentul are ciclu de viață
`Draft → Operat → (Stornat)`; la operare motorul generează rândurile de
registru (stoc + note) tranzacțional. Corecție directă/anulare permisă
DOAR fără dependenți în perioada deschisă (dependență exactă pe loturi +
verificare sold intermediar ≥ 0 pe loturile afectate); altfel storno.
Perioada închisă = graniță absolută. Îngustare a deciziei 4: schema
registrelor (care există, pe ce dimensiuni) = COD; regulile de alimentare
(tip document × semn × filtru) = DATE.
