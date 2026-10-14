# TR-D9b-CV — revizia după review-ul Claude

- Tip: analiza
- Data: 2026-10-12
- Alimentează: [contractul CV](tr-d9b-corectii-valorice-contract.md), [matricea pe postări](tr-d9b-corectii-valorice-producator-si-compensare.md)

Review primit de la owner în conversație. Constatările majore sunt
confirmate în surse prin CodeGraph; recensământul complementar cu `rg`
acoperă `Module` fără migrații. Nu s-a schimbat codul produsului și nu
s-a rulat catalogul CV. **Punctul 2 rămâne neacceptat.** Acest raport
corectează propunerea și face explicite cerințele încă neprobate.

A doua rundă (2026-10-13): owner-ul a fixat apartenența compensării la
corecția A. R1, R3 și R4 sunt aduse la zi; comparația și obligațiile noi
sunt în R6.

A treia rundă (2026-10-14): delimitarea pasului 2 este închisă la cererea
owner-ului, pe verificarea contra codului din R8. Afirmația de mai sus
despre punctul 2 este istorică.

## R1 — forma compensării

Prima matrice trecea `Conservare`, dar k1/k2 fără inversă și j1/j2 inverse
de Storno încălcau `Citiri.Invarianti.VerificaProvenienta:159`.
`Explicatii.VerificaAcoperire:67` respingea k1/k2;
`Contabil.Postari:72` omitea toate patru. După storno B, contabilul ar fi
arătat stoc1.080/consum120, deși suma brută a cubului dă1.200/0.
Afirmația inițială „formă admisă în Storno” era insuficientă și este retrasă.

| Variantă | Consecințe | Recomandare |
|---|---|---|
| Compensare în Storno, discriminator explicit | Necesită schimbarea regulii inversei verificabile, a oglinzii explicației și a filtrului contabil; inversa compensării ar indica tot Storno. Discriminatorul ar trebui persistat și urmărit de cititori. | nerecomandată: extinde semantica Storno pentru un fapt care nu este inversă |
| Operare distinctă în aceeași comandă | D602−120/C302−120, cantitate0, `Atribuit=a3`, fără `InversaDin`. Inversa ulterioară este Storno cu original Operare. Cele trei reguli de mai sus rămân valabile. | recomandată, cu obligațiile de mai jos; compatibilitatea integrală nu este încă demonstrată |

Comanda storno B scrie Storno B (s1/s2) și câte o Operare pe fiecare
document de corecție cu compensare nenulă (aici Operare A, k1/k2), apoi
starea B devine Stornat. O singură tranzacție de bază de date: eșecul
oricărei scrieri sau al explicației anulează tot, inclusiv starea. Nu se
cheamă `Opereaza` pe B sau pe A și niciun document nu trece înapoi prin
Draft. Inversarea A scrie i1…i4 și j1/j2 într-o tranzacție Storno A;
originalul fiecărei j este acum Operare, nu Storno. `Conservare` păstrează
toate gardurile.

`Tranzactie.Fel` descrie faptul, nu starea actuală a documentului. În
cititorii enumerați mai jos nu există un filtru general „Operare numai
dacă documentul este Operat”; modelul tranzacției nu impune unicitate
Operare/document. Totuși, a doua Operare pe A contrazice litera lui 090 (a)
și cere probe explicite pentru explicație, cronologie, proiecții și
retragerea lui A. Nu este certificată numai prin schemă.

### Explicația proprie a compensării

`Explicatii:58` cere JSON propriu pentru fiecare Operare; `ExplicatieDinId`
singur nu ajunge. Nici copia explicației B originale nu merge: ea ar
declara ieșirea60/600 într-o tranzacție cu zero ieșiri cantitative, iar
`Explicatii:118–127` o respinge.

Propunere de contract pentru explicație: declarantul este cel al
documentului A (`DeclarantFacturaIntrare` în fixture, cerut de verificarea
de la107–110), cu decizie nouă de compensare valorică și ipotezele familiei.
Decizia conține linia A, rădăcina a3, beneficiarul inversat (linia B,
referința b1 și inversa ei), pozițiile sursă/destinație, suma semnată120 și
referințele faptelor din care rezultă suma activă. Nu conține
`ValoareIesire`/`ValoareDeclarata` artificială cu q0 și nu schimbă liniile
A sau B. Prima Operare a lui A are, analog, decizii explicite de alocare
valorică. Câmpul `Declarant` numește clasa documentului, nu mecanismul care
a calculat compensarea; sensul lui se fixează odată cu tipurile de decizie.

Trebuie extinse tipurile deciziilor/codec-ul (`Cub/Explicatie.cs:79,158`),
verificarea corespondenței cu perechile valorice și DTO-ul explicației.
Faptul că actualul verificator ar accepta o listă goală nu este probă
suficientă. Storno A trebuie să expună explicațiile originale A și K prin
`InversaDin`; k1/k2 cer exact o decizie de compensare pe pereche/rădăcină.
Aceasta extinde auditul explicației, fără relaxarea oglinzii exacte de la67.

## R2 — gardul temporal precedă suma

`MotorOperare.Storneaza:234` verifică azi numai data înregistrării
documentului. Pentru CV se cere în plus `dataComenzii >= max(data faptelor
familiei afectate)`, sub blocaj, după descoperirea familiei **fără filtru de
dată**. Sunt incluse rădăcinile, beneficiarii, perechile, compensările,
inversele lor și dependenții valorici relevanți, inclusiv cei neutralizați.
Nu se ignoră un fapt viitor pentru că documentul lui e deja Stornat.

Gardul se aplică și stornării beneficiarului B, nu numai corecției A.
Aceeași zi este permisă; ordinea serializată a comenzilor decide suma.
Istoricul nu se repară prin calcul pe toate faptele cu dată viitoare:
aceasta ar corecta finalul și ar strica lecturile intermediare.

| Caz | Rezultat cerut, fără postări noi la refuz |
|---|---|
| A15.01; storno A25.01; apoi storno B datat20.01 | `CV_CRONOLOGIE`; la22.01 rămân40/480, consum720; după25.01 rămân40/400, consum600 |
| A15.01; storno B datat14.01 | `CV_CRONOLOGIE`, deși B este din10.01; nu mută consumul înaintea corecției |
| A15.01; storno B25.01; apoi storno A datat20.01 | `CV_CRONOLOGIE`; la22.01:40/480, consum720; după25.01:100/1.200, consum0 |
| A15.01; storno A25.01; storno B25.01 ulterior în ordine serială | acceptat, compensare0, final100/1.000, consum0 |

SC-CVL-27 fixează aceste cazuri. Gardurile privind ieșirile ulterioare din
CV-D5 rămân separate; gardul temporal nu le înlocuiește.

## R3 — predicatul și selecția, fără reinterpretarea partidelor

Pentru pilot, clasificarea structurală se verifică față de rolul declarat
în explicație/operand. „Are Atribuit” nu este un predicat suficient.
Referințele trebuie să existe, să fie mai vechi în ordinea scrierii și să
respecte aceeași carte, același produs și aceeași identitate de lot.

| Rol | Predicat necesar și verificări |
|---|---|
| Beneficiar b | postare Operare pe lot, `InversaDin=null`, `Atribuit=null`, `Cantitate>0`; este recepția internă sau piciorul intern de consum din fixture; contrapiciorul cantitativ se identifică prin tranzacție/pereche |
| Rădăcină r | postare Operare pe lot, q0, `InversaDin=null`, `Contrapondere=null`, `Atribuit=b.ID`; b satisface predicatul beneficiarului; poziția r reproduce cont/repartitor/lot/produs/carte/analiză ale b; contrapiciorul r este terțul aceleiași perechi, cu `Contrapondere.Tert` și fără atribuire; explicația o declară alocare |
| Pereche de compensare k | ambele postări sunt în aceeași Operare, pe lot, q0, fără inversă și contrapondere, cu aceeași atribuire la o rădăcină r de consum validă; cauza și proprietarul sunt documentul/linia rădăcinii r; D la poziția de consum și C la poziția de stoc a contrapiciorului b, valoare egală `−activ(r,b)`; explicația declară compensare și o leagă de inversa exactă b scrisă de aceeași comandă |
| Inversă i/j | Storno, referință completă la o sursă ne-Storno, măsuri negate exact, coordonate istorice/cauză/atribuire păstrate conform regulii comune de dată/fiscal; nicio a doua inversă a aceleiași surse |

Rădăcina care țintește altă alocare este invalidă: ținta are q0, nu este
beneficiar. Nu se reclasifică tacit drept compensare, deoarece nu are
perechea internă, tranzacția proprie și decizia de compensare cerute.
Mutantul „alocare cu țintă alocare” se adaugă la SC-CVL-26.

La storno B, calea CV **înlocuiește** selecția cauzat ∪ atribuit pentru
aceste rădăcini: inversează numai faptele proprii eligibile și calculează
compensarea. Interogarea actuală `Materializare:110` ar lua a3 și i3,
fiindcă inversa păstrează atribuirea; nu se reutilizează nediferențiat.
Nominalizarea de partidă (`Materializare:201`) își păstrează propria cale.

La storno A, k1/k2 sunt postări proprii din Operare, deci selecția
cauzatelor produce exact i1…i4 + j1/j2, fără cauze străine; saltul pe
`Atribuit` le regăsește și se deduplică. Nu se inventează o selecție
recursivă nouă. Cele două
salturi descriu familia folosită la activitate și cronologie. Se elimină
duplicatele după `(Spațiu, ID)` și sursele deja inversate; nu se inversează
inverse. Predicatul de mai sus și gardurile sunt noi, selecția cu un salt
nu le dovedește.

090 (i) este deja amendată semantic de 091 (h). În decizia090 se completează
marcajul `[amendat de 91]`, păstrând textul istoric; antetul091 numește
90 (i), iar amendamentele gate-ului rămân descrise în Execuție. Pentru forma fizică nouă
va fi necesară o decizie numerotată la acceptare; prezentul draft nu o
activează. Aceeași decizie amendează 090 (a), care cere încă „EXACT o
tranzacție de fel `Operare`” pe document; amendamentul 110 de pe (a)
privește altceva. Lipsa marcajului la090 (f) pentru S-D14 este restanță documentară
veche, raportată; nu se inventează un număr de decizie pentru contractul S-D14.

Decizia cerută aici este [119](../decizii/119-compensarea-valorica-apartine-corectiei.md), scrisă la 2026-10-13 la
cererea owner-ului: amendează 090 (a) și (i) și fixează apartenența din R6.

## R4 — inventarul cititorilor de cauză și de proprietar

Rădăcină căi: `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/`.
După R6, k1/k2 și j1/j2 au cauza și proprietarul A; divergența dintre
cauză și proprietar din prima redactare a dispărut. Tabelul păstrează
cititorii verificați, cu consecința pe varianta aleasă.

| Consumator verificat | Comportament actual / consecință / așteptare de probat |
|---|---|
| `Cub/Citiri/Contabil.cs:72,84,93` → `Proiectii/ContabilProiectii.cs:241,510,612` | Filtrul include noua K Operare și J inversă a K. Atomii contabili poartă cauza; sumele trebuie1.200/0 după B și1.000/0 după A. Fișa și jurnalul arată K și J sub numărul lui A, cu K la data stornării lui B. |
| `Cub/Citiri/Loturi.cs:48,64,161` | `PostariCitite.DocumentId` este proprietarul; excluderea `faraDocument` și gardul retragerii folosesc cauza. La K și J cele două coincid (A); nu se schimbă sensul câmpului. Excluderea `faraDocument = A` scoate întreaga familie a corecției, inclusiv K. Soldul/snapshot-ul adună fapte și nu filtrează starea documentelor. |
| `Cub/Citiri/Explicatii.cs:31,54` și `Api/Explicatii/ExplicatieTranzactieDto.cs` | Răspunsul numește proprietarul tranzacției, sursele se găsesc prin inverse. K cere JSON propriu, pe A; Storno A expune explicațiile ambelor Operare ale lui A. Ușa HTTP cere citirea completă a domeniului, nu acces pe document. |
| `Saft/SaftProiectii.PeCub.cs:303` — GL | Grupează pe tranzacție și ia documentul/tipul/numărul din prima cauză. Cu K pe A, toate rândurile unei tranzacții au aceeași cauză, deci antetul nu depinde de ordine; schimbarea cerută în prima redactare nu mai e necesară. Fragilitatea rămâne pentru orice Storno viitor cu cauză străină. Martor cu ordinea postărilor permutată. |
| Același fișier`:379,389,419` — corecții/facturi | Filtrează pe tipul cauzei înainte de gruparea pe tranzacție. K are cauză FCT: tranzacția ei nu are picior de terț și nici fapte fiscale, deci e sărită fără refuz (`:431–434`). La storno A, j1/j2 intră în grupul liniei lA și îi schimbă distribuția pe conturi față de factura inițială. **Regulă de definit și probat:** evenimentul de factură separă perechile comerciale de compensările interne (perechi fără picior de terț). Se probează exact un eveniment de inversă FCT, brut−200. Alegerea nu rezolvă reprezentarea q0/multicont. |
| `Saft/SaftProiectii.PeCub.Stocuri.cs:111,218,499` | Preia proprietarul din Loturi și îl compară cu GL grupat pe cauză. La K și J cele două coincid, deci componentele se închid pe tipul FCT; schimbarea cheii nu mai e necesară pentru acest caz. |
| Același fișier`:229` | Refuză q0/v≠0 (`RefuzMiscareValorica`, SAFT-r2), deci nici A, nici K/J nu au export de stoc acceptat astăzi. Necesită contract de reprezentare a ajustării valorice; nu se inventează cantitate și nu se omit sumele din PhysicalStock/reconciliere. |
| `Cub/Citiri/Partide.cs:184,205,295`, `OriginiPartide.cs`, `Materializare.Origini.cs` | Calea partidelor vede a2/a4 și inversele lor; K/J nu au unitate Partida și nu trebuie să creeze origini. Rest A−200 după B, apoi0 după A; originea rămâne pentru istoricul stornat. |
| `Cub/Citiri/Plati.cs:62,79`, `SaftProiectii.PeCub.Plati.cs:14,59` | Identifică documentele pe cauză; exportul plăților restrânge tipurile de plată. BCS/FCT nu devin plăți prin K/J. Proba SC-CVL-24 păstrează stingerea A separată de lot. |
| `Cub/Citiri/Fiscale.cs:72`, `Motor/FiscalitateService.cs:58`, `DiagnosticTvaService.cs:60`, `TvaProiectii`, `D394Proiectii` | Filtrul este faptul fiscal. K/J nu au bloc fiscal, nu adaugă D300/D394. A este document fiscal distinct când se va contracta cazul taxabil; o legătură comercială nu activează inversa tehnică. |
| `Cub/Citiri/Receptii.cs`, `Cub/ReceptiiConexe.cs:80` | Citesc cauza FCT și Operare pe lot pentru recepția conexă. FCT corectivă q0 nu trebuie recunoscută drept recepție cantitativă; fără NIR nou, fără reancorarea recepției vechi. O FCT mixtă (recepție + corecție) ar primi prin K a doua tranzacție cu rânduri de lot pe debit și ar pica pe univocitatea de la `:88–90`; factura mixtă rămâne în afara pachetului. |
| `Cub/Citiri/Imobilizari.cs`, `Imobilizari.Conturi.cs` | Filtrele de fișă/conturi IMO nu sunt domeniul acestui pilot. Excluderea imobilizărilor rămâne gard; nu se deduce compatibilitate pentru un pachet viitor. |

Acesta este inventar static, nu certificat de compatibilitate. Trebuie
adăugate probele HTTP ale explicației și accesului la cauză/proprietar,
inclusiv când unul este ascuns rolului; disponibilitatea postării nu
autorizează dezvăluirea documentului ascuns. Exportul SAF-T își păstrează
gardul de acces complet. Nu s-a rulat hostul în această revizie.

## R5 — cititorii lui CorecteazaId și exportul FCT

Recensământ `rg CorecteazaId Module`, fără migrații; consecințele alegerii
se documentează acum, chiar dacă TVA-ul pilotului este zero:

| Loc | Efect al legăturii / consecință pentru FCT nouă |
|---|---|
| `Motor/CorectieService.cs:31,60,80`, `GardianEditare.cs:400,431,448` | copie, unicitate, original Stornat, motiv și protecția legăturii; nu sunt semantica unei delta peste original Operat |
| `Motor/FiscalitateService.cs:37,96` | atribuirea corecției tehnice și refuzul depunerii cât înlocuitorul e Draft; FCT comercială nouă urmează propriile repere, conform103 (b/c) |
| `SaftProiectii.PeCub.cs:141,383,454`, `SaftReguli.cs:214` | `CorecteazaId=null`: +200 ar fi380, −200 ar fi381;384 numai la reemitere legată; inversa este381. Acestea sunt ramuri ale codului, nu certificare fiscală a noului export. |
| `Api/ApiProiectii.cs:25`, `BusinessObjects/Documente/Document.cs:82,152` | afișarea corecției și motivului folosește legătura tehnică; sursa comercială de pe linie necesită afișare proprie, fără să pretindă că originalul a fost înlocuit |
| `Api/Itv/InchidereTvaApply.cs:71,111`, `BusinessObjects/Documente/InchidereTva.cs:52` → `InchidereTvaService.DataSoldurilor` | tăietura soldului ITV; nu se extinde semantica acestui parametru la referința comercială FCT |
| `BusinessObjects/BackOfficeDbContext.cs:322,325` | FK/index al legăturii tehnice; noua referință comercială este distinctă și rămâne de modelat pe frunză |

În plus, `SaftProiectii.PeCub.cs:494–516` are **două refuzuri deja pentru
pilotul fără TVA**: linie cu contrapartide pe302 și602, respectiv cantitate
comercială0. Nu se ajunge la un export valid doar alegând InvoiceType380.
Nu se introduce q1 în document pentru a ocoli cititorul. Reprezentarea
comercială în export, distribuția pe conturi și mișcarea valorică de stoc
nu pot fi amânate tacit sub eticheta „TVA în afară”. Owner-ul le-a amânat
explicit la 2026-10-13: restanță cu refuz (R7, CV-D9), nu blocaj.

## R6 — apartenența compensării (2026-10-13)

Analiza Claude pe cititori, observațiile Codex și decizia owner-ului.
Într-o `Operare`, cauza este documentul tranzacției
(`Conservare.VerificaDocumentul:101`), deci cauza și proprietarul lui K se
aleg împreună: beneficiarul B sau corecția A. Aritmetica e aceeași.

| Loc | K pe beneficiarul B | K pe corecția A (ales) |
|---|---|---|
| Storno A (`Materializare:101–112`) | K găsită prin saltul pe `Atribuit`; J cu cauză străină | K între postările proprii; nicio cauză străină |
| Document Stornat | B rămâne cu 302 +120/602 −120 cât timp A e activă | B se închide la zero pe fiecare cont; A poartă integral delta |
| Antetul GL și reconcilierea de stoc SAF-T | două schimbări necesare | niciuna pentru acest caz |
| Fișa și jurnalul | stornarea lui A scrie rânduri cu numărul lui B | toate rândurile corecției poartă numărul lui A |
| Retragerea lui A | K rămâne orfană; refuzul e obligatoriu | K ar fi ștearsă odată cu A; refuzul rămâne regulă (CV-D5) |
| Găzduirea noii Operare | fiecare tip beneficiar | producătorii de corecții |

Regula fixată: **K aparține documentului și liniei care au produs alocarea
compensată. Se scrie la data stornării beneficiarului, într-o tranzacție
Operare suplimentară a corecției, cu explicație proprie și legătură
verificabilă către beneficiarul inversat. Comanda scrie atomic inversa
beneficiarului și toate compensările.** Temeiul este apartenența efectului:
cei 120 provin din A, K le schimbă repartizarea între consum și stoc, iar
stornarea lui B doar declanșează mutarea. 091 (h) cere două componente
într-o comandă atomică, fără să impună că ambele aparțin documentului stornat.

„1 + N” înseamnă o tranzacție Storno B și câte o Operare pentru fiecare
document de corecție cu compensare nenulă. Rădăcinile aceluiași document au
perechi distincte în aceeași tranzacție.

Limite și obligații care rămân:

1. Retragerea lui A cu compensări active rămâne refuzată în primul pachet.
   Ștergerea postărilor proprii ar elimina și K și ar da finalul numeric
   corect în exemplul simplu, dar apartenența nu amendează CV-D5. Relaxarea
   cere scenariu separat pentru dependenți, referințe istorice și
   perioadele tuturor faptelor șterse.
2. 090 (a) spune încă „EXACT o tranzacție de fel `Operare`”. Precedentul
   împerecherii (`Transfer` scris pe un document Operat) arată că se adaugă
   fapte ulterior, nu că a doua Operare este admisă. Ambele variante cer
   amendarea explicită; absența constrângerii în cod nu ține locul deciziei.
3. Avantajul pe tipurile beneficiare este real, dar limitat: scade suprafața
   afectată de apartenență. FCL, BTR și ASM cer în continuare scenarii
   proprii pentru identificarea beneficiarului și destinația compensării.
4. Costul SAF-T rămâne explicit: K fără terț și fără fapte fiscale este
   sărită de cititorul facturilor, iar J intră în grupul liniei A la storno.
   Separarea perechilor comerciale de compensările interne se definește și
   se probează; alegerea nu rezolvă reprezentarea q0/multicont.
5. Comanda storno B blochează fiecare document de corecție pe care scrie.
   Se probează storno B concurent cu storno A și rollback-ul după scrierea
   numai a unei părți dintre cele N compensări (SC-CVL-29).

Neacoperit de analiza pe cititori: filtrele de egalitate pe document scrise
cu alt nume de parametru decât `p`/`t`/`o`, listele XAF pe postări, clientul
React și aserțiunile ModelCheck. Verificarea a fost citire statică.

## R7 — storno recepției, explicația și SAF-T (2026-10-13)

Trei completări cerute de owner după R6; textul normativ este în contract.

**Storno recepției.** R3 descria înlocuirea selecției numai la storno B.
Recepția este și ea beneficiar: a1 și inversa ei i1 au `Atribuit = f1`,
iar `Materializare:110` le-ar lua fără perechile lor. Din citirea codului,
rezultatul ar fi `PerecheInvalida` (`Conservare:43`), nu un refuz de
domeniu; cazul nu a fost rulat. Regula din CV-D5: cu valoare de corecție
activă pe stoc, storno F se refuză cu `CV_DEPENDENTI`; cu toate corecțiile
neutralizate, conține exact inversa recepției. Retragerea beneficiarilor
unei corecții, chiar stornate, rămâne refuzată: recepția prin gardul
loturilor folosite (`MotorOperare:306`), BCS prin `CV_DEPENDENTI`. SC-CVL-30.

**Explicația.** Propunerea din R1 devine CV-D8: două feluri noi de decizie
în ierarhia închisă a nucleului (`Motor/Decizie.cs`), fără ipoteze noi,
cu versiunea explicației neschimbată. De atins la implementare: codec-ul
(`Cub/Explicatie.cs:120,158`), gruparea pe linie (`:60,79`), invariantul
(`Citiri/Explicatii.cs:54`) și DTO-ul liniei
(`Api/Explicatii/ExplicatieTranzactieDto.cs:72`). Faptele familiei nu se
copiază în explicație; invariantul le recitește din cub. SC-CVL-31.

**SAF-T.** Refuzurile există și opresc emiterea fișierului
(`SaftXml:92`, `SaftController:153`): `SAFT_SURSA_INCOMPLETA` și
`SAFT_PROVENIENTA_AMBIGUA` pe factură, `SAFT_MISCARE_VALORICA` pe stocuri.
Luna în care o corecție este operată sau stornată nu se poate exporta. Luna
cu numai compensarea trece în D406 lunar: tranzacția ei nu are picior de
terț, deci cititorul facturilor o sare (`SaftProiectii.PeCub:431`), iar în
GL apare sub jurnalul FCT. Restanța este CV-r1, lângă SAFT-r2, care numea
deja „primul producător pe lot” ca moment de intrare. SC-CVL-32 fixează
refuzurile.

## R8 — închiderea delimitării pasului 2 (2026-10-14)

Owner-ul a cerut închiderea delimitării: forma compensării (R1),
predicatele (R3) și explicația (CV-D8). Verificarea este citire de cod pe
`main` la `d0f7a2f3`, fără rulare. Textul normativ este în contract
(CV-D1, CV-D8, CV-D10). Tranșările de mai jos schimbă propunerea și sunt
ale reviziei; owner-ul le poate întoarce înaintea închiderii pasului 3.

Perimetrul, „punctul 3” al review-ului, a fost hotărât separat de owner în
aceeași zi: corecția taxabilă refuzată, factura mixtă refuzată,
eligibilitatea restrânsă la lanțul FCT → NIR fără diferențe → BCS,
corecția neantedatată și refuzul retragerii sau stornării ei cât există o
ieșire ulterioară activă.

### Ce confirmă codul

| Afirmație din R1, R3, R6, R7 | Loc verificat | Rezultat |
|---|---|---|
| Perechea K (D consum / C stoc, aceeași valoare negativă, q0) trece conservarea în `Operare` | `Conservare.VerificaPerechile:48–58`, `VerificaDocumentul:103`, `VerificaFormele:216–233` | confirmat: laturi opuse, valori egale, aceeași cauză; unitatea lotului poartă contul capătului |
| K și inversa ei intră în contabil | `Citiri/Contabil.Postari:78–80` | confirmat: `Operare` și `Storno` cu original `Operare` |
| Inversa lui K are origine verificabilă | `Citiri/Invarianti.VerificaProvenienta:161–164` | confirmat: originalul nu este `Storno` |
| A doua `Operare` cere explicație proprie | `Citiri/Explicatii.VerificaAcoperire:58` | confirmat: referința `ExplicatieDinId` nu ajunge |
| K nu este ieșire pe lot și nu cere decizie de valoare a ieșirii | `Citiri/Explicatii:81–88` | confirmat: filtrul este `Cantitate < 0` |
| Partida proprie a corecției nu cere `AlocareFifo` | `Citiri/Explicatii:156–161` | confirmat: `IdentitatiPartide.EsteProprie` |
| Stornoul corecției găsește K între postările proprii | `Materializare.Storneaza:101–105` | confirmat: filtrul este `DocumentId` și `Operare`, fără limită pe numărul tranzacțiilor |
| Stornoul beneficiarului ar lua rădăcina și inversa ei | `Materializare.Storneaza:110–112` | confirmat: saltul pe `Atribuit` nu privește felul tranzacției |
| Cititorul explicației compune mai multe origini la storno | `Citiri/Explicatii.PeTranzactie:37–49` | confirmat |
| Comenzile sunt serializate pe bază | `TranzactieComanda.BlocajScriere` (97000) | confirmat; blocarea documentului de corecție se adaugă prin `Materializare.BlocheazaDocumente` |

### Ce nu ținea și cum se tranșează

**R8-1 — atribuirea pe mișcare.** `N.Miscare` nu poartă `Atribuit`, iar
`Miscare.Postari` scrie postările fără el. Singura scriere de azi
(`Materializare.Imperecheaza:201`) îl pune după contractare, pe toate
postările transferului. Rădăcina cere atribuire numai pe piciorul intern,
compensarea pe ambele. Mișcarea capătă atribuirea fiecărui capăt, ca
postările verificate de `Conservare` și explicate să fie cele scrise.

**R8-2 — forma fizică a selecției.** La stornare, saltul pe `Atribuit` nu
mai urmează postările pe lot. Nominalizarea de partidă atribuie numai
postări de partidă ale aceluiași document, deci calea ei rămâne cea din
090 (i). La stornoul corecției, K sunt postări proprii. `Storno.Selecteaza`
din nucleu nu se schimbă.

**R8-3 — linia corectată se derivă din lot.** CV-D1 propunea o referință
la linia FCT de recepție, lângă `LotId`. Lotul o are deja:
`Lot.LinieIntrareId`, prezent în operand (`LotFapt`). O a doua referință ar
cere un gard de egalitate fără să aducă informație în primul pachet.
Rămâne numai felul explicit al liniei. Referința proprie intră când sursa
comercială nu este linia de intrare a lotului (facturarea avizului).

**R8-4 — bijecția din CV-D8 contrazicea SC-CVL-06.** Punctul 1 cerea o
decizie pentru fiecare postare și invers. Punctele 2 și 3 cer suma
ponderilor și reproducerea repartizării, deci și decizia beneficiarului
căruia Hamilton îi dă zero: al doilea bon din SC-CVL-06 are pondere 1 și
valoare 0, fără postare. Regula corectată: decizie pentru fiecare beneficiar
cu pondere pozitivă; postare numai pentru decizia cu valoare nenulă.

**R8-5 — referința inversei iese din decizia de compensare.** Inversa
beneficiarului este unică și se găsește prin `InversaDin`; punctul 4 al
invariantului o recitește oricum din cub. Decizia păstrează rădăcina și
beneficiarul.

**R8-6 — gruparea repartizării se recitește din cub.** Decizia poartă
unitatea beneficiarului (lot × cont), nu cheia completă. Invariantul ia
poziția din postarea beneficiară referită, apoi reface repartizarea.

**R8-7 — proba de harness a operării.** `ProbeCub.ProbaOperare` cere
„cel mult o `Operare`” și toate tranzacțiile la data înregistrării. Codifică
litera veche a lui 090 (a). După 119 (a) rămâne valabilă numai până la prima
compensare; textul probei se aduce la zi odată cu scenariile.

**R8-8 — dependenții pe care codul nu-i oprește azi.**

| Comandă | Comportament actual | Gard cerut |
|---|---|---|
| retragerea bonului cu alocări | nimic nu refuză; `Atribuit` ar rămâne spre o postare ștearsă | `CV_DEPENDENTI` |
| retragerea corecției cu compensări | `Materializare.Anuleaza:252–265` șterge toate tranzacțiile `Operare` ale documentului, deci și compensările | `CV_DEPENDENTI` |
| stornarea corecției datată înaintea unei compensări | `MotorOperare.Storneaza:234` compară numai cu data înregistrării | `CV_CRONOLOGIE` |
| operarea facturii corective | `MotorOperare.GenereazaConex` filtrează liniile numai pe natură (`Potrivire.Conex`), deci ar clona linia cu cantitate zero într-un NIR | liniile de corecție nu intră în conex |
| pregătirea valorii | `FacturaIntrare.BazaLinie` întoarce preț × cantitate, deci ar aduce valoarea la zero | valoarea liniei de corecție se culege |
| ușa entității | `FacturaIntrare.ValideazaOperare:139` refuză cantitatea nepozitivă înaintea declarantului | excepție numai pe felul explicit |

**R8-9 — anul închis.** Produsul nu are închidere de an. Refuzul
`CV_AN_INCHIS` se aplică unui consum beneficiar datat într-un an anterior
anului corecției. Stocul rămas este beneficiar curent, oricând a fost
recepția.

Owner-ul (2026-10-14): închiderea anului merită prinsă ca operație
explicită; pe bugetar este obligatorie, iar pe privat poate ajuta. Anul
calendaristic rămâne gardul primului pachet; când operația există, „an
închis” înseamnă anul închis prin ea (CV-r2, activă: owner-ul a marcat-o
pentru „rotund”).

**R8-10 — ieșirea ulterioară.** Un bon cu cota zero nu are rădăcină, deci
nu se deosebește prin postări de o ieșire făcută după corecție. Deosebirea
este ordinea scrierii: ieșirea ulterioară este ieșirea activă a cărei
tranzacție are `ScrisLa` după prima `Operare` a corecției. Comenzile sunt
serializate, deci ordinea este totală; egalitatea se tratează ca ulterioară.

**R8-11 — activul la stornarea beneficiarului.** Un document se stornează
o singură dată, iar retragerea beneficiarului cu alocări este refuzată.
Compensarea pentru perechea (rădăcină, beneficiar) nu poate preexista, deci
la stornarea beneficiarului activul rădăcinii este valoarea ei, dacă nu are
inversă, și zero altfel. Suma pe familie din matrice rămâne definiția
auditată de invariant, nu calculul comenzii.

**R8-12 — eligibilitatea se judecă pe forma faptelor.** Motorul nu
numește tipurile de documente (25b, 32a). Lotul este corectabil după forma
perechilor care îl ating, din CV-D10; BTR, retur, asamblare, inventar și
recepția cu diferențe ies prin forma lor, fără listă de tipuri.

### Ce rămâne neschimbat

`Contrapondere.Tert` pe piciorul de terț al rădăcinii, chiar cu cantitate
zero: conservarea și `VerificaCoordonate` cer marcajul numai la cantitate
nenulă și niciun cititor nu filtrează pe această valoare. Gardul temporal
din R2 și cele patru cazuri. Apartenența din R6. Storno recepției și SAF-T
din R7. Limitele perimetrului din CV-D1 și CV-D5, aprobate de owner:
corecția taxabilă, factura mixtă și ieșirea ulterioară activă sunt refuzuri.

## Scenarii și acceptare

SC-CVL-26 exclude ordinalele numerice și include mutantul alocare→alocare.
SC-CVL-27 acoperă cronologia; SC-CVL-28 acoperă tranzacțiile 1 + N,
explicația, cititorii și închiderea lui B pe fiecare cont; SC-CVL-29 acoperă
concurența storno B/storno A și rollback-ul parțial; SC-CVL-30…32 acoperă
storno recepției, explicația și refuzurile SAF-T. Nu se relaxează niciun
refuz existent pentru a declara aceste probe verzi.

Punctul 1 are recomandare confirmată de review. Punctul 2 are forma fizică
revizuită și apartenența fixată prin decizia 119, care amendează 090 (a) și
(i). Predicatele și explicația sunt închise prin R8 (2026-10-14). Cere încă
probe de produs pe ambele profiluri. Exportul SAF-T nu mai este condiție: rămâne
restanța CV-r1. Nu este cerută aprobarea pentru implementare în acest pas.

## Artefact și verificarea reviziei

Simulatorul documentar este versionat în
[probe/cvl_matrice.py](probe/cvl_matrice.py). Nu este scriptul Claude din
scratchpad și nu pretinde că rulează produsul. Verifică aritmetica a16
ordini, patru cazuri temporale, perechile, inversa exactă și o reproducere
restrânsă a predicatului de includere din `Contabil.Postari`. Nu verifică
EF/SQL, codec-ul explicației, clasificarea provenienței, exportul sau XAF.

Comenzi executate, din rădăcină:

```powershell
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Infrastructura
python docs/nucleu/probe/cvl_matrice.py --out run-verificari/20261012-223306-537/cvl-matrice.json
docskit gen
docskit check
docskit metrics --check
git diff --check
```

Infrastructura: exit0, build izolat fără avertismente/erori, ambele probe
verzi; manifest `run-verificari/20261012-223306-537/rezultat.json`, zero baze.
Simulatorul:16 ordini și4 cazuri temporale verzi; JSON-ul păstrează toate
postările, soldurile intermediare, refuzurile și SHA256 al scriptului.
`docskit gen/check`, `docskit metrics --check` și `git diff --check` sunt
verzi. Avertismente:26 trimiteri (27 înainte de alinierea090/091),
218 restanțe,3 cod,4 căi; baseline-ul metricilor nu a fost schimbat.
Nucleu/Scenarii/Integral/HTTP nu au fost rulate; nu există implementare CV.

După R6, simulatorul scrie compensarea ca Operare a corecției, câte o
tranzacție pe document. Asertează în plus că nicio postare nu are cauză
diferită de proprietar, că postările cu cauza B se închid la zero pe fiecare
cont după storno B și că fiecare corecție activă poartă integral delta ei.
Rulare 2026-10-13: 16 ordini și 4 cazuri temporale verzi;
`run-verificari/20261013-005151-cvl-r6/cvl-matrice.json`. Tot model
aritmetic: ține corecțiile active într-o mulțime, nu reconstituie familia
din fapte, și nu rulează mutanții din SC-CVL-26.

După R7, simulatorul are și storno recepției, cu cinci cazuri: refuz pe
stoc insuficient cât B e activ, `CV_DEPENDENTI` cu A activă după compensare,
două ordini acceptate cu final 0/0 și numai inversele recepției, și
`CV_CRONOLOGIE`. Rulare 2026-10-13: 16 ordini, 4 cazuri temporale și 5 de
recepție verzi; `run-verificari/20261013-010142-cvl-r7/cvl-matrice.json`.
Lotul intact nu este modelat: fixture-ul are mereu un BCS.
