# CITIRI — TR-D8

### Raport și snapshot de partide — specificație înaintea codului, 2026-09-25

SC-CIT-49: FCT 100 și PLT 40: raportul conține partida facturii,
contul furnizorului și rest datorie 60. După desfacere,
factura are rest 100 și plata are rest creanță 40. Cheia rândului include
unitatea, contul și partenerul; documentul este numai etichetă și navigare.

SC-CIT-50: deschidere creditor 60 + 40 pentru același furnizor, fără document:
două partide distincte, total 100; NTC 75 lasă numai a doua cu rest 25.
Snapshot-ul și raportul păstrează identitatea, inclusiv după reconstrucție.

SC-CIT-51: factura 100 stinsă integral în ianuarie, plata stornată în februarie:
raportul ianuarie rămâne fără rest, februarie arată factura cu rest 100.
Citirea directă, snapshot + fereastră și reconstrucția dau aceleași valori.
Starea curentă Stornat a unui document nu șterge faptele istorice.

SC-CIT-52: două partide pe același document și cont, pentru furnizori diferiți,
rămân separate. Modificarea unui snapshot cu +7 este raportată înainte de
rescriere; a doua reconstrucție are zero diferențe.

SC-CIT-53: identitatea documentului deschizător este verificată prin regula
092; plata care nominalizează partida facturii nu devine eticheta acesteia.
Traducerea SQL a identității este identică funcției nucleului pentru UUID-uri
distincte; deschiderea inițială nu se confundă cu identitatea unui document.
Cheia veche document + cont a ieșit (102c).

SC-CIT-54: HTTP înainte/după snapshot și reconstrucție: ascunderea postărilor
sau a valorii schimbă raportul conform drepturilor, fără acces la totalul
global materializat. Rândul fără document rămâne raportabil, fără buton de
stingere prin comanda veche care cere două documente.

2026-09-24. Specificație înaintea portării rapoartelor. Surse: 090(f/k),
091(g/i), 097 și `tr-d8-citiri-contract.md`. Așteptările sunt constante;
o citire directă din cub în ModelCheck este proba independentă, nu sursa
de producție.

| ID | Scenariu | Așteptare | Stare |
|---|---|---|---|
| SC-CIT-01 | FCT 1.200 → PIF contabil 1.200/fiscal 900 → storno PIF | intrarea contabilă întoarce zero postări pentru PIF și inversa sa; FCT păstrează D/C 1.200 | verificat pe intrarea comună |
| SC-CIT-02 | AMO contabil 100/fiscal 50 → storno în aceeași lună | două postări contabile originale de 100, două inverse de −100; nicio postare fiscală în citirea contabilă | verificat pe intrarea comună |
| SC-CIT-03 | DVI: bază fiscală 100 și TVA contabilă 21 | pe 4426 rulaj D 21/C 0; perechea fiscală 100/100 nu intră în jurnal/fișă/balanță | verificat pe intrarea comună |
| SC-CIT-04 | ASM mixt: Operare D/C 40 și Transfer pe același cont; storno mixt | jurnalul include numai postările economice de 40/−40; exclude Transfer și inversele lui prin origine, plus contraponderile structurale | verificat pe intrarea comună |
| SC-CIT-05 | BTR pe același cont → storno | zero rânduri contabile generale, mișcările pe lot rămân vizibile | verificat pe intrarea comună |
| SC-CIT-06 | Deschidere D activ 1.200/C amortizare 200/C capital 1.000 → PIF | trei postări inițiale fără document; nicio dublare; sold net activ 1.200, amortizare 200 | verificat pe intrarea comună |
| SC-CIT-07 | Storno BTR 4/40 | fiecare inversă poartă exact originea ei Transfer, scrisă de storno; invariantul de proveniență trece; citirea contabilă rămâne fără BTR (102c: backfill-ul istoric a ieșit) | verificat pe ambele profiluri |
| SC-CIT-08 | — | depășit de 102: completarea provenienței prin migrație a ieșit | — |
| SC-CIT-09 | Citire contabilă filtrată pe același BTR stornat, înainte și după separarea diagnosticului | A: diagnostic + citire = 2 comenzi SQL; B: compunere = 0, citire = 1; ambele întorc zero rânduri BTR; diagnosticul explicit rămâne o comandă | verificat pe ambele profiluri |
| SC-CIT-10 | Origine lipsă, ID inexistent, spațiu greșit sau altă postare Storno | invariantul de proveniență (`Cub.Citiri.Invarianti`) refuză cu număr 1; nicio inversă astfel coruptă nu devine rulaj contabil; pe proveniența validă trece; seed-ul nu mai citește proveniența (102) | verificat pe ambele profiluri |

SC-CIT-07/08 sunt probe de migrare: fixture operațional real, alterări SQL
controlate numai în tranzacția de probă, rollback obligatoriu. Nu sunt o
cale de culegere a documentelor. Review advers amânat de owner până la
sincronizare, după NIR.

Acesta este primul lot al intrărilor comune. Nu certifică portarea
rapoartelor, snapshot-urilor, fiscalului, securității sau SAF-T. Matricea
completă se extinde înaintea fiecărei portări, conform inventarului TR-D8.

Execuție: `run-verificari/20260924-100942-750/rezultat.json`, integral pe
ambele profiluri, exit 0. SC-CIT-03 este privat (DVI); celelalte sunt pe
ambele profiluri. Probele apelează `Cub.Citiri.Contabil.Postari`; jurnalul,
fișa și balanța API nu au fost încă portate. Activarea generală a cititorului
cere mai întâi diagnosticul stornourilor istorice fără origine.

Corecturi TR-D8/DEC: selectiv `DEC,BTR,ASM`, 327/389 OK, manifest
`run-verificari/20260924-160814-726/rezultat.json`; integral 2.723 bugetar /
3.814 privat OK, zero FAIL, build fără avertismente, exit 0,
`run-verificari/20260924-160912-899/rezultat.json`.
Migrația `OriginiStornoUnivoce` completează proveniența univocă;
diagnosticul explicit refuză restul cu numărul postărilor. Verificarea
provenienței este separată de citirea raportului. Seed-ul doar avertizează;
refuzul activării se leagă în felia primului raport portat. Performanța
pe istoric mare și portarea consumatorilor rămân în TR-D8.

D8-B3, prima execuție (cu refuz la seed, corectat ulterior): selectiv `BTR,ASM,DVI,IMO`, **519 bugetar /
719 privat OK**, zero FAIL, exit 0; build fără avertismente,
`run-verificari/20260924-234234-729/rezultat.json`. SC-CIT-09 numără
comenzile, nu estimează latența pe volum. SC-CIT-10 alterează numai în
tranzacție anulată la final și probează refuzul prin `ContaSeeder.Seed`.

Integral înaintea corecturii din 2026-09-25: **2.849 bugetar / 3.930 privat OK**, zero FAIL,
exit 0, build 0 avertismente; sursele C# nemodificate pe durata rulării.
`run-verificari/20260924-234503-103/rezultat.json`.

Corectură 2026-09-25: seed-ul avertizează în `RaportSeed` și în consolă,
fără refuz pentru proveniență. SC-CIT-10 execută seed-ul complet pentru
cele patru defecte și pentru istoric valid, verifică numărul 1/0 și
prezența/absența avertismentului, apoi păstrarea provenienței. Diagnosticul
explicit refuză în continuare defectele. Selectiv BTR: **118/118 OK**,
zero FAIL, exit 0, build fără avertismente,
`run-verificari/20260925-004612-520/rezultat.json`.

Integral după corectură: **2.864 bugetar / 3.945 privat OK**, zero FAIL,
exit 0, build fără avertismente; sursele C# nemodificate pe durata rulării.
`run-verificari/20260925-004756-015/rezultat.json`.

## Portarea contabilă — matrice înaintea codului (2026-09-25)

| ID | Scenariu | Așteptare |
|---|---|---|
| SC-CIT-11 | FCT stoc 10 × 10, BTR 4 × 10 și inversa lui | balanța contului de stoc D 100/C 0; soldul furnizorului C 100; filtrul gestiunii sursă D 100, fără rulajul BTR; partenerul și gestiunea rămân coordonate distincte |
| SC-CIT-12 | Aceeași recepție, închidere ianuarie, consum în februarie de 2 × 10 | snapshot ianuarie: D stoc 100, C furnizor 100; în februarie inițial stoc 100, rulaj C 20, final 80; rezultatele sunt identice cu cumulul fără snapshot |
| SC-CIT-13 | Reconstrucția snapshot-ului de mai sus, apoi repetare | aceleași coordonate și valori 100/100; repetarea raportează zero diferențe; etichetele nu intră în cheia sumelor |
| SC-CIT-14 | Snapshot contabil alterat cu +7, apoi reconstrucție | reconstrucția raportează diferența înainte de rescriere și citirea dă din nou finalul 80; un snapshot absent/vid citește integral cubul |
| SC-CIT-15 | DVI și ASM din SC-CIT-03/04 prin rapoartele contabile | DVI: D 4426 = 21, nu 121; ASM: D/C economic 40 și inversa −40, fără Transfer sau contraponderi |

RepartitorId din proiecția contabilă reprezintă Partener; gestiunea are
filtru și coordonată distincte, inclusiv în snapshot. Nu se reconstruiește
vechea dimensiune prin coalesce între partener și gestiune. MaterialId
corespunde Produs, iar UnitateId analizei organizaționale, nu lotului.

| ID | Scenariu | Așteptare |
|---|---|---|
| SC-CIT-16 | Fișa contului după închidere 100 și consum 20; filtru pe produs și gestiune | o postare C 20, sold curent 80; contrapartida cheltuielii este afișată; sursa este aceeași cu balanța |
| SC-CIT-17 | Jurnalul FCT 100 și BCS 20, cu BTR/inversă intercalate | câte o linie per postare, total debit 120 și credit 120; identitate postare × spațiu și tranzacție; BTR nu intră în rulaje |

### Fișă și jurnal: înlocuirea fixture-urilor din registre

Probele vechi `VerificaFisaJurnal` scriau direct perechi în RegistruContabil,
cu storno pozitiv artificial și identitate comună celor două laturi. Ele
se înlocuiesc prin documente NTC reale; nu se păstrează perechea ca formă a
jurnalului. Soft-delete-ul unei note de registru devine anularea comenzii.

| ID | Scenariu | Așteptare |
|---|---|---|
| SC-CIT-18 | Sold anterior 100, D 60, C 25, D/C pe același cont 40, C 10 și storno real −10 | fișă: 160 → 135 → 175 sau 95 → 135 → 125 → 135; la egalitate de dată ordinea totală a identităților decide numai soldul intermediar; 6 postări distincte |
| SC-CIT-19 | Aceeași fișă prin DataSourceLoader, pagini de câte 2 și filtru de grilă | paginile reproduc secvența integrală; filtrul nu recalculează soldul curent; ziua D/C 40 are net final 135 |
| SC-CIT-20 | Jurnalul din februarie pentru documentele de mai sus | 10 postări, debit 125 și credit 125; fiecare postare are tranzacție și identitate; fără filtru de dată intră și soldul anterior (încă 2 postări) |
| SC-CIT-21 | Tranzacție cu D serviciu 20 + D furnizor 30 = C stoc 50 | fișa stocului arată ambele conturi corespondente; nu inventează o contrapartidă unică |
| SC-CIT-22 | NTC D/C 15 operată și apoi anulată | cele 2 postări dispar împreună din fișă și jurnal; celelalte documente rămân; numărul și tipul NTC sunt atributele documentului real |

| ID | Scenariu | Așteptare |
|---|---|---|
| SC-CIT-23 | Invarianții pe baza completă și apoi eliminarea controlată a postărilor unei note, în tranzacție anulată | baza completă trece; nota fără postări refuză cu CITIRE_ISTORIC_INCOMPLET (acoperirea registru ↔ cub cât durează regimul dual) |
| SC-CIT-24 | — | depășit de 102: hosturile nu verifică la pornire, snapshot-ul se scrie numai din cub |

Snapshot-urile contabile se folosesc în ObjectSpace-urile de sistem.
Citirea securizată cumulează postările autorizate: snapshot-ul global nu
păstrează identitățile necesare permisiunilor pe rând și pe membru.
Egalitatea numărului de rânduri vizibile nu dovedește accesul la valorile lor.
Fișa compune SQL-ul generat din interogările securizate, inclusiv etichetele.
Această verificare de securitate nu înlocuiește diagnosticul de proveniență
din invarianți.

### Balanță și plan — fixture de documente reale

| ID | Scenariu | Așteptare |
|---|---|---|
| SC-CIT-26 | NTC: inițial D 100/C 300 pe contul A; februarie D 50/C 90; postare după perioadă 999 | inițial C 200, final C 240; la partenerii diferiți D 100 și C 340; contul cu numai inițial 70 rămâne; contul cu D/C 50 rămâne cu sold zero |
| SC-CIT-27 | Același cont: inițial fără partener 200, rulaj fără partener 30, cu partener 11 | fișa fără partener 230, cu partener 11, sintetic 241; paginile de 2 reproduc întreaga balanță în ambele moduri; filtrele se aplică înaintea agregării |
| SC-CIT-28 | Arbore: copil D 12, nepot D 4, alt copil C 30, mișcare proprie grupă D 1 | grupa D 17/C 30, net C 13; copil intermediar D 16; rădăcinile reproduc balanța plată; limitarea adâncimii nu schimbă sumele; un ciclu nu blochează citirea |
| SC-CIT-29 | Eticheta unui cont cu debit 17 ascunsă controlat, cu un copil debitor 5 | valoarea 17 rămâne în balanță și fișă cu etichetă goală; copilul devine rădăcină; tranzacția probei restaurează nomenclatorul |
| SC-CIT-25 | HTTP: NTC D 628 125 = C 401 100 + C 462 25, conturi temporare de test cu permisiuni | Admin/Cititor: 4 postări și fișă final 125; fără acces: liste goale; rândul 25 ascuns: 100; membrul Valoare ascuns: 0; clientul afișează ambele conturi corespondente |

### Extinderea citirilor (2026-09-25)

| ID | Scenariu | Așteptare |
|---|---|---|
| SC-CIT-30 | Balanță și fișă de la 0001-01-01, cu nota de 100 și rulajele din SC-CIT-18 | niciun sold anterior; final 135, aceeași sumă ca pe intervalul scenei; fără depășirea domeniului DateOnly |
| SC-CIT-31 | FCT 10 × 10, BTR 4 × 10 către altă gestiune, apoi storno BTR | cititorul de lot: sursa 6/60, destinația 4/40; după storno sursa 10/100, destinația zero; balanța rămâne D 100 |
| SC-CIT-32 | BCS 2 × 10 după recepția de mai sus | contul de stoc în magazie 8/80; cheltuiala la locul de consum 2/20; cheile nu se confundă |
| SC-CIT-33 | FCT stoc 100, NIR conex încă draft, numai privat | Suppliers: inițial 100 din ianuarie, final 200 după FCT nouă de 100; egalitate cu GLA; nu așteaptă registrul conexului |

PAR-V21 și DVI-V15 sunt aliniate cu coordonatele D8-B1: filtrul Partener
nu reconstruiește vechea dimensiune Repartitor. Pe 446 fără urmărire,
fișa nefiltrată are C 210/D 210 și sold zero; cea filtrată pe biroul vamal
este goală. Acoperirea bugetară a partidelor așteaptă alegerea D8-B6.

SC-CIT-34: eliminarea ambelor postări ale unei singure linii NTC lasă
tranzacția echilibrată, dar invarianții o refuză prin acoperirea pe linie/latură.
Alterarea debitului cu 1 păstrează prezența liniilor, dar refuză conservarea
pe tranzacție/carte. Ambele alterări sunt în tranzacții cu rollback.

### Evaluarea operațională a loturilor — în lucru

| ID | Scenariu | Așteptare |
|---|---|---|
| SC-CIT-35 | Recepție 3/10, trei consumuri succesive de câte 1 | cub: ieșiri 3,33 + 3,34 + 3,33; lot final 0/0; evaluarea nu reia soldul registrului vechi |
| SC-CIT-36 | Deschidere numai în cub, lot 4/40, consum 2 și storno | consum 20; disponibil 2/20, apoi 4/40; fără recepție artificială în registru |
| SC-CIT-37 | Același lot mutat între gestiuni; consum și inversări retroactive | soldurile sunt verificate pe lot/cont/produs/gestiune, în fiecare zi; retragerea intrării cu consum dependent refuză atomic |

În regimul dual, ASM păstrează separat soldul registrului pentru R și
soldul cubului pentru C. Absorbția Δ rămâne conform ASM-B6. Probele vechi
care consemnau reziduul produs de citirea registrului se înlocuiesc pentru
operații noi cu așteptarea 0/0; diagnosticul istoricului divergent rămâne.

### FIFO și raportul de stoc — înaintea implementării

| ID | Scenariu | Așteptare |
|---|---|---|
| SC-CIT-38 | Privat: două loturi de deschidere, 2/20 și 3/60; FCL 4, fără pin | DSC draft alocă 2 din primul și 2 din al doilea, chiar fără RegistruStoc; la operare cost 60, stoc rămas 1/20; ordinea este data deschiderii și ID |
| SC-CIT-39 | Aceeași deschidere, pin pe lotul al doilea și linie fără pin, la data înregistrării | pinul se alocă primul; FIFO scade alocarea deja făcută; nici o alocare din alt cont sau altă gestiune; data fizică mai veche nu ascunde intrarea contabilă disponibilă |
| SC-CIT-40 | Raport stoc după SC-CIT-31/32, etichete ascunse controlat și schimbarea politicii TipStoc | sumele și cheia lot/cont/produs/gestiune rămân; etichetele pot fi goale; costul unitar este Valoare/Cantitate; zero cantitativ nu provoacă împărțire la zero |

SC-CIT-39 include două mutații izolate, cu rollback: primul lot este mutat
pe alt cont, apoi pe altă gestiune. În fiecare caz, FIFO alocă numai cele
3 unități din al doilea lot; cele 2 unități de pe coordonata străină nu
devin disponibil pentru factură.

SC-CIT-25 se repetă prin HTTP după închiderea lui ianuarie și după
reconstrucție. În februarie, inițialul debitor este 125 pentru Admin/Cititor,
100 când postările de 25 sunt ascunse și 0 când membrul Valoare este ascuns.
Existența snapshot-ului global de 125 nu poate ocoli permisiunile.
Proba durabilă: `nou/tools/ProbeHttp/citiri-cub.py`, exclusiv pe baza
privată izolată `.CodexBCS`; rezultatul se consemnează după rulare.

### Validare contabil + stoc (2026-09-25)

Integral ambele profiluri: **2.899 bugetar / 3.989 privat OK**, zero FAIL,
exit 0, build fără avertismente; `run-verificari/20260925-031339-966/rezultat.json`.
SC-CIT-25 HTTP a trecut înainte/după închidere și după reconstrucție, în două
execuții succesive; `run-verificari/trd8-http-final.log`.
Raportul de stoc a fost verificat prin HTTP și browser pe FCT → NIR:
cont 371, 3 bucăți/10 lei, cost unitar 3,333333. Client build și regenerarea
repetată a celor trei artefacte fără drift sunt verzi.
Aceste probe nu închid suprafețele încă neportate ale TR-D8.

### Partide comerciale bugetare — decizia 100, înaintea probelor

SC-CIT-41: seed-ul activează exact 401.01.00, 404.01.00 și 411.01.01
DinSeed fără rol SAF-T. Un cont trecut manual la UrmarestePartide=false
rămâne astfel la re-seed; după rollback seed-ul este idempotent.
SC-CIT-42: FCT 100, PLT 40 → partida facturii −60, plata 0;
storno stingerii redeschide factura −100 și plata +40. Același ciclu FCL/INC
are semne opuse. Cele două profiluri au aceeași nominalizare.
SC-CIT-43: eliminarea unității și a partenerului de pe creditul FCT 100,
refuză invarianții cu CITIRE_PARTIDE_INCOMPLETE. Nu se reconstruiește din sold. O notă pe 401
fără partener explicit rămâne validă conform T-D3; baza fiscală DVI nu
este partidă. Mutanții se probează în tranzacții cu rollback.
SC-CIT-44: deschidere cu 401 creditor 100 detaliat 60+40 pentru același
furnizor; NTC debit 401 de 75 consumă 60+15, rămâne −25; storno NTC
redeschide −60/−40. Ambelor profiluri li se aplică aceeași cheie.
SC-CIT-45: FCT de imobilizare 100 → credit 404 cu partidă −100,
RolTert bugetar rămâne Niciunul; inversarea readuce restul zero.

SC-NTC-07…11, 13, 15…17, 20 și 22 devin probe pe ambele profiluri,
cu 401.01.00/628.00.00 în bugetar. SC-NTC-12 (419) rămâne privat.

SC-CIT-46: FCT 100 → PLT automată 100 nominalizată pe factura sursă.
DELETE împerechere în perioada deschisă lasă FCT −100 și PLT +100 pe
propria partidă, prin Transfer; postările Operare rămân identice.
PLT manuală 40 poate stinge factura, iar ștergerea ei reface −100/+40.
Storno PLT automată după desfacere aduce partida proprie la zero și
păstrează datoria facturii −100. Transferul de desfacere este atribuit
nominalizării inițiale, astfel încât inversarea ei să-i compenseze efectul.
Aceeași PLT automată, după desfacere, este reîmperecheată manual cu 40
pe factura inițială și apoi desfăcută din nou. FCT revine la −100,
PLT la +100; stornarea plății aduce numai partida proprie la zero.
Transferul manual și inversa lui se compensează fără a fi inverse ale
nominalizării automate originale.
SC-CIT-47: desfacerea nominalizării automate în februarie păstrează soldul
zero al facturii în ianuarie și o redeschide la −100 în februarie;
storno ulterior al plății închide partida ei proprie fără a dubla efectul.
Storno direct în februarie cu legătura automată încă activă inversează
nominalizarea o singură dată și redeschide factura la −100.

SC-CIT-48: ștergerea CRUD directă a unei împerecheri care atinge cubul
se refuză; comanda de ștergere rămâne disponibilă cu drept Delete și
execută atomic desfacerea pe cub și ștergerea legăturii în perioada deschisă.

Validare după 100: **3.036 bugetar / 4.026 privat OK**, zero FAIL, exit 0,
build fără avertismente; `run-verificari/20260925-073729-162/rezultat.json`.
SC-CIT-46/48 trec prin HTTP (404/403/204), apoi separat prin acțiunea XAF,
cu verificarea FCT −100 / PLT +100 în cub și PLT zero după anulare.
`nou/tools/ProbeHttp/partide-cub.py`, loguri `trd8-partide-http` și
`trd8-partide-xaf`; după probă nu rămân documente sau parteneri activi.
Metadata/OpenAPI/types și build-ul clientului sunt verzi, regenerarea
repetată păstrează cele trei hash-uri. Raportul/snapshot-ul partidelor și
restul regulii de oprire TR-D8 rămân deschise.

### Împerecherea cu efect obligatoriu — decizia 101, înaintea codului

SC-CIT-55: PLT 70 către partener, NTC 581 = 531 cu repartitor cont propriu:
cererea de împerechere 50 se refuză atomic; nicio legătură sau postare nouă,
restul plății rămâne 70. Plata nu apare între candidații acestei note.

SC-CIT-56: INC 100 pe contul de client, NTC debit furnizor 60 pentru același
partener: contul diferit refuză împerecherea; restul încasării rămâne 100.
Nota corectă, cu debit pe contul clientului, permite stingerea pe acel cont.

SC-CIT-57: FCT 100, NTC cu nominalizare 75 deja scrisă: legătura manuală
75 este permisă, restul rămâne 25, fără Transfer suplimentar. O a doua
legătură pentru aceeași nominalizare este refuzată. Ștergerea acestei legături
manuale nu inversează operarea NTC; storno NTC redeschide FCT la 100.

SC-CIT-58: disponibilul plății 100, factura are două partide pe conturi
separate (40 + 60), numai contul de 40 este comun: cererea 100 se refuză;
cererea 40 mută exact 40. Dacă sunt mai multe ținte eligibile și comanda
pe document nu le distinge, refuz explicit de ambiguitate, fără alegerea
postării cu valoare maximă.

SC-CIT-59: refuzul repetat al unei comenzi nu lasă obiecte noi care să poată fi comise
accidental prin salvarea ulterioară a aceluiași ObjectSpace.

SC-CIT-60: FCT 100, NTC 75 nominalizată: asociere 50 apoi asociere 25,
fără transferuri. Ștergerea primei păstrează restul 25. O plată distinctă
de 25, împerecheată prin două transferuri de 10 și 15, lasă rest zero;
ștergerea celeilalte asocieri nu desface aceste transferuri. Desfacerea
ambelor transferuri reface rest 25. Pe aceeași pereche, fiecare desfacere
inversează exact propriul transfer, inclusiv când se șterge primul.

SC-CIT-61: stingerea retroactivă nu poate consuma un sold redevenit disponibil
numai după desfacerea ulterioară: la fiecare dată deja scrisă, restul ambelor
partide rămâne în sensul său. Refuz înaintea materializării.

Completare SC-CIT-43: contul devine manual, fără urmărire, iar factura
nu are unități: invarianții refuză `CITIRE_PARTIDE_POLITICA`, distinct
de cazul cu urmărire activă și unități lipsă. SC-CIT-58 probează ambiguitatea
în funcția pură; limita 40 din factura 40 + 60 trece prin documente reale.
NTC bugetar nu culege toate analizele cerute de 404, deci nu este folosit
ca fixture artificial pentru această ramură.

SC-CIT-62: FCT cu stoc 50 + TVA capitalizată 9,5 + serviciu 100,
NIR încă Draft: PLT automată 159,5 nominalizează întreaga datorie din cub;
restul sursei și al plății devin zero. Aceeași regulă pentru factura numai
cu stoc 100, indiferent de operarea întârziată a NIR-ului. Probele integrale
FCT/împerechere automată și F27-R3c exercită lanțul real pe ambele profiluri.

Completare SC-CIT-61/62: sursă 159,5 cu stingere manuală ulterioară 60,
plată automată retroactivă 30, apoi 100: a doua nominalizează numai 69,5,
restul de 30,5 rămâne pe partida proprie. Disponibilul nominalizării este
minimul păstrat la fiecare dată deja scrisă, inclusiv după data operării.

SC-CIT-63: PLT 30 pe 401 și INC 30 pe 4111 pentru același partener
nu se sting direct între ele: ambele sensuri ale cererii 10 se refuză,
resturile rămân 30 și nu se scrie nicio legătură. Conturile diferite
cer nota de compensare, conform 101(b); F19-D16 probează această limită.

SC-CIT-64: RDC fără factură de nominalizat, venit stornat 100 + TVA 21,
cost revenit 30: totalul documentului rămâne −121, partida este datorie
121; raportul și lista de datorii o includ cu 121, fără costul de 30.
SC-CIT-65: gardul de acoperire acceptă factura/returul cu postări de cost
fără partide; verifică acoperirea totalului de decontare prin partidele
Operare. Totalul antetului este numai martor de diagnostic, niciodată sursă
a restului. O politică manuală fără partide rămâne refuzată (SC-CIT-43).
SC-CIT-62, precizare: soldul efectiv citit la data plății rămâne 121;
disponibilul pe cont este separat, 61 după rezervarea stingerii viitoare 60.

Completare SC-CIT-58: FCT imobilizare 500 pe 404 și TVA 105 pe 401,
PLT automată 605 pe 401: legătura confirmă numai nominalizarea 105;
rest FCT 500 pe 404 și rest PLT 500 pe propria partidă 401.

SC-CIT-66 (101-r1, D-2): FCT 100 la 5 ianuarie, PLT 100 împerecheată manual
la 15 ianuarie, NTC la 7 ianuarie cu D 401/furnizor 100. Nota nu
nominalizează factura (disponibilul ei temporal este 0, fiindcă e stinsă la
15), deschide partidă proprie 100; factura are −100 la 7 ianuarie și 0 de la
15, niciodată creanță. Același calcul temporal servește PLT automată,
transferul manual, stingerea partidei inițiale și dependenții (C-D5).

SC-CIT-67 (101-r1, D-3): FCT 100, PLT automată 100, legătura ștearsă prin
comandă: PLT are +100 proprie numai prin transferul desfacerii. O INC 100
de la același furnizor, pe 401, o stinge: panoul oferă 100 și comanda
acceptă, restul PLT ajunge la 0. Varianta NTC (C 401/furnizor 100 operată
înaintea desfacerii) are același rezultat. Ținta comenzii este orice unitate
proprie a stinsului, inclusiv cea deschisă prin Transfer.

### Validare finală partide — 2026-09-25

SC-CIT-49…65 și integralele sunt verzi: **3.107 bugetar / 4.103 privat OK**,
zero FAIL, exit 0. Dovezile și încercările intermediare sunt în
[review-ul propriu](../tr-d8-review-codex.md). Raportul/snapshot-ul trec
matricea HTTP pe rând/membru înainte și după închidere/reconstrucție;
împerecherea 40 din 100 lasă 60, cererea 61 se refuză atomic, iar ștergerea
și anularea sunt verificate independent în cub. Browserul confirmă raportul
cu parteneri distincți și disponibilul 60 din panou. 101-r1 este închisă;
restul TR-D8 și matricea generală de securitate rămân delimitate în review.
