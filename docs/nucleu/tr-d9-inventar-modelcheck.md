# TR-D9a, pasul 1 — inventarul referințelor ModelCheck la registre

- **Felia**: TR-D9a (`docs/nucleu/tr-d9-taierea-contract.md`, D9-D7 c; pasul 1 din D9-D15).
- **Baza**: `tr-d9-taierea` la `88b45a7`; `nou/tools/ModelCheck/`.
- **Datele**: `run-nucleu/tr-d9a/pas1/` — `unitati.tsv`, `asertii.tsv`, `referinte.tsv`, `ajutoare.tsv`,
  `referinte-regulastoc.tsv` (D9-D8, în afara listei canonice); verificarea `verifica.py` (acolo sunt și
  idiomurile numărate). Documentul de față le rezumă; TSV-urile sunt sursa rând cu rând.
- **Completat** (2026-10-05) cu trecerea pe flux de date (secțiunile 1 și 12) și cu aserțiile pe `RegulaStoc`
  (secțiunea 11).

## 1. Metoda și cifrele de control

**Unitatea de inventar e aserția.** O aserție depinde de registre dacă condiția ei citește, direct sau prin flux
de date, un rând de registru: o interogare pe `RegistruContabil` / `RegistruStoc` / `RegistruTva` /
`RegistruImobilizari`, o variabilă umplută dintr-o asemenea interogare, un ajutor local care le citește, un
serviciu vechi (`StocService`), coloanele `Document.TotalStingere` și `TipDocument.PosteazaInCub` sau oracolul
registre → cub. Verdictele sunt cele din spec-ul pasului: `forma`, `regula`, `regula-catalog`, `oracol`,
`comportament`, `neclar`; restul aserțiilor sunt neatinse.

**Cum s-a citit.** Fiecare unitate din `Program.cs` cu cel puțin o referință a fost parcursă pe o vedere a
întregii unități: toate aserțiile cu condiția lor, toate liniile atinse de registre (referințe directe, ajutoare,
variabile purtătoare propagate la punct fix), comenzile motorului și bannerele. Comentariile narative și
atribuirile de proprietăți din montajul scenei au fost sărite; în unitățile cu puține referințe, aserțiile care
nu ating nici registre, nici valori, solduri sau totaluri au fost citite numai ca nume.

**Completarea prin trecerea pe flux de date.** Citirea de mai sus a ratat cel puțin o aserție legată de registre
printr-un lanț lung (ajutor → dicționar → aserție: `Program.cs:23483`). Lista aserțiilor a fost de aceea
completată cu o trecere mecanică peste toate fișierele inventariate. Sursele sunt identificatorii din
`referinte.sh`, în cod și în textul SQL al instrucțiunii. De la o sursă, trecerea urmărește valoarea, în
interiorul fișierului, prin:

- declarații și atribuiri, cu declaratori multipli, tupluri și deconstrucții;
- elemente de dicționar și membri (`c["cheie"] = …` face purtător tot dicționarul), `.Add` / `.AddRange` și rudele lor;
- `foreach`, `using (var … = …)`, `out var` și `is T x`;
- valoarea întoarsă de o funcție locală sau de un lambda atribuit (`return`, `yield return`, corp-expresie);
- parametrii funcțiilor locale chemate cu un argument purtător (fără context: un singur apel purtător face
  parametrul purtător pentru toate apelurile) și variabilele exterioare scrise dintr-o funcție locală;
- dependența de control: o atribuire, un `.Add` sau un `return` sub un `if` / `else` / `foreach` / `while` /
  `switch` cu condiție purtătoare.

O atribuire simplă cu membrul drept curat oprește urma. O aserție e purtătoare dacă un argument al ei, altul
decât numele, citește o sursă sau o valoare purtătoare.

Măsura trecerii față de prima livrare: din cele 431 de aserții cu verdict din `Program.cs`, trecerea le regăsește
singură pe 403. Celelalte 28 nu sunt flux de date prin construcție: 26 `comportament` (valoarea liniei scrisă de
planul vechi, textul unui refuz), `Program.cs:26537` (prin `ImperecheriProiectii.VerificaAcoperire`) și
`Program.cs:27014` (tipul numit numai în text). În sens invers, trecerea a dat 15 candidați fără verdict pe
sursele canonice (7 în `Program.cs`, 8 în celelalte fișiere) și 22 pe sursele lărgite (`CheieStoc`, `MiscareStoc`, `SoldStoc`, `Invarianti.Verifica`,
`VerificaAcoperire`, `AreTranzactii`, `VerificaGoliri`, codurile regimului): **o aserție ratată**, 21 de candidați
respinși cu motiv (secțiunea 12). Trei variante de control — fără oprirea urmei, cu vizibilitatea lărgită la
toată unitatea, amândouă — n-au adus niciun candidat real în plus (28 de coliziuni de nume, verificate).

**Ce NU garantează trecerea.**

1. Nu trece granița de fișier: un ajutor din alt fișier, chemat cu punct (`ProbeCub.X(…)`) sau moștenit din baza
   scenelor, nu duce urma mai departe. Ajutoarele de acest fel care citesc registre sunt în `ajutoare.tsv` și au
   fost urmărite la citire, nu de trecere.
2. Nu vede citirile de registru făcute ÎNĂUNTRUL produsului: o aserție pe rezultatul unui serviciu sau al unei
   proiecții care încă citește registre e văzută numai dacă numele serviciului e în lista surselor. Pentru
   consumatorii din produs autoritatea e inventarul părții de motor (`tr-d9-inventar.md`, §8–9), nu acest document.
3. Nu urmărește TEXTUL unui refuz: o aserție care potrivește mesajul unui gard al planului vechi, obținut prin
   `MotorOperare.Opereaza`, n-are legătură sintactică cu sursa. De mână au fost căutate numai textul gardului de
   sold („Sold negativ”: `Program.cs:23486` și `:24015`, amândouă cu verdict; `ScenariiAsm.cs:430` și
   `ScenariiNir.cs:90` potrivesc deja textul gardului de pe cub) și textele din secțiunea 10.
4. E o analiză pe text, nu pe arborele compilatorului: nu rezolvă supraîncărcări, proprietăți calculate, câmpuri
   de clasă scrise într-o metodă și citite în alta, valori trecute prin `ref`, sau o variabilă purtătoare citită
   într-o buclă ÎNAINTEA atribuirii ei din aceeași iterație.
5. Aserțiile `comportament` nu sunt flux de date și rămân cele găsite la citire; la fel aserțiile prin
   `Invarianti.Verifica` / `VerificaAcoperire`, urmărite de mână (secțiunea 7).
6. Idiomul rămâne cel numărat: o verificare care nu trece prin `Check` / `Verifica` / `check` (un `throw` în
   scenă, excepția așteptată a unui martor) nu e aserție în acest inventar.

**Idiomul numărat.** `Program.cs`: liniile cu `Check(` sau `CheckRefuza(`. Scenele de catalog (`Scenarii*.cs`,
`ScenaDocumente.cs`): liniile cu `Verifica(` / `Refuza(` (neprecedate de punct) și apelurile al căror prim
argument e un identificator literal de catalog (`"SC-…"`, `"NUC-…"`, `"STR-…"`, `"X-…"`), fiindcă scenele
asertează mai ales prin ajutoarele bazei (`Postari`, `Sold`, `FaraEfecte`, `RefuzDeclaratie`).
`AcoperireInvarianti.cs`: `verifica(`. `ProbeAsmOperand.cs`, `Nucleu/ProbeCub.cs`, `Nucleu/ProbeNucleu.cs`: `check(`.
`PerfCub.Evaluare.cs`: `Check(`, `check(`, `Verifica(`. `Purja.cs` n-are aserții.

| Ce | Valoare |
|---|---|
| linii `Program.cs` / unități | 33.241 / 108 |
| aserții `Program.cs` (`Check(` 1.755 + `CheckRefuza(` 270) | 2025 |
| unități `Program.cs` cu cel puțin o aserție atinsă | 63 din 108 |
| aserții atinse în `Program.cs` | 442: forma 34, regula 309, regula-catalog 0, oracol 13, comportament 53, neclar 33 |
| alte fișiere inventariate (un rând per fișier) | 43, cu 1529 linii de aserție |
| aserții atinse în celelalte fișiere | 97: forma 9, regula 10, regula-catalog 0, oracol 39, comportament 38, neclar 1 |
| **total aserții atinse** | **539**: forma 43, regula 319, regula-catalog 0, oracol 52, comportament 91, neclar 34 |
| referințe directe (lista canonică) | 807 (658 în `Program.cs`): ajutor 78, asertie 367, comentariu 26, montaj 33, neclar 4, oracol 133, purja 156, raport 10 |
| ajutoare locale inventariate | 138 |
| aserții atinse de D9-D8 (`RegulaStoc`; cuprinse în totalul de mai sus) | 11: (a) 7, (c) 3, (d) 1 |
| referințe `RegulaStoc` (în afara listei canonice) | 56 |
| aserții aduse de trecerea pe flux de date | 1 (`Program.cs:23483`); 21 de candidați respinși cu motiv |

**Convenții de verdict** (aplicate identic peste tot):

- O aserție mixtă (număr de rânduri și cifră) e `regula`; numărătoarea cade, cifra se re-țintește.
- Clauza „după refuz sau dry-run nimic materializat” (30 aserții, nota `atomicitate`), „după
  anulare nu rămân rânduri” (30, nota `anulare`) și „documentul nu mișcă stoc / nu produce fapt fiscal”
  (15, nota `absenta`) sunt `regula` cu ținta „zero postări pe `DocumentId`”: faptul de comandă
  rămâne, materializarea devine cubul. Dacă pin-ul 2 se citește strict („absența rândurilor de registru ca
  atare” = formă), cele trei note permit trecerea lor în bloc la `forma`.
- Flag-ul `Storno` al rândului, când aserția păzește cifra inversată, se re-țintește pe `Tranzactie.Fel = Storno`.
- Rândurile NIR-ului conex stau pe cub pe tranzacția facturii (TR-D3, B-D8 pct. 1): aceeași cifră, alt `DocumentId`;
  rămâne `regula`, cu filtrul numit (nota `TR-D3`, 13 aserții).
- Cheia `TipStoc` a registrului devine contul postării (`ContId` în `Citiri.Loturi`); consumul BCS e postarea pe
  contul de cheltuială, cu `Gestiune` = locul de consum.
- `RegistruTva.Data` e data faptului fiscal: pe `FaptFiscal` e `DataDocument` pentru operare și `Data` pentru storno.
- Câmpurile rezumatului SAF-T cu „Registru” în nume sunt deja calculate pe cub; aserțiile pe ele sunt neatinse.

**Maparea folosită pentru `tinta`.**

| Ce citea aserția | Cititorul cubului |
|---|---|
| notele unui document (cont debitor / creditor, valoare) | `Citiri.Contabil.Postari` pe `DocumentId` (o notă = două postări) |
| soldul sau rulajul unui cont | `Citiri.Contabil.Postari` (Σ pe cont × latură, `Data <=`); pe interval `Citiri.Contabil.Jurnal` |
| analiza notei (cod economic, sursă, proiect, centru de cost) | `Citiri.Contabil.Jurnal` (analiza laturii se păstrează pe postare) |
| soldul de stoc pe cheie (`StocService.Sold`, `SolduriLaData`, Σ `RegistruStoc`) | `Citiri.Loturi.Solduri` pe `LotId`, `GestiuneId`, `ContId` |
| mișcările de stoc ale unui document | `Citiri.Loturi.Postari` pe `DocumentId` |
| rândurile fiscale | `Citiri.Fiscale.Fapte`; pe luni `Citiri.Fiscale.IntreLuni` |
| fișa imobilizării | `Citiri.Imobilizari.Randuri` |
| `Document.TotalStingere` | `Citiri.Partide.Total` (prin `ImperechereService.Total`) |
| gardul de sold al registrului | `Citiri.Loturi.VerificaSoldIntermediar` |

Țintele celor 319 aserții `regula`, după primul cititor numit: `Citiri.Contabil.Postari` 129, `Citiri.Loturi.Postari` 62, `Citiri.Loturi.Solduri` 47, `Citiri.Fiscale.Fapte` 46, `Citiri.Imobilizari.Randuri` 12, `Citiri.Partide.Total` 10, `Citiri.Contabil.Jurnal` 5, `Citiri.Loturi.VerificaSoldIntermediar` 2, `Citiri.Fiscale.IntreLuni` 1, `Citiri.Fiscale.Postari` 1, `Citiri.Loturi.Cumulate` 1; restul pe cubul însuși (`Postare` / `Tranzactie`).

## 2. Unitățile

Unitățile din `Program.cs` acoperă liniile 1…33.241 fără găuri; cele care n-au nicio aserție atinsă sunt grupate
pe ultimul rând al tabelului. Aserțiile `forma` și purjele nu se enumeră aici una câte una: cifra e pe unitate, iar
rândurile stau în `asertii.tsv` (coloana `verdict` = `forma`) și în `referinte.tsv` (`verdict` = `purja`).

| Unitate (`Program.cs`) | linii | aserții | forma | regula | catalog | oracol | comp. | neclar | neatinse |
|---|---|---|---|---|---|---|---|---|---|
| ProbaReconciliere | 337–351 | 1 | 0 | 0 | 0 | 1 | 0 | 0 | 0 |
| Privat: Scenariul e2e P1 (profil privat) | 801–1316 | 39 | 0 | 14 | 0 | 0 | 0 | 2 | 23 |
| Privat: Scenariul e2e P2 (DSC) | 1317–1798 | 40 | 1 | 8 | 0 | 0 | 0 | 1 | 30 |
| Privat: Scenariul 1C-a NotaContabila | 1799–1891 | 8 | 0 | 4 | 0 | 0 | 0 | 1 | 3 |
| Privat: compensarea (NTC stingator) | 1892–2157 | 24 | 0 | 2 | 0 | 0 | 0 | 0 | 22 |
| Privat: Scenariul 1C-a InchidereTva | 2158–2368 | 16 | 1 | 7 | 0 | 0 | 0 | 0 | 8 |
| Privat: Felia API ITV (F21) | 2369–2944 | 46 | 0 | 5 | 0 | 0 | 0 | 0 | 41 |
| Privat: Scenariul 1C-a Asamblare | 2945–3196 | 17 | 1 | 7 | 0 | 0 | 5 | 0 | 4 |
| Privat: Scenariul 1C-a ReturFurnizor/ReturClient | 3197–3667 | 30 | 1 | 14 | 0 | 0 | 2 | 0 | 13 |
| Privat: 104c + Api DEC override TVA | 3733–3834 | 7 | 0 | 1 | 0 | 0 | 0 | 0 | 6 |
| Privat: Felia Api FCL | 3835–4381 | 60 | 0 | 5 | 0 | 0 | 1 | 0 | 54 |
| Bugetar: Scenariul e2e 3b | 4471–4643 | 26 | 3 | 7 | 0 | 0 | 1 | 2 | 13 |
| Bugetar: spike 1 felia BTR | 4644–4844 | 23 | 1 | 2 | 0 | 0 | 2 | 0 | 18 |
| Bugetar: Scenariul 3c FCT -> NIR | 4845–5172 | 37 | 1 | 10 | 0 | 5 | 0 | 1 | 20 |
| Bugetar: felia 2 Api FCT + NIR | 5173–5539 | 38 | 0 | 4 | 0 | 0 | 0 | 0 | 34 |
| Bugetar: Scenariul 3c BonConsum | 5540–5744 | 21 | 1 | 7 | 0 | 4 | 1 | 1 | 7 |
| Bugetar: Scenariul 3c ListaDiferenteInventar | 5745–5919 | 24 | 1 | 9 | 0 | 0 | 1 | 0 | 13 |
| Bugetar: Scenariul 3c FacturaIesire | 5920–6074 | 23 | 1 | 7 | 0 | 0 | 0 | 1 | 14 |
| Bugetar: Scenariul 3c Plata/Incasare + Imperechere | 6075–6419 | 42 | 0 | 7 | 0 | 0 | 0 | 1 | 34 |
| Bugetar: Felia Api Trz | 6420–6884 | 60 | 0 | 4 | 0 | 0 | 0 | 0 | 56 |
| Bugetar: felia 7 viramentul intern | 6885–7325 | 35 | 0 | 8 | 0 | 0 | 0 | 0 | 27 |
| Bugetar: Scenariul 3c Decont | 7326–7529 | 24 | 1 | 5 | 0 | 0 | 0 | 3 | 15 |
| Bugetar: Scenariul 1C-a NotaContabila | 7530–7724 | 18 | 1 | 8 | 0 | 0 | 0 | 2 | 7 |
| Bugetar: InchidereTva (tip inert) | 7725–7792 | 8 | 0 | 0 | 0 | 0 | 1 | 0 | 7 |
| Bugetar: felia 5 Api NIR scriere | 7818–8120 | 29 | 0 | 5 | 0 | 0 | 0 | 0 | 24 |
| Bugetar: felia 6 Api BCS scriere | 8121–8367 | 25 | 0 | 6 | 0 | 0 | 1 | 0 | 18 |
| Bugetar: felia 6 Api LDI scriere | 8368–8855 | 42 | 0 | 8 | 0 | 0 | 1 | 0 | 33 |
| Bugetar: Felia Api DEC | 8856–9249 | 42 | 0 | 7 | 0 | 0 | 0 | 2 | 33 |
| Bugetar: felia 8 pas 3 pereche prin API (E2E-APER) | 9250–9773 | 58 | 0 | 5 | 0 | 0 | 0 | 0 | 53 |
| VerificaRegistruTva | 10026–10628 | 24 | 2 | 14 | 0 | 0 | 0 | 2 | 6 |
| VerificaSaft | 11813–12844 | 46 | 0 | 2 | 0 | 0 | 0 | 0 | 44 |
| VerificaMiscariSaft | 13199–13494 | 14 | 0 | 0 | 0 | 0 | 1 | 0 | 13 |
| VerificaSaftStocuri | 13495–14330 | 34 | 0 | 3 | 0 | 0 | 0 | 0 | 31 |
| VerificaD300 | 15322–15986 | 26 | 0 | 4 | 0 | 0 | 0 | 0 | 22 |
| VerificaD394 | 16044–16674 | 28 | 1 | 6 | 0 | 0 | 0 | 0 | 21 |
| VerificaValoareIesire | 16675–17167 | 29 | 0 | 5 | 0 | 0 | 14 | 7 | 3 |
| VerificaApiNtc | 17168–17984 | 76 | 0 | 8 | 0 | 0 | 0 | 1 | 67 |
| VerificaApiAsm | 17985–18692 | 66 | 1 | 7 | 0 | 0 | 11 | 0 | 47 |
| VerificaApiRlf | 18693–19179 | 49 | 0 | 10 | 0 | 0 | 1 | 0 | 38 |
| VerificaApiRdc | 19180–19565 | 39 | 0 | 8 | 0 | 0 | 1 | 0 | 30 |
| VerificaF23Gardian | 20397–20785 | 9 | 0 | 0 | 0 | 0 | 1 | 0 | 8 |
| VerificaF24Rol | 20974–21063 | 3 | 0 | 0 | 0 | 0 | 0 | 1 | 2 |
| VerificaF24Gardian | 21064–21214 | 3 | 0 | 1 | 0 | 0 | 1 | 0 | 1 |
| VerificaPotrivire | 21215–21436 | 1 | 0 | 0 | 0 | 0 | 1 | 0 | 0 |
| VerificaF24Explica | 21437–21687 | 12 | 0 | 1 | 0 | 0 | 0 | 0 | 11 |
| VerificaD85 | 21688–22022 | 8 | 0 | 0 | 0 | 0 | 0 | 1 | 7 |
| VerificaDvi | 22023–22591 | 24 | 0 | 5 | 0 | 0 | 0 | 1 | 18 |
| VerificaSolduriPerioada | 22997–23703 | 25 | 0 | 2 | 0 | 0 | 0 | 0 | 23 |
| VerificaDataInregistrare | 23704–24107 | 19 | 0 | 9 | 0 | 0 | 0 | 1 | 9 |
| VerificaPerioadaDeclarare | 24108–24446 | 21 | 0 | 7 | 0 | 0 | 0 | 0 | 14 |
| VerificaCorectie | 24452–25023 | 25 | 0 | 8 | 0 | 0 | 0 | 0 | 17 |
| VerificaAcceptare | 25024–25507 | 24 | 0 | 1 | 0 | 0 | 0 | 0 | 23 |
| VerificaPartide | 26000–26557 | 27 | 1 | 2 | 0 | 0 | 0 | 1 | 23 |
| VerificaImobilizari | 26558–28127 | 71 | 2 | 12 | 0 | 0 | 0 | 1 | 56 |
| VerificaImobilizariApi | 28128–28645 | 26 | 1 | 0 | 0 | 0 | 0 | 0 | 25 |
| VerificaApiDvi | 28646–29012 | 23 | 0 | 1 | 0 | 0 | 0 | 0 | 22 |
| VerificaReviewF26 | 29013–29683 | 25 | 0 | 4 | 0 | 0 | 0 | 0 | 21 |
| VerificaReviewF27 | 29684–30480 | 32 | 2 | 7 | 0 | 0 | 0 | 0 | 23 |
| VerificaNucleuTrezorerie | 31287–31773 | 33 | 5 | 0 | 0 | 1 | 1 | 0 | 26 |
| VerificaNucleuBcs | 31774–32001 | 9 | 0 | 1 | 0 | 1 | 3 | 0 | 4 |
| VerificaNucleuBtr | 32002–32190 | 8 | 1 | 0 | 0 | 0 | 0 | 0 | 7 |
| VerificaNucleuFclDsc | 32191–32654 | 21 | 2 | 0 | 0 | 0 | 0 | 0 | 19 |
| VerificaNucleuFct | 32655–33084 | 24 | 2 | 5 | 0 | 1 | 2 | 0 | 14 |
| *45 unități fără aserții atinse* | — | 258 | 0 | 0 | 0 | 0 | 0 | 0 | 258 |
| **Total `Program.cs`** | 1–33241 | **2025** | **34** | **309** | **0** | **13** | **53** | **33** | **1583** |

Unitățile fără aserții atinse: antet (usings, Conexiunea, RegistruTvaIntreLuni) (1–56); mod --dump-metadata (57–79); mod --dump-integritate-tph (80–96); mod --declaratie-pe-baza (97–119); mod --reconciliere-cub (120–162); infrastructura A (profil, Check, lant, moduri fara baza, provider) (163–336); infrastructura B (InchideAcceptTot, severitate ITV, seed, conventie) (352–402); model si seed (bloc filtruScenarii == null) (403–714); mod --perf-saft (715–747); mod --perf-cub (748–790); mod --scenarii (791–800); Privat: Api FCT override TVA (3668–3732); Privat: dispecerul suitei private (4382–4470); Bugetar: retururile (tipuri inerte) (7793–7817); Bugetar: dispecerul suitei bugetare (9774–9842); VerificaAxaTaxareInversa (9843–9892); VerificaGardianCicluCont (9893–10016); VerificaBalanta + VerificaFisaJurnal (delegari) (10017–10025); VerificaD300Seed (10629–10990); VerificaD394Seed (10991–11209); VerificaAdresaPartener (11210–11481); VerificaSaftModel (11482–11812); VerificaSaftJson (12845–12992); VerificaSaftXml (12993–13198); VerificaSaftStocuriXml (14331–14659); VerificaSaftStocuriFixuri (14660–14963); VerificaSincronizareAnaf (14964–15321); FctBugetaraOperata + PurjaFctBugetara (15987–16043); VerificaF23Model (19566–19687); VerificaF23Rezolvare (19688–19945); VerificaF23Seed (19946–20069); VerificaF24Seed (20070–20396); VerificaF23Raport (20786–20935); VerificaF23ClasaFiscala (20936–20973); VerificaReconciliereMf (22592–22739); VerificaPerioade (22740–22996); PrimaLinie + Ziua (24447–24451); VerificaReviewAcceptare (25508–25999); VerificaF28 (30481–30946); ScenelePeTip (30947–31048); RuleazaScenele (31049–31075); VerificaInvariantiCub (31076–31086); VerificaSchemaCub (31087–31230); VerificaPozitieLinii (31231–31286); VerificaLaturi (33085–33241).

| Fișier (unitate = fișierul întreg) | linii | aserții | forma | regula | catalog | oracol | comp. | neclar | neatinse |
|---|---|---|---|---|---|---|---|---|---|
| `ScenariiAsm.cs` | 436 | 90 | 0 | 0 | 0 | 7 | 20 | 0 | 63 |
| `ScenariiBcs.cs` | 370 | 60 | 0 | 1 | 0 | 0 | 0 | 0 | 59 |
| `ScenariiBtr.cs` | 178 | 45 | 1 | 0 | 0 | 1 | 0 | 0 | 43 |
| `ScenariiDec.cs` | 231 | 54 | 0 | 1 | 0 | 0 | 0 | 0 | 53 |
| `ScenariiDeschidere.cs` | 401 | 62 | 1 | 0 | 0 | 0 | 0 | 0 | 61 |
| `ScenariiDvi.cs` | 309 | 72 | 0 | 0 | 0 | 2 | 2 | 0 | 68 |
| `ScenariiExplicatii.cs` | 154 | 16 | 0 | 0 | 0 | 0 | 1 | 0 | 15 |
| `ScenariiFct.cs` | 212 | 54 | 0 | 2 | 0 | 0 | 0 | 0 | 52 |
| `ScenariiFisa.cs` | 110 | 15 | 0 | 0 | 0 | 3 | 0 | 0 | 12 |
| `ScenariiImo.cs` | 443 | 77 | 0 | 1 | 0 | 0 | 0 | 0 | 76 |
| `ScenariiItv.cs` | 129 | 29 | 0 | 1 | 0 | 0 | 0 | 0 | 28 |
| `ScenariiLdi.Folosinta.cs` | 134 | 37 | 3 | 0 | 0 | 2 | 0 | 0 | 32 |
| `ScenariiNir.Acoperire.cs` | 64 | 3 | 0 | 0 | 0 | 3 | 0 | 0 | 0 |
| `ScenariiNir.Diferente.cs` | 339 | 53 | 0 | 1 | 0 | 3 | 3 | 0 | 46 |
| `ScenariiNir.cs` | 194 | 49 | 0 | 0 | 0 | 2 | 0 | 0 | 47 |
| `ScenariiNtc.cs` | 229 | 67 | 1 | 0 | 0 | 0 | 0 | 0 | 66 |
| `ScenariiPartideCub.cs` | 472 | 54 | 0 | 0 | 0 | 0 | 3 | 1 | 50 |
| `ScenariiRdc.cs` | 218 | 50 | 0 | 1 | 0 | 0 | 2 | 0 | 47 |
| `ScenariiRlf.cs` | 192 | 54 | 0 | 0 | 0 | 0 | 3 | 0 | 51 |
| `ScenariiSaftStocuri.cs` | 611 | 47 | 1 | 0 | 0 | 0 | 0 | 0 | 46 |
| `ScenariiSnapshotStoc.cs` | 204 | 17 | 1 | 0 | 0 | 0 | 1 | 0 | 15 |
| `ScenariiStocCub.cs` | 101 | 6 | 1 | 0 | 0 | 0 | 0 | 0 | 5 |
| `ScenaDocumente.cs` | 475 | 15 | 0 | 1 | 0 | 0 | 0 | 0 | 14 |
| `AcoperireInvarianti.cs` | 267 | 3 | 0 | 0 | 0 | 1 | 0 | 0 | 2 |
| `PerfCub.Evaluare.cs` | 266 | 13 | 0 | 0 | 0 | 3 | 0 | 0 | 10 |
| `ProbeAsmOperand.cs` | 108 | 13 | 0 | 0 | 0 | 7 | 2 | 0 | 4 |
| `Nucleu/ProbeCub.cs` | 328 | 11 | 0 | 1 | 0 | 3 | 1 | 0 | 6 |
| `Nucleu/ProbeNucleu.cs` | 103 | 6 | 0 | 0 | 0 | 2 | 0 | 0 | 4 |
| *15 fișiere fără aserții atinse* | — | 457 | 0 | 0 | 0 | 0 | 0 | 0 | 457 |
| **Total celelalte fișiere** | — | **1529** | **9** | **10** | **0** | **39** | **38** | **1** | **1432** |
| **Total general** | — | **3554** | **43** | **319** | **0** | **52** | **91** | **34** | **3015** |

Fișierele fără aserții atinse: `Scenarii.cs`, `ScenariiBalanta.cs`, `ScenariiCitiri.cs`, `ScenariiConcurenta.cs`, `ScenariiFiscale.cs`, `ScenariiImo.Review.cs`, `ScenariiLdi.cs`, `ScenariiNir.Review.cs`, `ScenariiRegim.cs`, `ScenariiSaft.cs`, `ScenariiSnapshotStocReview.cs`, `ScenariiTrezorerie.cs`, `ScenariiTvaIntervale.cs`, `ScenariiVanzare.cs`, `Purja.cs`.

`regula-catalog` e zero: niciun rând `SC-…` n-a fost confruntat clauză cu clauză cu o aserție din `Program.cs`,
deci nicio aserție de regulă nu e propusă spre ștergere pe motiv de acoperire. Unde un rând de catalog e vizibil
apropiat, e trecut în coloana `pereche` (de exemplu `SC-RLF-05` pentru golirea fiscală a returului).

## 3. Ajutoarele

Ajutoarele sunt funcții locale ale scenei; aceleași nume (`Note`, `Sold`, `SoldCheie`, `Curata`) se redefinesc
în mai multe unități. `apeluri` = numărul de apeluri din unitatea lui (din tot fișierul pentru cele globale).

Cele 57 funcții de curățenie (`Curata…`, `PurjaFctBugetara`) sunt purje: își pierd numai liniile de
registru și rămân. Restul:

| Fișier:linie | Ajutor | Ce citește | Apeluri | Verdict | Ținta |
|---|---|---|---|---|---|
| `Program.cs:54` | RegistruTvaIntreLuni (global) | RegistruTva pe lunile de declarare din interval | 2 | regula | Citiri.Fiscale.IntreLuni (Citiri.Fiscale.Fapte, deLa, panaLa) |
| `Program.cs:340` | ProbaReconciliere (global) | ReconciliereCub.Ruleaza pe documentele scenei (registre vs cub) | 8 | oracol |  |
| `Program.cs:944` | Note (Privat P1) | RegistruContabil pe DocumentId, fără Storno | 13 | regula | Citiri.Contabil.Postari (DocumentId, fără Storno) |
| `Program.cs:1415` | Note (Privat P2 DSC) | RegistruContabil pe DocumentId, fără Storno | 3 | regula | Citiri.Contabil.Postari (DocumentId, fără Storno) |
| `Program.cs:1532` | Venit (Privat P2 DSC) | nota de venit a liniei, din noteFcl (RegistruContabil) | 11 | regula | Citiri.Contabil.Postari (DocumentId, LinieId) |
| `Program.cs:2214` | SoldDebitor (+ SoldCreditor, Privat ITV) | Σ RegistruContabil pe cont până la dată (debit − credit), toată baza | 9 | regula | Citiri.Contabil.Postari (Σ pe cont, Data <=) |
| `Program.cs:2238` | NoteItv | RegistruContabil pe DocumentId, fără Storno | 2 | forma |  |
| `Program.cs:2439` | SoldDebitorApi (+ SoldCreditorApi, Privat API ITV) | Σ RegistruContabil pe cont până la dată, toată baza | 6 | regula | Citiri.Contabil.Postari (Σ pe cont, Data <=) |
| `Program.cs:3018` | Stoc (Privat ASM) | RegistruStoc pe DocumentId, fără Storno | 3 | regula | Citiri.Loturi.Postari (DocumentId) |
| `Program.cs:3088` | RefuzAsm | refuz la operare + absența RegistruStoc/RegistruContabil pe document | 9 | regula | zero postări pe DocumentId (atomicitate) |
| `Program.cs:3310` | Stoc (Privat RLF/RDC) | RegistruStoc pe DocumentId, fără Storno | 3 | regula | Citiri.Loturi.Postari (DocumentId) |
| `Program.cs:3312` | Note (Privat RLF/RDC) | RegistruContabil pe DocumentId, fără Storno | 4 | regula | Citiri.Contabil.Postari (DocumentId) |
| `Program.cs:3314` | SoldCont (Privat RLF/RDC) | Σ RegistruContabil pe cont (debit − credit), toată baza | 5 | regula | Citiri.Contabil.Postari (Σ pe cont) |
| `Program.cs:3317` | SoldStoc (Privat RLF/RDC) | StocService.Sold pe (lot, gestiune, Marfuri), opțional la dată | 5 | regula | Citiri.Loturi.Solduri (LotId, GestiuneId, laData) |
| `Program.cs:3459` | RefuzRet | refuz la operare + absența RegistruStoc/RegistruContabil pe document | 8 | regula | zero postări pe DocumentId (atomicitate) |
| `Program.cs:4538` | Sold (Bugetar 3b) | StocService.Sold pe (lot, gestiune, Magazie) | 8 | regula | Citiri.Loturi.Solduri (LotId, GestiuneId) |
| `Program.cs:4539` | RanduriStoc (Bugetar 3b) | RegistruStoc pe DocumentId (cu inversele) | 3 | regula | Citiri.Loturi.Postari (DocumentId) |
| `Program.cs:5619` | Sold (Bugetar BCS) | StocService.Sold pe (lot, repartitor, TipStoc) | 6 | regula | Citiri.Loturi.Solduri (LotId, GestiuneId, ContId) |
| `Program.cs:5820` | Sold (Bugetar LDI) | StocService.Sold pe (lot, MAG1, Magazie) | 6 | regula | Citiri.Loturi.Solduri (LotId, GestiuneId) |
| `Program.cs:5987` | Note (Bugetar FCL) | RegistruContabil pe DocumentId (cu inversele) | 5 | regula | Citiri.Contabil.Postari (DocumentId) |
| `Program.cs:6166` | Note (Bugetar Plata/Incasare) | RegistruContabil pe DocumentId, fără Storno | 4 | regula | Citiri.Contabil.Postari (DocumentId) |
| `Program.cs:7385` | Note (Bugetar Decont) | RegistruContabil pe DocumentId, fără Storno | 4 | regula | Citiri.Contabil.Postari (DocumentId) |
| `Program.cs:7596` | Note (Bugetar NTC) | RegistruContabil pe DocumentId, fără Storno | 4 | regula | Citiri.Contabil.Postari (DocumentId) |
| `Program.cs:7642` | RefuzNtc | refuz la operare + absența RegistruContabil pe document | 4 | regula | zero postări pe DocumentId (atomicitate) |
| `Program.cs:7981` | RefuzNir | refuz la operare + absența RegistruStoc/RegistruContabil pe document | 3 | regula | zero postări pe DocumentId (atomicitate) |
| `Program.cs:8204` | SoldBcs | StocService.Sold pe (lot, repartitor, TipStoc) | 6 | regula | Citiri.Loturi.Solduri (LotId, GestiuneId, ContId) |
| `Program.cs:8278` | RefuzBcs | refuz la operare + absența RegistruStoc/RegistruContabil pe document | 4 | regula | zero postări pe DocumentId (atomicitate) |
| `Program.cs:8453` | SoldLdi | StocService.Sold pe (lot, MAG1, Magazie) | 6 | regula | Citiri.Loturi.Solduri (LotId, GestiuneId) |
| `Program.cs:8657` | RefuzLdi | refuz la operare + absența RegistruStoc/RegistruContabil pe document | 9 | regula | zero postări pe DocumentId (atomicitate) |
| `Program.cs:8944` | NoteDec | RegistruContabil pe DocumentId, fără Storno | 2 | regula | Citiri.Contabil.Postari (DocumentId) |
| `Program.cs:9112` | RefuzDec | refuz la operare + absența RegistruContabil/RegistruStoc pe document | 5 | regula | zero postări pe DocumentId (atomicitate) |
| `Program.cs:9348` | Sold581 | Σ RegistruContabil pe 581 (debit − credit) pe documentele date | 4 | regula | Citiri.Contabil.Postari (Σ pe cont, pe DocumentId) |
| `Program.cs:10074` | Fiscal (VerificaRegistruTva) | RegistruTva pe DocumentId (cu inversele) | 10 | regula | Citiri.Fiscale.Fapte (DocumentId) |
| `Program.cs:10150` | Rand (VerificaRegistruTva) | rândul fiscal al liniei, din fiscalA (RegistruTva) | 8 | regula | Citiri.Fiscale.Fapte (DocumentId, DetaliuId) |
| `Program.cs:14796` | LotCuDeschidere | scrie de mână un rând RegistruStoc de deschidere (ramură moartă: numai la cantitate <= 0) | 1 | montaj | ramura se șterge; calea vie e FCT + NIR |
| `Program.cs:15422` | SoldNet (VerificaD300) | Σ RegistruContabil pe cont până la dată (debit − credit), toată baza | 2 | regula | Citiri.Contabil.Postari (Σ pe cont, Data <=) |
| `Program.cs:16456` | RegBaza / RegTva (VerificaD394) | Σ RegistruTva pe sens, din variabila brut | 3 | regula | Citiri.Fiscale.Fapte (pe perioadă, pe sens) |
| `Program.cs:16584` | CusaturaS (VerificaD394) | Σ proiecție D394 = Σ RegistruTva (brutS) pe sens | 2 | regula | Citiri.Fiscale.Fapte (pe perioadă, pe sens) |
| `Program.cs:16775` | SoldCheie (VerificaValoareIesire) | Σ RegistruStoc (cantitate, valoare) pe cheia (lot, repartitor, TipStoc) | 21 | regula | Citiri.Loturi.Solduri (LotId, GestiuneId, ContId) |
| `Program.cs:16780` | Randuri (VerificaValoareIesire) | RegistruStoc pe DocumentId (cu inversele) | 4 | regula | Citiri.Loturi.Postari (DocumentId) |
| `Program.cs:16817` | Deschidere (VerificaValoareIesire) | rând sintetic StocService.RandGolire pentru VerificaGoliri | 1 | neclar | soarta StocService.VerificaGoliri |
| `Program.cs:17273` | NoteNtc | RegistruContabil pe DocumentId, fără Storno | 4 | regula | Citiri.Contabil.Postari (DocumentId) |
| `Program.cs:17455` | RefuzOperare (VerificaApiNtc) | refuz la operare + absența RegistruContabil/RegistruStoc pe document | 4 | regula | zero postări pe DocumentId (atomicitate) |
| `Program.cs:18123` | SoldCheie (VerificaApiAsm) | Σ RegistruStoc pe (lot, repartitor, TipStoc) | 10 | regula | Citiri.Loturi.Solduri (LotId, GestiuneId) |
| `Program.cs:18822` | SoldCheie (VerificaApiRlf) | Σ RegistruStoc pe cheia lotului | 9 | regula | Citiri.Loturi.Solduri (LotId, GestiuneId) |
| `Program.cs:18827` | Note (VerificaApiRlf) | RegistruContabil pe DocumentId, fără Storno | 3 | regula | Citiri.Contabil.Postari (DocumentId) |
| `Program.cs:19280` | SoldCheie (VerificaApiRdc) | Σ RegistruStoc pe cheia lotului scenei | 5 | regula | Citiri.Loturi.Solduri (LotId, GestiuneId) |
| `Program.cs:19285` | Note (VerificaApiRdc) | RegistruContabil pe DocumentId, fără Storno | 2 | regula | Citiri.Contabil.Postari (DocumentId) |
| `Program.cs:22488` | Net446 (VerificaDvi) | Σ RegistruContabil pe 446 (debit − credit) pe documentele date | 2 | regula | Citiri.Contabil.Postari (Σ pe cont, pe DocumentId) |
| `Program.cs:23082` | SnapshotStoc (VerificaSolduriPerioada) | SoldPerioadaStoc (snapshot-ul perioadei), nu registre; poartă valorile în tipul SoldStoc din Motor/StocService.cs, care moare (D9-D2) — se schimbă purtătorul, nu sursa | 2 | ajutor | alt purtător (tuplu sau record local) |
| `Program.cs:23087` | AsteptatStoc (VerificaSolduriPerioada) | Postare (cubul), nu registre; poartă valorile în tipul SoldStoc din Motor/StocService.cs, care moare (D9-D2) — se schimbă purtătorul, nu sursa | 2 | ajutor | alt purtător (tuplu sau record local) |
| `Program.cs:23393` | Citiri (VerificaSolduriPerioada) | dicționarul citirilor comparate cu și fără snapshot; cinci chei citesc prin StocService (SolduriLaData, Sold, AlocaFifoTolerant, VerificaSoldIntermediar), restul prin proiecțiile produsului | 2 | regula | Citiri.Loturi.Cumulate / Disponibile / VerificaSoldIntermediar |
| `Program.cs:23438` | Solduri (în Citiri) | StocService.SolduriLaData pe loturile scenei | 2 | regula | Citiri.Loturi.Cumulate (laData) |
| `Program.cs:23496` | ComparaCitirile | compară Citiri(os) cu citirile de dinaintea închiderii; un Check per cheie | 2 | regula | vezi Citiri |
| `Program.cs:23771` | StocDoc (VerificaDataInregistrare) | RegistruStoc pe DocumentId | 6 | regula | Citiri.Loturi.Postari (DocumentId) |
| `Program.cs:23773` | NoteDoc (VerificaDataInregistrare) | RegistruContabil pe DocumentId | 4 | regula | Citiri.Contabil.Postari (DocumentId) |
| `Program.cs:24167` | Fiscale (VerificaPerioadaDeclarare) | RegistruTva pe DocumentId, ordonat pe Storno | 5 | regula | Citiri.Fiscale.Fapte (DocumentId) |
| `Program.cs:24521` | StocDoc (VerificaCorectie) | RegistruStoc pe DocumentId, ordonat pe Storno | 3 | regula | Citiri.Loturi.Postari (DocumentId) |
| `Program.cs:24524` | NoteDoc (VerificaCorectie) | RegistruContabil pe DocumentId, ordonat pe Storno | 3 | regula | Citiri.Contabil.Postari (DocumentId) |
| `Program.cs:24527` | Fiscale (VerificaCorectie) | RegistruTva pe DocumentId, ordonat pe Storno | 8 | regula | Citiri.Fiscale.Fapte (DocumentId) |
| `Program.cs:31846` | Intrare (VerificaNucleuBcs) | SCRIE de mână un rând RegistruStoc de intrare (Magazie, MAG1) | 4 | montaj | DeschidereScena.Scrie |
| `Program.cs:32062` | Intrare (VerificaNucleuBtr) | SCRIE de mână un rând RegistruStoc de intrare (Magazie, MAG1) | 4 | montaj | DeschidereScena.Scrie |
| `Program.cs:32515` | Intrare (VerificaNucleuFclDsc) | SCRIE de mână un rând RegistruStoc de intrare | 6 | montaj | DeschidereScena.Scrie |
| `AcoperireInvarianti.cs:78` | RegistruIstoric | scrie prin SQL rânduri RegistruContabil fără document (martorul și mutantul DESCHIDERE) | 2 | oracol |  |
| `AcoperireInvarianti.cs:113` | TotalDecontareDiferit | schimbă Document.TotalStingere (mutantul POLITICA) | 1 | neclar | D9-D10 |
| `AcoperireInvarianti.cs:125` | FisaPeAltaUnitate | țintă aleasă din RegistruImobilizari (mutantul IMO-FISA) | 1 | oracol |  |
| `AcoperireInvarianti.cs:166` | RegistruImobilizariDiferit | schimbă un rând RegistruImobilizari (mutantul IMO-REGISTRU) | 1 | oracol |  |
| `Nucleu/ProbeCub.cs:113` | ProbaTransferPliat | cub pliat = registre normalizate | 4 | oracol |  |
| `Nucleu/ProbeCub.cs:146` | FaraRanduri | zero tranzacții și postări pe document (cub) | 11 | comportament | apelurile STR-NEMIGRAT rămân fără obiect |
| `Nucleu/ProbeCub.cs:152` | ProbaOperare | STR-OPERARE / STR-ORACOL (oracol) + STR-ROUNDTRIP (cub = contract) | 16 | oracol | rămân numai aserțiile pe cub (165, 173, 211) |
| `Nucleu/ProbeCub.cs:276` | Scrie | afișează diferența a două seturi de postări prin Comparabil | 2 | ajutor | înlocuitor fără Comparabil |
| `Nucleu/ProbeCub.cs:306` | Comutator (Migrat / Nemigrat / MigratPeClasa) | comută TipDocument.PosteazaInCub pe durata scenei | 4 | montaj | dispare cu D9-D5 |
| `Nucleu/ProbeNucleu.cs:22` | Proba | contract vs registre normalizate (B-D8) | 25 | oracol | rămân contractul acceptat, conservarea, determinismul și pragul de interogări |
| `ScenaDocumente.cs:405` | FaraEfecte | zero efecte persistate: Tranzactie, Postare și cele trei registre | 105 | regula | clauzele de registru se șterg; cele pe cub rămân |
| `ScenariiAsm.cs:225` | ProbaReconciliere (local) | ReconciliereCub.Ruleaza / Asm pe document | 1 | oracol |  |
| `ScenariiAsm.cs:352` | Diagnostic | DiagnosticValoriStoc.Citeste pe lot | 2 | oracol |  |
| `ScenariiBcs.cs:178` | FaraEfecte (ScenariiBcs) | zero efecte persistate: Tranzactie, Postare, RegistruStoc, RegistruContabil | 8 | regula | clauzele de registru se șterg |
| `ScenariiBtr.cs:26` | Registru (în Capete) | Σ cantitate RegistruStoc pe document × lot × gestiune | 2 | forma |  |
| `ScenariiDvi.cs:165` | Normalizare | Normalizari.Toate(CubDinRegistre.Transforma(…)) | 4 | oracol |  |
| `ScenariiNir.Acoperire.cs:41` | Registru | Σ cantitate RegistruStoc pe lot × documente | 2 | oracol |  |
| `ScenariiRlf.cs:61` | InCub | TipDocument.PosteazaInCub pe cod | 3 | comportament | D9-D5 |

## 4. Aserțiile `regula`, pe unități

`pereche` = aserția din aceeași scenă care verifică deja aceeași cifră pe cub (la pasul 3 cea veche doar se
șterge). Unde coloana e goală, re-țintirea scrie citirea de pe cub.

### Privat: Scenariul e2e P1 (profil privat)

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 981 | FCT: 3 note — serviciul net (628 = 401, 100) + câte un rând 4426 per linie | Citiri.Contabil.Postari (DocumentId) |  | numărătoarea (3 note) cade; pe cub FCT poartă și recepția (TR-D3), deci filtrul e pe cont: 628 D 100 / 401 C; 4426 D pe ambele linii |
| 985 | Rândul 4426 al liniei de STOC există deși linia nu are regulă principală (netul… | Citiri.Contabil.Postari (DocumentId, LinieId) |  | 4426 D 10,5 pe linia de stoc |
| 999 | NIR: +5/50 în stoc (evaluare la net) și o singură notă 302 = 401, 50 — fără rân… | Citiri.Loturi.Postari + Citiri.Contabil.Postari (DocumentId = FCT) |  | TR-D3: pe cub recepția 5/50 și 302 = 401 stau pe tranzacția FCT, nu pe NIR-ul conex; numărătorile cad |
| 1011 | Plata contează 401 = 5121 (banca) pe brut | Citiri.Contabil.Postari (DocumentId) |  | 401 D / 5121 C, Σ 181,5; numărătoarea cade |
| 1041 | FCL: 4111 = 704 net (200) + 4111 = 4427 (42); scadența default +30; total brut … | Citiri.Contabil.Postari (DocumentId) |  | 4111 D 200 + 42; 704 C 200; 4427 C 42; numărătoarea cade |
| 1081 | Taxare inversă: serviciul net (628 = 401, 100) + autolichidare 4426 = 4427 (21)… | Citiri.Contabil.Postari (DocumentId) |  | 628 D 100 / 401 C 100; 4426 D 21 / 4427 C 21; numărătoarea cade |
| 1094 | Storno cu TVA: rândurile inverse includ și rândul 4426 = 4427 (−100, −21) | Citiri.Contabil.Postari (DocumentId, Tranzactie.Fel = Storno) |  | flag-ul Storno al rândului devine felul tranzacției; −100 și −21 pe 4426; numărătoarea cade |
| 1126 | F13-D1 FCL + TI21: livrarea în taxare inversă NU poartă taxă — linia rămâne pe … | Citiri.Contabil.Postari (DocumentId) + Citiri.Fiscale.Fapte (DocumentId) |  | 4111 D 300 / 704 C 300, fără 4426/4427; fapt fiscal Livrare/TaxareInversa/21 %, Baza 300, Tva 0, Storno fals; numărătorile cad |
| 1143 | F13-D1 storno FCL-TI: se inversează DOAR venitul (−300) — nu există rând de TVA… | Citiri.Contabil.Postari (DocumentId, Tranzactie.Fel = Storno) |  | −300 pe venit, nicio postare 4426/4427 |
| 1179 | F13-D1 gard: mesajul numește linia și suma culeasă, iar dry-run-ul (`Valideaza`… | Citiri.Contabil.Postari + Citiri.Fiscale.Postari (DocumentId): zero postări |  | atomicitate: numai clauza „nimic materializat" citește registrele; restul aserției rămâne |
| 1204 | Capitalizat (NED21): Valoare = brut 121, ValoareTva 0, o singură notă 628 = 401… | Citiri.Contabil.Postari (DocumentId) | 1218: NUC-FCT-CAP (contract, nu cub persistat) | 628 D 121 (pe cub bază 100 + taxă 21 pe același cont, B-D8 pct. 5), fără 4426; numărătoarea cade |
| 1241 | ValoareTva culeasă manual (20,9) nu se suprascrie la operare; rândul 4426 o pos… | Citiri.Contabil.Postari (DocumentId) | 1253: NUC-FCT-TOLERANTA-NULL (contract) | 4426 D 20,9 |
| 1287 | FCL: ValoareTva culeasă (20,99) nu se suprascrie; 4111 = 4427 postează exact 20… | Citiri.Contabil.Postari (DocumentId) |  | 4111 D / 4427 C 20,99 |
| 1306 | DEC: ValoareTva culeasă (20,99) nu se suprascrie; 4426 = 542 postează exact 20,… | Citiri.Contabil.Postari (DocumentId) |  | 4426 D / 542 C 20,99 |

### Privat: Scenariul e2e P2 (DSC)

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 1439 | Loturi de marfă finalizate la NET (A1=5, A2=6/buc, în Marfuri) + C1=8 (Magazie) | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | 10/10/5 pe loturi; cheia TipStoc (Marfuri/Magazie) devine contul de stoc 371/345 |
| 1533 | FCL note: 4111=707 net pe marfă L1/L2/L4 (venitul se postează ACUM, inclusiv fă… | Citiri.Contabil.Postari (DocumentId, LinieId) |  | 4111 D / 707 C: 120, 50, 63 |
| 1537 | FCL note: 4111=701 pe produsul finit L5 (60), 4111=704 pe serviciul L3 (100) | Citiri.Contabil.Postari (DocumentId, LinieId) |  | 701 C 60; 704 C 100 |
| 1570 | Cost marfă: 607 = 371 la COST 92 (30+50+12) — decuplat de vânzarea 707 (233) | Citiri.Contabil.Postari (DocumentId) |  | 607 D 92 cu credit pe contul de stoc 371; 707 C 233 pe FCL; lot cu preț unic, deci evaluarea cubului dă aceeași cifră (T-D4.2 nu se manifestă) |
| 1575 | Cost produs finit: 711 = 345, 24; exact 4 note pe DSC | Citiri.Contabil.Postari (DocumentId) |  | 711 D / 345 C 24; numărătoarea (4 note) cade |
| 1582 | DSC stoc: −10 A1, −7 A2 (Marfuri), −3 C1 (Magazie), toate pe gestiune | Citiri.Loturi.Postari (DocumentId) |  | −10 A1, −7 A2, −3 C1 pe gestiune; clauza TipStoc cade (devine contul postării) |
| 1610 | DSC₂ operat: 607 = 371 la costul B (28) | Citiri.Contabil.Postari (DocumentId) |  | 607 D / 371 C 28 |
| 1618 | Storno DSC₂ → Stornat, B1 revenit în stoc | Citiri.Loturi.Solduri (LotId, GestiuneId, laData) |  | 7 după storno |

### Privat: Scenariul 1C-a NotaContabila

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 1867 | Privat: exact 2 rânduri, conturile OMFP explicite (605 = 401; 471 = 605) | Citiri.Contabil.Postari (DocumentId) |  | 605 D / 401 C; 471 D / 605 C; numărătoarea (2) cade |
| 1870 | Privat: valorile CA ATARE (300 / −50) | Citiri.Contabil.Postari (DocumentId) |  | 300 și −50, valoarea ca atare (semnul rămâne pe postare) |
| 1875 | Privat: nota nu postează TVA (fără TipTva pe linii) și nu mișcă stoc | Citiri.Loturi.Postari (DocumentId): zero postări |  | absenta: „nu mișcă stoc"; clauza DetaliuId != null devine LinieId != null pe Citiri.Contabil.Postari |
| 1881 | Privat: storno → rânduri inverse (−300 / +50) la data stornării | Citiri.Contabil.Postari (DocumentId, Tranzactie.Fel = Storno) |  | −300 / +50 la data stornării; numărătorile (4, 2) cad |

### Privat: compensarea (NTC stingator)

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 2030 | Nota de compensare operată: 401 = 4111 pe X (60), fără stoc și fără TVA | Citiri.Contabil.Postari (DocumentId) |  | 401 D 60 / 4111 C 60 |
| 2067 | După ștergerea stingerilor nota se anulează normal (link fără registre proprii … | Citiri.Contabil.Postari (DocumentId): zero postări |  | anulare |

### Privat: Scenariul 1C-a InchidereTva

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 2221 | Precondiție: soldurile cumulate 4426/4427 la 30.09.2026 sunt 0 (blocurile anter… | Citiri.Contabil.Postari (Σ pe cont, Data <=) |  | precondiție: 4426/4427 la zero |
| 2267 | Septembrie: TVA-ul lunii în registru — 4426 deductibilă 21, 4427 colectată 42 | Citiri.Contabil.Postari (Σ pe cont, Data <=) |  | 4426 D 21; 4427 C 42 |
| 2287 | După închidere: 4423 TVA de plată = 21, iar 4426/4427 rămân la 0 pe 30.09 | Citiri.Contabil.Postari (Σ pe cont, Data <=) |  | 4423 C 21; 4426/4427 zero |
| 2318 | Operare octombrie: 4424 TVA de recuperat = 10,5 și 4426 revine la 0 | Citiri.Contabil.Postari (Σ pe cont, Data <=) |  | 4424 D 10,5; 4426 zero |
| 2325 | Storno octombrie: rânduri inverse la data stornării, soldul de recuperat revine… | Citiri.Contabil.Postari (Σ pe cont, Data <=) |  | după storno: 4424 zero, 4426 D 10,5 |
| 2354 | Draftul stale nu a lăsat rânduri-fantomă (33d) | Citiri.Contabil.Postari (DocumentId): zero postări |  | atomicitate |
| 2362 | Curățenie finală închidere TVA (fără reziduuri e2e; soldurile de TVA revin la 0) | Citiri.Contabil.Postari (Σ pe cont, Data <=) |  | după purjă soldurile 4426/4427 sunt zero; restul aserției nu citește registre |

### Privat: Felia API ITV (F21)

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 2446 | F21-D9 precondiție — BLOCUL ĂSTA TREBUIE SĂ RĂMÂNĂ ÎNAINTEA SCENELOR `E2E-ASM` … | Citiri.Contabil.Postari (Σ pe cont, Data <=) |  | precondiție: 4426/4427 zero la 30.11 și 31.12; restul clauzelor nu citesc registre |
| 2690 | F21-D9.5 — după operare luna e închisă în REGISTRU: 4423 TVA de plată = 10,5, i… | Citiri.Contabil.Postari (Σ pe cont, Data <=) | 2696: Previzualizeaza decembrie Sold4426/Sold4427 = 0 (cititorul produsului; 4423 fără pereche) | 4423 C 10,5; 4426/4427 zero |
| 2744 | F21-D9.5b — refuzul n-a lăsat rânduri-fantomă (33d) și n-a dublat 4423: la 31.1… | Citiri.Contabil.Postari (DocumentId): zero postări |  | atomicitate |
| 2774 | F21-D9.5b/c — scena restaurată: închiderea lui decembrie a dispărut, noiembrie … | Citiri.Contabil.Postari (Σ pe cont, Data <=) |  | 4426/4427 zero după re-operare |
| 2929 | F21-D9.9 — curățenie finală felia ITV: purjă FIZICĂ, ZERO închideri de TVA răma… | Citiri.Contabil.Postari (Σ pe cont, Data <=) |  | după purjă 4423/4426/4427 zero la 31.12 |

### Privat: Scenariul 1C-a Asamblare

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 3033 | Precondiție ASM: loturile componentelor pe Marfuri (A 10 buc × 5, B 4 buc × 10) | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | precondiție: A 10, B 4; TipStoc Marfuri devine contul 371 |
| 3073 | Stoc: EXACT 3 rânduri (−6/−30 A, −2/−20 B, +2/+50 kit) pe Marfuri, în gestiunea… | Citiri.Loturi.Postari (DocumentId) |  | −6/−30 A, −2/−20 B, +2/+50 kit pe gestiune; numărătoarea (3) și cheia TipStoc cad; contraponderile de transformare nu sunt în Loturi.Postari (Unitate null) |
| 3081 | Lotul kitului finalizat de motor: PretUnitar = valoarea alocată / cantitate (25… | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | kit 2, A 4, B 2; Lot.PretUnitar = 25 nu citește registre |
| 3096 | nume + " — fără rânduri-fantomă în ObjectSpace (33d)", !os.GetObjectsQuery<Regi… | Citiri.Loturi.Postari + Citiri.Contabil.Postari (DocumentId): zero postări |  | atomicitate (9 apeluri) |
| 3162 | Stoc după dezasamblare: kit 1 rămas, loturile componente noi cu 1 buc fiecare | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | kit 1, A2 1, B2 1; Stoc(dez).Count == 3 cade |
| 3171 | Anulare directă (dezasamblarea e frunză) → Draft + registre goale | Citiri.Loturi.Postari (DocumentId): zero postări + Citiri.Loturi.Solduri |  | anulare; kit revine la 2 |
| 3182 | Storno → 3 rânduri inverse (Storno=true) la data stornării; soldurile revin (ki… | Citiri.Loturi.Solduri (laData) + Citiri.Loturi.Postari (DocumentId, Tranzactie.Fel = Storno) |  | Σ cantitate inversă −1; kit 2, A2 0 după storno; numărătorile (6, 3) cad |

### Privat: Scenariul 1C-a ReturFurnizor/ReturClient

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 3329 | Precondiție retururi: lotul original pe Marfuri (10 buc × 10 lei) | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | precondiție: 10 buc |
| 3349 | RLF stoc: UN rând −4 / −40 pe Marfuri, în gestiunea predatoare (regula +1 × lin… | Citiri.Loturi.Postari (DocumentId) + Citiri.Loturi.Solduri |  | −4/−40 pe gestiune; sold 6; numărătoarea și cheia TipStoc cad |
| 3355 | RLF note: 371 = 401 cu −40 și 4426 = 401 cu −8.4, pe corespondența ORIGINALĂ și… | Citiri.Contabil.Postari (DocumentId) |  | 371 D −40 / 401 C; 4426 D −8,4 / 401 C, pe corespondența originală (declarantul păstrează semnul); „fără flag Storno" devine Tranzactie.Fel = Operare; numărătoarea cade |
| 3362 | TVA deductibilă scade cu 8.4 (soldul 4426 după retur) | Citiri.Contabil.Postari (Σ pe cont) |  | 4426 scade cu 8,4 față de soldul inițial |
| 3375 | Retur refuzat — fără rânduri-fantomă (33d) | Citiri.Loturi.Postari + Citiri.Contabil.Postari (DocumentId): zero postări |  | atomicitate |
| 3405 | RDC stoc: UN rând +3 / +30 pe Marfuri, în gestiunea primitoare — marfa REVINE p… | Citiri.Loturi.Postari (DocumentId) + Citiri.Loturi.Solduri |  | +3/+30 pe lotul original; sold 9; numărătoarea și cheia TipStoc cad |
| 3411 | RDC note: 4111 = 707 cu −100, 4111 = 4427 cu −21, 607 = 371 cu −30 — toate fără… | Citiri.Contabil.Postari (DocumentId) |  | 4111 D −100 / 707 C; 4111 D −21 / 4427 C; 607 D −30 / 371 C; Fel = Operare; numărătoarea cade |
| 3431 | REGRESIE D1 (felia 11): linia de COST a returului nu produce fapt fiscal — `Pre… | Citiri.Fiscale.Fapte (DocumentId) |  | un singur fapt, pe linia de venit: Baza −100, Tva −21, Livrare |
| 3440 | TVA colectată (sold CREDITOR) scade cu 21 după retur | Citiri.Contabil.Postari (Σ pe cont) |  | 4427 creditor scade cu 21 |
| 3446 | Anulare directă RDC (frunză) → Draft, registre goale, stocul revine la 6 | Citiri.Loturi.Postari + Citiri.Contabil.Postari (DocumentId): zero postări; Citiri.Loturi.Solduri |  | anulare; stocul revine la 6 |
| 3463 | nume + " — fără rânduri-fantomă în ObjectSpace (33d)", !os.GetObjectsQuery<Regi… | Citiri.Loturi.Postari + Citiri.Contabil.Postari (DocumentId): zero postări |  | atomicitate (apelurile RefuzRet) |
| 3608 | Storno pe RETUR: rândurile inverse sunt POZITIVE și poartă Storno=true (flag-ul… | Citiri.Loturi.Postari + Citiri.Contabil.Postari (DocumentId, Tranzactie.Fel = Storno) |  | inversele pozitive: +4/+40 la data stornării; 371 D 40, 4426 D 8,4; numărătorile cad |
| 3615 | Stocul revine după stornarea returului (10 − 4 + 3 + 4 = 13); TVA deductibilă r… | Citiri.Loturi.Solduri (laData) + Citiri.Contabil.Postari (Σ pe cont) |  | stoc 13; 4426 revine la soldul inițial |
| 3650 | F13-D1 RLF + TI21: pe latura DEDUCTIBIL (achiziția stornată) autolichidarea RĂM… | Citiri.Contabil.Postari (DocumentId) |  | 371 D −20 / 401 C; 4426 D −4,2 / 4427 C; Fel = Operare; numărătoarea cade |

### Privat: 104c + Api DEC override TVA

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 3820 | Api DEC privat operat: nota principală (cheltuiala = 542, 200 net) + rândul de … | Citiri.Contabil.Postari (DocumentId) |  | cheltuiala D 200 / 542 C; 4426 D 42 / 542 C; numărătoarea cade; clauza DecontApply.Citeste rămâne |

### Privat: Felia Api FCL

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 4102 | Dry-run-ul nu materializează nimic: documentul rămâne Draft, fără număr și fără… | Citiri.Contabil.Postari (DocumentId): zero postări |  | atomicitate (dry-run); clauza DTO rămâne |
| 4125 | Aceeași cale de postare ca în UI: 4111 = 707 pe marfă (140), 4111 = 704 pe serv… | Citiri.Contabil.Postari (DocumentId) |  | 707 C 140; 704 C 100; 4427 C 50,4; toate pe 4111 D; numărătoarea (6) cade |
| 4218 | Costul se postează pe DSC, decuplat de vânzare: 607 = 371 la 38 (18+20), în tim… | Citiri.Contabil.Postari (DocumentId) |  | 607 D / 371 C, Σ 38; numărătoarea cade |
| 4223 | Operarea DSC scoate marfa din gestiune: −3 pe lotul pin, −4 pe lotul vechi, Mar… | Citiri.Loturi.Postari (DocumentId) |  | −3 lot pin, −4 lot vechi, pe gestiunea de descărcare; numărătoarea și cheia TipStoc cad |
| 4361 | Anularea descărcării o readuce pe Draft, îi șterge rândurile de stoc și ELIBERE… | Citiri.Loturi.Postari (DocumentId): zero postări |  | anulare; clauzele DTO rămân |

### Bugetar: Scenariul e2e 3b

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 4549 | Operare → −4/−40 pe MAG1 | Citiri.Loturi.Postari (DocumentId) | 4556: solduri MAG1/MAG2 | −4/−40 pe MAG1 la data documentului; BTR e Transfer pe cub, sursa postează Debit −40 cu cantitate −4, deci aceeași cifră semnată; „!Storno" devine Tranzactie.Fel = Transfer |
| 4551 | Operare → +4/+40 pe MAG2 | Citiri.Loturi.Postari (DocumentId) | 4556: solduri MAG1/MAG2 | +4/+40 pe MAG2 |
| 4556 | Solduri după transfer: MAG1=6, MAG2=4 | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | MAG1 6, MAG2 4 |
| 4614 | BTR2 operat (MAG2 golit) | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | MAG2 0, MAG1 10 |
| 4621 | Anulare BTR2 → Draft + rânduri șterse | Citiri.Loturi.Postari (DocumentId): zero postări |  | anulare |
| 4623 | Solduri revenite: MAG1=6, MAG2=4 | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | MAG1 6, MAG2 4 |
| 4638 | Storno → solduri nete: MAG1=10, MAG2=0 | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | MAG1 10, MAG2 0 după storno |

### Bugetar: spike 1 felia BTR

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 4763 | Dry-run NU materializează nimic: zero rânduri de registru, număr neconsumat, st… | Citiri.Loturi.Postari + Citiri.Contabil.Postari (DocumentId): zero postări |  | atomicitate (dry-run); numărul și starea rămân |
| 4826 | ComenziDocument.AnuleazaOperarea → Draft, registrele proprii șterse, affordance… | Citiri.Loturi.Postari (DocumentId): zero postări |  | anulare |

### Bugetar: Scenariul 3c FCT -> NIR

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 4977 | FCT contează DOAR linia de serviciu: 628 = 401, 100 | Citiri.Contabil.Postari (DocumentId, LinieId) |  | 628 D 100 / 401 C 100 pe linia de serviciu; „doar linia de serviciu" devine filtrul pe linie (pe cub FCT poartă și recepția, TR-D3); numărătoarea cade |
| 4999 | NIR → +5/+59,5 Magazie pe gestiunea primitoare | Citiri.Loturi.Postari (DocumentId = FCT) |  | +5/+59,5 pe MAG1, lotul facturii; TR-D3; numărătoarea și cheia TipStoc cad |
| 5003 | NIR contează recepția: 302.01.00 = 401, 59,5 | Citiri.Contabil.Postari (DocumentId = FCT, LinieId) |  | 302 D 59,5 / 401 C; TR-D3; numărătoarea cade |
| 5006 | Nota NIR: Materialul implicit din lot (produsul) pe ambele laturi (3d) | Citiri.Contabil.Postari (DocumentId = FCT): Produs |  | Material din lot pe ambele picioare = Produs pe postări (așteptarea oracolului de la 5059–5062 îl pune pe ambele); TR-D3 |
| 5009 | Sold lot după recepție: 5 pe MAG1 | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | 5 pe MAG1 |
| 5095 | NIR anulat (corecție directă): Draft, fără rânduri | Citiri.Loturi.Solduri (LotId): fără sold |  | anulare; pe cub recepția e pe FCT (TR-D3), deci filtrul pe NIR ar fi vid și înainte — faptul e „lotul nu mai are stoc" |
| 5108 | Storno NIR → sold lot 0 | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | 0 după storno |
| 5112 | Storno FCT → nota serviciului inversată, append-only | Citiri.Contabil.Postari (DocumentId, Tranzactie.Fel = Storno) |  | −100 pe nota serviciului; numărătoarea cade |
| 5159 | Creditul vine din ContImplicit al partenerului (404, nu fallback 401) | Citiri.Contabil.Postari (DocumentId) |  | creditul pe 404 (ContImplicit al partenerului) |
| 5161 | Puntea angajamentului: E pe 404 satisfăcut fără cod economic; B/F/P rezolvate p… | Citiri.Contabil.Jurnal (DocumentId) |  | analiza piciorului 404 C: fără cod economic, cu sursă, cod funcțional, proiect (B-D8 pct. 8) |

### Bugetar: felia 2 Api FCT + NIR

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 5432 | FCT postează DOAR linia de serviciu: 628 = 401, 121 (brut capitalizat) | Citiri.Contabil.Postari (DocumentId, LinieId) |  | 628 D / 401 C 121 pe linia de serviciu (pe cub brutul capitalizat = bază + taxă pe același cont, Σ 121; FCT poartă și recepția, TR-D3); numărătoarea cade |
| 5468 | NIR → +5/+59,5 Magazie pe gestiunea primitoare, pe lotul născut la culegerea fa… | Citiri.Loturi.Postari (DocumentId = FCT) |  | +5/+59,5 pe MAG1, lotul născut la culegere; TR-D3; numărătoarea și cheia TipStoc cad |
| 5472 | NIR contează recepția: 302.01.00 = 401, 59,5 | Citiri.Contabil.Postari (DocumentId = FCT, LinieId) |  | 302 D 59,5 / 401 C; TR-D3; numărătoarea cade |
| 5489 | NIR anulat: Draft, registrele proprii șterse — dar RĂMÂNE autogenerat | Citiri.Loturi.Solduri (LotId): fără sold |  | anulare; pe cub recepția e pe FCT (TR-D3); clauzele DTO rămân |

### Bugetar: Scenariul 3c BonConsum

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 5636 | Operare → −4/−40 Magazie pe MAG1 | Citiri.Loturi.Postari (DocumentId) | 5640: solduri | C 302 pe MAG1: −4 / 40; cheia TipStoc.Magazie devine contul 302 |
| 5638 | Operare → +4/+40 Consum pe locul de consum | Citiri.Loturi.Postari (DocumentId) | 5640: solduri | D 6xx pe locul de consum: +4 / 40 (TR-D4: rândul de Consum e unificat cu piciorul 6xx; cheia TipStoc.Consum devine contul de cheltuială) |
| 5640 | Solduri: Magazie MAG1=6, Consum loc=4 | Citiri.Loturi.Solduri (LotId, GestiuneId, ContId) |  | MAG1 6 pe 302; locul de consum 4 pe 6xx |
| 5643 | Contare: 602.01.00 = 302.01.00 (creditul din contul Tipului), 40 | Citiri.Contabil.Postari (DocumentId) |  | 602 D 40 / 302 C 40; numărătoarea cade |
| 5724 | Anulare BCS → Draft + solduri revenite (Magazie 10, Consum 0) | Citiri.Loturi.Solduri (LotId, GestiuneId, ContId) |  | anulare: Magazie 10, consum 0 |
| 5732 | Storno BCS → 4 rânduri stoc (2 + 2 inverse) și nota inversată | Citiri.Contabil.Postari (DocumentId, Tranzactie.Fel = Storno) | 5737: soldurile nete | nota inversată −40; numărătorile de rânduri de stoc (4, 2 cu Storno) cad |
| 5737 | Storno → solduri nete: Magazie 10, Consum 0 | Citiri.Loturi.Solduri (LotId, GestiuneId, ContId) |  | Magazie 10, consum 0 după storno |

### Bugetar: Scenariul 3c ListaDiferenteInventar

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 5866 | Operare → 2 rânduri de stoc, ambele Magazie pe gestiunea inventariată | Citiri.Loturi.Postari (DocumentId) |  | ambele mișcări pe gestiunea inventariată; numărătoarea (2) și cheia TipStoc cad |
| 5868 | Minus → −2/−20 pe lotul vechi | Citiri.Loturi.Postari (DocumentId) | 5872: solduri | −2/−20 pe lotul vechi |
| 5870 | Plus → +3/+21 pe lotul nou | Citiri.Loturi.Postari (DocumentId) | 5872: solduri | +3/+21 pe lotul nou |
| 5872 | Solduri: lot vechi 8, lot nou 3 | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | vechi 8, nou 3 |
| 5874 | Contare minus: 602.01.00 = 302.01.00, POZITIVĂ (normalizată cu semnul filtrului) | Citiri.Contabil.Postari (DocumentId) |  | 602 D 20 / 302 C 20, pozitivă |
| 5877 | Contare plus: 302.01.00 = 791.00.00, 21 | Citiri.Contabil.Postari (DocumentId) |  | 302 D 21 / 791 C 21 |
| 5900 | Anulare LDI → Draft + solduri revenite (vechi 10, nou 0) | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | anulare: vechi 10, nou 0 |
| 5909 | Storno LDI → 4 rânduri stoc (2 + 2 inverse) și notele inversate (−20, −21) | Citiri.Contabil.Postari (DocumentId, Tranzactie.Fel = Storno) | 5913: soldurile nete | notele inversate −20 și −21; numărătorile de rânduri de stoc cad |
| 5913 | Storno → solduri nete: vechi 10, nou 0 | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | vechi 10, nou 0 după storno |

### Bugetar: Scenariul 3c FacturaIesire

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 6022 | FCL nu mișcă stoc | Citiri.Loturi.Postari (DocumentId): zero postări |  | absenta: „FCL nu mișcă stoc" |
| 6025 | Contare servicii: 411.01.01 = 751.01.00, 200 | Citiri.Contabil.Postari (DocumentId) |  | 411 D / 751 C 200 |
| 6028 | Contare chirie: 411.01.01 = 750.02.00, 50 | Citiri.Contabil.Postari (DocumentId) |  | 411 D / 750 C 50 |
| 6049 | Debitul din ContImplicit al clientului (461, nu fallback 411) | Citiri.Contabil.Postari (DocumentId) |  | debitul pe 461 (ContImplicit al clientului) |
| 6051 | Valoarea postată include TVA (o singură Valoare pe linie: 119) | Citiri.Contabil.Postari (DocumentId) |  | Σ 119 pe 461 D (pe cub capitalizatul e bază + taxă pe același cont, B-D8 pct. 5 — „o singură valoare" devine Σ pe cont) |
| 6059 | Anulare FCL → Draft + notele șterse | Citiri.Contabil.Postari (DocumentId): zero postări |  | anulare |
| 6066 | Storno FCL → note inverse append-only (−200, −50) la data stornării | Citiri.Contabil.Postari (DocumentId, Tranzactie.Fel = Storno) |  | −200 și −50 la data stornării; numărătorile cad |

### Bugetar: Scenariul 3c Plata/Incasare + Imperechere

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 6214 | Plata contează per linie de defalcare: 401 = 770 (59,5 + 100) | Citiri.Contabil.Postari (DocumentId) |  | 401 D / 770 C, Σ 159,5; numărătoarea cade |
| 6217 | Plata nu mișcă stoc | Citiri.Loturi.Postari (DocumentId): zero postări |  | absenta: „plata nu mișcă stoc" |
| 6235 | După ștergerea imperecherii, anularea plății merge (Draft, notele șterse) | Citiri.Contabil.Postari (DocumentId): zero postări |  | anulare |
| 6275 | Contare încasare: 531.01.01 (casa) = 411.01.01 (fallback client), 119 | Citiri.Contabil.Postari (DocumentId) |  | 531 D / 411 C 119; numărătoarea cade |
| 6319 | Contare avans: 542.01.00 (ContImplicit angajat) = 531.01.01 (casa), 50 | Citiri.Contabil.Postari (DocumentId) | 6329: SC-DEC-16 (contract: 542 D 50 / 531 C 50) | 542 D / 531 C 50; numărătoarea cade |
| 6346 | Storno încasare → nota inversată append-only (−119) la data stornării | Citiri.Contabil.Postari (DocumentId, Tranzactie.Fel = Storno) |  | −119; numărătoarea cade |
| 6407 | 82: refuzul stingerii NU persistă operarea, numărul sau registrele/relația | Citiri.Contabil.Postari + Citiri.Loturi.Postari + Citiri.Fiscale.Postari (DocumentId): zero postări |  | atomicitate; clauzele de stare, număr și Imperechere rămân |

### Bugetar: Felia Api Trz

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 6600 | PLT contează din laturi: 401.01.00 (fallback furnizor) = 531.01.01 (ContImplici… | Citiri.Contabil.Postari (DocumentId) |  | 401 D / 531 C 150; numărătoarea cade |
| 6603 | PLT nu mișcă stoc | Citiri.Loturi.Postari (DocumentId): zero postări |  | absenta: „PLT nu mișcă stoc" |
| 6638 | Apply<Incasare> + operare: număr din seria proprie (INC-), contare oglindită 53… | Citiri.Contabil.Postari (DocumentId) |  | 531 D / 411 C 80; numărătoarea cade; clauzele DTO rămân |
| 6725 | Plata autogenerată operată: numărul rămâne CEL CULES pe factură (AsignaNumar on… | Citiri.Contabil.Postari (DocumentId) |  | 401 D / 770 C 121; numărătoarea cade; clauzele DTO rămân |

### Bugetar: felia 7 viramentul intern

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 7077 | F7-D9 ancora 2: niciun refuz n-a lăsat rânduri-fantomă și n-a consumat serie (d… | Citiri.Contabil.Postari + Citiri.Loturi.Postari (DocumentId): zero postări |  | atomicitate (dry-run); clauzele de număr și serie rămân |
| 7118 | F7-D9 ancora 3: piciorul de ieșire postează 581 (tranzit) = 531.01.01 (contul p… | Citiri.Contabil.Postari (DocumentId) |  | 581 D / 531 C 500; fără postări de stoc; numărătoarea cade |
| 7122 | F7-D9 ancora 3 (F7-D5b): dimensiunea Repartitor = contul propriu AL PICIORULUI … | Citiri.Contabil.Postari (DocumentId): Gestiune |  | contul propriu al piciorului pe ambele postări; viramentul n-are partener, deci convenția cubului coincide cu a notei (B-D8 pct. 9, „rândurile fără dimensiune de partener rămân neatinse"; NUC-PLT-VIR) |
| 7175 | F7-D9 ancora 5: piciorul de INTRARE postează 770.00.00 (contul propriu DESTINAȚ… | Citiri.Contabil.Postari (DocumentId) |  | 770 D / 581 C 500 la data piciorului; Gestiune = contul propriu al piciorului pe ambele postări; fără stoc; numărătoarea cade |
| 7196 | F7-D9 ancora 5: EXACT două rânduri de registru contabil pe toată perechea, ZERO… | Citiri.Contabil.Postari (Σ pe cont, pe documentele perechii) |  | 581 se închide la 0; fără postări de stoc; numărătoarea (2 rânduri) cade |
| 7288 | F7-D9 ancora 7: anularea laturii pereche o readuce în Draft, fără rânduri propr… | Citiri.Contabil.Postari (DocumentId): zero postări |  | anulare |
| 7292 | F7-D9 ancora 7: anularea SURSEI șterge draftul autogenerat (gardienii de grup e… | Citiri.Contabil.Postari (DocumentId): zero postări |  | anulare; clauzele DTO rămân |
| 7313 | F7-D9 ancora 7: storno pe AMBELE picioare — câte un rând invers marcat Storno l… | Citiri.Contabil.Postari (DocumentId, Tranzactie.Fel = Storno) |  | inversele −500 la data stornării; 581 Σ 0; numărătorile (4, 2) cad |

### Bugetar: Scenariul 3c Decont

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 7437 | Decontul nu mișcă stoc | Citiri.Loturi.Postari (DocumentId): zero postări |  | absenta: „decontul nu mișcă stoc" |
| 7439 | Contare deplasare: debit din contul Tipului (614) = 542, 30 | Citiri.Contabil.Postari (DocumentId) |  | 614 D / 542 C 30 |
| 7442 | Contare protocol: debitul EXPLICIT al liniei (623 bate Tipul 628) = 542, 23,8 | Citiri.Contabil.Postari (DocumentId) |  | 623 D / 542 C 23,8 |
| 7514 | Anulare decont → Draft + notele șterse | Citiri.Contabil.Postari (DocumentId): zero postări |  | anulare |
| 7521 | Storno decont → note inverse append-only (−30, −23,8) la data stornării | Citiri.Contabil.Postari (DocumentId, Tranzactie.Fel = Storno) |  | −30 și −23,8 la data stornării; numărătorile cad |

### Bugetar: Scenariul 1C-a NotaContabila

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 7624 | Nota nu mișcă stoc | Citiri.Loturi.Postari (DocumentId): zero postări |  | absenta: „nota nu mișcă stoc" |
| 7629 | Conturile = cele EXPLICITE ale liniilor (581.01.01 = 581.01.02; 623 = 581.01.01) | Citiri.Contabil.Postari (DocumentId, LinieId) |  | 581.01.01 D / 581.01.02 C; 623 D / 581.01.01 C |
| 7632 | Valorile se postează CA ATARE, inclusiv negativa (100 / −40), fără flag de stor… | Citiri.Contabil.Postari (DocumentId, LinieId) |  | 100 și −40 ca atare; „fără flag de storno" devine Tranzactie.Fel = Operare |
| 7656 | nume + " — fără rânduri-fantomă în ObjectSpace (33d)", !os.GetObjectsQuery<Regi… | Citiri.Contabil.Postari (DocumentId): zero postări |  | atomicitate (4 apeluri RefuzNtc) |
| 7700 | 628 cu cod economic cules pe linie → operare acceptată, dimensiunea pe latura c… | Citiri.Contabil.Jurnal (DocumentId) |  | codul economic pe postarea 628 D (analiza laturii, B-D8 pct. 8) |
| 7707 | Anulare directă → Draft + registrele goale | Citiri.Contabil.Postari (DocumentId): zero postări |  | anulare |
| 7711 | Re-operare → același număr, aceleași 2 rânduri (100 / −40) | Citiri.Contabil.Postari (DocumentId) |  | Σ 60 (100 − 40) după re-operare; numărătoarea cade |
| 7716 | Storno → rânduri inverse append-only la data stornării (−100 / +40, nota negati… | Citiri.Contabil.Postari (DocumentId, Tranzactie.Fel = Storno) |  | −100 și +40 la data stornării; numărătorile cad |

### Bugetar: felia 5 Api NIR scriere

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 7987 | nume + " — fără rânduri-fantomă în ObjectSpace (33d)", !os.GetObjectsQuery<Regi… | Citiri.Loturi.Postari + Citiri.Contabil.Postari (DocumentId): zero postări |  | atomicitate (3 apeluri RefuzNir) |
| 8012 | Dry-run-ul NU materializează nimic: documentul rămâne Draft, fără registre și f… | Citiri.Loturi.Postari + Citiri.Contabil.Postari (DocumentId): zero postări |  | atomicitate (dry-run); starea și lotul nefinalizat rămân |
| 8030 | NIR manual → +6/+75 Magazie pe gestiunea primitoare, pe lotul propriu | Citiri.Loturi.Postari (DocumentId) |  | +6/+75 pe MAG1, lotul propriu (NIR manual, document propriu pe cub); numărătoarea și cheia TipStoc cad |
| 8035 | NIR manual contează recepția ca oricare alta: 302.01.00 = 401, 75 (regula de op… | Citiri.Contabil.Postari (DocumentId) |  | 302 D 75 / 401 C; numărătoarea cade |
| 8096 | NIR conex operat după PUT: +4/+23,8 pe lotul facturii — gardul de preț (F5-D7b)… | Citiri.Loturi.Postari (DocumentId = FCT) |  | +4/+23,8 pe lotul facturii; TR-D3 + 098/099: pe cub recepția conexă e pe FCT, iar NIR-ul conex postează numai diferența față de recepția facturii — de verificat la re-țintire pe ce document cade recepția parțială |

### Bugetar: felia 6 Api BCS scriere

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 8283 | nume + " — fără rânduri-fantomă în ObjectSpace (33d)", !os.GetObjectsQuery<Regi… | Citiri.Loturi.Postari + Citiri.Contabil.Postari (DocumentId): zero postări |  | atomicitate (4 apeluri RefuzBcs); starea și numărul rămân |
| 8308 | Dry-run-ul NU materializează nimic: documentul rămâne Draft, fără registre și f… | Citiri.Loturi.Postari (DocumentId): zero postări |  | atomicitate (dry-run) |
| 8321 | BCS operat → DOUĂ registre simultan: −6/−60 Magazie pe gestiune, +6/+60 Consum … | Citiri.Loturi.Postari (DocumentId) | 8327: solduri | C 302 MAG1 −6/60; D 6xx loc +6/60; numărătoarea și cheile TipStoc cad |
| 8327 | Solduri după operare: Magazie 14, Consum 6 | Citiri.Loturi.Solduri (LotId, GestiuneId, ContId) |  | Magazie 14; consum 6 |
| 8340 | Anulare prin API → Draft + solduri revenite (Magazie 20, Consum 0) | Citiri.Loturi.Solduri (LotId, GestiuneId, ContId) + Citiri.Loturi.Postari (DocumentId): zero postări |  | anulare: Magazie 20, consum 0 |
| 8345 | Storno prin API → Stornat, 4 rânduri de stoc (2 + 2 inverse), solduri nete reve… | Citiri.Loturi.Solduri (LotId, GestiuneId, ContId) |  | Magazie 20, consum 0 după storno; numărătorile de rânduri (4, 2 cu Storno) cad |

### Bugetar: felia 6 Api LDI scriere

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 8662 | nume + " — fără rânduri-fantomă în ObjectSpace (33d)", !os.GetObjectsQuery<Regi… | Citiri.Loturi.Postari + Citiri.Contabil.Postari (DocumentId): zero postări |  | atomicitate (9 apeluri RefuzLdi); starea și numărul rămân |
| 8741 | Refuzul F6-F1 — fără rânduri-fantomă (33d) | Citiri.Loturi.Postari (DocumentId): zero postări |  | atomicitate |
| 8751 | Dry-run-ul NU materializează nimic: Draft, fără număr, fără registre, lotul plu… | Citiri.Loturi.Postari (DocumentId): zero postări |  | atomicitate (dry-run) |
| 8775 | LDI operat → 2 rânduri, ambele Magazie pe gestiunea INVENTARIATĂ: −2/−20 pe lot… | Citiri.Loturi.Postari (DocumentId) | 8779: solduri | −2/−20 pe lotul vechi, +3/+21 pe lotul nou, pe gestiunea inventariată; numărătoarea și cheia TipStoc cad |
| 8779 | Solduri după operare: lot vechi 8, lot nou 3 | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | vechi 8, nou 3 |
| 8781 | Contare pe direcție (SemnFiltru): două note — minusul POZITIV (normalizat), plu… | Citiri.Contabil.Postari (DocumentId) |  | minusul pozitiv 20 și plusul 21; numărătoarea (2 note) cade |
| 8791 | Anulare prin API → Draft + solduri revenite (vechi 10, nou 0) | Citiri.Loturi.Solduri (LotId, GestiuneId) + Citiri.Loturi.Postari (DocumentId): zero postări |  | anulare: vechi 10, nou 0 |
| 8799 | Storno prin API → Stornat, 4 rânduri de stoc (2 + 2 inverse), solduri nete reve… | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | vechi 10, nou 0 după storno; numărătorile de rânduri (4, 2 cu Storno) cad |

### Bugetar: Felia Api DEC

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 9119 | nume + " — fără rânduri-fantomă și fără număr consumat (33d + GATE D6)", !os.Ge… | Citiri.Contabil.Postari + Citiri.Loturi.Postari (DocumentId): zero postări |  | atomicitate (5 apeluri RefuzDec); starea și numărul rămân |
| 9153 | Dry-run-ul NU materializează nimic: Draft, fără număr, fără note | Citiri.Contabil.Postari (DocumentId): zero postări |  | atomicitate (dry-run) |
| 9165 | Decontul nu mișcă stoc | Citiri.Loturi.Postari (DocumentId): zero postări |  | absenta: „decontul nu mișcă stoc" |
| 9170 | Contare (2 note, una per linie): debitul din contul Tipului (614) pe linia fără… | Citiri.Contabil.Postari (DocumentId, LinieId) |  | 614 D 30 / 542 C; 542 C pe ambele linii; numărătoarea cade |
| 9175 | ANCORA F8-D13.2: contul CULES pe linie (623) BATE rezolvarea declarativă (Sursa… | Citiri.Contabil.Postari (DocumentId, LinieId) |  | 623 D 24,2 (contul cules bate rezolvarea declarativă) |
| 9217 | Anulare prin API → Draft + notele șterse | Citiri.Contabil.Postari (DocumentId): zero postări |  | anulare |
| 9225 | Storno prin API → Stornat, note inverse append-only (−30, −24,2) la data stornă… | Citiri.Contabil.Postari (DocumentId, Tranzactie.Fel = Storno) |  | −30 și −24,2 la data stornării; numărătorile cad |

### Bugetar: felia 8 pas 3 pereche prin API (E2E-APER)

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 9379 | ANCORA F8-D13.5 (nonregresie F7): latura pereche operată NU generează un al tre… | Citiri.Contabil.Postari (Σ pe cont, pe documentele perechii) |  | 581 se închide la 0; restul clauzelor nu citesc registre |
| 9480 | ANCORA F8-D13.1a: piciorul cules CU link nu generează nimic la rândul lui; pere… | Citiri.Contabil.Postari (Σ pe cont, pe documentele perechii) |  | 581 la 0; numărătoarea (2 rânduri) cade |
| 9589 | ANCORA F8-D13.2: niciunul dintre cele șapte refuzuri n-a lăsat rânduri-fantomă … | Citiri.Contabil.Postari + Citiri.Loturi.Postari (DocumentId): zero postări |  | atomicitate (dry-run); numărul și seria rămân |
| 9728 | ANCORA D1-B: perechea regenerată se operează (pointer-ul stornat nu mai blochea… | Citiri.Contabil.Postari (Σ pe cont, pe cele trei documente) |  | 581 la 0 și după storno + regenerare |
| 9762 | ANCORA D1 (PerecheActiva pe OPERAT): perechea operată e activă, 581 se închide … | Citiri.Contabil.Postari (Σ pe cont, pe documentele perechii) |  | 581 la 0 |

### VerificaRegistruTva

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 10151 | JT-D4: cele patru regimuri pe un singur document → patru rânduri fiscale, câte … | Citiri.Fiscale.Fapte (DocumentId, DetaliuId) |  | un fapt per linie: Normal 100/21, TaxareInversa 50/10,5, Scutit 70/0, Capitalizat 82,64/17,36 (Tva = rolul Taxă; autolichidarea e separat în Autocolectare) |
| 10158 | JT-D1/JT-D3: sensul vine din direcția politicii (FCT deduce → Achiziție), parte… | Citiri.Fiscale.Fapte (DocumentId) |  | Sens = Achiziție, PartenerId = furnizorul, Storno fals; data rândului fiscal = DataDocument pe FaptFiscal (B-D8 pct. 5) |
| 10166 | MOTIVUL DE EXISTENȚĂ al registrului (design, „de ce nu o proiecție peste Regist… | Citiri.Fiscale.Fapte + Citiri.Contabil.Postari (DocumentId, LinieId) |  | liniile Scutit și Capitalizat au fapt fiscal fără postare pe conturile de TVA; numărătoarea (2 note de TVA) cade — pe cub taxarea inversă are două postări pe conturi de TVA |
| 10172 | JT-D4, Capitalizat: se rotunjește BAZA, iar TVA-ul e DIFERENȚA — deci Baza + Tv… | Citiri.Fiscale.Fapte (DocumentId, DetaliuId) |  | Capitalizat: Baza + Tva = valoarea brută a liniei |
| 10185 | JT-D2, cealaltă jumătate a criteriului: NIR-ul conex CLONEAZĂ TipTvaId (ca info… | Citiri.Fiscale.Fapte (DocumentId) |  | absenta: NIR-ul conex n-are fapt fiscal, factura are unul |
| 10204 | Premisa contopirii: patru linii, dintre care DOUĂ cu același TipTva (N21) — reg… | Citiri.Fiscale.Fapte (DocumentId) |  | patru fapte (unul per linie), două cu același TipTva — premisa contopirii din jurnal |
| 10235 | Cealaltă latură: o LIVRARE cu același TipTva (N21) produce un rând fiscal cu Se… | Citiri.Fiscale.Fapte (DocumentId) |  | Livrare, Baza 200, Tva 42, partenerul clientului, N21 |
| 10272 | Premisa verificării 2, construită explicit: pe profilul bugetar o factură cu Ti… | Citiri.Fiscale.Fapte (DocumentId): zero |  | absenta (bugetar): factura cu TipTva pe linie nu produce fapt fiscal; restul clauzelor rămân |
| 10343 | VERIFICAREA 2: profilul bugetar n-are NICIUN rând fiscal, deși premisa are dinț… | Citiri.Fiscale.Fapte: zero pe toată baza |  | absenta (bugetar): niciun fapt fiscal; clauzele de politică rămân |
| 10393 | VERIFICAREA 3: găurile declarate sunt numărabile și coerente (liniile fără TipT… | Citiri.Fiscale.Fapte |  | fiecare fapt cu partener îl are printre laturile documentului; numărătorile de „găuri" se fac pe fapte |
| 10405 | JT-D5, storno: patru rânduri inverse la DATA STORNĂRII (Storno = true, bază/TVA… | Citiri.Fiscale.Fapte (DocumentId) |  | storno: patru fapte inverse (Storno adevărat) la data stornării, fiecare cu originalul lui pe aceeași identitate fiscală; Σ Baza = Σ Tva = 0 |
| 10459 | JT-D5, anulare: corecția directă șterge rândurile fiscale odată cu celelalte do… | Citiri.Fiscale.Fapte + Citiri.Contabil.Postari (DocumentId): zero |  | anulare |
| 10500 | VERIFICAREA 4 (JT-D7): pe fiecare sens, jurnalul == registrul — Σ bază și Σ TVA… | Citiri.Fiscale.Fapte (pe perioadă, pe sens) |  | azi e egalitate proiecție pe cub (TvaProiectii.JurnalTva) = registru; re-țintită, sursa brută devine Fiscale.Fapte pe aceeași perioadă (Σ bază, Σ TVA, numărul de (document, TipTva)); filtrul pe r.Data devine DataDocument / perioada de declarare |
| 10543 | VERIFICAREA 6 (JT-D7): decontul == jurnalul, pe aceeași perioadă și pe ambele s… | Citiri.Fiscale.Fapte (pe perioadă) |  | numai clauza decont.Sum(Randuri) == numărul de rânduri brute ale lunii; restul compară două proiecții ale produsului |

### VerificaSaft

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 12023 | D16-V2 premisă: luna scenei e goală înainte de scenă (D300/D394 și-au purjat do… | Citiri.Contabil.Jurnal (deLa, panaLa) + Citiri.Fiscale.IntreLuni: zero |  | premisă de scenă: luna e goală înaintea scenei (zero postări cu document, zero fapte fiscale) |
| 12686 | D16-V2 cusătura 3 (facturi): pe FIECARE sens, Σ bazei rândurilor fiscale AȘEZAT… | Citiri.Fiscale.IntreLuni (Citiri.Fiscale.Fapte) |  | numai clauzele cu bazaFaraSectiune: Σ Baza pe documentele fără secțiune de facturi = 60, citită independent de proiecție; restul sunt câmpuri ale rezumatului SAF-T (deja pe cub) |

### VerificaSaftStocuri

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 13650 | D17-V2 premisă: luna scenei e goală de mișcări de stoc înainte de scenă (2027 e… | Citiri.Loturi.Postari (în lună, cu document): zero |  | premisă de scenă: luna e goală de mișcări de stoc |
| 13860 | D17/ASM: lipsa contului refuză atomic operarea; degradarea nomenclatorului se p… | Citiri.Loturi.Postari (DocumentId): zero postări | 13860: aceeași aserție are deja clauza pe Postare | atomicitate; clauza RegistruStoc doar se șterge |
| 14326 | Curățenie finală felia SAF-T S (fără reziduuri e2e: repartitori, documente, lot… | Citiri.Loturi.Postari (în lună): zero |  | numai clauza de reziduu „rânduri de stoc în lună" (FaraReziduu la 14311–14312); restul curățeniei rămâne |

### VerificaD300

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 15441 | D3-V2/V4 premisă: perioada scenei e goală înainte de scenă (niciun rând fiscal … | Citiri.Fiscale.Fapte (pe perioadă): zero + Citiri.Contabil.Postari (Σ pe cont, Data <=) |  | premisă de scenă: niciun fapt fiscal în perioadă, soldurile 4426/4427 zero; clauza de închideri vii rămâne |
| 15724 | D3-V3 (D3-D4) — NIMIC nu se pierde: Σ rândurilor cu mapări directe + Σ nemapate… | Citiri.Fiscale.Fapte (pe perioadă) |  | azi e egalitate proiecție pe cub (D300Proiectii) = registru; re-țintită, sursa brută devine Fiscale.Fapte pe aceeași perioadă (Σ bază, Σ TVA pe grupuri Sens × TipTva) |
| 15756 | D3-V2/V3 (F13-D1) taxarea inversă pe LIVRARE: rândul de registru poartă bază 30… | Citiri.Fiscale.Fapte (pe perioadă) |  | un singur fapt TaxareInversa/Livrare: Baza 300, Tva 0; clauzele pe rd. 13 rămân |
| 15977 | Curățenie finală felia D300 (fără reziduuri e2e — inclusiv nota de închidere, a… | Citiri.Fiscale.Fapte (pe perioadă): zero |  | numai clauza „niciun rând fiscal în perioadă" după purjă; restul curățeniei rămâne |

### VerificaD394

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 16088 | D4-V2 premisă: luna scenei e goală înainte de scenă — altfel cifrele exacte și … | Citiri.Fiscale.Fapte (pe perioadă): zero |  | premisă de scenă |
| 16146 | D4-V2 (bugetar): profilul neplătitor n-are `PoliticaTva` ⇒ `RegistruTva` gol ⇒ … | Citiri.Fiscale.Fapte: zero pe document și pe toată baza |  | absenta (bugetar); clauzele pe proiecția D394 rămân |
| 16431 | D4-V3 storno: rândurile inverse ale lui C1 stau în a doua jumătate a lunii, cu … | Citiri.Fiscale.Fapte (Storno, partener, Livrare, cota 21) |  | un fapt invers în a doua jumătate a lunii: Baza −400, Tva −84, perioada lunii; filtrul pe r.Data devine data tranzacției de storno; clauzele pe proiecție rămân |
| 16461 | D4-V4 (D4-D4) — NIMIC nu se pierde: Σ `Operatiuni` + Σ `Neincluse` == Σ `Regist… | Citiri.Fiscale.Fapte (pe perioadă, pe sens) |  | azi e egalitate proiecție pe cub (D394Proiectii) = registru; sursa brută devine Fiscale.Fapte |
| 16593 | Fix 6 (review advers, 104f): partenerul INACTIV rămâne în op1 pe tip 1 | Citiri.Fiscale.Fapte (pe perioadă, pe sens) |  | numai clauzele CusaturaS (Σ op + Σ neincluse = Σ brut); restul e pe proiecție |
| 16666 | Curățenie finală felia D394 (fără reziduuri e2e: repartitori, documente, lot, p… | Citiri.Fiscale.Fapte (pe perioadă): zero |  | numai clauza „niciun rând fiscal în perioadă" după purjă |

### VerificaValoareIesire

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 16905 | D18-V2 (a) contarea consumului (6xx = 3xx) postează valoarea materializată, 10,… | Citiri.Contabil.Postari (DocumentId) |  | 6xx D / 3xx C 10,00 (postarea = valoarea liniei); numărătoarea cade |
| 16931 | D18-V2 (a) stornoul liniei absorbante = rânduri inverse identice (−(−10,00) pe … | Citiri.Loturi.Postari (DocumentId, Tranzactie.Fel = Storno) + Citiri.Loturi.Solduri |  | inversele exacte ale mișcărilor de 10,00; lotul revine la 1/10,00; numărătorile și cheia TipStoc cad |
| 17040 | D18-V2 (r, pe cub, S3-D4) SAF-T S nu ascunde reziduul retro: intrarea 0→0 bucăț… | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | valoarea reziduului retro (soldRetro) cu care se compară intrarea SAF-T S; azi e egalitate proiecție pe cub = registru |
| 17102 | D18-V2 (g) returul la furnizor care golește lotul rămâne la `preț × cantitate` … | Citiri.Loturi.Solduri (LotId, GestiuneId) + Citiri.Contabil.Postari (DocumentId) | catalog SC-RLF-05 (ScenariiRlf), de confirmat acoperirea | RLF e sursa (b): linia −10,01 nu se schimbă; lotul rămâne 0 / −0,01; 401 C −10,01 |
| 17157 | D18-V2 `ReziduValoricFaraCantitate` pe cheile golite de documente ale scenei = … | Citiri.Loturi.Solduri (pe loturile scenei) |  | cheile cu cantitate 0 și valoare ≠ 0 = exact limitele declarate (retro; RLF fiscal pe privat); cheia (Lot, Repartitor, TipStoc) devine (Lot, Gestiune, Cont); de confirmat sub dual că reziduurile cubului coincid (T-r13) |

### VerificaApiNtc

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 17461 | nume + " — fără rânduri-fantomă și fără număr consumat (33d + GATE D6)", !os.Ge… | Citiri.Contabil.Postari + Citiri.Loturi.Postari (DocumentId): zero postări |  | atomicitate (apelurile RefuzOperare); starea și numărul rămân |
| 17494 | Api NTC: dry-run-ul NU materializează nimic (Draft, fără număr, fără note) | Citiri.Contabil.Postari (DocumentId): zero postări |  | atomicitate (dry-run) |
| 17507 | Api NTC: nota nu mișcă stoc și nu scrie jurnal de TVA (fără `PoliticaTva` în ni… | Citiri.Loturi.Postari + Citiri.Fiscale.Postari (DocumentId): zero |  | absenta: nota nu mișcă stoc și nu produce fapt fiscal |
| 17515 | ANCORA F19-D14 (NTC): operarea prin API postează pe POSTAREA EXPLICITĂ a liniei… | Citiri.Contabil.Postari (DocumentId, LinieId) |  | 4111 D / 401 C; 4111 D / 581 C; 581 D / 4111 C pe liniile lor; numărătoarea (3) cade |
| 17521 | Api NTC: valorile se postează CA ATARE, inclusiv negativa (60 / 25 / −10), fără… | Citiri.Contabil.Postari (DocumentId, LinieId) |  | 60, 25, −10 ca atare; „fără flag de storno" devine Tranzactie.Fel = Operare |
| 17963 | Api NTC: anulare prin API → Draft + notele șterse | Citiri.Contabil.Postari (DocumentId): zero postări |  | anulare |
| 17967 | Api NTC: re-operare după anulare — același număr, aceleași trei rânduri (valori… | Citiri.Contabil.Postari (DocumentId) |  | Σ 75 după re-operare; numărătoarea (3) cade; clauzele DTO rămân |
| 17971 | Api NTC: storno prin API → Stornat, rânduri inverse append-only la data stornăr… | Citiri.Contabil.Postari (DocumentId, Tranzactie.Fel = Storno) |  | −60, −25, +10 la data stornării; numărătorile (6, 3) cad |

### VerificaApiAsm

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 18130 | Api ASM premisă: lotul „cuminte” de 10 × 5,00 se naște prin NIR operat, în gest… | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | premisă: 10 / 50,00 |
| 18438 | Api ASM (riscul 3, MĂSURAT): coerența Tip↔Produs nu e păzită la CULEGERE — lotu… | Citiri.Loturi.Postari (Unitate = lot): zero |  | absenta: lotul născut pe draft n-are mișcări; restul clauzelor rămân |
| 18471 | Api ASM (D18-V2 replicat): lotul „cu rest” are prețul 10,006667 și soldul 1 buc… | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | premisă: lotul cu rest 1 / 10,00 |
| 18537 | Api ASM: operarea SEMNEAZĂ cantitățile (consum −1, produs +1 — 28a), consumă se… | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | numai clauza de sold a lotului kit (1 / 10,00); cantitățile semnate, seria și prețul lotului nu citesc registre |
| 18587 | Api ASM: …și operează, cu ambele loturi finalizate la prețurile distribuite | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | clauza de sold 0/0 a lotului consumat; starea și prețul lotului rămân |
| 18669 | Api ASM: anulare prin API → Draft, rândurile de stoc dispar, lotul consumat își… | Citiri.Loturi.Postari (DocumentId): zero postări + Citiri.Loturi.Solduri |  | anulare: lotul consumat revine la 1 / 10,00 |
| 18681 | Api ASM: storno prin API → Stornat, rânduri INVERSE append-only la data stornăr… | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | lotul revine la 1 / 10,00 după storno; numărătoarea rândurilor cu Storno (2) cade |

### VerificaApiRlf

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 18849 | Api RLF premisă: lotul de 10 × 10,00 se naște prin NIR operat, în gestiunea ret… | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | premisă: 10 / 100,00 |
| 19004 | Api RLF: refuzul n-a lăsat rânduri-fantomă (33d) | Citiri.Loturi.Postari + Citiri.Contabil.Postari (DocumentId): zero postări |  | atomicitate |
| 19060 | Api RLF: stoc −4 / −40,00 în gestiunea PREDATOARE (regula +1 × linia negativă),… | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | soldul lotului 6 / 60,00 după retur; numărătoarea (un rând) cade |
| 19065 | Api RLF: note pe corespondența ORIGINALĂ, negative — `371 = 401` cu −40,00 și `… | Citiri.Contabil.Postari (DocumentId) |  | 371 D −40 / 401 C; 4426 D −8,40 / 401 C, pe corespondența originală; Fel = Operare; numărătoarea cade |
| 19082 | Api RLF premisă (scena golirii): ambele loturi „strâmbe” au preț 10,003333 și a… | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | premisă: ambele loturi la 1 / 10,01; prețurile loturilor nu citesc registre |
| 19108 | ANCORA F18/F5 (`IDocumentCuIesireFiscala`, replicată pe calea API): returul GOL… | Citiri.Loturi.Solduri (LotId, GestiuneId) + Citiri.Contabil.Postari (DocumentId) | catalog SC-RLF-05 (ScenariiRlf), de confirmat acoperirea | RLF e sursa (b): lotul rămâne 0 / 0,01; 371 D −10,00 / 401 C; 4426 D −2,10 / 401 C; Σ către furnizor −12,10; numărătoarea cade |
| 19116 | ANCORA F18/F5 (contrastul care face cifra să însemne ceva): pe lotul GEAMĂN, în… | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | lotul geamăn golit de o ieșire evaluată din sold rămâne 0 / 0 |
| 19128 | Api RLF: anularea readuce Draft-ul, șterge registrele și întoarce soldul lotulu… | Citiri.Loturi.Postari + Citiri.Contabil.Postari (DocumentId): zero postări; Citiri.Loturi.Solduri |  | anulare: lotul revine la 10 / 100,00; clauzele DTO rămân |
| 19148 | RISCUL 6: re-operarea după anulare + PUT dă EXACT aceleași cifre și același num… | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | numai clauzele Note(idRlf).Count (cade) și soldul 6 / 60,00; cifrele liniei (sursa b) rămân |
| 19157 | Api RLF: storno prin API → Stornat, rânduri INVERSE append-only (POZITIVE, cu f… | Citiri.Contabil.Postari (DocumentId, Tranzactie.Fel = Storno) + Citiri.Loturi.Solduri |  | inversa 371 D +40; lotul revine la 10 / 100,00; numărătorile rândurilor cu Storno cad |

### VerificaApiRdc

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 19304 | Api RDC premisă: lotul original de 10 × 10,00 (livrarea care se stornează) e pe… | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | premisă: 10 / 100,00 |
| 19463 | Api RDC: refuzul n-a lăsat rânduri-fantomă (33d) | Citiri.Loturi.Postari + Citiri.Contabil.Postari (DocumentId): zero postări |  | atomicitate |
| 19504 | Api RDC: stoc +3 / +30,00 pe LOTUL ORIGINAL, în gestiunea PRIMITOARE (regula −1… | Citiri.Loturi.Postari (DocumentId) + Citiri.Loturi.Solduri |  | mișcarea e pe lotul original; soldul 13 / 130,00; numărătoarea cade |
| 19510 | Api RDC: note pe corespondența ORIGINALĂ, toate negative — `4111 = 707` cu −100… | Citiri.Contabil.Postari (DocumentId) |  | 4111 D −100 / 707 C; 4111 D −21 / 4427 C; 607 D −30 / 371 C; Fel = Operare; numărătoarea cade |
| 19518 | Api RDC (consecința lui F19-D7, măsurată pe REGISTRU): jurnalul de TVA are UN S… | Citiri.Fiscale.Fapte (DocumentId) |  | un singur fapt, pe linia de venit: Baza −100, Tva −21, Livrare |
| 19534 | RISCUL 6 (RDC): pe DRAFT bate Apply — round-trip-ul unui ReadDto SEMNAT readuce… | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | numai clauza de sold 10 / 100,00 după anulare; clauzele DTO rămân |
| 19543 | RISCUL 6 (RDC): re-operarea după anulare + PUT dă EXACT aceleași cifre și acela… | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | numai clauzele Note(idRdc).Count (cade) și soldul 13 / 130,00; cifrele liniilor (costul RDC e sursa b) rămân |
| 19549 | Api RDC: storno prin API → Stornat, rânduri INVERSE append-only (POZITIVE, cu f… | Citiri.Loturi.Solduri (LotId, GestiuneId) |  | soldul 10 / 100,00 după storno; numărătorile rândurilor cu Storno cad |

### VerificaF24Gardian

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 21177 | F24-G2 ({eticheta}) ȘTERGEREA unui `TipTva` referit se refuză ca DEZACTIVAREA l… | Citiri.Fiscale.Postari (TipTvaId distincte) |  | premisa „TipTva liber": tipurile referite de jurnalul fiscal se citesc azi din RegistruTva; pe cub din postările cu TipTvaId (gardul de produs GardianEditare.VerificaTipTva e martor în lista X-D2) |

### VerificaF24Explica

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 21619 | F24-E6 CONSISTENȚĂ (42c): pe un document REAL echivalent (FCT cu o linie de ser… | Citiri.Contabil.Postari (DocumentId) |  | conturile postărilor documentului real = conturile din explicație (debit / credit); numărătoarea (un rând) cade |

### VerificaDvi

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 22208 | DVI-V3 (privat) PREMISA: factura furnizorului extern e operată cu `IMP` — cotă … | Citiri.Fiscale.Fapte (DocumentId) |  | premisă: factura cu IMP are un singur fapt fiscal, cu Tva 0 |
| 22254 | DVI-V4 (privat) planul declarației, cu `Motor/*` NEATINS: ZERO mișcări de stoc,… | Citiri.Contabil.Postari (DocumentId) + Citiri.Loturi.Postari (DocumentId): zero |  | 4426 D 210 / 446 C; 4426 D 105 / 4427 C (baza vamală stă în Carte = Fiscal, în afara Citiri.Contabil); „zero mișcări de stoc" azi e numărătoare globală de rânduri RegistruStoc; numărătoarea notelor cade |
| 22265 | DVI-V5 (privat) jurnalul fiscal: două rânduri de ACHIZIȚIE (direcția vine din `… | Citiri.Fiscale.Fapte (DocumentId) |  | două fapte de Achiziție: Normal 1000/210, TaxareInversa 500/105, Storno fals; PartenerId = biroul vamal de confirmat pe fapt (446 nu poartă Partener pe cub) |
| 22563 | DVI-V16 (privat) stornoul declarației inversează AMBELE registre la data stornă… | Citiri.Contabil.Postari (DocumentId, Tranzactie.Fel = Storno) + Citiri.Fiscale.Fapte (Storno) |  | două postări-pereche inverse la data stornării; Σ Baza −1500, Σ Tva −315; numărătorile cad |
| 22584 | DVI (privat) scena nu lasă urme: partenerii, documentele, legăturile și registr… | Citiri.Fiscale.Fapte (pe perioadă): zero |  | numai clauza „niciun rând fiscal în februarie" după purjă |

### VerificaSolduriPerioada

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 23483 | SOL-C0 ({eticheta}) precondiția comparației: citirile de pe scena deschisă sunt… | Citiri.Loturi.VerificaSoldIntermediar (STOC_INSUFICIENT) |  | G1 (flux indirect: StocService.VerificaSoldIntermediar 23458, 23460 -> c[...] -> Citiri -> citiriDeschis): clauzele „refuz-sold-negativ" și „sold-intermediar-sub-sold" vin din gardul de sold al REGISTRULUI, care moare (inventarul motorului, §3 rândul 2); pe gardul cubului textul începe cu codul („STOC_INSUFICIENT: Sold negativ …", Cub/Citiri/Loturi.cs:100), deci StartsWith(„Sold negativ") nu mai ține — clauza se rescrie pe cod; celelalte clauze (balanță, fișă, sold-stoc) citesc proiecții deja pe cub |
| 23505 | SOL-C ({eticheta}, {moment}) „{cheie}” iese IDENTIC cu și fără snapshot: consum… | Citiri.Loturi.Cumulate + Citiri.Loturi.Disponibile + Citiri.Loturi.VerificaSoldIntermediar (consumatorii cu snapshot ai cubului) |  | o singură linie, rulată per cheie × moment; depind de registre numai cheile citite prin StocService: „solduri-la-data" (SolduriLaData), „sold-pe-cheie" (Sold), „aloca-fifo" (AlocaFifoTolerant), „refuz-sold-negativ" și „sold-intermediar-sub-sold" (VerificaSoldIntermediar). Egalitatea e cu ea însăși (cu și fără snapshot), deci re-țintirea schimbă consumatorul, nu cifra; „aloca-fifo" ține de clasa FIFO (4607). Celelalte chei sunt proiecții ale produsului, deja pe cub |

### VerificaDataInregistrare

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 23893 | DIR-V4 ({eticheta}) documentul cu `Data` {Zi(1, 20):dd.MM.yyyy} într-o perioadă… | Citiri.Loturi.Postari + Citiri.Contabil.Postari (DocumentId) |  | toate postările documentului întârziat sunt datate la data înregistrării (05.02); starea, lotul și numărul rămân |
| 23936 | DIR-V5 ({eticheta}) pe factura întârziată tot ce scrie motorul în registrele cu… | Citiri.Contabil.Postari + Citiri.Loturi.Postari (DocumentId) + Citiri.Fiscale.Fapte (DocumentId) |  | postările la data înregistrării (06.02); faptul fiscal păstrează data documentului: RegistruTva.Data devine FaptFiscal.DataDocument (B-D8 pct. 5); TR-D3 pentru rândurile conexului |
| 23951 | DIR-V6 ({eticheta}) anularea documentului întârziat e PERMISĂ deși `Data` lui e… | Citiri.Loturi.Postari + Citiri.Contabil.Postari (DocumentId): zero postări |  | anulare |
| 23956 | DIR-V6b ({eticheta}) re-operarea îl aduce înapoi, cu registrele tot la data înr… | Citiri.Loturi.Postari (DocumentId) |  | după re-operare postările sunt tot la data înregistrării |
| 24013 | DIR-V10 ({eticheta}) o ieșire înregistrată ÎNAINTEA intrării lotului cade pe ga… | Citiri.Loturi.VerificaSoldIntermediar (STOC_INSUFICIENT) |  | refuzul de sold pe o ieșire înregistrată înaintea intrării lotului; gardul registrului moare (D9-D4), cel al cubului rămâne, dar TEXTUL diferă: registrul scrie „Sold negativ ({sold:0.####}) la …" (Motor/StocService.cs:234), cubul scrie codul în față și soldul neformatat („{cod}: Sold negativ ({sold}) la …", Cub/Citiri/Loturi.cs:100) — clauza Contains(„Sold negativ (-1) la data") se verifică la re-țintire (scara zecimală a sumei), nu se presupune |
| 24024 | DIR-V8 ({eticheta}) cu o dată ULTERIOARĂ înregistrării stornoul trece, iar rând… | Citiri.Loturi.Postari + Citiri.Contabil.Postari (DocumentId, Tranzactie.Fel = Storno) |  | inversele la data stornării (10.02) |
| 24059 | DIR-V11 ({eticheta}) al patrulea registru urmează aceeași regulă: rândul PIF-ul… | Citiri.Imobilizari.Randuri (fișa, panaLa) |  | un rând al PIF-ului întârziat, la data înregistrării (12.02); clauza refuzului de lună rămâne |
| 24066 | DIR-V11b ({eticheta}) stornoul din luna înregistrării trece și scrie rândul inv… | Citiri.Imobilizari.Randuri (fișa, panaLa) |  | rândul invers (Storno) la data stornării (20.02) |
| 24089 | DIR-V12 ({eticheta}) normalizarea din motor („necules ⇒ data documentului”) e g… | Citiri.Loturi.Postari (DocumentId) |  | postările la data normalizată (18.02) |

### VerificaPerioadaDeclarare

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 24127 | PDT-V0 ({eticheta}) profilul neplătitor n-are nicio `PoliticaTva`, deci `Regist… | Citiri.Fiscale.Fapte: zero pe toată baza |  | absenta (bugetar); clauza de politici rămâne |
| 24245 | PDT-V1 ({eticheta}) faptul dintr-o perioadă DESCHISĂ ({Zi(3, 3):dd.MM.yyyy}, în… | Citiri.Fiscale.Fapte (DocumentId) |  | un fapt: data documentului (RegistruTva.Data devine FaptFiscal.DataDocument), perioada de declarare 03 |
| 24307 | PDT-V2 ({eticheta}) factura de intrare cu `Data` {Zi(1, 20):dd.MM.yyyy} (perioa… | Citiri.Fiscale.Fapte (DocumentId) |  | FCT întârziată: DataDocument 20.01, perioada 02, Baza 300 |
| 24312 | PDT-V3 ({eticheta}) factura de ieșire, cu ACELEAȘI date, se declară în 01/{An} — | Citiri.Fiscale.Fapte (DocumentId) |  | FCL întârziată: DataDocument 20.01, perioada 01, Baza 500 |
| 24317 | PDT-V4 ({eticheta}) `ScrisLa` e populat pe toate rândurile scenei, în UTC — făr… | Citiri.Fiscale.Fapte (DocumentId) |  | ScrisLa populat, în UTC (pe cub e Tranzactie.ScrisLa) |
| 24360 | PDT-V7 ({eticheta}) D300 nedepus: livrarea 700/147 rămâne în ianuarie | Citiri.Fiscale.Fapte (DocumentId) |  | perioada 01, Baza 700, Tva 147 |
| 24414 | PDT-V12 ({eticheta}) rândul invers al unei facturi declarate în 01/{An} cade în… | Citiri.Fiscale.Fapte (DocumentId, Storno) |  | faptul invers: data stornării (FaptFiscal.Data), perioada 02, Baza −500, ScrisLa populat |

### VerificaCorectie

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 24679 | COR — precondiție fiscală ({eticheta}): faptele lui ianuarie se declară în 01/{… | Citiri.Fiscale.Fapte (DocumentId) |  | precondiție: faptele lui ianuarie au perioada de declarare 01 |
| 24759 | COR-V3 ({eticheta}) comanda stornează originalul la data corecției — rândurile … | Citiri.Loturi.Postari + Citiri.Contabil.Postari (DocumentId, Tranzactie.Fel = Storno) |  | inversele la data corecției, câte una pentru fiecare postare a originalului; starea rămâne |
| 24811 | COR-V6 ({eticheta}) draftul corectat se operează NORMAL — registrele cad la dat… | Citiri.Loturi.Postari (DocumentId) + Citiri.Loturi.Solduri |  | postările corecției la data înregistrării (10.02); lot vechi 0, lot nou 12; câmpurile lotului rămân |
| 24907 | COR-V13 ({eticheta}) suma algebrică a notelor originalului e ZERO (operarea + s… | Citiri.Contabil.Postari (DocumentId) |  | Σ algebrică a originalului (operare + storno) = 0; Σ corecției = 120 |
| 24924 | COR-V14 ({eticheta}) la EROARE MATERIALĂ rândurile inverse păstrează data faptu… | Citiri.Fiscale.Fapte (DocumentId, Storno) | 24934: STR-CORECTIE (postările Storno din cub poartă PerioadaDeclarare a originalului) | eroare materială: faptul invers la data corecției, perioada originalului (01), Baza −100 |
| 24951 | COR-V15 ({eticheta}) rândurile documentului NOU păstrează data faptului fiscal | Citiri.Fiscale.Fapte (DocumentId) |  | documentul nou păstrează data faptului (DataDocument 15.01), perioada 01, Baza 150 |
| 24981 | COR-V17 ({eticheta}) la FAPT NOU nimic nu se mută: rândul invers se declară în … | Citiri.Fiscale.Fapte (DocumentId) | 24990: STR-CORECTIE (postările Storno din cub rămân în perioada stornării) | fapt nou: inversul în perioada 02; documentul nou cu data 12.02 și perioada 02 |
| 25002 | COR-V18 ({eticheta}) FAPT NOU pe factura de ieșire propune data evenimentului n… | Citiri.Fiscale.Fapte (DocumentId) | 25002: aceeași aserție are deja clauza Fiscale.Fapte (PerioadaD394) | data evenimentului nou 13.02, perioada 02 |

### VerificaAcceptare

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 25378 | ACC-V11 ({eticheta}) consecința scrisă în textul constatării E adevărată: cu da… | Citiri.Contabil.Postari (DocumentId) |  | postările documentului operat cu data înregistrării în luna următoare sunt datate 03.02; starea rămâne |

### VerificaPartide

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 26186 | PAR-V1 ({eticheta}) `TotalStingere` e scris de motor la operare din partidele c… | Citiri.Partide.Total (prin ImperechereService.Total) | 26186: aceeași aserție compară deja cu ImperechereService.Total | coloana Document.TotalStingere dispare (D9-D6): rămân cifrele 100 / 250 / 120 pe totalul din partide |
| 26531 | PAR-V22/SC-CIT-64 ({eticheta}): RDC are totalul de stins 121 din cub, fără cost… | Citiri.Partide.Total (prin ImperechereService.Total) | 26531: aceeași aserție are deja ImperechereService.Total == 121 | totalul de stins al RDC = 121 (fără cost); clauzele pe liniile de creanță rămân |

### VerificaImobilizari

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 26803 | IMO-V6 ({eticheta}) achiziția unei imobilizări postează contul implicit al tipu… | Citiri.Contabil.Postari (DocumentId) + Citiri.Loturi.Postari (DocumentId): zero |  | contul tipului D 3600 / furnizor C; „niciun rând de stoc" azi e numărătoare globală de rânduri RegistruStoc; numărătoarea notelor cade |
| 26886 | IMO-V8 ({eticheta}) punerea în funcțiune NU postează nimic (zero note) și scrie… | Citiri.Imobilizari.Randuri (fișa, panaLa) |  | rândul de intrare al fișei: Fel, Valoare 3600, fiscal 3600, amortizare 0, luni 0, metode, durate, categorie, exclusivitate, Storno fals. Clauza „zero note contabile" e formă (pe cub PIF postează pe fișă); RepartitorId n-are câmp în RandImobilizare (pe cub e Gestiune pe postarea fișei) |
| 26928 | IMO-V10c ({eticheta}) anularea unei INTRĂRI readuce fișa în `Noua` și îi șterge… | Citiri.Imobilizari.Randuri (fișa): zero |  | anulare; starea fișei rămâne |
| 27068 | IMO-V19 ({eticheta}) modernizarea adaugă brut FĂRĂ parametri (null = „neschimba… | Citiri.Imobilizari.Randuri (fișa; pe document) |  | modernizare: Valoare 1650, fără parametri; revizuire: Valoare 0, durata 48, metoda fiscală |
| 27111 | IMO-V23 ({eticheta}) stornarea revizuirii adaugă rândul invers (append-only) ȘI… | Citiri.Imobilizari.Randuri (fișa; pe document) |  | după storno documentul are două rânduri (originalul și inversul); clauzele pe situația fișei rămân |
| 27121 | IMO-V24 ({eticheta}) anularea modernizării șterge rândul ei (corecție directă, … | Citiri.Imobilizari.Randuri (fișa; pe document): zero |  | anulare; clauzele pe situație și stare rămân |
| 27198 | IMO-V26 ({eticheta}) ieșirea postează DOUĂ note pe conturile din `PoliticaAmort… | Citiri.Contabil.Postari (DocumentId) + Citiri.Imobilizari.Randuri |  | amortizare D 900 / imobilizare C; cheltuială cedare D 1500 / imobilizare C; rândul de ieșire −2400 / −2400 / −900 / −900, luni 0; numărătoarea notelor cade |
| 27275 | IMO-V28 ({eticheta}) anularea ieșirii readuce fișa `InFunctiune`, îi șterge dat… | Citiri.Imobilizari.Randuri (fișa; pe document): zero |  | anulare; clauzele pe fișă și situație rămân |
| 27475 | IMO-V31b ({eticheta}) operarea postează o notă per linie cu contabil NENUL (6 n… | Citiri.Contabil.Postari (DocumentId) + Citiri.Imobilizari.Randuri |  | cheltuială D / amortizare C pe liniile cu contabil nenul; rândul fișei A: amortizare 100 / fiscal 100 / deductibil 100, luni 1, valoare 0; fișa G fiscal 100; numărătorile (6 note, 7 rânduri) devin șase perechi de postări și șapte rânduri; RepartitorId fără câmp în RandImobilizare |
| 27493 | IMO-V31d ({eticheta}) codul economic al fișei ajunge pe linia amortizării și de… | Citiri.Contabil.Jurnal (DocumentId) |  | codul economic pe postarea de debit (analiza laturii, B-D8 pct. 8) |
| 27739 | IMO-V48 ({eticheta}) casarea unei fișe amortizate parțial descarcă exact cumula… | Citiri.Contabil.Postari (DocumentId) + Citiri.Imobilizari.Randuri |  | amortizare D 559,09; cheltuială cedare D 4690,91; rândul de ieșire −5250 / −559,09; numărătoarea cade; situația și starea fișei rămân |
| 27872 | IMO-V51d ({eticheta}) după stornarea ieșirii, noiembrie se anulează: dependenți… | Citiri.Imobilizari.Randuri (fișa; pe document): zero |  | anulare |

### VerificaApiDvi

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 29005 | Api DVI: scena nu lasă urme — partenerii, documentele, legăturile și registrele… | Citiri.Fiscale.Fapte (pe perioadă): zero |  | numai clauza „niciun rând fiscal în februarie" după purjă |

### VerificaReviewF26

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 29388 | IMO-R6 ({eticheta}) centrul de cost e dimensiune a notei (`DimensiuniCulese`), … | Citiri.Contabil.Jurnal (DocumentId) |  | centrul de cost pe postarea de debit a notei de amortizare (analiza laturii) |
| 29439 | IMO-R8 ({eticheta}) ieșirea unei fișe neamortizate postează DOAR nota valorii r… | Citiri.Contabil.Postari (DocumentId) |  | ieșirea fișei neamortizate: există postări și niciuna cu valoare zero |
| 29558 | AMO-V1 ({eticheta}) fișa pusă în funcțiune pe {Zi(1, 5):dd.MM.yyyy} și înregist… | Citiri.Imobilizari.Randuri (fișele; pe document) |  | rândul fișei liniare: luni 2, amortizare 200; rândul fișei la timp: luni 1; clauzele pe liniile documentului rămân |
| 29646 | AMO-V7 ({eticheta}) stornoul unei amortizări cu recuperare scrie `-2` luni, nu … | Citiri.Imobilizari.Randuri (fișa; pe document, Storno) |  | inversul amortizării cu recuperare: luni −2, amortizare −200; clauzele pe situație rămân |

### VerificaReviewF27

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 30010 | F27-R4b ({eticheta}) același BTR cu `Data` fizică 10.01 și înregistrare 10.02 T… | Citiri.Loturi.Postari (DocumentId) |  | postările BTR-ului la data înregistrării (10.02); starea și soldul la 31.01 (cititorul cubului) rămân |
| 30141 | F27-R1d ({eticheta}) ușa XAF nu mai comite imperecherea pe ObjectSpace-ul secur… | Citiri.Partide.Total (prin ImperechereService.Total) |  | partida FCL-X la închidere = totalul de stins al documentului; azi totalul se citește din coloana Document.TotalStingere |
| 30215 | F27-R7b ({eticheta}) re-închiderea rescrie snapshot-ul și partidele FĂRĂ docume… | Citiri.Contabil.Postari (DocumentId) |  | documentul anulat nu mai are postări (bugetar) / le păstrează (privat, anularea refuzată); numărătoarea devine existență |
| 30299 | F27-R6a ({eticheta}) FCL 100 stinsă cu 60 în 01 (partidă 40); desfacerea din 02… | Citiri.Partide.Total (prin ImperechereService.Total) |  | total6 = totalul de stins al FCL6, citit azi din Document.TotalStingere; cifrele partidei (total − 60, apoi total) rămân |
| 30311 | F27-R6b ({eticheta}) închiderea lui 02 materializează partidele cu desfacerea (… | Citiri.Partide.Total (prin ImperechereService.Total) |  | idem 30299: partida din februarie = totalul de stins |
| 30352 | F27-RL1 ({eticheta}) OBSERVAȚIE: `Desfa` acceptă și o imperechere din fereastra… | Citiri.Partide.Total (prin ImperechereService.Total) |  | după desfacere restul = totalul de stins (azi coloana TotalStingere) |
| 30411 | F27-RL4 ({eticheta}) `PerioadaFaptului` peste o perioadă NEDEFINITĂ cade pe per… | Citiri.Fiscale.Fapte (DocumentId) |  | faptul din 12 anul precedent își păstrează perioada (PerioadaAn, PerioadaLuna); clauza de rectificativă rămâne |

### VerificaNucleuBcs

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 31906 | NUC-BCS-N-R3-1 ({eticheta}): cheia de stoc are 20 buc / 300 lei (raport 15), dar | Citiri.Loturi.Solduri (LotId, GestiuneId, laData) |  | premisa N-r3: cheia are 20 buc / 300 lei; pe cub cere ca lotul „corectat" să existe ca sold (azi e scris numai în registru, de mână) |

### VerificaNucleuFct

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 32781 | NUC-FCT-P4-2 ({eticheta}): patru rânduri fiscale — capitalizatul desface brutul… | Citiri.Fiscale.Fapte (DocumentId, DetaliuId) |  | patru fapte: capitalizat 82,64 / 17,36; scutit 70 / 0; linia de stoc 50 / 10,5. Azi scena operează factura sub ProbeCub.Nemigrat (fără postări pe cub); re-țintirea cere operarea pe cub |
| 32804 | NUC-FCT-IMO-2 ({eticheta}): netul pe 404 (fallback-ul regulii de natură), taxa … | Citiri.Contabil.Postari (DocumentId) |  | netul pe 404 (500) și taxa 4426 D 105 / 401 C; numărătoarea cade |
| 32893 | NUC-FCT-N-R4-1 ({eticheta}): trei linii de 0,01 la 21% — taxa documentului (0,0… | Citiri.Contabil.Postari (Σ 4426 D pe DocumentId) + Citiri.Fiscale.Fapte (DocumentId) | 32904: NUC-FCT-N-R4-2 (contractul postează taxa 0,01 pe o singură linie) | taxa documentului 0,01 pe o singură linie; trei fapte fiscale cu Σ Tva 0,01; câmpurile liniilor rămân |
| 33008 | STR-REFUZ ({eticheta}): refuzul declarației e refuzul operației — „{mesajRefuz?… | Citiri.Contabil.Postari + Citiri.Fiscale.Postari (DocumentId): zero postări | 33014: ProbeCub.FaraRanduri pe același document | atomicitate; codul refuzului și starea rămân |
| 33060 | STR-FCT-DOUA-PARTIDE ({eticheta}) premisă: linia postează 408 = 401, două contu… | Citiri.Contabil.Postari (DocumentId) |  | 408 D 100 / 401 C 100 pe aceeași linie |

### `ScenariiBcs.cs`

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 180 | id, "Draft; zero tranzacții/postări/registre proprii", os.GetObjectsQuery<Docum… | zero tranzacții și postări pe DocumentId | 180: aceeași aserție are deja clauzele pe C.Tranzactie și C.Postare | atomicitate: ajutorul FaraEfecte al scenei BCS; clauzele de registru doar se șterg |

### `ScenariiDec.cs`

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 227 | SC-DEC-10 | Citiri.Partide.Total (prin ImperechereService.Total) | 227: aceeași aserție are deja ImperechereService.Total == 21 | SC-DEC-10: clauza Decont.TotalStingere == 21 doar se șterge |

### `ScenariiFct.cs`

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 124 | SC-FCT-11 | Citiri.Partide.Total (prin ImperechereService.Total) |  | SC-FCT-11: clauza TotalStingere == 36,41 („total = totalul de stins"); aici fără pereche pe cub în aceeași aserție |
| 170 | SC-FCT-10 | Citiri.Partide.Total (prin ImperechereService.Total) | 170: aceeași aserție are deja ImperechereService.Total == 100 | SC-FCT-10: clauza TotalStingere == 100 doar se șterge |

### `ScenariiImo.cs`

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 149 | SC-IMO-03 | Citiri.Imobilizari.Randuri (fișa; pe document): zero | 148: FaraEfecte(SC-IMO-03) pe același document | SC-IMO-03: „refuzul nu scrie registrul dual" — atomicitate; starea fișei rămâne |

### `ScenariiItv.cs`

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 33 | id, "numărul de linii și absența faptelor fiscale", detalii.Count == linii.Leng… | Citiri.Fiscale.Fapte (DocumentId): zero |  | absenta: închiderea de TVA nu produce fapte fiscale; clauza numărului de linii rămâne |

### `ScenariiNir.Diferente.cs`

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 291 | SC-NIR-30/avans | Citiri.Partide.Total (prin ImperechereService.Total) | 291: aceeași aserție are deja ImperechereService.Total == 100 | SC-NIR-30/avans: clauza TotalStingere == 100 doar se șterge |

### `ScenariiRdc.cs`

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 107 | SC-RDC-05 | Citiri.Fiscale.Fapte (DocumentId) |  | SC-RDC-05: un singur fapt fiscal pe document (numai linia de venit) |

### `ScenaDocumente.cs`

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 407 | id, "Draft și zero efecte persistate proprii", os.GetObjectsQuery<Document>().S… | zero tranzacții și postări pe DocumentId | 407: aceeași aserție are deja clauzele pe C.Tranzactie și C.Postare | atomicitate: ajutorul FaraEfecte (folosit de toate scenele de catalog); clauzele pe RegistruContabil / RegistruStoc / RegistruTva doar se șterg |

### `Nucleu/ProbeCub.cs`

| Linie | Aserția | Ținta | Pereche | Notă |
|---|---|---|---|---|
| 257 | check($"STR-STORNO {prefix}: Σ cub a documentului = 0 pe FIECARE coordonată " +… | ProbeCub.Postari (cubul însuși), grupate pe coordonată |  | Σ cub = 0 pe fiecare coordonată după storno; aserția e pe cub, dar proiectează prin Comparabil.Proiecteaza (tip al oracolului) — cere o proiecție proprie |

## 5. Aserțiile `comportament`

Grupate pe schimbarea din D9-D1; pentru ASM, pe rândul din tabelul D9-D3.

### Schimbarea 1 — valoarea liniei de ieșire nu mai vine din soldul registrului (D9-D3 a) — 26 aserții

Aserțiile citesc valoarea liniei (sau totalul documentului) DUPĂ operare, pe tipurile sursei (a): BCS, BTR, DSC, minusul LDI, consumul ASM. Pe scenele cu preț unic cifra rămâne; se schimbă sursa. Câteva aserții își schimbă și cifra (marcate „CIFRA SE SCHIMBĂ” în notă; vezi secțiunea 10).

| Fișier:linie | Aserția | Schimbarea | Notă |
|---|---|---|---|
| `Program.cs:3068` | Semnarea direcției: consumurile devin negative (−6/−30, −2/−20), produsul rămân… | 1 (D9-D3 a: consumul ASM) | valorile liniilor de consum după operare (−30, −20) vin azi din soldul registrului (AplicaValoareIesire); cifra rămâne, sursa devine decizia ValoareIesire |
| `Program.cs:3159` | Dezasamblare (1 kit → 2 componente): Σ produse (10+15) = Σ consum (25); loturi … | 1 (D9-D3 a: consumul ASM) | consumKit.Valoare = −25 după operare; prețurile loturilor noi nu citesc registre |
| `Program.cs:3176` | Re-operare → același număr, aceleași 3 rânduri (semnarea e idempotentă) | 1 (D9-D3 a: consumul ASM) | consumKit.Valoare = −25 după re-operare; clauza Stoc(dez).Count == 3 e formă și cade |
| `Program.cs:4348` | Lista DSC → un rând, cu Stare ca text (CASE în SQL), gestiune/client, marcajul … | 1 (D9-D3 a: DSC) | Total = 38 în lista DSC după operare = Σ valorilor liniilor de ieșire scrise la operare (azi din soldul registrului); restul clauzelor nu depind de registre |
| `Program.cs:4553` | Operare → valoarea liniei = preț lot × cantitate | 1 (D9-D3 a: BTR) | valoarea liniei de transfer după operare (40) vine azi din soldul registrului |
| `Program.cs:4784` | Citeste după operare → Numar din politică, Total și Valoare materializate de mo… | 1 (D9-D3 a: BTR) | Total = 40 și Linii[0].Valoare = 40 după operare; restul clauzelor rămân |
| `Program.cs:4794` | Lista → un rând, cu Stare ca text (CASE în SQL) și Total din agregatul liniilor | 1 (D9-D3 a: BTR) | Total = 40 în listă după operare |
| `Program.cs:5633` | Operare → valoarea liniei = preț lot × cantitate | 1 (D9-D3 a: BCS) | valoarea liniei de consum după operare (40) vine azi din soldul registrului |
| `Program.cs:5859` | Minus: direcția materializată în semn (−2 / −20) | 1 (D9-D3 a: minusul LDI) | valoarea liniei de minus după operare (−20) vine azi din soldul registrului; cantitatea semnată nu depinde de registre |
| `Program.cs:8329` | Valoarea culeasă e cea postată: hook-ul de operare rescrie aceeași formulă (gea… | 1 (D9-D3 a: BCS) | valoarea liniei de consum după operare (60) vine azi din soldul registrului |
| `Program.cs:8764` | Operarea SEMNEAZĂ cantitatea (28a) — ReadDto o arată ca atare pe documentul (or… | 1 (D9-D3 a: minusul LDI) | clauza Minus.Valoare == −20 după operare; cantitățile semnate și valoarea plusului nu depind de registre |
| `Program.cs:16880` | D18-V2 (a) ieșirile care NU golesc rămân la `preț × cantitate`: 10,01 și 10,01,… | 1 (D9-D3 a: BCS, 1 + 1 + 1 pe lot 3/30,02) | valorile liniilor care nu golesc (10,01 și 10,01) după operare; clauza de sold 1/10,00 se re-țintește pe Citiri.Loturi.Solduri |
| `Program.cs:16897` | D18-V2 (a) a treia ieșire GOLEȘTE lotul: valoarea liniei = 10,00 (soldul valori… | 1 (D9-D3 a: BCS, golirea) | valoarea liniei care golește = 10,00; soldul 0/0 și mișcările ±1/±10,00 se re-țintesc pe Citiri.Loturi.Solduri / Citiri.Loturi.Postari |
| `Program.cs:16902` | D18-V2 (a) dry-run == operare: `Valideaza` trece prin ACELAȘI calcul (33d) și l… | 1 (D9-D2/D9-D3: dry-run-ul lasă estimarea) | CIFRA SE SCHIMBĂ: aserția citește valoarea lăsată de MotorOperare.Valideaza pe linia din memorie (10,00 = golirea planului vechi); după tăiere dry-run-ul nu întoarce valori, pe linie rămâne estimarea cantitate × preț de intrare (10,01) |
| `Program.cs:16908` | D18-V2 (a) ReadDto după operare arată valoarea MATERIALIZATĂ (10,00), nu previz… | 1 (D9-D3 a: BCS) | ReadDto după operare: valoarea liniei și totalul 10,00 |
| `Program.cs:16920` | D18-V2 (a) anulare + re-operare: soldul revine la 1 / 10,00, re-operarea scrie … | 1 (D9-D3 a: BCS, anulare + re-operare) | valoarea liniei re-operate 10,00; soldurile 1/10,00 și 0/0 se re-țintesc pe Citiri.Loturi.Solduri; numărătoarea de rânduri (2) cade |
| `Program.cs:16944` | D18-V2 (b) trei linii pe aceeași cheie în ACELAȘI document se acumulează în ord… | 1 (D9-D3 a: BCS, trei linii pe aceeași cheie) | 10,01; 10,01; 10,00 în ordinea liniilor; soldul 0/0 se re-țintește pe Citiri.Loturi.Solduri — e proba D9-D3 „sursa (a)" pe valorile liniilor |
| `Program.cs:16962` | D18-V2 (c) BTR care golește sursa: valoarea liniei e comună ambelor laturi, dec… | 1 (D9-D3 a: BTR care golește sursa) | valoarea liniei 10,00; sursa 0/0 și destinația 1/10,00 se re-țintesc pe Citiri.Loturi.Solduri |
| `Program.cs:16979` | D18-V2 (d) minusul de inventar care golește lotul ia restul CU SEMNUL liniei (c… | 1 (D9-D3 a: minusul LDI care golește) | valoarea liniei −10,00; soldul 0/0 se re-țintește |
| `Program.cs:16987` | D18-V2 (e) ieșirea care NU golește cheia rămâne exact ca înainte de D2: 10,01 =… | 1 (D9-D3 a: BCS, ieșirea care nu golește) | valoarea liniei 10,01; soldul 2/20,01 se re-țintește |
| `Program.cs:17021` | D18-V2 (r) LIMITA regulii (F1), ca fapt: golirea e decisă la operare pe registr… | 1 (D9-D3: limita retro, „golirea se decide la momentul operării") | valorile liniilor celor două BCS și reziduul 0 / (20,01 − 2 × 10,01) pe cheia registrului; D9-D3 declară că limita rămâne — cifra de pe evaluarea cubului se confirmă la pasul 2 |
| `Program.cs:17079` | D18-V2 (f) DSC-ul conex al FCL-ului care golește lotul: generatorul lasă previz… | 1 (D9-D3 a: DSC care golește lotul) | previzualizarea 10,01 devine 10,00 la operare, pe linie și în DTO; soldul 0/0 se re-țintește |
| `Program.cs:18675` | Api ASM: re-operare după anulare — același număr, aceleași cifre (idempotența `… | 1 (D9-D3 a: consumul ASM) | valoarea consumului re-operat −10,00 (și produsul 10,00); soldul 0/0 se re-țintește |
| `Program.cs:31896` | NUC-BCS-{eticheta} scenă: BCS operat pe lot necorectat — linia la 40 (4 × 10), | 1 (D9-D3 a: BCS) + D9-D5 (operat sub ProbeCub.Nemigrat) | valoarea liniei 40 după operare; clauzele de numărare (2 rânduri de stoc, 1 notă) sunt formă |
| `Program.cs:31913` | NUC-BCS-N-R3-2 ({eticheta}): motorul vechi valorizează cu prețul înghețat — X =… | 1 (D9-D3 a; diferența declarată N-r3) | CIFRA SE SCHIMBĂ: motorul vechi scrie pe linie și pe notă 50 (5 × prețul înghețat 10); după tăiere linia ia decizia contractului, 75 (raportul curent 15) |
| `Program.cs:31924` | NUC-BCS-N-R3-3 ({eticheta}): nucleul evaluează pe raportul CURENT — Y = {valoar… | 1 (D9-D3 a; diferența declarată N-r3) | clauza valoareNoua − valoareVeche == 25 compară contractul cu valoarea motorului vechi; după tăiere rămâne numai 75 |

### Schimbarea 2 — ASM: gardul P = C, fără absorbție, distribuirea pe evaluarea pură (D9-D3) — 36 aserții

**SC-ASM-17**

| Fișier:linie | Aserția | Schimbarea | Notă |
|---|---|---|---|
| `ScenariiAsm.cs:265` | SC-ASM-17 | 2 (SC-ASM-17) | „decizie Δ exactă": Σ AbsorbtieEvaluare.Delta = C − P |
| `ScenariiAsm.cs:267` | SC-CIT-98 | 2 (SC-ASM-17) | SC-CIT-98: absorbția Δ persistată pe purtătorul Transfer |
| `ScenariiAsm.cs:269` | SC-ASM-17 | 2 (SC-ASM-17) | „absorbția nu schimbă prețul cules, valoarea liniei sau prețul lotului" |
| `ScenariiAsm.cs:274` | Postari("SC-ASM-17", d.Id, N.FelTranzactie.Transfer, Ianuarie, [.. Rand(d, 0, -… | 2 (SC-ASM-17) | postările Transfer la valoarea C (cu Δ absorbit); după tăiere al doilea și al treilea ASM sunt refuzate până la redistribuire |
| `ScenariiAsm.cs:278` | Sold("SC-ASM-17/T-r13: evaluare din cub", dual, 0, 0); Diagnostic("SC-ASM-17", … | 2 (SC-ASM-17) | soldul sursei 0/0 după cele trei ASM-uri (ținta ASM-B7 rămâne probă) |

**SC-ASM-18**

| Fișier:linie | Aserția | Schimbarea | Notă |
|---|---|---|---|
| `ScenariiAsm.cs:149` | SC-ASM-15 | 2 (SC-ASM-18) | Corectie(SC-ASM-18): stornoul și corecția ASM-ului cu Δ |

**SC-ASM-19**

| Fișier:linie | Aserția | Schimbarea | Notă |
|---|---|---|---|
| `Program.cs:18485` | ANCORA 75-r1 (capcana, MĂSURATĂ): la culegere documentul pare ECHILIBRAT (`Dife… | 2 (SC-ASM-19: refuz înaintea redistribuirii) | capcana 75-r1: dry-run-ul refuză azi cu textul validării de frunză („nu creează și nu distruge valoare") pe valoarea de golire scrisă de planul vechi; după tăiere refuzul e al gardului P = C (ASAMBLARE_NEBALANSATA) — textul potrivit literal se schimbă |
| `Program.cs:18499` | ANCORA F19-D4: comanda PREZICE valoarea consumului prin dry-run-ul MOTORULUI (d… | 2 (SC-ASM-19: DistribuieValoarea pe evaluarea pură) | predicția consumului 10,00 vine azi din MotorOperare.Valideaza + valorile scrise de planul vechi (PrezicSumaConsum); după tăiere din evaluarea declarantului |
| `Program.cs:18513` | ANCORA F19-D4 (idempotența): a doua rulare pe același document, cu același regi… | 2 (SC-ASM-19: a doua distribuire nu schimbă nimic) | idempotența predicției |
| `Program.cs:18530` | ANCORA F19-D4 (cusătura MĂSURATĂ, nu afirmată): cifra pe care a PREZIS-O comand… | 2 (SC-ASM-19) + 1 (consumul ASM) | predicția = valoarea consumului după operare (−10,00); soldul 0/0 se re-țintește pe Citiri.Loturi.Solduri |
| `ScenariiAsm.cs:333` | SC-ASM-19 | 2 (SC-ASM-19) | capcana: valid înaintea consumului intermediar |
| `ScenariiAsm.cs:336` | SC-ASM-19 | 2 (SC-ASM-19) | RefuzDeclaratie: ASAMBLARE_NEBALANSATA |
| `ScenariiAsm.cs:337` | SC-ASM-19 | 2 (SC-ASM-19) | refuzul operării potrivit pe textul „Valoarea produsă" (validarea de frunză care se mută) + FaraEfecte |
| `ScenariiAsm.cs:338` | SC-ASM-19 | 2 (SC-ASM-19) | BCS-ul intermediar păstrat |
| `ScenariiAsm.cs:340` | Sold("SC-ASM-19", lot, 1, rest); Distribuie(d.Id); Opereaza(d.Id); Postari("SC-… | 2 (SC-ASM-19) | soldul lotului după refuz |
| `ScenariiAsm.cs:342` | Postari("SC-ASM-19", d.Id, N.FelTranzactie.Transfer, data, [.. Rand(d, 0, -1, r… | 2 (SC-ASM-19) | postările după DistribuieValoarea + operare |
| `ScenariiAsm.cs:344` | Sold("SC-ASM-19", lot, 0, 0); }  void Distribuie(Guid doc) => Comanda(os => { | 2 (SC-ASM-19) | lotul 0/0 la sfârșit |

**SC-ASM-20**

| Fișier:linie | Aserția | Schimbarea | Notă |
|---|---|---|---|
| `ScenariiAsm.cs:290` | Postari("SC-ASM-20", d.Id, N.FelTranzactie.Transfer, Ianuarie, [.. Rand(d, 0, -… | 2 (SC-ASM-20) | postările Transfer cu Δ local pe grup |

**SC-ASM-21**

| Fișier:linie | Aserția | Schimbarea | Notă |
|---|---|---|---|
| `ScenariiAsm.cs:296` | Postari("SC-ASM-21", alt.Id, N.FelTranzactie.Operare, Ianuarie, [.. Rand(alt, 0… | 2 (SC-ASM-21) | grup numai-consum și produs pe alt cont, în Operare |

**SC-ASM-22**

| Fișier:linie | Aserția | Schimbarea | Notă |
|---|---|---|---|
| `ProbeAsmOperand.cs:41` | check($"SC-ASM-22/{conventie}: Δ −0,02 și +0,02 se acumulează înaintea gardului… | 2 (SC-ASM-22: dispare aserția acumulării Δ; rămân cifrele și acceptarea) | decizia AbsorbtieEvaluare cu Δ −0,02 și +0,02; operandul poartă SolduriLoturiRegistru |

**SC-ASM-23**

| Fișier:linie | Aserția | Schimbarea | Notă |
|---|---|---|---|
| `ScenariiAsm.cs:420` | SC-ASM-23 | 2 (SC-ASM-23) | RefuzDeclaratie: ASAMBLARE_DELTA_FARA_ANCORA (dispare; devine ASAMBLARE_NEBALANSATA) |
| `ScenariiAsm.cs:421` | SC-ASM-23 | 2 (SC-ASM-23) | refuzul operării cu același cod |
| `ScenariiAsm.cs:422` | FaraEfecte("SC-ASM-23", d.Id); }  void Dependenti() { | 2 (SC-ASM-23) | FaraEfecte după refuz |

**SC-ASM-24**

| Fișier:linie | Aserția | Schimbarea | Notă |
|---|---|---|---|
| `ProbeAsmOperand.cs:63` | check($"SC-ASM-24/{conventie}: produs final {cost - 9.99m}, refuz atomic", inva… | 2 (SC-ASM-24: fără obiect în forma cu Δ; datele dau acum refuz de dezechilibru) | ASAMBLARE_PRODUS_NEPOZITIV pe produsul final după absorbție (P = 10 contra C = 9,99 sau 9) |

**SC-ASM-25**

| Fișier:linie | Aserția | Schimbarea | Notă |
|---|---|---|---|
| `ScenariiAsm.cs:281` | Sold("SC-ASM-25/T-r13", alDoilea.Linii[1], 0, 0); Diagnostic("SC-ASM-25", alDoi… | 2 (SC-ASM-25) | produsul golit de BCS, lot 0/0 |

**fără rând SC-ASM numit**

| Fișier:linie | Aserția | Schimbarea | Notă |
|---|---|---|---|
| `Program.cs:3095` | nume, () => MotorOperare.Opereaza(os, doc)); Check(nume + " — fără rânduri-fant… | 2 (gardul de balansare ASM: ΣP = ΣR devine ΣP = ΣC) | o singură linie pentru 9 apeluri RefuzAsm; numai apelul de la 3107 (2 × 30 = 60 ≠ 50 consumate) trece prin gardul de balansare; celelalte 8 refuzuri nu depind de registre |
| `Program.cs:17124` | D18-V2 (h) ASM cu produsul evaluat NAIV la preț × 1 = 10,01 iar consumul GOLEȘT… | 2 (gardul de balansare ASM: P = 10,01 contra consumului 10,00) | refuzul vine azi din P ≠ R (registru) pe valoarea de golire scrisă de planul vechi; după tăiere e ASAMBLARE_NEBALANSATA pe P ≠ C |
| `Program.cs:17127` | D18-V2 (h) la refuz consumul a fost deja evaluat la rest (−10,00): valoarea fin… | 1 + 2 (D9-D3 „Validările de frunză pe valoarea finală") | CIFRA SE POATE SCHIMBA: aserția citește consum.Valoare = −10,00 lăsată pe linie de operarea REFUZATĂ (planul vechi scrie golirea înaintea validării); după tăiere, la refuz linia poartă estimarea |
| `Program.cs:17136` | D18-V2 (h) cu produsul cules la valoarea consumului (10,00) asamblarea trece: c… | 2 (P = C trece fără redistribuire) + 1 | consumul −10,00 (golirea), produsul 10,00, prețul lotului kit; soldul 0/0 se re-țintește |
| `Program.cs:18572` | ANCORA F19-D4 (reziduul se PLIMBĂ): pe mai multe linii de produs repartizarea e… | 2 (distribuirea pe evaluarea pură, mai multe linii de produs) | SumaConsum 10,00 (predicția golirii) și reziduul plimbat |
| `Program.cs:18580` | Api ASM: distribuirea pe mai multe linii e și ea idempotentă (a doua rulare nu … | 2 (distribuirea pe evaluarea pură, mai multe linii de produs) | idempotența |
| `Program.cs:18584` | Api ASM: documentul distribuit pe mai multe linii trece dry-run-ul FĂRĂ erori (… | 2 (gardul P = C trece după distribuire) | dry-run fără erori pe documentul distribuit |
| `Program.cs:18606` | ANCORA F19-D4 (limita 75-r4, DECLARATĂ nu ascunsă): când niciun preț de 6 zecim… | 2 (ținta distribuirii = consumul prezis; limita 75-r4 rămâne la TR-D9b) | refuzul poartă cifrele 0,01 și 10,01 (predicția consumului) |
| `Program.cs:18660` | ANCORA F19-D4 (cheia de rezervă): dacă NICIO linie de produs n-are valoare cule… | 2 (distribuirea pe evaluarea pură, cheia de rezervă) | SumaConsum 5,00 din predicție |
| `ScenariiAsm.cs:93` | SC-CIT-98 | 2 (decizia AbsorbtieEvaluare dispare) | SC-CIT-98: numai clauza „explicația poartă o decizie AbsorbtieEvaluare"; restul (purtătorul explicației, ieșirile evaluate 60 și 40) rămâne |
| `ScenariiExplicatii.cs:146` | SC-CIT-96 | 2 (decizia AbsorbtieEvaluare dispare) | SC-CIT-96: explicația sintetică serializată include o decizie N.AbsorbtieEvaluare (linia 132); aserția rămâne, fixture-ul pierde decizia |

### Schimbarea 4 — operarea unui tip inert pe profil (D9-D5) — 7 aserții

Ce măsoară catalogul azi, pe profilul bugetar: `SC-RLF-12` și `SC-RDC-16` operează documentul FĂRĂ număr cules și potrivesc refuzul numerotării („politică de numerotare”), iar `SC-DVI-12` potrivește refuzul frunzei DVI („TVA de import”); toate trei cu `FaraEfecte`. Cazul cu număr cules — documentul rămâne Operat fără niciun efect (din cod: `tr-d9-inventar.md`, §6) — NU e măsurat de nicio scenă. Scenele nu dovedesc deci că tipul inert e refuzat la operare: potrivesc trei texte, niciunul al inerției. ITV nu are document de operat în scene (generatorul întoarce `ProfilInert`: `Program.cs:7737`, `ScenariiItv.cs:47`); DSC pe bugetar nu e măsurat în ModelCheck (`ScenariiStocCub.cs:24` iese din scenă), BPR n-are scenă.

| Fișier:linie | Aserția | Schimbarea | Notă |
|---|---|---|---|
| `Program.cs:32947` | STR-CONFIG ({eticheta}): `PosteazaInCub` pe un tip FĂRĂ declarant (BPR) refuză … | 4 / D9-D5 (tipul care nu declară: refuz cu un singur cod) | STR-CONFIG: mesajul refuzului numește azi „PosteazaInCub" și „nu declară" (BPR pus pe cub prin ProbeCub.Migrat); după tăiere flag-ul dispare și refuzul are alt cod/text; clauza „nimic în RegistruContabil" devine zero postări |
| `ScenariiDvi.cs:60` | SC-DVI-12 | 4 / D9-D5 (tip inert pe bugetar: DVI) | SC-DVI-12: „fără politică fiscală și fără activare cub" — clauza !PosteazaInCub |
| `ScenariiDvi.cs:64` | SC-DVI-12 | 4 (operarea tipului inert: DVI pe bugetar) | SC-DVI-12 MĂSOARĂ comportamentul de azi: operarea e REFUZATĂ, cu textul „TVA de import", fără efecte; după tăiere refuzul are un singur cod pe toate ușile |
| `ScenariiRdc.cs:67` | SC-RDC-16 | 4 / D9-D5 (tip inert pe bugetar: RDC) | SC-RDC-16: „fără politică RDC și fără activare cub" — clauza !PosteazaInCub |
| `ScenariiRdc.cs:71` | SC-RDC-16 | 4 (operarea tipului inert: RDC pe bugetar) | SC-RDC-16 MĂSOARĂ comportamentul de azi: operarea e REFUZATĂ, cu textul „politică de numerotare", fără efecte; după tăiere refuzul are un singur cod |
| `ScenariiRlf.cs:50` | SC-RLF-12 | 4 / D9-D5 (tip inert pe bugetar: RLF) | SC-RLF-12: „fără politică RLF și fără activare cub" — clauza !PosteazaInCub |
| `ScenariiRlf.cs:55` | SC-RLF-12 | 4 (operarea tipului inert: RLF pe bugetar) | SC-RLF-12 MĂSOARĂ comportamentul de azi: operarea e REFUZATĂ, cu textul „politică de numerotare", fără efecte; după tăiere refuzul are un singur cod |

### `PosteazaInCub`, regimul dual și `POSTEAZA_IN_CUB_IREVERSIBIL` (D9-D5) — 12 aserții

Aserții care există numai fiindcă un tip poate fi operat cu `PosteazaInCub` fals sau poate ieși din regim.

| Fișier:linie | Aserția | Schimbarea | Notă |
|---|---|---|---|
| `Program.cs:7783` | Bugetar: ASM activ pe cub, cu stoc și numerotare; fără politici contabile/fisca… | D9-D5 (PosteazaInCub dispare) + D9-D8 | clauza tipAsm.PosteazaInCub (D9-D5); clauza Count(RegulaStoc ASM, predator, +1) == 2 pe bugetar — (a) ASM: rândurile ies din seed (D9-D8); rămân numerotarea și lipsa politicilor contabile/fiscale |
| `Program.cs:31461` | STR-NEMIGRAT/101: plata fără postări nu poate crea o legătură de stingere | D9-D5 (regimul dual: tip operat cu PosteazaInCub fals) | STR-NEMIGRAT: refuzul împerecherii unei plăți „fără postări" există numai fiindcă plata a fost operată sub ProbeCub.Nemigrat; după tăiere nu mai există document operat fără postări |
| `Program.cs:32992` | STR-REFUZ ({eticheta}) premisă: motorul VECHI operează factura cu TVA cules 21,… | D9-D5 (regimul dual: tip operat cu PosteazaInCub fals) | STR-REFUZ premisă: „motorul vechi operează" factura pe care declarația o refuză — există numai sub ProbeCub.Nemigrat; după tăiere documentul nu se mai poate opera fără contract |
| `Nucleu/ProbeCub.cs:148` | check(nume, Tranzactii(os, document).Count == 0 && Postari(os, document).Count … | D9-D5 (regimul dual: tip operat cu PosteazaInCub fals) | FaraRanduri e generic (zero tranzacții și postări pe document); apelurile STR-NEMIGRAT / STR-TRANSFER-0 (Program.cs 31526, 31542, 31969, 32922, 32995) probează că un tip nemigrat nu atinge cubul și rămân fără obiect; apelurile STR-ANULARE, STR-CONFIG, STR-REFUZ, STR-VALIDEAZA rămân neschimbate |
| `ScenariiNir.Diferente.cs:144` | try { Anuleaza(na); FaraEfecte("SC-NIR-26", na); Sold("SC-NIR-26", fa.Linii[0],… | D9-D5 (regimul dual: flag-ul tipului oprit între operare și anulare) | SC-NIR-26: anularea NIR-ului operat pe cub cu PosteazaInCub pus de mână pe fals; după tăiere nu există flag de oprit |
| `ScenariiNir.Diferente.cs:325` | SC-NIR-30 | D9-D5 (regimul dual: flag-ul sursei oprit) | SC-NIR-30: operarea NIR-ului cu PosteazaInCub fals și fără politica conexului; soldul 4/100 |
| `ScenariiNir.Diferente.cs:326` | SC-NIR-30 | D9-D5 (regimul dual: flag-ul sursei oprit) | SC-NIR-30: „dovada istorică ține … cu flagul sursei oprit" |
| `ScenariiPartideCub.cs:210` | SC-CIT-109 | D9-D5 (POSTEAZA_IN_CUB_IREVERSIBIL, Materializare.AreTranzactii) | SC-CIT-109: premisa — FCT are tranzacții în cub, CAS nu; ambele cu PosteazaInCub |
| `ScenariiPartideCub.cs:213` | SC-CIT-109 | D9-D5 (POSTEAZA_IN_CUB_IREVERSIBIL) | SC-CIT-109: gardianul refuză ieșirea din regim a tipului cu tranzacții |
| `ScenariiPartideCub.cs:218` | SC-CIT-109 | D9-D5 (POSTEAZA_IN_CUB_IREVERSIBIL) | SC-CIT-109: tipul fără tranzacții poate ieși din regim |
| `ScenariiRlf.cs:62` | SC-CIT-109 | D9-D5 (POSTEAZA_IN_CUB_IREVERSIBIL / alinierea din seed) | SC-CIT-109: seed-ul bugetar cere ieșirea din regim — RLF cu tranzacții rămâne, ITV iese, FCT rămâne |
| `ScenariiSnapshotStoc.cs:120` | SC-CIT-69 | D9-D5 (regimul dual: cititorul vechi al registrului) | SC-CIT-69 „citirea registrului dual nu preia snapshot-ul cubului": StocService.SolduriLaData dă −6 pe registru; fără StocService n-are obiect |

Aserțiile `comportament` cu ținta D9-D8 (`RegulaStoc`, 10 în afara celei de mai sus, care poartă și D9-D5) sunt în secțiunea 11.

### Schimbarea 3 — dreptul de citire pe tipul unui registru (D9-D9)

Nicio aserție din ModelCheck nu citește `RegistrulCitibil` și nicio probă de aici nu pune în cifre dreptul de
citire pe tipul unui registru; schimbarea se probează pe HTTP (`refuzuri.ps1`). Două aserții ating însă tipurile
de registru ca subiect de permisiune sau de listă și sunt `neclar` (secțiunea 8): `Program.cs:21057` (lista
tipurilor interzise rolului `Configurator`) și `Program.cs:21921` (`RegistruStoc_ListView`).

## 6. Montajele

Din cele 33 referințe `montaj`, 15 scriu rânduri de registru de mână (nu prin motor).

### Rânduri de registru scrise de mână

| Fișier:linie | Unitate | Ce scrie și ce ar aranja scena pe cub |
|---|---|---|
| `Program.cs:3922` | Privat: Felia Api FCL | rânduri RegistruStoc de deschidere (10 buc pe două loturi, Marfuri); geamănul pe cub există deja: DeschidereScena.Scrie la 3919 |
| `Program.cs:4516` | Bugetar: Scenariul e2e 3b | rând RegistruStoc de deschidere (10/100, Magazie, MAG1); geamănul pe cub există: DeschidereScena.Scrie la 4515 |
| `Program.cs:4674` | Bugetar: spike 1 felia BTR | rând RegistruStoc de deschidere (10/100, Magazie, MAG1); geamănul pe cub există: DeschidereScena.Scrie la 4673 |
| `Program.cs:5598` | Bugetar: Scenariul 3c BonConsum | rând RegistruStoc de deschidere (10/100, Magazie, MAG1); geamănul pe cub există: DeschidereScena.Scrie la 5597 |
| `Program.cs:5811` | Bugetar: Scenariul 3c ListaDiferenteInventar | rând RegistruStoc de deschidere (10/100, Magazie, MAG1); geamănul pe cub există: DeschidereScena.Scrie la 5810 |
| `Program.cs:8195` | Bugetar: felia 6 Api BCS scriere | rând RegistruStoc de deschidere (20/200, Magazie, MAG1); geamănul pe cub există: DeschidereScena.Scrie la 8194 |
| `Program.cs:8444` | Bugetar: felia 6 Api LDI scriere | rând RegistruStoc de deschidere (10/100, Magazie, MAG1); geamănul pe cub există: DeschidereScena.Scrie la 8443 |
| `Program.cs:13746` | VerificaSaftStocuri | rânduri RegistruStoc de deschidere (pozițiile inițiale ale scenei, Marfuri); geamănul pe cub există: DeschidereScena.Scrie la 13744 |
| `Program.cs:14812` | VerificaSaftStocuriFixuri | pe ramură MOARTĂ: LotCuDeschidere scrie un rând RegistruStoc numai când cantitate <= 0, iar singurul apel (14819) are cantitate 100 și trece prin FCT + NIR; fără geamăn pe cub — ramura se șterge |
| `Program.cs:16611` | VerificaD394 | SQL brut): rând RegistruTva „ca înainte de F13" — TaxareInversa × Livrare cu Tva 21 pe linia TI21 a facturii LV; pe cub n-are geamăn (declarantul nu postează taxă pe TI × Colectat) și proba (16627) rămâne fără obiect |
| `Program.cs:27004` | VerificaImobilizari | prin ușa securizată, ca probă de gard, nu ca date): rând RegistruImobilizari de amortizare pe care GardianEditare trebuie să-l refuze; pe cub proba echivalentă e scrierea directă a unei Postare / Tranzactie |
| `Program.cs:31847` | VerificaNucleuBcs | FĂRĂ geamăn pe cub: ajutorul Intrare scrie rânduri RegistruStoc de intrare pe loturile scenei, inclusiv lotul „corectat" (10/100 + 10/200 pe același lot cu preț înghețat 10 — date pe care motorul nu le produce, N-r3); pe cub scena ar trebui să deschidă aceleași solduri (DeschidereScena.Scrie: 20 buc / 300 lei pe lotul corectat) |
| `Program.cs:32063` | VerificaNucleuBtr | FĂRĂ geamăn pe cub: ajutorul Intrare scrie rânduri RegistruStoc de intrare (lotul B: 10/100 + 10/200, preț înghețat 10); pe cub: DeschidereScena.Scrie cu aceleași solduri |
| `Program.cs:32516` | VerificaNucleuFclDsc | FĂRĂ geamăn pe cub: ajutorul Intrare scrie rânduri RegistruStoc de intrare (loturi cu 10/100 + 10/200 și 10/150, prețuri înghețate diferite de raport); pe cub: DeschidereScena.Scrie cu aceleași solduri |
| `Program.cs:33172` | VerificaLaturi | FĂRĂ geamăn pe cub: rând RegistruStoc de intrare (10/100, Magazie, MAG1) ca premisă a probelor de latură; pe cub: DeschidereScena.Scrie |

Două familii. Soldurile de deschidere `RegistruStoc` din scenele vechi au deja geamănul pe cub, cu o linie mai
sus (`DeschidereScena.Scrie`): rândul de registru doar se șterge. Ajutoarele `Intrare` din `VerificaNucleuBcs`,
`VerificaNucleuBtr`, `VerificaNucleuFclDsc` și rândul din `VerificaLaturi` scriu NUMAI registrul: scenele acelea
lucrează pe loturi cu două intrări la valori diferite și preț înghețat (N-r3), date pe care motorul nu le produce;
pe cub le poate da numai o deschidere (`DeschidereScena.Scrie`) cu aceleași solduri. Separat: rândul `RegistruTva`
injectat prin SQL (`Program.cs:16611`) nu are geamăn posibil, iar rândul `RegistruImobilizari` de la
`Program.cs:27004` e o probă de gard, nu date.

### Celelalte montaje

| Fișier:linie | Unitate | Ce aranjează |
|---|---|---|
| `Program.cs:23330` | VerificaSolduriPerioada | citire de premisă (nu scriere): conturile și repartitorii notei NIR, ca să construiască nota de probă cu dimensiuni pe ambele laturi; pe cub: Citiri.Contabil.Postari pe documentul NIR |
| `Program.cs:23376` | VerificaSolduriPerioada | citire de premisă: contul de stoc și contul de terț ale scenei (din nota NIR); pe cub: Citiri.Contabil.Postari |
| `Program.cs:23385` | VerificaSolduriPerioada | citire de premisă: cheia TipStoc a scenei — moare cu cheia de registru (devine contul de stoc) |
| `AcoperireInvarianti.cs:260` | (fișier întreg) | mutantul EXPLICATIE-STORNO ajustează de mână și RegistruStoc, numai ca mutantul să treacă de acoperirea stocului și să ajungă la ramura explicației; după tăiere ajustarea n-are obiect |
| `AcoperireInvarianti.cs:263` | (fișier întreg) | mutantul EXPLICATIE-STORNO ajustează de mână și RegistruStoc, numai ca mutantul să treacă de acoperirea stocului și să ajungă la ramura explicației; după tăiere ajustarea n-are obiect |
| `Nucleu/ProbeCub.cs:315` | (fișier întreg) | Comutator: comută TipDocument.PosteazaInCub pe durata scenei (ProbeCub.Migrat / Nemigrat / MigratPeClasa) — regimul ca premisă; dispare cu D9-D5 |
| `Nucleu/ProbeCub.cs:316` | (fișier întreg) | Comutator: comută TipDocument.PosteazaInCub pe durata scenei (ProbeCub.Migrat / Nemigrat / MigratPeClasa) — regimul ca premisă; dispare cu D9-D5 |
| `Nucleu/ProbeCub.cs:323` | (fișier întreg) | Comutator: comută TipDocument.PosteazaInCub pe durata scenei (ProbeCub.Migrat / Nemigrat / MigratPeClasa) — regimul ca premisă; dispare cu D9-D5 |
| `ProbeAsmOperand.cs:31` | (fișier întreg) | operandul sintetic poartă SolduriLoturiRegistru (soldul registrului pentru R); câmpul dispare (D9-D2) — toate probele din buclă își construiesc operandul prin el |
| `ScenariiExplicatii.cs:132` | (fișier întreg) | explicația sintetică a probei de serializare (SC-CIT-96) conține o decizie N.AbsorbtieEvaluare; tipul deciziei dispare |
| `ScenariiNir.Diferente.cs:143` | (fișier întreg) | SC-NIR-26: TipDocument(NIR).PosteazaInCub pus de mână pe fals și refăcut — regimul ca premisă |
| `ScenariiNir.Diferente.cs:145` | (fișier întreg) | SC-NIR-26: TipDocument(NIR).PosteazaInCub pus de mână pe fals și refăcut — regimul ca premisă |
| `ScenariiNir.Diferente.cs:323` | (fișier întreg) | SC-NIR-30: PosteazaInCub pus de mână pe fals (și PoliticaConex ștearsă), apoi refăcute — regimul ca premisă |
| `ScenariiNir.Diferente.cs:334` | (fișier întreg) | SC-NIR-30: PosteazaInCub pus de mână pe fals (și PoliticaConex ștearsă), apoi refăcute — regimul ca premisă |
| `ScenariiPartideCub.cs:212` | (fișier întreg) | SC-CIT-109: PosteazaInCub comutat de mână pe FCT și pe CAS — regimul ca premisă |
| `ScenariiPartideCub.cs:214` | (fișier întreg) | SC-CIT-109: PosteazaInCub comutat de mână pe FCT și pe CAS — regimul ca premisă |
| `ScenariiPartideCub.cs:215` | (fișier întreg) | SC-CIT-109: PosteazaInCub comutat de mână pe FCT și pe CAS — regimul ca premisă |
| `ScenariiPartideCub.cs:219` | (fișier întreg) | SC-CIT-109: PosteazaInCub comutat de mână pe FCT și pe CAS — regimul ca premisă |

## 7. Oracolul: apelanți și moduri de rulare care mor

Mor întregi (D9-D7 a): `Nucleu/CubDinRegistre.cs`, `Nucleu/Normalizari.cs`, `Nucleu/Comparabil.cs`,
`Nucleu/ReconciliereCub.cs`, `Nucleu/GatePeBaza.cs`, `Nucleu/DiagnosticValoriStoc.cs`.

| Ce moare | Unde e chemat |
|---|---|
| modul `--declaratie-pe-baza` (`GatePeBaza.Ruleaza`) | `Program.cs:97–119` |
| modul `--reconciliere-cub` (`ReconciliereCub`, `DiagnosticValoriStoc`) | `Program.cs:120–162` |
| `ProbaReconciliere` / `STR-RECONCILIERE` (definiția `Program.cs:340`, aserția `:348`) | 8 apeluri: `Program.cs:31764`, `31979`, `32135`, `32366`, `32584`, `32959`, `33044`, `33076` |
| `ProbaReconciliere` local + `NUC-ASM-RECONCILIERE` | `ScenariiAsm.cs:225–248`, apel la `:106` |
| blocurile `NUC-ORACOL-1…9` (`CubDinRegistre` + `Normalizari` + `Comparabil` în scenă) | `Program.cs:5039–5086` (FCT → NIR), `5669–5709` (BCS) |
| `NUC-PLT-SPLIT-4`, `NUC-BCS-N-R3-4`, `NUC-FCT-N-R4-3` | `Program.cs:31493–31502`, `31928–31941`, `32909–32919` |
| `ProbeNucleu.Proba` — aserțiile „normalizările fără reziduu” și „postările = registrele normalizate” | `Nucleu/ProbeNucleu.cs:60`, `:68`; 25 de apeluri în `Program.cs` |
| `ProbeCub.ProbaOperare` — `STR-ORACOL` și `STR-OPERARE … = registrele normalizate` | `Nucleu/ProbeCub.cs:195`, `:204`; 16 apeluri în `Program.cs` |
| `ProbeCub.ProbaTransferPliat` — `STR-TRANSFER-3` | `Nucleu/ProbeCub.cs:141`; 4 apeluri în `Program.cs` |
| X-D3 pe baza de volum: reconcilierea (a)–(g), „fiecare ramură are fapte”, diagnosticul ASM-B7 | `PerfCub.Evaluare.cs:142`, `:144`, `:146` |
| diagnosticul valoric și `ReconciliereCub.CodIesire` ca subiect de probă | `ProbeAsmOperand.cs:83–104`; `ScenariiAsm.cs:357` (ajutorul `Diagnostic`, apeluri la `:279`, `:282`) |
| reconcilierea și diagnosticul în scenele de catalog | `ScenariiNir.cs:133`, `:135`; `ScenariiNir.Diferente.cs:272–279`; `ScenariiDvi.cs:153`, `:179`; `ScenariiLdi.Folosinta.cs:53`, `:129` |
| mutanții acoperirilor registru → cub din `INV-CUB` | `AcoperireInvarianti.cs:17`, `18`, `21`, `25` (`DESCHIDERE-EGALA`, `DESCHIDERE`, `IMO-FISA`, `IMO-REGISTRU`), verdictul la `:64` |
| mutanții acoperirii scriși în scene (prin `Invarianti.Verifica`) | `ScenariiFisa.cs:21`, `25`, `85` (`CITIRE_ISTORIC_INCOMPLET`); `ScenariiBtr.cs:38` și `ScenariiNir.Acoperire.cs:33`, `50`, `60` (`CITIRE_ISTORIC_STOC_INCOMPLET`) |

Rămân după tăiere, cu oracolul scos din ele:

- `ProbeNucleu.Proba`: contractul acceptat, `Conservare.Verifica`, determinismul și pragul de interogări (patru
  din șase aserții);
- `ProbeCub.ProbaOperare`: cele două aserții `STR-OPERARE` pe cubul persistat și `STR-ROUNDTRIP`;
  `ProbeCub.ProbaStorno` întreg, cu două dependențe de `Comparabil` de înlocuit: proiecția din `:253` și afișarea
  diferenței `Scrie` (`:276`);
- `ProbeCub.FaraRanduri`: apelurile `STR-ANULARE`, `STR-CONFIG`, `STR-REFUZ`, `STR-VALIDEAZA`; cele `STR-NEMIGRAT` și
  `STR-TRANSFER-0` rămân fără obiect, la fel `ProbeCub.Migrat` / `Nemigrat` / `MigratPeClasa` (comutatorul de la `:306`);
- `AcoperireInvarianti`: ceilalți mutanți; `Invarianti.Verifica` ca probă pozitivă pe fiecare scenă (`:50`), cu
  domeniul redus la invarianții interni ai cubului.

Referințe indirecte (fără identificator în lista canonică), urmărite de mână:

- `C.Citiri.Invarianti.Verifica` ca purtător al acoperirilor registru → cub: `AcoperireInvarianti.cs:74`,
  `ScenariiFisa.cs:20–25`, `:85`, `ScenariiBtr.cs:38`, `ScenariiNir.Acoperire.cs:60`, `PerfCub.Evaluare.cs:80`;
- martorii chemați direct: `C.Citiri.Loturi.VerificaAcoperire` (`ScenariiBtr.cs:30`, `ScenariiNir.Acoperire.cs:35`, `:52`)
  și `C.Citiri.Imobilizari.VerificaAcoperire` (`ScenariiImo.cs:124`, în ajutorul `Sold`) — apeluri care aruncă, nu aserții;
- `ImperecheriProiectii.VerificaAcoperire` (compară `Document.TotalStingere` cu partidele cubului): `Program.cs:26537`,
  `ScenariiPartideCub.cs:420`, mutantul `POLITICA` din `AcoperireInvarianti.cs:113`;
- tipurile `CheieStoc` și `SoldStoc` din `Motor/StocService.cs`, folosite în `Program.cs` (`new CheieStoc(…)` de 32 de
  ori, pe aceleași linii cu `StocService`; `new SoldStoc(…)` ca valoare de comparație a ajutoarelor `SoldCheie`);
  `SoldStoc` e și purtătorul ajutoarelor `SnapshotStoc` / `AsteptatStoc` (`Program.cs:23082`, `:23087`), care citesc
  snapshot-ul și cubul, nu registrele: acolo se schimbă tipul purtător, nu sursa.

## 8. Aserțiile `neclar`

34 aserții, în șase clase. Nota fiecăreia, în `asertii.tsv`, poartă întrebarea întreagă.

### Repartitorul de pe latura notei — 17

Aserția citește `DimensiuniDebit()` / `DimensiuniCredit().RepartitorId`. Registrul pune pe fiecare latură repartitorul contrapartidei (debit ← predator, credit ← primitor) sau un implicit polimorf; cubul pune `Gestiune` pe piciorul intern și `Partener` pe piciorul de terț, numai pe cont cu `RolTert` / urmărire de partide sau cu `CodTva` (B-D8 pct. 4, 9, 10 din `tr-d6b-declaratia-fluxului-contract.md`). Cifra nu e aceeași prin construcție. **Întrebarea, o dată pentru toată clasa:** clauza se rescrie pe convenția cubului, declarant cu declarant, sau cade ca formă a notei? Valorile și conturile din aceleași aserții se re-țintesc (coloana `tinta`).

| Fișier:linie | Aserția | Întrebarea |
|---|---|---|
| `Program.cs:987` | Rândul 4426 al serviciului: 21; dimensiunile din default-ul polimorf … | valoarea 4426 D 21 se re-țintește; repartitorul pe latura notei (debit=furnizor, credit=MAG1) e convenția contrapartidei, pe cub piciorul poartă Partener/Gestiune proprii (B-D8 pct. 4, 9) — se rescrie pe convenția cubului sau cade? |
| `Program.cs:1060` | DEC: cheltuiala net (628 = 542, 30) + TVA justificat (4426 = 542, 6,3… | cifrele 628 D 30, 4426 D 6,3, 542 C se re-țintesc; „creditul pe titular" citește repartitorul laturii credit — pe cub e Partener pe postarea 542 numai dacă 542 are RolTert (B-D8 pct. 10) |
| `Program.cs:1577` | DSC dimensiuni: AMBELE laturi Repartitor=gestiunea (override polimorf… | ambele laturi ale notei poartă gestiunea (override polimorf) și Material = produsul lotului; pe cub Gestiune/Produs stau pe postare, nu pe latura notei — ce picior le poartă pe DSC? |
| `Program.cs:1872` | Privat: dimensiunile din default-ul polimorf (debit←predator, credit←… | debit←predator (SEDIU), credit←primitor (unitate) e convenția contrapartidei pe latura notei; pe cub piciorul poartă repartitorul propriu (B-D8 pct. 9) |
| `Program.cs:4980` | Nota FCT: dimensiuni rezolvate (cod economic + repartitori laturi) | codul economic pe piciorul de debit se re-țintește (analiza laturii se păstrează, B-D8 pct. 8); repartitorii laturilor (debit=furnizor, credit=MAG1) au altă convenție pe cub (pct. 9: gestiunea pe piciorul propriu, 401 fără ea) |
| `Program.cs:5646` | Nota BCS: repartitori din laturi (debit←predator, credit←primitor — 0… | debit←predator (MAG1), credit←primitor (loc) e convenția contrapartidei; pe cub D 6xx poartă locul de consum și C 3xx poartă MAG1 (așteptarea NUC-ORACOL-3, 5696–5697) — invers |
| `Program.cs:6032` | Nota FCL: repartitori din laturi (debit←emitent, credit←client — 00 §… | debit←emitent (SEDIU), credit←client pe laturile notei; pe cub gestiunea/partenerul stau pe piciorul propriu (B-D8 pct. 9, 10; pe bugetar niciun cont n-are RolTert) |
| `Program.cs:6322` | Nota avansului: repartitori din laturi (debit←casă, credit←angajat — … | debit←casă, credit←angajat pe laturile notei e inversul convenției cubului |
| `Program.cs:7447` | Dimensiuni debit: default←titular la deplasare, repartitorul EXPLICIT… | codul economic pe debit se re-țintește; repartitorul laturii de debit (titular / MAG1 explicit) — DeclarantDecont.Capat pune repartitorul fiecărui picior ca Partener (extern) sau Gestiune (intern), deci probabil aceeași cifră, dar pe alt câmp; de confirmat |
| `Program.cs:7451` | Dimensiuni credit: 542 pe TITULAR (default polimorf, nu primitorul SE… | 542 pe titular = Partener pe postarea 542 C (DeclarantDecont.Capat); probabil aceeași cifră, de confirmat |
| `Program.cs:7470` | Angajat fără ContImplicit → creditul cade pe fallback-ul 542.01.00 | creditul pe 542 se re-țintește; repartitorul laturii credit = angajatul — idem 7451 |
| `Program.cs:7635` | Dimensiuni debit: repartitorul EXPLICIT (MAG1) pe prima linie, defaul… | repartitorul laturii de debit (explicit MAG1 / default predator SEDIU) — convenția piciorului pe cub de confirmat pe DeclarantNotaContabila |
| `Program.cs:7638` | Dimensiuni credit: default polimorf (primitor) pe ambele linii | repartitorul laturii de credit (default primitor) — idem 7635 |
| `Program.cs:9179` | ANCORA F8-D13.2: repartitorul CULES (MAG1) e nivelul MAXIM al coalesc… | codul economic pe debit se re-țintește; repartitorul laturii de debit (MAG1 cules / titular implicit) — DeclarantDecont.Capat îl pune ca Gestiune / Partener pe piciorul lui; probabil aceeași cifră, de confirmat |
| `Program.cs:9183` | Creditul (542) se dimensionează pe TITULAR pe AMBELE linii (default p… | 542 pe titular = Partener pe postarea 542 C; idem 9179 |
| `Program.cs:17524` | Api NTC: dimensiunile — repartitorul EXPLICIT al liniei e nivelul MAX… | repartitorul explicit al liniei pe laturile notei (partener X/Y, unitate) și codul economic pe debit; codul economic se re-țintește, repartitorul pe latură cere convenția DeclarantNotaContabila |
| `Program.cs:22522` | DVI-V15 (privat) TVA-ul în vamă se plătește ca orice TAXĂ, nu prin im… | 446 D / cont propriu C 210 și net 446 = 0 se re-țintesc; clauza notaDvi446[0].CreditRepartitorId == biroul vamal nu are aceeași cifră pe cub — scena însăși măsoară că 446 n-are coordonata Partener (fisaVama.Count == 0); clauza cade sau se rescrie? |

### Probele `StocService.VerificaGoliri` — 7

Funcție pură pe rânduri sintetice `RandGolire`, fără apelant de producție (o cheamă numai `Import1C.ReconciliereLuna` și ModelCheck). **Întrebarea:** moare cu `StocService` sau rămâne ca regulă pură (D9-D2: „în afară de ce pasul 1 dovedește că e regulă pură cu un consumator rămas”)?

| Fișier:linie | Aserția | Întrebarea |
|---|---|---|
| `Program.cs:16850` | D18-V2 (o) oracolul golirii: golirea corectă ⇒ un singur verdict, `Ex… | probă pură a StocService.VerificaGoliri pe rânduri sintetice de registru (RandGolire); funcția n-are apelant de producție (numai Import1C.ReconciliereLuna și ModelCheck) — moare cu StocService sau rămâne ca regulă pură (D9-D2)? |
| `Program.cs:16855` | D18-V2 (o) valoarea dublată pe rândul golitor ⇒ `CuValoare` cu Σ −10,… | idem 16850 (valoarea dublată ⇒ CuValoare) |
| `Program.cs:16858` | D18-V2 (o) retro (F1): rândul din 10.05 nu golea la operare, dar azi … | idem 16850 (retro ⇒ ReDeschisaRetro) |
| `Program.cs:16863` | D18-V2 (o) rândul golitor STORNAT nu mai e ieșire (perechea pe Detali… | idem 16850 (rândul golitor stornat) |
| `Program.cs:16865` | D18-V2 (o) golirea FISCALĂ (RLF, F5) lasă −0,01 pe lot prin contract … | idem 16850 (golirea fiscală RLF) |
| `Program.cs:16867` | D18-V2 (o) fereastra lunii (F7): verdict doar pe rândurile din interv… | idem 16850 (fereastra lunii) |
| `Program.cs:17026` | D18-V2 (r) oracolul clasifică rândul din 10.05 `ReDeschisaRetro` (măs… | StocService.VerificaGoliri pe rândurile reale de registru ale lotului retro (ReDeschisaRetro); idem 16850 |

### Alocarea FIFO a `StocService` — 3

`AlocaFifo` / `AlocaFifoTolerant` sunt oracole ale probelor (lista X-D2). `Citiri.Loturi.Disponibile` dă ordinea FIFO, dar alocarea n-are cititor public pe cub. **Întrebarea:** proba se re-țintește pe `Loturi.Disponibile` (+ primitiva `N.Fifo`) sau cade cu serviciul?

| Fișier:linie | Aserția | Întrebarea |
|---|---|---|
| `Program.cs:4607` | FIFO → o alocare pe lotul disponibil | StocService.AlocaFifo e oracol al probelor fără apelant de producție (lista X-D2); aserția probează alocarea FIFO a serviciului care moare — se re-țintește pe Loturi.Disponibile (+ N.Fifo) sau cade cu serviciul? |
| `Program.cs:4608` | FIFO peste disponibil → refuz | idem 4607: refuzul FIFO peste disponibil al StocService.AlocaFifo |
| `Program.cs:24002` | DIR-V9 ({eticheta}) lotul documentului întârziat e mai NOU decât al c… | ordinea FIFO a loturilor după data înregistrării e probată prin StocService.AlocaFifoTolerant (oracol fără apelant de producție); clasa FIFO (4607): Loturi.Disponibile dă ordinea (Deschisa, LotId), alocarea n-are cititor |

### Cusătura JT-D6 `RegistruTva` ↔ `RegistruContabil` — 2

Σ TVA fiscal (Normal și TaxareInversa) = Σ pe conturile de TVA, per document, pe toată baza. Pe cub faptul fiscal și postarea de taxă sunt aceeași postare, iar Σ pe conturile de TVA nu mai e aceeași cifră la taxare inversă (4426 D și 4427 C). **Întrebarea:** se rescrie ca invariant pe cub sau cade ca egalitate între două registre? Ține de ea și diagnosticul tipărit de la `Program.cs:10375`.

| Fișier:linie | Aserția | Întrebarea |
|---|---|---|
| `Program.cs:10319` | VERIFICAREA 1 (JT-D6), pe toate cele {docIdsFiscale.Count} documente … | cusătura JT-D6 RegistruTva ↔ RegistruContabil (Σ TVA fiscal Normal/TI = Σ pe conturile de TVA, per document, pe toată baza): pe cub faptul fiscal și postarea de taxă sunt aceeași postare, iar Σ pe conturile de TVA nu mai e aceeași cifră la taxare inversă (4426 D și 4427 C); se rescrie ca invariant pe cub (Σ Fapte.Tva = Σ postări RolTva = Taxă) sau cade ca egalitate între două registre? |
| `Program.cs:10422` | JT-D6 peste storno: cusătura se închide algebric pe document (0 == 0)… | idem 10319: cusătura JT-D6 pe documentul stornat |

### Invariantul `CITIRE_PARTIDE_POLITICA` — 2

`ImperecheriProiectii.VerificaAcoperire` compară totalul din antet (`Document.TotalStingere`) cu partidele cubului. Fără coloană rămâne fără termen de comparație. **Întrebarea:** cade sau se reformulează (D9-D10)? Aceeași soartă o are mutantul `POLITICA` (`AcoperireInvarianti.cs:113–123`, verdict `neclar` pe referință).

| Fișier:linie | Aserția | Întrebarea |
|---|---|---|
| `Program.cs:26537` | SC-CIT-65 ({eticheta}): costul RDC fără partidă respectă totalul de d… | SC-CIT-65: ImperecheriProiectii.VerificaAcoperire trece — invariantul CITIRE_PARTIDE_POLITICA compară antetul (Document.TotalStingere) cu partidele cubului; fără coloană rămâne fără termen de comparație — cade sau se reformulează (D9-D10)? Referință indirectă (fără identificator canonic pe linie) |
| `ScenariiPartideCub.cs:420` | SC-CIT-43 | SC-CIT-43: mutantul CITIRE_PARTIDE_POLITICA trece prin ImperecheriProiectii.VerificaAcoperire, care compară antetul (Document.TotalStingere) cu partidele cubului; fără coloană invariantul rămâne fără termen de comparație — cade sau se reformulează (D9-D10)? Referință indirectă |

### Individuale — 3

| Fișier:linie | Aserția | Întrebarea |
|---|---|---|
| `Program.cs:21057` | F24-R3 ({eticheta}) `Updater.SeedRolConfigurator` e idempotent (un si… | lista tipurilor pe care rolul Configurator NU are voie să le scrie numește tipurile de registru (typeof(RegistruContabil/RegistruStoc/RegistruTva)); după tăiere tipurile dispar: se scot din listă (formă) sau se înlocuiesc cu Postare/Tranzactie, ca regula „Configuratorul nu scrie evidența" să rămână păzită (leagă de D9-D9)? |
| `Program.cs:21921` | D85-R1 ({eticheta}) pe `RegistruStoc_ListView` real, fiecare coloană … | aserția probează modul ServerView pe lista XAF REALĂ a registrului de stoc (RegistruStoc_ListView): proiecția păstrează coloanele grilei. Lista moare la pasul 7 (D9-D9); se re-țintește pe lista XAF pe Postare (pasul 4) sau cade? |
| `Program.cs:27759` | IMO-V49 ({eticheta}) luna IEȘIRII nu se amortizează (fișa casată pe 1… | fără cititor: clauza randE.RepartitorId == locul nou al fișei citește repartitorul rândului de registru; RandImobilizare (Citiri.Imobilizari.Randuri) nu-l poartă — pe cub e Gestiune pe postarea fișei. Restul aserției (liniile lui noiembrie) rămâne |

Referințe `neclar` (linii care nu sunt aserții): `Program.cs:10375`; `AcoperireInvarianti.cs:117`; `AcoperireInvarianti.cs:119`; `AcoperireInvarianti.cs:121` — diagnosticul JT-D6 și mutantul `POLITICA`.

## 9. Fișierele

**Dispar întregi** (oracolul, D9-D7 a): `Nucleu/CubDinRegistre.cs`, `Nucleu/Normalizari.cs`, `Nucleu/Comparabil.cs`,
`Nucleu/ReconciliereCub.cs`, `Nucleu/GatePeBaza.cs`, `Nucleu/DiagnosticValoriStoc.cs`. `ProbeCititoriRegistre.cs`
și `ProbeCititoriRegistre.Lista.cs` nu sunt inventariate aici (lista nominală X-D2 se reduce la clasa `Legatura`,
D9-D7 b).

**Rămân, după scoaterea referințelor** (cele 29 de fișiere din lista canonică):

| Fișier | Referințe | Ce rămâne de făcut |
|---|---|---|
| `Program.cs` | 658: ajutor 72, asertie 337, comentariu 22, montaj 18, neclar 1, oracol 51, purja 149, raport 8 | tot inventarul de mai sus |
| `AcoperireInvarianti.cs` | 12: montaj 2, neclar 3, oracol 7 | patru mutanți mor; `POLITICA` după D9-D10; ajustarea de registru din `EXPLICATIE-STORNO` cade |
| `Nucleu/ProbeCub.cs` | 31: ajutor 3, asertie 1, comentariu 2, montaj 3, oracol 22 | comutatorul `PosteazaInCub` și probele „= registrele normalizate” cad; rămân probele pe cub |
| `Nucleu/ProbeNucleu.cs` | 8: oracol 8 | rămâne fără oracol: patru aserții din șase |
| `PerfCub.Evaluare.cs` | 8: oracol 8 | blocul X-D3 `Reconciliaza` pierde reconcilierea, contoarele de registru și diagnosticul |
| `ProbeAsmOperand.cs` | 16: asertie 1, montaj 1, oracol 14 | probele SC-ASM-22 și SC-ASM-24 se rescriu pe gardul P = C; `Diagnostic` și SC-ASM-16 mor |
| `Purja.cs` | 1: comentariu 1 | un comentariu |
| `ScenaDocumente.cs` | 7: asertie 3, purja 4 | `FaraEfecte` pierde trei clauze; purja scenei pierde patru linii |
| `ScenariiAsm.cs` | 14: asertie 3, oracol 11 | rândurile SC-ASM-17…25 se rescriu (pasul 2); reconcilierea locală și diagnosticul mor |
| `ScenariiBcs.cs` | 6: asertie 2, comentariu 1, purja 3 |  |
| `ScenariiBtr.cs` | 1: ajutor 1 |  |
| `ScenariiDec.cs` | 1: asertie 1 |  |
| `ScenariiDeschidere.cs` | 1: asertie 1 |  |
| `ScenariiDvi.cs` | 6: asertie 1, oracol 5 |  |
| `ScenariiExplicatii.cs` | 1: montaj 1 |  |
| `ScenariiFct.cs` | 2: asertie 2 |  |
| `ScenariiImo.cs` | 1: asertie 1 |  |
| `ScenariiItv.cs` | 1: asertie 1 |  |
| `ScenariiLdi.Folosinta.cs` | 5: asertie 3, oracol 2 |  |
| `ScenariiNir.Acoperire.cs` | 1: ajutor 1 |  |
| `ScenariiNir.Diferente.cs` | 8: asertie 1, montaj 4, oracol 3 |  |
| `ScenariiNir.cs` | 2: oracol 2 |  |
| `ScenariiNtc.cs` | 1: asertie 1 |  |
| `ScenariiPartideCub.cs` | 5: asertie 1, montaj 4 |  |
| `ScenariiRdc.cs` | 2: asertie 2 |  |
| `ScenariiRlf.cs` | 2: ajutor 1, asertie 1 |  |
| `ScenariiSaftStocuri.cs` | 3: asertie 1, raport 2 |  |
| `ScenariiSnapshotStoc.cs` | 2: asertie 2 |  |
| `ScenariiStocCub.cs` | 1: asertie 1 |  |

**Fără nicio referință azi** (neatinse de tăiere, în afara celor două referințe indirecte numite): `ScenariiFisa.cs` (trei aserții `oracol` prin `Invarianti.Verifica`), `ScenariiConcurenta.cs` (cheamă `Invarianti.Verifica` ca probă pozitivă), `ProbeTransferCititori.cs` (numește martorul `Imobilizari.VerificaAcoperire` într-o listă, `:36`); restul fișierelor uneltei nu referă registrele, motorul vechi sau oracolul: `AscundereControlata.cs`, `CapturaSql.cs`, `DeschidereScena.cs`, `Duk.cs`, `ImoFixture.cs`, `IntegritateTph.cs`, `MetadataDump.cs`, `ModelAplicatie.cs`, `NumaratorSql.cs`, `OracolStocFizic.cs`, `PerfCub.cs`, `PerfCub.Masuri.cs`, `PerfCub.ProbaPlan.cs`, `PerfSaft.cs`, `ProbeBlocajScriere.cs`, `ProbeCititoriCub.cs`, `ProbeCulegere.cs`, `ProbeLdiOperand.cs`, `ProbeLinii.cs`, `ProbeNirOperand.cs`, `ProbeRegim.cs`, `ProbeStraturi.cs`, `ScanareGetObjectByKey.cs`, `Scenarii.cs`, `ScenariiBalanta.cs`, `ScenariiCitiri.cs`, `ScenariiFiscale.cs`, `ScenariiImo.Review.cs`, `ScenariiLdi.cs`, `ScenariiNir.Review.cs`, `ScenariiRegim.cs`, `ScenariiSaft.cs`, `ScenariiSnapshotStocReview.cs`, `ScenariiTrezorerie.cs`, `ScenariiTvaIntervale.cs`, `ScenariiVanzare.cs`, `SursaProductie.cs`, `UnicitateFiscala.cs`, `ValidareD406.cs`.

## 10. Constatări în afara verdictelor

- **Cifre care se schimbă, nu doar sursa lor.** `Program.cs:16902` citește valoarea lăsată de `MotorOperare.Valideaza` pe
  linia din memorie (10,00, golirea planului vechi); după tăiere dry-run-ul lasă estimarea (10,01). `Program.cs:17127`
  citește valoarea lăsată pe linia de consum ASM de o operare refuzată. `Program.cs:31913` și `:31924` pun în cifre
  diferența declarată N-r3 (50 azi, 75 pe evaluarea cubului). Toate patru sunt `comportament 1`.
- **Tipurile inerte pe bugetar: ce e măsurat și ce nu.** `SC-RLF-12` (`ScenariiRlf.cs:55`) și `SC-RDC-16`
  (`ScenariiRdc.cs:71`) măsoară refuzul operării FĂRĂ număr cules („politică de numerotare”), iar `SC-DVI-12`
  (`ScenariiDvi.cs:64`) refuzul frunzei DVI („TVA de import”). Cazul cu număr cules — Operat fără efect, din cod
  (`tr-d9-inventar.md`, §6) — nu e măsurat de nicio scenă. Refuzul cu un singur cod al schimbării 4 (D9-D5) are
  deci de pus în cifre cazul nemăsurat și schimbă textul potrivit de cele trei scene. Vezi 5, schimbarea 4.
- **Aserții care potrivesc textul unui refuz al planului vechi sau al validării de frunză** (nu citesc registre;
  țin de inventarul refuzurilor, D9-D4): 38c „nu are regulă de contare de vânzare / de cost pentru Tipul ei”
  (`Program.cs:1730`, `1743`, `3571`); „nu creează și nu distruge valoare” (`Program.cs:18485`) și „Valoarea produsă”
  (`ScenariiAsm.cs:337`) din invariantul valoric ASM; dimensiunile obligatorii per cont, probate numai ca refuz
  (între altele `Program.cs:4960`, `5141`, `5851`, `6010`, `6269` și apelurile de la `7666`, `8686`, `17482`).
- **Nume care scapă pattern-ului din `referinte.sh`**: tipurile `CheieStoc` și `SoldStoc` (definite în
  `Motor/StocService.cs`); câmpurile rezumatului SAF-T `RanduriRegistru`, `ValoareRegistruContabil`, `TvaRegistru`,
  `BazaRegistruAchizitie`, `BazaRegistruLivrare`, `RanduriRegistruStoc` (calculate pe cub, nume de registru).
  `RegistruStocCantitate` / `RegistruStocValoare` / `RegistruStocBate` sunt prinse de pattern și au verdictul
  `raport`; niciunul dintre aceste nume nu e în lista D9-D12.
- **Fără cititor în `Citiri.Imobilizari`**: repartitorul rândului de fișă (`RegistruImobilizari.RepartitorId`) nu e
  în `RandImobilizare`; pe cub e `Gestiune` pe postarea fișei. Atinge `Program.cs:27759` (`neclar`) și câte o
  clauză din `:26886` și `:27475`.
- **Ramură moartă**: `Program.cs:14812` (`LotCuDeschidere` scrie registrul numai la cantitate ≤ 0; singurul apel are 100).

## 11. D9-D8: aserțiile pe `RegulaStoc`

După tăiere, în seed rămân numai rândurile `RegulaStoc` ale LDI (3 pe bugetar, 2 pe privat) și DSC (2, pe privat);
ies cele de pe ASM, BCS, BTR, NIR, RDC și RLF, iar „Explică” nu mai potrivește reguli de stoc pe aceste tipuri
(`tr-d9-inventar.md`, §7). Spec-ul pasului n-a cerut aceste aserții; au fost căutate pe cele 56 de linii cu
`RegulaStoc` / `ReguliStoc` / `RegulaStocFapt` / `Potrivire.Stoc` (`referinte-regulastoc.tsv`: 50 în `Program.cs`, 4 în `ProbeLdiOperand.cs`, 2 în `ScenariiLdi.Folosinta.cs`)
și, de la ele, prin aceeași trecere pe flux de date. Liniile NU intră în lista canonică (rămâne 807).

Clasele:

- **(a)** aserția numără sau citește rânduri de seed care ies — 7;
- **(b)** citește rânduri care rămân (LDI, DSC) — neatinsă, nu e în `asertii.tsv`;
- **(c)** scrie o regulă de mână, ca montaj al unei probe care rămâne validă — 3: una pe `Potrivire.Stoc` (pură) și
  două pe gardul tabelei (`GardianEditare`), trecute aici ca variantă a clasei și marcate în notă;
- **(d)** leagă altă politică de cheia `RegulaStoc` — 1;
- **(e)** afirmă ABSENȚA regulilor pe un tip care n-are nici azi (NTC, ITV, FCL, PLT / INC, DEC, DVI, PIF / CAS / AMO;
  DSC, RLF și RDC pe bugetar): rămâne adevărată, tratată ca (b) — neatinsă, nu e în `asertii.tsv`. Clasa nu era în
  cerere; e numită ca să poată fi mutată în bloc.

### Atinse — 11 aserții (`comportament`, ținta D9-D8)

| Fișier:linie | Clasa | Aserția | Notă |
|---|---|---|---|
| `Program.cs:2984` | (a) | Seed ASM: ancoră TipDocument + numerotare ASM-; UN SINGUR set de reguli de stoc… | (a) ASM, privat: numără rândurile de seed RegulaStoc ale ASM (2: +1 pe predator, generic→Magazie, MF→Marfuri) — rândurile ies din seed; rămân clauzele de ancoră și numerotare |
| `Program.cs:3250` | (a) | Seed RLF: ancoră + numerotare RLF-; stoc +1 pe PREDATOR (generic→Magazie, MF→Ma… | (a) RLF, privat: stocRlf.Count == 2 (+1 pe predator, generic→Magazie, MF→Marfuri) — rândurile ies din seed; rămân ancora și numerotarea |
| `Program.cs:3257` | (a) | Seed RDC: ancoră + numerotare RDC-; stoc −1 pe PRIMITOR (marfa REVINE pe lotul … | (a) RDC, privat: stocRdc.Count == 2 (−1 pe primitor, generic→Magazie, MF→Marfuri) — rândurile ies din seed; rămân ancora și numerotarea |
| `Program.cs:7783` | (a) | Bugetar: ASM activ pe cub, cu stoc și numerotare; fără politici contabile/fisca… | clauza tipAsm.PosteazaInCub (D9-D5); clauza Count(RegulaStoc ASM, predator, +1) == 2 pe bugetar — (a) ASM: rândurile ies din seed (D9-D8); rămân numerotarea și lipsa politicilor contabile/fiscale |
| `Program.cs:13361` | (d) | D17-V1 (privat) politica acoperă FIECARE pereche (tip × registru) pe care regul… | (d) PoliticaMiscareSaft legată de cheia RegulaStoc: acoperirea politicii se măsoară pe perechile (tip × TipStoc) citite din RegulaStoc pentru NIR, BTR, BCS, LDI, DSC, ASM, RLF, RDC; după tăiere rămân numai perechile LDI și DSC, deci registre.Count ≠ acoperite.Count — premisa „politica numește registrele pe care regulile de stoc chiar le scriu" își pierde sursa pentru NIR, BTR, BCS, ASM, RLF, RDC |
| `Program.cs:18058` | (a) | Api ASM — precondiții de profil: ancora ASM cu seria „ASM-”, UN SINGUR set de r… | (a) ASM, privat (precondiția feliei Api ASM): reguliAsm.Count > 0 și toate +1 pe predator — rândurile ies din seed; rămân ancora, numerotarea, lipsa PoliticaTva și RegulaContare |
| `Program.cs:18767` | (a) | Api RLF — precondiții de profil: ancora RLF cu seria „RLF-”, reguli de stoc +1 … | (a) RLF, privat (precondiția feliei Api RLF): reguliRlf.Count > 0 și toate +1 pe predator — rândurile ies din seed; rămân ancora, numerotarea, PoliticaTva deductibil și TipTvaImplicit |
| `Program.cs:19247` | (a) | Api RDC — precondiții de profil: ancora RDC cu seria „RDC-”, reguli de stoc −1 … | (a) RDC, privat (precondiția feliei Api RDC): reguliRdc.Count > 0 și toate −1 pe primitor — rândurile ies din seed; rămân ancora, numerotarea, PoliticaTva colectat și TipTvaImplicit |
| `Program.cs:20660` | (c) | F23-V4 ({eticheta}) invarianții de POTRIVIRE: sursa „Explicit” fără cont (pe `P… | (c) variantă pe GARD, nu pe Potrivire.Stoc: RsNoua (20645) scrie de mână o RegulaStoc pe primul TipDocument din bază — Semn = 0 refuzat, Semn = +1 acceptat (GardianEditare.VerificaRegulaStoc); proba rămâne validă cât rămâne tabela; celelalte clauze (PoliticaTva, RegulaContare) nu depind de RegulaStoc |
| `Program.cs:21129` | (c) | F24-G1 ({eticheta}) enum-ul fără membru 0 nu se mai scrie TĂCUT: gardianul refu… | (c) variantă pe GARD, nu pe Potrivire.Stoc: două RegulaStoc scrise de mână pe primul TipDocument din bază (21103 fără Latura → refuz generic pe enum nedefinit; 21119 rând complet → acceptat); proba rămâne validă cât rămâne tabela; clauzele PoliticaTva și MapareD300 nu depind de RegulaStoc |
| `Program.cs:21233` | (c) | enunt + (divergente.Count > 0 ? $" — divergențe: {string.Join("; ", divergente)… | (c) o singură linie pentru toate apelurile Regula(…) din VerificaPotrivire; depind de RegulaStoc numai F24-P3 (21326) și F24-P4 (21341): Potrivire.Stoc pe RegulaStocFapt construite de mână (Rs, 21246), pe tipuri sintetice; proba e pură și rămâne validă cât rămâne Potrivire.Stoc (consumatorii LDI, DSC și „Explică") |

`Program.cs:7783` poartă două schimbări (D9-D5 și D9-D8) pe un singur rând; în secțiunea 5 apare la D9-D5.

### Neatinse, cu motivul

| Aserția | Motivul |
|---|---|
| `Program.cs:1372` | (b) DSC, privat — cele 2 rânduri DSC rămân în seed; neatinsă |
| `Program.cs:1831` | NTC, privat — (e) absența regulilor pe un tip care n-are nici azi: rămâne adevărată, neatinsă |
| `Program.cs:2204` | ITV, privat — (e) absența regulilor pe un tip care n-are nici azi: rămâne adevărată, neatinsă |
| `Program.cs:4916` | DSC, bugetar (textul numelui aserției) — (e) absența regulilor pe un tip care n-are nici azi: rămâne adevărată, neatinsă |
| `Program.cs:5963` | FCL, bugetar — (e) absența regulilor pe un tip care n-are nici azi: rămâne adevărată, neatinsă |
| `Program.cs:6143` | PLT / INC, bugetar — (e) absența regulilor pe un tip care n-are nici azi: rămâne adevărată, neatinsă |
| `Program.cs:6960` | PLT / INC (viramentul), bugetar — (e) absența regulilor pe un tip care n-are nici azi: rămâne adevărată, neatinsă |
| `Program.cs:7370` | DEC, bugetar — (e) absența regulilor pe un tip care n-are nici azi: rămâne adevărată, neatinsă |
| `Program.cs:7569` | NTC, bugetar — (e) absența regulilor pe un tip care n-are nici azi: rămâne adevărată, neatinsă |
| `Program.cs:17237` | NTC (precondiția feliei Api NTC) — (e) absența regulilor pe un tip care n-are nici azi: rămâne adevărată, neatinsă |
| `Program.cs:19653` | schema, nu rânduri — lista cheilor unice așteptate în model (F23-V1) cuprinde indexul RegulaStoc(TipDocumentId, Latura, ClasaId) cu NULLS NOT DISTINCT; tabela rămâne, neatinsă |
| `Program.cs:22046` | DVI, bugetar — (e) absența regulilor pe un tip care n-are nici azi: rămâne adevărată, neatinsă |
| `Program.cs:22136` | DVI, privat — (e) absența regulilor pe un tip care n-are nici azi: rămâne adevărată, neatinsă |
| `Program.cs:26666` | PIF / CAS / AMO, ambele profiluri — (e) absența regulilor pe un tip care n-are nici azi: rămâne adevărată, neatinsă |
| `ProbeLdiOperand.cs:58` | (b) LDI — SC-LDI-14 refuz INVENTAR_STOC_NEACOPERIT pe ReguliStoc gol; rămâne, neatinsă |
| `ProbeLdiOperand.cs:59` | (b) LDI — SC-LDI-14 refuz pe regula Custodie; rămâne, neatinsă |
| `ProbeLdiOperand.cs:60` | (b) LDI — SC-LDI-14 refuz pe două reguli; rămâne, neatinsă |
| `Program.cs:7813` | RLF și RDC, bugetar, prin ajutorul `Inert` (`:7806`) — (e) absența regulilor: rămâne adevărată, neatinsă |
| `Program.cs:21516` | „Explică” pe FCT × stoc: `Stoc.Length == 0` — (e) FCT n-are reguli de stoc nici azi |
| `Program.cs:21559` | „Explică” pe LDI × stoc: o latură, cu reguli — (b) LDI rămâne |

### Montaje, ajutoare și restul liniilor

| Fișier:linie | Verdict | Ce face |
|---|---|---|
| `Program.cs:413` | raport | tipărește numărul de rânduri ReguliStoc din bază (diagnosticul de pornire); cifra scade după D9-D8, nicio aserție |
| `Program.cs:7806` | ajutor | Inert(tip): fără RegulaStoc / RegulaContare / politici; chemat de 7813 pe RLF și RDC, bugetar — (e) absența regulilor pe un tip care n-are nici azi: rămâne adevărată, neatinsă |
| `Program.cs:16691` | montaj | (a) BCS — citește rândurile BCS (predator, semn < 0) ca să afle cheia TipStoc a scenei VerificaValoareIesire; rândurile ies, deci `.First` aruncă; premisa moare odată cu cheia de registru (TipStoc devine contul lotului); aserțiile din aval au deja verdict în asertii.tsv |
| `Program.cs:20645` | ajutor | RsNoua: scrie de mână o RegulaStoc pe primul TipDocument din bază — (c) variantă pe gard → 20660 |
| `Program.cs:20646` | ajutor | RsNoua → 20660 |
| `Program.cs:21103` | montaj | 21129: (c) variantă pe gard — RegulaStoc scrisă de mână fără Latura (refuzul generic pe enum nedefinit) |
| `Program.cs:21119` | montaj | 21129: (c) variantă pe gard — RegulaStoc scrisă de mână, rând complet (acceptat) |
| `Program.cs:21246` | ajutor | Rs: construiește de mână o RegulaStocFapt pe tipuri sintetice — (c) → 21233 (F24-P3, F24-P4) |
| `ProbeLdiOperand.cs:31` | montaj | (b) LDI — RegulaStocFapt de mână în operandul sintetic al DeclarantDiferenteInventar (consumator care rămâne) |
| `ScenariiLdi.Folosinta.cs:103` | montaj | (b) LDI — citește regula LDI × clasa OF (predator) ca să-i schimbe TipStoc; rândul rămâne în seed |
| `ScenariiLdi.Folosinta.cs:105` | montaj | (b) LDI — schimbă de mână TipStoc-ul regulii pentru fixture-ul SC-LDI-22 (cheia istorică de REGISTRU; aserțiile scenei au deja verdict în asertii.tsv) |

Celelalte 9 linii sunt comentarii, texte de măsurătoare tipărite sau numele unei aserții.

### Constatări

- **Niciun rând de seed al BCS, BTR sau NIR nu e numărat de o aserție.** Rândurile care ies sunt citite ca aserție
  numai pe ASM, RLF și RDC (seed-ul privat și precondițiile feliilor Api) și pe ASM bugetar. Pe BCS există un singur
  cititor, montajul de la `Program.cs:16691`: ia din rândurile BCS cheia `TipStoc` a scenei `VerificaValoareIesire`,
  iar `.First` aruncă fără ele. Premisa moare oricum odată cu cheia de registru.
- **„Explică” pe tipurile care își pierd regulile n-are nicio aserție.** Secțiunea de stoc a lui `ExplicaApply` e
  citită în două locuri (`Program.cs:21516` pe FCT, `:21559` pe LDI), amândouă neatinse. Schimbarea de răspuns pe
  BCS, BTR, NIR, ASM, RDC și RLF, numită în `tr-d9-inventar.md` §7, nu e probată azi de nimic.
- **`Program.cs:13361` (d)** e singura aserție care leagă altă politică de `RegulaStoc`: premisa „`PoliticaMiscareSaft`
  numește registrele pe care regulile de stoc chiar le scriu” are, după tăiere, perechi numai pe LDI și DSC. Ori
  primește altă sursă a perechilor (conturile de stoc pe care declaranții postează), ori cade.
- **Probele pe gard** (`Program.cs:20660`, `:21129`) scriu regula pe primul `TipDocument` din bază, oricare ar fi;
  rămân valide cât rămâne tabela, indiferent de tipul nimerit.
- `ProbeLdiOperand.cs` și `ScenariiLdi.Folosinta.cs` ating numai reguli LDI (b); fixture-ul `SC-LDI-22` schimbă de
  mână `TipStoc`-ul regulii ca să producă o cheie istorică de REGISTRU — aserțiile lui au verdict în `asertii.tsv`.

## 12. Trecerea pe flux de date: ce a adus și ce a respins

Candidat = aserție purtătoare (secțiunea 1) fără rând în `asertii.tsv`. 22 de candidați pe sursele lărgite, în
toate fișierele inventariate; 15 dintre ei apar și pe sursele canonice (toți, în afara rândurilor `SnapshotStoc`
și `Invarianti.Verifica`).

| Candidat | Urma | Hotărârea |
|---|---|---|
| `Program.cs:23483` (SOL-C0) | `StocService.VerificaSoldIntermediar` (`:23458`, `:23460`) → `c[…]` → `Citiri` → `citiriDeschis` | **adăugată**, `regula` → `Citiri.Loturi.VerificaSoldIntermediar`; clauza `StartsWith(„Sold negativ”)` nu mai ține pe gardul cubului, care pune codul în față (`Cub/Citiri/Loturi.cs:100`) |
| `Program.cs:23491` (SOL-C0b) | același dicționar `citiriDeschis` | respinsă: cheia citită, „sold-stoc-lot-golit”, vine din `StocProiectii.SoldStoc` (proiecție pe cub); dicționarul e purtător în bloc |
| `Program.cs:14191`, `:14277` | câmpurile `RegistruStocBate` / `RegistruStocValoare` | respinse: câmpuri ale rezumatului SAF-T, calculate pe cub (verdict `raport` pe referință) |
| `Program.cs:21740`, `:21780`, `:21964` | ajutoarele `Lv` / `NumeAfisat` din `VerificaD85` | respinse: ajutorul e purtător numai prin apelul cu textul `RegistruStoc_ListView` (`:21921`, care are verdict); aserțiile citesc alte ListView-uri |
| `Program.cs:23556`, `:23564`, `:23610`, `:23664` | `SnapshotStoc` / `AsteptatStoc` / `EgalStoc` | respinse: datele vin din `SoldPerioadaStoc` și din cub; al serviciului vechi e numai tipul purtător `SoldStoc` (ajutoarele sunt acum în `ajutoare.tsv`) |
| `Nucleu/ProbeCub.cs:211`, `:243` | `MultisetEgal` | respinse: ajutorul e purtător prin apelul din `STR-ORACOL` (`:195`, `oracol`); aici compară cubul citit cu contractul și cu `Storno.Inverseaza` |
| `ProbeAsmOperand.cs:46`, `:47`, `:49`, `:67` | `Declara(Operand(…))` | respinse ca verdict, semnalate: operandul sintetic poartă `SolduriLoturiRegistru` (montaj, `:31`), dar aserțiile (determinismul declarației, refuzurile de structură `SC-ASM-13`) nu citesc absorbția — se schimbă montajul, nu aserția |
| `ScenariiExplicatii.cs:149` | `json` ← `explicatie` (cu o decizie `AbsorbtieEvaluare`, `:132`) | respinsă: același fixture ca `:146` (care are verdict), dar clauza înlocuiește `ValoareIesire` și `PerioadaDeschisa`, nu atinge decizia care moare |
| `ScenariiSaftStocuri.cs:322` | câmpul `RegistruStocBate` | respinsă: rezumatul SAF-T pe cub, ca mai sus |
| `ScenariiConcurenta.cs:289`, `ScenariiFisa.cs:92`, `ScenariiPartideCub.cs:418` | `Invarianti.Verifica` | respinse: proba blocajului de scriere și invarianții interni ai cubului (`CITIRE_CUB_DEZECHILIBRAT`, `CITIRE_PARTIDE_INCOMPLETE`), care rămân |

O corectură pe un rând existent, din aceeași urmă: `Program.cs:24013` (DIR-V10) potrivește „Sold negativ (-1) la …”;
registrul formatează soldul cu `0.####`, cubul îl scrie neformatat și cu codul în față, deci clauza se verifică la
re-țintire (nota din `asertii.tsv` spunea „același text”).

