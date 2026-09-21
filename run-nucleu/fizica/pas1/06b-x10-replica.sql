-- Pas 1, x10: o trecere de replicare, cu :k ani inapoi. Se ruleaza in baza x10
-- o data pentru fiecare k = 1..9. Se citesc DOAR randurile originale
-- (tranzactiile din cub."_OrigTranzactii" si randurile F0 din 2025), deci
-- trecerile nu se compun.
\set ON_ERROR_STOP on
\timing on

DROP TABLE IF EXISTS cub."_Map";
CREATE UNLOGGED TABLE cub."_Map" AS SELECT o."ID" AS vechi, uuidv7() AS nou FROM cub."_OrigTranzactii" o;
CREATE UNIQUE INDEX ON cub."_Map"(vechi);
ANALYZE cub."_Map";

INSERT INTO cub."Tranzactie"("ID","DocumentId","Fel","Data","ScrisLa")
SELECT m.nou, t."DocumentId", t."Fel",
       (t."Data" - (:k * INTERVAL '1 year'))::date,
       t."ScrisLa" - (:k * INTERVAL '1 year')
FROM cub."Tranzactie" t JOIN cub."_Map" m ON m.vechi = t."ID";

INSERT INTO f1."Postare"(
  "ID","Spatiu","TranzactieId","DocumentId","LinieId","Data","Cont","Latura",
  "Partener","Gestiune","Produs","Unitate","CodTvaId","RolTva","PerioadaDeclarare",
  "Carte","Valuta","CodFunctional","CodEconomic","SursaFinantare",
  "UnitateOrganizatorica","Proiect","CentruCost","Atribuit",
  "Cantitate","ValoareValuta","Valoare")
SELECT uuidv7(), p."Spatiu", m.nou, p."DocumentId", p."LinieId",
       (p."Data" - (:k * INTERVAL '1 year'))::date, p."Cont", p."Latura",
       p."Partener", p."Gestiune", p."Produs", p."Unitate", p."CodTvaId", p."RolTva",
       p."PerioadaDeclarare" - :k * 100,
       p."Carte", p."Valuta", p."CodFunctional", p."CodEconomic", p."SursaFinantare",
       p."UnitateOrganizatorica", p."Proiect", p."CentruCost", p."Atribuit",
       p."Cantitate", p."ValoareValuta", p."Valoare"
FROM f1."Postare" p JOIN cub."_Map" m ON m.vechi = p."TranzactieId";

INSERT INTO f2."Postare"(
  "ID","Spatiu","TranzactieId","DocumentId","LinieId","Data","Cont","Latura",
  "Partener","Gestiune","Produs","Unitate","CodTvaId","RolTva","PerioadaDeclarare",
  "Carte","Valuta","CodFunctional","CodEconomic","SursaFinantare",
  "UnitateOrganizatorica","Proiect","CentruCost","Atribuit",
  "Cantitate","ValoareValuta","Valoare")
SELECT uuidv7(), p."Spatiu", m.nou, p."DocumentId", p."LinieId",
       (p."Data" - (:k * INTERVAL '1 year'))::date, p."Cont", p."Latura",
       p."Partener", p."Gestiune", p."Produs", p."Unitate", p."CodTvaId", p."RolTva",
       p."PerioadaDeclarare" - :k * 100,
       p."Carte", p."Valuta", p."CodFunctional", p."CodEconomic", p."SursaFinantare",
       p."UnitateOrganizatorica", p."Proiect", p."CentruCost", p."Atribuit",
       p."Cantitate", p."ValoareValuta", p."Valoare"
FROM f2."Postare" p JOIN cub."_Map" m ON m.vechi = p."TranzactieId";

INSERT INTO f3."Postare"(
  "ID","Spatiu","TranzactieId","DocumentId","LinieId","Data","Cont","Latura",
  "Partener","Gestiune","Produs","Unitate","CodTvaId","RolTva","PerioadaDeclarare",
  "Carte","Valuta","CodFunctional","CodEconomic","SursaFinantare",
  "UnitateOrganizatorica","Proiect","CentruCost","Atribuit",
  "Cantitate","ValoareValuta","Valoare")
SELECT uuidv7(), p."Spatiu", m.nou, p."DocumentId", p."LinieId",
       (p."Data" - (:k * INTERVAL '1 year'))::date, p."Cont", p."Latura",
       p."Partener", p."Gestiune", p."Produs", p."Unitate", p."CodTvaId", p."RolTva",
       p."PerioadaDeclarare" - :k * 100,
       p."Carte", p."Valuta", p."CodFunctional", p."CodEconomic", p."SursaFinantare",
       p."UnitateOrganizatorica", p."Proiect", p."CentruCost", p."Atribuit",
       p."Cantitate", p."ValoareValuta", p."Valoare"
FROM f3."Postare" p JOIN cub."_Map" m ON m.vechi = p."TranzactieId";

-- F0: aceleasi randuri in registre, cu ID nou si data mutata.
-- SolduriPerioada*, PartideDeschise, Imperecheri, Documente NU se replica.
INSERT INTO "RegistruContabil"(
  "ID","Data","NumarNota","ContDebitId","ContCreditId","Valoare",
  "DimensiuniDebit_RepartitorId","DimensiuniDebit_MaterialId","DimensiuniDebit_CodFunctionalId",
  "DimensiuniDebit_CodEconomicId","DimensiuniDebit_SursaFinantareId","DimensiuniDebit_UnitateId",
  "DimensiuniDebit_ProiectId","DimensiuniDebit_CentruCostId",
  "DimensiuniCredit_RepartitorId","DimensiuniCredit_MaterialId","DimensiuniCredit_CodFunctionalId",
  "DimensiuniCredit_CodEconomicId","DimensiuniCredit_SursaFinantareId","DimensiuniCredit_UnitateId",
  "DimensiuniCredit_ProiectId","DimensiuniCredit_CentruCostId",
  "Storno","DocumentId","DetaliuId","GCRecord","OptimisticLockField")
SELECT uuidv7(), (r."Data" - (:k * INTERVAL '1 year'))::date, r."NumarNota", r."ContDebitId", r."ContCreditId", r."Valoare",
  r."DimensiuniDebit_RepartitorId", r."DimensiuniDebit_MaterialId", r."DimensiuniDebit_CodFunctionalId",
  r."DimensiuniDebit_CodEconomicId", r."DimensiuniDebit_SursaFinantareId", r."DimensiuniDebit_UnitateId",
  r."DimensiuniDebit_ProiectId", r."DimensiuniDebit_CentruCostId",
  r."DimensiuniCredit_RepartitorId", r."DimensiuniCredit_MaterialId", r."DimensiuniCredit_CodFunctionalId",
  r."DimensiuniCredit_CodEconomicId", r."DimensiuniCredit_SursaFinantareId", r."DimensiuniCredit_UnitateId",
  r."DimensiuniCredit_ProiectId", r."DimensiuniCredit_CentruCostId",
  r."Storno", r."DocumentId", r."DetaliuId", r."GCRecord", r."OptimisticLockField"
FROM "RegistruContabil" r WHERE r."Data" >= DATE '2025-01-01';

INSERT INTO "RegistruStoc"(
  "ID","Data","TipStoc","LotId","RepartitorId","Cantitate","Valoare",
  "Storno","DocumentId","DetaliuId","GCRecord","OptimisticLockField")
SELECT uuidv7(), (s."Data" - (:k * INTERVAL '1 year'))::date, s."TipStoc", s."LotId", s."RepartitorId",
  s."Cantitate", s."Valoare", s."Storno", s."DocumentId", s."DetaliuId", s."GCRecord", s."OptimisticLockField"
FROM "RegistruStoc" s WHERE s."Data" >= DATE '2025-01-01';

INSERT INTO "RegistruTva"(
  "ID","Data","PerioadaAn","PerioadaLuna","ScrisLa","Sens","DocumentId","DetaliuId",
  "PartenerId","TipTvaId","Regim","Cota","Baza","Tva","Storno","GCRecord","OptimisticLockField")
SELECT uuidv7(), (v."Data" - (:k * INTERVAL '1 year'))::date, v."PerioadaAn" - :k, v."PerioadaLuna",
  v."ScrisLa" - (:k * INTERVAL '1 year'), v."Sens", v."DocumentId", v."DetaliuId",
  v."PartenerId", v."TipTvaId", v."Regim", v."Cota", v."Baza", v."Tva", v."Storno",
  v."GCRecord", v."OptimisticLockField"
FROM "RegistruTva" v WHERE v."Data" >= DATE '2025-01-01';

DROP TABLE cub."_Map";
