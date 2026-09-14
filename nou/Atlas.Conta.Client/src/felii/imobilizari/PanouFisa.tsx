import { Link } from 'react-router';
import { DateBox } from 'devextreme-react';
import { Column, DataGrid } from 'devextreme-react/data-grid';
import { useQuery } from '@tanstack/react-query';
import { PanouErori } from '../../nucleu/PanouErori';
import { labelEnum } from '../../nucleu/campMeta';
import { bani } from '../../nucleu/format';
import { eroriDin } from '../../nucleu/http';
import { rutaTip } from '../../nucleu/stingeri';
import { useUrlStare } from '../../nucleu/urlStare';
import { azi, izolataZi, ziLocala } from '../../nucleu/zi';
import { imobilizari, type RandImobilizareDto } from './api';

// Fișa unei imobilizări la o dată: antetul nomenclatorului, situația, parametrii
// ultimului eveniment și rândurile de registru. Toate cifrele vin din DTO —
// niciun total și nicio diferență nu se compune aici (42c).

const BANI = { dataType: 'number', format: '#,##0.00', alignment: 'right', width: 130 } as const;

export function PanouFisa({ id }: { id: string }) {
  const [stare, seteaza] = useUrlStare({ laData: azi() });

  const fisa = useQuery({
    queryKey: ['imobilizari', 'fisa', id, stare.laData],
    queryFn: () => imobilizari.fisa(id, stare.laData),
  });
  const f = fisa.data;
  const s = f?.Situatie;

  return (
    <section className="imo__sectiune">
      <h3>Fișa la o dată</h3>

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
        erori={fisa.error ? eroriDin(fisa.error) : []}
        titlu="Fișa nu s-a putut citi"
      />

      {f && (
        <>
          <table className="tabel-mic">
            <tbody>
              <tr>
                <th>Tip (cont/clasă)</th>
                <td>{etichetaPereche(f.TipMaterialCod, f.TipMaterialDenumire)}</td>
                <th>Clasificare</th>
                <td>
                  {etichetaPereche(f.ClasificareCod, f.ClasificareDenumire)}
                  {f.DurataFiscalaMinLuni != null && (
                    <> — durată fiscală admisă: {f.DurataFiscalaMinLuni}–{f.DurataFiscalaMaxLuni} luni</>
                  )}
                </td>
              </tr>
              <tr>
                <th>Loc</th>
                <td>{text(f.LocDenumire)}</td>
                <th>Centru de cost</th>
                <td>{text(f.CentruCostDenumire)}</td>
              </tr>
              <tr>
                <th>Cod economic</th>
                <td>{text(f.CodEconomicCod)}</td>
                <th>Responsabil</th>
                <td>{text(f.ResponsabilNume)}</td>
              </tr>
            </tbody>
          </table>

          <h4>Situația la {ziLocala(f.LaData)}</h4>
          <table className="tabel-mic">
            <tbody>
              <tr>
                <th>Valoare brută (contabil)</th>
                <td className="num">{bani(s?.Valoare)}</td>
                <th>Valoare brută (fiscal)</th>
                <td className="num">{bani(s?.ValoareFiscala)}</td>
              </tr>
              <tr>
                <th>Amortizare cumulată (contabil)</th>
                <td className="num">{bani(s?.Amortizare)}</td>
                <th>Amortizare cumulată (fiscal)</th>
                <td className="num">{bani(s?.AmortizareFiscala)}</td>
              </tr>
              <tr>
                <th>Amortizare deductibilă</th>
                <td className="num">{bani(s?.AmortizareDeductibila)}</td>
                <th>Luni amortizate</th>
                <td className="num">{s?.Luni ?? '—'}</td>
              </tr>
              <tr>
                <th>Net contabil</th>
                <td className="num">{bani(s?.NetContabil)}</td>
                <th>Net fiscal</th>
                <td className="num">{bani(s?.NetFiscal)}</td>
              </tr>
            </tbody>
          </table>

          <h4>Parametrii curenți</h4>
          <table className="tabel-mic">
            <tbody>
              <tr>
                <th>Metoda</th>
                <td>{text(labelEnum('MetodaAmortizare', s?.Metoda))}</td>
                <th>Durata (luni)</th>
                <td className="num">{s?.DurataLuni ?? '—'}</td>
              </tr>
              <tr>
                <th>Valoare reziduală</th>
                <td className="num">{bani(s?.ValoareReziduala)}</td>
                <th>Metoda fiscală</th>
                <td>{text(labelEnum('MetodaAmortizare', s?.MetodaFiscala))}</td>
              </tr>
              <tr>
                <th>Durata fiscală (luni)</th>
                <td className="num">{s?.DurataFiscalaLuni ?? '—'}</td>
                <th>Categorie fiscală</th>
                <td>{text(labelEnum('CategorieFiscala', s?.CategorieFiscala))}</td>
              </tr>
              <tr>
                <th>Utilizare exclusivă</th>
                <td>{daNu(s?.UtilizareExclusiva)}</td>
                <th>Data ultimului eveniment</th>
                <td>{text(ziLocala(s?.DataUltimEveniment))}</td>
              </tr>
            </tbody>
          </table>

          <h4>Rândurile registrului</h4>
          <DataGrid dataSource={f.Randuri ?? []} keyExpr="Id" showBorders columnAutoWidth>
            <Column dataField="Data" caption="Data" dataType="date" format="dd.MM.yyyy" width={100} />
            <Column
              dataField="Fel"
              caption="Fel"
              width={160}
              calculateCellValue={(r: RandImobilizareDto) => labelEnum('FelMiscareImobilizare', r.Fel)}
            />
            <Column dataField="Storno" caption="Storno" dataType="boolean" width={80} />
            <Column dataField="DocumentNumar" caption="Document" width={150} cellRender={celulaDocument} />
            <Column dataField="Valoare" caption="Valoare (brut contabil)" {...BANI} />
            <Column dataField="ValoareFiscala" caption="Valoare fiscală (brut fiscal)" {...BANI} />
            <Column dataField="Amortizare" caption="Amortizare" {...BANI} />
            <Column dataField="AmortizareFiscala" caption="Amortizare fiscală" {...BANI} />
            <Column dataField="AmortizareDeductibila" caption="Amortizare deductibilă" {...BANI} />
            <Column dataField="Luni" caption="Luni" dataType="number" alignment="right" width={70} />
            <Column
              dataField="Metoda"
              caption="Metoda"
              width={120}
              calculateCellValue={(r: RandImobilizareDto) => labelEnum('MetodaAmortizare', r.Metoda)}
            />
            <Column dataField="DurataLuni" caption="Durata (luni)" dataType="number" alignment="right" width={110} />
            <Column
              dataField="MetodaFiscala"
              caption="Metoda fiscală"
              width={130}
              calculateCellValue={(r: RandImobilizareDto) => labelEnum('MetodaAmortizare', r.MetodaFiscala)}
            />
            <Column
              dataField="DurataFiscalaLuni"
              caption="Durata fiscală (luni)"
              dataType="number"
              alignment="right"
              width={140}
            />
          </DataGrid>

          <p className="indiciu">
            Rândurile sunt cele cu data până la ziua aleasă. Starea și datele de punere în funcțiune /
            ieșire din antet sunt cele de AZI, de pe fișă — nu cele de la acea dată.
          </p>
        </>
      )}
    </section>
  );
}

// `DocumentTip` e vocabularul închis al rutării (PIF/CAS/AMO): un tip fără felie
// rămâne text, nu link mort.
function celulaDocument({ data }: { data: RandImobilizareDto }) {
  const eticheta = data.DocumentNumar || data.DocumentTip || '(document)';
  const ruta = data.DocumentId ? rutaTip(data.DocumentTip, data.DocumentId) : null;
  return ruta ? <Link to={ruta}>{eticheta}</Link> : <span>{eticheta}</span>;
}

function etichetaPereche(cod: string | null | undefined, denumire: string | null | undefined): string {
  if (cod && denumire) return `${cod} — ${denumire}`;
  return cod || denumire || '—';
}

function text(v: string | null | undefined): string {
  return v ? v : '—';
}

function daNu(v: boolean | null | undefined): string {
  return v == null ? '—' : v ? 'Da' : 'Nu';
}
