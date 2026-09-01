import { useMemo } from 'react';
import { Link, useNavigate } from 'react-router';
import { CheckBox } from 'devextreme-react';
import {
  Column, ColumnFixing, DataGrid, FilterRow, GroupItem, Grouping, GroupPanel,
  HeaderFilter, Pager, Paging, Search, Sorting, Summary, TotalItem,
} from 'devextreme-react/data-grid';
import type { components } from '../../generated/api-types';
import { storeRemote } from '../../nucleu/dxStore';
import { urlCu, useDimensiuniUrl, useUrlStare } from '../../nucleu/urlStare';
import { CasetaPerioada, lunaCurenta } from './comune';

// Balanța de verificare (R-D2…R-D5): agregarea registrului contabil, calculată
// integral pe server. Grilă de CITIRE remote, ca `SoldStoc` — dar cu o
// divergență cerută: DTO-ul e TIPAT din codegen, iar numele de coloană trece
// prin `camp()`, deci o greșeală de nume nu compilează. `SoldStoc` folosește
// `dataField` brut; nu se imită pe punctul ăsta.
type BalantaRand = components['schemas']['BalantaRand'];
const camp = (n: keyof BalantaRand & string) => n;

export function Balanta() {
  const navigheaza = useNavigate();
  const luna = lunaCurenta();
  // Perioada și modul trăiesc în URL (43c): deep-link, refresh și drill-down cu
  // context păstrat. Perioada e PARAMETRU DE PROIECȚIE, nu filtru de grilă
  // (R-D2) — `dataStart` decide ce înseamnă soldul inițial, o graniță
  // dinăuntrul agregării pe care `DataSourceLoader` n-are cum s-o exprime.
  const [stare, seteaza] = useUrlStare({
    dataStart: luna.start,
    dataEnd: luna.sfarsit,
    analitic: false,
  });

  // Cheia grilei depinde de MOD, fiindcă cheia de grupare a proiecției depinde
  // de el (R-D4): sintetic = contul, analitic = cont × repartitor. Cu `ContId`
  // singur, două rânduri analitice ale aceluiași cont ar fi „același rând"
  // pentru grilă.
  // Dimensiunile din URL merg mai departe la API (C1): ecranul nu le culege,
  // dar un deep-link care le conține trebuie ori să filtreze, ori să nu le
  // accepte — nu să le înghită tăcut și să arate balanța întreagă.
  const dimensiuni = useDimensiuniUrl();

  const sursa = useMemo(() => storeRemote(
    urlCu('/api/proiectii/balanta', { ...stare, ...dimensiuni }),
    stare.analitic ? ['ContId', 'RepartitorId'] : ['ContId'],
  ), [stare, dimensiuni]);

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>Balanță de verificare</h2>
        {/* Puntea spre pliul pe plan (BP-D3): acolo cifrele de grup sunt rânduri
            calculate pe server, nu totaluri de grilă — adică exact ce interzice
            R-D5 aici. Perioada merge mai departe. */}
        <Link
          className="buton buton--mic"
          to={urlCu('/balanta-plan', { dataStart: stare.dataStart, dataEnd: stare.dataEnd, ...dimensiuni })}
        >
          Vezi pe planul de conturi
        </Link>
      </div>

      <div className="bara-raport">
        <CasetaPerioada dataStart={stare.dataStart} dataEnd={stare.dataEnd} seteaza={seteaza} />
        <label className="bara-raport__camp">
          <span className="camp__eticheta">Analitic (pe repartitor)</span>
          <CheckBox
            value={stare.analitic}
            onValueChanged={(e) => { if (e.event) seteaza({ analitic: e.value === true }); }}
          />
        </label>
      </div>

      <DataGrid
        // Cheia de remontare: schimbarea modului schimbă CHEIA grilei, iar
        // `DataGrid` nu o reevaluează pe o instanță vie (rândurile vechi rămân
        // indexate pe cheia veche). Remontarea e explicită, nu un efect.
        key={stare.analitic ? 'analitic' : 'sintetic'}
        dataSource={sursa}
        remoteOperations={{ filtering: true, sorting: true, paging: true, summary: true, grouping: true }}
        showBorders
        columnAutoWidth
        height="calc(100vh - 220px)"
        onRowDblClick={(e) => {
          // Rândurile de GRUP n-au date de rând — dublu-click pe ele nu duce
          // nicăieri.
          const rand = e.data as BalantaRand | undefined;
          if (!rand?.ContId) return;
          navigheaza(urlCu('/fisa-cont', {
            contId: rand.ContId,
            dataStart: stare.dataStart,
            dataEnd: stare.dataEnd,
            // Rândul analitic E cont × repartitor: fișa lui înseamnă fișa
            // contului FILTRATĂ pe acel repartitor, altfel drill-down-ul ar
            // duce la alte cifre decât cele din rândul clicat.
            repartitorId: stare.analitic ? rand.RepartitorId : undefined,
            // …iar „fără repartitor" e un rând ANALITIC LEGITIM (LEFT JOIN-ul
            // îl păstrează deliberat; pe baza de import e chiar majoritar —
            // deschiderea s-a scris fără dimensiuni, 47c, iar 2025 a trecut
            // fără dimensiuni culese pe linie, DIM-2). `repartitorId: null` ar
            // dispărea din URL (`urlCu` sare valorile goale) și fișa s-ar
            // deschide pe TOT contul, cu ultimul sold curent egal cu soldul
            // SINTETIC — deci santinelă explicită, nu absență (review D3).
            repartitorNul: stare.analitic && !rand.RepartitorId ? true : undefined,
          }));
        }}
      >
        <Sorting mode="multiple" />
        <FilterRow visible />
        <HeaderFilter visible><Search enabled /></HeaderFilter>
        <GroupPanel visible />
        <Grouping autoExpandAll={false} />
        <ColumnFixing enabled />
        <Paging defaultPageSize={50} />
        <Pager showInfo showPageSizeSelector allowedPageSizes={[50, 100, 200]} />

        {/* Join-ul pe `Cont` e LEFT (review D4): un atom nu se pierde din
            balanță fiindcă i-a dispărut ETICHETA (cont șters soft, sau
            invizibil prin securitate) — altfel linia dispărea și `Σ RulajDebit
            != Σ RulajCredit` în footer, fără nicio explicație. Cifrele rămân
            deci întregi, iar rândul fără etichetă se MARCHEAZĂ, nu se afișează
            gol (un gol ar arăta ca o scăpare de randare). */}
        <Column dataField={camp('ContSimbol')} caption="Cont" fixed width={110} cellRender={celulaCont} />
        <Column dataField={camp('ContDenumire')} caption="Denumire" />
        <Column
          dataField={camp('RepartitorDenumire')}
          caption="Repartitor"
          visible={stare.analitic}
          cellRender={celulaRepartitor}
        />

        <Column caption="Sold inițial">
          <Column dataField={camp('SoldInitialDebit')} caption="Debitor" {...BANI} />
          <Column dataField={camp('SoldInitialCredit')} caption="Creditor" {...BANI} />
        </Column>
        <Column caption="Rulaje perioadă">
          <Column dataField={camp('RulajDebit')} caption="Debit" {...BANI} />
          <Column dataField={camp('RulajCredit')} caption="Credit" {...BANI} />
        </Column>
        <Column caption="Sold final">
          <Column dataField={camp('SoldFinalDebit')} caption="Debitor" {...BANI} />
          <Column dataField={camp('SoldFinalCredit')} caption="Creditor" {...BANI} />
        </Column>
        {/* Sumele brute cumulate până la `dataStart`, ascunse implicit: intră în
            balanța de verificare doar la nevoie, dar SUNT aditive — spre
            deosebire de solduri — deci pot purta totaluri. */}
        <Column dataField={camp('InitialDebit')} caption="Cumulat debit (până la start)" visible={false} {...BANI} />
        <Column dataField={camp('InitialCredit')} caption="Cumulat credit (până la start)" visible={false} {...BANI} />

        {/* ═══ R-D5, impus, nu doar afirmat ═══
            Sumele — de grup ȘI de total — stau EXCLUSIV pe coloanele de rulaj și
            pe cumulatele brute. NICIODATĂ pe `Sold*`: netarea nu e aditivă
            (401 cu un furnizor pe debit 100 și altul pe credit 200 dă analitic
            „D 100 / C 200", sintetic „C 100"), deci un total de solduri pe grup
            ar fi o cifră FALSĂ afișată cu aceeași autoritate ca restul rândului.
            Totalul rulajelor, în schimb, e chiar proba partidei duble:
            Σ debit == Σ credit. */}
        <Summary>
          <GroupItem column={camp('RulajDebit')} summaryType="sum" valueFormat="#,##0.00" displayFormat="{0}" showInGroupFooter={false} alignByColumn />
          <GroupItem column={camp('RulajCredit')} summaryType="sum" valueFormat="#,##0.00" displayFormat="{0}" showInGroupFooter={false} alignByColumn />
          <TotalItem column={camp('RulajDebit')} summaryType="sum" valueFormat="#,##0.00" displayFormat="Σ {0}" />
          <TotalItem column={camp('RulajCredit')} summaryType="sum" valueFormat="#,##0.00" displayFormat="Σ {0}" />
        </Summary>
      </DataGrid>

      <p className="indiciu">
        Dublu-click pe un rând deschide fișa contului, pe aceeași perioadă.
        Totalurile se pun doar pe rulaje — soldurile nu se însumează (netarea nu e aditivă).
      </p>
    </div>
  );
}

// Contul nerezolvat (LEFT JOIN, review D4): cifrele rândului sunt reale și intră
// în totaluri — doar eticheta lipsește. Se spune ca atare, cu identificatorul în
// `title`, ca rândul să fie urmăribil.
function celulaCont({ data }: { data: BalantaRand }) {
  if (data.ContSimbol) return <span>{data.ContSimbol}</span>;
  return <span className="indiciu" title={data.ContId ?? ''}>(cont indisponibil)</span>;
}

// „Fără repartitor" e o cheie de grupare legitimă, nu o celulă goală — și e
// exact rândul al cărui drill-down cere santinela `repartitorNul` (review D3).
function celulaRepartitor({ data }: { data: BalantaRand }) {
  return data.RepartitorDenumire
    ? <span>{data.RepartitorDenumire}</span>
    : <span className="indiciu">(fără repartitor)</span>;
}

const BANI = { dataType: 'number', format: '#,##0.00', alignment: 'right', width: 130 } as const;
