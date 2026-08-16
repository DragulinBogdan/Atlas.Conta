import { useMemo, useState } from 'react';
import { Link } from 'react-router';
import { SelectBox } from 'devextreme-react';
import {
  Column, ColumnFixing, DataGrid, FilterRow, Pager, Paging, Sorting,
} from 'devextreme-react/data-grid';
import type { components } from '../../generated/api-types';
import { storeRemote } from '../../nucleu/dxStore';
import { rutaTip } from '../../nucleu/stingeri';
import { urlCu, useUrlStare } from '../../nucleu/urlStare';
import { CasetaData, etichetaCont, lunaCurenta, useSursaConturi, type ElementCont } from './comune';

// Fișa de cont (R-D6): rândurile unui cont, cronologic, cu SOLDUL CURENT cumulat
// până la fiecare rând inclusiv — calculat de o funcție de fereastră pe server.
// Nimic nu se cumulează aici: un cumul în TS ar minți la prima paginare (42c).
type FisaContRand = components['schemas']['FisaContRand'];
const camp = (n: keyof FisaContRand & string) => n;

export function FisaCont() {
  const luna = lunaCurenta();
  const [stare, seteaza] = useUrlStare({
    contId: '',
    dataStart: luna.start,
    dataEnd: luna.sfarsit,
    // Dus mai departe de drill-down-ul din balanța ANALITICĂ: rândul clicat e
    // cont × repartitor, deci fișa lui e a contului filtrată pe repartitor.
    // Filtru de PROIECȚIE (R-D2), nu de grilă — se aplică înaintea ferestrei.
    repartitorId: '',
  });

  const sursaConturi = useSursaConturi();
  // Contul ales, ca date brute ale nomenclatorului: îl dă widget-ul (pagina lui
  // încărcată sau `byKey`-ul pe deep-link), deci nu există un al doilea fetch și
  // nici o a doua sursă de adevăr pentru simbol/denumire.
  const [cont, setCont] = useState<ElementCont | null>(null);

  const sursa = useMemo(() => (
    stare.contId
      ? storeRemote(urlCu('/api/proiectii/fisa-cont', stare), ['Id', 'Sens'])
      : null
  ), [stare]);

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>Fișă de cont{cont?.Simbol ? ` — ${etichetaCont(cont)}` : ''}</h2>
        <Link className="buton buton--mic" to={urlCu('/balanta', { dataStart: stare.dataStart, dataEnd: stare.dataEnd })}>
          Înapoi la balanță
        </Link>
      </div>

      <div className="bara-raport">
        <label className="bara-raport__camp bara-raport__camp--lat">
          <span className="camp__eticheta">Cont</span>
          <SelectBox
            dataSource={sursaConturi}
            value={stare.contId || null}
            valueExpr="ID"
            // Referință STABILĂ, nu arrow inline: un `displayExpr` nou la
            // fiecare randare face widget-ul să reia rezolvarea afișării
            // (`byKey`), iar rezolvarea ridică `onSelectionChanged` — cu un
            // `setCont` necondiționat asta e o buclă infinită de cereri
            // `Cont(id)` care înfometează grila. Găsit la smoke, exact așa.
            displayExpr={etichetaCont}
            searchEnabled
            searchExpr={['Simbol', 'Denumire']}
            searchTimeout={300}
            noDataText="Nimic găsit"
            width={340}
            onValueChanged={(e) => {
              // Aceeași regulă ca peste tot (F2/F3): doar acțiunile omului
              // schimbă starea. Schimbarea PROGRAMATICĂ (deep-link rezolvat de
              // widget) ar rescrie URL-ul cu valoarea din care tocmai a venit.
              if (!e.event) return;
              seteaza({ contId: (e.value as string) ?? '' });
            }}
            // A doua jumătate a plasei: se re-randează doar la SCHIMBAREA
            // contului. Fără gardul ăsta, orice re-rezolvare a afișării ar
            // pune un obiect NOU în stare și ar reporni ciclul, chiar cu
            // `displayExpr` stabil. (`ID` e obiect `Guid` DevExtreme —
            // comparația trece prin `String()`, ca în `Lookup`.)
            onSelectionChanged={(e) => {
              const ales = (e.selectedItem as ElementCont) ?? null;
              setCont((precedent) => (String(precedent?.ID) === String(ales?.ID) ? precedent : ales));
            }}
          />
        </label>
        <CasetaData eticheta="De la" valoare={stare.dataStart} seteaza={(v) => seteaza({ dataStart: v })} />
        <CasetaData eticheta="Până la" valoare={stare.dataEnd} seteaza={(v) => seteaza({ dataEnd: v })} />
      </div>

      {sursa === null ? (
        <p className="indiciu">Alegeți un cont — sau intrați din balanță, cu dublu-click pe rândul lui.</p>
      ) : (
        <>
          <DataGrid
            // Perioada și contul sunt PARAMETRI ai proiecției, nu filtre de
            // grilă: la schimbarea lor se schimbă sursa, iar grila se remontează
            // (starea ei de filtrare/paginare n-are ce transporta între două
            // rapoarte diferite).
            key={`${stare.contId}|${stare.dataStart}|${stare.dataEnd}|${stare.repartitorId}`}
            dataSource={sursa}
            remoteOperations={{ filtering: true, paging: true }}
            showBorders
            columnAutoWidth
            height="calc(100vh - 230px)"
          >
            {/* ═══ Ordinea e FIXĂ cronologic (R-D6) ═══
                Soldul curent are înțeles doar în ordinea în care a fost cumulat.
                Reordonat după alt criteriu ar fi o coloană de cifre fără sens,
                afișată cu aceeași autoritate ca restul — de aceea serverul
                REFUZĂ `sort=` (îl anulează tăcut), iar grila nici nu-l oferă.
                Filtrarea rămâne permisă: `SoldCurent` e proprietate a
                registrului la acel rând, deci rămâne adevărată pe orice
                submulțime afișată. */}
            <Sorting mode="none" />
            <FilterRow visible />
            {/* FĂRĂ `HeaderFilter`, și nu din gust: lista lui de valori se cere
                server-side prin protocolul de GRUPARE al `DataSourceLoader`, iar
                fișa îl refuză (`loadOptions.Group = null`, R-D6) — panoul ar
                ieși gol sau, mai rău, plauzibil și greșit. Filtrul de rând nu
                trece prin grupare, deci rămâne. */}
            <ColumnFixing enabled />
            <Paging defaultPageSize={50} />
            <Pager showInfo showPageSizeSelector allowedPageSizes={[50, 100, 200]} />

            <Column dataField={camp('Data')} caption="Data" dataType="date" format="dd.MM.yyyy" width={100} fixed />
            <Column dataField={camp('NumarNota')} caption="Nr. notă" width={110} />
            <Column dataField={camp('Sens')} caption="Sens" width={70} alignment="center" />
            <Column dataField={camp('ContrapartidaSimbol')} caption="Contrapartidă" width={120} />
            <Column dataField={camp('Debit')} caption="Debit" {...BANI} />
            <Column dataField={camp('Credit')} caption="Credit" {...BANI} />
            {/* Vine cumulat de la server (fereastră SQL), niciodată din TS. */}
            <Column dataField={camp('SoldCurent')} caption="Sold curent" {...BANI} />
            <Column dataField={camp('RepartitorDenumire')} caption="Repartitor" />
            <Column
              dataField={camp('DocumentNumar')}
              caption="Document"
              width={150}
              // `DocumentTip` se completează în memorie peste pagină (R-D8), deci
              // nu e coloană filtrabilă — rutarea trece prin `rutaTip`
              // (vocabular închis): tip fără felie de client rămâne TEXT, nu link
              // mort. `DocumentId` null = rând de sold de deschidere scris de
              // migrare (25e/34d) — se afișează ca atare, fără marcaj special.
              allowFiltering={false}
              cellRender={celulaDocument}
            />
            <Column dataField={camp('Storno')} caption="Storno" dataType="boolean" width={80} />
          </DataGrid>

          <p className="indiciu">
            Ordinea e cronologică fixă și nu se poate schimba: soldul curent e cumulat în această ordine.
            Rândurile fără document sunt solduri de deschidere.
          </p>
        </>
      )}
    </div>
  );
}

function celulaDocument({ data }: { data: FisaContRand }) {
  if (!data.DocumentId) return <span className="indiciu">(deschidere)</span>;
  const ruta = rutaTip(data.DocumentTip, data.DocumentId);
  const text = data.DocumentNumar || data.DocumentTip || '(document)';
  return ruta ? <Link to={ruta}>{text}</Link> : <span>{text}</span>;
}

const BANI = { dataType: 'number', format: '#,##0.00', alignment: 'right', width: 130 } as const;
