import { useState } from 'react';
import { Formular, eroriStructurale } from '../../nucleu/formular';
import { CampData, CampNumar, CampSelectie, CampText } from '../../nucleu/campuri';
import { Lookup } from '../../nucleu/Lookup';
import { LookupGrila } from '../../nucleu/LookupGrila';
import { PanouErori } from '../../nucleu/PanouErori';
import { campMeta } from '../../nucleu/campMeta';
import { etichetaLot } from '../../nucleu/lot';
import { precompleteazaTip } from '../../nucleu/etichete';
import { SCHEMA_LINIE, TIP_LINIE, type AsmLinieWrite } from './api';

// Liniile de draft se editează cu VOCABULARUL `Camp*`, nu în grilă (43c): grila
// doar afișează. Editorul nu vorbește cu serverul — CRUD-ul per linie a fost
// respins în designul API (agregatul se trimite întreg).
//
// Ce e PROPRIU feliei ASM: **direcția e comutatorul liniei**, exact ca pe LDI
// (F6-D3), cu alt vocabular și cu altă gestiune a lotului nou:
//
//  1. **Produs** — marfa care IESE din asamblare: `Produs` e MECANISMUL lotului
//     nou (serverul îl naște la salvare, în gestiunea în care se asamblează —
//     PREDATORUL, F19-D3), iar `PretEvaluare` e prețul cu care intră (operarea
//     îl cere pozitiv). Atributele lotului (dată expirare, lot fabricație) se
//     culeg pe poziție și le copiază motorul pe `Lot` la operare.
//  2. **Consum** — marfa care INTRĂ în asamblare: linia DESCARCĂ un lot
//     existent, la prețul LUI. Tipul se precompletează din produsul lotului
//     (`$expand=Produs` aduce `Produs.TipMaterialId` — un Guid, fără `$expand`
//     imbricat); ETICHETA lui rămâne a serverului, apare după Salvează.
//
// Comutarea de direcție GOLEȘTE client-side câmpurile celeilalte direcții —
// oglinda golirii persistate din `AsamblareApply` („inert devine adevărat, nu
// doar afirmat"). Câmpurile direcției inactive nu se randează deloc: un câmp
// read-only care arată o valoare pe care serverul o aruncă ar fi tot o minciună.
//
// Ce NU face: nu verifică semnul cantității (culegerea e pozitivă prin contract,
// semnarea e a operării — 28a), nu verifică soldul lotului, nu verifică
// invariantul valoric al documentului (46d, la operare — și cu ajutorul comenzii
// „Distribuie valoarea consumului") și nu calculează nicio valoare.
const CAMPURI: (keyof AsmLinieWrite & string)[] = ['TipMaterialId', 'Cantitate'];

// Etichetele CULESE la selecție, pentru grila documentului (mecanismul 61b):
// liniile nesalvate n-au încă ReadDto, dar răspunsul OData al selecției e DEJA
// în client — nu se inventează nimic în TS, se reține ce a afișat lookup-ul.
export type EticheteCulese = {
  TipMaterialCod?: string;
  TipMaterialDenumire?: string;
  ProdusDenumire?: string;
  LotEticheta?: string;
};

const text = (v: unknown) => (v == null ? '' : String(v));

export function AsmEditorLinie(props: {
  linie: AsmLinieWrite;
  readOnly: boolean;
  onSalveaza: (l: AsmLinieWrite, etichete: EticheteCulese) => void;
  onRenunta: () => void;
}) {
  const { readOnly, onSalveaza, onRenunta } = props;
  const [linie, setLinie] = useState<AsmLinieWrite>(() => ({ ...props.linie }));
  const [etichete, setEtichete] = useState<EticheteCulese>({});
  const [aratErori, setAratErori] = useState(false);

  const produs = linie.Directie === 'Produs';
  const consum = linie.Directie === 'Consum';

  // Stratul 1, structural: regulile din schemă + obligativitățile CONDIȚIONATE
  // de direcție. `PretEvaluare` NU e printre ele, deliberat: operarea îl cere
  // pozitiv, dar comanda „Distribuie valoarea consumului" e tocmai calea prin
  // care se completează corect (F19-D4) — iar comanda refuză un document în care
  // unele linii de produs au preț cules și altele nu.
  const structurale = [
    ...eroriStructurale(TIP_LINIE, SCHEMA_LINIE, linie as Record<string, unknown>, CAMPURI),
    ...cerut('Directie', linie.Directie != null && linie.Directie !== ''),
    ...(produs ? cerut('ProdusId', linie.ProdusId != null) : []),
    ...(consum ? cerut('LotId', linie.LotId != null) : []),
  ];

  // Comutarea direcției. Comparația se face AICI, unde `linie` e încă valoarea
  // DINAINTEA schimbării (pattern-ul LDI/FCL): într-un update funcțional
  // direcția nouă ar fi deja în `prev`, iar comparația ar fi mereu falsă.
  function schimba(v: AsmLinieWrite) {
    if (v.Directie !== linie.Directie && v.Directie === 'Consum') {
      // Consumul descarcă un lot existent: câmpurile produsului nu-i aparțin,
      // iar serverul oricum le golește. `LotId` se golește și el (lecția F6-M2):
      // după un produs salvat, ReadDto îl dă = lotul PROPRIU al liniei — comutat
      // pe consum, pinul ar apărea pre-umplut cu lotul „(în culegere)" al liniei
      // însăși, pe care serverul urmează să-l ȘTEARGĂ.
      setEtichete((prev) => ({ ...prev, ProdusDenumire: '', LotEticheta: '' }));
      setLinie({
        ...v,
        ProdusId: null, PretEvaluare: null, DataExpirare: null, LotFabricatie: null,
        LotId: null,
      });
      return;
    }
    if (v.Directie !== linie.Directie && v.Directie === 'Produs') {
      // Pe produs lotul e SERVER-OWNED (se naște din produs): pinul cules pentru
      // consum n-are ce căuta pe el.
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
            {/* Direcția n-are membru 0 în enumerare (F19-D3): o alege
                operatorul, și de ea atârnă restul liniei. `obligatoriu` e
                declarat local — pe sârmă câmpul e nullable (linii istorice de
                tip bază), dar la culegere cerința e reală. */}
            <CampSelectie<AsmLinieWrite> camp="Directie" enumerare="DirectieAsamblare" obligatoriu />
            <p className="indiciu">
              Consum = marfa care intră în asamblare (se descarcă un lot existent);
              produs = marfa care iese (se naște un lot nou, în gestiunea în care se asamblează).
            </p>
          </div>
          <Lookup<AsmLinieWrite>
            camp="TipMaterialId"
            entitate="TipMaterial"
            mod="local"
            afisare={codSiDenumire}
            laSelectie={(t) => setEtichete((prev) => ({
              ...prev, TipMaterialCod: text(t?.Cod), TipMaterialDenumire: text(t?.Denumire),
            }))}
          />
          {/* Cantitatea se culege POZITIVĂ pe ambele direcții — semnul îl pune
              operarea (28a). Pe un document deja operat ReadDto o dă semnată,
              dar atunci ecranul e oricum read-only. */}
          <CampNumar<AsmLinieWrite> camp="Cantitate" />

          {produs && (
            <>
              <div>
                <LookupGrila<AsmLinieWrite>
                  camp="ProdusId"
                  entitate="Produs"
                  obligatoriu
                  afisare={codSiDenumire}
                  laSelectie={(p) => {
                    // ODataStore deserializează Edm.Guid ca OBIECT `Guid`
                    // DevExtreme, nu ca string — `String()` îl aduce la forma de
                    // sârmă.
                    setEtichete((prev) => ({ ...prev, ProdusDenumire: text(p?.Denumire) }));
                    // Tipul + eticheta lui, o singură decizie (F20-D3). Eticheta
                    // vine din cache-ul de nomenclatoare, nu dintr-un
                    // `$expand=TipMaterial` plătit pe fiecare pagină de căutare
                    // într-un nomenclator de sute de mii de produse.
                    precompleteazaTip({
                      linie,
                      tipId: p?.TipMaterialId == null ? undefined : String(p.TipMaterialId),
                      setLinie,
                      setEtichete,
                    });
                  }}
                />
                <p className="indiciu">
                  Din el se naște lotul produsului, la salvare, în gestiunea în care se asamblează (predatorul).
                </p>
              </div>
              <div>
                <CampNumar<AsmLinieWrite> camp="PretEvaluare" zecimale={6} />
                <p className="indiciu">
                  Prețul cu care intră lotul nou — operarea îl cere pozitiv. Butonul „Distribuie valoarea
                  consumului” îl rescrie astfel încât produsele să valoreze exact cât consumurile.
                </p>
              </div>
              <CampData<AsmLinieWrite> camp="DataExpirare" />
              <CampText<AsmLinieWrite> camp="LotFabricatie" />
            </>
          )}

          {consum && (
            <div>
              {/* Lotul CONSUMAT: nefiltrat pe gestiune (F6-D8, precedentul
                  BTR/BCS/LDI) — locația curentă a unui lot e soldul din
                  registru, nu nașterea lui. Refuzul „nu există sold aici" e al
                  gardianului motorului, ca și refuzul consumului dintr-un lot
                  produs de ACELAȘI document (46d). */}
              <LookupGrila<AsmLinieWrite>
                camp="LotId"
                entitate="Lot"
                obligatoriu
                expand={['Produs']}
                afisare={etichetaLot}
                cauta="Produs.Denumire"
                sortare="Data"
                laSelectie={(l) => {
                  // Tipul se precompletează din PRODUSUL lotului: `$expand=Produs`
                  // aduce `Produs.TipMaterialId`, deci valoarea e reală, fără
                  // `$expand` imbricat. Eticheta Tipului vine din cache-ul de
                  // nomenclatoare (F20-D3), în aceeași decizie cu id-ul.
                  const p = l?.Produs as Record<string, unknown> | null | undefined;
                  setEtichete((prev) => ({ ...prev, LotEticheta: l ? etichetaLot(l) : '' }));
                  precompleteazaTip({
                    linie,
                    tipId: p?.TipMaterialId == null ? undefined : String(p.TipMaterialId),
                    setLinie,
                    setEtichete,
                  });
                }}
              />
              <p className="indiciu">
                Lotul consumat — el dă și prețul cu care se evaluează consumul.
              </p>
            </div>
          )}
        </div>

        {/* Nicio secțiune de dimensiuni: frunza ASM n-are `CodEconomic`
            (documentul nu postează — zero `RegulaContare`, 23c), iar
            `Angajament`-ul BAZEI face round-trip FĂRĂ editor, ca la NIR/LDI
            (restanța 62g): dacă linia îl are din altă cale (import, ecranul
            XAF), un PUT din client nu i-l șterge. */}
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
// linie e ori consum, ori produs — niciunul dintre cele două câmpuri nu poate fi
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
