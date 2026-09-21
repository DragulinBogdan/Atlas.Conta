-- Pas 2, SINGURA iteratie permisa si declarata: C1 (Cont,Data INCLUDE ...) si
-- S1 (Gestiune,Unitate,Data INCLUDE ...) nu sunt atinse de NICIO interogare a lor
-- la x10 (nici pe f2, nici pe f3). Se scot si se re-masoara; ambele cifre se raporteaza.
\set ON_ERROR_STOP on
\timing on
SELECT 'inainte', pg_size_pretty(sum(pg_relation_size(c.oid))) FROM pg_class c
JOIN pg_namespace n ON n.oid=c.relnamespace
WHERE c.relkind IN ('i','I') AND n.nspname IN ('f2','f3')
  AND (c.relname LIKE '%Cont_Data_Latura%' OR c.relname LIKE '%Gestiune_Unitate_Data%'
       OR c.relname IN ('ix_f2c_cont_data','ix_f3c_cont_data','ix_f2s_gest_unit_data','ix_f3s_gest_unit_data'));
DROP INDEX f2.ix_f2c_cont_data, f2.ix_f2s_gest_unit_data;
DROP INDEX f3.ix_f3c_cont_data, f3.ix_f3s_gest_unit_data;
ANALYZE f2."Postare";
ANALYZE f3."Postare";
