import { Column } from 'devextreme-react/data-grid';
import { campMeta, labelEnum } from '../../nucleu/campMeta';
import { ListaDocumente } from '../../nucleu/ListaDocumente';
import type { ApiTrezorerie } from './api';

const SCHEMA_LISTA = 'TrezorerieListDto';

// Comună plăților și încasărilor; titlul și numele laturilor le dă ruta (43a).
export function TrezorerieLista(props: {
  api: ApiTrezorerie;
  ruta: string;
  titlu: string;
  // Tipul de metadata (`Plata`/`Incasare`).
  tip: string;
  capPredator: string;
  capPrimitor: string;
}) {
  const { api, ruta, titlu, tip, capPredator, capPrimitor } = props;
  const cap = (membru: string) => campMeta(tip, membru, SCHEMA_LISTA).caption;

  return (
    <ListaDocumente titlu={titlu} ruta={ruta} nou="Nouă" store={api.storeLista}>
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
      {/* Plata născută din factură (31e). */}
      <Column dataField="Autogenerat" caption={cap('Autogenerat')} dataType="boolean" />
      {/* Predicatul `EsteVirament` e al serverului (F7); proiecție a DTO-ului, fără caption în metadata. */}
      <Column dataField="EsteVirament" caption="Virament intern" dataType="boolean" />
      <Column dataField="Total" caption={cap('Total')} dataType="number" format="#,##0.00" alignment="right" />
    </ListaDocumente>
  );
}
