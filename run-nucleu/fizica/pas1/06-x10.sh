#!/bin/sh
# Pas 1, x10: clona x1 -> x10, apoi 9 treceri de replicare (k = 1..9).
# Se ruleaza din run-nucleu/fizica/pas1/. Idempotent: recreeaza baza x10.
set -e
export MSYS_NO_PATHCONV=1
C=contapal-postgres-1
X1="Atlas.Conta.Nucleu.Fizica.x1"
X10="Atlas.Conta.Nucleu.Fizica.x10"
OUT=out
mkdir -p "$OUT"

for f in 06a-x10-pregatire.sql 06b-x10-replica.sql 06c-x10-incheiere.sql; do
  docker cp "$f" "$C:/tmp/"
done

echo "== clona x10 <- x1 $(date -Is)"
docker exec "$C" psql -U postgres -Atc "DROP DATABASE IF EXISTS \"$X10\""
docker exec "$C" psql -U postgres -Atc "CREATE DATABASE \"$X10\" TEMPLATE \"$X1\""

echo "== pregatire (drop indexi) $(date -Is)"
docker exec "$C" psql -U postgres -d "$X10" -v ON_ERROR_STOP=1 -f /tmp/06a-x10-pregatire.sql > "$OUT/06a-pregatire.txt" 2>&1

for k in 1 2 3 4 5 6 7 8 9; do
  echo "== k=$k $(date -Is)"
  docker exec "$C" psql -U postgres -d "$X10" -v ON_ERROR_STOP=1 -v k=$k -f /tmp/06b-x10-replica.sql > "$OUT/06b-k$k.txt" 2>&1
done

echo "== incheiere (recreare indexi + ANALYZE) $(date -Is)"
docker exec "$C" psql -U postgres -d "$X10" -v ON_ERROR_STOP=1 -f /tmp/06c-x10-incheiere.sql > "$OUT/06c-incheiere.txt" 2>&1
echo "== gata $(date -Is)"
