import { Column } from 'devextreme-react/data-grid';
import { dec, RUTA, TIP_ANTET } from './api';
import { campMeta } from '../../nucleu/campMeta';
import { ListaDocumente } from '../../nucleu/ListaDocumente';

const SCHEMA_LISTA = 'DecontListDto';
const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

export function DecLista() {
  return (
    <ListaDocumente
      titlu="Deconturi"
      ruta={RUTA}
      nou="Nou"
      store={dec.storeLista}
      indiciu="Decontul justifică avansul unui titular și se stinge cu plăți/încasări (lanțul avans → decont → regularizare)."
    >
      <Column dataField="Numar" caption={cap('Numar')} defaultSortOrder="desc" />
      <Column dataField="Data" caption={cap('Data')} dataType="date" format="dd.MM.yyyy" />
      <Column dataField="PredatorDenumire" caption="Titular" />
      <Column dataField="PrimitorDenumire" caption={cap('PrimitorId')} />
      <Column dataField="Stare" caption={cap('Stare')} />
      <Column dataField="Total" caption={cap('Total')} dataType="number" format="#,##0.00" alignment="right" />
    </ListaDocumente>
  );
}
