# Catalogul de scenarii — proba supremă a motorului (decizia 091)

**Actualizat: 2026-09-24.** Regula: `docs/decizii/091-scenarii-in-loc-de-import.md`,
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
| SC-X-09 | ASM din două loturi → DSC din produsul rezultat → RLF pe unul din componente | circuit numeric și inversări în [ASM.md](ASM.md); reevaluarea cu nepropagare rămâne TR-D9 (design §11) |
| SC-X-10 | închidere de lună cu documente operate → document întârziat în luna închisă → corecție | perioada închisă ca graniță absolută; storno legat + document nou |
| SC-X-11 | `Deschidere` (contabil bloc + stoc pe lot + terți pe partidă) → PLT pe partida de deschidere → FCL nouă | partida fără document se stinge prin aceeași cheie ca una cu document |
| SC-X-12 | închidere de an: 121 → 1174, `Deschidere` a anului nou = `Sold` la 31.12 | F27-r4; snapshot-ul de referință egal cu suma postărilor |
| SC-X-13 | DVI legată la FCT → ajustarea costului pe lot cu taxa vamală → BCS din lot | 86-r1: `Atribuit` pe lot, costul ieșirii după ajustare |
| SC-X-14 | Faptele fiscale din fiecare scenă, înainte de curățenie: operare → storno/corecție | unicitate Bază/Taxă independent de Carte, absență explicită pe liniile fără TVA; FCT/FCL 100/21, RDC −100/−21, RLF −20/−4,20, DVI 100/21; matricea completă în [DVI-B7](../tr-d7b-dvi-baza-fiscala-contract.md#dvi-b7--probe-obligatorii-înainte-de-activare); implementat în `UnicitateFiscala` și scenele catalogului |

Concurența pe două conexiuni (X-D6, `ScenariiConcurenta`, `--scenarii X`):
scena ține blocajul scrierii, cele două comenzi intră pe rând în coada lui
și rulează în ordinea cozii. Verificate pe ambele profiluri, 2026-10-03.

| Id | Comenzile, în ordinea serializată | Rezultatul serial |
|---|---|---|
| SC-X-15 | lot 10/100; două BCS de câte 6 | primul trece, al doilea `STOC_INSUFICIENT`, fără efecte; lot 4/40 (SC-BCS-13) |
| SC-X-16 | lot 3/1,00; două BCS de câte 1 | ambele trec, 0,33 apoi 0,34; soldurile citite din explicații sunt 3/1,00 și 2/0,67; lot 1/0,33 |
| SC-X-17 | FCT 10 × 10 operată; BCS 4 și stornoul FCT, în ambele ordini | consum → storno: stornoul e refuzat, lot 6/60; storno → consum: consumul e refuzat, lot 0/0 |
| SC-X-18 | FCT 100; două plăți de 60 împerecheate cu ea | prima trece, a doua e refuzată peste rest; rest factură 40, a doua plată nealocată |
| SC-X-19 | FCT 100 și PLT 100, aceeași pereche: 40 + 40, apoi 15 + 15 | 40 + 40 trec, rest 20; din 15 + 15 trece prima, rest 5 |
| SC-X-20 | BCS draft cu o linie; două sesiuni adaugă câte un detaliu | ambele trec; pozițiile sunt 1, 2, 3 (S-r9) |
| SC-X-21 | lot 10/100; BCS 4 și închiderea lunii lui, în ambele ordini | operare → închidere: ambele trec, snapshot 6/60; închidere → operare: operarea e refuzată, fără efecte, iar snapshot-ul lunii rămâne 6/60 (SC-BCS-14, F27-r8) |
| SC-X-22 | cele 16 feluri de comandă, pe captura SQL | fiecare începe cu blocajul scrierii; dry-run-ul, citirile și salvarea fără detalii noi nu îl iau |
| SC-X-23 | comandă care așteaptă peste timpul ei | refuz `SCRIERE_OCUPATA`, fără efecte; după eliberare trece |
| SC-X-24 | fiecare Anulează, Stornează și Corectează din scenele catalogului, cu regimul citit înaintea comenzii | regimul care refuză ⇒ comanda refuză; comanda care refuză ⇒ regimul refuza, în afara refuzurilor dovedite pe valori, pe data cerută ori pe limita 106-r7 declarată de scenă (106k, [REGIM](REGIM.md)) |

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
| RDC | retur de la client | pas 4 (privat) | [RDC.md](RDC.md) | venit/cost, partidă proprie negativă, compensare NTC, ciclu mixt peste perioadă; inert bugetar; cititorii comuni TR-D8 |
| RLF | retur la furnizor | pas 4 (privat) | [RLF.md](RLF.md) | valoare fiscală, reziduu pe lot gol, compensare NTC, ciclu peste perioadă; inert bugetar |
| DVI | declarație vamală | pas 4 (privat) | [DVI.md](DVI.md) | bază distinctă în Carte=Fiscal, 20 de scenarii, cicluri complete și SC-X-14; inert bugetar |
| ASM | asamblare | pas 5 | [ASM.md](ASM.md) | transformare n→m, Δ dual pe grup, storno mixt și corecție pe ambele profiluri; SC-X-09 privat; TR-D8/TR-D9 rămân explicite |
| LDI | listă de inventar | pas 5 | [LDI.md](LDI.md) | Magazie/Marfuri pe ambele profiluri; Folosinta în gestiune reală pe lanțul FCT/NIR/BTR/BCS/LDI, inversări, corecție și istoric (093); Custodie explicit neacoperită |
| NIR | recepție manuală și conex | pas 5 | [NIR.md](NIR.md) | verificat pe ambele profiluri: ciclul complet, stingeri, dependenți, Folosinta/Marfuri; conexul sursei migrate nu dublează cubul |
| — | `Deschidere` | pas 6 | [DESCHIDERE.md](DESCHIDERE.md) | detaliere fără dublare (094), refuz atomic, unicitate, stingere/anulare/storno sub blocare comună; analiza/valuta păstrate la intrare, stingerea în valută neacoperită; cititorii de producție rămân TR-D8 |
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

## SC-X-14 — unicitatea fiscală (2026-09-23)

`UnicitateFiscala` consumă așteptările numerice ale scenelor, fără citirea
registrelor pentru construirea matricei. Cheia este tranzacție × linie ×
tip TVA × sens, fără Carte; exact o Bază și cel mult o Taxă, cu măsurile
semnate așteptate. Liniile fără fapt fiscal au explicit 0/0. `Postari`
verifică separat tranzacția unică și toate coordonatele; matricile rămase
sunt reverificate înainte de curățenie. BCS are aceeași probă de absență
pe toate liniile sale operate/stornate. Rularea filtrată păstrează proba.

Acoperirea apare în log pe tip/regim/profil. Sunt incluse FCT normal,
capitalizat (NED21: 100/21 pe cost) și taxare inversă, FCL normal,
taxare inversă/scutit (100/0), RDC venit și cost, RLF și DVI; plus liniile
nefiscale BCS, BTR, DSC, PLT/INC, NTC și ITV. Cazurile NED21/TI21 ale FCT
și TI21/SDD ale FCL au și inversare -100/-21, respectiv -100/0. Pe bugetar
absența faptelor fiscale este explicită; tipurile inerte se probează prin
refuz și zero efecte. Nu include tipurile încă nemigrate, toate combinațiile
viitoare sau FCL capitalizat (T-r8). Martorii în memorie detectează baza
copiată în cealaltă carte, taxa duplicată și baza așteptată eliminată.

### Felia fiscală TR-D8 (103)

`verifica.ps1 -Suita Scenarii -Tip FISCALE -Profil Ambele -Sufix .FiscalCub`
rulează `ScenariiFiscale`, inclus și în `CITIRI`/integrală. HTTP securizat:
`python -X utf8 nou/tools/ProbeHttp/fiscal-cub.py --baza <bază privată izolată>`.
Proba HTTP cere hostul local la 5089 (sau `--host`), seed privat, conturile
Admin/Cititor/User cu parolă goală și anul 2021 fără perioade. Creează date,
roluri și confirmări temporare; curăță fixture-ul în `finally`.
Nu se rulează pe o bază de lucru a utilizatorului.

### SAF-T pe cub — S1 implementat, S2/S3 în delimitare

[SAFT.md](SAFT.md) conține matricea S1 GL/facturi și delimitarea S2 Payments,
S3 stocuri. [Contractul SAF-T](../tr-d8-saft-contract.md) integrează
SAF-D1/D2/D4=A, SAF-D3=C și S1 cu S1-D5 = B' (381 + 384), toate aprobate de
owner. S1 rulează prin `--scenarii SAFT` (anul 2024), plus verificările
SC-SAFT-10/11 din scenele ASM, IMO și DEC. Ruta publică L nu este comutată
(R1), iar validarea XSD/DUK a noului fișier așteaptă S0. Mapările și
derivarea cauzelor S3 au blocaje explicite.
