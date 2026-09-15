import { Column } from 'devextreme-react/data-grid';
import { rlf, TIP_ANTET } from './api';
import { campMeta } from '../../nucleu/campMeta';
import { ListaDocumente } from '../../nucleu/ListaDocumente';

const SCHEMA_LISTA = 'RlfListDto';
const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

const INDICIU = `Totalul e POZITIV cât timp documentul e Draft (cifra de pe nota de credit a furnizorului) și NEGATIV după
  operare — returul se postează ca valori negative pe corespondența achiziției.`;

export function RlfLista() {
  return (
    <ListaDocumente titlu="Retururi la furnizor" ruta="/rlf" nou="Nou" store={rlf.storeLista} indiciu={INDICIU}>
      <Column dataField="Numar" caption={cap('Numar')} defaultSortOrder="desc" />
      <Column dataField="Data" caption={cap('Data')} dataType="date" format="dd.MM.yyyy" />
      <Column dataField="PredatorDenumire" caption="Gestiune" />
      <Column dataField="PrimitorDenumire" caption="Furnizor" />
      <Column dataField="Stare" caption={cap('Stare')} />
      <Column dataField="Total" caption={cap('Total')} dataType="number" format="#,##0.00" alignment="right" />
    </ListaDocumente>
  );
}
