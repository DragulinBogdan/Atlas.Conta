# Pasul 5, felia 10 — balanța pliată pe planul de conturi (rollup)

Felia 9 a livrat balanța, fișa și jurnalul, și a lăsat **un singur lucru deschis
cu nume**: rollup-ul pe plan (R-D5). Motivul refuzului de atunci e chiar miezul
feliei ăsteia — pe un grup de grilă **rulajele se pot însuma, dar soldurile nu**:

> Contul 401 cu un furnizor pe sold debitor 100 și altul pe sold creditor 200 dă,
> analitic, `D 100 / C 200`, dar sintetic `C 100`. Ambele corecte, la niveluri
> diferite.

De aceea clientul poartă totaluri de grup exclusiv pe coloanele de rulaj, iar
„balanța pe clase" **nu se poate obține grupând balanța plată** — oricâte
`GroupItem` ai pune. Cifra corectă la nivel de clasă nu e o sumă de rânduri
afișate; e o agregare care trebuie făcută pe **brute**, cu netarea aplicată din
nou la fiecare nivel. Felia asta e mecanismul care lipsea.

## Scope

**Intră:** o proiecție pliată peste `Balanta` (nu peste registru direct),
endpoint-ul ei și un ecran de arbore în client.

**Nu intră, cu motiv:** modul analitic pliat (BP-D4), totalurile de grilă (fiecare
rând E deja un total), exportul/tipărirea. Jurnalele de TVA sunt felie separată,
peste altă sursă — un registru care încă nu există.

**Zero migrații, zero atingere a motorului.** Ca la felia 9: dacă un pas ar fi
cerut o schimbare de model, ar fi fost semn că s-a greșit ceva.

---

## Deciziile feliei

### BP-D1 — Se cumulează BRUTELE în sus, se netează LA NOD. Niciodată invers

Singura propoziție care contează. Un nod primește sumele brute
(`InitialDebit/Credit`, `RulajDebit/Credit`) ale tuturor descendenților lui **plus
ale lui însuși**, iar `Sold*` se recalculează din ele:
`net = (iniD − iniC) + (rulD − rulC)`.

Consecința e vizibilă imediat pe date reale: clasa 4 a bazei de import iese
`C 10.100.771,84`, în timp ce însumarea coloanelor de sold ale grupelor ei ar
afișa `D 4.623.420,74 / C 14.724.192,58` — două cifre, niciuna adevărată la acel
nivel.

Corolar impus în client, nu doar afirmat: **ecranul n-are `Summary`**. Aici nu e
o economie de cod — un total de grilă peste rândurile afișate ar aduna părinții
cu copiii lor, adică ar număra fiecare frunză de câte ori are strămoși. Cifra de
control (`Σ rădăcini == Σ balanța plată`) se verifică unde poate fi exactă: în
ModelCheck.

### BP-D2 — Frunzele nu se recalculează: sunt chiar `Balanta`

Agregarea de bază e apelul existent, cu aceiași parametri de proiecție. O a doua
agregare, oricât de asemănătoare, ar fi un al doilea adevăr care divergează tăcut
de primul la prima schimbare — regula care ține un singur `AtomContabil` și care
a urcat `CoduriTip` în `ApiProiectii`.

Pliul de deasupra e **în memorie**, deliberat: planul e mărginit prin construcție
(1.679 de sintetice la bugetar, ~700 la privat), recursivitatea pe arbore în SQL
ar cere un CTE recursiv scris de mână, iar rezultatul e oricum ne-paginabil.

Două cazuri de margine, ambele tratate ca **rădăcină, nu ca rând pierdut** —
lecția review-ului D4 al feliei 9 (un atom nu are voie să dispară fiindcă i-a
dispărut eticheta):

- contul al cărui **părinte e invizibil** (șters logic sau tăiat de securitate);
- contul care **nu e în plan deloc** (are rânduri de registru, dar eticheta lui
  nu se vede) — apare ca rădăcină fără simbol, cu cifrele lui intacte.

Invariantul care le acoperă pe amândouă: fiecare frunză contribuie la **exact o
rădăcină**, deci `Σ peste rădăcini == Σ peste balanța plată`.

### BP-D3 — Un arbore nu se paginează: fără `DataSourceLoader`

Un nod fără strămoșii lui în pagină e un rând orfan, iar `LIMIT/OFFSET` peste
mulțimea pliată taie exact strămoșii. Endpoint-ul întoarce deci **tabloul
întreg**; mărginirea vine din date (numărul de noduri ≤ numărul de conturi ale
planului), nu din paginare.

E singura proiecție a pasului 5 fără `loadOptions`, și diferența e de natură, nu
de comoditate: dincolo, forma rezultatului e o listă; aici, un graf.

### BP-D4 — Modul analitic nu se pliază

Cheia analitică e `Cont × Repartitor`, adică **o a doua ierarhie**. Pliată pe
arborele de conturi ar amesteca două axe: „clasa 4 pe furnizorul X" nu e un nod
al planului. Modul analitic rămâne plat, unde e corect.

Dimensiunile rămân **filtre**, exact ca la balanța plată, aplicate tot pe atomi,
înaintea agregării.

### BP-D5 — `nivelMaxim` taie rânduri, nu sume

„Balanța pe clase" = `nivelMaxim=1`; „pe grupe" = `2`. Trunchierea e gratuită
fiindcă cifrele descendenților sunt **deja** în strămoși, iar arborele rămâne
închis în sus (părintele are întotdeauna `Nivel` mai mic, deci nu poate fi tăiat
înaintea copilului).

Două consecințe scrise în cod:

- `AreCopii` se calculează peste mulțimea **păstrată**, nu peste plan — altfel
  ultimul nivel afișat ar oferi o expandare goală;
- `nivelMaxim < 1` se **refuză cu 400**, nu se normalizează tăcut: ar goli
  raportul fără ca vreo cifră să dispară de undeva, adică un raport gol care
  arată exact ca o bază goală.

---

## Contractul de ieșire

```
GET /api/proiectii/balanta-plan
    ?dataStart&dataEnd&nivelMaxim&<cele 8 dimensiuni>
 → [ { ContId, ParinteId?, ContSimbol, ContDenumire, Nivel, AreCopii,
       AreMiscareProprie,
       InitialDebit, InitialCredit, SoldInitialDebit, SoldInitialCredit,
       RulajDebit, RulajCredit, SoldFinalDebit, SoldFinalCredit } ]
```

`AreMiscareProprie` = contul are rânduri de registru **pe el**, nu doar prin
descendenți. Legitim (nimic nu interzice postarea pe un cont sumator — filtrul
din lookup-ul Decontului e afordanță, nu validare), dar înseamnă că cifrele
nodului **nu** sunt suma copiilor afișați. Clientul îl marchează, iar
drill-down-ul spre fișă e permis **doar** de pe nodurile care îl au: fișa unui
cont pur-sumator ar fi goală, iar un ecran gol se citește ca o defecțiune, nu ca
un răspuns.

## Verificări (ModelCheck, ambele profiluri)

Scena e ramura adăugată blocului de balanță al feliei 9, iar checks rulează
**după** scena D4 — acolo lui `c6` tocmai i-a dispărut eticheta, iar `cOrfan`
atârnă de el: cazul „părinte invizibil" există fără a inventa o a doua scenă.

1. **BP-D1, miezul**: grupa iese `C 13` (rulaj `17 D / 30 C`), în timp ce
   însumarea coloanelor de sold ale frunzelor ei dă `D 17 / C 30`.
2. **BP-D1, pe copii**: nodul intermediar poartă mișcarea lui **și** a nepotului;
   copilul creditor nu e „compensat" de frații lui.
3. **Poziția în arbore**: `Nivel` 0/1/2, `AreCopii`, `AreMiscareProprie`.
4. **Închiderea pliului**: `Σ rădăcini == Σ balanța plată`, pe toate patru
   cifrele brute — proba că pliul nici nu pierde, nici nu dublează.
5. **Partida dublă** supraviețuiește la nivelul rădăcinilor.
6. **Frunza fără strămoș vizibil** devine rădăcină; contul fără etichetă rămâne
   nod, cu cifra lui.
7. **BP-D5**: la `nivelMaxim=1` rămân doar rădăcinile, totalul e neschimbat,
   `AreCopii` devine fals.
8. **BP-D4**: dimensiunile filtrează identic pe calea pliată.
9. **Ciclu în `Cont.Parinte`**: pliul **termină**. `Cont.Parinte` e o navigație
   editabilă din UI; fără garda de vizitare, verificarea n-ar pica — ar atârna.

---

## Închidere (2026-08-17)

- [x] **Contract îndeplinit.** ModelCheck **bugetar 658 OK / 0 FAIL**, **privat
  311 OK / 0 FAIL** (+9 verificări, rulate în ambele profiluri). Soluția și
  clientul compilează; `openapi.json`/`api-types.ts` regenerate. **Zero
  migrații.**
- [x] **Probat în browser** pe clona bazei de import (205k documente): anul 2025
  se pliază în 197 de noduri din 119 frunze, 8 rădăcini; clasa 4 iese
  `C 10.100.771,84` cu grupele 40…48 dedesubt, iar aritmetica nodului se închide
  exact pe netele copiilor. `Σ rădăcini == Σ balanța plată` verificat și pe date
  reale, prin cele două endpoint-uri. Drill-down 401 → fișa contului (73.241
  rânduri, perioada păstrată). `nivelMaxim=2` + „Extinde tot" dă balanța pe grupe,
  cu ultimul nivel fără expandare.
- [x] **Perf** (aceeași metodă ca 59, clona de import, HTTP cald): **58–75 ms**
  pentru orice adâncime — mai ieftin decât balanța plată (93 ms), fiindcă nu
  plătește `requireTotalCount`. Niciun index adăugat.

### Ce a scos smoke-ul, în afara mandatului

**`autoExpandAll` se aplică la montare, nu la sosirea datelor.** Arborele se
monta pe tabloul gol (datele vin din `useQuery`, deci mai târziu) și rămânea cu
toate nodurile strânse — bifa „Extinde tot" nu făcea nimic, tăcut. Nu se repară
cu o cheie de remontare care include datele; randarea e **gate-uită pe
încărcare**, ca arborele să se monte o singură dată, cu datele deja acolo.

### Rămase, ne-blocante

- **Fără rollup analitic** (BP-D4) — dacă apare cerința, e o a doua ierarhie, cu
  designul ei.
- **Cele 8 dimensiuni n-au UI** nici aici: pass-through din URL, ca la felia 9.
- **Nodurile fără mișcare proprie nu au drill-down** — deliberat; alternativa ar
  fi o fișă „consolidată" pe subarbore, care e alt raport.
- Ciclul din `Cont.Parinte` e **oprit**, nu **raportat**: cifrele unui plan
  ciclic rămân fără sens, dar serverul nu atârnă. Un gardian de nomenclator
  (părinte care nu poate fi descendent) ar fi fixul de fond, și e aditiv.
