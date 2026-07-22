CREATE procedure spPreiaStocuriInitiale (@baza_old varchar(100)= null, @baza_noua varchar(100) = null, @NewAn int = null ) as
begin

  declare @sql varchar(max)
  --stocuri initiale

  --inseram gest_gnmcl ce s-au adaugat in baza veche si nu sunt in cea noua
  --aici trebuie sa avem grija sa nu 
  set @sql = '
      set identity_insert ['+ @baza_noua + ']..gest_gnmcl on
      insert into ['+ @baza_noua + ']..gest_gnmcl (
            codmat, id_angajamente_defalcare,id_initial, id_utilizatori, 	pret_unitar, 	pret_receptie, 	cota_tva, 	tipmat, 	denmat, 	um,
            lohn, data_cod, data_expirare, stare, tva, id_gest_sumator, lot_fabricatie, um_suplimentara, conversie_um, pret_receptie_valuta,  cod_tarif_vamal,
            tva_amanat,  adaos, adaos_impus, categorie_grupare,  tip_valuta_receptie, cota_adaos, cota_adaos_impus, pret_receptie_tva,  id_document_intrare,
          id_document_receptie,  tmp_id_gest_docum, id_gest_tip_material, produs, nrinventar, gestintrare, id_trecere_an        
      )
      select 
            codmat, id_angajamente_defalcare,id_initial, id_utilizatori, 	pret_unitar, 	pret_receptie, 	cota_tva, 	tipmat, 	denmat, 	um,
            lohn, data_cod, data_expirare, stare, tva, id_gest_sumator, lot_fabricatie, um_suplimentara, conversie_um, pret_receptie_valuta,  cod_tarif_vamal,
            tva_amanat,  adaos, adaos_impus, categorie_grupare,  tip_valuta_receptie, cota_adaos, cota_adaos_impus, pret_receptie_tva,  id_document_intrare,
          id_document_receptie,  tmp_id_gest_docum, id_gest_tip_material, produs, nrinventar, gestintrare, codmat        
      from ['+ @baza_old + ']..gest_gnmcl a 
      where not exists(select top 1 1 from ['+ @baza_noua + ']..gest_gnmcl b where b.codmat = a.codmat)
    set identity_insert ['+ @baza_noua + ']..gest_gnmcl off '
  exec spExecuteSQL @sql, 1
    
  set @sql = '
      insert into ['+ @baza_noua + ']..gest_gnmcl (
            id_angajamente_defalcare,id_initial, id_utilizatori, 	pret_unitar, 	pret_receptie, 	cota_tva, 	tipmat, 	denmat, 	um,
            lohn, data_cod, data_expirare, stare, tva, id_gest_sumator, lot_fabricatie, um_suplimentara, conversie_um, pret_receptie_valuta,  cod_tarif_vamal,
            tva_amanat,  adaos, adaos_impus, categorie_grupare,  tip_valuta_receptie, cota_adaos, cota_adaos_impus, pret_receptie_tva,  id_document_intrare,
          id_document_receptie,  tmp_id_gest_docum, id_gest_tip_material, produs, nrinventar, gestintrare, id_trecere_an        
      )
      select 
            id_angajamente_defalcare,id_initial, id_utilizatori, 	pret_unitar, 	pret_receptie, 	cota_tva, 	tipmat, 	denmat, 	um,
            lohn, data_cod, data_expirare, stare, tva, id_gest_sumator, lot_fabricatie, um_suplimentara, conversie_um, pret_receptie_valuta,  cod_tarif_vamal,
            tva_amanat,  adaos, adaos_impus, categorie_grupare,  tip_valuta_receptie, cota_adaos, cota_adaos_impus, pret_receptie_tva,  id_document_intrare,
          id_document_receptie,  tmp_id_gest_docum, id_gest_tip_material, produs, nrinventar, gestintrare, codmat        
      from ['+ @baza_old + ']..gest_gnmcl a 
      where  exists(select top 1 1 from ['+ @baza_noua + ']..gest_gnmcl b where b.codmat = a.codmat and a.denmat <> b.denmat)
    '
  exec spExecuteSQL @sql, 1

  if object_id('tempdb..##STOCK_CONT') is not null
    drop table ##STOCK_CONT

  set @sql = '
    SELECT 
      [' + @baza_old + '].dbo.fnGetContDebitItems(a.ID_GEST_TIP_STOC, a.id_gest_tip_material, a.cod_economic) as CONT,
      a.ID_REPARTITORI ,
      a.CODMAT, 
      max(ltrim(rtrim(a.produs))) as produs,
      max(a.id_gest_tip_material) as id_gest_tip_material,
      max(a.pret_unitar) as pret_unitar,
      max(a.pret_receptie) as pret_receptie,
      max(a.pret_receptie_tva) as pret_receptie_tva,
      max(a.id_gest_sumator) as id_gest_sumator, max(DENMAT) as denmat, 
      SUM(a.STOCK) AS STOCK, 
      SUM(a.STOCKVALORIC) AS STOCKVALORIC,
      CONVERT(INT, NULL) AS ID_ANGAJAMENTE_DEFALCARE,
      CONVERT(VARCHAR(100), NULL) AS COD_FUNCTIONAL,
      CONVERT(int, NULL) AS id_oi_unitati,
      a.COD_ECONOMIC, 
      CONVERT(int, NULL) AS id_oi_proiecte,
      CONVERT(INT, NULL) AS ID_GEST_TIP_STOCK
    INTO ##STOCK_CONT
    FROM 
        [' + @baza_old + '].dbo.fnStockCodMat(''' + ltrim(rtrim(Str(@NewAn))) + '-01-01'' ) as a
        left join [' + @baza_old + '].dbo.gest_tip_material b on (a.id_gest_tip_material = b.id_gest_tip_material)
    WHERE 
        a.ID_GEST_TIP_STOC in (1, 8, 31)
        AND a.GESTINT= 1 
        AND (a.STOCKVALORIC <> 0 OR a.STOCK <> 0)
      GROUP BY 
        a.ID_REPARTITORI, a.CODMAT, a.COD_ECONOMIC,
        [' + @baza_old + '].dbo.fnGetContDebitItems(a.ID_GEST_TIP_STOC, a.id_gest_tip_material, a.cod_economic)
      HAVING 
        SUM(a.STOCK) <> 0 OR  SUM(a.STOCKVALORIC) <> 0 
  '
  exec spExecuteSQL @sql, 1

  set @sql = '  
    UPDATE A SET 
    a.id_gest_tip_material = 
        isnull((select top 1 id_gest_tip_material from [' + @baza_noua + ']..gest_tip_material aa where aa.cont like a.cont), a.id_gest_tip_material),
    a.cod_functional = (select top 1 cod_functional from [' + @baza_old + ']..gest_itemsi aa join [' + @baza_old + ']..gest_docum bb on (aa.id_gest_docum = bb.id_gest_docum)
        where aa.codmat = a.codmat and bb.id_primitor = a.id_repartitori and aa.stare = 1 and bb.stare = 1 order by data_docum -  ''01/01/' + ltrim(rtrim(str(@newan))) + ''' ),
    a.id_oi_unitati = (select top 1 id_oi_unitati from [' + @baza_old + ']..gest_itemsi aa join [' + @baza_old + ']..gest_docum bb on (aa.id_gest_docum = bb.id_gest_docum)
        where aa.codmat = a.codmat and bb.id_primitor = a.id_repartitori and aa.stare = 1 and bb.stare = 1 order by data_docum -  ''01/01/' + ltrim(rtrim(str(@newan))) + ''' ),
    a.id_oi_proiecte = (select top 1 id_oi_proiecte from [' + @baza_old + ']..gest_itemsi aa join [' + @baza_old + ']..gest_docum bb on (aa.id_gest_docum = bb.id_gest_docum)
        where aa.codmat = a.codmat and bb.id_primitor = a.id_repartitori and aa.stare = 1 and bb.stare = 1 order by data_docum -  ''01/01/' + ltrim(rtrim(str(@newan))) + ''' ),
    a.id_angajamente_defalcare = 
      (select top 1 id_angajamente_defalcare from [' + @baza_old + ']..gest_itemsi aa join [' + @baza_old + ']..gest_docum bb on (aa.id_gest_docum = bb.id_gest_docum)
      where aa.codmat = a.codmat and bb.id_primitor = a.id_repartitori and aa.stare = 1 and bb.stare = 1 order by data_docum -  ''01/01/' + ltrim(rtrim(str(@newan))) + ''' ),
    a.id_gest_tip_stock = 
      (select top 1 id_gest_tip_stock from [' + @baza_old + ']..gest_itemsi aa join [' + @baza_old + ']..gest_docum bb on (aa.id_gest_docum = bb.id_gest_docum)
      where aa.codmat = a.codmat and bb.id_primitor = a.id_repartitori and aa.stare = 1 and bb.stare = 1 order by data_docum -  ''01/01/' + ltrim(rtrim(str(@newan))) + ''' )
  from ##stock_cont a
  '
  exec spExecuteSQL @sql, 1

  set @sql = '
  alter table ['+ @baza_noua + ']..gest_docum disable trigger all
  delete from ['+ @baza_noua + ']..gest_docum where data_docum = dateadd(day, -1, ''' + ltrim(rtrim(Str(@NewAn))) + '-01-01'')
                                                    and id_gest_tip_docum in (select top 1 id_gest_tip_docum from ['+ @baza_noua + ']..gest_tip_docum where cod_docum = ''ldi'')
  delete a from ['+ @baza_noua + ']..gest_itemsi a  where not exists(select top 1 1 from ['+ @baza_noua + ']..gest_docum b where a.id_gest_docum = b.id_Gest_docum)
  insert into ['+ @baza_noua + ']..gest_docum 
    (ID_GEST_TIP_DOCUM, ID_PREDATOR, ID_PRIMITOR, NR_DOCUM, DATA_DOCUM, TOTALDOC, TOTALTVA, ID_UTILIZATORI, EXPLICATIE,
    ID_GEST_DEFA_DOCUM, VALIDAT, ID_INITIAL, STARE, DATA_OPERARE, ID_MODIFICARE, id_document_conex, ID_TRANZACTIE)
  select
    (SELECT TOP 1 ID_GEST_TIP_DOCUM FROM ['+ @baza_noua + ']..GEST_TIP_DOCUM WHERE COD_DOCUM = ''LDI'') AS ID_GEST_TIP_DOCUM,
    id_repartitori  AS ID_PREDATOR,
    id_repartitori  AS ID_PRIMITOR,
    ''' + ltrim(rtrim(Str(@NewAn))) + ''' AS NR_DOCUM,
    dateadd(day, -1, ''' + ltrim(rtrim(Str(@NewAn))) + '-01-01'') AS DATA_DOCUM,
    NULL AS TOTALDOC,
    NULL AS TOTALTVA,
    (SELECT TOP 1 ID_UTILIZATORI FROM ['+ @baza_noua + ']..UTILIZATORI WHERE NUME LIKE ''Admin%'') AS ID_UTILIZATORI,
    ''LDI ADMINISTRATIV'' AS EXPLICATIE,
    (SELECT TOP 1 ID_GEST_DEFA_DOCUM FROM ['+ @baza_noua + ']..GEST_DEFA_DOCUM WHERE ID_GEST_TIP_DOCUM IN (SELECT TOP 1 ID_GEST_TIP_DOCUM FROM ['+ @baza_noua + ']..GEST_TIP_DOCUM WHERE COD_DOCUM = ''LDI'')) AS ID_GEST_DEFA_DOCUM,
    1 AS VALIDAT,
    NULL AS ID_INITIAL,
    1 AS STARE,
    GETDATE() AS DATA_OPERARE,
    NULL AS ID_MODIFICARE,
    NULL AS id_document_conex,
    NULL AS ID_TRANZACTIE
  from 
    ##STOCK_CONT 
  group by id_repartitori
  alter table ['+ @baza_noua + ']..gest_docum enable trigger all'
 
  exec spExecuteSQL @sql, 1

  set @sql = '
    alter table ['+ @baza_noua + ']..GEST_ITEMSI disable trigger all
    INSERT INTO ['+ @baza_noua + ']..GEST_ITEMSI(
      ID_GEST_DOCUM,  ID_ANGAJAMENTE_DEFALCARE,  CODMAT,  ID_GEST_TIP_MATERIAL,  ID_GEST_TIP_STOCK,  ID_UTILIZATORI,  STOCK_BEFORE,  CANTITATE,
      STOCK_AFTER,  PRET_UNITAR,  PRET_RECEPTIE,  COTA_TVA,  PRET_RECEPTIE_TVA,  PRET_LIVRARE,  PRET_LIVRARE_TVA,   
      STARE,  TVA,  PRODUS,  COD_FUNCTIONAL,  ID_OI_UNITATI, COD_ECONOMIC, ID_OI_PROIECTE, TVA_LIVRARE,  TVA_RECEPTIE,  VALOARE_LIVRARE,  VALOARE_LIVRARE_TVA,  
      VALOARE_RECEPTIE,  VALOARE_RECEPTIE_TVA
    )
    SELECT 
        ( SELECT TOP 1 ID_GEST_DOCUM FROM ['+ @baza_noua + ']..GEST_DOCUM WHERE STARE= 1 AND NR_DOCUM = ''' + ltrim(rtrim(Str(@NewAn))) + ''' AND DATA_DOCUM = dateadd(day, -1, ''01/01/' + ltrim(rtrim(Str(@NewAn))) + ''') AND 
          ID_GEST_TIP_DOCUM IN (SELECT TOP 1 ID_GEST_TIP_DOCUM FROM ['+ @baza_noua + ']..GEST_TIP_DOCUM WHERE COD_DOCUM = ''LDI'') and id_predator = a.id_repartitori
        ) AS ID_GEST_DOCUM, 
      a.ID_ANGAJAMENTE_DEFALCARE, a.CODMAT,  a.ID_GEST_TIP_MATERIAL,  a.ID_GEST_TIP_STOCK,    
      (SELECT TOP 1 ID_UTILIZATORI FROM ['+ @baza_noua + ']..UTILIZATORI WHERE NUME LIKE ''Admin%'') AS ID_UTILIZATORI,  
      0 as STOCK_BEFORE,  a.stock as CANTITATE,  a.stock as STOCK_AFTER,  
      a.PRET_UNITAR as PRET_UNITAR,  a.PRET_RECEPTIE,  
      case when a.PRET_UNITAR = a.PRET_RECEPTIE_TVA then 0 else B.COTA_TVA end as COTA_TVA,  
      a.PRET_RECEPTIE_TVA,  a.PRET_RECEPTIE as PRET_LIVRARE,  a.PRET_RECEPTIE_TVA as PRET_LIVRARE_TVA,  
      1 as STARE,  a.STOCKVALORIC - a.PRET_RECEPTIE_TVA * a.STOCK as TVA,  
      a.produs,  a.cod_functional,  a.id_oi_unitati, a.cod_economic, a.id_oi_proiecte,
      a.STOCKVALORIC - a.PRET_RECEPTIE_TVA * a.STOCK as TVA_LIVRARE,  a.STOCKVALORIC - a.PRET_RECEPTIE_TVA * a.STOCK as TVA_RECEPTIE,  
      a.PRET_UNITAR * a.STOCK as VALOARE_LIVRARE,  a.STOCKVALORIC as VALOARE_LIVRARE_TVA,  
      a.PRET_UNITAR * a.STOCK as  VALOARE_RECEPTIE,  a.STOCKVALORIC as VALOARE_RECEPTIE_TVA
        
    FROM
      ##STOCK_CONT a
      JOIN ['+ @baza_noua + ']..GEST_GNMCL b ON (a.CODMAT = isnull(b.id_trecere_an, b.CODMAT))  
    alter table ['+ @baza_noua + ']..GEST_ITEMSI enable trigger all
  '
  exec spExecuteSQL @sql, 1 
  
  set @sql = '
      alter table ['+ @baza_noua + ']..gest_docum disable trigger all
      update a 
      set 
        id_modificare = id_gest_docum, id_initial = id_gest_docum, id_document_conex = id_gest_docum, id_tranzactie = id_gest_docum,
        totaldoc = (select sum(valoare_receptie_tva) from ['+ @baza_noua + ']..gest_itemsi b where a.id_gest_docum = b.id_gest_docum and b.stare = 1 )
      from ['+ @baza_noua + ']..gest_docum  a 
      where nr_docum = ''' + ltrim(rtrim(str(@newan))) + ''' and data_docum = dateadd(day, -1, ''' + ltrim(rtrim(str(@newan))) + '-01-01'')
        and id_gest_tip_docum in (select top 1 id_gest_tip_docum from ['+ @baza_noua + ']..gest_tip_docum where cod_docum = ''ldi'')
    alter table ['+ @baza_noua + ']..gest_docum enable trigger all
    '
  exec spExecuteSQL @sql, 1

end

