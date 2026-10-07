# TR-D9a, pasul 8 — închiderea

Data: 2026-10-07. Execuție: main, direct. Contractul: D9-D15, rândul 8 și regula de oprire; D9-D13, D9-D14;
amendamentul 1 (D9-A1, D9-A9). Decizia: [`110`](../decizii/110-taierea-cubul-singurul-scriitor.md).
Execuția inițială a pasului nu a atins cod de produs: scena `PerfCub` și rețetele de probă.
Corecturile ulterioare ale review-ului ating și `Citiri.Invarianti.VerificaPerechi` din Module.

Felia e închisă (2026-10-07): review-ul advers Codex e închis (§11), owner-ul a acceptat proba PerfCub pe clone diferite ca abatere de la regula de oprire 6 și a aprobat decizia 110.

## 1. Ce s-a rulat

Codul probat: `46916d9` (pasul 7c) plus corectura scenei `PerfCub` din acest pas. Bazele sunt create la zi din
`InitialCreate`.

| Probă | Baza | Rezultat | Urma |
|---|---|---|---|
| Integrala ModelCheck, privat | `Atlas.Conta.ModelCheck.Privat`, creată de ModelCheck | **4.803 OK**, zero FAIL | `run-nucleu/tr-d9a/pas8/final-privat.log` |
| Integrala ModelCheck, bugetar | `Atlas.Conta.BackOffice`, recreată | **3.524 OK**, zero FAIL | `final-bugetar.log` |
| Nucleu | — | 190/190 | pasul 7c |
| `--probe-sursa` | — | 11/11 | `pas7b/7c-probe-sursa.log` |
| `--dump-integritate-tph` | ambele baze de mai sus | 111 interogări, zero încălcări | `pas8/tph-*.out` |
| `verifica:drift` | — | zero | `pas8/drift.log` |
| `refuzuri.ps1` pe host viu | `Atlas.Conta.BackOffice.Privat.D9P8`, nouă din seed | 318/318 de două ori la rând | `run-verificari/d9-pas8-http/` |
| `neexpunere-cub.py` | aceeași | 0 FAIL | la fel |
| `explicatii.py` | aceeași | 9 PASS, cu SC-CIT-111 | la fel |
| `PerfCub`, termenul B | clonele `.D9P8` ale celor două baze | 1.540 privat / 1.010 bugetar OK, zero FAIL; DUK exit 0 | `run-verificari/perf-cub-20261007-145947/` |
| Browser: operarea și lista | `.Privat.D9P8`, host Blazor pe 5091 | factură operată din interfață; lista arată postările ei | `run-verificari/d9-pas8-http/ui-postari.png` |
| Import1C, ianuarie | `Atlas.Conta.Import1C.Flax.D9P8`, nouă | 15.232 importate, zero eșecuri de import; 10 rânduri FAIL de reconciliere | `run-verificari/d9-pas8-import/` |

Rețeta HTTP nouă: `run-verificari/d9-pas8-http.ps1` (`-Cod` = arborele din care se ia codul, `-DoarBaza`,
`-FaraBaza`). Cere o bază care nu există și nu șterge nimic.

## 2. Numărul de `Check`, reconciliat

| Pas | Privat | Bugetar | Diferența | Unde e socotită |
|---|---|---|---|---|
| linia de bază duală (5c) | 4.862 | 3.554 | — | `tr-d9-pas5c-imbinare-partide.md` |
| 6, tăierea | 4.763 | 3.481 | −99 / −73 | verdict nominal pe fiecare aserție: `run-nucleu/tr-d9a/pas6/lucru.tsv` |
| 6b | 4.774 | 3.495 | +11 / +14 | 14 Check-uri `D9-A10`, SC-PLT-08; numai adaos |
| 6b, D9-6B-R1 | 4.784 | 3.505 | +10 / +10 | SC-PLT-09, SC-INC-09 |
| 6c, spargerea | 4.784 | 3.505 | 0 | secvență identică |
| 7 | 4.791 | 3.512 | +7 / +7 | `STR-VIZUAL-*` |
| 7b | 4.795 | 3.516 | +4 / +4 | cei doi mutanți ai perechii, pe ambele apariții |
| 7c | 4.803 | 3.524 | +8 / +8 | SC-CIT-111 |
| 8, pe bazele finale | 4.803 | 3.524 | 0 | — |
| corecturi D9-F-R1…R4 | 4.821 | 3.542 | +18 / +18 | 14 execuții de mutanți noi + 4 aserții SC-CIT-111-IZOLARE; §10 |
| review-ul corecturilor | 4.828 | 3.549 | +7 / +7 | curățenia refuzului de seed în SC-X-26 (1) și cinci mutanți noi ai perechii (6 execuții); §11 |

După pasul 6 nicio aserție n-a dispărut: fiecare comparație (`pas3/scripts/compara.py`) dă zero linii dispărute.

## 3. `PerfCub` A/B (regula de oprire 6)

Termenul A e din regimul dual (`run-verificari/perf-cub-20261006-161201`, cu a doua mostră
`…-164756` pentru zgomot). Termenul B folosește aceeași rețetă și aceiași parametri (m = 0, 6, 12; k = 1, 4, 16,
64; 16 unități). **Abaterea de la „aceeași bază"**: clonele `.D9P5b` ale termenului A sunt pe schema dinaintea
tăierii și nu pot purta codul de azi; B rulează pe clone noi, `.D9P8`, ale bazelor recreate din același seed.
Scena e construită de unealtă în ambele cazuri, cu aceleași numere de execuții pe fiecare comandă. Tabelul
complet: `run-nucleu/tr-d9a/pas8/perfcub-AB.md`.

**Stare după D9-F-R4:** proba alternativă a fost prezentată ca abatere de la „aceeași bază”, nu declarată
îndeplinită. Owner-ul a acceptat-o explicit la 2026-10-07, cu limita ei: baza fizică diferă, scena și cifrele
de control sunt aceleași.

**Criteriile de formă se păstrează**: zero FAIL pe ambele profiluri; `PREST-NI` rămâne `AMÂNAT` (F27-r16), ca
în A. Cifrele de control ale cititorilor sunt identice cu A pe toate cele 784 (privat) și 496 (bugetar) de
puncte, iar cardinalul la fel.

**Costul comenzii fără registre.** Comenzi SQL pe comandă (identice la m = 0, 6, 12, cu excepția AMO la m = 0):

| comandă | privat A → B | bugetar A → B | durata mediană, B față de A |
|---|---|---|---|
| FCT | 80 → 64 | 63 → 55 | −12 … −18 % |
| FCL | 66 → 53 | 52 → 44 | −14 … −18 % |
| NIR | 67 → 57 | 65 → 55 | −16 … −21 % |
| BCS | 50 → 40 | 50 → 40 | −20 … −24 % |
| BCS lung | 113 → 103 | 113 → 103 | −20 … −35 % (o execuție; zgomot până la 51 %) |
| ASM | 53 → 43 | 53 → 43 | −16 … −19 % |
| BTR | 47 → 38 | 47 → 38 | −17 … −21 % |
| DSC | 56 → 49 | — | −15 … −17 % |
| AMO | 89 → 79 | 91 → 81 | −6 … −15 % |
| PLT | 43 → 37 | 45 → 39 | −11 … −16 % |
| INC | 43 → 36 | 45 → 38 | −13 … −17 % |
| împerechere | 29,5 → 27,5 | 29,5 → 27,5 | −2 … −7 % (în zgomot) |
| închidere | 68 → 68 | 50 → 50 | −8 … −12 % |

Nicio pereche comandă × m nu are mai multe comenzi SQL după tăiere: 36 din 38 pe privat și 33 din 35 pe bugetar
au mai puține, restul (închiderea) același număr. Zgomotul duratei între cele două mostre A e sub 6,2 % pe
comenzile cu cel puțin 64 de execuții; scăderile de 11–24 % ale comenzilor de document sunt peste el, cele
ale împerecherii și ale închiderii nu.

**Cititorii.** Numărul de comenzi SQL e neschimbat pe toți cititorii, cu două excepții care sunt predicții
de scriere: `FIFO-N` 31 → 22 și `PDISP-N` 25 → 18. `PREST-NV` și `PREST-SV` citesc mai puține rânduri din
`Postare` la m = 6 (3.894 față de 5.533 pe privat): e efectul îmbinării corectate la pasul 5c, după măsurarea
lui A. Cititorul SAF-T L citește cu 19 rânduri mai puțin, constant în k și m, cu același cardinal: sunt
rânduri de nomenclator, nu de cub.

Două observații, raportate, nu normalizate:

- pe punctele calde cu o singură execuție, durata are un salt de circa 25 ms care cade pe alte puncte în B
  decât în A, cu timpul SQL neschimbat (de pildă `FISA-SV` la k = 64: 24 → 47 ms cu 7,4 ms de SQL în ambele;
  `SAFTS-N` la k = 64: 70 → 46 ms). Sunt 33 de puncte în sus și 11 în jos pe privat. Nu e al bazei de date;
  cauza din proces nu e stabilită. Criteriile de formă nu judecă durata absolută;
- pe bugetar, la m = 12 și k = 64, trei cititori de partide citesc cu 24–26 de rânduri de index mai mult
  (4.662 față de 4.636), cu cifrele de control și cardinalul neschimbate. Neatribuit.

**Verificările scalei.** A: 1.541 / 1.011 OK; B: 1.540 / 1.010 OK. Cincisprezece linii au dispărut și
paisprezece au apărut pe fiecare profil. Dispărute: reconcilierea integrală (a)–(g) și diagnosticul ASM-B7 pe
baza de volum (câte trei), mutanții acoperirii registru → cub (`DESCHIDERE`, `DESCHIDERE-EGALA`, `IMO-FISA`,
`IMO-REGISTRU`, `POLITICA`), cele trei probe X-D2 pe registre și forma veche a aserției de repartitor. Apărute:
proba numelor interzise (două), X-D2 pe legătură (două), mutanții valorii liniei și ai semnului ieșirii
(șapte), cei doi ai perechii și aserția de repartitor rescrisă pe capăt.

**Scena `PerfCub` nu mai rulase de la tăiere.** Integrala nu rulează scala, iar prima rulare de la pasul 8 a
căzut la construirea scenei, pe schimbarea declarată 2: cele trei asamblări 1 + 1 + 1 din lotul de
3 × 3,333333 declarau produse de 3,33, 3,33 și 3,34, pe când consumul evaluat e 3,33, 3,34 și 3,33. Sub
regimul dual diferența se absorbea; acum e `ASAMBLARE_NEBALANSATA`. Scena declară acum P = C
(`nou/tools/ModelCheck/PerfCub.cs`). Numărul de documente și de postări al scenei e același.

**D9-D10 (b)** rămâne re-amânat explicit de owner, cu cifra (2,7 s de SQL pentru 488.733 de partide cu rest la
5 milioane de postări), până la decizia 111.

## 4. HTTP pe host viu

Bază privată nouă din seed, WebApi izolat pe 5089. `refuzuri.ps1`: 318 probe, 318 PASS, de două ori la rând,
cu starea bazei neschimbată între rulări (0 documente, 0 tranzacții, 4 roluri, 4 utilizatori).
`neexpunere-cub.py`: `Postare`, `Tranzactie` și `PostareVizual` n-au rută și nu apar în `openapi.json`.
`explicatii.py`: explicația pe ușa HTTP, cu refuzurile ei de acces și cu proba SC-CIT-111 — regula de contare
editată de două ori prin `PATCH api/odata/RegulaContare(id)` are contorul curent +2, iar explicația bonului
operat înainte o arată „schimbată".

## 5. Browser

Pe aceeași bază, host Blazor pe 5091, utilizatorul `Admin`. Un draft de factură de intrare (serviciu 100 lei,
TVA 21 %) creat prin API a fost deschis în ecranul XAF și operat cu butonul „Operează", cu confirmarea din
dialog. După operare: starea „Operat"; „Operează" dezactivat cu motivul „Documentul e deja operat."; „Anulează
operarea", „Stornează" și „Corectează" active. Avertismentul afișat, `TVA_IN_AFARA_INTERVALULUI`, e cel
așteptat: documentul e datat în 2012, în afara intervalului tipului de TVA.

Lista „Registre → Postări" (`PostareVizual_ListView`) arată 16 rânduri, cu coduri în loc de identificatori și
fără acțiuni de creare sau ștergere: cele 12 ale probei HTTP (recepția cu gestiunea virtuală „Furnizor",
bonul și stornoul lui) și cele 4 ale facturii operate din browser — 628 D 100 / 401 C 100 și 4426 D 21 /
401 C 21, cu partida `P8-F-…/2012-01-15` pe 401. În bază, cele patru postări poartă perechile 1 și 2, iar
tranzacția are explicația în formatul 2.

De știut: coloana `Pereche` nu e între coloanele implicite ale listei. Textul confirmării spune în continuare
„Se vor scrie registrele"; „Registre" e și numele grupului de navigare al listei (D9-A12), deci l-am lăsat.
Proba a mers prin Chrome DevTools, într-un context izolat; extensia Chrome nu era conectată.

## 6. Import1C, rularea-diagnostic pe ianuarie

Prima rulare a conectorului pe cititorii cubului (portarea de la pasul 5 era probată numai prin compilare).
`--recreeaza --cititori --pana-la 1`, într-o bază nouă, `Atlas.Conta.Import1C.Flax.D9P8`; `.Flax.R3f`, baza
ultimei reconcilieri registre ↔ cub, a rămas neatinsă. Termenul de comparație e rularea dinaintea tăierii pe
aceeași sursă (`run-verificari/r3-ian-final2/`, 2026-10-04).

| | Înaintea tăierii | După |
|---|---|---|
| documente importate / sărite | 15.232 / 15 | 15.232 / 15 |
| copii autogenerate operate | 1.365 | 1.365 |
| eșecuri de import | 0 | 0 |
| tranzacții / postări | 18.727 / 82.144 | 18.713 / 82.142 |
| `INV-CUB` pe baza integrală | verde | verde, cu invariantul perechii și cu explicațiile în formatul 2 |
| stingeri pe partide inițiale refuzate | 37 | 39 |
| rânduri FAIL de reconciliere | 9 | 10 |

Pe contracte (`run-verificari/d9-pas8-import/diff-fata-de-r3f.txt`):

1. **Sold per cont: 3 conturi fără explicație, față de niciunul.** 371 (−9.674,37), 3028 (+9.316,01) și 303
   (+358,36). Sunt exact sumele constatării A-1 din pasul 5b: reclasificarea 371 → 3028 / 303 e postată o dată
   de ASM-ul `SED#-R`, pe loturi, și a doua oară de nota-punte a conectorului, `SED#-P`. Schimbarea cu nume
   (109 e): contractul 1 citea `RegistruContabil`, în care ASM-ul nu posta; de la pasul 5 (`8be5b8b`, 109-r1)
   citește cubul. Nu e a motorului: puntea cade pentru ASM-urile care postează (110-r2, migrare).
2. **Închiderea de TVA**: neschimbată.
3. **Stoc pe produs × gestiune**: zero chei nejustificate, ca înainte (15.564 de chei comparate față de
   15.569). Categoriile de justificare s-au redistribuit (379 de chei justificate față de 307), fiindcă
   termenul comparat e acum stocul cubului, nu registrul de stoc, care diferea de el.
4. **Deriva de rotunjire**: −0,08 lei pe 8 chei, față de 0,01 pe una; pragul e 1,47.
5. **Partide inițiale: 5 fără explicație, față de 8**, pe alte partide decât înainte (401 × 3, 4111 × 2).
   1.472 stinse integral (1.471), 38 explicate de refuzuri (36). **Neatribuit unei schimbări cu nume**:
   candidații sunt alegerea structurală a piciorului de terț în trezorerie (pasul 6b, `ea5d0bc`) și portarea
   conectorului (pasul 5). Diagnosticul se urmărește în felia de migrare (091-r4, 107-r9); cauza rămâne neatribuită.
   Datele de mai sus nu exclud o contribuție a schimbărilor motorului.

Limita din `limite-curente.md` se actualizează: portarea nu mai e „numai compilată", dar nu e nici verde.
Import1C rămâne unealtă de migrare, nu gate (091).

## 7. Documentația (D9-D14) și restanțele (D9-D13)

- `stare-curenta/`: ultimele două mențiuni ale scrierii duale (rândurile PIF și AMO din
  `domeniu-si-operare.md`) au ieșit. Invarianții I, III și VI sunt în literă curentă de la pasul 6.
- `CLAUDE.md`, §Stare: rescrisă ca stare. Paragraful nucleului nu mai descrie regimul dual, iar povestea pe
  pași a feliei a ieșit; rămâne în `istoric-plan-de-lucru.md` și aici.
- `restante.md`: verdictul din D9-D13 pe fiecare rând; nicio restanță nu mai are starea „cade la TR-D9".
  Opt rânduri noi: D9-r1, D9-r3, D9-r4 și 110-r1…110-r5.

## 8. Bazele

Create în acest pas, niciuna ștearsă:

| Bază | Rost | Se poate șterge |
|---|---|---|
| `Atlas.Conta.BackOffice`, `Atlas.Conta.BackOffice.Privat`, `Atlas.Conta.ModelCheck.Privat` | bazele de dezvoltare, recreate din `InitialCreate` (cu `Pereche`) | nu |
| `Atlas.Conta.BackOffice.P7b`, `Atlas.Conta.ModelCheck.Privat.P7b` | integralele pașilor 7b și 7c | da |
| `Atlas.Conta.BackOffice.D9P8`, `Atlas.Conta.ModelCheck.Privat.D9P8` | `PerfCub`, termenul B | da |
| `Atlas.Conta.BackOffice.Privat.D9P8` | HTTP și browser; are documentele probelor | da |
| `Atlas.Conta.Import1C.Flax.D9P8` | importul pe ianuarie la codul de azi | rămâne baza de import la zi |

Bazele vechi rămase pe schema dinaintea tăierii: `.Flax.R3f`, `.Flax.M1s`, `.Flax.R3`, `.D9P5b` (două),
`.D9Vol`, `.D9VolProba`, `.D9P7` (trei), `.Privat.D9P4`. Termenul B fiind măsurat, `.D9P5b` nu mai e
necesară. Ștergerea lor e a owner-ului.

## 9. Ce rămâne

Hotărâte la 2026-10-07: review-ul advers Codex e închis (regula de oprire 8), owner-ul a acceptat abaterea
PerfCub de la „aceeași bază” (regula de oprire 6) și a aprobat decizia 110; merge în main prin PR #22.

- Întrebările owner-ului din decizie: B-r3, 110-r1 (`PastreazaSemn`), 110-r3 (`Guid.Empty`).
- Purja politicilor FCT după `DinSeed` în `ScenariiTvaIntervale` și `ScenariiFiscale`; `explicatii.py`, care
  editează pe HTTP regula reală din seed (§11).


## 10. Corecturile review-ului D9-F-R1…R4 (2026-10-07)

Worktree `d9-pas7b`, peste `aecd189`, fără commit. Invariantul perechii verifică forma și
multiplicitățile transformărilor fără ordinal; SC-CIT-111 folosește exclusiv politica proprie,
cu purjarea refuzului ei de seed și probe de izolare/recuperare. Condiția 6 rămâne abatere de
acceptat, iar atribuirea diferențelor Import1C contractul 5 rămâne deschisă.

| Probă | Rezultat | Manifest/loguri în worktree |
|---|---|---|
| CITIRI + ASM, ambele profiluri | exit 0 | `run-verificari/20261007-205021-699/rezultat.json` |
| Integrala finală, bugetar | **3.542 OK, 0 FAIL** | `run-verificari/20261007-205913-703/integral-bugetar.log` |
| Integrala finală, privat | **4.821 OK, 0 FAIL** | `run-verificari/20261007-205913-703/integral-privat.log` |
| Nucleu | **190/190**, 0 eșuate, 0 omise | `run-verificari/20261007-210544-728/rezultat.json` |

Integrala: manifest `run-verificari/20261007-205913-703/rezultat.json`, exit 0;
SHA-256 ModelCheck.dll `24E6CCEA3983894AA689263DCCD1F361278E9CC82E00FF6CAE3D9061AC660166`.
Diferența față de review este +18 aserții pe fiecare profil: cei șase mutanți de predicate
apar de două ori (12), cei doi de transformare o dată (2), izolarea politicii adaugă 4.
Nu s-a eliminat nicio aserție; aserția veche de refacere a seed-ului verifică acum curățenia
politicii proprii. `SC-DES-21` este verde în rularea finală, fără modificarea sursei sale.
`git diff --check`: fără erori.

Comenzile de validare, din worktree:

```powershell
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip CITIRI,ASM -Profil Ambele -Sufix .CodexD9Fix -PregatesteBaze
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip CITIRI,ASM -Profil Ambele -Sufix .CodexD9Fix
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexD9Fix
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Nucleu
```

Prima comandă a pregătit bazele, apoi a eșuat la compilarea conversiei `Guid?` → `Guid`
din helperul BCS nou (`20261007-204935-806`); corectată înaintea scenariilor.
Prima integrală (`20261007-205233-530`) a avut 3.542 OK bugetar și 4.820 OK / 1 FAIL privat:
`SC-DES-21`, observația blocajului a surprins textul `BEGIN TRANSACTION ISOLATION LEVEL READ COMMITTED`.
Rezultatul operației concurente a trecut. Proba citește `pg_stat_activity` și `pg_blocking_pids`;
un dezacord tranzitoriu al observațiilor este plauzibil, dar nu demonstrat. Rularea finală
recompilează și mutanții ordinal/latură rafinați să izoleze predicatele lor; eșecul anterior
rămâne raportat, nu este eliminat din evidență.

Bazele folosite: `Atlas.Conta.BackOffice.CodexD9Fix` și `Atlas.Conta.ModelCheck.Privat.CodexD9Fix`.
HTTP/browser/PerfCub/import și scara de volum nu au fost rerulate; dovezile §1 rămân cele inițiale.
Nu s-au schimbat scriitorul cubului, API-ul sau schema. Predarea cere reverificarea owner/Claude
înainte de commit; nu închide singură review-ul.

## 11. Review-ul corecturilor (Claude, 2026-10-07)

Corecturile D9-F-R1…R4 țin. Reverificate independent pe clone noi `.ClaudeD9Rv`: 4.821 privat / 3.542 bugetar,
zero FAIL (`run-verificari/20261007-213212-607/`). Proba separată a invariantului perechii, prin funcția reală
`VerificaPerechi` pe date în memorie: 45 din 45 (`run-verificari/claude-d9-corecturi-review/Program.cs`); ea nu
verifică traducerea EF.

Review-ul a găsit două lucruri din aceeași clasă cu R2/R3, în afara fișierelor corectate, și trei observații
mici. Toate sunt aplicate în aceeași schimbare:

- **SC-X-26** (`ScenariiTaiere`) purja orice regulă `!DinSeed` pe BTR, iar ștergerea prin gardian lăsa un
  `RefuzSeed` pe care nu-l curăța nimeni. Regula stă acum pe un tip de material propriu
  (`E2E-SC-TAI-REGULA`), iar curățenia e cea a SC-CIT-111, mutată în `ScenaDocumente.CurataPolitica`. O aserție
  nouă cere ca refuzul lăsat de ștergere să fie scos și cele preexistente să rămână identice.
- **Bazele de dezvoltare** purtau refuzul vechiului SC-CIT-111 (BCS / tipul de stoc) și pe cel al SC-X-26
  (BTR, natura Stoc). Cele două rânduri din fiecare bază au fost șterse nominal
  (`run-verificari/claude-d9-corecturi-review/sterge-refuzuri.log`). Clonele `.CodexD9Fix` și `.ClaudeD9Rv`,
  făcute înainte, le păstrează.
- Ținta mutanților noi ai perechii e ordonată după `ID`.
- Invariantul ia gestiunile virtuale din `GestiuniVirtuale.Toate` (nucleu); `Este` se sprijină pe aceeași listă.

La cererea owner-ului, ramurile predicatului perechii rămase fără mutant pe PostgreSQL au primit câte unul.
Un mutant strică datele, nu codul: un singur fapt al cubului, într-o tranzacție anulată, după care invariantul
trebuie să refuze.

| Mutant | Stricăciunea | Disjuncția |
|---|---|---|
| `PERECHE-DOCUMENT` | alt `DocumentId` pe o postare a unei perechi de operare | `Documente != 1` |
| `PERECHE-TRANSFER-LATURA` | latura întoarsă pe o postare a unui transfer, cu valorile egalate | `Transfer && Laturi != 1` |
| `PERECHE-TRANSFER-VALOARE` | +1 la valoarea unei postări a unui transfer | suma valorii pe aceeași latură |
| `PERECHE-TRANSFER-VALUTA` | +1 la valoarea în valută a unei postări a unui transfer | suma în valută pe aceeași latură |
| `PERECHE-DESCHIDERE` | o postare a deschiderii primește ordinal și o contrapartidă clonată, cu sumele opuse | `Fel == Deschidere` |

Izolarea e probată și invers: cu câte o disjuncție scoasă din predicat, pe scenele DESCHIDERE, BTR și BCS
(privat), supraviețuiește exact mutantul ei, iar ceilalți 12 mutanți ai perechii rămân uciși. Rețeta e
`nou/tools/ModelCheck/scripts/izolare-mutanti.ps1`: fiecare variantă trece prin `verifica.ps1`, iar rezultatul
așteptat e scris pe variantă. Starea fiecăruia dintre cei 13 mutanți executați e cerută exact: numai FAIL pentru
cel așteptat, numai OK pentru ceilalți; un FAIL urmat de OK e refuzat (IZ-R1). În varianta fără ramura de latură
a transferului e așteptat și `N-r8`, proba de sursă care cere mențiunea `FelTranzactie.Transfer` în
`VerificaPerechi`; e efectul experimentului, nu o mascare. Rețeta ia blocajul verificărilor înaintea citirii
sursei și îl ține până după restaurare și rularea predicatului întreg; fără blocaj, sursa și binarele rămân
neatinse (IZ-R2). Rulată: exit 0, `run-verificari/claude-d9-corecturi-review/izolare-mutanti.log`, manifestele
`20261007-232002-349` … `20261007-232230-055`. Judecata ieșirii are proba ei fără bază (`-ProbaClasificator`,
12 cazuri), iar refuzul sub blocaj străin e în `proba-blocaj.log`, alături de jurnal.

| Probă | Rezultat | Manifest/loguri în worktree |
|---|---|---|
| Nucleu | **190/190** | `run-verificari/20261007-220942-442/rezultat.json` |
| Integrala după SC-X-26, `OrderBy` și `Toate`, clone noi `.ClaudeD9Rv2` | 3.543 bugetar / 4.822 privat OK, 0 FAIL | `run-verificari/20261007-220946-928/` |
| Integrala finală, cu cei cinci mutanți, bugetar | **3.549 OK, 0 FAIL** | `run-verificari/20261007-223433-979/integral-bugetar.log` |
| Integrala finală, cu cei cinci mutanți, privat | **4.828 OK, 0 FAIL** | `run-verificari/20261007-223433-979/integral-privat.log` |
| `RefuzuriSeed` după rulare, în clone și în bazele de dezvoltare | zero rânduri | interogare numai de citire |

Cele +6 execuții: `PERECHE-DOCUMENT` de două ori, ceilalți patru o dată.

Rămâne deschis: `ScenariiTvaIntervale` și `ScenariiFiscale` purjează politicile FCT `!DinSeed` cu un filtru
necorelat; nu sunt atinse aici. `explicatii.py` editează pe HTTP regula reală din seed, pe o bază de unică
folosință.
