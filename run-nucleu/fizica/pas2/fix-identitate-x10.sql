-- Pas 2: REPARAREA bazei x10, nu tuning. La pasul 1, 06a a sters indexii ne-constrangere
-- si 06c i-a recreat din `pg_get_indexdef`, care pentru un index pe tabela PARTITIONATA
-- intoarce `CREATE INDEX ... ON ONLY ...` => s-a recreat doar invelisul parintelui, FARA
-- indexii de partitie. Rezultat: la x10, f2 si f3 au ramas fara indexii de identitate
-- (DocumentId, LinieId, TranzactieId) pe care x1 ii are. Cele doua scari nu erau
-- masurate pe acelasi set. Se repara si se re-ruleaza formele afectate; ambele cifre
-- se raporteaza.
\set ON_ERROR_STOP on
\timing on
DROP INDEX IF EXISTS f2.ix_f2_document, f2.ix_f2_linie, f2.ix_f2_tranzactie;
DROP INDEX IF EXISTS f3.ix_f3_document, f3.ix_f3_linie, f3.ix_f3_tranzactie;
CREATE INDEX ix_f2_tranzactie ON f2."Postare"("TranzactieId");
CREATE INDEX ix_f2_document   ON f2."Postare"("DocumentId");
CREATE INDEX ix_f2_linie      ON f2."Postare"("LinieId");
CREATE INDEX ix_f3_tranzactie ON f3."Postare"("TranzactieId");
CREATE INDEX ix_f3_document   ON f3."Postare"("DocumentId");
CREATE INDEX ix_f3_linie      ON f3."Postare"("LinieId");
VACUUM (ANALYZE) f2."Postare";
VACUUM (ANALYZE) f3."Postare";
SELECT t.relname, count(*) AS indexi FROM pg_index i
JOIN pg_class c ON c.oid=i.indexrelid JOIN pg_class t ON t.oid=i.indrelid
JOIN pg_namespace n ON n.oid=t.relnamespace
WHERE n.nspname='f2' AND t.relname LIKE 'Postare\_%' GROUP BY 1 ORDER BY 1;
