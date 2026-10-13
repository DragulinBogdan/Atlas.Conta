# TR-D9b, pasul 4 — originea persistată a partidei

- Tip: analiza
- Data: 2026-10-12
- Contract: [TR-D9b](tr-d9b-contract.md), D9B-D2…D7
- Alimentează: acceptarea pasului 4 și pregătirea termenului B din TR-D9b
- Stare: implementat și verificat; review-ul ulterior este în pasul5, fără commit

## Schimbarea

`OriginePartidaPersistata` este POCO EF în `OriginiPartide`: ID determinist,
cont, titular, document opțional și, exclusiv pentru deschidere, referință
și ordinal inițial. Migrația `20261012114235_OriginiPartide` adaugă tabela,
FK-uri fără cascadă, unicitatea celor două chei naturale și constrângerea
celor două forme de origine. Nu modifică postările și nu face backfill.

`Materializare.Scrie` extrage originile din postările finale eligibile,
deduplică baza și obiectele noi din ObjectSpace și le scrie în tranzacția
comenzii. Nu depinde de lista explicativă `PartidaDeschisa`. Originea
incompatibilă este refuz de materializare cu codul stabil
`ORIGINE_PARTIDA_INCONSISTENTA`; nu este refuz al declarantului.

Retragerea elimină originea proprie fără suport rămas. Postările străine
net zero nu o țin în viață și nu primesc FK spre proiecție. Stornoul și
desfacerea transferului păstrează originea; reoperarea reface aceeași
identitate, iar data rămâne în faptele cubului.

`Origini`, `OriginiDocument` și ordinalele `Deschideri` citesc proiecția.
Citirea de sistem nu descoperă originea prin hash sau `DISTINCT` peste cub.
Ușa securizată adaugă `EXISTS` pe suportul propriu vizibil; agregatele și
snapshot-urile păstrează sursele anterioare. Interogarea rămâne pe server,
inclusiv paginația. `AsEnumerable` din expresia subinterogării alege
semnătura acceptată de vizitatorul XAF; proba verifică SQL cu `EXISTS` și
`LIMIT`, fără hash și fără ramura de nul între surse.

Nu există endpoint OData ori navigație pentru proiecție. Gardul comun
refuză creare, modificare și ștergere inclusiv administratorului securizat.
Exportul SAF-T L include proiecția în lista tipurilor pentru care cere
acces integral; SC-SAFT-36 compară lista cu tabelele citite efectiv.

Proba HTTP a descoperit că `DocumentTrezorerie.CautaPereche` nu traducea
testele de tip pe navigațiile predător/primitor când rolul avea restricții
de rând pe documente. Filtrul folosește acum cheile din interogarea
securizată `ContPropriu`, pentru ambele direcții ale perechii autogenerate.
Rămâne interogare pe server; drepturile și criteriul de virament nu se
schimbă. Proba păstrează aceeași factură ascunsă și cere detaliul plății
vizibile prin HTTP.

## Probe

[Catalogul D9B](scenarii/D9B.md) păstrează sumele fixate înaintea schimbării.
Pe lângă ciclurile documentelor, probele verifică:

- referințele și ordinalele reale ale deschiderii, inclusiv FIFO;
- dry-run fără efecte, cădere înainte de salvare și după `SaveChanges`,
  înaintea commit-ului tranzacției; retry cu o singură origine;
- refuzul unei origini incompatibile deja pregătite în ObjectSpace;
- unicitatea ID-ului și a cheii naturale, două comenzi simultane pe aceeași
  factură și numărul constant de citiri de origine pentru 2/51 partide;
- rol restrictiv: suportul facturii100 ascuns, plata40 vizibilă, origine
  absentă și rest vizibil40; refuzurile CRUD ale adminului securizat;
- invariant pe faptele scenelor și mutanți: origine lipsă, document străin,
  referință inițială diferită și ordinal inițial greșit; dublarea este
  oprită de constrângerile bazei.

Purja infrastructurii șterge și proiecțiile documentelor și ale deschiderii
proprii; nu relaxează FK-urile schemei. `INV-CUB` verifică și absența
originilor reziduale după scenarii.

SC-CIT-110 cere acum prezența `OriginiPartide` și absența hash-ului în
cititorii executați, păstrând interdicția ramurilor de nul. Prima rulare a
semnalat două condiții care cereau vechiul hash și o ramură de nul în
invariantul nou; s-au corectat forma probei și SQL-ul, fără schimbarea
așteptărilor numerice sau admiterea unei excepții `RAMURA-NUL`.

HTTP verifică refuzul rutelor pentru numele proiecției, inclusiv orice
set găsit în EDM. Modelul EDM poate descrie tipuri fără controller;
prezența numelui în XML nu înseamnă endpoint. Rutele probate răspund404 la
GET/PATCH/DELETE și405 la POST. Gardul CRUD este probat separat pe
ObjectSpace securizat, unde adminul ar avea drept de scriere.

## Validare

Integrala finală: **3.756 bugetar / 5.158 privat OK, zero FAIL**, cu
`INV-CUB` pe65/71 scene, toți mutanții uciși, zero postări și zero origini
după purjă, `RAMURA-NUL` verde. Artefact:
`run-verificari/20261012-150800-674/rezultat.json`, build fără avertismente
sau erori. Proba de scriere2/51 produce2/51 origini, credit200/5100 și
**o singură citire de origine în ambele cazuri**, pe ambele profiluri.

HTTP: **327/327 PASS**, plus SC-D9B-10 verde pe baza izolată privată,
`run-verificari/20261012-150627-604/rezultat.json`. Sunt verificate pe HTTP
factura ascunsă404, plata vizibilă200, resturile60/40 și refuzul rutelor
OData ale proiecției; curățenia nu lasă origini. Amprentele binarelor și
conexiunea sunt în `http-mediu.json` din aceeași rulare. Hostul este oprit.

Nucleu: **199/199**, zero eșecuri, `run-verificari/20261012-151357-791/`.
Infrastructura: verde, inclusiv căderea controlată și ieșirea normală fără
intervenție, `run-verificari/20261012-151405-487/`.

Regresia intermediară selectată a trecut în
`run-verificari/20261012-145017-085/`. Prima integrală a semnalat lista
SAF-T incompletă (SC-SAFT-36); integrala finală include corecția ei.
Probele HTTP intermediare au semnalat cititorul perechii de trezorerie,
literalul GUID netipat al fixture-ului și presupunerea greșită că orice
tip prezent în EDM ar fi endpoint. Logurile sunt păstrate; nu se numără
ca rulări verzi. Așteptările numerice și de vizibilitate nu s-au schimbat.

Bazele ModelCheck au sufix `.CodexD9bOrigine`, create din migrații; referința
A `.CodexD9bA` este păstrată. Pachetele sunt cele offline ale pașilor 2–3;
auditul NuGet este dezactivat numai în mediul proceselor, nu certificat.

Comenzile de verificare ale pasului:

```powershell
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip D9B,CITIRI,DESCHIDERE,PLT,NTC -Profil Ambele -Sufix .CodexD9bOrigine
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexD9bOrigine
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita D9bHttp -Profil Privat -Sufix .CodexD9bOrigine
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Nucleu -Profil Ambele -Sufix .CodexD9bOrigine
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Infrastructura -Profil Ambele -Sufix .CodexD9bOrigine
```

Schema a fost generată prin `dotnet ef migrations add OriginiPartide
--context BackOfficeEFCoreDbContext`; baza bugetară proprie prin
`dotnet ef database update --context BackOfficeEFCoreDbContext` cu
conexiunea explicită spre baza sufixată de pe5446. Baza privată este creată
și migrată de ModelCheck. Nu s-au actualizat bazele fără sufix sau referința A.

Documentație: `docskit gen`, `docskit check`, `docskit metrics --check` și
`git diff --check` verzi. Logurile sunt în
`run-verificari/d9b-origini/docskit-check.log` și `docskit-metrics.log`.
Nu sunt erori sau avertismente noi; avertismentele existente rămân
27 trimiteri,218 restanțe,3 cod,4 căi. Linia metricilor nu a fost schimbată.

## Limite

Termenul B la volum, reluarea SQL-ului A pe aceleași date și pragurile D7
aparțin pasului 5. Nu se declară câștig de performanță din proba fără N+1.
Schema nouă nu actualizează istoria bazelor vechi: acestea se recreează
conform 102. Review-ul și comparația sunt în [pasul5](tr-d9b-pas5-comparatie.md);
commit-ul nu este efectuat. Pasul 4 nu
închide pachetul ori întreaga TR-D9b. Registrele bugetare 111(f) rămân în
afara acestei implementări.
