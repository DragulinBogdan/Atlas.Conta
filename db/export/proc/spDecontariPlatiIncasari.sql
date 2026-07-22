create procedure spDecontariPlatiIncasari (@IsCasaBanca int = null, @specificCod int = null)
as
begin
/*
  @IsCasaBanca = 0 -- nu conteaza
  @IsCasaBanca = 1 -- case
  @IsCasaBanca = 2 -- banci
*/


  set nocount on 
  set @IsCasaBanca = isnull(@IsCasaBanca, 0)
  set @specificCod = isnull(@specificCod, -1)
  update bregistru set nr_list = cod where nr_list is null
  set nocount off  
  select
    isnull(nr_list, cod) as cod, c.denumire, tipdoc, nrdoc, data, explicatie, 
    case when plati is null then 0 else 1 end as tip_plata,
    isnull(incasari, plati) as total,
    isnull((select sum(suma) from gest_decontari aa join gest_docum bb on (aa.id_gest_docum = bb.id_gest_docum) where aa.id_bregistru = isnull(a.nr_list, a.cod) and bb.stare=1), 0) as asignat,
    isnull(incasari, plati) - isnull((select sum(suma) from gest_decontari aa join gest_docum bb on (aa.id_gest_docum = bb.id_gest_docum) where aa.id_bregistru = isnull(a.nr_list, a.cod) and bb.stare=1), 0) as ramas,
    isnull(case when isnull(incasari, plati) > 0 then (select sum(suma) from gest_decontari where id_bregistru = isnull(a.nr_list, a.cod)) / isnull(incasari, plati) * 100 else 0 end,0) as procent,
    a.codgest,  b.nume
  from bregistru a
       left join repartitori b on (a.codgest = b.id_repartitori)
       join casierie c on (c.cod_cb = a.cod_cb)
  where 
     isnull(a.sold_initial,0)= 0 and
     isnull(c.is_banca, 0) =  case when @iscasabanca = 0  then isnull(c.is_banca, 0) 
           when @iscasabanca = 1 then 0
           when @iscasabanca = 2 then 1 end
     and (@specificcod = -1 or @specificcod = a.cod)
  order by isnull(incasari, plati), c.denumire, data
end   

