# Pas 1 (fizica): clona, schema cubului în trei forme, transformarea, reconcilierea, replicarea ×10

Contractul: `run-nucleu/fizica/CONTRACT.md` (citește-l integral; FZ-D1…D9
sunt decizii luate, nu le rediscuți). Semantica registrelor, cu dovezi:
`run-nucleu/fizica/02-semantica-mapare.md` (citește-l integral — maparea de
mai jos e derivată din el). Designul: `docs/nucleu/nucleu-cub-design.md` §2
și `docs/nucleu/nucleu-coordonate-rapoarte.md` §2 (doar cât să știi ce e o
coordonată; nu re-deriva).

Mediu: Postgres 18.4 în docker `contapal-postgres-1`, fără psql local:
`docker exec contapal-postgres-1 psql -U postgres -d <baza> -Atc "..."`;
pentru scripturi: `docker cp fisier.sql contapal-postgres-1:/tmp/` apoi
`psql ... -f /tmp/fisier.sql`. Toate scripturile SQL le ții în
`run-nucleu/fizica/pas1/` (numerotate, rulabile în ordine) și ieșirile lor în
`run-nucleu/fizica/pas1/out/`. Flax NU se atinge (nici măcar un index).

## 1. Clona

`CREATE DATABASE "Atlas.Conta.Nucleu.Fizica.x1" TEMPLATE
"Atlas.Conta.Import1C.Flax"` (merge doar fără conexiuni active pe sursă —
dacă pică, raportezi cine e conectat, nu forțezi). În clonă, schema `public`
= F0 (neatinsă). Cubul stă în scheme separate: `f1`, `f2`, `f3`, plus
`cub` pentru tabelele comune (`Tranzactie`, `CodTva`).

## 2. Schema logică (comună celor trei forme)

```sql
cub."Tranzactie"(ID uuid PK, DocumentId uuid NULL, Fel smallint  -- 1 Operare, 2 Storno, 3 Deschidere
                 , Data date, ScrisLa timestamptz)
cub."CodTva"(ID int PK, TipTvaId uuid, Regim smallint, Cota numeric(5,2), Sens smallint)  -- DISTINCT din RegistruTva
"Postare"(
  ID uuid, Spatiu smallint NOT NULL,                       -- 1 Contabil, 2 Stoc, 3 Fiscal
  TranzactieId uuid NOT NULL, DocumentId uuid NULL, LinieId uuid NULL,
  Data date NOT NULL,
  Cont uuid NULL, Latura smallint NULL,                    -- 1 D, 2 C; NOT NULL doar pe Contabil
  Partener uuid NULL, Gestiune uuid NULL, Produs uuid NULL, Unitate uuid NULL,
  CodTvaId int NULL, RolTva smallint NULL,                 -- 1 Bază, 2 Taxă
  PerioadaDeclarare int NULL,                              -- AAAALL
  Carte smallint NOT NULL DEFAULT 1, Valuta char(3) NULL,
  CodFunctional uuid NULL, CodEconomic uuid NULL, SursaFinantare uuid NULL,
  UnitateOrganizatorica uuid NULL, Proiect uuid NULL, CentruCost uuid NULL,
  Atribuit uuid NULL,
  Cantitate numeric(18,3) NOT NULL DEFAULT 0,
  ValoareValuta numeric(18,2) NOT NULL DEFAULT 0,
  Valoare numeric(18,2) NOT NULL DEFAULT 0)
```

Fără FK-uri fizice (măsurăm cubul, nu integritatea; se spune în raport).
`ID` = `gen_random_uuid()` (uuid v4; dacă `uuidv7()` există în PG 18,
folosește-l și notează — inserția ordonată contează la scriere).

Formele fizice (aceleași coloane, aceeași umplere, prin `INSERT … SELECT`
din `f1`):
- **f1."Postare"**: tabelă plată, PK pe `ID`, un btree separat pe FIECARE din:
  `TranzactieId, DocumentId, LinieId, Data, Cont, Partener, Gestiune, Produs,
  Unitate, CodTvaId, PerioadaDeclarare, CodFunctional, CodEconomic,
  SursaFinantare, UnitateOrganizatorica, Proiect, CentruCost` (portarea
  naivă „EF-style" — exact cum arată `RegistruContabil` azi).
- **f2."Postare"**: `PARTITION BY LIST ("Spatiu")`, trei partiții
  `Postare_Contabil` / `Postare_Stoc` / `Postare_Fiscal`. PK `(Spatiu, ID)`.
  În pasul 1 primește DOAR indexii de identitate: `(TranzactieId)`,
  `(DocumentId)`, `(LinieId)`. Indexii per uz îi proiectează pasul 2 din
  interogări; NU îi inventa acum.
- **f3."Postare"**: ca f2, iar fiecare partiție de spațiu e
  `PARTITION BY RANGE ("Data")` pe an (2016…2025 + DEFAULT). Aceiași indexi
  de identitate.

## 3. Transformarea (public → f1), determinist, în această ordine

1. `cub."CodTva"`: `SELECT DISTINCT "TipTvaId","Regim","Cota","Sens" FROM
   "RegistruTva"`, cu `ID` serial. Raportezi câte versiuni ies.
2. `cub."Tranzactie"`: una per `DocumentId` distinct din reuniunea celor trei
   registre (`Fel`=1, `Data` = `Documente.DataInregistrare`, `ScrisLa` =
   `Documente.DataOperare`), plus UNA cu `DocumentId NULL`, `Fel`=3, `Data`
   2024-12-31 pentru rândurile de deschidere. Storno nu apare (0 rânduri pe
   Flax — verifici și raportezi cifra).
3. Contabil — din fiecare rând `RegistruContabil` două postări, `Spatiu`=1:
   - Latura 1: `Cont`=`ContDebitId`, coordonatele din `DimensiuniDebit_*`;
     Latura 2: `Cont`=`ContCreditId`, din `DimensiuniCredit_*`.
   - `Partener` = `RepartitorId` al laturii DACĂ `Repartitori.ClrType` ∈
     {`Partener`, `Angajat`}; `Gestiune` = `RepartitorId` DACĂ `ClrType` ∈
     {`Gestiune`, `ContPropriu`, `UnitateInterna`}. Verifică valorile reale
     ale `ClrType` (`SELECT DISTINCT`) înainte; dacă apare altceva, oprire.
   - `Produs`=`MaterialId`; `CodFunctional`, `CodEconomic`, `SursaFinantare`,
     `UnitateOrganizatorica`=`UnitateId`, `Proiect`, `CentruCost` — din
     coloanele plate ale laturii.
   - `Valoare` = `Valoare` a rândului (semnul rămâne — storno „în roșu");
     `Data` = `Data` a rândului; `DocumentId`, `LinieId`=`DetaliuId`;
     `TranzactieId` = tranzacția documentului (sau cea de deschidere).
   - **Unitate (partida)** pe postările al căror `Cont` are `Conturi.RolTert
     <> 0`:
     - dacă documentul postării NU e stingător în `Imperecheri` (nu apare ca
       `DocumentStingatorId`): `Unitate` = `DocumentId` propriu (partida pe
       care o deschide);
     - dacă e stingător: postarea de terț se DESPICĂ — o postare per rând de
       `Imperecheri` al lui (`Unitate` = `Imperecheri.DocumentId`,
       `Valoare` = `Suma`, același semn/latură) plus, dacă
       `Valoare − Σ Suma ≠ 0`, o postare cu restul pe `Unitate` =
       `DocumentId` propriu. Dacă un document stingător are MAI MULTE
       postări de terț (mai multe rânduri contabile pe conturi de terț —
       numără!), despicarea se aplică o singură dată, pe postarea cu valoarea
       cea mai mare, restul rămân pe partida proprie; raportezi câte
       documente sunt în cazul ăsta (e o limită a modelului de măsurare,
       declarată).
     - Rândurile inverse (`InverseazaId IS NOT NULL`): pe Flax verifici că
       sunt 0; dacă nu, oprire și raport.
4. Stoc — o postare per rând `RegistruStoc`, `Spatiu`=2: `Cantitate`,
   `Valoare` cu semnul rândului; `Gestiune`=`RepartitorId` (inclusiv
   `UnitateInterna` = sink-ul de consum); `Unitate`=`LotId`;
   `Produs`=`Loturi.ProdusId`; **`Cont`** = `TipuriMaterial.ContImplicitId` pe
   lanțul `Loturi.ProdusId → Produse.TipMaterialId → TipuriMaterial` (acoperire
   totală pe Flax, semantica §8 — verifică 0 NULL); `Latura` NULL;
   `Data`, `DocumentId`, `LinieId`, `TranzactieId` ca la contabil.
5. Fiscal — două postări per rând `RegistruTva`, `Spatiu`=3: `RolTva`=1 cu
   `Valoare`=`Baza`, `RolTva`=2 cu `Valoare`=`Tva` (scrisă și când e 0 —
   numără câte au `Tva`=0); `CodTvaId` din versiune; `PerioadaDeclarare` =
   `PerioadaAn*100+PerioadaLuna`; `Partener`=`PartenerId`; `Data` = `Data` a
   rândului (fizică — așa e azi); `DocumentId`, `LinieId`, `TranzactieId`.
6. `GCRecord`: registrele au coloana; verifică dacă există rânduri cu
   `GCRecord IS NOT NULL` (șterse logic) și EXCLUDE-le, raportând cifra.

## 4. Reconcilierea — GATE, se rulează și se salvează ÎNAINTE de f2/f3 și de ×10

Fiecare probă = un SQL care întoarce numărul de abateri; toate trebuie 0
(sau egalitatea exactă a cifrelor). Salvezi ieșirile în `out/reconciliere.txt`.

- (a) per `TranzactieId` pe `Spatiu`=1: `Σ Valoare (Latura 1) = Σ Valoare
  (Latura 2)` — abateri = 0.
- (b) per `Cont`: `Σ Valoare` Latura 1 = `Σ Valoare` din `RegistruContabil`
  grupat pe `ContDebitId`; idem Latura 2 pe `ContCreditId`.
- (c) per `Unitate` (lot): `Σ Cantitate`, `Σ Valoare` = `RegistruStoc` per `LotId`.
- (d) per (`CodTvaId`, `PerioadaDeclarare`): `Σ Valoare` rol 1 = `Σ Baza`,
  rol 2 = `Σ Tva` din `RegistruTva` pe aceeași cheie.
- (e) numărători: Contabil = 2·295.013 + (postările în plus din despicarea
  pe partide, calculate independent: Σ peste stingători de (nr. împerecheri
  + [rest≠0] − 1)); Stoc = 274.031; Fiscal = 2·86.636. Scrii formula și
  cifrele.
- (f) per partidă de terț: `Σ Valoare(D) − Σ Valoare(C)` pe `Unitate` pentru
  facturile de ieșire = `PartideDeschise.Rest` la 12/2025 pentru aceleași
  documente — raportezi câte coincid și câte nu, cu 3 exemple de nepotrivire
  (aici pot apărea diferențe LEGITIME: `TotalStingere` e brut per document;
  nu le „repari", le raportezi).

O abatere pe (a)–(e) = OPRIRE (raportezi, nu normalizezi). (f) e informativ.

## 5. Umplerea f2 și f3, apoi ×10

- `INSERT INTO f2."Postare" SELECT * FROM f1."Postare"`; idem f3. `ANALYZE`.
- Replicarea: `CREATE DATABASE "Atlas.Conta.Nucleu.Fizica.x10" TEMPLATE
  "Atlas.Conta.Nucleu.Fizica.x1"`, apoi în ea, pentru k = 1…9: copiezi
  rândurile din 2025 cu `Data − k ani` (`PerioadaDeclarare − k·100`), ID-uri
  noi pentru postări și `Tranzactie` (același `DocumentId`, `LinieId`,
  coordonate), în f1, f2, f3 ȘI în F0 (`RegistruContabil`, `RegistruStoc`,
  `RegistruTva` — cu `ID` nou, restul identic, `Data`/`PerioadaAn` mutate;
  `SolduriPerioada*`, `PartideDeschise`, `Imperecheri`, `Documente` NU se
  replică — se declară). Rândurile de deschidere (2024-12-31) NU se replică.
  `ANALYZE` la final; `VACUUM` nu e necesar pe tabele proaspete.
- Mărimi: `pg_total_relation_size`, tabelă vs indexi, per formă și per
  partiție, la ×1 și ×10, plus lățimea medie a rândului
  (`pg_relation_size / reltuples`) pentru f1 vs `RegistruContabil` — în
  `out/marimi.txt`.

## Reguli

- Fiecare script e idempotent la re-rulare (DROP … IF EXISTS la început).
- Nu atingi `Atlas.Conta.Import1C.Flax*`; nu atingi cod din `nou/`; nu comiți.
- Regulă de oprire: orice abatere la reconciliere, orice valoare neașteptată
  (ClrType necunoscut, cont de stoc NULL, împerechere inversă, GCRecord
  nenul în cantitate mare) → te oprești și raportezi cu cifre. NU normalizezi
  tăcut. Dacă transformarea unei categorii nu se poate face conform spec-ului,
  o lași NEfăcută și spui de ce.
- Rulările lungi: transformarea ×1 durează minute, ×10 poate dura zeci de
  minute — rulează-le ca proces detașat cu log (`Start-Process`/`nohup`), nu
  ca task de fundal al harness-ului (secerat la 10 min), și verifică
  pulsul cu `count(*)`.
- Raportul final către coordonator (max 20 de linii): cifrele de
  reconciliere (a)–(f), numărul de postări per spațiu la ×1 și ×10, mărimile
  f1/f2/f3 vs F0, versiunile CodTva, documentele cu mai multe postări de
  terț, timpul transformării și al replicării, și tot ce a fost neașteptat.
