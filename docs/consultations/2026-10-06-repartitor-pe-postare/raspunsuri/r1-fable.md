# Runda 1 (oarbă): verdict

Analiză doar din regulă contabilă și din coerența modelului, fără cod și fără date. TVA 21% în exemple.

## 1. Clasament și verdict

1. **Forma 5.** Este forma 3 plus identitatea mișcării; fiecare coloană are un singur sens, iar capătul celălalt rămâne citibil.
2. **Forma 3.** Același model de coordonate, dar pierde contul corespondent dacă linia nu identifică deja mișcarea.
3. **Forma 4.** Gruparea e corectă, dar adaugă două coloane de eveniment denormalizate, care pe jumătate din picioare repetă antetul.
4. **Forma 1b.** Închide soldurile, dar `Partener` are două sensuri și `Gestiune` ține locații care nu există.
5. **Forma 1.** Cade la cazul e: contul cere analiză pe repartitor și postarea nu o are.
6. **Forma 2.** Pune antetul pe fiecare picior, deci nicio cheie de sold nu se închide.

**Verdict: forma 5**, cu trei precizări:
- Partenerul fiscal stă în blocul fiscal, nu se citește prin pereche.
- Capătul din afara evidenței se recunoaște după natura contului, nu după o gestiune.
- Cheia de pereche e identitatea mișcării; dacă (document, linie, componentă) există deja pe postare, 5 și 3 sunt aceeași formă.

Motivul principal: soldul are o singură coordonată „cine/unde", iar felul ei e proprietatea contului. Celelalte forme amestecă trei lucruri într-o coloană: coordonata de sold, atributul de eveniment și faptul fiscal.

## 2. Postările

Notație: G1, G2 gestiuni; F furnizor; B, B1, B2 conturi proprii; CC loc de consum; E angajat; S furnizorul de pe bon; vF, vConsum, vT virtuale; `fisc{X}` partener în blocul fiscal; `–` nul; `?` nedefinit de formă.

**a. Recepție 10 buc × 100, plată, consum 4 buc**

| Postare | F1: Gestiune / Partener | F3: Repartitor | F4: Ieș→Intr / Grupare |
|---|---|---|---|
| 371 D 1000 (+10, L1) | G1 / F (fiscal) | G1, fisc{F} | F→G1 / G1 |
| 401 C 1000 (−10) | vF / F + partidă | F | F→G1 / F |
| 4426 D 210 | G1 / F (fiscal) | –, fisc{F} | F→G1 / – |
| 401 C 210 | vF / F + partidă | F | F→G1 / F |
| 401 D 1210 | ? / F + partidă | F | B→F / F |
| 5121 C 1210 | B / – | B | B→F / B |
| 371 C 400 (−4, L1) | G1 / – | G1 | G1→CC / G1 |
| 6xx D 400 (+4) | vConsum / – | CC | G1→CC / CC |

- **F1:** locul de consum se pierde, fiindcă virtuala îi ia locul. `Partener` e atribut pe 371 și coordonată pe 401. Piciorul de plată pe 401 nu are gestiune definită.
- **F3:** toate soldurile se închid. Nu se pierde nimic cât timp postarea păstrează identitatea liniei.
- **F4:** se închide. „4426 intră în G1" e fals, iar șase picioare din opt repetă antetul.

**b. Transfer 300 (3 buc, L1):** 371 D −300 (−3) și 371 D +300 (+3).
- F1: G1/– și G2/–. F3: G1 și G2. F4: G1→G2 pe ambele, grupare G1 și G2.
- Toate se închid. Ieșirea apare ca intrare în roșu în fișa gestiunii; e costul convenției de transfer, nu al formei.

**c. Compensare 500:** 401 D 500, 4111 C 500.
- F1: ?/A + partidă și ?/B + partidă; `Gestiune` nu are ce spune.
- F3: A și B.
- F4: B→A pe ambele, grupare A și B. „B→A" e mecanica debit/credit, nu un flux: nimic nu trece de la B la A.

**d. Virament 1000.**
- Ca transfer pe 5121 (D −1000 pe B1, D +1000 pe B2) se închide în orice formă, dar B1 nu are rulaj creditor. Registrul de bancă și funcțiunea contului 581 din OMFP 1802 cer 581 = 5121 și 5121 = 581.
- Recomand patru picioare: 581 D, 5121 C, 5121 D, 581 C.
- F1: –, B1, B2, – (o „gestiune" care e bancă). F3: la fel, într-o coloană. F4: B1→B2 pe toate patru, grupare doar pe 5121.
- F5: perechea lui B1 e 581, nu B2; legătura B1↔B2 rămâne la document.

**e. 628 = 401, 800.**
- F1: 628 D –/F (fiscal); 401 C –/–. Partenerul stă pe cheltuială și lipsește de pe datorie.
- F1b: 401 C –/F, se repară.
- F3: 628 pe CC sau –, cu fisc{F}; 401 pe F.
- F4: F→CC, grupare CC și F.

**f. Asamblare: 10 buc P1 (L1) de 500 → 1 buc P2 (L2) de 500.**
- F1: 371 C 500 (−10 P1) G1; contrapicior (+10 P1) vT; contrapicior (−1 P2) vT; 371 D 500 (+1 P2) G1. Dacă contrapicioarele stau pe 371, contul adună la vT +10 P1 și −1 P2 pentru totdeauna, iar orice citire de stoc trebuie să excludă virtualele.
- F3/F5: aceleași patru picioare, cu contrapicioarele pe un cont tehnic de transformare, fără repartitor, cu valoare zero pe document. Atunci perechi există. „Transformarea nu are pereche" e adevărat doar dacă scrii lot la lot și rupi conservarea pe produs.
- F4: G1→G1 pe toate, adică informație zero.

**g. Decont: bon de la S, 100 + 21; angajat E.**

| Postare | F1 | F3 | F4 |
|---|---|---|---|
| 6xx D 100 | vConsum / S (fiscal) | CC, fisc{S} | E→CC / CC |
| 4426 D 21 | – / S (fiscal) | –, fisc{S} | E→CC / – |
| 542 C 100 și C 21 | – / E + partidă | E | E→CC / E |

- F1: merge mecanic, dar aceeași coloană ține S și E în același document.
- F4: S nu are loc în nicio coloană; „partenerul din D394" citit din ieșire dă angajatul.
- F5 prin pereche: tot angajatul. Doar blocul fiscal îl ține pe S.

**h. Deschidere:** 371 D 1000 (+10, L1), 5121 D 210, 401 C 1210.
- Valoarea se închide, cantitatea nu: +10 fără contracantitate. Trebuie picioare pe 891, de exemplu 891 C 1000 (−10).
- F1: îi trebuie o virtuală care nu e pe listă.
- F3: G1, B, F, iar 891 fără repartitor.
- F4: ieșirea și intrarea sunt nule peste tot și rămâne doar gruparea, ceea ce arată că ea e singura coordonată reală.
- F5: fără pereche, sau pereche cu 891.

## 3. Partenerul fiscal

**Este atribut al faptului fiscal, scris pe postare.**

- **Nu e coordonată de sold.** 4426 și 4427 se închid lunar în 4423/4424 fără partener. D394 e o sumă de fapte pe perioadă, nu un sold. Ca dimensiune de snapshot, înmulțește cheile cu numărul de parteneri fără niciun cititor contabil.
- **Nu se deduce din document.** La decont, documentul are angajatul și fiecare linie alt furnizor. La taxarea inversă (4426 = 4427) perechea nu are niciun terț. La bonuri și facturi simplificate, CUI-ul e pe linie.
- **Storno.** Faptul se copiază cu semn opus și același partener, în perioada stornării. Cu atribut scris e o copie; cu deducție depinzi de antetul documentului de storno.
- **Corecția de partener.** −210 pe A și +210 pe B: soldul 4426 nu se mișcă, D394 se schimbă pe doi parteneri. E dovada că partenerul nu ține de sold. Mutarea partidei pe 401 de la A la B e un transfer separat.
- **Append-only.** Într-un jurnal append-only, deducția din antet ar rescrie retroactiv o declarație depusă.

## 4. Gestiunile virtuale

**Nu sunt un concept necesar.** Amestecă trei lucruri, fiecare cu un înlocuitor mai curat:

1. **Marcajul „în afara evidenței"** devine natura contului. Stocul real este cantitatea pe un cont cu natură de stoc; orice cantitate pe alt cont e contracantitate.
2. **Felul mișcării** (de la furnizor, consum, inventar) devine atribut al tipului de document sau al liniei.
3. **Purtătorul contracantității** devine piciorul pereche: 401 pe terț, 6xx pe locul de consum, contul tehnic.

Pe cele cinci virtuale:
- **Furnizor și Client:** terțul însuși, cu rând real.
- **Consum:** unitatea internă pe contul de cheltuială; recuperezi locul de consum, pe care forma 1 îl pierde.
- **Inventar:** contul de plus sau minus, fără repartitor sau cu gestiunea inventariată.
- **Transformare:** cont tehnic cu valoare zero pe document.

Condiție: natura contului trebuie versionată pe perioadă, altfel o schimbare de politică reinterpretează istoria.

## 5. Forma 4

**Gruparea trebuie scrisă.**

- În cazul simplu merge: capătul propriu e intrarea pe debit și ieșirea pe credit.
- Un debit negativ e fie storno în roșu al unei intrări (capătul propriu e intrarea), fie sursa unui transfer (capătul propriu e ieșirea). Latura și semnul sunt aceleași, răspunsul e opus, deci îți trebuie și felul mișcării.
- La valoare zero (gratuități, linii doar cantitative) semnul nu există.
- Un snapshot nu se poate chei pe o expresie peste trei coloane și politica de cont.
- Odată scrisă gruparea, ieșirea și intrarea se obțin prin pereche, adică forma 5.

**Storno și valoare negativă.**
- Ieșirea și intrarea trebuie definite ca fiind capetele mișcării originale, invariante la semn.
- La storno în negru (laturi inversate) se răstoarnă, iar regula „partenerul e ieșirea la achiziții" cade.
- G1→G2 cu −300 și G2→G1 cu +300 sunt două scrieri ale aceluiași fapt. E nevoie de o regulă canonică: valoare negativă doar cu referință la postarea stornată.

**Capăt care nu e latură a antetului.**
- 4426, 628, 665/765, rotunjirile și 4427 la taxare inversă nu au repartitor.
- Dacă scrii nul, coloanele sunt nule pe jumătate din jurnal. Dacă scrii ecoul antetului, e fals și forma 4 degenerează în forma 2.

## 6. Cont, sau cont și tip de document

**La nivel de cont analitic, cu o mulțime de feluri admise, nu cu un singur fel.** Tipul de document doar validează ce fel poate furniza; nu intră în politica soldului. Altfel, sensul unui sold ar depinde de cine l-a postat.

- **461, 462:** terț = {partener, angajat}. Dacă vrei separare, o faci prin analitice, nu prin tip de document.
- **542:** angajat, deși e în clasa 5; clasa nu dictează felul.
- **581:** niciunul. Dacă rămâne sold în tranzit, se urmărește prin unitate (viramentul).
- **4091, 419, 408, 418:** terț cu partidă. Rolul de furnizor sau client e al contului, nu al repartitorului.
- **Stocuri la terți (351, 357, 8033):** singurul caz greu. Contul declară terț, iar invariantul devine „stocul cere repartitor de felul contului, produs și lot".
- Custodia primită de la mai mulți proprietari în aceeași gestiune are două dimensiuni simultane. Se rezolvă cu proprietarul în identitatea lotului, cum partida ține partenerul, nu cu a doua coloană.

Nomenclatorul unic cu cinci feluri susține direct coloana unică: două coloane tipate peste un singur nomenclator sunt tipare redundantă.

## 7. Agregate și snapshot-uri

| Forma | Cheia de sold | Cost |
|---|---|---|
| 1, 1b | cont, gestiune, partener, unitate, produs | Două dimensiuni nulabile. Soldul corect cere ignorarea uneia după natura contului, adică forma 3 prin convenție. |
| 2 | aceeași, cu ambele pline | Cheile se înmulțesc (gestiuni × parteneri) pe 371, 4426, 6xx, fără cititor. |
| 3, 5 | cont, repartitor, unitate, produs | Minimă. D394 e un agregat separat pe faptele fiscale. |
| 4 | aceeași ca la 3 | Jurnalul crește cu două coloane și cel puțin două indexuri pe partiție. |

- **Forma 4:** la 10 milioane de postări pe an, cele două coloane înseamnă circa 160 MB brut, plus indexurile și amplificarea la scriere. Un agregat de rulaj pe perechea ieșire–intrare e un cub, nu un snapshot.
- **Forma 5:** opt octeți pe rând. Capătul celălalt e un self-join pe cheia de pereche, mereu în aceeași partiție, pentru că perechea e în aceeași tranzacție.
- **Peste tot:** dimensiunea nulă se ține ca santinelă, nu ca NULL. Unicitatea pe NULL diferă între motoare.

## 8. Schimbarea coordonatelor

**Dovezi cerute înainte:**
1. Noua coordonată se derivă determinist din coloanele vechi plus politica de cont, pentru toate rândurile istorice. Procentul de rânduri ambigue se măsoară și trebuie să fie zero sau enumerat.
2. Balanța sintetică e identică pe fiecare perioadă. Soldurile analitice, stocul pe lot și partidele deschise se reconciliază între proiecția veche și cea nouă.
3. D300 și D394 regenerate pe perioadele depuse sunt identice cu cele depuse.
4. Inventarul tuturor cititorilor coloanelor vechi.

**Se poate face aditiv:**
- Coloană nouă, scriere dublă, backfill ca proiecție derivată și versionată. Append-only protejează faptele, nu proiecțiile.
- Snapshot-uri reconstruite în spațiu de chei paralel și comparate în umbră pe câteva perioade închise, apoi comutarea cititorilor.
- Coloanele vechi nu se șterg pe perioadele închise.
- Unde datele vechi nu determină coordonata, backfill din document cu marcaj de proveniență. Dacă nici documentul nu o are, notă de corecție în perioada curentă, nu rescriere.

## 9. Contraexemple numerice

- **F1:** două facturi de servicii, F1 de 800 și F2 de 500, pe 401 fără partide. Soldul e 1300, iar pe furnizor nu există.
- **F1 și F1b:** recepție 1210 pe (vF, F) și plată 1210 pe (?, F). Dacă plata nu poartă virtuala, rămân −1210 și +1210 pe chei diferite. Dacă `Partener` intră în cheia lui 371, stocul e +1000 pe (G1, F) și −400 pe (G1, –).
- **F2:** factură pe (G1, F) cu −1210, plată pe (B, F) cu +1210; 401 nu se închide niciodată pe cheie. La transferul G1→G2 nu e clar ce partener poartă piciorul.
- **F3:** nu am găsit un contraexemplu de sold. Limita e custodia: 10 buc ale lui Y și 5 ale lui Z în G1 dau 15 pe G1 fără proprietar, dacă lotul nu îl ține.
- **F4:** la decont, 21 TVA raportat în D394 pe angajat. La 4426 = 4427 de 210, partener inexistent.
- **F5:** aceleași două cazuri dacă partenerul fiscal se citește prin pereche; niciunul dacă stă în blocul fiscal.

## Fapte care mi-ar schimba verdictul

1. Postarea are deja (document, linie, componentă) stabile: 5 se reduce la 3 și cheia e inutilă.
2. Rapoartele dominante filtrează pe capătul celălalt, iar self-join-ul măsurat pe partiții e prea lent: denormalizarea din forma 4 se justifică, dar ca proiecție de citire, nu în jurnal.
3. Există conturi cu două dimensiuni de sold independente pe care unitatea nu le poate absorbi: coloana unică nu ajunge.
4. Forma de azi e 1 sau 1b, cu ani de declarații depuse citite din `Partener`: riscul migrării poate depăși câștigul, iar 1b cu regulă de cheie pe natura contului devine acceptabilă.
5. Stornoul se face în negru: argumentele despre intrare și ieșire se înăspresc, restul rămâne.
6. Rapoartele de stoc folosesc virtualele drept clasificator de mișcare și nu există alt atribut: eliminarea lor cere întâi acel atribut.
7. Politica de cont s-a schimbat în timp fără versionare: tiparea pe cont nu se poate aplica retroactiv.
8. Frecvența reală a faptelor fiscale fără picior de terț (taxare inversă, deconturi, bonuri): dacă e aproape zero, citirea prin pereche e tolerabilă, deși tot greșită.

## Încredere

**4 din 5** pe direcție: o singură coordonată de sold tipată de cont, partener fiscal ca atribut, fără virtuale, forma 2 respinsă. **3 din 5** pe marginea dintre 3, 5 și 4, care depinde de profilul de citire și de identitatea liniei, pe care nu le văd.
