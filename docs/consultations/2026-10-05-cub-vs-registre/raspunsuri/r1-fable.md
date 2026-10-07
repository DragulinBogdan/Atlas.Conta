# Runda 1: forma registrului — judecată din principii

Legendă: **[P]** = principiu, **[S]** = presupunere. „Cub" = forma C, „registre" = forma R; opțiunile rămân A–D. B scrie ca A plus proiecția și D scrie ca A plus C, deci în coloanele lor notez doar ce se adaugă sau se rupe.

## 1. Cazuri × opțiuni

| Caz | A (numai cub) | B (cub + registre derivate) | C (numai registre) | D |
|---|---|---|---|---|
| **1. Factură cu marfă** | O tranzacție: D 371 pe lot nou (unitate, gestiune, produs, cantitate, valoare), D 4426, C 401 pe partidă nouă, plus postări în cartea fiscală. Stocul este soldul contului, iar restul este suma pe partidă, prin construcție [P]. Se poate rupe: taxa din cartea fiscală ≠ rulajul 4426, fiindcă sunt două postări [P]. | Proiecția emite un rând de stoc, două note-pereche și un rând de TVA. Cere ca postarea să poarte corespondența D=C, altfel perechile nu sunt deterministe [P]. | Planul scrie rând de stoc, notele 371=401 și 4426=401, rând de TVA și restul pe antet. Consecvența ține de corectitudinea planului și de acordul dintre regula de stoc și regula de contare [P]. Se pot rupe: stoc ≠ sold 371, rest ≠ sold 401, registru TVA ≠ 4426 [P]. | Orice divergență rămâne fără arbitru [P]. |
| **2. Recepție ulterioară cu diferență** | Postare pe lotul existent: cantitate, sau numai valoare pentru preț. Cota cantității deja consumate merge pe cheltuială, calculată din soldul lotului [P]. Cubul nu are coordonată pentru cauză, deci ea stă în felul tranzacției sau în deciziile persistate [S]. | Cauza trebuie să fie în cub, nu doar în decizii, ca să ajungă în registrul de stoc [P]. | Rând de stoc numai-valoare, marcat cu cauza, plus notă. Împărțirea consumat/rămas citește registrul de stoc. Două rânduri de ținut de acord [P]. | Cauza se scrie de două ori. |
| **3. Asamblare** | O tranzacție: credit pe loturile componentelor, debit pe lotul nou. Balansarea garantează conservarea doar la monografie directă (3xx=3xx). Prin 601/711 trebuie invariant explicit: suma valorii pe unitățile de stoc din tranzacție = 0 [P]. | Nimic nou. | Ieșiri per lot, intrare și note. Conservarea e proprietate a planului, verificabilă doar peste două registre [P]. | Două calcule de cost care pot diferi la rotunjire [S]. |
| **4. Notă manuală** | (a) Două postări, fără cost. (b) Profilul contului cere partener și unitate, deci nota alege sau deschide o partidă; prețul e un UI de alegere [P]. (c) Postare numai-valoare pe lot = reevaluarea lotului, coerentă [P]. Rămân posibile: valoare pe lot cu cantitate zero și 4426 fără carte fiscală. | Proiecția trebuie să fie totală peste orice postare validă, nu doar peste tiparele declaranților. Aici se rupe B în practică [S]. | (a) Un rând. (b) Nota mișcă soldul 401, dar nu restul și împerecherile; altfel devine document cu partidă, adică cod în plus [P]. (c) Sold 371 ≠ registru de stoc, ireparabil. Singura apărare e interdicția, deci portița se închide unde e nevoie [P]. | Nota se scrie de două ori sau într-o singură formă. |
| **5. Storno în lună închisă** | Tranzacție nouă în perioada deschisă, cu postări inverse pe aceleași unități. Dependenții sunt postările cu altă proveniență pe unitățile documentului: un singur mecanism [P]. Lot consumat înseamnă sold negativ, deci refuz cu lista dependenților. Stingerea se mută întâi într-un avans prin transfer [S]. | Rânduri inverse derivate. | Rând invers în patru registre, plus solduri, rest și împerecheri. Dependenții se caută prin două mecanisme [P]. Restul de pe antet rescrie un document operat și contrazice propria constrângere [P]. | Două stornouri independente. |
| **6. Citiri grele** | Indecși: (cont, partener, dată), (unitate), (document), parțial pe cartea fiscală (perioadă TVA, cod), parțial pe stoc (gestiune, produs, dată). Solduri de perioadă pe cont×analitic și pe unitate, plus un tabel de sold curent al unităților deschise, obligatoriu pentru FIFO [P]. La dată = sold de perioadă + delta. Rămân deschise: partiționare pe an, materializare la închidere. SAF-T iese nativ pe linii [P]. | Indecși proprii pe registre, scriere dublă. Cache-ul perioadelor închise nu se invalidează niciodată [P]. | Registrul contabil cere doi indecși și UNION la orice citire de cont, cu dimensiuni pe coloane diferite [P]. Restul rapoartelor sunt directe, iar perechile se despică trivial pentru SAF-T. | Două seturi de indecși. |
| **7. Custodie** | Regulă: un rând de politică, postare numai-cantitate pe 8033, pe lot, cu partenerul proprietar. Notă: aceeași postare scrisă de mână, cu evidență cantitativă completă [P]. Cere partidă simplă în clasa 8, fiindcă balansarea la valoare zero e vidă [P]. | Proiecția trebuie să cunoască tipul de stoc „custodie". | Regulă: tip de stoc „custodie", natural. Notă: registrul contabil nu are cantitate, deci portița nu acoperă cazul [P]. | — |
| **8. ALOP** | Nicio tabelă nouă [S]. Creditul și angajamentul sunt unități, faza e transfer din unitatea precedentă, depășirea e sold negativ și refuz [P]. Nou: declaranți, tipuri de unitate, legătura unitate→părinte, partidă simplă pe 806x. | Un registru ALOP proiectat; dacă se citește direct din cub, B nu mai e B. | Registru nou, solduri, împerecheri între faze, reguli și pas de storno. Fiecare concept „cu rest" costă o tabelă [P]. | De două ori. |

## 2. Clasamente

1. **Coerență: A > B > C > D.**
   - A face imposibile stoc≠cont, rest≠cont și imobilizare≠cont [P]. Rămân: carte fiscală≠4426, metadatele unității și tabelul de solduri curente, care e derivat și reconstruibil.
   - B adaugă driftul proiecției, reparabil.
   - C are patru perechi de adevăruri paralele, apărate doar de plan, plus două rânduri de politică pentru același fapt [P].
   - D nu are arbitru.
2. **Simplitate: A > C > B > D.**
   - A are un scriitor, un storno și un mecanism de sold. Costul: obligativitatea coordonatelor devine cod, nu schemă [P].
   - C are piese simple, dar multiplicate.
   - B este A plus proiecție, reconstrucție și detecție de drift.
   - A câștigă doar dacă „unitatea" nu curge: dacă ajunge cu câmpuri nule pe tip și ramificări pe tip, simplitatea e fictivă [P].
3. **Reprezentarea tipului: A = B > C = D, cu rezervă.** Diferența vine mai ales din stilul motorului, nu din formă (vezi §6). Forma contează într-un singur loc: în registre un fapt cere regulă de stoc, regulă de contare și suprascriere, deci cititorul vede fragmente [P].
4. **Portița: A > B > C > D.** În A nota trece prin același validator ca documentele. În C portița e fie închisă, fie generatoare de divergență [P].
5. **Optimizări ulterioare: A = B > C > D.** B este ceea ce devine A când e optimizat; orice accelerare e derivare dintr-un singur adevăr [P]. C rămâne cu forma-pereche și indexarea ei dublă.
6. **Audit: A > B > C > D.** În A orice cifră e sumă de postări cu proveniență, cu un singur mecanism de coborâre. B adaugă un salt, dar are arbitru. În C diferența dintre două registre nu are explicație în model [P].
7. **Reversibilitate: B > A > D > C.**
   - Din cub se refac registrele dacă există corespondența și cauza [P].
   - Din registre nu se refac ieftin unitățile, legătura rând de stoc ↔ notă și deciziile motorului [P].
   - Politica e editabilă, deci postările nu se pot regenera din documente în nicio opțiune [P]. „Reconstruibil" înseamnă doar derivat din postări.

## 3. Cel mai puternic argument pentru fiecare

- **A.** Un fapt, un rând. Invariantele sunt sume, reconcilierea dispare ca activitate, iar extensiile cu rest (ALOP) vin prin declarație. Precedent: SAP a unificat jurnalul tocmai ca să elimine reconcilierea între module [S].
- **B.** Izolează rapoartele și SAF-T de evoluția cubului, în tabele înguste și tipate. Dacă există deja cod de raportare pe registre, e calea cea mai ieftină de migrare, iar reconstrucția repară retroactiv erorile de proiecție [S].
- **C.** Schema e specificația: coloane tipate, NOT NULL și FK per registru, corespondență directă cu registrele legale și forma D=C nativă contabilului român. Pentru 20 de tipuri stabile, generalitatea cubului se plătește fără garanția că e folosită [P].
- **D.** E singura opțiune care poate detecta o eroare a cubului. O proiecție moștenește greșelile sursei; o a doua scriere independentă nu [P]. Valoros ca oracol diferențial în teste și în migrare, nu în producție.

## 4. Recomandare

**A ca model, cu registrele expuse ca vederi SQL nematerializate, drept contract de citire. B nu se construiește acum.**

Materializarea se adaugă per vedere, numai la închiderea perioadei și numai unde o cere măsurătoarea. Perioada închisă e imuabilă, deci cache-ul ei nu se invalidează; perioada deschisă se citește viu din cub plus solduri [P]. Dacă mi se impune B, aleg același regim: la cerere pe perioada deschisă, materializat la închidere. Derivarea asincronă strică citirea propriei scrieri, iar cea sincronă leagă operarea de bug-urile proiecției [P].

Resping C din cauza criteriului 4: portița e cerință, iar în registre ea e exact locul unde modelul se rupe. Resping D ca stare permanentă.

Recomandarea ține dacă:

1. postarea poartă corespondența (cheie de pereche sau grup), ca jurnalul D=C și contul corespondent din fișă să fie derivabile;
2. profilul contului declară coordonatele obligatorii și e aplicat tuturor scriitorilor, inclusiv notei manuale;
3. metadatele unității sunt tipate în sateliți, iar motorul nu ramifică pe tipul unității;
4. soldul curent al unităților e tranzacțional, cu blocare în ordine deterministă și verificarea ipotezelor citite la commit;
5. semantica pe carte e definită: balansare per carte, partidă simplă în clasa 8, baza și taxa reprezentate explicit;
6. deciziile și ipotezele se persistă;
7. citirile grele își ating țintele pe volumul real.

## 5. Ce mi-ar schimba verdictul (lista pentru runda a doua)

1. Are postarea cheie de corespondență? Se reface jurnalul în perechi pentru tranzacții N:M? Dacă nu, A nu servește rapoartele românești, iar un B care primește perechile de la motor e D deghizat.
2. Proiecția cub→registre citește ceva în afara cubului și a datelor imuabile ale documentului (politici, de exemplu)? Dacă da, nu e reconstruibilă.
3. Câte ramificări pe tipul unității și pe carte există în motor și în interogări? Câte coordonate sunt nule în medie? Câte CHECK-uri apără cubul?
4. Măsurători pe volum real: p95 pentru fișa de terț, balanța la dată, stocul la dată, jurnalul de TVA și SAF-T lunar; timpul de operare per document; numărul de indecși și mărimea lor.
5. Rapoartele scrise pe cub repetă fiecare definiția „ce e o mișcare de stoc"? Dacă da, lipsește stratul de vederi.
6. Test: notă manuală pe 401 fără unitate și pe 371 fără lot. Trece? Ce invariante cad după?
7. Test: storno pe un document din lună închisă, cu lot consumat și factură stinsă. Refuz, cascadă sau corupere?
8. Test: două bonuri de consum concurente pe același lot.
9. Se verifică taxa din cartea fiscală contra rulajului 4426/4427 pe perioadă? D394 iese fără join-uri euristice?
10. În ce formă stau azi datele de producție și cât costă migrarea istoricului?
11. Pe date reale din registre: câte divergențe stoc↔371 și rest↔401 există? Dacă sunt zero după ani de note manuale, slăbiciunea lui C e teoretică și verdictul se înmoaie.
12. Câte linii au motorul și un declarant tipic, față de plan și suprascrieri? Cât din suită rulează fără bază?
13. Un document antedatat în perioada deschisă schimbă ordinea FIFO deja consumată?
14. Poate o unitate să refere o unitate-părinte? Cele șase dimensiuni acoperă clasificația bugetară completă?

## 6. Ce lipsește sau e greșit pus

- **Trei decizii sunt împachetate ca una**: forma stocării, puritatea motorului și declarant versus polimorfism. Un motor pur cu declaranți poate emite registre la fel de bine. Întrebarea riscă să crediteze cubul cu meritele funcției pure, iar criteriul 3 judecă mai ales stilul motorului [P].
- **Două opțiuni nelistate.**
  - E: cub autoritar, registre ca vederi, materializare selectivă la închidere. E recomandarea mea.
  - F: registre autoritare, cub ca vedere (UNION al laturilor). E ieftină și e termenul de referință corect în locul lui D. O resping pentru că nu are unități și nici legătura stoc↔notă [P].
- **Reproductibilitatea nu e pusă.** Cu politică editabilă fără versiuni, postările sunt singurul adevăr și nu se pot reface din documente. Asta mută greutatea criteriului 7 pe persistarea deciziilor.
- **Corespondența conturilor nu e criteriu**, deși e singurul loc unde registrele au un avantaj structural asupra cubului.
- **Lipsesc cazuri de probă**: antedatarea față de FIFO, stocul negativ, concurența pe lot, reevaluarea valutară a partidelor, rotunjirea la repartizare, migrarea istoricului.
- **„Balansat" e nedefinit** pentru cantitate, pentru cartea fiscală și pentru clasa 8. Cazurile 7 și 8 depind exact de asta.
- **Cache-ul e supralicitat.** Sute de mii de documente înseamnă 10–30 de milioane de postări pe an [S], volum mic pentru PostgreSQL cu solduri de perioadă. Viteza probabil nu decide; decide coerența după nota manuală.
- **Denumirile încurcă**: „opțiunea C" este „forma R". Redenumiți înainte de runda a doua.

---
Notă de mediu: conectorii claude.ai Gmail, Google Calendar, Google Drive și dxdocs cer autorizare din setările de conectori claude.ai și sunt indisponibili până atunci. Nu au fost necesari aici.
