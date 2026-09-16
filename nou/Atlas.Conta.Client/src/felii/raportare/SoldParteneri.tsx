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

// Soldurile pe (cont × repartitor) la o zi (F27-D7): partea de SOLD a balanței
// analitice, fără noțiunea de perioadă.
//
// ATENȚIE la ce înseamnă „repartitor" aici: dimensiunea urmează LATURILE
// documentului (debit←predator, credit←primitor — 00 §5), nu contul de terț.
// Pe o factură de client, atomul de DEBIT al lui 4111 poartă emitentul, iar
// clientul apare pe atomul de CREDIT al venitului (măsurat pe baza reală:
// 103.301 din 108.912 rânduri de 4111 au „Sediul central" pe debit). Creanța
// PER PARTENER se citește deci din partidele deschise (`documente-cu-rest`),
// nu de aici; ecranul acesta e soldul pe cheia contabilă, așa cum e ea.
//
// Cifrele sunt ale serverului (42c): aceiași atomi cumulați ca balanța, deci
// rândurile coincid la cent cu modul analitic al ei. TS-ul nu netează nimic —
// netarea nu e aditivă (66d) și se face la nivelul cheii, pe server.
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
        <h2>Solduri pe cont și repartitor</h2>
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
            // „Fără repartitor" e un rând legitim (LEFT JOIN): santinelă
            // explicită, altfel fișa s-ar deschide pe TOT contul.
            repartitorNul: rand.RepartitorId ? undefined : true,
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
          caption="Partener / repartitor"
          cellRender={celulaRepartitor}
        />
        <Column dataField={camp('SoldDebitor')} caption="Sold debitor" {...BANI} />
        <Column dataField={camp('SoldCreditor')} caption="Sold creditor" {...BANI} />
        <Column dataField={camp('Debit')} caption="Cumulat debit" visible={false} {...BANI} />
        <Column dataField={camp('Credit')} caption="Cumulat credit" visible={false} {...BANI} />
      </DataGrid>

      <p className="indiciu">
        Rândurile cu sold zero nu apar. Dublu-click deschide fișa contului pe anul datei cerute.
        Soldurile nu se însumează pe grup — netarea nu e aditivă. Repartitorul e dimensiunea laturii
        documentului, nu partenerul contului de terț: restul pe partener se citește din documentele cu rest.
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
    : <span className="indiciu">(fără repartitor)</span>;
}

const BANI = { dataType: 'number', format: '#,##0.00', alignment: 'right', width: 140 } as const;
