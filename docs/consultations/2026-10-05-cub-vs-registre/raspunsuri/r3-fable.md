# Runda 3: critica încrucișată (analistul Y)

**Rămân la A, cu TR-D9a amendat și fără prototip B.** Numai citire, nimic rulat. Prescurtări: `M/` și `N/` ca în runda 2; `K` = contractul TR-D9a; `TX` = `docs/nucleu/tr-d8-transversal-contract.md`; `P` = `docs/api/p5-perf-masuratori.md`; `RX` = `docs/consultare/raport-X.md`; `NTC` = `docs/nucleu/scenarii/NTC.md`.

## 1. Cele mai slabe trei afirmații ale lui

**1. „Aș refuza implicit nota nenominalizată pe conturi controlate” (`RX:120`, amendamentul 3 la `RX:146`).**
- Recensământul pe Flax numără 7.816 picioare pe conturi de terț fără repartitor și 3.365 pe 3xx fără lot (`NTC:73-77`). SC-NTC-14 vine chiar din ele (`NTC:41`).
- Conturile bugetare nu au `RolTert` (`NTC:12-13`).
- Pentru partide există deja o regulă calibrată: poziția fără unitate e lipsă numai cu partener sau cu latură externă pe document (`M/Cub/Citiri/Partide.cs:198-204`).
- Refuzul implicit ar respinge forme prezente de mii de ori în datele reale. Alternativa din aceeași frază a lui, excepția într-o evidență reconciliabilă, e cea corectă.

**2. „B ≥ C > A” la optimizări și „raportul nu mai interpretează contraponderi, originea stornoului” (`RX:17`, `RX:97`).**
- Interpretarea nu stă în rapoarte. Felul tranzacției și originea stornoului se filtrează o dată, în `Contabil.Postari` (`M/Cub/Citiri/Contabil.cs:33-46`).
- Contraponderea e un predicat aplicat în cinci intrări de domeniu (`Contabil.cs:34`, `Imobilizari.Conturi.cs:20`, `Invarianti.cs:96`, `Partide.cs:178, 198`).
- Singura materializare măsurată e mai lentă decât recitirea la volumul scenei: 12,9 ms contra 8,1 ms (`P:1204-1207`). Rapoartele API o ocolesc din cauza securității pe rând (`TX:714-716`). [judecată] Un registru derivat ar moșteni aceeași problemă.
- Are dreptate într-un punct pe care l-am subevaluat: citirea vizibilă are „cost care crește cu istoricul” (`P:1207-1209`).

**3. Comparația „A/B pe aceeași populație mare, cu rapoarte complete” în 2 zile, plus prototip B în 2–3 (`RX:148`, `RX:151`).**
- Nu există bază de volum reală (`TX:201`).
- Nicio interogare pe registre în `M/Proiectii`, `M/Saft` sau `WebApi` (grep `GetObjectsQuery<Registru…>` azi; `docs/dosar.md:31`).
- Calea registrelor a pierdut deja acoperire: ASM pe conturi diferite e „cub 40, registre 0” (`TX:783`).
- Termenul de comparație ar trebui reînviat din istoric, nu măsurat.
- [judecată] Prețul derivării sincrone se află fără prototip. Cifrele de scriere de azi sunt în regim dual (`dosar.md:28-29`; FCT 78 de comenzi SQL, 38,9 ms, `P:1122`), iar pasul 8 rulează oricum `PerfCub` (`K:667`). Diferența înainte și după pasul 6 este costul unei a doua materializări sincrone.

## 2. Ce a văzut el și mi-a scăpat

- **Versiunea politicii e text fix:** `new VersiunePolitica("seed", doc.DataInregistrare)` (`M/Motor/Fapte.cs:144`), singura valoare scrisă în produs (grep). „Audit: A > C, întărit” din raportul meu era prea tare.
- **„Numai cubul” nu e literal:** fișa imobilizării ia metoda, durata și categoria din liniile documentelor (`M/Cub/Citiri/Imobilizari.cs:56-70`).
- **Corecția de preț fără diferență de cantitate nu postează nimic:** `q == 0` dă `v = 0` și linia e sărită (`M/Declaratii/DeclarantNir.Diferenta.cs:43-45, 60`).
- **`PosteazaInCub` e deja ireversibil** pe tipul cu tranzacții (`M/Motor/GardianEditare.cs:1043-1045`).
- **Snapshot-ul de stoc crește cu tot ce s-a consumat vreodată** (`P:1225-1230`), iar dry-run-ul notei stingătoare crește cu istoricul partenerului (`P:1218-1219`).
- **ALOP are logică, nu doar tabele:**
  - trei plafoane (angajament, trimestrial, anual) și depășire permisă prin parametru (`legacy/Buget/AlopLichidare.pas:659-680`);
  - avertisment sau refuz la angajare (`legacy/Buget/AlopAngajamente.pas:506-519`);
  - „al patrulea gard” din raportul meu e deci o politică de avertizare, nu un refuz de sold negativ.
- **Cheia de pereche nu acoperă ASM:** transformarea produce postare reală plus contrapondere, nu debit–credit (`N/Motor/Motor.cs:20-24`).

## 3. Dezacordurile și ce le-ar tranșa

| Dezacord | Dovada care îl tranșează | Fără prototip? |
|---|---|---|
| Optimizări: el B ≥ C > A; eu A > C, B nejustificat | Scara sintetică la 5–10 milioane de postări, cu ruta securizată `Vizibila` măsurată separat și prag fixat dinainte de owner. Dacă pică, câștigă un read model cu cheie de vizibilitate, adică B selectiv. | Da, `PerfCub` există. |
| Prototip B înaintea ștergerii | Ce anume închide ștergerea pentru B. Pe cod, nimic: forma pe perechi nu e derivabilă (`M/Cub/Postare.cs:8-65`), iar cheia `TipStoc` dispare oricum (`K:490`). B se poate construi din cub și după tăiere. | Da, e argument de cod. |
| Portița: el refuz implicit; eu rest legitim, cu raport și gard | Clasificarea celor 7.816 + 3.365 de picioare: operații legitime sau erori de culegere. | Da, interogare read-only pe clona Flax. |
| Cheia de pereche: el o condiționează de owner; eu o cer acum | Câte tranzacții reale dau `ContrapartidaId` NULL (`M/Proiectii/ContabilProiectii.cs:486-487`) și dacă registrul-jurnal legal cere conturi corespondente. | Da, numărătoare plus o întrebare de reglementare. |
| Simplitate: el A > B > C; eu A ≈ C | Același tip scris pe ambele căi. Nu mai contează: el estimează întoarcerea la 20–35 de zile (`RX:101`), eu la 4–6 săptămâni. | Nu. |
| „Toate cele patru defecte sunt reparabile pe registre” | [judecată] Defectul partidei cere un registru cu identitate proprie și alocări FIFO, adică jumătate din cub. E dezacord de etichetă. | Nu merită efortul. |

## 4. Recomandarea

**Nu o schimb.** Din cauza lui adaug versiunea politicii, restrâng cheia de pereche și cer ruta securizată în măsurătoare. Amendamentele, în ordinea priorității:

1. **Cheie de pereche pentru mișcări și mutări:** un ordinal în tranzacție, scris în `N/Motor/Motor.cs:10-19`. Transformarea rămâne grup. Se face înaintea pasului 7, cât schema se resetează (`K:666`).
2. **Înaintea pasului 6** (`K:665`), trei lucruri:
   - ultima reconciliere registre ↔ cub pe clona Flax, arhivată;
   - `PerfCub` în regim dual, ca bază de comparație;
   - scara de volum, cu ruta securizată.
3. **Portița rescrisă cum e:** sold cont = Σ unități + poziția fără unitate, cu raport și cu gard de semn pe 3xx fără lot. Fără refuz implicit.
4. **I2 și I5 în această felie.** I5 prin refuz la editare, nu ca restanță „după PoC” (`docs/nucleu/tr-d9-inventar.md:26`).
5. **`ReatribuieInversaFiscala`** (`M/Cub/Materializare.cs:213-222`): rând nou sau excepție declarată.
6. **Versiunea politicii:** identificator real al regulilor consumate sau limită declarată în decizia 110. [judecată] E ieftin acum, cât formatul explicației e la versiunea 1 (`M/Cub/Explicatie.cs:30`).
7. **Limite declarate:** corecția de preț după consum, custodia și ALOP rămân în afara feliei.

## 5. Un minut

Tăiați registrele. Pe cod nu am găsit nimic care să justifice întoarcerea sau un al doilea model de citire, iar celălalt analist a ajuns la aceeași direcție venind dinspre C. Tăierea șterge însă două lucruri care nu se mai recuperează ieftin: martorul intern pentru diferența de 585 de mii pe 3xx (`docs/nucleu/nucleu-bilant.md:51`) și ocazia de a pune aproape gratuit o cheie de pereche pe postare. Rezolvați-le înaintea pașilor 6, respectiv 7. Nu vă bazați pe formularea „cubul face imposibilă diferența cont–stoc”: nota manuală o reintroduce, pe date reale, de mii de ori. Cereți un raport al poziției fără unitate, nu un refuz. Nu aveți nicio cifră de volum: 7.951 de postări nu spun nimic despre zece ani. Două-trei zile de scară sintetică, cu ruta securizată a API-ului, decid dacă veți avea vreodată nevoie de B. Explicația de audit înregistrează azi versiunea politicii ca text fix, „seed”: faceți-o reală sau nu o mai invocați.

---
*În afara raportului:* conectorii claude.ai Gmail, Google Calendar, Google Drive și dxdocs cer autorizare din setările de conectori claude.ai și nu sunt disponibili până atunci; nu au fost necesari aici. CodeGraph are versiunea 1.6.2 disponibilă (`codegraph upgrade`).
