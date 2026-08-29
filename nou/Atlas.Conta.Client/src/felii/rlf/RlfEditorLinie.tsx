import { useState } from 'react';
import { Formular, eroriStructurale } from '../../nucleu/formular';
import { CampNumar } from '../../nucleu/campuri';
import { Lookup } from '../../nucleu/Lookup';
import { PanouErori } from '../../nucleu/PanouErori';
import { campMeta } from '../../nucleu/campMeta';
import { etichetaLot } from '../../nucleu/lot';
import { SCHEMA_LINIE, TIP_LINIE, type RlfLinieWrite } from './api';

// Liniile de draft se editează cu VOCABULARUL `Camp*`, nu în grilă (43c): grila
// doar afișează. Editorul nu vorbește cu serverul — CRUD-ul per linie a fost
// respins în designul API (agregatul se trimite întreg).
//
// Linia de retur la furnizor = linia de BCS plus TVA:
//
//  1. **Lotul ORIGINAL** al intrării, nefiltrat (F6-D8, precedentul BTR/BCS/LDI):
//     locația curentă a unui lot e soldul din registru, nu nașterea lui. Din el
//     iese și prețul cu care se evaluează returul — de aceea `Valoare` nu e în
//     WriteDto și n-are editor aici. Tipul se precompletează din produsul
//     lotului (`$expand=Produs` aduce `Produs.TipMaterialId` — un Guid, fără
//     `$expand` imbricat), doar când e gol: nu suprascriem alegerea omului.
//  2. **TVA-ul, cu aceeași semantică pe tot clientul** (o singură regulă, 36a):
//     pe o linie NOUĂ `TipTva` gol = implicitul tipului de document (N21); pe o
//     linie EXISTENTĂ golirea e deliberată și se trimite ca atare (round-trip în
//     `spreWrite`); `ValoareTva` se trimite DOAR dacă operatorul a atins câmpul
//     în sesiunea asta — pe sârmă valoarea înseamnă „override manual", și e
//     tocmai ce cere returul: nota de credit a furnizorului bate rotunjirea
//     noastră. Serverul recalculează TVA-ul când se schimbă DECLANȘATORII bazei
//     (cantitatea sau lotul — prețul e al lotului) ori tipul de TVA; un Salvează
//     care nu le atinge păstrează override-ul.
//
// Ce NU face: nu calculează nicio valoare, nu verifică soldul lotului (refuzul e
// al gardianului motorului) și nu prezice golirea lotului — returul NU absoarbe
// restul de cenți (`IDocumentCuIesireFiscala`, 75a), iar o cifră „prezisă" aici
// ar fi al doilea adevăr al regulii.
const CAMPURI: (keyof RlfLinieWrite & string)[] = ['TipMaterialId', 'Cantitate'];

// Etichetele CULESE la selecție, pentru grila documentului (mecanismul 61b):
// liniile nesalvate n-au încă ReadDto, dar răspunsul OData al selecției e DEJA
// în client — nu se inventează nimic în TS, se reține ce a afișat lookup-ul.
// `Valoare`/`ValoareTva` rămân ale serverului (apar după Salvează).
export type EticheteCulese = {
  TipMaterialCod?: string;
  TipMaterialDenumire?: string;
  LotEticheta?: string;
  TipTvaCod?: string;
};

const text = (v: unknown) => (v == null ? '' : String(v));

export function RlfEditorLinie(props: {
  linie: RlfLinieWrite;
  // Ce a calculat SERVERUL pentru linia asta (ReadDto) — doar pentru afișare.
  valoareTvaCitita?: number | null;
  readOnly: boolean;
  onSalveaza: (l: RlfLinieWrite, etichete: EticheteCulese) => void;
  onRenunta: () => void;
}) {
  const { readOnly, onSalveaza, onRenunta } = props;
  const [linie, setLinie] = useState<RlfLinieWrite>(() => ({
    ...props.linie,
    ValoareTva: props.linie.ValoareTva ?? props.valoareTvaCitita ?? undefined,
  }));
  const [etichete, setEtichete] = useState<EticheteCulese>({});
  // Override-ul de TVA nu e un flag global de formular: e starea UNUI câmp, iar
  // singura sursă de adevăr e „valoarea lui s-a schimbat de când s-a deschis
  // editorul" (același mecanism ca la FCT/FCL).
  const [tvaAtins, setTvaAtins] = useState(props.linie.ValoareTva != null);
  const [aratErori, setAratErori] = useState(false);
  // Stratul 1, structural. `LotId` e nullable pe SCHEMĂ (e nullable pe baza
  // detaliului), dar pe RLF fiecare linie descarcă lotul ORIGINAL — deci cerința
  // se scrie EXPLICIT aici, ca gardul afișat pe câmp și gardul care oprește
  // butonul să spună același lucru (precedentul ASM/LDI).
  const structurale = [
    ...eroriStructurale(TIP_LINIE, SCHEMA_LINIE, linie as Record<string, unknown>, CAMPURI),
    ...cerut('LotId', linie.LotId != null),
  ];

  function schimba(v: RlfLinieWrite) {
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
          <div>
            {/* Lotul e OBLIGATORIU pe fiecare linie de retur (ieșirea e pe lot —
                decizia 13): schema îl dă nullable fiindcă e nullable pe baza
                detaliului, deci cerința se declară aici. */}
            <Lookup<RlfLinieWrite>
              camp="LotId"
              entitate="Lot"
              mod="remote"
              obligatoriu
              expand={['Produs']}
              afisare={etichetaLot}
              cauta="Produs.Denumire"
              sortare="Data"
              laSelectie={(l) => {
                // Tipul se precompletează din PRODUSUL lotului: `$expand=Produs`
                // aduce `Produs.TipMaterialId`, deci valoarea e reală, fără
                // `$expand` imbricat. Aplicarea e UPDATE FUNCȚIONAL pe starea
                // liniei: `seteaza`-ul valorii a rulat deja în același event, iar
                // un patch din closure l-ar fi pierdut. Eticheta Tipului NU se
                // inventează — vine de la server după Salvează (lookup-ul local o
                // rezolvă oricum pe ecran).
                const p = l?.Produs as Record<string, unknown> | null | undefined;
                const tip = p?.TipMaterialId == null ? undefined : String(p.TipMaterialId);
                if (tip)
                  setLinie((prev) => prev.TipMaterialId ? prev : { ...prev, TipMaterialId: tip });
                setEtichete((prev) => ({ ...prev, LotEticheta: l ? etichetaLot(l) : '' }));
              }}
            />
            <p className="indiciu">
              Lotul ORIGINAL al intrării — marfa se întoarce la furnizor pe el, la prețul lui. Lista nu e
              filtrată pe gestiune: locația curentă a lotului e soldul din registru, iar „nu există sold
              aici” e refuz al operării.
            </p>
          </div>
          <Lookup<RlfLinieWrite>
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
            <CampNumar<RlfLinieWrite> camp="Cantitate" />
            <p className="indiciu">
              Cantitatea returnată, POZITIVĂ — cifra de pe nota de credit a furnizorului. Semnul minus îl
              pune operarea.
            </p>
          </div>
          <Lookup<RlfLinieWrite>
            camp="TipTvaId"
            entitate="TipTva"
            mod="local"
            afisare={etichetaTipTva}
            cauta={['Cod', 'Denumire']}
            laSelectie={(t) => setEtichete((prev) => ({ ...prev, TipTvaCod: text(t?.Cod) }))}
          />
          <div>
            <CampNumar<RlfLinieWrite> camp="ValoareTva" zecimale={2} />
            <p className="indiciu">
              {tvaAtins
                ? 'Suprascris manual — se trimite ca atare.'
                : 'Calculat de server din cotă; modificați doar dacă nota de credit a furnizorului diferă.'}
            </p>
          </div>
        </div>

        {/* Nicio secțiune de dimensiuni: RLF folosește detaliul de BAZĂ (F19-D5),
            iar `Angajament`-ul lui face round-trip FĂRĂ editor, ca la NIR/LDI
            (restanța 62g): dacă linia îl are din altă cale (import, ecranul XAF),
            un PUT din client nu i-l șterge. */}
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

// Obligativitatea declarată în FELIE, cu ȘABLONUL de mesaj al nucleului și cu
// caption-ul din metadata: schema dă câmpul nullable, tipul de document îl cere.
function cerut(membru: string, indeplinit: boolean): string[] {
  return indeplinit ? [] : [`„${campMeta(TIP_LINIE, membru, SCHEMA_LINIE).caption}” este obligatoriu.`];
}

// Nomenclatoarele au `DefaultProperty = Denumire`, dar operatorul caută pe cod —
// afișăm amândouă. Fără `Cod`, două tipuri omonime sunt indistinctibile.
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
