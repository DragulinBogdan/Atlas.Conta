import { useCallback, useMemo } from 'react';
import { Link } from 'react-router';
import { Column, ColumnChooser, DataGrid, FilterRow, Pager, Paging, Sorting } from 'devextreme-react/data-grid';
import { SelectBox, CheckBox } from 'devextreme-react';
import type { ValueChangedEvent as SelectChanged } from 'devextreme/ui/select_box';
import type { ValueChangedEvent as CheckChanged } from 'devextreme/ui/check_box';
import type { components } from '../../generated/api-types';
import { storeRemote } from '../../nucleu/dxStore';
import { storeOData } from '../../nucleu/odata';
import { urlCu, useUrlStare } from '../../nucleu/urlStare';
import { CasetaPerioada, lunaCurenta } from '../raportare/comune';

type Rand = components['schemas']['RandDiagnosticTva'];
const camp = (n: keyof Rand & string) => n;
const pagini = [50, 100, 200];
const bani = { dataType: 'number', format: '#,##0.00', width: 110 } as const;

export function DiagnosticTva() {
  const luna = lunaCurenta();
  const [stare, seteaza] = useUrlStare({ dataStart: luna.start, dataEnd: luna.sfarsit,
    tipTvaId: '', includeCompensate: false });
  const sursa = useMemo(() => storeRemote(urlCu('/api/proiectii/diagnostic-tva', stare),
    ['DocumentId', 'LinieId', 'Cod']), [stare]);
  const tipuri = useMemo(() => ({ store: storeOData('TipTva'), sort: 'Cod', select: ['ID', 'Cod'] }), []);
  const tipSchimbat = useCallback((e: SelectChanged) => {
    if (e.event) seteaza({ tipTvaId: e.value ?? '' });
  }, [seteaza]);
  const istoricSchimbat = useCallback((e: CheckChanged) => {
    if (e.event) seteaza({ includeCompensate: e.value ?? false });
  }, [seteaza]);
  return <div className="ecran">
    <div className="ecran__bara"><h2>Impact și avertismente TVA</h2>
      <Link to="/politici/tipuri-tva">Tipuri de TVA</Link></div>
    <div className="bara-raport">
      <CasetaPerioada dataStart={stare.dataStart} dataEnd={stare.dataEnd} seteaza={seteaza} optionala />
      <label className="bara-raport__camp"><span className="camp__eticheta">Tip TVA</span>
        <SelectBox dataSource={tipuri} valueExpr="ID" displayExpr="Cod" value={stare.tipTvaId || null}
          showClearButton searchEnabled onValueChanged={tipSchimbat} /></label>
      <CheckBox text="Include avertismente compensate" value={stare.includeCompensate} onValueChanged={istoricSchimbat} />
    </div>
    <DataGrid dataSource={sursa} remoteOperations showBorders height="calc(100vh - 260px)">
      <Sorting mode="multiple" /><FilterRow visible /><ColumnChooser enabled /><Paging defaultPageSize={50} />
      <Pager showInfo showPageSizeSelector allowedPageSizes={pagini} />
      <Column dataField={camp('Numar')} caption="Document" width={150} />
      <Column dataField={camp('Exigibilitate')} caption="Exigibilitate" dataType="date" format="dd.MM.yyyy" width={120} />
      <Column dataField={camp('Cod')} caption="Cod avertisment" width={260} />
      <Column dataField={camp('Motiv')} caption="Explicație" width={380} />
      <Column dataField={camp('LinieId')} caption="Linie" width={160} visible={false} />
      <Column dataField={camp('Registru')} caption="Registru" width={135} />
      <Column dataField={camp('Stare')} caption="Stare curentă" width={120} />
      <Column dataField={camp('TipTvaCod')} caption="Tip TVA" width={100} />
      <Column dataField="Calificare.Cota" caption="Cotă fapt / draft" dataType="number" width={115} />
      <Column dataField="CalificareActuala.Cota" caption="Cotă actuală" dataType="number" width={110} />
      <Column dataField="Calificare.Regim" caption="Regim fapt / draft" width={140} />
      <Column dataField="CalificareActuala.Regim" caption="Regim actual" width={140} />
      <Column dataField="Calificare.DeImport" caption="Import fapt / draft" dataType="boolean" width={120} />
      <Column dataField="CalificareActuala.DeImport" caption="Import actual" dataType="boolean" width={110} />
      <Column dataField={camp('Baza')} caption="Bază" {...bani} />
      <Column dataField={camp('Taxa')} caption="Taxă" {...bani} />

    </DataGrid>
    <p className="indiciu">Perioada filtrează exigibilitatea. Raportul arată starea curentă:
      o inversă dintr-o lună ulterioară compensează originalul. Drafturile folosesc calificarea curentă;
      faptele operate o păstrează pe cea istorică. Nicio sumă nu se modifică din acest raport.</p>
  </div>;
}
