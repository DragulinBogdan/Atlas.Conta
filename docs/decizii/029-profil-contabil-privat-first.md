# Decizia 29 — Profil contabil

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §29

---

**Profil contabil: privat-first ca direcție de produs, bugetar ca profil
de validare.** Motorul e agnostic la plan (niciun simbol hardcodat —
totul prin RegulaContare/SursaCont/ContImplicit pe FK-uri); ce e specific
planului trăiește în seed. Se introduce conceptul de **profil contabil**
= pachet de seed (plan CSV + Clasă/Tip + politici + validări specifice):
CPLAN devine *profilul bugetar*, primul din două — nu „planul aplicației".
Tranșări: (a) profilul bugetar rămâne vehiculul de lucru până trece
pasul 4 — reconcilierea cu legacy se poate face DOAR pe planul în care
trăiesc soldurile; (b) singura scurgere bugetară din cod — validarea FCT
„angajament SAU cod economic" — se mută din clasa de document spre
politică/profil la 3d (validarea declarativă); (c) profilul privat
(OMFP 1802) se scrie când apare clientul privat: al doilea CSV + propriul
set de politici (diferă de CONȚINUT, nu doar simboluri: plus inventar
3xx=6xx/345=711 la privat vs 3xx=791 la bugetari; mărfuri 371→607, nu
671 — derivările de seed sunt per-profil, mecanismele — Cod Tip = simbol,
tăierea segmentelor — se transferă); (d) dimensiunile bugetare din owned
(CodFunctional/CodEconomic/SursaFinantare) rămân în model — nullable,
inofensive la privat; ALOP/angajamente/execuție = modulele „bugetar
peste" (deciziile 9, 22c), stratificarea există deja.
