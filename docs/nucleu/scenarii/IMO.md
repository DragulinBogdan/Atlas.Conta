# IMO — PIF, AMO, CAS complete pe cub

2026-09-24. Implementat și verificat pe ambele profiluri, conform 095 și
`tr-d7c-imobilizari-contract.md`, IMO-B2 A aprobată prin 097.
SC-IMO-01…25 sunt probate prin `ScenariiImo`, pe comenzile reale,
în ani proprii începând cu 2019. Review-ul advers al feliei rămâne de făcut.
Recensământ Flax: zero documente PIF/AMO/CAS.

Conturile simbolice I/A/E/C sunt contul activului, amortizării,
cheltuielii de amortizare și cedării, rezolvate din profil. Valorile
așteptate de mai jos sunt constante independente de registre.

| ID | Lanț / așteptare numerică | Rezultat așteptat |
|---|---|---|
| SC-IMO-01 | FCT I/404 1.200 → PIF: Transfer D I anonim −1.200, D I/fișă +1.200; balanța și rulajele I rămân 1.200; brut fiscal 900 cu echilibrare distinctă | acceptat în A |
| SC-IMO-02 | FCT 1.200 → două fișe 400 + 800; a treia alocare de 1 depășește suportul | primele acceptate, ultima refuzată atomic |
| SC-IMO-03 | Bază goală → PIF fără sursă 1.200/cumulat 200 | refuzat în A; nu se acceptă −1.200/−200 pe poziții anonime |
| SC-IMO-04 | Suport prin deschidere/NTC: I 1.200 și A 200 → PIF fără FCT: brut 1.200, cumulat 200, net 1.000; conturile nu se dublează | acceptat în A |
| SC-IMO-05 | Doi PIF concurenți de 800 pe același suport 1.200 | unul acceptat, unul refuzat; disponibil 400 |
| SC-IMO-06 | Storno FCT/NTC cu PIF viu | refuz; după inversarea PIF, suportul este din nou liber |
| SC-IMO-07 | PIF 1.200/12 luni contabil, 900/18 fiscal → prima AMO 100/50 | două perechi în cărți distincte; net 1.100/850 |
| SC-IMO-08 | Activ cu rest contabil 0, fiscal 100 → AMO fiscală 50 | zero postări contabile, net fiscal 50 |
| SC-IMO-09 | Liniară brut 100, durată 3 luni | 33,33 × 3 + 0,01 în luna suplimentară = 100, net final zero (F26-D7/F27-D4) |
| SC-IMO-10 | Modernizare +120 după o AMO pe brut 1.200/12: luna evenimentului păstrează 100; cota următoare din rest 1.120/10 | 112 din luna următoare; valoarea modernizării are suport |
| SC-IMO-11 | Revizuire după două luni: brut 1.200, cumulat 200, durată totală 22 | postare zero cauzată; luna următoare 50; anularea reface metoda/durata anterioară |
| SC-IMO-12 | CAS după o AMO: brut/cumulat 1.200/100 și 900/50 | descărcare 100 + 1.100 contabil, 50 + 850 fiscal; brut și cumulat fiecare zero |
| SC-IMO-13 | CAS cu cumulat 0; CAS complet amortizat | o singură pereche pe carte, fără dublare sau sumă negativă |
| SC-IMO-14 | Anulare/reoperare PIF, AMO, CAS fără dependenți în lună deschisă | zero efecte după anulare; valori identice la reoperare |
| SC-IMO-15 | Storno în luna documentului pentru fiecare eveniment, inclusiv revizuire zero | inverse exacte în ambele cărți; suport eliberat; parametrii revin; originale imuabile |
| SC-IMO-16 | Storno/corecție în altă lună ori după închiderea lunii documentului | refuzul explicit din 087(g); nicio scriere parțială |
| SC-IMO-17 | PIF cu AMO ulterioară; AMO cu CAS ulterior; CAS cu AMO generată ulterior | inversarea necronologică refuzată |
| SC-IMO-18 | PIF fizic ianuarie, înregistrat martie, 1.200/12 → AMO martie | 200 și Luni=2; aprilie 100 și Luni=1; ianuarie/februarie contabil neschimbate |
| SC-IMO-19 | Vehicul neexclusiv, AMO fiscală 8.000 în luna recuperării, plafon 1.500 privat | fiscal 8.000, deductibil 1.500 o singură dată; bugetar deductibil 8.000 |
| SC-IMO-20 | Schimbare Loc între februarie și martie | februarie 100 la locul vechi, martie 100 la locul nou; nicio rescriere |
| SC-IMO-21 | Fișă fără politică de amortizare: PIF cu suport permis, generatorul AMO refuză nominal; durată negativă, sursă FCT neoperată, AMO stale, CAS cu AMO în luna ieșirii | refuzuri fără scrieri parțiale prin comenzile reale; durata zero rămâne permisă pentru active neamortizabile |
| SC-IMO-22 | 2/51 fișe și linii | citiri pe set, număr constant de interogări; fără lazy loading pe fișă |
| SC-IMO-23 | Accelerată 1.200/36 luni | 50 pe primele 12 luni, apoi 25 în următoarele 24; total 1.200 |
| SC-IMO-24 | Degresivă AD1, 1.200/36 luni, k=1,5 | primul an 600 (12 × 50); apoi 300 + 300 (24 × 25); total 1.200 |
| SC-IMO-25 | Fișa 1.200/cumulat 200, reziduală contabilă 100, 10 luni rămase | contabil: 10 × 90 și net final 100; fiscal: 10 × 100 și net final zero, fără reziduală fiscală |

Citirile verifică atât sumele pe fișă, cât și balanța, rulajele și lipsa
efectului cărții fiscale în contabil. Probele 01–06 aplică alegerea A
aprobată prin 097. Nu se adaptează așteptările
la ieșirea motorului. Probele existente IMO-V rămân regresie, nu înlocuiesc
matricea numerică pe cub.

## Execuție și limite

`verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexBCS`:
**2.614 bugetar / 3.705 privat OK**, zero FAIL, exit 0;
`run-verificari/20260924-100942-750/rezultat.json`. Include și SC-CIT-01/02/06.
Nucleu: **179/179**, zero omise, exit 0;
`run-verificari/20260924-100513-273/rezultat.json`.

SC-IMO-22 măsoară strict contractarea pe ObjectSpace nou: **11/11 interogări**
la 2/51 fișe. Nu este o măsurătoare a întregii operări sau a istoricului mare.
Suplimentar, concurența PIF 800 contra anulării suportului 1.200 permite
exact un câștigător; nu rămâne o fișă fără suport. Migrația canonică a fost
aplicată ambelor baze de probă. Regenerarea metadata/OpenAPI/TypeScript nu
aduce drift, iar `tsc -b` trece.

Storno/corecția peste luna documentului rămân refuzate explicit (087g).
Blocajul tranzacțional IMO este comun pe bază; performanța concurenței la
volum mare nu este măsurată. Istoricul fără suport, fișă sau origine se
refuză la activare; nu este reconstruit implicit. Referința suportului poate
supraviețui anulării fizice a sursei deja eliberate: auditul complet rămâne
TR-D9. API Imo și AMO/CAS citesc fișa din cub; rapoartele contabile generale,
snapshot-urile și eliminarea scrierii duale nu sunt declarate migrate aici.
Raport: `run-nucleu/tr-d7c/imo/raport.md`.
