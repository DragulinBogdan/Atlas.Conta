import { useMemo } from 'react';
import { Column, Lookup } from 'devextreme-react/data-grid';
import { storeOData } from '../../nucleu/odata';
import { GrilaPolitica } from './GrilaPolitica';
import { afisareNav, codSiDenumire, optiuniEnum } from './comune';

// Documentul conex (26d): motorul îl generează ÎN tranzacția operării sursei, cu
// liniile care trec filtrul de natură. Ținta inversează laturile acolo unde
// primitorul sursei devine predatorul documentului generat.

const TIP = 'PoliticaConex';

export function PoliticiConex() {
  const tipuriDocument = useMemo(() => ({ store: storeOData('TipDocument'), sort: 'Cod' }), []);
  const naturi = useMemo(() => optiuniEnum('NaturaClasa', 'toate liniile'), []);

  return (
    <GrilaPolitica
      titlu="Documente conexe"
      entitate={TIP}
      expand={['TipDocumentSursa', 'TipDocumentTinta']}
      indiciu={(
        <>
          Documentul țintă se generează automat la operarea sursei, cu liniile care trec filtrul de
          natură (gol = toate). Copiii operați blochează anularea sursei; drafturile autogenerate se
          șterg odată cu ea.
        </>
      )}
    >
      <Column
        dataField="TipDocumentSursaId"
        caption="Tip sursă"
        calculateDisplayValue={afisareNav('TipDocumentSursa', codSiDenumire)}
        calculateSortValue="TipDocumentSursa.Cod"
        defaultSortOrder="asc"
        width={200}
      >
        <Lookup dataSource={tipuriDocument} valueExpr="ID" displayExpr={codSiDenumire} />
      </Column>
      <Column
        dataField="TipDocumentTintaId"
        caption="Tip țintă"
        calculateDisplayValue={afisareNav('TipDocumentTinta', codSiDenumire)}
        calculateSortValue="TipDocumentTinta.Cod"
        width={200}
      >
        <Lookup dataSource={tipuriDocument} valueExpr="ID" displayExpr={codSiDenumire} />
      </Column>
      <Column dataField="InverseazaLaturi" caption="Inversează laturile" dataType="boolean" width={170} />
      <Column dataField="NaturaFiltru" caption="Filtru de natură" width={200}>
        <Lookup dataSource={naturi} valueExpr="valoare" displayExpr="label" />
      </Column>
    </GrilaPolitica>
  );
}
