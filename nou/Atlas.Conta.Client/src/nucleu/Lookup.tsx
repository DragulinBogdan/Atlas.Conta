import { useMemo } from 'react';
import { SelectBox } from 'devextreme-react';
import type { ValueChangedEvent } from 'devextreme/ui/select_box';
import DataSource from 'devextreme/data/data_source';
import { CampShell } from './CampShell';
import { defaultProperty } from './campMeta';
import { useCamp } from './formular';
import { CAMP_CAUTARE, areCautare, storeOData } from './odata';
import type { PropsCamp } from './campuri';

// `Lookup` = SelectBox + store-ul OData al nucleului (43f, F20-D2):
// consumatorul ușii OData opt-in pe nomenclatoare (D7 + F2-D4 —
// Partener/Produs/TipTva/dimensiuni/Gestiune/TipMaterial/Lot). Nu construim
// proiecții custom per nomenclator; `$filter`/`$top`/`$select`/`$expand` sunt
// deja acolo și trec prin securitatea aplicației.
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
  // Câmpul (sau câmpurile) pe care caută operatorul. DEFAULT: coloana generată
  // `Cautare` a entității (F20-D1) — o singură coloană care acoperă și codul, și
  // denumirea, fără diacritice; entitățile care n-o au (`Lot`) cad pe
  // proprietatea de afișare, ca înainte. Apelantul o suprascrie când mai are
  // câmpuri de căutat care NU intră în `Cautare` (`CodFiscal`, `Iban`, `Marca`)
  // — atunci le scrie EXPLICIT, `Cautare` inclusă.
  cauta?: string | string[];
  expand?: string[];
  sortare?: string;
  // FILTRU server-side pe nomenclator (`DataSource.filter` → `$filter`), pentru
  // lookup-ul DEPENDENT de altă valoare a formularului: pinul de lot al FCL
  // (F4-D6) arată doar loturile produsului ales. Condiționalitatea („activ doar
  // cu produs ales") rămâne în COD, în felie (43a) — aici doar se transmite.
  //
  // Formatul e cel al DevExtreme (`['ProdusId', '=', id]`). Valorile GUID se
  // serializează ca `Edm.Guid` (nu ca string între apostrofuri) prin `fieldTypes`
  // dedus mai jos — altfel `$filter` iese `ProdusId eq '…'` și serverul îl refuză.
  filtru?: unknown[];
  // NOTIFICAREA selecției inițiate de operator: felia aplică datele derivate
  // (Produs → Tip) pe propria stare, cu UPDATE FUNCȚIONAL (`set(prev => …)`) —
  // nu prin starea formularului din closure. (Prima formă — patch întors aici
  // și aplicat cu `seteazaMulte` — interfera cu rezolvarea afișării
  // widget-ului; bug găsit la smoke F2.)
  laSelectie?: (element: ElementNomenclator | null) => void;
};

export function Lookup<T extends object>(props: PropsLookup<T>) {
  const { camp, readOnly, obligatoriu, eticheta, entitate, mod, afisare, cauta, expand, sortare, filtru, laSelectie } = props;
  const c = useCamp<string>(camp, readOnly, obligatoriu, eticheta);
  const proprietateAfisare = defaultProperty(entitate);
  // Prezența coloanei se citește din `metadata.json` — nu dintr-o listă de
  // entități scrisă de mână aici, care ar drifta la prima entitate nouă.
  const campCautare = cauta ?? (areCautare(entitate) ? CAMP_CAUTARE : proprietateAfisare);

  // `expand`/`filtru` sunt ARRAY-uri scrise inline în JSX: ca dependențe directe
  // ar fi mereu „noi" și ar reconstrui sursa la fiecare randare (widget reîncărcat
  // sub degetele operatorului). Cheia de identitate e CONȚINUTUL lor.
  const cheieExpand = JSON.stringify(expand ?? null);
  const cheieFiltru = JSON.stringify(filtru ?? null);

  const sursa = useMemo(() => new DataSource({
    // Store-ul e al NUCLEULUI (F20-D2): JWT, 401, normalizarea căutării și
    // `byKey` prin cache-ul comun stau într-un singur loc, nu în fiecare widget.
    store: storeOData(entitate, { fieldTypes: tipuriGuid(filtru) }),
    expand,
    filter: filtru,
    sort: sortare ?? proprietateAfisare,
    paginate: mod === 'remote',
    pageSize: mod === 'remote' ? 50 : undefined,
    // eslint-disable-next-line react-hooks/exhaustive-deps -- `expand`/`filtru` intră prin cheile de conținut de mai sus.
  }), [entitate, mod, proprietateAfisare, cheieExpand, sortare, cheieFiltru]);

  return (
    <CampShell meta={c.meta} eroare={c.eroare}>
      <SelectBox
        dataSource={sursa}
        value={c.valoare ?? null}
        readOnly={c.readOnly}
        valueExpr="ID"
        displayExpr={afisare ?? proprietateAfisare}
        searchEnabled
        searchExpr={campCautare}
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

// Tipurile de câmp pe care ODataStore le folosește la serializarea filtrului.
// Se DEDUC din valorile filtrului: o valoare cu forma unui GUID e `Edm.Guid`,
// deci `$filter` iese `ProdusId eq 1234…` (fără apostrofuri), forma pe care o
// cere OData v4. Deducerea din VALOARE, nu din numele câmpului, ca să nu existe
// o convenție tăcută („orice `…Id` e Guid") care să mintă la primul filtru pe un
// câmp text.
const FORMA_GUID = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

function tipuriGuid(filtru?: unknown[]): Record<string, 'Guid'> | undefined {
  if (!filtru) return undefined;
  const tipuri: Record<string, 'Guid'> = {};
  const parcurge = (nod: unknown) => {
    if (!Array.isArray(nod)) return;
    if (nod.length === 3 && typeof nod[0] === 'string' && typeof nod[2] === 'string' && FORMA_GUID.test(nod[2]))
      tipuri[nod[0]] = 'Guid';
    nod.forEach(parcurge);
  };
  parcurge(filtru);
  return Object.keys(tipuri).length > 0 ? tipuri : undefined;
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
