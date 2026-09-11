# Decizia 21 — Seed-ul politicilor se face PE FUNCȚIONALITATE, nu prin transcrierea config-ului legacy

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă

## Regula durabilă

**Seed-ul politicilor se face PE FUNCȚIONALITATE, nu prin transcrierea
config-ului legacy** (nici a lui 1C — 35b). Sursele externe sunt
evidență/direcție, niciodată canonic; fiecare gaură de profil = decizie
explicită, nu transcriere. Amânare cu nume: defalcarea multi-sursă pe linie
(cofinanțarea) se proiectează odată cu SursaFinantare (jurnal 29d/34d).

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
