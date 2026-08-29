import { useMemo } from 'react';
import { useNavigate } from 'react-router';
import { Column, DataGrid, FilterRow, HeaderFilter, Pager, Paging, Sorting } from 'devextreme-react/data-grid';
import { rdc, TIP_ANTET } from './api';
import { campMeta } from '../../nucleu/campMeta';

// Grila de citire (43c): `DataGrid` + store remote pe protocolul
// `DataSourceLoader` — filtrare/sortare/paginare server-side.
//
// DOUĂ coloane de bani, nu una: `Total` e brutul liniilor de VENIT (ce ajustează
// creanța — aceeași definiție ca pe ecranul de document, `ReturClient.Total`
// virtual), `TotalCost` e Σ liniilor cu lot. Grila nu are voie să arate altă
// cifră decât detaliul și nici să le adune (F19-D9).
const SCHEMA_LISTA = 'RdcListDto';
const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

export function RdcLista() {
  const navigheaza = useNavigate();
  const sursa = useMemo(() => rdc.storeLista(), []);

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>Retururi de la client</h2>
        <button type="button" className="buton buton--primar" onClick={() => navigheaza('/rdc/nou')}>
          Nou
        </button>
      </div>

      <DataGrid
        dataSource={sursa}
        remoteOperations
        showBorders
        columnAutoWidth
        height="calc(100vh - 170px)"
        onRowDblClick={(e) => navigheaza(`/rdc/${(e.data as { Id: string }).Id}`)}
      >
        <Sorting mode="multiple" />
        <FilterRow visible />
        <HeaderFilter visible />
        <Paging defaultPageSize={25} />
        <Pager showInfo showPageSizeSelector allowedPageSizes={[25, 50, 100]} />

        <Column dataField="Numar" caption={cap('Numar')} defaultSortOrder="desc" />
        <Column dataField="Data" caption={cap('Data')} dataType="date" format="dd.MM.yyyy" />
        <Column dataField="PredatorDenumire" caption="Client" />
        <Column dataField="PrimitorDenumire" caption="Gestiune" />
        <Column dataField="Stare" caption={cap('Stare')} />
        <Column dataField="Total" caption="Total (venit)" dataType="number" format="#,##0.00" alignment="right" />
        <Column dataField="TotalCost" caption="Cost (marfă returnată)" dataType="number" format="#,##0.00" alignment="right" />
      </DataGrid>

      <p className="indiciu">
        Dublu-click pe un rând deschide documentul. „Total” e brutul liniilor de venit — cifra care
        ajustează creanța clientului; „Cost” e valoarea mărfii care revine în gestiune. Cele două nu se
        adună. Cifrele sunt pozitive pe Draft și negative după operare.
      </p>
    </div>
  );
}
