# 112 — Recepția facturii se contează pe regula ei de contare (`FCT/Stoc`)

- Data: 2026-10-08
- Stare: **activă**; destinația restanței B-r3 (regulă proprie pe FCT) hotărâtă de owner 2026-10-08. Amendează 026 (a) și precizează 026 (c); închide B-r3.
- Docs: catalogul `docs/nucleu/scenarii/FCT.md` (SC-FCT-15, SC-FCT-16); `docs/stare-curenta/domeniu-si-operare.md`, `politici-si-fiscalitate.md`, `limite-curente.md`.

## Regula durabilă

**Linia de stoc a facturii de intrare se contează pe regula de contare a facturii, ca orice altă natură. Niciun cont al recepției nu se împrumută din altă politică.**

(a) **FCT are regulă pe natura `Stoc`.** Seed-ul ambelor profiluri poartă rândul `FCT/Stoc`: debit din contul Tipului, credit din contul furnizorului (latura predator), cu rezerva 401 la privat și 401.01.00 la bugetar. Declarantul rezolvă fiecare linie, de orice natură, prin aceeași potrivire: Tip exact → natură → generică.

(b) **Fără regulă, recepția e refuzată.** Linia de stoc fără regulă potrivită e refuzată cu `REGULA_CONTARE_LIPSA`. Contul furnizorului nu vine nici din politica de TVA, nici din regula altei naturi.

(c) **Debitul recepției e contul lotului.** O regulă care rezolvă debitul pe alt cont decât al lotului născut de linie e refuzată cu `CONT_STOC_LIPSA`, ca pe NIR-ul manual.

(d) **Dimensiunile recepției urmează regula.** `Comun`, `OverrideDebit` și `OverrideCredit` ale regulii se aplică peste analiza liniei, ca la celelalte naturi.

(e) **Explicația configurației spune ce face declarantul.** Pe un tip al cărui declarant contează prin reguli, linia fără regulă e anunțată ca refuz (`REGULA_CONTARE_LIPSA` între rezerve). Pe un tip care nu contează prin reguli, absența regulii nu e refuz.

(f) **Explicația tranzacției reține regula recepției.** `ContRezolvat` numește regula pe ambele conturi, iar ipoteza `VersiunePolitica` o poartă. Politica de TVA se reține numai când declarația are fapt fiscal.

(g) **NIR-ul conex rămâne documentul diferențelor** (098, 099). Regula `NIR/Stoc` e a NIR-ului manual și a plusului constatat. Cele două reguli nu se citesc una pe alta.

(h) **O bază fără rândul `FCT/Stoc` refuză linia de stoc** până la alinierea seed-ului. Codul nu poartă o rezervă pentru baze nealiniate (102 b).

## Context

B-r3 s-a deschis la TR-D6b: declarația a mutat recepția pe factură (TR-D3), dar regula ei de contare a rămas a NIR-ului conex, care nu e în operandul facturii. Amânarea avea un motiv: cât trăiau registrele, un rând `FCT/Stoc` în `RegulaContare` ar fi făcut planul vechi să posteze recepția de două ori. La tăiere (110) motivul a dispărut, iar destinația a rămas a owner-ului.

Starea codului înaintea acestei decizii:

- Debitul recepției era contul implicit al Tipului, fixat în declarant.
- Creditul se rezolva o dată pe document: din `PoliticaTva.SursaContrapartida`, iar în lipsa politicii de TVA din prima regulă `FCT/Serviciu` sau `FCT/Cheltuiala`.
- Bugetarul nu are `PoliticaTva`, deci contul de furnizor al recepției de stoc venea mereu din regula de servicii. O editare a creditului acelei reguli muta tăcut și recepțiile de stoc.
- Recepția nu putea primi override de dimensiuni din politică.
- `api/politici/explica` răspundea pe FCT × stoc „linia nu contează pe acest tip de document", deși cubul posta recepția pe factură. Proba F24-E1 pin-uia acest răspuns.

## Tranșări

Trei destinații au fost puse în fața owner-ului:

1. **Regulă proprie `FCT/Stoc`** (aleasă). Toate naturile trec prin aceeași rezolvare; funcția care împrumuta contrapartida dispare din declarant.
2. **Recepția citește regula `NIR/Stoc`**, a țintei conexului. Respinsă: operandul facturii ar încărca politica altui tip, iar explicația ar cere cod special ca să nu mintă.
3. **Limită declarată**, cu comportamentul neschimbat. Respinsă: împrumutul din regula de servicii ar fi devenit canonic prin vechime.

Costul variantei alese e un rând de politică în plus pe fiecare profil. Riscul ca `FCT/Stoc` și `NIR/Stoc` să diveargă e mic: pe NIR-ul conex, regula `NIR/Stoc` dă numai debitul plusului, iar acela e oricum păzit de contul lotului.

Cifrele seed-ului nu se schimbă: recepția rămâne 3xx = 401 pe ambele profiluri. Se schimbă de unde vine contul și ce se întâmplă când regula lipsește.

Constatarea (e) a apărut la verificare: toți declaranții care contează prin reguli rezolvă regula pe fiecare linie și refuză linia fără regulă (BCS, DSC, LDI, FCL, FCT, NIR-ul manual și plusul constatat, RDC, RLF, PLT/INC, DEC); numai liniile NIR-ului conex preluate din recepția facturii moștenesc capătul ei de stoc și nu cer regulă. Textul „linia nu contează" al explicației era deci fals pe toate aceste tipuri, nu doar pe FCT × stoc, și a fost înlocuit cu rezerva de refuz.

## Verificare

- Catalog: SC-FCT-15 și SC-FCT-16, pe operand scris de mână în care politica de TVA și regula de servicii duc spre alte conturi decât recepția. Pe codul dinainte, creditul recepției ar fi ieșit pe contul politicii de TVA.
- SC-FCT-01…14 și lanțurile SC-X rămân verzi cu așteptările neatinse.
- F24-E1 re-țintită: FCT × 302 câștigă regula pe natură, „Se postează 302 = 401". F24-E1b nouă: BCS × serviciu anunță refuzul, BTR × stoc nu.
- ModelCheck integral pe ambele profiluri, pe clonele `.ClaudeBr3`: bugetar 3.554 / 0, privat 4.834 / 0 (`run-verificari/20261008-002932-023/rezultat.json`).
- `pnpm verifica:drift` fără diferențe; `tsc --noEmit` fără erori. Fără migrație, fără caption nou.

## Ce rămâne deschis

Nicio restanță nouă.

- Textele de probă din ModelCheck care mai spun „recepția contează pe NIR (26a)" (`VerificaSaft`, `VerificaSaftStocuri`, `E2eFacturaIntrare`) se taie la atingere; aserțiile lor citesc cubul și rămân adevărate.
- Bazele hosturilor (`Atlas.Conta.BackOffice.Privat` și clonele de import) primesc rândul `FCT/Stoc` la următoarea rulare a updater-ului; până atunci refuză linia de stoc a facturii, conform (h).
