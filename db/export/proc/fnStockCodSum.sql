create function fnStockCodSum (@dataStock datetime) returns table as
return
(
select
  id_repartitori,
  gestiune,
  gestint,
  produs,
  id_gest_tip_material,
  max(cod_economic)        as cod_economic,
  count(distinct codmat)   as nrCodMat,
  tipmat,
  denmat,
  min(nr_docum)            as nr_docum,
  min(data_docum)          as data_cod,
  min(pret_unitar)         as pret_unitar,
  min(pret_receptie)       as pret_receptie,
  min(pret_receptie_tva)   as pret_receptie_tva,
  id_gest_tip_stoc,
  id_gest_nivel_stoc,
  id_gest_grupa_stoc, 
  min(codmat)              as codmat,
  id_gest_sumator,
  sum(stock)               as stock,
  sum(stockValoric)        as stockValoric
from vStockAll
where 
  convert(datetime, floor(convert(float, data_docum))) < @dataStock
group by 
  id_repartitori,
  gestiune,
  gestint,
  produs,
  tipmat,
  denmat,
  id_gest_tip_material,
  id_gest_tip_stoc,
  id_gest_nivel_stoc,
  id_gest_grupa_stoc, 
  id_gest_sumator
)


