create procedure spGestNotaCulegere (@idCulgestDocum int)
as
begin
 -- declare @NotaCulegere table
    create table  #NotaCulegere
      (id int identity(1,1),
      id_gest_docum int, id_gest_itemsi int,
      id_gest_defa_docum_nota_model int, 
      codmat int,
      ContD varchar(100), RepartitorDebit int, ContC varchar(100), RepartitorCredit int,
     CodFunctional varchar(100), CodEconomic varchar(100), idAngajamenteDefalcare int,
     valoare money, explicatie varchar(255),
     descriere_document varchar(255), cod_document varchar(20), nr_docum varchar(50),  data_document datetime,  data_scadenta datetime
    )


  declare @AORDER varchar(8000)
 
  declare TMPEVALPOZ cursor for
  select 
    ' 
  insert into #NotaCulegere (
        id_gest_docum, id_gest_itemsi, id_gest_defa_docum_nota_model, codmat,
        ContD, RepartitorDebit, ContC, RepartitorCredit,
        CodFunctional, CodEconomic, idAngajamenteDefalcare, valoare, explicatie)
  select
    gest_docum.id_culgest_docum,
    gest_itemsi.id_culgest_itemsi,
    c.id_gest_defa_docum_nota_model,
    gest_itemsi.codmat,'  
   +  c.ContDebit + ','  + c.RepartitorDebit + ','+   c.ContCredit + ','+ c.repartitorCredit + ','+
    c.codFunctional+','+ c.codEconomic+','+  c.idAngajamenteDefalcare +','+ c.formula_nota+','+ c.explicatie 
  +'
  from 
      culgest_docum as gest_docum 
      join culgest_itemsi as gest_itemsi on (gest_docum.id_culgest_docum = gest_itemsi.id_culgest_docum)
      join gest_defa_docum_nota_model c on (c.id_gest_defa_docum='+ ltrim(rtrim(str(c.id_gest_defa_docum))) + ' and gest_itemsi.id_gest_tip_material=c.id_gest_tip_material)
  where gest_docum.id_culgest_docum = ' + ltrim(rtrim(str(@idCulgestDocum))) + ' and id_gest_defa_docum_nota_model = ' + ltrim(rtrim(str(c.id_gest_defa_docum_nota_model)))
 from gest_defa_docum_nota_model c 
    where exists(select top 1 1  from culgest_docum aa join culgest_itemsi bb on (aa.id_culgest_docum = bb.id_culgest_docum) where aa.id_culgest_docum = @idCulgestDocum 
      and aa.id_Gest_defa_docum = c.id_Gest_defa_docum and bb.id_gest_tip_material = c.id_gest_tip_material
       )
   or exists(select top 1 1 from culgest_docum aa 
              join culgest_itemsi bb on (aa.id_culgest_docum = bb.id_culgest_docum) 
          where aa.id_culgest_docum = @idCulgestDocum 
            and (select top 1 c.id_gest_defa_docum 
        from (select  * from gest_defa_docum where id_gest_defa_Docum = aa.id_Gest_defa_docum)  a 
          join gest_tip_docum b on (a.id_document_conex = b.id_gest_tip_docum)
          join gest_defa_docum c on (b.id_gest_tip_docum = c.id_gest_tip_docum 
                and c.predator_intern = a.predator_intern and c.primitor_intern = a.primitor_intern)
          )   = c.id_Gest_defa_docum and bb.id_gest_tip_material = c.id_gest_tip_material
       )



          OPEN TMPEVALPOZ
          FETCH NEXT FROM TMPEVALPOZ INTO @AORDER
          WHILE @@FETCH_STATUS = 0
            BEGIN
              --PRINT @AORDER
              EXEC(@AORDER)
              FETCH NEXT FROM TMPEVALPOZ INTO @AORDER
            END
          CLOSE TMPEVALPOZ
          DEALLOCATE TMPEVALPOZ


  update a 
  set 
    a.descriere_document = isnull(d.cod_docum + ', ', '') +  ltrim(rtrim(b.nr_docum)) + ', '+ convert(varchar(10), b.data_docum, 103),
    a.cod_document = d.cod_docum,
    a.nr_docum = b.nr_docum,
    a.data_document = b.data_docum,
    a.data_scadenta = b.scadenta
  from 
    #NotaCulegere a 
    join culgest_Docum b on (a.id_gest_docum  = b.id_culgest_docum)
    join culgest_itemsi c on(b.id_culgest_docum = c.id_culgest_docum)
    join gest_tip_Docum d on (b.id_Gest_tip_Docum = d.id_gest_tip_docum)


  select * from #NotaCulegere
end

