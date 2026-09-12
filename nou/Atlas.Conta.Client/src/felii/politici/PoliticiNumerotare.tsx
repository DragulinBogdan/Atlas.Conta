import { useMemo } from 'react';
import { Column, Lookup } from 'devextreme-react/data-grid';
import { storeOData } from '../../nucleu/odata';
import { GrilaPolitica } from './GrilaPolitica';
import { afisareNav, captionPolitica, codNav, codSiDenumire, urlExplica } from './comune';

// Numerotarea server-owned (25f): numărul se asignează abia la MATERIALIZARE, din
// rândul de aici. `Format` e opțional în motor (seed-ul nu-l scrie), deci
// gardianul cere doar să fie compunabil, nu nevid.
//
// `UrmatorulNumar` e un contor viu: editarea lui mută seria. Nu e ascunsă —
// e chiar gestul pentru care ecranul există (preluarea unei plaje de la o altă
// aplicație) — dar rândul devine „manual", iar raportul de profil îl arată.

const TIP = 'PoliticaNumerotare';
const cap = (m: string) => captionPolitica(TIP, m);

export function PoliticiNumerotare() {
  const tipuriDocument = useMemo(() => ({ store: storeOData('TipDocument'), sort: 'Cod' }), []);

  return (
    <GrilaPolitica
      titlu="Numerotare"
      entitate={TIP}
      explica={(r) => urlExplica(codNav(r, 'TipDocument'))}
      expand={['TipDocument']}
      indiciu={(
        <>
          Numărul se asignează la materializare, nu la culegere — pe factura de intrare numărul
          e al furnizorului și nu trece pe aici. „Următorul număr" e un contor viu: modificarea
          lui mută seria de la următorul document operat.
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
      <Column dataField="Serie" caption={cap('Serie')} width={140} />
      <Column dataField="UrmatorulNumar" caption="Următorul număr" dataType="number" width={150} />
      <Column dataField="Format" caption={cap('Format')} />
    </GrilaPolitica>
  );
}
