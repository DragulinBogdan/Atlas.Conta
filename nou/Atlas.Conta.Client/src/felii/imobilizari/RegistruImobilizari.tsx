import { useNavigate } from 'react-router';
import { DateBox } from 'devextreme-react';
import { Column, DataGrid, FilterRow, Sorting } from 'devextreme-react/data-grid';
import { useQuery } from '@tanstack/react-query';
import { PanouErori } from '../../nucleu/PanouErori';
import { labelEnum } from '../../nucleu/campMeta';
import { bani } from '../../nucleu/format';
import { eroriDin } from '../../nucleu/http';
import { urlCu, useUrlStare } from '../../nucleu/urlStare';
import { azi, izolataZi, ziLocala } from '../../nucleu/zi';
import { imobilizari, type RandRegistruImobilizariDto } from './api';

// Registrul imobilizărilor la o dată: o linie per fișă pusă în funcțiune.
// Totalurile sunt CÂMPURI ale DTO-ului, calculate pe server (42c) — de aceea
// stau într-un tabel sub grilă, nu într-un `Summary` de grilă.

const BANI = { dataType: 'number', format: '#,##0.00', alignment: 'right', width: 130 } as const;

export function RegistruImobilizari() {
  const navigheaza = useNavigate();
  const [stare, seteaza] = useUrlStare({ laData: azi() });

  const registru = useQuery({
    queryKey: ['imobilizari', 'registru', stare.laData],
    queryFn: () => imobilizari.registru(stare.laData),
  });
  const r = registru.data;

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>Registrul imobilizărilor la {ziLocala(stare.laData)}</h2>
      </div>

      <div className="bara-raport">
        <label className="bara-raport__camp">
          <span className="camp__eticheta">La data</span>
          <DateBox
            type="date"
            displayFormat="dd.MM.yyyy"
            value={stare.laData}
            width={160}
            onValueChanged={(e) => {
              if (!e.event) return;
              const zi = izolataZi(e.value);
              if (zi) seteaza({ laData: zi });
            }}
          />
        </label>
      </div>

      <PanouErori
        erori={registru.error ? eroriDin(registru.error) : []}
        titlu="Registrul nu s-a putut citi"
      />

      {r && (
        <>
          <DataGrid
            dataSource={r.Linii ?? []}
            keyExpr="ImobilizareId"
            showBorders
            columnAutoWidth
            onRowDblClick={(e) => {
              const rand = e.data as RandRegistruImobilizariDto;
              if (rand.ImobilizareId) {
                navigheaza(urlCu(`/imobilizari/${rand.ImobilizareId}`, { laData: stare.laData }));
              }
            }}
          >
            <Sorting mode="multiple" />
            <FilterRow visible />

            <Column dataField="NumarInventar" caption="Număr de inventar" width={150} />
            <Column dataField="Denumire" caption="Denumire" />
            <Column dataField="TipMaterialCod" caption="Tip (cont/clasă)" width={120} />
            <Column dataField="LocDenumire" caption="Loc" width={180} />
            <Column
              dataField="Stare"
              caption="Stare"
              width={130}
              calculateCellValue={(rand: RandRegistruImobilizariDto) => labelEnum('StareImobilizare', rand.Stare)}
            />
            <Column dataField="Brut" caption="Brut contabil" {...BANI} />
            <Column dataField="BrutFiscal" caption="Brut fiscal" {...BANI} />
            <Column dataField="AmortizareCumulata" caption="Amortizare cumulată" {...BANI} />
            <Column dataField="AmortizareFiscalaCumulata" caption="Amortizare fiscală cumulată" {...BANI} />
            <Column dataField="DeductibilCumulat" caption="Deductibil cumulat" {...BANI} />
            <Column dataField="NetContabil" caption="Net contabil" {...BANI} />
            <Column dataField="NetFiscal" caption="Net fiscal" {...BANI} />
            <Column dataField="Luni" caption="Luni" dataType="number" alignment="right" width={70} />
          </DataGrid>

          <table className="tabel-mic">
            <tbody>
              <tr>
                <th>Total brut contabil</th>
                <td className="num">{bani(r.TotalBrut)}</td>
                <th>Total brut fiscal</th>
                <td className="num">{bani(r.TotalBrutFiscal)}</td>
              </tr>
              <tr>
                <th>Total amortizare cumulată</th>
                <td className="num">{bani(r.TotalAmortizareCumulata)}</td>
                <th>Total amortizare fiscală cumulată</th>
                <td className="num">{bani(r.TotalAmortizareFiscalaCumulata)}</td>
              </tr>
              <tr>
                <th>Total deductibil cumulat</th>
                <td className="num">{bani(r.TotalDeductibilCumulat)}</td>
                <th />
                <td />
              </tr>
              <tr>
                <th>Total net contabil</th>
                <td className="num">{bani(r.TotalNetContabil)}</td>
                <th>Total net fiscal</th>
                <td className="num">{bani(r.TotalNetFiscal)}</td>
              </tr>
            </tbody>
          </table>

          <p className="indiciu">
            Registrul listează fișele puse în funcțiune. Totalurile sunt ale serverului, pe TOATE
            rândurile de la acea dată — filtrarea grilei nu le schimbă. Dublu-click deschide fișa,
            cu aceeași dată.
          </p>
        </>
      )}
    </div>
  );
}
