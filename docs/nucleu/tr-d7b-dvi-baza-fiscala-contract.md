# DVI — baza vamală distinctă în cartea fiscală

2026-09-23. **Direcție aprobată de owner:** baza distinctă în `Carte=Fiscal`,
în același cub, cu echilibrare proprie și fără efect asupra balanței
contabile sau partidelor. **Forma concretă a fost aprobată și implementată.**
DVI este activat pe cub numai în profilul privat; bugetarul rămâne inert.

Completează T-D8 din `tr-d7b-tipuri-ramase-contract.md` și B-D8 pct. 5 din
`tr-d6b-declaratia-fluxului-contract.md`. Execută 090 (a/k) și amendamentul 091;
nu schimbă reprezentarea fiscală a FCT/FCL/retururilor. T-r10 se închide
numai după implementare și probe, nu prin aprobarea direcției.

## DVI-B1 — Faptul care trebuie păstrat

Pe DVI, `DocumentDetaliu.Valoare` este baza vamală culeasă, iar
`ValoareTva` este taxa culeasă sau calculată (086). Baza 100 cu taxa 21
produce numai nota contabilă 4426 = 446: 21. Baza nu se poate recupera exact
din taxă/cotă: 100 și 100,01 pot produce aceeași taxă 21 după rotunjire.

Cubul trebuie să păstreze independent ambele cifre, fără citirea
valorilor din document pentru reconstruirea jurnalului fiscal. Identitatea
documentului, MRN-ul și celelalte atribute descriptive se citesc în
continuare prin `Cauza`, conform B-D8 pct. 5.

## DVI-B2 — Cele patru postări, fără conturi noi

Nota de taxă folosește politica existentă. Mișcarea de bază se echilibrează
pe **același cont de TVA deductibil** în cartea fiscală. Contul se rezolvă
din `TipTvaFapt.ContTvaDeductibilId`; 4426 este exemplul profilului, nu o
constantă în declarant. Nu se adaugă un cont fictiv în plan sau un GUID
fără nomenclator.

Exemplu: bază 100, tip IMP21, taxa 21, contrapartidă 446:

| Carte | Latură | Cont | Valoare | CodTva | Partener | Unitate |
|---|---|---|---:|---|---|---|
| Contabil | Debit | 4426 | 21 | IMP21 / Achiziție / Taxă | predatorul DVI | null |
| Contabil | Credit | 446 | 21 | null | numai dacă se deschide partidă | numai dacă 446 are RolTert |
| Fiscal | Debit | 4426 | 100 | IMP21 / Achiziție / Bază | predatorul DVI | null |
| Fiscal | Credit | 4426 | 100 | null | null | null |

Creditul fiscal echilibrează măsura de bază. Nu este datorie, TVA colectat,
venit sau bază negativă în jurnal; nu poartă `CodTva`. Cele două postări
fiscale au sold net zero inclusiv pe contul 4426. Rulajele lor de 100 sunt
vizibile la inspectarea cărții fiscale și nu reprezintă rulaje de taxă.

Ambele postări fiscale au aceeași `Cauza(document,linie)`, aceeași dată de
înregistrare, `Gestiune` internă a DVI și `Analiza` liniei. Cantitatea și
valoarea în valută sunt 0; `Produs`, `Unitate`, `Valuta`, `Atribuit` sunt
null. Numai debitul fiscal are perioada declarării și partenerul fiscal;
creditul fiscal are și `PerioadaDeclarare=null`. Ambele sunt în
`Spatiu.Contabil`: spațiul este derivat din unitate, iar cartea fiscală
nu este o partiție nouă.

O linie obișnuită produce două mișcări, patru postări, într-o singură
tranzacție `Operare`. Fiecare carte se balansează separat. Mișcarea bazei
este `Operare`, nu `Transfer`: înregistrează un fapt fiscal nou.

**Consecința alegerii contului:** rolul Bază, cartea și codul fiscal sunt
semantica măsurii 100. Un raport care numește orice rulaj 4426 „TVA” fără
carte/rol este incorect. Alegerea evită o nouă familie de conturi tehnice,
cu prețul acestei discipline explicite de citire.

## DVI-B3 — Variante și reguli de unicitate

Amendament aprobat (103e): unicitatea Bază/Taxă este completată cu cel mult
o Autocolectare pentru achizițiile cu taxare inversă, pe contrapartea
colectată, cu Sens=Achiziție. Baza rămâne unică; D300 nu dublează prin oglindă.

- Contrapartidă 401 prin comisionar: taxa 21 deschide partida proprie
  `(document,401,partener)` cu rest −21. Baza 100 nu trece prin
  `Partide.Numeste`/`CuPartida`; ambele capete fiscale rămân fără unitate.
- 446 cu RolTert: aceeași regulă, numai taxa deschide partida. În seed-ul
  privat actual 446 nu are RolTert; nu se schimbă seed-ul contului pentru
  a forța o partidă în scenă. `PoateFiStins=false` rămâne T-r5 / 86-r11.
- Taxare inversă IMPTI21: contabil 4426 = 4427: 21, baza fiscală 100 în aceeași
  pereche ca mai sus. Conform amendamentului 103, creditul 4427 poartă
  Autocolectare cu Sens=Achiziție; B-r4 este acoperită în felia fiscală TR-D8.
- Bază 0,01 cu taxa calculată 0: numai cele două postări fiscale de 0,01;
  există tranzacție operată, nu există postări de taxă sau partidă.
- Taxa culeasă 21,03 la baza 100 se păstrează după politica existentă de
  toleranță; baza rămâne 100. Taxa nu se copiază și în `Carte=Fiscal`.

Pe o linie cu fapt fiscal, per `(tranzacție,linie,tipTVA,sens)` există exact
un fapt Bază și cel mult un fapt Taxă. Cheia nu include `Carte`: o copie
în altă carte este tot o dublare. La DVI baza este în cartea fiscală,
taxa în cea contabilă.
La celelalte tipuri, faptul de bază deja atașat netului rămâne acolo:
nu se adaugă o copie fiscală. Aceasta completează B-D8 pct. 5 pentru
documentul fără net, fără o normalizare care ascunde valori lipsă.

## DVI-B4 — Contractul citirii

| Citire | Selecție și măsură |
|---|---|
| Balanță/rulaje/fișă contabilă | `Carte=Contabil`; regulile existente pentru felul tranzacției rămân |
| Rest/FIFO/stingeri | `Carte=Contabil`, unitate Partidă; baza nu are unitate |
| Jurnal/decont TVA | toate cărțile, numai `CodTva != null`; grupare pe sens, tip, rol, perioadă și dimensiunile cerute; sumă din `Valoare` semnată |
| Conservare | separat pe fiecare `Carte`, pentru toate postările inclusiv echilibrarea |
| Audit | toate cărțile; păstrează cauza, latura, rolul, perioada și măsurile |

În jurnalul fiscal nu se aplică semnul debit−credit: valoarea este deja
semnată, iar sensul Achiziție/Livrare și rolul Bază/Taxă sunt coordonate.
Altfel vânzările creditate și retururile ar avea semne greșite.

Schimbare concretă identificată: `ReconciliereCub.Contabile` agregă astăzi
toate cărțile; va cere `Carte=Contabil`. `Fiscale` și `Storno` trebuie să
citească ambele cărți, numai faptele cu cod, iar `Balanta` rămâne per carte.
Se verifică și cititorii de partide și helper-ele scenelor; un filtru pe
`Spatiu.Contabil` singur nu selectează cartea contabilă.

Neutralitatea perechii fiscale pe cont nu înlocuiește filtrul pe carte.
La SC-DVI-01, fără filtru pe partener, soldul 4426 este 21 chiar dacă
se amestecă cele două cărți, dar rulajele sunt greșit 121 debit / 100
credit. Cu filtrul pe partenerul fiscal, creditul de echilibrare dispare
și inclusiv soldul nefiltrat pe carte devine greșit 121. SC-DVI-16 cere
atât rulajele corecte, cât și soldul 21 pe cont × partener.

`Materializare.Imperecheaza` citește azi toate cărțile. Pentru această
formă DVI, `Transferuri.Muta` nu este afectat: `Cea` selectează numai
unități Partidă, identitățile se caută în unitățile nenule, iar sumele
pentru plafon/disponibil sunt restrânse la ID-ul partidei. Baza nu are
unitate pe niciun capăt. Se păstrează mecanismul actual și se fixează
această proprietate prin SC-DVI-20; nu se invocă numai `PoateFiStins=false`
drept justificare. Pentru viitoare unități în cartea fiscală această
garanție nu ajunge: izolarea cărții trebuie revizuită înaintea introducerii lor.

La TR-D8, balanța, fișa și rulajele pe cont vor folosi o singură intrare
de citire contabilă, care impune `Carte=Contabil`. T-r11 urmărește această
convergență și probele ei; un filtru pus doar în diagnosticul de azi nu
închide restanța pentru cititorii de producție.

Portarea rapoartelor de producție rămâne TR-D8. Aici se probează proiecția
numerică direct din postări, fără citirea cifrelor din document sau
preluarea așteptărilor din registre. Comparația suplimentară cu registrele
nu înlocuiește aceste așteptări.

## DVI-B5 — Ciclu de viață și date

Anularea în perioada deschisă elimină atomic ambele mișcări; reoperarea
reproduce aceleași fapte o singură dată. Stornoul inversează toate cele
patru postări cu mecanismul comun, fără schimbarea laturii/cărții și fără
atingerea originalelor. Taxa 0 inversează cele două postări de bază.

Storno comercial în februarie al DVI din ianuarie: −100 / −21 fiscal în
februarie, 100 / 21 rămân în ianuarie. Corecție pentru eroare materială în
februarie la baza 80: invers −100 / −21 și nou 80 / 16,80 declarate în ianuarie;
net fiscal ianuarie 80 / 16,80, postări contabile ale corecției în februarie.
Ambele roluri folosesc aceeași politică a perioadei declarării (088).

Document întârziat, fără corecție: dacă ianuarie este încă deschis,
baza și taxa se declară în ianuarie; dacă ianuarie este închis, politica
deductibilă a DVI le declară în februarie. Toate postările sunt
datate la înregistrarea din februarie. Catalogul separă aceste fixture-uri
(SC-DVI-07 / SC-DVI-18); regula nu se deduce doar din `Data != DataInregistrare`.

Gardul partidelor cu dependenți rămâne comun: dacă taxa a deschis o
partidă consumată prin NTC, anularea/stornarea refuză atomic înainte de
inversare. Baza fiscală nu creează sau eliberează dependenți de partidă.
Legăturile `DviFactura` sunt evidență; nu nominalizează partida FCT și
nu schimbă baza declarată. Protecția FCT legate, 86-r2, rămâne distinctă.

## DVI-B6 — Date de intrare și suprafața implementării

`DeclarantDvi` rămâne pur și lucrează numai cu operandul închis. Faptul
`TipTvaFapt` primește `DeImport`, proiectat în interogarea pe set deja
existentă din `Fapte.TipuriTva`; azi câmpul lipsește din fapt, deși există
pe entitate. Nu se verifică importul după prefixul codului. Nu se adaugă
interogări per linie sau coloane persistente în cub.

Declarantul validează liniile, baza pozitivă, tipul de import cu cotă
pozitivă, politica Deductibil și conturile rezolvabile. Folosește calculul
de taxă și `Fiscal.Impozitul` comune. Noul helper al bazei, dacă se extrage,
stă în `Declaratii/Fiscal.cs` și este apelat explicit de DVI, fără
`switch` după tip în motor. Nu se creează RegulaContare de net pentru DVI.
MRN-ul și starea facturilor legate păstrează validarea actuală a entității;
acoperirea celor două uși se notează distinct în catalog.

Suprafață prevăzută: declarantul și override-ul DVI, helper fiscal,
`TipTvaFapt`/maparea sa, coduri stabile de refuz, seed privat,
`ScenariiDvi` și înregistrarea scenei, helper de comparație care include
`Carte`, curățenia `DviFactura`, filtrele cititorilor/gate-ului atinse.
Intră și `Normalizari.Fiscal` / contextul citit pe set pentru adaptorul
`CubDinRegistre`: baza DVI din `RegistruTva` se transformă în întreaga
pereche din DVI-B2, inclusiv creditul de echilibrare, nu se atașează
postării contabile de taxă. Delimitarea DVI este explicită în context;
nu se aplică această regulă oricărei baze fără țintă. Lipsa țintei pentru
alte tipuri rămâne diagnostic. Contul deductibil și gestiunea trebuie
rezolvate și când taxa este zero, fără deducerea lor dintr-o postare de
taxă absentă. Adaptorul nu citește cubul rezultat și nu cheamă declarantul
ca să-și construiască așteptarea. SC-DVI-19 verifică forma normalizată
contra acelorași constante independente, cu contor pentru normalizarea
numită și fără diagnostic de bază rămasă fără țintă pe DVI.
`Conservare`, `Miscare`, `Postare`, `Storno` au deja forma necesară și nu
cer modificarea modelului pur sau a schemei EF pentru această propunere.

## DVI-B7 — Probe obligatorii înainte de activare

Catalogul numeric este `scenarii/DVI.md`, SC-DVI-01…20. În plus față de
ciclul complet: bazele 100 și 100,01 cu aceeași taxă 21 rămân distincte;
taxa culeasă 21,03 nu schimbă baza 100; baza 0,01 cu taxă 0 supraviețuiește;
partida 401 rămâne −21; multicota păstrează 100 / 21 și 50 / 5,50.

Se compară exact toate coordonatele postărilor, inclusiv `Carte` și
creditul de echilibrare fără cod. Se asertează rulajele contabile 21 / 21,
nu doar soldul net care ar ascunde 100 debit + 100 credit în plus. Se
verifică invarianta per carte, unicitatea faptelor fiscale, lipsa
unităților pe perechea bazei și proiecția fiscală înainte/după storno.

SC-DVI-16 rulează și `ReconciliereCub.Ruleaza` pe DVI operată: litera (a)
trebuie să dea zero diferențe cu `Carte=Contabil`. Proba asertează separat
21 debit / 21 credit contabil și soldul 21 pe 4426 × partener, din
constante. Într-o proiecție martor fără filtrul de carte se constată
121 debit / 121 credit total și soldul 121 pe 4426 × partener. Astfel
testul detectează omiterea filtrului chiar dacă soldul total pe cont
rămâne accidental corect. Nu se schimbă codul de producție sau datele
persistate pentru acest martor.

**SC-X-14 — unicitatea fiscală pe catalogul implementat.** O probă
comună se aplică postărilor fiecărei scene înainte de curățenia fixture-ului,
inclusiv la rularea filtrată pe tip. Se verifică multiplicitatea pe cheia
din DVI-B3, independent de carte, atât pe `Operare`, cât și pe `Storno`;
corecția are propria tranzacție. Matricea așteptată a scenei declară și
liniile fără fapt fiscal: absența tuturor faptelor nu poate trece vacuu.

| Caz din catalog | Fapte pe linie (Bază / Taxă) | Măsuri așteptate |
|---|---|---|
| FCT și FCL normale, bază 100 la 21% | 1 / 1 | 100 / 21 |
| FCT capitalizat, brut 121 | 1 / 1 | 100 / 21, ambele pe cost |
| FCT cu taxare inversă, bază 100 | 1 / 1 achiziție + 1 Autocolectare (103) | 100 / 21 / 21 |
| FCL cu taxare inversă sau scutită, bază 100 | 1 / 0 | 100 / 0 |
| RDC venit 100 la 21%; linia separată de cost | 1 / 1 pe venit; 0 / 0 pe cost | −100 / −21 fiscal |
| RLF bază 20 la 21% | 1 / 1 | −20 / −4,20 |
| DVI bază 100 la 21%; apoi cazul cu bază 0,01 | 1 / 1; respectiv 1 / 0 | 100 / 21; respectiv 0,01 / 0 |
| Linii fără TVA din toate tipurile deja pe cub, inclusiv BCS/BTR/DSC/PLT/INC/NTC/ITV | 0 / 0 | nicio măsură fiscală |

Pentru storno se păstrează multiplicitatea și se inversează măsurile.
Helper-ul verifică cardinalitățile și sumele așteptate ale fiecărei linii,
nu numărul apelurilor `CuFapt`. O copie în memorie cu Baza duplicată în
cealaltă carte trebuie respinsă; la fel duplicarea Taxei sau eliminarea
unei Baze așteptate. Se raportează acoperirea pe tip/regim/profil. Pe
bugetar, lipsa politicilor fiscale este așteptare explicită. Tipurile încă
nemigrate intră în această probă odată cu scenele lor; nu sunt declarate
acoperite acum. Limitele existente, inclusiv T-r8, rămân declarate.

Gate: `verifica.ps1 -Suita Scenarii -Tip DVI -Profil Ambele -Sufix .CodexBCS`,
apoi `-Suita Integral -Profil Ambele -Sufix .CodexBCS` și suita Nucleu;
secvențial, cu manifest, zero FAIL și curățenie pe marcaj. Pe bugetar se
probează explicit profilul inert. Zero DVI pe Flax rămâne consemnat;
Import1C nu se rulează ca probă supremă (091).

## Limite și punct de review

Nu intră DVI pe loturi/SC-X-13 sau reevaluarea,
stingerea directă DVI sau portarea generală a rapoartelor. Nu se schimbă
declaranții deja validați pentru a muta toate bazele în cartea fiscală.

**Punctul concret de review este DVI-B2:** contul TVA deductibil drept
ancoră pentru debitul și creditul fiscal egale, cu rol Bază numai pe
debit. Restul disciplinei rezultă din separarea pe cărți aleasă de owner.
Implementarea este `DeclarantDvi`, cu `DeImport` citit în operandul închis.
Cele 20 de scenarii sunt executabile în `ScenariiDvi`; verificarea fiscală
comună este `UnicitateFiscala`, integrată în scenele catalogului. Rezultatele
și manifestele de validare sunt în `scenarii/DVI.md`.
