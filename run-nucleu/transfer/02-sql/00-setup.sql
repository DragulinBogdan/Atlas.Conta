-- Explorarea 02 (partida + imperecherea): tabelele de lucru.
-- Scrie DOAR in schema `tr`. Idempotent. Baza: Atlas.Conta.Nucleu.Fizica.x1.
\set ON_ERROR_STOP on
\timing on

CREATE SCHEMA IF NOT EXISTS tr;

-- ---------- 1. Picioarele contabile, despicate pe latura (ca f1/f2) ----------
-- Aceeasi regula de Partener ca transformarea cubului (amendamentul 1):
-- repartitorul de tip Partener/Angajat de pe ORICARE latura a randului.
DROP TABLE IF EXISTS tr."Picior";
CREATE UNLOGGED TABLE tr."Picior" AS
SELECT r."ID" AS "RandId", v."Latura", v."Cont",
       r."Data", r."DocumentId", r."DetaliuId", r."Valoare", r."Storno",
       c."Simbol" AS "ContSimbol",
       COALESCE(c."RolTert", 0) AS "RolTert",
       COALESCE(
         CASE WHEN rp."ClrType"  IN ('Partener','Angajat') THEN v."RepartitorId" END,
         CASE WHEN rpp."ClrType" IN ('Partener','Angajat') THEN v."RepartitorPereche" END) AS "Partener",
       rp."ClrType" AS "TipRepartitorLatura"
FROM "RegistruContabil" r
CROSS JOIN LATERAL (VALUES
  (1::smallint, r."ContDebitId",  r."DimensiuniDebit_RepartitorId",  r."DimensiuniCredit_RepartitorId"),
  (2::smallint, r."ContCreditId", r."DimensiuniCredit_RepartitorId", r."DimensiuniDebit_RepartitorId")
) AS v("Latura","Cont","RepartitorId","RepartitorPereche")
LEFT JOIN "Conturi" c      ON c."ID"  = v."Cont"
LEFT JOIN "Repartitori" rp  ON rp."ID"  = v."RepartitorId"
LEFT JOIN "Repartitori" rpp ON rpp."ID" = v."RepartitorPereche";
CREATE INDEX ON tr."Picior"("DocumentId");
CREATE INDEX ON tr."Picior"("Cont");
ANALYZE tr."Picior";

-- ---------- 2. Postarile de TERT, agregate pe (document, cont, latura) ----------
DROP TABLE IF EXISTS tr."TertDocCont";
CREATE UNLOGGED TABLE tr."TertDocCont" AS
SELECT p."DocumentId", p."Cont", p."ContSimbol", p."RolTert", p."Latura",
       min(p."Partener"::text)::uuid AS "Partener",
       count(DISTINCT p."Partener") AS "ParteneriDistincti",
       sum(p."Valoare")  AS "Valoare",
       count(*)          AS "Picioare"
FROM tr."Picior" p
WHERE p."RolTert" <> 0 AND p."DocumentId" IS NOT NULL
GROUP BY 1,2,3,4,5;
CREATE INDEX ON tr."TertDocCont"("DocumentId");
ANALYZE tr."TertDocCont";

-- 2b. Un rand per (document, cont): soldul SEMNAT al contului de tert pe
--     documentul acela. Conventie: debit = +, credit = -, apoi se aduce la
--     SENSUL contului prin `RolTert` la nevoie.
DROP TABLE IF EXISTS tr."TertDocContNet";
CREATE UNLOGGED TABLE tr."TertDocContNet" AS
SELECT "DocumentId", "Cont", max("ContSimbol") AS "ContSimbol",
       max("RolTert") AS "RolTert",
       sum(CASE WHEN "Latura" = 1 THEN "Valoare" ELSE 0 END) AS "Debit",
       sum(CASE WHEN "Latura" = 2 THEN "Valoare" ELSE 0 END) AS "Credit",
       sum(CASE WHEN "Latura" = 1 THEN "Valoare" ELSE -"Valoare" END) AS "NetDebitor",
       count(DISTINCT "Latura") AS "Laturi",
       min("Partener"::text)::uuid AS "Partener"
FROM tr."TertDocCont"
GROUP BY 1,2;
ALTER TABLE tr."TertDocContNet" ADD PRIMARY KEY ("DocumentId","Cont");
ANALYZE tr."TertDocContNet";

-- ---------- 3. Antetul documentului, cu tipul ----------
DROP TABLE IF EXISTS tr."Doc";
CREATE UNLOGGED TABLE tr."Doc" AS
SELECT d."ID", d."ClrType", d."Numar", d."Data", d."DataInregistrare",
       d."Stare", d."TotalStingere", d."DocumentSursaId", d."Autogenerat",
       d."PredatorId", d."PrimitorId"
FROM "Documente" d
WHERE d."GCRecord" = 0;
ALTER TABLE tr."Doc" ADD PRIMARY KEY ("ID");
CREATE INDEX ON tr."Doc"("ClrType");
CREATE INDEX ON tr."Doc"("DocumentSursaId");
ANALYZE tr."Doc";

-- ---------- 4. Asignatul algebric per document (ambele roluri), la o data ----------
DROP TABLE IF EXISTS tr."Asignat";
CREATE UNLOGGED TABLE tr."Asignat" AS
SELECT u."Doc" AS "DocumentId", sum(u."Suma") AS "Asignat"
FROM (
  SELECT "DocumentStingatorId" AS "Doc", "Suma" FROM "Imperecheri" WHERE "GCRecord" = 0
  UNION ALL
  SELECT "DocumentId"          AS "Doc", "Suma" FROM "Imperecheri" WHERE "GCRecord" = 0
) u GROUP BY 1;
ALTER TABLE tr."Asignat" ADD PRIMARY KEY ("DocumentId");
ANALYZE tr."Asignat";

SELECT 'picioare' k, count(*) v FROM tr."Picior"
UNION ALL SELECT 'tert_doc_cont', count(*) FROM tr."TertDocCont"
UNION ALL SELECT 'tert_doc_cont_net', count(*) FROM tr."TertDocContNet"
UNION ALL SELECT 'documente', count(*) FROM tr."Doc"
UNION ALL SELECT 'asignat', count(*) FROM tr."Asignat";
