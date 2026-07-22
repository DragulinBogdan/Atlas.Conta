CREATE PROCEDURE [dbo].[spGestCreateDocConex] (@ID_SURSA INT, @ID_GEST_DEFA_DOCUM INT, @idUser int = null, @isTemp bit = null) AS      
BEGIN      
      
  set nocount on      
  set @isTemp =  isnull(@isTemp, 0)      
  set @isTemp = 0      
/*      
  if (@isTemp =1) and ((select top  1 id_functiuni from utilizatori where id_utilizatori = @idUser)) in (1,2)      
    begin      
      exec SP_CREARE_DOCUMENT_CONEX_TMP @ID_SURSA, @ID_GEST_DEFA_DOCUM      
      --select convert(int, null) as ID_GEST_DOCUM      
      return      
    end      
*/      
        
  DECLARE @NEW_ID_DOCUM      INT      
  DECLARE @ID_GEST_TIP_DOCUM INT      
  DECLARE @TIP_GENERARE      INT      
  DECLARE @GenereazaNr bit      
  set @GenereazaNr = (select top 1 isnull(NUMAR_AUTOMAT, 0) from gest_defa_docum where id_gest_defa_docum = @ID_GEST_DEFA_DOCUM)      
      
  SELECT @TIP_GENERARE = ISNULL(TIP_DESCARCARE,0),  @ID_GEST_TIP_DOCUM = ID_GEST_TIP_DOCUM FROM GEST_DEFA_DOCUM WHERE ID_GEST_DEFA_DOCUM = @ID_GEST_DEFA_DOCUM      
  UPDATE GEST_DOCUM SET ID_DOCUMENT_CONEX = ISNULL(ID_DOCUMENT_CONEX, @ID_SURSA) FROM GEST_DOCUM WHERE ID_GEST_DOCUM = @ID_SURSA      
 

  set @NEW_ID_DOCUM = null      
  if exists(select top 1 1 FROM GEST_ITEMSI       
    WHERE ID_GEST_DOCUM = @ID_SURSA      
    and id_gest_tip_material in (select distinct id_gest_tip_material from GEST_ITEMSI_TIP_MATERIAL where id_Gest_defa_docum = @ID_GEST_DEFA_DOCUM)      
  )      
  begin      
      
      select       
    *       
      into #CULGEST_DOCUM      
      from GEST_DOCUM      
      WHERE ID_GEST_DOCUM = @ID_SURSA      
      
       
     update a set       
        ID_GEST_TIP_DOCUM = @ID_GEST_TIP_DOCUM,      
        ID_PREDATOR = CASE WHEN @TIP_GENERARE = 0 THEN ID_PREDATOR ELSE ID_PRIMITOR END,      
        ID_PRIMITOR = CASE WHEN @TIP_GENERARE = 0 THEN ID_PRIMITOR ELSE ID_PREDATOR END,      
        NR_DOCUM = isnull(NR_DOC_CONEX, case when @GenereazaNr = 1 then .dbo.fnGestNextNumarDocum(@ID_GEST_DEFA_DOCUM,null, null, null, null) else NR_DOCUM end),      
        DATA_DOCUM = isnull(DATA_DOC_CONEX,  DATA_DOCUM),        
        ID_GEST_DEFA_DOCUM= @ID_GEST_DEFA_DOCUM,      
        ID_INITIAL = @ID_SURSA,      
        ID_DOCUMENT_CONEX =(SELECT TOP 1 ISNULL(ID_DOCUMENT_CONEX, @ID_SURSA) FROM GEST_DOCUM  WHERE ID_GEST_DOCUM = @ID_SURSA),      
        STARE = 1,      
        DATA_OPERARE = getdate(),      
    AUTOGENERAT = 1        
    from #CULGEST_DOCUM a      
       
    -- facem salvarea fara sa tinem cont de structura de date care poate fi diferita de la client la client      
        
   declare @name varchar(200)      
    declare @sql nvarchar(4000)      
   set @sql = null      
        
    declare tmpCursor cursor for      
     select name from syscolumns  a      
     where id = object_id('GEST_DOCUM') and iscomputed = 0 and xtype not in (189) and status & 0x80 <> 0x80      
      and exists(select top 1 1 from tempdb..syscolumns b where id = object_id('tempdb..#CULGEST_DOCUM') and a.name = b.name)       
    open tmpCursor      
    fetch next from tmpCursor into @name       
   while @@fetch_status = 0       
    begin      
      set @sql = isnull(@sql + ', ', '') + @name      
      fetch next from tmpCursor into @name       
    end      
    close tmpCursor      
    deallocate tmpCursor      
        
    set @sql = 'insert into gest_docum(' + @sql + ')' + char(13) + char(10) +       
     ' select ' + @sql + ' from #CULGEST_DOCUM '      
     + ' select @NEW_ID_DOCUM = scope_identity()'      
    exec sp_executesql @sql, N'@NEW_ID_DOCUM int output', @NEW_ID_DOCUM output      
         
      UPDATE GEST_DOCUM SET ID_INITIAL = @NEW_ID_DOCUM, ID_TRANZACTIE = @NEW_ID_DOCUM WHERE ID_GEST_DOCUM = @NEW_ID_DOCUM      
      
    select       
      *      
    into #CULGEST_ITEMSI      
      FROM GEST_ITEMSI       
      WHERE ID_GEST_DOCUM = @ID_SURSA      
        and id_gest_tip_material in (select distinct id_gest_tip_material from GEST_ITEMSI_TIP_MATERIAL where id_Gest_defa_docum = @ID_GEST_DEFA_DOCUM)      
   
      update #CULGEST_ITEMSI set ID_GEST_DOCUM = @NEW_ID_DOCUM      
      
     set @sql = null      
      
    declare tmpCursor cursor for      
     select name from syscolumns  a      
     where id = object_id('GEST_ITEMSI') and iscomputed = 0 and xtype not in (189) and status & 0x80 <> 0x80      
      and exists(select top 1 1 from tempdb..syscolumns b where id = object_id('tempdb..#CULGEST_ITEMSI') and a.name = b.name)       
    open tmpCursor      
    fetch next from tmpCursor into @name       
   while @@fetch_status = 0       
    begin      
      set @sql = isnull(@sql + ', ', '') + @name      
      fetch next from tmpCursor into @name       
    end      
    close tmpCursor      
    deallocate tmpCursor      
        
    set @sql = 'insert into GEST_ITEMSI (' + @sql + ')' + char(13) + char(10) +       
     ' select ' + @sql + ' from #CULGEST_ITEMSI ORDER BY ID_GEST_ITEMSI '      
  --  print @sql      
    exec (@sql)      
      
      
    declare @aorder varchar(8000)      
    declare @fieldList    varchar(8000)      
    declare @fieldFormula varchar(8000)      
      
    declare tmpEvalPoz cursor for      
    select       
        '['+rtrim(ltrim(field_name))+'] = (select top 1 '+rtrim(ltrim(formula_calcul))+' from gest_itemsi aa where id_gest_docum = '+rtrim(ltrim(str(@new_id_docum)))+')'      
      from gest_defa_docum_document      
        where id_gest_defa_docum = @ID_GEST_DEFA_DOCUM and       
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
          
      update c set c.id_document_receptie = a.id_gest_docum from gest_docum a join gest_itemsi b on (a.id_gest_docum = b.id_gest_docum)        
      join gest_gnmcl c on (b.codmat = c.codmat)      
      where       
        a.stare = 1 and b.stare = 1 and a.id_gest_docum = @NEW_ID_DOCUM      
        and (c.id_document_receptie is null or not exists(select top 1 1 from gest_docum where stare = 1 and id_Gest_docum = c.id_document_receptie))      
  end      
  set nocount off      
  SELECT @NEW_ID_DOCUM AS ID_GEST_DOCUM, convert(bit, 0) as IsTemp      
        
END      

