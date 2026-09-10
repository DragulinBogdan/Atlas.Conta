import { useMemo } from 'react';
import { Column, Lookup } from 'devextreme-react/data-grid';
import { storeOData } from '../../nucleu/odata';
import { GrilaPolitica, type GrupFormular } from './GrilaPolitica';
import {
  afisareNav, captionPolitica, codNav, codSiDenumire, editorCautare, optiuniEnum, simbolSiDenumire, urlExplica,
} from './comune';

// Maparea contabilă (26b/26c): tip de document × treaptă de potrivire → conturi
// per latură + dimensiuni. Ecranul e singurul din familie care editează în
// POPUP: cele 24 de FK-uri de dimensiuni (trei seturi × opt componente, decizia
// 15) n-au loc pe un rând de grilă, dar sunt tot atât de configurabile ca
// restul.
//
// Rândul nou pornește cu ambele surse pe „Explicit” VIZIBIL (81-r8): e valoarea
// 0 a enum-ului, deci rândul pleca oricum așa, doar că fără contul pe care
// „Explicit” îl cere. Refuzul rămâne al gardianului — ecranul propune, nu
// validează.

const TIP = 'RegulaContare';
const cap = (m: string) => captionPolitica(TIP, m);

// Componentele unei dimensiuni, în ordinea din `Dimensiuni` (15). `entitate` e
// nomenclatorul-țintă pe ușa OData; `Unitate` are tip în EDM dar NU are
// controller (`GET api/odata/Unitate` ⇒ 404), deci cele trei câmpuri ale ei
// rămân de citit — un lookup ar fi cerut o ușă OData nouă, iar felia nu deschide
// niciuna (76a).
const DIMENSIUNI: { sufix: string; entitate: string | null }[] = [
  { sufix: 'Repartitor', entitate: 'Repartitor' },
  { sufix: 'Material', entitate: 'Produs' },
  { sufix: 'CodFunctional', entitate: 'CodFunctional' },
  { sufix: 'CodEconomic', entitate: 'CodEconomic' },
  { sufix: 'SursaFinantare', entitate: 'SursaFinantare' },
  { sufix: 'Unitate', entitate: null },
  { sufix: 'Proiect', entitate: 'Proiect' },
  { sufix: 'CentruCost', entitate: 'Repartitor' },
];

const SETURI = [
  { prefix: 'Comun', titlu: 'Dimensiuni comune' },
  { prefix: 'OverrideDebit', titlu: 'Override debit' },
  { prefix: 'OverrideCredit', titlu: 'Override credit' },
];

const GRUPURI: GrupFormular[] = [
  { titlu: 'Potrivire', campuri: ['TipDocumentId', 'TipMaterialId', 'NaturaFiltru', 'SemnFiltru', 'PastreazaSemn'] },
  { titlu: 'Conturi', campuri: ['SursaContDebit', 'ContDebitId', 'SursaContCredit', 'ContCreditId'] },
  ...SETURI.map((s) => ({
    titlu: s.titlu,
    campuri: DIMENSIUNI.map((d) => `${s.prefix}${d.sufix}Id`),
  })),
];

// Doar navigațiile pe care le AFIȘEAZĂ grila sau formularul fără lookup: pentru
// celelalte 21 de dimensiuni eticheta o rezolvă lookup-ul prin `byKey`, adică
// prin cache-ul comun (77b), nu prin lărgirea fiecărui rând.
const EXPAND = [
  'TipDocument', 'TipMaterial', 'ContDebit', 'ContCredit',
  ...SETURI.map((s) => `${s.prefix}Unitate`),
];

export function ReguliContare() {
  const tipuriDocument = useMemo(() => ({ store: storeOData('TipDocument'), sort: 'Cod' }), []);
  const tipuriMaterial = useMemo(() => ({ store: storeOData('TipMaterial'), sort: 'Cod' }), []);
  const conturi = useMemo(
    () => ({ store: storeOData('Cont'), sort: 'Simbol', paginate: true, pageSize: 50 }), []);
  const naturi = useMemo(() => optiuniEnum('NaturaClasa', 'orice natură'), []);
  const surse = useMemo(() => optiuniEnum('SursaCont'), []);
  // Câte un store per nomenclator-țintă, nu per câmp: cele 21 de lookup-uri de
  // dimensiune cad pe cinci nomenclatoare.
  const storeDimensiuni = useMemo(() => new Map(
    [...new Set(DIMENSIUNI.map((d) => d.entitate).filter((e): e is string => e != null))]
      .map((e) => [e, { store: storeOData(e), sort: 'Cod' }]),
  ), []);

  return (
    <GrilaPolitica
      titlu="Reguli de contare"
      entitate={TIP}
      explica={(r) => urlExplica(codNav(r, 'TipDocument'), { tipMaterial: r.TipMaterialId, semn: r.SemnFiltru })}
      expand={EXPAND}
      formular={GRUPURI}
      laRandNou={(rand) => {
        rand.SursaContDebit = 'Explicit';
        rand.SursaContCredit = 'Explicit';
      }}
      indiciu={(
        <>
          Treptele de potrivire sunt ALTERNATIVE: tipul de material exact bate filtrul de natură, care
          bate regula generică (fără niciunul); o regulă cu amândouă e refuzată. Filtrul de semn scoate
          regula din joc înaintea treptelor. Fără regulă potrivită, linia nu contează pe acest tip de
          document — așa se împarte lanțul factură/NIR fără dublă postare. Dimensiunile se editează pe
          formular, prin butonul de editare al rândului.
        </>
      )}
    >
      <Column
        dataField="TipDocumentId"
        caption={cap('TipDocumentId')}
        calculateDisplayValue={afisareNav('TipDocument', codSiDenumire)}
        calculateSortValue="TipDocument.Cod"
        defaultSortOrder="asc"
        width={160}
      >
        <Lookup dataSource={tipuriDocument} valueExpr="ID" displayExpr={codSiDenumire} />
      </Column>
      <Column
        dataField="TipMaterialId"
        caption="Tip material"
        calculateDisplayValue={afisareNav('TipMaterial', codSiDenumire)}
        calculateSortValue="TipMaterial.Cod"
        editorOptions={editorCautare}
        width={220}
      >
        <Lookup dataSource={tipuriMaterial} valueExpr="ID" displayExpr={codSiDenumire} allowClearing />
      </Column>
      <Column dataField="NaturaFiltru" caption="Filtru de natură" width={170}>
        <Lookup dataSource={naturi} valueExpr="valoare" displayExpr="label" />
      </Column>
      <Column dataField="SemnFiltru" caption="Filtru de semn" dataType="number" width={130} />
      <Column dataField="PastreazaSemn" caption="Păstrează semnul" dataType="boolean" width={150} />
      <Column dataField="SursaContDebit" caption="Sursa contului debitor" width={180}>
        <Lookup dataSource={surse} valueExpr="valoare" displayExpr="label" />
      </Column>
      <Column
        dataField="ContDebitId"
        caption="Cont debitor"
        calculateDisplayValue={afisareNav('ContDebit', simbolSiDenumire)}
        calculateSortValue="ContDebit.Simbol"
        editorOptions={editorCautare}
        width={220}
      >
        <Lookup dataSource={conturi} valueExpr="ID" displayExpr={simbolSiDenumire} allowClearing />
      </Column>
      <Column dataField="SursaContCredit" caption="Sursa contului creditor" width={180}>
        <Lookup dataSource={surse} valueExpr="valoare" displayExpr="label" />
      </Column>
      <Column
        dataField="ContCreditId"
        caption="Cont creditor"
        calculateDisplayValue={afisareNav('ContCredit', simbolSiDenumire)}
        calculateSortValue="ContCredit.Simbol"
        editorOptions={editorCautare}
        width={220}
      >
        <Lookup dataSource={conturi} valueExpr="ID" displayExpr={simbolSiDenumire} allowClearing />
      </Column>

      {/* Dimensiunile: în grilă ascunse, pe formular editabile — `visible={false}`
          nu le scoate din formular cât timp `editing.form.items` le listează. */}
      {SETURI.flatMap((s) => DIMENSIUNI.map((d) => {
        const camp = `${s.prefix}${d.sufix}`;
        const sursa = d.entitate == null ? undefined : storeDimensiuni.get(d.entitate);
        return (
          <Column
            key={camp}
            dataField={`${camp}Id`}
            caption={cap(`${camp}Id`)}
            calculateDisplayValue={sursa ? undefined : afisareNav(camp, codSiDenumire)}
            allowEditing={sursa != null}
            editorOptions={sursa && editorCautare}
            visible={false}
          >
            {sursa && <Lookup dataSource={sursa} valueExpr="ID" displayExpr={codSiDenumire} allowClearing />}
          </Column>
        );
      }))}
    </GrilaPolitica>
  );
}
