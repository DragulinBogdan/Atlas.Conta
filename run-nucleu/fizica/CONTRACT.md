# Pasul 2 al designului cubului: FIZICA — contractul măsurătorii (2026-09-19)

Întrebarea (din `docs/nucleu/nucleu-cub-design.md` §11): un cub logic, N
partiții fizice pe spațiu, indexi parțiali per uz — CIFRĂ pe volumele de
import, nu opinie. Concret: care formă fizică a unui singur cub de postări
dă, pe interogările REALE ale rapoartelor (pasul 1, §3), cifre cel puțin la
fel de bune ca cele patru registre + snapshot-urile de azi, și cum scalează
la un istoric de 10 ani. Secundar, dar obligatoriu: proba că cubul construit
din faptele bazei reale RECONSTRUIEȘTE rapoartele de azi cu aceleași cifre.

## Deciziile pin-uite (FZ-D)

- **FZ-D1 Baza**: `Atlas.Conta.Import1C.Flax` (2025 integral, profil privat,
  197.554 documente, 331.757 linii; registre: contabil 295.013, stoc 274.031,
  TVA 86.636; `SolduriPerioadaContabil` 184.780 pe 12/2025; `PartideDeschise`
  201.046; `Imperecheri` 44.448). Se lucrează pe CLONĂ
  (`CREATE DATABASE "Atlas.Conta.Nucleu.Fizica" TEMPLATE
  "Atlas.Conta.Import1C.Flax"`), Flax nu se atinge. Postgres 18.4 în docker,
  setările implicite (`shared_buffers` 128 MB, `work_mem` 4 MB) rămân
  NESCHIMBATE și se declară — sunt aceleași pentru toate formele.
- **FZ-D2 Un singur cub, o singură tabelă logică `Postare`** cu coordonatele
  din enumerarea pasului 1 §2, ca coloane tipate (nu EAV, nu JSONB):
  `Spatiu` (smallint: 1 Contabil, 2 Stoc, 3 Fiscal), `TranzactieId`,
  `DocumentId`, `LinieId`, `Data` (înregistrării), `Cont`, `Latura` (1 D / 2 C,
  doar Contabil), `Partener`, `Gestiune`, `Produs`, `Unitate`, `CodTvaId` +
  `RolTva` (1 Bază / 2 Taxă), `PerioadaDeclarare` (int AAAALL), `Carte`
  (smallint), `Valuta` (char 3, null), `CodFunctional`, `CodEconomic`,
  `SursaFinantare`, `UnitateOrganizatorica`, `Proiect`, `CentruCost`,
  `Atribuit` (uuid null); măsuri `Cantitate`, `ValoareValuta`, `Valoare`
  (numeric 18,3 / 18,2 / 18,2). Plus `Tranzactie(ID, DocumentId null, Fel,
  Data, ScrisLa)` și `CodTva(ID, TipTvaId, Regim, Cota, Sens)` = versiunile
  DISTINCTE găsite în `RegistruTva`.
- **FZ-D3 Maparea registre → cub e DETERMINISTĂ și declarată** (modelul de
  măsurare, nu modelarea finală a documentelor — aceea e pasul 3):
  - `RegistruContabil` → 2 postări (Latura D cu dimensiunile debit, Latura C
    cu ale creditului), `Spatiu`=Contabil, `Valoare` = valoarea rândului;
    `Partener` = repartitorul laturii când e `Partener`/`Angajat`, `Gestiune` =
    repartitorul când e `Gestiune`/`ContPropriu`/`UnitateInterna` (se
    confirmă din explorarea semanticii); `Produs` = Material; analiza ×6 din
    coloanele plate; `Unitate` = partida (documentul care a deschis-o) pe
    postările de pe conturi cu `RolTert` ≠ 0 — pentru plăți/încasări, prin
    `Imperecheri` (o postare per partidă stinsă, restul pe documentul propriu).
  - `RegistruStoc` → 1 postare, `Spatiu`=Stoc, `Cantitate`+`Valoare` cu semnul
    rândului, `Gestiune` = repartitor, `Unitate` = lot, `Produs` = al lotului,
    `Cont` = NULL în modelul de măsurare (unificarea rândului de stoc cu
    postarea 3xx e decizie de modelare; efectul ei se ESTIMEAZĂ din numărări,
    nu se măsoară aici).
  - `RegistruTva` → 2 postări, `Spatiu`=Fiscal: rol Bază cu `Valoare`=`Baza`
    și rol Taxă cu `Valoare`=`Tva` (rândul de taxă se scrie și când e 0 —
    `Scutit`/`Neimpozabil` au taxă zero cu bază nenulă; se numără separat
    câte sunt); `CodTvaId` = versiunea (`TipTvaId`, `Regim`, `Cota`, `Sens`);
    `PerioadaDeclarare` din `PerioadaAn/Luna`; `Data` = data rândului.
  - Rândurile de deschidere (`DocumentId IS NULL`: 64 contabile, 6.947 stoc)
    → o singură `Tranzactie` cu `Fel`=Deschidere. Storno: pe Flax 0 rânduri
    (cifră), deci `Fel`=Storno nu apare în date; forma e prevăzută în schemă.
  - Imobilizări: `RegistruImobilizari` e gol pe Flax → nu se mapează; se spune.
- **FZ-D4 Probele de reconciliere sunt gate ÎNAINTE de orice măsurătoare**:
  (a) Σ Valoare D = Σ Valoare C per tranzacție pe Contabil (toate); (b) per
  cont, Σ D și Σ C din cub = Σ din `RegistruContabil` pe `ContDebitId` /
  `ContCreditId`; (c) per lot, Σ Cantitate și Σ Valoare = `RegistruStoc`;
  (d) per (versiune CodTva, perioadă), Σ Bază și Σ Taxă = `RegistruTva`;
  (e) numărul de postări per spațiu = formula din FZ-D3 (2·295.013 = 590.026;
  274.031; 2·86.636 = 173.272; total 1.037.329, plus cele din despicarea pe
  partide, numărate). O abatere = oprire, nu normalizare.
- **FZ-D5 Formele fizice comparate** (aceleași date, aceeași bază, aceleași
  setări, măsurate una după alta):
  - **F0** = azi: registrele + `SolduriPerioadaContabil`/`Stoc` +
    `PartideDeschise`, cu indexii EF existenți; SQL-ul = forma proiecțiilor
    curente (explorarea 01).
  - **F1** = cubul plat, NEpartiționat, indexi „EF-style": un btree pe fiecare
    coloană-cheie (cum are azi `RegistruContabil`, 24 de indexi) — portarea
    naivă.
  - **F2** = cubul partiționat LIST pe `Spatiu` (3 partiții), indexi
    COMPUȘI și PARȚIALI proiectați per uz din lotul de interogări (cu
    justificarea fiecăruia scrisă), fără indexi pe coloane neinterogate.
  - **F3** = F2 sub-partiționat RANGE pe `Data` (an) — istoricul de 10 ani ca
    partiții pe an, perioada închisă = partiție rece.
  - **+S** = F2/F3 cu tabela de snapshot `Sold(Perioada, coordonate, ΣD, ΣC,
    ΣCantitate)` la granițele de perioadă (designul §7), folosită DOAR de
    interogările care azi folosesc `SolduriPerioada*` (balanțe, solduri
    inițiale SAF-T). Se măsoară cu și fără.
- **FZ-D6 Două scări**: ×1 (2025 real) și ×10 (2016–2025: același an
  replicat cu `Data` mutată cu −k ani, `PerioadaDeclarare` la fel, ID-uri noi
  pentru `Tranzactie`/postări, ACELEAȘI ID-uri de coordonate — selectivitatea
  pe cont/partener/gestiune rămâne realistă, cea pe dată crește ×10; se
  declară că partidele replicate nu se sting peste ani). F0 se replică la
  fel (registrele; `SolduriPerioada*` rămân doar pe 12/2025, cum sunt și în
  realitate — doar perioadele de referință sunt materializate). Cifrele se
  raportează la AMBELE scări; verdictul de scalare vine din ×10.
- **FZ-D7 Lotul de interogări** = Q01–Q14 din explorarea 01 (balanțe, fișă,
  sold × partener, documente cu rest, jurnal TVA, D394, loturi FIFO,
  PhysicalStock, terți SAF-T, GLE, citirea pentru storno, scrierea
  operării), rescrise pe cub în forma din pasul 1 §3 (sume pe coordonate;
  nimic din document în afara `Cauza`). Pe F1/F2/F3 SQL-ul e ACELAȘI (doar
  tabela diferă); pe F0 e forma de azi. Rezultatele (nu doar timpii) se
  DUMP-uiesc și se compară sortate F0 ↔ F2 la ×1: egalitate pe cifre =
  proba de reconstruibilitate; diferență = raport, nu ascundere.
- **FZ-D8 Disciplina de măsurare**: psql în docker, `EXPLAIN (ANALYZE,
  BUFFERS)`, 6 rulări per interogare, prima aruncată, MEDIANA celor 5 calde
  (metoda din `p5-perf-masuratori.md`); se notează și `shared hit/read`
  (la ×10 datele depășesc `shared_buffers` — se declară). Dimensiuni: tabelă,
  indexi, per partiție (`pg_total_relation_size`), lățimea medie a rândului.
  Scrierea (Q14): timpul unei tranzacții de ~50 de linii pe fiecare formă,
  cu ROLLBACK, mediană din 5; și încărcarea în bloc a unei luni (INSERT …
  SELECT), o dată.
- **FZ-D9 Ce NU face pasul**: nu optimizează Postgres (setări), nu compară
  cu SQL Server, nu decide modelarea documentelor (unificarea stoc/contabil,
  linia FCT vs NIR), nu proiectează API. Cifrele se COMPARĂ DOAR cu ele
  însele pe aceeași bază (capcana din CLAUDE.md).

## Livrabile

- `run-nucleu/fizica/01-interogari-azi.md` + `f0/qNN.sql` (explorare 01).
- `run-nucleu/fizica/02-semantica-mapare.md` (explorare 02, salvată de main).
- `run-nucleu/fizica/pas1/`: DDL-ul formelor, SQL-ul de transformare, al
  replicării, al reconcilierii, cu ieșirile lor.
- `run-nucleu/fizica/pas2/`: `qNN-{f0,f1,f2,f3,f2s,f3s}-{x1,x10}.txt`
  (EXPLAIN-urile), `rezultate.md` (tabelul medianelor + dimensiuni),
  `egalitate.md` (diff-urile de rezultate F0 ↔ F2).
- `docs/nucleu/nucleu-fizica.md`: sinteza (main) + review advers aplicat;
  `nucleu-cub-design.md` §11 bifat; CLAUDE.md neatins (regula: nu rezumate).

## Regula de oprire a pasului

Când fiecare interogare are cifră pe fiecare formă la ambele scări, cu
egalitatea F0 ↔ F2 verificată la ×1, și sinteza răspunde la: (1) care formă,
(2) ce indexi, (3) ce cumpără snapshot-ul, (4) ce se rupe la ×10, (5) ce nu
s-a putut proba — pasul e închis. Nu se extinde la modelare.
