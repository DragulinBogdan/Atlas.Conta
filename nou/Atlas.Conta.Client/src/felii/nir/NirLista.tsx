import { useMemo } from 'react';
import { useNavigate } from 'react-router';
import { Column, DataGrid, FilterRow, HeaderFilter, Pager, Paging, Sorting } from 'devextreme-react/data-grid';
import { nir, TIP_ANTET } from './api';
import { campMeta } from '../../nucleu/campMeta';

// Listă pură de citire: NIR-ul nu se creează din client în felia asta (F2-D3) —
// de aceea nu există buton „Nou". Lipsa lui e contractul, nu o omisiune.
const SCHEMA_LISTA = 'NirListDto';
const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

export function NirLista() {
  const navigheaza = useNavigate();
  const sursa = useMemo(() => nir.storeLista(), []);

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>NIR-uri</h2>
      </div>

      <DataGrid
        dataSource={sursa}
        remoteOperations
        showBorders
        columnAutoWidth
        height="calc(100vh - 170px)"
        onRowDblClick={(e) => navigheaza(`/nir/${(e.data as { Id: string }).Id}`)}
      >
        <Sorting mode="multiple" />
        <FilterRow visible />
        <HeaderFilter visible />
        <Paging defaultPageSize={25} />
        <Pager showInfo showPageSizeSelector allowedPageSizes={[25, 50, 100]} />

        <Column dataField="Numar" caption={cap('Numar')} defaultSortOrder="desc" />
        <Column dataField="Data" caption={cap('Data')} dataType="date" format="dd.MM.yyyy" />
        <Column dataField="PredatorDenumire" caption={cap('PredatorId')} />
        <Column dataField="PrimitorDenumire" caption={cap('PrimitorId')} />
        <Column dataField="Stare" caption={cap('Stare')} />
        <Column dataField="Autogenerat" caption={cap('Autogenerat')} dataType="boolean" />
        <Column dataField="Total" caption={cap('Total')} dataType="number" format="#,##0.00" alignment="right" />
      </DataGrid>

      <p className="indiciu">
        Dublu-click pe un rând deschide documentul. NIR-urile se nasc din operarea facturii de intrare.
      </p>
    </div>
  );
}
