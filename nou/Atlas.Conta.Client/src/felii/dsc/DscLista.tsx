import { useMemo } from 'react';
import { useNavigate } from 'react-router';
import { Column, DataGrid, FilterRow, HeaderFilter, Pager, Paging, Search, Sorting } from 'devextreme-react/data-grid';
import { dsc, TIP_ANTET } from './api';
import { campMeta } from '../../nucleu/campMeta';

// Listă pură de citire: descărcarea nu se culege din client (F4-D2) — de aceea
// nu există buton „Nouă". Lipsa lui e contractul, nu o omisiune.
const SCHEMA_LISTA = 'DscListDto';
const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

export function DscLista() {
  const navigheaza = useNavigate();
  const sursa = useMemo(() => dsc.storeLista(), []);

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>Descărcări de gestiune</h2>
      </div>

      <DataGrid
        dataSource={sursa}
        remoteOperations
        showBorders
        columnAutoWidth
        height="calc(100vh - 170px)"
        onRowDblClick={(e) => navigheaza(`/dsc/${(e.data as { Id: string }).Id}`)}
      >
        <Sorting mode="multiple" />
        <FilterRow visible />
        <HeaderFilter visible><Search enabled /></HeaderFilter>
        <Paging defaultPageSize={25} />
        <Pager showInfo showPageSizeSelector allowedPageSizes={[25, 50, 100]} />

        <Column dataField="Numar" caption={cap('Numar')} defaultSortOrder="desc" />
        <Column dataField="Data" caption={cap('Data')} dataType="date" format="dd.MM.yyyy" />
        {/* Laturile DSC: gestiunea predă, clientul primește (37a). Numele de aici
            sunt alegerea FELIEI — captions-urile bazei („Predator (de la)") sunt
            corecte, dar abstracte în listă. */}
        <Column dataField="PredatorDenumire" caption="Gestiune" />
        <Column dataField="PrimitorDenumire" caption="Client" />
        <Column dataField="Stare" caption={cap('Stare')} />
        <Column dataField="Autogenerat" caption={cap('Autogenerat')} dataType="boolean" />
        {/* `Total` = costul descărcat, calculat de server. */}
        <Column dataField="Total" caption={cap('Total')} dataType="number" format="#,##0.00" alignment="right" />
      </DataGrid>

      <p className="indiciu">
        Dublu-click pe un rând deschide documentul. Descărcările se nasc din operarea facturii de ieșire
        sau din comanda de backorder de pe factură.
      </p>
    </div>
  );
}
