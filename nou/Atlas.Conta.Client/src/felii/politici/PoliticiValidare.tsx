import { useMemo } from 'react';
import { Column, Lookup } from 'devextreme-react/data-grid';
import { storeOData } from '../../nucleu/odata';
import { GrilaPolitica } from './GrilaPolitica';
import {
  afisareNav, captionPolitica, codNav, codSiDenumire, optiuniEnum, urlExplica,
} from './comune';

// Profilul de VALIDARE per tip (33c): motorul îl aplică înaintea hook-ului
// propriu tipului. Regula bugetară („angajament SAU cod economic”) e a
// profilului, nu a clasei de document — la privat rândurile pur și simplu
// lipsesc, dar coloana rămâne pe ambele profiluri (DIM-4).

const TIP = 'PoliticaValidare';
const cap = (m: string) => captionPolitica(TIP, m);

export function PoliticiValidare() {
  const tipuriDocument = useMemo(() => ({ store: storeOData('TipDocument'), sort: 'Cod' }), []);
  const naturi = useMemo(() => optiuniEnum('NaturaClasa', 'niciuna'), []);

  return (
    <GrilaPolitica
      titlu="Validări per tip de document"
      entitate={TIP}
      explica={(r) => urlExplica(codNav(r, 'TipDocument'))}
      expand={['TipDocument']}
      indiciu={(
        <>
          Se aplică la operare, înaintea validărilor proprii tipului. Natura interzisă refuză liniile cu
          clasa respectivă — așa rămâne factura de ieșire pură creanță acolo unde profilul nu descarcă
          gestiune din ea.
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
        dataField="CereClasificatieBugetara"
        caption="Cere clasificație bugetară"
        dataType="boolean"
        width={210}
      />
      <Column dataField="NaturaInterzisa" caption="Natură interzisă" width={200}>
        <Lookup dataSource={naturi} valueExpr="valoare" displayExpr="label" />
      </Column>
    </GrilaPolitica>
  );
}
