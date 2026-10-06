# TR-D9a — pasul 3: aserțiile de regulă re-țintite pe cititorii cubului

- Data: 2026-10-06
- Bază: `tr-d9-taierea` la `8cfcb73`; contractul
  [`tr-d9-taierea-contract.md`](tr-d9-taierea-contract.md), D9-D7 și D9-D15,
  rândul pasului 3.
- Ce conține: contabilitatea probelor (D9-D7 e), aserțiile rămase pe forma
  veche și cine le ia, ce corectează inventarul pasului 1.
- Regimul rămâne dual. Pasul nu schimbă comportament și nicio cifră așteptată.
  Atinge numai `nou/tools/ModelCheck/`.

## 1. Ce a intrat

Domeniul: 338 de aserții din ModelCheck care păzeau o cifră sau un fapt de
domeniu citindu-l din registre (319 `regula`, 17 din clasa repartitorului de
pe latura notei, `DIR-V9` și `IMO-V49`).

| Starea | Aserții | Ce înseamnă |
|---|---|---|
| re-țintită | 313 | aceeași aserție, același nume, aceleași literale, citită din cub |
| ștearsă cu rând de catalog | 1 | „Contare avans: 542.01.00 = 531.01.01, 50" (bugetar), acoperită de SC-DEC-16 |
| amânată, repartitor | 17 | aserția veche neatinsă; 8 au geamăn pe cifră, cu sufixul ` [cub]` |
| rămasă pe forma veche, altă cifră pe cub | 6 | §3 |
| amânată, montaj | 1 | `NUC-BCS-N-R3-1`, §4 |

Proba pasului (D9-D7 d): în faza A fiecare aserție nouă a rulat lângă cea
veche, pe aceeași scenă, cu amândouă verzi (privat 5.069 OK, bugetar 3.720
OK, zero FAIL); abia apoi cea veche a fost ștearsă.

Citirile trec printr-un singur fișier de ajutoare, `ModelCheck/CubScena.cs`,
așezat peste `Cub/Citiri` (`Contabil`, `Loturi`, `Fiscale`, `Imobilizari`).
Niciun cititor nou în produs. O excepție numită: locul fișei de imobilizare
(`CubScena.LocFisa`, trei clauze în `IMO-V8`, `IMO-V31b`, `IMO-V49`) citește
`Gestiune` direct de pe `Postare`, fiindcă niciun cititor din `Cub/Citiri` nu
poartă gestiunea fișei.

## 2. Contabilitatea probelor

| | Înainte | După | Diferența |
|---|---|---|---|
| ModelCheck privat, linii `OK` | 4.835 | 4.839 | +4 gemeni de repartitor |
| ModelCheck bugetar, linii `OK` | 3.527 | 3.531 | −1 (SC-DEC-16), +5 gemeni de repartitor |
| `FAIL` | 0 | 0 | |
| linii cu referințe la registre în `Program.cs` | 610 | 402 | |

Comparația pe nume între linia de bază și rularea finală: nicio aserție
dispărută pe privat, una pe bugetar (cea din tabelul de la §1). Numele care
poartă valori de rulare (GUID-uri, numere de document) sunt normalizate
înaintea comparației.

Convențiile de re-țintire sunt cele din anexa inventarului, §1. Una se vede
în diff și merită știută la review: numărul de rânduri de registru cade, iar
unde forma pe cub e sigură e tradus (o notă = două postări). O aserție care
spunea „exact două note" spune acum „aceste două note"; exhaustivitatea
rămâne a catalogului de scenarii.

## 3. Aserții rămase pe forma veche: cubul dă altă cifră

Toate sunt diferențe declarate, nu defecte. Mor sau se rescriu la pasul 6,
pe cifra din catalog.

| Aserția | Registre | Cub | Declarată în |
|---|---|---|---|
| Api RLF, premisa scenei golirii | loturile (1; 10,01) | (1; 10,00) | D9-D3: ieșirea evaluată din sold |
| ANCORA F18/F5 pe calea API | lotul după retur 0 / 0,01 | 0 / 0,00 | D9-D3 |
| NIR anulat (corecție directă): Draft, fără rânduri | lotul fără sold | 5 / 59,50 pe MAG1 | SC-NIR-09: recepția stă pe tranzacția facturii |
| Storno NIR → sold lot 0 | 0 | 5 / 59,50 până la stornarea facturii | SC-NIR-09 |
| NIR anulat: Draft, registrele proprii șterse | lotul fără sold | 5 / 59,50 pe MAG1 | SC-NIR-09 |
| `NUC-FCT-P4-2`: patru rânduri fiscale | 4 rânduri | niciun fapt | scena operează sub `ProbeCub.Nemigrat`; moare cu oracolul |

## 4. Ce rămâne și cine îl ia

- **Clasa repartitorului** (17 aserții vechi, neatinse): pasul 6b, odată cu
  regula partenerului pe piciorul de terț (D9-A10).
- **Scenele cu registre scrise de mână** (`VerificaNucleuBcs`,
  `VerificaNucleuBtr`, `VerificaNucleuFclDsc`, `VerificaLaturi`, rândul
  `RegistruTva` injectat în `VerificaD394`): înaintea pasului 6. Primele patru
  își pot lua premisa dintr-o deschidere de scenă cu același sold pe lot. A
  cincea n-are operare reală care s-o producă (declarantul nu postează taxă
  pe taxare inversă × colectat), deci proba rămâne fără obiect după tăiere.
- **Blocurile vechi păstrate numai pentru linii `MĂSURAT`** (`VerificaD300`,
  `VerificaD394`, `VerificaCorectie`, `VerificaPerioadaDeclarare`): pasul 6.

## 5. Ce corectează inventarul pasului 1

Coloana `pereche` din lista de lucru numea 30 de aserții ca având deja
geamănul pe cub. La verificare a ținut întocmai pentru una. Restul:

- la 10, perechea purta cantitatea, nu și valoarea, data sau baza fiscală
  (soldurile de după BTR, BCS și LDI; `COR-V14`, `COR-V17`). Au fost
  re-țintite cu toate cifrele, nu șterse (D9-D7 c: o aserție de regulă nu se
  șterge fără rândul `SC-…` care o acoperă);
- la 11, perechea era în aceeași aserție sau aserția purta și clauze fără
  registru: s-au șters numai clauzele de registru;
- la 7, perechea nu purta cifra: geamăn complet;
- una e în tabelul de la §3.

Constatare în afara pasului: aserțiile „Cautare == Normalizeaza pe … (N
rânduri)" își poartă numărătoarea în nume, iar N crește cu unu la fiecare
rulare a integralei. O scenă lasă câte un rând de nomenclator în urmă. E
dinaintea pasului.

Urmele rulărilor: `run-nucleu/tr-d9a/pas3/` (necomis).
