# Invarianții Atlas.Conta

Document viu, agreat 2026-08-02 (sesiune de arhitectură). Nu descrie codul —
îl **judecă**. Jurnalul de decizii (`docs/decizii/`, un fișier per decizie;
rezumatul durabil în CLAUDE.md) răspunde la „de ce e așa"; pagina asta
răspunde la „ce trebuie să rămână adevărat".

**Modul de folosire**: la orice mâncărime arhitecturală, întrebarea e „ce
invariant e amenințat?". Dacă răspunsul e „niciunul, doar implementarea mă
irită" — e o discuție de cost, nu de arhitectură. Un invariant bun interzice
ceva; clauzele de interdicție sunt partea care contează.

---

## I. Operația intră în sistem ca document: sursă → destinație, header + detalii

Orice urmă în registre are în spate un document operat (excepție unică,
declarată: rândurile de deschidere ale migrării, `DocumentId = null`). Nu
există tip de document fără semantica ambelor laturi definită — dacă o cerință
„n-are predator", răspunsul e: găsește-l, nu scoate latura. Ce nu postează nu
e operație — e **relație între documente** (imperecherea) sau **proiecție
peste registre** (solduri, balanțe).

## II. Baza poartă identitatea operației; frunza poartă culegerea; motorul nu cunoaște frunzele

Nucleul generic (operare, stoc, rezolvarea dimensiunilor, gardienii) consumă
exclusiv baza — ce are nevoie de la un tip anume primește prin **contract**
(hook polimorf sau interfață declarată de frunză), niciodată prin `is`/`switch`
pe tip. Comportamentul specific unui mecanism trăiește într-un serviciu numit
(insulă: DescarcareService, InchidereTvaService, ImperechereService), nu
împrăștiat în nucleu.

Pe bază intră un câmp doar dacă trece **ambele** teste: (1) semantica lui e
identică pentru orice tip care l-ar purta și (2) motorul îl consumă direct la
postare (Valoare, ValoareTva, Cantitate semnată, Lot). Ce variază per tip ca
prezență sau semantică = coloană pe frunză + contract. „Des null" e acceptabil
pe bază; „alt înțeles per tip" e interzis (lecția câmpurilor refolosite din
legacy). **Motorul are nevoie de valoare, nu de coloană.**

*Prima reconciliere cunoscută*: `DocumentDetaliu.Dimensiuni` (owned pe bază)
încalcă invariantul la starea-țintă — **decizia 54** (2026-08-02): dimensiunile
coboară pe frunze ca FK-uri explicite, motorul le primește ca value object
ne-persistat prin contract; registrul păstrează setul plin, plat. Stocarea
rămâne inline pe tabelele owner-ilor (normalizarea în tabelă separată a fost
analizată și respinsă).

## III. Registrele sunt singurul adevăr al agregării: append-only, complete, scrise doar de motor

Orice sold, balanță, fișă sau raport e o **sumă peste registre** (+
nomenclatoare pentru etichete) — niciodată o plimbare peste documente; nicio
interogare polimorfă pe `Document` în fluxuri calde. Rândul de registru se
scrie o dată, complet rezolvat (conturi finale, dimensiuni pline per latură) —
cine citește nu re-rezolvă și nu face coalesce. Un rând scris nu se modifică
niciodată: corecția e anularea (doar fără dependenți, în perioadă deschisă)
sau storno-ul (rânduri inverse, la data stornării). Perioada închisă e graniță
absolută, fără excepții de admin.

Demarcația: *agregatul scanează registre; starea unui document se citește de
pe document* (restul de stins al imperecherii e calcul operațional
per-document, nu agregare). Soldul lui 401 nu se calculează niciodată din
facturi.

## IV. Structura e cod; politica e date; politica nu inventează comportament

Ce *există* — clasele, câmpurile, ierarhia tipurilor, schema registrelor,
mecanismele motorului — e cod, compile-time. Ce se *alege* — care conturi,
care semne și filtre de stoc, obligativități, numerotare, scadențe, conturile
de TVA — e date: tabele de politică cheiate pe ancorele seed 1:1 cu clasele.

Interdicții: **(1)** un rând de politică doar parametrizează un mecanism
existent în cod; când o cerință nu încape într-o politică existentă, răspunsul
e mecanism nou în cod (eventual + tabela lui de politică), niciodată o tabelă
„flexibilă" sau expresii interpretabile în date — pe cliff-ul ăla a murit
legacy-ul (EAV) și tot pe el ar muri un interpretor propriu. **(2)** structura
nu se configurează: nu există și nu vor exista tabele care descriu câmpuri sau
ecrane (GEST_DEFA_DOCUM a murit definitiv). **(3)** profilul contabil e pachet
de seed — plan + Clasă/Tip + politici + validări — **niciodată clase**;
profilul și convențiile lui (rotunjire) sunt înghețate per bază, sub gardian
(`SetareProfil`/`VerificaProfil`). Testul la tentația claselor per profil:
„diferă *schema* sau doar *politica și vizibilitatea*?" — până azi răspunsul a
fost de fiecare dată al doilea.

## V. Sursele externe sunt evidență, niciodată canonic

Legacy, 1C, orice conector viitor: servesc ca direcție și probă, nu ca
specificație. Politicile și structura se definesc **per cerință**, curat, în
sistemul nou — niciodată prin transcrierea config-ului sursei. Mișcările
importate intră ca **documente operate prin motor** — nu se copiază registre
(unica scriere directă: deschiderea, excepția din I). La reconciliere,
diferențele sursei se **raportează, nu se ascund**; a forța valorile sursei în
import ca să „iasă potrivirea" e interzis. Harness-ul validează modelul
nostru: când nu iese la cent, ori modelul are o gaură (o repari), ori sursa
are un fapt propriu (îl înregistrezi ca divergență măsurată, la locul faptei)
— a treia opțiune nu există.

## VI. Evaluarea stocului e motorul: lotul are identitate, prețul lui e fapt, o singură metodă activă per bază

`Produs` e catalog; `Lot`-ul se naște pe linia de intrare, cu preț unitar
fixat la operare, gestiune și dată — și nu-și schimbă prețul niciodată.
Ieșirile referă loturi: picking auto-FIFO, override-ul manual (pinul) e
excepția intenționată. Orice mișcare de stoc are rândul ei de registru de stoc
— stocul nu se mișcă „doar contabil".

Interdicții: **(1)** valoarea unei ieșiri e fapt scris în registru la operare,
niciodată funcție recalculabilă retroactiv — lumea recalculului (1C) e cea din
care registrele au ieșit deliberat; **(2)** nu există evaluare paralelă sau
„doar la raport" — metoda de evaluare *este* motorul (finalizarea lotului,
descărcările, gardianul de sold, storno), nu un parametru de afișare;
**(3)** metoda e una singură per bază, înghețată sub gardian ca profilul și
rotunjirea — supapa `PoliticaEvaluare` (CMP periodic, decizia 51e) e parcată
*cu nume*: când va veni, schimbă funcția de evaluare în punctele de
descărcare, nu structura, și intră tot sub gheața per bază.

---

*Ce NU e în pagină, deliberat: nimic despre XAF, React, owned types, EF —
stratul de implementare e exact cel de care constituția apără. Owned vs flat
(2026-08-02) a fost o discuție de cost, nu de invariant.*
