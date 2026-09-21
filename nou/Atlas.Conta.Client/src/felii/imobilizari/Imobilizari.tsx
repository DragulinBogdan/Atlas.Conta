import { Column } from 'devextreme-react/data-grid';
import { campMeta, labelEnum } from '../../nucleu/campMeta';
import { ListaNomenclator } from '../nomenclatoare/ListaNomenclator';
import { TIP, type Imobilizare } from './api';

const cap = (membru: string) => campMeta(TIP, membru, TIP).caption;

export function Imobilizari() {
  return (
    <ListaNomenclator
      titlu="Fișe de imobilizări"
      entitate={TIP}
      ruta="/imobilizari"
      // Tipul, clasificarea și locul vin prin `$expand` doar ca etichete: sunt
      // proprietăți de navigație, deci coloanele lor nu se sortează și nu se
      // filtrează pe server.
      expand={['TipMaterial', 'Clasificare', 'Loc']}
      sortare="NumarInventar"
      substitutCautare="Caută în număr de inventar sau denumire…"
      indiciu={(
        <>
          Dublu-click pe un rând deschide fișa. Starea și datele de punere în funcțiune / ieșire
          le scrie motorul, la operarea documentelor PIF și CAS — nu se culeg aici.
        </>
      )}
    >
      <Column dataField="NumarInventar" caption={cap('NumarInventar')} width={150} />
      <Column dataField="Denumire" caption={cap('Denumire')} />
      <Column
        dataField="TipMaterial.Cod"
        caption={cap('TipMaterialId')}
        allowSorting={false}
        allowFiltering={false}
        width={120}
      />
      <Column
        dataField="Clasificare.Cod"
        caption={cap('ClasificareId')}
        allowSorting={false}
        allowFiltering={false}
        width={120}
      />
      <Column
        dataField="Loc.Denumire"
        caption={cap('LocId')}
        allowSorting={false}
        allowFiltering={false}
        width={180}
      />
      <Column
        dataField="Stare"
        caption={cap('Stare')}
        width={130}
        // Sortarea rămâne pe COLOANA reală: un selector-funcție ar fi sortat
        // tăcut doar pagina încărcată.
        calculateSortValue="Stare"
        calculateCellValue={(r: Imobilizare) => labelEnum('StareImobilizare', r.Stare)}
      />
      <Column
        dataField="DataPunereInFunctiune"
        caption={cap('DataPunereInFunctiune')}
        dataType="date"
        format="dd.MM.yyyy"
        width={150}
      />
    </ListaNomenclator>
  );
}
