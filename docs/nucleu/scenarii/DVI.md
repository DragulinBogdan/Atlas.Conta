# DVI — declarație vamală

2026-09-23: catalog preliminar, **implementarea pe cub oprită la problema
de reprezentare a bazei**, înaintea activării seed-ului. Contract: 086,
090(a/k), TR-D7b T-D8; recensământul read-only din
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

**Propunere în așteptarea alegerii:** baza distinctă în `Carte=Fiscal`,
în același cub, balansată în propria carte. Contractul concret trebuie
să numească ambele capete/conturi, să excludă nominalizarea partidei pe
această mișcare și să precizeze citirea bazei exact o dată. Balanța și
restul de plată rămân21. Alternativa este amânarea DVI până la discutarea
reprezentării fiscale. Propunerea nu este încă decizie adoptată.

## Așteptări independente de reprezentarea aleasă

Privat: taxe4426/4427, contrapartidă446 sau401 din partener. Fără stoc.
Bugetar: profil inert, fără politică și fără activare cub. Coloana Bază
se referă la proiecția fiscală, nu la o sumă adăugată balanței contabile.

| ID | Pași și așteptare numerică | Rezultat | Stare |
|---|---|---|---|
| SC-DVI-01 | Bază100 IMP21 → D4426/C446:21; proiecție fiscală bază100/taxă21. Dry-run fără efecte, apoi operare. | acceptat | specificat; blocat de reprezentarea bazei |
| SC-DVI-02 | Linii100 IMP21 +50 IMP11 → taxe21+5,50; total contabil26,50; două baze fiscale100/50. | acceptat | specificat |
| SC-DVI-03 | Anulare SC01 în luna deschisă → zero efecte; reoperare → aceleași100/21, fără dublare. | acceptat | specificat |
| SC-DVI-04 | Storno SC01 în aceeași lună →−21 contabil, −100/−21 fiscal; net0; originalele intacte, repetarea refuzată. | acceptat / refuz stare | specificat |
| SC-DVI-05 | Ianuarie închis: anularea refuzată; storno februarie →−100/−21 fiscal în februarie; ianuarie rămâne100/21. | acceptat pentru storno | specificat |
| SC-DVI-06 | Corecție în februarie a bazei100 la80, eroare materială: original inversat, draft legat; noua taxă16,80; fiscal ianuarie net80/16,80. | acceptat | specificat |
| SC-DVI-07 | Data05.01, înregistrare05.02: contabil0 la31.01 și21 în februarie; fiscal100/21 declarat în ianuarie, conform088. | acceptat | specificat |
| SC-DVI-08 | Comisionar cu cont implicit401: D4426/C401:21; partidă proprie−21; baza100 nu adaugă datorie și nu intră în FIFO. Storno → partidă0. | acceptat | specificat |
| SC-DVI-09 | Bază100 IMPTI21: D4426/C4427:21; bază100/taxă deductibilă21; fără partidă; B-r4 privind faptul colectat rămâne explicită. | acceptat, limită declarată | specificat |
| SC-DVI-10 | MRN absent / fără linii / bază≤0 / TVA obișnuit N21 / IMP cu cotă0 / FCT legată neoperată: refuz cu zero efecte. | refuzat | specificat |
| SC-DVI-11 | Două FCT operate legate la aceeași DVI; operarea DVI nu dublează recepțiile/datoriile lor. DVI fără legături este permisă. | acceptat | specificat |
| SC-DVI-12 | Bugetar: seed inert; încercare de operare fără politica necesară, zero postări. | refuzat / inert | specificat |

Probe viitoare în `ScenariiDvi`; familia existentă `DVI-V*` verifică
regimul actual pe registre, nu rezolvă absența bazei din cub. Ajustarea
costului pe lot și SC-X-13 rămân TR-D9; 86-r2 (dependența FCT legate)
rămâne de tratat explicit, fără a o declara rezolvată prin aceste probe.
