import { useMemo } from 'react';
import { DateBox } from 'devextreme-react';
import DataSource from 'devextreme/data/data_source';
import ODataStore from 'devextreme/data/odata/store';
import { expiraSesiunea, token } from '../../nucleu/auth';
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

// Sursa de conturi pentru selectorul fișei. Duplică deliberat cablajul din
// `nucleu/Lookup` (ODataStore + JWT + 401), fiindcă `Lookup` e legat de
// formular prin `useCamp` și aici nu există formular. Dacă apare a treia
// utilizare în afara unui formular, extragerea în nucleu se justifică — azi ar
// fi o abstracție cu un singur consumator.
export function useSursaConturi() {
  return useMemo(() => new DataSource({
    store: new ODataStore({
      url: '/api/odata/Cont',
      key: 'ID',
      keyType: 'Guid',
      version: 4,
      beforeSend: (e) => { e.headers = { ...e.headers, Authorization: `Bearer ${token() ?? ''}` }; },
      errorHandler: (e) => { if (e.httpStatus === 401) expiraSesiunea(); },
    }),
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
