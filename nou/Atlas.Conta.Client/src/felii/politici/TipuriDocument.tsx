import { useMemo } from 'react';
import { Column, Lookup } from 'devextreme-react/data-grid';
import { storeOData } from '../../nucleu/odata';
import { GrilaPolitica } from './GrilaPolitica';
import { afisareNav, captionPolitica, etichetaTipTva } from './comune';

// Ancora tipurilor de document (decizia 20): oglindește clasele 1:1. Nu e un
// nomenclator — e o listă de chei străine pentru rândurile de politică.
//
// De aceea grila n-are „Nou" și n-are „Șterge", iar `Cod`/`Denumire`/`ClrType`
// sunt read-only: nu e o restricție de permisiune (aia o dă serverul), e faptul
// că un tip de document E o clasă din cod. Gardianul refuză oricum și crearea, și
// ștergerea, și schimbarea codului — aici doar nu se oferă gestul.
// Singurul câmp editabil e implicitul de TVA (fallback-ul de ultimă treaptă al
// rezolvării, F23-D2).

const TIP = 'TipDocument';
const cap = (m: string) => captionPolitica(TIP, m);

export function TipuriDocument() {
  const tipuriTvaActive = useMemo(
    () => ({ store: storeOData('TipTva'), filter: ['Activ', '=', true], sort: 'Cod' }), []);

  return (
    <GrilaPolitica
      titlu="Tipuri de document (ancoră)"
      entitate={TIP}
      expand={['TipTvaImplicit']}
      poateAdauga={false}
      poateSterge={false}
      indiciu={(
        <>
          Codul e IDENTITATEA tipului — politicile îl referă prin el, iar clasele de document
          sunt cod, nu date (decizia 20). Se poate schimba doar tipul de TVA implicit: ultima
          treaptă a rezolvării, cea care se aplică atunci când nici partenerul, nici o politică,
          nici produsul nu spun altceva.
        </>
      )}
    >
      <Column dataField="Cod" caption={cap('Cod')} width={110} allowEditing={false} defaultSortOrder="asc" />
      <Column dataField="Denumire" caption={cap('Denumire')} allowEditing={false} />
      <Column dataField="ClrType" caption="Clasa (cod)" width={200} allowEditing={false} />
      <Column
        dataField="TipTvaImplicitId"
        caption="Tip TVA implicit"
        calculateDisplayValue={afisareNav('TipTvaImplicit', etichetaTipTva)}
        calculateSortValue="TipTvaImplicit.Cod"
        width={220}
      >
        <Lookup dataSource={tipuriTvaActive} valueExpr="ID" displayExpr={etichetaTipTva} allowClearing />
      </Column>
    </GrilaPolitica>
  );
}
