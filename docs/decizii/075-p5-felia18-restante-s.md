# Decizia 75 — Pasul 5, felia 18 — restanțele grele ale lui S: golirea valorică (motor), reclasificarea ca mișcare, perf proiecție

- **Data**: 2026-08-29
- **Stare**: activă (închide 74-r4, 74-r6, 74-r9; amendează 45e cu limitele
  declarate ale derivei rămase)
- **Rezumat durabil**: `CLAUDE.md` §75
- **Docs**: docs/api/p5-felia-restante-s-contract.md (D18-D1…D4, riscurile
  1–10, D18-V1…V4, review-ul advers F1–F10, §Închidere pașii 1–4),
  docs/import/faza-1c-design.md §15

---

**Pasul 5, felia 18 — executată** (4 pași + review advers cu fix-uri, un
agent per pas; commit-uri: `bc8f8ea`/`ba233f6` pas 1, `7436988` pas 2,
`06f9fc4` pas 3, fix-urile F1–F10 + proba finală odată cu decizia). Cele trei
restanțe grele lăsate de F17: perf-ul proiecției S (74-r4), reziduul valoric
per lot al golirilor (74-r6) și dubla creditare 381 din reclasificările la
transfer (74-r9). Decizii de intrare (utilizatorul): r6 se rezolvă în MOTOR,
nu în import; r9 = reclasificarea devine mișcare de stoc; o singură re-rulare
Flax, la final.

## D18-D1 + perf (74-r4) — închisă CU CIFRA, nu cu ținta

`SaftProiectii.AgregatStoc` = o singură trecere peste `RegistruStoc` (era
trei), cifre identice (XML diff = doar `SoftwareVersion`). FAPTUL care a
închis restanța: atribuirea „O(istoric)" din 74-r4 era GREȘITĂ — cele trei
scanări costau 0,27 s din 4 s (index scan, nu seq-scan), deci indexul promis
de contract nu se justifică (condiția „dacă cifra o cere" nu e îndeplinită,
59). Costul real: `ApiProiectii.CoduriTip` (materializare polimorfă cu toate
join-urile TPT, 1,4 s × 2 apeluri) ⇒ §7 trecut pe `CoduriTipPeTipuri`
(ancora `TipDocument.ClrType`, doar Guid-uri per tip, ~0,4 s pe TOATE cele
205 k documente), dicționar PARTAJAT cu `ComponenteS3`. Filtrul pe id-uri în
SQL încercat și RESPINS pe cifră (Npgsql `= ANY(@ids)` = |ids| sondări de
index: 1,4–1,8 s pe istoricul cerut de S3). Rezultat: 4,0 → 2,9 s / 4,1 →
3,0 s (mediană caldă); **ținta < 1 s NU e atinsă și nu mai există un vinovat
dominant** (§8–13 ≈ 1,2 s în felii de 0,1–0,3 s) — restanțele cu cifre: F8
(`CoduriTipPeTipuri` liniar cu baza) și `ApiProiectii.CoduriTip` pe
hot-path-ul API (75-r2).

## D18-D2 (74-r6) — golirea valorică e a MOTORULUI

Ieșirea care GOLEȘTE cheia (Lot × Repartitor × TipStoc) preia tot soldul
valoric rămas — `StocService.ValoareGolire` (pură) + `SolduriLaData` +
`AplicaValoareIesire`, chemat din `MotorOperare` DUPĂ `PregatesteOperare` și
ÎNAINTE de `ValideazaOperare` (33d); regula stă în motor fiindcă cheia și
semnul sunt ale REGULII de stoc, nu ale frunzei (42a). Frunzele rămân
previzualizarea `preț × cantitate`. Import1C: `AlocareIesire.Aloca` întoarce
valoarea prin ACELAȘI helper. Efect măsurat pe Flax: deriva per lot a
ieșirilor MOARE — `ReziduValoricFaraCantitate` în S: 861/1.168 → **131/132**
chei (rămân loturile deschiderii + limitele de mai jos).

**Limitele regulii, declarate (review F1/F5), nu corectate tăcut**:
- *Retro* (F1): golirea se decide LA OPERARE, pe registrul care EXISTĂ atunci
  (`Data ≤` + `DataOperare ≤`); un document retro operat ulterior nu
  re-decide linii deja operate ⇒ cheia poate rămâne 0 bucăți cu reziduu ≠ 0.
  Reziduul se DECLARĂ (S: `ReziduValoricFaraCantitate`; D4: „re-deschisă
  retro", cu măsurarea rândului retro). Pe Flax: 1 caz/an (aprilie, 0,01).
- *Fiscal* (F5): **RLF NU absoarbe restul** — suma returului e a hârtiei
  furnizorului (`q × preț`), baza TVA = valoarea liniei. Contract pe tipul de
  document: `IDocumentCuIesireFiscala` (marker în `Comun/Interfete.cs`,
  declarat de `ReturFurnizor`); `AplicaValoareIesire` sare documentele care
  îl declară; `Alocare.Aloca(absoarbeLaGolire: false)` prezice aceeași cifră
  în import. Reziduul rămâne pe lot, contorizat („goliri fiscale”: 400/an pe
  Flax, toate cu reziduu 0,00).

## D18-D3 (74-r9) — reclasificarea de cont la transfer = MIȘCARE de stoc

Rândul `TransferDeMarfuri` cu `contDebit ≠ contCredit` produce, în aceeași
unitate de import, în ordinea **ASM → BTR → punte** (F4: puntea transcrie
mișcarea, deci se scrie după ce mișcarea există; fără ASM operat, nici BTR,
nici punte — rândurile sursei se declară nepostate): ASM `#reclas` în
gestiunea sursei (consum lot@contVechi prin `Alocare.Aloca`, produs = lot NOU
pe produs@contNou, `PretEvaluare = Σ consum / cantitate` ⇒ invariantul 46d la
zero), NTC-punte la valoarea ASM-ului, BTR pe lotul nou doar când gestiunile
diferă. Cheia `1C:Lot` cu simbolul nou = lotul nou (canonic); `1C:LotAlias`
rămâne doar al RDC. Cazurile-gaură pe calea veche, contorizate:
`ReclasificariDejaPeContNou`, `ReclasificariFaraTipNou`,
`ReclasificariValoareZero` (ASM cere preț pozitiv — invariantul NU se
relaxează). **F9**: cheia `#reclas` cerută de sursă dar fără ASM produs se
leagă „fără document" (`Bucla.LeagaFaraDocument`, ținta `Guid.Empty` —
convenția „țintă goală"; `Executa` = skip cu motiv, `--deblocheaza` o șterge)
— altfel unitatea rămânea „parțială" la nesfârșit; pe Flax a lovit exact o
dată (cazul valoare-0, 03/2025).

Efect pe an (proba finală): 556 rânduri de punte, **552 ca mișcare** (81 chei
discriminate `{lot}~{transfer}`), **381/608 dispar integral din registrul
divergențelor** (dubla creditare 381 moare pe toate lunile), S3 pe 371:
punțile == ASM `#reclas` per cont LA CENT (component ⇒ 0), 381 absent din S3.
Riscul 8 DECIS: reclasificarea iese în S cu codul ASM-ului (20/70) — codul e
al TIPULUI × registrului (74a), iar mișcarea E o asamblare; DUK n-o
contrazice.

## D18-D4 — reconcilierea: categoria „reziduu absorbit" + ORACOLUL golirii

`ReconciliereLuna.ReziduuAbsorbit` = DEFALCARE din registru (nu categorie
aditivă): per cont, reziduul liniilor care au golit + `Δ fără reziduu`
(comparabil cu rapoartele pre-D2), luna + cumulat (`ReziduuAbsorbitCumulat`,
F7 — citește doar ieșirile lunii). **Oracolul (F2, linie de CONTRACT `1'`)**:
logica pură în MOTOR — `StocService.VerificaGoliri` peste `RandGolire`, cu
verdicte `Exacta / CuValoare / Fiscala / ReDeschisaRetro / Negolita`; cifra e
a REGISTRULUI, nu a `ValoareGolire` (necircular); perechile stornate pe
`DetaliuId` se sar (F3). Pe Flax: verde 12/12 luni, 0 goliri cu valoare
rămasă (8,7–11,3 k goliri verificate/lună).

## Review-ul advers (F1–F10) și proba finală

F1 limita retro (docs + probă D18-V2 (r)); F2 oracolul; F3 stornoul; F4
ordinea; F5 `IDocumentCuIesireFiscala`; F6 → restanță (ASM UI); F7 D4 pe
luna curentă; F8 → restanță (scalare); F9 cheia fără document; F10 docs.
ModelCheck verde AMBELE profiluri după fix-uri. **Incident de execuție**
(consemnat în contract, pasul 4): prima rulare integrală a pornit pe binarul
construit ÎNAINTE de fix-uri (sesiunea a murit pe o pană de net, fără
commit) — raportul ei nu era proba; proba finală = re-rularea pe codul
final: 28.08 23:32 → 29.08 01:25, **contract îndeplinit 12 luni, 0 FAIL**,
diff-ul vs baseline-ul F17 explicat integral (530 linii, toate atribuite
D2/D3/D4), DUK ok pe `--saft-s 2025 9/12`.

## Ce rămâne deschis (restanțele 75-r1…r5)

- **75-r1** — ASM prin UI: operatorul care evaluează produsul la `preț ×
  cantitate` pe un lot golit primește refuz pe invariantul 46d cu cenți
  (F6); derivarea automată a valorii produsului din consum rămâne de
  proiectat.
- **75-r2** — scalarea rezoluției de tip: `CoduriTipPeTipuri` listează toate
  documentele bazei per tip (0,4 s pe 205 k, liniar cu baza; alternativa =
  join pe ancoră / discriminator materializat, F8); `ApiProiectii.CoduriTip`
  are aceeași problemă pe hot-path-ul API. Se măsoară când cifra o cere (59).
- **75-r3** — `--continua` pe o bază deja importată = fals-roșu pe
  contractele 1/3 + replanificare cu efect real (`Persista` purjează
  justificările vechi); idempotența NU e probată prin el — proba validă =
  re-rulare integrală.
- **75-r4** — `PretEvaluare` la 6 zecimale: pe cantități ≥ ~10.000,
  `round(q × preț)` poate diferi cu un ban de Σ consum ⇒ invariantul 46d ar
  refuza ASM-ul `#reclas` (0 apariții pe Flax).
- **75-r5** — rândul invers al stornoului poartă `DataOperare` a
  documentului original (stornarea nu are timbru propriu) ⇒ ordinea intra-zi
  a oracolului e aproximată pentru stornouri.
