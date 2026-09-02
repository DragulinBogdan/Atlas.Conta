import { useEffect, useMemo, useState } from 'react';
import { Link, useNavigate } from 'react-router';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { SelectBox } from 'devextreme-react';
import { Column, DataGrid, FilterRow, HeaderFilter, Pager, Paging, Search, Sorting } from 'devextreme-react/data-grid';
import { itv, SCHEMA_LISTA, TIP_ANTET, type PrevizualizareItv } from './api';
import { campMeta, defaultProperty, labelEnum } from '../../nucleu/campMeta';
import { PanouErori } from '../../nucleu/PanouErori';
import { eroriDin, ia } from '../../nucleu/http';
import { useUrlStare } from '../../nucleu/urlStare';

// Ecranul de GENERARE al închiderii de TVA (F21-D8) — o consolă de comandă cu
// istoricul dedesubt, nu o listă cu buton „Nouă": închiderea nu se culege, se
// cere. De aceea bara de sus nu e un filtru al grilei, ci parametrii comenzii
// (an × lună × unitatea internă), iar sub ea stă PREVIZUALIZAREA lunii alese —
// dry-run-ul serverului, cu cifrele lui.
//
// Nimic nu se calculează aici (42c): soldurile, cele trei linii și verdictul
// „se poate / nu se poate" vin gata din `PrevizualizareItvDto`. Chiar și
// activarea butonului stă pe `Motiv == null`, nu pe o condiție reconstruită în
// TS din solduri.

const LUNI = [
  'ianuarie', 'februarie', 'martie', 'aprilie', 'mai', 'iunie',
  'iulie', 'august', 'septembrie', 'octombrie', 'noiembrie', 'decembrie',
];

// Serverul acceptă 2000..2100 (`ItvController`); lista oferă fereastra practică,
// ca selectorul să nu ceară derulat prin optzeci de ani. Marginea rămâne a
// serverului — un `?an=` din afara ferestrei ajunge tot la el.
const AN_MINIM = 2020;

const cap = (membru: string) => campMeta(TIP_ANTET, membru, SCHEMA_LISTA).caption;

type Unitate = { ID?: unknown; Cod?: string; Denumire?: string };

export function ItvLista() {
  const navigheaza = useNavigate();
  const cache = useQueryClient();
  const acum = new Date();

  // Luna e STARE DE URL (43c): „închiderea pe octombrie" trebuie să fie un link.
  // Unitatea NU intră în URL — e o alegere de operator, nu un parametru al
  // raportului, iar precompletarea de mai jos ar intra în conflict cu ea.
  const [stare, seteaza] = useUrlStare({
    an: String(acum.getFullYear()),
    luna: String(acum.getMonth() + 1),
  });
  const [unitateId, setUnitateId] = useState<string | null>(null);

  // Setul de unități interne, ÎNTREG: e nomenclator mic, iar ecranul are nevoie
  // și de NUMĂRUL lui — precompletarea din F21-D4 se declanșează doar la exact
  // un rând („un default care nu minte: cu două unități nu alege"). Un lookup
  // paginat ar fi răspuns la „ce se potrivește", nu la „câte sunt".
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
    queryKey: ['itv', 'previzualizare', stare.an, stare.luna],
    queryFn: () => itv.previzualizare(stare.an, stare.luna),
  });
  const prev = previzualizare.data;

  // Rezultatul comenzii poartă și CEREREA pentru care s-a produs (tiparul SAF-T):
  // altfel refuzul lunii august rămâne pe ecran după ce operatorul trece pe
  // septembrie — un mesaj despre ALTĂ lună, așezat peste cea de acum.
  const cerereCurenta = `${stare.an}-${stare.luna}`;
  const [rezultat, setRezultat] = useState<{ erori: string[]; mesaje: string[]; cerere: string }>(
    { erori: [], mesaje: [], cerere: '' });
  const [ocupat, setOcupat] = useState(false);
  const alRandului = rezultat.cerere === cerereCurenta;

  const sursa = useMemo(() => itv.storeLista(), []);

  async function genereaza() {
    if (!unitateId) return;
    const cerere = cerereCurenta;
    setOcupat(true);
    setRezultat({ erori: [], mesaje: [], cerere });
    try {
      const r = await itv.genereaza({ An: Number(stare.an), Luna: Number(stare.luna), UnitateId: unitateId });
      await cache.invalidateQueries({ queryKey: ['itv'] });
      // 200 fără document = RAPORT, nu eroare (F21-D7): luna s-a golit între
      // previzualizare și comandă, sau altcineva a închis-o deja.
      if (r.DocumentId) {
        navigheaza(`/itv/${r.DocumentId}`);
        return;
      }
      setRezultat({
        erori: [],
        mesaje: [`Nu s-a generat nicio închidere: ${motiv(r.Motiv)}`],
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
        <h2>Închideri de TVA</h2>
      </div>

      <div className="bara-raport">
        <label className="bara-raport__camp">
          <span className="camp__eticheta">An</span>
          {/* `<select>` nativ, ca la SAF-T: listă închisă, fără stare
              intermediară de reprezentat, deci fără o cerere per tastă. */}
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
          {/* Modelul n-are „unitatea internă a societății" (F21-D4), deci
              parametrul se CULEGE. Nu e `Lookup`: acela e legat de agregatul
              unui formular prin `useCamp`, iar aici nu există formular —
              aceeași despărțire ca la selectorul de cont al fișei. */}
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
              // mai sus scrie `value` programatic, iar widget-ul ridică
              // `onValueChanged` și atunci.
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

      {/* Refuzul CERERII de previzualizare (400 lună/an, 403 fără drept). */}
      <PanouErori
        erori={previzualizare.error ? eroriDin(previzualizare.error) : []}
        titlu="Luna nu se poate previzualiza"
      />
      <PanouErori erori={alRandului ? rezultat.erori : []} titlu="Refuzat de server" />
      <PanouErori erori={alRandului ? rezultat.mesaje : []} titlu="Rezultat" fel="succes" />

      {previzualizare.isPending
        ? <p className="indiciu">Se încarcă previzualizarea…</p>
        : prev && <Previzualizare prev={prev} />}

      <div className="itv__sectiune">
        <h3>Închiderile existente</h3>
        {/* Grila NU primește `height` cu `calc(100vh − …)`, spre deosebire de
            listele de documente: deasupra ei stă un bloc de înălțime VARIABILĂ
            (previzualizarea are patru forme, de la „fără sold" la trei linii cu
            link), iar o înălțime fixă ar lăsa fie un gol, fie o grilă tăiată. */}
        <DataGrid
          dataSource={sursa}
          remoteOperations
          showBorders
          columnAutoWidth
          onRowDblClick={(e) => navigheaza(`/itv/${(e.data as { Id: string }).Id}`)}
        >
          <Sorting mode="multiple" />
          <FilterRow visible />
          <HeaderFilter visible><Search enabled /></HeaderFilter>
          <Paging defaultPageSize={12} />
          <Pager showInfo showPageSizeSelector allowedPageSizes={[12, 25, 50]} />

          <Column dataField="Numar" caption={cap('Numar')} />
          {/* `Data` e ultima zi a lunii închise — de aceea ține loc și de „luna":
              ordinea implicită a listei (`Data` desc) e a serverului. */}
          <Column dataField="Data" caption={cap('Data')} dataType="date" format="dd.MM.yyyy" />
          <Column dataField="Stare" caption={cap('Stare')} />
          {/* Latura închiderii E unitatea internă, iar DTO-ul o numește așa:
              caption-ul entității („Predator (de la)") e corect și abstract,
              vocabularul feliei e „Unitatea". */}
          <Column dataField="UnitateDenumire" caption="Unitatea" />
          <Column dataField="Total" caption={cap('Total')} dataType="number" format="#,##0.00" alignment="right" />
        </DataGrid>
      </div>

      <p className="indiciu">
        Dublu-click pe un rând deschide închiderea. Închiderea se generează <strong>cronologic</strong>:
        cât timp o lună are o închidere vie (Draft sau Operat), lunile dinaintea ei sunt blocate. Storno la
        chiar data închiderii (ultima zi a lunii) lasă soldurile exact cum erau și redeschide luna.
      </p>
    </div>
  );
}

// Previzualizarea: verdictul serverului, cu cifrele lui. Cele patru forme sunt
// ale lui `MotivNegenerare` plus „se poate"; nimic nu se derivă din solduri.
function Previzualizare({ prev }: { prev: PrevizualizareItv }) {
  const sePoate = prev.Motiv == null;
  return (
    <div className="itv__sectiune">
      <h3>{`Luna ${prev.Luna}/${prev.An}`}</h3>

      {!sePoate && (
        <p className="panou panou--atentie">
          <strong>{motiv(prev.Motiv)}.</strong>
          {prev.InchidereVieId && (
            <>
              {' '}
              Închiderea care blochează:{' '}
              <Link to={`/itv/${prev.InchidereVieId}`}>
                {prev.InchidereVieNumar || '(draft fără număr)'}
              </Link>
              {prev.InchidereVieStare ? ` — ${labelEnum('StareDocument', prev.InchidereVieStare)}` : ''}
            </>
          )}
        </p>
      )}

      <table className="tabel-mic">
        <tbody>
          <tr>
            {/* Simbolurile vin din politică prin DTO (79 M6), nu din cod. */}
            <th>Sold {prev.SimbolDeductibila ?? ''} (TVA deductibilă)</th>
            <td className="num">{bani(prev.Sold4426)}</td>
            <th>Sold {prev.SimbolColectata ?? ''} (TVA colectată)</th>
            <td className="num">{bani(prev.Sold4427)}</td>
          </tr>
        </tbody>
      </table>

      <table className="tabel-mic" style={{ marginTop: 8 }}>
        <thead>
          <tr>
            <th>Linia care s-ar genera</th>
            <th>Valoare</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>Transfer ({prev.SimbolColectata ?? '…'} = {prev.SimbolDeductibila ?? '…'})</td>
            <td className="num">{bani(prev.Transfer)}</td>
          </tr>
          <tr>
            <td>TVA de plată ({prev.SimbolColectata ?? '…'} = {prev.SimbolDePlata ?? '…'})</td>
            <td className="num">{bani(prev.DePlata)}</td>
          </tr>
          <tr>
            <td>TVA de recuperat ({prev.SimbolDeRecuperat ?? '…'} = {prev.SimbolDeductibila ?? '…'})</td>
            <td className="num">{bani(prev.DeRecuperat)}</td>
          </tr>
        </tbody>
      </table>

      <p className="indiciu">
        {sePoate
          ? 'Cifrele sunt cele ale registrului contabil la ultima zi a lunii, calculate de motor — aceeași funcție care va genera liniile. Alegeți unitatea internă și apăsați „Generează”.'
          : 'Cifrele rămân afișate ca informație: sunt soldurile lunii, nu ce se va genera.'}
        {' '}Închiderea e a SOCIETĂȚII (soldurile întregului registru); unitatea internă e doar latura pe care
        o poartă nota, nu schimbă cifra (79 M1).
      </p>
    </div>
  );
}

// Motivul, tradus din enum-ul serverului (`MotivNegenerare`, pe sârmă ca numele
// membrului). `labelEnum` cade pe numele membrului dacă dump-ul n-ar avea
// eticheta — mai bine un nume tehnic decât un gol.
function motiv(valoare: string | null | undefined): string {
  if (!valoare) return 'fără motiv raportat';
  return labelEnum('MotivNegenerare', valoare) || valoare;
}

function bani(v: number | null | undefined): string {
  if (v == null) return '—';
  return v.toLocaleString('ro-RO', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function ani(anCurent: number): number[] {
  const lista: number[] = [];
  for (let a = anCurent + 1; a >= AN_MINIM; a--)
    lista.push(a);
  return lista;
}
