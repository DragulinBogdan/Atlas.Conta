import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { SelectBox } from 'devextreme-react';
import { Column, DataGrid } from 'devextreme-react/data-grid';
import DataSource from 'devextreme/data/data_source';
import { DocumentShell, type Comanda } from '../../nucleu/DocumentShell';
import { ConfirmareInline } from '../../nucleu/ConfirmareInline';
import { Formular, eroriStructurale } from '../../nucleu/formular';
import { CampData, CampSelectie } from '../../nucleu/campuri';
import { Lookup, tipuriGuid } from '../../nucleu/Lookup';
import { PanouErori } from '../../nucleu/PanouErori';
import { campMeta, labelEnum } from '../../nucleu/campMeta';
import { eroriDin } from '../../nucleu/http';
import { storeOData } from '../../nucleu/odata';
import { bani } from '../../nucleu/format';
import {
  antetGol, cas, spreWrite,
  SCHEMA_ANTET, SCHEMA_LISTA, TIP_ANTET, TIP_LINIE,
  type CasWrite,
} from './api';

// Antetul și fișele se culeg; liniile le produce serverul din situația fișelor la `Data` (F26-D6).

const CAMPURI_ANTET: (keyof CasWrite & string)[] = ['Data', 'Cauza', 'PredatorId', 'PrimitorId'];
const capAntet = (m: string) => campMeta(TIP_ANTET, m, SCHEMA_ANTET).caption;
const capLinie = (m: string) => campMeta(TIP_LINIE, m, 'CasLinieReadDto').caption;

type FisaCulesa = { NumarInventar: string; Denumire: string };

export function CasDetaliu() {
  const { id } = useParams();
  const nou = id === 'nou' || id === undefined;
  const navigheaza = useNavigate();
  const cache = useQueryClient();

  const [agregat, setAgregat] = useState<CasWrite>(antetGol);
  const [modificat, setModificat] = useState(false);
  const [aratErori, setAratErori] = useState(false);
  const [erori, setErori] = useState<string[]>([]);
  const [mesaje, setMesaje] = useState<string[]>([]);
  const [deSters, setDeSters] = useState(false);
  // Fișele alese în sesiunea asta, pe id: ce a afișat selectorul, cât timp
  // documentul n-are încă ReadDto pentru ele.
  const [fiseCulese, setFiseCulese] = useState<Record<string, FisaCulesa>>({});
  const [fisaDeAdaugat, setFisaDeAdaugat] = useState<string | null>(null);
  const [localeErori, setLocaleErori] = useState<string[]>([]);

  const citit = useQuery({
    queryKey: ['cas', id],
    queryFn: () => cas.citeste(id!),
    enabled: !nou,
  });

  useEffect(() => {
    if (citit.data) {
      setAgregat(spreWrite(citit.data));
      setModificat(false);
      setFiseCulese({});
    }
  }, [citit.data]);

  const doc = citit.data;
  const poateEdita = nou || (doc?.PoateEdita ?? false);
  const fise = agregat.Fise ?? [];

  // Fișele în funcțiune de pe LOCUL predatorului. GUID-ul e recunoscut de
  // `tipuriGuid`; `Stare` pleacă pe sârmă ca literal de string.
  const predatorId = agregat.PredatorId;
  const sursaFise = useMemo(() => {
    if (!predatorId) return null;
    const filtru = [['LocId', '=', predatorId], 'and', ['Stare', '=', 'InFunctiune']];
    return new DataSource({
      store: storeOData('Imobilizare', { fieldTypes: tipuriGuid(filtru) }),
      filter: filtru,
      sort: 'NumarInventar',
      paginate: true,
      pageSize: 50,
    });
  }, [predatorId]);

  const structurale = useMemo(
    () => [
      ...eroriStructurale(TIP_ANTET, SCHEMA_ANTET, agregat as Record<string, unknown>, CAMPURI_ANTET),
      ...(agregat.Cauza ? [] : [`„${capAntet('Cauza')}” este obligatorie.`]),
      ...(fise.length === 0 ? ['Documentul n-are nicio fișă.'] : []),
    ],
    [agregat, fise.length]);

  function raporteaza(promisiune: Promise<string[]>) {
    setErori([]);
    setMesaje([]);
    return promisiune
      .then((m) => setMesaje(m))
      .catch((e) => setErori(eroriDin(e)));
  }

  async function invalideaza() {
    await cache.invalidateQueries({ queryKey: ['cas'] });
    // Operarea scrie `Stare`, `DataIesire` și registrul fișelor.
    await cache.invalidateQueries({ queryKey: ['imobilizari'] });
  }

  const salvare = useMutation({
    mutationFn: async () => (nou ? cas.creeaza(agregat) : cas.actualizeaza(id!, agregat)),
    onSuccess: (salvat) => {
      setErori([]);
      setMesaje(['Salvat.']);
      setModificat(false);
      void invalideaza();
      if (nou) navigheaza(`/cas/${salvat.Id}`, { replace: true });
      else {
        setAgregat(spreWrite(salvat));
        setFiseCulese({});
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
      await cas.sterge(id!);
      await invalideaza();
      navigheaza('/cas');
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
      ruleaza: () => void raporteaza(cas.valideaza(id!).then((e) => {
        if (e.length > 0) { setErori(e); return []; }
        return ['Documentul trece toți gardienii motorului.'];
      })),
    },
    { eticheta: 'Operează', disponibila: (doc?.PoateOpera ?? false) && !modificat, ruleaza: () => void comanda(() => cas.opereaza(id!)) },
    { eticheta: 'Anulează operarea', disponibila: doc?.PoateAnula ?? false, ruleaza: () => void comanda(() => cas.anuleaza(id!)) },
    {
      eticheta: 'Stornează',
      disponibila: doc?.PoateStorna ?? false,
      cereData: { eticheta: 'Data stornării' },
      ruleaza: (data) => { if (data) void comanda(() => cas.storneaza(id!, data)); },
    },
    { eticheta: 'Șterge', disponibila: !nou && (doc?.PoateSterge ?? false), ruleaza: () => setDeSters(true) },
    { eticheta: 'Înapoi la listă', disponibila: true, ruleaza: () => navigheaza('/cas') },
  ];

  function schimbaAntet(v: CasWrite) {
    setAgregat(v);
    setModificat(true);
  }

  function adaugaFisa() {
    if (!fisaDeAdaugat) return;
    if (fise.includes(fisaDeAdaugat)) {
      setLocaleErori(['Fișa e deja pe document — o fișă iese o singură dată.']);
      return;
    }
    setLocaleErori([]);
    setAgregat({ ...agregat, Fise: [...fise, fisaDeAdaugat] });
    setModificat(true);
    setFisaDeAdaugat(null);
  }

  function scoateFisa(fisaId: string) {
    setAgregat({ ...agregat, Fise: fise.filter((f) => f !== fisaId) });
    setModificat(true);
  }

  // Rândurile selectorului: eticheta vine din ce a cules SelectBox-ul sau din
  // liniile ReadDto (o fișă salvată are cel puțin linia de amortizare cumulată).
  const randuriFise = fise.map((fisaId) => {
    const cules = fiseCulese[fisaId];
    const linie = doc?.Linii?.find((l) => l.ImobilizareId === fisaId);
    return {
      ImobilizareId: fisaId,
      NumarInventar: cules?.NumarInventar ?? linie?.NumarInventar ?? '',
      Denumire: cules?.Denumire ?? linie?.ImobilizareDenumire ?? '',
    };
  });

  return (
    <DocumentShell
      citire={citit}
      titlu={nou ? 'Ieșire de imobilizări — nouă' : `Ieșire de imobilizări ${doc?.Numar ?? ''}`}
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
              <CampData<CasWrite> camp="Data" />
              <CampSelectie<CasWrite> camp="Cauza" enumerare="CauzaIesire" obligatoriu />
              {/* Predatorul e LOCUL de pe fișe; primitorul e unitatea internă
                  care înregistrează ieșirea. */}
              <Lookup<CasWrite> camp="PredatorId" entitate="Repartitor" mod="remote" afisare={codSiDenumire} />
              <Lookup<CasWrite> camp="PrimitorId" entitate="UnitateInterna" mod="local" />
            </div>
          </Formular>
        </>
      }
      linii={
        <>
          <div className="linii__bara">
            <h3>Fișele care ies</h3>
            <SelectBox
              dataSource={sursaFise ?? []}
              value={fisaDeAdaugat}
              valueExpr="ID"
              displayExpr={numarSiDenumire}
              disabled={!poateEdita || !predatorId}
              searchEnabled
              searchExpr="Cautare"
              searchTimeout={400}
              showClearButton
              noDataText="Nicio fișă în funcțiune pe locul ales"
              width={360}
              placeholder={predatorId ? 'Căutați fișa' : 'Alegeți întâi locul'}
              onValueChanged={(e) => {
                if (!e.event) return;
                const valoare = (e.value as string) ?? null;
                setFisaDeAdaugat(valoare);
                const selectat = e.component.option('selectedItem') as Record<string, unknown> | null;
                if (valoare && selectat)
                  setFiseCulese((prev) => ({
                    ...prev,
                    [valoare]: {
                      NumarInventar: selectat.NumarInventar == null ? '' : String(selectat.NumarInventar),
                      Denumire: selectat.Denumire == null ? '' : String(selectat.Denumire),
                    },
                  }));
              }}
            />
            <button
              type="button"
              className="buton"
              disabled={!poateEdita || !fisaDeAdaugat}
              onClick={adaugaFisa}
            >
              Adaugă fișa
            </button>
          </div>

          <PanouErori erori={localeErori} titlu="Fișa nu s-a adăugat" fel="atentie" />

          <DataGrid dataSource={randuriFise} keyExpr="ImobilizareId" showBorders columnAutoWidth>
            <Column dataField="NumarInventar" caption="Număr de inventar" />
            <Column dataField="Denumire" caption="Denumire" />
            <Column
              caption=""
              width={90}
              cellRender={(c) => (
                <button
                  type="button"
                  className="buton buton--mic"
                  disabled={!poateEdita}
                  onClick={() => scoateFisa((c.data as { ImobilizareId: string }).ImobilizareId)}
                >
                  Scoate
                </button>
              )}
            />
          </DataGrid>

          <div className="imo__sectiune">
            <h3>Liniile ieșirii</h3>
            {modificat
              ? <p className="indiciu">Liniile se recalculează la salvare.</p>
              : (
                <>
                  <DataGrid dataSource={doc?.Linii ?? []} keyExpr="Id" showBorders columnAutoWidth>
                    <Column dataField="NumarInventar" caption="Număr de inventar" />
                    <Column dataField="ImobilizareDenumire" caption={capLinie('ImobilizareId')} />
                    <Column
                      dataField="Fel"
                      caption={capLinie('Fel')}
                      width={170}
                      calculateCellValue={(r: { Fel?: string | null }) => labelEnum('FelLinieIesire', r.Fel)}
                    />
                    <Column dataField="ContDebitSimbol" caption={capLinie('ContDebitId')} width={140} />
                    <Column dataField="ContCreditSimbol" caption={capLinie('ContCreditId')} width={140} />
                    <Column dataField="Valoare" caption={capLinie('Valoare')} dataType="number" format="#,##0.00" alignment="right" width={160} />
                  </DataGrid>
                  <p className="indiciu">
                    Liniile le produce serverul din situația fișelor la data ieșirii, pe conturile din
                    politica de amortizare; PUT-ul le re-produce. Fiecare fișă are linia de amortizare
                    cumulată și, dacă netul nu e zero, linia de valoare rămasă.
                  </p>
                </>
              )}
          </div>
        </>
      }
    />
  );
}

function Sumar(props: { stare?: string | null; total?: number; modificat: boolean }) {
  const necunoscut = props.modificat || props.total == null;
  return (
    <div className="sumar">
      <span className="sumar__stare">{labelEnum('StareDocument', props.stare) || 'nesalvat'}</span>
      <span className="sumar__total">
        {campMeta(TIP_ANTET, 'Total', SCHEMA_LISTA).caption}: {necunoscut
          ? <em title="Cifrele le calculează serverul la salvare.">— recalculat la salvare</em>
          : bani(props.total)}
      </span>
    </div>
  );
}

function numarSiDenumire(element: Record<string, unknown>): string {
  if (!element) return '';
  const numar = element.NumarInventar == null ? '' : String(element.NumarInventar);
  const denumire = element.Denumire == null ? '' : String(element.Denumire);
  return numar && denumire ? `${numar} — ${denumire}` : numar || denumire;
}

function codSiDenumire(element: Record<string, unknown>): string {
  if (!element) return '';
  const cod = element.Cod == null ? '' : String(element.Cod);
  const denumire = element.Denumire == null ? '' : String(element.Denumire);
  return cod && denumire ? `${cod} — ${denumire}` : cod || denumire;
}
