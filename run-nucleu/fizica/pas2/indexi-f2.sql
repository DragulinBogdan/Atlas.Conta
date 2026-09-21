-- Pas 2: indexii per uz pentru f2 si f3. Justificarea: pas2/indexi-f2.md.
-- Se creeaza O DATA, inaintea masuratorilor. Identitatea (PK, TranzactieId,
-- DocumentId, LinieId) exista din pasul 1 si nu intra in bugetul de 6.
\set ON_ERROR_STOP on
\timing on

-- ---------- f2 (LIST pe Spatiu): index pe fiecare partitie ----------
CREATE INDEX ix_f2c_cont_data ON f2."Postare_Contabil"("Cont","Data")
  INCLUDE ("Latura","Valoare","Partener","Unitate");
CREATE INDEX ix_f2c_partener ON f2."Postare_Contabil"("Partener","Cont","Data")
  WHERE "Partener" IS NOT NULL;
CREATE INDEX ix_f2c_data ON f2."Postare_Contabil"("Data");

CREATE INDEX ix_f2s_gest_unit_data ON f2."Postare_Stoc"("Gestiune","Unitate","Data")
  INCLUDE ("Cantitate","Valoare");
CREATE INDEX ix_f2s_produs_data ON f2."Postare_Stoc"("Produs","Data")
  INCLUDE ("Cantitate","Valoare","Gestiune","Unitate");

CREATE INDEX ix_f2f_perioada_codtva ON f2."Postare_Fiscal"("PerioadaDeclarare","CodTvaId")
  INCLUDE ("RolTva","Valoare","Partener","DocumentId","TranzactieId","LinieId");

-- ---------- f3 (LIST x RANGE): acelasi set, pe partitia-parinte de spatiu ----------
CREATE INDEX ix_f3c_cont_data ON f3."Postare_Contabil"("Cont","Data")
  INCLUDE ("Latura","Valoare","Partener","Unitate");
CREATE INDEX ix_f3c_partener ON f3."Postare_Contabil"("Partener","Cont","Data")
  WHERE "Partener" IS NOT NULL;
CREATE INDEX ix_f3c_data ON f3."Postare_Contabil"("Data");

CREATE INDEX ix_f3s_gest_unit_data ON f3."Postare_Stoc"("Gestiune","Unitate","Data")
  INCLUDE ("Cantitate","Valoare");
CREATE INDEX ix_f3s_produs_data ON f3."Postare_Stoc"("Produs","Data")
  INCLUDE ("Cantitate","Valoare","Gestiune","Unitate");

CREATE INDEX ix_f3f_perioada_codtva ON f3."Postare_Fiscal"("PerioadaDeclarare","CodTvaId")
  INCLUDE ("RolTva","Valoare","Partener","DocumentId","TranzactieId","LinieId");
