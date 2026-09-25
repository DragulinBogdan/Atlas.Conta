# C102 — Felia de curățenie: fără compatibilitate cu bazele de dezvoltare

- Data: 2026-09-25
- Stare: contract scris, aprobat de owner ca direcție; se execută după
  commit-ul de bază al TR-D8 (Codex), într-o sesiune nouă (Claude)
- Surse: decizia 102 (a)–(g); 101-r1 redeschisă (D-2, D-3);
  `comunicari/2026-09-25-1237-claude-codex-tr-d8-partide-review.md`
- Restanțe: 102-r1, 102-r2, 102-r3, 101-r1

## Scopul

Scoatem codul și textul care există numai pentru date persistate înaintea
codului curent. Corectăm D-2/D-3 pe codul rezultat. Recreăm bazele de
dezvoltare. Nimic altceva: nu intră îmbunătățiri de model, perf sau UI.
O idee apărută pe drum se notează ca restanță.

## Pin-urile

**C-D1. Baza de pornire.** Felia pornește de la commit-ul care conține starea
TR-D8 a lui Codex și 101-r1 redeschisă. Se lucrează pe branch propriu, cu
commit per pas. Nu începe peste un working tree cu modificări străine.

**C-D2. Criteriul de includere.** Intră un simbol, o coloană, o migrație sau
un text dacă îndeplinește una dintre condiții:

- există numai pentru date scrise înaintea codului curent (102c);
- este cod mort rămas din portarea TR-D8, adică fără apelant de producție
  (hosturi, Module), dovedit prin grep sau CodeGraph.

Un apelant numai din ModelCheck nu ține codul în viață: proba se portează
pe citirea curentă.

**C-D3. Inventarul inițial (de confirmat la pasul 1, nu de extins după):**

| Țintă | Unde | Ce devine |
|---|---|---|
| Cheia veche a partidei `(document, cont)` | `IdentitatiPartide.Anterioara`, `EsteProprie`, `Partide.Origini`, `HasDbFunction cub_partida_anterioara`, migrația `CitiriPartideIstorice` | dispare; identitatea e numai 092(a) |
| Snapshot fără proveniență | `DinCub` pe `SoldPerioadaContabil`/`PartidaDeschisa`, `CITIRE_SNAPSHOT_VECHI` (`Partide.Cumulate`, `SolduriService.SnapshotContabilDinCub`, `Activare`), filtrul indexului unic, `NOT DinCub` din `DiferentePartide` | dispar; snapshot-ul e scris numai din cub |
| Legături fără efect verificat | `Imperechere.EfectCubVerificat`, ramura `suma < 0 && !desfaceNominalizare` din `Materializare.Imperecheaza`, `Partide.Diagnostic`/`Efect`, `StingeriDto.Avertismente` + randarea din `PanouStingeri` | dispar; desfacerea are două căi: transfer exact (`TranzactieCubId`) și nominalizare automată |
| Proveniența stornourilor vechi | migrația `OriginiStornoUnivoce`, `Contabil.VerificaProvenienta`/`NumaraFaraProvenienta`, `RaportSeed.PostariFaraProvenienta` | scriitorul garantează proveniența; rămâne o probă ModelCheck |
| Refuzul istoricului la pornire | apelurile `Activare.Verifica` din cele două `Program.cs`; ramurile „istoric" din `Activare`, `Partide.VerificaAcoperire`, `ImperecheriProiectii.VerificaAcoperire`, `Imobilizari.VerificaAcoperire` | vezi C-D4 |
| Storno pe document operat înainte de trecerea tipului pe cub | `Materializare.Storneaza` („Operat înainte ca tipul lui să fie migrat", S-D5) | decizia se ia pe `TipDocument.PosteazaInCub`; tip pe cub fără postări = refuz, nu `return` tăcut |
| Cod mort al portării | `ImperecheriProiectii.Asignari` (apelanți numai în ModelCheck), `AsignariDeschidere`/`AsignatDeschidere` dacă rămân fără apelant | dispar sau se justifică |

Nu intră: regimul dual al registrelor (TR-D9), `TotalStingere` (fapt de
document, cu rol separat), tipurile inerte pe bugetar (`PosteazaInCub=false`
prin profil) și refuzurile conectorului de import (102f).

**C-D4. Invarianții devin probe (102d).** Verificarea de activare nu mai
rulează în hosturi. Ce verifica ea și nu e garantat de scriere rămâne o
funcție de diagnostic, apelată din ModelCheck la finalul fiecărui profil,
pe baza rezultată:

- acoperirea registru ↔ cub pe linie și latură, cât durează regimul dual;
- echilibrul pe tranzacție și carte;
- deschiderea;
- unitățile complete ale partidelor;
- totalul de decontare față de partidele Operare;
- acoperirea imobilizărilor;
- proveniența inversei.

Probele SC-CIT-23/24/34/43 existente se păstrează, cu ținta mutată.
Mutanții lor se aplică în tranzacții anulate, ca azi.

**C-D5. Un singur calcul al disponibilului temporal.** Azi există patru
variante ale aceluiași calcul (minimul soldului peste zilele deja scrise):

- `Fapte.Sursa`;
- `Partide.NominalizataLibera`;
- `Materializare.VerificaDisponibilTemporal`;
- `Materializare.VerificaPartideFaraDependenti`.

Se unifică în `Cub.Citiri.Partide`, cu o singură semantică: zilele mai vechi
decât data cerută se lipesc de ea, iar mișcările din aceeași zi se compensează
împreună. Pe acest calcul unic se corectează D-2.

**C-D6. D-2 (101-r1).** `Fapte.PartideDisponibile` oferă declarantului NTC
disponibilul temporal pe unitate, nu soldul la data notei. Scenariul se scrie
înaintea corecturii:

1. FCT 100 la d0;
2. PLT 100 împerecheată la d3;
3. NTC la d1 cu D 401/X 100.

Așteptat: nominalizare 0 pe FCT, partidă proprie 100 pentru NTC, iar FCT
rămâne 0 de la d3. Se rulează pe ambele profiluri.

**C-D7. D-3 (101-r1).** Țintele comenzii de împerechere sunt toate unitățile
proprii ale stinsului, inclusiv cele deschise prin Transfer, aceeași mulțime
din care `Partide.Perechi` construiește candidații. Scenariul se scrie
înaintea corecturii:

1. FCT 100, PLT automată 100, DELETE pe legătură: PLT +100 proprie.
2. INC 100 de la același partener, pe același cont, stinge PLT.

Așteptat: panoul și comanda sunt de acord (candidatul 100 este acceptat), iar
restul PLT ajunge la 0. Scenariul acoperă și varianta cu NTC ca stingător.

**C-D8. Migrațiile (102e): întrebare deschisă, tranșată de owner la pasul 5.**

- **Recomandarea:** comprimăm lanțul într-o singură migrație inițială, care
  include SQL-ul brut (partiționarea cubului, cheile din SQL, funcția
  `cub_partida_id`).
- **Proba A/B:** `pg_dump --schema-only` pe o bază creată din lanțul complet
  și pe una creată din migrația comprimată, pe ambele profiluri. Diferența
  trebuie să fie zero, cu excepția `__EFMigrationsHistory`.
- **Alternativa:** păstrăm lanțul și scoatem numai transformările de date,
  care devin no-op.

**C-D9. Recrearea bazelor (102b, 102-r3).** Pasul 1 inventariază bazele
existente (`\l` pe Postgres): ModelCheck, clonele Privat, import, CodexBCS,
clonele de perf. Owner-ul confirmă lista înaintea oricărui `DROP`. Rețeta
(drop, `database update`, seed, import prin comenzi unde e cazul) se scrie
în `stare-curenta/dezvoltare-si-validare.md` și se rulează.

**C-D10. Documentele (102-r2).** În același commit cu codul pe care îl
descriu se actualizează:

- `stare-curenta` (regulile care dispar: activarea la pornire, snapshot-ul
  vechi, legăturile istorice, cheia veche);
- `CITIRI.md` și cataloagele care cer comportamentul scos;
- contractul TR-D8 (D8-B3, D8-B6 partea de istoric, D8-B7 legăturile istorice);
- `restante.md`, cu restanțele care cad marcate „depășită de 102".

Textul deciziilor vechi nu se șterge: li se schimbă numai starea.

## Pașii

1. **Inventarul.** Confirmă C-D3 prin CodeGraph și grep, plus lista bazelor.
   Stop dacă inventarul crește cu peste 30% față de C-D3: raportezi
   owner-ului înainte de a continua.
2. **Scoaterea și probele.** Scoți codul de compatibilitate și portezi
   probele (C-D3, C-D4). Build, ModelCheck selectiv CITIRI/FCT/PLT/INC/NTC.
3. **Unificarea și corecturile.** C-D5, apoi scenariile D-2/D-3 (roșii),
   apoi corecturile (verzi).
4. **Transportul.** API/client: `Avertismente` dispare, openapi/types se
   regenerează, drift-ul se verifică.
5. **Migrațiile.** C-D8 după alegerea owner-ului, cu proba A/B.
6. **Bazele și validarea.** Recreezi bazele (C-D9), rulezi ModelCheck integral
   pe ambele profiluri, pornești cele două hosturi pe o bază recreată și
   rulezi `ProbeHttp/partide-cub.py`.
7. **Închiderea.** Review advers scurt și actualizarea documentelor (C-D10).

## Regula de oprire

Felia e rotundă când:

- inventarul C-D3 e scos sau justificat rând cu rând;
- ModelCheck integral e verde pe ambele profiluri, cu D-2/D-3 verzi;
- hosturile pornesc pe o bază recreată;
- drift-ul openapi e zero;
- review-ul e făcut.

Buget: dacă pasul 2 sau 3 cere schimbare de model în afara C-D3, te oprești
și raportezi. O singură rulare grea o dată, fără ModelCheck în paralel cu
build-ul.
