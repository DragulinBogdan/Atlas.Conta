\echo == ReguliStoc (profil privat, baza Fizica.x1) ==
SELECT td."Cod" tip, rs."Latura", cp."Cod" clasa, rs."TipStoc", rs."Semn", rs."DinSeed"
FROM "ReguliStoc" rs JOIN "TipuriDocument" td ON td."ID"=rs."TipDocumentId"
LEFT JOIN "ClaseProduse" cp ON cp."ID"=rs."ClasaId" ORDER BY 1,2,4;
\echo == TipuriMaterial -> ContImplicit (cum se decide contul de stoc azi) ==
SELECT tm."Cod", tm."Denumire", cp."Cod" clasa, cp."Natura", c."Simbol" cont_implicit
FROM "TipuriMaterial" tm LEFT JOIN "ClaseProduse" cp ON cp."ID"=tm."ClasaId"
LEFT JOIN "Conturi" c ON c."ID"=tm."ContImplicitId" ORDER BY cp."Cod", tm."Cod";
\echo == ClaseProduse ==
SELECT "Cod","Denumire","Natura" FROM "ClaseProduse" ORDER BY 1;
