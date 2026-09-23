# DVI — declarație vamală

2026-09-23: catalog specificat înaintea implementării. **Owner-ul a ales
baza distinctă în Carte=Fiscal**; forma concretă este în review în
[`tr-d7b-dvi-baza-fiscala-contract.md`](../tr-d7b-dvi-baza-fiscala-contract.md).
Contracte:086,090(a/k),TR-D7b T-D8; recensământul read-only din
`recensamant-retururi-dvi.sql` găsește zero DVI pe Flax.

## Contraexemplul care cere completarea contractului

DVI cu bază100 și TVA21 are numai nota contabilă4426=446:21. `Fiscal.Impozitul`
poate păstra taxa, codul și perioada; `Fiscal.CuFapt` atașează rolul Bază
unei postări de net, dar DVI nu are acea postare. Baza100 nu este măsură
în cub. Nici recuperarea din cotă nu este exactă: bazele100 și100,01 pot
produce aceeași taxă21 după rotunjire, iar taxa culeasă este autoritară.

T-D8 cere numai postarea taxei; B-D8 pct.5 presupune existența netului.
Aplicarea lor literală nu poate reconstrui jurnalul fiscal din cub.
Nu este B-r4 (faptul colectat la taxare inversă), nici DVI pe loturi.
`Normalizari.Fiscal` constată lipsa țintei de bază și păstrează postarea
fiscală diagnostică; aceasta nu este o regulă de producție.

**Direcție aleasă:** baza distinctă în `Carte=Fiscal`, în același cub,
balansată în propria carte. Forma propusă DVI-B2: D4426/C4426:100 fiscal,
cu rol Bază numai pe debit, fără unitate pe ambele capete. Taxa rămâne
D4426/C446:21 contabil. Rulajul contabil și datoria rămân21; jurnalul
citește baza100 și taxa21 o singură dată. Alegerea contului drept ancoră
și probele exacte sunt în contractul de mai sus; codul nu este încă scris.

## Așteptări independente de reprezentarea aleasă

Privat: taxe4426/4427, contrapartidă446 sau401 din partener. Fără stoc.
Bugetar: profil inert, fără politică și fără activare cub. Coloana Bază
se referă la proiecția fiscală, nu la o sumă adăugată balanței contabile.

| ID | Pași și așteptare numerică | Rezultat | Stare |
|---|---|---|---|
| SC-DVI-01 | Bază100 IMP21 → D4426/C446:21 contabil și D4426/C4426:100 fiscal, cu rol Bază numai pe debit; proiecție fiscală100/21. Dry-run fără efecte, apoi4 postări exacte. | acceptat | specificat |
| SC-DVI-02 | Linii100 IMP21 +50 IMP11 → taxe21+5,50; total contabil26,50; două baze fiscale100/50. | acceptat | specificat |
| SC-DVI-03 | Anulare SC01 în luna deschisă → zero efecte; reoperare → aceleași100/21, fără dublare. | acceptat | specificat |
| SC-DVI-04 | Storno SC01 în aceeași lună →−21 contabil, −100/−21 fiscal; net0; originalele intacte, repetarea refuzată. | acceptat / refuz stare | specificat |
| SC-DVI-05 | Ianuarie închis: anularea refuzată; storno februarie →−100/−21 fiscal în februarie; ianuarie rămâne100/21. | acceptat pentru storno | specificat |
| SC-DVI-06 | Corecție în februarie a bazei100 la80, eroare materială: original inversat, draft legat; noua taxă16,80; fiscal ianuarie net80/16,80. | acceptat | specificat |
| SC-DVI-07 | Data05.01, înregistrare05.02 după închiderea lui ianuarie: contabil0 la31.01 și21 în februarie; fiscal100/21 declarat în februarie, politica deductibilului din088. | acceptat | specificat |
| SC-DVI-08 | Comisionar cu cont implicit401: D4426/C401:21; partidă proprie−21; baza100 nu adaugă datorie și nu intră în FIFO. Storno → partidă0. | acceptat | specificat |
| SC-DVI-09 | Bază100 IMPTI21: D4426/C4427:21; bază100/taxă deductibilă21; fără partidă; B-r4 privind faptul colectat rămâne explicită. | acceptat, limită declarată | specificat |
| SC-DVI-10 | MRN absent / fără linii / bază≤0 / TVA obișnuit N21 / IMP cu cotă0 / FCT legată neoperată: refuz cu zero efecte. | refuzat | specificat |
| SC-DVI-11 | Două FCT operate legate la aceeași DVI; operarea DVI nu dublează recepțiile/datoriile lor. DVI fără legături este permisă. | acceptat | specificat |
| SC-DVI-12 | Bugetar: seed inert; încercare de operare fără politica necesară, zero postări. | refuzat / inert | specificat |
| SC-DVI-13 | Bază100, taxă culeasă21,03, toleranța implicită null: D4426/C446:21,03 contabil, pereche fiscală100/100; jurnal100/21,03. | acceptat | specificat |
| SC-DVI-14 | Două DVI cu baze100 și100,01, taxe calculate21 și21: cubul păstrează distinct bazele; jurnal cumulat200,01/42. | acceptat | specificat |
| SC-DVI-15 | Bază0,01 IMP21, taxă calculată0: exact2 postări fiscale, contabil0, zero partidă. Storno →−0,01 fiscal, net0; originalele intacte. | acceptat | specificat |
| SC-DVI-16 | Pe SC01: rulaje contabile D21/C21; carte fiscală D100/C100, sold net4426 fiscal0; unicitate Bază/Taxă, două postări fiscale fără unități. Jurnal100/21, fără dublare. | acceptat | specificat |
| SC-DVI-17 | DVI pe401 cu taxă21 → NTC401=5121:10 pe același partener: rest−11; inversarea DVI cu NTC activ refuzată atomic. Storno NTC redeschide−21, apoi storno DVI →0 contabil și0 fiscal cumulat. | acceptat / refuz dependență | specificat |
| SC-DVI-18 | Același document întârziat ca SC07, dar ianuarie definit și încă deschis: fiscal100/21 în ianuarie, contabil21 în februarie. O pereche distinctă de fixture față de SC07. | acceptat | specificat |

Probe viitoare în `ScenariiDvi`; familia existentă `DVI-V*` verifică
regimul actual pe registre, nu rezolvă absența bazei din cub. Ajustarea
costului pe lot și SC-X-13 rămân TR-D9; 86-r2 (dependența FCT legate)
rămâne de tratat explicit, fără a o declara rezolvată prin aceste probe.
