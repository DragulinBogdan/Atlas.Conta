CREATE view [dbo].[vStockAll] as          
select          
  c.codmat,          
  c.id_gest_sumator,          
  case when e.predator = 1 then a.id_predator else a.id_primitor end as id_repartitori,          
  case when e.predator = 1 then g.nume else h.nume end       as gestiune,          
  case when e.predator = 1 then g.gestint else h.gestint end as gestint,          
  a.id_predator,          
  a.id_primitor,          
  g.nume as gest_predator,          
  h.nume as gest_primitor,          
  b.produs,          
  i.cod_docum,          
  a.id_gest_defa_docum,          
  a.id_gest_tip_docum,          
  a.id_gest_docum,          
  b.id_gest_itemsi,          
  a.id_utilizatori,          
  a.nr_docum,          
  a.data_docum,          
  c.tipmat,          
  c.denmat,          
  c.um,          
  b.pret_unitar,          
  b.pret_receptie,          
  b.pret_receptie_tva,          
  b.valoare_receptie,          
  b.valoare_receptie_tva,          
  b.tva_receptie,          
  b.cod_functional,        
  b.id_analitic,        
  b.id_oi_unitati,        
  b.id_oi_proiecte,        
  b.cod_economic,          
  b.id_gest_tip_material,          
  f.id_gest_tip_stoc    as id_gest_tip_stoc,          
  f.id_gest_nivel_stoc,          
  f.id_gest_grupa_stoc,          
  e.semn,          
  e.semn * b.cantitate              as stock,          
  e.semn * isnull(b.valoare_receptie_tva, b.cantitate * c.pret_receptie_tva) as stockValoric,        
  a.data_operare,  
 c.data_expirare,
 c.data_cod
from           
  gest_docum a with (nolock)        
  join gest_itemsi b with (nolock) on (a.id_gest_docum = b.id_gest_docum)          
  join gest_gnmcl c with (nolock) on (c.codmat = b.codmat)          
  join gest_tip_produse d with (nolock) on (d.tip_produs = b.produs)          
  join gest_defa_stoc_tip_produse e with (nolock) on (e.id_gest_defa_docum = a.id_gest_defa_docum and e.id_gest_tip_produse = d.id_gest_tip_produse)          
  join gest_tip_stoc f  with (nolock) on (e.id_gest_tip_stoc = f.id_gest_tip_stoc)          
  join repartitori g with (nolock) on (g.id_repartitori = a.id_predator)          
  join repartitori h with (nolock) on (h.id_repartitori = a.id_primitor)          
  join gest_tip_docum i with (nolock) on (i.id_gest_tip_docum = a.id_gest_tip_docum)          
where      
  case when b.cantitate>=0 then 1 else -1 end = e.semn_items and      
   a.stare=1 and b.stare=1          
--  and b.produs in ('M', 'P', 'O', 'OF')      
