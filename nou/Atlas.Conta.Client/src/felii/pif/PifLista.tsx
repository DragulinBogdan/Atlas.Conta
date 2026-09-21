import { Column } from 'devextreme-react/data-grid';
import { pif, SCHEMA_LISTA, TIP_ANTET, type PifListRand } from './api';
import { campMeta, labelEnum } from '../../nucleu/campMeta';
import { ListaDocumente } from '../../nucleu/ListaDocumente';

const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

export function PifLista() {
  return (
    <ListaDocumente
      titlu="Puneri în funcțiune"
      ruta="/pif"
      nou="Punere în funcțiune nouă"
      store={pif.storeLista}
      perioada
      indiciu={
        'Perioada filtrează după data punerii în funcțiune. Un document acoperă un singur loc: fișele liniilor '
        + 'trebuie să aibă locul primitorului.'
      }
    >
      <Column dataField="Numar" caption={cap('Numar')} />
      <Column dataField="Data" caption={cap('Data')} dataType="date" format="dd.MM.yyyy" defaultSortOrder="desc" />
      <Column
        dataField="Stare"
        caption={cap('Stare')}
        width={110}
        calculateCellValue={(r: PifListRand) => labelEnum('StareDocument', r.Stare)}
      />
      <Column dataField="PredatorDenumire" caption="Unitatea" />
      <Column dataField="PrimitorDenumire" caption="Locul" />
      <Column dataField="Total" caption={cap('Total')} dataType="number" format="#,##0.00" alignment="right" />
      <Column dataField="NrLinii" caption="Linii" dataType="number" width={90} alignment="right" />
    </ListaDocumente>
  );
}
