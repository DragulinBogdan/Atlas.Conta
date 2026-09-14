import { useEffect, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Formular, eroriStructurale } from '../../nucleu/formular';
import { CampBifa, CampNumar, CampSelectie } from '../../nucleu/campuri';
import { Lookup } from '../../nucleu/Lookup';
import { PanouErori } from '../../nucleu/PanouErori';
import { campMeta, labelEnum } from '../../nucleu/campMeta';
import { bani } from '../../nucleu/format';
import { ziLocala } from '../../nucleu/zi';
import { citesteFisa, SCHEMA_LINIE, TIP_LINIE, type FisaImobilizare, type LinieSursaCandidata, type PifLinieWrite } from './api';
import { PanouLiniiSursa } from './PanouLiniiSursa';

// Linia PIF: parametrii obligatorii pe `Intrare`/`Revizuire`, opționali pe `Modernizare`; indiciile vin din fișă, refuzul e al motorului (F26-D5, 42c).

const CAMPURI: (keyof PifLinieWrite & string)[] = ['ImobilizareId'];

// Parametrii ceruți de motor pe `Intrare`/`Revizuire`; bifa se normalizează la confirmare.
const PARAMETRI: (keyof PifLinieWrite & string)[] = [
  'Metoda', 'DurataLuni', 'MetodaFiscala', 'DurataFiscalaLuni', 'CategorieFiscala',
];

const capLinie = (m: string) => campMeta(TIP_LINIE, m, SCHEMA_LINIE).caption;

export type EticheteCulese = {
  NumarInventar?: string;
  ImobilizareDenumire?: string;
  LinieSursaNumar?: string;
};

const text = (v: unknown) => (v == null ? '' : String(v));

export function PifEditorLinie(props: {
  linie: PifLinieWrite;
  eticheteInitiale?: EticheteCulese;
  // Locul documentului: fișele alese trebuie să stea pe el (un PIF acoperă un
  // singur loc).
  primitorId?: string | null;
  data?: string | null;
  // Liniile sursă legate deja de ALTE linii ale aceluiași document: serverul nu
  // le cunoaște cât timp documentul e nesalvat.
  liniiSursaFolosite?: string[];
  readOnly: boolean;
  onSalveaza: (l: PifLinieWrite, etichete: EticheteCulese) => void;
  onRenunta: () => void;
}) {
  const { primitorId, data, liniiSursaFolosite = [], readOnly, onSalveaza, onRenunta } = props;
  const [linie, setLinie] = useState<PifLinieWrite>(props.linie);
  const [etichete, setEtichete] = useState<EticheteCulese>(props.eticheteInitiale ?? {});
  const [aratErori, setAratErori] = useState(false);
  const [alegeLinieSursa, setAlegeLinieSursa] = useState(false);

  const fel = linie.Fel ?? 'Intrare';
  const cuParametriObligatorii = fel === 'Intrare' || fel === 'Revizuire';
  const areLinieSursa = linie.LinieSursaId != null;
  const cuCifreInitiale = fel === 'Intrare' && !areLinieSursa;

  const fisa = useQuery({
    queryKey: ['imobilizari', 'fisa', linie.ImobilizareId ?? null, data ?? null],
    queryFn: () => citesteFisa(linie.ImobilizareId!, data),
    enabled: linie.ImobilizareId != null,
  });

  // `Revizuire` pornește de la parametrii de ACUM ai fișei: revizuirea schimbă
  // unul sau doi, nu-i reintroduce pe toți. Prefill de valori CITITE, nu calcul
  // — și doar cât timp niciunul n-a fost atins.
  useEffect(() => {
    const situatie = fisa.data?.Situatie;
    if (fel !== 'Revizuire' || !situatie) return;
    setLinie((prev) => {
      if (PARAMETRI.some((p) => prev[p] != null)) return prev;
      return {
        ...prev,
        Metoda: situatie.Metoda ?? undefined,
        DurataLuni: situatie.DurataLuni ?? undefined,
        ValoareReziduala: situatie.ValoareReziduala ?? undefined,
        MetodaFiscala: situatie.MetodaFiscala ?? undefined,
        DurataFiscalaLuni: situatie.DurataFiscalaLuni ?? undefined,
        CategorieFiscala: situatie.CategorieFiscala ?? undefined,
        UtilizareExclusiva: situatie.UtilizareExclusiva ?? undefined,
      };
    });
  }, [fel, fisa.data]);

  // Revizuirea nu mișcă valoare: câmpul e 0 și read-only, nu o validare care să
  // refuze după ce operatorul a scris (`Imobilizari.ValideazaLinie`).
  useEffect(() => {
    if (fel !== 'Revizuire') return;
    setLinie((prev) => (prev.Valoare === 0 ? prev : { ...prev, Valoare: 0 }));
  }, [fel]);

  const structurale = [
    ...eroriStructurale(TIP_LINIE, SCHEMA_LINIE, linie as Record<string, unknown>, CAMPURI),
    ...(linie.Fel ? [] : [`„${capLinie('Fel')}” este obligatoriu.`]),
    ...(cuParametriObligatorii
      ? PARAMETRI
        .filter((p) => linie[p] == null)
        .map((p) => `„${capLinie(p)}” este obligatoriu pe linia „${labelEnum('FelLiniePif', fel)}”.`)
      : []),
  ];

  function alegeCandidat(c: LinieSursaCandidata) {
    setAlegeLinieSursa(false);
    setLinie((prev) => ({ ...prev, LinieSursaId: c.LinieId, Valoare: c.Rest ?? 0 }));
    setEtichete((prev) => ({ ...prev, LinieSursaNumar: `${text(c.Numar)} / ${ziLocala(c.Data) ?? ''}` }));
  }

  function scoateLinieSursa() {
    setLinie((prev) => ({ ...prev, LinieSursaId: undefined }));
    setEtichete((prev) => ({ ...prev, LinieSursaNumar: '' }));
  }

  function confirma() {
    setAratErori(true);
    if (structurale.length > 0)
      return;
    // Bifa nebifată = neexclusiv, declarat explicit pe felurile care cer parametrul.
    const exclusiva = linie.UtilizareExclusiva ?? (cuParametriObligatorii ? false : undefined);
    onSalveaza({ ...linie, UtilizareExclusiva: exclusiva }, etichete);
  }

  // GUID-ul primitorului e recunoscut de `tipuriGuid`; `Stare` pleacă pe sârmă
  // ca literal de string. Fără loc ales, filtrul lipsește cu totul: un
  // `LocId eq null` ar fi o întrebare despre altceva.
  const filtruFise = primitorId
    ? [['LocId', '=', primitorId], 'and', ['Stare', '=', fel === 'Intrare' ? 'Noua' : 'InFunctiune']]
    : undefined;

  return (
    <div className="editor-linie">
      <Formular
        tip={TIP_LINIE}
        schema={SCHEMA_LINIE}
        valoare={linie}
        onSchimba={setLinie}
        readOnly={readOnly}
        aratErori={aratErori}
      >
        <div className="grila-campuri">
          <CampSelectie<PifLinieWrite> camp="Fel" enumerare="FelLiniePif" obligatoriu />
          <div>
            <Lookup<PifLinieWrite>
              camp="ImobilizareId"
              entitate="Imobilizare"
              mod="remote"
              cauta="Cautare"
              filtru={filtruFise}
              afisare={numarSiDenumire}
              laSelectie={(f) => setEtichete((prev) => ({
                ...prev,
                NumarInventar: text(f?.NumarInventar),
                ImobilizareDenumire: text(f?.Denumire),
              }))}
            />
            {!primitorId && (
              <p className="indiciu">Alegeți întâi locul (primitorul documentului): fișele se filtrează pe el.</p>
            )}
          </div>
        </div>

        {fisa.data && <IndiciiFisa fisa={fisa.data} fel={fel} />}

        {fel === 'Intrare' && (
          <div className="imo__sectiune">
            <div className="linii__bara">
              <h3>Linia de factură care hrănește fișa</h3>
              <button
                type="button"
                className="buton"
                disabled={readOnly}
                onClick={() => setAlegeLinieSursa(true)}
              >
                Linie de factură…
              </button>
              {areLinieSursa && (
                <button type="button" className="buton" disabled={readOnly} onClick={scoateLinieSursa}>
                  Scoate linia sursă
                </button>
              )}
            </div>
            <p className="indiciu">
              {areLinieSursa
                ? `Linia sursă: ${etichete.LinieSursaNumar || '(aleasă)'}. Valoarea a pornit de la restul ei — plafonul e pe valoare și îl verifică motorul.`
                : 'Fără linie sursă, punerea în funcțiune e o deschidere, o producție proprie sau o migrare — atunci se culeg cifrele inițiale de mai jos.'}
            </p>
          </div>
        )}

        {cuCifreInitiale && (
          <div className="imo__sectiune">
            <h3>Cifre inițiale — deschidere / producție proprie / migrare</h3>
            <div className="grila-campuri">
              <CampNumar<PifLinieWrite> camp="AmortizareInitiala" zecimale={2} />
              <CampNumar<PifLinieWrite> camp="AmortizareFiscalaInitiala" zecimale={2} />
              <CampNumar<PifLinieWrite> camp="LuniAmortizateInitial" zecimale={0} />
            </div>
          </div>
        )}

        <div className="grila-campuri">
          <CampNumar<PifLinieWrite> camp="Valoare" zecimale={2} readOnly={fel === 'Revizuire'} />
          <div>
            <CampNumar<PifLinieWrite> camp="ValoareFiscala" zecimale={2} />
            <p className="indiciu">Lăsată goală, brutul fiscal e egal cu valoarea.</p>
          </div>
        </div>

        <div className="imo__sectiune">
          <h3>Parametrii de amortizare</h3>
          <div className="grila-campuri">
            <CampSelectie<PifLinieWrite> camp="Metoda" enumerare="MetodaAmortizare" obligatoriu={cuParametriObligatorii} />
            <CampNumar<PifLinieWrite> camp="DurataLuni" zecimale={0} obligatoriu={cuParametriObligatorii} />
            <CampNumar<PifLinieWrite> camp="ValoareReziduala" zecimale={2} />
            <CampSelectie<PifLinieWrite> camp="MetodaFiscala" enumerare="MetodaAmortizare" obligatoriu={cuParametriObligatorii} />
            <CampNumar<PifLinieWrite> camp="DurataFiscalaLuni" zecimale={0} obligatoriu={cuParametriObligatorii} />
            <CampSelectie<PifLinieWrite> camp="CategorieFiscala" enumerare="CategorieFiscala" obligatoriu={cuParametriObligatorii} />
            <CampBifa<PifLinieWrite> camp="UtilizareExclusiva" />
          </div>
          <p className="indiciu">
            Metoda degresivă cere o durată multiplu de 12 luni — graficul ei se construiește pe ani întregi.
            Durata fiscală trebuie să fie în banda clasificării fișei. Pe modernizare, parametrii lăsați goi
            rămân cei de acum ai fișei.
          </p>
        </div>
      </Formular>

      {aratErori && <PanouErori erori={structurale} titlu="Completați linia" />}

      <div className="editor-linie__comenzi">
        <button type="button" className="buton buton--primar" disabled={readOnly} onClick={confirma}>
          {props.linie.Id ? 'Actualizează linia' : 'Adaugă linia'}
        </button>
        <button type="button" className="buton" onClick={onRenunta}>Renunță</button>
      </div>

      {alegeLinieSursa && (
        <PanouLiniiSursa
          dataDocument={data}
          exclusi={liniiSursaFolosite}
          onAlege={alegeCandidat}
          onInchide={() => setAlegeLinieSursa(false)}
        />
      )}
    </div>
  );
}

// Indiciile fișei alese: banda catalogului, starea și — pe modernizare sau
// revizuire — situația de la care pornește evenimentul. Toate citite de pe
// server; ecranul nu adună și nu derivă nimic (42c).
function IndiciiFisa({ fisa, fel }: { fisa: FisaImobilizare; fel: string }) {
  const s = fisa.Situatie;
  const banda = fisa.DurataFiscalaMinLuni != null && fisa.DurataFiscalaMaxLuni != null;
  return (
    <div className="imo__sectiune">
      <p className="indiciu">
        Starea fișei: <strong>{labelEnum('StareImobilizare', fisa.Stare) || '—'}</strong>.
        {banda && (
          <>
            {' '}Banda clasificării {fisa.ClasificareCod ?? ''}:{' '}
            <strong>{fisa.DurataFiscalaMinLuni}–{fisa.DurataFiscalaMaxLuni} luni</strong>.
          </>
        )}
      </p>

      {(fel === 'Modernizare' || fel === 'Revizuire') && s && (
        <table className="tabel-mic">
          <tbody>
            <tr>
              <th>Brut contabil</th>
              <td className="num">{bani(s.Valoare)}</td>
              <th>Amortizare cumulată</th>
              <td className="num">{bani(s.Amortizare)}</td>
              <th>Net contabil</th>
              <td className="num">{bani(s.NetContabil)}</td>
            </tr>
            <tr>
              <th>Luni amortizate</th>
              <td className="num">{s.Luni ?? '—'}</td>
              <th>Metodă / durată</th>
              <td className="num">{`${labelEnum('MetodaAmortizare', s.Metoda) || '—'} / ${s.DurataLuni ?? '—'}`}</td>
              <th>Fiscal</th>
              <td className="num">{`${labelEnum('MetodaAmortizare', s.MetodaFiscala) || '—'} / ${s.DurataFiscalaLuni ?? '—'}`}</td>
            </tr>
          </tbody>
        </table>
      )}
    </div>
  );
}

// Fișa se recunoaște după numărul de inventar, dar `DefaultProperty` e
// `Denumire` — se afișează amândouă.
function numarSiDenumire(element: Record<string, unknown>): string {
  if (!element) return '';
  const numar = element.NumarInventar == null ? '' : String(element.NumarInventar);
  const denumire = element.Denumire == null ? '' : String(element.Denumire);
  return numar && denumire ? `${numar} — ${denumire}` : numar || denumire;
}
