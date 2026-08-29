import { useState, type ReactNode } from 'react';
import { DateBox } from 'devextreme-react';
import { PanouErori } from './PanouErori';
import { azi, izolataZi } from './zi';

// `DocumentShell` (43a): zona de antet, zona de linii, bara de comenzi condusă
// de AFFORDANCES și panoul `Erori[]`. Nu știe nimic despre BTR sau FCT — feliile
// îi dau conținutul; el dă forma comună a oricărui ecran de document.
//
// De ce comenzile vin ca DATE de la felie, dar disponibilitatea vine din
// ReadDto: `PoateEdita/PoateOpera/PoateAnula/PoateStorna` sunt calculate de
// server (42e). Clientul NU re-derivă „ce se poate" din `Stare` — ar fi un
// geamăn al regulii, exact ce interzice designul.

export type Comanda = {
  eticheta: string;
  disponibila: boolean;
  primara?: boolean;
  // Comanda care are nevoie de un PARAMETRU de dată (stornarea). Shell-ul
  // deschide un rând inline cu `DateBox` și cheamă `ruleaza(data)` la confirmare
  // — datoria spike-ului: `window.prompt` nu se poate formata, nu se poate
  // valida și nu arată calendarul cu care lucrează operatorul.
  cereData?: { eticheta: string; implicit?: string };
  ruleaza: (data?: string) => void;
};

export function DocumentShell(props: {
  titlu: string;
  sumar?: ReactNode;
  comenzi: Comanda[];
  erori: string[];
  mesaje?: string[];
  // Slot sub panoul de rezultat: acolo unde răspunsul serverului cere o
  // NAVIGARE, nu doar un text (FCT operată → „Deschide NIR-ul generat").
  rezultatExtra?: ReactNode;
  ocupat?: boolean;
  antet: ReactNode;
  linii?: ReactNode;
  // Zonă de sub linii, pentru ce ATÂRNĂ de document fără să fie agregatul lui:
  // azi panoul de STINGERI (imperecherile sunt legături între documente operate,
  // nu părți ale WriteDto-ului — 31d). Slot, nu componentă cunoscută de shell:
  // felia decide dacă și când îl montează.
  subsol?: ReactNode;
  // Confirmarea unei acțiuni a FELIEI (ștergerea draftului), randată în același
  // loc ca cererea de dată a shell-ului — de aceea slotul e aici și nu lângă
  // butonul care o deschide: cele două cereri se exclud. Felia montează un
  // `<ConfirmareInline>`; shell-ul îi dă doar locul.
  confirmare?: ReactNode;
  // Cum se închide confirmarea feliei. O SINGURĂ cerere în așteptare, iar
  // starea celor două stă în locuri diferite (cererea de dată e a shell-ului,
  // confirmarea e a feliei), deci regula are nevoie de câte un drum în fiecare
  // sens: `confirmare` non-nulă ascunde cererea de dată (mai jos), iar
  // deschiderea unei comenzi cu parametru cheamă callback-ul ăsta. Fără el,
  // operatorul ar vedea două întrebări suprapuse și ar răspunde la cealaltă.
  inchideConfirmarea?: () => void;
}) {
  const {
    titlu, sumar, comenzi, erori, mesaje = [], rezultatExtra, ocupat = false,
    antet, linii, subsol, confirmare, inchideConfirmarea,
  } = props;
  const [cerere, setCerere] = useState<Comanda | null>(null);
  const [data, setData] = useState<string | undefined>(azi());

  function apasa(c: Comanda) {
    inchideConfirmarea?.();
    if (!c.cereData) {
      c.ruleaza();
      return;
    }
    setData(c.cereData.implicit ?? azi());
    setCerere(c);
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
              onClick={() => apasa(c)}
            >
              {c.eticheta}
            </button>
          ))}
        </div>
      </div>

      {confirmare}

      {confirmare == null && cerere?.cereData && (
        <div className="cerere-data">
          <label className="camp__eticheta">{cerere.cereData.eticheta}</label>
          <DateBox
            type="date"
            displayFormat="dd.MM.yyyy"
            value={data ?? null}
            onValueChanged={(e) => setData(izolataZi(e.value))}
          />
          <button
            type="button"
            className="buton buton--primar"
            disabled={!data}
            onClick={() => { const c = cerere; setCerere(null); c.ruleaza(data); }}
          >
            Confirmă
          </button>
          <button type="button" className="buton" onClick={() => setCerere(null)}>Renunță</button>
        </div>
      )}

      <PanouErori erori={erori} titlu="Refuzat de server" />
      <PanouErori erori={mesaje} titlu="Rezultat" fel="succes" />
      {rezultatExtra}

      <section className="document__antet">{antet}</section>
      {linii && <section className="document__linii">{linii}</section>}
      {subsol}
    </div>
  );
}
