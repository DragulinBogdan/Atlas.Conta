import { useState, type ReactNode } from 'react';
import { Formular, eroriStructurale } from '../../nucleu/formular';
import { CampData, CampNumar, CampText } from '../../nucleu/campuri';
import { CampShell } from '../../nucleu/CampShell';
import { Lookup } from '../../nucleu/Lookup';
import { LookupGrila } from '../../nucleu/LookupGrila';
import { PanouErori } from '../../nucleu/PanouErori';
import { campMeta } from '../../nucleu/campMeta';
import { SCHEMA_LINIE, TIP_LINIE, type NirLinieWrite } from './api';

// Liniile de draft se editează cu VOCABULARUL `Camp*`, nu în grilă (43c): grila
// doar afișează. Editorul nu vorbește cu serverul — CRUD-ul per linie a fost
// respins în designul API (agregatul se trimite întreg).
//
// Ce e PROPRIU feliei NIR, față de FCT:
//
//  1. **Zero TVA** (F5-D5). NIR-ul n-are `PoliticaTva`: nu există nici câmp de
//     tip TVA, nici override de `ValoareTva`, nici starea „TVA-ul a fost atins".
//     TVA-ul se postează pe factură; NIR-ul duce netul (36b).
//  2. **Două feluri de linie, distinse de LOT.** Pe recepția MANUALĂ produsul e
//     mecanismul lotului (F5-D2): serverul îl naște la salvare, cu prețul cules.
//     Pe clona CONEXĂ lotul e STRĂIN (născut pe linia facturii): marfa și prețul
//     sunt ale LUI, iar serverul le ignoră pe linie (gardul F5-D3, F5-D6a) —
//     de aceea `Produs`/`Preț unitar` se arată INERTE, nu ascunse: operatorul
//     trebuie să vadă de ce nu le poate schimba.
//  3. **Cantitatea rămâne editabilă și pe linia moștenită**: recepția PARȚIALĂ
//     (se primește mai puțin decât s-a facturat) e flux de producție, iar
//     valoarea o reevaluează serverul din prețul lotului.
const CAMPURI: (keyof NirLinieWrite & string)[] = ['TipMaterialId', 'Cantitate', 'PretUnitar'];

// Etichetele CULESE la selecție, pentru grila documentului: liniile nesalvate
// n-au încă ReadDto, dar răspunsul OData al selecției e DEJA în client — nu se
// inventează nimic în TS, se reține ce a afișat lookup-ul. Valoarea și lotul
// rămân ale serverului (apar după Salvează).
export type EticheteCulese = {
  TipMaterialCod?: string;
  TipMaterialDenumire?: string;
  ProdusDenumire?: string;
};

const text = (v: unknown) => (v == null ? '' : String(v));

export function NirEditorLinie(props: {
  linie: NirLinieWrite;
  // Proveniența lotului, DECISĂ DE SERVER (`LotStrain` din ReadDto — F5-D8):
  // clientul nu o re-derivă dintr-o euristică pe `ProdusId`.
  lotStrain: boolean;
  lotEticheta?: string;
  readOnly: boolean;
  onSalveaza: (l: NirLinieWrite, etichete: EticheteCulese) => void;
  onRenunta: () => void;
}) {
  const { lotStrain, lotEticheta, readOnly, onSalveaza, onRenunta } = props;
  const [linie, setLinie] = useState<NirLinieWrite>(() => ({ ...props.linie }));
  const [etichete, setEtichete] = useState<EticheteCulese>({});
  const [aratErori, setAratErori] = useState(false);
  const structurale = eroriStructurale(TIP_LINIE, SCHEMA_LINIE, linie as Record<string, unknown>, CAMPURI);

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
          <div>
            <LookupGrila<NirLinieWrite>
              camp="ProdusId"
              entitate="Produs"
              // Inert pe linia cu lot moștenit: marfa e a LOTULUI, iar serverul
              // nu atinge nici lotul, nici produsul acolo (F5-D3/D4).
              readOnly={lotStrain}
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
            <p className="indiciu">
              {lotStrain
                ? 'Marfa e a lotului moștenit din factură — nu se alege aici.'
                : 'Obligatoriu pe liniile de stoc: din el se naște lotul recepției, la salvare.'}
            </p>
          </div>
          <Lookup<NirLinieWrite>
            camp="TipMaterialId"
            entitate="TipMaterial"
            mod="local"
            afisare={codSiDenumire}
            laSelectie={(t) => setEtichete((prev) => ({
              ...prev, TipMaterialCod: text(t?.Cod), TipMaterialDenumire: text(t?.Denumire),
            }))}
          />
          {/* LOTUL e server-owned (F5-D4): se naște din produs la salvare sau e
              cel moștenit de la factură. Afișat, niciodată cules. */}
          <div>
            <Static membru="LotId" valoare={lotEticheta} />
            <p className="indiciu">
              {lotStrain
                ? 'Lot moștenit din factură — recepția conexă nu-și alege marfa, o preia.'
                : 'Îl creează serverul din produs la salvare; prețul i se fixează la operare.'}
            </p>
          </div>
          <CampNumar<NirLinieWrite> camp="Cantitate" />
          <div>
            <CampNumar<NirLinieWrite> camp="PretUnitar" zecimale={6} readOnly={lotStrain} />
            <p className="indiciu">
              {lotStrain
                ? 'Prețul e al lotului moștenit — valoarea liniei o recalculează serverul din el.'
                : 'Prețul de recepție: cu el se naște lotul, deci operarea îl cere pozitiv.'}
            </p>
          </div>
          <CampData<NirLinieWrite> camp="DataExpirare" />
          <CampText<NirLinieWrite> camp="LotFabricatie" />
        </div>

        {/* Clasificația bugetară e a PROFILULUI (54d): la privat nomenclatoarele
            sunt goale și secțiunea rămâne pliată — de asta e `<details>`, nu o
            grilă de câmpuri mereu deschisă. Pe frunza NIR sunt toate patru
            (DIM-2), ca pe FCT: clona conexă le primește prin contract, iar
            recepția manuală le culege. */}
        <details className="sectiune-pliabila">
          <summary>Clasificație bugetară</summary>
          <div className="grila-campuri">
            <Lookup<NirLinieWrite> camp="CodEconomicId" entitate="CodEconomic" mod="local" afisare={codSiDenumire} />
            <Lookup<NirLinieWrite> camp="SursaFinantareId" entitate="SursaFinantare" mod="local" afisare={codSiDenumire} />
            <Lookup<NirLinieWrite> camp="CodFunctionalId" entitate="CodFunctional" mod="local" afisare={codSiDenumire} />
            <Lookup<NirLinieWrite> camp="ProiectId" entitate="Proiect" mod="local" afisare={codSiDenumire} />
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

// Câmp de AFIȘARE în interiorul formularului: aceeași ramă ca la culegere
// (etichetă din metadata, slot de control), cu text în locul editorului.
// `LotId` nu e în WriteDto, deci `campMeta` îl dă corect NEobligatoriu — nimeni
// nu cere operatorului un câmp pe care serverul îl umple singur.
function Static(props: { membru: string; valoare: ReactNode }) {
  const meta = { ...campMeta(TIP_LINIE, props.membru, SCHEMA_LINIE), obligatoriu: false };
  return (
    <CampShell meta={meta}>
      <div className="valoare-statica">{props.valoare == null || props.valoare === '' ? '—' : props.valoare}</div>
    </CampShell>
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
