import type { ReactNode } from 'react';
import { PanouErori } from './PanouErori';

// `DocumentShell` (43a): zona de antet, zona de linii, bara de comenzi condusă
// de AFFORDANCES și panoul `Erori[]`. Nu știe nimic despre BTR — feliile îi dau
// conținutul; el dă forma comună a oricărui ecran de document.
//
// De ce comenzile vin ca DATE de la felie, dar disponibilitatea vine din
// ReadDto: `PoateEdita/PoateOpera/PoateAnula/PoateStorna` sunt calculate de
// server (42e). Clientul NU re-derivă „ce se poate" din `Stare` — ar fi un
// geamăn al regulii, exact ce interzice designul.

export type Comanda = {
  eticheta: string;
  disponibila: boolean;
  primara?: boolean;
  ruleaza: () => void;
};

export function DocumentShell(props: {
  titlu: string;
  sumar?: ReactNode;
  comenzi: Comanda[];
  erori: string[];
  mesaje?: string[];
  ocupat?: boolean;
  antet: ReactNode;
  linii?: ReactNode;
}) {
  const { titlu, sumar, comenzi, erori, mesaje = [], ocupat = false, antet, linii } = props;
  return (
    <div className="document">
      <div className="document__bara">
        <h2 className="document__titlu">{titlu}</h2>
        <div className="document__sumar">{sumar}</div>
        <div className="document__comenzi">
          {comenzi.map((c) => (
            <button
              key={c.eticheta}
              type="button"
              className={c.primara ? 'buton buton--primar' : 'buton'}
              disabled={!c.disponibila || ocupat}
              onClick={c.ruleaza}
            >
              {c.eticheta}
            </button>
          ))}
        </div>
      </div>

      <PanouErori erori={erori} titlu="Refuzat de server" />
      <PanouErori erori={mesaje} titlu="Rezultat" fel="succes" />

      <section className="document__antet">{antet}</section>
      {linii && <section className="document__linii">{linii}</section>}
    </div>
  );
}
