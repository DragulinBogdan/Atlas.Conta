import { useMemo } from 'react';
import { useNavigate } from 'react-router';
import { Column, DataGrid, FilterRow, HeaderFilter, Pager, Paging, Sorting } from 'devextreme-react/data-grid';
import { fcl, TIP_ANTET } from './api';
import { campMeta } from '../../nucleu/campMeta';

// Grila de citire (43c): `DataGrid` + store remote pe protocolul
// `DataSourceLoader` — filtrare/sortare/paginare server-side.
const SCHEMA_LISTA = 'FacturaIesireListDto';
const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

export function FclLista() {
  const navigheaza = useNavigate();
  const sursa = useMemo(() => fcl.storeLista(), []);

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>Facturi ieșire</h2>
        <button type="button" className="buton buton--primar" onClick={() => navigheaza('/fcl/nou')}>
          Nouă
        </button>
      </div>

      <DataGrid
        dataSource={sursa}
        remoteOperations
        showBorders
        columnAutoWidth
        height="calc(100vh - 170px)"
        onRowDblClick={(e) => navigheaza(`/fcl/${(e.data as { Id: string }).Id}`)}
      >
        <Sorting mode="multiple" />
        <FilterRow visible />
        <HeaderFilter visible />
        <Paging defaultPageSize={25} />
        <Pager showInfo showPageSizeSelector allowedPageSizes={[25, 50, 100]} />

        <Column dataField="Numar" caption={cap('Numar')} defaultSortOrder="desc" />
        <Column dataField="Data" caption={cap('Data')} dataType="date" format="dd.MM.yyyy" />
        {/* Laturile FCL: predatorul E emitentul (repartitor intern), primitorul E
            clientul. Captions-urile din metadata sunt cele ale BAZEI („Predator
            (de la)"), corecte pentru orice document și prea abstracte în lista de
            facturi — numele de aici sunt o alegere a FELIEI, scrisă explicit
            (precedentul: coloana „Furnizor" din `FctLista`). Fixul de fond ar fi
            `[XafDisplayName]` pe `FacturaIesire.Predator/Primitor` în Module +
            regenerarea dump-ului. */}
        <Column dataField="PredatorDenumire" caption="Emitent" />
        <Column dataField="PrimitorDenumire" caption="Client" />
        <Column dataField="DataScadenta" caption={cap('DataScadenta')} dataType="date" format="dd.MM.yyyy" />
        <Column dataField="Stare" caption={cap('Stare')} />
        {/* `Total` vine calculat de server (proiecție), niciodată din TS. */}
        <Column dataField="Total" caption={cap('Total')} dataType="number" format="#,##0.00" alignment="right" />
      </DataGrid>

      <p className="indiciu">Dublu-click pe un rând deschide documentul.</p>
    </div>
  );
}
