import { useState } from 'react';
import { Formular, eroriStructurale } from '../../nucleu/formular';
import { CampData, CampNumar, CampSelectie, CampText } from '../../nucleu/campuri';
import { Lookup } from '../../nucleu/Lookup';
import { PanouErori } from '../../nucleu/PanouErori';
import { campMeta } from '../../nucleu/campMeta';
import { etichetaLot } from '../../nucleu/lot';
import { SCHEMA_LINIE, TIP_LINIE, type LdiLinieWrite } from './api';

// Liniile de draft se editează cu VOCABULARUL `Camp*`, nu în grilă (43c): grila
// doar afișează. Editorul nu vorbește cu serverul — CRUD-ul per linie a fost
// respins în designul API (agregatul se trimite întreg).
//
// Ce e PROPRIU feliei LDI: **direcția e comutatorul liniei**. Un plus și un
// minus n-au aceleași câmpuri, iar serverul aplică două contracte diferite pe
// aceeași frunză (F6-D3):
//
//  1. **Plus** — se constată marfă negăsită în evidență: `Produs` e MECANISMUL
//     lotului nou (serverul îl naște la salvare, în gestiunea INVENTARIATĂ —
//     predatorul, F6-D2), iar `PretEvaluare` e prețul cu care intră (operarea îl
//     cere pozitiv — 28e). Atributele de lot (dată expirare, lot fabricație) se
//     culeg pe poziție.
//  2. **Minus** — se constată lipsă: linia DESCARCĂ un lot existent, la prețul
//     LUI. Tipul se alege manual, nu se precompletează din lot: ar cere un
//     `$expand` imbricat (Lot → Produs → TipMaterial) pe care nu-l facem.
//
// Comutarea de direcție GOLEȘTE client-side câmpurile celeilalte direcții —
// oglinda golirii persistate din `ListaDiferenteInventarApply` (lecția F5:
// „inert devine adevărat, nu doar afirmat"). Câmpurile direcției inactive nu se
// randează deloc: un câmp read-only care arată o valoare pe care serverul o
// aruncă ar fi tot o minciună.
//
// Ce NU face: nu verifică semnul cantității (culegerea e pozitivă prin contract,
// semnarea e a operării — 28a), nu verifică soldul lotului și nu calculează
// nicio valoare.
const CAMPURI: (keyof LdiLinieWrite & string)[] = ['TipMaterialId', 'Cantitate'];

// Etichetele CULESE la selecție, pentru grila documentului (mecanismul 61b):
// liniile nesalvate n-au încă ReadDto, dar răspunsul OData al selecției e DEJA
// în client — nu se inventează nimic în TS, se reține ce a afișat lookup-ul.
// `Valoare` rămâne a serverului (apare după Salvează).
export type EticheteCulese = {
  TipMaterialCod?: string;
  TipMaterialDenumire?: string;
  ProdusDenumire?: string;
  LotEticheta?: string;
};

const text = (v: unknown) => (v == null ? '' : String(v));

export function LdiEditorLinie(props: {
  linie: LdiLinieWrite;
  readOnly: boolean;
  onSalveaza: (l: LdiLinieWrite, etichete: EticheteCulese) => void;
  onRenunta: () => void;
}) {
  const { readOnly, onSalveaza, onRenunta } = props;
  const [linie, setLinie] = useState<LdiLinieWrite>(() => ({ ...props.linie }));
  const [etichete, setEtichete] = useState<EticheteCulese>({});
  const [aratErori, setAratErori] = useState(false);

  const plus = linie.Directie === 'Plus';
  const minus = linie.Directie === 'Minus';

  // Stratul 1, structural: regulile din schemă + cele DOUĂ obligativități
  // condiționate de direcție. Șablonul mesajului rămâne al nucleului (43a) —
  // se interpolează doar caption-ul, ca peste tot.
  const structurale = [
    ...eroriStructurale(TIP_LINIE, SCHEMA_LINIE, linie as Record<string, unknown>, CAMPURI),
    ...cerut('Directie', linie.Directie != null && linie.Directie !== ''),
    ...(plus ? cerut('ProdusId', linie.ProdusId != null) : []),
    ...(minus ? cerut('LotId', linie.LotId != null) : []),
  ];

  // Comutarea direcției. Comparația se face AICI, unde `linie` e încă valoarea
  // DINAINTEA schimbării (pattern-ul pinului FCL): într-un update funcțional
  // direcția nouă ar fi deja în `prev`, iar comparația ar fi mereu falsă.
  function schimba(v: LdiLinieWrite) {
    if (v.Directie !== linie.Directie && v.Directie === 'Minus') {
      // Minusul descarcă un lot existent: câmpurile plusului nu-i aparțin, iar
      // serverul oricum le golește (F6-D3). Le golim și aici, ca payload-ul să
      // spună de la început același lucru.
      setEtichete((prev) => ({ ...prev, ProdusDenumire: '' }));
      setLinie({ ...v, ProdusId: null, PretEvaluare: null, DataExpirare: null, LotFabricatie: null });
      return;
    }
    if (v.Directie !== linie.Directie && v.Directie === 'Plus') {
      // Pe plus lotul e SERVER-OWNED (se naște din produs): pinul cules pentru
      // minus n-are ce căuta pe el.
      setEtichete((prev) => ({ ...prev, LotEticheta: '' }));
      setLinie({ ...v, LotId: null });
      return;
    }
    setLinie(v);
  }

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
        onSchimba={schimba}
        readOnly={readOnly}
        aratErori={aratErori}
      >
        <div className="grila-campuri">
          <div>
            {/* Direcția n-are default valid (28e): o alege operatorul, și de ea
                atârnă restul liniei. `obligatoriu` e declarat local — pe sârmă
                câmpul e nullable (linii istorice de tip bază), dar la culegere
                cerința e reală. */}
            <CampSelectie<LdiLinieWrite> camp="Directie" enumerare="DirectieDiferenta" obligatoriu />
            <p className="indiciu">
              Plus = marfă găsită în plus (se naște un lot nou); minus = lipsă (se descarcă un lot existent).
            </p>
          </div>
          <Lookup<LdiLinieWrite>
            camp="TipMaterialId"
            entitate="TipMaterial"
            mod="local"
            afisare={codSiDenumire}
            cauta={['Cod', 'Denumire']}
            laSelectie={(t) => setEtichete((prev) => ({
              ...prev, TipMaterialCod: text(t?.Cod), TipMaterialDenumire: text(t?.Denumire),
            }))}
          />
          {/* Cantitatea se culege POZITIVĂ pe ambele direcții — semnul îl pune
              operarea (28a). Pe un document deja operat ReadDto o dă semnată,
              dar atunci ecranul e oricum read-only. */}
          <CampNumar<LdiLinieWrite> camp="Cantitate" />

          {plus && (
            <>
              <div>
                <Lookup<LdiLinieWrite>
                  camp="ProdusId"
                  entitate="Produs"
                  mod="remote"
                  obligatoriu
                  afisare={codSiDenumire}
                  cauta={['Cod', 'Denumire']}
                  expand={['TipMaterial']}
                  laSelectie={(p) => {
                    // ODataStore deserializează Edm.Guid ca OBIECT `Guid`
                    // DevExtreme, nu ca string — `String()` îl aduce la forma de
                    // sârmă. Aplicarea e UPDATE FUNCȚIONAL pe starea liniei:
                    // `seteaza`-ul valorii a rulat deja în același event, iar un
                    // patch din closure l-ar fi pierdut.
                    const tip = p?.TipMaterialId == null ? undefined : String(p.TipMaterialId);
                    if (tip)
                      setLinie((prev) => prev.TipMaterialId ? prev : { ...prev, TipMaterialId: tip });
                    // Eticheta Tipului precompletat vine din `$expand=TipMaterial`
                    // al aceleiași selecții; se reține doar când precompletarea
                    // chiar se aplică.
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
                  Din el se naște lotul plusului, la salvare, în gestiunea inventariată.
                </p>
              </div>
              <div>
                <CampNumar<LdiLinieWrite> camp="PretEvaluare" zecimale={6} />
                <p className="indiciu">
                  Prețul cu care intră lotul nou — operarea îl cere pozitiv.
                </p>
              </div>
              <CampData<LdiLinieWrite> camp="DataExpirare" />
              <CampText<LdiLinieWrite> camp="LotFabricatie" />
            </>
          )}

          {minus && (
            <div>
              {/* Lotul DESCĂRCAT: nefiltrat pe gestiune (F6-D8, precedentul
                  BTR/BCS) — locația curentă a unui lot e soldul din registru, nu
                  nașterea lui. Refuzul „nu există sold aici" e al gardianului
                  motorului. Tipul NU se precompletează din lot: ar cere un
                  `$expand` imbricat, deci îl alege operatorul. */}
              <Lookup<LdiLinieWrite>
                camp="LotId"
                entitate="Lot"
                mod="remote"
                obligatoriu
                expand={['Produs']}
                afisare={etichetaLot}
                cauta="Produs.Denumire"
                sortare="Data"
                laSelectie={(l) => setEtichete((prev) => ({ ...prev, LotEticheta: l ? etichetaLot(l) : '' }))}
              />
              <p className="indiciu">
                Lotul lipsă — el dă și prețul cu care se evaluează minusul.
              </p>
            </div>
          )}
        </div>

        {/* Clasificația bugetară e a PROFILULUI (54d): la privat nomenclatorul e
            gol și secțiunea rămâne pliată. Pe frunza LDI dimensiunea culeasă e
            una singură — `CodEconomic` (DIM-2) — și se aplică pe AMBELE direcții
            (contarea minusului 6xx = 3xx o poate cere la fel ca plusul). */}
        <details className="sectiune-pliabila">
          <summary>Clasificație bugetară</summary>
          <div className="grila-campuri">
            <Lookup<LdiLinieWrite> camp="CodEconomicId" entitate="CodEconomic" mod="local" afisare={codSiDenumire} cauta={['Cod', 'Denumire']} />
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

// Obligativitatea CONDIȚIONATĂ de direcție: schema OpenAPI le dă nullable (o
// linie e ori plus, ori minus — niciunul dintre cele două câmpuri nu poate fi
// obligatoriu structural), deci cerința se scrie aici, explicit, cu ȘABLONUL
// mesajului din nucleu și cu caption-ul din metadata.
function cerut(membru: string, indeplinit: boolean): string[] {
  return indeplinit ? [] : [`„${campMeta(TIP_LINIE, membru, SCHEMA_LINIE).caption}” este obligatoriu.`];
}

// Nomenclatoarele au `DefaultProperty = Denumire`, dar operatorul caută pe cod —
// afișăm amândouă. Fără `Cod`, două produse omonime sunt indistinctibile.
function codSiDenumire(element: Record<string, unknown>): string {
  if (!element) return '';
  const cod = element.Cod == null ? '' : String(element.Cod);
  const denumire = element.Denumire == null ? '' : String(element.Denumire);
  return cod && denumire ? `${cod} — ${denumire}` : cod || denumire;
}
