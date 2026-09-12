import { useMemo } from 'react';
import { Column, Lookup } from 'devextreme-react/data-grid';
import { storeOData } from '../../nucleu/odata';
import { GrilaPolitica } from './GrilaPolitica';
import {
  afisareNav, captionPolitica, codNav, codSiDenumire, optiuniEnum, urlExplica,
} from './comune';

// Regula de alimentare a registrului de stoc (23b/25): tip de document × latură
// × filtru de clasă → tip de stoc + semn. Spre deosebire de filtrele celorlalte
// politici, semnul de aici SCRIE rândul, nu îl caută — de asta n-are „orice
// semn", iar gardianul refuză orice altceva decât ±1.
//
// Semnul nu se propune după latură: seed-ul are ambele semne pe Predator.

const TIP = 'RegulaStoc';
const cap = (m: string) => captionPolitica(TIP, m);

export function ReguliStoc() {
  const tipuriDocument = useMemo(() => ({ store: storeOData('TipDocument'), sort: 'Cod' }), []);
  const clase = useMemo(() => ({ store: storeOData('ClasaProdus'), sort: 'Cod' }), []);
  const laturi = useMemo(() => optiuniEnum('LaturaDocument'), []);
  const tipuriStoc = useMemo(() => optiuniEnum('TipStoc'), []);

  return (
    <GrilaPolitica
      titlu="Reguli de stoc"
      entitate={TIP}
      explica={(r) => urlExplica(codNav(r, 'TipDocument'))}
      expand={['TipDocument', 'Clasa']}
      laRandNou={(rand) => {
        rand.Latura = 'Predator';
        rand.TipStoc = 'Magazie';
        rand.Semn = 1;
      }}
      indiciu={(
        <>
          Latura spune pe cine se scrie mișcarea (predatorul sau primitorul documentului), semnul îi dă
          direcția: −1 ieșire, +1 intrare. Clasa goală = regula generică a tipului, care se aplică doar
          liniilor cu natura „Stoc”; o clasă exactă bate genericul.
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
      <Column dataField="Latura" caption={cap('Latura')} width={140}>
        <Lookup dataSource={laturi} valueExpr="valoare" displayExpr="label" />
      </Column>
      <Column
        dataField="ClasaId"
        caption="Clasă (gol = generic)"
        calculateDisplayValue={afisareNav('Clasa', codSiDenumire)}
        calculateSortValue="Clasa.Cod"
        width={220}
      >
        <Lookup dataSource={clase} valueExpr="ID" displayExpr={codSiDenumire} allowClearing />
      </Column>
      <Column dataField="TipStoc" caption="Tip de stoc" width={170}>
        <Lookup dataSource={tipuriStoc} valueExpr="valoare" displayExpr="label" />
      </Column>
      <Column dataField="Semn" caption={cap('Semn')} dataType="number" width={110} />
    </GrilaPolitica>
  );
}
