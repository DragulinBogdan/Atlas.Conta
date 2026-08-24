# Decizia 21 — Seed-ul politicilor se face PE FUNCȚIONALITATE, nu prin transcrierea config-ului legacy

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §21

---

**Seed-ul politicilor se face PE FUNCȚIONALITATE, nu prin transcrierea
config-ului legacy.** Configurarea legacy a fost făcută fără verificări;
servește ca direcție/evidență, NU canonic. Inventarul documentează ce face
sistemul + zgomotul cunoscut; politicile noi se definesc curat per cerință.
Context: FCT vin parțial/complet dintr-un sistem extern (Tethys) —
FacturaIntrare are nevoie de cale de import; tabelele `*_procent` au fost
hack pentru lipsa sursei de finanțare (defalcare procentuală de la
angajament în jos) — nevoia reală (cofinanțare multi-sursă pe linie) se
rezolvă în modelul nou prin mecanism propriu de defalcare, de proiectat
odată cu dimensiunea SursaFinantare (decizia 11/15).
