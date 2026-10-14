# TR-D9b, pasul 7 — membrii de tip `SensTva` își iau numele întreg

Tip: analiza
Data: 2026-10-13
Alimentează: contractul TR-D9b, precizarea pasului 7; decizia 120 (a); glosarul, „sens”

[Contractul feliei](tr-d9b-contract.md). Baza: `e473c9ed`, pasul 6. Cerut de
owner în timpul pasului 6: `Sens` rămâne numele debitului și al creditului,
iar membrul care poartă sensul taxei se numește `SensTva`. Pasul nu schimbă
comportament fiscal; schimbă nume de membri, trei coloane, doi indecși și
șase câmpuri de API.

## Schimbarea

Selecția s-a făcut pe simboluri C#, cu aceeași unealtă Roslyn ca la pasul 6.

| Ce | Cât |
|---|---|
| Înlocuiri pe simboluri | 275, în 37 de fișiere |
| din care pe membri, parametri și variabile de tip `SensTva` | 237 |
| din care pe cele șase câmpuri text de API | 38 |
| Declarații redenumite | 46: 40 de tip `SensTva`, 6 câmpuri text |
| Pe proiecte (fișiere) | Module 18, ModelCheck 17, Nucleu 1, Import1C 1 |

Membrii persistați sau publici: `CodTva.SensTva` în nucleu,
`MapareD300.SensTva`, `MapareD394.SensTva`, `MapareTvaSaft.SensTva` și
`Fiscale.SensTva` în Module. Restul sunt înregistrări de diagnostic, tupluri
de seed, parametri și variabile locale.

Câmpurile text duc pe API numele membrului din `SensTva` și nu au tipul
enumului, deci au fost numite explicit: `D300Nemapat`, `D394Operatiune`,
`D394Neinclus`, `DecontTvaRand`, `RectificativaTvaRand`, `SaftNeinclus`.

De mână:

- `ProfilPrivat.PerechiRefuzate` citea cheia refuzului de seed cu textul
  `"Sens"`; citește acum `nameof(MapareD300.SensTva)`.
- `ProbeHttp/stergere-fizica.py` numea coloana în SQL.
- un comentariu din `Politici.cs`, unul din `TvaControllere.cs` și mesajul
  unei verificări din `VerificaF24Gardian.cs` numeau membrul vechi.
- cele trei proprietăți de mapare au captionul „Sens TVA”. Fără el, XAF ar fi
  derivat „Sens Tva” din nume; înainte captionul era „Sens”.

## Schema

Migrația `20261013082803_SensTvaPeMapari`, incrementală (120 a), generată de
EF fără SQL scris de mână: redenumește coloana `Sens` în `SensTva` pe
`MapariTvaSaft`, `MapariD394` și `MapariD300` și indecșii
`IX_MapariD394_TipTvaId_Sens` și `IX_MapariD300_TipTvaId_Sens_RandId`.
Aplicată peste `SensPePostare` pe baza bugetară `.Pas6`; baza privată a
fost migrată de ModelCheck.

## API și client

`openapi.json`, `api-types.ts` și `metadata.json` sunt regenerate: 12 câmpuri
`Sens` devin `SensTva` (șase pe schemele celor trei mapări, șase pe
proiecții) și trei membri în metadata, cu captionul nou.

Paginile React aduse la câmpul nou: `MapariD300`, `MapariD394`,
`MapariTvaSaft`, `BandaRectificativa`, `D300`, `D394`, `DecontTva`, `Saft`;
în `JurnalTva` tipul local se numește `SensTva`. Captionurile scrise în
paginile de TVA rămân „Sens”.

## Consecința pe refuzurile de seed

Cheia unui `RefuzSeed` este JSON-ul coloanelor din indexul unic, cu numele
lor. Un refuz scris pe o mapare D300 sau D394 înaintea acestui pas poartă
cheia `Sens` și nu mai potrivește rândul: seed-ul ar recrea rândul șters, iar
`PerechiRefuzate` ar pica pe cheia lipsă. Migrația nu transformă date
(102 e), deci baza care ține asemenea refuzuri se recreează (102 b). Regula
e în [tva](../stare-curenta/tva.md).

## Verificarea

| Probă | Rezultat | Unde |
|---|---|---|
| Build ModelCheck, Import1C, WebApi | zero erori; aceleași nouă avertismente | — |
| Nucleu | **199/199** | `run-verificari/20261013-112945-637/` |
| Integrala, ambele profiluri | **3.759 bugetar / 5.161 privat OK**, zero FAIL | `run-verificari/20261013-112954-429/` |
| Metadata clientului | regenerată; proba „Metadata clientului e la zi” verde în integrală | — |
| Clientul | `pnpm build` (`tsc -b` și `vite build`) verde | — |
| HTTP pe host izolat | **327/327** `refuzuri.ps1`, plus SC-D9B-10 | `run-verificari/20261013-113445-406/` |

Înaintea corecțiilor din client, `tsc -b` a dat nouă erori, toate pe
câmpurile redenumite; după ele, niciuna.

## Limite

- Câmpurile redenumite nu au fost citite dintr-un răspuns HTTP viu: numele
  lor este dovedit de OpenAPI regenerat și de compilarea clientului contra
  lui. Matricea `refuzuri.ps1` nu cheamă proiecțiile de TVA cu date.
- Paginile React atinse nu au fost deschise în browser.
- Parametrul de interogare `sens` al jurnalului de TVA păstrează numele;
  la fel îl folosesc partidele și documentele cu rest pentru sensul
  stingerii.
- Membrii de tip `SensStingere` numiți `Sens` (37 de folosiri, cu câmpurile
  text `Sens` din `NotaContabilaDtos` și `ImperecheriProiectii`) nu au fost
  ceruți și nu sunt atinși.
- Comparația D9b nu a fost rerulată: SQL-ul cititorului A nu numește
  coloanele mapărilor.
- Import1C compilează; nu a fost executat.
- Fără review advers.
