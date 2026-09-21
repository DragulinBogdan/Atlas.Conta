-- Pas 1: verificarile de oprire, rulate INAINTE de transformare. Read-only.
\set ON_ERROR_STOP on
SELECT 'clrtype_repartitori' k, string_agg(DISTINCT "ClrType", ',' ORDER BY "ClrType") v FROM "Repartitori";
SELECT 'clrtype_necunoscut' k, count(*)::text v FROM "Repartitori"
  WHERE "ClrType" NOT IN ('Partener','Angajat','Gestiune','ContPropriu','UnitateInterna');
SELECT 'imperecheri_inverse' k, count(*)::text v FROM "Imperecheri" WHERE "InverseazaId" IS NOT NULL;
SELECT 'imperecheri_suma_negativa' k, count(*)::text v FROM "Imperecheri" WHERE "Suma" < 0;
SELECT 'storno_contabil' k, count(*)::text v FROM "RegistruContabil" WHERE "Storno"
UNION ALL SELECT 'storno_stoc', count(*)::text FROM "RegistruStoc" WHERE "Storno"
UNION ALL SELECT 'storno_tva', count(*)::text FROM "RegistruTva" WHERE "Storno";
SELECT 'gcrecord_nenul_contabil' k, count(*)::text v FROM "RegistruContabil" WHERE "GCRecord" IS NOT NULL AND "GCRecord" <> 0
UNION ALL SELECT 'gcrecord_nenul_stoc', count(*)::text FROM "RegistruStoc" WHERE "GCRecord" IS NOT NULL AND "GCRecord" <> 0
UNION ALL SELECT 'gcrecord_nenul_tva', count(*)::text FROM "RegistruTva" WHERE "GCRecord" IS NOT NULL AND "GCRecord" <> 0;
SELECT 'stoc_fara_cont_implicit' k, count(*)::text v
FROM "RegistruStoc" s
LEFT JOIN "Loturi" l ON l."ID" = s."LotId"
LEFT JOIN "Produse" p ON p."ID" = l."ProdusId"
LEFT JOIN "TipuriMaterial" tm ON tm."ID" = p."TipMaterialId"
WHERE tm."ContImplicitId" IS NULL;
SELECT 'randuri_contabil' k, count(*)::text v FROM "RegistruContabil"
UNION ALL SELECT 'randuri_stoc', count(*)::text FROM "RegistruStoc"
UNION ALL SELECT 'randuri_tva', count(*)::text FROM "RegistruTva"
UNION ALL SELECT 'deschidere_contabil', count(*)::text FROM "RegistruContabil" WHERE "DocumentId" IS NULL
UNION ALL SELECT 'deschidere_stoc', count(*)::text FROM "RegistruStoc" WHERE "DocumentId" IS NULL
UNION ALL SELECT 'documente', count(*)::text FROM "Documente"
UNION ALL SELECT 'linii', count(*)::text FROM "DocumentDetalii"
UNION ALL SELECT 'imperecheri', count(*)::text FROM "Imperecheri"
UNION ALL SELECT 'partide_deschise', count(*)::text FROM "PartideDeschise"
UNION ALL SELECT 'solduri_contabil', count(*)::text FROM "SolduriPerioadaContabil"
UNION ALL SELECT 'solduri_stoc', count(*)::text FROM "SolduriPerioadaStoc";
