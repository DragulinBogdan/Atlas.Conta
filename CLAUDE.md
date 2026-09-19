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
| Gate-ul XAF; dimensiunile pe frunze | `docs/gate-xaf-contract.md`; `docs/dim/dim-2-inventar.md` |
| Tierul API / clientul React (design) | `docs/api/p5-api-design.md`, `p5-react-design.md` |
| Contractele feliilor pasului 5 (D-urile pin-uite) | `docs/api/p5-*-contract.md` |
| Perf pe baza de import | `docs/api/p5-perf-masuratori.md` |
| Ce rămâne de la XAF Blazor pentru React | `docs/api/lista-react.md` |
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
- **Refuzurile de acces**: 404 = inexistent sau invizibil, 403 = vizibil fără
  drept, 422 = domeniu; ordinea 401 → 400 → 404 → 403 → 422 pe toate ușile, un
  singur corp `EroriDto` (80).
- **Sursele externe (legacy, 1C, ANAF) sunt evidență, niciodată canonic**;
  diferențele se RAPORTEAZĂ, nu se ascund (21, 34f, 35b).
- **Scara numerică** cu gardian (bani 18,2 / prețuri 18,6 / cantități 18,3);
  rotunjirea = dată de profil, înghețată per bază (49e, 51c, 52a).

## Stare

Toate tipurile de document au felie prin API și client (ITV ca COMANDĂ, 79;
DVI ca agregat cu legătură n→m la facturi, 86); singurul rămas e BPR
(rezervat, 19). Refuzurile de acces sunt uniforme pe REST și OData și
MĂSURATE (80). Listele XAF Blazor: `Server` implicit cu paginare, `ServerView`
pe registre, `Client` explicit pe grilele de culegere, IF/IFV doar cu prag
(85). Imobilizările au modul propriu (87, 2026-09-15): fișa ca nomenclator
subțire, un singur registru append-only (`RegistruImobilizari`, al patrulea,
prin `IDocumentCuRegistruPropriu`), PIF/CAS operate, AMO generată lunar cu
trei cifre (contabil/fiscal/deductibil) din `Motor/AmortizareService.cs`,
conturile și regulile de deductibilitate exclusiv din politică versionată,
catalogul HG 2139/2004 ca date. Perioada e lanț și închiderea e comandă (88,
2026-09-17): perioade contigue cu absența = închidere, verificare → acceptare
conștientă pe cheie → închidere în tranzacție, severitatea constatărilor din
politică; soldurile și partidele deschise se materializează DOAR pe perioadele
de referință (ultima închisă + decembrie), citite printr-un singur serviciu
(`Motor/SolduriService`); `DataInregistrare` e reperul registrelor, iar `Data`
rămâne a documentului fizic; `PerioadaDeclarare` e reperul fiscal, cu
rectificativa derivată din `InchisaPrimaOara`; corecția în perioadă închisă =
storno legat + document nou cu motiv; împerecherea e fapt datat, desfăcut prin
rând invers. Cele trei ierarhii sunt TPH cu discriminatorul mapat `ClrType`
(89, 2026-09-18): tipul e dată pe rând, citit printr-un singur cititor, tipul
țintei unui FK spre frunză îl ține gardianul și îl probează ModelCheck, iar
lanțul de migrații a fost resetat la un singur `InitialCreate`. Ultima felie
închisă: 28 (89). **Decizia 90** (2026-09-20,
`docs/decizii/090-nucleu-cub-de-postari.md`, docs în `docs/nucleu/`): nucleul
devine un singur cub de postări (motor pur pe operand închis, unitatea
nominalizată numită pe linie, împerecherea ca nominalizare + document
`Împerechere`, FCT postează recepția, o singură postare de stoc, stornoul ca
tranzacție distinctă); pașii TR-D6a…D10 sunt felii cu contract propriu,
regimul dual e per `TipDocument` ca dată, iar XAF și React sunt ÎNGHEȚATE pe
funcții (90m). **Felia 29 = TR-D6a ÎNCHISĂ** (2026-09-20,
`docs/nucleu/tr-d6a-nucleu-pur-contract.md`): `nou/Atlas.Conta.Nucleu` e
nucleul pur (BCL, zero pachete, fără consumator încă) cu cubul, conservarea
structurală, unitatea/FIFO/evaluarea pe raportul curent, Hamilton, TVA per
document × cotă, `Sold`, stornoul și motorul pe declarație; invarianții
1–6 sunt proprietăți în `Atlas.Conta.Nucleu.Teste`, al 7-lea e al lui
TR-D7/D10 (N-r1). Cronologia integrală:
`docs/decizii/istoric-plan-de-lucru.md`.

**Următorul pas**: TR-D6b, declarația fluxului per tip (pilot BCS, PLT,
FCT; singurul pas care poate întoarce decizia): un declarant per frunză în
`Module` (primul și singurul consumator al nucleului) care construiește
operandul închis prin adaptorul `Fapte` și produce, din nucleu, exact
postările pe care motorul vechi le scrie azi în registre — probă pe scenele
ModelCheck ale celor trei tipuri, transformate în cub cu maparea fizicii
portată în C# ca helper de test; contract propriu în `docs/nucleu/`. De
confirmat sau întors acolo: N-r2 (capătul virtual al cantității), N-r3
(evaluarea pe raportul curent), N-r4 (TVA per document × cotă). Apoi
TR-D7…D9 (90l). Contractul IM e depășit de 90. Cererile de produs apărute între timp
(F26-r1/r8/r9/r13, F27-r11/r13/r1, 84-r5, 86-r11, 86-r13, 80-r1, 77-r1/r6)
intră în `restante.md` cu decizia lor, nu în felie (90m).

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
(bazele fără sufix sunt ale unei singure rulări). ModelCheck compilează și
proiectul Blazor.Server (modelul real al hostului, 85h): build-ul lui pică pe
DLL-uri blocate cât timp hostul Blazor rulează din același `bin`.

## Reguli de lucru pentru Claude Code

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
  UI; schimbările de motor/registre au ca probă supremă re-rularea integrală
  Import1C cu raport identic cu baseline-ul (precedentul DIM-4) — la felii
  mari, nu la orice commit.
- Rulările lungi = proces detașat + monitor, nu task de fundal al harness-ului
  (50d).
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
