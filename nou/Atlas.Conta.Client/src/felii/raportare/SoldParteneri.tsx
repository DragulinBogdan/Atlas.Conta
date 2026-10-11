import { useMemo } from 'react';
import { useNavigate } from 'react-router';
import { DateBox } from 'devextreme-react';
import {
  Column, ColumnFixing, DataGrid, FilterRow, HeaderFilter, Pager, Paging, Search, Sorting,
} from 'devextreme-react/data-grid';
import type { components } from '../../generated/api-types';
import { storeRemote } from '../../nucleu/dxStore';
import { urlCu, useDimensiuniUrl, useUrlStare } from '../../nucleu/urlStare';
import { azi, izolataZi } from '../../nucleu/zi';

// Soldurile pe cont și terț la o zi (F27-D7, C111-Q13). Cifrele sunt ale
// serverului (42c); netarea nu e aditivă (66d) și se face pe cheie, pe server.
type SoldPartenerRand = components['schemas']['SoldPartenerRand'];
const camp = (n: keyof SoldPartenerRand & string) => n;

export function SoldParteneri() {
  const navigheaza = useNavigate();
  // Data trăiește în URL, ca perioada rapoartelor (43c): „soldurile la 31.12"
  // trebuie să fie un link, nu o stare de sesiune.
  const [stare, seteaza] = useUrlStare({ laData: azi() });
  const dimensiuni = useDimensiuniUrl();

  const sursa = useMemo(() => storeRemote(
    urlCu('/api/proiectii/sold-parteneri', { ...stare, ...dimensiuni }),
    ['ContId', 'RepartitorId'],
  ), [stare, dimensiuni]);

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>Solduri pe cont și terț</h2>
      </div>

      <div className="bara-raport">
        <label className="bara-raport__camp">
          <span className="camp__eticheta">La data</span>
          <DateBox
            value={stare.laData}
            displayFormat="dd.MM.yyyy"
            width={160}
            onValueChanged={(e) => {
              if (!e.event) return;
              seteaza({ laData: izolataZi(e.value) ?? azi() });
            }}
          />
        </label>
      </div>

      <DataGrid
        dataSource={sursa}
        remoteOperations={{ filtering: true, sorting: true, paging: true }}
        showBorders
        columnAutoWidth
        height="calc(100vh - 220px)"
        onRowDblClick={(e) => {
          const rand = e.data as SoldPartenerRand | undefined;
          if (!rand?.ContId) return;
          // Drill-down în fișa contului, pe anul care se termină la data cerută:
          // rândul de aici e un SOLD cumulat, iar fișa îi arată mișcările.
          const an = String(stare.laData ?? azi()).slice(0, 4);
          navigheaza(urlCu('/fisa-cont', {
            contId: rand.ContId,
            dataStart: `${an}-01-01`,
            dataEnd: stare.laData,
            repartitorId: rand.RepartitorId ?? undefined,
            faraTert: rand.RepartitorId ? undefined : true,
          }));
        }}
      >
        <Sorting mode="multiple" />
        <FilterRow visible />
        <HeaderFilter visible><Search enabled /></HeaderFilter>
        <ColumnFixing enabled />
        <Paging defaultPageSize={50} />
        <Pager showInfo showPageSizeSelector allowedPageSizes={[50, 100, 200]} />

        <Column dataField={camp('ContSimbol')} caption="Cont" fixed width={110} cellRender={celulaCont} />
        <Column dataField={camp('ContDenumire')} caption="Denumire cont" />
        <Column
          dataField={camp('RepartitorDenumire')}
          caption="Partener / angajat"
          cellRender={celulaRepartitor}
        />
        <Column dataField={camp('SoldDebitor')} caption="Sold debitor" {...BANI} />
        <Column dataField={camp('SoldCreditor')} caption="Sold creditor" {...BANI} />
        <Column dataField={camp('Debit')} caption="Cumulat debit" visible={false} {...BANI} />
        <Column dataField={camp('Credit')} caption="Cumulat credit" visible={false} {...BANI} />
      </DataGrid>

      <p className="indiciu">
        Rând propriu au partenerii și angajații. Restul contului (fără repartitor, pe gestiune, pe unitate
        internă sau pe cont propriu) stă în rândul „fără terț”, deci suma rândurilor unui cont e soldul lui
        din balanță. Rândurile cu sold zero nu apar. Dublu-click deschide fișa contului pe anul datei cerute,
        filtrată pe rând. Soldurile nu se însumează pe grup: netarea nu e aditivă.
      </p>
    </div>
  );
}

function celulaCont({ data }: { data: SoldPartenerRand }) {
  if (data.ContSimbol) return <span>{data.ContSimbol}</span>;
  return <span className="indiciu" title={data.ContId ?? ''}>(cont indisponibil)</span>;
}

function celulaRepartitor({ data }: { data: SoldPartenerRand }) {
  return data.RepartitorDenumire
    ? <span>{data.RepartitorDenumire}</span>
    : <span className="indiciu">(fără terț)</span>;
}

const BANI = { dataType: 'number', format: '#,##0.00', alignment: 'right', width: 140 } as const;
