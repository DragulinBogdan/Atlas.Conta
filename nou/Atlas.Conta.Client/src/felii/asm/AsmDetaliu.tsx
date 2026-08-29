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
  antetGol, asm, linieGoala, spreWrite,
  SCHEMA_ANTET, SCHEMA_LINIE, TIP_ANTET, TIP_LINIE,
  type AsmLinieRead, type AsmLinieWrite, type AsmWrite,
} from './api';
import { AsmEditorLinie, type EticheteCulese } from './AsmEditorLinie';

// Felia verticală ASM, ecranul de document — șablonul consolidat (43c):
//   (1) server-read  → `useQuery` pe ReadDto (affordances, invariant, etichete);
//   (2) formular     → `agregat`: WriteDto ÎNTREG, o singură valoare locală;
//   (3) efemeride    → linia în editare, erorile afișate.
// URL-ul e starea globală: `/asm/nou` și `/asm/:id`.
//
// Piesa proprie a ecranului e BARA INVARIANTULUI (F19-D9/D4). Asamblarea nu
// postează nimic (zero `RegulaContare`) — singura ei regulă valorică e că marfa
// produsă valorează exact cât consumurile, cu toleranța 0,005 (46d). Cele trei
// cifre vin calculate de SERVER; clientul nu face aritmetică (42c).
//
// Și capcana pe care o rezolvă butonul: la culegere documentul POATE părea
// echilibrat (`Diferenta = 0,00`) și totuși operarea să refuze, fiindcă un
// consum care GOLEȘTE lotul preia tot soldul valoric rămas, nu `preț ×
// cantitate` (75a). Culegerea NU are voie să prezică golirea — ar fi al doilea
// adevăr al regulii —, deci previziunea trăiește în comandă, care o cere
// motorului prin dry-run.
//
// ═══ De ce `Diferenta` NU mai colorează bara (review advers F19, M1) ═══
// Cifrele `SumaConsum`/`SumaProdus`/`Diferenta` sunt ale CULEGERII. Pe scena
// pentru care comanda `distribuie-valoarea` a fost scrisă (consum care golește
// lotul) ele rămân DELIBERAT nezero DUPĂ o distribuire reușită — măsurat în
// ModelCheck: `10,01 / 10,00 / −0,01` pe un document pe care dry-run-ul îl
// acceptă fără nicio eroare. Legate de o culoare, ele mint în AMBELE sensuri:
// verde pe capcană (culegerea prezice `preț × cantitate` ⇒ `Diferenta = 0,00`
// pe un document pe care motorul îl REFUZĂ) și roșu pe documentul corect de
// după comandă.
//
// Deci: cifrele rămân cifre (informative, neutre), iar VERDICTUL „e sau nu
// operabil" îl dă singurul care are voie să-l dea — dry-run-ul motorului (43b),
// citit automat pe draft și reîmprospătat la fiecare salvare/comandă. Explicația
// stă PERMANENT lângă bară, nu într-un mesaj care dispare la prima recitire:
// operatorul care redeschide documentul a doua zi vede același lucru ca cel care
// tocmai a apăsat butonul.

const CAMPURI_ANTET: (keyof AsmWrite & string)[] = ['Data', 'PredatorId', 'PrimitorId'];
const capLinie = (m: string) => campMeta(TIP_LINIE, m, SCHEMA_LINIE).caption;

export function AsmDetaliu() {
  const { id } = useParams();
  const nou = id === 'nou' || id === undefined;
  const navigheaza = useNavigate();
  const cache = useQueryClient();

  const [agregat, setAgregat] = useState<AsmWrite>(antetGol);
  const [modificat, setModificat] = useState(false);
  const [aratErori, setAratErori] = useState(false);
  const [erori, setErori] = useState<string[]>([]);
  const [mesaje, setMesaje] = useState<string[]>([]);
  const [inEditare, setInEditare] = useState<AsmLinieWrite | null>(null);
  const [indiceEditat, setIndiceEditat] = useState<number | null>(null);
  // Etichetele CULESE în sesiunea asta, per POZIȚIE de linie (paralel cu
  // `agregat.Linii`).
  const [eticheteLinii, setEticheteLinii] = useState<(EticheteCulese | undefined)[]>([]);
  // Confirmarea INLINE a distribuirii (57f: `window.confirm` blochează
  // renderer-ul și nu se poate stiliza). Comanda rescrie prețurile culese de
  // operator — nu se declanșează dintr-un singur clic.
  const [confirmaDistribuirea, setConfirmaDistribuirea] = useState(false);

  const citit = useQuery({
    queryKey: ['asm', id],
    queryFn: () => asm.citeste(id!),
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

  // ═══ Verdictul AUTORITAR: dry-run-ul motorului (43b) ═══
  // Nu e o a doua părere despre `Diferenta`, e SINGURA părere care contează —
  // motorul calculează + validează pe valorile FINALE (inclusiv regula golirii,
  // D18-D2), fără să materializeze nimic. Cheia stă SUB `['asm', id]`, deci
  // `invalidateQueries({ queryKey: ['asm'] })` (salvare, distribuire, operare)
  // îl reîmprospătează automat: verdictul nu poate rămâne în urma unei comenzi
  // reușite.
  //
  // Se cere doar pe DRAFT: pe un document operat motorul răspunde „Doar un
  // document în starea Draft poate fi operat", ceea ce nu e un verdict despre
  // conținut. Butonul „Verifică" îl poate cere oricând, explicit (`refetch`).
  // `Stare` face parte din CHEIE: după operare verdictul de pe draft nu mai
  // descrie nimic, iar o cheie nouă îl scoate de pe ecran în loc să-l lase
  // acolo, verde și învechit.
  const verdict = useQuery({
    queryKey: ['asm', id, 'verdict', doc?.Stare],
    queryFn: () => asm.valideaza(id!),
    enabled: !nou && !modificat && doc?.Stare === 'Draft',
  });

  const structurale = useMemo(
    () => [
      ...eroriStructurale(TIP_ANTET, SCHEMA_ANTET, agregat as Record<string, unknown>, CAMPURI_ANTET),
      ...(linii.length === 0 ? ['Documentul nu are nicio linie.'] : []),
    ],
    [agregat, linii.length]);

  const salvare = useMutation({
    mutationFn: async () => (nou ? asm.creeaza(agregat) : asm.actualizeaza(id!, agregat)),
    onSuccess: (salvat) => {
      setErori([]);
      setMesaje(['Salvat.']);
      setModificat(false);
      cache.invalidateQueries({ queryKey: ['asm'] });
      if (nou) navigheaza(`/asm/${salvat.Id}`, { replace: true });
      // Serverul poate REORDONA liniile la recitire — etichetele per poziție nu
      // mai corespund; se golesc aici, iar cele server-owned vin cu refetch-ul.
      else { setAgregat(spreWrite(salvat)); setEticheteLinii([]); }
    },
    onError: (e) => { setMesaje([]); setErori(eroriDin(e)); },
  });

  // Comanda proprie feliei (F19-D4). Rezultatul se afișează ca CE S-A SCHIMBAT
  // — sumele deciziei și reziduul plimbat între linii —, iar agregatul local se
  // re-seed-uiește din documentul recitit de server.
  const distribuire = useMutation({
    mutationFn: async () => asm.distribuie(id!),
    onSuccess: (r) => {
      setErori([]);
      setConfirmaDistribuirea(false);
      if (r.Document) {
        setAgregat(spreWrite(r.Document));
        setEticheteLinii([]);
        setModificat(false);
      }
      setMesaje([
        `Valoarea prezisă a consumului: ${(r.SumaConsum ?? 0).toFixed(2)}.`,
        `Repartizat pe liniile de produs: ${(r.SumaProdus ?? 0).toFixed(2)}.`,
        (r.ReziduuPlimbat ?? 0) === 0
          ? 'Împărțirea a ieșit exact — niciun reziduu de plimbat între linii.'
          : `Reziduu plimbat între linii ca sumele să se închidă la cent: ${(r.ReziduuPlimbat ?? 0).toFixed(2)}.`,
        'Prețurile de evaluare au fost rescrise; verdictul motorului de sub bară s-a recitit.',
      ]);
      void cache.invalidateQueries({ queryKey: ['asm'] });
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
      await cache.invalidateQueries({ queryKey: ['asm'] });
      // Asamblarea mișcă stocul în ambele sensuri (consumă loturi, naște unul
      // nou): soldul afișat în altă parte nu mai e valabil.
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
      await asm.sterge(id!);
      await cache.invalidateQueries({ queryKey: ['asm'] });
      navigheaza('/asm');
    }
    catch (e) {
      setErori(eroriDin(e));
    }
  }

  const comenzi: Comanda[] = [
    { eticheta: nou ? 'Creează' : 'Salvează', disponibila: poateEdita, primara: true, ruleaza: salveaza },
    {
      // O SINGURĂ sursă a verdictului: butonul nu deschide o a doua cale, cere
      // reîmprospătarea aceleiași interogări. Rezultatul se vede tot în blocul
      // permanent de sub bară — nu într-un mesaj care dispare la prima recitire.
      eticheta: 'Verifică',
      disponibila: !nou && !modificat,
      ruleaza: () => { setErori([]); setMesaje([]); void verdict.refetch(); },
    },
    { eticheta: 'Operează', disponibila: (doc?.PoateOpera ?? false) && !modificat, ruleaza: () => void comanda(() => asm.opereaza(id!)) },
    { eticheta: 'Anulează operarea', disponibila: doc?.PoateAnula ?? false, ruleaza: () => void comanda(() => asm.anuleaza(id!)) },
    {
      eticheta: 'Stornează',
      disponibila: doc?.PoateStorna ?? false,
      cereData: { eticheta: 'Data stornării' },
      ruleaza: (data) => { if (data) void comanda(() => asm.storneaza(id!, data)); },
    },
    { eticheta: 'Șterge', disponibila: !nou && poateEdita, ruleaza: () => void stergeDocumentul() },
    { eticheta: 'Înapoi la listă', disponibila: true, ruleaza: () => navigheaza('/asm') },
  ];

  function schimbaAntet(v: AsmWrite) {
    setAgregat(v);
    setModificat(true);
  }

  function salveazaLinie(linie: AsmLinieWrite, culese: EticheteCulese) {
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

  // Butonul de distribuire e afordanță SERVER-SIDE (`PoateDistribui`: Draft cu
  // cel puțin o linie de fiecare rol) — clientul n-o re-derivă din linii. Se
  // adaugă doar condiția locală „nu sunt modificări nesalvate": comanda lucrează
  // pe ce e în BAZĂ, nu pe ce e pe ecran.
  const poateDistribui = !nou && (doc?.PoateDistribui ?? false) && !modificat && !distribuire.isPending;

  return (
    <DocumentShell
      titlu={nou ? 'Asamblare — nouă' : `Asamblare ${doc?.Numar ?? ''}`}
      sumar={<Sumar stare={doc?.Stare} diferenta={doc?.Diferenta} modificat={modificat || nou} />}
      comenzi={comenzi}
      erori={erori}
      mesaje={mesaje}
      ocupat={salvare.isPending || distribuire.isPending || citit.isFetching}
      antet={
        <>
          {aratErori && <PanouErori erori={structurale} titlu="De completat înainte de salvare" />}

          {/* ═══ Bara invariantului (46d) ═══ Trei cifre calculate de SERVER și
              un buton. Nimic nu se însumează aici: `SumaConsum`/`SumaProdus`/
              `Diferenta` vin din ReadDto (F19-D9). Cifre NEUTRE, deliberat —
              vezi antetul fișierului: sunt ale CULEGERII, iar verdictul îl dă
              blocul de dedesubt. */}
          {!nou && (
            <div className="asm__invariant">
              <div className="asm__cifre">
                <span>
                  Consumuri: <strong>{cifra(doc?.SumaConsum, modificat)}</strong>
                </span>
                <span>
                  Produse: <strong>{cifra(doc?.SumaProdus, modificat)}</strong>
                </span>
                <span>
                  Diferență: <strong>{cifra(doc?.Diferenta, modificat)}</strong>
                </span>
              </div>
              <button
                type="button"
                className="buton"
                disabled={!poateDistribui}
                onClick={() => setConfirmaDistribuirea(true)}
              >
                Distribuie valoarea consumului
              </button>
            </div>
          )}

          {/* Explicația PERMANENTĂ a cifrelor: stă pe ecran cât timp există
              documentul, nu într-un `mesaj` care moare la prima recitire. Fără
              ea, un „−0,01" pe un document corect distribuit e o enigmă. */}
          {!nou && (
            <p className="indiciu">
              Cifrele de mai sus sunt ale CULEGERII (preț lot × cantitate). Un consum care GOLEȘTE lotul se
              evaluează la operare cu tot soldul valoric rămas, nu cu prețul unitar — de aceea „Diferență” poate
              rămâne nenulă (cenți) pe un document perfect operabil, inclusiv după o distribuire reușită.
              Ce decide dacă documentul trece e verdictul de mai jos, dat de motor.
            </p>
          )}

          {/* Verdictul motorului — al treilea element al barei și singurul cu
              culoare. Se recitește singur după orice salvare sau comandă, deci
              nu poate contrazice o comandă tocmai rulată cu succes. */}
          {!nou && (
            modificat
              ? (
                <p className="indiciu">
                  Sunt modificări nesalvate — verdictul motorului se recalculează după salvare.
                </p>
              )
              : verdict.isFetching
                ? <p className="indiciu">Se cere verdictul motorului…</p>
                : verdict.data == null
                  ? null
                  : verdict.data.length === 0
                    ? (
                      <PanouErori
                        fel="succes"
                        titlu="Verdictul motorului"
                        erori={['Documentul trece toți gardienii motorului, invariantul valoric inclus — poate fi operat.']}
                      />
                    )
                    : <PanouErori titlu="Motorul refuză documentul" erori={verdict.data} />
          )}

          {confirmaDistribuirea && (
            // Confirmare INLINE (57f), nu `window.confirm`: comanda RESCRIE
            // prețurile de evaluare culese de operator.
            <div className="cerere-data">
              <span>
                Prețurile de evaluare ale liniilor de produs se rescriu astfel încât produsele să valoreze
                exact cât consumurile (proporțional cu ce e cules azi). Continuați?
              </span>
              <button
                type="button"
                className="buton buton--primar"
                disabled={distribuire.isPending}
                onClick={() => distribuire.mutate()}
              >
                Distribuie
              </button>
              <button type="button" className="buton" onClick={() => setConfirmaDistribuirea(false)}>
                Renunță
              </button>
            </div>
          )}

          {!nou && !doc?.PoateDistribui && doc?.Stare === 'Draft' && (
            <p className="indiciu">
              Distribuirea cere cel puțin o linie de consum ȘI una de produs.
            </p>
          )}

          <Formular
            tip={TIP_ANTET}
            schema={SCHEMA_ANTET}
            valoare={agregat}
            onSchimba={schimbaAntet}
            readOnly={!poateEdita}
            aratErori={aratErori}
          >
            <div className="grila-campuri">
              {/* `Numar` e AFIȘARE: ASM are politică de numerotare („ASM-"),
                  deci seria se asignează la operare și nici nu există în
                  WriteDto (F19-D6). */}
              <Static membru="Numar" valoare={doc?.Numar} />
              <CampData<AsmWrite> camp="Data" />
              {/* PREDATORUL e gestiunea în care se asamblează — și tot în ea se
                  nasc loturile produselor (`GestiuneLoturiCulese`, F19-D3).
                  Caption-ul bazei („Predator (de la)") e corect, dar prea
                  abstract pentru ecranul de asamblare. */}
              <Lookup<AsmWrite>
                camp="PredatorId"
                entitate="Gestiune"
                mod="local"
                eticheta="Gestiunea în care se asamblează"
                cauta={['Cod', 'Denumire']}
              />
              <div>
                <Lookup<AsmWrite>
                  camp="PrimitorId"
                  entitate="Gestiune"
                  mod="local"
                  eticheta="Gestiunea care primește"
                  cauta={['Cod', 'Denumire']}
                />
                <p className="indiciu">
                  De regulă aceeași — dar pot diferi. Lotul produsului se naște în gestiunea în care se
                  asamblează, nu în cea care primește.
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
              serverul, nu editează. Etichetele vin din ReadDto (server-owned)
              sau, pe liniile încă nesalvate, din ce a CULES editorul la selecție;
              lotul produsului și valorile rămân exclusiv ale serverului. */}
          <DataGrid
            dataSource={linii.map((l, i) => ({ ...l, __indice: i, ...etichete(doc?.Linii, l, eticheteLinii[i]) }))}
            keyExpr="__indice"
            showBorders
            columnAutoWidth
            onRowClick={(e) => {
              if (!poateEdita) return;
              const rand = e.data as AsmLinieWrite & { __indice: number };
              setIndiceEditat(rand.__indice);
              setInEditare(linii[rand.__indice]);
            }}
          >
            <Column
              caption={capLinie('Directie')}
              width={90}
              cellRender={(c) => labelEnum('DirectieAsamblare', (c.data as { Directie?: string | null }).Directie)}
            />
            <Column dataField="TipMaterialCod" caption="Cod tip" />
            <Column dataField="TipMaterialDenumire" caption={capLinie('TipMaterialId')} />
            {/* Produsul e al liniei de PRODUS (naște lotul); pe consum rămâne gol
                prin construcție — serverul îl golește. */}
            <Column dataField="ProdusDenumire" caption={capLinie('ProdusId')} />
            {/* Lotul: pe consum e pinul cules, pe produs îl naște serverul la
                salvare. */}
            <Column dataField="LotEticheta" caption={capLinie('LotId')} />
            <Column dataField="Cantitate" caption={capLinie('Cantitate')} dataType="number" format="#,##0.###" alignment="right" />
            <Column dataField="PretEvaluare" caption={capLinie('PretEvaluare')} dataType="number" format="#,##0.######" alignment="right" />
            {/* `Valoare` e SEMNATĂ de server încă de la culegere (F19-D8):
                consumul negativ, produsul pozitiv. Nu se calculează în TS. */}
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
            // `key` = poziția editată: schimbarea liniei REMONTEAZĂ editorul.
            <AsmEditorLinie
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

// Cifrele invariantului nu se recalculează în TS: cât timp agregatul local
// diferă de ce e salvat, ce a spus serverul nu mai descrie ecranul — se spune
// asta, nu se inventează o cifră nouă (43c).
function cifra(valoare: number | undefined, modificat: boolean): ReactNode {
  if (modificat || valoare == null)
    return <em title="Cifrele le calculează serverul la salvare.">— recalculat la salvare</em>;
  return valoare.toFixed(2);
}

// Etichetele unei linii: cele CULESE în sesiunea asta (alegerea proaspătă a
// operatorului) bat ReadDto-ul, care se caută după `Id`; linia nouă fără
// culegere rămâne goală — nu le inventăm în TS.
function etichete(citite: AsmLinieRead[] | null | undefined, linie: AsmLinieWrite, culese?: EticheteCulese) {
  const g = citite?.find((c) => c.Id === linie.Id);
  return {
    TipMaterialCod: culese?.TipMaterialCod ?? g?.TipMaterialCod ?? '',
    TipMaterialDenumire: culese?.TipMaterialDenumire ?? g?.TipMaterialDenumire ?? '',
    ProdusDenumire: culese?.ProdusDenumire ?? g?.ProdusDenumire ?? '',
    LotEticheta: culese?.LotEticheta ?? g?.LotEticheta ?? '',
    Valoare: g?.Valoare,
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

// Sumarul din bara documentului: starea + diferența invariantului. `Total`-ul
// draftului ASM E diferența (valorile sunt semnate de la culegere — F19-D8),
// deci se arată o singură cifră, cea care înseamnă ceva.
function Sumar(props: { stare?: string | null; diferenta?: number; modificat: boolean }) {
  return (
    <div className="sumar">
      <span className="sumar__stare">{labelEnum('StareDocument', props.stare) || 'nesalvat'}</span>
      <span className="sumar__total">
        Diferență: {cifra(props.diferenta, props.modificat)}
      </span>
    </div>
  );
}
