CREATE PROCEDURE [dbo].[SP_VALIDEAZA_DOCUMENT] (@ID_CULGEST_DOCUM INT, @idUser int= null, @isTemp bit = null) AS
BEGIN
  set @isTemp = isnull(@isTemp, 0)
   set @isTemp  = 0
  declare @idGestDefaDocum int
  set @idGestDefaDocum = isnull((select top 1 id_Gest_defa_docum from culgest_docum where id_culgest_docum = @ID_CULGEST_DOCUM), -1)

  set @idUser = isnull(@idUser, (select top 1 id_utilizatori from culgest_docum where id_culgest_docum = @ID_CULGEST_DOCUM))
    
  if  (@isTemp= 1) and exists((select top 1 1 from gest_defa_docum where id_gest_defa_Docum = @idGestDefaDocum and id_gest_tip_stoc_predator = -1)) and ((select top  1 id_functiuni from utilizatori where id_utilizatori = @idUser)) in (1,2)
    begin
      exec SP_VALIDEAZA_DOCUMENT_TMP @ID_CULGEST_DOCUM, @idUser
      return
    end


  set nocount on

  /* ADAUGAM IN GEST_DOCUM */
  declare @nr_chitanta int
  declare @data_chitanta datetime
  declare @tethysID varchar(36)

  DECLARE @NEW_ID_DOCUM INT
  declare @idDefaDocum  int
  --ID_DOCUMENT_CONEX - IDENTIFICATOR UNIC AL UNUI DOCUMENT
  --ID_TRANZACTIE - IDENTIFICATOR IN CADRUL UNUI GRUP DE DOCUMENTE AL DOCUMENTELOR

  select @idDefaDocum = id_gest_defa_docum, @tethysID = TethysId from culgest_docum where id_culgest_docum = @id_culgest_docum

  if isnull((select top 1 SE_GEN_CHITANTA from culgest_docum where id_culgest_docum = @id_culgest_docum), 0) = 1 begin
      select top 1 @nr_chitanta =nr_chitanta, @data_chitanta = data_chitanta from culgest_docum where id_culgest_docum= @id_culgest_docum
      if @nr_chitanta is null begin
          set @nr_chitanta = isnull(@nr_chitanta, (select isnull(max(nr_chitanta), 2000) from gest_docum where stare = 1) + 1)
          --exec sp_get_next_value_var 'GEST_CHITANTA', @nr_chitanta output
          set @data_chitanta = coalesce(@data_chitanta, (select top 1 data_docum from culgest_docum where id_culgest_docum = @id_culgest_docum), getdate())
      end
  end
  /*
  if ltrim(rtrim(isnull((select top 1 id_repartitori_delegati from culgest_docum where id_culgest_docum= @id_culgest_docum), ''))) <> ''
   begin 
      select top 1 @nr_chitanta =nr_chitanta, @data_chitanta = data_chitanta from culgest_docum where id_culgest_docum= @id_culgest_docum
      if @nr_chitanta is null begin
          set @nr_chitanta = isnull(@nr_chitanta, (select isnull(max(nr_chitanta), 2000) from gest_docum where stare = 1) + 1)
          --exec sp_get_next_value_var 'GEST_CHITANTA', @nr_chitanta output
          set @data_chitanta = coalesce(@data_chitanta, (select top 1 data_docum from culgest_docum where id_culgest_docum = @id_culgest_docum), getdate())
      end
   end
  */

  INSERT INTO GEST_DOCUM (
    ID_GEST_TIP_DOCUM, ID_PREDATOR, ID_PRIMITOR, NR_DOCUM, DATA_DOCUM, TOTALDOC, COST_MARFA, TOTALTVA, TIP_PRET, TIP_ADAOS, COTA_ADAOS, ADAOS,
    TIP_DISCNT, COTA_DISCNT, DISCOUNT, ACCIZE, VALACCIZE, ID_UTILIZATORI, PACHET, NR_LIST, LINE, NUME_DELEGAT, MIJLTRANSPORT, TOTALVALUT,
    ID_TIP_VALUTA, CURS_SCHIMB, CURS_FIRMA, ACHITAT,  DATA_EMITERE, PE_DRUM, TRANSMIS, EMIS_HQ, SCADENTA, CONDLVR, NPRODUS, EXPLICATIE,
    ID_GEST_DEFA_DOCUM, CONT_CONTABIL, TVA_TRANSPORT, VALOARE_TRANSPORT,
    VALIDAT, ID_INITIAL, ID_MODIFICARE, STARE, DATA_OPERARE, ID_DOCUMENT_CONEX, ID_TRANZACTIE, AUTOGENERAT,
    ID_CURSURI, DESCRIERE_CURS, NR_CHITANTA, DATA_CHITANTA, NR_DOC_CONEX, DATA_DOC_CONEX, TethysId, NOTA_AUTOMAT, NR_NOTA, DATA_NOTA, DATA_SCADENTA
 --   ,ID_REPARTITORI_DELEGATI, ID_REPARTITORI_TRANSPORT 
)
  SELECT 
    ID_GEST_TIP_DOCUM, ID_PREDATOR, ID_PRIMITOR, NR_DOCUM, DATA_DOCUM, TOTALDOC, COST_MARFA, TOTALTVA, TIP_PRET, TIP_ADAOS, COTA_ADAOS, ADAOS,
    TIP_DISCNT, COTA_DISCNT, DISCOUNT, ACCIZE, VALACCIZE, ID_UTILIZATORI, PACHET, NR_LIST, LINE, NUME_DELEGAT, MIJLTRANSPORT, TOTALVALUT, ID_TIP_VALUTA,
    CURS_SCHIMB, CURS_FIRMA, ACHITAT, DATA_EMITERE, PE_DRUM, TRANSMIS, EMIS_HQ, SCADENTA, CONDLVR, NPRODUS, EXPLICATIE, ID_GEST_DEFA_DOCUM, CONT_CONTABIL,
    TVA_TRANSPORT, VALOARE_TRANSPORT, 
    (select top 1 case when isnull(aa.auto_validare_emitent, 0) = 1 then 1 else 0 end from gest_defa_Docum aa where aa.id_gest_defa_docum = a.id_gest_defa_Docum) AS VALIDAT,
    ID_INITIAL, ID_MODIFICARE,
    1 AS STARE, GETDATE() AS DATA_OPERARE, ID_DOCUMENT_CONEX, ID_TRANZACTIE, 0 AS AUTOGENERAT, ID_CURSURI, DESCRIERE_CURS,
    @nr_chitanta, @data_chitanta, NR_DOC_CONEX, DATA_DOC_CONEX, TethysId, NOTA_AUTOMAT, 
    ISNULL(case when a.id_utilizatori = 402 then ltrim(rtrim(str(month(ISNULL(DATA_NOTA, DATA_DOCUM))))) + '/MAG' else null end , NR_NOTA), DATA_NOTA, DATA_SCADENTA
 --   ,ID_REPARTITORI_DELEGATI, ID_REPARTITORI_TRANSPORT
  FROM CULGEST_DOCUM a
    WHERE ID_CULGEST_DOCUM = @ID_CULGEST_DOCUM

---  SET @NEW_ID_DOCUM = @@IDENTITY
  SET @NEW_ID_DOCUM = SCOPE_IDENTITY()

  if isnull(@tethysID, '') <> '' 
   insert into TethysDOCUMENTE (id_gest_docum, TT_Id, stare ) values(  @NEW_ID_DOCUM, @tethysID, 1)

  UPDATE GEST_DOCUM SET 
      ID_MODIFICARE     = ISNULL(ID_MODIFICARE, @NEW_ID_DOCUM),
      ID_DOCUMENT_CONEX = ISNULL(ID_DOCUMENT_CONEX, @NEW_ID_DOCUM),
      ID_INITIAL        = ISNULL(ID_INITIAL, @NEW_ID_DOCUM),
      ID_TRANZACTIE     = ISNULL(ID_TRANZACTIE, @NEW_ID_DOCUM)
  WHERE ID_GEST_DOCUM = @NEW_ID_DOCUM

  /* defalcarea codului sumator o facem direct in culgest_itemsi
     o sa apara la culegere defalcat, astfel incat sa avem pret-ul de livrare pentru fiecare pozitie separat 
     in cazul bonului de consum oricum ne trebuie lucrul acesta */

  if dbo.fnParamSoc(1, 'tipStock', 'S') = 'S'
    exec spSpargeCodSumCulegere @id_culgest_docum

  INSERT INTO GEST_ITEMSI (
    ID_GEST_DOCUM, STARE,
    ID_ANGAJAMENTE_DEFALCARE, CODMAT, ID_GEST_TIP_MATERIAL, ID_GEST_TIP_STOCK, ID_UTILIZATORI, STOCK_BEFORE, CANTITATE, STOCK_AFTER,
    PRET_UNITAR, PRET_RECEPTIE, COTA_TVA, PRET_RECEPTIE_TVA, PRET_LIVRARE, PRET_LIVRARE_TVA, PRET_LIVRARE_VALUTA, LOHN, DATA_EXPIRARE, 
    TVA, PRODUS, COD_FUNCTIONAL, COD_ECONOMIC, LOT_FABRICATIE, UM_SUPLIMENTARA, CONVERSIE_UM, CANTITATE_SUPLIMENTARA, 
    PRET_RECEPTIE_VALUTA,TVA_LIVRARE, TVA_RECEPTIE, VALOARE_LIVRARE, VALOARE_LIVRARE_TVA, VALOARE_RECEPTIE_VALUTA, VALOARE_LIVRARE_VALUTA, 
    VALOARE_RECEPTIE, VALOARE_RECEPTIE_TVA,COD_TARIF_VAMAL, TVA_AMANAT, ADAOS, CANTITATE_ESTIMATA, ADAOS_IMPUS, CATEGORIE_GRUPARE, 
    TIP_VALUTA_RECEPTIE, TIP_VALUTA_LIVRARE, COTA_ADAOS, COTA_ADAOS_IMPUS, SEMN_CANTITATE, SE_EMITE, NR_CHITANTA, DATA_GARANTIE, ID_ANALITIC, 
		ContD, ContC, idGestCategorii, RepD, RepC, id_oi_unitati, id_oi_proiecte
  )
  SELECT @NEW_ID_DOCUM AS ID_GEST_DOCUM, 1 AS STARE, 
    ID_ANGAJAMENTE_DEFALCARE, CODMAT, ID_GEST_TIP_MATERIAL, ID_GEST_TIP_STOCK, ID_UTILIZATORI, STOCK_BEFORE, CANTITATE, STOCK_AFTER, PRET_UNITAR, 
    PRET_RECEPTIE, COTA_TVA, PRET_RECEPTIE_TVA, PRET_LIVRARE, PRET_LIVRARE_TVA, PRET_LIVRARE_VALUTA, LOHN, DATA_EXPIRARE, 
    TVA, PRODUS, COD_FUNCTIONAL, COD_ECONOMIC, LOT_FABRICATIE, UM_SUPLIMENTARA, CONVERSIE_UM, CANTITATE_SUPLIMENTARA, 
    PRET_RECEPTIE_VALUTA,TVA_LIVRARE, TVA_RECEPTIE, VALOARE_LIVRARE, VALOARE_LIVRARE_TVA, VALOARE_RECEPTIE_VALUTA, VALOARE_LIVRARE_VALUTA, 
    VALOARE_RECEPTIE, VALOARE_RECEPTIE_TVA,COD_TARIF_VAMAL, TVA_AMANAT, ADAOS, CANTITATE_ESTIMATA, ADAOS_IMPUS, CATEGORIE_GRUPARE, 
    TIP_VALUTA_RECEPTIE, TIP_VALUTA_LIVRARE, COTA_ADAOS, COTA_ADAOS_IMPUS, SEMN_CANTITATE, SE_EMITE, NR_CHITANTA, DATA_GARANTIE, ID_ANALITIC, 
    ContD, ContC, idGestCategorii, RepD, RepC, id_oi_unitati, id_oi_proiecte
  FROM CULGEST_ITEMSI 
  WHERE ID_CULGEST_DOCUM = @ID_CULGEST_DOCUM
  ORDER BY ID_CULGEST_ITEMSI

  update c set c.id_document_intrare = isnull(c.id_document_intrare,  a.id_gest_docum), c.data_cod = isnull(c.data_cod, a.data_docum) ,
      c.GestIntrare = isnull(c.GestIntrare, a.id_Primitor)
  from 
    gest_docum a join gest_itemsi b on (a.id_gest_docum = b.id_gest_docum)  
    join gest_gnmcl c on (b.codmat = c.codmat)
  where 
    a.stare = 1 and b.stare = 1 and a.id_gest_docum = @NEW_ID_DOCUM
    and (c.id_document_intrare is null or not exists(select top 1 1 from gest_docum where stare = 1 and id_Gest_docum = c.id_document_intrare))

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
  SET @DECONT_VALOARE = ISNULL((SELECT SUM(VALOARE_RECEPTIE_TVA) FROM GEST_ITEMSI WHERE ID_GEST_DOCUM = @NEW_ID_DOCUM), 0)
  SELECT @DECONT_GENERATE = DECONT_GENERATE,  
    @DECONT_COD_CB = DECONT_COD_CB,  @DECONT_NRDOC = DECONT_NRDOC, @DECONT_DATA = DECONT_DATA, @DECONT_TIPDOC = DECONT_TIPDOC
  FROM 
    CULGEST_DOCUM WHERE ID_CULGEST_DOCUM = @ID_CULGEST_DOCUM

  SET @DECONT_CONT = (
    SELECT TOP 1 B.CONT_CREDITOR  FROM GEST_DOCUM A JOIN GEST_DEFA_NOTA_CONT B ON (A.ID_GEST_DEFA_DOCUM = B.ID_GEST_DEFA_DOCUM)
      JOIN GEST_ITEMSI C ON (A.ID_GEST_DOCUM = C.ID_GEST_DOCUM)
    WHERE A.ID_GEST_DEFA_DOCUM = @idGestDefaDocum ORDER BY CASE WHEN B.CONT_CREDITOR LIKE '4%' THEN 0 ELSE 1 END)

  SELECT TOP 1 
    @DECONT_EXPLICATIE = 'PLATA '+ B.NUME + ISNULL(' ' + (SELECT TOP 1 COD_DOCUM FROM GEST_TIP_DOCUM WHERE ID_GEST_TIP_DOCUM = A.ID_GEST_TIP_DOCUM) + ' ', '') + ISNULL(A.NR_DOCUM + ' ', '') + ISNULL('din ' + CONVERT(VARCHAR(10), DATA_DOCUM, 103), '') ,
    @DECONT_CODGEST =  A.ID_PREDATOR, 
    @DECONT_CONT = ISNULL(@DECONT_CONT, '4011'),
    @DECONT_C_O = A.ID_UTILIZATORI
  FROM 
    GEST_DOCUM A 
    JOIN REPARTITORI B ON (A.ID_PREDATOR = B.ID_REPARTITORI)
  WHERE ID_GEST_DOCUM = @NEW_ID_DOCUM

  IF (ISNULL(@DECONT_GENERATE, 0) = 1) AND  (@DECONT_VALOARE <> 0) BEGIN
    --NOU ID
    EXEC SP_GET_NEXT_VALUE_VAR 'BREGISTRU', @NEW_COD OUTPUT
    --INSERAM IN CASA BANCA
    INSERT INTO BREGISTRU(COD, COD_CB, DATA, TIPDOC, NRDOC, POZ, EXPLICATIE, INCASARI, PLATI, CONT_CSP, DATAEM, C_O, ECL, CODGEST)
    SELECT @NEW_COD,@DECONT_COD_CB, @DECONT_DATA, @DECONT_TIPDOC, @DECONT_NRDOC, 1, @DECONT_EXPLICATIE, NULL, @DECONT_VALOARE, @DECONT_CONT, GETDATE(),@DECONT_C_O, 1 AS ECL, @DECONT_CODGEST
    --INSERAM IN DECONTARI
    INSERT INTO GEST_DECONTARI(ID_GEST_DOCUM, ID_BREGISTRU, SUMA, AUTOGENERAT) VALUES(@NEW_ID_DOCUM, @NEW_COD, @DECONT_VALOARE, 1)
    SELECT @DECONT_ID = SCOPE_IDENTITY()
    --INSERAM IN DEFALCARI
    INSERT INTO GEST_DEFALCARE_DECONTARI(ID_GEST_DECONTARI, ID_GEST_ITEMSI, SUMA)
    SELECT @DECONT_ID, ID_GEST_ITEMSI, VALOARE_RECEPTIE_TVA FROM GEST_ITEMSI WHERE ID_GEST_DOCUM =  @NEW_ID_DOCUM
  END

  DELETE FROM CULGEST_DOCUM WHERE ID_CULGEST_DOCUM = @ID_CULGEST_DOCUM
  DELETE FROM CULGEST_ITEMSI WHERE ID_CULGEST_DOCUM = @ID_CULGEST_DOCUM

  -- Pana se completeaza formulele de calcul o sa lasam si actualizarile acestea
  UPDATE GEST_DOCUM SET 
    TOTALDOC = (SELECT SUM(VALOARE_RECEPTIE_TVA) FROM GEST_ITEMSI WHERE ID_GEST_DOCUM = @NEW_ID_DOCUM),
    TOTALTVA = (SELECT SUM(VALOARE_RECEPTIE_TVA) FROM GEST_ITEMSI WHERE ID_GEST_DOCUM = @NEW_ID_DOCUM) 
  WHERE ID_GEST_DOCUM = @NEW_ID_DOCUM

  declare @aorder varchar(8000)
  declare @fieldList    varchar(8000)
  declare @fieldFormula varchar(8000)

  set @aorder = ''
  set @fieldList = ''
  -- Fortam actualizarea formulelor de calcul de la nivelul documentului


  declare tmpEvalPoz cursor for
  select 
      '['+rtrim(ltrim(field_name))+'] = (select top 1 '+rtrim(ltrim(formula_calcul))+' from gest_itemsi aa where id_gest_docum = '+rtrim(ltrim(str(@new_id_docum)))+')'
    from gest_defa_docum_document
      where id_gest_defa_docum = @idDefaDocum and 
      --semn_items = sign((select min(cantitate) from gest_itemsi where id_gest_docum = @new_id_docum)) and 
      rtrim(ltrim(formula_calcul)) <> ''
    order by precedenta
  open tmpEvalPoz
  fetch next from tmpEvalPoz into @fieldFormula
  while @@fetch_status = 0
    begin
      if @fieldList > ''
        set @fieldList = @fieldList + ', '
      set @fieldList = @fieldList + @fieldFormula
      fetch next from tmpEvalPoz into @fieldFormula
    end
  close tmpEvalPoz
  deallocate tmpEvalPoz

  if @fieldList > ''
    begin
      set @aorder = 'update gest_docum set '+@fieldList+' where id_Gest_docum = '+rtrim(ltrim(str(@new_id_docum)))
      print (@aorder)
      exec (@aorder)
    end

  set nocount off

  SELECT @NEW_ID_DOCUM AS ID_GEST_DOCUM

END

