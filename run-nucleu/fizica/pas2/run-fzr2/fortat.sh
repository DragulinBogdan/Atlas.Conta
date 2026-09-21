#!/bin/bash
# FZ-r2 faza 3 (x10): (Cont,Data) recreat temporar; planul FORTAT pe el prin DROP INDEX ix_f2c_data intr-o tranzactie cu ROLLBACK (starea nu se schimba).
DB=Atlas.Conta.Nucleu.Fizica.x10; OUT=/tmp/fzr2out/x10-fortat; rm -rf $OUT; mkdir -p $OUT/f2 $OUT/f2s
psql -U postgres -d $DB -X -q -c 'CREATE INDEX ix_f2c_cont_data ON f2."Postare_Contabil"("Cont","Data") INCLUDE ("Latura","Valoare","Partener","Unitate");' -c 'ANALYZE f2."Postare";'
for FQ in f2/q03 f2/q03c f2s/q03s f2s/q03cs; do
  for R in 1 2 3 4 5 6; do
    { echo 'BEGIN;'; echo 'DROP INDEX f2.ix_f2c_data;'; cat /tmp/fzr2/sql/$FQ.sql; echo 'ROLLBACK;'; } | psql -U postgres -d $DB -X -q > $OUT/$FQ.r$R.txt 2>&1
  done
  echo "gata $FQ"
done
psql -U postgres -d $DB -X -q -c 'DROP INDEX f2.ix_f2c_cont_data;' -c 'ANALYZE f2."Postare";'
psql -U postgres -d $DB -X -Atc "select string_agg(indexname, ',' order by indexname) from pg_indexes where schemaname='f2' and tablename='Postare_Contabil'"
echo "=== FORTAT DONE"
