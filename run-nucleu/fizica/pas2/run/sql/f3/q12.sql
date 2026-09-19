EXPLAIN (ANALYZE, BUFFERS)
SELECT p."ID", p."Data", p."TranzactieId", p."DocumentId", p."LinieId",
       p."Cont", p."Latura", p."Valoare", p."Partener", p."Gestiune", p."Produs",
       p."Unitate", p."CodFunctional", p."CodEconomic", p."SursaFinantare",
       p."UnitateOrganizatorica", p."Proiect", p."CentruCost",
       t."Fel", t."Data" AS "TranzactieData", t."ScrisLa",
       d."Numar" AS "DocumentNumar", d."ClrType" AS "DocumentTip"
FROM f3."Postare" p
JOIN cub."Tranzactie" t ON t."ID" = p."TranzactieId"
LEFT JOIN (SELECT dd."ID", dd."Numar", dd."ClrType" FROM "Documente" dd WHERE dd."GCRecord" = 0) d
       ON d."ID" = p."DocumentId"
WHERE p."Spatiu" = 1 AND p."Data" >= DATE '2025-06-01' AND p."Data" <= DATE '2025-06-30'
  AND p."DocumentId" IS NOT NULL;
