# Catalogul de scenarii — proba supremă a motorului (decizia 091)

**Actualizat: 2026-09-23.** Regula: `docs/decizii/091-scenarii-in-loc-de-import.md`,
literele (a)–(e). Un fișier per tip de document (`<COD>.md`), cu rândurile
de mai jos. Așteptarea se scrie de mână, din OMFP 1802 și din politica
profilului, cu cifre; nu se derivă din registre, din oracol sau din Flax.

## Forma unui rând

| Coloană | Conținut |
|---|---|
| Id | `SC-<COD>-<NN>`; transversalele `SC-X-<NN>` |
| Scenariu | documentul sau lanțul, cu datele care contează (cantități, prețuri, cote, perioade, unități numite) |
| Așteptare | postările pe cub (cont × latura × măsură × unitate) și citirile rezultate, cu cifre |
| Proba | numele check-ului ModelCheck (`SC-…`) sau al testului de nucleu |
| Proveniență | `regulă` (OMFP / politică), `recensământ Flax` (cu cifra găsită), `review` (Codex 2026-09-22 sau advers) |
| Rezultat așteptat | `acceptat` sau `refuzat`, cu codul stabil al refuzului dacă există; un refuz trebuie să lase persistarea neschimbată |
| Stare | `specificat` / `implementat` / `verificat`; pentru verificare se indică profilul și proba; `amânat la TR-D8/TR-D9` când depinde de un mecanism încă nelivrat |

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

## Lanțurile transversale (`SC-X-*`)

Cele mai valoroase scenarii sunt cele pe care niciun import nu le exercită
și niciun tip nu le poartă singur:

| Id | Lanț | Ce probează |
|---|---|---|
| SC-X-01 | FCT cu recepție → BCS din lotul recepționat → storno FCT | refuzul stornoului cu dependenți; lotul cu ieșiri nu dispare |
| SC-X-02 | FCT → NIR conex operat → BCS | o singură recepție în cub (T-D9, `PoliticaConex`) |
| SC-X-03 | FCL cu avans (INC anterior) → transfer INC → FCL → RDC parțial | două partide, plafonul pe rest, creanța netă după retur |
| SC-X-04 | FCT → PLT parțială → PLT rest → storno PLT a doua | partida se redeschide exact cu suma stornată |
| SC-X-05 | recepție 100 × 10 → BCS 60 → corecție de preț +200 → storno BCS | 091 (h): inversare 600 + compensare 120 înapoi pe lot; datoria rămâne |
| SC-X-06 | recepție 100 × 10 → BCS 60 → corecție de preț +200 → storno corecție → storno BCS | corecția este inversată înaintea consumului; inversarea BCS readuce 600 pe lot, fără compensarea încă o dată a celor 120 |
| SC-X-07 | recepție 100 × 10 → BCS 60 → corecție de preț +200 → corecție de preț +100 → storno BCS | inversare 600 + compensare cumulată 180; stoc final 1.300, datorie 1.300, consum net zero |
| SC-X-08 | BTR între gestiuni → BCS din gestiunea primitoare → storno BTR | refuz: lotul mutat are ieșiri |
| SC-X-09 | ASM din două loturi → DSC din produsul rezultat → RLF pe unul din componente | costul produsului nu se re-propagă (granița declarată din design §11) |
| SC-X-10 | închidere de lună cu documente operate → document întârziat în luna închisă → corecție | perioada închisă ca graniță absolută; storno legat + document nou |
| SC-X-11 | `Deschidere` (contabil bloc + stoc pe lot + terți pe partidă) → PLT pe partida de deschidere → FCL nouă | partida fără document se stinge prin aceeași cheie ca una cu document |
| SC-X-12 | închidere de an: 121 → 1174, `Deschidere` a anului nou = `Sold` la 31.12 | F27-r4; snapshot-ul de referință egal cu suma postărilor |
| SC-X-13 | DVI legată la FCT → ajustarea costului pe lot cu taxa vamală → BCS din lot | 86-r1: `Atribuit` pe lot, costul ieșirii după ajustare |

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
| NTC | notă contabilă | pas 3 | `NTC.md` | se scrie cu pasul |
| ITV | închidere TVA | pas 3 | `ITV.md` | se scrie cu pasul |
| RDC | retur de la client | pas 4 | `RDC.md` | se scrie cu pasul |
| RLF | retur la furnizor | pas 4 | `RLF.md` | se scrie cu pasul |
| DVI | declarație vamală | pas 4 | `DVI.md` | se scrie cu pasul |
| ASM | asamblare | pas 5 | `ASM.md` | se scrie cu pasul |
| LDI | listă de inventar | pas 5 | `LDI.md` | se scrie cu pasul |
| NIR | recepție manuală | pas 5 | `NIR.md` | se scrie cu pasul |
| — | `Deschidere` | pas 6 | `DESCHIDERE.md` | se scrie cu pasul |
| PIF/CAS/AMO | imobilizări | TR-D9 | `IMO.md` | la TR-D9 |

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
