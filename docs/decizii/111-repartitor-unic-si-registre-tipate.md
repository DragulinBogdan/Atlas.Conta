# 111 — Un singur repartitor de sold pe postare, partenerul fiscal în blocul fiscal, registre tipate pentru ce nu se balansează

- Data: 2026-10-06
- Stare: **propusă**, neaprobată. Nu schimbă nimic în cod, în contractul
  TR-D9a sau în stare-curenta. Poarta de decizie e contractul TR-D9b, cu
  măsurătorile din §„Înaintea deciziei" ca pas 0 al lui. Aprobă numai
  owner-ul. Numărul 110 e rezervat închiderii TR-D9a.
- Docs: `docs/consultations/2026-10-06-repartitor-pe-postare/` (consultare
  Fable + Codex, două runde, README cu verdictele); `docs/nucleu/tr-d9-taierea-amendament-2.md`
  (D9-A10, forma îngustă aprobată; pasul 6b, mutat de owner după tăiere); 090 (c), B-D8 pct. 4 și 9.

## Direcția propusă

**Postarea poartă o singură coordonată de sold pentru „cine sau unde", cu
felul ei declarat de cont. Ce nu e coordonată de sold stă în altă parte:
partenerul fiscal în blocul fiscal, capătul celălalt al mișcării în cheia de
pereche, iar faptele care nu se balansează în registre tipate, lângă cub.**

(a) **Un singur `Repartitor` pe postare**, în locul coloanelor `Partener` și
`Gestiune`. Contul declară felurile admise ca mulțime (partener, angajat,
gestiune, unitate internă, cont propriu, niciunul), nu un fel unic; 461 și
462 cer mulțimea. Declarantul pune pe fiecare capăt repartitorul al cărui
sold îl mișcă postarea. `UrmarestePartide` și `RolTert` rămân axe separate:
prima e a unității, a doua e clasificarea SAF-T.

(b) **Partenerul fiscal e atribut al faptului fiscal**, persistat pe postarea
de bază și pe cea de taxă, în blocul fiscal, scris de politica de TVA la
declarare. Nu e coordonată de sold și nu se deduce din document, din pereche
sau din capete: la taxare inversă perechea n-are terț, la decont contrapartida
e angajatul, la DVI predătorul e vama.

(c) **Gestiunile virtuale dispar.** Capătul din afara evidenței și
contraponderea transformării primesc un marcaj explicit pe postare, ca nucleul
să deosebească un declarant care a uitat lotul de o contracantitate legitimă.
Furnizor și Client devin terțul real; Consum, Inventar, Transformare devin
feluri de capăt, nu identificatori fără rând.

(d) **Capătul celălalt al mișcării se citește prin cheia de pereche**
(D9-A2), nu prin coloane de flux pe postare. Materializarea capetelor se
admite numai ca proiecție de citire, dacă un raport o cere la volum măsurat.

(e) **Respinse**: ambele coloane completate pe toate postările documentului
cu terț (soldurile pe cheie nu se închid: factura pe gestiune, plata fără);
fluxul pe postare ca formă finală (capetele nu se pot completa onest la
decont, virament, TVA, locul fișei, imputatul de pe linia NIR).

(f) **Criteriul depozitului.** Ce se balansează valoric în contabilitate e
postare în cartea contabilă. Ce trebuie să se balanseze altfel primește o
carte proprie, ca azi cartea fiscală. Ce nu se balansează și are ciclu de
viață (credit bugetar, angajament anual și multianual, rectificativă,
lichidare, ordonanțare) e **registru tipat**, separat de cub, legat prin
document și linie, cu invarianții lui și cu un invariant transversal
(executat pe angajament, citit din cub, ≤ angajamentul din registru). Un
registru ține numai fapte pe care cubul nu le poate exprima; ce se poate
re-agrega din cub e proiecție, nu registru. Fiecare registru e clasă în cod,
cu hook de declarare pe document, ca `IDeclarant`; nu se configurează
generic prin tabele.

(g) **Limitele până la decizie.** În TR-D9a nu se schimbă nicio coordonată a
postării și nu se adaugă niciun registru. Orice constatare nouă pe tema asta
devine o linie în §„Constatări strânse", nu cod și nu amendament.

(h) **Anticipări cu cost mic, permise în TR-D9a** fiindcă nu schimbă
comportament și nu schimbă schema:
- pasul 6b (D9-A10, inițial 2c) se face printr-un singur helper al capătului de terț, cu regula
  „terțul capătului" (la decont și la nota contabilă terțul e al liniei, nu
  latura externă a documentului); textul D9-A10 se precizează la prima
  redeschidere a contractului;
- gardul analizei citește repartitorul capătului dintr-o singură funcție, nu
  printr-un coalesce pe două coloane;
- cele 17 aserții ale clasei „repartitorul de pe latura notei" și orice probă
  nouă se scriu pe cititori (`AtomContabil.RepartitorId`, `Citiri.*`), nu pe
  numele coloanelor postării;
- cheia de pereche (7b) se definește ca identitate a mișcării, cu ordinal
  remapat la stornoul care reunește postări din mai multe tranzacții;
- lista XAF pe `Postare` și DTO-urile noi nu promit coloanele `Partener` și
  `Gestiune` ca nume stabile de contract.
O coloană `PartenerFiscal` adăugată la pasul 7, cât `InitialCreate` se
regenerează, ar fi anticipare de schemă: cere un amendament de o linie și
hotărârea owner-ului.

## Context

Cubul poartă azi `Partener` și `Gestiune` completate selectiv: gestiunea pe
piciorul intern și, virtuală, pe capătul din afara evidenței; partenerul
numai cu partida și pe faptul fiscal. Pe baza de import Flax, 22.389 din
81.836 de postări au ambele coloane: 14.484 cu fapt fiscal peste gestiune
reală, 7.905 cu gestiune virtuală peste partener. Discuția a pornit de la
G1 (gardul analizei pe flag-ul `Repartitor`) și de la întrebarea owner-ului
dacă un `Repartitor` generic, sau fluxul pe postare cu o coloană de grupare,
n-ar fi un model mai general. Owner-ul a adăugat registrele pentru partea
bugetară: codurile și angajamentele vor cere mai mult decât coordonate.

## Consultarea (2026-10-06)

Fable și Codex, runda oarbă și runda informată pe clonă, converg: un singur
repartitor tipat de cont, partenerul fiscal separat, fără gestiuni virtuale,
forma „ambele coloane" respinsă, fluxul respins ca formă finală, forma
recomandată 3 + cheia de pereche. Diferă pe coloanele exacte (Fable:
`FelRepartitor` pe postare; Codex: `RolCantitativ`), pe unirea declarațiilor
contului și pe cost (4–5 zile față de 7–10). Amândoi cer hotărârea formei
înaintea pasului 2c și așază schimbarea la pasul 7 al TR-D9a; argumentul
„ieftin la pasul 7" presupune date vii, pe care PoC-ul nu le are (102 b).
Owner-ul a hotărât să nu decidă acum și să nu lărgească TR-D9a.

## Înaintea deciziei (pas 0 al contractului TR-D9b, numai citire, ≤ 2 zile)

1. Recensământul perechilor de feluri pe cont, pe Flax: niciun cont nu
   poartă două repartitoare reale pe același picior.
2. D394 regenerat cu partenerul mutat în blocul fiscal: amprenta declarației
   identică.
3. Prototip al nucleului cu repartitor și fel, `Conservare` rescrisă, cele
   180 de teste verzi.
4. Fișa contului cu contrapartida prin pereche față de subinterogarea de azi,
   pe scena multiplicată din D9-A1.
5. Lista conturilor bugetare cu flag `Repartitor` și felul propus, confirmată
   de owner.
6. 110-r3, restrânsă de owner (2026-10-08) la cele 11 coordonate de sold
   (`Partener`, `Gestiune`, `Produs`, `Unitate`, `Valuta` și cele șase coduri
   bugetare): îmbinarea pe tuplul de coordonate, cu nul față de identificator
   gol, pe o copie a scenei multiplicate din D9-A1. Cele 7 coloane de
   referință și de bloc fiscal (`DocumentId`, `LinieId`, `SuportId`,
   `InversaDinId`, `TipTvaId`, `DocumentFiscalId`, `Atribuit`) rămân nulabile.
   Se hotărăște odată cu (a) și (c): scrierea felului „niciunul" pe postare și
   FK-ul repartitorului unic pe `Repartitori` sunt aceeași întrebare.

## Constatări strânse

Din consultare, de verificat la poartă:

- 3xx la recepție poartă partenerul fiscal; balanța analitică pe repartitor a
  stocurilor arată furnizorul și după consum.
- Viramentul pune aceeași bancă pe ambele picioare ale fiecărei etape; 581 pe
  bancă nu se închide.
- DSC pune virtuala Client și la primitor intern; LDI pierde comisia; BCS
  pune lotul și locul pe 6xx.
- La decont partenerul fiscal e angajatul; furnizorul bonului nu există în
  model în nicio formă și ar cere un atribut pe linie.
- Invariantul inversei fiscale nu verifică partenerul.
- Custodia (stoc al mai multor proprietari în aceeași gestiune) rămâne
  neacoperită; candidat: proprietarul în identitatea lotului.

Re-amânat aici de owner (2026-10-06), din TR-D9a: criteriul de formă al
partidelor (F27-r16, D9-D10 (b)). Documentul deschizător al partidei se
recunoaște azi la citire, parcurgând postările de partidă; cu originea
scrisă, o linie pe partidă, citirea măsurată scade de la 2,9 s la 0,8 s la
651.644 de partide (`docs/nucleu/tr-d9-pas5c-imbinare-partide.md` §3).

## Ce rămâne în afara acestei decizii

Registrele bugetare (ALOP: credite, angajamente, rectificative) au decizie
proprie după PoC, odată cu analiza owner-ului; aici intră numai criteriul
(f). Codurile bugetare existente (cod funcțional, cod economic, sursă, centru
de cost, proiect) rămân coordonate pe postare.
