import { useMemo } from 'react';
import { Link, useNavigate } from 'react-router';
import { useQuery } from '@tanstack/react-query';
import { CheckBox, SelectBox } from 'devextreme-react';
import { Column, ColumnFixing, Scrolling, TreeList } from 'devextreme-react/tree-list';
import type { components } from '../../generated/api-types';
import { ia } from '../../nucleu/http';
import { PanouErori } from '../../nucleu/PanouErori';
import { urlCu, useDimensiuniUrl, useUrlStare } from '../../nucleu/urlStare';
import { CasetaData, lunaCurenta } from './comune';

// Balanța pliată pe planul de conturi (BP-D1…BP-D5) — rollup-ul pe care felia 9
// l-a lăsat deschis cu un motiv (R-D5): pe un grup de grilă rulajele se pot
// însuma, dar SOLDURILE nu. Aici cifrele de grup nu mai sunt totaluri de grilă,
// ci rânduri calculate pe server, unde brutele se cumulează în sus și netarea se
// face la fiecare nod.
//
// Ecran separat de balanța plată, nu un mod al ei, fiindcă datele vin altfel:
// dincolo, `DataSourceLoader` cu paginare remote; aici, tabloul ÎNTREG (un arbore
// nu se paginează — BP-D3). Două căi de date sub același `if` ar fi fost o
// componentă cu două personalități; legătura dintre ecrane e un link care duce
// perioada mai departe.
type Nod = components['schemas']['BalantaPlanRand'];
const camp = (n: keyof Nod & string) => n;

// Adâncimile oferite. „Tot planul" e implicitul: trunchierea e un raport ANUME
// („balanța pe clase"), nu un mod implicit de citire.
const ADANCIMI = [
  { valoare: '', eticheta: 'Tot planul' },
  { valoare: '1', eticheta: 'Doar nivelul 1 (clase)' },
  { valoare: '2', eticheta: 'Până la nivelul 2 (grupe)' },
  { valoare: '3', eticheta: 'Până la nivelul 3' },
];

export function BalantaPlan() {
  const navigheaza = useNavigate();
  const luna = lunaCurenta();
  const [stare, seteaza] = useUrlStare({
    dataStart: luna.start,
    dataEnd: luna.sfarsit,
    nivelMaxim: '',
    extinde: false,
  });
  // Ca la balanța plată: ecranul nu culege dimensiuni, dar un deep-link care le
  // conține trebuie ori să filtreze, ori să nu fie acceptat — niciodată să le
  // înghită tăcut și să arate altă balanță decât cea cerută.
  const dimensiuni = useDimensiuniUrl();

  const cale = urlCu('/api/proiectii/balanta-plan', {
    dataStart: stare.dataStart,
    dataEnd: stare.dataEnd,
    nivelMaxim: stare.nivelMaxim,
    ...dimensiuni,
  });
  const citit = useQuery({ queryKey: ['balanta-plan', cale], queryFn: () => ia<Nod[]>(cale) });
  const date = useMemo(() => citit.data ?? [], [citit.data]);

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>Balanță pe planul de conturi</h2>
        <Link
          className="buton buton--mic"
          to={urlCu('/balanta', { dataStart: stare.dataStart, dataEnd: stare.dataEnd, ...dimensiuni })}
        >
          Vezi balanța plată
        </Link>
      </div>

      <div className="bara-raport">
        <CasetaData eticheta="De la" valoare={stare.dataStart} seteaza={(v) => seteaza({ dataStart: v })} />
        <CasetaData eticheta="Până la" valoare={stare.dataEnd} seteaza={(v) => seteaza({ dataEnd: v })} />
        <label className="bara-raport__camp">
          <span className="camp__eticheta">Adâncime</span>
          <SelectBox
            width={220}
            items={ADANCIMI}
            valueExpr="valoare"
            displayExpr="eticheta"
            value={stare.nivelMaxim}
            // Regula canonică a vocabularului (56e/57f): doar acțiunile omului
            // schimbă starea — schimbarea programatică ar raporta prin closure-ul
            // vechi și ar rescrie ce s-a cules între timp.
            onValueChanged={(e) => { if (e.event) seteaza({ nivelMaxim: String(e.value ?? '') }); }}
          />
        </label>
        <label className="bara-raport__camp">
          <span className="camp__eticheta">Extinde tot</span>
          <CheckBox
            value={stare.extinde}
            onValueChanged={(e) => { if (e.event) seteaza({ extinde: e.value === true }); }}
          />
        </label>
      </div>

      <PanouErori erori={citit.error ? [String((citit.error as Error).message)] : []} />

      {citit.isPending ? <p className="indiciu">Se încarcă…</p> : (
      <TreeList
        // Remontare la schimbarea adâncimii sau a modului de expandare:
        // `autoExpandAll` și mulțimea de chei nu se reevaluează pe o instanță vie.
        //
        // Și de aceea randarea e GATE-uită pe încărcare, nu doar cheiată:
        // `autoExpandAll` se aplică la momentul în care arborele își primește
        // datele. Montat pe tabloul gol (datele vin din `useQuery`, deci mai
        // târziu), rămânea cu toate nodurile strânse, iar bifa „Extinde tot” nu
        // făcea nimic — probat în browser.
        key={`${stare.nivelMaxim}|${stare.extinde}`}
        dataSource={date}
        keyExpr={camp('ContId')}
        parentIdExpr={camp('ParinteId')}
        // Rădăcina e `null` (nu 0, implicitul bibliotecii): `ParinteId` e `Guid?`,
        // iar nodul fără părinte VIZIBIL îl are chiar null (BP-D2) — cu implicitul
        // nemodificat, întreg arborele ar fi ieșit gol.
        rootValue={null}
        // Nodurile fără copii în mulțimea ÎNTOARSĂ nu oferă expandare: sub
        // trunchiere, ultimul nivel păstrat are descendenți în plan, dar nu aici.
        hasItemsExpr={camp('AreCopii')}
        autoExpandAll={stare.extinde}
        showBorders
        columnAutoWidth
        height="calc(100vh - 220px)"
        onRowDblClick={(e) => {
          const nod = e.data as Nod | undefined;
          // Drill-down doar de pe nodurile cu mișcare PROPRIE: fișa unui cont
          // pur-sumator ar fi goală, fiindcă cifrele lui sunt ale descendenților.
          // Un ecran gol s-ar citi ca o defecțiune, nu ca un răspuns.
          if (!nod?.ContId || !nod.AreMiscareProprie) return;
          navigheaza(urlCu('/fisa-cont', {
            contId: nod.ContId,
            dataStart: stare.dataStart,
            dataEnd: stare.dataEnd,
            ...dimensiuni,
          }));
        }}
      >
        <Scrolling mode="standard" />
        <ColumnFixing enabled />

        <Column dataField={camp('ContSimbol')} caption="Cont" fixed width={220} cellRender={celulaCont} />
        <Column dataField={camp('ContDenumire')} caption="Denumire" />

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

        {/* Fără `Summary`, deliberat — și nu din economie de cod: pe ecranul ăsta
            fiecare rând E DEJA un total, iar un total de grilă peste rândurile
            afișate ar aduna părinții cu copiii lor, adică ar număra fiecare
            frunză de câte ori are strămoși. Cifra de control (Σ rădăcini ==
            Σ balanța plată) se verifică în ModelCheck, unde poate fi exactă. */}
      </TreeList>
      )}

      <p className="indiciu">
        Cifrele fiecărui nod sunt cumulate din descendenți și <strong>netate la
        nivelul lui</strong>: soldul unei grupe nu e suma soldurilor copiilor.
        Nodurile marcate „·” au și mișcare proprie, deci diferă de suma copiilor
        afișați. Dublu-click pe un cont cu mișcare proprie deschide fișa lui, pe
        aceeași perioadă.
      </p>
    </div>
  );
}

// Simbolul, cu cele două lucruri pe care rândul trebuie să le spună onest:
// contul fără etichetă (șters logic sau invizibil prin securitate — cifrele lui
// rămân în arbore, review D4 al feliei 9) și nodul care are și mișcare proprie.
function celulaCont({ data }: { data: Nod }) {
  const eticheta = data.ContSimbol
    ? <span>{data.ContSimbol}</span>
    : <span className="indiciu" title={data.ContId ?? ''}>(cont indisponibil)</span>;
  if (!data.AreMiscareProprie || !data.AreCopii) return eticheta;
  return (
    <span>
      {eticheta}
      <span className="indiciu" title="Are și mișcare proprie, nu doar prin descendenți"> ·</span>
    </span>
  );
}

const BANI = { dataType: 'number', format: '#,##0.00', alignment: 'right', width: 130 } as const;
