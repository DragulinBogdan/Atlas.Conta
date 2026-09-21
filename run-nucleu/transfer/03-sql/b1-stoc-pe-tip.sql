-- B1: RegistruStoc pe (tip document, TipStoc, semnul cantitatii).
-- Semn = sign(Cantitate); rand cu Cantitate=0 => semn 0.
-- Documentele lipsa (deschideri, DocumentId NULL) apar ca '(deschidere)'.
SELECT COALESCE(d."ClrType",'(deschidere)') AS tip,
       rs."TipStoc",
       CASE WHEN rs."Cantitate">0 THEN 1 WHEN rs."Cantitate"<0 THEN -1 ELSE 0 END AS semn,
       count(*) AS randuri,
       sum(rs."Cantitate") AS suma_cantitate,
       sum(rs."Valoare")   AS suma_valoare,
       count(*) FILTER (WHERE rs."Storno") AS storno
FROM "RegistruStoc" rs
LEFT JOIN "Documente" d ON d."ID"=rs."DocumentId"
GROUP BY 1,2,3
ORDER BY 1,2,3;
