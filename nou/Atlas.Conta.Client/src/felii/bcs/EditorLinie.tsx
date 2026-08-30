import { useState } from 'react';
import { Formular, eroriStructurale } from '../../nucleu/formular';
import { CampNumar } from '../../nucleu/campuri';
import { Lookup } from '../../nucleu/Lookup';
import { LookupGrila } from '../../nucleu/LookupGrila';
import { PanouErori } from '../../nucleu/PanouErori';
import { etichetaLot } from '../../nucleu/lot';
import { precompleteazaTip } from '../../nucleu/etichete';
import { SCHEMA_LINIE, TIP_LINIE, type BcsLinieWrite } from './api';

// Liniile de draft se editează cu VOCABULARUL `Camp*`, nu în grilă (43c): grila
// doar afișează. Editorul nu vorbește cu serverul — CRUD-ul per linie a fost
// respins în designul API (agregatul se trimite întreg).
//
// Linia de consum e cea mai simplă din tot clientul: TIPUL (contul/clasa), LOTUL
// descărcat și cantitatea. Nu naște nimic (spre deosebire de NIR/LDI+) și nu
// culege niciun preț — valoarea o materializează serverul din prețul LOTULUI, la
// culegere și încă o dată la operare (F6-D6).
const CAMPURI: (keyof BcsLinieWrite & string)[] = ['TipMaterialId', 'LotId', 'Cantitate'];

// Etichetele CULESE la selecție, pentru grila documentului (mecanismul 61b):
// liniile nesalvate n-au încă ReadDto, dar răspunsul OData al selecției e DEJA
// în client — nu se inventează nimic în TS, se reține ce a afișat lookup-ul.
// `Valoare` rămâne a serverului (apare după Salvează).
export type EticheteCulese = {
  TipMaterialDenumire?: string;
  LotEticheta?: string;
};

const text = (v: unknown) => (v == null ? '' : String(v));

export function EditorLinie(props: {
  linie: BcsLinieWrite;
  readOnly: boolean;
  onSalveaza: (l: BcsLinieWrite, etichete: EticheteCulese) => void;
  onRenunta: () => void;
}) {
  const { readOnly, onSalveaza, onRenunta } = props;
  const [linie, setLinie] = useState<BcsLinieWrite>(() => ({ ...props.linie }));
  const [etichete, setEtichete] = useState<EticheteCulese>({});
  const [aratErori, setAratErori] = useState(false);
  const structurale = eroriStructurale(TIP_LINIE, SCHEMA_LINIE, linie as Record<string, unknown>, CAMPURI);

  function confirma() {
    setAratErori(true);
    if (structurale.length === 0)
      onSalveaza(linie, etichete);
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
          <Lookup<BcsLinieWrite>
            camp="TipMaterialId"
            entitate="TipMaterial"
            mod="local"
            laSelectie={(t) => setEtichete((prev) => ({ ...prev, TipMaterialDenumire: text(t?.Denumire) }))}
          />
          <div>
            {/* Lotul NU se filtrează pe gestiunea predatoare (F4-D6, reconfirmat
                F6-D8): locația curentă a unui lot e soldul din registru, nu
                nașterea lui — un lot transferat trebuie să rămână consumabil.
                Refuzul „nu există sold aici" e al gardianului motorului. */}
            <LookupGrila<BcsLinieWrite>
              camp="LotId"
              entitate="Lot"
              expand={['Produs']}
              afisare={etichetaLot}
              cauta="Produs.Denumire"
              sortare="Data"
              laSelectie={(l) => {
                setEtichete((prev) => ({ ...prev, LotEticheta: l ? etichetaLot(l) : '' }));
                // Tipul din PRODUSUL lotului, doar când e gol — același șablon
                // ca pe ASM/RLF/RDC (F20-D3). `$expand=Produs` e deja cerut
                // pentru eticheta lotului, deci `Produs.TipMaterialId` e în
                // răspunsul selecției.
                const p = l?.Produs as Record<string, unknown> | null | undefined;
                precompleteazaTip({
                  linie,
                  tipId: p?.TipMaterialId == null ? undefined : String(p.TipMaterialId),
                  setLinie,
                  setEtichete,
                });
              }}
            />
            <p className="indiciu">
              Lotul consumat — din el iese prețul cu care se evaluează linia.
            </p>
          </div>
          <CampNumar<BcsLinieWrite> camp="Cantitate" />
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
