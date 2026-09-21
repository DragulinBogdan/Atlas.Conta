import { useMemo, type ComponentProps, type ReactNode } from 'react';
import { useNavigate } from 'react-router';
import DataSource from 'devextreme/data/data_source';
import type CustomStore from 'devextreme/data/custom_store';
import { GrilaDocumente, INDICIU_GRILA } from './GrilaDocumente';
import { useUrlStare } from './urlStare';
import { CasetaPerioada, lunaCurenta } from '../felii/raportare/comune';

type Props = {
  titlu: string;
  ruta: string;
  // Eticheta butonului de creare; lipsește pe tipurile care nu se culeg din client.
  nou?: string;
  store: () => CustomStore;
  // Interogarea filtrată pe `Data` în perioada din URL (43c).
  perioada?: boolean;
  indiciu?: ReactNode;
  children: ReactNode;
};

export function ListaDocumente(props: Props) {
  return props.perioada ? <ListaPePerioada {...props} /> : <ListaIntreaga {...props} />;
}

function ListaIntreaga(props: Props) {
  const { store } = props;
  const sursa = useMemo(() => store(), [store]);
  return <Ecran {...props} sursa={sursa} inaltime="calc(100vh - 170px)" />;
}

function ListaPePerioada(props: Props) {
  const { store } = props;
  const luna = lunaCurenta();
  const [stare, seteaza] = useUrlStare({ dataStart: luna.start, dataEnd: luna.sfarsit });

  const sursa = useMemo(() => new DataSource({
    store: store(),
    filter: [['Data', '>=', stare.dataStart], 'and', ['Data', '<=', stare.dataEnd]],
    sort: [{ selector: 'Data', desc: true }],
    paginate: true,
    pageSize: 25,
    requireTotalCount: true,
  }), [store, stare.dataStart, stare.dataEnd]);

  return (
    <Ecran
      {...props}
      sursa={sursa}
      inaltime="calc(100vh - 230px)"
      bara={(
        <div className="bara-raport">
          <CasetaPerioada dataStart={stare.dataStart} dataEnd={stare.dataEnd} seteaza={seteaza} />
        </div>
      )}
    />
  );
}

function Ecran(props: Props & {
  sursa: ComponentProps<typeof GrilaDocumente>['sursa'];
  inaltime: string;
  bara?: ReactNode;
}) {
  const { titlu, ruta, nou, indiciu, children, sursa, inaltime, bara } = props;
  const navigheaza = useNavigate();

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>{titlu}</h2>
        {nou && (
          <button type="button" className="buton buton--primar" onClick={() => navigheaza(`${ruta}/nou`)}>
            {nou}
          </button>
        )}
      </div>

      {bara}

      <GrilaDocumente sursa={sursa} ruta={ruta} inaltime={inaltime}>
        {children}
      </GrilaDocumente>

      <p className="indiciu">
        {INDICIU_GRILA}
        {indiciu && <> {indiciu}</>}
      </p>
    </div>
  );
}
