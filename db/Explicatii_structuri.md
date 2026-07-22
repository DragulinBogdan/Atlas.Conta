Tabele Folosite si interpretarea lor

Structuri Mari:
  1. Entitate in care toti posibili predatori si primitori ai unui document.
    Poate fi partener extern in calitate de furnizor, client, gestiune interna, angajat, centru de cost, etc.
    REPARTITORI_TIPURI      : Tabela de tipuri
    REPARTITORI             : Tabela entitati
    REPARTITORI_CLASIFICATI : Tabela tipuri entitati  manytomany 
  2. Documente
    GEST_TIP_DOCUM            : Tabela tipuri de documente
    GEST_DEFA_DOCUM           : Tipuri de documente in functie de predator si primitor (in versiunea curenta doar : extern / intern)
        *) cred ca am putea pastra o singura tabela si cu o tabele separate cu tipuri de repartitori la predator si tipuri de repartitori la primitori
    GEST_DEFA_DOCUM_DOCUMENT  : Ce campuri si formule sunt la nivel de document
    GEST_DEFA_DOCUM_ITEMSI    : Ce campuri si formule sunt la nivel de pozitie de document

    GEST_DOCUM              : Tabela de documente
    GEST_ITEMSI             : Tabela de pozitii la nivel de document

    GEST_TIP_PRODUSE        : Grupa mare de produse (nivelul 1 sau 2 din planul de conturi)
    GEST_TIP_MATERIAL       : Tipul de material nivel final din plan de conturi fara analitice
    GEST_GNMCL              : Nomenclator de produse
        - PRODUS = GEST_TIP_PRODUSE.TIP_PRODUS
        - ID_GEST_TIP_MATERIAL  = GEST_TIP_MATERIAL.ID_GEST_TIP_MATERIAL
        - ID_GEST_SUMATOR       = gest_sumator.ID_GEST_SUMATOR
          *) ID_GEST_SUMATOR - era gandit ca element de grupare pentru stock (caracteristici similare tehnico-functionale)

    GEST_DEFA_STOC_TIP_PRODUSE  : Descrierea regulilor de stocuri pe tipuri de document
    vStockAll                   : view care aplica regulile de stock

    GEST_DEFA_NOTA_CONT         : Reguli de generare note contabile din documente

  3. Contabilitate:
    CPLAN                       : Planul de conturi
      - SUMATOR                 : Daca se insumeaza direct pe parinti sau se soldeaza pe parinti
      - FCTCONT                 : D - Debit, C - Credit, B - Bifunctional
      - TIP                     : S - Sintetic A - Analitic (doar conturi sintetice in plan, Analiticele se deriva)
      - BALANTA                 : Cum se poate defalca contul : R - Repratitor, M - Material, E - Executie, B - Buget, F - Sursa de finantare, P - Proiect
    CPLAN_DEFALCARE             : Explicatii defalcare pentru selectie rapida
    CNOTE                       : Note contabile
      Nota contabila are:
        - CONTD+CONT_DEBT si CONTC+CONT_CRED -> era gandit impreuna cu COMPUSA daca eveam nota compusa : CONTC = '%', CONTD = '401', COMPUSA = 1 liniile urmatoare pentru acelasi COD aveau COMPUSA = 0 si CONT_CRED si CONT_DEBT completate. Linia cu '%' era doar de afisare
        - perechi de atribute care tindeau catre urmatoarea regula :
          3 campuri cu _DEBIT/_D, _CREDIT/_C si fara ex: cod_functional, cod_functional_d si cod_functional_c cu precedenta :
            cand se citea pe debit -> coalesce(cod_functional_d, cod_functional) si pe credit : coalesce(cod_functional_c, cod_functional)
            alte atribute : 
              - codget + RAPARITOR_DEBIT + REPATITOR_CREDIT
              - cod_economic + cod_economic_d + cod_economic_c
              - id_oi_unitati + id_oi_unitati_d + id_oi_unitati_c
              ...
            nu este consecventa denumirea dar ideea de baza era sa avem cele 3 forme ale dimensiunii cu fallback dinspre debit si credit

      In mod normal ele trebuiau sa fie sincronizate cu tipurile de defalcare ale contului din plan.
      In balanta se genereaza automat analitic in functie de tipul de defalcare al contului si coduri de la nivelul id-ului (dimensiunii) ex : 401.01.<cod_fiscal> -> analitic pana la nivel de cod fiscal

      Modul curent acoperea marea majoritate cerintelor contabile si de executie. Ar fi trebuit separat sursa finantare pentru bugetari - acum se extrage din codul de sector al clasificatiei functionale printr-o functie de mapare ( a fost folosit abuziv insa codul de sector fiind unic a functionat ca discriminator)
      

      Documentele genereaza automat note contabile si pentru asta trebuiau toate elementele/dimensiunile gestionate la nivel de items si propagate pana la nivel de nota contabila

      Centrul de cost nu era pastrat la nivel de nota ci prin calitatea repartitorului ca si centru de cost.

      Ce m-a durut : 
        - trasabilitate/viteza/reproductibilitate -> evaluare o facem in sql-uri : punea presiune pe serverul de sql si devenea greu de intretinut.
        - chiar daca recalculul era consecvent era greu de gestionat la documente operate anterior
      
      Solutie luata in calcul :
        - realizarea de registre persistate pe baza regulilor si formulelor defalcate pe dimensiuni 
        - folosirea de tabele finale stocuri, solduri defalcate pe dimensiuni
        
        
      