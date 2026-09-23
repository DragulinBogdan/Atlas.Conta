# DVI — declarație vamală

2026-09-23: catalog specificat înaintea implementării. **Owner-ul a ales
baza distinctă în Carte=Fiscal**; forma concretă este în review în
[`tr-d7b-dvi-baza-fiscala-contract.md`](../tr-d7b-dvi-baza-fiscala-contract.md).
Contracte: 086, 090 (a/k), TR-D7b T-D8; recensământul read-only din
`recensamant-retururi-dvi.sql` găsește zero DVI pe Flax.

## Contraexemplul care cere completarea contractului

DVI cu bază 100 și TVA 21 are numai nota contabilă 4426 = 446: 21. `Fiscal.Impozitul`
poate păstra taxa, codul și perioada; `Fiscal.CuFapt` atașează rolul Bază
unei postări de net, dar DVI nu are acea postare. Baza 100 nu este măsură
în cub. Nici recuperarea din cotă nu este exactă: bazele 100 și 100,01 pot
produce aceeași taxă 21 după rotunjire, iar taxa culeasă este autoritară.

T-D8 cere numai postarea taxei; B-D8 pct. 5 presupune existența netului.
Aplicarea lor literală nu poate reconstrui jurnalul fiscal din cub.
Nu este B-r4 (faptul colectat la taxare inversă), nici DVI pe loturi.
`Normalizari.Fiscal` constată lipsa țintei de bază și păstrează postarea
fiscală diagnostică; aceasta nu este o regulă de producție.

**Direcție aleasă:** baza distinctă în `Carte=Fiscal`, în același cub,
balansată în propria carte. Forma propusă DVI-B2: D 4426 / C 4426: 100 fiscal,
cu rol Bază numai pe debit, fără unitate pe ambele capete. Taxa rămâne
D 4426 / C 446: 21 contabil. Rulajul contabil și datoria rămân 21; jurnalul
citește baza 100 și taxa 21 o singură dată. Alegerea contului drept ancoră
și probele exacte sunt în contractul de mai sus; codul nu este încă scris.

## Așteptări independente de reprezentarea aleasă

Privat: taxe 4426 / 4427, contrapartidă 446 sau 401 din partener. Fără stoc.
Bugetar: profil inert, fără politică și fără activare cub. Coloana Bază
se referă la proiecția fiscală, nu la o sumă adăugată balanței contabile.

| ID | Pași și așteptare numerică | Rezultat | Stare |
|---|---|---|---|
| SC-DVI-01 | Bază 100 IMP21 → D 4426 / C 446: 21 contabil și D 4426 / C 4426: 100 fiscal, cu rol Bază numai pe debit; proiecție fiscală 100 / 21. Dry-run fără efecte, apoi 4 postări exacte. | acceptat | specificat |
| SC-DVI-02 | Linii 100 IMP21 + 50 IMP11 → taxe 21 + 5,50; total contabil 26,50; două baze fiscale 100 / 50. | acceptat | specificat |
| SC-DVI-03 | Anulare SC-DVI-01 în luna deschisă → zero efecte; reoperare → aceleași 100 / 21, fără dublare. | acceptat | specificat |
| SC-DVI-04 | Storno SC-DVI-01 în aceeași lună → −21 contabil, −100 / −21 fiscal; net 0; originalele intacte, repetarea refuzată. | acceptat / refuz stare | specificat |
| SC-DVI-05 | Ianuarie închis: anularea refuzată; storno februarie → −100 / −21 fiscal în februarie; ianuarie rămâne 100 / 21. | acceptat pentru storno | specificat |
| SC-DVI-06 | Corecție în februarie a bazei 100 la 80, eroare materială: original inversat, draft legat; noua taxă 16,80; fiscal ianuarie net 80 / 16,80. | acceptat | specificat |
| SC-DVI-07 | Data 05.01, înregistrare 05.02 după închiderea lui ianuarie: contabil 0 la 31.01 și 21 în februarie; fiscal 100 / 21 declarat în februarie, politica deductibilului din 088. | acceptat | specificat |
| SC-DVI-08 | Comisionar cu cont implicit 401: D 4426 / C 401: 21; partidă proprie −21; baza 100 nu adaugă datorie și nu intră în FIFO. Storno → partidă 0. | acceptat | specificat |
| SC-DVI-09 | Bază 100 IMPTI21: D 4426 / C 4427: 21; bază 100 / taxă deductibilă 21; fără partidă; B-r4 privind faptul colectat rămâne explicită. | acceptat, limită declarată | specificat |
| SC-DVI-10 | MRN absent / fără linii / bază ≤ 0 / TVA obișnuit N21 / IMP cu cotă 0 / FCT legată neoperată: refuz cu zero efecte. | refuzat | specificat |
| SC-DVI-11 | Două FCT operate legate la aceeași DVI; operarea DVI nu dublează recepțiile/datoriile lor. DVI fără legături este permisă. | acceptat | specificat |
| SC-DVI-12 | Bugetar: seed inert; încercare de operare fără politica necesară, zero postări. | refuzat / inert | specificat |
| SC-DVI-13 | Bază 100, taxă culeasă 21,03, toleranța implicită null: D 4426 / C 446: 21,03 contabil, pereche fiscală 100 / 100; jurnal 100 / 21,03. | acceptat | specificat |
| SC-DVI-14 | Două DVI cu baze 100 și 100,01, taxe calculate 21 și 21: cubul păstrează distinct bazele; jurnal cumulat 200,01 / 42. | acceptat | specificat |
| SC-DVI-15 | Bază 0,01 IMP21, taxă calculată 0: exact 2 postări fiscale, contabil 0, zero partidă. Storno → −0,01 fiscal, net 0; originalele intacte. | acceptat | specificat |
| SC-DVI-16 | Pe SC-DVI-01: rulaje contabile D 21 / C 21; carte fiscală D 100 / C 100; sold 4426 contabil 21, inclusiv pe partener, și fiscal 0 pe cont fără filtrare de partener. ReconciliereCub litera (a): zero diferențe. Martor fără filtru Carte: rulaje totale 121 / 121, sold 4426 × partener 121. Jurnal 100 / 21 și două capete fiscale fără unități. | acceptat | specificat |
| SC-DVI-17 | DVI pe 401 cu taxă 21 → NTC 401 = 5121: 10 pe același partener: rest −11; inversarea DVI cu NTC activ refuzată atomic. Storno NTC redeschide −21, apoi storno DVI → 0 contabil și 0 fiscal cumulat. | acceptat / refuz dependență | specificat |
| SC-DVI-18 | Același document întârziat ca SC-DVI-07, dar ianuarie definit și încă deschis: fiscal 100 / 21 în ianuarie, contabil 21 în februarie. O pereche distinctă de fixture față de SC-DVI-07. | acceptat | specificat |
| SC-DVI-19 | Adaptorul CubDinRegistre + Normalizari.Fiscal pe SC-DVI-01 / SC-DVI-09 / SC-DVI-13: exact cele 4 postări din contract, inclusiv perechea fiscală; pe SC-DVI-15: exact 2 postări fiscale. Bazele 100 și 0,01 rămân distincte de taxe; zero diagnostic de bază fără țintă pe DVI. Constante independente de declarant și cub, contor al normalizării. | acceptat | specificat |
| SC-DVI-20 | Proba pură Transferuri.Muta: stins cu partidă 401 de −21, stingător cu partidă 401 de +10, cerere 10 → mutare 10. Adăugarea perechii fiscale 100 / 100 fără unități la oricare dintre cele două intrări lasă rezultatul identic. Nu operează o împerechere DVI interzisă de PoateFiStins. | acceptat la nivelul funcției pure | specificat |

Unicitatea faptelor se probează și transversal prin SC-X-14 din README,
detaliat în DVI-B7: FCT, FCL, RDC, RLF și DVI, plus absența explicită
pe liniile fără TVA din celelalte tipuri. Probele sunt specificate,
nu implementate sau rulate prin această revizie a contractului.

Probe viitoare în `ScenariiDvi`; familia existentă `DVI-V*` verifică
regimul actual pe registre, nu rezolvă absența bazei din cub. Ajustarea
costului pe lot și SC-X-13 rămân TR-D9; 86-r2 (dependența FCT legate)
rămâne de tratat explicit, fără a o declara rezolvată prin aceste probe.
