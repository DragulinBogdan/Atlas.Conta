import { useEffect, useState, type ReactNode } from 'react';
import { DateBox } from 'devextreme-react';
import { PanouErori } from './PanouErori';
import { eroriDin } from './http';
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

// Rezultatul interogării de CITIRE a documentului, în forma pe care o are un
// `useQuery` — felia îi pasează chiar obiectul ei (`citire={citit}`), fără să
// desfacă nimic. Shell-ul citește DOAR `isError`/`error`: `isPending` face parte
// din formă (ca ecranul să nu fie nevoit să construiască un obiect ad-hoc), dar
// nu-l ramifică nimeni aici — cât timp citirea e în curs se randează ecranul
// normal, iar „ocupat" rămâne al feliei.
export type CitireDocument = { isError: boolean; error: unknown; isPending?: boolean };

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
  // Orice nod FALSY (`false`, `null`, `undefined`, `''`) înseamnă „nicio
  // confirmare" — feliile scriu natural `confirmare={deSters && <…/>}`, care dă
  // `false`, iar `false == null` e fals (review F20 F1: gardul de mai jos era
  // permanent fals și „Stornează" murise pe 11 ecrane; `tsc` nu-l poate prinde,
  // `ReactNode` acceptă `false`).
  confirmare?: ReactNode;
  // Cum se închide confirmarea feliei. O SINGURĂ cerere în așteptare, iar
  // starea celor două stă în locuri diferite (cererea de dată e a shell-ului,
  // confirmarea e a feliei), deci regula are nevoie de câte un drum în fiecare
  // sens: `confirmare` non-nulă ascunde cererea de dată (mai jos), iar
  // deschiderea unei comenzi cu parametru cheamă callback-ul ăsta. Fără el,
  // operatorul ar vedea două întrebări suprapuse și ar răspunde la cealaltă.
  inchideConfirmarea?: () => void;
  // Interogarea de citire a documentului (F22-D8). Când ea a EȘUAT — 404 pe un
  // document invizibil pentru utilizatorul curent, 403, sau o eroare tehnică —
  // shell-ul arată mesajul serverului ȘI NU randează copiii: fără el, ecranul
  // cădea pe agregatul gol al feliei și afișa un formular NOU, gol, ca și cum
  // documentul ar fi fost citit. Un formular gol pe un refuz e o minciună; mai
  // rău, „Salvează" de pe el ar fi creat un al doilea document.
  // Zero ramificare pe status (43b): se arată fraza pe care a scris-o singura
  // ușă care știe motivul.
  citire?: CitireDocument;
}) {
  const {
    titlu, sumar, comenzi, erori, mesaje = [], rezultatExtra, ocupat = false,
    antet, linii, subsol, confirmare, inchideConfirmarea, citire,
  } = props;
  const [cerere, setCerere] = useState<Comanda | null>(null);
  const [data, setData] = useState<string | undefined>(azi());

  // Deschiderea unei confirmări a FELIEI trebuie să ÎNCHIDĂ cererea de dată (nu
  // doar s-o ascundă): altfel reapare, armată cu data veche, după „Renunță".
  const confirmareDeschisa = !!confirmare;
  useEffect(() => { if (confirmareDeschisa) setCerere(null); }, [confirmareDeschisa]);

  function apasa(c: Comanda) {
    inchideConfirmarea?.();
    if (!c.cereData) {
      c.ruleaza();
      return;
    }
    setData(c.cereData.implicit ?? azi());
    setCerere(c);
  }

  // Citirea a eșuat ⇒ rama și motivul, nimic altceva. Return-ul e DUPĂ toate
  // hook-urile shell-ului (altfel ordinea lor s-ar schimba între randări), iar
  // bara rămâne fără comenzi: nu se operează un document care nu s-a putut citi.
  if (citire?.isError) {
    return (
      <div className="document">
        <div className="document__bara">
          <h2 className="document__titlu">{titlu}</h2>
        </div>
        <PanouErori erori={eroriDin(citire.error)} titlu="Documentul nu s-a putut citi" />
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
              onClick={() => apasa(c)}
            >
              {c.eticheta}
            </button>
          ))}
        </div>
      </div>

      {confirmare}

      {!confirmareDeschisa && cerere?.cereData && (
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
