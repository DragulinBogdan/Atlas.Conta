# CITIRI — TR-D8

### Fiscal — specificație aprobată înaintea codului (103, D8-B8)

**Implementat și verificat în domeniul de mai jos.** Owner-ul a aprobat
F1=A, F2=A, F3=A (103). Exemplele sunt așteptări independente.
Sumele de mai jos sunt în lei, înaintea rotunjirii la unitatea formularului.

| Scenariu | Date independente | Așteptarea aprobată |
|---|---|---|
| SC-CIT-79 — normal și capitalizat | FCT normal 100 + 21; FCL 200 + 42; FCT capitalizat 100 + 21 | achiziții: bază 200, taxă 42, din care dedusă 21; livrări: 200/42; cost capitalizat 121, nu bază fiscală 121; trei documente fiscale |
| SC-CIT-80 — cotă istorică | fapt 31.07.2025: 100/19; fapt 01.08.2025: 100/21; citire după schimbarea cotei/etichetei tipului | două calificări: 19% și 21%, bază 200, taxă 40; inverse −100/−19 și −100/−21; niciodată recalcul 42; formularul istoric indisponibil este semnalat |
| SC-CIT-81 — autolichidare (F3) | achiziție TI internă 100 la 21%, deducere integrală; separat, livrare TI 100 | achiziție: datorie 100, bază unică 100, taxă 21 și autocolectare 21, D300 ambele laturi 21, D394 o achiziție C; livrarea are bază 100, taxă facturată 0 și nu inventează taxa beneficiarului |
| SC-CIT-82 — DVI | bază vamală 100 în cartea Fiscal, TVA 21 în Contabil | intrarea comună 100/21; contraponderea fiscală nu adaugă încă 100; soldul contabil și datoria 446 rămân 21; storno −100/−21; importul nu devine achiziție internă D394 |
| SC-CIT-83 — document fizic de corecție (F2) | factură 100/21 în ianuarie declarat; factură distinctă de reducere −20/−4,20 emisă/primită în februarie | ianuarie 100/21; februarie −20/−4,20; cumulat 80/16,80; două documente, legate prin proveniență; nicio rescriere a originalului |
| SC-CIT-84 — eroare de evidență (F2) | aceeași factură a fost introdusă 100/21, corect este 80/16,80; descoperire în februarie după declararea lui ianuarie | cub: original + inversă + corectă, net 80/16,80; D300 Δ −20/−4,20 la regularizare în februarie; D394 ianuarie înlocuit cu 80/16,80, **o** factură; inversa tehnică nu este document nou |
| SC-CIT-85 — primit ulterior (F1) | document 31 ianuarie, primit/înregistrat 5 februarie, 100/21, ianuarie deschis; deducerea normală este exercitată în februarie | D394 februarie 100/21, D300 februarie 100/21 în ipoteza precizată; data documentului rămâne ianuarie; închiderea ulterioară nu mută atribuirea |
| SC-CIT-86 — rotunjire și taxă manuală | trei linii cu bază 0,03 și taxă acceptată 0,01 fiecare | fapte: bază 0,09, taxă 0,03, nu recalcul 0,02 pe agregat; inverse −0,09/−0,03; diferența față de calculul global rămâne vizibilă, rotunjirea formularului numai la margine |
| SC-CIT-87 — securitate și profil | două fapte 100/21 și 25/5,25; aceleași citiri înainte/după închidere | privat Admin/Cititor 125/26,25; refuzul rândurilor celui de-al doilea fapt 100/21; refuzul postărilor Taxă păstrează bazele fără a reconstrui taxa din cotă; Valoare refuzată nu expune sume prin totalurile fiscale/TaxInformation; User fără citire pe Postare primește 403; bugetar fără politică TVA păstrează contabilul/stocul și nu inventează fapte fiscale, SAF-T neaplicabil |

SC-CIT-79/81/82/85/87: verificăm separat jurnalul, D300, D394 și
TaxInformation SAF-T, cu excluderi explicite după domeniul fiecăruia.
SC-CIT-83 are document fiscal nou; SC-CIT-84 are numai corecție tehnică.
Storno integral peste închidere păstrează și proba existentă 100/21 în
ianuarie și −100/−21 în februarie; nu este confundat automat cu ștergerea
unei facturi din declarația inițială.

Pe bugetar se repetă mecanismele comune (istoric, inversă, permisiuni),
cu politică fiscală explicită în fixture atunci când testăm fapte TVA;
nu modificăm seed-ul profilului pentru a fabrica o obligație fiscală.
Un fapt cu TipTvaId șters/invizibil păstrează cifrele și afișează lipsa
etichetei. Duplicarea unui rol sau calificările istorice incompatibile
sunt detectate de invarianții ModelCheck și de mutanții lor; nu se adaugă
scanare la activare (102).

Validare fiscală 2026-09-26: `ScenariiFiscale` și integrala
`run-verificari/20260925-234727-019/rezultat.json`, **3.182/4.214 OK**.
SC-CIT-79…86 au probe private; SC-CIT-87 are mecanism bugetar inert și
politică explicită temporară, plus HTTP privat în cele trei stări. Refuzul
pe întregul tip produce 403, iar cel pe rând/valoare/taxă păstrează cifrele
aprobate. `fiscal-http-probe4.log` este execuția HTTP finală.

Precizarea fixture-ului SC-CIT-80: tranziția 19→21 este simulată pe același
TipTva, în aceeași lună februarie a anului izolat 2023, înainte/după mutarea
cotei. Probează istoricul, eticheta absentă, maparea absentă și inversarea;
nu certifică aplicabilitatea calendaristică a regimurilor din iulie/august
2025. Datele acelea rămân exemplul de produs din specificație.

Browser: date fiscale și implicitul primirii; jurnal cu cele două perioade;
39 mapări SAF-T cu rolurile afișate; D394 10 facturi, 100.000/21.000 și
confirmare explicită. Performanța și limitele se citesc în
[review-ul propriu](../tr-d8-review-codex.md#felia-fiscală-103--review-propriu-2026-09-26).

### Corecturi snapshot — specificație înaintea codului, 2026-09-25

SC-CIT-76: lotul de deschidere 4/40 este în snapshot-ul lui ianuarie.
Evaluarea unui BCS de 1 în februarie dă 10. Alterarea snapshot-ului la
4/48 schimbă evaluarea aceleiași ieșiri la 12; reconstrucția raportează
diferența 8 și evaluarea revine la 10. Tranzacția probei se anulează.
Excluderea documentului curent folosește numai o referință anterioară
datei sale; excluderea istorică generală păstrează SC-CIT-73.

SC-CIT-77: construim citirile contabile, de partide și de lot înaintea
închiderii lui februarie; închiderea pe altă conexiune înlocuiește
referința ianuarie și îi elimină snapshot-urile. Executarea citirilor
deja construite păstrează cifrele de control (stocul lotului 8/80 în
sursă și 2/20 consum, datoria facturii 100). Alegerea referinței și
citirea au o singură instrucțiune SQL, fără execuții la compunerea LINQ.

SC-CIT-78: un ObjectSpace secured este refuzat înaintea oricărei scrieri
globale de snapshot: materializare, materializarea separată a partidelor,
reconstrucție și eliminare. Refuzul nu modifică snapshot-urile existente.
În XAF EF Core contează securitatea activă a contextului; fabrica
nesecurizată poate întoarce tot clasa `SecuredEFCoreObjectSpace`.
SC-CIT-74 HTTP verifică închiderea/reconstrucția prin fabrica reală și
citirea filtrată prin utilizatorii reali.

### `Transfer` pe cititorii comuni — N-r8, X-D2 (2026-10-03)

Așteptările sunt numărate de mână din scenă. Oracolul citește cubul direct:
rândurile tranzacțiilor `Transfer` ale documentelor și rândurile `Storno` a
căror origine `InversaDin` este unul dintre ele. Fiecare intrare comună cu
regim declarat le numără pe aceleași identități.

| ID | Scenariu | Așteptare | Stare |
|---|---|---|---|
| SC-CIT-100 | BTR 4 × 10 pe lot, stornat în ianuarie | 2 postări Transfer și 2 inverse; `Loturi.Postari` le întoarce pe toate 4; `Contabil.Postari`, `Contabil.Jurnal`, `Plati.Postari` și `Partide.Postari` întorc zero | verificat pe ambele profiluri |
| SC-CIT-101 | FCT 100, plăți 40 și 60 împerecheate; plata de 60 stornată în februarie cu desfacerea legăturii | 6 postări Transfer (40, 60 și desfacerea −60, câte două), nicio inversă `Storno`; `Partide.Postari` le întoarce pe toate 6; intrările contabile și `Loturi.Postari` întorc zero | verificat pe ambele profiluri |
| SC-CIT-102 | ASM mixt (Operare + Transfer) stornat în februarie | 4 postări Transfer și 4 inverse într-un `Storno` care poartă și inversele `Operare`; `Loturi.Postari` întoarce cele 4 rânduri pe lot; intrările contabile întorc zero, deși inversele `Operare` ale aceluiași storno rămân în jurnal (SC-CIT-04) | verificat pe ambele profiluri |

`Fiscale.Postari` și `Imobilizari.PozitiiFaraFisa` nu filtrează felul și sunt
doar măsurate. Proba structurală `N-r8` cere regimul declarat pentru orice
intrare publică nouă.

### Activarea, regimul dual și restanțele TR-D8 — X-D7 (2026-10-03)

Așteptările cantitative sunt scrise de mână din regula recepției conexe
(098, 099): registrul ține cumulul constatat pe NIR, cubul ține recepția pe
factură și numai diferența pe NIR. Probele citesc registrul și cubul direct,
nu prin interogarea invariantului.

| ID | Scenariu | Așteptare | Stare |
|---|---|---|---|
| SC-CIT-103 | FCT 4 × 25 cu recepție conexă, de trei ori: constatat 4, constatat 3, constatat 4 plus un lot nou de 1; recepția cu 3 stornată; recepția cu 4 corectată la 2 | pe lotul sursei cubul ține 4 pe factură cu 0, −1 și 0 pe recepție; lotul adăugat are 1 pe recepție; după storno cubul rămâne 4 pe factură; după corecție, 4 pe factură și −2 pe corecție. Comparația cu registrul grupului a ieșit odată cu registrele (TR-D9a, pasul 6) | verificat pe ambele profiluri |
| SC-CIT-104 | aceleași fapte, cu trei alterări în tranzacție anulată: cantitatea recepției de pe factură pusă pe zero, cantitatea deltei pusă pe zero, `SursaReceptieiId` ștearsă | fără obiect după tăiere: mutanții erau ai acoperirii registru → cub (`CITIRE_ISTORIC_STOC_INCOMPLET`) | scos (TR-D9a, pasul 6) |
| SC-CIT-105 | BTR 4 din lotul 10 × 10 | −4 pe sursă și +4 pe destinație în cub; cu cantitatea unui capăt pusă pe zero, pe rând, `CITIRE_TRANSFER_NECONSERVAT` | verificat pe ambele profiluri |
| SC-CIT-106 | ASM mixt stornat (faptele SC-CIT-102) | 8 contraponderi Transformare (4 linii și inversele lor); zero pe fiecare dintre cele șapte intrări comune | verificat pe ambele profiluri |
| SC-CIT-107 | BTR stornat; plățile împerecheate, una desfăcută; ASM mixt stornat | `RegistruJurnal` și `FisaCont` nu listează nicio postare `Transfer` și nicio inversă a ei; `Loturi.Postari` le întoarce cu felul fiecăreia (2 `Transfer` și 2 `Storno`, în ambele scene), `Partide.Postari` la fel (6 `Transfer`) | verificat pe ambele profiluri |
| SC-CIT-108 | plată nominalizată automat pe factură; nominalizarea mutată în cub pe altă partidă, apoi readusă | ștergerea și desfacerea legăturii dau `IMPERECHERE_FARA_EFECT`; legătura rămâne, rândul invers nu se scrie, cubul plății e neschimbat | verificat pe ambele profiluri |
| SC-CIT-109 | FCT cu tranzacții în cub, CAS fără; pe privat, seed-ul profilului bugetar rulat nesalvat după un RLF operat | fără obiect după tăiere: regimul `PosteazaInCub` nu mai există (D9-D5) | scos (TR-D9a, pasul 6) |

`Imobilizari.PozitiiFaraFisa` exclude acum contraponderile ASM, ca celelalte
intrări. Ramurile fișei fără origine și fără suport (097-r1) au mutanții
`IMO-ORIGINE` și `IMO-SUPORT` în `INV-CUB`.

### Explicația deciziei — X-D4 (2026-10-03)

Așteptările sunt scrise de mână din regula evaluării: ieșirea ia
`q × sold / cantitate`, ultima ieșire ia restul, stingerea ia partidele în
ordinea deschiderii. Explicația se citește prin `Cub.Citiri.Explicatii`, nu
din contractul recalculat.

| ID | Scenariu | Așteptare | Stare |
|---|---|---|---|
| SC-CIT-96 | Recepție 3 × 0,333333 și 10 × 12,50; BCS cu liniile 1 (lot ieftin), 4 (lot scump), 1 (lot ieftin); BTR 2 din lotul scump, după bon; storno al amândurora; alt BCS operat și anulat | dry-run-ul nu persistă nimic; o singură explicație, pe `Operare`, cu declarantul, perioada, regula de contare consumată (cu contorul ei) și două solduri citite; pe linii: 0,33 din 1,00/3, 50 din 125/10, 0,34 din soldul curent 0,67/2; BTR: purtătorul e `Transfer`, decizia +2/+25 din soldul net 75/6 explică sursa postată −2/−25; `Storno` nu are explicație proprie, iar cititorul întoarce explicația originalului; baza refuză explicația pe `Storno` (`CK_Tranzactie_Explicatie`); anularea nu lasă nici tranzacție, nici explicație; forma persistată se citește înapoi identic, și după trecerea prin `jsonb`; versiune, decizie sau ipoteză necunoscută = refuz | verificat pe ambele profiluri |
| SC-CIT-111 | D9-A8: recepție proprie 4 × 12,50 pe tipul de material al scenei; primul BCS de 1 pe regula proprie; editarea aceleiași reguli prin ușa gardianului (contul creditor explicit, rezervă fără efect pe postări); al doilea BCS de 1; ștergerea ei prin gardian; BTR; pe HTTP, regula editată de două ori pe OData | explicația reține o singură `VersiunePolitica` (`RegulaContare`, rândul câștigător, `OptimisticLockField` al lui) și regula pe fiecare `ContRezolvat`; DTO-ul o arată neschimbată; editarea crește contorul cu 1 și face rândul al clientului, operarea nouă reține contorul nou, explicația veche rămâne identică și e arătată „schimbată”, cea nouă nu; regula ștearsă = „schimbată”, fără contor curent; regula proprie și refuzul ei de seed sunt purjate pe identitatea tipului, fără atingerea regulilor existente; BTR n-are ipoteză; forma persistată e versiunea 2, versiunea 1 e refuzată; pe host: contorul curent +2, explicația veche „schimbată” | ModelCheck reverificat pe ambele profiluri (`20261007-205913-703`); HTTP probat la pasul 8 inițial; reverificat la 113 (`20261008-005737-235`, `run-verificari/110r1-http/`) |
| SC-CIT-111-IZOLARE | Regulă BCS proprie editată, apoi într-o iterație nouă ștearsă; curățenie în ObjectSpace nou; regulă BCS străină martor | după editare: regula proprie este a clientului; după ștergere: exact un refuz de seed nou; reluarea curăță regula și refuzul proprii; regula străină și cea din seed rămân identice inclusiv contorul; mulțimea refuzurilor preexistente este identică | verificat pe ambele profiluri (D9-F-R2/R3), integrala `20261007-205913-703` |
| SC-CIT-97 | Două facturi de 60 și 40 ale aceluiași furnizor; NTC cu liniile 70 și 50 pe contul furnizorului; separat, plată împerecheată cu factura | linia 70 stinge 60 + 10, linia 50 stinge 30 și deschide partida proprie pentru 20; soldurile citite ale partidelor sunt −60 și −40; transferul împerecherii nu are explicație, operarea plății o are pe a ei | verificat pe ambele profiluri |
| SC-CIT-98 | ASM mixt (consum 60 pe contul produsului, consum 40 pe alt cont); ASM cu Δ de rotunjire; RLF 1 bucată la golirea lotului 3 × 10,006667 după două ieșiri; NIR conex care constată 3 din 4 × 25 | ASM mixt: o explicație, pe `Operare`, referită de `Transfer` prin `ExplicatieDinId`; consumurile 60 și 40 evaluate din soldurile 60 și 40; stornoul inversează ambele tranzacții și are o singură explicație de origine; Δ-ul absorbit e în explicația purtătorului `Transfer`; RLF: 1/10,01 `ValoareDeclarata` cu sursa `Linie`, fără sold citit, reziduul −0,01 rămâne pe lot; NIR-minus: 1/25 `ValoareDeclarata` cu sursa `Receptie` | verificat pe ambele profiluri; RLF numai privat |
| SC-CIT-99 | HTTP pe host viu, bază privată clonată: FCT 10 × 10 și 5 × 5, BCS 4 și 1, apoi storno | `Admin` și `Cititor`: 200, 40 din 100/10 și 5 din 25/5; `User` și un id inexistent: același 404; trei roluri cu citire implicită și o restricție — membrul `Postare.Valoare`, rândul `DocumentDetaliu` cu cantitatea 1, rândurile `Postare` dinaintea datei bonului: 403 `EXPLICATIE_ACCES_INCOMPLET` cu tipul restricționat și fără nicio valoare, linie sau lot; tranzacția `Storno`: 200 cu explicația originalului | verificat (`nou/tools/ProbeHttp/explicatii.py`) |

Invariantul de audit rulează în `INV-CUB`, pe faptele fiecărei scene a
catalogului, cu șapte mutanți proprii. S-r11 e probată în scena SC-CIT-96:
o cantitate cu patru zecimale, necomisă, dă refuzul `DECLARATIE_INVALIDA`.

### Cititorul declarat — C104 pasul 2, 2026-09-27

SC-CIT-95: în tranzacția SC-CIT-76, după alterarea snapshot-ului lui
ianuarie cu +8, soldul lotului la 31 ianuarie citit `Integrala` depășește
cu exact 8 soldul citit `Vizibila`: citirea vizibilă nu atinge snapshot-ul,
cea integrală pornește din el. Alegerea o face apelantul, nu tipul
ObjectSpace-ului (104b).

### Snapshot de stoc — specificație înaintea codului, 2026-09-25

SC-CIT-69: FCT 10/100 cu NIR încă draft și deschidere exclusiv în cub
4/40. Închiderea păstrează ambele loturi, fără să aștepte registrul NIR.
Cheia snapshot-ului este lot × cont × produs × gestiune; data deschiderii
vine din postări. Cantitatea și valoarea sunt semnate; numai 0/0 se omite.

SC-CIT-70: BTR 4/40 din recepția 10/100, BCS 2/20 din sursă. La închidere:
stoc sursă 4/40, stoc destinație 4/40, consum 2/20 pe contul de cheltuială
și locul de consum. Același lot pe coordonate diferite rămâne separat.

SC-CIT-71: storno BTR în februarie, după închiderea lui ianuarie:
sursa 8/80, destinația 0/0 omisă, consumul 2/20 păstrat. Citirea directă,
snapshot + fereastră și raportul coincid. Ianuarie rămâne 4/40 + 4/40 + 2/20.
Închiderea lui februarie incrementală coincide cu reconstrucția integrală.

SC-CIT-72: alterarea unui snapshot cu +7 valoare se raportează înaintea
rescrierii; a doua reconstrucție are zero diferențe. Alterarea numai a datei
deschiderii se raportează de asemenea, fără diferență numerică fictivă.

SC-CIT-73: lotul de deschidere 4/40, consumat integral în februarie,
nu mai este disponibil în gestiunea reală; ieșirea păstrează valoarea 40.
FIFO și raportul păstrează data deschiderii peste granița snapshot-ului.
Citirea cu excluderea unui document recitește postările, fără a-l păstra
ascuns în snapshot. Un snapshot absent permite citirea integrală din cub.

SC-CIT-74: HTTP, înainte/după închidere și reconstrucție: ascunderea
postărilor sau a membrului Valoare schimbă raportul conform drepturilor.
Citirea securizată nu folosește snapshot-ul global. Proba pe ObjectSpace
nesecurizat nu înlocuiește această verificare.

SC-CIT-75 (privat): recepție 3 × 10,006667 = 30,02, două DSC de câte
1/10,01 și RLF 1/10,01 lasă în gestiunea reală cantitate 0 și valoare −0,01.
Snapshot-ul și raportul păstrează reziduul; costul unitar este 0 și FIFO
nu îl oferă ca disponibil. Coordonatele virtuale ale DSC se păstrează,
fără a deveni gestiuni reale disponibile.

### Raport și snapshot de partide — specificație înaintea codului, 2026-09-25

SC-CIT-49: FCT 100 și PLT 40: raportul conține partida facturii,
contul furnizorului și rest datorie 60. După desfacere,
factura are rest 100 și plata are rest creanță 40. Cheia rândului include
unitatea, contul și partenerul; documentul este numai etichetă și navigare.

SC-CIT-50: deschidere creditor 60 + 40 pentru același furnizor, fără document:
două partide distincte, total 100; NTC 75 lasă numai a doua cu rest 25.
Snapshot-ul și raportul păstrează identitatea, inclusiv după reconstrucție.

SC-CIT-112 (115 (d)): pe faptele SC-CIT-50, partida de 60 e născută la sursă
pe 15.12 și cea de 40 pe 10.12 ale anului dinainte, iar cea de 60 are
identificatorul mai mic. NTC 75 stinge întâi partida de 40, mai veche, apoi 35
din cea de 60: FIFO urmează data nașterii partidei, nu identificatorul.

SC-CIT-113 (116, 117): aceeași deschidere dă altui furnizor două partide, de 30 și
de 20, născute în aceeași zi; cea de 30 e dată prima și are identificatorul mai
mare. NTC 25 stinge 25 din cea de 30 și lasă neatinsă pe cea de 20: la aceeași
dată și fără document, FIFO urmează ordinea din deschidere.

SC-CIT-51: factura 100 stinsă integral în ianuarie, plata stornată în februarie:
raportul ianuarie rămâne fără rest, februarie arată factura cu rest 100.
Citirea directă, snapshot + fereastră și reconstrucția dau aceleași valori.
Starea curentă Stornat a unui document nu șterge faptele istorice.

SC-CIT-52: două partide pe același document și cont, pentru furnizori diferiți,
rămân separate. Modificarea unui snapshot cu +7 este raportată înainte de
rescriere; a doua reconstrucție are zero diferențe.

SC-CIT-53: identitatea documentului deschizător este verificată prin regula
092; plata care nominalizează partida facturii nu devine eticheta acesteia.

SC-CIT-110 (TR-D9a pasul 5c): pe faptele SC-CIT-42 și SC-CIT-52, SQL-ul
generat pentru `PartideCuRest` și `DocumenteCuRest` pe ambele feluri de
citire, pentru `Proprii`, `Perechi` și `MiscariPePartidele` și pentru
verificarea și scrierea snapshot-ului de partide nu conține nicio îmbinare cu
`a = b OR (a IS NULL AND b IS NULL)`. Verificat pe ambele profiluri.
Forma e căutată din 2026-10-08 în tot SQL-ul rulării (`RAMURA-NUL`);
SC-CIT-110 rămâne dovada că acești cititori sunt executați.
Traducerea SQL a identității este identică funcției nucleului pentru UUID-uri
distincte; deschiderea inițială nu se confundă cu identitatea unui document.
Cheia veche document + cont a ieșit (102c).

SC-CIT-54: HTTP înainte/după snapshot și reconstrucție: ascunderea postărilor
sau a valorii schimbă raportul conform drepturilor, fără acces la totalul
global materializat. Rândul fără document rămâne raportabil, fără buton de
stingere prin comanda veche care cere două documente.

2026-09-24. Specificație înaintea portării rapoartelor. Surse: 090(f/k),
091(g/i), 097 și `tr-d8-citiri-contract.md`. Așteptările sunt constante;
o citire directă din cub în ModelCheck este proba independentă, nu sursa
de producție.

| ID | Scenariu | Așteptare | Stare |
|---|---|---|---|
| SC-CIT-01 | FCT 1.200 → PIF contabil 1.200/fiscal 900 → storno PIF | intrarea contabilă întoarce zero postări pentru PIF și inversa sa; FCT păstrează D/C 1.200 | verificat pe intrarea comună |
| SC-CIT-02 | AMO contabil 100/fiscal 50 → storno în aceeași lună | două postări contabile originale de 100, două inverse de −100; nicio postare fiscală în citirea contabilă | verificat pe intrarea comună |
| SC-CIT-03 | DVI: bază fiscală 100 și TVA contabilă 21 | pe 4426 rulaj D 21/C 0; perechea fiscală 100/100 nu intră în jurnal/fișă/balanță | verificat pe intrarea comună |
| SC-CIT-04 | ASM mixt: Operare D/C 40 și Transfer pe același cont; storno mixt | jurnalul include numai postările economice de 40/−40; exclude Transfer și inversele lui prin origine, plus contraponderile structurale | verificat pe intrarea comună |
| SC-CIT-05 | BTR pe același cont → storno | zero rânduri contabile generale, mișcările pe lot rămân vizibile | verificat pe intrarea comună |
| SC-CIT-06 | Deschidere D activ 1.200/C amortizare 200/C capital 1.000 → PIF | trei postări inițiale fără document; nicio dublare; sold net activ 1.200, amortizare 200 | verificat pe intrarea comună |
| SC-CIT-07 | Storno BTR 4/40 | fiecare inversă poartă exact originea ei Transfer, scrisă de storno; invariantul de proveniență trece; citirea contabilă rămâne fără BTR (102c: backfill-ul istoric a ieșit) | verificat pe ambele profiluri |
| SC-CIT-08 | — | depășit de 102: completarea provenienței prin migrație a ieșit | — |
| SC-CIT-09 | Citire contabilă filtrată pe același BTR stornat, înainte și după separarea diagnosticului | A: diagnostic + citire = 2 comenzi SQL; B: compunere = 0, citire = 1; ambele întorc zero rânduri BTR; diagnosticul explicit rămâne o comandă | verificat pe ambele profiluri |
| SC-CIT-10 | Origine lipsă, ID inexistent, spațiu greșit sau altă postare Storno | invariantul de proveniență (`Cub.Citiri.Invarianti`) refuză cu număr 1; nicio inversă astfel coruptă nu devine rulaj contabil; pe proveniența validă trece; seed-ul nu mai citește proveniența (102) | verificat pe ambele profiluri |

SC-CIT-07/08 sunt probe de migrare: fixture operațional real, alterări SQL
controlate numai în tranzacția de probă, rollback obligatoriu. Nu sunt o
cale de culegere a documentelor. Review advers amânat de owner până la
sincronizare, după NIR.

Acesta este primul lot al intrărilor comune. Nu certifică portarea
rapoartelor, snapshot-urilor, fiscalului, securității sau SAF-T. Matricea
completă se extinde înaintea fiecărei portări, conform inventarului TR-D8.

Execuție: `run-verificari/20260924-100942-750/rezultat.json`, integral pe
ambele profiluri, exit 0. SC-CIT-03 este privat (DVI); celelalte sunt pe
ambele profiluri. Probele apelează `Cub.Citiri.Contabil.Postari`; jurnalul,
fișa și balanța API nu au fost încă portate. Activarea generală a cititorului
cere mai întâi diagnosticul stornourilor istorice fără origine.

Corecturi TR-D8/DEC: selectiv `DEC,BTR,ASM`, 327/389 OK, manifest
`run-verificari/20260924-160814-726/rezultat.json`; integral 2.723 bugetar /
3.814 privat OK, zero FAIL, build fără avertismente, exit 0,
`run-verificari/20260924-160912-899/rezultat.json`.
Migrația `OriginiStornoUnivoce` completează proveniența univocă;
diagnosticul explicit refuză restul cu numărul postărilor. Verificarea
provenienței este separată de citirea raportului. Seed-ul doar avertizează;
refuzul activării se leagă în felia primului raport portat. Performanța
pe istoric mare și portarea consumatorilor rămân în TR-D8.

D8-B3, prima execuție (cu refuz la seed, corectat ulterior): selectiv `BTR,ASM,DVI,IMO`, **519 bugetar /
719 privat OK**, zero FAIL, exit 0; build fără avertismente,
`run-verificari/20260924-234234-729/rezultat.json`. SC-CIT-09 numără
comenzile, nu estimează latența pe volum. SC-CIT-10 alterează numai în
tranzacție anulată la final și probează refuzul prin `ContaSeeder.Seed`.

Integral înaintea corecturii din 2026-09-25: **2.849 bugetar / 3.930 privat OK**, zero FAIL,
exit 0, build 0 avertismente; sursele C# nemodificate pe durata rulării.
`run-verificari/20260924-234503-103/rezultat.json`.

Corectură 2026-09-25: seed-ul avertizează în `RaportSeed` și în consolă,
fără refuz pentru proveniență. SC-CIT-10 execută seed-ul complet pentru
cele patru defecte și pentru istoric valid, verifică numărul 1/0 și
prezența/absența avertismentului, apoi păstrarea provenienței. Diagnosticul
explicit refuză în continuare defectele. Selectiv BTR: **118/118 OK**,
zero FAIL, exit 0, build fără avertismente,
`run-verificari/20260925-004612-520/rezultat.json`.

Integral după corectură: **2.864 bugetar / 3.945 privat OK**, zero FAIL,
exit 0, build fără avertismente; sursele C# nemodificate pe durata rulării.
`run-verificari/20260925-004756-015/rezultat.json`.

## Portarea contabilă — matrice înaintea codului (2026-09-25)

| ID | Scenariu | Așteptare |
|---|---|---|
| SC-CIT-11 | FCT stoc 10 × 10, BTR 4 × 10 și inversa lui | balanța contului de stoc D 100/C 0; soldul furnizorului C 100; filtrul gestiunii sursă D 100, fără rulajul BTR; partenerul și gestiunea rămân coordonate distincte |
| SC-CIT-12 | Aceeași recepție, închidere ianuarie, consum în februarie de 2 × 10 | snapshot ianuarie: D stoc 100, C furnizor 100; în februarie inițial stoc 100, rulaj C 20, final 80; rezultatele sunt identice cu cumulul fără snapshot |
| SC-CIT-13 | Reconstrucția snapshot-ului de mai sus, apoi repetare | aceleași coordonate și valori 100/100; repetarea raportează zero diferențe; etichetele nu intră în cheia sumelor |
| SC-CIT-14 | Snapshot contabil alterat cu +7, apoi reconstrucție | reconstrucția raportează diferența înainte de rescriere și citirea dă din nou finalul 80; un snapshot absent/vid citește integral cubul |
| SC-CIT-15 | DVI și ASM din SC-CIT-03/04 prin rapoartele contabile | DVI: D 4426 = 21, nu 121; ASM: D/C economic 40 și inversa −40, fără Transfer sau contraponderi |

RepartitorId din proiecția contabilă reprezintă Partener; gestiunea are
filtru și coordonată distincte, inclusiv în snapshot. Nu se reconstruiește
vechea dimensiune prin coalesce între partener și gestiune. MaterialId
corespunde Produs, iar UnitateId analizei organizaționale, nu lotului.

| ID | Scenariu | Așteptare |
|---|---|---|
| SC-CIT-16 | Fișa contului după închidere 100 și consum 20; filtru pe produs și gestiune | o postare C 20, sold curent 80; contrapartida cheltuielii este afișată; sursa este aceeași cu balanța |
| SC-CIT-17 | Jurnalul FCT 100 și BCS 20, cu BTR/inversă intercalate | câte o linie per postare, total debit 120 și credit 120; identitate postare × spațiu și tranzacție; BTR nu intră în rulaje |

### Fișă și jurnal: înlocuirea fixture-urilor din registre

Probele vechi `VerificaFisaJurnal` scriau direct perechi în RegistruContabil,
cu storno pozitiv artificial și identitate comună celor două laturi. Ele
se înlocuiesc prin documente NTC reale; nu se păstrează perechea ca formă a
jurnalului. Soft-delete-ul unei note de registru devine anularea comenzii.

| ID | Scenariu | Așteptare |
|---|---|---|
| SC-CIT-18 | Sold anterior 100, D 60, C 25, D/C pe același cont 40, C 10 și storno real −10 | fișă: 160 → 135 → 175 sau 95 → 135 → 125 → 135; la egalitate de dată ordinea totală a identităților decide numai soldul intermediar; 6 postări distincte |
| SC-CIT-19 | Aceeași fișă prin DataSourceLoader, pagini de câte 2 și filtru de grilă | paginile reproduc secvența integrală; filtrul nu recalculează soldul curent; ziua D/C 40 are net final 135 |
| SC-CIT-20 | Jurnalul din februarie pentru documentele de mai sus | 10 postări, debit 125 și credit 125; fiecare postare are tranzacție și identitate; fără filtru de dată intră și soldul anterior (încă 2 postări) |
| SC-CIT-21 | Tranzacție cu D serviciu 20 + D furnizor 30 = C stoc 50 | fișa stocului arată ambele conturi corespondente; nu inventează o contrapartidă unică |
| SC-CIT-22 | NTC D/C 15 operată și apoi anulată | cele 2 postări dispar împreună din fișă și jurnal; celelalte documente rămân; numărul și tipul NTC sunt atributele documentului real |

| ID | Scenariu | Așteptare |
|---|---|---|
| SC-CIT-23 | Invarianții pe baza completă și apoi eliminarea controlată a postărilor unei note, în tranzacție anulată | baza completă trece. Mutantul notei fără postări a ieșit odată cu acoperirea registru ↔ cub (TR-D9a, pasul 6) |
| SC-CIT-24 | — | depășit de 102: hosturile nu verifică la pornire, snapshot-ul se scrie numai din cub |

Snapshot-urile contabile se folosesc în ObjectSpace-urile de sistem.
Citirea securizată cumulează postările autorizate: snapshot-ul global nu
păstrează identitățile necesare permisiunilor pe rând și pe membru.
Egalitatea numărului de rânduri vizibile nu dovedește accesul la valorile lor.
Fișa compune SQL-ul generat din interogările securizate, inclusiv etichetele.
Această verificare de securitate nu înlocuiește diagnosticul de proveniență
din invarianți.

### Balanță și plan — fixture de documente reale

| ID | Scenariu | Așteptare |
|---|---|---|
| SC-CIT-26 | NTC: inițial D 100/C 300 pe contul A; februarie D 50/C 90; postare după perioadă 999 | inițial C 200, final C 240; la partenerii diferiți D 100 și C 340; contul cu numai inițial 70 rămâne; contul cu D/C 50 rămâne cu sold zero |
| SC-CIT-27 | Același cont: inițial fără partener 200, rulaj fără partener 30, cu partener 11 | fișa fără partener 230, cu partener 11, sintetic 241; paginile de 2 reproduc întreaga balanță în ambele moduri; filtrele se aplică înaintea agregării |
| SC-CIT-28 | Arbore: copil D 12, nepot D 4, alt copil C 30, mișcare proprie grupă D 1 | grupa D 17/C 30, net C 13; copil intermediar D 16; rădăcinile reproduc balanța plată; limitarea adâncimii nu schimbă sumele; un ciclu nu blochează citirea |
| SC-CIT-29 | Eticheta unui cont cu debit 17 ascunsă controlat, cu un copil debitor 5 | valoarea 17 rămâne în balanță și fișă cu etichetă goală; copilul devine rădăcină; tranzacția probei restaurează nomenclatorul |
| SC-CIT-25 | HTTP: NTC D 628 125 = C 401 100 + C 462 25, conturi temporare de test cu permisiuni | Admin/Cititor: 4 postări și fișă final 125; fără acces: liste goale; rândul 25 ascuns: 100; membrul Valoare ascuns: 0; clientul afișează ambele conturi corespondente |

### Extinderea citirilor (2026-09-25)

| ID | Scenariu | Așteptare |
|---|---|---|
| SC-CIT-30 | Balanță și fișă de la 0001-01-01, cu nota de 100 și rulajele din SC-CIT-18 | niciun sold anterior; final 135, aceeași sumă ca pe intervalul scenei; fără depășirea domeniului DateOnly |
| SC-CIT-31 | FCT 10 × 10, BTR 4 × 10 către altă gestiune, apoi storno BTR | cititorul de lot: sursa 6/60, destinația 4/40; după storno sursa 10/100, destinația zero; balanța rămâne D 100 |
| SC-CIT-32 | BCS 2 × 10 după recepția de mai sus | contul de stoc în magazie 8/80; cheltuiala la locul de consum 2/20; cheile nu se confundă |
| SC-CIT-33 | FCT stoc 100, NIR conex încă draft, numai privat | Suppliers: inițial 100 din ianuarie, final 200 după FCT nouă de 100; egalitate cu GLA; nu așteaptă registrul conexului |

PAR-V21 și DVI-V15 sunt aliniate cu coordonatele D8-B1: filtrul Partener
nu reconstruiește vechea dimensiune Repartitor. Pe 446 fără urmărire,
fișa nefiltrată are C 210/D 210 și sold zero; cea filtrată pe biroul vamal
este goală. Acoperirea bugetară a partidelor așteaptă alegerea D8-B6.

SC-CIT-34: alterarea debitului cu 1 refuză conservarea pe tranzacție/carte,
în tranzacție cu rollback. Eliminarea ambelor postări ale unei singure linii
NTC lasă tranzacția echilibrată și nu mai e văzută de niciun invariant:
acoperirea pe linie/latură era a registrelor și a ieșit odată cu ele
(TR-D9a, pasul 6; limită consemnată).

### Evaluarea operațională a loturilor — în lucru

| ID | Scenariu | Așteptare |
|---|---|---|
| SC-CIT-35 | Recepție 3/10, trei consumuri succesive de câte 1 | cub: ieșiri 3,33 + 3,34 + 3,33; lot final 0/0; evaluarea nu reia soldul registrului vechi |
| SC-CIT-36 | Deschidere numai în cub, lot 4/40, consum 2 și storno | consum 20; disponibil 2/20, apoi 4/40 |
| SC-CIT-37 | Același lot mutat între gestiuni; consum și inversări retroactive | soldurile sunt verificate pe lot/cont/produs/gestiune, în fiecare zi; retragerea intrării cu consum dependent refuză atomic |

În regimul dual, ASM păstrează separat soldul registrului pentru R și
soldul cubului pentru C. Absorbția Δ rămâne conform ASM-B6. Probele vechi
care consemnau reziduul produs de citirea registrului se înlocuiesc pentru
operații noi cu așteptarea 0/0; diagnosticul istoricului divergent rămâne.

### FIFO și raportul de stoc — înaintea implementării

| ID | Scenariu | Așteptare |
|---|---|---|
| SC-CIT-38 | Privat: două loturi de deschidere, 2/20 și 3/60; FCL 4, fără pin | DSC draft alocă 2 din primul și 2 din al doilea, chiar fără RegistruStoc; la operare cost 60, stoc rămas 1/20; ordinea este data deschiderii și ID |
| SC-CIT-39 | Aceeași deschidere, pin pe lotul al doilea și linie fără pin, la data înregistrării | pinul se alocă primul; FIFO scade alocarea deja făcută; nici o alocare din alt cont sau altă gestiune; data fizică mai veche nu ascunde intrarea contabilă disponibilă |
| SC-CIT-40 | Raport stoc după SC-CIT-31/32, etichete ascunse controlat și schimbarea politicii TipStoc | sumele și cheia lot/cont/produs/gestiune rămân; etichetele pot fi goale; costul unitar este Valoare/Cantitate; zero cantitativ nu provoacă împărțire la zero |

SC-CIT-39 include două mutații izolate, cu rollback: primul lot este mutat
pe alt cont, apoi pe altă gestiune. În fiecare caz, FIFO alocă numai cele
3 unități din al doilea lot; cele 2 unități de pe coordonata străină nu
devin disponibil pentru factură.

SC-CIT-25 se repetă prin HTTP după închiderea lui ianuarie și după
reconstrucție. În februarie, inițialul debitor este 125 pentru Admin/Cititor,
100 când postările de 25 sunt ascunse și 0 când membrul Valoare este ascuns.
Existența snapshot-ului global de 125 nu poate ocoli permisiunile.
Proba durabilă: `nou/tools/ProbeHttp/citiri-cub.py`, exclusiv pe baza
privată izolată `.CodexBCS`; rezultatul se consemnează după rulare.

### Validare contabil + stoc (2026-09-25)

Integral ambele profiluri: **2.899 bugetar / 3.989 privat OK**, zero FAIL,
exit 0, build fără avertismente; `run-verificari/20260925-031339-966/rezultat.json`.
SC-CIT-25 HTTP a trecut înainte/după închidere și după reconstrucție, în două
execuții succesive; `run-verificari/trd8-http-final.log`.
Raportul de stoc a fost verificat prin HTTP și browser pe FCT → NIR:
cont 371, 3 bucăți/10 lei, cost unitar 3,333333. Client build și regenerarea
repetată a celor trei artefacte fără drift sunt verzi.
Aceste probe nu închid suprafețele încă neportate ale TR-D8.

### Partide comerciale bugetare — decizia 100, înaintea probelor

SC-CIT-41: seed-ul activează exact 401.01.00, 404.01.00 și 411.01.01
DinSeed fără rol SAF-T. Un cont trecut manual la UrmarestePartide=false
rămâne astfel la re-seed; după rollback seed-ul este idempotent.
SC-CIT-42: FCT 100, PLT 40 → partida facturii −60, plata 0;
storno stingerii redeschide factura −100 și plata +40. Același ciclu FCL/INC
are semne opuse. Cele două profiluri au aceeași nominalizare.
SC-CIT-43: eliminarea unității și a partenerului de pe creditul FCT 100,
refuză invarianții cu CITIRE_PARTIDE_INCOMPLETE. Nu se reconstruiește din sold. O notă pe 401
fără partener explicit rămâne validă conform T-D3; baza fiscală DVI nu
este partidă. Mutanții se probează în tranzacții cu rollback.
SC-CIT-44: deschidere cu 401 creditor 100 detaliat 60+40 pentru același
furnizor; NTC debit 401 de 75 consumă 60+15, rămâne −25; storno NTC
redeschide −60/−40. Ambelor profiluri li se aplică aceeași cheie.
SC-CIT-45: FCT de imobilizare 100 → credit 404 cu partidă −100,
RolTert bugetar rămâne Niciunul; inversarea readuce restul zero.

SC-NTC-07…11, 13, 15…17, 20 și 22 devin probe pe ambele profiluri,
cu 401.01.00/628.00.00 în bugetar. SC-NTC-12 (419) rămâne privat.

SC-CIT-46: FCT 100 → PLT automată 100 nominalizată pe factura sursă.
DELETE împerechere în perioada deschisă lasă FCT −100 și PLT +100 pe
propria partidă, prin Transfer; postările Operare rămân identice.
PLT manuală 40 poate stinge factura, iar ștergerea ei reface −100/+40.
Storno PLT automată după desfacere aduce partida proprie la zero și
păstrează datoria facturii −100. Transferul de desfacere este atribuit
nominalizării inițiale, astfel încât inversarea ei să-i compenseze efectul.
Aceeași PLT automată, după desfacere, este reîmperecheată manual cu 40
pe factura inițială și apoi desfăcută din nou. FCT revine la −100,
PLT la +100; stornarea plății aduce numai partida proprie la zero.
Transferul manual și inversa lui se compensează fără a fi inverse ale
nominalizării automate originale.
SC-CIT-47: desfacerea nominalizării automate în februarie păstrează soldul
zero al facturii în ianuarie și o redeschide la −100 în februarie;
storno ulterior al plății închide partida ei proprie fără a dubla efectul.
Storno direct în februarie cu legătura automată încă activă inversează
nominalizarea o singură dată și redeschide factura la −100.

SC-CIT-48: ștergerea CRUD directă a unei împerecheri care atinge cubul
se refuză; comanda de ștergere rămâne disponibilă cu drept Delete și
execută atomic desfacerea pe cub și ștergerea legăturii în perioada deschisă.

Validare după 100: **3.036 bugetar / 4.026 privat OK**, zero FAIL, exit 0,
build fără avertismente; `run-verificari/20260925-073729-162/rezultat.json`.
SC-CIT-46/48 trec prin HTTP (404/403/204), apoi separat prin acțiunea XAF,
cu verificarea FCT −100 / PLT +100 în cub și PLT zero după anulare.
`nou/tools/ProbeHttp/partide-cub.py`, loguri `trd8-partide-http` și
`trd8-partide-xaf`; după probă nu rămân documente sau parteneri activi.
Metadata/OpenAPI/types și build-ul clientului sunt verzi, regenerarea
repetată păstrează cele trei hash-uri. Raportul/snapshot-ul partidelor și
restul regulii de oprire TR-D8 rămân deschise.

### Împerecherea cu efect obligatoriu — decizia 101, înaintea codului

SC-CIT-55: PLT 70 către partener, NTC 581 = 531 cu repartitor cont propriu:
cererea de împerechere 50 se refuză atomic; nicio legătură sau postare nouă,
restul plății rămâne 70. Plata nu apare între candidații acestei note.

SC-CIT-56: INC 100 pe contul de client, NTC debit furnizor 60 pentru același
partener: contul diferit refuză împerecherea; restul încasării rămâne 100.
Nota corectă, cu debit pe contul clientului, permite stingerea pe acel cont.

SC-CIT-57: FCT 100, NTC cu nominalizare 75 deja scrisă: legătura manuală
75 este permisă, restul rămâne 25, fără Transfer suplimentar. O a doua
legătură pentru aceeași nominalizare este refuzată. Ștergerea acestei legături
manuale nu inversează operarea NTC; storno NTC redeschide FCT la 100.

SC-CIT-58: disponibilul plății 100, factura are două partide pe conturi
separate (40 + 60), numai contul de 40 este comun: cererea 100 se refuză;
cererea 40 mută exact 40. Dacă sunt mai multe ținte eligibile și comanda
pe document nu le distinge, refuz explicit de ambiguitate, fără alegerea
postării cu valoare maximă.

SC-CIT-59: refuzul repetat al unei comenzi nu lasă obiecte noi care să poată fi comise
accidental prin salvarea ulterioară a aceluiași ObjectSpace.

SC-CIT-60: FCT 100, NTC 75 nominalizată: asociere 50 apoi asociere 25,
fără transferuri. Ștergerea primei păstrează restul 25. O plată distinctă
de 25, împerecheată prin două transferuri de 10 și 15, lasă rest zero;
ștergerea celeilalte asocieri nu desface aceste transferuri. Desfacerea
ambelor transferuri reface rest 25. Pe aceeași pereche, fiecare desfacere
inversează exact propriul transfer, inclusiv când se șterge primul.

SC-CIT-61: stingerea retroactivă nu poate consuma un sold redevenit disponibil
numai după desfacerea ulterioară: la fiecare dată deja scrisă, restul ambelor
partide rămâne în sensul său. Refuz înaintea materializării.

Completare SC-CIT-43: cazul contului devenit manual, fără urmărire, cu
factura fără unități, era refuzat de `CITIRE_PARTIDE_POLITICA`; invariantul
compara totalul din antet cu partidele și a ieșit odată cu el (TR-D9a, D9-A6
I8). Rămâne cazul cu urmărire activă și unități lipsă. SC-CIT-58 probează ambiguitatea
în funcția pură; limita 40 din factura 40 + 60 trece prin documente reale.
NTC bugetar nu culege toate analizele cerute de 404, deci nu este folosit
ca fixture artificial pentru această ramură.

SC-CIT-62: FCT cu stoc 50 + TVA capitalizată 9,5 + serviciu 100,
NIR încă Draft: PLT automată 159,5 nominalizează întreaga datorie din cub;
restul sursei și al plății devin zero. Aceeași regulă pentru factura numai
cu stoc 100, indiferent de operarea întârziată a NIR-ului. Probele integrale
FCT/împerechere automată și F27-R3c exercită lanțul real pe ambele profiluri.

Completare SC-CIT-61/62: sursă 159,5 cu stingere manuală ulterioară 60,
plată automată retroactivă 30, apoi 100: a doua nominalizează numai 69,5,
restul de 30,5 rămâne pe partida proprie. Disponibilul nominalizării este
minimul păstrat la fiecare dată deja scrisă, inclusiv după data operării.

SC-CIT-63: PLT 30 pe 401 și INC 30 pe 4111 pentru același partener
nu se sting direct între ele: ambele sensuri ale cererii 10 se refuză,
resturile rămân 30 și nu se scrie nicio legătură. Conturile diferite
cer nota de compensare, conform 101(b); F19-D16 probează această limită.

SC-CIT-64: RDC fără factură de nominalizat, venit stornat 100 + TVA 21,
cost revenit 30: totalul documentului rămâne −121, partida este datorie
121; raportul și lista de datorii o includ cu 121, fără costul de 30.
SC-CIT-65: fără obiect după tăiere. Proba compara totalul scris pe antet cu
partidele cubului (`CITIRE_PARTIDE_POLITICA`); totalul nu mai e scris, iar
restul și totalul se citesc numai din postări (TR-D9a, D9-D10, D9-A6 I8).
SC-CIT-62, precizare: soldul efectiv citit la data plății rămâne 121;
disponibilul pe cont este separat, 61 după rezervarea stingerii viitoare 60.

Completare SC-CIT-58: FCT imobilizare 500 pe 404 și TVA 105 pe 401,
PLT automată 605 pe 401: legătura confirmă numai nominalizarea 105;
rest FCT 500 pe 404 și rest PLT 500 pe propria partidă 401.

SC-CIT-66 (101-r1, D-2): FCT 100 la 5 ianuarie, PLT 100 împerecheată manual
la 15 ianuarie, NTC la 7 ianuarie cu D 401/furnizor 100. Nota nu
nominalizează factura (disponibilul ei temporal este 0, fiindcă e stinsă la
15), deschide partidă proprie 100; factura are −100 la 7 ianuarie și 0 de la
15, niciodată creanță. Același calcul temporal servește PLT automată,
transferul manual, stingerea partidei inițiale și dependenții (C-D5).

SC-CIT-67 (101-r1, D-3): FCT 100, PLT automată 100, legătura ștearsă prin
comandă: PLT are +100 proprie numai prin transferul desfacerii. O INC 100
de la același furnizor, pe 401, o stinge: panoul oferă 100 și comanda
acceptă, restul PLT ajunge la 0. Varianta NTC (C 401/furnizor 100 operată
înaintea desfacerii) are același rezultat. Ținta comenzii este orice unitate
proprie a stinsului, inclusiv cea deschisă prin Transfer.

SC-CIT-68 (review-ul advers C102): FCT 100, PLT automată, legătura desfăcută
la 10 ianuarie, deci partida proprie a plății există numai de la 10. O a
doua FCT 100: împerecherea de 50 datată 7 ianuarie se refuză
(`PARTIDA_PROPRIE_INSUFICIENTA`, punctul de la data cerută intră în calcul);
aceeași împerechere la 10 ianuarie trece: FCT2 rest 50, plata 50.

### Validare finală partide — 2026-09-25

SC-CIT-49…65 și integralele sunt verzi: **3.107 bugetar / 4.103 privat OK**,
zero FAIL, exit 0. Dovezile și încercările intermediare sunt în
[review-ul propriu](../tr-d8-review-codex.md). Raportul/snapshot-ul trec
matricea HTTP pe rând/membru înainte și după închidere/reconstrucție;
împerecherea 40 din 100 lasă 60, cererea 61 se refuză atomic, iar ștergerea
și anularea sunt verificate independent în cub. Browserul confirmă raportul
cu parteneri distincți și disponibilul 60 din panou. 101-r1 este închisă;
restul TR-D8 și matricea generală de securitate rămân delimitate în review.

### Review fiscal 103 — scenarii înaintea codului, 2026-09-26

SC-CIT-88: exportul lunar D300/D394 pentru FCT 100/21 conține amprenta
faptelor citite, califică formularul și perioada. După operarea unei alte
FCT 25/5,25 în aceeași perioadă, confirmarea versiunii vechi este refuzată
cu `DEPUNERE_VERSIUNE_DEPASITA`, fără înregistrare de depunere. Exportul nou
125/26,25 poate fi confirmat; repetarea aceleiași confirmări întoarce aceeași
identitate. Confirmarea compară amprenta sub blocajul exclusiv. O altă
perioadă sau un alt formular nu poate reutiliza versiunea. O schimbare cu
aceleași totaluri, dar alte identități, schimbă versiunea. Citirea raportului
și a amprentei folosește același snapshot tranzacțional; o versiune calculată
pe date mascate/refuzate nu confirmă setul complet de fapte.
Exportul disponibil în această felie este DTO-ul JSON cu amprentă; nu este
fișier XML ANAF. UI păstrează versiunea exportului ales, nu o etichetă liberă.

SC-CIT-89: pentru o postare fiscală reală, fiecare eliminare a unei calificări
sau a unui reper obligatoriu este refuzată de CHECK-ul bazei, inclusiv lipsa
datei primirii la achiziție. Tranzacția probei este anulată; faptele inițiale
rămân intacte. Invarianții ModelCheck păstrează probele de calificări
incompatibile între roluri, duplicare și proveniența inversei.

### R6 — implementat și verificat (2026-09-28)

Runner: `ScenariiTvaIntervale`, selectoarele `FISCALE` / `CITIRI`.
Probe secured: `nou/tools/ProbeHttp/tva-intervale.py`. Dovezile de rulare și
verificarea XAF/React sunt în contract, secțiunea „Verificarea implementării”.

Contract: `../tr-d8-tva-intervale-contract.md`, R6-B1…B8. Owner-ul a aprobat
M1(B) și M7 (103i). Probele de schimbare a cotei folosesc datele din
iulie/august/septembrie 2025. Intervalele se configurează explicit pe
tipurile fixture-ului, nu pe rândurile `DinSeed`, ca probele să nu depindă
de seed. Seed-ul are proba lui separată, SC-CIT-93a. Taxele intenționat diferite
folosesc `TolerantaTaxa = null`; gardienii existenți nu sunt dezactivați.

SC-CIT-90: la 28 august configurăm tipul 19% până la 31 iulie inclusiv și
tipul 21% de la 1 august inclusiv. Factura emisă la 10 august pentru o
livrare/exigibilitate la 31 iulie, bază 100 și taxă 19: fără avertisment.
Factura cu exigibilitate la 5 august, operată cu 100/19: avertisment de
interval, dar operare reușită. Raportul o arată la Emise. Aceleași date pe
FCT dau avertisment la Primite, cu orientare către furnizor. Modificarea
retroactivă a intervalului descoperă și faptele scrise înainte de 28 august.
O corecție tehnică 100/19 → 100/21, când D300 august nu este depus,
păstrează august fără regularizare: baza netă 100, taxa 21; D394 are o
factură. Originalul și inversa formează un singur caz „compensat”, absent
din raportul implicit și vizibil la cererea istoricului. Inversa nu primește
avertisment propriu. Aceeași compensare este recunoscută când inversa este
în septembrie, chiar dacă filtrul raportului selectează exigibilitatea din
august: raportul arată starea curentă. O altă factură −100/−19 fără legătură
`InversaDinId` nu închide cazul original. Documentul distinct de corecție
rămâne cazul separat SC-CIT-83.

SC-CIT-90a (M3): L3, factură 10 august / exigibilitate 31 iulie, două reguli
și tipuri eligibile 19% până la 31 iulie și 21% de la 1 august: propune 19%.
Fără exigibilitate explicită propune 21%. Dacă tipul istoric este inactiv,
nu propune nici tipul inactiv, nici 21% în afara intervalului; `Explica`
arată motivul lipsei candidatului. Alegerea existentă nu este înlocuită
automat la schimbarea datei. După o editare a calificării care generează
impact asupra primului caz, raportul filtrat pe iulie îl include, cel pe
august nu îl include, chiar dacă data facturii este în august.

SC-CIT-90b (M2): livrare iulie 100/19; retur parțial RDC în septembrie
−40/−7,60, cotă 19. TVA ajustării rămâne −7,60 în septembrie; nu se mută în
iulie și nu devine −8,40 la 21%. În delimitarea minimă R6, fără proveniență
fiscală rezolvată, apare `TVA_AJUSTARE_FARA_SURSA`, fără avertismentul de
interval și fără avertisment aritmetic. Aceeași așteptare pentru RLF și
reducere FCT/FCL nemarcată ca avans. Varianta −40/−8,40 cu cotă 19 primește
suplimentar `TVA_TAXA_DIFERITA_DE_COTA`; varianta cotă 21 și taxă −8,40
rămâne fără sursă verificabilă, chiar dacă aritmetica este corectă.

SC-CIT-91: factura operată 100/19 păstrează calificarea și taxa 19 după
editarea tipului pe loc la 21. Raportul arată diferența 19 versus 21 și
documentul; nu modifică sumele și nu declară greșită aritmetica faptului
înghețat 100 × 19% = 19. Draftul cu același tip și taxă salvată 19 păstrează
19 cu marcajul `TvaCules`, adică o taxă culeasă de operator, păstrează 19
la operare, dar îngheață cota 21. `TVA_TAXA_DIFERITA_DE_COTA` apare atât în
raportul draftului, cât și la operare și ulterior pe fapt. Taxa 19 este
raportată pe calificarea 21%, fără corecție automată. Draftul cu taxa 19
calculată, deci nemarcată, se operează la 100/21/21, fără avertisment
aritmetic. Pe draftul marcat, acțiunea L3 „Recalculează TVA la cotă” dă 21,
stinge marcajul și elimină abaterea aritmetică.
Ieșirea exigibilității din interval rămâne avertisment independent.
Regim/DeImport apar și ele în impactul istoric/curent. Dacă există numai
maparea SAF-T pentru calificarea veche, noua calificare primește diagnosticul
de lipsă a mapării 103(f); istoricul păstrează maparea corespunzătoare.
Proba citește toate codurile raportului, fără filtru de avertismente SAF-T.
Pe Bugetar, FCT fără mapare nu primește `TipTvaFaraCodSaft`, nici în draft,
nici la operare sau în raportul faptului. Pe Privat, aceeași lipsă produce
avertisment; mapările explicite ale fixture-ului îl elimină. Mesajele
operării identifică linia proprie prin poziție, fără GUID.
HTTP probează și PUT cu baza neschimbată și `ValoareTva = null`: păstrează
taxa culeasă 19 inclusiv la operare după schimbarea cotei la 21; recalculul
explicit ulterior produce 21 și șterge marcajul.

SC-CIT-91a: trei linii normale cu baze 0,02 la 21% au taxa pe document
0,01, repartizată 0,01/0/0 (ordinea urmează repartizarea stabilă). Fără
avertisment pentru repartizare. La baza 100 și cota 21, taxele 21,01 și
21,02 au abateri 0,01 și 0,02: prima fără avertisment, a doua cu avertisment.
Capitalizat 100 net → 121 brut nu primește eroare pentru lipsa taxei
separate; scutit 100/0 la fel. Taxare inversă deductibilă 100: Taxă 21 și
Autocolectare 21, fără comparație cu 42; taxare inversă colectată 100/0,
fără eroare pentru taxa zero. Cu toleranța motorului configurată la zero,
100/cotă 21/taxă 19 păstrează refuzul existent și absența efectelor.

SC-CIT-91b (B, 103i): tranzițiile marcajului `TvaCules`.
- Taxa 19,50 tastată pe linia 100 la 19% dă `TvaCules` = true.
- Schimbarea cantității sau a tipului TVA recalculează taxa și dă false.
- Taxa adusă la 0 în ecran revine la cotă și dă false.
- Un 0 trimis prin `Apply` pe un tip cu taxă rămâne refuzat, ca înainte.
- Linia nouă și linia clonată în conex au false.
- Draftul de corecție copiază taxa și marcajul liniei sursă.
- RDC/RLF păstrează marcajul la inversarea semnului.
- O scriere directă `TvaCules = true` cu `ValoareTva = 0` e refuzată de
  CHECK-ul bazei (23514).
- Migrația marchează drafturile existente cu taxă nenulă și lasă neatinse
  liniile operate.

SC-CIT-92: avans iulie 100/19; factura finală august are linie 300/63 și
regularizare −100/−19, marcată prin politică și referită la linia avansului. Fără
avertisment: cota 19 a regularizării este cea a avansului. D300 iulie TVA 19,
august TVA 44; D394 iulie 100/19/o factură, august două calificări,
300/63 la 21 și −100/−19 la 19, același document fiscal final. Taxa negativă
nu se mută în iulie. Regularizarea −100/−21, culeasă cu cotă 21%, produce
avertisment de calificare diferită și păstrează cifrele culese (august 42),
fără reparare automată.
Fără referință: avertisment distinct, fără refuz; fără marca de politică,
simbolul contului nu activează mecanismul. Linia pozitivă marcată 100/19 a
avansului nu cere referință; se verifică normal pe interval. Două facturi
finale care referă aceeași linie pentru −40/−7,60 și −60/−11,40 nu sunt
refuzate pentru reutilizarea referinței; nu se pretinde calculul restului.
Se probează atât FCL cât și FCT. Varianta greșită −100/−21 distinge:
cotă 21 → calificare diferită de sursă; cotă 19 → abatere aritmetică.

SC-CIT-93: tip fără ambele limite, 100/21: fără avertisment de interval.
O singură limită testează numai capătul definit; chiar în ziua limitei
nu există abatere. Verificarea intervalului nu oprește nicio operare fiscală.
Intervalul inversat este refuz de configurare, nu refuz fiscal.

SC-CIT-93a (M7, 103i): N19/TI19/CAP19 includ
31.07.2025 și exclud 01.08.2025; tipurile 21/11 din R6-B8 includ 01.08.2025
și exclud 31.07.2025; N9 include 31.07.2026 și exclude 01.08.2026. Re-seed-ul
aliniază intervalele `DinSeed`, păstrează intervalele rândului manual și nu
recreează un rând refuzat prin `RefuzSeed`. Trecerea zilei nu schimbă `Activ`.

SC-CIT-94: avans cu două linii la cote diferite, 100/19 și 100/9.
Regularizările −100/−19 și −100/−9 referă fiecare linia corespunzătoare:
fără avertisment, total regularizat −200/−28. Prima referită intenționat
la linia de 9% produce `TVA_AVANS_CALIFICARE_DIFERITA`, fără alegerea altei
surse. Sursa neoperată, nemarcată, de alt partener/sens ori din propria
factură produce `TVA_AVANS_REFERINTA_INVALIDA`; lipsa sursei produce
`TVA_AVANS_FARA_REFERINTA`.

SC-CIT-94a: avans stornat sau înlocuit prin corecție tehnică, referința
draftului rămasă pe linia veche: `TVA_AVANS_SURSA_COMPENSATA`, fără mutare
automată către linia nouă. Alegerea explicită a noii linii cu fapt activ
elimină avertismentul dacă datele corespund. Proba de model verifică o
singură coloană/FK pentru frunzele FCT/FCL. Ștergerea fizică a unei surse
draft referite se refuză atomic, 422; nu șterge linia care o referă.

SC-CIT-94b: accesul la sursă este retras după salvarea referinței. Mesajul
operării conține codul și linia proprie, fără cotă, dată, număr sau valori
ale sursei. Raportul secured nu dezvăluie sursa ascunsă, inclusiv prin
explicații sau totaluri; membrii refuzați rămân protejați. Pentru cititor,
sursa invizibilă și cea inexistentă au același răspuns. Lookup și DTO/Apply
respectă filtrele de acces; React nu primește un editor nou al documentului.
