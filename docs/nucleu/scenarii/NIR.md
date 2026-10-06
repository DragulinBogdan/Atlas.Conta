# NIR — recepție manuală și conexul facturii

2026-09-24. **Recepția manuală și delta conexului implementate (098/099).**
Contractul nou: [tr-d8-nir-delta-contract.md](../tr-d8-nir-delta-contract.md),
inclusiv maparea bugetară aprobată de owner. Review-ul advers 1945 este
aplicat; urmează reverificarea independentă înainte de commit. Matricea de mai jos descrie întâi lotul
inițial, apoi completările verificate.
Contract inițial: TR-D7b T-D5,
T-D13, 090(h), 091 și 093 pentru Folosinta. Proveniență: regulă și politica
profilului. Cititorii comuni rămân TR-D8; 408 pe aviz rămâne TR-r4/B-r5.

## Recensământ

Recensământul comun `recensamant-asm-ldi-nir.sql`, rezultat în
`run-nucleu/tr-d7b/pas5-asm/recensamant.json`: 17.814 NIR, 34.289 linii,
toate autogenerate, zero recepții manuale, zero întârziate. 5.131 documente
cu produse multiple, 27 cu conturi multiple. Aceste cifre nu probează
recepția manuală; așteptările de mai jos sunt independente.

## Reguli și matrice numerică

S = 302 privat / 302.01.00 bugetar; F = 401 / 401.01.00 din politica
NIR/Stoc. D S: +q, lot, produs, gestiunea primitoare; C F: −q pe Furnizor,
produs, partidă proprie și partener după `Cont.UrmarestePartide` (pe contul F din seed: privat).
Fără fapt TVA, fără Carte=Fiscal. Conturile și analizele vin din politică.
Lot propriu pe frunză: prețul cules × q, rotunjit. Liniile istorice de bază
păstrează valoarea culeasă când lotul este propriu; lotul străin folosește
prețul lui, conform regulii existente NIR.PregatesteOperare (F5-D6).

| ID | Scenariu și așteptare | Rezultat așteptat |
|---|---|---|
| SC-NIR-01 | Manual 6 × 12,5 = 75: două postări, lot 6/75, datorie 75; prețul lotului în draft 0, declarație deterministă, dry-run fără efecte. | acceptat |
| SC-NIR-02 | Două loturi, 2/20 și 3/60: patru postări, o partidă 401 de 80; unitățile de stoc distincte. | acceptat |
| SC-NIR-03 | Storno în ianuarie al recepției 6/75: lot 0/0 și datorie 0, original intact; repetare refuzată. | acceptat/refuz stare |
| SC-NIR-04 | Recepție 2/30 în ianuarie; storno în februarie după închidere: ianuarie 2/30, februarie 0/0. | acceptat |
| SC-NIR-05 | Anulare/reoperare 2/20: zero efecte proprii după anulare, apoi aceleași postări. | acceptat |
| SC-NIR-06 | Corecție peste închidere a recepției 2/30: invers 2/30, lot nou 3/60, partidă pe document nou. | acceptat |
| SC-NIR-07 | Manual 2/50 → BCS 1/25: storno recepției refuzat, după inversarea consumului storno admis; lot 0/0. | refuz dependență, apoi acceptat |
| SC-NIR-08 | Manual 2/50 → PLT 20 împerecheată: datorie 30; storno plată refuzat până la desfacerea împerecherii. Desfacerea reface 50; storno plată, apoi NIR reface 0. | acceptat/refuz împerechere activă |
| SC-NIR-09 | FCT 4/100 cu NIR conex: înainte/după operare, anulare/reoperare și storno NIR, cubul FCT rămâne neschimbat; NIR are zero tranzacții proprii. | delta zero, recepție istorică dovedită |
| SC-NIR-10 | Autogenerat cu sursă care nu declară structural acoperirea: nu este absorbit; recepție proprie 1/10. Autogenerat fără sursă: idem. | acceptat |
| SC-NIR-11 | Data fizică ianuarie, înregistrare februarie: 1/10 numai în februarie; operarea în ianuarie închis refuză. | acceptat/refuz perioadă |
| SC-NIR-12 | Cantitate zero/negativă, lot absent, preț nul/negativ, gestiune greșită, produs/tip incompatibil, regulă/cont absent. | cod stabil pe declarație, zero efecte |
| SC-NIR-13 | 2 și 51 linii: număr constant de interogări, ≤16, determinism. | acceptat |
| SC-NIR-14 | Cantitate 0,001 × preț 0,001: valoare rotunjită 0, păstrarea cantității; 3 × 3,333333 = 10. | acceptat |
| SC-NIR-15 | Bugetar Folosinta 2/50 în gestiune reală → BTR 1/25 în altă gestiune; privat Marfuri 371 2/50. | acceptat |
| SC-NIR-16 | Toate ciclurile NIR: fără fapte TVA, matrice SC-X-14; contul terț are o partidă per document/cont/partener. | acceptat |
| SC-NIR-17 | Operand pur, 6 bucăți: lot propriu cu preț 12,5 și valoare veche 999 → 75; linie de bază fără preț, valoare 42 → 42; lot străin cu preț 3, indiferent de prețul cules 12,5 → 18. Ambele convenții de rotunjire. | acceptat |

## Limite și execuție

### Completarea 098/099 — așteptări înaintea codului

S = contul stocului; X = contrapartida cauzei din NIR-D3/D4; F = 401
privat / 401.01.00 bugetar. Așteptările fără TVA sunt comune profilurilor.
Rândurile noi sunt verificate pe ambele profiluri; execuțiile sunt raportate mai jos.

| ID | Scenariu | Așteptare independentă |
|---|---|---|
| SC-NIR-09b | FCT A 4/100; NIR A 3/75 și lot propriu B 1/10 | A 3/75, B 1/10, D clarificare 25, C F 100, C facturi nesosite 10; FCT intactă |
| SC-NIR-18 | FCT 4/100 → NIR 3/75 | delta C S 25/D X 25, cantitate −1; stoc 75, datorie F 100 |
| SC-NIR-19 | FCT 4/100 → NIR 4/100 + lot nou 1/10 | delta D S 10/C 408 (analiticul bugetar) 10; stoc total 110, F 100 |
| SC-NIR-20 | 4/100 → 0/0, InClarificare | stoc 0/0, clarificare 100, F 100; aceeași cantitate zero pe manual refuzată |
| SC-NIR-21 | Minus 25 pentru fiecare cauză permisă | stoc 75, D contul cauzei 25; Imputabila fără partener și Plus pe minus refuzate atomic |
| SC-NIR-22 | Lipsuri imputabile 10 și 15 la doi parteneri | două partide distincte pe același cont, solduri 10/15; angajat bugetar pe 428.01.02; lipsa analizei obligatorii refuzată în dry-run și operare |
| SC-NIR-23 | Schimbare lot A 4/100 → B 4/100; linie-sursă ștearsă sau duplicată | refuz atomic în fiecare caz; cubul FCT rămâne 4/100 |
| SC-NIR-24 | 4/100 → 3/75; schimbarea politicii; storno; corecție 2/50 | inversa deltei 25 pe contul vechi; delta nouă 50 pe contul nou; stoc final 2/50, F 100 |
| SC-NIR-25 | PeDrum: 4/100 → 3/75 → corecție cumul 4/100 | cont tranzit 25, apoi zero; lot 3/75, apoi 4/100; F rămâne 100 |
| SC-NIR-26 | Anulare/reoperare după schimbarea politicii conexului și PosteazaInCub | anularea șterge delta proprie existentă; nu atinge FCT; reoperarea în regimul activ recalculează o singură deltă |
| SC-NIR-27 | Două recepții candidate pentru aceeași sursă, inclusiv concurent | exact una activă; a doua refuzată fără număr/registre/cub noi |
| SC-NIR-28 | Corecție peste închidere: 4/100 → 3/75 în ianuarie → 2/50 în februarie | ianuarie 3/75; februarie 2/50; proveniența FCT păstrată fără Autogenerat |
| SC-NIR-29 | FCT 4/121 cu TVA capitalizat 21 → constatat 3 bucăți | stoc 90,75; delta valorică −30,25; zero fapte TVA noi pe NIR |
| SC-NIR-30 | Conex nemodificat, apoi politica eliminată; FCT de avans | primul are delta zero cu proveniență; avansul nu produce recepție absorbită |
| SC-NIR-31 | Fără obiect după tăiere: proba era a reconcilierii registre ↔ cub pe grupul cu deltă | scos (TR-D9a, pasul 6); diferența de 25 pe furnizor rămâne probată de rândurile de diferență ale catalogului |

Recensământ 098-r2: `recensamant-nir-delta.sql`, read-only pe Flax.Api și
Flax.TrD7b: fiecare cu 17.814 NIR/FCT, zero laturi diferite, zero diferențe
de multiset (lot, tip, cantitate, valoare), zero corecții. Rezultatele sunt
în `run-nucleu/tr-d8/nir-delta/recensamant*.json`; absența cazurilor în clonă
nu probează noul comportament.

### Execuțiile lotului inițial

Comanda selectivă:
`pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip NIR -Profil Ambele -Sufix .CodexBCS`.
Rezultat: **114 bugetar / 121 privat OK**, zero FAIL, exit 0, build fără
avertismente: `run-verificari/20260924-005954-434/rezultat.json`.
SC-NIR-13 măsoară **10/10 interogări** pentru 2/51 linii pe fiecare profil.
SC-NIR-12 pentru configurare și SC-NIR-17 sunt probe pure pe operand,
în ambele convenții; celelalte folosesc comenzile reale și cubul persistat.

Rularea inițială `20260924-005250-658` a expus ordinea greșită din scena
PLT: storno înaintea desfacerii împerecherii manuale. Proba a fost corectată
să aserteze refuzul existent, desfacerea, apoi storno; motorul nu a fost
relaxat. Rularea intermediară `20260924-005501-993` trece înaintea adăugării
probelor pure de configurare și valoare.

Recepția manuală nu rezolvă avizul pe 408 sau factura ulterioară
pe un lot recepționat înainte: TR-r4/B-r5. Absorbția conexului cere recepția efectiv postată de sursă și contractul
structural; simplul Autogenerat, DocumentSursa sau politica de azi nu ajung.
Import1C rămâne unealtă de migrare, nu gate.

Regresie integrală finală:
`pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexBCS`:
**2.293 bugetar / 3.342 privat OK**, zero FAIL, exit 0, build fără
avertismente: `run-verificari/20260924-010719-358/rezultat.json`.
Prima regresie integrală (`20260924-010044-906`) a găsit trei probe private
eșuate dintr-o premisă depășită: STR-CONFIG folosea NIR ca tip fără
declarant, îl opera și provoca apoi a doua operare din contextul vechi.
STR-CONFIG folosește acum DEC, încă fără declarant; verificările cubului
și reconcilierii FCT/NIR au rămas neschimbate și trec.
Nucleul nu a fost modificat de această felie; proba 178/178 rămâne cea din
`run-verificari/20260923-235301-386/rezultat.json`.
Raport: `run-nucleu/tr-d7b/pas5-nir/raport.md`.

SC-NIR-30/avans: 4091 privat / 409.01.01 bugetar, 100 fără TVA,
produce D avans 100/C furnizor 100, fără NIR/lot/cantitate. Seed-ul bugetar
corectează încadrarea 409.01.01 din clasa de combustibil în clasa fără stoc S,
conform 099(d); nu se reclasifică istoricul deja postat. Totalul de stins
este datoria 100, nu și creanța avansului (102).

SC-NIR-37: aceeași factură de avans verifică panoul împerecherilor, lista
`DocumenteCuRest` și `ImperechereService` în patru stadii: nestins
100/100/0 (total/rămas/asignat), după PLT 40 100/60/40, după PLT 60
100/0/100, după desfacerea stingerii de 60 100/60/40. Pe privat verifică
în fiecare stadiu că soldul 4091 al furnizorului rămâne +100, adică
stingerea datoriei 401 nu consumă creanța avansului (review-ul Codex R2).

### Execuție 098/099 — 2026-09-24

Scenarii NIR: **201 bugetar / 206 privat OK**, zero FAIL,
`run-verificari/20260924-190418-862/rezultat.json`. Acoperă matricea nouă,
refuzurile de analiză ale imputării bugetare și avansul fără stoc. Pentru
SC-NIR-29 privat, proba schimbă temporar regimul TVA în Capitalizat și îl
restaurează; istoricul este apoi verificat ca fapte persistate.

HTTP pe baza privată `.CodexBCS`: salvare 3/75/PeDrum și refuz 422 la
ștergerea liniei-sursă; browser: schimbare în Imputabilă, alegerea
partenerului, salvare, operare. Rezultat independent în cub: C 302/D 461
25, Q −1 pe lot. Ecranul politicilor afișează 30 de rânduri private,
cu cele șase cauze și contul pentru personal. Probe de acces focalizate:
15 PASS, zero FAIL (401/403/404 și CRUD Configurator pentru noua politică).
Datele probei au fost eliminate; zero documente/produse active și zero
postări proprii. Scriptul general `ProbeHttp/refuzuri.ps1` nu a executat
matricea pe această bază fiindcă îi lipsește fixture-ul ITV; nu este
raportat ca trecut. Loguri: `run-verificari/nir-http.log`, `nir-acces.log`.

Cititorii generali și snapshot-urile rămân de portat la TR-D8. La corecția
unui cumul, intervalul până la operarea draftului nou poate lăsa registrele
vechi negative; grupul este incomplet în reconciliere, cubul păstrează
recepția FCT și gardianul propriu de stoc. Limita nu este ascunsă prin (h).

Regresie integrală după ultima modificare de cod (centralizarea codurilor
NIR în CoduriRefuz): **2.810 bugetar / 3.899 privat OK, zero FAIL**,
exit 0, build fără avertismente/erori, `run-verificari/20260924-192639-359`.
Comandă: `pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1
-Suita Integral -Profil Ambele -Sufix .CodexBCS` (o singură linie).
Sursa nu a fost modificată în timpul rulării. Doar CSV-ul planului, fără
nicio diferență de conținut, a revenit la terminatorii de linie din HEAD.
Probele anterioare ale lotului inițial rămân istoric; acestea sunt cifrele
feliei delta, împreună cu corecturile TR-D8/DEC. Predare fără commit pentru
review-ul advers solicitat după NIR.

Metadata/OpenAPI/tipuri TS: regenerarea repetată păstrează hash-urile
SHA-256, `run-verificari/nir-drift.log`. Clientul compilează; singurul
avertisment rămas este dimensiunea bundle-ului Vite.

## Completări după review-ul 1945

| Scenariu | Premisă | Așteptare |
|---|---|---|
| SC-NIR-32 | NIR conex 4/100 operat, corecție la 3/75 | corecția păstrează sursa recepției fără Autogenerat, numai delta 25, sold cub 3/75 (completarea prin migrație a ieșit, 102c) |
| SC-NIR-33 | NIR cu 1, respectiv 12 linii; se modifică numai cantitățile | gardianul editării: zero citiri; operare: o singură citire a postărilor recepției-sursă |
| SC-NIR-34 | contul stocului cere CodFunctional după FCT 4/100; NIR 3/75 are analiza nouă | stocul păstrează analiza istorică, doar diferența se validează în politica actuală; delta 25 acceptată |
| SC-NIR-35 | imputat rămas la PeDrum sau la Imputabila cu delta zero | imputat golit pe server, fără partidă de imputare |
| SC-NIR-36/FCT | FCT serviciu 100 bugetar pe furnizor 408.00.00 | D628.00.00 100 / C408.00.00 100 și partida furnizorului −100 |

SC-NIR-23 asertează NIR_DELTA_STRUCTURA pe ușa declarației pentru fiecare
contraexemplu; ștergerea ultimei linii primește pe ușa entității refuzul
anterior al bazei („Documentul nu are nicio linie”). SC-NIR-22/analiza cere
explicit contul 428.01.02 și lipsa Codului funcțional în dry-run, respectiv
mesajul analizei și atomicitatea la operare.

Limita 098-r3 include storno/anulare simplă după consum, nu numai corecția:
registrul lotului poate rămâne negativ și poate bloca operații ulterioare
care încă îl folosesc; cubul păstrează recepția FCT. Nu se adaugă adaptări
pentru regimul dual. Garda registrelor cade la TR-D9.

Verificare după corecturile 1945: selectiv NIR **227 bugetar / 224 privat
OK**, zero FAIL (`run-verificari/20260924-220839-873`); Integral **2.836 /
3.917 OK**, zero FAIL, exit 0, build fără avertismente/erori
(`run-verificari/20260924-220953-307`). Codul a rămas înghețat în timpul
regresiei integrale. Review-ul este aplicat, în așteptarea reverificării
independente înainte de commit.

Metadata/OpenAPI/tipuri TS: regenerare repetată cu hash identic
(`nir-review-drift.log`). Clientul compilează. HTTP: 3 PASS pentru imputat
valid/PeDrum/delta zero (`nir-review-http.log`). În browser, schimbarea
Imputabila → PeDrum → Imputabila lasă imputatul gol înainte de salvare.
Fixture-ul a fost curățat prin API, hosturile temporare oprite.

## Extinderea partidelor bugetare — 2026-09-25

Decizia 100 activează urmărirea pe 401.01.00, 404.01.00 și 411.01.01,
fără rol comercial SAF-T. Așteptările de partidă (rest, transfer, storno,
corecție) ale ciclurilor comune se aplică acum ambelor profiluri.
Mențiunile anterioare „numai privat” descriu acoperirea de la data probării
inițiale; fiscalul, DSC și contul 419 rămân specifice profilului privat.
Probele TR-D8 SC-CIT-41…45 verifică separat politica și istoricul.
