# PLT — plată

**2026-09-23: primul lot independent verificat pe ambele profiluri.** Reguli: 031 (a,c–e),
076 (f), 088 (j), 090 (c,f,i), 091 (a–e). Probe: `ScenariiTrezorerie`.
Fixture în anul 2003, CASA (privat 5311, bugetar 531.01.01), furnizor
propriu (401 / 401.01.00), tip TRZ. Privat: partidă pe document și cont;
bugetar: conturile fără RolTert nu nominalizează partide. Valorile sunt RON.

| ID | Pași și așteptare | Rezultat | Proveniență | Proba | Stare |
|---|---|---|---|---|---|
| SC-PLT-01 | Dry-run fără efecte; plata100 → D401=100 pe partida proprie, C5311=100 pe CASA; cantitate0. | acceptat | 031c | `ScenariiTrezorerie` / ID-ul rândului | verificat pe ambele profiluri aplicabile |
| SC-PLT-02 | Două linii40+60 → patru postări, total100, aceeași partidă de furnizor. | acceptat | defalcarea 031a; cazul multiline nu există în recensământ | `ScenariiTrezorerie` / ID-ul rândului | verificat pe ambele profiluri aplicabile |
| SC-PLT-03 | Storno100 în aceeași lună → postări−100, sold propriu0; originalul păstrat, al doilea storno refuzat. | acceptat / repetarea refuzată | 090i | `ScenariiTrezorerie` / ID-ul rândului | verificat pe ambele profiluri aplicabile |
| SC-PLT-04 | Plata100 în ianuarie, închidere, storno în februarie: sold100 la31.01,0 în februarie. | acceptat | 088j | `ScenariiTrezorerie` / ID-ul rândului | verificat pe ambele profiluri aplicabile |
| SC-PLT-05 | Anulare în luna deschisă → Draft, zero postări, tranzacții și registre proprii. | acceptat | invariant III | `ScenariiTrezorerie` / ID-ul rândului | verificat pe ambele profiluri aplicabile |
| SC-PLT-06 | Corecție legată în februarie a plății100 din ianuarie închis: original inversat; corecție80 → sold original0, sold corecție80. | acceptat | 088h | `ScenariiTrezorerie` / ID-ul rândului | verificat pe ambele profiluri aplicabile |
| SC-PLT-07 | Linie0: VALOARE_NEPOZITIVA prin declarație, refuz pe comandă, zero efecte. | refuzat | contract declarant | `ScenariiTrezorerie` / ID-ul rândului | verificat pe ambele profiluri aplicabile |
| SC-X-04 | FCT serviciu100; PLT40 împerecheată → rest datorie60; PLT60 împerecheată →0; ianuarie închis; storno PLT60 în februarie inversează și împerecherea → datorie60, partida PLT60=0; PLT40 rămâne stinsă. Încercarea101 peste rest refuzată fără scriere. | acceptat / depășirea refuzată | 031d amendat088j | `ScenariiTrezorerie` / ID-ul rândului | verificat pe ambele profiluri aplicabile |

La împerecherea manuală privat, transferul mută debitul pozitiv de pe partida
PLT pe cea FCT: −40/+40, apoi −60/+60, pe același cont și aceeași latură;
originalele de operare nu se rescriu. La bugetar nu există partide pe cub;
se probează ciclul comenzii și conservarea postărilor contabile, nu o
stingere pe unitate inexistentă. Stornoul cu împerechere în luna încă
deschisă este refuzat de gardianul dual; după închidere se scrie inversul.

Recensământ Flax read-only 2026-09-23 (`recensamant-tipuri.sql`): 2.486
documente și linii, toate monolinie; zero linii negative, corecții legate
sau înregistrări întârziate. Aceste absențe justifică fixture-uri sintetice.
Cititorii fișă/balanță/snapshot sunt amânați la TR-D8; împerecherea ca
document, avansul cu regularizare și valută, compensările la TR-D9 ori la
rânduri ulterioare. Viramentele și concurența rămân necertificate de acest lot.

Validare finală: 54 verificări independente bugetar / 70 privat (grupul comun PLT/INC).
Include verificările comune de nemodificare la storno/corecție.
Gate integral pe ambele profiluri: `run-verificari/20260923-010027-252/rezultat.json`,
1.699 / 2.089 verificări, zero eșecuri.
