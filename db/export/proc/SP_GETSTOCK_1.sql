CREATE PROCEDURE [dbo].[SP_GETSTOCK_1] (@ID_GEST_DEFA_DOCUM INT, @ID_PREDATOR INT, @ID_PRIMITOR INT, @DATA_STOC DATETIME) AS              
BEGIN              
              
  SET NOCOUNT ON              
              
              
  declare @idDefaDocumConex int              
  set @idDefaDocumConex =               
      (select top 1 c.id_gest_defa_Docum from gest_defa_docum a join gest_tip_docum b on (a.id_document_conex = b.id_gest_tip_docum)               
        join gest_defa_docum c on (b.id_gest_tip_docum = c.id_gest_tip_docum and a.predator_intern = c.predator_intern and a.primitor_intern = c.primitor_intern)              
       where a.id_Gest_defa_docum = @ID_GEST_DEFA_DOCUM)              
              
  create table #tipMaterial (id_gest_tip_material int)              
  insert into #tipMaterial              
  select distinct id_gest_tip_material               
  from               
  (Select distinct id_gest_tip_material from gest_defa_nota_cont with(nolock) where id_gest_defa_docum = @ID_GEST_DEFA_DOCUM              
   union all              
   Select distinct id_gest_tip_material from GEST_DEFA_ITEMSI_TIP_MATERIAL with(nolock) where id_gest_defa_docum = @ID_GEST_DEFA_DOCUM              
    ) as A              
            
              
  declare @tipExecStock char(1)              
  declare @dataCurenta datetime              
  select  @tipExecStock = dbo.fnParamSoc(1, 'tipStock', 'S')              
  set @dataCurenta = convert(datetime, floor(convert(float, getdate())))              
  set @data_stoc   = isnull(@data_stoc, @dataCurenta)              
            
--  DECLARE @ID_STOCK_PREDATOR INT              
--  DECLARE @ID_STOCK_PRIMITOR INT              
              
  DECLARE @CNT_STOCK_PREDATOR INT              
  DECLARE @CNT_STOCK_PRIMITOR INT              
              
              
  DECLARE @COMBINA_STOC      BIT              
                
  DECLARE @PREDATOR_INTERN   BIT              
  DECLARE @PRIMITOR_INTERN   BIT              
              
  SELECT @PREDATOR_INTERN = GESTINT FROM REPARTITORI WHERE ID_REPARTITORI = @ID_PREDATOR              
  SELECT @PRIMITOR_INTERN = GESTINT FROM REPARTITORI WHERE ID_REPARTITORI = @ID_PRIMITOR              
              
              
  set @CNT_STOCK_PREDATOR = isnull((select COUNT(*) FROM vGEST_DEFA_DOCUM_STOC WHERE ID_GEST_DEFA_DOCUM = @ID_GEST_DEFA_DOCUM AND PREDATOR = 1), 0)              
  set @CNT_STOCK_PRIMITOR = isnull((select COUNT(*) FROM vGEST_DEFA_DOCUM_STOC WHERE ID_GEST_DEFA_DOCUM = @ID_GEST_DEFA_DOCUM AND PREDATOR = 2), 0)              
            
          
  create table #stockPredator (ID_GEST_TIP_STOC INT, PREDATOR INT, CODMAT INT, ID_GEST_SUMATOR INT NULL, DATACOD DATETIME null, NR_DOCUM VARCHAR(50) null,               
    CANTITATE MONEY null, CANTITATE_ZI MONEY NULL, CONT VARCHAR(128) null, PRODUS varchar(5), ID_GEST_TIP_MATERIAL INT, VALOARE MONEY,              
    COD_ECONOMIC varchar(128))              
  create table #stockPrimitor (ID_GEST_TIP_STOC INT, PREDATOR INT, CODMAT INT, ID_GEST_SUMATOR INT NULL, DATACOD DATETIME null, NR_DOCUM VARCHAR(50) null,               
    CANTITATE MONEY null, CANTITATE_ZI MONEY NULL, CONT VARCHAR(128) null, PRODUS varchar(5), ID_GEST_TIP_MATERIAL INT, COD_ECONOMIC varchar(128), VALOARE MONEY)              
              
  create table #stockFinal (ID_SOLD_FINAL INT IDENTITY(1,1) , ID_GEST_TIP_STOC INT, PREDATOR INT, TIP_STOCK int, codmat int, id_gest_sumator int, datacod datetime null,             
    nr_docum varchar(50) null, cant_predator money null, cant_primitor money null, cant_predator_zi money null,               
    cant_primitor_zi money null, PRODUS varchar(5), ID_GEST_TIP_MATERIAL INT, COD_ECONOMIC varchar(128), VALOARE MONEY)              
  create table #stockLaZi (ID_GEST_TIP_STOC INT, PREDATOR INT, codmat int, id_gest_sumator int, PRODUS varchar(5), ID_GEST_TIP_MATERIAL INT, COD_ECONOMIC varchar(128), scazut money, VALOARE MONEY)              
              
  if @tipExecStock = 'S'              
    begin              
      PRINT 'SUMATOR'              
      if @CNT_stock_predator >0              
        begin              
  INSERT INTO #stockPredator (ID_GEST_TIP_STOC, PREDATOR, CODMAT, id_gest_sumator, DATACOD, NR_DOCUM, CANTITATE, CANTITATE_ZI, PRODUS, ID_GEST_TIP_MATERIAL, COD_ECONOMIC, VALOARE)              
          select ID_GEST_TIP_STOC, 1 AS PREDATOR, CODMAT, id_gest_sumator,               
              data_cod, NR_DOCUM, Stock, Stock, PRODUS, ID_GEST_TIP_MATERIAL, COD_ECONOMIC, StockValoric from dbo.fnStockCodSum(@DATA_STOC) where id_repartitori = @id_predator               
            --AND id_gest_tip_stoc = @id_stock_predator              
            and id_gest_tip_stoc in (Select distinct id_gest_tip_stoc from vGEST_DEFA_DOCUM_STOC where predator = 1 and id_gest_defa_Docum = @ID_GEST_DEFA_DOCUM)              
            and id_gest_tip_material in (Select id_gest_tip_material from #tipMaterial)              
                        
          delete from #stockLaZi              
          insert into #stockLaZi (ID_GEST_TIP_STOC, PREDATOR, id_gest_sumator, produs, id_gest_tip_material, cod_economic, scazut, valoare)              
          select ID_GEST_TIP_STOC, 1 AS PREDATOR, id_gest_sumator, produs, id_gest_tip_material, cod_economic, sum(stock), sum(stockValoric) from vStockAll               
                    where               
                      id_gest_tip_stoc in (Select distinct id_gest_tip_stoc from vGEST_DEFA_DOCUM_STOC where predator = 1 and id_gest_defa_Docum = @ID_GEST_DEFA_DOCUM)                                    
                      and id_gest_tip_material in (Select id_gest_tip_material from #tipMaterial)              
                      --id_gest_tip_stoc = @id_stock_predator               
                      and id_repartitori = @id_predator               
                      and convert(datetime, floor(convert(float, data_docum))) > @data_stoc and convert(datetime, floor(convert(float, data_docum))) <= @dataCurenta              
                      --and semn = -1              
          group by ID_GEST_TIP_STOC, id_gest_sumator, produs, id_gest_tip_material, cod_economic              
              
          update #stockPredator set cantitate_zi = isnull(a.cantitate, 0) + isnull(b.scazut, 0)              
          from #stockPredator a               
              join #stockLaZi b               
            on (a.id_gest_sumator = b.id_Gest_sumator and isnull(a.produs, 'X') = isnull(b.produs, 'X') and isnull(a.id_gest_tip_material, -1) = isnull(b.id_gest_tip_material, -1) and isnull(a.cod_economic, 'X') = isnull(b.cod_economic, 'X') )           
   
              
      end              
              
      if @CNT_stock_primitor >0              
        begin              
          INSERT INTO #stockPrimitor (ID_GEST_TIP_STOC, PREDATOR, codmat, id_gest_sumator, DATACOD, NR_DOCUM, CANTITATE, CANTITATE_ZI, PRODUS, ID_GEST_TIP_MATERIAL, COD_ECONOMIC, VALOARE)              
          select ID_GEST_TIP_STOC, 2 AS PREDATOR, codmat, id_gest_sumator, data_cod, NR_DOCUM, Stock, Stock, PRODUS, ID_GEST_TIP_MATERIAL, COD_ECONOMIC, StockValoric from dbo.fnStockCodSum(@DATA_STOC) where id_repartitori = @id_primitor               
            --and id_gest_tip_stoc = @id_stock_primitor              
            and id_gest_tip_stoc in (Select distinct id_gest_tip_stoc from vGEST_DEFA_DOCUM_STOC where predator = 2 and id_gest_defa_Docum = @ID_GEST_DEFA_DOCUM)              
              
          delete from #stockLaZi              
          insert into #stockLaZi (ID_GEST_TIP_STOC, PREDATOR, id_gest_sumator, produs, id_gest_tip_material, cod_economic, scazut, valoare)              
          select ID_GEST_TIP_STOC, 2 AS PREDATOR, id_gest_sumator, produs, id_gest_tip_material, cod_economic, sum(stock), sum(stockValoric) from vStockAll               
                    where               
                      --id_gest_tip_stoc = @id_stock_primitor               
                      id_gest_tip_stoc in (Select distinct id_gest_tip_stoc from vGEST_DEFA_DOCUM_STOC where predator = 2 and id_gest_defa_Docum = @ID_GEST_DEFA_DOCUM)              
                      and id_gest_tip_material in (Select id_gest_tip_material from #tipMaterial)              
             and id_repartitori = @id_primitor              
                      and convert(datetime, floor(convert(float, data_docum))) > @data_stoc and convert(datetime, floor(convert(float, data_docum))) <= @dataCurenta              
                      --and semn = -1              
          group by ID_GEST_TIP_STOC, id_gest_sumator, produs, id_gest_tip_material, cod_economic              
              
          update #stockPredator set cantitate_zi = isnull(a.cantitate, 0) + isnull(b.scazut, 0)              
          from #stockPredator a               
          join #stockLaZi b on               
           (a.id_gest_sumator = b.id_Gest_sumator and isnull(a.produs, 'X') = isnull(b.produs, 'X') and isnull(a.id_gest_tip_material, -1) = isnull(b.id_gest_tip_material, -1) and isnull(a.cod_economic, 'X') = isnull(b.cod_economic, 'X') )              
              
        end              
              
      insert into #stockFinal (ID_GEST_TIP_STOC, PREDATOR, TIP_STOCK, codmat, id_gest_sumator, datacod, nr_docum, cant_predator, cant_predator_zi, cant_primitor, cant_primitor_zi, PRODUS, ID_GEST_TIP_MATERIAL, COD_ECONOMIC, VALOARE)              
      SELECT              
        ISNULL(A.ID_GEST_TIP_STOC, B.ID_GEST_TIP_STOC) AS ID_GEST_TIP_STOC,              
        ISNULL(A.PREDATOR, B.PREDATOR)     AS PREDATOR,              
        CASE WHEN a.id_gest_sumator IS NULL THEN 1 WHEN b.id_gest_sumator IS NULL THEN 2 ELSE 3 END AS TIP_STOCK,              
        ISNULL(A.CODMAT, B.CODMAT)     AS codmat,              
        isnull(a.id_gest_sumator, b.id_gest_sumator) as id_gest_sumator,              
        ISNULL(A.DATACOD, B.DATACOD)   AS datacod,              
        ISNULL(A.NR_DOCUM, B.NR_DOCUM) AS nr_docum,              
        A.CANTITATE                    AS cant_predator,              
        A.CANTITATE_ZI                 AS cant_predator_zi,              
        B.CANTITATE                    AS cant_primitor,              
        B.CANTITATE_ZI                 AS cant_primitor_zi,              
        ISNULL(A.PRODUS, B.PRODUS)     AS produs,              
        ISNULL(A.ID_GEST_TIP_MATERIAL, B.ID_GEST_TIP_MATERIAL) AS ID_GEST_TIP_MATERIAL,              
        ISNULL(A.COD_ECONOMIC, B.COD_ECONOMIC) as COD_ECONOMIC,              
        ISNULL(A.VALOARE, B.VALOARE) AS VALOARE              
      FROM #stockPredator A               
      FULL OUTER JOIN #stockPrimitor B ON (              
          a.ID_GEST_TIP_STOC = b.ID_GEST_TIP_STOC  AND a.PREDATOR = b.PREDATOR AND               
          a.id_gest_sumator = b.id_Gest_sumator               
          and isnull(a.produs, 'X') = isnull(b.produs, 'X')               
          and isnull(a.id_gest_tip_material, -1) = isnull(b.id_gest_tip_material, -1)               
          and isnull(a.cod_economic, 'X') = isnull(b.cod_economic, 'X') )              
              
    end              
  else              
    begin              
      PRINT 'COD MATERIAL'              
      if @CNT_STOCK_PREDATOR > 0              
        begin              
          INSERT INTO #stockPredator (ID_GEST_TIP_STOC, PREDATOR, CODMAT, id_gest_sumator, DATACOD, NR_DOCUM, CANTITATE, CANTITATE_ZI, PRODUS, ID_GEST_TIP_MATERIAL, COD_ECONOMIC, VALOARE)              
          select ID_GEST_TIP_STOC, 1 AS PREDATOR, CODMAT, id_gest_sumator,               
            MIN(data_cod), MIN(NR_DOCUM), SUM(Stock), SUM(Stock),               
            MIN(PRODUS),               
            ID_GEST_TIP_MATERIAL,               
            MIN(COD_ECONOMIC), SUM(stockValoric)               
          from dbo.fnStockCodMat(@DATA_STOC)               
          where id_repartitori = @id_predator               
            and id_gest_tip_stoc in (Select distinct id_gest_tip_stoc from vGEST_DEFA_DOCUM_STOC where predator = 1 and id_gest_defa_Docum = @ID_GEST_DEFA_DOCUM)              
            and id_gest_tip_material in (Select id_gest_tip_material from #tipMaterial)              
          GROUP BY               
            ID_GEST_TIP_STOC, CODMAT, id_gest_sumator, ID_GEST_TIP_MATERIAL              
          HAVING NOT (SUM(Stock) = 0  AND SUM(stockValoric) = 0)              
              
          delete from #stockLaZi              
          insert into #stockLaZi (ID_GEST_TIP_STOC, PREDATOR, codmat, produs, id_gest_tip_material, cod_economic, scazut, valoare)              
            select ID_GEST_TIP_STOC, 1 AS PREDATOR, codmat, produs, id_gest_tip_material, cod_economic, sum(stock), sum(stockValoric)               
          from vStockAll               
            where               
              id_gest_tip_stoc in (Select distinct id_gest_tip_stoc from vGEST_DEFA_DOCUM_STOC where predator = 1 and id_gest_defa_Docum = @ID_GEST_DEFA_DOCUM)              
              --id_gest_tip_stoc = @id_stock_predator               
              and id_repartitori = @id_predator              
              and convert(datetime, floor(convert(float, data_docum))) > @data_stoc and convert(datetime, floor(convert(float, data_docum))) <= @dataCurenta              
              and id_gest_tip_material in (Select id_gest_tip_material from #tipMaterial)              
              --and semn = -1              
          group by ID_GEST_TIP_STOC, codmat, produs, id_gest_tip_material, cod_economic              
                        
          -- Atentie stock-ul intoarce pana la ziu inclusiv              
                        
          update #stockPredator set cantitate_zi = isnull(a.cantitate, 0) + isnull(b.scazut, 0)              
          from #stockPredator a join #stockLaZi b on (a.codmat = b.codmat and isnull(a.produs, 'X') = isnull(b.produs, 'X')            
               and isnull(a.id_gest_tip_material, -1) = isnull(b.id_gest_tip_material, -1)             
               and isnull(a.cod_economic, 'X') = isnull(b.cod_economic, 'X'))              
              
        end              
                     
                
      if @CNT_stock_primitor > 0              
        begin              
          INSERT INTO #stockPrimitor (ID_GEST_TIP_STOC, PREDATOR, codmat, id_gest_sumator, DATACOD, NR_DOCUM, CANTITATE, CANTITATE_ZI, PRODUS, ID_GEST_TIP_MATERIAL, COD_ECONOMIC, VALOARE)              
          select ID_GEST_TIP_STOC, 2 AS PREDATOR, codmat, id_gest_sumator,               
            MIN(data_cod), MIN(NR_DOCUM), SUM(Stock), SUM(Stock),               
            MIN(PRODUS),               
            ID_GEST_TIP_MATERIAL,               
            MIN(COD_ECONOMIC), SUM(stockValoric)               
          from dbo.fnStockCodMat(@DATA_STOC) where id_repartitori = @id_primitor and               
            id_gest_tip_stoc in (Select distinct id_gest_tip_stoc from vGEST_DEFA_DOCUM_STOC where predator = 2 and id_gest_defa_Docum = @ID_GEST_DEFA_DOCUM)              
          GROUP BY               
            ID_GEST_TIP_STOC, CODMAT, id_gest_sumator, ID_GEST_TIP_MATERIAL              
          HAVING NOT (SUM(Stock) = 0  AND SUM(stockValoric) = 0)              
              
          delete from #stockLaZi              
          insert into #stockLaZi (ID_GEST_TIP_STOC, PREDATOR, codmat, produs, id_gest_tip_material, cod_economic, scazut, valoare)              
          select ID_GEST_TIP_STOC, 2 AS PREDATOR, codmat, produs, id_gest_tip_material, cod_economic, sum(stock), sum(stockValoric) from vStockAll               
          where id_gest_tip_stoc in (Select distinct id_gest_tip_stoc from vGEST_DEFA_DOCUM_STOC where predator = 2 and id_gest_defa_Docum = @ID_GEST_DEFA_DOCUM)              
                and id_repartitori = @id_primitor              
                and convert(datetime, floor(convert(float, data_docum))) > @data_stoc and convert(datetime, floor(convert(float, data_docum))) <= @dataCurenta              
                --and semn = -1              
          group by ID_GEST_TIP_STOC, codmat, produs, id_gest_tip_material, cod_economic              
              
          -- Atentie stock-ul intoarce pana la zi inclusiv              
          update #stockPrimitor set cantitate_zi = isnull(a.cantitate, 0) + isnull(b.scazut, 0)              
          from #stockPrimitor a join #stockLaZi b on (a.codmat = b.codmat and isnull(a.produs, 'X') = isnull(b.produs, 'X')             
               and isnull(a.id_gest_tip_material, -1) = isnull(b.id_gest_tip_material, -1)             
               and isnull(a.cod_economic, 'X') = isnull(b.cod_economic, 'X') )              
              
        end              
              
      insert into #stockFinal (ID_GEST_TIP_STOC, PREDATOR, TIP_STOCK, codmat, id_gest_sumator, datacod, nr_docum, cant_predator, cant_predator_zi,             
                            cant_primitor, cant_primitor_zi, PRODUS, ID_GEST_TIP_MATERIAL, COD_ECONOMIC, VALOARE)              
      SELECT              
        ISNULL(A.ID_GEST_TIP_STOC, B.ID_GEST_TIP_STOC) AS ID_GEST_TIP_STOC,               
        ISNULL(A.PREDATOR, B.PREDATOR)     AS PREDATOR,                       
        CASE WHEN a.codmat IS NULL THEN 1 WHEN b.codmat IS NULL THEN 2 ELSE 3 END AS TIP_STOCK,              
        ISNULL(A.CODMAT, B.CODMAT)     AS codmat,              
        isnull(a.id_gest_sumator, b.id_gest_sumator) as id_gest_sumator,              
        ISNULL(A.DATACOD, B.DATACOD)   AS datacod,              
        ISNULL(A.NR_DOCUM, B.NR_DOCUM) AS nr_docum,              
        A.CANTITATE                    AS cant_predator,              
        A.CANTITATE_ZI                 AS cant_predator_zi,              
        B.CANTITATE                    AS cant_primitor,              
        B.CANTITATE_ZI                 AS cant_primitor_zi,              
        ISNULL(A.PRODUS, B.PRODUS)     AS produs,              
        ISNULL(A.ID_GEST_TIP_MATERIAL, B.ID_GEST_TIP_MATERIAL) AS ID_GEST_TIP_MATERIAL,              
        ISNULL(A.COD_ECONOMIC, B.COD_ECONOMIC) AS COD_ECONOMIC,              
        ISNULL(A.VALOARE, B.VALOARE) AS VALOARE              
      FROM #stockPredator A               
        FULL OUTER JOIN #stockPrimitor B ON (              
           a.ID_GEST_TIP_STOC = b.ID_GEST_TIP_STOC  AND a.PREDATOR = b.PREDATOR AND               
           a.codmat = b.codmat  and               
           isnull(a.produs, 'X') = isnull(b.produs, 'X') and               
           isnull(a.id_gest_tip_material, -1) = isnull(b.id_gest_tip_material, -1) and               
           isnull(a.cod_economic, 'X') = isnull(b.cod_economic, 'X')              
        )              
                
    end              
            
              
  --STERGEM POZITILE CARE AU 0 PE CRITERIUL ID_GEST_TIP_STOC, PREDATOR, CODMAT              
  --DELETE FROM #stockFinal WHERE CODMAT IN ( SELECT DISTINCT CODMAT FROM #stockFinal GROUP BY CODMAT HAVING SUM(CANT_PREDATOR) = 0)              
--    DELETE FROM #stockFinal WHERE ID_SOLD_FINAL IN ( SELECT MIN(ID_SOLD_FINAL) FROM #stockFinal GROUP BY ID_GEST_TIP_STOC, PREDATOR, CODMAT  HAVING SUM(CANT_PREDATOR) = 0  AND SUM(VALOARE) = 0 )                
  --  DELETE FROM #stockFinal WHERE CHEIE  IN ( SELECT CHEIE FROM #stockFinal GROUP BY CHEIE HAVING SUM(CANT_PREDATOR) = 0  AND SUM(VALOARE) = 0 )              
--  SELECT .dbo.fnGetContDebitItems(NULL, ID_GEST_TIP_MATERIAL, NULL), SUM(VALOARE) FROM  #stockFinal GROUP BY  .dbo.fnGetContDebitItems(NULL, ID_GEST_TIP_MATERIAL, NULL)              
              
  create table #stockFinal_Unic (id_gest_tip_stoc int, predator int, codmat int, produs varchar(5), tip_stock int, cant_predator_zi money,              
   cant_primitor_zi money, cant_predator money, cant_primitor money, valoare money, id_gest_tip_material int, datacod datetime, nr_docum varchar(50), cod_economic varchar(100))              
              
  insert into #stockFinal_Unic              
  select              
    B.ID_GEST_TIP_STOC,               
    B.PREDATOR AS PREDATOR,              
    B.codmat,               
    max(B.PRODUS) as PRODUS,              
    max(B.TIP_STOCK) as TIP_STOCK,              
    SUM(B.CANT_PREDATOR_ZI) as CANT_PREDATOR_ZI,              
    sum(B.CANT_PRIMITOR_ZI) as CANT_PRIMITOR_ZI,              
    sum(B.CANT_PREDATOR) as CANT_PREDATOR,              
    sum(B.CANT_PRIMITOR) as CANT_PRIMITOR,              
    SUM(B.VALOARE) AS VALOARE,              
    (B.ID_GEST_TIP_MATERIAL) as ID_GEST_TIP_MATERIAL,              
    max(B.DATACOD) as DATACOD,              
    max(B.NR_DOCUM) as NR_DOCUM,              
    max(B.COD_ECONOMIC) as COD_ECONOMIC              
  FROM #stockFinal B              
  group BY                   
    B.ID_GEST_TIP_STOC, B.codmat, B.PREDATOR, B.ID_GEST_TIP_MATERIAL              
            
  SET NOCOUNT OFF              
            
  SELECT               
  --  PREDATOR,              
  --  CASE WHEN PREDATOR = 1 THEN 1 WHEN PREDATOR = 2 THEN -1 ELSE 0 END AS SEMN_CANTITATE,               
  --  ID_GEST_TIP_STOC AS ID_GEST_TIP_STOCK,              
 --   1 AS ID_STOCK_PREDATOR,              
 --   1 AS ID_STOCK_PRIMITOR,              
  --  convert(bit, null) as SELECTAT,              
  --  convert(money, 0)  as CANTITATE_SELECTATA,              
 --   ltrim(rtrim(B.PRODUS)) as PRODUS,              
 --   B.TIP_STOCK,              
 --   B.VALOARE,               
   -- isnull(B.CANT_PREDATOR, B.CANT_PRIMITOR) * PRET_UNITAR * (1.0 + ISNULL(COTA_TVA/ 100.0, 0)) AS VAL_CU_TVA,              
        
 --   ISNULL(B.CANT_PREDATOR_ZI, B.CANT_PRIMITOR) as CANT_PREDATOR_ZI,              
--    (B.CANT_PRIMITOR_ZI) as CANT_PRIMITOR_ZI,              
--    isnull(B.CANT_PREDATOR, B.CANT_PRIMITOR) as CANT_PREDATOR,              
   -- (B.CANT_PRIMITOR) as CANT_PRIMITOR,              
  --  (B.ID_GEST_TIP_MATERIAL) as ID_GEST_TIP_MATERIAL,              
 --   .dbo.fnGetContDebitItems(NULL, b.ID_GEST_TIP_MATERIAL, NULL) AS cont,              
        
  --  (B.DATACOD) as DATACOD,              
         
                
 --   (SELECT TOP 1 COD_FUNCTIONAL FROM GEST_ITEMSI AA JOIN GEST_DOCUM BB ON (AA.ID_GEST_DOCUM = BB.ID_GEST_DOCUM) WHERE AA.STARE = 1 AND BB.STARE = 1 AND AA.CODMAT = A.CODMAT ORDER BY ISNULL(COD_FUNCTIONAL, '') DESC ) AS COD_FUNCTIONAL,              
  --  (SELECT TOP 1 ID_OI_UNITATI FROM GEST_ITEMSI AA JOIN GEST_DOCUM BB ON (AA.ID_GEST_DOCUM = BB.ID_GEST_DOCUM) WHERE AA.STARE = 1 AND BB.STARE = 1 AND AA.CODMAT = A.CODMAT ORDER BY ISNULL(ID_OI_UNITATI, 0) DESC ) AS ID_OI_UNITATI,            
--(SELECT TOP 1 ID_OI_PROIECTE FROM GEST_ITEMSI AA JOIN GEST_DOCUM BB ON (AA.ID_GEST_DOCUM = BB.ID_GEST_DOCUM) WHERE AA.STARE = 1 AND BB.STARE = 1 AND AA.CODMAT = A.CODMAT ORDER BY ISNULL(ID_OI_PROIECTE, 0) DESC ) AS ID_OI_PROIECTE           
  a.denmat,      
   A.codmat,        
isnull(B.CANT_PREDATOR, B.CANT_PRIMITOR)    as CANTITATE,            
 a.PRET_UNITAR,        
 a.pret_receptie,        
 a.um,         
 gi.data_expirare,        
   a.LOT_FABRICATIE,         
 a.CATEGORIE_GRUPARE,         
  a.ID_GEST_TIP_MATERIAL,        
 a.PRODUS,        
      @ID_PREDATOR       AS ID_PREDATOR,              
  @ID_PRIMITOR       AS ID_PRIMITOR,       
  a.ID_GEST_SUMATOR,         
 (B.COD_ECONOMIC) as COD_ECONOMIC,       
   a.data_cod,        
   b.datacod,      
 a.GestIntrare,        
 (B.NR_DOCUM) as NR_DOCUM,         
  a.id_document_intrare,        
 a.id_document_receptie,       
  a.tipmat,      
      
      coalesce(d.cont_debitor, e.cont_debitor) as ContD, coalesce(d.cont_creditor, e.cont_creditor) as ContC              
  FROM GEST_GNMCL A              
       JOIN #stockFinal_Unic B ON (A.CODMAT = B.CODMAT)      
OUTER APPLY (     SELECT TOP 1         gi.DATA_EXPIRARE     FROM GEST_ITEMSI gi     JOIN GEST_DOCUM gd          ON gd.ID_GEST_DOCUM = gi.ID_GEST_DOCUM     WHERE gi.CODMAT = A.CODMAT       AND gi.STARE = 1       AND gd.STARE = 1     ORDER BY          CASE 
WHEN gi.DATA_EXPIRARE IS NULL THEN 1 ELSE 0 END,         gi.DATA_EXPIRARE,         gi.ID_GEST_ITEMSI ) gi    
       left join gest_defa_nota_cont d on (d.id_gest_defa_nota_cont = (select min(id_Gest_defa_nota_cont) from gest_defa_nota_cont aa where aa.id_gest_defa_Docum = @ID_GEST_DEFA_DOCUM and aa.id_Gest_tip_material = b.id_Gest_tip_material))              
       left join gest_defa_nota_cont e on (e.id_gest_defa_nota_cont = (select min(id_Gest_defa_nota_cont) from gest_defa_nota_cont aa where aa.id_gest_defa_Docum = @idDefaDocumConex and aa.id_Gest_tip_material = a.id_Gest_tip_material))              
  ORDER BY id_Gest_tip_Stoc, id_predator, a.codmat              
              
END 
