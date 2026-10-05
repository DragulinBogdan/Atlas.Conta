# Decizia 109 — review advers Codex

Data: 2026-10-04. Țintă: `107-r3-drift-tva`, `52759f5`, peste `6cd322c`.
Contract: [decizia 109](../decizii/109-taxa-nemarcata-pe-document-scrisa-pe-linie.md), regula aprobată (a)–(g).

**Verdict inițial: review neînchis — o observație P2, reprodusă.**
Regula aprobată nu este contestată. La cererea owner-ului, Codex a aplicat
ulterior corectura descrisă mai jos.

**Stare 2026-10-05: review închis.** Corectura a fost reverificată
independent de Claude (ultima secțiune).

## 109-R1 — P2: atribuirea tardivă a poziției schimbă taxa și totalul după salvarea draftului

`TvaService.RepartizeazaTaxa` sortează după `Pozitie`, apoi `ID`
(`Module/Motor/TvaService.cs:54–56`). Linia nouă are încă poziția 0 când
`CulegereDocument.Normalizeaza` calculează taxa. Poziția definitivă se
atribuie ulterior, în `BackOfficeEFCoreDbContext.SaveChanges`, prin
`AtribuiePozitii` (`BusinessObjects/BackOfficeDbContext.cs:856–859, 874–881`).
O linie adăugată la un document salvat este astfel prima la calcul și ultima
în ordinea persistată. Următoarea normalizare/operare repartizează în altă ordine.

Contraexemplu determinist, privat, prin ObjectSpace și L3, apoi comanda reală de operare:

1. FCL cu două linii salvate, fiecare net 10,03, N21, poziții 1 și 2.
2. Taxa liniei 2 este culeasă 2,15 (`TvaCules=true`). Se adaugă o a treia
   linie net 10,03, N21, fără poziție atribuită manual.
3. Se normalizează documentul și se salvează. Citit într-un ObjectSpace nou,
   în ordinea pozițiilor persistate: taxele sunt **2,11 / 2,15 / 2,11**, total **36,46**.
4. Renormalizarea draftului fără editări produce `ModifiedObjects` nenul.
5. Operarea fără alte editări produce **2,11 / 2,15 / 2,10**, total **36,45**.

Cu taxa marcată, schimbarea ordinii nu mută doar un ban între linii: partea
repartizată liniei marcate este ignorată, deci se schimbă și suma efectivă.
Sunt încălcate 109(a), egalitatea draft–operat din 109(b) și idempotența
109(g). Fără marcaj rămâne problema repartizării pe linii și a modificării
draftului la renormalizare, chiar dacă totalul este conservat.

SC-FCL-11 nu detectează cazul: helperul `Vinde` atribuie manual pozițiile
(`ScenariiVanzare.cs:29`), iar proba normalizează din nou după salvare
(`ScenariiVanzare.cs:141`).

Corectura trebuie să asigure aceeași ordine pentru calculul din culegere,
prima salvare și operare. Nu este suficientă încă o normalizare în probă.
Regresia trebuie să includă o linie nouă cu poziția implicită 0, adăugată la
linii persistate, cu și fără taxă culeasă, plus verificarea din ObjectSpace
nou imediat după prima salvare și verificarea absenței modificărilor la
renormalizare. Pentru adaptorul XAF, se repetă adăugarea din ecran.

## Verificări și dovezi

Comenzi executate din rădăcina repo-ului, serial, prin procese PowerShell
detașate cu fereastra ascunsă:

```powershell
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip FCL,FCT,DEC,DVI,RDC,RLF -Profil Ambele -Sufix .Codex109 -PregatesteBaze
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip FCL -Profil Privat -Sufix .Codex109
```

- Prima comandă, surse nemodificate: **248 OK bugetar / 969 OK privat**, zero
  FAIL, exit 0. `run-verificari/20261004-233925-237/rezultat.json`.
- A doua, cu proba adversă temporară: **3 FAIL**, toate pentru 109-R1
  (repartizare după prima salvare, idempotență, total draft–operat), exit 1.
  `run-verificari/20261004-234127-066/rezultat.json` și `scenarii-privat.log`.
  Jurnalul tipărește ambele distribuții și totaluri. Purja confirmă zero
  postări rămase. Patch reproductibil: `run-verificari/codex109-probe.patch`.
- Proba temporară a fost retrasă din `ScenariiVanzare.cs`; restaurarea a fost
  verificată cu `git diff --exit-code`. Prima repetare FCL după restaurare
  (`20261004-234220-020`) a reutilizat binarul probei: copierea păstrase data
  veche a sursei. Cele 3 FAIL identice din acea rulare nu sunt o validare a
  sursei restaurate. Data fișierului a fost actualizată pentru recompilare.
- Recompilare și FCL privat pe sursa restaurată:
  `run-verificari/20261004-234403-698/rezultat.json`, **249 OK, zero FAIL,
  exit 0**. Hash-ul DLL este identic cu cel al primei rulări nemodificate.
- Build-ul de bază reușește, cu trei avertismente de nullabilitate în
  `Cub/Explicatie.cs` și `Cub/ReceptiiConexe.cs`, fișiere neatinse de felie.

Inspecția surselor a acoperit cele cinci atribuiri ale conectorului,
gruparea fiscală a FCL/FCT/DEC/DVI/RDC/RLF, `Fiscal.Taxa/Impozitul`, invariantul
nou, culegerea și adaptorul de reîncărcare. Singurul apel de producție la
`Contractare.Contracteaza` găsit în `nou/Atlas.Conta.BackOffice` este în
`Cub/Materializare.cs:314`. Nu am identificat o a doua observație confirmată.

## Limite

Nu am reluat integrala, importul 1C sau proba din browser. Rezultatele lor
din predare rămân dovezile autorului. Proba proprie confirmă defectul pe L3,
persistență și operare, nu certifică interacțiunea vizuală. Cazul surorii cu
modificări nesalvate în alt tab, retururile cu repartizare nerotundă și tipul
fără politică TVA nu au primit probe noi în acest review. Limita declarată a
invariantului (nu detectează taxa fără nicio postare) rămâne valabilă.
Nu am schimbat starea restanțelor 107-r7/109-r3 și nu am făcut commit.

## Corectura 109-R1 aplicată de Codex

La cererea owner-ului, ordinea liniilor fără poziție este comună calculului
și salvării: după liniile numerotate, în ordinea din `Document.Detalii`.
`BackOfficeEFCoreDbContext.InOrdineaPozitiilor` reutilizează selecția
`DetaliiFaraPozitie` pe care o consumă atribuirea pozițiilor. Acea selecție
folosește ordinea colecției documentului; ordinea ChangeTracker singură
nu păstrează culegerea după o salvare intermediară.

Pozițiile existente rămân neschimbate. Formula nu atribuie poziții și nu
citește un maxim din bază pentru a rezerva numere. `SaveChanges` și
`SaveChangesAsync` păstrează citirea maximului și atribuirea sub blocajul
scrierii, inclusiv revenirea la poziția 0 după eroare.

SC-FCL-13/14 acoperă câte trei variante: document nou, o linie salvată plus
două noi, două linii salvate plus una nouă. Așteptările sunt 2,11/2,11/2,10
și 36,41 fără marcaj, respectiv 2,11/2,15/2,10 și 36,45 cu taxa celei de-a
doua linii culeasă. Se verifică înaintea salvării, imediat după salvare în
ObjectSpace nou, la renormalizare (zero obiecte modificate), apoi prin
operare, matricea cubului și soldul partidei.

Comenzi suplimentare:

```powershell
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip FCL -Profil Privat -Sufix .Codex109
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .Codex109
git diff --check
```

FCL privat cu corectura: **315 OK**, exit 0, zero FAIL,
`run-verificari/20261004-235207-092/`. Control negativ cu ordonarea veche
reintrodusă numai în `RepartizeazaTaxa`: **12 FAIL**, exit 1,
`run-verificari/20261004-235310-585/`. Mutantul a fost retras înaintea
integralei. Variantele intermediare bazate exclusiv pe ordinea ChangeTracker
au fost respinse de probe (10 FAIL în `20261004-234923-578` și
`20261004-235044-295`); așteptările numerice au fost păstrate.

Corectura nu atinge controllerele XAF. Browserul nu a fost reluat; validarea
proprie acoperă L3, salvarea EF, operarea și cubul. Reverificarea independentă
a owner-ului/Claude, inclusiv adăugarea din ecran cerută în review, rămâne
înaintea commit-ului.

**Rezultat final, 2026-10-05:** integrala pe ambele profiluri a trecut,
**3.459 OK bugetar / 4.771 OK privat**, zero FAIL, exit 0.
`run-verificari/20261004-235420-458/rezultat.json`. Build reușit; mutantul
nu este în surse sau în binarul final. `git diff --check` trece.
109-R1 este corectată și verificată de Codex; predarea nu substituie
reverificarea independentă înainte de commit.

## Reverificarea independentă (Claude, 2026-10-05)

Corectura a fost citită și păstrată așa cum a scris-o Codex; nu am schimbat
cod. Verificări proprii, pe arborele de lucru peste `52759f5`:

- **Binarul.** Build-ul nu e determinist (două recompilări integrale dau
  hash-uri diferite), deci hash-ul nu leagă binarul de surse. Integrala a fost
  rulată din nou după `dotnet build --no-incremental`: **3.459 OK bugetar /
  4.771 OK privat**, zero FAIL, exit 0,
  `run-verificari/20261005-004505-569/`.
- **HTTP, prin WebApi (spațiu de obiecte securizat).** Draft FCL cu două
  linii salvate, a treia adăugată prin PUT, fără și cu taxa liniei 2 culeasă
  2,15: 2,11 / 2,11 / 2,10 și 36,41, respectiv 2,11 / 2,15 / 2,10 și 36,45,
  în răspunsul PUT, la prima citire, la PUT fără editări și după operare.
  `run-verificari/r3-ui-r1-http.py`, `r3-ui/r1-http.log`.
- **Browser, pe host viu.** Aceleași două drafturi, a treia linie adăugată
  din grila documentului (tabul liniei are spațiul lui de obiecte). Linia
  nouă arată 2,10 înaintea salvării; salvarea rescrie linia soră 2 de la 2,10
  la 2,11 în varianta nemarcată; tabul documentului arată 2,11 / 2,11 / 2,10
  și 36,41, respectiv 2,11 / 2,15 / 2,10 și 36,45. Operarea trece fără
  conflict de versiune: FCL-6 și FCL-7, cu aceleași valori pe totalul de
  stins și în cub, per linie. `run-verificari/r3-ui/r1-*`.
- **Ianuarie pe Flax** cu binarul corectat, baza `.Flax.R3f` recreată:
  contractele 1–4 identice pe conținut cu `r3-ian-final2/`, INV-CUB verde
  după deschidere și pe baza integrală, durată neschimbată (circa 23 de
  minute, deci citirea ChangeTracker-ului la fiecare repartizare nu se vede).
  Contractul 5: 7 partide fără explicație (107-r7).
  `run-verificari/r3-ian-r1/`.

Controlul negativ al lui Codex (`20261004-235310-585`) a fost citit, nu
repetat: cele 12 FAIL sunt toate pe SC-FCL-13/14, variantele cu 1 și 2 linii
salvate.

**Observație, nu defect.** Baza regulii 109 (a) e suma tuturor liniilor
fiscale ale cotei, iar partea repartizată unei linii marcate se înlocuiește cu
taxa ei. Suma taxelor nemarcate depinde deci de poziția liniei marcate: cu
trei linii de 10,03 la 21%, marcajul pe linia 1 sau 2 lasă nemarcatelor 2,11 +
2,10, iar marcajul pe linia 3 le lasă 2,11 + 2,11. După corectură ordinea e
stabilă, deci rezultatul e determinist și același pe draft și la operare.
Independența de poziție ar cere altă bază (numai liniile nemarcate), adică o
schimbare a regulii aprobate; rămâne la decizia owner-ului.

Neacoperit în continuare: adăugarea din ecran pe FCT, DEC, DVI, RDC și RLF
(același adaptor), sora cu modificări nesalvate în alt tab, retururile cu
repartizare nerotundă, tipul fără politică de TVA.
