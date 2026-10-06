# TR-D9a, pasul 5c — îmbinarea partidelor pe chei nulabile

**Data:** 2026-10-06. **Stare:** făcut. **Contract:**
[`tr-d9-taierea-contract.md`](tr-d9-taierea-contract.md), D9-D10 (b) și
D9-D15, rândul 5c. Pasul e aprobat de owner (2026-10-06), pe cifra din
[`tr-d9-pas5b-probe.md`](tr-d9-pas5b-probe.md) §4.

## 1. Ce s-a corectat

Scara de volum a arătat că partidele cu rest și snapshot-ul de partide cresc
pătratic. Cauza: cititorii îmbinau soldurile partidelor cu originile lor pe
unitate, cont și partener, iar unitatea și partenerul sunt coloane nulabile
pe `Postare`. Pentru două chei nulabile EF generează
`a = b OR (a IS NULL AND b IS NULL)`, pe care Postgres nu o poate îmbina prin
hash sau merge.

**Regula.** EF adaugă ramura de nul numai când ambele părți ale egalității
sunt nulabile. Cu o singură parte făcută ne-nulă, comparația iese egalitate
simplă. Cititorii de partide fac asta pe partea soldurilor
(`COALESCE` cu identificatorul gol); cheile sunt oricum nenule prin filtrul
`Partide.Postari`, deci rezultatul e același.

**Codul.**

- `Partide.CuOrigine` e singurul loc care îmbină soldurile cu documentul
  deschizător. Îl folosesc snapshot-ul de partide
  (`SolduriService.SursaPartide`), `PartideCuRest` și `Partide.Proprii`.
- `Partide.OriginiDocument` recunoaște originea numai după identitate
  (`Unitate = cub_partida_id(DocumentId, Cont, Partener)`), fără ramura de
  deschidere și fără îmbinarea cu `Tranzactie`. Ramura de deschidere aducea
  doar `DocumentId` nul, adică ce dă oricum îmbinarea la stânga.
  `Partide.Origini`, cu ambele ramuri, rămâne pentru alocarea plăților, care
  deosebește partida inițială de partida fără origine.
- `Partide.MiscariPePartidele` înlocuiește îmbinarea scrisă în `Fapte.Sursa`
  (restul sursei la documentul autogenerat).
- `Partide.Perechi` și `DocumenteCuRest` aveau aceeași formă pe alte chei:
  partenerul, respectiv documentul și contrapartida. Pe `DocumenteCuRest` a
  găsit-o proba nouă, nu scara; îmbinarea cu totalurile pleacă acum de la
  antet, care are chei nenule.

Nu s-a schimbat schema și nu s-a adăugat niciun index. Un index parțial pe
predicatul de origine a fost măsurat și nu ajută (3,5 s cu el, 2,9 s fără).

## 2. Cifrele

Pe `Atlas.Conta.ModelCheck.Privat.D9Vol`, treapta ×629: 5.001.179 de postări,
651.644 de partide, 488.733 cu rest. „Înainte" e rularea scării din pasul
5b; „după" e același cititor, cu binarul corectat, în același container.

| | Înainte | După |
|---|---|---|
| partidele cu rest, la cald: sistem / din snapshot / securizat | peste 10 minute | 6,1 / 5,3 / 6,2 s, din care 2,7 s SQL |
| prima reconstrucție a snapshot-urilor | oprită la 30 de minute | 48,1 s |
| reconstrucția repetată (`RECON-N`), rece / cald | nemăsurată | 51,8 / 55,3 s |
| disponibilul partidei (`PDISP-N`), la cald | 152 ms | 147 ms |

Partidele cu rest livrează 488.733 de rânduri, adică 777 × 629, pe toate
cele trei rute. Snapshot-ul scris are 367.965 de partide, adică 585 × 629.

Reconstrucția verifică și apoi rescrie tot. Citirile de verificare durează
4,4 s pe contabil, 1,2 s pe stoc și 3,5 s pe partide; restul timpului e
ștergerea și scrierea a 1.474.780 de rânduri de snapshot.

Formele încercate în SQL scris de mână, pe aceeași bază, o rulare fiecare,
la cald:

| Forma | Timp |
|---|---|
| ramura de nul (produsul dinaintea pasului) | peste 10 minute |
| egalitate simplă, originile cu ambele ramuri | 5,1 s |
| egalitate simplă, originea numai după identitate (forma aleasă) | 2,9 s |
| fără îmbinare, originea proiectată în aceeași agregare | 2,5–2,7 s |
| soldurile singure | 0,7 s |

Proiecția fără îmbinare n-a fost aleasă: câștigă 0,3 s, cere un agregat
`max(uuid)` pe care Postgres 18 nu îl are și nu acoperă ruta din snapshot,
unde o partidă stinsă la închidere și redeschisă după își are originea
înaintea snapshot-ului.

Proprietățile pe care se sprijină echivalența, verificate pe `.D9Vol` și pe
`Atlas.Conta.Import1C.Flax.R3f` (11.853 de unități): o unitate are un singur
cont și un singur partener, cel mult un document deschizător și nicio
postare înaintea originii; nicio unitate nu are și origine pe document, și
postare de deschidere.

## 3. D9-D10 (b)

Criteriul de formă, așa cum îl scrie contractul, rămâne picat: documentul
deschizător se citește tot parcurgând postările de partidă. Parcurgerea e
liniară. Ce creștea pătratic era altceva, îmbinarea, și e corectat.

D9-D10 (b) e deci **re-amânat explicit, cu cifra**: 2,7 s de SQL pentru
488.733 de partide cu rest la 5 milioane de postări. Owner-ul a confirmat
re-amânarea (2026-10-06): rămâne până la decizia 111.

Închiderea criteriului cere ca originea să fie scrisă, nu recunoscută la
citire. Reper pentru decizia 111, măsurat pe `.D9Vol` cu un tabel temporar
care ține o linie pe partidă (unitatea și documentul ei, 651.015 linii):
aceeași citire durează 0,8 s integral și 0,6 s pe ruta din snapshot, față de
2,9 s. Pe ruta din snapshot, `Postare` se mai citește numai pentru mișcările
de după închidere. Coordonatele ne-nule (identificator gol în loc de nul) nu
schimbă această parcurgere: ele scot ramura de nul din îmbinări, nu
recunoașterea originii.

## 4. Proba

`SC-CIT-110`, pe ambele profiluri, pe SQL-ul generat: `PartideCuRest` și
`DocumenteCuRest` pe ambele feluri de citire, `Proprii`, `Perechi`,
`MiscariPePartidele`, plus verificarea și scrierea snapshot-ului de partide
nu conțin nicio îmbinare cu ramură de nul. Proba cade pe forma veche: așa a
fost găsit `DocumenteCuRest`.

## 5. Verificarea

- ModelCheck integral: **4.862 privat / 3.554 bugetar OK**, zero FAIL. Față
  de pasul 5b apar numai cele două aserții `SC-CIT-110` pe fiecare profil;
  niciuna nu dispare.
- Soluția compilează fără erori; `--probe-sursa` trece.
- Dovezile, neversionate: `run-nucleu/tr-d9a/pas5c/` (logurile integralei)
  și `run-verificari/scara-volum-pas5c-20261006-*/` (măsurările pe `.D9Vol`,
  cu planurile).

## 6. Limite

- `DocumenteCuRest` nu are cifră la volum: scara nu îl măsoară. Forma
  îmbinării e probată; totalurile lui parcurg în continuare toate postările
  (F27-r16).
- `.D9Vol` are acum snapshot-urile treptei ×629, scrise de reconstrucția
  acestui pas.
- Proba acoperă cititorii de partide numiți. O îmbinare nouă pe două chei
  nulabile ale lui `Postare`, în alt cititor, nu e prinsă de ea.
