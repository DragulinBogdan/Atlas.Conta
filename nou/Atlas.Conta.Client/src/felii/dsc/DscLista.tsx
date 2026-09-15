import { Column } from 'devextreme-react/data-grid';
import { dsc, TIP_ANTET } from './api';
import { campMeta } from '../../nucleu/campMeta';
import { ListaDocumente } from '../../nucleu/ListaDocumente';

const SCHEMA_LISTA = 'DscListDto';
const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

// Fără `nou`: descărcarea nu se culege din client (F4-D2).
export function DscLista() {
  return (
    <ListaDocumente
      titlu="Descărcări de gestiune"
      ruta="/dsc"
      store={dsc.storeLista}
      indiciu="Descărcările se nasc din operarea facturii de ieșire sau din comanda de backorder de pe factură."
    >
      <Column dataField="Numar" caption={cap('Numar')} defaultSortOrder="desc" />
      <Column dataField="Data" caption={cap('Data')} dataType="date" format="dd.MM.yyyy" />
      <Column dataField="PredatorDenumire" caption="Gestiune" />
      <Column dataField="PrimitorDenumire" caption="Client" />
      <Column dataField="Stare" caption={cap('Stare')} />
      <Column dataField="Autogenerat" caption={cap('Autogenerat')} dataType="boolean" />
      <Column dataField="Total" caption={cap('Total')} dataType="number" format="#,##0.00" alignment="right" />
    </ListaDocumente>
  );
}
