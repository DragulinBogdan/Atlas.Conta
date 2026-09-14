import { useMemo } from 'react';
import { Column, Lookup } from 'devextreme-react/data-grid';
import { azi } from '../../nucleu/zi';
import { GrilaPolitica } from './GrilaPolitica';
import { captionPolitica, optiuniEnum } from './comune';

// Deductibilitatea fiscală a amortizării, versionată în timp (F26-D8): regula
// aplicabilă e rândul cu `DeLa` maxim ≤ data lunii, per (Categorie, Fel).

const TIP = 'RegulaDeductibilitate';
const cap = (m: string) => captionPolitica(TIP, m);

export function ReguliDeductibilitate() {
  const categorii = useMemo(() => optiuniEnum('CategorieFiscala'), []);
  const feluri = useMemo(() => optiuniEnum('FelDeductibilitate'), []);

  return (
    <GrilaPolitica
      titlu="Deductibilitate fiscală"
      entitate={TIP}
      laRandNou={(rand) => {
        rand.Categorie = 'Standard';
        rand.Fel = 'PlafonLunar';
        rand.DoarNeexclusiv = false;
        rand.DeLa = azi();
      }}
      indiciu={(
        <>
          Se aplică rândul cu „De la” cel mai recent până la data lunii amortizate, pentru fiecare
          pereche categorie × fel. Plafonul lunar e în lei pe lună; procentul 0 înseamnă
          nedeductibil. „Doar la utilizare neexclusivă” sare regula pentru fișele marcate ca folosite
          exclusiv în interesul activității.
        </>
      )}
    >
      <Column
        dataField="Categorie"
        caption={cap('Categorie')}
        defaultSortOrder="asc"
        defaultSortIndex={0}
        width={260}
      >
        <Lookup dataSource={categorii} valueExpr="valoare" displayExpr="label" />
      </Column>
      <Column dataField="DoarNeexclusiv" caption={cap('DoarNeexclusiv')} dataType="boolean" width={190} />
      <Column dataField="Fel" caption={cap('Fel')} width={140}>
        <Lookup dataSource={feluri} valueExpr="valoare" displayExpr="label" />
      </Column>
      <Column
        dataField="Valoare"
        caption={cap('Valoare')}
        dataType="number"
        format="#,##0.00"
        alignment="right"
        width={130}
      />
      <Column
        dataField="DeLa"
        caption={cap('DeLa')}
        dataType="date"
        format="dd.MM.yyyy"
        defaultSortOrder="desc"
        defaultSortIndex={1}
        width={120}
      />
      <Column dataField="PanaLa" caption={cap('PanaLa')} dataType="date" format="dd.MM.yyyy" width={120} />
      <Column dataField="Temei" caption={cap('Temei')} />
    </GrilaPolitica>
  );
}
