**Resping amendamentul 1 în forma propusă. Accept amendamentul 2 cu schimbări obligatorii.** [judecată]

La primul, problema decisivă este dispariția filtrării fără definirea unei proiecții contabile înlocuitoare: transformarea de valoare V introduce debit și credit de câte **2V**, iar agregarea pe produs poate ascunde complet redistribuirea valorii. La al doilea, politica explicită este utilă, dar definiția găleții, închiderea și nominalizarea sunt insuficient precizate. [judecată]

Am lucrat exclusiv prin citiri și căutări: fără modificări, build, ModelCheck sau acces la baze. Referințele folosesc prescurtările din fișa de fapte: `N/` pentru nucleu, `M/` pentru modul; suplimentar, `C/` înseamnă `M/Cub/Citiri/`, iar `D/` înseamnă `docs/nucleu/`. Clasificările, calculele proprii și recomandările sunt marcate `[judecată]`.

**Față de runda 1, păstrez preferința pentru X, dar corectez un argument important.** Cititorul contabil exclude tranzacțiile `Transfer`, nu doar contraponderile. Prin urmare, nici X nu actualizează soldul contabil filtrat pe produs pentru o asamblare integral transfer; soldul corect se obține din cititorul loturilor. Nu pot prezenta această limită ca defect exclusiv al Y. (`C/Contabil.cs:42`, `C/Loturi.cs:21`)

Răspunsurile la toate condițiile enumerate atunci, la `docs/consultare/runda1-proprie.md:115`:

| Fapt care putea schimba verdictul | Ce rezultă acum |
|---|---|
| Natura economică a documentelor | Conturile sursei sunt aproape exclusiv 371.1; aceasta susține cercetarea reambalării, dar nu dovedește natura fiecărei operațiuni. (`docs/consultare/fapte.md:36`) [judecată] |
| Nod exclus structural sau alte semne | Nu: `Miscare` produce D/C pozitive, iar filtrul exclude numai contraponderi cu valoare zero. (`N/Cub/Miscare.cs:11`, `C/Transformare.cs:7`) |
| X compensează operațiuni independente/pierde legături | Grupează pe cont; păstrează lotul și cauza liniei, dar nu construiește o matrice consum–produs. Independența economică a liniilor nu este demonstrată. (`M/Declaratii/DeclarantAsamblare.cs:86`, `:112`) |
| Costuri suplimentare, pierderi, producție în curs | Declarantul verifică P=R și absoarbe diferența evaluării; aceste mecanisme nu reprezintă asemenea fenomene economice. Existența lor în documentele reale rămâne necunoscută. (`M/Declaratii/DeclarantAsamblare.cs:78`, `:94`) [judecată] |
| R este doar o identitate tehnică nealocată | Materialul primit nu descrie această implementare; condiția rămâne nedovedită. (`docs/consultare/fapte.md:53`) [judecată] |
| Q are reconciliere, istoric și atomicitate | Există gard cronologic pentru lipsa fișei și serializare; acestea nu demonstrează existența întregului Q. (`M/Cub/Materializare.PozitieFaraFisa.cs:32`, `M/Motor/TranzactieComanda.cs:14`) |
| Practica arată numeroase excepții | Recensământul oferă distribuția conturilor, nu frecvența corecțiilor, pierderilor sau excepțiilor de export. (`docs/consultare/fapte.md:38`) |

Îmi reduc încrederea în Q față de runda 1: propunerea concretă de închidere poate bloca tocmai rezolvarea soldului. În schimb, retrag suspiciunea că lipsește controlul stornării stocului: materializatorul verifică inversa completă înaintea scrierii. (`M/Cub/Materializare.cs:105`, `:113`) [judecată]

**Y nu trece prin nucleul actual fără modificări.** `Miscare` permite reprezentarea dorită și motorul o pune în `Operare`; însă `Conservare.VerificaTransformarile` respinge orice valoare nenulă pe gestiunea `Transformare`. `Mutare` nu rezolvă problema: produce postări pe aceeași latură și cere același cont. (`N/Motor/Motor.cs:10`, `N/Conservare.cs:26`, `N/Motor/Mutare.cs:14`)

După eliminarea verificării specifice X, fiecare mișcare Y se echilibrează singură. Consumul de 100 și produsul de 130 ar satisface egalitatea globală D=C, lăsând nodul cu −30. Este obligatoriu gardul **Σ(D−C) pe nod, document și carte = 0**, verificat pe fiecare cont. [judecată]

Acesta nu impune singur „un singur cont”: două transformări independente, fiecare echilibrată pe alt cont, îl trec. Mai trebuie cardinalitatea conturilor egală cu unu, ambele roluri prezente și forma capetelor controlată. [judecată]

În declarant aș pune restricția ASM la un cont, natura operațiunii, rolurile, evaluarea și disponibilitatea. În nucleu aș păstra integritatea unui **nod tehnic declarat explicit**, inclusiv închiderea și perechile cantitative; fără ramificație după clasa ASM. Verificările pentru `Transfer` rămân necesare altor mecanisme, dar nu mai verifică ASM-Y. [judecată]

**Cele patru cazuri proprii** folosesc un singur cont și evaluări fără diferență duală. Notația `D A(q;v)` indică postarea lotului real, iar `D T_A(q;v)` nodul fără lot. Inversa schimbă semnul cantității și valorii, păstrând latura. Convențiile provin din `N/Motor/Transformare.cs:26` și `N/Cub/Miscare.cs:11`; cifrele următoare sunt construite aici. [judecată]

| Caz | X | Y |
|---|---|---|
| **2 A/126 → 1 B/126** | D A(−2;−126), D T_A(+2;0); D B(+1;+126), D T_B(−1;0) | C A(−2;126), D T_A(+2;126); D B(+1;126), C T_B(−1;126) |
| **FIFO:** L1=3 A/0,10; L2=2 A/0,13. Consum 4 A; produc B și C | D L1(−3;−0,10), D T_A(+3;0); D L2(−1;−0,07), D T_A(+1;0); D B(+1;0,09), D T_B(−1;0); D C(+1;0,08), D T_C(−1;0) | C L1(−3;0,10), D T_A(+3;0,10); C L2(−1;0,07), D T_A(+1;0,07); D B(+1;0,09), C T_B(−1;0,09); D C(+1;0,08), C T_C(−1;0,08) |
| **Storno după consum parțial:** A6/90→B3/90; ulterior B1/30 consumat. Inversa ASM | D A(+6;+90), D T_A(−6;0); D B(−3;−90), D T_B(+3;0) | C A(+6;−90), D T_A(−6;−90); D B(−3;−90), C T_B(+3;−90) |
| **Corecție în februarie:** ianuarie A4/80→B2/80; februarie inversează și înlocuiește cu A3/60→Bnou3/60 | Inversă: D A(+4;+80), D T_A(−4;0), D B(−2;−80), D T_B(+2;0). Nou: D A(−3;−60), D T_A(+3;0), D Bnou(+3;+60), D T_Bnou(−3;0) | Inversă: C A(+4;−80), D T_A(−4;−80), D B(−2;−80), C T_B(+2;−80). Nou: C A(−3;60), D T_A(+3;60), D Bnou(+3;60), C T_Bnou(−3;60) |

Consecințele sunt următoarele. [judecată]

- **Primul caz:** ambele lasă fizic A=0, B=126. Cubul integral X păstrează această distribuție valorică; Y, agregat fără discriminarea nodului, arată A=126, B=0. Rulajul contabil actual X este zero, Y este 252/252. Cititorul contabil X omite transferul și arată și el A=126/B=0: trebuie folosit cititorul potrivit.
- **FIFO:** L2 rămâne 1/0,06, produsele însumează 0,17. Împărțirea independentă, cu jumătățile rotunjite în sus, dă 0,09+0,09=0,18. În Y, egalitatea D=C nu detectează banul creat; închiderea nodului îl detectează. Rulaj X=0, Y=0,34/0,34. Ultima ieșire trebuie să preia restul, conform `N/Masura/Evaluare.cs:14`.
- **Storno:** inversa ar lăsa B=−1/−30 și A=6/90; conservările algebrice trec. Comanda trebuie refuzată, iar codul verifică soldurile zilnice, inclusiv viitoare. Rulajul inversei Y ar fi −180/−180; X rămâne exclus contabil. (`C/Loturi.cs:73`, `M/Cub/Materializare.cs:113`)
- **Corecție:** ianuarie rămâne neschimbat; după februarie există A1/20 și Bnou3/60. Y are în ianuarie 160/160, iar în februarie −160+120=−40 pe fiecare latură; X are zero. Rescrierea lui ianuarie sau recalcularea inversei la costul curent ar falsifica istoricul. Corecția existentă stornează și creează draft legat, cu lot produs nou. (`M/Motor/CorectieService.cs:70`, `:122`)

**Inventarul cititorilor arată că „dispare filtrul” nu este o simplificare gratuită.** Rezultatele de mai jos presupun Y materializat după modificarea nucleului. Sunt deducții, nu rezultate de execuție. [judecată]

| Cititor și ancoră | Ce întoarce sub Y |
|---|---|
| `C/Contabil.cs:32`, `:48` | Include nodul: D=C=2V; jurnalul păstrează toate cele patru postări. |
| `M/Proiectii/ContabilProiectii.cs:185`, `:196`, `:358` — Atomi, Balanță, BalanțăPlan | Sold sintetic corect; rulaje 2V. Filtrarea numai pe produs poate anula redistribuirea. Gestiunea este filtru opțional. |
| Același fișier, `:302`, `:439`, `:510` — SoldParteneri, FișaCont, RegistruJurnal | SoldParteneri: net sintetic neschimbat, debit/credit mărite; filtrul de produs moștenește problema. Fișa afișează tranzitul și contrapartida aceluiași cont; jurnalul include nodul. |
| `M/Motor/SolduriService.cs:217`, `:238`, `:273` | Snapshotul și cumulul transportă aceleași valori; defectul nu dispare la închiderea lunii. |
| `M/Saft/SaftProiectii.cs:273`; `SaftProiectii.PeCub.cs:303`, `:642` | Soldurile conturilor rămân corecte; GL și totalurile includ 2V. |
| `M/Saft/SaftProiectii.PeCub.cs:184` — Parteneri | Filtrează conturile cu rol de terț; ASM pe stoc obișnuit nu intră. Dacă rolurile se suprapun, agregarea include tranzitul. |
| `M/Saft/SaftProiectii.PeCub.Stocuri.cs:499` | Reconcilierea netă pe cont/document vede zero contra zero și nu detectează rulajul suplimentar. Legătura Movement–GL apare acum și pentru ASM-Y (`:367`). |
| `M/Motor/InchidereTvaService.cs:272` | Pentru conturile TVA distincte, neschimbat; calculează netul, deci nici tranzitul echilibrat nu schimbă rezultatul. |
| `C/Imobilizari.Conturi.cs:19`; `M/Motor/ImobilizariFapte.cs:49` | Nodurile Y intră în „fără fișă”; pe conturi suprapuse pot deveni disponibil/suport fals. |
| `M/Cub/Materializare.PozitieFaraFisa.cs:33` | Citește inclusiv nodul, fără filtrul contraponderii; poate aplica nejustificat gardul suport. |

Apelurile rămase ale filtrului nu au același risc: `Partide.Postari` cere partidă nenulă; `Partide.VerificaAcoperire` poate însă confunda nodul cu lipsa nominalizării dacă acel cont urmărește partide. Verificarea provenienței trebuie extinsă și la nod. Apelul ModelCheck este selecția unui mutant pe partidă, nu agregator. (`C/Partide.cs:178`, `:198`, `C/Invarianti.cs:94`, `nou/tools/ModelCheck/AcoperireInvarianti.cs:103`)

Cititorii loturilor, fișelor și împerecherilor filtrează unitatea; stocul și SAF-T Stocuri rămân corecte. `Plati.Postari` ar include ASM la nivelul sursei generale, dar alocarea restrânge documentele primite. (`C/Loturi.cs:21`, `C/Imobilizari.cs:47`, `M/Proiectii/ImperecheriProiectii.cs:50`, `C/Plati.cs:60`, `:75`)

**Clasificarea verificărilor trebuie să evite transformarea implementării în principiu.** „Rămâne” înseamnă principiu contabil sau structural justificat independent; nu pretind că fiecare convenție tehnică este lege contabilă. [judecată]

| Verificare în `N/Conservare.cs` | Verdict |
|---|---|
| Tranzacție nevidă (`:7`); document, cauză și excepția stornoului atribuit (`:44`) | Rămân: integritate și proveniență. |
| D=C separat pe carte (`:71`) | Rămâne; insuficient pentru închiderea nodului. |
| Cantitate conservată pe produs (`:83`) | Rămâne ca principiu al modelului cu capete externe; nu conservare fizică A→B. |
| Transfer: valoare pe cont/latură și cantitate pe cont/produs (`:96`) | Rămâne pentru transfer; aplicarea sa la ASM este alegerea X. |
| Excepțiile deschiderii și semnul ei (`:14`, `:119`) | Convenții ale modelului de deschidere, independente de X/P; rămân. |
| Data, gestiunea/produsul cantității, unitatea în gestiune reală (`:134`) | Rămân. Excepția virtuală cere rol structural sigur. |
| Concordanța contului unității; lot–produs–gestiune; partidă–partener (`:158`) | Rămân; nu sunt politica P pentru poziția nenominalizată. |
| Perechi reale/virtuale, cauză și cantitate (`:26`) | Principiul trasabilității rămâne. |
| Valoarea virtuală obligatoriu zero (`:36`) | Artefact X; se rescrie pentru Y. Restricțiile fără TVA/valută definesc domeniul ASM, nu conservarea universală. |

Pentru **toate verificările din `C/Invarianti.cs`**: [judecată]

- `:19`, `:42`, plus acoperirea loturilor și comparațiile imobilizărilor: martori ai regimului dual, **nici principii, nici artefacte P**; dispar/reformulează la tăiere. A le eticheta forțat „X” ar fi fals.
- `:35` echilibrul și `:64` transferurile: rămân; ASM-Y nu mai intră în mulțimea transferurilor.
- `:79` taxa egală cu taxa liniei: rămâne.
- `:94` proveniența stornoului: rămâne; excluderea contraponderii este artefact X.
- `:104`: completitudinea fiscală, autocolectarea validă, unicitatea rolului, calificarea coerentă și inversa fiscală fidelă rămân fiecare.
- Apelul partidelor (`:55`): identitatea unei partide declarate rămâne; obligația dedusă din cont și prezența terțului este artefact P. (`C/Partide.cs:198`)
- Apelul împerecherilor (`:56`): nominalizarea documentelor tipate rămâne; comparația cu `TotalStingere` este martor dual. (`M/Proiectii/ImperecheriProiectii.cs:101`)
- Apelul fișelor (`:57`): cauza, originea inversei și suportul nominalizării rămân; comparațiile cu registrul sunt duale. (`C/Imobilizari.cs:9`)
- Apelul explicațiilor (`:58`): explicația, referința validă, inversa exactă, mecanismul, corespondența decizie–ieșire, evaluarea și alocările FIFO rămân. (`C/Explicatii.cs:53`, `:100`)

**Scenariile sunt listă de rescriere, nu justificare.** În tabel, „R” înseamnă că rezultatul economic rămâne; „F” că forma/politica așteptării se rescrie. Clasificarea este [judecată].

| Scenariu; linia din `D/scenarii/ASM.md` | Clasificare pentru Y/Q |
|---|---|
| 01; `:30` | R solduri; F Transfer/rulaj zero. |
| 02; `:31` | R solduri; F laturi/valori virtuale. |
| 03; `:32` | R repartizare; F postări. |
| 04; `:33` | F acceptare și nota directă; noul domeniu refuză. |
| 05; `:34` | F două feluri și acceptare multicont. |
| 06; `:35` | F acceptare multicont. |
| 07; `:36` | R inversă exactă și refuz repetare; F postări. |
| 08; `:37` | R graniță temporală; F fixture multicont/două feluri. |
| 09; `:38` | R reversibilitate fără dependenți; F fixture multicont. |
| 10; `:39` | R corecție și solduri; F postări. |
| 11; `:40` | R evaluare și golire exactă. |
| 12; `:41` | R refuzul banului neacoperit. |
| 13; `:42` | R integritatea documentului/loturilor. |
| 14; `:43` | R protecția dependențelor. |
| 15; `:44` | R perioada înregistrării. |
| 16; `:45` | R fără TVA în domeniul restrâns; F valoarea zero. |
| 17; `:46` | F absorbția duală; R evaluarea și soldul final. |
| 18; `:47` | R inversa istorică; F absorbția. |
| 19; `:48` | R reevaluare și redistribuire înainte de operare. |
| 20; `:49` | F grupuri, absorbție, Transfer. |
| 21; `:50` | F multicont și absorbție. |
| 22; `:51` | F acumularea Δ și acceptare multicont. |
| 23; `:52` | F refuzul specific Δ. |
| 24; `:53` | F negativitate produsă prin Δ; R validarea valorii finale. |
| 25; `:54` | R golire 0/0; F comparația duală. |
| 26; `:55` | R inversă fidelă dacă există istoric; fără obiect numai în greenfield. |

NTC-01 (`D/scenarii/NTC.md:28`) și NTC-14 (`:41`): **F/P**, acceptarea devine condiționată de regim, deși sămânța păstrează rezultatul. NTC-15 (`:42`): **R**, nota anonimă nu stinge factura; acceptarea este **F/P**. IMO-27, 28, 31 (`D/scenarii/IMO.md:41`, `:42`, `:45`): **R în regimul suport** — disponibil cronologic, dependențe, atomicitate; selectarea conturilor protejate este **F/P**. [judecată]

**Q schimbă în primul rând declararea politicii, nu automat rezultatele sămânței.** Nota fără terț revine astăzi fără partidă; cu terț explicit, nominalizează FIFO sau deschide partidă. „Deschisă” nu trebuie să dezactiveze această nominalizare. (`M/Declaratii/DeclarantNotaContabila.cs:52`)

- **Închisă:** gard nou pentru poziția realmente nealocată; cantitatea zero nu îl înlocuiește.
- **Deschisă:** păstrează cazurile anonime NTC menționate, dar cere reconciliere distinctă între sold nominalizat și nealocat.
- **Suport:** păstrează sensul debitor pentru cost și creditor pentru amortizare, disponibilitatea pe coordonate complete și verificarea tuturor zilelor afectate. Înlocuiește deducerea conturilor din tipuri, politici și istoric. (`M/Cub/Materializare.PozitieFaraFisa.cs:13`, `:47`) [judecată]

**Închiderea cu sold, formulată ca interzicere a tuturor postărilor noi, este greșită:** și nominalizarea eliberatoare necesită postări noi. Accept o suspendare a alimentărilor, cu lichidare permisă; „închisă complet” numai după rezolvarea pozițiilor detaliate. Netul zero poate ascunde +100 și −100 pe produse diferite. [judecată]

Blocajul suport este global (`97001`), iar scrierea este deja serială per bază (`97000`). Extinderea suportului mărește citirile istorice și durata secțiunii seriale; nu introduce pentru prima dată serializarea. Nu aș fragmenta blocajele înaintea măsurării. (`M/Cub/Materializare.Suport.cs:16`, `M/Motor/TranzactieComanda.cs:13`) [judecată]

Aș numi politica `RegimNenominalizat`, evitând `Unitate`, care este deja steag verificat pe dimensiunea organizatorică. (`M/Motor/MotorOperare.cs:526`) Aș folosi **cont × fel de nominalizare**, cu fel așteptat explicit pentru poziția nulă; simplificarea pe cont este sigură numai dacă se impune unicitatea felului admis. [judecată]

**Gestiunea virtuală poate identifica tehnic nodul, dar nu îl separă suficient în contractul actual.** Identitățile sunt deterministe, fără nomenclator; totuși, nodul fără lot ajunge în `Spatiu.Contabil`, exact ca găleata, iar cititorii și gardul fără fișă îl admit. (`N/Cub/GestiuniVirtuale.cs:6`, `N/Cub/Postari.cs:5`, `C/Imobilizari.Conturi.cs:19`)

Alternativa mai sigură este un rol persistent al capătului: nominalizat, nealocat, contracapăt tehnic, cu identitatea transformării și constructori validați. Excluderea tuturor gestiunilor virtuale din contabilitate ar elimina și contrapartide legitime LDI, ale căror conturi vin din politică. (`M/Declaratii/DeclarantDiferenteInventar.cs:83`) [judecată]

**„Același cont” trebuie să însemne aceeași identitate contabilă rezolvată**, nu același tip material sau simbol părinte. Contul este derivat acum din tipul curent al produsului; schimbarea tipului liniei singure nu ocolește verificarea de concordanță. (`M/Motor/Fapte.cs:95`, `M/Declaratii/DeclarantAsamblare.cs:30`)

Dacă nomenclatorul se schimbă între intrare și consum, rezolvarea poate căuta soldul pe alt cont și refuza stoc existent. Invers, aceeași mapare poate ascunde naturi economice diferite: egalitatea conturilor nu dovedește reambalarea. Sunt riscuri deduse, nu exploatări demonstrate. Cer identitatea istorică a contului lotului sau reclasificare explicită. [judecată]

**În plan, recomand păstrarea X simplificat și a gardului P=C la pasul 6 din TR-D9a.** Dacă Y este reformulat și demonstrat prin prototip, locul lui este înlocuirea punctului ASM, cu contract și cititori pregătiți înainte; introducerea după tăiere dublează rescrierea și adaugă compatibilitate între forme. Introducerea directă la pasul 6, fără demonstrație, amestecă două schimbări greu de diagnosticat. (`D/tr-d9-taierea-contract.md:286`, `:665`) [judecată]

S-ar modifica D9-D1, D9-D3, pașii 2/6, ASM-B2…B4 și contractele cititorilor; „filtrul dispare” trebuie înlocuit cu definiția precisă a proiecției. **Q aparține TR-D9b**: în D9a ar extinde schema, politica și comportamentul tăierii. Până atunci trebuie conservate gardurile existente și rezolvat separat I2; Q nu repară lipsa analizei obligatorii la recepție. (`D/tr-d9-inventar.md:23`) [judecată]

**Fișa de fapte înclină prin omisiune:** minimalizează refuzul explicit din `Conservare`, rulajul 2V, filtrarea implicită a transferurilor, blocajul global deja existent și ambiguitatea închiderii. Analogia LDI nu justifică nodul ASM pe același cont. Recensământul corectează util cifra multicont, însă SQL-ul verificabil citește contul tipului, iar cifrele sursei originale rămân doar furnizate, nu reverificate aici. (`N/Conservare.cs:36`, `C/Contabil.cs:42`, `D/scenarii/recensamant-asm-ldi-nir.sql:6`, `docs/consultare/fapte.md:40`)

Trimiterea tuturor cazurilor multicont la BPR omite și reclasificările legitime și faptul că BPR este rezervat, nu disponibil. (`docs/decizii/019-lista-tipurilor-de-documente.md:11`) [judecată]

**Ordinea schimbărilor cerute** este: [judecată]

1. Definirea naturii ASM și a proiecției contabile; fără aceasta, Y rămâne respins.
2. Separarea structurală nod–găleată și gardurile nodului/contului unic.
3. Politica Q cu sens, fel, dată de efect, lichidare și reconciliere detaliată.
4. Stabilizarea contului lotului și păstrarea gardurilor temporale.
5. Rescrierea contractelor, cititorilor, scenariilor și martorilor duali.

Înaintea deciziei aș cere următoarele prototipuri/măsurători; estimările sunt zile de lucru, nu lucrări executate: proiecții X/Y și cele patru cazuri — **2 zile**; loturi cu mapare schimbată și documentul multicont real — **1 zi**; Q, închidere și nominalizare concurentă — **2 zile**; debit, latență p95 și volum citit sub suport extins — **1 zi**. [judecată]

**Încredere:** amendamentul 1, **4/5** — aritmetica și impactul codului sunt clare, natura economică reală rămâne insuficient documentată; amendamentul 2, **3/5** — direcția este bună, dar contractul tranzițiilor și costul operațional nu sunt demonstrate. [judecată]