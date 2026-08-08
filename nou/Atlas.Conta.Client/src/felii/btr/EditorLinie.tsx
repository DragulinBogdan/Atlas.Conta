import { useState } from 'react';
import { Formular, eroriStructurale } from '../../nucleu/formular';
import { CampNumar } from '../../nucleu/campuri';
import { Lookup } from '../../nucleu/Lookup';
import { PanouErori } from '../../nucleu/PanouErori';
import { SCHEMA_LINIE, TIP_LINIE, type BtrLinieWrite } from './api';

// Liniile de draft se editează cu VOCABULARUL `Camp*`, nu în grilă (43c):
// grila doar afișează. Editorul nu vorbește cu serverul — CRUD-ul per linie a
// fost respins în designul API (agregatul se trimite întreg), iar componentele
// nu-l reintroduc pe ușa din dos.

const CAMPURI: (keyof BtrLinieWrite & string)[] = ['TipMaterialId', 'LotId', 'Cantitate'];

// `Lot` e singurul lookup unde `DefaultProperty` nu ajunge pe sârmă: `Eticheta`
// e `[NotMapped]`, deci OData nu o expune. Compunem eticheta din `$expand=Produs`
// — aceleași trei atribute ca `Lot.Eticheta` în model (produs · dată · preț).
function etichetaLot(element: Record<string, unknown>): string {
  if (!element) return '';
  const produs = element.Produs as { Denumire?: string; Cod?: string } | undefined;
  const data = String(element.Data ?? '').slice(0, 10);
  const pret = Number(element.PretUnitar ?? 0).toFixed(2);
  return `${produs?.Denumire ?? produs?.Cod ?? '?'} · ${data} · ${pret}`;
}

export function EditorLinie(props: {
  linie: BtrLinieWrite;
  readOnly: boolean;
  onSalveaza: (l: BtrLinieWrite) => void;
  onRenunta: () => void;
}) {
  const { readOnly, onSalveaza, onRenunta } = props;
  const [linie, setLinie] = useState<BtrLinieWrite>(props.linie);
  const [aratErori, setAratErori] = useState(false);
  const structurale = eroriStructurale(TIP_LINIE, SCHEMA_LINIE, linie as Record<string, unknown>, CAMPURI);

  function confirma() {
    setAratErori(true);
    if (structurale.length === 0)
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
          <Lookup<BtrLinieWrite> camp="TipMaterialId" entitate="TipMaterial" mod="local" cauta={['Cod', 'Denumire']} />
          <Lookup<BtrLinieWrite>
            camp="LotId"
            entitate="Lot"
            mod="remote"
            expand={['Produs']}
            afisare={etichetaLot}
            cauta="Produs.Denumire"
            sortare="Data"
          />
          <CampNumar<BtrLinieWrite> camp="Cantitate" />
        </div>
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
