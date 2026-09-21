#!/bin/bash
# FZ-r2 faza 2: indexul (Cont,Data) recreat TEMPORAR pe f2 (scos in iteratia declarata a pasului 2), re-masurare f2/f2s, apoi scos la loc.
for SC in x1 x10; do
  DB=Atlas.Conta.Nucleu.Fizica.$SC
  echo "=== $SC create $(date -Is)"
  psql -U postgres -d $DB -X -q -c '\timing on' -c 'CREATE INDEX ix_f2c_cont_data ON f2."Postare_Contabil"("Cont","Data") INCLUDE ("Latura","Valoare","Partener","Unitate");' -c 'ANALYZE f2."Postare";' -c "SELECT pg_size_pretty(pg_relation_size('f2.ix_f2c_cont_data')) AS ix_cont_data;"
  bash /tmp/fzr2/run.sh $DB $SC-idx "f2 f2s"
  echo "=== $SC drop $(date -Is)"
  psql -U postgres -d $DB -X -q -c 'DROP INDEX f2.ix_f2c_cont_data;' -c 'ANALYZE f2."Postare";'
done
echo "=== IDX DONE $(date -Is)"
