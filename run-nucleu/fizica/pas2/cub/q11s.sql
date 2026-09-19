-- Q11+S (cub) idem, cu soldul initial cladit din cub."Sold" la cea mai apropiata
-- granita <= 2025-05-31, adica 2024-12-31 (2025-05 nu e granita), plus postarile
-- 2025-01-01..2025-05-31. Asta e exact regula "ultimul an inchis + postarile de dupa".
SELECT u."Partener", u."Cont", u."Latura",
       COALESCE(sum(u."Ini"), 0.0) AS "Initial",
       COALESCE(sum(u."Rul"), 0.0) AS "Rulaj"
FROM (
  SELECT s."Partener", s."Cont", s."Latura", s."Valoare" AS "Ini", 0.0 AS "Rul"
  FROM cub."Sold" s
  WHERE s."Spatiu" = 1 AND s."Granita" = DATE '2024-12-31'
    AND s."Cont" = ANY ('{01a0b48c-a8aa-7e36-8f14-4fa2fa4ea009,01a0b48c-a8ab-7b50-b3b7-071a2b7a349b,01a0b48c-a8ac-7e4b-a6f0-9387b76e96eb,01a0b48c-a8ac-7ab5-a52e-152a7dc2545f,01a0b48c-a8ad-7886-bf9d-1ef6ec9b9d85,01a0b48c-a8ad-738e-9300-b9812b654f5d,01a0b48c-a8ae-7e28-ab0c-d459207dc577,01a0b48c-a8af-76f6-8adc-75a198ef6a63,01a0b48c-a8af-70d2-ad7b-1d1f3709b4fe,01a0b48c-a8b0-734d-a49d-507ee62971ef,01a0b48c-a8b1-7d53-a2d6-b11eb0cc58ed,01a0b48c-a8b2-73ce-a6cd-a045b80364a8,01a0b48c-a8b2-7841-9d84-1c268bdb59e3,01a0b48c-a8b3-7212-9f12-a75dc52b91c2,01a0b48c-a8b3-70d2-85c6-14793081762f,01a0b48c-a8b4-75ad-a6f5-0403d7678e18}')
  UNION ALL
  SELECT p."Partener", p."Cont", p."Latura", p."Valoare", 0.0
  FROM {T} p
  WHERE p."Spatiu" = 1 AND p."Data" >= DATE '2025-01-01' AND p."Data" <= DATE '2025-05-31'
    AND p."Cont" = ANY ('{01a0b48c-a8aa-7e36-8f14-4fa2fa4ea009,01a0b48c-a8ab-7b50-b3b7-071a2b7a349b,01a0b48c-a8ac-7e4b-a6f0-9387b76e96eb,01a0b48c-a8ac-7ab5-a52e-152a7dc2545f,01a0b48c-a8ad-7886-bf9d-1ef6ec9b9d85,01a0b48c-a8ad-738e-9300-b9812b654f5d,01a0b48c-a8ae-7e28-ab0c-d459207dc577,01a0b48c-a8af-76f6-8adc-75a198ef6a63,01a0b48c-a8af-70d2-ad7b-1d1f3709b4fe,01a0b48c-a8b0-734d-a49d-507ee62971ef,01a0b48c-a8b1-7d53-a2d6-b11eb0cc58ed,01a0b48c-a8b2-73ce-a6cd-a045b80364a8,01a0b48c-a8b2-7841-9d84-1c268bdb59e3,01a0b48c-a8b3-7212-9f12-a75dc52b91c2,01a0b48c-a8b3-70d2-85c6-14793081762f,01a0b48c-a8b4-75ad-a6f5-0403d7678e18}')
  UNION ALL
  SELECT p."Partener", p."Cont", p."Latura", 0.0, p."Valoare"
  FROM {T} p
  WHERE p."Spatiu" = 1 AND p."Data" >= DATE '2025-06-01' AND p."Data" <= DATE '2025-06-30'
    AND p."Cont" = ANY ('{01a0b48c-a8aa-7e36-8f14-4fa2fa4ea009,01a0b48c-a8ab-7b50-b3b7-071a2b7a349b,01a0b48c-a8ac-7e4b-a6f0-9387b76e96eb,01a0b48c-a8ac-7ab5-a52e-152a7dc2545f,01a0b48c-a8ad-7886-bf9d-1ef6ec9b9d85,01a0b48c-a8ad-738e-9300-b9812b654f5d,01a0b48c-a8ae-7e28-ab0c-d459207dc577,01a0b48c-a8af-76f6-8adc-75a198ef6a63,01a0b48c-a8af-70d2-ad7b-1d1f3709b4fe,01a0b48c-a8b0-734d-a49d-507ee62971ef,01a0b48c-a8b1-7d53-a2d6-b11eb0cc58ed,01a0b48c-a8b2-73ce-a6cd-a045b80364a8,01a0b48c-a8b2-7841-9d84-1c268bdb59e3,01a0b48c-a8b3-7212-9f12-a75dc52b91c2,01a0b48c-a8b3-70d2-85c6-14793081762f,01a0b48c-a8b4-75ad-a6f5-0403d7678e18}')
) u
GROUP BY u."Partener", u."Cont", u."Latura"
