# TR-D9b, pasul 8 — sensul stingerii și parametrii de interogare își iau numele întreg

Tip: analiza
Data: 2026-10-13
Alimentează: contractul TR-D9b, precizarea pasului 8; glosarul, „sens”

[Contractul feliei](tr-d9b-contract.md). Baza: `eefaa6fc`, pasul 7. Cerut de
owner după pasul 7, cu parametrii de interogare incluși. Pasul nu schimbă
comportament; schimbă nume de membri, trei câmpuri de API și trei parametri
de interogare. Sensul stingerii nu este persistat, deci nu există migrație.

## Schimbarea

Selecția s-a făcut pe simboluri C#, cu unealta Roslyn a pașilor 6 și 7.

| Ce | Cât |
|---|---|
| Înlocuiri pe simboluri | 79, în 11 fișiere (Module 7, ModelCheck 4) |
| din care pe membri, parametri și variabile de tip `SensStingere` | 42 |
| din care pe cele patru câmpuri text | 37 |
| Declarații redenumite | 22: 18 de tip `SensStingere`, 4 câmpuri text |

Cele de tip `SensStingere` sunt parametri și variabile locale
(`Partide.Ramas`, `Disponibil`, `Total`, `Capacitate`, `PlafonStingere`,
`Opus`, parametrii din `ImperecheriProiectii`) și două tupluri. Unul dintre
tupluri, în `NotaContabila`, numea sensul stingerii `Latura`; a devenit
`SensStingere`, ca „latură” să rămână doar a documentului.

Câmpurile text duc numele membrului din enum: `NtcContrapartidaDto`,
`DocumentCuRestRand`, `PartidaCuRestRand` (pe API) și `AntetCuRest`
(intern). Câmpurile `Sens` ale fișei de cont și registrului jurnal duc „D”
sau „C” și păstrează numele.

De mână:

- două proiecții anonime din `ImperecheriProiectii.DocumenteCuRest` citeau
  câmpul text prin nume; unealta nu le urmărește, compilatorul le-a arătat.
- parametrii de interogare din `DocumenteCuRestController`,
  `PartideController` (`sensStingere`) și `TvaControllere` (`sensTva`), cu
  mesajul de eroare care îl numește pe cel din urmă.
- `ProbeHttp`: `citiri-cub.py` citește câmpul nou, iar `fiscal-cub.py` și
  `partide-cub.py` trimit parametrii noi.
- un comentariu din `ImperechereDtos.cs` numea membrul vechi.

## API și client

`openapi.json` și `api-types.ts` sunt regenerate: trei câmpuri `Sens` devin
`SensStingere` și trei parametri `sens` devin `sensStingere` (doi) și
`sensTva` (unul). Metadata clientului nu se schimbă.

În client: `NtcDetaliu` și `Partide` citesc câmpul nou; `Partide` își ține
filtrul în URL sub `sensStingere`, fiindcă starea paginii merge direct în
cerere; `stingeri.ts` și `JurnalTva` trimit parametrii noi.

Un apelant care trimite încă `?sens=` pe partide sau pe documente cu rest
nu primește eroare: parametrul necunoscut este ignorat și lista vine
nefiltrată. Pe jurnalul de TVA primește 400, fiindcă `sensTva` e obligatoriu.

## Verificarea

| Probă | Rezultat | Unde |
|---|---|---|
| Build ModelCheck, Import1C, WebApi | zero erori; aceleași nouă avertismente | — |
| Integrala, ambele profiluri | **3.759 bugetar / 5.161 privat OK**, zero FAIL | `run-verificari/20261013-115203-880/` |
| Clientul | `tsc -b` fără erori; `pnpm build` verde | — |
| HTTP pe host izolat | **327/327** `refuzuri.ps1`, plus SC-D9B-10 | `run-verificari/20261013-115710-093/` |
| Legarea parametrilor pe host viu | **9/9** | `run-verificari/d9b-sensstingere/legare-http.log` |

Proba de legare, pe același host izolat și baza `Atlas.Conta.Http.Privat.Pas6`,
cu utilizatorul `Admin`: pe fiecare dintre cele trei rute, valoarea validă
dă 200 și cea necunoscută 400 sub numele nou; numele vechi dă 200 nefiltrat
pe partide și pe documente cu rest și 400 cu mesajul „sensTva” pe jurnalul
de TVA. Scriptul e `run-verificari/d9b-sensstingere/legare-http.ps1`,
neversionat.

Nucleul nu e atins în acest pas și nu a fost rerulat.

## Limite

- Câmpul `SensStingere` nu a fost citit dintr-un răspuns cu date: lista de
  partide a bazei de probă era goală. Numele lui e dovedit de OpenAPI
  regenerat și de compilarea clientului.
- `citiri-cub.py`, `fiscal-cub.py` și `partide-cub.py` au fost corectate,
  dar nu rulate: cer hosturile de dezvoltare și fixture-urile lor.
- Paginile React atinse nu au fost deschise în browser.
- Import1C compilează; nu a fost executat.
- Fără review advers.
