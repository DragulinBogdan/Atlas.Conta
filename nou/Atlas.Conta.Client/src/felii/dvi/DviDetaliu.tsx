import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Column, DataGrid } from 'devextreme-react/data-grid';
import { DocumentShell, type Comanda } from '../../nucleu/DocumentShell';
import { ConfirmareInline } from '../../nucleu/ConfirmareInline';
import { Formular, eroriStructurale } from '../../nucleu/formular';
import { CampData, CampText } from '../../nucleu/campuri';
import { Lookup } from '../../nucleu/Lookup';
import { LookupGrila } from '../../nucleu/LookupGrila';
import { PanouErori } from '../../nucleu/PanouErori';
import { campMeta, labelEnum } from '../../nucleu/campMeta';
import { eroriDin } from '../../nucleu/http';
import { useNomenclator } from '../../nucleu/odata';
import {
  antetGol, dvi, linieGoala, spreWrite,
  SCHEMA_ANTET, SCHEMA_LINIE, TIP_ANTET, TIP_LINIE,
  type DviLinieRead, type DviLinieWrite, type DviWrite, type FacturaCandidata,
} from './api';
import { DviEditorLinie, type EticheteCulese } from './DviEditorLinie';
import { PanouFacturi } from './PanouFacturi';

// Felia DVI, ecranul de document (DVI-D6) — șablonul FCT/NTC (43c):
//   (1) server-read  → `useQuery` pe ReadDto (affordances, Baza/Tva, etichete);
//   (2) formular     → `agregat`: WriteDto ÎNTREG, o singură valoare locală;
//   (3) efemeride    → linia în editare, facturile culese în sesiune, erorile.
//
// Ce e propriu feliei: laturile sunt ALTELE decât pe FCT (Predatorul e biroul
// vamal — un partener; Primitorul e o unitate internă, nu o gestiune), liniile
// n-au produs și n-au cantitate (baza e valoarea VĂMUITĂ, culeasă), iar
// agregatul poartă o a doua colecție — `FacturiIds`, legăturile cu facturile de
// import, trimise ÎNTREGI la fiecare salvare.

const CAMPURI_ANTET: (keyof DviWrite & string)[] = ['Numar', 'Data', 'PredatorId', 'PrimitorId'];
const capAntet = (m: string) => campMeta(TIP_ANTET, m, SCHEMA_ANTET).caption;
const capLinie = (m: string) => campMeta(TIP_LINIE, m, SCHEMA_LINIE).caption;

export function DviDetaliu() {
  const { id } = useParams();
  const nou = id === 'nou' || id === undefined;
  const navigheaza = useNavigate();
  const cache = useQueryClient();

  const [agregat, setAgregat] = useState<DviWrite>(antetGol);
  const [modificat, setModificat] = useState(false);
  const [aratErori, setAratErori] = useState(false);
  const [erori, setErori] = useState<string[]>([]);
  const [mesaje, setMesaje] = useState<string[]>([]);
  const [deSters, setDeSters] = useState(false);
  const [inEditare, setInEditare] = useState<DviLinieWrite | null>(null);
  const [indiceEditat, setIndiceEditat] = useState<number | null>(null);
  const [eticheteLinii, setEticheteLinii] = useState<(EticheteCulese | undefined)[]>([]);
  // Facturile alese în sesiunea asta, pe id: ce a afișat popup-ul candidaților,
  // cât timp legătura n-are încă ReadDto.
  const [facturiCulese, setFacturiCulese] = useState<Record<string, FacturaCandidata>>({});
  // Tipul de material sugerat de ULTIMA factură adăugată (DVI-D5/D6): serverul
  // îl derivă din liniile facturii, clientul îl propune liniei noi.
  const [tipMaterialSugerat, setTipMaterialSugerat] = useState<string | null>(null);

  const citit = useQuery({
    queryKey: ['dvi', id],
    queryFn: () => dvi.citeste(id!),
    enabled: !nou,
  });

  // ReadDto proaspăt ⇒ formularul se re-seed-uiește. Etichetele culese local mor
  // odată cu re-seed-ul: serverul le are de acum pe toate.
  useEffect(() => {
    if (citit.data) {
      setAgregat(spreWrite(citit.data));
      setModificat(false);
      setEticheteLinii([]);
      setFacturiCulese({});
    }
  }, [citit.data]);

  const doc = citit.data;
  const poateEdita = nou || (doc?.PoateEdita ?? false);
  const linii = agregat.Linii ?? [];
  const facturiIds = agregat.FacturiIds ?? [];

  // Eticheta tipului sugerat: o citire de nomenclator prin cache-ul comun, ca
  // linia precompletată să nu apară fără nume în grilă.
  const materialSugerat = useNomenclator<{ Cod?: string; Denumire?: string }>('TipMaterial', tipMaterialSugerat);

  const structurale = useMemo(
    () => [
      ...eroriStructurale(TIP_ANTET, SCHEMA_ANTET, agregat as Record<string, unknown>, CAMPURI_ANTET),
      // MRN-ul: serverul îl lasă nullable pe DRAFT și îl cere la operare
      // (`Dvi.ValideazaOperare`). Îl cerem la culegere, fiindcă operatorul are
      // declarația în mână — aceeași regulă, arătată mai devreme.
      ...(agregat.Numar?.trim() ? [] : [`„${capAntet('Numar')}” este obligatoriu — declarația poartă MRN-ul ei.`]),
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
    mutationFn: async () => (nou ? dvi.creeaza(agregat) : dvi.actualizeaza(id!, agregat)),
    onSuccess: (salvat) => {
      setErori([]);
      setMesaje(['Salvat.']);
      setModificat(false);
      cache.invalidateQueries({ queryKey: ['dvi'] });
      if (nou) navigheaza(`/dvi/${salvat.Id}`, { replace: true });
      else {
        setAgregat(spreWrite(salvat));
        setEticheteLinii([]);
        setFacturiCulese({});
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
      await cache.invalidateQueries({ queryKey: ['dvi'] });
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
      await dvi.sterge(id!);
      await cache.invalidateQueries({ queryKey: ['dvi'] });
      navigheaza('/dvi');
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
      ruleaza: () => void raporteaza(dvi.valideaza(id!).then((e) => {
        if (e.length > 0) { setErori(e); return []; }
        return ['Documentul trece toți gardienii motorului.'];
      })),
    },
    { eticheta: 'Operează', disponibila: (doc?.PoateOpera ?? false) && !modificat, ruleaza: () => void comanda(() => dvi.opereaza(id!)) },
    { eticheta: 'Anulează operarea', disponibila: doc?.PoateAnula ?? false, ruleaza: () => void comanda(() => dvi.anuleaza(id!)) },
    {
      eticheta: 'Stornează',
      disponibila: doc?.PoateStorna ?? false,
      cereData: { eticheta: 'Data stornării' },
      ruleaza: (data) => { if (data) void comanda(() => dvi.storneaza(id!, data)); },
    },
    { eticheta: 'Șterge', disponibila: !nou && poateEdita, ruleaza: () => setDeSters(true) },
    { eticheta: 'Înapoi la listă', disponibila: true, ruleaza: () => navigheaza('/dvi') },
  ];

  function schimbaAntet(v: DviWrite) {
    setAgregat(v);
    setModificat(true);
  }

  function salveazaLinie(linie: DviLinieWrite, culese: EticheteCulese) {
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

  function adaugaFacturi(alesi: FacturaCandidata[]) {
    const idsNoi = alesi.map((c) => c.FacturaId).filter((v): v is string => v != null);
    if (idsNoi.length === 0)
      return;
    setAgregat({ ...agregat, FacturiIds: [...facturiIds, ...idsNoi] });
    setFacturiCulese((prev) => {
      const urmator = { ...prev };
      for (const c of alesi)
        if (c.FacturaId) urmator[c.FacturaId] = c;
      return urmator;
    });
    const sugerat = [...alesi].reverse().find((c) => c.TipMaterialSugeratId != null);
    if (sugerat?.TipMaterialSugeratId)
      setTipMaterialSugerat(sugerat.TipMaterialSugeratId);
    setModificat(true);
  }

  function scoateFactura(facturaId: string) {
    setAgregat({ ...agregat, FacturiIds: facturiIds.filter((f) => f !== facturaId) });
    setModificat(true);
  }

  // Linie NOUĂ: tipul sugerat de ultima factură adăugată, cu eticheta lui. Nu se
  // aplică niciodată peste o linie existentă și nu se re-aplică peste o alegere
  // a operatorului — editorul pornește de la valoarea asta, nu o impune.
  function linieNoua(): DviLinieWrite {
    return tipMaterialSugerat
      ? { ...linieGoala(), TipMaterialId: tipMaterialSugerat }
      : linieGoala();
  }

  const eticheteLinieNoua: EticheteCulese | undefined = tipMaterialSugerat && materialSugerat
    ? { TipMaterialCod: materialSugerat.Cod ?? '', TipMaterialDenumire: materialSugerat.Denumire ?? '' }
    : undefined;

  return (
    <DocumentShell
      citire={citit}
      titlu={nou ? 'Declarație vamală de import — nouă' : `Declarație vamală ${doc?.Numar ?? ''}`}
      sumar={<Sumar stare={doc?.Stare} baza={doc?.Baza} tva={doc?.Tva} modificat={modificat || nou} />}
      comenzi={comenzi}
      confirmare={deSters && (
        <ConfirmareInline
          intrebare="Ștergeți definitiv acest draft, cu tot cu liniile și legăturile lui?"
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
              {/* `Numar` E MRN-ul declarației, cules — nu o serie proprie. */}
              <CampText<DviWrite> camp="Numar" obligatoriu />
              <CampData<DviWrite> camp="Data" />
              {/* Predatorul e PARTENERUL căruia i se datorează taxa (biroul
                  vamal, cu cont implicit 446): 129k parteneri ⇒ lookup cu grilă,
                  căutare server-side. */}
              <LookupGrila<DviWrite>
                camp="PredatorId"
                entitate="Partener"
                cauta={['Cautare', 'CodFiscal']}
                eticheta="Biroul vamal"
              />
              {/* Primitorul e o UNITATE INTERNĂ, nu o gestiune: declarația nu
                  mișcă stoc (`Dvi.ValideazaOperare`). */}
              <Lookup<DviWrite> camp="PrimitorId" entitate="UnitateInterna" mod="local" eticheta="Unitatea internă" />
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
              onClick={() => { setIndiceEditat(null); setInEditare(linieNoua()); }}
            >
              Adaugă linie
            </button>
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
              const rand = e.data as DviLinieWrite & { __indice: number };
              setIndiceEditat(rand.__indice);
              setInEditare(linii[rand.__indice]);
            }}
          >
            <Column dataField="TipMaterialCod" caption="Cod tip" />
            <Column dataField="TipMaterialDenumire" caption={capLinie('TipMaterialId')} />
            <Column dataField="TipTvaCod" caption="Cod TVA" />
            <Column dataField="TipTvaDenumire" caption={capLinie('TipTvaId')} />
            <Column dataField="Valoare" caption={capLinie('Valoare')} dataType="number" format="#,##0.00" alignment="right" />
            <Column dataField="ValoareTva" caption={capLinie('ValoareTva')} dataType="number" format="#,##0.00" alignment="right" />
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
            <DviEditorLinie
              key={indiceEditat ?? 'linie-noua'}
              linie={inEditare}
              eticheteInitiale={indiceEditat == null ? eticheteLinieNoua : eticheteLinii[indiceEditat]}
              partenerId={agregat.PredatorId}
              data={agregat.Data}
              readOnly={!poateEdita}
              onSalveaza={salveazaLinie}
              onRenunta={() => { setInEditare(null); setIndiceEditat(null); }}
            />
          )}

          <PanouFacturi
            facturiIds={facturiIds}
            citite={doc?.Facturi}
            culesi={facturiCulese}
            dviId={nou ? null : id}
            dataDocument={agregat.Data}
            readOnly={!poateEdita}
            onAdauga={adaugaFacturi}
            onScoate={scoateFactura}
          />
        </>
      }
    />
  );
}

// Etichetele unei linii: cele CULESE în sesiunea asta bat ReadDto-ul, care se
// caută după `Id`. Valorile rămân ale agregatului local (sunt culese, nu
// calculate de server pe draft) — taxa născută din cotă apare după operare.
function etichete(citite: DviLinieRead[] | null | undefined, linie: DviLinieWrite, culese?: EticheteCulese) {
  const g = citite?.find((c) => c.Id === linie.Id);
  return {
    TipMaterialCod: culese?.TipMaterialCod ?? g?.TipMaterialCod ?? '',
    TipMaterialDenumire: culese?.TipMaterialDenumire ?? g?.TipMaterialDenumire ?? '',
    TipTvaCod: culese?.TipTvaCod ?? g?.TipTvaCod ?? '',
    TipTvaDenumire: culese?.TipTvaDenumire ?? g?.TipTvaDenumire ?? '',
  };
}

// `Baza` și `Tva` sunt cifrele SERVERULUI (42c). `Total (brut)` nu există pe DVI:
// baza plus taxa n-ar fi nici valoarea vămuită, nici ce se datorează (DVI-D4).
function Sumar(props: { stare?: string | null; baza?: number; tva?: number; modificat: boolean }) {
  const necunoscut = props.modificat || props.baza == null;
  return (
    <div className="sumar">
      <span className="sumar__stare">{labelEnum('StareDocument', props.stare) || 'nesalvat'}</span>
      <span className="sumar__total">
        {capAntet('Baza')}: {necunoscut
          ? <em title="Cifrele le calculează serverul la salvare.">— recalculat la salvare</em>
          : props.baza?.toFixed(2)}
      </span>
      <span className="sumar__total">
        {capAntet('Taxa')}: {necunoscut || props.tva == null ? '—' : props.tva.toFixed(2)}
      </span>
    </div>
  );
}
