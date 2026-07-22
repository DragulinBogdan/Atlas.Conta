CREATE TABLE TIPURI_DOC(ID_TIPURI_DOC INT IDENTITY(1,1), TIP_DOC VARCHAR(5), DENUMIRE VARCHAR(100))

INSERT INTO TIPURI_DOC(TIP_DOC, DENUMIRE) VALUES (
        'Cht','Chitanta de incasare NUMERAR'
)
INSERT INTO TIPURI_DOC(TIP_DOC, DENUMIRE) VALUES (
        'DcI','Dispozitie de INCASARE prin casierie'
)
INSERT INTO TIPURI_DOC(TIP_DOC, DENUMIRE) VALUES (
        'CEC','CEC de ridicare numerar din banca'
)
INSERT INTO TIPURI_DOC(TIP_DOC, DENUMIRE) VALUES (
        'FV','Foaie de varsamint ( depunere numerar in banca )'
)
INSERT INTO TIPURI_DOC(TIP_DOC, DENUMIRE) VALUES (
        'DcP','Dispozitie de PLATA prin casierie '
)
INSERT INTO TIPURI_DOC(TIP_DOC, DENUMIRE) VALUES (
        'Bon','Bon de cumparare ( emis de o alta firma ) achitat prin CASA'
)
INSERT INTO TIPURI_DOC(TIP_DOC, DENUMIRE) VALUES (
        'Fct','Factura emisa de o alta firma si platita cu NUMERAR prin CASA'
)
