import { useMemo } from 'react';
import { useNavigate } from 'react-router';
import { Column, DataGrid, FilterRow, HeaderFilter, Pager, Paging, Search, Sorting } from 'devextreme-react/data-grid';
import { bcs, TIP_ANTET } from './api';
import { campMeta } from '../../nucleu/campMeta';

// Grila de citire (43c): `DataGrid` + store remote pe protocolul
// `DataSourceLoader` — filtrare/sortare/paginare server-side. Descriptorii de
// coloane stau ÎN felie și consumă `campMeta` pentru captions.
//
// Fără coloana „Autogenerat": BCS-ul nu e artefactul niciunei operări (nu e
// țintă de `PoliticaConex` și niciun tip nu-l produce ca secundar), iar
// `BcsListDto` nici nu poartă câmpul.
const SCHEMA_LISTA = 'BcsListDto';
const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

export function BcsLista() {
  const navigheaza = useNavigate();
  const sursa = useMemo(() => bcs.storeLista(), []);

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>Bonuri de consum</h2>
        <button type="button" className="buton buton--primar" onClick={() => navigheaza('/bcs/nou')}>
          Nou
        </button>
      </div>

      <DataGrid
        dataSource={sursa}
        remoteOperations
        showBorders
        columnAutoWidth
        height="calc(100vh - 170px)"
        onRowDblClick={(e) => navigheaza(`/bcs/${(e.data as { Id: string }).Id}`)}
      >
        <Sorting mode="multiple" />
        <FilterRow visible />
        <HeaderFilter visible><Search enabled /></HeaderFilter>
        <Paging defaultPageSize={25} />
        <Pager showInfo showPageSizeSelector allowedPageSizes={[25, 50, 100]} />

        <Column dataField="Numar" caption={cap('Numar')} defaultSortOrder="desc" />
        <Column dataField="Data" caption={cap('Data')} dataType="date" format="dd.MM.yyyy" />
        <Column dataField="Stare" caption={cap('Stare')} />
        {/* Coloanele DTO poartă denumirea; caption-ul îl dă membrul de MODEL
            corespunzător (FK-ul moștenește caption-ul navigației în dump). */}
        <Column dataField="PredatorDenumire" caption={cap('PredatorId')} />
        <Column dataField="PrimitorDenumire" caption={cap('PrimitorId')} />
        {/* `Total` vine calculat de server (proiecție), niciodată din TS. */}
        <Column dataField="Total" caption={cap('Total')} dataType="number" format="#,##0.00" alignment="right" />
      </DataGrid>

      <p className="indiciu">Dublu-click pe un rând deschide documentul.</p>
    </div>
  );
}
