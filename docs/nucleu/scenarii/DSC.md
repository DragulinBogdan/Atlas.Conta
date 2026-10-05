# DSC — descărcare de gestiune

**2026-09-23: primul lot independent verificat pe profilurile aplicabile.** Numai privat; bugetarul nu
configurează DSC. Reguli: P2, TR-D7b T-D4/13,090 (g,j),088,091.
Probe: `ScenariiVanzare`. Fixture2005: recepții reale FCT→NIR, loturi371,
cost607, gestiune MAG1, client propriu. DSC manual este legal (P2 §3).
Fiecare ieșire q/v: C371 v/−q pe lot și MAG1; D607 v/+q pe gestiunea
virtuală Client, cu partenerul client, fără unitate. Nu deschide partidă.

| ID | Pași și așteptare | Rezultat | Proveniență | Proba | Stare |
|---|---|---|---|---|---|
| SC-DSC-01 | Intrare10/100, dry-run, descărcare4/40; sold6/60 și cost40. | acceptat | T-D4 | `ScenariiVanzare` / ID-ul rândului | verificat privat; neaplicabil bugetar |
| SC-DSC-02 | Loturi10/100 și5/100; descărcare2/20 și3/60; solduri8/80 și2/40, cost80. | acceptat | 10.385 documente multiline | `ScenariiVanzare` / ID-ul rândului | verificat privat; neaplicabil bugetar |
| SC-DSC-03 | Storno4/40 în aceeași lună → stoc10/100 și cost0; original păstrat, repetare refuzată. | acceptat / refuz repetare | 090i | `ScenariiVanzare` / ID-ul rândului | verificat privat; neaplicabil bugetar |
| SC-DSC-04 | Descărcare4/40 în ianuarie închis; storno februarie: ianuarie6/60, februarie10/100. | acceptat | 088 | `ScenariiVanzare` / ID-ul rândului | verificat privat; neaplicabil bugetar |
| SC-DSC-05 | Anulare4/40 în luna deschisă → Draft, zero efecte proprii; lot10/100. | acceptat | invariant III | `ScenariiVanzare` / ID-ul rândului | verificat privat; neaplicabil bugetar |
| SC-DSC-06 | Corecție legată în februarie a descărcării4/40: invers, noua ieșire3/30; lot7/70, cost net30. | acceptat | 088h | `ScenariiVanzare` / ID-ul rândului | verificat privat; neaplicabil bugetar |
| SC-DSC-07 | Lot3/1; două linii1 și2 → cost0,33+0,67; lot0/0. | acceptat | 325 documente cu lot repetat; golirea ia restul | `ScenariiVanzare` / ID-ul rândului | verificat privat; neaplicabil bugetar |
| SC-DSC-08 | Ieșire11 din10: STOC_INSUFICIENT prin declarație; comanda refuzată de gardianul registrului, zero efecte. | refuzat | 090j | `ScenariiVanzare` / ID-ul rândului | verificat privat; neaplicabil bugetar |
| SC-DSC-09 | Bugetar: zero reguli DSC, tip în afara cubului; lot 10/100, descărcare 2. Azi: dry-run și operare refuzate de frunză („nu are regulă de contare de cost"), cu sau fără număr cules; zero efecte, lot 10/100. După tăiere (pasul 6): refuz cu codul unic al tipului fără politică pe profil (propus `TIP_FARA_DECLARATIE`). | refuzat / inert | D9-D5, D9-A6 (I6) | `ScenariiTaiere` / ID-ul rândului | verificat azi, bugetar |

Lanțul real FCL→DSC este SC-FCL-10. Cititorii de producție la TR-D8,
reevaluarea/compensările la TR-D9; FIFO pe mai multe loturi ale aceluiași
produs, reintrarea și concurența rămân de extins. Acest lot nu certifică
întregul catalog obligatoriu din README.

Recensământ Flax read-only2026-09-23:36.696 documente,62.063 linii,
maxim46/document,10.385 multiline,325 cu lot repetat; zero linii negative,
corecții legate sau înregistrări întârziate.

Validare finală: 26 verificări independente bugetar / 75 privat (grupul comun FCL/DSC; DSC numai privat).
Include verificările comune de nemodificare la storno/corecție.
Gate integral pe ambele profiluri: `run-verificari/20260923-010027-252/rezultat.json`,
1.699 / 2.089 verificări, zero eșecuri.
