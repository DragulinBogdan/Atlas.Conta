import { useEffect, useMemo, useRef, useState } from 'react';
import { DropDownBox, type DropDownBoxRef } from 'devextreme-react/drop-down-box';
import { TextBox, type TextBoxRef } from 'devextreme-react/text-box';
import { Column, DataGrid, Scrolling, Sorting, type DataGridRef } from 'devextreme-react/data-grid';
import DataSource from 'devextreme/data/data_source';
import { CampShell } from './CampShell';
import { defaultProperty } from './campMeta';
import { useCamp } from './formular';
import { CAMP_CAUTARE, areCautare, storeOData } from './odata';
import { tipuriGuid, type ElementNomenclator, type PropsLookup } from './Lookup';

// `LookupGrila` = fratele lui `Lookup` pentru nomenclatoarele unde UN rând de
// text nu poate purta identitatea: doi „Ștefan SRL" se despart doar cu
// CodFiscal vizibil, iar un lot nu se alege bine fără dată și preț. Câmpul e un
// `DropDownBox`, panoul e un `DataGrid` cu coloane multiple — pe ACEEAȘI
// conductă (`storeOData`: JWT, 401, normalizarea `Cautare`, `byKey` prin
// cache-ul comun). Mereu remote — un nomenclator mic n-are nevoie de grilă,
// rămâne pe `Lookup`.
//
// ═══ Disciplina `e.event`, rezolvată STRUCTURAL ═══
// La `SelectBox`, „doar acțiunile omului se propagă" se citește din `e.event`
// (56e — bug-ul de la smoke F2). Aici selecția vine din grila interioară, al
// cărei `onSelectionChanged` NU distinge omul de sincronizarea programatică —
// de aceea grila nu are selecție deloc: propagarea stă pe `onRowClick`, care e
// prin construcție o acțiune a omului, iar valoarea programatică (formularul a
// pus câmpul) nu atinge grila niciodată. `onValueChanged` al casetei rămâne cu
// gardul clasic pe `e.event` și tratează un singur caz: golirea din butonul ×.
//
// ═══ Căutarea: în panou, nu în câmp ═══
// Câmpul tastabil (`acceptCustomValue`) ar face din textul liber o „valoare"
// care trebuie apoi dezamorsată la fiecare blur/close (tiparul din docs cere
// resetări în `onClosed` + `onInput` + `onOpened`). Caseta de căutare din panou,
// focalizată la deschidere, dă același flux de tastare (click → scrii) fără
// niciun caz de dezamorsat: termenul intră în `DataSource.searchValue` pe
// `searchExpr`-ul de `Cautare`, deci trece prin aceeași normalizare fără
// diacritice din `beforeSend` (F20-D1). Enter alege primul rând vizibil.

export type ColoanaLookup = {
  camp: string;
  eticheta: string;
  latime?: number;
  fel?: 'text' | 'data' | 'numar';
};

// Coloanele per entitate — IDENTITATEA lor e cod, într-un singur loc (43a), ca
// toate ecranele să arate același partener la fel. Apelantul le suprascrie doar
// când are un motiv al feliei.
const PRESETURI: Record<string, ColoanaLookup[]> = {
  Partener: [
    { camp: 'Cod', eticheta: 'Cod', latime: 100 },
    { camp: 'Denumire', eticheta: 'Denumire' },
    { camp: 'CodFiscal', eticheta: 'Cod fiscal', latime: 110 },
    { camp: 'Localitate', eticheta: 'Localitate', latime: 130 },
  ],
  Produs: [
    { camp: 'Cod', eticheta: 'Cod', latime: 110 },
    { camp: 'Denumire', eticheta: 'Denumire' },
    { camp: 'UM', eticheta: 'UM', latime: 70 },
  ],
  // Coloanele de navigație cer `expand={['Produs']}` — pe care toate siturile de
  // lot îl au deja pentru etichetă și precompletare.
  Lot: [
    { camp: 'Produs.Denumire', eticheta: 'Produs' },
    { camp: 'Data', eticheta: 'Data', fel: 'data', latime: 95 },
    { camp: 'PretUnitar', eticheta: 'Preț unitar', fel: 'numar', latime: 105 },
    { camp: 'LotFabricatie', eticheta: 'Lot fabricație', latime: 110 },
  ],
};

export type PropsLookupGrila<T extends object> = Omit<PropsLookup<T>, 'mod'> & {
  coloane?: ColoanaLookup[];
};

export function LookupGrila<T extends object>(props: PropsLookupGrila<T>) {
  const { camp, readOnly, obligatoriu, eticheta, entitate, afisare, cauta, expand, sortare, filtru, laSelectie, coloane } = props;
  const c = useCamp<string>(camp, readOnly, obligatoriu, eticheta);
  const proprietateAfisare = defaultProperty(entitate);
  const campCautare = cauta ?? (areCautare(entitate) ? CAMP_CAUTARE : proprietateAfisare);
  const coloaneEfective = coloane ?? PRESETURI[entitate] ?? [{ camp: proprietateAfisare, eticheta: 'Denumire' }];

  const casetaRef = useRef<DropDownBoxRef>(null);
  const cautareRef = useRef<TextBoxRef>(null);
  const grilaRef = useRef<DataGridRef>(null);

  // Bufferul termenului de căutare (lecția 69h: widget tastabil legat de un
  // store asincron cere buffer local) + debounce spre `searchValue`.
  const [termen, setTermen] = useState('');

  // Aceeași disciplină de identitate ca în `Lookup`: array-urile scrise inline
  // în JSX intră prin cheia de CONȚINUT, altfel sursa s-ar reconstrui la fiecare
  // randare și grila s-ar reîncărca sub degetele operatorului.
  const cheieExpand = JSON.stringify(expand ?? null);
  const cheieFiltru = JSON.stringify(filtru ?? null);
  const cheieCauta = JSON.stringify(campCautare);

  const sursa = useMemo(() => new DataSource({
    store: storeOData(entitate, { fieldTypes: tipuriGuid(filtru) }),
    expand,
    filter: filtru,
    searchExpr: campCautare,
    sort: sortare ?? proprietateAfisare,
    paginate: true,
    pageSize: 50,
    // eslint-disable-next-line react-hooks/exhaustive-deps -- `expand`/`filtru`/`cauta` intră prin cheile de conținut.
  }), [entitate, proprietateAfisare, cheieExpand, sortare, cheieFiltru, cheieCauta]);

  useEffect(() => {
    const ceas = setTimeout(() => {
      const activ = (sursa.searchValue() as string | null) ?? null;
      const dorit = termen || null;
      if (activ === dorit) return;
      sursa.searchValue(dorit);
      void sursa.load();
    }, 350);
    return () => clearTimeout(ceas);
  }, [termen, sursa]);

  // Alegerea unui element — SINGURA cale prin care grila scrie formularul.
  const alege = (element: ElementNomenclator | undefined) => {
    if (element) {
      const valoare = String(element.ID);
      if (valoare !== String(c.valoare ?? '')) {
        c.seteaza(valoare);
        if (laSelectie)
          laSelectie(element);
      }
    }
    casetaRef.current?.instance().close();
  };

  return (
    <CampShell meta={c.meta} eroare={c.eroare}>
      <DropDownBox
        ref={casetaRef}
        dataSource={sursa}
        value={c.valoare ?? null}
        readOnly={c.readOnly}
        valueExpr="ID"
        displayExpr={afisare ?? proprietateAfisare}
        showClearButton={!c.meta.obligatoriu}
        dropDownOptions={OPTIUNI_PANOU}
        onOpened={() => {
          // Panoul se randează amânat (`deferRendering`), deci focusul se cere
          // după cadrul în care s-a montat caseta de căutare.
          requestAnimationFrame(() => cautareRef.current?.instance().focus());
        }}
        onClosed={() => {
          // Următoarea deschidere pornește de la lista ÎNTREAGĂ — un filtru
          // rămas dintr-o căutare veche ar arăta „nomenclatorul" ca fiind de
          // trei rânduri.
          setTermen('');
          if (sursa.searchValue() != null) {
            sursa.searchValue(null);
            void sursa.load();
          }
        }}
        onValueChanged={(e) => {
          // Doar golirea din butonul × al operatorului: orice altă valoare
          // intră prin `alege`, iar schimbările programatice (formularul a pus
          // câmpul) nu se propagă înapoi — gardul clasic pe `e.event` (56e).
          if (!e.event || e.value != null)
            return;
          c.seteaza(undefined);
          if (laSelectie)
            laSelectie(null);
        }}
        contentRender={() => (
          <div className="lookup-grila">
            <TextBox
              ref={cautareRef}
              value={termen}
              placeholder="Caută…"
              showClearButton
              valueChangeEvent="input"
              onValueChanged={(e) => setTermen((e.value as string) ?? '')}
              onEnterKey={() => alege(grilaRef.current?.instance().getVisibleRows()[0]?.data as ElementNomenclator | undefined)}
            />
            <DataGrid
              ref={grilaRef}
              dataSource={sursa}
              remoteOperations
              showBorders
              columnAutoWidth
              hoverStateEnabled
              height={320}
              onRowClick={(e) => alege(e.data as ElementNomenclator)}
            >
              <Sorting mode="single" />
              <Scrolling mode="virtual" />
              {coloaneEfective.map((col) => (
                <Column
                  key={col.camp}
                  dataField={col.camp}
                  caption={col.eticheta}
                  width={col.latime}
                  dataType={col.fel === 'data' ? 'date' : col.fel === 'numar' ? 'number' : undefined}
                  format={col.fel === 'data' ? 'dd.MM.yyyy' : col.fel === 'numar' ? '#,##0.00##' : undefined}
                  alignment={col.fel === 'numar' ? 'right' : undefined}
                  // Proprietățile de navigație (`Produs.Denumire`) nu se sortează
                  // pe server — un buton care tace ar minți (tiparul din Produse).
                  allowSorting={!col.camp.includes('.')}
                />
              ))}
            </DataGrid>
          </div>
        )}
      />
    </CampShell>
  );
}

// Constantă de modul, nu literal în JSX: un obiect nou per randare ar
// reconfigura popup-ul degeaba.
const OPTIUNI_PANOU = { width: 640 };
