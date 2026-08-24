# Decizia 64 — Pasul 5, felia 7 — viramentul intern

- **Data**: 2026-08-15 (primul commit în jurnal)
- **Stare**: activă; (k) gaura „al doilea picior cules manual" închisă de 65b
- **Rezumat durabil**: `CLAUDE.md` §64
- **Docs**: docs/api/p5-felia-vir-contract.md

---

**Pasul 5, felia 7 — viramentul intern (transferul 581) — executată**
(contract + închidere: `docs/api/p5-felia-vir-contract.md`, F7-D1…F7-D11;
flux-ancoră complet în browser pe baza Privat). Închide amânarea 31f
(„transferul între conturi proprii — pereche PLT+INC conexă, 581") și
răspunde întrebării deschise din inventar 09 §4 (`TRANSFER`/`COD_CBT`).
Tranșările:
(a) **NICIUN tip de document nou** — testul invariantului IV („diferă
*schema* sau doar *politica și vizibilitatea*?"): header, linii și ciclu de
viață sunt ale unei plăți obișnuite; diferă doar CONTRAPARTIDA (al doilea
cont propriu) și, în consecință, contul de postare. Perechea stă pe
ACELEAȘI laturi (predator = contul sursă, primitor = contul destinație pe
ambele documente) — direcția o poartă TIPUL, nu laturile, deci nu se
inversează nimic la generare. Pereche, nu document unic, fiindcă cele două
picioare sunt confirmate de documente diferite, la date diferite (foaia de
vărsământ azi, extrasul mâine): 581 e exact contul care ține diferența de
timp, iar importul viitor de extrase aduce fiecare picior separat.
(b) **`NaturaClasa.Virament` = singura schimbare de MODEL** (verificat: nu
există niciun `switch` exhaustiv pe enum în toată soluția ⇒ strict aditiv);
Clasa/Tipul `VIR` cu `ContImplicit` = 581 și cele două `RegulaContare`
(PLT×VIR: 581 = cont propriu predator; INC×VIR: cont propriu primitor =
581) sunt DATE per profil, în ambele. **Fără migrație** — prima felie a
pasului 5 fără schimbare de schemă.
(c) **Cuplajul laturi ↔ natura liniei, validat în AMBELE sensuri** +
`Predator == Primitor` refuzat + oglinda 38c: **linie de natura Virament
fără regulă de contare potrivită = refuz explicit**. Fără ele, un virament
cules cu Tipul obișnuit (sau un profil cu Tipul VIR dar fără rândul de
politică) ar cădea pe regula GENERICĂ a tipului și ar posta „destinație =
sursă" pe FIECARE picior — dublă postare tăcută.
(d) **Generarea perechii = `GenereazaSecundar`, nu `PoliticaConex`**:
contract `CreeazaPereche` (Plata→Incasare, Incasare→Plata) + gardul
`Autogenerat`. `PoliticaConex` ar fi cerut două rânduri simetrice și n-are
niciun gard contra recursiei (operarea copilului ar genera din nou un
copil, la infinit); iar existența celui de-al doilea picior nu e alegere de
profil, deci n-are ce căuta ca rând de politică (invariantul IV.1). Nu se
copiază `Numar` (serie proprie) și `NumarExtras`/`DataExtras` (fiecare
picior are extrasul lui).
(e) **Fix de FOND în motor #1**: pasul 6 din `Opereaza` („plata autogenerată
își creează imperecherea cu sursa") se declanșa pentru ORICE document de
trezorerie autogenerat cu sursă — latura pereche e exact asta, iar
invarianții o permiteau (sensuri opuse, contrapartidă comună), deci s-ar fi
auto-imperecheat cu propria sursă și ar fi BLOCAT anularea/stornarea ambelor
picioare. Gardul e polimorf, fără `is` pe tip în motor:
`CapacitateStingere(os) != null`, iar viramentul întoarce `null` („nu stinge
și nu poate fi stins": contrapartida unei plăți normale nu apare niciodată
pe laturile lui).
(f) **Fix de FOND în motor #2 (dimensiuni)**: `RepartitorImplicitDebit/
Credit` primesc `IObjectSpace` (alinierea cu toate celelalte hook-uri —
erau singura excepție), iar viramentul face override: AMBELE laturi poartă
contul propriu AL PICIORULUI. Default-ul (debit←Predator, credit←Primitor)
pune pe rândul de BANI contrapartida — inofensiv când e un partener, dar
între două conturi proprii pe același sintetic (două bănci 5121) ieșirea lui
A s-ar atribui lui B și invers: soldul per cont propriu ar ieși exact
INVERSAT. Precedentul mecanismului: `Decont` (32c) și `DescarcareGestiune`
(37a). PLT/INC normale rămân neatinse.
(g) **API aproape gol** (dovada că forma e corectă): rutele, WriteDto și
`Aplica` NEATINSE; singura adăugire e affordance-ul `EsteVirament` în
ReadDto — calculat în ACELAȘI query, cu formula domeniului: **AMBELE laturi
conturi proprii**, nu doar contrapartida (un draft cu laturile inversate ar
fi fost etichetat virament pe căile care rulează înaintea validării).
`DocumenteCuRest` neatinsă, cu motivul scris în cod. Client: al treilea fel
de contrapartidă (sondă OData), Tipul `VIR` precompletat, panoul de stingeri
înlocuit de indiciul perechii cu link prin `rutaTip`; `esteVirament` are
două surse, fiecare autoritară pe ce știe (serverul pe salvat, sonda doar pe
culegerea nesalvată) — nu există a treia definiție a viramentului în TS.
(h) **Constatare raportată, NEtranșată**: convenția dimensiunii Repartitor
(„debit←Predator / credit←Primitor") pune pe fiecare rând contrapartida
laturii, nu repartitorul CONTULUI de pe acel rând — deci soldul per cont
propriu se citește azi exclusiv din SIMBOLUL contului, iar două conturi
proprii pe același sintetic nu se separă în balanță. Viramentul își rezolvă
cazul local (f), fiindcă acolo greșeala e activă; alternativa coerentă
(„dimensiunea urmează `SursaCont`-ul laturii") ar schimba conținutul
registrului pentru TOATE tipurile și ar cere re-validarea baseline-ului de
import — decizie proprie, nu felie de API.
(i) **Ștergerea laturii pereche autogenerate rămâne PERMISĂ** — invers
față de conexul NIR (62f), și cu motiv: acolo refuzul era singura apărare
(factura rămânea operată fără marfă și fără datorie, invizibil), aici
pierderea e cel mai vizibil semnal contabil posibil (581 nu se mai închide),
iar calea manuală reproduce EXACT același document. Ce s-a închis e
tăcerea: viramentul operat fără pereche își spune starea în panou.
(j) **Review advers: 3 defecte de FOND**, tranșate diferit fiecare.
(1) **„Viramentul nu poate fi STINS" era AFIRMAT, nu impus** — argumentul
„contrapartida unei plăți normale nu apare pe laturile lui" e adevărat
pentru trezorerie și FALS pentru `NotaContabila`, ale cărei capacități sunt
repartitorii EXPLICIȚI ai liniilor (49a): o notă cu repartitorul = contul
propriu al viramentului trecea toți invarianții și îi bloca definitiv
anularea/stornarea, iar clientul ascunde panoul de stingeri pe virament ⇒
operatorul n-ar fi văzut DE CE. Fix: `Document.PoateFiStins(os)` — cealaltă
jumătate polimorfă a rolului, consultată de `ValideazaCreare` și de poarta
pasului 6 (care întreba doar „copilul poate stinge?", nu și „sursa poate fi
stinsă?"). Corolar: `DocumenteCuRest` CHIAR întorcea picioare de virament
filtrată pe un cont propriu — excluse prin anti-join, afirmația din F7-D7
(„imposibil structural") era greșită și s-a corectat, nu s-a șters.
(2) **Gardul „linia VIR fără regulă" nu oglindea matcher-ul motorului** —
ignora `SemnFiltru`, pe care motorul îl filtrează PRIMUL; cum pe trezorerie
semnul e 0, un `SemnFiltru` ±1 pus din greșeală pe rândul VIR (dată
editabilă în XAF) trecea de gard și motorul cădea pe regula generică = exact
dubla postare tăcută pe care gardul există s-o prevină. Lecție generală: un
gard care oglindește o potrivire din motor trebuie să oglindească TOATE
axele ei, altfel e decor.
(3) **Ștergerea liberă a draftului autogenerat** (clasa 62f) — tranșată
INVERS față de conexul NIR, deliberat: acolo refuzul era singura apărare
(factura rămânea operată fără marfă și fără datorie, invizibil), aici
pierderea e cel mai vizibil semnal contabil posibil (581 nu se închide), iar
un draft de neșters ar deveni el însuși capcană în fluxul (k) — operatorul
care a cules deja piciorul celălalt manual n-ar mai avea cum să curețe.
Ce s-a închis e TĂCEREA: viramentul operat fără pereche își spune starea în
panou, cu calea de recuperare (anulare + re-operare o regenerează).
Minorele fixate în aceeași trecere: `ValoareTva` clonat pe latura pereche
(intră în `Total`, e editabil în XAF); `EsteVirament` în `Lista` (altfel
picioarele de virament erau indistinguibile în grile); predicatul clientului
adus pe AMBELE laturi (o singură definiție, a serverului); `existaInSet`
propagă eroarea (`undefined` = „încă nu știu" ≠ „nu").
(k) **Gaură DESCHISĂ, documentată (review D2)**: nu există noțiunea „acest
virament are deja o latură pereche". Dacă operatorul ignoră draftul
autogenerat și culege a doua latură MANUAL, ea generează la rândul ei un al
treilea document, iar operarea ambelor picioare de intrare dublează postarea
— tăcut, fiindcă două viramente identice între aceleași conturi sunt
perfect legitime (nu există criteriu de conținut care să le distingă).
Fixul are nume: **legătura explicită de pereche** — fie un flag CULES
`GenereazaLaturaPereche` pe header (precedentul `GenereazaPlata` al FCT,
31e), fie alegerea perechii la culegere; ambele sunt aditive, dar cer
migrație, deci ies din felia asta (care n-a avut niciuna).
(k) Rămase, ne-blocante: comisionul bancar (latura pereche cu sumă mai mică
e permisă — reziduul rămâne vizibil pe 581 și se închide cu o notă
`627 = 581`; mecanism dedicat = aditiv, la cerință reală); importul
extraselor și ordinele de plată (09 §4) rămân amânate; viramentul în valută.
