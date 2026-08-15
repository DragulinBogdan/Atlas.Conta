# Pasul 5, felia 8 — Decont (DEC) + legătura explicită de pereche (contract)

Data fixării: 2026-08-15. Șablonul consolidat al feliilor F2–F7 (contractele
`p5-felia-fct-contract.md`, `p5-felia-trz-contract.md`,
`p5-felia-ldi-bcs-contract.md`, `p5-felia-nir-contract.md`,
`p5-felia-vir-contract.md`). Deciziile F8-D1…F8-D15 sunt **PIN-UITE** — agenții
de implementare nu le redeschid; orice nepotrivire cu realitatea codului se
RAPORTEAZĂ, nu se normalizează tăcut.

## Scop

Două lucruri independente, într-o singură felie fiindcă amândouă sunt restanțe
declarate ale feliilor imediat anterioare și amândouă ating aceeași margine
(trezoreria / lanțul avans↔decont):

1. **Decont (DEC)** — scriere + citire + comenzi prin API, felie client
   editabilă. Ridică excluderea F6-D1/F6-D12, care numea exact trei blocaje:
   lookup pentru `Angajament`, expunerea `Cont` (postarea explicită pe linie) și
   decizia de aderare a lui `DecontDetaliu` la `ILinieCuPretUnitar`. Fiecare se
   tranșează mai jos.
2. **Legătura explicită de pereche** — gaura DESCHISĂ și documentată la decizia
   64k (constatarea feliei 7): dacă operatorul ignoră draftul autogenerat și
   culege al doilea picior al viramentului MANUAL, acela e un virament valid și
   își generează la rândul lui perechea (un al treilea document), iar operarea
   ambelor picioare de intrare **dublează postarea, tăcut**. Nu există criteriu
   de CONȚINUT care să distingă cazul (două viramente identice între aceleași
   conturi, în aceeași zi, sunt perfect legitime) ⇒ fixul e o legătură declarată,
   nu o euristică.

Fluxul-ancoră DEC: titularul justifică un avans — `Decont` cules pe angajat
(predator) către unitatea internă (primitor), linii cu cheltuiala și TVA-ul
bonului, unele cu postare explicită pe linie → operat (`6xx = 542`) →
imperecheat cu plata de avans (lanțul 31d/32d).

Fluxul-ancoră pereche: `PLT-9` CASA → BANCA operată generează `INC` draft cu
legătura pusă de motor; operatorul care culege manual al doilea picior îl
declară pereche și acela **nu mai generează nimic**; dacă nu-l declară, primește
un avertisment consultativ care îi arată picioarele candidate.

## Deciziile

### F8-D1 — Scope: DEC complet + pereche; nimic altceva

DEC urmează șablonul consolidat: `Api/Dec/` (Dtos + Apply) în Module (testabil
în ModelCheck), controller subțire în WebApi, felie client editabilă, blocuri
ModelCheck, codegen + drift. Comenzile (operare/anulare/storno/dry-run) merg pe
`OperareApi` existent, ca la toate feliile.

Cele trei blocaje F6-D1 se tranșează la F8-D2 (`ILinieCuPretUnitar`), F8-D4
(`Cont` + `Angajament` în OData) și F8-D5 (editorul cu postare explicită).

### F8-D2 — Modelul DEC: aderarea la `ILinieCuPretUnitar`, ZERO migrație pe DEC

`DecontDetaliu` declară `ILinieCuPretUnitar` (`PretUnitar` există deja pe clasă
din 3a ⇒ pură declarație de contract, nicio coloană). Consecința: seam-ul comun
de recalcul (`TvaService.CalculeazaLaCulegere`, GATE XAF D5) devine disponibil
și pe DEC — valoarea și TVA-ul se văd la culegere, nu abia la operare, exact
cum cere comentariul din `Comun/Interfete.cs:29-32`.

**Cantitatea pro-formă devine VIZIBILĂ**: normalizarea `0 → 1` (32d, azi doar în
`PregatesteOperare`) se aplică și în `Aplica` — „culegerea ar trebui s-o facă
vizibil, nu în spate" e chiar nota din interfață. `PregatesteOperare` rămâne
NEATINS (idempotent; e calea XAF/import).

Captions RO (`[XafDisplayName]`) pe câmpurile DEC care azi ies brute în
`--dump-metadata`: `Descriere`, `PretUnitar`, `ContDebit`, `ContCredit`,
`RepartitorDebit`, `RepartitorCredit`, `NumarPV`, `DataPV` (se verifică în dump
înainte de a le adăuga — cele care au deja caption nu se ating).

### F8-D3 — Contractul de scriere DEC

`Numar` este **SERVER-OWNED** — DEC are `PoliticaNumerotare` (`DEC-`) în AMBELE
profiluri (`ProfilPrivat.cs:743`, `ProfilBugetar.cs:522`) ⇒ NU intră în
`WriteDto` (ca BTR/FCL/TRZ/NIR/LDI/BCS; invers față de FCT, unde numărul e al
furnizorului).

- Header cules: `Data`, `PredatorId` (titularul — `Angajat`), `PrimitorId`
  (`UnitateInterna`/`Gestiune`), `NumarPV`, `DataPV` (`IDocumentCuPV`).
- Linie culeasă: `TipMaterialId`, `Descriere`, `Cantitate`, `PretUnitar`,
  `TipTvaId`, `ValoareTva`, `CodEconomicId`, `AngajamentId`, plus cele patru
  câmpuri de postare explicită (`ContDebitId`, `ContCreditId`,
  `RepartitorDebitId`, `RepartitorCreditId`).
- `Valoare` e SERVER-OWNED (rezultat: `PretUnitar × Cantitate` prin
  `TvaService`), read-only în client — regula FCT/FCL, neschimbată.

**TVA-ul urmează IDENTIC regulile F2/F4** (o singură sursă, fără variantă a
treia): `TipTvaImplicit` doar pe liniile NOI (pe cele existente absența = golire
deliberată); `CalculeazaLaCulegere` CONDIȚIONAT de declanșatori (baza sau
`TipTva` schimbate — altfel override-ul de `ValoareTva` moare la orice PUT);
override acceptat DOAR pe regimurile cu TVA separat și niciodată negativ. DEC
are `PoliticaTva` la privat (`Deductibil`, 4426 = 542) și `TipTvaImplicit` în
ambele profiluri — deci calea e vie pe amândouă.

### F8-D4 — OData: trei intrări noi, toate ReadOnly

- **`Cont`** — planul de conturi (1.679 bugetar / 644 privat). Nomenclator de
  politică ⇒ ReadOnly (regula lui `TipTva`/dimensiuni, F2-D4); `mod="remote"` în
  client (volumul e de ordinul loturilor/partenerilor).
- **`Angajament`** — entitatea există (`Nomenclatoare/DimensiuniBugetare.cs:50`,
  `DefaultProperty = Denumire`), tabela e azi GOALĂ (modulul de angajamente e
  amânat — decizia 22c). Lookup-ul e onest: gol înseamnă gol. ReadOnly (datele
  vin din alt modul, când apare).
- **`Repartitor`** — BAZA TPT. Cele două câmpuri `RepartitorDebit/Credit` ale
  postării explicite acceptă ORICE repartitor (`ILinieCuPostareExplicita` e
  tipată pe bază), deci un lookup pe una dintre derivatele expuse ar minți prin
  omisiune. ReadOnly, `mod="remote"`.

Consecință acceptată, deja declarată ca datorie în spike (55h): `$metadata`
expune și mai mult din forma modelului. Nu se adaugă scriere pe niciuna.

### F8-D5 — Clientul DEC

Felia `src/felii/dec` pe șablonul consolidat (`DocumentShell` + `Formular` +
editor de linie), cu:

- **grupul „Postare explicită" pe linie**, colapsat implicit (`<details>`, ca
  „Plată automată" pe FCT): e trăsătura PROPRIE a tipului (32a), nu default-ul
  culegerii. Cele patru câmpuri rămân opționale — nerezolvate cad pe regulă.
- **Lookup `Cont`**: `mod="remote"`, `cauta: ['Simbol', 'Denumire']`,
  `afisare: e => \`${e.Simbol} — ${e.Denumire}\``, `filtru: ['Sumator', '=', false]`.
  Filtrul e **AFFORDANCE, nu validare** — autoritatea rămâne motorul (nu se
  adaugă nicio regulă nouă de refuz pe conturi sumator în felia asta).
- **Laturile**: predator = `Lookup` pe `Angajat`; primitor = `Lookup` pe
  `UnitateInterna` cu escape-ul 61c (valoare istorică din afara setului sau
  sondă nereușită ⇒ afișare statică din ReadDto — „default-ul care nu minte";
  `Gestiune` rămâne valid server-side, doar nu e ofertat).
- **`PanouStingeri` rol `este-stins`**: DEC e deja una dintre cele 5 ramuri ale
  proiecției `DocumenteCuRest` (57c) și stă pe lanțul avans↔decont↔regularizare
  (31d/32d) — panoul e cerința funcțională a tipului, nu decor.
- **Etichetele culese (61b)** pe `TipMaterial`, `TipTva`, `Cont*`, `Angajament`
  — liniile nesalvate nu afișează gol.
- Rută `/dec` + meniu (`App.tsx`) + `rutaTip('DEC')` (`nucleu/stingeri.ts:44` —
  DEC iese din lista „tipuri fără felie, rămân TEXT").

Convențiile cimentate se respectă integral: garda `if (e.event)`, `laSelectie` =
notificare + update funcțional, `String()` pe Guid-uri, formularul = sursa de
adevăr, zero calcul de domeniu în TS.

### F8-D6 — `LaturaPerecheId`: o coloană, o parte scrisă, citire derivată

`DocumentTrezorerie.LaturaPerecheId` (`Guid?`, FK → `DocumentTrezorerie`,
`DeleteBehavior.Restrict`) + navigația. **Singura migrație a feliei**
(`LaturaPereche`), strict aditivă.

- **Motorul îl pune pe COPIL** la generare: `pereche.LaturaPerecheId = ID`
  (în `GenereazaSecundar`, lângă clonarea laturilor).
- **Operatorul îl culege pe latura manuală**, arătând spre piciorul existent.

Se scrie o SINGURĂ parte, deliberat: cealaltă e Operată, iar gardianul de
Committing refuză orice scriere pe documentele ne-Draft (55a) — o legătură
bidirecțională ar cere o ușă non-secured pentru un simplu link.

Citirea e DERIVATĂ și simetrică, un singur helper pe `DocumentTrezorerie`:

```
public Guid? PerecheId(IObjectSpace os) =>
    LaturaPerecheId ?? os.GetObjectsQuery<DocumentTrezorerie>()
        .Where(x => x.LaturaPerecheId == ID).Select(x => (Guid?)x.ID).FirstOrDefault();
```

Consumatori: suprimarea generării (F8-D7), avertismentul (F8-D10), gardianul
(F8-D9), affordance-ul din ReadDto (F8-D11).

### F8-D7 — Suprimarea generării, în AMBELE sensuri

`GenereazaSecundar` întoarce `null` și când **cineva mă arată pe MINE** ca
pereche, nu doar când eu arăt spre altcineva:

```
if (Autogenerat || !EsteVirament(os) || PerecheId(os) != null) return null;
```

Fără jumătatea a doua, cazul „ambele picioare culese manual înainte de operare"
(al doilea legat de primul) ar genera un al treilea document la operarea
PRIMULUI — gaura 64k mutată cu o zi mai devreme, la fel de tăcută.

Gardul `Autogenerat` rămâne (recursia F7-D4), deși copilul are acum și link —
două motive independente pentru același refuz, niciunul nu-l face pe celălalt
redundant (copilul poate fi șters și recules manual).

### F8-D8 — Validarea legăturii (`ValideazaOperare` pe `DocumentTrezorerie`)

Legătura NU e obligatorie (generarea acoperă cazul normal); dacă e prezentă, se
validează integral:

1. documentul care o declară e el însuși virament (o plată normală cu link ⇒
   refuz — „latura pereche există doar la viramentul intern");
2. ținta e virament (ambele laturi conturi proprii — predicatul `EsteVirament`,
   o singură definiție);
3. ținta e de tipul OPUS — prin contract, fără `is`/`switch` pe tip în bază:
   `public abstract Type TipLaturaPereche();` (Plata → `typeof(Incasare)`,
   Incasare → `typeof(Plata)`), verificat cu `IsInstanceOfType` pe instanța
   încărcată polimorf;
4. ținta are ACELEAȘI laturi (`PredatorId`/`PrimitorId` identice — F7-D1: cele
   două picioare stau pe aceleași laturi, direcția o poartă tipul);
5. ținta nu e deja legată de un al TREILEA document (nici `LaturaPerecheId`-ul
   ei spre altcineva, nici alt document care o arată) — altfel triplete;
   > **Amendament (verificarea pasului 1)** — se refuză și legătura RECIPROCĂ
   > (`A→B` cu `B→A` deja pus). Contractul inițial o lăsa să treacă („nu e un al
   > treilea document"), dar e o capcană cu ieșire zero: după operarea ambelor
   > picioare fiecare îl blochează pe celălalt la anulare/storno (F8-D9), iar
   > linkul nu se mai poate șterge — documentele nu mai sunt Draft. Legătura e
   > unilaterală prin construcție (F8-D6), deci dublarea ei nu are niciun caz
   > legitim; mesajul spune explicit că celălalt picior o declară deja.
6. ținta nu sunt eu însumi (self-link).

Verificările lucrează pe FK-uri + `IObjectSpace`, fără navigații lazy în
enumerare (25b).

### F8-D9 — Gardian simetric de anulare/storno

`VerificaFaraLaturaPerecheOperata`: refuz cât timp există un document **OPERAT**
care mă arată ca pereche. E oglinda exactă a lui `VerificaFaraConexeOperate`
(pointer ≈ copil, țintă ≈ sursă): piciorul care DECLARĂ legătura e frunza și se
anulează liber; ținta nu, cât timp pointer-ul e operat.

Perechea autogenerată e acoperită deja de gardianul de grup (`DocumentSursaId`)
— noul gardian o acoperă a doua oară, inofensiv. Ordinea apelurilor se alege
astfel încât mesajul cel mai specific să ajungă primul la operator; se verifică
în ModelCheck ce mesaj iese pe fiecare cale.

### F8-D10 — Avertisment consultativ, NU refuz

Hook polimorf nou pe bază: `Document.MesajeDupaOperare(IObjectSpace os)` →
`IReadOnlyList<string>` (default gol), consumat de `OperareApi.Opereaza` —
poarta UNICĂ a ambelor căi (API + `DocumentOperareController`, 55b/58c).

Pe `DocumentTrezorerie`: la operarea unui virament care **și-a generat perechea**
(deci nedeclarat), dacă există picioare compatibile neîmperecheate — virament,
tip OPUS, aceleași laturi, `Operat`, `PerecheId == null` — mesajul le enumeră
(plafon 5, cu „…și încă N") și spune calea corectă: „dacă acesta e piciorul lor,
anulați, alegeți «latura pereche» și ștergeți draftul generat".

Refuzul e exclus prin construcție: două viramente identice între aceleași
conturi, în aceeași zi, sunt legitime (64k) — un criteriu de conținut ar
transforma un caz real în blocaj.

**Atenție**: ModelCheck are assert-uri pe `Mesaje.Count` (Program.cs ~2299,
2400, 2861, 3408, 4417). Se verifică fiecare — un assert care devine
false-negative se REPARĂ argumentat, nu se relaxează global.

### F8-D11 — API pereche

- `TrzWriteDto`: `LaturaPerecheId` (`Guid?`, cules).
- `TrzReadDto`: `LaturaPerecheId` + **`Pereche`** (obiect derivat nullable:
  `{ Id, Tip, Numar, Stare }`) — sursa UNICĂ a indiciului din client, care azi
  se deduce din `Copii[]`/`DocumentSursaId` și deci ratează legătura manuală.
- Endpoint de candidați, pe ambele rute:
  `GET /api/{plt|inc}/candidati-pereche?predatorId=&primitorId=&exclusId=` →
  listă mică (`Id`, `Numar`, `Data`, `Total`, `Stare`) cu picioarele de tip
  OPUS, viramente pe aceleași laturi, fără pereche. Plafon `take` ca la restul
  proiecțiilor. Trăiește în felia `Api/Trz` (e specifică trezoreriei), nu în
  `ApiProiectii`.
- `DocumenteCuRest` rămâne NEATINSĂ (anti-join-ul pe viramente e deja acolo din
  F7 — amendamentul F7-D7).

`TrezorerieApply.Aplica` acceptă noul câmp ca orice câmp cules; validarea lui
rămâne în motor (F8-D8) — apply-ul nu duplică reguli de domeniu.

### F8-D12 — Clientul pereche

În modul virament (`EsteVirament`), indiciul introdus la F7 devine **câmp**:

- `Latura pereche` = SelectBox pe candidați (fetch la schimbarea laturilor sau a
  tipului; garda `if (e.event)`; „(niciuna)" = generarea automată rămâne
  comportamentul);
- starea perechii se citește din `Pereche` (ReadDto), cu link prin `rutaTip` —
  înlocuiește deducerea din `Copii[]`, care nu vedea legătura manuală;
- mesajul consultativ ajunge pe calea existentă `Mesaje[]` a operării, fără
  mecanism nou;
- panoul de stingeri rămâne ascuns pe virament (F7-D8), neschimbat.

### F8-D13 — ModelCheck: blocurile `E2E-ADEC` și `E2E-APER`

Ambele pe suita BUGETARĂ (baza aplicației), cu `dotnet run privat` verde la
fiecare pas. Șablonul feliilor 6/7 (fixture + dry-run pe OS propriu +
`CheckRefuza` + curățenie finală cu assert).

**`E2E-ADEC`** — ancorele obligatorii:

1. culegere prin `DecontApply.Aplica`: `Numar` NEcerut și asignat abia la
   operare (proba GATE D6 — refuzul nu consumă seria); cantitate 0 → 1 vizibilă
   după `Aplica`; `Valoare = PretUnitar × Cantitate` la culegere; `TipTvaImplicit`
   doar pe liniile noi; override `ValoareTva` reținut fără declanșator și cedat
   la schimbarea bazei (probele F2, pe DEC);
2. operare: debitul din `ContDebitId` **bate** rezolvarea declarativă
   (`SursaCont.TipMaterial`), creditul cade pe titular (fallback 542), iar
   `RepartitorDebitId` cules e nivelul maxim al coalesce-ului de dimensiuni
   (32a) — verificat pe rândurile de registru;
3. refuzuri prin dry-run, fără rânduri-fantomă și fără serie consumată:
   predator ne-`Angajat`, primitor greșit, linie cu `Valoare <= 0`;
4. imperecherea avans → decont (nonregresia 32d/31d) și affordance-ele oneste
   (`PoateAnula/PoateStorna` false cât există imperecheri — 57d);
5. `Citeste` / `Lista` / `Sterge` + reconcilierea liniilor (Id repetat, linie de
   tip BAZĂ prin Id — probele M3, 60d).

**`E2E-APER`** — ancorele obligatorii:

1. legătura manuală suprimă generarea în AMBELE sensuri: (a) al doilea picior
   cules cu link, operat ⇒ zero copii; (b) ambele picioare Draft, al doilea
   legat, se operează PRIMUL ⇒ zero copii (F8-D7);
2. fiecare refuz din F8-D8, separat, prin dry-run;
3. gardianul F8-D9: anularea țintei cu pointer-ul Operat = refuz; anularea
   pointer-ului = permisă; după anularea pointer-ului, ținta se anulează;
4. mesajul F8-D10: prezent când există candidați compatibili, ABSENT când nu —
   ambele asertate (un mesaj care apare mereu e zgomot, unul care nu apare
   niciodată e mort);
5. nonregresia F7 integrală: perechea autogenerată primește `LaturaPerecheId`,
   581 = 0 după ambele picioare, ZERO imperecheri, gardul de recursie ține.

### F8-D14 — Ce NU intră în felie

Modulul de angajamente (22c — lookup pe tabelă goală, nimic altceva); wiring-ul
lookup-ului `Angajament` în celelalte felii (FCT/NIR/TRZ/LDI poartă deja
`AngajamentId` în DTO-uri — aditiv, la cerință); importul extraselor de
trezorerie; mecanismul pentru comisionul bancar (64k — reziduul rămâne pe 581 și
se închide cu notă); scrierea prin OData pe `Cont`/`Angajament`/`Repartitor`;
ecranul XAF al DEC-ului (rămâne generic — clientul e ținta feliei); orice regulă
nouă de refuz pe conturi sumator.

### F8-D15 — Pașii de implementare (un agent per pas, secvențial)

1. **Pas 1 — Module: pereche** (F8-D6…F8-D10) + migrația `LaturaPereche` +
   `--dump-metadata`. Verificare: build, `database update`, ModelCheck AMBELE
   profiluri verde cu blocurile existente (assert-urile pe `Mesaje` verificate).
2. **Pas 2 — Module: DEC** (F8-D2) + `Api/Dec` (Dtos + Apply) + controller host
   + OData (F8-D4) + blocul `E2E-ADEC`. Verificare: ModelCheck ambele profiluri,
   openapi regenerat + `verifica:drift`.
3. **Pas 3 — API pereche** (F8-D11) + blocul `E2E-APER` + openapi/drift.
4. **Pas 4 — Clientul** (F8-D5 + F8-D12), `gen:types`, build client verde.
5. **Review advers dedicat** (agent separat, scenarii concrete de exploatare);
   fix-urile le aplică main-ul.
6. **Smoke browser** pe perechea WebApi + client (baza Privat): DEC cules →
   operat → imperecheat cu un avans; virament cu latura pereche declarată
   manual (zero al treilea document) + avertismentul consultativ pe cazul
   nedeclarat.

Comenzile de verificare (main le re-rulează independent la fiecare pas):

```
cd nou/tools/ModelCheck && dotnet run                     # bugetar (baza aplicației)
cd nou/tools/ModelCheck && dotnet run privat              # baza dedicată
dotnet run --project nou/tools/ModelCheck -- --dump-metadata
cd nou/Atlas.Conta.Client && pnpm verifica:drift           # pasul 2+
cd nou/Atlas.Conta.Client && pnpm build                    # pasul 4
```

O SINGURĂ migrație în felie (`LaturaPereche`, pasul 1) — dacă un agent ajunge să
scrie a doua, s-a abătut de la contract și RAPORTEAZĂ.

## Riscurile pin-uite (review-ul advers le țintește)

1. **Dubla postare, mutată în altă zi**: ambele picioare Draft cu link pe al
   doilea; linkul pus și apoi ȘTERS pe draft; linkul spre un picior care își are
   deja copilul autogenerat; trei picioare între aceleași conturi în aceeași zi.
2. **Legătura ca armă**: link către un document de alt tip / cu alte laturi /
   deja legat; link circular (A→B și B→A); link către sine; link pe o plată
   normală (non-virament) care apoi devine virament prin schimbarea laturilor.
3. **Gardianul F8-D9 vs. grupul conex**: care mesaj iese; se poate ajunge la un
   picior imposibil de anulat (deadlock A↔B)? storno pe unul singur — ce rămâne
   pe 581 și ce spune panoul.
4. **DEC — postarea explicită**: conturi explicite parțiale (doar debit); cont
   sumator ales din lookup (affordance, nu validare — ce face motorul);
   repartitor explicit vs. coalesce-ul de dimensiuni; linia fără `TipMaterial`
   dar cu ambele conturi (DEC NU e `IDocumentCuPostareExplicita` — postarea
   explicită nu-l scutește de regulă).
5. **DEC — TVA + cantitate**: `Cantitate = 0` la culegere vs. la operare;
   override `ValoareTva` pe regim fără TVA separat; profilul bugetar (fără
   `PoliticaTva` pe DEC) — TVA-ul cules rămâne cifră moartă sau se refuză?
6. **Seria consumată / rânduri-fantomă la refuz** (33d) pe ambele căi noi.
7. **OData**: `Repartitor` bază expusă — ce iese în `$metadata` și dacă vreo
   derivată devine scriibilă din greșeală; `Cont` ReadOnly cu volum mare (perf
   lookup remote).
8. **Calea XAF vie**: ecranul PLT/INC din back-office permite culegerea
   linkului? Vreo capcană tăcută (lecția F5/F6 — „un gard care tace devine
   capcană exact pe calea unde există și alternativa corectă").
