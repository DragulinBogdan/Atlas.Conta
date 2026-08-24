# Decizia 44 — Reordonarea roadmap-ului

- **Data**: 2026-07-25 (primul commit în jurnal)
- **Stare**: executată (reordonarea s-a consumat: 1C, GATE XAF, DIM, apoi pasul 5); rămâne regula „XAF Blazor = vehicul de iterație, luptele structurale → lista React"
- **Rezumat durabil**: `CLAUDE.md` §44

---

**Reordonarea roadmap-ului: pasul 5 se AMÂNĂ după călirea modelului pe
date reale.** Pasul 5 (API+React, deciziile 42/43) îngheață contracte
(DTO-uri, tipuri generate), iar orice gaură de model descoperită
post-îngheț costă dublu; XAF Blazor rămâne vehiculul de iterație
(schimbare de model = migrație + baseline, nu cascadă prin codegen).
Ordinea nouă:
(1) **FAZA 1C**: profilul privat se finalizează PRIN conector 1C +
reconciliere pe un an fiscal complet, INCLUSIV închiderea lunară de TVA
(4423/4424 — iese din amânarea 36f; e forcing function-ul care validează
TVA-ul structural din P1). Profilul nu se finalizează în abstract:
politicile se definesc curat pe măsură ce importul scoate găurile
(filosofia 21/35b — 1C e evidență, niciodată canonic).
(2) **GATE XAF**: polish pe DOUĂ ecrane, FacturaIntrare + FacturaIesire
(trafic maxim, exersează tot: lookup-uri grele, linii, TVA, conex-NIR,
descărcare). Regulă de oprire SCRISĂ în contractul feliei: „un contabil
tolerant le operează zilnic", explicit NU product-grade; orice luptă
structurală cu Blazor (async, dialoguri, feedback) NU se hack-uiește —
se notează pe lista React și moare acolo. Gate trecut = „avem pe ce
construi".
(3) Abia apoi pasul 5, pe model călit, cu designurile 42/43 neatinse
(ancorate pe cusăturile stabile: registre, motor, agregat).
Conectorul 1C NU are nevoie de tierul API: precedentul e unealta Migrare
(consolă, ObjectSpace non-secured, motor direct, idempotență prin
MigrareLegatura). „Importul ca API" (42d) rămâne pentru fluxurile vii.
