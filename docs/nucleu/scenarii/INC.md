# INC — încasare

**2026-09-23: primul lot independent verificat pe ambele profiluri.** Reguli și fixture comune:
[PLT.md](PLT.md). Probe: `ScenariiTrezorerie`; client propriu,
4111 / 411.01.01, CASA 5311 / 531.01.01. Tip TRZ, anul 2003.

| ID | Pași și așteptare | Rezultat | Proveniență | Proba | Stare |
|---|---|---|---|---|---|
| SC-INC-01 | Dry-run fără efecte; încasare100 → D5311=100 pe CASA, C4111=100 pe partida proprie, cantitate0. | acceptat | 031c | `ScenariiTrezorerie` / ID-ul rândului | verificat pe ambele profiluri aplicabile |
| SC-INC-02 | Două linii40+60 → patru postări, total100, aceeași partidă de client. | acceptat | 031a | `ScenariiTrezorerie` / ID-ul rândului | verificat pe ambele profiluri aplicabile |
| SC-INC-03 | Storno100 în aceeași lună →−100, sold propriu0; originalul păstrat, repetarea refuzată. | acceptat / repetarea refuzată | 090i | `ScenariiTrezorerie` / ID-ul rândului | verificat pe ambele profiluri aplicabile |
| SC-INC-04 | Încasare100 în ianuarie închis; storno februarie → sold−100 la31.01,0 în februarie. | acceptat | 088 | `ScenariiTrezorerie` / ID-ul rândului | verificat pe ambele profiluri aplicabile |
| SC-INC-05 | Anulare în luna deschisă → Draft și zero efecte proprii. | acceptat | invariant III | `ScenariiTrezorerie` / ID-ul rândului | verificat pe ambele profiluri aplicabile |
| SC-INC-06 | Corecție în februarie a încasării100 din ianuarie închis: original inversat; corecție80 → sold original0, corecție−80. | acceptat | 088h | `ScenariiTrezorerie` / ID-ul rândului | verificat pe ambele profiluri aplicabile |
| SC-INC-07 | Linie0: VALOARE_NEPOZITIVA prin declarație și refuz pe comandă, fără efecte. | refuzat | contract declarant | `ScenariiTrezorerie` / ID-ul rândului | verificat pe ambele profiluri aplicabile |
| SC-INC-09 | Oglinda lui SC-PLT-09: regulă cu ambele conturi explicite (D cont propriu / C462), predator creditor, primitor cont propriu de mandat. Cu flag pe contul propriu: refuz pe debit, zero efecte. Fără flag: C462 cu partenerul creditorului, D cont propriu fără repartitor. | refuzat / acceptat | D9-6B-R1 (2026-10-07) | `ScenariiTrezorerie` / ID-ul rândului | verificat pe ambele profiluri |

Stingerea FCL→INC este probată în [FCL.md](FCL.md); SC-X-03 cu avans și
retur rămâne deschis. Conturile bugetare nu au RolTert, deci probele de
partidă sunt explicit neaplicabile acolo. Limitele PLT se aplică și aici.

Recensământ Flax read-only 2026-09-23: 31.381 documente și linii, toate
monolinie; zero linii negative, corecții legate și înregistrări întârziate.

Validare finală: 54 verificări independente bugetar / 70 privat (grupul comun PLT/INC).
Include verificările comune de nemodificare la storno/corecție.
Gate integral pe ambele profiluri: `run-verificari/20260923-010027-252/rezultat.json`,
1.699 / 2.089 verificări, zero eșecuri.

## Extinderea partidelor bugetare — 2026-09-25

Decizia 100 activează urmărirea pe 401.01.00, 404.01.00 și 411.01.01,
fără rol comercial SAF-T. Așteptările de partidă (rest, transfer, storno,
corecție) ale ciclurilor comune se aplică acum ambelor profiluri.
Mențiunile anterioare „numai privat” descriu acoperirea de la data probării
inițiale; fiscalul, DSC și contul 419 rămân specifice profilului privat.
Probele TR-D8 SC-CIT-41…45 verifică separat politica și istoricul.
