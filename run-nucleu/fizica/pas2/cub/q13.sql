-- Q13 (cub) Citirea pentru storno: TOATE postarile documentului, toate spatiile,
-- O SINGURA interogare (F0 are trei, cate una per registru).
-- ales: NotaContabila SED00000038 = documentul cu cele mai multe randuri contabile (228).
SELECT p.*
FROM {T} p
WHERE p."DocumentId" = '01a0b494-a2da-7484-94ce-1019f56c4fef'
