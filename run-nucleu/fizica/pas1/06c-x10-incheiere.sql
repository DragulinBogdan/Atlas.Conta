-- Pas 1, x10: recrearea indexilor salvati de 06a, identic, apoi ANALYZE.
\set ON_ERROR_STOP on
\timing on
DO $$
DECLARE r record;
BEGIN
  FOR r IN SELECT def FROM cub."_IndexDdl" ORDER BY ord LOOP
    EXECUTE r.def;
  END LOOP;
END $$;
ANALYZE;
SELECT 'x10_postari' k, "Spatiu"::text s, count(*)::text v FROM f1."Postare" GROUP BY 2
UNION ALL SELECT 'x10_f2', "Spatiu"::text, count(*)::text FROM f2."Postare" GROUP BY 2
UNION ALL SELECT 'x10_f3', "Spatiu"::text, count(*)::text FROM f3."Postare" GROUP BY 2
UNION ALL SELECT 'x10_tranzactii', "Fel"::text, count(*)::text FROM cub."Tranzactie" GROUP BY 2
UNION ALL SELECT 'x10_F0', 'RegistruContabil', count(*)::text FROM "RegistruContabil"
UNION ALL SELECT 'x10_F0', 'RegistruStoc', count(*)::text FROM "RegistruStoc"
UNION ALL SELECT 'x10_F0', 'RegistruTva', count(*)::text FROM "RegistruTva"
ORDER BY 1,2;
