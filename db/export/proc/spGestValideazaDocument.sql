create PROCEDURE [dbo].[spGestValideazaDocument] 
--  adaugat idGestTipDocum pentru a permite sa nu fie completat campul detalii angajament pe factura de iesire 
  (@id_culgest_docum INT, @idUser int = null, @isTemp bit = null) 
AS
BEGIN

  DECLARE @idGestDefaDocum int
  SET @idGestDefaDocum  = COALESCE((SELECT TOP 1 id_Gest_defa_docum FROM culgest_docum WHERE id_culgest_docum = @ID_CULGEST_DOCUM), -1)
  SET @idUser           = COALESCE(@idUser, (SELECT TOP 1 id_utilizatori FROM culgest_docum WHERE id_culgest_docum = @ID_CULGEST_DOCUM))
        
  SET NOCOUNT ON

  /* ADAUGAM IN GEST_DOCUM */
  DECLARE @nr_chitanta int
  DECLARE @data_chitanta datetime
  DECLARE @tethysID varchar(36)

  DECLARE @new_id_docum INT
  DECLARE @idDefaDocum  int
  DECLARE @idInitial    int
  --ID_DOCUMENT_CONEX - IDENTIFICATOR UNIC AL UNUI DOCUMENT
  --ID_TRANZACTIE - IDENTIFICATOR IN CADRUL UNUI GRUP DE DOCUMENTE AL DOCUMENTELOR

  SELECT 
    @idInitial = id_modificare, 
    @idDefaDocum = id_gest_defa_docum, 
    @tethysID = TethysId 
  FROM culgest_docum 
  WHERE id_culgest_docum = @id_culgest_docum

  DECLARE @clasificatieObligatorie  bit
  DECLARE @descDocum                nvarchar(128)
  DECLARE @idGestTipDocum           int   -- << nou >>

  SELECT
    @clasificatieObligatorie  = clasificatieObligatorie,
    @descDocum                = den_docum,
    @idGestTipDocum           = b.id_gest_tip_docum   -- << nou >>
  FROM gest_defa_docum a
       JOIN gest_tip_docum b ON (a.id_gest_tip_docum = b.id_gest_tip_docum)
  WHERE a.id_gest_defa_docum = @idDefaDocum

  IF @clasificatieObligatorie = 1
  BEGIN
    IF @idGestTipDocum <> 12
       AND EXISTS (
             SELECT TOP 1 1 
             FROM culgest_itemsi 
             WHERE id_culgest_docum = @id_culgest_docum 
               AND id_angajamente_defalcare IS NULL 
               AND cod_economic IS NULL
           )
    BEGIN
      RAISERROR('Eroare : pentru tipul de document %s trebuie selectat angajamentul legal sau global', 2, 1, @descDocum)
      RETURN
    END
  END

  IF EXISTS (
       SELECT TOP 1 1 
       FROM culgest_docum a 
       LEFT JOIN culgest_itemsi b ON a.id_culgest_docum = b.id_culgest_docum 
       WHERE b.id_culgest_docum = @id_culgest_docum 
         AND cantitate < 0 
         AND produs = 'M' 
         AND id_gest_tip_docum = 2
     )
  BEGIN
    RAISERROR('Eroare: Nu puteti introduce pozitii cu minus pentru cantitate pe acest tip de produs!', 17, 1, @descDocum)
    RETURN
  END

  IF ISNULL((SELECT TOP 1 se_gen_chitanta FROM culgest_docum WHERE id_culgest_docum = @id_culgest_docum), 0) = 1 
  BEGIN
    SELECT TOP 1 
      @nr_chitanta = nr_chitanta, 
      @data_chitanta = data_chitanta 
    FROM culgest_docum 
    WHERE id_culgest_docum = @id_culgest_docum

    IF @nr_chitanta IS NULL 
    BEGIN
      SET @nr_chitanta = ISNULL(@nr_chitanta, (SELECT ISNULL(MAX(nr_chitanta), 2000) FROM gest_docum WHERE stare = 1) + 1)
      --exec sp_get_next_value_var 'GEST_CHITANTA', @nr_chitanta output
      SET @data_chitanta = COALESCE(@data_chitanta, (SELECT TOP 1 data_docum FROM culgest_docum WHERE id_culgest_docum = @id_culgest_docum), GETDATE())
    END
  END

  -- validat       - se citeste din gest_defa_docum daca se merge pe autovalidarea documentului sau se valideaza de altcineva cand VALIDAT= (null sau 0) in gest_defa_docum
  SELECT 
    validat       = (SELECT TOP 1 CASE WHEN ISNULL(aa.auto_validare_emitent, 0) = 1 THEN 1 ELSE 0 END FROM gest_defa_Docum aa WHERE aa.id_gest_defa_docum = a.id_gest_defa_Docum),
    stare         = 1, 
    data_operare  = GETDATE(),
    id_gest_docum = CONVERT(int, NULL), 
    *
  INTO #culgest_docum
  FROM culgest_docum a
  WHERE id_culgest_docum = @id_culgest_docum

  UPDATE a SET 
    AUTOGENERAT   = 0, 
    NR_CHITANTA   = @nr_chitanta,
    DATA_CHITANTA = @data_chitanta,
    NR_NOTA       = ISNULL(
                       CASE 
                         WHEN a.id_utilizatori = 402 
                           THEN LTRIM(RTRIM(STR(MONTH(ISNULL(DATA_NOTA, DATA_DOCUM))))) + '/MAG' 
                         ELSE NULL 
                       END, 
                       NR_NOTA
                     )
  FROM #CULGEST_DOCUM a

  --facem salvarea fara sa tinem cont de structura de date care poate fi diferita de la client la client

  DECLARE @name varchar(200)
  DECLARE @sql nvarchar(4000)
        
  SET @sql = NULL
  SELECT
    @sql = COALESCE(@sql + ', ', '') + name
  FROM syscolumns a
  WHERE id = OBJECT_ID('GEST_DOCUM')
        AND iscomputed    = 0               -- calculat
        AND xtype         NOT IN (189)      -- timestamp
        AND status & 0x80 <> 0x80           -- identity
        AND EXISTS(SELECT TOP 1 1 FROM tempdb..syscolumns b WHERE id = OBJECT_ID('tempdb..#CULGEST_DOCUM') AND a.name = b.name)
        
  SET @sql = 'INSERT INTO gest_docum(' + @sql + ')' + CHAR(13) + CHAR(10) 
           + 'SELECT ' + @sql + ' FROM #CULGEST_DOCUM ' + CHAR(13) + CHAR(10) 
           + ' SELECT @NEW_ID_DOCUM = SCOPE_IDENTITY()'
  EXEC sp_executesql @sql, N'@NEW_ID_DOCUM int OUTPUT', @NEW_ID_DOCUM OUTPUT

  IF ISNULL(@tethysID, '') <> '' 
    INSERT INTO TethysDOCUMENTE (id_gest_docum, TT_Id, stare ) 
    VALUES(@NEW_ID_DOCUM, @tethysID, 1)

  UPDATE GEST_DOCUM SET 
      ID_MODIFICARE     = ISNULL(ID_MODIFICARE, @NEW_ID_DOCUM),
      ID_DOCUMENT_CONEX = ISNULL(ID_DOCUMENT_CONEX, @NEW_ID_DOCUM),
      ID_INITIAL        = ISNULL(ID_INITIAL, @NEW_ID_DOCUM),
      ID_TRANZACTIE     = ISNULL(ID_TRANZACTIE, @NEW_ID_DOCUM)
  WHERE ID_GEST_DOCUM = @new_id_docum

  /* defalcarea codului sumator o facem direct in culgest_itemsi
     o sa apara la culegere defalcat, astfel incat sa avem pret-ul de livrare pentru fiecare pozitie separat 
     in cazul bonului de consum oricum ne trebuie lucrul acesta */

  IF dbo.fnParamSoc(1, 'tipStock', 'S') = 'S'
    EXEC spSpargeCodSumCulegere @id_culgest_docum

  SELECT id_gest_docum = @new_id_docum, * 
  INTO #culgest_itemsi 
  FROM culgest_itemsi 
  WHERE id_culgest_docum = @id_culgest_docum
        
  UPDATE #culgest_itemsi 
  SET stare  = 1

  DECLARE @fieldFromList varchar(max)
  DECLARE @fieldToList   varchar(max)

  SELECT
    @fieldFromList  = COALESCE(@fieldFromList + ', ', '') + 'a.'+name,
    @fieldToList    = COALESCE(@fieldToList + ', ', '') + name
  FROM syscolumns a
  WHERE id = OBJECT_ID('GEST_ITEMSI')
        AND iscomputed      = 0           -- calculat
        AND xtype           NOT IN (189)  -- timestamp
        AND status & 0x80   <> 0x80       -- identity
        AND EXISTS (SELECT TOP 1 1 FROM tempdb..syscolumns b WHERE id = OBJECT_ID('tempdb..#CULGEST_ITEMSI') AND a.name = b.name)

  CREATE TABLE #mapare_itemsi (id_gest_itemsi int, id_culgest_itemsi int)

  SET @sql = 'MERGE gest_itemsi USING #culgest_itemsi AS a ON (1=0) ' +
             'WHEN NOT MATCHED BY TARGET THEN INSERT ('+@fieldToList+') VALUES ('+@fieldFromList+')'
  SET @sql = @sql + ' OUTPUT inserted.id_gest_itemsi, a.id_culgest_itemsi INTO #mapare_itemsi (id_gest_itemsi, id_culgest_itemsi);'
  --PRINT @sql
  EXEC (@sql)

  -- copiem si procentii pentru istoric
  INSERT INTO gest_itemsi_procent (
    id_gest_docum, id_gest_itemsi, id_culgest_itemsi, id_angajamente_defalcare, codmat, 
    procProcent, sumaTotalaProcent, cantitateProcent, descAngajament, valFacturare,
    codFunctionalProcent, codEconomicProcent, id_oi_proiecte, id_oi_unitati
  )
  SELECT
    a.id_gest_docum, a.id_gest_itemsi, c.id_culgest_itemsi, c.id_angajamente_defalcare, a.codmat, 
    c.procProcent, c.sumaTotalaProcent, c.cantitateProcent, c.descAngajament, c.valFacturare,
    c.codFunctionalProcent, c.codEconomicProcent, c.id_oi_proiecte, c.id_oi_unitati
  FROM gest_itemsi a
       JOIN #mapare_itemsi          b ON (a.id_gest_itemsi = b.id_gest_itemsi)
       JOIN culgest_itemsi_procent  c ON (c.id_culgest_itemsi = b.id_culgest_itemsi)
  WHERE a.id_gest_docum = @new_id_docum

  UPDATE c SET 
    c.id_document_intrare = ISNULL(c.id_document_intrare,  a.id_gest_docum), 
    c.data_cod            = ISNULL(c.data_cod, a.data_docum), 
    c.GestIntrare         = ISNULL(c.GestIntrare, a.id_Primitor)
  FROM 
    gest_docum a 
    JOIN gest_itemsi  b ON (a.id_gest_docum = b.id_gest_docum) 
    JOIN gest_gnmcl   c ON (b.codmat = c.codmat)
  WHERE a.stare             = 1 
        AND b.stare         = 1 
        AND a.id_gest_docum = @new_id_docum
        AND (
              c.id_document_intrare IS NULL 
              OR NOT EXISTS(SELECT TOP 1 1 FROM gest_docum WHERE stare = 1 AND id_Gest_docum = c.id_document_intrare)
            )

  /*inseram in casa banca inregistrare daca este cazul*/
  DECLARE @DECONT_COD_CB INT
  DECLARE @DECONT_NRDOC VARCHAR(10)
  DECLARE @DECONT_DATA DATETIME
  DECLARE @DECONT_TIPDOC VARCHAR(3)
  DECLARE @DECONT_GENERATE BIT
  DECLARE @NEW_COD INT
  DECLARE @DECONT_CODGEST INT
  DECLARE @DECONT_EXPLICATIE VARCHAR(80)
  DECLARE @DECONT_VALOARE MONEY
  DECLARE @DECONT_CONT VARCHAR(50)
  DECLARE @DECONT_C_O INT
  DECLARE @DECONT_ID INT
  ------    
  DECLARE @cod_functional VARCHAR(50)    
  DECLARE @cod_economic VARCHAR(50)    
  DECLARE @id_oi_proiecte INT    
  DECLARE @id_oi_unitati INT    
  DECLARE @ID_BREG_p INT    
       
  SET @DECONT_VALOARE = ISNULL((SELECT SUM(VALOARE_RECEPTIE_TVA) FROM GEST_ITEMSI WHERE ID_GEST_DOCUM = @NEW_ID_DOCUM), 0)
  SELECT 
    @DECONT_GENERATE = DECONT_GENERATE, 
    @DECONT_COD_CB   = DECONT_COD_CB,  
    @DECONT_NRDOC    = DECONT_NRDOC, 
    @DECONT_DATA     = DECONT_DATA, 
    @DECONT_TIPDOC   = DECONT_TIPDOC
  FROM 
    CULGEST_DOCUM 
  WHERE ID_CULGEST_DOCUM = @ID_CULGEST_DOCUM

  SET @DECONT_CONT = (
    SELECT TOP 1 B.CONT_CREDITOR  
    FROM GEST_DOCUM A 
      JOIN GEST_DEFA_NOTA_CONT B ON (A.ID_GEST_DEFA_DOCUM = B.ID_GEST_DEFA_DOCUM)
      JOIN GEST_ITEMSI C ON (A.ID_GEST_DOCUM = C.ID_GEST_DOCUM)
    WHERE A.ID_GEST_DEFA_DOCUM = @idGestDefaDocum 
    ORDER BY CASE WHEN B.CONT_CREDITOR LIKE '4%' THEN 0 ELSE 1 END
  )
      
  SET @ID_BREG_p = (SELECT MAX(ID_BREG_P) + 1 FROM breg_p)
      
  SELECT TOP 1 
    @DECONT_EXPLICATIE = 'PLATA ' + B.NUME 
                         + ISNULL(' ' + (SELECT TOP 1 COD_DOCUM FROM GEST_TIP_DOCUM WHERE ID_GEST_TIP_DOCUM = A.ID_GEST_TIP_DOCUM) + ' ', '') 
                         + ISNULL(A.NR_DOCUM + ' ', '') 
                         + ISNULL('din ' + CONVERT(VARCHAR(10), DATA_DOCUM, 103), ''), 
    @DECONT_CODGEST =  A.ID_PREDATOR, 
    @DECONT_CONT    = @DECONT_CONT,
    @DECONT_C_O     = A.ID_UTILIZATORI,
    @cod_functional = c.cod_functional,    
    @cod_economic   = cod_economic,    
    @id_oi_proiecte = id_oi_proiecte,    
    @id_oi_unitati  = id_oi_unitati
  FROM 
    GEST_DOCUM A 
    JOIN REPARTITORI B ON (A.ID_PREDATOR = B.ID_REPARTITORI)
    JOIN GEST_ITEMSI  c ON a.id_gest_docum = c.id_gest_docum
  WHERE a.ID_GEST_DOCUM = @NEW_ID_DOCUM

  IF (ISNULL(@DECONT_GENERATE, 0) = 1) AND  (@DECONT_VALOARE <> 0) 
  BEGIN
    --NOU ID
    EXEC SP_GET_NEXT_VALUE_VAR 'BREGISTRU', @NEW_COD OUTPUT
    --INSERAM IN CASA BANCA
    INSERT INTO BREGISTRU(
      COD, COD_CB, DATA, TIPDOC, NRDOC, POZ, EXPLICATIE, INCASARI, PLATI, CONT_CSP, DATAEM, C_O, ECL, CODGEST
    )
    SELECT 
      @NEW_COD, @DECONT_COD_CB, @DECONT_DATA, @DECONT_TIPDOC, @DECONT_NRDOC, 1, 
      @DECONT_EXPLICATIE, NULL, @DECONT_VALOARE, @DECONT_CONT, GETDATE(), @DECONT_C_O, 1 AS ECL, @DECONT_CODGEST

    --INSERAM IN DEFALCAREA DE LA CASA BANCA
    INSERT INTO breg_p (
      breg_cod, valoare, C_O, ID_BREG_p, poz, cod_functional, cod_economic, cont_csp, id_oi_proiecte, id_oi_unitati
    )
    SELECT 
      @NEW_COD, @DECONT_VALOARE, @DECONT_C_O, @ID_BREG_p, 1, 
      @cod_functional, @cod_economic, @DECONT_CONT, @id_oi_proiecte, @id_oi_unitati

    --INSERAM IN DECONTARI
    INSERT INTO GEST_DECONTARI(ID_GEST_DOCUM, ID_BREGISTRU, SUMA, AUTOGENERAT) 
    VALUES(@NEW_ID_DOCUM, @NEW_COD, @DECONT_VALOARE, 1)

    SELECT @DECONT_ID = SCOPE_IDENTITY()

    --INSERAM IN DEFALCARI
    INSERT INTO GEST_DEFALCARE_DECONTARI(ID_GEST_DECONTARI, ID_GEST_ITEMSI, SUMA)
    SELECT @DECONT_ID, ID_GEST_ITEMSI, VALOARE_RECEPTIE_TVA 
    FROM GEST_ITEMSI 
    WHERE ID_GEST_DOCUM =  @NEW_ID_DOCUM
  END

  DELETE FROM culgest_itemsi_procent 
  WHERE id_culgest_itemsi IN (
    SELECT a.id_culgest_itemsi 
    FROM culgest_itemsi AS a 
    WHERE id_culgest_docum = @id_culgest_docum
  )

  DELETE FROM CULGEST_DOCUM WHERE ID_CULGEST_DOCUM = @ID_CULGEST_DOCUM
  DELETE FROM CULGEST_ITEMSI WHERE ID_CULGEST_DOCUM = @ID_CULGEST_DOCUM

  -- Pana se completeaza formulele de calcul o sa lasam si actualizarile acestea
  UPDATE GEST_DOCUM SET 
    TOTALDOC = CASE 
                 WHEN id_gest_tip_docum = 12 
                   THEN COALESCE((SELECT SUM(VALOARE_LIVRARE) FROM GEST_ITEMSI WHERE ID_GEST_DOCUM = @NEW_ID_DOCUM), 0)
                 ELSE COALESCE((SELECT SUM(VALOARE_RECEPTIE_TVA) FROM GEST_ITEMSI WHERE ID_GEST_DOCUM = @NEW_ID_DOCUM), 0)
               END,
    TOTALTVA = COALESCE(
                (SELECT SUM(VALOARE_RECEPTIE_TVA) FROM GEST_ITEMSI WHERE ID_GEST_DOCUM = @NEW_ID_DOCUM), 
                0
              ) * CASE WHEN curs_schimb <> 0 THEN curs_schimb ELSE 1.00 END
  WHERE ID_GEST_DOCUM = @NEW_ID_DOCUM

  DECLARE @aorder       varchar(8000)
  DECLARE @fieldList    varchar(8000)
  DECLARE @fieldFormula varchar(8000)

  SET @aorder = ''
  SET @fieldList = ''
  -- Fortam actualizarea formulelor de calcul de la nivelul documentului

  DECLARE tmpEvalPoz CURSOR FOR
    SELECT 
      '[' + RTRIM(LTRIM(field_name)) + '] = (SELECT TOP 1 ' 
         + RTRIM(LTRIM(formula_calcul)) 
         + ' FROM gest_itemsi aa WHERE id_gest_docum = ' + RTRIM(LTRIM(STR(@new_id_docum))) + ')'
    FROM gest_defa_docum_document
    WHERE id_gest_defa_docum = @idDefaDocum 
      AND RTRIM(LTRIM(formula_calcul)) <> ''
    ORDER BY precedenta

  OPEN tmpEvalPoz
  FETCH NEXT FROM tmpEvalPoz INTO @fieldFormula

  WHILE @@FETCH_STATUS = 0
  BEGIN
    IF @fieldList > ''
      SET @fieldList = @fieldList + ', '
    SET @fieldList = @fieldList + @fieldFormula
    FETCH NEXT FROM tmpEvalPoz INTO @fieldFormula
  END

  CLOSE tmpEvalPoz
  DEALLOCATE tmpEvalPoz

  -- Actualizam pozitiile de document din lichidare
  UPDATE alop_ordonantare_lichidare SET 
    id_gest_docum = @new_id_docum,
    id_tcv        = (
                      SELECT TOP 1 id_gest_itemsi 
                      FROM gest_itemsi AS c 
                      WHERE c.id_gest_docum = @idInitial 
                        AND c.codmat = b.codmat
                    )
  FROM alop_ordonantare_lichidare AS a
       LEFT JOIN gest_itemsi b ON (a.id_tcv = b.id_gest_itemsi)
  WHERE a.id_gest_docum = @idInitial

  IF @fieldList > ''
  BEGIN
    SET @aorder = 'UPDATE gest_docum SET '+@fieldList+' WHERE id_Gest_docum = '+RTRIM(LTRIM(STR(@new_id_docum)))
    PRINT (@aorder)
    EXEC (@aorder)
  END

  SET NOCOUNT OFF

  SELECT @NEW_ID_DOCUM AS ID_GEST_DOCUM

END

