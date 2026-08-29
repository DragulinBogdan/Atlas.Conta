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
import { PanouStingeri, type GrupCandidati } from '../../nucleu/PanouStingeri';
import { campMeta, labelEnum } from '../../nucleu/campMeta';
import { eroriDin } from '../../nucleu/http';
import {
  antetGol, linieGoala, ntc, spreWrite,
  SCHEMA_ANTET, SCHEMA_LINIE, TIP_ANTET, TIP_LINIE,
  type NtcLinieRead, type NtcLinieWrite, type NtcWrite,
} from './api';
import { NtcEditorLinie, type EticheteCulese } from './NtcEditorLinie';

// Felia verticală NTC, ecranul de document — șablonul consolidat (43c):
//   (1) server-read  → `useQuery` pe ReadDto (affordances, Total, etichete);
//   (2) formular     → `agregat`: WriteDto ÎNTREG, o singură valoare locală;
//   (3) efemeride    → linia în editare, erorile afișate.
// URL-ul e starea globală: `/ntc/nou` și `/ntc/:id`.
//
// Nota contabilă e cea mai puternică ușă de scriere din sistem (riscul 1 al
// contractului F19): o linie cu postare COMPLETĂ postează în ABSENȚA oricărei
// reguli de contare. Ecranul nu adaugă niciun gard propriu peste asta — n-ar
// avea ce, fiindcă nu există regulă de „corespondență permisă": nota contabilă
// manuală ASTA e (48b/46b). Ce stă între operator și note arbitrare e
// autorizarea + gardianul de editare + dimensiunile obligatorii ale contului,
// toate pe server.
//
// Al doilea lucru propriu tipului: rolul de STINGĂTOR (compensarea, 48b).
// Panoul de la subsol NU e cel al trezoreriei: nota poartă contrapartidele pe
// LINII, iar plafonul are și SENS (F19-D16) — vezi `grupuri` mai jos.

const CAMPURI_ANTET: (keyof NtcWrite & string)[] = ['Data', 'PredatorId', 'PrimitorId'];
const capLinie = (m: string) => campMeta(TIP_LINIE, m, SCHEMA_LINIE).caption;

export function NtcDetaliu() {
  const { id } = useParams();
  const nou = id === 'nou' || id === undefined;
  const navigheaza = useNavigate();
  const cache = useQueryClient();

  const [agregat, setAgregat] = useState<NtcWrite>(antetGol);
  const [modificat, setModificat] = useState(false);
  const [aratErori, setAratErori] = useState(false);
  const [erori, setErori] = useState<string[]>([]);
  const [mesaje, setMesaje] = useState<string[]>([]);
  // Ștergerea draftului cere confirmare INLINE (F20-D4), nu `window.confirm`:
  // dialogul nativ blochează renderer-ul și nu poate spune CE se pierde.
  // Starea e a feliei; locul de randare îl dă `DocumentShell`.
  const [deSters, setDeSters] = useState(false);
  const [inEditare, setInEditare] = useState<NtcLinieWrite | null>(null);
  const [indiceEditat, setIndiceEditat] = useState<number | null>(null);
  // Etichetele CULESE în sesiunea asta, per POZIȚIE de linie (paralel cu
  // `agregat.Linii`): grila le folosește cât timp linia n-are încă etichete
  // server-owned în ReadDto (documentul/linia nesalvată).
  const [eticheteLinii, setEticheteLinii] = useState<(EticheteCulese | undefined)[]>([]);

  const citit = useQuery({
    queryKey: ['ntc', id],
    queryFn: () => ntc.citeste(id!),
    enabled: !nou,
  });

  const doc = citit.data;
  const operat = doc?.Stare === 'Operat';

  // Candidații de compensare: UN apel, doar pe documentul OPERAT (pe draft
  // serverul întoarce oricum `PoateStinge = false` și lista goală — nu-l
  // deranjăm degeaba).
  const candidati = useQuery({
    queryKey: ['ntc-candidati', id],
    queryFn: () => ntc.candidati(id!),
    enabled: !nou && operat,
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

  const poateEdita = nou || (doc?.PoateEdita ?? false);
  const linii = agregat.Linii ?? [];

  // Traducerea DTO → forma panoului: doar redenumiri și label-uri de enum.
  // Niciun număr nu se compune aici — capacitatea, asignatul și disponibilul
  // vin din `CapacitateStingere`/`AsignatFataDe`, adică din exact funcțiile pe
  // care le cheamă `ImperechereService.ValideazaCreare` (42c).
  const grupuri = useMemo<GrupCandidati[]>(
    () => (candidati.data?.Contrapartide ?? []).map((c) => ({
      contrapartidaId: String(c.RepartitorId),
      contrapartidaEticheta: [c.RepartitorCod, c.RepartitorDenumire].filter(Boolean).join(' — ')
        || '(repartitor fără nume)',
      sensEticheta: labelEnum('SensStingere', c.Sens) || (c.Sens ?? ''),
      capacitate: c.Capacitate ?? 0,
      asignat: c.Asignat ?? 0,
      disponibil: c.Disponibil ?? 0,
      candidati: c.Candidati ?? [],
      maiSunt: c.MaiSunt ?? false,
    })),
    [candidati.data]);

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
    mutationFn: async () => (nou ? ntc.creeaza(agregat) : ntc.actualizeaza(id!, agregat)),
    onSuccess: (salvat) => {
      setErori([]);
      setMesaje(['Salvat.']);
      setModificat(false);
      cache.invalidateQueries({ queryKey: ['ntc'] });
      if (nou) navigheaza(`/ntc/${salvat.Id}`, { replace: true });
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
      await cache.invalidateQueries({ queryKey: ['ntc'] });
      await cache.invalidateQueries({ queryKey: ['ntc-candidati'] });
      // Nota postează pe conturi arbitrare: orice raport de registre afișat în
      // altă parte nu mai e valabil.
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
      await ntc.sterge(id!);
      await cache.invalidateQueries({ queryKey: ['ntc'] });
      navigheaza('/ntc');
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
      ruleaza: () => void raporteaza(ntc.valideaza(id!).then((e) => {
        if (e.length > 0) { setErori(e); return []; }
        return ['Documentul trece toți gardienii motorului.'];
      })),
    },
    { eticheta: 'Operează', disponibila: (doc?.PoateOpera ?? false) && !modificat, ruleaza: () => void comanda(() => ntc.opereaza(id!)) },
    { eticheta: 'Anulează operarea', disponibila: doc?.PoateAnula ?? false, ruleaza: () => void comanda(() => ntc.anuleaza(id!)) },
    {
      eticheta: 'Stornează',
      disponibila: doc?.PoateStorna ?? false,
      cereData: { eticheta: 'Data stornării' },
      ruleaza: (data) => { if (data) void comanda(() => ntc.storneaza(id!, data)); },
    },
    { eticheta: 'Șterge', disponibila: !nou && poateEdita, ruleaza: () => setDeSters(true) },
    { eticheta: 'Înapoi la listă', disponibila: true, ruleaza: () => navigheaza('/ntc') },
  ];

  function schimbaAntet(v: NtcWrite) {
    setAgregat(v);
    setModificat(true);
  }

  function salveazaLinie(linie: NtcLinieWrite, culese: EticheteCulese) {
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
      titlu={nou ? 'Notă contabilă — nouă' : `Notă contabilă ${doc?.Numar ?? ''}`}
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
              {/* `Numar` e AFIȘARE: NTC are politică de numerotare („NTC-") în
                  ambele profiluri, deci seria se asignează la operare și nici nu
                  există în WriteDto (F19-D6). Pe un draft e gol — asta e
                  adevărul, nu o lipsă. */}
              <Static membru="Numar" valoare={doc?.Numar} />
              <CampData<NtcWrite> camp="Data" />
              {/* AMBELE laturi sunt repartitori INTERNI: contrapartidele reale
                  (partenerii compensării) stau pe LINII, pe cele două câmpuri de
                  repartitor ale postării. Lookup-urile sunt NEFILTRATE (F6-D8):
                  refuzul laturii `Partener` e al operării, nu al listei. */}
              <Lookup<NtcWrite>
                camp="PredatorId"
                entitate="UnitateInterna"
                mod="local"
                eticheta="Unitatea (de la)"
              />
              <div>
                <Lookup<NtcWrite>
                  camp="PrimitorId"
                  entitate="UnitateInterna"
                  mod="local"
                  eticheta="Unitatea (către)"
                />
                <p className="indiciu">
                  Laturile notei sunt interne (de regulă aceeași unitate) — partenerii compensării se culeg
                  pe linii, ca repartitori ai postării. Operarea refuză o latură de tip partener.
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
              (nimic inventat în TS). */}
          <DataGrid
            dataSource={linii.map((l, i) => ({ ...l, __indice: i, ...etichete(doc?.Linii, l, eticheteLinii[i]) }))}
            keyExpr="__indice"
            showBorders
            columnAutoWidth
            onRowClick={(e) => {
              if (!poateEdita) return;
              const rand = e.data as NtcLinieWrite & { __indice: number };
              setIndiceEditat(rand.__indice);
              setInEditare(linii[rand.__indice]);
            }}
          >
            <Column dataField="Descriere" caption={capLinie('Descriere')} />
            <Column dataField="ContDebitSimbol" caption="Cont debitor" width={110} />
            <Column dataField="RepartitorDebitDenumire" caption="Repartitor debit" />
            <Column dataField="ContCreditSimbol" caption="Cont creditor" width={110} />
            <Column dataField="RepartitorCreditDenumire" caption="Repartitor credit" />
            <Column dataField="TipMaterialCod" caption="Cod tip" width={90} />
            {/* CULEASĂ, nu calculată (F19-D8) — și poate fi negativă. */}
            <Column dataField="Valoare" caption={capLinie('Valoare')} dataType="number" format="#,##0.00" alignment="right" />
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
            <NtcEditorLinie
              key={indiceEditat ?? 'linie-noua'}
              linie={inEditare}
              readOnly={!poateEdita}
              onSalveaza={salveazaLinie}
              onRenunta={() => { setInEditare(null); setIndiceEditat(null); }}
            />
          )}
        </>
      }
      // COMPENSAREA (48b): nota operată e STINGĂTOR, cu plafon per
      // (contrapartidă × SENS). Panoul rulează în modul GRUPAT — un rând per
      // jumătate, cu sensul vizibil și cu `ContrapartidaId` trimis explicit la
      // creare (F19-D16): fără el, un document care poartă două dintre
      // contrapartidele notei primește un refuz de ambiguitate corect, dar orb.
      subsol={!nou && doc?.Id && operat
        ? (
          <PanouStingeri
            documentId={doc.Id}
            rol="stinge"
            grupuri={grupuri}
            mesajFaraGrupuri={
              candidati.isFetching
                ? 'Se încarcă plafoanele de compensare…'
                : 'Nota n-are niciun repartitor pe linii — fără contrapartidă nu există plafon de compensare.'
            }
            onSchimbare={() => {
              void cache.invalidateQueries({ queryKey: ['ntc'] });
              void cache.invalidateQueries({ queryKey: ['ntc-candidati'] });
            }}
          />
        )
        : undefined}
    />
  );
}

// Etichetele unei linii: cele CULESE în sesiunea asta (alegerea proaspătă a
// operatorului) bat ReadDto-ul, care se caută după `Id`; linia nouă fără
// culegere rămâne goală — nu le inventăm în TS.
function etichete(citite: NtcLinieRead[] | null | undefined, linie: NtcLinieWrite, culese?: EticheteCulese) {
  const g = citite?.find((c) => c.Id === linie.Id);
  return {
    TipMaterialCod: culese?.TipMaterialCod ?? g?.TipMaterialCod ?? '',
    ContDebitSimbol: culese?.ContDebitSimbol ?? g?.ContDebitSimbol ?? '',
    ContCreditSimbol: culese?.ContCreditSimbol ?? g?.ContCreditSimbol ?? '',
    RepartitorDebitDenumire: culese?.RepartitorDebitDenumire ?? g?.RepartitorDebitDenumire ?? '',
    RepartitorCreditDenumire: culese?.RepartitorCreditDenumire ?? g?.RepartitorCreditDenumire ?? '',
  };
}

// Câmp de AFIȘARE în interiorul formularului: aceeași ramă ca la culegere
// (etichetă din metadata, slot de control), cu text în locul editorului.
function Static(props: { membru: string; valoare: ReactNode }) {
  const meta = { ...campMeta(TIP_ANTET, props.membru, SCHEMA_ANTET), obligatoriu: false };
  return (
    <CampShell meta={meta}>
      <div className="valoare-statica">{props.valoare == null || props.valoare === '' ? '—' : props.valoare}</div>
    </CampShell>
  );
}

// `Total` = Σ liniilor, redusă de SERVER. Pe notă suma nu e nici creanță, nici
// datorie — e cât s-a postat, și poate fi negativă (note storno). De asta nota
// lipsește deliberat din proiecția de rest (F19-D10): n-are semantică de „rest".
function Sumar(props: { stare?: string | null; total?: number; modificat: boolean }) {
  return (
    <div className="sumar">
      <span className="sumar__stare">{labelEnum('StareDocument', props.stare) || 'nesalvat'}</span>
      <span className="sumar__total">
        Total postat: {props.modificat || props.total == null
          ? <em title="Totalul îl calculează serverul la salvare.">— recalculat la salvare</em>
          : props.total.toFixed(2)}
      </span>
    </div>
  );
}
