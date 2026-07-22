CREATE procedure [dbo].[spInchidereStocuri] (@dataInchidere datetime, @dbName varchar(128) = null) as
begin

  DECLARE @ID_GEST_TIP_STOC INT
  DECLARE @ID_REPARTITORI INT  
  DECLARE @AORDER VARCHAR(8000)  

  SET @ID_GEST_TIP_STOC = 1

  if @dbName is null
    set @dbName = db_name()

  create table #gest_gnmcl (codmat int, id_gest_tip_material int, pret_unitar money, cota_tva money, cod_functional varchar(128), cod_economic varchar(128))
  exec('insert into #gest_gnmcl (codmat, id_gest_tip_material, pret_unitar, cota_tva, cod_functional, cod_economic) select codmat, id_gest_tip_material, pret_unitar, cota_tva, cod_functional, cod_economic from '+@dbName+'..gest_gnmcl')

  /* CREEM TABELA DE STOCURI */  
  CREATE TABLE #STOCURI     (CODMAT INT, DATACOD DATETIME, NR_DOCUM VARCHAR(128), CANTITATE MONEY, CONT VARCHAR(128) NULL)
  CREATE TABLE #STOCURI_ALL (ID_REPARTITORI INT, CODMAT INT, DATACOD DATETIME, NR_DOCUM VARCHAR(128), CANTITATE MONEY, CONT VARCHAR(128) NULL, VALOARE MONEY, TVA MONEY, VALOARE_CU_TVA MONEY, id_gest_tip_material int)

  set @aorder = 'declare @an int'
  set @aorder = @aorder + ' set @an = '+rtrim(ltrim(str(year(@dataInchidere))))
  set @aorder = @aorder + ' update '+@dbName+'..gest_docum set stare=-10 where year(data_docum) <= @an and year(data_operare) > @an and id_gest_tip_docum = 17'
--  exec (@aorder)

  DECLARE TMP_PARSE_GESTIUNI CURSOR FOR   
    SELECT ID_REPARTITORI FROM REPARTITORI AS A WHERE GESTINT = 1 AND   
           EXISTS (SELECT TOP 1 1 FROM GEST_DOCUM WHERE ID_PREDATOR = A.ID_REPARTITORI OR ID_PRIMITOR = A.ID_REPARTITORI and year(data_docum) < 2007)
  OPEN TMP_PARSE_GESTIUNI   
  FETCH NEXT FROM TMP_PARSE_GESTIUNI INTO @ID_REPARTITORI  
  WHILE @@FETCH_STATUS = 0  
    BEGIN  
      TRUNCATE TABLE #STOCURI
      set @aorder = 'declare @dataStoc datetime'+char(13)+char(10)
      set @aorder = @aorder + 'set @dataStoc = convert(datetime, '''+convert(varchar(10), @dataInchidere, 103)+''', 103)'+char(13)+char(10)
      set @aorder = @aorder + 'EXEC '+@dbName+'..SP_GETSTOCK_GESTIUNE '+rtrim(ltrim(str(@ID_GEST_TIP_STOC)))+', '+rtrim(ltrim(str(@ID_REPARTITORI)))+', @dataStoc, 0'
      INSERT INTO #STOCURI (CODMAT, DATACOD, NR_DOCUM, CANTITATE, CONT) exec(@aorder)
      IF (SELECT COUNT(*) FROM #STOCURI) > 0  
        BEGIN  
          -- ACTUALIZAM STOCURILE
          insert into #STOCURI_ALL (ID_REPARTITORI, CODMAT, DATACOD, NR_DOCUM, CANTITATE, CONT)
          select @ID_REPARTITORI, CODMAT, DATACOD, NR_DOCUM, CANTITATE, CONT from #STOCURI
        END  
      FETCH NEXT FROM TMP_PARSE_GESTIUNI INTO @ID_REPARTITORI  
    END  
  CLOSE TMP_PARSE_GESTIUNI   
  DEALLOCATE TMP_PARSE_GESTIUNI  

  delete from #STOCURI_ALL where left(cont, 1) <> '3'

  update #STOCURI_ALL set 
    id_gest_tip_material = isnull( b.id_gest_tip_material, case when cont = '3031' then 4 when cont = '3032' then 3 when cont = '3021' then 2 when cont = '3028' then 1 end) ,
    VALOARE        = isnull(a.cantitate, 0) * isnull(b.PRET_UNITAR, 0),
    TVA            = isnull(a.cantitate, 0) * isnull(b.PRET_UNITAR, 0) * ( ( isnull(b.COTA_TVA,0) ) / 100),
    VALOARE_CU_TVA = isnull(a.cantitate, 0) * isnull(b.PRET_UNITAR, 0) * ( ( 100.00 + isnull(b.COTA_TVA,0) ) / 100)
  from #STOCURI_ALL a join #gest_gnmcl b on (a.codmat = b.codmat)

  declare @idTranzactie int
  exec sp_get_next_value_var 'INCHIDERE_STOCURI', @idTranzactie OUTPUT

  if @idTranzactie is null
    begin
      raiserror('Eroare : nu se poate genera numar de tranzactie pentru inchiderea stocurilor', 17, 1)
      return
    end

  alter table gest_docum disable trigger all

  /* creem documentul de stoc initial */
  insert into gest_docum (ID_GEST_TIP_DOCUM, ID_PREDATOR, ID_PRIMITOR, NR_DOCUM, DATA_DOCUM, TOTALDOC, COST_MARFA, TOTALTVA, TIP_PRET, TIP_ADAOS, COTA_ADAOS, ADAOS, TIP_DISCNT, COTA_DISCNT, 
    DISCOUNT, ACCIZE, VALACCIZE, ID_UTILIZATORI, PACHET, NR_LIST, LINE, NUME_DELEGAT,  MIJLTRANSPORT, TOTALVALUT, ID_TIP_VALUTA, CURS_SCHIMB, CURS_FIRMA, ACHITAT, DATA_EMITERE, PE_DRUM, 
    TRANSMIS, EMIS_HQ, SCADENTA, CONDLVR, NPRODUS, EXPLICATIE, ID_GEST_DEFA_DOCUM, DOCUMENT_FIZIC, VALIDAT, ID_INITIAL, STARE, DATA_OPERARE, ID_MODIFICARE, CONT_CONTABIL, ID_REPARTITORI_DELEGATI, ID_REPARTITORI_TRANSPORT)
  select 
    (select id_gest_tip_docum from gest_tip_docum where cod_docum like 'LDI') as ID_GEST_TIP_DOCUM, 
    id_repartitori  as ID_PREDATOR, 
    id_repartitori  as ID_PRIMITOR, 
    'LDI_1'         as NR_DOCUM,
    @dataInchidere  as DATA_DOCUM,
    sum(valoare_cu_tva) as TOTALDOC,
    0               as COST_MARFA,
    sum(a.tva)      as TOTALTVA,
    0               as TIP_PRET,
    0               as TIP_ADAOS,
    0               as COTA_ADAOS,
    0               as ADAOS,
    0               as TIP_DISCNT,
    0               as COTA_DISCNT,
    0               as DISCOUNT,
    0               as ACCIZE,
    0               as VALACCIZE,
    (select id_utilizatori from utilizatori where nume like 'admin') as ID_UTILIZATORI,
    null            as PACHET,
    @idTranzactie   as NR_LIST,
    null            as LINE,
    null            as NUME_DELEGAT,
    null            as MIJLTRANSPORT,
    null            as TOTALVALUT,
    null            as ID_TIP_VALUTA,
    0               as CURS_SCHIMB,
    0               as CURS_FIRMA,
    0               as ACHITAT,
    getdate()       as DATA_EMITERE,
    null            as PE_DRUM,
    null            as TRANSMIS,
    null            as EMIS_HQ,
    null            as SCADENTA,
    null            as CONDLVR,
    null            as NPRODUS,
    null            as EXPLICATIE,
    ( select id_gest_defa_docum from gest_tip_docum aa join gest_defa_docum bb on (aa.id_gest_tip_docum = bb.id_gest_tip_docum) where cod_docum like 'LDI' and predator_intern = 1 ) as ID_GEST_DEFA_DOCUM,
    null            as DOCUMENT_FIZIC,
    1               as VALIDAT, 
    null            as ID_INITIAL,
    1               as STARE,
    getdate()       as DATA_OPERARE,
    null            as ID_MODIFICARE,
    null            as CONT_CONTABIL,
    null            as ID_REPARTITORI_DELEGATI,
    null            as ID_REPARTITORI_TRANSPORT
  from #STOCURI_ALL a join #gest_gnmcl b on (a.codmat = b.codmat)
    where left(cont, 1) = '3'
  group by id_repartitori

  update gest_docum set id_initial = id_gest_docum, id_modificare = id_gest_docum where NR_LIST = @idTranzactie

  -- Anulam documentele anterioare datei de inchidere
  update gest_docum set stare=-20 where data_docum <= @dataInchidere and isnull(NR_LIST,-1) <> @idTranzactie

  alter table gest_docum enable trigger all

  alter table gest_itemsi disable trigger all

  insert into gest_itemsi (ID_GEST_DOCUM, ID_ANGAJAMENTE_DEFALCARE, CODMAT, ID_GEST_TIP_MATERIAL, ID_GEST_TIP_STOCK, ID_UTILIZATORI, STOCK_BEFORE, CANTITATE, STOCK_AFTER, PRET_UNITAR, PRET_RECEPTIE, COTA_TVA, PRET_RECEPTIE_TVA, 
    PRET_LIVRARE, PRET_LIVRARE_TVA, PRET_LIVRARE_VALUTA, LOHN, STARE, TVA, PRODUS, COD_FUNCTIONAL, COD_ECONOMIC, LOT_FABRICATIE, UM_SUPLIMENTARA, CONVERSIE_UM, CANTITATE_SUPLIMENTARA, PRET_RECEPTIE_VALUTA, TVA_LIVRARE, 
    TVA_RECEPTIE, VALOARE_LIVRARE, VALOARE_LIVRARE_TVA, VALOARE_RECEPTIE_VALUTA, VALOARE_LIVRARE_VALUTA, VALOARE_RECEPTIE, VALOARE_RECEPTIE_TVA, COD_TARIF_VAMAL, TVA_AMANAT, ADAOS, CANTITATE_ESTIMATA, ADAOS_IMPUS, 
    CATEGORIE_GRUPARE, TIP_VALUTA_RECEPTIE, TIP_VALUTA_LIVRARE, COTA_ADAOS, COTA_ADAOS_IMPUS, DATA_EXPIRARE)
  select
    c.ID_GEST_DOCUM, 
    null      as ID_ANGAJAMENTE_DEFALCARE,
    a.CODMAT,
    a.ID_GEST_TIP_MATERIAL,
    1         as ID_GEST_TIP_STOCK,
    (select id_utilizatori from utilizatori where nume like 'admin') as ID_UTILIZATORI,
    0           as STOCK_BEFORE,
    a.cantitate as CANTITATE,
    a.cantitate as STOCK_AFTER,
    b.PRET_UNITAR,
    b.PRET_UNITAR as PRET_RECEPTIE,
    isnull(b.COTA_TVA,0),
    isnull(b.PRET_UNITAR, 0) * ( ( 100.00 + isnull(b.COTA_TVA,0) ) / 100) as PRET_RECEPTIE_TVA,
    0             as PRET_LIVRARE,
    0             as PRET_LIVRARE_TVA,
    0             as PRET_LIVRARE_VALUTA,
    0             as LOHN,
    1             as STARE,
    isnull(b.PRET_UNITAR, 0) * ( ( isnull(b.COTA_TVA,0) ) / 100) as TVA,
    'M'           as PRODUS,
    null          as COD_FUNCTIONAL,
    null          as COD_ECONOMIC,
    @idTranzactie as LOT_FABRICATIE,
    null          as UM_SUPLIMENTARA,
    null          as CONVERSIE_UM,
    null          as CANTITATE_SUPLIMENTARA,
    null          as PRET_RECEPTIE_VALUTA,
    null          as TVA_LIVRARE,
    isnull(a.cantitate, 0) * isnull(b.PRET_UNITAR, 0) * ( ( isnull(b.COTA_TVA,0) ) / 100) as TVA_RECEPTIE,
    null          as VALOARE_LIVRARE,
    null          as VALOARE_LIVRARE_TVA,
    null          as VALOARE_RECEPTIE_VALUTA,
    null          as VALOARE_LIVRARE_VALUTA,
    isnull(a.cantitate, 0) * isnull(b.PRET_UNITAR, 0) as VALOARE_RECEPTIE,
    isnull(a.cantitate, 0) * isnull(b.PRET_UNITAR, 0) * ( ( 100.00 + isnull(b.COTA_TVA,0) ) / 100) as VALOARE_RECEPTIE_TVA,
    null          as COD_TARIF_VAMAL,
    null          as TVA_AMANAT,
    null          as ADAOS,
    null          as CANTITATE_ESTIMATA,
    null          as ADAOS_IMPUS,
    null          as CATEGORIE_GRUPARE,
    null          as TIP_VALUTA_RECEPTIE,
    null          as TIP_VALUTA_LIVRARE,
    null          as COTA_ADAOS,
    null          as COTA_ADAOS_IMPUS,
    null          as DATA_EXPIRARE
  from #STOCURI_ALL a join #gest_gnmcl b on (a.codmat = b.codmat)
       join (select id_gest_docum, id_predator from gest_docum where nr_list = @idTranzactie) as c on (a.id_repartitori = c.id_predator)
    where left(cont, 1) = '3'

  -- Anulam documentele anterioare datei de inchidere
  update gest_itemsi set stare=-20 where id_gest_docum in (select id_gest_docum from gest_docum where data_docum <= @dataInchidere and isnull(NR_LIST,-1) <> @idTranzactie) 

  alter table gest_itemsi enable trigger all

  select cont, sum ( isnull(a.cantitate, 0) * isnull ( b.pret_unitar, 0) * (100 +isnull(cota_tva,0)) / 100 ) as valoare from #STOCURI_ALL a join #gest_gnmcl b on (a.codmat = b.codmat) 
  where left(cont, 1) = '3' group by cont  

  SET NOCOUNT OFF  
end




