CREATE VIEW "PostareVizual" AS
SELECT p.*,
    t."Fel" AS "TranzactieFel",
    d."Numar" AS "DocumentNumar",
    c."Simbol" AS "ContSimbol",
    r."Cod" AS "RepartitorCod",
    pf."Cod" AS "PartenerFiscalCod",
    lf."Cod" AS "LocCod",
    pr."Cod" AS "ProdusCod",
    CASE p."FelUnitate"
        WHEN 1 THEN pr."Cod" || '/' || to_char(l."Data", 'YYYY-MM-DD')
        WHEN 2 THEN r."Cod" || '/' || to_char(p."UnitateDeschisa", 'YYYY-MM-DD')
        WHEN 3 THEN f."NumarInventar"
    END AS "UnitateCod",
    tt."Cod" AS "TipTvaCod",
    cf."Cod" AS "CodFunctionalCod",
    ce."Cod" AS "CodEconomicCod",
    sf."Cod" AS "SursaFinantareCod",
    u."Cod" AS "UnitateOrganizatoricaCod",
    pj."Cod" AS "ProiectCod",
    cc."Cod" AS "CentruCostCod"
FROM "Postare" p
LEFT JOIN "Tranzactie" t ON t."ID" = p."TranzactieId"
LEFT JOIN "Documente" d ON d."ID" = p."DocumentId"
LEFT JOIN "Conturi" c ON c."ID" = p."Cont"
LEFT JOIN "Repartitori" r ON r."ID" = p."Repartitor"
LEFT JOIN "Repartitori" pf ON pf."ID" = p."PartenerFiscal"
LEFT JOIN "Repartitori" lf ON lf."ID" = p."Loc"
LEFT JOIN "Produse" pr ON pr."ID" = p."Produs"
LEFT JOIN "Loturi" l ON p."FelUnitate" = 1 AND l."ID" = p."Unitate"
LEFT JOIN "Imobilizari" f ON p."FelUnitate" = 3 AND f."ID" = p."Unitate"
LEFT JOIN "TipuriTva" tt ON tt."ID" = p."TipTvaId"
LEFT JOIN "CoduriFunctionale" cf ON cf."ID" = p."CodFunctional"
LEFT JOIN "CoduriEconomice" ce ON ce."ID" = p."CodEconomic"
LEFT JOIN "SurseFinantare" sf ON sf."ID" = p."SursaFinantare"
LEFT JOIN "Unitati" u ON u."ID" = p."UnitateOrganizatorica"
LEFT JOIN "Proiecte" pj ON pj."ID" = p."Proiect"
LEFT JOIN "Repartitori" cc ON cc."ID" = p."CentruCost";
