#!/bin/bash
# Pas 2: Q14 (scrierea). $1 = baza, $2 = scara. 6 rulari, prima aruncata.
DB="$1"; SC="$2"; OUT=/tmp/pas2out/q14-$SC
rm -rf $OUT; mkdir -p $OUT
echo "=== Q14 START $SC $(date -Is) ==="
for F in f0 f1 f2 f3; do
  for R in 1 2 3 4 5 6; do
    psql -U postgres -d "$DB" -X -q -f /tmp/pas2/q14/$F.sql > $OUT/$F.r$R.txt 2>&1
  done
  echo "gata $F"
done
if [ "$SC" = "x1" ]; then
  psql -U postgres -d "$DB" -X -q -f /tmp/pas2/fk-f2.sql > $OUT/fk-create.log 2>&1
  for R in 1 2 3 4 5 6; do
    psql -U postgres -d "$DB" -X -q -f /tmp/pas2/q14/f2.sql > $OUT/f2fk.r$R.txt 2>&1
  done
  echo "gata f2fk"
  psql -U postgres -d "$DB" -X -q -f /tmp/pas2/fk-f2-drop.sql > $OUT/fk-drop.log 2>&1
fi
echo "=== Q14 STOP $SC $(date -Is) ==="
touch /tmp/pas2out/q14-$SC.DONE
