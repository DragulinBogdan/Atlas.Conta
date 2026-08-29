import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { useNavigate, useParams } from 'react-router';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Column, DataGrid } from 'devextreme-react/data-grid';
import { DocumentShell, type Comanda } from '../../nucleu/DocumentShell';
import { ConfirmareInline } from '../../nucleu/ConfirmareInline';
import { CampShell } from '../../nucleu/CampShell';
import { Formular, eroriStructurale } from '../../nucleu/formular';
import { CampData } from '../../nucleu/campuri';
import { Lookup } from '../../nucleu/Lookup';
import { PanouErori } from '../../nucleu/PanouErori';
import { campMeta, labelEnum } from '../../nucleu/campMeta';
import { eroriDin } from '../../nucleu/http';
import {
  antetGol, ldi, linieGoala, spreWrite,
  SCHEMA_ANTET, SCHEMA_LINIE, TIP_ANTET, TIP_LINIE,
  type LdiLinieRead, type LdiLinieWrite, type LdiWrite,
} from './api';
import { LdiEditorLinie, type EticheteCulese } from './LdiEditorLinie';

// Felia verticală LDI, ecranul de document — șablonul consolidat (43c):
//   (1) server-read  → `useQuery` pe ReadDto (affordances, Total, etichete);
//   (2) formular     → `agregat`: WriteDto ÎNTREG, o singură valoare locală;
//   (3) efemeride    → linia în editare, erorile afișate.
// URL-ul e starea globală: `/ldi/nou` și `/ldi/:id`.
//
// Documentul e ÎNTOTDEAUNA cules manual (nu are clonă conexă și nu e secundarul
// nimănui), iar liniile lui merg în ambele sensuri pe aceeași listă: plusuri și
// minusuri, cu un singur set de reguli de stoc — cantitatea semnată la operare
// dă direcția (28a). De aceea `Total` e NET, nu absolut.

const CAMPURI_ANTET: (keyof LdiWrite & string)[] = ['Data', 'PredatorId', 'PrimitorId'];
const capLinie = (m: string) => campMeta(TIP_LINIE, m, SCHEMA_LINIE).caption;

export function LdiDetaliu() {
  const { id } = useParams();
  const nou = id === 'nou' || id === undefined;
  const navigheaza = useNavigate();
  const cache = useQueryClient();

  const [agregat, setAgregat] = useState<LdiWrite>(antetGol);
  const [modificat, setModificat] = useState(false);
  const [aratErori, setAratErori] = useState(false);
  const [erori, setErori] = useState<string[]>([]);
  const [mesaje, setMesaje] = useState<string[]>([]);
  // Ștergerea draftului cere confirmare INLINE (F20-D4), nu `window.confirm`:
  // dialogul nativ blochează renderer-ul și nu poate spune CE se pierde.
  // Starea e a feliei; locul de randare îl dă `DocumentShell`.
  const [deSters, setDeSters] = useState(false);
  const [inEditare, setInEditare] = useState<LdiLinieWrite | null>(null);
  const [indiceEditat, setIndiceEditat] = useState<number | null>(null);
  // Etichetele CULESE în sesiunea asta, per POZIȚIE de linie (paralel cu
  // `agregat.Linii`): grila le folosește cât timp linia n-are încă etichete
  // server-owned în ReadDto (documentul/linia nesalvată).
  const [eticheteLinii, setEticheteLinii] = useState<(EticheteCulese | undefined)[]>([]);

  const citit = useQuery({
    queryKey: ['ldi', id],
    queryFn: () => ldi.citeste(id!),
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
    mutationFn: async () => (nou ? ldi.creeaza(agregat) : ldi.actualizeaza(id!, agregat)),
    onSuccess: (salvat) => {
      setErori([]);
      setMesaje(['Salvat.']);
      setModificat(false);
      cache.invalidateQueries({ queryKey: ['ldi'] });
      if (nou) navigheaza(`/ldi/${salvat.Id}`, { replace: true });
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
      await cache.invalidateQueries({ queryKey: ['ldi'] });
      // Inventarul mișcă stocul în ambele sensuri: soldul afișat în altă parte
      // nu mai e valabil.
      await cache.invalidateQueries({ queryKey: ['stoc'] });
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
      await ldi.sterge(id!);
      await cache.invalidateQueries({ queryKey: ['ldi'] });
      navigheaza('/ldi');
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
      ruleaza: () => void raporteaza(ldi.valideaza(id!).then((e) => {
        if (e.length > 0) { setErori(e); return []; }
        return ['Documentul trece toți gardienii motorului.'];
      })),
    },
    { eticheta: 'Operează', disponibila: (doc?.PoateOpera ?? false) && !modificat, ruleaza: () => void comanda(() => ldi.opereaza(id!)) },
    { eticheta: 'Anulează operarea', disponibila: doc?.PoateAnula ?? false, ruleaza: () => void comanda(() => ldi.anuleaza(id!)) },
    {
      eticheta: 'Stornează',
      disponibila: doc?.PoateStorna ?? false,
      cereData: { eticheta: 'Data stornării' },
      ruleaza: (data) => { if (data) void comanda(() => ldi.storneaza(id!, data)); },
    },
    { eticheta: 'Șterge', disponibila: !nou && poateEdita, ruleaza: () => setDeSters(true) },
    { eticheta: 'Înapoi la listă', disponibila: true, ruleaza: () => navigheaza('/ldi') },
  ];

  function schimbaAntet(v: LdiWrite) {
    setAgregat(v);
    setModificat(true);
  }

  function salveazaLinie(linie: LdiLinieWrite, culese: EticheteCulese) {
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
      titlu={nou ? 'Listă de diferențe — nouă' : `Listă de diferențe ${doc?.Numar ?? ''}`}
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
              {/* `Numar` e AFIȘARE: LDI are politică de numerotare („LDI-"),
                  deci seria se asignează la operare și nici nu există în WriteDto
                  (F6-D4). Pe un draft e gol — asta e adevărul, nu o lipsă. */}
              <Static membru="Numar" valoare={doc?.Numar} />
              <CampData<LdiWrite> camp="Data" />
              {/* Gestiunea INVENTARIATĂ: din ea ies minusurile și tot în ea se
                  nasc loturile plusurilor (`GestiuneLoturiCulese` — F6-D2).
                  Caption-ul bazei („Predator (de la)") e corect, dar prea
                  abstract pentru ecranul de inventar — felia îl numește în
                  vocabularul ei (escapa `eticheta`). */}
              <Lookup<LdiWrite>
                camp="PredatorId"
                entitate="Gestiune"
                mod="local"
                eticheta="Gestiunea inventariată"
              />
              <div>
                {/* Primitorul e COMISIA de inventariere (28d): un intern purtător
                    al calității `Comisie`. Lookup-ul e NEFILTRAT pe calitate
                    (F6-D8): filtrarea pe `Calitati` (flags) nu traversează lanțul
                    DevExtreme→OData — refuzul e al motorului. */}
                <Lookup<LdiWrite>
                  camp="PrimitorId"
                  entitate="UnitateInterna"
                  mod="local"
                  eticheta="Comisia de inventariere"
                />
                <p className="indiciu">
                  Trebuie să fie comisia de inventariere (calitatea „Comisie”) — lista nu e filtrată pe
                  calitate, iar operarea refuză alegerile greșite.
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
              sau, pe liniile încă nesalvate, din ce a CULES editorul la selecție
              (nimic inventat în TS); lotul plusului și valorile rămân exclusiv
              ale serverului — apar după Salvează. */}
          <DataGrid
            dataSource={linii.map((l, i) => ({ ...l, __indice: i, ...etichete(doc?.Linii, l, eticheteLinii[i]) }))}
            keyExpr="__indice"
            showBorders
            columnAutoWidth
            onRowClick={(e) => {
              if (!poateEdita) return;
              const rand = e.data as LdiLinieWrite & { __indice: number };
              setIndiceEditat(rand.__indice);
              setInEditare(linii[rand.__indice]);
            }}
          >
            <Column
              caption={capLinie('Directie')}
              width={90}
              cellRender={(c) => labelEnum('DirectieDiferenta', (c.data as { Directie?: string | null }).Directie)}
            />
            <Column dataField="TipMaterialCod" caption="Cod tip" />
            <Column dataField="TipMaterialDenumire" caption={capLinie('TipMaterialId')} />
            {/* Produsul e al PLUSULUI (naște lotul); pe minus rămâne gol prin
                construcție — serverul îl golește (F6-D3). */}
            <Column dataField="ProdusDenumire" caption={capLinie('ProdusId')} />
            {/* Lotul: pe minus e pinul cules, pe plus îl naște serverul la
                salvare (F6-D5). */}
            <Column dataField="LotEticheta" caption={capLinie('LotId')} />
            <Column dataField="Cantitate" caption={capLinie('Cantitate')} dataType="number" format="#,##0.###" alignment="right" />
            <Column dataField="PretEvaluare" caption={capLinie('PretEvaluare')} dataType="number" format="#,##0.######" alignment="right" />
            {/* `Valoare` e SEMNATĂ de server încă de la culegere (F6-D6):
                minusul negativ, plusul pozitiv. Nu se calculează în TS. */}
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
            <LdiEditorLinie
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
// culeasă: e a serverului prin construcție (F6-D6).
function etichete(citite: LdiLinieRead[] | null | undefined, linie: LdiLinieWrite, culese?: EticheteCulese) {
  const g = citite?.find((c) => c.Id === linie.Id);
  return {
    TipMaterialCod: culese?.TipMaterialCod ?? g?.TipMaterialCod ?? '',
    TipMaterialDenumire: culese?.TipMaterialDenumire ?? g?.TipMaterialDenumire ?? '',
    ProdusDenumire: culese?.ProdusDenumire ?? g?.ProdusDenumire ?? '',
    LotEticheta: culese?.LotEticheta ?? g?.LotEticheta ?? '',
    Valoare: g?.Valoare,
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

// `Total` = valoarea din ReadDto, redusă de SERVER — NET pe listă de diferențe
// (plusuri − minusuri). La editare nesalvată nu există (baza s-a schimbat) — se
// marchează explicit, nu se recalculează în TS (43c).
function Sumar(props: { stare?: string | null; total?: number; modificat: boolean }) {
  return (
    <div className="sumar">
      <span className="sumar__stare">{labelEnum('StareDocument', props.stare) || 'nesalvat'}</span>
      <span className="sumar__total">
        Total net: {props.modificat || props.total == null
          ? <em title="Totalul îl calculează serverul la salvare.">— recalculat la salvare</em>
          : props.total.toFixed(2)}
      </span>
    </div>
  );
}
