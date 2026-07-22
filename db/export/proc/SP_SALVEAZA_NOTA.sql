CREATE PROCEDURE SP_SALVEAZA_NOTA (@COD INT, @ID_UTILIZATORI INT) AS
BEGIN
  SET NOCOUNT ON
  SELECT * INTO #NOTA_CONTABILA FROM CITEMS WHERE ID_UTILIZATORI = @ID_UTILIZATORI

  update #NOTA_CONTABILA set 
        contc = case when contd like '8%' then null else contc end,  
        contd = case when contc like'8%' then null else contd end
  where 
      contd like '8%' or contc like '8%'  
  update #NOTA_CONTABILA set 
        cont_cred = case when cont_debt like '8%' then 'X' else cont_cred end,  
        cont_debt = case when cont_cred like '8%' then 'X' else cont_debt end
  where 
      cont_debt like '8%' or cont_cred like '8%'  

  INSERT INTO CNOTE (COD, POZ, JURNAL, NRDOC, DATA, EXPLICATIE, VALOARE, AC, ECL, MODUL, C_O, BUGET, COMPUSA, CONTD, CONTC, CONT_DEBT, CONT_CRED, ID_INITIAL, ID_PARINTE,  STARE, DATA_OPERARE, ID_ANGAJAMENTE_DEFALCARE, COD_FUNCTIONAL, COD_ECONOMIC, REPARTITOR_DEBIT, REPARTITOR_CREDIT,
    NR_OP, DATA_OP, CODGEST, TIP_DOCUMENT, ID_ORDIN_PLATA,  NR_CONTRACT, DATA_CONTRACT, ID_ANALITIC, cod_document, nr_document, data_document, data_scadenta,
    DOCUMENT)  SELECT @COD AS COD, POZ, JURNAL, NRDOC, CONVERT(DATETIME, FLOOR(CONVERT(FLOAT, DATA)))  as DATA, ISNULL(EXPLICATIE, ''), VALOARE, AC, ECL, MODUL, @ID_UTILIZATORI AS C_O, BUGET, COMPUSA, 
      ltrim(rtrim(CONTD)) as contd, ltrim(rtrim(CONTC)) as contc, ltrim(rtrim(CONT_DEBT)) as cont_debt, ltrim(rtrim(CONT_CRED)) as cont_cred, COD AS ID_INITIAL, ID_CITEMS AS ID_PARINTE, 1, GETDATE(), ID_ANGAJAMENTE_DEFALCARE, COD_FUNCTIONAL, COD_ECONOMIC, REPARTITOR_DEBIT, REPARTITOR_CREDIT,
     NR_OP,DATA_OP, CODGEST, TIP_DOCUMENT,ID_ORDIN_PLATA, NR_CONTRACT, DATA_CONTRACT, ID_ANALITIC, cod_document, nr_document, data_document, data_scadenta,
     isnull(cod_document + ', ', '') + isnull(nr_document + ', ', '') + isnull(CONVERT(VARCHAR(10), data_document, 103), '') as Document
  FROM #NOTA_CONTABILA ORDER BY ID_CITEMS

  SELECT A.NR AS ID, B.ID_CITEMS, B.ID_PARINTE INTO #LST_CODURI FROM CNOTE A JOIN #NOTA_CONTABILA B ON (A.ID_PARINTE = B.ID_CITEMS)
    WHERE A.COD = @COD AND ID_UTILIZATORI = @ID_UTILIZATORI

  UPDATE CNOTE SET ID_PARINTE = C.ID
    FROM CNOTE A JOIN #LST_CODURI B ON (A.NR = B.ID)
         LEFT JOIN #LST_CODURI C ON (C.ID_CITEMS = B.ID_PARINTE)
    WHERE A.COD = @COD

  DELETE FROM CITEMS WHERE ID_UTILIZATORI = @ID_UTILIZATORI
  SET NOCOUNT OFF
END



