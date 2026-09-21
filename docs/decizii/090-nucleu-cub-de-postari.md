# 90. Nucleul ca ledger n-dimensional — un singur cub de postări în locul celor patru registre; motor pur pe operand închis (`Contract = Postari + Decizii + Ipoteze`); unitatea nominalizată (lot = partidă = fișă) numită pe linie; împerecherea ca nominalizare la operare + document `Împerechere` cu tranzacție de fel `Transfer`; linia FCT postează recepția; o singură postare de stoc, `TipStoc` dispare structural; stornoul ca tranzacție distinctă = inversul cauzat ∪ atribuit; fiscala postată în `Carte=Fiscal`; ordinea TR-D6a…D10 cu Import1C ca probă supremă; XAF și React înghețate pe funcții

- **Data**: 2026-09-20
- **Stare**: activă (amendează invarianții I, III și VI, întărește II; DEPĂȘEȘTE contractul IM `docs/api/p5-felia-izolare-motor-contract.md`; concretizează 51e ca parametru al unității; închide 76-r3, IM-r1, IM-r3, IM-r4 la TR-D9)
- **Docs**: `docs/nucleu/nucleu-cub-design.md` (designul, §1–§11), `docs/nucleu/nucleu-coordonate-rapoarte.md` (pasul 1: coordonatele contra rapoartelor reale, patru amendamente structurale), `docs/nucleu/nucleu-fizica.md` (pasul 2: forma fizică măsurată, FZ-D1…D9, FZ-r), `docs/nucleu/nucleu-transfer.md` (pasul 3: TR-D1…D10, §3 ce se transferă / se rescrie / dispare, §4.1 propunerea de execuție, TR-r), `docs/nucleu/nucleu-bilant.md` (sinteza câștig / pierdere pentru decizie); probele în `run-nucleu/{coordonate,fizica,transfer}/` (inventare cu `fișier:linie`, SQL, EXPLAIN-uri, review-urile adverse). Fără cod: decizia e pasul 0 (TR-D5), oprirea lui e textul de aici.

## Regula durabilă

**Registrul sistemului e un singur cub de postări; documentul tipat rămâne
la intrare, fluxul stă la ieșire ca rezultat al motorului, iar tot ce e
sold, rest, cost, curs sau valoare rămasă e o sumă pe cub. Motorul e o
funcție pură pe un operand închis; regula se aplică la scriere, citirea
doar adună.**

(a) **Cubul.** `Postare = { Coordonate[], Cantitate, ValoareValuta, Valoare,
Cauza, Atribuit?, Unitate? }` — trei măsuri aditive, restul e proveniență.
`Tranzactie = Postare[]` balansată per spațiu de conservare, cu fel
`Operare | Storno | Transfer | Deschidere`, cu data înregistrării și `ScrisLa`
proprii. Un document operat are EXACT o tranzacție de fel `Operare`; stornoul
e a doua tranzacție, distinctă, pe același document. Coordonatele (reuniunea
interogărilor reale, probată raport cu raport în pasul 1): `Cont`, `Latura`
(D/C, doar pe spațiul contabil), `Data` (a înregistrării, 88), `Partener`,
`Gestiune` (reale + VIRTUALE: Furnizor, Client, Consum, Producție…), `Produs`,
`Unitate`, `CodTva` (tip@versiune × sens × rol Bază/Taxă), `PerioadaDeclarare`,
`Valuta`, `Carte` (Contabil | Fiscal), Analiză ×6 (CodFunctional, CodEconomic,
SursaFinantare, UnitateOrganizatorica, Proiect, CentruCost); `Cauza`
(document, linie, tranzacție) și `Atribuit` sunt legături, nu coordonate.
**Cubul este jurnalul**: formula debitor/creditor/sumă e randare, tabela de
note contabile e cel mult read model reconstructibil. Registrele de azi
(`RegistruContabil`, `RegistruStoc`, `RegistruTva`, `RegistruImobilizari`),
cele trei snapshot-uri, `PartideDeschise`, `Imperecheri`,
`Document.TotalStingere`, `TipStoc`, flag-ul de storno pe rând, `NumarNota`,
coloanele de dimensiuni per latură și `Lot.PretUnitar` ca fapt înghețat
DISPAR (transfer §3.3) — la TR-D9, nu înainte.

(b) **Trei feluri de reguli, un motor pur.** Conservarea e structurală, în
cod (valoarea pe Contabil ∪ Stoc, cantitatea pe Stoc); rezolvarea
coordonatelor e politică — date versionate pe perioadă, tabelele de azi
rămân (`RegulaContare`, `RegulaStoc`, `PoliticaTva*`, `Mapare*`,
`PoliticaMiscareSaft` re-cheiată…); admisibilitatea e a gardienilor
(perioadă, stoc negativ, partener blocat), parte cod, parte date. Motorul e
`Motor(operand închis) → Contract { Postari[], Decizii[], Ipoteze[] } |
Refuzuri`: operandul = documentul + tot ce influențează rezultatul, rezolvat
și înghețat (versiunea politicii, unitățile și soldurile lor, starea
perioadei și a partenerului), DTO, nu entitate urmărită; deciziile (alocarea
FIFO, costul, contul rezolvat) și ipotezele (ce stare s-a citit) sunt în
contract explicit. Validarea = același motor fără reținere; operarea =
motorul sub serializare, cu ipotezele reverificate; materializarea = adaptor
care scrie `Tranzactie` + `Postare`; commit-ul e al apelantului (IM-D3).
Nucleul compilează fără EF/XAF/HTTP (test de arhitectură) și are un singur
consumator: `Atlas.Conta.BackOffice.Module`.

(c) **Unitatea nominalizată, numită pe linie.** Lotul, partida și fișa de
imobilizare sunt același concept: unitatea a cărei fracție ΣValoare / Σmăsură
primară e costul, cursul sau valoarea rămasă; soldul ei e `Σ[Unitate]`.
Cantitatea e structurală pe lot (nominalizată FIFO, trasabilitate); valoarea
ieșirii e politică redusă la un parametru — unitatea de evaluare (lot = FIFO;
gestiune × produs sau produs = cost mediu; 51e devine acest parametru, tot
sub gheața per bază). Orice linie care postează pe un cont cu unitate (terț →
partidă; stoc → lot; imobilizare → fișă) POARTĂ unitatea ca dată de culegere
(`UnitateDebitId`/`UnitateCreditId` pe liniile cu postare explicită,
`LotId`/`PartidaId` pe celelalte), iar pe o postare care numește o unitate
`Partener` se SCRIE din unitate — partenerul din repartitor stă doar pe
postarea care DESCHIDE. Nota contabilă nu postează cantitate și nu atinge o
coordonată cu unitate fără s-o numească (gard de operare).

(d) **Partida.** Se deschide când o postare pe un cont cu `RolTert` (atribut
de nomenclator al contului) nu numește o partidă existentă; are identitate
proprie și atributele `(Cont, Partener, document deschizător | deschidere)`;
restul e `Σ Valoare` pe unitate, sensul = semnul restului × rolul contului.
Un document cu două conturi de terț are DOUĂ partide (FCL cu avans pe 419:
4111 și 419; regularizarea e o notă care numește ambele unități), fiindcă un
singur număr per document nu e sumă pe niciun cont. Documentele care nu
postează pe terț n-au partidă. Deschiderea de terți intră ca tranzacție de
fel `Deschidere` cu o unitate per `(Cont, Partener)` — sau per factură
deschisă, când sursa o dă — cu partenerul obligatoriu pe postare; fără
partener nu există partidă (cerință a conectorului 1C, TR-r6/r10).
„Documente cu rest" = `Σ[Data ≤ t; Unitate] ≠ 0` și își schimbă DECLARAT
semantica față de azi (63.336 → 66.658 pe Flax la 31.12.2025).

(e) **Împerecherea = nominalizarea partidei**, în două forme ale aceleiași
primitive. La operare: postarea de terț a stingătorului (plata, încasarea,
factura care regularizează un avans) numește partida pe care o stinge —
linia o alege; automat, FIFO pe partidele deschise ale aceluiași
`(Cont, Partener)` când politica o cere; contul e al liniei stingătorului,
alegerea nu traversează conturi; plafonul = restul partidei; suma peste rest
deschide o partidă nouă pe stingător. Ulterior și la desfacere: document
tipat `Împerechere` (linii = `(partida sursă, partida țintă, sumă)`), operat
prin motor, cu tranzacție de fel `Transfer` — are gard de perioadă, dată
(≥ înregistrarea ambelor partide, 88j), storno, `Cauza`. Stornoul unui
document a cărui partidă a fost nominalizată de alții se refuză până la
desfacere; stornoul stingătorului inversează și nominalizarea. Cele patru
hook-uri de stingere (`SensDeStins`, `PoateFiStins`, `CapacitateStingere`,
`SursaStingeriiAutomate`), `ImperechereService`, `ImperecheriProiectii` și
tabela `Imperecheri` dispar; „un document poate fi stins" = „are partidă".
Latura pereche a viramentului rămâne legătura celor două picioare pe 581, nu
stingere.

(f) **Fel = `Transfer`.** Tranzacție cu `Σ per (Cont, Latura) = 0`, care mută
o măsură între unități sau gestiuni ale ACELUIAȘI cont (împerecherea
ulterioară, transferul între gestiuni pe același cont). E EXCLUSĂ din
rapoartele pe cont (balanță, fișă, registru-jurnal, GLE, `TotalDebit`/
`TotalCredit`) și inclusă în cele pe unitate/gestiune (fișa partidei,
documente cu rest, fișa de magazie, PhysicalStock) — altfel fișa 4111 crește
cu 38 % și jurnalul cu 21,7 %. Rulajul brut per partidă nu e sumă (TR-r8).
`Valoare` negativă pe o latură apare DOAR în tranzacții de fel `Storno` sau
`Transfer`; D406 trece DUK cu ea.

(g) **Stocul: o linie de stoc = o postare.** Postarea pe un cont de stoc
poartă `Cantitate` și `Unitate` = lotul (1:1 cu valoare egală pe 100 % din
perechile de azi). Semn: cantitatea iese din gestiunea-sursă (−) și intră în
gestiunea-țintă (+), inclusiv virtuală. Transferul pe același cont = fel
`Transfer`; transferul care schimbă contul (`3028 = 371`) = postare reală;
consumul = `6xx (gestiune virtuală Consum, +q, lot) = 3xx (gestiune, −q, lot)`
— postarea de cheltuială poartă cantitatea și lotul; asamblarea = `345 = 301`
cu loturile ambelor laturi; custodia = cantitate pe 803x cu valoare zero.
Fostul `TipStoc` devine STRUCTURAL: Magazie/Mărfuri = contul; Consum/
Folosință/Gratuit/ProducțieNeterminată = gestiuni virtuale; Custodie = 803x.
`PoliticaMiscareSaft` se re-cheiază pe `(TipDocument, Cont, Semn, Gestiune
virtuală?)`, cu injectivitatea ca PROBĂ ModelCheck pe ambele seed-uri.
`Spatiu` (partiția fizică): **Stoc = postările cu unitate de tip lot,
Contabil = restul**; conservarea valorii se verifică pe reuniune, a
cantității doar pe Stoc; FK-ul per partiție pe `Unitate` se păstrează. Δ de
sold 3xx față de contabilul de azi (+585.404,66 pe Flax) e CONSTATARE
tranșată prin contractul 1 al reconcilierii la TR-D7 (TR-r12), nu consecință
acceptată tăcut.

(h) **Linia de stoc a facturii de intrare postează recepția**
(`3xx = 401` la net, `+Cantitate` pe lotul născut de linie, `4426 = 401` per
linie) dacă lotul numit n-are deja postare de recepție; dacă numește un lot
recepționat pe aviz (`NIR` manual, `3xx = 408`), postează `408 = 401` —
purtătorul legăturii e `LotId`. FCT deschide o singură partidă pe 401 cu
datoria integrală. NIR-ul autogenerat din FCT dispare (clonă 1:1 fără
informație proprie pe 17.814 / 34.289 de perechi); `PoliticaConex` și
mecanismul clonei dispar cu el; `NIR` rămâne tip pentru recepția fără
factură. **Testul documentului-copil**: un document conex/secundar există
DOAR dacă poartă informație proprie (dată, gestiune, loturi alese,
instrument, cont propriu); trec DSC din FCL, plata din FCT, latura pereche.
SAF-T: `MovementOfGoods`/`MovementReference` se mută de pe NIR pe FCT,
declarat.

(i) **Storno, perioadă, sold.** Stornoul = inversul EXACT al postărilor
cauzate ∪ atribuite documentului, scris ca tranzacție distinctă (`Atribuit`
e ce face definiția completă după o reevaluare). Perioada e gard; corecția
în perioadă închisă = storno legat + document nou datat în perioada deschisă
(88). `Sold` = sumă la granițele de perioadă, unic pe toate coordonatele, per
spațiu, cu invariantul de egalitate ca probă; orice număr persistat e ori
fapt (postare), ori sumă verificată. Re-proiecția e UNEALTĂ (rulează motorul
peste documentele unei perioade deschise, compară, raportează), nu sursă: o
regulă schimbată nu re-proiectează istoricul. Read model-uri doar
deterministe din postări, cu comandă de reconstrucție și probă de egalitate.

(j) **Rotunjirea e disciplină de motor.** TVA: unitatea legală e documentul
× cotă — acolo se DECIDE și se rotunjește; se POSTEAZĂ per linie prin
`Repartizeaza(taxa documentului, bazele liniilor)` (SAF-T cere `TaxAmount`
per linie); pe facturile primite taxa e DATĂ, validată cu toleranță, niciodată
recalculată. Valoarea liniei = round2(cant × preț), decizie la operare.
Ieșirea FIFO se evaluează pe raportul CURENT al unității, în secvență, ultima
ia restul ⇒ **cantitate zero pe unitate ⇒ valoare zero** (ține doar cu
evaluare la scriere serializată). O singură primitivă `Repartizeaza(total,
ponderi) → cote` cu Σ = total (Hamilton, ierarhic: exact per coordonată,
apoi în interior), folosită de reevaluare, discount global, DVI, transport.
Reziduul postat apare doar din politică (665/765, 658/758). Scările și modul
de rotunjire rămân înghețate per bază (49e, 51c, 52a).

(k) **Reevaluarea, fiscala și impozitul pe profit.** Reevaluarea = postare
doar de valoare (`Cantitate = 0`) distribuită peste postările de stoc ale
unității proporțional cu cantitatea, cu `Atribuit` per postare; contrapartida
o aduce documentul de corecție prin fluxul lui (corecția de preț, DVI pe
loturi, transportul pe lot, diferența de curs, reevaluarea imobilizării în
`Carte=Contabil`); sink pe an închis → 1174 prin politică (F27-r1 devine rând
de politică). Producția multi-nivel: reevaluarea aterizează pe sink-ul de
consum și NU se propagă în lotul produs. AMO postează contabil
(`Carte=Contabil`) ȘI fiscal (`Carte=Fiscal`, cu aceeași disciplină de
rotunjire); deductibilul e CITIRE cu politică pe granul fișă × lună,
înghețată în contractul documentului care o consumă. Impozitul pe profit =
document generat la închidere, cu Registrul de evidență fiscală ca contract
(`Ajustare { Nume, Fel = Citire | Postare, Valoare, Proveniență[] }`); ce
poartă valoare rămasă între perioade e UNITATE în `Carte=Fiscal`. Fișa de
imobilizare = cubul filtrat pe unități de tip fișă; `IDocumentCuRegistruPropriu`
dispare.

(l) **Execuția: ordinea, regimul dual, regula de oprire.** Pașii sunt
TR-D6a nucleul pur → TR-D6b declarația fluxului per tip (pilot BCS, PLT,
FCT; singurul pas care poate întoarce decizia) → TR-D7 strangler per tip →
TR-D8 citirile pe cub → TR-D9 unitățile și tăierea; TR-D10 e proba supremă
la orice pas. Fiecare pas = felie cu contract în `docs/nucleu/` (D-uri
pin-uite, regulă de oprire, review advers la închidere), branch per felie,
`main` doar cu felii închise, multiagent-delivery; niciun pas nu începe până
ce cel dinainte nu e ÎNCHIS. Reguli de execuție acceptate: **regulile pure
intră cu tipul lor** (nucleul primește la TR-D6a doar ce cere pilotul,
fiecare tip își aduce regulile la TR-D7, cu probele lor); **regimul dual e
per `TipDocument`, ca DATĂ** (`PosteazaInCub` pe ancora tipului, nu per
instanță, fără `is` pe frunză); **`Postare` e O SINGURĂ entitate EF**,
partiționarea LIST pe `Spatiu` și FK-urile per partiție sunt SQL explicit în
migrație, probate de ModelCheck; **maparea fizicii registre → cub e cod de
test** în C# (oracolul pilotului și al strangler-ului), nu script; cei 7
invarianți ai designului (§10) sunt teste pe proprietăți în
`Atlas.Conta.Nucleu.Teste`; tăierea de la TR-D9 e greenfield, fără migrare de
date (bazele se refac prin seed + Import1C, 89h). Trei oracole, refolosite:
scenele ModelCheck ale tipului, maparea fizicii, raportul Import1C pe Flax
IDENTIC pe conținut sortat cu `reconciliere-20260918-154628.txt`; orice
diferență e ori defect, ori consecință DECLARATĂ a unei TR-D — a treia opțiune
nu există (invariantul V). Cifrele de perf se compară A/B pe aceeași bază;
niciun contract de citire nu aduce o interogare per linie.

(m) **Înghețul XAF și React e pe FUNCȚII.** React e izolat prin DTO-uri; din
XAF mor doar cele patru grile pe registre (re-țintirea lor pe `Postare` la
TR-D9 e dezghețarea declarată). O schimbare în XAF sau React e permisă DOAR
dacă (1) e consecință declarată a unei TR-D în contractul feliei (ecranul de
stingere pe partidă și „documente cu rest" la TR-D8; culegerea unității pe
linie la TR-D9 — până atunci nucleul nominalizează implicit FIFO) sau (2)
repară un defect care blochează un gate (Import1C, ModelCheck,
`refuzuri.ps1`). Orice cerere de produs apărută între timp intră în
`restante.md` cu decizia ei, nu în felie. Valul de la sfârșit se ține mic
prin DTO-uri stabile și prin tăierea listei amânate în două: citiri la TR-D8,
unități la TR-D9.

## Context

Sesiunea de arhitectură din 2026-09-19 a pornit de la cererea owner-ului de
a gândi nucleul fără constrângerile proiectului (fără XAF, fără forma
curentă a registrelor) și a convers pe designul din
`nucleu-cub-design.md`. Diagnosticul (design §1): conceptele de azi sunt
corecte — structură tipată per document, motorul pe bază, politica ca date,
registre append-only, gard de perioadă, FIFO pe lot persistat — dar două
lucruri sunt greșite la nivel de MODEL, și XAF le-a încurajat pe amândouă:
documentul e centrul de greutate în locul înregistrării (patru registre cu
patru forme; închiderea, stornoul, snapshot-ul și soldul scrise de patru
ori), iar polimorfismul stă în ierarhia ORM în loc de vocabular semantic
(motorul „știe să citească" fiecare tip prin hook-uri; politica e cheiată pe
tip × câmp). Legacy-ul avea FORMA corectă de flux (predator → primitor, semn
pe cantitate) și a eșuat fiindcă a pus fluxul la INTRARE, ca structură
configurabilă; aici fluxul stă la IEȘIRE, documentul rămâne tipat, cubul e
rezultat, regula se aplică la scriere.

Decizia s-a luat în trei pași probați, câte unul per sesiune, fiecare cu
review advers de agent separat (design §11):

- **Pasul 1, coordonatele** (`nucleu-coordonate-rapoarte.md`,
  `run-nucleu/coordonate/`): patru inventare cu `fișier:linie` ale
  rapoartelor reale (contabil, TVA, SAF-T, unități); „minimal" se ține,
  „reconstruibil" se ține cu patru amendamente structurale — `Latura` D/C
  separată de semn, stornoul ca tranzacție distinctă, taxa postată per
  linie, partenerul pe postarea de terț. `Valuta` și `Atribuit` rămân
  neprobate de niciun raport real; `DeductibilCumulat` și `Luni` sunt limita
  declarată a lui „reconstruibil".
- **Pasul 2, fizica** (`nucleu-fizica.md`, `run-nucleu/fizica/`, bazele
  `Atlas.Conta.Nucleu.Fizica.x1/.x10`): un cub logic partiționat LIST pe
  `Spatiu` (F2), ales pe STRUCTURĂ (FK per partiție pe `Unitate` polimorfă;
  scrierea 1,7 contra 5,3 ms cu 9 FK-uri în loc de 24; 7 indexi în loc de
  24), nu pe viteză de citire; 8 din 13 rapoarte reconstruite identic, 4
  probează fidelitatea transformării, partidele diferă (tranșate la pasul 3).
  FZ-r2 măsurată: fișa 4111 de la `Sold` 348 ms contra F0 cu referință 248
  la ×10.
- **Pasul 3, transferul** (`nucleu-transfer.md`, `run-nucleu/transfer/`,
  review 6 MAJOR / 9 MEDIU / 5 MINOR, toate acceptate): TR-D1…D4 de mai sus,
  clasificarea transferă / rescrie / dispare (§3), ordinea TR-D5…D10 (§4) și
  propunerea de execuție (§4.1, acceptată de owner 2026-09-20).

Sinteza pentru decizie e `nucleu-bilant.md`: **noua formă se plătește prin
corectitudine și prin unicitatea mecanismului, nu prin viteză și nu prin
disc.**

## Tranșările (TR-D1…D4), cu faptele care le-au decis

Textul integral, cifrele și SQL-ul sunt în `nucleu-transfer.md` §2; aici
doar ce a decis fiecare.

- **TR-D1 partida** — trei definiții candidate (A = documentul, B = document
  × cont de terț, C = lanțul conex FCT+NIR) contra faptelor de pe Flax:
  87.478 de „partide" de azi stau pe documente care nu postează NIMIC pe un
  cont de terț; 2.036 FCL cu avans pe 419 au un `TotalStingere` care nu e
  sumă pe niciun cont; 14.341 FCT au rest NEGATIV pe cub fiindcă datoria e
  ruptă între FCT și NIR. „B e singura care se închide pe sold" s-a dovedit
  artefact de JOIN (review); argumentul lui B a rămas cel structural — restul
  trebuie să fie sumă pe un cont —, iar după TR-D3 A și B diferă DOAR pe
  documentele cu două conturi de terț. Partenerul se scrie din unitate
  fiindcă nota contabilă nu poate purta partener (7.824 de picioare de terț
  fără partener pe Flax, 4.068 pe 4111); deschiderea de terți per partener e
  cerință a conectorului fiindcă 65,5 % din soldul de terț stă azi pe
  deschidere fără partener.
- **TR-D2 împerecherea** — propunerea din fizica §4.4 („două postări pe
  același (Cont, Latura)" pentru ORICE împerechere) a fost respinsă de fapte:
  43.237 din 44.448 de împerecheri leagă laturi OPUSE, niciun document nu
  apare în ambele roluri, 22.045 sting o singură partidă. Cazul normal nu e
  transfer, e POSTARE care numește partida; transferul rămâne pentru
  împerecherea ulterioară și desfacere, ca document. Felul `Transfer` e
  exclus din rapoartele pe cont fiindcă altfel 88.896 de postări cu valoare
  negativă ar umfla fișa 4111 cu 38 % și jurnalul cu 21,7 %.
- **TR-D3 FCT postează recepția** — NIR-ul conex e clonă 1:1 (17.814 ↔
  17.814 documente, 34.289 de perechi de linii cu cantitate și valoare
  identice, `NirDetaliu.PretUnitar` 0 pe toate); împărțirea postării pe 401
  nu e cod, e ABSENȚA unei reguli de contare pe FCT/Stoc. Recepția pe aviz
  (`408`) rămâne prin `LotId` al liniei; `LinieSursaId` nu e necesar.
- **TR-D4 stocul** — 98.385 de perechi stoc ↔ contabil 1:1 cu `|Valoare|`
  EGALĂ (diferență 0,00), `TipStoc` derivabil din `(Cont, Repartitor)` cu
  zero coliziuni pe 283.498 de rânduri, cheia `(TipDocument, Cont, Semn)` a
  politicii de mișcare injectivă (0 coliziuni). Custodie/Folosință/Gratuit/
  ProducțieNeterminată au 0 rânduri (TR-r7). Δ 3xx +585.404,66 (371
  +404.030,06; 3028 +134.853,80; 303 +26.236,26…) vine din BTR între conturi,
  ASM și deschidere — postări pe care contabilul de azi NU le are;
  economia: 100.332 de postări (8,63 % din cub).

## Ordinea și regula de oprire (TR-D6a…D10)

| Pas | Ce | Oprire |
|---|---|---|
| TR-D6a nucleul pur (1–2 sesiuni) | `nou/Atlas.Conta.Nucleu` fără niciun pachet în afara BCL + `Atlas.Conta.Nucleu.Teste`: `Postare`, `Tranzactie`, `Contract`, conservarea per spațiu, `Repartizeaza`, unitatea (raport, nominalizare FIFO, deschidere), stornoul, `Sold`, rotunjirea cu contorul de jumătăți de ban; doar regulile pure cerute de pilot | cei 7 invarianți din design §10 ca teste pe proprietăți (cu felul `Transfer` în „un document = o tranzacție"); testul de arhitectură pe referințele assembly-ului; ModelCheck neatins |
| TR-D6b declarația fluxului (2–3 sesiuni) | forma care înlocuiește cele 30 de override-uri `PregatesteOperare`/`ValideazaOperare` și celelalte 11 hook-uri: un declarant per frunză, în Module, care construiește operandul închis prin adaptorul `Fapte` și DECLARĂ liniile cu măsurile lor, unitățile numite/născute, refuzurile proprii, copiii cu informație proprie; pilot BCS (două mișcări + sink), PLT (nominalizare de partidă + latura pereche), apoi FCT (recepția) | forma scrisă în contract; cele trei frunze produc din operand închis EXACT postările motorului vechi (scenele ModelCheck transformate în cub cu maparea fizicii) plus diferențele declarate de TR-D1…D4. Dacă forma nu e mai simplă decât cele 41 de hook-uri, decizia se întoarce aici |
| TR-D7 strangler per tip (6–8 sesiuni) | motorul nou scrie `Tranzactie`/`Postare` în aceeași tranzacție de comandă în care motorul vechi scrie registrele; `PosteazaInCub` per `TipDocument`; ordinea: tipurile pilotului, apoi după volumul pe Flax, tipurile cu conex la urmă; Import1C operează prin ambele | per tip: Σ D = Σ C per tranzacție, Σ per cont / lot / cod TVA × perioadă egale cu registrele (cu diferențele DECLARATE: partidele, 401 pe FCT, sink-ul de consum, Δ 3xx prin contractul 1 — TR-r12); Import1C identic cu baseline-ul; ModelCheck verde pe ambele profiluri cu probele de regulă neatinse; `refuzuri.ps1` 294/294; D406 trece DUK; injectivitatea politicii de mișcare pe cheile de după TR-D3 |
| TR-D8 citirile pe cub (3–4 sesiuni) | proiecțiile din pasul 1 §3, `Sold` unic + închiderea pe `Sold`, fișa/registrul de imobilizări pe unitate, „documente cu rest" și ecranul de stingere pe partidă (indexul `(Unitate, Data)`, FZ-r3), probele de formă rescrise pe `Postare`; raport cu raport, tăiat pe cub când diff-ul din `egalitate.md`, portat în ModelCheck, e zero; A/B-ul FZ-r1 aici | 13/13 rapoarte ale fizicii identice sau cu diferența declarată; D406 DUK; perf ≤ cifrele fizicii A/B pe aceeași bază INCLUSIV citirea per partidă; Import1C reconciliat din cub |
| TR-D9 unitățile și tăierea (3–4 sesiuni) | împerecherea ca nominalizare + `Împerechere`, AMO în două cărți, reevaluarea cu `Atribuit` (prima instanță reală: DVI pe loturi), apoi tăierea din (a); grilele XAF re-țintite; motorul vechi dispare; lanțul de migrații se resetează | Import1C identic din cub singur; ModelCheck cu probele de formă rescrise și cu scenele care acoperă ce Import1C nu atinge (`DVI-V*`, `IMO-V*`, `AMO-V*`, `COR-V*`, `F19-D*`, `PAR-V*`, stornoul unui document nominalizat); `--dump-integritate-tph` zero; `verifica:drift` zero sau driftul declarat pe partide |
| TR-D10 proba supremă | la orice pas, raportul Import1C pe Flax IDENTIC pe conținut sortat cu `reconciliere-20260918-154628.txt` | orice diferență = defect sau consecință declarată a unei TR-D |

Total estimat ≈ 18–22 de sesiuni cu produsul înghețat. Contractul feliei
TR-D6a se scrie la deschiderea ei, în `docs/nucleu/`, pe precedentul
`docs/api/p5-*-contract.md`.

## Ce se transferă, ce se rescrie, ce dispare

Criteriul (transfer §3): **regula** (intrări plate → ieșiri) se transferă;
**mecanismul** formei de azi se rescrie o singură dată pe cub; ce exista
doar din cauza formei dispare.

- **Se transferă** neschimbat: cele 20 de frunze cu liniile și culegerea lor
  (+ câmpul de unitate din (c) și tipul nou `Împerechere`); politicile ca
  date (16 tabele; `PoliticaConex` dispare); `Potrivire`, `DimensiuniResolver`,
  `Fapte`, `ImpliciteService` (sunt deja forma nucleului); regulile pure din
  servicii (`StocService.ValoareGolire`, `AmortizareService.CotaLunara/…`,
  `InchidereTvaService.CalculeazaLinii`, `TvaService.CalculeazaValori`,
  `PerioadaService.Verifica`, `SolduriService.Referinte`, `DescarcareService`
  cu pin-uri înaintea FIFO) — ca funcții pe operand închis, fiecare cu tipul
  care o consumă; lanțul de perioade (88); `GardianEditare` (a), (d)–(o) cu
  (b) re-țintit pe `Postare`/`Tranzactie`/`Sold` și (c) pe `Împerechere`;
  adaptoarele (`RandDupaCheie`, `TranzactieComanda`, `CititorTipDocument`,
  `OperareApi`, controllerele, `EroriDto`); harness-ul ModelCheck și probele
  de REGULĂ (≈1.000 + scenele e2e); Import1C cu punctele de atingere numite
  (≈15 citiri de `TipStoc`, `Deschidere` scrie tranzacția); nomenclatoarele,
  seed-ul, securitatea.
- **Se rescrie** o dată: `MotorOperare` (814 linii) → motor pur + adaptor de
  materializare; `StocService` + `LoturiCulegereService` + `DescarcareService`
  → unitatea (FIFO în SQL pe `(Produs, Gestiune)`, evaluare la scriere
  serializată); `ImperechereService` → (e); `SolduriService` → `Sold` unic;
  `AmortizareService` + `RegistruImobilizari` → fișa ca unitate;
  `RegistruTvaService` + `TvaService` → postări în spațiul Fiscal;
  `PerioadaService.Inchide/Redeschide` → `Sold` la graniță; `CorectieService`
  → storno legat + document nou, nimic rescris; proiecțiile (6.384 linii) pe
  forma pasului 1 §3; probele de FORMĂ din ModelCheck (≈293, TR-r9); cele
  patru grile XAF pe registre; `Import1C/Bucla.Opereaza`.
- **Dispare**: lista din (a) + hook-urile de stingere, `IDocumentCuRegistruPropriu`,
  `SensDeStins`, `ImperecheriProiectii`, puntea 891 a importului,
  `LoturiCulegereService` în forma pe `ModifiedObjects`.

## Raportul cu invarianții

Testat contra `docs/invarianti.md`, cu amendamentele scrise ACOLO ca
reconciliere (starea-țintă; codul de azi rămâne pe litera veche până la
TR-D9):

- **I amendat**: orice postare are document (neschimbat), dar împerecherea
  DEVINE operație — document `Împerechere` cu tranzacție de fel `Transfer`;
  deschiderea rămâne excepția unică, ca tranzacție de fel `Deschidere` fără
  document, dar CU unitate (lot / partidă) și partener pe terți.
- **II întărit**: motorul nu cunoaște frunzele fiindcă nu cunoaște documente,
  ci operanzi închiși; regimul dual e dată pe tip (`PosteazaInCub`); `RolTert`
  e atribut de nomenclator al contului, nu al frunzei; discriminatorul rămâne
  etichetă (89).
- **III amendat în literă**: „registrele" devin cubul — un singur registru,
  append-only, complet rezolvat, scris doar de motor; `TotalStingere` nu mai
  e „stare a documentului": restul e `Σ[Unitate]`, sumă pe cub, iar cubul e
  singurul adevăr al agregării. Demarcația „agregatul scanează registre;
  starea documentului se citește de pe document" rămâne pentru atributele
  documentului (număr, dată fizică, instrument), citite prin `Cauza`.
- **IV neatins**: politicile rămân date care parametrizează mecanisme; două
  tabele se re-cheiază pe coordonate structurale (cont, gestiune), nu pe un
  enum de cod; unitatea de evaluare e parametrul cu nume al lui 51e.
- **V neatins**: Import1C rămâne proba, diferențele se raportează — inclusiv
  Δ 3xx.
- **VI amendat**: prețul lotului = raportul unității (`ΣValoare / ΣCantitate`),
  fiecare postare e fapt; valoarea ieșirii rămâne fapt scris la operare;
  metoda rămâne una per bază, sub gheață, ca parametru al unității.

## Raportul cu contractul IM

`p5-felia-izolare-motor-contract.md` (2026-09-10, reanalizat 2026-09-18)
avea aceeași țintă — faza pură, contracte de citire, commit-ul la apelant —
pe FORMA de azi; cuplajul pe care voia să-l izoleze creștea cu 58 % în opt
zile. TR-D6a/b și TR-D7 o realizează pe forma nouă; contractul își schimbă
starea în **depășit de 90**. Ce IM livrase deja se preia: precedentele pure
(`Potrivire`, `DimensiuniResolver`, `CotaLunara`, `CalculeazaLinii`,
`Verifica`), `SolduriService` ca adaptor, `RandDupaCheie`, tranzacția la
apelant (`TranzactieComanda`), `CititorTipDocument`. Pașii IM 0a–0c și 1–7 NU
se mai execută ca felie proprie. Restanțele IM-r1…r7, re-evaluate:

- IM-r1 (nivelul specific per tip ca serviciu) — absorbită de declarantul
  per frunză (TR-D6b); se închide la TR-D9.
- IM-r2 (adaptorul EF direct, hostul fără XAF) — rămâne deschisă, sub 90:
  nucleul e pur, dar singurul consumator e `Module`; hostul fără XAF e
  decizie proprie după TR-D9.
- IM-r3 (`IDocument`/`ILinie` peste bază) — absorbită: operandul închis e
  DTO, nu interfață peste entitate; se închide la TR-D9.
- IM-r4 (identificatorul semantic al tipului + fabrici) — depășită de 89
  (`CititorTipDocument.Clasa`) și de regimul dual ca dată; închisă.
- IM-r5 (async efectiv în host-uri) — rămâne deschisă, felie proprie cu
  cifră, după TR-D9.
- IM-r6 (concurența între operatori) — = 25f / F27-r8, rămâne acolo.
- IM-r7 (extensia per client a modelului) — rămâne deschisă, spike separat.

## Bilanțul (ce a cântărit)

Din `nucleu-bilant.md` §1 și §5, fără cifre noi:

| Dimensiune | Verdict | Cifra |
|---|---|---|
| Corectitudinea modelului | câștig mare | 87.478 „partide" fără datornic; 14.341 FCT cu rest negativ; Δ 3xx 585 k azi INVIZIBIL; balanța „pe partener" e pe sediu în 96 % din 4111 |
| Un mecanism în loc de patru | câștig | 4 registre + 3 snapshot-uri + `PartideDeschise` → `Postare` + `Sold`; 24 FK → 9; 24 indexi → 7 |
| Izolarea motorului (ținta IM) | câștig | 15 hook-uri × 20 frunze → declarație pe operand închis |
| Scrierea unui document | câștig, din FK-uri, nu din cub | 1,7 ms contra 5,3 |
| Citirile lunii | câștig | jurnal 23 → 7 ms; D394 18 → 8; fișa × partener 719 → 33 |
| Citirile integrale de istoric | pierdere | fișa 4111 de la `Sold` 348 contra 248 ms (+40 %); terți SAF-T +60 % |
| Disc | pierdere | 1,5–1,8× (11,56 M postări contra 6,72 M rânduri) |
| Semantica vizibilă | pierdere de continuitate | „documente cu rest" 63.336 → 66.658; FCL cu avans 104,02 → 16,61; SAF-T în 4 locuri |
| Disciplina `Fel = Transfer` | cost permanent | uitată: fișa +38 %, jurnal +21,7 % |
| Declarația fluxului (TR-D6b) | necunoscuta principală | NESCRISĂ; singurul pas care poate întoarce decizia |
| Timpul | cost sigur | ≈ 18–22 de sesiuni cu produsul înghețat; reversibil pas cu pas prin `PosteazaInCub` |

Decizia s-a luat pe corectitudine și pe unicitatea mecanismului; cine ar
alege forma pentru performanță s-ar înșela pe cifre. Costul e făcut
reversibil de strangler-ul per tip ca dată: cubul se scrie LÂNGĂ registre,
un tip nemigrat rămâne pe forma veche.

## Ce rămâne deschis (restanțele deciziei)

Numele stau în `restante.md`; textul integral în `nucleu-transfer.md` §7 și
`nucleu-fizica.md` §6.

- **TR-r2** notele pe conturi de stoc fără lot (2.997 pe Flax, cinci
  corespondențe): conectorul decide per corespondență la TR-D7.
- **TR-r3** fizica re-măsurată cu postarea de stoc unificată și `Transfer`
  exclus (`Spatiu` re-definit, FK per partiție, indexul `(Unitate, Data)`).
- **TR-r4** recepția fără factură (NIR pe aviz, 408): cerință de produs de
  confirmat; NIR rămâne tip până atunci.
- **TR-r5** DSC din FCL trece testul documentului-copil azi; de re-judecat
  dacă descărcarea devine clonă fără alegere de loturi.
- **TR-r6** conectorul 1C: data reală a împerecherii și deschiderea de terți
  per partener / per factură deschisă.
- **TR-r7** sink-urile bugetare ca gestiuni virtuale / 803x: probate la
  primul seed bugetar pe cub, cu injectivitatea.
- **TR-r8** rulajul brut per partidă nu e sumă; fișa partidei = fereastră.
- **TR-r9** probele de formă din ModelCheck (≈300): inventar rescrie/șterge la
  TR-D8/D9.
- **TR-r10** deschiderea ca tranzacție de fel `Deschidere` cu unitate și
  partener; `Deschidere.cs` scrie tranzacția, nu rânduri bloc.
- **TR-r11** `Numar`, `DataScadenta`, `Autogenerat`, `DocumentSursa` rămân
  atribute scrise la operare sub gardianul (a).
- **TR-r12** Δ 3xx +585.404,66: tranșată prin contractul 1 la TR-D7;
  constatare, nu consecință acceptată.
- **FZ-r1** granul lui `Sold` contra snapshot-urile de azi (și dacă un read
  model mai grosier merită): gate la TR-D8, nu condiție prealabilă (FZ-r2 e
  măsurată și absorbită aici).
- **FZ-r3…r10**: indexul per partidă, creșterea coordonatelor pe istoric
  lung, partiționarea pe an (redeschisă doar la prag), reperul
  `TaxInformation`, costul FK la scară, BRIN pe `Data`, împerecherea pe date
  cu împerecheri datate real.
- **Neprobate de niciun raport real** (transfer §6): valuta și partidele în
  valută, `Atribuit`, stornoul pe cub, imobilizările pe cub, avansul de
  furnizor, compensarea 401 = 4111, DVI pe loturi, profilul bugetar — se
  acoperă prin scene ModelCheck la TR-D9 (review E8).
- **Granițe declarate** (design §11): producția multi-nivel fără propagare;
  tranziția de politică de evaluare (medie → FIFO) cere o reevaluare de
  aliniere, nescrisă.
- Restanțele de produs pe care 90 le rezolvă STRUCTURAL rămân deschise până
  la pasul care le închide: 76-r3 (hook-urile de stingere, TR-D9), F27-r1
  (rând de politică, TR-D9), F27-r11 (partenerul din unitate, TR-D9),
  F27-r16 și F28-r1 (`DocumenteCuRest`/`ImperecheriProiectii` dispar la
  TR-D8/D9).
