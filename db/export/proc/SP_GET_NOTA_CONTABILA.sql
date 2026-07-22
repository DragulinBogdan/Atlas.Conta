CREATE PROCEDURE [dbo].[SP_GET_NOTA_CONTABILA] (@NR_NOTA VARCHAR(100)= NULL, @LUNA INT=NULL, @AN INT = NULL, @JURNAL VARCHAR(100) = NULL, @CUMULAT INT = NULL,   
  @Data Datetime = null, @modul int = null, @idUtilizator int = null, @PerioadaStart datetime = null, @PerioadaEnd datetime = null,    
  @cod_functional varchar (128)=NULL, @id_oi_unitati int = NULL, @cod_economic varchar (128)=NULL, @id_oi_proiecte int=NULL) AS    
BEGIN    
    
  set @idUtilizator = isnull(@idUtilizator, -1)    
  SET @modul = ISNULL(@modul, -99)    
  DECLARE @DATA_START DATETIME     
  DECLARE @DATA_END   DATETIME    
  SET @AN = ISNULL(@AN, YEAR((select min(data_end)from perioade_fiscale)))    
    
  set @cod_functional = isnull(ltrim(rtrim(@cod_functional)), '')      
  set @cod_economic = isnull(ltrim(rtrim(@cod_economic)), '')    
  set @ID_OI_UNITATI = isnull(@ID_OI_UNITATI, -1)    
  set @ID_OI_PROIECTE = isnull(@ID_OI_PROIECTE, -1)    
  
 declare @ClasaFunctionala table (clasa varchar(128))    
  if @COD_FUNCTIONAL > ''    
    insert into @ClasaFunctionala (clasa)    
    select    
      rtrim(ltrim(clasa))+'%'    
    from bg_plan_functional a    
         join dbo.fnSplitStringList(@cod_functional) b on (a.cod_functional like rtrim(ltrim(b.id_resultat)) + '%')    
      
  declare @ClasaEconomica varchar(1000)    
  set @ClasaEconomica = null    
  if @COD_ECONOMIC > ''    
    select @ClasaEconomica = rtrim(ltrim(clasa))+'%' from bg_plan_economic where cod_economic like @COD_ECONOMIC    
  else    
    set @ClasaEconomica = null    
  
  create table #Functional (cod_functional varchar(100) primary key)    
  insert into #Functional     
  select distinct aa.cod_functional     
  from bg_plan_functional as aa    
       join @ClasaFunctionala as bb on (aa.clasa like bb.clasa)    
      
  create table #Economic (cod_economic varchar(128) primary key)    
  insert into #Economic select distinct aa.cod_economic from bg_plan_economic as aa where clasa like @ClasaEconomica    
      
  set nocount on    
  exec spRunBeforeRapNote    
  if @Data is null and @PerioadaStart is not null and @PerioadaEnd is not null    
  begin    
      SET @DATA_START = @PerioadaStart    
      SET @DATA_END   = @PerioadaEnd    
      set @AN = isnull(@AN, year(@PerioadaStart))    
  end    
  else    
  if @Data is not null    
  begin    
      SET @DATA_START = @Data    
      SET @DATA_END   = @Data    
      set @AN = isnull(@AN, year(@Data))    
      --set @NR_NOTA = null    
  end    
  else    
  IF @NR_NOTA IS NULL AND @LUNA IS NULL    
    BEGIN    
      SET @DATA_START = '01/01/'+LTRIM(RTRIM(STR(@AN)))    
      SET @DATA_END   = DATEADD(DAY, -1, '01/01/'+LTRIM(RTRIM(STR(@AN+1))))    
    END    
  ELSE    
  IF @LUNA IS NULL    
    BEGIN    
      SET @DATA_START = '01/01/'+LTRIM(RTRIM(STR(@AN)))    
      SET @DATA_END   = DATEADD(DAY, -1, '01/01/'+LTRIM(RTRIM(STR(@AN+1))))    
    END    
  ELSE    
    BEGIN    
      SET @DATA_START = CONVERT(DATETIME, '01/'+RTRIM(LTRIM(STR(@LUNA)))+'/'+LTRIM(RTRIM(STR(@AN))), 103)    
      SET @DATA_END   = DATEADD(MONTH, 1, @DATA_START)    
      SET @DATA_END   = DATEADD(DAY, -1, @DATA_END)    
    END    
    
  SELECT     
    @LUNA as Luna,    
    case @luna     
        when 1 then 'Ianuarie '    
        when 2 then 'Februarie '    
        when 3 then 'Martie '    
        when 4 then 'Aprilie '    
        when 5 then 'Mai '    
        when 6 then 'Iunie '    
        when 7 then 'Iulie '    
        when 8 then 'August '    
        when 9 then 'Septembrie '    
        when 10 then 'Octombrie '    
        when 11 then 'Noiembrie '    
        when 12 then 'Decembrie '    
        else null    
    end + LTRIM(RTRIM(STR(@AN))) as LunaStr,    
    NR,    
    COD,    
    JURNAL,    
    (select denumire from cjurnale where jurnal = ltrim(rtrim(a.JURNAL)) ) as DEN_JURNAL,    
    NRDOC,    
    DATA,    
    A.EXPLICATIE,    
    a.cod_document,           
    a.nr_document,     
    a.data_document,    
    VALOARE,    
    CONTD,    
    CONTC,    
    B.NUME,    
    B.NUMEINTREG,    
    coalesce(case when d.gestint = 0 then d.nume else null end, case when c.gestint = 0 then c.nume else null end, C.NUME, D.NUME, E.NUME) AS REPARTITOR,    
    ISNULL((SELECT DESCRIERE FROM moduleimport AA WHERE AA.MODUL > 0 AND AA.MODUL = A.MODUL), 'Note Contabile') AS DESC_NOTA,    
    D.NUME AS REPARTITOR_DEBIT,    
    E.NUME AS REPARTITOR_CREDIT,    
    F.COD_FUNCTIONAL,    
    a.id_oi_unitati,    
    G.COD_ECONOMIC,    
    a.id_oi_proiecte,    
    F.DENUMIRE AS DENUMIRE_FUNCTIONAL,    
    (select denumire from oi_unitati aa where a.id_oi_unitati = aa.id_oi_unitati) as den_unitate,    
    (select denumire from oi_proiecte bb where a.id_oi_proiecte = bb.id_oi_proiecte) as den_proiecte,    
    G.DENUMIRE AS DENUMIRE_ECONOMIC,    
    case when @data is not null then ltrim(rtrim(JURNAL))  + ' - ' + ltrim(rtrim(convert(varchar(10), @data, 103))) else '' end as descriere_luna,    
    a.modul,     
    @DATA_START as data_start,    
    @DATA_END as data_end    
  into #Result    
  FROM CNOTE A    
      LEFT JOIN UTILIZATORI B ON (A.C_O = B.ID_UTILIZATORI)    
      LEFT JOIN REPARTITORI C ON (A.CODREP = C.ID_REPARTITORI)    
      LEFT JOIN REPARTITORI D ON (A.REPARTITOR_DEBIT = D.ID_REPARTITORI)    
      LEFT JOIN REPARTITORI E ON (A.REPARTITOR_CREDIT = E.ID_REPARTITORI)     
      LEFT JOIN BG_PLAN_FUNCTIONAL F ON (A.COD_FUNCTIONAL = F.COD_FUNCTIONAL)    
      LEFT JOIN BG_PLAN_ECONOMIC G ON (A.COD_ECONOMIC = G.COD_ECONOMIC)    
  WHERE     
    DATA BETWEEN @DATA_START AND @DATA_END AND A.STARE=1    
    AND (@NR_NOTA IS NULL OR NRDOC LIKE @NR_NOTA)    
    AND (@JURNAL IS NULL OR ltrim(rtrim(JURNAL)) = ltrim(rtrim(@JURNAL)))    
    --AND CONTD IN (select cont from cplan where left(cont, 1) <> '8')    
    --AND CONTC IN (select cont from cplan where left(cont, 1) <> '8')    
    AND (@modul = -99  OR isnull(A.MODUL, -100)= @modul)    
    and (@idUtilizator = -1 or a.c_o = @idUtilizator)    
    -- and (@cod_functional = '' or (@cod_functional <> '' and isnull(a.cod_functional, '') = @cod_functional))    
 -- and (@cod_economic = '' or (@COD_ECONOMIC <> '' and isnull(a.cod_economic,'') = @cod_economic))    
 and (@COD_FUNCTIONAL = '' or     
        (isnull(cod_functional_d, a.cod_functional) in (select * from #Functional)    
          or    
         isnull(cod_functional_c, a.cod_functional) in (select * from #Functional)    
        )    
      )     
  
  and (@ClasaEconomica is null or     
          (isnull(cod_economic_d, a.cod_economic) in (select * from #Economic)    
            or    
           isnull(cod_Economic_c, a.cod_economic) in (select * from #Economic)    
          )    
        )         
    and (@id_oi_unitati = -1  or (@id_oi_unitati  <> -1 and isnull(a.id_oi_unitati, 0) = @id_oi_unitati))    
    and (@id_oi_proiecte = -1 or (@id_oi_proiecte <> -1 and isnull(a.id_oi_proiecte, 0) = @id_oi_proiecte))     
    
  set nocount off    
    
  if @CUMULAT = 1    
    select contd, contc, sum(valoare) as valoare from #Result group by contd, contc order by contd, contc    
  else    
  if @CUMULAT = 2    
    select * from #Result ORDER BY JURNAL, DATA, NRDOC, NR    
  else    
  if @CUMULAT = 3    
    select * from #Result ORDER BY  DATA, JURNAL, modul, len(NRDOC), NRDOC, NR    
   else    
  if @CUMULAT = 4      
    select nrdoc, max(convert(varchar(10), data_document, 104)) as dataDocument, max(desc_nota) as descriereNota,contd, contc,cod_functional,cod_economic, sum(valoare) as valoare from #Result 
		group by nrdoc, contd, contc, cod_functional, cod_economic order by len(NRDOC), NRDOC, contd, contc, cod_functional, cod_economic     
  else    
   if @CUMULAT = 5    
    select * from #Result ORDER BY len(NRDOC), NRDOC,  DATA, NR      
  else    
    select * from #Result ORDER BY modul, len(NRDOC), NRDOC, DATA, NR    
    
    
END    


