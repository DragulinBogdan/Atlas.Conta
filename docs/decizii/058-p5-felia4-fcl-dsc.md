# Decizia 58 — Pasul 5, felia 4 — FCL + descărcarea de gestiune prin API

- **Data**: 2026-08-09 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §58
- **Docs**: docs/api/p5-felia-fcl-contract.md

---

**Pasul 5, felia 4 — FCL + descărcarea de gestiune prin API — executată**
(contract + închidere: `docs/api/p5-felia-fcl-contract.md`, F4-D1…D10;
flux-ancoră complet în browser pe baza Privat de import, review advers cu
2 defecte de fond fixate). Completează fluxul de VÂNZARE în client.
Tranșările care rămân adevărate:
(a) **`FacturaIesireApply` pe șablonul FCT cu contractul inversat pe
`Numar`** (FCL are PoliticaNumerotare — serie fiscală server-owned, nu în
WriteDto, ca BTR/TRZ) și FĂRĂ `LoturiCulegereService` (FCL nu naște
loturi — pinul REFERĂ; `Sterge` fără CurataOrfane). Singura
dimensiune-frunză: `CodEconomicId`. Regulile TVA identice cu F2 (o
singură sursă: implicit doar pe linii noi, recalcul pe declanșatori,
override validat pe regim).
(b) **DSC prin API = citire + comenzi** (șablonul NIR; scrierea manuală =
felie viitoare); `PoateEdita=false` prin contract. **Generarea manuală =
comandă pe ruta FCL** (`POST /api/fcl/{id}/genereaza-descarcare`) pe ușa
NON-secured cu gate `ComandaAutorizata` — obligatoriu: serviciul scrie
`Autogenerat`/`DocumentSursa` (server-owned, gardianul le refuză pe
secured). `GET .../rest-nedescarcat` + affordance onestă
`PoateGeneraDescarcare` (Operat && gestiune && Σrest>0, server-side).
Zero adăugiri OData pentru DSC; `UnitateInterna` expusă ReadOnly
(amendament F4-D5 — lookup-ul emitentului FCL nu avea sursă).
(c) **Review advers D1 — exact riscul pin-uit în contract**: acțiunea XAF
„Generează descărcarea" era RUPTĂ de la spike-1 (OS-ul controllerului =
familia secured ⇒ gardianul refuza `Autogenerat`); migrată pe secvența
DocumentOperareController (CanWrite → `INonSecuredObjectSpaceFactory` din
`Application.ServiceProvider` → același Apply ca endpoint-ul), probată
empiric în Blazor. Regula generală: ORICE cale UI care apelează un
serviciu de motor care scrie câmpuri server-owned trebuie migrată pe ușa
non-secured — secured-ul controllerelor XAF nu mai e o opțiune.
(d) **Review advers D2 — plafonul de acoperire per linie-sursă**, validare
NOUĂ în `DescarcareGestiune.ValideazaOperare`: Σ(operat pe ALTE
documente) + liniile proprii ≤ cantitatea facturată — contra realității
MATERIALIZATE, nu a drafturilor străine (anti-dublarea drafturilor rămâne
a generatorului); dintr-o cursă de generare dublă primul operat câștigă,
al doilea pică zgomotos; închide și DSC-ul manual suprapus
(single-operator). Concurența multi-operator pe commit rămâne parcată
(42/25f).
(e) **Client**: felia `fcl` (editor de linie cu pinul de lot — Lookup
OData filtrat pe produs, `$expand=Produs`, eticheta compusă client-side;
pinul se stinge la schimbarea produsului, comparat PRE-update), secțiunea
„Descărcare" (tabelul acoperirii per linie + generare cu dată +
PanouStingeri rol 'este-stins'), felia `dsc` read-only cu „Generat din"
prin `rutaTip`. Extensie de nucleu ratificată: `Lookup.filtru` (format
DevExtreme → `$filter`, tipurile Guid DEDUSE din forma valorii, memo pe
chei de CONȚINUT — a reparat și bug-ul latent al lui `expand`).
(f) Minore documentate în contract §Închidere (mesajul de rest în client,
M2 FK-restrict 500 pe WebApi, M3 check-uri lipsă) + datoria de PERF
re-confirmată (proiecțiile + PoateGeneraDescarcare pe baza de import —
de măsurat înainte de release). Bonus: ListView-ul RegistruContabil
(restanța DIM-4) se încarcă normal pe 305k rânduri.
