import { useMemo } from 'react';
import { Column, Lookup } from 'devextreme-react/data-grid';
import { GrilaPolitica } from './GrilaPolitica';
import { captionPolitica, optiuniEnum } from './comune';

// Severitatea constatărilor de închidere de perioadă (F27-D2): un rând per fel,
// cu `Ignorat` ca „nu mă opri cu asta”. Blocantele STRUCTURALE ale lanțului (o
// lună nedefinită, una deja închisă, precedenta deschisă) NU sunt aici — ele nu
// se configurează: sunt chiar regula lanțului.

const TIP = 'PoliticaInchidere';
const cap = (m: string) => captionPolitica(TIP, m);

export function PoliticiInchidere() {
  const feluri = useMemo(() => optiuniEnum('FelConstatareInchidere'), []);
  const severitati = useMemo(() => optiuniEnum('SeveritateConstatare'), []);

  return (
    <GrilaPolitica
      titlu="Închidere de perioadă"
      entitate={TIP}
      laRandNou={(rand) => {
        rand.Fel = 'DraftInPerioada';
        rand.Severitate = 'Avertisment';
      }}
      indiciu={(
        <>
          Un rând per fel de constatare. „Blocant” refuză închiderea până când faptul dispare și nu
          se poate accepta; „Avertisment” o lasă să treacă doar cu acceptare explicită pe constatarea
          concretă, scrisă în istoricul lunii; „Ignorat” nu emite constatarea deloc. Un fel fără rând
          se comportă ca avertisment și o spune pe ecran.
        </>
      )}
    >
      <Column
        dataField="Fel"
        caption={cap('Fel')}
        defaultSortOrder="asc"
        defaultSortIndex={0}
        width={320}
      >
        <Lookup dataSource={feluri} valueExpr="valoare" displayExpr="label" />
      </Column>
      <Column dataField="Severitate" caption={cap('Severitate')} width={200}>
        <Lookup dataSource={severitati} valueExpr="valoare" displayExpr="label" />
      </Column>
    </GrilaPolitica>
  );
}
