# Egalitatea F0 ↔ F2 la ×1 — proba de reconstruibilitate

Baza: `Atlas.Conta.Nucleu.Fizica.x1`. Fiecare interogare are o **proiecție declarată**
(cheia + măsurile comparabile), dump-uită sortat pe ambele părți cu `COPY … TO` și
diff-uită pe gazdă. SQL-ul: `pas2/egalitate.sql`; CSV-urile: `pas2/egalitate/csv/`.

Coloanele necomparabile se exclud EXPLICIT, cu motivul scris — nu se maschează:
- `SoldCurent` (Q03/Q04): depinde de ORDINEA ferestrei. F0 ordonează pe
  `(Data, Id, Sens DESC)`, cubul pe `(Data, TranzactieId, ID)`. Soldul FINAL se
  compară separat și e identic.
- `Contrapartida` (Q03): pe F0 e contul celeilalte laturi a RÂNDULUI, pe cub lista
  conturilor TRANZACȚIEI. Diferență semantică, nu de cifre (§3 din
  `nucleu-coordonate-rapoarte.md`); costul ei e măsurat separat, ca Q03c.
- `Cota` s-a normalizat la `numeric(5,2)` pe ambele părți: `RegistruTva.Cota` e
  `numeric(18,4)` („19.0000") iar `cub."CodTva".Cota` e `numeric(5,2)` („19.00").
  Fără normalizare, Q07 și Q08 apăreau 100% diferite dintr-o scară, nu dintr-o cifră.

## Rezultatul

**12 din 13 identice, rând cu rând. Singura care diferă e Q06.**

| Q | rânduri F0 | rânduri F2 | linii diferite | verdict |
|---|---|---|---|---|
| Q01 balanță sintetică | 119 | 119 | 0 | identic |
| Q02 balanță Cont × Partener (vs `q02b`) | 20.190 | 20.190 | 0 | identic |
| Q03 fișa 4111 | 144.239 | 144.239 | 0 | identic |
| Q04 fișa 4111 × partener (vs `q04b`) | 3.034 | 3.034 | 0 | identic |
| Q05 sold 4111 × partener | 2.517 | 2.517 | 0 | identic |
| Q06 partide cu rest | 93.206 | 92.002 | 42.526 | **diferă — mecanism mai jos** |
| Q07 jurnal cumpărări 06/2025 | 1.543 | 1.543 | 0 | identic |
| Q08 D394 06/2025 | 4.725 | 4.725 | 0 | identic |
| Q09 FIFO (produs × gestiune) | 30 | 30 | 0 | identic |
| Q10 PhysicalStock 31.12 | 7.782 | 7.782 | 0 | identic |
| Q11 terți SAF-T 06/2025 | 22.343 | 22.343 | 0 | identic |
| Q12 GLE 06/2025 | 47.126 | 47.126 | 0 | identic |
| Q13 documentul de storno | 456 | 456 | 0 | identic |

Soldul final al contului 4111 la 31.12.2025: **4.951.749,02 pe ambele părți.**

## Două semantici care au fost CONVERTITE, nu presupuse

- **Q02 / Q04**: F0-ul de azi cheiază pe REPARTITORUL LATURII; cubul pe PARTENERUL
  RÂNDULUI. Egalitatea s-a făcut față de `f0/q02b.sql` și `f0/q04b.sql`, scrise
  pentru pasul 2, care aplică regula cubului pe registru. Diferența dintre cele două
  chei, ca fapt: pe 4111 și 401, **1.985 de chei (Cont × repartitor)** contra
  **20.190 de chei (Cont × partener)**. Balanța „analitică" de azi grupează pe ceva
  de zece ori mai grosier decât partenerul.
- **Q05**: F0-ul real citește `SolduriPerioadaContabil`, care e cheiat tot pe
  repartitor — **1.591 de chei pe 4111**, față de **19.784 de parteneri distincți**
  în cub. Necomparabil pe cheie, deci referința declarată e același registru citit
  cu regula partenerului. Pe acea referință, egalitate perfectă.

## Q06: de ce diferă, mecanic

Intersecția declarată = cele șase tipuri concrete pe care le acoperă
`ImperecheriProiectii.DocumenteCuRest`. Pe restul, F0 are partide pe care cubul nu
le contrazice, ci le are altfel: `PartideDeschise` la 12/2025 conține și
NotaTransfer (45.542), DescarcareGestiune (36.689), NIR (17.803), NotaContabila
(6.833), BonConsum (545) — documente care nu apar niciodată în raport.

Pe intersecție, pe tipuri:

| Tip | coincid | diferă | doar F0 | doar F2 |
|---|---|---|---|---|
| Incasare | 31.381 | 0 | 0 | 0 |
| Plata | 2.486 | 0 | 0 | 0 |
| FacturaIesire | 36.623 | 2.072 | 1 | 6 |
| FacturaIntrare | 851 | 16.942 | 1.219 | 10 |
| ReturClient | 0 | 1.631 | 0 | 0 |

Trei mecanisme, toate verificate pe date, niciunul „probabil":

1. **Restul cubului e soldul TUTUROR conturilor de terț ale documentului; restul lui
   F0 e `TotalStingere − Asignat`, un fapt de DOCUMENT care nu știe pe ce conturi
   s-a postat.** Exemplu integral (FCL `FLXONL000094802`): documentul are două
   rânduri contabile, `4111 / 419` de 87,41 și `4111 / 4427` de 16,61. Atât 4111
   (client) cât și 419 (client-creditor) au `RolTert ≠ 0`, deci cubul le adună:
   16,61 + 87,41 − 87,41 = **16,61**. F0 raportează `TotalStingere` = **104,02**.
   Avansul încasat se compensează în cub în interiorul partidei, la F0 nu.
   Același tipar pe toate cele 2.072 de FCL care diferă.
2. **FacturaIntrare: datoria e împărțită pe două documente prin conexul FCT → NIR.**
   NIR-ul creditează 401 cu netul, factura doar cu TVA-ul. În cub partida se deschide
   pe documentul care POSTEAZĂ, deci NIR și FCT au partide separate; `PartideDeschise`
   tratează factura ca partidă unică. Constatarea e cea din pasul 1 (proba f), acum
   pe interogarea completă: 16.942 din 17.793. Cele **1.219 „doar F0"** sunt
   facturile cu `TotalStingere` dar FĂRĂ nicio postare pe cont de terț.
3. **ReturClient: semn.** Toate cele 1.631 de partide de retur au `Rest < 0` în
   `PartideDeschise`; dump-ul cubului a folosit `abs(Sold)`. Convenție de semn în
   proiecție, nu diferență de cifre.

Niciunul dintre cele trei nu e un defect al cubului sau al mapării: 1 și 2 sunt
aceeași limită structurală — **ce înseamnă „o partidă", documentul sau postarea pe
contul de terț** — pe care pasul 3 (modelarea documentelor) trebuie s-o tranșeze.

## Ce NU s-a putut proba pe egalitate

- **Q12b (TaxInformation)**: cele două părți citesc pe REPERE DIFERITE prin
  construcție — cubul ia postările fiscale ale acelorași `LinieId` ca rândurile
  contabile ale lunii, F0 le ia pe `PerioadaDeclarare` (`SaftProiectii.cs:353-358`).
  Nu e o diferență de mapare, ci cele două repere din `03-saft.md` §8; comparabile
  doar pe intersecția de `LinieId`, ceea ce nu probează nimic despre cub.
- **Q09 TipStoc**: `f0/q09.sql` mai filtrează `TipStoc = 1`, coordonată pe care cubul
  n-o are în modelul de măsurare (FZ-D3). Pe perechea aleasă (produs × gestiune)
  filtrul nu schimbă mulțimea — de aceea Q09 iese identic — dar egalitatea NU probează
  că `TipStoc` e redundant; pe sink-ul de consum (`UnitateInterna`, TipStoc 2) el e
  singurul care separă.
