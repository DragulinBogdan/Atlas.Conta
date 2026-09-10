import { useMemo } from 'react';
import { Column, Lookup } from 'devextreme-react/data-grid';
import { storeOData } from '../../nucleu/odata';
import { GrilaPolitica } from './GrilaPolitica';
import { afisareNav, captionPolitica, codNav, codSiDenumire, urlExplica } from './comune';

// Scadența implicită per tip de document (30c): se aplică DOAR dacă scadența nu
// e culeasă pe document. Un singur rând per tip — unicitatea o dă indexul
// (F23-D3), iar gardianul dă mesajul înaintea constraint-ului.

const TIP = 'PoliticaScadenta';
const cap = (m: string) => captionPolitica(TIP, m);

export function PoliticiScadenta() {
  const tipuriDocument = useMemo(() => ({ store: storeOData('TipDocument'), sort: 'Cod' }), []);

  return (
    <GrilaPolitica
      titlu="Scadențe implicite"
      entitate={TIP}
      explica={(r) => urlExplica(codNav(r, 'TipDocument'))}
      expand={['TipDocument']}
      indiciu="Se aplică doar când scadența nu e culeasă pe document. Un singur rând per tip de document."
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
      <Column dataField="ZileDefault" caption="Zile" dataType="number" width={120} />
    </GrilaPolitica>
  );
}
