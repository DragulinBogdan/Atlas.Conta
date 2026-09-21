-- B0: ancore (profil, volume). Idempotent. Creeaza schema tr3 (singura scrisa).
CREATE SCHEMA IF NOT EXISTS tr3;
SELECT 'profil' k, "Profil"::text v FROM "SetariProfil"
UNION ALL SELECT 'RegistruStoc total', count(*)::text FROM "RegistruStoc"
UNION ALL SELECT 'RegistruStoc GCRecord not null', count(*)::text FROM "RegistruStoc" WHERE "GCRecord" IS NOT NULL
UNION ALL SELECT 'RegistruContabil total', count(*)::text FROM "RegistruContabil"
UNION ALL SELECT 'RegistruContabil GC', count(*)::text FROM "RegistruContabil" WHERE "GCRecord" IS NOT NULL
UNION ALL SELECT 'Documente', count(*)::text FROM "Documente"
UNION ALL SELECT 'DocumentDetalii', count(*)::text FROM "DocumentDetalii"
UNION ALL SELECT 'f2.Postare', count(*)::text FROM f2."Postare";
