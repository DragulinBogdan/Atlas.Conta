create procedure [dbo].[sp_gest_get_def_stoc]
as
begin

  set nocount on
  select id_gest_tip_stoc, denumire, descriere into #tmp_def_stoc from gest_tip_stoc
  declare @alter_stm varchar(1000)
  select * into #GEST_DEFA_STOC_TIP_PRODUSE from GEST_DEFA_STOC_TIP_PRODUSE
  if not exists(select top 1 1 from tempdb..syscolumns where name like 'id_gest_defa_Docum' and id = object_id('tempdb..#GEST_DEFA_STOC_TIP_PRODUSE')) 
  begin
     alter table #GEST_DEFA_STOC_TIP_PRODUSE add id_gest_defa_Docum int
    update a set a.id_gest_defa_docum = b.id_gest_defa_docum from #GEST_DEFA_STOC_TIP_PRODUSE a join gest_defa_docum b on (a.id_gest_tip_docum = b.id_gest_tip_docum)
  end 



  --select * from sysobjects where name like ''
  declare @id_gest_defa_docum int

  declare #tmp_cursor cursor for select b.id_gest_defa_docum	from 
		gest_tip_docum a
		join gest_defa_docum b on (a.id_gest_tip_docum = b.id_gest_tip_docum)
	where 
		a.stare = 1

  open #tmp_cursor
  fetch next from #tmp_cursor into @id_gest_defa_docum
  while @@fetch_status = 0  begin
    set @alter_stm = 'ALTER TABLE #tmp_def_stoc ADD DOC_' + LTRIM(RTRIM(STR(@id_gest_defa_docum))) + '_PD_PLUS INT'
    EXEC (@alter_stm)
    set @alter_stm = 'ALTER TABLE #tmp_def_stoc ADD DOC_' + LTRIM(RTRIM(STR(@id_gest_defa_docum))) + '_PD_MINUS INT'
    EXEC (@alter_stm)
    set @alter_stm = 'ALTER TABLE #tmp_def_stoc ADD DOC_' + LTRIM(RTRIM(STR(@id_gest_defa_docum))) + '_PM_PLUS INT'
    EXEC (@alter_stm)
    set @alter_stm = 'ALTER TABLE #tmp_def_stoc ADD DOC_' + LTRIM(RTRIM(STR(@id_gest_defa_docum))) + '_PM_MINUS INT'
    EXEC (@alter_stm)
    SET @alter_stm = 'UPDATE a set a.DOC_' + LTRIM(RTRIM(STR(@id_gest_defa_docum))) + '_PD_PLUS = isnull(b.semn * b.SEMN_ITEMS,0)  from #tmp_def_stoc a join #GEST_DEFA_STOC_TIP_PRODUSE b on (a.id_gest_tip_stoc = b.id_gest_tip_stoc) where SEMN_ITEMS = 1 and predator = 1 and b.id_gest_defa_docum = ' +  LTRIM(RTRIM(STR(@id_gest_defa_docum)))
    print (@alter_stm)
    exec (@alter_stm)
    SET @alter_stm = 'UPDATE a set a.DOC_' + LTRIM(RTRIM(STR(@id_gest_defa_docum))) + '_PD_MINUS = isnull(b.semn * b.SEMN_ITEMS,0)  from #tmp_def_stoc a join #GEST_DEFA_STOC_TIP_PRODUSE b on (a.id_gest_tip_stoc = b.id_gest_tip_stoc) where SEMN_ITEMS = -1 and predator = 1 and b.id_gest_defa_docum = ' +  LTRIM(RTRIM(STR(@id_gest_defa_docum)))
    exec (@alter_stm)
    SET @alter_stm = 'UPDATE a set a.DOC_' + LTRIM(RTRIM(STR(@id_gest_defa_docum))) + '_PM_PLUS = isnull(b.semn * b.SEMN_ITEMS,0)  from #tmp_def_stoc a join #GEST_DEFA_STOC_TIP_PRODUSE b on (a.id_gest_tip_stoc = b.id_gest_tip_stoc) where SEMN_ITEMS = 1 and predator = 2 and b.id_gest_defa_docum = ' +  LTRIM(RTRIM(STR(@id_gest_defa_docum)))
    exec (@alter_stm)
    SET @alter_stm = 'UPDATE a set a.DOC_' + LTRIM(RTRIM(STR(@id_gest_defa_docum))) + '_PM_MINUS = isnull(b.semn * b.SEMN_ITEMS,0) from #tmp_def_stoc a join #GEST_DEFA_STOC_TIP_PRODUSE b on (a.id_gest_tip_stoc = b.id_gest_tip_stoc) where SEMN_ITEMS = -1 and predator = 2 and b.id_gest_defa_docum = ' +  LTRIM(RTRIM(STR(@id_gest_defa_docum)))
    exec (@alter_stm)
    fetch next from #tmp_cursor into @id_gest_defa_docum
  end

  close #tmp_cursor
  deallocate #tmp_cursor
  set nocount off
  select * from #tmp_def_stoc

end

