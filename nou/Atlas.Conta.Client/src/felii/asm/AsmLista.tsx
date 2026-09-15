import { Column } from 'devextreme-react/data-grid';
import { asm, TIP_ANTET } from './api';
import { campMeta } from '../../nucleu/campMeta';
import { ListaDocumente } from '../../nucleu/ListaDocumente';

const SCHEMA_LISTA = 'AsmListDto';
const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

export function AsmLista() {
  return (
    <ListaDocumente
      titlu="Asamblări"
      ruta="/asm"
      nou="Nouă"
      store={asm.storeLista}
      indiciu="Coloana „Diferență” e Σ liniilor semnate — pe un document echilibrat e 0,00; cifrele culegerii nu prezic însă regula golirii, verdictul e „Verifică”."
    >
      <Column dataField="Numar" caption={cap('Numar')} defaultSortOrder="desc" />
      <Column dataField="Data" caption={cap('Data')} dataType="date" format="dd.MM.yyyy" />
      <Column dataField="PredatorDenumire" caption={cap('PredatorId')} />
      <Column dataField="PrimitorDenumire" caption={cap('PrimitorId')} />
      <Column dataField="Stare" caption={cap('Stare')} />
      <Column dataField="Total" caption="Diferență" dataType="number" format="#,##0.00" alignment="right" />
    </ListaDocumente>
  );
}
