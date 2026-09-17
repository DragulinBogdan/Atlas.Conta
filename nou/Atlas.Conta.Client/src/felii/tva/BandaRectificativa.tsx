import { Column, DataGrid, Paging, Sorting } from 'devextreme-react/data-grid';
import type { components } from '../../generated/api-types';
import { labelEnum } from '../../nucleu/campMeta';

// Banda de rectificativă a unei declarații (F27-D5) — aceeași pe D300 și D394,
// fiindcă răspunde aceleiași întrebări din aceeași sursă: ce cifre au intrat în
// perioadă DUPĂ ce ea a fost declarată o dată.
//
// Nu există flag de rectificativă nicăieri în model: e diferența dintre
// `RegistruTva.ScrisLa` și `PerioadaFiscala.InchisaPrimaOara`. Banda apare doar
// când serverul spune că perioada chiar are astfel de cifre; pe un interval de
// mai multe luni întrebarea n-are subiect, iar serverul răspunde „nu".
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
        Perioada a fost închisă cel puțin o dată, iar cifrele de mai jos au intrat în registrul
        fiscal <strong>după</strong> acea închidere: declarația de sus diferă de cea depusă atunci,
        exact cu ele. Nu e un marcaj cules de nimeni — e diferența dintre momentul scrierii fiecărui
        rând și momentul primei închideri a perioadei. Fișierul de depunere marcat ca rectificativ
        rămâne altă unealtă.
      </p>
      {perioadaDeschisa && (
        <p className="indiciu">
          <strong>Perioadă redeschisă</strong> — conținutul de mai jos devine rectificativă la
          re-închidere. Reperul rămâne prima închidere, deci ce se mai scrie până atunci intră
          tot aici.
        </p>
      )}
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
