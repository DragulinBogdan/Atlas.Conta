import { useMemo } from 'react';
import { Column, Lookup } from 'devextreme-react/data-grid';
import { storeOData } from '../../nucleu/odata';
import { GrilaPolitica } from './GrilaPolitica';
import {
  afisareNav, captionPolitica, codSiDenumire, editorCautare, simbolSiDenumire,
} from './comune';

// Conturile amortizării, per tip de material de clasă F (F26-D6). O fișă al
// cărei tip n-are rând aici blochează generarea lunii („o fișă eligibilă n-are
// politică de amortizare”), iar ieșirea nu-și poate produce notele.

const TIP = 'PoliticaAmortizare';
const cap = (m: string) => captionPolitica(TIP, m);

export function PoliticiAmortizare() {
  const tipuriMaterial = useMemo(() => ({ store: storeOData('TipMaterial'), sort: 'Cod' }), []);
  const conturi = useMemo(
    () => ({ store: storeOData('Cont'), sort: 'Simbol', paginate: true, pageSize: 50 }), []);

  return (
    <GrilaPolitica
      titlu="Amortizare (conturi)"
      entitate={TIP}
      expand={['TipMaterial', 'ContAmortizare', 'ContCheltuialaAmortizare', 'ContCheltuialaCedare']}
      indiciu={(
        <>
          Rândul e pe tipul (contul) fișei, nu pe fișă. Amortizarea lunară postează cheltuiala cu
          amortizarea pe contul de amortizare; cedarea scoate brutul, cu valoarea rămasă pe contul
          de cheltuială cu cedarea. Un tip fără rând aici oprește generarea lunii.
        </>
      )}
    >
      <Column
        dataField="TipMaterialId"
        caption={cap('TipMaterialId')}
        calculateDisplayValue={afisareNav('TipMaterial', codSiDenumire)}
        calculateSortValue="TipMaterial.Cod"
        defaultSortOrder="asc"
        editorOptions={editorCautare}
        width={220}
      >
        <Lookup dataSource={tipuriMaterial} valueExpr="ID" displayExpr={codSiDenumire} />
      </Column>
      <Column
        dataField="ContAmortizareId"
        caption={cap('ContAmortizareId')}
        calculateDisplayValue={afisareNav('ContAmortizare', simbolSiDenumire)}
        calculateSortValue="ContAmortizare.Simbol"
        editorOptions={editorCautare}
        width={240}
      >
        <Lookup dataSource={conturi} valueExpr="ID" displayExpr={simbolSiDenumire} allowClearing />
      </Column>
      <Column
        dataField="ContCheltuialaAmortizareId"
        caption={cap('ContCheltuialaAmortizareId')}
        calculateDisplayValue={afisareNav('ContCheltuialaAmortizare', simbolSiDenumire)}
        calculateSortValue="ContCheltuialaAmortizare.Simbol"
        editorOptions={editorCautare}
        width={260}
      >
        <Lookup dataSource={conturi} valueExpr="ID" displayExpr={simbolSiDenumire} allowClearing />
      </Column>
      <Column
        dataField="ContCheltuialaCedareId"
        caption={cap('ContCheltuialaCedareId')}
        calculateDisplayValue={afisareNav('ContCheltuialaCedare', simbolSiDenumire)}
        calculateSortValue="ContCheltuialaCedare.Simbol"
        editorOptions={editorCautare}
        width={260}
      >
        <Lookup dataSource={conturi} valueExpr="ID" displayExpr={simbolSiDenumire} allowClearing />
      </Column>
    </GrilaPolitica>
  );
}
