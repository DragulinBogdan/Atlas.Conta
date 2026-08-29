import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Column, DataGrid } from 'devextreme-react/data-grid';
import { DocumentShell, type Comanda } from '../../nucleu/DocumentShell';
import { Formular, eroriStructurale } from '../../nucleu/formular';
import { CampData, CampText } from '../../nucleu/campuri';
import { Lookup } from '../../nucleu/Lookup';
import { PanouErori } from '../../nucleu/PanouErori';
import { campMeta, labelEnum } from '../../nucleu/campMeta';
import { eroriDin } from '../../nucleu/http';
import {
  antetGol, btr, spreWrite,
  SCHEMA_ANTET, SCHEMA_LINIE, TIP_ANTET, TIP_LINIE,
  type BtrLinieWrite, type BtrWrite,
} from './api';
import { EditorLinie } from './EditorLinie';

// Felia verticală BTR, ecranul de document. Starea (43c):
//   (1) server-read  → `useQuery` pe ReadDto (affordances, Total, etichete);
//   (2) formular     → `agregat`: WriteDto ÎNTREG, o singură valoare locală;
//   (3) efemeride    → linia în editare, erorile afișate.
// URL-ul e starea globală: `/btr/nou` și `/btr/:id`.

const CAMPURI_ANTET: (keyof BtrWrite & string)[] = ['Data', 'PredatorId', 'PrimitorId', 'NumarPV', 'DataPV'];
const capLinie = (m: string) => campMeta(TIP_LINIE, m, SCHEMA_LINIE).caption;

export function BtrDetaliu() {
  const { id } = useParams();
  const nou = id === 'nou' || id === undefined;
  const navigheaza = useNavigate();
  const cache = useQueryClient();

  const [agregat, setAgregat] = useState<BtrWrite>(antetGol);
  const [modificat, setModificat] = useState(false);
  const [aratErori, setAratErori] = useState(false);
  const [erori, setErori] = useState<string[]>([]);
  const [mesaje, setMesaje] = useState<string[]>([]);
  const [inEditare, setInEditare] = useState<BtrLinieWrite | null>(null);
  const [indiceEditat, setIndiceEditat] = useState<number | null>(null);

  const citit = useQuery({
    queryKey: ['btr', id],
    queryFn: () => btr.citeste(id!),
    enabled: !nou,
  });

  // ReadDto proaspăt ⇒ formularul se re-seed-uiește. Nu e sincronizare de
  // store: e o singură direcție, server → agregat, la fiecare recitire.
  useEffect(() => {
    if (citit.data) {
      setAgregat(spreWrite(citit.data));
      setModificat(false);
    }
  }, [citit.data]);

  const doc = citit.data;
  const poateEdita = nou || (doc?.PoateEdita ?? false);
  const linii = agregat.Linii ?? [];

  const structurale = useMemo(
    () => [
      ...eroriStructurale(TIP_ANTET, SCHEMA_ANTET, agregat as Record<string, unknown>, CAMPURI_ANTET),
      ...(linii.length === 0 ? ['Documentul nu are nicio linie.'] : []),
    ],
    [agregat, linii.length]);

  function raporteaza(promisiune: Promise<string[]>) {
    setErori([]);
    setMesaje([]);
    return promisiune
      .then((m) => setMesaje(m))
      .catch((e) => setErori(eroriDin(e)));
  }

  const salvare = useMutation({
    mutationFn: async () => (nou ? btr.creeaza(agregat) : btr.actualizeaza(id!, agregat)),
    onSuccess: (salvat) => {
      setErori([]);
      setMesaje(['Salvat.']);
      setModificat(false);
      cache.invalidateQueries({ queryKey: ['btr'] });
      if (nou) navigheaza(`/btr/${salvat.Id}`, { replace: true });
      else setAgregat(spreWrite(salvat));
    },
    onError: (e) => { setMesaje([]); setErori(eroriDin(e)); },
  });

  function salveaza() {
    setAratErori(true);
    if (structurale.length > 0) {
      setErori([]);
      setMesaje([]);
      return;
    }
    salvare.mutate();
  }

  async function comanda(rulare: () => Promise<{ Mesaje?: string[] | null; StareNoua?: string | null }>) {
    setErori([]);
    setMesaje([]);
    try {
      const rezultat = await rulare();
      setMesaje([`Stare nouă: ${labelEnum('StareDocument', rezultat.StareNoua)}`, ...(rezultat.Mesaje ?? [])]);
      await cache.invalidateQueries({ queryKey: ['btr'] });
    }
    catch (e) {
      setErori(eroriDin(e));
    }
  }

  const comenzi: Comanda[] = [
    { eticheta: nou ? 'Creează' : 'Salvează', disponibila: poateEdita, primara: true, ruleaza: salveaza },
    {
      eticheta: 'Verifică',
      disponibila: !nou && !modificat,
      ruleaza: () => void raporteaza(btr.valideaza(id!).then((e) => {
        if (e.length > 0) { setErori(e); return []; }
        return ['Documentul trece toți gardienii motorului.'];
      })),
    },
    { eticheta: 'Operează', disponibila: (doc?.PoateOpera ?? false) && !modificat, ruleaza: () => void comanda(() => btr.opereaza(id!)) },
    { eticheta: 'Anulează operarea', disponibila: doc?.PoateAnula ?? false, ruleaza: () => void comanda(() => btr.anuleaza(id!)) },
    {
      eticheta: 'Stornează',
      disponibila: doc?.PoateStorna ?? false,
      cereData: { eticheta: 'Data stornării' },
      ruleaza: (data) => { if (data) void comanda(() => btr.storneaza(id!, data)); },
    },
    { eticheta: 'Înapoi la listă', disponibila: true, ruleaza: () => navigheaza('/btr') },
  ];

  function schimbaAntet(v: BtrWrite) {
    setAgregat(v);
    setModificat(true);
  }

  function salveazaLinie(linie: BtrLinieWrite) {
    const urmatoare = [...linii];
    if (indiceEditat == null) urmatoare.push(linie);
    else urmatoare[indiceEditat] = linie;
    setAgregat({ ...agregat, Linii: urmatoare });
    setModificat(true);
    setInEditare(null);
    setIndiceEditat(null);
  }

  function stergeLinie(indice: number) {
    setAgregat({ ...agregat, Linii: linii.filter((_, i) => i !== indice) });
    setModificat(true);
  }

  return (
    <DocumentShell
      titlu={nou ? 'Notă de transfer — nouă' : `Notă de transfer ${doc?.Numar ?? ''}`}
      sumar={<Sumar
        stare={doc?.Stare}
        total={doc?.Total}
        modificat={modificat || nou}
      />}
      comenzi={comenzi}
      erori={erori}
      mesaje={mesaje}
      ocupat={salvare.isPending || citit.isFetching}
      antet={
        <>
          {aratErori && <PanouErori erori={structurale} titlu="De completat înainte de salvare" />}
          <Formular
            tip={TIP_ANTET}
            schema={SCHEMA_ANTET}
            valoare={agregat}
            onSchimba={schimbaAntet}
            readOnly={!poateEdita}
            aratErori={aratErori}
          >
            <div className="grila-campuri">
              <CampData<BtrWrite> camp="Data" />
              <Lookup<BtrWrite> camp="PredatorId" entitate="Gestiune" mod="local" />
              <Lookup<BtrWrite> camp="PrimitorId" entitate="Gestiune" mod="local" />
              <CampText<BtrWrite> camp="NumarPV" />
              <CampData<BtrWrite> camp="DataPV" />
            </div>
          </Formular>
        </>
      }
      linii={
        <>
          <div className="linii__bara">
            <h3>Linii</h3>
            <button
              type="button"
              className="buton"
              disabled={!poateEdita || inEditare != null}
              onClick={() => { setIndiceEditat(null); setInEditare({ Cantitate: 0 }); }}
            >
              Adaugă linie
            </button>
          </div>

          {/* Grilă READONLY peste liniile agregatului local: nu vorbește cu
              serverul, nu editează. Etichetele bogate (lot, valoare) vin din
              ReadDto — server-owned; liniile nesalvate le arată goale. */}
          <DataGrid
            dataSource={linii.map((l, i) => ({ ...l, __indice: i, ...etichete(doc?.Linii, l) }))}
            keyExpr="__indice"
            showBorders
            columnAutoWidth
            onRowClick={(e) => {
              if (!poateEdita) return;
              const rand = e.data as BtrLinieWrite & { __indice: number };
              setIndiceEditat(rand.__indice);
              setInEditare(linii[rand.__indice]);
            }}
          >
            <Column dataField="TipMaterialDenumire" caption={capLinie('TipMaterialId')} />
            <Column dataField="LotEticheta" caption={capLinie('LotId')} />
            <Column dataField="Cantitate" caption={capLinie('Cantitate')} dataType="number" format="#,##0.###" alignment="right" />
            {/* `Valoare` = preț lot × cantitate, materializată de motor. */}
            <Column dataField="Valoare" caption="Valoare (server)" dataType="number" format="#,##0.00" alignment="right" />
            <Column
              caption=""
              width={90}
              cellRender={(c) => (
                <button
                  type="button"
                  className="buton buton--mic"
                  disabled={!poateEdita}
                  onClick={(ev) => { ev.stopPropagation(); stergeLinie((c.data as { __indice: number }).__indice); }}
                >
                  Șterge
                </button>
              )}
            />
          </DataGrid>

          {inEditare && (
            // `key` = poziția editată: schimbarea liniei REMONTEAZĂ editorul, ca
            // starea lui locală să nu rămână a liniei precedente.
            <EditorLinie
              key={indiceEditat ?? 'linie-noua'}
              linie={inEditare}
              readOnly={!poateEdita}
              onSalveaza={salveazaLinie}
              onRenunta={() => { setInEditare(null); setIndiceEditat(null); }}
            />
          )}
        </>
      }
    />
  );
}

// Etichetele server-owned ale unei linii (denumire tip, etichetă lot, valoare):
// se caută în ReadDto după `Id`. Linia nouă nu are încă niciuna — corect: nu le
// inventăm în TS.
function etichete(citite: { Id?: string; TipMaterialDenumire?: string | null; LotEticheta?: string | null; Valoare?: number }[] | null | undefined,
  linie: BtrLinieWrite) {
  const g = citite?.find((c) => c.Id === linie.Id);
  return {
    TipMaterialDenumire: g?.TipMaterialDenumire ?? '',
    LotEticheta: g?.LotEticheta ?? '',
    Valoare: g?.Valoare,
  };
}

// `Total` = valoarea din ReadDto, redusă de SERVER. La editare nesalvată nu
// există (baza s-a schimbat) — se marchează explicit, nu se recalculează în TS
// (43c: „clientul nu ține niciodată un sold").
function Sumar(props: { stare?: string | null; total?: number; modificat: boolean }) {
  return (
    <div className="sumar">
      <span className="sumar__stare">{labelEnum('StareDocument', props.stare) || 'nesalvat'}</span>
      <span className="sumar__total">
        Total: {props.modificat || props.total == null
          ? <em title="Totalul îl calculează serverul la salvare.">— recalculat la salvare</em>
          : props.total.toFixed(2)}
      </span>
    </div>
  );
}
