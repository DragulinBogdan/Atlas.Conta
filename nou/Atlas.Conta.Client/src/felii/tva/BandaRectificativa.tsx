import { Column, DataGrid, Paging, Sorting } from 'devextreme-react/data-grid';
import type { components } from '../../generated/api-types';
import { labelEnum } from '../../nucleu/campMeta';

type DecontTvaRand = components['schemas']['DecontTvaRand'];

const camp = (n: keyof DecontTvaRand & string) => n;
const BANI = { dataType: 'number', format: '#,##0.00', alignment: 'right', width: 140 } as const;

export function BandaRectificativa(
  { rectificativa, perioadaDeschisa, diferente }:
  { rectificativa?: boolean; perioadaDeschisa?: boolean; diferente?: DecontTvaRand[] | null },
) {
  if (!rectificativa) return null;
  const randuri = diferente ?? [];
  return (
    <div className="d300__neincluse">
      <h3>RECTIFICATIVĂ — diferențe față de declarat</h3>
      <p className="indiciu">
        Sunt afișate faptele înregistrate după ultima confirmare explicită a depunerii D394.
        Declarația înlocuitoare păstrează perioada inițială și identitatea facturii corectate.
      </p>
      {perioadaDeschisa && <p className="indiciu">Perioada contabilă este deschisă.</p>}
      <DataGrid
        dataSource={randuri}
        showBorders
        columnAutoWidth
        height={Math.min(60 + randuri.length * 34, 300)}
      >
        <Sorting mode="none" />
        <Paging enabled={false} />
        <Column dataField={camp('Sens')} caption="Sens" width={110} cellRender={celulaSens} />
        <Column dataField={camp('TipTvaCod')} caption="Tip TVA" width={90} />
        <Column dataField={camp('TipTvaDenumire')} caption="Denumire" />
        <Column dataField={camp('Regim')} caption="Regim" width={130} cellRender={celulaRegim} />
        <Column dataField={camp('Cota')} caption="Cotă %" dataType="number" format="#0.##" alignment="right" width={80} />
        <Column dataField={camp('Baza')} caption="Bază" {...BANI} />
        <Column dataField={camp('Tva')} caption="TVA" {...BANI} />
        <Column dataField={camp('Randuri')} caption="Rânduri" dataType="number" format="#,##0" alignment="right" width={90} />
      </DataGrid>
    </div>
  );
}

function celulaSens({ data }: { data: DecontTvaRand }) {
  return <span>{labelEnum('SensTva', data.Sens)}</span>;
}

function celulaRegim({ data }: { data: DecontTvaRand }) {
  return <span>{labelEnum('RegimTva', data.Regim)}</span>;
}
