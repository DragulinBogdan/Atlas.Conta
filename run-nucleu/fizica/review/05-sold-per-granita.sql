-- Proba M2 + E4 + N1/N2: cub."Sold" per granita, numararile exacte si marimile.
-- Bazele: Atlas.Conta.Nucleu.Fizica.x1 si .x10

\timing on

-- ---------- 1. Sold: numarul de randuri per granita e CONSTANT la x10 ----------
-- Replicarea x10 (FZ-D6) a refolosit ACELEASI ID-uri de coordonate, deci fiecare
-- granita are aceleasi chei. Consecinta nedeclarata de sinteza: Q05+S, Q06+S si
-- Q11+S citesc O SINGURA granita, deci masoara date de x1, nu de x10 — exact
-- viciul pe care nota ° il declara doar pentru F0.
-- REZULTAT pe .x10 (Spatiu 1 = Contabil):
--   2016-12-31  321.142 | 2017..2023  389.656 fiecare | 2024, 2025  389.713
--   Stoc 109.391 pe 2016..2023, 110.786 pe 2024-2025; Fiscal 44.016 peste tot.
-- REZULTAT pe .x1: doar doua granite; 2024-12-31 are 57 randuri contabile si
--   6.947 de stoc (deschiderile), 2025-12-31 are 321.199 / 110.786 / 44.016.
--   Deci "321.199 pe Contabil" din §2.3 e cifra de x1, nu de x10.
SELECT "Granita", "Spatiu", count(*) FROM cub."Sold" GROUP BY 1, 2 ORDER BY 1, 2;

SELECT count(*) AS sold_total FROM cub."Sold";   -- .x10: 5.365.020 (rezultate.md: 5.365.168)

-- ---------- 2. Numarul EXACT de postari (contra estimarilor reltuples) ----------
-- marimi-x10.txt si pas2/out/marimi.txt raporteaza 11.557.253 / 11.561.913 /
-- 11.564.018 / 11.566.669 / 11.561.972 — toate `reltuples`, estimari de ANALYZE.
-- REZULTAT exact pe .x10: 6.975.448 + 2.772.457 + 1.814.640 = 11.562.545
-- REZULTAT exact pe .x1:     697.660 +   283.498 +   181.464 =  1.162.622
SELECT "Spatiu", count(*) FROM f2."Postare" GROUP BY 1 ORDER BY 1;

-- ---------- 3. Marimea REALA a indexilor, dupa iteratia declarata ----------
-- rezultate.md da F2 2.132 MB si F3 2.644 MB — stare de DINAINTE de scoaterea
-- lui C1/S1. Sinteza §2.4 scrie "≈1,4 GB dupa iteratie" dar pastreaza totalul
-- "≈4,5 GB", calculat cu 2.132. Cu 1.441 MB totalul e
-- 2.008 + 1.441 + 283 + 123 ≈ 3,85 GB, adica 1,7x F0, nu ~2x.
-- REZULTAT pe .x10: cub 778 MB | f1 1.752 MB | f2 1.441 MB | f3 1.952 MB
SELECT n.nspname, pg_size_pretty(sum(pg_relation_size(i.indexrelid))) AS indexi
FROM pg_index i
JOIN pg_class c ON c.oid = i.indexrelid
JOIN pg_namespace n ON n.oid = c.relnamespace
WHERE n.nspname IN ('f1', 'f2', 'f3', 'cub')
GROUP BY 1 ORDER BY 1;

-- ---------- 4. F1 nu a fost atins de iteratie (proba M4) ----------
-- C1 (Cont, Data) INCLUDE si S1 (Gestiune, Unitate, Data) au fost scoase din
-- f2 si f3. F1 are acelasi numar de indexi inainte si dupa.
-- REZULTAT pe .x10: f1 = 18, f2 = 20, f3 = 212.
-- Totusi medianele lui F1 s-au mutat intre tabel-x10.md si tabel-x10c.md cu
-- -12,3 % (Q02), -12 % (Q08), -10 % (Q11), -7,4 % (Q01), -6,1 % (Q05), -5,7 % (Q06).
-- Acela e pragul de zgomot al seriei, si el inghite castigul de "9-16 %".
SELECT schemaname, count(*) FROM pg_indexes
WHERE schemaname IN ('f1', 'f2', 'f3') GROUP BY 1 ORDER BY 1;

-- ---------- 5. Deschiderea CHIAR e balansata (intrebarea leadului) ----------
-- Contul-ancora din tools/Import1C/Deschidere.cs:104-115 face fiecare rand
-- balansat, deci si tranzactia unica Fel=3.
-- REZULTAT pe .x1: D = C = 43.995.897,03 pe 128 de postari (64 randuri x 2).
SELECT sum(CASE WHEN p."Latura" = 1 THEN p."Valoare" ELSE 0 END) AS d,
       sum(CASE WHEN p."Latura" = 2 THEN p."Valoare" ELSE 0 END) AS c,
       count(*)
FROM f2."Postare" p
JOIN cub."Tranzactie" t ON t."ID" = p."TranzactieId"
WHERE t."Fel" = 3 AND p."Spatiu" = 1;

-- ---------- 6. GCRecord (proba N3) ----------
-- pas1/02-transform.sql citeste registrele FARA `GCRecord = 0`, desi fiecare
-- interogare F0 il filtreaza. Inofensiv AICI, dar egalitatea nu acopera o baza
-- cu stergeri logice.
-- REZULTAT pe .x1: 0 randuri cu GCRecord <> 0 pe toate patru.
SELECT 'contabil' t, count(*) FILTER (WHERE "GCRecord" <> 0) gc, count(*) FROM "RegistruContabil"
UNION ALL SELECT 'stoc',        count(*) FILTER (WHERE "GCRecord" <> 0), count(*) FROM "RegistruStoc"
UNION ALL SELECT 'tva',         count(*) FILTER (WHERE "GCRecord" <> 0), count(*) FROM "RegistruTva"
UNION ALL SELECT 'imperecheri', count(*) FILTER (WHERE "GCRecord" <> 0), count(*) FROM "Imperecheri";
