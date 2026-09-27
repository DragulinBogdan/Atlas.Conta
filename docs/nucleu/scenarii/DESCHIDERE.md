# DESCHIDERE — solduri inițiale detaliate

2026-09-24. Specificat înaintea implementării. Proveniență: 094,
DES-B1…B4, 090(d/g), 091(f), 092. Ambele profiluri, an propriu 2017.
Recepția inițială este comandă de motor fără document, excepția I;
nu se inserează postări direct din scenă.

S = 302 privat / 302.01.00 bugetar. Ancora se alege din profil.
Un cont de probă cu UrmarestePartide și parteneri reali permite aceeași
probă de mecanism pe ambele profiluri (096).

| ID | Intrare și așteptare numerică | Rezultat |
|---|---|---|
| SC-DES-01 | D S 100, C ancoră 100; lot A 4/40, B 3/60. Exact trei postări; S=100, nu 200; loturile 4/40 și 3/60. | acceptat |
| SC-DES-02 | C terț 150, D ancoră 150; F1/ref1 60, F1/ref2 40, F2/ref1 50. Trei partide distincte, fără rând bloc pe terț. | acceptat |
| SC-DES-03 | PLT 20 → partida F1/ref1: rest 40, celelalte 40 și 50; inversare PLT reface 60. Originalul deschiderii intact. | acceptat |
| SC-DES-04 | Detalii stoc 99 sau 101 contra totalului 100; detalii cerute absente, inclusiv stoc nemarcat Detaliat; detaliu fără sold de control. | refuz atomic |
| SC-DES-05 | Repetare după commit, două apeluri înainte de commit și două sesiuni care construiesc simultan deschiderea. | refuz, o singură tranzacție; a doua sesiune primește unique violation |
| SC-DES-06 | Cont/lot/gestiune/partener absent, cont diferit de al lotului, dată de lot ulterioară, referință goală, cantitate/valoare negativă, solduri neechilibrate. | refuz atomic |
| SC-DES-07 | Lot gratuit 2/0; cantitatea rămâne 2. Soldul creditor 150 din SC-DES-02 este păstrat pe Credit; cărțile nu se echilibrează între ele. | acceptat/refuz carte |
| SC-DES-08 | Ziua anterioară: zero; data deschiderii: soldurile 100 și 150; citire round-trip fără document sau linie fictivă. | acceptat |
| SC-DES-09 | PLT peste rest ori către alt partener, deschidere la 5 ian, PLT la 7 ian: stingere la 4 ian refuzată pentru deschidere, la 6 ian pentru plata încă neînregistrată; lună închisă. | refuz atomic |
| SC-DES-10 | 2/51 poziții: citire pe set, număr constant de interogări; eșecul nu lasă entități noi de cub. | acceptat |
| SC-DES-11 | NTC stinge pe 25 ian partida PLT de 20. PLT încearcă pe 5 ian să stingă 20 din partida inițială de 60, apoi pe 25 ian altă NTC de 20: ambele refuzate; partida inițială rămâne 60. | refuz, inclusiv consumul viitor |
| SC-DES-12 | PLT 20 stinge partida inițială de 40: restul de domeniu al PLT devine 0. NTC încearcă să stingă PLT: refuz prin serviciu și direct pe materializarea cubului, fără Sare. | refuz |
| SC-DES-13 | Anularea PLT care a stins partida inițială este refuzată; amprenta documentului și transferului rămâne identică. | refuz |
| SC-DES-14 | PLT pe 5 ian, stingere pe 25 ian, storno pe 10 ian: refuz fără efecte. Storno pe 25 ian: partida inițială revine la 40. | refuz / acceptat |
| SC-DES-15 | Partidă inițială 100 lei / 20 în valută cu analiză economică: coordonate păstrate, raport 5. Stingerea încă neacoperită este refuzată fără transfer. Coordonate pe total detaliat ori sumă fără valută: refuz. | acceptat / limită explicită |
| SC-DES-16 | Randuri.Citeste fără DocumentId și fără navigația Tranzactie: InvalidOperationException explicită, nu NullReferenceException. | refuz explicit |
| SC-DES-17 | C terț 100 / D ancoră 100 în cartea fiscală, detaliat prin partidă: refuz; partidele inițiale se sting numai contabil. | refuz |
| SC-DES-18 | Contul cere CodEconomic; intrare fără cod: refuz înainte de adăugarea vreunei postări. | refuz |
| SC-DES-19 | FCT/NIR 5/50 pe 5 ian, deschidere pe 6 ian: refuz din cauza istoriei existente. | refuz |
| SC-DES-20 | Lot cu recepție existentă 5/50 sau lot nou atașat unei linii de intrare: refuz, fără dublare. | refuz |
| SC-DES-21 | Două sesiuni reale: stingere ținută necomisă vs. storno/anulare, apoi storno necomis vs. stingere. Se observă FOR UPDATE înaintea citirii cubului; stornoul include transferul, anularea refuză, stingerea recitește Stornat. Partida rămâne 40. | serializare și sold exact |

Storno-ul plății și refuzurile se execută prin comenzile reale. Deschiderea
nu are anulare/storno de document; resetarea bazei de test este curățenie.
Cititorii de producție rămân TR-D8, conectorul 1C rămâne migrare.

Fixture-ul persistent reunește SC-DES-01/02/07: trei loturi, trei partide
și două contraponderi, exact opt postări, D=C=250. Exemplul izolat cu
două loturi și trei postări se verifică separat, într-o tranzacție retrasă.
PLT 20 mută pe aceeași latură Debit -20 de pe partida proprie și +20 pe
partida inițială; suma pe cont și latură este zero. Refuzul peste rest
este probat separat cu PLT 70 care încearcă să stingă 61 din partida 60.

## Verificare, 2026-09-24

- Selectiv: `pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip DESCHIDERE -Profil Ambele -Sufix .CodexBCS`;
  exit 0 pe ambele profiluri, `run-verificari/20260924-020709-010/rezultat.json`.
- Integral, inclusiv probele suplimentare pe exemplul izolat și transfer:
  `pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexBCS`;
  **2.353 bugetar / 3.402 privat**, zero FAIL, build 0 warnings, exit 0:
  `run-verificari/20260924-021417-392/rezultat.json`.
- Perf pe aceeași bază, 2/51 loturi: **6/6 interogări** pe fiecare profil.
- Nucleul pur nu s-a schimbat în pasul Deschidere; ultima probă relevantă
  178/178: `run-verificari/20260923-235301-386/rezultat.json`.

Încercări intermediare: compilarea a prins folosirea purjei BaseObject pe
POCO; migrația a fost aplicată explicit înaintea scenariilor. Proba de
perf a eșuat cu 9/58 interogări, rezolvat prin eliminarea lazy loading la
crearea postărilor. Prima rulare integrală din
`20260924-021234-635` a prins o așteptare greșită în proba nouă de transfer:
regula aprobată mută +20/-20 pe aceeași latură, nu D20/C20. Așteptarea
a fost corectată conform conservării per cont/latură; codul transferului
nu s-a schimbat pentru acest rezultat.

SC-X-11 nu este declarat complet: citirile de producție și lanțul cu
documentul ulterior rămân la TR-D8. Review-ul advers de sincronizare este
primit; corecturile Deschidere sunt supuse reverificării înainte de commit.
Intrarea păstrează analiza și valuta; ciclul multivalutar rămâne la TR-D9,
cu refuz explicit la stingerea unei partide inițiale în valută. Soldurile
nedetaliate au o singură analiză pe cheia de control (DES-B4).
Ștergerea concurentă a nomenclatoarelor (MINOR-3) nu este rezolvată aici.

## Corecturile review-ului de sincronizare, 2026-09-24

SC-DES-11…21 completează catalogul; SC-DES-04/07/09 disting acum exact
refuzurile cerute. Nucleul probează separat identitatea: aceleași
cont/partener/referință dau partide diferite pentru deschidere și document,
iar data nu schimbă identitatea inițială (500 de cazuri).

Prima probă pe codul de bază a rămas roșie în
`run-verificari/20260924-122840-440/rezultat.json` (8 FAIL). După corecturi,
fixture-ul a fost izolat de nominalizarea FIFO implicită a NTC: nota se
operează înaintea plății, iar plata probei valutare se stornează înaintea
probelor de stingere. Nu s-au relaxat așteptările. Observarea concurenței
șterge snapshot-ul statistic PostgreSQL între citiri; fără asta două
blocări reale nu erau observate, deși rezultatele contabile erau corecte.
Manifestele intermediare păstrează aceste eșecuri:
`20260924-123235-477`, `20260924-123933-887`, `20260924-124236-237`.
Selectiv ulterior, ambele profiluri: zero FAIL, exit 0,
`run-verificari/20260924-124544-262/rezultat.json`. Perf: **10/10 interogări**.
Nucleu: **180/180**, zero omise, exit 0,
`run-verificari/20260924-124628-047/rezultat.json`.

Integral final pe sursa predată (inclusiv consumul datat în viitor):
**2.656 bugetar / 3.747 privat**, zero FAIL, exit 0 și build fără avertismente,
`run-verificari/20260924-125210-353/rezultat.json`.
Comandă: `pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexBCS`.
Rularea integrală precedentă, tot verde, este în
`run-verificari/20260924-124655-233/rezultat.json`.
Corecturile sunt predate pentru reverificare și commit de către Claude,
conform 098(d); următoarea felie este IMO.

### Completare la review: citiri de stingere inițială (2026-09-24)

SC-DES-11/12 compară detaliul, proiecția de listă și snapshot-ul SQL:
PLT 20 stinsă normal are rest 0; stinsă din deschidere în două tranșe de
10 are succesiv rest 10 și 0 pe toate citirile. Data stingerii taie corect
fereastra listei (0 înainte, 20 la dată); inversa eliberează atribuirea.
Detaliul și lista folosesc aceeași intrare `AsignariDeschidere`; geamănul
SQL al snapshot-ului este acoperit de egalitatea numerică.

Verificare comună cu IMO: **2.709 bugetar / 3.800 privat OK**, zero FAIL,
exit 0, build fără avertismente; suita Integral, ambele profiluri,
`run-verificari/20260924-150531-943/rezultat.json`.
