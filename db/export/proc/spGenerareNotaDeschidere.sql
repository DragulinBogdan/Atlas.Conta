CREATE procedure spGenerareNotaDeschidere (@DATASTART DATETIME, @DATAEND DATETIME, @IGNORE_IMPORTED BIT = NULL) as  
begin  
  
  set nocount on  
  
 select  
    '12' as    NR_NOTA,  
  null as    poz,   
  '12' as    NR_DOCUM,  
   null as    ID_DOCUMENT,  
  id_oi_unitati,   
    id_oi_proiecte,   
  COD_FUNCTIONAL,  
    COD_ECONOMIC,     
    @dataend   as data,  
  id_repartitori as REPARTITOR_DEBIT,  
  id_repartitori as REPARTITOR_CREDIT,  
    'Nota deschidere an ' as DOCUMENT,  
  'Nota deschidere an cont '+cont+' CF : '+cod_functional+' CE : '+cod_economic+ ' Proiect : ' + isnull((select nume from repartitori b where b.id_repartitori = a.id_oi_proiecte), '')  AS POZITIE,            
    'Nota deschidere an cont '+CONT+' CF : '+cod_functional+' CE : '+cod_economic+ ' Proiect : ' + isnull((select nume from repartitori b where b.id_repartitori = a.id_oi_proiecte), '') AS EXPLICATIE,  
     case when sold_debitor = 0 then sold_creditor else sold_debitor end AS VALOARE,            
     case when sold_creditor = 0 then cont else  '117.00.00' end AS CONT_CREDIT,            
     case when sold_debitor = 0 then cont else '117.00.00' end AS CONT_DEBIT,            
     '12' AS MODUL        
  into #NoteGenerate            
  FROM            
   solduri_repartitori a     
   where cont like '121%' or cont like '489%'  
  
   set nocount off  
     
   if object_id('tempdb..#TMP_LISTA_NOTE') is not null        
    INSERT INTO #TMP_LISTA_NOTE (        
      NR_NOTA, NR_DOCUM, id_document,COD_FUNCTIONAL, COD_ECONOMIC, DATA, REPARTITOR_DEBIT, REPARTITOR_CREDIT,         
      DOCUMENT, poz, POZITIE, EXPLICATIE, VALOARE, CONT_CREDIT, CONT_DEBIT, MODUL, id_oi_unitati, id_oi_proiecte)      
    select         
      NR_NOTA, NR_DOCUM, id_document,COD_FUNCTIONAL, COD_ECONOMIC, DATA, REPARTITOR_DEBIT, REPARTITOR_CREDIT,         
      DOCUMENT, poz, POZITIE, EXPLICATIE, VALOARE, CONT_CREDIT, CONT_DEBIT, MODUL, id_oi_unitati, id_oi_proiecte      
    from #NoteGenerate    
    
       
  --else        
 --   select *      
 --   from #NoteGenerate    
 end
