import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { Link, useNavigate, useParams } from 'react-router';
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
import { rutaTip } from '../../nucleu/stingeri';
import {
  antetGol, linieGoala, nir, spreWrite,
  SCHEMA_ANTET, SCHEMA_LINIE, TIP_ANTET, TIP_LINIE,
  type NirLinieRead, type NirLinieWrite, type NirWrite,
} from './api';
import { NirEditorLinie, type EticheteCulese } from './NirEditorLinie';

// Felia verticală NIR, ecranul de document — șablonul consolidat (43c):
//   (1) server-read  → `useQuery` pe ReadDto (affordances, Total, etichete);
//   (2) formular     → `agregat`: WriteDto ÎNTREG, o singură valoare locală;
//   (3) efemeride    → linia în editare, erorile afișate.
// URL-ul e starea globală: `/nir/nou` și `/nir/:id`.
//
// Fluxul pe care îl deblochează felia 5: RECEPȚIA FĂRĂ FACTURĂ — marfa intră pe
// aviz, NIR-ul se culege manual (loturile se nasc pe liniile lui), factura vine
// ulterior. Ecranul rămâne în același timp destinația CONEXULUI: FCT operată →
// `ConexId` → aici, unde recepția parțială se corectează pe cantitate, iar marfa
// și prețul rămân ale lotului moștenit (F5-D4).

const CAMPURI_ANTET: (keyof NirWrite & string)[] = ['Data', 'PredatorId', 'PrimitorId'];
const capLinie = (m: string) => campMeta(TIP_LINIE, m, SCHEMA_LINIE).caption;

export function NirDetaliu() {
  const { id } = useParams();
  const nou = id === 'nou' || id === undefined;
  const navigheaza = useNavigate();
  const cache = useQueryClient();

  const [agregat, setAgregat] = useState<NirWrite>(antetGol);
  const [modificat, setModificat] = useState(false);
  const [aratErori, setAratErori] = useState(false);
  const [erori, setErori] = useState<string[]>([]);
  const [mesaje, setMesaje] = useState<string[]>([]);
  const [inEditare, setInEditare] = useState<NirLinieWrite | null>(null);
  const [indiceEditat, setIndiceEditat] = useState<number | null>(null);
  // Etichetele CULESE în sesiunea asta, per POZIȚIE de linie (paralel cu
  // `agregat.Linii`): grila le folosește cât timp linia n-are încă etichete
  // server-owned în ReadDto (documentul/linia nesalvată).
  const [eticheteLinii, setEticheteLinii] = useState<(EticheteCulese | undefined)[]>([]);

  const citit = useQuery({
    queryKey: ['nir', id],
    queryFn: () => nir.citeste(id!),
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
    mutationFn: async () => (nou ? nir.creeaza(agregat) : nir.actualizeaza(id!, agregat)),
    onSuccess: (salvat) => {
      setErori([]);
      setMesaje(['Salvat.']);
      setModificat(false);
      cache.invalidateQueries({ queryKey: ['nir'] });
      if (nou) navigheaza(`/nir/${salvat.Id}`, { replace: true });
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
      await cache.invalidateQueries({ queryKey: ['nir'] });
      // Anularea/stornarea NIR-ului schimbă și ce poate face FACTURA-sursă
      // (gardienii de grup conex) — cache-ul ei nu mai e de încredere.
      await cache.invalidateQueries({ queryKey: ['fct'] });
      // Recepția mișcă stocul: soldul afișat în altă parte nu mai e valabil.
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
      await nir.sterge(id!);
      await cache.invalidateQueries({ queryKey: ['nir'] });
      // Draftul CONEX șters dispare din grupul facturii-sursă.
      await cache.invalidateQueries({ queryKey: ['fct'] });
      navigheaza('/nir');
    }
    catch (e) {
      setErori(eroriDin(e));
    }
  }

  // Ruta sursei prin `rutaTip` + TIPUL din ReadDto (D-6b): azi sursa e mereu o
  // factură de intrare, dar clientul nu presupune asta — un tip fără felie de
  // client rămâne text, nu link mort.
  const rutaSursa = doc?.DocumentSursaId ? rutaTip(doc.DocumentSursaTip, doc.DocumentSursaId) : null;

  const comenzi: Comanda[] = [
    { eticheta: nou ? 'Creează' : 'Salvează', disponibila: poateEdita, primara: true, ruleaza: salveaza },
    {
      eticheta: 'Verifică',
      disponibila: !nou && !modificat,
      ruleaza: () => void raporteaza(nir.valideaza(id!).then((e) => {
        if (e.length > 0) { setErori(e); return []; }
        return ['Documentul trece toți gardienii motorului.'];
      })),
    },
    { eticheta: 'Operează', disponibila: (doc?.PoateOpera ?? false) && !modificat, ruleaza: () => void comanda(() => nir.opereaza(id!)) },
    { eticheta: 'Anulează operarea', disponibila: doc?.PoateAnula ?? false, ruleaza: () => void comanda(() => nir.anuleaza(id!)) },
    {
      eticheta: 'Stornează',
      disponibila: doc?.PoateStorna ?? false,
      cereData: { eticheta: 'Data stornării' },
      ruleaza: (data) => { if (data) void comanda(() => nir.storneaza(id!, data)); },
    },
    // Draftul AUTOGENERAT e artefactul operării facturii, nu un document al
    // operatorului: serverul îi refuză ștergerea (review advers F5-F2 — pe o
    // factură numai cu linii de stoc el poartă singura postare a datoriei), iar
    // butonul n-are ce căuta activ. Editarea RĂMÂNE (F5-D8b, recepția parțială).
    { eticheta: 'Șterge', disponibila: !nou && poateEdita && !doc?.Autogenerat, ruleaza: () => void stergeDocumentul() },
    { eticheta: 'Înapoi la listă', disponibila: true, ruleaza: () => navigheaza('/nir') },
  ];

  function schimbaAntet(v: NirWrite) {
    setAgregat(v);
    setModificat(true);
  }

  function salveazaLinie(linie: NirLinieWrite, culese: EticheteCulese) {
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
      titlu={nou ? 'NIR — nou' : `NIR ${doc?.Numar ?? ''}`}
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
              {/* `Numar` e AFIȘARE: NIR are politică de numerotare („NIR-"),
                  deci seria se asignează la operare și nici nu există în WriteDto
                  (F5-D8). Pe un draft e gol — asta e adevărul, nu o lipsă. */}
              <Static membru="Numar" valoare={doc?.Numar} />
              <CampData<NirWrite> camp="Data" />
              {/* Furnizorul: 129k parteneri ⇒ căutare server-side. */}
              <Lookup<NirWrite> camp="PredatorId" entitate="Partener" mod="remote" cauta={['Denumire', 'Cod', 'CodFiscal']} />
              {/* Gestiunea primitoare: tot din ea se nasc loturile liniilor
                  culese manual (`GestiuneLoturiCulese` — F5-D2). */}
              <Lookup<NirWrite> camp="PrimitorId" entitate="Gestiune" mod="local" cauta={['Cod', 'Denumire']} />
              <Static membru="Stare" valoare={labelEnum('StareDocument', doc?.Stare)} />
              <Static membru="DataOperare" valoare={doc?.DataOperare?.slice(0, 10)} />
              <Static membru="Autogenerat" valoare={nou ? null : (doc?.Autogenerat ? 'da' : 'nu')} />
              <Static
                membru="DocumentSursaId"
                valoare={doc?.DocumentSursaId
                  ? (rutaSursa
                    ? <Link to={rutaSursa}>{doc.DocumentSursaNumar || 'Deschide documentul sursă'}</Link>
                    : `${doc.DocumentSursaTip ?? ''} ${doc.DocumentSursaNumar ?? ''}`.trim() || '—')
                  : null}
              />
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
              (nimic inventat în TS); lotul și valorile rămân exclusiv ale
              serverului — apar după Salvează. */}
          <DataGrid
            dataSource={linii.map((l, i) => ({ ...l, __indice: i, ...etichete(doc?.Linii, l, eticheteLinii[i]) }))}
            keyExpr="__indice"
            showBorders
            columnAutoWidth
            onRowClick={(e) => {
              if (!poateEdita) return;
              const rand = e.data as NirLinieWrite & { __indice: number };
              setIndiceEditat(rand.__indice);
              setInEditare(linii[rand.__indice]);
            }}
          >
            <Column dataField="TipMaterialCod" caption="Cod tip" />
            <Column dataField="TipMaterialDenumire" caption={capLinie('TipMaterialId')} />
            <Column dataField="ProdusDenumire" caption={capLinie('ProdusId')} />
            {/* Lotul e server-owned (F5-D4): născut din produs la salvare, sau
                moștenit de la factură. Coloana „moștenit" citește `LotStrain`
                din ReadDto — proveniența e decisă în SQL, nu ghicită aici. */}
            <Column dataField="LotEticheta" caption={capLinie('LotId')} />
            <Column
              caption="Lot moștenit"
              width={110}
              cellRender={(c) => ((c.data as { LotStrain?: boolean }).LotStrain ? 'da' : '—')}
            />
            <Column dataField="Cantitate" caption={capLinie('Cantitate')} dataType="number" format="#,##0.###" alignment="right" />
            <Column dataField="PretUnitar" caption={capLinie('PretUnitar')} dataType="number" format="#,##0.######" alignment="right" />
            {/* Valorile sunt ale SERVERULUI (43b): apar după Salvează, nu se
                calculează în TS nici măcar ca previzualizare. */}
            <Column dataField="Valoare" caption={`${capLinie('Valoare')} (server)`} dataType="number" format="#,##0.00" alignment="right" />
            {/* TVA-ul e INFORMATIV pe NIR (F5-D5): clona conexă îl poartă de pe
                factură, ecranul nu-l culege niciodată. */}
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
            // starea lui locală să nu rămână a liniei precedente.
            <NirEditorLinie
              key={indiceEditat ?? 'linie-noua'}
              linie={inEditare}
              lotStrain={etichete(doc?.Linii, inEditare).LotStrain}
              lotEticheta={etichete(doc?.Linii, inEditare).LotEticheta}
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
// culegere rămâne goală — nu le inventăm în TS. Lotul, proveniența lui și
// valorile n-au variantă culeasă: sunt ale serverului prin construcție
// (F5-D4/D6). `LotStrain` cade pe `false` pe linia nesalvată — corect: fără lot
// nu există „moștenit".
function etichete(citite: NirLinieRead[] | null | undefined, linie: NirLinieWrite, culese?: EticheteCulese) {
  const g = citite?.find((c) => c.Id === linie.Id);
  return {
    TipMaterialCod: culese?.TipMaterialCod ?? g?.TipMaterialCod ?? '',
    TipMaterialDenumire: culese?.TipMaterialDenumire ?? g?.TipMaterialDenumire ?? '',
    ProdusDenumire: culese?.ProdusDenumire ?? g?.ProdusDenumire ?? '',
    LotEticheta: g?.LotEticheta ?? '',
    LotStrain: g?.LotStrain ?? false,
    Valoare: g?.Valoare,
    ValoareTva: g?.ValoareTva,
    TipTvaCod: g?.TipTvaCod ?? '',
  };
}

// Câmp de AFIȘARE în interiorul formularului: aceeași ramă ca la culegere
// (etichetă din metadata, slot de control), cu text în locul editorului.
// Membrii server-owned nu există în WriteDto, deci `campMeta` îi dă corect
// NEobligatorii — nimeni nu cere operatorului un câmp pe care nu-l poate scrie.
function Static(props: { membru: string; valoare: ReactNode }) {
  const meta = { ...campMeta(TIP_ANTET, props.membru, SCHEMA_ANTET), obligatoriu: false };
  return (
    <CampShell meta={meta}>
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
