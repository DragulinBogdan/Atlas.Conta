# 111(f) — Q3: proveniența sumelor; Q4: limitele primei felii

Tip: analiza
Data: 2026-10-12
Alimentează: 111 (f), viitoarea decizie și contractul feliei

Investigație cerută de owner după aprobarea Q1–Q2 din
[contractele conceptuale](2026-10-11-111f-contracte-operatii.md).
Baza citită: `96ccc5acbb4048d7ba41f60924863d631e83daca`, prin CodeGraph,
fără modificări de cod, rulări de produs sau acces la baze. Constatările
descriu sursele. Owner-ul a aprobat Q3-P1…P4 și Q4-P1…P4 la 2026-10-12,
cerând păstrarea ritmului dezvoltării pentru privat. Formulările de propunere
de mai jos păstrează conținutul acceptat; aprobarea este de proiectare,
fără implementare. „Prima felie” desemnează prima felie procedurală 111(f),
nu următoarea felie de dezvoltare a produsului.

## Q3 — constatările din cod

| Sursă citită | Constatare | Consecință pentru model |
|---|---|---|
| [DeclarantFacturaIntrare](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Declaratii/DeclarantFacturaIntrare.cs), `Declara`, `Netele` | Mișcările au cauza document + linie; netul și taxa pot produce mai multe mișcări. | Obligația unei poziții se poate sprijini pe mai multe postări. |
| [Partide din declarații](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Declaratii/Partide.cs), `Numeste`, `Proprie`, `Sursa` | Factura numește o partidă pe cont și partener, deschisă de document. Nominalizarea automată a sursei de trezorerie cere `Autogenerat` și `DocumentSursaId`. | Partida nu identifică poziția facturii sau alocarea procedurală. Relația la nivel de antet existentă nu satisface ținta nouă. |
| [DeclarantTrezorerie](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Declaratii/DeclarantTrezorerie.cs), `Declara` | Linia se poate împărți între partida sursei și partida proprie, cu păstrarea cauzei liniei; excedentul poate rămâne pe partida proprie. | O linie de plată nu înseamnă o singură postare și nici stingerea integral atribuită unei facturi. |
| [Postare](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Cub/Postare.cs) și [Randuri](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Cub/Randuri.cs), `Scrie` | Sunt persistate documentul, linia, suma, data, cartea, latura, unitatea, analiza, perechea și ordinalul. Nu există referințe la perechea bugetar–legal sau la alocarea ordonanțării. | Cauza oferă proveniență, dar nu înlocuiește defalcarea de execuție. |
| [Contabil](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Cub/Citiri/Contabil.cs), `Postari` | Cititorul include operarea/deschiderea și inversele lor prin `InversaDin`; exclude transferurile și inversele transferurilor. | Este reperul pentru filtrarea mișcărilor economice; nu este singur un cititor al plăților pe angajamente. |
| [Partide](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Cub/Citiri/Partide.cs), `Postari`, `Solduri` | Soldul partidei include transferurile și agregă debit minus credit. | Restul facturii este o măsură diferită de plata monetară; nu deducem orice plată ca total factură minus sold. |
| [Materializare](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Cub/Materializare.cs), `Imperecheaza`, `Anuleaza`, `Scrie` | Împerecherea poate scrie `Transfer`; anularea șterge postări și tranzacții `Operare`/`Transfer`; scrierea atribuie ID-uri și `ScrisLa`. | Transferul nu este o plată nouă. ID-ul postării nu supraviețuiește anulării și reoperării. |
| [Storno](../../nou/Atlas.Conta.Nucleu/Atlas.Conta.Nucleu/Cub/Storno.cs), `Inverseaza`, `Selecteaza` | Inversa păstrează latura, schimbă semnul și data; `Materializare.Storneaza` fixează referința `InversaDin`. `Atribuit` are deja semantica postărilor atribuite altor postări. | Citirea netă trebuie să păstreze semnul. Nu refolosim `Atribuit` ca identificator de angajament. |
| [MotorOperare](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Motor/MotorOperare.cs) și [TranzactieComanda](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Motor/TranzactieComanda.cs) | Există tranzacție și blocaj comun al scrierii. `LaOperare` precedă materializarea cubului; `Materializare.Opereaza` întoarce contractul, nu o mapare a postărilor persistate. Pregătirea cere declarant de cub. | Integrarea are nevoie de contract explicit pentru fapte procedurale și maparea suportului; un hook pus înaintea cubului nu rezolvă singur legăturile. |

`Postare.Data` este data efectului contabil în selecția propusă.
`Tranzactie.ScrisLa` este momentul scrierii. Câmpul `DataInregistrare` al
documentului este o dată contabilă, iar omonimul de pe postare aparține
blocului fiscal; niciunul nu se confundă cu momentul înregistrării.
`Document.DataOperare` este golit la anulare. Chiar dacă `ScrisLa` există,
rândurile șterse nu mai pot fi recuperate printr-un filtru asupra lui.

Catalogul [FCT](../nucleu/scenarii/FCT.md) și catalogul
[PLT](../nucleu/scenarii/PLT.md) conțin exemplele contabile și extensia
partidelor bugetare. Paragrafele lor vechi care spun că profilul bugetar
nu are partide sunt depășite de completarea explicită din aceleași fișiere.
Nu sunt folosite drept limită a modelului nou și nu sunt rescrise în acest pas.

## Q3-P1 — sursa canonică și selecția explicită a efectului

Propunere: declarația tipului identifică efectele contabile care reprezintă
obligația sau plata, iar atribuirea procedurală împarte acele efecte.
Cititorul nu deduce ulterior rolul din simbolul contului, dintr-un cod de
document sau din politica de contare de la data citirii.

| Măsură | Sursa propusă în fluxul obișnuit furnizor–factură–plată |
|---|---|
| Obligație constatată pe poziție | Suma semnată a efectelor contabile selectate de declarant pe capătul furnizorului, cu cauza poziției facturii; pentru FCT obișnuită, creditul furnizorului |
| Plată monetară atribuită obligației | Suma semnată a efectelor selectate pe capătul furnizorului al plății, care nominalizează partida obligației; pentru PLT obișnuită, debitul furnizorului |
| Execuție pe bugetar/legal/ordonanțare | Porțiunile aceleiași plăți canonice, repartizate explicit și reconciliate la scriere |
| Rest contabil pe partidă | Soldul cubului pe unitate, inclusiv transferuri, prin cititorul de partide |
| Plafoane, rezervări, autorizări și valabilități | Faptele procedurale tipizate, în afara cubului |

Conturile rezultă în continuare din politica de contare. Nu se însumează
ambele laturi ale mișcării și nici cartea fiscală peste cea contabilă.
Stornoul contribuie cu semnul său, prin proveniența spre efectul original;
nu se folosește valoarea absolută. Compensarea, avansul și rambursarea nu
devin implicit plăți ale acestui flux.

Prima felie propusă cere partide și nominalizarea obligației la operarea
plății procedurale. Nu acceptă un excedent pe partida proprie drept execuție
pe angajament. O plată de 40 cu atribuire procedurală 40 și numai 35
nominalizați contabil pe obligație este refuzată integral. Această cerință
este nouă pentru fluxul procedural; nu rescrie comportamentul tuturor
documentelor PLT existente. Alegerea mai multor facturi cere extinderea
contractului de nominalizare, nu multiplicarea legăturii de antet existente.

Un transfer ulterior poate schimba stingerea pe partide fără să miște bani.
Prin urmare, pentru o plată deja atribuită procedural, desfacerea sau mutarea
stingerii către altă obligație trebuie să treacă și prin gardul procedural.
În prima felie propunem refuzul reatribuirii cât timp atribuirea inițială
este activă; operația de corectare explicită rămâne în Q4.

## Q3-P2 — legături valorice în afara cubului

O singură coloană `AngajamentId` pe postare nu acoperă o postare de 70
împărțită în 50 și 20. Propunem legături tipizate valorice între alocări
și efectele canonice. Numele următoare descriu concepte, nu clase deja create:

| Element | Informații și legături |
|---|---|
| Alocarea obligației | ID stabil; document și poziție-sursă; poziții legală și bugetară de origine prin asocierea valorică |
| Suport contabil al alocării | Alocare; referință completă la postare (`Id`, `Spatiu`); suma fixă din acea postare atribuită alocării |
| Alocarea ordonanțării | Obligația, acoperirea anuală executabilă, suma și valabilitatea; proveniența ordonanțării vechi la reportare |
| Atribuirea execuției | Poziția plății; alocarea ordonanțării; referința la efectul contabil selectat al plății; suma fixă atribuită |

O alocare poate avea mai multe suporturi, iar un suport poate fi împărțit
între mai multe alocări. Suma fragmentului exprimă alegerea de repartizare,
pe care cubul nu o poate deduce; nu este un al doilea sold de plată.
Nu persistăm separat totaluri autoritative de facturat/plătit/executat.

Pentru fiecare efect canonic inclus în flux, suma fragmentelor sale trebuie
să fie exact suma acelui efect. Se verifică semnul, scara, cauza, partida,
titularul și compatibilitatea cu obligația. Perechile repetate se normalizează
sau se refuză înaintea scrierii, fără a dubla suma. Pentru o plată, cele două
defalcări se reconciliază și pe obligația-sursă, nu doar pe totalul antetului.
Rapoartele însumează fragmentele validate; totalul lor trebuie să fie egal
cu suma efectelor canonice distincte din cub.

Îmbinarea unei postări de 70 cu două alocări nu autorizează însumarea coloanei
`Postare.Valoare` de două ori. Agregăm separat suportul contabil și
fragmentele sau citim direct măsura fragmentelor pentru gruparea procedurală.

Clasificarea și perechea bugetar–legal sunt fixate pe identitățile și faptele
procedurale de origine. Nu le reconstruim după nomenclatoare editate ulterior.
Eventualele copii pentru citire sunt derivate și cer verificare de echivalență.
Această structură nu schimbă identitatea partidei sau direcția TR-D9b.
`Ordinal` identifică poziția în tranzacția materializată (117), nu rolul
economic al postării și nici identitatea stabilă a unei alocări.

## Q3-P3 — scriere, retragere și inversare

Propunem pregătirea unui singur rezultat de comandă: efectele contabile,
faptele procedurale și legăturile dintre ele. Validarea cuprinde toate
limitele și reconcilierile înaintea publicării. Materializarea atribuie
identități postărilor și rezolvă suporturile în aceeași tranzacție, sub
blocajul existent. Refuzul oricărei părți lasă zero efecte publicate.
Maparea exactă dintre efectul candidat și postarea persistată trebuie
adăugată contractului scrierii; nu se ghicește prin „prima postare a liniei”.

Identitatea poziției rămâne stabilă, dar fiecare operare acceptată are o
identitate proprie de execuție a comenzii. Retrimiterea aceleiași comenzi
nu produce efecte noi; reoperarea după retragere produce efecte și legături
noi. ID-urile postărilor vechi nu sunt reutilizate sau redirecționate.

La retragerea unei operații mixte, gardienii contabili și cei procedurali
se verifică împreună. Dacă este admisă, în aceeași tranzacție se păstrează
urma retragerii procedurale, se elimină legăturile operaționale active spre
cub și se aplică anularea contabilă existentă. Dovezile istorice de atribuire
păstrează identitatea operării, cauza, identificatorii sursei de atunci și
fragmentele atribuite, separat de legăturile active. Nu lăsăm un FK activ
spre o postare ștearsă și nu ștergem în cascadă istoricul procedural.
O referință arhivată se prezintă ca proveniență a unui efect retras, nu ca
postare disponibilă în cub. Schema exactă a arhivării va fi fixată în felie.

Există o integrare suplimentară concretă: `MotorOperare.AnuleazaOperarea`
refuză astăzi un document cu rânduri de împerechere. Pentru plata procedurală,
propunem ca anularea coordonată să includă explicit retragerea propriilor
atribuiri și desfacerea propriei stingeri, când există, într-un singur lot
validat. Se disting efectele proprii eliminate de lot de dependenții externi
care îl blochează. Desfacerea izolată a stingerii rămâne refuzată cât timp
atribuirea procedurală este activă. Această extensie nu este deja oferită
de ușa contabilă existentă și trebuie probată ca atare în viitoarea felie.

La storno, legătura inversă referă postarea inversă și atribuirea originală,
cu semnul și data noi. Orice inversare păstrează controalele de dependență și
perioadă. Expirarea anuală din Q2 nu este storno. O corecție monetară cu efect
în anul următor ridică o întrebare distinctă de execuție anuală și rămâne
în afara primei felii propuse (Q4-P4).

## Q3-P4 — limita explicită a istoricului

Recomandarea pentru prima felie:

- Citiri financiare/procedurale integrate la data efectului, pe baza
  faptelor valabile cunoscute acum, reconciliate cu cubul existent.
- Istoric procedural complet al aprobărilor, variațiilor, atribuirilor,
  retragerilor și inversărilor, cu data efectului și momentul înregistrării.
- Fără promisiunea reconstrucției financiare complete la un moment trecut
  al cunoașterii după anulări de cub. O astfel de cerere primește o limită
  explicită; nu se întoarce situația curentă etichetată drept cea de atunci.

Contraexemplu: plată 40 cu efect la 10 ianuarie, scrisă pe 11 ianuarie,
anulată pe 12 ianuarie. În cubul curent plata este zero. Doar filtrul
`ScrisLa <= 11 ianuarie` nu poate recupera cei 40 șterși. Dovada atribuirii
procedurale arată că aceștia au fost atribuiți și apoi retrași; nu certifică
singură întreaga situație contabilă de la 11 ianuarie.

Dacă owner-ul cere și această reconstrucție financiară în prima felie,
este necesar un contract suplimentar pentru conservarea efectelor contabile
retrase și a momentelor retragerii, plus reconcilierea explicită a
invariantului III. Investigația nu implementează și nu aprobă implicit
această extindere.

## Q3 — verificări numerice propuse

Acestea sunt așteptări noi de proiectare, independente, nu probe executate.
Identificatorii Q3-N1…N7 sunt locali analizei.

| ID | Caz | Așteptare |
|---|---|---|
| Q3-N1 | F/1 = 70; efect furnizor C70; alocări A = 50, B = 20; O autorizează A30 și B10; plată cu efect furnizor D40, fragmente A30 și B10 | Obligație 70, plată 40, rest 30; pe alocări rest A20/B10; contabilul de 70 nu devine 140 prin join |
| Q3-N2 | Același suport 70, fragmente 50 și 21 | Refuz 71 ≠ 70; zero efecte ale comenzii |
| Q3-N3 | Plată 40; suport nominalizat pe obligație 35, excedent propriu 5; execuție declarată 40 | Refuz de reconciliere; nu se persistă separat excedentul sau execuția |
| Q3-N4 | Martor exclusiv contabil: plată proprie 40, apoi transfer 40 către factura 70 | Plata monetară rămâne 40; transferul nu o duce la 80. Restul facturii devine 30. Martorul nu creează retroactiv execuție procedurală |
| Q3-N5 | S9: F70 pe B27/LM, O27 și plată 40; O28 și plată 30 pe același LM | Obligație 70, plată 70, rest 0; execuție B27 = 40, B28 = 30; suportul facturii și legalul rămân aceleași |
| Q3-N6 | Ramuri independente, aceeași perioadă și an deschise: F70, O40 și plată40; retragere admisă a plății, respectiv storno admis ulterior | Retragere: plata curentă 0, rest factură70, auditul atribuirii păstrat. Storno: înaintea datei inversei plată40/rest30, după ea plată0/rest70. Se aplică și gardienii contabili existenți |
| Q3-N7 | Reoperare după retragerea admisă din Q3-N6, aceeași sumă40; apoi retrimiterea aceleiași comenzi | O singură plată activă40, rest30; suporturi noi distincte de cele retrase; retrimiterea nu produce plată80 |

S3 continuă să probeze reconcilierea pe facturi distincte, nu doar pe total.
Pentru suport compus din mai multe postări, o probă suplimentară trebuie
să arate că fragmentele se conservă per postare, nu numai per document.

## Q4 — propunerea de delimitare

Q4 urmează Q3: limitele de mai jos restrâng prima implementare, fără a
declara rezolvate interpretări de domeniu care nu au fost discutate.

**Q4-P1 — variații fixe.** Păstrăm efectele aditive aprobate. Prima felie
primește suma inițială și variații semnate. „Noul plafon este 120” ca
înlocuire cu efect asupra rectificărilor ulterioare rămâne în afara ei.
O eventuală culegere comodă „adu la 120” poate calcula diferența numai
față de o bază verificată sub blocaj și poate persista un delta fix;
aceasta ar fi o convenție separată de culegere, nu plafon înlocuitor.

Martor: 100 inițial, +20 în februarie, apoi −10 cu efect în ianuarie.
Dacă toate limitele permit corecția, februarie devine 110. Pentru a păstra
120 ar trebui o nouă variație explicită +10; nu recalculăm vechiul +20.

**Q4-P2 — clasificare și atribuire fixate la operare.** Tripletul și
referințele sursă ale faptelor operate nu se editează. Corecțiile valorice
nu mută obligația între legale/bugetare sau facturi. Reatribuirea cu
consumatori, schimbarea clasificării și desfacerea stingerii contabile a
unei plăți procedural active cer operație proprie, amânată. Retragerea sau
inversarea deja agreată rămâne disponibilă numai sub gardienii ei; nu este
o cale de ocolire a dependenților.

**Q4-P3 — traseul anual confirmat.** Intră S10: propunere100, atribuire80,
semnare75, corecții explicite −5 pe legal și bugetar, rezervare75. Facturile
scenariilor primei felii vor fi ulterioare semnării. Eligibilitatea înaintea
semnării și semnarea peste valoarea atribuirii rămân de clarificat cu owner-ul.
Propunem să nu fie acceptate în fluxul procedural inițial; nu le etichetăm
drept interdicții legislative și nu inventăm acum o stare juridică nouă.
Contractul feliei trebuie să facă această limită verificabilă, nu doar
să omită scenariile din probe.

**Q4-P4 — monedă și operații speciale.** Prima felie propusă folosește RON,
fără avansuri, curs, rambursări, compensări, comisioane ori corecții monetare
între ani. Exemplele inițiale sunt fără TVA; nu se decide implicit baza
bugetară a TVA-ului. Cubul oferă suma datorată furnizorului, dar aceasta
nu decide singură ce plafon bugetar trebuie să consume fiecare componentă.
De exemplu, catalogul FCT distinge datorie121 la taxă obișnuită de datorie100
la taxare inversă: formula generală „net + orice taxă” nu este un contract.

Angajamentul global rămâne direcție aprobată, cu triplet și plafon proprii.
Documentul salarial și o eventuală lichidare cu fapt propriu nu intră în
prima implementare până nu au contracte de sursă și efect. Nu le înlocuim
cu o factură artificială.

**Excludere deja hotărâtă de owner:** contractele lunare în care luna a
douăsprezecea este plătită în ianuarie rămân neprinse în model. Nu primesc
aici un tratament, o probă numerică sau o interpretare prin analogie cu S9.

Limitele propuse privesc doar activarea noului flux procedural. Nu sunt
interdicții noi aplicate fluxurilor contabile existente. La intrarea în
flux, datele din afara domeniului acceptat trebuie refuzate explicit și
atomic, cu motiv de acoperire a produsului; nu se operează doar jumătatea
contabilă pretinzând execuție procedurală validată.

## Aprobarea și ordinea dezvoltării

Owner-ul a aprobat Q3-P1…P3 pentru legături valorice și scriere atomică; Q3-P4
cu istoric procedural și situație financiară la data efectului, fără
reconstrucție financiară completă după momentul cunoașterii; Q4-P1…P4 ca
limite ale primei felii procedurale. Q1–Q4 sunt închise la nivelul acestei
analize, pentru domeniul delimitat. Cazurile amânate rămân fără interpretare;
acceptarea limitei nu înseamnă că au fost modelate.

Prioritatea rămâne privat-first, cu TR-D9b drept următoarea felie de
implementare. Scenariile bugetare aprobate sunt repere de compatibilitate
pentru dezvoltarea viitoare. Implementarea planificării, deschiderilor,
angajamentelor, ordonanțărilor și reportării anuale nu devine condiție
pentru închiderea TR-D9b sau livrarea fluxurilor private.

Pentru a păstra această separare, recomandarea de execuție este:

- Închidem etapa de explorare 111(f) la documentele aprobate. Schema fizică,
  migrațiile, culegerea și rapoartele procedurale intră în contractul lor
  când această funcționalitate este programată efectiv.
- Într-o felie pentru privat intră o schimbare comună numai dacă are o
  cerință privată concretă, rezultat verificabil și poate fi livrată
  independent de fluxul bugetar. Posibila reutilizare ulterioară nu este
  singură motiv pentru extinderea feliei.
- Documentele private obișnuite nu cer angajamente, ordonanțări sau alocări
  procedurale fictive. Limitele Q4 privind RON, TVA și cazurile speciale
  privesc noul flux procedural, nu capabilitățile contabilității private.
- Integrarea ulterioară se face prin contractele tipizate aprobate, la
  operațiile care declară efecte procedurale; nucleul nu primește ramificații
  după profil pentru aceste reguli. Nu anticipăm acum coloane, tabele sau
  interogări de validare doar pentru viitoarea extensie.
- Regresia pe ambele profiluri și verificările cerute de felia curentă se
  păstrează. Scenariile procedurale neimplementate nu sunt adăugate ca
  cerințe de acceptare pentru o livrare privată fără această funcționalitate.

Următorul pas recomandat este reluarea contractului TR-D9b și a criteriilor
lui de închidere. Orice extindere motivată exclusiv de 111(f) se notează în
analiza de față și se tratează când se deschide felia procedurală.

Viitorul contract trebuie să fixeze schema suporturilor active și a
provenienței arhivate, maparea efectelor la scriere, gardul comun cu
împerecherea/stornoul, codurile refuzurilor, securitatea, probele prin
documente reale și măsurarea pe aceeași bază. Invarianții I și III trebuie
reconciliați explicit pentru faptele procedurale; această analiză nu îi
amendează. Nu sunt introduse clase XAF, migrații, API-uri sau alte tabele.
