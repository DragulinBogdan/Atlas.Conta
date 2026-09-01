import { useMemo } from 'react';
import { useNavigate } from 'react-router';
import { Column, DataGrid, FilterRow, HeaderFilter, Pager, Paging, Search, Sorting } from 'devextreme-react/data-grid';
import { nir, TIP_ANTET } from './api';
import { campMeta } from '../../nucleu/campMeta';

// Grila de citire (43c): `DataGrid` + store remote pe protocolul
// `DataSourceLoader` — filtrare/sortare/paginare server-side.
//
// Butonul „Nou" există de la felia 5: NIR-ul se culege și MANUAL (recepția fără
// factură — marfa intră pe aviz, loturile se nasc pe liniile lui). Restul se
// nasc din operarea facturii, ca până acum — cele două căi duc la același ecran.
const SCHEMA_LISTA = 'NirListDto';
const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

export function NirLista() {
  const navigheaza = useNavigate();
  const sursa = useMemo(() => nir.storeLista(), []);

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>NIR-uri</h2>
        <button type="button" className="buton buton--primar" onClick={() => navigheaza('/nir/nou')}>
          Nou
        </button>
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
        <HeaderFilter visible><Search enabled /></HeaderFilter>
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
        Dublu-click pe un rând deschide documentul. NIR-urile se nasc din operarea facturii de intrare
        sau se culeg manual, când marfa intră înaintea facturii.
      </p>
    </div>
  );
}
