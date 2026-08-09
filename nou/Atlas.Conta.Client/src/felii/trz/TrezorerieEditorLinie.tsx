import { useEffect, useState } from 'react';
import { Formular, eroriStructurale } from '../../nucleu/formular';
import { CampNumar } from '../../nucleu/campuri';
import { Lookup } from '../../nucleu/Lookup';
import { PanouErori } from '../../nucleu/PanouErori';
import { SCHEMA_LINIE, TIP_LINIE, type TrzLinieWrite } from './api';
import { idTipTrz } from './tipTrz';

// Editorul de linie al trezoreriei — vocabularul `Camp*`, ca la BTR/FCT (43c);
// grila doar afișează. Ce e propriu aici:
//
//  1. **Linia e DEFALCAREA sumei** (31a): `Valoare` se CULEGE (nu există
//     `PregatesteOperare` pe trezorerie), iar cantitatea/lotul/TVA-ul n-au
//     semantică și nici nu sunt în contract.
//  2. **`TRZ` precompletat pe linia NOUĂ** (F3-D7): default de culegere, nu
//     validare — lookup-ul rămâne NEFILTRAT, fiindcă liniile clonate din
//     factură poartă Tipul ei (302/628…) și trebuie să rămână corectabile.
const CAMPURI: (keyof TrzLinieWrite & string)[] = ['TipMaterialId', 'Valoare'];

export function TrezorerieEditorLinie(props: {
  linie: TrzLinieWrite;
  readOnly: boolean;
  onSalveaza: (l: TrzLinieWrite) => void;
  onRenunta: () => void;
}) {
  const { readOnly, onSalveaza, onRenunta } = props;
  const [linie, setLinie] = useState<TrzLinieWrite>(props.linie);
  const [aratErori, setAratErori] = useState(false);
  const structurale = eroriStructurale(TIP_LINIE, SCHEMA_LINIE, linie as Record<string, unknown>, CAMPURI);
  const linieNoua = props.linie.Id == null;

  // Precompletarea Tipului tehnic, o dată, la MONTAREA unei linii noi. Update
  // FUNCȚIONAL și numai peste gol: dacă între timp operatorul a ales altceva
  // (răspunsul OData vine asincron), alegerea lui rămâne.
  useEffect(() => {
    if (!linieNoua || readOnly || props.linie.TipMaterialId != null) return;
    let activ = true;
    void idTipTrz().then((id) => {
      if (activ && id)
        setLinie((prev) => (prev.TipMaterialId ? prev : { ...prev, TipMaterialId: id }));
    });
    return () => { activ = false; };
  }, [linieNoua, readOnly, props.linie.TipMaterialId]);

  function confirma() {
    setAratErori(true);
    if (structurale.length > 0)
      return;
    onSalveaza(linie);
  }

  return (
    <div className="editor-linie">
      <Formular
        tip={TIP_LINIE}
        schema={SCHEMA_LINIE}
        valoare={linie}
        onSchimba={setLinie}
        readOnly={readOnly}
        aratErori={aratErori}
      >
        <div className="grila-campuri">
          <Lookup<TrzLinieWrite>
            camp="TipMaterialId"
            entitate="TipMaterial"
            mod="local"
            afisare={codSiDenumire}
            cauta={['Cod', 'Denumire']}
          />
          <div>
            <CampNumar<TrzLinieWrite> camp="Valoare" zecimale={2} />
            <p className="indiciu">Suma defalcată pe această poziție; motorul refuză la operare liniile pe 0.</p>
          </div>
        </div>

        {/* Clasificația bugetară e a PROFILULUI (54d): la privat nomenclatoarele
            sunt goale și secțiunea rămâne pliată. Obligativitatea (`PoliticaValidare`
            — PLT cere clasificație la bugetar) e a serverului, nu a ecranului. */}
        <details className="sectiune-pliabila">
          <summary>Clasificație bugetară</summary>
          <div className="grila-campuri">
            <Lookup<TrzLinieWrite> camp="CodEconomicId" entitate="CodEconomic" mod="local" afisare={codSiDenumire} cauta={['Cod', 'Denumire']} />
            <Lookup<TrzLinieWrite> camp="SursaFinantareId" entitate="SursaFinantare" mod="local" afisare={codSiDenumire} cauta={['Cod', 'Denumire']} />
            <Lookup<TrzLinieWrite> camp="CodFunctionalId" entitate="CodFunctional" mod="local" afisare={codSiDenumire} cauta={['Cod', 'Denumire']} />
            <Lookup<TrzLinieWrite> camp="ProiectId" entitate="Proiect" mod="local" afisare={codSiDenumire} cauta={['Cod', 'Denumire']} />
          </div>
        </details>
      </Formular>

      {aratErori && <PanouErori erori={structurale} titlu="Completați linia" />}

      <div className="editor-linie__comenzi">
        <button type="button" className="buton buton--primar" disabled={readOnly} onClick={confirma}>
          {props.linie.Id ? 'Actualizează linia' : 'Adaugă linia'}
        </button>
        <button type="button" className="buton" onClick={onRenunta}>Renunță</button>
      </div>
    </div>
  );
}

// Nomenclatoarele au `DefaultProperty = Denumire`, dar operatorul caută pe cod —
// afișăm amândouă (același ajutor ca în editorul FCT).
function codSiDenumire(element: Record<string, unknown>): string {
  if (!element) return '';
  const cod = element.Cod == null ? '' : String(element.Cod);
  const denumire = element.Denumire == null ? '' : String(element.Denumire);
  return cod && denumire ? `${cod} — ${denumire}` : cod || denumire;
}
