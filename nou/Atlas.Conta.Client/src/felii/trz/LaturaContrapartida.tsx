import { useEffect, useState } from 'react';
import { Lookup } from '../../nucleu/Lookup';
import { useCamp } from '../../nucleu/formular';
import { existaInSet } from '../../nucleu/sonda';

// Latura NE-trezorerie a unei plăți/încasări: beneficiarul plății, respectiv
// plătitorul încasării. Poate fi PARTENER sau ANGAJAT (31a: ContPropriu →
// Partener/Angajat, avansul 542 pe angajat) — două nomenclatoare diferite pe
// aceeași coloană `Repartitor`.
//
// Selectorul de FEL e două `Lookup`-uri COMUTATE ÎN COD (43a: condiționalitate
// în JSX, nu descriptor). Cele două n-ar putea fi un singur lookup: partenerii
// sunt 129k (căutare remote), angajații sunt un nomenclator mic (local), iar
// „Repartitor" ca entitate unică ar amesteca gestiuni și conturi proprii în
// aceeași listă.
//
// ═══ Deducerea felului la ÎNCĂRCAREA unui document existent ═══
// ReadDto-ul dă `PrimitorId` + `PrimitorDenumire`, nu FELUL laturii — și nici
// n-ar trebui: pe server e un `Repartitor`, distincția e a nomenclatoarelor.
// Deci se PROBEAZĂ mulțimea îngustă (angajații) cu sonda de existență din
// nucleu (`existaInSet`); 0 rezultate ⇒ partener (default). Eșecul sondei NU e
// fatal: felul rămâne pe default, iar operatorul are oricând comutatorul
// manual — care e și escape hatch-ul dacă deducerea greșește.

type Fel = 'Partener' | 'Angajat';

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
  const [fel, setFel] = useState<Fel>('Partener');
  // Id-ul pentru care s-a rulat deja sonda: fără el, fiecare re-randare a
  // formularului (adică fiecare tastă) ar întreba serverul din nou.
  const [probat, setProbat] = useState<string | null>(null);

  useEffect(() => {
    if (!valoare || valoare === probat) return;
    let activ = true;
    setProbat(valoare);
    void existaInSet('Angajat', valoare).then((da) => {
      if (activ && da) setFel('Angajat');
    });
    return () => { activ = false; };
  }, [valoare, probat]);

  function comuta(nou: Fel) {
    if (nou === fel) return;
    setFel(nou);
    // Schimbarea felului e o schimbare de INTENȚIE: valoarea veche aparține
    // celuilalt nomenclator și n-ar mai avea nici măcar afișare.
    c.seteaza(undefined);
    setProbat(null);
  }

  return (
    <>
      <div className="camp">
        <label className="camp__eticheta">{eticheta} — fel</label>
        <div className="camp__control camp__comutator">
          <button
            type="button"
            className={fel === 'Partener' ? 'buton buton--primar buton--mic' : 'buton buton--mic'}
            disabled={c.readOnly}
            onClick={() => comuta('Partener')}
          >
            Partener
          </button>
          <button
            type="button"
            className={fel === 'Angajat' ? 'buton buton--primar buton--mic' : 'buton buton--mic'}
            disabled={c.readOnly}
            onClick={() => comuta('Angajat')}
          >
            Angajat
          </button>
        </div>
        <div className="camp__eroare" />
      </div>

      {fel === 'Partener'
        ? <Lookup<T> camp={camp} eticheta={eticheta} entitate="Partener" mod="remote" readOnly={readOnly} cauta={['Denumire', 'Cod', 'CodFiscal']} />
        : <Lookup<T> camp={camp} eticheta={eticheta} entitate="Angajat" mod="local" readOnly={readOnly} cauta={['Denumire', 'Cod', 'Marca']} />}
    </>
  );
}
