-- Pas 1, x10: pregatirea. Se ruleaza IN baza x10 (clona x1).
-- Salveaza definitiile indexilor ne-constrangere si ii sterge (se recreeaza
-- identic la 06c); incarcarea cu 18 indexi pe f1 ar dura ore.
\set ON_ERROR_STOP on
\timing on

DROP TABLE IF EXISTS cub."_OrigTranzactii";
CREATE TABLE cub."_OrigTranzactii" AS SELECT "ID" FROM cub."Tranzactie" WHERE "Fel" IN (1,4);
ALTER TABLE cub."_OrigTranzactii" ADD PRIMARY KEY ("ID");
ANALYZE cub."_OrigTranzactii";

DROP TABLE IF EXISTS cub."_IndexDdl";
CREATE TABLE cub."_IndexDdl"(ord int, nume text, def text);
INSERT INTO cub."_IndexDdl"(ord, nume, def)
SELECT row_number() OVER (ORDER BY n.nspname, c.relname),
       quote_ident(n.nspname)||'.'||quote_ident(c.relname),
       pg_get_indexdef(i.indexrelid)
FROM pg_index i
JOIN pg_class c ON c.oid = i.indexrelid
JOIN pg_namespace n ON n.oid = c.relnamespace
JOIN pg_class t ON t.oid = i.indrelid
JOIN pg_namespace tn ON tn.oid = t.relnamespace
WHERE NOT t.relispartition
  AND NOT EXISTS (SELECT 1 FROM pg_constraint k WHERE k.conindid = i.indexrelid)
  AND ( (tn.nspname IN ('f1','f2','f3') AND t.relname = 'Postare')
     OR (tn.nspname = 'cub' AND t.relname = 'Tranzactie')
     OR (tn.nspname = 'public' AND t.relname IN ('RegistruContabil','RegistruStoc','RegistruTva')) );
SELECT count(*) AS indexi_salvati FROM cub."_IndexDdl";

DO $$
DECLARE r record;
BEGIN
  FOR r IN SELECT nume FROM cub."_IndexDdl" ORDER BY ord LOOP
    EXECUTE 'DROP INDEX ' || r.nume;
  END LOOP;
END $$;
