# TR-D9a — pasul 6b: repartitorul pe postare și gardul strict

- Data: 2026-10-07
- Bază: `tr-d9-taierea` la `9580b8b`; contractul
  [`tr-d9-taierea-contract.md`](tr-d9-taierea-contract.md), D9-D15, rândul
  pasului 6b; amendamentul 2, D9-A10; decizia 111 (h), forma minimală.
- Ce conține: măsurătorile cerute înaintea codului, ce a intrat în cod,
  probele, constatările pentru deciziile 110 și 111.
- Schimbarea de comportament a pasului e a opta din D9-D1: postările de pe
  piciorul de terț al conturilor cu flag-ul `Repartitor` capătă `Partener`.
  Nicio altă cifră nu s-a schimbat: diferența logurilor față de pasul 6 e numai
  adaos (§4).

## 1. Măsurătorile dinaintea codului (D9-A10, „de măsurat”)

Instrumentarea a fost temporară, în `GardAnaliza.Verifica`, pe integrala
bugetară a pasului 6 (3.481 OK): fiecare capăt din cartea contabilă pe un cont
cu flag-ul `Repartitor`, cu ce coordonată purta. Urma:
`run-nucleu/tr-d9a/pas6b/masura-bugetar.tsv` (883 de rânduri).

| Tip | Cont | Latura | Partener | Gestiune | Unitate | Postări | Documente |
|---|---|---|---|---|---|---|---|
| FCT | 401.01.00 | C | da | — / virtuală | partidă | 457 | 180 |
| FCT | 404.01.00 | C | da | — | partidă | 15 | 7 |
| NIR | 401.01.00 | C | da | virtuală | partidă | 58 | 24 |
| FCL | 411.01.01 | D | da | — | partidă | 63 | 26 |
| INC | 401.01.00 / 411.01.01 | C | da | — | partidă | 46 | 21 |
| PLT | 401.01.00 | D | da | — | partidă | 99 | 44 |
| NTC | 401.01.00 / 411.01.01 | D, C | da | — | partidă | 83 | 25 |
| NTC | 401.01.00 / 411.01.01 | D, C | — | reală (repartitor intern al liniei) | — | 62 | 27 |

1. **Rândurile de catalog ale căror postări capătă coordonata: zero.** Pe
   bugetar, conturile cu flag atinse de catalog sunt numai 401.01.00,
   404.01.00 și 411.01.01, toate cu partide (decizia 100); fiecare capăt purta
   deja partenerul (prin partidă) sau o gestiune reală (nota contabilă cu
   repartitor intern pe linie). Pe privat niciun cont din seed nu poartă
   flag-uri. Dintre cele 20 de conturi bugetare cu flag, 16 nu urmăresc
   partide (401.02.00, 411.02.08, 442.08, 442.08.01, 437.01/02, 458.x,
   462.01.01, 462.01.09, 552.00.00, 774, 803.00.01, 804.90.00, 805.00.00) și niciunul nu
   e atins de catalog. Coordonata apare numai în scena nouă SC-PLT-08
   (462.01.09 bugetar; 462 privat cu flag pus în scenă).
2. **Cititorii care grupează pe partener sau pe gestiune** (balanța analitică,
   `SoldParteneri`, fișa contului, SAF-T, partidele): nicio cifră nu se
   schimbă, fiindcă nicio postare existentă nu capătă coordonata. Proba e
   integrala: aceleași nume, aceleași cifre, numai adaos (§4).
3. **Invarianții**: nucleul cere partener numai pe partidă (`PARTENER_LIPSA`,
   `UNITATE_NEPOTRIVITA`) și interzice partenerul numai pe contraponderea
   transformării; `INV-CUB` grupează faptele fiscale pe partener. Niciunul nu
   presupune „partener numai cu partidă” sau „gestiune numai cu stoc”.
4. Oracolul nu mai există (pasul 6); n-ar fi putut proba coordonata.

Concluzia măsurării: gardul strict nu refuză niciun document de catalog
acceptat înainte (regula de oprire 12), iar schimbarea 8 e, pe seed,
o promisiune pentru conturile cu flag fără partide, probată sintetic.

## 2. Ce a intrat în cod

- **`ContFapt.CereRepartitor`** (`Declaratii/Operand.cs`): flag-ul
  `Repartitor` al contului, citit o singură dată în `Fapte.Conturi`; cele două
  locuri care mai construiau `ContFapt` de mână (`ReceptiiConexe`,
  `ImobilizariFapte`) trec prin aceeași funcție.
- **Helperul capătului de terț** (`Declaratii/Partide.cs`, clasa `Terti`):
  `Terti.Capat(operand, capăt, terț, partidă)` pune partida și partenerul când
  contul o urmărește, altfel numai partenerul când contul cere repartitor; nu
  atinge capătul cu gestiune reală (e intern), capătul din cartea fiscală sau
  un partener deja pus (faptul fiscal). Supraîncărcarea pe dicționarul de
  partide înlocuiește `Partide.CuPartida` în FCT, FCL, NIR, RTC, RTF, DVI.
  Trezoreria alege piciorul de terț structural, nu după lipsa gestiunii:
  banii trec de pe contul predatorului (credit) pe al primitorului (debit),
  deci capătul laturii externe e singurul dat helperului, cu partida când
  contul o urmărește și fără ea altfel; piciorul propriu nu trece prin helper
  (corectat la review, D9-6B-R1). În ceilalți declaranți supraîncărcarea pe
  dicționar primește ambele capete, iar gardul „fără gestiune reală” al
  helperului e acoperit prin construcție: piciorul intern poartă acolo mereu
  gestiunea reală a documentului.
- **Gardul** (`Cub/GardAnaliza`): `Lipsuri` judecă numai capătul, prin
  `GardAnaliza.Repartitor(capăt)` = `Citiri.Contabil.Repartitor(partener,
  gestiune)`, funcția unică a repartitorului pe scriitor și pe cititor.
  `Verifica` nu mai citește `RepartitorImplicitDebit/Credit`; cele două
  metode virtuale și cele patru suprascrieri (DEC, DSC, DVI, TRZ) au dispărut
  de pe `Document`.
- **DEC, NTC, DSC, diferența NIR** nu s-au atins: puneau deja partenerul pe
  capătul extern necondiționat (T-D13 g), o supramulțime a regulii.

## 3. Probele

- **SC-PLT-08** (`ScenariiTrezorerie.RepartitorPeTert`, ambele profiluri):
  plata către creditorul cu cont fără partide și fără flag → fără partener;
  plata către creditorul cu cont fără partide care cere repartitor → partener
  pe debit, fără partidă (`Partide.Postari` = 0), balanța analitică pe creditor
  cu un singur rând; pe privat flag-ul se pune pe 462 în scenă și se restaurează.
- **SC-PLT-09 / SC-INC-09** (`ScenariiTrezorerie.ConturiExplicite`, ambele
  profiluri, regresia D9-6B-R1): regulă de contare cu ambele conturi explicite
  (cont propriu 552.00.00 / 5121 cu flag, creditor 462.01.09 / 462 cu flag).
  Cu flag pe contul propriu: refuz „Contul 552.00.00 (credit, …) cere:
  Repartitor”, Draft și zero efecte. Fără flag pe contul propriu: acceptat,
  partenerul numai pe piciorul de terț, piciorul propriu fără repartitor,
  balanța analitică pe creditor numai pe contul de terț.
- **`GARD-ANALIZA`** (`ProbeGardAnaliza`): fără partener și fără gestiune pe
  capăt gardul refuză; partenerul sau gestiunea îl satisfac;
  `GardAnaliza.Repartitor` întoarce partenerul înaintea gestiunii și nul fără
  amândouă. Cazul „documentul îl poartă” a dispărut odată cu fallback-ul.
- **Cele 17 aserții ale clasei „repartitorul de pe latura notei”** (inventar
  §13, șterse la pasul 6) sunt rescrise ca 14 Check-uri `D9-A10 …` pe cititori
  (`CubScena.Note` → `Citiri.Contabil.Postari`; `PostareScena.Repartitor` =
  `Contabil.Repartitor`): piciorul de terț pe `Repartitor`, piciorul intern pe
  gestiune (fiindcă pe el `Partener` e partenerul fiscal, B-D8 pct. 5). Harta:
  987 → FCT privat; 1060 → DEC privat; 1577 → DSC; 1872 → NTC privat;
  4980 → FCT bugetar; 5646 → BCS; 6032 → FCL; 6322 → avans; 7447 + 7451 → DEC;
  7470 → DEC fără cont implicit; 7635 + 7638 → NTC; 9179 + 9183 → Api DEC;
  17524 → Api NTC; 22522 → DVI-V15 (446 rămâne fără partener: forma îngustă).
  Convenția pozițională a notei vechi apare acum explicit răsturnată: pe FCT
  debitul 628 poartă gestiunea primitoare, creditul 401 furnizorul.

## 4. Cifrele

| Probă | Pasul 6 | Pasul 6b |
|---|---|---|
| ModelCheck bugetar | 3.481 OK, 0 FAIL | 3.505 OK, 0 FAIL (3.495 înaintea R1) |
| ModelCheck privat | 4.763 OK, 0 FAIL | 4.784 OK, 0 FAIL (4.774 înaintea R1) |
| `--probe-sursa` | 10 / 10 | 10 / 10 |
| nucleu | 180 / 180 | 180 / 180 |

Comparația pe nume (`pas3/scripts/compara.py`): dispărute numai proba
`GARD-ANALIZA` redenumită, contorul `TRZ: N matrice` (16 → 18, scena nouă) și
numărătoarea „Cautare == Normalizeaza (N rânduri)”, care crește la fiecare
rulare (pas 3, constatarea 3); apărute numai Check-urile `D9-A10`, SC-PLT-08,
SC-PLT-09, SC-INC-09 și matricele lor. Logurile:
`run-nucleu/tr-d9a/pas6b/final-*.log` (înaintea R1), `r1-*.log` (după).
Contraexemplul Codex (`run-nucleu/tr-d9a/review-codex-6-6b/repartitor-explicit.cs`)
dă după corecție, pe regula explicită, `DeLa: partner=null` și un refuz al
gardului. Nicio schimbare de API, deci driftul openapi nu e atins.

## 5. Constatări pentru deciziile 110 și 111

- **(111) Partenerul fiscal maschează gestiunea în `Repartitor`.** Pe piciorul
  intern cu fapt fiscal (628 bază, 4426 taxă), `Partener` e partenerul fiscal
  (B-D8 pct. 5), deci `Contabil.Repartitor` întoarce furnizorul, nu gestiunea.
  Gardul e satisfăcut oricum; balanța analitică pe `RepartitorId` vede
  furnizorul pe 628. Separarea `PartenerFiscal` din 111 o rezolvă; până atunci
  aserțiile pe piciorul intern se scriu pe gestiune.
- **(111) Patru declaranți pun partenerul pe capătul extern necondiționat**
  (DEC, NTC, DSC, diferența NIR — T-D13 g), iar DSC îl pune pe capătul de COST
  (607 cu gestiunea virtuală a clientului): „cu cine s-a întâmplat
  evenimentul”, exact forma respinsă de owner pentru restul tipurilor.
  Netratat aici: scoaterea ar schimba cifre prin dispariție (regula de oprire).
- **(110, limită)** Pe trezorerie, o regulă de contare editată cu ambele
  conturi explicite lasă piciorul propriu fără gestiune (B-D8 pct. 9 depinde de
  `SursaCont`); după D9-6B-R1 lipsa e vizibilă gardului (refuz când contul
  propriu cere repartitor), nu mai e acoperită de partenerul terțului.
  Gestiunea structurală a piciorului propriu rămâne de hotărât în 110.
- **(110)** Coordonata apare pe seed numai pe 16 conturi bugetare fără partide,
  niciunul atins de catalog; proba e sintetică (SC-PLT-08).

## 6. Review-ul Codex (D9-6B-R1, 2026-10-07)

Finding P2 confirmat: helperul atribuia terțul și piciorului propriu fără
gestiune, pe regula cu ambele conturi explicite (comunicare
`2026-10-07-0719-codex-claude-…`). Corecția e cea de mai sus (§2,
trezoreria), regresia SC-PLT-09 / SC-INC-09; reverificată prin integrala
ambelor profiluri și prin contraexemplul lui Codex.

## 7. Restanțele

Proba pe `Partener` cerută de D9-D13 pentru 64h, 86-r13 și F27-r11 există
acum (cele 14 Check-uri `D9-A10` și SC-PLT-08, pe lângă constatarea
inventarului §13 că niciun cititor de sold nu mai grupează pe laturile
documentului). Verdictul se scrie în `restante.md` la închiderea feliei, ca
restul lui D9-D13.

## 8. Urme

`run-nucleu/tr-d9a/pas6b/`: `masura-bugetar.tsv` și `.log` (instrumentarea),
`plt-*.log` (scena singură), `final-*.log`, `probe-sursa.log`.
