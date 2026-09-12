# Pasul 5, felia 24 — politicile: execuția deciziei 83, ecranele rămase și „Explică" (contract)

Data: 2026-09-10. Stare: ÎNCHISĂ 2026-09-11 (decizia 84; §Închidere). Pleacă din decizia 83 (seed-ul și
proveniența: re-seed pe `DinSeed`, golurile privat cu poarta spre DVI, rolul
`Configurator`), din 81k („Explică" + ecranele `RegulaContare`/`RegulaStoc`/
`MapareD300`/`MapareD394`/`PoliticaTva`/`Conex`/`Validare`) și din 81-r8.
Decizia rezultată: 084 (se scrie la închidere). Se livrează pe
`multiagent-delivery`: un agent per pas, regulă de oprire per pas, verificare
independentă, review advers la închidere.

Două track-uri cu regulă de oprire proprie:

- **Track A — configurația**: execuția deciziei 83 (seed, goluri, rol) + cele
  șapte ecrane de politică rămase peste `GrilaPolitica`.
- **Track B — „Explică"**: potrivirea motorului extrasă ca funcții PURE pe
  fapte (`Motor/Potrivire.cs`), consumate de motor, de oglinzile de mână și
  de `GET api/politici/explica`; panou în client. **Avans declarat pe
  contractul de izolare a motorului** (`p5-felia-izolare-motor-contract.md`,
  IM-D2 explicația / IM-D4 contractul „Politici"): forma faptelor de aici e
  forma pe care o va consuma pasul 2 al lui IM, nu o lucrare paralelă.

## Scop

| Zona | Azi | Problema |
|---|---|---|
| Re-seed (83a–e) | „insert dacă lipsește" pe toate politicile; `Seedat` timbrează doar creările; rândul șters nu se recreează | un rând al profilului corectat în cod rămâne, pe bazele existente, în forma primei seed-uiri — și e timbrat seed, deci raportul de profil nu-l vede |
| Golurile privat (83f–g) | `ImpliciteTva` privat = 6 rânduri (FCL/RDC × UE/extra-UE → SDD; FCT/RLF × UE → TI21) | achiziția de la neînregistrat RO și importul cad pe ancora `N21`, plauzibil pe orice linie |
| Cine editează (83h–i) | `Administrators` singurul scriitor; `Cititori` dev-only; cele 17 tipuri `ICuProvenienta` enumerate de mână în gardian (`switch`) și în raport (`Tabel<T>`) | contabilul care configurează = administratorul de useri; un tip nou poate scăpa din una din liste |
| Ecranele de politică | 7 ecrane peste `GrilaPolitica` (81i); lipsesc `RegulaContare` (8 câmpuri de potrivire/conturi + 24 FK-uri de dimensiuni comun/override per latură, 15), `RegulaStoc`, `PoliticaTva`, `PoliticaConex`, `PoliticaValidare`, `MapareD300`, `MapareD394` | configurația e scriibilă pe OData (81e) dar fără ecran; **81-r8**: `SursaCont.Explicit == 0` e default-ul enum-ului, iar `Explicit ⇒ cont` (81e) refuză rândul nou dacă ecranul nu propune sursa |
| Explicabilitatea | rezolvarea motorului se vede DOAR prin dry-run `valideaza` pe un document cules; potrivirea e inline în `MotorOperare` (`RegulaContare`: `SemnFiltru` → TipMaterial exact → `NaturaFiltru` → generic, `MotorOperare.cs:150-159`; `RegulaStoc`: per latură, Clasă exactă bate genericul pe `Natura=Stoc`, `PotrivesteReguliStoc`; `PoliticaTva`: `FirstOrDefault` pe tip; `PoliticaConex`: `NaturaFiltru`; implicitul TVA: `ImpliciteService`) | oglinzile de mână există și trebuie ținute fidele manual — patru interogări `GetObjectsQuery<RegulaContare>` în frunze: gardul VIR (`Trezorerie.cs:332`, oglindește `SemnFiltru` + nivelurile), refuzul 38c „linie de stoc fără regulă per Tip" pe DSC (`DescarcareGestiune.cs:50`), FCL (`FacturaIesire.cs:103`) și RDC (`Retururi.cs:208`) — cele trei din urmă oglindesc DOAR axa `TipMaterialId != null`, nu și `SemnFiltru` (64: „un gard care oglindește o potrivire oglindește TOATE axele ei" — azi nu e adevărat); `RegulaStoc` are o A DOUA definiție a potrivirii, `DescarcareService.TipStocPentruClasa` (Clasă exactă bate genericul, „ca în motor"), cu doi apelanți reali (`DescarcareService.Genereaza`, `FacturaIesire.ValideazaOperare`); `Import1C/Catalog.cs:328` (`IncarcaContare`) citește `RegulaContare` pentru un singur tipar (`Explicit`/`TipMaterial`, 6xx = 3xx) ca predicție; un contabil nu poate întreba „de ce postează 371 = 401 pe linia asta?" fără să culeagă un document |

Mecanica de dedesubt, măsurată pe surse:

- Potrivirea din motor lucrează deja pe DATE plate: `reguliContare` e
  `List<RegulaContare>` încărcată o dată per tip, `claseTip` e dicționar
  `(ClasaId, Natura, Denumire, ContImplicitId)`, `RezolvaCont` e deja
  `static` pur pe `(SursaCont, contExplicit, contTipMaterial, repartitori)`.
  Extragerea ca funcții pure nu schimbă algoritmul, doar îl numește.
- `GardianEditare.Verifica` e `public static`, chemat direct de ModelCheck
  fără host; `ContaSeeder.Seed` la fel — probele de re-seed nu cer host.
- `GrilaPolitica` citește prin `storeOData` și scrie prin `api.ts`
  (mesajul `EroriDto` în rând, 81i); coloanele sunt COD per ecran, lookup-urile
  de enum iau etichetele din `metadata.json` (57f).
- Rolurile sunt seed-uite în `Updater.cs` cu `PermissionPolicy` +
  permisiuni pe tip; `refuzuri.ps1` are trei oracole (Admin/Cititor/User) și
  blocul „politici" (81j).
- `VerificaD300`/`VerificaD394` din seed compară mapările reale cu listele
  „nemapate deliberat" (`NemapateDeliberat`, `NemapateDeliberatD394`); un tip
  nou fără mapare și fără motiv pică seed-ul.

## Deciziile

### Track A

#### F24-D1 — Un singur helper de aliniere în seed (83a–d)

`ContaSeeder.Aliniaza<T>(T gasit, Func<T> creeaza, Action<T> seteaza)`:

- `gasit == null` ⇒ `Seedat(creeaza())` + `seteaza`; contor „creat".
- `gasit.DinSeed` ⇒ `seteaza` pe o copie de comparație: câmpurile ne-cheie
  care diferă se scriu și se tipăresc `tip / cheie / câmp: vechi → nou`;
  contor „corectat"; zero diferențe = nimic scris.
- `!gasit.DinSeed` ⇒ neatins; contor „manual pe cheie de seed" (listat, ca
  azi în raport).
- Rândul șters logic (`IgnoreQueryFilters`) rămâne șters; se spune (ca azi).

Toate locurile „găsit == null ⇒ creează" din `ContaSeeder`/`ProfilPrivat`/
`ProfilBugetar` trec pe helper; cheia fiecărui apel = cheia indexului unic
(83b), niciodată altceva. Seed-urile derivate (`SeedContare6xxDin3xx`,
`SeedContareVanzare`) recalculează rândul `DinSeed` de pe cheia acoperită și
sar cheia acoperită manual (83c). `SeedTipuriTvaInactive` neschimbat.
`SetareProfil`, `Societate`, repartitorii rămân în afara helper-ului (nu sunt
`ICuProvenienta`).

Sumarul tipărit la final: per tabel `create / corectate / manuale / șterse`.

#### F24-D2 — Seed-ul privat închide golurile (83f–g)

- `TipTva` nou `IMP` („Achiziție din import — TVA prin DVI", 0 %, regim
  `Neimpozabil`, `Activ`), cod SAF-T luat din nomenclatorul ANAF
  (`anaf/` local, memoria `anaf-saft-nomenclator-xlsx`); dacă nomenclatorul
  n-are cod distinct pentru factura furnizorului extern, alegerea și motivul
  stau în seed lângă rând (83-r3).
- `ImpliciteTva` += `FCT/NeinregistratRo → NIM`, `RLF/NeinregistratRo → NIM`,
  `FCT/ExtraUe → IMP`, `RLF/ExtraUe → IMP`.
- `NemapateDeliberat` (D300) += `IMP × Achizitie` („baza și taxa se declară
  din DVI"), `IMP × Livrare` („tip de achiziție"); `NemapateDeliberatD394`
  += ambele sensuri („partener extra-UE nu se declară"). `VerificaD300`/
  `VerificaD394` rămân verzi prin liste, nu prin excepție în cod.
- Bugetar: zero rânduri noi.

#### F24-D3 — `Politici.TipuriConfigurabile` + rolul `Configurator` (83h–i)

- `BusinessObjects/Politici/TipuriConfigurabile.cs`: `IReadOnlyList<Type>`
  static cu cele 17 tipuri `ICuProvenienta`. Consumatori: seed-ul rolului,
  `VerificareProfilService` (înlocuiește apelurile `Tabel<T>` de mână),
  proba de acoperire. Gardianul își păstrează `switch`-ul pe tip pentru
  invarianții specifici (81e); pasul de proveniență rămâne pe `is
  ICuProvenienta` (nu are nevoie de listă).
- `Updater.cs`: rol `Configurator` = `ReadOnlyAllByDefault` + pentru fiecare
  tip din listă `AddTypePermissionsRecursively(Create|Write|Delete, Allow)`;
  Deny explicit pe `PermissionPolicyRole`/`ApplicationUser` (ca `Default`),
  nimic pe documente/registre/`Societate`/`SetareProfil`. User dev
  `Configurator` fără parolă, ca `Cititor` (80g), creat de același updater.
- Probă ModelCheck: reflecție peste assembly-ul Module — orice tip concret care
  implementează `ICuProvenienta` e în listă și invers; rolul (după seed) are
  Write pe fiecare tip din listă și NU are Write pe `Document`/`Registru*`.
- `refuzuri.ps1`: al patrulea oracol `Configurator`: 200/201/204 pe blocul
  „politici" (aceleași probe ca `Admin`, inclusiv domeniul 422), 403 pe
  scrierea unui document, pe comenzi, pe `Societate`; 200 pe `verificare`.
- 83-r5 (navigația XAF pentru rol) nu intră.

#### F24-D4 — Cele șapte ecrane peste `GrilaPolitica`

Un ecran per politică în `felii/politici/`, pe șablonul existent
(`PoliticiScadenta.tsx`): coloane COD, lookup-uri pe `storeOData`
(`TipDocument`, `TipMaterial`, `ClasaProdus`, `Cont`, `TipTva`, `RandD300`),
enum-uri cu etichete din `metadata.json`.

| Ecran | Coloane în grilă | Particularități |
|---|---|---|
| `RegulaStoc` | TipDocument, Latura, Clasa?, TipStoc, Semn | rând nou: `Semn` propus ±1 după latură; unicitatea o dă gardianul (81c) |
| `RegulaContare` | TipDocument, TipMaterial?, NaturaFiltru?, SemnFiltru?, PastreazaSemn, SursaContDebit, ContDebit?, SursaContCredit, ContCredit? | **formular popup cu grupuri** (`GrilaPolitica` capătă modul `formular`): „Potrivire", „Conturi", „Dimensiuni comune", „Override debit", „Override credit"; cele 24 de FK-uri de dimensiuni sunt DOAR în formular (`visible={false}` în grilă); **81-r8**: `onInitNewRow` pune `SursaContDebit = SursaContCredit = Explicit` VIZIBIL, iar `Explicit ⇒ cont` (81e) rămâne al gardianului, nu al TS-ului |
| `PoliticaTva` | TipDocument, Directie, SursaContrapartida, ContrapartidaFallback? | rând nou: `SursaContrapartida = Explicit` (81-r8) |
| `PoliticaConex` | TipDocumentSursa, TipDocumentTinta, InverseazaLaturi, NaturaFiltru? | — |
| `PoliticaValidare` | TipDocument, CereClasificatieBugetara, NaturaInterzisa? | `CereClasificatieBugetara` afișat pe ambele profiluri (DIM-4 rămâne) |
| `MapareD300` | TipTva, Sens, Rand | lookup `RandD300` filtrat pe `Operatiuni` (69b) — filtrul e afordanță, refuzul e al gardianului |
| `MapareD394` | TipTva, Sens, Tip | lookup `Tip` din enum — `TintaPermisa` (71c) rămâne a gardianului |

Vizibilitatea dimensiunilor per profil rămâne DIM-4 (nu intră). Fiecare grilă
primește, după B, butonul „Explică pe acest tip" care deschide panoul
precompletat (F24-D7).

### Track B

#### F24-D5 — `Motor/Potrivire.cs`: potrivirea ca funcții pure pe fapte

Fapte = `record`-uri plate cu Id-uri și valori, fără entități (forma IM-D4):

```
RegulaContareFapt(Id, TipMaterialId?, NaturaFiltru?, SemnFiltru?, PastreazaSemn,
                  SursaContDebit, ContDebitId?, SursaContCredit, ContCreditId?, DinSeed)
RegulaStocFapt(Id, Latura, ClasaId?, TipStoc, Semn, DinSeed)
LinieFapt(TipMaterialId, ClasaId, Natura, Semn, LotId?, ContImplicitTipId?)
LaturiFapt(ContImplicitPredatorId?, ContImplicitPrimitorId?)
```

Funcții (toate `static`, fără I/O, fără `IObjectSpace`):

- `Potrivire.Contare(reguli, linie) → Potrivit<RegulaContareFapt>`:
  `Castigator?`, `Nivel` (`TipMaterialExact | Natura | Generic | Niciuna`),
  `Candidati` cu `MotivEliminare` per rând (`SemnNepotrivit`, `NivelMaiSlab`,
  `TipMaterialDiferit`, `NaturaDiferita`). Algoritmul = exact
  `MotorOperare.cs:150-159`.
- `Potrivire.Stoc(reguli, linie) → IReadOnlyList<PotrivitStoc>` per latură,
  cu `Nivel` (`ClasaExacta | Generic | Niciuna`) și motivul pentru
  `Natura != Stoc`. Algoritmul = `PotrivesteReguliStoc` fără crearea
  mișcării; **înlocuiește și `DescarcareService.TipStocPentruClasa`** (a doua
  definiție moare; cei doi apelanți ai ei consumă `Potrivire.Stoc`).
- `Potrivire.Cont(sursa, contExplicit, contTipMaterial, laturi) →
  (ContId?, Sursa: string)` — `RezolvaCont` mutat, plus explicația sursei
  („din TipMaterial", „din repartitorul primitor", „fallback explicit").
- `Potrivire.Conex(politica, natura liniilor) → liniile eligibile + motiv`.
- `Potrivire.TvaImplicit(...)` = clasamentul din `ImpliciteService` (clasă
  exactă → generic, `ValabilDeLa` cel mai recent, inactivul sare cu motiv),
  fără citirea nomenclatoarelor.

**Motorul consumă `Potrivire`** (`CalculeazaSiValideaza` mapează entitățile
la fapte o dată per document, apoi cheamă funcțiile); nicio a doua definiție.
**Oglinzile mor**: gardul VIR din `Trezorerie.cs` devine `Potrivire.Contare(…)
.Nivel is Generic or Niciuna ⇒ refuz`; refuzul 38c pe DSC/FCL/RDC devine
`Nivel != TipMaterialExact ⇒ refuz` (capătă și axa `SemnFiltru`, pe care azi
n-o oglindește — 64); `GetObjectsQuery<RegulaContare>` dispare din
`BusinessObjects/Documente`; `TipStocPentruClasa` dispare din
`DescarcareService`. Predicția din `Import1C/Catalog.IncarcaContare` NU se
atinge în felie (F24-r1): e un tipar, pe o unealtă cu baseline propriu.
Regula de oprire a pasului: ModelCheck
IDENTIC pe ambele profiluri + primele probe PURE din ModelCheck (fapte în
memorie, fără bază: semnul scoate rândul din joc la toate nivelurile; Tip
exact bate Natura bate generic; Clasă exactă bate genericul de stoc; linia
ne-Stoc fără regulă pe Clasă nu intră în stoc).

#### F24-D6 — `GET api/politici/explica`: explică CONFIGURAȚIA pe o linie ipotetică

Parametri: `tip` (cod), `tipMaterial` (id), `semn` (±1, default +1),
`data` (default azi), opțional `predator`/`primitor` (id repartitor),
`partener`/`produs` (pentru implicitul TVA). Nu cere document.

Răspuns `ExplicatieDto`, un bloc per mecanism, fiecare cu `Castigator`,
`Nivel`, `Candidati[]` (rândul + motivul eliminării + `DinSeed`) și
`Concluzie` text scurt:

- `Contare`: regula, conturile rezolvate cu sursa fiecăruia
  („371 din TipMaterial", „401 din repartitorul predator"), sau „linia nu
  contează pe acest tip" (26c).
- `Stoc`: per latură, regula, `TipStoc`, semnul, sau „nu intră în stoc".
- `Tva`: `PoliticaTva` a tipului, direcția, contrapartida; „tipul nu postează
  TVA".
- `Conex`: politicile sursă → țintă și dacă linia ipotetică ar trece filtrul.
- `Implicit`: rezultatul `ImpliciteService` cu sursa și motivul (81f).
- `Validare`, `Scadenta`, `Numerotare`: rândul tipului sau absența.

Gate (80e/81g): `CanRead` pe toate tipurile din `TipuriConfigurabile` +
`TipMaterial` + `Repartitor` luat pe ușa SECURIZATĂ înainte; calculul pe ușa
NON-SECURED (pe cea filtrată explicația ar fi FALSĂ, nu goală). `User` ⇒ 403
`EroriDto`; tip necunoscut ⇒ 400 `EroriDto` (70f); `tipMaterial` invizibil ⇒
422 (80f). Ruta stă în `PoliticiController`, lângă `verificare`.

Diferența declarată față de IM pasul 5: aici se explică **configurația** pe o
linie ipotetică; acolo planul unui **document**. Amândouă consumă
`Potrivire`; a doua nu se face aici.

#### F24-D7 — Panoul „Explică" în client

Ecran `/politici/explica`: formular (tip document, tip material, semn, dată,
laturi, partener, produs — lookup-uri pe `storeOData`, `Lookup.filtru` unde
are sens) + un card per mecanism cu câștigătorul, nivelul, candidații cu
motivul lor și marcajul „seed / manual" (`DinSeed`, ca în grile). Zero calcul
în TS (42c, 81f). Din fiecare grilă de politică, „Explică pe acest tip"
deschide panoul precompletat (URL = starea, 43c). Refuzurile pe ramura unică
`Erori[]` (80h).

## Testul contra invarianților

- **IV**: track A nu adaugă mecanism, adaugă rânduri și permisiuni; `IMP` e
  rând sub regim existent. Track B nu schimbă algoritmul de potrivire, îl
  numește și îl expune; „Explică" e citire, nu politică interpretabilă.
- **II**: `Potrivire` lucrează pe fapte plate; motorul rămâne singurul care
  mapează entități → fapte; nicio ramură nouă pe tip.
- **III**: neatins; „Explică" nu citește registre.
- **V**: codul SAF-T al lui `IMP` vine din nomenclator, semantica din lege.

## Pașii (un agent per pas, regulă de oprire per pas, verificare independentă)

Track A și B sunt independente pe fișiere până la pasul 6 (A nu atinge
`Motor/MotorOperare.cs`; B nu atinge seed-ul, `Updater.cs`, grilele); pot
rula în paralel pe branch-uri proprii, cu merge-ul lui B DUPĂ re-rularea
Import1C.

**Track A**

1. **Seed-ul** (D1 + D2): helper-ul `Aliniaza`, migrarea tuturor
   apelurilor, derivatele, sumarul; `IMP`/`NIM`, listele nemapate. Oprire:
   ModelCheck ambele profiluri, cu probele noi (rând seed modificat pe ușa de
   sistem → readus; editat securizat → lăsat; șters → nu se recreează, raportat;
   cheie derivată acoperită manual → neatinsă; `IMP`/`NIM` propuse de
   `ImpliciteService` cu sursa `Politica`); seed-ul pe Flax rulează cu sumar
   „0 corectate" la a doua rulare; **re-rularea integrală Import1C identică
   cu baseline-ul** (seed-ul privat capătă rânduri, nu schimbă conturi;
   Import1C păstrează `tipTva` „ca în sursă", 70c).
2. **Rolul** (D3): lista, rolul, userul, probele ModelCheck, blocul în
   `refuzuri.ps1`. Oprire: ModelCheck verde; `refuzuri.ps1` cu 4 oracole
   PASS/0 FAIL pe host viu, după re-seed Privat.
3. **Ecranele** (D4): modul `formular` în `GrilaPolitica`, cele 7 ecrane,
   81-r8, rutele + navigația. Oprire: `verifica:drift` verde; smoke în browser
   per ecran (creare, editare cu 422 al gardianului vizibil în rând, ștergere,
   `Configurator` vs `Cititor`); `RegulaContare` nouă cu sursă `Explicit`
   fără cont ⇒ mesajul gardianului, nu 500.

**Track B**

4. **`Potrivire.cs`** (D5): faptele, funcțiile, consumul din motor, moartea
   oglinzilor (VIR, 38c, raport). Oprire: ModelCheck IDENTIC (aceleași
   numere OK, 0 FAIL) + probele pure; `grep "GetObjectsQuery<RegulaContare>"
   BusinessObjects/Documente` = 0 și `grep TipStocPentruClasa Module` = 0
   (cele cinci oglinzi au murit); **Import1C integral identic**.
5. **`explica`** (D6): DTO, gate, ruta, probe HTTP (Admin/Configurator/
   Cititor 200, User 403, tip necunoscut 400, tipMaterial invizibil 422);
   drift.
6. **Panoul** (D7) + butoanele din grile. Oprire: smoke în browser pe FCT
   (linie de stoc: „nu contează, recepția e pe NIR" + conex NIR; linie de
   serviciu: 6xx = 401), VIR (nivel generic ⇒ concluzia spune refuzul), LDI
   (semnul schimbă regula).

**Închidere**

7. **Review advers + docs**: scenariile din §Review; decizia 084; CLAUDE.md
   (§84, roadmap, restanțe); `docs/stare-curenta` (politici-si-fiscalitate:
   re-seed, implicitele noi, Configurator, „Explică"; api-si-client: ruta și
   ecranele; limite-curente: ce iese); istoricul planului; `docs/decizii/081`
   (81-r8 închisă).

## Review advers (scenariile de la închidere)

- Re-seed-ul corectează un rând pe care clientul îl credea al lui ⇒ doar cu
  timbru aprins (83e asumat, probat pe „editat ⇒ lăsat").
- Helper-ul schimbă o cheie ⇒ imposibil prin construcție (cheia e argumentul
  de căutare, nu al `seteaza`); probă: un `seteaza` care atinge cheia pică.
- Două rânduri seed pe aceeași cheie ⇒ eroare de seed, nu al doilea rând.
- `Configurator` poate scrie un document printr-un FK de politică? Nu:
  Write e pe tipurile din listă; documentele nu sunt în listă (probă 403).
- `Configurator` poate dezactiva `TipTva` folosit ca implicit ⇒ gardianul
  refuză (81e), indiferent de rol.
- `Potrivire` și motorul divergează ⇒ imposibil: motorul o consumă; proba
  ModelCheck identic e testul.
- `TipStocPentruClasa` cade pe regula generică FĂRĂ gardul `Natura == Stoc`
  pe care motorul îl are (`PotrivesteReguliStoc`); pe `Potrivire.Stoc`
  generatorul DSC capătă gardul. Divergența se DECLARĂ în pasul 4 cu probă
  (linie ne-Stoc pe FCL nu primește `TipStoc` din regula generică), nu se
  păstrează ca „a doua definiție".
- `explica` pe ușa non-secured dezvăluie conturi ale unui `TipMaterial`
  invizibil ⇒ 422 înainte de calcul (80f).
- Panoul afișează o concluzie pe care motorul n-ar lua-o (de ex. postarea
  explicită a Decontului bate regula) ⇒ `Contare` declară `PostareExplicita`
  ca nivel când tipul e `IDocumentCuPostareExplicita`, fără să calculeze
  contul (nu e linie reală).
- `IMP` pe FCL (livrare) ⇒ D300 nemapat cu motiv „tip de achiziție",
  gardianul nu-l interzice (regimul e cules, corectabil).

## Ce NU intră (amânări cu nume)

- DVI ca tip de document (83-r4) — poarta e deschisă, tipul nu.
- Recrearea unui rând de seed șters la runtime (83-r1).
- `Cont` scriibil pe OData (81k), export/import de politici între baze (81k),
  baseline-ul XAF pe politici și filtrul `Activ` (81-r7), navigația XAF a
  rolului (83-r5).
- Vizibilitatea dimensiunilor per profil în ecrane (DIM-4).
- Explicația pe DOCUMENT (planul cu regula/lotul/sursa dimensiunii) = IM
  pasul 5.
- D394 tip `N` (83-r2).
- **F24-r1** `Import1C/Catalog.IncarcaContare` pe `Potrivire.Contare` +
  `Potrivire.Cont` (predicția ar acoperi orice tipar de regulă, nu doar
  `Explicit`/`TipMaterial`) — cu baseline-ul ca probă, când se atinge unealta.

## Închidere (2026-09-11, decizia 84)

Livrată pe `multiagent-delivery` pe branch-ul `p5-f24-politici-explica`, cu
track-urile A și B în paralel (worktree + `MODELCHECK_BAZA_SUFIX`, clonă a
bazei bugetare), un agent per pas, verificare independentă și commit per pas,
review advers cu fix-uri înaintea probei finale.

| Pas | Commit | Proba |
|---|---|---|
| 1 seed (D1, D2) | `5da99e7` | ModelCheck privat 988 / bugetar 933; seed Flax ×2 (a doua `0 / 0`); **Import1C integral identic cu baseline-ul F18** (`run-f24a`, 467 linii, 932 avertismente) |
| 2 rol (D3) | `1129df2` | 991 / 936; `refuzuri.ps1` 100/100 (4 oracole) |
| 4 `Potrivire` (D5) | `5d15cb0` → merge `48fc5ef` | 986 (`.B`, fără DUK) / 934; smoke Import1C 3 luni pe clonă; merge 1001 / 943 |
| 5 `explica` (D6) | `1519447` | 1009 / 943; `refuzuri.ps1` 111/111; codegen idempotent |
| 3 ecrane (D4) | merge `5cc3ea8` | smoke în browser pe 7 ecrane, 81-r8 în popup, roluri |
| 6 panou (D7) | `7139918` | 19 cazuri în browser |
| 7a review advers | `cf607e8` | 1016 / 946; `refuzuri.ps1` 117/117 |
| 7b docs + proba finală | — | Import1C integral pe codul închis (`run-f24`, 1h51): CONTRACT ÎNDEPLINIT, 932 avertismente, raport IDENTIC cu baseline-ul F18 |

Devierile față de contract, declarate: `Aliniaza` primește predicatul și
cheia afișată (nu obiectul găsit) ca să centralizeze raportarea rândului
șters; `IMP` rămâne fără cod SAF-T (83-r3 tranșată), `NIM` capătă 308302;
`Cont.Functie/RolTert/Denumire/Parinte`, ancora `TipTvaImplicit`,
`PoliticaConex/Validare` se aliniază doar pe timbru; `SeedRolConfigurator`
rulează și pe RELEASE (83h), fără Deny suplimentar; enum-urile verdictului
stau în `BusinessObjects/Comun` (etichete în `metadata.json`), nu în `Motor`;
`GardContareAttribute` are două constructoare (C# nu acceptă `Nullable<enum>`
ca parametru de atribut); `Unitate*` read-only în formularul regulii de
contare; semnul regulii de stoc nu se propune după latură; D300/D394 fără
buton „Explică"; probele `VerificaF24Explica` doar pe privat, `F24-G3` pe
bugetar.

Review-ul advers (raport în sesiune) a găsit două defecte de fond (enum fără
membru scris tăcut prin OData și interpretat de motor; concluzia „Se
postează" pe linii pe care gardurile frunzelor le refuză) și trei medii (DEC
fără `PostareExplicita`, curățenia `PoliticaValidare` ștergea rânduri manuale,
ștergerea unui `TipTva` referit trecea) — toate fixate în `cf607e8`, cu
probe. Restanțele 84-r1…r12 sunt în decizia 84.
