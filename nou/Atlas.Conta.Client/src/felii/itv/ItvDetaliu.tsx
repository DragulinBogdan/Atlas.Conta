import { useState, type ReactNode } from 'react';
import { useNavigate, useParams } from 'react-router';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { Column, DataGrid } from 'devextreme-react/data-grid';
import { DocumentShell, type Comanda } from '../../nucleu/DocumentShell';
import { ConfirmareInline } from '../../nucleu/ConfirmareInline';
import { CampShell } from '../../nucleu/CampShell';
import { PanouErori } from '../../nucleu/PanouErori';
import { campMeta, labelEnum } from '../../nucleu/campMeta';
import { eroriDin } from '../../nucleu/http';
import { ziLocala } from '../../nucleu/zi';
import { itv, SCHEMA_ANTET, TIP_ANTET, type ItvRead } from './api';

// Ecranul documentului de închidere (F21-D8). E singurul ecran de document FĂRĂ
// formular: nu există niciun câmp cules — antetul, liniile și cele trei valori
// sunt toate ale serviciului care a generat draftul. Operatorul are aici doar
// COMENZI: verifică, operează, anulează, stornează, regenerează, șterge.
//
// De aceea `DocumentShell` primește un `antet` de AFIȘARE (aceeași ramă de câmp
// ca la culegere, cu text în locul editorului) și nu un `<Formular>`: nu există
// agregat local de reconciliat, deci nici starea a doua din 43c.
//
// Singurul verdict propriu ecranului e panoul `Stale`: draftul a fost calculat
// pe soldurile de la generare, iar între timp au mai intrat documente în lună.
// Nu-l deducem noi — `ItvReadDto.Stale` folosește EXACT criteriul gardianului
// anti-stale din `InchidereTva.ValideazaOperare`, deci ecranul și motorul spun
// același lucru (dacă ar diverge, ecranul ar minți).

const LUNI = [
  'ianuarie', 'februarie', 'martie', 'aprilie', 'mai', 'iunie',
  'iulie', 'august', 'septembrie', 'octombrie', 'noiembrie', 'decembrie',
];

type Cerere = 'sterge' | 'regenereaza';

export function ItvDetaliu() {
  const { id } = useParams();
  const navigheaza = useNavigate();
  const cache = useQueryClient();

  const [erori, setErori] = useState<string[]>([]);
  const [mesaje, setMesaje] = useState<string[]>([]);
  // O SINGURĂ cerere de confirmare în așteptare — slotul shell-ului e unul, iar
  // cele două acțiuni ireversibile (ștergere, regenerare) se exclud.
  const [cerere, setCerere] = useState<Cerere | null>(null);

  const citit = useQuery({
    queryKey: ['itv', id],
    queryFn: () => itv.citeste(id!),
    enabled: id != null,
  });
  const doc = citit.data;

  async function comanda(rulare: () => Promise<{ Mesaje?: string[] | null; StareNoua?: string | null }>) {
    setErori([]);
    setMesaje([]);
    try {
      const rezultat = await rulare();
      setMesaje([`Stare nouă: ${labelEnum('StareDocument', rezultat.StareNoua)}`, ...(rezultat.Mesaje ?? [])]);
      // Închiderea scrie în registrul contabil: lista, previzualizările lunilor
      // (aceeași cheie `['itv']`) și orice raport de stingeri afișat aiurea nu
      // mai sunt valabile.
      await cache.invalidateQueries({ queryKey: ['itv'] });
      await cache.invalidateQueries({ queryKey: ['stingeri'] });
    }
    catch (e) {
      setErori(eroriDin(e));
    }
  }

  function raporteaza(promisiune: Promise<string[]>) {
    setErori([]);
    setMesaje([]);
    return promisiune
      .then((m) => setMesaje(m))
      .catch((e) => setErori(eroriDin(e)));
  }

  async function regenereaza() {
    setCerere(null);
    setErori([]);
    setMesaje([]);
    try {
      const r = await itv.regenereaza(id!);
      await cache.invalidateQueries({ queryKey: ['itv'] });
      // Draftul vechi e ȘTERS, iar cel nou are alt id: rămânerea pe URL-ul
      // vechi ar arăta un 404. 200 fără document e legitim (luna nu mai are ce
      // închide) și iese ca raport, pe listă.
      if (r.DocumentId) {
        navigheaza(`/itv/${r.DocumentId}`, { replace: true });
        return;
      }
      navigheaza('/itv');
    }
    catch (e) {
      setErori(eroriDin(e));
      // Și pe refuz se reîncarcă (review 79 F2): serverul garantează acum că un
      // refuz lasă draftul intact, dar ecranul nu presupune — recitește ca să
      // arate starea REALĂ a documentului, nu una ținută minte.
      await cache.invalidateQueries({ queryKey: ['itv'] });
    }
  }

  async function stergeDocumentul() {
    setCerere(null);
    setErori([]);
    setMesaje([]);
    try {
      await itv.sterge(id!);
      await cache.invalidateQueries({ queryKey: ['itv'] });
      navigheaza('/itv');
    }
    catch (e) {
      setErori(eroriDin(e));
    }
  }

  const comenzi: Comanda[] = [
    {
      eticheta: 'Verifică',
      disponibila: doc != null,
      ruleaza: () => void raporteaza(itv.valideaza(id!).then((e) => {
        if (e.length > 0) { setErori(e); return []; }
        return ['Închiderea trece toți gardienii motorului.'];
      })),
    },
    {
      eticheta: 'Operează',
      disponibila: doc?.PoateOpera ?? false,
      primara: true,
      ruleaza: () => void comanda(() => itv.opereaza(id!)),
    },
    {
      eticheta: 'Anulează operarea',
      disponibila: doc?.PoateAnula ?? false,
      ruleaza: () => void comanda(() => itv.anuleaza(id!)),
    },
    {
      eticheta: 'Stornează',
      disponibila: doc?.PoateStorna ?? false,
      // Data IMPLICITĂ e chiar `Data` documentului, adică ultima zi a lunii
      // închise (F21-D7): stornarea acolo lasă soldurile 4426/4427 exact cum
      // erau și redeschide luna pentru o închidere nouă. Rămâne editabilă —
      // implicitul e un default, nu o regulă.
      cereData: { eticheta: 'Data stornării', implicit: doc?.Data },
      ruleaza: (data) => { if (data) void comanda(() => itv.storneaza(id!, data)); },
    },
    {
      eticheta: 'Regenerează',
      disponibila: doc?.PoateRegenera ?? false,
      ruleaza: () => setCerere('regenereaza'),
    },
    { eticheta: 'Șterge', disponibila: doc?.PoateSterge ?? false, ruleaza: () => setCerere('sterge') },
    { eticheta: 'Înapoi la listă', disponibila: true, ruleaza: () => navigheaza('/itv') },
  ];

  return (
    <DocumentShell
      titlu={`Închidere de TVA ${doc?.Numar ?? '(draft)'}`}
      sumar={<Sumar doc={doc} />}
      comenzi={comenzi}
      confirmare={cerere === 'sterge' ? (
        <ConfirmareInline
          intrebare="Ștergeți definitiv acest draft de închidere, cu tot cu liniile lui?"
          verb="Șterge închiderea"
          onConfirma={() => void stergeDocumentul()}
          onRenunta={() => setCerere(null)}
        />
      ) : cerere === 'regenereaza' ? (
        <ConfirmareInline
          intrebare="Draftul se șterge și se generează din nou, pe soldurile de acum. Închiderea va primi alt număr intern."
          verb="Regenerează"
          onConfirma={() => void regenereaza()}
          onRenunta={() => setCerere(null)}
        />
      ) : null}
      inchideConfirmarea={() => setCerere(null)}
      erori={erori}
      mesaje={mesaje}
      ocupat={citit.isFetching}
      antet={
        <>
          {/* Doar pe DRAFT: pe Operat/Stornat `Stale` e `null`, fiindcă
              întrebarea („ce s-ar genera acum?") nu mai are înțeles. */}
          <PanouErori
            erori={doc?.Stale === true
              ? ['Soldurile de TVA s-au schimbat de la generarea acestui draft — regenerați închiderea înainte de a o opera.']
              : []}
            titlu="Draft depășit"
            fel="atentie"
          />

          <div className="grila-campuri">
            <Static membru="Numar" valoare={doc?.Numar} />
            <Static membru="Data" valoare={ziLocala(doc?.Data)} />
            <Explicit eticheta="Luna închisă" valoare={etichetaLuna(doc)} />
            <Explicit eticheta="Unitatea internă" valoare={doc?.UnitateDenumire} />
            <Static membru="Stare" valoare={labelEnum('StareDocument', doc?.Stare)} />
            <Static membru="DataOperare" valoare={ziLocala(doc?.DataOperare)} />
          </div>

          <SolduriTva doc={doc} />
        </>
      }
      linii={
        <>
          <div className="linii__bara"><h3>Liniile închiderii</h3></div>
          {/* Grilă de CITIRE: liniile sunt ale serviciului, nu ale
              operatorului — nu există „adaugă linie" și nici editor. Conturile
              vin din `PoliticaInchidereTva`, nu din simboluri scrise în cod. */}
          <DataGrid
            dataSource={doc?.Linii ?? []}
            keyExpr="Id"
            showBorders
            columnAutoWidth
          >
            <Column dataField="Descriere" caption="Descriere" />
            <Column dataField="ContDebitSimbol" caption="Cont debitor" width={120} />
            <Column dataField="ContCreditSimbol" caption="Cont creditor" width={120} />
            <Column dataField="Valoare" caption="Valoare" dataType="number" format="#,##0.00" alignment="right" width={160} />
          </DataGrid>
        </>
      }
      subsol={
        <p className="indiciu">
          Închiderea nu se culege: liniile o produce motorul din soldurile 4426/4427 la data de mai sus,
          pe conturile din politica de închidere. „Regenerează” o reface pe soldurile de acum.
          <strong> Stornarea la chiar data închiderii</strong> (ultima zi a lunii) lasă soldurile
          neschimbate și redeschide luna pentru o închidere nouă — o dată de storno ulterioară mută
          reversarea în altă lună.
        </p>
      }
    />
  );
}

// Soldurile pe care motorul le vede ACUM la data documentului, plus cele trei
// valori ale închiderii. Toate șase vin din DTO — niciun `Math.min` aici (42c).
function SolduriTva({ doc }: { doc: ItvRead | undefined }) {
  if (!doc) return null;
  return (
    <div className="itv__sectiune">
      <h3>Soldurile de TVA la data închiderii</h3>
      <table className="tabel-mic">
        <tbody>
          {/* Fără simboluri de cont în etichete: conturile sunt ale POLITICII
              (29) și se văd pe liniile de mai sus, nu se afirmă din cod. */}
          <tr>
            <th>Sold TVA deductibilă</th>
            <td className="num">{bani(doc.Sold4426Curent)}</td>
            <th>Sold TVA colectată</th>
            <td className="num">{bani(doc.Sold4427Curent)}</td>
          </tr>
          <tr>
            <th>Transfer</th>
            <td className="num">{bani(doc.Transfer)}</td>
            <th>TVA de plată</th>
            <td className="num">{bani(doc.DePlata)}</td>
          </tr>
          <tr>
            <th>TVA de recuperat</th>
            <td className="num">{bani(doc.DeRecuperat)}</td>
            <th />
            <td />
          </tr>
        </tbody>
      </table>
      <p className="indiciu">
        {doc.Stare === 'Draft'
          ? <>Pe un draft, primele două cifre sunt soldurile de <em>acum</em>; celelalte trei sunt cele cu care
              s-a generat. Diferența dintre ele e exact ce raportează avertismentul de draft depășit.</>
          : <>Închiderea e în registru: soldurile la data ei sunt cele de <em>după</em> închidere (zero, dacă
              nimic n-a intrat între timp în lună), iar cele trei cifre sunt liniile documentului.</>}
      </p>
    </div>
  );
}

// Câmp de AFIȘARE cu caption din metadata (aceeași ramă ca la culegere).
function Static(props: { membru: string; valoare: ReactNode }) {
  const meta = { ...campMeta(TIP_ANTET, props.membru, SCHEMA_ANTET), obligatoriu: false };
  return <Rama meta={meta} valoare={props.valoare} />;
}

// Câmp de afișare pentru ce NU e membru de entitate (`An`/`Luna` sunt derivate
// din `Data` în DTO, unitatea e numită de felie): eticheta se scrie explicit,
// nu se cere de la `campMeta` — o cerere acolo ar avertiza în consolă și ar
// cădea pe numele câmpului.
function Explicit(props: { eticheta: string; valoare: ReactNode }) {
  return <Rama meta={{ caption: props.eticheta, obligatoriu: false }} valoare={props.valoare} />;
}

function Rama(props: { meta: { caption: string; obligatoriu: boolean }; valoare: ReactNode }) {
  return (
    <CampShell meta={props.meta}>
      <div className="valoare-statica">
        {props.valoare == null || props.valoare === '' ? '—' : props.valoare}
      </div>
    </CampShell>
  );
}

function Sumar({ doc }: { doc: ItvRead | undefined }) {
  return (
    <div className="sumar">
      <span className="sumar__stare">{labelEnum('StareDocument', doc?.Stare) || '—'}</span>
      <span className="sumar__total">
        Total postat: {doc?.Total == null ? '—' : bani(doc.Total)}
      </span>
    </div>
  );
}

// Luna și anul sunt derivate de DTO din `Data` (nu sunt membri de entitate).
// Codegen-ul le dă opționale — Module n-are context nullable activ, deci
// `required[]` lipsește din schemă și TOATE câmpurile ies `?`. Aici absența se
// tratează, nu se afirmă cu `!`: un antet care spune „—" e adevărat.
function etichetaLuna(doc: ItvRead | undefined): string | null {
  if (!doc || doc.Luna == null || doc.An == null) return null;
  return `${LUNI[doc.Luna - 1] ?? doc.Luna} ${doc.An}`;
}

function bani(v: number | null | undefined): string {
  if (v == null) return '—';
  return v.toLocaleString('ro-RO', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

