import { useState, type ReactNode } from 'react';
import { Link, useNavigate, useParams } from 'react-router';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { Column, DataGrid } from 'devextreme-react/data-grid';
import { DocumentShell, type Comanda } from '../../nucleu/DocumentShell';
import { CampShell } from '../../nucleu/CampShell';
import { campMeta, labelEnum } from '../../nucleu/campMeta';
import { eroriDin } from '../../nucleu/http';
import { nir, SCHEMA_ANTET, SCHEMA_LINIE, TIP_ANTET, TIP_LINIE } from './api';

// Ecran READ-ONLY cu comenzi (F2-D3). Nu există `Formular`, fiindcă nu există
// agregat de scriere: câmpurile sunt `CampShell` cu valoare statică — același
// layout și aceleași captions ca la culegere, fără niciun editor.
//
// Fluxul-ancoră: FCT operată → `ConexId` → aici → Operează. Legătura înapoi spre
// factură (`DocumentSursaNumar`) e link, nu text: grupul conex se parcurge în
// ambele sensuri.

const capLinie = (m: string) => campMeta(TIP_LINIE, m, SCHEMA_LINIE).caption;

export function NirDetaliu() {
  const { id } = useParams();
  const navigheaza = useNavigate();
  const cache = useQueryClient();
  const [erori, setErori] = useState<string[]>([]);
  const [mesaje, setMesaje] = useState<string[]>([]);

  const citit = useQuery({
    queryKey: ['nir', id],
    queryFn: () => nir.citeste(id!),
    enabled: id != null,
  });
  const doc = citit.data;

  async function comanda(rulare: () => Promise<{ Mesaje?: string[] | null; StareNoua?: string | null }>) {
    setErori([]);
    setMesaje([]);
    try {
      const rezultat = await rulare();
      setMesaje([`Stare nouă: ${labelEnum('StareDocument', rezultat.StareNoua)}`, ...(rezultat.Mesaje ?? [])]);
      await cache.invalidateQueries({ queryKey: ['nir'] });
      // Anularea/stornarea NIR-ului schimbă și ce poate face FACTURA-sursă
      // (gardienii de grup conex) — cache-ul ei nu mai e de încredere.
      await cache.invalidateQueries({ queryKey: ['fct'] });
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
        void nir.valideaza(id!)
          .then((e) => (e.length > 0 ? setErori(e) : setMesaje(['Documentul trece toți gardienii motorului.'])))
          .catch((e) => setErori(eroriDin(e)));
      },
    },
    { eticheta: 'Operează', disponibila: doc?.PoateOpera ?? false, primara: true, ruleaza: () => void comanda(() => nir.opereaza(id!)) },
    { eticheta: 'Anulează operarea', disponibila: doc?.PoateAnula ?? false, ruleaza: () => void comanda(() => nir.anuleaza(id!)) },
    {
      eticheta: 'Stornează',
      disponibila: doc?.PoateStorna ?? false,
      cereData: { eticheta: 'Data stornării' },
      ruleaza: (data) => { if (data) void comanda(() => nir.storneaza(id!, data)); },
    },
    { eticheta: 'Înapoi la listă', disponibila: true, ruleaza: () => navigheaza('/nir') },
  ];

  return (
    <DocumentShell
      titlu={`NIR ${doc?.Numar ?? ''}`}
      sumar={
        <div className="sumar">
          <span className="sumar__stare">{labelEnum('StareDocument', doc?.Stare) || '—'}</span>
          <span className="sumar__total">Total: {doc?.Total == null ? '—' : doc.Total.toFixed(2)}</span>
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
          <Static membru="PredatorId" valoare={doc?.PredatorDenumire} />
          <Static membru="PrimitorId" valoare={doc?.PrimitorDenumire} />
          <Static membru="DataOperare" valoare={doc?.DataOperare?.slice(0, 10)} />
          <Static membru="Autogenerat" valoare={doc?.Autogenerat ? 'da' : 'nu'} />
          <Static
            membru="DocumentSursaId"
            valoare={doc?.DocumentSursaId
              ? <Link to={`/fct/${doc.DocumentSursaId}`}>{doc.DocumentSursaNumar || 'Deschide factura sursă'}</Link>
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
            <Column dataField="LotEticheta" caption={capLinie('LotId')} />
            <Column dataField="Cantitate" caption={capLinie('Cantitate')} dataType="number" format="#,##0.###" alignment="right" />
            <Column dataField="Valoare" caption={capLinie('Valoare')} dataType="number" format="#,##0.00" alignment="right" />
            <Column dataField="ValoareTva" caption={capLinie('ValoareTva')} dataType="number" format="#,##0.00" alignment="right" />
            <Column dataField="TipTvaCod" caption={capLinie('TipTvaId')} />
            {/* Dimensiunile frunzei (DIM-2), ca la culegerea facturii: aici doar
                codurile — clona conexă le-a primit prin contract, nu se editează. */}
            <Column dataField="CodEconomicCod" caption={capLinie('CodEconomicId')} />
            <Column dataField="SursaFinantareCod" caption={capLinie('SursaFinantareId')} />
            <Column dataField="CodFunctionalCod" caption={capLinie('CodFunctionalId')} />
            <Column dataField="ProiectCod" caption={capLinie('ProiectId')} />
          </DataGrid>
        </>
      }
    />
  );
}

// Câmp de AFIȘARE: aceeași ramă ca la culegere (etichetă din metadata, slot de
// control), cu text în locul editorului. `obligatoriu` se stinge explicit — pe un
// ecran fără scriere, asteriscul n-ar cere nimic de la nimeni.
function Static(props: { membru: string; valoare: ReactNode }) {
  const meta = { ...campMeta(TIP_ANTET, props.membru, SCHEMA_ANTET), obligatoriu: false };
  return (
    <CampShell meta={meta}>
      <div className="valoare-statica">{props.valoare == null || props.valoare === '' ? '—' : props.valoare}</div>
    </CampShell>
  );
}
