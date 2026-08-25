# Decizia 72 — Pasul 5, felia 15 — identitatea extinsă a partenerului + sincronizarea ANAF

- **Data**: 2026-08-26
- **Stare**: activă (amendează 71b — adresa e câmp plat pe `Partener`, nu satelit; 34g rămâne deschis doar pentru IBAN/delegați/contact/valute)
- **Rezumat durabil**: `CLAUDE.md` §72
- **Docs**: docs/api/p5-felia-partener-anaf-contract.md (D15-D1…D6, riscurile pin-uite, §Închidere — V4 măsurat), docs/import/faza-1c-design.md §13 (cifrele V5)

---

**Pasul 5, felia 15 — partener + ANAF — executată** (contract:
`docs/api/p5-felia-partener-anaf-contract.md`; 4 pași + închidere, un agent
per pas; review advers cu 3 constatări de fixat înainte de închidere — toate
fixate — și 10 restanțe cu nume). Prima felie care îmbogățește nomenclatorul
de parteneri dincolo de cele 4 câmpuri fiscale ale D394 (71b): adresa
structurată în forma cerută de SAF-T, nomenclatorul de județe și registrul
ANAF (serviciul v9 `PlatitorTva`) ca sursă canonică pentru statutul de TVA,
la cerere. Motivul: SAF-T (D406) cere adresa partenerului cu `Region`
codificat, iar D394 a arătat că eticheta „înregistrat în scopuri de TVA" din
1C minte pe sute de parteneri (71h) — registrul ANAF e singurul care știe.

(a) **Adresa = câmpuri PLATE pe frunza `Partener`, nu satelit.** `Strada`,
`Numar`, `DetaliiAdresa` (bloc/scară/etaj/ap. + ce nu are coloană),
`Localitate`, `CodPostal`, `JudetId?`; `MaxLength` = lungimile SAF-T
(`City` = 35, `Street` = 70 etc.) și sunt SINGURA sursă a lungimilor — se
citesc prin reflecție (`SincronizareAnafService.Lungimi`), în serviciu ȘI în
Import1C (R6 al review-ului: o singură listă). Amendează 71b: satelitul 34g
nu se deschide pentru adresă (o adresă per partener e suficientă azi;
adresele multiple, IBAN-ul, delegații, contactul rămân 34g). Plus două câmpuri
server-owned: `DataSincronizareAnaf` (timbrul: „registrul ANAF a vorbit la
data X") și `InactivFiscal` (F2 al review-ului: e fapt exclusiv al
registrului; scris pe ușa secured = proveniență fabricată ⇒ gardianul îl
refuză ca pe timbru).

(b) **`Judet` = nomenclator de nucleu, `ForbidCRUD`, seed autoritar** din
`Comun/JudeteRo` (42 de rânduri: cod ISO 3166-2 `RO-XX`, cod auto, codul din
CNP, denumirea normalizată; `DupaCodCnp`, `DupaCodAuto`, `DupaDenumire`);
pe AMBELE profiluri, `VerificaProfil` neatins. Gardian: **județ doar pe
`Tara == RO`** — pe ușa secured (`GardianEditare`, navigație + FK), în
serviciul ANAF și în Import1C (F1 al review-ului: conectorul scria județ pe
partener străin; acum ⇒ denumirea brută în `DetaliiAdresa` + contor
`JudetPeTaraStraina`; schimbarea țării pe un partener cu județ golește
județul). Grafiile 1C („Sector 3", „Jud. Timiș") se normalizează în CONECTOR,
nu în `JudeteRo` (`DupaDenumire("Sector 3") == null` rămâne probat).

(c) **Clientul `PlatitorTva` v9 = o clasă în Module, fără DI** (`HttpClient`
injectat de host; `AddHttpClient<PlatitorTvaClient>` în WebApi/Blazor):
loturi de ≤100 CUI, 1 apel/s ÎNTRE loturile aceleiași interogări,
deserializare tolerantă (câmpuri necunoscute ignorate, `cod`/`message`
opționale — V3 cu fixture real), erori per lot clasificate
tranzitorie (429/5xx/timeout) vs fatală. `CuiInterogabil`: doar cifrele
(„RO 123 45" ⇒ 12345), 2–10 cifre, `Fizica` cu 13 cifre = CNP ⇒ necandidat,
`Tara ≠ RO` ⇒ necandidat.

(d) **Regula de merge — „gol se umple, diferit se raportează, canonicul
bate"** (`SincronizareAnafService.Aplica`, pură, fără HTTP — V2): pe axa TVA
(`InregistratTva`, `TvaLaIncasare`, `InactivFiscal`) ANAF e CANONIC și scrie
întotdeauna; pe adresă + `Denumire` + `RegistruComert` merge-ul e PER CÂMP:
câmp gol ⇒ se umple, valoare diferită ⇒ `Diferenta` raportată, obiectul
neatins; `suprascrie = true` (explicit, doar pe REST) ⇒ scris, cu
`Modificare(camp, vechi, nou)` — rezultatul e reversibil din listă.
`notFound` ⇒ nicio modificare, FĂRĂ timbru, avertisment. Trunchierea la
`MaxLength` cu avertisment (pe date reale: 140 de localități „Sat X Com. Y"
peste 35 — legitim, câmpul e mapat corect). `CodPostal` cade pe
`date_generale.codPostal` când adresa aleasă n-are cod (R5 — adaos față de
tabelul D3, pe date reale).

(e) **REST = comandă pe partener, gate propriu, ușa non-secured.** `POST
api/parteneri/{id}/sincronizeaza-anaf` și lotul (≤500, `Sarite` per id cu
motiv); `ComandaAutorizata<T>` + `AutorizeazaLot<T>` generalizează gate-ul
de autorizare al documentelor (55b) FĂRĂ a-i schimba semantica pe `Document`
(overload-ul delegă la generic); gate-ul rulează ÎNAINTE de orice OS
non-secured. O singură traducere a refuzurilor: domeniu ⇒ 422, tranzitoriu
ANAF ⇒ 503. **V4 măsurat pe calea reală** (clona Flax.Api, WebApi pornit):
`User` ⇒ **404** (n-are nici Read pe `Partener` ⇒ ramura „invizibil" a
gate-ului; 403 e al lui Read-fără-Write, care nu există pe clonă), `Admin` ⇒
200 în 172 ms cu apel REAL la ANAF, partener străin ⇒ 422, lot 501 ⇒ 400,
PATCH OData pe `DataSincronizareAnaf`/`InactivFiscal` ⇒ 400 (mesajul
gardianului), PATCH pe `Localitate` ⇒ 204 (controlul). Cifrele în
§Închidere al contractului.

(f) **XAF: acțiunea „Sincronizează din ANAF" pe `Partener`**, rulează pe ușa
non-secured (58c) cu gate `CanWrite` per partener înainte; fără `suprascrie`
(doar REST); erorile ⇒ `UserFriendlyException`; `ObjectSpace.Refresh()`
după. Sincron, ~5 s la 500 selectați — asumat (R7: fără smoke vizual).

(g) **Import1C: adresele din 1C pe BLOC gol, apoi ANAF ca pas final
opțional.** `FlaxDb.AdresaPartener/AdreseParteneri` pe
`flax.InfoRg_InformatiaDeContact` cu EXACT filtrul/ordinea din
`vwDetaliiPartener` (sediu social înaintea punctului de lucru), fără
excluderea PF; proba în `--cititori` (53 cititoare). `AplicaAdresa` scrie
DOAR când toate cele 6 câmpuri sunt goale (o adresă culeasă sau ANAF e mai
bună decât 1C); județ: codul din CNP întâi, denumirea apoi, nerezolvat ⇒
denumirea brută în `DetaliiAdresa` (nicio ghicire); contoare
`AdresePreluate/FaraAdresaInSursa/AdreseDejaCompletate/JudetDinCodCnp/
JudetDinDenumire/JudetNerezolvat/JudetPeTaraStraina/AdreseTrunchiate`.
`--anaf` (și `--reclasifica --anaf`) = `Anaf1C` peste ACELAȘI serviciu
(tranșe de 1.000 peste loturile clientului, commit per partener, o reluare la
tranzitoriu — erorile vindecate nu se numără ca erori, R8), raport per cauză
+ distribuția D394 înainte/după, prag 5 % loturi fatale. **Canonicul ANAF nu
e răsturnat de reclasificare**: `AplicaClasificare` și semnalul din registru
(71h) sar axa TVA pe partenerii cu timbru (`PastratiDinAnaf`,
`RegistruContraAnaf`) — fără gardul ăsta orice `--reclasifica` ulterior
de-înregistra înapoi ce ANAF corectase. **V5 pe Flax** (`--recreeaza`, 3h38):
contract îndeplinit, 0 FAIL, 20.038/20.118 adrese preluate (99,6 %), județ
4.013 din CNP / 16.017 din denumire / 7 nerezolvate, 65 trunchieri;
**reconcilierea IDENTICĂ cu baseline-ul** (444 linii / 39.321 B, diff sortat
0); `--anaf` (6:42): 8.232 candidați, 8.230 găsiți / 2 negăsiți / 0 erori,
3.365 modificări (TvaLaIncasare 1.371, InregistratTva 444, InactivFiscal 52),
25.655 diferențe raportate, **190 parteneri tip 4 → tip 1**; a doua
`--reclasifica` = idempotentă (0 preluate / 20.038 neatinse). D394 09/2025 pe
clonă înainte → după: 2.601 op1 în ambele, 62 CUI-uri și-au schimbat cartușul
— toate = statutul ANAF de azi (47 PF-cu-CUI înregistrate 2 → 1; 15 marcate PF
în 1C dar neînregistrate azi 1 → 2, dintre care 2 achiziții cu TVA ies ca
`CombinatieRefuzata` = exact D4-r1; 1 A → AI prin TVA la încasare); Σ pe
formular identică. Sunt corecții, nu regresii (34f, 71h).

(h) **Review advers** (read-only, pe diff-ul întreg): 0 blocante; F1 (județ
pe partener străin în Import1C), F2 (`InactivFiscal` scriibil pe secured),
F3 (V4 nescris) — fixate + R6/R8 luate ieftin, într-un singur commit;
riscurile pin-uite 1–8 din contract acoperite cu probă (V1/V2/V3 în
ModelCheck, V4 HTTP, V5 Import1C).

(i) **Restanțe cu nume** (textul aici, numele în CLAUDE.md):
- **72-r1** rate-limit cross-request: 1 apel/s e garantat doar în interiorul
  unei interogări; N cereri REST/XAF în rafală = N apeluri/s (riscul 5,
  acceptat single-operator 25f); fix ieftin viitor = gate static process-wide
  în client (SemaphoreSlim + timestamp), per host.
- **72-r2** adresa hibridă fără proveniență per câmp: 1C umple blocul, ANAF
  umple per câmp golurile ⇒ pot coexista strada ANAF + localitatea 1C
  (frecvent pe Flax: 7.613 diferențe de localitate); consecință a D3 + D6, nu
  bug; opțiune: proveniența blocului + `suprascrie` pe adresă când sursa e 1C.
- **72-r3** `IntrariStricate` ⇒ eroare FATALĂ pe lot deși lotul a fost
  aplicat și comis (pe REST single ⇒ 422 după scriere; mesaj înșelător, nu
  pierdere).
- **72-r4** `StareInregistrare` (radiat) e parsat dar neconsumat: CUI radiat
  cu `scpTVA = false` ⇒ de-înregistrat tăcut (canonic corect), fără
  avertisment de radiere.
- **72-r5** XAF fără smoke vizual (mesaj multi-linie în toast, blocarea
  sincronă la lot mare).
- **72-r6** `RegistruContraAnaf` = 0 pe rularea combinată prin ordine
  (semnalul din registru rulează înaintea timbrului); devine real la a doua
  rulare — e D4-r1 („statutul la data documentului").
- **72-r7** ordinea nedeterministă la egalitate de contor în raportul de
  reconciliere (două linii permutate; cosmetic, diff-ul se face sortat).
- **72-r8** `CuiInterogabil("00")` ⇒ 0 trimis ca CUI (2 cifre trec filtrul).
- **72-r9** ecranul React de partener (`lista-react.md`).
- **72-r10** `User` ⇒ 404 vs 403: 403 pur cere un rol cu Read-fără-Write pe
  `Partener`, care nu există pe bazele de probă; gate-ul are ambele ramuri.

Ce NU a intrat (cu motiv, în contract): contact/IBAN/delegați/adrese
multiple (34g), nomenclator de țări cu denumiri (34g), istoricul statutului de
TVA (D4-r1), antetul societății proprii (D4-r10, felia SAF-T), job periodic
(infrastructură de host inexistentă), `RegistruComert`/CAEN/split/e-Factura
ca date canonice.
