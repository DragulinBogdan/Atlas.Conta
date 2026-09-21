-- Pas 1: GATE. Fiecare proba intoarce numarul de abateri; (a)-(e) trebuie 0
-- (sau egalitate exacta). (f)/(f2)/(g) sunt probe de semantica, raportate.
\set ON_ERROR_STOP on
\timing on

-- (a) echilibrul D=C per tranzactie, pe Contabil
SELECT 'a_tranzactii_dezechilibrate' proba, count(*)::text valoare FROM (
  SELECT "TranzactieId"
  FROM f1."Postare" WHERE "Spatiu"=1
  GROUP BY "TranzactieId"
  HAVING COALESCE(sum("Valoare") FILTER (WHERE "Latura"=1),0)
      <> COALESCE(sum("Valoare") FILTER (WHERE "Latura"=2),0)) x;

-- (b) sumele per cont, pe fiecare latura, fata de RegistruContabil
SELECT 'b_conturi_debit_diferite' proba, count(*)::text valoare FROM (
  SELECT COALESCE(c."Cont", r."ContDebitId") cont
  FROM (SELECT "Cont", sum("Valoare") s FROM f1."Postare" WHERE "Spatiu"=1 AND "Latura"=1 GROUP BY 1) c
  FULL JOIN (SELECT "ContDebitId", sum("Valoare") s FROM "RegistruContabil" GROUP BY 1) r
         ON r."ContDebitId" = c."Cont"
  WHERE COALESCE(c.s,0) <> COALESCE(r.s,0)) x
UNION ALL
SELECT 'b_conturi_credit_diferite', count(*)::text FROM (
  SELECT COALESCE(c."Cont", r."ContCreditId") cont
  FROM (SELECT "Cont", sum("Valoare") s FROM f1."Postare" WHERE "Spatiu"=1 AND "Latura"=2 GROUP BY 1) c
  FULL JOIN (SELECT "ContCreditId", sum("Valoare") s FROM "RegistruContabil" GROUP BY 1) r
         ON r."ContCreditId" = c."Cont"
  WHERE COALESCE(c.s,0) <> COALESCE(r.s,0)) y;

-- (c) sumele per lot, pe Stoc
SELECT 'c_loturi_diferite' proba, count(*)::text valoare FROM (
  SELECT COALESCE(c."Unitate", r."LotId") lot
  FROM (SELECT "Unitate", sum("Cantitate") q, sum("Valoare") v FROM f1."Postare" WHERE "Spatiu"=2 GROUP BY 1) c
  FULL JOIN (SELECT "LotId", sum("Cantitate") q, sum("Valoare") v FROM "RegistruStoc" GROUP BY 1) r
         ON r."LotId" = c."Unitate"
  WHERE COALESCE(c.q,0) <> COALESCE(r.q,0) OR COALESCE(c.v,0) <> COALESCE(r.v,0)) x;

-- (d) sumele per (versiune CodTva, perioada), pe Fiscal
SELECT 'd_tva_chei_diferite' proba, count(*)::text valoare FROM (
  SELECT COALESCE(c."CodTvaId", r.cod) cod, COALESCE(c."PerioadaDeclarare", r.per) per
  FROM (SELECT "CodTvaId", "PerioadaDeclarare",
               sum("Valoare") FILTER (WHERE "RolTva"=1) baza,
               sum("Valoare") FILTER (WHERE "RolTva"=2) tva
        FROM f1."Postare" WHERE "Spatiu"=3 GROUP BY 1,2) c
  FULL JOIN (SELECT ct."ID" cod, v."PerioadaAn"*100+v."PerioadaLuna" per,
                    sum(v."Baza") baza, sum(v."Tva") tva
             FROM "RegistruTva" v
             JOIN cub."CodTva" ct ON ct."TipTvaId"=v."TipTvaId" AND ct."Regim"=v."Regim"
                                 AND ct."Cota"=v."Cota" AND ct."Sens"=v."Sens"
             GROUP BY 1,2) r
         ON r.cod = c."CodTvaId" AND r.per = c."PerioadaDeclarare"
  WHERE COALESCE(c.baza,0) <> COALESCE(r.baza,0) OR COALESCE(c.tva,0) <> COALESCE(r.tva,0)) x;

-- (e) numaratori: contabil = 2*randuri + 2*imperecheri; stoc = randuri; fiscal = 2*randuri
SELECT 'e_contabil_asteptat' proba,
       (2*(SELECT count(*) FROM "RegistruContabil") + 2*(SELECT count(*) FROM "Imperecheri"))::text valoare
UNION ALL SELECT 'e_contabil_real', (SELECT count(*) FROM f1."Postare" WHERE "Spatiu"=1)::text
UNION ALL SELECT 'e_stoc_asteptat', (SELECT count(*) FROM "RegistruStoc")::text
UNION ALL SELECT 'e_stoc_real', (SELECT count(*) FROM f1."Postare" WHERE "Spatiu"=2)::text
UNION ALL SELECT 'e_fiscal_asteptat', (2*(SELECT count(*) FROM "RegistruTva"))::text
UNION ALL SELECT 'e_fiscal_real', (SELECT count(*) FROM f1."Postare" WHERE "Spatiu"=3)::text
UNION ALL SELECT 'e_tranzactii_operare', (SELECT count(*) FROM cub."Tranzactie" WHERE "Fel"=1)::text
UNION ALL SELECT 'e_tranzactii_storno', (SELECT count(*) FROM cub."Tranzactie" WHERE "Fel"=2)::text
UNION ALL SELECT 'e_tranzactii_deschidere', (SELECT count(*) FROM cub."Tranzactie" WHERE "Fel"=3)::text
UNION ALL SELECT 'e_tranzactii_imperechere', (SELECT count(*) FROM cub."Tranzactie" WHERE "Fel"=4)::text
UNION ALL SELECT 'e_stingatori_fara_postare_tert',
       (SELECT count(*) FROM (SELECT DISTINCT "DocumentStingatorId" d FROM "Imperecheri") s
        WHERE NOT EXISTS (SELECT 1 FROM cub."_TertDoc" x WHERE x."DocumentId"=s.d))::text
UNION ALL SELECT 'e_facturi_fara_postare_tert',
       (SELECT count(*) FROM (SELECT DISTINCT "DocumentId" d FROM "Imperecheri") s
        WHERE NOT EXISTS (SELECT 1 FROM cub."_TertDoc" x WHERE x."DocumentId"=s.d))::text
UNION ALL SELECT 'e_documente_cu_mai_multe_postari_tert',
       (SELECT count(*) FROM cub."_TertDoc" WHERE nr_postari_tert > 1)::text
UNION ALL SELECT 'e_stingatori_cu_mai_multe_postari_tert',
       (SELECT count(*) FROM cub."_TertDoc" t WHERE t.nr_postari_tert > 1
        AND EXISTS (SELECT 1 FROM "Imperecheri" i WHERE i."DocumentStingatorId"=t."DocumentId"))::text
UNION ALL SELECT 'e_fiscal_taxa_zero', (SELECT count(*) FROM "RegistruTva" WHERE "Tva" = 0)::text;

-- vederi de lucru pentru (f)/(f2): restul din cub per partida, cu semnul dat de
-- rolul contului (client: D-C; furnizor: C-D), la data-limita si fara limita
CREATE OR REPLACE VIEW cub."_RestCub" AS
SELECT p."Unitate" AS doc, d."ClrType",
       CASE WHEN c."RolTert" = 2 THEN -1 ELSE 1 END *
       (COALESCE(sum(p."Valoare") FILTER (WHERE p."Latura"=1),0)
      - COALESCE(sum(p."Valoare") FILTER (WHERE p."Latura"=2),0)) AS rest_total,
       CASE WHEN c."RolTert" = 2 THEN -1 ELSE 1 END *
       (COALESCE(sum(p."Valoare") FILTER (WHERE p."Latura"=1 AND p."Data" <= DATE '2025-12-31'),0)
      - COALESCE(sum(p."Valoare") FILTER (WHERE p."Latura"=2 AND p."Data" <= DATE '2025-12-31'),0)) AS rest_2025
FROM f1."Postare" p
JOIN "Documente" d ON d."ID" = p."Unitate"
JOIN "Conturi" c ON c."ID" = p."Cont"
WHERE p."Spatiu"=1 AND p."Unitate" IS NOT NULL
GROUP BY 1,2, c."RolTert";

-- (f) rest cub la 2025-12-31 vs PartideDeschise 12/2025, pe facturi
WITH pd AS (SELECT "DocumentId" doc, "Rest" rest FROM "PartideDeschise" WHERE "An"=2025 AND "Luna"=12)
SELECT 'f_'||lower(r."ClrType")||'_coincid' proba, count(*)::text valoare
FROM cub."_RestCub" r JOIN pd p ON p.doc = r.doc
WHERE r."ClrType" IN ('FacturaIesire','FacturaIntrare') AND r.rest_2025 = p.rest
GROUP BY 1
UNION ALL
SELECT 'f_'||lower(r."ClrType")||'_difera', count(*)::text
FROM cub."_RestCub" r JOIN pd p ON p.doc = r.doc
WHERE r."ClrType" IN ('FacturaIesire','FacturaIntrare') AND r.rest_2025 <> p.rest
GROUP BY 1
UNION ALL
SELECT 'f_'||lower(r."ClrType")||'_doar_in_cub', count(*)::text
FROM cub."_RestCub" r LEFT JOIN pd p ON p.doc = r.doc
WHERE r."ClrType" IN ('FacturaIesire','FacturaIntrare') AND p.doc IS NULL AND r.rest_2025 <> 0
GROUP BY 1
UNION ALL
SELECT 'f_'||lower(d."ClrType")||'_doar_in_partide', count(*)::text
FROM pd p JOIN "Documente" d ON d."ID" = p.doc
LEFT JOIN cub."_RestCub" r ON r.doc = p.doc
WHERE d."ClrType" IN ('FacturaIesire','FacturaIntrare') AND r.doc IS NULL
GROUP BY 1
ORDER BY 1;

-- (f) trei exemple de nepotrivire
WITH pd AS (SELECT "DocumentId" doc, "Rest" rest FROM "PartideDeschise" WHERE "An"=2025 AND "Luna"=12)
SELECT 'f_exemplu' proba, r."ClrType", r.doc::text, r.rest_2025 cub, p.rest partida,
       d."TotalStingere", t."Valoare" postat_pe_tert
FROM cub."_RestCub" r JOIN pd p ON p.doc = r.doc
JOIN "Documente" d ON d."ID" = r.doc
LEFT JOIN cub."_TertDoc" t ON t."DocumentId" = r.doc
WHERE r."ClrType" IN ('FacturaIesire','FacturaIntrare') AND r.rest_2025 <> p.rest
ORDER BY abs(r.rest_2025 - p.rest) DESC LIMIT 3;

-- (f2) rest cub FARA limita de data vs TotalStingere - suma imperecherilor
WITH asteptat AS (
  SELECT d."ID" doc, d."ClrType",
         d."TotalStingere" - COALESCE((SELECT sum(i."Suma") FROM "Imperecheri" i WHERE i."DocumentId"=d."ID"),0) rest
  FROM "Documente" d WHERE d."ClrType" IN ('FacturaIesire','FacturaIntrare'))
SELECT 'f2_'||lower(a."ClrType")||'_abateri' proba, count(*)::text valoare
FROM asteptat a JOIN cub."_RestCub" r ON r.doc = a.doc
WHERE r.rest_total <> a.rest
GROUP BY 1
UNION ALL
SELECT 'f2_'||lower(a."ClrType")||'_egale', count(*)::text
FROM asteptat a JOIN cub."_RestCub" r ON r.doc = a.doc
WHERE r.rest_total = a.rest
GROUP BY 1
ORDER BY 1;

WITH asteptat AS (
  SELECT d."ID" doc, d."ClrType", d."TotalStingere" ts,
         d."TotalStingere" - COALESCE((SELECT sum(i."Suma") FROM "Imperecheri" i WHERE i."DocumentId"=d."ID"),0) rest
  FROM "Documente" d WHERE d."ClrType" IN ('FacturaIesire','FacturaIntrare'))
SELECT 'f2_exemplu' proba, a."ClrType", a.doc::text, r.rest_total cub, a.rest asteptat,
       a.ts "TotalStingere", t."Valoare" postat_pe_tert
FROM asteptat a JOIN cub."_RestCub" r ON r.doc = a.doc
LEFT JOIN cub."_TertDoc" t ON t."DocumentId" = a.doc
WHERE r.rest_total <> a.rest ORDER BY abs(r.rest_total - a.rest) DESC LIMIT 3;

-- (g) amendamentul de mapare a Partenerului (contrapartida e pe cealalta latura)
WITH r AS (
  SELECT (rd."ClrType" IN ('Partener','Angajat')) pe_debit,
         (rc."ClrType" IN ('Partener','Angajat')) pe_credit
  FROM "RegistruContabil" x
  LEFT JOIN "Repartitori" rd ON rd."ID" = x."DimensiuniDebit_RepartitorId"
  LEFT JOIN "Repartitori" rc ON rc."ID" = x."DimensiuniCredit_RepartitorId")
SELECT 'g_randuri_partener_pe_ambele' proba, count(*)::text valoare FROM r WHERE pe_debit AND pe_credit
UNION ALL SELECT 'g_randuri_partener_doar_debit', count(*)::text FROM r WHERE pe_debit AND NOT COALESCE(pe_credit,false)
UNION ALL SELECT 'g_randuri_partener_doar_credit', count(*)::text FROM r WHERE COALESCE(pe_credit,false) AND NOT COALESCE(pe_debit,false)
UNION ALL SELECT 'g_randuri_fara_partener', count(*)::text FROM r WHERE NOT COALESCE(pe_debit,false) AND NOT COALESCE(pe_credit,false)
UNION ALL SELECT 'g_postari_tert_fara_partener',
  (SELECT count(*) FROM f1."Postare" p JOIN "Conturi" c ON c."ID"=p."Cont"
   WHERE p."Spatiu"=1 AND c."RolTert" <> 0 AND p."Partener" IS NULL)::text
UNION ALL SELECT 'g_postari_tert_total',
  (SELECT count(*) FROM f1."Postare" p JOIN "Conturi" c ON c."ID"=p."Cont"
   WHERE p."Spatiu"=1 AND c."RolTert" <> 0)::text
UNION ALL SELECT 'g_postari_contabile_cu_partener',
  (SELECT count(*) FROM f1."Postare" WHERE "Spatiu"=1 AND "Partener" IS NOT NULL)::text
UNION ALL SELECT 'g_postari_contabile_cu_gestiune',
  (SELECT count(*) FROM f1."Postare" WHERE "Spatiu"=1 AND "Gestiune" IS NOT NULL)::text;
