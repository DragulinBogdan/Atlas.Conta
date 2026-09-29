# TR-D8 — SAF-T pe cub: delimitare aprobată și contract S1

**2026-09-28 — SAF-D1/D2/D4=A și SAF-D3=C aprobate de owner.
S1 aprobat de owner cu S1-D5 = B' (381 + 384); review-ul și regulile
de derivare sunt în [S1-R](#s1-r--review-și-tranșări-2026-09-28).**

**2026-09-29 — S0 implementat: manifest pin-uit, XSD v249 cu substituție
declarată, DUK fără `SĂRIT` și cu perioada din antet; regulile și măsurătorile
sunt în [S0-R](#s0-r--artefactele-validării-fixate-2026-09-29).**

**Review advers S0, reverificat pe `00d8845`: S0-RV1 și S0-RV1.1 închise.
Mutanții de asociere sunt respinși; hărțile originale și santinela 2025-09
trec. XSD/DUK rămân reverificate. Fără constatări noi în corectură.**

**2026-09-29 — S0 aprobat de owner:** scena pe 2040 cu santinela 2025-09
(S0-R6) și scriitorul care emite secțiunile fără intrări goale, fără
totaluri, inclusiv pe ruta publică existentă (S0-R5). S0 este închis;
comutarea L rămâne la S2 (R1).
[Raportul Codex](tr-d8-saft-s0-review-codex.md).

**2026-09-29 — S3 (MovementOfGoods, PhysicalStock, comutarea C) propus de
Claude; owner: S3-Q1 = A (NIR delta = 10 semnat, cheia fără `Cauza`), S3-Q2 =
A (Δ ASM acceptat); review-ul Codex cerut:** [S3](#s3--movementofgoods-physicalstock-și-comutarea-c-contract-pentru-aprobare).
**2026-09-29 — S2 închis: review Codex S2-RV1 închis după reverificare.**
**2026-09-29 — S2 (Payments + comutarea L) propus de Claude; owner: S2-Q1 =
stornoul neagă liniile declarate, S2-Q2 = compensarea în afara S2 (SAFT-r1),
implementare în paralel cu review-ul Codex:** [S2](#s2--payments-și-comutarea-l-contract-pentru-aprobare).

Răspunde cererii din `comunicari/2026-09-28-0115-claude-codex-saft-sourcedocuments-contract.md`.
Bază inspectată: `faa8b2d`. Contracte existente: 073, 074, 090, 091,
103 și [D8-B1…B8](tr-d8-citiri-contract.md).

Alegerile owner-ului sunt consemnate în mesajul
`comunicari/2026-09-28-0845-claude-codex-saft-alegeri-owner-review.md`.
Felia are trei subfelii: **S1 GL + facturi; S2 Payments;
S3 MovementOfGoods + stocuri**, cu gate final comun. Contractul executabil
propus pentru S1 este la final; S2 și S3 vor primi detalierea proprie.

## SAF-B1 — delimitarea aprobată

| Id | Alegere owner | Regula |
|---|---|---|
| SAF-D1 | A | S1/S2/S3 cu gate final comun; portăm secțiunile L și stocuri existente |
| SAF-D2 | A | Assets/AssetTransactions au contract separat și rămân F26-r8 |
| SAF-D3 | C | `TipStoc` este taxonomie derivată din contul istoric; se păstrează politica 074, extinsă cu `Cauza?`; detaliere mai jos |
| SAF-D4 | A | Fără acces complet la datele și membrii necesari, exportul se refuză; nicio proiecție parțială descărcabilă |

**R1 — comutarea pe fișier:** L comută numai după S1 + S2 certificate;
C (denumirea din HeaderComment a fișierului de stocuri) după S3 certificată.
Între timp rămâne ruta veche pentru fișierul respectiv. Ruta veche are deja
unele citiri fiscale pe cub; această stare existentă nu este prezentată ca
export pur pe registre. Nu construim însă un fișier intermediar cu noul GL
și vechiul Payments. Noul export de fișier folosește integral cititorii comuni
de cub pentru măsurile migrate. S1 produce artefacte de probă, fără comutarea
rutei publice L. Proba SC-SAFT-15 verifică explicit această limită.

Scrierea registrelor rămâne până la TR-D9. Import1C rămâne înghețat.
091-r3 se lucrează separat; nu duplicăm testul de arhitectură. Această
felie nu închide restanțe prin documentare. SAF-D3 amendează explicit modelul
politicii 074, SAF-D4 delimitarea exportului din 073; niciuna nu aprobă încă
mapările ANAF ori alegerea de corecție fiscală din S1-D5.

## SAF-B2 — inventar verificat și intrări comune

Căile de cod de mai jos sunt relative la
`nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/`.
Cititorii marcați **nou** sunt necesități de contract, nu API-uri existente.

| Secțiune / suprafață | Sursa actuală | Intrarea pe cub / producători și delimitare |
|---|---|---|
| Header, nomenclatoare, TaxTable, UOM, Products, AnalysisTypeTable | Configurație, nomenclatoare, mapări fiscale | Rămân metadate autorizate. Nu inventăm măsuri din valorile curente ale produsului/lotului |
| GeneralLedgerAccounts; solduri Customers/Suppliers | `ContabilProiectii.Atomi`, deja pe cub | `Citiri.Contabil`; deschidere și toate operațiile contabile. `RolTert` rămâne clasificare comercială, nu se înlocuiește cu `UrmarestePartide` |
| GeneralLedgerEntries | `RegistruContabil`, grupare pe document; DataOperare pentru SystemEntryDate | `Citiri.Contabil.Postari`; toate operațiile D8-B2, inclusiv DEC, AMO, CAS, ASM Operare. Transfer/PIF nominalizare nu creează rulaj; Carte Fiscal nu intră |
| TaxInformation din GL și facturi | `Fiscale.Fapte/Postari`, deja pe cub, alăturate unor rânduri din registre/documente | Păstrăm cititorii fiscali; asociere pe eveniment și linie. Cheia `(Document, Linie, Storno)` singură nu distinge toate corecțiile |
| SalesInvoices | FCL/RDC: registru + fapte fiscale + valori ale detaliilor | **Nou:** proiecție comună a liniilor fiscale și contabile pe eveniment. FCL/RDC produc faptele; DSC nu devine factură |
| PurchaseInvoices | FCT/RLF: aceeași combinație; cont căutat și pe conex/lot/recepție | Același cititor **nou**, din FCT/RLF. FCT postează deja pe contul de intrare: nu căutăm un NIR viitor pentru contul facturii. DEC/DVI nu devin artificial facturi |
| Payments | PLT/INC: registru, valori ale detaliilor, `Imperechere` curentă | **Nou:** evenimente de plată din `Contabil.Postari`, alocări istorice din `Partide.Postari/Origini` și legăturile tranzacțiilor. Transferul dintre partide nu este o nouă plată |
| MovementOfGoods | `RegistruStoc`; politică tip document × TipStoc × semn; cont din tipul materialului curent | `Loturi.Postari/Miscari`, cu identitatea tranzacției și proveniența liniei păstrate printr-o intrare comună **nouă**. FCT, NIR delta, BTR, BCS, DSC, RDC/RLF, ASM, LDI și inversele lor |
| PhysicalStock și agregatele inițial/rulaj/final | `AgregatStoc` pe RegistruStoc; lot/preț/cont din nomenclatorul curent | `Loturi.Cumulate/Solduri`; cheia completă lot × cont × produs × gestiune. Deschiderea intră în inițial, fără a dubla soldul contabil |
| ComponenteStoc și controalele S1…S5 | Încă agregă RegistruStoc, apoi compară cu contabilul | Aceleași intrări comune de loturi și contabil. Controalele nu rămân o dependență ascunsă de registre |
| Assets / AssetTransactions | Nu există export economic implementat; `SaftXml` scrie Assets gol | SAF-D2; PIF/AMO/CAS sunt producători pe cub, dar existența lor nu livrează secțiunea anuală |

Sursa inventarului: `Saft/SaftProiectii.cs`, `Saft/SaftXml.cs`,
`Cub/Citiri/{Contabil,Partide,Loturi,Fiscale,Transformare}.cs`,
`Declaratii/DeclarantFacturaIntrare.cs`, controllerul SAF-T din WebApi.

Regimul dual rămâne numai pentru scriere și comparația A/B de verificare.
Nu folosim registrele ca fallback al noului export. Metadatele documentului
sunt admise pentru identificare/descripție; sumele, conturile și taxa sunt
din cub. Cantitățile fizice sunt din cub; cantitățile comerciale fără fapt
de stoc au sursa distinctă, explicită, propusă în S1-D4.

## SAF-B3 — identitate, timp și două blocaje de specificație

1. GL păstrează identitatea tranzacției și a postării `(Spatiu, ID)`.
   Gruparea XML nu poate uni două operări/corecții ale aceluiași document.
   Soldul și rulajul folosesc data contabilă, nu PerioadaDeclarare.
   **R2:** SystemEntryDate provine din `Tranzactie.ScrisLa`, scris UTC de
   `Materializare.Scrie`, inclusiv pentru inversă și noua operare a corecției.
   Este data UTC a acelui timbru, fără conversie după fusul mașinii care
   exportă. Reper absent = diagnostic, nu ultima DataOperare/data exportului.
2. **Corecție tehnică versus factură de corecție:** 073 tratează stornoul
   prin 381; 103 distinge inversa tehnică în fiscal, fără să tranșeze automat
   D406. Nu extindem regula D394 prin analogie. Înainte de S1 trebuie fixate
   separat InvoiceType, InvoiceDate, TaxPointDate, perioada de includere și
   referința la original pentru storno tehnic, retur și corecție legată.
   Recomandarea de model este păstrarea identității fiscale și evitarea
   unei facturi juridice inventate pentru fiecare inversă tehnică. Alternativa
   este păstrarea reprezentării pe eveniment din 073. Alegerea finală cere
   verificare normativă și owner; efectul asupra declarației unei luni deja
   raportate este arătat numeric în **S1-D5**, cu variantele A/B.
   **Această alegere blochează aprobarea și implementarea S1.**
3. Payments citește alocările la capătul perioadei, fără legături viitoare.
   PLT 70, din care 50 alocați unei facturi, nu poate exporta 70 drept
   achitare a acelei facturi. Propun 50 cu referință și 20 fără referință;
   **R4:** în XSD-ul public v249, calea Payments/Payment/PaymentLine/
   SourceDocumentID are `minOccurs="0"` (liniile 1413–1414). Aceasta este
   verificare preliminară; S2 cere aceeași verificare în XSD-ul fixat la S0
   și validarea DUK a variantei 50 + 20. Dacă profilul o respinge, oprim
   această formă, fără referință inventată, și revenim cu alternativă. Alocarea
   nominalizată la operare se numără o dată, chiar dacă există și legătura.
   Contabilizarea valutei și diferențele de curs se verifică separat de
   valoarea plății. Transferul bancă proprie → bancă proprie rămâne în GL,
   fără a fi transformat în plată către un terț.
4. La stoc folosim contul istoric și valoarea semnată; nu `Lot.PretUnitar`
   curent pentru recalcularea valorii. Transferurile reale de lot și inversele
   lor sunt necesare la stoc, deși sunt excluse din GL.
5. **Mapările S3:** FCT produce intrarea, NIR egal produce zero, NIR delta
   produce numai diferența. Politica actuală are intrare pe NIR și nu are
   `TipStoc` disponibil în cub. Înainte de S3 trebuie aprobat un tabel
   exhaustiv tip document × categorie derivată × semn × cauză opțională
   → cod ANAF sau excludere motivată, în aceeași `PoliticaMiscareSaft`,
   inclusiv PeDrum și mișcarea numai valorică. Nu alegem acum arbitrar
   codurile 100/101 ori 110/120 pentru orice delta NIR/ASM.
6. Cheile XML de mișcare și poziție de stoc trebuie să fie stabile și
   injective inclusiv pentru același lot pe două conturi, două gestiuni cu
   același nume trunchiat și mai multe evenimente ale documentului. Algoritmul
   exact și lungimile XSD sunt de pin-uit în S3; numele nu este identitate.

### SAF-D3=C — taxonomie și politica existentă

Contul istoric și măsurile semnate rămân din cub. Se propune proprietatea
nullabilă `Cont.CategorieStoc`: null înseamnă moștenire de la cel mai
apropiat părinte cu valoare explicită; lipsa pe întregul lanț înseamnă
neclasificat și refuz pentru o mișcare raportabilă pe lot. Fără implicit
Magazie, fără citirea `ClasaProdus` curentă sau `RegulaStoc.TipStoc`.
Moștenirea aceasta este regulă nouă aprobată, nu afirmația că `RolTert`
are deja un resolver identic în cod.

`TipStoc` rămâne temporar și enumerare de intrare pe RegulaStoc, și
taxonomie de ieșire; la TR-D9 rămâne numai taxonomia. Clasificarea este
editabilă pe planul de conturi, fără release. Magazie/Mărfuri și PeDrum se
detaliază la S3 pe Privat; Folosință/Custodie/Gratuit nu se rezolvă aici.
Contul istoric este stabil, dar modificarea categoriei lui poate reclasifica
un reexport: manifestul păstrează configurația efectivă și hashul ei; nu
pretindem că noua taxonomie este istorizată în Postare.

Cheia politicii devine `(TipDocument, TipStoc, Semn?, Cauza?)`.
Null înseamnă orice pe cele două filtre. Seed, gardian, unicitate, OData și
ecranul politicii se extind împreună; nu apare un tabel paralel. Propunere
de selecție pentru S3: câștigă regula strict mai specifică; două potriviri
incomparabile, de exemplu semn exact/cauză null și semn null/cauză exactă,
produc refuz de ambiguitate, nu alegerea primei. Cod null continuă să ceară
motiv explicit de excludere. Acest resolver rămâne pentru review-ul S3.

Proba duală compară categoria derivată cu TipStoc pentru fiecare rând vechi,
prin corespondența economică document/linie/origine/lot/gestiune/sens.
Nu presupune aceeași cardinalitate: recepția veche NIR poate corespunde
FCT, iar o contrapondere fără lot poate să nu aibă poziție fizică echivalentă.
Raportează rând cu rând egalitate, diferență, absență sau ambiguitate, cu
postările candidate și motiv. Nu folosește TipStoc vechi pentru a produce
categoria nouă și nu declară lipsa corespondentului drept egalitate.

### Propunerea `CauzaMiscareSaft` și limita de derivare

Valorile de mai jos sunt propuse, nu cod/seed aprobat. Inversa moștenește
cauza originii prin `(InversaDinSpatiu, InversaDinId)`; semnul politicii
este al originii, măsurile exportate sunt ale evenimentului curent.

| Valoare propusă | Distincție observabilă |
|---|---|
| ReceptieFactura | Lot din FCT, pe evenimentul de operare |
| DiferentaReceptie | Lot din NIR delta; NIR egal nu are eveniment economic |
| TransformareConsum / TransformareProdus | ASM, capăt real cu lot, cantitate originală negativă/pozitivă; contraponderea se exclude structural |
| TransferStoc | Transfer real de lot în afara transformării ASM; originea decide și pentru inversă |
| AjustareValorica | Pe lot, cantitate 0 și valoare nenulă, după excluderile structurale |
| Obisnuita | Celelalte mișcări cunoscute, clasificate și prin TipDocument; nu fallback pentru o cauză necunoscută |

Ordinea de clasificare propusă: eliminare contrapondere, rezolvare origine,
mișcare numai valorică, ASM, FCT/NIR, Transfer, restul catalogului cunoscut.
PeDrum este în primul rând categorie a contului, nu cauză dedusă din nume.

**Blocaj S3 identificat:** nu putem propune onest `DeltaAsm` drept cauză
separată. `DeclarantAsamblare` calculează `P + ajustări`, apoi persistă
valoarea finală cu cantitatea produsului. `AbsorbtieEvaluare` rămâne decizie
a contractului în memorie, nu fapt distinct persistat. Două istorii,
P=100/Δ=0 și P=99/Δ=1, pot produce aceeași postare +1/+100. Din ea nu se
poate recupera contribuția Δ. Nu o deducem din prețul curent sau din registru
și nu adăugăm coloane pe Postare. Oprim detalierea acestei clasificări și
cerem review al limitei: pentru cub este mișcare de transformare la valoarea
istorică finală, nu două mișcări identificabile.

Similar, o subcauză NIR precum InClarificare/Imputabila nu este garantat
injectivă în conturile configurabile; `ContRezolvat` nu persistă eticheta
deciziei. `DiferentaReceptie` este derivabilă, subcauzele trebuie dovedite
individual înaintea unei mapări care le cere. Nu introducem valori de enum
pe care cititorul nu le poate determina. Aceste limite blochează partea
respectivă S3, nu pregătirea S1/S2.

### Candidați ANAF pentru review-ul S3, fără fixarea codurilor

Sursa nomenclatorului: workbook-ul ANAF cu hash din SAF-B6, foaia
`Nomenclator stocuri`, transcrisă în
[structura D406](../api/d406-structura-2026.md). Codul existent în nomenclator
nu dovedește singur aplicabilitatea lui unei cauze Atlas. Tabelul de mai jos
este propunere pentru owner, nu seed aprobat și nu ocolește blocajele de derivare.

| Caz numeric | Candidat și limita recomandării |
|---|---|
| FCT intră 10 buc/100; NIR ulterior confirmă 10/100 | 10 Achiziție pe FCT; NIR nu emite mișcare suplimentară |
| NIR constată 2 buc/20 suplimentare, cu datorie furnizor 408 de 20 | 10 Achiziție, dacă sensul economic este bun primit suplimentar de la furnizor; nu 110 automat numai fiindcă se numește „Plus” |
| Reclasificare în PeDrum 2 buc/20 și descărcare corespunzătoare a poziției de origine | 80 Transfer intern este candidat numai dacă raportăm o mutare între poziții de stoc; contul 35x singur nu dovedește transferul fizic. Necesită martor complet S3 |
| Ajustare de preț identificabilă separat: q=0, v=+5 ori −5 | 100/101 Diferență de preț pozitivă/negativă numai dacă natura este demonstrată; q=0 nu este suficient — și capitalizarea ulterioară are cod distinct 90 |
| Inventar fizic: +2/+20 ori −2/−20 | 110/120 pentru plus/minus de inventar, nu automat pentru orice NIR cu diferență cantitativă |
| ASM produs +1/+100 cu P=99 și Δ=1 | 20 Producție pentru mișcarea produsului, potrivit politicii ASM existente; fără linie suplimentară 100 pentru Δ=1, care nu există distinct în cub. Această limită cere acceptarea owner-ului la S3 |

NIR minus în clarificare, imputarea și eventualele pierderi nu primesc
cod generic 120/180 pentru a umple tabelul. Fără distincție demonstrabilă
a cauzei, propunerea se oprește conform cererii owner-ului; alegerea codului
rămâne deschisă, iar exportul acelor cazuri nu poate fi certificat.

## SAF-B4 — restanțe și limitele închiderii

| Restanță | Tratament propus și proba de ieșire |
|---|---|
| T-r14 | Refolosim `Transformare.FaraContrapondere`; GL nu include nici Transfer. MovementOfGoods păstrează mișcările reale ASM și exclude numai contraponderile structurale. Nicio excludere nominală a întregului ASM |
| N-r8 | `Contabil.Postari` exclude deja inversele Transfer prin originea fiecărei postări. Probăm consumatorul SAF-T, inclusiv ASM mixt; nu introducem un al doilea filtru simplist `Fel != Transfer`. La loturi inversele Transfer rămân necesare |
| FZ-r3 | Indexul `(Unitate, Data)` pe partiția Contabil se decide/probează pe SQL-ul real al alocării Payments, cu plan și buffers. Inventarul migrației inițiale are indexuri Data și Partener/Cont/Data, nu perechea cerută. Existența fizică se verifică pe baza probei; nu o deducem din surse |
| TR-r7 | Bugetar este explicit neaplicabil SAF-T prin `SeAplica`; probăm refuzul L/S. Aceasta delimitează SAF-T, nu rezolvă Gratuit/Custodie în domeniu |
| B-r8 / S-r3 | Nu creăm nomenclatoare/FK pentru capete virtuale doar ca să le putem exporta. Pozițiile fizice cer gestiune reală; un lot raportabil cu gestiune nerezolvată produce diagnostic. Dacă S3 are nevoie de un capăt virtual nominal, revenim la contract înainte de implementare; restanțele generale rămân deschise |
| T-r11 / FZ-r7 | Carte Contabil pentru GL/solduri; DVI bază fiscală exclusă din rulaje. TaxInformation se leagă de linia și evenimentul fiscal, fără a folosi perioada D300 ca dată contabilă |
| F27-r10 | Portăm și inițialul, păstrând semantica de includere a pozițiilor fără rulaj curent. Nu copiem numărul rândurilor vechi drept criteriu economic fără probă |
| F26-r8 / 091-r3 | Assets conform SAF-D2; arhitectura accesului la Postare rămâne în lucrarea separată |

Închiderea locală SAF-T nu închide automat T-r14/N-r8/T-r11 în alți cititori.

## SAF-B5 — scenarii și echivalență

[SAFT.md](scenarii/SAFT.md) fixează prima matrice numerică independentă.
Rândurile cu alegere nerezolvată rămân explicit blocate, fără rezultat XML
pretins. După delimitare, fiecare subfelie completează probele nominale
și referința exactă XSD/nomenclator înainte de cod.

Comparația A/B rulează pe aceeași bază și același reper de citire. Raportul
arată secțiune, document, tranzacție, linie, cont/unitate, valori vechi/noi,
diferență și regula care o explică. Comparăm semantic; separat verificăm
unicitatea și stabilitatea cheilor XML. Nu cerem egalitate byte-cu-byte.

Diferențe explicabile: Operare ASM aprobată D8-B4; primirea la FCT versus
NIR în vechiul registru; contul istoric față de contul curent al materialului;
alocarea parțială Payments față de atribuirea întregii plăți unei singure
facturi. Fiecare cere martor numeric, nu toleranță globală pe document.
O diferență de identitate/perioadă fiscală devine explicată numai după
alegerea SAF-B3.2. Lipsa provenienței, duplicarea facturii, sumele pierdute,
un cont presupus ori un rest inexplicabil sunt defecte, nu regim dual.

## SAF-B6 — securitate și validare

Probe pe host viu pentru cele patru rute de sumar/XML lunar și stocuri:
Admin complet; lipsă drept pe Postare; refuz condițional de rând; refuz pe
Valoare/Partener și pe metadatele necesare. O declarație finală incompletă
trebuie refuzată înaintea primului byte XML; nu este suficient un total zero
obținut după filtrarea rândurilor ascunse. Nu ocolim securitatea pentru a
enumera în diagnostic datele ascunse. SAF-D4 stabilește forma produsului;
mecanismul de verificare a accesului complet trebuie probat separat.

**R3:** apelantul deschide o singură `Fiscale.DeschideCitirea(os)` înainte
de prima interogare și materializează toate secțiunile fișierului în aceeași
tranzacție RepeatableRead, pe același ObjectSpace securizat. Cititorii interni
o reutilizează; nu o închid individual. Helperul reutilizează orice tranzacție
existentă, deci apelantul nu poate accepta tacit una ReadCommitted: o
tranzacție ambiantă trebuie să ofere cel puțin garanția RepeatableRead.
Nu construim alt mecanism de snapshot. Proba concurentă
operează între citiri și cere fie imaginea de dinainte, fie cea de după,
niciodată GL vechi cu Payments/stoc nou. Snapshotul global nu înlocuiește
citirea autorizată. SQL-ul de diagnostic din `Citiri.Receptii` nu devine
implicit API securizat pentru export.

Validarea cere XML L și S, XSD, referințe interne, nomenclatoare și DUK.
Lipsa mapării, cotei/reperului temporal obligatoriu sau contului produce
diagnostic cu proveniență accesibilă și refuzul fișierului final; nu cod TVA
nefiscal implicit, cont dedus dintr-un document viitor sau zero plauzibil.

Manifestul validării fixează versiunile, namespace-ul și SHA256 pentru
XSD, nomenclator și validator. Inventarul local la redactare:

- `RO_SAFT_SchemaDefCod_16.02.2026.xlsx`: SHA256
  `050508BF6EADBD6BB4684A0016296253C629D4E9B54A1118DD9B9BFD0C1D0F2D`.
- DUK local declară `D406;J2.2.8;P2.0.1`; `D406Validator.jar`: SHA256
  `197FD169721022C43F8A3AE5BA66950A7DC2354CF1355B0DD012CEADA9A32373`.
- [Pagina ANAF D406](https://static.anaf.ro/static/10/Anaf/Declaratii_R/406.html)
  trimite la [XSD v249](https://static.anaf.ro/static/10/Anaf/Declaratii_R/AplicatiiDec/Ro_SAFT_Schema_v249_2025.xsd).
  Fișierul public are `targetNamespace` cu `d406t`, în timp ce exportul
  aplicației folosește `d406`. Compatibilitatea trebuie rezolvată explicit;
  nu declarăm validarea producției prin schimbarea tacită a namespace-ului.

**Gate S0 neîndeplinit** (la redactare; închis de
[S0-R](#s0-r--artefactele-validării-fixate-2026-09-29)): fixarea artefactului XSD utilizabil și verificarea
coerenței kitului/nomenclatorului. DUK existent permite `SĂRIT` și fallback
fără validarea perioadei; acestea nu sunt acceptabile pentru închiderea
feliei. Înregistrăm hashurile și după rulare pentru a detecta auto-update.
Nicio validare XSD/DUK nu a fost rulată în pregătirea acestui document.

## SAF-B7 — performanță

A/B pe aceeași bază izolată, aceeași lună, aceleași drepturi și același
conținut economic; separat rece/cald. Măsurăm exportul complet L și S:
durată totală, SQL round-trips/durată/rânduri, memorie maximă și alocări,
dimensiune XML, durată serializare; timpul DUK separat. Păstrăm planul
interogării Payments și comportamentul la creșterea istoricului de stoc.
Un serializator streaming nu dovedește memorie limitată dacă DTO-urile
materializează tot istoricul înainte de scriere.

Nu fixăm aici pragul transversal. Totuși N+1 pe factură/plată, încărcarea
întregului istoric pentru fiecare linie și imposibilitatea măsurării pe
exportul complet sunt blocante ale feliei, nu amânări sub eticheta perf.

## SAF-B8 — oprire și livrabile

**R6 — ordinea blocajelor:**

- S0 (namespace, artefacte pin-uite și DUK fără SĂRIT/fără pierderea perioadei)
  blochează certificarea oricărei secțiuni.
- B3.2/S1-D5 și aprobarea contractului S1 blochează implementarea S1;
  alegerea SAF-D1…D4 este deja închisă, nu se cere din nou.
- S2 se poate detalia în paralel cu alegerea S1-D5. Forma Payments fără
  SourceDocumentID are precondiția R4 de mai sus.
- B3.5 și imposibilitatea derivării cauzelor distincte blochează S3;
  codurile ANAF necesită în continuare alegere owner, cu sursă și cifre.

Nu extindem pe ascuns modelul pentru a satisface XML-ul.

La ieșire: toate cele trei subfelii, matrice numerică verificată, raport A/B
cu fiecare diferență clasificată, XML L/S validat cu manifest, probe HTTP,
măsurători complete și review advers al feliei. Nu închidem TR-D8 integral:
reconcilierea/auditul transversal, bugetul perf și inventarul complet T-r11
își păstrează gate-urile. Nu se face implementare pe baza acestei versiuni.

## S1 — GL și facturi: contract pentru aprobarea implementării

Stare: **pentru review; S1-D5 cere alegerea owner-ului**. D-urile de mai jos
fixează propunerea executabilă, fără a prezenta alegerea restantă ca aprobată.
Nu schimbă motorul, scrierea registrelor, Payments sau politica mișcărilor.

### S1-D1 — intrările și domeniul

GL și soldurile consumă `Citiri.Contabil.Postari`, faptele fiscale consumă
`Citiri.Fiscale.Fapte/Postari`. Proiecția comună de evenimente/linii SAF-T
se adaugă în `Cub/Citiri`; `SaftProiectii` o compune cu nomenclatoarele
autorizate. Nu declarăm că acest API nou există deja.

GL: toate postările contabile economice din intervalul `[început, sfârșit)`;
Transfer și inversele lui sunt excluse prin cititorul comun, la nivel de
postare. Deschiderea alimentează soldul inițial conform graniței comune
D8-B1, nu devine factură sau o nouă operare de achiziție în lună. Pozițiile
din deschidere fără DocumentId rămân în sold și reconciliere, nu se pierd
prin inner join cu Document. Partidele/fisele nu multiplică suma contabilă.

Facturi: FCT/RLF la cumpărări și FCL/RDC la vânzări; sunt incluse și
liniile nefiscale/fără taxă, fără a cere un fapt TVA pentru existența lor.
DSC, DVI, DEC, NTC, AMO/CAS rămân în GL dacă au efect contabil; nu primesc
facturi artificiale. Bugetar: refuz explicit de neaplicabilitate.

### S1-D2 — identități și proveniență (R5)

- Tranzacția GL: `TranzactieId`, global unic; nu DocumentId.
- Postarea GL: `(TranzactieId, Spatiu, ID)`. RecordID poate fi ordinal
  determinist în tranzacție, ordonat pe `(Spatiu, ID)`; manifestul leagă
  ordinalul de cheia completă, fără coliziuni între partiții.
- Ancora contabilă a liniei: `(TranzactieId, Spatiu, DocumentId, LinieId)`;
  poate avea mai multe postări, păstrate cu ID-urile proprii. Nu se presupune
  o singură contrapartidă și nu se alege prima după ordinea SQL.
- Evenimentul facturii: `(Sens, DocumentFiscalId, DocumentId, TranzactieId)`;
  pentru o linie fără fiscal, identitatea pornește din documentul și
  tranzacția contabilă, fără a inventa TipTvaId. Linia adaugă LinieId.
- Faptul fiscal are cheia completă a grupării `Fiscale.Fapte`, inclusiv
  TranzactieId, LinieId, TipTvaId, Sens și reperele fiscale. Proveniența sa
  păstrează mulțimea `(Spatiu, ID)` a postărilor Bază/Taxă/Autocolectare.
  Nu îl divizăm artificial pe spațiu: Baza și Taxa pot fi în spații diferite.
  Asocierea la GL folosește ancora și rolul propriu, nu produsul cartezian
  dintre toate postările liniei și toate taxele.

Identitatea fiscală agregată din varianta S1-D5 A este un nivel superior:
nu șterge identitățile evenimentelor care justifică versiunea facturii.
Două corecții succesive nu colizionează pe `(Document, Linie, Storno)`.
Un eveniment fără proveniența cerută oprește exportul cu diagnostic;
nu inventăm GUID, partidă sau pereche contabilă pentru a închide cusătura.

### S1-D3 — măsuri GL și timp

O postare economică produce o linie GL pe latura ei, cu valoarea semnată
istorică. Inversa păstrează latura și semnul negativ; nu se mută pe cealaltă
latură pentru a ascunde storno în rulaje. NumberOfEntries numără tranzacțiile
exportate, totalurile D/C însumează liniile lor. Echilibrarea se verifică
atât pe tranzacție, cât și pe fișier; zero total net nu înseamnă absența
evenimentelor. Contraponderile Transformare și Carte Fiscal nu intră.

Selecția GL și granița soldurilor folosesc `Postare.Data`/data tranzacției
contabile. SystemEntryDate folosește data UTC din ScrisLa; originalul,
inversa și înlocuitorul păstrează trei timbre proprii. GLPostingDate pentru
GL urmează tot data capturării în baza de date, distinctă de TransactionDate;
maparea este verificată la S0 pe schema fixată. La factura exportată,
InvoiceDate vine din DataDocument, TaxPointDate din DataExigibilitate,
GLPostingDate din DataInregistrare a versiunii reprezentate. Ultimele trei
nu sunt substituite cu perioada D300/D394 ori cu data inversării.

Pentru o linie fără coordonate fiscale folosim datele documentului operat
identificat, nu un coalesce nedocumentat peste valori lipsă. Date incompatibile
pe antetul aceluiași eveniment sunt refuz, nu Min/Max arbitrar.

### S1-D4 — măsuri facturi și TaxInformation

Netul fiscal este Baza, taxa este Taxa și autocolectarea rămâne rol distinct.
Capitalizatul 100 + 21 produce factură 100/21/121, chiar dacă în GL costul
este 121. Taxarea inversă nu adaugă autocolectarea la datoria comercială:
fixture TI21 are de plată 100, TVA deductibilă 21 și colectată 21 în GL.
Totalurile și codificarea regimului se validează împreună, nu prin regula
universală `brut = bază + orice taxă găsită`.

Nefiscalul ia netul din postările economice ale liniei; TaxInformation
nefiscal este explicit numai pentru o linie fără taxă, nu fallback la o
mapare TVA absentă. O mapare lipsă pentru un fapt fiscal refuză fișierul.
Contul liniei este cel istoric al efectului economic; nici NIR viitor, nici
TipMaterial curent nu îl înlocuiesc. O linie cu repartizare pe mai multe
conturi se împarte explicit în sublinii cu proveniență și măsuri conservate;
dacă faptele nu justifică alocarea bazei/taxei, se refuză, nu se proratează
după o regulă nouă introdusă în export.

**Cantitatea comercială:** la servicii `DeclarantFacturaIntrare.Netul`
declară cantitate zero în cub, deși factura poate avea 2 ore × 50.
Propunerea fixează aici o excepție explicită față de prima delimitare:
Quantity comercială, UM și descrierea provin din linia operată identificată
prin DocumentId/LinieId, păstrată de corecția prin document nou; la inversă
se rezolvă linia originală. Sumele nu se recalculează din această linie.
Pentru stoc se verifică și corespondența cu cantitatea fizică din cub;
cantitatea de pe contrapartida virtuală nu se numără încă o dată.
UnitPrice comercial se justifică prin net/quantity și regula de rotunjire;
o cantitate absentă nu devine implicit 1 sau zero plauzibil. Proba trebuie
să demonstreze protecția liniei operate și că o corecție nu modifică versiunea
veche. Dacă aceasta nu ține pe o ușă publică, S1 se oprește: nu există încă
suport sigur pentru Quantity istorică. Nu adăugăm tacit câmpuri în Postare.

### S1-D5 — alegerea corecției, pe o lună deja raportată

**Tranșat de owner (2026-09-28): varianta B', detaliată în S1-R1.**
Textul de mai jos păstrează variantele A/B propuse, ca istoric.
Cele două variante au același GL și aceleași solduri.
Nu depunem declarații și nu extindem acum DepunereDeclaratie cu D406.

Fixture: factura fizică F din 8 ianuarie 2026 este de **80 + 16,80**.
Operatorul o culege greșit cu **100 + 21**, înregistrează la 10 ianuarie,
închide luna și exportă D406 ianuarie, considerat deja depus în scenariu.
Pe 5 februarie corectează EroareMateriala: inversă −100/−21, apoi
înlocuitor 80/16,80; DocumentFiscalId rămâne F. Nu a apărut o factură de
credit de la furnizor. Aceasta este eroare de evidență, nu reducere comercială.

| Ieșire | A — versiunea corectată a facturii, în perioada ei inițială | B — evenimentele din perioada contabilă, continuitate cu 073 |
|---|---|---|
| GL ianuarie | D cost 100, D TVA 21, C furnizor 121; nemodificat | Identic |
| GL februarie | Inversă −100/−21/−121 și înlocuitor 80/16,80/96,80; D/C net −24,20 | Identic |
| Facturi ianuarie, reexport după corecție | O factură 380, F, 80/16,80/96,80; diferență față de fișierul depus −20/−4,20/−24,20; necesită tratament de rectificare | Factura inițială 380, 100/21/121, neschimbată |
| Facturi februarie | Zero facturi noi pentru corecția tehnică; nu fabricăm 381 | Două evenimente: 381 cu −100/−21/−121 și 380 cu 80/16,80/96,80; net −20/−4,20/−24,20 |
| InvoiceDate / TaxPointDate | 8 ianuarie, din faptele identității fiscale; GLPostingDate al versiunii noi 5 februarie; apartenența raportării facturii rămâne ianuarie | Aceleași date documentare, dar ambele evenimente în februarie; GLPostingDate 5 februarie; identitățile evenimentelor distincte |
| Cusătura GL–facturi | Ianuarie: GL brut 121 față de factură 96,80, diferență 24,20 explicată exact de corecția din februarie; februarie GL −24,20 fără factură nouă | Cusătură pe evenimente în februarie; costul semantic este prezentarea inversei tehnice drept 381 |
| Impact | Cititor de versiuni ale aceleiași identități, diagnostic de perioadă afectată și reexport integral; nu se mută GL din perioada închisă | Cititor de evenimente mai simplu; păstrează convenția 073, dar cere justificare pentru 381 fără document fiscal nou |

În A, perioada de apartenență este luna primei înregistrări a identității
fiscale în evidență, identificată din rădăcina lanțului, nu PerioadaD394.
Versiunea se stabilește din lanțul complet cunoscut în tranzacția de citire,
inclusiv corecții ulterioare lunii reexportate. Astfel „la data contabilă”
pentru GL și „versiunea cunoscută la generare” pentru factură sunt criterii
distincte, raportate explicit. O corecție tehnică începută, cu înlocuitor
încă Draft, refuză exportul perioadelor afectate în ambele variante.

În B, cheia evenimentului nu poate fi confundată cu numărul juridic al
facturii. InvoiceNo rămâne al documentului, iar TransactionID și manifestul
de proveniență disting evenimentele. Nu creăm un număr juridic nou ca să
evităm o coliziune; dacă profilul fixat nu admite această reprezentare,
varianta B este neutilizabilă, indiferent de compatibilitatea cu vechiul cod.

În ambele variante, un retur comercial real RLF/RDC, cu identitate proprie,
datat 5 februarie, de −20/−4,20, rămâne factură 381 în februarie, cu referință
la F. Nu se confundă cu EroareMateriala. Un storno simplu fără înlocuitor
și fără document fiscal de credit este tot întrebare de reprezentare:
în A anulează versiunea eronată a identității în perioada de origine,
în B produce evenimentul 381. Nu îl clasificăm automat drept retur fizic.

**Baza verificată și limita ei:** OPANAF 1783/2021, anexa 3 pct. 18/21,
descrie rectificarea unei perioade și cere retransmiterea informației
complete, nu doar diferențele. Aceasta susține forma reexportului integral,
dar nu tranșează singură reprezentarea în InvoiceStructure a inversei
tehnice Atlas. Alegerea A este recomandare de model, nu concluzie juridică
dedusă dintr-o regulă D394. [Sursa ANAF](https://static.anaf.ro/static/10/Anaf/legislatie/OPANAF_1783_2021.pdf).

Înaintea aprobării se acceptă explicit cusătura numerică din A sau convenția
381 din B. Dacă review-ul normativ cere o a treia reprezentare, contractul
se amendează înainte de cod. Alegerea owner-ului nu substituie validarea
XSD/DUK și nici o regulă normativă aplicabilă identificată ulterior.

### S1-D6 — citire, acces și refuzuri

Se aplică SAF-B6/R3 și SAF-D4 aprobate: un singur ObjectSpace securizat,
o singură citire RepeatableRead, fără îmbogățire prin OS de sistem. Pentru
fișier final cerem acces necondiționat la tipurile și membrii utilizați;
o restricție condițională care poate ascunde rânduri nu este declarată
„export complet” pe baza sumei rândurilor vizibile. Lista permisiunilor
necesare se derivă din inventarul intrărilor și este probată HTTP.

Refuzuri propuse, înaintea XML: `SAFT_ACCES_INCOMPLET` (403),
`SAFT_REPER_LIPSA`, `SAFT_MAPARE_LIPSA`, `SAFT_PROVENIENTA_AMBIGUA`,
`SAFT_CORECTIE_INCOMPLETA`, `SAFT_SURSA_INCOMPLETA` (422).
Acestea sunt coduri de contract, încă neimplementate; nu schimbă codurile
motorului. Diagnosticul identifică numai datele autorizate. Refuzurile pe
acces nu publică sumele sau identificatorii rândurilor ascunse.

### S1-D7 — probe și ieșirea subfeliei

Scenariile S1 sunt SC-SAFT-01…04, 10…11, 13…14 și 15…23 din
[SAFT.md](scenarii/SAFT.md). Fiecare cere așteptări pe GL, solduri, facturi,
TaxInformation, identități și refuzuri; egalitatea cu exportul vechi nu
înlocuiește niciuna. Probele de nomenclator și XML folosesc manifestul S0.
Datele de încercare se creează prin documente și comenzi reale; nu se
inserează fapte artificiale în cub. Cazul „ianuarie depus” păstrează XML-ul
inițial ca artefact, fără confirmare fictivă D300/D394 în loc de D406.

Gate S1: probe numerice, HTTP și concurență; diferențe A/B clasificate;
validare XSD/DUK a artefactelor S1 și apoi validarea L complet la S1+S2;
fără SĂRIT. Un XML de probă S1, cu Payments gol pe fixture fără plăți,
nu certifică Payments și nu autorizează comutarea publică. Măsurătorile
S1 sunt preliminare; perf-ul L complet se măsoară după S2.

Oprire înainte de cod: lipsește alegerea S1-D5 ori aprobarea acestei
detalieri. Oprire în implementare: măsură fără proveniență, cantitate
comercială istorică neprotejată, asociere fiscală ambiguă sau refuz de
securitate ocolit. S0 blochează certificarea, nu munca de specificație.
Nu așteptăm rezolvarea S3 pentru review-ul S1.

## S1-R — review și tranșări (2026-09-28)

Review-ul Claude pe S1 (cerut în `comunicari/2026-09-28-0910-…`), cu
alegerea owner-ului pentru S1-D5 și aprobarea S1, inclusiv S1-D4. Regulile
de mai jos au prioritate față de textul S1-D1…D7 acolo unde îl precizează.

**S1-R1 — B' (381 + 384).** Nomenclatorul ANAF `Nom_Tipuri_facturi`
([structura D406](../api/d406-structura-2026.md), S.I.9) admite șase coduri.
Codul 381 este definit ca „factură storno, cu semnul minus, indiferent de
motivul stornării”, iar 384 ca „factura finală reemisă ca urmare a unei
corecții”. Varianta B a lui Codex (381 + 380) folosea deci greșit codul
înlocuitorului, iar varianta A contrazice invariantul III: stornoul se
reprezintă la data stornării, iar perioada închisă rămâne graniță absolută,
inclusiv pentru reexportul ei. Regula pe eveniment:

| Evenimentul | InvoiceType |
|---|---|
| Tranzacție `Storno` a unui document-factură | 381, cu sumele semnate ale inversei |
| `Operare` a unui document cu `CorecteazaId` | 384 |
| `Operare` cu totalul comercial negativ (retur RLF/RDC, factură cu minus) | 381 |
| Orice altă `Operare` | 380 |

Stornoul unui retur rămâne 381, cu sumele pozitive ale inversei: codul
spune „storno”, semnul rămâne al cubului. Reexportul unei luni închise este
stabil. În aceeași lună, 381 și 384 pot avea același `InvoiceNo`
(documentul fizic e același); evenimentele se disting prin `TransactionID`.
Dacă DUK respinge duplicatul la S0, reprezentarea se reia, fără număr
juridic inventat.

**S1-R2 — evenimentul și legătura cu GL.** Evenimentul este tranzacția
cubului (`Operare` sau `Storno`) a unui document FCT/RLF/FCL/RDC, cu
`Tranzactie.Data` în lună. Factura emite opționalele `TransactionID` =
`TranzactieId` (același cu tranzacția GL) și `GLPostingDate` =
`Tranzactie.Data`. Anularea șterge evenimentul, deci nu produce factură.

**S1-R3 — datele GL (amendează S1-D3).** `TransactionDate` = `GLPostingDate`
= `Tranzactie.Data`, adică data contabilă care decide perioada.
`SystemEntryDate` = data UTC a `Tranzactie.ScrisLa`. Capturarea nu intră
în `GLPostingDate`, fiindcă o capturare din martie pentru februarie ar ieși
din perioada declarată. Maparea se reverifică la S0.

**S1-R4 — măsurile facturii, derivate structural.**

- Contul facturii și partenerul vin din postările de terț ale evenimentului
  (`Cont.RolTert` = rolul secțiunii), cu `Postare.Partener`. Un singur cont
  și un singur partener distinct; altfel `SAFT_PROVENIENTA_AMBIGUA`, iar
  lipsa lor dă `SAFT_SURSA_INCOMPLETA`.
- Brutul comercial = suma postărilor de terț pe latura normală a terțului
  (furnizor: C − D; client: D − C).
- Linie fiscală: net, taxă și autocolectare din `Fiscale.Fapte` al aceleiași
  tranzacții și linii. Contul liniei = contul unic al postărilor cu rol
  Bază; mai multe conturi dau `SAFT_PROVENIENTA_AMBIGUA`, fără proratare.
- Linie nefiscală: netul = postările de terț ale liniei; contul = contul
  unic al contrapartidei. Mai multe conturi de contrapartidă pe aceeași
  linie dau `SAFT_PROVENIENTA_AMBIGUA`: niciun producător actual nu le
  generează, iar subliniile ar cere o regulă de cantitate neprobată
  (review Codex, punctul 3).
- O linie fără postare de terț nu este linie de factură; de exemplu,
  componenta de cost a RDC rămâne numai în GL. Un eveniment fără postare
  de terț și fără fapt fiscal nu este factură (RDC numai de stoc). Unul cu
  fapte fiscale, dar fără terț, dă `SAFT_SURSA_INCOMPLETA`.
- Conservarea: brutul = Σ(net + taxă − autocolectare) pe liniile incluse.
  Diferența dă `SAFT_PROVENIENTA_AMBIGUA`. Formula acoperă normalul,
  capitalizatul (100 + 21 = 121) și taxarea inversă (100 + 21 − 21 = 100).
- `InvoiceDate` = `DataDocument` din faptele fiscale (sau `Document.Data`
  pentru o factură fără fapt), unică pe eveniment. `TaxPointDate` =
  `DataExigibilitate` a liniei, altfel `InvoiceDate`.

**S1-R5 — cantitatea comercială (S1-D4 aprobat).** `Quantity` = |cantitatea
liniei operate|, iar `UnitPrice` = prețul unitar al liniei, sau |net| /
cantitate dacă prețul lipsește. Cantitatea zero dă `SAFT_SURSA_INCOMPLETA`.
Inversa citește aceeași linie; corecția are linii proprii.

*Amendament după review-ul Codex R1, alegerea owner-ului A (2026-09-29):*
UM nu stă pe linie, ci pe produs. De aceea unitatea produsului
(`UnitateMasuraId` și textul `UM`) devine imutabilă din momentul în care
produsul are postări în cub sau apare pe o linie operată; completarea
unei unități lipsă rămâne permisă. Regula e a nomenclatorului și protejează
și stocul, nu doar factura. Pentru altă unitate se creează alt produs.
Descrierile (produs, tip de material) rămân etichete curente din nomenclator:
redenumirea corectează textul, nu măsura. Gardianul e `GardianEditare`,
iar proba e SC-SAFT-22 (UM istorică).

**S1-R6 — refuzurile.** `SaftDto.Refuzuri` (cod, mesaj, document,
tranzacție); cu lista nevidă, XML-ul nu se scrie. Stornoul din lună al unui
document al cărui înlocuitor e încă `Draft` dă `SAFT_CORECTIE_INCOMPLETA`.
O mapare lipsă pe un fapt fiscal, în GL sau pe factură, dă
`SAFT_MAPARE_LIPSA`.

**S1-R7 — GL.** Tranzacția GL = tranzacția cubului din
`Citiri.Contabil.Postari`, inclusiv `Deschidere` din lună (jurnalul
`DESCHIDERE`), ca totalul D/C să fie rulajul balanței. Linia GL = postarea,
ordonată pe `(Spatiu, ID)`, cu `RecordID` ordinal. Fiecare tranzacție
exportată se verifică echilibrată (D = C); altfel `SAFT_PROVENIENTA_AMBIGUA`.
Taxa unei postări Taxă/Autocolectare este suma postării, cu baza faptului ei.

**S1-R8 — securitate și comutare.** Conform R1, S1 nu are rută publică.
Refuzul `SAFT_ACCES_INCOMPLET`, probele HTTP SC-SAFT-13 și proba pe ușile
publice a liniei operate (SC-SAFT-22, azi prin gardian) se livrează la
comutarea L, în S2, pe ruta reală. În S1 rămân proba RepeatableRead
(SC-SAFT-17) și refuzul unei tranzacții ambiante mai slabe.

**S1-R9 — A/B și proveniența (după review-ul Codex R2).** Comparația cu
exportul vechi merge în ambele sensuri, pe cheia (document, storno). Pe
facturi compară tipul, netul, taxa, brutul, contul, numărul de linii și data;
pe GL compară totalurile D/C per document. Proba cere și respingerea unui
mutant cu o factură omisă. *R2.1:* o diferență e explicată numai prin
potrivirea exactă (secțiune, document, storno, valoare veche, valoare nouă),
iar gate-ul cere ca mulțimea diferențelor găsite să fie exact mulțimea
declarată. Omisiunea unei facturi cu o diferență deja clasificată (TI21)
trebuie și ea respinsă. Diferențele clasificate pe fixture-ul S1 sunt:
brutul TI21, `InvoiceDate` (data documentului, inclusiv pentru stornoul 381)
și recepția stocului la FCT (cub) față de NIR (registrul vechi). Linia GL
păstrează `(Spatiu, ID)` postării în DTO; manifestul ca fișier rămâne la S0.

## S0-R — artefactele validării fixate (2026-09-29)

Implementat în `nou/tools/ModelCheck` (`ValidareD406.cs`, `Duk.cs`), probat
în `ScenariiSaft` (SC-SAFT-24, SC-SAFT-25). Regulile de mai jos închid gate-ul
S0 pentru artefactele S1; Payments (S2) și stocurile (S3) se certifică pe
același manifest când există.

**S0-R1 — manifestul.** Pin-urile SHA-256 stau în cod (`ManifestD406`):
XSD `Ro_SAFT_Schema_v249_2025.xsd` (`80AD7EAA…DCCC2`, versionat în
`nou/tools/ModelCheck/Anaf/`, `-text` în `.gitattributes`), validatorul
`lib/D406Validator.jar` J2.2.18 (`197FD169…32373`) plus
`DUKIntegrator.jar`, `DUKIntegrator_AnLunaUI.jar`, `lib/DecValidation.jar`,
`lib/Validator.jar`, și nomenclatorul `RO_SAFT_SchemaDefCod_16.02.2026.xlsx`
(`050508BF…0F2D`). Fiecare rulare scrie `manifest-d406.json` lângă fișiere:
pin-urile, SHA-256 al schemei derivate, SHA-256 al fiecărui XML, perioada,
erorile XSD și verdictul DUK cu comanda exactă. Kitul se verifică înainte și
după fiecare rulare DUK; o abatere (auto-update, fișier lipsă) e respingere.

**S0-R2 — versiunea reală a kitului.** `config/versiuniCurente.txt` spune
J2.2.8, dar jar-ul local este octet cu octet cel publicat de ANAF pentru
J2.2.18 (`update5/D406_35/D406Validator.jar`, `versiuni.xml` la
2026-09-29). Versiunea se citește din pin, nu din fișierul kitului;
73-r8 și 74-r13 se închid. Nomenclatorul local 16.02.2026 este un superset
strict al celui legat de pagina ANAF (05.02.2026): singura diferență este
codul WHT 604040, prezent în `Parameters_v3` al validatorului pin-uit.

**S0-R3 — namespace-ul.** XSD-ul public declară `targetNamespace` și
prefixul `nsSAFT` pe `d406t`; producția folosește `d406`. Validarea XSD
înlocuiește exact aceste două atribute (fiecare trebuie să apară o singură
dată) și înregistrează substituția în manifest. Avertismentul „fără
informație de schemă” este eroare, iar rădăcina trebuie să aibă declarație
în schemă. Validatorul D406 impune `d406` (D406T impune `d406t`); un fișier
`d406t` este respins și de XSD-ul derivat, și de DUK D406.

**S0-R4 — perioada.** Măsurat: `an`/`luna` date lui
`DUKIntegrator_AnLunaUI.jar` aleg versiunea nomenclatorului, nu se compară
cu antetul. Același fișier din 2024 cu coduri ale cotei 21% are 51 de
erori de cod pe `an=2024` și zero pe `an=2026`. Deci perioada vine numai
din `Header/SelectionCriteria` (o singură lună; altfel refuz), nu separat.
Fallback-ul pe `DUKIntegrator.jar` fără perioadă este eliminat, iar kitul
absent sau nepotrivit dă `INDISPONIBIL` = verificare picată, nu `SĂRIT`
(inclusiv pentru probele D16-V3/D17-V3).

**S0-R5 — secțiunile fără intrări.** Măsurat pe L: `SalesInvoices` sau
`Payments` cu `NumberOfEntries = 0` și totaluri zero sunt respinse de DUK
(„elementul ... ar fi trebuit sa apara de minimum 1 ori”); omisiunea
secțiunii trece DUK, dar contrazice XSD-ul (secțiunea e obligatorie).
Scriitorul emite deci secțiunea goală, fără totaluri, pe ambele module
(`CuTotaluri` = există intrări); XSD-ul și DUK o acceptă.

**S0-R6 — fixture-ul în regimul cotei.** Catalogul S1 folosea anul 2024
cu cota 21%, pe care nomenclatorul ANAF al anului 2024 nu o conține. Scena
SAFT se mută pe 2040 (primul an liber al suitei după 2036), unde se aplică
nomenclatorul curent. Cifrele economice ale fixture-ului nu se schimbă.
Scena probează deci cel mai nou nomenclator al validatorului pin-uit, nu
perioade istorice. Când se actualizează pin-ul, 2040 preia automat
nomenclatorul nou. O schimbare ANAF datată în viitor (de exemplu o cotă
nouă dintr-un an ulterior) ar despărți însă 2040 de prezent. Santinela
acoperă acest risc: fișierul din ianuarie, cu antetul pe luna reală 2025-09
(regimul 21%), trebuie să treacă la fel ca pe 2040. Dacă diverg, scena se
mută pe perioada reală; nu se ajustează santinela.

**S0-R7 — ce certifică S0 pe S1.** XML-urile L ale lunilor 1–3 trec XSD
v249 (d406) și DUK J2.2.18, fără atenționări, inclusiv: 381 și 384 cu același
`InvoiceNo` în februarie (S1-R1 ține, fără număr inventat); TI21 și cele două
cote; `SystemEntryDate` în afara perioadei (timbrul real). Schema fixată
cere `SystemEntryDate` și `GLPostingDate` pe `Transaction`; pe `Invoice`
`GLPostingDate` și pe `PaymentLine` `SourceDocumentID` sunt opționale (R4).
Măsurat, nu probă: DUK acceptă și `GLPostingDate` = data capturării, în afara
perioadei; S1-R3 rămâne o alegere de semantică (data contabilă care decide
perioada), nu o constrângere a validatorului. Mutanții respinși: antetul pe
2024 (codurile 21% nu sunt în nomenclator), tranzacția fără `GLPostingDate`
(XSD și DUK), namespace-ul `d406t` (XSD și DUK). Payments rămâne gol în S1:
fișierul nu certifică Payments (S1-D7).

Integral verde pe ambele profiluri (bugetar 3.267, privat 4.375 verificări),
inclusiv D16-V3/D17-V3 pe kitul pin-uit: `run-verificari/20260929-011541-801`
(manifestul S0 în `tmp/atlas-saft/s0-*/manifest-d406.json`).

**S0-R8 — proveniența în manifest (review Codex S0-RV1).** Fiecare fișier
certificat poartă în `manifest-d406.json`, lângă SHA-256-ul lui, harta scoasă
din DTO-ul exact din care s-a scris: linia GL `(TransactionID, RecordID)` →
`(Spatiu, ID)` al postării, cu `DocumentId` și `LinieId`; factura
`(secțiune, InvoiceNo, InvoiceType, TransactionID)` → `(DocumentId, Storno)`.
Proba citește harta înapoi din fișierul manifest și cere: aceeași mulțime de
`(TransactionID, RecordID)` ca XML-ul, fără dubluri; cheia `(Spatiu, ID)`
completă și unică; fiecare postare există în cub cu `TranzactieId` =
`TransactionID`, același document și aceeași linie; aceeași mulțime de facturi
ca XML-ul, cu eveniment unic. *RV1.1:* asocierea se probează exact, nu
doar prin apartenența la aceeași tranzacție. Linia XML a fiecărui
`(TransactionID, RecordID)` are contul, latura și suma postării indicate.
În fiecare tranzacție, ordinalele urmează ordinea `(Spatiu, ID)` a surselor
(S1-R7). `DocumentId` și `Storno` ale facturii sunt ale tranzacției reale
`TransactionID` din cub (`Fel` = `Storno`). Mutanții respinși sunt: linie GL
omisă, cheie de postare dublată, surse permutate între RecordID 1 și 2,
factură omisă, `DocumentId` schimbat, `Storno` inversat.
Integral verde după RV1.1 și santinelă (bugetar 3.267, privat 4.383):
`run-verificari/20260929-092618-574`. Măsurat: 62, 28 și 8 linii GL; 13, 7 și
2 facturi în lunile 1–3. Au intrat în catalog și probele adverse ale lui Codex
care au trecut: luna fără rulaj, pe cub și pe ruta veche, trece XSD și DUK;
un copil obligatoriu fără namespace este respins de XSD.
Integral verde pe ambele profiluri (bugetar 3.267, privat 4.382):
`run-verificari/20260929-083657-241`.

## S2 — Payments și comutarea L: contract pentru aprobare

Stare: **S2 închis (2026-09-29).** S2-Q1 și S2-Q2 au fost tranșate de owner
(S2-R1, S2-R2). Implementarea e verificată, iar ruta L e comutată pe cub
(S2-R3). Review-ul advers Codex e închis (S2-RV1 corectat în `3ac87ee`,
reverificat, integrala independentă `20260929-205701-186`). FZ-r3 rămâne
deschisă pentru gate-ul transversal de perf. Bază inspectată: `1985d7e` (S0 închis). Nu schimbă
motorul, scrierea registrelor, împerecherea sau politica mișcărilor. Payments
se derivă numai din cub; `Imperechere` rămâne metadatele comenzii, nu sursa
alocării. La ieșire, ruta publică L comută pe cub (R1).

Faptele de model pe care se sprijină (verificate în cod la redactare):

- PLT/INC postează prin `DeclarantTrezorerie`: o mișcare per linie, capătul
  terțului pe partidă. Plata cu document-sursă se **nominalizează** în
  `Operare`: postarea terțului stă direct pe partida sursei, cât ține restul
  ei, iar excedentul pe partida proprie (`Partide.Identitate(P, cont, terț)`).
- Legătura manuală (`ImperechereService.Imperecheaza`) scrie o tranzacție
  `Transfer` a stingătorului: o singură `Mutare` = două postări pe același
  cont, partener și latură, −x pe partida proprie și +x pe partida țintă
  (`Transferuri.Muta`). Desfacerea și stornoul scriu transferul invers datat;
  data legăturii e în perioadă deschisă (`GardianPerioada`).
- Partida inițială din deschidere are referință `Guid` opacă, fără număr de
  document (`PartidaInitiala.Referinta`).
- Trezoreria nu produce fapte fiscale (TVA la încasare = 36f, după PoC) și nu
  are valută în cub (`Miscare` cu valută 0; 73-r16, 64k, B-r6).
- `TipDocument.LaturaContPropriu` (B-r2, dată de seed) spune latura pe care
  stă contul propriu: `Predator` pe plată, `Primitor` pe încasare.

### S2-D1 — evenimentul și domeniul

Evenimentul de plată este tranzacția cubului `Operare` sau `Storno` (a unei
`Operare`) a unui document din mulțimea restrânsă pe tip
`DocumentTrezorerie` (89b), cu `Tranzactie.Data` în lună, citită din
`Citiri.Contabil.Postari` (Transferul nu e eveniment de plată: SAF-B3.3).
Anularea șterge evenimentul. Bugetar: neaplicabil, ca la S1.

Latura contrapartidei = opusul laturii contului propriu: `Debit` când
`LaturaContPropriu = Predator`, `Credit` când e `Primitor`. Postările
evenimentului pe latura contrapartidei sunt **postările de contrapartidă**;
ele singure dau liniile. Lipsa datei pe tip = `SAFT_SURSA_INCOMPLETA`.

Viramentul intern (ambele laturi conturi proprii) nu este plată către terț:
rămâne numai în GL și, ca pe ruta veche, nu intră în `Neincluse`.
Excluderi raportate în `Neincluse` cu cauza (paritate cu ruta veche):

- latura externă este `Partener`, dar nicio postare de contrapartidă nu are
  cont cu `RolTert` (`PlataFaraContTert`, de exemplu 4423 către ANAF);
- documentul fără latură externă (`DocumentFaraPartener`).

Latura externă `Angajat` intră, cu `CustomerID` = `SupplierID` = codul
societății și avertismentul `PlataCatreAngajat` (paritate; SAF-T nu are
identitate de angajat).

Gărzi, înaintea XML: postare de contrapartidă cu `Valuta` ≠ null sau fapt
fiscal (`Fiscale.Fapte`) pe tranzacția evenimentului = `SAFT_SURSA_INCOMPLETA`
(modelul nu le are; nu convertim și nu inventăm TVA la încasare); mai mulți
parteneri distincți pe postările de contrapartidă = `SAFT_PROVENIENTA_AMBIGUA`.

### S2-D2 — linia de plată

Linia = grupul postărilor de contrapartidă ale evenimentului pe
`(Cont, Partener, ținta alocării)`; ținta vine din S2-D3. Nu există linie pe
detaliul documentului: partida e pe document × cont × terț, iar un transfer
nu se poate atribui unei linii de defalcare fără proratare (S1-R4).

- `AccountID` = simbolul contului postării; `CustomerID`/`SupplierID` după
  `Cont.RolTert` (Client → client + societate; Furnizor → societate +
  furnizor), cu `Postare.Partener`. Contul fără rol, fără partidă (de exemplu
  542 pe angajat), dă o linie fără țintă.
- `DebitCreditIndicator` = latura postării; `PaymentLineAmount` = suma
  semnată (stornoul negativ, pe aceeași latură, ca GL-ul S1-D3).
- `SourceDocumentID` = `Document.Numar` al documentului-origine al țintei;
  lipsește pentru rest (avans) și pentru partida inițială.
- `TaxInformation` = nefiscal explicit: regula e că evenimentul nu are fapt
  fiscal (garda din S2-D1), nu fallback la o mapare lipsă.
- `Analysis` = analiza comună a postărilor liniei; dacă diferă, se omite pe
  linie (GL o păstrează integral), cu avertismentul `PlataAnalizaMixta`.
- `Description` = descrierea liniei de defalcare când linia provine dintr-un
  singur detaliu cu descriere, altfel descrierea plății.
- Ordinea: țintele cu document după (Data, Numar, Id) ale originii, apoi
  partida inițială, apoi restul; `LineNumber` ordinal.

Antetul: `PaymentRefNo` = `Document.Numar` (același și la storno, ca
381/384 la S1-R1); `TransactionID` = `TranzactieId` (aceeași cu tranzacția
GL, cusătura S1-R2); `TransactionDate` = `Tranzactie.Data`;
`Period`/`PeriodYear` = luna exportată; `PaymentMethod`/`PaymentMechanism`
din `DocumentTrezorerie.TipInstrument` (`SaftReguli.MetodaPlata`, metadată
de identificare); `GrossTotal` = Σ liniilor. Secțiunea: `NumberOfEntries` =
evenimentele, `TotalDebit`/`TotalCredit` = Σ liniilor D/C (formula actuală).

Cusătura: Σ liniilor evenimentului = Σ postărilor de contrapartidă ale
aceleiași `TransactionID` din GL, pe cont și latură; altfel
`SAFT_PROVENIENTA_AMBIGUA`.

### S2-D3 — alocarea la capătul lunii operării (R4, SAF-B3.3)

Pentru documentul P, cu `Operare` în luna M_O și capătul E_O = ultima zi a
lui M_O. Alocarea lui P este:

1. **nominalizată**: postarea de contrapartidă a `Operare` pe o partidă
   străină u (origine `Partide.Origini` ≠ P) e alocată originii lui u;
   originea fără document (deschidere) = partida inițială;
2. **prin transfer**: pe partida proprie u_P, fiecare tranzacție `Transfer`
   (citită prin `Citiri.Partide.Postari`, care o include) cu `Data ≤ E_O` care are o postare pe u_P este o pereche (u_P, w), pe
   același cont, partener și latură; altă formă = `SAFT_PROVENIENTA_AMBIGUA`.
   Ținta w primește −(efectul semnat al transferului pe u_P, în sensul laturii
   lui P). Sursa transferului nu contează: legătura lui P, desfacerea ei,
   o notă de compensare care stinge avansul lui P (SC-DES-11) sau desfacerea
   nominalizării automate intră la fel. Țintele cu sumă netă 0 dispar;
3. **rest** = suma lui P pe u_P − Σ alocărilor prin transfer.

Fiecare alocare și restul au semnul sumei lui P și nu o depășesc; altfel
`SAFT_PROVENIENTA_AMBIGUA` (gardianul împerecherii o face imposibilă; proba
o cere).

**Stornoul** lui P, în orice lună, are liniile `Operare`-ului calculate la
E_O, cu suma negată și aceleași ținte. Conservare: Σ postărilor stornoului
= −Σ postărilor `Operare`, pe cont și latură; altfel refuz. Corecția este alt
document, cu alocarea proprie.

**R4, măsurat la redactare (2026-09-29):** în `saft-L-2040-01.xml` al S0
(ianuarie, anul scenei) s-a injectat o plată `PLT-1` cu `TransactionID`,
linia 1 D 401 50 cu `SourceDocumentID` = o factură a fișierului, linia 2
D 401 20 fără referință, plus stornoul ei cu −50/−20 și același
`PaymentRefNo`. DUK J2.2.18 (`an=2040`, `luna=01`) le validează fără erori
și fără atenționări. Identificatorul societății pe latura opusă trebuie să
fie `00` + CUI (`SaftReguli.IdSocietate`); `RO` + CUI este respins
(„formatul este invalid”). XSD-ul se verifică la certificare, pe XML-ul
real al scriitorului.

Consecințe (**S2-Q1**, recomandarea): legătura datată după E_O nu apare
în nicio declarație, iar reexportul unei luni închise este stabil, fiindcă
transferurile ≤ E_O nu se mai pot scrie. Stornoul neagă exact ce s-a
declarat pentru P, deci suma referințelor unei facturi pe toate declarațiile
rămâne coerentă: PLT 70 + legătură 50 în ianuarie, storno în februarie →
ianuarie 50 F + 20 rest, februarie −50 F și −20 rest. Legătura din
februarie și stornoul din martie → ianuarie 70 rest, martie −70 rest.
Alternativa „stornoul inversează starea de dinaintea lui” ar raporta −50 F
fără +50 F declarat.

### S2-D4 — ce nu intră în Payments

**S2-Q2 (owner):** compensarea prin notă contabilă (48b) nu este eveniment
de plată în S2; rămâne în GL, ca pe ruta veche. Recomandare: restanță
numită (Payments cu `PaymentMethod` 02 / mecanism 97 are nevoie de o regulă
de linie pe ambele laturi ale compensării). Efectul ei asupra unei plăți
(S2-D3.2) intră deja, cu referința la NTC.

Valuta, diferențele de curs și TVA la încasare nu există în cub pentru
trezorerie; garda S2-D1 le refuză dacă apar, fără a le declara acoperite.

### S2-D5 — comutarea L și accesul complet

După gate-ul S2, `GET api/proiectii/saft` și `…/saft/xml` citesc
`SaftPeCub`; rutele S (`…/saft/stocuri*`) rămân pe `SaftStocuri` până la S3
(R1). `SaftProiectii.Saft` (L vechi) rămâne numai oracol A/B în ModelCheck
și sursa Import1C (înghețat, 091-r4); nu mai e accesibil public. Se șterge
la gate-ul final, odată cu partea L a rutei vechi.

`Refuzuri` nevid: XML = 422 cu `EroriDto` (codurile și mesajele), înaintea
primului byte; sumarul JSON = 200 cu lista de refuzuri (ecranul arată de ce
fișierul nu pleacă).

`SAFT_ACCES_INCOMPLET` (403, SAF-D4 A), pe ambele uși L, înaintea
proiecției (și a sumarului: un sumar filtrat este tot o proiecție parțială).
Verificat în sursele DevExpress 26.1.4: `CanRead(Type, os)` răspunde `false`
numai când criteriul combinat e exact `1 = 0`; o restricție condițională de
rând sau de membru lasă `true` (`PermissionRequestProcessor.IsGrantedInSameRole`,
`SelectCriteriaProcessor`). Nu poate deci proba accesul complet. Mecanismul:

- `ISelectDataSecurity` din `SecurityStrategy.CreateSelectDataSecurity(os)`,
  aceeași instanță din care EF Core filtrează rândurile și ascunde membrii
  (`SecurityPermissionProcessor`);
- acces complet pe un tip = `GetObjectCriteria(tip)` fără criteriu nevid și
  `GetMemberCriteria(tip, m)` fără criteriu nevid pentru fiecare membru de
  securitate (`SecurityMembersHelper.GetSecurityMembers`); administratorul și
  tipurile nesecurizate dau liste goale;
- lista tipurilor = exact tipurile interogate de `SaftPeCub` (bază **și**
  frunze: o permisiune declarată pe o frunză nu se aplică cererii pe bază,
  iar una pe bază se aplică frunzei), derivată din inventarul cititorilor și
  fixată printr-o probă care o compară cu tipurile efectiv interogate;
- un membru-referință spre alt tip securizat moștenește criteriul tipului
  țintă, deci tipul țintă intră în listă.

Răspunsul 403 numește tipul sau membrul lipsă, nu rânduri ori sume.

### S2-D6 — probe

Scenariile S2 sunt SC-SAFT-05, 06, 13, 15 și 26…36 din
[SAFT.md](scenarii/SAFT.md): așteptări pe linii, ținte, `SourceDocumentID`,
sume semnate, cusătura cu GL, excluderi și refuzuri, create prin documente
și comenzi reale (legătură, desfacere, storno, corecție), fără fapte inserate
în cub. Certificarea L complet (lunile scenei, Payments nevid) pe manifestul
S0: XSD v249 + DUK J2.2.18, fără atenționări; S0-R8 se extinde cu harta
plății `(TransactionID, LineNumber)` → mulțimea `(Spatiu, ID)` a postărilor
de contrapartidă și a transferurilor care o justifică, plus ținta; mutanții
(linie omisă, sursă permutată, `SourceDocumentID` schimbat) sunt respinși.

A/B pe plăți (cheia document × storno, în ambele sensuri, S1-R9): total,
cont, linii, referințe. Diferențele declarate exact: împărțirea 50 F + 20
rest față de plata întreagă pe F (ruta veche pune referința pe toate liniile
dacă legătura e unică), legătura viitoare citită de ruta veche fără dată,
`TransactionID` nou. Orice altă diferență e defect.

Perf (SAF-B7, FZ-r3): numărul de interogări al exportului nu crește cu
numărul plăților (măsurat la n și 2n); planul SQL al citirii transferurilor
pe partidele proprii (`EXPLAIN (ANALYZE, BUFFERS)`) decide indexul
`(Unitate, Data)`; dacă e nevoie, migrația cubului se scrie în SQL.

### S2-D7 — gate și oprire

Gate S2 = L complet: probele S2 și S1 verzi pe ambele profiluri, XML-urile
lunilor scenei certificate cu manifest, probele HTTP pe host viu
(SC-SAFT-13, SC-SAFT-22 pe ușa publică, SAFT_ACCES_INCOMPLET), A/B clasificat,
review advers Codex închis. Abia apoi se comută ruta.

Oprire: alocare fără proveniență completă; eveniment fără cusătură cu GL;
mecanism de acces care nu detectează o restricție condițională de rând sau
de membru (nu comutăm pe un export care poate ascunde rânduri); o formă
Payments respinsă de DUK (50 + 20 fără referință, R4) — revenim cu
alternativă, fără referință inventată.

### S2-R — tranșările owner-ului (2026-09-29)

**S2-R1 — stornoul plății.** Varianta recomandată din S2-D3: liniile
stornoului sunt liniile `Operare`-ului calculate la E_O, negate. Legătura
de după E_O nu apare în nicio declarație.

**S2-R2 — compensarea.** Azi compensarea este o notă contabilă culeasă cu
repartitori pe linii (48b); nu există un document de compensare care să-și
genereze singur postările. Un astfel de document, cu declarant propriu, ar fi
sursa firească a unei plăți 02/97. De aceea compensarea nu intră în S2, iar
explorarea documentului de compensare devine restanța **SAFT-r1**. Efectul
unei NTC asupra alocării unei plăți rămâne în S2-D3.2 (SC-SAFT-31).

### S2-R3 — implementare și probe (2026-09-29)

Commit-uri pe `tr-d8-saft-s2`: `b57c1bc` (Payments pe cub) și commit-ul
comutării. Cititorul `Cub/Citiri/Plati` implementează S2-D3. Proiecția
`SaftProiectii.PeCub.Plati` implementează S2-D1/D2. `SaftAcces` implementează
S2-D5, iar `SaftController` comută L.

- **Lista tipurilor verificate** nu e scrisă din memorie. Proba SC-SAFT-36
  capturează SQL-ul emis de `SaftPeCub` prin `DiagnosticListener`-ul EF Core
  și cere ca mulțimea tabelelor citite să fie egală cu tabelele din
  `SaftAcces.Citite`. Prima rulare a găsit trei tabele lipsă
  (`ClaseProduse`, `MapariTvaSaft`, `SetariProfil`); lista le include acum.
  Garda verifică toate tipurile mapate în aceste tabele (TPH: bază și frunze).
- **HTTP pe host izolat** (`ProbeHttp/saft-acces.py`, baza
  `…Privat.SaftS2Http`). Restricțiile de rând pe `Postare`, de membru pe
  `Postare.Valoare` și de rând pe `Partener` dau toate 403
  `SAFT_ACCES_INCOMPLET` pe sumar și fișier, fără sume în corp. `User` dă tot
  403. Admin și Cititor primesc 200, cu plata 50 F + 20 rest și TVA 21 în GL.
  Editarea liniei FCT operate dă 422, iar fișierul rămâne identic.
  `refuzuri.ps1` trece 294/294.
  **Schimbare intenționată:** SC-CIT-87 aștepta pe SAF-T L un XML filtrat
  pentru rolurile cu restricții; acum ele primesc 403 (SAF-D4), iar proba
  `fiscal-cub.py` e actualizată.
- **Perf (S2-D6):** exportul emite 42, 42 și 41 de comenzi SQL la 14, 3 și
  1 plăți (20, 7 și 2 facturi), deci fără N+1. **Abatere de la S2-D6:**
  indexul FZ-r3 nu e decis. Interogarea reală e fixată: transferurile și
  originile pe `Unitate = ANY(…)`, `Data ≤ capăt`, pe `Postare_Contabil`,
  care n-are index pe `Unitate`. Nu există bază de volum după C102, iar un
  plan pe baza scenei nu dovedește nimic. FZ-r3 rămâne activă până la
  gate-ul transversal de perf.
- **Abateri de fixture, fără efect asupra regulii:** în ramura SC-SAFT-27,
  legătura din februarie se șterge înaintea stornoului din martie, fiindcă
  gardianul refuză stornoul cu legătură vie în perioadă deschisă. SC-SAFT-30
  rulează în scena DES, al cărei cont de terț primește `RolTert` Furnizor.
  Fără rol, plata ar fi ieșit corect în `Neincluse`.

Integral verde pe ambele profiluri după Payments (bugetar 3.267, privat
4.406): `run-verificari/20260929-115901-443`; după comutare (bugetar 3.267,
privat 4.408): `run-verificari/20260929-122333-161`. Probele HTTP:
`run-verificari/saft-s2-http-{probe,fiscal,refuzuri}.log`.

### S2-R4 — review Codex S2-RV1 și integrarea ecranului (2026-09-29)

**S2-RV1 / P1, corectat.** O plată nominalizată integral la operare nu are
postare pe partida proprie. Linia de rest nu exista, iar transferul invers
al desfacerii nu se aplica: exportul păstra 50 pe factură. Acum linia de rest
se creează pentru fiecare partidă proprie, chiar la zero. Liniile legate prin
transferuri pe aceeași partidă proprie își unesc contribuțiile, deci
proveniența nu depinde de ordinea transferurilor. Proba SC-SAFT-37 acoperă:
nominalizarea integrală desfăcută, excedentul, realocarea pe altă factură,
stornoul din luna următoare și proveniența. Cititorul de la `663cadb` pică
pe toate cele trei verificări.

**Ecranul L după comutare.** Sumarul pe cub completa numai o parte din
`SaftRezumat`, iar pagina ar fi arătat „diferă” pe cusăturile construite pe
registrele vechi. Pe cub, „evidența” din cusături înseamnă:

- rulajul debitor al balanței lunii;
- TVA din faptele fiscale (`Tva + Autocolectare`), cu TVA capitalizată
  numărată numai dacă n-are linie de taxă în GL;
- baza faptelor fiscale pe sens, din care baza fără factură (DVI, decont)
  e cea a faptelor din tranzacții care nu sunt evenimente-factură;
- `TotalPlati` = Σ brut al plăților.

Proba SC-SAFT-15 cere egalitatea lor pe lunile scenei. Pagina React afișează
`Refuzuri` din sumar înainte de descărcare, iar textele ei numesc sursa nouă.
Verificat în browser pe baza izolată: ianuarie are toate cusăturile egale și
plata 70; februarie arată `SAFT_CORECTIE_INCOMPLETA` deasupra secțiunilor
(`run-verificari/saft-s2-ui-februarie-refuz.jpg`).
Integral verde după corectură (bugetar 3.267, privat 4.415): `run-verificari/20260929-204252-054`.

## S3 — MovementOfGoods, PhysicalStock și comutarea C: contract pentru aprobare

Stare: **propus de Claude (2026-09-29); S3-Q1 și S3-Q2 tranșate de owner
([S3-R](#s3-r--tranșările-owner-ului-2026-09-29)); review-ul advers Codex
cerut.** Bază inspectată: `66e25c1` (main după PR #12).
Nu schimbă motorul, declaranții, scrierea registrelor sau împerecherea.
Schimbă modelul politicii: `Cont.CategorieStoc` (SAF-D3=C), iar
`PoliticaMiscareSaft.TipStoc` înseamnă de acum categoria contului, nu
registrul. La ieșire, rutele S comută pe cub (R1).

Faptele de model, verificate în cod la redactare:

- Postarea pe lot este cea din `Loturi.Postari`: Carte Contabil, `FelUnitate.Lot`,
  cu unitate, produs și gestiune (Spatiu Stoc). Capătul fără lot al mișcării
  stă pe o gestiune virtuală (Furnizor, Client, Consum, Inventar) sau pe un
  cont fără unitate. Contraponderea ASM are `Unitate = null` și nu intră;
  T-r14 este satisfăcută structural, fără filtru nominal.
- Producătorii pe lot, cu capătul real: FCT +q (Furnizor), NIR propriu +q,
  NIR delta ±q (Inventar; contul cauzei 099 fără lot), BTR −q/+q între două
  gestiuni reale (Transfer), BCS −q pe 3xx și +q pe contul de consum,
  LDI +q (Inventar) / −q (Consum), DSC −q (Client), RLF −q (Furnizor),
  RDC +q (Client), ASM −q consum / +q produs (Operare sau Transfer pe cont).
- Stornoul unui document este **o** tranzacție `Storno` care inversează și
  Operarea, și Transferul pe stoc; fiecare postare are `InversaDin`.
  `Deschidere` este unică, fără document, fără istorie anterioară datei ei.
- Singurul lot pe un cont care nu e de stoc este capătul de consum BCS
  (`Unitate` pe 6xx, respectiv 711 pentru 345), în gestiunea primitorului.
- Linia FCT de stoc cu TVA capitalizată postează pe același lot două rânduri:
  (q, bază) și (0, taxă).
- NIR delta cu q = 0 are v = 0 (valoarea e proporțională), deci nu produce
  mișcare numai valorică. Azi niciun producător nu scrie pe lot o linie cu
  cantitate netă 0 și valoare nenulă.
- ASM: `DeclarantAsamblare` impune ΣP = ΣR și absoarbe Δ = C − R în produs,
  deci Σ valorilor produselor = ΣC, consumul evaluat FIFO pe cub. Pe cub
  transformarea este echilibrată la C. Δ există numai față de registrul vechi.

Normele, din ghidul D406 v2.0 (dec. 2021, §2.8, §2.10, §4.4) și foaia
`Nomenclator stocuri` a workbook-ului din SAF-B6:

- `AccountID` pe linia de mișcare este contul analitic de clasa 3 al stocului.
- `Quantity` = 0 la 60, 90, 100, 101, 130, 140 și 180 („Alte tranzacții
  (care nu presupun mișcări cantitative)”).
- PhysicalStock: fiecare produs, per gestiune și per preț unitar (FIFO).
  Include și stocurile terților (custodie, consignație), neacoperite aici.
- Etichetele codurilor apar în rapoartele ANAF (nota 6). `SaftReguli.CoduriMiscare`
  diferă azi de etichetele oficiale la 90, 100, 101, 130, 140 și 180. Se
  aliniază verbatim, iar `MovementTypeTable.Description` = eticheta RO.
- Stocurile se depun la cerere, câte o declarație pe lună sau trimestru.

### S3-D1 — categoria contului (SAF-D3=C)

`Cont.CategorieStoc : TipStoc?`. Rezolvarea urcă pe `Parinte` până la prima
valoare explicită. Lipsa pe tot lanțul înseamnă cont neclasificat. Un singur
rezolver servește exportul și probele. Categoria se editează pe planul de
conturi, fără release, fără gardian propriu. O schimbare după postare poate
reclasifica un reexport. De aceea manifestul S0-R8 primește perechile cont →
categorie rezolvată ale conturilor atinse, plus hash-ul lor.

Rolul categoriei în fișierul S este structură, deci stă în cod:

| Categorie | Rol |
|---|---|
| Magazie, Marfuri | poziție raportabilă în PhysicalStock; fiecare linie cere cod |
| Consum | nu e stoc de clasa 3 (lotul a trecut pe cheltuială); fără poziție; linia cere rând de politică, de regulă excludere cu motiv (BCS/Consum/+1 de azi) |
| Folosinta, Custodie, Gratuit, ProductieNeterminata | neacoperite de S3 (Folosința există numai pe bugetar, 093): poziția sau linia = refuz `SAFT_CATEGORIE_NEACOPERITA` |
| neclasificat | refuz `SAFT_CATEGORIE_LIPSA`, cu simbolul contului și numărul postărilor |

Pe categoriile raportabile, gardianul refuză rândul de politică fără cod:
o excludere acolo ar rupe identitatea poziției (S3-D7a).

Seed privat propus: 30, 34, 38 → Magazie; 37 → Marfuri; 6 și 711 → Consum.
Restul clasei 3 (32x, 33x, 35x, 36x, 39x) rămâne neclasificat. Azi aceste
conturi nu poartă loturi, iar primul lot acolo cere decizie, nu moștenire
tăcută. Bugetarul nu primește seed (SAF-T neaplicabil, TR-r7).

### S3-D2 — mișcarea și linia

**Evenimentul** = tranzacția cubului `Operare`, `Transfer` sau `Storno`, cu
`Tranzactie.Data` în lună. `Deschidere` nu este mișcare (S3-D4).

**Linia** = agregatul postărilor pe lot ale evenimentului pe
`(LinieId, Unitate, Cont, Gestiune)`. `Quantity` = Σ cantităților.
`BookValue` = Σ valorilor semnate (Debit +, Credit −). O linie cu (0, 0)
dispare. O linie cu cantitate 0 și valoare nenulă primește refuzul
`SAFT_MISCARE_VALORICA`: nu are producător azi, iar codurile 90/100/101/180
cer contract propriu.

**Codul.** Semnul politicii este semnul cantității pe `Operare`/`Transfer`
și opusul lui pe `Storno` (semnul originii, SAF-D3=C). Categoria este cea
a contului liniei. Rezolvarea pe `(TipDocument, categorie, semn)`:

- rândul cu semn exact bate rândul cu semn null;
- lipsa rândului dă refuzul `SAFT_MISCARE_FARA_POLITICA`;
- codul null înseamnă excludere cu motiv, cu cifrele în `Excluse`;
- codul în afara nomenclatorului dă refuz.

Cheia rămâne fără `Cauza` (S3-Q1).

**StockMovement** = (eveniment, cod): o tranzacție pe două coduri (ASM 70 și
20) dă două mișcări.

- `MovementReference`: S3-D5.
- `MovementDate` = `Tranzactie.Data`.
- `MovementPostingDate` = data UTC din `Tranzactie.ScrisLa`, același reper ca
  SystemEntryDate din GL (R2). Fără `MovementPostingTime`.
- `MovementType` = codul; `DocumentReference` = (codul tipului, `Document.Numar`),
  fără `DocumentLine`, fiindcă linia agregă.
- `LineNumber` ordinal.
- `AccountID` = simbolul SAF-T al contului istoric al postării.
- `TransactionID` = `TranzactieId`, numai când tranzacția are postări în GL-ul
  lunii (cusătura S1-R2). Transferul pur nu are.
- `CustomerID`/`SupplierID`: rolul din rândul de politică și terțul extern al
  documentului (`Document.Laturi()`; DSC autogenerat: al sursei), cu
  `TertiLinieStoc`. Rolul cerut fără terț este refuzul `SAFT_TERT_LIPSA`
  (azi avertisment). Roluri diferite în aceeași mișcare dau
  `SAFT_PROVENIENTA_AMBIGUA`.
- `ShipFrom.WarehouseID` pentru Q < 0, `ShipTo.WarehouseID` pentru Q > 0
  (nou; leagă linia de poziție).
- `ProductCode`, `StockAccountNo` (S3-D5), `UnitOfMeasure` = UM produsului,
  imutabilă după operare (S1-R1), factorul 1.
- `MovementSubType` = codul.

Ordinea: mișcările după (Data, ScrisLa, TranzactieId, cod); liniile după
(poziția liniei în document, data deschiderii lotului, LotId, WarehouseID).
`NumberOfMovementLines`, `TotalQuantityReceived`/`Issued` au formula actuală.

### S3-D3 — codurile (SAF-B3.5): tabelul propus

Cheia (tip, categorie, semn) este exhaustivă pentru producătorii de azi.
Magazie și Marfuri primesc același rând.

| Tip | Categorie | Semn | Cod | Rol | Notă |
|---|---|---|---|---|---|
| FCT | Magazie/Marfuri | orice | 10 | Furnizor | **nou**; recepția e a facturii pe cub (T-D3) |
| NIR | Magazie/Marfuri | orice | 10 | Furnizor | recepția proprie și delta, pe ambele semne (S3-Q1) |
| BTR | Magazie/Marfuri | orice | 80 | — | ambele picioare |
| BCS | Magazie/Marfuri | −1 | 70 | — | |
| BCS | Consum | +1 | — | — | excludere, motivul actual (27a) |
| LDI | Magazie/Marfuri | +1 / −1 | 110 / 120 | — | |
| DSC | Magazie/Marfuri | −1 | 30 | Client | |
| RLF | Magazie/Marfuri | −1 | 50 | Furnizor | |
| RDC | Magazie/Marfuri | +1 | 40 | Client | |
| ASM | Magazie/Marfuri | +1 / −1 | 20 / 70 | — | valoarea produsului = ΣC (S3-Q2) |

Rândurile NIR existente acoperă deja delta: semnul null prinde și minusul.
Se adaugă FCT. Celelalte rânduri rămân, iar semantica `TipStoc` devine
categoria contului.

### S3-D4 — poziția (PhysicalStock)

Poziția = (Lot, Cont, Produs, Gestiune) pe o categorie raportabilă.

- **Opening** = Σ postărilor cu Data < începutul lunii, plus postările
  `Deschidere` din lună. Soldul inițial al bazei este inițial, nu mișcare.
  GL îl arată în lună ca jurnal DESCHIDERE (S1-R); diferența se explică în
  cusătura S3-D7b a lunii deschiderii.
- **Closing** = Opening + Σ liniilor lunii.
- Citirea folosește `Loturi.Cumulate` cu snapshot-ul lunii anterioare, în
  aceeași `Fiscale.DeschideCitirea` (R3).
- Poziția intră dacă Opening ≠ 0, Closing ≠ 0 sau are linii în lună (F27-r10).

Câmpuri:

- `WarehouseID`, `ProductCode`, `StockAccountNo` (S3-D5).
- `ProductType` = simbolul SAF-T al contului, ca pe ruta veche (18 caractere).
- `OwnerID` = raportorul; UM; factorul 1.
- `StockCharacteristic` și NC: convențiile existente.
- `UnitPrice` = round(V/Q, 2) pe primul capăt cu Q ≠ 0 (Closing, apoi Opening),
  altfel pe Σ liniilor de intrare din lună. Nu folosim `Lot.PretUnitar` curent
  (SAF-B3.4).

Refuzuri, fiindcă cubul le face imposibile și apariția lor este defect:

- Q < 0 sau V < 0 la un capăt: `SAFT_SOLD_NEGATIV`;
- Q = 0 și V ≠ 0 la un capăt: `SAFT_REZIDU_VALORIC` (FIFO închide lotul la
  valoarea rămasă).

### S3-D5 — cheile (SAF-B3.6)

- `WarehouseID` = `Repartitor.Cod` fără trunchiere. Un cod de peste 35 de
  caractere sau două gestiuni cu același WarehouseID în fișier dau
  `SAFT_CHEIE_NEINJECTIVA`.
- `StockAccountNo` = `{LotId:N}` întotdeauna (32 ≤ 70). Cheia e stabilă între
  luni și nu apare odată cu al doilea lot, ca pe ruta veche.
- Poziția XML (WarehouseID, ProductCode, StockAccountNo, ProductType) trebuie
  să fie injectivă pe (gestiune, produs, lot, cont). `SimbolSaft` scoate
  punctele, iar `ProductType` taie la 18 caractere, deci coliziunea este
  posibilă în principiu. Ea dă tot `SAFT_CHEIE_NEINJECTIVA`.
- `MovementReference` (35 de caractere). Forma lizibilă este
  `{CodTip}-{Numar}`, plus felul evenimentului (nimic pe Operare, `/T` pe
  Transfer, `/S` pe Storno), plus `/{cod}` când evenimentul are mai multe
  coduri. Exemple: `FCT-12`, `FCT-12/S`, `BTR-3/T`, `ASM-4/70`, `ASM-4/T/20`.
  Dacă forma depășește 35 de caractere ori coincide în fișier cu altă formă
  lizibilă, toate mișcările afectate primesc rezerva `{TranzactieId:N}{cod}`
  (≤ 35, fără `-`, deci disjunctă de formele lizibile). Aceasta acoperă și
  numerele duplicate între documente și un eventual al doilea eveniment de
  același fel pe document. Injectivitatea vine din construcție. Referința
  depinde numai de eveniment și de conținutul lunii, deci rămâne stabilă la
  reexportul unei luni închise. Trunchierea și discriminantul `#n` de pe ruta
  veche dispar.

### S3-D6 — acces, citire, comutare

Aceeași formă ca S2-D5:

- `SAFT_ACCES_INCOMPLET` (403) pe ambele uși S, înaintea proiecției.
- `SaftAcces.Citite` se extinde cu tabelele citite de fișierul S. Lista se
  fixează prin captura SQL (SC-SAFT-36 extinsă pe S).
- `Refuzuri` nevid dă 422 pe XML, înaintea primului byte, și 200 pe sumar.
  Ecranul afișează refuzurile.
- Bugetarul rămâne neaplicabil.

Secțiunile comune (GeneralLedgerAccounts, TaxTable, UOM, Products,
AnalysisTypeTable) folosesc cititorii S1 pe cub, nu copii.

După gate, `…/saft/stocuri` și `…/saft/stocuri/xml` citesc proiecția pe cub.
`SaftProiectii.SaftStocuri` rămâne numai oracol A/B în ModelCheck și sursă
Import1C (înghețat, 091-r4). Se șterge la gate-ul final, cu L vechi (R1).

### S3-D7 — cusături (probe pe fiecare lună a scenei, ambele sensuri)

(a) **Poziție:** Opening + Σ liniilor = Closing, exact, pe Q și pe V.
(b) **Cont:** Σ Closing V pe cont (categorii raportabile) = soldul contabil al
contului la capătul lunii, din același reper, minus postările fără lot pe acel
cont. Acestea din urmă se listează pe document, ca `Componente`: o NTC
directă pe 371 este diferență explicată, nu refuz. În luna deschiderii se
compară și Opening cu inițialul GL plus jurnalul DESCHIDERE.
(c) **Eveniment:** pentru mișcările cu `TransactionID`, Σ BookValue pe cont =
Σ postărilor pe lot ale aceleiași tranzacții în GL. Pe `Transfer`, Σ Q = 0 și
Σ V = 0 pe mișcare.
(d) **Conservare:** Σ liniilor emise + Σ liniilor excluse = Σ tuturor
postărilor pe lot din lună. Nu există „neincluse”: ce nu e emis e exclus cu
motiv sau refuzat.
(e) **Probă duală (SAF-D3=C):** categoria derivată față de
`RegistruStoc.TipStoc`, rând cu rând, prin corespondența
document/linie/lot/gestiune/sens. Egalitate, diferență, absență sau
ambiguitate, cu motiv; NIR vechi ↔ FCT nou nu e egalitate declarată.

### S3-D8 — probe, A/B și perf

Scenariile S3 sunt SC-SAFT-07…09, 12 și 38…49 din [SAFT.md](scenarii/SAFT.md).
Ele sunt create prin documente și comenzi reale (operare, storno, corecție,
constatare NIR, inventar, asamblare), fără fapte inserate în cub.

Certificarea S pe lunile scenei, pe manifestul S0: XSD v249 și DUK J2.2.18,
fără atenționări. S0-R8 se extinde cu harta liniei de mișcare
(MovementReference, LineNumber) → mulțimea `(Spatiu, ID)` a postărilor
agregate și harta poziției → postările ei. Mutanții (linie omisă, postare
permutată între loturi, cod schimbat) sunt respinși.

A/B contra `SaftStocuri`, pe cheia document × storno × lot × gestiune, în
ambele sensuri. Diferențele declarate exact:

- recepția pe FCT față de NIR în registru, cu NIR delta ca rest;
- contul istoric față de contul curent al tipului de material;
- valoarea FIFO a cubului față de registru, inclusiv Δ ASM;
- WarehouseID pe cod, cheile noi, avertismentele devenite refuzuri.

Orice altă diferență este defect.

Perf (SAF-B7): numărul de comenzi SQL nu crește cu numărul mișcărilor
(măsurat la n și 2n); Opening citește snapshot-ul, nu tot istoricul.

### S3-D9 — gate și oprire

Gate S3 = fișierul S complet:

- probele S3 verzi pe ambele profiluri, fără regresie S1/S2;
- XML-ul S al lunilor scenei certificat cu manifest;
- probele HTTP pe host viu (403, 422, neaplicabil pe bugetar);
- A/B clasificat și proba duală raportată;
- review advers Codex închis.

Abia apoi comută C. Urmează gate-ul final comun al SAF-T (SAF-B8).

Oprire:

- o mișcare sau poziție fără proveniență completă;
- o cusătură S3-D7 care nu se închide;
- o formă respinsă de DUK (ShipFrom/ShipTo, `StockAccountNo` = lot,
  rezerva `MovementReference`): revenim cu alternativă, fără cheie
  trunchiată;
- un producător nou pe lot pe care tabelul S3-D3 nu îl acoperă.

### S3-Q — întrebările pentru owner

**S3-Q1 — NIR delta (SAF-B3.5).** Constatarea NIR după FCT mută −q (lipsă,
oricare dintre cele cinci cauze 099) sau +q (plus, 408) pe lotul recepționat
de factură. Recomandare **A**: codul 10 cu cantitatea semnată, adică
corecția recepției facturate. FCT a declarat 10 × +4, iar recepția reală a
fost 3, deci NIR declară 10 × −1. Netul 10 × +3 coincide cu ce declara ruta
veche (recepția era pe NIR). Cauza contabilă (473/461/6xx/32x) rămâne în GL.
Cheia politicii nu are nevoie de `Cauza`, iar SAF-D3=C se amendează pe acest
punct: `Cauza?` intră când apare un producător care o cere (restanță numită).
**B**: minusul primește cod pe cauză (de exemplu 120/160 pentru
imputabilă/perisabilitate). Aceasta cere `Cauza` derivată din contul
contrapartidei, iar pe privat Perisabilitate și Neimputabila au același
cont 6xx, deci nu sunt injective. Nu există nici cod ANAF pentru „în
clarificare” ori „pe drum”.

**S3-Q2 — Δ ASM.** Recomandare **A**: acceptăm. Nu este o limită a
cubului: pe cub, ΣP + ΣΔ = ΣC, deci producția 20 la valoarea finală egalează
exact consumurile 70 evaluate FIFO. O linie separată 100 pentru Δ ar
dezechilibra mișcarea. Δ este diferența față de registrul vechi, intră în A/B
și dispare la TR-D9. **B**: persistăm Δ distinct. Varianta B schimbă
modelul cubului, contrar SAF-D3=C („nu adăugăm coloane pe Postare”).

### S3-R — tranșările owner-ului (2026-09-29)

**S3-R1 — NIR delta.** Varianta A: constatarea NIR după FCT, pe oricare
cauză 099 și pe ambele semne, este codul 10 cu cantitatea semnată (corecția
recepției facturate). Cauza contabilă rămâne în GL. SAF-D3=C se amendează
pe acest punct: cheia `PoliticaMiscareSaft` rămâne (TipDocument, TipStoc,
Semn?), iar `TipStoc` devine categoria contului. `Cauza?` și mișcările numai
valorice intră odată cu primul producător care le cere, ca restanța
**SAFT-r2**.

**S3-R2 — Δ ASM.** Varianta A: producția 20 se raportează la valoarea finală
a cubului (ΣP + ΣΔ = ΣC), fără linie separată pentru Δ. Δ rămâne diferență
declarată în A/B față de registrul vechi (SC-SAFT-44) și dispare la TR-D9.
Limita din SAF-D3=C („Blocaj S3 identificat”) este astfel închisă.
