import { useState } from 'react';
import { Formular, eroriStructurale } from '../../nucleu/formular';
import { CampNumar, CampText } from '../../nucleu/campuri';
import { Lookup } from '../../nucleu/Lookup';
import { PanouErori } from '../../nucleu/PanouErori';
import { SCHEMA_LINIE, TIP_LINIE, type NtcLinieWrite } from './api';

// Liniile de draft se editează cu VOCABULARUL `Camp*`, nu în grilă (43c): grila
// doar afișează. Editorul nu vorbește cu serverul — CRUD-ul per linie a fost
// respins în designul API (agregatul se trimite întreg).
//
// Ce e PROPRIU feliei NTC: **linia ESTE postarea**. Pe orice alt tip conturile
// se rezolvă declarativ (`SursaCont`, 26b) și linia poartă doar ce se culege;
// aici cele patru FK-uri ale postării explicite (32a) SUNT documentul, iar
// `Valoare` e culeasă direct, fără lanț de valori (F19-D8):
//
//  * **Conturile** sunt obligatorii — nu structural (schema le lasă nullable, ca
//    draftul să aibă voie să fie incomplet), ci prin TIP, la operare. Se
//    marchează local (escapa `obligatoriu`, ca `Numar` pe FCT): cerința e
//    aceeași, arătată mai devreme. Filtrul `Sumator = false` e afordanță, nu
//    regulă — pe un cont sintetic postarea refuză oricum.
//  * **Repartitorii** per latură sunt OPȚIONALI și se caută pe BAZA `Repartitor`
//    (F19-D12, ca la DEC): postarea explicită acceptă orice repartitor, iar o
//    listă pe o singură derivată ar minți prin omisiune. Ce nu se culege cade pe
//    default-ul polimorf al header-ului (32c).
//  * **`Valoare` poate fi NEGATIVĂ** (note storno). Editorul nu-i verifică
//    semnul și nu refuză zero: zero îl refuză tipul la operare, iar un al doilea
//    gard aici ar fi a doua sursă a aceleiași reguli (42a).
//  * **`Tip (cont/clasă)`** e obligatoriu structural, dar fără rol semantic pe
//    notă: baza îl cere NOT NULL pentru toate celelalte tipuri. Se culege ca pe
//    orice linie — API-ul nu inventează un default (ar fi un simbol de profil
//    hardcodat în afara politicilor, 29).
//
// Ce NU face: nu verifică corespondența conturilor (nota contabilă manuală ESTE
// libertatea de a pune orice cont în corespondență cu oricare altul — 48b/46b),
// nu verifică dimensiunile obligatorii ale contului (33a, la operare) și nu
// calculează nicio valoare.
const CAMPURI: (keyof NtcLinieWrite & string)[] = ['TipMaterialId', 'Valoare'];

// Filtrul de afordanță al conturilor: se postează pe FRUNZE, nu pe sintetice
// sumatoare (precedentul DEC).
const FILTRU_CONT_FRUNZA = ['Sumator', '=', false];

// Caption-urile celor patru FK-uri ale postării ies azi BRUTE în dump
// („ContDebit", „RepartitorCredit"…): frunza n-are `[XafDisplayName]` pe ele.
// Felia le numește în vocabularul ei (escapa `eticheta`, ca „Gestiunea
// inventariată" pe LDI), într-un singur loc — lookup-ul și mesajul de eroare
// spun același cuvânt. Fixul de fond e în Module + regenerarea dump-ului.
const ETICHETA_CONT_DEBIT = 'Cont debitor';
const ETICHETA_CONT_CREDIT = 'Cont creditor';

// Etichetele CULESE la selecție, pentru grila documentului (mecanismul 61b):
// liniile nesalvate n-au încă ReadDto, dar răspunsul OData al selecției e DEJA
// în client — nu se inventează nimic în TS, se reține ce a afișat lookup-ul.
export type EticheteCulese = {
  TipMaterialCod?: string;
  TipMaterialDenumire?: string;
  ContDebitSimbol?: string;
  ContCreditSimbol?: string;
  RepartitorDebitDenumire?: string;
  RepartitorCreditDenumire?: string;
  CodEconomicCod?: string;
};

const text = (v: unknown) => (v == null ? '' : String(v));

export function NtcEditorLinie(props: {
  linie: NtcLinieWrite;
  readOnly: boolean;
  onSalveaza: (l: NtcLinieWrite, etichete: EticheteCulese) => void;
  onRenunta: () => void;
}) {
  const { readOnly, onSalveaza, onRenunta } = props;
  const [linie, setLinie] = useState<NtcLinieWrite>(() => ({ ...props.linie }));
  const [etichete, setEtichete] = useState<EticheteCulese>({});
  const [aratErori, setAratErori] = useState(false);

  // Stratul 1, structural: regulile din schemă + cele două conturi, cerute de
  // TIP la operare. Șablonul mesajului rămâne al nucleului (43a) — se
  // interpolează doar caption-ul.
  const structurale = [
    ...eroriStructurale(TIP_LINIE, SCHEMA_LINIE, linie as Record<string, unknown>, CAMPURI),
    ...cerut(ETICHETA_CONT_DEBIT, linie.ContDebitId != null),
    ...cerut(ETICHETA_CONT_CREDIT, linie.ContCreditId != null),
  ];

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
          {/* Explicația operațiunii — pe notă e singura descriere a ce s-a
              întâmplat (nu există produs, lot sau cantitate care s-o spună). */}
          <CampText<NtcLinieWrite> camp="Descriere" />
          <Lookup<NtcLinieWrite>
            camp="TipMaterialId"
            entitate="TipMaterial"
            mod="local"
            afisare={codSiDenumire}
            cauta={['Cod', 'Denumire']}
            laSelectie={(t) => setEtichete((prev) => ({
              ...prev, TipMaterialCod: text(t?.Cod), TipMaterialDenumire: text(t?.Denumire),
            }))}
          />
          <div>
            {/* Culeasă DIRECT, cu semn: negativul e legitim (nota storno).
                Zecimalele sunt cele ale banilor (49e). */}
            <CampNumar<NtcLinieWrite> camp="Valoare" zecimale={2} />
            <p className="indiciu">
              Se postează exact cât se scrie aici. Valoarea negativă e permisă (notă de stornare);
              zero îl refuză operarea.
            </p>
          </div>
        </div>

        {/* Postarea explicită — pe NTC nu e o secțiune pliabilă ca pe DEC, ci
            CORPUL liniei: fără ea nota n-are ce posta (nu există regulă de
            contare pe tip, în niciun profil). */}
        <div className="grila-campuri">
          <Lookup<NtcLinieWrite>
            camp="ContDebitId"
            entitate="Cont"
            mod="remote"
            obligatoriu
            eticheta={ETICHETA_CONT_DEBIT}
            afisare={etichetaCont}
            cauta={['Simbol', 'Denumire']}
            filtru={FILTRU_CONT_FRUNZA}
            laSelectie={(c) => setEtichete((prev) => ({ ...prev, ContDebitSimbol: text(c?.Simbol) }))}
          />
          <Lookup<NtcLinieWrite>
            camp="ContCreditId"
            entitate="Cont"
            mod="remote"
            obligatoriu
            eticheta={ETICHETA_CONT_CREDIT}
            afisare={etichetaCont}
            cauta={['Simbol', 'Denumire']}
            filtru={FILTRU_CONT_FRUNZA}
            laSelectie={(c) => setEtichete((prev) => ({ ...prev, ContCreditSimbol: text(c?.Simbol) }))}
          />
          <div>
            {/* Repartitorul de pe DEBIT e și contrapartida pe care nota poate
                stinge DATORII (F19-D16): jumătatea de plafon a compensării se
                naște exact din câmpul ăsta. */}
            <Lookup<NtcLinieWrite>
              camp="RepartitorDebitId"
              entitate="Repartitor"
              mod="remote"
              eticheta="Repartitor debit"
              afisare={codSiDenumire}
              cauta={['Denumire', 'Cod']}
              laSelectie={(r) => setEtichete((prev) => ({ ...prev, RepartitorDebitDenumire: text(r?.Denumire) }))}
            />
            <p className="indiciu">
              Pe o notă de compensare, partenerul de aici dă jumătatea de plafon „datorie”.
            </p>
          </div>
          <div>
            <Lookup<NtcLinieWrite>
              camp="RepartitorCreditId"
              entitate="Repartitor"
              mod="remote"
              eticheta="Repartitor credit"
              afisare={codSiDenumire}
              cauta={['Denumire', 'Cod']}
              laSelectie={(r) => setEtichete((prev) => ({ ...prev, RepartitorCreditDenumire: text(r?.Denumire) }))}
            />
            <p className="indiciu">
              …iar cel de aici, jumătatea „creanță”. Fără repartitor, latura cade pe implicitul documentului.
            </p>
          </div>
        </div>

        {/* Clasificația bugetară e a PROFILULUI (54d): la privat nomenclatorul e
            gol și secțiunea rămâne pliată. Pe frunza NTC dimensiunea culeasă e
            una singură — `CodEconomic` (DIM-2) —, cerută de conturile cu
            defalcare E (33a). */}
        <details className="sectiune-pliabila">
          <summary>Clasificație bugetară</summary>
          <div className="grila-campuri">
            <Lookup<NtcLinieWrite>
              camp="CodEconomicId"
              entitate="CodEconomic"
              mod="local"
              afisare={codSiDenumire}
              cauta={['Cod', 'Denumire']}
              laSelectie={(c) => setEtichete((prev) => ({ ...prev, CodEconomicCod: text(c?.Cod) }))}
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

// Cerința pe care schema n-o poartă (conturile sunt nullable pe sârmă — draftul
// are voie să fie incomplet), scrisă explicit cu ȘABLONUL mesajului din nucleu.
// Numele e cel al feliei, același cu eticheta lookup-ului de mai sus, fiindcă
// frunza n-are încă `[XafDisplayName]` pe cele patru FK-uri ale postării.
function cerut(eticheta: string, indeplinit: boolean): string[] {
  return indeplinit ? [] : [`„${eticheta}” este obligatoriu.`];
}

// Nomenclatoarele au `DefaultProperty = Denumire`, dar operatorul caută pe cod —
// afișăm amândouă.
function codSiDenumire(element: Record<string, unknown>): string {
  if (!element) return '';
  const cod = element.Cod == null ? '' : String(element.Cod);
  const denumire = element.Denumire == null ? '' : String(element.Denumire);
  return cod && denumire ? `${cod} — ${denumire}` : cod || denumire;
}

function etichetaCont(element: Record<string, unknown>): string {
  if (!element) return '';
  const simbol = element.Simbol == null ? '' : String(element.Simbol);
  const denumire = element.Denumire == null ? '' : String(element.Denumire);
  return simbol && denumire ? `${simbol} — ${denumire}` : simbol || denumire;
}
