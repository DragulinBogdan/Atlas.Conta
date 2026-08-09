import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { Link, useNavigate, useParams } from 'react-router';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Column, DataGrid } from 'devextreme-react/data-grid';
import { DocumentShell, type Comanda } from '../../nucleu/DocumentShell';
import { Formular, eroriStructurale } from '../../nucleu/formular';
import { CampData, CampSelectie, CampText } from '../../nucleu/campuri';
import { PanouErori } from '../../nucleu/PanouErori';
import { PanouStingeri } from '../../nucleu/PanouStingeri';
import { campMeta, labelEnum } from '../../nucleu/campMeta';
import { eroriDin } from '../../nucleu/http';
import { rutaTip } from '../../nucleu/stingeri';
import {
  antetGol, linieGoala, spreWrite,
  SCHEMA_ANTET, SCHEMA_LINIE, TIP_LINIE,
  type ApiTrezorerie, type DocumentCopil, type TrzLinieRead, type TrzLinieWrite, type TrzRead, type TrzWrite,
} from './api';
import { TrezorerieEditorLinie } from './TrezorerieEditorLinie';

// Ecranul de document al trezoreriei — forma comună a celor două felii (același
// argument ca pe server: `TrezorerieApply` e generic, controllerele sunt
// subțiri). IDENTITATEA rămâne în felie și vine ca props: ruta, titlurile,
// captions-urile laturilor și **JSX-ul laturilor** — cine e contul propriu și
// cine e contrapartida se scrie acolo, în cod, nu se derivă aici.
//
// Peste șablonul BTR/FCT, felia asta aduce:
//   • `Numar` NU se culege (server-owned — PoliticaNumerotare): se AFIȘEAZĂ;
//   • `Autogenerat` + `DocumentSursa` — plata născută din factură (31e) e un
//     document normal, dar operatorul trebuie să vadă de unde vine;
//   • **panoul de STINGERI** pe document OPERAT: trezoreria e stingătorul
//     „clasic" (31d), deci rolul ei e `stinge`.

const CAMPURI_ANTET: (keyof TrzWrite & string)[] = ['Data', 'PredatorId', 'PrimitorId'];
const capLinie = (m: string) => campMeta(TIP_LINIE, m, SCHEMA_LINIE).caption;

export function TrezorerieDetaliu(props: {
  api: ApiTrezorerie;
  ruta: string;
  // Cheia de cache a feliei (`plt`/`inc`) — invalidarea listei după orice
  // comandă.
  cheieCache: string;
  // Tipul de METADATA (`Plata`/`Incasare`) — sursa captions-urilor.
  tip: string;
  titluNou: string;
  titluExistent: (numar: string) => string;
  // Laturile, scrise în felie: contul propriu + contrapartida, în ordinea
  // fluxului real (PLT: din ce cont → cui; INC: de la cine → în ce cont).
  laturi: ReactNode;
  // Contrapartida documentului, pentru candidații panoului de stingeri
  // (PLT: primitorul; INC: predatorul).
  contrapartida: (doc: TrzRead) => string | undefined;
}) {
  const { api, ruta, cheieCache, tip, titluNou, titluExistent, laturi, contrapartida } = props;
  const { id } = useParams();
  const nou = id === 'nou' || id === undefined;
  const navigheaza = useNavigate();
  const cache = useQueryClient();

  const [agregat, setAgregat] = useState<TrzWrite>(antetGol);
  const [modificat, setModificat] = useState(false);
  const [aratErori, setAratErori] = useState(false);
  const [erori, setErori] = useState<string[]>([]);
  const [mesaje, setMesaje] = useState<string[]>([]);
  const [inEditare, setInEditare] = useState<TrzLinieWrite | null>(null);
  const [indiceEditat, setIndiceEditat] = useState<number | null>(null);

  const citit = useQuery({
    queryKey: [cheieCache, id],
    queryFn: () => api.citeste(id!),
    enabled: !nou,
  });

  // ReadDto proaspăt ⇒ formularul se re-seed-uiește. O singură direcție,
  // server → agregat, la fiecare recitire.
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
      ...eroriStructurale(tip, SCHEMA_ANTET, agregat as Record<string, unknown>, CAMPURI_ANTET),
      ...(linii.length === 0 ? ['Documentul nu are nicio linie.'] : []),
    ],
    [tip, agregat, linii.length]);

  function raporteaza(promisiune: Promise<string[]>) {
    setErori([]);
    setMesaje([]);
    return promisiune
      .then((m) => setMesaje(m))
      .catch((e) => setErori(eroriDin(e)));
  }

  const salvare = useMutation({
    mutationFn: async () => (nou ? api.creeaza(agregat) : api.actualizeaza(id!, agregat)),
    onSuccess: (salvat) => {
      setErori([]);
      setMesaje(['Salvat.']);
      setModificat(false);
      cache.invalidateQueries({ queryKey: [cheieCache] });
      if (nou) navigheaza(`${ruta}/${salvat.Id}`, { replace: true });
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
      await cache.invalidateQueries({ queryKey: [cheieCache] });
      // Operarea unei plăți autogenerate creează imperecherea automată (31e), iar
      // anularea/stornarea o desface — panoul de stingeri al documentului (și al
      // facturii-sursă, dacă e deschisă în altă filă) nu mai e de încredere.
      await cache.invalidateQueries({ queryKey: ['stingeri'] });
      await cache.invalidateQueries({ queryKey: ['fct'] });
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
      await api.sterge(id!);
      await cache.invalidateQueries({ queryKey: [cheieCache] });
      navigheaza(ruta);
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
      ruleaza: () => void raporteaza(api.valideaza(id!).then((e) => {
        if (e.length > 0) { setErori(e); return []; }
        return ['Documentul trece toți gardienii motorului.'];
      })),
    },
    { eticheta: 'Operează', disponibila: (doc?.PoateOpera ?? false) && !modificat, ruleaza: () => void comanda(() => api.opereaza(id!)) },
    // `PoateAnula`/`PoateStorna` sunt ONESTE (F3-D2): țin cont de imperecheri —
    // un document stins nu se anulează până nu se șterge legătura din panou.
    { eticheta: 'Anulează operarea', disponibila: doc?.PoateAnula ?? false, ruleaza: () => void comanda(() => api.anuleaza(id!)) },
    {
      eticheta: 'Stornează',
      disponibila: doc?.PoateStorna ?? false,
      cereData: { eticheta: 'Data stornării' },
      ruleaza: (data) => { if (data) void comanda(() => api.storneaza(id!, data)); },
    },
    { eticheta: 'Șterge', disponibila: !nou && poateEdita, ruleaza: () => void stergeDocumentul() },
    { eticheta: 'Înapoi la listă', disponibila: true, ruleaza: () => navigheaza(ruta) },
  ];

  function schimbaAntet(v: TrzWrite) {
    setAgregat(v);
    setModificat(true);
  }

  function salveazaLinie(linie: TrzLinieWrite) {
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
      titlu={nou ? titluNou : titluExistent(doc?.Numar ?? '')}
      sumar={<Sumar doc={doc} modificat={modificat || nou} />}
      comenzi={comenzi}
      erori={erori}
      mesaje={mesaje}
      rezultatExtra={<Provenienta doc={doc} />}
      ocupat={salvare.isPending || citit.isFetching}
      antet={
        <>
          {aratErori && <PanouErori erori={structurale} titlu="De completat înainte de salvare" />}
          <Formular
            tip={tip}
            schema={SCHEMA_ANTET}
            valoare={agregat}
            onSchimba={schimbaAntet}
            readOnly={!poateEdita}
            aratErori={aratErori}
          >
            <div className="grila-campuri">
              <CampData<TrzWrite> camp="Data" />
              {laturi}
              {/* Instrumentul e ENUM pe sârmă ca STRING (F3-D1): valorile vin din
                  metadata, nu dintr-o listă ținută în client. */}
              <CampSelectie<TrzWrite> camp="TipInstrument" enumerare="TipInstrumentPlata" />
              <CampText<TrzWrite> camp="NumarExtras" />
              <CampData<TrzWrite> camp="DataExtras" />
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

          {/* Grilă READONLY peste liniile agregatului local (43c). Codurile de
              dimensiune sunt etichete server-owned: apar după salvare. */}
          <DataGrid
            dataSource={linii.map((l, i) => ({ ...l, __indice: i, ...etichete(doc?.Linii, l) }))}
            keyExpr="__indice"
            showBorders
            columnAutoWidth
            onRowClick={(e) => {
              if (!poateEdita) return;
              const rand = e.data as TrzLinieWrite & { __indice: number };
              setIndiceEditat(rand.__indice);
              setInEditare(linii[rand.__indice]);
            }}
          >
            <Column dataField="TipMaterialCod" caption="Cod tip" />
            <Column dataField="TipMaterialDenumire" caption={capLinie('TipMaterialId')} />
            <Column dataField="Valoare" caption={capLinie('Valoare')} dataType="number" format="#,##0.00" alignment="right" />
            <Column dataField="CodEconomicCod" caption={capLinie('CodEconomicId')} />
            <Column dataField="SursaFinantareCod" caption={capLinie('SursaFinantareId')} />
            <Column dataField="CodFunctionalCod" caption={capLinie('CodFunctionalId')} />
            <Column dataField="ProiectCod" caption={capLinie('ProiectId')} />
            <Column dataField="AngajamentCod" caption={capLinie('AngajamentId')} />
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
            // `key` = poziția editată: schimbarea liniei REMONTEAZĂ editorul (și
            // cu el precompletarea TRZ a liniei noi).
            <TrezorerieEditorLinie
              key={indiceEditat ?? 'linie-noua'}
              linie={inEditare}
              readOnly={!poateEdita}
              onSalveaza={salveazaLinie}
              onRenunta={() => { setInEditare(null); setIndiceEditat(null); }}
            />
          )}
        </>
      }
      subsol={doc && doc.Id && doc.Stare === 'Operat'
        ? (
          <PanouStingeri
            documentId={doc.Id}
            contrapartidaId={contrapartida(doc)}
            // Trezoreria e STINGĂTORUL (31d): plata stinge factura.
            rol="stinge"
            // Trezoreria stinge creanțe/datorii, nu alte documente de
            // trezorerie (F3-D6a; INC↔PLT — avans↔regularizare — merge la
            // momentul 581, aditiv).
            tipuriCandidate={['FCT', 'FCL', 'DEC']}
            onSchimbare={() => void cache.invalidateQueries({ queryKey: [cheieCache] })}
          />
        )
        : undefined}
    />
  );
}

// Etichetele server-owned ale unei linii: se caută în ReadDto după `Id`. Linia
// nouă nu are încă niciuna — corect: nu le inventăm în TS.
function etichete(citite: TrzLinieRead[] | null | undefined, linie: TrzLinieWrite) {
  const g = citite?.find((c) => c.Id === linie.Id);
  return {
    TipMaterialCod: g?.TipMaterialCod ?? '',
    TipMaterialDenumire: g?.TipMaterialDenumire ?? '',
    CodEconomicCod: g?.CodEconomicCod ?? '',
    SursaFinantareCod: g?.SursaFinantareCod ?? '',
    CodFunctionalCod: g?.CodFunctionalCod ?? '',
    ProiectCod: g?.ProiectCod ?? '',
    AngajamentCod: g?.AngajamentCod ?? '',
  };
}

// De unde vine documentul și ce a mai născut el: plata autogenerată (31e) are
// factura-sursă, iar un document de trezorerie poate avea la rândul lui copii.
// Link, nu redirect: operatorul decide când trece pe celălalt ecran.
function Provenienta(props: { doc?: TrzRead }) {
  const doc = props.doc;
  const copii = doc?.Copii ?? [];
  if (!doc || (!doc.DocumentSursaId && copii.length === 0)) return null;
  return (
    <div className="panou panou--succes">
      <div className="panou__titlu">Grup conex</div>
      <ul className="panou__lista">
        {doc.DocumentSursaId && (
          <li>
            Generat din{' '}
            <Link to={`/fct/${doc.DocumentSursaId}`}>{doc.DocumentSursaNumar || 'documentul sursă'}</Link>
            {doc.Autogenerat ? ' (autogenerat de motor)' : ''}
            {/* D-5b: draftul autogenerat e artefact al operării sursei — anularea
                ei îl ȘTERGE cu tot cu modificările manuale (26d/31e). */}
            {doc.Autogenerat && doc.Stare === 'Draft' && (
              <div className="indiciu">
                Draft autogenerat: dacă anulați operarea documentului-sursă, draftul se șterge
                — inclusiv modificările făcute aici. Re-operarea sursei îl regenerează curat.
              </div>
            )}
          </li>
        )}
        {copii.map((c: DocumentCopil) => (
          <li key={c.Id}>
            <Legatura copil={c} /> — {labelEnum('StareDocument', c.Stare)}{c.Autogenerat ? ', autogenerat' : ''}
          </li>
        ))}
      </ul>
    </div>
  );
}

function Legatura(props: { copil: DocumentCopil }) {
  const { Id, Tip, Numar } = props.copil;
  const ruta = Id ? rutaTip(Tip, Id) : null;
  const eticheta = `${Tip ?? ''} ${Numar ?? ''}`.trim();
  return ruta ? <Link to={ruta}>{eticheta}</Link> : <span>{eticheta}</span>;
}

// `Total` = brutul redus de SERVER; `Rest` vine tot din ReadDto (F3-D2) — TS nu
// scade nimic. La editare nesalvată nu există: baza s-a schimbat.
function Sumar(props: { doc?: TrzRead; modificat: boolean }) {
  const { doc, modificat } = props;
  return (
    <div className="sumar">
      <span className="sumar__stare">{labelEnum('StareDocument', doc?.Stare) || 'nesalvat'}</span>
      <span className="sumar__total">
        Total: {modificat || doc?.Total == null
          ? <em title="Totalul îl calculează serverul la salvare.">— recalculat la salvare</em>
          : doc.Total.toFixed(2)}
      </span>
      {!modificat && doc?.Stare === 'Operat' && doc.Ramas != null && (
        <span>Rest de stins: <strong>{doc.Ramas.toFixed(2)}</strong></span>
      )}
      {doc?.Numar && <span>Număr: {doc.Numar}</span>}
    </div>
  );
}
