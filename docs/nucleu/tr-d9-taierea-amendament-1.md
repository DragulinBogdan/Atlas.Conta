# TR-D9a — amendamentul 1 la contract: pentru aprobare

- Data: 2026-10-05. Stare: **aprobat de owner, 2026-10-05**, după hotărârea
  fiecărui punct; transcris în contract. Contractul amendat:
  [`tr-d9-taierea-contract.md`](tr-d9-taierea-contract.md), la `88b45a7`.
- Surse: consultarea cub vs registre
  (`docs/consultations/2026-10-05-cub-vs-registre/`, rundele 2 și 3),
  inventarul pasului 1 (`tr-d9-inventar.md`, I1…I8), explorarea închisă fără
  schimbare de model (`docs/consultations/2026-10-05-nod-si-galeata/`).
- Hotărârile owner-ului din 2026-10-05, pe care se sprijină: registrele se
  taie, fără prototip de registre derivate, cu măsurătoarea la volum
  înaintea pasului 6; cheia de pereche pe postare intră acum; I2 se aprobă;
  I5 devine refuz la editare; poziția fără unitate rămâne cum e azi.
- Forma din D9-A5, scara de volum ca raport (D9-A1) și nivelul 1 la D9-A8
  sunt confirmate de owner; vezi sfârșitul. Niciun punct nu mai e deschis.

Verdictul consultării, pentru context: ambii analiști recomandă cubul singur
(opțiunea A), cu TR-D9a amendat. Analistul care alesese registrele în runda
oarbă a trecut la A după citirea codului.

## D9-A1 — Probele dinaintea tăierii (pas nou, 5b)

Pasul 6 șterge scriitorul vechi și martorii registru → cub. Trei lucruri nu
se mai pot obține după el și se fac înainte, sub regimul dual, fără cod de
produs:

1. **Ultima reconciliere registre ↔ cub, arhivată.** `ModelCheck
   --reconciliere-cub` pe baza de import rămasă (`.Flax.M1s`), cu ieșirea și
   cu explicația fiecărei diferențe în `run-verificari/`. Diferența de
   +585.404,66 pe 3xx dintre registrul de stoc și cel contabil (TR-r12) se
   consemnează cu valoarea ei pe cub. E constatare arhivată, nu gate: o
   diferență nouă, neexplicată, se raportează owner-ului.
2. **`PerfCub` în regim dual, ca termen de comparație.** Regula de oprire
   cere deja costul comenzii fără registre „A/B pe aceeași bază" (D9-D15, 6);
   termenul A trebuie măsurat cât scriitorul vechi există. Aceeași scenă,
   același container, aceleași puncte.
3. **Scara de volum, cu ruta securizată.** Nu există cifră de volum: scena
   are 7.951 de postări. Se măsoară cititorii comuni (balanța, fișa contului,
   registrul jurnal, soldurile de loturi și de partide, snapshot-urile) pe cel
   puțin 5 milioane de postări, obținute prin multiplicarea scenei direct în
   SQL, pe ruta de sistem și pe ruta securizată `Vizibila`, separat. Cifrele
   sunt ale citirii; scrierea nu se măsoară așa.

Rezultatul scării se raportează owner-ului înaintea pasului 6 și intră în
decizia 110. Nu oprește tăierea: un registru derivat sau un model de citire
cu cheie de vizibilitate se poate construi din cub și după ea: cu cheia de
pereche (D9-A2) forma pe perechi a registrelor devine derivabilă din cub, iar
`TipStoc` dispare oricum (D9-D8).

## D9-A2 — Cheia de pereche pe postare (pas nou, 7b)

Azi cubul nu ține corespondența dintre cele două postări ale unei mișcări;
fișa contului o reconstituie ca mulțime și întoarce contrapartida nulă când
tranzacția are mai multe conturi pe sensul opus
(`Proiectii/ContabilProiectii.cs:486-487`).

- **Ce se scrie.** `Postare.Pereche`: ordinalul mișcării în tranzacția ei.
  Cele două postări născute din aceeași `Miscare` sau `Mutare` poartă
  același ordinal, și numai ele. Ordinalul se dă în nucleu, pe orice cale
  care produce postări dintr-o mișcare sau mutare, nu la materializare.
- **Ce rămâne fără pereche.** Postările de transformare (linia reală și
  contraponderea ei rămân legate prin cauză, iar corespondența consum ↔
  produs e de grup, nu de pereche) și postările de deschidere. Ordinalul lor
  e nul.
- **Inversa.** Postarea de storno poartă ordinalul originalului ei.
- **Invariantul.** În nucleu, în `Conservare`: fiecare ordinal nenul al unei
  tranzacții are exact două postări, cu aceeași cauză și aceeași valoare
  absolută, pe laturi opuse la `Operare` și pe aceeași latură cu semne opuse
  la `Transfer`. Dublura persistată intră în `INV-CUB`, cu mutantul ei.
- **Ce nu e.** Nu e coordonată: nu intră în cheile de sold, în snapshot-uri
  și în nicio agregare.
- **Cititorii nu se schimbă în TR-D9a.** Fișa contului și registrul jurnal
  întorc aceleași rezultate ca azi. Folosirea cheii de către ei e restanța
  D9-r4.

Coloana intră în `InitialCreate` la pasul 7b, imediat după eliminarea
atomică, cu bazele recreate. Nu schimbă nicio cifră de catalog.

## D9-A3 — I2: recepția facturii intră în gardul analizei obligatorii

D9-D4, „domeniul", se completează: intră și postările recepției liniei de
stoc făcute de factură (`DeclarantFacturaIntrare.Receptia`), deși nu se nasc
dintr-o regulă de contare. Refuzul se mută de pe NIR-ul conex pe factură:
aceeași regulă, alt moment. Diferența postată de NIR-ul conex rămâne păzită
de `ReceptiiConexe.VerificaAnaliza`.

E schimbare de comportament: o factură acceptată azi fără dimensiunile
obligatorii ale contului de stoc e refuzată la operare. Intră în lista din
D9-D1 ca schimbarea 5. Scena bugetară obligatorie se scrie la pasul 2, în
cele trei forme din D9-D4: trece cu angajament și fără cod economic, refuză
atomic când lipsesc amândouă, trece cu cod economic explicit.

## D9-A4 — I5: regula de contare fără consumator e refuzată la editare

O `RegulaContare` pe un tip al cărui declarant nu contează prin reguli (azi
ASM, BTR, PIF, AMO, CAS, NTC, ITV, DVI) produce notă numai în registru. După
tăiere n-ar mai avea niciun efect, în tăcere.

- Declarantul spune singur dacă contează prin reguli, printr-un membru al
  `IDeclarant`. Gardianul de editare citește membrul prin declarantul tipului;
  nu ramifică pe tip (25b, 32a).
- `GardianEditare.VerificaRegulaContare` refuză regula, cu un cod propriu,
  când declarantul tipului nu contează prin reguli sau când tipul n-are
  declarant.
- Seed-ul nu are astfel de reguli pe niciun profil; proba o confirmă.

Intră în lista din D9-D1 ca schimbarea 6. Scenariul se scrie la pasul 2 și
refuzul intră la pasul 6, cu celelalte schimbări de comportament: pasul 2
admite numai refuzuri echivalente cu cele de azi. Restanța D9-r2,
propusă în inventar, nu se mai deschide.

## D9-A5 — Inversa fiscală se naște finală

`Materializare.ReatribuieInversaFiscala` (`Cub/Materializare.cs:213-222`)
schimbă perioadele și marcajul postărilor de storno după ce au fost
inserate. `CorectieService.Corecteaza` o cheamă în aceeași tranzacție de
comandă cu stornarea, înaintea commit-ului (`Motor/CorectieService.cs:54,
72, 95-103`). Niciun fapt confirmat nu e rescris, dar e singura actualizare
pe o `Postare` existentă găsită la căutare în produs.

Un rând compensator ar pune în cartea fiscală două rânduri în plus pentru
aceeași corecție și ar muta taxa printr-o perioadă în care n-a stat
niciodată. De aceea nu se propune.

Se propune: atribuirea fiscală a corecției se calculează înaintea stornării
și intră în inversă la construcția ei. `ReatribuieInversaFiscala` dispare.
Proba: pe toată integrala, nicio postare existentă nu ajunge modificată la
commit. Rândurile finale sunt aceleași ca azi; nu e schimbare de
comportament. Se face la pasul 6, unde dispare și perechea ei de pe
`RegistruTva`.

Oprire: dacă atribuirea nu poate intra în stornare fără altă cifră, forma
de azi rămâne ca excepție declarată în decizia 110, cu aceeași probă
restrânsă la postările confirmate.

## D9-A6 — Precizările I1, I3, I4, I6, I7, I8

Se transcriu în contract cum le propune inventarul, fără altă schimbare de
comportament:

| Id | Unde | Ce se precizează |
|---|---|---|
| I1 | D9-D2 | `IDocumentCuRegistruPropriu` se redenumește și rămâne hook polimorf al frunzei (operare, anulare, stornare, cele două motive); dispar numai scrierile de registru |
| I3 | D9-D6 | cititorul SAF-T al plăților trece pe `Document.Laturi()` la pasul 4, înaintea scoaterii `LaturaContPropriu` la pasul 7; cifrele `Payments` neschimbate |
| I4 | D9-D4 | cele trei refuzuri de ștergere se re-țintesc pe `Postare` la pasul 4, cu probă pe fiecare |
| I6 | D9-D5 | schimbarea 4 se confirmă: refuzul tipului inert, cu un singur cod, la pasul 6; SC-RLF-12, SC-RDC-16, SC-DVI-12 rescrise nominal |
| I7 | D9-D3 | prețul de intrare al lotului de deschidere rămâne ce a pus creatorul lotului; un rând de catalog fixează cifra |
| I8 | D9-D10 | invariantul `CITIRE_PARTIDE_POLITICA` se judecă la pasul 4: cade cu martorii sau se reformulează pe o sursă independentă; SC-CIT-65, SC-CIT-43 și mutantul îi urmează soarta |

## D9-A7 — Poziția fără unitate rămâne cum e

Nu se schimbă nimic: toleranța condiționată la partide, niciun gard la
loturi, gardul de semn la fișe. Consultarea a cerut un contract explicit;
explorarea regimului declarat pe cont s-a închis fără schimbare. Decizia 110
consemnează limita: nota contabilă poate lăsa un sold pe cont în afara
unităților, iar diferența față de loturi o semnalează azi numai
reconcilierea SAF-T. Raportul poziției fără unitate e restanța NG-r4.

## D9-A8 — Versiunea politicii din explicație: regula care a decis (pas nou, 7c)

Explicația persistată înregistrează versiunea politicii ca text fix:
`new VersiunePolitica("seed", …)` (`Motor/Fapte.cs:144`), iar API-ul
explicațiilor o arată ca atare și când clientul a editat regulile
(`Api/Explicatii/ExplicatieTranzactieDto.cs:45-49`). Owner-ul a ales nivelul
1 din patru (tabelul de la sfârșit).

- **Ce se reține.** Pentru fiecare rând de politică din care declarantul a
  luat un fapt folosit la o postare: felul politicii, identificatorul
  rândului și contorul lui de versiune. Contorul există deja pe orice rând de
  politică (`Editabila.OptimisticLockField`) și crește la fiecare salvare.
  Ipoteza `VersiunePolitica` își schimbă forma în acest sens, una pe rând
  consumat, fără duplicate; textul fix dispare.
- **Legătura cu linia.** Decizia `ContRezolvat` poartă identificatorul
  regulii de contare când contul vine dintr-o regulă; `ContareLinie.Regula`
  îl are deja la îndemână.
- **Domeniul.** Tabelele de politică (`Politica`) consumate prin operand.
  Lista lor nominală, declarant cu declarant, se scrie înaintea codului, cu
  verdict pe fiecare fapt: reținut sau „nu decide postări". Un declarant care
  nu consumă reguli nu scrie ipoteza.
- **Ce nu intră.** Nomenclatoarele care decid și ele contarea (contul
  implicit al tipului de material, steagurile contului, conturile tipului de
  TVA): rezultatul lor e deja în `ContRezolvat`, cu sursa. Conținutul regulii
  la momentul operării nu se reține.
- **Cititorul.** DTO-ul explicației înlocuiește cele două câmpuri de azi cu
  lista regulilor consumate și spune, pe fiecare, dacă rândul s-a schimbat de
  la operare: contor diferit sau rând dispărut.
- **Formatul.** Explicația trece la versiunea 2. Nu se scrie cititor pentru
  versiunea 1: pasul 7 recreează bazele.
- **Probele.** Operarea reține regula câștigătoare și contorul ei. După
  editarea regulii prin ușa gardianului, o operare nouă reține contorul nou,
  explicația veche rămâne neschimbată și e arătată ca „schimbată". O regulă
  a clientului ștearsă apare ca „schimbată". Tipul fără reguli dă lista vidă.

Oprire: dacă contorul nu se poate citi în proiecția faptelor sau nu crește pe
una dintre ușile de scriere ale politicilor, se raportează owner-ului.

Nivelurile 2 și 3 și nomenclatoarele care decid rămân restanța D9-r3.

## D9-A9 — Limite consemnate în decizia 110

Decizia 110, scrisă la pasul 8, spune explicit ce nu acoperă tăierea:

- registrele derivate nu se construiesc; se pot deriva din cub ulterior;
- cheia de pereche nu acoperă transformările;
- „numai cubul" nu e literal la imobilizări: fișa ia metoda, durata și
  categoria din liniile documentelor (`Cub/Citiri/Imobilizari.cs:56-70`);
- corecția de preț fără diferență de cantitate nu postează nimic
  (`Declaratii/DeclarantNir.Diferenta.cs:43-45, 60`); corecția de preț după
  consum rămâne a TR-D9b;
- Custodia și ALOP sunt extensii separate; tăierea nu le probează;
- explicația reține care regulă a decis și dacă s-a schimbat, nu și cum arăta
  la operare (D9-A8);
- poziția fără unitate (D9-A7).

## Ce se schimbă în textul contractului

**D9-D1, lista închisă** primește trei rânduri:

5. Recepția liniei de stoc a facturii e păzită de gardul analizei
   obligatorii la operarea facturii (D9-A3).
6. Regula de contare pe un tip al cărui declarant nu contează prin reguli e
   refuzată la editare (D9-A4).
7. Explicația persistată și API-ul ei rețin regulile de politică consumate,
   cu contorul rândului, în locul textului fix; formatul trece la versiunea 2
   (D9-A8).

**D9-D15, pașii:**

| Pas | Ce se adaugă | Schimbă comportament |
|---|---|---|
| 2 | scena bugetară a recepției facturii (D9-A3); scenariul regulii fără consumator (D9-A4) | D9-A3: schimbarea 5 |
| 4 | I3, I4, I8 (D9-A6) | nu |
| 5b, nou | probele dinaintea tăierii (D9-A1); fără cod de produs | nu |
| 6 | refuzul regulii fără consumator (D9-A4); inversa fiscală născută finală (D9-A5) | schimbarea 6 |
| 7b, nou | cheia de pereche: nucleu, materializare, `InitialCreate`, invariant (D9-A2) | nu |
| 7c, nou | versiunea politicii în explicație: ipoteza, decizia, DTO-ul, formatul 2 (D9-A8) | schimbarea 7 |
| 8 | decizia 110 cu limitele din D9-A9 și cu rezultatul scării de volum | nu |

**Regula de oprire** se completează: „cele patru schimbări" devin „cele
șapte"; probele din D9-A1 sunt arhivate înaintea pasului 6; invariantul
perechii e verde în nucleu și în `INV-CUB`; nicio postare existentă nu
ajunge modificată la commit (D9-A5); probele din D9-A8 sunt verzi și
`verifica:drift` e zero după regenerare.

**D9-D13, restanțe noi ale feliei:**

- **D9-r3** — explicația de audit dincolo de nivelul 1: conținutul regulii
  la momentul operării (din jurnalul de audit sau din politici cu istoric
  propriu) și nomenclatoarele care decid contarea (D9-A8). `după PoC`.
- **D9-r4** — cititorii care folosesc cheia de pereche: contrapartida din
  fișa contului și conturile corespondente din registrul jurnal (D9-A2).
  `după PoC`.

**„Ce NU intră"** primește: NG-r2 și NG-r4, candidate pentru TR-D9b; NG-r1,
de probat separat; NG-r3, odată cu BPR.

## Confirmările owner-ului (2026-10-05)

1. **D9-A5, forma: confirmată.** Inversa fiscală se naște finală, fără rând
   compensator.
2. **D9-A1, scara de volum: numai raport**, fără prag fixat înainte.
3. **Costul adăugat: acceptat** (scara de volum, pasul cheii de pereche).

4. **D9-A8: nivelul 1**, în această felie. Nivelurile, măsurate pe cod:

| Nivel | Ce înseamnă | Ce dă | Mărime estimată |
|---|---|---|---|
| 0 | limita declarată în decizia 110; explicația păstrează textul fix | nimic nou; explicația reține deja contul rezolvat și sursa lui pe fiecare linie (`ContRezolvat`) | zero |
| 1 | explicația reține regula care a decis: identificatorul rândului și contorul lui de versiune, pe care orice rând de politică îl are deja (`Editabila.OptimisticLockField`) | se poate spune dacă regula s-a schimbat de la operare; nu și cum arăta atunci | una până la două zile; formatul explicației trece la versiunea 2 |
| 2 | reconstituirea conținutului vechi din jurnalul de audit XAF, care e deja activ | cum arăta regula la operare | două până la patru zile; acoperirea jurnalului pe cele trei uși de scriere e neverificată; leagă auditul de XAF |
| 3 | politici cu istoric propriu: valabilitate sau versiuni pe cele circa douăzeci de tabele, rezolvarea la data documentului, seed pe versiuni, ecrane | reconstituire completă, fără XAF | săptămâni; cerință de produs nouă (090 m) |

Estimările sunt ale sesiunii care a scris amendamentul, nemăsurate.
