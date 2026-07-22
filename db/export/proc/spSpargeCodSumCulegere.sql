CREATE procedure spSpargeCodSumCulegere (@id_culgest_docum int) as
begin

  SET NOCOUNT ON
--  TRUNCATE TABLE log_calcul 
  
  declare @newId            int
  declare @idItemsi         int
  declare @cantitate        money
  declare @idStock          int
  declare @idSumator        int
  declare @idPredator       INT
  declare @idPrimitor       int
  declare @dataDoc          datetime
  declare @codMat           int
  declare @stock            money
  declare @pretUnitar       money
  declare @idStockPredator  int
  declare @idDefaDocum      int
  declare @aorder           varchar(8000)
  DECLARE @semnCantiate     int
  
  --ce s-a scos pana acum ... daca am mai multe codmaturi cu acelasi sumator in ecran
  DECLARE @sumatorLocal TABLE (id_gest_sumator INT, codmat INT, cantitate money)

  select @idDefaDocum = a.id_gest_defa_docum, @dataDoc = a.data_docum, @idPredator = a.id_predator, @idPrimitor = a.id_primitor, @idStockPredator = b.id_gest_tip_stoc_predator 
  from culgest_docum a
       join gest_defa_docum b on (a.id_gest_defa_docum = b.id_gest_defa_docum)
    where id_culgest_docum = @id_culgest_docum

  declare tmpSpargeCodMat cursor static for
    select id_culgest_itemsi, id_gest_sumator, cantitate, isnull(id_gest_tip_stock, @idStockPredator), semn_cantitate
    from culgest_itemsi where id_culgest_docum = @id_culgest_docum
  open tmpSpargeCodMat
  fetch next from tmpSpargeCodMat into @idItemsi, @idSumator, @cantitate, @idStock, @semnCantiate
  while @@fetch_status = 0
    BEGIN
      -- extragem stock-ul pe codul sumator si realizam scaderea in functie de lifo, fifo
      declare tmpSpargere cursor static for
        select codmat, stock, pret_unitar from fnStockCodMat(@dataDoc) 
        where 
            id_repartitori = CASE WHEN @semnCantiate = -1 THEN @idPrimitor ELSE  @idPredator  end
            and id_gest_tip_stoc = @idStock and id_gest_sumator = @idSumator
        order by data_cod DESC, codmat
-- pentru fifo se debifeaza linia urmatoare sau se elimina desc din orderul de mai sus
--            order by b.data_cod 
      open tmpSpargere
      fetch next from tmpSpargere into @codMat, @stock, @pretUnitar
      --actualizam stocul cu ce s-a scos pana acum
      PRINT 'id_culgest_itemsi ' + STR(@idItemsi)
      PRINT 'cantitate ' + STR(@cantitate)

      
      while @@fetch_status = 0 and @cantitate > 0
        BEGIN
           PRINT '----------------------------'      
           PRINT 'codmat ' + STR(@codmat)
           PRINT 'stock REAL ' + STR(@stock)      
        
           SET @stock = @stock - ISNULL((SELECT SUM(cantitate) FROM @sumatorLocal WHERE codmat = @codmat), 0)        
           PRINT 'cantitate de scos ' + STR(@cantitate)
           PRINT 'stock RAMAS : ' + STR(@stock)
          
          IF @stock = 0 GOTO codmaturmator
          
          --INSERT INTO log_calcul SELECT 'codmat '  + STR(@codmat) + ' cu stock ' + STR(@stock) + ' din cantitate '  + STR(@cantitate)
          -- Adaugam in limita stock-ului
          if @cantitate < @stock
            set @stock = @cantitate

          insert into culgest_itemsi (id_culgest_docum, id_angajamente_defalcare, codmat, id_gest_tip_material, id_gest_tip_stock, id_utilizatori, stock_before, cantitate, stock_after,
              pret_unitar, pret_receptie, cota_tva, pret_receptie_tva, pret_livrare, pret_livrare_tva, pret_livrare_valuta, lohn, data_expirare, 
              tva, id_gest_sumator, produs, cod_functional, cod_economic, lot_fabricatie, um_suplimentara, conversie_um, cantitate_suplimentara, 
              pret_receptie_valuta,tva_livrare, tva_receptie, valoare_livrare, valoare_livrare_tva, valoare_receptie_valuta, valoare_livrare_valuta, 
              valoare_receptie, valoare_receptie_tva,cod_tarif_vamal, tva_amanat, adaos, cantitate_estimata, adaos_impus, categorie_grupare, 
              tip_valuta_receptie, tip_valuta_livrare, cota_adaos, cota_adaos_impus, semn_cantitate)
          select 
              @id_culgest_docum as id_culgest_docum, id_angajamente_defalcare, @codmat, id_gest_tip_material, id_gest_tip_stock, id_utilizatori, 
              @cantitate as stock_before, 
              @stock     as cantitate, 
              @cantitate - @stock as stock_after, 
              @pretUnitar as pret_unitar, 
              pret_receptie, cota_tva, pret_receptie_tva, pret_livrare, pret_livrare_tva, pret_livrare_valuta, lohn, data_expirare, tva,
              id_gest_sumator, produs, cod_functional, cod_economic, lot_fabricatie, um_suplimentara, conversie_um, cantitate_suplimentara, 
              pret_receptie_valuta, tva_livrare, tva_receptie, valoare_livrare, valoare_livrare_tva, valoare_receptie_valuta, valoare_livrare_valuta, 
              valoare_receptie, valoare_receptie_tva,cod_tarif_vamal, tva_amanat, adaos, cantitate_estimata, adaos_impus, categorie_grupare,
              tip_valuta_receptie, tip_valuta_livrare, cota_adaos, cota_adaos_impus, semn_cantitate
          from culgest_itemsi
              where id_culgest_itemsi = @idItemsi

          set @newId = ident_current('culgest_itemsi')

          -- actualizam formulele de calcul pentru pozitia nou adaugata (executam formulele de calcul de la nivelul itemsi-lor
          declare tmpEvalPoz cursor for
            select 
              'update culgest_itemsi set ['+rtrim(ltrim(field_name))+'] = '+rtrim(ltrim(formula_calcul))+' where id_culgest_itemsi = '+rtrim(ltrim(@newId))
            from gest_defa_docum_itemsi
              where id_gest_defa_docum = @idDefaDocum /*and semn_items = sign(@cantitate)*/ and rtrim(ltrim(formula_calcul)) > ''
            order by ISNULL(precedenta, 0)
          open tmpEvalPoz
          fetch next from tmpEvalPoz into @aorder
          while @@fetch_status = 0
            BEGIN
              exec(@aorder)
              fetch next from tmpEvalPoz into @aorder
            end
          close tmpEvalPoz
          deallocate tmpEvalPoz

          set @cantitate = @cantitate - @stock    
          INSERT INTO @sumatorLocal (id_gest_sumator, codmat,  cantitate) VALUES (@idSumator, @codmat,  @stock)
   codmaturmator:    
      fetch next from tmpSpargere into @codMat, @stock, @pretUnitar
        END
        
      close tmpSpargere
      deallocate tmpSpargere

      delete from culgest_itemsi where id_culgest_itemsi = @idItemsi

      fetch next from tmpSpargeCodMat into @idItemsi, @idSumator, @cantitate, @idStock
    end
  close tmpSpargeCodMat
  deallocate tmpSpargeCodMat
  SET NOCOUNT OFF
END


