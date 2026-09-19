EXPLAIN (ANALYZE, BUFFERS)
SELECT r."RepartitorId", r."LotId", r."TipStoc", COALESCE(sum(CASE
	    WHEN r."Data" < '2025-12-01' THEN r."Cantitate"
	    ELSE 0.0
	END), 0.0) AS "CantitateInitiala", COALESCE(sum(CASE
	    WHEN r."Data" < '2025-12-01' THEN r."Valoare"
	    ELSE 0.0
	END), 0.0) AS "ValoareInitiala", COALESCE(sum(CASE
	    WHEN r."Data" >= '2025-12-01' THEN r."Cantitate"
	    ELSE 0.0
	END), 0.0) AS "CantitateRulaj", COALESCE(sum(CASE
	    WHEN r."Data" >= '2025-12-01' THEN r."Valoare"
	    ELSE 0.0
	END), 0.0) AS "ValoareRulaj", COALESCE(sum(CASE
	    WHEN r."Data" < '2025-12-01' THEN 1
	    ELSE 0
	END), 0)::int AS "RanduriInitiale", count(*)::int AS "Randuri"
	FROM "RegistruStoc" AS r
	WHERE r."GCRecord" = 0 AND r."Data" <= '2025-12-31'
	GROUP BY r."RepartitorId", r."LotId", r."TipStoc";
