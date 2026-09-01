import { useMemo } from 'react';
import { useNavigate } from 'react-router';
import { Column, DataGrid, FilterRow, HeaderFilter, Pager, Paging, Search, Sorting } from 'devextreme-react/data-grid';
import { campMeta, labelEnum } from '../../nucleu/campMeta';
import type { ApiTrezorerie } from './api';

// Grila de citire a unei felii de trezorerie (43c): remote pe protocolul
// `DataSourceLoader`, ca la FCT. Forma e comună celor două rute; ce diferă —
// titlul și numele LATURILOR — vine ca props din felie, scris explicit acolo
// (identitatea rămâne în cod, 43a).
const SCHEMA_LISTA = 'TrezorerieListDto';

export function TrezorerieLista(props: {
  api: ApiTrezorerie;
  ruta: string;
  titlu: string;
  // Tipul de METADATA (`Plata`/`Incasare`) — sursa captions-urilor comune.
  tip: string;
  capPredator: string;
  capPrimitor: string;
}) {
  const { api, ruta, titlu, tip, capPredator, capPrimitor } = props;
  const navigheaza = useNavigate();
  const sursa = useMemo(() => api.storeLista(), [api]);
  const cap = (membru: string) => campMeta(tip, membru, SCHEMA_LISTA).caption;

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>{titlu}</h2>
        <button type="button" className="buton buton--primar" onClick={() => navigheaza(`${ruta}/nou`)}>
          Nouă
        </button>
      </div>

      <DataGrid
        dataSource={sursa}
        remoteOperations
        showBorders
        columnAutoWidth
        height="calc(100vh - 170px)"
        onRowDblClick={(e) => navigheaza(`${ruta}/${(e.data as { Id: string }).Id}`)}
      >
        <Sorting mode="multiple" />
        <FilterRow visible />
        <HeaderFilter visible><Search enabled /></HeaderFilter>
        <Paging defaultPageSize={25} />
        <Pager showInfo showPageSizeSelector allowedPageSizes={[25, 50, 100]} />

        <Column dataField="Numar" caption={cap('Numar')} defaultSortOrder="desc" />
        <Column dataField="Data" caption={cap('Data')} dataType="date" format="dd.MM.yyyy" />
        <Column dataField="Stare" caption={cap('Stare')} />
        <Column dataField="PredatorDenumire" caption={capPredator} />
        <Column dataField="PrimitorDenumire" caption={capPrimitor} />
        <Column
          dataField="TipInstrument"
          caption={cap('TipInstrument')}
          calculateDisplayValue={(r: { TipInstrument?: string | null }) => labelEnum('TipInstrumentPlata', r.TipInstrument)}
        />
        {/* Autogenerat = plata născută din factură (31e) — coloană proprie
            trezoreriei: operatorul trebuie să vadă ce n-a cules el. */}
        <Column dataField="Autogenerat" caption={cap('Autogenerat')} dataType="boolean" />
        {/* Marcaj de VIRAMENT INTERN (F7), în stilul coloanei de mai sus: un
            picior al transferului 581 are număr din aceeași serie și
            contrapartidă ca orice plată — fără coloană ar fi indistinguibil de
            banii ieșiți din patrimoniu. Predicatul vine din SERVER
            (`EsteVirament` — ambele laturi conturi proprii), nu din felul
            laturilor dedus în TS. Caption LITERAL: nu e membru al entității
            XAF (e o proiecție a DTO-ului), deci n-are ce căuta în metadata. */}
        <Column dataField="EsteVirament" caption="Virament intern" dataType="boolean" />
        {/* `Total` vine calculat de server (proiecție), niciodată din TS. */}
        <Column dataField="Total" caption={cap('Total')} dataType="number" format="#,##0.00" alignment="right" />
      </DataGrid>

      <p className="indiciu">Dublu-click pe un rând deschide documentul.</p>
    </div>
  );
}
