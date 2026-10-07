# Fișa de fapte: repartitorul pe postare

Scrisă de sesiunea care orchestrează consultarea, la commit-ul `717c3b5`.
Fiecare rând a fost citit în cod sau măsurat; ce n-a fost verificat e marcat.
Fișa poate fi părtinitoare: vezi §9. Verific-o, nu o crede.

Prefixe: `M` = `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/`,
`N` = `nou/Atlas.Conta.Nucleu/Atlas.Conta.Nucleu/`.

## 1. Schema de azi

- `M/Cub/Postare.cs:8–66`: rândul persistat. Coordonatele de repartitor sunt
  `Partener` (linia 22) și `Gestiune` (23), amândouă `Guid?`. Lângă ele:
  `Produs`, `Unitate` cu `FelUnitate` și `UnitateDeschisa`, blocul fiscal
  (`TipTvaId` … `InversaTehnica`, liniile 34–48), analiza (53–58), măsurile.
- `N/Cub/Coordonate.cs`: coordonatele nucleului, cu `Partener` și `Gestiune`.
- `M/Cub/Randuri.cs:8–70`: maparea rând ↔ postare, unu la unu.
- `N/Cub/Postari.cs:5–6`: spațiul e Stoc când unitatea e lot, altfel Contabil.
- Predătorul și primitorul documentului nu există pe postare. Postarea ajunge
  la ei prin `DocumentId`.

## 2. Cum completează declaranții

- Gestiunea internă pe piciorul intern, de exemplu
  `M/Declaratii/DeclarantFacturaIntrare.cs:142` și `:164`.
- Gestiunea virtuală pe capătul din afara evidenței:
  `DeclarantFacturaIntrare.cs:150` (Furnizor), `DeclarantNir.cs:50`,
  `DeclarantReturFurnizor.cs:54`, `DeclarantDescarcareGestiune.cs:85`
  (Client), `DeclarantReturClient.cs:65`, `DeclarantDiferenteInventar.cs:84`
  (Inventar sau Consum), `DeclarantNir.Diferenta.cs:86`. Definiția:
  `N/Cub/GestiuniVirtuale.cs`.
- Partenerul vine împreună cu partida, numai pe conturile care urmăresc
  partide: `M/Declaratii/Partide.cs:12–13` și `:51–57` (`CuPartida`).
- Partenerul fiscal pe postarea de bază și pe cea de taxă:
  `M/Declaratii/Fiscal.cs:59–75` (`CuFapt`). Vine de pe latura documentului
  aleasă de politica de TVA. Gestiunea postării de taxă e latura opusă:
  `Fiscal.cs:79–84` și `:119–125`.
- Exclusiv una sau alta, după partea repartitorului:
  `DeclarantDecont.cs:86–87`, `DeclarantNotaContabila.cs:58–59`.
- Trezoreria pune gestiunea pe ambele capete și adaugă partenerul cu partida
  pe capătul de terț: `DeclarantTrezorerie.cs:55–66` și `:118–121`. Funcția
  `Gestiuni(…)`, definită la linia 158, n-a fost citită.
- Contractul laturilor pe tip de document: `M/Declaratii/Laturi.cs` și
  suprascrierile `Laturi()` din `M/BusinessObjects/Documente/`.

## 3. Măsurătoare pe baza de import

Baza `Atlas.Conta.Import1C.Flax.M1s`, profil privat, date reale importate,
81.836 de postări. Baza nu e în clonă; cifrele vin dintr-o interogare numai
în citire, rulată de orchestrator:

```sql
select case when "Partener" is not null and "Gestiune" is not null then 'ambele'
            when "Partener" is not null then 'doar partener'
            when "Gestiune" is not null then 'doar gestiune' else 'niciuna' end, count(*)
from "Postare" group by 1;
```

| Ce poartă postarea | Postări |
|---|---|
| numai gestiune | 33.401 |
| numai partener | 26.002 |
| ambele | 22.389 |
| niciuna | 44 |

Cele 22.389 cu ambele, împărțite după faptul fiscal și după existența
gestiunii în nomenclator:

| Familia | Postări | Conturi, în ordinea volumului |
|---|---|---|
| cu fapt fiscal, gestiune reală | 14.484 | 4427 C, 707 C, 4426 D, 371 D cu lot, 704 C, 767 C, 419 C, 6xx D |
| fără fapt fiscal, gestiune virtuală | 7.905 | 607 D fără unitate (5.185), 401 C cu partidă (2.720) |

Baza conține numai tipurile aduse de import. Retururile, recepția manuală și
diferența ei nu apar, deși codul le dă și lor ambele coloane (§2).

## 4. Cine citește partenerul și gestiunea postării

Numărul de linii cu `.Partener` sau `.PartenerId`, pe fișier, în `M/Cub`,
`M/Proiectii`, `M/Saft`, `M/Motor/SolduriService.cs` și în nucleu:
`Cub/Citiri/Partide.cs` 24, `Proiectii/D394Proiectii.cs` 11,
`Cub/Citiri/Plati.cs` 11, `Proiectii/ImperecheriProiectii.cs` 9,
`Saft/SaftProiectii.PeCub.cs` 6, `Cub/Transferuri.cs` 6,
`Cub/Materializare.Deschidere.cs` 5, `N/Conservare.cs` 4,
`Saft/SaftProiectii.PeCub.Plati.cs` 4, `Proiectii/TvaProiectii.cs` 4,
`Cub/Randuri.cs` 4, `Proiectii/ContabilProiectii.cs` 3,
`Cub/Citiri/Invarianti.cs` 3, `Cub/Citiri/Fiscale.cs` 3,
`Cub/Citiri/Explicatii.cs` 3, restul câte 1–2.

Locuri unde prezența partenerului are sens propriu:

- `M/Proiectii/ContabilProiectii.cs:190`: atomul contabil își ia
  `RepartitorId` din `Postare.Partener` și `GestiuneId` din `Gestiune`;
  balanța analitică și soldul pe parteneri grupează pe ele (`:302–321`).
- `M/Motor/SolduriService.cs:72`: cheia snapshot-ului contabil are
  `RepartitorId` și `GestiuneId`; `:263–268`: snapshot-ul de stoc grupează pe
  lot, cont, produs, gestiune.
- `M/Cub/Citiri/Plati.cs:131`: postarea de plată fără unitate sau fără
  partener devine linie „fără partidă".
- `M/Saft/SaftProiectii.PeCub.Plati.cs:95`: plata cu mai mulți parteneri pe
  contrapartidă e refuzată.
- `M/Saft/SaftProiectii.PeCub.cs:185–188`: agregatul de terți filtrează pe
  conturile cu `RolTert`, apoi grupează pe cont și repartitor.
- `M/Cub/Citiri/Invarianti.cs:117–124`: unicitatea faptului fiscal are
  partenerul în cheie.
- `M/Cub/GardAnaliza.cs:52`: gardul analizei citește
  `capat.Partener ?? capat.Gestiune ?? alDocumentului`.

## 5. Regulile nucleului care depind de cele două coloane

- `N/Conservare.cs:142` și `:170`: cantitatea cere gestiune; lotul cere
  gestiune.
- `N/Conservare.cs:178–183`: postarea pe partidă cere partener, egal cu al
  partidei.
- `N/Conservare.cs:26–41`: contraponderea transformării stă în gestiunea
  virtuală Transformare și nu are voie să poarte partener.
- `M/Cub/Citiri/Transformare.cs:7–11`: filtrul contraponderii, folosit de
  cititori, cere `Partener == null`.

## 6. Convenția registrului vechi, care încă se scrie

- `M/Motor/MotorOperare.cs:193–200`: nota își ia repartitorul fiecărei laturi
  din repartitorul explicit, apoi din linie și din regulă, apoi din implicitul
  documentului.
- `M/BusinessObjects/Documente/Document.cs:259–260`: implicitul e pozițional,
  debitul ia predătorul și creditul ia primitorul. Suprascrieri: `Decont.cs:38`,
  `DescarcareGestiune.cs:24`, `Dvi.cs:29`, `Trezorerie.cs:213–215`.
- Urmare: pe o factură de intrare, nota 628 = 401 poartă furnizorul pe 628 și
  gestiunea primitoare pe 401.

## 7. Starea planului

- Felia în lucru e TR-D9a, tăierea registrelor:
  `docs/nucleu/tr-d9-taierea-contract.md`. Pașii 0–2 sunt făcuți.
- D9-A10, aprobat 2026-10-06 (`docs/nucleu/tr-d9-taierea-amendament-2.md`):
  partenerul și pe piciorul de terț al conturilor cu flag-ul `Repartitor`,
  fără partidă; gardul analizei strict pe postare. Pas 2c, neimplementat.
  Respinsă atunci: partenerul pe toate postările documentului.
- D9-A2 (`docs/nucleu/tr-d9-taierea-amendament-1.md`, secțiunea D9-A2): cheia
  de pereche pe postare. Pas 7b, neimplementat. Transformările și
  deschiderile rămân fără pereche; cititorii rămân neschimbați.
- Pasul 7 regenerează migrația unică `InitialCreate` și recreează bazele
  (contract, D9-D6 și D9-D15). O schimbare de coloane e ieftină acolo și
  scumpă după.
- TR-D9b, unitățile, vine după, cu contract separat.

## 8. Ce declară contul azi

`M/BusinessObjects/Nomenclatoare/PlanConturi.cs`: `DimensiuniObligatorii`
(linia 26, flag-urile din `M/BusinessObjects/Comun/Enums.cs:125–135`, printre
care `Repartitor`), `RolTert` (33; valorile la `Enums.cs:178–182`),
`UrmarestePartide` (36). Pe seed-ul bugetar, 16 din cele 20 de conturi cu
flag-ul `Repartitor` nu urmăresc partide (`docs/nucleu/tr-d9-pas2-probe.md`,
G1; cifra n-a fost re-numărată pentru această fișă).

## 9. Pozițiile care pot înclina fișa

- Owner-ul a propus forma 4 și vede câștig de generalitate în forma 3.
- Sesiunea orchestratoare a recomandat forma 5 acum și materializarea
  capetelor mai târziu, și a respins forma 2.
- Forma 1 e cea implementată; varianta 1b e aprobată și neimplementată.
