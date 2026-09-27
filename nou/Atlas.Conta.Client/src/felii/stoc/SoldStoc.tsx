import { useMemo } from 'react';
import { Column, DataGrid, FilterRow, HeaderFilter, Pager, Paging, Search, Sorting, Summary, TotalItem } from 'devextreme-react/data-grid';
import { storeRemote } from '../../nucleu/dxStore';

// Soldurile și totalurile sunt calculate pe cub, exclusiv pe server.
export function SoldStoc() {
  // Cheia proiecției e COMPUSĂ — soldul se ține per `Lot × Cont × Produs × Gestiune`
  // (D9); `LotId` singur nu identifică un rând.
  const sursa = useMemo(() => storeRemote('/api/proiectii/sold-stoc', ['LotId', 'ContId', 'ProdusId', 'RepartitorId']), []);

  return (
    <div className="ecran">
      <div className="ecran__bara"><h2>Sold stoc</h2></div>

      <DataGrid
        dataSource={sursa}
        remoteOperations={{ filtering: true, sorting: true, paging: true, summary: true, grouping: true }}
        showBorders
        columnAutoWidth
        height="calc(100vh - 170px)"
      >
        <Sorting mode="multiple" />
        <FilterRow visible />
        <HeaderFilter visible><Search enabled /></HeaderFilter>
        <Paging defaultPageSize={50} />
        <Pager showInfo showPageSizeSelector allowedPageSizes={[50, 100, 200]} />

        <Column dataField="ProdusCod" caption="Cod produs" />
        <Column dataField="ProdusDenumire" caption="Produs" />
        <Column dataField="ProdusUM" caption="UM" />
        <Column dataField="GestiuneDenumire" caption="Gestiune" />
        <Column dataField="ContSimbol" caption="Cont" />
        <Column dataField="LotData" caption="Data lot" dataType="date" format="dd.MM.yyyy" />
        <Column dataField="LotPretUnitar" caption="Cost unitar" dataType="number" format="#,##0.000000" alignment="right" />
        <Column dataField="Cantitate" dataType="number" format="#,##0.###" alignment="right" />
        <Column dataField="Valoare" dataType="number" format="#,##0.00" alignment="right" />

        <Summary>
          <TotalItem column="Valoare" summaryType="sum" valueFormat="#,##0.00" displayFormat="Σ {0}" />
        </Summary>
      </DataGrid>
    </div>
  );
}
