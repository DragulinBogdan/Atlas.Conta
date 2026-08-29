import { Fragment, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router';
import type { components } from '../../generated/api-types';
import { labelEnum } from '../../nucleu/campMeta';
import { descarcaFisier, eroriDin, ia } from '../../nucleu/http';
import { PanouErori } from '../../nucleu/PanouErori';
import { urlCu, useUrlStare } from '../../nucleu/urlStare';

// SAF-T — declarația D406, modulele L (felia 16) și S (stocuri, felia 17). Al
// treilea ecran de „formular peste registre", după D300 și D394, și primul care
// are un FIȘIER.
//
// Ce arată, în ordinea în care se citește o declarație înainte de a o depune:
//  (1) antetul — cine declară, pe ce perioadă, pe ce bază contabilă;
//  (2) rezumatul per secțiune — câte intrări are fiecare și cât fac;
//  (3) CUSĂTURILE — probele de completitudine ale proiecției (partidă dublă,
//      TVA în trei termeni, baza facturilor per sens, soldurile contra balanței;
//      pe S: stocul fizic care se închide, nimic pierdut, referințele),
//      fiecare cu verdictul ei vizibil: dacă una nu bate, se vede AICI, nu abia
//      la validatorul ANAF;
//  (4) ce n-a intrat în fișier: `Excluse` DELIBERAT (politica, cu motivul scris
//      de om) și `Neincluse` (ce n-a găsit regulă — adică o gaură);
//  (5) avertismentele agregate — ce cere formularul și modelul n-are.
//
// L și S sunt DOUĂ DECLARAȚII, nu două file ale aceleiași: se depun separat, au
// alte secțiuni și alt fișier. Ecranul e totuși unul, cu un comutator, fiindcă
// întrebarea omului e aceeași („ce iese pe luna asta și pot să-l depun?"), iar
// felul e în URL (43c: URL-ul e starea globală) — `/saft?fel=S` e un link care
// arată exact ce a văzut cel care l-a trimis.
//
// Ce NU face: nu calculează nimic — nici sume, nici NUMĂRĂTORI. Ecranul consumă
// SUMARUL (`SaftSumarDto`), nu declarația: pe o lună reală declarația e de 38,6
// MiB de JSON din care ecranul n-ar afișa nicio linie, așa că ușa JSON servește
// antetul, contoarele per secțiune, cusăturile, `Excluse`, `Neincluse` și
// avertismentele — iar listele trăiesc doar în FIȘIER (butonul „Descarcă XML").
// Contoarele vin gata numărate de server (42c, în litera lui: „TS nu calculează
// niciodată sold/rest/total"); clientul doar așază cifre pe care serverul le-a
// trimis pe amândouă.
type SaftSumar = components['schemas']['SaftSumarDto'];
type Rezumat = components['schemas']['SaftRezumat'];
type Neinclus = components['schemas']['SaftNeinclus'];
type NeinclusAgregat = components['schemas']['NeinclusAgregat'];
type Exclus = components['schemas']['SaftExclus'];
type DiferentaCont = components['schemas']['SaftDiferentaCont'];
type Avertisment = components['schemas']['SaftAvertisment'];

const LUNI = [
  'ianuarie', 'februarie', 'martie', 'aprilie', 'mai', 'iunie',
  'iulie', 'august', 'septembrie', 'octombrie', 'noiembrie', 'decembrie',
];

// `PeriodYear` are `minInclusive = 2020` în schemă — aceeași margine ca refuzul
// serverului, ca lista să nu ofere ce API-ul respinge.
const AN_MINIM = 2020;

// `Guid.Empty` pe sârmă: „câmpul e prezent, dar nu are nimic în spate". Un FK
// nenullabil în DTO nu poate lipsi, deci absența se scrie așa — iar un link
// construit pe el ar deschide un ecran gol.
const GUID_GOL = '00000000-0000-0000-0000-000000000000';

// Cele două module, cu tot ce diferă între ele într-un singur loc: ruta
// sumarului, ruta fișierului, numele implicit al descărcării și titlul. Un
// `if (fel === 'S')` împrăștiat prin componentă ar fi însemnat că a treia
// declarație (A, imobilizări) se adaugă căutând prin JSX.
const MODULE = {
  L: {
    titlu: 'lunar',
    cale: '/api/proiectii/saft',
    caleXml: '/api/proiectii/saft/xml',
    prefixFisier: 'SAF-T',
  },
  S: {
    titlu: 'stocuri, la cerere',
    cale: '/api/proiectii/saft/stocuri',
    caleXml: '/api/proiectii/saft/stocuri/xml',
    prefixFisier: 'SAF-T-S',
  },
} as const;

type Fel = keyof typeof MODULE;

export function Saft() {
  const acum = new Date();
  const [stare, seteaza] = useUrlStare({
    an: String(acum.getFullYear()),
    luna: String(acum.getMonth() + 1),
    fel: 'L',
  });
  // Starea descărcării poartă și CEREREA pentru care s-a produs. Fără asta,
  // refuzul unei descărcări (de pildă „01/2020 n-are stoc fizic") rămâne pe
  // ecran după ce omul schimbă luna sau modulul — un mesaj despre ALTĂ
  // declarație, așezat peste cea de acum, care e mai rău decât niciun mesaj.
  // Se rezolvă la randare, comparând cu cererea curentă, nu cu un `useEffect`
  // de curățare: efectul ar fi șters eroarea într-un al doilea pas, adică ar fi
  // existat o randare în care mesajul greșit era încă acolo.
  const [descarcare, setDescarcare] = useState<{ activa: boolean; erori: string[]; cerere: string }>(
    { activa: false, erori: [], cerere: '' });

  // Felul vine din URL, deci poate fi orice a tastat cineva: ce nu e un modul
  // cunoscut cade pe L. Nu e o validare — e refuzul de a randa un ecran gol
  // pentru o literă greșită.
  const fel: Fel = stare.fel === 'S' ? 'S' : 'L';
  const modul = MODULE[fel];

  const cale = urlCu(modul.cale, { an: stare.an, luna: stare.luna });
  const citit = useQuery({ queryKey: ['saft', cale], queryFn: () => ia<SaftSumar>(cale) });

  const sumar = citit.data;
  const rezumat: Rezumat = sumar?.Rezumat ?? {};
  const neincluse = sumar?.Neincluse ?? [];
  const excluse = sumar?.Excluse ?? [];
  const avertismente = sumar?.Avertismente ?? [];

  async function descarca() {
    const cerere = cale;
    setDescarcare({ activa: true, erori: [], cerere });
    try {
      // Numele implicit e o plasă: serverul îl trimite în `Content-Disposition`
      // (el știe CUI-ul societății), iar `descarcaFisier` îl preferă pe al lui.
      await descarcaFisier(
        urlCu(modul.caleXml, { an: stare.an, luna: stare.luna }),
        `${modul.prefixFisier}_${stare.an}-${String(stare.luna).padStart(2, '0')}.xml`,
        'application/xml, application/json');
      setDescarcare({ activa: false, erori: [], cerere });
    }
    catch (e) {
      // 403 (fără drept pe registru), 422 (profil bugetar, CUI lipsă, iar pe S
      // luna fără nicio intrare de stoc fizic) și orice altceva ies ca listă de
      // mesaje, în același panou ca refuzurile de domeniu — inline, nu `alert`.
      setDescarcare({ activa: false, erori: eroriDin(e), cerere });
    }
  }

  // Refuzul se arată doar cât timp e al declarației de pe ecran.
  const eroriDescarcare = descarcare.cerere === cale ? descarcare.erori : [];

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>SAF-T — declarația D406 ({modul.titlu})</h2>
      </div>

      <div className="bara-raport">
        <label className="bara-raport__camp">
          <span className="camp__eticheta">Declarație</span>
          {/* Comutatorul L/S. `<select>` nativ, ca perioada: două valori
              închise, fără stare intermediară de reprezentat. Schimbarea
              rescrie URL-ul, iar cheia de query se schimbă odată cu ruta —
              deci nu există moment în care ecranul „S" să arate cifrele lui
              „L". */}
          <select value={fel} onChange={(e) => seteaza({ fel: e.target.value })}>
            <option value="L">L — lunar (contabilitate)</option>
            <option value="S">S — stocuri (la cerere)</option>
          </select>
        </label>
        <label className="bara-raport__camp">
          <span className="camp__eticheta">An</span>
          {/* `<select>` nativ, nu widget: valorile sunt o listă închisă, iar un
              câmp tastabil legat de URL ar declanșa o cerere per tastă (lecția
              69h). Aici nu există stare intermediară de reprezentat. */}
          <select value={stare.an} onChange={(e) => seteaza({ an: e.target.value })}>
            {ani(acum.getFullYear()).map((a) => <option key={a} value={String(a)}>{a}</option>)}
          </select>
        </label>
        <label className="bara-raport__camp">
          <span className="camp__eticheta">Luna</span>
          <select value={stare.luna} onChange={(e) => seteaza({ luna: e.target.value })}>
            {LUNI.map((nume, i) => (
              <option key={nume} value={String(i + 1)}>{`${i + 1} — ${nume}`}</option>
            ))}
          </select>
        </label>
        {sumar && (
          <button
            type="button"
            className="buton buton--primar"
            disabled={descarcare.activa}
            onClick={descarca}
          >
            {descarcare.activa ? 'Se generează…' : 'Descarcă XML'}
          </button>
        )}
      </div>

      {/* Refuzul cererii (400 perioadă, 422 profil bugetar) — cu motivul
          serverului, și fără buton de descărcare: n-ai ce descărca. */}
      <PanouErori erori={citit.error ? eroriDin(citit.error) : []} titlu="Declarația nu se poate genera" />
      <PanouErori erori={eroriDescarcare} titlu="Fișierul nu s-a putut genera" />

      {citit.isPending ? <p className="indiciu">Se încarcă…</p> : sumar && (
        <>
          <Antet sumar={sumar} />
          {fel === 'S' ? (
            <>
              <SectiuniStoc sumar={sumar} rezumat={rezumat} />
              <CusaturiStoc rezumat={rezumat} />
              <StocPerCont lista={rezumat.StocPerCont ?? []} rezumat={rezumat} sumar={sumar} />
              <Excluse lista={excluse} />
            </>
          ) : (
            <>
              <Sectiuni sumar={sumar} rezumat={rezumat} />
              <Cusaturi rezumat={rezumat} />
            </>
          )}
          <Neincluse lista={neincluse} fel={fel} />
          <Avertismente lista={avertismente} />
        </>
      )}

      {fel === 'S' ? (
        <p className="indiciu">
          Cifrele sunt cele din <strong>registrul de stoc</strong>, așezate pe structura D406 modul
          <strong> S</strong> („la cerere"): tipurile de mișcare, produsele, stocul fizic per gestiune și
          lot, mișcările de bunuri. Se depune tot pe <strong>o lună</strong>, la solicitarea ANAF, ca
          fișier separat de cel lunar. Codul fiecărei mișcări e o <strong>politică</strong>
          („Politici → Mișcări SAF-T"), nu o regulă din cod — un tip de document fără politică apare mai
          jos la „Neincluse". Cantitățile sunt semnate ca în registru (intrare +, ieșire −).
        </p>
      ) : (
        <p className="indiciu">
          Cifrele sunt cele din <strong>registrul contabil</strong> și din <strong>registrul de TVA</strong>,
          așezate pe structura D406 modul <strong>L</strong> (lunar): master files, jurnalele contabile,
          facturile emise și primite, plățile. Declarația <strong>S</strong> (mișcări de stocuri) e altă
          declarație, nu o secțiune opțională a acesteia — se alege din comutatorul de sus;
          <strong> A</strong> (imobilizări) e un modul separat, încă neimplementat.
          Sumele sunt exacte (în bani); rotunjirea la 2 zecimale e a fișierului.
        </p>
      )}
    </div>
  );
}

function ani(anCurent: number): number[] {
  const lista: number[] = [];
  for (let a = anCurent + 1; a >= AN_MINIM; a--)
    lista.push(a);
  return lista;
}

// Antetul: cine declară și pe ce. Vine din `Header`, adică din rândul unic
// `Societate` — dacă lipsește ceva, avertismentele de jos o spun pe nume.
// `HeaderComment` e SINGURUL lucru care distinge modulul în fișier („L" / „C"),
// deci se afișează: e răspunsul la „ce fel de declarație am descărcat?".
function Antet({ sumar }: { sumar: SaftSumar }) {
  const h = sumar.Header;
  if (!h) return null;
  return (
    <table className="tabel-mic saft__antet">
      <tbody>
        <tr>
          <th>Declarant</th>
          <td>{h.Name || <span className="indiciu">(fără denumire)</span>}</td>
          <th>Cod de identificare</th>
          <td>{h.RegistrationNumber || <span className="indiciu">(fără CUI)</span>}</td>
        </tr>
        <tr>
          <th>Perioada</th>
          <td>{`${h.PeriodStart}/${h.PeriodStartYear} — ${h.PeriodEnd}/${h.PeriodEndYear}`}</td>
          <th>Bază contabilă</th>
          <td>{h.TaxAccountingBasis}</td>
        </tr>
        <tr>
          <th>Regiune / țară</th>
          <td>{[h.AuditFileRegion, h.AuditFileCountry].filter(Boolean).join(' · ')}</td>
          <th>Versiune / segment / modul</th>
          <td>{`${h.AuditFileVersion} · ${h.SegmentIndex}/${h.TotalSegmentsInSequence} · ${h.HeaderComment}`}</td>
        </tr>
      </tbody>
    </table>
  );
}

// Rezumatul per secțiune. TOATE cifrele vin de pe server: contoarele din sumar
// (calculate de `SaftProiectii.Sumar`, funcție pură pe declarație), banii din
// `Rezumat` — nimic nu se numără și nimic nu se adună în TS. Cele două coloane de
// sumă își schimbă înțelesul de la rând la rând (debit/credit la jurnale,
// net/brut la facturi), de aceea capul de tabel le numește pe amândouă.
function Sectiuni({ sumar, rezumat }: { sumar: SaftSumar; rezumat: Rezumat }) {
  const randuri: { nume: string; intrari: number; linii?: number; suma1?: number; suma2?: number }[] = [
    { nume: 'Conturi (GeneralLedgerAccounts)', intrari: sumar.Conturi ?? 0 },
    { nume: 'Clienți', intrari: sumar.Clienti ?? 0 },
    { nume: 'Furnizori', intrari: sumar.Furnizori ?? 0 },
    { nume: 'Coduri de taxă', intrari: sumar.CoduriTaxa ?? 0 },
    { nume: 'Unități de măsură', intrari: sumar.Unitati ?? 0 },
    { nume: 'Produse', intrari: sumar.Produse ?? 0 },
    { nume: 'Tipuri de analiză', intrari: sumar.TipuriAnaliza ?? 0 },
    {
      nume: `Jurnale (${sumar.Jurnale ?? 0}) — tranzacții`,
      intrari: sumar.Tranzactii ?? 0,
      linii: sumar.LiniiGl ?? 0,
      suma1: rezumat.TotalDebit,
      suma2: rezumat.TotalCredit,
    },
    {
      nume: 'Facturi emise',
      intrari: sumar.FacturiEmise ?? 0,
      suma1: rezumat.NetTotalEmise,
      suma2: rezumat.GrossTotalEmise,
    },
    {
      nume: 'Facturi primite',
      intrari: sumar.FacturiPrimite ?? 0,
      suma1: rezumat.NetTotalPrimite,
      suma2: rezumat.GrossTotalPrimite,
    },
    { nume: 'Plăți', intrari: sumar.Plati ?? 0, suma2: rezumat.TotalPlati },
  ];
  return (
    <div className="saft__sectiune">
      <h3>Secțiunile declarației</h3>
      <table className="tabel-mic">
        <thead>
          <tr>
            <th>Secțiune</th>
            <th>Intrări</th>
            <th>Linii</th>
            <th>Debit / Net</th>
            <th>Credit / Brut</th>
          </tr>
        </thead>
        <tbody>
          {randuri.map((r) => (
            <tr key={r.nume}>
              <td>{r.nume}</td>
              <td className="num">{numar(r.intrari)}</td>
              <td className="num">{r.linii == null ? '' : numar(r.linii)}</td>
              <td className="num">{r.suma1 == null ? '' : bani(r.suma1)}</td>
              <td className="num">{r.suma2 == null ? '' : bani(r.suma2)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

// Secțiunile modulului S. Aceeași disciplină: contoarele sunt ale serverului,
// coloanele de sumă își schimbă înțelesul de la rând la rând (cantitate/valoare
// la stocul fizic, recepționat/eliberat la mișcări), de aceea capul le numește.
function SectiuniStoc({ sumar, rezumat }: { sumar: SaftSumar; rezumat: Rezumat }) {
  const randuri: { nume: string; intrari: number; linii?: number; suma1?: number; suma2?: number;
    cant?: boolean }[] = [
    { nume: 'Conturi (GeneralLedgerAccounts)', intrari: sumar.Conturi ?? 0 },
    { nume: 'Coduri de taxă', intrari: sumar.CoduriTaxa ?? 0 },
    { nume: 'Unități de măsură', intrari: sumar.Unitati ?? 0 },
    { nume: 'Tipuri de analiză', intrari: sumar.TipuriAnaliza ?? 0 },
    { nume: 'Tipuri de mișcare (MovementTypeTable)', intrari: sumar.TipuriMiscare ?? 0 },
    { nume: 'Produse', intrari: sumar.Produse ?? 0 },
    {
      nume: 'Stoc fizic (PhysicalStock) — per gestiune × lot',
      intrari: sumar.StocFizic ?? 0,
      suma1: rezumat.StocClosingCantitate,
      suma2: rezumat.StocClosingValoare,
      cant: true,
    },
    {
      nume: 'Mișcări de bunuri (MovementOfGoods)',
      intrari: sumar.MiscariStoc ?? 0,
      linii: sumar.LiniiMiscare ?? 0,
      suma1: rezumat.MiscariCantitate,
      suma2: rezumat.MiscariValoare,
      cant: true,
    },
  ];
  return (
    <div className="saft__sectiune">
      <h3>Secțiunile declarației</h3>
      <table className="tabel-mic">
        <thead>
          <tr>
            <th>Secțiune</th>
            <th>Intrări</th>
            <th>Linii</th>
            <th>Cantitate (închidere / mișcată)</th>
            <th>Valoare</th>
          </tr>
        </thead>
        <tbody>
          {randuri.map((r) => (
            <tr key={r.nume}>
              <td>{r.nume}</td>
              <td className="num">{numar(r.intrari)}</td>
              <td className="num">{r.linii == null ? '' : numar(r.linii)}</td>
              <td className="num">{r.suma1 == null ? '' : cantitate(r.suma1)}</td>
              <td className="num">{r.suma2 == null ? '' : bani(r.suma2)}</td>
            </tr>
          ))}
        </tbody>
      </table>
      <p className="indiciu">
        Secțiunile lunarului (jurnale, facturi, plăți, clienți, furnizori) sunt <strong>goale</strong> în
        declarația de stocuri, iar <em>Owners</em> rămâne gol fiindcă bunurile sunt proprii. Fără nicio
        intrare de stoc fizic, fișierul <strong>nu se poate genera</strong>: validatorul cere secțiunea
        <em> PhysicalStock</em> prezentă.
      </p>
    </div>
  );
}

// CUSĂTURILE (D16-D4). Fiecare e o egalitate pe care proiecția o afirmă și pe
// care serverul o trimite cu AMBII termeni — clientul nu recalculează nimic, doar
// arată cele două cifre și dacă sunt egale. O cusătură care nu bate rămâne
// VIZIBILĂ: „diferă" cu cifra diferenței, nu o excepție care ascunde restul.
function Cusaturi({ rezumat }: { rezumat: Rezumat }) {
  const zero = 0;
  const lista: { nume: string; explicatie: string; stanga: number; dreapta: number }[] = [
    {
      nume: 'Partidă dublă',
      explicatie: 'Σ debit == Σ credit pe liniile de jurnal',
      stanga: rezumat.TotalDebit ?? zero,
      dreapta: rezumat.TotalCredit ?? zero,
    },
    {
      nume: 'Jurnalele contra registrului',
      explicatie: 'Σ debit == Σ valoare semnată din registrul contabil (rândurile cu document)',
      stanga: rezumat.TotalDebit ?? zero,
      dreapta: rezumat.ValoareRegistruContabil ?? zero,
    },
    {
      nume: 'TVA (trei termeni)',
      explicatie: 'TVA din jurnal + TVA capitalizat + TVA fără cod SAF-T == TVA din registrul fiscal',
      stanga: (rezumat.TvaGl ?? zero) + (rezumat.TvaCapitalizat ?? zero) + (rezumat.TvaFaraCodSaft ?? zero),
      dreapta: rezumat.TvaRegistru ?? zero,
    },
    {
      nume: 'Baza facturilor — achiziții',
      explicatie: 'Σ bază pe facturile primite + Σ bază neinclusă == Σ bază din registrul fiscal',
      stanga: (rezumat.BazaFacturiAchizitie ?? zero) + (rezumat.BazaNeincluseAchizitie ?? zero),
      dreapta: rezumat.BazaRegistruAchizitie ?? zero,
    },
    {
      nume: 'Baza facturilor — livrări',
      explicatie: 'Σ bază pe facturile emise + Σ bază neinclusă == Σ bază din registrul fiscal',
      stanga: (rezumat.BazaFacturiLivrare ?? zero) + (rezumat.BazaNeincluseLivrare ?? zero),
      dreapta: rezumat.BazaRegistruLivrare ?? zero,
    },
    {
      nume: 'Solduri finale',
      explicatie: 'Σ solduri de închidere din conturi == balanța, net',
      stanga: rezumat.ClosingGla ?? zero,
      dreapta: rezumat.ClosingBalanta ?? zero,
    },
  ];
  return (
    <div className="saft__sectiune">
      <h3>Cusăturile (probele de completitudine)</h3>
      <table className="tabel-mic">
        <thead>
          <tr>
            <th>Cusătură</th>
            <th>Din declarație</th>
            <th>Din registru</th>
            <th>Diferență</th>
            <th>Stare</th>
            <th>Ce verifică</th>
          </tr>
        </thead>
        <tbody>
          {lista.map((c) => {
            const diferenta = c.stanga - c.dreapta;
            const egal = Math.abs(diferenta) < 0.005;
            return (
              <tr key={c.nume} className={egal ? undefined : 'saft__difera'}>
                <td>{c.nume}</td>
                <td className="num">{bani(c.stanga)}</td>
                <td className="num">{bani(c.dreapta)}</td>
                <td className="num">{egal ? '' : <strong>{bani(diferenta)}</strong>}</td>
                <td>{egal ? 'egal' : <strong>diferă</strong>}</td>
                <td>{c.explicatie}</td>
              </tr>
            );
          })}
        </tbody>
      </table>
      <p className="indiciu">
        Cusăturile sunt calculate de proiecție, pe aceleași date din care se scrie fișierul. „Diferă"
        nu oprește generarea — e un fapt măsurat, care spune unde s-a pierdut o cifră.
      </p>
    </div>
  );
}

// Cusăturile modulului S (D17-D3). S1 și S2 sunt egalități în DOUĂ unități
// (cantitate și valoare), iar verdictul lor NU e comparația celor două totaluri:
// serverul îl trimite gata dat (`StocFizicBate` e per INTRARE — Σ ar putea bate
// global cu două intrări greșite în sens opus). Clientul arată verdictul
// serverului și cifrele care-l explică. S4 nu e o sumă, ci o numărătoare de
// referințe rupte — de aceea are rândurile lui, cu contoare.
function CusaturiStoc({ rezumat }: { rezumat: Rezumat }) {
  const zero = 0;
  const sume: {
    nume: string; explicatie: string; stanga: number; dreapta: number; bate: boolean; cant?: boolean;
  }[] = [
    {
      nume: 'S1 stoc fizic vs registru (valoare)',
      explicatie: 'deschidere + rândurile de registru ale lunii == închidere, pe FIECARE intrare '
        + '(gestiune × lot)',
      stanga: (rezumat.StocOpeningValoare ?? zero) + (rezumat.StocMiscariValoare ?? zero),
      dreapta: rezumat.StocClosingValoare ?? zero,
      bate: (rezumat.StocIntrariDiferite ?? 0) === 0,
    },
    {
      nume: 'S1 stoc fizic vs registru (cantitate)',
      explicatie: 'aceeași egalitate, pe cantități',
      stanga: (rezumat.StocOpeningCantitate ?? zero) + (rezumat.StocMiscariCantitate ?? zero),
      dreapta: rezumat.StocClosingCantitate ?? zero,
      bate: (rezumat.StocIntrariDiferite ?? 0) === 0,
      cant: true,
    },
    {
      nume: 'S5 stoc fizic vs fișier (valoare)',
      explicatie: 'aceeași egalitate, dar cu Σ luată din liniile EMISE în MovementOfGoods — S1 confruntă '
        + 'trei interogări pe registru între ele, deci nu vede ce s-a scris în fișier',
      stanga: (rezumat.StocOpeningValoare ?? zero) + (rezumat.StocEmiseValoare ?? zero),
      dreapta: rezumat.StocClosingValoare ?? zero,
      bate: (rezumat.StocFizicVsMiscariDiferite ?? 0) === 0,
    },
    {
      nume: 'S5 stoc fizic vs fișier (cantitate)',
      explicatie: 'aceeași egalitate, pe cantități',
      stanga: (rezumat.StocOpeningCantitate ?? zero) + (rezumat.StocEmiseCantitate ?? zero),
      dreapta: rezumat.StocClosingCantitate ?? zero,
      bate: (rezumat.StocFizicVsMiscariDiferite ?? 0) === 0,
      cant: true,
    },
    {
      nume: 'S2 nimic nu se pierde (valoare)',
      explicatie: 'Σ mișcări + Σ excluse deliberat + Σ neincluse == Σ registrul de stoc al lunii '
        + '(toate tipurile de stoc)',
      stanga: (rezumat.MiscariValoare ?? zero) + (rezumat.ExcluseValoare ?? zero)
        + (rezumat.NeincluseStocValoare ?? zero),
      dreapta: rezumat.RegistruStocValoare ?? zero,
      bate: rezumat.RegistruStocBate ?? false,
    },
    {
      nume: 'S2 nimic nu se pierde (cantitate)',
      explicatie: 'aceeași egalitate, pe cantități',
      stanga: (rezumat.MiscariCantitate ?? zero) + (rezumat.ExcluseCantitate ?? zero)
        + (rezumat.NeincluseStocCantitate ?? zero),
      dreapta: rezumat.RegistruStocCantitate ?? zero,
      bate: rezumat.RegistruStocBate ?? false,
      cant: true,
    },
  ];
  const referinte: { nume: string; rupte: number; total: number; explicatie: string }[] = [
    {
      nume: 'S4 produse (referite / nedeclarate)',
      rupte: rezumat.ProduseLipsa ?? 0,
      total: rezumat.ProduseReferite ?? 0,
      explicatie: 'fiecare `ProductCode` din mișcări și din stocul fizic există în secțiunea Products',
    },
    {
      nume: 'S4 coduri de mișcare (folosite / nedeclarate)',
      rupte: rezumat.CoduriMiscareLipsa ?? 0,
      total: rezumat.CoduriMiscareFolosite ?? 0,
      explicatie: 'fiecare cod folosit pe o mișcare există în MovementTypeTable',
    },
    {
      nume: 'S4 identități de terț (invalide)',
      rupte: rezumat.IdentitatiTertInvalide ?? 0,
      total: rezumat.IdentitatiTertInvalide ?? 0,
      explicatie: 'fiecare CustomerID/SupplierID ≠ 0 are unul dintre formatele 00–08 ale legii',
    },
    {
      nume: 'S4 referințe de mișcare (duplicate)',
      rupte: rezumat.ReferinteDuplicate ?? 0,
      total: rezumat.NumarMiscari ?? 0,
      explicatie: 'MovementReference e identitatea mișcării în fișier, deci trebuie să fie unică — '
        + 'numerele de document duplicate primesc un discriminant',
    },
  ];
  return (
    <div className="saft__sectiune">
      <h3>Cusăturile (probele de completitudine)</h3>
      <table className="tabel-mic">
        <thead>
          <tr>
            <th>Cusătură</th>
            <th>Din declarație</th>
            <th>Din registru</th>
            <th>Diferență</th>
            <th>Stare</th>
            <th>Ce verifică</th>
          </tr>
        </thead>
        <tbody>
          {sume.map((c) => {
            const diferenta = c.stanga - c.dreapta;
            const fmt = c.cant ? cantitate : bani;
            return (
              <tr key={c.nume} className={c.bate ? undefined : 'saft__difera'}>
                <td>{c.nume}</td>
                <td className="num">{fmt(c.stanga)}</td>
                <td className="num">{fmt(c.dreapta)}</td>
                <td className="num">{c.bate ? '' : <strong>{fmt(diferenta)}</strong>}</td>
                <td>{c.bate ? 'egal' : <strong>diferă</strong>}</td>
                <td>{c.explicatie}</td>
              </tr>
            );
          })}
          {referinte.map((r) => (
            <tr key={r.nume} className={r.rupte === 0 ? undefined : 'saft__difera'}>
              <td>{r.nume}</td>
              <td className="num">{numar(r.total)}</td>
              <td className="num">{numar(r.rupte)}</td>
              <td className="num">{r.rupte === 0 ? '' : <strong>{numar(r.rupte)}</strong>}</td>
              <td>{r.rupte === 0 ? 'egal' : <strong>diferă</strong>}</td>
              <td>{r.explicatie}</td>
            </tr>
          ))}
        </tbody>
      </table>
      <p className="indiciu">
        S1 și S5 se verifică <strong>pe fiecare intrare</strong> de stoc fizic (
        {numar(rezumat.StocIntrari ?? 0)} intrări, {numar(rezumat.StocIntrariDiferite ?? 0)} diferite față de
        registru, {numar(rezumat.StocFizicVsMiscariDiferite ?? 0)} față de fișier), nu doar pe total — două
        erori de semn opus s-ar anula într-o singură sumă. Validatorul ANAF nu face nicio aritmetică pe
        declarația de stocuri: aceste cusături sunt singura probă că nu s-a pierdut nimic.
      </p>
    </div>
  );
}

// S3 — stocul fizic contra balanței, PER CONT. Se raportează, NU e blocantă:
// registrul contabil poartă conturile de stoc și din note contabile sau din
// solduri de deschidere fără lot, care n-au ce căuta în stocul fizic. E un fapt
// de citit înainte de depunere, nu un refuz.
//
// Simbolul contului e un LINK către fișa lui pe perioada declarației: „raportată,
// nu blocantă" cere ca omul să poată răspunde la „de unde vine diferența", iar
// singurul răspuns e lista rândurilor contului pe luna aceea. Perioada nu se
// calculează aici — sunt `DataStart`/`DataEnd` ale sumarului, adică exact
// intervalul pe care s-au făcut cifrele de deasupra.
//
// `ContId` gol (`Guid.Empty`) = niciun cont în spatele simbolului (diferența e a
// unei grupări fără cont propriu): fără id nu există fișă, deci nu se pune un
// link care ar deschide un ecran gol.
function StocPerCont({ lista, rezumat, sumar }:
  { lista: DiferentaCont[]; rezumat: Rezumat; sumar: SaftSumar }) {
  if (lista.length === 0) return null;
  return (
    <div className="saft__sectiune">
      <h3>
        S3 — stoc fizic contra balanței, per cont ({numar(rezumat.ConturiStocDiferite ?? 0)} din{' '}
        {numar(rezumat.ConturiStocVerificate ?? 0)} diferite)
      </h3>
      <p className="indiciu">
        <strong>Raportată, nu blocantă.</strong> Diferența e legitimă: contul de stoc din balanță poate
        purta și note contabile sau solduri de deschidere fără lot, care nu au corespondent în stocul
        fizic per gestiune × lot. Se citește ca să se știe <em>cât</em> și <em>pe ce cont</em>.
      </p>
      <table className="tabel-mic">
        <thead>
          <tr>
            <th>Cont</th>
            <th>Închidere stoc fizic</th>
            <th>Închidere balanță</th>
            <th>Diferență</th>
          </tr>
        </thead>
        <tbody>
          {lista.map((c) => (
            <Fragment key={c.Cont ?? ''}>
              <tr className={(c.Diferenta ?? 0) === 0 ? undefined : 'saft__difera'}>
                <td>{c.ContId && c.ContId !== GUID_GOL
                  ? (
                    <Link to={urlCu('/fisa-cont', {
                      contId: c.ContId,
                      dataStart: sumar.DataStart,
                      dataEnd: sumar.DataEnd,
                    })}
                    >
                      {c.Cont}
                    </Link>
                  )
                  : c.Cont}
                </td>
                <td className="num">{bani(c.ClosingStocFizic ?? 0)}</td>
                <td className="num">{bani(c.ClosingBalanta ?? 0)}</td>
                <td className="num">
                  {(c.Diferenta ?? 0) === 0 ? '' : <strong>{bani(c.Diferenta ?? 0)}</strong>}
                </td>
              </tr>
              {/* Componentele apar DOAR sub conturile care chiar diferă: pe unul
                  care se închide la zero ar fi zgomot. Ele spun CE TIP de
                  document pune diferența — singura formă în care cifra devine
                  acționabilă. */}
              {(c.Diferenta ?? 0) !== 0 && (c.Componente ?? []).map((comp) => (
                <tr key={`${c.Cont ?? ''}|${comp.TipDocument ?? ''}`} className="saft__componenta">
                  <td>↳ {comp.TipDocument}</td>
                  <td className="num">{bani(comp.StocFizic ?? 0)}</td>
                  <td className="num">{bani(comp.Balanta ?? 0)}</td>
                  <td className="num">{bani(comp.Diferenta ?? 0)}</td>
                </tr>
              ))}
            </Fragment>
          ))}
          <tr>
            <th>Total</th>
            <td className="num">{bani(rezumat.ClosingStocFizic ?? 0)}</td>
            <td className="num">{bani(rezumat.ClosingBalantaStoc ?? 0)}</td>
            <td className="num">
              {bani((rezumat.ClosingStocFizic ?? 0) - (rezumat.ClosingBalantaStoc ?? 0))}
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  );
}

// `Excluse` ≠ `Neincluse`, și distincția e chiar contractul feliei: aici sunt
// rândurile pe care POLITICA le lasă deliberat afară, cu motivul scris de om
// (consumul pe responsabil nu e stoc în magazie). Lista trece ÎNTREAGĂ prin
// sumar — e mărginită de numărul de politici, nu de volumul registrului.
function Excluse({ lista }: { lista: Exclus[] }) {
  if (lista.length === 0) return null;
  return (
    <div className="d300__neincluse">
      <h3>Excluse deliberat ({numar(lista.length)})</h3>
      <p className="indiciu">
        Rânduri de registru pe care o politică de mișcare le lasă <strong>intenționat</strong> în afara
        declarației, cu motivul ei. Cifrele lor intră în cusătura S2 ca termen separat — nu se pierd,
        doar nu se declară.
      </p>
      <table className="tabel-mic">
        <thead>
          <tr>
            <th>Tip document</th>
            <th>Tip stoc</th>
            <th>Semn</th>
            <th>Motiv</th>
            <th>Rânduri</th>
            <th>Cantitate</th>
            <th>Valoare</th>
          </tr>
        </thead>
        <tbody>
          {lista.map((x, i) => (
            <tr key={`${x.TipDocument ?? ''}|${x.TipStoc ?? ''}|${x.Semn ?? ''}|${i}`}>
              <td>{x.TipDocument}</td>
              <td>{labelEnum('TipStoc', x.TipStoc)}</td>
              <td className="num">{x.Semn == null ? '±' : semn(x.Semn)}</td>
              <td>{x.Motiv}</td>
              <td className="num">{numar(x.Numar ?? 0)}</td>
              <td className="num">{cantitate(x.Cantitate ?? 0)}</td>
              <td className="num">{bani(x.Valoare ?? 0)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

// Nimic nu se pierde (62f): ce n-a putut intra în fișier apare aici cu cauza.
//
// AGREGAT PER CAUZĂ, ca la D394 și ca avertismentele de mai jos. Tabelul plat de
// până acum era plafonat la 200 de rânduri și pe o bază reală spunea „afișate
// primele 200 din 41.000" — adică taman întrebarea la care nu răspundea: CÂTE
// sunt pe fiecare cauză și cât fac. Acum întregul e vizibil (cauzele sunt
// mărginite de enum, nu de volumul registrului), iar cazurile nominale sunt la
// un clic, pliate în `<details>`, cu aceleași coloane ca înainte.
//
// Coloanele exemplelor diferă între module fiindcă și cifrele diferă: pe L un
// rând neinclus are bază/TVA/debit/credit, pe S are produs, tip de stoc,
// cantitate și valoare. A afișa coloanele lunarului pe stocuri ar fi însemnat
// șapte celule goale.
function Neincluse({ lista, fel }: { lista: NeinclusAgregat[]; fel: Fel }) {
  if (lista.length === 0) return null;
  const stoc = fel === 'S';
  // O NUMĂRĂTOARE de cazuri peste contoarele serverului, nu o cifră contabilă
  // (42c privește sold/rest/total, iar cusăturile de mai sus adună deja termeni
  // trimiși de server). Totalul trebuie să existe: altfel titlul ar spune „4"
  // acolo unde sunt 41.000 de rânduri, adică ar minți despre mărimea găurii.
  const cazuri = lista.reduce((s, n) => s + (n.Numar ?? 0), 0);
  const coloane = stoc ? 5 : 4;
  return (
    <div className="d300__neincluse">
      <h3>
        Neincluse în declarație ({numar(cazuri)} în {numar(lista.length)}{' '}
        {lista.length === 1 ? 'cauză' : 'cauze'})
      </h3>
      <p className="indiciu">
        {stoc
          ? 'Rânduri ale registrului de stoc pentru care nu există politică de mișcare sau cont de stoc '
            + 'pe produs — deci cifrele lor nu se regăsesc în secțiunile de sus, dar intră în cusătura S2 '
            + 'ca termen separat. O politică nouă („Politici → Mișcări SAF-T") le aduce în fișier fără '
            + 'schimbare de cod.'
          : 'Rânduri, documente sau solduri ale perioadei care nu au unde să cadă în structura D406 — '
            + 'deci cifrele lor nu se regăsesc în secțiunile de sus, dar intră în cusăturile de mai sus '
            + 'ca termen separat.'}
      </p>
      <table className="tabel-mic">
        <thead>
          <tr>
            <th>Cauză</th>
            <th>Cazuri</th>
            <th>Rânduri</th>
            {stoc && <th>Cantitate</th>}
            <th>{stoc ? 'Valoare' : 'Sumă'}</th>
          </tr>
        </thead>
        <tbody>
          {lista.map((n) => (
            <Fragment key={n.Cauza ?? ''}>
              <tr>
                <td><strong>{labelEnum('CauzaNeincludere', n.Cauza)}</strong></td>
                <td className="num">{numar(n.Numar ?? 0)}</td>
                <td className="num">{numar(n.Randuri ?? 0)}</td>
                {stoc && (
                  <td className="num">{n.Cantitate == null ? '' : cantitate(n.Cantitate)}</td>
                )}
                <td className="num">{bani(n.Suma ?? 0)}</td>
              </tr>
              {(n.Exemple?.length ?? 0) > 0 && (
                <tr className="saft__exemple">
                  <td colSpan={coloane}>
                    <details>
                      <summary>
                        {n.Exemple!.length < (n.Numar ?? 0)
                          ? `Primele ${numar(n.Exemple!.length)} din ${numar(n.Numar ?? 0)}`
                          : 'Cazurile'}
                      </summary>
                      <ExempleNeincluse lista={n.Exemple!} stoc={stoc} />
                    </details>
                  </td>
                </tr>
              )}
            </Fragment>
          ))}
        </tbody>
      </table>
    </div>
  );
}

// Cazurile nominale ale unei cauze — exact tabelul de dinainte de agregare,
// mutat sub `<details>`. Coloana „Cauză" lipsește: e scrisă pe rândul care le
// deschide.
function ExempleNeincluse({ lista, stoc }: { lista: Neinclus[]; stoc: boolean }) {
  return (
    <table className="tabel-mic">
      <thead>
        <tr>
          <th>Secțiune</th>
          {stoc ? <th>Document</th> : <th>Sens</th>}
          {stoc ? <th>Produs</th> : <th>Document</th>}
          {stoc ? <th>Tip stoc</th> : <th>Cont</th>}
          {stoc ? <th>Semn</th> : <th>Repartitor</th>}
          {stoc ? <th>Cantitate</th> : <th>Bază</th>}
          {stoc ? <th>Valoare</th> : <th>TVA</th>}
          {!stoc && <th>Debit</th>}
          {!stoc && <th>Credit</th>}
          <th>Rânduri</th>
        </tr>
      </thead>
      <tbody>
        {lista.map((n, i) => (
          <tr key={`${n.DocumentId ?? ''}|${n.ContId ?? ''}|${n.RepartitorId ?? ''}|${i}`}>
            <td>{n.Sectiune}</td>
            {stoc ? (
              <>
                <td>{[n.DocumentTip, n.DocumentNumar].filter(Boolean).join(' ')}</td>
                <td>{n.ProdusCod}</td>
                <td>{labelEnum('TipStoc', n.TipStoc)}</td>
                <td className="num">{n.Semn == null ? '±' : semn(n.Semn)}</td>
                <td className="num">{n.Cantitate == null ? '' : cantitate(n.Cantitate)}</td>
                <td className="num">{n.Valoare == null ? '' : bani(n.Valoare)}</td>
              </>
            ) : (
              <>
                <td>{labelEnum('SensTva', n.Sens)}</td>
                <td>{[n.DocumentTip, n.DocumentNumar].filter(Boolean).join(' ')}</td>
                <td>{n.ContSimbol}</td>
                <td>{n.RepartitorDenumire}</td>
                <td className="num">{n.Baza == null ? '' : bani(n.Baza)}</td>
                <td className="num">{n.Tva == null ? '' : bani(n.Tva)}</td>
                <td className="num">{n.Debit == null ? '' : bani(n.Debit)}</td>
                <td className="num">{n.Credit == null ? '' : bani(n.Credit)}</td>
              </>
            )}
            <td className="num">{numar(n.Randuri ?? 0)}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

// Avertismentele, AGREGATE per cauză pe server (aceeași formă ca la D394): un
// rând per cod, cu numărul de cazuri, suma și exemplele nominale pliate.
function Avertismente({ lista }: { lista: Avertisment[] }) {
  if (lista.length === 0) return null;
  return (
    <div className="d394__avertismente">
      <h3>Avertismente ({lista.length} {lista.length === 1 ? 'cauză' : 'cauze'})</h3>
      <p className="indiciu">
        Ce cere formularul și modelul nu are. Fișierul se generează oricum — cu valoarea de rezervă
        pe care o spune mesajul; validatorul ANAF poate să nu fie la fel de îngăduitor.
      </p>
      <table className="tabel-mic">
        <thead>
          <tr>
            <th>Cauză</th>
            <th>Cazuri</th>
            <th>Sumă</th>
            <th>Descriere</th>
          </tr>
        </thead>
        <tbody>
          {lista.map((a) => (
            <tr key={a.Cod ?? ''}>
              <td><strong>{labelEnum('CodAvertismentSaft', a.Cod)}</strong></td>
              <td className="num">{numar(a.Numar ?? 0)}</td>
              <td className="num">{a.Suma == null ? '' : bani(a.Suma)}</td>
              <td>
                {a.Mesaj}
                {(a.Exemple?.length ?? 0) > 0 && (
                  <details>
                    <summary>
                      {a.Exemple!.length === a.Numar
                        ? 'Cazurile'
                        : `Primele ${a.Exemple!.length} din ${numar(a.Numar ?? 0)}`}
                    </summary>
                    <ul>{a.Exemple!.map((e, i) => <li key={i}>{e}</li>)}</ul>
                  </details>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function bani(v: number): string {
  return v.toLocaleString('ro-RO', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

// Cantitățile au 3 zecimale (scara 18,3 — decizia 49e), iar zecimalele
// nesemnificative nu se taie: „2" și „2,000" sunt aceeași cantitate, dar numai a
// doua se citește ca o cantitate.
function cantitate(v: number): string {
  return v.toLocaleString('ro-RO', { minimumFractionDigits: 3, maximumFractionDigits: 3 });
}

function numar(v: number): string {
  return v.toLocaleString('ro-RO');
}

// Semnul unei politici de mișcare: `+1` intrare, `−1` ieșire, absent = orice.
function semn(v: number): string {
  return v > 0 ? '+' : v < 0 ? '−' : '0';
}
