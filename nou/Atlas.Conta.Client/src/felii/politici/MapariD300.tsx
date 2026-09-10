import { useMemo } from 'react';
import { Column, Lookup } from 'devextreme-react/data-grid';
import { storeOData } from '../../nucleu/odata';
import { GrilaPolitica } from './GrilaPolitica';
import { afisareNav, captionPolitica, codSiDenumire, etichetaTipTva, optiuniEnum } from './comune';

// Așezarea unei operațiuni pe rândul decontului (69b): `(TipTva × Sens) → rând`,
// cu n rânduri per pereche. Lookup-ul rândurilor e filtrat pe „Operațiuni” —
// AFORDANȚĂ, nu garanție: refuzul unui rând de total/oglindă/extern rămâne al
// gardianului, ca și gardul contra dublei numărări pe aceeași verticală.

const TIP = 'MapareD300';
const cap = (m: string) => captionPolitica(TIP, m);

export function MapariD300() {
  const tipuriTva = useMemo(() => ({ store: storeOData('TipTva'), sort: 'Cod' }), []);
  const randuri = useMemo(
    () => ({ store: storeOData('RandD300'), filter: ['Fel', '=', 'Operatiuni'], sort: 'Ordine' }), []);
  const sensuri = useMemo(() => optiuniEnum('SensTva'), []);

  return (
    <GrilaPolitica
      titlu="Mapări D300"
      entitate={TIP}
      expand={['TipTva', 'Rand']}
      laRandNou={(rand) => { rand.Sens = 'Achizitie'; }}
      indiciu={(
        <>
          Perechea e DIRECȚIONALĂ: același tip de TVA cade pe alt rând la achiziție decât la livrare.
          Lista oferă doar rândurile de operațiuni — totalurile și oglinzile se calculează. O pereche
          nemapată nu e eroare: cifrele ei apar în panoul „neincluse” al decontului.
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
      <Column
        dataField="RandId"
        caption={cap('RandId')}
        calculateDisplayValue={afisareNav('Rand', codSiDenumire)}
        calculateSortValue="Rand.Ordine"
      >
        <Lookup dataSource={randuri} valueExpr="ID" displayExpr={codSiDenumire} />
      </Column>
    </GrilaPolitica>
  );
}
