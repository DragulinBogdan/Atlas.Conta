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

// Registrul-jurnal (R-D9): listarea cronologică a notelor așa cum au fost
// scrise — rândurile BRUTE ale registrului, nu atomii unpivotați (o notă apare
// o dată, cu ambele conturi pe ea).
//
// FĂRĂ drill-down spre fișă, deliberat: rândul are DOUĂ conturi, deci „fișa
// cărui cont?" n-are răspuns. `ContDebitId`/`ContCreditId` există în DTO pentru
// filtrare și pentru ce va veni, nu ca navigare inventată aici.
type JurnalRand = components['schemas']['JurnalRand'];
const camp = (n: keyof JurnalRand & string) => n;

export function RegistruJurnal() {
  // Perioada e OPȚIONALĂ aici — o listare n-are noțiune de sold inițial, deci
  // nici graniță dinăuntrul unei agregări; serverul o tratează ca filtru simplu.
  // De aceea implicitul e `''` (fără filtru), nu luna curentă: cu un implicit
  // calculat, caseta n-ar mai putea fi GOLITĂ — golirea ar cădea înapoi pe el.
  const [stare, seteaza] = useUrlStare({ dataStart: '', dataEnd: '' });

  const sursa = useMemo(() => storeRemote(urlCu('/api/proiectii/registru-jurnal', stare), 'Id'), [stare]);

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
        // Sortarea e PERMISĂ aici (spre deosebire de fișă): jurnalul n-are sold
        // curent de rupt — e o listare, iar ordinea implicită e cea a scrierii.
        // `grouping: true` nu e pentru un panou de grupare (nu există aici), ci
        // fiindcă `HeaderFilter` își cere lista de valori prin protocolul de
        // grupare: fără el, grila ar încerca să materializeze tot registrul
        // (305k rânduri) în browser ca să compună panoul. Jurnalul, spre
        // deosebire de fișă, nu refuză gruparea pe server.
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
        <Column dataField={camp('ContDebitSimbol')} caption="Cont debitor" width={120} />
        <Column dataField={camp('ContCreditSimbol')} caption="Cont creditor" width={120} />
        <Column dataField={camp('Valoare')} caption="Valoare" dataType="number" format="#,##0.00" alignment="right" width={140} />
        <Column
          dataField={camp('DocumentNumar')}
          caption="Document"
          width={150}
          // Ca la fișă: `DocumentTip` se completează peste pagina materializată
          // (R-D8), deci nu e filtrabil; rutarea prin `rutaTip`.
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
