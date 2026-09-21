-- Q14 (cub) SCRIEREA: postarile documentului FCT `FA/EU-2500084872` (49 de linii)
-- reinserate cu ID-uri noi si TranzactieId nou, plus randul de Tranzactie.
-- In cub documentul are 100 de postari contabile (2 x 50 de randuri) + 98 fiscale
-- (2 x 49) + 0 de stoc = 198, fata de 50 + 49 + 0 = 99 de randuri in F0.
-- Tranzactie cu ROLLBACK: nimic nu se persista.
-- `Data >= 2025-01-01` si `LIMIT 1`: la x10 replicarea a pastrat ACELASI DocumentId pe
-- toate cele 10 copii, deci fara filtru s-ar scrie 10 documente, nu unul. Masuram
-- scrierea ACELUIASI document intr-o tabela de 10 ori mai mare, nu o scriere de 10 ori
-- mai mare. Filtrul e neutru la x1 (documentul e din 2025).
-- TranzactieId literal (nu uuidv7()) ca rularile repetate sa fie identice.
BEGIN;

EXPLAIN (ANALYZE, BUFFERS)
INSERT INTO cub."Tranzactie"("ID","DocumentId","Fel","Data","ScrisLa")
SELECT '01999999-0000-7000-8000-000000000001', t."DocumentId", t."Fel", t."Data", now()
FROM cub."Tranzactie" t
WHERE t."DocumentId" = '01a0b4d1-42ba-7622-a0c8-6a1e534425fb' AND t."Fel" = 1
  AND t."Data" >= DATE '2025-01-01'
LIMIT 1;

EXPLAIN (ANALYZE, BUFFERS)
INSERT INTO f2."Postare"(
  "ID","Spatiu","TranzactieId","DocumentId","LinieId","Data","Cont","Latura",
  "Partener","Gestiune","Produs","Unitate","CodTvaId","RolTva","PerioadaDeclarare",
  "Carte","Valuta","CodFunctional","CodEconomic","SursaFinantare",
  "UnitateOrganizatorica","Proiect","CentruCost","Atribuit",
  "Cantitate","ValoareValuta","Valoare")
SELECT uuidv7(), p."Spatiu", '01999999-0000-7000-8000-000000000001', p."DocumentId",
  p."LinieId", p."Data", p."Cont", p."Latura", p."Partener", p."Gestiune", p."Produs",
  p."Unitate", p."CodTvaId", p."RolTva", p."PerioadaDeclarare", p."Carte", p."Valuta",
  p."CodFunctional", p."CodEconomic", p."SursaFinantare", p."UnitateOrganizatorica",
  p."Proiect", p."CentruCost", p."Atribuit", p."Cantitate", p."ValoareValuta", p."Valoare"
FROM f2."Postare" p
WHERE p."DocumentId" = '01a0b4d1-42ba-7622-a0c8-6a1e534425fb'
  AND p."Data" >= DATE '2025-01-01';

ROLLBACK;
