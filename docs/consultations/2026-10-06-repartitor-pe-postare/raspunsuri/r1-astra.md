**Verdict: forma 3 pentru nucleul contabil; forma 5 dacă legătura dintre picioare este necesară operațional.** Coordonata care trebuie să se stingă în sold trebuie separată de participanții evenimentului și de partenerul fiscal. Nicio declarație fiscală nu justifică amestecarea acestor roluri.

**1. Clasament**

| Loc | Formă | Motiv |
|---|---|---|
| 1 | **3** | Exprimă direct identitatea analitică a piciorului și separă partenerul fiscal, cu minimum de redundanță. |
| 2 | **5** | Păstrează avantajele formei 3 și adaugă trasabilitate, dar perechea nu poate recupera informații care lipsesc de pe ambele picioare. |
| 3 | **1b** | Poate produce soldurile cerute, însă suprapune sensuri în `Partener` și ascunde categorii tehnice în `Gestiune`. |
| 4 | **4** | Separă corect soldul de eveniment, dar dublează capetele și supraestimează capacitatea lor de a determina partenerul fiscal. |
| 5 | **1** | Absența partidei poate elimina chiar identitatea creditorului, deși analiza pe persoană rămâne necesară. |
| 6 | **2** | Propagă identități străine piciorului și produce fragmentarea soldurilor dacă ambele coloane intră în cheie. |

OMFP 1802 cere evidența creanțelor și datoriilor pe persoane, independent de mecanismul informatic al partidelor; forma 1 este deci insuficientă dacă identitatea nu există nici în analiticul contului. [OMFP 1802, pct. 329](https://static.anaf.ro/static/10/Anaf/legislatie/OMFP_1802_2014.pdf)

Clasamentul presupune că forma 3 permite **mulțimi de feluri admise și politici pe analitice**, nu o etichetă rigidă pentru întregul cont sintetic.

**2. Postările în formele 3, 4 și 5**

Notație: `S` furnizor; `A/B` terți; `E` angajat; `G1/G2` gestiuni; `U` loc de consum; `B1/B2` conturi bancare proprii; `∅` nul.

În tabel:

- postările și repartitorii sunt scriși în aceeași ordine;
- `D/C` reprezintă latura; valorile sunt pozitive dacă nu apare minus;
- forma 4 este `(Ieșire,Intrare;Grupare)`, completată pentru **fiecare picior**;
- forma 5 este `(Grupare;Pereche)`, moștenind blocul fiscal din forma 3;
- cantitatea este semnată independent de debit/credit; dacă lipsește, este zero.

Presupun analiză pe `U` pentru cheltuielile relevante, fără repartitor de sold pe 4426, 581, 711 și 890. Sumele sunt ilustrative: bază 100, TVA 21.

| Caz și postări | Forma 3: R | Forma 4: toate cele trei coloane | Forma 5 |
|---|---|---|---|
| a. D371 100; C401 100 | G1; S | (S,G1;G1); (S,G1;S) | (G1;m1); (S;m1) |
| a. D4426 21; C401 21 | ∅; S | (S,G1;∅); (S,G1;S) | (∅;m2); (S;m2) |
| a. Plata: D401 121; C5121 121 | S; B1 | (B1,S;S); (B1,S;B1) | (S;m3); (B1;m3) |
| a. Reclasificare pentru consum: D3028 40; C371 40 | G1; G1 | (G1,G1;G1); (G1,G1;G1) | (G1;m4); (G1;m4) |
| a. Consum: D6028 40; C3028 40 | U; G1 | (G1,U;U); (G1,U;G1) | (U;m5); (G1;m5) |
| b. D371 **−40**; D371 **+40** | G1; G2 | (G1,G2;G1); (G1,G2;G2) | (G1;m6); (G2;m6) |
| c. D401 70; C4111 70 | A; B | (B,A;A); (B,A;B) | (A;m7); (B;m7) |
| d. D581 100; C5121 100 | ∅; B1 | (B1,B2;∅); (B1,B2;B1) | (∅;m8); (B1;m8) |
| d. D5121 100; C581 100 | B2; ∅ | (B1,B2;B2); (B1,B2;∅) | (B2;m9); (∅;m9) |
| e. D628 100; C401 100 | U; S | (S,U;U); (S,U;S) | (U;m10); (S;m10) |
| f. Consum: D601 100; C301 100 | U; G1 | (G1,U;U); (G1,U;G1) | (U;∅); (G1;∅) |
| f. Producție: D345 100; C711 100 | G1; ∅ | (∅,G1;G1); (∅,G1;∅) | (G1;∅); (∅;∅) |
| g. Avans: D542 121; C5121 121 | E; B1 | (B1,E;E); (B1,E;B1) | (E;m11); (B1;m11) |
| g. Decont: D628 100; C542 100 | U; E | (E,U;U); (E,U;E) | (U;m12); (E;m12) |
| g. TVA: D4426 21; C542 21 | ∅; E | (E,U;∅); (E,U;E) | (∅;m13); (E;m13) |
| h. Deschidere: D371 100; C890 100 | G1; ∅ | (∅,∅;G1); (∅,∅;∅) | (G1;∅); (∅;∅) |
| h. Deschidere: D890 100; C401 100 | ∅; S | (∅,∅;∅); (∅,∅;S) | (∅;∅); (S;∅) |

Precizări indispensabile:

- **a:** recepția are `q=+10/−10`, produs P, lot L; reclasificarea `+4/−4`, același lot; consumul `+4/−4`, cu contracantitatea pe cheltuială, în afara stocului. Rezultă 401/S=0 și stoc L/G1=6 bucăți, 60 lei. Postările 401 folosesc aceeași partidă; dacă plata intră întâi pe altă partidă, trebuie și transferul de alocare.
- **b:** `q=−4/+4`, același produs și lot; soldul total 371 nu se modifică.
- **c:** presupun un act justificativ pentru decontarea între persoane diferite; înregistrarea singură nu dovedește existența unei compensări valabile.
- **d:** folosesc 581 pentru viramentul bancar; convenția transferului analitic fără rulaj nu trebuie extinsă automat la toate viramentele de trezorerie. [Funcționarea contului 581](https://static.anaf.ro/static/10/Anaf/legislatie/OMFP_1802_2014.pdf)
- **f:** consumul are `+2/−2` pentru P1/L1, producția `+1/−1` pentru P2/L2; contracantitățile sunt pe 601 și 711. Nu există conservare între produse diferite, ci separat pentru fiecare produs, prin capetele tehnice.
- **g:** presupun factură eligibilă emisă societății de S, achitată din avans de E; 542/E se închide, partenerul fiscal este S.
- **h:** `q=+10/−10` pe prima înregistrare; 890 se închide valoric. „Fără document” poate însemna fără antet comercial, dar cere lot de import, proveniență și justificarea soldurilor.

În a și g, **371/628 și 4426 poartă fiscal S**, cu roluri bază/taxă distincte, fără dublarea sumelor.

**Pierderi pe forme:**

- **3:** soldurile se închid; se pierde contextul de flux care nu este coordonată proprie, precum G1 pe 4426.
- **5:** aceleași solduri; perechea 4426–401 recuperează S, dar nu G1; legătura dintre m8 și m9 cere identitatea viramentului.
- **4:** soldurile se închid prin Grupare; în g, capetele E/U nu conțin S, deci D394 cere atribut fiscal separat. În f/h, nulurile din flux sunt o **extensie necesară**: definiția exclusiv prin două capete reale nu acoperă aceste evenimente.

**3. Partenerul fiscal**

Este **atribut al faptului fiscal**, obligatoriu unde natura raportării îl cere; poate coincide cu repartitorul de sold, fără a deveni prin aceasta coordonată de sold.

D394 distinge operațiunile după partener, tip și cotă; există și raportări agregate, deci afirmația „fiecare fapt D394 are obligatoriu CUI individual” este prea tare. [Structura D394](https://static.anaf.ro/static/10/Anaf/Declaratii_R/AplicatiiDec/structD394_15092025.pdf)

Documentul poate furniza valoarea **la postare**, cu prioritatea explicită a liniei. Citirea ulterioară dintr-un antet modificabil este nesigură.

Stornoul păstrează partenerul faptului anulat. Corecția A→B trebuie să lase auditabil `−bază/−TVA la A`, apoi `+bază/+TVA la B`, fără a muta automat stocul sau soldurile corecte. Corectarea unei declarații depuse și emiterea unui document de corecție sunt evenimente diferite; D394 permite înlocuirea declarației eronate. [Ghid ANAF D394](https://static.anaf.ro/static/21/Anaf/Ghid_D394_2016.pdf)

D300 cere clasificarea fiscală și perioada potrivită, nu solduri pe furnizori; perioadele relevante pentru D300 și D394 nu trebuie presupuse identice în toate regimurile. [Instrucțiuni D300](https://static.anaf.ro/static/10/Anaf/legislatie/OPANAF_174_2026.pdf)

**4. Gestiunile virtuale**

**Nu sunt necesare ca gestiuni.** Este necesară distincția dintre stoc real și suportul tehnic al contracantității.

În 3/4/5 o înlocuiesc un rol explicit al postării — stoc real, contracantitate — și natura evenimentului. Furnizorul identificat nu înlocuiește acest rol: același terț poate păstra fizic stoc propriu în custodie.

Consum, Inventar și Transformare devin categorii de eveniment, fără partener fictiv. Contracantitățile rămân pe postări, cu produsul și semnul corect. `Repartitor=null` singur nu trebuie să însemne simultan „necunoscut”, „neaplicabil” și „în afara stocului”.

**5. Deducerea grupării în forma 4**

Din latură, semn și natura contului, **nu în general**.

Transferul cu valoare negativă distruge convenția „minus=sursă”: pentru valoare −40, sursa primește +40, destinația −40. Cantitatea poate avea alt semn decât valoarea.

Aș păstra Ieșire/Intrare drept **roluri nominale stabile**. Stornoul inversează măsurile, nu schimbă participanții; un retur fizic este alt eveniment și poate inversa efectiv capetele.

Gruparea trebuie scrisă sau derivată dintr-un rol explicit, imuabil, al piciorului și o politică versionată. Prima variantă simplifică agregarea.

Repartitorul explicit al liniei prevalează asupra antetului. Dacă niciun capăt nu corespunde antetului, se scriu capetele reale ale evenimentului.

**6. Cont sau cont și document?**

**Contul stabilește contractul analitic; tipul documentului și rolul piciorului stabilesc completarea.** Documentul nu trebuie să schimbe arbitrar identitatea după care aceeași datorie se stinge.

- **461/462:** persoane juridice sau fizice, inclusiv angajați; nu un singur subtip rigid.
- **542:** titularul avansului, nu furnizorul fiscal; numerarul acordat terților are tratament distinct, inclusiv 461.
- **581:** cont de tranzit; gruparea alternativ pe banca sursă și banca destinație produce solduri artificiale; identificarea viramentului aparține unei referințe distincte.
- **4091/419:** furnizor/client chiar dacă reprezintă avansuri.
- **408/418:** terțul rămâne necesar înaintea facturii finale; unitatea urmărită poate fi documentul provizoriu.
- **Stocuri la terți:** custodele și locul fizic pot fi două informații simultane; o gestiune externă identificată poate referi custodele, dar dacă relația nu este funcțională, o singură coordonată nu ajunge.

Distincțiile contabile pentru 542, 461 și stocurile la terți sunt prevăzute în [OMFP 1802](https://static.anaf.ro/static/10/Anaf/legislatie/OMFP_1802_2014.pdf).

SAF-T cere maparea identităților și informațiilor fiscale după rolul înregistrării; schema de export nu impune copierea furnizorului pe toate coordonatele interne. [Ghid D406](https://static.anaf.ro/static/10/Anaf/Informatii_R/SAF_T_Ghidul_D406_1712021.pdf)

**7. Agregate și snapshot-uri**

Fie K cheia comună: entitate, perioadă, cont, monedă și coordonatele necesare unității/produsului.

| Formă | Cheia de sold recomandată |
|---|---|
| 1/1b | K + proiecția relevantă din Partener/Gestiune; nu automat ambele |
| 2 | K + ambele produce fragmentare; proiecția selectivă salvează soldul |
| 3 | K + Repartitor |
| 4 | K + Grupare; Ieșire/Intrare numai în agregatele de eveniment |
| 5 | K + Grupare; perechea nu intră în snapshot |

Cardinalitatea depinde de combinațiile existente, nu obligatoriu de produsul cardinalităților. Totuși, 100 gestiuni × 10.000 furnizori pot genera un milion de grupe în locul a 100, dacă lotul nu face deja furnizorul redundant.

La milioane de rânduri contează memoria agregărilor, indexurile, scrierile și reconstrucția snapshot-urilor. Forma 4 mărește rândurile; forma 5 adaugă joinuri, potențial între partiții. Două identificatoare suplimentare de 8 octeți înseamnă circa 160 MB la zece milioane de rânduri, înaintea indexurilor și compresiei. Verdictul de performanță cere măsurători.

**8. Schimbarea coordonatelor append-only**

Cer înainte:

- definiția exactă a cheilor și tuturor cititorilor;
- dependențele funcționale demonstrate pe date;
- reconciliere pe cont, repartitor, partidă, lot și perioadă;
- verificarea rulajelor, nu doar a soldurilor finale;
- reproducerea declarațiilor istorice, inclusiv storno și corecții;
- măsurători și procedură de revenire.

Se poate face **aditiv**: proiecție V2 sau extensie versionată pe ID-ul postării, scriere simultană pentru evenimente noi, reconstrucție și comparație înainte de comutarea cititorilor.

Informația absentă nu se reconstituie prin presupunere. Corecțiile economice cer postări compensatoare; schimbarea reprezentării tehnice nu trebuie să creeze rulaje economice.

**9. Contraexemple numerice**

- **1:** servicii de 100 la A și 200 la B, 401 fără partide și fără partener: soldul 300 nu permite recuperarea celor două datorii.
- **1b:** stoc 100 cu Partener=A fiscal, ieșire integrală 100 cu Partener nul: cheia brută păstrează +100/A și −100/nul; proiecția corectă elimină defectul.
- **2:** factură 121 pe `(A,G1)`, plată 121 pe `(A,B1)`: 401 total zero, două solduri analitice opuse.
- **3:** 10 bucăți la custodele A, dintre care 6 la locația X și 4 la Y: R=A pierde locațiile; R=X/Y pierde custodele dacă acesta nu poate fi determinat separat.
- **4:** decont E, factură S de 100+21: capetele E/U raportează greșit furnizorul sau nu-l pot determina; Grupare=E nu repară informația fiscală.
- **5:** aceeași TVA de 21 poate proveni din recepție în G1 sau G2; perechea `(4426,∅)–(401,S)` este identică în ambele situații și nu recuperează gestiunea.

Ultimele exemple arată limite de informație; **nu demonstrează că formele 3 sau 5 produc inevitabil solduri greșite**.

Mi-ar schimba verdictul: existența partenerului în analiticul contului; necesitatea mai multor coordonate independente pe același picior; documente și linii imuabile care păstrează integral contextul; utilizarea intensă, măsurată, a filtrelor de flux; perechi realmente suficiente pentru toate interogările; costuri demonstrate ale migrării; reguli actuale care exclud atributele fiscale și contextuale din cheia soldului.

**Încredere: 4/5** în separarea semantică; **3/5** în ordinea dintre formele viabile, fără date și interogări reale.