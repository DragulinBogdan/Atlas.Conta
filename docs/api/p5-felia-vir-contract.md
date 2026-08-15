# Pasul 5, felia 7 — viramentul intern (transferul 581) (contract)

Data fixării: 2026-08-13. Șablonul consolidat al feliilor F2–F6 (contractele
`p5-felia-fct-contract.md`, `p5-felia-trz-contract.md`,
`p5-felia-ldi-bcs-contract.md`). Deciziile F7-D1…F7-D11 sunt **PIN-UITE** —
agenții de implementare nu le redeschid; orice nepotrivire cu realitatea
codului se RAPORTEAZĂ, nu se normalizează tăcut.

## Scop

Transferul de bani între conturile proprii (casă ↔ bancă, bancă ↔ bancă) —
amânarea declarată la decizia 31f („pereche PLT+INC conexă, 581 — 09 §4"), a
doua oară pe lista roadmap-ului. Legacy: `BREGISTRU.TRANSFER`/`COD_CBT`
(inventar 09 §4, „deschis: probabil pereche Plata+Incasare legate conex — de
confirmat la modelul nou"). Felia îl confirmă și îl închide.

Fluxul-ancoră: operatorul culege o **Plată** din CASA către **BANCA** (ambele
conturi proprii) → operează → motorul generează automat **latura pereche**
(draft `Incasare` autogenerat, aceleași laturi) → operatorul îi pune data
extrasului și o operează. Contabil: `581 = 5311` pe latura de ieșire,
`5121 = 581` pe latura de intrare; 581 („viramente interne") se închide singur
când ambele laturi sunt operate.

## Deciziile

### F7-D1 — Forma: pereche PLT+INC pe ACELEAȘI laturi; niciun tip nou

Viramentul NU primește tip de document propriu. Testul invariantului IV
(„diferă *schema* sau doar *politica și vizibilitatea*?"): header identic,
linii identice, ciclu de viață identic — **diferă doar contrapartida (un cont
propriu în loc de partener/angajat) și, în consecință, contul de postare**.
Ambele sunt politică/date.

Laturile perechii sunt **IDENTICE pe ambele documente**: predator = contul
propriu SURSĂ, primitor = contul propriu DESTINAȚIE. Direcția o poartă TIPUL
documentului (Plata = banii pleacă, Incasare = banii sosesc), nu laturile —
exact ca la orice plată/încasare. Nu se inversează nimic la generare.

De ce PERECHE și nu un singur document care ar posta direct `5121 = 5311`:
cele două picioare sunt confirmate de documente diferite, la date diferite
(foaia de vărsământ azi, extrasul mâine), iar fiecare registru propriu trebuie
să-și vadă operațiunea la data lui. 581 e exact contul care ține diferența de
timp. Bonus: importul viitor de extrase (amânat) aduce fiecare picior separat
— cu un singur document, extrasul contului destinație n-ar avea ce înregistra.

### F7-D2 — `NaturaClasa.Virament` — singura schimbare de MODEL

Valoare nouă în enum (cod = structură). Verificat pe toată soluția: nu există
niciun `switch` exhaustiv și niciun consumator care să presupună un set închis
de valori (Module, ModelCheck, Import1C, WebApi, client TS — dump-ul de enum e
reflectiv) ⇒ adăugarea e strict aditivă.

Semantica: „linia mută bani în interiorul patrimoniului, între două conturi
proprii" — nu e nici stoc, nici serviciu, nici cheltuială; `Tehnica` nu merge
pentru că exact acolo stă `TRZ`, iar discriminarea de care depinde contarea
(F7-D3) ar dispărea.

Clasa `VIR` („Viramente interne", Natura=Virament) + `TipMaterial` `VIR` cu
`ContImplicit` = contul de tranzit = **DATE per profil** (F7-D6). Motorul nu
cunoaște niciun simbol (decizia 29).

**Fără migrație**: `Natura` e int pe `ClasaProdus`, nicio coloană și nicio
tabelă nouă în toată felia.

### F7-D3 — Cuplajul laturi ↔ natura liniei, validat în AMBELE sensuri

Laturile PLT/INC se relaxează: contrapartida ∈ {`Partener`, `Angajat`,
`ContPropriu`}. Contul propriu rămâne obligatoriu pe latura lui (predator la
Plata, primitor la Incasare).

`DocumentTrezorerie.ValideazaOperare` capătă cuplajul, în ambele sensuri:

- contrapartidă `ContPropriu` + o linie cu Natura ≠ Virament ⇒ **refuz**;
- linie cu Natura = Virament + contrapartidă care NU e `ContPropriu` ⇒ **refuz**;
- `PredatorId == PrimitorId` ⇒ **refuz** (virament către sine).

Fără cuplaj, un virament cules din greșeală cu Tipul `TRZ` cade pe regula
generică a tipului și postează `5121B = 5311A` pe FIECARE picior ⇒ **dublă
postare tăcută**. Cuplajul e formulat pe `NaturaClasa` (cod), nu pe coduri de
Clasă/Tip sau simboluri de cont — zero scurgere de profil în clasă.

Al treilea refuz, oglinda precedentului 38c („linie de stoc FCL / linie DSC
fără regulă de contare per-Tip = refuz explicit"): **linia de Natura=Virament
fără regulă de contare potrivită pe Tip sau pe `NaturaFiltru=Virament` ⇒
refuz**. Motivul e identic și e cel mai periculos scenariu al feliei: potrivirea
de reguli cade la final pe regula GENERICĂ a tipului (`TipMaterialId == null &&
NaturaFiltru == null`), deci un profil cu Tipul `VIR` seed-uit dar fără rândul
de regulă ar posta din nou `5121B = 5311A` pe ambele picioare, fără niciun
zgomot.

Toate verificările lucrează pe FK-uri + `IObjectSpace`, fără navigații lazy în
enumerare (25b) — natura liniilor se preîncarcă exact ca în motor.

### F7-D4 — Generarea laturii pereche = `GenereazaSecundar`, nu `PoliticaConex`

Hook-ul secundar (precedentul 31e — plata automată din FCT) pe
`DocumentTrezorerie`, o singură implementare simetrică:

```
protected abstract DocumentTrezorerie CreeazaPereche(IObjectSpace os);
// Plata → os.CreateObject<Incasare>();  Incasare → os.CreateObject<Plata>()
```

(contract, nu `is`/`switch` pe tip în clasa de bază — invariantul II).

```
public override Document GenereazaSecundar(IObjectSpace os) {
    if (Autogenerat) return null;                       // vezi mai jos
    if (os.GetObjectByKey<Repartitor>(GetContrapartidaId()) is not ContPropriu) return null;
    ...
}
```

Se copiază: `Data`, `TipInstrument`, **laturile ca atare** (NEinversate),
liniile (Tip, `Valoare`, `AngajamentId`, dimensiunile prin contractul
`DimensiuniCulese()`/`PreiaDimensiuni()` — DIM-1), pe frunza
`DocumentTrezorerieDetaliu`.
NU se copiază: `Numar` (server-owned, se asignează din seria proprie la
operarea laturii pereche), `NumarExtras`/`DataExtras` (fiecare picior are
extrasul lui).

De ce nu `PoliticaConex`: (1) ar cere DOUĂ rânduri (PLT→INC și INC→PLT) și
ambele deschid **ping-pong infinit** — `GenereazaConex` nu are niciun gard
contra recursiei, iar operarea copilului ar genera din nou un copil; (2)
filtrarea lui e pe `NaturaFiltru`, adică exact discriminarea pe care o avem
deja, dar fără gardul de recursie; (3) generarea laturii pereche nu e o
alegere de profil (convenția tranzitului implică două picioare, punct), deci
n-are ce căuta ca rând de politică — invariantul IV.1.

Gardul `if (Autogenerat) return null` e OBLIGATORIU și trăiește în hook (local,
explicit), nu ca regulă globală în motor.

### F7-D5 — Fix de FOND în motor: imperecherea automată doar dacă documentul STINGE

Pasul 6 din `MotorOperare.Opereaza` („plata autogenerată își creează
imperecherea cu sursa la propria operare") se declanșează azi pentru **orice**
`DocumentTrezorerie` autogenerat cu sursă. Latura pereche a viramentului e
exact asta ⇒ fără fix, operarea ei ar crea o imperechere PLT↔INC cu propria
sursă (invarianții o permit: sensuri opuse, contrapartidă comună), care ar
**bloca anularea și stornarea ambelor picioare** și ar polua panoul de stingeri.

Fix, polimorf (fără `is` pe tip în motor):

- `DocumentTrezorerie.CapacitateStingere(os)` ⇒ **`null` pe virament**
  (contrapartida e un cont propriu — un virament nu stinge nicio datorie și nu
  poate fi stins: contrapartida unei plăți normale nu apare niciodată pe
  laturile lui);
- pasul 6 se condiționează pe `CapacitateStingere(os) != null` înainte de a
  calcula suma.

> **Amendament (review advers, F1/F3)** — argumentul din paranteză e FALS pentru
> `NotaContabila`: capacitățile ei sunt repartitorii EXPLICIȚI ai liniilor (49a),
> deci pot cădea pe orice latură, inclusiv pe conturile proprii ale unui picior
> de virament. „Nu poate fi stins" devine hook propriu — `Document.PoateFiStins`
> (default `true`, override `!EsteVirament` pe trezorerie), consultat de
> `ImperechereService.ValideazaCreare`. Poarta pasului 6 întreabă AMBELE jumătăți
> ale rolului: „copilul stinge?" (`CapacitateStingere`) **și** „sursa se lasă
> stinsă?" (`PoateFiStins`) — altfel o pereche editată până nu mai e virament ar
> trece poarta și ar arunca un refuz de imperechere pe calea operării.

Nonregresia obligatorie: plata autogenerată din FCT își creează în continuare
imperecherea automată (probă dedicată în ModelCheck).

### F7-D5b — Dimensiunea Repartitor pe viramente: contul propriu al piciorului

`Document.RepartitorImplicitDebit()/Credit()` primesc `IObjectSpace os`
(aliniere cu TOATE celelalte hook-uri ale motorului — Document.cs §101-103:
„primesc IObjectSpace și lucrează pe FK-uri"; cele două erau singura excepție).
Refactor mecanic: bază + 2 override-uri existente (`Decont`,
`DescarcareGestiune`) + 4 situri de apel în `MotorOperare`. Semantica lor
rămâne EXACT cea de azi.

`DocumentTrezorerie` face override: pe virament, **ambele laturi** primesc
contul propriu AL PICIORULUI (predatorul la Plata, primitorul la Incasare);
altfel default-ul bazei, neatins.

Motivul e de corectitudine, nu de estetică: default-ul (debit←Predator,
credit←Primitor) pune pe rândul de BANI contrapartida. La o plată normală
contrapartida e un partener, deci rândul 5xx nu se atribuie greșit altui cont
propriu. La un virament între două conturi pe același simbol sintetic (două
bănci, ambele 5121 — decizia 10: analiticele se derivă din dimensiuni) ieșirea
lui A s-ar atribui lui B și intrarea lui B lui A: soldul per cont propriu iese
**exact inversat**. Precedentul mecanismului e vechi: `Decont` (32c) și
`DescarcareGestiune` (37a) au deja override pe credit din exact același motiv
— „contul creditat aparține predatorului".

PLT/INC **normale rămân neatinse** — convenția lor de azi nu se schimbă în
felia asta (vezi §Constatări).

### F7-D6 — Seed: date, ambele profiluri

Per profil (`ProfilPrivat` + `ProfilBugetar`), în `SeedPoliticiTrezorerie`:

- Clasa `VIR` („Viramente interne", `Natura = Virament`) + `TipMaterial` `VIR`
  cu `ContImplicit` = **`581`** — sinteticul, în ambele planuri (CPLAN are
  `581` cu `Defalcare=S`; OMFP are `581` frunză). Analiticul nu se persistă
  (decizia 10); `ContImplicit` se pune EXPLICIT (precedentul `S371`, 52b) —
  derivările din simbol nu ating codul „VIR".
- `RegulaContare` PLT × TipMaterial=VIR: debit `SursaCont.TipMaterial` (581),
  credit `SursaCont.RepartitorPredator` **fără fallback** (cont propriu fără
  cont = eroare clară la operare — precedentul 31c).
- `RegulaContare` INC × TipMaterial=VIR: debit `SursaCont.RepartitorPrimitor`
  fără fallback, credit `SursaCont.TipMaterial`.

Gardurile de idempotență devin **per rând**: azi
`if (os.FirstOrDefault<RegulaContare>(x => x.TipDocument.Cod == "PLT") == null)`
ar sări rândul nou pe orice bază existentă. Șablonul e cel de la NIR/FCT (garda
pe cheia rândului, seed incremental la fiecare updater).

Fără `PoliticaConex`, fără `PoliticaValidare` nouă, fără `PoliticaTva`
(viramentul nu poartă TVA), fără regulă de stoc (Natura ≠ Stoc ⇒ motorul nu
caută nimic).

Notă bugetar: `PoliticaValidare.CereClasificatieBugetara` pe PLT rămâne cum e
— la bugetar linia de virament va cere angajament SAU cod economic, ca orice
linie de plată. E politică editabilă ca dată, nu se atinge speculativ.

### F7-D7 — API: aproape nimic

Rutele, `TrzWriteDto` și `TrezorerieApply` rămân **neschimbate** (contrapartida
e deja un `Guid` liber; validarea de laturi trăiește în motor). Singura
adăugire: **`EsteVirament` (bool) în `TrzReadDto`** — affordance calculată pe
server (contrapartida e un `ContPropriu`), din care clientul decide forma
ecranului.

`DocumenteCuRest` rămâne **NEATINSĂ**, deliberat: un picior de virament are
Rest > 0 pe veci, dar apare doar când proiecția e filtrată pe o contrapartidă
care e cont propriu (imposibil pentru documentele care se sting), iar clientul
cere oricum `tipuriCandidate = ['FCT','FCL','DEC']`. Panoul se ascunde pe
virament (F7-D8), deci nu există cale prin care numărul mincinos să ajungă sub
ochii operatorului. Se documentează în cod, nu se improvizează un anti-join.

> **Amendament (review advers, F1 corolar)** — „imposibil structural" e fals:
> pe ramurile PLT/INC contrapartida CHIAR e un cont propriu (primitorul plății,
> predatorul încasării), deci panoul deschis pentru un alt document de trezorerie
> le-ar fi întors. Ascunderea panoului e formă de ecran, nu garanție de date.
> Anti-join-ul pe tabela mică `ContPropriu` s-a aplicat pe cele două ramuri, în
> aceeași formă cu predicatul domeniului (AMBELE laturi conturi proprii).
> Adăugat tot atunci: `EsteVirament` și în `TrezorerieListDto` — fără el
> picioarele transferului sunt indistinguibile în grilă de plățile reale.

### F7-D8 — Client: al treilea fel de contrapartidă

- `felii/trz/LaturaContrapartida.tsx`: al treilea fel `ContPropriu`, dedus pe
  documentele existente cu `existaInSet` (`nucleu/sonda.ts`) în ordinea
  Angajat → ContPropriu → Partener (`ContPropriu` e deja expus ReadOnly în
  OData — F3-D6, fără adăugiri).
- În modul virament: Tipul liniei se precompletează cu `VIR` în loc de `TRZ`
  (generalizarea `tipTrz.ts` → `idTip(cod)`, memoizat per cod — convenția de
  client, ca `TRZ` azi), **panoul de stingeri se ascunde** (`EsteVirament`),
  iar în locul lui apare indiciul „Virament intern — la operare se generează
  automat latura pereche" + link-ul către latura pereche (`Copii[]` pe sursă,
  `DocumentSursaId`/`DocumentSursaTip` pe latura generată; `rutaTip` există,
  61a).
- Convențiile cimentate se respectă integral: garda `if (e.event)`,
  `laSelectie` = notificare + update funcțional, `String()` pe Guid-uri,
  formularul = sursa de adevăr, zero calcul de domeniu în TS.

### F7-D9 — ModelCheck: bloc `E2E-AVIR`, pe profil BUGETAR + privat verde

Ambele profiluri au politici ⇒ blocul rulează pe suita bugetară (ca BCS/LDI,
F6-D11), cu `dotnet run privat` verde la fiecare pas. Șablonul feliei 6
(fixture + `DryRunTrz` cu OS propriu + `CheckRefuza` + curățenie finală cu
assert). Ancorele obligatorii:

1. virament cules prin `TrezorerieApply.Aplica<Plata>` (CASA → BANCA, linie
   `VIR`) → `Citeste` întoarce `EsteVirament = true`;
2. refuzurile prin dry-run, **fără rânduri-fantomă și fără serie consumată**:
   linie `TRZ` pe contrapartidă cont propriu; linie `VIR` pe contrapartidă
   partener; `Predator == Primitor`; linie `VIR` fără regulă de contare
   potrivită (regula ștearsă temporar în OS-ul probei);
3. operare → EXACT două rânduri de registru contabil (`581` debit / contul
   contului propriu sursă credit), **zero** rânduri de stoc, dimensiunea
   Repartitor = contul propriu al piciorului pe AMBELE rânduri (F7-D5b);
4. latura pereche în `Copii[]`: `Incasare`, `Draft`, `Autogenerat`, laturi
   IDENTICE cu sursa, `Numar` gol, `NumarExtras`/`DataExtras` goale, liniile
   clonate cu dimensiuni;
5. operarea laturii pereche → `5121 = 581`, **zero imperecheri create**
   (F7-D5), soldul contului 581 = 0 după ambele picioare;
6. **nonregresie F7-D5**: FCT cu `GenereazaPlata` → plata autogenerată operată
   ÎȘI creează în continuare imperecherea automată;
7. anularea laturii pereche, apoi anularea sursei (draftul regenerat/șters de
   gardienii de grup existenți); storno pe ambele picioare;
8. gardul de recursie: latura pereche operată NU generează la rândul ei un al
   treilea document.

### F7-D10 — Ce NU intră în felie

Importul extraselor de trezorerie (`xml_trezor`, 09 §3 — rămâne la fluxurile
de import); imperecherea pe poziții; ordinele de plată (`ORDINE_PLATA`,
09 §4); un mecanism dedicat pentru **comisionul bancar** (latura pereche cu
sumă mai mică e permisă tehnic — draftul e editabil —, iar reziduul rămâne
vizibil pe 581 și se închide cu o notă contabilă `627 = 581`; dacă apare
cerința reală, mecanismul e aditiv); viramentul în valută.

### F7-D11 — Pașii de implementare (un agent per pas, secvențial)

1. **Pas 1 — Module: modelul + motorul + seed-ul** — F7-D2, F7-D3, F7-D4,
   F7-D5, F7-D5b, F7-D6 + `--dump-metadata`. Verificare: build, ModelCheck
   AMBELE profiluri verde cu blocurile existente NEATINSE (fără migrație).
2. **Pas 2 — API + ModelCheck** — `EsteVirament` în `TrzReadDto` (F7-D7),
   blocul `E2E-AVIR` complet (F7-D9), openapi regenerat + `verifica:drift`.
3. **Pas 3 — Clientul** — F7-D8, `gen:types`, build client verde.
4. **Review advers dedicat** (agent separat, scenarii concrete de exploatare);
   fix-urile le aplică main-ul.
5. **Smoke browser** pe perechea WebApi + client (baza Privat): fluxul-ancoră
   CASA → BANCA, cu registrele verificate și 581 închis.

Comenzile de verificare (main le re-rulează independent la fiecare pas):

```
cd nou/tools/ModelCheck && dotnet run                    # bugetar (baza aplicației)
cd nou/tools/ModelCheck && dotnet run privat             # baza dedicată
dotnet run --project nou/tools/ModelCheck -- --dump-metadata
cd nou/Atlas.Conta.Client && pnpm verifica:drift          # pasul 2+
cd nou/Atlas.Conta.Client && pnpm build                   # pasul 3
```

Nicio migrație în felia asta — dacă un agent ajunge să scrie una, s-a abătut
de la contract și RAPORTEAZĂ.

## Riscurile pin-uite (review-ul advers le țintește)

1. **Dubla postare tăcută**: profil cu Tip `VIR` dar fără rândul de regulă
   (regula generică prinde linia); virament cules cu Tip `TRZ`; document MIXT
   (linii `VIR` + linii `TRZ`); contrapartida comutată pe un draft cu linii
   deja culese.
2. **Ping-pong-ul generării**: gardul `Autogenerat`; operatorul care culege
   MANUAL a doua latură (rezultă două picioare de intrare, 581 nu se închide —
   e vizibil sau tăcut?); re-operarea sursei după anulare (se regenerează un
   al doilea draft?).
3. **Imperecherea automată (F7-D5)**: nonregresia FCT→plata automată; un
   virament oferit ca stingător/stins pe vreo cale (API, UI XAF, panoul de
   stingeri al altui document).
4. **Grupul conex**: anularea sursei cu latura pereche Draft (ștergere) vs
   Operată (refuz); stornarea unui singur picior — ce rămâne pe 581.
5. **Dimensiunile (F7-D5b)**: refactorul de semnătură a atins semantica
   `Decont`/`DescarcareGestiune`? Rândul de tranzit și rândul de bani poartă
   același Repartitor pe fiecare picior?
6. **UI XAF** (calea vie, nu doar clientul React): ecranul PLT/INC din
   back-office permite același virament? Vreo capcană tăcută (lecția F5/F6 —
   „un gard care tace devine capcană exact pe calea unde există și
   alternativa corectă").

## Închidere (2026-08-13)

- [x] Contract îndeplinit. ModelCheck final **bugetar 491 OK / 0 FAIL**,
  **privat 249 OK / 0 FAIL** (re-rulate de main la fiecare pas, nu doar
  raportate); 34 de verificări noi în blocul `E2E-AVIR` (27 la implementare +
  7 la review). Nicio migrație, cum cerea contractul.
- [x] **Smoke browser** pe perechea WebApi + client (baza Privat de import):
  `PLT-9` cules CASA → BANCA (al treilea fel de contrapartidă, Tipul liniei
  precompletat `VIR`, panoul de stingeri ÎNLOCUIT de indiciul perechii) →
  operat: **`581 = 5311`, ambele dimensiuni pe CASA** (default-ul ar fi pus
  BANCA pe credit — proba lui F7-D5b), zero rânduri de stoc, latura pereche
  `INC` generată ca Draft cu laturi identice și linia clonată; `INC-2` operat
  → **`5121 = 581`, dimensiunile pe BANCA**; **soldul 581 = 0** pe toată baza
  și **zero imperecheri** pe ambele picioare (proba lui F7-D5).
- [x] Trei check-uri ModelCheck existente au devenit false-negative din cauza
  contractului și au fost REPARATE (nu normalizate): gărzile „Seed PLT/INC"
  presupuneau un singur rând `RegulaContare` per tip (restrânse la rândul
  GENERIC), iar proba „laturile inversate" aștepta două mesaje, din care unul
  a dispărut legitim odată cu F7-D3 (contul propriu = contrapartidă legală).
- Rafinare peste contract, aplicată de main: **`EsteVirament` cere AMBELE
  laturi conturi proprii**, nu doar contrapartida — un draft cu laturile
  inversate (plată DE LA un partener CĂTRE casă) are contrapartida
  `ContPropriu` fără să fie virament, iar predicatul e consumat de căi care
  rulează ÎNAINTE de validare (affordance-ul din API, dimensiunile notei).
  Lecția F5/F6: „un gard care tace devine capcană".
- [x] **Ștergerea laturii pereche autogenerate — tranșată: rămâne PERMISĂ**
  (`Sterge` refuză doar ne-Draft), spre deosebire de conexul NIR (62f). Acolo
  refuzul era singura apărare: factura rămânea operată fără marfă și fără
  datorie, INVIZIBIL. Aici pierderea e cel mai vizibil semnal contabil posibil
  (581 nu se mai închide), iar calea manuală reproduce EXACT același document —
  un refuz ar lăsa operatorul cu un draft de neșters după ce a cules piciorul
  celălalt de mână. Ce nu se acceptă e tăcerea: viramentul OPERAT fără pereche
  își spune starea în panou („latura pereche lipsește — 581 rămâne deschis
  până când…"), nu se ghicește din absența unei linii în listă.
- [x] **Review advers — închis**; cele 7 fix-uri aplicate, ModelCheck **bugetar
  491 OK / 0 FAIL** (484 + 7 verificări noi), **privat 249 OK / 0 FAIL**, client
  build verde, codegen idempotent. Două defecte de FOND:
  **F1** — „viramentul nu poate fi stins" era doar afirmat: `NotaContabila`
  aduce contrapartide arbitrare din liniile ei, deci o notă cu repartitorul =
  contul propriu al viramentului putea stinge un picior și îi bloca definitiv
  anularea/stornarea, invizibil (panoul e ascuns pe virament). Fix polimorf:
  `Document.PoateFiStins` + corolarul de proiecție (vezi amendamentele F7-D5 și
  F7-D7). **F2** — gardul „linia VIR fără regulă" ignora `SemnFiltru`, pe care
  motorul îl filtrează PRIMUL: un semn pus din greșeală pe rândul VIR (dată
  editabilă în XAF) trecea de gard, iar motorul cădea pe regula generică =
  exact dubla postare tăcută pe care gardul o previne. Gardul oglindește acum
  matcher-ul fidel.
  Minore, tot aplicate: poarta pasului 6 întreabă și sursa (F3, vezi
  amendamentul F7-D5); latura pereche clonează `ValoareTva` (intră în `Total`,
  iar în XAF câmpul e editabil); `EsteVirament` în `TrezorerieListDto` + coloană
  de grilă; predicatul de virament al clientului sondează AMBELE laturi, ca
  serverul; `existaInSet` propagă erorile (`undefined` = „nu știu" ≠ „nu"),
  aliniat cu promisiunea lui `useSonda`.
- Abatere de mediu, nu de contract: `Directory.Packages.props` —
  `Microsoft.Data.SqlClient` 6.1.2 → 6.1.6, planșeul tranzitiv al lui
  `Microsoft.EntityFrameworkCore.SqlServer` flotant `10.0.*` (10.0.11 a apărut
  în feed în timpul feliei); probat pre-existent pe arborele curat.

## Constatări (raportate, NEtranșate în felia asta)

- **Nu există noțiunea „acest virament are deja o latură pereche"** (review
  advers D2 — riscul pin-uit §Riscuri 2, cu răspunsul măsurat pe cod: TĂCUT).
  Dacă operatorul ignoră draftul autogenerat și culege a doua latură MANUAL, ea
  e un virament valid și își generează la rândul ei perechea (un al treilea
  document, draft), iar operarea AMBELOR picioare de intrare dublează postarea:
  581 ajunge la −500 și contul destinație e debitat de două ori. Nimic nu-l
  prinde, fiindcă nu există criteriu de CONȚINUT care să distingă cazul —
  două viramente identice între aceleași conturi, în aceeași zi, sunt perfect
  legitime. Fixul are nume: **legătura explicită de pereche** — fie un flag
  CULES `GenereazaLaturaPereche` pe header (precedentul `GenereazaPlata` al
  FCT, 31e), fie alegerea perechii la culegere. Ambele sunt aditive, dar cer
  migrație (coloană nouă) ⇒ ies din felia asta, care n-a avut niciuna.
  Mitigare de azi: draftul autogenerat rămâne vizibil pe ecranul sursei și în
  lista tipului, iar 581 nu se închide dacă lipsește un picior.
- **Dimensiunea Repartitor pe rândurile 581 nu se închide per repartitor**:
  +X pe contul sursă, −X pe cel destinație (global 0 — asertat în ModelCheck;
  analitic, niciodată). Nu e greșit contabil — tranzitul e o punte între două
  registre —, dar se declară aici ca să nu fie descoperit la prima balanță
  analitică pe 581.
- **Convenția dimensiunii Repartitor nu identifică contul propriu pe rândul de
  bani.** La orice plată/încasare normală, rândul `5xxx` primește
  contrapartida (partenerul), nu contul propriu — deci soldul per cont propriu
  se citește azi exclusiv din SIMBOLUL contului, iar două conturi proprii pe
  același sintetic (două bănci = 5121) nu se separă în balanță. Viramentul își
  rezolvă cazul local (F7-D5b), fiindcă acolo greșeala e activă (atribuire
  inversată), dar întrebarea generală rămâne deschisă: default-ul „debit←
  Predator / credit←Primitor" pune pe fiecare rând contrapartida laturii, nu
  repartitorul CONTULUI de pe acel rând. Alternativa coerentă („dimensiunea
  urmează `SursaCont`-ul laturii") ar schimba conținutul registrului pentru
  TOATE tipurile și ar cere re-validarea baseline-ului de import — decizie
  proprie, nu felie de API.
