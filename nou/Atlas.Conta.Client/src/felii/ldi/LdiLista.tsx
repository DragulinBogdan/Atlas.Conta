import { Column } from 'devextreme-react/data-grid';
import { ldi, TIP_ANTET } from './api';
import { campMeta } from '../../nucleu/campMeta';
import { ListaDocumente } from '../../nucleu/ListaDocumente';

const SCHEMA_LISTA = 'LdiListDto';
const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

export function LdiLista() {
  return (
    <ListaDocumente
      titlu="Liste de diferențe de inventar"
      ruta="/ldi"
      nou="Nouă"
      store={ldi.storeLista}
      indiciu="Totalul e efectul NET al inventarului (plusuri − minusuri)."
    >
      <Column dataField="Numar" caption={cap('Numar')} defaultSortOrder="desc" />
      <Column dataField="Data" caption={cap('Data')} dataType="date" format="dd.MM.yyyy" />
      <Column dataField="PredatorDenumire" caption={cap('PredatorId')} />
      <Column dataField="PrimitorDenumire" caption={cap('PrimitorId')} />
      <Column dataField="Stare" caption={cap('Stare')} />
      <Column dataField="Total" caption={cap('Total')} dataType="number" format="#,##0.00" alignment="right" />
    </ListaDocumente>
  );
}
