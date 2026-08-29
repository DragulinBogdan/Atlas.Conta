import { Column } from 'devextreme-react/data-grid';
import { campMeta } from '../../nucleu/campMeta';
import { ListaNomenclator } from './ListaNomenclator';

// Lista de parteneri (72-r9). Coloanele spun ce se caută zilnic: identitatea
// fiscală (cod, denumire, CUI) și axa TVA — cea pe care o stăpânește ANAF-ul
// (`InregistratTva`, `InactivFiscal`, 72d), nu culegerea.
const cap = (membru: string) => campMeta('Partener', membru).caption;

export function Parteneri() {
  return (
    <ListaNomenclator
      titlu="Parteneri"
      entitate="Partener"
      cauta={['CodFiscal']}
      ruta="/parteneri"
      indiciu={
        <>
          Dublu-click pe un rând deschide partenerul. Căutarea nu ține cont de diacritice
          („stefan" găsește „Ștefan"). Statutul de TVA și cel de inactiv fiscal se scriu doar
          prin comanda „Sincronizează din ANAF".
        </>
      }
    >
      <Column dataField="Cod" caption={cap('Cod')} width={140} />
      <Column dataField="Denumire" caption={cap('Denumire')} />
      <Column dataField="CodFiscal" caption="Cod fiscal" width={140} />
      <Column dataField="Tara" caption={cap('Tara')} width={70} />
      <Column dataField="Localitate" caption={cap('Localitate')} width={160} />
      <Column dataField="InregistratTva" caption="TVA" dataType="boolean" width={70} />
      <Column dataField="InactivFiscal" caption="Inactiv fiscal" dataType="boolean" width={110} />
      <Column
        dataField="DataSincronizareAnaf"
        caption="Sincronizat ANAF"
        dataType="date"
        format="dd.MM.yyyy HH:mm"
        width={150}
      />
    </ListaNomenclator>
  );
}
