# Limite curente

**Actualizat: 2026-09-09.** [Index](README.md)

Această pagină delimitează implementarea disponibilă. Elementele de aici nu
sunt angajamente de livrare și nu descriu o ordine de implementare.

## Domeniu și operare

- Serializarea operațiilor concurente și reluarea idempotentă generală a
  comenzilor nu sunt acoperite complet. Validarea într-o singură operație
  nu dovedește protecția față de două comenzi simultane. (25f, 42f)
- Două împerecheri noi, încă necomise în același context, au o limită de
  vizibilitate în calculele bazate pe interogarea bazei. Plafoanele nu trebuie
  prezentate ca protecție completă pentru orice lot de scrieri concurente. (41d)
- Capacitatea NTC este netată pe contrapartidă și latură, nu pe cont.
  Calculul restului scade conservator legăturile existente în ambele roluri. (76-r1, 76-r2)
- Ieșirea fiscală RLF poate lăsa un reziduu valoric pe cheia de stoc golită.
  Documentele retroactive nu reevaluează ieșirile deja operate. (75a)
- Unele distribuiri ASM nu sunt reprezentabile exact la precizia prețului.
  Cazurile refuzate nu sunt corectate prin prețuri sau valori forțate. (75-r4, 76c)
- Recepția unui NIR nu declanșează automat completarea DSC pentru facturile
  cu acoperire parțială. Există comanda de generare suplimentară pe FCL. (37g, 38d)
- Fluxurile de rezervare, comenzi de vânzare și distribuire a aceleiași
  facturi din mai multe gestiuni nu sunt acoperite complet. (37g, C1a)
- Retururile nu au un flux general propriu de compensare; se folosește NTC. (46f, 76g)
- Salariile, imobilizările, execuția bugetară completă, producția pe rețete,
  împărțirea pe cofinanțări și contabilitatea multivalutară nu sunt module
  complete în produsul curent. Importul 1C nu echivalează cu un import bancar
  operațional general. (9, 21, 31f)

## Fiscalitate și nomenclatoare

- `SDD`/`SFD` nu au cod SAF-T de achiziție; `N9` poartă codul de livrare al
  rândului 10.1 în timp ce maparea D300 îl pune pe rândul 11. (84-r1, 84-r2)

- D300 și D394 sunt proiecții pentru cazurile implementate, nu acoperirea
  integrală a formularelor. Nu există export XML D300/D394. (69-r6, D4-r13)
- Prorata, ajustările, cazurile fiscale speciale și toate secțiunile
  informative D394 nu sunt implementate integral. Datele neacoperite rămân
  explicite în avertismente și în sumele neincluse. (36f, 69-r2, D4-r7)
- Marcajele ANAF pentru TVA la încasare nu constituie implementarea întregului
  mecanism contabil. Fluxurile de exigibilitate amânată și conturile
  intermediare aferente nu sunt acoperite complet. (36f, D4-r9)
- Rotunjirea TVA pe grup fiscal document × cotă nu este un mecanism general.
  Registrul păstrează valorile fiscale, dar nu un snapshot complet al
  identității și clasificării istorice a partenerului. (36f, D4-r1)
- Implicitele TVA pentru cumpărări extra-UE și anumite cumpărări de la
  neînregistrați nu au politici distincte în seed-ul privat. Fallback-ul la
  ancora documentului poate cere alegere explicită la culegere. (81-r1)
- SAF-T acoperă profilurile L și S implementate și testate. Nu acoperă toate
  variantele de contribuabil, multivaluta, proprietarii terți ai stocului și
  toate mișcările logistice. Validarea XML nu dovedește completitudinea
  datelor economice. (73-r2, 73-r16, 74-r1)
- Nu există catalog NC complet și nici conversie automată sigură pentru
  orice text UM legacy. Precizia câmpurilor de preț exportate trebuie
  distinsă de precizia internă a prețului și de cea a cantității. (73-r4, 73-r5, 73-r11)
- Nu există integrare operațională e-Factura.

## ANAF și configurare

- Limitarea ritmului ANAF este pe cerere; nu există coordonare globală între
  cereri simultane. (72-r1)
- Proveniența adresei nu este păstrată separat pentru fiecare componentă.
  Completarea golurilor poate produce o adresă cu surse mixte. (72-r2)
- Nu toate informațiile ANAF despre înregistrare/radiere sunt consumate în
  domeniu. Fluxul individual poate păstra o salvare efectuată înaintea unei
  erori ulterioare; mesajul transportului nu este dovada unui rollback global. (72-r3, 72-r4)
- Sincronizarea ANAF în lot este disponibilă prin API, fără ecran React
  dedicat pentru întregul flux. (77-r3)
- În formularul regulii de contare, dimensiunile `Unitate*` sunt doar de
  citit: setul OData `Unitate` nu are controller. Întoarcerea din panoul
  „Explică" în grilă nu focalizează rândul. (84-r4)
- Captions: `SursaCont` și mai mulți membri ai politicilor nu au
  `[XafDisplayName]`; ecranele poartă caption-ul în cod. (84-r5)
- Selectoarele XAF de tip TVA nu filtrează încă `Activ`; unele grile de
  politici afișează FK-uri brute și nu au configurarea vizuală completă. (81k, 81-r7)
- Explicația e a configurației pe o linie ipotetică, nu planul unui
  document; gate-ul ei de citire e pe tip, nu pe obiect, iar codul de tip se
  rezolvă înaintea gate-ului (400 pe cod inexistent pentru orice rol). Nu
  există import/export general al configurației. Raportul de profil nu are
  categoria „rând de seed lipsă"; recrearea unui rând de seed șters rămâne
  manuală. Marcajul istoric nu distinge toate intervențiile manuale
  anterioare, iar alinierea atinge acum și `Cont.DimensiuniObligatorii` pe
  rândurile marcate. (81k, 81-r3, 83-r1, 84-r3, 84-r6, 84-r8)
- Rolul `Configurator` e al release-ului: permisiunile lui se reaplică la
  fiecare seed, iar pe RELEASE nu există un utilizator cu acest rol până nu îl
  atribuie administratorul; navigația XAF nu e configurată. (83-r5, 84-r7)
- Administrarea perioadelor fiscale nu are un flux React complet. Conturile
  sunt expuse prin OData pentru citire, nu prin editor contabil general. (53i, 79-r3, 70g)

## Client, API și livrare

- Cheia cache-ului `byKey` combină proiecțiile de selecție și expandare într-o
  formă care nu distinge toate combinațiile posibile. Nu se presupune că
  orice proiecție nouă este sigură fără verificarea cheii. (77b)
- Invalidarea cache-ului nu garantează reîncărcarea imediată a tuturor
  widgeturilor deja montate. Selectoarele pot necesita reîmprospătare. (77-r5)
- La unele citiri OData, transportul DevExtreme reduce eroarea la status și
  pierde mesajele `Erori`. Unele mesaje de transport rămân în engleză. (80-r1, 70-r5, 81-r9)
- Refuzurile asupra navigațiilor OData expandate nu au acoperire completă
  prin probe HTTP pe roluri restrânse. (80-r4)
- Calculul ASM `distribuie-valoarea` citește prețuri prin context nesecurizat
  fără un drept separat de citire pe Lot. Autorizarea documentului nu
  echivalează cu această permisiune asupra nomenclatorului. (80-r7)
- Controllerul de rapoarte rămas din scaffold poate întoarce 404 cu corp gol,
  în afara formei comune `Erori`. (80-r3)
- Metadata OData descrie modelul expus și nu este filtrată ca o listă de
  înregistrări după permisiunile utilizatorului. (55g)
- Găzduirea clientului și API-ului pe aceeași origine este contractul de
  livrare. Hostul are `UseStaticFiles`, dar proiectul WebApi nu include
  copierea automată a build-ului React și fallback-ul rutelor SPA.
  Publicarea cere și configurarea JWT pentru mediul țintă. (43e, 55g)
- Reluarea parțială a importului cu `--continua` poate afecta interpretarea
  reconcilierii. Validarea completă cere o bază și un set de referință
  controlate, cu starea reluării cunoscută. (75-r3)
