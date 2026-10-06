## Verdictul

**Recomand forma 5 în sensul strict: forma 3 plus cheia de pereche deja aprobată (D9-A2), cu două coloane în plus față de cum au fost enunțate: `PartenerFiscal` în blocul fiscal și `FelRepartitor`.** Mai jos îi spun „F3+". Formele 1, 1b, 2 și 4 le resping, cu contraexemple din cod.

| Forma | Verdict | Contraexemplul |
|---|---|---|
| 1 (azi) | respinsă | 371 D poartă `Partener`=furnizor ca fapt fiscal (`DeclarantFacturaIntrare.cs:182` aplică `CuFapt` pe capătul intern al recepției; `DeclarantReturFurnizor.cs:57`), iar balanța analitică grupează pe `Partener` (`ContabilProiectii.cs:221`). După un consum, soldul 371 „pe furnizor" rămâne la valoarea recepției pentru totdeauna. Cazul A. |
| 1b (D9-A10) | respinsă ca formă finală, acceptabilă ca interimar | cheia snapshot-ului ține și `GestiuneId` (`SolduriService.cs:71-74`): 401 C din recepție are (F, vFurnizor), 401 D din plată are (F, –). Două rânduri care nu se închid. Cazul B. |
| 2 | respinsă | owner-ul a respins-o explicit (amendamentul 2, „Respinsă"); tehnic, 4426 ar purta furnizorul ca sold, iar BTR nu are ce partener să poarte. |
| 3 | acceptată cu schimbări | fără `PartenerFiscal` separat, 4426 și 3xx ar purta furnizorul ca cheie de sold, adică defectul formei 1 sub alt nume. |
| 4 | respinsă | viramentul: `DeclarantTrezorerie.cs:161-163` cunoaște pe PLT doar banca proprie; cealaltă bancă stă pe documentul pereche (`Trezorerie.cs:218`). `Intrare` nu poate fi scrisă onest la declarare. Plus 4426, `fisa.Loc` la imobilizări (`DeclarantImobilizari.cs:39`), imputatul de pe linie la NIR-diferență (`DeclarantNir.Diferenta.cs:67`). |
| 5 | acceptată cu schimbări | perechea nu e cheie de sold și nu e sursa partenerului fiscal: la taxare inversă perechea 4426=4427 n-are terț (`Fiscal.cs:126-138`). |

## Sarcinile

**1. Faptele din runda 1, răspunse.**
(1) Postarea are `DocumentId` și `LinieId` (`M/Cub/Postare.cs:16-17`), fără componentă. O linie produce mai multe mișcări: net plus taxă (`DeclarantFacturaIntrare.cs:47-49`), două nete la capitalizat (`:189-192`), bucăți FIFO la NTC (`DeclarantNotaContabila.cs:41-48`), bucăți pe partide la PLT (`DeclarantTrezorerie.cs:118-125`). Deci 5 ≠ 3 și ordinalul din D9-A2 e necesar.
(2) Cititorii capătului celălalt: fișa contului îl reconstituie pe tranzacție și dă nul la mai multe conturi (`ContabilProiectii.cs:486-489`), SAF-T L la fel (`SaftProiectii.PeCub.cs:358-361`). Nimic nu e măsurat; D9-A1 abia urmează. Denormalizarea din forma 4 n-are o cifră în spate.
(3) Nu există cont cu două dimensiuni de sold simultane: trezoreria pune gestiunea doar pe piciorul propriu și partenerul doar pe cel de terț (`DeclarantTrezorerie.cs:120-121`, `:158-172`); cele 22.389 „ambele" din fișa §3 sunt fapt fiscal peste gestiune reală sau virtuală peste partidă. Custodia e declarată neacoperită (`CLAUDE.md:131`).
(4) Nu există istoric de migrat: bazele se recreează la pasul 7 (contract D9-D6). Apărarea principală a lui 1b cade.
(5) Stornoul e în roșu: latura se păstrează, valorile se negă (`N/Cub/Storno.cs:11-26`). Argumentul despre semn și latură în forma 4 rămâne.
(6) Virtualele nu sunt clasificator de raport: SAF-T S ia rolul terțului din `PoliticaMiscareSaft` (`ProfilPrivat.cs:893-918`), iar stocul filtrează pe unitate de lot (`Loturi.cs:22-23`). Cititorii virtualelor sunt doi: marcajul contraponderii (`Citiri/Transformare.cs:7-11`) și filtrul gardului NIR (`ReceptiiConexe.cs:216`).
(7) Politica de cont nu e versionată (`PlanConturi.cs:26-36`, D9-A8 nivelurile). Cu o singură coloană, cititorul nu are nevoie de politică ca să interpreteze cheia; doar gardul de scriere o citește. Condiția mea din runda 1 se restrânge la gard.
(8) Frecvența nu se vede în Flax (doar tipuri importate). Codul arată însă că la decont partenerul fiscal e angajatul (`ProfilPrivat.cs:519`, `DeclarantDecont.cs:62-66`), pe care D394 îl scoate la „neincluse" ca `RepartitorNePartener` (`D394Proiectii.cs:256-258`). Furnizorul de pe bon nu există în model, în nicio formă.

**2. Nucleul.** `Coordonate.cs:7-8` și `Capat.cs:5-6` au cele două coloane fără nicio regulă: artefact. Regulile:

| Regula | Verdict | În F3+ |
|---|---|---|
| cantitatea cere gestiune (`Conservare.cs:140-144`) | principiu: cantitatea cere un loc | cantitatea pe unitate de lot cere `Repartitor` |
| unitatea lipsă e tolerată pe gestiune virtuală (`:152`) | artefact | contracantitatea e `Cantitate ≠ 0 ∧ Unitate = null`; `Sold.Din` o ignoră deja prin spațiu (`Sold.cs:43`, `Postari.cs:5-6`) |
| lotul cere gestiune (`:169-170`) | principiu | lotul cere repartitor; felul e al contractului laturilor (`Laturi.cs:28`, `:35-39`). BCS pune lotul și pe 6xx cu locul de consum (`DeclarantBonConsum.cs:85-89`), deci azi regula admite orice repartitor intern, nu doar gestiune |
| partida cere partener egal cu al partidei (`:177-184`, `Unitate.cs:9-14`) | principiu: partida are titular | `Repartitor == Unitate.Titular` |
| contraponderea stă în Transformare și n-are partener (`:26-41`, `Transformare.cs:17-19,30`) | artefact al Guid-ului santinelă | contrapondere = cantitate fără unitate, valoare zero, același cont, produs și cauză cu o postare reală; marcaj explicit, nu Guid |

Ce ar trebui să recunoască nucleul în locul gestiunii virtuale: `FelRepartitor` pe capăt (enumul există deja, `Operand.cs:9-15`, dedus din `ClrType` în `Fapte.cs:400-403`). Cu el, plasa de siguranță de la `:152` devine „cantitate pe repartitor intern fără unitate = refuz", iar capătul din afara evidenței e orice capăt fără unitate. Fără un fel pe postare, nucleul nu poate deosebi un declarant care a uitat lotul de o contracantitate legitimă. De aceea recomand persistarea lui.

**3. Declaranții.** Contractele laturilor: `FacturaIntrare.cs:26`, `FacturaIesire.cs:21`, `DocumenteGestiune.cs:38,224,249`, `Decont.cs:21`, `Trezorerie.cs:561,587`, `Retururi.cs:37,126`, `NotaContabila.cs:24`, `ListaDiferenteInventar.cs:19`, `Asamblare.cs:45`, `Dvi.cs:19`, `Imobilizari.cs:16,380,572`.

| Declarant | Azi, pe picior | F4: ieșire→intrare / grupare | Capăt care nu e latură (N) sau latura nu deosebește (L) |
|---|---|---|---|
| FCT | 3xx: G=primitor, lot, P=furnizor fiscal (`:142-144,:182`); 401: G=vFurnizor, P+partidă (`:150,:62`); 4426: G=primitor, P fiscal (`Fiscal.cs:122`); 4427 inversă: P fiscal | F→G / 3xx G, 401 F, 4426 –, 6xx G | N: 4426, 4427 |
| FCL | 7xx: G=predator, P fiscal (`:97,:107`); 4111: P+partidă; 4427: G=predator | G→C / 7xx G, 4111 C | N: 4427 |
| NIR | 3xx: G, lot; 401/408: vFurnizor, P+partidă (`:49-56`) | F→G | – |
| NIR diferență | stoc: G, lot; diferență: vInventar, P=predator / imputat de pe linie / –, partidă dacă urmărește (`:67-87`) | ? / G, imputat | N: imputatul (linie), vInventar |
| BCS | 3xx: G=predator, lot; 6xx: G=loc consum, lot pe 6xx (`:76-89`) | G→CC / G, CC | – |
| DSC | 3xx: G, lot; 607: vClient necondiționat, P=primitor dacă extern (`:85-87`) | G→C / G, C | L: primitor intern primește tot vClient |
| LDI | 3xx: G, lot; 6xx/7xx: vConsum sau vInventar, fără comisie (`:83-86`) | Inventar→G, G→comisie / G, ? | N: vInventar; comisia se pierde |
| DEC | fiecare picior: linie, regulă, apoi predator (`:46-49`); P dacă extern, G dacă intern (`:85-87`); 4426 contrapartidă forțată pe predator (`:62-66`) | E→unitate / 6xx E (azi), 542 E | N: furnizorul bonului nu există |
| NTC | repartitor explicit pe linie sau implicitul laturii; P numai explicit extern, FIFO pe partide (`:54-84`) | per linie | L: ambele implicite interne; compensarea A/B nu e flux |
| BTR | ambele: G predator / G primitor, aceeași latură (`:73-86`) | G1→G2 / G1, G2 | – |
| RDC | cost: 607 vClient+P, 3xx G lot (`:53-66`); venit: 7xx G=primitor, P fiscal; 4111 partidă | C→G | N: 4427 |
| RLF | 3xx: G, lot, P fiscal (`:48-57`); 401: vFurnizor, partidă | G→F | N: 4426 |
| PLT/INC | propriu: G=contul propriu; terț: P+partidă doar dacă urmărește, altfel nimic (`:67-73,:120-121`); virament: aceeași bancă pe ambele picioare (`:161-163`) | B→F / B, F | N: a doua bancă a viramentului |
| ASM | real: G, lot; contrapondere vTransformare (`Transformare.cs:30`) | G→G | L |
| PIF/AMO/CAS | toate capetele G=`fisa.Loc` (`DeclarantImobilizari.cs:39`) | intern→intern | N: locul fișei |
| DVI | 4426: G=primitor, P=vama fiscal; ancoră fiscală pe cartea fiscală (`:55-60`) | vama→G | N: furnizorul de import |

**4. Cititorii.**

| Folosire | Clasa | F3/F5 | F4 |
|---|---|---|---|
| `ContabilProiectii.cs:190,221,322,469-477` | cheie de sold (ambele) | un filtru și un grup; dispare `repartitorNul` | grupare = același lucru; ieșire/intrare fără cititor |
| `SolduriService.cs:71-74,263-268,121-125` | cheie de sold / lot / partidă | sarcina 5 | idem |
| `Citiri/Partide.cs:155,185`, `Plati.cs:131,137`, `Transferuri.cs:42-48,84-85`, `IdentitatiPartide.cs:7-13`, `Randuri.cs:109,134`, `Explicatii.cs:89-92`, `Deschidere.cs:105` | identitate de partidă | doar redenumire; `Unitate.Partener` rămâne titular | idem |
| `Loturi.cs:22-23,39`, `Saft…Stocuri.cs:36,289,353` | identitate de lot | `GestiuneId` → `Repartitor` | idem |
| `Fiscale.cs:74,82`, `Invarianti.cs:117-124`, `D394Proiectii.cs:160`, `TvaProiectii.cs:98,123` | fapt fiscal | trec pe `PartenerFiscal`; se rup dacă rămân pe coloana de sold | idem |
| `Saft…PeCub.cs:185-188,292` | cheie de sold pe conturi `RolTert` | neschimbat semantic | idem |
| `Saft…PeCub.cs:358-361` | atribut de eveniment dedus din tranzacție | perechea îl dă exact (D9-r4) | ieșire/intrare îl dă, dar e fals la 4426 |
| `GardAnaliza.cs:52` | deja o singură coordonată prin coalesce | devine citirea coloanei | idem |
| `Citiri/Transformare.cs:8-9`, `ReceptiiConexe.cs:216` | marcaj de contrapondere / filtru | marcaj explicit | idem |
| `DeclarantDescarcareGestiune.cs:87`, `ReturClient.cs:65`, `Decont.cs:86` | atribut de eveniment scris în coloana de sold | decizie explicită: 607 pe client e cheie de sold sau nu | idem |

Ce devine mai simplu în 3 și 5: un singur grup în balanță și snapshot, gardul fără coalesce, nucleul fără `GestiuniVirtuale`, D394 fără join pe `Repartitor` pentru a afla felul (`D394Proiectii.cs:198-205`) dacă `FelRepartitor` e pe postare. Forma 4 nu simplifică nimic peste 3 și adaugă două coloane pe care niciun cititor de azi nu le cere.

**5. Snapshot-urile.** Azi cheia contabilă e contul plus nouă dimensiuni, opt nulabile, cu `UNION ALL + GROUP BY` și santinelă `Guid.Empty` (`SolduriService.cs:65-83,318-319`). În 1b cardinalitatea crește pe conturile cu flag `Repartitor` cu numărul partenerilor, dar rândurile (F, vFurnizor) și (F, –) rămân separate pe 401, iar 3xx rămâne spart pe furnizorul fiscal. În 3 și 5 cheia are opt dimensiuni, 3xx și 4426 nu se mai sparg pe partener, 401 se închide pe un rând; stocul schimbă `GestiuneId` în `Repartitor` cu aceeași cardinalitate, partidele îl redenumesc pe `PartenerId`. În 4 cheia e aceeași ca în 3 dacă ieșirea și intrarea stau în afara ei; dacă intră, 371 se înmulțește cu furnizorii fără niciun cititor. Perechea nu intră în cheie (amendamentul 1, D9-A2, „Ce nu e").

**6. Fiscalul.** Partenerul fiscal se scrie la declarare din latura aleasă de politica de TVA (`Fiscal.cs:68-72`) pe postarea de bază și pe cea de taxă, inclusiv pe autocolectarea 4427 (`:137-138`). Se poate muta în blocul fiscal: un scriitor (`CuFapt`) și doi cititori direcți (`Fiscale.cs:74,82`, `Invarianti.cs:117,120,124`); D394 și jurnalul citesc `FaptFiscal.PartenerId` și nu se schimbă. Nu se poate deduce din pereche: la taxare inversă ambele capete sunt proprii, la decont contrapartida e angajatul, la DVI predatorul e vama, la capitalizat piciorul de terț n-are partener. Nu trebuie dedus din antet la citire: `Fiscale.Versiune` include `PartenerId` în amprenta declarației (`:58-66`), iar antetul e editabil pe corecție. Stornoul copiază coordonatele și mută perioada D394 (`Storno.cs:13-22`); invariantul cere calificarea identică (`Invarianti.cs:127-132`); cu coloană separată nu se schimbă nimic. Corecția rescrie perioadele inversei (`CorectieService.cs:95-103`, `Materializare.cs:213-222`, dispare prin D9-A5) și lasă partenerul neatins; corecția de partener A→B iese ca −X pe A în inversă și +X pe B pe documentul nou, corect în orice formă cu partener persistat. Inversa tehnică cere același partener pe original și inversă, altfel D394 al perioadei originale arată A și B în loc de net.

**7. Cazuri proprii.** Notație ca în runda 1; „F3+" scrie `Rep`, `Fel`, `PF` (partener fiscal).

A. Recepție 10 × 100 + 21 %, apoi consum 4. Azi: 371 D 1000 +10 L1 G1 P=F; 401 C 1000 −10 vF P=F partidă; 4426 D 210 G1 P=F; 401 C 210 P=F partidă; 371 C 400 −4 L1 G1; 6xx D 400 +4 L1 CC. Sold contabil pe (371, F, G1) = 1000, pe (371, –, G1) = −400. Balanța analitică și soldul pe parteneri arată 371/F = 1000 după consum. F3+: 371 D Rep=G1 PF=F; 401 C Rep=F; 4426 D Rep=– PF=F; 371 C Rep=G1; 6xx D Rep=CC. 371 pe G1 = 600, nimic pe F. Tăcut azi: soldul pe repartitor al stocurilor.

B. Plată 1210 pe 401.02.00 bugetar, cont cu flag `Repartitor` fără partide (`plan-conturi.csv:1150`, lista din `ProfilBugetar.cs:258-260`). Azi: 401 D 1210 fără nimic (`DeclarantTrezorerie.cs:71-73`), 5121 C 1210 G=B; recepția lăsase 401 C 1210 pe vF. Două rânduri de snapshot, suma zero, soldul pe vF −1210 în orice filtru pe gestiune. 1b: (F, vF) și (F, –), tot două rânduri. F3+: Rep=F pe ambele, un rând.

C. Decont: angajat E, bon de la S, 100 + 21. Azi: 6xx D 100 P=E (implicitul predatorului), PF=E; 542 C 121 P=E partidă; 4426 D 21 PF=E. D394 îl scoate pe E la neincluse. F3+: identic, S lipsește tot. Forma 4 ar scrie „ieșire = E" și ar părea corectă. Tăcut în toate: furnizorul bonului cere un atribut pe linia decontului [judecată].

D. Virament 1000 din B1 în B2. Azi, pe PLT ambele picioare poartă B1 (`:161-163`), pe INC ambele B2; 581 pe B1 = +1000 și pe B2 = −1000 pentru totdeauna, deși 581 total e zero. F3+: dacă 581 declară „fără repartitor", Rep = – pe ambele și se închide. Forma 4 nu poate scrie B2 pe PLT.

E. Descărcare cu primitor intern (contractul admite `ExternaSauInterna`). Azi: 607 D G=vClient, P=– (`:85-87`). Balanța pe 607 arată gestiunea „Client" pentru un consum intern. F3+: Rep=primitorul intern, Fel=UnitateInterna.

F. Minus de inventar 2 buc, comisia K. Azi: 371 C −2 L1 G1; 6xx D +2 vConsum. BCS pune locul și lotul pe 6xx, LDI nu: aceeași natură de mișcare, două convenții. F3+: 6xx D Rep=K. Tăcut azi: un Guid fără rând de nomenclator în coloana `GestiuneId` a snapshot-ului.

**8. Declarația contului.** Trei câmpuri: `DimensiuniObligatorii.Repartitor` (`PlanConturi.cs:26`, `Enums.cs:127`), `RolTert` (`:33`, `Enums.cs:178-182`), `UrmarestePartide` (`:36`). Pe privat, dimensiunile sunt goale peste tot (`ProfilPrivat.cs:233-234`), rolul e pe prefixele 411/413/418/419 și 401/403/404/405/408/409, partidele pe rol plus 542 și 461 (`:336-364`). Pe bugetar rolul e nul, partidele sunt opt simboluri (`ProfilBugetar.cs:258-260`), flag-ul R e pe 20 de conturi (`plan-conturi.csv:327,336,354,355,594,1149,1150,1153,1160,1163,1196,1197,1206,1282,1287,1288,1294,1297,1299,1463`); patru dintre ele urmăresc partide, deci 16 nu, cum spune fișa. Se pot uni două din trei: flag-ul R și felul într-un `RepartitorCerut` cu valori multiple (Partener, Angajat, Gestiune, ContPropriu, UnitateInterna, Oricare). `UrmarestePartide` rămâne separat, fiindcă e proprietatea unității, nu a coordonatei (401.02.00 cere repartitor fără partide; 542 cere partide pe angajat). `RolTert` rămâne separat, fiindcă e clasificare SAF-T: 419 e client cu sold creditor, 461 n-are rol deși poartă terț. Conturile din seed pe care o unire strictă, cu un singur fel, le-ar strica: 461 și 462.01.09 (partener sau angajat), 552.00.00, 774, 803.00.01, 804.90.00, 805.00.00, 437.01.00, 437.02.00, 442.08 și 442.08.01, unde felul nu se deduce din simbol și ar cere `Oricare` sau hotărârea owner-ului.

**9. Pașii aprobați.** 2c duce spre 3 și 5: pune pe piciorul de terț exact valoarea pe care `Repartitor` ar purta-o, și spune explicit că nu reproduce convenția pozițională, adică respinge 2. Duce și spre 4 doar pe `Grupare`. Lasă însă `Gestiune = vFurnizor` lângă `Partener = F` pe același picior (`DeclarantFacturaIntrare.cs:150`), deci adâncește forma 1b. Textul „Partener = latura externă a documentului" e greșit pentru DEC și NTC, unde terțul e al liniei (`DeclarantDecont.cs:46-49`, `DeclarantNotaContabila.cs:54`); ar trebui „terțul capătului". 7b duce spre 5 și arată că ieșirea și intrarea din 4 sunt derivabile, deci argumentează contra lui 4. Implementarea lui 2c acum creează de desfăcut: rescrierea celor 17 aserții (`tr-d9-inventar.md:422`) de două ori, normalizarea oracolului o dată degeaba, și fiecare declarant atins încă o dată la schimbarea coloanei. Dacă 2c se face printr-un singur helper de capăt de terț și aserțiile se scriu pe proiecții, nu pe numele coloanei, restul e redenumire. Dar ordinea corectă e: forma întâi, 2c după, ca parte din ea.

**10. Locul în plan.**
(a) În TR-D9a, la pașii 7 și 7b: schema se scrie o singură dată în `InitialCreate`, bazele se recreează oricum, oracolul e mort după 6, deci nu mai cere normalizări. Riscul: contractul se definește ca „scoate cod și nu adaugă comportament" (`tr-d9-taierea-contract.md:30-31`) și își apără separarea „am scos" de „am schimbat" (`:38-41`); schimbarea ar fi a noua din lista închisă, cu catalogul rescris pe coordonate. Text schimbat: D9-D1 schimbarea 9; D9-D6 coloanele; D9-D15 pasul 7 cu forma și 2c absorbit; D9-A10 rescris; D9-D9 coloanele listei XAF; regula de oprire 12; D9-D13 proba pe `Repartitor`.
(b) În TR-D9b: vecinătate bună cu unitățile și cu titularul partidei; riscul e al doilea reset al unei scheme declarate canonice și o felie deja încărcată, iar 1b devine între timp convenția pe care se scriu cititorii noi. Text: contract TR-D9b cu un D al formei; 110 consemnează 1b ca interimar.
(c) După: datele se acumulează, apar condițiile de migrare din runda 1 §8, iar două coloane tipate devin canonice prin vechime. Nu recomand.
Recomand (a), cu 2c redus la helper și probele catalogului scrise o dată, pe forma nouă.

**11. Fișa de fapte.** Omite: BCS pune lotul și locul pe 6xx, LDI pune virtuala Consum, deci virtualele nu sunt o convenție; 3xx poartă partenerul fiscal, ceea ce sparge soldul pe repartitor al stocurilor; DSC pune vClient și la primitor intern; viramentul pune aceeași bancă pe ambele picioare; imobilizările iau locul din fișă; NIR-diferență ia imputatul de pe linie; D394 raportează angajatul la decont; amprenta declarației include partenerul. Înclină: §2 afirmă că partenerul vine numai cu partida, dar DSC, RDC, DEC, NTC și NIR-diferență îl scriu fără partidă; §4 numără linii cu `.Partener`, nu semantici, ceea ce face identitatea de partidă să pară cititorul dominant; §6 descrie convenția registrului care moare la pasul 6 ca și cum ar conta pentru forma cubului; §7 formulează deja locul în plan. Greșit nu am găsit nimic în cifre: familiile din §3 se adună, cele 16 din 20 se confirmă pe seed, ancorele de linie sunt corecte.

## Livrabilele

**Ce am schimbat față de runda 1 și de ce.** Partenerul fiscal se deduce azi din antet, dar la scriere și persistat; asta e acceptabil, iar defectul real e lipsa furnizorului pe linia de decont, nu forma. Viramentul e un contraexemplu mai tare pentru 4 decât credeam: a doua bancă nu e pe document. Virtualele nu se pot înlocui doar cu natura contului; nucleul are nevoie de un fel pe postare, pe care îl adaug. BCS nu pierde locul de consum, LDI da. Costul în octeți al formei 4 e irelevant; argumentul e că nu se poate completa. Marginea 3 contra 5 e tranșată de 7b: componenta nu există, ordinalul e necesar. Custodia nu e implementată, deci nu blochează. Riscul de migrare nu există, deci 1b pierde apărarea.

**Forma recomandată, coloanele pe `Postare`.** Dispar `Partener` și `Gestiune`. Intră `Repartitor Guid?`, `FelRepartitor byte?`, `PartenerFiscal Guid?` lângă `DocumentFiscalId`, și `Pereche int?` din D9-A2. Cheia soldului contabil e cont, repartitor, produs și cele șase analize; a stocului e lot, cont, produs, repartitor; a partidei e unitate, cont, titular. Regula de completare: declarantul pune pe fiecare capăt repartitorul al cărui sold îl mișcă postarea, cu felul lui din nomenclator: gestiunea sau unitatea internă pe piciorul intern, terțul pe piciorul de terț, contul propriu pe piciorul de trezorerie, nimic pe conturile care declară că nu poartă repartitor; partida își ia titularul din repartitor; partenerul fiscal se scrie separat, de politica de TVA, pe postările cu cod de TVA; contracantitatea e capătul fără unitate; contraponderea transformării e marcată, nu adresată printr-un Guid.

**Schimbări cerute, în ordine.**
1. Hotărârea formei înaintea pasului 2c; 2c rescris ca „terțul capătului pe piciorul de terț", printr-un singur helper.
2. Schema la pasul 7: cele trei coloane, snapshot-urile cu opt dimensiuni, `InitialCreate` o singură dată.
3. Nucleul: `Coordonate` și `Capat` cu repartitor și fel; `Conservare` pe regulile din sarcina 2; `GestiuniVirtuale` ștearsă.
4. `Fiscal.CuFapt` scrie `PartenerFiscal`; `Fiscale`, `Invarianti`, D394, jurnalul TVA pe ea; amprenta declarației neschimbată ca semantică.
5. Declaranții: DSC cu primitorul real, LDI cu comisia, 581 fără repartitor, DEC și NTC pe terțul liniei.
6. Contul: `RepartitorCerut` în locul flag-ului R; gardul strict pe postare.
7. Cititorii: un filtru de repartitor plus fel; SAF-T și stocul pe coloana nouă.
8. ModelCheck: cele 17 aserții scrise o singură dată, pe proiecții.

**De măsurat sau prototipat înainte, cu costul.** Recensământ pe Flax, numai citire, al perechilor de feluri pe cont, ca să confirme că niciun cont nu poartă două repartitoare reale: o zi. D394 regenerat pe Flax cu partenerul mutat în blocul fiscal, amprentă identică: jumătate de zi. Prototip de nucleu cu repartitor și fel, `Conservare` rescrisă, cele 180 de teste verzi: una până la două zile. Fișa contului cu contrapartida prin pereche contra subinterogării de azi, pe scena multiplicată din D9-A1: o zi. Lista conturilor cu flag R pe bugetar cu felul propus, confirmată de owner: jumătate de zi. În total patru până la cinci zile.

**Încrederea: 4 din 5** pe direcție și pe respingerea formei 4. O limitează faptul că n-am rulat nimic, n-am citit regula de seed a viramentului (o deduc din `DeclarantTrezorerie.cs:154-157`), n-am inventariat cititorii din client și din API, și că nevoile viitoare de custodie și ALOP rămân neprobate.
