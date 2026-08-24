import { useMemo } from 'react';
import {
  Column, ColumnFixing, DataGrid, FilterRow, HeaderFilter, Pager, Paging, Sorting,
  Summary, TotalItem,
} from 'devextreme-react/data-grid';
import type { components } from '../../generated/api-types';
import { labelEnum } from '../../nucleu/campMeta';
import { storeRemote } from '../../nucleu/dxStore';
import { urlCu, useUrlStare } from '../../nucleu/urlStare';
import { CasetaData, lunaCurenta } from '../raportare/comune';

// Scheletul D300 (JT-D7/JT-D9): cifrele perioadei per tip de TVA, cu codurile
// SAF-T atașate — pe ambele sensuri deodată.
//
// Ce NU e, spus și în ecran: o declarație. Generarea D300/D394/SAF-T (XML,
// validări ANAF, versionare per an fiscal) e proiect izolat (35c); ce garantează
// felia e că FAPTELE ei stau într-un registru, cu codurile ANAF legate de
// nomenclator. Un ecran care s-ar prezenta drept „decont gata de depus" ar fi
// singura minciună posibilă aici.
type DecontTvaRand = components['schemas']['DecontTvaRand'];
const camp = (n: keyof DecontTvaRand & string) => n;

export function DecontTva() {
  const luna = lunaCurenta();
  // Ca la jurnale: perioada e opțională pe server (filtru simplu), dar ecranul îi
  // dă implicitul lunii curente — un decont se citește pe perioadă fiscală.
  const [stare, seteaza] = useUrlStare({ dataStart: luna.start, dataEnd: luna.sfarsit });

  const sursa = useMemo(() => storeRemote(
    urlCu('/api/proiectii/decont-tva', stare),
    // Cheia grilei = cheia de GRUPARE a proiecției, toată: (Sens × TipTva ×
    // Regim × Cota). Nu doar `TipTvaId`, fiindcă snapshot-urile chiar pot
    // despărți grupul — dacă `Cota` (sau `Regim`) a unui `TipTva` a fost editată
    // între două luni ale perioadei, rândurile vechi poartă valorile vechi, iar
    // proiecția le ține SEPARATE (exact ce cere D300, ale cărui linii sunt per
    // cotă). Cu o cheie mai îngustă, grila ar considera „același rând" două
    // rânduri pe care serverul le-a despărțit deliberat.
    ['Sens', 'TipTvaId', 'Regim', 'Cota'],
  ), [stare]);

  return (
    <div className="ecran">
      <div className="ecran__bara"><h2>Decont TVA (schelet D300)</h2></div>

      <div className="bara-raport">
        <CasetaData eticheta="De la" valoare={stare.dataStart} seteaza={(v) => seteaza({ dataStart: v })} />
        <CasetaData eticheta="Până la" valoare={stare.dataEnd} seteaza={(v) => seteaza({ dataEnd: v })} />
      </div>

      <DataGrid
        key={`${stare.dataStart}|${stare.dataEnd}`}
        dataSource={sursa}
        // `summary` remote (peste toate rândurile filtrate, nu peste pagină);
        // `grouping` pentru lista de valori a `HeaderFilter`, ca la jurnale.
        remoteOperations={{ filtering: true, sorting: true, paging: true, summary: true, grouping: true }}
        showBorders
        columnAutoWidth
        height="calc(100vh - 220px)"
      >
        <Sorting mode="multiple" />
        <FilterRow visible />
        <HeaderFilter visible />
        <ColumnFixing enabled />
        <Paging defaultPageSize={50} />
        <Pager showInfo showPageSizeSelector allowedPageSizes={[50, 100, 200]} />

        {/* `Sens` vine pe sârmă ca STRING (numele membrului), ca peste tot în
            proiecții — eticheta trece prin `labelEnum`, aceeași sursă ca XAF. */}
        <Column dataField={camp('Sens')} caption="Sens" width={110} fixed cellRender={celulaSens} />
        {/* Cheia de raportare e `TipTva` — el poartă mapările ANAF. `SDD` (scutit
            CU drept de deducere) și `SFD` (FĂRĂ drept) au același regim și aceeași
            cotă 0, dar coduri SAF-T diferite și rânduri diferite în D300: o
            grupare pe regim×cotă le-ar fi fuzionat, adică ar fi produs exact cifra
            pe care declarația n-o poate folosi. */}
        <Column dataField={camp('TipTvaCod')} caption="Tip TVA" width={90} cellRender={celulaTipTva} />
        <Column dataField={camp('TipTvaDenumire')} caption="Denumire" />
        <Column dataField={camp('Regim')} caption="Regim" width={130} cellRender={celulaRegim} />
        <Column dataField={camp('Cota')} caption="Cotă %" dataType="number" format="#0.##" alignment="right" width={80} />
        <Column dataField={camp('CodSafT')} caption="Cod SAF-T" width={110} />
        {/* Câte rânduri de REGISTRU (linii de document) stau în spatele cifrei —
            urma spre granularitatea SAF-T, nu o cifră de declarație. */}
        <Column dataField={camp('Randuri')} caption="Rânduri" dataType="number" format="#,##0" alignment="right" width={90} />

        <Column dataField={camp('Baza')} caption="Bază" {...BANI} />
        <Column dataField={camp('Tva')} caption="TVA" {...BANI} />

        {/* Aditive, deci însumabile (spre deosebire de soldurile balanței, R-D5).
            Totalul amestecă însă DELIBERAT cele două sensuri: e o cifră de
            control peste ce s-a filtrat, nu „TVA de plată" — diferența dintre
            colectat și deductibil e treaba închiderii lunare (ITV), care e un
            document, nu o scădere făcută în grilă. Spus și în indiciul de jos. */}
        <Summary>
          <TotalItem column={camp('Baza')} summaryType="sum" valueFormat="#,##0.00" displayFormat="Σ {0}" />
          <TotalItem column={camp('Tva')} summaryType="sum" valueFormat="#,##0.00" displayFormat="Σ {0}" />
          <TotalItem column={camp('Randuri')} summaryType="sum" valueFormat="#,##0" displayFormat="Σ {0}" />
        </Summary>
      </DataGrid>

      <p className="indiciu">
        <strong>Schelet D300</strong>, nu declarație: cifrele perioadei per tip de TVA, cu codurile SAF-T
        atașate din nomenclator. Generarea fișierului (XML, validări ANAF, versionare per an) e altă unealtă.
        Totalul de jos însumează ambele sensuri — TVA-ul de plată sau de recuperat rezultă din închiderea
        lunară (documentul ITV), nu dintr-o scădere făcută aici.
      </p>
    </div>
  );
}

function celulaSens({ data }: { data: DecontTvaRand }) {
  return <span>{labelEnum('SensTva', data.Sens)}</span>;
}

function celulaRegim({ data }: { data: DecontTvaRand }) {
  return <span>{labelEnum('RegimTva', data.Regim)}</span>;
}

// `TipTva` e nomenclator editabil, deci ștergibil: join-ul e LEFT, iar cifra
// rămâne în raport chiar dacă eticheta a dispărut (lecția review-ului D4 al
// feliei 9). Se marchează, nu se afișează gol.
function celulaTipTva({ data }: { data: DecontTvaRand }) {
  if (data.TipTvaCod) return <span>{data.TipTvaCod}</span>;
  return <span className="indiciu" title={data.TipTvaId ?? ''}>(tip TVA indisponibil)</span>;
}

const BANI = { dataType: 'number', format: '#,##0.00', alignment: 'right', width: 150 } as const;
