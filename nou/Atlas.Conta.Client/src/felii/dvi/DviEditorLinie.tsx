import { useState } from 'react';
import { Formular, eroriStructurale } from '../../nucleu/formular';
import { CampNumar } from '../../nucleu/campuri';
import { Lookup } from '../../nucleu/Lookup';
import { usePrecompletareTipTva } from '../../nucleu/implicite';
import { PanouErori } from '../../nucleu/PanouErori';
import { SCHEMA_LINIE, TIP_LINIE, type DviLinieWrite } from './api';

// Linia declarației vamale: patru câmpuri, pe BAZA `DocumentDetaliu` (DVI-D1).
// Ce e propriu feliei:
//
//  1. **Tipul de TVA e filtrat pe `DeImport`** — coloana de nomenclator adăugată
//     de felie e singura cale de a spune „tip de import" fără cod hardcodat
//     (29). Filtrul pleacă pe sârmă ca `$filter=DeImport eq true`; refuzul
//     liniei cu un tip nepotrivit rămâne al motorului, la operare.
//  2. **Implicitul vine tot de la server** (F23-D6), pe ancora `DVI`, și doar pe
//     linie nouă cu câmpul gol — aceeași funcție pe care o aplică PUT-ul.
//  3. **`ValoareTva` NU e override** (spre deosebire de FCT): e taxa din
//     declarație, culeasă. Zero înseamnă „se naște din cotă la operare" (48b: un
//     TVA cules nu se suprascrie), deci se trimite mereu, fără stare „atins".
const CAMPURI: (keyof DviLinieWrite & string)[] = ['TipMaterialId', 'Valoare'];

// Filtrul lookup-ului, CONSTANT: scris inline în JSX ar fi un array nou la
// fiecare randare, iar `Lookup` reconstruiește sursa la schimbarea conținutului.
const DOAR_IMPORT: unknown[] = ['DeImport', '=', true];

// Etichetele CULESE la selecție, pentru grila documentului: liniile nesalvate
// n-au încă ReadDto, dar răspunsul OData al selecției e DEJA în client.
export type EticheteCulese = {
  TipMaterialCod?: string;
  TipMaterialDenumire?: string;
  TipTvaCod?: string;
  TipTvaDenumire?: string;
};

const text = (v: unknown) => (v == null ? '' : String(v));

export function DviEditorLinie(props: {
  linie: DviLinieWrite;
  // Etichetele deja cunoscute ale liniei (tipul sugerat de facturile legate,
  // rezolvat de ecran) — pornesc starea, ca grila să nu rămână goală pe o linie
  // pe care operatorul n-a atins lookup-ul.
  eticheteInitiale?: EticheteCulese;
  // Contextul implicitului de TVA: pe DVI partenerul e PREDATORUL (biroul
  // vamal), iar data e a declarației, nu ziua de azi.
  partenerId?: string | null;
  data?: string | null;
  readOnly: boolean;
  onSalveaza: (l: DviLinieWrite, etichete: EticheteCulese) => void;
  onRenunta: () => void;
}) {
  const { readOnly, onSalveaza, onRenunta } = props;
  const [linie, setLinie] = useState<DviLinieWrite>(props.linie);
  const [etichete, setEtichete] = useState<EticheteCulese>(props.eticheteInitiale ?? {});
  const [aratErori, setAratErori] = useState(false);
  const structurale = eroriStructurale(TIP_LINIE, SCHEMA_LINIE, linie as Record<string, unknown>, CAMPURI);

  const motivImplicit = usePrecompletareTipTva(
    {
      tipDocument: 'DVI',
      partenerId: props.partenerId,
      data: props.data,
    },
    props.linie.Id == null && linie.TipTvaId == null,
    (tipTvaId, cod) => {
      setLinie((prev) => (prev.TipTvaId ? prev : { ...prev, TipTvaId: tipTvaId }));
      setEtichete((prev) => (prev.TipTvaCod ? prev : { ...prev, TipTvaCod: cod ?? '' }));
    });

  function confirma() {
    setAratErori(true);
    if (structurale.length > 0)
      return;
    onSalveaza(linie, etichete);
  }

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
          <Lookup<DviLinieWrite>
            camp="TipMaterialId"
            entitate="TipMaterial"
            mod="local"
            afisare={codSiDenumire}
            laSelectie={(t) => setEtichete((prev) => ({
              ...prev, TipMaterialCod: text(t?.Cod), TipMaterialDenumire: text(t?.Denumire),
            }))}
          />
          <div>
            <Lookup<DviLinieWrite>
              camp="TipTvaId"
              entitate="TipTva"
              mod="local"
              filtru={DOAR_IMPORT}
              afisare={etichetaTipTva}
              laSelectie={(t) => setEtichete((prev) => ({
                ...prev, TipTvaCod: text(t?.Cod), TipTvaDenumire: text(t?.Denumire),
              }))}
            />
            {motivImplicit && <p className="indiciu">{motivImplicit}</p>}
          </div>
          <CampNumar<DviLinieWrite> camp="Valoare" zecimale={2} />
          <div>
            <CampNumar<DviLinieWrite> camp="ValoareTva" zecimale={2} />
            <p className="indiciu">Lăsat 0, îl calculează motorul din cota tipului la operare.</p>
          </div>
        </div>
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
// se afișează amândouă.
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
