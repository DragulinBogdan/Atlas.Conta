-- Q04b (F0) Fisa contului 4111 x PARTENER, 2025 -- varianta de EGALITATE pentru pasul 2.
-- f0/q04.sql filtreaza pe REPARTITORUL LATURII, si pe 4111 cel mai activ e o UnitateInterna
-- ("Sediul central", 107.033 randuri). Cubul filtreaza pe PARTENERUL randului. Q04b aplica
-- regula cubului pe F0: partenerul = repartitorul de tip Partener/Angajat de pe ORICARE
-- latura, cu latura proprie prima.
-- ales: 01a0b48c-a9fe-73e1-89f8-2908bb52cd65 = "CONSUMATOR FINAL", partenerul cu cele mai
-- multe postari pe 4111 IN CUB (3.034). Acelasi id in cub/q04.sql.
-- Cifrele q04 si q04b NU se compara intre ele (cazuri diferite); q04b e perechea lui q04 pe cub.
SELECT
    f."Id" AS "Id", f."Data" AS "Data", f."NumarNota" AS "NumarNota", f."Sens" AS "Sens",
    f."Debit" AS "Debit", f."Credit" AS "Credit", f."SoldCurent" AS "SoldCurent",
    f."ContrapartidaId" AS "ContrapartidaId", cp."Simbol" AS "ContrapartidaSimbol",
    rep."Denumire" AS "RepartitorDenumire", f."DocumentId" AS "DocumentId",
    CAST(NULL AS text) AS "DocumentTip", doc."Numar" AS "DocumentNumar", f."Storno" AS "Storno"
FROM (
    SELECT a.*,
        SUM(a."Debit" - a."Credit") OVER (
            ORDER BY a."Data", a."Id", a."Sens" DESC
            ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS "SoldCurent"
    FROM (
            SELECT r."ID" AS "Id", r."Data" AS "Data", r."NumarNota" AS "NumarNota",
                CAST('D' AS text) AS "Sens", r."Valoare" AS "Debit",
                CAST(0 AS numeric(18,2)) AS "Credit", r."ContCreditId" AS "ContrapartidaId",
                r."DocumentId" AS "DocumentId", r."Storno" AS "Storno",
                r."DimensiuniDebit_RepartitorId" AS "RepartitorId",
                COALESCE(CASE WHEN rd."ClrType" IN ('Partener','Angajat') THEN r."DimensiuniDebit_RepartitorId" END,
                         CASE WHEN rc."ClrType" IN ('Partener','Angajat') THEN r."DimensiuniCredit_RepartitorId" END) AS "PartenerId"
            FROM "RegistruContabil" r
            LEFT JOIN "Repartitori" rd ON rd."ID" = r."DimensiuniDebit_RepartitorId" AND rd."GCRecord" = 0
            LEFT JOIN "Repartitori" rc ON rc."ID" = r."DimensiuniCredit_RepartitorId" AND rc."GCRecord" = 0
            WHERE r."GCRecord" = 0 AND r."ContDebitId" = '01a0b48c-a8b2-73ce-a6cd-a045b80364a8' AND r."Data" <= DATE '2025-12-31'
        UNION ALL
            SELECT r."ID" AS "Id", r."Data" AS "Data", r."NumarNota" AS "NumarNota",
                CAST('C' AS text) AS "Sens", CAST(0 AS numeric(18,2)) AS "Debit",
                r."Valoare" AS "Credit", r."ContDebitId" AS "ContrapartidaId",
                r."DocumentId" AS "DocumentId", r."Storno" AS "Storno",
                r."DimensiuniCredit_RepartitorId" AS "RepartitorId",
                COALESCE(CASE WHEN rc."ClrType" IN ('Partener','Angajat') THEN r."DimensiuniCredit_RepartitorId" END,
                         CASE WHEN rd."ClrType" IN ('Partener','Angajat') THEN r."DimensiuniDebit_RepartitorId" END) AS "PartenerId"
            FROM "RegistruContabil" r
            LEFT JOIN "Repartitori" rd ON rd."ID" = r."DimensiuniDebit_RepartitorId" AND rd."GCRecord" = 0
            LEFT JOIN "Repartitori" rc ON rc."ID" = r."DimensiuniCredit_RepartitorId" AND rc."GCRecord" = 0
            WHERE r."GCRecord" = 0 AND r."ContCreditId" = '01a0b48c-a8b2-73ce-a6cd-a045b80364a8' AND r."Data" <= DATE '2025-12-31'
    ) a
    WHERE a."PartenerId" = '01a0b48c-a9fe-73e1-89f8-2908bb52cd65'
) f
LEFT JOIN "Conturi" cp ON cp."ID" = f."ContrapartidaId" AND cp."GCRecord" = 0
LEFT JOIN "Repartitori" rep ON rep."ID" = f."RepartitorId" AND rep."GCRecord" = 0
LEFT JOIN "Documente" doc ON doc."ID" = f."DocumentId" AND doc."GCRecord" = 0
WHERE f."Data" >= DATE '2025-01-01'
