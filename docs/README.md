# Documentația — lista fișierelor

Harta întrebare → unde stă în [CLAUDE.md](../CLAUDE.md), „Harta surselor de adevăr”.
Stratul curent: invarianții, `stare-curenta/`, deciziile din afara `arhiva/`, registrul
restanțelor, contractele deschise, cataloagele vii, `stare.md`. Restul e jurnal.
Lista de mai jos e generată de `docskit gen` din titlul și prima propoziție a fiecărui fișier.

<!-- gen:fisiere -->
### `docs/` (5)

- [Documentația — lista fișierelor](README.md) — Harta întrebare → unde stă în CLAUDE.md, „Harta surselor de adevăr”.
- [Sesiune de arhitectură 28.07.2026 — fluxul comenzilor online (rezervare, picking, PO furnizori)](architecture-notes-2026-07-28.md) — Magazin online cu stocul sursă în gestiuni identificate „Online" din Atlas.Conta.
- [GATE XAF — contractul feliei (decizia 44.2)](gate-xaf-contract.md) — Regula de oprire (cu dinți): orice luptă STRUCTURALĂ cu Blazor (async, dialoguri, refresh de UI, feedback) NU se hack-uiește — se notează în docs/api/lista-react.md și moare a…
- [Invarianții Atlas.Conta](invarianti.md) — Document viu, agreat 2026-08-02 (sesiune de arhitectură).
- [Unde suntem](stare.md) — Generat de docskit gen din antete, registru și cod; nu se editează.

### `docs/api/` (38)

- [D300 — Decont de TVA, structura oficială 2026 (OPANAF 174/2026)](api/d300-structura-2026.md) — Formularul are 45 de rânduri numerotate (1–45), plus sub-rândurile 3.1, 5.1, 7.1, 12.1, 12.2, 20.1, 22.1, 26.1, 26.2, 29.1 — deci 55 de poziţii numerotate în corpul decontului, …
- [D394 — Declarația informativă 394, structura oficială 2026 (OPANAF 3769/2015, modificat prin OPANAF 2194/2025)](api/d394-structura-2026.md) — Formularul 394 a fost adaptat prin adăugarea cotelor 21% și 11% la fiecare defalcare pe cote, păstrând toate cotele istorice (24%, 20%, 19%, 9%, 5%).
- [D406 (SAF-T) — structura oficială, ancora 16.02.2026](api/d406-structura-2026.md) — Toate schimbările sunt de nomenclator, nu de structură (xlsx, Istoric document r.110–133):
- [Lista React — luptele structurale cu XAF Blazor](api/lista-react.md) — Artefactul cerut de decizia 44.2: orice luptă STRUCTURALĂ cu Blazor (async, dialoguri, refresh, feedback) NU se hack-uiește în XAF — se notează aici și se rezolvă în clientul Re…
- [Pasul 5 — API + React: designul tierului Web API (design)](api/p5-api-design.md) — Motorul e UI-agnostic: clase statice pe IObjectSpace + entități (MotorOperare.Opereaza — Motor/MotorOperare.cs:21; DescarcareService, ImperechereService, StocService).
- [Pasul 5, felia 10 — balanța pliată pe planul de conturi (rollup)](api/p5-felia-balanta-plan-contract.md) — Felia 9 a livrat balanța, fișa și jurnalul, și a lăsat un singur lucru deschis cu nume: rollup-ul pe plan (R-D5).
- [Pasul 5, felia 12 — D300 (decontul de TVA) ca proiecție peste RegistruTva — DESIGN](api/p5-felia-d300-design.md) — Felia 11 (decizia 68) a lăsat registrul fiscal gata și un schelet: proiecția DecontTva, grupată pe (Sens × TipTva), cu comentariul din App.tsx „decontul e scheletul D300, …
- [Pasul 5, felia 14 — D394 (declarația informativă) ca proiecție peste RegistruTva — DESIGN](api/p5-felia-d394-design.md) — Felia 12 (decizia 69) a așezat cifrele registrului fiscal pe rândurile D300.
- [Pasul 5, felia 8 — Decont (DEC) + legătura explicită de pereche (contract)](api/p5-felia-dec-pereche-contract.md) — Data fixării: 2026-08-15.
- [Pasul 5 — Felia 4: FCL + descărcarea de gestiune prin API (contract)](api/p5-felia-fcl-contract.md) — Premise verificate pe cod (explorare + spot-check main): FCL e COMPLET wired în XAF (culegere cu TVA, validări, acțiunea „Generează descărcarea") — spre deosebire de FCT la feli…
- [Pasul 5 — Felia 2: FacturaIntrare + conex-NIR prin API (contract)](api/p5-felia-fct-contract.md) — F2-D1.
- [Pasul 5, felia 20 — finisajul clientului (contract)](api/p5-felia-finisaj-client-contract.md) — Data fixării: 2026-08-29.
- [Pasul 5, felia 21 — ITV (închiderea de TVA) prin API și client (contract)](api/p5-felia-itv-contract.md) — Data fixării: 2026-09-01.
- [Izolarea motorului de IObjectSpace — plan independent de ORM, contracte economice de citire (contract)](api/p5-felia-izolare-motor-contract.md) — Amendament 2026-09-18 (felia 28, decizia 89).
- [Pasul 5, felia 11 — jurnalele de TVA — DESIGN](api/p5-felia-jurnale-tva-design.md) — Ultima extensie naturală a raportării, și prima care nu poate fi scrisă peste registrele existente.
- [Pasul 5, felia 6 — LDI + BCS prin API și client (contract)](api/p5-felia-ldi-bcs-contract.md) — Data fixării: 2026-08-12.
- [Pasul 5, felia 13 — motor/structură post-D300 (contract)](api/p5-felia-motor-structura-contract.md) — Data fixării: 2026-08-25.
- [Pasul 5 — Felia 5: NIR scriere (recepția culeasă manual) — contract](api/p5-felia-nir-contract.md) — A cincea felie verticală.
- [Pasul 5, felia 19 — NTC + ASM + retururi (RLF/RDC) prin API și client (contract)](api/p5-felia-ntc-asm-retururi-contract.md) — Data fixării: 2026-08-29.
- [Pasul 5, felia 15 — identitatea extinsă a partenerului + sincronizarea ANAF — CONTRACT](api/p5-felia-partener-anaf-contract.md) — Partener (Repartitori.cs:30-77) are exact CodFiscal, RegistruComert, TipPersoana, Tara (ISO-2, normalizat în setter, gard în GardianEditare.VerificaPartener), Inr…
- [Pasul 5, felia 9 — raportarea pe registre (balanță, fișă de cont, registru-jurnal)](api/p5-felia-raportare-contract.md) — Prima felie a pasului 5 care nu adaugă un tip de document, ci suprafața de citire care lipsea.
- [Pasul 5, felia 22 — refuzurile de acces pe toate ușile: 404 / 403 / 422 (contract)](api/p5-felia-refuzuri-acces-contract.md) — Azi fiecare ușă răspunde altfel la aceeași întrebare („n-ai voie"):
- [Pasul 5, felia 18 — restanțele grele ale lui S: 74-r4 (perf O(istoric)), 74-r6 (deriva per lot), 74-r9 (reclasificarea la transfer) — CONTRACT](api/p5-felia-restante-s-contract.md) — 74-r4.
- [Pasul 5, felia 16 — SAF-T (D406 L) ca proiecție peste registre — CONTRACT](api/p5-felia-saft-contract.md) — Referința structurii oficiale: docs/api/d406-structura-2026.md (schema 4.1.16 / istoric 133 din 16.02.2026; namespace producție mfp:anaf:dgti:d406:declaratie:v1; kit DUK loc…
- [Pasul 5, felia 17 — SAF-T D406 S (stocuri) peste RegistruStoc — CONTRACT](api/p5-felia-saft-s-contract.md) — Sursele: XSD v2.4.7/2.4.8/2.4.9 (D:\Dev\Work\Anaf.XML\SafT\, secțiunile S identice între versiuni), anaf/RO_SAFT_SchemaDefCod_16.02.2026.xlsx (foile Nomenclator stocuri, …
- [Pasul 5 — Felia 3: trezoreria prin API (PLT/INC + imperecheri + plata automată) (contract)](api/p5-felia-trz-contract.md) — F3-D1.
- [Pasul 5, felia 7 — viramentul intern (transferul 581) (contract)](api/p5-felia-vir-contract.md) — Data fixării: 2026-08-13.
- [Pasul 5, felia 23 — implicitele de culegere și întreținerea politicilor (contract)](api/p5-felia23-implicite-politici-contract.md) — Două lucruri care azi lipsesc, legate prin aceeași ușă:
- [Pasul 5, felia 24 — politicile: execuția deciziei 83, ecranele rămase și „Explică" (contract)](api/p5-felia24-politici-explica-contract.md) — Două track-uri cu regulă de oprire proprie:
- [Pasul 5, felia 25 — DVI (declarația vamală de import) ca tip de document (contract)](api/p5-felia25-dvi-contract.md) — Data fixării: 2026-09-12.
- [Pasul 5, felia 26 — Imobilizări și amortizare, contabil și fiscal (contract)](api/p5-felia26-imobilizari-contract.md) — Data fixării: 2026-09-14 (după două runde de întrebări cu owner-ul, 12 + 9, toate tranșate).
- [Felia 27, pasul 0 — spike: cursa perioadei (F27-D1) și costul soldurilor (F27-D3)](api/p5-felia27-pas0-spike.md) — Codul experimentelor stă în scratchpad-ul sesiunii (scratchpad/spike/ — SQL + masoara.sh + lock-proba.sh; scratchpad/spike/ef/ — program C# cu ProjectReference spre Mo…
- [Felia 27 — perioada ca lanț, închiderea ca comandă, soldurile materializate la închidere, data înregistrării (contract)](api/p5-felia27-perioade-solduri-contract.md) — Registrele sunt rulaje pure și orice sold se calculează la fiecare citire, prin sumă peste TOT istoricul cheii.
- [Felia 28 — pasul 0: spike-ul de mapare TPH (notă)](api/p5-felia28-pas0-spike.md) — Stare: executat, închis.
- [Felia 28 — TPH cu discriminator pe cele trei ierarhii; tipul ca dată (contract)](api/p5-felia28-tph-contract.md) — Trei ierarhii sunt mapate azi TPT (decizia 3, 16): Document (20 de derivate, două niveluri), DocumentDetaliu (12) și Repartitor (5).
- [Pasul 5 — măsurarea de perf a proiecțiilor pe baza de import](api/p5-perf-masuratori.md) — Executată 2026-08-09.
- [Pasul 5 — API + React: designul clientului React (design)](api/p5-react-design.md) — Diagnosticul-ancoră al sesiunii (motivația migrării de pe XAF Blazor, re-formulată structural): XAF e un interpretor — framework-ul randează, customizarea trece prin override-ur…
- [Pasul 5 — Spike 1: fir complet subțire API+React pe NotaTransfer (contract)](api/p5-spike1-contract.md) — D1.

### `docs/consultari/` (1)

- [(fără titlu)](consultari/codex-2026-09-22-cub.md) — in folderul docs/decizii am istoricul de decizii pentru proiectul curent greenfield.

### `docs/consultations/2026-10-05-cub-vs-registre/` (5)

- [Runda 1 (oarbă): forma registrului unui sistem de contabilitate și gestiune](consultations/2026-10-05-cub-vs-registre/1-runda-oarba.md) — Ești consultat ca arhitect independent.
- [Runda 2 (informată): verificarea pe cod](consultations/2026-10-05-cub-vs-registre/2-runda-informata.md) — Primești trei lucruri: răspunsul tău din runda 1, dosarul de fapte (docs/dosar.md) și acces de citire la o clonă a repo-ului, la commit-ul a5df5fe.
- [Runda 3: critica încrucișată](consultations/2026-10-05-cub-vs-registre/3-critica-incrucisata.md) — Doi analiști independenți au răspuns aceleiași întrebări despre acest repo, la commit-ul a5df5fe: înaintea ștergerii registrelor (contractul TR-D9a, docs/nucleu/tr-d9-taierea…
- [Consultare: cubul, registrele sau amândouă (înaintea TR-D9a)](consultations/2026-10-05-cub-vs-registre/README.md) — Întrebarea owner-ului: înainte de ștergerea registrelor, ce câștigăm și ce pierdem cu cubul; rămâne cubul singur, cubul ca autoritate cu registre derivate sau registrele.
- [Dosarul de fapte (pentru runda 2)](consultations/2026-10-05-cub-vs-registre/dosar.md) — Textele deciziilor și CLAUDE.md sunt scrise de autorii direcției actuale.

### `docs/consultations/2026-10-05-cub-vs-registre/raspunsuri/` (6)

- [(fără titlu)](consultations/2026-10-05-cub-vs-registre/raspunsuri/r1-astra.md) — [presupunere] Aleg C pentru domeniul descris, condiționat de validări comune și proveniență completă.
- [Runda 1: forma registrului — judecată din principii](consultations/2026-10-05-cub-vs-registre/raspunsuri/r1-fable.md) — 1.
- [(fără titlu)](consultations/2026-10-05-cub-vs-registre/raspunsuri/r2-astra.md) — [judecată] Îmi schimb recomandarea din C în A, cu TR-D9a amendat.
- [Runda 2: verificarea pe cod (a5df5fe)](consultations/2026-10-05-cub-vs-registre/raspunsuri/r2-fable.md) — Prescurtări: M/ = nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/, N/ = nou/Atlas.Conta.Nucleu/Atlas.Conta.Nucleu/, dn/ = docs/nucleu/, perf = docs/api/…
- [(fără titlu)](consultations/2026-10-05-cub-vs-registre/raspunsuri/r3-astra.md) — [judecată] Mențin A, cu TR-D9a amendat.
- [Runda 3: critica încrucișată (analistul Y)](consultations/2026-10-05-cub-vs-registre/raspunsuri/r3-fable.md) — Rămân la A, cu TR-D9a amendat și fără prototip B. Numai citire, nimic rulat.

### `docs/consultations/2026-10-05-nod-si-galeata/` (4)

- [Runda 1 (oarbă): transformarea de stoc și poziția fără unitate](consultations/2026-10-05-nod-si-galeata/1-runda-oarba.md) — Ești consultat ca analist independent.
- [Runda 2 (informată): cele două amendamente, pe cod](consultations/2026-10-05-nod-si-galeata/2-runda-informata.md) — Primești trei lucruri: răspunsul tău din runda 1 (docs/consultare/runda1-proprie.md), fișa de fapte (docs/consultare/fapte.md) și acces de citire la o clonă a repo-ului, la …
- [Consultare: nodul de transformare și poziția fără unitate](consultations/2026-10-05-nod-si-galeata/README.md) — Rezultat pe scurt: forma Y scrisă ca mișcare obișnuită e respinsă de ambii analiști (rulaj dublu, valoare permanentă în nod, nucleul o refuză azi în Conservare.VerificaTransfor…
- [Fapte pentru runda 2](consultations/2026-10-05-nod-si-galeata/fapte.md) — Scris de sesiunea care a propus forma Y și regula Q. E părtinitor prin construcție.

### `docs/consultations/2026-10-05-nod-si-galeata/raspunsuri/` (4)

- [(fără titlu)](consultations/2026-10-05-nod-si-galeata/raspunsuri/r1-astra.md) — Verdictul 1: aleg X pentru transformările care sunt redistribuiri de stoc, deoarece conservă valoarea fără solduri valorice artificiale pe produse; resping însă acceptarea autom…
- [Consultare runda 1: transformarea de stoc și poziția fără unitate](consultations/2026-10-05-nod-si-galeata/raspunsuri/r1-fable.md) — Întrebarea 1: Y, cu două corecturi.
- [(fără titlu)](consultations/2026-10-05-nod-si-galeata/raspunsuri/r2-astra.md) — Resping amendamentul 1 în forma propusă.
- [Runda 2: raport](consultations/2026-10-05-nod-si-galeata/raspunsuri/r2-fable.md) — Am lucrat numai prin citire, la a5df5fe.

### `docs/consultations/2026-10-06-repartitor-pe-postare/` (4)

- [Runda 1 (oarbă): cum poartă postarea repartitorul](consultations/2026-10-06-repartitor-pe-postare/1-runda-oarba.md) — Ești consultat ca analist independent.
- [Runda 2 (informată): repartitorul pe postare, pe cod](consultations/2026-10-06-repartitor-pe-postare/2-runda-informata.md) — Primești trei lucruri: răspunsul tău din runda 1 (docs/consultare/runda1-proprie.md), fișa de fapte (docs/consultare/fapte.md) și acces de citire la o clonă a repo-ului, la …
- [Consultare: cum poartă postarea repartitorul](consultations/2026-10-06-repartitor-pe-postare/README.md) — Întrebarea owner-ului: cubul poartă azi două coloane tipate, Partener și Gestiune, completate selectiv.
- [Fișa de fapte: repartitorul pe postare](consultations/2026-10-06-repartitor-pe-postare/fapte.md) — Scrisă de sesiunea care orchestrează consultarea, la commit-ul 717c3b5.

### `docs/consultations/2026-10-06-repartitor-pe-postare/raspunsuri/` (4)

- [(fără titlu)](consultations/2026-10-06-repartitor-pe-postare/raspunsuri/r1-astra.md) — Verdict: forma 3 pentru nucleul contabil; forma 5 dacă legătura dintre picioare este necesară operațional.
- [Runda 1 (oarbă): verdict](consultations/2026-10-06-repartitor-pe-postare/raspunsuri/r1-fable.md) — Analiză doar din regulă contabilă și din coerența modelului, fără cod și fără date.
- [(fără titlu)](consultations/2026-10-06-repartitor-pe-postare/raspunsuri/r2-astra.md) — Recomand forma 5, cu separarea explicită a repartitorului de sold, a partenerului fiscal și a rolului cantitativ.
- [(fără titlu)](consultations/2026-10-06-repartitor-pe-postare/raspunsuri/r2-fable.md) — Recomand forma 5 în sensul strict: forma 3 plus cheia de pereche deja aprobată (D9-A2), cu două coloane în plus față de cum au fost enunțate: PartenerFiscal în blocul fiscal ș…

### `docs/decizii/` (121)

- [1 — Nucleu generic + moștenire, nu tabele complet separate](decizii/001-nucleu-generic-mostenire.md) — Nucleu generic + moștenire, nu tabele separate.
- [2 — Testul de apartenență a unui câmp](decizii/002-testul-apartenentei-campului.md) — Testul de apartenență a unui câmp: apare într-o formulă de stoc sau regulă contabilă → bază; apare doar pe ecranul unui tip → derivată.
- [3 — Mapare EF Core](decizii/003-mapare-ef-core-tpt.md) — EF Core TPT pentru header și pentru detaliile derivate (frunzele există unde schema diferă; declarația per document = [TipDetaliu], 40a).
- [4 — Structura devine cod, politica rămâne date](decizii/004-structura-cod-politica-date.md) — Structura devine cod, politica rămâne date: câmpurile per tip = clase + validare declarativă; maparea Clasă/Tip → conturi, definițiile de stoc, numerotarea, conexul, scadența, T…
- [5 — Backend](decizii/005-backend-xaf-frontend-react.md) — Backend XAF (logică, securitate, validare, audit) expus prin Web API; frontend React + DevExtreme pentru ecranele operaționale.
- [6 — API-ul NU expune ierarhia polimorfic](decizii/006-api-fara-ierarhie-polimorfa.md) — API-ul NU expune ierarhia polimorfic: endpoint per tip de document, DTO-uri plate prin codegen OpenAPI→TS; moștenirea e detaliu de persistență.
- [7 — Nav properties în Web API](decizii/007-fk-explicite-jsonignore.md) — Nav properties în API: FK-urile explicite, JsonIgnore pe navigații.
- [8 — Metadata-driven UI](decizii/008-metadata-driven-ui.md) — Metadata-driven UI doar pe criteriul deciziei 4: build-time = OpenAPI→TS + captions; runtime = doar politici-date; layout-ul per tip e cod React (rafinat de 42e — serializarea l…
- [9 — Scope faza 1](decizii/009-scope-faza-1.md) — Scope faza 1: contabilitate + gestiune, plățile/încasările ca documente.
- [10 — Plan de conturi doar sintetic](decizii/010-plan-de-conturi-sintetic.md) — Plan de conturi doar sintetic; analiticele se derivă din dimensiuni la raportare, nu se persistă.
- [11 — Dimensiuni pe note](decizii/011-sursa-finantare-dimensiune-explicita.md) — Sursa de finanțare = dimensiune explicită (mecanismul COMPUSA din legacy se elimină; denumirile dimensiunilor normalizate).
- [12 — Migrare green-field la graniță de ciclu](decizii/012-migrare-green-field.md) — Migrare green-field la graniță de ciclu: fără paritate document-cu-document; se preiau nomenclatoare, politici (seed), solduri/stocuri de deschidere.
- [13 — Evaluare stoc](decizii/013-evaluare-stoc-identificare-pe-lot.md) — Evaluare stoc: identificare specifică pe lot. Produs (catalog) + Lot (creat de linia de intrare, preț unitar fix, gestiune, dată); ieșirile referă lotul; picking auto-FIFO î…
- [14 — Registre persistate, append-only](decizii/014-registre-append-only.md) — Registre persistate, append-only.
- [15 — Dimensiuni](decizii/015-dimensiuni-owned-type.md) — Dimensiunile = setul R/M/CF/CE/F/U/P/CC (FK nullable fiecare); regula de notă poartă Comun/OverrideDebit/OverrideCredit, rezolvarea (coalesce) e generică în motor; linia d…
- [16 — Repartitori](decizii/016-repartitori-tpt-si-calitati.md) — Repartitori: TPT bază + derivate doar unde schema diferă și identitatea e exclusivă (Partener, Gestiune, Angajat, UnitateInterna, ContPropriu); calitățile transversale (Gestiona…
- [17 — Plăți/încasări = tipuri de document](decizii/017-plati-incasari-imperechere-conex.md) — Plăți/încasări = tipuri de document; Imperecherea (stingerea) = link many-to-many plată↔factură cu sume parțiale, NU document (31d).
- [18 — Migrarea preia nomenclatoare + solduri de deschidere](decizii/018-migrarea-preia-nomenclatoare-solduri.md) — Migrarea preia nomenclatoare + solduri de deschidere; politicile se SEED-uiesc (definite curat), nu se migrează.
- [19 — Lista țintă de tipuri de documente](decizii/019-lista-tipurilor-de-documente.md) — Tipurile de document = ierarhia de clase: FacturaIntrare, FacturaIesire, NIR, BonConsum, NotaTransfer, ListaDiferenteInventar, Decont, Plata, Incasare (+ DescarcareGestiune 37; …
- [20 — Un singur nomenclator de tipuri, și e codul](decizii/020-un-singur-nomenclator-de-tipuri.md) — Un singur nomenclator de tipuri, și e codul: nivelul intermediar tip × predator × primitor din legacy dispare — derivata își fixează direcția; TipDocument seed oglindește clas…
- [21 — Seed-ul politicilor se face PE FUNCȚIONALITATE, nu prin transcrierea config-ului legacy](decizii/021-seed-pe-functionalitate-nu-transcriere.md) — Seed-ul politicilor se face PE FUNCȚIONALITATE, nu prin transcrierea config-ului legacy (nici a lui 1C — 35b).
- [22 — Testul bazei](decizii/022-testul-bazei-inchis.md) — Testul bazei — închis (db/inventar/11-testul-bazei.md).
- [23 — Felia 3a](decizii/023-felia-3a-persistenta-seed.md) — Persistență + seed.
- [24 — Owned Dimensiuni sub XAF/EF Core — limitarea e gestionabilă, rămânem pe EF Core](decizii/024-owned-dimensiuni-sub-xaf.md) — EF Core, nu XPO. Restul (owned Dimensiuni sub XAF, OwnedObjectBase, CreateProxy) e DEPĂȘIT de 54c.
- [25 — Felia 3b](decizii/025-felia-3b-motorul-de-operare.md) — Motorul de operare (Module/Motor/).
- [26 — Felia 3c-1 — NIR + FacturaIntrare + mecanismul conex](decizii/026-felia-3c1-nir-fct-conex.md) — NIR + FacturaIntrare + conex.
- [27 — Felia 3c-2 — BonConsum](decizii/027-felia-3c2-bon-consum.md) — BonConsum.
- [28 — Felia 3c-3 — ListaDiferenteInventar](decizii/028-felia-3c3-lista-diferente-inventar.md) — ListaDiferenteInventar.
- [29 — Profil contabil](decizii/029-profil-contabil-privat-first.md) — Profil contabil = pachet de seed per bază; motorul e AGNOSTIC la plan (niciun simbol hardcodat).
- [30 — Felia 3c-4 — FacturaIesire](decizii/030-felia-3c4-factura-iesire.md) — FacturaIesire.
- [31 — Felia 3c-5 — Plata/Incasare + Imperechere](decizii/031-felia-3c5-plata-incasare-imperechere.md) — Plata / Incasare + Imperechere.
- [32 — Felia 3c-6 — Decont](decizii/032-felia-3c6-decont.md) — Decont.
- [33 — Felia 3d — validarea transversală](decizii/033-felia-3d-validare-transversala.md) — Validarea transversală.
- [34 — Pasul 4 — migrarea datelor](decizii/034-pasul-4-migrarea-datelor.md) — Migrarea legacy (nou/tools/Migrare; azi prototip de conector, 35a).
- [35 — Pivot privat-first](decizii/035-pivot-privat-first.md) — Pivot privat-first.
- [36 — Felia P1 — profil privat + TVA structural](decizii/036-felia-p1-tva-structural.md) — TVA structural.
- [37 — Design P2 — descărcarea de gestiune la FacturaIesire](decizii/037-design-p2-descarcare-gestiune.md) — Descărcarea de gestiune (design).
- [38 — Felia P2 — DescarcareGestiune](decizii/038-felia-p2-descarcare-gestiune.md) — DSC executat.
- [39 — Pre-polish](decizii/039-atlas-dxf-constraint-translator.md) — Atlas.DXF în Conta.
- [40 — Felia polish XAF](decizii/040-felia-polish-xaf.md) — Polish XAF. (a) [TipDetaliu] per document: New creează derivata, coloanele ei; baseline-ul ascunde FK-urile brute.
- [41 — Felia „restanțele 40e](decizii/041-restantele-40e-atlas-dxf-26-1-3-6.md) — Genericul în Atlas.DXF.
- [42 — Design pasul 5 — tierul API + React](decizii/042-design-p5-tierul-api.md) — Design API. (a) O singură sursă de reguli: gardian de Committing activ DOAR pe ObjectSpace-uri SECURED; non-secured = ușa de sistem; nimeni n-are Write pe registre.
- [43 — Design pasul 5 — clientul React](decizii/043-design-p5-clientul-react.md) — Design React.
- [44 — Reordonarea roadmap-ului](decizii/044-reordonare-roadmap-1c-inainte-de-p5.md) — XAF Blazor = vehiculul de iterație al modelului; luptele structurale cu Blazor NU se hack-uiesc — merg în lista-react.md.
- [45 — Design FAZA 1C — conector de import + reconciliere pe an fiscal complet](decizii/045-design-faza-1c-import-reconciliere.md) — Design 1C.
- [46 — Felia 1C-a — tipurile noi de model](decizii/046-felia-1c-a-tipurile-noi.md) — Tipurile noi. (a) Storno = valori NEGATIVE pe corespondența ORIGINALĂ; RegulaContare.PastreazaSemn; flag-ul Storno = al meta-operației.
- [47 — Felia 1C-b — scheletul Import1C + nomenclatoare + deschiderea](decizii/047-felia-1c-b-import1c-deschiderea.md) — Import1C — nomenclatoare + deschiderea.
- [48 — Analiza pre-1C-c — patru probleme tranșate ÎNAINTE de execuție](decizii/048-analiza-pre-1c-c.md) — Pre-1C-c.
- [49 — Felia 1C-c — documentele 2025 prin motor](decizii/049-felia-1c-c-documentele-prin-motor.md) — Documentele prin motor.
- [50 — Felia 1C-d — gate-ul fazei](decizii/050-felia-1c-d-gate.md) — Gate-ul 1C-d.
- [51 — Analiza post-gate 1C-d](decizii/051-analiza-post-gate-1c-d-rotunjire-cmp.md) — Post-gate.
- [52 — Felia 1C-d-final](decizii/052-felia-1c-d-final.md) — 1C-d-final — FAZA 1C ÎNCHISĂ.
- [53 — GATE XAF](decizii/053-gate-xaf.md) — GATE XAF trecut („un contabil tolerant le operează zilnic", NU product-grade).
- [54 — Sesiunea de arhitectură 2026-08-02](decizii/054-owned-vs-relational-invarianti-dimensiuni-pe-frunze.md) — Owned vs relational.
- [55 — Pasul 5, spike 1 — fir complet subțire API+React pe NotaTransfer](decizii/055-p5-spike1-btr.md) — Spike BTR — regulile tuturor feliilor.
- [56 — Pasul 5, felia 2 — FacturaIntrare + conex-NIR prin API](decizii/056-p5-felia2-fct-nir.md) — FCT + NIR prin API. LoturiCulegereService = seam-ul UNIC al loturilor (XAF și Apply).
- [57 — Pasul 5, felia 3 — trezoreria prin API](decizii/057-p5-felia3-trezorerie.md) — Trezoreria prin API. (a) TrezorerieApply<T> unic; Numar server-owned; enum-uri pe sârmă ca string, parse pe NUME înainte de CreateObject.
- [58 — Pasul 5, felia 4 — FCL + descărcarea de gestiune prin API](decizii/058-p5-felia4-fcl-dsc.md) — FCL + DSC prin API. FCL nu naște loturi (pinul REFERĂ).
- [59 — Măsurarea de perf a proiecțiilor pe baza de import](decizii/059-p5-perf-masuratori.md) — Perf: totul sub ~150 ms în afară de DocumenteCuRest (~410 ms, structural); optimizarea documentată se aplică CÂND cifra o cere; niciun index preventiv.
- [60 — Mărunțișurile F3/F4 golite](decizii/060-p5-maruntisuri-f3-f4.md) — Mărunțișuri.
- [61 — Restanțele §Închidere F3/F4 golite](decizii/061-p5-restante-f3-f4.md) — Restanțe.
- [62 — Pasul 5, felia 5 — NIR scriere](decizii/062-p5-felia5-nir-scriere.md) — NIR scriere.
- [63 — Pasul 5, felia 6 — LDI + BCS prin API și client](decizii/063-p5-felia6-ldi-bcs.md) — LDI + BCS prin API. NasteLot pe contract (LDI: doar Plus), gard înaintea celui de lot străin; gestiunea = PREDATORUL.
- [64 — Pasul 5, felia 7 — viramentul intern](decizii/064-p5-felia7-virament-581.md) — Viramentul 581.
- [65 — Pasul 5, felia 8 — Decont](decizii/065-p5-felia8-decont-pereche.md) — Decont + pereche.
- [66 — Pasul 5, felia 9 — raportarea pe registre](decizii/066-p5-felia9-raportare.md) — Raportarea pe registre.
- [67 — Pasul 5, felia 10 — balanța pliată pe planul de conturi](decizii/067-p5-felia10-balanta-pliata.md) — Balanța pliată.
- [68 — Pasul 5, felia 11 — jurnalele de TVA](decizii/068-p5-felia11-jurnale-tva.md) — Jurnalele de TVA — RegistruTva, al treilea registru, un rând per LINIE (liniile fără TVA postat apar legal în jurnal); generarea cere PoliticaTva pe tip ȘI TipTva pe linie…
- [69 — Pasul 5, felia 12 — D300, decontul de TVA ca proiecție peste RegistruTva](decizii/069-p5-felia12-d300.md) — D300 = proiecție peste RegistruTva.
- [70 — Pasul 5, felia 13 — motor/structură post-D300 (69-r4/r7/r5 + 67e)](decizii/070-p5-felia13-motor-structura.md) — Motor/structură post-D300.
- [71 — Pasul 5, felia 14 — D394, declarația informativă ca proiecție peste RegistruTva](decizii/071-p5-felia14-d394.md) — D394 = proiecție peste RegistruTva, per partener.
- [72 — Pasul 5, felia 15 — identitatea extinsă a partenerului + sincronizarea ANAF](decizii/072-p5-felia15-partener-anaf.md) — Partener + ANAF.
- [73 — Pasul 5, felia 16 — SAF-T (D406 L) ca proiecție peste registre](decizii/073-p5-felia16-saft.md) — SAF-T (D406 L) = proiecție peste registre + FIȘIER.
- [74 — Pasul 5, felia 17 — SAF-T D406 S (stocuri) peste RegistruStoc](decizii/074-p5-felia17-saft-s.md) — SAF-T S (stocuri) = proiecție peste RegistruStoc + FIȘIER C.
- [75 — Pasul 5, felia 18 — restanțele grele ale lui S: golirea valorică (motor), reclasificarea ca mișcare, perf proiecție](decizii/075-p5-felia18-restante-s.md) — Restanțele grele ale lui S (F18).
- [76 — Pasul 5, felia 19 — NTC + ASM + retururi (RLF/RDC) prin API și client](decizii/076-p5-felia19-ntc-asm-retururi.md) — NTC + ASM + retururi prin API și client (ultimele patru tipuri fără felie de scriere).
- [77 — Pasul 5, felia 20 — finisajul clientului: căutarea fără diacritice, cache-ul de nomenclator, confirmările, Neincluse agregat, primele ecrane de nomenclator](decizii/077-p5-felia20-finisaj-client.md) — Finisajul clientului (F20).
- [78 — Căutarea fără diacritice pe proiecții — rescrierea predicatelor DataSourceLoader prin normalizatorul comun](decizii/078-cautare-fara-diacritice-proiectii.md) — Căutarea fără diacritice pe PROIECȚII (perechea lui 77a — acolo coloana generată pe OData, aici ușa DataSourceLoader).
- [79 — Pasul 5, felia 21 — ITV (închiderea de TVA) prin API și client: comandă cu cauză + ecran de rezultat](decizii/079-p5-felia21-itv.md) — ITV prin API și client — COMANDĂ cu cauză, nu agregat (76a).
- [80 — Refuzurile de acces pe toate ușile: 404 / 403 / 422 — o singură ordine, un singur corp](decizii/080-refuzuri-acces-toate-usile.md) — Refuzurile de acces pe toate ușile: 404 / 403 / 422, o singură ordine, un singur corp (închide 77-r8 și familia 70-r1/72-r10/76-r4/76-r5/77k/79-r6; amendează 79b).
- [81 — Pasul 5, felia 23 — implicitele de culegere și întreținerea politicilor: regimul e al partenerului, cota e a produsului; politicile se deschid pe OData cu invarianții în gardian, unicitatea în schemă și proveniența ca timbru](decizii/081-p5-felia23-implicite-politici.md) — Felia 23 — implicitele de culegere + întreținerea politicilor.
- [82 — Stingerea automată prin contract de document](decizii/082-stingere-automata-prin-contract.md) — Stingerea automată prin contract (invariantul II).
- [83 — Seed-ul și proveniența: re-seed-ul corectează doar DinSeed, golurile privat (NIM / IMP cu poarta spre DVI), rolul Configurator](decizii/083-seed-provenienta-goluri-configurator.md) — Seed-ul și proveniența (execuția în felia 24).
- [84 — Pasul 5, felia 24 — politicile: seed-ul aliniază, rolul Configurator, cele șapte ecrane, potrivirea ca funcții pure și „Explică"](decizii/084-p5-felia24-politici-explica.md) — Felia 24 — politicile: seed-ul aliniază, Configurator, cele șapte ecrane, Potrivire, „Explică" (execută 83; închide 81-r8, 81k).
- [85 — Modul de acces al ListView-urilor XAF Blazor (EF Core, PostgreSQL): Server implicit cu paginare, ServerView opt-in pe registre, Client explicit pe grilele de culegere, IF/IFV doar cu prag măsurat](decizii/085-mod-acces-listview-xaf.md) — Modul de acces al listelor XAF Blazor.
- [86 — Pasul 5, felia 25 — DVI (declarația vamală de import) ca tip de document: TVA-ul în vamă intră în evidență, D300 rd. 24/25 și 7/22, codurile SAF-T de import](decizii/086-p5-felia25-dvi.md) — DVI — declarația vamală de import (închide 83-r4).
- [87 — Pasul 5, felia 26 — Imobilizări și amortizare, contabil și fiscal: fișa ca nomenclator subțire, un singur registru append-only de evenimente și luni, PIF/CAS ca documente operate, AMO generată lunar cu trei cifre din AmortizareService, conturile și regulile de deductibilitate exclusiv din politică, versionate în timp](decizii/087-p5-felia26-imobilizari.md) — Imobilizările — evidența pe tiparele existente, fără mecanism nou în nucleu.
- [88 — Pasul 5, felia 27 — Perioada fiscală ca lanț, închiderea ca comandă cu acceptare conștientă, soldurile și partidele deschise materializate la închidere, data înregistrării ca reper al registrelor, perioada de declarare ca reper fiscal, corecția legată cu motiv](decizii/088-p5-felia27-perioade-solduri.md) — Perioada fiscală e un LANȚ, nu un flag; închiderea e o comandă a motorului, iar ce se închide se materializează.
- [89 — Pasul 5, felia 28 — TPH cu discriminatorul mapat ClrType pe Document, DocumentDetaliu și Repartitor; coloanele partajate fără prefix de tip; coloana de frunză se citește doar pe o mulțime restrânsă pe tip; tipul țintei unui FK spre frunză ținut de gardian și probat în bază; tipul documentului ca dată, printr-un singur cititor; căutarea după cheie a unui tip ne-rădăcină pe rădăcina ierarhiei, cu tipul verificat; lanțul de migrații resetat](decizii/089-tph-discriminator-tipul-ca-data.md) — Tipul concret al unui rând e o DATĂ pe rând, nu o tabelă: cele trei ierarhii stau fiecare pe tabela rădăcinii, cu discriminatorul mapat ClrType.
- [90 — Nucleul ca ledger n-dimensional — un singur cub de postări în locul celor patru registre; motor pur pe operand închis (Contract = Postari + Decizii + Ipoteze); unitatea nominalizată (lot = partidă = fișă) numită pe linie; împerecherea ca nominalizare la operare + document Împerechere cu tranzacție de fel Transfer; linia FCT postează recepția; o singură postare de stoc, TipStoc dispare structural; stornoul ca tranzacție distinctă = inversul cauzat ∪ atribuit; fiscala postată în Carte=Fiscal; ordinea TR-D6a…D10 cu Import1C ca probă supremă; XAF și React înghețate pe funcții](decizii/090-nucleu-cub-de-postari.md) — Registrul sistemului e un singur cub de postări; documentul tipat rămâne la intrare, fluxul stă la ieșire ca rezultat al motorului, iar tot ce e sold, rest, cost, curs sau valoa…
- [91 — Proba supremă devine catalogul de scenarii — Import1C se îngheață ca unealtă de migrare, nu ca gate; clona Flax rămâne sursă de întrebări; „rotund" primește definiție ca regulă de oprire a PoC-ului; stornoul după reevaluare = inversarea originalului + compensarea efectelor rămase active; proiecțiile persistate doar reconstruibile din postări; triajul restanțelor în patru stări](decizii/091-scenarii-in-loc-de-import.md) — Motorul se probează pe scenarii scrise de mână din regula contabilă, nu prin egalitate cu motorul vechi pe o bază importată.
- [92 — Identitatea partidei include partenerul](decizii/092-identitatea-partidei-include-partenerul.md) — (a) Partida nouă are ID determinist din (document deschizător, cont, partener).
- [93 — Folosința păstrează gestiunea reală și lotul](decizii/093-folosinta-gestiune-reala.md) — (a) Coordonatele Folosință sunt gestiunea reală, lotul și contul politicii.
- [94 — Deschiderea detaliază soldul contabil, fără dublare](decizii/094-deschidere-prin-detalierea-soldului.md) — (a) Postările de lot și partidă înlocuiesc soldul bloc pe cheia Carte/Cont/Latura.
- [95 — DEC și imobilizările complete pe cub înaintea TR-D8](decizii/095-producatori-completi-inaintea-citirilor.md) — (a) Ordinea este DEC → contractul complet al imobilizărilor → PIF/AMO/CAS pe cub → TR-D8.
- [96 — Urmărirea partidelor este separată de rolul comercial](decizii/096-urmarirea-partidelor-separata-de-rolul-comercial.md) — (a) Cont.UrmarestePartide stabilește dacă motorul urmărește restul pe unități de partidă.
- [97 — PIF nominalizează valoarea contabilă cu suport obligatoriu](decizii/097-pif-nominalizare-cu-suport-obligatoriu.md) — (a) PIF nominalizează pe fișă valoare contabilă existentă.
- [98 — Conexul editat postează delta; poziția fără fișă nu devine negativă; contul fișei vine din cub](decizii/098-corecturi-review-sincronizare.md) — (a) NIR conex acoperit postează numai delta față de sursă.
- [99 — Cauza diferenței dintre evidență și constatarea fizică](decizii/099-cauza-diferentei-evidenta-constatare.md) — (a) Cauza diferenței este o noțiune generală a domeniului, nu a NIR. Ea clasifică diferența dintre ce spune evidența (factura, scripticul, transferul) și ce s-a constatat fizic.
- [100 — Partide comerciale în politica bugetară](decizii/100-partide-comerciale-bugetare.md) — (a) Politica bugetară activează Cont.UrmarestePartide pe conturile 401.01.00, 404.01.00 și 411.01.01 cu DinSeed=true.
- [101 — Împerecherea cere efect verificabil pe partidă](decizii/101-imperechere-cu-efect-pe-partida.md) — (a) O împerechere nouă nu poate consuma un disponibil numai în registrul legăturilor.
- [102 — Fără compatibilitate cu bazele de dezvoltare până la prima producție](decizii/102-fara-compatibilitate-cu-bazele-de-dezvoltare.md) — (a) Nu există bază de producție.
- [103 — Fapte fiscale istorice și perioade distincte de declarare](decizii/103-fapte-fiscale-istorice-si-perioade-distincte.md) — (a) Cubul păstrează calificarea fiscală istorică: regim, cotă, import, identitatea documentului fiscal și atribuiri distincte D300/D394.
- [104 — Straturi izolate, culegerea în XAF, entități proprii fără ștergere amânată](decizii/104-straturi-izolate-entitati-proprii.md) — (a) Cinci straturi, cu dependențe într-un singur sens.
- [105 — SAF-T D406 pe cub: sursă unică, acces complet, certificare cu manifest](decizii/105-saft-pe-cub.md) — Starea verificării B8, 2026-09-30: toate constatările sunt rezolvate.
- [106 — Regimul pe stare: o singură sursă a editabilității și a comenzilor disponibile](decizii/106-regim-pe-stare.md) — (a) Formatul unitar al ecranului de document e definit de regimul pe stare.
- [107 — Migrarea deschide terții per partidă din BalantaNivel3, prin Materializare.Deschide; stingerile anului pe partide inițiale; partener generic de migrare](decizii/107-deschiderea-de-terti-din-balanta-nivel3.md) — Cutoff-ul migrării se bazează pe solduri, nu pe documente.
- [108 — Gate-ul transversal TR-D8: registrele nu se mai citesc, scrierea e serială per bază, explicația deciziei e persistată](decizii/108-gate-transversal-tr-d8.md) — Starea verificării, 2026-10-04: pașii 1–6 din X-D8 sunt implementați și verificați pe tr-d8-transversal.
- [109 — Taxa nemarcată se decide pe document × cotă și se scrie pe linie; declarația postează taxa liniei; taxa unui document real adusă de o cale fără culegere e marcată](decizii/109-taxa-nemarcata-pe-document-scrisa-pe-linie.md) — Linia documentului poartă taxa care se postează.
- [110 — Tăierea: cubul e singurul scriitor și singura sursă; registrele, regimul dual și oracolul lor au ieșit (TR-D9a)](decizii/110-taierea-cubul-singurul-scriitor.md) — Operarea unui document are un singur efect contabil: tranzacțiile contractului, scrise în cub. Nu există al doilea scriitor, al doilea sold și nici un regim care să aleagă între…
- [111 — Un singur repartitor de sold pe postare, partenerul fiscal în blocul fiscal, registre tipate pentru ce nu se balansează](decizii/111-repartitor-unic-si-registre-tipate.md) — Postarea poartă o singură coordonată de sold pentru „cine sau unde", cu felul ei declarat de cont.
- [112 — Recepția facturii se contează pe regula ei de contare (FCT/Stoc)](decizii/112-receptia-facturii-pe-regula-ei.md) — Linia de stoc a facturii de intrare se contează pe regula de contare a facturii, ca orice altă natură.
- [113 — Semnul valorii e al liniei; RegulaContare.PastreazaSemn a ieșit](decizii/113-semnul-e-al-liniei-pastreazasemn-scos.md) — Valoarea se postează cu semnul liniei.
- [114 — Departajarea FIFO: data, documentul deschizător, apoi identificatorul](decizii/114-departajarea-fifo-pe-documentul-deschizator.md) — La aceeași dată de deschidere, FIFO consumă întâi unitatea deschisă de documentul creat mai devreme.
- [115 — Linia de notă poate numi partida pe care o stinge; partida inițială își poartă data de naștere](decizii/115-nota-numeste-partida.md) — O linie de notă contabilă poate numi, pe fiecare latură, partida pe care o stinge.
- [116 — Unitatea fără document se departajează pe postarea ei de deschidere; ID-urile postărilor urmează ordinea tranzacției](decizii/116-postarea-de-deschidere-departajeaza-unitatea-fara-document.md) — Ordinea în care deschiderea își primește unitățile e ordinea în care FIFO le consumă la aceeași dată.
- [117 — Postarea își poartă ordinalul în tranzacție; unitatea fără document se departajează pe el](decizii/117-ordinalul-postarii-in-tranzactie.md) — Ordinea postărilor unei tranzacții e dată persistată, nu o proprietate a ID-urilor.
- [118 — Guvernanța documentației: kitul unificat dă forma, Atlas.Conta îl aplică pe structura lui, cu gardieni care intră pe rând](decizii/118-guvernanta-documentatiei.md) — Forma documentației e dată de kitul de guvernanță.
- [Jurnalul de decizii — index (un fișier per decizie)](decizii/README.md)
- [Istoricul de execuție al planului de lucru (snapshot 2026-08-24)](decizii/istoric-plan-de-lucru.md) — 1.
- [Restanțele și amânările cu nume](decizii/restante.md) — Backlog-ul dezvoltatorului: fiecare rând e o amânare declarată într-o decizie.

### `docs/dim/` (1)

- [DIM-2 — Inventarul dimensiunilor culese per tip (pasul 1, decizia 54e)](dim/dim-2-inventar.md) — F1.

### `docs/import/` (5)

- [FAZA 1C — conector de import + reconciliere pe an fiscal complet (design)](import/faza-1c-design.md) — Baza 1C flax pe SQL Server (local), trusted connection; view-urile lizibile generate de SkyContaSource există în EServicesFlx, schema [flax] — 262 de view-uri, verificat…
- [M1 — Deschiderea de terți din BalantaNivel3 și stingerile 2025 pe partide inițiale (contract)](import/m1-deschidere-terti-contract.md) — Ce deschide: 091-r4 spune „conectorul repornit pe modelul final, deschiderea de terți din BalantaNivel3 (T-r4), stingerile 2024".
- [M1 — review advers Codex](import/m1-review-codex.md) — Stare curentă: reverificarea finală la 111b89a închide M1-R1–R4, inclusiv M1-R3a, cu limitele documentate.
- [M1 — reverificarea corecturilor R1–R4](import/m1-rv1-review-codex.md) — Stare curentă: RV2 la 111b89a închide și M1-R3a.
- [M1 — reverificare finală M1-R3a](import/m1-rv2-review-codex.md) — Verdict: M1-R3a închisă.

### `docs/nucleu/` (54)

- [Decizia 109 — review advers Codex](nucleu/109-review-codex.md) — Verdict inițial: review neînchis — o observație P2, reprodusă.
- [C102 — Felia de curățenie: fără compatibilitate cu bazele de dezvoltare](nucleu/c102-curatenie-contract.md) — Scoatem codul și textul care există numai pentru date persistate înaintea codului curent.
- [C106 — reverificarea corecturilor R1…R4](nucleu/c106-review-codex-rv1.md) — 2026-10-04, Codex.
- [C106 — închiderea observației R5](nucleu/c106-review-codex-rv2.md) — 2026-10-04, Codex.
- [C106 — review advers Codex](nucleu/c106-review-codex.md) — Verdict inițial: necesită corecturi — patru observații P2.
- [Bilanțul noii forme: ce câștigăm, ce pierdem (sinteza pașilor 1–3, pentru TR-D5)](nucleu/nucleu-bilant.md) — Citirea scurtă: noua formă se plătește prin corectitudine și prin unicitatea mecanismului, nu prin viteză și nu prin disc.
- [Coordonatele cubului contra rapoartelor reale (pasul 1 din §11)](nucleu/nucleu-coordonate-rapoarte.md) — „Minimal" se ține.
- [Nucleul ca ledger n-dimensional (design greenfield)](nucleu/nucleu-cub-design.md) — Conceptele curente sunt corecte (structură tipată per document, motorul pe bază, politica ca date, registre append-only, gard de perioadă, FIFO pe lot persistat).
- [Fizica cubului, măsurată pe baza de import (pasul 2 din §11)](nucleu/nucleu-fizica.md) — Baza: clona lui Atlas.Conta.Import1C.Flax (2025 integral, profil privat: 304.382 rânduri contabile, 283.498 de stoc, 90.732 fiscale, 205.186 documente operate, 44.448 împerech…
- [Transferul: ce se mută din implementarea curentă în nucleu și în ce ordine (pasul 3 din §11)](nucleu/nucleu-transfer.md) — 1.
- [TR-D6a — Nucleul pur (contract)](nucleu/tr-d6a-nucleu-pur-contract.md) — Proiectul nou/Atlas.Conta.Nucleu (BCL, zero pachete, zero referințe la EF/XAF/HTTP) cu tipurile cubului și regulile pure cerute de pilotul TR-D6b (BCS, PLT, FCT), plus Atlas.…
- [TR-D6b — Declarația fluxului per tip (pilot BCS, PLT, FCT) — contract](nucleu/tr-d6b-declaratia-fluxului-contract.md) — E singurul pas care poate întoarce decizia 090.
- [TR-D7a — Strangler-ul per tip, felia 31: cubul persistat și cele trei tipuri ale pilotului](nucleu/tr-d7a-strangler-contract.md) — Felii TR-D7: TR-D7 e 6–8 sesiuni (§4.1).
- [ASM — transformarea produselor și conservarea cantității](nucleu/tr-d7b-asm-transformare-contract.md) — 2026-09-23.
- [TR-D7b — Deschiderea generică](nucleu/tr-d7b-deschidere-contract.md) — Cub.Materializare.Deschide primește solduri inițiale și detalierea lor, într-o singură tranzacție Fel=Deschidere, fără document.
- [DVI — baza vamală distinctă în cartea fiscală](nucleu/tr-d7b-dvi-baza-fiscala-contract.md) — 2026-09-23.
- [LDI — diferențe de inventar pe cub](nucleu/tr-d7b-ldi-contract.md) — 2026-09-24.
- [TR-D7b — Strangler-ul per tip, felia 32: tipurile rămase pe cub și deschiderea ca tranzacție](nucleu/tr-d7b-tipuri-ramase-contract.md) — Felii TR-D7: felia 31 (TR-D7a) = fundația persistată + BCS, FCT, PLT, INC. Felia 32 (TR-D7b) = TOATE tipurile rămase + deschiderea.
- [DEC — declarație completă înaintea TR-D8](nucleu/tr-d7c-dec-contract.md) — DEC justifică cheltuiala titularului.
- [Imobilizări complete pe cub înaintea TR-D8](nucleu/tr-d7c-imobilizari-contract.md) — Catalog numeric: scenarii/IMO.md.
- [TR-D8 — Citiri comune și comutarea consumatorilor](nucleu/tr-d8-citiri-contract.md) — O singură intrare Module/Cub/Citiri definește domeniul fiecărei citiri.
- [TR-D8 — Inventarul de intrare și ordinea portării](nucleu/tr-d8-citiri-inventar.md) — 2026-09-25.
- [NIR — diferența față de recepția postată de FCT](nucleu/tr-d8-nir-delta-contract.md) — FCT declară structural că acoperă recepția conexă.
- [TR-D8 — review propriu Codex](nucleu/tr-d8-review-codex.md) — 2026-09-25.
- [TR-D8 SAF-B8 — raportul A/B final: ruta pe registre ↔ cubul](nucleu/tr-d8-saft-ab.md) — B8-RV1.2 corectat și verificat de Codex, 2026-09-30: constatările B8 închise.
- [SAF-B8 — review advers Codex](nucleu/tr-d8-saft-b8-review-codex.md) — B8-RV1.2 rezolvată; toate constatările B8 sunt închise.
- [SAF-B8 — corecția B8-RV1.2](nucleu/tr-d8-saft-b8-rv12-corectie.md) — Clasificator.Factura verifică ambele ramuri SAF-B5 prin FacturaDinCub.
- [TR-D8 — SAF-T pe cub: delimitare aprobată și contract S1](nucleu/tr-d8-saft-contract.md) — 2026-09-28 — SAF-D1/D2/D4=A și SAF-D3=C aprobate de owner.
- [TR-D8 SAF-T S0 — review advers Codex](nucleu/tr-d8-saft-s0-review-codex.md) — 2026-09-29.
- [TR-D8 SAF-T S1 — review advers Codex](nucleu/tr-d8-saft-s1-review-codex.md) — 2026-09-28.
- [TR-D8 SAF-T S2 — review advers Codex](nucleu/tr-d8-saft-s2-review-codex.md) — 2026-09-29.
- [TR-D8 SAF-T S3 — review advers Codex](nucleu/tr-d8-saft-s3-review-codex.md) — Răspuns la comunicari/2026-09-30-0050-claude-codex-saft-s3-rv21-corectat.md.
- [TR-D8 — gate-ul transversal: contract pentru aprobare](nucleu/tr-d8-transversal-contract.md) — Toate consumatoarele din inventar sunt portate nominal (contabil, stoc, partide, fiscal, SAF-T, imobilizări, snapshot-uri).
- [TR-D8 transversal — review advers al implementării](nucleu/tr-d8-transversal-implementare-review-codex.md) — Stare curentă, 2026-10-04: X-RI1…X-RI4 închise, după reverificarea corecturilor 791e41c la HEAD 7223abc.
- [TR-D8 transversal — review advers al contractului](nucleu/tr-d8-transversal-review-codex.md) — Toate observațiile X-RV1…X-RV7 sunt închise la nivel de contract, inclusiv X-RV1.1, X-RV1.2 și X-RV5.1.
- [TR-D8 — intervale TVA și avertismente, R6](nucleu/tr-d8-tva-intervale-contract.md) — TipTva.ValabilDeLa / ValabilPanaLa sunt opționale, cu limite incluzive.
- [TR-D9a, pasul 1 — inventarul referințelor ModelCheck la registre](nucleu/tr-d9-inventar-modelcheck.md) — Completat (2026-10-05) cu trecerea pe flux de date (secțiunile 1 și 12) și cu aserțiile pe RegulaStoc (secțiunea 11).
- [TR-D9a — inventarul pasului 1](nucleu/tr-d9-inventar.md) — Ce conține: partea de produs și de unelte.
- [TR-D9a — pasul 2: gardul analizei pe mișcări și probele dinaintea tăierii](nucleu/tr-d9-pas2-probe.md) — Ce conține: ce a intrat în cod, ce s-a măsurat, ce corectează inventarul pasului 1 și ce rămâne de hotărât.
- [TR-D9a — pasul 3: aserțiile de regulă re-țintite pe cititorii cubului](nucleu/tr-d9-pas3-retintire.md) — Ce conține: contabilitatea probelor (D9-D7 e), aserțiile rămase pe forma veche și cine le ia, ce corectează inventarul pasului 1.
- [TR-D9a — pasul 5b: probele dinaintea tăierii (D9-A1)](nucleu/tr-d9-pas5b-probe.md) — Ce conține: ultima reconciliere registre ↔ cub cu fiecare diferență explicată, PerfCub în regim dual ca termen A al comparației de după tăiere, scara de volum și cifra pentru …
- [TR-D9a, pasul 5c — îmbinarea partidelor pe chei nulabile](nucleu/tr-d9-pas5c-imbinare-partide.md) — Scara de volum a arătat că partidele cu rest și snapshot-ul de partide cresc pătratic.
- [TR-D9a — pasul 6: tăierea scriitorilor vechi și a regimului dual](nucleu/tr-d9-pas6-taierea.md) — Ce conține: ce a ieșit din produs, ce a intrat în locul lui, contabilitatea probelor (D9-D7 e), ce rămâne pașilor 6b și 7, ce e al owner-ului.
- [TR-D9a — pasul 6b: repartitorul pe postare și gardul strict](nucleu/tr-d9-pas6b-repartitor.md) — Ce conține: măsurătorile cerute înaintea codului, ce a intrat în cod, probele, constatările pentru deciziile 110 și 111.
- [TR-D9a, pasul 6c — spargerea mecanică a Program.cs din ModelCheck](nucleu/tr-d9-pas6c-spargere-modelcheck.md) — nou/tools/ModelCheck/Program.cs (32.499 linii, 2,08 MB, 1.994 aserții, 94 de funcții locale la nivelul de sus, 51 de markere de secțiune) a devenit un dispecer de 526 de linii…
- [TR-D9a, pasul 7 — eliminarea atomică a declarațiilor și lista cubului pe PostareVizual](nucleu/tr-d9-pas7-declaratii.md) — Entitățile RegistruContabil, RegistruStoc, RegistruTva (BusinessObjects/Registre/Registre.cs) și RegistruImobilizari (RegistruImobilizari.cs), cu DbSet-urile, aliasu…
- [TR-D9a, pasul 7b — cheia de pereche pe postare (D9-A2)](nucleu/tr-d9-pas7b-pereche.md) — Nucleu.
- [TR-D9a, pasul 7c — versiunea politicii în explicație: regula care a decis (D9-A8)](nucleu/tr-d9-pas7c-versiunea-politicii.md) — Domeniul: tabelele Politica consumate prin operand (Declaratii/Operand.cs, Motor/Fapte.cs, Cub/ReceptiiConexe.Completeaza, Motor/ImobilizariFapte.Completeaza).
- [TR-D9a, pasul 8 — închiderea](nucleu/tr-d9-pas8-inchiderea.md) — Felia e închisă (2026-10-07): review-ul advers Codex e închis (§11), owner-ul a acceptat proba PerfCub pe clone diferite ca abatere de la regula de oprire 6 și a aprobat decizia…
- [TR-D9a — amendamentul 1 la contract: pentru aprobare](nucleu/tr-d9-taierea-amendament-1.md) — Hotărârile owner-ului din 2026-10-05, pe care se sprijină: registrele se taie, fără prototip de registre derivate, cu măsurătoarea la volum înaintea pasului 6; cheia de pereche …
- [TR-D9a — amendamentul 2 la contract](nucleu/tr-d9-taierea-amendament-2.md) — Ce e azi. Flag-ul Repartitor al unui cont nu refuză nimic pe documente.
- [TR-D9a — amendamentul 3 la contract](nucleu/tr-d9-taierea-amendament-3.md) — Ce e azi. Lista XAF Postare (pasul 4, D9-D9) arată identificatori bruți pentru tranzacție, document, cont, partener, gestiune și unitate, fiindcă Postare e POCO fără navigaț…
- [TR-D9a — tăierea registrelor și a regimului dual: contract pentru aprobare](nucleu/tr-d9-taierea-contract.md) — Amendamentul 1 (2026-10-05): hotărârile owner-ului după consultarea cub vs registre și după inventarul pasului 1, D9-A1…D9-A9, cu textul întreg în [tr-d9-taierea-amendament-1.m…
- [TR-D9b, pasul 0 — măsurătorile dinaintea deciziei 111](nucleu/tr-d9b-pas0-masuratori.md) — Cele șase puncte sunt cele din 111 §„Înaintea deciziei".

### `docs/nucleu/scenarii/` (22)

- [ASM — asamblare](nucleu/scenarii/ASM.md) — 2026-09-23; adus la zi 2026-10-07 (TR-D9a, pasul 6: gardul ΣP = ΣC, fără absorbție).
- [BCS — bon de consum](nucleu/scenarii/BCS.md) — Surse: decizia 027 (a)–(c) (consum pe responsabil, contarea 6xx = 3xx), 090 (a), (c), (g), (i) (cub, evaluare pe unitate, cantitate și storno), 088 (data înregistrării, perioada…
- [BTR — transfer între gestiuni](nucleu/scenarii/BTR.md) — 2026-09-23: primul lot independent verificat pe ambele profiluri.
- [CITIRI — TR-D8](nucleu/scenarii/CITIRI.md) — Implementat și verificat în domeniul de mai jos. Owner-ul a aprobat F1=A, F2=A, F3=A (103).
- [DEC — decontul titularului](nucleu/scenarii/DEC.md) — 2026-09-24.
- [DESCHIDERE — solduri inițiale detaliate](nucleu/scenarii/DESCHIDERE.md) — 2026-09-24.
- [DSC — descărcare de gestiune](nucleu/scenarii/DSC.md) — 2026-09-23: primul lot independent verificat pe profilurile aplicabile.
- [DVI — declarație vamală](nucleu/scenarii/DVI.md) — 2026-09-23: implementat și probat prin ScenariiDvi, numai pe privat; profilul bugetar este inert.
- [FCL — factură de ieșire](nucleu/scenarii/FCL.md) — 2026-09-23: primul lot independent verificat pe profilurile aplicabile.
- [FCT — factură de intrare](nucleu/scenarii/FCT.md) — 2026-09-23: primul lot independent verificat pe ambele profiluri.
- [IMO — PIF, AMO, CAS complete pe cub](nucleu/scenarii/IMO.md) — 2026-09-24.
- [INC — încasare](nucleu/scenarii/INC.md) — 2026-09-23: primul lot independent verificat pe ambele profiluri.
- [ITV — închidere TVA](nucleu/scenarii/ITV.md) — 2026-09-23: catalog specificat înaintea implementării pasului 3.
- [LDI — lista diferențelor de inventar](nucleu/scenarii/LDI.md) — 2026-09-24.
- [NIR — recepție manuală și conexul facturii](nucleu/scenarii/NIR.md) — 2026-09-24.
- [NTC — notă contabilă](nucleu/scenarii/NTC.md) — 2026-09-23: catalog specificat înaintea implementării pasului 3.
- [PLT — plată](nucleu/scenarii/PLT.md) — 2026-09-23: primul lot independent verificat pe ambele profiluri.
- [RDC — retur de la client](nucleu/scenarii/RDC.md) — 2026-09-23: așteptări specificate înaintea declarantului.
- [Catalogul de scenarii — proba supremă a motorului (decizia 091)](nucleu/scenarii/README.md) — Tabelul este indexul.
- [REGIM — regimul pe stare (decizia 106)](nucleu/scenarii/REGIM.md) — Regula: Draft → editabil; Operează, Validează, Șterge.
- [RLF — retur la furnizor](nucleu/scenarii/RLF.md) — 2026-09-23: așteptări specificate înaintea implementării.
- [SAF-T — scenarii S1 și matricea de delimitare S2/S3](nucleu/scenarii/SAFT.md) — Reverificare S3 pe b0b2c63, 2026-09-30: RV2.1 și RV2 închise; toate constatările review-ului S3 sunt rezolvate.

### `docs/privat/` (2)

- [P1 — Profil privat + TVA structural (design)](privat/p1-tva-design.md) — DocumentDetaliu (bază): TipMaterialId, LotId?, Cantitate, Valoare, AngajamentId?, Dimensiuni — fără nimic de TVA (decizia 22a: „adăugare ulterioară aditivă" — adău…
- [P2 — Descărcarea de gestiune la FacturaIesire (design)](privat/p2-descarcare-design.md) — FCL azi (30, sub P1): pur creanță — un rând generic 4111 = 7xx la net (credit SursaCont.TipMaterial → contul Tipului VEN) + 4111 = 4427 per linie prin PoliticaTva.

### `docs/stare-curenta/` (7)

- [Atlas.Conta — starea curentă și regulile aplicabile](stare-curenta/README.md) — Implementat: cubul e singurul scriitor și singura sursă contabilă; registrele, regimul dual și oracolul lor au ieșit (110).
- [API și client](stare-curenta/api-si-client.md) — Module deține modelul, regulile de domeniu, DTO-urile și serviciile comune.
- [Dezvoltare și validare](stare-curenta/dezvoltare-si-validare.md) — TR-D9a, pasul 6 (tăierea, 2026-10-07): motorul nu mai scrie, nu mai citește și nu mai ramifică pe registre și pe regim; oracolul registre → cub a ieșit din ModelCheck (CubDinRe…
- [Domeniu și operare](stare-curenta/domeniu-si-operare.md) — Un document are două laturi cu sens economic, Predator și Primitor, un antet și o colecție de linii.
- [Limite curente](stare-curenta/limite-curente.md) — Această pagină delimitează implementarea disponibilă.
- [Politici și fiscalitate](stare-curenta/politici-si-fiscalitate.md) — Aceste reguli descriu comportamentul implementat.
- [Termeni](stare-curenta/termeni.md) — Glosarul numește termenii paginilor de stare curentă și trimite la regula fiecăruia, fără s-o dubleze.
<!-- /gen:fisiere -->
