import { useMemo, useState } from 'react';
import { Link } from 'react-router';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { NumberBox } from 'devextreme-react';
import { Column, DataGrid, FilterRow, Pager, Paging, Sorting } from 'devextreme-react/data-grid';
import { PanouErori } from './PanouErori';
import { eroriDin } from './http';
import { rutaTip, stingeri, type DocumentCuRest, type StingereRand } from './stingeri';

// Panoul de STINGERI (F3-D8) — componentă de NUCLEU fiindcă resursa e a
// documentului: același panou pe PLT, pe INC și pe FCT.
//
// Trei lucruri pe care le respectă prin construcție:
//
//  1. **Numerele sunt ale serverului.** Total/Asignat/Rest vin din `StingeriDto`
//     (`ImperechereService`) — TS nu însumează, nu scade și nu recalculează
//     nimic după un POST: reîncarcă panoul (43b/42c).
//  2. **Rolul e IDENTITATE, nu deducție.** Cine stinge pe cine nu se poate
//     deriva în client (e polimorf — `Document.CapacitateStingere`, 48b), deci
//     felia îl declară explicit: trezoreria STINGE, factura E STINSĂ. Refuzul
//     de invariant rămâne al serviciului și se afișează ca atare.
//  3. **Candidații vin din proiecția de rest**, filtrată pe contrapartida
//     documentului curent — nu dintr-o listă construită în client.
export type RolStingere = 'stinge' | 'este-stins';

export function PanouStingeri(props: {
  documentId: string;
  // Contrapartida documentului curent (PLT: primitorul; INC/FCT: predatorul).
  // Lipsa ei ascunde doar zona de ADĂUGARE — lista existentă se vede oricum.
  contrapartidaId?: string | null;
  rol: RolStingere;
  // Tipurile de document care POT fi candidați ai acestei stingeri (review
  // F3-D6a: proiecția întoarce toate documentele cu rest ale contrapartidei,
  // dar nu toate se pot stinge — un buton pe un candidat incompatibil = refuz
  // GARANTAT al serverului). Felia le declară fiindcă ea știe rolul: trezoreria
  // (`stinge`) stinge creanțe/datorii (FCT/FCL/DEC); factura (`este-stins`) e
  // stinsă doar de trezorerie (PLT/INC). Se aplică ca filtru pe grilă → ajunge
  // la DataSourceLoader → SQL. Gol/absent = fără filtru (compatibil înainte).
  tipuriCandidate?: string[];
  // Stingerea schimbă affordances-urile documentului (`PoateAnula`/`PoateStorna`
  // țin cont de imperecheri — F3-D2), deci felia trebuie să-și recitească
  // ReadDto-ul. Panoul nu știe cum se cheamă cache-ul feliei; îl anunță.
  onSchimbare?: () => void;
}) {
  const { documentId, contrapartidaId, rol, tipuriCandidate, onSchimbare } = props;
  const cache = useQueryClient();
  const [erori, setErori] = useState<string[]>([]);
  const [candidat, setCandidat] = useState<DocumentCuRest | null>(null);
  const [suma, setSuma] = useState<number | undefined>(undefined);
  const [ocupat, setOcupat] = useState(false);
  // Rândul pentru care se cere confirmarea ștergerii — confirmare INLINE, nu
  // `window.confirm`: dialogul nativ BLOCHEAZĂ renderer-ul (găsit la smoke F3),
  // pe lângă că nu e stilabil. Nul = nicio confirmare în așteptare.
  const [randDeSters, setRandDeSters] = useState<StingereRand | null>(null);
  // Reîncărcarea grilei REMOTE de candidați: store nou ⇒ grilă reîncărcată.
  // (Rândurile se schimbă la fiecare stingere — restul lor scade.)
  const [versiune, setVersiune] = useState(0);

  const citit = useQuery({
    queryKey: ['stingeri', documentId],
    queryFn: () => stingeri.citeste(documentId),
  });
  const date = citit.data;
  const randuri = date?.Imperecheri ?? [];

  const sursaCandidati = useMemo(
    () => (contrapartidaId ? stingeri.storeCandidati(contrapartidaId) : null),
    [contrapartidaId, versiune]);

  async function reincarca() {
    await cache.invalidateQueries({ queryKey: ['stingeri'] });
    setVersiune((v) => v + 1);
    onSchimbare?.();
  }

  function alege(rand: DocumentCuRest) {
    setErori([]);
    setCandidat(rand);
    // Propunerea = cât se poate stinge de fapt, din numerele SERVERULUI: restul
    // documentului curent și al candidatului. Rotunjirea la bani e a scării
    // (49e) — serverul refuză explicit trei zecimale.
    const propriu = date?.Ramas ?? 0;
    const celalalt = rand.Rest ?? 0;
    setSuma(Number(Math.min(Math.abs(propriu), Math.abs(celalalt)).toFixed(2)));
  }

  async function confirma() {
    if (!candidat?.DocumentId || suma == null) return;
    setErori([]);
    setOcupat(true);
    try {
      await stingeri.creeaza(rol === 'stinge'
        ? { DocumentStingatorId: documentId, DocumentId: candidat.DocumentId, Suma: suma }
        : { DocumentStingatorId: candidat.DocumentId, DocumentId: documentId, Suma: suma });
      setCandidat(null);
      setSuma(undefined);
      await reincarca();
    }
    catch (e) { setErori(eroriDin(e)); }
    finally { setOcupat(false); }
  }

  async function confirmaStergere() {
    const rand = randDeSters;
    if (!rand?.Id) return;
    setRandDeSters(null);
    setErori([]);
    setOcupat(true);
    try {
      await stingeri.sterge(rand.Id);
      await reincarca();
    }
    catch (e) { setErori(eroriDin(e)); }
    finally { setOcupat(false); }
  }

  // Filtrul de tip pentru candidați (F3-D6a): `["Tip","=","PLT"] or […]`,
  // formatul DataSourceLoader — ajunge la SQL prin loadOptions.
  const filtruTipuri = useMemo(() => {
    if (!tipuriCandidate || tipuriCandidate.length === 0) return undefined;
    if (tipuriCandidate.length === 1) return ['Tip', '=', tipuriCandidate[0]];
    return tipuriCandidate
      .map((t) => ['Tip', '=', t])
      .reduce((acc, f) => (acc.length === 0 ? f : [acc, 'or', f]), [] as unknown[]);
  }, [tipuriCandidate]);

  return (
    <section className="document__stingeri">
      <div className="linii__bara">
        <h3>Stingeri</h3>
        <div className="stingeri__numere">
          <span>Total: <strong>{(date?.Total ?? 0).toFixed(2)}</strong></span>
          <span>Asignat: <strong>{(date?.Asignat ?? 0).toFixed(2)}</strong></span>
          <span>Rest: <strong>{(date?.Ramas ?? 0).toFixed(2)}</strong></span>
        </div>
      </div>

      <PanouErori erori={erori} titlu="Refuzat de server" />

      {randuri.length === 0
        ? <p className="indiciu">Documentul nu are nicio stingere.</p>
        : (
          <DataGrid dataSource={randuri} keyExpr="Id" showBorders columnAutoWidth>
            <Column
              caption="Direcție"
              width={130}
              cellRender={(c) => ((c.data as StingereRand).EsteStingator ? 'stinge →' : '← stins de')}
            />
            <Column
              caption="Document"
              cellRender={(c) => <Celalalt rand={c.data as StingereRand} />}
            />
            <Column dataField="Suma" caption="Sumă" dataType="number" format="#,##0.00" alignment="right" />
            <Column
              dataField="Autogenerat"
              caption="Autogenerat"
              width={110}
              cellRender={(c) => ((c.data as StingereRand).Autogenerat ? 'da' : 'nu')}
            />
            <Column
              caption=""
              width={90}
              cellRender={(c) => (
                <button
                  type="button"
                  className="buton buton--mic"
                  disabled={ocupat}
                  onClick={() => setRandDeSters(c.data as StingereRand)}
                >
                  Șterge
                </button>
              )}
            />
          </DataGrid>
        )}

      {randDeSters && (
        <div className="cerere-data">
          <span>
            Ștergeți legătura de stingere cu <strong>{randDeSters.CelalaltTip} {randDeSters.CelalaltNumar}</strong>?
            {' '}Restul ambelor documente se eliberează.
          </span>
          <button type="button" className="buton buton--primar" disabled={ocupat} onClick={() => void confirmaStergere()}>
            Șterge legătura
          </button>
          <button type="button" className="buton" onClick={() => setRandDeSters(null)}>Renunță</button>
        </div>
      )}

      {sursaCandidati == null
        ? <p className="indiciu">Documentul n-are contrapartidă — nu se pot propune candidați de stins.</p>
        : (
          <details className="sectiune-pliabila">
            <summary>Adaugă stingere</summary>

            {candidat && (
              <div className="cerere-data">
                <span>
                  {rol === 'stinge' ? 'Stinge' : 'Stins de'} <strong>{candidat.Tip} {candidat.Numar}</strong>
                  {' '}(rest {(candidat.Rest ?? 0).toFixed(2)}) cu suma
                </span>
                <NumberBox
                  value={suma}
                  format="#,##0.00"
                  onValueChanged={(e) => setSuma(e.value == null ? undefined : Number(e.value))}
                />
                <button
                  type="button"
                  className="buton buton--primar"
                  disabled={ocupat || suma == null || suma === 0}
                  onClick={() => void confirma()}
                >
                  Confirmă
                </button>
                <button type="button" className="buton" onClick={() => setCandidat(null)}>Renunță</button>
              </div>
            )}

            {/* Proiecția întoarce TOATE documentele operate cu rest ale
                contrapartidei — inclusiv documentul curent (are și el rest).
                Butonul lui lipsește: serverul l-ar refuza oricum („un document
                nu se stinge pe sine"), dar refuzul se evită, nu se provoacă. */}
            <DataGrid
              dataSource={sursaCandidati}
              remoteOperations
              showBorders
              columnAutoWidth
              height={280}
              defaultFilterValue={filtruTipuri}
            >
              <Sorting mode="multiple" />
              <FilterRow visible />
              <Paging defaultPageSize={10} />
              <Pager showInfo />

              <Column dataField="Tip" caption="Tip" width={80} />
              <Column dataField="Numar" caption="Număr" />
              <Column dataField="Data" caption="Dată" dataType="date" format="dd.MM.yyyy" />
              <Column dataField="Total" caption="Total" dataType="number" format="#,##0.00" alignment="right" />
              <Column dataField="Asignat" caption="Asignat" dataType="number" format="#,##0.00" alignment="right" />
              <Column dataField="Rest" caption="Rest" dataType="number" format="#,##0.00" alignment="right" />
              <Column
                caption=""
                width={90}
                cellRender={(c) => {
                  const rand = c.data as DocumentCuRest;
                  if (rand.DocumentId === documentId) return <span className="indiciu">acest document</span>;
                  return (
                    <button type="button" className="buton buton--mic" disabled={ocupat} onClick={() => alege(rand)}>
                      Stinge
                    </button>
                  );
                }}
              />
            </DataGrid>
          </details>
        )}
    </section>
  );
}

// Celălalt capăt al legăturii: link acolo unde felia există, text unde nu (nota
// de compensare, returul) — vezi `rutaTip`.
function Celalalt(props: { rand: StingereRand }) {
  const { CelalaltTip, CelalaltNumar, CelalaltDocumentId } = props.rand;
  const eticheta = `${CelalaltTip ?? '—'} ${CelalaltNumar ?? ''}`.trim();
  const ruta = CelalaltDocumentId ? rutaTip(CelalaltTip, CelalaltDocumentId) : null;
  return ruta ? <Link to={ruta}>{eticheta}</Link> : <span>{eticheta}</span>;
}
