CREATE function [dbo].[fnGetContDebitItems] (@idGestTipStock int, @idGestTipMaterial int, @clasaEconomica varchar(128)) returns varchar(64) as    
begin    
  declare @Result varchar(64)    
    
    
  set @result =     
    (select top 1 cont from gest_tip_material where id_gest_tip_material =  @idGestTipMaterial)    
    
  if (@idGestTipStock <> 1)    
     select top 1 @Result = cont_debitor from gest_defa_nota_cont where id_gest_tip_material = @idGestTipMaterial and cont_debitor like '303%'    
    
    
  if @Result is null    
    begin    
      set @Result = null    
      --select @Result = cont_debitor from gest_defa_nota_cont where id_gest_defa_docum = @idGestTipStock and id_gest_tip_material = @idGestTipMaterial and id_gest_tip_material > -1    
    
      --if @Result is null    
      --  select @Result = cont_debitor from gest_defa_nota_cont where id_gest_defa_docum = @idGestTipStock and cod_economic = @clasaEconomica and id_gest_tip_material = -1    
    
      if @Result is null    
        select @Result = cont_debitor from gest_defa_nota_cont where cod_economic = @clasaEconomica and id_gest_tip_material = @idGestTipMaterial and cont_debitor like '3%'    
    
      if (@Result is null) or (@idGestTipStock <> 1)    
        select @Result = cont_creditor from gest_defa_nota_cont where cod_economic = @clasaEconomica and id_gest_tip_material = @idGestTipMaterial and cont_creditor like '3%'    
    
    
      if @Result is null    
        select @Result = cont_debitor from gest_defa_nota_cont where cod_economic = @clasaEconomica and id_gest_tip_material = -1 and cont_debitor like '3%'    
    
      if (@Result is null)     
        select @Result = cont_creditor from gest_defa_nota_cont where cod_economic = @clasaEconomica and id_gest_tip_material = -1 and cont_creditor like '3%'     
    
      if @Result is null     
         set @Result =     
           (select top 1 case when cont_creditor like '3%' then cont_creditor else cont_debitor end as cont from gest_defa_nota_cont   
     where id_gest_tip_material = @idGestTipMaterial and (cont_debitor  like '3%' or cont_creditor like '3%')   
     group by  
        case when cont_creditor like '3%' then cont_creditor else cont_debitor end order by count(*) desc)    
    
    
    end    
    
  return @Result    
end    
