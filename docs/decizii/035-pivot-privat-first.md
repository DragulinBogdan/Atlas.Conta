# Decizia 35 — Pivot privat-first

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §35

---

**Pivot privat-first: privatul devine sursa principală de cerințe;
legacy-ul pierde statutul canonic.** Pasul 4 a încasat valoarea ancorei
legacy (reconcilierea a validat modelul pe date reale); direcția se ia de
acum din produsul privat — decizia 29 promovată din „direcție" în mod de
lucru. Tranșări:
(a) **Importul din legacy = proiect separat, viitor**, tratat ca import
din orice sursă; unealta Migrare îngheață ca prim prototip de conector;
profilul bugetar rămâne pachet de seed funcțional (dovada agnosticismului
motorului); modulele bugetare (ALOP/angajamente/execuție) se reiau după ce
aplicația privată e rotundă — structura documentelor se așteaptă să ducă
greul și acolo.
(b) **1C primește statutul deciziei 21** (evidență/direcție, niciodată
canonic): MD-uri + view-uri peste structura generică
(_DocumentXXX/_CatalogXXX) + bază istorică consistentă (ideal un an fiscal
complet, cu închiderile de TVA) = ținta de reconciliere a profilului
privat și primul conector al proiectului de import.
(c) **SAF-T (D406) + e-Factura (UBL) + D394 = checklist orizontal de
completitudine** a modelului (atribute parteneri/conturi/tipuri de TVA),
NU modele de date; librăria e-Factura (API+UBL) și exportul SAF-T din 1C
există ca proiecte izolate ale utilizatorului — se integrează la momentul
lor.
(d) **Tenancy: bază-per-client** (profil contabil per bază) — rezolvă
scalarea produsului și limitarea 25f fără schemă nouă. Disciplina de hot
path rămâne: raportarea trăiește pe registre; nicio interogare polimorfă
pe `Document` în fluxuri calde.
(e) Ordinea fazei private: **P1** = profil privat + TVA structural
(nomenclator TipTva mapat pe codurile SAF-T/D394, defalcare 4426/4427);
**P2** = FacturaIesire completă cu descărcare de gestiune; apoi polish XAF
pe modelul stabilizat; pasul 5 (API+React) neschimbat.
