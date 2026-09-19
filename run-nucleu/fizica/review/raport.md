# Review advers al sintezei `docs/nucleu/nucleu-fizica.md` (2026-09-19)

Read-only: nimic modificat în repo în afara acestui director, niciun obiect creat
sau șters în Postgres, doar `SELECT` și `EXPLAIN (ANALYZE, BUFFERS)` pe
`Atlas.Conta.Nucleu.Fizica.x1` și `.x10`. Fix-urile le aplică coordonatorul.

Probele rulate de review, reproductibile:
`review/01-f0-cu-referinta.sql`, `02-q06s-cu-antet.sql`, `03-q10-coloanele-f0.sql`,
`04-imperecheri-mapare.sql`, `05-sold-per-granita.sql`.

---

## MAJOR

### M1. „F0 nu scalează pe «sold la dată» prin construcție" (§2.4) e fals, și cu el cade câștigul de titlu 618 → 113 ms

**Afirmația atacată.** §2.4, ultimul bullet: „**F0 nu scalează pe «sold la dată»
prin construcție**: doar perioadele de referință sunt materializate; balanța lui
2025 nu găsește referința 2024-12-31 și citește registrul integral". Pe ea stă
§2.3, „ce cumpără snapshot-ul: 6–10×", și coloana F0 din tabelul §2.1.

**Proba.** `SolduriService.Referinte` (`Motor/SolduriService.cs:75-89`) e
„ultima închisă **plus fiecare decembrie închis**". Într-o bază cu zece ani
închiși, `Referinta(2024-12-31)` EXISTĂ, iar F0 citește snapshot plus rulajele
unui an, exact mecanismul lui `cub."Sold"`. Baza de măsurare are un singur
decembrie închis (2025-12), iar replicarea ×10 a lăsat `SolduriPerioada*`
nereplicate (FZ-D6), deci F0 a fost măsurat cu mecanismul lui dezactivat.

Măsurat acum pe `.x10`, una după alta, aceeași mașină, același moment
(`review/01-f0-cu-referinta.sql`):

| formă | mediană |
|---|---|
| F0 cu referință: snapshot 184.780 rânduri + rulajele unui an | **57 ms** |
| F2+S: `cub."Sold"` la 2024-12-31 + rulajele aceluiași an | **99 ms** |

F0 e de ~1,7× mai RAPID, nu de 5,5× mai lent. Cauza e chiar granularitatea pe
care §2.3 o vinde ca beneficiu: `SolduriPerioadaContabil` are 184.780 de rânduri
la graniță, `cub."Sold"` are 389.713 pe Contabil la aceeași graniță
(`review/05-sold-per-granita.sql` §1). „Cubul păstrează TOATE coordonatele" e și
un cost de citire, nu doar un câștig de acoperire.

**Ce trebuie să se schimbe.** Bulletul se scoate sau se rescrie ca „pe baza
măsurată, F0 are o singură referință, deci citește integral". Câștigul lui `Sold`
rămâne probat CUBE-VS-CUBE (F2 424 → F2+S 113). Față de F0, verdictul e
nemăsurat, iar prima măsurătoare disponibilă îl arată invers. Coloana F0 din §2.1
primește o notă la Q01/Q02/Q11 de același fel cu ° de la Q05/Q06.

### M2. Cifrele „+S" de la ×10 nu sunt nici ele cifre de scară

**Afirmația atacată.** Nota ° din §2.1 spune despre Q05/Q06 că „F0 citește DOAR
snapshot-urile de referință, nereplicate la ×10: cifra nu scalează prin
construcție; perechea comparabilă e coloana +S". §2.3: „Câștigul e de 6–10× și
crește cu istoricul".

**Proba.** `review/05-sold-per-granita.sql` §1: pe `.x10`, `cub."Sold"` are un
număr CONSTANT de rânduri per graniță — 389.656 pe Contabil la fiecare graniță
2017…2023, 389.713 la 2024 și 2025, 109.391 pe Stoc, 44.016 pe Fiscal. Replicarea
a refolosit aceleași ID-uri de coordonate, deci granițele nu se îngroașă. Q05+S
(54,6 ms), Q06+S (63,2) și Q11+S (104,6) citesc O SINGURĂ graniță, fără atingerea
postărilor, deci măsoară date de ×1. Este exact viciul pe care nota ° îl declară
numai pentru F0.

Q01+S și Q02+S sunt altceva: ele citesc graniță plus un an de postări din zece,
deci acolo câștigul e real.

**Ce trebuie să se schimbe.** Nota ° se extinde la coloana +S pentru Q05, Q06 și
Q11. Fraza „câștigul crește cu istoricul" se scoate: nu e măsurată, iar
avertismentul deja scris despre mărimea lui `Sold` („limită inferioară") se
aplică identic la TIMP, nu doar la disc.

### M3. Q06 „partide 567 → 63, bate F0 (72)" compară un raport cu o sumă

**Afirmația atacată.** §2.1, rândul „Partide cu rest (Q06)": F0 72°, F2 567,
F2+S **63**. §2.3: „partide 567 → 63".

**Proba.** `pas2/cub/q06s.sql` întoarce trei coloane: `Unitate`, `Sold`, `Sens`.
`f0/q06.sql` livrează la 71,8 ms raportul întreg — DocumentId, Tip, Număr, Data,
ContrapartidaId, ContrapartidaDenumire, Sens, Total, Asignat, Rest — prin șase
uniuni peste `Documente` cu `LEFT JOIN` pe `Repartitori` și excluderea
viramentelor interne `ContPropriu → ContPropriu`.

Adăugat DOAR antetul de document la Q06+S, pe `.x10`, acum
(`review/02-q06s-cu-antet.sql`):

| variantă | mediană |
|---|---|
| forma măsurată în pas 2, trei coloane | 70,9 ms, 114.680 rânduri |
| + `JOIN Documente` + `LEFT JOIN Repartitori` | **131–143 ms** |

Adică ~2× F0, iar varianta B e încă incompletă: nu are ramurile pe tip, nu
exclude viramentele interne, nu calculează `Asignat`. Mulțimile diferă și ele,
114.680 contra 93.206, iar cubul filtrează `HAVING <> 0` unde F0 cere `Rest > 0`.

**Ce trebuie să se schimbe.** Ori se măsoară forma care livrează raportul, ori
„bate F0" dispare din §2.1 și §2.3, iar cifra de 63 ms se declară ca „agregatul
gol, fără antet".

### M4. „Scoaterea lui C1/S1 a CÂȘTIGAT 9–16 %" nu e demonstrată

**Afirmația atacată.** §2.2: „Scoaterea lor (iterația unică, declarată) a CÂȘTIGAT
9–16 % pe patru interogări: 1,4 GB de index nefolosit concurează totuși pentru
cache". Și `pas2/indexi-f2.md`: „Scoaterea lor nu costă nimic; pe patru
interogări câștigă".

**Proba 1, martorul netratat.** C1 și S1 au fost scoase din f2 și f3. **F1 nu a
fost atins**: 18 indexi înainte și după, verificat acum
(`review/05-sold-per-granita.sql` §4: f1 = 18, f2 = 20, f3 = 212). Totuși
medianele lui F1 s-au mutat între `tabel-x10.md` și `tabel-x10c.md` cu aceeași
magnitudine și același semn ca „câștigul" lui F2:

| Q | F1 x10 | F1 x10c | Δ |
|---|---|---|---|
| Q02 | 1.357,8 | 1.190,2 | −12,3 % |
| Q08 | 7,5 | 6,6 | −12,0 % |
| Q11 | 968,0 | 871,3 | −10,0 % |
| Q01 | 523,2 | 484,5 | −7,4 % |
| Q05 | 412,9 | 387,8 | −6,1 % |
| Q06 | 733,2 | 691,1 | −5,7 % |

**Proba 2, mecanismul e contrazis de propriile buffere.** Dacă 1,4 GB de index
ar fi concurat pentru cache, scoaterea lor ar fi redus paginile citite. Din
`tabel-x10.md` contra `tabel-x10c.md`, `shared read` pe F2: Q01 154.335 → 154.357,
Q02 152.643 → 152.665, Q06 139.591 → 139.825, Q10 65.174 → 65.175. Zero schimbare,
în două cazuri creștere.

Singurul candidat care iese din banda lui F1 e Q10: F2 −16,3 % contra F1 −2,0 %.

**Ce trebuie să se schimbe.** §2.2 spune „scoaterea nu costă nimic și scutește
1,4 GB", nu „câștigă 9–16 %". Explicația cu cache-ul se taie, fiindcă bufferele
o infirmă. Dacă se vrea cifra, iterația trebuie re-rulată cu F1 ca martor în
aceeași serie.

### M5. Amendamentul 4 (împerecherea) e validat de un gate gol, iar datele care l-ar rupe lipsesc din bază

**Afirmația atacată.** §4.4: „**Împerecherea e tranzacție proprie, datată** …
Σ per (`Cont`, `Latura`) neschimbată, deci rulajele nu se umflă. **Validat de
gate**: probele (a)–(d) trec neschimbate cu 44.448 de asemenea tranzacții".

**Proba 1, gate-ul nu poate cădea.** Proba (a) e Σ D = Σ C per tranzacție. O
tranzacție de împerechere are două postări pe ACEEAȘI latură, −Suma și +Suma,
deci Σ D = 0 și Σ C = 0. Trece prin construcție, oricare ar fi maparea.

**Proba 2, postările nu intră în nicio interogare.** Toate cele 88.896 de postări
de împerechere au `Data = 2026-09-18`, iar fiecare interogare a lotului taie pe
`Data <= 2025-12-31` sau pe o lună din 2025. Verificat: 0 postări în fereastră
(`review/04-imperecheri-mapare.sql` §1). Cele 13 probe de egalitate nu văd
niciuna. În particular, Q06 iese identic pe `Incasare` (31.381 coincid, 0 diferă)
și `Plata` (2.486 / 0) tocmai pentru că NICIUNA dintre părți nu stinge nimic:
`PartideDeschise` ignoră aceleași împerecheri, din același motiv.

**Proba 3, ce ar rupe maparea.** `pas1/02-transform.sql` §4 ia `Cont` și `Latura`
din postarea de terț a STINGĂTORULUI și le pune pe AMÂNDOUĂ picioarele; documentul
stins primește doar `Unitate`. Numărat pe `.x1`
(`review/04-imperecheri-mapare.sql` §2), din 44.448:

| situație | cazuri | ce se rupe |
|---|---|---|
| contul de terț al documentului stins e altul | **1.200** | partida stinsă se închide pe un cont pe care n-a postat niciodată |
| documentul stins n-are nicio postare de terț | **1.179** | cubul deschide o partidă din nimic |
| partener diferit între stingător și stins | 1.194 | `Partener` de pe stins se scrie pe ambele picioare |

Plus: `Valoare` devine semnată pe Contabil (Latura 1: −520.769,01 … 520.769,01;
Latura 2: −600.688,00 … 600.688,00), ceea ce rupe regula că semnul stă în
`Latura`. Orice `SUM(Valoare) WHERE Latura = 1` citit ca rulaj debitor brut per
partidă e greșit. Per cont se compensează, de aceea proba (b) trece.

**Proba 4, „38 de documente stingătoare cu mai multe postări de terț" e mult sub
realitate.** `_TertDoc` alege una singură per document. Pe `.x1`: 59.549 documente
au o postare de terț, ~56.500 au mai multe (35.434 cu două, 12.426 cu patru, până
la 228). Mecanismul 1 din `pas2/egalitate.md`, ilustrat pe 2.072 de FCL cu avans
pe 419, atinge de fapt jumătate din documentele cu partidă.

**Ce trebuie să se schimbe.** §4.4 nu poate spune „validat de gate". Se declară
netestat, cu cele patru rupturi de mai sus, și se trimite la pasul 3 împreună cu
întrebarea „ce e o partidă". §2.5 („ce nu s-a putut proba") primește un rând:
împerecherea, din cauza artefactului de dată deja constatat în §5.

---

## MEDIU

### E1. Q10 „698 → 524" cumpără victoria prin coloane lipsă

`pas2/cub/q10.sql` calculează două sume pe două chei. `f0/q10.sql`, SQL real EF,
calculează șase sume condiționate plus două numărători, pe trei chei, cu
`TipStoc`. Rulat pe `.x10` cu setul de coloane al lui F0
(`review/03-q10-coloanele-f0.sql`): **657 ms** față de 524 ale formei măsurate;
F0 = 697,6. Câștigul scade de la −25 % la −6 %, iar cheia `TipStoc` tot lipsește
din model (FZ-D3), deci nici varianta B nu e egală.
→ cifra se marchează ca „gran redus" sau se re-măsoară cu aceleași coloane.

### E2. „Bate sau egalează F0 pe toate interogările lunii" e contrazis de propriul tabel

§2.1 conține Q11, terți SAF-T pe o lună: F0 600, F2 965. Și Q12b, tot o lună, în
`rezultate.md`: F0 16,7, F2 137 — rând scos din tabelul sintezei. Verdictul
enumeră ca pierderi doar fișa integrală și GLE.
→ se adaugă Q11 și Q12b la pierderi, sau fraza se restrânge la interogările
fiscale ale lunii (Q07, Q08), singurele unde F2 chiar bate fără `Sold`.

### E3. Tabelul §2.1 nu e „cel de după iterația declarată", e un amestec

Nota spune „Cifrele F2/F3 sunt cele de după iterația declarată a indexilor".
De fapt Q01/Q02/Q03/Q06/Q10 vin din `tabel-x10c.md`, iar Q04/Q05/Q07/Q08/Q09b/
Q11/Q12/Q13 din `tabel-x10.md`. Sub setul final declarat: Q07 F2 = 7,4 nu 6 și
F1 câștigă cu 5,4; Q08 = 7,5 și F1 câștigă cu 6,6; Q05 = 322,8 nu 336; Q11 F3 =
1.219,8 nu 1.181. Q07 și Q08 sunt totuși îngroșate ca victorii F2. Îngroșarea mai
e greșită la Q13, 0,6 marcat deși F0 dă 0,3 și F1 0,5, și la Q04, 32 marcat deși
F3 dă 31.
→ o singură sursă pentru toată coloana, plus recitirea îngroșărilor.

### E4. Aritmetica discului din §2.4 se contrazice cu propria premisă

§2.4: „F2 (2,0 GB tabelă + ≈1,4 GB indexi după iterație) + `Tranzactie` (283 MB)
+ `Sold` ≈ 4,5 GB". Măsurat acum (`review/05-sold-per-granita.sql` §3): f2 =
1.441 MB indexi, f3 = 1.952, f1 = 1.752. Cu 1,4 GB totalul e 2.008 + 1.441 + 283
+ 123 ≈ **3,85 GB**, adică 1,7× F0, nu ~2×. Cei 4,5 GB vin din 2.132 MB
pre-iterație, rămași în tabelul de mărimi din `rezultate.md`, care nu mai descrie
starea declarată finală. În plus F0 e socotit cu o graniță de snapshot iar cubul
cu `Sold`; dacă F0 primește cele zece decembrii pe care `Referinte` le
materializează, ajunge la ≈3,4 GB.
→ ambele părți se recalculează pe același număr de granițe, iar tabelul de mărimi
din `rezultate.md` se aduce la starea finală.

### E5. Reconstruibilitatea: patru probe din treisprezece sunt tautologice, nu două

§3: „Două s-au comparat contra unei variante F0 cu regula cubului". Sunt patru:
Q02 (`f0/q02b.sql`), Q04 (`f0/q04b.sql`), Q05 și Q11 (`pg_temp."_AtomiPartener"`,
`pas2/egalitate.sql:40,63,129,148,264`). Toate folosesc exact expresia
`COALESCE(CASE WHEN ClrType IN ('Partener','Angajat') …)` din
`pas1/02-transform.sql` §3. Egalitatea probează că transformarea se re-execută
identic — proprietate utilă, dar slabă — nu că se reconstruiește raportul de azi.
La Q05, sursa reală a lui F0 (`SolduriPerioadaContabil`) nu e comparată niciodată.
→ §3 spune „patru", și separă explicit „transformare fidelă" de „reconstruiește
raportul de azi".

### E6. Q03 nu e „rând cu rând", iar două coloane ale fișei nu există în cub

Proiecția de egalitate compară `(Data, Sens, Debit, Credit, DocumentId)` și
ordonează după exact aceleași cinci coloane: potrivire de multiset, nu de rând.
Peste excluderile declarate (`SoldCurent`, `Contrapartida`), modelul de măsurare
nu are nici `Storno`, nici `NumarNota`, pe care fișa de azi le proiectează
(`f0/q03.sql`).
→ ambele intră în lista de excluderi, cu motivul „coordonata nu există în cub";
§2.5 le adaugă la „ce nu s-a putut proba".

### E7. BRIN a fost respins pe forma de interogare de DINAINTE de `Sold`

`pas2/indexi-f2.md`: „singura interogare condusă de `Data` din lot (Q12) EXTRAGE
rândurile unei ferestre înguste, unde tid-urile exacte ale unui b-tree bat un
bitmap lossy". Corect pentru lotul fără `Sold`. Dar §4.3 recomandă `Sold`, și
atunci fiecare balanță devine o fereastră de un an peste zece: Q01+S și Q02+S
intră deja pe C3 `(Data)`, 94 MB la ×10, cu `correlation` 0,9996 pe Contabil.
→ respingerea se reevaluează pe forma RECOMANDATĂ, nu pe cea de azi; de deschis
ca FZ-r.

### E8. Q12 (GLE): nota ‡ explică doar o parte din cei 89×

`pas2/cub/q12.sql` face `JOIN cub."Tranzactie"` (2,5 M rânduri la ×10) pentru
fiecare postare a lunii; F0 aduce antetele printr-o a doua interogare, în C#.
Cubul putea face la fel. Cei 276 ms măsoară o alegere de scriere a interogării
plus dublarea rândurilor, nu forma fizică.
→ ‡ se completează cu „și join-ul pe `Tranzactie`, evitabil".

---

## MINOR

**N1. Numerele de postări la ×10 se contrazic între fișiere.** 11.557.253 și
11.564.018 în `pas1/out/marimi-x10.txt`, 11.566.669 și 11.561.972 în
`pas2/out/marimi.txt`, 11.562.545 în `rezultate.md`. Primele sunt `reltuples`,
estimări de ANALYZE. Exact, prin `count(*)`: 6.975.448 + 2.772.457 + 1.814.640 =
**11.562.545**. La fel, `cub."Sold"` numără 5.365.020 acum față de 5.365.168 în
`rezultate.md`. → o cifră exactă, o dată, cu mențiunea că restul sunt estimări.

**N2. §2.3 amestecă scările.** „la 31.12.2025 are 321.199 rânduri pe Contabil" e
cifra de la ×1; pe `.x10` aceeași graniță are 389.713. Fraza stă într-un paragraf
de ×10 și se compară cu 184.780, care e tot de la ×1.

**N3. Transformarea nu filtrează `GCRecord`.** `pas1/02-transform.sql` citește cele
trei registre fără `GCRecord = 0`, deși fiecare interogare F0 îl filtrează
explicit. Inofensiv aici: 0 rânduri cu `GCRecord <> 0` pe RegistruContabil
304.382, RegistruStoc 283.498, RegistruTva 90.732 și Imperecheri 44.448
(`review/05-sold-per-granita.sql` §6). Dar egalitatea nu acoperă o bază cu
ștergeri logice. → de declarat ca limită a probei.

**N4. Scrierea: „~12 % mai ieftin" e sub pragul de zgomot.** F2 cu FK 4,28 ms
(4,14–4,98) contra F0 4,88 (4,63–5,47): intervale suprapuse, iar pragul stabilit
la M4 e de 6–12 %. Separat, §5 dă 82 % ponderea FK din mediana caldă, iar
`note-main.md` dă 33 din 57 ms, adică 58 %, din rularea rece a explorării 01. → o
singură cifră, cu regimul ei declarat.

---

## Ce rămâne în picioare din sinteză

- **F2 peste F1**, cu cifră: scriere 1,47 contra 6,55 ms la ×1, 1,75 GB de index
  EF-style, lipsa integrității pe coloana polimorfă.
- **F2 peste F3** pe interogările lunii, și motivul plauzibil (`Append` peste 12
  sub-partiții peste un index pe `Data` care taie deja luna).
- **Respingerea indexului per coordonată** și a index-atelierului pe `Cont`:
  planificatorul nu l-a atins la ×10, iar 4111 ține 144.241 din 697.660 de postări.
- **Q04 (−96 %) și Q09b** ca victorii reale ale cubului, cu plan explicit.
- **Partiționarea pe spațiu cumpără integritate** pe `Unitate`: argument de
  structură, independent de orice cifră contestată aici.
- **Constatările colaterale din §5**, toate verificabile: artefactul de dată al
  împerecherilor, `IX_RegistruTva_PerioadaAn_PerioadaLuna` nefolosit din cauza
  predicatului aritmetic, capcana `pg_get_indexdef … ON ONLY`.

**Ce NU e susținut de lotul măsurat:** verdictul „F2 bate sau egalează F0 pe
citiri", câștigul lui `Sold` față de F0, cifrele de scară ale coloanei +S, și
validarea amendamentului 4.
