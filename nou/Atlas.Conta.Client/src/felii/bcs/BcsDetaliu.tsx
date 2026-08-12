import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Column, DataGrid } from 'devextreme-react/data-grid';
import { DocumentShell, type Comanda } from '../../nucleu/DocumentShell';
import { Formular, eroriStructurale } from '../../nucleu/formular';
import { CampData } from '../../nucleu/campuri';
import { Lookup } from '../../nucleu/Lookup';
import { PanouErori } from '../../nucleu/PanouErori';
import { campMeta, labelEnum } from '../../nucleu/campMeta';
import { eroriDin } from '../../nucleu/http';
import {
  antetGol, bcs, linieGoala, spreWrite,
  SCHEMA_ANTET, SCHEMA_LINIE, TIP_ANTET, TIP_LINIE,
  type BcsLinieRead, type BcsLinieWrite, type BcsWrite,
} from './api';
import { EditorLinie, type EticheteCulese } from './EditorLinie';

// Felia verticală BCS, ecranul de document — șablonul consolidat (43c):
//   (1) server-read  → `useQuery` pe ReadDto (affordances, Total, etichete);
//   (2) formular     → `agregat`: WriteDto ÎNTREG, o singură valoare locală;
//   (3) efemeride    → linia în editare, erorile afișate.
// URL-ul e starea globală: `/bcs/nou` și `/bcs/:id`.
//
// Consumul alimentează DOUĂ registre la operare (27a): −Magazie pe gestiunea
// predatoare și +Consum pe locul de consum primitor. Ecranul nu spune nimic
// despre asta — registrele sunt fapta motorului; el doar culege laturile.

const CAMPURI_ANTET: (keyof BcsWrite & string)[] = ['Data', 'PredatorId', 'PrimitorId'];
const capLinie = (m: string) => campMeta(TIP_LINIE, m, SCHEMA_LINIE).caption;

export function BcsDetaliu() {
  const { id } = useParams();
  const nou = id === 'nou' || id === undefined;
  const navigheaza = useNavigate();
  const cache = useQueryClient();

  const [agregat, setAgregat] = useState<BcsWrite>(antetGol);
  const [modificat, setModificat] = useState(false);
  const [aratErori, setAratErori] = useState(false);
  const [erori, setErori] = useState<string[]>([]);
  const [mesaje, setMesaje] = useState<string[]>([]);
  const [inEditare, setInEditare] = useState<BcsLinieWrite | null>(null);
  const [indiceEditat, setIndiceEditat] = useState<number | null>(null);
  // Etichetele CULESE în sesiunea asta, per POZIȚIE de linie (paralel cu
  // `agregat.Linii`): grila le folosește cât timp linia n-are încă etichete
  // server-owned în ReadDto (documentul/linia nesalvată).
  const [eticheteLinii, setEticheteLinii] = useState<(EticheteCulese | undefined)[]>([]);

  const citit = useQuery({
    queryKey: ['bcs', id],
    queryFn: () => bcs.citeste(id!),
    enabled: !nou,
  });

  // ReadDto proaspăt ⇒ formularul se re-seed-uiește. O singură direcție,
  // server → agregat, la fiecare recitire. Etichetele culese local mor odată cu
  // re-seed-ul: serverul le are de acum pe toate.
  useEffect(() => {
    if (citit.data) {
      setAgregat(spreWrite(citit.data));
      setModificat(false);
      setEticheteLinii([]);
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
    mutationFn: async () => (nou ? bcs.creeaza(agregat) : bcs.actualizeaza(id!, agregat)),
    onSuccess: (salvat) => {
      setErori([]);
      setMesaje(['Salvat.']);
      setModificat(false);
      cache.invalidateQueries({ queryKey: ['bcs'] });
      if (nou) navigheaza(`/bcs/${salvat.Id}`, { replace: true });
      // Serverul poate REORDONA liniile la recitire — etichetele per poziție nu
      // mai corespund; se golesc aici, iar cele server-owned vin cu refetch-ul.
      else { setAgregat(spreWrite(salvat)); setEticheteLinii([]); }
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
      await cache.invalidateQueries({ queryKey: ['bcs'] });
      // Consumul mișcă stocul (două registre): soldul afișat în altă parte nu
      // mai e valabil.
      await cache.invalidateQueries({ queryKey: ['stoc'] });
    }
    catch (e) {
      setErori(eroriDin(e));
    }
  }

  async function stergeDocumentul() {
    if (!window.confirm('Ștergeți definitiv acest draft, cu tot cu linii?')) return;
    setErori([]);
    setMesaje([]);
    try {
      await bcs.sterge(id!);
      await cache.invalidateQueries({ queryKey: ['bcs'] });
      navigheaza('/bcs');
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
      ruleaza: () => void raporteaza(bcs.valideaza(id!).then((e) => {
        if (e.length > 0) { setErori(e); return []; }
        return ['Documentul trece toți gardienii motorului.'];
      })),
    },
    { eticheta: 'Operează', disponibila: (doc?.PoateOpera ?? false) && !modificat, ruleaza: () => void comanda(() => bcs.opereaza(id!)) },
    { eticheta: 'Anulează operarea', disponibila: doc?.PoateAnula ?? false, ruleaza: () => void comanda(() => bcs.anuleaza(id!)) },
    {
      eticheta: 'Stornează',
      disponibila: doc?.PoateStorna ?? false,
      cereData: { eticheta: 'Data stornării' },
      ruleaza: (data) => { if (data) void comanda(() => bcs.storneaza(id!, data)); },
    },
    { eticheta: 'Șterge', disponibila: !nou && poateEdita, ruleaza: () => void stergeDocumentul() },
    { eticheta: 'Înapoi la listă', disponibila: true, ruleaza: () => navigheaza('/bcs') },
  ];

  function schimbaAntet(v: BcsWrite) {
    setAgregat(v);
    setModificat(true);
  }

  function salveazaLinie(linie: BcsLinieWrite, culese: EticheteCulese) {
    const urmatoare = [...linii];
    const et = [...eticheteLinii];
    if (indiceEditat == null) {
      urmatoare.push(linie);
      et[urmatoare.length - 1] = culese;
    }
    else {
      urmatoare[indiceEditat] = linie;
      et[indiceEditat] = { ...et[indiceEditat], ...culese };
    }
    setAgregat({ ...agregat, Linii: urmatoare });
    setEticheteLinii(et);
    setModificat(true);
    setInEditare(null);
    setIndiceEditat(null);
  }

  function stergeLinie(indice: number) {
    setAgregat({ ...agregat, Linii: linii.filter((_, i) => i !== indice) });
    setEticheteLinii(eticheteLinii.filter((_, i) => i !== indice));
    setModificat(true);
  }

  return (
    <DocumentShell
      titlu={nou ? 'Bon de consum — nou' : `Bon de consum ${doc?.Numar ?? ''}`}
      sumar={<Sumar stare={doc?.Stare} total={doc?.Total} modificat={modificat || nou} />}
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
              <CampData<BcsWrite> camp="Data" />
              <Lookup<BcsWrite> camp="PredatorId" entitate="Gestiune" mod="local" cauta={['Cod', 'Denumire']} />
              <div>
                {/* Primitorul e LOCUL DE CONSUM: un intern purtător al calității
                    `LocConsum` (27b). Lookup-ul e NEFILTRAT pe calitate (F6-D8):
                    filtrarea pe `Calitati` (flags) nu traversează lanțul
                    DevExtreme→OData, iar precedentul e cimentat — refuzul e al
                    motorului, cu mesaj de domeniu. */}
                <Lookup<BcsWrite> camp="PrimitorId" entitate="UnitateInterna" mod="local" cauta={['Cod', 'Denumire']} />
                <p className="indiciu">
                  Trebuie să fie un loc de consum (calitatea „Loc consum”) — lista nu e filtrată pe
                  calitate, iar operarea refuză alegerile greșite.
                </p>
              </div>
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
              onClick={() => { setIndiceEditat(null); setInEditare(linieGoala()); }}
            >
              Adaugă linie
            </button>
          </div>

          {/* Grilă READONLY peste liniile agregatului local: nu vorbește cu
              serverul, nu editează. Etichetele vin din ReadDto (server-owned)
              sau, pe liniile încă nesalvate, din ce a CULES editorul la selecție
              (nimic inventat în TS). */}
          <DataGrid
            dataSource={linii.map((l, i) => ({ ...l, __indice: i, ...etichete(doc?.Linii, l, eticheteLinii[i]) }))}
            keyExpr="__indice"
            showBorders
            columnAutoWidth
            onRowClick={(e) => {
              if (!poateEdita) return;
              const rand = e.data as BcsLinieWrite & { __indice: number };
              setIndiceEditat(rand.__indice);
              setInEditare(linii[rand.__indice]);
            }}
          >
            <Column dataField="TipMaterialDenumire" caption={capLinie('TipMaterialId')} />
            <Column dataField="LotEticheta" caption={capLinie('LotId')} />
            <Column dataField="Cantitate" caption={capLinie('Cantitate')} dataType="number" format="#,##0.###" alignment="right" />
            {/* `Valoare` = preț lot × cantitate, materializată de SERVER la
                culegere (F6-D6) — apare după Salvează, niciodată calculată în TS. */}
            <Column dataField="Valoare" caption={`${capLinie('Valoare')} (server)`} dataType="number" format="#,##0.00" alignment="right" />
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

// Etichetele unei linii: cele CULESE în sesiunea asta (alegerea proaspătă a
// operatorului) bat ReadDto-ul, care se caută după `Id`; linia nouă fără
// culegere rămâne goală — nu le inventăm în TS. `Valoare` n-are variantă
// culeasă: e a serverului prin construcție.
function etichete(citite: BcsLinieRead[] | null | undefined, linie: BcsLinieWrite, culese?: EticheteCulese) {
  const g = citite?.find((c) => c.Id === linie.Id);
  return {
    TipMaterialDenumire: culese?.TipMaterialDenumire ?? g?.TipMaterialDenumire ?? '',
    LotEticheta: culese?.LotEticheta ?? g?.LotEticheta ?? '',
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
