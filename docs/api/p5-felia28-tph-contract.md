# Felia 28 — TPH cu discriminator pe cele trei ierarhii; tipul ca dată (contract)

Data: 2026-09-18. Stare: **deschisă, aprobată de owner** (decizie tehnică +
preferință declarată). Decizia rezultată se scrie la închidere ca **089**
(amendează 3 și 16; amendează IM-D10 din contractul de izolare). Execuția
pornește pe context nou, pe modul multiagent-delivery (un agent per pas,
regulă de oprire per pas, verificare independentă de main, commit per pas,
review advers la închidere). Branch: `p5-f28-tph`.

## Scop

Trei ierarhii sunt mapate azi TPT (decizia 3, 16): `Document` (20 de
derivate, două niveluri), `DocumentDetaliu` (12) și `Repartitor` (5). Faptele
măsurate pe schema curentă:

| Ierarhie | Tabele derivate | Coloane proprii |
|---|---|---|
| `Document` | 20 | 15 tabele au ZERO coloane proprii (NIR, BonConsum, NotaContabila, InchidereTva, Asamblare, Dvi, LDI, PIF, AmortizareLunara, RaportProductie, ReturClient, ReturFurnizor, DSC, Plata, Incasare); FacturaIntrare 15, DocumentTrezorerie 4, Decont/FacturaIesire/NotaTransfer 2, IesireImobilizare 1 |
| `DocumentDetaliu` | 12 | 82 de coloane brute, 33 de nume distincte |
| `Repartitor` | 5 | Partener 16, ContPropriu 2, Angajat 1, Gestiune/UnitateInterna 0; nume disjuncte |

TPT aici e un discriminator implementat ca tabele cu o singură coloană,
join-uite la fiecare citire polimorfă. Costurile documentate în
`p5-perf-masuratori.md`: `CoduriTip` 1,4 s pe o lună de import și incidentul
de 11 s (materializare polimorfă doar ca să se citească clasa CLR);
`RegistruTva` Server 805 ms (9 LEFT JOIN pe `DocumentDetalii`, 85-r6);
`CoduriTipPeTipuri` ~19 interogări per proiecție SAF-T (75-r2); fiecare
interogare pe bază din motor (dependenți, copii, storno, drafturi în perioadă,
documente stinse) plătește 20 de LEFT JOIN-uri + CASE. Docs EF Core: „TPT
shows inferior performance when compared to TPH in many cases"; sub TPH toate
formele de interogare stau pe o tabelă, cu index pe discriminator. XAF EF Core
n-are nicio restricție pe TPH (restricțiile lui: chei compuse, keyless, owned).

Ce cumpără felia, declarat:

1. **Tipul devine dată pe rând**: discriminatorul = numele clasei CLR = exact
   ancora `TipDocument.ClrType` (decizia 20). Cele zece locuri care azi
   afirmă „sub TPT nu există discriminator" și materializează entități ca să
   afle clasa (`ApiProiectii.CoduriTip`, `SaftProiectii.CoduriTipPeTipuri`,
   `PerioadaService.DraftInPerioada`, `CorectieController.RefuzPeTipulConcret`,
   `BackfillTva/Reconciliere`) citesc o coloană.
2. **Taxa de join dispare** de pe citirile pe bază, deci contractele IM-D4
   „Relații" și „Politici" se vor scrie și măsura pe maparea finală, fără
   ocoluri care ar deveni canonice.
3. **Greenfield asumat**: bazele existente se aruncă; nicio migrare de date.

Ce NU e motivul: schimbarea ierarhiei CLR (identică), a hook-urilor, a
`[TipDetaliu]`, a dispecerizării polimorfe sau a invarianților.

## Testul contra invarianților

- **II** (motorul nu cunoaște frunzele): neatins. Discriminatorul e o
  ETICHETĂ citită ca dată, nu un comutator de comportament: niciun `switch`
  pe valoarea lui în `Motor/*`; rezoluția conexului rămâne prin
  `TipDocument.ClrType` (IM-D9). Coloanele partajate fizic pe tabela bazei NU
  devin contract al bazei: decizia 54c rămâne literal („motorul are nevoie
  de valoare, nu de coloană"); o coloană de frunză se citește DOAR prin tipul
  frunzei (`as`/`OfType`/`GetObjectsQuery<Frunza>`), niciodată prin
  entitatea de bază și niciodată în SQL brut fără filtru pe discriminator.
- **III** (registrele = adevărul agregării): întărit — „nicio interogare
  polimorfă pe `Document` în fluxuri calde" nu mai are nevoie de excepția
  „mulțime mărginită" a lui `CoduriTip`.
- **IV** (politica = date): neatins; `TipDocument` rămâne ancora FK a
  politicilor, seed-ul îl oglindește 1:1 cu clasele.
- 20 (un singur nomenclator de tipuri, și e codul): întărit — valoarea
  discriminatorului e derivată din cod (numele clasei), nu un al doilea
  nomenclator; ModelCheck o probează contra seed-ului.
- 23a (migrațiile EF = canonice): păstrat; lanțul se RESETEAZĂ (F28-D5),
  nu se ocolește.
- 54b/54c (dimensiunile inline, pe frunză): păstrate; NULL-urile pe Postgres
  sunt bitmap, argument deja folosit de 54b.

## Deciziile

### F28-D1 — TPH pe toate trei ierarhiile, discriminator `ClrType`

`Document`, `DocumentDetaliu`, `Repartitor` trec pe `UseTphMappingStrategy()`.
Discriminatorul este o **proprietate CLR MAPATĂ** pe fiecare bază —
`public virtual string ClrType { get; protected set; }` — declarată prin
`HasDiscriminator(x => x.ClrType)`, cu valorile IMPLICITE ale EF (numele
scurt al clasei CLR), nicio `HasValue` scrisă de mână. Motivul (owner,
2026-09-18): tipul trebuie să fie vizibil în grile (membru mapat, deci
compatibil cu `Server`/`ServerView`), filtrabil prin OData și criterii XAF,
folosibil în `Appearance`, rapoarte și permisiuni pe obiect, și să fie
punctul de extensie pentru ce e „per tip" (semnături, rapoarte asociate).
Umbra ar fi cumpărat doar protecție la scriere greșită din cod; setter-ul
protejat + generatorul de discriminator al EF + proba (f) din F28-D7 o
înlocuiesc. Curatoria XAF: `[ModelDefault("AllowEdit","False")]`, caption
„Tip", ascuns din DetailView prin layout-ul autoritar, coloană în ListView-urile
de bază (documentele, lookup-ul de `Repartitor` al postării explicite).
`ClrType` intră explicit în `ExcluseDocument`/`ExcluseLinie` ale
`CorectieService` (nu mai e umbră, deci `IsShadowProperty()` n-o mai sare).

Pe `Document`, coloana este și **FK spre cheia alternativă
`TipDocument.ClrType`** (unică deja, `AplicaUnicitatiPolitici`), cu navigația
`Document.TipDocument` (`OnDelete Restrict`): baza garantează că orice rând
are ancoră în seed, `CoduriTip` devine un join, iar extensiile per tip se
agață de `TipDocument`, tabela de politică existentă (IV: politica = date).
Ordinea seed → documente e deja respectată de toate căile (Blazor
`--updateDatabase`, ModelCheck, Import1C). **Presupunere de verificat la
pasul 0**: EF acceptă ca proprietatea de discriminator să participe într-un
FK; dacă refuză, fallback declarat = fără constraint FK, navigația se
înlocuiește cu cititorul din F28-D6, iar proba (a) ține garanția.
`Repartitor` n-are nomenclator de tipuri: acolo `ClrType` rămâne string mapat.

Nivelul abstract `DocumentTrezorerie` rămâne declarat explicit în model
(`Trezorerie.cs` construiește `Expression.Parameter` pe el). `Document` e
abstract, deci fără valoare; `DocumentDetaliu` e concret și are valoarea lui
(liniile de tip bază există în date). Index pe `ClrType` pe fiecare din cele
trei tabele (EF nu-l creează singur; FK-ul spre `TipDocument` nu-l
înlocuiește); pe `DocumentDetalii` compus `(ClrType, DocumentId)` se decide
la pasul 0 pe EXPLAIN, nu pe intuiție.

### F28-D2 — Coloane partajate, fără prefix de tip, generic

Preferință declarată a owner-ului: atenție la `WHERE`, nu la n coloane.
Proprietățile cu același nume pe frunze-surori se mapează pe **aceeași
coloană**, numită ca proprietatea, prin `HasColumnName` aplicat GENERIC în
`OnModelCreating` (o buclă în stilul `AplicaScaraNumerica`: pentru fiecare
ierarhie TPH, pentru fiecare proprietate DECLARATĂ pe un tip derivat, numele
coloanei = numele proprietății). Niciun prefix `Tip_Prop` nu are voie să
existe în schemă (probă ModelCheck pe `IDesignTimeModel`: pentru fiecare
proprietate a ierarhiilor, `GetColumnName() == Name`).

Gardian la construirea modelului (ca cel al scării): două proprietăți cu
același nume pe frunze-surori dar cu tip CLR sau facete diferite (lungime,
scară) = excepție zgomotoasă, nu coloană dublată. Pe schema de azi
coliziunile sunt toate omonime cu tip identic (24 de nume pe `DocumentDetaliu`
— dimensiunile, `ContDebit/Credit`, `RepartitorDebit/Credit`, `ProdusId`,
`PretUnitar`, `LotFabricatie`, `DataExpirare`, `Descriere`, `Directie`, `Fel`,
`ImobilizareId`, `LinieSursaId`, `ValoareFiscala`…; 4 pe `Document` —
`DataPV`, `NumarPV`, `DataScadenta`, `CodCpv`; 0 pe `Repartitor`).

FK-urile și indexurile pe coloane partajate: EF trebuie să producă UN singur
constraint și UN singur index per coloană + principal (verificat la pasul 0;
dacă EF le dublează sau refuză, se configurează explicit numele constraint-ului
și al indexului, tot generic, în aceeași buclă — nu de mână per proprietate).

Capcana din docs EF (cast fără filtru pe discriminator citește valoarea
fratelui) devine REGULĂ: `(x as Frunza).Prop` în LINQ e sigur (EF emite
`CASE WHEN ClrType IN (…)`); SQL-ul brut care citește o coloană de frunză de
pe `Documente`/`DocumentDetalii`/`Repartitori` filtrează pe `ClrType` sau nu
există. Pe schema de azi nu există SQL brut pe coloane de frunză (verificat).

### F28-D3 — Nullabilitatea frunzelor: în CLR, nu în CHECK

Coloanele frunzelor devin nullable în tabelă. Nu se generează CHECK-uri per
discriminator pentru ele: valorile de tip valoare (`int`, `decimal`, `bool`,
`Guid`) nu pot ajunge NULL din CLR, iar obligativitățile de domeniu stau deja
în `GardianEditare`/validările XAF/motor (33d). `Cod`/`Denumire` pe
`Repartitor` rămân NOT NULL + CHECK, fiindcă sunt declarate pe BAZĂ
(`AplicaColoanaCautare`, 77-r2) — mecanismul e neafectat (verificat pe cod:
filtrul `DeclaringType` sare frunzele, numele constraint-ului vine din tabela
bazei, `CK_Repartitori_Cod_negol`).

### F28-D4 — Tipul țintei unui FK spre frunză: gardian generic

Opt FK-uri țintesc azi TABELE de frunză și pierd garanția de tip din bază:
`DviFactura.FacturaId → FacturaIntrare`, `DviFactura.DviId → Dvi`,
`DocumentTrezorerie.LaturaPerecheId → DocumentTrezorerie`,
`Lot.GestiuneId → Gestiune` (required + cascade), `Imobilizare.ResponsabilId
→ Angajat`, `Societate.ContBancarId → ContPropriu`,
`FacturaIesire.GestiuneDescarcareId → Gestiune`,
`FacturaIntrare.PlataContPropriuId → ContPropriu`. Regulă nouă în
`GardianEditare`, GENERICĂ prin metadata EF (nu listă de opt): pentru fiecare
obiect nou/modificat, pentru fiecare FK al cărui tip principal e un tip
NE-rădăcină al unei ierarhii TPH, ținta trebuie să existe cu discriminatorul
tipului (sau al unui subtip) — altfel refuz 422 cu mesaj de domeniu. Probă
ModelCheck pe ambele profiluri: un `Lot` îndreptat spre id-ul unui `Partener`
e refuzat; ușa de sistem (Import1C/seed) rămâne pe contractul ei de azi (nu
trece prin gardian, ca și celelalte reguli).

Comportamentele `OnDelete` rămân cele de azi (FK-urile de moștenire
frunză→bază cu CASCADE dispar de la sine; `Purja` rămâne corectă fiindcă
lucrează pe `GetRootType()`).

### F28-D5 — Lanțul de migrații se resetează; bazele se recreează

Cele 45 de migrații se ȘTERG și se generează o singură `InitialCreate` (data
zilei) din modelul TPH. Niciun cod și niciun tool nu referă un nume de
migrație (verificat); trasabilitatea din docs rămâne text istoric, cu o notă
în decizia 089 („migrațiile de dinainte de 2026-09-18 sunt istorie în git,
nu în lanț"). Excepția de marcat: `p5-felia27-pas0-spike.md:107` citează
`InitialCreate.Designer.cs:50-54` pentru `OptimisticLockField` — trimiterea
se rescrie pe snapshot-ul nou.

Bazele: `Atlas.Conta.BackOffice` (bugetar) și `Atlas.Conta.BackOffice.Privat`
se șterg și se recreează prin `dotnet ef database update --context
BackOfficeEFCoreDbContext [--connection …]` + seed prin Blazor
`--updateDatabase --forceUpdate --silent` (23a); `Atlas.Conta.ModelCheck.Privat`
se șterge (ModelCheck o recreează); `Atlas.Conta.Import1C.Flax` prin
`--recreeaza`; `Atlas.Conta.Import1C.Flax.Api` se șterge și se recreează după
importul integral (clonă). Niciodată `--no-build` la `dotnet ef`.

### F28-D6 — Tipul ca dată: un singur cititor, consumatorii mutați

Un singur helper (în `ApiProiectii` sau fișier propriu în `Api/`) răspunde
„ce tip au documentele cu id-urile astea" printr-o PROIECȚIE cu join pe
navigația din F28-D1 (`Select(d => new { d.ID, d.ClrType, Cod =
d.TipDocument.Cod })`; fără FK, `Select(d => new { d.ID, d.ClrType })` +
maparea `ClrType → TipDocument.Cod` memoizată per apel), cu aceeași
semantică de `null` pentru id inexistent/invizibil. Consumatori mutați în
felie:

- `ApiProiectii.CoduriTip`/`CodTip` (toți apelanții rămân pe semnătură);
- `SaftProiectii.CoduriTipPeTipuri` + `IdsDocumenteDeTip` DISPAR (75-r2 se
  închide): §7 și S3 primesc dicționarul din același cititor, pe mulțimea
  cerută, într-o singură interogare;
- `PerioadaService.DraftInPerioada` proiectează `ID`, `Numar`,
  `DataInregistrare`, `ClrType` (fără materializare polimorfă la fiecare
  `verifica`);
- `CorectieController.RefuzPeTipulConcret` citește tipul din proiecție și
  rezolvă `Type` prin `TipDocument.ClrType → assembly` o dată (cache static);
- `BackfillTva/Reconciliere` (două locuri) și fallback-ul din
  `OperareApi.Eticheta` (azi întoarce „…Proxy") folosesc cititorul;
- `ImperechereService.AsignatFataDe` RĂMÂNE pe materializare (are nevoie de
  instanță pentru hook-ul `SensDeStins`) — doar join-urile dispar.

Cele cinci copii ale dezproxării (`ClasaReala`, `TipReal`,
`VerificaCodDenumire`, `TipDomeniu`, inline în `CoduriTip`/`Reconciliere`) se
reduc la `MotorOperare.ClasaReala` + `Refuzuri.TipReal` (cele două semantici
existente); nu se rescriu cele care nu se ating.

NU intră: generic-ul `TOpus` din `CandidatiPereche` și uniunea per tip din
`ImperecheriProiectii` (F27-r16 rămâne; jumătatea „contrapartida per tip" e
semantică, nu mapare) — se notează ca restanță `F28-r1`.

### F28-D7 — Probele și comentariile

ModelCheck (ambele profiluri) primește probele `F28-*`: (a) pentru fiecare
`TipDocument` din seed, valoarea discriminatorului tipului CLR mapat ==
`ClrType` (și invers: fiecare tip concret mapat are ancoră); (b) niciun nume
de coloană cu prefix de tip pe cele trei tabele, `GetColumnName() == Name`;
(c) indexul pe `ClrType` există pe cele trei tabele; (d) refuzul din F28-D4;
(e) cititorul din F28-D6 dă aceleași coduri ca `ClasaReala` pe o mulțime de
documente de toate tipurile (anti-regresie pe semantică); (f) `ClrType` nu
se poate scrie din cod de aplicație (setter protejat) și e completat de EF la
`Add` pe fiecare tip concret, iar pe `Document` un rând cu `ClrType` fără
ancoră în `TipDocument` e refuzat de bază (FK) — dacă FK-ul cade la pasul 0,
proba (a) rămâne singura garanție și se spune explicit; (g) `ClrType` apare
ca membru persistent în modelul XAF (`IsPersistent`), read-only, deci trece
precondițiile D85-M2 pe `ServerView`. Probele existente
care depind de forma TPT se rescriu pe intenție, nu pe text: `Program.cs:2585`
(`sqlNtc.Contains("InchideriTva")` → conține filtrul pe `ClrType`),
`Program.cs:21253` (`Tabel == "Parteneri"` — de verificat sursa etichetei),
titlurile care spun „sub TPT" (#8, #10, #12). Cifrele de probe la deschidere:
bugetar 1278 / privat 1434 — nu scad.

Comentariile din cod: cele 67 de ocurențe „TPT" se tratează pe regula
codului slim (CLAUDE.md): cele care afirmă lucruri FALSE sub TPH (lista din
raport: „nu există discriminator" ×10, „cast-ul devine LEFT JOIN" ×10,
„tabela derivată n-are GCRecord", `Purja.cs:31-36`, `Catalog.cs:98`,
`ApiProiectii.cs:43-50`, `ImperecheriProiectii.cs:12-17`,
`TrezorerieApply.cs:453-481`, `BackOfficeDbContext.cs:80/104/208/579/684/697`,
`Cautare.cs:160-179`) se TAIE sau se reduc la o linie cu trimiterea `// 89`;
restul nu se curăță în masă.

### F28-D8 — Regula de oprire a feliei

Felia e închisă când, pe codul final:

- ModelCheck verde pe AMBELE profiluri (bugetar ≥ 1278, privat ≥ 1434 + probele
  `F28-*`), `has-pending-model-changes` curat, `--dump-metadata` idempotent;
- Import1C integral pe `Atlas.Conta.Import1C.Flax` cu `--recreeaza --cititori
  --inchide-lunile` (+ `--reclasifica` după), exit 0, raportul de reconciliere
  IDENTIC pe conținut sortat cu `nou/tools/Import1C/reconciliere-20260917-121343.txt`
  (diferă doar antetul cu data), 12/12 luni închise cu 0 constatări,
  `SolduriService.Reconstruieste` 0 diferențe;
- `refuzuri.ps1` 285/285 pe host viu Privat (după re-seed pentru
  `Cititor`/`Configurator`);
- `pnpm build` și `verifica:drift` verzi cu hosturile oprite; ZERO schimbare
  de sârmă în DTO-uri (openapi/api-types fără drift);
- perf A/B pe ACEEAȘI bază de conținut (Flax reconstruită de Import1C, cu
  lunile închise, ca la F27 8a): cele șapte cifre ale feliei 27 (fișa `4111`
  122 ms, balanța analitică 210, `sold-parteneri` 219, `sold-stoc` 50,
  sintetica 58, operarea cu 49 de linii 394, `documente-cu-rest` 181) plus
  `CoduriTip` pe extrasul cu 335 de stingeri (185 ms), `RegistruTva` Server
  (805 ms), SAF-T D406 pe o lună (2,9–3,0 s): niciuna mai proastă decât
  baseline-ul peste zgomot; cele așteptate mai bune se raportează cu cifra,
  fără promisiune. O cifră mai proastă nu blochează felia dacă are cauză
  demonstrată și restanță cu nume, dar se raportează, nu se ascunde;
- IM-D10 amendat în `p5-felia-izolare-motor-contract.md`: „ZERO schimbare de
  schemă" devine „schimbările de schemă intră doar prin decizie proprie,
  reanalizată la momentul ei" (regula owner-ului: orice decizie de
  arhitectură apărută pe parcurs se reanalizează, nu se interzice în avans);
  cifrele de ModelCheck din IM-D10 se actualizează la cele de la închiderea
  feliei 28.

## Pașii (un agent per pas; main verifică independent și comite)

0. **Spike-ul de mapare** (Opus): pe branch-ul `p5-f28-tph`, cele trei
   `UseTphMappingStrategy()` + discriminatorul `ClrType` + bucla generică de
   `HasColumnName` (F28-D2) + gardianul de coliziune + indexurile pe
   `ClrType`; ștergerea celor 45 de migrații și `dotnet ef migrations add
   InitialCreate --context BackOfficeEFCoreDbContext`; build-ul soluției.
   Livrabil: nota `docs/api/p5-felia28-pas0-spike.md` cu: numărul de coloane
   rezultat pe cele trei tabele (așteptat ~37 / ~45 / ~27), lista
   constraint-urilor și indexurilor pe coloanele partajate (dublate sau nu),
   orice plângere EF și cum s-a rezolvat generic, `EXPLAIN` pe interogarea
   `RegistruTva` Server și pe `DraftInPerioada` pentru decizia indexului
   compus; verdictul pe FK-ul discriminator → `TipDocument.ClrType` (F28-D1:
   acceptat de EF sau fallback-ul declarat, cu mesajul EF citat). **Oprire**:
   dacă EF cere configurare de mână per proprietate pentru coloanele
   partajate, sau dacă vreo coliziune de nume are tipuri diferite, se oprește
   și raportează — nu se normalizează tăcut.
1. **Bazele și ModelCheck** (Opus): F28-D5 pe toate bazele de dev; ModelCheck
   pe ambele profiluri; rescrierea probelor dependente de TPT (F28-D7) și
   probele noi (a)(b)(c). Oprire: orice probă existentă care pică din alt
   motiv decât forma TPT se raportează, nu se „adaptează".
2. **Tipul ca dată + gardianul FK** (Opus): F28-D6 și F28-D4, probele (d)(e),
   comentariile false tăiate (F28-D7). Oprire: dacă un consumator are nevoie
   de instanță (hook polimorf) și nu de tip, rămâne pe materializare și se
   raportează; nu se introduce niciun `switch` pe `ClrType` în `Motor/*`.
3. **Probele supreme** (Opus, proces detașat + monitor, nu task de fundal,
   50d): Import1C integral cu `--recreeaza --cititori --inchide-lunile`,
   diff pe conținut sortat cu baseline-ul; `Reconstruieste`; recrearea
   `Flax.Api`; `refuzuri.ps1` pe host viu Privat; `verifica:drift`; perf A/B
   (F28-D8), cu EXPLAIN pe cele trei cifre atribuite TPT. Oprire: raport de
   reconciliere diferit = STOP, se investighează cauza, nu se re-baseline-ază.
4. **Review advers + docs** (agent separat pentru review, pe scenarii
   concrete: cast pe frunză fără filtru în SQL brut; FK spre frunză cu id de
   alt tip prin API și prin OData; `GetObjectsQuery<NotaContabila>` pe un ITV;
   coloană partajată citită prin bază; discriminator fără ancoră în seed;
   corecția unui document cu linii de tip bază; `Purja` pe ierarhii; securitate
   pe tip cu `GetObjectByKey<Document>`), fix-urile aplicate de main; apoi:
   decizia `089-tph-discriminator-tipul-ca-data.md` (antet + §Regula durabilă
   cu sub-punctele (a)–(h) din D1–D8 + textul integral), `README.md`
   (rândul 089; 3 și 16 → „amendată de 89"), antetele lui 003 și 016,
   `restante.md` (75-r2 închisă; 81-r4 adnotată — spațiul de coduri e acum
   fizic aceeași tabelă, unicitatea rămâne blocată de coliziunile din import;
   85-r6 adnotată — cauza de join dispare, coloanele incluse rămân; `F28-r1`),
   `istoric-plan-de-lucru.md` (bulletul feliei + nota la linia 17),
   `stare-curenta` (domeniu-si-operare :12-14 și :149-150, api-si-client
   :233-238, dezvoltare-si-validare: lanțul resetat, bazele, proba supremă
   neschimbată), `p5-perf-masuratori.md` (addendum F28 cu A/B),
   `p5-felia-izolare-motor-contract.md` (IM-D10 amendat, cifrele),
   `invarianti.md` (nota de reconciliere la II, în stilul celei de la 54:
   discriminatorul e etichetă, nu comutator), CLAUDE.md §Stare/§Următorul
   pas (izolarea motorului rămâne următorul pas, acum pe maparea finală).

## Ce NU intră (amânări cu nume)

- **F28-r1** simplificarea `CandidatiPereche<T, TOpus>` și a uniunii per tip
  din `ImperecheriProiectii` (jumătatea „Tip" e rezolvată de discriminator,
  jumătatea „contrapartida per tip" nu) — se rezolvă împreună cu F27-r16.
- **F28-r2** index unic pe `Repartitor.Cod` (81-r4): mecanismul devine
  trivial, dar datele de import au coliziuni legitime între familii.
- **F28-r3** partiționarea/`CLUSTER` pe `ClrType` — fără cifră care s-o ceară.
- Izolarea motorului (IM) rămâne felia următoare, pe contractul ei.
