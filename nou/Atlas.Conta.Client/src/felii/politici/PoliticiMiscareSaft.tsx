import { useMemo } from 'react';
import { Column, Lookup } from 'devextreme-react/data-grid';
import { nomenclator } from '../../nucleu/campMeta';
import { storeOData } from '../../nucleu/odata';
import { GrilaPolitica } from './GrilaPolitica';
import { afisareNav, captionPolitica, codSiDenumire, optiuniEnum, type OptiuneLookup } from './comune';

// Politica de mișcare SAF-T (74a) — ecranul care era DOAR de citit (74-r12) și
// devine editabil odată cu deschiderea ușii OData (F23-D5). Motivul de atunci
// („politicile rămân ReadOnly pe API, decizia 56") a expirat: gardianul stă pe
// ușa COMUNĂ, deci un rând invalid nu mai poate intra pe ușa din dos.
//
// Ce s-a schimbat, în afară de scriere: `Semn` și `CodMiscare` afișau textul
// printr-un `calculateCellValue`. O coloană calculată nu se poate edita, iar
// textul e oricum o hartă valoare → etichetă — adică exact un `Lookup`. Deci
// aceleași cuvinte, dar dintr-o sursă care servește și afișarea, și editorul.
//
// Cele două valori pe care ecranul trebuie să le țină distincte:
//   • `Semn` null = „orice semn" (regula se potrivește pe ambele sensuri);
//   • `CodMiscare` null = EXCLUDERE DELIBERATĂ, cu motiv — rândurile ei ies în
//     raport ca „Excluse". Un tip de document FĂRĂ rând aici e altceva: iese ca
//     „Neincluse". Grila spune care e care.

const TIP = 'PoliticaMiscareSaft';
const cap = (m: string) => captionPolitica(TIP, m);

type CodMiscare = { Cod: string; Denumire: string };

export function PoliticiMiscareSaft() {
  const tipuriDocument = useMemo(() => ({ store: storeOData('TipDocument'), sort: 'Cod' }), []);
  const tipuriStoc = useMemo(() => optiuniEnum('TipStoc'), []);
  const roluri = useMemo(() => optiuniEnum('RolTertSaft'), []);

  // Semnele: valori întregi, cu nulul ca regulă proprie.
  const semne = useMemo<OptiuneLookup[]>(() => [
    { valoare: null, label: 'orice' },
    { valoare: 1, label: '+1 (intrare)' },
    { valoare: -1, label: '−1 (ieșire)' },
  ], []);

  // Codurile de mișcare sunt LEGE ÎN COD pe server (`SaftReguli`, 74b) și ajung
  // aici prin `metadata.json` (77f) — nu sunt entitate și n-au ușă OData.
  const coduri = useMemo<OptiuneLookup[]>(() => [
    { valoare: null, label: '— exclus deliberat' },
    ...nomenclator<CodMiscare>('CoduriMiscare').map((c) => ({ valoare: c.Cod, label: `${c.Cod} — ${c.Denumire}` })),
  ], []);

  return (
    <GrilaPolitica
      titlu="Mișcări SAF-T (politică)"
      entitate={TIP}
      expand={['TipDocument']}
      indiciu={(
        <>
          Codul de mișcare e al tipului de document × registrului de stoc × semnului. Un rând cu
          cod gol e o excludere DELIBERATĂ: rândurile lui apar în raportul SAF-T ca „Excluse", cu
          motivul de aici. Un tip de document fără rând deloc apare ca „Neincluse" — altă
          categorie, altă cauză.
        </>
      )}
    >
      <Column
        dataField="TipDocumentId"
        caption={cap('TipDocumentId')}
        calculateDisplayValue={afisareNav('TipDocument', codSiDenumire)}
        calculateSortValue="TipDocument.Cod"
        defaultSortOrder="asc"
        width={180}
      >
        <Lookup dataSource={tipuriDocument} valueExpr="ID" displayExpr={codSiDenumire} />
      </Column>
      <Column dataField="TipStoc" caption={cap('TipStoc')} width={160}>
        <Lookup dataSource={tipuriStoc} valueExpr="valoare" displayExpr="label" />
      </Column>
      <Column dataField="Semn" caption={cap('Semn')} width={130}>
        <Lookup dataSource={semne} valueExpr="valoare" displayExpr="label" />
      </Column>
      <Column dataField="CodMiscare" caption={cap('CodMiscare')} width={240}>
        <Lookup dataSource={coduri} valueExpr="valoare" displayExpr="label" />
      </Column>
      <Column dataField="RolTert" caption={cap('RolTert')} width={180}>
        <Lookup dataSource={roluri} valueExpr="valoare" displayExpr="label" />
      </Column>
      <Column dataField="Motiv" caption={cap('Motiv')} />
    </GrilaPolitica>
  );
}
