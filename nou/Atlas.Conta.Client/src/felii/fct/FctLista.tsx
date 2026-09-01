import { useMemo } from 'react';
import { useNavigate } from 'react-router';
import { Column, DataGrid, FilterRow, HeaderFilter, Pager, Paging, Search, Sorting } from 'devextreme-react/data-grid';
import { fct, TIP_ANTET } from './api';
import { campMeta } from '../../nucleu/campMeta';

// Grila de citire (43c): `DataGrid` + store remote pe protocolul
// `DataSourceLoader` — filtrare/sortare/paginare server-side, cu 187k documente
// în baza de import.
const SCHEMA_LISTA = 'FacturaIntrareListDto';
const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

export function FctLista() {
  const navigheaza = useNavigate();
  const sursa = useMemo(() => fct.storeLista(), []);

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>Facturi intrare</h2>
        <button type="button" className="buton buton--primar" onClick={() => navigheaza('/fct/nou')}>
          Nouă
        </button>
      </div>

      <DataGrid
        dataSource={sursa}
        remoteOperations
        showBorders
        columnAutoWidth
        height="calc(100vh - 170px)"
        onRowDblClick={(e) => navigheaza(`/fct/${(e.data as { Id: string }).Id}`)}
      >
        <Sorting mode="multiple" />
        <FilterRow visible />
        <HeaderFilter visible><Search enabled /></HeaderFilter>
        <Paging defaultPageSize={25} />
        <Pager showInfo showPageSizeSelector allowedPageSizes={[25, 50, 100]} />

        <Column dataField="Numar" caption={cap('Numar')} defaultSortOrder="desc" />
        <Column dataField="Data" caption={cap('Data')} dataType="date" format="dd.MM.yyyy" />
        {/* Pe FCT predatorul E furnizorul: caption-ul din metadata e cel al bazei
            („Predator (de la)"), corect pentru orice document, prea abstract în
            lista de facturi. Numele de aici e o alegere a FELIEI, scrisă explicit;
            fixul de fond ar fi `[XafDisplayName("Furnizor")]` pe
            `FacturaIntrare.Predator` (Module) + regenerarea dump-ului. */}
        <Column dataField="PredatorDenumire" caption="Furnizor" />
        <Column dataField="PrimitorDenumire" caption={cap('PrimitorId')} />
        <Column dataField="DataScadenta" caption={cap('DataScadenta')} dataType="date" format="dd.MM.yyyy" />
        <Column dataField="Stare" caption={cap('Stare')} />
        {/* `Total` vine calculat de server (proiecție), niciodată din TS. */}
        <Column dataField="Total" caption={cap('Total')} dataType="number" format="#,##0.00" alignment="right" />
      </DataGrid>

      <p className="indiciu">Dublu-click pe un rând deschide documentul.</p>
    </div>
  );
}
