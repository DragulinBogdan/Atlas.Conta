CREATE procedure [dbo].[spGestAnuleazaDocum](@idGestDocum int)    
as    
begin    
    
  declare @dataOpMin datetime    
  declare @dataOpMax datetime    
      
  select codmat into #codmat from  gest_docum a join gest_itemsi b on a.id_gest_docum = b.id_gest_docum     
                                                join gest_tip_docum c on a.ID_GEST_TIP_DOCUM = c.ID_GEST_TIP_DOCUM    
     where a.ID_GEST_DOCUM = @idGestDocum and (c.den_docum  like '%factura%' or c.den_docum  like '%N%I%R%' )    
    
	if exists (select codmat from #codmat where codmat in (select codmat from  gest_docum a join gest_itemsi b on a.id_gest_docum = b.id_gest_docum     
                                                                                        join gest_tip_docum c on a.ID_GEST_TIP_DOCUM = c.ID_GEST_TIP_DOCUM    
		where a.stare =1 and data_docum >= (select data_docum from gest_docum where ID_GEST_DOCUM = @idGestDocum) and c.den_docum not like '%factura%' and c.den_docum not like '%N%I%R%' ))
		
  begin      
    RAISERROR ('Documentul nu poate fi sters deoarece produsele au fost folosite si in alte documente', 16, 1)    
    return    
  end    
    
  select @dataOpMin = min(data_docum), @dataOpMax = max(data_docum) from     
    (select isnull(data_nota, data_docum) as data_docum from gest_docum where id_gest_docum = @idGestDocum) as a    
    
  declare @data_min datetime      
  declare @data_max datetime    
    
  select @data_min = min(data_start), @data_max = max(data_end) from perioade_fiscale where inchisa = 1      
  if @data_max is null     
    select @data_max = dateadd(day, -1, min(data_start)) from perioade_fiscale    
  if @data_min is null     
    set @data_min = dateadd(year, -1, @data_max)    
    
  if @dataOpMin between @data_min and @data_max or    
     @dataOpMax between @data_min and @data_max    
  begin      
    RAISERROR ('Data este in cadrul unei perioade fiscale inchise. Nu se pot efectua modificari', 16, 1)      
    return    
  end    
    
  select @data_min = min(data_start), @data_max = max(data_end) from perioade_fiscale    
    
  if @dataOpMin < @data_min or    
     @dataOpMax > @data_max    
  begin      
    RAISERROR ('Data este nu este in cadrul exercitiului financiar definit ! Nu se pot efectua modificari', 16, 1)    
    return    
  end    
    
  DECLARE @ID_TRANZACTIE INT    
  DECLARE @ID_DOCUMENT_CONEX INT    
    
  declare @listDocs table (id_gest_docum int)    
     
  SELECT @ID_TRANZACTIE = ID_TRANZACTIE, @ID_DOCUMENT_CONEX = ID_DOCUMENT_CONEX  FROM GEST_DOCUM WHERE ID_GEST_DOCUM  = @idGestDocum    
     
  insert into @listDocs (id_gest_docum)     
    select distinct id_gest_docum from gest_docum     
      where ((ID_DOCUMENT_CONEX = @ID_DOCUMENT_CONEX AND @ID_TRANZACTIE = @ID_DOCUMENT_CONEX) OR (@ID_TRANZACTIE <> @ID_DOCUMENT_CONEX AND ID_GEST_DOCUM = @idGestDocum)) AND STARE <> 0     
    
  UPDATE GEST_ITEMSI SET STARE = 0 WHERE ID_GEST_DOCUM IN (select a.id_gest_docum from @listDocs as a)    
  UPDATE GEST_DOCUM  SET STARE = 0, DATA_STERGERE = GETDATE(), ID_UTILIZATOR_STERGERE = .DBO.FNUTILIZATORCURENT() WHERE ID_GEST_DOCUM IN (select a.id_gest_docum from @listDocs as a)    
  DELETE FROM BREGISTRU WHERE ISNULL(NR_LIST, COD) IN (SELECT ID_BREGISTRU FROM GEST_DECONTARI WHERE ISNULL(AUTOGENERAT, 0) = 1 AND ID_GEST_DOCUM IN (select a.id_gest_docum from @listDocs as a))    
  DELETE FROM GEST_DEFALCARE_DECONTARI WHERE ID_GEST_DECONTARI IN (SELECT ID_GEST_DECONTARI FROM GEST_DECONTARI WHERE ISNULL(AUTOGENERAT, 0) = 1 AND ID_GEST_DOCUM IN (select a.id_gest_docum from @listDocs as a))    
  DELETE FROM GEST_DECONTARI WHERE ISNULL(AUTOGENERAT ,0) = 1 AND ID_GEST_DOCUM IN (select a.id_gest_docum from @listDocs as a)    
    
end  

