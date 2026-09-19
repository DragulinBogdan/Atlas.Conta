-- Proba M1: F0 CU referinta de decembrie, contra F2+S, pe ACEEASI baza, una dupa alta.
-- Baza: Atlas.Conta.Nucleu.Fizica.x10
-- docker exec -i contapal-postgres-1 psql -U postgres -d Atlas.Conta.Nucleu.Fizica.x10 -f -
--
-- DE CE: sinteza (§2.4) spune "F0 nu scaleaza pe «sold la data» prin constructie".
-- Fals: SolduriService.Referinte = "ultima inchisa plus FIECARE decembrie inchis"
-- (nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Motor/SolduriService.cs:75-89).
-- Intr-o baza cu 10 ani inchisi, Referinta(2024-12-31) EXISTA si F0 citeste
-- snapshot + rulajele unui an, exact ca F2+S. Replicarea x10 a pasului 1 nu a
-- creat referintele, deci F0 a fost masurat fara mecanismul lui.
--
-- Simularea e de COST, nu de cifra: se foloseste snapshot-ul existent (An=2025,
-- Luna=12, 184.780 randuri) ca si cum ar fi granita 2024-12-31, plus rulajele
-- unui an din registru. Formele de acces si numarul de randuri sunt cele reale
-- ale lui SolduriService.AtomiCumulati cu referinta gasita.
--
-- REZULTAT (4 rulari, prima aruncata): F0 cu referinta 58,2 / 57,4 / 57,1 ms
--                                      F2+S             100,9 / 100,5 / 98,0 ms
-- Sinteza raporteaza F0 618 ms (fara referinta) contra F2+S 113 ms.
-- Cauza diferentei de cheiere: SolduriPerioadaContabil are 184.780 randuri la
-- granita, cub."Sold" are 389.713 pe Contabil la aceeasi granita (vezi 05-*.sql).

\timing on

-- ---------- A. F0 cu referinta (forma AtomiCumulati: snapshot + rulajele de dupa) ----------
EXPLAIN (ANALYZE, BUFFERS)
SELECT u."ContId",
       COALESCE(sum(u."Debit"), 0.0)  AS d,
       COALESCE(sum(u."Credit"), 0.0) AS c
FROM (
  SELECT sp."ContId", sp."Debit", sp."Credit"
  FROM "SolduriPerioadaContabil" sp
  WHERE sp."GCRecord" = 0 AND sp."An" = 2025 AND sp."Luna" = 12
  UNION ALL
  SELECT r."ContDebitId", r."Valoare", 0.0
  FROM "RegistruContabil" r
  WHERE r."GCRecord" = 0 AND r."Data" >= DATE '2025-01-01' AND r."Data" <= DATE '2025-12-31'
  UNION ALL
  SELECT r."ContCreditId", 0.0, r."Valoare"
  FROM "RegistruContabil" r
  WHERE r."GCRecord" = 0 AND r."Data" >= DATE '2025-01-01' AND r."Data" <= DATE '2025-12-31'
) u
GROUP BY u."ContId";

-- ---------- B. F2+S, acelasi gran, aceeasi masina, acelasi moment ----------
EXPLAIN (ANALYZE, BUFFERS)
SELECT u."Cont", sum(u."ID_"), sum(u."RD_")
FROM (
  SELECT s."Cont",
         CASE WHEN s."Latura" = 1 THEN s."Valoare" ELSE 0.0 END AS "ID_",
         0.0 AS "RD_"
  FROM cub."Sold" s
  WHERE s."Spatiu" = 1 AND s."Granita" = DATE '2024-12-31'
  UNION ALL
  SELECT p."Cont", 0.0,
         CASE WHEN p."Latura" = 1 THEN p."Valoare" ELSE 0.0 END
  FROM f2."Postare" p
  WHERE p."Spatiu" = 1 AND p."Data" >= DATE '2025-01-01' AND p."Data" <= DATE '2025-12-31'
) u
GROUP BY u."Cont";
