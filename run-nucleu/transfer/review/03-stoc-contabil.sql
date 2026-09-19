\pset footer off
\echo ===== R3.1 conturile de stoc: rulajele contabile de AZI contra rulajelor pe care le-ar aduce postarea unica de stoc =====
WITH cont_stoc AS (SELECT "ID","Simbol" FROM "Conturi" WHERE "Simbol" ~ '^3'),
azi AS (
  SELECT c."Simbol",
         sum(CASE WHEN v.lat=1 THEN v.val ELSE 0 END) AS rulaj_d,
         sum(CASE WHEN v.lat=2 THEN v.val ELSE 0 END) AS rulaj_c,
         sum(CASE WHEN v.lat=1 THEN v.val ELSE -v.val END) AS sold
  FROM "RegistruContabil" r
  CROSS JOIN LATERAL (VALUES (1,r."ContDebitId",r."Valoare"),(2,r."ContCreditId",r."Valoare")) v(lat,cont,val)
  JOIN cont_stoc c ON c."ID"=v.cont GROUP BY 1),
maine AS (
  SELECT c."Simbol",
         sum(CASE WHEN s."Valoare">0 THEN s."Valoare" ELSE 0 END)  AS rulaj_d,
         sum(CASE WHEN s."Valoare"<0 THEN -s."Valoare" ELSE 0 END) AS rulaj_c,
         sum(s."Valoare") AS sold
  FROM "RegistruStoc" s JOIN "Loturi" l ON l."ID"=s."LotId"
       JOIN "Produse" p ON p."ID"=l."ProdusId" JOIN "TipuriMaterial" t ON t."ID"=p."TipMaterialId"
       JOIN cont_stoc c ON c."ID"=t."ContImplicitId" GROUP BY 1)
SELECT COALESCE(a."Simbol",m."Simbol") AS cont,
       round(COALESCE(a.rulaj_d,0),2) AS azi_rulaj_d, round(COALESCE(m.rulaj_d,0),2) AS stoc_rulaj_d,
       round(COALESCE(m.rulaj_d,0)-COALESCE(a.rulaj_d,0),2) AS delta_rulaj_d,
       round(COALESCE(a.sold,0),2) AS azi_sold, round(COALESCE(m.sold,0),2) AS stoc_sold,
       round(COALESCE(m.sold,0)-COALESCE(a.sold,0),2) AS delta_sold
FROM azi a FULL OUTER JOIN maine m ON m."Simbol"=a."Simbol" ORDER BY 1;

\echo
\echo ===== R3.2 de unde vine delta de rulaj: rulajul de stoc pe tip de document, pe contul produsului =====
SELECT COALESCE(d."ClrType",'(deschidere)') AS tip, c."Simbol" AS cont,
       count(*) AS randuri,
       round(sum(CASE WHEN s."Valoare">0 THEN s."Valoare" ELSE 0 END),2) AS rulaj_d_nou,
       round(sum(CASE WHEN s."Valoare"<0 THEN -s."Valoare" ELSE 0 END),2) AS rulaj_c_nou,
       round(sum(s."Valoare"),2) AS sold_nou
FROM "RegistruStoc" s JOIN "Loturi" l ON l."ID"=s."LotId"
     JOIN "Produse" p ON p."ID"=l."ProdusId" JOIN "TipuriMaterial" t ON t."ID"=p."TipMaterialId"
     JOIN "Conturi" c ON c."ID"=t."ContImplicitId"
     LEFT JOIN "Documente" d ON d."ID"=s."DocumentId"
WHERE COALESCE(d."ClrType",'x') IN ('NotaTransfer','Asamblare') OR s."DocumentId" IS NULL
GROUP BY 1,2 ORDER BY 1,2;
