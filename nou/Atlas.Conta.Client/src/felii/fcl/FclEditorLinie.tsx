import { useState } from 'react';
import { Formular, eroriStructurale } from '../../nucleu/formular';
import { CampNumar, CampText } from '../../nucleu/campuri';
import { Lookup } from '../../nucleu/Lookup';
import { LookupGrila } from '../../nucleu/LookupGrila';
import { PanouErori } from '../../nucleu/PanouErori';
import { etichetaLot } from '../../nucleu/lot';
import { SCHEMA_LINIE, TIP_LINIE, type FclLinieWrite } from './api';

// Liniile de draft se editează cu VOCABULARUL `Camp*`, nu în grilă (43c): grila
// doar afișează. Editorul nu vorbește cu serverul — CRUD-ul per linie a fost
// respins în designul API (agregatul se trimite întreg).
//
// Ce e PROPRIU feliei FCL, față de FCT:
//
//  1. **General! + Specific?** (37d/P2 §4). `Produs` e identitatea liniei de
//     stoc — el o face descărcabilă, iar Tipul (contul/clasa) se precompletează
//     din răspunsul OData al SELECȚIEI, fără fetch în plus și doar când e gol.
//     `Lot` e RAFINAREA opțională: pinul de identificare specifică, activ doar
//     după alegerea produsului și filtrat pe el.
//  2. **Pinul aparține produsului pentru care a fost ales**: la schimbarea
//     produsului se stinge (vezi `schimba`) — altfel ar rămâne un lot al altui
//     produs, refuzat abia la operare.
//  3. **TVA-ul se poartă la fel ca la FCT** (o singură semantică pe tot
//     clientul): pe linie NOUĂ `TipTva` gol = implicitul tipului de document; pe
//     linie EXISTENTĂ golirea e deliberată; `ValoareTva` se trimite DOAR dacă
//     operatorul a atins câmpul în sesiunea asta (override manual — regula 36a).
//
// Ce NU face: nu decide dacă linia e „de stoc" (o spune natura Tipului, la
// server), nu verifică soldul lotului („întâi BTR" e refuz al motorului — F4-D6)
// și nu calculează nicio valoare.
const CAMPURI: (keyof FclLinieWrite & string)[] = ['TipMaterialId', 'Cantitate', 'PretUnitar'];

// Etichetele CULESE la selecție, pentru grila documentului: liniile nesalvate
// n-au încă ReadDto, dar răspunsul OData al selecției e DEJA în client — nu se
// inventează nimic în TS, se reține ce a afișat lookup-ul. Valoare/ValoareTva
// rămân ale serverului (apar după Salvează).
export type EticheteCulese = {
  TipMaterialCod?: string;
  TipMaterialDenumire?: string;
  ProdusDenumire?: string;
  LotEticheta?: string;
  TipTvaCod?: string;
};

const text = (v: unknown) => (v == null ? '' : String(v));

export function FclEditorLinie(props: {
  linie: FclLinieWrite;
  // Ce a calculat SERVERUL pentru linia asta (ReadDto) — doar pentru afișare.
  valoareTvaCitita?: number | null;
  readOnly: boolean;
  onSalveaza: (l: FclLinieWrite, etichete: EticheteCulese) => void;
  onRenunta: () => void;
}) {
  const { readOnly, onSalveaza, onRenunta } = props;
  const [linie, setLinie] = useState<FclLinieWrite>(() => ({
    ...props.linie,
    ValoareTva: props.linie.ValoareTva ?? props.valoareTvaCitita ?? undefined,
  }));
  const [etichete, setEtichete] = useState<EticheteCulese>({});
  // Override-ul de TVA nu e un flag global de formular: e starea UNUI câmp, iar
  // singura sursă de adevăr e „valoarea lui s-a schimbat de când s-a deschis
  // editorul" (același mecanism ca la FCT).
  const [tvaAtins, setTvaAtins] = useState(props.linie.ValoareTva != null);
  const [aratErori, setAratErori] = useState(false);
  const structurale = eroriStructurale(TIP_LINIE, SCHEMA_LINIE, linie as Record<string, unknown>, CAMPURI);
  const produsId = linie.ProdusId ?? null;

  function schimba(v: FclLinieWrite) {
    if (v.ValoareTva !== linie.ValoareTva)
      setTvaAtins(true);
    // Pinul se stinge la schimbarea produsului. Comparația se face AICI, unde
    // `linie` e încă valoarea dinaintea schimbării — într-un update funcțional
    // (`laSelectie`) produsul nou ar fi deja în `prev`, iar comparația ar fi
    // mereu falsă.
    const lot = v.ProdusId !== linie.ProdusId ? null : v.LotId ?? null;
    setLinie({ ...v, LotId: lot });
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
          <div>
            <LookupGrila<FclLinieWrite>
              camp="ProdusId"
              entitate="Produs"
              afisare={codSiDenumire}
              expand={['TipMaterial']}
              laSelectie={(p) => {
                // ODataStore deserializează Edm.Guid ca OBIECT `Guid` DevExtreme,
                // nu ca string — `String()` îl aduce la forma de sârmă. Aplicarea
                // e UPDATE FUNCȚIONAL pe starea liniei: `seteaza`-ul valorii a
                // rulat deja în același event, iar un patch din closure l-ar fi
                // pierdut.
                const tip = p?.TipMaterialId == null ? undefined : String(p.TipMaterialId);
                if (tip)
                  setLinie((prev) => prev.TipMaterialId ? prev : { ...prev, TipMaterialId: tip });
                // Eticheta Tipului precompletat vine din `$expand=TipMaterial` al
                // aceleiași selecții (doar când precompletarea chiar se aplică);
                // eticheta pinului se stinge odată cu pinul (`schimba`).
                const tipEl = p?.TipMaterial as Record<string, unknown> | null | undefined;
                setEtichete((prev) => ({
                  ...prev,
                  ProdusDenumire: text(p?.Denumire),
                  LotEticheta: '',
                  ...(tip && !linie.TipMaterialId
                    ? { TipMaterialCod: text(tipEl?.Cod), TipMaterialDenumire: text(tipEl?.Denumire) }
                    : {}),
                }));
              }}
            />
            <p className="indiciu">
              Obligatoriu pe liniile de stoc — el face linia descărcabilă din gestiune.
            </p>
          </div>
          <Lookup<FclLinieWrite>
            camp="TipMaterialId"
            entitate="TipMaterial"
            mod="local"
            afisare={codSiDenumire}
            laSelectie={(t) => setEtichete((prev) => ({
              ...prev, TipMaterialCod: text(t?.Cod), TipMaterialDenumire: text(t?.Denumire),
            }))}
          />
          <div>
            {/* PINUL de lot (F4-D6): identificare specifică, prioritară la
                picking și FĂRĂ fallback FIFO pe restul ei — de asta e opțional.
                Sursa e OData `Lot`, filtrat pe produsul liniei; `Lot` n-are `Cod`/
                `Denumire`, iar `DefaultProperty` (`Eticheta`) e [NotMapped] și nu
                traversează OData (restanța 40d) — de aceea eticheta se compune
                aici din `$expand=Produs` + dată + preț, iar sortarea/căutarea se
                dau pe coloane REALE (`Data` = ordinea FIFO, `LotFabricatie`). */}
            <LookupGrila<FclLinieWrite>
              camp="LotId"
              entitate="Lot"
              readOnly={produsId == null}
              afisare={etichetaLot}
              expand={['Produs']}
              sortare="Data"
              cauta={['LotFabricatie']}
              filtru={produsId == null ? undefined : ['ProdusId', '=', produsId]}
              laSelectie={(l) => setEtichete((prev) => ({ ...prev, LotEticheta: l ? etichetaLot(l) : '' }))}
            />
            <p className="indiciu">
              {produsId == null
                ? 'Alegeți întâi produsul — loturile se filtrează pe el.'
                : 'Opțional: fixează lotul descărcat. Gol = FIFO în gestiunea facturii.'}
            </p>
          </div>
          <CampNumar<FclLinieWrite> camp="Cantitate" />
          <CampNumar<FclLinieWrite> camp="PretUnitar" zecimale={6} />
          <Lookup<FclLinieWrite>
            camp="TipTvaId"
            entitate="TipTva"
            mod="local"
            afisare={etichetaTipTva}
            laSelectie={(t) => setEtichete((prev) => ({ ...prev, TipTvaCod: text(t?.Cod) }))}
          />
          <div>
            <CampNumar<FclLinieWrite> camp="ValoareTva" zecimale={2} />
            <p className="indiciu">
              {/* Golirea câmpului NU anulează suprascrierea (payload-ul fără
                  ValoareTva = „nu m-am pronunțat", regula F2 — serverul păstrează
                  override-ul salvat); indiciul spune adevărul pe cazul ăsta
                  (review F4/M1). */}
              {tvaAtins
                ? (linie.ValoareTva == null
                  ? 'Golirea nu anulează suprascrierea salvată — modificați cantitatea, prețul sau tipul de TVA ca să revină calculul.'
                  : 'Suprascris manual — se trimite ca atare.')
                : 'Calculat de server din preț × cantitate; modificați doar dacă factura emisă diferă.'}
            </p>
          </div>
          <CampText<FclLinieWrite> camp="Descriere" />
        </div>

        {/* Clasificația bugetară e a PROFILULUI (54d): la privat nomenclatorul e
            gol și secțiunea rămâne pliată. Pe frunza FCL dimensiunea culeasă e
            una singură — `CodEconomic` (DIM-2). */}
        <details className="sectiune-pliabila">
          <summary>Clasificație bugetară</summary>
          <div className="grila-campuri">
            <Lookup<FclLinieWrite> camp="CodEconomicId" entitate="CodEconomic" mod="local" afisare={codSiDenumire} />
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

// Nomenclatoarele au `DefaultProperty = Denumire`, dar operatorul caută pe cod —
// afișăm amândouă.
function codSiDenumire(element: Record<string, unknown>): string {
  if (!element) return '';
  const cod = element.Cod == null ? '' : String(element.Cod);
  const denumire = element.Denumire == null ? '' : String(element.Denumire);
  return cod && denumire ? `${cod} — ${denumire}` : cod || denumire;
}

function etichetaTipTva(element: Record<string, unknown>): string {
  if (!element) return '';
  const cota = element.Cota == null ? null : Number(element.Cota);
  return `${codSiDenumire(element)}${cota == null ? '' : ` (${cota}%)`}`;
}
