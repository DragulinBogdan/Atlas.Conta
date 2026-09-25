import { useMemo } from 'react';
import { Link } from 'react-router';
import {
  Column, ColumnFixing, DataGrid, FilterRow, HeaderFilter, Pager, Paging, Search, Sorting,
} from 'devextreme-react/data-grid';
import type { components } from '../../generated/api-types';
import { storeRemote } from '../../nucleu/dxStore';
import { rutaTip } from '../../nucleu/stingeri';
import { urlCu, useUrlStare } from '../../nucleu/urlStare';
import { CasetaPerioada } from './comune';

type JurnalRand = components['schemas']['JurnalRand'];
const camp = (n: keyof JurnalRand & string) => n;

export function RegistruJurnal() {
  const [stare, seteaza] = useUrlStare({ dataStart: '', dataEnd: '' });

  const sursa = useMemo(() => storeRemote(urlCu('/api/proiectii/registru-jurnal', stare), ['Id', 'Spatiu']), [stare]);

  return (
    <div className="ecran">
      <div className="ecran__bara"><h2>Registru-jurnal</h2></div>

      <div className="bara-raport">
        <CasetaPerioada dataStart={stare.dataStart} dataEnd={stare.dataEnd} optionala seteaza={seteaza} />
        <span className="indiciu">Fără perioadă: tot registrul.</span>
      </div>

      <DataGrid
        key={`${stare.dataStart}|${stare.dataEnd}`}
        dataSource={sursa}
        remoteOperations={{ filtering: true, sorting: true, paging: true, grouping: true }}
        showBorders
        columnAutoWidth
        height="calc(100vh - 220px)"
      >
        <Sorting mode="multiple" />
        <FilterRow visible />
        <HeaderFilter visible><Search enabled /></HeaderFilter>
        <ColumnFixing enabled />
        <Paging defaultPageSize={50} />
        <Pager showInfo showPageSizeSelector allowedPageSizes={[50, 100, 200]} />

        <Column dataField={camp('Data')} caption="Data" dataType="date" format="dd.MM.yyyy" width={100} fixed />
        <Column dataField={camp('NumarNota')} caption="Nr. notă" width={110} />
        <Column dataField={camp('TranzactieId')} caption="Tranzacție" visible={false} />
        <Column dataField={camp('ContSimbol')} caption="Cont" width={120} />
        <Column dataField={camp('Sens')} caption="Sens" width={70} />
        <Column dataField={camp('Debit')} caption="Debit" dataType="number" format="#,##0.00" alignment="right" width={140} />
        <Column dataField={camp('Credit')} caption="Credit" dataType="number" format="#,##0.00" alignment="right" width={140} />
        <Column
          dataField={camp('DocumentNumar')}
          caption="Document"
          width={150}
          allowFiltering={false}
          allowSorting={false}
          cellRender={celulaDocument}
        />
        <Column dataField={camp('Storno')} caption="Storno" dataType="boolean" width={80} />
      </DataGrid>

      <p className="indiciu">Rândurile fără document sunt solduri de deschidere; rândurile de storno intră normal.</p>
    </div>
  );
}

function celulaDocument({ data }: { data: JurnalRand }) {
  if (!data.DocumentId) return <span className="indiciu">(deschidere)</span>;
  const ruta = rutaTip(data.DocumentTip, data.DocumentId);
  const text = data.DocumentNumar || data.DocumentTip || '(document)';
  return ruta ? <Link to={ruta}>{text}</Link> : <span>{text}</span>;
}
