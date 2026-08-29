import type { ReactNode } from 'react';
import { PanouErori } from '../../nucleu/PanouErori';

// Rama unui ecran de nomenclator: titlu, comenzi, panourile de verdict, slotul
// de confirmare, formularul.
//
// ═══ De ce NU `DocumentShell` ═══
// Forma e aceeași și clasele CSS sunt chiar ale lui — dar vocabularul lui e
// document: `Comanda.cereData` (parametrul stornării), `linii`, `subsol` (panoul
// de stingeri), și disponibilitatea condusă de affordances calculate de server
// (`PoateOpera/PoateAnula/PoateStorna`). Un nomenclator n-are nici una: n-are
// stări, n-are linii, n-are imperecheri, iar „ce se poate" e „exista rândul?".
// A-l fi refolosit ar fi însemnat să car pe fiecare ecran de nomenclator un
// mecanism de cerere de dată care nu se deschide niciodată — și, mai rău, să
// las impresia că nomenclatorul E un document cu mai puține butoane.
// Ce se ÎMPARTE rămâne împărțit: `PanouErori`, `ConfirmareInline`, `Formular`,
// `Camp*`, `Lookup` și clasele de stil.

export type ComandaNomenclator = {
  eticheta: string;
  disponibila: boolean;
  primara?: boolean;
  ruleaza: () => void;
};

export function ShellNomenclator(props: {
  titlu: string;
  sumar?: ReactNode;
  comenzi: ComandaNomenclator[];
  erori: string[];
  mesaje?: string[];
  // Avertismentele nu sunt nici refuz, nici confirmare (felia 12): registrul
  // ANAF le produce („CUI-ul nu figurează"), iar afișate ca erori ar speria
  // degeaba.
  atentionari?: string[];
  ocupat?: boolean;
  // Confirmarea unei acțiuni ireversibile, randată ÎNAINTEA panourilor, în
  // același loc ca pe documente. Slot, nu componentă: ecranul montează un
  // `<ConfirmareInline>` (F20-D4).
  confirmare?: ReactNode;
  // Rezultatul unei comenzi cu formă proprie (raportul sincronizării ANAF).
  rezultat?: ReactNode;
  children: ReactNode;
}) {
  const {
    titlu, sumar, comenzi, erori, mesaje = [], atentionari = [],
    ocupat = false, confirmare, rezultat, children,
  } = props;

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

      {confirmare}

      <PanouErori erori={erori} titlu="Refuzat de server" />
      <PanouErori erori={mesaje} titlu="Rezultat" fel="succes" />
      <PanouErori erori={atentionari} titlu="Atenționări" fel="atentie" />
      {rezultat}

      <section className="document__antet">{children}</section>
    </div>
  );
}
