import { useMemo } from 'react';
import { useNavigate } from 'react-router';
import DataSource from 'devextreme/data/data_source';
import { Column, DataGrid, FilterRow, HeaderFilter, Pager, Paging, Search, Sorting } from 'devextreme-react/data-grid';
import { pif, SCHEMA_LISTA, TIP_ANTET, type PifListRand } from './api';
import { campMeta, labelEnum } from '../../nucleu/campMeta';
import { useUrlStare } from '../../nucleu/urlStare';
import { CasetaPerioada, lunaCurenta } from '../raportare/comune';

// Lista punerilor în funcțiune: grilă remote (`DataSourceLoader`) peste o
// PERIOADĂ care trăiește în URL (43c). `Total` și `NrLinii` vin din proiecția
// serverului; TS nu le adună (42c).

const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

export function PifLista() {
  const navigheaza = useNavigate();
  const luna = lunaCurenta();
  const [stare, seteaza] = useUrlStare({ dataStart: luna.start, dataEnd: luna.sfarsit });

  const sursa = useMemo(() => new DataSource({
    store: pif.storeLista(),
    filter: [['Data', '>=', stare.dataStart], 'and', ['Data', '<=', stare.dataEnd]],
    sort: [{ selector: 'Data', desc: true }],
    paginate: true,
    pageSize: 25,
    requireTotalCount: true,
  }), [stare.dataStart, stare.dataEnd]);

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>Puneri în funcțiune</h2>
        <button type="button" className="buton buton--primar" onClick={() => navigheaza('/pif/nou')}>
          Punere în funcțiune nouă
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
        onRowDblClick={(e) => navigheaza(`/pif/${(e.data as { Id: string }).Id}`)}
      >
        <Sorting mode="multiple" />
        <FilterRow visible />
        <HeaderFilter visible><Search enabled /></HeaderFilter>
        <Paging defaultPageSize={25} />
        <Pager showInfo showPageSizeSelector allowedPageSizes={[25, 50, 100]} />

        <Column dataField="Numar" caption={cap('Numar')} />
        <Column dataField="Data" caption={cap('Data')} dataType="date" format="dd.MM.yyyy" defaultSortOrder="desc" />
        <Column
          dataField="Stare"
          caption={cap('Stare')}
          width={110}
          calculateCellValue={(r: PifListRand) => labelEnum('StareDocument', r.Stare)}
        />
        {/* Laturile documentului, numite în vocabularul feliei: predatorul e
            unitatea internă care pune în funcțiune, primitorul e LOCUL pe care
            stau fișele. */}
        <Column dataField="PredatorDenumire" caption="Unitatea" />
        <Column dataField="PrimitorDenumire" caption="Locul" />
        <Column dataField="Total" caption={cap('Total')} dataType="number" format="#,##0.00" alignment="right" />
        <Column dataField="NrLinii" caption="Linii" dataType="number" width={90} alignment="right" />
      </DataGrid>

      <p className="indiciu">
        Dublu-click pe un rând deschide documentul. Perioada filtrează după data punerii în funcțiune.
        Un document acoperă un singur loc: fișele liniilor trebuie să aibă locul primitorului.
      </p>
    </div>
  );
}
