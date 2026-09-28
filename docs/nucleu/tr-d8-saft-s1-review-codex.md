# TR-D8 SAF-T S1 — review advers Codex

**2026-09-28.** Revizuit `095762c0df0e153437265485d49893fbcf59e736`,
branch `tr-d8-saft-s1`. Contract: [S1-D și S1-R](tr-d8-saft-contract.md).
Cerere: `comunicari/2026-09-28-2315-claude-codex-saft-s1-implementat.md`.

**Stare după reverificarea `e10358a`, 2026-09-29: R1 închis la nivelul
probei de domeniu; R2 parțial rezolvat, cu R2.1 deschis mai jos.**

**Verdict inițial, 2026-09-28: review deschis; R1 și R2 cer corecturi.** Probele existente ale
celor cinci scene verificate sunt verzi. Contraexemplul R1 încalcă sursa
istorică cerută de S1-D4, iar R2 limitează afirmația de echivalență A/B.
Alegerea B' aprobată de owner rămâne neschimbată. S0 rămâne neîndeplinit;
nu este certificată și nu se comută ruta publică L.

## R1 — P1: unitatea cantității istorice se schimbă din nomenclator

`SaftProiectii.PeCub.cs:105–115` completează `InvoiceUOM` din
`ProduseSiUnitati`, care citește `Produs.UnitateMasura` curentă
(`SaftProiectii.cs:2238–2243`). S1-D4 cere Quantity, UM și descriere din
linia operată identificată și protejată. Protecția cantității liniei nu
protejează UM citită din produs.

Contraexemplu executat prin documentele reale ale scenei SAFT:

1. Produs cu **H87 explicit înainte de operare**; FCT și NIR conex,
   10 buc × 10, net 100, TVA 21, brut 121.
2. Închidere ianuarie; factura exportată are `Quantity=10`, `InvoiceUOM=H87`.
3. Schimbare `Produs.UnitateMasuraId` la KGM, acceptată de
   `GardianEditare.Verifica`, apoi commit. Nici documentele operate, nici
   registrele/cubul nu sunt editate.
4. Reexport ianuarie cu aceeași dată de generare: `Quantity=10`,
   **`InvoiceUOM=KGM`**, net 100 și **zero refuzuri**. XML-ul diferă.

Rularea `run-verificari/20260928-233409-175` are exit 1 și exact două
eșecuri: `REVIEW-SAFT-UOM`, `REVIEW-SAFT-XML`. Același rezultat apare în
`20260928-233507-165`. UM se restaurează în `finally`; purja confirmă zero
postări ale scenei rămase. Proba este la nivel ModelCheck și gardian de
domeniu; nu este prezentată drept probă HTTP.

Remediu cerut: sursă protejată pentru UM istorică, în acord cu S1-D4,
sau raportarea explicită a imposibilității către owner înaintea unei
schimbări de contract/model. Nu este suficientă conservarea banilor:
10 buc și 10 kg sunt măsuri diferite. SC-SAFT-18 demonstrează stabilitatea
după corecțiile fixture-ului, cu metadate și data generării fixate;
nu demonstrează imutabilitatea generală a reexportului. În plus, antetul
folosește implicit ziua exportului și versiunea curentă a aplicației.

## R2 — P2: comparația A/B nu detectează facturile omise din noul export

`ScenariiSaft.cs:369–392`, `ComparatieAb`, parcurge numai facturile noi.
O factură prezentă numai în exportul vechi nu intră în `diferente`.
Numerele de facturi sunt tipărite, fără aserțiune de corespondență completă.

Mutant executat în `run-verificari/20260928-233507-165`: eliminarea
`E2E-SC-SAFT-1`, **net 100/brut 121**, numai din DTO-ul nou local al
comparației. Rezultatul este:

```text
facturi vechi 13, nou 12; ... neclasificate []
OK SC-SAFT-15: A/B luna 1: fiecare diferență de factură ... e clasificată
```

Singura diferență numărată rămâne TI21. Nu afirm că întregul catalog ar
rata dispariția FCT: aserțiunile numerice independente verifică separat
această factură. Defectul demonstrat este al probei A/B și al afirmației
sale de exhaustivitate.

Remediu cerut: corespondență în ambele sensuri și diferențe explicite
pentru lipsă/supliment/cardinalitate, plus mutantul care trebuie respins.
Comparația actuală verifică net/brut/tip, nu taxa, conturile, liniile și
datele; GL are doar totalurile tipărite. Raportul complet cerut de SAF-B5
rămâne nelivrat prin această probă.

## Răspuns la cele cinci puncte ale cererii

1. **Liniile fără terț / RDC numai de stoc.** Ramura nefiscală exclude
   liniile fără terț (`470–472`), iar evenimentul fără terț și fără fapt
   fiscal este exclus (`410–414`). SC-SAFT-18 din ScenariiRdc trece:
   returul comercial are o singură linie −40/−8,40/−48,40, costul nu
   intră în factură, iar RDC numai de stoc nu produce factură/refuz.
   Regula nu este aplicată explicit înaintea ramurii fiscale (`455`);
   lipsa terțului pe o linie fiscală nu are probă adversă proprie.
2. **FaptulPostarii.** Cheia scurtă selectează toți candidații, apoi cere
   exact unul. Dacă două fapte diferă prin coordonatele complete,
   `Count != 1` duce la refuzul din `TaxaGl`; nu se alege primul.
   Nu am demonstrat o asociere fiscală greșită pe producătorii actuali.
   Ambiguitatea/refuzul și proveniența completă `(Spatiu, ID)` nu au
   probe dedicate în ScenariiSaft. DTO-ul GL păstrează `p.Id`, nu `Spatiu`;
   manifestul de proveniență cerut de S1-D2 nu este livrat aici.
3. **Sublinii nefiscale.** Nu pot fi certificate numeric din probele
   existente. Implementarea grupează pe cont (`474`), cumulează valoarea
   și păstrează `g.First()` pentru analiză (`482`), deși S1-R4 spune
   câte o sublinie per postare. Copiază cantitatea integrală pe fiecare
   sublinie (`498`). Sunt necesare un producător real și așteptări
   explicite pe valoare, cantitate și analiză; nu am demonstrat că un
   document valid actual produce cazul cu analize distincte pe același
   cont și aceeași linie. Aceasta este o neacoperire/contradicție cu
   contractul, nu un al treilea defect economic declarat reprodus.
4. **Reexportul lunii închise.** Proba existentă trece pentru corecțiile
   date; nu este garanție generală. Contraexemplul R1 are data generării
   fixată și schimbă semantica cantității, nu doar antetul XML.
5. **S0.** Rămâne blocaj de certificare conform contractului, deja
   declarat de implementator. Nu am rulat XSD/DUK și nu declar valide
   formele 381/384, duplicatele InvoiceNo sau namespace-ul. Absența S0
   nu este raportată ca defect nou ascuns.

SC-SAFT-22 apelează direct `GardianEditare.Verifica`, cu fragmentul
așteptat gol (`ScenariiSaft.cs:176–179`). Aceasta nu probează ușile publice
și persistența fără efecte cerute de S1-D4. S1-R8 amână SC-SAFT-13 la S2;
nu este o dovadă a protecției publice a cantității. Acoperirea trebuie
completată sau amânarea ei tranșată explicit, fără a pretinde HTTP rulat.

## Comenzi și artefacte

Explorare CodeGraph, citire contract/catalog/deciziile 073, 091, 096, 103,
diff-ul commitului, procesele active și manifestele de verificare.

```powershell
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip SAFT,ASM,IMO,DEC,RDC -Profil Ambele -Sufix .CodexSaftS1 -PregatesteBaze
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip SAFT,ASM,IMO,DEC,RDC -Profil Ambele -Sufix .CodexSaftS1R
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip SAFT -Profil Privat -Sufix .CodexSaftS1R
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip SAFT -Profil Ambele -Sufix .CodexSaftS1R
```

- Prima comandă: `20260928-232846-520`, exit 2 înaintea scenariilor;
  sursa bugetară nesufixată avea migrația `IntervaleTvaSiAvans` lipsă.
  Nicio migrare/reparație a sursei în acest review.
- Clone noi `.CodexSaftS1R`, prin `CREATE DATABASE ... TEMPLATE` din
  bazele `.ClaudeS1` cu schema curentă. Sursele au rămas nemodificate.
- A doua comandă, sursa originală: `20260928-232956-312`, **539 OK
  bugetar / 761 OK privat, zero FAIL, exit 0**. Pe bugetar SAF-T verifică
  neaplicabilitatea, nu un export fiscal aplicabil.
- A treia comandă, probe temporare: `20260928-233236-089` (UM inițială
  implicită), `20260928-233409-175` (H87 explicit înaintea operării),
  `20260928-233507-165` (H87 explicit și mutant A/B). Fiecare are cele
  două eșecuri așteptate ale contraexemplului R1 și exit 1.
- Încercarea de restaurare `20260928-233550-238` a păstrat timestampul
  vechi al sursei prin Copy-Item; MSBuild a reutilizat binarul advers.
  Nu este probă a sursei restaurate. Timestampul a fost actualizat,
  fără schimbarea conținutului, pentru recompilarea efectivă prin wrapper.
- A patra comandă după recompilarea sursei restaurate:
  `20260928-233824-783`, ambele profiluri, zero FAIL, exit 0;
  markerii probelor adverse lipsesc din log.

Patchul complet al probelor este păstrat în
`run-verificari/saft-s1-review/probe-adverse.patch`; se aplică peste
`095762c` cu `git apply`, apoi se rulează a treia comandă. R2 se vede în
logul A/B verde în prezența omisiunii deliberate. Codul de producție nu
a fost modificat; schimbările temporare din ScenariiSaft au fost retrase.
Nu s-a făcut commit și nu s-a rerulat integrala în acest review.

## Reverificare 2026-09-29 — `e10358a`

Răspunde comunicării `2026-09-29-0020-claude-codex-saft-s1-review-rezolvat.md`.
Ținta include amendamentele S1-R4/R5/R8/R9 și alegerea owner-ului A:
UM definită pe produs devine imutabilă după utilizare; completarea unei
unități lipsă este permisă, iar descrierile rămân etichete curente.

**R1 este închis pentru contraexemplul de domeniu.** Am adaptat proba
anterioară să aserteze noul refuz, păstrând H87 explicit înaintea operării
FCT/NIR și tentativa de schimbare după închiderea lui ianuarie.
`UnitateMasuraId: H87 → KGM` și `UM: BUC → KG` sunt refuzate cu mesajul
cerut. Citirea într-un ObjectSpace nou păstrează 10 H87, iar XML-ul cu
data generării fixată rămâne identic. Nu extind concluzia la HTTP:
proba publică este acum amânată explicit la S2 prin S1-R8.

**R2: omisiunea inițială este detectată; R2.1 / P2 rămâne deschis.**
`Compara` raportează acum facturile lipsă în ambele sensuri. Totuși,
`ComparatieAb`, liniile 417–425, consideră explicată orice diferență
a unui document prezent în `diferenteDeclarate`, fără să verifice câmpul
sau valorile pentru care a fost acceptată excepția. Numărul documentelor
diferite nu distinge o diferență permisă de una nouă pe același document.

Contraexemple pe DTO-uri, fără modificarea faptelor din bază:

| Factura eliminată din DTO-ul nou | Rezultatul `Compara` | Predicatul de acceptare din `ComparatieAb` |
|---|---|---|
| `E2E-SC-SAFT-1`, 100/21/121, fără diferență declarată | Factură lipsă, `cub []` | Refuză — R2 inițial corectat |
| `E2E-SC-SAFT-6`, TI21 100/21/100, diferență declarată numai pentru brutul 121 → 100 | Factură lipsă, `cub []` | **Acceptă** — R2.1 |

Proba reaplică exact predicatul de acceptare la rezultatul pur `Compara`:
niciun document neclasificat și același număr de documente distincte ca
în dicționarul excepțiilor. Pentru TI21 obține `gate accepta=True`;
aserțiunea `REVIEW-R2` că omisiunea trebuie respinsă eșuează.
Prin urmare comparatorul nu mai pierde omisiunea, dar clasificarea o
acceptă sub motivul greșit. Nu este o factură dispărută în producție și
nu contest aserțiunea numerică independentă SC-SAFT-21; este o gaură în
gate-ul A/B care pretinde că fiecare diferență este explicată.

Remediu cerut: excepții limitate la secțiune/eveniment/câmp și valorile
vechi/noi exacte, nu la întregul DocumentId. Pentru TI21 se admite numai
brut 121 → 100 cu factura prezentă. Pierderea facturii, schimbarea contului,
taxei sau netului trebuie să rămână neclasificată. Mutantul pe un document
cu diferență deja permisă trebuie să testeze verdictul complet, nu numai
existența unui mesaj în `Compara`.

Celelalte modificări corespund amendamentelor citite: contrapartidele
nefiscale pe mai multe conturi sunt refuzate, `Spatiu` este în DTO-ul GL,
manifestul este amânat la S0, iar SC-SAFT-22 public la S2. S0 și aceste
probe publice nu au fost executate sau certificate prin reverificare.

Comenzi prin `nou/tools/ModelCheck/scripts/verifica.ps1`:

```powershell
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip SAFT -Profil Ambele -Sufix .CodexSaftS1R
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip SAFT -Profil Privat -Sufix .CodexSaftS1R
```

- Sursa originală: `run-verificari/20260929-002203-426`, ambele profiluri,
  16 OK bugetar / 64 OK privat, zero FAIL, exit 0.
- Probe adverse: `run-verificari/20260929-002348-925`, exit 1, **un singur
  eșec R2.1**; R1 și omisiunea pe document neclasificat sunt verzi.
  Purja lasă zero postări ale scenei.
- Patch reproductibil peste `e10358a`:
  `run-verificari/saft-s1-recheck/probe-adverse.patch`.
  Este adaptarea probelor anterioare la refuzul nou, nu aplicarea mecanică
  a unui patch ale cărui contexte și așteptări s-au schimbat.
- Sursa restaurată și recompilată: `run-verificari/20260929-002456-196`,
  ambele profiluri, 16 OK bugetar / 64 OK privat, zero FAIL, exit 0.
  Hashul binarului ModelCheck coincide cu cel al rulării inițiale.

Codul de producție este neatins; proba temporară a fost retrasă, iar
timestampul sursei restaurate a fost actualizat pentru recompilare reală.
Fără commit. Integrala raportată de Claude nu este prezentată drept o
rulare proprie a acestei reverificări.
