import { useMemo } from 'react';
import { useNavigate } from 'react-router';
import DataSource from 'devextreme/data/data_source';
import { Column, DataGrid, FilterRow, HeaderFilter, Lookup, Pager, Paging, Search, Sorting } from 'devextreme-react/data-grid';
import { cas, SCHEMA_LISTA, TIP_ANTET, type CasListRand } from './api';
import { campMeta, labelEnum, valoriEnum } from '../../nucleu/campMeta';
import { useUrlStare } from '../../nucleu/urlStare';
import { CasetaPerioada, lunaCurenta } from '../raportare/comune';

// Lista ieșirilor de imobilizări: grilă remote peste o PERIOADĂ din URL (43c).
// `Total` și `NrFise` vin din proiecția serverului (42c).

const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

export function CasLista() {
  const navigheaza = useNavigate();
  const luna = lunaCurenta();
  const cauze = useMemo(() => valoriEnum('CauzaIesire'), []);
  const [stare, seteaza] = useUrlStare({ dataStart: luna.start, dataEnd: luna.sfarsit });

  const sursa = useMemo(() => new DataSource({
    store: cas.storeLista(),
    filter: [['Data', '>=', stare.dataStart], 'and', ['Data', '<=', stare.dataEnd]],
    sort: [{ selector: 'Data', desc: true }],
    paginate: true,
    pageSize: 25,
    requireTotalCount: true,
  }), [stare.dataStart, stare.dataEnd]);

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>Ieșiri de imobilizări</h2>
        <button type="button" className="buton buton--primar" onClick={() => navigheaza('/cas/nou')}>
          Ieșire nouă
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
        onRowDblClick={(e) => navigheaza(`/cas/${(e.data as { Id: string }).Id}`)}
      >
        <Sorting mode="multiple" />
        <FilterRow visible />
        <HeaderFilter visible><Search enabled /></HeaderFilter>
        <Paging defaultPageSize={25} />
        <Pager showInfo showPageSizeSelector allowedPageSizes={[25, 50, 100]} />

        <Column dataField="Numar" caption={cap('Numar')} />
        <Column dataField="Data" caption={cap('Data')} dataType="date" format="dd.MM.yyyy" defaultSortOrder="desc" />
        <Column dataField="Cauza" caption={cap('Cauza')} width={120}>
          <Lookup dataSource={cauze} valueExpr="valoare" displayExpr="label" />
        </Column>
        <Column
          dataField="Stare"
          caption={cap('Stare')}
          width={110}
          calculateCellValue={(r: CasListRand) => labelEnum('StareDocument', r.Stare)}
        />
        {/* Predatorul ieșirii e LOCUL de pe fișe. */}
        <Column dataField="PredatorDenumire" caption="Locul" />
        <Column dataField="Total" caption={cap('Total')} dataType="number" format="#,##0.00" alignment="right" />
        <Column dataField="NrFise" caption="Fișe" dataType="number" width={90} alignment="right" />
      </DataGrid>

      <p className="indiciu">
        Dublu-click pe un rând deschide documentul. Perioada filtrează după data ieșirii.
        Luna ieșirii nu se amortizează.
      </p>
    </div>
  );
}
