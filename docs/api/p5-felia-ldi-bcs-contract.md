# Pasul 5, felia 6 — LDI + BCS prin API și client (contract)

Data fixării: 2026-08-12. Șablonul consolidat al feliilor F2–F5 (contractele
`p5-spike1-contract.md`, `p5-felia-fct-contract.md`, `p5-felia-nir-contract.md`).
Deciziile de mai jos (F6-D1…F6-D12) sunt PIN-UITE — agenții de implementare nu
le redeschid; orice nepotrivire cu realitatea codului se RAPORTEAZĂ, nu se
normalizează tăcut.

## Scop

- **ListaDiferenteInventar (LDI)** — scriere + citire + comenzi prin API, felie
  client editabilă. Include pasul de MODEL care închide restanța 53i pe LDI+:
  culegerea produsului care naște lotul de plus (exact ce a făcut F5 pe NIR).
- **BonConsum (BCS)** — scriere + citire + comenzi prin API, felie client
  editabilă (clonă a șablonului BTR).
- **Decont (DEC) — EXCLUS**, felie viitoare (F6-D12).

## Deciziile

### F6-D1 — Scope: LDI + BCS; DEC amânat

DEC cere trei lucruri pe care felia asta nu le are: lookup pentru
`AngajamentId` (nu există nicăieri în client — restanța 62g; pe profil bugetar
`PoliticaValidare.CereClasificatieBugetara` îl face necesar pe fiecare linie),
expunerea OData a `Cont` pentru cele 4 câmpuri `ILinieCuPostareExplicita`
(azi Cont nu e expus deloc) și decizia de aderare a lui `DecontDetaliu` la
`ILinieCuPretUnitar` (deliberat neaderat — `Comun\Interfete.cs`). Fiecare e o
decizie proprie — DEC primește contractul lui.

### F6-D2 — Pasul de model LDI, indivizibil (închide 53i pe LDI+)

`ListaDiferenteInventarDetaliu` capătă `ProdusId` (Guid?) + navigația `Produs`
și declară `ILinieCareNasteLot`. `ListaDiferenteInventar` face override
`GestiuneLoturiCulese` → **PREDATORUL** (gestiunea inventariată — 28d;
default-ul e primitorul, iar primitorul LDI e comisia, care nu e `Gestiune` ⇒
fără override serviciul ar tăcea pentru totdeauna). Migrația `LdiCulegereLot`,
strict aditivă. Cele trei piese sunt indivizibile: oricare lipsă lasă mesajul
„Linia de plus își creează lotul la culegere (alegeți produsul)" neîndeplinibil
sau naște lotul în locul greșit.

Captions RO (`[XafDisplayName]`) pe câmpurile LDI care azi ies brute în dump:
`Directie`, `PretEvaluare`, `DataExpirare`, `LotFabricatie`, + `Produs` nou.

### F6-D3 — Gardul de direcție: `NasteLot` pe contract

`ILinieCareNasteLot` capătă membrul `bool NasteLot => true;` (default interface
member — FCT/NIR neatinse). LDI îl suprascrie: `NasteLot => Directie ==
DirectieDiferenta.Plus`. `LoturiCulegereService.Sincronizeaza` îl consultă:
`NasteLot == false` rutează linia pe ramura de curățare (șterge lotul propriu
NEFINALIZAT / rupe referința), plasată DUPĂ gardul de lot străin și ÎNAINTE de
self-healing — altfel o linie comutată Plus→Minus cu lotul propriu finalizat
(operare + anulare + comutare) ar primi `ProdusId` înapoi prin self-heal,
luptându-se cu golirea din Apply. Fără gardul ăsta, o linie de MINUS cu produs
cules ar naște lot-artefact pe draft (raportul de explorare, punctul 3).

În `LdiApply`, liniile de Minus se GOLESC de câmpurile care aparțin plusului:
`ProdusId`, `PretEvaluare`, `DataExpirare`, `LotFabricatie` (persistat null —
lecția F5 „inert devine adevărat, nu doar afirmat"; comutarea de direcție în
UI nu blochează documentul).

### F6-D4 — `Numar` server-owned pe ambele

LDI și BCS au `PoliticaNumerotare` în ambele profiluri (`LDI-`, `BCS-`) ⇒
seria e server-owned, `Numar` NU intră în WriteDto (contractul BTR/NIR/FCL/TRZ,
invers față de FCT). Consumat abia la materializare (GATE D6) — ModelCheck
probează că un refuz nu consumă seria.

### F6-D5 — `LotId` pe LDI: cules DOAR pe Minus

`LotId` intră în `LdiLinieWriteDto`, dar Apply îl aplică NUMAI pe liniile
Minus (pinul lotului descărcat). Pe Plus e server-owned (lotul se naște prin
serviciu); valoarea din payload se IGNORĂ — obligatoriu, fiindcă ReadDto îl
întoarce (round-trip-ul agregatului l-ar retrimite mereu). Pe BCS `LotId` e
cules normal, nullable pe draft (ca BTR); validarea de operare îl cere.

Fără TVA pe niciunul (nu au `PoliticaTva` în niciun profil — precedentul
F5-D5: cifră moartă). `TipTvaId`/`ValoareTva` nu intră în DTO-uri.

### F6-D6 — Valoarea la culegere: `MaterializeazaValori` geamănă

Ambele Apply-uri materializează `Valoare` la culegere cu formula GEAMĂNĂ a
hook-ului `PregatesteOperare` (precedentul F5-D6):

- **BCS**: `Valoare = RotunjesteBani(Cantitate × lot.PretUnitar)` (lot ales;
  fără lot ⇒ 0).
- **LDI**: SEMNATĂ — Minus: `−|Cantitate| × lot.PretUnitar`; Plus:
  `+|Cantitate| × (PretEvaluare ?? 0)`. `Cantitate` rămâne cum a fost culeasă
  (pozitivă) până la operare — semnarea cantității e a operării (28a);
  `Total`-ul draftului arată efectul NET al inventarului.

Reguli moștenite din F5: `OfType<frunză>()` pe LDI (liniile de tip BAZĂ
istorice nu se ating — clasa de defect GATE D1), `IsObjectToDelete` sărit,
rulează DUPĂ `Sincronizeaza`. BTR rămâne neatins (ModelCheck îi probează azi
`Valoare == 0` pre-operare); retrofit-ul lui = item minor, notat la Închidere.

### F6-D7 — ReadDto și affordances

- Șablonul NIR: header plat cu denumirile laturilor, linii cu etichete per FK,
  `Stare`/`Directie` ca STRING pe sârmă (enum → nume; parse pe NUME la
  graniță, ÎNAINTE de orice `CreateObject` — precedentul `ApiEnum`).
- LDI se citește pe BAZA detaliului cu frunza prin `as`-cast (TPT LEFT JOIN,
  nullable explicit pe valorile frunzei) + refuzul „linie de tip vechi" în
  reconciliere — există LDI-uri istorice de import. BCS se citește direct pe
  `DocumentDetaliu` (nu are frunză) — șablonul BTR, fără cast.
- Affordances: `PoateEdita/PoateOpera` din Draft; `PoateAnula/PoateStorna` =
  Operat + `!AreImperecheri` (predicatul e al BAZEI — șablonul NIR). Fără
  `Copii[]` (niciunul nu generează conex/secundar). Fără `LotStrain` pe LDI —
  `Directie` conduce UI-ul (LDI e întotdeauna cules manual, nu are clonă
  conexă); `LotEticheta` prin `ApiProiectii.EtichetaLot`.
- `Cantitate` pe un LDI operat iese SEMNATĂ (fapta operării) — clientul o
  afișează ca atare, documentul e oricum read-only.

### F6-D8 — Laturile interne în client: nefiltrat, autoritatea e motorul

Primitorul BCS (intern cu calitatea `LocConsum`) și primitorul LDI (comisia —
calitatea `Comisie`) se culeg prin `Lookup` pe `UnitateInterna`, mod local,
NEFILTRAT pe calitate. Filtrarea pe `Calitati` (flags) nu traversează azi
lanțul DevExtreme→OData (`has` nu se generează), iar precedentul e cimentat:
lookup-ul de Lot pe BTR e nefiltrat, „refuzul e al motorului" (F4-D6).
Dry-run-ul `valideaza` + refuzul de operare dau mesajul de domeniu. Limitare
documentată la Închidere: filtrarea pe calitate (set OData dedicat sau `has`)
= extensie viitoare; un purtător de `LocConsum` care NU e `UnitateInterna`
(flag-ul poate sta pe orice intern) nu e selectabil din client în felia asta.

### F6-D9 — Clientul

- **BCS** = clona feliei BTR: aceleași trei rute, listă, detaliu, editor de
  linie (TipMaterial + Lot remote cu `$expand=Produs` + Cantitate), minus
  `NumarPV`/`DataPV` (BCS nu e `IDocumentCuPV`), plus comanda Șterge (ca NIR).
- **LDI** = editor de linie cu COMUTATOR de direcție (`CampSelectie`
  `enumerare="DirectieDiferenta"`): Plus ⇒ Produs (remote, `$expand=
  TipMaterial`, precompletare Tip cu update funcțional) + PretEvaluare +
  DataExpirare/LotFabricatie + CodEconomic; Minus ⇒ Lot (remote, `$expand=
  Produs`, sortare Data, nefiltrat) + precompletare Tip din lotul ales.
  Comutarea de direcție GOLEȘTE client-side câmpurile celeilalte direcții
  (comparație PRE-update, pattern-ul pinului FCL) — oglinda golirii din Apply.
- Etichete nesalvate per poziție (mecanismul 61b), inclusiv `LotEticheta`
  culeasă la selecție pe Minus (pattern-ul FCL).
- **Extracția `etichetaLot` în nucleu** (`nucleu/lot.ts`): a treia și a patra
  utilizare — varianta bogată din FCL (cazul „în culegere" + `Edm.Date` ca
  obiect) devine unica; BTR și FCL migrează pe ea în aceeași felie.
- Înregistrare: rute `/bcs`, `/ldi` (+ `/nou`, `/:id`), `NavLink`, `rutaTip`
  (+ 'LDI', + 'BCS').
- Convențiile cimentate se respectă integral: garda `if (e.event)`,
  `laSelectie` = notificare + update funcțional, `String()` pe Guid-uri,
  formularul = sursa de adevăr, zero calcul de domeniu în TS.

### F6-D10 — Oglinda XAF (lecția F5: gardul care tace devine capcană)

Declararea `ILinieCareNasteLot` face ecranul XAF de LDI cale VIE de culegere
(`DocumenteLoturiCulegereController` e generic pe `Document`). Oglinda în
`ContaUiBaseline`: coloana `Produs` intră în layout-ul frunzei LDI;
`[Appearance]` pe frunză comută editabilitatea pe direcție (Plus: `Lot`
read-only; Minus: `Produs`/`PretEvaluare`/atributele read-only). Ecranul XAF
de LDI rămâne funcțional, NU product-grade (ca NIR la F5 — 62g); smoke
minimal la închidere.

### F6-D11 — ModelCheck: blocurile Api, pe profil BUGETAR

Ambele tipuri au politici în ambele profiluri și zero TVA ⇒ blocurile rulează
pe suita bugetară (ca blocul NIR). Marcaje `E2E-API-BCS` / `E2E-API-LDI`,
șablonul feliei 5 (fixture + `DryRun` cu OS propriu + `CheckRefuza` + curățenie
finală cu assert). Ancorele obligatorii:

- BCS: Apply → Citeste/Lista; `Valoare` materializată la culegere = preț lot ×
  cantitate; refuzuri (laturi, lot lipsă la operare, cantitate ≤ 0) fără
  rânduri-fantomă; seria `BCS-` neconsumată la refuz; operare → −Magazie
  predator + +Consum primitor; anulare; storno.
- LDI: **testul-ancoră** — plusul cules prin Apply (ProdusId) naște lotul pe
  linia proprie în gestiunea PREDATORULUI (hook-ul override); PUT repetat NU
  naște al doilea lot; comutarea Plus→Minus prin PUT șterge lotul propriu
  nefinalizat și golește câmpurile plusului; minusul cu lot pinuit rămâne
  NEATINS (gardul de lot străin); linia frunză „istorică" cu lot finalizat și
  `ProdusId` null → self-healing, nu ștergere (proba F5 replicată); `Valoare`
  semnată la culegere; operare → semnare cantitate + lot finalizat cu
  `PretUnitar = PretEvaluare` + atribute copiate; refuzurile existente ale
  tipului (Comisie, preț de evaluare, direcție) prin `CheckRefuza`; anulare;
  storno.

Verde pe AMBELE profiluri la fiecare pas (privatul prin `dotnet run privat`).

### F6-D12 — DEC: felie viitoare

Vezi F6-D1. La contractul lui: expunerea `Cont` în OData (ReadOnly), lookup
`Angajament`, aderarea `ILinieCuPretUnitar`, editorul cu postare explicită.

## Pașii de implementare (un agent per pas, secvențial)

1. **Pas 1 — Modelul LDI + serviciile** (Module): F6-D2 + F6-D3 + F6-D10 +
   captions + migrația + `--dump-metadata`. Verificare: build, `database
   update`, ModelCheck AMBELE profiluri verde (inclusiv blocul e2e LDI
   existent, neatins — `CreeazaLot` manual rămâne valid).
2. **Pas 2 — API** (Module Api + WebApi): `Api\Bcs\` + `Api\Ldi\`
   (Dtos + Apply), controllere host, blocurile ModelCheck F6-D11, openapi
   regenerat + drift verde.
3. **Pas 3 — Clientul** (Atlas.Conta.Client): feliile `bcs` + `ldi`, extracția
   `etichetaLot`, rute/meniu/rutaTip, `gen:types`, build client verde.
4. **Review advers dedicat** (agent separat, scenarii concrete de exploatare);
   fix-urile le aplică main-ul.
5. **Smoke browser** pe perechea WebApi + client (baza Privat), flux-ancoră:
   BCS cules → operat → registre; LDI plus+minus cules → operat → lot născut
   în gestiunea inventariată; comutarea de direcție; refuzurile prin dry-run.

Comenzile de verificare (main le re-rulează independent la fiecare pas):

```
cd nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module
dotnet ef migrations add LdiCulegereLot --context BackOfficeEFCoreDbContext   # doar pasul 1
dotnet ef database update --context BackOfficeEFCoreDbContext                  # înainte de ModelCheck bugetar
cd nou/tools/ModelCheck && dotnet run                    # bugetar (pe baza aplicației)
cd nou/tools/ModelCheck && dotnet run privat             # baza dedicată
dotnet run --project nou/tools/ModelCheck -- --dump-metadata                   # la orice caption nou
cd nou/Atlas.Conta.Client && pnpm verifica:drift                               # pasul 2+
cd nou/Atlas.Conta.Client && pnpm build                                        # pasul 3
```

Niciodată `--no-build` la `dotnet ef` (incident cunoscut).

## Riscurile pin-uite (review-ul advers le țintește)

1. Ramura `NasteLot == false` din serviciu: poziția față de self-healing și de
   gardul de lot străin; comutarea de direcție cu lot propriu FINALIZAT
   (operare→anulare→comutare) — ce rămâne în urmă.
2. Golirea câmpurilor de plus pe minus: Apply vs serviciu vs XAF — toate căile
   ajung la aceeași stare persistată?
3. PUT pe LDI cu lot propriu deja născut: round-trip-ul `LotId` pe Plus nu
   re-leagă / nu dublează.
4. Liniile istorice de import (frunză cu lot finalizat, `ProdusId` null;
   posibil linii de tip BAZĂ) — citire, reconciliere, self-healing.
5. Seria consumată / rânduri-fantomă la refuz (33d) pe căile noi.
6. Capcana XAF: ecranul LDI cu contract declarat — există cale prin care
   operatorul re-recepționează marfă (lot străin pe plus)? (validarea de
   operare o refuză — dar verifică că nu există cale TĂCUTĂ).

## Închidere (2026-08-13)

- [x] Contract îndeplinit; ModelCheck verde ambele profiluri (bugetar 456+,
  privat 249, 88 verificări noi ale feliei incl. probele post-review);
  `pnpm verifica:drift` verde; build client verde.
- [x] Smoke browser executat pe perechea WebApi + client (baza Privat):
  BCS-548 cules → operat (valoarea la culegere 5×0,65=3,25; −Magazie pe
  gestiune + +Consum pe SEDIU; nota 6028=3028); LDI-19 cules → operat
  (lotul plusului născut la salvare „(în culegere)" în gestiunea
  PREDATORULUI, finalizat de motor la 0,70/13.08.2026; minusul semnat −1/
  −0,65; note 3028=7588 + 6028=3028 pozitiv normalizat; Total net 0,75);
  comutatorul de direcție în ambele sensuri; precompletarea Tipului din
  produs; gardianul de sold cu mesaj de domeniu pe lotul fără sold
  (validarea empirică a F6-D8). Capcană de MEDIU prinsă la smoke: migrația
  trebuie aplicată și pe bazele de dev suplimentare
  (`dotnet ef database update --connection ...Database=Atlas.Conta.BackOffice.Privat`)
  — altfel ORICE query TPT pe detalii dă 500 (coloana nouă lipsește).
- [x] Review advers: 2 defecte de FOND fixate (F1 — minusul putea descărca
  lotul născut de linia-FRATE a aceluiași document la preț nefinalizat 0,
  gardianul de sold trecea ⇒ gardul ASM 46d replicat pe LDI; F2 — lipsea
  validarea de coerență Tip↔Produs pe plusul care naște lot ⇒ replicată de
  pe NIR) + minore: M1 golirea produsului în serviciu pe `NasteLot == false`
  (gardul mutat ÎNAINTEA gardului de lot străin — toate căile ajung la
  aceeași stare), M2 pinul golit la comutarea Plus→Minus în client, M4
  mesajul „tip vechi" atins (tipul se judecă înaintea parse-ului de
  direcție). Găurile de acoperire închise cu 6 probe ModelCheck noi
  (predatorul schimbat după naștere, lotul-frate, coerența, comutarea cu
  lot finalizat + curățenia „fără urme" la Sterge).
- Datorii/limitări documentate:
  - M3: lotul propriu FINALIZAT rămâne orfan după comutarea de direcție
    (operare→anulare→comutare) — inofensiv (sold 0, nerefolosibil pe plus);
    curățenia „fără urme" îl culege abia la ștergerea documentului;
  - M5 (UX XAF): pin de lot străin cules pe Plus înainte de alegerea
    direcției nu mai poate fi scos decât comutând dus-întors pe Minus —
    refuzul de operare e zgomotos, capcană tăcută nu există;
  - filtrarea laturilor interne pe `Calitati` (F6-D8) — extensie viitoare;
    un purtător de LocConsum/Comisie care nu e UnitateInterna nu e
    selectabil din client;
  - retrofit `MaterializeazaValori` pe BTR (F6-D6) — minor;
  - lookup-urile pe linia EXISTENTĂ afișează placeholder până la
    deschiderea dropdown-ului (rezolvarea lazy a display-ului; moștenit
    din BTR) — item lista-react;
  - `window.confirm` la Șterge (moștenit NIR) — item existent lista-react;
  - DEC (F6-D12) — felie viitoare.
