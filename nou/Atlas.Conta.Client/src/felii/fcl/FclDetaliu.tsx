import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { Link, useNavigate, useParams } from 'react-router';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { DateBox } from 'devextreme-react';
import { Column, DataGrid } from 'devextreme-react/data-grid';
import { DocumentShell, type Comanda } from '../../nucleu/DocumentShell';
import { ConfirmareInline } from '../../nucleu/ConfirmareInline';
import { CampShell } from '../../nucleu/CampShell';
import { Formular, eroriStructurale } from '../../nucleu/formular';
import { CampData } from '../../nucleu/campuri';
import { Lookup } from '../../nucleu/Lookup';
import { LookupGrila } from '../../nucleu/LookupGrila';
import { PanouErori } from '../../nucleu/PanouErori';
import { PanouStingeri } from '../../nucleu/PanouStingeri';
import { campMeta, labelEnum } from '../../nucleu/campMeta';
import { eroriDin } from '../../nucleu/http';
import { existaInSet } from '../../nucleu/odata';
import { rutaTip } from '../../nucleu/stingeri';
import { azi, izolataZi } from '../../nucleu/zi';
import {
  antetGol, fcl, linieGoala, spreWrite,
  SCHEMA_ANTET, SCHEMA_LINIE, TIP_ANTET, TIP_LINIE,
  type DocumentCopil, type FclLinieRead, type FclLinieWrite, type FclWrite,
} from './api';
import { FclEditorLinie, type EticheteCulese } from './FclEditorLinie';

// Felia verticală FCL, ecranul de document — șablonul consolidat (43c):
//   (1) server-read  → `useQuery` pe ReadDto (affordances, Total, etichete, Copii);
//   (2) formular     → `agregat`: WriteDto ÎNTREG, o singură valoare locală;
//   (3) efemeride    → linia în editare, erorile afișate.
// URL-ul e starea globală: `/fcl/nou` și `/fcl/:id`.
//
// Ce exersează felia asta peste FCT: numărul SERVER-OWNED (serie fiscală — nici
// nu apare în formular), pinul de lot la culegere, gestiunea de descărcare pe
// antet și, mai ales, DESCĂRCAREA DE GESTIUNE: documentul secundar pe care
// motorul îl generează la operare și pe care operatorul îl poate cere din nou,
// pentru restul nelivrat (backorder — F4-D3/D4).

const CAMPURI_ANTET: (keyof FclWrite & string)[] =
  ['Data', 'PredatorId', 'PrimitorId', 'DataScadenta', 'GestiuneDescarcareId'];
const capLinie = (m: string) => campMeta(TIP_LINIE, m, SCHEMA_LINIE).caption;

export function FclDetaliu() {
  const { id } = useParams();
  const nou = id === 'nou' || id === undefined;
  const navigheaza = useNavigate();
  const cache = useQueryClient();

  const [agregat, setAgregat] = useState<FclWrite>(antetGol);
  const [modificat, setModificat] = useState(false);
  const [aratErori, setAratErori] = useState(false);
  const [erori, setErori] = useState<string[]>([]);
  const [mesaje, setMesaje] = useState<string[]>([]);
  // Ștergerea draftului cere confirmare INLINE (F20-D4), nu `window.confirm`:
  // dialogul nativ blochează renderer-ul și nu poate spune CE se pierde.
  // Starea e a feliei; locul de randare îl dă `DocumentShell`.
  const [deSters, setDeSters] = useState(false);
  const [inEditare, setInEditare] = useState<FclLinieWrite | null>(null);
  const [indiceEditat, setIndiceEditat] = useState<number | null>(null);
  // Etichetele CULESE în sesiunea asta, per POZIȚIE de linie (paralel cu
  // `agregat.Linii`): grila le folosește cât timp linia n-are încă etichete
  // server-owned în ReadDto (documentul/linia nesalvată).
  const [eticheteLinii, setEticheteLinii] = useState<(EticheteCulese | undefined)[]>([]);

  const citit = useQuery({
    queryKey: ['fcl', id],
    queryFn: () => fcl.citeste(id!),
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

  // Emitentul unui document EXISTENT: istoricul poate purta orice repartitor
  // intern (validarea cere doar „nu Partener"), iar lookup-ul pe setul
  // `UnitateInterna` ar minți pe valorile din afara lui (afişare goală, listă
  // care nu conține valoarea). Sonda de existență decide: valoarea E în set ⇒
  // lookup normal (editabil pe draft); altfel — sau cât timp nu știm — afișare
  // statică din ReadDto. Eșecul sondei cade deci pe varianta care nu minte.
  const emitentInSet = useQuery({
    queryKey: ['sonda', 'UnitateInterna', doc?.PredatorId],
    queryFn: () => existaInSet('UnitateInterna', doc!.PredatorId!),
    enabled: !nou && doc?.PredatorId != null,
    staleTime: Infinity,
  });
  const emitentEditabil = nou || doc?.PredatorId == null || emitentInSet.data === true;

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
    mutationFn: async () => (nou ? fcl.creeaza(agregat) : fcl.actualizeaza(id!, agregat)),
    onSuccess: (salvat) => {
      setErori([]);
      setMesaje(['Salvat.']);
      setModificat(false);
      cache.invalidateQueries({ queryKey: ['fcl'] });
      if (nou) navigheaza(`/fcl/${salvat.Id}`, { replace: true });
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

  // Recitirea de după orice comandă: documentul, DESCĂRCĂRILE (grupul conex se
  // schimbă) și stingerile (affordances-urile lor țin cont de imperecheri).
  async function reciteste() {
    await cache.invalidateQueries({ queryKey: ['fcl'] });
    await cache.invalidateQueries({ queryKey: ['dsc'] });
    await cache.invalidateQueries({ queryKey: ['stingeri'] });
  }

  async function comanda(rulare: () => Promise<{ Mesaje?: string[] | null; StareNoua?: string | null; ConexId?: string | null }>) {
    setErori([]);
    setMesaje([]);
    try {
      const rezultat = await rulare();
      setMesaje([
        `Stare nouă: ${labelEnum('StareDocument', rezultat.StareNoua)}`,
        ...(rezultat.Mesaje ?? []),
        ...(rezultat.ConexId
          ? ['Motorul a generat descărcarea de gestiune în aceeași tranzacție.']
          : []),
      ]);
      await reciteste();
    }
    catch (e) {
      setErori(eroriDin(e));
    }
  }

  async function stergeDocumentul() {
    setDeSters(false);
    setErori([]);
    setMesaje([]);
    try {
      await fcl.sterge(id!);
      await cache.invalidateQueries({ queryKey: ['fcl'] });
      navigheaza('/fcl');
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
      ruleaza: () => void raporteaza(fcl.valideaza(id!).then((e) => {
        if (e.length > 0) { setErori(e); return []; }
        return ['Documentul trece toți gardienii motorului.'];
      })),
    },
    { eticheta: 'Operează', disponibila: (doc?.PoateOpera ?? false) && !modificat, ruleaza: () => void comanda(() => fcl.opereaza(id!)) },
    { eticheta: 'Anulează operarea', disponibila: doc?.PoateAnula ?? false, ruleaza: () => void comanda(() => fcl.anuleaza(id!)) },
    {
      eticheta: 'Stornează',
      disponibila: doc?.PoateStorna ?? false,
      cereData: { eticheta: 'Data stornării' },
      ruleaza: (data) => { if (data) void comanda(() => fcl.storneaza(id!, data)); },
    },
    { eticheta: 'Șterge', disponibila: !nou && poateEdita, ruleaza: () => setDeSters(true) },
    { eticheta: 'Înapoi la listă', disponibila: true, ruleaza: () => navigheaza('/fcl') },
  ];

  function schimbaAntet(v: FclWrite) {
    setAgregat(v);
    setModificat(true);
  }

  function salveazaLinie(linie: FclLinieWrite, culese: EticheteCulese) {
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
      titlu={nou ? 'Factură de ieșire — nouă' : `Factură de ieșire ${doc?.Numar ?? ''}`}
      sumar={<Sumar stare={doc?.Stare} total={doc?.Total} modificat={modificat || nou} />}
      comenzi={comenzi}
      confirmare={deSters && (
        <ConfirmareInline
          intrebare="Ștergeți definitiv acest draft, cu tot cu liniile lui?"
          verb="Șterge documentul"
          onConfirma={() => void stergeDocumentul()}
          onRenunta={() => setDeSters(false)}
        />
      )}
      inchideConfirmarea={() => setDeSters(false)}
      erori={erori}
      mesaje={mesaje}
      rezultatExtra={<Copii copii={doc?.Copii} />}
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
              {/* `Numar` lipsește DELIBERAT: FCL are politică de numerotare
                  (serie fiscală „FCL-"), deci numărul e al serverului și nici nu
                  există în WriteDto (F4-D1). Se vede în titlu, după operare. */}
              <CampData<FclWrite> camp="Data" />

              {/* EMITENTUL = unitatea internă (sediul — cum operează și
                  ModelCheck/importul); `UnitateInterna` e expusă ReadOnly în
                  OData exact pentru lookup-ul ăsta (amendament F4-D5). Pe un
                  document EXISTENT sonda `emitentInSet` decide (vezi sus):
                  valoarea în set ⇒ lookup (rămâne editabil pe draft); valoare
                  istorică din afara setului ⇒ afișare statică din ReadDto —
                  lookup-ul ar minți pe ea. */}
              {emitentEditabil
                ? (
                  <Lookup<FclWrite>
                    camp="PredatorId"
                    entitate="UnitateInterna"
                    mod="local"
                    eticheta="Emitent"
                  />
                )
                : <Static membru="PredatorId" eticheta="Emitent" valoare={doc?.PredatorDenumire} />}

              {/* Clientul: 129k parteneri ⇒ căutare server-side. */}
              <LookupGrila<FclWrite>
                camp="PrimitorId"
                entitate="Partener"
                cauta={['Cautare', 'CodFiscal']}
                eticheta="Client"
              />
              {/* Necules ⇒ politica de scadență aplică default-ul (+30 zile) la
                  operare (decizia 30c) — clientul nu-l calculează. */}
              <CampData<FclWrite> camp="DataScadenta" />
              {/* O gestiune per factură la P2 (37d): din ea se descarcă toate
                  liniile de stoc. Obligativitatea („există linii de stoc") e a
                  operării. */}
              <Lookup<FclWrite> camp="GestiuneDescarcareId" entitate="Gestiune" mod="local" />
            </div>
          </Formular>
        </>
      }
      subsol={!nou && doc?.Id
        ? (
          <>
            {/* Descărcarea e a documentului OPERAT: pe un draft nu există încă
                acoperire de calculat, iar comanda o refuză explicit (F4-D3). */}
            {doc.Stare !== 'Draft' && (
              <PanouDescarcare
                documentId={doc.Id}
                poateGenera={doc.PoateGeneraDescarcare ?? false}
                areGestiune={doc.GestiuneDescarcareId != null}
                onMesaje={(m) => { setErori([]); setMesaje(m); }}
                onErori={(e) => { setMesaje([]); setErori(e); }}
              />
            )}
            {doc.Stare === 'Operat' && (
              // Factura de ieșire stă pe rolul de STINSĂ (încasarea o stinge —
              // 31d): rolul e identitate declarată de felie, nu deducție în panou.
              <PanouStingeri
                documentId={doc.Id}
                contrapartidaId={doc.PrimitorId}
                rol="este-stins"
                // O factură e stinsă doar de trezorerie (F3-D6a).
                tipuriCandidate={['PLT', 'INC']}
                onSchimbare={() => void cache.invalidateQueries({ queryKey: ['fcl'] })}
              />
            )}
          </>
        )
        : undefined}
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
              (nimic inventat în TS); valorile rămân exclusiv ale serverului —
              apar după Salvează. */}
          <DataGrid
            dataSource={linii.map((l, i) => ({ ...l, __indice: i, ...etichete(doc?.Linii, l, eticheteLinii[i]) }))}
            keyExpr="__indice"
            showBorders
            columnAutoWidth
            onRowClick={(e) => {
              if (!poateEdita) return;
              const rand = e.data as FclLinieWrite & { __indice: number };
              setIndiceEditat(rand.__indice);
              setInEditare(linii[rand.__indice]);
            }}
          >
            <Column dataField="TipMaterialCod" caption="Cod tip" />
            <Column dataField="TipMaterialDenumire" caption={capLinie('TipMaterialId')} />
            <Column dataField="ProdusDenumire" caption={capLinie('ProdusId')} />
            {/* Pinul de lot, dacă a fost cules: eticheta e a serverului. */}
            <Column dataField="LotEticheta" caption={capLinie('LotId')} />
            <Column dataField="Cantitate" caption={capLinie('Cantitate')} dataType="number" format="#,##0.###" alignment="right" />
            <Column dataField="PretUnitar" caption={capLinie('PretUnitar')} dataType="number" format="#,##0.######" alignment="right" />
            {/* Valorile sunt ale SERVERULUI (43b): apar după Salvează, nu se
                calculează în TS nici măcar ca previzualizare. */}
            <Column dataField="Valoare" caption={`${capLinie('Valoare')} (server)`} dataType="number" format="#,##0.00" alignment="right" />
            <Column dataField="ValoareTva" caption={`${capLinie('ValoareTva')} (server)`} dataType="number" format="#,##0.00" alignment="right" />
            <Column dataField="TipTvaCod" caption={capLinie('TipTvaId')} />
            <Column dataField="Descriere" caption={capLinie('Descriere')} />
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
            // starea lui locală (inclusiv „TVA-ul a fost atins") să nu rămână a
            // liniei precedente.
            <FclEditorLinie
              key={indiceEditat ?? 'linie-noua'}
              linie={inEditare}
              valoareTvaCitita={etichete(doc?.Linii, inEditare).ValoareTva}
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

// ── Descărcarea de gestiune (F4-D3/D4) ──────────────────────────────────────
//
// Panoul e al FELIEI, nu al nucleului: acoperirea per linie e o noțiune a
// fluxului de vânzare (design P2 §2.2), nu una a oricărui document.
//
// Trei lucruri pe care le respectă prin construcție:
//  1. **Numerele sunt ale serverului.** `Cantitate/Acoperit/Rest` vin din
//     proiecția `RestNedescarcat`; TS nu scade și nu însumează nimic.
//  2. **Butonul urmează AFFORDANCE-ul** (`PoateGeneraDescarcare` — Operat &&
//     gestiune setată && Σ rest > 0), nu o re-derivare locală din stare.
//  3. **Nu decide ce se generează**: spargerea pe loturi, pinul, FIFO-ul și
//     refuzul „întâi BTR" sunt ale `DescarcareService`/motorului.
function PanouDescarcare(props: {
  documentId: string;
  poateGenera: boolean;
  areGestiune: boolean;
  onMesaje: (m: string[]) => void;
  onErori: (e: string[]) => void;
}) {
  const { documentId, poateGenera, areGestiune, onMesaje, onErori } = props;
  const cache = useQueryClient();
  const [data, setData] = useState<string | undefined>(azi());
  const [ocupat, setOcupat] = useState(false);

  const rest = useQuery({
    queryKey: ['fcl', documentId, 'rest'],
    queryFn: () => fcl.restNedescarcat(documentId),
  });
  const randuri = rest.data ?? [];

  async function genereaza() {
    if (!data) return;
    setOcupat(true);
    try {
      const rezultat = await fcl.genereazaDescarcarea(documentId, data);
      const resturi = rezultat.Resturi ?? [];
      onMesaje([
        rezultat.DscId
          ? 'Descărcarea de gestiune a fost generată ca draft — o găsiți în „Documente generate".'
          : 'Nu era nimic de descărcat: stocul disponibil nu acoperă nicio linie rămasă.',
        ...(resturi.length > 0
          ? [`Rămân neacoperite ${resturi.length} linii (backorder) — reveniți după recepția marfii.`]
          : ['Toate liniile de stoc sunt acoperite.']),
      ]);
      await cache.invalidateQueries({ queryKey: ['fcl'] });
      await cache.invalidateQueries({ queryKey: ['dsc'] });
    }
    catch (e) {
      onErori(eroriDin(e));
    }
    finally {
      setOcupat(false);
    }
  }

  return (
    <section className="document__stingeri">
      <div className="linii__bara">
        <h3>Descărcare de gestiune</h3>
      </div>

      {randuri.length === 0
        ? <p className="indiciu">Factura nu are linii de stoc — nu se descarcă nimic din gestiune.</p>
        : (
          <>
            <DataGrid dataSource={randuri} keyExpr="LinieId" showBorders columnAutoWidth>
              <Column dataField="ProdusDenumire" caption="Produs" />
              <Column
                caption="Lot fixat"
                width={100}
                cellRender={(c) => ((c.data as { LotId?: string | null }).LotId ? 'da' : '—')}
              />
              <Column dataField="Cantitate" caption="Cantitate" dataType="number" format="#,##0.###" alignment="right" />
              <Column dataField="Acoperit" caption="Acoperit" dataType="number" format="#,##0.###" alignment="right" />
              <Column dataField="Rest" caption="Rest" dataType="number" format="#,##0.###" alignment="right" />
            </DataGrid>

            <div className="cerere-data">
              <label className="camp__eticheta">Data descărcării</label>
              <DateBox
                type="date"
                displayFormat="dd.MM.yyyy"
                value={data ?? null}
                onValueChanged={(e) => { if (e.event) setData(izolataZi(e.value)); }}
              />
              <button
                type="button"
                className="buton buton--primar"
                disabled={!poateGenera || ocupat || !data}
                onClick={() => void genereaza()}
              >
                Generează descărcarea
              </button>
              {!poateGenera && (
                <span className="indiciu">
                  {areGestiune
                    ? 'Nimic de generat: factura nu e operată sau tot restul e deja acoperit.'
                    : 'Completați gestiunea de descărcare pe antet, apoi operați factura.'}
                </span>
              )}
            </div>
          </>
        )}
    </section>
  );
}

// Etichetele unei linii: cele CULESE în sesiunea asta (alegerea proaspătă a
// operatorului) bat ReadDto-ul, care se caută după `Id`; linia nouă fără
// culegere rămâne goală — nu le inventăm în TS. Valorile n-au variantă culeasă:
// sunt ale serverului prin construcție (43b).
function etichete(citite: FclLinieRead[] | null | undefined, linie: FclLinieWrite, culese?: EticheteCulese) {
  const g = citite?.find((c) => c.Id === linie.Id);
  return {
    TipMaterialCod: culese?.TipMaterialCod ?? g?.TipMaterialCod ?? '',
    TipMaterialDenumire: culese?.TipMaterialDenumire ?? g?.TipMaterialDenumire ?? '',
    ProdusDenumire: culese?.ProdusDenumire ?? g?.ProdusDenumire ?? '',
    LotEticheta: culese?.LotEticheta ?? g?.LotEticheta ?? '',
    TipTvaCod: culese?.TipTvaCod ?? g?.TipTvaCod ?? '',
    Valoare: g?.Valoare,
    ValoareTva: g?.ValoareTva,
  };
}

// Grupul conex al facturii de ieșire: DESCĂRCĂRILE generate (P2). Link, nu
// redirect automat — operatorul decide când trece pe documentul copil.
// Rutarea trece prin `rutaTip` (vocabular închis, în nucleu): un tip fără felie
// de client rămâne TEXT, nu link mort.
function Copii(props: { copii?: DocumentCopil[] | null }) {
  const copii = props.copii ?? [];
  if (copii.length === 0) return null;
  return (
    <div className="panou panou--succes">
      <div className="panou__titlu">Documente generate</div>
      <ul className="panou__lista">
        {copii.map((c) => {
          const ruta = c.Id ? rutaTip(c.Tip, c.Id) : null;
          const eticheta = c.Tip === 'DSC'
            ? `Deschide descărcarea ${c.Numar ?? ''}`
            : `${c.Tip ?? ''} ${c.Numar ?? ''}`;
          return (
            <li key={c.Id}>
              {ruta ? <Link to={ruta}>{eticheta.trim()}</Link> : <span>{eticheta.trim()}</span>}
              {' '}— {labelEnum('StareDocument', c.Stare)}{c.Autogenerat ? ', autogenerat' : ''}
            </li>
          );
        })}
      </ul>
    </div>
  );
}

// Câmp de AFIȘARE în interiorul formularului: aceeași ramă ca la culegere
// (etichetă din metadata, slot de control), cu text în locul editorului.
function Static(props: { membru: string; eticheta?: string; valoare: ReactNode }) {
  const meta = campMeta(TIP_ANTET, props.membru, SCHEMA_ANTET);
  const efectiv = {
    ...meta,
    obligatoriu: false,
    ...(props.eticheta == null ? {} : { caption: props.eticheta }),
  };
  return (
    <CampShell meta={efectiv}>
      <div className="valoare-statica">{props.valoare == null || props.valoare === '' ? '—' : props.valoare}</div>
    </CampShell>
  );
}

// `Total` = valoarea din ReadDto (BRUT: Σ Valoare + ValoareTva), redusă de
// SERVER. La editare nesalvată nu există (baza s-a schimbat) — se marchează
// explicit, nu se recalculează în TS (43c).
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
