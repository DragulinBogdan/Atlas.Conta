import { useMemo } from 'react';
import { SelectBox } from 'devextreme-react';
import type { ValueChangedEvent } from 'devextreme/ui/select_box';
import DataSource from 'devextreme/data/data_source';
import ODataStore from 'devextreme/data/odata/store';
import { CampShell } from './CampShell';
import { defaultProperty } from './campMeta';
import { useCamp } from './formular';
import { expiraSesiunea, token } from './auth';
import type { PropsCamp } from './campuri';

// `Lookup` = SelectBox + `ODataStore` nativ (43f): consumatorul ușii OData
// opt-in pe nomenclatoare (D7 + F2-D4 — Partener/Produs/TipTva/dimensiuni/
// Gestiune/TipMaterial/Lot). Nu construim proiecții custom per nomenclator;
// `$filter`/`$top`/`$select`/`$expand` sunt deja acolo și trec prin securitatea
// aplicației.
//
// Mic vs mare = PROP EXPLICIT, nu mecanism (43f): `mod="local"` încarcă
// integral și filtrează în browser (gestiuni, tipuri); `mod="remote"` caută
// server-side cu debounce (parteneri 129k, produse 312k, loturi).
//
// Display-ul vine din `DefaultProperty`-ul emis în dump (43f), cu escape:
// `Lot.DefaultProperty` = `Eticheta`, dar `Eticheta` e `[NotMapped]`, deci NU
// traversează OData — de aceea `afisare` poate fi dată explicit de apelant.

// Elementul selectat, ca date brute ale nomenclatorului. Există pentru
// precompletări (Produs → Tip): răspunsul OData al selecției e DEJA în client,
// deci nu se face niciun fetch în plus.
export type ElementNomenclator = Record<string, unknown>;

export type PropsLookup<T extends object> = PropsCamp<T> & {
  entitate: string;
  mod: 'local' | 'remote';
  afisare?: (element: ElementNomenclator) => string;
  cauta?: string | string[];
  expand?: string[];
  sortare?: string;
  // NOTIFICAREA selecției inițiate de operator: felia aplică datele derivate
  // (Produs → Tip) pe propria stare, cu UPDATE FUNCȚIONAL (`set(prev => …)`) —
  // nu prin starea formularului din closure. (Prima formă — patch întors aici
  // și aplicat cu `seteazaMulte` — interfera cu rezolvarea afișării
  // widget-ului; bug găsit la smoke F2.)
  laSelectie?: (element: ElementNomenclator | null) => void;
};

export function Lookup<T extends object>(props: PropsLookup<T>) {
  const { camp, readOnly, obligatoriu, eticheta, entitate, mod, afisare, cauta, expand, sortare, laSelectie } = props;
  const c = useCamp<string>(camp, readOnly, obligatoriu, eticheta);
  const proprietateAfisare = defaultProperty(entitate);

  const sursa = useMemo(() => new DataSource({
    store: new ODataStore({
      url: `/api/odata/${entitate}`,
      key: 'ID',
      keyType: 'Guid',
      version: 4,
      // JWT-ul nu poate trece prin `http.ts` aici: cererea o face componenta
      // DevExtreme. Același token, același header, un singur loc.
      beforeSend: (e) => { e.headers = { ...e.headers, Authorization: `Bearer ${token() ?? ''}` }; },
      // Sesiunea expirată trebuie tratată la fel pe TOATE conductele, nu doar pe
      // `http.ts`: altfel lookup-urile rămân mute (listă goală) după expirare.
      errorHandler: (e) => { if (e.httpStatus === 401) expiraSesiunea(); },
    }),
    expand,
    sort: sortare ?? proprietateAfisare,
    paginate: mod === 'remote',
    pageSize: mod === 'remote' ? 50 : undefined,
  }), [entitate, mod, proprietateAfisare, expand, sortare]);

  return (
    <CampShell meta={c.meta} eroare={c.eroare}>
      <SelectBox
        dataSource={sursa}
        value={c.valoare ?? null}
        readOnly={c.readOnly}
        valueExpr="ID"
        displayExpr={afisare ?? proprietateAfisare}
        searchEnabled
        searchExpr={cauta ?? proprietateAfisare}
        searchTimeout={mod === 'remote' ? 400 : 200}
        showClearButton={!c.meta.obligatoriu}
        noDataText="Nimic găsit"
        onValueChanged={(e) => {
          // DOAR schimbările OPERATORULUI se propagă în formular (bug găsit la
          // smoke F2): widget-ul ridică `onValueChanged` și la schimbarea
          // PROGRAMATICĂ a `value` (starea a pus câmpul), iar un `seteaza` din
          // acel apel pleacă din closure-ul VECHI al contextului și ȘTERGE
          // celelalte câmpuri abia scrise (ProdusId murea când precompletarea
          // punea TipMaterialId). Formularul e sursa de adevăr; widget-ul
          // raportează exclusiv acțiunile omului (`e.event`).
          if (!e.event)
            return;
          // ODataStore livrează cheile Edm.Guid ca OBIECTE `Guid` DevExtreme —
          // pe sârmă serializează corect prin propriul `toJSON`; comparațiile
          // se normalizează cu `String()` (`elementSelectat`, `laSelectie`).
          const valoare = (e.value as string) ?? undefined;
          c.seteaza(valoare);
          if (laSelectie)
            laSelectie(elementSelectat(e, valoare));
        }}
      />
    </CampShell>
  );
}

// `selectedItem` e actualizat asincron pe drumul valorii (drop_down_list îl
// rezolvă printr-un `Deferred`), deci nu se poate conta pe el singur: plasa e
// pagina ÎNCĂRCATĂ a sursei, unde elementul tocmai clicat există prin
// construcție. Dacă nu-l găsim, nu inventăm nimic — precompletarea pur și
// simplu nu se întâmplă.
function elementSelectat(e: ValueChangedEvent, valoare: string | undefined): ElementNomenclator | null {
  if (valoare == null) return null;
  // `ID`-ul item-ului e obiect `Guid` DevExtreme, `valoare` e string normalizat —
  // comparația trece prin `String()` pe ambele părți (bug găsit la smoke F2:
  // `===` pe forme diferite nu găsea niciodată elementul).
  const cheie = String(valoare);
  const selectat = e.component.option('selectedItem') as ElementNomenclator | null | undefined;
  if (selectat && String(selectat.ID) === cheie) return selectat;
  const incarcate = e.component.getDataSource()?.items() as ElementNomenclator[] | undefined;
  return incarcate?.find((i) => String(i.ID) === cheie) ?? null;
}
