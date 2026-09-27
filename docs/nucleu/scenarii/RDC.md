# RDC — retur de la client

2026-09-23: așteptări specificate înaintea declarantului. Reguli: TR-D7b
T-D8, 076(d–g), 088(h), 090(d,i), 091(a–e), 092. Probe planificate:
`ScenariiRdc`, anul 2008, marcaj `E2E-SC-RDC`; comenzi în ObjectSpace-uri
noi, documente reale, curățenie în `finally`.

Privat: venit 704/707, client 4111, TVA 4427, marfă 371, cost 607.
Bugetar: RDC nu are politică de contare; rămâne inert, fără activare sau
politică inventată pentru probă. Așteptările sunt constante RON, sold D−C.
La comanda documentului fără număr, primul refuz este lipsa politicii de
numerotare; proba îl identifică explicit și nu pretinde atingerea contării.
Returul se culege pozitiv; operarea semnează valorile negativ. Costul
revine pe lotul original, fără fapt fiscal; venitul deschide partida
proprie negativă, fără nominalizare automată pe FCL (aceasta rămâne TR-D9).

| ID | Pași și așteptare numerică | Rezultat | Proveniență | Proba | Stare |
|---|---|---|---|---|---|
| SC-RDC-01 | Venit 100 fără TVA: dry-run fără postări; operare D4111 −100 / C704 −100, Q0; partida RDC −100. | acceptat | T-D8 | `ScenariiRdc` / ID | verificat, privat |
| SC-RDC-02 | Două venituri 100 N21 și 50 N11: D4111 total −176,50, C704 −150, C4427 −26,50; două cote distincte, o partidă. | acceptat | recensământ multicotă; T-D8 | `ScenariiRdc` / ID | verificat, privat |
| SC-RDC-03 | Storno al venitului100 în ianuarie: +100/+100, partidă0, originale intacte; repetare refuzată fără efecte. | acceptat / refuz stare | 090i | `ScenariiRdc` / ID | verificat, privat |
| SC-RDC-04 | Anulare100 în luna deschisă: Draft, zero efecte; reoperare păstrează minusul și suma100. | acceptat | 076e | `ScenariiRdc` / ID | verificat, privat |
| SC-RDC-05 | Recepție10×10, FCL6×20+TVA25,20 și DSC6/60. RDC venit40+TVA8,40 și cost2/20: stoc6/60, venit net80, TVA16,80; partida FCL145,20 rămâne intactă, partida RDC−48,40. Costul nu are TVA. | acceptat | T-D8; recensământ mixt | `ScenariiRdc` / ID | verificat, privat |
| SC-RDC-06 | După SC05, returul restului4 cu venit80+TVA16,80 și cost40: stoc10/100; venit/cost/TVA nete ale vânzării și retururilor0. | acceptat | T-D8 | `ScenariiRdc` / ID | verificat, privat |
| SC-RDC-07 | Lot golit prin DSC: recepție10/100, ieșire10/100; RDC numai cost2/20 → stoc2/20, două postări, fără partidă sau TVA. Storno revine0/0. | acceptat | T-D8 | `ScenariiRdc` / ID | verificat, privat |
| SC-RDC-08 | RDC cu două loturi: +2/20 și +3/60; cont de cost −80, cantități pe loturi separate, fără netare de identități. | acceptat | recensământ multiline | `ScenariiRdc` / ID | verificat, privat |
| SC-RDC-09 | FCL100 → INC40 și împerechere → RDC30: partida FCL60, RDC−30. NTC4111=4111:30 cu client pe ambele laturi stinge RDC și scade FCL la30; storno NTC redeschide60/−30. Storno RDC cât NTC activ refuzat atomic. | acceptat / refuz dependență | T-D8, 090e, 076g | `ScenariiRdc` / ID | verificat, privat |
| SC-RDC-10 | Venit100 N21 în ianuarie închis: anulare refuzată; storno05.02 +100/+21, fiscal februarie. Partida−121 la31.01 și0 la05.02. | acceptat pentru storno | 088, 090i | `ScenariiRdc` / ID | verificat, privat |
| SC-RDC-11 | Corecție legată a venitului100 N21 în februarie: original inversat +121; înlocuitor80 N21 →−96,80, fiscal ianuarie, Data originală și DataInregistrare februarie. Originalele intacte. | acceptat | 088h | `ScenariiRdc` / ID | verificat, privat |
| SC-RDC-12 | Data05.01, DataInregistrare05.02, venit100 fără TVA: zero în ianuarie, partida−100 numai din februarie. | acceptat | 088 | `ScenariiRdc` / ID | verificat, privat |
| SC-RDC-13 | Fără linii / venit0 / venit cu Tip de stoc fără lot / cost cu cantitate0 / TVA capitalizat pe venit / latură internă în loc de client: refuz, zero efecte. | refuzat, coduri stabile pe declarație; gardienii existenți pe comandă | T-D8, 076 | `ScenariiRdc` / ID | verificat, privat |
| SC-RDC-14 | Venit100 cu taxare inversă: D4111−100/C704−100, fapt de bază; fără 4427 și fără taxă. | acceptat | politica Colectat, T-D8 | `ScenariiRdc` / ID | verificat, privat |
| SC-RDC-15 | Cost pe lot cu preț0: retur2 → +2/0 pe lot, −2/0 pe capătul virtual Client, fără partidă/TVA. | acceptat | recensământ: 3 linii de cost zero; T-D8 | `ScenariiRdc` / ID | verificat, privat |
| SC-RDC-16 | Bugetar: zero reguli RDC, PosteazaInCub=false; documentul încercat este refuzat de politica lipsă, zero postări. | refuzat / profil inert | seed curent | `ScenariiRdc` / ID | verificat, bugetar |
| SC-RDC-17 | Recepție2/20 → DSC2 → RDC cost2 → DSC2: stoc0; storno RDC refuzat fără efecte. Inversarea ultimului DSC redeschide2/20, apoi storno RDC readuce0/0. | refuzat, apoi acceptat | review dependență de stoc, înaintea probei | `ScenariiRdc` / ID | verificat, privat |
| SC-RDC-18 | Recepție10/100 → DSC4/40 → două RDC cu cost2/20 fiecare, adăugate veniturilor SC10/11: stoc10/100 la31.01. În februarie storno primului și corecția celui de-al doilea la cost1/10: stoc7/70; istoricul lui ianuarie rămâne10/100. | acceptat | review ciclu mixt, înaintea probei | `ScenariiRdc` / ID | verificat, privat |

## Recensământ și limite

Read-only pe `Atlas.Conta.Import1C.Flax.TrD7b`, 2026-09-23, script
`recensamant-retururi-dvi.sql`: 1.666 RDC / 4.093 linii; 1.605 documente
mixte, maximum32 linii; 2.010 linii de cost și2.083 venit; 2 documente
multicotă, zero întârziate și corecții legate. Cost: 2.007 valori negative,
3 zero, toate pe lot existent. Venit: 1.374 N19,700 N21,9 fără TVA.

Fișa/balanța și `Sold` reconstruibil sunt TR-D8; nominalizarea pe factura
originală, reevaluarea și eliminarea registrelor TR-D9. Lanțul SC-X-03
complet cu avans și concurența rămân în catalogul transversal. Această
felie probează returul după încasare parțială și compensarea prin NTC.

## Validare

`verifica.ps1 -Suita Scenarii -Tip RDC -Profil Ambele -Sufix .CodexBCS`:
3 verificări bugetar / 78 privat, zero eșecuri. Manifest:
`run-verificari/20260923-122728-544/rezultat.json`. Costul zero, dependența
de stoc și corecția mixtă sunt incluse, fără modificarea registrelor din probe.

Gate integral pe aceleași baze: 1.743 OK bugetar / 2.383 OK privat,
zero FAIL, exit0 la build și ambele rulări. Manifest:
`run-verificari/20260923-122814-720/rezultat.json`.
