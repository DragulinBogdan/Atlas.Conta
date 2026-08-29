import { useMemo } from 'react';
import { Column, DataGrid, Pager, Paging, Sorting } from 'devextreme-react/data-grid';
import DataSource from 'devextreme/data/data_source';
import { storeOData } from '../../nucleu/odata';
import { campMeta, labelEnum, nomenclator } from '../../nucleu/campMeta';

// Politica de mișcare SAF-T (74-r12) — GRILĂ DE CITIRE, deliberat.
//
// Regula 56 („politicile = ReadOnly pe OData") nu se redeschide aici: politica
// decide ce cod de mișcare capătă fiecare `(TipDocument × TipStoc × Semn)` în
// fișierul `C`, iar `cod NULL` e o EXCLUDERE cu motiv — o valoare cu consecințe
// în declarație. Editarea ei din React ar fi fost politică schimbată pe ușa din
// dos; se face în BackOffice, unde stă și restul politicilor.
//
// Ce aduce ecranul, și de ce merită să existe fără scriere: denumirea codului.
// `CodMiscare` e „30", „70", „120" — cifre din lege. Lista lor e cod pe server
// (`SaftReguli`, 74b) și ajunge aici prin `metadata.json` (F20-D6), deci grila
// poate arăta „30 — Vânzare" fără să inventeze nimic și fără o a doua listă în
// TS care ar drifta de lege.

type CodMiscare = { Cod: string; Denumire: string };

const cap = (membru: string) => campMeta('PoliticaMiscareSaft', membru).caption;

export function PoliticiMiscareSaft() {
  const denumiri = useMemo(() => {
    const harta = new Map<string, string>();
    for (const c of nomenclator<CodMiscare>('CoduriMiscare')) harta.set(c.Cod, c.Denumire);
    return harta;
  }, []);

  const sursa = useMemo(() => new DataSource({
    store: storeOData('PoliticaMiscareSaft'),
    expand: ['TipDocument'],
    paginate: true,
    pageSize: 50,
    requireTotalCount: true,
  }), []);

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>Mișcări SAF-T (politică)</h2>
      </div>

      <DataGrid
        dataSource={sursa}
        remoteOperations
        showBorders
        columnAutoWidth
        height="calc(100vh - 190px)"
      >
        <Sorting mode="multiple" />
        <Paging defaultPageSize={50} />
        <Pager showInfo showPageSizeSelector allowedPageSizes={[25, 50, 100]} />

        <Column
          dataField="TipDocument.Cod"
          caption={cap('TipDocument')}
          allowSorting={false}
          allowFiltering={false}
          width={110}
        />
        <Column
          dataField="TipDocument.Denumire"
          caption="Document"
          allowSorting={false}
          allowFiltering={false}
        />
        <Column
          dataField="TipStoc"
          caption={cap('TipStoc')}
          width={150}
          calculateCellValue={(r: Record<string, unknown>) => labelEnum('TipStoc', r.TipStoc as string)}
        />
        {/* `Semn` null = „orice semn" (D17-D1). Gol în grilă ar fi arătat ca o
            valoare lipsă, nu ca regula care e. */}
        <Column
          dataField="Semn"
          caption={cap('Semn')}
          width={110}
          calculateCellValue={(r: Record<string, unknown>) =>
            (r.Semn == null ? 'orice' : Number(r.Semn) > 0 ? '+1 (intrare)' : '−1 (ieșire)')}
        />
        <Column
          dataField="CodMiscare"
          caption={cap('CodMiscare')}
          width={230}
          calculateCellValue={(r: Record<string, unknown>) => {
            // Codul NULL e excluderea DELIBERATĂ (74a): rândurile lui ies în
            // `Excluse`, cu motiv — altceva decât un rând fără politică, care
            // iese în `Neincluse`. Grila spune care e care.
            const cod = r.CodMiscare == null ? null : String(r.CodMiscare);
            if (!cod) return '— exclus deliberat';
            const denumire = denumiri.get(cod);
            return denumire ? `${cod} — ${denumire}` : cod;
          }}
        />
        <Column
          dataField="RolTert"
          caption={cap('RolTert')}
          width={180}
          calculateCellValue={(r: Record<string, unknown>) => labelEnum('RolTertSaft', r.RolTert as string)}
        />
        <Column dataField="Motiv" caption={cap('Motiv')} />
      </DataGrid>

      <p className="indiciu">
        Se editează în BackOffice (XAF) — politicile rămân ReadOnly pe API (decizia 56).
        Un rând cu cod gol e o excludere deliberată (rândurile lui apar în raportul SAF-T ca
        „Excluse", cu motivul de aici); un tip de document FĂRĂ rând aici apare ca „Neincluse".
      </p>
    </div>
  );
}
