# 77. Pasul 5, felia 20 — finisajul clientului: căutarea fără diacritice, cache-ul de nomenclator, confirmările, `Neincluse` agregat, primele ecrane de nomenclator

- **Data**: 2026-08-29
- **Stare**: activă (închide 70-r1, 72-r9, 73-r6, 73-r10, 74-r7, 74-r12 ca citire, 76-r6 integral, restanța F6 „placeholder pe linia existentă", datoria F12 „smoke vizual al jurnalelor"; amendează 56 doar prin excepția deja existentă a lui `Societate`)
- **Docs**: `docs/api/p5-felia-finisaj-client-contract.md` (F20-D1…D10 + §Închidere), `docs/api/lista-react.md` (curățată de itemii închiși)

## Context

După 76 toate tipurile de document aveau felie de scriere; ce rămăsese în
client erau **limitele mecanismelor comune** (semnalate de patru ori la rând în
`lista-react.md`, ultima dată la F19 — „vor reapărea pe orice felie următoare")
și **ecranele de nomenclator** cerute de feliile 15–17, fără niciun precedent în
client (nu exista niciun ecran de nomenclator, `http.ts` n-avea `PATCH`).

Inventarul de intrare a fost MĂSURAT înainte de contract (două explorări
read-only, client + server): colația `en_US.utf8` libc pe toate bazele, fără
`UseCollation`/extensii/coloană normalizată; `ODataStore` per instanță fără
cache; 11 `window.confirm` identice; `Neincluse` trimis întreg (plafonat în
client la 200); `BazeContabile` și `CoduriMiscare` liste `static readonly`
invizibile pe orice ușă; refuzurile gardianului pe OData ieșind `400 text/plain`.

## Tranșările

### (a) Căutarea fără diacritice = coloană GENERATĂ, nu colație (F20-D1)

Trei căi, două respinse cu motiv: colația ICU nedeterministă **rupe**
`LIKE`/`strpos` (exact ce traduce OData din `contains`) și ar schimba semantica
egalității pe indexurile unice existente; `unaccent(...)` nu poate fi injectat în
jurul coloanei fiindcă `$filter`-ul îl compune `ODataStore` și EF îl traduce
mecanic (și `unaccent` e STABLE, deci nici într-o coloană generată n-ar sta).
Aleasă: **`Cautare` = `translate(lower(coalesce(Cod,'') || ' ' ||
coalesce(Denumire,'')), De, La)` GENERATED ALWAYS STORED** — `lower` și
`translate` sunt IMMUTABLE (măsurat `pg_proc.provolatile = 'i'` pe 18.4);
`concat()` NU e, de-aia `||` + `coalesce`. O singură coloană acoperă căutarea pe
cod ȘI pe denumire.

Mecanica: `Comun/Cautare.cs` = tabelul `De`/`La` (grafiile RO cu virguliță ȘI
cu sedilă + un set restrâns de diacritice latine; doar minuscule — `lower`
rulează întâi; fără mapări 1→n, `translate` e caracter-la-caracter și un `La`
mai scurt ȘTERGE), `Normalizeaza` (C#), `ExpresieSql` (o singură sursă pentru
`OnModelCreating` ȘI migrație), gardian în static ctor (lungimi egale, fără
dubluri). `ICuCautare` declarată pe 14 nomenclatoare; configurarea e **o buclă
generică** pe model (`AplicaColoanaCautare`): coloana se așază pe tipul care
DECLARĂ proprietatea — sub TPT o singură coloană pe `Repartitori` pentru toate
frunzele; `Simbol` pe `Cont`, altfel `Cod`; `Denumire` obligatorie (altfel
eroare la construirea modelului). `Lot` NU intră (fără denumire), `RandD300` nici
(nu e în conducta de lookup-uri). Vizibilitatea în XAF: `VisibleIn*View(false)`,
nu `Browsable(false)` (ar fi scos-o din TypesInfo, deci din EDM); probat că nu
cade în grupul-mătură `Unplaced` al layout-ului declarat.

Oracolul din ModelCheck: pe FIECARE entitate `ICuCautare`, pentru TOATE
rândurile, `Cautare` citită din SQL == `Cautare.Compune(cod, denumire)` în C#
(9.343 rânduri bugetar / 2.951 privat) — descoperă entitățile cu aceeași regulă
ca `OnModelCreating`, deci un nomenclator nou intră automat în probă. Probat pe
HTTP: `contains(Cautare,'clienti')` ⇒ 419; din client „Clienți – Cred" cu
ț-virgulă găsește rândul scris cu ţ-sedilă. Migrația aplicată pe toate cele 5
baze de dev (inclusiv `Import1C.Flax`, după review F6). Tabelul se PUBLICĂ în
`metadata.json`, ca TS-ul să nu-l copieze.

### (b) Un singur store OData în client, cu `byKey` prin cache (F20-D2)

`nucleu/odata.ts`: `storeOData` = `ODataStore.inherit` (clasă DevExtreme, nu ES)
cu `_byKeyImpl` suprascris — nu `byKey`, ca `_withLock`/`_addFailHandlers` ale
bazei să rămână — prin `cache.fetchQuery` pe cheia
**`['nomenclator', entitate, id, proiecție]`** cu `staleTime: Infinity`. Al
patrulea segment (expand/select sortate) e al review-ului intern al pasului:
fără el un lot cerut fără `$expand=Produs` ar fi servit din cache unui lookup
care compune eticheta din `Produs.Denumire`. Implementarea de bază se capturează
o dată din prototip (`callBase` e legat doar pe durata apelului sincron, iar
răspunsul vine printr-o promisiune); `when(promisiune)` pentru Deferred
(`fromPromise` nu e în `.d.ts`). `QueryClient`-ul a ieșit din `main.tsx` în
`nucleu/cache.ts` (consumatori din afara arborelui React). `useNomenclator`/
`citesteNomenclator`/`useSonda` pe aceeași cheie; `sonda.ts` a murit.

Normalizarea căutării: DevExtreme generează `contains(tolower(Cautare),'x')`;
`beforeSend` scoate `tolower(` (coloana e deja lowercase) și trece literalul prin
ACELAȘI tabel — pentru `contains`/`startswith`/`endswith` (review F4), ancorat pe
numele coloanei, deci termenii pe alte câmpuri (`CodFiscal`) rămân neatinși.
`Lookup` caută DEFAULT pe `Cautare` când tipul o are în metadata — 64 de
`cauta=` explicite au dispărut, 12 au devenit `['Cautare', CodFiscal/Marca/Iban]`
(câmpuri pe care `Cautare` NU le acoperă), 7 pe `Lot` neatinse.

Măsurat: cifra „8 cereri" din inventarul F19 nu s-a reprodus — pe NTC sunt 2
lookup-uri de `UnitateInterna`, deci baseline-ul real e **2 → 1** la deschidere
și **+2 → 0** la al doilea document cu aceeași unitate. Restanța F6 („lookup-ul
pe linia existentă arată placeholder până la deschidere") **s-a închis de la
sine**: linia persistată arată eticheta imediat, 2 `byKey` la prima deschidere,
0 la a doua.

### (c) Precompletarea scrie perechea (id, etichetă) prin cache (F20-D3)

`nucleu/etichete.ts::precompleteazaTip` — un singur verdict „e gol?" luat o dată
din `linie`, aplicat pe AMBELE stări (azi `setLinie` avea update funcțional pe
`prev`, iar `setEtichete` citea `linie` din closure — puteau diverge). Sursa
etichetei = `citesteNomenclator('TipMaterial', id)`, NU `$expand` imbricat pe
`Lot` (costul ar fi căzut pe fiecare tastare într-un nomenclator mare);
`expand={['TipMaterial']}` a fost scos și de pe lookup-urile de `Produs` (plătit
pe fiecare pagină de căutare pentru o etichetă folosită o dată). Aplicat pe ASM,
LDI (plus și minus), RLF, RDC, BCS. **BTR n-a intrat**: n-are `laSelectie`,
n-are `EticheteCulese` — e singura felie care n-a adoptat 61b deloc; a o adăuga e
plumbing de felie, nu aplicarea helperului (restanță 77-r1).

### (d) `ConfirmareInline` + slot în `DocumentShell`; `window.confirm` a murit (F20-D4)

`nucleu/ConfirmareInline.tsx` `{intrebare, verb, ocupat?, onConfirma, onRenunta}`
pe `.cerere-data`; `DocumentShell` capătă `confirmare` + `inchideConfirmarea`
(regula „o singură cerere în așteptare" are nevoie de un drum în fiecare sens,
stările stând în locuri diferite). Zero `window.confirm` în cod (11 ștergeri +
`PanouStingeri` + distribuirea ASM); blocurile cu `NumberBox`/`DateBox` rămân.

**Review F1 (de fond)**: gardul `confirmare == null` era permanent fals — feliile
scriu natural `confirmare={deSters && <…/>}`, care dă `false`, iar
`false == null` e fals ⇒ **„Stornează" murise pe 11 din 13 ecrane**, invizibil
pentru `tsc` (`ReactNode` acceptă `false`). Fix: gardul pe falsy + `useEffect`
care ÎNCHIDE cererea de dată când se deschide o confirmare (nu doar o ascunde —
altfel reapărea armată cu data veche). Lecția: un slot de `ReactNode` nu se
compară cu `null`.

### (e) `Neincluse` agregat per cauză PE SERVER (F20-D5)

`SaftSumarDto.Neincluse` = `NeinclusAgregat[]` `{Cauza, Numar, Randuri, Suma,
Cantitate?, Exemple ≤ 20}` — funcție PURĂ pe lista plată în `Sumar(SaftDto)`;
`SaftDto.Neincluse` (fișierul) neatins. **`Suma`**: pe S valoarea SEMNATĂ (Σ ==
`NeincluseStocValoare`, termenul cusăturii S2 — un storno care anulează o gaură o
anulează și în sumar); pe L |Baza| pe linii fiscale, |Debit|+|Credit| pe solduri
de terți (acolo cusăturile au deja termenii semnați în `Rezumat`, iar suma
modulelor nu poate ieși 0 dintr-o compensare care ar ascunde cauza). Ordinea
`Numar` desc + `Cauza` ordinal; cusătura în ModelCheck pe L și S (Σ
Numar/Randuri/Suma/Cantitate == lista plată, aceleași cauze, exemplele primele,
round-trip JSON). `SaftDiferentaCont.ContId` (simbol normalizat → Guid din
dicționarul deja citit — zero interogări; `Guid.Empty` pe `ProductTypeImplicit`,
fără link) ⇒ S3 trimite la fișa contului pe `DataStart`/`DataEnd` ale sumarului;
probat: soldul din fișă == „Închidere balanță" din S3 pe 371. Clientul: plafonul
200 a murit, un rând per cauză cu exemplele în `<details>`.

### (f) `metadata.json` capătă secțiunea `Nomenclatoare` (F20-D6)

Listele legii care sunt COD (`BazeContabile` — acum descrise, lista de coduri a
gardianului se DERIVĂ din perechi; `CoduriMiscare`; tabelul `Cautare`) se
publică **declarat explicit** în `MetadataDump`, nu prin reflecție pe statice, și
NU devin entități (ar contrazice motivația din `SaftReguli`). Clientul le citește
cu `nomenclator(nume)`; driftul e prins de gardul existent al ModelCheck.

### (g) 70-r1 închisă: refuzurile de domeniu pe OData ies `422 EroriDto` (F20-D7)

Cauza: `UserFriendlyExceptionFilter` al DevExpress, global, transformă orice
`IUserFriendlyException` în `ContentResult` 400 text; pe REST nu se vedea fiindcă
`ContaApiController.Domeniu` prinde înaintea filtrelor. `RefuzDomeniuOdataFilter`
(WebApi, nu Module — 42a) cu `Order = int.MaxValue` = cel mai din interior,
primește excepția primul, `ExceptionHandled = true`; filtrul DevExpress rămâne
înregistrat pentru rutele care nu-s ale noastre. `ContentResult` serializat cu
`IOptions<JsonOptions>` ale aplicației (altfel ieșea `erori`, nu `Erori`).
Deliberat NEatinse: `IUserFriendlySecurityException`/`HttpUserFriendlyException`.
Probat: `PATCH DataSincronizareAnaf` ⇒ 422 JSON; `Localitate` ⇒ 204; al doilea
`Societate` ⇒ 422. **Review F3**: refuzul de PERMISIUNE pe scrierea OData rămâne
404/403 `text/plain` (vine din controllerul securizat XAF, nu ajunge la filtru);
clientul îl traduce în `http.ts` DOAR pe ușa `/api/odata/` într-un mesaj de
domeniu („nu aveți drept…") — pe REST statusurile rămân ale gate-ului (55b).

### (h) Primele ecrane de nomenclator (F20-D8)

`felii/nomenclatoare/`: `api.ts` (conducta OData de scriere: POST 201 + entitatea,
PATCH 204 fără `Prefer`/`If-Match`, DELETE 200; PATCH e DELTĂ — absența unui câmp
NU e golire, spre deosebire de PUT-ul documentelor, 56), `ListaNomenclator`
(căutare pe `Cautare` normalizată, în URL, + `cauta` suplimentar — `CodFiscal`
pe parteneri, review F5), `ShellNomenclator`. **Partener** (72-r9): județ activ
doar pe `Tara == RO` și golit la schimbarea țării, server-owned-urile readonly,
„Sincronizează din ANAF" cu `Modificari`/`Diferente`/`Avertismente` traduse prin
captions, `suprascrie` confirmat inline, 503 ⇒ `EroareIndisponibil` + „Reia".
**Societate** (73-r6): rând unic, `BazaContabila` din `nomenclator`, `ContBancar`
filtrat `EsteBanca`, `RaporteazaCnp` explicat. **Produs**: `UnitateMasura`
remote, `CodNc` `^\d{8}$` în client (format, nu fond). **`PoliticaMiscareSaft`**:
grilă DE CITIRE — regula 56 nu se redeschide; editarea din React = restanță cu
nume. Amendament la D8: lungimile SAF-T vin din schemele OData ale
`openapi.json` (`maxLength`), NU din dump — ipoteza „OData nu e în openapi" era
falsă, sursa aleasă e cea mai bună. `http.ts` capătă `modifica` (PATCH) și
`EroareIndisponibil` (503 = a treia specie: cererea era bună, reîncercarea are
sens).

### (i) Licența DevExtreme (F20-D9)

`config({licenseKey: import.meta.env.VITE_DEVEXTREME_LICENSE})` în `main.tsx`;
`.env.example` comis, `.env`/`.env.local` ignorate. Cheia e a utilizatorului.

### (j) Review advers și smoke

Review: 11 constatări — F1 (fond, storno mort), F2 (fond: cache-ul supraviețuia
„Ieșire" — logout SPA fără `cache.clear()`, utilizatorul B primea etichetele lui
A), F3 (permisiunea pe OData), F4 (`startswith`), F5 (CUI pe lista de parteneri),
F6 (Flax fără migrație), F7 (`.env` neignorat), F8/F9/F10 (comentarii/cod mort),
F11 (precompletarea lasă tipul primei selecții — preexistent). F1–F9 fixate de
main; F10/F11 = restanțe. Smoke pe calea reală: 7/7 (S3 → fișă, jurnalele +
decontul văzute cu cusătura la cent, F6 închisă, precompletarea ASM/BCS,
diacriticele, XAF fără `Cautare`, consola zero erori) + D1 (meniul ieșea din
viewport la 1416 px — fixat, bara se rupe) + D2 (afișarea `TipMaterial` diferă
ASM vs BCS — restanță). Constatare nouă: **`Cod`/`Denumire` nu sunt obligatorii
pe nicio ușă** (partener fără denumire acceptat) — restanță.

## Ce rămâne deschis (restanțele 77-r1…r8)

- **77-r1** BTR fără convenția 61b (etichete, precompletare) — plumbing de felie.
- **77-r2** `Cod`/`Denumire` neobligatorii pe `Repartitor`/`Produs` pe nicio ușă
  (gardian, DB) — regulă de fond de pus în `GardianEditare`, cu proba pe HTTP.
- **77-r3** editarea `PoliticaMiscareSaft` din React (regula 56 rămâne; decizie
  separată) + comanda ANAF de LOT (nu există listă cu selecție multiplă).
- **77-r4** `CodFiscal`/`Iban`/`Marca` în afara lui `Cautare` — căutarea după CUI
  rămâne sensibilă la caz/diacritice (CUI e numeric, expunerea e mică).
- **77-r5** precompletarea nu distinge alegerea operatorului de precompletarea
  anterioară (review F11, preexistent); invalidarea `['nomenclator', …]` nu
  reîmprospătează `SelectBox`-urile deja montate (review F10).
- **77-r6** afișarea `TipMaterial` diferă între felii (ASM cod + denumire, BCS
  doar denumire) — un `displayExpr` de nucleu.
- **77-r7** `Cautare` fără index: `contains` pe 22 k produse / 20 k parteneri e
  seq scan — se măsoară când cifra o cere (59), nu preventiv.
- **77-r8** refuzul de permisiune pe OData rămâne `text/plain` pe server (tradus
  doar în client); familia 70-r1/72-r10/76-r4/76-r5 cere o decizie unică pe
  „404 vs 403 vs 422" pe toate ușile.
