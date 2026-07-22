CREATE procedure [dbo].[spGestRebuildValidari] (@id_gest_docum int = null)
as
begin

  select * into #gest_validari_docum from 
      gest_validari_docum where id_logare_users is not null and data_validare is not null
      and (@id_gest_docum is null or id_gest_docum = @id_gest_docum)

  if @id_gest_docum is null
    truncate table GEST_VALIDARI_DOCUM 
  else
    delete from GEST_VALIDARI_DOCUM where @id_gest_docum is null or id_gest_docum = @id_gest_docum

  insert into gest_validari_docum (id_functiune, id_gest_docum, zile_gratie, prioritate, tip_validare, id_logare_users, data_validare)
  select id_functiune, id_gest_docum as id_gest_docum, zile_gratie, prioritate, tip_validare,
    case when isnull(c.auto_validare_emitent, 0)= 1 then -1 else null end,
    case when isnull(c.auto_validare_emitent, 0)= 1 then  b.data_operare else null end
  from 
    gest_template_validari a 
    join gest_docum b on (a.id_gest_defa_docum = b.id_gest_defa_docum and tip_validare & 1 = 1)
    join gest_defa_docum c on (b.id_gest_defa_docum = c.id_gest_defa_Docum)
  where 
    (@id_gest_docum is null or id_gest_docum = @id_gest_docum)
    and b.stare = 1

  update a set 
    a.id_logare_users = b.id_logare_users, a.data_validare = b.data_validare, a.tiparit = b.tiparit
  from gest_validari_docum a join #gest_validari_docum b on (a.id_functiune = b.id_functiune and a.id_gest_docum = b.id_gest_docum and a.tip_validare = b.tip_validare and a.prioritate = b.prioritate)
  where a.id_logare_users is null or a.data_validare is null
  
end

