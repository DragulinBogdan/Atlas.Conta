BEGIN TRANSACTION READ ONLY;

WITH tipuri(clr) AS (
    VALUES ('Decont'), ('PunereInFunctiune'), ('AmortizareLunara'), ('IesireImobilizare')
)
SELECT t.clr, count(d."ID") AS documente,
       count(d."ID") FILTER (WHERE d."Stare" = 1) AS operate
FROM tipuri t
LEFT JOIN "Documente" d ON d."ClrType" = t.clr AND d."GCRecord" = 0
GROUP BY t.clr ORDER BY t.clr;

ROLLBACK;
