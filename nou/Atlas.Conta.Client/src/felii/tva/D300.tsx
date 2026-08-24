import { useEffect, useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { NumberBox } from 'devextreme-react';
import {
  Column, ColumnFixing, DataGrid, Grouping, GroupPanel, Paging, Scrolling, Sorting,
} from 'devextreme-react/data-grid';
import type { components } from '../../generated/api-types';
import { labelEnum } from '../../nucleu/campMeta';
import { eroriDin, ia } from '../../nucleu/http';
import { PanouErori } from '../../nucleu/PanouErori';
import { urlCu, useUrlStare } from '../../nucleu/urlStare';
import { CasetaData, lunaCurenta } from '../raportare/comune';

// D300 — decontul de TVA (felia 12, D3-D7): cele 55 de poziții ale formularului
// OPANAF 174/2026, alimentate din `RegistruTva`.
//
// Ecran separat de `/decont-tva`, nu un mod al lui, din același motiv pentru care
// balanța pliată e separată de cea plată: datele vin ALTFEL. Decontul-schelet e
// o listă paginată remote pe `(Sens × TipTva × Regim × Cotă)`; aici răspunsul e
// un FORMULAR — tabloul întreg, în ordinea legii, cu totaluri care sunt RÂNDURI
// ale lui, nu sume de grilă. Un `if` între cele două căi de date ar fi fost o
// componentă cu două personalități.
//
// Ce NU e, la fel de apăsat ca pe scheletul de alături: fișierul declarației.
// Felia garantează CIFRELE și dovada că nu pierde nimic din registru (panoul
// „Neincluse"); XML-ul pe schema v12.0.0, validarea DUKIntegrator și depunerea
// sunt proiect izolat (35c).
type D300Dto = components['schemas']['D300Dto'];
type Rand = components['schemas']['D300Rand'];
type Nemapat = components['schemas']['D300Nemapat'];

const camp = (n: keyof Rand & string) => n;
const campNemapat = (n: keyof Nemapat & string) => n;

// Ordinea secțiunilor e cea a FORMULARULUI, nu cea alfabetică a numelor de enum
// (care azi coincide — „Colectata" < „Deductibila" < „Regularizari" — și tocmai
// de aceea ar fi trecut neobservată la prima redenumire). Rangul e explicit.
const RANG_SECTIUNE: Record<string, number> = { Colectata: 1, Deductibila: 2, Regularizari: 3 };

// Cei patru externi (D3-D3 pasul 6): cifre pe care formularul le cere, dar
// modelul nu le are. Trăiesc în URL ca tot restul stării globale (43c), deci ca
// STRING-uri — `useUrlStare` are exact două feluri de valoare, iar un „număr"
// din query string e oricum text până la parse. Implicitul „0" iese din URL, ca
// orice implicit, deci un link fără istoric rămâne scurt.
const EXTERNI = [
  { cheie: 'soldPlataPrecedent', eticheta: 'rd. 38 — Sold TVA de plată precedent' },
  { cheie: 'diferentePlata', eticheta: 'rd. 39 — Diferențe de plată (organ fiscal)' },
  { cheie: 'soldNegativPrecedent', eticheta: 'rd. 41 — Sold negativ TVA precedent' },
  { cheie: 'diferenteNegative', eticheta: 'rd. 42 — Diferențe negative (organ fiscal)' },
] as const;

export function D300() {
  const luna = lunaCurenta();
  // Perioada e OBLIGATORIE pe server (400 fără ea): aici perioada E declarația,
  // nu un filtru. Implicitul lunii curente există ca prima deschidere să arate
  // ceva, nu ca să scutească alegerea — se vede în bară și în URL.
  const [stare, seteaza] = useUrlStare({
    dataStart: luna.start,
    dataEnd: luna.sfarsit,
    soldPlataPrecedent: '0',
    diferentePlata: '0',
    soldNegativPrecedent: '0',
    diferenteNegative: '0',
  });

  const cale = urlCu('/api/proiectii/d300', {
    dataStart: stare.dataStart,
    dataEnd: stare.dataEnd,
    soldPlataPrecedent: stare.soldPlataPrecedent,
    diferentePlata: stare.diferentePlata,
    soldNegativPrecedent: stare.soldNegativPrecedent,
    diferenteNegative: stare.diferenteNegative,
  });
  // Aceeași cale de citire ca balanța pliată: `useQuery` + `ia` (JWT, 401 →
  // sesiune expirată, 400/422 `EroriDto` → mesaje de domeniu, afișate). Nu
  // `storeRemote`: n-avem ce cere unui `DataSourceLoader` pe un tablou de 55 de
  // rânduri pe care serverul îl întoarce întreg și în ordinea lui.
  const citit = useQuery({ queryKey: ['d300', cale], queryFn: () => ia<D300Dto>(cale) });

  const randuri = useMemo(() => citit.data?.Randuri ?? [], [citit.data]);
  const nemapate = useMemo(() => citit.data?.Nemapate ?? [], [citit.data]);
  const avertismente = citit.data?.Avertismente ?? [];
  // Cheia de remontare: parametrii cererii. Gruparea și expandarea nu se
  // reevaluează pe o instanță vie (lecția `BalantaPlan`).
  const cheie = `${stare.dataStart}|${stare.dataEnd}`;

  return (
    <div className="ecran">
      <div className="ecran__bara"><h2>D300 — decont de TVA</h2></div>

      <div className="bara-raport">
        <CasetaData eticheta="De la" valoare={stare.dataStart} seteaza={(v) => seteaza({ dataStart: v })} />
        <CasetaData eticheta="Până la" valoare={stare.dataEnd} seteaza={(v) => seteaza({ dataEnd: v })} />
        {EXTERNI.map(({ cheie: c, eticheta }) => (
          <CasetaExtern
            key={c}
            eticheta={eticheta}
            valoare={numar(stare[c])}
            seteaza={(v) => seteaza({ [c]: String(v) })}
          />
        ))}
      </div>

      {/* Refuzurile serverului (perioadă inversată, extern negativ, 38 ∧ 41) sunt
          DATE, nu excepții de transport: se citesc în bară, nu în consolă. */}
      <PanouErori erori={citit.error ? eroriDin(citit.error) : []} titlu="Cererea a fost refuzată" />
      {avertismente.length > 0 && (
        <PanouErori erori={avertismente} titlu="Avertismente" fel="atentie" />
      )}

      {citit.isPending ? <p className="indiciu">Se încarcă…</p> : (
        <DataGrid
          key={cheie}
          className="d300"
          dataSource={randuri}
          keyExpr={camp('Ordine')}
          showBorders
          // FĂRĂ `columnAutoWidth`, spre deosebire de toate listele: denumirile
          // formularului sunt propoziții de lege („Livrări de bunuri sau prestări
          // de servicii pentru care locul livrării…"), iar o coloană dimensionată
          // după conținut le-ar da 1400px și ar împinge BAZA și TVA-ul în afara
          // ecranului. Pe un formular, cifrele sunt lucrul care trebuie văzut
          // fără scroll orizontal; denumirea se rupe pe rânduri.
          wordWrapEnabled
          height="calc(100vh - 320px)"
          onRowPrepared={(e) => {
            if (e.rowType !== 'data') return;
            const r = e.data as Rand;
            if (r.Fel === 'Total') e.rowElement.classList.add('d300__total');
            if (r.Fel === 'Oglinda') e.rowElement.classList.add('d300__oglinda');
            if (r.Fel === 'Extern') e.rowElement.classList.add('d300__extern');
          }}
        >
          {/* Formularul se citește ÎNTREG și DOAR în ordinea lui: fără paginare
              (rd. 19 fără rd. 9 nu e „pagina 1 dintr-un decont"), fără sortare de
              utilizator (o coloană sortată descrescător ar produce un tablou care
              arată a decont și nu e). */}
          <Sorting mode="none" />
          <Paging enabled={false} />
          <Scrolling mode="standard" />
          <ColumnFixing enabled />
          <GroupPanel visible={false} />
          <Grouping autoExpandAll />

          <Column
            dataField={camp('Sectiune')}
            caption="Secțiune"
            groupIndex={0}
            sortingMethod={(a: string, b: string) => (RANG_SECTIUNE[a] ?? 99) - (RANG_SECTIUNE[b] ?? 99)}
            groupCellRender={({ value }: { value: string }) => (
              <span>{labelEnum('SectiuneD300', value)}</span>
            )}
          />
          {/* Ordinea din formular, ca sortare fixă a rândurilor din grup. Coloana
              e invizibilă: e cheia de așezare, nu o cifră de citit. */}
          <Column dataField={camp('Ordine')} visible={false} sortIndex={1} sortOrder="asc" />

          <Column dataField={camp('Cod')} caption="Rd." width={70} fixed alignment="right" />
          <Column dataField={camp('Denumire')} caption="Denumire" minWidth={320} cellRender={celulaDenumire} />

          {/* `Baza`/`Tva` sunt `null` acolo unde formularul N-ARE coloana
              (rd. 13/14/15/29 fără TVA, rd. 31/36-45 fără bază). Celula rămâne
              GOALĂ: „0,00" într-o casetă inexistentă e o cifră inventată, iar
              zero e o cifră adevărată („coloana există și e goală"). */}
          <Column dataField={camp('Baza')} caption="Bază" {...BANI} />
          <Column dataField={camp('Tva')} caption="TVA" {...BANI} />

          {/* Urma: câte rânduri de registru au alimentat cifra și ce tipuri de TVA
              au intrat în ea. 0 / gol pe totaluri și externi — acolo nu intră
              rânduri noi de registru. */}
          <Column dataField={camp('Randuri')} caption="Rânduri" dataType="number" format="#,##0" alignment="right" width={90} />
          <Column dataField={camp('Surse')} caption="Surse" width={160} />

          {/* Fără `Summary`, deliberat: totalurile formularului (19, 30, 31, 35,
              36/37, 40, 43, 44/45) SUNT rânduri ale lui, calculate pe server după
              formulele legii. Un total de grilă le-ar aduna peste rândurile de
              operațiuni care le compun — adică ar număra fiecare cifră de două
              ori și ar produce o sumă care nu există în niciun formular. */}
        </DataGrid>
      )}

      {/* D3-D4: nimic nu se pierde. Panoul apare DOAR când există grupuri fără
          mapare — dar când există, arată cifrele lor, nu un număr. Un gard care
          tace devine capcană (62f); aici tăcerea ar fi însemnat un decont care
          pare complet și nu e. */}
      {nemapate.length > 0 && (
        <div className="d300__neincluse">
          <h3>Operațiuni neincluse în decont</h3>
          <p className="indiciu">
            Rânduri de registru pe perioada aleasă a căror pereche (tip de TVA × sens) nu are
            mapare către un rând al formularului — deci cifrele de mai jos <strong>nu</strong> se
            regăsesc în tabloul de sus. Cauze legitime: un tip de TVA propriu, încă nemapat, sau o
            gaură deliberată a formularului (achiziția cu cota tranzitorie de 9% n-are rând în
            forma 2026). Maparea se face în BackOffice, pe politica <em>MapareD300</em>.
          </p>
          <DataGrid
            key={`neincluse|${cheie}`}
            dataSource={nemapate}
            showBorders
            columnAutoWidth
            height={Math.min(60 + nemapate.length * 34, 300)}
          >
            <Sorting mode="none" />
            <Paging enabled={false} />
            <Column dataField={campNemapat('Sens')} caption="Sens" width={110} cellRender={celulaSens} />
            <Column dataField={campNemapat('TipTvaCod')} caption="Tip TVA" width={90} cellRender={celulaTipTva} />
            <Column dataField={campNemapat('TipTvaDenumire')} caption="Denumire" />
            <Column dataField={campNemapat('Regim')} caption="Regim" width={130} cellRender={celulaRegim} />
            <Column dataField={campNemapat('Cota')} caption="Cotă %" dataType="number" format="#0.##" alignment="right" width={80} />
            <Column dataField={campNemapat('Baza')} caption="Bază" {...BANI} />
            <Column dataField={campNemapat('Tva')} caption="TVA" {...BANI} />
            <Column dataField={campNemapat('Randuri')} caption="Rânduri" dataType="number" format="#,##0" alignment="right" width={90} />
          </DataGrid>
        </div>
      )}

      <p className="indiciu">
        Cifrele sunt cele din <strong>registrul de TVA</strong>, așezate pe rândurile formularului
        OPANAF 174/2026. Rândurile în <em>italic</em> sunt oglinzi (copia unui rând din secțiunea
        cealaltă, la taxare inversă), cele îngroșate sunt totaluri calculate după formulele
        ordinului, iar cele marcate „extern" nu au sursă în model (se culeg în bară sau rămân 0).
        Ecranul <strong>nu</strong> produce declarația: fișierul XML, validările ANAF și depunerea
        sunt altă unealtă.
      </p>
    </div>
  );
}

// Caseta unui extern. Există ca și COMPONENTĂ, nu ca `NumberBox` inline, pentru
// starea locală — și starea locală există dintr-un motiv măsurat în browser, nu
// din prudență:
//
// `if (e.event)` (57f) e necesar, dar NU suficient pentru un widget în care se
// TASTEAZĂ și a cărui valoare face drumul întors printr-un store ASINCRON (aici
// URL-ul, prin `setSearchParams`). Legat direct de `stare`, `NumberBox` producea
// două evenimente la un singur Enter:
//
//     val=100 prev=0   event=change   ← editarea omului; scrie ?…=100
//     val=0   prev=100 event=change   ← ECOUL propriului prop, încă STALE (0),
//                                       cu ACELAȘI obiect `change` atașat
//
// Al doilea trece de gardă (are `e.event`!) și rescrie implicitul, adică șterge
// exact ce tocmai s-a cules. Ecranele de până acum n-au întâlnit-o fiindcă
// `useCamp` scrie într-un `useState` care e la zi la următoarea randare, iar
// `SelectBox`/`CheckBox` din rapoarte n-au buffer de tastare care să rămână în
// urmă. Cu valoarea ținută local, prop-ul nu mai e niciodată stale, deci ecoul
// nu se mai poate naște; URL-ul rămâne destinația, nu sursa de la o tastă la
// alta. `useEffect` re-însămânțează caseta când URL-ul se schimbă din AFARĂ
// (deep-link, butonul „înapoi"), ca link-ul să rămână adevărul partajabil.
//
// Dacă apare a doua casetă tastabilă legată de URL în altă felie, asta e piesa
// care se mută în `nucleu` — azi ar fi o abstracție cu un singur consumator.
function CasetaExtern(props: { eticheta: string; valoare: number; seteaza: (v: number) => void }) {
  const { eticheta, valoare, seteaza } = props;
  const [local, setLocal] = useState(valoare);
  useEffect(() => { setLocal(valoare); }, [valoare]);
  return (
    <label className="bara-raport__camp">
      <span className="camp__eticheta">{eticheta}</span>
      <NumberBox
        width={190}
        format="#,##0.00"
        min={0}
        value={local}
        onValueChanged={(e) => {
          if (!e.event) return;
          const v = Number(e.value ?? 0);
          setLocal(v);
          seteaza(v);
        }}
      />
    </label>
  );
}

// Denumirea, cu cele trei lucruri pe care rândul trebuie să le spună dintr-o
// privire: e sub-rând „din care" (indentat), e oglindă, sau e o cifră fără sursă
// în model. Marcajul e pe CELULĂ, nu doar pe rând, ca să supraviețuiască unei
// eventuale schimbări de temă a grilei.
function celulaDenumire({ data }: { data: Rand }) {
  return (
    <span className={data.Nivel === 1 ? 'd300__nivel1' : undefined}>
      {data.Denumire}
      {data.Fel === 'Extern' && <span className="d300__eticheta" title="Fără sursă în model — se culege în bară sau rămâne 0"> extern</span>}
      {data.Fel === 'Oglinda' && <span className="d300__eticheta" title="Copie a rândului-sursă din cealaltă secțiune (taxare inversă)"> oglindă</span>}
    </span>
  );
}

function celulaSens({ data }: { data: Nemapat }) {
  return <span>{labelEnum('SensTva', data.Sens)}</span>;
}

function celulaRegim({ data }: { data: Nemapat }) {
  return <span>{labelEnum('RegimTva', data.Regim)}</span>;
}

// LEFT join pe nomenclator: un `TipTva` șters logic nu face grupul să dispară din
// panou — cifra rămâne, eticheta se marchează (review D4 al feliei 9).
function celulaTipTva({ data }: { data: Nemapat }) {
  if (data.TipTvaCod) return <span>{data.TipTvaCod}</span>;
  return <span className="indiciu" title={data.TipTvaId ?? ''}>(tip TVA indisponibil)</span>;
}

function numar(v: string): number {
  const n = Number(v);
  return Number.isFinite(n) ? n : 0;
}

const BANI = { dataType: 'number', format: '#,##0.00', alignment: 'right', width: 150 } as const;
