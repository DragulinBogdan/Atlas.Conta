\set ON_ERROR_STOP on
ALTER TABLE f2."Postare_Contabil" DROP CONSTRAINT fk_c_cont, DROP CONSTRAINT fk_c_partener, DROP CONSTRAINT fk_c_gestiune, DROP CONSTRAINT fk_c_produs, DROP CONSTRAINT fk_c_unitate, DROP CONSTRAINT fk_c_tranzactie;
ALTER TABLE f2."Postare_Stoc"     DROP CONSTRAINT fk_s_gestiune, DROP CONSTRAINT fk_s_produs, DROP CONSTRAINT fk_s_unitate, DROP CONSTRAINT fk_s_tranzactie;
ALTER TABLE f2."Postare_Fiscal"   DROP CONSTRAINT fk_f_partener, DROP CONSTRAINT fk_f_codtva, DROP CONSTRAINT fk_f_tranzactie;
