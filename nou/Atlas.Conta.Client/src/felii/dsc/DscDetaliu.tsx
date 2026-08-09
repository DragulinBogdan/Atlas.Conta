import { useState, type ReactNode } from 'react';
import { Link, useNavigate, useParams } from 'react-router';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { Column, DataGrid } from 'devextreme-react/data-grid';
import { DocumentShell, type Comanda } from '../../nucleu/DocumentShell';
import { CampShell } from '../../nucleu/CampShell';
import { campMeta, labelEnum } from '../../nucleu/campMeta';
import { eroriDin } from '../../nucleu/http';
import { rutaTip } from '../../nucleu/stingeri';
import { dsc, SCHEMA_ANTET, SCHEMA_LINIE, TIP_ANTET, TIP_LINIE } from './api';

// Ecran READ-ONLY cu comenzi (F4-D2), pe șablonul NIR-ului. Nu există `Formular`,
// fiindcă nu există agregat de scriere: câmpurile sunt `CampShell` cu valoare
// statică — același layout și aceleași captions ca la culegere, fără editor.
//
// Fluxul-ancoră: FCL operată → descărcarea generată (automat sau pe backorder) →
// aici → Operează. Legătura înapoi spre factură e link, nu text: grupul conex se
// parcurge în ambele sensuri.
//
// Liniile sunt rezultatul PICKINGULUI: o linie de factură se poate sparge pe mai
// multe loturi (37b), iar valoarea e COSTUL (preț de lot × cantitate), nu prețul
// de vânzare — venitul și TVA-ul rămân integral pe factură.

const capLinie = (m: string) => campMeta(TIP_LINIE, m, SCHEMA_LINIE).caption;

export function DscDetaliu() {
  const { id } = useParams();
  const navigheaza = useNavigate();
  const cache = useQueryClient();
  const [erori, setErori] = useState<string[]>([]);
  const [mesaje, setMesaje] = useState<string[]>([]);

  const citit = useQuery({
    queryKey: ['dsc', id],
    queryFn: () => dsc.citeste(id!),
    enabled: id != null,
  });
  const doc = citit.data;

  async function comanda(rulare: () => Promise<{ Mesaje?: string[] | null; StareNoua?: string | null }>) {
    setErori([]);
    setMesaje([]);
    try {
      const rezultat = await rulare();
      setMesaje([`Stare nouă: ${labelEnum('StareDocument', rezultat.StareNoua)}`, ...(rezultat.Mesaje ?? [])]);
      await cache.invalidateQueries({ queryKey: ['dsc'] });
      // Anularea/stornarea descărcării schimbă și ce poate face FACTURA-sursă
      // (gardienii de grup conex) și acoperirea ei — cache-ul ei nu mai e de
      // încredere.
      await cache.invalidateQueries({ queryKey: ['fcl'] });
    }
    catch (e) {
      setErori(eroriDin(e));
    }
  }

  const comenzi: Comanda[] = [
    {
      eticheta: 'Verifică',
      disponibila: id != null,
      ruleaza: () => {
        setErori([]);
        setMesaje([]);
        void dsc.valideaza(id!)
          .then((e) => (e.length > 0 ? setErori(e) : setMesaje(['Documentul trece toți gardienii motorului.'])))
          .catch((e) => setErori(eroriDin(e)));
      },
    },
    { eticheta: 'Operează', disponibila: doc?.PoateOpera ?? false, primara: true, ruleaza: () => void comanda(() => dsc.opereaza(id!)) },
    { eticheta: 'Anulează operarea', disponibila: doc?.PoateAnula ?? false, ruleaza: () => void comanda(() => dsc.anuleaza(id!)) },
    {
      eticheta: 'Stornează',
      disponibila: doc?.PoateStorna ?? false,
      cereData: { eticheta: 'Data stornării' },
      ruleaza: (data) => { if (data) void comanda(() => dsc.storneaza(id!, data)); },
    },
    { eticheta: 'Înapoi la listă', disponibila: true, ruleaza: () => navigheaza('/dsc') },
  ];

  // Sursa unei descărcări e azi întotdeauna o factură de ieșire, dar tipul vine
  // din ReadDto și ruta din `rutaTip` — vocabularul închis al nucleului — nu
  // dintr-un string lipit aici (D-6b).
  const rutaSursa = doc?.DocumentSursaId ? rutaTip(doc.DocumentSursaTip, doc.DocumentSursaId) : null;

  return (
    <DocumentShell
      titlu={`Descărcare de gestiune ${doc?.Numar ?? ''}`}
      sumar={
        <div className="sumar">
          <span className="sumar__stare">{labelEnum('StareDocument', doc?.Stare) || '—'}</span>
          <span className="sumar__total">Cost total: {doc?.Total == null ? '—' : doc.Total.toFixed(2)}</span>
        </div>
      }
      comenzi={comenzi}
      erori={erori}
      mesaje={mesaje}
      ocupat={citit.isFetching}
      antet={
        <div className="grila-campuri">
          <Static membru="Numar" valoare={doc?.Numar} />
          <Static membru="Data" valoare={doc?.Data} />
          <Static membru="Stare" valoare={labelEnum('StareDocument', doc?.Stare)} />
          <Static membru="PredatorId" eticheta="Gestiune" valoare={doc?.PredatorDenumire} />
          <Static membru="PrimitorId" eticheta="Client" valoare={doc?.PrimitorDenumire} />
          <Static membru="DataOperare" valoare={doc?.DataOperare?.slice(0, 10)} />
          <Static membru="Autogenerat" valoare={doc?.Autogenerat ? 'da' : 'nu'} />
          <Static
            membru="DocumentSursaId"
            eticheta="Generat din"
            valoare={rutaSursa
              ? <Link to={rutaSursa}>{doc?.DocumentSursaNumar || 'Deschide factura de ieșire'}</Link>
              : null}
          />
        </div>
      }
      linii={
        <>
          <div className="linii__bara"><h3>Linii</h3></div>
          <DataGrid dataSource={doc?.Linii ?? []} keyExpr="Id" showBorders columnAutoWidth>
            <Column dataField="TipMaterialCod" caption="Cod tip" />
            <Column dataField="TipMaterialDenumire" caption={capLinie('TipMaterialId')} />
            <Column dataField="ProdusDenumire" caption="Produs" />
            <Column dataField="LotEticheta" caption={capLinie('LotId')} />
            <Column dataField="Cantitate" caption={capLinie('Cantitate')} dataType="number" format="#,##0.###" alignment="right" />
            {/* `Valoare` pe DSC e COSTUL descărcat (preț de lot × cantitate — 37a),
                nu prețul de vânzare. */}
            <Column dataField="Valoare" caption={`${capLinie('Valoare')} (cost)`} dataType="number" format="#,##0.00" alignment="right" />
            {/* Dimensiunea frunzei (DIM-2), clonată din linia facturii. */}
            <Column dataField="CodEconomicCod" caption={capLinie('CodEconomicId')} />
          </DataGrid>
        </>
      }
    />
  );
}

// Câmp de AFIȘARE: aceeași ramă ca la culegere (etichetă din metadata, slot de
// control), cu text în locul editorului. `obligatoriu` se stinge explicit — pe un
// ecran fără scriere, asteriscul n-ar cere nimic de la nimeni.
function Static(props: { membru: string; eticheta?: string; valoare: ReactNode }) {
  const meta = campMeta(TIP_ANTET, props.membru, SCHEMA_ANTET);
  const efectiv = {
    ...meta,
    obligatoriu: false,
    ...(props.eticheta == null ? {} : { caption: props.eticheta }),
  };
  return (
    <CampShell meta={efectiv}>
      <div className="valoare-statica">{props.valoare == null || props.valoare === '' ? '—' : props.valoare}</div>
    </CampShell>
  );
}
