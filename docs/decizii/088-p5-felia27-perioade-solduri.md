# 88. Pasul 5, felia 27 — Perioada fiscală ca lanț, închiderea ca comandă cu acceptare conștientă, soldurile și partidele deschise materializate la închidere, data înregistrării ca reper al registrelor, perioada de declarare ca reper fiscal, corecția legată cu motiv

- **Data**: 2026-09-17
- **Stare**: activă (amendează 31d — împerecherea nu mai e link care se șterge liber; închide 79-r3; întărește 14 și 42c; F27-r3 n-a fost nevoie să se activeze)
- **Docs**: `docs/api/p5-felia27-perioade-solduri-contract.md` (F27-D1…D10, §Amendamente, §Pașii cu notele *Executat 0…8b*, §„Ce NU intră"), `docs/api/p5-felia27-pas0-spike.md`, `nou/.../Module/BusinessObjects/Nomenclatoare/PerioadaFiscala.cs` (`PerioadaFiscala`, `InchiderePerioada`), `nou/.../Module/BusinessObjects/Registre/SolduriPerioada.cs` (`SoldPerioadaContabil`, `SoldPerioadaStoc`, `PartidaDeschisa`), `nou/.../Module/Motor/{PerioadaService,SolduriService,CorectieService,TranzactieComanda,GardianPerioada,GardianEditare,RegistruTvaService,ImperechereService,AmortizareService}.cs`, `nou/.../Module/Proiectii/{ContabilProiectii,StocProiectii,TvaProiectii,ImperecheriProiectii}.cs`, `nou/.../Module/BusinessObjects/Politici/Politici.cs` (`PoliticaInchidere`, `PoliticaTva.DeclarareIntarziata`), `nou/.../Module/Api/Perioade/`, `nou/.../WebApi/API/Conta/{Perioade,Corectie,Imperecheri,SoldPartener,DocumenteCuRest}Controller.cs`, `nou/Atlas.Conta.Client/src/felii/perioade/`, `nou/Atlas.Conta.Client/src/felii/raportare/SoldParteneri.tsx`, `nou/Atlas.Conta.Client/src/felii/politici/PoliticiInchidere.tsx`, `nou/Atlas.Conta.Client/src/nucleu/CorectieDocument.tsx`, `nou/tools/ModelCheck/Program.cs` (`PER-V*`, `SOL-C*`, `DIR-V*`, `COR-V*`, `PAR-V*`, `ACC-V*`, `AMO-V*`, `F27-R*`, `F27-RA*`), `nou/tools/Import1C/Program.cs` (`--inchide-lunile`), `nou/tools/ProbeHttp/refuzuri.ps1`; migrațiile `20260916123442_F27Pas1PerioadaLant`, `20260916131506_F27Pas2SolduriPerioada`, `20260916184148_F27Pas3DataInregistrare`, `20260916193817_F27Pas4PerioadaDeclarare`, `20260916201658_F27Pas4bLuniAmortizate`, `20260916210018_F27Pas5Corectie`, `20260916220304_F27Pas6PartideImperecheri`, `20260916231038_F27Pas7PoliticaInchidere`

## Regula durabilă

**Perioada fiscală e un LANȚ, nu un flag; închiderea e o comandă a
motorului, iar ce se închide se materializează.**

(a) **Lanțul.** Perioadele sunt verigi contigue `(An, Luna)`, unice în
schemă. Închiderea lui P cere P−1 închisă; **perioada absentă e închisă prin
absență**, deci capetele lanțului sunt date de perioadele DEFINITE, iar
perioadele se definesc înainte, ca nomenclator. Redeschiderea lui P cere P+1
deschisă sau absentă: se redeschide numai ultima închisă, cascada e
explicită. `Inchisa`, `InchisaLa`, `InchisaPrimaOara` sunt ALE MOTORULUI —
`GardianEditare` le refuză pe ușa securizată ca pe registre (14/42a); `An` și
`Luna` se creează liber, se schimbă doar cât perioada n-are istoric, iar o
perioadă cu istoric de închideri sau închisă nu se șterge. Istoricul e
append-only: `InchiderePerioada` (`Fel` Închidere | Redeschidere, `La`,
`De`, `Motiv`, `Acceptari`) — cine, când, ce a acceptat. `InchisaPrimaOara`
NU se șterge la redeschidere: ce a fost declarat o dată rămâne reperul (g).

(b) **Închiderea = verificare → acceptare conștientă → închidere, în aceeași
tranzacție.** Verificarea întoarce constatări tipizate cu `Cheie` stabilă
(`ITV-LIPSA`, `AMO-LIPSA`, `DRAFT-IN-PERIOADA:{id}`, `REST-SCADENT:{id}`),
`Fel`, `Severitate`, `Text`, `Obiect`. Comanda o rulează DIN NOU în
tranzacția ei: un blocant refuză mereu; un avertisment a cărui cheie nu e
acceptată refuză cu LISTA ÎNTREAGĂ, ca operatorul să accepte constatări
concrete, nu un flag de forțare. „Verifică, închide, apoi raportează" e
respins: raportul de după nu mai poate schimba nimic. **Severitatea e
POLITICĂ** (`PoliticaInchidere`, un rând per fel, `Blocant | Avertisment |
Ignorat`, aliniată de seed pe `DinSeed`); `Ignorat` nu emite ȘI NU CAUTĂ.
Blocantele STRUCTURALE stau în cod și nu se configurează: P−1 neînchisă,
redeschidere cu P+1 închisă, snapshot inconsistent. O familie de constatări
peste plafonul de listare (200) primește un rând de REZUMAT cu cheie proprie
`{FEL}:REZUMAT`, cu **severitatea familiei, din aceeași politică**, care
spune câte rânduri nelistate acoperă: acceptarea lui e o ACCEPTARE ÎN BLOC,
conștientă pentru că poartă cifra și severitatea. **Un fapt, o constatare**:
un draft deja raportat de o altă constatare a aceleiași verificări nu se mai
raportează a doua oară — dar numai dacă a fost raportat EFECTIV (pe `Ignorat`
familia nu se caută, deci faptul reapare pe calea lui proprie).

(c) **Redeschiderea cere MOTIV** și e tot comandă: șterge snapshot-urile și
partidele lui P, reconstruiește referința anterioară prin `SUM` integral și
scrie istoricul. Ce a fost declarat rămâne declarat: conținutul scris după
prima închidere devine rectificativă la re-închidere (g), iar ecranele spun
că perioada e redeschisă.

(d) **Soldurile se materializează DOAR pe perioadele de REFERINȚĂ** = ultima
perioadă închisă SAU un decembrie închis. `SoldPerioadaContabil` (contul +
cele opt dimensiuni ale laturii, `Debit` și `Credit` cumulate SEPARAT) și
`SoldPerioadaStoc` (lot × repartitor × tip stoc, cantitate și valoare) țin
**cheia completă a ATOMULUI, nu a raportului**: orice raport e rollup aditiv,
iar netarea se face abia la nivelul cerut (66d). Închiderea lui P scrie
snapshot(P) = snapshot(P−1) + rulaje(P) și ELIMINĂ snapshot-urile tuturor
referințelor de dinaintea lui P care nu sunt decembrie. Cheile integral zero
se OMIT: cheia absentă = zero pentru orice consumator. Invariantul, probat pe
ambele profiluri: rânduri de snapshot există ⇔ perioada e de referință, și
pentru fiecare referință snapshot(P) = `SUM(registru, dată ≤ sfârșit P)` la
cent, pe fiecare cheie. Snapshot-urile NU sunt registre și nu sunt urme (I):
sunt proiecții persistate, derivabile integral; reconstrucția RAPORTEAZĂ
diferențele înainte să rescrie (35b). Registrele nu se ating niciodată.

(e) **Un singur serviciu de citire a soldurilor.** `Motor/SolduriService`
răspunde „soldul cheilor X la data d" ca snapshot(ultima referință ≤ d) +
rulajele de după ea. Îl consumă balanța (deci și balanța pliată și inițialul
de cont al SAF-T), fișa de cont, soldul de stoc, `SolduriLaData`/`Sold`/
`AlocaFifoTolerant`/`VerificaSoldIntermediar` din motor și închiderea de TVA.
Niciun hot path nu mai citește rânduri de dinaintea ultimei referințe; costul
oricărei citiri e mărginit de fereastra deschisă, nu de vechimea bazei.
Serviciul e scris ca serviciu cu întrebări economice (chei + dată → solduri),
nu ca `IQueryable` expus — e primul adaptor al contractului IM-D4.

(f) **`Document.DataInregistrare` — registrele se scriu la data
înregistrării.** Câmp pe bază, editabil în Draft, implicit `= Data`, cerut
`>= Data`. Gardianul de perioadă, registrele (contabil, stoc, imobilizări),
nașterea lotului și capătul stornoului trec pe ea. `Data` rămâne A
DOCUMENTULUI FIZIC: numerotarea, scadența, cronologia seriilor și identitatea
fiscală. **Documentul întârziat e flux NORMAL, nu corecție**: `Data` în
perioadă închisă + `DataInregistrare` în cea deschisă se operează fără motiv
și fără să atingă snapshot-ul; apare în luna în care a intrat în evidență, cu
numărul și data lui fizică vizibile. Ordinea FIFO e ordinea INTRĂRII ÎN
EVIDENȚĂ — singura compatibilă cu „sold ≥ 0 la orice dată" (13, VI). Datele
fișei de imobilizare (punerea în funcțiune, ieșirea) rămân pe data fizică:
sunt ale fișei, nu ale registrului.

(g) **`PerioadaDeclarare` pe registrul fiscal; rectificativa e o consecință,
nu un flag.** `RegistruTva` poartă perioada de DECLARARE (an + lună) distinctă
de `Data` (data faptului fiscal) și `ScrisLa` (momentul scrierii RÂNDULUI).
Jurnalele, D300, D394 și SAF-T filtrează pe perioada de declarare, printr-un
singur helper. Regula de completare: perioada lui `Data` dacă e deschisă;
dacă e închisă, decide POLITICA (`PoliticaTva.DeclarareIntarziata`:
`PerioadaInregistrarii` — implicit pe deductibil, art. 301 — sau
`PerioadaFaptului` — implicit pe colectat); o perioadă a faptului NEDEFINITĂ
cade pe perioada înregistrării indiferent de politică, ca rândul să aibă o
lună care există și se poate închide. **Conținutul de rectificativă pentru P
= rândurile cu perioada de declarare P scrise după `InchisaPrimaOara` a lui
P.** Nu există flag: e derivat din două timestamp-uri. Stornoul rămâne fapt
al perioadei stornării (JT-D5); excepția e corecția cu motiv (h).
Consecință acceptată: perioada fiscală E luna, deci un raport fiscal nu mai
poate fi tăiat pe o fereastră de zile.

(h) **Corecția în perioadă închisă = storno legat + document nou, cu motiv.**
Nu există editare în loc a unui document operat, nicăieri (55a). În perioadă
deschisă: anulare sau corecție directă fără dependenți, altfel storno (14).
În perioadă închisă: comanda `corecteaza` = stornarea originalului la o dată
din perioada deschisă + un Draft de ACELAȘI tip concret, cu culegerea copiată
și cu `Numar`/`Data` ale documentului fizic păstrate, legat 1:1 prin
`CorecteazaId` și purtând `MotivCorectie` ∈ `EroareMateriala | FaptNou`.
Legătura e a motorului, verificată la FIECARE commit: motiv prezent, original
existent și `Stornat`, un original nu se corectează de două ori. **Motivul
decide EFECTUL FISCAL, nu contarea**: `EroareMateriala` ⇒ rândurile de TVA
ale stornoului ȘI ale documentului nou primesc perioada de declarare a
ORIGINALULUI, deci diferența apare ca rectificativă pe luna originalului;
`FaptNou` ⇒ regula normală (g). Contarea rămâne cea din politică, iar
reclasificarea pe rezultatul reportat a erorilor semnificative rămâne decizia
contabilului: motorul nu judecă semnificația. Snapshot-ul perioadei închise
nu se atinge — situațiile ei rămân cum au fost la închidere, adevărul
corectat se vede din prima perioadă deschisă încolo.

(i) **Totalul e fapt scris la operare; partidele deschise se materializează
la închidere.** `Document.TotalStingere` = Σ liniilor de creanță, scris de
motor la operare, `null` la anulare, neatins la storno — nu se mai
recalculează la fiecare citire (întărește 42c: nici TS, nici proiecția nu
calculează total/rest). `PartidaDeschisa` (`An`, `Luna`, `DocumentId`,
`Rest`) se scrie la închidere, pe aceeași regulă de referință ca
snapshot-urile, pentru toate documentele cu rest la sfârșitul perioadei; la
31.12 lista partidelor deschise E arieratele la nivel de document, înghețate,
fără recalcul ulterior. Documentele cu rest la o dată = partidele referinței
+ documentele operate în fereastra deschisă + **documentele atinse de o
împerechere din fereastra deschisă** (fără a treia ramură, o desfacere ar
lăsa restul nevăzut).

(j) **Împerecherea e FAPT DATAT** (amendează 31d: nu mai e un link care se
șterge liber). `Imperechere.Data` se persistă la creare: automat = data
înregistrării stingătorului, manual = data cerută; în ambele cazuri ≥
înregistrarea AMBELOR documente și în perioadă deschisă — gardianul de
perioadă intră și pe această cale, iar stingerea automată își aliniază
secundarul ca invariantul de ordine să fie satisfăcut prin construcție. Ștergerea directă rămâne doar în
perioadă deschisă; **o împerechere dintr-o perioadă închisă se desface prin
RÂND INVERS** (sumă negativă, dată în perioada deschisă, legătură 1:1 spre
originalul ei), scris de motor la stornarea unui document împerecheat și
disponibil ca acțiune explicită. Ștergerea originalului unei împerecheri
desfăcute și ștergerea rândului invers se refuză pe fond: ar reînvia tăcut o
stingere dintr-o perioadă închisă. Asignările și partidele însumează
ALGEBRIC, ca registrele.

(k) **Amortizarea întârziată se recuperează, nu se pierde.** Luna M postează
lunile DATORATE (de la luna de după punerea în funcțiune) minus cele deja
ACOPERITE (coloana `Luni` a rândurilor de amortizare), plafonate la durata
RĂMASĂ, cu podeaua 1 — `n = 1` reproduce exact calculul dinainte. Aritmetica
ITEREAZĂ cota (nu `n × cota`), iar deductibilul se calculează pe SUMA lunii,
cu regula valabilă la sfârșitul ei: plafonul lunar se aplică o singură dată pe
suma recuperată. `Luni` e cifră pe linie și pe rândul de registru, intră în
cheia anti-stale și se inversează la storno.

(l) **Cursa închidere ↔ operare se rezolvă prin blocarea verigii, în
tranzacția comenzii.** Fiecare comandă (operare, anulare, storno, generare,
închidere, redeschidere, reconstrucție, corecție) rulează într-o tranzacție
explicită pe `DbContext`-ul ObjectSpace-ului ei; gardianul de perioadă
citește veriga cu `FOR SHARE`, comanda de închidere ia `FOR UPDATE` pe ea ca
PRIMĂ instrucțiune, iar reconstrucția blochează lanțul ÎNTREG. `MotorOperare`
nu știe de tranzacție: ea e a apelantului. Comenzile perioadei rulează pe ușa
NON-SECURED (58c) — scriu exact ce refuză gardianul pe ușa securizată —, cu
refuzurile uniforme 401 → 400 → 404 → 403 → 422 și un singur corp (80), iar
verificarea cere dreptul de citire pe tot ce însumează (80e).

## Context

Registrele erau rulaje pure: orice sold se calcula la fiecare citire, prin
sumă peste TOT istoricul cheii, iar baza e per client și nu se resetează
niciodată (legacy avea bază nouă per an și solduri materializate la trecerea
de an). Măsurat la 59 și 66, nimic nu trecea de 300 ms la un an de date — dar
fiecare consumator creștea LINIAR cu vechimea bazei: balanța, fișa de cont,
soldul de stoc, `SolduriLaData`/`VerificaSoldIntermediar` din motor,
închiderea de TVA, documentele cu rest.

Perioada fiscală era un nomenclator de patru câmpuri, fără nicio regulă:
ianuarie se putea redeschide cu martie închis, motorul accepta un document
retroactiv în ianuarie, iar închiderile de TVA din februarie și martie
rămâneau tăcut greșite. Contiguitatea era codificată de două ori, local, pe
tipuri (ITV, AMO) — semnul că regula aparține perioadei, nu tipului.
`Document` avea doar `Data` și un timestamp tehnic: un document întârziat
dintr-o perioadă închisă nu se putea opera deloc, iar o eroare materială nu
avea altă cale decât un storno fără legătură declarată și fără motiv.
`RegistruTva` avea doar data faptului, deci noțiunea de rectificativă nu
exista. `Imperechere` n-avea dată proprie și nici gardian de perioadă.

Felia a pornit pe CERINȚA DE PRODUS declarată (soldurile la zi, soldul
partenerilor, arieratele la închiderea de an), nu pe performanță: cifrele de
azi erau sub prag, iar mărginirea costului e consecința, nu motivul.

## Tranșări

**(a) Snapshot pe perioade de REFERINȚĂ, nu pe fiecare perioadă închisă
(amendament după pasul 0).** Cheia completă a atomului crește cu ~15 k/lună
și nu scade; un snapshot per perioadă închisă ar fi ajuns la ~27 M rânduri la
5 ani, ≈ 19× registrul. Referința (ultima închisă + fiecare decembrie) dă
același răspuns la orice dată — o dată istorică între capete costă cel mult
un an de rulaje — și păstrează exact ce cere produsul: arieratele la 31.12.
Cheile integral zero se omit (−92,9 % pe stoc); consecința de RAPORTARE, scrisă
în limite: balanța analitică a unei luni afișează cu 1.743 de rânduri mai
puțin pe baza de import, toate cu inițial, rulaj și sold zero.

**(b) Tranzacția e a comenzii, nu a interceptorului.** Din cele cinci forme
probate la pasul 0, F1 (tranzacție explicită pe `DbContext`-ul
ObjectSpace-ului comenzii) e singura care ține pe CALEA REALĂ XAF:
`CommitChanges` se înrolează în tranzacția curentă, `FOR SHARE` ține rândul
înainte și după commit, rollback-ul anulează și commit-ul. Interceptorul (F2)
ar fi depins tăcut de `AutoTransactionBehavior.Always`; blocarea optimistă pe
perioadă (F3) producea conflicte false între două operări din aceeași lună.
Cursa dintre OPERATORI (25f) rămâne parcată: felia rezolvă doar cursa
perioadei.

**(c) Data înregistrării intră pe BAZĂ, nu pe frunze.** Trece ambele teste
ale apartenenței (2): semantică identică pe orice tip, consumată direct de
motor la postare. Implicitul stă la trei seam-uri, NICIUNUL în setter —
controllerul de implicite XAF (cu un abonament care trage data înregistrării
după data documentului cât timp erau egale), adaptorul de scriere al API-ului
și normalizarea din motor („`default` ⇒ `Data`"), care ține Import1C și
Migrare neatinse. Ordinea `DataInregistrare >= Data` e refuzată în toate
trei, cu text identic. Documentele GENERATE o primesc la CREARE, altfel
draftul ar fi purtat `default` până la operare și l-ar fi ARĂTAT așa.

**(d) Perioada de declarare are granularitate de LUNĂ — și asta a rupt o
probă (oprire raportată la pasul 4a).** O probă a lui D394 tăia august în două
ferestre de ZILE și cerea cifre diferite pe ele; cu filtrul pe perioada de
declarare, ambele jumătăți întorc luna întreagă. Nu e un defect de
implementare, ci consecința directă a formulei: perioada fiscală E luna, deci
un interval de zile nu mai poate selecta o parte din ea. Alternativa (filtru
compus — zile pentru rândurile nemutate, lună pentru cele mutate) a fost
respinsă ca preț prea mare pentru un caz pe care legea nu-l cere. Proba și-a
păstrat toate cifrele lunii, iar dovada „stornoul e fapt al perioadei
stornării" s-a mutat pe registrul citit direct.

**(e) Reperul rectificativei e rândul, nu documentul.** Contractul pin-uise
`Document.DataOperare > InchisaPrimaOara`; `DataOperare` e a DOCUMENTULUI și
nu descrie rândul de storno, scris mai târziu peste același document.
Registrul fiscal a primit `ScrisLa` propriu. Limită declarată: pentru
stornourile de dinaintea migrației, `ScrisLa` e data operării documentului.

**(f) Culegerea corecției se copiază GENERIC, prin metadata EF**, tot lanțul
TPT (scalare + FK-uri), cu o listă explicită de excluderi (identitatea,
starea, datele motorului, legătura, câmpurile de sistem). Alternativa — un
hook per tip — ar fi cerut 18 implementări și ar fi rămas în urmă la fiecare
tip nou. Lotul: linia care îl NAȘTE primește pe copie un lot propriu,
nefinalizat, pe care motorul îl finalizează la operare; linia care doar
CONSUMĂ păstrează lotul. `Numar` și `Data` se păstrează fiindcă nu există
unicitate de număr pe serie în model (verificat), deci seria nu se consumă.

**(g) `ItvLipsa` blocant e incompatibil cu scenele harness-ului — și ieșirea
a fost calea operatorului real (oprire raportată la pasul 7).** Scenele
ModelCheck închid luni fără să facă decontul de TVA (a-l face le-ar schimba
chiar cifrele contabile măsurate), iar o blocantă nu se acceptă. Helperul de
închidere al harness-ului COBOARĂ severitatea felului ÎN POLITICĂ pe durata
închiderii și o pune la loc — exact ce ar face un operator —, iar valoarea de
seed rămâne subiectul probelor de acceptare. Ocolirea mecanismului n-a fost
pusă în discuție.

**(h) `sold-parteneri` nu e creanța per partener — constatare de PRODUS
(pasul 6).** Dimensiunea `Repartitor` urmează LATURILE documentului, nu contul
de terț: pe factura de client, atomul de debit al lui 4111 poartă EMITENTUL
(103.301 din 108.912 rânduri pe baza Privat). Ecranul spune asta explicit;
creanța per partener se citește din partidele deschise. Dimensionarea
conturilor de terț pe partener rămâne decizie separată (F27-r11, familia
64h/86-r13).

**(i) Partidele se materializează din serviciul de solduri, nu din comanda
perioadei.** Cei trei apelanți (închidere, redeschidere pe P−1, reconstrucție)
le vor pe toate trei, iar eliminarea și verificarea existenței le acoperă
uniform; comanda perioadei a rămas NEATINSĂ. Backfill-ul datei împerecherii ia
`max(stingător, stins)`, nu data stingătorului singură, ca invariantul de
ordine să fie adevărat și pe istoric.

**(j) `MotorOperare` s-a atins de trei ori în toată felia**, de fiecare dată
numărat pe `git diff`: 8 înlocuiri de dată + normalizarea + gardul de ordine
la pasul 3, cele două blocuri de materializare a TVA-ului la 4a, trei
instrucțiuni la 6 (`TotalStingere` la operare, `null` la anulare, inversarea
împerecherilor la storno în locul gardianului). Nicio regulă de potrivire,
contare, evaluare sau stingere nu s-a schimbat.

## Review advers

Două runde, agent separat, în worktree cu bază proprie.

**Runda 1 — pașii 0–6** (2026-09-17): 0 MAJOR, 3 MEDIU, 5 MINOR, 7
observații; toate reparate, fiecare cu proba ei (`F27-R*`, scena 2036),
niciun fix cerând migrație.

- **1b (MEDIU)** — reconstrucția nu lua niciun lock: o închidere care comitea
  după citirea referințelor rămânea fără snapshot, iar soldurile porneau tăcut
  de la zero. Reconstrucția blochează acum LANȚUL ÎNTREG ca primă instrucțiune.
- **3e (MEDIU)** — plata autogenerată a unei facturi înregistrate DUPĂ ziua
  plății era imposibil de operat (împerecherea automată ar fi precedat
  înregistrarea facturii): plata primește `max(ziua plății, înregistrarea
  facturii)`; motorul neatins.
- **L3 (MEDIU)** — ștergerea originalului unei împerecheri desfăcute cădea pe
  fixup-ul EF al rândului invers, cu text fără legătură, iar pe o cale fără
  gardian rândul invers rămânea orfan: gardianul refuză acum pe FOND ambele
  ștergeri și tace pe modificarea colaterală.
- **1c, 3c, L1, L2, 10 (MINOR)** — împerecherea nouă din XAF se comitea pe ușa
  securizată, unde `FOR SHARE`-ul e în autocommit ⇒ acțiune cu dialog pe ușa
  non-secured, `New` retras; corecția copia `TotalStingere` ⇒ exclus; lanțul cu
  GOL lăsa snapshot orfan (eliminarea căuta doar P−1 definit) ⇒ se elimină toate
  referințele de dinaintea lui P care nu sunt decembrie; `PerioadaFaptului` peste
  o perioadă NEDEFINITĂ declara într-o lună inexistentă ⇒ perioada înregistrării;
  `refuzuri.ps1` a primit 403 pe desfacere cu subiect VIZIBIL.
- **Observații acceptate și documentate**: D394 rectificativ cu partener
  schimbat (partenerul vechi cu factură + storno net 0, cel nou cu corecția);
  desfacerea prin rând invers acceptată și în fereastra deschisă (alegere);
  recuperarea amortizării iterează pe baza de la ultimul eveniment (aproximare
  declarată); `ScrisLa` pe stornourile de dinaintea migrației; integritatea
  snapshot-ului sub sabotaj ⇒ F27-r12.

**Runda 2 — pasul 7** (2026-09-17): două fix-uri, amândouă în serviciul
perioadei, niciunul cerând migrație (`F27-RA*`, scena 2037).

- Rândul de rezumat al unei familii plafonate avea severitatea HARDCODATĂ
  `Avertisment`: un fel pus pe `Blocant` se închidea cu rezumatul acceptat, iar
  operatorul accepta o constatare pe care n-o văzuse. Rezumatul a devenit
  ACCEPTARE ÎN BLOC declarată (b). Alternativele — „rezumat neacceptabil" (o
  lună cu peste 200 de rânduri de același fel nu se mai poate închide) și
  „informativ, exclus din refuz" (rândurile nelistate ignorate tăcut) — pierd
  tocmai acceptarea conștientă.
- Un fapt raportat de două constatări în aceeași verificare: excluderea se face
  din INTEROGARE (deci și din plafon, și din numărătoare), dar numai pentru
  faptele RAPORTATE EFECTIV; cu familia pe `Ignorat` faptul reapare pe calea
  lui proprie, altfel ar fi dispărut de tot.

## Cifrele

| | la deschidere (87, 2026-09-15) | la închidere |
|---|---|---|
| ModelCheck bugetar | 1068 / 0 FAIL | 1278 / 0 FAIL |
| ModelCheck privat | 1184 / 0 FAIL | 1434 / 0 FAIL |
| `refuzuri.ps1` (host viu Privat) | 229 / 229 | 285 / 285 |
| Migrații | — | 8, toate cu backfill în aceeași migrație |
| `Motor/*` | — | `PerioadaService`, `SolduriService`, `CorectieService`, `TranzactieComanda` noi; `MotorOperare` atins de trei ori |

**Proba supremă** (2026-09-17, 12:12 → 14:27, exit 0): Import1C integral pe
clona Flax cu `--inchide-lunile`, adică CU lunile închise pe parcurs — deci
importul citește peste snapshot-uri, nu peste registrul integral. Raportul de
reconciliere e IDENTIC pe conținut sortat cu baseline-ul feliei 26. 12/12 luni
închise, 0 constatări fiecare, 0,7 s → 5,2 s per lună.

**Integritatea soldurilor**: reconstrucția (9,4 s) dă **0 diferențe** pe toate
trei materializările — contabil 184.780 / 184.780, stoc 7.914 / 7.914, partide
201.046 / 201.046, toate deltele 0,00. Redeschiderea lui 12/2025 (6,2 s)
rematerializează prin `SUM` integral referința nouă (171.396 / 7.790 / 184.458).

**Perf, A/B pe ACEEAȘI bază** („A" = 11 luni închise, „B" = lanțul desfăcut
prin 11 redeschideri, apoi RE-închis cronologic cu aceleași cifre la rând):
fișa `4111` pe decembrie 187 → **122 ms**; balanța analitică pe decembrie 254
→ **210 ms**; `sold-parteneri` la 31.12 290 → 219 ms; `sold-stoc` la zi 153 →
50 ms; balanța sintetică 83 → 58 ms; operarea unei FCT cu 49 de linii 411 →
394 ms; `documente-cu-rest` 171 → 181 ms, adică NESCHIMBAT cu și fără partide —
A/B-ul a infirmat bănuiala că materializarea le-ar fi încărcat. Două ținte
rămân neatinse și, conform regulii de oprire, nu s-a optimizat nimic pentru
ele: fișa de cont (calea de date costă 7 ms, restul e cadrul cererii —
F27-r14) și balanța analitică (cardinalitatea cheii, țintă calibrată pe
ianuarie — F27-r15). Documentele cu rest nu sunt o țintă ratată, ci o
restanță de FORMĂ (F27-r16): ținta de 150 ms fusese calibrată pe baza Privat,
unde măsurătoarea pasului 6 dădea 147 ms; cele 181 ms sunt cifra altei baze.

## Ce rămâne deschis (restanțele deciziei)

Restanțele poartă numele din contract (`F27-rN`).

- **F27-r1** reclasificarea pe 1174 a erorilor semnificative din exerciții anterioare: notă contabilă manuală; pragul de semnificație se decide separat.
- **F27-r2** scadențar/aging pe partidele deschise (aceeași listă + scadența + bucket-uri).
- **F27-r3** cursa închidere ↔ operare prin blocarea verigii — **n-a fost nevoie să se activeze**: pasul 0 a probat F1 pe ambele capete.
- **F27-r4** închiderea de an ca operație distinctă (121 → 1174/117, soldurile de deschidere ale anului nou): nu e necesară pentru solduri (decembrie le acoperă), rămâne cerință de produs separată.
- **F27-r5** D406/D300/D394 rectificative ca FIȘIER (marcajul în XML/PDF): proiecțiile expun conținutul, formatul de depunere e felie proprie.
- **F27-r6** constatări de închidere pe reconcilierea 1C (documente neimportate în P): conectorul, nu mecanismul.
- **F27-r7** soldul în lookup-urile de partener din culegere.
- **F27-r8** concurența între operatori (25f) rămâne parcată.
- **F27-r9** editabilitatea datei de înregistrare pe documentele GENERATE (o primesc la creare, din sursă sau din lună).
- **F27-r10** SAF-T: inițialul de stoc rămâne pe registrul integral; mutarea pe referință cere schimbarea semanticii lui `Randuri` din avertismentul de sold neraportat — decizie de raportare, nu de motor. Inițialul de CONT e deja pe referință.
- **F27-r11** dimensionarea conturilor de terț pe PARTENER (constatarea de produs de la pasul 6): azi `Repartitor` urmează laturile documentului, deci `sold-parteneri` nu e creanța per partener. Familia 64h / 86-r13 / 73-r12.
- **F27-r12** integritatea snapshot-ului memorată în istoric: rândul de închidere reține numărul de rânduri și sumele scrise, o constatare (și o probă) le compară cu ce e în bază, iar referința fără niciun rând se refuză când numărul memorat e pozitiv. Azi un rând de snapshot șters direct din bază dă o balanță tăcut greșită, detectabilă doar prin reconstrucție.
- **F27-r13** perioadele fiscale ale unei baze noi: seed-ul scrie 12 luni ale unui AN HARDCODAT (2026), iar perioadele se pot adăuga doar din XAF (nomenclator) — nu din React și nu prin OData. Verificat: crearea e posibilă și gardianul o acceptă (perioadă nouă, deschisă, `(An, Luna)` unic), deci e o lipsă de ERGONOMIE, nu una funcțională. Devine consecventă acum că perioada e lanț: baza de import a ieșit cu 24 de verigi, dintre care 12 ale unui an fără niciun document. Familia 53i („perioadele fiscale ≠ 2026 se adaugă manual"), care rămâne deschisă.
- **F27-r14** costul de CADRU al unei cereri: 58 de instrucțiuni SQL de bootstrap de securitate per ObjectSpace, plus hidratarea și serializarea. E motivul pentru care fișa de cont ratează ținta end-to-end deși calea ei de date costă 7 ms; ținta se ratează din AFARA feliei.
- **F27-r15** ținta de perf a balanței analitice (< 100 ms) era calibrată pe IANUARIE (lună fără sold inițial, 84 ms la 66); pe decembrie, cu 71.167 de grupe `Cont × Repartitor`, nu e atingibilă în forma de azi. De re-calibrat (comparabilul lui decembrie e „an = 269 ms") sau de pre-agregat. Ridicarea lui `work_mem` pe sesiune NU e butonul care pare: 136 ms la 4 MB (agregare paralelă) față de 155/162/183 ms la 8/16/64 MB, unde planificatorul renunță la paralelism.
- **F27-r16** forma proiecției documentelor cu rest. Panoul filtrat pe contrapartidă costă 181 ms pe baza de import (ținta de 150 ms fusese calibrată pe Privat, unde pasul 6 măsurase 147 ms — felia nu lasă în urmă o țintă încălcată pe baza pe care a fost pusă). Cauza e dublă: parcurgeri integrale ale împerecherilor (44.448 de rânduri × 3 bucle, 2,18 M rânduri respinse la nested loop) și mulțimea de candidați moștenită de la 59 — uniunea TUTUROR documentelor operate. **Fixul evident a fost măsurat și RESPINS motivat**, nu omis: corelarea legăturii pe fereastră elimină complet parcurgerile integrale (8 citiri prin index) și duce panoul filtrat la 82 ms, dar mută costul pe calea NEPLAFONATĂ pe care o consumă constatarea de rest scadent (nefiltrată, materializată integral, fără paginare): 220 ms → 1,02 s, adică 4,6×; varianta cu două sume corelate e și mai slabă (1,23 s). Niciun index nu lipsește — cele două indexuri pe împerecheri există și se folosesc. Restanța e deci una de FORMĂ, și una singură: mulțimea de candidați și forma legăturilor se rezolvă ÎMPREUNĂ, altfel câștigul pe un capăt e pierdere pe celălalt. O formă dependentă de parametru (corelată când vine contrapartida, derivată altfel) rămâne opțiune deschisă — cu probele ei.
- **F27-r17** ordinea totală a listei `op1` din D394: cheia de ordonare (`TipPartener`, cod fiscal, denumire, tip, cotă) nu e o ordine TOTALĂ pentru persoanele fizice fără cod cu aceeași denumire — rândurile lor rămân în ordinea bazei, deci două generări ale aceleiași luni pot livra fișiere care diferă la rând. Familia 72-r7.
- **F27-r18** coloana cu data înregistrării în LISTELE React de documente: câmpul e cules și afișat pe toate formularele, dar adăugarea coloanei ar fi dus atingerile clientului peste pragul regulii de oprire a pasului 3.
