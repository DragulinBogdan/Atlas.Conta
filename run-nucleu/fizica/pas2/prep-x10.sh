#!/bin/bash
set -e
DB="Atlas.Conta.Nucleu.Fizica.x10"
echo "=== START $(date -Is) ==="
psql -U postgres -d "$DB" -f /tmp/indexi-f2.sql
echo "=== INDEXI GATA $(date -Is) ==="
psql -U postgres -d "$DB" -f /tmp/vac.sql
echo "=== VACUUM GATA $(date -Is) ==="
psql -U postgres -d "$DB" -v y1=2016 -f /tmp/snapshot.sql
echo "=== SNAPSHOT GATA $(date -Is) ==="
touch /tmp/prep-x10.DONE
