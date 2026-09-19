-- Q13 Citirea unui document pentru STORNO: cele 3 registre ale lui
-- Oglindeste: MotorOperare.Storneaza, Motor/MotorOperare.cs:639-641 -- TREI interogari
--   separate, fiecare `.ToList()`, care materializeaza ENTITATI (toate coloanele, sub
--   UseChangeTrackingProxies), nu proiectii.
-- Filtre copiate din cod: DOAR `r.DocumentId == doc.ID`. GCRecord = 0 vine de la
--   HasQueryFilter (calea LINQ). NU se filtreaza Storno: un document deja stornat e refuzat
--   mai devreme, prin `doc.Stare != Operat` (:628).
-- Al patrulea registru (RegistruImobilizari) se citeste doar daca documentul implementeaza
--   IDocumentCuRegistruPropriu -- :694-695 (StorneazaRegistrul).
-- ales: 01a0b494-a2da-7484-94ce-1019f56c4fef = NotaContabila SED00000038, documentul cu CELE
--   MAI MULTE randuri contabile din baza (228). Are 0 randuri de stoc si 0 fiscale.
--   (Documentul cu cel mai mare TOTAL pe cele trei registre e altul: BonConsum BCS-237,
--   01a0b4b1-b2d1-7abc-9d71-b4f433c41ee0, cu 97 contabile + 194 de stoc.)
-- CE MAI FACE STORNAREA inainte de aceste trei citiri, si nu e SQL de raport:
--   GardianPerioada.VerificaDeschisa, VerificaFaraLaturaPerecheOperata,
--   VerificaFaraConexeOperate, ImperechereService.InverseazaLaStorno,
--   StergeConexeDraftAutogenerate, StocService.VerificaSoldIntermediar -- :631-644.

SELECT r."ID", r."Data", r."TipStoc", r."LotId", r."RepartitorId", r."Cantitate", r."Valoare",
       r."Storno", r."DocumentId", r."DetaliuId", r."GCRecord", r."OptimisticLockField"
FROM "RegistruStoc" AS r
WHERE r."GCRecord" = 0 AND r."DocumentId" = '01a0b494-a2da-7484-94ce-1019f56c4fef';

SELECT r.*
FROM "RegistruContabil" AS r
WHERE r."GCRecord" = 0 AND r."DocumentId" = '01a0b494-a2da-7484-94ce-1019f56c4fef';

SELECT r.*
FROM "RegistruTva" AS r
WHERE r."GCRecord" = 0 AND r."DocumentId" = '01a0b494-a2da-7484-94ce-1019f56c4fef';
