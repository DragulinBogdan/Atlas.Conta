import { useState, type ReactNode } from 'react';
import { useNavigate, useParams } from 'react-router';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { Column, DataGrid } from 'devextreme-react/data-grid';
import { DocumentShell, type Comanda } from '../../nucleu/DocumentShell';
import { CorectieDocument } from '../../nucleu/CorectieDocument';
import { ConfirmareInline } from '../../nucleu/ConfirmareInline';
import { CampShell } from '../../nucleu/CampShell';
import { PanouErori } from '../../nucleu/PanouErori';
import { campMeta, labelEnum } from '../../nucleu/campMeta';
import { bani } from '../../nucleu/format';
import { eroriDin } from '../../nucleu/http';
import { ziLocala } from '../../nucleu/zi';
import { amo, SCHEMA_ANTET, TIP_ANTET, type AmoRead } from './api';

// Oglinda lui `ItvDetaliu`: document generat, doar comenzi; `Stale` e criteriul gardianului (F26-D7).

const LUNI = [
  'ianuarie', 'februarie', 'martie', 'aprilie', 'mai', 'iunie',
  'iulie', 'august', 'septembrie', 'octombrie', 'noiembrie', 'decembrie',
];

type Cerere = 'sterge' | 'regenereaza';

export function AmoDetaliu() {
  const { id } = useParams();
  const navigheaza = useNavigate();
  const cache = useQueryClient();

  const [erori, setErori] = useState<string[]>([]);
  const [mesaje, setMesaje] = useState<string[]>([]);
  const [cerere, setCerere] = useState<Cerere | null>(null);

  const citit = useQuery({
    queryKey: ['amo', id],
    queryFn: () => amo.citeste(id!),
    enabled: id != null,
  });
  const doc = citit.data;

  async function invalideaza() {
    await cache.invalidateQueries({ queryKey: ['amo'] });
    // Amortizarea scrie în registrul de imobilizări și în cel contabil.
    await cache.invalidateQueries({ queryKey: ['imobilizari'] });
    await cache.invalidateQueries({ queryKey: ['stingeri'] });
  }

  async function comanda(rulare: () => Promise<{ Mesaje?: string[] | null; StareNoua?: string | null }>) {
    setErori([]);
    setMesaje([]);
    try {
      const rezultat = await rulare();
      setMesaje([`Stare nouă: ${labelEnum('StareDocument', rezultat.StareNoua)}`, ...(rezultat.Mesaje ?? [])]);
      await invalideaza();
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
      const r = await amo.regenereaza(id!);
      await invalideaza();
      // Draftul vechi e ȘTERS, iar cel nou are alt id: rămânerea pe URL-ul
      // vechi ar arăta un 404. 200 fără document e legitim (luna n-are ce
      // amortiza) și iese ca raport, pe listă.
      if (r.DocumentId) {
        navigheaza(`/amo/${r.DocumentId}`, { replace: true });
        return;
      }
      navigheaza('/amo');
    }
    catch (e) {
      setErori(eroriDin(e));
      // Și pe refuz se reîncarcă: serverul garantează că draftul rămâne intact,
      // dar ecranul nu presupune — recitește starea REALĂ.
      await invalideaza();
    }
  }

  async function stergeDocumentul() {
    setCerere(null);
    setErori([]);
    setMesaje([]);
    try {
      await amo.sterge(id!);
      await invalideaza();
      navigheaza('/amo');
    }
    catch (e) {
      setErori(eroriDin(e));
    }
  }

  const comenzi: Comanda[] = [
    {
      eticheta: 'Verifică',
      disponibila: doc != null,
      ruleaza: () => void raporteaza(amo.valideaza(id!).then((e) => {
        if (e.length > 0) { setErori(e); return []; }
        return ['Amortizarea trece toți gardienii motorului.'];
      })),
    },
    {
      eticheta: 'Operează',
      disponibila: doc?.PoateOpera ?? false,
      primara: true,
      ruleaza: () => void comanda(() => amo.opereaza(id!)),
    },
    {
      eticheta: 'Anulează operarea',
      disponibila: doc?.PoateAnula ?? false,
      ruleaza: () => void comanda(() => amo.anuleaza(id!)),
    },
    {
      eticheta: 'Stornează',
      disponibila: doc?.PoateStorna ?? false,
      // Implicitul e chiar `Data` documentului — ultima zi a lunii amortizate:
      // stornarea acolo redeschide luna.
      cereData: { eticheta: 'Data stornării', implicit: doc?.Data },
      ruleaza: (data) => { if (data) void comanda(() => amo.storneaza(id!, data)); },
    },
    {
      eticheta: 'Regenerează',
      disponibila: doc?.PoateRegenera ?? false,
      ruleaza: () => setCerere('regenereaza'),
    },
    { eticheta: 'Șterge', disponibila: doc?.PoateSterge ?? false, ruleaza: () => setCerere('sterge') },
    { eticheta: 'Înapoi la listă', disponibila: true, ruleaza: () => navigheaza('/amo') },
  ];

  return (
    <DocumentShell
      corectie={<CorectieDocument id={doc?.Id} stare={doc?.Stare} corectie={doc?.Corectie} />}
      citire={citit}
      titlu={`Amortizare lunară ${doc?.Numar ?? '(draft)'}`}
      sumar={<Sumar doc={doc} />}
      comenzi={comenzi}
      confirmare={cerere === 'sterge' ? (
        <ConfirmareInline
          intrebare="Ștergeți definitiv acest draft de amortizare, cu tot cu liniile lui?"
          verb="Șterge amortizarea"
          onConfirma={() => void stergeDocumentul()}
          onRenunta={() => setCerere(null)}
        />
      ) : cerere === 'regenereaza' ? (
        <ConfirmareInline
          intrebare="Draftul se șterge și se generează din nou, pe situația de acum a fișelor. Amortizarea va primi alt număr intern."
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
              ? ['Situația fișelor s-a schimbat de la generarea acestui draft — regenerați amortizarea înainte de a o opera.']
              : []}
            titlu="Draft depășit"
            fel="atentie"
          />

          <div className="grila-campuri">
            <Static membru="Numar" valoare={doc?.Numar} />
            <Static membru="Data" valoare={ziLocala(doc?.Data)} />
            <Static membru="DataInregistrare" valoare={ziLocala(doc?.DataInregistrare)} />
            <Explicit eticheta="Luna amortizată" valoare={etichetaLuna(doc)} />
            <Explicit eticheta="Unitatea internă" valoare={doc?.UnitateDenumire} />
            <Static membru="Stare" valoare={labelEnum('StareDocument', doc?.Stare)} />
            <Static membru="DataOperare" valoare={ziLocala(doc?.DataOperare)} />
          </div>
        </>
      }
      linii={
        <>
          <div className="linii__bara"><h3>Liniile amortizării</h3></div>
          {/* Grilă de CITIRE: liniile sunt ale serviciului. Linia care nu
              postează (contabil 0, fiscal pozitiv) rămâne FĂRĂ conturi — celule
              goale, nu un „—" care ar sugera o valoare lipsă. */}
          <DataGrid dataSource={doc?.Linii ?? []} keyExpr="Id" showBorders columnAutoWidth>
            <Column dataField="NumarInventar" caption="Număr de inventar" />
            <Column dataField="Denumire" caption="Denumire" />
            <Column dataField="Contabil" caption="Amortizare contabilă" dataType="number" format="#,##0.00" alignment="right" />
            <Column dataField="Fiscal" caption="Amortizare fiscală" dataType="number" format="#,##0.00" alignment="right" />
            <Column dataField="Deductibil" caption="Amortizare deductibilă" dataType="number" format="#,##0.00" alignment="right" />
            <Column dataField="Luni" caption="Luni acoperite" dataType="number" alignment="right" />
            <Column dataField="ContCheltuialaSimbol" caption="Cont de cheltuială" width={140} />
            <Column dataField="ContAmortizareSimbol" caption="Cont de amortizare" width={140} />
            <Column dataField="LocDenumire" caption="Loc" />
          </DataGrid>

          <table className="tabel-mic" style={{ marginTop: 8 }}>
            <tbody>
              <tr>
                <th>Total contabil</th>
                <td className="num">{bani(doc?.TotalContabil)}</td>
                <th>Total fiscal</th>
                <td className="num">{bani(doc?.TotalFiscal)}</td>
                <th>Total deductibil</th>
                <td className="num">{bani(doc?.TotalDeductibil)}</td>
              </tr>
            </tbody>
          </table>
        </>
      }
      subsol={
        <p className="indiciu">
          Amortizarea nu se culege: liniile le produce motorul din situația fișelor la sfârșitul lunii,
          pe conturile din politica de amortizare. „Regenerează” o reface pe situația de acum.
          Cronologia e strictă — o lună ulterioară operată blochează orice eveniment retroactiv.
        </p>
      }
    />
  );
}

// Câmp de AFIȘARE cu caption din metadata (aceeași ramă ca la culegere).
function Static(props: { membru: string; valoare: ReactNode }) {
  const meta = { ...campMeta(TIP_ANTET, props.membru, SCHEMA_ANTET), obligatoriu: false };
  return <Rama meta={meta} valoare={props.valoare} />;
}

// Câmp de afișare pentru ce NU e membru de entitate (`An`/`Luna` sunt derivate
// de DTO, unitatea e numită de felie): eticheta se scrie explicit, altfel
// `campMeta` ar avertiza în consolă și ar cădea pe numele câmpului.
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

function Sumar({ doc }: { doc: AmoRead | undefined }) {
  return (
    <div className="sumar">
      <span className="sumar__stare">{labelEnum('StareDocument', doc?.Stare) || '—'}</span>
      <span className="sumar__total">
        Amortizare contabilă: {doc?.TotalContabil == null ? '—' : bani(doc.TotalContabil)}
      </span>
    </div>
  );
}

function etichetaLuna(doc: AmoRead | undefined): string | null {
  if (!doc || doc.Luna == null || doc.An == null) return null;
  return `${LUNI[doc.Luna - 1] ?? doc.Luna} ${doc.An}`;
}
