# TR-D9a — amendamentul 3 la contract

- Data: 2026-10-07
- Bază: `tr-d9-taierea` la `d456729`; contractul
  [`tr-d9-taierea-contract.md`](tr-d9-taierea-contract.md), D9-D9; analiza
  beneficiu / efort cerută de owner pe identificatorii bruți din lista XAF
  `Postare` (`comunicari/2026-10-07-0913-claude-owner-identificatori-lista-postare.md`;
  `limite-curente.md`, rândul D9-D9 din 2026-10-06).
- Stare: aprobat de owner (2026-10-07, în chat): „am luat decizia de a merge
  cu view". Transcris în contract, secțiunea „Amendamentul 3".
- Respinse: snapshot-ul de valoare la scriere (coduri înghețate pe postare) și
  navigațiile EF pe POCO. Motivele stau în analiză; pe scurt: snapshot-ul
  schimbă schema cubului și calea de scriere pentru o nevoie de afișare, ar
  fi singurul cititor cu codul vechi după o redenumire și ar produce în SAF-T
  coduri fără corespondent în master files; navigațiile sunt parțiale (partida
  și gestiunea virtuală n-au rând; `SuportId`/`InversaDinId` au cheie compusă)
  și sunt trimise, cu FK-urile, în TR-D9b.

## D9-A12 — lista XAF de evidență a cubului e o proiecție de citire (view SQL)

**Ce e azi.** Lista XAF `Postare` (pasul 4, D9-D9) arată identificatori bruți
pentru tranzacție, document, cont, partener, gestiune și unitate, fiindcă
`Postare` e POCO fără navigații (S-D1) și maparea cubului nu se schimbă în
TR-D9a.

**Regula.**

- (a) Lista XAF de evidență a cubului se mută pe `Cub.PostareVizual`:
  entitate EF cu cheia `ID`, mapată `ToView("PostareVizual")`, read-only
  (`ForbidCRUD`), fără `BaseObject`, în `Module/Cub/`. View-ul SQL stă în
  migrație (`CREATE VIEW` în `Up`, `DROP VIEW` în `Down`), peste `Postare`
  cu `LEFT JOIN` pe nomenclatoare: contul (`Simbol`), partenerul și gestiunea
  (`Repartitori.Cod`), produsul (`Cod`), unitatea pe Stoc (lotul), unitatea pe
  fișă (numărul de inventar), documentul (serie și număr), tipul de TVA,
  valuta, dimensiunile bugetare. Coloanele postării rămân toate în view, cu
  numele lor; codurile vin alături, cu sufixul numelui nomenclatorului
  (`ContSimbol`, `PartenerCod`, `GestiuneCod`, `ProdusCod`, `UnitateCod`,
  `DocumentNumar`, …). `ToView` e exclus din snapshot-ul EF: fără drift la
  `has-pending-model-changes`.
- (b) Coordonatele fără rând sunt expresii SQL, nu îmbinări: partida
  (`FelUnitate = Partida`) se etichetează din codul partenerului, numărul
  documentului de deschidere și `UnitateDeschisa`, în forma în care o
  reconstruiesc cititorii (`IdentitatiPartide`); gestiunea virtuală
  (`GestiuniVirtuale.*`) se etichetează cu numele constantei. Lista nu
  interpretează etichetele.
- (c) `Postare` rămâne POCO, fără `NavigationItem`, cu calea de scriere
  neatinsă (42c, S-D1). Subiectul permisiunii pe cele trei porți și pe
  verificarea închiderii rămâne dreptul de citire completă pe `Postare`
  (D9-D9). Lista pe `PostareVizual` NU e un subiect nou de drept: un rol fără
  citire completă pe `Postare` (criteriu de rând sau de membru) primește lista
  goală cu textul refuzului, prin aceeași funcție de acces ca porțile
  (`Api.AccesComplet`), la activarea listei. Criteriile de rând pe `Postare`
  nu se traduc pe view; de aceea gardul e la activare, nu pe criteriu.
- (d) `PostareVizual` nu se expune prin OData și nu intră în metadata
  clientului React; intră în proba de neexpunere alături de `Postare` și
  `Tranzactie`. Gardianul o refuză la scriere pe orice ușă securizată, ca pe
  `Postare` (cazurile gardianului se extind cu ea).
- (e) Modul listei: `Server` (85); paginarea, sortarea și filtrarea pe
  coloanele de cod se execută în Postgres. Lista nu are totaluri și nu
  ocolește regula `Transfer` (108 b): felul tranzacției rămâne coloană.
- (f) Probe: paritatea coloanelor (fiecare proprietate mapată a lui
  `Postare` e coloană a lui `PostareVizual`; o coloană nouă pe postare uitată
  în view pică proba); egalitatea numărului de rânduri view ↔ `Postare` pe
  ambele profiluri; cele două etichete din (b) pe câte o postare de catalog;
  refuzul din (c) pe un ObjectSpace securizat cu rolul cu criteriu de rând;
  neexpunerea din (d) pe host viu; costul paginii întâi și al sortării pe
  `ContSimbol` măsurat o dată pe `.D9Vol`, A/B pe aceeași bază, raportat în
  documentul pasului, fără prag.
- (g) La TR-D9b, când coordonatele postării se schimbă (decizia 111), view-ul
  se rescrie ca text SQL în migrația acelei felii; nu se migrează date.
- (h) Fidelitatea istorică la redenumirile de nomenclator nu e cerință acum.
  Dacă devine, locul ei e un istoric al nomenclatorului (citire „la data"),
  nu snapshot pe cub; restanță cu nume la deschiderea cerinței.

**Unde se aplică.** D9-D9 (lista); pasul 7 (view-ul, entitatea, lista,
gardul, probele, în același pas cu `InitialCreate` regenerat); regula de
oprire 5 (cazurile gardianului includ `PostareVizual`) și „oprire înaintea
termenului" (lista pe `PostareVizual` care nu funcționează pe hostul real);
`limite-curente.md` își închide rândul D9-D9 despre identificatorii bruți la
pasul 7.

## Review-ul advers

Owner, 2026-10-07: Codex iese din buclă. Review-urile adverse ale pașilor
rămași (7, 7b, 7c, 8) și al închiderii se fac de main, pe diff, cu probele
contractului; forma unui review extern, dacă owner-ul o cere, se hotărăște
atunci (`/consult-external`). Regula de oprire 8 se citește „review-ul advers
e închis", fără numele instrumentului.
