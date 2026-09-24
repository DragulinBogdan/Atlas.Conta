# DESCHIDERE — solduri inițiale detaliate

2026-09-24. Specificat înaintea implementării. Proveniență: 094,
DES-B1…B4, 090(d/g), 091(f), 092. Ambele profiluri, an propriu 2017.
Recepția inițială este comandă de motor fără document, excepția I;
nu se inserează postări direct din scenă.

S = 302 privat / 302.01.00 bugetar. Ancora se alege din profil.
Un cont de terț de probă cu RolTert și parteneri reali permite aceeași
probă de mecanism pe bugetar, al cărui seed nu marchează 401 ca RolTert.

| ID | Intrare și așteptare numerică | Rezultat |
|---|---|---|
| SC-DES-01 | D S 100, C ancoră 100; lot A 4/40, B 3/60. Exact trei postări; S=100, nu 200; loturile 4/40 și 3/60. | acceptat |
| SC-DES-02 | C terț 150, D ancoră 150; F1/ref1 60, F1/ref2 40, F2/ref1 50. Trei partide distincte, fără rând bloc pe terț. | acceptat |
| SC-DES-03 | PLT 20 → partida F1/ref1: rest 40, celelalte 40 și 50; inversare PLT reface 60. Originalul deschiderii intact. | acceptat |
| SC-DES-04 | Detalii stoc 99 sau 101 contra totalului 100; detalii cerute absente; detaliu fără sold de control. | refuz atomic |
| SC-DES-05 | Repetare după commit, două apeluri înainte de commit și două sesiuni care construiesc simultan deschiderea. | refuz, o singură tranzacție; a doua sesiune primește unique violation |
| SC-DES-06 | Cont/lot/gestiune/partener absent, cont diferit de al lotului, dată de lot ulterioară, referință goală, cantitate/valoare negativă, solduri neechilibrate. | refuz atomic |
| SC-DES-07 | Lot gratuit 2/0; cantitatea rămâne 2. Soldul creditor 150 din SC-DES-02 este păstrat pe Credit; cărțile nu se echilibrează între ele. | acceptat/refuz carte |
| SC-DES-08 | Ziua anterioară: zero; data deschiderii: soldurile 100 și 150; citire round-trip fără document sau linie fictivă. | acceptat |
| SC-DES-09 | PLT peste rest ori către alt partener, stingere anterioară deschiderii sau în lună închisă. | refuz atomic |
| SC-DES-10 | 2/51 poziții: citire pe set, număr constant de interogări; eșecul nu lasă entități noi de cub. | acceptat |

Storno-ul plății și refuzurile se execută prin comenzile reale. Deschiderea
nu are anulare/storno de document; resetarea bazei de test este curățenie.
Cititorii de producție rămân TR-D8, conectorul 1C rămâne migrare.

Fixture-ul persistent reunește SC-DES-01/02/07: trei loturi, trei partide
și două contraponderi, exact opt postări, D=C=250. Exemplul izolat cu
două loturi și trei postări se verifică separat, într-o tranzacție retrasă.
PLT 20 mută pe aceeași latură Debit -20 de pe partida proprie și +20 pe
partida inițială; suma pe cont și latură este zero. Refuzul peste rest
este probat separat cu PLT 70 care încearcă să stingă 61 din partida 60.

## Verificare, 2026-09-24

- Selectiv: `pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip DESCHIDERE -Profil Ambele -Sufix .CodexBCS`;
  exit 0 pe ambele profiluri, `run-verificari/20260924-020709-010/rezultat.json`.
- Integral, inclusiv probele suplimentare pe exemplul izolat și transfer:
  `pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexBCS`;
  **2.353 bugetar / 3.402 privat**, zero FAIL, build 0 warnings, exit 0:
  `run-verificari/20260924-021417-392/rezultat.json`.
- Perf pe aceeași bază, 2/51 loturi: **6/6 interogări** pe fiecare profil.
- Nucleul pur nu s-a schimbat în pasul Deschidere; ultima probă relevantă
  178/178: `run-verificari/20260923-235301-386/rezultat.json`.

Încercări intermediare: compilarea a prins folosirea purjei BaseObject pe
POCO; migrația a fost aplicată explicit înaintea scenariilor. Proba de
perf a eșuat cu 9/58 interogări, rezolvat prin eliminarea lazy loading la
crearea postărilor. Prima rulare integrală din
`20260924-021234-635` a prins o așteptare greșită în proba nouă de transfer:
regula aprobată mută +20/-20 pe aceeași latură, nu D20/C20. Așteptarea
a fost corectată conform conservării per cont/latură; codul transferului
nu s-a schimbat pentru acest rezultat.

SC-X-11 nu este declarat complet: citirile de producție și lanțul cu
documentul ulterior rămân la TR-D8. Review-ul advers este solicitat,
nu declarat efectuat.
