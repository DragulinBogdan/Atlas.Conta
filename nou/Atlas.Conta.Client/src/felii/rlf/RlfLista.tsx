import { useMemo } from 'react';
import { useNavigate } from 'react-router';
import { Column, DataGrid, FilterRow, HeaderFilter, Pager, Paging, Search, Sorting } from 'devextreme-react/data-grid';
import { rlf, TIP_ANTET } from './api';
import { campMeta } from '../../nucleu/campMeta';

// Grila de citire (43c): `DataGrid` + store remote pe protocolul
// `DataSourceLoader` — filtrare/sortare/paginare server-side.
//
// Fără coloana „Autogenerat": RLF nu e artefactul unei operări (nu e țintă de
// `PoliticaConex` și niciun tip nu-l produce ca secundar), iar `RlfListDto` nici
// nu poartă câmpul.
const SCHEMA_LISTA = 'RlfListDto';
const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

export function RlfLista() {
  const navigheaza = useNavigate();
  const sursa = useMemo(() => rlf.storeLista(), []);

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>Retururi la furnizor</h2>
        <button type="button" className="buton buton--primar" onClick={() => navigheaza('/rlf/nou')}>
          Nou
        </button>
      </div>

      <DataGrid
        dataSource={sursa}
        remoteOperations
        showBorders
        columnAutoWidth
        height="calc(100vh - 170px)"
        onRowDblClick={(e) => navigheaza(`/rlf/${(e.data as { Id: string }).Id}`)}
      >
        <Sorting mode="multiple" />
        <FilterRow visible />
        <HeaderFilter visible><Search enabled /></HeaderFilter>
        <Paging defaultPageSize={25} />
        <Pager showInfo showPageSizeSelector allowedPageSizes={[25, 50, 100]} />

        <Column dataField="Numar" caption={cap('Numar')} defaultSortOrder="desc" />
        <Column dataField="Data" caption={cap('Data')} dataType="date" format="dd.MM.yyyy" />
        <Column dataField="PredatorDenumire" caption="Gestiune" />
        <Column dataField="PrimitorDenumire" caption="Furnizor" />
        <Column dataField="Stare" caption={cap('Stare')} />
        <Column dataField="Total" caption={cap('Total')} dataType="number" format="#,##0.00" alignment="right" />
      </DataGrid>

      <p className="indiciu">
        Dublu-click pe un rând deschide documentul. Totalul e POZITIV cât timp documentul e Draft (cifra de
        pe nota de credit a furnizorului) și NEGATIV după operare — returul se postează ca valori negative
        pe corespondența achiziției.
      </p>
    </div>
  );
}
