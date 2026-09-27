# CLAUDE.md — Atlas.Conta: contabilitate/gestiune (Delphi + SQL → XAF + React)

> Fișierul de față ține doar ce e adevărat în ORICE sesiune: contextul,
> harta surselor de adevăr, principiile transversale, starea și regulile de
> lucru. Regula durabilă a fiecărei decizii stă în fișierul deciziei
> (`docs/decizii/NNN-*.md`, secțiunea „Regula durabilă"), regulile pe arii în
> `docs/stare-curenta/`, restanțele în `docs/decizii/restante.md`. Nimic de
> aici nu se citește ca „decizia N" fără a deschide fișierul ei.

## Context general

Rescriem o aplicație de contabilitate/gestiune scrisă în Delphi + SQL Server.
Aplicația veche folosește un model generic de documente configurat prin tabele
(EAV-like); generalizarea nu s-a plătit: ușor de implementat inițial, foarte
greu de întreținut, cu un număr mic și stabil de tipuri de documente.
Direcția de produs e **privat-first** (decizia 35): produsul privat (OMFP 1802)
e sursa de cerințe; profilul bugetar rămâne pachet de seed funcțional.

## Arhitectura veche (legacy, în /legacy și /db)

Tabelă unică de documente + tabelă unică de detalii cu `TipDocument`, document
generic predator → primitor configurat per combinație tip × predator ×
primitor; Clasă/Tip pe produse ≈ plan de conturi operațional; stocurile = semn
aplicat pe cantitate per tip × filtru. Statut: **evidență, niciodată canonic**
(21); istoricul de documente rămâne în legacy (18). Inventarul complet e în
`db/inventar/`.

```
/legacy   → surse Delphi (.pas, .dfm) + scripturi SQL vechi
/db       → export schemă + conținutul tabelelor de configurare (SQL Server local,
            Contabilitate_2026); inventarul legacy în db/inventar/
/nou      → soluția nouă: BackOffice (XAF Blazor + Module), WebApi, Client (React),
            tools/ (ModelCheck, Migrare, Import1C, BackfillTva, ProbeHttp)
/docs     → invarianți, jurnal, stare curentă, design-uri și contracte per felie
```

## Harta surselor de adevăr

Straturile cunoașterii (2026-09-09): invarianții = ce trebuie să rămână
adevărat; stare-curenta = CE și CUM, pe responsabilități, fără istoric;
decizii = DE CE, integral; codul = implementarea. O regulă are o singură
descriere detaliată, în pagina responsabilității ei; puntea spre jurnal e
identificatorul între paranteze (`(42b)`, `(76-r1)`).

| Întrebarea | Unde |
|---|---|
| Ce trebuie să rămână adevărat | `docs/invarianti.md` (6 invarianți, 2026-08-02) |
| Regulile și acoperirea curentă, pe responsabilități | [docs/stare-curenta/README.md](docs/stare-curenta/README.md) → domeniu-si-operare, politici-si-fiscalitate, api-si-client, dezvoltare-si-validare, limite-curente |
| Regula durabilă a deciziei N („decizia 42c") | `docs/decizii/042-*.md`, secțiunea „Regula durabilă", sub-punctul (c); antetul spune dacă e activă/amendată/depășită; indexul cu stări în `docs/decizii/README.md` |
| De ce e așa (context, tranșări, review) | același fișier, sub „Regula durabilă" |
| Restanțele și amânările cu nume (backlog) | `docs/decizii/restante.md` (id → decizie; starea deschisă/închisă) |
| Istoricul de execuție (feliile, în ordine) | `docs/decizii/istoric-plan-de-lucru.md` |
| Testul bazei (câmpurile `Document`/`DocumentDetaliu`) | `db/inventar/11-testul-bazei.md` |
| TVA structural / descărcarea de gestiune (privat) | `docs/privat/p1-tva-design.md`, `p2-descarcare-design.md` |
| Conectorul 1C și contractul de reconciliere | `docs/import/faza-1c-design.md` |
| Catalogul de scenarii (proba supremă a motorului, 91) | `docs/nucleu/scenarii/README.md` |
| Gate-ul XAF; dimensiunile pe frunze | `docs/gate-xaf-contract.md`; `docs/dim/dim-2-inventar.md` |
| Tierul API / clientul React (design) | `docs/api/p5-api-design.md`, `p5-react-design.md` |
| Contractele feliilor pasului 5 (D-urile pin-uite) | `docs/api/p5-*-contract.md` |
| Perf pe baza de import | `docs/api/p5-perf-masuratori.md` |
| Luptele structurale cu XAF Blazor (culegerea rămâne în XAF, 104d) | `docs/api/lista-react.md` |
| Fluxul comenzilor online (bifurcație deschisă) | `docs/architecture-notes-2026-07-28.md` |

## Principii transversale (valabile în orice arie)

Fiecare are textul complet în decizia din paranteză; aici doar cât să nu fie
contrazise din neatenție.

- **Nucleu generic + moștenire TPH**: `Document` + `DocumentDetaliu` de bază,
  derivate per tip; motoarele de stoc și contabile consumă DOAR baza (1–3, 22).
  Discriminatorul `ClrType` e etichetă, nu comutator; o coloană de frunză se
  citește doar pe o mulțime restrânsă pe tip sau prin `is ? :` — `as`/cast
  NU filtrează pe tip (89b).
- **Structura devine cod, politica rămâne date**: câmpurile per tip = clase;
  conturile, stocul, numerotarea, conexul, scadența, TVA-ul, validările = tabele
  de politică editabile fără release. Politica nu inventează comportament (4).
- **Motorul e agnostic la plan și nu cunoaște frunzele**: niciun simbol de
  cont hardcodat (29); ce are nevoie de la un tip primește prin hook polimorf
  sau interfață declarată, niciodată prin `is`/`switch` (25b, 32a).
- **Registre persistate, append-only**: `Draft → Operat → (Stornat)`;
  `Opereaza` = calculează → validează → materializează; perioada închisă =
  graniță absolută; corecție directă doar fără dependenți (14, 33d).
- **Evaluare pe lot, FIFO**, o singură metodă în motor (13, 51d).
- **Bază-per-client, profil = pachet de seed**; profilurile diferă de
  conținut, nu de mecanisme; seed-ul aliniază DOAR rândurile `DinSeed` (29,
  35d, 84a).
- **O singură sursă de reguli**: gardianul de Committing pe ObjectSpace-uri
  SECURED; non-secured = ușa de sistem; motorul rulează în OS propriu, în
  secvență, prin ID (42a/b, 55a/b, 58c).
- **Citirea = registre + proiecții; scrierea = agregat per document; TS nu
  calculează niciodată sold/rest/total** (42c/d, 43).
- **Cinci straturi, dependențe într-un singur sens**: L0 Nucleu → L1
  Declarații → L2 coaja comenzii → L3 culegerea → L4 randarea; motorul (L0–L2)
  nu cunoaște securitatea XAF și nici ștergerea. L3 (`Module/Culegere/`) e
  singura sursă de implicite, recalcul și validare de domeniu; controllerele
  XAF și `Api/*Apply` sunt adaptori peste ea (104a/c).
- **Culegerea documentelor se face în XAF; React face citiri și proiecții.**
  Paginile React de detaliu ale documentelor sunt înghețate: nu primesc câmpuri
  noi și nu blochează nicio felie; `WriteDto`/`Apply` rămân, fiindcă sunt L3
  (104d).
- **Fără ștergere amânată**: draftul se șterge fizic, operatul se stornează,
  nomenclatorul se inactivează (`Activ`); `Cascade` doar în agregat (104f/g).
- **Refuzurile de acces**: 404 = inexistent sau invizibil, 403 = vizibil fără
  drept, 422 = domeniu; ordinea 401 → 400 → 404 → 403 → 422 pe toate ușile, un
  singur corp `EroriDto` (80).
- **Sursele externe (legacy, 1C, ANAF) sunt evidență, niciodată canonic**;
  diferențele se RAPORTEAZĂ, nu se ascund (21, 34f, 35b).
- **Scara numerică** cu gardian (bani 18,2 / prețuri 18,6 / cantități 18,3);
  rotunjirea = dată de profil, înghețată per bază (49e, 51c, 52a).

## Stare

**Produs (înghețat pe funcții, 90m).** Toate tipurile de document au felie
prin API și client; singurul rămas e BPR (rezervat, 19). Refuzurile de acces
sunt uniforme pe REST și OData și măsurate (80). Listele XAF Blazor pe
`Server` / `ServerView` / `Client` după 85. Imobilizările au modul propriu cu
registru append-only (87). Perioada e lanț, închiderea e comandă, soldurile se
materializează doar pe perioadele de referință și se citesc prin
`Motor/SolduriService` (88). Cele trei ierarhii sunt TPH cu discriminatorul
mapat `ClrType` (89).

**Nucleul (90, amendat de 91).** `nou/Atlas.Conta.Nucleu` e motorul pur (BCL,
zero pachete, teste de proprietate). Declarația fluxului stă în
`Module/Declaratii/` (`IDeclarant` numit prin `Document.Declarant()`, driverul
`Contractare`, laturile ca structură prin `Document.Laturi()`). Cubul e
persistat în `Module/Cub/` (`Tranzactie` / `Postare`, POCO, tabelă
partiționată pe `Spatiu`, migrații scrise în SQL) și se scrie în aceeași
tranzacție de comandă cu registrele pentru tipurile cu `PosteazaInCub` (regim
dual, dată de profil). Pe cub azi: BCS, FCT, PLT, INC, BTR, FCL, NTC,
DSC, ITV, RDC, RLF și DVI (ultimele cinci numai privat), plus ASM, LDI, NIR, DEC și PIF/AMO/CAS pe ambele profiluri;
LDI acoperă Magazie/Marfuri și lanțul Folosință în gestiune reală (093),
cu Custodie explicit neacoperită. NIR conex postează diferența față de
recepția istorică a facturii, cu proveniență păstrată la corecție (098, 099). Deschiderea generică detaliază soldul inițial
prin loturi și partide, fără dublare, cu refuz atomic al diferențelor (094).
Fișa imobilizării este citită din cub de AMO/CAS și API Imo (097).
Citirile contabile/stoc/partide și snapshot-urile lor sunt pe cub; felia
fiscală este portată (103). Restul SAF-T rămâne TR-D8; scrierea registrelor
se taie la TR-D9.

**Proba supremă (91, 2026-09-22)** e catalogul de scenarii
`docs/nucleu/scenarii/`: așteptări scrise de mână din regula contabilă, ciclul
complet per tip, lanțuri transversale, pe ambele profiluri. Import1C e unealtă
de migrare, înghețată, la sfârșit (091-r4); clona Flax e sursă de întrebări
prin recensământ, nu gate; `--declaratie-pe-baza` și `--reconciliere-cub` sunt
diagnostic. „Rotund" (regula de oprire a PoC-ului) e 091 (g). Restanțele au
patru stări (091 (k)); lista `activă` din `restante.md` e singurul backlog al
PoC-ului.

Cronologia, cifrele și contractele feliilor: `docs/decizii/istoric-plan-de-lucru.md`,
`docs/nucleu/*-contract.md`. Un rezumat de felie nu se mai adaugă aici (91l).

**Următorul pas**: felia C104 (decizia 104, branch `c104-straturi`), pasul 5:
review advers pe pașii 1–3 și închiderea 104-r1 (pașii 1–4 sunt comiși). După ea: review-ul specificației intervalelor TVA și avertismentelor
(103h, `docs/nucleu/tr-d8-tva-intervale-contract.md`), înaintea codului R6.
Apoi restul SAF-T din TR-D8 (SourceDocuments integral), reconcilierea,
auditul și gate-ul transversal de performanță. Felia fiscală
103 este implementată și verificată; snapshot-ul de stoc este pe cub;
contractul și probele sunt în `docs/nucleu/tr-d8-citiri-contract.md` și
`docs/nucleu/scenarii/CITIRI.md`.
Felia de curățenie C102 este închisă (2026-09-25, branch `c102-curatenie`):
compatibilitatea cu bazele de dezvoltare a ieșit, invarianții cubului rulează
în ModelCheck (`INV-CUB`), migrațiile s-au comprimat în `InitialCreate`, bazele
s-au recreat, 101-r1 e închisă; rămân 102-r4/r5. TR-D8 — portarea consumatorilor pe intrările comune ale
cubului, după DEC și PIF/AMO/CAS (095, 097), cu scenarii numerice înaintea
implementării. Review-urile adverse LDI, NIR, Deschidere, DEC și IMO sunt făcute
(2026-09-24); corecturile se aplică pe felii peste `94ddfa8` (098).
Urmărirea partidelor este separată de rolul comercial SAF-T (096). Transformarea ASM și absorbția
temporară Δ în regimul dual sunt aprobate și implementate:
`docs/nucleu/tr-d7b-asm-transformare-contract.md` (ASM-B2…B7).
Review-ul ASM este aplicat; delimitarea contabilă (a)/(h) este aprobată
de owner numai pentru regimul dual (T-r15, D8-B4).
RDC, RLF și DVI au cataloage independente
în `docs/nucleu/scenarii/`; DVI păstrează baza distinctă în Carte=Fiscal
conform `docs/nucleu/tr-d7b-dvi-baza-fiscala-contract.md`.
Identitatea partidei include partenerul (092);
probele și limitele pasului 3 sunt în `docs/nucleu/scenarii/NTC.md` și `ITV.md`.
Primele loturi
independente pentru tipurile deja pe cub sunt verificate; ciclurile complete
rămân deschise conform fișierelor tipurilor. Contractul feliei:
`docs/nucleu/tr-d7b-tipuri-ramase-contract.md`, amendamentul 091 sub „Pașii”.

**Capcane de probare**: o cifră de perf se compară DOAR cu ea însăși pe
ACEEAȘI bază (A/B prin schimbarea stării, nu între baze — altfel diferența de
date trece drept efect), iar o grilă paginată ascunde costul căii care consumă
TOT (`LIMIT` oprește execuția devreme, `ToList` nu); `genereaza` SCRIE ori de
câte ori luna e liberă (79); `dotnet test` pe nucleu rulează 500 de cazuri
per proprietate cu sămânța fixă `Gen.Samanta + index` — un caz picat se
reproduce izolat cu sămânța din mesaj, nu prin re-rularea suitei;
probele de securitate se rulează prin `nou/tools/ProbeHttp/refuzuri.ps1` pe
host viu (Privat, după re-seed pentru `Cititor`/`Configurator`), nu se refac
de mână; două ModelCheck-uri în paralel cer worktree + `MODELCHECK_BAZA_SUFIX`
(bazele fără sufix sunt ale unei singure rulări), iar un `dotnet build`
concurent cu un ModelCheck în rulare pică pe DLL-uri blocate — `--no-build`
pe binarul vechi probează codul vechi (verifică stamp-ul DLL-ului);
redirectarea `*>` din PowerShell scrie log-ul UTF-16 — rețeta bash
`run-nucleu/tr-d6b/pas4-final/run.sh` scrie UTF-8. ModelCheck compilează și
proiectul Blazor.Server (modelul real al hostului, 85h): build-ul lui pică pe
DLL-uri blocate cât timp hostul Blazor rulează din același `bin`. O SINGURĂ
rulare grea o dată (ModelCheck, gate, import): două ModelCheck-uri concurente
pot cădea în purje și lăsa reziduu. Purja SAF-T șterge acum și regulile
`DinSeed` atașate tipului temporar după o cădere (S-r10), fără a permite
rulări concurente. Interogările pe catalogul
Postgres cer cast explicit (`partattrs` e `int2vector` de la 0, `conkey` e
`int2[]` de la 1, `partstrat` e `"char"` ⇒ `::text`), altfel pică și opresc
rularea. O bază care nu corespunde codului se recreează, nu se repară (102b).

## Reguli de lucru comune (Claude Code și Codex)

- **Decizie nouă** = fișier nou `docs/decizii/NNN-slug.md` (numărul următor;
  antetul cu Data/Stare/Docs, apoi secțiunea „Regula durabilă" — regula, nu
  povestea, cu sub-punctele (a)–(k) — apoi textul integral: context, tranșări,
  review, ce rămâne deschis) + o linie în `docs/decizii/README.md` + un rând
  per restanță în `docs/decizii/restante.md` (numele acolo, textul doar în
  fișierul deciziei). Numerele și literele nu se renumerotează niciodată; o
  decizie depășită/amendată își schimbă `Stare:` în antet și în README
  („depășită de N"), textul nu se șterge; o restanță închisă își schimbă
  starea în `restante.md`, nu dispare. Când ai nevoie de „decizia N":
  deschizi UN fișier, nu directorul.
- **Orice propunere arhitecturală se testează întâi contra `docs/invarianti.md`**;
  o felie mare are contract scris în `docs/` (D-uri pin-uite, regulă de oprire,
  review advers la închidere) — precedentele: `docs/api/p5-*-contract.md`.
- **O schimbare de comportament actualizează `docs/stare-curenta/`** (regula,
  acoperirea, limitele, data) în ACELAȘI commit cu codul.
- **ModelCheck rămâne verde pe AMBELE profiluri** după orice schimbare de
  model/motor/politică; migrațiile EF sunt canonice (23a); `--dump-metadata`
  la orice caption nou; driftul openapi verificat (56d).
- Cazurile speciale descoperite în surse externe se raportează înainte de a
  decide unde ajung în model. La explorarea legacy: grep selectiv pe
  tabele/câmpuri, nu citit formuri la rând.
- Probele se fac pe CALEA REALĂ (66h): HTTP pentru securitate, browser pentru
  UI; schimbările de motor au ca probă supremă catalogul de scenarii
  (`docs/nucleu/scenarii/`, 91): un tip nu e „pe cub" fără ciclul complet
  verde pe ambele profiluri; Import1C nu mai e gate, e migrare.
- Rulările lungi = proces detașat + monitor, nu task de fundal al harness-ului
  (50d).
- **Comunicarea Claude ↔ Codex** trece prin `D:\Dev\Atlas.Conta\comunicari\`
  (gitignored, cale absolută și din worktree-uri): un fișier per mesaj,
  `AAAA-MM-DD-HHMM-emitent-receptor-subiect.md` (emitent/receptor: `owner`,
  `claude`, `codex`), antet cu `Răspuns la:` și `Cere:` (decizie / review /
  informare); răspunsul = fișier nou, niciodată editarea mesajului primit.
  Mesajul nu e sursă de adevăr: ce se tranșează intră în contract/decizie,
  iar o aprobare vine doar de la owner.
- **Codul e slim: „ce" și „cum" se citesc din cod, „de ce" din decizii.**
  Fără comentarii narative, raționament sau istoric în cod („review advers
  D8", „înainte era…"). XML doc pe API-ul public doar cât servește completării:
  o propoziție de contract, nu motivația. Un comentariu e permis DOAR pentru
  ce codul nu poate exprima (contract de apelant, capcană de bibliotecă cu
  sursă, decizie contra-intuitivă): o linie, cu trimiterea la decizie ca
  identificator (`// 33d`), nu cu textul ei. Un avertisment care merită păstrat
  devine PROBĂ în ModelCheck, nu comentariu. Tranziție: codul existent nu se
  curăță în masă; se taie la atingere, iar capcana reală care dispare din
  comentariu se mută în probă sau în stare-curenta în același commit.
- **CLAUDE.md rămâne mic**: aici intră doar ce e valabil în orice sesiune;
  regula unei decizii intră în fișierul ei, nu aici. Link-uri, nu `@import`
  (importul ar încărca tot conținutul în context).

## Cunoștințe utilizator (context)

Dezvoltator .NET cu experiență de producție în DevExpress XAF (Blazor Server),
Serenity, React, Flutter, EF, OData. Fluent cu pattern-ul de codegen
C#→TypeScript din Serenity (echivalentul mental al OpenAPI→TS).
Motivația migrării de pe XAF Blazor pe frontend React: limitări structurale
ale ObjectSpace-ului sincron (fără async nativ, dialoguri, extensibilitate
greoaie) — nefixabile la nivel de librărie.
Dezvoltatorul inițial al aplicației legacy.
