import { useMemo } from 'react';
import { Column, Lookup } from 'devextreme-react/data-grid';
import { storeOData } from '../../nucleu/odata';
import { GrilaPolitica } from './GrilaPolitica';
import { afisareNav, captionPolitica, etichetaTipTva, optiuniEnum } from './comune';

const TIP = 'MapareTvaSaft';
const cap = (m: string) => captionPolitica(TIP, m);

export function MapariTvaSaft() {
  const tipuri = useMemo(() => ({ store: storeOData('TipTva'), sort: 'Cod' }), []);
  const enumuri = useMemo(() => ({ Sectiune: optiuniEnum('SectiuneTvaSaft'),
    Regim: optiuniEnum('RegimTva'), Sens: optiuniEnum('SensTva'),
    Rol: optiuniEnum('RolTva').filter(o => o.valoare !== 'Baza') }), []);
  return (
    <GrilaPolitica titlu="Mapări TVA SAF-T" entitate={TIP} expand={['TipTva']}
      laRandNou={r => { r.Sectiune = 'Facturi'; r.Sens = 'Achizitie'; r.Regim = 'Normal'; r.Rol = 'Taxa'; }}
      indiciu="Codificarea folosește versiunea exportului și calificarea istorică. Autocolectarea se declară numai în GeneralLedger.">
      <Column dataField="Versiune" caption={cap('Versiune')} width={160} />
      <Column dataField="TipTvaId" caption="Tip TVA" width={240}
        calculateDisplayValue={afisareNav('TipTva', etichetaTipTva)} calculateSortValue="TipTva.Cod">
        <Lookup dataSource={tipuri} valueExpr="ID" displayExpr={etichetaTipTva} />
      </Column>
      {(Object.keys(enumuri) as (keyof typeof enumuri)[]).map(c => (
        <Column key={c} dataField={c} caption={cap(c)} width={150}>
          <Lookup dataSource={enumuri[c]} valueExpr="valoare" displayExpr="label" />
        </Column>
      ))}
      <Column dataField="Cota" caption={cap('Cota')} dataType="number" width={80} />
      <Column dataField="DeImport" caption={cap('DeImport')} dataType="boolean" width={85} />
      <Column dataField="TaxType" caption={cap('TaxType')} width={100} />
      <Column dataField="TaxCode" caption={cap('TaxCode')} width={110} />
    </GrilaPolitica>
  );
}
