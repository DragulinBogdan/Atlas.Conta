import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { useNavigate } from 'react-router';
import { TextBox } from 'devextreme-react';
import { DataGrid, Pager, Paging, Sorting } from 'devextreme-react/data-grid';
import DataSource from 'devextreme/data/data_source';
import { CAMP_CAUTARE, areCautare, storeOData } from '../../nucleu/odata';
import { defaultProperty } from '../../nucleu/campMeta';
import { useUrlStare } from '../../nucleu/urlStare';
import { idRand } from './api';

// Lista unui nomenclator — jumătatea de CITIRE a șablonului F20-D8.
//
// Deosebirea față de listele de documente nu e cosmetică: acelea merg pe
// `DataSourceLoader` (`storeRemote`, protocolul `devextreme-aspnet-data`), peste
// controllere REST scrise per felie. Nomenclatoarele n-au controller — au ușa
// OData deja deschisă (42f), deci grila merge pe `storeOData` (F20-D2), aceeași
// conductă pe care o folosesc lookup-urile. Un al doilea drum către aceleași
// rânduri ar fi însemnat două seturi de mesaje pentru același refuz.
//
// ═══ Căutarea: un câmp EXPLICIT, nu `searchPanel` ═══
// `searchPanel` al grilei ar căuta pe toate coloanele vizibile, cu OR — adică
// și pe `CodFiscal`, și pe `Tara`, fiecare cu sintaxa lui —, iar normalizarea
// fără diacritice (F20-D1) e a coloanei generate `Cautare`, nu a celorlalte. Un
// singur câmp pe `Cautare` spune exact ce face: „caută în cod SAU denumire,
// fără să conteze diacriticele". Termenul pleacă NENORMALIZAT: `beforeSend`-ul
// store-ului rescrie `contains(tolower(Cautare),'x')` ⇒ `contains(Cautare,'x')`
// normalizat — un singur loc care știe tabelul, pentru lookup-uri și pentru
// grile deopotrivă.
//
// Căutarea e în URL (43c): „lista de parteneri filtrată pe «alfa»" e un link.

export function ListaNomenclator(props: {
  titlu: string;
  entitate: string;
  // Prefixul rutelor feliei: `/parteneri` ⇒ `/parteneri/nou`, `/parteneri/:id`.
  ruta: string;
  // `Nou` lipsește pe ecranele fără creare (nomenclatoarele de citire).
  poateCrea?: boolean;
  expand?: string[];
  sortare?: string;
  substitutCautare?: string;
  indiciu?: ReactNode;
  // Coloanele — `<Column>`-uri, scrise de ecran. Grila nu le deduce din
  // metadata: identitatea coloanelor e cod, ca identitatea editorilor (43a).
  children: ReactNode;
}) {
  const { titlu, entitate, ruta, poateCrea = true, expand, sortare, substitutCautare, indiciu, children } = props;
  const navigheaza = useNavigate();
  const [stare, seteaza] = useUrlStare({ cauta: '' });

  // Buffer LOCAL peste starea din URL (lecția 69h): un `TextBox` legat direct de
  // o valoare care se schimbă asincron pierde tastări. Aici bufferul e sursa cât
  // timp se tastează, iar URL-ul (deci și filtrul grilei) primește valoarea după
  // o pauză — altfel fiecare literă ar fi o cerere și o intrare de istoric.
  const [text, setText] = useState(stare.cauta);
  useEffect(() => {
    const ceas = setTimeout(() => { if (text !== stare.cauta) seteaza({ cauta: text }); }, 350);
    return () => clearTimeout(ceas);
  }, [text, stare.cauta, seteaza]);

  const cautaPeCautare = areCautare(entitate);
  const cheieExpand = JSON.stringify(expand ?? null);

  const sursa = useMemo(() => new DataSource({
    store: storeOData(entitate),
    expand,
    // Entitatea fără coloană generată cade pe proprietatea ei de afișare —
    // căutarea rămâne, doar că sensibilă la diacritice. Prezența se citește din
    // metadata, nu dintr-o listă scrisă de mână care ar drifta.
    filter: stare.cauta
      ? [cautaPeCautare ? CAMP_CAUTARE : defaultProperty(entitate), 'contains', stare.cauta]
      : null,
    sort: sortare ?? defaultProperty(entitate),
    paginate: true,
    pageSize: 25,
    requireTotalCount: true,
    // `expand` intră prin `cheieExpand` (JSON stabil), nu prin identitatea
    // array-ului: un literal `['TipMaterial']` scris în JSX e alt obiect la
    // fiecare render și ar recrea `DataSource`-ul (deci ar reîncărca grila) fără
    // ca nimic să se fi schimbat. Același tipar ca în `Lookup`.
  }), [entitate, cheieExpand, sortare, stare.cauta, cautaPeCautare]);

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>{titlu}</h2>
        <div className="nomenclator__cautare">
          <TextBox
            value={text}
            placeholder={substitutCautare ?? 'Caută în cod sau denumire…'}
            showClearButton
            valueChangeEvent="input"
            onValueChanged={(e) => setText((e.value as string) ?? '')}
          />
        </div>
        {poateCrea && (
          <button type="button" className="buton buton--primar" onClick={() => navigheaza(`${ruta}/nou`)}>
            Nou
          </button>
        )}
      </div>

      <DataGrid
        dataSource={sursa}
        remoteOperations
        showBorders
        columnAutoWidth
        height="calc(100vh - 170px)"
        onRowDblClick={(e) => { if (poateCrea) navigheaza(`${ruta}/${idRand(e.data)}`); }}
      >
        <Sorting mode="multiple" />
        <Paging defaultPageSize={25} />
        <Pager showInfo showPageSizeSelector allowedPageSizes={[25, 50, 100]} />
        {children}
      </DataGrid>

      {indiciu && <p className="indiciu">{indiciu}</p>}
    </div>
  );
}
