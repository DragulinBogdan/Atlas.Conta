-- B5. Decontul cu angajatul si cazurile partener-partener.
\set ON_ERROR_STOP on
\pset footer off

\echo ===== B5.1 exista Decont / Angajat pe Flax? =====
SELECT (SELECT count(*) FROM tr."Doc" WHERE "ClrType"='Decont')                       AS documente_decont,
       (SELECT count(*) FROM "Repartitori" WHERE "ClrType"='Angajat')                 AS repartitori_angajat,
       (SELECT count(*) FROM tr."Picior" WHERE "ContSimbol" LIKE '542%')              AS picioare_542,
       (SELECT count(*) FROM tr."Picior" p JOIN "Repartitori" r ON r."ID"=p."Partener"
          WHERE r."ClrType"='Angajat')                                                AS picioare_cu_angajat,
       (SELECT count(*) FROM "Repartitori" WHERE "ClrType"='Partener')                AS repartitori_partener;

\echo
\echo ===== B5.2 documente cu >=2 parteneri distincti pe postarile de tert =====
WITH t AS (
  SELECT n."DocumentId", count(DISTINCT n."Partener") AS parteneri, count(*) AS conturi
  FROM tr."TertDocContNet" n GROUP BY 1 HAVING count(DISTINCT n."Partener") >= 2)
SELECT d."ClrType", count(*) AS documente, sum(t.parteneri) AS suma_parteneri
FROM t JOIN tr."Doc" d ON d."ID" = t."DocumentId" GROUP BY 1 ORDER BY 2 DESC;

\echo
\echo ===== B5.3 cele cu >=2 parteneri: detaliul celor 5 =====
WITH t AS (
  SELECT n."DocumentId" FROM tr."TertDocContNet" n GROUP BY 1 HAVING count(DISTINCT n."Partener") >= 2)
SELECT d."ClrType", d."Numar", d."Data", d."TotalStingere",
       n."ContSimbol", n."Latura", r."Denumire" AS partener, n."Valoare"
FROM t JOIN tr."Doc" d ON d."ID" = t."DocumentId"
JOIN tr."TertDocCont" n ON n."DocumentId" = d."ID"
LEFT JOIN "Repartitori" r ON r."ID" = n."Partener"
ORDER BY d."Numar", n."ContSimbol", n."Latura";

\echo
\echo ===== B5.4 compensarea: notele contabile care STING (si pe cate documente) =====
SELECT count(DISTINCT i."DocumentStingatorId") AS note_care_sting,
       count(*) AS imperecheri, sum(i."Suma") AS suma,
       count(*) FILTER (WHERE ds."ClrType"='FacturaIesire')  AS spre_fcl,
       count(*) FILTER (WHERE ds."ClrType"='FacturaIntrare') AS spre_fct
FROM "Imperecheri" i
JOIN tr."Doc" dg ON dg."ID" = i."DocumentStingatorId" AND dg."ClrType" = 'NotaContabila'
JOIN tr."Doc" ds ON ds."ID" = i."DocumentId";

\echo
\echo ===== B5.5 note care sting: au si 401 SI 4111 pe acelasi partener? =====
WITH n AS (
  SELECT DISTINCT i."DocumentStingatorId" AS id FROM "Imperecheri" i
  JOIN tr."Doc" dg ON dg."ID" = i."DocumentStingatorId" AND dg."ClrType"='NotaContabila')
SELECT count(*) AS note,
       count(*) FILTER (WHERE (SELECT count(DISTINCT t."RolTert") FROM tr."TertDocContNet" t
                               WHERE t."DocumentId" = n.id) = 2) AS cu_ambele_roluri,
       count(*) FILTER (WHERE (SELECT count(DISTINCT t."Partener") FROM tr."TertDocContNet" t
                               WHERE t."DocumentId" = n.id) >= 2) AS cu_doi_parteneri
FROM n;

\echo
\echo ===== B5.6 lantul avans->factura->regularizare: stingatorii care sunt ei insisi stinsi =====
SELECT dg."ClrType" AS tip, count(DISTINCT i."DocumentStingatorId") AS ca_stingator,
       count(DISTINCT j."DocumentId") AS si_stinse_de_altcineva
FROM "Imperecheri" i
JOIN tr."Doc" dg ON dg."ID" = i."DocumentStingatorId"
LEFT JOIN "Imperecheri" j ON j."DocumentId" = i."DocumentStingatorId"
GROUP BY 1 ORDER BY 2 DESC;
