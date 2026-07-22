CREATE function [dbo].[fnStockCodMat] (@dataStock datetime) returns table as    
return    
(    
select    
  id_repartitori,    
  min(gestiune)    as gestiune,    
  gestint,    
  produs,    
  id_gest_tip_material,    
  max(cod_economic) as cod_economic,    
  max(cod_functional) as cod_functional,    
  max(id_analitic) as id_analitic,    
  max(id_oi_unitati) as id_oi_unitati,    
  max(id_oi_proiecte) as id_oi_proiecte,    
  1                 as nrCodMat,    
  tipmat,    
  denmat,    
  min(nr_docum)    as nr_docum,    
  --min(data_docum)       as data_cod,
	min(data_cod) as data_cod,
  min(pret_unitar)        as pret_unitar,    
  min(pret_receptie)      as pret_receptie,    
  min(pret_receptie_tva)  as pret_receptie_tva,    
  id_gest_tip_stoc,    
  id_gest_nivel_stoc,    
  id_gest_grupa_stoc,     
  codmat,    
  id_gest_sumator         as id_gest_sumator,    
  sum(stock)              as stock,    
  sum(stockValoric)       as stockValoric,    
  max(um) as um    
from vStockAll    
  where data_docum < dateadd(day, 1, convert(datetime, floor(convert(float, @dataStock))))  
--  and id_gest_tip_material in (select distinct id_gest_tip_material from gest_defa_nota_cont where cont_debitor like '302%' or cont_creditor like '302%' )    
group by     
  id_repartitori,    
  gestint,    
  produs,    
  tipmat,    
  denmat,    
--  cod_economic,    
  id_gest_sumator,    
  id_gest_tip_material,    
  id_gest_tip_stoc,    
  id_gest_nivel_stoc,    
  id_gest_grupa_stoc,     
  codmat    
)   
