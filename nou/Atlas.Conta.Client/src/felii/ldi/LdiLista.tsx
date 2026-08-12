import { useMemo } from 'react';
import { useNavigate } from 'react-router';
import { Column, DataGrid, FilterRow, HeaderFilter, Pager, Paging, Sorting } from 'devextreme-react/data-grid';
import { ldi, TIP_ANTET } from './api';
import { campMeta } from '../../nucleu/campMeta';

// Grila de citire (43c): `DataGrid` + store remote pe protocolul
// `DataSourceLoader` — filtrare/sortare/paginare server-side.
//
// Fără coloana „Autogenerat": LDI nu e niciodată artefactul unei operări (nu e
// țintă de `PoliticaConex` și niciun tip nu-l produce ca secundar), iar
// `LdiListDto` nici nu poartă câmpul.
const SCHEMA_LISTA = 'LdiListDto';
const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

export function LdiLista() {
  const navigheaza = useNavigate();
  const sursa = useMemo(() => ldi.storeLista(), []);

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>Liste de diferențe de inventar</h2>
        <button type="button" className="buton buton--primar" onClick={() => navigheaza('/ldi/nou')}>
          Nouă
        </button>
      </div>

      <DataGrid
        dataSource={sursa}
        remoteOperations
        showBorders
        columnAutoWidth
        height="calc(100vh - 170px)"
        onRowDblClick={(e) => navigheaza(`/ldi/${(e.data as { Id: string }).Id}`)}
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
        {/* `Total` vine calculat de server (proiecție) și e NET: minusurile
            intră cu valoare negativă, plusurile cu pozitivă (F6-D6). */}
        <Column dataField="Total" caption={cap('Total')} dataType="number" format="#,##0.00" alignment="right" />
      </DataGrid>

      <p className="indiciu">
        Dublu-click pe un rând deschide documentul. Totalul e efectul NET al inventarului
        (plusuri − minusuri).
      </p>
    </div>
  );
}
