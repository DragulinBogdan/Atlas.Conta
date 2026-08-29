# Jurnalul de decizii — index (un fișier per decizie)

> **Cum se folosește.** O trimitere „decizia 42c" = deschide `042-*.md` și caută
> sub-punctul `(c)`. NU se citește tot directorul — antetul fiecărui fișier
> spune dacă decizia e activă, amendată sau depășită. Rezumatul durabil al
> fiecărei decizii, sub aceeași numerotare, e în `CLAUDE.md`; constituția e
> `docs/invarianti.md`; istoricul planului de lucru (feliile executate) e în
> [`istoric-plan-de-lucru.md`](istoric-plan-de-lucru.md).
>
> **Decizie nouă** = fișier nou `NNN-slug.md` (numărul următor, antet + text
> integral: context, tranșări, review, ce rămâne deschis) + o linie aici +
> rezumatul în `CLAUDE.md`. **Decizie depășită/amendată** = se actualizează
> `Stare:` în antetul ei (și rezumatul din CLAUDE.md) — textul nu se șterge și
> nu se renumerotează niciodată; sub-punctele (a)–(k) sunt referite din cod.

| # | Decizia | Data | Stare |
|---|---|---|---|
| 1 | [Nucleu generic + moștenire, nu tabele complet separate](001-nucleu-generic-mostenire.md) | 2026-07-22 | activă |
| 2 | [Testul de apartenență a unui câmp](002-testul-apartenentei-campului.md) | 2026-07-22 | activă |
| 3 | [Mapare EF Core](003-mapare-ef-core-tpt.md) | 2026-07-22 | activă |
| 4 | [Structura devine cod, politica rămâne date](004-structura-cod-politica-date.md) | 2026-07-22 | activă |
| 5 | [Backend](005-backend-xaf-frontend-react.md) | 2026-07-22 | activă |
| 6 | [API-ul NU expune ierarhia polimorfic](006-api-fara-ierarhie-polimorfa.md) | 2026-07-22 | activă |
| 7 | [Nav properties în Web API](007-fk-explicite-jsonignore.md) | 2026-07-22 | activă |
| 8 | [Metadata-driven UI](008-metadata-driven-ui.md) | 2026-07-22 | activă |
| 9 | [Scope faza 1](009-scope-faza-1.md) | 2026-07-22 | activă |
| 10 | [Plan de conturi doar sintetic](010-plan-de-conturi-sintetic.md) | 2026-07-22 | activă |
| 11 | [Dimensiuni pe note](011-sursa-finantare-dimensiune-explicita.md) | 2026-07-22 | activă |
| 12 | [Migrare green-field la graniță de ciclu](012-migrare-green-field.md) | 2026-07-22 | activă |
| 13 | [Evaluare stoc](013-evaluare-stoc-identificare-pe-lot.md) | 2026-07-22 | activă |
| 14 | [Registre persistate, append-only](014-registre-append-only.md) | 2026-07-22 | activă |
| 15 | [Dimensiuni](015-dimensiuni-owned-type.md) | 2026-07-22 | activă |
| 16 | [Repartitori](016-repartitori-tpt-si-calitati.md) | 2026-07-22 | activă |
| 17 | [Plăți/încasări = tipuri de document](017-plati-incasari-imperechere-conex.md) | 2026-07-22 | activă |
| 18 | [Migrarea preia nomenclatoare + solduri de deschidere](018-migrarea-preia-nomenclatoare-solduri.md) | 2026-07-22 | activă |
| 19 | [Lista țintă de tipuri de documente](019-lista-tipurilor-de-documente.md) | 2026-07-22 | activă |
| 20 | [Un singur nomenclator de tipuri, și e codul](020-un-singur-nomenclator-de-tipuri.md) | 2026-07-22 | activă |
| 21 | [Seed-ul politicilor se face PE FUNCȚIONALITATE, nu prin transcrierea config-ului legacy](021-seed-pe-functionalitate-nu-transcriere.md) | 2026-07-22 | activă |
| 22 | [Testul bazei](022-testul-bazei-inchis.md) | 2026-07-22 | activă |
| 23 | [Felia 3a](023-felia-3a-persistenta-seed.md) | 2026-07-22 | activă |
| 24 | [Owned `Dimensiuni` sub XAF/EF Core — limitarea e gestionabilă, rămânem pe EF Core](024-owned-dimensiuni-sub-xaf.md) | 2026-07-22 | DEPĂȘITĂ de 54c (owned-ul a murit din Conta) |
| 25 | [Felia 3b](025-felia-3b-motorul-de-operare.md) | 2026-07-22 | activă |
| 26 | [Felia 3c-1 — NIR + FacturaIntrare + mecanismul conex](026-felia-3c1-nir-fct-conex.md) | 2026-07-22 | activă |
| 27 | [Felia 3c-2 — BonConsum](027-felia-3c2-bon-consum.md) | 2026-07-22 | activă |
| 28 | [Felia 3c-3 — ListaDiferenteInventar](028-felia-3c3-lista-diferente-inventar.md) | 2026-07-22 | activă |
| 29 | [Profil contabil](029-profil-contabil-privat-first.md) | 2026-07-22 | activă |
| 30 | [Felia 3c-4 — FacturaIesire](030-felia-3c4-factura-iesire.md) | 2026-07-22 | activă |
| 31 | [Felia 3c-5 — Plata/Incasare + Imperechere](031-felia-3c5-plata-incasare-imperechere.md) | 2026-07-22 | activă |
| 32 | [Felia 3c-6 — Decont](032-felia-3c6-decont.md) | 2026-07-22 | activă |
| 33 | [Felia 3d — validarea transversală](033-felia-3d-validare-transversala.md) | 2026-07-22 | activă |
| 34 | [Pasul 4 — migrarea datelor](034-pasul-4-migrarea-datelor.md) | 2026-07-22 | activă |
| 35 | [Pivot privat-first](035-pivot-privat-first.md) | 2026-07-22 | activă |
| 36 | [Felia P1 — profil privat + TVA structural](036-felia-p1-tva-structural.md) | 2026-07-23 | activă |
| 37 | [Design P2 — descărcarea de gestiune la FacturaIesire](037-design-p2-descarcare-gestiune.md) | 2026-07-23 | activă |
| 38 | [Felia P2 — DescarcareGestiune](038-felia-p2-descarcare-gestiune.md) | 2026-07-23 | activă |
| 39 | [Pre-polish](039-atlas-dxf-constraint-translator.md) | 2026-07-23 | activă |
| 40 | [Felia polish XAF](040-felia-polish-xaf.md) | 2026-07-23 | activă |
| 41 | [Felia „restanțele 40e](041-restantele-40e-atlas-dxf-26-1-3-6.md) | 2026-07-24 | activă |
| 42 | [Design pasul 5 — tierul API + React](042-design-p5-tierul-api.md) | 2026-07-24 | activă |
| 43 | [Design pasul 5 — clientul React](043-design-p5-clientul-react.md) | 2026-07-24 | activă |
| 44 | [Reordonarea roadmap-ului](044-reordonare-roadmap-1c-inainte-de-p5.md) | 2026-07-25 | executată (reordonarea s-a consumat: 1C, GATE XAF, DIM, apoi pasul 5) |
| 45 | [Design FAZA 1C — conector de import + reconciliere pe an fiscal complet](045-design-faza-1c-import-reconciliere.md) | 2026-07-25 | activă |
| 46 | [Felia 1C-a — tipurile noi de model](046-felia-1c-a-tipurile-noi.md) | 2026-07-25 | activă |
| 47 | [Felia 1C-b — scheletul Import1C + nomenclatoare + deschiderea](047-felia-1c-b-import1c-deschiderea.md) | 2026-07-25 | activă |
| 48 | [Analiza pre-1C-c — patru probleme tranșate ÎNAINTE de execuție](048-analiza-pre-1c-c.md) | 2026-07-25 | activă |
| 49 | [Felia 1C-c — documentele 2025 prin motor](049-felia-1c-c-documentele-prin-motor.md) | 2026-07-25 | activă |
| 50 | [Felia 1C-d — gate-ul fazei](050-felia-1c-d-gate.md) | 2026-07-27 | activă |
| 51 | [Analiza post-gate 1C-d](051-analiza-post-gate-1c-d-rotunjire-cmp.md) | 2026-07-27 | activă |
| 52 | [Felia 1C-d-final](052-felia-1c-d-final.md) | 2026-07-28 | activă |
| 53 | [GATE XAF](053-gate-xaf.md) | 2026-07-31 | activă |
| 54 | [Sesiunea de arhitectură 2026-08-02](054-owned-vs-relational-invarianti-dimensiuni-pe-frunze.md) | 2026-08-06 | activă |
| 55 | [Pasul 5, spike 1 — fir complet subțire API+React pe NotaTransfer](055-p5-spike1-btr.md) | 2026-08-08 | activă |
| 56 | [Pasul 5, felia 2 — FacturaIntrare + conex-NIR prin API](056-p5-felia2-fct-nir.md) | 2026-08-08 | activă |
| 57 | [Pasul 5, felia 3 — trezoreria prin API](057-p5-felia3-trezorerie.md) | 2026-08-09 | activă |
| 58 | [Pasul 5, felia 4 — FCL + descărcarea de gestiune prin API](058-p5-felia4-fcl-dsc.md) | 2026-08-09 | activă |
| 59 | [Măsurarea de perf a proiecțiilor pe baza de import](059-p5-perf-masuratori.md) | 2026-08-09 | activă |
| 60 | [Mărunțișurile F3/F4 golite](060-p5-maruntisuri-f3-f4.md) | 2026-08-09 | activă |
| 61 | [Restanțele §Închidere F3/F4 golite](061-p5-restante-f3-f4.md) | 2026-08-09 | activă |
| 62 | [Pasul 5, felia 5 — NIR scriere](062-p5-felia5-nir-scriere.md) | 2026-08-12 | activă |
| 63 | [Pasul 5, felia 6 — LDI + BCS prin API și client](063-p5-felia6-ldi-bcs.md) | 2026-08-13 | activă |
| 64 | [Pasul 5, felia 7 — viramentul intern](064-p5-felia7-virament-581.md) | 2026-08-15 | activă |
| 65 | [Pasul 5, felia 8 — Decont](065-p5-felia8-decont-pereche.md) | 2026-08-16 | activă |
| 66 | [Pasul 5, felia 9 — raportarea pe registre](066-p5-felia9-raportare.md) | 2026-08-17 | activă |
| 67 | [Pasul 5, felia 10 — balanța pliată pe planul de conturi](067-p5-felia10-balanta-pliata.md) | 2026-08-17 | activă |
| 68 | [Pasul 5, felia 11 — jurnalele de TVA](068-p5-felia11-jurnale-tva.md) | 2026-08-19 | activă |
| 69 | [Pasul 5, felia 12 — D300, decontul de TVA](069-p5-felia12-d300.md) | 2026-08-24 | activă |
| 70 | [Pasul 5, felia 13 — motor/structură post-D300 (69-r4/r7/r5 + 67e)](070-p5-felia13-motor-structura.md) | 2026-08-25 | activă |
| 71 | [Pasul 5, felia 14 — D394, declarația informativă](071-p5-felia14-d394.md) | 2026-08-25 | activă (71b amendată de 72a) |
| 72 | [Pasul 5, felia 15 — partener + ANAF (adresa, `Judet`, sincronizarea ANAF)](072-p5-felia15-partener-anaf.md) | 2026-08-26 | activă |
| 73 | [Pasul 5, felia 16 — SAF-T (D406 L): `Societate`, `UnitateMasura`, `CodNc`, `RolTert`, proiecție + fișier XML + DUK](073-p5-felia16-saft.md) | 2026-08-26 | activă |
| 74 | [Pasul 5, felia 17 — SAF-T S (stocuri): `PoliticaMiscareSaft`, proiecție peste `RegistruStoc`, cusăturile S1–S5, fișier + DUK](074-p5-felia17-saft-s.md) | 2026-08-27 | activă |
| 75 | [Pasul 5, felia 18 — restanțele grele ale lui S: golirea valorică (motor), reclasificarea ca mișcare, perf proiecție](075-p5-felia18-restante-s.md) | 2026-08-29 | activă |
| 76 | [Pasul 5, felia 19 — NTC + ASM + retururi (RLF/RDC): scriere prin API și client; plafonul de stingere cu latură și netat](076-p5-felia19-ntc-asm-retururi.md) | 2026-08-29 | activă (amendează 31d/48b) |
