import { useMemo } from 'react';
import { useNavigate } from 'react-router';
import DataSource from 'devextreme/data/data_source';
import { Column, DataGrid, FilterRow, HeaderFilter, Pager, Paging, Search, Sorting } from 'devextreme-react/data-grid';
import { dvi, SCHEMA_LISTA, TIP_ANTET } from './api';
import { campMeta } from '../../nucleu/campMeta';
import { useUrlStare } from '../../nucleu/urlStare';
import { CasetaPerioada, lunaCurenta } from '../raportare/comune';

// Lista declarațiilor vamale: grilă remote (`DataSourceLoader`) peste o PERIOADĂ
// care trăiește în URL (43c) — „declarațiile pe septembrie" trebuie să fie un
// link. Perioada e filtru al INTEROGĂRII, nu al paginii încărcate: se duce pe
// sârmă ca orice filtru al grilei.
//
// Cifrele (`Baza`, `Tva`, `NrFacturi`) vin din proiecția serverului; TS nu le
// adună (42c).

const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

export function DviLista() {
  const navigheaza = useNavigate();
  const luna = lunaCurenta();
  const [stare, seteaza] = useUrlStare({ dataStart: luna.start, dataEnd: luna.sfarsit });

  const sursa = useMemo(() => new DataSource({
    store: dvi.storeLista(),
    filter: [['Data', '>=', stare.dataStart], 'and', ['Data', '<=', stare.dataEnd]],
    sort: [{ selector: 'Data', desc: true }],
    paginate: true,
    pageSize: 25,
    requireTotalCount: true,
  }), [stare.dataStart, stare.dataEnd]);

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>Declarații vamale de import</h2>
        <button type="button" className="buton buton--primar" onClick={() => navigheaza('/dvi/nou')}>
          Declarație nouă
        </button>
      </div>

      <div className="bara-raport">
        <CasetaPerioada dataStart={stare.dataStart} dataEnd={stare.dataEnd} seteaza={seteaza} />
      </div>

      <DataGrid
        dataSource={sursa}
        remoteOperations
        showBorders
        columnAutoWidth
        height="calc(100vh - 230px)"
        onRowDblClick={(e) => navigheaza(`/dvi/${(e.data as { Id: string }).Id}`)}
      >
        <Sorting mode="multiple" />
        <FilterRow visible />
        <HeaderFilter visible><Search enabled /></HeaderFilter>
        <Paging defaultPageSize={25} />
        <Pager showInfo showPageSizeSelector allowedPageSizes={[25, 50, 100]} />

        <Column dataField="Numar" caption={`${cap('Numar')} (MRN)`} />
        <Column dataField="Data" caption={cap('Data')} dataType="date" format="dd.MM.yyyy" defaultSortOrder="desc" />
        <Column dataField="Stare" caption={cap('Stare')} width={110} />
        {/* Predatorul DVI e biroul vamal (sau comisionarul care a plătit taxa în
            vamă): caption-ul bazei („Predator (de la)") e corect și prea
            abstract aici, ca pe FCT. Numele e o alegere a FELIEI, scrisă
            explicit. */}
        <Column dataField="PredatorDenumire" caption="Biroul vamal" />
        {/* `Baza`/`Tva` sunt proiecția serverului; `Taxa` e numele membrului pe
            entitate, `Tva` numele câmpului pe DTO — caption-ul vine tot din
            metadata, prin membrul care îl poartă. */}
        <Column dataField="Baza" caption={cap('Baza')} dataType="number" format="#,##0.00" alignment="right" />
        <Column dataField="Tva" caption={cap('Taxa')} dataType="number" format="#,##0.00" alignment="right" />
        <Column dataField="NrFacturi" caption="Facturi legate" dataType="number" width={110} alignment="right" />
      </DataGrid>

      <p className="indiciu">
        Dublu-click pe un rând deschide declarația. Perioada filtrează după data vămuirii.
      </p>
    </div>
  );
}
