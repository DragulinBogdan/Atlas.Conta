# FCL — factură de ieșire

**2026-09-23: primul lot independent verificat pe profilurile aplicabile.** Reguli: TR-D7b T-D4,
088 (h,j), 090, 091; TVA conform P1. Probe: `ScenariiVanzare`, anul2005.
Privat: serviciu704, client4111, TVA4427; bugetar:751.01.00 și411.01.01,
fără politică TVA și fără RolTert. Așteptările sunt constante RON.

| ID | Pași și așteptare | Rezultat | Proveniență | Proba | Stare |
|---|---|---|---|---|---|
| SC-FCL-01 | Serviciu100 fără TVA; dry-run fără efecte; D client100, C venit100 pe gestiunea emitentă; nicio postare de stoc. | acceptat | T-D4 | `ScenariiVanzare` / ID-ul rândului | verificat pe ambele profiluri |
| SC-FCL-02 | Două linii100 și50 fără TVA → creanță150, patru postări, aceeași partidă4111 la privat. | acceptat | 10.778 documente multiline | `ScenariiVanzare` / ID-ul rândului | verificat pe ambele profiluri |
| SC-FCL-03 | Storno100 în aceeași perioadă →−100, partidă0, original intact; repetare refuzată. | acceptat / refuz repetare | 090i | `ScenariiVanzare` / ID-ul rândului | verificat pe ambele profiluri |
| SC-FCL-04 | Ianuarie închis, storno februarie; factura100+21 la privat → creanță121 la31.01,0 în februarie, fiscalul inversului în februarie. Bugetar fără TVA:100→0. | acceptat | 088 | `ScenariiVanzare` / ID-ul rândului | verificat pe ambele profiluri |
| SC-FCL-05 | Anulare100 în luna deschisă → Draft și zero efecte. | acceptat | invariant III | `ScenariiVanzare` / ID-ul rândului | verificat pe ambele profiluri |
| SC-FCL-06 | Corecție EroareMateriala în februarie: inversarea facturii100(+21 privat), document legat cu preț80 → creanță96,80 privat/80 bugetar; fiscalul corecției în ianuarie. | acceptat | 088h | `ScenariiVanzare` / ID-ul rândului | verificat pe ambele profiluri |
| SC-FCL-07 | Privat: net100 N21 + net50 N11 → venit150, TVA26,50, creanță176,50; nicio cantitate. | acceptat | 19 documente multicotă | `ScenariiVanzare` / ID-ul rândului | verificat privat; neaplicabil bugetar |
| SC-FCL-08 | Cantitate0: CANTITATE_NEPOZITIVA prin declarație, refuz pe comandă și zero efecte. | refuzat | contract declarant | `ScenariiVanzare` / ID-ul rândului | verificat pe ambele profiluri |
| SC-FCL-09 | Factură100→INC40→împerechere: creanță60; desfacere prin invers datat după închiderea lunii → creanță100, partida INC−40. Transferuri Credit−40/+40, apoi inversul; operările intacte. | acceptat | 031d,088j,T-D4 | `ScenariiVanzare` / ID-ul rândului | verificat pe ambele profiluri |
| SC-FCL-10 | Privat: recepție10/100, vânzare4×20 fără TVA → venit80/creanță80; DSC conex Draft; operare DSC → cost40, stoc6/60. Storno FCL cu DSC operat refuzat. | acceptat / refuz dependență | T-D4 | `ScenariiVanzare` / ID-ul rândului | verificat privat; neaplicabil bugetar |
| SC-FCL-11 | Privat: trei linii net 10,03 la N21, fără taxă culeasă. Taxa documentului = 30,09 × 21% = 6,3189 → 6,32 (nu 3 × 2,11 = 6,33), repartizată 2,11 / 2,11 / 2,10. Draftul cules arată deja aceste taxe și totalul 36,41; după operare liniile, totalul documentului, TVA 4427 și creanța 4111 sunt aceleași: 36,41. Încasarea de 36,41 stinge integral partida. Taxa unei linii modificată după operare e refuzată de INV-CUB (`CITIRE_TAXA_DIFERITA_DE_LINIE`). | acceptat / refuz invariant | 109 (a,b); recensământ Flax ianuarie 2025: 481 din 4.926 de documente cu taxă nemarcată aveau Σ linii ≠ taxa postată | `ScenariiVanzare` / ID-ul rândului | verificat privat; neaplicabil bugetar |
| SC-FCL-12 | Privat: aceleași trei linii, prima cu taxa culeasă 2,15. Taxa marcată rămâne; nemarcatele primesc repartizarea documentului, 2,11 și 2,10. TVA 6,36, creanță și total 36,45, pe draft și după operare. | acceptat | 109 (a), 103 (i) | `ScenariiVanzare` / ID-ul rândului | verificat privat; neaplicabil bugetar |

La bugetar SC-FCL-07/10 sunt neaplicabile (neplătitor; Stoc interzis pe
FCL, fără DSC); în SC-FCL-09 se verifică relațiile și postările contabile,
nu se pretinde o partidă pe cont fără RolTert. Cititorii fișă/balanță la
TR-D8; reevaluarea la TR-D9. Avansul/regularizarea SC-X-03, taxarea inversă,
valuta, reducerile/retururile semnate și concurența cer rânduri suplimentare.

Recensământ Flax read-only2026-09-23:40.535 documente,51.570 linii,
maxim3/document,10.778 multiline,19 multicotă,3.391 linii cu valoare sau
cantitate negativă, zero corecții legate sau înregistrări întârziate.
Liniile negative cer clasificare de domeniu; nu devin automat reguli noi.

Validare finală: 26 verificări independente bugetar / 75 privat (grupul comun FCL/DSC; DSC numai privat).
Include verificările comune de nemodificare la storno/corecție.
Gate integral pe ambele profiluri: `run-verificari/20260923-010027-252/rezultat.json`,
1.699 / 2.089 verificări, zero eșecuri.

Extensie SC-X-14 (2026-09-23): FCL TI21 și SDD: bază 100, taxă 0; exact un fapt Bază și niciun fapt Taxă.
Sunt probate operarea și stornoul în aceeași lună, cu măsuri inverse, prin
matrice independente și martori de duplicare/lipsă. Aceasta nu certifică
ciclul complet al regimurilor speciale peste perioadă.

## Extinderea partidelor bugetare — 2026-09-25

Decizia 100 activează urmărirea pe 401.01.00, 404.01.00 și 411.01.01,
fără rol comercial SAF-T. Așteptările de partidă (rest, transfer, storno,
corecție) ale ciclurilor comune se aplică acum ambelor profiluri.
Mențiunile anterioare „numai privat” descriu acoperirea de la data probării
inițiale; fiscalul, DSC și contul 419 rămân specifice profilului privat.
Probele TR-D8 SC-CIT-41…45 verifică separat politica și istoricul.
