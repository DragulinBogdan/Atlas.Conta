# BTR — transfer între gestiuni

**2026-09-23: primul lot independent verificat pe ambele profiluri.** Reguli: 090 (f,g,j),
TR-D7b T-D2.1, 088 (h), 091. Probe: `ScenariiBtr`, anul2004,
material302 / 302.01.00, FCT→NIR real, MAG1→gestiune proprie.
BTR mută același lot, pe același cont: două postări Debit, cu măsuri
opuse, în tranzacție `Transfer`. Nu naște lot sau partidă.

| ID | Pași și așteptare | Rezultat | Proveniență | Proba | Stare |
|---|---|---|---|---|---|
| SC-BTR-01 | Intrare10/100; dry-run fără efecte; transfer4/40: sursa D−4/−40, ținta D+4/+40; solduri6/60 și4/40, cont net100. | acceptat | T-D2.1 | `ScenariiBtr` / ID-ul rândului | verificat pe ambele profiluri |
| SC-BTR-02 | Loturi10/100 și5/100; transfer2/20 și3/60: sursa8/80 și2/40, ținta2/20 și3/60. | acceptat | 13.014 documente multiline în recensământ | `ScenariiBtr` / ID-ul rândului | verificat pe ambele profiluri |
| SC-BTR-03 | Storno transfer4/40 în aceeași lună: sursa+4/+40, ținta−4/−40; solduri10/100 și0/0; repetare refuzată. | acceptat / refuz repetare | 090i | `ScenariiBtr` / ID-ul rândului | verificat pe ambele profiluri |
| SC-BTR-04 | Transfer4/40 în ianuarie închis; storno februarie păstrează soldurile ianuarie6/60 și4/40; februarie10/100 și0/0. | acceptat | 088 | `ScenariiBtr` / ID-ul rândului | verificat pe ambele profiluri |
| SC-BTR-05 | Anulare transfer4/40 în luna deschisă: Draft fără postări; sursa10/100, ținta0/0. | acceptat | invariant III | `ScenariiBtr` / ID-ul rândului | verificat pe ambele profiluri |
| SC-BTR-06 | Corecție legată în februarie: transferul4/40 inversat, noul transfer3/30 → sursa7/70, ținta3/30. | acceptat | 088h | `ScenariiBtr` / ID-ul rândului | verificat pe ambele profiluri |
| SC-BTR-07 | Același lot3/1, două linii de transfer1 și2: valori0,33 și0,67; sursa0/0, ținta3/1. | acceptat | 235 documente cu lot repetat; ultima ieșire ia restul | `ScenariiBtr` / ID-ul rândului | verificat pe ambele profiluri |
| SC-BTR-08 | Transfer11 din10: STOC_INSUFICIENT prin declarație; refuzul comenzii prin gardianul dual de stoc, fără efecte. | refuzat | 090j | `ScenariiBtr` / ID-ul rândului | verificat pe ambele profiluri |
| SC-X-08 | Intrare10/100→BTR4/40→BCS1/10 din țintă; storno BTR refuzat: sursa6/60 și ținta3/30, consum10 rămân. | refuzat; gardian registru până la TR-D8 | 025d | `ScenariiBtr` / ID-ul rândului | verificat pe ambele profiluri |

Recensământ Flax read-only 2026-09-23: 45.552 documente,85.027 linii,
maxim67/document,13.014 multiline,235 cu lot repetat; zero linii negative,
corecții legate sau date întârziate. Cititorii de producție sunt la TR-D8,
reevaluările la TR-D9; concurența și transferul retroactiv cer rânduri
ulterioare. Ambele profiluri sunt aplicabile; TVA/stingerea nu se aplică BTR.

Validare finală: 46 verificări independente bugetar / 46 privat.
Include verificările comune de nemodificare la storno/corecție.
Gate integral pe ambele profiluri: `run-verificari/20260923-010027-252/rezultat.json`,
1.699 / 2.089 verificări, zero eșecuri.
