import { useMemo } from 'react';
import { Column, Lookup } from 'devextreme-react/data-grid';
import { storeOData } from '../../nucleu/odata';
import { GrilaPolitica } from './GrilaPolitica';
import { afisareNav, codSiDenumire, editorCautare, optiuniEnum, simbolSiDenumire } from './comune';

export function PoliticiDiferente() {
  const tipuri = useMemo(() => ({ store: storeOData('TipDocument'), sort: 'Cod' }), []);
  const clase = useMemo(() => ({ store: storeOData('ClasaProdus'), sort: 'Cod' }), []);
  const conturi = useMemo(() => ({ store: storeOData('Cont'), sort: 'Simbol', paginate: true, pageSize: 50 }), []);
  const cauze = useMemo(() => optiuniEnum('CauzaDiferentei'), []);
  return (
    <GrilaPolitica titlu="Diferențe la recepție" entitate="PoliticaDiferenta"
      expand={['TipDocument', 'Clasa', 'Cont', 'ContPersonal']}
      indiciu="Contul diferenței se alege după tipul documentului, cauză și clasa materialului. La imputarea unui angajat se folosește contul pentru personal, dacă este completat. Contul imputării trebuie să urmărească partide.">
      <Column dataField="TipDocumentId" caption="Tip document" width={180}
        calculateDisplayValue={afisareNav('TipDocument', codSiDenumire)} calculateSortValue="TipDocument.Cod" defaultSortOrder="asc">
        <Lookup dataSource={tipuri} valueExpr="ID" displayExpr={codSiDenumire} />
      </Column>
      <Column dataField="Cauza" caption="Cauza diferenței" width={180}>
        <Lookup dataSource={cauze} valueExpr="valoare" displayExpr="label" />
      </Column>
      <Column dataField="ClasaId" caption="Clasă" width={210}
        calculateDisplayValue={afisareNav('Clasa', codSiDenumire)} calculateSortValue="Clasa.Cod">
        <Lookup dataSource={clase} valueExpr="ID" displayExpr={codSiDenumire} />
      </Column>
      <Column dataField="ContId" caption="Cont" width={260} editorOptions={editorCautare}
        calculateDisplayValue={afisareNav('Cont', simbolSiDenumire)} calculateSortValue="Cont.Simbol">
        <Lookup dataSource={conturi} valueExpr="ID" displayExpr={simbolSiDenumire} />
      </Column>
      <Column dataField="ContPersonalId" caption="Cont personal" width={260} editorOptions={editorCautare}
        calculateDisplayValue={afisareNav('ContPersonal', simbolSiDenumire)} calculateSortValue="ContPersonal.Simbol">
        <Lookup dataSource={conturi} valueExpr="ID" displayExpr={simbolSiDenumire} allowClearing />
      </Column>
    </GrilaPolitica>
  );
}
