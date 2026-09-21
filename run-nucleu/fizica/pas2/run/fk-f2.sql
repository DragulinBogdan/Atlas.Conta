-- Pas 2 D: FK-urile NOT VALID pe f2, pentru costul lor la SCRIERE (comparabil cu cele
-- 24 de triggere FK ale F0). NOT VALID nu verifica randurile existente, dar VERIFICA
-- fiecare rand NOU -- exact ce masuram.
-- Coloana Unitate e POLIMORFA in cub (document pe Contabil, lot pe Stoc): pe f2 devine
-- FK-abila pentru ca partitia pe Spatiu separa cele doua intelesuri. Faptul se raporteaza.
\set ON_ERROR_STOP on
\timing on
ALTER TABLE f2."Postare_Contabil" ADD CONSTRAINT fk_c_cont      FOREIGN KEY ("Cont")         REFERENCES "Conturi"("ID")        NOT VALID;
ALTER TABLE f2."Postare_Contabil" ADD CONSTRAINT fk_c_partener  FOREIGN KEY ("Partener")     REFERENCES "Repartitori"("ID")    NOT VALID;
ALTER TABLE f2."Postare_Contabil" ADD CONSTRAINT fk_c_gestiune  FOREIGN KEY ("Gestiune")     REFERENCES "Repartitori"("ID")    NOT VALID;
ALTER TABLE f2."Postare_Contabil" ADD CONSTRAINT fk_c_produs    FOREIGN KEY ("Produs")       REFERENCES "Produse"("ID")        NOT VALID;
ALTER TABLE f2."Postare_Contabil" ADD CONSTRAINT fk_c_unitate   FOREIGN KEY ("Unitate")      REFERENCES "Documente"("ID")      NOT VALID;
ALTER TABLE f2."Postare_Contabil" ADD CONSTRAINT fk_c_tranzactie FOREIGN KEY ("TranzactieId") REFERENCES cub."Tranzactie"("ID") NOT VALID;
ALTER TABLE f2."Postare_Stoc"     ADD CONSTRAINT fk_s_gestiune  FOREIGN KEY ("Gestiune")     REFERENCES "Repartitori"("ID")    NOT VALID;
ALTER TABLE f2."Postare_Stoc"     ADD CONSTRAINT fk_s_produs    FOREIGN KEY ("Produs")       REFERENCES "Produse"("ID")        NOT VALID;
ALTER TABLE f2."Postare_Stoc"     ADD CONSTRAINT fk_s_unitate   FOREIGN KEY ("Unitate")      REFERENCES "Loturi"("ID")         NOT VALID;
ALTER TABLE f2."Postare_Stoc"     ADD CONSTRAINT fk_s_tranzactie FOREIGN KEY ("TranzactieId") REFERENCES cub."Tranzactie"("ID") NOT VALID;
ALTER TABLE f2."Postare_Fiscal"   ADD CONSTRAINT fk_f_partener  FOREIGN KEY ("Partener")     REFERENCES "Repartitori"("ID")    NOT VALID;
ALTER TABLE f2."Postare_Fiscal"   ADD CONSTRAINT fk_f_codtva    FOREIGN KEY ("CodTvaId")     REFERENCES cub."CodTva"("ID")     NOT VALID;
ALTER TABLE f2."Postare_Fiscal"   ADD CONSTRAINT fk_f_tranzactie FOREIGN KEY ("TranzactieId") REFERENCES cub."Tranzactie"("ID") NOT VALID;
