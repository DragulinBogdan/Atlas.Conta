# X — scenarii transversale

- Tip: catalog
- Actualizat: 2026-10-16

Definițiile lanțurilor transversale. Regulile comune, forma unui rând și
starea tipurilor sunt în [README](README.md).

## Lanțuri

Cele mai valoroase scenarii sunt cele pe care niciun import nu le exercită
și niciun tip nu le poartă singur:

| Id | Lanț | Ce probează |
|---|---|---|
| SC-X-01 | FCT cu recepție → BCS din lotul recepționat → storno FCT | refuzul stornoului cu dependenți; lotul cu ieșiri nu dispare |
| SC-X-02 | FCT → NIR conex operat → BCS | o singură recepție în cub (T-D9, `PoliticaConex`) |
| SC-X-03 | FCL cu avans (INC anterior) → transfer INC → FCL → RDC parțial | două partide, plafonul pe rest, creanța netă după retur |
| SC-X-04 | FCT → PLT parțială → PLT rest → storno PLT a doua | partida se redeschide exact cu suma stornată |
| SC-X-05 | recepție 100 × 10 → BCS 60 → corecție de preț +200 → storno BCS | 091 (h): inversare 600 + compensare 120 înapoi pe lot; datoria rămâne |
| SC-X-06 | recepție 100 × 10 → BCS 60 → corecție de preț +200 → storno corecție → storno BCS | corecția este inversată înaintea consumului; inversarea BCS readuce 600 pe lot, fără compensarea încă o dată a celor 120 |
| SC-X-07 | recepție 100 × 10 → BCS 60 → corecție de preț +200 → corecție de preț +100 → storno BCS | inversare 600 + compensare cumulată 180; stoc final 1.300, datorie 1.300, consum net zero |
| SC-X-08 | BTR între gestiuni → BCS din gestiunea primitoare → storno BTR | refuz: lotul mutat are ieșiri |
| SC-X-09 | ASM din două loturi → DSC din produsul rezultat → RLF pe unul din componente | circuit numeric și inversări în [ASM.md](ASM.md); reevaluarea cu nepropagare rămâne TR-D9 (design §11) |
| SC-X-10 | închidere de lună cu documente operate → document întârziat în luna închisă → corecție | perioada închisă ca graniță absolută; storno legat + document nou |
| SC-X-11 | `Deschidere` (contabil bloc + stoc pe lot + terți pe partidă) → PLT pe partida de deschidere → FCL nouă | partida fără document se stinge prin aceeași cheie ca una cu document |
| SC-X-12 | închiderile lunare 6/7 → 121 pe tot anul → decembrie închis → exercițiul închis → balanța lui ianuarie din anul nou | E3-D8: `SoldInceputAn` = soldul final la 31.12 pe fiecare cheie, 6 și 7 la zero, fără nicio tranzacție `Deschidere`; snapshot-ul de referință egal cu suma postărilor; pe bugetar 121 și 489 în 117 |
| SC-X-13 | DVI legată la FCT → ajustarea costului pe lot cu taxa vamală → BCS din lot | 86-r1: `Atribuit` pe lot, costul ieșirii după ajustare |
| SC-X-14 | Faptele fiscale din fiecare scenă, înainte de curățenie: operare → storno/corecție | unicitate Bază/Taxă independent de Carte, absență explicită pe liniile fără TVA; FCT/FCL 100/21, RDC −100/−21, RLF −20/−4,20, DVI 100/21; matricea completă în [DVI-B7](../tr-d7b-dvi-baza-fiscala-contract.md#dvi-b7--probe-obligatorii-înainte-de-activare); implementat în `UnicitateFiscala` și scenele catalogului |

### SC-X-14 — unicitatea fiscală

`UnicitateFiscala` consumă așteptările numerice ale scenelor, fără citirea
registrelor pentru construirea matricei. Cheia este tranzacție × linie ×
tip TVA × sens, fără Carte; exact o Bază și cel mult o Taxă, cu măsurile
semnate așteptate. Liniile fără fapt fiscal au explicit 0/0. `Postari`
verifică separat tranzacția unică și toate coordonatele; matricile rămase
sunt reverificate înainte de curățenie. BCS are aceeași probă de absență
pe toate liniile sale operate/stornate. Rularea filtrată păstrează proba.

Acoperirea apare în log pe tip/regim/profil. Sunt incluse FCT normal,
capitalizat (NED21: 100/21 pe cost) și taxare inversă, FCL normal,
taxare inversă/scutit (100/0), RDC venit și cost, RLF și DVI; plus liniile
nefiscale BCS, BTR, DSC, PLT/INC, NTC și ITV. Cazurile NED21/TI21 ale FCT
și TI21/SDD ale FCL au și inversare -100/-21, respectiv -100/0. Pe bugetar
absența faptelor fiscale este explicită; tipurile inerte se probează prin
refuz și zero efecte. Nu include tipurile încă nemigrate, toate combinațiile
viitoare sau FCL capitalizat (T-r8). Martorii în memorie detectează baza
copiată în cealaltă carte, taxa duplicată și baza așteptată eliminată.

## Concurență și transversale de comandă

Concurența pe două conexiuni (X-D6, `ScenariiConcurenta`, `--scenarii X`):
scena ține blocajul scrierii, cele două comenzi intră pe rând în coada lui
și rulează în ordinea cozii. Verificate pe ambele profiluri, 2026-10-03.

| Id | Comenzile, în ordinea serializată | Rezultatul serial |
|---|---|---|
| SC-X-15 | lot 10/100; două BCS de câte 6 | primul trece, al doilea `STOC_INSUFICIENT`, fără efecte; lot 4/40 (SC-BCS-13) |
| SC-X-16 | lot 3/1,00; două BCS de câte 1 | ambele trec, 0,33 apoi 0,34; soldurile citite din explicații sunt 3/1,00 și 2/0,67; lot 1/0,33 |
| SC-X-17 | FCT 10 × 10 operată; BCS 4 și stornoul FCT, în ambele ordini | consum → storno: stornoul e refuzat, lot 6/60; storno → consum: consumul e refuzat, lot 0/0 |
| SC-X-18 | FCT 100; două plăți de 60 împerecheate cu ea | prima trece, a doua e refuzată peste rest; rest factură 40, a doua plată nealocată |
| SC-X-19 | FCT 100 și PLT 100, aceeași pereche: 40 + 40, apoi 15 + 15 | 40 + 40 trec, rest 20; din 15 + 15 trece prima, rest 5 |
| SC-X-20 | BCS draft cu o linie; două sesiuni adaugă câte un detaliu | ambele trec; pozițiile sunt 1, 2, 3 (S-r9) |
| SC-X-21 | lot 10/100; BCS 4 și închiderea lunii lui, în ambele ordini | operare → închidere: ambele trec, snapshot 6/60; închidere → operare: operarea e refuzată, fără efecte, iar snapshot-ul lunii rămâne 6/60 (SC-BCS-14, F27-r8) |
| SC-X-22 | cele 16 feluri de comandă, pe captura SQL | fiecare începe cu blocajul scrierii; dry-run-ul, citirile și salvarea fără detalii noi nu îl iau |
| SC-X-23 | comandă care așteaptă peste timpul ei | refuz `SCRIERE_OCUPATA`, fără efecte; după eliberare trece |
| SC-X-24 | fiecare Anulează, Stornează și Corectează din scenele catalogului, cu regimul citit înaintea comenzii | regimul care refuză ⇒ comanda refuză; comanda care refuză ⇒ regimul refuza, în afara refuzurilor dovedite pe valori, pe data cerută ori pe limita 106-r7 declarată de scenă (106k, [REGIM](REGIM.md)) |
| SC-X-25 | BPR (nu declară, fără politici), pe ambele profiluri | dry-run și operare refuzate cu `TIP_FARA_DECLARATIE`, cu și fără număr cules; documentul rămâne Draft, zero efecte (D9-D5, schimbarea 4). Înaintea tăierii, cu număr cules rămânea Operat fără nicio postare. Geamănele pe profil: SC-RLF-12/14, SC-RDC-16/19, SC-DSC-09, SC-DVI-12 |
| SC-X-26 | regulă de contare pe un tip al cărui declarant nu contează prin reguli (BTR) | refuzată la editare, pe ușa gardianului, cu `REGULA_CONTARE_FARA_CONSUMATOR`, și nepersistată; o regulă pe BCS trece; ștergerea unei reguli existente nu e refuzată de acest gard; regula probei stă pe un tip de material propriu, iar refuzul de seed lăsat de ștergere e curățat pe identitate, cu refuzurile preexistente neschimbate; seed-ul nu are nicio asemenea regulă (D9-A4, schimbarea 6) |
| SC-X-27 | FCT 10/100 → NIR → BCS 4 → storno NIR; apoi BCS 2 datat înaintea stornării și BCS 4 după ea | nimic nu e refuzat: cubul 6/60 → 0/0, cu 20 și 40 pe ieșiri; recepția stă pe factură (098-r3) |
