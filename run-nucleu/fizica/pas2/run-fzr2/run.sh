#!/bin/bash
# FZ-r2: seria de masurare a fisei de la Sold. $1 = baza, $2 = eticheta (x1/x10/x1-idx/x10-idx), $3 = formele.
DB="$1"; SC="$2"; FORME="${3:-f0 f2 f3 f2s f3s}"
OUT=/tmp/fzr2out/$SC; rm -rf $OUT; mkdir -p $OUT
echo "=== START $SC $(date -Is) ==="
for FORMA in $FORME; do
  mkdir -p $OUT/$FORMA
  while read -r Q; do
    [ -z "$Q" ] && continue
    SRC=/tmp/fzr2/sql/$FORMA/$Q.sql; [ -f "$SRC" ] || continue
    for R in 1 2 3 4 5 6; do
      psql -U postgres -d "$DB" -X -q -f "$SRC" > $OUT/$FORMA/$Q.r$R.txt 2>&1
    done
    echo "gata $FORMA/$Q $(date -Is)"
  done < /tmp/fzr2/lista-$FORMA.txt
done
echo "=== STOP $SC $(date -Is) ==="
