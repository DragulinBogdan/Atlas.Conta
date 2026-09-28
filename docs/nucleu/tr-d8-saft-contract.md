# TR-D8 — SAF-T pe cub: delimitare aprobată și contract S1

**2026-09-28 — SAF-D1/D2/D4=A și SAF-D3=C aprobate de owner.
S1 aprobat de owner cu S1-D5 = B' (381 + 384); review-ul și regulile
de derivare sunt în [S1-R](#s1-r--review-și-tranșări-2026-09-28).**

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

**Gate S0 neîndeplinit:** fixarea artefactului XSD utilizabil și verificarea
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
  contrapartidei. Mai multe conturi produc sublinii exacte, câte una per
  postare, fără proratare.
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
Refuzul `SAFT_ACCES_INCOMPLET` și probele HTTP SC-SAFT-13 se livrează la
comutarea L, în S2, pe ruta reală. În S1 rămân proba RepeatableRead
(SC-SAFT-17) și refuzul unei tranzacții ambiante mai slabe.
