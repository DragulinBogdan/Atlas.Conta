import { Column } from 'devextreme-react/data-grid';
import { rdc, TIP_ANTET } from './api';
import { campMeta } from '../../nucleu/campMeta';
import { ListaDocumente } from '../../nucleu/ListaDocumente';

const SCHEMA_LISTA = 'RdcListDto';
const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

const INDICIU = `„Total” e brutul liniilor de venit — cifra care ajustează creanța clientului; „Cost” e valoarea mărfii
  care revine în gestiune. Cele două nu se adună. Cifrele sunt pozitive pe Draft și negative după operare.`;

// Două coloane de bani care nu se adună (F19-D9).
export function RdcLista() {
  return (
    <ListaDocumente titlu="Retururi de la client" ruta="/rdc" nou="Nou" store={rdc.storeLista} indiciu={INDICIU}>
      <Column dataField="Numar" caption={cap('Numar')} defaultSortOrder="desc" />
      <Column dataField="Data" caption={cap('Data')} dataType="date" format="dd.MM.yyyy" />
      <Column dataField="PredatorDenumire" caption="Client" />
      <Column dataField="PrimitorDenumire" caption="Gestiune" />
      <Column dataField="Stare" caption={cap('Stare')} />
      <Column dataField="Total" caption="Total (venit)" dataType="number" format="#,##0.00" alignment="right" />
      <Column dataField="TotalCost" caption="Cost (marfă returnată)" dataType="number" format="#,##0.00" alignment="right" />
    </ListaDocumente>
  );
}
