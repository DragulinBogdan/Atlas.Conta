# 119 — Compensarea valorică aparține corecției: a doua `Operare` pe un document operat; stornoul beneficiarului conține numai inversa lui, iar efectele active le mută corecția

- Data: 2026-10-13
- Stare: activă
- Execuție: apartenența compensării hotărâtă de owner 2026-10-13, pe comparația celor două variante (R6 din `docs/nucleu/tr-d9b-corectii-valorice-review-r1.md`). Amendează 090 (a) și (i); precizează 091 (h). Neimplementată: intră cu pasul 3 al contractului TR-D9b-CV, după review-ul predicatelor și al explicației
- Arii: domeniu-si-operare
- Amendează: 90 (a, i)
- Docs: contractul `docs/nucleu/tr-d9b-corectii-valorice-contract.md` (CV-D4, CV-D5, CV-D7, CV-D8); `docs/nucleu/tr-d9b-corectii-valorice-review-r1.md` (R1, R3, R6, R7); matricea `docs/nucleu/tr-d9b-corectii-valorice-producator-si-compensare.md`; catalogul `docs/nucleu/scenarii/CVL.md` (SC-CVL-26…30); simulatorul `docs/nucleu/probe/cvl_matrice.py`.

## Regula durabilă

**Efectul unei corecții valorice rămâne al corecției, oriunde îl mută stornarea altui document. Stornarea beneficiarului inversează numai faptele lui; mutarea o scrie corecția.**

(a) **Câte tranzacții `Operare` are un document.** Operarea scrie exact una. Un document de corecție valorică, cât timp e operat, mai primește câte una la fiecare stornare a unui beneficiar asupra căruia are efect activ nenul. Altă cale spre a doua `Operare` nu există: `Opereaza` nu se cheamă din nou și documentul nu trece prin Draft. Restul lui 090 (a) rămâne.

(b) **Compensarea aparține documentului și liniei care au produs alocarea compensată.** Se scrie la data stornării beneficiarului, într-o tranzacție `Operare` suplimentară a corecției, cu explicație proprie și legătură verificabilă către beneficiarul inversat. Cauza și proprietarul sunt corecția; stornarea beneficiarului doar declanșează mutarea. Nu se adaugă fel de tranzacție și nici câmp pe postare.

(c) **Stornoul beneficiarului.** Tranzacția lui `Storno` conține exact inversa postărilor proprii. Alocările valorice atribuite postărilor lui și inversele acelor alocări nu intră în ea: „cauzate ∪ atribuite” din 090 (i) nu se mai aplică acestor atribuiri. Efectul lor încă activ se mută prin compensare (b). Documentul stornat se închide la zero pe fiecare cont.

(d) **O comandă, 1 + N tranzacții.** Comanda de stornare a beneficiarului scrie atomic tranzacția `Storno` și câte o `Operare` pentru fiecare document de corecție cu compensare nenulă. Alocările aceluiași document au perechi distincte în aceeași tranzacție. O corecție deja stornată nu primește nimic. Eșecul oricărei scrieri anulează tot, inclusiv starea documentului. Comanda blochează fiecare document de corecție pe care scrie.

(e) **Stornoul corecției** rămâne o singură tranzacție `Storno`: inversa exactă a tuturor postărilor ei, alocări și compensări, din toate tranzacțiile ei `Operare`. Nu conține postări cu altă cauză.

(f) **Fără destinație, comanda se refuză.** Stornarea recepției unui lot cu valoare de corecție activă pe stoc nu are unde muta valoarea și este refuzată. Cu toate corecțiile neutralizate, conține exact inversa recepției.

(g) **Ce nu se schimbă.** Atribuirea de la nominalizarea partidelor și selecția ei la storno rămân cele din 090 (i). Conservarea, regula inversei verificabile și oglinda explicației nu se relaxează. Apartenența nu ridică refuzul retragerii unei corecții cu compensări active.

## Context

090 (i) definea stornoul ca inversul postărilor cauzate ∪ atribuite, cu `Atribuit` drept „ce face definiția completă după o reevaluare”. 091 (h) a despărțit stornoul după reevaluare în două componente ale aceleiași comenzi atomice, inversarea originalului și compensarea efectelor rămase active, fără să spună unde se scrie compensarea și a cui este. Nucleul avea deja contraexemplul: `StornoulCuAtribuitDinAltDocumentNuECauzaStrainaDarNuEBalansat` arată că selecția atribuitului nu aduce singură contrapartida.

Prima matrice a contractului TR-D9b-CV scria compensarea în tranzacția `Storno` a beneficiarului. Review-ul a arătat că forma trecea `Conservare`, dar încălca regula inversei verificabile (`Citiri.Invarianti`), oglinda explicației (`Citiri.Explicatii`) și filtrul contabil (`Citiri.Contabil`): după stornarea bonului, contabilul ar fi arătat stoc 1.080 și consum 120, deși suma cubului dă 1.200 și 0. Revizia a mutat compensarea într-o `Operare` separată, în aceeași comandă.

Într-o `Operare`, cauza postării este documentul tranzacției (`Conservare.VerificaDocumentul`). Cauza și proprietarul compensării se aleg deci împreună, iar variantele sunt două: beneficiarul stornat sau corecția. Aritmetica este aceeași. A doua `Operare` pe un document operat contrazice în ambele variante litera lui 090 (a); lipsa unei constrângeri în schemă sau în cod nu ține locul deciziei. Amendamentul 110 de pe 090 (a) privește altceva.

## Tranșări

1. **Compensarea în `Storno`, cu discriminator persistat.** Respinsă: schimbă regula inversei verificabile, oglinda explicației și filtrul contabil, iar inversa compensării ar indica tot un `Storno`. Extinde semantica stornoului pentru un fapt care nu este inversă.
2. **`Operare` a beneficiarului stornat.** Respinsă. Stornoul corecției ar găsi compensarea numai prin saltul pe `Atribuit` și ar conține cauze străine. Bonul stornat ar rămâne cu sume nenule pe stoc și pe consum cât timp corecția e activă. Antetul de jurnal și reconcilierea de stoc din SAF-T ar cere schimbări. Fișa ar arăta stornarea corecției sub numărul bonului. Fiecare tip beneficiar ar găzdui o `Operare` nouă și explicația ei.
3. **`Operare` a corecției** (aleasă, owner). Stornoul corecției găsește compensarea între postările proprii. Beneficiarul se închide la zero pe fiecare cont. Toate rândurile corecției poartă numărul ei. Găzduirea rămâne la producătorii de corecții. Costuri asumate: 1 + N tranzacții și blocarea fiecărui document de corecție; fișa corecției primește un rând datat la stornarea bonului; la stornoul corecției, inversa compensării intră în evenimentul de factură din SAF-T, de unde trebuie despărțită (CV-r1).
4. **Retragerea corecției cu compensări active.** Ar șterge și compensările, fiindcă sunt ale ei, și ar da finalul numeric corect în exemplul simplu. Nealeasă în primul pachet: relaxarea cere scenariu propriu pentru dependenți, referințe istorice și perioadele tuturor faptelor șterse.

Precedentul împerecherii, un `Transfer` scris ulterior pe un document operat, arată că un document primește fapte după operare. Nu arată că a doua `Operare` era admisă; de aceea amendarea este explicită.

## Probe

Fără probă de produs: mecanismul nu este implementat și catalogul CVL nu este încă selecție executabilă.

- Simulatorul aritmetic `docs/nucleu/probe/cvl_matrice.py`: 16 ordini de stornare, 4 cazuri temporale și 5 de stornare a recepției. Asertează că nicio postare nu are cauză diferită de proprietar, că beneficiarul stornat se închide la zero pe fiecare cont și că fiecare corecție activă poartă integral delta ei. Ține corecțiile active într-o mulțime; nu reconstituie familia din fapte.
- Citire statică a cititorilor de cauză și de proprietar (R4, R6). Neacoperite: filtrele pe document scrise cu alt nume de parametru decât `p`/`t`/`o`, listele XAF pe postări, clientul React și aserțiunile ModelCheck.
- Că selecția de azi ar pica pe `PerecheInvalida` la stornarea recepției (f) rezultă din citirea codului; cazul nu a fost rulat.

## Ce rămâne deschis

- **Implementarea**, în pașii 3–5 ai contractului, cu SC-CVL-26…30 și SC-X-05/06/07 prin comenzile reale, pe ambele profiluri. Paginile de stare curentă descriu codul și se aduc la zi atunci.
- **Predicatele** rădăcinii și ale perechii de compensare (R3) și **explicația** (CV-D8) se confirmă la review-ul dinaintea codului. Decizia fixează apartenența și numărul tranzacțiilor, nu forma lor fizică.
- **CV-r1** — reprezentarea SAF-T a corecției și despărțirea compensărilor de liniile comerciale; până atunci exportul refuză (CV-D9).
- **Alți beneficiari și alte destinații.** Primul pachet acoperă recepția și consumul. FCL, BTR și ASM cer scenarii proprii pentru identificarea beneficiarului și destinația compensării. Cursul și fișa de imobilizare din 091 (h) își primesc destinația în pachetele lor; apartenența din (b) li se aplică.
