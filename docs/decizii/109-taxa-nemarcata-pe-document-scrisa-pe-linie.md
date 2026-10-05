# 109 — Taxa nemarcată se decide pe document × cotă și se scrie pe linie; declarația postează taxa liniei; taxa unui document real adusă de o cale fără culegere e marcată

- Data: 2026-10-04
- Stare: activă, aprobată de owner 2026-10-04; implementată pe `107-r3-drift-tva`; review advers Codex închis: 109-R1 corectată de Codex și reverificată independent 2026-10-05 (`docs/nucleu/109-review-codex.md`); precizează 090 (j) și 103 (i); închide 107-r3; face din N-r4 o egalitate (registrele și cubul poartă aceeași taxă)
- Docs: `docs/nucleu/scenarii/FCL.md` (SC-FCL-11…14), `FCT.md` (SC-FCT-11); `docs/nucleu/109-review-codex.md`; `docs/stare-curenta/politici-si-fiscalitate.md` („TVA la operare”); 107 (i); `docs/nucleu/tr-d8-tva-intervale-contract.md` (R6-B2); dovezile în `run-verificari/r3-ian/`, `r3-ian-final/`, `r3-motor/`

## Regula durabilă

**Linia documentului poartă taxa care se postează. Taxa nemarcată e decisă o
singură dată, pe document × cotă, de formula entității; taxa marcată e a
operatorului sau a documentului real și intră neschimbată.**

(a) **Taxa nemarcată se decide pe document × cotă (090j) și se scrie pe
linii.** Baza de calcul e suma valorilor rotunjite ale liniilor fiscale ale
cotei; rezultatul, rotunjit o dată, se repartizează pe linii prin Hamilton
peste |valoare|, pe fiecare semn, în ordinea operării (poziție, apoi cheie).
Liniile noi, încă fără poziție, vin după cele numerotate, în ordinea
colecției documentului: aceeași în care salvarea le atribuie pozițiile
(precizat 2026-10-05, 109-R1).
Formula e a entității (`Document.CalculeazaValori`, 104c), aceeași la
culegere (recalcul și salvare) și la pregătirea operării. Draftul arată deci
totalul care se va posta.

(b) **Declarația postează taxa liniei; nu o decide din nou.** Taxa
documentului calculată de nucleu rămâne în declarație numai pentru gardul
toleranței (S-D15). Linia, totalul documentului, registrele regimului dual și
cubul poartă aceeași taxă, iar totalul documentului e totalul partidei.
INV-CUB refuză taxa postată separat care diferă de taxa liniei
(`CITIRE_TAXA_DIFERITA_DE_LINIE`); taxa capitalizată stă în valoarea liniei
și nu intră în comparație.

(c) **O cale care nu culege prin L3 și aduce taxa unui document real o
marchează culeasă la scriere (103i).** Conectorul 1C o face într-un singur
loc (`Catalog.TaxaDinSursa`). Nemarcată, taxa sursei e taxă calculată: se
recalculează la operare, iar diferența față de sursă e a conectorului, nu a
motorului.

(d) **Un tip fără politică de TVA păstrează taxa calculată pe linie.** Nu
postează TVA, deci nu are ce repartiza.

(e) **Un drift al contractului 1 se atribuie întâi unei schimbări cu nume
între cele două rulări** (regula care s-a schimbat, cu commit-ul ei), înainte
de orice ipoteză despre motor. Cât ține regimul dual, contractul 1 citește
registrele, iar închiderea de TVA se generează din cub: o diferență registru ↔
cub apare ca sold rămas pe 4426/4427 după închidere.

(f) **Reîncărcarea nu e culegere, iar documentul se reîncarcă întreg.** În
ecranul XAF linia se editează în tabul ei, cu spațiul ei de obiecte, iar
salvarea de acolo renormalizează tot documentul. Adaptorul de culegere nu
recalculează pe evenimentele unei reîncărcări (`IsReloading`); când o linie a
documentului e reîncărcată, tabul documentului reîncarcă și liniile surori pe
care nu le-a modificat el.

(g) **Formula scrie fiecare câmp o singură dată.** Renormalizarea unui
document neschimbat nu modifică nicio linie: fără valoare intermediară
nerotunjită, fără taxă de linie rescrisă apoi de repartizare.

## Context

107-r3 numea „drift-ul motorului” diferențele contractului 1 din ianuarie
față de baseline-ul 2026-09-21: 401 −10,57, 4423 +8,45, 4111 +1,26, 4427
+1,00, 4426 −0,14, cu 212 împerecheri refuzate pentru 1–3 bani. Owner-ul a
cerut tratarea ei înaintea TR-D9.

**Cauza, pe cod.** R6 (`faa8b2d`, 2026-09-28, 103i) a înlocuit proxy-ul
„taxă nenulă = taxă culeasă” cu marcajul explicit `TvaCules`: condiția de
păstrare din `TvaService.CalculeazaValori` a trecut din `ValoareTva != 0` în
`TvaCules`. Conectorul, înghețat de 091 (f), scria `ValoareTva` din sursă fără
marcaj. De la R6, motorul îi recalculează taxa la cotă.

**Măsurat pe `.Flax.M1s`** (ianuarie, rularea de închidere a M1):

- 7.242 de linii cu taxă (FCL 4.112, FCT 2.864, RDC 244, RLF 22), niciuna
  marcată.
- Registrele poartă taxa recalculată pe linie; cubul poartă taxa pe document
  × cotă. Diferența cub − registru: 4427 +1,16 (FCL), −0,21 (RDC), +0,05
  (autocolectarea FCT) = +1,00; 4426 +0,14 (FCT). Închiderea de TVA se
  generează din cub, iar contractul 1 citește registrul: de aici soldurile
  rămase 4427 +1,00 și 4426 −0,14.
- 4111 +1,26 și 401 −10,57 sunt taxa recalculată față de taxa sursei; pe RLF,
  recalculul pleacă de la baza evaluată la prețul lotului, nu de la baza
  facturii. 4423 +8,45 e consecința în închidere.
- 481 din 4.926 de documente cu taxă nemarcată aveau Σ taxelor de linie ≠
  taxa postată în cub (FCL 308 din 3.297, FCT 150 din 1.440, RDC 23 din 170,
  RLF 0 din 19), cu cel mult 0,02 pe document.

Ultima cifră nu ține de migrare. Pe orice document cules fără taxă marcată,
linia arăta taxa rotunjită pe linie, iar cubul posta taxa documentului: trei
linii de 10,03 la 21% dădeau total 36,42 pe document și creanță 36,41 în
partidă. Plata generată de factură (`FacturaIntrare.GenereazaSecundar`)
pleacă din valorile liniilor, deci plătea alt total decât datoria. N-r4
consemnase diferența ca a regimului dual; tăierea registrelor nu o închidea.

## Tranșări

- **Formula pe entitate, nu scriere înapoi la operare.** Scrierea taxei
  postate pe linii abia la operare ar fi lăsat draftul cu alt total decât cel
  postat; 104 (c) cere aceeași formulă la culegere și la operare.
- **Declarația citește linia.** Două decidente care trebuie să coincidă
  (formula entității și `Fiscal.Valoarea`) erau cauza diferenței. Cu un singur
  decident, o factură operată se recontractează la aceeași taxă și după
  schimbarea cotei. Prețul: `Contractare.Contracteaza` chemat pe un draft
  nepregătit nu mai completează taxa; producția contractează numai după
  `PregatesteOperare` (`Materializare`), iar proba `NUC-FCT-CULESE-2` pregătește
  acum documentul.
- **Baza e valoarea rotunjită a liniei.** Cubul calcula deja așa; formula de
  linie înmulțea baza nerotunjită. e-Factura cere același lucru pe factura
  emisă: taxa categoriei = baza categoriei × cota, rotunjită la două zecimale
  (EN 16931, BR-CO-17), iar factura nu are taxă pe linie.
- **Marcajul în conector e scriere directă, într-un singur loc.** L3
  (`CulegereDocument.Mapata`) face și precompletări pe care importul nu le
  vrea (tipul de TVA implicit pe linia lăsată deliberat fără tip). CHECK-ul
  `TvaCules ⇒ ValoareTva ≠ 0` păzește scrierea.
- **Probele N-r4 se rescriu, nu se șterg.** `NUC-FCT-N-R4-1…3` măsurau Δ =
  0,01 între registre și nucleu; acum cer egalitatea.

## Verificare

Corectura 109-R1 (Codex, 2026-10-05): liniile noi se ordonează după cele
numerotate, în ordinea colecției documentului, comună repartizării și
atribuirii pozițiilor la salvare. Pozițiile se atribuie în continuare sub
blocajul scrierii. SC-FCL-13/14 verifică 0/1/2 linii deja salvate și restul
noi, cu/fără taxă culeasă, prima salvare, idempotența și operarea. Integrala:
3.459 OK bugetar / 4.771 OK privat, zero FAIL, exit 0,
`run-verificari/20261004-235420-458/`. Raport și control negativ:
`docs/nucleu/109-review-codex.md`.

Reverificarea independentă (Claude, 2026-10-05), pe binar recompilat integral
din arborele corectat: 3.459 OK bugetar / 4.771 OK privat, zero FAIL
(`run-verificari/20261005-004505-569/`). Pe HTTP, prin WebApi: draft cu două
linii salvate, a treia adăugată prin PUT, fără și cu taxa liniei 2 culeasă —
2,11 / 2,11 / 2,10 și 36,41, respectiv 2,11 / 2,15 / 2,10 și 36,45, la
răspuns, la prima citire, la PUT fără editări și după operare
(`run-verificari/r3-ui-r1-http.py`, `r3-ui/r1-http.log`). În browser, pe host
viu: a treia linie adăugată din grila documentului, în tabul ei; linia nouă
arată 2,10 înaintea salvării, tabul documentului arată aceleași taxe și
totaluri, operarea trece fără conflict de versiune (FCL-6: 36,41; FCL-7:
36,45), cu aceleași valori pe totalul de stins și în cub
(`run-verificari/r3-ui/r1-*`). Ianuarie pe Flax cu binarul corectat
(`run-verificari/r3-ian-r1/`, baza `.Flax.R3f`): contractele 1–4 identice cu
`r3-ian-final2/`, INV-CUB verde, aceeași durată; contractul 5 a dat 7 partide
fără explicație (107-r7).

- **Scenarii, scrise înaintea rulării.** SC-FCL-11, SC-FCL-12, SC-FCT-11.
  Pe motorul neschimbat pică exact pe partea de document (4 verificări) și pe
  cele trei probe N-r4 rescrise; postările din cub treceau deja
  (`run-verificari/r3-motor/scenarii-fcl-fct-privat-motor-vechi.log`).
- **ModelCheck integral**: 4.705 privat, 3.459 bugetar, zero FAIL
  (`run-verificari/r3-motor/privat.log`, `bugetar.log`). Baza bugetară avea
  patru migrații neaplicate; au fost aplicate înaintea rulării.
- **Conectorul singur, pe motorul neschimbat** (`run-verificari/r3-ian/`,
  baza `.Flax.R3`, `--recreeaza --cititori --pana-la 1`, 23:12): 0 eșecuri de
  import; contractul 1 cu 0 conturi fără explicație (401 Δ −20,01 explicat de
  9 înregistrări, ca la baseline); contractul 2 egal cu sursa (4427 = 4423
  431.791,93; 4427 = 4426 977.268,92); contractele 3 și 4 verzi. Diferența pe
  conținut sortat față de baseline: numai Δ-ul declarat al stocului pe 371 și
  891 (107 e).
- **Binarul final pe aceeași bază** (`r3-ian-final/reluare/`, 47 s): INV-CUB
  cu invariantul nou trece pe baza integrală; contractele 1 și 2 neschimbate.
- **Binarul final, import proaspăt** (`run-verificari/r3-ian-final/`, baza
  `.Flax.R3f`, 23:23): 0 eșecuri de import, INV-CUB verde după deschidere și
  pe baza integrală, contractele 1–4 identice pe conținut cu rularea numai cu
  conectorul. Binarul precede o singură tăietură fără efect: câmpul rămas
  nefolosit `LinieOperand.TvaCules`; ModelCheck integral a fost rerulat după
  ea, cu aceleași cifre.
- **Binarul de după (f) și (g), import proaspăt**
  (`run-verificari/r3-ian-final2/`, baza `.Flax.R3f` recreată, 23:56): 0
  eșecuri de import, INV-CUB verde, contractele 1–4 identice pe conținut cu
  rularea numai cu conectorul.
- **Browser, pe host viu** (`run-verificari/r3-ui.ps1`, baza
  `Atlas.Conta.BackOffice.Privat.R3Ui`, Blazor pe 5091, draft creat prin API
  pe 5089; instantaneele în `run-verificari/r3-ui/`). Draft cu trei linii de
  10,03 la N21: API-ul și ecranul arată 2,11 / 2,11 / 2,10, total 36,41. Linia
  3 deschisă din grilă în tabul ei, prețul dus la 20,00: tabul liniei arată
  imediat valoarea 20,00 și taxa 4,20; după salvare, tabul documentului arată
  2,11 / 2,10 / 4,20 și total 48,47 (linia 2 s-a mutat, deși s-a editat linia
  3). Operarea din ecran dă FCL-3, cu 48,47 pe linii, pe totalul de stins, pe
  4111 în cub și în registre; taxa fiecărei linii e cea postată.
- **Regresia găsită de probă și corectată prin (f).** Prima trecere, înaintea
  lui (f): după salvarea liniei, operarea din tabul documentului era refuzată
  cu „obiectul a fost schimbat de alt utilizator”. `ReloadObject` ridică
  evenimente de schimbare, adaptorul le trata ca editare, iar repartizarea
  rescria în memorie linia soră cu versiunea veche. Controlul cu valori
  rotunde (3 × 10,00, fără mutare pe surori) opera fără conflict.
- **(g) pe contoarele de versiune.** Înaintea lui (g), salvarea unei linii
  ridica versiunea tuturor liniilor cu taxă nerotundă, fără schimbare de
  valoare; după, linia 1 rămâne la versiunea 0 și la salvare, și la operare.
  Proba `SC-FCL-11` „renormalizarea draftului neschimbat nu modifică nicio
  linie” e roșie pe formula dinainte
  (`r3-motor/scenarii-fcl-privat-formula-veche.log`).
- **Împerecherile refuzate „peste totalul documentului”**: 212 (Σ 756.238,35)
  → 46 (Σ 378.517,19), apoi 47 la următoarele două rulări. Din cele 11 detaliate în jurnal, niciuna nu mai e de
  bani mărunți: nouă depășesc restul cu sub 20 de lei (cele patru verificate
  sunt facturi cu taxare inversă ale aceluiași furnizor, cu excedente de
  0,95–11,48: divergențele de valoare declarate pe 401, Σ −20,01, prezente și
  la baseline, unde refuzurile erau 6, Σ 21k), două
  țintesc o factură a cărei partidă proprie e mai mică decât totalul sursei
  (restul stă pe punte). Restul de 35 nu e triat (109-r3).
- **Contractul 5**: 12, 10 și 8 partide fără explicație la cele trei importuri
  proaspete (10 la rularea de închidere a M1). Cele afișate poartă eticheta
  107-r9; variația de la o rulare la alta e 107-r7.

## Ce rămâne deschis

- 109-r1 — contractele 1 și 2 ale reconcilierii citesc `RegistruContabil`
  (`ReconciliereLuna`); la TR-D9 trec pe cititorii cubului.
- 109-r2 — închisă 2026-10-04 prin proba din browser și prin (f).
- 109-r3 — 46 de împerecheri pe documente refuzate „peste restul nestins al
  documentului” în ianuarie, față de 6 la baseline; clasa de bani mărunți a
  dispărut, restul nu e triat (candidat: facturile cu o parte din creanță pe
  punte, a căror partidă proprie e mai mică decât totalul sursei).
- Neschimbate: S-r1 (valoarea de produs a toleranței), 103-r3 (semnele
  incompatibile pe FCT/FCL), 107-r1, 107-r7, 107-r9.
- 109-r4 — tabul liniei care nu a fost deschis din grila documentului (deschis
  prin adresă sau rămas dintr-o sesiune anterioară) nu reîmprospătează tabul
  documentului la salvare: documentul arată valorile vechi până la Refresh.
  Operarea merge, fiindcă lucrează pe cheie. Comportament XAF dinaintea 109.
