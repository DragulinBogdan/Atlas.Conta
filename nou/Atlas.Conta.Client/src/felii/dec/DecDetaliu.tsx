import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { useNavigate, useParams } from 'react-router';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Column, DataGrid } from 'devextreme-react/data-grid';
import { DocumentShell, type Comanda } from '../../nucleu/DocumentShell';
import { ConfirmareInline } from '../../nucleu/ConfirmareInline';
import { CampShell } from '../../nucleu/CampShell';
import { Formular, eroriStructurale } from '../../nucleu/formular';
import { CampData, CampText } from '../../nucleu/campuri';
import { Lookup } from '../../nucleu/Lookup';
import { PanouErori } from '../../nucleu/PanouErori';
import { PanouStingeri } from '../../nucleu/PanouStingeri';
import { campMeta, labelEnum } from '../../nucleu/campMeta';
import { eroriDin } from '../../nucleu/http';
import { useSonda } from '../../nucleu/odata';
import {
  antetGol, dec, linieGoala, spreWrite, RUTA,
  SCHEMA_ANTET, SCHEMA_LINIE, TIP_ANTET, TIP_LINIE,
  type DecLinieRead, type DecLinieWrite, type DecWrite,
} from './api';
import { DecEditorLinie, type EticheteCulese } from './DecEditorLinie';

// Felia verticală DEC, ecranul de document — șablonul consolidat (43c):
//   (1) server-read  → `useQuery` pe ReadDto (affordances, Total, etichete);
//   (2) formular     → `agregat`: WriteDto ÎNTREG, o singură valoare locală;
//   (3) efemeride    → linia în editare, erorile afișate.
// URL-ul e starea globală: `/dec/nou` și `/dec/:id`.
//
// Ce exersează felia peste FCT/LDI: **postarea explicită pe linie** (32a, în
// editorul de linie) și **stingerea pe lanțul avans↔decont↔regularizare**
// (31d/32d) — decontul stă pe rolul de STINS, ca factura.

const CAMPURI_ANTET: (keyof DecWrite & string)[] = ['Data', 'PredatorId', 'PrimitorId', 'NumarPV', 'DataPV'];
const capLinie = (m: string) => campMeta(TIP_LINIE, m, SCHEMA_LINIE).caption;

export function DecDetaliu() {
  const { id } = useParams();
  const nou = id === 'nou' || id === undefined;
  const navigheaza = useNavigate();
  const cache = useQueryClient();

  const [agregat, setAgregat] = useState<DecWrite>(antetGol);
  const [modificat, setModificat] = useState(false);
  const [aratErori, setAratErori] = useState(false);
  const [erori, setErori] = useState<string[]>([]);
  const [mesaje, setMesaje] = useState<string[]>([]);
  // Ștergerea draftului cere confirmare INLINE (F20-D4), nu `window.confirm`:
  // dialogul nativ blochează renderer-ul și nu poate spune CE se pierde.
  // Starea e a feliei; locul de randare îl dă `DocumentShell`.
  const [deSters, setDeSters] = useState(false);
  const [inEditare, setInEditare] = useState<DecLinieWrite | null>(null);
  const [indiceEditat, setIndiceEditat] = useState<number | null>(null);
  // Etichetele CULESE în sesiunea asta, per POZIȚIE de linie (paralel cu
  // `agregat.Linii`): grila le folosește cât timp linia n-are încă etichete
  // server-owned în ReadDto (documentul/linia nesalvată).
  const [eticheteLinii, setEticheteLinii] = useState<(EticheteCulese | undefined)[]>([]);

  const citit = useQuery({
    queryKey: ['dec', id],
    queryFn: () => dec.citeste(id!),
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

  // Primitorul unui document EXISTENT: validarea operării acceptă și `Gestiune`,
  // nu doar `UnitateInterna`, iar lookup-ul pe setul `UnitateInterna` ar minți pe
  // valorile din afara lui (afișare goală, listă care nu conține valoarea).
  // Sonda de existență decide (61c): valoarea E în set ⇒ lookup normal (editabil
  // pe draft); altfel — sau cât timp nu știm — afișare statică din ReadDto.
  // Eșecul sondei cade deci pe varianta care nu minte.
  const primitorInSet = useSonda('UnitateInterna', doc?.PrimitorId, !nou);
  const primitorEditabil = nou || doc?.PrimitorId == null || primitorInSet === true;

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
    mutationFn: async () => (nou ? dec.creeaza(agregat) : dec.actualizeaza(id!, agregat)),
    onSuccess: (salvat) => {
      setErori([]);
      setMesaje(['Salvat.']);
      setModificat(false);
      cache.invalidateQueries({ queryKey: ['dec'] });
      if (nou) navigheaza(`${RUTA}/${salvat.Id}`, { replace: true });
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
      await cache.invalidateQueries({ queryKey: ['dec'] });
      // Anularea/stornarea desface stingerile pe care le poate avea decontul —
      // panoul lui (și al plăților care-l sting, dacă sunt deschise în altă
      // filă) nu mai e de încredere.
      await cache.invalidateQueries({ queryKey: ['stingeri'] });
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
      await dec.sterge(id!);
      await cache.invalidateQueries({ queryKey: ['dec'] });
      navigheaza(RUTA);
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
      ruleaza: () => void raporteaza(dec.valideaza(id!).then((e) => {
        if (e.length > 0) { setErori(e); return []; }
        return ['Documentul trece toți gardienii motorului.'];
      })),
    },
    { eticheta: 'Operează', disponibila: (doc?.PoateOpera ?? false) && !modificat, ruleaza: () => void comanda(() => dec.opereaza(id!)) },
    // `PoateAnula`/`PoateStorna` sunt ONESTE (F3-D2/57d): țin cont de stingeri —
    // un decont imperecheat nu se anulează până nu se șterge legătura din panou.
    { eticheta: 'Anulează operarea', disponibila: doc?.PoateAnula ?? false, ruleaza: () => void comanda(() => dec.anuleaza(id!)) },
    {
      eticheta: 'Stornează',
      disponibila: doc?.PoateStorna ?? false,
      cereData: { eticheta: 'Data stornării' },
      ruleaza: (data) => { if (data) void comanda(() => dec.storneaza(id!, data)); },
    },
    { eticheta: 'Șterge', disponibila: !nou && poateEdita, ruleaza: () => setDeSters(true) },
    { eticheta: 'Înapoi la listă', disponibila: true, ruleaza: () => navigheaza(RUTA) },
  ];

  function schimbaAntet(v: DecWrite) {
    setAgregat(v);
    setModificat(true);
  }

  function salveazaLinie(linie: DecLinieWrite, culese: EticheteCulese) {
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
      citire={citit}
      titlu={nou ? 'Decont — nou' : `Decont ${doc?.Numar ?? ''}`}
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
              {/* `Numar` e AFIȘARE: DEC are politică de numerotare („DEC-"),
                  deci seria se asignează la operare și nici nu există în WriteDto
                  (F8-D3). Pe un draft e gol — asta e adevărul, nu o lipsă. */}
              <Static membru="Numar" valoare={doc?.Numar} />
              <CampData<DecWrite> camp="Data" />
              {/* TITULARUL care justifică avansul: un `Angajat` (invariant al
                  operării). Caption-ul bazei („Predator (de la)") e corect, dar
                  prea abstract pentru ecranul de decont — felia îl numește în
                  vocabularul ei (escapa `eticheta`). */}
              <Lookup<DecWrite>
                camp="PredatorId"
                entitate="Angajat"
                mod="local"
                eticheta="Titular"
                cauta={['Cautare', 'Marca']}
              />
              <div>
                {/* Unitatea internă care primește justificarea. Pe documentele
                    existente, sonda decide între lookup și afișare statică
                    (61c): validarea acceptă și `Gestiune`, care NU e ofertată
                    aici — un lookup pe `UnitateInterna` ar minți pe ea. */}
                {primitorEditabil
                  ? (
                    <Lookup<DecWrite>
                      camp="PrimitorId"
                      entitate="UnitateInterna"
                      mod="local"
                      eticheta="Unitatea care primește justificarea"
                    />
                  )
                  : (
                    <Static
                      membru="PrimitorId"
                      eticheta="Unitatea care primește justificarea"
                      valoare={doc?.PrimitorDenumire}
                    />
                  )}
                <p className="indiciu">
                  Lista oferă unitățile interne; o gestiune rămâne validă server-side, dar nu se
                  ofertează aici.
                </p>
              </div>
              {/* Procesul-verbal al decontului (`IDocumentCuPV` — testul bazei 22e). */}
              <CampText<DecWrite> camp="NumarPV" />
              <CampData<DecWrite> camp="DataPV" />
              <Static membru="Stare" valoare={labelEnum('StareDocument', doc?.Stare)} />
              <Static membru="DataOperare" valoare={doc?.DataOperare?.slice(0, 10)} />
            </div>
          </Formular>
        </>
      }
      subsol={!nou && doc?.Id && doc.Stare === 'Operat'
        ? (
          // Decontul stă pe rolul de STINS: avansul plătit îl stinge, iar
          // regularizarea închide lanțul (31d/32d). Rolul e identitate declarată
          // de felie, nu deducție în panou.
          <PanouStingeri
            documentId={doc.Id}
            // Contrapartida DEC în proiecția de rest e TITULARUL (predatorul) —
            // aceeași latură pe care o normalizează `DocumenteCuRest`.
            contrapartidaId={doc.PredatorId}
            rol="este-stins"
            // Stingătorii posibili ai unui decont: trezoreria (avansul plătit,
            // regularizarea) ȘI nota contabilă — `NotaContabila` e stingător
            // legitim din 49a (compensarea), cu capacitățile date de repartitorii
            // expliciți ai liniilor ei. Lista e afordanță, nu regulă: autoritatea
            // rămâne `ImperechereService.ValideazaCreare`.
            //
            // Azi filtrul e INERT pe 'NTC': proiecția de rest (`DocumenteCuRest`,
            // 57c) are cinci ramuri CONCRETE — FCT/FCL/PLT/INC/DEC —, deci o notă
            // nu apare printre candidați indiferent de filtru. Rămâne declarat ca
            // să nu fie filtrul ĂSTA cel care exclude, când ramura va exista.
            tipuriCandidate={['PLT', 'INC', 'NTC']}
            onSchimbare={() => void cache.invalidateQueries({ queryKey: ['dec'] })}
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
              serverul, nu editează. Etichetele vin din ReadDto (server-owned)
              sau, pe liniile încă nesalvate, din ce a CULES editorul la selecție
              (nimic inventat în TS); valorile rămân exclusiv ale serverului —
              apar după Salvează. */}
          <DataGrid
            dataSource={linii.map((l, i) => ({ ...l, __indice: i, ...etichete(doc?.Linii, l, eticheteLinii[i]) }))}
            keyExpr="__indice"
            showBorders
            columnAutoWidth
            onRowClick={(e) => {
              if (!poateEdita) return;
              const rand = e.data as DecLinieWrite & { __indice: number };
              setIndiceEditat(rand.__indice);
              setInEditare(linii[rand.__indice]);
            }}
          >
            <Column dataField="TipMaterialCod" caption="Cod tip" />
            <Column dataField="TipMaterialDenumire" caption={capLinie('TipMaterialId')} />
            <Column dataField="Descriere" caption={capLinie('Descriere')} />
            <Column dataField="Cantitate" caption={capLinie('Cantitate')} dataType="number" format="#,##0.###" alignment="right" />
            <Column dataField="PretUnitar" caption={capLinie('PretUnitar')} dataType="number" format="#,##0.######" alignment="right" />
            {/* Valorile sunt ale SERVERULUI (43b): apar după Salvează, nu se
                calculează în TS nici măcar ca previzualizare. */}
            <Column dataField="Valoare" caption={`${capLinie('Valoare')} (server)`} dataType="number" format="#,##0.00" alignment="right" />
            <Column dataField="ValoareTva" caption={`${capLinie('ValoareTva')} (server)`} dataType="number" format="#,##0.00" alignment="right" />
            <Column dataField="TipTvaCod" caption={capLinie('TipTvaId')} />
            {/* Postarea explicită, coloană cu coloană: operatorul vede dintr-o
                privire care linii ocolesc regula de contare (32a). */}
            <Column dataField="ContDebitSimbol" caption={capLinie('ContDebitId')} />
            <Column dataField="ContCreditSimbol" caption={capLinie('ContCreditId')} />
            <Column dataField="RepartitorDebitDenumire" caption={capLinie('RepartitorDebitId')} />
            <Column dataField="RepartitorCreditDenumire" caption={capLinie('RepartitorCreditId')} />
            <Column dataField="CodEconomicCod" caption={capLinie('CodEconomicId')} />
            <Column dataField="AngajamentCod" caption={capLinie('AngajamentId')} />
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
            <DecEditorLinie
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

// Etichetele unei linii: cele CULESE în sesiunea asta (alegerea proaspătă a
// operatorului) bat ReadDto-ul, care se caută după `Id`; linia nouă fără
// culegere rămâne goală — nu le inventăm în TS. `Valoare`/`ValoareTva` n-au
// variantă culeasă: sunt ale serverului prin construcție (F8-D2).
function etichete(citite: DecLinieRead[] | null | undefined, linie: DecLinieWrite, culese?: EticheteCulese) {
  const g = citite?.find((c) => c.Id === linie.Id);
  return {
    TipMaterialCod: culese?.TipMaterialCod ?? g?.TipMaterialCod ?? '',
    TipMaterialDenumire: culese?.TipMaterialDenumire ?? g?.TipMaterialDenumire ?? '',
    TipTvaCod: culese?.TipTvaCod ?? g?.TipTvaCod ?? '',
    ContDebitSimbol: culese?.ContDebitSimbol ?? g?.ContDebitSimbol ?? '',
    ContCreditSimbol: culese?.ContCreditSimbol ?? g?.ContCreditSimbol ?? '',
    RepartitorDebitDenumire: culese?.RepartitorDebitDenumire ?? g?.RepartitorDebitDenumire ?? '',
    RepartitorCreditDenumire: culese?.RepartitorCreditDenumire ?? g?.RepartitorCreditDenumire ?? '',
    CodEconomicCod: culese?.CodEconomicCod ?? g?.CodEconomicCod ?? '',
    AngajamentCod: culese?.AngajamentCod ?? g?.AngajamentCod ?? '',
    Valoare: g?.Valoare,
    ValoareTva: g?.ValoareTva,
  };
}

// Câmp de AFIȘARE în interiorul formularului: aceeași ramă ca la culegere
// (etichetă din metadata, slot de control), cu text în locul editorului.
// Membrii server-owned nu există în WriteDto, deci `campMeta` îi dă corect
// NEobligatorii — nimeni nu cere operatorului un câmp pe care nu-l poate scrie.
function Static(props: { membru: string; eticheta?: string; valoare: ReactNode }) {
  const meta = campMeta(TIP_ANTET, props.membru, SCHEMA_ANTET);
  const efectiv = {
    ...meta,
    obligatoriu: false,
    ...(props.eticheta == null ? {} : { caption: props.eticheta }),
  };
  return (
    <CampShell meta={efectiv}>
      <div className="valoare-statica">{props.valoare == null || props.valoare === '' ? '—' : props.valoare}</div>
    </CampShell>
  );
}

// `Total` = valoarea din ReadDto (BRUT: Σ Valoare + ValoareTva), redusă de
// SERVER — aceeași cifră pe care o stinge imperecherea. La editare nesalvată nu
// există (baza s-a schimbat) — se marchează explicit, nu se recalculează în TS
// (43c).
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
