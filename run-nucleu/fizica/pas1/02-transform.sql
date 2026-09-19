-- Pas 1: transformarea public (F0) -> f1."Postare". Determinista.
-- ID-uri: uuidv7() (exista in PG 18) - inserare ordonata.
-- Amendamentul 1 (lead, 2026-09-19): Partener = repartitorul de tip
--   Partener/Angajat de pe ORICARE latura a randului, scris pe ambele postari
--   (fiecare latura il ia pe al ei cand il are).
-- Amendamentul 3 (lead, 2026-09-19): imperecherea e TRANZACTIE DATATA, nu
--   despicare. Postarea de tert primeste partida PROPRIE; fiecare rand
--   "Imperecheri" da o tranzactie Fel=4 cu doua postari pe acelasi cont/latura
--   (-Suma pe partida stingatorului, +Suma pe partida facturii).
\set ON_ERROR_STOP on
\timing on

-- ---------- 1. cub."CodTva" ----------
TRUNCATE cub."CodTva" CASCADE;
INSERT INTO cub."CodTva"("ID","TipTvaId","Regim","Cota","Sens")
SELECT row_number() OVER (ORDER BY "TipTvaId","Regim","Cota","Sens"),
       "TipTvaId","Regim"::smallint,"Cota"::numeric(5,2),"Sens"::smallint
FROM (SELECT DISTINCT "TipTvaId","Regim","Cota","Sens" FROM "RegistruTva") d;
SELECT 'codtva_versiuni' k, count(*) v FROM cub."CodTva";

-- ---------- 2. cub."Tranzactie" (operare + deschidere) ----------
TRUNCATE cub."Tranzactie";
INSERT INTO cub."Tranzactie"("ID","DocumentId","Fel","Data","ScrisLa")
SELECT uuidv7(), d."ID", 1::smallint, d."DataInregistrare", d."DataOperare"
FROM "Documente" d
WHERE d."ID" IN (
  SELECT "DocumentId" FROM "RegistruContabil" WHERE "DocumentId" IS NOT NULL
  UNION SELECT "DocumentId" FROM "RegistruStoc" WHERE "DocumentId" IS NOT NULL
  UNION SELECT "DocumentId" FROM "RegistruTva");
INSERT INTO cub."Tranzactie"("ID","DocumentId","Fel","Data","ScrisLa")
VALUES (uuidv7(), NULL, 3::smallint, DATE '2024-12-31', NULL);

CREATE OR REPLACE VIEW cub."_TranzactiePeDocument" AS
SELECT "DocumentId", "ID" AS "TranzactieId" FROM cub."Tranzactie" WHERE "Fel"=1;

-- ---------- 3. Contabil: doua postari per rand ----------
TRUNCATE f1."Postare";
DROP TABLE IF EXISTS cub."_C";
CREATE UNLOGGED TABLE cub."_C" AS
SELECT r."ID" AS "RandId", v."Latura", v."Cont",
       v."RepartitorId", v."RepartitorPereche",
       v."MaterialId", v."CodFunctionalId", v."CodEconomicId",
       v."SursaFinantareId", v."UnitateId", v."ProiectId", v."CentruCostId",
       r."Data", r."DocumentId", r."DetaliuId", r."Valoare",
       COALESCE(c."RolTert",0) AS "RolTert",
       COALESCE(
         CASE WHEN rp."ClrType"  IN ('Partener','Angajat') THEN v."RepartitorId" END,
         CASE WHEN rpp."ClrType" IN ('Partener','Angajat') THEN v."RepartitorPereche" END) AS "Partener",
       CASE WHEN rp."ClrType" IN ('Gestiune','ContPropriu','UnitateInterna')
            THEN v."RepartitorId" END AS "Gestiune"
FROM "RegistruContabil" r
CROSS JOIN LATERAL (VALUES
  (1::smallint, r."ContDebitId",  r."DimensiuniDebit_RepartitorId",  r."DimensiuniCredit_RepartitorId",
   r."DimensiuniDebit_MaterialId",
   r."DimensiuniDebit_CodFunctionalId",  r."DimensiuniDebit_CodEconomicId",
   r."DimensiuniDebit_SursaFinantareId", r."DimensiuniDebit_UnitateId",
   r."DimensiuniDebit_ProiectId",  r."DimensiuniDebit_CentruCostId"),
  (2::smallint, r."ContCreditId", r."DimensiuniCredit_RepartitorId", r."DimensiuniDebit_RepartitorId",
   r."DimensiuniCredit_MaterialId",
   r."DimensiuniCredit_CodFunctionalId", r."DimensiuniCredit_CodEconomicId",
   r."DimensiuniCredit_SursaFinantareId", r."DimensiuniCredit_UnitateId",
   r."DimensiuniCredit_ProiectId", r."DimensiuniCredit_CentruCostId")
) AS v("Latura","Cont","RepartitorId","RepartitorPereche","MaterialId","CodFunctionalId","CodEconomicId",
       "SursaFinantareId","UnitateId","ProiectId","CentruCostId")
LEFT JOIN "Conturi" c ON c."ID" = v."Cont"
LEFT JOIN "Repartitori" rp  ON rp."ID"  = v."RepartitorId"
LEFT JOIN "Repartitori" rpp ON rpp."ID" = v."RepartitorPereche";
CREATE INDEX ON cub."_C"("DocumentId");
ANALYZE cub."_C";

INSERT INTO f1."Postare"(
  "ID","Spatiu","TranzactieId","DocumentId","LinieId","Data","Cont","Latura",
  "Partener","Gestiune","Produs","Unitate","CodTvaId","RolTva","PerioadaDeclarare",
  "Carte","Valuta","CodFunctional","CodEconomic","SursaFinantare",
  "UnitateOrganizatorica","Proiect","CentruCost","Atribuit",
  "Cantitate","ValoareValuta","Valoare")
SELECT uuidv7(), 1::smallint,
       COALESCE(t."TranzactieId", des."ID"),
       c."DocumentId", c."DetaliuId", c."Data", c."Cont", c."Latura",
       c."Partener", c."Gestiune", c."MaterialId",
       CASE WHEN c."RolTert" <> 0 THEN c."DocumentId" END,
       NULL::int, NULL::smallint, NULL::int,
       1::smallint, NULL::char(3),
       c."CodFunctionalId", c."CodEconomicId", c."SursaFinantareId", c."UnitateId",
       c."ProiectId", c."CentruCostId", NULL::uuid,
       0, 0, c."Valoare"
FROM cub."_C" c
LEFT JOIN cub."_TranzactiePeDocument" t ON t."DocumentId" = c."DocumentId"
CROSS JOIN (SELECT "ID" FROM cub."Tranzactie" WHERE "Fel"=3) des;

-- ---------- 4. Imperecherea = tranzactie datata (Fel=4) ----------
-- partida de referinta a unui document = postarea lui de tert cu |Valoare|
-- maxima (conteaza doar la cele cu mai multe postari de tert).
DROP TABLE IF EXISTS cub."_TertDoc";
CREATE UNLOGGED TABLE cub."_TertDoc" AS
SELECT DISTINCT ON (c."DocumentId")
       c."DocumentId", c."Cont", c."Latura", c."Partener", c."Valoare",
       count(*) OVER (PARTITION BY c."DocumentId") AS nr_postari_tert
FROM cub."_C" c
WHERE c."RolTert" <> 0 AND c."DocumentId" IS NOT NULL
ORDER BY c."DocumentId", abs(c."Valoare") DESC, c."RandId", c."Latura";
ALTER TABLE cub."_TertDoc" ADD PRIMARY KEY ("DocumentId");
ANALYZE cub."_TertDoc";

DROP TABLE IF EXISTS cub."_TranzactieImperechere";
CREATE UNLOGGED TABLE cub."_TranzactieImperechere" AS
SELECT i."ID" AS "ImperechereId", uuidv7() AS "TranzactieId",
       i."DocumentStingatorId", i."DocumentId", i."Suma", i."Data"
FROM "Imperecheri" i;
ALTER TABLE cub."_TranzactieImperechere" ADD PRIMARY KEY ("ImperechereId");
ANALYZE cub."_TranzactieImperechere";

INSERT INTO cub."Tranzactie"("ID","DocumentId","Fel","Data","ScrisLa")
SELECT m."TranzactieId", m."DocumentStingatorId", 4::smallint, m."Data", m."Data"::timestamptz
FROM cub."_TranzactieImperechere" m;

INSERT INTO f1."Postare"(
  "ID","Spatiu","TranzactieId","DocumentId","LinieId","Data","Cont","Latura",
  "Partener","Gestiune","Produs","Unitate","CodTvaId","RolTva","PerioadaDeclarare",
  "Carte","Valuta","CodFunctional","CodEconomic","SursaFinantare",
  "UnitateOrganizatorica","Proiect","CentruCost","Atribuit",
  "Cantitate","ValoareValuta","Valoare")
SELECT uuidv7(), 1::smallint, m."TranzactieId", m."DocumentStingatorId", NULL::uuid,
       m."Data", s."Cont", s."Latura",
       f."Partener", NULL::uuid, NULL::uuid, u."Unitate",
       NULL::int, NULL::smallint, NULL::int,
       1::smallint, NULL::char(3),
       NULL::uuid, NULL::uuid, NULL::uuid, NULL::uuid, NULL::uuid, NULL::uuid, NULL::uuid,
       0, 0, u."Valoare"
FROM cub."_TranzactieImperechere" m
JOIN cub."_TertDoc" s ON s."DocumentId" = m."DocumentStingatorId"
LEFT JOIN cub."_TertDoc" f ON f."DocumentId" = m."DocumentId"
CROSS JOIN LATERAL (VALUES
  (m."DocumentStingatorId", -m."Suma"),
  (m."DocumentId",           m."Suma")) AS u("Unitate","Valoare");

-- ---------- 5. Stoc ----------
INSERT INTO f1."Postare"(
  "ID","Spatiu","TranzactieId","DocumentId","LinieId","Data","Cont","Latura",
  "Partener","Gestiune","Produs","Unitate","CodTvaId","RolTva","PerioadaDeclarare",
  "Carte","Valuta","CodFunctional","CodEconomic","SursaFinantare",
  "UnitateOrganizatorica","Proiect","CentruCost","Atribuit",
  "Cantitate","ValoareValuta","Valoare")
SELECT uuidv7(), 2::smallint,
       COALESCE(t."TranzactieId", des."ID"),
       s."DocumentId", s."DetaliuId", s."Data", tm."ContImplicitId", NULL::smallint,
       NULL::uuid, s."RepartitorId", l."ProdusId", s."LotId",
       NULL::int, NULL::smallint, NULL::int,
       1::smallint, NULL::char(3),
       NULL::uuid, NULL::uuid, NULL::uuid, NULL::uuid, NULL::uuid, NULL::uuid, NULL::uuid,
       s."Cantitate", 0, s."Valoare"
FROM "RegistruStoc" s
JOIN "Loturi" l ON l."ID" = s."LotId"
JOIN "Produse" p ON p."ID" = l."ProdusId"
JOIN "TipuriMaterial" tm ON tm."ID" = p."TipMaterialId"
LEFT JOIN cub."_TranzactiePeDocument" t ON t."DocumentId" = s."DocumentId"
CROSS JOIN (SELECT "ID" FROM cub."Tranzactie" WHERE "Fel"=3) des;

-- ---------- 6. Fiscal ----------
INSERT INTO f1."Postare"(
  "ID","Spatiu","TranzactieId","DocumentId","LinieId","Data","Cont","Latura",
  "Partener","Gestiune","Produs","Unitate","CodTvaId","RolTva","PerioadaDeclarare",
  "Carte","Valuta","CodFunctional","CodEconomic","SursaFinantare",
  "UnitateOrganizatorica","Proiect","CentruCost","Atribuit",
  "Cantitate","ValoareValuta","Valoare")
SELECT uuidv7(), 3::smallint, t."TranzactieId",
       v."DocumentId", v."DetaliuId", v."Data", NULL::uuid, NULL::smallint,
       v."PartenerId", NULL::uuid, NULL::uuid, NULL::uuid,
       ct."ID", r."RolTva", v."PerioadaAn"*100 + v."PerioadaLuna",
       1::smallint, NULL::char(3),
       NULL::uuid, NULL::uuid, NULL::uuid, NULL::uuid, NULL::uuid, NULL::uuid, NULL::uuid,
       0, 0, r."Valoare"
FROM "RegistruTva" v
JOIN cub."_TranzactiePeDocument" t ON t."DocumentId" = v."DocumentId"
JOIN cub."CodTva" ct ON ct."TipTvaId" = v."TipTvaId" AND ct."Regim" = v."Regim"
                    AND ct."Cota" = v."Cota" AND ct."Sens" = v."Sens"
CROSS JOIN LATERAL (VALUES (1::smallint, v."Baza"), (2::smallint, v."Tva"))
  AS r("RolTva","Valoare");

ANALYZE f1."Postare";
ANALYZE cub."Tranzactie";
SELECT "Fel", count(*) FROM cub."Tranzactie" GROUP BY 1 ORDER BY 1;
SELECT "Spatiu", count(*) FROM f1."Postare" GROUP BY 1 ORDER BY 1;
