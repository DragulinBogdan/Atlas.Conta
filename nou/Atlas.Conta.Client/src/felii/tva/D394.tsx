import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Column, DataGrid, FilterRow, Grouping, GroupPanel, HeaderFilter, Paging, Scrolling, Sorting,
} from 'devextreme-react/data-grid';
import type { components } from '../../generated/api-types';
import { labelEnum } from '../../nucleu/campMeta';
import { eroriDin, ia } from '../../nucleu/http';
import { PanouErori } from '../../nucleu/PanouErori';
import { urlCu, useUrlStare } from '../../nucleu/urlStare';
import { CasetaPerioada, lunaCurenta } from '../raportare/comune';

// D394 — declarația informativă (felia 14, D4-D7): aceleași cifre ca în D300,
// așezate PER PARTENER (OPANAF 3769/2015, mod. 2194/2025). Ecran separat de
// `/d300` din același motiv pentru care D300 e separat de `/decont-tva`: alt
// formular, altă așezare a aceluiași registru.
//
// Corpul are patru piese, în ordinea în care se citește formularul: (1) lista
// `op1` grupată pe cartușe (C/D/E/F), sortabilă și filtrabilă LOCAL — răspunsul
// e întreg (fără `DataSourceLoader`), deci grila lucrează pe el fără să mai
// întrebe serverul; (2) rezumatele C–F per cotă și cartușul H (suma de
// control); (3) panoul „Neincluse în declarație" — nimic nu se pierde (D4-D4);
// (4) avertismentele (D4-D5) — ce cere formularul și modelul nu are.
//
// Ce NU e: fișierul XML (35c). Cifrele sunt bani exacți; rotunjirea la leu e a
// fișierului.
type D394Dto = components['schemas']['D394Dto'];
type Operatiune = components['schemas']['D394Operatiune'];
type Rezumat = components['schemas']['D394Rezumat'];
type RezumatCota = components['schemas']['D394RezumatCota'];
type Neinclus = components['schemas']['D394Neinclus'];
type Avertisment = components['schemas']['D394Avertisment'];

const camp = (n: keyof Operatiune & string) => n;
const campNeinclus = (n: keyof Neinclus & string) => n;

// Cartușele formularului, pe `tip_partener` (structura §4.2, verbatim din anexa
// de validări). Definiția e a legii, nu variază per client — deci constantă în
// cod, ca lista UE de pe server.
const CARTUSE: Record<number, { litera: string; denumire: string }> = {
  1: { litera: 'C', denumire: 'persoane impozabile înregistrate în scopuri de TVA în România' },
  2: { litera: 'D', denumire: 'persoane neînregistrate în scopuri de TVA (inclusiv persoane fizice)' },
  3: { litera: 'E', denumire: 'persoane nestabilite în România, stabilite în alt stat membru UE' },
  4: { litera: 'F', denumire: 'persoane nestabilite în România și nestabilite pe teritoriul UE' },
};

function cartus(tip: number): string {
  const c = CARTUSE[tip];
  return c ? `${c.litera} — ${c.denumire}` : `Tip partener ${tip}`;
}

export function D394() {
  const luna = lunaCurenta();
  // Perioada e OBLIGATORIE pe server (400 fără ea): perioada E declarația.
  // Implicitul lunii curente e ca prima deschidere să arate ceva; se vede în
  // bară și în URL.
  const [stare, seteaza] = useUrlStare({ dataStart: luna.start, dataEnd: luna.sfarsit });

  const cale = urlCu('/api/proiectii/d394', { dataStart: stare.dataStart, dataEnd: stare.dataEnd });
  // Aceeași cale de citire ca D300: `useQuery` + `ia` (JWT, 401 → sesiune
  // expirată, 400 `EroriDto` → mesaje de domeniu, afișate).
  const citit = useQuery({ queryKey: ['d394', cale], queryFn: () => ia<D394Dto>(cale) });

  const operatiuni = useMemo(() => citit.data?.Operatiuni ?? [], [citit.data]);
  const rezumat = useMemo(() => citit.data?.Rezumat ?? [], [citit.data]);
  const rezumatCote = useMemo(() => citit.data?.RezumatCote ?? [], [citit.data]);
  const neincluse = useMemo(() => citit.data?.Neincluse ?? [], [citit.data]);
  const avertismente: Avertisment[] = citit.data?.Avertismente ?? [];
  // Cheia de remontare: parametrii cererii (gruparea nu se reevaluează pe o
  // instanță vie — lecția `BalantaPlan`).
  const cheie = `${stare.dataStart}|${stare.dataEnd}`;

  return (
    <div className="ecran">
      <div className="ecran__bara"><h2>D394 — declarație informativă</h2></div>

      <div className="bara-raport">
        <CasetaPerioada dataStart={stare.dataStart} dataEnd={stare.dataEnd} seteaza={seteaza} />
        {citit.data && (
          <span className="indiciu">
            Parteneri: C {citit.data.NrCui1 ?? 0} · D {citit.data.NrCui2 ?? 0} · E {citit.data.NrCui3 ?? 0} · F {citit.data.NrCui4 ?? 0}
          </span>
        )}
      </div>

      <PanouErori erori={citit.error ? eroriDin(citit.error) : []} titlu="Cererea a fost refuzată" />
      <Avertismente lista={avertismente} />

      {citit.isPending ? <p className="indiciu">Se încarcă…</p> : (
        <>
          <DataGrid
            key={cheie}
            className="d394"
            dataSource={operatiuni}
            showBorders
            columnAutoWidth
            height="calc(100vh - 320px)"
            onRowPrepared={(e) => {
              if (e.rowType !== 'data') return;
              const r = e.data as Operatiune;
              // TVA pe un tip FĂRĂ coloană (V pe date pre-F13): cifra e în
              // avertisment ȘI aici, pe rând — nu doar într-o listă de sus.
              if ((r.TvaNedeclarat ?? 0) !== 0) e.rowElement.classList.add('d394__nedeclarat');
            }}
          >
            {/* Local, nu remote: răspunsul e ÎNTREG. Sortarea și filtrarea sunt
                ale cititorului (un partener anume, o cotă), nu ale formularului
                — ordinea oficială o dau rezumatele și XML-ul, nu lista. */}
            <Sorting mode="multiple" />
            <FilterRow visible />
            <HeaderFilter visible />
            <Paging enabled={false} />
            <Scrolling mode="virtual" />
            <GroupPanel visible={false} />
            <Grouping autoExpandAll />

            <Column
              dataField={camp('TipPartener')}
              caption="Cartuș"
              groupIndex={0}
              sortOrder="asc"
              groupCellRender={({ value }: { value: number }) => <span>{cartus(value)}</span>}
            />
            <Column dataField={camp('CuiP')} caption="CUI" width={130} cellRender={celulaCui} />
            <Column dataField={camp('Denumire')} caption="Denumire" minWidth={240} />
            <Column
              dataField={camp('Tip')}
              caption="Tip"
              width={110}
              cellRender={({ data }: { data: Operatiune }) => (
                <span title={labelEnum('TipOperatiuneD394', data.Tip)}>{data.Tip}</span>
              )}
            />
            <Column dataField={camp('Cota')} caption="Cotă %" dataType="number" alignment="right" width={80} />
            <Column dataField={camp('NrFact')} caption="Facturi" dataType="number" format="#,##0" alignment="right" width={90} />
            <Column dataField={camp('Baza')} caption="Bază" {...BANI} />
            {/* `Tva` e `null` unde tipul N-ARE coloană (V/LS/AS/N): celula rămâne
                GOALĂ — „0,00" într-o casetă inexistentă e o cifră inventată. */}
            <Column dataField={camp('Tva')} caption="TVA" {...BANI} />
            <Column
              dataField={camp('TvaNedeclarat')}
              caption="TVA nedeclarat"
              {...BANI}
              width={130}
              cellRender={({ value }: { value: number }) => (
                value ? <strong>{bani(value)}</strong> : <span />
              )}
            />
            <Column dataField={camp('Documente')} caption="Documente" dataType="number" format="#,##0" alignment="right" width={100} />
            <Column dataField={camp('Randuri')} caption="Rânduri" dataType="number" format="#,##0" alignment="right" width={90} />
          </DataGrid>

          <Rezumate rezumat={rezumat} />
          <CartusH cote={rezumatCote} />
        </>
      )}

      {/* D4-D4: nimic nu se pierde. Panoul apare DOAR când există grupuri care
          n-au unde cădea în declarație — dar atunci arată cifrele lor. */}
      {neincluse.length > 0 && (
        <div className="d300__neincluse">
          <h3>Neincluse în declarație</h3>
          <p className="indiciu">
            Rânduri de registru pe perioada aleasă care nu au unde să cadă în declarație — deci
            cifrele de mai jos <strong>nu</strong> se regăsesc în listele de sus. Cauze legitime: un
            tip de TVA scutit/neimpozabil (nemapat deliberat), un tip de TVA propriu încă nemapat
            (politica <em>MapareD394</em> din BackOffice), un rând fără partener (contrapartidă
            explicită) sau o contrapartidă care nu e partener (angajatul de pe decont).
          </p>
          <DataGrid
            key={`neincluse|${cheie}`}
            dataSource={neincluse}
            showBorders
            columnAutoWidth
            height={Math.min(60 + neincluse.length * 34, 320)}
          >
            <Sorting mode="none" />
            <Paging enabled={false} />
            <Column dataField={campNeinclus('Cauza')} caption="Cauză" cellRender={({ data }: { data: Neinclus }) => <span>{labelEnum('CauzaNeincludere', data.Cauza)}</span>} />
            <Column dataField={campNeinclus('Sens')} caption="Sens" width={110} cellRender={({ data }: { data: Neinclus }) => <span>{labelEnum('SensTva', data.Sens)}</span>} />
            <Column dataField={campNeinclus('TipTvaCod')} caption="Tip TVA" width={90} cellRender={celulaTipTva} />
            <Column dataField={campNeinclus('TipTvaDenumire')} caption="Denumire" />
            <Column dataField={campNeinclus('Cota')} caption="Cotă %" dataType="number" format="#0.##" alignment="right" width={80} />
            <Column dataField={campNeinclus('RepartitorDenumire')} caption="Repartitor" />
            <Column dataField={campNeinclus('Baza')} caption="Bază" {...BANI} />
            <Column dataField={campNeinclus('Tva')} caption="TVA" {...BANI} />
            <Column dataField={campNeinclus('Randuri')} caption="Rânduri" dataType="number" format="#,##0" alignment="right" width={90} />
          </DataGrid>
        </div>
      )}

      <p className="indiciu">
        Cifrele sunt cele din <strong>registrul de TVA</strong>, agregate per partener după regulile
        formularului 394 (OPANAF 3769/2015, modificat prin OPANAF 2194/2025): tipul de operațiune
        din politica <em>MapareD394</em>, tipul de partener din identitatea fiscală a partenerului,
        numărul de facturi 1/0 per document pe cota cu TVA-ul maxim. Sumele sunt exacte (în bani);
        rotunjirea la leu întreg, detaliul pe categorii de bunuri, bonurile fiscale și fișierul XML
        sunt altă unealtă.
      </p>
    </div>
  );
}

// Avertismentele (D4-D5), AGREGATE per cauză pe server (fix 7 al review-ului
// advers): un rând per cauză cu numărul de cazuri și suma, exemplele nominale
// (cel mult 5) pliate sub rând. Pe o bază reală o cauză poate avea mii de
// cazuri — tabelul rămâne de câteva rânduri, iar semnalul rar (tip 1 fără CUI,
// V cu TVA) nu se mai îneacă în cel comun (PF cu CNP în alt format).
function Avertismente({ lista }: { lista: Avertisment[] }) {
  if (lista.length === 0) return null;
  return (
    <div className="d394__avertismente">
      <h3>Avertismente ({lista.length} {lista.length === 1 ? 'cauză' : 'cauze'})</h3>
      <table className="tabel-mic">
        <thead>
          <tr>
            <th>Cauză</th>
            <th>Cazuri</th>
            <th>Sumă</th>
            <th>Descriere</th>
          </tr>
        </thead>
        <tbody>
          {lista.map((a) => (
            <tr key={a.Cod ?? ''}>
              <td><strong>{labelEnum('CodAvertismentD394', a.Cod)}</strong></td>
              <td className="num">{a.Numar}</td>
              <td className="num">{a.Suma == null ? '' : bani(a.Suma)}</td>
              <td>
                {a.Mesaj}
                {(a.Exemple?.length ?? 0) > 0 && (
                  <details>
                    <summary>
                      {a.Exemple!.length === a.Numar ? 'Cazurile' : `Primele ${a.Exemple!.length} din ${a.Numar}`}
                    </summary>
                    <ul>{a.Exemple!.map((e, i) => <li key={i}>{e}</li>)}</ul>
                  </details>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

// Coloanele unui rezumat C–F, în ordinea formularului. Fiecare tip are
// facturi + bază (+ TVA unde XSD-ul are coloana). O celulă e goală când
// serverul a trimis `null` (XSD-ul cere absența), „0" când e un zero adevărat.
const COLOANE_REZUMAT: { tip: string; facturi: keyof Rezumat; baza: keyof Rezumat; tva?: keyof Rezumat }[] = [
  { tip: 'L', facturi: 'FacturiL', baza: 'BazaL', tva: 'TvaL' },
  { tip: 'LS', facturi: 'FacturiLS', baza: 'BazaLS' },
  { tip: 'A', facturi: 'FacturiA', baza: 'BazaA', tva: 'TvaA' },
  { tip: 'AÎ', facturi: 'FacturiAI', baza: 'BazaAI', tva: 'TvaAI' },
  { tip: 'AS', facturi: 'FacturiAS', baza: 'BazaAS' },
  { tip: 'V', facturi: 'FacturiV', baza: 'BazaV' },
  { tip: 'C', facturi: 'FacturiC', baza: 'BazaC', tva: 'TvaC' },
  { tip: 'N', facturi: 'FacturiN', baza: 'BazaN' },
];

// Rezumatele C–F: un tabel per cartuș, un rând per cotă. Tabele HTML, nu grile:
// sunt cel mult câteva rânduri, fără sortare/filtrare (ordinea e a formularului),
// iar coloanele prezente depind de cartuș — se arată doar cele pe care cel puțin
// un rând le are non-null, ca tabelul să nu fie o mare de celule goale.
function Rezumate({ rezumat }: { rezumat: Rezumat[] }) {
  const tipuri = [1, 2, 3, 4].filter((t) => rezumat.some((r) => r.TipPartener === t));
  if (tipuri.length === 0) return null;
  return (
    <div className="d394__rezumate">
      {tipuri.map((tp) => {
        const randuri = rezumat.filter((r) => r.TipPartener === tp);
        const coloane = COLOANE_REZUMAT.filter((c) => randuri.some((r) => r[c.facturi] != null || r[c.baza] != null));
        return (
          <div key={tp} className="d394__rezumat">
            <h3>Rezumat {cartus(tp)}</h3>
            <table className="tabel-mic">
              <thead>
                <tr>
                  <th rowSpan={2}>Cotă %</th>
                  {coloane.map((c) => <th key={c.tip} colSpan={c.tva ? 3 : 2}>{c.tip}</th>)}
                </tr>
                <tr>
                  {coloane.map((c) => (
                    <FragmentCap key={c.tip} tva={c.tva != null} />
                  ))}
                </tr>
              </thead>
              <tbody>
                {randuri.map((r) => (
                  <tr key={r.Cota}>
                    <td className="num">{r.Cota}</td>
                    {coloane.map((c) => (
                      <FragmentVal key={c.tip} facturi={r[c.facturi] as number | null | undefined}
                        baza={r[c.baza] as number | null | undefined}
                        tva={c.tva ? (r[c.tva] as number | null | undefined) : undefined} cuTva={c.tva != null} />
                    ))}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        );
      })}
    </div>
  );
}

function FragmentCap({ tva }: { tva: boolean }) {
  return (
    <>
      <th>Facturi</th>
      <th>Bază</th>
      {tva && <th>TVA</th>}
    </>
  );
}

function FragmentVal(props: { facturi?: number | null; baza?: number | null; tva?: number | null; cuTva: boolean }) {
  const { facturi, baza, tva, cuTva } = props;
  return (
    <>
      <td className="num">{facturi ?? ''}</td>
      <td className="num">{baza == null ? '' : bani(baza)}</td>
      {cuTva && <td className="num">{tva == null ? '' : bani(tva)}</td>}
    </>
  );
}

// Cartușul H — suma de control per cotă (V intră la L, C intră la A: §4.3).
function CartusH({ cote }: { cote: RezumatCota[] }) {
  if (cote.length === 0) return null;
  return (
    <div className="d394__rezumat">
      <h3>Cartușul H — total per cotă (sume de control)</h3>
      <table className="tabel-mic">
        <thead>
          <tr>
            <th rowSpan={2}>Cotă %</th>
            <th colSpan={3}>L (incl. V)</th>
            <th colSpan={3}>A (incl. C)</th>
            <th colSpan={3}>AÎ</th>
          </tr>
          <tr>
            <FragmentCap tva /><FragmentCap tva /><FragmentCap tva />
          </tr>
        </thead>
        <tbody>
          {cote.map((c) => (
            <tr key={c.Cota}>
              <td className="num">{c.Cota}</td>
              <FragmentVal facturi={c.NrFacturiL} baza={c.BazaL} tva={c.TvaL} cuTva />
              <FragmentVal facturi={c.NrFacturiA} baza={c.BazaA} tva={c.TvaA} cuTva />
              <FragmentVal facturi={c.NrFacturiAI} baza={c.BazaAI} tva={c.TvaAI} cuTva />
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

// CUI-ul lipsă NU e o celulă goală tăcută: rândul se declară, dar ANAF îl
// respinge — avertismentul de sus spune care partener; marcajul de aici spune
// care rând.
function celulaCui({ data }: { data: Operatiune }) {
  if (data.CuiP) return <span>{data.CuiP}</span>;
  return <span className="indiciu" title="Partener fără cod fiscal în nomenclator">(fără CUI)</span>;
}

function celulaTipTva({ data }: { data: Neinclus }) {
  if (data.TipTvaCod) return <span>{data.TipTvaCod}</span>;
  return <span className="indiciu" title={data.TipTvaId ?? ''}>(tip TVA indisponibil)</span>;
}

function bani(v: number): string {
  return v.toLocaleString('ro-RO', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

const BANI = { dataType: 'number', format: '#,##0.00', alignment: 'right', width: 150 } as const;
