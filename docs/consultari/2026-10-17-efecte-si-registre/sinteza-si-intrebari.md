# Documente cu efecte în cub și registre specializate

Tip: note
Data: 2026-10-17
Stare: propunere pentru consultare, fără decizie de implementare
Autor: Codex, sinteză a discuției cu owner-ul
Scop: evaluarea extensibilității și a costului de întreținere față de modelul actual al cubului unic

## Întrebarea consultării

Atlas.Conta reprezintă efectele documentelor printr-un cub comun de postări, din care se obțin registrele și rapoartele. Întrebarea owner-ului este dacă obligativitatea trecerii prin acest model general devine un punct de îngustare: o funcționalitate nouă poate necesita extinderea și reverificarea vocabularului comun, chiar când semantica ei este locală unui domeniu.

Alternativa discutată este ca tipul de document să declare contractele efectelor sale: mișcări în cub, fapte în registre de informații și, unde se justifică, mișcări în registre de acumulare. Efectele obligatorii ale unei operări se validează și se persistă atomic.

Ipoteza de lucru este că această separare poate accelera iterațiile pentru categorii noi de fapte. Câștigul nu este încă măsurat. Consultarea trebuie să poată concluziona și că modelul actual este mai avantajos pentru funcționalitățile urmărite.

## Modelul rezultat din discuție

Cele trei planuri sunt destinații posibile ale efectelor unui document, cu semantici diferite.

| Plan | Ce păstrează | Regula de citire |
|---|---|---|
| Cub de mișcări | Transferuri de cantitate, valoare și valoare în valută între coordonate, cu dimensiuni și proveniență | Sume, solduri și rulaje în domeniul de agregare potrivit |
| Registre de informații | Fapte datate: metodă, durată, preț, stare, clasificare | Valoarea valabilă la o dată și istoricul schimbărilor |
| Registre de acumulare | Mișcări tipate, cu dimensiuni, măsuri și reguli proprii unui domeniu | Intrări minus ieșiri sau rulaje, după contractul registrului |

Forma conceptuală este:

```text
Document tipat + fapte citite + politici
                  |
                  v
             Plan de operare
                  |
                  +-- 0..N mișcări în cub
                  +-- 0..T înregistrări de informații
                  +-- 0..R mișcări în registre de acumulare
                  |
                  v
       Validarea întregului pachet
                  |
                  v
      Persistare în aceeași tranzacție
```

O operație are o identitate comună și poate produce mai multe mișcări în cub. Nu se impune o mișcare în cub pentru fiecare document: schimbarea duratei de amortizare poate produce numai un fapt temporal.

La nivelul cubului, ieșirea și intrarea sunt capetele mișcării rezolvate. Nu trebuie presupus că toate capetele tuturor mișcărilor coincid literal cu predatorul și primitorul din antet: contarea, taxele și operațiile compuse pot produce mai multe capete economice.

Registrele de acumulare păstrează mișcările individuale și cauza lor. Totalurile pot fi calculate sau materializate pentru eficiență; păstrarea numai a totalurilor ar pierde explicația și suportul corecțiilor. O mișcare poate avea sens explicit și resurse pozitive; nu este obligatorie aceeași convenție de semn pentru toate registrele.

Registrele de informații nu însumează valorile succesive. O durată nouă înlocuiește, de la momentul stabilit de contract, durata aplicabilă anterior. Politica temporală și retragerea/corectarea evenimentelor trebuie definite explicit.

## Efect primar sau proiecție derivată

Termenul „proiecție” folosit în discuție acoperă două responsabilități care trebuie separate în implementare.

**Efectul primar** păstrează un fapt produs de operare, cu propriul contract de domeniu. **Proiecția derivată** reconstruiește o vedere din fapte deja persistate.

Pentru fiecare registru propus trebuie răspuns la întrebarea: informația sa se poate obține integral și fără ambiguitate din cub sau din alt registru autoritativ?

- Dacă da, registrul este o proiecție reconstructibilă. Regula de derivare îi aparține; documentele nu trebuie să calculeze independent aceleași valori încă o dată.
- Dacă nu, registrul păstrează un fapt distinct, iar documentul declară explicit efectul care îl alimentează.

Exemple conceptuale:

| Cerință | Încadrare de analizat |
|---|---|
| Sold pe gestiune și produs, când toate mișcările relevante există în cub | Proiecție a cubului |
| Rezervare comercială care reduce disponibilul, fără schimbarea stocului fizic sau a valorii contabile | Efect distinct într-un registru de rezervări |
| Schimbarea duratei de amortizare | Fapt temporal într-un registru de informații |
| Raport nou peste aceleași fapte fiscale | Proiecție; nu cere automat modificarea documentului |

Cubul este deja o structură de acumulare multidimensională. A treia categorie se justifică printr-o semantică distinctă sau printr-o nevoie demonstrată de citire, nu doar prin dorința de a crea încă un registru.

Atomicitatea privește toate efectele primare obligatorii și orice proiecție sincronă necesară deciziilor operaționale. O proiecție exclusiv de raportare poate fi actualizată ulterior dacă produsul acceptă întârzierea; aceasta este o opțiune de analiză, nu o cerință a propunerii.

## Ce declară documentul și ce rămâne comun

Tipul de document este proprietarul compoziției efectelor sale. Nu devine o clasă care implementează singură toate mecanismele registrelor.

| Responsabilitate | Contract |
|---|---|
| Document | Date necesare, validări specifice, efecte posibile, condiții de producere și dependențe între efecte |
| Registru sau modul de domeniu | Structură tipată, chei, validări, citire la dată și reguli de corecție |
| Coordonatorul operării | Tranzacție, proveniență, idempotență, verificarea ipotezelor și coordonarea concurenței/perioadelor |
| Proiecție | Derivarea unei citiri din faptele autoritative și mecanismul de reconstrucție |

Calculul TVA, evaluarea FIFO și distribuirea rotunjirilor trebuie reutilizate. De exemplu, costul unei ieșiri se decide o singură dată și alimentează efectele care depind de el; două registre nu îl recalculează independent.

„Toate contractele documentului” înseamnă toate efectele sale primare cunoscute și condițiile lor. Nu înseamnă că factura trebuie să cunoască fiecare raport care va fi adăugat ulterior.

Structura rămâne cod tipat. Politicile aleg parametrii mecanismelor existente; propunerea nu introduce scheme EAV, expresii arbitrare în tabele sau un interpretor de comportamente.

## Dovezi din Atlas și configurația Flax

### Atlas are deja declaranți pe tip

[IDeclarant](../../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Declaratii/IDeclarant.cs) definește o funcție pură pe operand închis. Punctul de extensie există deja; schimbarea discutată privește vocabularul rezultatului și destinațiile materializării.

[Coordonate](../../../nou/Atlas.Conta.Nucleu/Atlas.Conta.Nucleu/Cub/Coordonate.cs) cere cont, sens și dată. Modelul comun este adecvat mișcărilor pentru care aceste coordonate au sens. Extinderea lui către alte fapte trebuie evaluată separat.

### Revizuirea amortizării este cazul concret de comparație

În [ScenariiImo.cs](../../../nou/tools/ModelCheck/ScenariiImo.cs), metoda `Revizuire` verifică două postări cu valoare zero care păstrează fișa și cauza; verifică și revenirea la durata anterioară după inversarea evenimentului.

[Cititorul imobilizărilor](../../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Cub/Citiri/Imobilizari.cs) selectează postările, apoi citește metoda, durata și alte atribute din liniile tipate ale documentelor pentru a reconstrui evenimentele.

Aceasta este o implementare existentă, nu o funcționalitate lipsă. Ea arată însă că semantica necesară amortizării nu este integral exprimată de măsurile cubului. Cubul identifică și evenimente, iar parametrii lor sunt păstrați în documente.

Alternativa de comparat este un efect explicit de stabilire a parametrilor, persistat într-un registru temporal, fără postări zero folosite numai ca ancore ale evenimentului.

### Flax declară destinații cu semantici diferite

Exportul local analizat este `D:\Dev\Work\EServices\SkyContaSource\DataBase`. Inventarul XML conține 107 tipuri de documente, 1 registru contabil, 13 de acumulare, 85 de informații și 5 de calcul. Aceste numere includ configurare și infrastructură; nu sunt un catalog al funcționalităților care trebuie preluate.

| Fișier relativ la export | Observație verificată |
|---|---|
| `Documents/AprovizionareMarfuriSiServiciiPrimite.xml`, secțiunea `RegisterRecords` | Declară nouă destinații, între care loturi, TVA, aprovizionări, prețuri și contabilitate generală |
| `Documents/SchimbareaParametrilorAmortizariiMF.xml`, secțiunea `RegisterRecords` | Declară numai registrele de informații pentru parametrii amortizării și evenimentele mijlocului fix |
| `InformationRegisters/ParametriiDeAmortizareMF.xml` | Registru subordonat documentului, periodicitate `RecorderPosition`, resurse precum durată, valoare de calcul și coeficient |
| `AccumulationRegisters/Loturi.xml` | Registru de sold cu cantitate și valoare, pe dimensiuni proprii stocului |
| `AccumulationRegisters/TVACumparari.xml` | Registru de rulaje cu bază și TVA, pe dimensiuni fiscale |

Metadatele arată registrele în care documentul poate scrie. Nu demonstrează că toate sunt alimentate în fiecare operare și nu înlocuiesc verificarea algoritmilor sau a datelor efective.

Documentația oficială 1C confirmă separarea dintre [declarația registrelor și algoritmul de postare](https://1c-dn.com/library/tutorials/practical_developer_guide_creating_register_records_of_document/), precum și semantica distinctă a [registrelor de informații](https://1c-dn.com/library/tutorials/practical_developer_guide_understanding_information_registers/) față de acumulări.

Flax este evidență pentru alternative de modelare. Nu devine specificația canonică a Atlas.

## Beneficii posibile și costuri

| Aspect | Cub unic | Cub și registre specializate |
|---|---|---|
| Document nou cu efecte deja cunoscute | Extensie locală prin declarant | Extensie locală prin compoziție |
| Categorie nouă de fapt | Adaptare la vocabularul comun sau mecanism auxiliar | Contract local, dacă infrastructura permite extensia |
| Coerența valorilor comune | Un fapt poate susține mai multe citiri | Necesită proprietate clară a faptului și reguli între efecte |
| Storno și snapshot | Mecanisme comune puternice pentru mișcări | Infrastructură comună, cu semantică de corecție specifică registrului |
| Citirea parametrilor istorici | Poate necesita identificarea cauzei și citirea documentului | Contract temporal explicit |
| Cost de întreținere | Risc de excepții în nucleul general | Risc de duplicare a mecanismelor și divergență între registre |

Motivul inițial al cubului rămâne valid: evitarea mai multor evidențe independente ale aceleiași valori și a duplicării mecanismelor. Propunerea trebuie să păstreze aceste câștiguri.

Punctul de îngustare discutat este semantic și organizațional: cât cod comun trebuie schimbat și reverificat pentru o extensie. Nu s-a demonstrat un blocaj de performanță al cubului. Serializarea actuală a scrierii este o problemă distinctă; adăugarea registrelor nu o elimină automat.

## Condiții de corectitudine

1. Toate efectele obligatorii reușesc sau sunt retrase împreună.
2. Fiecare efect păstrează identitatea operației, documentului și liniei, după caz, plus originea unei corecții.
3. Repetarea aceleiași comenzi nu dublează efectele; cheia de idempotență și comportamentul reluării trebuie stabilite.
4. Faptele și ipotezele folosite la calcul rămân protejate până la commit, prin mecanismul de concurență ales.
5. Relațiile dintre efecte sunt validate explicit, de exemplu între costul descărcării și efectul contabil asociat.
6. Deciziile și politicile consumate rămân identificabile. Reconstrucția unui raport nu recalculează retroactiv evaluarea cu politica de azi.
7. Corecția respectă semantica registrului. Negarea unei sume și retragerea unei valori temporale nu sunt același algoritm.
8. Perioadele închise și dependenții sunt verificați și pentru noile efecte. Tratamentul unei schimbări temporale retroactive trebuie precizat.
9. Fiecare proiecție derivată are o sursă autoritativă clară și o regulă de reconstrucție.

## Raportul cu invarianții actuali

[Invarianții Atlas](../../invarianti.md) rămân regulile active. Această consultare nu îi amendează.

| Invariant | Implicație pentru propunere |
|---|---|
| I — operația intră ca document | Proveniența se păstrează; acceptarea unui document fără postare în cub, dar cu efect informațional, cere clarificarea formulării actuale |
| II — motorul nu cunoaște frunzele | Compatibil cu declaranți și efecte tipate; nucleul nu trebuie să acumuleze comutatoare pe tipuri de document |
| III — registrele sunt un singur cub | Regula amendată prin decizia 90 ar necesita o nouă decizie explicită; trebuie precizate și regulile de citire temporală |
| IV — structură în cod, politică în date | Se păstrează |
| V — sursele externe sunt evidență | Se păstrează; nu se copiază automat schema Flax |
| VI — evaluarea este fapt la operare | Se păstrează; registrele noi nu introduc reevaluare retroactivă implicită |

Anularea și stornoul trebuie analizate cu excepțiile deja existente în Atlas. Propunerea nu presupune că implementarea actuală este strict append-only în toate operațiile și nici nu autorizează ștergeri suplimentare.

## Experimentul propus pentru evaluare

Prima comparație recomandată este revizuirea parametrilor de amortizare: există implementare, scenarii și o alternativă semantică precisă.

1. Definirea unui efect tipat de stabilire a parametrilor și a contractului temporal.
2. Păstrarea cubului pentru valorile contabile și fiscale.
3. Prototip izolat care înlocuiește rolul postărilor zero cu registrul temporal.
4. Compararea acelorași scenarii: revizuire, amortizare ulterioară, anulare, storno, dependenți, perioadă închisă și reluarea comenzii.
5. Evaluarea unei extensii concrete suplimentare, cu rezultatele așteptate stabilite înaintea implementării.

Criteriile de comparație sunt numărul de contracte comune atinse, claritatea citirii istorice, duplicarea mecanismelor, localitatea testelor, compatibilitatea rapoartelor și costul unei schimbări ulterioare. Numărul de linii și timpul de execuție sunt indicatori auxiliari, măsurați în condiții comparabile.

Dacă prototipul mută complexitatea fără să reducă dependențele sau dublează infrastructura de corecție, direcția nu este justificată de acest caz. Separarea fiscalității sau a stocului nu este inclusă implicit în experiment.

## Întrebări pentru runda de analiză

1. Care sunt funcționalitățile concrete din catalogul produsului pentru care modelul actual produce cost transversal?
2. Postările zero și citirea parametrilor din documente reprezintă un compromis suficient de bun sau o limită care merită eliminată?
3. Ce fapte trebuie să rămână în cub pentru a evita evidențe paralele ale aceleiași valori?
4. Pentru fiecare registru nou, care este faptul distinct și care este doar o proiecție derivată?
5. Poate infrastructura de operare primi un tip nou de efect fără modificări specifice în coordonator?
6. Cum se definește valoarea temporală valabilă după corecții, retrageri și operații cu aceeași dată?
7. Ce reguli între efecte asigură coerența contabilitate–stoc–fiscalitate?
8. Ce mecanisme de storno, perioadă, audit și snapshot pot rămâne comune și ce comportamente trebuie specializate?
9. Ce amendamente ale invarianților sunt necesare și ce garanții se păstrează?
10. Ce rezultat măsurabil ar justifica adoptarea, limitarea sau respingerea propunerii?

Răspunsul consultării ar trebui să recomande una dintre variante: păstrarea modelului actual, adăugarea doar a registrelor informaționale sau extinderea către mai multe familii de efecte. Recomandarea trebuie susținută prin cazuri concrete și costuri comparate, inclusiv argumente împotriva direcției preferate.

## Limitele acestei sinteze

Constatările despre Atlas provin din codul și documentația citite în sesiunea din 2026-10-17; cele despre Flax provin din metadatele exportului local. Nu au fost executate teste, benchmark-uri sau validări în baza Flax pentru această consultare.

Nu există încă o decizie de implementare, un contract de migrare sau o demonstrație a câștigului de viteză. Direcția reținută pentru analiză este păstrarea cubului acolo unde exprimă natural faptele și verificarea unei căi explicite pentru efectele cu altă semantică.
