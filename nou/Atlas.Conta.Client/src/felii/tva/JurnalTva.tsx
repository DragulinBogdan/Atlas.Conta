import { useMemo } from 'react';
import { Link } from 'react-router';
import {
  Column, ColumnFixing, DataGrid, FilterRow, HeaderFilter, Pager, Paging, Search, Sorting,
  Summary, TotalItem,
} from 'devextreme-react/data-grid';
import type { components } from '../../generated/api-types';
import { labelEnum } from '../../nucleu/campMeta';
import { storeRemote } from '../../nucleu/dxStore';
import { rutaTip } from '../../nucleu/stingeri';
import { urlCu, useUrlStare } from '../../nucleu/urlStare';
import { CasetaPerioada, lunaCurenta } from '../raportare/comune';

type JurnalTvaRand = components['schemas']['JurnalTvaRand'];
const camp = (n: keyof JurnalTvaRand & string) => n;

type Sens = 'Achizitie' | 'Livrare';

function JurnalTva({ sens, titlu }: { sens: Sens; titlu: string }) {
  const luna = lunaCurenta();

  const [stare, seteaza] = useUrlStare({ dataStart: luna.start, dataEnd: luna.sfarsit });

  const sursa = useMemo(() => storeRemote(
    urlCu('/api/proiectii/jurnal-tva', { sens, ...stare }),

    ['DocumentId', 'TipTvaId', 'Storno', 'Regim', 'Cota', 'PerioadaAn', 'PerioadaLuna'],
  ), [sens, stare]);

  return (
    <div className="ecran">
      <div className="ecran__bara"><h2>{titlu}</h2></div>

      <div className="bara-raport">
        <CasetaPerioada dataStart={stare.dataStart} dataEnd={stare.dataEnd} seteaza={seteaza} />
      </div>

      <DataGrid

        key={`${sens}|${stare.dataStart}|${stare.dataEnd}`}
        dataSource={sursa}

        remoteOperations={{ filtering: true, sorting: true, paging: true, summary: true, grouping: true }}
        showBorders

        height="calc(100vh - 220px)"
      >
        <Sorting mode="multiple" />
        <FilterRow visible />
        <HeaderFilter visible><Search enabled /></HeaderFilter>
        <ColumnFixing enabled />
        <Paging defaultPageSize={50} />
        <Pager showInfo showPageSizeSelector allowedPageSizes={[50, 100, 200]} />

        <Column dataField={camp('DataDocument')} caption="Data documentului" dataType="date" format="dd.MM.yyyy" width={100} fixed />
        <Column
          dataField={camp('PerioadaAn')}
          caption="Perioada D300"
          width={100}
          allowFiltering={false}
          cellRender={celulaPerioada}
        />
        <Column
          dataField={camp('DocumentNumar')}
          caption="Document"
          width={150}
          fixed

          allowFiltering={false}
          allowSorting={false}
          cellRender={celulaDocument}
        />
        <Column dataField={camp('PartenerDenumire')} caption="Partener" minWidth={180} cellRender={celulaPartener} />
        <Column dataField={camp('PartenerCodFiscal')} caption="Cod fiscal" width={130} />

        <Column dataField={camp('TipTvaCod')} caption="Tip TVA" width={90} cellRender={celulaTipTva} />
        <Column dataField={camp('TipTvaDenumire')} caption="Denumire TVA" width={200} />
        <Column dataField={camp('Regim')} caption="Regim" width={130} cellRender={celulaRegim} />
        <Column dataField={camp('Cota')} caption="Cotă %" dataType="number" format="#0.##" alignment="right" width={80} />
        <Column dataField={camp('CodSafT')} caption="Cod SAF-T" width={110} />

        <Column dataField={camp('DataPrimire')} caption="Data primirii" dataType="date" format="dd.MM.yyyy" width={115} />
        <Column dataField={camp('DataExigibilitate')} caption="Exigibilitate" dataType="date" format="dd.MM.yyyy" width={115} />
        <Column dataField={camp('DataInregistrare')} caption="Înregistrare" dataType="date" format="dd.MM.yyyy" width={115} />
        <Column dataField={camp('PerioadaD394')} caption="Perioada D394" width={115} />
        <Column dataField={camp('RegularizareD300')} caption="Regularizare D300" dataType="boolean" width={125} />
        <Column dataField={camp('InversaTehnica')} caption="Inversă tehnică" dataType="boolean" width={115} />
        <Column dataField={camp('Baza')} caption="Bază" {...BANI} />
        <Column dataField={camp('Tva')} caption="TVA" {...BANI} />
        <Column
          dataField={camp('Storno')}
          caption="Storno"
          dataType="boolean"
          width={80}
        />
        <Summary>
          <TotalItem column={camp('Baza')} summaryType="sum" valueFormat="#,##0.00" displayFormat="Σ {0}" />
          <TotalItem column={camp('Tva')} summaryType="sum" valueFormat="#,##0.00" displayFormat="Σ {0}" />
        </Summary>
      </DataGrid>

      <p className="indiciu">
        Rândurile păstrează separat documentul, tipul și calificarea fiscală, inclusiv cota istorică.
        Filtrul selectează <strong>perioada D300</strong>; perioada D394 și datele documentului,
        primirii, exigibilității și înregistrării sunt afișate distinct.
        Inversele au valori negative; corecțiile de evidență sunt marcate separat de facturile noi de corecție.
        Pe deconturi contrapartida e <strong>titularul</strong> (angajatul), nu comerciantul de pe bon: atât știe modelul.
        Liniile fără tip de TVA nu apar deloc — e o gaură a datelor, care se măsoară, nu se umple cu un regim presupus.
      </p>
    </div>
  );
}

export function JurnalCumparari() {
  return <JurnalTva sens="Achizitie" titlu="Jurnal de cumpărări" />;
}

export function JurnalVanzari() {
  return <JurnalTva sens="Livrare" titlu="Jurnal de vânzări" />;
}

function celulaDocument({ data }: { data: JurnalTvaRand }) {

  const ruta = data.DocumentId ? rutaTip(data.DocumentTip, data.DocumentId) : null;
  const text = data.DocumentNumar || data.DocumentTip || '(document)';
  return ruta ? <Link to={ruta}>{text}</Link> : <span>{text}</span>;
}

function celulaPartener({ data }: { data: JurnalTvaRand }) {
  if (!data.PartenerId) return <span className="indiciu">(fără contrapartidă)</span>;
  if (!data.PartenerDenumire)
    return <span className="indiciu" title={data.PartenerId}>(partener indisponibil)</span>;
  return <span>{data.PartenerDenumire}</span>;
}

function celulaTipTva({ data }: { data: JurnalTvaRand }) {
  if (data.TipTvaCod) return <span>{data.TipTvaCod}</span>;
  return <span className="indiciu" title={data.TipTvaId ?? ''}>(tip TVA indisponibil)</span>;
}

function celulaPerioada({ data }: { data: JurnalTvaRand }) {
  if (!data.PerioadaAn) return <span className="indiciu">—</span>;
  return <span>{String(data.PerioadaLuna).padStart(2, '0')}/{data.PerioadaAn}</span>;
}

function celulaRegim({ data }: { data: JurnalTvaRand }) {
  return <span>{labelEnum('RegimTva', data.Regim)}</span>;
}

const BANI = { dataType: 'number', format: '#,##0.00', alignment: 'right', width: 140 } as const;
