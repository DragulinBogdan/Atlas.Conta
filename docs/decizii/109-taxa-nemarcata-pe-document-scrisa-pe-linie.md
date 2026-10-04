# 109 — Taxa nemarcată se decide pe document × cotă și se scrie pe linie; declarația postează taxa liniei; taxa unui document real adusă de o cale fără culegere e marcată

- Data: 2026-10-04
- Stare: implementată și verificată pe `107-r3-drift-tva`; așteaptă aprobarea owner-ului și review-ul advers; precizează 090 (j) și 103 (i); închide 107-r3; face din N-r4 o egalitate (registrele și cubul poartă aceeași taxă)
- Docs: `docs/nucleu/scenarii/FCL.md` (SC-FCL-11, SC-FCL-12), `FCT.md` (SC-FCT-11); `docs/stare-curenta/politici-si-fiscalitate.md` („TVA la operare”); 107 (i); `docs/nucleu/tr-d8-tva-intervale-contract.md` (R6-B2); dovezile în `run-verificari/r3-ian/`, `r3-ian-final/`, `r3-motor/`

## Regula durabilă

**Linia documentului poartă taxa care se postează. Taxa nemarcată e decisă o
singură dată, pe document × cotă, de formula entității; taxa marcată e a
operatorului sau a documentului real și intră neschimbată.**

(a) **Taxa nemarcată se decide pe document × cotă (090j) și se scrie pe
linii.** Baza de calcul e suma valorilor rotunjite ale liniilor fiscale ale
cotei; rezultatul, rotunjit o dată, se repartizează pe linii prin Hamilton
peste |valoare|, pe fiecare semn, în ordinea operării (poziție, apoi cheie).
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

- **Scenarii, scrise înaintea rulării.** SC-FCL-11, SC-FCL-12, SC-FCT-11.
  Pe motorul neschimbat pică exact pe partea de document (4 verificări) și pe
  cele trei probe N-r4 rescrise; postările din cub treceau deja
  (`run-verificari/r3-motor/scenarii-fcl-fct-privat-motor-vechi.log`).
- **ModelCheck integral**: 4.704 privat, 3.459 bugetar, zero FAIL
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
- **Împerecherile refuzate „peste totalul documentului”**: 212 (Σ 756.238,35)
  → 46 (Σ 378.517,19), respectiv 47 (Σ 394.612,67) la a doua rulare. Din cele 11 detaliate în jurnal, niciuna nu mai e de
  bani mărunți: nouă depășesc restul cu sub 20 de lei (cele patru verificate
  sunt facturi cu taxare inversă ale aceluiași furnizor, cu excedente de
  0,95–11,48: divergențele de valoare declarate pe 401, Σ −20,01, prezente și
  la baseline, unde refuzurile erau 6, Σ 21k), două
  țintesc o factură a cărei partidă proprie e mai mică decât totalul sursei
  (restul stă pe punte). Restul de 35 nu e triat (109-r3).
- **Contractul 5**: 12 partide fără explicație la prima rulare, 10 la a doua
  (10 la rularea de închidere a M1). Cele afișate poartă eticheta 107-r9;
  variația între două importuri proaspete ale aceluiași cod e 107-r7.

## Ce rămâne deschis

- 109-r1 — contractele 1 și 2 ale reconcilierii citesc `RegistruContabil`
  (`ReconciliereLuna`); la TR-D9 trec pe cititorii cubului.
- 109-r2 — în ecranul XAF, recalculul unei linii mută taxa și pe celelalte
  linii nemarcate ale documentului. Starea salvată e corectă (normalizarea de
  la commit recalculează tot documentul); reafișarea celorlalte rânduri
  înaintea salvării nu e probată în browser.
- 109-r3 — 46 de împerecheri pe documente refuzate „peste restul nestins al
  documentului” în ianuarie, față de 6 la baseline; clasa de bani mărunți a
  dispărut, restul nu e triat (candidat: facturile cu o parte din creanță pe
  punte, a căror partidă proprie e mai mică decât totalul sursei).
- Neschimbate: S-r1 (valoarea de produs a toleranței), 103-r3 (semnele
  incompatibile pe FCT/FCL), 107-r1, 107-r7, 107-r9.
