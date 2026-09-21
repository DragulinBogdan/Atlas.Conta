-- Ajutatoare in schema tr3 (singura in care scriu). Idempotent.
DROP TABLE IF EXISTS tr3.stoc; DROP TABLE IF EXISTS tr3.cont; DROP TABLE IF EXISTS tr3.pereche;
CREATE TABLE tr3.stoc AS
SELECT rs."ID", rs."DetaliuId", rs."DocumentId", rs."TipStoc", rs."LotId", rs."RepartitorId",
       rs."Cantitate", rs."Valoare", rs."Storno", rs."Data",
       COALESCE(doc."ClrType",'(deschidere)') AS tip,
       rep."ClrType" AS rep_clr, rep."Cod" AS rep_cod, rep."Denumire" AS rep_den,
       lot."ProdusId", p."TipMaterialId", tm."ContImplicitId" AS cont_tip_material,
       ci."Simbol" AS simbol_tip_material
FROM "RegistruStoc" rs
LEFT JOIN "Documente" doc ON doc."ID"=rs."DocumentId"
LEFT JOIN "Repartitori" rep ON rep."ID"=rs."RepartitorId"
LEFT JOIN "Loturi" lot ON lot."ID"=rs."LotId"
LEFT JOIN "Produse" p ON p."ID"=lot."ProdusId"
LEFT JOIN "TipuriMaterial" tm ON tm."ID"=p."TipMaterialId"
LEFT JOIN "Conturi" ci ON ci."ID"=tm."ContImplicitId";
CREATE INDEX ix_tr3_stoc_det ON tr3.stoc("DetaliuId");
CREATE INDEX ix_tr3_stoc_doc ON tr3.stoc("DocumentId");

CREATE TABLE tr3.cont AS
SELECT rc."ID", rc."DetaliuId", rc."DocumentId", rc."Valoare", rc."Storno", rc."Data",
       rc."ContDebitId", rc."ContCreditId",
       cd."Simbol" AS simbol_d, cc."Simbol" AS simbol_c,
       rc."DimensiuniDebit_RepartitorId" AS rep_d, rc."DimensiuniCredit_RepartitorId" AS rep_c,
       rc."DimensiuniDebit_MaterialId" AS mat_d, rc."DimensiuniCredit_MaterialId" AS mat_c,
       COALESCE(doc."ClrType",'(deschidere)') AS tip
FROM "RegistruContabil" rc
LEFT JOIN "Conturi" cd ON cd."ID"=rc."ContDebitId"
LEFT JOIN "Conturi" cc ON cc."ID"=rc."ContCreditId"
LEFT JOIN "Documente" doc ON doc."ID"=rc."DocumentId";
CREATE INDEX ix_tr3_cont_det ON tr3.cont("DetaliuId");
CREATE INDEX ix_tr3_cont_doc ON tr3.cont("DocumentId");
ANALYZE tr3.stoc; ANALYZE tr3.cont;
SELECT 'tr3.stoc' t, count(*) n FROM tr3.stoc UNION ALL SELECT 'tr3.cont', count(*) FROM tr3.cont;
