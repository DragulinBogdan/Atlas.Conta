# CITIRI — TR-D8

2026-09-24. Specificație înaintea portării rapoartelor. Surse: 090(f/k),
091(g/i), 097 și `tr-d8-citiri-contract.md`. Așteptările sunt constante;
o citire directă din cub în ModelCheck este proba independentă, nu sursa
de producție.

| ID | Scenariu | Așteptare | Stare |
|---|---|---|---|
| SC-CIT-01 | FCT 1.200 → PIF contabil 1.200/fiscal 900 → storno PIF | intrarea contabilă întoarce zero postări pentru PIF și inversa sa; FCT păstrează D/C 1.200 | verificat pe intrarea comună |
| SC-CIT-02 | AMO contabil 100/fiscal 50 → storno în aceeași lună | două postări contabile originale de 100, două inverse de −100; nicio postare fiscală în citirea contabilă | verificat pe intrarea comună |
| SC-CIT-03 | DVI: bază fiscală 100 și TVA contabilă 21 | pe 4426 rulaj D 21/C 0; perechea fiscală 100/100 nu intră în jurnal/fișă/balanță | verificat pe intrarea comună |
| SC-CIT-04 | ASM mixt: Operare D/C 40 și Transfer pe același cont; storno mixt | jurnalul include numai postările economice de 40/−40; exclude Transfer și inversele lui prin origine, plus contraponderile structurale | verificat pe intrarea comună |
| SC-CIT-05 | BTR pe același cont → storno | zero rânduri contabile generale, mișcările pe lot rămân vizibile | verificat pe intrarea comună |
| SC-CIT-06 | Deschidere D activ 1.200/C amortizare 200/C capital 1.000 → PIF | trei postări inițiale fără document; nicio dublare; sold net activ 1.200, amortizare 200 | verificat pe intrarea comună |
| SC-CIT-07 | Storno BTR 4/40 cu cele două origini istorice lipsă | refuz cu număr 2; backfill exact 2, repetare 0; citirea contabilă rămâne fără BTR | verificat pe ambele profiluri |
| SC-CIT-08 | Aceeași istorie cu doi candidați originali identici pentru o inversă | completează numai perechea univocă; inversa ambiguă rămâne fără origine și blochează citirea cu număr 1 | verificat pe ambele profiluri |

SC-CIT-07/08 sunt probe de migrare: fixture operațional real, alterări SQL
controlate numai în tranzacția de probă, rollback obligatoriu. Nu sunt o
cale de culegere a documentelor. Review advers amânat de owner până la
sincronizare, după NIR.

Acesta este primul lot al intrărilor comune. Nu certifică portarea
rapoartelor, snapshot-urilor, fiscalului, securității sau SAF-T. Matricea
completă se extinde înaintea fiecărei portări, conform inventarului TR-D8.

Execuție: `run-verificari/20260924-100942-750/rezultat.json`, integral pe
ambele profiluri, exit 0. SC-CIT-03 este privat (DVI); celelalte sunt pe
ambele profiluri. Probele apelează `Cub.Citiri.Contabil.Postari`; jurnalul,
fișa și balanța API nu au fost încă portate. Activarea generală a cititorului
cere mai întâi diagnosticul stornourilor istorice fără origine.

Corecturi TR-D8/DEC: selectiv `DEC,BTR,ASM`, 327/389 OK, manifest
`run-verificari/20260924-160814-726/rezultat.json`; integral 2.723 bugetar /
3.814 privat OK, zero FAIL, build fără avertismente, exit 0,
`run-verificari/20260924-160912-899/rezultat.json`.
Migrația `OriginiStornoUnivoce` completează proveniența univocă;
cititorul refuză restul cu numărul postărilor. Verificarea provenienței
este o interogare suplimentară pe domeniul contabil complet; performanța
pe istoric mare și portarea consumatorilor rămân în TR-D8.
