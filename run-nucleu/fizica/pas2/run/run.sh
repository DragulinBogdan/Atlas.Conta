#!/bin/bash
# Pas 2: seria de masurare. $1 = baza, $2 = scara (x1/x10).
DB="$1"; SC="$2"
OUT=/tmp/pas2out/$SC
rm -rf $OUT; mkdir -p $OUT
echo "=== START $SC $(date -Is) ==="
for FORMA in f0 f1 f2 f3 f2s f3s; do
  mkdir -p $OUT/$FORMA
  while read -r Q; do
    [ -z "$Q" ] && continue
    SRC=/tmp/pas2/sql/$FORMA/$Q.sql
    [ -f "$SRC" ] || continue
    T0=$(date +%s)
    for R in 1 2 3 4 5 6; do
      psql -U postgres -d "$DB" -X -q -f "$SRC" > $OUT/$FORMA/$Q.r$R.txt 2>&1
      if [ $R -eq 1 ]; then
        T1=$(date +%s)
        if [ $((T1-T0)) -gt 60 ]; then echo "LENT $FORMA/$Q $((T1-T0))s: o singura rulare"; break; fi
      fi
    done
    echo "gata $FORMA/$Q $(date -Is)"
  done < /tmp/pas2/lista-$FORMA.txt
done
echo "=== STOP $SC $(date -Is) ==="
touch /tmp/pas2out/$SC.DONE
