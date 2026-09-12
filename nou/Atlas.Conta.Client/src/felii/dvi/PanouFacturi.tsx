import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CheckBox, SelectBox } from 'devextreme-react';
import { Popup } from 'devextreme-react/popup';
import { Column, DataGrid, Selection } from 'devextreme-react/data-grid';
import DataSource from 'devextreme/data/data_source';
import { defaultProperty, labelEnum } from '../../nucleu/campMeta';
import { eroriDin } from '../../nucleu/http';
import { storeOData } from '../../nucleu/odata';
import { PanouErori } from '../../nucleu/PanouErori';
import { CasetaPerioada, lunaCurenta } from '../raportare/comune';
import { dvi, type DviFactura, type FacturaCandidata } from './api';

// Panoul „Facturi de import" (DVI-D3/D6): legăturile declarației, ca listă
// LOCALĂ de id-uri. Nu are verbe proprii pe sârmă — `FacturiIds` pleacă întreg
// la salvarea declarației, iar serverul creează lipsa și șterge plusul.
//
// `Stare` se AFIȘEAZĂ, nu se presupune (DVI-r2): anularea unei facturi legate la
// o declarație OPERATĂ nu e refuzată, iar cifrele declarației nu derivă din
// factură — deci ecranul trebuie să arate starea reală, oricare ar fi.

type Rand = {
  FacturaId: string;
  Numar?: string | null;
  Data?: string | null;
  PartenerDenumire?: string | null;
  Stare?: string | null;
  Valoare?: number | null;
};

export function PanouFacturi(props: {
  facturiIds: string[];
  // Legăturile așa cum le vede SERVERUL (ReadDto) — singura sursă a stării.
  citite?: DviFactura[] | null;
  // Candidații aleși în sesiunea asta, încă nesalvați: ce a afișat popup-ul,
  // reținut ca să nu rămână rânduri fără nume până la prima salvare.
  culesi: Record<string, FacturaCandidata>;
  dviId?: string | null;
  dataDocument?: string | null;
  readOnly: boolean;
  onAdauga: (candidati: FacturaCandidata[]) => void;
  onScoate: (facturaId: string) => void;
}) {
  const { facturiIds, citite, culesi, dviId, dataDocument, readOnly, onAdauga, onScoate } = props;
  const [alegeFacturi, setAlegeFacturi] = useState(false);

  const randuri: Rand[] = facturiIds.map((id) => {
    const server = citite?.find((f) => f.FacturaId === id);
    if (server)
      return {
        FacturaId: id,
        Numar: server.Numar,
        Data: server.Data,
        PartenerDenumire: server.PartenerDenumire,
        Stare: server.Stare,
        Valoare: server.Valoare,
      };
    const cules = culesi[id];
    // Starea rămâne GOALĂ pe o legătură nesalvată: candidații sunt facturi
    // operate prin construcția rutei, dar starea afișată e a serverului, nu o
    // deducție a clientului.
    return {
      FacturaId: id,
      Numar: cules?.Numar,
      Data: cules?.Data,
      PartenerDenumire: cules?.PartenerDenumire,
      Stare: null,
      Valoare: cules?.Valoare,
    };
  });

  return (
    <div className="dvi__sectiune">
      <div className="linii__bara">
        <h3>Facturi de import</h3>
        <button
          type="button"
          className="buton"
          disabled={readOnly}
          onClick={() => setAlegeFacturi(true)}
        >
          Adaugă facturi
        </button>
      </div>

      <DataGrid dataSource={randuri} keyExpr="FacturaId" showBorders columnAutoWidth>
        <Column dataField="Numar" caption="Număr" />
        <Column dataField="Data" caption="Dată" dataType="date" format="dd.MM.yyyy" />
        <Column dataField="PartenerDenumire" caption="Furnizor" />
        <Column
          dataField="Stare"
          caption="Stare"
          width={110}
          calculateCellValue={(r: Rand) => labelEnum('StareDocument', r.Stare)}
        />
        <Column dataField="Valoare" caption="Valoare" dataType="number" format="#,##0.00" alignment="right" />
        <Column
          caption=""
          width={90}
          cellRender={(c) => (
            <button
              type="button"
              className="buton buton--mic"
              disabled={readOnly}
              onClick={() => onScoate((c.data as Rand).FacturaId)}
            >
              Scoate
            </button>
          )}
        />
      </DataGrid>

      <p className="indiciu">
        Legătura e evidență și explicație, nu sursa cifrelor: baza și taxa sunt cele declarate în vamă.
        O declarație poate sta și fără nicio factură. Starea facturilor nesalvate apare după „Salvează”.
      </p>

      {alegeFacturi && (
        <PopupCandidati
          dviId={dviId}
          dataDocument={dataDocument}
          exclusi={facturiIds}
          onAdauga={(alesi) => { onAdauga(alesi); setAlegeFacturi(false); }}
          onInchide={() => setAlegeFacturi(false)}
        />
      )}
    </div>
  );
}

// Popup-ul candidaților: perioada (obligatorie pe rută), partenerul (opțional) și
// bifa „toate facturile" — parametrii CERERII, nu filtre ale paginii încărcate.
// Lista e plafonată pe server; `MaiSunt` spune că s-a tăiat și că perioada
// trebuie îngustată (nu se pagineaza aici: cererea e alta, cu altă perioadă).
function PopupCandidati(props: {
  dviId?: string | null;
  dataDocument?: string | null;
  exclusi: string[];
  onAdauga: (candidati: FacturaCandidata[]) => void;
  onInchide: () => void;
}) {
  const { dviId, dataDocument, exclusi, onAdauga, onInchide } = props;
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
    queryKey: ['dvi', 'facturi-candidate', perioada.dataStart, perioada.dataEnd, partenerId, toate, dviId ?? null],
    queryFn: () => dvi.facturiCandidate({
      dataStart: perioada.dataStart,
      dataEnd: perioada.dataEnd,
      partenerId,
      toate,
      dviId,
    }),
    enabled: !!perioada.dataStart && !!perioada.dataEnd,
  });

  // Facturile deja în agregat ies din listă: serverul le scoate pe cele SALVATE
  // (prin `dviId`), dar pe o declarație nesalvată — sau pe una căreia tocmai i
  // s-au adăugat legături — lista locală e singura care le știe.
  const randuri = (candidati.data?.Candidati ?? [])
    .filter((c) => c.FacturaId != null && !exclusi.includes(c.FacturaId));

  function adauga() {
    onAdauga(randuri.filter((c) => c.FacturaId != null && alese.includes(c.FacturaId)));
  }

  return (
    <Popup
      visible
      title="Facturi de import de legat"
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
          <span className="camp__eticheta">Toate facturile (nu doar extra-UE)</span>
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
          Lista e trunchiată la 500 de facturi — îngustați perioada ca să le vedeți pe toate.
        </p>
      )}

      <DataGrid
        dataSource={randuri}
        keyExpr="FacturaId"
        showBorders
        columnAutoWidth
        height={380}
        selectedRowKeys={alese}
        onSelectionChanged={(e) => setAlese(e.selectedRowKeys as string[])}
      >
        <Selection mode="multiple" showCheckBoxesMode="always" />
        <Column dataField="Numar" caption="Număr" />
        <Column dataField="Data" caption="Dată" dataType="date" format="dd.MM.yyyy" />
        <Column dataField="PartenerDenumire" caption="Furnizor" />
        <Column dataField="ClasaFiscala" caption="Clasă fiscală" width={140} />
        <Column dataField="Valoare" caption="Valoare" dataType="number" format="#,##0.00" alignment="right" />
      </DataGrid>

      <div className="editor-linie__comenzi">
        <button
          type="button"
          className="buton buton--primar"
          disabled={alese.length === 0}
          onClick={adauga}
        >
          {alese.length > 0 ? `Adaugă facturile alese (${alese.length})` : 'Adaugă facturile alese'}
        </button>
        <button type="button" className="buton" onClick={onInchide}>Renunță</button>
      </div>

      <p className="indiciu">
        Implicit se propun doar facturile operate ale furnizorilor extra-UE, în perioada aleasă.
        Bifa le arată pe toate cele operate.
      </p>
    </Popup>
  );
}

// Perioada implicită a popup-ului: luna DECLARAȚIEI, nu luna curentă — vămuirea
// și facturile ei stau de obicei în aceeași lună.
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
