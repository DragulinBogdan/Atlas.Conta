import { useMemo } from 'react';
import { Link } from 'react-router';
import {
  Column, ColumnFixing, DataGrid, FilterRow, HeaderFilter, Pager, Paging, Sorting,
  Summary, TotalItem,
} from 'devextreme-react/data-grid';
import type { components } from '../../generated/api-types';
import { labelEnum } from '../../nucleu/campMeta';
import { storeRemote } from '../../nucleu/dxStore';
import { rutaTip } from '../../nucleu/stingeri';
import { urlCu, useUrlStare } from '../../nucleu/urlStare';
import { CasetaData, lunaCurenta } from '../raportare/comune';

// Jurnalul de cumpărări / de vânzări (JT-D9) — UN component, DOUĂ ecrane.
//
// Sunt aceeași proiecție pe laturi diferite, deci se parametrizează pe `Sens`,
// exact cum `PLT`/`INC` partajează nucleul de trezorerie (57a). Nu un ecran cu un
// comutator, însă: sunt două rapoarte distincte, fiecare cu ruta lui, cu meniul
// lui și cu deep-link propriu — comutatorul ar fi făcut din ele o singură stare
// partajabilă, iar „jurnalul de cumpărări pe februarie" n-ar mai fi avut link.
//
// Ca peste tot în raportare: nimic nu se calculează aici (42c). Bazele, TVA-ul și
// agregarea la nivel de factură vin gata făcute de pe server; grila doar afișează
// și însumează ce a primit.
type JurnalTvaRand = components['schemas']['JurnalTvaRand'];
const camp = (n: keyof JurnalTvaRand & string) => n;

type Sens = 'Achizitie' | 'Livrare';

function JurnalTva({ sens, titlu }: { sens: Sens; titlu: string }) {
  const luna = lunaCurenta();
  // Perioada trăiește în URL (43c), implicit luna curentă. Serverul o tratează ca
  // filtru SIMPLU și o acceptă lipsă (un jurnal n-are „sold inițial", JT-D7) —
  // dar ecranul îi dă totuși un implicit, fiindcă un jurnal de TVA se citește
  // întotdeauna pe o perioadă fiscală: o declarație e lunară sau trimestrială,
  // niciodată „de la începutul evidenței". Consecința convenției `useUrlStare`:
  // cu implicit calculat, casetele NU pot fi golite — deliberat aici, invers față
  // de registrul-jurnal, care e o listare.
  const [stare, seteaza] = useUrlStare({ dataStart: luna.start, dataEnd: luna.sfarsit });

  const sursa = useMemo(() => storeRemote(
    urlCu('/api/proiectii/jurnal-tva', { sens, ...stare }),
    // ═══ Cheia grilei e PERECHEA, nu documentul ═══
    // Cheia de contract a proiecției e (Document × TipTva): aceeași factură cu
    // două cote e DOUĂ rânduri. Cu `DocumentId` singur, grila le-ar considera
    // „același rând" — exact capcana pe care balanța o rezolvă cu cheia dublă în
    // modul analitic.
    // Grupul proiecției e cu un pas mai larg (poartă și `PartenerId`, `Regim`,
    // `Cota`), dar cele trei sunt funcțional determinate de pereche prin
    // construcție — toate rândurile fiscale ale unui document se scriu într-o
    // singură operare, din același `TipTva`. `PartenerId` e în plus `Guid?`, iar
    // o cheie de grilă cu componentă nulă e mai riscantă decât coliziunea
    // teoretică pe care ar preveni-o.
    ['DocumentId', 'TipTvaId'],
  ), [sens, stare]);

  return (
    <div className="ecran">
      <div className="ecran__bara"><h2>{titlu}</h2></div>

      <div className="bara-raport">
        <CasetaData eticheta="De la" valoare={stare.dataStart} seteaza={(v) => seteaza({ dataStart: v })} />
        <CasetaData eticheta="Până la" valoare={stare.dataEnd} seteaza={(v) => seteaza({ dataEnd: v })} />
      </div>

      <DataGrid
        // Remontare la schimbarea raportului: perioada e PARAMETRU al proiecției,
        // nu filtru de grilă, iar starea de filtrare/paginare a lunii precedente
        // n-are ce transporta în luna următoare. `sens` intră în cheie deși cele
        // două ecrane sunt componente diferite — dacă vreodată ar ajunge pe
        // aceeași poziție de rutare, remontarea rămâne garantată.
        key={`${sens}|${stare.dataStart}|${stare.dataEnd}`}
        dataSource={sursa}
        // `summary` remote: totalurile se calculează de `DataSourceLoader`, peste
        // TOATE rândurile filtrate, nu peste pagina afișată. `grouping` nu e
        // pentru un panou de grupare (nu există aici), ci fiindcă `HeaderFilter`
        // își cere lista de valori prin protocolul de grupare — fără el, grila ar
        // încerca să materializeze tot jurnalul în browser ca să compună panoul.
        remoteOperations={{ filtering: true, sorting: true, paging: true, summary: true, grouping: true }}
        showBorders
        // FĂRĂ `columnAutoWidth` (spre deosebire de balanță și de registrul-jurnal):
        // aici coloanele de etichetă sunt lungi (denumiri de parteneri și de
        // tipuri de TVA), iar dimensionarea pe conținut împingea `Bază` și `TVA`
        // dincolo de marginea ecranului — adică exact cifrele pentru care se
        // deschide raportul. Fără el, lățimile declarate se respectă și restul se
        // împarte, deci textul lung se trunchiază în loc să scoată cifrele afară.
        height="calc(100vh - 220px)"
      >
        {/* Sortarea e permisă (ca la registrul-jurnal, spre deosebire de fișă):
            jurnalul n-are sold cumulat de rupt — ordinea implicită, declarată pe
            server, e cronologică. */}
        <Sorting mode="multiple" />
        <FilterRow visible />
        <HeaderFilter visible />
        <ColumnFixing enabled />
        <Paging defaultPageSize={50} />
        <Pager showInfo showPageSizeSelector allowedPageSizes={[50, 100, 200]} />

        <Column dataField={camp('Data')} caption="Data" dataType="date" format="dd.MM.yyyy" width={100} fixed />
        <Column
          dataField={camp('DocumentNumar')}
          caption="Document"
          width={150}
          fixed
          // Ca la fișă și la registrul-jurnal: `DocumentTip` se completează în
          // memorie peste pagină (R-D8), deci nu e coloană filtrabilă/sortabilă,
          // iar rutarea trece prin `rutaTip` — vocabular ÎNCHIS: un tip fără felie
          // de client (azi retururile RLF/RDC) rămâne TEXT, nu link mort.
          allowFiltering={false}
          allowSorting={false}
          cellRender={celulaDocument}
        />

        {/* Etichetele vin din join-uri LEFT (proiecția, lecția review-ului D4 al
            feliei 9): un rând nu are voie să dispară dintr-un raport fiindcă i-a
            dispărut ETICHETA — mai ales din raportul care ajunge într-o
            declarație fiscală. Ce lipsește se MARCHEAZĂ; un gol s-ar citi ca o
            scăpare de randare. */}
        <Column dataField={camp('PartenerDenumire')} caption="Partener" minWidth={180} cellRender={celulaPartener} />
        <Column dataField={camp('PartenerCodFiscal')} caption="Cod fiscal" width={130} />

        <Column dataField={camp('TipTvaCod')} caption="Tip TVA" width={90} cellRender={celulaTipTva} />
        {/* Plafonată explicit: denumirile tipurilor sunt lungi („TVA 19%
            (standard, istoric — până la 31.07.2025)") și ar mânca lățimea
            cifrelor. Se trunchiază — codul, regimul și cota de alături spun
            oricum ce e. */}
        <Column dataField={camp('TipTvaDenumire')} caption="Denumire TVA" width={200} />
        {/* SNAPSHOT-uri de pe rând (JT-D3): au intrat în aritmetica bazei și a
            TVA-ului, deci nu se re-citesc din nomenclatorul de azi. Eticheta
            regimului trece prin `labelEnum` — o singură sursă cu XAF-ul (dump-ul
            de metadata); azi label-ul E numele membrului, iar un `[XafDisplayName]`
            adăugat în C# apare aici fără nicio schimbare de client. */}
        <Column dataField={camp('Regim')} caption="Regim" width={130} cellRender={celulaRegim} />
        <Column dataField={camp('Cota')} caption="Cotă %" dataType="number" format="#0.##" alignment="right" width={80} />
        {/* Etichetă direcțională, rezolvată la citire (JT-D3): codul ANAF diferă
            între livrare și achiziție, iar nomenclatorul se schimbă cu anul de
            raportare. */}
        <Column dataField={camp('CodSafT')} caption="Cod SAF-T" width={110} />

        <Column dataField={camp('Baza')} caption="Bază" {...BANI} />
        <Column dataField={camp('Tva')} caption="TVA" {...BANI} />

        {/* ═══ Totaluri LEGITIME aici — și nu e o inconsecvență față de R-D5 ═══
            Balanța interzice totalurile pe `Sold*` fiindcă netarea nu e aditivă
            (401 cu un furnizor pe debit 100 și altul pe credit 200 dă analitic
            „D 100 / C 200", sintetic „C 100"). `Baza` și `Tva` NU sunt solduri
            netate, ci sume de operațiuni: însumarea lor peste perioadă e chiar
            cifra pe care o cere decontul. Rândurile de storno intră cu semnul lor
            (JT-D5) — suma algebrică rămâne adevărul. */}
        <Summary>
          <TotalItem column={camp('Baza')} summaryType="sum" valueFormat="#,##0.00" displayFormat="Σ {0}" />
          <TotalItem column={camp('Tva')} summaryType="sum" valueFormat="#,##0.00" displayFormat="Σ {0}" />
        </Summary>
      </DataGrid>

      <p className="indiciu">
        Un rând = o factură × un tip de TVA (o factură cu două cote apare de două ori).
        Stornările intră cu semn negativ, la data stornării — jurnalul unei luni deja declarate rămâne cum a fost.
        Pe deconturi contrapartida e <strong>titularul</strong> (angajatul), nu comerciantul de pe bon: atât știe modelul.
        Liniile fără tip de TVA nu apar deloc — e o gaură a datelor, care se măsoară, nu se umple cu un regim presupus.
      </p>
    </div>
  );
}

// Cele două ecrane: aceeași proiecție, laturi diferite. Rute și meniuri proprii.
export function JurnalCumparari() {
  return <JurnalTva sens="Achizitie" titlu="Jurnal de cumpărări" />;
}

export function JurnalVanzari() {
  return <JurnalTva sens="Livrare" titlu="Jurnal de vânzări" />;
}

function celulaDocument({ data }: { data: JurnalTvaRand }) {
  // `DocumentId` e NENUL în registru (JT-D1: jurnalul n-are rânduri de
  // deschidere), deci aici nu există cazul „(deschidere)" al fișei.
  const ruta = data.DocumentId ? rutaTip(data.DocumentTip, data.DocumentId) : null;
  const text = data.DocumentNumar || data.DocumentTip || '(document)';
  return ruta ? <Link to={ruta}>{text}</Link> : <span>{text}</span>;
}

// Două fapte DIFERITE, spuse diferit (risc 4 din design):
//   • `PartenerId` nul  = politica tipului rezolvă contrapartida altfel decât
//     dintr-o latură (`Explicit`/`TipMaterial`) ⇒ jurnalul chiar nu are pe cine
//     arăta. Se raportează, nu se refuză — postarea contabilă a aceluiași rând e
//     perfectă.
//   • `PartenerId` prezent, denumire lipsă = LEFT JOIN: repartitor șters logic
//     sau invizibil prin securitate. Cifra rândului e reală și intră în totaluri.
function celulaPartener({ data }: { data: JurnalTvaRand }) {
  if (!data.PartenerId) return <span className="indiciu">(fără contrapartidă)</span>;
  if (!data.PartenerDenumire)
    return <span className="indiciu" title={data.PartenerId}>(partener indisponibil)</span>;
  return <span>{data.PartenerDenumire}</span>;
}

function celulaTipTva({ data }: { data: JurnalTvaRand }) {
  if (data.TipTvaCod) return <span>{data.TipTvaCod}</span>;
  return <span className="indiciu" title={data.TipTvaId ?? ''}>(tip TVA indisponibil)</span>;
}

function celulaRegim({ data }: { data: JurnalTvaRand }) {
  return <span>{labelEnum('RegimTva', data.Regim)}</span>;
}

const BANI = { dataType: 'number', format: '#,##0.00', alignment: 'right', width: 140 } as const;
