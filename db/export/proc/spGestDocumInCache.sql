CREATE procedure [dbo].[spGestDocumInCache] (@ID_GEST_DOCUM INT, @ID_UTILIZATORI INT) AS            
BEGIN            
          
  set nocount on            
          
  declare @dataOpMin datetime          
  declare @dataOpMax datetime          
          
  select @dataOpMin = min(data_docum), @dataOpMax = max(data_docum) from           
    (select isnull(data_nota, data_docum) as data_Docum from gest_docum where id_gest_docum = @ID_GEST_DOCUM) as a          
          
  declare @data_min datetime            
  declare @data_max datetime          
          
  select @data_min = min(data_start), @data_max = max(data_end) from perioade_fiscale where inchisa = 1            
  if @data_max is null           
    select @data_max = dateadd(day, -1, min(data_start)) from perioade_fiscale          
  if @data_min is null           
    set @data_min = dateadd(year, -1, @data_max)          
          
  if @dataOpMin between @data_min and @data_max or @dataOpMax between @data_min and @data_max          
  begin            
    RAISERROR ('Data este in cadrul unei perioade fiscale inchise. Nu se pot efectua modificari', 16, 1)            
    return          
  end          
          
  select @data_min = min(data_start), @data_max = max(data_end) from perioade_fiscale          
          
  if @dataOpMin < @data_min or @dataOpMax > @data_max          
  begin            
    RAISERROR ('Data este nu este in cadrul exercitiului financiar definit ! Nu se pot efectua modificari', 16, 1)          
    return          
  end          
          
  /* daca exista ordonantare in baza facturii nu mai permitem modificarea */          
  if exists (select top  1 1 from alop_ordonantare_lichidare a join alop_ordonantare b on (a.id_alop_ordonantare = b.id_alop_ordonantare)          
             where a.id_gest_docum = @id_gest_docum and b.stare = 1 and b.validat = 1 and b.redactat = 1)          
  begin          
    RAISERROR ('Exista ordonantare pentru documentul selectat ! Anulati ordonantarea si dupa aceea modificati documentul !', 16, 1)          
    return          
  end          
            
 /* daca exista BCS nu mai permitem modificarea FCT  */        
 if exists (select top 1 1 from gest_docum a join gest_itemsi b on a.id_gest_docum = b.id_gest_docum         
       where codmat in (select codmat from gest_itemsi where id_gest_docum = @id_gest_docum)       
    and a.id_gest_docum <> @ID_GEST_DOCUM      
    and a.stare = 1      
    and data_docum > (select data_docum from gest_docum where id_gest_docum = @id_gest_docum))     
  begin          
    RAISERROR ('Exista document ulterior pentru materialele din documentul selectat! Anulati documentul ulterior si dupa aceea modificati documentul!', 16, 1)          
    return          
  end          
        
  DECLARE @NEW_ID_DOCUM INT            
  DECLARE @ID_TRANZACTIE INT          
  DECLARE @ID_DOCUMENT_CONEX INT          
          
  declare @listDocs table (id_gest_docum int)          
           
  SELECT @ID_TRANZACTIE = ID_TRANZACTIE, @ID_DOCUMENT_CONEX = ID_DOCUMENT_CONEX  FROM GEST_DOCUM WHERE ID_GEST_DOCUM  = @ID_GEST_DOCUM          
           
  insert into @listDocs (id_gest_docum)           
    select distinct id_gest_docum from gest_docum           
      where     
      ((ID_DOCUMENT_CONEX = @ID_DOCUMENT_CONEX AND @ID_TRANZACTIE = @ID_DOCUMENT_CONEX) OR (@ID_TRANZACTIE <> @ID_DOCUMENT_CONEX AND ID_GEST_DOCUM = @ID_GEST_DOCUM))     
        AND STARE > 0           
          
  DELETE FROM CULGEST_ITEMSI WHERE ID_CULGEST_DOCUM IN (SELECT ID_CULGEST_DOCUM FROM CULGEST_DOCUM WHERE ID_UTILIZATORI = @ID_UTILIZATORI)          
  DELETE FROM CULGEST_DOCUM WHERE ID_UTILIZATORI = @ID_UTILIZATORI          
            
  -- TRANSFERAM DATELE IN ZONA TAMPON            
          
  select           
    *          
  into #CULGEST_DOCUM          
  FROM GEST_DOCUM a WHERE ID_GEST_DOCUM = @ID_GEST_DOCUM          
           
  update #CULGEST_DOCUM set ID_UTILIZATORI = @ID_UTILIZATORI, ID_MODIFICARE = @ID_GEST_DOCUM          
          
 declare @name varchar(200)          
  declare @sql nvarchar(4000)          
          
 set @sql = null          
  select          
    @sql = coalesce(@sql + ', ', '') + name          
  from syscolumns as a          
    where id = object_id('CULGEST_DOCUM')          
          and iscomputed = 0            -- coloana calculata          
          and xtype not in (189)        -- timestamp          
          and status & 0x80 <> 0x80     -- identity          
         and exists (select top 1 1 from tempdb..syscolumns b where id = object_id('tempdb..#CULGEST_DOCUM') and a.name = b.name)           
          
  set @sql = 'insert into CULGEST_DOCUM(' + @sql + ')' + char(13) + char(10) + ' select ' + @sql + ' from #CULGEST_DOCUM ' + char(13) + char(10) + ' select @NEW_ID_DOCUM = scope_identity()'          
  exec sp_executesql @sql, N'@NEW_ID_DOCUM int output', @NEW_ID_DOCUM output          
          
          
          
  -- PRELUAM SI ITEMSII          
  select           
    @NEW_ID_DOCUM as ID_CULGEST_DOCUM,          
   DETALII_ANGAJAMENT = dbo.fnDenumireGestItems(a.id_gest_itemsi),          
    b.tipmat, b.denmat, b.um, b.data_cod, b.id_gest_sumator,           
   a.*          
  into #CULGEST_ITEMSI          
  FROM GEST_ITEMSI A           
       JOIN GEST_GNMCL B ON (A.CODMAT = B.CODMAT)            
  WHERE A.ID_GEST_DOCUM = @ID_GEST_DOCUM            
          
  -- PRELUAM SI ITEMSII            
  UPDATE a set           
    a.ID_UTILIZATORI = @ID_UTILIZATORI,          
  a.pret_unitar= b.pret_unitar,          
  a.PRET_RECEPTIE = b.PRET_RECEPTIE,           
  a.COTA_TVA = b.cota_tva,           
    a.pret_receptie_tva = b.PRET_RECEPTIE_TVA          
  from #CULGEST_ITEMSI a JOIN GEST_GNMCL B ON (A.CODMAT = B.CODMAT)            
          
          
          
 set @sql = null          
  select          
    @sql = coalesce(@sql + ', ', '') + name          
  from syscolumns as a          
    where id = object_id('CULGEST_ITEMSI')          
          and iscomputed = 0              --- computed          
          and xtype not in (189)          --- timestamp          
          and status & 0x80 <> 0x80       --- identity          
         and exists (select top 1 1 from tempdb..syscolumns b where id = object_id('tempdb..#CULGEST_ITEMSI') and a.name = b.name)           
          
  set @sql = 'insert into culgest_itemsi (' + @sql + ')' + char(13) + char(10) + 'select ' + @sql + ' from #culgest_itemsi order by id_gest_itemsi'          
--  print @sql          
  exec (@sql)          
          
  -- copiem si procentii pentru codurile materiale care urmeaza sa se modifice          
  insert into culgest_itemsi_procent (id_culgest_itemsi, id_angajamente_defalcare, procProcent, sumaTotalaProcent, cantitateProcent, valFacturare, codEconomicProcent, id_oi_proiecte, id_oi_unitati)          
  select          
    b.id_culgest_itemsi, a.id_angajamente_defalcare, a.procProcent, a.sumaTotalaProcent, a.cantitateProcent, a.valFacturare, a.codEconomicProcent, a.id_oi_proiecte, a.id_oi_unitati          
  from gest_gnmcl_procent a          
       join culgest_itemsi b on (a.codmat = b.codmat)          
    where b.id_culgest_docum = @new_id_docum          
          
  UPDATE GEST_ITEMSI SET STARE = 0 WHERE ID_GEST_DOCUM IN (select a.id_gest_docum from @listDocs as a)          
  UPDATE GEST_DOCUM  SET STARE = 0, DATA_STERGERE = GETDATE(), ID_UTILIZATOR_STERGERE = @ID_UTILIZATORI WHERE ID_GEST_DOCUM IN (select a.id_gest_docum from @listDocs as a)          
          
  -- Se sterge documentul de plata generat automat          
  declare @DecontariAutomate table (id_gest_decontari int, id_bregistru int)          
  insert into @DecontariAutomate (id_gest_decontari, id_bregistru) select id_gest_decontari, id_bregistru from gest_decontari where id_gest_docum = @id_gest_docum and coalesce(autogenerat, 0) = 1          
  if exists (select top 1 1 from @DecontariAutomate)          
    begin          
      update culgest_docum set decont_cod_cb = b.cod_cb, decont_nrdoc = b.nrdoc, decont_data = b.data, decont_tipdoc = tipdoc, decont_generate = 1          
      from culgest_docum a, bregistru b          
        where a.id_culgest_docum = @new_id_docum and coalesce(b.nr_list, b.cod) in (select id_bregistru from @DecontariAutomate)          
      delete bregistru where coalesce(nr_list, cod) in (select a.id_bregistru from @DecontariAutomate as a)          
      delete gest_defalcare_decontari where id_gest_decontari in (select a.id_gest_decontari from @DecontariAutomate as a)          
      delete gest_decontari where id_gest_decontari in (select a.id_gest_decontari from @DecontariAutomate as a)          
    end        
          
  delete from TethysDOCUMENTE where id_gest_docum in (select id_gest_docum from gest_docum where stare = 0) and stare = 0            
          
   exec spCulgestItemsiRecalc @NEW_ID_DOCUM    
  set nocount off          
          
end    
    
    
