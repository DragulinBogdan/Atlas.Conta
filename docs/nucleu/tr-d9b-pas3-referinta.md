# TR-D9b — catalog și măsurătoare de referință

- Tip: analiza
- Data: 2026-10-12
- Alimentează: [contractul TR-D9b](tr-d9b-contract.md), D9B-D7, pașii 3–5

Catalogul [SC-D9B](scenarii/D9B.md) are probe numerice executabile pentru
comportamentul actual, distincte de acceptarea viitoarei persistențe.
Codul produsului nu este schimbat de acest pas.

## Protocolul A

Intrarea reproductibilă este:

```powershell
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita D9bReferinta -Profil Privat -Sufix .CodexD9bA
```

Harness-ul cere explicit baza dedicată fără postări și fără schema
`scara`. Scena `PerfCub`, 12 luni ×16 unități și luna măsurată cu64,
produce documentele prin motor, cu controale numerice independente.
Multiplicarea folosește `ScaraVolum.sql` și verificările existente de
conservare, pe trepte1/10/100/F, F minim pentru cel puțin5.000.000 postări.
Copiile SQL sunt exclusiv infrastructură de volum; nu țin locul probelor
scriitorului prin documente. `VACUUM (ANALYZE)` precedă fiecare treaptă.

Se citesc `Origini`, `Cumulate(Vizibila)` și `PartideCuRest(Vizibila)`,
fiecare integral și paginat 50, prin sistem și prin ObjectSpace securizat.
Pagina este `Take(50)` cu ordine explicită, fără o cerere `COUNT` separată.
Utilizatorul este administratorul scenei PerfCub: măsoară costul ușii
securizate, nu dovedește filtrarea unui rol restrictiv (SC-D9B-10).
Ordinea pe ID este explicită. O încălzire și cinci rulări, ObjectSpace nou
la fiecare citire. Hash-ul SHA-256 al rezultatului se calculează în afara
intervalului cronometrat; cele șase rezultate trebuie să fie identice.

Se păstrează SQL-ul și parametrii A în fișiere `*-sql.json` în
`nou/tools/ModelCheck/Referinte/D9bA/`, planurile
`EXPLAIN (ANALYZE, BUFFERS)` și, separat, planul diagnostic fără scanare
secvențială. JSON-ul măsurilor păstrează toate duratele, alocările,
comenzile și rândurile. Manifestul fiecărei trepte identifică baza, schema,
binarul, numărul și amprenta faptelor, configurația și mărimea bazei.

După citiri, `ScenaD9bScriere` măsoară o comandă NTC cu 2/51 titulari,
100 lei pe titular: o încălzire și cinci operări, fiecare urmată de
retragere și verificarea restaurării stării Draft fără postări. Drafturile
și identificatorii se păstrează pentru termenul B, pe aceeași bază.
Această probă poate fi reluată distinct, fără multiplicare sau recitirea
scării, prin `-Suita D9bScriere -Profil Privat -Sufix .CodexD9bA`.
Reluarea acceptă numai cele două drafturi proprii, în anul 2077; nu purjează
un an ocupat de alte documente.

## Rezultatele citirilor A

Rulare: `run-verificari/20261012-121930-504/`. **48 combinații**, fiecare
cu o încălzire și cinci măsurări: zero diferențe între cele șase rezultate,
zero diferențe sistem/admin securizat pentru aceeași lectură, toate
planurile cerute obținute. Controalele de conservare ale multiplicării sunt
verzi pe toate cele patru trepte. Rezumatul numeric și cele cinci durate
individuale sunt în `nou/tools/ModelCheck/Referinte/D9bA/referinta.json`.

Mediane, milisecunde, citire integrală prin sistem:

| Factor | Postări | Origini | Solduri | PartideCuRest | Rânduri cu rest |
|---|---:|---:|---:|---:|---:|
| 1 | 7.951 | 24,01 | 3,48 | 18,74 | 777 |
| 10 | 79.510 | 160,37 | 26,85 | 135,86 | 7.770 |
| 100 | 795.100 | 1.084,63 | 313,96 | 1.337,18 | 77.700 |
| 629 | 5.001.179 | 8.004,81 | 1.496,23 | 14.778,18 | 488.733 |

La treapta maximă, citirile integrale de origine și sold livrează fiecare
651.644 rânduri. Detaliul pagină/integral și sistem/securizat:

| Lectură | Sistem integral ms | Sistem pagina50 ms | Securizat integral ms | Securizat pagina50 ms |
|---|---:|---:|---:|---:|
| Origini | 8.004,81 | 7.670,97 | 7.452,55 | 6.790,31 |
| Solduri | 1.496,23 | 1,52 | 1.431,36 | 4,07 |
| PartideCuRest | 14.778,18 | 13.277,73 | 14.190,25 | 13.563,37 |

Fiecare citire prin sistem emite o comandă SQL, fiecare citire securizată
șase (inclusiv infrastructura de securitate). Alocări mediane la scară
maximă, integral: sistem 249,50 / 281,95 / 261,44 MiB pentru
origini/solduri/rest; securizat 181,70 / 194,17 / 257,62 MiB. Sunt măsurări
pe acest proces și această mașină, cu sistemul urmat de securizat; fiecare
serie este reper pentru propria comparație A/B.

Planul implicit al raportului integral de sistem atinge 6.156.325 rânduri
în două scanări ale postărilor și 191.511 buffers. Pagina mică nu elimină
costul deducerii originii. Aceasta susține măsurarea separată a originii și
a agregării, fără a certifica anticipat soluția nouă.

Pragul de 80% raportat la această mediană este **11.822,55 ms**. Nu este
verdict B: în pasul 5, A se rerulează alături de B pe același set de date.
Celelalte praguri din contract rămân neschimbate.

Identificarea referinței:

- PostgreSQL 18.4, nativ 127.0.0.1:5446; `shared_buffers=128MB`,
  `work_mem=4MB`, maximum doi lucrători paraleli per gather, JIT activ.
- Schema: `20261010151835_InitialCreate`.
- SHA-256 ModelCheck pentru citiri:
  `C7C8A42D8C4491FAD76EF96660B0DF0156753E103E4EAD6ACD981AD5DAF1B59A`.
- Amprenta MD5 a faptelor ordonate pe ID, la treapta maximă:
  `74cb9cffa424ad531f5ea7e843fd117a`.
- Baza la măsurare: 2.726.401.727 octeți, inclusiv fotografia `scara`.
  Partițiile postărilor au 1.072.324.608 octeți de tabele și 924.024.832
  octeți de indecși, citite după scriere. Tabela/indecșii originii persistate
  nu există în A; mărimea lor se va raporta la B.

## Scrierea A

Rulare separată verde: `run-verificari/20261012-125032-229/rezultat.json`.
Aceeași bază de 5.001.179 postări, schema neschimbată, aceleași două
drafturi restaurate între runde. O încălzire și cinci măsurări prin
`ComenziDocument.Sistem.Opereaza`:

| Partide/comandă | Valoare | Postări noi | Mediană ms | Comenzi SQL | Alocări mediane MiB |
|---:|---:|---:|---:|---:|---:|
| 2 | 200 | 4 | 24,53 | 32 | 1,53 |
| 51 | 5.100 | 102 | 90,56 | 32 | 18,09 |

Nu există încă interogări de întreținere a proiecției originii; 32 este
numărul total al comenzii actuale, nu un număr atribuit scriitorului nou.
Pragurile corespunzătoare pentru B sunt 34,53 / 104,14 ms, cu aceeași
rezervă a rerulării pe aceleași date. Drafturile au ID-urile păstrate în
`referinta.json`; pot fi refolosite prin `D9bScriere`.

SHA-256 ModelCheck al probei separate:
`DF9B33734688AA8692CC8EB7E6B4891602701948A34D258166BAD7B2A57A693B`.
Binarul diferă de cel al citirilor prin completări/corecturi în harness;
produsul și schema nu s-au schimbat între ele.

Recitirea completă după probele de scriere confirmă **5.001.179 postări și
aceeași amprentă** `74cb9cffa424ad531f5ea7e843fd117a`: faptele A nu au fost
schimbate de probele comenzilor. Rezultatul este păstrat în `DupaScriere`
din referința JSON.

## Verificarea montajului de volum

Rularea citirilor s-a terminat cu **exit 1** după măsurători: prima formă a
probei de scriere moștenea verificarea globală `INV-CUB` a scenelor normale.
Aceasta a refuzat corect `CITIRE_TAXA_DIFERITA_DE_LINIE`: copiile sintetice
de volum păstrează identificatorii liniilor originale și nu multiplică
liniile fiscale. Nu este o bază de documente complet operabilă și nu este
certificată ca atare. Nu s-au modificat invariantul sau produsul.

Proba de scriere a fost separată: operează documente reale, verifică exact
2/51 origini, 200/5.100 lei, 4/102 postări și o tranzacție, apoi retrage și
verifică absența postărilor și tranzacțiilor proprii. Eșecul restaurează
comanda în `finally`. Verificarea globală rămâne obligatorie în scenariile
normale D9B, pe bazele fără multiplicare SQL. Urma citirilor rămâne raportată
distinct de exit-ul întregului montaj inițial.

Prima compilare a fixture-ului separat a cerut constructorul explicit al
recordului; următoarea rulare a refuzat extragerea anului dintr-o dată
Postgres infinită existentă în scena de volum. Filtrul este acum intervalul
inclusiv de date, ca în fixture-ul comun. Rularea verde de mai sus este
ulterioară ambelor corecturi.

## Reproducere și limite

Verificări finale executate prin scriptul comun:

```powershell
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip D9B,CITIRI,DESCHIDERE,PLT,NTC -Profil Ambele -Sufix .CodexD9bRename
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita D9bScriere -Profil Privat -Sufix .CodexD9bA
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Infrastructura
```

Regresie verde: **800 bugetar / 927 privat OK**, zero FAIL,
`run-verificari/20261012-125206-497/`. Include SC-D9B-03b, `INV-CUB` pe
13 scene/profil, zero reziduu și `RAMURA-NUL` verde. Infrastructura este
verde, inclusiv închiderea nesupravegheată a procesului căzut:
`run-verificari/20261012-125400-664/`. Nucleul și integrala nu au fost
rerulate în acest pas fără schimbare de produs; rezultatele pasului 2
rămân identificate separat.

`docskit gen`, `docskit check`, `docskit metrics --check` și
`git diff --check` sunt comenzile documentare ale predării. Review-ul
advers și commit-ul nu sunt efectuate.

Build-ul folosește pachetele baseline locale DevExpress26.1.4 și
Atlas.DXF26.1.4.10, prin configurare exclusiv de proces, ca în
[pasul 2](tr-d9b-pas2-redenumiri.md). Auditul NuGet nu a rulat offline.
Nu se modifică versiunile proiectului pentru mediul instrumentului.

Prima lansare a fost întreruptă de timeout-ul instrumentului înaintea
măsurărilor. Baza de411 postări este păstrată sub
`Atlas.Conta.ModelCheck.Privat.CodexD9bAIntrerupt`; nu este termen A.
Reluarea folosește un proces Windows detașat și baza curată `.CodexD9bA`.

La pasul 4, datele noii forme se produc prin scriitorul nou. Comparația B
cere rerularea SQL-ului A păstrat **pe aceeași bază și aceleași date ca B**;
timpul din această referință nu autorizează comparația între baze diferite.
Multiplicarea trebuie extinsă pentru proiecția nouă cu controale de origine;
un backfill SQL al originii nu certifică scriitorul. Restanțele și pragurile
din contract rămân deschise, inclusiv HTTP, mutanții și review-ul advers.
