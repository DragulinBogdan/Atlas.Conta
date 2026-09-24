# 98. Conexul editat postează delta; poziția fără fișă nu devine negativă; contul fișei vine din cub

- **Data**: 2026-09-24
- **Stare**: aprobată de owner, activă; amendează TR-D7b T-D5, precizează 097(b) și IMO-B3.
- **Docs**: `docs/nucleu/tr-d7b-tipuri-ramase-contract.md` (T-D5),
  `docs/nucleu/tr-d7c-imobilizari-contract.md` (IMO-B2/B3),
  `docs/nucleu/scenarii/NIR.md`, `docs/nucleu/scenarii/IMO.md`.

## Regula durabilă

(a) **NIR conex acoperit postează numai delta față de sursă.** Recepția
rămâne a liniei FCT (090h, TR-D3). Când draftul conex e editat (recepție
parțială, linie manuală, alt lot), NIR-ul postează în cub diferența dintre
liniile lui și liniile sursei filtrate prin politica conexului: minusul pe
lotul sursei, plusul pe lotul propriu. Contrapartida fiecărei direcții e
dată de politică, per profil, fără simbol de cont în motor. Conexul needitat
are delta zero și nu scrie nimic în cub. Stornoul și corecția inversează
postările existente ale documentului, fără să recitească politica.

(b) **Poziția fără fișă pe un cont de imobilizare nu coboară sub zero.** Pe
conturile pe care se nominalizează fișe, niciun document nu poate lăsa
poziția fără fișă sub zero la nicio dată, inclusiv în viitor. Ieșirea
activului nominalizat trece prin CAS pe fișă. Refuzul este atomic, pe toate
ușile, cu cod stabil. Aceasta face din 097(b) un invariant de poziție, nu
doar o regulă a PIF.

(c) **Contul fișei vine din poziția ei în cub.** AMO și CAS postează pe
contul activului și pe contul amortizării pe care fișa stă efectiv în cub.
Politica dă conturile numai la prima nominalizare. O schimbare ulterioară
de politică nu mută fișele existente și nu lasă solduri pe două conturi de
amortizare pentru aceeași fișă.

(d) **Corecturile review-ului de sincronizare** (2026-09-24) se aplică
câte un commit pe felie, cu probă numerică pentru fiecare constatare
confirmată, peste commit-ul de bază `94ddfa8`.

## Context

Review-ul advers de la sincronizarea owner–Claude–Codex a confirmat trei
constatări care cer o alegere de comportament:

- NIR-ul conex e editabil prin proiectare (recepția parțială, 26d, F5-D4),
  dar T-D5 îl excludea din cub în întregime. Factura 4/100 cu recepția 3/75
  lăsa cubul 4/100 și registrele 3/75. Varianta refuzului la operare a fost
  respinsă; owner-ul a ales delta.
- După PIF, o NTC fără fișă care creditează contul activului lăsa poziția
  anonimă −1.200, cu fișa 1.200 și balanța zero: exact forma refuzată de 097
  pentru PIF-ul fără suport.
- AMO și CAS citeau conturile din politica curentă. O schimbare de politică
  între ele lăsa 2811 creditor 100 și 2812 debitor 100 după ieșire.

Celelalte constatări ale review-ului sunt corecturi fără alegere de
comportament și stau în cataloagele tipurilor.

## Restanțe

- 098-r1: conturile de contrapartidă ale deltei NIR per profil, propuse în
  contract și aprobate de owner înaintea implementării.
- 098-r2: recensământul pe clona Flax al conexelor NIR editate față de sursă.
