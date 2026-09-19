-- Pas 1 (fizica): schemele cubului. Idempotent.
-- Fara FK-uri fizice: masuram cubul, nu integritatea.
DROP SCHEMA IF EXISTS f1 CASCADE;
DROP SCHEMA IF EXISTS f2 CASCADE;
DROP SCHEMA IF EXISTS f3 CASCADE;
DROP SCHEMA IF EXISTS cub CASCADE;
CREATE SCHEMA cub;
CREATE SCHEMA f1;
CREATE SCHEMA f2;
CREATE SCHEMA f3;

CREATE TABLE cub."Tranzactie"(
  "ID" uuid PRIMARY KEY,
  "DocumentId" uuid NULL,
  "Fel" smallint NOT NULL,            -- 1 Operare, 2 Storno, 3 Deschidere
  "Data" date NOT NULL,
  "ScrisLa" timestamptz NULL);
CREATE INDEX ix_tranzactie_document ON cub."Tranzactie"("DocumentId");

CREATE TABLE cub."CodTva"(
  "ID" int PRIMARY KEY,
  "TipTvaId" uuid NOT NULL,
  "Regim" smallint NOT NULL,
  "Cota" numeric(5,2) NOT NULL,
  "Sens" smallint NOT NULL);

-- forma canonica a coloanelor; f1/f2/f3 o repeta identic (ordinea conteaza:
-- umplerea f2/f3 se face prin INSERT ... SELECT * FROM f1)
CREATE TABLE f1."Postare"(
  "ID" uuid NOT NULL,
  "Spatiu" smallint NOT NULL,          -- 1 Contabil, 2 Stoc, 3 Fiscal
  "TranzactieId" uuid NOT NULL,
  "DocumentId" uuid NULL,
  "LinieId" uuid NULL,
  "Data" date NOT NULL,
  "Cont" uuid NULL,
  "Latura" smallint NULL,              -- 1 D, 2 C
  "Partener" uuid NULL,
  "Gestiune" uuid NULL,
  "Produs" uuid NULL,
  "Unitate" uuid NULL,
  "CodTvaId" int NULL,
  "RolTva" smallint NULL,              -- 1 Baza, 2 Taxa
  "PerioadaDeclarare" int NULL,        -- AAAALL
  "Carte" smallint NOT NULL DEFAULT 1,
  "Valuta" char(3) NULL,
  "CodFunctional" uuid NULL,
  "CodEconomic" uuid NULL,
  "SursaFinantare" uuid NULL,
  "UnitateOrganizatorica" uuid NULL,
  "Proiect" uuid NULL,
  "CentruCost" uuid NULL,
  "Atribuit" uuid NULL,
  "Cantitate" numeric(18,3) NOT NULL DEFAULT 0,
  "ValoareValuta" numeric(18,2) NOT NULL DEFAULT 0,
  "Valoare" numeric(18,2) NOT NULL DEFAULT 0);

-- f2: LIST pe Spatiu, PK (Spatiu, ID)
CREATE TABLE f2."Postare"(LIKE f1."Postare" INCLUDING DEFAULTS) PARTITION BY LIST ("Spatiu");
CREATE TABLE f2."Postare_Contabil" PARTITION OF f2."Postare" FOR VALUES IN (1);
CREATE TABLE f2."Postare_Stoc"     PARTITION OF f2."Postare" FOR VALUES IN (2);
CREATE TABLE f2."Postare_Fiscal"   PARTITION OF f2."Postare" FOR VALUES IN (3);

-- f3: LIST pe Spatiu x RANGE pe Data (an). Postgres cere cheia FIECARUI nivel
-- intr-un index unic => PK (Spatiu, Data, ID), nu (Spatiu, ID) ca la f2.
CREATE TABLE f3."Postare"(LIKE f1."Postare" INCLUDING DEFAULTS) PARTITION BY LIST ("Spatiu");
CREATE TABLE f3."Postare_Contabil" PARTITION OF f3."Postare" FOR VALUES IN (1) PARTITION BY RANGE ("Data");
CREATE TABLE f3."Postare_Stoc"     PARTITION OF f3."Postare" FOR VALUES IN (2) PARTITION BY RANGE ("Data");
CREATE TABLE f3."Postare_Fiscal"   PARTITION OF f3."Postare" FOR VALUES IN (3) PARTITION BY RANGE ("Data");
DO $$
DECLARE s text; a int;
BEGIN
  FOREACH s IN ARRAY ARRAY['Contabil','Stoc','Fiscal'] LOOP
    FOR a IN 2016..2026 LOOP
      EXECUTE format('CREATE TABLE f3.%I PARTITION OF f3.%I FOR VALUES FROM (%L) TO (%L)',
        'Postare_'||s||'_'||a, 'Postare_'||s, a||'-01-01', (a+1)||'-01-01');
    END LOOP;
    EXECUTE format('CREATE TABLE f3.%I PARTITION OF f3.%I DEFAULT', 'Postare_'||s||'_rest', 'Postare_'||s);
  END LOOP;
END $$;
