-- Proba M3: Q06+S masurat contra Q06+S care livreaza ce livreaza raportul F0.
-- Baza: Atlas.Conta.Nucleu.Fizica.x10
--
-- DE CE: pas2/cub/q06s.sql intoarce TREI coloane (Unitate, Sold, Sens). f0/q06.sql
-- intoarce raportul: DocumentId, Tip, Numar, Data, ContrapartidaId,
-- ContrapartidaDenumire, Sens, Total, Asignat, Rest, prin sase uniuni peste
-- "Documente" cu LEFT JOIN pe "Repartitori" si excluderea viramentelor interne.
-- Sinteza compara 63 ms (cub) cu 72 ms (F0) si conchide "bate F0".
--
-- REZULTAT (3 rulari calde):
--   A (forma masurata in pas2)            70,9 ms, 114.680 randuri
--   B (+ antetul de document, minimul)    143,3 / 131,0 / 139,6 ms
-- F0 = 71,8 ms pentru raportul COMPLET. Deci ~2x, nu "bate".
-- B e inca INCOMPLETA fata de F0: nu are cele sase ramuri pe tip, nu exclude
-- viramentele interne ContPropriu->ContPropriu, nu calculeaza Asignat.
--
-- Mulțimile difera si ele: 114.680 randuri (cub) contra 93.206 (F0), iar cubul
-- filtreaza HAVING <> 0 unde F0 filtreaza Rest > 0.

\timing on
\set CONTURI '01a0b48c-a8aa-7e36-8f14-4fa2fa4ea009,01a0b48c-a8ab-7b50-b3b7-071a2b7a349b,01a0b48c-a8ac-7e4b-a6f0-9387b76e96eb,01a0b48c-a8ac-7ab5-a52e-152a7dc2545f,01a0b48c-a8ad-7886-bf9d-1ef6ec9b9d85,01a0b48c-a8ad-738e-9300-b9812b654f5d,01a0b48c-a8ae-7e28-ab0c-d459207dc577,01a0b48c-a8af-76f6-8adc-75a198ef6a63,01a0b48c-a8af-70d2-ad7b-1d1f3709b4fe,01a0b48c-a8b0-734d-a49d-507ee62971ef,01a0b48c-a8b1-7d53-a2d6-b11eb0cc58ed,01a0b48c-a8b2-73ce-a6cd-a045b80364a8,01a0b48c-a8b2-7841-9d84-1c268bdb59e3,01a0b48c-a8b3-7212-9f12-a75dc52b91c2,01a0b48c-a8b3-70d2-85c6-14793081762f,01a0b48c-a8b4-75ad-a6f5-0403d7678e18'

-- ---------- A. forma masurata in pas2 (trei coloane) ----------
EXPLAIN (ANALYZE, BUFFERS)
SELECT g."Unitate", g."Sold"
FROM (
  SELECT s."Unitate",
         sum(CASE WHEN s."Latura" = 1 THEN s."Valoare" ELSE -s."Valoare" END) AS "Sold"
  FROM cub."Sold" s
  WHERE s."Spatiu" = 1 AND s."Granita" = DATE '2025-12-31'
    AND s."Unitate" IS NOT NULL
    AND s."Cont" = ANY (('{' || :'CONTURI' || '}')::uuid[])
  GROUP BY 1
  HAVING sum(CASE WHEN s."Latura" = 1 THEN s."Valoare" ELSE -s."Valoare" END) <> 0.0
) g;

-- ---------- B. + antetul de document (minimul pe care F0 il livreaza) ----------
EXPLAIN (ANALYZE, BUFFERS)
SELECT g."Unitate" AS "DocumentId", d."ClrType" AS "Tip", d."Numar", d."Data",
       d."TotalStingere" AS "Total", g."Sold" AS "Rest",
       rp."Denumire" AS "Contrapartida",
       CASE WHEN g."Sold" > 0 THEN 'Creanta' ELSE 'Datorie' END AS "Sens"
FROM (
  SELECT s."Unitate",
         sum(CASE WHEN s."Latura" = 1 THEN s."Valoare" ELSE -s."Valoare" END) AS "Sold"
  FROM cub."Sold" s
  WHERE s."Spatiu" = 1 AND s."Granita" = DATE '2025-12-31'
    AND s."Unitate" IS NOT NULL
    AND s."Cont" = ANY (('{' || :'CONTURI' || '}')::uuid[])
  GROUP BY 1
  HAVING sum(CASE WHEN s."Latura" = 1 THEN s."Valoare" ELSE -s."Valoare" END) <> 0.0
) g
JOIN "Documente" d ON d."ID" = g."Unitate" AND d."GCRecord" = 0 AND d."Stare" = 1
LEFT JOIN "Repartitori" rp ON rp."ID" = COALESCE(d."PrimitorId", d."PredatorId") AND rp."GCRecord" = 0;
