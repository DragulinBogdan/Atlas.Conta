import { useMemo } from 'react';
import { useNavigate } from 'react-router';
import { Column, DataGrid, FilterRow, HeaderFilter, Pager, Paging, Sorting } from 'devextreme-react/data-grid';
import { ntc, TIP_ANTET } from './api';
import { campMeta } from '../../nucleu/campMeta';

// Grila de citire (43c): `DataGrid` + store remote pe protocolul
// `DataSourceLoader` — filtrare/sortare/paginare server-side.
//
// Fără coloana „Autogenerat": nota nu se generează niciodată automat (nu e țintă
// de `PoliticaConex` și niciun tip nu o produce ca secundar — închiderea de TVA
// își are propriul tip), iar `NtcListDto` nici nu poartă câmpul.
const SCHEMA_LISTA = 'NtcListDto';
const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

export function NtcLista() {
  const navigheaza = useNavigate();
  const sursa = useMemo(() => ntc.storeLista(), []);

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>Note contabile</h2>
        <button type="button" className="buton buton--primar" onClick={() => navigheaza('/ntc/nou')}>
          Nouă
        </button>
      </div>

      <DataGrid
        dataSource={sursa}
        remoteOperations
        showBorders
        columnAutoWidth
        height="calc(100vh - 170px)"
        onRowDblClick={(e) => navigheaza(`/ntc/${(e.data as { Id: string }).Id}`)}
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
        {/* `Total` = Σ liniilor, calculată de server. Poate fi negativă: nota de
            stornare postează valori negative pe corespondența originală (46a). */}
        <Column dataField="Total" caption={cap('Total')} dataType="number" format="#,##0.00" alignment="right" />
      </DataGrid>

      <p className="indiciu">
        Dublu-click pe un rând deschide documentul. Nota postează pe conturile scrise pe linii, fără nicio
        regulă de contare — inclusiv compensarea unei facturi cu un retur.
      </p>
    </div>
  );
}
