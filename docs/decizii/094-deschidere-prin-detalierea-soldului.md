# 94. Deschiderea detaliază soldul contabil, fără dublare

- Data: 2026-09-24
- Stare: aprobată de owner, activă
- Docs: `docs/nucleu/tr-d7b-deschidere-contract.md` DES-B1…B4;
  TR-D7b T-D7; 090(d/g), 091(f/i), 092.

## Regula durabilă

(a) Postările de lot și partidă înlocuiesc soldul bloc pe cheia
Carte/Cont/Latura. Nu se adaugă încă o dată peste același sold contabil.
(b) Totalul pozițiilor detaliate trebuie să fie exact soldul de control.
Diferența, lipsa detaliilor cerute sau referințele invalide refuză atomic
deschiderea. Nu se fabrică o poziție reziduală fără unitate sau partener.
(c) Ancora contabilă este furnizată de apelant, fără simbol hardcodat și
fără dublarea ei. Deschiderea generică verifică echilibrarea pe carte;
excepția C6 din nucleul pur nu se modifică.
(d) Tranzacția rămâne fără document, unică per bază. Terții au partide
cu identitatea cont/partener/referință stabilă, nu documente fictive.
Conectorul 1C rămâne înghețat conform 091(f).

## Contraexemplul și alegerea

T-D7 prescria sold D 302 100/C 891 100 plus loturi D 302/A 40 și
D 302/B 60. Reuniunea contabilă ar fi arătat 302 = 200 și dezechilibru
100. Varianta aprobată scrie numai loturile 40 + 60 și C 891 100.
Contul rămâne 100; loturile păstrează detalierea; D=C.
Owner-ul a aprobat explicit „detalierea și refuzul diferențelor”.
Această regulă înlocuiește forma aditivă din T-D7; diferențele istorice de
migrare rămân raportate, nu devin valori acceptate ale modelului nou.

Probe și starea implementării: `docs/nucleu/scenarii/DESCHIDERE.md`.
TR-r10 urmărește mecanismul generic; partea de conector rămâne 091-r4/T-r4.
