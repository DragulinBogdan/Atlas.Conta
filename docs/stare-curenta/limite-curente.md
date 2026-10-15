# Limite curente

**Actualizat: 2026-10-15.** [Index](README.md)

Ce nu face încă o funcționalitate livrată, în forma în care o vede
utilizatorul, pe ariile catalogului. Ce nu e livrat deloc e un rând
`deschisă` în [catalogul funcționalităților](../functionalitati.md) și nu se
repetă aici. Limitele mecanismelor, fără efect direct pentru utilizator, stau
în pagina ariei lor, la `## Limitele implementării`. (121)

## Documente

- Regimul pe stare nu promite refuzurile care depind de valori (sold de
  stoc, poziție fără fișă) și, la stornare și corecție, pe cele care depind
  de data cerută: perioada ei, luna la imobilizări cât luna documentului e
  deschisă, nominalizările stinse ulterior. La stornarea unui document cu
  legături vii în perioade închise, regimul nu numără nominalizarea
  partenerului pe aceeași partidă când ea nu vine din transferul legăturii,
  inclusiv la legătura manuală care acoperă exact o nominalizare făcută la
  operare; refuzul `PARTIDA_CU_DEPENDENTI` rămas după desfacerea legăturilor
  apare abia la comandă. (106i, 106k, 106-r7)
- Un tip fără politică pe profil e refuzat (`TIP_FARA_DECLARATIE`) la dry-run
  și la operare, dar regimul pe stare nu îl arată: butonul de operare rămâne
  activ. (D9-D5)
- La corecția unui document operat, motivul decide efectul fiscal, nu
  contarea. Reclasificarea pe 1174 a erorilor semnificative din exerciții
  anterioare se face cu notă contabilă manuală: motorul nu judecă
  semnificația. (F27-r1)
- Corecția are refuzurile stornării: un document cu împerecheri în perioada
  deschisă se corectează după desfacerea lor (cele dintr-o perioadă închisă
  se inversează singure), iar unul cu conex operat sau cu latură pereche
  operată e refuzat. (F27-D6, F27-D8)
- Documentele generate (AMO, ITV, DSC și NIR autogenerat) moștenesc data
  înregistrării sursei sau ultima zi a lunii; editarea ei pe ele nu e
  decisă. Plata generată din factura de intrare ia data culeasă. (F27-D4,
  F27-D5, F27-r9)

## Lucrul concurent

- Comenzile care scriu se execută una câte una per bază: doi operatori nu
  scriu simultan, iar al doilea așteaptă comanda primului, oricât de străine
  ar fi documentele lor. Așteaptă și salvarea unui draft care adaugă linii
  fără poziție. Peste timpul de comandă al conexiunii (30 s implicit),
  așteptarea iese ca refuz `SCRIERE_OCUPATA`; pragul nu se configurează
  separat. (X-D6, X-r1)

## Stoc

- O ieșire retroactivă nu reevaluează ieșirile deja operate, iar un retur la
  furnizor poate lăsa valoare pe un lot rămas fără cantitate. (75a, D9-D3)
- Pe un lot cu corecție valorică activă, returul la furnizor și returul de la
  client sunt refuzate (`CV_DEPENDENTI`) până la stornarea corecției; lotul
  care a trecut prin retur, transfer, asamblare, inventar sau deschidere nu
  se corectează (F-CVL-07). (CV-D10)
- O asamblare culeasă înaintea altei ieșiri din același lot poate fi refuzată
  la operare cu `ASAMBLARE_NEBALANSATA`, până la redistribuire. Distribuirile
  care nu se pot reprezenta exact la precizia prețului sunt refuzate, nu
  forțate prin prețuri sau valori. (75-r4, 76c, D9-D3)
- Pe draft, valoarea liniei unei ieșiri evaluate din sold e estimarea
  `cantitate × preț de intrare`, nu promisiune; dry-run-ul nu întoarce
  valoarea decisă. Ea apare pe linie după operare și în explicația
  tranzacției. (D9-D3, D9-D12)
- Pe profilul bugetar, ieșirile spre Gratuit și Custodie nu au flux:
  Custodie e refuzată la inventar. Folosința păstrează gestiunea reală.
  (093, LDI-B3, TR-r7)
- Recepția unui NIR nu completează automat descărcarea de gestiune a
  facturilor de ieșire cu acoperire parțială; descărcarea suplimentară se
  generează din factura de ieșire. (37g, 38d)
- NIR-ul fără factură se contează la net, fără TVA. (TR-r4, B-r5)
- Declarația vamală nu are rest: taxa din vamă se plătește către biroul
  vamal fără stingere pe declarație. Anularea unei facturi legate de o
  declarație operată nu e refuzată; starea facturii se arată. (86-r2, 86-r11)

## Partide

- Soldurile de pornire nu au ecran sau rută HTTP: deschiderea rulează prin
  unelte și import. Soldurile nedetaliate nu se împart pe mai multe analize
  în aceeași cheie, iar stingerea unei partide inițiale în valută e refuzată.
  (094, X-D2)
- Compensarea unui retur se face prin notă contabilă. (46f, 76g)
- Retururile de la client nu apar în lista documentelor cu rest: creanța lor
  e negativă după operare. Împerecherea unui retur se face din document, în
  XAF. (F27-D7)

## Imobilizări

- Stornoul unei puneri în funcțiune, amortizări sau ieșiri (PIF, AMO, CAS) se
  face numai în luna documentului, și la corecție. (087g)
- Anularea unui eveniment din luna unei amortizări operate e refuzată, deși
  rândul lunar nu depinde de el. Două generări simultane ale aceleiași luni
  lasă două drafturi care se blochează reciproc. (F26-r18, F26-r22)
- Amortizarea se generează din comanda lunară, fără acțiune în XAF. Brutul
  fiscal zero pe linia de punere în funcțiune nu se poate exprima. (F26-r7,
  F26-r17)
- Eligibilitatea metodei fiscale pe categorie nu e verificată, ci doar
  documentată; patru poziții-părinte din catalogul HG 2139/2004 n-au bandă de
  durată. (F26-r14, F26-r15)

## Perioade

- Închiderea lunii nu semnalează documentele neimportate din 1C:
  reconcilierea cu sursa externă e a conectorului. (F27-D2, F27-r6)
- Constatarea de rest scadent privește numai facturile de intrare și de
  ieșire, singurele cu scadență; o plată, o încasare, un decont sau un retur
  cu rest nu apar. (F27-D2)
- Constatările de închidere listează cel mult 200 de rânduri per fel; restul
  intră într-un rând de rezumat, iar acceptarea lui acceptă în bloc toate
  rândurile nelistate. (F27-D2)
- Cât o lună are un blocant structural (de exemplu luna precedentă
  deschisă), ecranul arată doar blocanta, nu și constatările de conținut.
  (F27-D2)

## Politici

- Explicația configurației se face pe o linie ipotetică, nu pe un document;
  dreptul de a o citi se judecă pe tip, iar un cod de tip inexistent dă 400
  pentru orice rol. (81k, 81-r3)
- Raportul de profil nu semnalează un rând de seed lipsă, iar recrearea unui
  rând de seed șters e manuală. Marcajul istoric nu distinge toate
  intervențiile manuale anterioare, iar alinierea seed-ului atinge și
  `Cont.DimensiuniObligatorii` pe rândurile marcate. (83-r1, 84-r3, 84-r6,
  84-r8)
- Rolul `Configurator` vine cu release-ul: permisiunile lui se reaplică la
  fiecare seed, pe RELEASE nu are utilizator până nu îl atribuie
  administratorul, iar navigația XAF pentru el nu e configurată. (83-r5,
  84-r7)
- Implicitele TVA pentru cumpărări extra-UE și pentru unele cumpărări de la
  neînregistrați nu au politici distincte în seed-ul privat; culegerea poate
  cere alegerea explicită. (81-r1)

## TVA

- La decont, partenerul fiscal e titularul, latura numită de politica de TVA;
  furnizorul de pe bon nu apare în D394. (C111-r1)
- O politică de TVA care nu numește o latură (sursă `Explicit` sau
  `TipMaterial`) lasă baza în D394 la „fără partener”. Cele șase politici din
  seed numesc terțul documentului. (S13)
- Diagnosticul R6 verifică intervalele configurate ale cotelor, nu toate
  condițiile legale. Nu deduce factura fiscală din lot pentru ajustările
  RDC/RLF și reduceri, nu ține consumul și restul avansului și nu refuză
  depășirea lui; referințele parțiale repetate sunt permise. Nu repară taxe
  sau calificări istorice. (103-r2)
- Pe FCT și FCL, baza negativă cu taxa culeasă pozitivă e admisă la
  culegere; abaterea e doar diagnosticată. (103-r3)
- Implicitele privat `FCT|RLF/UE → TI21` și `DVI → IMP21` nu sunt datate:
  înainte de 2025-08-01 tipul se alege explicit, cu explicație. Rândurile
  istorice datate în seed cer alegerea owner-ului. (103h, 103i, R6-B5)
- D300 și D394 acoperă cazurile implementate, nu formularele integral; datele
  neacoperite apar în avertismente și în sumele neincluse. Indicatorul de
  rectificativă D394 e disponibil pentru exact o lună; D300 se corectează
  prin regularizări. (36f, 69-r2, 69-r6, D4-r7, D4-r13)
- Confirmarea depunerii păstrează formularul, perioada, amprenta faptelor
  exportate și momentul confirmării; nu transmite declarația la ANAF și nu
  verifică recipisa. (103)
- Rotunjirea pe document × cotă privește taxa nemarcată; taxa culeasă rămâne
  suma liniilor, fără regularizare pe grup. Cubul păstrează valorile și
  calificarea fiscală, nu și identitatea și clasificarea partenerului de la
  data faptului. (109, 36f, D4-r1)

## SAF-T

- SAF-T acoperă profilurile L și S pentru cazurile implementate: nu toate
  variantele de contribuabil, proprietarii terți ai stocului sau toate
  mișcările logistice. Validarea XML nu dovedește completitudinea datelor
  economice. (73-r2, 73-r16, 74-r1)
- Compensarea prin notă contabilă nu apare în `Payments`. Exportul refuză
  plățile în valută și cu TVA la încasare; factura în valută se declară în
  RON, cu avertismentul `FacturaInValuta`. (SAFT-r1, B-r6)
- Exportul și validatorul rulează pe lună întreagă; trimestrul e exclus. (S0-R4)
- `SDD` și `SFD` nu au cod SAF-T de achiziție; `N9` poartă codul de livrare
  al rândului 10.1, iar maparea D300 îl pune pe rândul 11. (84-r1, 84-r2)
- Nu există catalog NC complet și nici conversie sigură a oricărui text de
  unitate de măsură din legacy. Precizia prețului exportat e distinctă de
  precizia internă a prețului și a cantității. (73-r4, 73-r5, 73-r11)

## Raportare

- După prima închidere, un cont sau un cont cu repartitor fără mișcare și cu
  sold zero nu mai apare în balanță; cifrele celorlalte rânduri nu se
  schimbă, iar declarațiile nu raportează soldul zero. (F27-D3)
- Raportul de stoc listează și consumurile bonurilor, ca rânduri pe contul de
  cheltuială la locul de consum; ele cresc cu tot ce s-a consumat. (X-D5,
  X-r3)
- Soldurile pe parteneri nu desfac rândul „fără terț”: sumele pe repartitor
  intern, pe cont propriu și fără repartitor ale unui cont apar împreună;
  defalcarea e în balanța analitică, iar postările în fișă, cu `faraTert`.
  Locul și partenerul fiscal nu sunt axe de sold și nu au filtru pe nicio
  proiecție. (C111-Q13, C111-Q8)
- Rândurile unei asamblări n-au contrapartidă în fișa contului și în
  registrul jurnal: consumul și produsul se corespund pe grup, nu pe pereche.
  (D9-A2)
- Explicația există numai pe tranzacțiile scrise de un contract:
  împerecherea, desfacerea, deschiderea și stingerea ei nu au explicație.
  (X-D4)
- Explicația numește rândul de politică care a decis și dacă s-a schimbat de
  la operare, nu cum arăta atunci; nomenclatoarele care decid contarea
  (contul tipului de material, conturile tipului de TVA, steagurile contului)
  nu se rețin. Politica de amortizare apare pe orice fișă atinsă care are
  rând, fără să spună care cont a venit din ea. (D9-r3, D9-A8)

## Nomenclatoare

- Proveniența adresei de la ANAF nu se păstrează pe componente: completarea
  golurilor poate produce o adresă cu surse mixte. (72-r2)
- Nu toate informațiile ANAF despre înregistrare și radiere ajung în domeniu.
  Sincronizarea individuală poate păstra o salvare făcută înaintea unei erori
  ulterioare; mesajul transportului nu dovedește un rollback global. (72-r3,
  72-r4)

## Securitate și refuzuri

- Un rol cu citire restrânsă pe rândurile `Postare` primește balanța, fișa,
  registrul jurnal și soldurile pe terți calculate numai din rândurile pe
  care le vede, fără refuz și fără semn că cifra e parțială; 403 cu dreptul
  unic e numai pe rutele ușii de sistem. Niciun rol din seed nu are un
  asemenea criteriu. (110 e, C111-Q22)
- Refuzul de acces pe cifrele din cub e fraza generică de citire pe
  `Postare`: nu spune dacă lipsește dreptul pe tip sau dacă rolul are un
  criteriu de rând ori de membru. (D9-D9)
- `EXPLICATIE_ACCES_INCOMPLET` se decide pe tip: un criteriu de rând pe
  `Postare` sau pe o frunză de document refuză explicația oricărei
  tranzacții, inclusiv a uneia pe care criteriul n-o atinge. (SAF-D4)
- Refuzurile de ștergere pentru lot, tip de TVA și fișă numesc încă
  registrul în text. (D9-A6)
- `SCRIERE_OCUPATA` iese 422, ca orice refuz de domeniu; clientul îl
  deosebește de un refuz definitiv numai după cod. (108)
- O comandă repetată, de exemplu după un timeout, nu e recunoscută ca
  reluare: nu există reluare idempotentă generală. (42f)
- Calculul ASM `distribuie-valoarea` citește prețurile loturilor fără un
  drept separat de citire pe `Lot`. (80-r7)
- Controllerul de rapoarte rămas din scaffold poate întoarce 404 cu corp gol,
  în afara formei comune `Erori`. (80-r3)
- Metadata OData descrie modelul expus și nu e filtrată după permisiunile
  utilizatorului. (55g)

## Ecrane

- Paginile React de detaliu ale documentelor sunt înghețate: câmpurile
  adăugate modelului după 2026-09-27 nu apar acolo, iar convențiile de
  selector neadoptate (BTR, `TipMaterial`) nu se mai aplică. Culegerea e în
  XAF Blazor. (104d, 104-r4)
- Listele de documente din React n-au coloana „Data înregistrării” și nici
  legătura de corecție, care se vede pe ecranul documentului, ca bandă, și în
  grupul „Corecție” din XAF. Nu există listă proprie de împerecheri în React:
  desfacerea se face din panoul documentului. (F27-r9, F27-D6, F27-D8)
- Soldul partenerului nu apare în selectoarele de partener din culegere; se
  vede în ecranul și pe ruta soldurilor pe parteneri. (F27-r7)
- Un link vechi cu `gestiuneId` deschide raportul nefiltrat, fără refuz.
  Legătura „Înapoi la balanță” din fișă apare și când fișa e deschisă din
  soldurile pe terți. (C111-Q6)
- Invalidarea cache-ului nu reîncarcă imediat toate widgeturile montate;
  selectoarele pot cere reîmprospătare. (77-r5)
- La unele citiri OData, transportul DevExtreme reduce eroarea la status și
  pierde mesajele `Erori`; unele mesaje de transport sunt în engleză. (80-r1,
  70-r5, 81-r9)
- În formularul regulii de contare, dimensiunile `Unitate*` sunt numai de
  citit; întoarcerea din panoul „Explică” în grilă nu focalizează rândul.
  (84-r4)
- Perioadele fiscale nu se administrează complet din React; conturile se
  citesc prin OData, fără editor contabil general. (53i, 79-r3, 70g)
- În XAF, selectoarele de tip TVA nu filtrează `Activ`, unele grile de
  politici afișează FK-uri brute, iar selectoarele fișei de imobilizare și ale
  liniei PIF nu filtrează după natură, stare și loc; clasificarea se caută
  doar după denumire. Mesajele gardienilor de imobilizări scriu numele
  membrilor enum. (81k, 81-r7, F26-r19, F26-r20, 87)
- În XAF, totalul și valorile de livrare și recepție ale documentelor apar în
  DetailView, nu în liste. Un layout salvat poate ascunde toate coloanele; pe
  `ServerView` celulele rămân goale până la Refresh după re-bifarea lor. Grila
  Detalii a unei facturi importate poate arăta coloana `Produs` goală. (85-r3,
  85-r9, 85-r10)
- Lista de evidență a cubului (`PostareVizual`): două loturi ale aceluiași
  produs din aceeași zi au aceeași etichetă, valuta nu are cod, rândul n-are
  navigație spre tranzacție, sortarea pe o coloană de cod scanează tot cubul,
  iar contraponderea se afișează cu numele enumului, fără diacritice (`Tert`).
  (D9-A12, S5)

<!-- gen:decizii -->
## Decizii

Deciziile care numesc pagina în `Arii:` (29; titlurile în [index](../decizii/README.md); generat de `docskit gen`, nu se editează): 37, 41, 42 (amendată de 104, 106), 43 (amendată de 104), 46 (amendată de 113), 53, 55, 72, 75, 77, 80, 81, 84, 85, 87 (amendată de 97), 89, 91 (amendată de 107, 118, 120), 94, 95, 97 (amendată de 98), 102, 103, 104 (amendată de 106), 106, 107, 108 (amendată de 110), 109, 115, 116 (amendată de 117).
<!-- /gen:decizii -->
