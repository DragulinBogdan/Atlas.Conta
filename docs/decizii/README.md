# Jurnalul de decizii — index (un fișier per decizie)

> **Cum se folosește.** O trimitere „decizia 42c" = deschide `042-*.md` și caută
> sub-punctul `(c)`. NU se citește tot directorul — antetul fiecărui fișier
> spune dacă decizia e activă, amendată sau depășită, iar secțiunea „Regula
> durabilă" de sub antet ține regula (cu sub-punctele (a)–(k)), înaintea
> textului integral. Constituția e `docs/invarianti.md`; restanțele cu nume
> sunt în [`restante.md`](restante.md); istoricul planului de lucru (feliile
> executate) e în [`istoric-plan-de-lucru.md`](istoric-plan-de-lucru.md).
>
> **Decizie nouă** = fișier nou `NNN-slug.md` (numărul următor; antetul cu
> câmpurile `Data`, `Stare`, `Execuție`, `Arii`, `Amendează`, `Închide`,
> `Docs`; „Regula durabilă" cu sub-punctele (a)…; apoi context, tranșări,
> review, ce rămâne deschis, istoric) + un rând per restanță în
> `restante.md`. Indexul de mai jos e GENERAT din antete cu `docskit gen`,
> din rădăcina repo-ului; nu se editează de mână (118e).
> **Decizie amendată** = `Stare: amendată de N` în antetul ei, `Amendează:`
> în antetul lui N și marcajul `[amendat de N]` în sub-punctul atins, cu
> textul păstrat (118c). **Decizie depășită integral** = banner și mutare în
> `arhiva/` (118d). Nimic nu se renumerotează; sub-punctele sunt referite
> din cod.

<!-- gen:index -->
### Active (98)

| # | Decizia | Data | Stare |
|---|---|---|---|
| 1 | [Nucleu generic + moștenire, nu tabele complet separate](001-nucleu-generic-mostenire.md) | 2026-07-22 | activă |
| 2 | [Testul de apartenență a unui câmp](002-testul-apartenentei-campului.md) | 2026-07-22 | activă |
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
| 17 | [Plăți/încasări = tipuri de document](017-plati-incasari-imperechere-conex.md) | 2026-07-22 | activă |
| 18 | [Migrarea preia nomenclatoare + solduri de deschidere](018-migrarea-preia-nomenclatoare-solduri.md) | 2026-07-22 | activă |
| 19 | [Lista țintă de tipuri de documente](019-lista-tipurilor-de-documente.md) | 2026-07-22 | activă |
| 20 | [Un singur nomenclator de tipuri, și e codul](020-un-singur-nomenclator-de-tipuri.md) | 2026-07-22 | activă |
| 21 | [Seed-ul politicilor se face PE FUNCȚIONALITATE, nu prin transcrierea config-ului legacy](021-seed-pe-functionalitate-nu-transcriere.md) | 2026-07-22 | activă |
| 22 | [Testul bazei](022-testul-bazei-inchis.md) | 2026-07-22 | activă |
| 23 | [Felia 3a](023-felia-3a-persistenta-seed.md) | 2026-07-22 | activă |
| 24 | [Owned `Dimensiuni` sub XAF/EF Core — limitarea e gestionabilă, rămânem pe EF Core](024-owned-dimensiuni-sub-xaf.md) | 2026-07-22 | activă |
| 25 | [Felia 3b](025-felia-3b-motorul-de-operare.md) | 2026-07-22 | activă |
| 27 | [Felia 3c-2 — BonConsum](027-felia-3c2-bon-consum.md) | 2026-07-22 | activă |
| 28 | [Felia 3c-3 — ListaDiferenteInventar](028-felia-3c3-lista-diferente-inventar.md) | 2026-07-22 | activă |
| 29 | [Profil contabil](029-profil-contabil-privat-first.md) | 2026-07-22 | activă |
| 30 | [Felia 3c-4 — FacturaIesire](030-felia-3c4-factura-iesire.md) | 2026-07-22 | activă |
| 32 | [Felia 3c-6 — Decont](032-felia-3c6-decont.md) | 2026-07-22 | activă |
| 33 | [Felia 3d — validarea transversală](033-felia-3d-validare-transversala.md) | 2026-07-22 | activă |
| 34 | [Pasul 4 — migrarea datelor](034-pasul-4-migrarea-datelor.md) | 2026-07-22 | activă |
| 35 | [Pivot privat-first](035-pivot-privat-first.md) | 2026-07-22 | activă |
| 37 | [Design P2 — descărcarea de gestiune la FacturaIesire](037-design-p2-descarcare-gestiune.md) | 2026-07-23 | activă |
| 39 | [Pre-polish](039-atlas-dxf-constraint-translator.md) | 2026-07-23 | activă |
| 40 | [Felia polish XAF](040-felia-polish-xaf.md) | 2026-07-23 | activă |
| 41 | [Felia „restanțele 40e](041-restantele-40e-atlas-dxf-26-1-3-6.md) | 2026-07-24 | activă |
| 44 | [Reordonarea roadmap-ului](044-reordonare-roadmap-1c-inainte-de-p5.md) | 2026-07-25 | activă |
| 49 | [Felia 1C-c — documentele 2025 prin motor](049-felia-1c-c-documentele-prin-motor.md) | 2026-07-25 | activă |
| 50 | [Felia 1C-d — gate-ul fazei](050-felia-1c-d-gate.md) | 2026-07-27 | activă |
| 51 | [Analiza post-gate 1C-d](051-analiza-post-gate-1c-d-rotunjire-cmp.md) | 2026-07-27 | activă |
| 52 | [Felia 1C-d-final](052-felia-1c-d-final.md) | 2026-07-28 | activă |
| 53 | [GATE XAF](053-gate-xaf.md) | 2026-07-31 | activă |
| 54 | [Sesiunea de arhitectură 2026-08-02](054-owned-vs-relational-invarianti-dimensiuni-pe-frunze.md) | 2026-08-06 | activă |
| 55 | [Pasul 5, spike 1 — fir complet subțire API+React pe NotaTransfer](055-p5-spike1-btr.md) | 2026-08-08 | activă |
| 57 | [Pasul 5, felia 3 — trezoreria prin API](057-p5-felia3-trezorerie.md) | 2026-08-09 | activă |
| 58 | [Pasul 5, felia 4 — FCL + descărcarea de gestiune prin API](058-p5-felia4-fcl-dsc.md) | 2026-08-09 | activă |
| 59 | [Măsurarea de perf a proiecțiilor pe baza de import](059-p5-perf-masuratori.md) | 2026-08-09 | activă |
| 60 | [Mărunțișurile F3/F4 golite](060-p5-maruntisuri-f3-f4.md) | 2026-08-09 | activă |
| 61 | [Restanțele §Închidere F3/F4 golite](061-p5-restante-f3-f4.md) | 2026-08-09 | activă |
| 62 | [Pasul 5, felia 5 — NIR scriere](062-p5-felia5-nir-scriere.md) | 2026-08-12 | activă |
| 63 | [Pasul 5, felia 6 — LDI + BCS prin API și client](063-p5-felia6-ldi-bcs.md) | 2026-08-13 | activă |
| 65 | [Pasul 5, felia 8 — Decont](065-p5-felia8-decont-pereche.md) | 2026-08-16 | activă |
| 66 | [Pasul 5, felia 9 — raportarea pe registre](066-p5-felia9-raportare.md) | 2026-08-17 | activă |
| 67 | [Pasul 5, felia 10 — balanța pliată pe planul de conturi](067-p5-felia10-balanta-pliata.md) | 2026-08-17 | activă |
| 68 | [Pasul 5, felia 11 — jurnalele de TVA](068-p5-felia11-jurnale-tva.md) | 2026-08-19 | activă |
| 69 | [Pasul 5, felia 12 — D300, decontul de TVA ca proiecție peste `RegistruTva`](069-p5-felia12-d300.md) | 2026-08-24 | activă |
| 70 | [Pasul 5, felia 13 — motor/structură post-D300 (69-r4/r7/r5 + 67e)](070-p5-felia13-motor-structura.md) | 2026-08-25 | activă |
| 72 | [Pasul 5, felia 15 — identitatea extinsă a partenerului + sincronizarea ANAF](072-p5-felia15-partener-anaf.md) | 2026-08-26 | activă |
| 75 | [Pasul 5, felia 18 — restanțele grele ale lui S: golirea valorică (motor), reclasificarea ca mișcare, perf proiecție](075-p5-felia18-restante-s.md) | 2026-08-29 | activă |
| 76 | [Pasul 5, felia 19 — NTC + ASM + retururi (RLF/RDC) prin API și client](076-p5-felia19-ntc-asm-retururi.md) | 2026-08-29 | activă |
| 77 | [Pasul 5, felia 20 — finisajul clientului: căutarea fără diacritice, cache-ul de nomenclator, confirmările, Neincluse a…](077-p5-felia20-finisaj-client.md) | 2026-08-29 | activă |
| 78 | [Căutarea fără diacritice pe proiecții — rescrierea predicatelor `DataSourceLoader` prin normalizatorul comun](078-cautare-fara-diacritice-proiectii.md) | 2026-09-01 | activă |
| 80 | [Refuzurile de acces pe toate ușile: 404 / 403 / 422 — o singură ordine, un singur corp](080-refuzuri-acces-toate-usile.md) | 2026-09-02 | activă |
| 81 | [Pasul 5, felia 23 — implicitele de culegere și întreținerea politicilor: regimul e al partenerului, cota e a produsului…](081-p5-felia23-implicite-politici.md) | 2026-09-02 | activă |
| 82 | [Stingerea automată prin contract de document](082-stingere-automata-prin-contract.md) | 2026-09-08 | activă |
| 84 | [Pasul 5, felia 24 — politicile: seed-ul aliniază, rolul Configurator, cele șapte ecrane, potrivirea ca funcții pure și…](084-p5-felia24-politici-explica.md) | 2026-09-11 | activă |
| 85 | [Modul de acces al ListView-urilor XAF Blazor (EF Core, PostgreSQL): Server implicit cu paginare, ServerView opt-in p…](085-mod-acces-listview-xaf.md) | 2026-09-11 | activă |
| 86 | [Pasul 5, felia 25 — DVI (declarația vamală de import) ca tip de document: TVA-ul în vamă intră în evidență, D300 rd. 24/…](086-p5-felia25-dvi.md) | 2026-09-13 | activă |
| 89 | [Pasul 5, felia 28 — TPH cu discriminatorul mapat ClrType pe Document, DocumentDetaliu și Repartitor…](089-tph-discriminator-tipul-ca-data.md) | 2026-09-18 | activă |
| 93 | [Folosința păstrează gestiunea reală și lotul](093-folosinta-gestiune-reala.md) | 2026-09-24 | activă |
| 94 | [Deschiderea detaliază soldul contabil, fără dublare](094-deschidere-prin-detalierea-soldului.md) | 2026-09-24 | activă |
| 95 | [DEC și imobilizările complete pe cub înaintea TR-D8](095-producatori-completi-inaintea-citirilor.md) | 2026-09-24 | activă |
| 98 | [Conexul editat postează delta; poziția fără fișă nu devine negativă; contul fișei vine din cub](098-corecturi-review-sincronizare.md) | 2026-09-24 | activă |
| 99 | [Cauza diferenței dintre evidență și constatarea fizică](099-cauza-diferentei-evidenta-constatare.md) | 2026-09-24 | activă |
| 102 | [Fără compatibilitate cu bazele de dezvoltare până la prima producție](102-fara-compatibilitate-cu-bazele-de-dezvoltare.md) | 2026-09-25 | activă |
| 103 | [Fapte fiscale istorice și perioade distincte de declarare](103-fapte-fiscale-istorice-si-perioade-distincte.md) | 2026-09-25 | activă |
| 105 | [SAF-T D406 pe cub: sursă unică, acces complet, certificare cu manifest](105-saft-pe-cub.md) | 2026-09-30 | activă |
| 106 | [Regimul pe stare: o singură sursă a editabilității și a comenzilor disponibile](106-regim-pe-stare.md) | 2026-09-30 | activă |
| 107 | [Migrarea deschide terții per partidă din BalantaNivel3, prin Materializare.Deschide…](107-deschiderea-de-terti-din-balanta-nivel3.md) | 2026-10-01 | activă |
| 109 | [Taxa nemarcată se decide pe document × cotă și se scrie pe linie; declarația postează taxa liniei…](109-taxa-nemarcata-pe-document-scrisa-pe-linie.md) | 2026-10-04 | activă |
| 111 | [Un singur repartitor de sold pe postare, partenerul fiscal în blocul fiscal, registre tipate pentru ce nu se balansează](111-repartitor-unic-si-registre-tipate.md) | 2026-10-06 | activă |
| 112 | [Recepția facturii se contează pe regula ei de contare (`FCT/Stoc`)](112-receptia-facturii-pe-regula-ei.md) | 2026-10-08 | activă |
| 113 | [Semnul valorii e al liniei; `RegulaContare.PastreazaSemn` a ieșit](113-semnul-e-al-liniei-pastreazasemn-scos.md) | 2026-10-08 | activă |
| 115 | [Linia de notă poate numi partida pe care o stinge; partida inițială își poartă data de naștere](115-nota-numeste-partida.md) | 2026-10-09 | activă |
| 117 | [Postarea își poartă ordinalul în tranzacție; unitatea fără document se departajează pe el](117-ordinalul-postarii-in-tranzactie.md) | 2026-10-09 | activă |
| 119 | [Compensarea valorică aparține corecției: a doua Operare pe un document operat…](119-compensarea-valorica-apartine-corectiei.md) | 2026-10-13 | activă |
| 120 | [Lanțul de migrații e de unică folosință până la „rotund”; recomprimarea lui este a șaptea condiție](120-lantul-de-migratii-se-recomprima-la-rotund.md) | 2026-10-13 | activă |
| 121 | [Ariile stării curente sunt 16, pe responsabilitățile catalogului de funcționalități](121-ariile-pe-familiile-catalogului.md) | 2026-10-15 | activă |
| 123 | [D300 și D394 se verifică încrucișat pe aceleași fapte: maparea D394 e completă și coerentă cu decontul, iar diferențele…](123-d300-d394-verificare-incrucisata.md) | 2026-10-16 | activă |
| 124 | [Rezultatul se închide lunar printr-un document generat, exercițiul închis e starea lui decembrie, iar balanța are graniț…](124-exercitiul-se-inchide-pe-decembrie.md) | 2026-10-16 | activă |
| 125 | [Citirea de sold pe o listă de loturi se face pe lot, nu pe fereastra de dată](125-citirea-pe-loturi-se-face-pe-lot.md) | 2026-10-16 | activă |
| 126 | [Drumul spre „rotund”: ce rămâne din 091 (g), restanțele închise sau retrase din lista activă și semantica cititorilor de…](126-drumul-spre-rotund.md) | 2026-10-17 | activă |
| 127 | [Validarea judecă lotul fixat, anularea PIF judecă AMO pe lună, produsul cu stoc nu-și schimbă clasa, lotul nu se naște n…](127-validarea-judeca-lotul-fixat.md) | 2026-10-17 | activă |
| 128 | [Golirea lotului prin retur la furnizor trece reziduul pe politica de diferențe](128-golirea-prin-retur-trece-reziduul-pe-politica.md) | 2026-10-17 | activă |
| 129 | [Minusul de inventar își culege cauza: cheltuiala rămâne pe tip, imputarea merge pe politica de diferențe](129-minusul-de-inventar-isi-culege-cauza.md) | 2026-10-17 | activă |
| 130 | [„Rotund” e declarat: PoC-ul se oprește, migrarea devine etapa următoare, iar restanțele se rejudecă pe modelul rotund](130-rotund-declarat.md) | 2026-10-17 | activă |
| 131 | [Cubul adună; faptul datat e supapă cu nume, construită la primul consumator, nu registru de acumulare și nu postare de z…](131-cubul-aduna-faptul-datat-e-supapa.md) | 2026-10-17 | activă |
| 132 | [Migrarea din 1C: conectorul numește ce numește sursa, iar diferența unei partide se atribuie pe document](132-migrarea-numeste-ce-numeste-sursa.md) | 2026-10-17 | propusă |
| 133 | [Migrarea imobilizărilor: fișa se naște din soldul 1C la graniță, amortizarea o face motorul, ieșirea se face pe fișă](133-migrarea-imobilizarilor-fisa-din-sold.md) | 2026-10-17 | propusă |

### Amendate parțial (35)

| # | Decizia | Data | Stare |
|---|---|---|---|
| 3 | [Mapare EF Core](003-mapare-ef-core-tpt.md) | 2026-07-22 | amendată de 89 |
| 16 | [Repartitori](016-repartitori-tpt-si-calitati.md) | 2026-07-22 | amendată de 89 |
| 26 | [Felia 3c-1 — NIR + FacturaIntrare + mecanismul conex](026-felia-3c1-nir-fct-conex.md) | 2026-07-22 | amendată de 112 |
| 31 | [Felia 3c-5 — Plata/Incasare + Imperechere](031-felia-3c5-plata-incasare-imperechere.md) | 2026-07-22 | amendată de 76, 88 |
| 36 | [Felia P1 — profil privat + TVA structural](036-felia-p1-tva-structural.md) | 2026-07-23 | amendată de 71 |
| 38 | [Felia P2 — DescarcareGestiune](038-felia-p2-descarcare-gestiune.md) | 2026-07-23 | amendată de 81, 84 |
| 42 | [Design pasul 5 — tierul API + React](042-design-p5-tierul-api.md) | 2026-07-24 | amendată de 104, 106 |
| 43 | [Design pasul 5 — clientul React](043-design-p5-clientul-react.md) | 2026-07-24 | amendată de 104 |
| 45 | [Design FAZA 1C — conector de import + reconciliere pe an fiscal complet](045-design-faza-1c-import-reconciliere.md) | 2026-07-25 | amendată de 75 |
| 46 | [Felia 1C-a — tipurile noi de model](046-felia-1c-a-tipurile-noi.md) | 2026-07-25 | amendată de 113 |
| 47 | [Felia 1C-b — scheletul Import1C + nomenclatoare + deschiderea](047-felia-1c-b-import1c-deschiderea.md) | 2026-07-25 | amendată de 107 |
| 48 | [Analiza pre-1C-c — patru probleme tranșate ÎNAINTE de execuție](048-analiza-pre-1c-c.md) | 2026-07-25 | amendată de 76 |
| 56 | [Pasul 5, felia 2 — FacturaIntrare + conex-NIR prin API](056-p5-felia2-fct-nir.md) | 2026-08-08 | amendată de 77, 81 |
| 64 | [Pasul 5, felia 7 — viramentul intern](064-p5-felia7-virament-581.md) | 2026-08-15 | amendată de 84 |
| 71 | [Pasul 5, felia 14 — D394, declarația informativă ca proiecție peste `RegistruTva`](071-p5-felia14-d394.md) | 2026-08-25 | amendată de 72 |
| 73 | [Pasul 5, felia 16 — SAF-T (D406 L) ca proiecție peste registre](073-p5-felia16-saft.md) | 2026-08-26 | amendată de 74, 105 |
| 74 | [Pasul 5, felia 17 — SAF-T D406 **S (stocuri)** peste `RegistruStoc`](074-p5-felia17-saft-s.md) | 2026-08-27 | amendată de 105 |
| 79 | [Pasul 5, felia 21 — ITV (închiderea de TVA) prin API și client: comandă cu cauză + ecran de rezultat](079-p5-felia21-itv.md) | 2026-09-01 | amendată de 80 |
| 83 | [Seed-ul și proveniența: re-seed-ul corectează doar DinSeed, golurile privat (NIM / IMP cu poarta spre DVI), rolul Con…](083-seed-provenienta-goluri-configurator.md) | 2026-09-10 | amendată de 84, 86, 104 |
| 87 | [Pasul 5, felia 26 — Imobilizări și amortizare, contabil și fiscal: fișa ca nomenclator subțire, un singur registru appen…](087-p5-felia26-imobilizari.md) | 2026-09-15 | amendată de 97 |
| 88 | [Pasul 5, felia 27 — Perioada fiscală ca lanț, închiderea ca comandă cu acceptare conștientă, soldurile și partidele desc…](088-p5-felia27-perioade-solduri.md) | 2026-09-17 | amendată de 103 |
| 90 | [Nucleul ca ledger n-dimensional — un singur cub de postări în locul celor patru registre…](090-nucleu-cub-de-postari.md) | 2026-09-20 | amendată de 91, 92, 93, 95, 96, 110, 119 |
| 91 | [Proba supremă devine catalogul de scenarii — Import1C se îngheață ca unealtă de migrare, nu ca gate…](091-scenarii-in-loc-de-import.md) | 2026-09-22 | amendată de 107, 118, 120 |
| 92 | [Identitatea partidei include partenerul](092-identitatea-partidei-include-partenerul.md) | 2026-09-23 | amendată de 102 |
| 96 | [Urmărirea partidelor este separată de rolul comercial](096-urmarirea-partidelor-separata-de-rolul-comercial.md) | 2026-09-24 | amendată de 100, 102 |
| 97 | [PIF nominalizează valoarea contabilă cu suport obligatoriu](097-pif-nominalizare-cu-suport-obligatoriu.md) | 2026-09-24 | amendată de 98 |
| 100 | [Partide comerciale în politica bugetară](100-partide-comerciale-bugetare.md) | 2026-09-25 | amendată de 102 |
| 101 | [Împerecherea cere efect verificabil pe partidă](101-imperechere-cu-efect-pe-partida.md) | 2026-09-25 | amendată de 102 |
| 104 | [Straturi izolate, culegerea în XAF, entități proprii fără ștergere amânată](104-straturi-izolate-entitati-proprii.md) | 2026-09-26 | amendată de 106 |
| 108 | [Gate-ul transversal TR-D8: registrele nu se mai citesc, scrierea e serială per bază, explicația deciziei e persistată](108-gate-transversal-tr-d8.md) | 2026-10-04 | amendată de 110 |
| 110 | [Tăierea: cubul e singurul scriitor și singura sursă; registrele, regimul dual și oracolul lor au ieșit (TR-D9a)](110-taierea-cubul-singurul-scriitor.md) | 2026-10-07 | amendată de 113 |
| 114 | [Departajarea FIFO: data, documentul deschizător, apoi identificatorul](114-departajarea-fifo-pe-documentul-deschizator.md) | 2026-10-09 | amendată de 116 |
| 116 | [Unitatea fără document se departajează pe postarea ei de deschidere; ID-urile postărilor urmează ordinea tranzacției](116-postarea-de-deschidere-departajeaza-unitatea-fara-document.md) | 2026-10-09 | amendată de 117 |
| 118 | [Guvernanța documentației: kitul unificat dă forma, Atlas.Conta îl aplică pe structura lui, cu gardieni care intră pe rân…](118-guvernanta-documentatiei.md) | 2026-10-10 | amendată de 121 |
| 122 | [Declarațiile D300 și D394 se serializează din proiecția lor, în lei, cu refuz înaintea fișierului…](122-declaratiile-se-serializeaza-din-proiectie.md) | 2026-10-15 | amendată de 123 |

### Depășite integral (0)

| # | Decizia | Data | Stare |
|---|---|---|---|

<!-- /gen:index -->

