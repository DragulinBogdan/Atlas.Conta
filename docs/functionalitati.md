# Catalogul funcționalităților

- Tip: catalog
- Actualizat: 2026-10-15

Ce promite produsul, în unitatea în care se măsoară scopul: o
funcționalitate e ce ar numi utilizatorul, cu ținta pe care o servește,
aria care o descrie, etapa și scenariile care o probează. Regula
catalogului e în kitul de guvernanță, §3.9; țintele, structura și etapa
curentă sunt în [scop](scop.md).

Rândurile inițiale sunt derivate din
[acoperirea funcțională](stare-curenta/README.md#acoperirea-funcțională),
[limitele curente](stare-curenta/limite-curente.md) și
[familiile de scenarii](nucleu/scenarii/README.md), propuse de agent și
acceptate de owner la 2026-10-14. Un rând `livrată` de dinaintea
catalogului poartă data adoptării (linia zero), nu data livrării. Rândurile
`deschisă` fără etapă vin din limite și sunt parcate; unul care devine
non-țintă trece în `retrasă prin N`.

| ID | Funcționalitate | Ținte | Arie | Etapă | Stare | Scenarii |
|---|---|---|---|---|---|---|
| F-CVL-01 | Corectez valoarea unui lot recepționat prin factura furnizorului, cu o linie de corecție pe factură; diferența se împarte între stocul rămas și consumul deja făcut, iar ieșirile ulterioare se evaluează din soldul corectat | T1 | stoc | E1 | deschisă | SC-CVL-01…07, SC-CVL-18, SC-CVL-19, SC-CVL-21, SC-CVL-25 |
| F-CVL-02 | Retrag, stornez sau corectez o corecție valorică; inversarea e refuzată cât există o ieșire activă făcută după ea | T1 | stoc | E1 | deschisă | SC-CVL-08, SC-CVL-09, SC-CVL-13, SC-CVL-14, SC-CVL-17, SC-CVL-27 |
| F-CVL-03 | Stornez un consum făcut înaintea corecției, iar partea lui din corecție revine în stoc o singură dată | T1 | stoc | E1 | deschisă | SC-CVL-10…12, SC-CVL-22, SC-CVL-26, SC-CVL-28…30 |
| F-CVL-04 | Corecția valorică intră în luna ei, iar soldurile unei luni închise rămân neschimbate | T1 | stoc | E1 | deschisă | SC-CVL-15, SC-CVL-16, SC-CVL-23 |
| F-CVL-05 | Văd efectul corecției în fișa de cont, balanță și soldul stocului, cu explicația împărțirii pe stoc și consum | T1, T3 | stoc | E1 | deschisă | SC-CVL-20, SC-CVL-31 |
| F-CVL-06 | Sting prin plată datoria născută din corecție, ca partidă proprie a corecției | T1 | stoc | E1 | deschisă | SC-CVL-24 |
| F-CVL-07 | Corectez valoarea unui lot care a trecut prin transfer, retur, asamblare, inventar sau deschidere | T1 | stoc | | deschisă | — |
| F-CVL-08 | Corectez valoarea unui lot printr-o factură cu TVA sau printr-o factură mixtă | T1, T2 | stoc | | deschisă | — |
| F-FCT-01 | Înregistrez factura furnizorului pentru stoc sau servicii, cu TVA pe mai multe cote, taxare inversă sau TVA capitalizat, și o anulez, o stornez sau o corectez | T1 | stoc | | livrată 2026-10-14 | SC-FCT-01…08, SC-FCT-10, SC-FCT-11, SC-X-01 |
| F-FCT-02 | Înregistrez factura furnizorului pe clasificația bugetară, cu angajament și cod economic | T5 | stoc | | livrată 2026-10-14 | SC-FCT-12…14 |
| F-NIR-01 | Recepționez marfa facturată și înregistrez diferențele de recepție pe cauze: lipsă, plus, imputabilă, pe drum | T1 | stoc | | livrată 2026-10-14 | SC-NIR-09, SC-NIR-18…25, SC-NIR-28, SC-X-02 |
| F-NIR-02 | Recepționez marfă fără factură și inversez recepția | T1 | stoc | | livrată 2026-10-14 | SC-NIR-01…07, SC-NIR-11 |
| F-BCS-01 | Dau în consum din stoc prin bon de consum, evaluat FIFO pe lot | T1 | stoc | | livrată 2026-10-14 | SC-BCS-01…07, SC-BCS-09…11 |
| F-BTR-01 | Transfer stocul între gestiuni, cu lotul și prețul lui | T1 | stoc | | livrată 2026-10-14 | SC-BTR-01…08, SC-X-08 |
| F-DSC-01 | Descarc gestiunea pentru marfa vândută, din factura de vânzare | T1 | stoc | | livrată 2026-10-14 | SC-DSC-01…08, SC-FCL-10 |
| F-LDI-01 | Înregistrez plusurile și minusurile de inventar pe lot | T1 | stoc | | livrată 2026-10-14 | SC-LDI-01…12, SC-LDI-17 |
| F-LDI-02 | Urmăresc obiectele de inventar în folosință pe gestiunea reală, de la recepție la consum | T1, T5 | stoc | | livrată 2026-10-14 | SC-LDI-18, SC-LDI-19, SC-LDI-21, SC-NIR-15 |
| F-ASM-01 | Asamblez componente din stoc într-un produs nou | T1 | stoc | | livrată 2026-10-14 | SC-ASM-01…11, SC-X-09 |
| F-RLF-01 | Returnez marfă la furnizor, cu TVA și cu partida furnizorului | T1 | stoc | | livrată 2026-10-14 | SC-RLF-01…10 |
| F-RDC-01 | Primesc retur de la client, cu venitul și costul inversate | T1 | stoc | | livrată 2026-10-14 | SC-RDC-01…12, SC-RDC-14, SC-RDC-15 |
| F-DVI-01 | Înregistrez declarația vamală de import, cu TVA-ul din vamă | T1, T2 | stoc | | livrată 2026-10-14 | SC-DVI-01…09, SC-DVI-11, SC-DVI-13…17 |
| F-DVI-02 | Includ taxa vamală în costul loturilor din facturile legate | T1 | stoc | | deschisă | SC-X-13 |
| F-FCL-01 | Emit factura către client, cu TVA pe mai multe cote rotunjit pe document și cotă, și o anulez, o stornez sau o corectez | T1, T2 | partide | | livrată 2026-10-14 | SC-FCL-01…08, SC-FCL-11…14 |
| F-PLT-01 | Plătesc furnizorii și alți creditori din casă sau bancă | T1 | partide | | livrată 2026-10-14 | SC-PLT-01…09 |
| F-INC-01 | Încasez de la clienți în casă sau bancă | T1 | partide | | livrată 2026-10-14 | SC-INC-01…07, SC-INC-09 |
| F-VIR-01 | Mut bani între conturile proprii, de exemplu din bancă în casă | T1 | partide | | livrată 2026-10-14 | SC-C111-21, SC-SAFT-29 |
| F-PRT-01 | Sting o factură prin plată, încasare sau notă, parțial sau integral, și văd restul pe document | T1 | partide | | livrată 2026-10-14 | SC-X-04, SC-X-18, SC-FCL-09, SC-D9B-01…03b, SC-D9B-06…09 |
| F-PRT-02 | Desfac o stingere și retrag documentul care stingea | T1 | partide | | livrată 2026-10-14 | SC-D9B-04, SC-D9B-05, SC-NIR-08 |
| F-PRT-03 | Văd partidele deschise pe scadențe și pe vechime | T1 | partide | | deschisă | — |
| F-NTC-01 | Înregistrez o notă contabilă și o anulez, o stornez sau o corectez | T1 | partide | | livrată 2026-10-14 | SC-NTC-01…06, SC-NTC-10…19 |
| F-NTC-02 | Compensez prin notă contabilă datorii și creanțe, stinse FIFO pe partidele partenerului | T1 | partide | | livrată 2026-10-14 | SC-NTC-07…09, SC-RLF-06, SC-RDC-09 |
| F-NTC-03 | Aleg pe linia notei partida pe care o sting | T1 | partide | | deschisă | SC-NTC-23…26 |
| F-DEC-01 | Înregistrez decontul angajatului, cu TVA, avans și restituirea restului | T1 | partide | | livrată 2026-10-14 | SC-DEC-01…12, SC-DEC-14, SC-DEC-16, SC-DEC-17 |
| F-DES-01 | Încarc soldurile de pornire: conturi, stoc pe lot și terți pe partidă, apoi le sting cu documente noi | T4 | partide | | livrată 2026-10-14 | SC-DES-01…09, SC-DES-11…14 |
| F-IMO-01 | Pun în funcțiune o imobilizare din factură, notă sau deschidere, cu modernizare și revizuire | T1 | imobilizari | | livrată 2026-10-14 | SC-IMO-01…06, SC-IMO-10, SC-IMO-11, SC-IMO-26, SC-IMO-27 |
| F-IMO-02 | Generez amortizarea lunară contabilă și fiscală, liniară, accelerată sau degresivă, cu plafonul de deductibilitate | T1, T2 | imobilizari | | livrată 2026-10-14 | SC-IMO-07…09, SC-IMO-19, SC-IMO-23…25 |
| F-IMO-03 | Scot din funcțiune o imobilizare, cu valoarea rămasă pe ambele cărți | T1 | imobilizari | | livrată 2026-10-14 | SC-IMO-12, SC-IMO-13 |
| F-IMO-04 | Țin evidența investițiilor în curs, a reevaluării, a leasingului și a deprecierii imobilizărilor | T1 | imobilizari | | deschisă | — |
| F-PER-01 | Închid luna cu constatările ei și soldurile fixate; un document întârziat intră în luna deschisă | T1 | perioade | | livrată 2026-10-14 | SC-X-21, SC-BCS-04, SC-CIT-12…14 |
| F-PER-02 | Închid anul: rezultatul trece în 1174, iar anul nou se deschide din soldurile de la 31.12 | T1 | perioade | | deschisă | SC-X-12 |
| F-ITV-01 | Închid lunar TVA-ul: generez, regenerez și inversez nota de regularizare | T1, T2 | tva | | livrată 2026-10-14 | SC-ITV-01…11 |
| F-REGIM-01 | Văd pe fiecare document ce pot face cu el acum și de ce, după stare, stingeri și perioadă | T1 | documente | | livrată 2026-10-14 | SC-REGIM-01…15 |
| F-X-01 | Lucrez în același timp cu alți operatori fără stoc negativ, dublă stingere sau lună închisă peste un document operat | T1 | motor | | livrată 2026-10-14 | SC-X-15…23 |
| F-CIT-01 | Citesc balanța de verificare și balanța pe planul de conturi, la orice dată | T1 | raportare | | livrată 2026-10-14 | SC-CIT-11, SC-CIT-26…30 |
| F-CIT-02 | Citesc fișa de cont și registrul-jurnal, cu sold inițial, rulaje și contrapartidă | T1, T3 | raportare | | livrată 2026-10-14 | SC-CIT-16…22, SC-C111-31, SC-C111-32 |
| F-CIT-03 | Citesc soldul stocului pe gestiune și pe lot, la o dată | T1 | raportare | | livrată 2026-10-14 | SC-CIT-31, SC-CIT-32, SC-CIT-35…40 |
| F-CIT-04 | Citesc soldul pe parteneri și documentele cu rest | T1 | raportare | | livrată 2026-10-14 | SC-C111-10, SC-CIT-33 |
| F-CIT-05 | Aflu de ce o postare are contul și suma ei: regula de politică care a decis | T3 | raportare | | livrată 2026-10-14 | SC-CIT-96…99 |
| F-CIT-06 | Citesc jurnalele de TVA, decontul și D300, D394, cu corecțiile pe luna lor | T2 | tva | | livrată 2026-10-14 | SC-CIT-79…87 |
| F-SAFT-01 | Generez SAF-T lunar (L): registrul general, facturi și plăți, cu sumar, avertismente și XML validat | T2 | saft | | livrată 2026-10-14 | SC-SAFT-01…06, SC-SAFT-16…35 |
| F-SAFT-02 | Generez SAF-T de stocuri (S) pe luni, coerent cu L | T2 | saft | | livrată 2026-10-14 | SC-SAFT-38…51 |
| F-SAFT-03 | Generez SAF-T pentru o lună cu corecție valorică pe lot | T2 | saft | | deschisă | — |
| F-ANAF-01 | Completez și actualizez un partener din datele ANAF | T1 | nomenclatoare | | livrată 2026-10-14 | — |
| F-ANAF-02 | Actualizez din ANAF mai mulți parteneri dintr-un ecran | T1 | ecrane | | deschisă | — |
| F-ANAF-03 | Trimit și primesc facturi prin e-Factura | T2 | nomenclatoare | | deschisă | — |
| F-CFG-01 | Schimb din aplicație, fără versiune nouă, politicile: TVA, numerotare, scadențe, implicitele documentelor, mișcările SAF-T, amortizarea | T1 | politici | | livrată 2026-10-14 | — |
| F-CFG-02 | Verific configurația profilului și văd cine a schimbat ce | T1, T3 | politici | | livrată 2026-10-14 | — |
| F-SEC-01 | Văd și operez doar ce îmi permite rolul, cu refuzuri uniforme pe toate ușile | T1 | api | | livrată 2026-10-14 | SC-CIT-87, SC-SAFT-36, SC-D9B-10 |
| F-IMP-01 | Aduc documentele dintr-un sistem 1C prin motor, cu diferențele raportate | T4 | import | | deschisă | — |
| F-VAL-01 | Țin partide și solduri în valută, cu diferența de curs la stingere | T1 | partide | | deschisă | — |
| F-CMD-01 | Rezerv stoc pe comenzi de vânzare și distribui o factură din mai multe gestiuni | T1 | stoc | | deschisă | — |
| F-SAL-01 | Calculez și contez salariile | T1 | documente | | deschisă | — |
| F-PRD-01 | Produc pe rețete, cu consumul normat | T1 | stoc | | deschisă | — |
| F-BUG-01 | Țin execuția bugetară completă, cu împărțirea pe cofinanțări | T5 | politici | | deschisă | — |
