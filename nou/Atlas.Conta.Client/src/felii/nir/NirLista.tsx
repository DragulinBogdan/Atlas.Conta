import { Column } from 'devextreme-react/data-grid';
import { nir, TIP_ANTET } from './api';
import { campMeta } from '../../nucleu/campMeta';
import { ListaDocumente } from '../../nucleu/ListaDocumente';

const SCHEMA_LISTA = 'NirListDto';
const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

export function NirLista() {
  return (
    <ListaDocumente
      titlu="NIR-uri"
      ruta="/nir"
      nou="Nou"
      store={nir.storeLista}
      indiciu="NIR-urile se nasc din operarea facturii de intrare sau se culeg manual, când marfa intră înaintea facturii."
    >
      <Column dataField="Numar" caption={cap('Numar')} defaultSortOrder="desc" />
      <Column dataField="Data" caption={cap('Data')} dataType="date" format="dd.MM.yyyy" />
      <Column dataField="PredatorDenumire" caption={cap('PredatorId')} />
      <Column dataField="PrimitorDenumire" caption={cap('PrimitorId')} />
      <Column dataField="Stare" caption={cap('Stare')} />
      <Column dataField="Autogenerat" caption={cap('Autogenerat')} dataType="boolean" />
      <Column dataField="Total" caption={cap('Total')} dataType="number" format="#,##0.00" alignment="right" />
    </ListaDocumente>
  );
}
