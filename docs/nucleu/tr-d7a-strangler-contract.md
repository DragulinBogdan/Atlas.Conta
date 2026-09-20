# TR-D7a — Strangler-ul per tip, felia 31: cubul persistat și cele trei tipuri ale pilotului

- **Data**: 2026-09-20
- **Stare**: ÎN LUCRU (branch `tr-d7-strangler`, tăiat din `main` = 1016890, felia 30 închisă)
- **Docs**: decizia 090 §Regula durabilă (l), (m); `docs/nucleu/nucleu-transfer.md` (TR-D1…D4, TR-D7, §4.1); `docs/nucleu/tr-d6b-declaratia-fluxului-contract.md` (B-D8 lista închisă, B-D9 amânările cu numele TR-D7); `run-nucleu/fizica/pas1/01-schema.sql` + `pas2/fk-f2.sql` + `pas2/indexi-f2.md` (forma fizică măsurată); `docs/decizii/restante.md` (B-r1…B-r11, TR-r2, TR-r10, TR-r12, N-r7, N-r8); probele feliei în `run-nucleu/tr-d7a/`.
- **Felii TR-D7**: TR-D7 e 6–8 sesiuni (§4.1). Felia 31 = TR-D7a = fundația persistată (entitățile, migrația, materializarea, stornoul, anularea, gate-ul) + BCS, PLT/INC, FCT migrate pe cub, cu restanțele lor. Tipurile următoare (după volumul pe Flax, cele cu conex la urmă) sunt TR-D7b…, fiecare cu contract propriu care refolosește fundația de aici.

## Scop

Motorul nou scrie `Tranzactie`/`Postare` în ACEEAȘI tranzacție de comandă în
care motorul vechi scrie registrele, DOAR pentru tipurile cu `PosteazaInCub`
(dată pe `TipDocument`). Citirile rămân pe registre (TR-D8 le mută pe cub).
Cubul persistat e reconciliat cu registrele pe tipurile migrate prin trei
oracole refolosite: scenele ModelCheck (postările persistate = contractul =
registrele normalizate B-D8), maparea fizicii ca cod de test și raportul
Import1C pe Flax identic cu baseline-ul feliei 28.

## Testul contra invarianților

| Invariant | Cum îl respectă felia |
|---|---|
| I (registre append-only) | cubul e append-only: operarea = o tranzacție `Operare`, stornoul = a doua tranzacție `Storno` (inversul exact), anularea operării (corecție directă fără dependenți, 14/33d) șterge tranzacția documentului simetric cu registrele — declarat (S-D6) |
| II (motorul nu cunoaște frunzele) | regimul dual e dată pe `TipDocument`; niciun `is`/`switch` pe frunză; `Declarant()` rămâne singurul membru polimorf |
| III (structura = cod, politica = date) | `PosteazaInCub`, `LaturaContPropriu`, `TolerantaTaxa` sunt coloane de politică seed-uite `DinSeed`; nu inventează comportament |
| IV (o singură sursă de reguli) | refuzul declarației e refuzul operației, pe același OS non-secured, în aceeași tranzacție; `Valideaza` (dry-run) îl arată la fel |
| V (diferențele se raportează) | orice reziduu cub ≠ registre e defect sau consecință DECLARATĂ (S-D9); nicio normalizare tăcută; Import1C identic pe conținut sortat |
| VI (fără interogare per linie) | `Fapte.Operand` ≤ 16 interogări (probă existentă); materializarea scrie pe set prin OS; cititorul rândurilor nu face lookup per rând |

## Deciziile (S-D1…S-D12)

### S-D1 — Entitățile: `Postare` și `Tranzactie`, POCO EF cu cheia `ID`

Folder `Module/Cub/` (nu `Nucleu/`), namespace `Atlas.Conta.BackOffice.Module.Cub`.

- `Tranzactie`: `ID` (Guid, cheie), `DocumentId` (Guid?, FK → `Documente`), `Fel` (`N.FelTranzactie`, smallint), `Data` (DateOnly), `ScrisLa` (DateTime UTC, timestamptz), navigație `Postari`.
- `Postare`: `ID` (Guid, cheie), `Spatiu` (`N.Spatiu`, smallint, calculat la materializare din `postare.Spatiu()`), `TranzactieId` (FK), `DocumentId` (Guid?, FK → `Documente`; nullable pentru `Deschidere`, TR-r10), `LinieId` (Guid?, fără FK — ca `Lot.LinieIntrareId`), `Data`, `Cont` (Guid, NOT NULL, FK → `Conturi`), `Latura` (`N.Latura`, smallint), `Partener`, `Gestiune`, `Produs` (Guid?), `Unitate` (Guid?), `UnitateDeschisa` (DateOnly?, **amendament la FZ-D2**: data deschiderii unității e decizia declarantului la postare — cheia FIFO `(Deschisa, Id)` — și partida n-are rând de nomenclator; fără ea cititorul rândurilor ar face lookup), `TipTvaId`, `SensTva`, `RolTva` (cele trei componente ale `N.CodTva`; nu există tabelă `CodTva` — N-D2), `PerioadaDeclarare` (int?), `Valuta` (Guid?, fără FK până la B-r6), `Carte` (`N.Carte`, smallint), `CodFunctional`, `CodEconomic`, `SursaFinantare`, `UnitateOrganizatorica`, `Proiect`, `CentruCost` (Guid?), `Atribuit` (Guid?), `Cantitate` (18,3), `ValoareValuta` (18,2), `Valoare` (18,2).
- NU derivă din `BaseObject`: cubul e append-only, fără `GCRecord`, fără `OptimisticLockField`, fără filtru global de interogare. Dacă convențiile globale ale contextului le aplică oricum, se exclud explicit pe cele două entități; dacă excluderea nu e posibilă, regula de oprire.
- Fără `[DefaultClassOptions]`/`NavigationItem`: nu apar în UI (090m); grilele pe `Postare` sunt ale lui TR-D9.
- `Scara.ScaraPentru` primește `ValoareValuta` (bani) și `TolerantaTaxa` (bani) — gardianul scării rămâne singura sursă a preciziei.
- Enumurile nucleului (`N.Latura`, `N.FelTranzactie`, `N.Spatiu`, `N.Carte`, `N.SensTva`, `N.RolTva`) se mapează direct pe smallint cu valorile lor numerice; nu se dublează în Module.
- `FelUnitate` NU se persistă: la TR-D7 `Spatiu = Stoc ⇔ Lot`, `Contabil ⇒ Partida`; fișa (TR-D9) aduce ce-i trebuie.

### S-D2 — Migrația: o singură entitate, partiționarea și FK-urile per partiție în SQL explicit

XAF EF Core NU suportă chei compuse (docs DevExpress „Key Properties",
26.1: „entities with composite/compound keys … are not supported"), iar
Postgres cere cheia partiției în orice constrângere unică a unei tabele
partiționate. Rezolvarea:

- modelul EF declară cheia `ID`; migrația `2026092…_CubDePostari` înlocuiește `CreateTable("Postare")` generat cu `migrationBuilder.Sql(...)`: `CREATE TABLE "Postare" (...) PARTITION BY LIST ("Spatiu")`, `PRIMARY KEY ("Spatiu","ID")`, partițiile `"Postare_Contabil" FOR VALUES IN (1)` și `"Postare_Stoc" FOR VALUES IN (2)` (DOUĂ partiții — `Spatiu` e cel din N-D2/TR-D4, nu cel cu Fiscal din măsurătoare; fiscalul e atribut pe postare); `Down` are `DROP TABLE` explicit. Snapshot-ul EF rămâne cu cheia `ID` — divergență DECLARATĂ față de baza reală, inofensivă: cubul nu se actualizează niciodată pe cheie, id-urile sunt Guid-uri, iar `has-pending-model-changes` nu vede baza.
- FK-uri per partiție, în SQL: Contabil → `Conturi`(Cont), `Repartitori`(Partener, Gestiune — NOT VALID nu e necesar pe bază nouă), `Produse`(Produs), `Tranzactie`, `Documente`(DocumentId); Stoc → `Conturi`, `Repartitori`(Gestiune), `Produse`, `Loturi`(Unitate), `Tranzactie`, `Documente`. **Fără FK pe `Unitate` pe Contabil**: id-ul partidei e hash determinist (`Unitate.DeschidePartida`), nu id de document — abatere DECLARATĂ față de `fk-f2.sql` (partida ca rând e a lui TR-D9). Gestiunile virtuale (`GestiuniVirtuale.*`) sunt id-uri fără rând (B-r8) — FK-ul pe `Gestiune` NU se declară pe nicio partiție cât timp nu au rând `DinSeed`; se consemnează.
- Indexii de la TR-D7 (setul minim; TR-r3 re-măsoară la TR-D8): PK; `(TranzactieId)`, `(DocumentId)` pe părinte; Stoc `(Produs, Data) INCLUDE (Cantitate, Valoare, Gestiune, Unitate)` (S2); Contabil `(Partener, Cont, Data) WHERE Partener IS NOT NULL` (C2), `(Data)` (C3), `(PerioadaDeclarare, TipTvaId) WHERE PerioadaDeclarare IS NOT NULL` (F1 adaptat). `Tranzactie(DocumentId)`.
- Coloanele noi de politică în aceeași migrație: `TipDocument.PosteazaInCub` (bool, default false), `TipDocument.LaturaContPropriu` (enum de Module `LaturaDocument { Predator = 1, Primitor = 2 }`, smallint null — B-r2: care repartitor al documentului e contul propriu, nu latura D/C), `PoliticaTva.TolerantaTaxa` (18,2 — B-r1), `DocumentDetaliu.Pozitie` (int, default 0 — B-r11).
- Probele ModelCheck `STR-SCHEMA-*` pe `pg_partitioned_table`/`pg_inherits`/`pg_constraint`/`pg_indexes`: părintele e partiționat LIST pe `Spatiu`, două partiții cu valorile 1/2, PK `(Spatiu, ID)`, fiecare FK de mai sus există pe partiția ei, indexii există. Rulează pe ambele profiluri.
- Comanda: `dotnet ef migrations add CubDePostari --context BackOfficeEFCoreDbContext` (fără `--no-build`); bugetarul cere `dotnet ef database update` înainte de ModelCheck; `has-pending-model-changes` curat.

### S-D3 — `PosteazaInCub` e dată pe ancoră; `null` la declarant devine eroare de configurare (B-r9)

`TipDocument.PosteazaInCub` se citește o dată per operare din `TipDoc` al
planului (deja încărcat prin `GasesteTipDocument`), fără cache nou. Când e
`true` și `doc.Declarant() == null`, operarea REFUZĂ cu
`OperareException("Tipul X e marcat PosteazaInCub, dar clasa nu declară")` —
eroare de configurare, nu regim tăcut. Când e `false`, nimic din felie nu
rulează (documentul nu atinge cubul). Seed: `SeedTipuriDocument` aliniază
`PosteazaInCub` ca proprietate `DinSeed` pe ambele profiluri; la închiderea
feliei BCS, PLT, INC, FCT sunt `true`, restul `false`. Pe parcursul pașilor
seed-ul rămâne `false` și scenele îl comută local (S-D8).

### S-D4 — Materializarea: în `MotorOperare.Opereaza`, în aceeași tranzacție, după registre, înainte de commit

`Cub/Materializare.cs` (static, în Module):

- `Opereaza(IObjectSpace os, Document doc, TipDocument tip)`: dacă `!tip.PosteazaInCub` iese; altfel `Contractare.Contracteaza(os, doc)`; contract refuzat ⇒ `OperareException` cu toate refuzurile (`COD: mesaj [linie]`), nimic scris, tranzacția comenzii se anulează (refuzul declarației = refuzul operației — o singură sursă de reguli); contract acceptat ⇒ o `Tranzactie(Fel = Operare, Data = DataInregistrare, ScrisLa = UtcNow)` + o `Postare` per postare a contractului, prin `os.CreateObject<T>()` ca registrele.
- Locul: `MotorOperare.Opereaza`, după `ImperechereService.CreeazaAutomataLaOperare` (`:425`) și înainte de `os.CommitChanges()` (`:427`) — deci și pe calea Import1C (`Bucla.cs:910`, care ocolește `OperareApi`), și pe `Valideaza`. Numărul e deja asignat (`NUMAR_LIPSA` al FCT), loturile născute au preț.
- `Valideaza` (dry-run, `OperareApi:92-95`): pe tip migrat adaugă refuzurile declarației la erorile motorului vechi — consecință declarată pe 422 (mesaje în plus pe gardurile mai stricte, forma `EroriDto` neschimbată).
- Operandul citește starea DINAINTEA documentului din baza de date (rândurile create în OS nu sunt încă scrise; `SolduriLaData(..., doc.ID)` exclude oricum documentul). Determinismul probat la pilot rămâne: `Contracteaza` după commit dă același contract.
- Decizii/ipoteze/`JumatatiDeBan` NU se persistă la TR-D7 (nu au cititor); consemnat ca S-r.

### S-D5 — Cititorul rândurilor și stornoul ca a doua tranzacție

- `Cub/Randuri.cs`: `Postare` (rând EF) → `N.Postare` exact (coordonatele, `Unitate = new(Unitate, Spatiu == Stoc ? Lot : Partida, Cont, Partener, Produs, UnitateDeschisa)`, `CodTva` din cele trei coloane, `Cauza(DocumentId, LinieId)`, `Atribuit`), și invers. Proba `STR-ROUNDTRIP`: rândurile scrise ale unui document, citite înapoi, sunt EGALE structural cu `contract.Tranzactie.Postari` (nu doar `Comparabil`).
- `Materializare.Storneaza(os, doc, dataStorno, perioadaDeclarare)`, chemată din `MotorOperare.Storneaza` înainte de `os.CommitChanges()` (`:705`), gardată de `PosteazaInCub`: citește pe set rândurile documentului + cele `Atribuit` spre ele (un salt, `Storno.Selecteaza`), `Storno.Inverseaza` → `Tranzactie(Fel = Storno, Data = dataStorno)`. **Amendament N-D10, cu test de proprietate în nucleu**: `Inverseaza` primește `perioadaDeclarare` și o re-ștampilează pe postările care poartă una — reperul fiscal al stornoului e perioada stornării (088; motorul vechi scrie `PerioadaAn/Luna` ale stornării la `:681-699`), restul coordonatelor neschimbate. Un document operat ÎNAINTE ca tipul lui să fie migrat n-are tranzacție `Operare` ⇒ stornoul nu scrie nimic în cub (regimul dual e al momentului operării) — declarat; pe Flax după `--recreeaza` nu există cazul.
- `Materializare.Anuleaza(os, doc)` din `AnuleazaOperarea`: șterge fizic tranzacția `Operare` a documentului și postările ei (simetric cu ștergerea registrelor; fără `GCRecord`). Declarat; TR-D9 decide soarta anulării.
- `Corecteaza` (storno legat + document nou) trece prin cele două de mai sus, fără cod propriu.

### S-D6 — `Pozitie` pe `DocumentDetaliu`, citită de AMBELE motoare (B-r11)

`Pozitie` (int) se atribuie o singură dată, la salvarea unei linii noi cu
`Pozitie == 0`: în `SaveChanges` al contextului (un singur loc pentru UI,
WebApi, Import1C, conexul clonat) = max-ul liniilor documentului (din bază
+ cele urmărite) + 1, în ordinea urmăririi. `Fapte.Operand` ordonează
`OrderBy(Pozitie).ThenBy(ID)`; motorul vechi ordonează la fel în cele trei
enumerări (`MotorOperare.cs:150, :228, :333`) — singura atingere a lui
`MotorOperare` în afara celor trei apeluri de materializare. Consecință
DECLARATĂ pe Import1C: pe un document cu mai multe linii pe același lot,
ordinea devine cea de culegere în loc de ordinea heap-ului; N-D7 („ultima
ieșire ia restul") poate muta 0,01 între linii ale aceluiași document —
totalul pe lot e același; dacă raportul de reconciliere arată o diferență,
ea se explică EXACT prin asta sau e defect. `Pozitie` NU intră în DTO-urile
WebApi (React înghețat; ordinea de culegere e cea de azi).

### S-D7 — Restanțele care intră cu tipul lor

| Restanță | Ce intră | Cu pasul |
|---|---|---|
| B-r1 | `PoliticaTva.TolerantaTaxa` (per linie a cotei, semantica pilotului); `Fapte.Operand` o citește din politica tipului; seed-ul = valoarea care face Flax să treacă, MĂSURATĂ de gate-ul read-only (pasul 3) și consemnată pentru owner (S-r1) | FCT |
| B-r2 | `TipDocument.LaturaContPropriu` (PLT = Predator, INC = Primitor, seed pe ambele profiluri); `DeclarantTrezorerie` refuză `LATURA_CONT_PROPRIU_NEPOTRIVITA` când contul propriu nu e pe latura declarată; `CONT_PROPRIU_LIPSA` rămâne pentru lipsă | PLT/INC |
| B-r3 | NU intră: NIR-ul conex trăiește până la TR-D9, iar un rând `FCT/Stoc` în `RegulaContare` ar face motorul vechi să posteze recepția de două ori. Rămâne deschisă, cu motivul | — |
| B-r5 | `408 = 401` pe linia FCT care numește un lot deja recepționat: intră DOAR dacă motorul vechi are fluxul azi (NIR manual pe aviz → FCT pe același lot); pasul 3 constată; dacă nu există flux, rămâne deschisă cu constatarea (nu se implementează fără oracol) | FCT |
| B-r7 | linie cu două conturi `RolTert` ⇒ refuz `DOUA_CONTURI_TERT` în `Contari.Rezolva` | BCS |
| B-r10 | confirmată sau infirmată de gate-ul read-only pe Flax: liniile fără regulă pe care motorul vechi le sare | pasul 3 |
| B-r8, B-r4, B-r6, TR-r2, TR-r10, TR-r12 | NU intră (tipurile lor sunt ale feliilor TR-D7b+; B-r8 doar la un raport care cere nume) | — |

### S-D8 — Probele ModelCheck ale feliei (`STR-*`)

Scenele existente `VerificaNucleuBcs/Trezorerie/Fct` se extind; fiecare
probă comută `PosteazaInCub` pe tipul ei ÎN OS-ul scenei, operează, verifică,
și RESTAUREAZĂ valoarea (seed-ul rămâne `false` până la pașii 4–5, ca
scenele celorlalte tipuri să nu se schimbe pe neanunțate):

- `STR-SCHEMA-*` (S-D2);
- `STR-OPERARE`: după operare există EXACT o `Tranzactie(Operare)` a documentului, postările persistate = `Comparabil(contract)` = registrele normalizate (oracolul B-D7/B-D8 existent), `STR-ROUNDTRIP` egalitate structurală;
- `STR-STORNO`: după `Storneaza` există a doua tranzacție `Fel = Storno`, cu postările = inversul exact (nucleu `Storno.Inverseaza` pe rândurile citite), `PerioadaDeclarare` = a stornării; suma cub a documentului = 0 pe fiecare coordonată;
- `STR-ANULARE`: după `AnuleazaOperarea` zero rânduri ale documentului;
- `STR-REFUZ`: refuzul declarației lasă ZERO rânduri în cub ȘI zero rânduri în registre (tranzacția s-a anulat integral), documentul rămâne `Draft`;
- `STR-CONFIG`: `PosteazaInCub = true` pe un tip fără declarant (ex. `NTC`) ⇒ `OperareException`, nimic scris;
- `STR-NEMIGRAT`: tip cu `PosteazaInCub = false` ⇒ zero rânduri;
- `STR-POZITIE`: liniile primesc `Pozitie` 1..n în ordinea adăugării; a doua salvare nu le renumerotează;
- după pașii 4–5 (seed `true`): TOATE scenele existente care operează BCS/PLT/INC/FCT materializează; orice refuz nou al declarației pe o scenă pe care motorul vechi o operează e constatare → decizie a main-ului (politică sau defect), nu normalizare.

### S-D9 — Gate-ul de reconciliere al fizicii pe tipul migrat (două unelte, ambele în ModelCheck)

1. **Read-only, înainte de comutare** (`ModelCheck --declaratie-pe-baza <baza> <coduri>`): pe o clonă a `Atlas.Conta.Import1C.Flax`, pentru fiecare document OPERAT al tipurilor cerute, `Contractare.Contracteaza` contra `CubDinRegistre` + `Normalizari` (oracolul pilotului, neschimbat), fără nicio scriere. Raport: per tip — documente, egale, refuzate (per cod, cu id-uri exemplu), diferite (per fel de reziduu, cu id-uri exemplu), distribuția abaterii TVA culese (max, percentile) pentru B-r1, liniile fără regulă (B-r10), reziduurile `Normalizari.Avertismente`. Rularea e „măsurătoarea" tipului; NIMIC nu se comută înainte ca raportul să fie 100 % egal sau fiecare diferență DECLARATĂ aici.
2. **Persistat, după Import1C integral** (`ModelCheck --reconciliere-cub <baza>`): SQL pe set, pe tipurile cu `PosteazaInCub`: (a) Σ `Valoare` per `(grup, Cont, Latura, lună)` din cub (`Fel = Operare`) = Σ din `RegistruContabil` (`!Storno`) unde grupurile sunt `{BCS}`, `{PLT}`, `{INC}`, `{FCT ∪ NIR conex al FCT}` (TR-D3: recepția e pe FCT în cub, pe NIR în registre, aceeași zi pe toate perechile — review/04 §R4.3); (b) Σ `Cantitate` per `(grup, Unitate=lot, lună)` pe `Spatiu = Stoc` = Σ `RegistruStoc` pe aceleași grupuri; (c) Σ per `(TipTva, Sens, Rol, PerioadaDeclarare)` = `RegistruTva` (Bază/Taxă) pe FCT; (d) fiecare tranzacție e balansată per `Carte` (Σ D = Σ C) și per document EXACT o tranzacție `Operare`; (e) numărul documentelor operate ale tipurilor migrate = numărul tranzacțiilor `Operare`. Toleranță 0 (nu 0,005): cubul și registrele sunt scrise din același operand. Orice rând ≠ 0 e defect sau consecință declarată (S-D6 pe 0,01 între linii NU apare aici — sumele sunt per lot).

### S-D10 — Proba supremă (TR-D10) și înghețul

- Import1C integral pe `Atlas.Conta.Import1C.Flax` cu `--recreeaza --cititori --inchide-lunile` (+ `--reclasifica`), cu seed-ul `PosteazaInCub = true` pe BCS/PLT/INC/FCT: exit 0, ZERO refuzuri ale declarației, raportul de reconciliere IDENTIC pe conținut sortat cu `nou/tools/Import1C/reconciliere-20260918-154628.txt` (diferă doar antetul), 12/12 luni închise cu 0 constatări, `Reconstruieste` 0 diferențe, `--dump-integritate-tph` 0 încălcări, `--reconciliere-cub` 0 rânduri ≠ 0. Durata se consemnează A/B față de 3 h 21 min (felia 28) — cifră, nu gate. Proces detașat, log în `run-nucleu/tr-d7a/import/`.
- `refuzuri.ps1` pe `Atlas.Conta.BackOffice.Privat` refăcută din noul import + updater: toate PASS (285/285 la F28; cifra curentă se consemnează).
- `git diff --stat main -- nou/…Blazor.Server nou/…WebApi nou/Atlas.Conta.Client` GOL cu EXCEPȚIA fișierelor generate (`Client/src/generated/metadata.json`, openapi) — coloanele noi pe `TipDocument`/`PoliticaTva`/`DocumentDetaliu` intră mecanic în metadata; `Postare`/`Tranzactie` NU trebuie să apară în metadata clientului (dump-ul le exclude ca tipuri fără UI; dacă nu are mecanism, se raportează).
- `MotorOperare.cs`: diff-ul conține DOAR cele trei apeluri de materializare (S-D4, S-D5) și `OrderBy(Pozitie)` (S-D6); hook-urile frunzelor neatinse; declaranții se ating doar pentru B-r2, B-r5, B-r7 și `TolerantaTaxa`.

### S-D11 — Ce NU intră (amânări cu nume)

- Citirile pe cub (`Sold`, proiecții, fișe, SAF-T/D394/D406 din cub): TR-D8. D406/SAF-T citesc registrele neschimbat — gate-ul „D406 trece DUK" e trivial la TR-D7 și se consemnează ca atare.
- Deschiderea ca tranzacție (`Deschidere.cs`, TR-r10), notele pe stoc fără lot (TR-r2), Δ 3xx (TR-r12), sink-urile bugetare (TR-r7): cu tipurile lor (NTC, BTR, ASM, deschiderea) în TR-D7b+.
- Persistarea deciziilor/ipotezelor contractului, re-măsurarea indexilor (TR-r3), `(Unitate, Data)` (FZ-r3): TR-D8.
- Partida ca rând, FK pe `Unitate` pe Contabil, fișa de imobilizare ca unitate, `Împerechere` ca document, tăierea registrelor/hook-urilor/conexului: TR-D9.
- B-r3, B-r4, B-r6, B-r8: rămân deschise cu motivele din S-D7.

### S-D12 — Regula de oprire a agenților

Un agent se oprește și raportează cu dovada, fără să normalizeze, când:
(a) modelul EF nu se poate construi cu POCO fără `BaseObject` (convenții
globale), sau XAF refuză tipul la bootstrap; (b) migrația generată nu poate
fi înlocuită curat cu SQL (ex. EF cere tabela pentru FK-uri generate);
(c) o scenă pe care motorul vechi o operează e REFUZATĂ de declarant;
(d) `--declaratie-pe-baza` dă diferențe care nu sunt în B-D8 sau în S-D6/S-D9;
(e) `Fapte.Operand` ar cere o interogare per linie sau depășește 16;
(f) nucleul ar avea nevoie de un pachet/referință (testul de arhitectură);
(g) un `Check` existent cade; (h) Import1C dă un refuz al declarației sau
raportul diferă de baseline; (i) forma cere `is`/`switch` pe frunză;
(j) ar trebui atins ceva în Blazor.Server/WebApi/Client în afara fișierelor
generate. Nu comite; nu atinge directoarele altor pași; raportul e cu
`path:line` și cifre.

## Pașii (un agent per pas; main verifică independent și comite per pas)

0. **Contractul** (main): fișierul de față; commit.
1. **Schema și entitățile** (S-D1, S-D2, coloanele de politică, `Scara`, seed-ul cu `PosteazaInCub = false` + `LaturaContPropriu` PLT/INC + `TolerantaTaxa` provizoriu 0,01; `STR-SCHEMA-*`; `STR-POZITIE` cu atribuirea din S-D6; `has-pending-model-changes` curat; `--dump-metadata` cu diff-ul raportat; ModelCheck verde pe AMBELE profiluri). Oprire: S-D12 (a), (b), (g), (j).
2. **Materializarea** (S-D3, S-D4, S-D5, S-D6 ordinea în ambele motoare, amendamentul N-D10 în nucleu cu test; `STR-OPERARE/ROUNDTRIP/STORNO/ANULARE/REFUZ/CONFIG/NEMIGRAT` pe scenele BCS/PLT/INC/FCT cu comutare locală; nucleu `dotnet test` verde; ModelCheck verde pe ambele profiluri). Oprire: (c), (e), (f), (g), (i).
3. **Gate-ul read-only pe Flax** (S-D9.1 ca unealtă ModelCheck; rulat pe clona `Atlas.Conta.Import1C.Flax` pentru BCS, PLT, INC, FCT; raportul în `run-nucleu/tr-d7a/pas3/`; constatările B-r1 (distribuția abaterii), B-r5 (există fluxul 408?), B-r7, B-r10; `--reconciliere-cub` scris (S-D9.2) și probat pe scenele ModelCheck). Oprire: (d) — raportul e livrabilul, deciziile sunt ale main-ului.
4. **BCS și PLT/INC pe cub** (seed `true` pe ambele profiluri; B-r2, B-r7; fix-urile decise de main din pasul 3; toate scenele existente ale celor trei tipuri materializează; ModelCheck verde pe ambele profiluri, `--declaratie-pe-baza` re-rulat pe cele trei tipuri = 100 % egal). Oprire: (c), (d), (g).
5. **FCT pe cub** (seed `true`; B-r1 ca politică cu valoarea măsurată; B-r5 dacă pasul 3 a găsit fluxul; ModelCheck verde; `--declaratie-pe-baza` FCT = 100 % egal sau diferențele declarate). Oprire: (c), (d), (g).
6. **Proba supremă** (main lansează procesul detașat; agentul pregătește rețeta `run-nucleu/tr-d7a/import/run.sh` cu Import1C integral → `--reconciliere-cub` → diff sortat cu baseline-ul → `--dump-integritate-tph` → clona `BackOffice.Privat` + updater → `refuzuri.ps1`; sumar cu cifrele). Oprire: (h).
7. **Review advers** (agent separat, read-only, tier-ul main-ului; zone: tranzacția comenzii și rollback-ul parțial, ordinea Pozitie contra ordinea veche pe Import1C, stornoul cross-perioadă, anularea cu dependenți, gestiuni virtuale fără FK, snapshot-ul EF divergent de PK, concurența serializată a operării cu partiția, `Valideaza` pe tip migrat, seed-ul `DinSeed` care forțează `PosteazaInCub` la re-seed pe o bază unde owner-ul l-a oprit). Fix-urile le aplică main-ul.
8. **Docs și închidere** (main): `stare-curenta/domeniu-si-operare.md` (secțiune „Cubul persistat și regimul dual"), `dezvoltare-si-validare.md` (probele `STR-*`, cele două unelte, rețeta importului, capcanele), `restante.md` (B-r1/2/5/7/9/10/11 închise sau amendate; S-r*), `istoric-plan-de-lucru.md` (felia 31), `nucleu-transfer.md` Stare, `invarianti.md` dacă I cere reconciliere pe anulare, CLAUDE.md §Stare/§Următorul pas (TR-D7b), memorie.

## Regula de oprire a feliei

- Entitățile, migrația partiționată și cele trei coloane de politică există și sunt probate (`STR-SCHEMA-*`) pe ambele profiluri; `has-pending-model-changes` curat.
- Materializarea rulează în tranzacția comenzii pe BCS, PLT, INC, FCT cu seed `PosteazaInCub = true` pe ambele profiluri; `STR-*` verzi; nucleu `dotnet test` verde (156 + ce aduce N-D10 amendat), 0 avertismente, testul de arhitectură neatins.
- `--declaratie-pe-baza` pe Flax = 100 % egal pe cele patru tipuri sau fiecare diferență declarată în S-D6/S-D9 cu cifra ei; Import1C integral: raport IDENTIC pe conținut sortat cu baseline-ul feliei 28, zero refuzuri ale declarației, `--reconciliere-cub` 0 rânduri ≠ 0, `--dump-integritate-tph` 0, `refuzuri.ps1` toate PASS.
- ModelCheck verde pe AMBELE profiluri cu toate `Check`-urile de azi.
- Diff-ul pe Blazor.Server/WebApi/Client gol în afara fișierelor generate; `MotorOperare.cs` atins doar cum spune S-D10.
- Review advers aplicat; docs din pasul 8 în commit-ul de închidere; `Stare: ÎNCHISĂ` cu data. Decizie proprie NU e necesară (felia execută 090l); amendamentele de literă (FZ-D2 `UnitateDeschisa`, N-D10 `perioadaDeclarare`, fără FK pe partida) se consemnează în contract și în `restante.md`, nu într-o decizie nouă.

## Restanțe noi (S-r*)

| Id | Conținut | Stare |
|---|---|---|
| S-r1 | valoarea de seed a `PoliticaTva.TolerantaTaxa`: măsurată pe Flax la pasul 3, seed-uită provizoriu la valoarea care face importul să treacă; owner-ul decide valoarea de produs | deschisă |
| S-r2 | deciziile și ipotezele contractului (`AlocareFifo`, `ValoareIesire`, `SoldUnitateCitit`…) nu se persistă; un cititor (audit, „de ce a costat atât") le cere la TR-D8 | deschisă |
| S-r3 | gestiunile virtuale n-au FK (id-uri fără rând); B-r8 le face rânduri `DinSeed` și atunci FK-ul pe `Gestiune` intră pe ambele partiții | deschisă |
| S-r4 | snapshot-ul EF declară cheia `ID` pe `Postare`, baza are `(Spatiu, ID)`: divergență declarată cât timp XAF EF Core nu suportă chei compuse; probată de `STR-SCHEMA` | deschisă |
