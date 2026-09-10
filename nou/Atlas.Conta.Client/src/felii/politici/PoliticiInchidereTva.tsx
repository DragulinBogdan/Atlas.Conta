import { useMemo } from 'react';
import { Column, Lookup } from 'devextreme-react/data-grid';
import { storeOData } from '../../nucleu/odata';
import { GrilaPolitica } from './GrilaPolitica';
import {
  afisareNav, captionPolitica, codNav, codSiDenumire, simbolSiDenumire, urlExplica,
} from './comune';

// Conturile închiderii de TVA (46c) — închide 79-r2.
//
// Serviciul cere SETUL COMPLET: gardianul acceptă ori toate patru conturile, ori
// niciunul. Un rând pe jumătate completat ar fi trecut de schemă și ar fi picat
// abia la generarea închiderii, cu un mesaj despre altceva.

const TIP = 'PoliticaInchidereTva';
const cap = (m: string) => captionPolitica(TIP, m);

export function PoliticiInchidereTva() {
  const tipuriDocument = useMemo(() => ({ store: storeOData('TipDocument'), sort: 'Cod' }), []);
  const conturi = useMemo(
    () => ({ store: storeOData('Cont'), sort: 'Simbol', select: ['ID', 'Simbol', 'Denumire'] }), []);

  return (
    <GrilaPolitica
      titlu="Închidere TVA (conturi)"
      entitate={TIP}
      explica={(r) => urlExplica(codNav(r, 'TipDocument'))}
      expand={['TipDocument', 'ContDeductibila', 'ContColectata', 'ContDePlata', 'ContDeRecuperat']}
      indiciu={(
        <>
          Cele patru conturi se completează împreună sau deloc: închiderea transferă deductibila
          și colectata și lasă soldul pe „de plată" sau pe „de recuperat". Închiderea nu închide
          perioada fiscală și nu atinge contul de rezultat.
        </>
      )}
    >
      <Column
        dataField="TipDocumentId"
        caption={cap('TipDocumentId')}
        calculateDisplayValue={afisareNav('TipDocument', codSiDenumire)}
        calculateSortValue="TipDocument.Cod"
        defaultSortOrder="asc"
        width={180}
      >
        <Lookup dataSource={tipuriDocument} valueExpr="ID" displayExpr={codSiDenumire} />
      </Column>
      <Column
        dataField="ContDeductibilaId"
        caption="TVA deductibilă"
        calculateDisplayValue={afisareNav('ContDeductibila', simbolSiDenumire)}
        calculateSortValue="ContDeductibila.Simbol"
        width={200}
      >
        <Lookup dataSource={conturi} valueExpr="ID" displayExpr={simbolSiDenumire} allowClearing />
      </Column>
      <Column
        dataField="ContColectataId"
        caption="TVA colectată"
        calculateDisplayValue={afisareNav('ContColectata', simbolSiDenumire)}
        calculateSortValue="ContColectata.Simbol"
        width={200}
      >
        <Lookup dataSource={conturi} valueExpr="ID" displayExpr={simbolSiDenumire} allowClearing />
      </Column>
      <Column
        dataField="ContDePlataId"
        caption="TVA de plată"
        calculateDisplayValue={afisareNav('ContDePlata', simbolSiDenumire)}
        calculateSortValue="ContDePlata.Simbol"
        width={200}
      >
        <Lookup dataSource={conturi} valueExpr="ID" displayExpr={simbolSiDenumire} allowClearing />
      </Column>
      <Column
        dataField="ContDeRecuperatId"
        caption="TVA de recuperat"
        calculateDisplayValue={afisareNav('ContDeRecuperat', simbolSiDenumire)}
        calculateSortValue="ContDeRecuperat.Simbol"
        width={200}
      >
        <Lookup dataSource={conturi} valueExpr="ID" displayExpr={simbolSiDenumire} allowClearing />
      </Column>
    </GrilaPolitica>
  );
}
