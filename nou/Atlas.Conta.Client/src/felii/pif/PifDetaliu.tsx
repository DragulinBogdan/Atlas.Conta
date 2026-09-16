import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Column, DataGrid } from 'devextreme-react/data-grid';
import { DocumentShell, type Comanda } from '../../nucleu/DocumentShell';
import { CorectieDocument } from '../../nucleu/CorectieDocument';
import { ConfirmareInline } from '../../nucleu/ConfirmareInline';
import { Formular, eroriStructurale } from '../../nucleu/formular';
import { CampData } from '../../nucleu/campuri';
import { Lookup } from '../../nucleu/Lookup';
import { PanouErori } from '../../nucleu/PanouErori';
import { campMeta, labelEnum } from '../../nucleu/campMeta';
import { eroriDin } from '../../nucleu/http';
import { bani } from '../../nucleu/format';
import {
  antetGol, linieGoala, pif, spreWrite,
  SCHEMA_ANTET, SCHEMA_LINIE, TIP_ANTET, TIP_LINIE,
  type PifLinieRead, type PifLinieWrite, type PifWrite,
} from './api';
import { PifEditorLinie, type EticheteCulese } from './PifEditorLinie';

// Șablonul DVI (43c); primitorul e LOCUL fișelor, un document acoperă un singur loc (F26-D5).

const CAMPURI_ANTET: (keyof PifWrite & string)[] = ['Data', 'DataInregistrare', 'PredatorId', 'PrimitorId'];
const capAntet = (m: string) => campMeta(TIP_ANTET, m, SCHEMA_ANTET).caption;
const capLinie = (m: string) => campMeta(TIP_LINIE, m, SCHEMA_LINIE).caption;

export function PifDetaliu() {
  const { id } = useParams();
  const nou = id === 'nou' || id === undefined;
  const navigheaza = useNavigate();
  const cache = useQueryClient();

  const [agregat, setAgregat] = useState<PifWrite>(antetGol);
  const [modificat, setModificat] = useState(false);
  const [aratErori, setAratErori] = useState(false);
  const [erori, setErori] = useState<string[]>([]);
  const [mesaje, setMesaje] = useState<string[]>([]);
  const [deSters, setDeSters] = useState(false);
  const [inEditare, setInEditare] = useState<PifLinieWrite | null>(null);
  const [indiceEditat, setIndiceEditat] = useState<number | null>(null);
  const [eticheteLinii, setEticheteLinii] = useState<(EticheteCulese | undefined)[]>([]);

  const citit = useQuery({
    queryKey: ['pif', id],
    queryFn: () => pif.citeste(id!),
    enabled: !nou,
  });

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

  async function invalideaza() {
    await cache.invalidateQueries({ queryKey: ['pif'] });
    // Operarea scrie `Stare`, `DataPunereInFunctiune` și registrul fișelor.
    await cache.invalidateQueries({ queryKey: ['imobilizari'] });
  }

  const salvare = useMutation({
    mutationFn: async () => (nou ? pif.creeaza(agregat) : pif.actualizeaza(id!, agregat)),
    onSuccess: (salvat) => {
      setErori([]);
      setMesaje(['Salvat.']);
      setModificat(false);
      void invalideaza();
      if (nou) navigheaza(`/pif/${salvat.Id}`, { replace: true });
      else {
        setAgregat(spreWrite(salvat));
        setEticheteLinii([]);
      }
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
      setMesaje([
        `Stare nouă: ${labelEnum('StareDocument', rezultat.StareNoua)}`,
        ...(rezultat.Mesaje ?? []),
      ]);
      await invalideaza();
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
      await pif.sterge(id!);
      await invalideaza();
      navigheaza('/pif');
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
      ruleaza: () => void raporteaza(pif.valideaza(id!).then((e) => {
        if (e.length > 0) { setErori(e); return []; }
        return ['Documentul trece toți gardienii motorului.'];
      })),
    },
    { eticheta: 'Operează', disponibila: (doc?.PoateOpera ?? false) && !modificat, ruleaza: () => void comanda(() => pif.opereaza(id!)) },
    { eticheta: 'Anulează operarea', disponibila: doc?.PoateAnula ?? false, ruleaza: () => void comanda(() => pif.anuleaza(id!)) },
    {
      eticheta: 'Stornează',
      disponibila: doc?.PoateStorna ?? false,
      cereData: { eticheta: 'Data stornării' },
      ruleaza: (data) => { if (data) void comanda(() => pif.storneaza(id!, data)); },
    },
    { eticheta: 'Șterge', disponibila: !nou && (doc?.PoateSterge ?? false), ruleaza: () => setDeSters(true) },
    { eticheta: 'Înapoi la listă', disponibila: true, ruleaza: () => navigheaza('/pif') },
  ];

  function schimbaAntet(v: PifWrite) {
    setAgregat(v);
    setModificat(true);
  }

  function salveazaLinie(linie: PifLinieWrite, culese: EticheteCulese) {
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

  // Liniile sursă legate de CELELALTE linii: dialogul candidaților le scoate,
  // ca aceeași linie de factură să nu hrănească de două ori același document.
  const liniiSursaFolosite = linii
    .filter((_, i) => i !== indiceEditat)
    .map((l) => l.LinieSursaId)
    .filter((v): v is string => v != null);

  return (
    <DocumentShell
      corectie={<CorectieDocument id={doc?.Id} stare={doc?.Stare} corectie={doc?.Corectie} />}
      citire={citit}
      titlu={nou ? 'Punere în funcțiune — nouă' : `Punere în funcțiune ${doc?.Numar ?? ''}`}
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
              <CampData<PifWrite> camp="Data" />
              <CampData<PifWrite> camp="DataInregistrare" />
              {/* Predatorul e unitatea internă care pune în funcțiune. */}
              <Lookup<PifWrite> camp="PredatorId" entitate="UnitateInterna" mod="local" />
              <div>
                {/* Primitorul e LOCUL pe care stau fișele — un `Repartitor`
                    (gestiune, unitate, angajat), nu o unitate internă. */}
                <Lookup<PifWrite> camp="PrimitorId" entitate="Repartitor" mod="remote" afisare={codSiDenumire} />
                <p className="indiciu">
                  Un document de punere în funcțiune acoperă un singur loc: fișele liniilor trebuie
                  să aibă locul primitorului.
                </p>
              </div>
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
              disabled={!poateEdita || inEditare != null || !agregat.PrimitorId}
              onClick={() => { setIndiceEditat(null); setInEditare(linieGoala()); }}
            >
              Adaugă linie
            </button>
            {!agregat.PrimitorId && <span className="indiciu">Alegeți întâi locul.</span>}
          </div>

          {/* Grilă READONLY peste liniile agregatului local: nu vorbește cu
              serverul, nu editează. Etichetele vin din ReadDto sau din ce a
              CULES editorul la selecție. */}
          <DataGrid
            dataSource={linii.map((l, i) => ({ ...l, __indice: i, ...etichete(doc?.Linii, l, eticheteLinii[i]) }))}
            keyExpr="__indice"
            showBorders
            columnAutoWidth
            onRowClick={(e) => {
              if (!poateEdita) return;
              const rand = e.data as PifLinieWrite & { __indice: number };
              setIndiceEditat(rand.__indice);
              setInEditare(linii[rand.__indice]);
            }}
          >
            <Column dataField="NumarInventar" caption="Număr de inventar" />
            <Column dataField="ImobilizareDenumire" caption={capLinie('ImobilizareId')} />
            <Column
              dataField="Fel"
              caption={capLinie('Fel')}
              width={150}
              calculateCellValue={(r: PifLinieWrite) => labelEnum('FelLiniePif', r.Fel)}
            />
            <Column dataField="LinieSursaNumar" caption={capLinie('LinieSursaId')} />
            <Column dataField="Valoare" caption={capLinie('Valoare')} dataType="number" format="#,##0.00" alignment="right" />
            <Column dataField="ValoareFiscala" caption={capLinie('ValoareFiscala')} dataType="number" format="#,##0.00" alignment="right" />
            <Column
              dataField="Metoda"
              caption={capLinie('Metoda')}
              width={120}
              calculateCellValue={(r: PifLinieWrite) => labelEnum('MetodaAmortizare', r.Metoda)}
            />
            <Column dataField="DurataLuni" caption={capLinie('DurataLuni')} dataType="number" width={100} alignment="right" />
            <Column
              dataField="MetodaFiscala"
              caption={capLinie('MetodaFiscala')}
              width={120}
              calculateCellValue={(r: PifLinieWrite) => labelEnum('MetodaAmortizare', r.MetodaFiscala)}
            />
            <Column dataField="DurataFiscalaLuni" caption={capLinie('DurataFiscalaLuni')} dataType="number" width={110} alignment="right" />
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
            <PifEditorLinie
              key={indiceEditat ?? 'linie-noua'}
              linie={inEditare}
              eticheteInitiale={indiceEditat == null ? undefined : eticheteLinii[indiceEditat]}
              primitorId={agregat.PrimitorId}
              data={agregat.Data}
              liniiSursaFolosite={liniiSursaFolosite}
              readOnly={!poateEdita}
              onSalveaza={salveazaLinie}
              onRenunta={() => { setInEditare(null); setIndiceEditat(null); }}
            />
          )}
        </>
      }
      subsol={
        <p className="indiciu">
          Punerea în funcțiune nu postează în registrul contabil: ea deschide fișa și scrie evenimentul
          în registrul de imobilizări. Luna punerii în funcțiune nu se amortizează, iar parametrii unei
          revizuiri curg din luna următoare evenimentului.
        </p>
      }
    />
  );
}

// Etichetele unei linii: cele CULESE în sesiunea asta bat ReadDto-ul, care se
// caută după `Id`.
function etichete(citite: PifLinieRead[] | null | undefined, linie: PifLinieWrite, culese?: EticheteCulese) {
  const g = citite?.find((c) => c.Id === linie.Id);
  return {
    NumarInventar: culese?.NumarInventar ?? g?.NumarInventar ?? '',
    ImobilizareDenumire: culese?.ImobilizareDenumire ?? g?.ImobilizareDenumire ?? '',
    LinieSursaNumar: culese?.LinieSursaNumar ?? g?.LinieSursaNumar ?? '',
  };
}

// `Total` e cifra SERVERULUI (42c): cât timp agregatul e modificat, ce s-ar
// afișa ar fi cifra veche prezentată ca fiind curentă.
function Sumar(props: { stare?: string | null; total?: number; modificat: boolean }) {
  const necunoscut = props.modificat || props.total == null;
  return (
    <div className="sumar">
      <span className="sumar__stare">{labelEnum('StareDocument', props.stare) || 'nesalvat'}</span>
      <span className="sumar__total">
        {capAntet('Total')}: {necunoscut
          ? <em title="Cifrele le calculează serverul la salvare.">— recalculat la salvare</em>
          : bani(props.total)}
      </span>
    </div>
  );
}

function codSiDenumire(element: Record<string, unknown>): string {
  if (!element) return '';
  const cod = element.Cod == null ? '' : String(element.Cod);
  const denumire = element.Denumire == null ? '' : String(element.Denumire);
  return cod && denumire ? `${cod} — ${denumire}` : cod || denumire;
}
