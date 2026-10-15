# Catalogul de scenarii — proba supremă a motorului (decizia 091)

**Actualizat: 2026-10-14.** Regula: `docs/decizii/091-scenarii-in-loc-de-import.md`,
literele (a)–(e). Un fișier per tip de document (`<COD>.md`), cu rândurile
de mai jos. Așteptarea se scrie de mână, din OMFP 1802 și din politica
profilului, cu cifre; nu se derivă din registre, din oracol sau din Flax.

[D9B — originea partidei](D9B.md) separă comportamentul actual verificat
numeric de persistența încă specificată; se execută prin `-Tip D9B`.

## Forma unui rând

| Coloană | Conținut |
|---|---|
| Id | `SC-<COD>-<NN>`; transversalele `SC-X-<NN>`, definite în [X.md](X.md) |
| Scenariu | documentul sau lanțul, cu datele care contează (cantități, prețuri, cote, perioade, unități numite) |
| Așteptare | postările pe cub (cont × latura × măsură × unitate) și citirile rezultate, cu cifre |
| Proba | numele check-ului ModelCheck (`SC-…`) sau al testului de nucleu |
| Proveniență | `regulă` (OMFP / politică), `recensământ Flax` (cu cifra găsită), `review` (Codex 2026-09-22 sau advers) |
| Rezultat așteptat | `acceptat` sau `refuzat`, cu codul stabil al refuzului dacă există; un refuz trebuie să lase persistarea neschimbată |
| Stare | `specificat` / `implementat` / `verificat`; pentru verificare se indică profilul și proba; `amânat la TR-D8/TR-D9` când depinde de un mecanism încă nelivrat |

Un lanț transversal se definește o singură dată, în [X.md](X.md): ce
probează. Realizarea lui executabilă, cu cifre, probă și stare, stă cel
mult într-un catalog de tip, ca rând cu id-ul `SC-X-NN` în prima celulă
sau ca alias după id-ul local (`SC-CVL-10 / SC-X-05`). Tabelele de
scenarii au prima coloană `ID`; un tabel de acoperire sau de stare
folosește alt antet (118 m).

Tabelul este indexul. Un scenariu complex are sub el starea inițială,
profilul/politica, comenzile în ordine cu date explicite și așteptarea după
FIECARE pas. Regula de domeniu se citează exact (decizie și literă ori sursă
normativă cu aplicabilitatea pe profil). O întrebare de domeniu nerezolvată
se marchează ca atare, nu devine așteptare prin copierea rezultatului motorului.
`verificat` se acordă numai după rularea probei; asocierea cu o probă veche
de egalitate cu registrele nu certifică scenariul independent (091c).

## Ciclul obligatoriu al fiecărui tip

Un tip nu e „pe cub" fără toate rândurile de mai jos, pe ambele profiluri
unde tipul există:

1. operare simplă, o linie;
2. operare cu mai multe linii pe unități diferite (loturi, partide, cote);
3. storno în aceeași perioadă;
4. storno peste graniță de perioadă (perioada fiscală re-ștampilată);
5. anulare (tranzacția dispare; unitățile născute dispar);
6. corecție în perioadă închisă: storno legat + document nou cu motiv;
7. stingere sau împerechere, unde tipul deschide partidă (transferul între
   partide, plafonul pe rest, desfacerea);
8. citirile după fiecare pas: sold pe unitate, fișă de cont, balanță la
   graniță, `Sold` reconstruit egal cu suma postărilor.

## Cazurile-limită obligatorii (unde tipul le poate produce)

- cantitate zero pe unitate ⇒ valoare zero (ultima ieșire ia restul);
- ieșire parțială cu rest, apoi golire, apoi re-intrare pe același produs;
- linie negativă (valoare sau cantitate) declarată semnată;
- lot născut „în roșu" (S-r7): refuz sau evaluare, ca așteptare explicită;
- avans + regularizare: două partide pe același document, soldul creanței e
  netul (S-D16);
- TVA pe mai multe cote cu rotunjire per document × cotă; taxare inversă
  (B-r4); regim `Capitalizat` (T-r8);
- valută cu curs (B-r6): `ValoareValuta` și `Valoare`, diferența de curs la
  stingere;
- document întârziat (`DataInregistrare` ≠ `Data`);
- storno de storno refuzat; storno al unui document cu dependenți refuzat;
- validare (dry-run) = operare pe același operand (S-r8, S-r11): același
  contract, aceleași refuzuri;
- două sesiuni reale pe același lot / aceeași partidă / operare simultană cu
  închiderea (F27-r8, S-r9) — condiție a lui „rotund", nu a pasului.

## Tipurile și starea lor

| Cod | Tip | Pe cub din | Fișier | Stare catalog |
|---|---|---|---|---|
| BCS | bon de consum | felia 31 | [BCS.md](BCS.md) | primul lot independent verificat pe ambele profiluri; ciclul complet încă deschis |
| FCT | factură intrare | felia 31 | [FCT.md](FCT.md) | primul lot independent verificat pe ambele profiluri; ciclul complet încă deschis |
| PLT | plată | felia 31 | [PLT.md](PLT.md) | primul lot independent verificat pe ambele profiluri; ciclul complet încă deschis |
| INC | încasare | felia 31 | [INC.md](INC.md) | primul lot independent verificat pe ambele profiluri; ciclul complet încă deschis |
| BTR | notă de transfer | pas 1 | [BTR.md](BTR.md) | primul lot independent verificat pe ambele profiluri; ciclul complet încă deschis |
| FCL | factură ieșire | pas 2 | [FCL.md](FCL.md) | primul lot independent verificat pe ambele profiluri; ciclul complet încă deschis |
| DSC | descărcare de gestiune | pas 2 (privat) | [DSC.md](DSC.md) | primul lot independent verificat pe privat; neaplicabil bugetar; ciclul complet încă deschis |
| NTC | notă contabilă | pas 3 | [NTC.md](NTC.md) | declarant explicit, FIFO și identitate cu partener (092); cititorii comuni rămân TR-D8 |
| ITV | închidere TVA | pas 3 (privat) | [ITV.md](ITV.md) | declarant comun NTC, corecție peste perioadă; profil inert bugetar; cititorii comuni rămân TR-D8 |
| REGIM | regimul pe stare (transversal) | 106 | [REGIM.md](REGIM.md) | editabilitatea și comenzile disponibile, cu motiv, pe Draft/Operat/Stornat, împerechere, conex operat, perioadă închisă; ReadDto = regim |
| X | lanțuri transversale | — | [X.md](X.md) | SC-X-01…27; realizările în cataloagele tipurilor: SC-X-01/02/04/08/09 și SC-X-05/06/07 (ca SC-CVL-10…12), SC-X-14 în `UnicitateFiscala`; SC-X-15…27 verificate pe ambele profiluri (`--scenarii X`); SC-X-03 și SC-X-10…13 definite, fără realizare |
| RDC | retur de la client | pas 4 (privat) | [RDC.md](RDC.md) | venit/cost, partidă proprie negativă, compensare NTC, ciclu mixt peste perioadă; inert bugetar; cititorii comuni TR-D8 |
| RLF | retur la furnizor | pas 4 (privat) | [RLF.md](RLF.md) | valoare fiscală, reziduu pe lot gol, compensare NTC, ciclu peste perioadă; inert bugetar |
| DVI | declarație vamală | pas 4 (privat) | [DVI.md](DVI.md) | bază distinctă în Carte=Fiscal, 20 de scenarii, cicluri complete și SC-X-14; inert bugetar |
| ASM | asamblare | pas 5 | [ASM.md](ASM.md) | transformare n→m, Δ dual pe grup, storno mixt și corecție pe ambele profiluri; SC-X-09 privat; TR-D8/TR-D9 rămân explicite |
| LDI | listă de inventar | pas 5 | [LDI.md](LDI.md) | Magazie/Marfuri pe ambele profiluri; Folosinta în gestiune reală pe lanțul FCT/NIR/BTR/BCS/LDI, inversări, corecție și istoric (093); Custodie explicit neacoperită |
| NIR | recepție manuală și conex | pas 5 | [NIR.md](NIR.md) | verificat pe ambele profiluri: ciclul complet, stingeri, dependenți, Folosinta/Marfuri; conexul sursei migrate nu dublează cubul |
| — | `Deschidere` | pas 6 | [DESCHIDERE.md](DESCHIDERE.md) | detaliere fără dublare (094), refuz atomic, unicitate, stingere/anulare/storno sub blocare comună; analiza/valuta păstrate la intrare, stingerea în valută neacoperită; cititorii de producție rămân TR-D8 |
| — | coordonatele postării (transversal) | C111 | [C111.md](C111.md) | 29 de rânduri pe formele din C111-D5 și pe S10–S14; verificate la pasul 2 pe ambele profiluri (`--scenarii C111`), în afară de SC-C111-21, care e a pasului 4 |
| CVL (mecanism propus pe FCT) | corecție valorică pe lot | TR-D9b-CV | [CVL.md](CVL.md) | 32 de scenarii documentate; `--scenarii CVL` rulează toate cele 32 de scenarii pe ambele profiluri; pachetul închis la 2026-10-15 (pasul 5); exportul SAF-T rămâne pe refuz (CV-r1) |
| DEC | decont | înainte de TR-D8 (095) | [DEC.md](DEC.md) | declarație contabilă/fiscală și partide pe angajat (096), verificări independente |
| PIF/CAS/AMO | imobilizări | pe cub (095, 097) | [IMO.md](IMO.md) | SC-IMO-01…25 verzi pe ambele profiluri; fișa citită din cub; review advers restant |

Rularea pe un tip: `ModelCheck --scenarii <TIP>[,<TIP>…] [privat]` (091-r1);
scena unui rând se înregistrează cu tipul ei în `ScenelePeTip`
(`nou/tools/ModelCheck/Program.cs`), altfel filtrul nu o vede.

Tipurile deja pe cub primesc fișierul înaintea pasului 3: probele
`STR-*`/`NUC-*` existente se mapează pe rândurile ciclului, iar rândurile
lipsă (corecția în perioadă închisă, citirile) se scriu și se probează
atunci. Recensământul pe clonă (091-r2) se face o dată per tip, înaintea
scenariilor lui, și își lasă cifrele în coloana „Proveniență".

## Lotul independent din 2026-09-23

FCT, PLT/INC, BTR și FCL/DSC au fixture-uri proprii, în anii 2002–2005,
prin `ScenaDocumente`: documente reale, ObjectSpace nou per comandă,
așteptări numerice explicite, curățenie după marcaj în `finally`.
`ScenariiFct`, `ScenariiTrezorerie`, `ScenariiBtr` și `ScenariiVanzare`
sunt înregistrate în `ScenelePeTip`, inclusiv în suita integrală.
Verificările comune `SC-…-IMUTABIL` confirmă că storno/corecția păstrează
identitățile, coordonatele și măsurile postărilor originale.

Recensământul tipurilor este reproductibil prin `recensamant-tipuri.sql`,
în tranzacție read-only pe Flax. Cifrele sunt în fișierele tipurilor.
În special, liniile negative FCT/FCL rămân întrebări de domeniu de clasificat;
nu au fost convertite în reguli după forma datelor importate.

Lanțuri probate în acest lot: SC-X-01/02 (regim dual FCT/NIR/BCS),
SC-X-04 (FCT/PLT, redeschiderea datoriei), SC-X-08 (BTR/BCS).
SC-FCL-09 probează FCL/INC, iar SC-FCL-10 separarea venitului de cost.
Cititorii comuni și snapshot-urile pe cub (TR-D8), reevaluarea/compensarea
(TR-D9), concurența și cazurile fiscale/valutare declarate în fiecare fișier
rămân deschise. Verdele acestui lot nu certifică încă „rotund” (091g).

Validarea finală: `run-verificari/20260923-010027-252/rezultat.json`,
ModelCheck integral **1.699 bugetar / 2.089 privat, zero eșecuri**.
Lotul adaugă 177 / 254 verificări independente față de baza cu BCS;
motorul, modelul și politicile nu au fost modificate.
Rulările pe grupe au fost urmate fiecare de gate integral pe ambele profiluri.

Comenzile au folosit `pwsh -NoProfile -File
nou/tools/ModelCheck/scripts/verifica.ps1 -Profil Ambele -Sufix .CodexBCS`,
cu `-Suita Scenarii -Tip FCT`, apoi `PLT,INC`, `BTR`, `FCL` (include DSC
pe privat) și `-Suita Integral` după fiecare grup. Ultimul gate cuprinde și
verificările comune de nemodificare. Nucleul pur nu a fost atins;
probele sale separate nu au fost rerulate în acest lot.

Controlul read-only după gate: zero repartitori/produse `E2E-SC-*`, zero
documente și perioade în anii fixture2001–2005, pe ambele baze `.CodexBCS`.

## Pasul 3 — NTC și ITV (2026-09-23)

Decizia 092 extinde identitatea partidei cu partenerul, păstrând ID-urile
istorice la citire și transfer. Scenariile noi verifică postarea explicită,
FIFO, partenerii multipli, dependențele în timp, regularizarea avansului,
storno/anulare/corecție și închiderea TVA. Lotul NTC/ITV: 41 verificări
bugetar / 215 privat; nucleu 165/165 teste. Cititorii comuni, fișa/balanța
și `Sold` reconstruibil rămân TR-D8.

Gate integral final, 2026-09-23: `verifica.ps1 -Suita Integral -Profil
Ambele -Sufix .CodexBCS`, **1.740 OK bugetar / 2.305 OK privat, 0 FAIL**,
exit 0; build cu 0 avertismente. Manifest:
`run-verificari/20260923-115007-397/rezultat.json`. Curățenie verificată
read-only: zero repartitori ai scenelor NTC/ITV/API-NTC, zero postări și
perioade din 2006–2007 pe ambele baze de test.

## Felia fiscală TR-D8 (103)

`verifica.ps1 -Suita Scenarii -Tip FISCALE -Profil Ambele -Sufix .FiscalCub`
rulează `ScenariiFiscale`, inclus și în `CITIRI`/integrală. HTTP securizat:
`python -X utf8 nou/tools/ProbeHttp/fiscal-cub.py --baza <bază privată izolată>`.
Proba HTTP cere hostul local la 5089 (sau `--host`), seed privat, conturile
Admin/Cititor/User cu parolă goală și anul 2021 fără perioade. Creează date,
roluri și confirmări temporare; curăță fixture-ul în `finally`.
Nu se rulează pe o bază de lucru a utilizatorului.

## SAF-T pe cub — S1 implementat, S2/S3 în delimitare

[SAFT.md](SAFT.md) conține matricea S1 GL/facturi și delimitarea S2 Payments,
S3 stocuri. [Contractul SAF-T](../tr-d8-saft-contract.md) integrează
SAF-D1/D2/D4=A, SAF-D3=C și S1 cu S1-D5 = B' (381 + 384), toate aprobate de
owner. S1 rulează prin `--scenarii SAFT` (anul 2024), plus verificările
SC-SAFT-10/11 din scenele ASM, IMO și DEC. Ruta publică L nu este comutată
(R1), iar validarea XSD/DUK a noului fișier așteaptă S0. Mapările și
derivarea cauzelor S3 au blocaje explicite.
