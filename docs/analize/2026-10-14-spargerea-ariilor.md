# Spargerea ariilor stării curente

Tip: analiza
Data: 2026-10-14
Alimentează: decizia următoare (121), care amendează 118 (l)

Pregătirea pentru o sesiune pe context nou. Nu e decizie: lista ariilor,
numele lor și ordinea pașilor le tranșează owner-ul.

## 1. Întrebarea

Pilotul stratului de scop (kit §3.9, PR-ul care aduce
[scop.md](../scop.md) și [catalogul](../functionalitati.md)) a arătat că
perimetrul feliei (kit §3.5.6) nu semnalează nimic cu ariile de acum.
Contractul [TR-D9b-CV](../nucleu/tr-d9b-corectii-valorice-contract.md)
își declară toate cele 4 arii, deci nicio atingere nu iese din perimetru.
Întrebarea e ce arii fac perimetrul util, fără ca reclasificarea să coste
mai mult decât aduce.

## 2. Ce leagă azi cele 4 arii

- **Regula.** [118 (l)](../decizii/118-guvernanta-documentatiei.md)
  numește cele 4 arii în textul ei. Spargerea e deci o decizie nouă cu
  `Amendează: 118 (l)`, nu doar o schimbare de config.
- **Config.** `arii` din `docs/docskit.json`. Gardianul `arii` (la
  `eroare`) cere fiecărui rând din registru o arie din listă.
- **Registrul.** 383 de rânduri. Pe toate rândurile: domeniu 127, politici
  108, api 87, dezvoltare 60. Pe cele deschise: politici 91, domeniu 83,
  api 68, dezvoltare 42. Precedentul popularii (118-r9): 223 de rânduri
  luate din decizia-sursă sau din prefixul contractului, 153 clasificate
  după titlu, verificate de owner prin sondaj.
- **Deciziile.** 88 de decizii au `Arii:` în antet: 43 cu o singură arie, 22
  cu două, 23 cu trei sau mai multe. Pe pagini: domeniu 52, dezvoltare 34,
  `limite-curente` 29, api 26, politici 22, `README` 6. `limite-curente` și
  `README` sunt transversale (118 l), nu arii. Blocul `gen:decizii` al
  fiecărei pagini se generează din aceste antete.
- **Catalogul.** 64 de rânduri: domeniu 42, politici 16, api 5,
  dezvoltare 1. Aria domeniului acoperă două treimi din funcționalități.
- **Linkurile.** Spre paginile ariilor sunt 138 de linkuri în Markdown
  (65 cu ancoră): domeniu 73 (51 cu ancoră, în 20 de fișiere), dezvoltare
  38, politici 18, api 9. Codul nu le citează.

## 3. Mărimea și forma paginilor

| Pagina | Linii | Secțiunile mari |
|---|---|---|
| `domeniu-si-operare` | 1.888 | Cubul persistat 597, Registre și ciclu de viață 415, Nucleul pur și declarația fluxului 205, Locurile regulilor în cod 191, Împerechere și stingere 121, Imobilizări 103 |
| `dezvoltare-si-validare` | 1.323 | preambul de 447 de linii înaintea primei secțiuni, Contracte generate și build 263, Model și persistență 187, Verificări 167 |
| `api-si-client` | 642 | Salvare și comenzi 162, Listele XAF 75, Formulare React 65 |
| `politici-si-fiscalitate` | 596 | TVA la operare 147, SAF-T 129 |

Două probleme de formă se suprapun cu spargerea, fără să fie aceeași
problemă:

- **Secțiuni numite după felii**, contra kit §3.3.1 (pe responsabilități,
  nu pe cronologie). Exemple: „Declarația fluxului per tip (TR-D6b, felia
  30)”, „FCL și DSC pe cub (T-D4, felia 32, pasul 2)”, „NIR delta și
  citirile comune (098/099)”.
- **Preambulul paginii de dezvoltare** e stare de felii, cu cifre de rulare
  (kit §3.3.3: fără succesiuni de implementare, fără cifre de mediu).

În plus, kit §3.3.7 cere ca acoperirea funcțională să fie catalogul, nu
proză în `README`, și ca limitele să nu repete funcționalitățile deschise
din catalog. Azi le repetă (salariile, e-Factura, valuta…).

## 4. Variantele

Testul fiecărei variante e câte arii ar declara TR-D9b-CV. Felia atinge
stocul, cubul, citirile, culegerea XAF, verificarea și refuzul SAF-T.

**A — fină, 16 arii, aliniată cu familiile catalogului.**

| Arie | Responsabilitatea | Vine din | Familiile catalogului |
|---|---|---|---|
| `documente` | modelul comun, ciclul de viață (culegere, operare, anulare, storno, corecție, retragere), data înregistrării, documentele generate | domeniu | REGIM |
| `motor` | declarația fluxului, contarea și dimensiunile, coordonatele, cubul persistat, scrierea serială, precizia, nucleul pur | domeniu | X |
| `stoc` | loturi, FIFO, recepția și diferențele ei, transformarea, inventarul, folosința, corecția valorică | domeniu | FCT, NIR, BCS, BTR, DSC, LDI, ASM, RLF, RDC, DVI, CVL |
| `partide` | partidele, stingerea, împerecherea, nominalizarea, decontul, deschiderea | domeniu | PLT, INC, VIR, PRT, NTC, DEC, DES |
| `imobilizari` | fișa, PIF/AMO/CAS, amortizarea și deductibilitatea | domeniu + politici | IMO |
| `perioade` | perioada ca lanț, închiderea, soldurile materializate | domeniu + politici | PER |
| `politici` | configurarea profilului, implicitele, proveniența, explicarea configurației | politici | CFG |
| `tva` | TVA la operare, implicitele TVA, închiderea lunară, D300/D394, citirea fiscală și depunerea | politici + api | ITV, CIT-06 |
| `saft` | SAF-T L și S | politici | SAFT |
| `raportare` | proiecțiile contabile și de stoc, soldul pe parteneri, explicația deciziei | politici + api | CIT |
| `nomenclatoare` | societatea, partenerii, produsele, ANAF | politici | ANAF |
| `api` | ușile HTTP, salvare și comenzi, refuzurile, OData, auditul, contractele generate | api | SEC |
| `ecrane` | XAF Blazor (liste, grile, regim) și React (formulare, selectoare, cache) | api | — |
| `dezvoltare` | organizarea sursei, modelul și persistența, migrațiile, codegen, build | dezvoltare | — |
| `verificare` | ModelCheck, scenariile, probele, documentația și docskit | dezvoltare | — |
| `import` | Import1C și reconcilierea cu legacy | dezvoltare | IMP |

TR-D9b-CV ar declara 6 din 16: `stoc`, `motor`, `raportare`, `ecrane`,
`verificare`, `saft`. Un rând nou de felie care atinge `partide` sau `tva`
ar fi un semnal.

**B — medie, 9 arii.** `documente-si-motor`, `stoc`, `partide`,
`imobilizari`, `fiscal` (TVA, SAF-T, perioade), `politici-si-nomenclatoare`,
`raportare`, `ui-si-api`, `dezvoltare-si-validare`. TR-D9b-CV ar declara
6 din 9, deci perimetrul rămâne slab pe feliile de motor.

**C — doar cele două pagini supraîncărcate.** `domeniu-si-operare` se sparge
în `documente`, `motor`, `stoc`, `partide`, `imobilizari`, `perioade`;
restul rămâne. Costul e cel mai mic, dar `politici-si-fiscalitate` rămâne
amestecul TVA + SAF-T + configurare, pe care îl ating jumătate din felii.

**Recomandare: A.** Perimetrul devine măsurabil, iar coloana `Arie` a
catalogului coincide cu familia funcționalității. Costul în plus față de C
e în registru, nu în pagini, și e în mare parte mecanic (secțiunea 5).

## 5. Mecanica, în ordinea pașilor

1. **Decizia** (121, `Amendează: 118 (l)`): lista ariilor, numele, regula
   de clasificare. 118 (l) primește marcajul `[amendat de 121]`.
2. **Mutarea paginilor, numai mutare.** Secțiunile trec întregi în paginile
   noi, cu heading-urile neschimbate, ca ancorele să rămână aceleași.
   Rescrierea pe responsabilități (secțiunile numite după felii, preambulul
   dezvoltării) e un pas separat, ca mutarea să nu ascundă schimbări de
   conținut (kit §6.6). `docskit muta` mută fișiere întregi, nu secțiuni,
   deci mutarea se face cu un script care ia harta secțiune → pagină.
3. **Linkurile.** Aceeași hartă rescrie cele 138 de linkuri: pagina veche și
   ancora dau pagina nouă. `docskit check` (gardianul `linkuri`) e proba.
4. **`Arii:` pe decizii, derivat din citări.** O decizie aparține paginilor
   care o citează (`(42b)`). După mutare, docskit știe ce pagină citează ce
   decizie; propunerea de `Arii:` se generează din asta, iar owner-ul o
   verifică prin sondaj. Rămân de mână doar deciziile necitate (32 n-au azi
   `Arii`).
5. **Registrul.** Fiecare rând primește aria din `Arii:` a deciziei-sursă,
   când aceea are o singură arie; altfel se clasifică după titlu, ca la
   118-r9, cu sondajul owner-ului. Gardianul `arii` trece abia după acest
   pas, deci pașii 4–6 intră în același PR cu configul.
6. **Catalogul și scopul.** Coloana `Arie` din catalog, după tabelul din
   varianta A; tabelul `## Structura` din [scop.md](../scop.md) cu ariile
   noi; `Arii:` din contractul TR-D9b-CV.
7. **Config și generatul.** `arii` în `docs/docskit.json`; `paginiFaraBloc`
   neschimbat; `docskit gen`, `check`, `metrics --check` verzi; harta din
   `CLAUDE.md` și `stare-curenta/README.md`.

Separat, după aceea: rescrierea secțiunilor numite după felii, preambulul
paginii de dezvoltare și curățarea lui `limite-curente` de funcționalitățile
deschise din catalog (kit §3.3.7).

**Regula de oprire.** Dacă după pasul 5 rămân peste 40 de rânduri de
registru fără arie sigură, se oprește și se pune întrebarea owner-ului
înainte de sondaj. Dacă la pasul 2 o secțiune nu are o singură arie
evidentă, nu se taie secțiunea: rămâne întreagă în aria cu cele mai multe
citări, iar tăierea ei intră în pasul de rescriere.

## 6. De tranșat de owner

1. Varianta (A, B sau C) și numele ariilor.
2. Regimul pe stare: `documente` (regula) sau `ecrane` (afișarea în XAF).
   Propunerea: regula în `documente`, afișarea în `ecrane`.
3. Amortizarea: `imobilizari` (propus) sau `politici`.
4. Explicația deciziei: `raportare` (propus) sau `motor`.
5. Dacă `limite-curente` rămâne o singură pagină transversală sau se sparge
   odată cu ariile. Propunerea: rămâne transversală (118 l), curățată după
   §3.3.7.
