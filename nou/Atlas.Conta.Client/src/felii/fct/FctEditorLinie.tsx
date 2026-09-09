import { useState } from 'react';
import { Formular, eroriStructurale } from '../../nucleu/formular';
import { CampData, CampNumar, CampText } from '../../nucleu/campuri';
import { Lookup } from '../../nucleu/Lookup';
import { usePrecompletareTipTva } from '../../nucleu/implicite';
import { LookupGrila } from '../../nucleu/LookupGrila';
import { PanouErori } from '../../nucleu/PanouErori';
import { SCHEMA_LINIE, TIP_LINIE, type FctLinieWrite } from './api';

// Liniile de draft se editează cu VOCABULARUL `Camp*`, nu în grilă (43c): grila
// doar afișează. Editorul nu vorbește cu serverul — CRUD-ul per linie a fost
// respins în designul API (agregatul se trimite întreg).
//
// Trei lucruri sunt PROPRII feliei FCT și se văd aici:
//
//  1. **Produs → Tip, precompletat.** Produsul e mecanismul lotului (GATE 53a):
//     el face linia „de stoc", iar Tipul (contul/clasa) rămâne obligatoriu în
//     contract. Îl precompletăm din răspunsul OData al SELECȚIEI (`laSelectie`),
//     fără fetch în plus și doar când e gol — nu suprascriem alegerea omului.
//  2. **TVA: tăcerea are semantică — dar nu e nevoie să rămână tăcere.** Pe o
//     linie EXISTENTĂ golirea e deliberată și se trimite ca atare (round-trip în
//     `spreWrite`); regula 56 nu se atinge. Pe o linie NOUĂ cu câmpul gol,
//     clientul PRE-COMPLETEAZĂ ce ar aplica serverul, cerându-i chiar lui
//     (F23-D6): aceeași funcție care rulează și la PUT, deci cele două nu pot
//     diverge. Sub câmp apare motivul serverului, nu unul reconstruit în TS.
//  3. **`ValoareTva` se trimite DOAR dacă operatorul a atins câmpul** în sesiunea
//     asta de editare: pe sârmă valoarea înseamnă „override manual, bate
//     rotunjirea" (regula 36a). Câmpul se PREPOPULEAZĂ cu ce a calculat serverul,
//     ca omul să compare cu factura — dar afișarea nu e trimitere.
const CAMPURI: (keyof FctLinieWrite & string)[] = ['TipMaterialId', 'Cantitate', 'PretUnitar'];

// Etichetele CULESE la selecție, pentru grila documentului: liniile nesalvate
// n-au încă ReadDto, dar răspunsul OData al selecției e DEJA în client — nu se
// inventează nimic în TS, se reține ce a afișat lookup-ul. Valoare/ValoareTva/
// lot rămân ale serverului (apar după Salvează).
export type EticheteCulese = {
  TipMaterialCod?: string;
  TipMaterialDenumire?: string;
  ProdusDenumire?: string;
};

const text = (v: unknown) => (v == null ? '' : String(v));

export function FctEditorLinie(props: {
  linie: FctLinieWrite;
  // Ce a calculat SERVERUL pentru linia asta (ReadDto) — doar pentru afișare.
  valoareTvaCitita?: number | null;
  // Contextul implicitului de TVA (F23-D6): pe FCT partenerul e PREDATORUL
  // (furnizorul), iar data e a documentului, nu ziua de azi.
  partenerId?: string | null;
  data?: string | null;
  readOnly: boolean;
  onSalveaza: (l: FctLinieWrite, etichete: EticheteCulese) => void;
  onRenunta: () => void;
}) {
  const { readOnly, onSalveaza, onRenunta } = props;
  const [linie, setLinie] = useState<FctLinieWrite>(() => ({
    ...props.linie,
    ValoareTva: props.linie.ValoareTva ?? props.valoareTvaCitita ?? undefined,
  }));
  const [etichete, setEtichete] = useState<EticheteCulese>({});
  // Override-ul de TVA nu e un flag global de formular: e starea UNUI câmp, iar
  // singura sursă de adevăr e „valoarea lui s-a schimbat de când s-a deschis
  // editorul". O linie care intră în editare purtând deja un override (a fost
  // suprascrisă mai devreme în sesiune) rămâne suprascrisă.
  const [tvaAtins, setTvaAtins] = useState(props.linie.ValoareTva != null);
  const [aratErori, setAratErori] = useState(false);
  const structurale = eroriStructurale(TIP_LINIE, SCHEMA_LINIE, linie as Record<string, unknown>, CAMPURI);

  // Implicitul se cere DOAR pe linie nouă cu câmpul gol; aplicarea e update
  // FUNCȚIONAL și nu trece niciodată peste o valoare existentă (77c).
  const motivImplicit = usePrecompletareTipTva(
    {
      tipDocument: 'FCT',
      partenerId: props.partenerId,
      produsId: linie.ProdusId,
      data: props.data,
    },
    props.linie.Id == null && linie.TipTvaId == null,
    (tipTvaId) => setLinie((prev) => (prev.TipTvaId ? prev : { ...prev, TipTvaId: tipTvaId })));

  function schimba(v: FctLinieWrite) {
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
          <LookupGrila<FctLinieWrite>
            camp="ProdusId"
            entitate="Produs"
            afisare={codSiDenumire}
            expand={['TipMaterial']}
            laSelectie={(p) => {
              // ODataStore deserializează Edm.Guid ca OBIECT `Guid` DevExtreme,
              // nu ca string (bug găsit la smoke: `typeof === 'string'` pica
              // mereu) — `String()` îl aduce la forma de sârmă. Aplicarea e
              // UPDATE FUNCȚIONAL pe starea liniei: `seteaza`-ul valorii a rulat
              // deja în același event, iar un patch din closure l-ar fi pierdut.
              const tip = p?.TipMaterialId == null ? undefined : String(p.TipMaterialId);
              if (tip)
                setLinie((prev) => prev.TipMaterialId ? prev : { ...prev, TipMaterialId: tip });
              // Eticheta Tipului precompletat vine din `$expand=TipMaterial` al
              // aceleiași selecții; se reține doar când precompletarea chiar se
              // aplică (guard-ul din closure e suficient pentru o etichetă).
              const tipEl = p?.TipMaterial as Record<string, unknown> | null | undefined;
              setEtichete((prev) => ({
                ...prev,
                ProdusDenumire: text(p?.Denumire),
                ...(tip && !linie.TipMaterialId
                  ? { TipMaterialCod: text(tipEl?.Cod), TipMaterialDenumire: text(tipEl?.Denumire) }
                  : {}),
              }));
            }}
          />
          <Lookup<FctLinieWrite>
            camp="TipMaterialId"
            entitate="TipMaterial"
            mod="local"
            afisare={codSiDenumire}
            laSelectie={(t) => setEtichete((prev) => ({
              ...prev, TipMaterialCod: text(t?.Cod), TipMaterialDenumire: text(t?.Denumire),
            }))}
          />
          <CampNumar<FctLinieWrite> camp="Cantitate" />
          <CampNumar<FctLinieWrite> camp="PretUnitar" zecimale={6} />
          <div>
            <Lookup<FctLinieWrite>
              camp="TipTvaId"
              entitate="TipTva"
              mod="local"
              afisare={etichetaTipTva}
            />
            {motivImplicit && <p className="indiciu">{motivImplicit}</p>}
          </div>
          <div>
            <CampNumar<FctLinieWrite> camp="ValoareTva" zecimale={2} />
            <p className="indiciu">
              {tvaAtins
                ? 'Suprascris manual — se trimite ca atare.'
                : 'Calculat de server; modificați doar dacă factura furnizorului diferă.'}
            </p>
          </div>
          <CampData<FctLinieWrite> camp="DataExpirare" />
          <CampText<FctLinieWrite> camp="LotFabricatie" />
          <CampText<FctLinieWrite> camp="CodCpv" />
        </div>

        {/* Clasificația bugetară e a PROFILULUI (54d): la privat nomenclatoarele
            sunt goale și secțiunea rămâne pliată — de asta e `<details>`, nu o
            grilă de câmpuri mereu deschisă. */}
        <details className="sectiune-pliabila">
          <summary>Clasificație bugetară</summary>
          <div className="grila-campuri">
            <Lookup<FctLinieWrite> camp="CodEconomicId" entitate="CodEconomic" mod="local" afisare={codSiDenumire} />
            <Lookup<FctLinieWrite> camp="SursaFinantareId" entitate="SursaFinantare" mod="local" afisare={codSiDenumire} />
            <Lookup<FctLinieWrite> camp="CodFunctionalId" entitate="CodFunctional" mod="local" afisare={codSiDenumire} />
            <Lookup<FctLinieWrite> camp="ProiectId" entitate="Proiect" mod="local" afisare={codSiDenumire} />
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
// afișăm amândouă. Fără `Cod`, două produse omonime sunt indistinctibile.
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
