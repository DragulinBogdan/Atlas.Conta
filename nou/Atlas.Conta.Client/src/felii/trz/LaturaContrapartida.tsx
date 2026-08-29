import { useState } from 'react';
import { Lookup } from '../../nucleu/Lookup';
import { useCamp } from '../../nucleu/formular';
import { useSonda } from '../../nucleu/odata';

// Latura NE-trezorerie a unei plăți/încasări: beneficiarul plății, respectiv
// plătitorul încasării. Poate fi PARTENER, ANGAJAT (31a: ContPropriu →
// Partener/Angajat, avansul 542 pe angajat) sau, de la felia viramentului
// (F7-D1/F7-D8), un al doilea CONT PROPRIU — trei nomenclatoare diferite pe
// aceeași coloană `Repartitor`.
//
// Selectorul de FEL e trei `Lookup`-uri COMUTATE ÎN COD (43a: condiționalitate
// în JSX, nu descriptor). Nu s-ar putea unifica: partenerii sunt 129k (căutare
// remote), angajații și conturile proprii sunt nomenclatoare mici (local), iar
// „Repartitor" ca entitate unică ar amesteca gestiuni, angajați și conturi
// proprii în aceeași listă.
//
// Al treilea fel NU e o simplă a treia opțiune de nomenclator: contrapartida
// cont propriu ÎNSEAMNĂ virament intern (transferul 581), iar din ea decurge
// forma restului ecranului (Tipul implicit al liniei, panoul de stingeri
// ascuns, latura pereche). Decizia aia NU se ia aici — shell-ul o citește din
// VALOAREA laturii (`TrezorerieDetaliu`), fiindcă formularul e sursa de adevăr,
// nu comutatorul de fel.
//
// ═══ Deducerea felului la ÎNCĂRCAREA unui document existent ═══
// ReadDto-ul dă `PrimitorId` + `PrimitorDenumire`, nu FELUL laturii — și nici
// n-ar trebui: pe server e un `Repartitor`, distincția e a nomenclatoarelor.
// Deci se PROBEAZĂ mulțimile înguste, în ordinea Angajat → ContPropriu, cu
// sonda de existență din nucleu (`useSonda`); niciuna ⇒ partener (default).
// Eșecul sondei NU e fatal: `undefined` („nu știm" — sonda propagă erorile, nu
// le traduce în „nu") lasă felul pe default, iar operatorul are oricând
// comutatorul manual — care e și escape hatch-ul dacă deducerea greșește.
// Proba a doua pleacă doar dacă prima a răspuns „nu": mulțimile sunt disjuncte
// prin construcție (TPT pe `Repartitor`), deci ordinea e ordinea declarată, nu
// o prioritate care ar putea ascunde un conflict.

type Fel = 'Partener' | 'Angajat' | 'ContPropriu';

export function LaturaContrapartida<T extends object>(props: {
  camp: Extract<keyof T, string>;
  // Numele laturii în vocabularul feliei („Beneficiar" pe plată, „Plătitor" pe
  // încasare) — caption-ul bazei („Primitor (către)") e corect, dar abstract.
  eticheta: string;
  readOnly?: boolean;
}) {
  const { camp, eticheta, readOnly } = props;
  const c = useCamp<string>(camp, readOnly);
  const valoare = c.valoare;
  // Alegerea OMULUI, când există: bate deducerea și rămâne până o schimbă tot
  // el. Nul = felul se deduce din valoare.
  const [felManual, setFelManual] = useState<Fel | null>(null);
  const deduce = felManual === null;

  const esteAngajat = useSonda('Angajat', valoare, deduce);
  const esteContPropriu = useSonda('ContPropriu', valoare, deduce && esteAngajat === false);

  const fel: Fel = felManual
    ?? (esteAngajat ? 'Angajat' : esteContPropriu ? 'ContPropriu' : 'Partener');

  function comuta(nou: Fel) {
    if (nou === fel) return;
    setFelManual(nou);
    // Schimbarea felului e o schimbare de INTENȚIE: valoarea veche aparține
    // altui nomenclator și n-ar mai avea nici măcar afișare.
    c.seteaza(undefined);
  }

  return (
    <>
      <div className="camp">
        <label className="camp__eticheta">{eticheta} — fel</label>
        <div className="camp__control camp__comutator">
          {FELURI.map((f) => (
            <button
              key={f.fel}
              type="button"
              className={fel === f.fel ? 'buton buton--primar buton--mic' : 'buton buton--mic'}
              disabled={c.readOnly}
              onClick={() => comuta(f.fel)}
            >
              {f.eticheta}
            </button>
          ))}
        </div>
        <div className="camp__eroare" />
      </div>

      {fel === 'Partener'
        && <Lookup<T> camp={camp} eticheta={eticheta} entitate="Partener" mod="remote" readOnly={readOnly} cauta={['Cautare', 'CodFiscal']} />}
      {fel === 'Angajat'
        && <Lookup<T> camp={camp} eticheta={eticheta} entitate="Angajat" mod="local" readOnly={readOnly} cauta={['Cautare', 'Marca']} />}
      {fel === 'ContPropriu'
        && <Lookup<T> camp={camp} eticheta={`${eticheta} — cont propriu (virament intern)`} entitate="ContPropriu" mod="local" readOnly={readOnly} cauta={['Cautare', 'Iban']} />}
    </>
  );
}

const FELURI: { fel: Fel; eticheta: string }[] = [
  { fel: 'Partener', eticheta: 'Partener' },
  { fel: 'Angajat', eticheta: 'Angajat' },
  { fel: 'ContPropriu', eticheta: 'Cont propriu' },
];
