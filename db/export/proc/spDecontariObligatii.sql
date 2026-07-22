create procedure spDecontariObligatii (@IsPlata int = null)
as
begin
  set @IsPlata = isnull(@IsPlata, 2)

  select
    a.id_document_conex as id, 
    isnull(g.id_gest_docum, a.id_gest_docum) as id_gest_docum, 
    b.cod_docum, 
    a.nr_docum, 
    a.data_docum, 
    isnull(g.totaldoc, a.totaldoc) as totaldoc, a.id_predator, a.id_primitor, c.nume as predator, d.nume as primitor, c.gestint as predator_intern, d.gestint as primitor_intern,
    isnull(g.totaldoc, a.totaldoc) - isnull((select sum(suma) from gest_decontari where id_gest_docum = isnull(g.id_gest_docum, a.id_gest_docum)), 0) as ramas, 
    isnull((select sum(suma) from gest_decontari where id_gest_docum = isnull(g.id_gest_docum, a.id_gest_docum)), 0) as asignat,
    isnull(case when isnull(g.totaldoc, a.totaldoc) > 0 then (select sum(suma) from gest_decontari where id_gest_docum = isnull(g.id_gest_docum, a.id_gest_docum)) / isnull(g.totaldoc, a.totaldoc) * 100 else 0 end,0) as procent
  from 
    gest_docum a
    join gest_tip_docum b on (a.id_gest_tip_docum = b.id_gest_tip_docum)
    left join repartitori c on (c.id_repartitori = a.id_predator)
    left join repartitori d on (d.id_repartitori = a.id_primitor)
    join gest_defa_Docum e on (a.id_gest_defa_Docum = e.id_gest_defa_docum)
    left join gest_defa_Docum f on (e.id_document_conex = f.id_gest_tip_docum and e.predator_intern = f.predator_intern and e.primitor_intern = f.primitor_intern)
    left join gest_docum g on (a.id_gest_docum <> g.id_gest_docum and a.id_document_conex = g.id_document_conex and g.id_gest_defa_docum = f.id_Gest_Defa_docum and g.stare = 1)
  where 
    a.stare=1 and a.id_gest_defa_docum in (select ID_GEST_DEFA_DOCUM from fnDocLichidare())
    AND 
     ( (@IsPlata not in (0, 1)) or 
       (@IsPlata = 1 and (d.gestint = 1 and c.gestint = 0))
      OR
      (@IsPlata = 0 and (d.gestint = 0 and c.gestint = 1)))

end   

