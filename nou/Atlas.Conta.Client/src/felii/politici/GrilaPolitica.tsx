import { useMemo, useState, type ReactNode } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { Column, DataGrid, Editing, Form, Pager, Paging, Popup, Sorting } from 'devextreme-react/data-grid';
import DataSource from 'devextreme/data/data_source';
import CustomStore from 'devextreme/data/custom_store';
import { storeOData } from '../../nucleu/odata';
import { eroriDin, ia } from '../../nucleu/http';
import { izolataZi } from '../../nucleu/zi';
import { creeazaRand, invalideaza, modificaRand, stergeRand } from '../nomenclatoare/api';

// Grila comună de ÎNTREȚINERE a unei politici (F23-D7): un `DataGrid` cu editare
// pe rând peste ușa OData deschisă de F23-D5, plus panoul de istoric al rândului
// selectat (F23-D9).
//
// ═══ De ce un `CustomStore` peste `storeOData`, și nu `storeOData` direct ═══
// Citirea și scrierea merg pe conducte DIFERITE, deliberat:
//   • CITIREA (`load`/`byKey`) deleagă la `storeOData` — paginarea, sortarea și
//     `$expand` rămân server-side, iar `byKey` trece prin cache-ul comun (77b).
//   • SCRIEREA (`insert`/`update`/`remove`) trece prin `nomenclatoare/api.ts`,
//     adică prin `http.ts`, fiindcă acolo se citește CORPUL răspunsului. Pe
//     conducta `ODataStore` mesajul serverului se pierde: `errorFromResponse`
//     păstrează doar `statusText` (80-r1, măsurat pe sursele DevExtreme). Grila
//     ar fi arătat „Unprocessable Content" în locul frazei gardianului — exact
//     ce clientul nu are voie să facă (80h: motivul e al serverului).
// Refuzul se re-aruncă drept `Error` cu mesajul serverului: `DataGrid` ține
// rândul în editare (`_processSaveEditDataResult` scoate schimbarea doar pe
// ștergere) și îl afișează în rândul de eroare (`errorRowEnabled`, implicit).
//
// ═══ De ce NU există un prop `poateEdita` ═══
// Ar fi însemnat să ghicim rolul în TS ca să ascundem butoane. Regula e 80h:
// clientul ARATĂ motivul, nu îl anticipează. Grila e mereu editabilă, iar
// `Cititor` primește la salvare 403-ul serverului („Nu aveți dreptul de a
// modifica …"), în rândul lui. Ce se închide prin prop sunt fapte de DOMENIU,
// nu permisiuni: `TipDocument` e ancoră, deci nu se creează și nu se șterge (20).

const NAMESPACE_CLR = 'Atlas.Conta.BackOffice.Module.BusinessObjects.';

// Forma `Deferred`-ului DevExtreme: `load` livrează DOUĂ argumente (datele și
// `extra`, cu `totalCount`), iar `then` ar păstra doar primul. De aceea
// delegarea se face pe `done`/`fail`, nu pe conducta de promisiuni.
type Amanat = {
  done: (f: (date: unknown, extra?: { totalCount?: number }) => void) => Amanat;
  fail: (f: (e: unknown) => void) => Amanat;
};

// Corpul unei scrieri de politică. Aceeași regulă ca `corpScriere` (77h) —
// `undefined`/`''` ⇒ `null`, ca golirea să plece ca golire — cu două deosebiri:
//   • DELTA: pentru `update` DevExtreme dă DOAR câmpurile schimbate, iar PATCH-ul
//     OData e chiar o deltă (absența = neatins). Nu se completează nimic.
//   • Datele: `Edm.Date` vine de pe sârmă ca STRING („2030-01-01" n-are `T`,
//     deci `ODataStore` nu-l deserializează), dar editorul de dată produce un
//     `Date`. `JSON.stringify` l-ar fi trimis ca datetime UTC, peste un
//     `DateOnly`. Se convertesc DOAR obiectele `Date`, pe componentele LOCALE.
function corpPolitica(valori: Record<string, unknown>): Record<string, unknown> {
  const corp: Record<string, unknown> = {};
  for (const [camp, v] of Object.entries(valori))
    corp[camp] = v === undefined || v === '' ? null : v instanceof Date ? izolataZi(v) : v;
  return corp;
}

// Gruparea câmpurilor pe formularul de editare. Grupurile sunt ale ECRANULUI
// (43a) — grila doar le așază; nimic aici nu deduce ce câmp intră în ce grup.
export type GrupFormular = { titlu: string; campuri: string[] };

export function GrilaPolitica(props: {
  titlu: string;
  entitate: string;
  expand?: string[];
  // Fapte de DOMENIU, nu permisiuni (vezi antetul).
  poateAdauga?: boolean;
  poateSterge?: boolean;
  // Numele CLR complet al entității auditate; implicit se derivă din numele ei
  // (toate clasele de model stau într-un singur namespace — verificat).
  tipClr?: string;
  indiciu?: ReactNode;
  // Prezent ⇒ editarea trece din rând în POPUP, cu grupurile de aici. Absent ⇒
  // editarea pe rând, neschimbată.
  formular?: GrupFormular[];
  // Valorile propuse pe rândul NOU (`onInitNewRow`) — afordanță, nu regulă: ce
  // se refuză rămâne al gardianului (81-r8). Se propune și pentru enum-urile
  // fără membrul 0 (`LaturaDocument`, `TipStoc`, `DirectieTva`, `SensTva`,
  // `TipOperatiuneD394` — convenția „default invalid" din `Enums.cs`): pe ușa
  // OData un câmp neatins pleacă absent, EF scrie 0, iar rândul se întoarce cu
  // un enum care nu e niciun membru — celulă goală, nu greșită.
  laRandNou?: (rand: Record<string, unknown>) => void;
  // Coloanele — `<Column>`-uri scrise de ecran (43a). Coloana `DinSeed` o pune
  // grila, fiindcă e a ȘABLONULUI: orice politică are proveniență (F23-D4).
  children: ReactNode;
}) {
  const {
    titlu, entitate, expand, formular, laRandNou,
    poateAdauga = true, poateSterge = true, tipClr, indiciu, children,
  } = props;
  const cache = useQueryClient();
  const [randFocalizat, setRandFocalizat] = useState<string | null>(null);

  // `expand` e un literal scris în JSX: ca dependență directă ar reconstrui sursa
  // la fiecare randare (tiparul din `Lookup`/`ListaNomenclator`).
  const cheieExpand = JSON.stringify(expand ?? null);

  // Itemii formularului de editare. `editing.form.items` e ce face modul popup
  // să merite: FĂRĂ el DevExtreme compune formularul din `getColumns()`, care
  // întoarce doar coloanele VIZIBILE; cu el, fiecare `dataField` se rezolvă prin
  // `columnOption('dataField:…')`, care caută în toate coloanele — deci o
  // coloană `visible={false}` ajunge editabilă pe formular (sursa:
  // `devextreme/esm/__internal/grids/grid_core/editing/m_editing_form_based.js`,
  // `getEditFormOptions`). Aceeași cheie de conținut ca la `expand`.
  const cheieFormular = JSON.stringify(formular ?? null);
  const itemiFormular = useMemo(
    () => formular?.map((g) => ({
      itemType: 'group' as const,
      caption: g.titlu,
      colCount: 2,
      items: g.campuri.map((dataField) => ({ dataField })),
    })),
  // eslint-disable-next-line react-hooks/exhaustive-deps -- `formular` intră prin cheia de conținut.
  [cheieFormular]);

  const sursa = useMemo(() => {
    const citire = storeOData(entitate);
    return new DataSource({
      store: new CustomStore({
        key: 'ID',
        load: (optiuni) => new Promise((resolve, reject) => {
          (citire.load({ ...optiuni, expand } as never) as unknown as Amanat)
            .done((date, extra) => resolve(extra && 'totalCount' in extra
              ? { data: date as unknown[], totalCount: extra.totalCount as number }
              : (date as unknown[])))
            .fail(reject);
        }),
        byKey: (cheie) => new Promise((resolve, reject) => {
          (citire.byKey(cheie) as unknown as Amanat).done(resolve).fail(reject);
        }),
        insert: (valori) => scrie(() => creeazaRand(entitate, corpPolitica(valori as Record<string, unknown>))),
        update: (cheie, valori) =>
          scrie(() => modificaRand(entitate, String(cheie), corpPolitica(valori as Record<string, unknown>))),
        remove: (cheie) => scrie(() => stergeRand(entitate, String(cheie))),
      }),
      paginate: true,
      pageSize: 25,
      requireTotalCount: true,
      // Fără `sort` aici, deliberat: `DataGrid` compune ordinea din COLOANE și o
      // suprascrie pe cea a sursei (`getSortDataSourceParameters`). Un `sort` pus
      // pe `DataSource` ar fi arătat ca ordine implicită și n-ar fi făcut nimic.
      // Ordinea de pornire a fiecărui ecran se scrie deci pe coloană
      // (`defaultSortOrder`), unde e vizibilă și operatorului.
    });
  // eslint-disable-next-line react-hooks/exhaustive-deps -- `expand` intră prin cheia de conținut.
  }, [entitate, cheieExpand]);

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>{titlu}</h2>
      </div>

      {/* Fără `keyExpr`: cheia o declară store-ul (`key: 'ID'`). Pe o sursă care
          nu e array DevExtreme îl ignoră și avertizează în consolă (W1011). */}
      <DataGrid
        dataSource={sursa}
        remoteOperations
        showBorders
        columnAutoWidth
        focusedRowEnabled
        height="calc(100vh - 430px)"
        onFocusedRowChanged={(e) => setRandFocalizat(e.row ? String((e.row.data as { ID?: unknown }).ID ?? '') : null)}
        // Două invalidări, din două motive diferite:
        //   • etichetele lookup-urilor trăiesc în cache-ul comun (`staleTime:
        //     Infinity`, 77b) — fără asta un tip de TVA redenumit ar rămâne cu
        //     eticheta veche în ecranele de document deja deschise;
        //   • ISTORICUL rândului tocmai s-a lungit cu scrierea de acum, iar cheia
        //     lui nu s-a schimbat: fără invalidare panoul ar arăta jurnalul de
        //     dinaintea modificării pe care operatorul tocmai a făcut-o.
        onSaved={() => {
          invalideaza(cache, entitate);
          void cache.invalidateQueries({ queryKey: ['audit'] });
        }}
        onInitNewRow={(e) => laRandNou?.(e.data as Record<string, unknown>)}
      >
        <Sorting mode="multiple" />
        <Paging defaultPageSize={25} />
        <Pager showInfo showPageSizeSelector allowedPageSizes={[25, 50, 100]} />
        {/* Refuzul serverului se vede la fel pe amândouă modurile: `renderErrorRow`
            primește conținutul popup-ului și prepend-ează mesajul în el
            (`grid_core/error_handling/m_error_handling.js`), iar rândul rămâne
            în editare. */}
        <Editing
          mode={itemiFormular ? 'popup' : 'row'}
          useIcons
          allowUpdating
          allowAdding={poateAdauga}
          allowDeleting={poateSterge}
        >
          {itemiFormular && <Popup title={titlu} showTitle width={960} height={620} />}
          {itemiFormular && <Form items={itemiFormular} labelLocation="top" />}
        </Editing>

        {children}

        {/* Proveniența (F23-D4): timbrul îl scrie seed-ul și îl stinge gardianul
            la prima editare — clientul îl ARATĂ, nu îl editează. */}
        <Column
          dataField="DinSeed"
          caption="Proveniență"
          width={110}
          allowEditing={false}
          calculateCellValue={(r: Record<string, unknown>) => (r.DinSeed ? 'seed' : 'manual')}
        />
      </DataGrid>

      {indiciu && <p className="indiciu">{indiciu}</p>}

      <PanouIstoric tipClr={tipClr ?? `${NAMESPACE_CLR}${entitate}`} id={randFocalizat} />
    </div>
  );
}

// Orice refuz — 403 al permisiunii, 404 al invizibilului, 422 al gardianului —
// ajunge în rândul grilei cu fraza serverului. `EroareDomeniu.message` e doar
// PRIMA linie; aici se arată toate, fiindcă gardianul poate refuza pe mai multe
// motive deodată.
async function scrie<T>(actiune: () => Promise<T>): Promise<T> {
  try {
    return await actiune();
  }
  catch (e) {
    throw new Error(eroriDin(e).join('\n'));
  }
}

// ── panoul „Istoric" (F23-D9) ───────────────────────────────────────────────
//
// Se cere prin `ia` din `http.ts`, NU prin `ODataStore`: aici contează și
// motivul unui refuz, iar pe conducta store-ului el s-ar pierde (80-r1).
//
// Forma interogării e cea MĂSURATĂ (pasul 2): `UserName`, `ObjectType` și
// `AuditedDefaultString` sunt `[NotMapped]` pe `AuditDataItemPersistent` și nu
// intră în EDM — un `$select=UserName` iese 400. Utilizatorul vine din
// `$expand=UserObject` ⇒ `UserObject.DefaultString`.
//
// `PropertyName` iese ca nume CLR (host-ul API n-are model de aplicație XAF —
// aceeași cauză ca 80-r5); se afișează ca atare, nu se traduce în TS.

type ReferintaAudit = { DefaultString?: string | null };
type RandAudit = {
  ID?: string;
  ModifiedOn?: string | null;
  OperationType?: string | null;
  PropertyName?: string | null;
  OldValue?: string | null;
  NewValue?: string | null;
  UserObject?: ReferintaAudit | null;
};

const LIMITA_ISTORIC = 20;

function caleAudit(tipClr: string, id: string): string {
  const filtru = `AuditedObject/TypeName eq '${tipClr}' and AuditedObject/Key eq '${id}'`;
  return '/api/odata/AuditDataItemPersistent'
    + `?$filter=${encodeURIComponent(filtru)}`
    + '&$expand=UserObject'
    + `&$orderby=${encodeURIComponent('ModifiedOn desc')}`
    + `&$top=${LIMITA_ISTORIC}`;
}

function PanouIstoric(props: { tipClr: string; id: string | null }) {
  const { tipClr, id } = props;
  const istoric = useQuery({
    queryKey: ['audit', tipClr, id],
    queryFn: () => ia<{ value?: RandAudit[] }>(caleAudit(tipClr, id!)),
    enabled: id != null && id !== '',
  });

  if (!id) return <p className="indiciu">Selectați un rând pentru a-i vedea istoricul.</p>;

  const randuri = istoric.data?.value ?? [];
  return (
    <section className="istoric">
      <h3>Istoric (ultimele {LIMITA_ISTORIC} modificări)</h3>
      {istoric.isError && <p className="indiciu">{eroriDin(istoric.error).join(' ')}</p>}
      {!istoric.isError && istoric.isPending && <p className="indiciu">Se citește…</p>}
      {/* Rolul `Default` vede în jurnal doar rândurile PROPRII (securitatea XAF),
          deci lista goală poate însemna „nimic nu s-a schimbat" SAU „nu vedeți
          modificările altora". Textul nu alege între ele — n-are cum. */}
      {!istoric.isError && !istoric.isPending && randuri.length === 0
        && <p className="indiciu">Fără istoric vizibil pentru acest rând.</p>}
      {randuri.length > 0 && (
        <table className="tabel-mic">
          <thead>
            <tr>
              <th>Când</th><th>Cine</th><th>Operația</th><th>Câmpul</th><th>Din</th><th>În</th>
            </tr>
          </thead>
          <tbody>
            {randuri.map((r, i) => (
              <tr key={r.ID ?? i}>
                <td>{r.ModifiedOn ? new Date(r.ModifiedOn).toLocaleString('ro-RO') : ''}</td>
                <td>{r.UserObject?.DefaultString ?? ''}</td>
                <td>{r.OperationType ?? ''}</td>
                <td>{r.PropertyName ?? ''}</td>
                <td>{r.OldValue ?? ''}</td>
                <td>{r.NewValue ?? ''}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  );
}
