\set ON_ERROR_STOP on
\timing on
ALTER TABLE f2."Postare" ADD CONSTRAINT pk_f2_postare PRIMARY KEY ("Spatiu","ID");
CREATE INDEX ix_f2_tranzactie ON f2."Postare"("TranzactieId");
CREATE INDEX ix_f2_document   ON f2."Postare"("DocumentId");
CREATE INDEX ix_f2_linie      ON f2."Postare"("LinieId");

-- f3: Postgres cere cheia fiecarui nivel de partitionare intr-un index unic
-- => PK (Spatiu, Data, ID), nu (Spatiu, ID). Abatere fortata, declarata.
ALTER TABLE f3."Postare" ADD CONSTRAINT pk_f3_postare PRIMARY KEY ("Spatiu","Data","ID");
CREATE INDEX ix_f3_tranzactie ON f3."Postare"("TranzactieId");
CREATE INDEX ix_f3_document   ON f3."Postare"("DocumentId");
CREATE INDEX ix_f3_linie      ON f3."Postare"("LinieId");
