import { useMemo } from 'react';
import { useNavigate } from 'react-router';
import { Column, DataGrid, FilterRow, HeaderFilter, Pager, Paging, Search, Sorting } from 'devextreme-react/data-grid';
import { dec, RUTA, TIP_ANTET } from './api';
import { campMeta } from '../../nucleu/campMeta';

// Grila de citire (43c): `DataGrid` + store remote pe protocolul
// `DataSourceLoader` — filtrare/sortare/paginare server-side.
//
// Fără coloana „Autogenerat": decontul nu se generează niciodată automat (nu e
// țintă de `PoliticaConex` și niciun tip nu-l produce ca secundar), iar
// `DecontListDto` nici nu poartă câmpul.
const SCHEMA_LISTA = 'DecontListDto';
const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

export function DecLista() {
  const navigheaza = useNavigate();
  const sursa = useMemo(() => dec.storeLista(), []);

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>Deconturi</h2>
        <button type="button" className="buton buton--primar" onClick={() => navigheaza(`${RUTA}/nou`)}>
          Nou
        </button>
      </div>

      <DataGrid
        dataSource={sursa}
        remoteOperations
        showBorders
        columnAutoWidth
        height="calc(100vh - 170px)"
        onRowDblClick={(e) => navigheaza(`${RUTA}/${(e.data as { Id: string }).Id}`)}
      >
        <Sorting mode="multiple" />
        <FilterRow visible />
        <HeaderFilter visible><Search enabled /></HeaderFilter>
        <Paging defaultPageSize={25} />
        <Pager showInfo showPageSizeSelector allowedPageSizes={[25, 50, 100]} />

        <Column dataField="Numar" caption={cap('Numar')} defaultSortOrder="desc" />
        <Column dataField="Data" caption={cap('Data')} dataType="date" format="dd.MM.yyyy" />
        {/* Titularul care justifică — caption-ul bazei („Predator (de la)") e
            corect, dar abstract pentru ecranul de decont. */}
        <Column dataField="PredatorDenumire" caption="Titular" />
        <Column dataField="PrimitorDenumire" caption={cap('PrimitorId')} />
        <Column dataField="Stare" caption={cap('Stare')} />
        {/* `Total` vine calculat de server (proiecție), BRUT (Σ Valoare +
            ValoareTva) — aceeași cifră pe care o stinge imperecherea. */}
        <Column dataField="Total" caption={cap('Total')} dataType="number" format="#,##0.00" alignment="right" />
      </DataGrid>

      <p className="indiciu">
        Dublu-click pe un rând deschide documentul. Decontul justifică avansul unui titular și se
        stinge cu plăți/încasări (lanțul avans → decont → regularizare).
      </p>
    </div>
  );
}
