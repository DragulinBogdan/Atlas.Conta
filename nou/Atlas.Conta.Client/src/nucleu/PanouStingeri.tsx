import { useMemo, useState } from 'react';
import { Link } from 'react-router';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { NumberBox } from 'devextreme-react';
import { Column, DataGrid, FilterRow, Pager, Paging, Sorting } from 'devextreme-react/data-grid';
import { PanouErori } from './PanouErori';
import { ConfirmareInline } from './ConfirmareInline';
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

// ═══ Al doilea mod: candidații GRUPAȚI (F19-D10/D16) ═══
// Trezoreria are O SINGURĂ contrapartidă, pe latură — de aceea modul clasic
// cere un `contrapartidaId` și citește candidații din proiecția remote. Nota
// contabilă le are pe LINII, câte vrea, iar plafonul are și SENS: „ce pot
// stinge cu nota asta" are n răspunsuri, fiecare cu jumătatea lui de plafon și
// cu lista lui de candidați. Serverul le rezolvă într-un apel (`GET
// api/ntc/{id}/candidati`), felia le traduce în forma de mai jos, panoul le
// AFIȘEAZĂ — zero aritmetică de domeniu aici (42c).
//
// Fără sensul VIZIBIL panoul ar minți: aceeași contrapartidă are două jumătăți
// cu disponibil diferit. Și fără `contrapartidaId` trimis EXPLICIT la creare,
// un document care poartă două dintre contrapartidele notei pe laturi diferite
// primește un refuz de ambiguitate corect, dar orb (F19-D16, a doua axă):
// panoul e singurul care știe sub ce grup a fost ales rândul.
export type GrupCandidati = {
  contrapartidaId: string;
  contrapartidaEticheta: string;
  // Numele/label-ul sensului, gata tradus de felie (`labelEnum`).
  sensEticheta: string;
  // Cele trei cifre ale jumătății, toate din server (`NtcContrapartidaDto`).
  capacitate: number;
  asignat: number;
  disponibil: number;
  candidati: DocumentCuRest[];
  // Lista e plafonată server-side; trunchierea se SPUNE, nu se ascunde.
  maiSunt: boolean;
};

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
  // MODUL GRUPAT (F19-D10): dat (chiar și gol) ⇒ zona de adăugare se randează
  // per (contrapartidă × sens), din datele feliei, iar `contrapartidaId`/
  // `tipuriCandidate` nu se mai consultă. Absent/`null` ⇒ modul clasic, exact
  // cum era (trezoreria, FCT, FCL, DEC — neatinse).
  grupuri?: GrupCandidati[] | null;
  // Ce se spune când modul grupat n-are niciun grup: motivul e al FELIEI (nota
  // pe draft nu stinge încă; nota fără repartitori pe linii n-are contrapartide).
  mesajFaraGrupuri?: string;
  // Stingerea schimbă affordances-urile documentului (`PoateAnula`/`PoateStorna`
  // țin cont de imperecheri — F3-D2), deci felia trebuie să-și recitească
  // ReadDto-ul. Panoul nu știe cum se cheamă cache-ul feliei; îl anunță.
  onSchimbare?: () => void;
}) {
  const { documentId, contrapartidaId, rol, tipuriCandidate, grupuri, mesajFaraGrupuri, onSchimbare } = props;
  const grupat = grupuri != null;
  const cache = useQueryClient();
  const [erori, setErori] = useState<string[]>([]);
  // Candidatul ales + GRUPUL sub care a fost afișat (modul grupat). Grupul e
  // parte din alegere, nu o deducție ulterioară: el dă și plafonul propus, și
  // `ContrapartidaId` trimis serverului.
  const [candidat, setCandidat] = useState<DocumentCuRest | null>(null);
  const [grupAles, setGrupAles] = useState<GrupCandidati | null>(null);
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

  // Sensul cerut de candidații acestui document — SERVER-COMPUTED
  // (`StingeriDto.SensCandidati` = `Opus(SensDeStins)`, o singură formulă pentru
  // ambele roluri). Panoul îl pasează, nu îl deduce: sensul e funcție de TIP,
  // deci polimorf, iar TS-ul nu are voie să-l reconstituie (42c).
  //
  // De ce e obligatoriu (F19-D16, review F3): filtrul de TIP de mai jos e o ALTĂ
  // axă și nu spune nimic despre sens. Măsurat pe baza Privat, 87 din 353 de
  // contrapartide au documente pe AMBELE sensuri (≈25 %) — pe o astfel de
  // contrapartidă panoul unei Încasări afișa zeci de facturi de FURNIZOR cu
  // buton „Stinge", toate cu refuz GARANTAT (422 „…n-are capacitate pe sensul
  // cerut…"), în timp ce panoul unei Plăți arăta exact aceleași rânduri, valide.
  // Asimetria nu se vedea din ecran. Cele două axe se aplică AMÂNDOUĂ.
  //
  // `null` (tipul nu declară sens) ⇒ fără filtru, exact ca înainte.
  const sensCandidati = date?.SensCandidati ?? null;
  // Store-ul se construiește DOAR după ce ReadDto-ul a venit: altfel prima
  // randare ar cere candidații fără sens și grila remote ar apuca să afișeze
  // tocmai rândurile pe care filtrul le scoate.
  const sursaCandidati = useMemo(
    () => (!grupat && contrapartidaId && citit.isSuccess
      ? stingeri.storeCandidati(contrapartidaId, sensCandidati)
      : null),
    [grupat, contrapartidaId, sensCandidati, citit.isSuccess, versiune]);

  async function reincarca() {
    await cache.invalidateQueries({ queryKey: ['stingeri'] });
    setVersiune((v) => v + 1);
    onSchimbare?.();
  }

  function alege(rand: DocumentCuRest, grup?: GrupCandidati) {
    setErori([]);
    setCandidat(rand);
    setGrupAles(grup ?? null);
    // Propunerea = cât se poate stinge de fapt, din numerele SERVERULUI: restul
    // documentului curent și al candidatului. Rotunjirea la bani e a scării
    // (49e) — serverul refuză explicit trei zecimale.
    //
    // În modul grupat plafonul propriu NU e `Ramas`-ul documentului: nota are
    // un plafon PER JUMĂTATE (`Disponibil`), iar `ValideazaCreare` exact pe el
    // îl verifică. Σ liniilor notei nu e un „rest" (F19-D10).
    const propriu = grup ? grup.disponibil : (date?.Ramas ?? 0);
    const celalalt = rand.Rest ?? 0;
    setSuma(Number(Math.min(Math.abs(propriu), Math.abs(celalalt)).toFixed(2)));
  }

  async function confirma() {
    if (!candidat?.DocumentId || suma == null) return;
    setErori([]);
    setOcupat(true);
    try {
      // `ContrapartidaId` se trimite DOAR în modul grupat, unde clientul chiar
      // știe sub ce plafon a afișat rândul (F19-D16). În modul clasic rămâne
      // absent — deducția serverului e neambiguă acolo (o singură contrapartidă,
      // pe latură), iar comportamentul trezoreriei nu se schimbă cu nimic.
      await stingeri.creeaza(rol === 'stinge'
        ? {
          DocumentStingatorId: documentId,
          DocumentId: candidat.DocumentId,
          Suma: suma,
          ContrapartidaId: grupAles?.contrapartidaId,
        }
        : { DocumentStingatorId: candidat.DocumentId, DocumentId: documentId, Suma: suma });
      setCandidat(null);
      setGrupAles(null);
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

  // Rândul de CONFIRMARE, o singură definiție pentru ambele moduri: aceeași
  // sumă propusă din numerele serverului, aceeași confirmare inline.
  const randConfirmare = candidat && (
    <div className="cerere-data">
      <span>
        {rol === 'stinge' ? 'Stinge' : 'Stins de'} <strong>{candidat.Tip} {candidat.Numar}</strong>
        {' '}(rest {(candidat.Rest ?? 0).toFixed(2)})
        {grupAles && <> pe <strong>{grupAles.contrapartidaEticheta} · {grupAles.sensEticheta}</strong></>}
        {' '}cu suma
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
      <button type="button" className="buton" onClick={() => { setCandidat(null); setGrupAles(null); }}>
        Renunță
      </button>
    </div>
  );

  return (
    <section className="document__stingeri">
      <div className="linii__bara">
        <h3>Stingeri</h3>
        <div className="stingeri__numere">
          <span>Total: <strong>{(date?.Total ?? 0).toFixed(2)}</strong></span>
          <span>Asignat: <strong>{(date?.Asignat ?? 0).toFixed(2)}</strong></span>
          {/* „Rest" există doar acolo unde Σ documentului E o creanță/datorie.
              Pe nota contabilă nu e (F19-D10): plafonul e per jumătate și se
              vede pe fiecare grup — un „Rest" global aici ar fi o cifră care
              nu plafonează nimic. */}
          {!grupat && <span>Rest: <strong>{(date?.Ramas ?? 0).toFixed(2)}</strong></span>}
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
        <ConfirmareInline
          intrebare={(
            <>
              Ștergeți legătura de stingere cu <strong>{randDeSters.CelalaltTip} {randDeSters.CelalaltNumar}</strong>?
              {' '}Restul ambelor documente se eliberează.
            </>
          )}
          verb="Șterge legătura"
          ocupat={ocupat}
          onConfirma={() => void confirmaStergere()}
          onRenunta={() => setRandDeSters(null)}
        />
      )}

      {/* Zona de ADĂUGARE. Două moduri (clasic / grupat), aceeași confirmare
          INLINE (57f) și aceeași conductă de creare — se schimbă doar de unde
          vin candidații și pe ce plafon se propune suma. */}
      {grupat
        ? (grupuri.length === 0
          ? (
            <p className="indiciu">
              {mesajFaraGrupuri ?? 'Documentul n-are nicio contrapartidă cu plafon de stingere.'}
            </p>
          )
          : (
            <details className="sectiune-pliabila">
              <summary>Adaugă stingere</summary>
              {randConfirmare}
              {/* Un bloc per (contrapartidă × SENS): și plafonul, și candidații
                  sunt ai JUMĂTĂȚII (F19-D16). Un singur bloc pe contrapartidă ar
                  promite capacitate dublă și ar propune facturi de client sub
                  jumătatea de datorie. */}
              {grupuri.map((g) => (
                <div className="stingeri__grup" key={`${g.contrapartidaId}|${g.sensEticheta}`}>
                  <div className="linii__bara">
                    <h4>{g.contrapartidaEticheta} · {g.sensEticheta}</h4>
                    <div className="stingeri__numere">
                      <span>Plafon: <strong>{g.capacitate.toFixed(2)}</strong></span>
                      <span>Asignat: <strong>{g.asignat.toFixed(2)}</strong></span>
                      <span>Disponibil: <strong>{g.disponibil.toFixed(2)}</strong></span>
                    </div>
                  </div>
                  {g.candidati.length === 0
                    ? <p className="indiciu">Nu există documente cu rest pe jumătatea asta.</p>
                    : (
                      <DataGrid dataSource={g.candidati} keyExpr="DocumentId" showBorders columnAutoWidth>
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
                              <button
                                type="button"
                                className="buton buton--mic"
                                disabled={ocupat}
                                onClick={() => alege(rand, g)}
                              >
                                Stinge
                              </button>
                            );
                          }}
                        />
                      </DataGrid>
                    )}
                  {g.maiSunt && (
                    <p className="indiciu">
                      Lista e plafonată la primele 100 de documente ale jumătății (cele mai vechi întâi) — mai
                      există și altele.
                    </p>
                  )}
                </div>
              ))}
            </details>
          ))
        : !contrapartidaId
          ? <p className="indiciu">Documentul n-are contrapartidă — nu se pot propune candidați de stins.</p>
          : sursaCandidati == null
            // Sensul candidaților vine din ReadDto-ul panoului; până sosește nu
            // se cere nimic, ca lista să nu apară o clipă nefiltrată.
            ? <p className="indiciu">Se încarcă lista de candidați…</p>
            : (
              <details className="sectiune-pliabila">
                <summary>Adaugă stingere</summary>
                {randConfirmare}

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
