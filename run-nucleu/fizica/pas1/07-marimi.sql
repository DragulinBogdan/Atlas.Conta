-- Pas 1: marimile. Tabela vs indexi, per forma si per partitie; latimea medie a randului.
\set ON_ERROR_STOP on
\pset footer off

SELECT 'rezumat' sectiune, n.nspname||'.'||c.relname obiect,
       c.reltuples::bigint randuri,
       pg_size_pretty(pg_table_size(c.oid)) tabela,
       pg_size_pretty(pg_indexes_size(c.oid)) indexi,
       pg_size_pretty(pg_total_relation_size(c.oid)) total
FROM pg_class c JOIN pg_namespace n ON n.oid=c.relnamespace
WHERE (n.nspname='f1' AND c.relname='Postare')
   OR (n.nspname IN ('f2','f3') AND c.relname='Postare')
   OR (n.nspname='public' AND c.relname IN ('RegistruContabil','RegistruStoc','RegistruTva',
        'SolduriPerioadaContabil','SolduriPerioadaStoc','PartideDeschise'))
   OR (n.nspname='cub' AND c.relname IN ('Tranzactie','CodTva'))
ORDER BY 2;

-- f2/f3 sunt partitionate: pg_total_relation_size pe parinte NU include partitiile
SELECT 'agregat_partitionat' sectiune, p.parent obiect,
       sum(c.reltuples)::bigint randuri,
       pg_size_pretty(sum(pg_table_size(c.oid))) tabela,
       pg_size_pretty(sum(pg_indexes_size(c.oid))) indexi,
       pg_size_pretty(sum(pg_total_relation_size(c.oid))) total
FROM (SELECT n.nspname||'.Postare' parent, i.inhrelid oid
      FROM pg_inherits i JOIN pg_class pc ON pc.oid=i.inhparent
      JOIN pg_namespace n ON n.oid=pc.relnamespace
      WHERE pc.relname='Postare' AND n.nspname IN ('f2','f3')) p
JOIN pg_class c ON c.oid=p.oid
WHERE c.relkind='r'
GROUP BY 1,2
UNION ALL
SELECT 'agregat_partitionat', 'f3.Postare (frunze)', sum(c.reltuples)::bigint,
       pg_size_pretty(sum(pg_table_size(c.oid))), pg_size_pretty(sum(pg_indexes_size(c.oid))),
       pg_size_pretty(sum(pg_total_relation_size(c.oid)))
FROM pg_class c JOIN pg_namespace n ON n.oid=c.relnamespace
WHERE n.nspname='f3' AND c.relkind='r'
GROUP BY 1,2
UNION ALL
SELECT 'agregat_partitionat', 'F0 (3 registre)', sum(c.reltuples)::bigint,
       pg_size_pretty(sum(pg_table_size(c.oid))), pg_size_pretty(sum(pg_indexes_size(c.oid))),
       pg_size_pretty(sum(pg_total_relation_size(c.oid)))
FROM pg_class c JOIN pg_namespace n ON n.oid=c.relnamespace
WHERE n.nspname='public' AND c.relname IN ('RegistruContabil','RegistruStoc','RegistruTva');

SELECT 'partitii' sectiune, n.nspname||'.'||c.relname obiect, c.reltuples::bigint randuri,
       pg_size_pretty(pg_table_size(c.oid)) tabela,
       pg_size_pretty(pg_indexes_size(c.oid)) indexi
FROM pg_class c JOIN pg_namespace n ON n.oid=c.relnamespace
WHERE n.nspname IN ('f2','f3') AND c.relkind='r' AND c.relname <> 'Postare'
ORDER BY 2;

SELECT 'latime_rand' sectiune, n.nspname||'.'||c.relname obiect,
       c.reltuples::bigint randuri,
       round(pg_relation_size(c.oid)::numeric / NULLIF(c.reltuples::numeric,0), 1) octeti_pe_rand
FROM pg_class c JOIN pg_namespace n ON n.oid=c.relnamespace
WHERE (n.nspname='f1' AND c.relname='Postare')
   OR (n.nspname='public' AND c.relname IN ('RegistruContabil','RegistruStoc','RegistruTva'))
   OR (n.nspname IN ('f2','f3') AND c.relkind='r' AND c.relname <> 'Postare')
ORDER BY 2;

SELECT 'indexi_f1' sectiune, indexname obiect, pg_size_pretty(pg_relation_size((quote_ident(schemaname)||'.'||quote_ident(indexname))::regclass)) marime
FROM pg_indexes WHERE schemaname='f1' ORDER BY pg_relation_size((quote_ident(schemaname)||'.'||quote_ident(indexname))::regclass) DESC;

SELECT 'indexi_F0' sectiune, tablename||' / '||indexname obiect,
       pg_size_pretty(pg_relation_size((quote_ident(schemaname)||'.'||quote_ident(indexname))::regclass)) marime
FROM pg_indexes WHERE schemaname='public'
  AND tablename IN ('RegistruContabil','RegistruStoc','RegistruTva')
ORDER BY 2;
