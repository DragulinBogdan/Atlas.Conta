import { useMemo } from 'react';
import { useNavigate } from 'react-router';
import { Column, DataGrid, FilterRow, HeaderFilter, Pager, Paging, Search, Sorting } from 'devextreme-react/data-grid';
import { asm, TIP_ANTET } from './api';
import { campMeta } from '../../nucleu/campMeta';

// Grila de citire (43c): `DataGrid` + store remote pe protocolul
// `DataSourceLoader` — filtrare/sortare/paginare server-side.
//
// Fără coloana „Autogenerat": ASM nu e niciodată artefactul unei operări (nu e
// țintă de `PoliticaConex` și niciun tip nu-l produce ca secundar), iar
// `AsmListDto` nici nu poartă câmpul.
const SCHEMA_LISTA = 'AsmListDto';
const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

export function AsmLista() {
  const navigheaza = useNavigate();
  const sursa = useMemo(() => asm.storeLista(), []);

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>Asamblări</h2>
        <button type="button" className="buton buton--primar" onClick={() => navigheaza('/asm/nou')}>
          Nouă
        </button>
      </div>

      <DataGrid
        dataSource={sursa}
        remoteOperations
        showBorders
        columnAutoWidth
        height="calc(100vh - 170px)"
        onRowDblClick={(e) => navigheaza(`/asm/${(e.data as { Id: string }).Id}`)}
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
        {/* Valorile sunt SEMNATE de la culegere (consum negativ, produs pozitiv),
            deci Σ liniilor E diferența invariantului: 0 înseamnă „echilibrat". */}
        <Column dataField="Total" caption="Diferență" dataType="number" format="#,##0.00" alignment="right" />
      </DataGrid>

      <p className="indiciu">
        Dublu-click pe un rând deschide documentul. Coloana „Diferență” e Σ liniilor semnate — pe un
        document echilibrat e 0,00; cifrele culegerii nu prezic însă regula golirii, verdictul e „Verifică”.
      </p>
    </div>
  );
}
