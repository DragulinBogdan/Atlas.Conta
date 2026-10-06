# DVI — declarație vamală

2026-09-23: implementat și probat prin `ScenariiDvi`, numai pe privat;
profilul bugetar este inert. **Baza distinctă în Carte=Fiscal** urmează
[`tr-d7b-dvi-baza-fiscala-contract.md`](../tr-d7b-dvi-baza-fiscala-contract.md).
Contracte: 086, 090 (a/k), TR-D7b T-D8; recensământul read-only din
`recensamant-retururi-dvi.sql` găsește zero DVI pe Flax.

## Contraexemplul rezolvat prin completarea contractului

DVI cu bază 100 și TVA 21 are numai nota contabilă 4426 = 446: 21. `Fiscal.Impozitul`
poate păstra taxa, codul și perioada; `Fiscal.CuFapt` atașează rolul Bază
unei postări de net, dar DVI nu are acea postare. Fără perechea fiscală,
baza 100 lipsea din cub. Nici recuperarea din cotă nu este exactă: bazele 100 și 100,01 pot
produce aceeași taxă 21 după rotunjire, iar taxa culeasă este autoritară.

Forma inițială T-D8 cerea numai postarea taxei; B-D8 pct. 5 presupunea netul.
Aplicarea lor literală nu poate reconstrui jurnalul fiscal din cub.
Nu este B-r4 (faptul colectat la taxare inversă), nici DVI pe loturi.
Adaptorul vechi `Normalizari.Fiscal` păstra baza doar ca diagnostic.
Acum contextul identifică explicit DVI și reconstruiește perechea fiscală,
inclusiv fără postare de taxă din care să poată deduce contul.

**Direcție aleasă:** baza distinctă în `Carte=Fiscal`, în același cub,
balansată în propria carte. Forma implementată DVI-B2: D 4426 / C 4426: 100 fiscal,
cu rol Bază numai pe debit, fără unitate pe ambele capete. Taxa rămâne
D 4426 / C 446: 21 contabil. Rulajul contabil și datoria rămân 21; jurnalul
citește baza 100 și taxa 21 o singură dată. Alegerea contului drept ancoră
și probele exacte sunt în contractul de mai sus.

## Așteptări independente de reprezentarea aleasă

Privat: taxe 4426 / 4427, contrapartidă 446 sau 401 din partener. Fără stoc.
Bugetar: profil inert, fără politică și fără activare cub. Coloana Bază
se referă la proiecția fiscală, nu la o sumă adăugată balanței contabile.

| ID | Pași și așteptare numerică | Rezultat | Stare |
|---|---|---|---|
| SC-DVI-01 | Bază 100 IMP21 → D 4426 / C 446: 21 contabil și D 4426 / C 4426: 100 fiscal, cu rol Bază numai pe debit; proiecție fiscală 100 / 21. Dry-run fără efecte, apoi 4 postări exacte. | acceptat | verificat privat |
| SC-DVI-02 | Linii 100 IMP21 + 50 IMP11 → taxe 21 + 5,50; total contabil 26,50; două baze fiscale 100 / 50. | acceptat | verificat privat |
| SC-DVI-03 | Anulare SC-DVI-01 în luna deschisă → zero efecte; reoperare → aceleași 100 / 21, fără dublare. | acceptat | verificat privat |
| SC-DVI-04 | Storno SC-DVI-01 în aceeași lună → −21 contabil, −100 / −21 fiscal; net 0; originalele intacte, repetarea refuzată. | acceptat / refuz stare | verificat privat |
| SC-DVI-05 | Ianuarie închis: anularea refuzată; storno februarie → −100 / −21 fiscal în februarie; ianuarie rămâne 100 / 21. | acceptat pentru storno | verificat privat |
| SC-DVI-06 | Corecție în februarie a bazei 100 la 80, eroare materială: original inversat, draft legat; noua taxă 16,80; fiscal ianuarie net 80 / 16,80. | acceptat | verificat privat |
| SC-DVI-07 | Data 05.01, înregistrare 05.02 după închiderea lui ianuarie: contabil 0 la 31.01 și 21 în februarie; fiscal 100 / 21 declarat în februarie, politica deductibilului din 088. | acceptat | verificat privat |
| SC-DVI-08 | Comisionar cu cont implicit 401: D 4426 / C 401: 21; partidă proprie −21; baza 100 nu adaugă datorie și nu intră în FIFO. Storno → partidă 0. | acceptat | verificat privat |
| SC-DVI-09 | Bază 100 IMPTI21: D 4426 / C 4427: 21; bază 100 / taxă deductibilă 21; fără partidă; B-r4 privind faptul colectat rămâne explicită. | acceptat, limită declarată | verificat privat |
| SC-DVI-10 | MRN absent / fără linii / bază ≤ 0 / TVA obișnuit N21 / IMP cu cotă 0 / FCT legată neoperată: refuz cu zero efecte. | refuzat | verificat privat |
| SC-DVI-11 | Două FCT operate legate la aceeași DVI; operarea DVI nu dublează recepțiile/datoriile lor. DVI fără legături este permisă. | acceptat | verificat privat |
| SC-DVI-12 | Bugetar: fără politică de TVA pe tip. Dry-run și operare refuzate cu `TIP_FARA_DECLARATIE`, zero efecte. Înaintea tăierii refuza validarea frunzei („TVA de import"). | refuzat | verificat, bugetar (TR-D9a, pasul 6) |
| SC-DVI-13 | Bază 100, taxă culeasă 21,03, toleranța implicită null: D 4426 / C 446: 21,03 contabil, pereche fiscală 100 / 100; jurnal 100 / 21,03. | acceptat | verificat privat |
| SC-DVI-14 | Două DVI cu baze 100 și 100,01, taxe calculate 21 și 21: cubul păstrează distinct bazele; jurnal cumulat 200,01 / 42. | acceptat | verificat privat |
| SC-DVI-15 | Bază 0,01 IMP21, taxă calculată 0: exact 2 postări fiscale, contabil 0, zero partidă. Storno → −0,01 fiscal, net 0; originalele intacte. | acceptat | verificat privat |
| SC-DVI-16 | Pe SC-DVI-01: rulaje contabile D 21 / C 21; carte fiscală D 100 / C 100; sold 4426 contabil 21, inclusiv pe partener, și fiscal 0 pe cont fără filtrare de partener. Martor fără filtru Carte: rulaje totale 121 / 121, sold 4426 × partener 121. Jurnal 100 / 21 și două capete fiscale fără unități. | acceptat | verificat privat |
| SC-DVI-17 | DVI pe 401 cu taxă 21 → NTC 401 = 5121: 10 pe același partener: rest −11; inversarea DVI cu NTC activ refuzată atomic. Storno NTC redeschide −21, apoi storno DVI → 0 contabil și 0 fiscal cumulat. | acceptat / refuz dependență | verificat privat |
| SC-DVI-18 | Același document întârziat ca SC-DVI-07, dar ianuarie definit și încă deschis: fiscal 100 / 21 în ianuarie, contabil 21 în februarie. O pereche distinctă de fixture față de SC-DVI-07. | acceptat | verificat privat |
| SC-DVI-19 | Fără obiect după tăiere: proba era a adaptorului de oracol (`CubDinRegistre` + `Normalizari.Fiscal`). Cele 4 postări ale contractului rămân probate de SC-DVI-01 / 09 / 13 / 15. | scos | TR-D9a, pasul 6 |
| SC-DVI-20 | Proba pură Transferuri.Muta: stins cu partidă 401 de −21, stingător cu partidă 401 de +10, cerere 10 → mutare 10. Adăugarea perechii fiscale 100 / 100 fără unități la oricare dintre cele două intrări lasă rezultatul identic. Nu operează o împerechere DVI interzisă de PoateFiStins. | acceptat la nivelul funcției pure | verificat privat |

Unicitatea faptelor se probează și transversal prin SC-X-14 din README,
detaliat în DVI-B7: FCT, FCL, RDC, RLF și DVI, plus absența explicită
pe liniile fără TVA din celelalte tipuri. Proba comună `UnicitateFiscala` rulează din comparațiile exacte ale scenelor,
pe matricile numerice așteptate, și înaintea curățeniei. Include martori
în memorie pentru duplicarea bazei între cărți, duplicarea taxei și baza lipsă.

Probele `SC-DVI-01…20` sunt în `ScenariiDvi`; familia existentă `DVI-V*`
continuă să verifice regimul dual pe registre. SC-DVI-10 asertează codurile
stabile pentru liniile invalide prin declarație; MRN-ul și starea FCT legate
sunt validate de entitate. SC-DVI-19 verifică și gestiunea taxei din adaptor,
care lipsea înaintea normalizării DVI explicite. Ajustarea
costului pe lot și SC-X-13 rămân TR-D9; 86-r2 (dependența FCT legate)
rămâne de tratat explicit, fără a o declara rezolvată prin aceste probe.

## Validare (2026-09-23)

Comenzi secvențiale, prin `nou/tools/ModelCheck/scripts/verifica.ps1`, cu
`-Sufix .CodexBCS`:

- `-Suita Scenarii -Tip DVI -Profil Ambele`: **3 OK bugetar / 177 OK privat**,
  zero FAIL (`run-verificari/20260923-143523-005/rezultat.json`). Numărul
  include SC-X-14 și reverificarea celor 20 de matrici DVI înainte de curățenie.
- `-Suita Integral -Profil Ambele`: **1.829 / 2.897 OK**, zero FAIL
  (`run-verificari/20260923-143104-680/rezultat.json`).
- `-Suita Nucleu`: **165/165**, zero omise
  (`run-verificari/20260923-143452-956/rezultat.json`).

Build și procese exit 0, fără avertismente. Control read-only după rulări:
zero documente/postări/perioade în anii 2001–2010 și zero repartitori/produse
`E2E-SC-*` pe ambele baze. Raport local:
`run-nucleu/tr-d7b/pas4-dvi/raport.md`.

Review-ul a detectat și corectat pierderea gestiunii taxei în adaptorul
DVI; comparațiile au păstrat aceleași constante. Verificarea finală BCS
selectează documentele după repartitorul scenei, evitând extragerea anului
din date PostgreSQL infinite. Nu există modificări ale nucleului pur,
schemei, API-ului sau clientului. T-r10 se închide; T-r11, B-r4, T-r5/86-r11,
86-r2, DVI pe loturi și T-r8 rămân declarate.
