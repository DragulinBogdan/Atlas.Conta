import { useMemo } from 'react';
import { Column, Lookup } from 'devextreme-react/data-grid';
import { storeOData } from '../../nucleu/odata';
import { GrilaPolitica } from './GrilaPolitica';
import { afisareNav, captionPolitica, optiuniEnum, simbolSiDenumire } from './comune';

// Tipurile de TVA — cotă × regim, cu conturile ca DATE (36a) și codurile SAF-T
// direcționale (36c).
//
// `Activ` e editabil, dar nu liber: gardianul refuză dezactivarea unui tip care
// e implicit undeva (ancora unui tip de document, un rând de `PoliticaTvaImplicit`,
// un partener sau un produs), cu lista referințelor în mesaj — altfel rezolvarea
// ar fi sărit treapta în tăcere. Un tip inactiv rămâne legitim pe liniile
// EXISTENTE: e istorie, nu eroare.

const TIP = 'TipTva';
const cap = (m: string) => captionPolitica(TIP, m);

export function TipuriTva() {
  // Planul de conturi rămâne ReadOnly pe OData (F23-D5) — aici e doar sursa
  // lookup-ului. `$select` îngustează încărcarea la ce afișează editorul.
  const conturi = useMemo(
    () => ({ store: storeOData('Cont'), sort: 'Simbol', select: ['ID', 'Simbol', 'Denumire'] }), []);
  const regimuri = useMemo(() => optiuniEnum('RegimTva'), []);

  return (
    <GrilaPolitica
      titlu="Tipuri de TVA"
      entitate={TIP}
      expand={['ContTvaDeductibil', 'ContTvaColectat', 'ContTvaNeexigibil']}
      indiciu={(
        <>
          Cota și regimul sunt cele două jumătăți ale unui tip: regimul îl aduce partenerul,
          cota o aduce produsul. Codurile SAF-T sunt direcționale (unul pe livrare, altul pe
          achiziție). Dezactivarea unui tip folosit ca implicit e refuzată cu lista locurilor
          care îl referă.
        </>
      )}
    >
      <Column dataField="Cod" caption={cap('Cod')} width={110} defaultSortOrder="asc" />
      <Column dataField="Denumire" caption={cap('Denumire')} />
      <Column dataField="Cota" caption={cap('Cota')} dataType="number" format="#0.## '%'" width={100} />
      <Column dataField="Regim" caption={cap('Regim')} width={150}>
        <Lookup dataSource={regimuri} valueExpr="valoare" displayExpr="label" />
      </Column>
      <Column dataField="Activ" caption={cap('Activ')} dataType="boolean" width={90} />
      {/* Felia 25: singura cale prin care motorul și ecranul declarației vamale
          spun „tip de import" fără să cunoască vreun cod (29). */}
      <Column dataField="DeImport" caption={cap('DeImport')} dataType="boolean" width={100} />
      <Column
        dataField="ContTvaDeductibilId"
        caption={cap('ContTvaDeductibilId')}
        calculateDisplayValue={afisareNav('ContTvaDeductibil', simbolSiDenumire)}
        calculateSortValue="ContTvaDeductibil.Simbol"
        width={180}
      >
        <Lookup dataSource={conturi} valueExpr="ID" displayExpr={simbolSiDenumire} allowClearing />
      </Column>
      <Column
        dataField="ContTvaColectatId"
        caption={cap('ContTvaColectatId')}
        calculateDisplayValue={afisareNav('ContTvaColectat', simbolSiDenumire)}
        calculateSortValue="ContTvaColectat.Simbol"
        width={180}
      >
        <Lookup dataSource={conturi} valueExpr="ID" displayExpr={simbolSiDenumire} allowClearing />
      </Column>
      <Column
        dataField="ContTvaNeexigibilId"
        caption={cap('ContTvaNeexigibilId')}
        calculateDisplayValue={afisareNav('ContTvaNeexigibil', simbolSiDenumire)}
        calculateSortValue="ContTvaNeexigibil.Simbol"
        width={180}
      >
        <Lookup dataSource={conturi} valueExpr="ID" displayExpr={simbolSiDenumire} allowClearing />
      </Column>
      <Column dataField="CodSafTLivrare" caption="Cod SAF-T livrare" width={120} />
      <Column dataField="CodSafTAchizitie" caption="Cod SAF-T achiziție" width={130} />
    </GrilaPolitica>
  );
}
