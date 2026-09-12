import { useMemo } from 'react';
import { Column, Lookup } from 'devextreme-react/data-grid';
import { storeOData } from '../../nucleu/odata';
import { GrilaPolitica } from './GrilaPolitica';
import {
  afisareNav, captionPolitica, codNav, codSiDenumire, editorCautare, optiuniEnum, simbolSiDenumire, urlExplica,
} from './comune';

// Pasul de TVA al motorului (36b): un tip FĂRĂ rând aici nu postează niciun rând
// de TVA, oricâtă cotă ar purta liniile. Direcția separă deductibilul de
// colectat, iar contrapartida (401/411/542) se rezolvă declarativ, ca la regula
// de contare.
//
// Rândul nou pornește cu sursa „Explicit” VIZIBILĂ (81-r8): e valoarea 0 a
// enum-ului, deci rândul pleca oricum așa — dar fără contul pe care „Explicit”
// îl cere, și cu o celulă goală care nu explica refuzul.

const TIP = 'PoliticaTva';
const cap = (m: string) => captionPolitica(TIP, m);

export function PoliticiTva() {
  const tipuriDocument = useMemo(() => ({ store: storeOData('TipDocument'), sort: 'Cod' }), []);
  const conturi = useMemo(
    () => ({ store: storeOData('Cont'), sort: 'Simbol', paginate: true, pageSize: 50 }), []);
  const directii = useMemo(() => optiuniEnum('DirectieTva'), []);
  const surse = useMemo(() => optiuniEnum('SursaCont'), []);

  return (
    <GrilaPolitica
      titlu="TVA per tip de document"
      entitate={TIP}
      explica={(r) => urlExplica(codNav(r, 'TipDocument'))}
      expand={['TipDocument', 'ContrapartidaFallback']}
      laRandNou={(rand) => {
        rand.Directie = 'Deductibil';
        rand.SursaContrapartida = 'Explicit';
      }}
      indiciu={(
        <>
          Un tip fără rând aici nu postează TVA deloc. Contrapartida rândului de TVA se ia din sursa
          declarată — repartitorul unei laturi sau contul explicit de alături; sursa „Explicit” fără
          cont e refuzată, fiindcă sursa explicită E contul.
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
      <Column dataField="Directie" caption="Direcție" width={150}>
        <Lookup dataSource={directii} valueExpr="valoare" displayExpr="label" />
      </Column>
      <Column dataField="SursaContrapartida" caption="Sursa contrapartidei" width={200}>
        <Lookup dataSource={surse} valueExpr="valoare" displayExpr="label" />
      </Column>
      <Column
        dataField="ContrapartidaFallbackId"
        caption="Cont contrapartidă"
        calculateDisplayValue={afisareNav('ContrapartidaFallback', simbolSiDenumire)}
        calculateSortValue="ContrapartidaFallback.Simbol"
        editorOptions={editorCautare}
        width={240}
      >
        <Lookup dataSource={conturi} valueExpr="ID" displayExpr={simbolSiDenumire} allowClearing />
      </Column>
    </GrilaPolitica>
  );
}
