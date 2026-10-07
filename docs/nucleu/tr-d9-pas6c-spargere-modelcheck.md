# TR-D9a, pasul 6c — spargerea mecanică a `Program.cs` din ModelCheck

Data: 2026-10-07. Aprobat de owner ca pas intermediar înaintea pasului 7. Execuție: agent Opus (`d9-pas6c`,
spec și raport în `run-nucleu/tr-d9a/pas6c/`, gitignored), verificare și reparațiile adiacente: main.

## Ce s-a făcut

`nou/tools/ModelCheck/Program.cs` (32.499 linii, 2,08 MB, 1.994 aserții, 94 de funcții locale la nivelul de
sus, 51 de markere de secțiune) a devenit un dispecer de 526 de linii: argumente, conexiune, comutatoarele
`--dump-*` / `--probe-sursa` / `--perf-*`, blocul de migrații, construirea `Suita`, secvența apelurilor.

- `Suita.cs` — contextul suitei (instanță, nu statică): `Check`, `CheckRefuza`, `Refuz`, `Esecuri`, `Provider`,
  `Deschide()`, `OsCuGardian()`, `Privat`, `Profil`, `ConnectionString`, `Args`, `FiltruScenarii`, `Opts`, și
  ajutoarele folosite de mai multe scene (`InchideLant`, `RedeschideLant`, `PurjaIstoricPerioade`,
  `InchideAcceptTot`, `SeveritateItvLipsa`, `SeteazaSeveritateItvLipsa`, `PrimaLinie`, `Ziua`, `Rezumat`).
- `Suita/*.cs` — 98 de fișiere: 65 de funcții locale (`Verifica*`, `FctBugetaraOperata`, `PurjaFctBugetara`,
  `ScenelePeTip`, `RuleazaScenele`…), fiecare `static class` cu `Ruleaza(Suita s, …)`, și 32 de secțiuni ale
  blocului E2E inline (`E2e*.cs`) plus `E2eCurata.cs` (singurul `Curata` și singurul marcaj folosit de două
  secțiuni). Cel mai mare fișier: `VerificaImobilizari.cs`, 1.583 de linii.
- Tăierea e făcută de script (`taie.py`), idempotent pe `ad2bad8`; zero corecturi de mână după generare.
  Multisetul literalilor de șir (13.933) și al liniilor de comentariu (3.671) e identic înainte și după.
- Nicio variabilă nu trece între secțiunile E2E: fiecare era un bloc închis. Fișierele deja extrase
  (`Scenarii*.cs`, `Probe*.cs`, `Perf*.cs`…) nu s-au schimbat pentru spargere.

## Proba

Regula a fost secvența, nu cifra: liniile `OK`/`FAIL` ale integralei, în ordine, cu GUID-urile, numerele de
document și duratele normalizate (`compara_ordonat.py`), trebuie să fie identice cu linia de bază.

Prima comparare contra `pas6b/r1-*.log` (`ea5d0bc`) a picat în două locuri, amândouă preexistente, cu dovadă
pe codul vechi (aceleași blocuri diferă între două rulări ale aceluiași commit):

1. **SC-X-14 pe BCS** emitea aserțiile în ordinea rândurilor din bază, fără `ORDER BY`
   (`ScenariiBcs.cs`). Reparat: tranzacțiile se ordonează pe `Fel` (Operare înaintea Storno); textul liniei nu
   depinde de document, deci secvența devine deterministă.
2. **Purja de scenă** compara codul dimensiunilor (`CodEconomic`, `CodFunctional`, `SursaFinantare`, `Proiect`)
   cu egalitate și scăpa sufixele (`-MIX` în `ScenariiPartideCub`, `-PR` în `ScenariiSaft`): câte un rând pe
   integrală rămânea în baza bugetară (23 → 31 între pasul 6 și 6c), vizibil în oracolul `Cautare ==
   Normalizeaza`. Reparat: `StartsWith(Marcaj)`, ca la produse și tipuri de material; niciun marcaj nu e prefixul
   altuia. Prima integrală după reparație a curățat reziduul (31 → 0).

Reparațiile sunt comise separat (`169b2fa`), înaintea spargerii. Linia de bază s-a refăcut pe codul vechi cu
reparațiile, într-un worktree la `169b2fa` (cu `ATLAS_ANAF` și `1C/mf/` copiate, altfel 14 probe DUK și
RECONCILIERE-MF pică de mediu). Rezultat: **IDENTICE pe ambele profiluri**, 3.505 bugetar / 4.784 privat, zero
FAIL. Probele ieftine (`--scenarii BCS,PLT privat`, `--probe-sursa`) IDENTICE și ele; build-ul soluției 0 erori;
nucleul 180/180.

## Abateri acceptate

- Ținta „sub 400 de linii" nu e atinsă (526): cele 46 de `using`-uri și ramurile `--perf-saft` / `--perf-cub`
  (~95 de linii, cu `return` la nivelul de sus) au rămas în dispecer. Nu merită un pas.
- Metoda se numește `Ruleaza` peste tot, inclusiv unde întoarce ceva (`FctBugetaraOperata`, `ScenelePeTip`).
- Comentariile s-au mutat ca atare, inclusiv cele narative; curățenia lor rămâne „la atingere", nu în masă.

## Material pentru mai târziu: scenele vechi fără filtru `--scenarii`

Sub filtru, prin `ScenelePeTip`, sunt numai `VerificaBalanta`, `VerificaFisaJurnal`, `VerificaSaft`,
`VerificaSaftStocuri` și `VerificaNucleu{Bcs,Btr,Trezorerie,Fct,FclDsc}` (primele două rulează și în secvență,
deci de două ori în integrală). Propunerea agentului pentru restul, nedecisă; a muta un apel în `ScenelePeTip`
îi schimbă poziția în log, deci se face cu linie de bază nouă:

| Funcția | Profil | Cod propus |
|---|---|---|
| VerificaRegistruTva, VerificaD300Seed, VerificaD394Seed, VerificaD300, VerificaD394, VerificaAxaTaxareInversa | ambele | FISCALE |
| VerificaSaftModel, VerificaMiscariSaft | ambele | SAFT |
| VerificaAdresaPartener, VerificaSincronizareAnaf | ambele | X (ANAF) sau FISCALE |
| VerificaGardianCicluCont, VerificaValoareIesire, VerificaCorectie, VerificaD85, VerificaF28, VerificaSchemaCub, VerificaPozitieLinii, VerificaLaturi | ambele | X |
| VerificaApiNtc | ambele | NTC |
| VerificaApiAsm / VerificaApiRlf / VerificaApiRdc | privat | ASM / RLF / RDC |
| VerificaF23*, VerificaF24*, VerificaPotrivire | ambele (F24Explica privat) | X (politici; fără cod în catalog) |
| VerificaDvi (ambele), VerificaApiDvi (privat) | | DVI |
| VerificaPerioade, VerificaSolduriPerioada, VerificaDataInregistrare, VerificaPerioadaDeclarare, VerificaAcceptare, VerificaReviewAcceptare, VerificaReviewF27 | ambele | X (lanțul perioadelor) |
| VerificaPartide | ambele | CITIRI |
| VerificaImobilizari, VerificaImobilizariApi, VerificaReviewF26 (ambele), VerificaReconciliereMf (privat) | | IMO |
