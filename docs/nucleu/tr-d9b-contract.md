# TR-D9b — unitățile de sold: contract și primul pachet de execuție

- Tip: contract
- Data: 2026-10-12
- Trigger: owner, „pornește primul pas”, după acceptarea revenirii la privat și a notei de reluare
- Decizii: 090, 092, 102, 110, 111, 114, 115, 116, 117, 118
- Stare: deschis

**Starea execuției:** contract redactat, cu inventarul de sursă pe
`96ccc5acbb4048d7ba41f60924863d631e83daca`. Pasul 1 documentar este făcut;
pasul 2 este implementat, cu verificarea în
[raportul redenumirilor](tr-d9b-pas2-redenumiri.md). Pasul 3 are catalogul A
verificat și referința măsurată până la 5.001.179 postări; condițiile și
limita montajului sintetic sunt în [raport](tr-d9b-pas3-referinta.md).
Pasul 4 are persistența, scriitorul și cititorii implementați; verificarea
și limitele sunt în [raportul originii](tr-d9b-pas4-origini.md). Direcția privat-first și
originea scrisă sunt aprobate; forma fizică și pragurile de mai jos sunt
propunerea concretă a acestui contract, fără a atribui owner-ului o aprobare
anterioară a detaliilor. Comparația A/B și review-ul independent Codex sunt
urmărite în [pasul 5](tr-d9b-pas5-comparatie.md); reverificarea owner/Claude
și commit-ul rămân separate.

Primul pachet livrează **originea explicită a partidei**, citită fără
redescoperirea documentului deschizător în postările cubului. Soldul rămâne
în cub. Contractul pornește din
[nota de reluare](../analize/2026-10-12-tr-d9b-reluare-privat.md), cu
[inventarul și constatările](tr-d9b-pas1-inventar.md) ca dovadă de sursă.

## Scop și delimitare

TR-D9b rămâne etapa unităților din 110 (k), pe coordonatele C111. Pachetul
curent acoperă originea partidelor, cititorii ei și redenumirile 118-r7/r8.
Țintele ulterioare — corecții valorice cu `Atribuit`, unitatea de evaluare,
valuta și costul vamal — primesc pachete cu contract și probe numerice
proprii. Închiderea primului pachet nu închide întreaga TR-D9b și nici
restanțele celorlalte pachete.

Pachetul următor are [contractul corecțiilor valorice](tr-d9b-corectii-valorice-contract.md),
[inventarul CodeGraph](tr-d9b-corectii-valorice-inventar.md) și
[catalogul CVL](scenarii/CVL.md), redactate înainte de implementare.
Propunerea de [producător FCT și compensare pe postări](tr-d9b-corectii-valorice-producator-si-compensare.md)
detaliază documentul real și ordinea inversărilor. Apartenența compensării
este fixată prin [decizia 119](../decizii/119-compensarea-valorica-apartine-corectiei.md); restul rămâne la review înainte de cod.
Costul vamal urmează mecanismului verificat; valuta și unitatea de evaluare
rămân pachete distincte. Aceste documente nu închid review-ul originii.

Nu intră: registrele procedurale 111(f), alocările pe poziții bugetare,
schimbarea identității partidei, reorganizarea loturilor/fișelor, un nou
algoritm de stingere, culegerea partidei în UI, funcționalitate valutară
nouă, curățenie generală a documentației sau compatibilitate cu baze vechi.
Funcționalitatea privată existentă, inclusiv TVA și valuta deja acoperită,
nu este restrânsă de cazurile RON ale probelor noi.

## D-uri ale primului pachet

### D9B-D1 — rezultatul și identitatea

Aceeași comandă validă produce aceleași postări, solduri și alocări FIFO.
Partida are ID-ul determinist din document × cont × titular (092), iar
partida inițială păstrează identitatea distinctă din referința deschiderii.
Data nu intră în ID. O factură cu mai multe poziții nu capătă automat
mai multe partide; două conturi sau doi titulari produc identități diferite.

### D9B-D2 — persistență restrânsă la originea partidei

Forma propusă este un POCO de citire `OriginePartidaPersistata`, în tabela
`OriginiPartide`, separat de DTO-ul actual `Citiri.OriginePartida`.
Este o proiecție de proveniență întreținută la scriere, reconstructibilă din
faptele cubului; nu este registru procedural și nu este sursă de sold.

Contractul rândului:

- cheie primară: ID-ul partidei;
- cont și titular obligatorii, cu FK spre nomenclatoarele existente;
- document deschizător nullable, cu FK fără cascadă spre document;
- pentru deschidere: referința inițială și ordinalul postării de deschidere;
  pentru origine pe document, aceste două valori sunt nule;
- exact una dintre originea pe document și cea inițială; cheile naturale
  document/cont/titular și referință/cont/titular inițială sunt unice separat.

Nu poartă sumă, rest, stare de stingere, curs, poziție bugetară sau
`Deschisa`. Data unității se citește în continuare din faptele ei, ca azi;
astfel retragerea și reoperarea la altă dată nu cer rescrierea unei date
pretins imuabile în identitate. Indecși: PK pentru îmbinarea pe unitate și
index pe document/cont/titular pentru citirea partidelor proprii.

Nu se adaugă acum FK din fiecare `Postare.Unitate` spre această proiecție:
coloana include și loturi/fișe, iar retragerea permisă poate lăsa istorie
net zero pe o partidă fără suport propriu curent (D9B-D4). Nu se creează o
tabelă universală a tuturor unităților pentru a rezolva primul pachet.

### D9B-D3 — producerea și atomizarea

Materializarea extrage pe set originile din postările finale eligibile
ale comenzii, înainte de commit. Pentru o partidă proprie, identitatea se
verifică la scriere cu regula 092; pentru deschidere, originea vine din
`PartidaInitiala` și ordinalul efectiv al postării. Hash-ul nu mai este
necesar pentru descoperirea originii în interogările de citire.

Nu se folosește exclusiv lista `Decizii.PartidaDeschisa`: aceasta explică
declarația, dar nu garantează enumerarea fiecărei partide postate în toate
formele curente. Nici simpla atingere a unei partide străine nu îi creează
o origine pe stingător. Transferul poate produce suport propriu dacă
postările lui îl au efectiv; desfacerea transferului păstrează originalul.

Deduplicarea acoperă atât rândurile din bază, cât și cele noi în același
ObjectSpace. Aceeași identitate cu alt cont/titular/origine este refuzată,
nu suprascrisă. Codul propus `ORIGINE_PARTIDA_INCONSISTENTA` intră în
`CoduriRefuz` la implementare. Nu este refuz de declarant: probele îl
asertă la materializare, cu rollback complet, și îl marchează astfel.
Refuzul/dry-run nu lasă origine, postare sau tranzacție nouă.

Se păstrează tranzacția și serializarea comenzii existente. Nu apare un
commit separat pentru origine și nici o interogare per postare/partidă.

### D9B-D4 — ciclul de viață

| Comandă/fapt | Originea | Soldul |
|---|---|---|
| Operare care deschide o partidă | creată o singură dată | exclusiv din postări |
| Plată nominalizată integral pe factura existentă | folosește originea facturii; nu inventează partidă proprie | scade restul facturii |
| Excedent pe partida proprie | origine separată a stingătorului | rămâne pe partida proprie |
| Transfer/desfacere | păstrează originea fiecărei unități, fără mutarea ei la țintă | se includ în soldurile pe unitate |
| Storno | originalul și originea rămân; nu apare o origine a inversei | inversa modifică soldul la data ei |
| Retragere permisă | se elimină originea proprie dacă nu mai rămâne suport propriu în cub | efectele retrase dispar atomic |
| Reoperare | aceeași cheie naturală dă același ID; originea se recreează o dată | numai efectele noii operări |
| Deschidere | document nul, referință reală, ordinal real | soldul inițial din cub |

**Cazul istoriei net zero:** gardul actual verifică efectul temporal al
dependenților, nu simpla existență a oricărei postări străine. Dacă o
retragere este permisă după neutralizarea dependenților, postările lor pot
rămâne cu ID-ul unității. Ele nu justifică păstrarea unei origini active
a documentului retras. `CuOrigine` păstrează îmbinarea exterioară, ca azi;
nu adaugă un FK care ar transforma această retragere într-un refuz nou.
Acest caz are probă obligatorie înaintea acceptării implementării.

Originea inițială nu are retragere de document. Curățenia scenariilor
șterge explicit proiecția împreună cu faptele proprii, pe marcaj/identități.
Nu se schimbă refuzul existent de retragere a unei plăți care a stins o
partidă inițială; acolo fluxul curent cere storno.

### D9B-D5 — cititori, securitate și FIFO

`Origini` și `OriginiDocument` citesc proiecția; `CuOrigine`, citirile
partidelor proprii, resturilor și plăților folosesc aceeași intrare.
Nu se adaugă un query per rând și nu se materializează setul complet
pentru paginare. Soldurile și snapshot-urile păstrează sursele existente.

Vizibilitatea originii nu este dată de simplul drept pe noua tabelă.
Pe ușa securizată, originea este disponibilă numai dacă există suport
propriu vizibil conform selecției actuale `Partide.Postari`; documentul și
etichetele trec în continuare prin interogările securizate. Proba include
rolul care vede plata, dar nu postările facturii. Verificarea de vizibilitate
poate folosi un `EXISTS` pe chei; nu rededuce identitatea prin hash și
`DISTINCT` peste întregul cub. Pe ușa de sistem, originea se citește direct.

Nu apar CRUD, navigație XAF sau endpoint OData pentru proiecție. Gardul de
scriere o protejează inclusiv față de administrator pe ușa securizată.
Nu se introduce cerință de citire integrală în proiecțiile care filtrează
astăzi pe rând (111, precizarea C111-F-R2 a 110).

FIFO rămâne data unității → document deschizător → ordinalul deschiderii
pentru unitățile fără document → ID unitate, conform 114–117. Ordinalul
inițial din proiecție înlocuiește descoperirea lui, nu ordinea decisă de 117.
`Vizibila` citește mișcări; `Integrala` poate folosi snapshot-ul (104b).

### D9B-D6 — redenumiri și schemă

`Unitate` → `UnitateSold` și enumul debit/credit `Latura` → `Sens` intră
împreună într-un pas mecanic, înaintea persistenței, conform 118-r7/r8.
Laturile documentului și unitățile interne/de măsură nu sunt redenumite.
Forma serializată a explicației și contractele externe sunt inventariate
înaintea înlocuirii; o schimbare de format/API se declară explicit, nu se
ascunde într-o înlocuire textuală. Ordinalul și perechea rămân distincte.

Schema folosește migrațiile EF canonice și baze recreabile conform 102.
Nu se implementează upgrade/backfill de producție sau citire duală.
`PostareVizual` și metadata se schimbă numai dacă sunt atinse de redenumire.

### D9B-D7 — probe și performanță

Catalogul nou al pachetului va avea prefixul `SC-D9B`; așteptările sunt
fixate mai jos. Fixture prin documente reale, `OperareApi` în ObjectSpace-uri
noi, an liber propriu verificat înaintea codului, curățenie în `finally`.
Deschiderea folosește excepția de motor existentă, nu inserări în cub.
Invarianții de origine se probează pe fapte și cu mutanți: lipsă, dublare,
origine străină, origine fără suport, ordinal inițial greșit. Refuzurile nu se contabilizează
ca funcționalitate neimplementată.

Performanța se măsoară A/B pe aceeași bază de probă și același set de date,
cu număr și amprentă de fapte raportate. SQL-ul cititorului A se păstrează
în harness, nu ca ramură de compatibilitate a produsului. Datele noii
forme se produc prin scriitorul nou; ambii cititori citesc aceeași bază.
Pentru costul scrierii se compară comenzile A/B pe aceeași bază pregătită
compatibil, din aceeași stare de pornire restaurată între loturi; binarul
și schema sunt identificați în raport. Nu se compară clone cu date diferite.

Protocol: o încălzire și cinci rulări măsurate, mediană, `EXPLAIN (ANALYZE,
BUFFERS)`, comenzi SQL și alocări/rânduri returnate; scară până la circa
5 milioane de postări. Se separă pagina de consumarea întregului rezultat,
ușa de sistem de cea securizată și lectura originii de agregarea soldului.

Praguri propuse, fixate înaintea termenului B:

- zero diferențe de rezultate, inclusiv origine nulă și vizibilitate;
- SQL-ul de sistem nu redescoperă originea prin scanarea postărilor;
- la treapta maximă, mediana citirii integrale `PartideCuRest` pe ușa de
  sistem este cel mult 80% din A; celelalte citiri măsurate nu depășesc
  `max(1,10 × A, A + 10 ms)`;
- comenzile de scriere măsurate nu depășesc `max(1,15 × A, A + 10 ms)`;
  pentru 2/51 partide din aceeași comandă, numărul de interogări de
  descoperire a originilor este constant; fără N+1;
- `RAMURA-NUL` rămâne verde; dimensiunea tabelei și indecșilor se raportează.

Un prag ratat se raportează. Nu se relaxează după rezultat și nu se declară
F27-r16/F28-r1 închise doar din răspunsul unei pagini mici.

## Scenarii de acceptare ale pachetului

Persistența și scenariile ei sunt **implementate și verificate la pasul 4**.
Termenul B și review-ul independent Codex sunt verificate în
[pasul 5](tr-d9b-pas5-comparatie.md); reverificarea owner/Claude rămâne
înainte de commit.
[Catalogul SC-D9B](scenarii/D9B.md) separă probele comportamentului actual
(termenul A) de cele pentru proiecția nouă. RON, fără TVA pentru cifrele
noi; catalogul existent verifică regresia celorlalte forme.

| ID | Intrare și așteptare independentă |
|---|---|
| SC-D9B-01 | FCT 100, PLT nominalizată 40: rest 60, o origine FCT, nicio origine proprie PLT fără postări proprii. |
| SC-D9B-02 | FCT A100/B70, plata40 pe A: A60/B70, două origini distincte. |
| SC-D9B-03 | NTC două datorii100 pe același cont, titulari diferiți: două origini; stingerea100 a primei lasă0/100. Separat, două conturi cu partide în aceeași plată: ambele origini persistate, chiar dacă explicația declară una. |
| SC-D9B-04 | FCT100, PLT liberă40, împerechere40: rest FCT60, PLT0; două origini. Desfacerea reface100/40; zero rulaj suplimentar pe cont din transferuri. |
| SC-D9B-05 | FCT100, plata nominalizată40 retrasă/reoperată:60 →100 →60. Retragerea separată a unei facturi fără dependenți elimină originea; reoperarea la altă dată reface același ID o dată și folosește data noii operări. |
| SC-D9B-06 | FCT100 fără dependenți, storno integral: rest0, o origine păstrată; la data anterioară stornoului rest100. |
| SC-D9B-07 | Deschidere60/40 pentru același titular, referințe diferite; stingere20 a primei:40/40, documente nule. Storno reface60/40; retragerea plății cu stingere inițială rămâne refuzată. |
| SC-D9B-08 | Două facturi aceeași dată, prima creată60/a doua40; NTC30 fără pin:30/40. Două partide inițiale30/50 la aceeași dată: NTC40 consumă30+10 după ordinal, lasă0/40; ID-urile nu decid ordinea. |
| SC-D9B-09 | Închidere după FCT100/PLT40: lectura directă și cea cu snapshot dau60; plata20 în luna următoare dă40. Citirea istorică la luna închisă rămâne60. |
| SC-D9B-10 | Rolul vede numai plata40, nu suportul facturii100: nicio origine/etichetă a facturii dezvăluită; valorile vizibile sunt identice înainte și după schimbare. CRUD direct al proiecției este refuzat, inclusiv pentru admin securizat. |
| SC-D9B-11 | Eșec după pregătirea originii, înaintea commit-ului: zero origini/postări/tranzacții noi; reluarea reușită produce un singur rând. Două comenzi concurente nu dublează originea. |
| SC-D9B-12 | FCT100; NTC nominalizată40 apoi storno NTC la aceeași dată; retragere FCT permisă de gardul existent: originea proprie dispare, postările străine +40/−40 rămân. Reoperare FCT: exact o origine și rest100. Dacă ruta reală refuză, se documentează cauza și se ajustează montajul, nu gardul. |

## Pași, verificări și riscuri

| Pas | Schimbare | Verificare | Risc | Stare |
|---|---|---|---|---|
| 1 | Contract și inventar de sursă | decizii/amendamente, invarianți, `docskit` | confundarea țintei cu implementarea | redactat 2026-10-12; fără review advers |
| 2 | Redenumirile 118-r7/r8 | nucleu, integrala ambelor profiluri, explicații/metadata/drift afectate | substituirea termenilor din alt domeniu sau schimbare de format | implementat și verificat 2026-10-12: nucleu 199/199, integrală 3.678/5.080; fără review advers sau commit |
| 3 | Catalog SC-D9B, fixture-uri și termen A | scenariile existente; așteptările noi distincte de baseline | probă vacuă, volum nereprezentativ | realizat 2026-10-12 pentru A: scenarii pe ambele profiluri, 48 combinații de citire până la 5.001.179 postări, scriere2/51; limita montajului sintetic și probele P rămase în [raport](tr-d9b-pas3-referinta.md); fără review advers |
| 4 | Persistență, scriitor, retragere și cititori | SC-D9B, invarianți/mutanți, integrala ambelor profiluri | origine lipsă la o cale rară; acces prea larg | implementat, verificarea în [raport](tr-d9b-pas4-origini.md); review în pasul 5, fără commit |
| 5 | Termen B și închiderea pachetului | pragurile D7, securitate, docskit, review advers | închiderea pe cifre din baze diferite | 48/48 perechi și scriere2/51 verzi; integrală 3.758/5.160 după corecțiile review-ului Codex; [raport](tr-d9b-pas5-comparatie.md); reverificarea owner/Claude înainte de commit rămâne |
| 6 | Membrii `Latura` ai sensului devin `Sens`, cu coloana de pe `Postare` | nucleu, integrala ambelor profiluri, metadata, drift OpenAPI, referința A | redenumirea laturii documentului; referința A comparată pe text | făcut 2026-10-13: 383 de înlocuiri pe simboluri, migrația `SensPePostare`, nucleu 199/199, integrală 3.759/5.161, drift zero, 48/48 perechi A = B; două praguri de timp picate pe SOLDURI, unde A și B sunt același cititor; [raport](tr-d9b-pas6-sens.md); fără review advers |
| 7 | Membrii de tip `SensTva` numiți `Sens` devin `SensTva`, cu coloanele mapărilor și câmpurile de API | nucleu, integrala ambelor profiluri, metadata, OpenAPI regenerat, build-ul clientului, HTTP | redenumirea sensului postării sau al stingerii; câmp de API schimbat fără client | făcut 2026-10-13: 275 de înlocuiri pe simboluri, migrația `SensTvaPeMapari`, nucleu 199/199, integrală 3.759/5.161, HTTP 327/327; [raport](tr-d9b-pas7-senstva.md); fără review advers |

Comenzile de produs, după pregătirea bazelor compatibile cu schema pasului:

```powershell
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Nucleu
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip D9B -Profil Ambele -Sufix .CodexD9bOrigine
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexD9bOrigine
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Infrastructura
```

`-Tip D9B` verifică la pasul 4 și proiecția persistată, pe ambele
profiluri. Pregătirea și rulările urmează
[rețeta comună](../stare-curenta/dezvoltare-si-validare.md), succesiv, cu
sufix propriu. Probele HTTP afectate urmează `refuzuri.ps1`; fără expunere
nouă în UI nu se cere un ecran nou. Import1C nu este gate; Flax nu se scrie.

## Regula de oprire și predarea

Pachetul se închide numai cu pașii 2–5 verificați, cifrele neschimbate,
origini corecte pe toate căile inventariate, securitate și praguri verzi,
documentația afectată actualizată și review advers închis pe constatări
`D9B-R1…`. Owner-ul și reviewer-ul reverifică înainte de commit; predarea
nu certifică singură.

Dacă un comportament cere schimbarea identității, a regulilor de retragere,
a ordinii FIFO sau o infrastructură procedurală, se oprește extinderea
acelui pas și se formulează contraexemplul. La o limită de buget stabilită
de owner, se predă doar ce este verificat, iar restul rămâne nominal;
nu se marchează închis pachetul incomplet.

Limite rămase: marja mică a scrierii cu 51 partide și volumul sintetic,
descrise în raportul pasului 5. Reverificarea Claude este consemnată acolo;
pachetul a fost comis la cererea owner-ului, la 2026-10-13.
Nicio restanță existentă nu este închisă automat de aceste măsurători.
După pachetul originii, următorul comportament
privat primește un contract distinct; 111(f) nu este condiție pentru el.

## Precizarea pasului 2 — 2026-10-12

Redenumirea este a tipurilor CLR `Atlas.Conta.Nucleu.Unitate` și
`Atlas.Conta.Nucleu.Latura`, împreună cu referințele lor semantice.
Proprietățile `Unitate`/`Latura`, coloanele, câmpurile DTO și forma JSON a
explicației păstrează numele. `FelUnitate` și laturile documentului nu se
schimbă. O eventuală redenumire a membrilor persistabili/API este un pas
distinct, cu inventarul consecințelor; nu este ascunsă în această schimbare.

## Precizarea pasului 6 — 2026-10-13

Cerut de owner după închiderea 118-r7 și 118-r8, înaintea codului
corecțiilor valorice: acele fișiere se rescriu oricum, iar documentele CV
citează linii din ele.

Se redenumesc membrii care poartă debitul sau creditul: `Coordonate.Latura`,
`Mutare.Latura`, `Postare.Latura`, `PostareVizual.Latura`, împreună cu
parametrii de același tip (de pildă cel din `Capat.Pe`). La 2026-10-13 sunt 33 de declarații de tip `Sens`
numite `Latura` în 23 de fișiere, în afara migrațiilor. Înlocuirea se face
pe simboluri, ca la pasul 2: latura documentului (`LaturaDocument`,
`Declaratii.Latura`, `RegulaStoc.Latura`, `LaturaPerecheId`) poartă același
nume în aceleași fișiere și nu se atinge.

Coloana `Latura` de pe `Postare` devine `Sens` printr-o migrație
incrementală (120): redenumire pe tabela părinte și view-ul `PostareVizual`
recreat, fiindcă își îngheață lista de coloane. Niciun index și nicio funcție
scrisă de mână nu numește coloana. SQL-ul brut care o numește:
`nou/tools/ModelCheck/ScaraVolum.sql`, `nou/tools/ProbeHttp/partide-cub.py`
și o probă din `ScenariiNir.Review.cs`.

De verificat înaintea înlocuirii: opt fișiere din
`nou/tools/ModelCheck/Referinte/D9bA/` conțin SQL cu numele coloanei; dacă
harness-ul le compară pe text, se regenerează, altfel comparația A/B pică.
Expunerea sensului postării prin OData și captionul din `PostareVizual` nu
sunt inventariate. JSON-ul explicației nu are câmpul.

Membrii numiți `Unitate` nu intră: `UnitateSold Unitate` este numele scurt
admis de glosar, iar redenumirea ar schimba coloana, indecșii ei și cheia
`unitate` din JSON-ul persistat al explicației. Confirmat de owner la
2026-10-13: după acest pas nu rămâne nimic de redenumit.

Realizat la 2026-10-13, cu trei abateri de la textul de mai sus, toate în
[raport](tr-d9b-pas6-sens.md): cifrele sunt ale simbolurilor (42 de
declarații, nu 33); view-ul a fost redenumit pe coloană, nu recreat, cu
aceeași definiție rezultată; capturile A au primit numele nou al coloanei
prin înlocuire, nu prin regenerare. În timpul pasului owner-ul a hotărât că
membrii de tip `SensTva` numiți `Sens` își iau numele întreg, într-un pas
următor; „nu rămâne nimic de redenumit” nu mai e adevărat până atunci.

## Precizarea pasului 7 — 2026-10-13

Cerut de owner în timpul pasului 6: `Sens` rămâne numele debitului și al
creditului, iar membrul care poartă sensul taxei se numește `SensTva`, fără
restanță.

Intră membrii, parametrii și variabilele de tip `SensTva` numite `Sens` sau
`sens`, cu coloanele `Sens` de pe `MapariD300`, `MapariD394` și
`MapariTvaSaft` și indecșii care le numesc, printr-o migrație incrementală
(120). Intră și câmpurile text numite `Sens` ale proiecțiilor de TVA, care
duc pe API numele membrului din `SensTva`: `D300Nemapat`, `D394Operatiune`,
`D394Neinclus`, `DecontTvaRand`, `RectificativaTvaRand`, `SaftNeinclus`.
OpenAPI și metadata se regenerează, iar paginile React care citesc
câmpurile se aduc la zi în același pas.

Nu intră: sensul postării (`Sens`), membrii de tip `SensStingere`,
parametrul de interogare `sens` al jurnalului de TVA și notația
`(TipTva × Sens)` din comentarii, care numește axa, nu membrul. Realizarea
și consecința pe refuzurile de seed sunt în
[raport](tr-d9b-pas7-senstva.md).
