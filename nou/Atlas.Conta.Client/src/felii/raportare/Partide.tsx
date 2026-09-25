import { useMemo } from 'react';
import { Link } from 'react-router';
import { DateBox } from 'devextreme-react';
import { Column, DataGrid, FilterRow, Pager, Paging, Sorting } from 'devextreme-react/data-grid';
import type { components } from '../../generated/api-types';
import { storeRemote } from '../../nucleu/dxStore';
import { rutaTip } from '../../nucleu/stingeri';
import { urlCu, useUrlStare } from '../../nucleu/urlStare';
import { azi, izolataZi } from '../../nucleu/zi';

type Rand = components['schemas']['PartidaCuRestRand'];
const camp = (n: keyof Rand & string) => n;
const BANI = { dataType: 'number', format: '#,##0.00', alignment: 'right', width: 135 } as const;

export function Partide() {
  const [stare, seteaza] = useUrlStare({ laData: azi(), contrapartidaId: '', sens: '' });
  const sursa = useMemo(() => storeRemote(urlCu('/api/proiectii/partide', stare),
    ['UnitateId', 'ContId', 'ContrapartidaId']), [stare]);
  return (
    <div className="ecran">
      <div className="ecran__bara"><h2>Partide deschise</h2></div>
      <div className="bara-raport">
        <label className="bara-raport__camp">
          <span className="camp__eticheta">La data</span>
          <DateBox value={stare.laData} displayFormat="dd.MM.yyyy" width={160}
            onValueChanged={(e) => { if (e.event) seteaza({ laData: izolataZi(e.value) ?? azi() }); }} />
        </label>
      </div>
      <DataGrid dataSource={sursa} remoteOperations={{ filtering: true, sorting: true, paging: true }}
        showBorders columnAutoWidth height="calc(100vh - 220px)">
        <Sorting mode="multiple" /><FilterRow visible />
        <Paging defaultPageSize={50} /><Pager showInfo showPageSizeSelector allowedPageSizes={[50, 100, 200]} />
        <Column dataField={camp('ContSimbol')} caption="Cont" />
        <Column dataField={camp('ContrapartidaDenumire')} caption="Partener / angajat" />
        <Column dataField={camp('Numar')} caption="Document" cellRender={document} />
        <Column dataField={camp('Data')} caption="Deschisă la" dataType="date" format="dd.MM.yyyy" />
        <Column dataField={camp('Sens')} caption="Sens" />
        <Column dataField={camp('Rest')} caption="Rest" {...BANI} />
        <Column dataField={camp('UnitateId')} caption="Identitate partidă" visible={false} />
      </DataGrid>
      <p className="indiciu">Fiecare rând este o partidă distinctă pe cont și partener.
        Deschiderile fără document sunt incluse. Rândurile cu rest zero nu apar.</p>
    </div>
  );
}

function document({ data }: { data: Rand }) {
  if (!data.DocumentId) return <span className="indiciu">Fără document</span>;
  const ruta = rutaTip(data.DocumentTip, data.DocumentId);
  const text = `${data.DocumentTip ?? ''} ${data.Numar ?? ''}`.trim() || 'Document';
  return ruta ? <Link to={ruta}>{text}</Link> : <span>{text}</span>;
}
