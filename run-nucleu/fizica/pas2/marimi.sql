\pset format unaligned
\pset fieldsep '|'
SELECT 'forma', f, pg_size_pretty(sum(tab)) AS tabela, pg_size_pretty(sum(idx)) AS indexi,
       pg_size_pretty(sum(tab+idx)) AS total, sum(randuri)::text AS randuri
FROM (
  SELECT CASE WHEN n.nspname='f1' THEN 'F1'
              WHEN n.nspname='f2' THEN 'F2'
              WHEN n.nspname='f3' THEN 'F3'
              WHEN c.relname='Sold' THEN 'Sold'
              WHEN c.relname='Tranzactie' THEN 'Tranzactie+CodTva'
              WHEN c.relname='CodTva' THEN 'Tranzactie+CodTva'
              ELSE 'F0' END AS f,
         pg_relation_size(c.oid) AS tab, pg_indexes_size(c.oid) AS idx, c.reltuples::bigint AS randuri
  FROM pg_class c JOIN pg_namespace n ON n.oid=c.relnamespace
  WHERE c.relkind='r' AND (
     (n.nspname IN ('f1','f2','f3'))
     OR (n.nspname='cub' AND c.relname IN ('Sold','Tranzactie','CodTva'))
     OR (n.nspname='public' AND c.relname IN ('RegistruContabil','RegistruStoc','RegistruTva',
         'SolduriPerioadaContabil','SolduriPerioadaStoc','PartideDeschise')))) x
GROUP BY f ORDER BY f;
SELECT 'partitie', n.nspname||'.'||c.relname, pg_size_pretty(pg_relation_size(c.oid)),
       pg_size_pretty(pg_indexes_size(c.oid)), c.reltuples::bigint::text, ''
FROM pg_class c JOIN pg_namespace n ON n.oid=c.relnamespace
WHERE c.relkind='r' AND n.nspname='f2' AND c.relname LIKE 'Postare\_%' ORDER BY 2;
SELECT 'F0-detaliu', c.relname, pg_size_pretty(pg_relation_size(c.oid)),
       pg_size_pretty(pg_indexes_size(c.oid)), c.reltuples::bigint::text, ''
FROM pg_class c JOIN pg_namespace n ON n.oid=c.relnamespace
WHERE c.relkind='r' AND n.nspname='public'
  AND c.relname IN ('RegistruContabil','RegistruStoc','RegistruTva','SolduriPerioadaContabil','SolduriPerioadaStoc','PartideDeschise')
ORDER BY 2;
