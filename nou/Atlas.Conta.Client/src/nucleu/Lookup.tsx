import { useMemo } from 'react';
import { SelectBox } from 'devextreme-react';
import DataSource from 'devextreme/data/data_source';
import ODataStore from 'devextreme/data/odata/store';
import { CampShell } from './CampShell';
import { defaultProperty } from './campMeta';
import { useCamp } from './formular';
import { token } from './auth';
import type { PropsCamp } from './campuri';

// `Lookup` = SelectBox + `ODataStore` nativ (43f): consumatorul ușii OData
// opt-in pe nomenclatoare (D7 — doar Gestiune/TipMaterial/Lot). Nu construim
// proiecții custom per nomenclator; `$filter`/`$top`/`$select`/`$expand` sunt
// deja acolo și trec prin securitatea aplicației.
//
// Mic vs mare = PROP EXPLICIT, nu mecanism (43f): `mod="local"` încarcă
// integral și filtrează în browser (gestiuni, tipuri); `mod="remote"` caută
// server-side cu debounce (46k loturi).
//
// Display-ul vine din `DefaultProperty`-ul emis în dump (43f), cu escape:
// `Lot.DefaultProperty` = `Eticheta`, dar `Eticheta` e `[NotMapped]`, deci NU
// traversează OData — de aceea `afisare` poate fi dată explicit de apelant.

export type PropsLookup<T extends object> = PropsCamp<T> & {
  entitate: string;
  mod: 'local' | 'remote';
  afisare?: (element: Record<string, unknown>) => string;
  cauta?: string | string[];
  expand?: string[];
  sortare?: string;
};

export function Lookup<T extends object>(props: PropsLookup<T>) {
  const { camp, readOnly, entitate, mod, afisare, cauta, expand, sortare } = props;
  const c = useCamp<string>(camp, readOnly);
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
        onValueChanged={(e) => c.seteaza((e.value as string) ?? undefined)}
      />
    </CampShell>
  );
}
