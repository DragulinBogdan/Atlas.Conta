import { useEffect, useMemo, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Column, DataGrid } from 'devextreme-react/data-grid';
import { DocumentShell, type Comanda } from '../../nucleu/DocumentShell';
import { Formular, eroriStructurale } from '../../nucleu/formular';
import { CampBifa, CampData, CampSelectie, CampText } from '../../nucleu/campuri';
import { Lookup } from '../../nucleu/Lookup';
import { PanouErori } from '../../nucleu/PanouErori';
import { PanouStingeri } from '../../nucleu/PanouStingeri';
import { campMeta, labelEnum } from '../../nucleu/campMeta';
import { eroriDin } from '../../nucleu/http';
import { rutaTip } from '../../nucleu/stingeri';
import {
  antetGol, fct, linieGoala, spreWrite,
  SCHEMA_ANTET, SCHEMA_LINIE, TIP_ANTET, TIP_LINIE,
  type DocumentCopil, type FctLinieRead, type FctLinieWrite, type FctWrite,
} from './api';
import { FctEditorLinie } from './FctEditorLinie';

// Felia verticală FCT, ecranul de document — același șablon ca BTR (43c):
//   (1) server-read  → `useQuery` pe ReadDto (affordances, Total, etichete, Copii);
//   (2) formular     → `agregat`: WriteDto ÎNTREG, o singură valoare locală;
//   (3) efemeride    → linia în editare, erorile afișate.
// URL-ul e starea globală: `/fct/nou` și `/fct/:id`.
//
// Ce exersează felia asta peste BTR: numărul CULES (al furnizorului), lotul
// născut din `Produs` la culegere (server-owned — nu apare în WriteDto), TVA-ul
// la culegere, dimensiunile pe frunză și mecanismul CONEX (NIR-ul generat la
// operare, oferit ca link în panoul de rezultat).

const CAMPURI_ANTET: (keyof FctWrite & string)[] =
  ['Numar', 'Data', 'PredatorId', 'PrimitorId', 'DataScadenta', 'NumarPV', 'DataPV', 'CodCpv'];
const capAntet = (m: string) => campMeta(TIP_ANTET, m, SCHEMA_ANTET).caption;
const capLinie = (m: string) => campMeta(TIP_LINIE, m, SCHEMA_LINIE).caption;

export function FctDetaliu() {
  const { id } = useParams();
  const nou = id === 'nou' || id === undefined;
  const navigheaza = useNavigate();
  const cache = useQueryClient();

  const [agregat, setAgregat] = useState<FctWrite>(antetGol);
  const [modificat, setModificat] = useState(false);
  const [aratErori, setAratErori] = useState(false);
  const [erori, setErori] = useState<string[]>([]);
  const [mesaje, setMesaje] = useState<string[]>([]);
  const [inEditare, setInEditare] = useState<FctLinieWrite | null>(null);
  const [indiceEditat, setIndiceEditat] = useState<number | null>(null);

  const citit = useQuery({
    queryKey: ['fct', id],
    queryFn: () => fct.citeste(id!),
    enabled: !nou,
  });

  // ReadDto proaspăt ⇒ formularul se re-seed-uiește. Nu e sincronizare de store:
  // e o singură direcție, server → agregat, la fiecare recitire.
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
      // Numărul furnizorului: serverul îl lasă nullable pe DRAFT și îl cere abia
      // la operare (`FacturaIntrare.ValideazaOperare`) — îl cerem la culegere,
      // fiindcă operatorul are hârtia în mână și un draft fără număr nu se poate
      // opera. Aceeași regulă, arătată mai devreme; nu una nouă.
      ...(agregat.Numar?.trim() ? [] : [`„${capAntet('Numar')}” este obligatoriu — factura poartă numărul furnizorului.`]),
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
    mutationFn: async () => (nou ? fct.creeaza(agregat) : fct.actualizeaza(id!, agregat)),
    onSuccess: (salvat) => {
      setErori([]);
      setMesaje(['Salvat.']);
      setModificat(false);
      cache.invalidateQueries({ queryKey: ['fct'] });
      if (nou) navigheaza(`/fct/${salvat.Id}`, { replace: true });
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

  async function comanda(rulare: () => Promise<{ Mesaje?: string[] | null; StareNoua?: string | null; ConexId?: string | null }>) {
    setErori([]);
    setMesaje([]);
    try {
      const rezultat = await rulare();
      setMesaje([
        `Stare nouă: ${labelEnum('StareDocument', rezultat.StareNoua)}`,
        ...(rezultat.Mesaje ?? []),
        ...(rezultat.ConexId ? ['Motorul a generat documentul conex (NIR) în aceeași tranzacție.'] : []),
      ]);
      // Grupul conex se recitește o dată cu documentul: `Copii[]` e sursa
      // link-urilor, ca ele să existe și la o revenire ulterioară pe ecran, nu
      // doar imediat după operare.
      await cache.invalidateQueries({ queryKey: ['fct'] });
      await cache.invalidateQueries({ queryKey: ['nir'] });
      // Operarea poate naște PLATA (31e) și, cu ea, imperecherea automată pe
      // restul stingibil — lista de plăți și panoul de stingeri se recitesc.
      await cache.invalidateQueries({ queryKey: ['plt'] });
      await cache.invalidateQueries({ queryKey: ['stingeri'] });
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
      await fct.sterge(id!);
      await cache.invalidateQueries({ queryKey: ['fct'] });
      navigheaza('/fct');
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
      ruleaza: () => void raporteaza(fct.valideaza(id!).then((e) => {
        if (e.length > 0) { setErori(e); return []; }
        return ['Documentul trece toți gardienii motorului.'];
      })),
    },
    { eticheta: 'Operează', disponibila: (doc?.PoateOpera ?? false) && !modificat, ruleaza: () => void comanda(() => fct.opereaza(id!)) },
    { eticheta: 'Anulează operarea', disponibila: doc?.PoateAnula ?? false, ruleaza: () => void comanda(() => fct.anuleaza(id!)) },
    {
      eticheta: 'Stornează',
      disponibila: doc?.PoateStorna ?? false,
      cereData: { eticheta: 'Data stornării' },
      ruleaza: (data) => { if (data) void comanda(() => fct.storneaza(id!, data)); },
    },
    { eticheta: 'Șterge', disponibila: !nou && poateEdita, ruleaza: () => void stergeDocumentul() },
    { eticheta: 'Înapoi la listă', disponibila: true, ruleaza: () => navigheaza('/fct') },
  ];

  function schimbaAntet(v: FctWrite) {
    setAgregat(v);
    setModificat(true);
  }

  function salveazaLinie(linie: FctLinieWrite) {
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
      titlu={nou ? 'Factură de intrare — nouă' : `Factură de intrare ${doc?.Numar ?? ''}`}
      sumar={<Sumar stare={doc?.Stare} total={doc?.Total} modificat={modificat || nou} />}
      comenzi={comenzi}
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
              <CampText<FctWrite> camp="Numar" obligatoriu />
              <CampData<FctWrite> camp="Data" />
              {/* Furnizorul: 129k parteneri ⇒ căutare server-side (`mod="remote"`). */}
              <Lookup<FctWrite> camp="PredatorId" entitate="Partener" mod="remote" cauta={['Denumire', 'Cod', 'CodFiscal']} />
              <Lookup<FctWrite> camp="PrimitorId" entitate="Gestiune" mod="local" cauta={['Cod', 'Denumire']} />
              <CampData<FctWrite> camp="DataScadenta" />
              <CampText<FctWrite> camp="NumarPV" />
              <CampData<FctWrite> camp="DataPV" />
              <CampText<FctWrite> camp="CodCpv" />
            </div>

            {/* PLATA AUTOMATĂ (F3-D5 / 31e): nu e „un alt document" cules aici,
                ci datele din care MOTORUL construiește plata la operarea
                facturii — echivalentul DECONT_* din legacy. Câmpurile apar doar
                când bifa e pusă: condiționalitate ÎN COD (43a), nu regulă de
                vizibilitate interpretată dintr-un descriptor.

                Ce NU face ecranul: nu construiește plata și nu-i calculează
                suma (o duce motorul, din liniile facturii), nu cere contul
                propriu ca „obligatoriu" — refuzul, dacă lipsește, e al
                gardianului, cu mesajul lui. */}
            {/* Secțiunea rămâne NEcontrolată: un `open` legat de bifă ar
                re-deschide-o la fiecare re-randare, peste voia operatorului.
                Starea se citește din etichetă, unde e oricum mai vizibilă. */}
            <details className="sectiune-pliabila">
              <summary>Plată automată{agregat.GenereazaPlata ? ' — activă' : ''}</summary>
              <div className="grila-campuri">
                <CampBifa<FctWrite> camp="GenereazaPlata" />
                {agregat.GenereazaPlata && (
                  <>
                    <Lookup<FctWrite>
                      camp="PlataContPropriuId"
                      entitate="ContPropriu"
                      mod="local"
                      cauta={['Cod', 'Denumire', 'Iban']}
                    />
                    <CampText<FctWrite> camp="PlataNumar" />
                    <CampData<FctWrite> camp="PlataData" />
                    <CampSelectie<FctWrite> camp="PlataTipInstrument" enumerare="TipInstrumentPlata" />
                  </>
                )}
              </div>
            </details>
          </Formular>
        </>
      }
      subsol={!nou && doc?.Id && doc.Stare === 'Operat'
        ? (
          // Factura stă pe rolul de STINSĂ (plata o stinge — 31d): rolul e
          // identitate declarată de felie, nu deducție în panou.
          <PanouStingeri
            documentId={doc.Id}
            contrapartidaId={doc.PredatorId}
            rol="este-stins"
            // O factură e stinsă doar de trezorerie (F3-D6a).
            tipuriCandidate={['PLT', 'INC']}
            onSchimbare={() => void cache.invalidateQueries({ queryKey: ['fct'] })}
          />
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
              serverul, nu editează. Etichetele bogate (lot, valori) vin din
              ReadDto — server-owned; liniile nesalvate le arată goale. */}
          <DataGrid
            dataSource={linii.map((l, i) => ({ ...l, __indice: i, ...etichete(doc?.Linii, l) }))}
            keyExpr="__indice"
            showBorders
            columnAutoWidth
            onRowClick={(e) => {
              if (!poateEdita) return;
              const rand = e.data as FctLinieWrite & { __indice: number };
              setIndiceEditat(rand.__indice);
              setInEditare(linii[rand.__indice]);
            }}
          >
            <Column dataField="TipMaterialCod" caption="Cod tip" />
            <Column dataField="TipMaterialDenumire" caption={capLinie('TipMaterialId')} />
            <Column dataField="ProdusDenumire" caption={capLinie('ProdusId')} />
            <Column dataField="Cantitate" caption={capLinie('Cantitate')} dataType="number" format="#,##0.###" alignment="right" />
            <Column dataField="PretUnitar" caption={capLinie('PretUnitar')} dataType="number" format="#,##0.######" alignment="right" />
            {/* Valorile sunt ale SERVERULUI (43b): apar după Salvează, nu se
                calculează în TS nici măcar ca previzualizare. */}
            <Column dataField="Valoare" caption={`${capLinie('Valoare')} (server)`} dataType="number" format="#,##0.00" alignment="right" />
            <Column dataField="ValoareTva" caption={`${capLinie('ValoareTva')} (server)`} dataType="number" format="#,##0.00" alignment="right" />
            {/* Lotul e server-owned pe FCT (53a): se naște din `Produs` la
                culegere și se finalizează la operare. */}
            <Column dataField="LotEticheta" caption={capLinie('LotId')} />
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
            <FctEditorLinie
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

// Etichetele server-owned ale unei linii: se caută în ReadDto după `Id`. Linia
// nouă nu are încă niciuna — corect: nu le inventăm în TS.
function etichete(citite: FctLinieRead[] | null | undefined, linie: FctLinieWrite) {
  const g = citite?.find((c) => c.Id === linie.Id);
  return {
    TipMaterialCod: g?.TipMaterialCod ?? '',
    TipMaterialDenumire: g?.TipMaterialDenumire ?? '',
    ProdusDenumire: g?.ProdusDenumire ?? '',
    LotEticheta: g?.LotEticheta ?? '',
    Valoare: g?.Valoare,
    ValoareTva: g?.ValoareTva,
  };
}

// Grupul conex (F2-D5): NIR-ul clonă și PLATA autogenerată (31e). Link, nu
// redirect automat: operatorul decide când trece pe documentul copil — factura
// poate avea și alte linii de verificat.
//
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
          const eticheta = c.Tip === 'NIR'
            ? `Deschide NIR-ul generat ${c.Numar ?? ''}`
            : c.Tip === 'PLT'
              ? `Deschide plata generată ${c.Numar ?? ''}`
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
