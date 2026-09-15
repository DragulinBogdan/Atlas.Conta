import { Column } from 'devextreme-react/data-grid';
import { bcs, TIP_ANTET } from './api';
import { campMeta } from '../../nucleu/campMeta';
import { ListaDocumente } from '../../nucleu/ListaDocumente';

const SCHEMA_LISTA = 'BcsListDto';
const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

export function BcsLista() {
  return (
    <ListaDocumente titlu="Bonuri de consum" ruta="/bcs" nou="Nou" store={bcs.storeLista}>
      <Column dataField="Numar" caption={cap('Numar')} defaultSortOrder="desc" />
      <Column dataField="Data" caption={cap('Data')} dataType="date" format="dd.MM.yyyy" />
      <Column dataField="Stare" caption={cap('Stare')} />
      <Column dataField="PredatorDenumire" caption={cap('PredatorId')} />
      <Column dataField="PrimitorDenumire" caption={cap('PrimitorId')} />
      <Column dataField="Total" caption={cap('Total')} dataType="number" format="#,##0.00" alignment="right" />
    </ListaDocumente>
  );
}
