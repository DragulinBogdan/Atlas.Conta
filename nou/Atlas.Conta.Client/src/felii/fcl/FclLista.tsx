import { Column } from 'devextreme-react/data-grid';
import { fcl, TIP_ANTET } from './api';
import { campMeta } from '../../nucleu/campMeta';
import { ListaDocumente } from '../../nucleu/ListaDocumente';

const SCHEMA_LISTA = 'FacturaIesireListDto';
const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

export function FclLista() {
  return (
    <ListaDocumente titlu="Facturi ieșire" ruta="/fcl" nou="Nouă" store={fcl.storeLista}>
      <Column dataField="Numar" caption={cap('Numar')} defaultSortOrder="desc" />
      <Column dataField="Data" caption={cap('Data')} dataType="date" format="dd.MM.yyyy" />
      <Column dataField="PredatorDenumire" caption="Emitent" />
      <Column dataField="PrimitorDenumire" caption="Client" />
      <Column dataField="DataScadenta" caption={cap('DataScadenta')} dataType="date" format="dd.MM.yyyy" />
      <Column dataField="Stare" caption={cap('Stare')} />
      <Column dataField="Total" caption={cap('Total')} dataType="number" format="#,##0.00" alignment="right" />
    </ListaDocumente>
  );
}
