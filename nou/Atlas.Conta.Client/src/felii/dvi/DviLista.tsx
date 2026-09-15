import { Column } from 'devextreme-react/data-grid';
import { dvi, SCHEMA_LISTA, TIP_ANTET } from './api';
import { campMeta } from '../../nucleu/campMeta';
import { ListaDocumente } from '../../nucleu/ListaDocumente';

const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

export function DviLista() {
  return (
    <ListaDocumente
      titlu="Declarații vamale de import"
      ruta="/dvi"
      nou="Declarație nouă"
      store={dvi.storeLista}
      perioada
      indiciu="Perioada filtrează după data vămuirii."
    >
      <Column dataField="Numar" caption={`${cap('Numar')} (MRN)`} />
      <Column dataField="Data" caption={cap('Data')} dataType="date" format="dd.MM.yyyy" defaultSortOrder="desc" />
      <Column dataField="Stare" caption={cap('Stare')} width={110} />
      <Column dataField="PredatorDenumire" caption="Biroul vamal" />
      {/* `Tva` pe DTO, `Taxa` pe entitate: caption-ul vine din membrul entității. */}
      <Column dataField="Baza" caption={cap('Baza')} dataType="number" format="#,##0.00" alignment="right" />
      <Column dataField="Tva" caption={cap('Taxa')} dataType="number" format="#,##0.00" alignment="right" />
      <Column dataField="NrFacturi" caption="Facturi legate" dataType="number" width={110} alignment="right" />
    </ListaDocumente>
  );
}
