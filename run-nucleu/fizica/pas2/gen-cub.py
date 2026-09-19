# -*- coding: utf-8 -*-
"""Generatorul textelor SQL pe cub (pas 2). {T} = f1."Postare" / f2."Postare" / f3."Postare"."""
import os

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "cub")
os.makedirs(OUT, exist_ok=True)

C4111 = "01a0b48c-a8b2-73ce-a6cd-a045b80364a8"
PART = "01a0b48c-a9fe-73e1-89f8-2908bb52cd65"   # CONSUMATOR FINAL: 3.034 postari pe 4111 in cub
GEST = "01a0b48c-d7ca-73c5-88b4-c0b508a78342"   # TRANZIT
PROD = "01a0b48d-78b6-754c-92e9-c3250a37995b"
DOC228 = "01a0b494-a2da-7484-94ce-1019f56c4fef"
TERTI = ("01a0b48c-a8aa-7e36-8f14-4fa2fa4ea009,01a0b48c-a8ab-7b50-b3b7-071a2b7a349b,"
         "01a0b48c-a8ac-7e4b-a6f0-9387b76e96eb,01a0b48c-a8ac-7ab5-a52e-152a7dc2545f,"
         "01a0b48c-a8ad-7886-bf9d-1ef6ec9b9d85,01a0b48c-a8ad-738e-9300-b9812b654f5d,"
         "01a0b48c-a8ae-7e28-ab0c-d459207dc577,01a0b48c-a8af-76f6-8adc-75a198ef6a63,"
         "01a0b48c-a8af-70d2-ad7b-1d1f3709b4fe,01a0b48c-a8b0-734d-a49d-507ee62971ef,"
         "01a0b48c-a8b1-7d53-a2d6-b11eb0cc58ed,01a0b48c-a8b2-73ce-a6cd-a045b80364a8,"
         "01a0b48c-a8b2-7841-9d84-1c268bdb59e3,01a0b48c-a8b3-7212-9f12-a75dc52b91c2,"
         "01a0b48c-a8b3-70d2-85c6-14793081762f,01a0b48c-a8b4-75ad-a6f5-0403d7678e18")

SOLD_COLS = """       g."InitialDebit", g."InitialCredit",
       CASE WHEN g."InitialDebit" - g."InitialCredit" > 0.0 THEN g."InitialDebit" - g."InitialCredit" ELSE 0.0 END AS "SoldInitialDebit",
       CASE WHEN g."InitialDebit" - g."InitialCredit" < 0.0 THEN -(g."InitialDebit" - g."InitialCredit") ELSE 0.0 END AS "SoldInitialCredit",
       g."RulajDebit", g."RulajCredit",
       CASE WHEN ((g."InitialDebit" - g."InitialCredit") + g."RulajDebit") - g."RulajCredit" > 0.0 THEN ((g."InitialDebit" - g."InitialCredit") + g."RulajDebit") - g."RulajCredit" ELSE 0.0 END AS "SoldFinalDebit",
       CASE WHEN ((g."InitialDebit" - g."InitialCredit") + g."RulajDebit") - g."RulajCredit" < 0.0 THEN -(((g."InitialDebit" - g."InitialCredit") + g."RulajDebit") - g."RulajCredit") ELSE 0.0 END AS "SoldFinalCredit\""""

CONTURI_JOIN = 'LEFT JOIN (SELECT c."ID", c."Denumire", c."Simbol" FROM "Conturi" c WHERE c."GCRecord" = 0) c0 ON g."Cont" = c0."ID"'
REP_JOIN = 'LEFT JOIN (SELECT rp."ID", rp."Denumire" FROM "Repartitori" rp WHERE rp."GCRecord" = 0) r0 ON g."Partener" = r0."ID"'


def agregat(group_extra=""):
    ge = (", " + group_extra) if group_extra else ""
    sel = (group_extra + ", ") if group_extra else ""
    return """  SELECT p."Cont", {sel}
    COALESCE(sum(CASE WHEN p."Data" <  DATE '2025-01-01' AND p."Latura" = 1 THEN p."Valoare" ELSE 0.0 END), 0.0) AS "InitialDebit",
    COALESCE(sum(CASE WHEN p."Data" <  DATE '2025-01-01' AND p."Latura" = 2 THEN p."Valoare" ELSE 0.0 END), 0.0) AS "InitialCredit",
    COALESCE(sum(CASE WHEN p."Data" >= DATE '2025-01-01' AND p."Latura" = 1 THEN p."Valoare" ELSE 0.0 END), 0.0) AS "RulajDebit",
    COALESCE(sum(CASE WHEN p."Data" >= DATE '2025-01-01' AND p."Latura" = 2 THEN p."Valoare" ELSE 0.0 END), 0.0) AS "RulajCredit"
  FROM {{T}} p
  WHERE p."Spatiu" = 1 AND p."Data" <= DATE '2025-12-31'
  GROUP BY p."Cont"{ge}""".format(sel=sel.replace('p."', 'p."') if sel else "", ge=ge)


def agregat_s(group_extra=""):
    """Sold la 2024-12-31 + postarile lui 2025."""
    ge = (", " + group_extra.replace("p.", "u.")) if group_extra else ""
    su = (group_extra.replace("p.", "s.") + ", ") if group_extra else ""
    pu = (group_extra + ", ") if group_extra else ""
    uu = (group_extra.replace("p.", "u.") + ", ") if group_extra else ""
    return """  SELECT u."Cont", {uu}
    COALESCE(sum(u."ID_"), 0.0) AS "InitialDebit", COALESCE(sum(u."IC_"), 0.0) AS "InitialCredit",
    COALESCE(sum(u."RD_"), 0.0) AS "RulajDebit",   COALESCE(sum(u."RC_"), 0.0) AS "RulajCredit"
  FROM (
    SELECT s."Cont", {su}
           CASE WHEN s."Latura" = 1 THEN s."Valoare" ELSE 0.0 END AS "ID_",
           CASE WHEN s."Latura" = 2 THEN s."Valoare" ELSE 0.0 END AS "IC_",
           0.0 AS "RD_", 0.0 AS "RC_"
    FROM cub."Sold" s WHERE s."Spatiu" = 1 AND s."Granita" = DATE '2024-12-31'
    UNION ALL
    SELECT p."Cont", {pu}0.0, 0.0,
           CASE WHEN p."Latura" = 1 THEN p."Valoare" ELSE 0.0 END,
           CASE WHEN p."Latura" = 2 THEN p."Valoare" ELSE 0.0 END
    FROM {{T}} p
    WHERE p."Spatiu" = 1 AND p."Data" >= DATE '2025-01-01' AND p."Data" <= DATE '2025-12-31'
  ) u GROUP BY u."Cont"{ge}""".format(uu=uu, su=su, pu=pu, ge=ge)


Q = {}

Q["q01"] = """-- Q01 (cub) Balanta plata sintetica 2025, per Cont. Granul si coloanele = f0/q01.sql.
-- Sume pe coordonate: Spatiu=1, taiere pe Data, sensul din Latura; nimic din document.
SELECT g."Cont" AS "ContId", c0."Simbol" AS "ContSimbol", c0."Denumire" AS "ContDenumire",
       NULL AS "RepartitorId", NULL AS "RepartitorDenumire",
{cols}
FROM (
{agg}) g
{cj}
""".format(cols=SOLD_COLS, agg=agregat(), cj=CONTURI_JOIN)

Q["q01s"] = """-- Q01+S (cub) idem, cu soldul initial din cub."Sold" la granita 2024-12-31
-- (cea mai apropiata granita <= dataStart-1) si rulajele din tabela masurata.
SELECT g."Cont" AS "ContId", c0."Simbol" AS "ContSimbol", c0."Denumire" AS "ContDenumire",
       NULL AS "RepartitorId", NULL AS "RepartitorDenumire",
{cols}
FROM (
{agg}) g
{cj}
""".format(cols=SOLD_COLS, agg=agregat_s(), cj=CONTURI_JOIN)

Q["q02"] = """-- Q02 (cub) Balanta analitica Cont x Partener 2025, la 4111 si 401.
-- Semantica difera de f0/q02.sql (acolo cheia e repartitorul laturii, pe 4111 o
-- UnitateInterna); referinta de egalitate e f0/q02b.sql (partenerul de pe oricare latura).
SELECT g."Cont" AS "ContId", c0."Simbol" AS "ContSimbol", c0."Denumire" AS "ContDenumire",
       g."Partener" AS "RepartitorId", r0."Denumire" AS "RepartitorDenumire",
{cols}
FROM (
{agg}) g
{cj}
{rj}
WHERE c0."Simbol" IN ('4111', '401')
""".format(cols=SOLD_COLS, agg=agregat('p."Partener"'), cj=CONTURI_JOIN, rj=REP_JOIN)

Q["q02s"] = """-- Q02+S (cub) idem Q02, cu soldul initial din cub."Sold" la granita 2024-12-31.
SELECT g."Cont" AS "ContId", c0."Simbol" AS "ContSimbol", c0."Denumire" AS "ContDenumire",
       g."Partener" AS "RepartitorId", r0."Denumire" AS "RepartitorDenumire",
{cols}
FROM (
{agg}) g
{cj}
{rj}
WHERE c0."Simbol" IN ('4111', '401')
""".format(cols=SOLD_COLS, agg=agregat_s('p."Partener"'), cj=CONTURI_JOIN, rj=REP_JOIN)

FISA = """SELECT f."ID", f."Data", f."TranzactieId", f."Sens", f."Debit", f."Credit", f."SoldCurent",
       f."DocumentId", doc."Numar" AS "DocumentNumar", doc."ClrType" AS "DocumentTip"{cpsel}
FROM (
  SELECT p."ID", p."Data", p."TranzactieId", p."DocumentId",
         CASE WHEN p."Latura" = 1 THEN 'D' ELSE 'C' END AS "Sens",
         CASE WHEN p."Latura" = 1 THEN p."Valoare" ELSE 0.0 END AS "Debit",
         CASE WHEN p."Latura" = 2 THEN p."Valoare" ELSE 0.0 END AS "Credit",
         SUM(CASE WHEN p."Latura" = 1 THEN p."Valoare" ELSE -p."Valoare" END)
           OVER (ORDER BY p."Data", p."TranzactieId", p."ID"
                 ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS "SoldCurent"
  FROM {{T}} p
  WHERE p."Spatiu" = 1 AND p."Cont" = '{cont}' AND p."Data" <= DATE '2025-12-31'{extra}
) f
LEFT JOIN "Documente" doc ON doc."ID" = f."DocumentId" AND doc."GCRecord" = 0{cpjoin}
WHERE f."Data" >= DATE '2025-01-01'
"""

Q["q03"] = ("""-- Q03 (cub) Fisa contului 4111 pe 2025, sold cumulativ pe fereastra.
-- Ordinea ferestrei: (Data, TranzactieId, ID) -- pe cub tranzactia e reperul, nu ID-ul de rand.
-- FARA contrapartida (varianta a doua: q03c).
""" + FISA).format(cont=C4111, extra="", cpsel="", cpjoin="")

CP = """
LEFT JOIN (
  SELECT q."TranzactieId", string_agg(DISTINCT c2."Simbol", '+') AS "Contrapartida"
  FROM {T} q JOIN "Conturi" c2 ON c2."ID" = q."Cont"
  WHERE q."Spatiu" = 1 AND q."Cont" <> '%s'
    AND q."TranzactieId" IN (SELECT p2."TranzactieId" FROM {T} p2
                             WHERE p2."Spatiu" = 1 AND p2."Cont" = '%s'
                               AND p2."Data" <= DATE '2025-12-31' AND p2."Data" >= DATE '2025-01-01')
  GROUP BY q."TranzactieId") cp ON cp."TranzactieId" = f."TranzactieId\"""" % (C4111, C4111)

Q["q03c"] = ("""-- Q03c (cub) Fisa 4111 CU contrapartida = celelalte conturi ale tranzactiei
-- (string_agg pe TranzactieId). Pe tranzactia cu mai multe picioare contrapartida nu e
-- unica -- de aceea e lista, nu un cont (docs/nucleu/nucleu-coordonate-rapoarte.md §3).
""" + FISA).format(cont=C4111, extra="", cpsel=', cp."Contrapartida"', cpjoin=CP)

Q["q04"] = ("""-- Q04 (cub) Fisa 4111 x partenerul cel mai activ pe 4111 IN CUB.
-- ales: CONSUMATOR FINAL = %s, 3.034 postari din 144.241.
-- Referinta de egalitate: f0/q04b.sql (acelasi partener, luat de pe oricare latura).
-- f0/q04.sql masoara alt caz (Sediul central, o UnitateInterna, 107.033 randuri) --
-- cifrele nu se compara intre ele, de aceea q04b se masoara separat.
""" % PART + FISA).format(cont=C4111, extra="""
    AND p."Partener" = '%s'""" % PART, cpsel="", cpjoin="")

Q["q05"] = """-- Q05 (cub) Sold cont 4111 x Partener la 2025-12-31, FARA snapshot.
SELECT g."Cont" AS "ContId", c0."Simbol" AS "ContSimbol", c0."Denumire" AS "ContDenumire",
       g."Partener" AS "RepartitorId", r0."Denumire" AS "RepartitorDenumire",
       g."Debit", g."Credit",
       CASE WHEN g."Debit" - g."Credit" > 0.0 THEN g."Debit" - g."Credit" ELSE 0.0 END AS "SoldDebitor",
       CASE WHEN g."Debit" - g."Credit" < 0.0 THEN -(g."Debit" - g."Credit") ELSE 0.0 END AS "SoldCreditor"
FROM (
  SELECT p."Cont", p."Partener",
         COALESCE(sum(CASE WHEN p."Latura" = 1 THEN p."Valoare" ELSE 0.0 END), 0.0) AS "Debit",
         COALESCE(sum(CASE WHEN p."Latura" = 2 THEN p."Valoare" ELSE 0.0 END), 0.0) AS "Credit"
  FROM {{T}} p
  WHERE p."Spatiu" = 1 AND p."Cont" = '{cont}' AND p."Data" <= DATE '2025-12-31'
  GROUP BY p."Cont", p."Partener") g
{cj}
{rj}
WHERE g."Debit" - g."Credit" <> 0.0
""".format(cont=C4111, cj=CONTURI_JOIN, rj=REP_JOIN)

Q["q05s"] = """-- Q05+S (cub) idem, direct din cub."Sold" la granita 2025-12-31 (= data ceruta).
SELECT g."Cont" AS "ContId", c0."Simbol" AS "ContSimbol", c0."Denumire" AS "ContDenumire",
       g."Partener" AS "RepartitorId", r0."Denumire" AS "RepartitorDenumire",
       g."Debit", g."Credit",
       CASE WHEN g."Debit" - g."Credit" > 0.0 THEN g."Debit" - g."Credit" ELSE 0.0 END AS "SoldDebitor",
       CASE WHEN g."Debit" - g."Credit" < 0.0 THEN -(g."Debit" - g."Credit") ELSE 0.0 END AS "SoldCreditor"
FROM (
  SELECT s."Cont", s."Partener",
         COALESCE(sum(CASE WHEN s."Latura" = 1 THEN s."Valoare" ELSE 0.0 END), 0.0) AS "Debit",
         COALESCE(sum(CASE WHEN s."Latura" = 2 THEN s."Valoare" ELSE 0.0 END), 0.0) AS "Credit"
  FROM cub."Sold" s
  WHERE s."Spatiu" = 1 AND s."Granita" = DATE '2025-12-31' AND s."Cont" = '{cont}'
  GROUP BY s."Cont", s."Partener") g
{cj}
{rj}
WHERE g."Debit" - g."Credit" <> 0.0
""".format(cont=C4111, cj=CONTURI_JOIN, rj=REP_JOIN)

Q["q06"] = """-- Q06 (cub) Partidele cu rest la 2025-12-31: postarile pe conturi cu RolTert != 0,
-- grupate pe Unitate (= documentul care a deschis partida). Sensul = semnul soldului.
SELECT g."Unitate" AS "DocumentId", g."Sold",
       CASE WHEN g."Sold" > 0.0 THEN 'Creanta' ELSE 'Datorie' END AS "Sens"
FROM (
  SELECT p."Unitate",
         COALESCE(sum(CASE WHEN p."Latura" = 1 THEN p."Valoare" ELSE -p."Valoare" END), 0.0) AS "Sold"
  FROM {{T}} p
  WHERE p."Spatiu" = 1 AND p."Data" <= DATE '2025-12-31'
    AND p."Unitate" IS NOT NULL
    AND p."Cont" = ANY ('{{{terti}}}')
  GROUP BY p."Unitate"
  HAVING COALESCE(sum(CASE WHEN p."Latura" = 1 THEN p."Valoare" ELSE -p."Valoare" END), 0.0) <> 0.0) g
""".format(terti=TERTI)

Q["q06s"] = """-- Q06+S (cub) idem, direct din cub."Sold" la granita 2025-12-31.
SELECT g."Unitate" AS "DocumentId", g."Sold",
       CASE WHEN g."Sold" > 0.0 THEN 'Creanta' ELSE 'Datorie' END AS "Sens"
FROM (
  SELECT s."Unitate",
         COALESCE(sum(CASE WHEN s."Latura" = 1 THEN s."Valoare" ELSE -s."Valoare" END), 0.0) AS "Sold"
  FROM cub."Sold" s
  WHERE s."Spatiu" = 1 AND s."Granita" = DATE '2025-12-31'
    AND s."Unitate" IS NOT NULL
    AND s."Cont" = ANY ('{{{terti}}}')
  GROUP BY s."Unitate"
  HAVING COALESCE(sum(CASE WHEN s."Latura" = 1 THEN s."Valoare" ELSE -s."Valoare" END), 0.0) <> 0.0) g
""".format(terti=TERTI)

Q["q07"] = """-- Q07 (cub) Jurnal cumparari 2025-06: Spatiu=3, PerioadaDeclarare=202506,
-- CodTva.Sens=1; per (DocumentId, TranzactieId, CodTvaId), Baza/Taxa = rolul.
-- Din document se citesc DOAR atribute (Numar, Data, ClrType), niciodata cifre.
SELECT g."DocumentId", d."Numar" AS "DocumentNumar", d."Data" AS "DocumentData",
       d."ClrType" AS "DocumentTip", g."TranzactieId", g."CodTvaId",
       ct."Regim", ct."Cota", t."Cod" AS "TipTvaCod", t."CodSafTAchizitie" AS "CodSafT",
       g."Baza", g."Taxa"
FROM (
  SELECT p."DocumentId", p."TranzactieId", p."CodTvaId",
         COALESCE(sum(CASE WHEN p."RolTva" = 1 THEN p."Valoare" ELSE 0.0 END), 0.0) AS "Baza",
         COALESCE(sum(CASE WHEN p."RolTva" = 2 THEN p."Valoare" ELSE 0.0 END), 0.0) AS "Taxa"
  FROM {T} p
  WHERE p."Spatiu" = 3 AND p."PerioadaDeclarare" = 202506
    AND p."CodTvaId" IN (SELECT k."ID" FROM cub."CodTva" k WHERE k."Sens" = 1)
  GROUP BY p."DocumentId", p."TranzactieId", p."CodTvaId") g
JOIN cub."CodTva" ct ON ct."ID" = g."CodTvaId"
LEFT JOIN (SELECT tt."ID", tt."Cod", tt."CodSafTAchizitie" FROM "TipuriTva" tt WHERE tt."GCRecord" = 0) t ON t."ID" = ct."TipTvaId"
LEFT JOIN (SELECT dd."ID", dd."Numar", dd."Data", dd."ClrType" FROM "Documente" dd WHERE dd."GCRecord" = 0) d ON d."ID" = g."DocumentId"
"""

Q["q08"] = """-- Q08 (cub) D394 2025-06, doar agregatul (restul e randare): per
-- (DocumentId, TranzactieId, Partener, CodTvaId), Suma Baza si Suma Taxa.
SELECT p."DocumentId", p."TranzactieId", p."Partener", p."CodTvaId",
       count(*) AS "Randuri",
       COALESCE(sum(CASE WHEN p."RolTva" = 1 THEN p."Valoare" ELSE 0.0 END), 0.0) AS "Baza",
       COALESCE(sum(CASE WHEN p."RolTva" = 2 THEN p."Valoare" ELSE 0.0 END), 0.0) AS "Taxa"
FROM {T} p
WHERE p."Spatiu" = 3 AND p."PerioadaDeclarare" = 202506
GROUP BY p."DocumentId", p."TranzactieId", p."Partener", p."CodTvaId"
"""

Q["q09"] = """-- Q09 (cub) FIFO: loturile cu sold > 0 in gestiunea TRANZIT la 2025-06-30,
-- IN ORDINEA FIFO, in SQL (azi ordinea e in C#, dupa un GetObjectByKey<Lot> per lot).
-- Lipseste TipStoc: nu e coordonata a cubului in modelul de masurare (se declara).
SELECT p."Unitate" AS "LotId",
       COALESCE(sum(p."Cantitate"), 0.0) AS "Sold",
       COALESCE(sum(p."Valoare"), 0.0) AS "Valoare",
       min(p."Data") AS "PrimaMiscare"
FROM {{T}} p
WHERE p."Spatiu" = 2 AND p."Gestiune" = '{gest}' AND p."Data" <= DATE '2025-06-30'
GROUP BY p."Unitate"
HAVING COALESCE(sum(p."Cantitate"), 0.0) > 0.0
ORDER BY min(p."Data"), p."Unitate"
""".format(gest=GEST)

Q["q09b"] = """-- Q09b (cub) varianta de EGALITATE cu f0/q09.sql: acelasi produs SI aceeasi
-- gestiune (calea reala a lui StocService.AlocaFifoTolerant).
-- f0/q09.sql mai filtreaza TipStoc = 1; cubul n-are TipStoc -- diferenta se raporteaza.
SELECT p."Unitate" AS "LotId", COALESCE(sum(p."Cantitate"), 0.0) AS "Sold"
FROM {{T}} p
WHERE p."Spatiu" = 2 AND p."Produs" = '{prod}' AND p."Gestiune" = '{gest}'
  AND p."Data" <= DATE '2025-06-30'
GROUP BY p."Unitate"
HAVING COALESCE(sum(p."Cantitate"), 0.0) > 0.0
""".format(prod=PROD, gest=GEST)

Q["q10"] = """-- Q10 (cub) SAF-T PhysicalStock la 2025-12-31: per (Gestiune, Unitate=lot).
-- f0/q10.sql grupeaza pe (Repartitor, Lot, TipStoc) si reduce peste TipStoc in memorie;
-- pe cub reducerea e deja facuta de gran -- egalitatea se verifica pe acelasi gran.
SELECT p."Gestiune", p."Unitate" AS "LotId",
       COALESCE(sum(p."Cantitate"), 0.0) AS "Cantitate",
       COALESCE(sum(p."Valoare"), 0.0) AS "Valoare"
FROM {T} p
WHERE p."Spatiu" = 2 AND p."Data" <= DATE '2025-12-31'
GROUP BY p."Gestiune", p."Unitate"
HAVING COALESCE(sum(p."Cantitate"), 0.0) <> 0.0
"""

Q["q11"] = """-- Q11 (cub) SAF-T Customers/Suppliers 2025-06: conturile cu RolTert != 0,
-- per (Partener, Cont, Latura), sold initial la 2025-05-31 + rulajul lunii 06.
SELECT p."Partener", p."Cont", p."Latura",
       COALESCE(sum(CASE WHEN p."Data" <  DATE '2025-06-01' THEN p."Valoare" ELSE 0.0 END), 0.0) AS "Initial",
       COALESCE(sum(CASE WHEN p."Data" >= DATE '2025-06-01' THEN p."Valoare" ELSE 0.0 END), 0.0) AS "Rulaj"
FROM {{T}} p
WHERE p."Spatiu" = 1 AND p."Data" <= DATE '2025-06-30'
  AND p."Cont" = ANY ('{{{terti}}}')
GROUP BY p."Partener", p."Cont", p."Latura"
""".format(terti=TERTI)

Q["q11s"] = """-- Q11+S (cub) idem, cu soldul initial cladit din cub."Sold" la cea mai apropiata
-- granita <= 2025-05-31, adica 2024-12-31 (2025-05 nu e granita), plus postarile
-- 2025-01-01..2025-05-31. Asta e exact regula "ultimul an inchis + postarile de dupa".
SELECT u."Partener", u."Cont", u."Latura",
       COALESCE(sum(u."Ini"), 0.0) AS "Initial",
       COALESCE(sum(u."Rul"), 0.0) AS "Rulaj"
FROM (
  SELECT s."Partener", s."Cont", s."Latura", s."Valoare" AS "Ini", 0.0 AS "Rul"
  FROM cub."Sold" s
  WHERE s."Spatiu" = 1 AND s."Granita" = DATE '2024-12-31'
    AND s."Cont" = ANY ('{{{terti}}}')
  UNION ALL
  SELECT p."Partener", p."Cont", p."Latura", p."Valoare", 0.0
  FROM {{T}} p
  WHERE p."Spatiu" = 1 AND p."Data" >= DATE '2025-01-01' AND p."Data" <= DATE '2025-05-31'
    AND p."Cont" = ANY ('{{{terti}}}')
  UNION ALL
  SELECT p."Partener", p."Cont", p."Latura", 0.0, p."Valoare"
  FROM {{T}} p
  WHERE p."Spatiu" = 1 AND p."Data" >= DATE '2025-06-01' AND p."Data" <= DATE '2025-06-30'
    AND p."Cont" = ANY ('{{{terti}}}')
) u
GROUP BY u."Partener", u."Cont", u."Latura"
""".format(terti=TERTI)

Q["q12"] = """-- Q12 (cub) SAF-T GLE 2025-06, prima interogare: postarile contabile ale lunii,
-- cu tranzactia (Fel, Data) si documentul (Numar, ClrType).
SELECT p."ID", p."Data", p."TranzactieId", p."DocumentId", p."LinieId",
       p."Cont", p."Latura", p."Valoare", p."Partener", p."Gestiune", p."Produs",
       p."Unitate", p."CodFunctional", p."CodEconomic", p."SursaFinantare",
       p."UnitateOrganizatorica", p."Proiect", p."CentruCost",
       t."Fel", t."Data" AS "TranzactieData", t."ScrisLa",
       d."Numar" AS "DocumentNumar", d."ClrType" AS "DocumentTip"
FROM {T} p
JOIN cub."Tranzactie" t ON t."ID" = p."TranzactieId"
LEFT JOIN (SELECT dd."ID", dd."Numar", dd."ClrType" FROM "Documente" dd WHERE dd."GCRecord" = 0) d
       ON d."ID" = p."DocumentId"
WHERE p."Spatiu" = 1 AND p."Data" >= DATE '2025-06-01' AND p."Data" <= DATE '2025-06-30'
  AND p."DocumentId" IS NOT NULL
"""

Q["q12b"] = """-- Q12b (cub) a doua interogare a aceleiasi sectiuni: TaxInformation =
-- postarile Spatiu=3 ale ACELORASI LinieId (identitatea fiscala a liniei).
SELECT p."ID", p."LinieId", p."DocumentId", p."CodTvaId", p."RolTva", p."Valoare",
       p."PerioadaDeclarare", ct."Regim", ct."Cota", ct."Sens"
FROM {T} p
JOIN cub."CodTva" ct ON ct."ID" = p."CodTvaId"
WHERE p."Spatiu" = 3
  AND p."LinieId" IN (
    SELECT c."LinieId" FROM {T} c
    WHERE c."Spatiu" = 1 AND c."Data" >= DATE '2025-06-01' AND c."Data" <= DATE '2025-06-30'
      AND c."LinieId" IS NOT NULL)
"""

Q["q13"] = """-- Q13 (cub) Citirea pentru storno: TOATE postarile documentului, toate spatiile,
-- O SINGURA interogare (F0 are trei, cate una per registru).
-- ales: NotaContabila SED00000038 = documentul cu cele mai multe randuri contabile (228).
SELECT p.*
FROM {{T}} p
WHERE p."DocumentId" = '{doc}'
""".format(doc=DOC228)

for name, text in Q.items():
    with open(os.path.join(OUT, name + ".sql"), "w", encoding="utf-8", newline="\n") as fh:
        fh.write(text)
print("scrise:", " ".join(sorted(Q)))
