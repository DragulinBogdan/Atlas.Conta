# TR-D9b, pasul 6 — membrii `Latura` ai sensului devin `Sens`

Tip: analiza
Data: 2026-10-13
Alimentează: contractul TR-D9b, precizarea pasului 6; decizia 120 (a); glosarul, „sens”

[Contractul feliei](tr-d9b-contract.md). Baza: `3698a70`. Pasul nu schimbă
comportament contabil; schimbă nume de membri, o coloană și trei captions.

## Schimbarea

Membrii care poartă debitul sau creditul se numesc `Sens`:
`Coordonate.Sens`, `Mutare.Sens`, `Postare.Sens`, `PostareVizual.Sens` și
ceilalți membri, parametri și variabile locale de tip `Sens` care se numeau
`Latura` sau `latura`. Selecția s-a făcut pe simboluri C#, prin Roslyn, pe
compilări per proiect cu referințele reale: se redenumește simbolul al cărui
tip este `Atlas.Conta.Nucleu.Sens` (sau `Sens?`), nu textul.

| Ce | Cât |
|---|---|
| Înlocuiri `Latura`/`latura` → `Sens`/`sens` | 347, în 83 de fișiere (cu rândul de mai jos) |
| Înlocuiri `RandScena.Sens` → `SensTva` | 36 |
| Declarații redenumite | 42: 23 numite `Latura` (16 fișiere), 19 numite `latura` |
| Pe proiecte (fișiere) | ModelCheck 35, Module 29, Nucleu.Teste 7, Nucleu 6, Import1C 6 |

Contractul număra 33 de declarații în 23 de fișiere, dintr-o căutare pe
text; cifrele de aici sunt ale simbolurilor.

Nu s-au atins, fiindcă nu au tipul `Sens`: latura documentului
(`LaturaDocument`, `Declaratii.Latura`, `RegulaStoc.Latura`,
`LaturaPerecheId`), `ExplicaStocDto.Latura`, tuplul `SensStingere Latura`
din `NotaContabila` și parametrii `int latura` din Import1C, care poartă
codul de debit/credit al sursei 1C.

Două ciocniri de nume, rezolvate de mână:

- `RandScena` (ModelCheck) avea deja `Sens`, de tip `SensTva`. Membrul a
  devenit `SensTva`, ca pe `Postare`. Owner-ul a făcut din asta regulă
  (2026-10-13): numele necalificat e al debitului/creditului, iar membrul
  care poartă un sens calificat îi ia numele întreg.
- în `DeclarantNotaContabila.Nominalizeaza`, localul `sens` (multiplicatorul
  de semn) a devenit `semn`.

Tot de mână: localul `laturaTert` din `SaftProiectii.PeCub` a devenit
`sensTert`; două comentarii din nucleu (`Mutare`, `Sold`) numesc sensul;
captionul coloanei din `PostareVizual` și din cele două liste de coloane ale
`ContaUiBaseline` este „Sens”, cum cere glosarul.

## Schema

Migrația `20261013073914_SensPePostare`, incrementală (120 a): redenumește
coloana pe tabela părinte `Postare` (partițiile o urmează) și coloana din
view-ul `PostareVizual`, cu `ALTER VIEW … RENAME COLUMN`. Contractul cerea
view-ul recreat; redenumirea dă aceeași definiție, verificat pe o bază de
probă: `pg_get_viewdef` al view-ului redenumit este identic cu al unuia
creat din nou peste tabela redenumită. `InitialCreate` nu se atinge.

Aplicată pe o clonă a bazei bugetare aflate la `InitialCreate`
(`Atlas.Conta.BackOffice.Pas6`): `Postare`, `Postare_Contabil`,
`Postare_Stoc` și `PostareVizual` au coloana `Sens` și nu mai au `Latura`.
Baza privată a rulării a fost creată de ModelCheck prin tot lanțul.

SQL-ul brut care numea coloana: `ScaraVolum.sql` (cinci rânduri),
`ProbeHttp/partide-cub.py` și proba SC-NIR-33 din `ScenariiNir.Review.cs`.

## Referința A

Harness-ul compară pe text ultimul SQL al cititorului A cu captura
înghețată (`SalveazaSqlD9b`, spațiile normalizate). În cele opt capturi REST
și SOLDURI numele coloanei a fost înlocuit, `p."Latura"` → `p."Sens"`, 24 de
apariții; capturile ORIGINI nu o numeau. `README.md` din
`Referinte/D9bA/` spune asta.

## Verificarea

Toate pe Postgres-ul nativ (5446), o rulare grea o dată.

| Probă | Rezultat | Unde |
|---|---|---|
| Build ModelCheck, Import1C, WebApi | zero erori; aceleași nouă avertismente ca înainte | — |
| Nucleu | **199/199** | `run-verificari/20261013-104025-008/` |
| Integrala, ambele profiluri | **3.759 bugetar / 5.161 privat OK**, zero FAIL; aceleași numere ca la `20261012-201158-086` | `run-verificari/20261013-104100-831/` |
| Metadata clientului | neschimbată după `--dump-metadata` | — |
| Drift OpenAPI | `verifica:drift` exit 0, `src/generated/` neschimbat | — |
| Comparația D9b | **48/48 perechi A = B**, niciun „SQL A diferit”; scriere 2/51 verde; **46/48 praguri**, exit 1 | `run-verificari/20261013-104824-959/` |

Integrala include `INV-CUB` (65/71 scene, mutanții uciși, zero postări și
zero origini după purjă) și `RAMURA-NUL`.

### Cele două praguri picate

| Pereche | A | B | Prag |
|---|---|---|---|
| f100 `D9B-SOLDURI-TOT-N` | 258,77 ms | 300,99 ms | 284,65 ms |
| f100 `D9B-SOLDURI-TOT-S` | 241,32 ms | 269,74 ms | 265,45 ms |

Pe lectura SOLDURI variantele A și B cheamă același cititor
(`Partide.Cumulate`); SQL-ul salvat al celor două este identic, comandă cu
comandă. Diferența nu poate veni din cod. În ambele perechi B a fost măsurat
primul. La rularea pasului 5 aceeași pereche N trecuse cu 1,1 ms
(269,14 față de 270,24). Pragul pe SOLDURI compară deci un cititor cu el
însuși și cade în zgomotul măsurării. Rularea nu a fost repetată. După
pasul 8 owner-ul a scos SOLDURI din praguri (D9B-D7, precizarea din
2026-10-13); pe cele 32 de perechi rămase cu prag, rularea de aici e verde.

## Mediul și bazele

- Baza de comparație cere numele `.CodexD9bB` și zero postări. Baza
  pasului 5 a fost redenumită `Atlas.Conta.ModelCheck.Privat.CodexD9bB.v2`
  (schema dinaintea pasului, 5.001.179 postări), după precedentul `.v1`.
  `.CodexD9bB` este acum baza acestei rulări, pe schema nouă. La cererea
  owner-ului, `.v1` și `.v2` au fost șterse în aceeași zi, cu bazele
  `.CodexD9bRename` și `.CodexD9bAIntrerupt`.
- Bazele `.Pas6` (bugetar și privat) sunt ale integralei acestui pas.
- `Atlas.Conta.BackOffice` de pe 5446 și bazele hosturilor de pe 5444 nu au
  fost migrate.
- Prima generare OpenAPI a picat cu containerul 5444 oprit și a trecut cu
  șirul de conexiune îndreptat, prin mediu, spre baza privată `.Pas6`.
  Repetată cu containerul pornit, fără nicio suprascriere, a trecut pe baza
  hosturilor rămasă la `InitialCreate`. Regula e în
  [dezvoltare-si-validare](../stare-curenta/dezvoltare.md#contracte-generate-și-build).

Unealta de redenumire și ieșirile ei sunt în `run-verificari/d9b-pas6/`
(`Program.cs`, `preview.json`, `applied.json`), neversionate.

## Limite

- Membrii mai vechi de tip `SensTva` numiți `Sens` (`CodTva.Sens`, mapările
  D300, D394 și SAF-T, proiecțiile fiscale) nu sunt redenumiți aici. Owner-ul
  a cerut pasul imediat următor pentru ei; inventarul pe simboluri dă 40 de
  declarații și 237 de înlocuiri în 34 de fișiere, dintre care trei coloane
  cu indecși.
- Proba HTTP (`D9bHttp`, `refuzuri.ps1`) nu a fost rulată: pasul nu schimbă
  niciun câmp de API, iar driftul OpenAPI este zero.
- Import1C compilează; nu a fost executat.
- Fără review advers.
