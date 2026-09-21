import { Column } from 'devextreme-react/data-grid';
import { fct, TIP_ANTET } from './api';
import { campMeta } from '../../nucleu/campMeta';
import { ListaDocumente } from '../../nucleu/ListaDocumente';

const SCHEMA_LISTA = 'FacturaIntrareListDto';
const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

export function FctLista() {
  return (
    <ListaDocumente titlu="Facturi intrare" ruta="/fct" nou="Nouă" store={fct.storeLista}>
      <Column dataField="Numar" caption={cap('Numar')} defaultSortOrder="desc" />
      <Column dataField="Data" caption={cap('Data')} dataType="date" format="dd.MM.yyyy" />
      <Column dataField="PredatorDenumire" caption="Furnizor" />
      <Column dataField="PrimitorDenumire" caption={cap('PrimitorId')} />
      <Column dataField="DataScadenta" caption={cap('DataScadenta')} dataType="date" format="dd.MM.yyyy" />
      <Column dataField="Stare" caption={cap('Stare')} />
      <Column dataField="Total" caption={cap('Total')} dataType="number" format="#,##0.00" alignment="right" />
    </ListaDocumente>
  );
}
