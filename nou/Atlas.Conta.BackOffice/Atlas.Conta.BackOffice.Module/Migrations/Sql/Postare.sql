CREATE TABLE "Postare" (
    "ID" uuid NOT NULL,
    "Spatiu" smallint NOT NULL,
    "TranzactieId" uuid NOT NULL,
    "Ordinal" integer NOT NULL,
    "DocumentId" uuid NULL,
    "LinieId" uuid NULL,
    "Data" date NOT NULL,
    "Cont" uuid NOT NULL,
    "Sens" smallint NOT NULL,
    "Repartitor" uuid NULL,
    "Contrapondere" smallint NULL,
    "Produs" uuid NULL,
    "Unitate" uuid NULL,
    "UnitateDeschisa" date NULL,
    "FelUnitate" smallint NULL,
    "SuportId" uuid NULL,
    "SuportSpatiu" smallint NULL,
    "InversaDinId" uuid NULL,
    "InversaDinSpatiu" smallint NULL,
    "TipTvaId" uuid NULL,
    "SensTva" smallint NULL,
    "RolTva" smallint NULL,
    "PartenerFiscal" uuid NULL,
    "Loc" uuid NULL,
    "PerioadaDeclarare" integer NULL,
    "Valuta" uuid NULL,
    "Carte" smallint NOT NULL,
    "CodFunctional" uuid NULL,
    "CodEconomic" uuid NULL,
    "SursaFinantare" uuid NULL,
    "UnitateOrganizatorica" uuid NULL,
    "Proiect" uuid NULL,
    "CentruCost" uuid NULL,
    "Atribuit" uuid NULL,
    "Pereche" integer NULL,
    "Cantitate" numeric(18,3) NOT NULL,
    "ValoareValuta" numeric(18,2) NOT NULL,
    "Valoare" numeric(18,2) NOT NULL,
    "CotaTva" numeric(18,4) NULL,
    "DataDocument" date NULL,
    "DataExigibilitate" date NULL,
    "DataInregistrare" date NULL,
    "DataPrimire" date NULL,
    "DeImport" boolean NULL,
    "DocumentFiscalId" uuid NULL,
    "InversaTehnica" boolean NOT NULL DEFAULT false,
    "PerioadaD394" integer NULL,
    "RegimTva" smallint NULL,
    "RegularizareD300" boolean NOT NULL DEFAULT false,
    CONSTRAINT "PK_Postare" PRIMARY KEY ("Spatiu", "ID"),
    CONSTRAINT "CK_Postare_FelUnitate" CHECK (
        ("Unitate" IS NULL AND "FelUnitate" IS NULL AND "UnitateDeschisa" IS NULL)
        OR ("Unitate" IS NOT NULL AND "UnitateDeschisa" IS NOT NULL AND "FelUnitate" IS NOT NULL
            AND (("Spatiu" = 2 AND "FelUnitate" = 1)
                OR ("Spatiu" = 1 AND "FelUnitate" IN (2, 3))))),
    CONSTRAINT "CK_Postare_Referinte" CHECK (
        (("InversaDinId" IS NULL AND "InversaDinSpatiu" IS NULL)
            OR ("InversaDinId" IS NOT NULL AND "InversaDinSpatiu" IS NOT NULL AND "InversaDinSpatiu" IN (1, 2)))
        AND (("SuportId" IS NULL AND "SuportSpatiu" IS NULL)
            OR ("SuportId" IS NOT NULL AND "SuportSpatiu" IS NOT NULL AND "SuportSpatiu" IN (1, 2)))),
    CONSTRAINT "CK_Postare_Contrapondere" CHECK ("Contrapondere" IS NULL OR "Contrapondere" BETWEEN 1 AND 4),
    CONSTRAINT "CK_Postare_BlocFiscal" CHECK (
        ("TipTvaId" IS NULL AND "PartenerFiscal" IS NULL AND ("Loc" IS NULL OR "FelUnitate" IS NOT DISTINCT FROM 2))
        OR ("TipTvaId" IS NOT NULL AND "Loc" IS NOT NULL)),
    CONSTRAINT "CK_Postare_FiscalComplet" CHECK (
        "TipTvaId" IS NULL OR (
            "DocumentId" IS NOT NULL AND "LinieId" IS NOT NULL AND
            "SensTva" IS NOT NULL AND "RolTva" IS NOT NULL AND
            "RegimTva" IS NOT NULL AND "CotaTva" IS NOT NULL AND "DeImport" IS NOT NULL AND
            "DocumentFiscalId" IS NOT NULL AND "DataDocument" IS NOT NULL AND
            "DataExigibilitate" IS NOT NULL AND "DataInregistrare" IS NOT NULL AND
            "PerioadaDeclarare" IS NOT NULL AND "PerioadaD394" IS NOT NULL AND
            ("SensTva" <> 1 OR "DataPrimire" IS NOT NULL)))
) PARTITION BY LIST ("Spatiu");
CREATE TABLE "Postare_Contabil" PARTITION OF "Postare" FOR VALUES IN (1);
CREATE TABLE "Postare_Stoc" PARTITION OF "Postare" FOR VALUES IN (2);
ALTER TABLE "Postare_Contabil"
    ADD CONSTRAINT "FK_Postare_Contabil_Tranzactie_TranzactieId"
        FOREIGN KEY ("TranzactieId") REFERENCES "Tranzactie" ("ID"),
    ADD CONSTRAINT "FK_Postare_Contabil_Documente_DocumentId"
        FOREIGN KEY ("DocumentId") REFERENCES "Documente" ("ID"),
    ADD CONSTRAINT "FK_Postare_Contabil_Conturi_Cont"
        FOREIGN KEY ("Cont") REFERENCES "Conturi" ("ID"),
    ADD CONSTRAINT "FK_Postare_Contabil_Repartitori_Repartitor"
        FOREIGN KEY ("Repartitor") REFERENCES "Repartitori" ("ID"),
    ADD CONSTRAINT "FK_Postare_Contabil_Repartitori_PartenerFiscal"
        FOREIGN KEY ("PartenerFiscal") REFERENCES "Repartitori" ("ID"),
    ADD CONSTRAINT "FK_Postare_Contabil_Repartitori_Loc"
        FOREIGN KEY ("Loc") REFERENCES "Repartitori" ("ID"),
    ADD CONSTRAINT "FK_Postare_Contabil_Produse_Produs"
        FOREIGN KEY ("Produs") REFERENCES "Produse" ("ID");
ALTER TABLE "Postare_Stoc"
    ADD CONSTRAINT "FK_Postare_Stoc_Tranzactie_TranzactieId"
        FOREIGN KEY ("TranzactieId") REFERENCES "Tranzactie" ("ID"),
    ADD CONSTRAINT "FK_Postare_Stoc_Documente_DocumentId"
        FOREIGN KEY ("DocumentId") REFERENCES "Documente" ("ID"),
    ADD CONSTRAINT "FK_Postare_Stoc_Conturi_Cont"
        FOREIGN KEY ("Cont") REFERENCES "Conturi" ("ID"),
    ADD CONSTRAINT "FK_Postare_Stoc_Repartitori_Repartitor"
        FOREIGN KEY ("Repartitor") REFERENCES "Repartitori" ("ID"),
    ADD CONSTRAINT "FK_Postare_Stoc_Repartitori_PartenerFiscal"
        FOREIGN KEY ("PartenerFiscal") REFERENCES "Repartitori" ("ID"),
    ADD CONSTRAINT "FK_Postare_Stoc_Repartitori_Loc"
        FOREIGN KEY ("Loc") REFERENCES "Repartitori" ("ID"),
    ADD CONSTRAINT "FK_Postare_Stoc_Produse_Produs"
        FOREIGN KEY ("Produs") REFERENCES "Produse" ("ID"),
    ADD CONSTRAINT "FK_Postare_Stoc_Loturi_Unitate"
        FOREIGN KEY ("Unitate") REFERENCES "Loturi" ("ID");
CREATE INDEX "IX_Postare_Stoc_Produs_Data" ON "Postare_Stoc" ("Produs", "Data")
    INCLUDE ("Cantitate", "Valoare", "Repartitor", "Unitate");
CREATE INDEX "IX_Postare_Stoc_Data" ON "Postare_Stoc" ("Data");
CREATE INDEX "IX_Postare_Stoc_PerioadaDeclarare_TipTvaId" ON "Postare_Stoc"
    ("PerioadaDeclarare", "TipTvaId") WHERE "PerioadaDeclarare" IS NOT NULL;
CREATE INDEX "IX_Postare_Contabil_Repartitor_Cont_Data" ON "Postare_Contabil"
    ("Repartitor", "Cont", "Data") WHERE "Repartitor" IS NOT NULL;
CREATE INDEX "IX_Postare_Contabil_Data" ON "Postare_Contabil" ("Data");
CREATE INDEX "IX_Postare_Contabil_PerioadaDeclarare_TipTvaId" ON "Postare_Contabil"
    ("PerioadaDeclarare", "TipTvaId") WHERE "PerioadaDeclarare" IS NOT NULL;
CREATE INDEX "IX_Postare_DocumentId" ON "Postare" ("DocumentId");
CREATE INDEX "IX_Postare_Tranzactie_Pereche" ON "Postare" ("TranzactieId", "Pereche");
CREATE INDEX "IX_Postare_Suport" ON "Postare" ("SuportSpatiu", "SuportId") WHERE "SuportId" IS NOT NULL;
CREATE INDEX "IX_Postare_InversaDin" ON "Postare" ("InversaDinSpatiu", "InversaDinId") WHERE "InversaDinId" IS NOT NULL;
CREATE INDEX "IX_Postare_PerioadaD394" ON "Postare" ("PerioadaD394") WHERE "PerioadaD394" IS NOT NULL;
CREATE INDEX "IX_Postare_DataExigibilitate" ON "Postare" ("DataExigibilitate") WHERE "DataExigibilitate" IS NOT NULL;
CREATE INDEX "IX_Postare_Unitate_Data" ON "Postare" ("Unitate", "Data") WHERE "Unitate" IS NOT NULL;
CREATE INDEX "IX_Postare_Fisa_Cont" ON "Postare" ("Cont") WHERE "FelUnitate" = 3;
