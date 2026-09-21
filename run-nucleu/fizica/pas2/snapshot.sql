-- Pas 2 C: snapshot-ul cub."Sold" = SUM pe TOATE coordonatele, per spatiu,
-- la granite de 31 decembrie. Se umple din f2. Parametru: :y1 = primul an.
\set ON_ERROR_STOP on
\timing on

DROP TABLE IF EXISTS cub."Sold";
CREATE TABLE cub."Sold"(
  "Spatiu" smallint NOT NULL,
  "Granita" date NOT NULL,
  "Cont" uuid NULL, "Latura" smallint NULL,
  "Partener" uuid NULL, "Gestiune" uuid NULL, "Produs" uuid NULL, "Unitate" uuid NULL,
  "CodTvaId" int NULL, "RolTva" smallint NULL,
  "Carte" smallint NOT NULL,
  "CodFunctional" uuid NULL, "CodEconomic" uuid NULL, "SursaFinantare" uuid NULL,
  "UnitateOrganizatorica" uuid NULL, "Proiect" uuid NULL, "CentruCost" uuid NULL,
  "Cantitate" numeric(18,3) NOT NULL, "Valoare" numeric(18,2) NOT NULL);

SET myapp.y1 = :'y1';
DO $$
DECLARE a int; g date; y1 int := current_setting('myapp.y1')::int;
BEGIN
  FOR a IN y1..2025 LOOP
    g := make_date(a,12,31);
    INSERT INTO cub."Sold"
    SELECT p."Spatiu", g, p."Cont", p."Latura", p."Partener", p."Gestiune", p."Produs",
           p."Unitate", p."CodTvaId", p."RolTva", p."Carte", p."CodFunctional",
           p."CodEconomic", p."SursaFinantare", p."UnitateOrganizatorica", p."Proiect",
           p."CentruCost", sum(p."Cantitate"), sum(p."Valoare")
    FROM f2."Postare" p
    WHERE p."Data" <= g
    GROUP BY p."Spatiu", p."Cont", p."Latura", p."Partener", p."Gestiune", p."Produs",
             p."Unitate", p."CodTvaId", p."RolTva", p."Carte", p."CodFunctional",
             p."CodEconomic", p."SursaFinantare", p."UnitateOrganizatorica", p."Proiect",
             p."CentruCost";
    RAISE NOTICE 'granita % gata', g;
  END LOOP;
END $$;

CREATE INDEX ix_sold_cont ON cub."Sold"("Spatiu","Granita","Cont","Latura")
  INCLUDE ("Partener","Unitate","Valoare");
CREATE INDEX ix_sold_unitate ON cub."Sold"("Spatiu","Granita","Unitate")
  INCLUDE ("Cont","Latura","Valoare") WHERE "Unitate" IS NOT NULL;
VACUUM (ANALYZE) cub."Sold";

SELECT "Granita", "Spatiu", count(*) AS randuri FROM cub."Sold" GROUP BY 1,2 ORDER BY 1,2;
SELECT pg_size_pretty(pg_total_relation_size('cub."Sold"')) AS total,
       pg_size_pretty(pg_relation_size('cub."Sold"')) AS tabela,
       pg_size_pretty(pg_indexes_size('cub."Sold"')) AS indexi;

-- Proba de egalitate ceruta de spec: Sold la 2025-12-31 == suma directa din f2.
SELECT 'sold_abateri_20251231' AS proba, count(*) AS valoare FROM (
  SELECT "Spatiu","Cont","Latura","Partener","Gestiune","Produs","Unitate","CodTvaId",
         "RolTva","Carte","CodFunctional","CodEconomic","SursaFinantare",
         "UnitateOrganizatorica","Proiect","CentruCost","Cantitate","Valoare"
  FROM cub."Sold" WHERE "Granita" = DATE '2025-12-31'
  EXCEPT ALL
  SELECT "Spatiu","Cont","Latura","Partener","Gestiune","Produs","Unitate","CodTvaId",
         "RolTva","Carte","CodFunctional","CodEconomic","SursaFinantare",
         "UnitateOrganizatorica","Proiect","CentruCost",sum("Cantitate"),sum("Valoare")
  FROM f2."Postare" WHERE "Data" <= DATE '2025-12-31'
  GROUP BY 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16) d;
