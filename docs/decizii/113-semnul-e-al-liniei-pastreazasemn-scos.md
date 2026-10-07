# 113 — Semnul valorii e al liniei; `RegulaContare.PastreazaSemn` a ieșit

- Data: 2026-10-08
- Stare: **activă**; destinația restanței 110-r1 (se scoate) hotărâtă de owner 2026-10-08. Amendează 046 (a) și precizează 110 (i); închide 110-r1.
- Docs: `docs/stare-curenta/politici-si-fiscalitate.md`, `limite-curente.md`, `dezvoltare-si-validare.md`; catalogul `docs/nucleu/scenarii/CITIRI.md` (SC-CIT-111).

## Regula durabilă

**Valoarea se postează cu semnul liniei. Regula de contare alege conturile și dimensiunile; nu poartă semn și nu-l schimbă.**

(a) **`RegulaContare.PastreazaSemn` nu există.** Au ieșit proprietatea, coloana (`20261007215604_RegulaContareFaraPastreazaSemn`), câmpul din `RegulaContareFapt`, din DTO-ul „Explică" și din formularul React al regulilor de contare.

(b) **`SemnFiltru` e numai filtru de potrivire.** Scoate regula din joc pentru linia cu alt semn (`Potrivire.Contare`). Nu normalizează valoarea postată.

(c) **Direcția e a declarantului.** Capetele mișcării sunt structură, scrisă în declarantul tipului; valoarea e a liniei, cu semnul ei. Retururile culeg pozitiv, semnează linia la operare și postează minus pe corespondența originală: restul lui 046 (a) rămâne neschimbat.

(d) **Politica fără consumator nu rămâne editabilă.** La nivel de regulă e refuzată la editare (110 (b)); la nivel de câmp se scoate din model. Un câmp de politică nu se leagă de un comportament inventat ca să aibă ce comuta (4).

## Context

046 (a) a introdus `PastreazaSemn` ca „unica extensie de motor" a retururilor. Motorul vechi posta nota cu

    valoare = PastreazaSemn ? V : (SemnFiltru ?? +1) × V

adică normaliza valoarea cu semnul filtrului, iar steagul scotea normalizarea. Steagul schimba rezultatul într-un singur caz: `SemnFiltru = −1`. Cele trei feluri de rânduri de seed care îl purtau (`RLF/Stoc`, `RDC/Serviciu`, `RDC/<Tip>` de cost) au toate `SemnFiltru` nul, deci steagul era fără efect pe seed și înainte de tăiere.

La tăiere (110) normalizarea a dispărut odată cu scriitorul vechi: niciun declarant nu înmulțește valoarea cu `SemnFiltru`. LDI alege capetele după plus/minus și postează valoarea pozitivă a evaluării; RDC și RLF postează `Valoare` a liniei, negativă după semnare. `PastreazaSemn` a rămas excepția unei reguli care nu mai există: se edita, apărea în „Explică", nu schimba nicio postare (constatat la TR-D9a pasul 7c, restanța 110-r1).

## Tranșări

1. **Se scoate** (aleasă). Modelul spune ce face codul; „Explică" nu mai arată un câmp care nu decide nimic.
2. **Se leagă.** Respinsă: singurul lucru de care s-ar putea lega e normalizarea cu `SemnFiltru`, care ar trebui reintrodusă în declaranți numai ca steagul să aibă ce opri. Direcția e deja structură în declarant (4, 090); a o face comutabilă din politică înseamnă politică ce inventează comportament.
3. **Rămâne, ca limită declarată.** Respinsă: aceeași categorie pe care D9-A4 o refuză la nivel de regulă.

Schimbarea e de model, nu de comportament: nicio postare nu se schimbă.

Probele SC-CIT-111 (ModelCheck și HTTP) editau tocmai `PastreazaSemn`, fiindcă era fără efect. Editează acum contul creditor explicit al regulii, care e rezervă cât timp sursa `TipMaterial` rezolvă contul.

Lanțul de migrații devine `InitialCreate`, `PostareVizual`, `RegulaContareFaraPastreazaSemn`; precizează 110 (i). Bazele existente se actualizează prin migrație, fără recreare.

## Verificare

- ModelCheck integral pe ambele profiluri, pe clonele `.Claude110r1` (`run-verificari/20261008-005737-235/rezultat.json`): bugetar 3.553 / 0, privat 4.834 / 0. Diferența față de linia de bază a deciziei 112 (`20261008-002932-023`), pe liniile de verificare cu identificatorii și numerele mascate: bugetar −1 (proba „`PastreazaSemn` e inert la bugetar", fără obiect), privat trei titluri de seed RLF/RDC fără numele câmpului; nimic altceva.
- Catalogul RDC, RLF, LDI și lanțurile SC-X rămân verzi cu așteptările neatinse.
- HTTP pe host viu, bază privată nouă din seed (`run-verificari/110r1-http/`): `refuzuri.ps1` 318/318 de două ori, `neexpunere-cub.py` 0 FAIL, `explicatii.py` 9 PASS cu SC-CIT-111 pe ușa OData.
- `--dump-metadata`, `gen:openapi` și `gen:types`: numai ștergeri (`PastreazaSemn` din metadata, din schema `RegulaContare` și din `RegulaContareRandDto`); `tsc -b` fără erori.

Constatare în afara restanței: proba #253 din `refuzuri.ps1` rămăsese pe starea dinaintea deciziei 112 (aștepta FCT × stoc fără regulă câștigătoare) și pica de la 112 încoace (observat pe acest branch, înaintea corecturii). E aliniată la F24-E1: câștigă regula pe natură, „Se postează 302 = 401".

## Ce rămâne deschis

Nicio restanță nouă.

- Bazele hosturilor și bazele de dezvoltare (`Atlas.Conta.BackOffice`, `Atlas.Conta.BackOffice.Privat`) primesc migrația prin `dotnet ef database update` după mersul în main; privatul ModelCheck se migrează singur.
- Textele istorice care numesc câmpul (`docs/import/faza-1c-design.md`, `docs/api/p5-felia24-politici-explica-contract.md`, `docs/nucleu/tr-d9-pas7c-versiunea-politicii.md`, corpul deciziei 046) rămân cum au fost scrise.
