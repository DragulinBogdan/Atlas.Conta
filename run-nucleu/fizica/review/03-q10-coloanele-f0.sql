-- Proba E1: Q10 pe cub, cu setul de coloane al lui F0.
-- Baza: Atlas.Conta.Nucleu.Fizica.x10
--
-- DE CE: pas2/cub/q10.sql calculeaza DOUA sume pe DOUA chei (Gestiune, Unitate).
-- f0/q10.sql (SQL real EF, capturat in run-f28/pas3/saft-B12-0.sql) calculeaza
-- SASE sume conditionate plus DOUA numaratori, pe TREI chei
-- (RepartitorId, LotId, TipStoc). Sinteza raporteaza F0 698 -> F2 524 ca victorie.
--
-- REZULTAT (4 rulari, prima aruncata):
--   A, forma masurata in pas2 (2 sume, 2 chei)      538 / 534 / 515 ms   (pas2: 523,6)
--   B, setul de coloane al lui F0 (6 sume + count)  691 / 657 / 657 ms
-- F0 = 697,6 ms. Castigul scade de la -25 % la -6 %.
-- B e inca INCOMPLETA: cheia TipStoc nu exista in modelul de masurare (FZ-D3),
-- deci gruparea ramane pe doua chei, nu pe trei.

\timing on

-- ---------- A. forma masurata in pas2 ----------
EXPLAIN (ANALYZE, BUFFERS)
SELECT p."Gestiune", p."Unitate" AS "LotId",
       COALESCE(sum(p."Cantitate"), 0.0) AS "Cantitate",
       COALESCE(sum(p."Valoare"), 0.0)   AS "Valoare"
FROM f2."Postare" p
WHERE p."Spatiu" = 2 AND p."Data" <= DATE '2025-12-31'
GROUP BY 1, 2
HAVING COALESCE(sum(p."Cantitate"), 0.0) <> 0.0;

-- ---------- B. aceleasi coloane ca f0/q10.sql ----------
EXPLAIN (ANALYZE, BUFFERS)
SELECT p."Gestiune", p."Unitate" AS "LotId",
       COALESCE(sum(CASE WHEN p."Data" <  DATE '2025-12-01' THEN p."Cantitate" ELSE 0.0 END), 0.0) AS "CantitateInitiala",
       COALESCE(sum(CASE WHEN p."Data" <  DATE '2025-12-01' THEN p."Valoare"   ELSE 0.0 END), 0.0) AS "ValoareInitiala",
       COALESCE(sum(CASE WHEN p."Data" >= DATE '2025-12-01' THEN p."Cantitate" ELSE 0.0 END), 0.0) AS "CantitateRulaj",
       COALESCE(sum(CASE WHEN p."Data" >= DATE '2025-12-01' THEN p."Valoare"   ELSE 0.0 END), 0.0) AS "ValoareRulaj",
       COALESCE(sum(CASE WHEN p."Data" <  DATE '2025-12-01' THEN 1 ELSE 0 END), 0)::int AS "RanduriInitiale",
       count(*)::int AS "Randuri"
FROM f2."Postare" p
WHERE p."Spatiu" = 2 AND p."Data" <= DATE '2025-12-31'
GROUP BY 1, 2;
