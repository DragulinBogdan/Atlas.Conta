import { useMemo } from 'react';
import { Column, Lookup } from 'devextreme-react/data-grid';
import { storeOData } from '../../nucleu/odata';
import { GrilaPolitica } from './GrilaPolitica';
import { afisareNav, captionPolitica, etichetaTipTva, optiuniEnum } from './comune';

// Așezarea unei operațiuni pe tipul de operațiune din secțiunea 2 a D394 (71c):
// `(TipTva × Sens) → tip`, o singură mapare per pereche. Lista de tipuri e
// ÎNTREAGĂ, iar potrivirea cu sensul rămâne a gardianului — pe livrare doar
// L/V/LS, pe achiziție doar A/C/AS; AÎ se derivă din partener, N n-are sursă în
// registrul de TVA.

const TIP = 'MapareD394';
const cap = (m: string) => captionPolitica(TIP, m);

export function MapariD394() {
  const tipuriTva = useMemo(() => ({ store: storeOData('TipTva'), sort: 'Cod' }), []);
  const sensuri = useMemo(() => optiuniEnum('SensTva'), []);
  const tipuriOperatiune = useMemo(() => optiuniEnum('TipOperatiuneD394'), []);

  return (
    <GrilaPolitica
      titlu="Mapări D394"
      entitate={TIP}
      expand={['TipTva']}
      laRandNou={(rand) => { rand.Sens = 'Achizitie'; rand.Tip = 'A'; }}
      indiciu={(
        <>
          O singură mapare per pereche (tip de TVA × sens). Ținta trebuie să se potrivească cu sensul —
          pe livrare L, V sau LS, pe achiziție A, C sau AS. Perechea nemapată apare în panoul
          „neincluse”, cu cifrele ei.
        </>
      )}
    >
      <Column
        dataField="TipTvaId"
        caption={cap('TipTvaId')}
        calculateDisplayValue={afisareNav('TipTva', etichetaTipTva)}
        calculateSortValue="TipTva.Cod"
        defaultSortOrder="asc"
        width={280}
      >
        <Lookup dataSource={tipuriTva} valueExpr="ID" displayExpr={etichetaTipTva} />
      </Column>
      <Column dataField="Sens" caption={cap('Sens')} width={140}>
        <Lookup dataSource={sensuri} valueExpr="valoare" displayExpr="label" />
      </Column>
      <Column dataField="Tip" caption={cap('Tip')}>
        <Lookup dataSource={tipuriOperatiune} valueExpr="valoare" displayExpr="label" />
      </Column>
    </GrilaPolitica>
  );
}
