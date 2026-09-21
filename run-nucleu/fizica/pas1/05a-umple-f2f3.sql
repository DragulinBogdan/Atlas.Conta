-- Pas 1: umplerea f2 si f3 din f1 (aceleasi coloane, aceeasi ordine).
\set ON_ERROR_STOP on
\timing on
TRUNCATE f2."Postare";
TRUNCATE f3."Postare";
INSERT INTO f2."Postare" SELECT * FROM f1."Postare";
INSERT INTO f3."Postare" SELECT * FROM f1."Postare";
