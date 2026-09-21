import { useMemo } from 'react';
import { Column, Lookup } from 'devextreme-react/data-grid';
import { cas, SCHEMA_LISTA, TIP_ANTET, type CasListRand } from './api';
import { campMeta, labelEnum, valoriEnum } from '../../nucleu/campMeta';
import { ListaDocumente } from '../../nucleu/ListaDocumente';

const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

export function CasLista() {
  const cauze = useMemo(() => valoriEnum('CauzaIesire'), []);

  return (
    <ListaDocumente
      titlu="Ieșiri de imobilizări"
      ruta="/cas"
      nou="Ieșire nouă"
      store={cas.storeLista}
      perioada
      indiciu="Perioada filtrează după data ieșirii. Luna ieșirii nu se amortizează."
    >
      <Column dataField="Numar" caption={cap('Numar')} />
      <Column dataField="Data" caption={cap('Data')} dataType="date" format="dd.MM.yyyy" defaultSortOrder="desc" />
      <Column dataField="Cauza" caption={cap('Cauza')} width={120}>
        <Lookup dataSource={cauze} valueExpr="valoare" displayExpr="label" />
      </Column>
      <Column
        dataField="Stare"
        caption={cap('Stare')}
        width={110}
        calculateCellValue={(r: CasListRand) => labelEnum('StareDocument', r.Stare)}
      />
      <Column dataField="PredatorDenumire" caption="Locul" />
      <Column dataField="Total" caption={cap('Total')} dataType="number" format="#,##0.00" alignment="right" />
      <Column dataField="NrFise" caption="Fișe" dataType="number" width={90} alignment="right" />
    </ListaDocumente>
  );
}
