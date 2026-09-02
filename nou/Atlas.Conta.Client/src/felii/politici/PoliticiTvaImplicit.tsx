import { useMemo } from 'react';
import { Column, Lookup } from 'devextreme-react/data-grid';
import { storeOData } from '../../nucleu/odata';
import { GrilaPolitica } from './GrilaPolitica';
import { afisareNav, captionPolitica, codSiDenumire, etichetaTipTva, optiuniEnum } from './comune';

// Implicitele de TVA (F23-D2): tip de document × clasa fiscală a partenerului ×
// „valabil de la" → tipul de TVA propus la culegere.
//
// Două lucruri se văd în felul coloanelor:
//   • Lookup-ul de EDITARE al tipului de TVA e filtrat pe `Activ` — un implicit
//     spre un tip inactiv ar fi sărit tăcut la rezolvare (F23-D3). Afișarea
//     rândurilor EXISTENTE nu trece prin lookup, ci prin `$expand`
//     (`calculateDisplayValue`), ca un rând vechi care referă un tip retras să
//     rămână vizibil, nu gol.
//   • `ClasaFiscala` nulă nu e o valoare lipsă, e regula „orice clasă"; are rând
//     propriu în lookup. La fel `Valabil de la` gol = „dintotdeauna".

const TIP = 'PoliticaTvaImplicit';
const cap = (m: string) => captionPolitica(TIP, m);

export function PoliticiTvaImplicit() {
  const tipuriDocument = useMemo(() => ({ store: storeOData('TipDocument'), sort: 'Cod' }), []);
  // `Activ eq true` pe ușa OData; `paginate` îl forțează lookup-ul pe `false`.
  const tipuriTvaActive = useMemo(
    () => ({ store: storeOData('TipTva'), filter: ['Activ', '=', true], sort: 'Cod' }), []);
  const clase = useMemo(() => optiuniEnum('ClasaFiscalaPartener', 'orice clasă'), []);

  return (
    <GrilaPolitica
      titlu="Implicite de TVA"
      entitate={TIP}
      expand={['TipDocument', 'TipTva']}
      indiciu={(
        <>
          Regimul e al PARTENERULUI, cota e a PRODUSULUI. Ordinea rezolvării la culegere:
          tipul de TVA de pe partener → rândul de aici, cel mai specific (clasa exactă bate
          „orice clasă"; la aceeași clasă bate „valabil de la" cel mai recent) → ancora tipului
          de document. Dacă produsul are tip propriu ȘI același regim, cota lui se impune peste
          rezultat. Un tip de TVA inactiv nu e ales niciodată: treapta se sare.
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
      <Column dataField="ClasaFiscala" caption={cap('ClasaFiscala')} width={260}>
        <Lookup dataSource={clase} valueExpr="valoare" displayExpr="label" />
      </Column>
      <Column
        dataField="ValabilDeLa"
        caption={cap('ValabilDeLa')}
        dataType="date"
        format="dd.MM.yyyy"
        width={140}
        customizeText={(c) => (c.value == null ? 'dintotdeauna' : c.valueText)}
      />
      <Column
        dataField="TipTvaId"
        caption={cap('TipTvaId')}
        calculateDisplayValue={afisareNav('TipTva', etichetaTipTva)}
        calculateSortValue="TipTva.Cod"
        width={220}
      >
        <Lookup dataSource={tipuriTvaActive} valueExpr="ID" displayExpr={etichetaTipTva} />
      </Column>
    </GrilaPolitica>
  );
}
