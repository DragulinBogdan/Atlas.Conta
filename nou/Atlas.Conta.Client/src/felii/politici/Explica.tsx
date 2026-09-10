import { useMemo, type ReactNode } from 'react';
import { Link } from 'react-router';
import { useQuery } from '@tanstack/react-query';
import { DateBox, SelectBox } from 'devextreme-react';
import DataSource from 'devextreme/data/data_source';
import { labelEnum } from '../../nucleu/campMeta';
import { PanouErori } from '../../nucleu/PanouErori';
import { eroriDin, ia } from '../../nucleu/http';
import { CAMP_CAUTARE, areCautare, storeOData } from '../../nucleu/odata';
import { urlCu, useUrlStare } from '../../nucleu/urlStare';
import { azi, izolataZi } from '../../nucleu/zi';
import { codSiDenumire } from './comune';
import type { components } from '../../generated/api-types';

// Panoul „Explică" (F24-D7): ce ar face configurația cu o linie IPOTETICĂ —
// tip de document × Tip (cont/clasă) × semn × dată × laturi × partener × produs.
// Nu e planul unui document: nu există nici document, nici registre.
//
// Ecranul nu decide nimic și nu compune nicio frază de verdict. Fiecare bloc
// vine cu `Concluzie` gata formată de server (F24-D6), fiindcă e aceeași frază
// pe care clientul ar fi trebuit s-o inventeze din patru câmpuri — adică exact
// calculul interzis în TS (42c/81f). Ce face ecranul: culege parametrii, ARATĂ
// rândurile și candidații eliminați cu motivul lor, și duce înapoi la grila
// politicii care a dat verdictul.
//
// Starea E URL-ul (43c): un link cu parametri deschide panoul precompletat și
// cere explicația singur — de aceea „Explică pe acest tip" din grile e un link,
// nu un canal propriu.

type Explicatie = components['schemas']['ExplicatieDto'];
type RandContare = components['schemas']['RegulaContareRandDto'];
type RandStoc = components['schemas']['RegulaStocRandDto'];
type RandImplicit = components['schemas']['RandTvaImplicitDto'];
type ContRezolvat = components['schemas']['ContRezolvatDto'];

const SEMNE = [
  { valoare: '1', label: '+1 — intrare / plus' },
  { valoare: '-1', label: '−1 — ieșire / minus' },
];

export function Explica() {
  const [stare, seteaza] = useUrlStare({
    tip: '',
    tipMaterial: '',
    semn: '1',
    data: azi(),
    predator: '',
    primitor: '',
    partener: '',
    produs: '',
  });

  // Ruta cere AMBELE: fără tipul de material răspunde 400, deci nu se întreabă.
  // Restul parametrilor sunt opționali și lipsesc din URL cât sunt goi (`urlCu`).
  const activa = stare.tip !== '' && stare.tipMaterial !== '';
  const cale = urlCu('/api/politici/explica', stare);

  const explicatie = useQuery({
    queryKey: ['politici', 'explica', cale],
    queryFn: () => ia<Explicatie>(cale),
    enabled: activa,
  });

  const e = explicatie.data;
  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>Explică o linie</h2>
      </div>

      <div className="bara-raport">
        <CampNomenclator
          eticheta="Tip de document"
          entitate="TipDocument"
          // Valoarea E CODUL: ruta cere codul ancorei, iar sursa e mică și se
          // încarcă întreagă — deci nu există un `byKey` de rezolvat.
          camp="Cod"
          local
          latime={220}
          valoare={stare.tip}
          seteaza={(v) => seteaza({ tip: v })}
        />
        <CampNomenclator
          eticheta="Tip (cont / clasă)"
          entitate="TipMaterial"
          latime={280}
          valoare={stare.tipMaterial}
          seteaza={(v) => seteaza({ tipMaterial: v })}
        />
        <label className="bara-raport__camp">
          <span className="camp__eticheta">Semnul liniei</span>
          <SelectBox
            dataSource={SEMNE}
            value={stare.semn}
            valueExpr="valoare"
            displayExpr="label"
            width={180}
            onValueChanged={(ev) => { if (ev.event) seteaza({ semn: String(ev.value ?? '1') }); }}
          />
        </label>
        <label className="bara-raport__camp">
          <span className="camp__eticheta">Data</span>
          <DateBox
            value={stare.data || null}
            displayFormat="dd.MM.yyyy"
            width={140}
            onValueChanged={(ev) => { if (ev.event) seteaza({ data: izolataZi(ev.value) ?? '' }); }}
          />
        </label>
        <CampNomenclator
          eticheta="Predator"
          entitate="Repartitor"
          latime={220}
          valoare={stare.predator}
          seteaza={(v) => seteaza({ predator: v })}
        />
        <CampNomenclator
          eticheta="Primitor"
          entitate="Repartitor"
          latime={220}
          valoare={stare.primitor}
          seteaza={(v) => seteaza({ primitor: v })}
        />
        <CampNomenclator
          eticheta="Partener"
          entitate="Partener"
          latime={240}
          valoare={stare.partener}
          seteaza={(v) => seteaza({ partener: v })}
        />
        <CampNomenclator
          eticheta="Produs"
          entitate="Produs"
          latime={240}
          valoare={stare.produs}
          seteaza={(v) => seteaza({ produs: v })}
        />
      </div>

      {explicatie.isError && <PanouErori erori={eroriDin(explicatie.error)} titlu="Refuzat de server" />}

      {!activa && (
        <p className="indiciu">
          Alegeți tipul de document și Tipul liniei. Laturile, partenerul și produsul sunt opționale —
          fără ele explicația arată ce se rezolvă și ce nu, exact ca pe un document incomplet.
        </p>
      )}

      {activa && !explicatie.isError && explicatie.isPending && <p className="indiciu">Se citește…</p>}

      {e && !explicatie.isError && (
        <>
          <Antet e={e} />
          <Card titlu="Contare" concluzie={e.Contare?.Concluzie} grila="/politici/reguli-contare"
            atentie={e.Contare?.PostareExplicita} eticheta={e.Contare?.PostareExplicita ? 'postare explicită' : undefined}>
            <p className="explica__nivel">
              Treapta de potrivire: <strong>{labelEnum('NivelContare', e.Contare?.Nivel)}</strong>
            </p>
            {e.Contare?.Castigator && (
              <>
                <h4 className="explica__subtitlu">Regula câștigătoare</h4>
                <TabelContare randuri={[e.Contare.Castigator]} />
                <div className="explica__conturi">
                  <ContLatura eticheta="Debit" cont={e.Contare.ContDebit} />
                  <ContLatura eticheta="Credit" cont={e.Contare.ContCredit} />
                </div>
              </>
            )}
            <Candidati
              randuri={(e.Contare?.Candidati ?? []).filter((c) => c.Motiv != null)}
              cheie={(c) => c.Regula?.Id}
              motiv={(c) => c.Motiv}
              rand={(c) => <CeluleContare r={c.Regula} />}
              antet={<><th>Tip material</th><th>Natură</th><th>Semn</th><th>Debit</th><th>Credit</th></>}
            />
          </Card>

          {/* Un card per LATURĂ, fiecare cu concluzia lui: `Stoc[]` are câte o
              intrare per latură cu reguli. Lista GOALĂ (tipul n-are nicio regulă
              de stoc) e singurul caz fără frază de la server — vezi indiciul. */}
          <Card titlu="Stoc" grila="/politici/reguli-stoc">
            {(e.Stoc ?? []).length === 0 && (
              <p className="explica__concluzie">Tipul n-are nicio regulă de stoc, pe nicio latură.</p>
            )}
            {(e.Stoc ?? []).map((s) => (
              <section key={s.Latura ?? ''} className="explica__latura">
                <p className="explica__concluzie">{s.Concluzie}</p>
                <p className="explica__nivel">
                  Latura <strong>{labelEnum('LaturaDocument', s.Latura)}</strong>
                  {' · '}{labelEnum('NivelStoc', s.Nivel)}
                  {s.Motiv && <> · {labelEnum('MotivStoc', s.Motiv)}</>}
                </p>
                {(s.Reguli ?? []).length > 0 && <TabelStoc randuri={s.Reguli ?? []} />}
              </section>
            ))}
          </Card>

          <Card titlu="TVA" concluzie={e.Tva?.Concluzie} grila="/politici/tva">
            {e.Tva?.Id && (
              <table className="tabel-mic">
                <thead>
                  <tr>
                    <th>Direcția</th><th>Sursa contrapartidei</th><th>Contrapartida de rezervă</th>
                    <th>Contrapartida rezolvată</th><th>Proveniență</th>
                  </tr>
                </thead>
                <tbody>
                  <tr>
                    <td>{labelEnum('DirectieTva', e.Tva.Directie)}</td>
                    <td>{labelEnum('SursaCont', e.Tva.SursaContrapartida)}</td>
                    <td>{e.Tva.ContrapartidaFallback ?? ''}</td>
                    <td><TextCont cont={e.Tva.Contrapartida} /></td>
                    <td><Provenienta dinSeed={e.Tva.DinSeed} /></td>
                  </tr>
                </tbody>
              </table>
            )}
          </Card>

          <Card titlu="Document conex" concluzie={e.Conex?.Concluzie} grila="/politici/conex">
            {e.Conex?.Id && (
              <table className="tabel-mic">
                <thead>
                  <tr>
                    <th>Ținta</th><th>Inversează laturile</th><th>Filtru de natură</th>
                    <th>Linia trece</th><th>Proveniență</th>
                  </tr>
                </thead>
                <tbody>
                  <tr>
                    <td>{e.Conex.Tinta}{e.Conex.TintaDenumire ? ` — ${e.Conex.TintaDenumire}` : ''}</td>
                    <td>{e.Conex.InverseazaLaturi ? 'da' : 'nu'}</td>
                    <td>{labelEnum('NaturaClasa', e.Conex.NaturaFiltru) || 'toate liniile'}</td>
                    <td>{e.Conex.Trece ? 'da' : 'nu'}</td>
                    <td><Provenienta dinSeed={e.Conex.DinSeed} /></td>
                  </tr>
                </tbody>
              </table>
            )}
          </Card>

          <Card titlu="Tipul de TVA implicit" concluzie={e.Implicit?.Concluzie} grila="/politici/tva-implicit">
            <p className="explica__nivel">
              Sursa: <strong>{labelEnum('SursaImplicit', e.Implicit?.Sursa)}</strong>
            </p>
            {e.Implicit?.Motiv && <p className="indiciu">{e.Implicit.Motiv}</p>}
            {e.Implicit?.RandPolitica && (
              <>
                <h4 className="explica__subtitlu">Rândul de politică aplicat</h4>
                <TabelImplicit randuri={[e.Implicit.RandPolitica]} />
              </>
            )}
            <Candidati
              randuri={(e.Implicit?.Candidati ?? []).filter((c) => c.Motiv != null)}
              cheie={(c) => c.Rand?.Id}
              motiv={(c) => c.Motiv}
              rand={(c) => <CeluleImplicit r={c.Rand} />}
              antet={<><th>Clasa fiscală</th><th>Valabil de la</th><th>Tip TVA</th></>}
            />
          </Card>

          <Card titlu="Validare" grila="/politici/validare">
            {e.Validare
              ? (
                <table className="tabel-mic">
                  <thead>
                    <tr><th>Cere clasificație bugetară</th><th>Natură interzisă</th><th>Proveniență</th></tr>
                  </thead>
                  <tbody>
                    <tr>
                      <td>{e.Validare.CereClasificatieBugetara ? 'da' : 'nu'}</td>
                      <td>{labelEnum('NaturaClasa', e.Validare.NaturaInterzisa) || '—'}</td>
                      <td><Provenienta dinSeed={e.Validare.DinSeed} /></td>
                    </tr>
                  </tbody>
                </table>
              )
              : <p className="explica__concluzie">Tipul n-are profil de validare.</p>}
          </Card>

          <Card titlu="Scadență" grila="/politici/scadente">
            {e.Scadenta
              ? (
                <table className="tabel-mic">
                  <thead><tr><th>Zile</th><th>Proveniență</th></tr></thead>
                  <tbody>
                    <tr>
                      <td>{e.Scadenta.ZileDefault}</td>
                      <td><Provenienta dinSeed={e.Scadenta.DinSeed} /></td>
                    </tr>
                  </tbody>
                </table>
              )
              : <p className="explica__concluzie">Tipul n-are politică de scadență.</p>}
          </Card>

          <Card titlu="Numerotare" grila="/politici/numerotare">
            {e.Numerotare
              ? (
                <table className="tabel-mic">
                  <thead><tr><th>Serie</th><th>Format</th><th>Următorul număr</th><th>Proveniență</th></tr></thead>
                  <tbody>
                    <tr>
                      <td>{e.Numerotare.Serie ?? ''}</td>
                      <td>{e.Numerotare.Format ?? ''}</td>
                      <td>{e.Numerotare.UrmatorulNumar}</td>
                      <td><Provenienta dinSeed={e.Numerotare.DinSeed} /></td>
                    </tr>
                  </tbody>
                </table>
              )
              : <p className="explica__concluzie">Tipul n-are politică de numerotare.</p>}
          </Card>

          <p className="indiciu">
            Explicația e a CONFIGURAȚIEI, pe o linie ipotetică: aceleași funcții de potrivire pe care
            le rulează motorul, pe fapte fabricate din parametrii de mai sus. Un document real mai
            aduce ce n-are linia asta — conturile culese pe tipurile cu postare explicită, loturile,
            soldurile — iar refuzurile de operare rămân ale motorului. Rândurile duc la grila
            politicii lor; grila nu focalizează încă rândul anume.
          </p>
        </>
      )}
    </div>
  );
}

// ── piesele de afișare ──────────────────────────────────────────────────────

function Antet(props: { e: Explicatie }) {
  const { e } = props;
  return (
    <table className="tabel-mic explica__antet">
      <thead>
        <tr>
          <th>Tip document</th><th>Tip (cont / clasă)</th><th>Clasa</th><th>Natura</th>
          <th>Semn</th><th>Data</th><th>Predator</th><th>Primitor</th><th>Partener</th><th>Produs</th>
        </tr>
      </thead>
      <tbody>
        <tr>
          <td>{e.TipDocument}{e.TipDocumentDenumire ? ` — ${e.TipDocumentDenumire}` : ''}</td>
          <td>{e.TipMaterial}{e.TipMaterialDenumire ? ` — ${e.TipMaterialDenumire}` : ''}</td>
          <td>{e.Clasa ?? ''}</td>
          <td>{labelEnum('NaturaClasa', e.Natura) || '—'}</td>
          <td>{e.Semn != null && e.Semn > 0 ? '+1' : '−1'}</td>
          <td>{e.Data ?? ''}</td>
          <td>{e.Predator ?? '—'}</td>
          <td>{e.Primitor ?? '—'}</td>
          <td>{e.Partener ?? '—'}</td>
          <td>{e.Produs ?? '—'}</td>
        </tr>
      </tbody>
    </table>
  );
}

function Card(props: {
  titlu: string;
  concluzie?: string | null;
  // Grila politicii care a dat verdictul — puntea înapoi (43c).
  grila: string;
  // Steagul pe care serverul îl ridică (azi: `PostareExplicita`): ecranul îl
  // EVIDENȚIAZĂ, textul rămâne al `Concluzie`-i.
  atentie?: boolean;
  eticheta?: string;
  children?: ReactNode;
}) {
  const { titlu, concluzie, grila, atentie, eticheta, children } = props;
  return (
    <section className="explica__card">
      <div className="explica__cap">
        <h3>{titlu}</h3>
        {eticheta && <span className="explica__marcaj">{eticheta}</span>}
        <Link className="buton buton--mic" to={grila}>Vezi politica</Link>
      </div>
      {concluzie && (
        <p className={atentie ? 'explica__concluzie explica__concluzie--atentie' : 'explica__concluzie'}>
          {concluzie}
        </p>
      )}
      {children}
    </section>
  );
}

function Candidati<T>(props: {
  randuri: T[];
  cheie: (c: T) => string | undefined;
  motiv: (c: T) => string | null | undefined;
  rand: (c: T) => ReactNode;
  antet: ReactNode;
}) {
  const { randuri, cheie, motiv, rand, antet } = props;
  if (randuri.length === 0) return null;
  return (
    <>
      <h4 className="explica__subtitlu">Candidați eliminați ({randuri.length})</h4>
      <table className="tabel-mic explica__candidati">
        <thead><tr>{antet}<th>Proveniență</th><th>De ce a picat</th></tr></thead>
        <tbody>
          {randuri.map((c, i) => (
            <tr key={cheie(c) ?? i}>
              {rand(c)}
              <td className="explica__motiv">{labelEnum('MotivEliminare', motiv(c))}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </>
  );
}

function TabelContare(props: { randuri: RandContare[] }) {
  return (
    <table className="tabel-mic">
      <thead>
        <tr>
          <th>Tip material</th><th>Natură</th><th>Semn</th><th>Debit</th><th>Credit</th><th>Proveniență</th>
        </tr>
      </thead>
      <tbody>
        {props.randuri.map((r, i) => <tr key={r.Id ?? i}><CeluleContare r={r} /></tr>)}
      </tbody>
    </table>
  );
}

// Celulele unui rând de regulă de contare — aceleași în tabelul câștigătorului
// și în cel al candidaților, ca cele două să nu poată drifta.
function CeluleContare(props: { r?: RandContare }) {
  const r = props.r;
  if (!r) return null;
  return (
    <>
      <td>{r.TipMaterial ?? '—'}</td>
      <td>{labelEnum('NaturaClasa', r.NaturaFiltru) || '—'}</td>
      <td>{r.SemnFiltru == null ? '—' : r.SemnFiltru > 0 ? '+1' : '−1'}</td>
      <td>{labelEnum('SursaCont', r.SursaContDebit)}{r.ContDebit ? ` (${r.ContDebit})` : ''}</td>
      <td>{labelEnum('SursaCont', r.SursaContCredit)}{r.ContCredit ? ` (${r.ContCredit})` : ''}</td>
      <td><Provenienta dinSeed={r.DinSeed} /></td>
    </>
  );
}

function TabelStoc(props: { randuri: RandStoc[] }) {
  return (
    <table className="tabel-mic">
      <thead><tr><th>Clasa</th><th>Tip de stoc</th><th>Semn</th><th>Proveniență</th></tr></thead>
      <tbody>
        {props.randuri.map((r, i) => (
          <tr key={r.Id ?? i}>
            <td>{r.Clasa ?? 'orice clasă'}</td>
            <td>{labelEnum('TipStoc', r.TipStoc)}</td>
            <td>{r.Semn != null && r.Semn > 0 ? '+1' : '−1'}</td>
            <td><Provenienta dinSeed={r.DinSeed} /></td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

function TabelImplicit(props: { randuri: RandImplicit[] }) {
  return (
    <table className="tabel-mic">
      <thead><tr><th>Clasa fiscală</th><th>Valabil de la</th><th>Tip TVA</th><th>Proveniență</th></tr></thead>
      <tbody>
        {props.randuri.map((r, i) => <tr key={r.Id ?? i}><CeluleImplicit r={r} /></tr>)}
      </tbody>
    </table>
  );
}

function CeluleImplicit(props: { r?: RandImplicit }) {
  const r = props.r;
  if (!r) return null;
  return (
    <>
      <td>{labelEnum('ClasaFiscalaPartener', r.ClasaFiscala) || 'orice clasă'}</td>
      <td>{r.ValabilDeLa ?? '—'}</td>
      <td>{r.TipTva ?? ''}</td>
      <td><Provenienta dinSeed={r.DinSeed} /></td>
    </>
  );
}

// Contul rezolvat al unei laturi: simbol, denumire și SURSA care l-a dat —
// „401" fără sursă n-ar fi spus dacă vine din regulă sau din repartitor.
function ContLatura(props: { eticheta: string; cont?: ContRezolvat }) {
  return (
    <div className="explica__cont">
      <span className="camp__eticheta">{props.eticheta}</span>
      <TextCont cont={props.cont} />
    </div>
  );
}

function TextCont(props: { cont?: ContRezolvat | null }) {
  const c = props.cont;
  if (!c) return <span className="explica__nerezolvat">—</span>;
  const sursa = labelEnum('SursaRezolvata', c.Sursa);
  if (!c.Simbol) return <span className="explica__nerezolvat">nerezolvat ({sursa})</span>;
  return <span>{c.Simbol}{c.Denumire ? ` — ${c.Denumire}` : ''} <em>({sursa})</em></span>;
}

// Aceeași grafie ca în grilele de politică (F23-D4): timbrul e al seed-ului, se
// stinge la prima editare.
function Provenienta(props: { dinSeed?: boolean }) {
  return <>{props.dinSeed ? 'seed' : 'manual'}</>;
}

// Un selector de nomenclator al BAREI de parametri — nu un `Lookup`: acela e
// legat de agregatul unui formular (`useCamp`), iar aici parametrul trăiește în
// URL (43c), ca la bara de raportare. Ce se împarte e store-ul, cu cache-ul și
// normalizarea căutării din F20-D2.
function CampNomenclator(props: {
  eticheta: string;
  entitate: string;
  // Câmpul care pleacă pe sârmă. Implicit cheia (`ID`); `TipDocument` face
  // excepția, fiindcă ruta cere CODUL ancorei.
  camp?: string;
  // Nomenclator mic: se încarcă întreg și se filtrează în browser (43f).
  local?: boolean;
  latime: number;
  valoare: string;
  seteaza: (v: string) => void;
}) {
  const { eticheta, entitate, camp = 'ID', local, latime, valoare, seteaza } = props;
  const sursa = useMemo(() => new DataSource({
    store: storeOData(entitate),
    sort: 'Cod',
    paginate: !local,
    pageSize: local ? undefined : 50,
  }), [entitate, local]);

  return (
    <label className="bara-raport__camp">
      <span className="camp__eticheta">{eticheta}</span>
      <SelectBox
        dataSource={sursa}
        value={valoare || null}
        valueExpr={camp}
        // Referință stabilă: un `displayExpr` nou la fiecare randare reia
        // rezolvarea afișării prin `byKey` (capcana măsurată în `FisaCont`).
        displayExpr={codSiDenumire}
        searchEnabled
        searchExpr={areCautare(entitate) ? CAMP_CAUTARE : 'Cod'}
        searchTimeout={local ? 200 : 400}
        showClearButton
        noDataText="Nimic găsit"
        width={latime}
        onValueChanged={(ev) => {
          // Doar acțiunile omului schimbă starea (56e/57f): widget-ul ridică
          // evenimentul și când deep-link-ul îi rezolvă valoarea, iar atunci
          // scrierea în URL ar fi o rescriere a valorii din care tocmai a venit.
          if (!ev.event) return;
          seteaza(ev.value == null ? '' : String(ev.value));
        }}
      />
    </label>
  );
}
