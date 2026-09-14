import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CheckBox, SelectBox } from 'devextreme-react';
import { Popup } from 'devextreme-react/popup';
import { Column, DataGrid, Selection } from 'devextreme-react/data-grid';
import DataSource from 'devextreme/data/data_source';
import { defaultProperty } from '../../nucleu/campMeta';
import { eroriDin } from '../../nucleu/http';
import { storeOData } from '../../nucleu/odata';
import { PanouErori } from '../../nucleu/PanouErori';
import { CasetaPerioada, lunaCurenta } from '../raportare/comune';
import { pif, type LinieSursaCandidata } from './api';

// Tiparul `PopupCandidati` al DVI, cu selecție simplă: o linie de PIF hrănește o singură fișă (F26-D5).

export function PanouLiniiSursa(props: {
  dataDocument?: string | null;
  // Liniile sursă deja folosite pe alte linii ale aceluiași document: serverul
  // nu le știe cât timp documentul e nesalvat.
  exclusi: string[];
  onAlege: (candidat: LinieSursaCandidata) => void;
  onInchide: () => void;
}) {
  const { dataDocument, exclusi, onAlege, onInchide } = props;
  const luna = lunaDocumentului(dataDocument);
  const [perioada, setPerioada] = useState({ dataStart: luna.start, dataEnd: luna.sfarsit });
  const [partenerId, setPartenerId] = useState<string | null>(null);
  const [toate, setToate] = useState(false);
  const [alese, setAlese] = useState<string[]>([]);

  const parteneri = useMemo(() => new DataSource({
    store: storeOData('Partener'),
    sort: defaultProperty('Partener'),
    paginate: true,
    pageSize: 50,
  }), []);
  const afisarePartener = defaultProperty('Partener');

  const candidati = useQuery({
    queryKey: ['pif', 'linii-sursa', perioada.dataStart, perioada.dataEnd, partenerId, toate],
    queryFn: () => pif.liniiSursa({
      dataStart: perioada.dataStart,
      dataEnd: perioada.dataEnd,
      partenerId,
      toate,
    }),
    enabled: !!perioada.dataStart && !!perioada.dataEnd,
  });

  const randuri = (candidati.data?.Candidati ?? [])
    .filter((c) => c.LinieId != null && !exclusi.includes(c.LinieId));

  function preia() {
    const ales = randuri.find((c) => c.LinieId != null && alese.includes(c.LinieId));
    if (ales) onAlege(ales);
  }

  return (
    <Popup
      visible
      title="Linii de factură de clasă F"
      width={900}
      height={620}
      showCloseButton
      onHiding={onInchide}
    >
      <div className="bara-raport">
        <CasetaPerioada
          dataStart={perioada.dataStart}
          dataEnd={perioada.dataEnd}
          seteaza={(v) => { setPerioada(v); setAlese([]); }}
        />
        <label className="bara-raport__camp">
          <span className="camp__eticheta">Furnizorul (opțional)</span>
          <SelectBox
            dataSource={parteneri}
            value={partenerId}
            valueExpr="ID"
            displayExpr={afisarePartener}
            searchEnabled
            searchExpr="Cautare"
            searchTimeout={400}
            showClearButton
            noDataText="Nimic găsit"
            width={280}
            placeholder="Toți furnizorii"
            onValueChanged={(e) => {
              if (!e.event) return;
              setPartenerId((e.value as string) ?? null);
              setAlese([]);
            }}
          />
        </label>
        <label className="bara-raport__camp">
          <span className="camp__eticheta">Și liniile consumate integral</span>
          <CheckBox
            value={toate}
            onValueChanged={(e) => { if (e.event) { setToate(e.value === true); setAlese([]); } }}
          />
        </label>
      </div>

      <PanouErori
        erori={candidati.error ? eroriDin(candidati.error) : []}
        titlu="Candidații nu s-au putut citi"
      />

      {candidati.data?.MaiSunt && (
        <p className="panou panou--atentie">
          Lista e trunchiată la 500 de linii — îngustați perioada ca să le vedeți pe toate.
        </p>
      )}

      <DataGrid
        dataSource={randuri}
        keyExpr="LinieId"
        showBorders
        columnAutoWidth
        height={380}
        selectedRowKeys={alese}
        onSelectionChanged={(e) => setAlese(e.selectedRowKeys as string[])}
      >
        <Selection mode="single" />
        <Column dataField="Numar" caption="Număr" />
        <Column dataField="Data" caption="Dată" dataType="date" format="dd.MM.yyyy" />
        <Column dataField="PartenerDenumire" caption="Furnizor" />
        <Column dataField="TipMaterialCod" caption="Cod tip" width={110} />
        <Column dataField="Descriere" caption="Descriere" />
        <Column dataField="Cantitate" caption="Cantitate" dataType="number" format="#,##0.###" alignment="right" width={110} />
        <Column dataField="Valoare" caption="Valoare" dataType="number" format="#,##0.00" alignment="right" />
        <Column dataField="Consumat" caption="Consumat" dataType="number" format="#,##0.00" alignment="right" />
        <Column dataField="Rest" caption="Rest" dataType="number" format="#,##0.00" alignment="right" />
      </DataGrid>

      <div className="editor-linie__comenzi">
        <button
          type="button"
          className="buton buton--primar"
          disabled={alese.length === 0}
          onClick={preia}
        >
          Preia linia
        </button>
        <button type="button" className="buton" onClick={onInchide}>Renunță</button>
      </div>

      <p className="indiciu">
        Implicit se propun doar liniile de clasă F ale facturilor operate care mai au rest, în perioada aleasă.
        Plafonul e pe VALOARE, nu pe cantitate: o linie cu cantitatea 3 poate hrăni trei fișe.
      </p>
    </Popup>
  );
}

// Perioada implicită a dialogului: luna DOCUMENTULUI, nu luna curentă.
function lunaDocumentului(data?: string | null): { start: string; sfarsit: string } {
  const bucati = (data ?? '').split('-');
  const an = Number(bucati[0]);
  const luna = Number(bucati[1]);
  if (!an || !luna)
    return lunaCurenta();
  const ultima = new Date(an, luna, 0).getDate();
  const doua = (n: number) => `${n}`.padStart(2, '0');
  return { start: `${an}-${doua(luna)}-01`, sfarsit: `${an}-${doua(luna)}-${doua(ultima)}` };
}
