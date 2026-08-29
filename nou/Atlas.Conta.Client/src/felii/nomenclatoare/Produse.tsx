import { Column } from 'devextreme-react/data-grid';
import { campMeta } from '../../nucleu/campMeta';
import { ListaNomenclator } from './ListaNomenclator';

// Lista de produse (73-r6). Coloanele care contează la SAF-T sunt aici, alături
// de identitate: `CodNc` (`ProductCommodityCode`) și unitatea de măsură din
// nomenclatorul UN/ECE — cele două lipsuri care ies ca avertisment agregat în
// fișier și pentru care ecranul ăsta există.
const cap = (membru: string) => campMeta('Produs', membru, 'Produs').caption;

export function Produse() {
  return (
    <ListaNomenclator
      titlu="Produse"
      entitate="Produs"
      ruta="/produse"
      // Tipul și unitatea vin prin `$expand` doar ca ETICHETE de afișat.
      // Coloanele lor nu se sortează și nu se filtrează pe server (proprietăți
      // de navigație), de aceea sunt marcate explicit — o coloană care pare
      // sortabilă și nu e ar fi un buton care tace.
      expand={['TipMaterial', 'UnitateMasura']}
      indiciu={
        <>
          Dublu-click pe un rând deschide produsul. Codul NC are exact 8 cifre sau rămâne gol —
          produsul fără NC iese în SAF-T cu „0" și un avertisment agregat, nu e refuzat.
        </>
      }
    >
      <Column dataField="Cod" caption={cap('Cod')} width={200} />
      <Column dataField="Denumire" caption={cap('Denumire')} />
      <Column dataField="UM" caption={cap('UM')} width={80} />
      <Column
        dataField="TipMaterial.Denumire"
        caption={cap('TipMaterial')}
        allowSorting={false}
        allowFiltering={false}
        width={200}
      />
      <Column
        dataField="UnitateMasura.Cod"
        caption="UM SAF-T"
        allowSorting={false}
        allowFiltering={false}
        width={100}
      />
      <Column dataField="CodNc" caption={cap('CodNc')} width={110} />
    </ListaNomenclator>
  );
}
