-- A (sprijin pe date): regulile de contare care produc postarile de tert,
-- si corespondentele reale pe trezorerie / FCT / NIR.
\set ON_ERROR_STOP on
\pset footer off

\echo ===== A.1 ReguliContare pe tipurile cu cont de tert =====
SELECT td."Cod", td."ClrType", rc."NaturaFiltru", tm."Denumire" AS tip_material,
       rc."SursaContDebit", cd."Simbol" AS cont_debit,
       rc."SursaContCredit", cc."Simbol" AS cont_credit, rc."PastreazaSemn", rc."SemnFiltru"
FROM "ReguliContare" rc
JOIN "TipuriDocument" td ON td."ID" = rc."TipDocumentId"
LEFT JOIN "TipuriMaterial" tm ON tm."ID" = rc."TipMaterialId"
LEFT JOIN "Conturi" cd ON cd."ID" = rc."ContDebitId"
LEFT JOIN "Conturi" cc ON cc."ID" = rc."ContCreditId"
WHERE td."ClrType" IN ('FacturaIntrare','NIR','FacturaIesire','Plata','Incasare','Decont','ReturFurnizor','ReturClient')
ORDER BY td."Cod", rc."NaturaFiltru", tm."Denumire";

\echo
\echo ===== A.2 PoliticiTva: sursa contrapartidei pe tipuri =====
SELECT td."Cod", td."ClrType", pt."Directie", pt."SursaContrapartida", c."Simbol" AS fallback
FROM "PoliticiTva" pt JOIN "TipuriDocument" td ON td."ID" = pt."TipDocumentId"
LEFT JOIN "Conturi" c ON c."ID" = pt."ContrapartidaFallbackId" ORDER BY td."Cod";

\echo
\echo ===== A.3 PoliticiConex (FCT -> NIR) =====
SELECT s."Cod" AS sursa, t."Cod" AS tinta, pc."NaturaFiltru", pc."InverseazaLaturi"
FROM "PoliticiConex" pc JOIN "TipuriDocument" s ON s."ID" = pc."TipDocumentSursaId"
JOIN "TipuriDocument" t ON t."ID" = pc."TipDocumentTintaId";

\echo
\echo ===== A.4 corespondentele REALE (perechi D/C) pe tipurile de trezorerie si lant =====
SELECT d."ClrType", cd."Simbol" AS debit, cc."Simbol" AS credit,
       count(*) AS randuri, sum(r."Valoare") AS valoare
FROM "RegistruContabil" r JOIN tr."Doc" d ON d."ID" = r."DocumentId"
LEFT JOIN "Conturi" cd ON cd."ID" = r."ContDebitId"
LEFT JOIN "Conturi" cc ON cc."ID" = r."ContCreditId"
WHERE d."ClrType" IN ('Plata','Incasare','FacturaIntrare','NIR','FacturaIesire')
GROUP BY 1,2,3 ORDER BY 1, 4 DESC;

\echo
\echo ===== A.5 LinieSursaId: exista pe liniile de NIR? =====
SELECT count(*) AS linii_nir,
       count(*) FILTER (WHERE dd."LinieSursaId" IS NOT NULL) AS cu_linie_sursa
FROM "DocumentDetalii" dd JOIN tr."Doc" d ON d."ID" = dd."DocumentId"
WHERE d."ClrType" = 'NIR';
SELECT d."ClrType", count(*) AS linii, count(*) FILTER (WHERE dd."LinieSursaId" IS NOT NULL) AS cu_linie_sursa
FROM "DocumentDetalii" dd JOIN tr."Doc" d ON d."ID" = dd."DocumentId"
GROUP BY 1 HAVING count(*) FILTER (WHERE dd."LinieSursaId" IS NOT NULL) > 0 ORDER BY 2 DESC;

\echo
\echo ===== A.6 stingerea automata: cate imperecheri sunt Autogenerat =====
SELECT "Autogenerat", count(*), sum("Suma") FROM "Imperecheri" GROUP BY 1;

\echo
\echo ===== A.7 un stingator acopera cate partide =====
SELECT nr, count(*) AS stingatori FROM (
  SELECT "DocumentStingatorId", count(*) AS nr FROM "Imperecheri" GROUP BY 1) q
GROUP BY 1 ORDER BY 1;

\echo
\echo ===== A.8 plati/incasari partiale: suma imperecherii contra totalul stingatorului =====
SELECT dg."ClrType",
       count(*) AS stingatori,
       count(*) FILTER (WHERE a."Asignat" = dg."TotalStingere") AS consumat_integral,
       count(*) FILTER (WHERE a."Asignat" < dg."TotalStingere")  AS partial,
       count(*) FILTER (WHERE a."Asignat" > dg."TotalStingere")  AS peste_total
FROM (SELECT DISTINCT "DocumentStingatorId" AS id FROM "Imperecheri") s
JOIN tr."Doc" dg ON dg."ID" = s.id JOIN tr."Asignat" a ON a."DocumentId" = s.id
GROUP BY 1;

\echo
\echo ===== A.9 documente stinse: suma imperecherii contra TotalStingere =====
SELECT ds."ClrType", count(*) AS documente,
       count(*) FILTER (WHERE a."Asignat" = ds."TotalStingere") AS stins_integral,
       count(*) FILTER (WHERE a."Asignat" < ds."TotalStingere")  AS partial
FROM (SELECT DISTINCT "DocumentId" AS id FROM "Imperecheri") s
JOIN tr."Doc" ds ON ds."ID" = s.id JOIN tr."Asignat" a ON a."DocumentId" = s.id
GROUP BY 1;
