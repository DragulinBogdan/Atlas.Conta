import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { useNavigate, useParams } from 'react-router';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Column, DataGrid } from 'devextreme-react/data-grid';
import { DocumentShell, type Comanda } from '../../nucleu/DocumentShell';
import { CampShell } from '../../nucleu/CampShell';
import { Formular, eroriStructurale } from '../../nucleu/formular';
import { CampData } from '../../nucleu/campuri';
import { Lookup } from '../../nucleu/Lookup';
import { PanouErori } from '../../nucleu/PanouErori';
import { campMeta, labelEnum } from '../../nucleu/campMeta';
import { eroriDin } from '../../nucleu/http';
import {
  antetGol, linieGoala, rdc, spreWrite,
  SCHEMA_ANTET, SCHEMA_LINIE, TIP_ANTET, TIP_LINIE,
  type RdcLinieRead, type RdcLinieWrite, type RdcWrite,
} from './api';
import { ETICHETA_ROL, RdcEditorLinie, rolLiniei, type EticheteCulese } from './RdcEditorLinie';

// Felia verticală RDC, ecranul de document — șablonul consolidat (43c):
//   (1) server-read  → `useQuery` pe ReadDto (affordances, cele două totaluri);
//   (2) formular     → `agregat`: WriteDto ÎNTREG, o singură valoare locală;
//   (3) efemeride    → linia în editare, erorile afișate.
// URL-ul e starea globală: `/rdc/nou` și `/rdc/:id`.
//
// ═══ Două totaluri, arătate ca două cifre ═══
// `Total` = DOAR liniile de venit (brutul care ajustează creanța); `TotalCost` =
// Σ liniilor cu lot. Amândouă vin din ReadDto (F19-D9) și NU se adună niciodată
// pe ecran: documentul valorează „−121 creanță + −30 cost", iar o singură cifră
// ar minți despre amândouă.
//
// ═══ Semnul ═══ Ca la RLF: culegere POZITIVĂ (cifra de pe factura originală),
// semnarea storno e fapta OPERĂRII. Ecranul afișează ce întoarce serverul, în
// ambele stări.
//
// ═══ Fără panou de stingeri (F19-D11) ═══ `ReturClient` e EXCLUS deliberat din
// `DocumenteCuRest` (`LiniiCreanta` e al TIPULUI, nu al coloanei). Compensarea
// unui retur cu factura originală se face prin NOTĂ CONTABILĂ (`/ntc`).

const CAMPURI_ANTET: (keyof RdcWrite & string)[] = ['Data', 'PredatorId', 'PrimitorId'];
const capLinie = (m: string) => campMeta(TIP_LINIE, m, SCHEMA_LINIE).caption;

export function RdcDetaliu() {
  const { id } = useParams();
  const nou = id === 'nou' || id === undefined;
  const navigheaza = useNavigate();
  const cache = useQueryClient();

  const [agregat, setAgregat] = useState<RdcWrite>(antetGol);
  const [modificat, setModificat] = useState(false);
  const [aratErori, setAratErori] = useState(false);
  const [erori, setErori] = useState<string[]>([]);
  const [mesaje, setMesaje] = useState<string[]>([]);
  const [inEditare, setInEditare] = useState<RdcLinieWrite | null>(null);
  const [indiceEditat, setIndiceEditat] = useState<number | null>(null);
  // Etichetele CULESE în sesiunea asta, per POZIȚIE de linie (paralel cu
  // `agregat.Linii`).
  const [eticheteLinii, setEticheteLinii] = useState<(EticheteCulese | undefined)[]>([]);

  const citit = useQuery({
    queryKey: ['rdc', id],
    queryFn: () => rdc.citeste(id!),
    enabled: !nou,
  });

  // ReadDto proaspăt ⇒ formularul se re-seed-uiește. O singură direcție,
  // server → agregat, la fiecare recitire.
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
    mutationFn: async () => (nou ? rdc.creeaza(agregat) : rdc.actualizeaza(id!, agregat)),
    onSuccess: (salvat) => {
      setErori([]);
      setMesaje(['Salvat.']);
      setModificat(false);
      cache.invalidateQueries({ queryKey: ['rdc'] });
      if (nou) navigheaza(`/rdc/${salvat.Id}`, { replace: true });
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
      await cache.invalidateQueries({ queryKey: ['rdc'] });
      // Marfa revine în gestiune: soldul afișat în altă parte nu mai e valabil.
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
      await rdc.sterge(id!);
      await cache.invalidateQueries({ queryKey: ['rdc'] });
      navigheaza('/rdc');
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
      ruleaza: () => void raporteaza(rdc.valideaza(id!).then((e) => {
        if (e.length > 0) { setErori(e); return []; }
        return ['Documentul trece toți gardienii motorului.'];
      })),
    },
    { eticheta: 'Operează', disponibila: (doc?.PoateOpera ?? false) && !modificat, ruleaza: () => void comanda(() => rdc.opereaza(id!)) },
    { eticheta: 'Anulează operarea', disponibila: doc?.PoateAnula ?? false, ruleaza: () => void comanda(() => rdc.anuleaza(id!)) },
    {
      eticheta: 'Stornează',
      disponibila: doc?.PoateStorna ?? false,
      cereData: { eticheta: 'Data stornării' },
      ruleaza: (data) => { if (data) void comanda(() => rdc.storneaza(id!, data)); },
    },
    { eticheta: 'Șterge', disponibila: !nou && poateEdita, ruleaza: () => void stergeDocumentul() },
    { eticheta: 'Înapoi la listă', disponibila: true, ruleaza: () => navigheaza('/rdc') },
  ];

  function schimbaAntet(v: RdcWrite) {
    setAgregat(v);
    setModificat(true);
  }

  function salveazaLinie(linie: RdcLinieWrite, culese: EticheteCulese) {
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
      titlu={nou ? 'Retur de la client — nou' : `Retur de la client ${doc?.Numar ?? ''}`}
      sumar={(
        <Sumar
          stare={doc?.Stare}
          total={doc?.Total}
          totalCost={doc?.TotalCost}
          modificat={modificat || nou}
        />
      )}
      comenzi={comenzi}
      erori={erori}
      mesaje={mesaje}
      ocupat={salvare.isPending || citit.isFetching}
      antet={
        <>
          {aratErori && <PanouErori erori={structurale} titlu="De completat înainte de salvare" />}

          {/* Documentul e UNUL, cu linii pe două roluri — și cele două cifre nu
              se adună. Spus o dată, aici, ca sumarul de sus să fie citibil. */}
          <p className="indiciu">
            Documentul are linii pe două feluri: <strong>venit</strong> (venitul stornat, cu TVA — el
            ajustează creanța și dă „Total”) și <strong>marfă returnată</strong> (marfa care revine pe
            lotul original, fără TVA — ea dă „Cost”). Cele două cifre nu se adună.
            {doc?.Stare === 'Draft' || nou
              ? ' Cifrele se culeg POZITIV; semnul minus îl pune operarea.'
              : ' Cifrele sunt negative fiindcă documentul e operat: returul se postează ca valori negative pe corespondența vânzării.'}
          </p>

          <Formular
            tip={TIP_ANTET}
            schema={SCHEMA_ANTET}
            valoare={agregat}
            onSchimba={schimbaAntet}
            readOnly={!poateEdita}
            aratErori={aratErori}
          >
            <div className="grila-campuri">
              {/* `Numar` e AFIȘARE: RDC are politică de numerotare („RDC-"),
                  deci seria se asignează la operare și nici nu există în
                  WriteDto (F19-D6). */}
              <Static membru="Numar" valoare={doc?.Numar} />
              <CampData<RdcWrite> camp="Data" />
              <Lookup<RdcWrite>
                camp="PredatorId"
                entitate="Partener"
                mod="remote"
                eticheta="Clientul care returnează"
                cauta={['Denumire', 'Cod', 'CodFiscal']}
              />
              <div>
                <Lookup<RdcWrite>
                  camp="PrimitorId"
                  entitate="Gestiune"
                  mod="local"
                  eticheta="Gestiunea în care revine marfa"
                  cauta={['Cod', 'Denumire']}
                />
                <p className="indiciu">
                  Marfa revine pe lotul ORIGINAL; gestiunea e cea în care intră soldul.
                </p>
              </div>
              <Static membru="Stare" valoare={labelEnum('StareDocument', doc?.Stare)} />
              <Static membru="DataOperare" valoare={doc?.DataOperare?.slice(0, 10)} />
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
              serverul, nu editează. Coloana „Fel" e ACEEAȘI traducere pe care o
              face editorul (`rolLiniei` — prezența lotului), nu o a doua regulă. */}
          <DataGrid
            dataSource={linii.map((l, i) => ({ ...l, __indice: i, ...etichete(doc?.Linii, l, eticheteLinii[i]) }))}
            keyExpr="__indice"
            showBorders
            columnAutoWidth
            onRowClick={(e) => {
              if (!poateEdita) return;
              const rand = e.data as RdcLinieWrite & { __indice: number };
              setIndiceEditat(rand.__indice);
              setInEditare(linii[rand.__indice]);
            }}
          >
            <Column
              caption="Fel"
              width={190}
              cellRender={(c) => ETICHETA_ROL[rolLiniei(c.data as { LotId?: string | null })]}
            />
            <Column dataField="TipMaterialCod" caption="Cod tip" />
            <Column dataField="TipMaterialDenumire" caption={capLinie('TipMaterialId')} />
            <Column dataField="LotEticheta" caption={capLinie('LotId')} />
            <Column dataField="Cantitate" caption={capLinie('Cantitate')} dataType="number" format="#,##0.###" alignment="right" />
            {/* `Valoare`: pe venit e cifra CULEASĂ (venitul stornat), pe marfă e
                costul lotului scris de SERVER (F19-D8). Coloana arată ce a
                întors serverul pe linia salvată — nu se calculează în TS. */}
            <Column dataField="Valoare" caption={`${capLinie('Valoare')} (server)`} dataType="number" format="#,##0.00" alignment="right" />
            <Column dataField="TipTvaCod" caption={capLinie('TipTvaId')} />
            <Column dataField="ValoareTva" caption={`${capLinie('ValoareTva')} (server)`} dataType="number" format="#,##0.00" alignment="right" />
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
            // starea lui locală (rolul inclus) să nu rămână a liniei precedente.
            <RdcEditorLinie
              key={indiceEditat ?? 'linie-noua'}
              linie={inEditare}
              valoareTvaCitita={doc?.Linii?.find((c) => c.Id === inEditare.Id)?.ValoareTva}
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
// culegere rămâne goală — nu le inventăm în TS.
function etichete(citite: RdcLinieRead[] | null | undefined, linie: RdcLinieWrite, culese?: EticheteCulese) {
  const g = citite?.find((c) => c.Id === linie.Id);
  return {
    TipMaterialCod: culese?.TipMaterialCod ?? g?.TipMaterialCod ?? '',
    TipMaterialDenumire: culese?.TipMaterialDenumire ?? g?.TipMaterialDenumire ?? '',
    LotEticheta: culese?.LotEticheta ?? g?.LotEticheta ?? '',
    TipTvaCod: culese?.TipTvaCod ?? g?.TipTvaCod ?? '',
    Valoare: g?.Valoare,
    ValoareTva: g?.ValoareTva,
  };
}

// Câmp de AFIȘARE în interiorul formularului: aceeași ramă ca la culegere, cu
// text în locul editorului.
function Static(props: { membru: string; valoare: ReactNode }) {
  const meta = { ...campMeta(TIP_ANTET, props.membru, SCHEMA_ANTET), obligatoriu: false };
  return (
    <CampShell meta={meta}>
      <div className="valoare-statica">{props.valoare == null || props.valoare === '' ? '—' : props.valoare}</div>
    </CampShell>
  );
}

// Cele DOUĂ cifre ale documentului, alăturate și numite. `Total` e brutul
// liniilor de venit (ce ajustează creanța), `Cost` e Σ liniilor cu lot — sunt
// calculate de SERVER și nu se adună niciodată aici (42c).
function Sumar(props: { stare?: string | null; total?: number; totalCost?: number; modificat: boolean }) {
  return (
    <div className="sumar">
      <span className="sumar__stare">{labelEnum('StareDocument', props.stare) || 'nesalvat'}</span>
      <span className="sumar__total" title="Brutul liniilor de venit — cifra care ajustează creanța clientului.">
        Total (venit): {cifra(props.total, props.modificat)}
      </span>
      <span className="sumar__total" title="Σ liniilor cu lot — mișcare internă venit↔stoc, nu creanță.">
        Cost (marfă returnată): {cifra(props.totalCost, props.modificat)}
      </span>
    </div>
  );
}

function cifra(valoare: number | undefined, modificat: boolean): ReactNode {
  if (modificat || valoare == null)
    return <em title="Cifrele le calculează serverul la salvare.">— recalculat la salvare</em>;
  return valoare.toFixed(2);
}
