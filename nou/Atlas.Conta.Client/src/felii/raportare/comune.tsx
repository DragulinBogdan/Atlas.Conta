import { useMemo } from 'react';
import { DateBox } from 'devextreme-react';
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

// Caseta de dată a barei de raport. NU e un `CampData`: acela e legat de
// agregatul unui formular (`useCamp`), iar aici nu există formular — parametrul
// trăiește în URL (43c). Aceeași regulă de propagare ca peste tot în client:
// doar acțiunile omului (`e.event`) schimbă starea.
export function CasetaData(props: {
  eticheta: string;
  valoare: string;
  optionala?: boolean;
  seteaza: (v: string) => void;
}) {
  const { eticheta, valoare, optionala, seteaza } = props;
  return (
    <label className="bara-raport__camp">
      <span className="camp__eticheta">{eticheta}</span>
      <DateBox
        type="date"
        displayFormat="dd.MM.yyyy"
        // Cu buton de golire, 140px taie data afișată („01.01.2…").
        width={optionala ? 170 : 140}
        value={valoare || null}
        showClearButton={optionala}
        onValueChanged={(e) => { if (e.event) seteaza(izolataZi(e.value) ?? ''); }}
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
