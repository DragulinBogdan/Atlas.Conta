import { useMemo } from 'react';
import { DateRangeBox } from 'devextreme-react/date-range-box';
import DataSource from 'devextreme/data/data_source';
import { storeOData } from '../../nucleu/odata';
import { izolataZi } from '../../nucleu/zi';

// Piesele comune celor trei ecrane de raportare. Stau în FELIE, nu în nucleu:
// bara de perioadă e vocabularul rapoartelor, nu al documentelor.

// Luna curentă — implicitul balanței și al fișei (perioada e OBLIGATORIE acolo,
// deci trebuie să existe una la prima deschidere; serverul refuză cu 400 dacă
// lipsește, și bine face: „sold inițial" fără `dataStart` n-are înțeles).
export function lunaCurenta(): { start: string; sfarsit: string } {
  const acum = new Date();
  const an = acum.getFullYear();
  const luna = acum.getMonth();
  return { start: zi(new Date(an, luna, 1)), sfarsit: zi(new Date(an, luna + 1, 0)) };
}

function zi(d: Date): string {
  return `${d.getFullYear()}-${`${d.getMonth() + 1}`.padStart(2, '0')}-${`${d.getDate()}`.padStart(2, '0')}`;
}

// Perioada barei de raport — UN widget (`DateRangeBox`), nu două casete: cele
// două date sunt UN parametru (un interval), iar start ≤ sfârșit vine din
// componentă, nu dintr-o validare a noastră. NU e un `CampData`: acela e legat
// de agregatul unui formular (`useCamp`), iar aici nu există formular —
// parametrul trăiește în URL (43c). Aceeași regulă de propagare ca peste tot în
// client: doar acțiunile omului (`e.event`) schimbă starea.
export function CasetaPerioada(props: {
  dataStart: string;
  dataEnd: string;
  // Ecranele cu perioadă OBLIGATORIE (balanța, fișa, formularele) primesc
  // intervalul doar ÎNTREG: în calendar alegerea începe cu startul, iar un
  // patch intermediar `[start, null]` ar pleca pe sârmă ca cerere invalidă
  // (serverul refuză cu 400, pe bună dreptate). Pe cele cu perioadă opțională
  // (registrul-jurnal) capetele sunt filtre independente, deci trec și singure.
  optionala?: boolean;
  seteaza: (v: { dataStart: string; dataEnd: string }) => void;
}) {
  const { dataStart, dataEnd, optionala, seteaza } = props;
  return (
    <label className="bara-raport__camp">
      <span className="camp__eticheta">Perioada</span>
      <DateRangeBox
        displayFormat="dd.MM.yyyy"
        startDateLabel=""
        endDateLabel=""
        width={280}
        value={[dataStart || null, dataEnd || null]}
        showClearButton={optionala}
        onValueChanged={(e) => {
          if (!e.event) return;
          const [s, f] = (e.value ?? [null, null]) as [unknown, unknown];
          const start = izolataZi(s) ?? '';
          const sfarsit = izolataZi(f) ?? '';
          if (!optionala && (!start || !sfarsit)) return;
          seteaza({ dataStart: start, dataEnd: sfarsit });
        }}
      />
    </label>
  );
}

// Sursa de conturi pentru selectorul fișei. Cablajul (URL + JWT + 401 +
// normalizarea căutării + `byKey` prin cache) e al NUCLEULUI din felia 20
// (F20-D2): a treia utilizare — ecranele de nomenclator — a venit, iar cu ea a
// venit și motivul pentru care duplicarea nu mai era doar redundantă (fiecare
// store nativ își refetcha singur eticheta valorii curente). `Lookup` rămâne
// separat fiindcă el e legat de formular prin `useCamp`, iar aici nu există
// formular; ce împart acum e store-ul, nu componenta.
export function useSursaConturi() {
  return useMemo(() => new DataSource({
    store: storeOData('Cont'),
    sort: 'Simbol',
    paginate: true,
    pageSize: 50,
  }), []);
}

export type ElementCont = { ID?: unknown; Simbol?: string; Denumire?: string };

export function etichetaCont(c: ElementCont | null | undefined): string {
  if (!c) return '';
  return c.Denumire ? `${c.Simbol} — ${c.Denumire}` : `${c.Simbol ?? ''}`;
}
