import { useEffect, useMemo, useState } from 'react';
import { Link, useNavigate } from 'react-router';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { SelectBox } from 'devextreme-react';
import { Column, DataGrid } from 'devextreme-react/data-grid';
import { amo, SCHEMA_LISTA, TIP_ANTET, type AmoListRand, type PrevizualizareAmo } from './api';
import { campMeta, defaultProperty, labelEnum } from '../../nucleu/campMeta';
import { PanouErori } from '../../nucleu/PanouErori';
import { bani } from '../../nucleu/format';
import { eroriDin, ia } from '../../nucleu/http';
import { useUrlStare } from '../../nucleu/urlStare';
import { GrilaDocumente, INDICIU_GRILA } from '../../nucleu/GrilaDocumente';

// Consola de generare pe tiparul `ItvLista`; cifrele și verdictul vin din previzualizarea serverului (F26-D7, 42c).

const LUNI = [
  'ianuarie', 'februarie', 'martie', 'aprilie', 'mai', 'iunie',
  'iulie', 'august', 'septembrie', 'octombrie', 'noiembrie', 'decembrie',
];

// Serverul acceptă 2000..2100 (`AmoController`); lista oferă fereastra practică.
const AN_MINIM = 2020;

const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

type Unitate = { ID?: unknown; Cod?: string; Denumire?: string };

export function AmoLista() {
  const navigheaza = useNavigate();
  const cache = useQueryClient();
  const acum = new Date();

  const [stare, seteaza] = useUrlStare({
    an: String(acum.getFullYear()),
    luna: String(acum.getMonth() + 1),
  });
  const [unitateId, setUnitateId] = useState<string | null>(null);

  const unitati = useQuery({
    queryKey: ['nomenclator-set', 'UnitateInterna'],
    queryFn: () => ia<{ value?: Unitate[] }>('/api/odata/UnitateInterna?$orderby=Denumire'),
    staleTime: Infinity,
  });
  const listaUnitati = useMemo(() => unitati.data?.value ?? [], [unitati.data]);
  const afisareUnitate = defaultProperty('UnitateInterna');

  useEffect(() => {
    if (listaUnitati.length === 1)
      setUnitateId((precedent) => precedent ?? String(listaUnitati[0].ID));
  }, [listaUnitati]);

  const previzualizare = useQuery({
    queryKey: ['amo', 'previzualizare', stare.an, stare.luna],
    queryFn: () => amo.previzualizare(stare.an, stare.luna),
  });
  const prev = previzualizare.data;

  // Rezultatul comenzii poartă și CEREREA care l-a produs: altfel refuzul unei
  // luni rămâne pe ecran după ce operatorul trece pe alta.
  const cerereCurenta = `${stare.an}-${stare.luna}`;
  const [rezultat, setRezultat] = useState<{ erori: string[]; mesaje: string[]; cerere: string }>(
    { erori: [], mesaje: [], cerere: '' });
  const [ocupat, setOcupat] = useState(false);
  const alRandului = rezultat.cerere === cerereCurenta;

  const sursa = useMemo(() => amo.storeLista(), []);

  async function genereaza() {
    if (!unitateId) return;
    const cerere = cerereCurenta;
    setOcupat(true);
    setRezultat({ erori: [], mesaje: [], cerere });
    try {
      const r = await amo.genereaza({ An: Number(stare.an), Luna: Number(stare.luna), UnitateId: unitateId });
      await cache.invalidateQueries({ queryKey: ['amo'] });
      // 200 fără document = RAPORT, nu eroare: luna s-a ocupat între
      // previzualizare și comandă, sau n-a mai rămas nicio fișă eligibilă.
      if (r.DocumentId) {
        navigheaza(`/amo/${r.DocumentId}`);
        return;
      }
      setRezultat({
        erori: [],
        mesaje: [`Nu s-a generat nicio amortizare: ${motiv(r.MotivEticheta, r.Motiv)}${r.Detaliu ? ` (${r.Detaliu})` : ''}`],
        cerere,
      });
    }
    catch (e) {
      setRezultat({ erori: eroriDin(e), mesaje: [], cerere });
    }
    finally {
      setOcupat(false);
    }
  }

  const sePoate = prev != null && prev.Motiv == null;

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>Amortizare lunară</h2>
      </div>

      <p className="indiciu">
        Luna evenimentului nu se amortizează, iar parametrii noi (revizuire, modernizare) curg din luna
        URMĂTOARE, indiferent de zi. Cronologia e strictă: o lună se amortizează doar după ce precedenta
        e operată.
      </p>

      <div className="bara-raport">
        <label className="bara-raport__camp">
          <span className="camp__eticheta">An</span>
          {/* `<select>` nativ, ca la ITV: listă închisă, fără stare intermediară. */}
          <select value={stare.an} onChange={(e) => seteaza({ an: e.target.value })}>
            {ani(acum.getFullYear()).map((a) => <option key={a} value={String(a)}>{a}</option>)}
          </select>
        </label>
        <label className="bara-raport__camp">
          <span className="camp__eticheta">Luna</span>
          <select value={stare.luna} onChange={(e) => seteaza({ luna: e.target.value })}>
            {LUNI.map((nume, i) => (
              <option key={nume} value={String(i + 1)}>{`${i + 1} — ${nume}`}</option>
            ))}
          </select>
        </label>
        <label className="bara-raport__camp bara-raport__camp--lat">
          <span className="camp__eticheta">Unitatea internă</span>
          <SelectBox
            dataSource={listaUnitati}
            value={unitateId}
            valueExpr="ID"
            displayExpr={afisareUnitate}
            searchEnabled
            searchExpr={afisareUnitate}
            noDataText="Nicio unitate internă"
            width={280}
            placeholder={listaUnitati.length > 1 ? 'Alegeți unitatea' : ''}
            onValueChanged={(e) => {
              // Doar acțiunile omului schimbă starea (56e): precompletarea de
              // mai sus scrie `value` programatic.
              if (!e.event) return;
              setUnitateId((e.value as string) ?? null);
            }}
          />
        </label>
        <button
          type="button"
          className="buton buton--primar"
          disabled={!sePoate || !unitateId || ocupat}
          onClick={() => void genereaza()}
        >
          {ocupat ? 'Se generează…' : 'Generează'}
        </button>
      </div>

      <PanouErori
        erori={previzualizare.error ? eroriDin(previzualizare.error) : []}
        titlu="Luna nu se poate previzualiza"
      />
      <PanouErori erori={alRandului ? rezultat.erori : []} titlu="Refuzat de server" />
      <PanouErori erori={alRandului ? rezultat.mesaje : []} titlu="Rezultat" fel="succes" />

      {previzualizare.isPending
        ? <p className="indiciu">Se încarcă previzualizarea…</p>
        : prev && <Previzualizare prev={prev} />}

      <div className="imo__sectiune">
        <h3>Amortizările existente</h3>
        {/* Fără `height` fix: blocul de deasupra are înălțime VARIABILĂ. */}
        <GrilaDocumente sursa={sursa} ruta="/amo" pagini={[12, 25, 50]}>
          <Column dataField="Numar" caption={cap('Numar')} />
          <Column dataField="Data" caption={cap('Data')} dataType="date" format="dd.MM.yyyy" />
          <Column
            dataField="Luna"
            caption="Luna"
            width={110}
            calculateCellValue={(r: AmoListRand) => `${r.Luna ?? ''}/${r.An ?? ''}`}
          />
          <Column
            dataField="Stare"
            caption={cap('Stare')}
            width={110}
            calculateCellValue={(r: AmoListRand) => labelEnum('StareDocument', r.Stare)}
          />
          <Column dataField="UnitateDenumire" caption="Unitatea" />
          <Column dataField="NrLinii" caption="Linii" dataType="number" width={90} alignment="right" />
          <Column dataField="TotalContabil" caption="Amortizare contabilă" dataType="number" format="#,##0.00" alignment="right" />
          <Column dataField="TotalFiscal" caption="Amortizare fiscală" dataType="number" format="#,##0.00" alignment="right" />
          <Column dataField="TotalDeductibil" caption="Amortizare deductibilă" dataType="number" format="#,##0.00" alignment="right" />
        </GrilaDocumente>
      </div>

      <p className="indiciu">
        {INDICIU_GRILA} Stornarea la chiar data amortizării (ultima zi a
        lunii) redeschide luna.
      </p>
    </div>
  );
}

// Previzualizarea: verdictul serverului, cu cifrele lui.
function Previzualizare({ prev }: { prev: PrevizualizareAmo }) {
  const sePoate = prev.Motiv == null;
  return (
    <div className="imo__sectiune">
      <h3>{`Luna ${prev.Luna}/${prev.An}`}</h3>

      {!sePoate && (
        <p className="panou panou--atentie">
          <strong>{motiv(prev.MotivEticheta, prev.Motiv)}.</strong>
          {prev.BlocantId && (
            <>
              {' '}Documentul care blochează:{' '}
              <Link to={`/amo/${prev.BlocantId}`}>
                {prev.BlocantNumar || '(draft fără număr)'}
              </Link>
              {prev.BlocantStare ? ` — ${labelEnum('StareDocument', prev.BlocantStare)}` : ''}
            </>
          )}
          {prev.Detaliu && <> {' '}Fișa: {prev.Detaliu}.</>}
        </p>
      )}

      <DataGrid dataSource={prev.Linii ?? []} keyExpr="ImobilizareId" showBorders columnAutoWidth>
        <Column dataField="NumarInventar" caption="Număr de inventar" />
        <Column dataField="Denumire" caption="Denumire" />
        <Column dataField="Contabil" caption="Amortizare contabilă" dataType="number" format="#,##0.00" alignment="right" />
        <Column dataField="Fiscal" caption="Amortizare fiscală" dataType="number" format="#,##0.00" alignment="right" />
        <Column dataField="Deductibil" caption="Amortizare deductibilă" dataType="number" format="#,##0.00" alignment="right" />
        <Column dataField="ContCheltuialaSimbol" caption="Cont de cheltuială" width={140} />
        <Column dataField="ContAmortizareSimbol" caption="Cont de amortizare" width={140} />
        <Column dataField="LocDenumire" caption="Loc" />
      </DataGrid>

      <table className="tabel-mic" style={{ marginTop: 8 }}>
        <tbody>
          <tr>
            <th>Total contabil</th>
            <td className="num">{bani(prev.TotalContabil)}</td>
            <th>Total fiscal</th>
            <td className="num">{bani(prev.TotalFiscal)}</td>
            <th>Total deductibil</th>
            <td className="num">{bani(prev.TotalDeductibil)}</td>
          </tr>
        </tbody>
      </table>

      <p className="indiciu">
        {sePoate
          ? 'Cifrele sunt cele ale registrului de imobilizări la sfârșitul lunii, calculate de motor — aceeași funcție care va genera liniile. Alegeți unitatea internă și apăsați „Generează”.'
          : 'Cifrele rămân afișate ca informație: sunt ce s-ar amortiza, nu ce se va genera.'}
        {' '}O linie cu amortizare contabilă zero și fiscală pozitivă rămâne fără conturi: ea nu postează,
        dar intră în registru.
      </p>
    </div>
  );
}

// Eticheta motivului vine de pe SERVER (`MotivEticheta`); `labelEnum` e plasa,
// iar numele membrului e ultima cădere — mai bine un nume tehnic decât un gol.
function motiv(eticheta: string | null | undefined, valoare: string | null | undefined): string {
  if (eticheta) return eticheta;
  if (!valoare) return 'fără motiv raportat';
  return labelEnum('MotivNegenerare', valoare) || valoare;
}

function ani(anCurent: number): number[] {
  const lista: number[] = [];
  for (let a = anCurent + 1; a >= AN_MINIM; a--)
    lista.push(a);
  return lista;
}
