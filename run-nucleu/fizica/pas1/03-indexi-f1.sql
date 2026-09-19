-- Pas 1: indexii celor trei forme, creati DUPA incarcare.
-- f1 = portarea naiva "EF-style": un btree pe fiecare coloana-cheie.
-- f2/f3 = doar indexii de identitate; indexii per uz ii proiecteaza pasul 2.
\set ON_ERROR_STOP on
\timing on

ALTER TABLE f1."Postare" ADD CONSTRAINT pk_f1_postare PRIMARY KEY ("ID");
CREATE INDEX ix_f1_tranzactie    ON f1."Postare"("TranzactieId");
CREATE INDEX ix_f1_document      ON f1."Postare"("DocumentId");
CREATE INDEX ix_f1_linie         ON f1."Postare"("LinieId");
CREATE INDEX ix_f1_data          ON f1."Postare"("Data");
CREATE INDEX ix_f1_cont          ON f1."Postare"("Cont");
CREATE INDEX ix_f1_partener      ON f1."Postare"("Partener");
CREATE INDEX ix_f1_gestiune      ON f1."Postare"("Gestiune");
CREATE INDEX ix_f1_produs        ON f1."Postare"("Produs");
CREATE INDEX ix_f1_unitate       ON f1."Postare"("Unitate");
CREATE INDEX ix_f1_codtva        ON f1."Postare"("CodTvaId");
CREATE INDEX ix_f1_perioada      ON f1."Postare"("PerioadaDeclarare");
CREATE INDEX ix_f1_codfunctional ON f1."Postare"("CodFunctional");
CREATE INDEX ix_f1_codeconomic   ON f1."Postare"("CodEconomic");
CREATE INDEX ix_f1_sursafin      ON f1."Postare"("SursaFinantare");
CREATE INDEX ix_f1_unitateorg    ON f1."Postare"("UnitateOrganizatorica");
CREATE INDEX ix_f1_proiect       ON f1."Postare"("Proiect");
CREATE INDEX ix_f1_centrucost    ON f1."Postare"("CentruCost");
