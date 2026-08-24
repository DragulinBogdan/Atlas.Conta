import { useState } from 'react';
import { Formular, eroriStructurale } from '../../nucleu/formular';
import { CampNumar, CampText } from '../../nucleu/campuri';
import { Lookup } from '../../nucleu/Lookup';
import { PanouErori } from '../../nucleu/PanouErori';
import { SCHEMA_LINIE, TIP_LINIE, type DecLinieWrite } from './api';

// Liniile de draft se editează cu VOCABULARUL `Camp*`, nu în grilă (43c): grila
// doar afișează. Editorul nu vorbește cu serverul — CRUD-ul per linie a fost
// respins în designul API (agregatul se trimite întreg).
//
// Ce e PROPRIU feliei DEC:
//
//  1. **POSTAREA EXPLICITĂ PE LINIE** (32a) — grup pliat, separat de culegerea
//     obișnuită: e trăsătura tipului, nu default-ul. Cele patru câmpuri sunt
//     opționale; ce rămâne gol cade pe regula de contare (debit din contul
//     Tipului, credit pe titular cu fallback 542), ce e cules o BATE. Clientul
//     nu deduce nimic din combinația lor — le culege și le trimite.
//  2. **Contul e ofertat NE-sumator** (`filtru: Sumator = false`, F8-D5) —
//     AFFORDANCE, nu validare: e o ușurare de căutare pe un plan de 1.679 de
//     conturi, nu o regulă nouă de refuz. Autoritatea rămâne motorul.
//  3. **Cantitatea e PRO-FORMĂ** (32d): 0 rămâne 0 aici și devine 1 la SERVER,
//     la culegere (F8-D2) — nu se normalizează în TS.
//  4. **TVA-ul se poartă la fel ca la FCT/FCL** (o singură semantică pe tot
//     clientul): pe linie NOUĂ `TipTva` gol = implicitul tipului de document; pe
//     linie EXISTENTĂ golirea e deliberată; `ValoareTva` se trimite DOAR dacă
//     operatorul a atins câmpul în sesiunea asta (override manual — regula 36a).
const CAMPURI: (keyof DecLinieWrite & string)[] = ['TipMaterialId', 'Cantitate', 'PretUnitar'];

// Etichetele CULESE la selecție, pentru grila documentului (mecanismul 61b):
// liniile nesalvate n-au încă ReadDto, dar răspunsul OData al selecției e DEJA
// în client — nu se inventează nimic în TS, se reține ce a afișat lookup-ul.
// `Valoare`/`ValoareTva` rămân ale serverului (apar după Salvează).
export type EticheteCulese = {
  TipMaterialCod?: string;
  TipMaterialDenumire?: string;
  TipTvaCod?: string;
  CodEconomicCod?: string;
  AngajamentCod?: string;
  ContDebitSimbol?: string;
  ContCreditSimbol?: string;
  RepartitorDebitDenumire?: string;
  RepartitorCreditDenumire?: string;
};

const text = (v: unknown) => (v == null ? '' : String(v));

// Conturile ne-sumatoare, ca AFFORDANCE de căutare (vezi antetul). Scris o
// singură dată: aceeași mulțime pentru ambele laturi.
const FILTRU_CONT_FRUNZA = ['Sumator', '=', false];

export function DecEditorLinie(props: {
  linie: DecLinieWrite;
  // Ce a calculat SERVERUL pentru linia asta (ReadDto) — doar pentru afișare.
  valoareTvaCitita?: number | null;
  readOnly: boolean;
  onSalveaza: (l: DecLinieWrite, etichete: EticheteCulese) => void;
  onRenunta: () => void;
}) {
  const { readOnly, onSalveaza, onRenunta } = props;
  const [linie, setLinie] = useState<DecLinieWrite>(() => ({
    ...props.linie,
    ValoareTva: props.linie.ValoareTva ?? props.valoareTvaCitita ?? undefined,
  }));
  const [etichete, setEtichete] = useState<EticheteCulese>({});
  // Override-ul de TVA nu e un flag global de formular: e starea UNUI câmp, iar
  // singura sursă de adevăr e „valoarea lui s-a schimbat de când s-a deschis
  // editorul" (același mecanism ca la FCT/FCL).
  const [tvaAtins, setTvaAtins] = useState(props.linie.ValoareTva != null);
  const [aratErori, setAratErori] = useState(false);
  const structurale = eroriStructurale(TIP_LINIE, SCHEMA_LINIE, linie as Record<string, unknown>, CAMPURI);

  function schimba(v: DecLinieWrite) {
    if (v.ValoareTva !== linie.ValoareTva)
      setTvaAtins(true);
    setLinie(v);
  }

  function confirma() {
    setAratErori(true);
    if (structurale.length > 0)
      return;
    onSalveaza({ ...linie, ValoareTva: tvaAtins ? linie.ValoareTva ?? null : null }, etichete);
  }

  return (
    <div className="editor-linie">
      <Formular
        tip={TIP_LINIE}
        schema={SCHEMA_LINIE}
        valoare={linie}
        onSchimba={schimba}
        readOnly={readOnly}
        aratErori={aratErori}
      >
        <div className="grila-campuri">
          <Lookup<DecLinieWrite>
            camp="TipMaterialId"
            entitate="TipMaterial"
            mod="local"
            afisare={codSiDenumire}
            cauta={['Cod', 'Denumire']}
            laSelectie={(t) => setEtichete((prev) => ({
              ...prev, TipMaterialCod: text(t?.Cod), TipMaterialDenumire: text(t?.Denumire),
            }))}
          />
          <CampText<DecLinieWrite> camp="Descriere" />
          <div>
            <CampNumar<DecLinieWrite> camp="Cantitate" />
            <p className="indiciu">
              Pro-formă: lăsată 0, serverul o aduce la 1 la salvare.
            </p>
          </div>
          <CampNumar<DecLinieWrite> camp="PretUnitar" zecimale={6} />
          <Lookup<DecLinieWrite>
            camp="TipTvaId"
            entitate="TipTva"
            mod="local"
            afisare={etichetaTipTva}
            cauta={['Cod', 'Denumire']}
            laSelectie={(t) => setEtichete((prev) => ({ ...prev, TipTvaCod: text(t?.Cod) }))}
          />
          <div>
            <CampNumar<DecLinieWrite> camp="ValoareTva" zecimale={2} />
            <p className="indiciu">
              {tvaAtins
                ? 'Suprascris manual — se trimite ca atare.'
                : 'Calculat de server; modificați doar dacă bonul justificat diferă.'}
            </p>
          </div>
        </div>

        {/* POSTAREA EXPLICITĂ (32a) — trăsătura PROPRIE a tipului, nu
            default-ul culegerii: de asta e `<details>` colapsat, ca „Plată
            automată" pe FCT. Toate patru câmpurile rămân opționale; ce nu se
            culege cade pe regula de contare. */}
        <details className="sectiune-pliabila">
          <summary>Postare explicită{arePostareExplicita(linie) ? ' — activă' : ''}</summary>
          <div className="grila-campuri">
            <Lookup<DecLinieWrite>
              camp="ContDebitId"
              entitate="Cont"
              mod="remote"
              afisare={etichetaCont}
              cauta={['Simbol', 'Denumire']}
              filtru={FILTRU_CONT_FRUNZA}
              laSelectie={(c) => setEtichete((prev) => ({ ...prev, ContDebitSimbol: text(c?.Simbol) }))}
            />
            <Lookup<DecLinieWrite>
              camp="ContCreditId"
              entitate="Cont"
              mod="remote"
              afisare={etichetaCont}
              cauta={['Simbol', 'Denumire']}
              filtru={FILTRU_CONT_FRUNZA}
              laSelectie={(c) => setEtichete((prev) => ({ ...prev, ContCreditSimbol: text(c?.Simbol) }))}
            />
            {/* Repartitorul explicit e nivelul MAXIM al coalesce-ului de
                dimensiuni (32a). Lookup pe BAZA `Repartitor` (F8-D4): câmpurile
                acceptă orice repartitor, iar o listă pe o singură derivată ar
                minți prin omisiune. */}
            <Lookup<DecLinieWrite>
              camp="RepartitorDebitId"
              entitate="Repartitor"
              mod="remote"
              afisare={codSiDenumire}
              cauta={['Denumire', 'Cod']}
              laSelectie={(r) => setEtichete((prev) => ({ ...prev, RepartitorDebitDenumire: text(r?.Denumire) }))}
            />
            <Lookup<DecLinieWrite>
              camp="RepartitorCreditId"
              entitate="Repartitor"
              mod="remote"
              afisare={codSiDenumire}
              cauta={['Denumire', 'Cod']}
              laSelectie={(r) => setEtichete((prev) => ({ ...prev, RepartitorCreditDenumire: text(r?.Denumire) }))}
            />
          </div>
          <p className="indiciu">
            Opționale: contul cules bate rezolvarea declarativă, iar ce rămâne gol cade pe regula
            de contare a tipului. Lista oferă doar conturi ne-sumatoare — refuzurile rămân ale
            motorului, la operare.
          </p>
        </details>

        {/* Clasificația bugetară e a PROFILULUI (54d): la privat nomenclatoarele
            sunt goale și secțiunea rămâne pliată. Politica bugetară cere pe DEC
            angajament SAU cod economic (33c) — invariant al OPERĂRII, nu al
            culegerii, deci aici nu e nimic obligatoriu. */}
        <details className="sectiune-pliabila">
          <summary>Clasificație bugetară</summary>
          <div className="grila-campuri">
            <Lookup<DecLinieWrite>
              camp="CodEconomicId"
              entitate="CodEconomic"
              mod="local"
              afisare={codSiDenumire}
              cauta={['Cod', 'Denumire']}
              laSelectie={(d) => setEtichete((prev) => ({ ...prev, CodEconomicCod: text(d?.Cod) }))}
            />
            {/* Modulul de angajamente e amânat (22c), deci tabela e azi GOALĂ:
                lookup-ul e ONEST — gol înseamnă gol (F8-D4). */}
            <Lookup<DecLinieWrite>
              camp="AngajamentId"
              entitate="Angajament"
              mod="local"
              afisare={codSiDenumire}
              cauta={['Cod', 'Denumire']}
              laSelectie={(a) => setEtichete((prev) => ({ ...prev, AngajamentCod: text(a?.Cod) }))}
            />
          </div>
        </details>
      </Formular>

      {aratErori && <PanouErori erori={structurale} titlu="Completați linia" />}

      <div className="editor-linie__comenzi">
        <button type="button" className="buton buton--primar" disabled={readOnly} onClick={confirma}>
          {props.linie.Id ? 'Actualizează linia' : 'Adaugă linia'}
        </button>
        <button type="button" className="buton" onClick={onRenunta}>Renunță</button>
      </div>
    </div>
  );
}

// Doar pentru eticheta secțiunii pliate (ca „Plată automată — activă" pe FCT):
// spune că grupul ascunde ceva cules, nu ce înseamnă combinația — semantica
// parțialului (doar debit, doar repartitor) e a motorului.
function arePostareExplicita(l: DecLinieWrite): boolean {
  return l.ContDebitId != null || l.ContCreditId != null
    || l.RepartitorDebitId != null || l.RepartitorCreditId != null;
}

// Nomenclatoarele au `DefaultProperty = Denumire`, dar operatorul caută pe cod —
// afișăm amândouă. Fără `Cod`, două omonime sunt indistinctibile.
function codSiDenumire(element: Record<string, unknown>): string {
  if (!element) return '';
  const cod = element.Cod == null ? '' : String(element.Cod);
  const denumire = element.Denumire == null ? '' : String(element.Denumire);
  return cod && denumire ? `${cod} — ${denumire}` : cod || denumire;
}

// Contul are `DefaultProperty = Simbol`; contabilul caută pe amândouă.
function etichetaCont(element: Record<string, unknown>): string {
  if (!element) return '';
  const simbol = element.Simbol == null ? '' : String(element.Simbol);
  const denumire = element.Denumire == null ? '' : String(element.Denumire);
  return simbol && denumire ? `${simbol} — ${denumire}` : simbol || denumire;
}

function etichetaTipTva(element: Record<string, unknown>): string {
  if (!element) return '';
  const cota = element.Cota == null ? null : Number(element.Cota);
  return `${codSiDenumire(element)}${cota == null ? '' : ` (${cota}%)`}`;
}
