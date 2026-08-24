# Decizia 20 — Un singur nomenclator de tipuri, și e codul

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §20

---

**Un singur nomenclator de tipuri, și e codul.** Nivelul GEST_TIP_DOCUM
devine ierarhia de clase; nivelul intermediar GEST_DEFA_DOCUM (combinații
tip × predator × primitor) dispare — fiecare clasă derivată își fixează
semantica de direcție, iar datele vii confirmă (un singur combo activ per
tip). Config-ul per-combo din legacy se separă: structură → clasă;
politici (numerotare, document conex țintă, tipuri de stoc pe laturi) →
rânduri seed în tabele de politică, cheiate pe un `TipDocument` seed care
oglindește clasele 1:1 (doar ca ancoră FK + UI).
