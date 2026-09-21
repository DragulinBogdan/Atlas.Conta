import { Column } from 'devextreme-react/data-grid';
import { ntc, TIP_ANTET } from './api';
import { campMeta } from '../../nucleu/campMeta';
import { ListaDocumente } from '../../nucleu/ListaDocumente';

const SCHEMA_LISTA = 'NtcListDto';
const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

export function NtcLista() {
  return (
    <ListaDocumente
      titlu="Note contabile"
      ruta="/ntc"
      nou="Nouă"
      store={ntc.storeLista}
      indiciu="Nota postează pe conturile scrise pe linii, fără nicio regulă de contare — inclusiv compensarea unei facturi cu un retur."
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
