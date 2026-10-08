# TR-D9b, pasul 0 — măsurătorile dinaintea deciziei 111

**Data:** 2026-10-08. **Stare:** măsurat. Nu decide nimic: decizia 111 e
propusă și o aprobă numai owner-ul. Nu există contract TR-D9b și nicio
coordonată a postării nu s-a schimbat.

Cele șase puncte sunt cele din
[`111`](../decizii/111-repartitor-unic-si-registre-tipate.md) §„Înaintea
deciziei". Fiecare are cifra, metoda și ce confirmă sau infirmă din direcția
(a)–(f).

## Rezumat

| Punct | Cifra | Față de 111 |
|---|---|---|
| 1. Recensământul felurilor pe Flax | 22.389 din 82.154 de postări au ambele coloane; niciuna fără explicație; 152 (0,19 %) pe 3 conturi poartă și partenerul partidei, și un repartitor intern real | (a) confirmat, cu o clasă de hotărât; mulțimea de feluri per cont e necesară pe 33 din 74 de sintetice |
| 2. D394 cu partenerul în blocul fiscal | D394 pe ianuarie 2025 identic octet cu octet, cu 15.054 din 15.206 postări fiscale fără coordonata `Partener` | (b) confirmat |
| 3. Prototipul nucleului | 195 / 195 teste verzi (190 înainte, niciunul șters); 229 de linii adăugate, 106 scoase | (a), (b), (c) confirmate în nucleu; (c) cu trei rezerve |
| 4. Fișa contului prin pereche | scenă: 31 ms față de 32 ms pe 320 de rânduri, 4,5 s față de 5,4 s pe 201.280; Flax: 0,16 s față de 1,29 s și 2,3 s față de 21,2 s | (d) confirmat; răspunsul însuși se schimbă: cont unic pe 100 % din rânduri față de 20 % |
| 5. Conturile bugetare cu flag `Repartitor` | 20 de conturi; 6 au mișcare în legacy 2026, 14 n-au | lista e mai jos, **de confirmat de owner** |
| 6. Nul față de identificator gol (110-r3) | îmbinarea pe tuplu: 3,2 s cu identificator gol, 5,5 s cu `COALESCE` pe nul, peste 300 s cu ramură de nul; tabelul crește cu 70 % | identificatorul gol nu se justifică prin viteză; costă 5 chei străine |

## Bazele și regula de comparare

Bazele Flax și clonele de volum vechi nu mai existau pe server, deci s-au
refăcut amândouă, pe rând (o singură rulare grea o dată).

- **Importul Flax pe ianuarie 2025**: `Import1C --recreeaza --cititori
  --pana-la 1`, binar Release de la `78554c4`, bază nouă
  `Atlas.Conta.Import1C.Flax.D9bP0`. 21,6 minute; 15.232 de documente
  importate, 15 sărite, 1.365 de copii autogenerate; 18.702 tranzacții,
  82.154 de postări; `INV-CUB` verde. Reconcilierea rămâne neverde (§7).
- **Scena D9-A1**: `scara-volum-container.ps1 -Sufix .D9bVol -Factori F`,
  14 minute, 164 OK, zero FAIL. 5.001.179 de postări (3.603.541 contabil,
  1.397.638 stoc), 1.483.182 de documente, 651.644 de partide; baza are
  3.026 MiB în tabelul de formă al raportului și 3.695 MiB după rulare. Raportul: `run-verificari/scara-volum-20261008-150636/`.
  Prima lansare a căzut: `ScaraVolum.sql` scria `Documente.TotalStingere`,
  scoasă la tăiere, și nu copia `Postare.Pereche`. Scriptul e adus la schema
  de azi în același commit cu acest document.

O cifră de timp se compară numai cu perechea ei de pe aceeași bază, în
aceeași sesiune. Timpul e cel de pe server (`EXPLAIN (ANALYZE, BUFFERS)`,
`Execution Time`), mediana pe rulări la cald, cu mașina fără altă rulare
grea. Postgres e pe configurația implicită (`shared_buffers` 128 MB,
`work_mem` 4 MB).

Scripturile și ieșirile, neversionate: `run-verificari/tr-d9b-pas0/`.

## 1. Recensământul felurilor pe cont

**Metoda.** Pe `…Flax.D9bP0`, fiecare postare primește felul coloanei
`Partener` și al coloanei `Gestiune` din `Repartitori.ClrType`; cele cinci
gestiuni virtuale se recunosc după identitățile din `GestiuniVirtuale`
(`p1-recensamant.sql`).

**Cifra.** 22.389 din 82.154 de postări au ambele coloane, exact numărul din
contextul deciziei 111:

| Partener | Gestiune | Fapt fiscal | Unitate | Postări | Conturi |
|---|---|---|---|---|---|
| partener | unitate internă | da | — | 8.076 | 4 |
| partener | virtuală Client | nu | — | 5.185 | 1 (607) |
| partener | gestiune | da | — | 3.542 | 20 |
| partener | virtuală Furnizor | nu | partidă | 2.720 | 1 (401) |
| partener | gestiune | da | lot | 2.714 | 5 |
| partener | unitate internă | da | partidă | 148 | 1 (419) |
| partener | gestiune | da | partidă | 4 | 2 (408, 4091) |

14.484 au fapt fiscal peste repartitor intern real, 7.905 au gestiune
virtuală peste partener. **Zero** postări au partener peste gestiune reală
fără fapt fiscal și fără partidă: partenerul nu apare nicăieri neexplicat.

**Ce nu se închide singur.** 152 de postări, pe 419 credit (148, facturi de
ieșire), 408 debit (3) și 4091 debit (1): baza fiscală stă pe contul de terț,
deci piciorul poartă și partenerul partidei, și repartitorul intern al
faptului fiscal. Pe forma (a) repartitorul e partenerul, cerut de partidă.
Gestiunea internă nu mai are loc pe acel picior. Azi n-o citește niciun
cititor de sold acolo (`Contabil.Repartitor` întoarce partenerul când există),
dar e singura urmă a locului intern pe faptul fiscal al unui avans.

**Mulțimea de feluri per cont.** Din 74 de conturi sintetice mișcate, 15 nu
poartă niciun repartitor, 26 poartă un singur fel, 33 poartă două sau mai
multe, iar 21 poartă trei. Coloana `Gestiune` ține de fapt „repartitorul
intern": gestiune și unitate internă pe același cont (302, 371, 6xx, 7xx),
cont propriu și unitate internă pe 512 și 531. Coloana `Partener` ține pe
Flax numai parteneri.

**Față de 111.** (a) e confirmat: niciun picior nu are două repartitoare de
sold fără ca unul să fie partenerul fiscal sau o gestiune virtuală, cu
excepția celor 152. „Contul declară felurile ca mulțime" nu e un caz de
margine pentru 461 și 462, ci regula: 33 din 59 de sintetice cu repartitor.

**Limite.** Flax pe ianuarie n-are decont, DVI, imobilizări și nimic
bugetar. Angajatul ca terț nu apare în date; e citit numai din cod (§3).

## 2. D394 cu partenerul mutat în blocul fiscal

**Metoda.** Două copii ale bazei Flax. Pe a doua: coloană nouă
`PartenerFiscal`, completată din `Partener` pe toate postările cu fapt fiscal
(15.206), apoi `Partener` golit pe cele unde nu e și coordonată de partidă
(15.054). D394 pe ianuarie 2025 se generează din produs
(`D394Proiectii.D394`), o dată cu binarul de azi pe copia neschimbată și o
dată cu `Fiscale.Fapte` citind `PartenerFiscal` pe copia modificată.
Prototipul e pe branch-ul local `proto/tr-d9b-pas0-d394` (`b335670`), nu se
merge.

**Cifra.**

| | Operațiuni | SHA-256 al D394 serializat | `Fiscale.Versiune` D394 |
|---|---|---|---|
| azi, baza neschimbată | 2.583 | `AA9D2569…684F8557` | `B9ACFDC5…538A546A` |
| `PartenerFiscal`, coordonata golită | 2.583 | `AA9D2569…684F8557` | `B9ACFDC5…538A546A` |
| martor: binarul de azi pe coordonata golită | 142 | `2FDF4CC9…B37EF3B5` | `FE5FF827…863CA00C` |

Cele două fișiere JSON (795.200 de octeți) sunt identice la `cmp`. Amprenta
D300 e și ea identică. Martorul arată că testul are dinți: fără mutarea
cititorului, D394 pierde 2.441 de operațiuni.

**Scriitorul.** Partenerul faptului fiscal are un singur scriitor,
`Fiscal.CuFapt`, care îl ia din latura documentului numită de
`PoliticaTva.SursaContrapartida`. Recensământul confirmă pe date: toate cele
15.206 postări fiscale poartă exact acea latură (8.224 primitor pe facturi de
ieșire, 6.444 predător pe facturi de intrare, 488 și 50 pe retururi), niciuna
altceva și niciuna fără partener. Stornoul copiază coordonatele întregi.

**Față de 111.** (b) e confirmat: D394 depinde de partener numai prin
`Fiscale.Fapte`, iar mutarea e o redenumire pe citire și o atribuire pe
scriere. O precizare la textul (b): partenerul fiscal **se deduce azi din
document**, prin politica de TVA. Ce nu se poate e deducerea lui din pereche
sau din capete.

**Limite.** Copia `PartenerFiscal` e făcută în SQL, nu de declarant. Flax
are numai facturi și retururi: decontul (partenerul fiscal e angajatul) și
DVI (predătorul e vama) nu sunt în date.

## 3. Prototipul nucleului cu repartitor și fel

**Metoda.** Prototip de aruncat, numai în `nou/Atlas.Conta.Nucleu/`, făcut de
un agent în worktree propriu și verificat separat: suita rulată din nou,
diff-ul limitat la nucleu, niciun atribut de test scos. Branch local
`proto/tr-d9b-pas0-nucleu` (`8c36600`), nu se merge.

**Cifra.** 190 de teste înainte, 195 după, toate verzi; 9 corpuri de test
rescrise pe forma nouă, 5 adăugate, niciunul șters. 15 fișiere, 229 de linii
adăugate și 106 scoase; `GestiuniVirtuale.cs` dispare. `Storno`, `Miscare`,
`Mutare`, `Motor`, `Unitate`, `Tva/` și `Masura/` nu sunt atinse.

**Forma.**

- `Repartitor? Repartitor`, o valoare cu identificator și fel (partener,
  angajat, gestiune, cont propriu, unitate internă). „Niciunul" e nul, nu
  membru de enum, deci fără identificator gol.
- `Contrapondere? Contrapondere` (terț, consum, inventar, transformare) în
  locul gestiunilor virtuale; nul înseamnă postare în evidență.
- `Guid? PartenerFiscal` lângă blocul fiscal, cu un invariant nou: apare
  numai pe postarea cu fapt fiscal.

**Invarianții.** Valoarea, cantitatea, transferul, semnul și perechile rămân
neschimbate. Verificarea transformărilor se simplifică. Regula cantității se
strânge: cantitatea nemarcată cere lot și repartitor intern, cea marcată e
contracantitate legitimă, deci nucleul deosebește acum declarantul care a
uitat lotul. Nicio formă refuzată azi nu devine acceptată.

**Ce infirmă sau nuanțează.**

- **Felul pe postare nu e cerut de niciun invariant de azi.** Cu verificările
  de fel reduse la „repartitor nenul" pică numai cele două teste noi scrise
  pentru fel. Felul se poate citi din `Repartitori.ClrType`.
- **(c), rezerva 1.** Marcajul și repartitorul sunt axe independente. DSC
  pune virtuala Client și la primitor intern, deci iese „terț" fără terț.
  Diferența de la NIR pune Inventar împreună cu terț și partidă
  (`DeclarantNir.Diferenta.cs:89`). Nucleul nu poate cere „terț ⇒ repartitor
  extern" și nici „inventar ⇒ fără repartitor".
- **(c), rezerva 2.** Furnizor și Client se contopesc în „terț"; sensul
  rămâne de dedus din document sau din cont.
- **(c), rezerva 3.** La ASM contraponderea stă pe contul de stoc fără
  repartitor; gardul felurilor per cont trebuie să scutească postările
  marcate.
- **(a).** „Lot ⇒ gestiune" a trebuit scris „lot ⇒ repartitor intern": BCS
  pune lotul la un loc de consum, unitate internă. Custodia ar rupe regula.
- **(d).** Transformările și deschiderea n-au pereche, deci ASM n:m nu se
  citește prin ea.

**Limite.** Modulul, WebApi și uneltele nu compilează pe prototip și n-au
fost încercate. Costul portării declaranților și al schemei nu e măsurat. Ce
ține de declaranți e citit din cod, nu rulat.

## 4. Fișa contului: contrapartida prin pereche

**Metoda.** Interogarea fișei, luată din SQL-ul produsului, fără soldul
inițial. Varianta „azi" caută contrapartida printre toate postările de sens
opus ale tranzacției; varianta „pereche" adaugă egalitatea pe
`Postare.Pereche`. Cinci rulări alternate la cald, mediana, pe contul 401 în
luna lui cea mai încărcată (`p4/fisa.py`).

**Cifra.**

| Baza | Forma | Rânduri | Azi | Prin pereche | Cont unic azi / prin pereche |
|---|---|---|---|---|---|
| scena ×629 | cont × partener × lună | 320 | 32 ms | 31 ms | 64 / 320 |
| scena ×629 | contul întreg, luna | 201.280 | 5.420 ms | 4.463 ms | 40.256 / 201.280 |
| Flax | cont × partener × lună | 1.230 | 1.291 ms | 156 ms | 354 / 1.154 |
| Flax | contul întreg, luna | 7.206 | 21.152 ms | 2.342 ms | 756 / 5.870 |

**Citirea cifrei.** Scena are tranzacții mici (mediana 2 postări, maximum
3.145), deci subinterogarea de azi costă puțin. Flax are tranzacția unică de
deschidere, cu 10.828 de postări: pentru fiecare rând de deschidere din
fișă, subinterogarea de azi îi parcurge toate postările (în medie 1.522 pe
rând), iar răspunsul e lista tuturor conturilor de sens opus din deschidere.
Prin pereche citește o postare.

Răspunsul se schimbă, nu doar timpul. Azi fișa dă un cont unic de
contrapartidă pe 20 % din rânduri pe scenă și pe 10–29 % pe Flax; restul
primesc o listă de simboluri. Prin pereche dă cont unic pe 100 % din rânduri
pe scenă și pe 81–94 % pe Flax. Rândurile rămase fără contrapartidă pe Flax
(76, respectiv 1.336) sunt postări fără pereche: toate cele 10.828 de postări
de deschidere, 634 din 21.744 de transfer și 236 din 49.582 de operare.

**Față de 111.** (d) e confirmat și la cost, și la sens. Nu cere proiecție
materializată a capetelor la volumul măsurat. Limita e cea din §3: fără
pereche nu există contrapartidă, iar asta e corect la deschidere.

**Limite.** S-a măsurat SQL scris de mână, nu produsul schimbat. Prima formă
pe scenă e cea din `PerfCub` (un partener are 320 de rânduri la orice
factor), deci nu crește cu volumul.

## 5. Conturile bugetare cu flag `Repartitor`

**Metoda.** Cele 20 de conturi din seed-ul bugetar cu `DimensiuniObligatorii`
conținând `Repartitor`. Dovada de fel vine din legacy (`Contabilitate_2026`,
tabela `cnote`, 5.460 de note între ianuarie și iulie 2026): repartitorul de
debit și de credit al fiecărei note, clasificat prin `REPARTITORI_CLASIFICATI`
și prin forma codului fiscal. Unde legacy n-are mișcare, felul e propus din
natura contului și e marcat ca atare.

| Cont | Denumire | Partide | Legacy 2026 | Fel propus |
|---|---|---|---|---|
| 401.01.00 | Furnizori sub 1 an | da | 1.123 de picioare, 87 de repartitori, toți cu CUI | partener |
| 401.02.00 | Furnizori peste 1 an | nu | fără mișcare | partener (din natură) |
| 404.01.00 | Furnizori de active fixe sub 1 an | da | fără mișcare | partener (din natură) |
| 411.01.01 | Clienți sub 1 an | da | 35 de picioare, 3 repartitori cu CUI | partener |
| 411.02.08 | Clienți incerți peste 1 an | nu | fără mișcare | partener (din natură) |
| 437.01.00, 437.02.00 | Contribuții pentru șomaj | nu | fără mișcare | **de hotărât** |
| 442.08, 442.08.01 | TVA neexigibilă | nu | fără mișcare | partener (din natură) |
| 458.01.00, 458.05.01, 458.05.02 | Sume de la autoritățile de implementare | nu | fără mișcare | partener (din natură) |
| 461.01.09 | Debitori sub 1 an | da | 12 picioare, 2 repartitori cu CUI | partener; angajat de confirmat |
| 462.01.01 | Creditori, datorii comerciale | nu | fără mișcare | partener, angajat (din natură) |
| 462.01.09 | Creditori, alte datorii | nu | 65 de picioare: 7 repartitori cu CNP, 4 cu CUI, 2 fără cod | partener **și** persoană fizică |
| 552.00.00 | Disponibil din sume de mandat și în depozit | nu | aceleași 65 de picioare, același repartitor ca pe 462.01.09 | **de hotărât** |
| 774 | Finanțare din fonduri externe | nu | fără mișcare | partener (din natură) |
| 803.00.01 | Active primite în folosință | nu | 7 picioare, fără repartitor | **de hotărât** |
| 804.90.00 | Garanție bancară de bună execuție | nu | 1 picior, fără repartitor | partener (din natură) |
| 805.00.00 | Disponibil din garanția constituită | nu | fără mișcare | partener (din natură) |

**Trei lucruri de hotărât de owner.**

1. **552.00.00.** În legacy repartitorul lui e deponentul, același ca pe
   462.01.09, pe fiecare notă. În modelul nou piciorul de trezorerie poartă
   contul propriu. Cu un singur repartitor nu încap amândouă: ori contul
   declară „partener" și contul propriu se pierde de pe picior, ori declară
   „cont propriu" și analitica pe deponent rămâne numai pe 462.
2. **462.01.09.** 7 din 13 repartitori sunt persoane fizice (CNP), neclasificate
   în legacy. Cer hotărât dacă intră ca angajați sau ca parteneri persoane
   fizice. Oricum contul cere mulțimea.
3. **437 și 803.** N-au dovadă în date: 437 n-are mișcare, 803 are note fără
   repartitor deși contul are flag.

14 din cele 20 de conturi n-au nicio mișcare în legacy 2026, deci felul lor e
propunere, nu constatare.

## 6. 110-r3: nul față de identificator gol

**Metoda.** Două copii ale scenei: N păstrează cele 11 coordonate nulabile; G
le are `NOT NULL`, cu identificatorul gol în loc de nul. Amândouă trec prin
`VACUUM (FULL, ANALYZE)`. Pe G indecșii parțiali pe `Partener` și `Unitate`
sunt refăcuți cu predicatul echivalent (`<> gol`). Întrebarea măsurată:
soldurile de dinaintea unei date (1.068.315 atomi) îmbinate la stânga cu
mișcările de după, pe cont și pe cele 11 coordonate (`p6/`).

**Cifra, pe scenă (5.001.179 de postări).**

| Forma îmbinării | N (nul) | G (identificator gol) |
|---|---|---|
| ramură de nul, forma pe care o dă EF | peste 300 s | — |
| `IS NOT DISTINCT FROM` | peste 300 s | — |
| `COALESCE` pe ambele părți | 5.463 ms | — |
| egalitate simplă | 1.573 ms, **rezultat greșit** (zero potriviri) | 3.212 ms |
| `UNION ALL` + `GROUP BY`, forma produsului de azi | 4.698 ms | 5.077 ms |
| agregarea pe tuplu, singură | 1.132 ms | 1.242 ms |
| balanța pe cont, o parcurgere | 253 ms | 282 ms |

Cele trei forme corecte dau același rezultat (1.068.315 rânduri, 3.777
potriviri). Pe Flax, la 82.154 de postări, ordinea e aceeași: 11,7 s cu
ramură de nul, 138 ms cu `COALESCE`, 84 ms cu identificator gol.

| Mărime | N | G | Diferență |
|---|---|---|---|
| `Postare_Contabil`, tabelă | 696 MiB | 1.210 MiB | +74 % |
| `Postare_Stoc`, tabelă | 290 MiB | 470 MiB | +62 % |
| indecșii ambelor partiții | 685 MiB | 621 MiB | −9 % |
| baza întreagă | 3.522 MiB | 4.151 MiB | +18 % |

Pe profilul privat `Valuta` și cele șase coduri bugetare sunt nule pe 100 %
din postări; celelalte patru coordonate sunt nule pe 33–53 %. Cele șapte
coloane mereu nule dau cea mai mare parte a creșterii.

**Ce cere schema.** Trei coordonate au azi chei străine pe partiții:
`Partener` → `Repartitori` și `Produs` → `Produse` pe ambele, `Unitate` →
`Loturi` pe stoc. Identificatorul gol cere scoaterea celor 5 chei sau câte un
rând-santinelă în fiecare tabel. `CK_Postare_FelUnitate` leagă `Unitate` nulă
de `FelUnitate` și `UnitateDeschisa` nule și ar trebui rescris. `Gestiune` nu
are cheie străină, din cauza identităților virtuale.

**Citirea cifrei.** Identificatorul gol face îmbinarea pe tuplu de 1,7 ori
mai rapidă decât `COALESCE` (3,2 s față de 5,5 s) și de 1,5 ori mai rapidă
decât forma de azi a produsului. Costă 70 % în plus la mărimea tabelului,
circa 10 % la fiecare parcurgere și cele 5 chei străine. Forma catastrofală
se evită azi fără schimbare de schemă, iar proba `RAMURA-NUL` o prinde în
ModelCheck.

**Față de 111.** Punctul 6 se leagă de (a) și (c) exact cum spune decizia:
prototipul din §3 ține „niciunul" ca nul și n-are nevoie de identificator
gol, iar un repartitor unic nulabil poate purta cheie străină pe
`Repartitori`. Cu gestiunile virtuale scoase, cheia devine posibilă și pe
piciorul intern.

**Limite.** S-a măsurat o singură întrebare de îmbinare, scrisă de mână.
Scăderea indecșilor pe G nu e explicată. Cifrele sunt pe profilul privat; pe
bugetar codurile nu sunt nule și creșterea ar fi mai mică.

## 7. Observații în afara celor șase puncte

- **Reconcilierea Import1C nu e reproductibilă între rulări.** Importul de
  azi are 15 rânduri FAIL, față de 10 la pasul 8 al TR-D9a; diferența e pe
  contractul 5 (partidele inițiale). Atribuirea pe commit-uri s-a oprit la a
  doua rulare, fiindcă același binar, pe aceeași sursă, dă rezultate
  diferite:

  | Rulare | Tranzacții | Postări | Împerecheri | Stinse integral | Fără explicație |
  |---|---|---|---|---|---|
  | pasul 8 (`46916d9`) | 18.713 | 82.142 | — | 1.472 | 5 |
  | cu 112, fără 113 (`9f15c3d`) | 18.724 | 82.144 | 2.133 | 1.468 | 8 |
  | azi (`78554c4`), prima | 18.702 | 82.154 | 2.129 | 1.473 | 11 |
  | azi (`78554c4`), a doua | 18.707 | 82.150 | 2.132 | 1.476 | 10 |

  Între cele două rulări ale aceluiași binar, numai 6 partide FAIL sunt
  comune (3 apar numai în prima, 4 numai în a doua), iar diferența pe
  partenerul cel mai afectat de pe 401 e −41.098,17 într-una și −190.471,37
  în cealaltă. Numărul documentelor importate e același (15.232). Variația
  între rulări e de mărimea diferenței dintre commit-uri, deci creșterea de
  la 5 la 10 nu se poate atribui deciziilor 112 sau 113 prin numărare.
  Cauza nu e dovedită. Candidat, citit din cod: stingerile se aplică în
  ordinea identificatorului de document (`Imperecheri.cs`, `OrderBy(d =>
  d.ID)`), iar identificatorii se generează din nou la fiecare import.
  Restanța: 107-r11. Dovezile: `run-verificari/tr-d9b-pas0/atribuire/`.
- **(e) și (f)** nu au măsurătoare proprie în pasul 0. Recensământul din §1
  e compatibil cu respingerea formei „ambele coloane".

## 8. Ce a rămas pe disc

Baze create, niciuna ștearsă (în afara a două încercări căzute ale
acestui pas, recreate imediat):

| Bază | Mărime | Rost |
|---|---|---|
| `Atlas.Conta.Import1C.Flax.D9bP0` | 107 MB | importul pe ianuarie la codul de azi |
| `Atlas.Conta.ModelCheck.Privat.P0D394a`, `.P0D394b` | 107 / 119 MB | copiile punctului 2 |
| `Atlas.Conta.P0.FlaxN`, `.FlaxG` | 105 / 114 MB | copiile mici ale punctului 6 |
| `Atlas.Conta.ModelCheck.Privat.D9bVol` | 3.695 MB | scena D9-A1 pe schema de azi |
| `Atlas.Conta.P0.VolN`, `.VolG` | 3.522 / 4.151 MB | copiile punctului 6 |
| `Atlas.Conta.Import1C.Flax.P0la112`, `.P0azi2` | câte ~107 MB | rulările de atribuire din §7 |

Branch-uri locale de prototip, nepublicate: `proto/tr-d9b-pas0-nucleu`
(worktree `.claude/worktrees/agent-adc54ba29b0d4336f`) și
`proto/tr-d9b-pas0-d394` (worktree `.claude/worktrees/d9b-p2`).
