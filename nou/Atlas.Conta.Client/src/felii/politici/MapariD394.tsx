import { useMemo } from 'react';
import { Column, Lookup } from 'devextreme-react/data-grid';
import { storeOData } from '../../nucleu/odata';
import { GrilaPolitica } from './GrilaPolitica';
import { afisareNav, captionPolitica, etichetaTipTva, optiuniEnum } from './comune';

// `(TipTva × Sens) → tip D394` sau excludere cu motiv (71c, 123 a); potrivirea cu sensul și cu D300 o judecă gardianul.

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
      laRandNou={(rand) => { rand.SensTva = 'Achizitie'; rand.Tip = 'A'; }}
      indiciu={(
        <>
          O singură mapare per pereche (tip de TVA × sens). Ținta trebuie să se potrivească cu sensul —
          pe livrare L, V sau LS, pe achiziție A, C sau AS — și cu rândul D300 al aceleiași perechi.
          Fără tip, perechea e exclusă din D394 și cere motivul. Perechea nemapată oprește fișierul D394.
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
      <Column dataField="SensTva" caption={cap('SensTva')} width={140}>
        <Lookup dataSource={sensuri} valueExpr="valoare" displayExpr="label" />
      </Column>
      <Column dataField="Tip" caption={cap('Tip')}>
        <Lookup dataSource={tipuriOperatiune} valueExpr="valoare" displayExpr="label" allowClearing />
      </Column>
      <Column dataField="Motiv" caption={cap('Motiv')} minWidth={240} />
    </GrilaPolitica>
  );
}
