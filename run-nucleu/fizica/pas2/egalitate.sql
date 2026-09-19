-- Pas 2: dump-urile de egalitate F0 <-> F2 la x1. Fiecare interogare are o PROIECTIE
-- DECLARATA (cheia + masurile comparabile), dump-uita sortat pe ambele parti; diff-ul
-- il face gazda. Rotunjirile: bani la 2 zecimale, cantitati la 3, ca sa nu se compare
-- scale numerice diferite.
-- Coloanele ne-comparabile se EXCLUD explicit, cu motivul scris langa fiecare.
\set ON_ERROR_STOP on

\set C4111 '01a0b48c-a8b2-73ce-a6cd-a045b80364a8'
\set PART  '01a0b48c-a9fe-73e1-89f8-2908bb52cd65'
\set GEST  '01a0b48c-d7ca-73c5-88b4-c0b508a78342'
\set PROD  '01a0b48d-78b6-754c-92e9-c3250a37995b'
\set DOC228 '01a0b494-a2da-7484-94ce-1019f56c4fef'

-- ============ Q01 balanta sintetica: cheie = Cont, masuri = cele 4 sume ============
COPY (
  SELECT u."ContId"::text, round(sum(u."D0"),2), round(sum(u."C0"),2), round(sum(u."D1"),2), round(sum(u."C1"),2)
  FROM (
    SELECT r."ContDebitId" AS "ContId",
           CASE WHEN r."Data" <  DATE '2025-01-01' THEN r."Valoare" ELSE 0 END AS "D0", 0::numeric AS "C0",
           CASE WHEN r."Data" >= DATE '2025-01-01' THEN r."Valoare" ELSE 0 END AS "D1", 0::numeric AS "C1"
    FROM "RegistruContabil" r WHERE r."GCRecord" = 0 AND r."Data" <= DATE '2025-12-31'
    UNION ALL
    SELECT r."ContCreditId", 0,
           CASE WHEN r."Data" <  DATE '2025-01-01' THEN r."Valoare" ELSE 0 END, 0,
           CASE WHEN r."Data" >= DATE '2025-01-01' THEN r."Valoare" ELSE 0 END
    FROM "RegistruContabil" r WHERE r."GCRecord" = 0 AND r."Data" <= DATE '2025-12-31') u
  GROUP BY 1 ORDER BY 1) TO '/tmp/eg/q01-f0.csv' WITH (FORMAT csv);

COPY (
  SELECT p."Cont"::text,
         round(sum(CASE WHEN p."Data" <  DATE '2025-01-01' AND p."Latura" = 1 THEN p."Valoare" ELSE 0 END),2),
         round(sum(CASE WHEN p."Data" <  DATE '2025-01-01' AND p."Latura" = 2 THEN p."Valoare" ELSE 0 END),2),
         round(sum(CASE WHEN p."Data" >= DATE '2025-01-01' AND p."Latura" = 1 THEN p."Valoare" ELSE 0 END),2),
         round(sum(CASE WHEN p."Data" >= DATE '2025-01-01' AND p."Latura" = 2 THEN p."Valoare" ELSE 0 END),2)
  FROM f2."Postare" p WHERE p."Spatiu" = 1 AND p."Data" <= DATE '2025-12-31'
  GROUP BY 1 ORDER BY 1) TO '/tmp/eg/q01-f2.csv' WITH (FORMAT csv);

-- ============ Q02 balanta analitica Cont x Partener (4111, 401) ============
-- Referinta F0 = regula q02b (partenerul de pe oricare latura), NU q02.
CREATE OR REPLACE VIEW pg_temp."_AtomiPartener" AS
  SELECT r."Data", r."ContDebitId" AS "Cont", 1::smallint AS "Latura", r."Valoare",
         COALESCE(CASE WHEN rd."ClrType" IN ('Partener','Angajat') THEN r."DimensiuniDebit_RepartitorId" END,
                  CASE WHEN rc."ClrType" IN ('Partener','Angajat') THEN r."DimensiuniCredit_RepartitorId" END) AS "Partener"
  FROM "RegistruContabil" r
  LEFT JOIN "Repartitori" rd ON rd."ID" = r."DimensiuniDebit_RepartitorId"
  LEFT JOIN "Repartitori" rc ON rc."ID" = r."DimensiuniCredit_RepartitorId"
  WHERE r."GCRecord" = 0
  UNION ALL
  SELECT r."Data", r."ContCreditId", 2::smallint, r."Valoare",
         COALESCE(CASE WHEN rc."ClrType" IN ('Partener','Angajat') THEN r."DimensiuniCredit_RepartitorId" END,
                  CASE WHEN rd."ClrType" IN ('Partener','Angajat') THEN r."DimensiuniDebit_RepartitorId" END)
  FROM "RegistruContabil" r
  LEFT JOIN "Repartitori" rd ON rd."ID" = r."DimensiuniDebit_RepartitorId"
  LEFT JOIN "Repartitori" rc ON rc."ID" = r."DimensiuniCredit_RepartitorId"
  WHERE r."GCRecord" = 0;

COPY (
  SELECT a."Cont"::text, COALESCE(a."Partener"::text,''),
         round(sum(CASE WHEN a."Data" <  DATE '2025-01-01' AND a."Latura" = 1 THEN a."Valoare" ELSE 0 END),2),
         round(sum(CASE WHEN a."Data" <  DATE '2025-01-01' AND a."Latura" = 2 THEN a."Valoare" ELSE 0 END),2),
         round(sum(CASE WHEN a."Data" >= DATE '2025-01-01' AND a."Latura" = 1 THEN a."Valoare" ELSE 0 END),2),
         round(sum(CASE WHEN a."Data" >= DATE '2025-01-01' AND a."Latura" = 2 THEN a."Valoare" ELSE 0 END),2)
  FROM pg_temp."_AtomiPartener" a
  JOIN "Conturi" c ON c."ID" = a."Cont" AND c."Simbol" IN ('4111','401')
  WHERE a."Data" <= DATE '2025-12-31'
  GROUP BY 1,2 ORDER BY 1,2) TO '/tmp/eg/q02-f0.csv' WITH (FORMAT csv);

COPY (
  SELECT p."Cont"::text, COALESCE(p."Partener"::text,''),
         round(sum(CASE WHEN p."Data" <  DATE '2025-01-01' AND p."Latura" = 1 THEN p."Valoare" ELSE 0 END),2),
         round(sum(CASE WHEN p."Data" <  DATE '2025-01-01' AND p."Latura" = 2 THEN p."Valoare" ELSE 0 END),2),
         round(sum(CASE WHEN p."Data" >= DATE '2025-01-01' AND p."Latura" = 1 THEN p."Valoare" ELSE 0 END),2),
         round(sum(CASE WHEN p."Data" >= DATE '2025-01-01' AND p."Latura" = 2 THEN p."Valoare" ELSE 0 END),2)
  FROM f2."Postare" p JOIN "Conturi" c ON c."ID" = p."Cont" AND c."Simbol" IN ('4111','401')
  WHERE p."Spatiu" = 1 AND p."Data" <= DATE '2025-12-31'
  GROUP BY 1,2 ORDER BY 1,2) TO '/tmp/eg/q02-f2.csv' WITH (FORMAT csv);

-- Faptul cerut de spec: cat difera q02 (repartitorul laturii) de q02b (partenerul randului).
COPY (
  SELECT 'q02_chei_repartitor'::text, count(*)::text FROM (
    SELECT DISTINCT r."ContDebitId", r."DimensiuniDebit_RepartitorId" FROM "RegistruContabil" r
    JOIN "Conturi" c ON c."ID" = r."ContDebitId" AND c."Simbol" IN ('4111','401')
    UNION SELECT DISTINCT r."ContCreditId", r."DimensiuniCredit_RepartitorId" FROM "RegistruContabil" r
    JOIN "Conturi" c ON c."ID" = r."ContCreditId" AND c."Simbol" IN ('4111','401')) x
  UNION ALL
  SELECT 'q02b_chei_partener', count(*)::text FROM (
    SELECT DISTINCT a."Cont", a."Partener" FROM pg_temp."_AtomiPartener" a
    JOIN "Conturi" c ON c."ID" = a."Cont" AND c."Simbol" IN ('4111','401')) y
  ) TO '/tmp/eg/q02-fapt.csv' WITH (FORMAT csv);

-- ============ Q03 fisa 4111 ============
-- EXCLUS: SoldCurent (depinde de ORDINEA ferestrei -- F0 ordoneaza pe (Data, Id, Sens DESC),
-- cubul pe (Data, TranzactieId, ID); soldul FINAL se compara separat, mai jos).
-- EXCLUS: Contrapartida (pe cub e lista conturilor tranzactiei, pe F0 contul celeilalte
-- laturi a RANDULUI -- diferenta e semantica, masurata separat in q03c).
COPY (
  SELECT f."Data"::text, f."Sens", round(f."Debit",2), round(f."Credit",2), COALESCE(f."DocumentId"::text,'')
  FROM (
    SELECT r."Data", 'D' AS "Sens", r."Valoare" AS "Debit", 0::numeric AS "Credit", r."DocumentId"
    FROM "RegistruContabil" r WHERE r."GCRecord"=0 AND r."ContDebitId"=:'C4111' AND r."Data" <= DATE '2025-12-31'
    UNION ALL
    SELECT r."Data", 'C', 0, r."Valoare", r."DocumentId"
    FROM "RegistruContabil" r WHERE r."GCRecord"=0 AND r."ContCreditId"=:'C4111' AND r."Data" <= DATE '2025-12-31') f
  WHERE f."Data" >= DATE '2025-01-01'
  ORDER BY 1,2,3,4,5) TO '/tmp/eg/q03-f0.csv' WITH (FORMAT csv);

COPY (
  SELECT p."Data"::text, CASE WHEN p."Latura"=1 THEN 'D' ELSE 'C' END,
         round(CASE WHEN p."Latura"=1 THEN p."Valoare" ELSE 0 END,2),
         round(CASE WHEN p."Latura"=2 THEN p."Valoare" ELSE 0 END,2),
         COALESCE(p."DocumentId"::text,'')
  FROM f2."Postare" p
  WHERE p."Spatiu"=1 AND p."Cont"=:'C4111' AND p."Data" BETWEEN DATE '2025-01-01' AND DATE '2025-12-31'
  ORDER BY 1,2,3,4,5) TO '/tmp/eg/q03-f2.csv' WITH (FORMAT csv);

COPY (
  SELECT 'sold_final_f0'::text, round(sum(CASE WHEN r."ContDebitId"=:'C4111' THEN r."Valoare" ELSE 0 END)
                                    - sum(CASE WHEN r."ContCreditId"=:'C4111' THEN r."Valoare" ELSE 0 END),2)::text
  FROM "RegistruContabil" r
  WHERE r."GCRecord"=0 AND (r."ContDebitId"=:'C4111' OR r."ContCreditId"=:'C4111') AND r."Data" <= DATE '2025-12-31'
  UNION ALL
  SELECT 'sold_final_f2', round(sum(CASE WHEN p."Latura"=1 THEN p."Valoare" ELSE -p."Valoare" END),2)::text
  FROM f2."Postare" p WHERE p."Spatiu"=1 AND p."Cont"=:'C4111' AND p."Data" <= DATE '2025-12-31'
  ) TO '/tmp/eg/q03-sold.csv' WITH (FORMAT csv);

-- ============ Q04 fisa 4111 x partener (referinta F0 = regula q04b) ============
COPY (
  SELECT a."Data"::text, CASE WHEN a."Latura"=1 THEN 'D' ELSE 'C' END, round(a."Valoare",2)
  FROM pg_temp."_AtomiPartener" a
  WHERE a."Cont"=:'C4111' AND a."Partener"=:'PART' AND a."Data" BETWEEN DATE '2025-01-01' AND DATE '2025-12-31'
  ORDER BY 1,2,3) TO '/tmp/eg/q04-f0.csv' WITH (FORMAT csv);

COPY (
  SELECT p."Data"::text, CASE WHEN p."Latura"=1 THEN 'D' ELSE 'C' END, round(p."Valoare",2)
  FROM f2."Postare" p
  WHERE p."Spatiu"=1 AND p."Cont"=:'C4111' AND p."Partener"=:'PART'
    AND p."Data" BETWEEN DATE '2025-01-01' AND DATE '2025-12-31'
  ORDER BY 1,2,3) TO '/tmp/eg/q04-f2.csv' WITH (FORMAT csv);

-- ============ Q05 sold 4111 x partener la 31.12 ============
-- F0-ul REAL (f0/q05.sql) citeste SolduriPerioadaContabil, care e cheiat pe REPARTITORUL
-- laturii, nu pe partener: nu e comparabil pe cheie. Referinta declarata = acelasi registru
-- citit cu regula partenerului (pg_temp."_AtomiPartener"), cumulat pana la 31.12.
COPY (
  SELECT COALESCE(a."Partener"::text,''),
         round(sum(CASE WHEN a."Latura"=1 THEN a."Valoare" ELSE 0 END),2),
         round(sum(CASE WHEN a."Latura"=2 THEN a."Valoare" ELSE 0 END),2)
  FROM pg_temp."_AtomiPartener" a
  WHERE a."Cont"=:'C4111' AND a."Data" <= DATE '2025-12-31'
  GROUP BY 1 HAVING sum(CASE WHEN a."Latura"=1 THEN a."Valoare" ELSE -a."Valoare" END) <> 0
  ORDER BY 1) TO '/tmp/eg/q05-f0.csv' WITH (FORMAT csv);

COPY (
  SELECT COALESCE(p."Partener"::text,''),
         round(sum(CASE WHEN p."Latura"=1 THEN p."Valoare" ELSE 0 END),2),
         round(sum(CASE WHEN p."Latura"=2 THEN p."Valoare" ELSE 0 END),2)
  FROM f2."Postare" p WHERE p."Spatiu"=1 AND p."Cont"=:'C4111' AND p."Data" <= DATE '2025-12-31'
  GROUP BY 1 HAVING sum(CASE WHEN p."Latura"=1 THEN p."Valoare" ELSE -p."Valoare" END) <> 0
  ORDER BY 1) TO '/tmp/eg/q05-f2.csv' WITH (FORMAT csv);

-- Faptul: cate chei are snapshot-ul de azi pe 4111 (repartitor) fata de cub (partener).
COPY (
  SELECT 'q05_chei_snapshot_repartitor'::text, count(*)::text
  FROM "SolduriPerioadaContabil" s WHERE s."GCRecord"=0 AND s."An"=2025 AND s."Luna"=12 AND s."ContId"=:'C4111'
  UNION ALL
  SELECT 'q05_chei_cub_partener', count(DISTINCT COALESCE(p."Partener"::text,'-'))::text
  FROM f2."Postare" p WHERE p."Spatiu"=1 AND p."Cont"=:'C4111' AND p."Data" <= DATE '2025-12-31'
  ) TO '/tmp/eg/q05-fapt.csv' WITH (FORMAT csv);

-- ============ Q06 partide cu rest la 31.12, pe INTERSECTIA tipurilor ============
-- F0 acopera SASE tipuri concrete; cubul deschide partida pe ORICE document care
-- posteaza pe un cont de tert. Intersectia declarata = cele sase tipuri ale lui F0.
COPY (
  SELECT p."DocumentId"::text, round(p."Rest",2)
  FROM "PartideDeschise" p JOIN "Documente" d ON d."ID" = p."DocumentId"
  WHERE p."GCRecord"=0 AND p."An"=2025 AND p."Luna"=12 AND p."Rest" <> 0
    AND d."ClrType" IN ('FacturaIntrare','FacturaIesire','Plata','Incasare','Decont','ReturClient')
  ORDER BY 1) TO '/tmp/eg/q06-f0.csv' WITH (FORMAT csv);

COPY (
  SELECT g."Unitate"::text, round(abs(g."Sold"),2) FROM (
    SELECT p."Unitate", sum(CASE WHEN p."Latura"=1 THEN p."Valoare" ELSE -p."Valoare" END) AS "Sold"
    FROM f2."Postare" p JOIN "Conturi" c ON c."ID" = p."Cont" AND c."RolTert" <> 0
    JOIN "Documente" d ON d."ID" = p."Unitate"
    WHERE p."Spatiu"=1 AND p."Data" <= DATE '2025-12-31' AND p."Unitate" IS NOT NULL
      AND d."ClrType" IN ('FacturaIntrare','FacturaIesire','Plata','Incasare','Decont','ReturClient')
    GROUP BY 1) g
  WHERE g."Sold" <> 0 ORDER BY 1) TO '/tmp/eg/q06-f2.csv' WITH (FORMAT csv);

COPY (
  SELECT d."ClrType", count(*)::text FROM "PartideDeschise" p JOIN "Documente" d ON d."ID"=p."DocumentId"
  WHERE p."GCRecord"=0 AND p."An"=2025 AND p."Luna"=12 AND p."Rest" <> 0
  GROUP BY 1 ORDER BY 1) TO '/tmp/eg/q06-tipuri-f0.csv' WITH (FORMAT csv);

-- ============ Q07 jurnal cumparari 2025-06, Sens = Achizitie ============
COPY (
  SELECT r."DocumentId"::text, r."TipTvaId"::text, r."Regim"::text, r."Cota"::numeric(5,2)::text,
         round(sum(r."Baza"),2), round(sum(r."Tva"),2)
  FROM "RegistruTva" r
  WHERE r."GCRecord"=0 AND r."Sens"=1 AND r."PerioadaAn"=2025 AND r."PerioadaLuna"=6
  GROUP BY 1,2,3,4 ORDER BY 1,2,3,4) TO '/tmp/eg/q07-f0.csv' WITH (FORMAT csv);

COPY (
  SELECT p."DocumentId"::text, ct."TipTvaId"::text, ct."Regim"::text, ct."Cota"::text,
         round(sum(CASE WHEN p."RolTva"=1 THEN p."Valoare" ELSE 0 END),2),
         round(sum(CASE WHEN p."RolTva"=2 THEN p."Valoare" ELSE 0 END),2)
  FROM f2."Postare" p JOIN cub."CodTva" ct ON ct."ID"=p."CodTvaId"
  WHERE p."Spatiu"=3 AND p."PerioadaDeclarare"=202506 AND ct."Sens"=1
  GROUP BY 1,2,3,4 ORDER BY 1,2,3,4) TO '/tmp/eg/q07-f2.csv' WITH (FORMAT csv);

-- ============ Q08 D394 2025-06 ============
-- Cheia F0 NU contine Regim; cubul o are in CodTvaId -> cubul se ridica la cheia F0.
COPY (
  SELECT r."DocumentId"::text, COALESCE(r."PartenerId"::text,''), r."Sens"::text,
         r."TipTvaId"::text, r."Cota"::numeric(5,2)::text, round(sum(r."Baza"),2), round(sum(r."Tva"),2)
  FROM "RegistruTva" r
  WHERE r."GCRecord"=0 AND r."PerioadaAn"=2025 AND r."PerioadaLuna"=6
  GROUP BY 1,2,3,4,5 ORDER BY 1,2,3,4,5) TO '/tmp/eg/q08-f0.csv' WITH (FORMAT csv);

COPY (
  SELECT p."DocumentId"::text, COALESCE(p."Partener"::text,''), ct."Sens"::text,
         ct."TipTvaId"::text, ct."Cota"::text,
         round(sum(CASE WHEN p."RolTva"=1 THEN p."Valoare" ELSE 0 END),2),
         round(sum(CASE WHEN p."RolTva"=2 THEN p."Valoare" ELSE 0 END),2)
  FROM f2."Postare" p JOIN cub."CodTva" ct ON ct."ID"=p."CodTvaId"
  WHERE p."Spatiu"=3 AND p."PerioadaDeclarare"=202506
  GROUP BY 1,2,3,4,5 ORDER BY 1,2,3,4,5) TO '/tmp/eg/q08-f2.csv' WITH (FORMAT csv);

-- ============ Q09 FIFO (produs x gestiune la 30.06) ============
-- F0 filtreaza SI TipStoc=1; cubul n-are TipStoc -- diferenta se raporteaza.
COPY (
  SELECT m."LotId"::text, round(sum(m."Cantitate"),3)
  FROM "RegistruStoc" m JOIN "Loturi" l ON l."ID"=m."LotId"
  WHERE m."GCRecord"=0 AND m."Data" <= DATE '2025-06-30' AND l."ProdusId"=:'PROD'
    AND m."RepartitorId"=:'GEST' AND m."TipStoc"=1
  GROUP BY 1 HAVING sum(m."Cantitate") > 0 ORDER BY 1) TO '/tmp/eg/q09-f0.csv' WITH (FORMAT csv);

COPY (
  SELECT p."Unitate"::text, round(sum(p."Cantitate"),3)
  FROM f2."Postare" p
  WHERE p."Spatiu"=2 AND p."Produs"=:'PROD' AND p."Gestiune"=:'GEST' AND p."Data" <= DATE '2025-06-30'
  GROUP BY 1 HAVING sum(p."Cantitate") > 0 ORDER BY 1) TO '/tmp/eg/q09-f2.csv' WITH (FORMAT csv);

-- ============ Q10 PhysicalStock la 31.12 ============
-- F0 grupeaza pe (Repartitor, Lot, TipStoc) si reduce peste TipStoc in memorie: aici se
-- reduce in SQL, ca sa fie acelasi gran cu cubul.
COPY (
  SELECT r."RepartitorId"::text, r."LotId"::text, round(sum(r."Cantitate"),3), round(sum(r."Valoare"),2)
  FROM "RegistruStoc" r WHERE r."GCRecord"=0 AND r."Data" <= DATE '2025-12-31'
  GROUP BY 1,2 HAVING sum(r."Cantitate") <> 0 ORDER BY 1,2) TO '/tmp/eg/q10-f0.csv' WITH (FORMAT csv);

COPY (
  SELECT p."Gestiune"::text, p."Unitate"::text, round(sum(p."Cantitate"),3), round(sum(p."Valoare"),2)
  FROM f2."Postare" p WHERE p."Spatiu"=2 AND p."Data" <= DATE '2025-12-31'
  GROUP BY 1,2 HAVING sum(p."Cantitate") <> 0 ORDER BY 1,2) TO '/tmp/eg/q10-f2.csv' WITH (FORMAT csv);

-- ============ Q11 terti SAF-T 2025-06 ============
-- F0-ul real intoarce perechea de conturi a randului si lasa reducerea pe (Partener, Cont,
-- Latura) in C# (SaftProiectii.PartenerulRandului). Referinta = acea reducere, scrisa in SQL.
COPY (
  SELECT COALESCE(a."Partener"::text,''), a."Cont"::text, a."Latura"::text,
         round(sum(CASE WHEN a."Data" <  DATE '2025-06-01' THEN a."Valoare" ELSE 0 END),2),
         round(sum(CASE WHEN a."Data" >= DATE '2025-06-01' THEN a."Valoare" ELSE 0 END),2)
  FROM pg_temp."_AtomiPartener" a JOIN "Conturi" c ON c."ID"=a."Cont" AND c."RolTert" <> 0
  WHERE a."Data" <= DATE '2025-06-30'
  GROUP BY 1,2,3 ORDER BY 1,2,3) TO '/tmp/eg/q11-f0.csv' WITH (FORMAT csv);

COPY (
  SELECT COALESCE(p."Partener"::text,''), p."Cont"::text, p."Latura"::text,
         round(sum(CASE WHEN p."Data" <  DATE '2025-06-01' THEN p."Valoare" ELSE 0 END),2),
         round(sum(CASE WHEN p."Data" >= DATE '2025-06-01' THEN p."Valoare" ELSE 0 END),2)
  FROM f2."Postare" p JOIN "Conturi" c ON c."ID"=p."Cont" AND c."RolTert" <> 0
  WHERE p."Spatiu"=1 AND p."Data" <= DATE '2025-06-30'
  GROUP BY 1,2,3 ORDER BY 1,2,3) TO '/tmp/eg/q11-f2.csv' WITH (FORMAT csv);

-- ============ Q12 GLE 2025-06: randurile lunii, despicate pe latura ============
COPY (
  SELECT u."DocumentId"::text, COALESCE(u."LinieId"::text,''), u."Cont"::text, u."Latura"::text, round(u."Valoare",2)
  FROM (
    SELECT r."DocumentId", r."DetaliuId" AS "LinieId", r."ContDebitId" AS "Cont", 1 AS "Latura", r."Valoare"
    FROM "RegistruContabil" r
    WHERE r."GCRecord"=0 AND r."Data" BETWEEN DATE '2025-06-01' AND DATE '2025-06-30' AND r."DocumentId" IS NOT NULL
    UNION ALL
    SELECT r."DocumentId", r."DetaliuId", r."ContCreditId", 2, r."Valoare"
    FROM "RegistruContabil" r
    WHERE r."GCRecord"=0 AND r."Data" BETWEEN DATE '2025-06-01' AND DATE '2025-06-30' AND r."DocumentId" IS NOT NULL) u
  ORDER BY 1,2,3,4,5) TO '/tmp/eg/q12-f0.csv' WITH (FORMAT csv);

COPY (
  SELECT p."DocumentId"::text, COALESCE(p."LinieId"::text,''), p."Cont"::text, p."Latura"::text, round(p."Valoare",2)
  FROM f2."Postare" p
  WHERE p."Spatiu"=1 AND p."Data" BETWEEN DATE '2025-06-01' AND DATE '2025-06-30' AND p."DocumentId" IS NOT NULL
  ORDER BY 1,2,3,4,5) TO '/tmp/eg/q12-f2.csv' WITH (FORMAT csv);

-- ============ Q13 documentul de storno (228 randuri contabile) ============
COPY (
  SELECT u."Spatiu"::text, u."Cont"::text, u."Latura"::text, round(u."Valoare",2)
  FROM (
    SELECT 1 AS "Spatiu", r."ContDebitId" AS "Cont", 1 AS "Latura", r."Valoare"
    FROM "RegistruContabil" r WHERE r."GCRecord"=0 AND r."DocumentId"=:'DOC228'
    UNION ALL
    SELECT 1, r."ContCreditId", 2, r."Valoare"
    FROM "RegistruContabil" r WHERE r."GCRecord"=0 AND r."DocumentId"=:'DOC228') u
  ORDER BY 1,2,3,4) TO '/tmp/eg/q13-f0.csv' WITH (FORMAT csv);

COPY (
  SELECT p."Spatiu"::text, p."Cont"::text, p."Latura"::text, round(p."Valoare",2)
  FROM f2."Postare" p WHERE p."DocumentId"=:'DOC228'
  ORDER BY 1,2,3,4) TO '/tmp/eg/q13-f2.csv' WITH (FORMAT csv);
