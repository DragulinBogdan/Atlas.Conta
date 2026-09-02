import type { ReactNode } from 'react';
import { PanouErori } from '../../nucleu/PanouErori';
import type { CitireDocument } from '../../nucleu/DocumentShell';
import { eroriDin } from '../../nucleu/http';

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
  // Interogarea de citire a rândului (F22-D8), aceeași mecanică și cu aceeași
  // formă ca pe documente (`CitireDocument`, tipul lui `DocumentShell`): un
  // `GET api/odata/Partener(id)` refuzat — 404 pe rândul invizibil pentru
  // utilizatorul curent, 403, ori o eroare tehnică — arată motivul serverului
  // ȘI NU randează formularul. Altfel ecranul cădea pe valoarea locală goală și
  // afișa un rând NOU, gol, ca și cum ar fi fost citit.
  citire?: CitireDocument;
  children: ReactNode;
}) {
  const {
    titlu, sumar, comenzi, erori, mesaje = [], atentionari = [],
    ocupat = false, confirmare, rezultat, citire, children,
  } = props;

  // Rama și motivul, fără comenzi: nu se salvează și nu se șterge un rând care
  // nu s-a putut citi. Shell-ul n-are hook-uri, deci return-ul poate sta aici.
  if (citire?.isError) {
    return (
      <div className="document">
        <div className="document__bara">
          <h2 className="document__titlu">{titlu}</h2>
        </div>
        <PanouErori erori={eroriDin(citire.error)} titlu="Rândul nu s-a putut citi" />
      </div>
    );
  }

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
