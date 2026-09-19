-- Proba M5: ce nu poate vedea nicio proba a pasului despre amendamentul 4
-- (imperecherea = tranzactie proprie datata).
-- Baza: Atlas.Conta.Nucleu.Fizica.x1
-- Cere tabelele de lucru ale pasului 1: cub."_TranzactieImperechere", cub."_TertDoc"
-- (create de pas1/02-transform.sql, UNLOGGED, ramase in baza).

\timing on

-- ---------- 1. TOATE postarile de imperechere sunt in afara ferestrei lotului ----------
-- Fiecare interogare Q01..Q13 taie pe Data <= 2025-12-31 sau pe o luna din 2025.
-- REZULTAT: min = max = 2026-09-18, 88.896 postari; 0 in fereastra.
-- Deci cele 13 probe de egalitate nu vad NICIUNA, iar proba (a) a gate-ului
-- (Suma D = Suma C per tranzactie) e 0 = 0 prin constructie.
SELECT min(p."Data") AS data_min, max(p."Data") AS data_max, count(*) AS postari
FROM f2."Postare" p
JOIN cub."Tranzactie" t ON t."ID" = p."TranzactieId"
WHERE t."Fel" = 4;

SELECT count(*) AS postari_in_fereastra_lotului
FROM f2."Postare" p
JOIN cub."Tranzactie" t ON t."ID" = p."TranzactieId"
WHERE t."Fel" = 4 AND p."Data" <= DATE '2025-12-31';

-- ---------- 2. Ambele picioare iau Cont/Latura de la STINGATOR ----------
-- pas1/02-transform.sql §4: `JOIN cub."_TertDoc" s ON s."DocumentId" = m."DocumentStingatorId"`
-- da Cont si Latura pentru AMANDOUA postarile; documentul stins primeste doar Unitate.
-- REZULTAT: 44.448 total | 1.200 cont diferit | 44.403 latura diferita
--           | 1.179 fara postare de tert pe documentul stins | 1.194 partener diferit
-- Cele 1.200: partida stinsa se inchide pe un cont pe care n-a postat niciodata.
-- Cele 1.179: cubul deschide o partida pe un document care n-are postare de tert.
SELECT count(*)                                                        AS total,
       count(*) FILTER (WHERE s."Cont"     IS DISTINCT FROM f."Cont")   AS cont_diferit,
       count(*) FILTER (WHERE s."Latura"   IS DISTINCT FROM f."Latura") AS latura_diferita,
       count(*) FILTER (WHERE f."DocumentId" IS NULL)                   AS fara_tert_pe_stins,
       count(*) FILTER (WHERE s."Partener" IS DISTINCT FROM f."Partener") AS partener_diferit
FROM cub."_TranzactieImperechere" m
JOIN      cub."_TertDoc" s ON s."DocumentId" = m."DocumentStingatorId"
LEFT JOIN cub."_TertDoc" f ON f."DocumentId" = m."DocumentId";

-- ---------- 3. Partida = documentul, nu postarea pe contul de tert ----------
-- `_TertDoc` alege UNA singura per document (DISTINCT ON, abs(Valoare) DESC).
-- REZULTAT: 59.549 documente cu o singura postare de tert, ~56.500 cu mai multe
-- (35.434 cu doua, 12.426 cu patru, pana la 228). Limita §3 mecanismul 1 din
-- pas2/egalitate.md e mult mai larga decat cele 2.072 de FCL citate.
SELECT nr_postari_tert, count(*) AS documente
FROM cub."_TertDoc"
GROUP BY 1 ORDER BY 1;

-- Stingatori cu mai multe postari de tert (nota coordonatorului: 38):
-- REZULTAT: 79 imperecheri.
SELECT count(*) AS imperecheri_cu_stingator_multi_tert
FROM cub."_TranzactieImperechere" m
JOIN cub."_TertDoc" s ON s."DocumentId" = m."DocumentStingatorId"
WHERE s.nr_postari_tert > 1;

-- ---------- 4. Valoare devine semnata pe Contabil ----------
-- Cele doua picioare sunt -Suma si +Suma pe ACEEASI latura, deci orice
-- `SUM(Valoare) WHERE Latura = 1` citit ca rulaj debitor BRUT per partida e gresit.
-- (Per cont se compenseaza, de aceea gate-ul (b) trece.)
-- REZULTAT: Latura 1 -> min -520.769,01 / max 520.769,01 pe 34.008 postari
--           Latura 2 -> min -600.688,00 / max 600.688,00 pe 54.888 postari
SELECT p."Latura", min(p."Valoare") AS val_min, max(p."Valoare") AS val_max, count(*)
FROM f2."Postare" p
JOIN cub."Tranzactie" t ON t."ID" = p."TranzactieId"
WHERE t."Fel" = 4
GROUP BY 1 ORDER BY 1;
