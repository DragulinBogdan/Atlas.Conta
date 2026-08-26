import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import type { components } from '../../generated/api-types';
import { labelEnum } from '../../nucleu/campMeta';
import { descarcaFisier, eroriDin, ia } from '../../nucleu/http';
import { PanouErori } from '../../nucleu/PanouErori';
import { urlCu, useUrlStare } from '../../nucleu/urlStare';

// SAF-T — declarația D406, modul L (felia 16, D16-D6). Al treilea ecran de
// „formular peste registre", după D300 și D394, și primul care are un FIȘIER.
//
// Ce arată, în ordinea în care se citește o declarație înainte de a o depune:
//  (1) antetul — cine declară, pe ce perioadă, pe ce bază contabilă;
//  (2) rezumatul per secțiune — câte intrări are fiecare și cât fac;
//  (3) CUSĂTURILE — probele de completitudine ale proiecției (partidă dublă,
//      TVA în trei termeni, baza facturilor per sens, soldurile contra balanței),
//      fiecare cu verdictul ei vizibil: dacă una nu bate, se vede AICI, nu abia
//      la validatorul ANAF;
//  (4) `Neincluse` — ce n-a putut intra în fișier, cu cauza (nimic nu se pierde);
//  (5) avertismentele agregate — ce cere formularul și modelul n-are.
//
// Ce NU face: nu calculează nicio sumă. Toate cifrele de bani vin din
// `Rezumat`-ul serverului (42c — „TS nu calculează niciodată sold/rest/total");
// clientul doar NUMĂRĂ rânduri de listă și compară două cifre pe care serverul
// le-a trimis pe amândouă.
type SaftDto = components['schemas']['SaftDto'];
type Rezumat = components['schemas']['SaftRezumat'];
type Neinclus = components['schemas']['SaftNeinclus'];
type Avertisment = components['schemas']['SaftAvertisment'];

const LUNI = [
  'ianuarie', 'februarie', 'martie', 'aprilie', 'mai', 'iunie',
  'iulie', 'august', 'septembrie', 'octombrie', 'noiembrie', 'decembrie',
];

// `PeriodYear` are `minInclusive = 2020` în schemă — aceeași margine ca refuzul
// serverului, ca lista să nu ofere ce API-ul respinge.
const AN_MINIM = 2020;

// Câte rânduri de `Neincluse` se randează. Pe scenă sunt câteva; pe o bază de
// producție pot fi mii, iar un tabel HTML de mii de rânduri îngheață pagina
// pentru o informație care se citește oricum pe cauze. Restul nu dispare: se
// spune câte sunt și cauzele se văd toate în rândurile afișate.
const NEINCLUSE_AFISATE = 200;

export function Saft() {
  const acum = new Date();
  const [stare, seteaza] = useUrlStare({
    an: String(acum.getFullYear()),
    luna: String(acum.getMonth() + 1),
  });
  const [descarcare, setDescarcare] = useState<{ activa: boolean; erori: string[] }>(
    { activa: false, erori: [] });

  const cale = urlCu('/api/proiectii/saft', { an: stare.an, luna: stare.luna });
  const citit = useQuery({ queryKey: ['saft', cale], queryFn: () => ia<SaftDto>(cale) });

  const dto = citit.data;
  const rezumat: Rezumat = dto?.Rezumat ?? {};
  const neincluse = dto?.Neincluse ?? [];
  const avertismente = dto?.Avertismente ?? [];

  async function descarca() {
    setDescarcare({ activa: true, erori: [] });
    try {
      // Numele implicit e o plasă: serverul îl trimite în `Content-Disposition`
      // (el știe CUI-ul societății), iar `descarcaFisier` îl preferă pe al lui.
      await descarcaFisier(
        urlCu('/api/proiectii/saft/xml', { an: stare.an, luna: stare.luna }),
        `SAF-T_${stare.an}-${String(stare.luna).padStart(2, '0')}.xml`,
        'application/xml, application/json');
      setDescarcare({ activa: false, erori: [] });
    }
    catch (e) {
      // 403 (fără drept pe registru), 422 (profil bugetar) și orice altceva ies
      // ca listă de mesaje, în același panou ca refuzurile de domeniu.
      setDescarcare({ activa: false, erori: eroriDin(e) });
    }
  }

  return (
    <div className="ecran">
      <div className="ecran__bara"><h2>SAF-T — declarația D406 (lunar)</h2></div>

      <div className="bara-raport">
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
        {dto && (
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
      <PanouErori erori={descarcare.erori} titlu="Fișierul nu s-a putut genera" />

      {citit.isPending ? <p className="indiciu">Se încarcă…</p> : dto && (
        <>
          <Antet dto={dto} />
          <Sectiuni dto={dto} rezumat={rezumat} />
          <Cusaturi rezumat={rezumat} />
          <Neincluse lista={neincluse} />
          <Avertismente lista={avertismente} />
        </>
      )}

      <p className="indiciu">
        Cifrele sunt cele din <strong>registrul contabil</strong> și din <strong>registrul de TVA</strong>,
        așezate pe structura D406 modul <strong>L</strong> (lunar): master files, jurnalele contabile,
        facturile emise și primite, plățile. Declarațiile <strong>S</strong> (mișcări de stocuri) și
        <strong> A</strong> (imobilizări) sunt alte declarații, nu secțiuni opționale ale acesteia.
        Sumele sunt exacte (în bani); rotunjirea la 2 zecimale e a fișierului.
      </p>
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
function Antet({ dto }: { dto: SaftDto }) {
  const h = dto.Header;
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
          <th>Versiune schemă / segment</th>
          <td>{`${h.AuditFileVersion} · ${h.SegmentIndex}/${h.TotalSegmentsInSequence}`}</td>
        </tr>
      </tbody>
    </table>
  );
}

// Rezumatul per secțiune. Numărul de intrări îl NUMĂRĂ clientul (e lungimea unei
// liste pe care o are deja), banii vin din `Rezumat` — niciun total nu se adună
// în TS. Cele două coloane de sumă își schimbă înțelesul de la rând la rând
// (debit/credit la jurnale, net/brut la facturi), de aceea capul de tabel le
// numește pe amândouă.
function Sectiuni({ dto, rezumat }: { dto: SaftDto; rezumat: Rezumat }) {
  const randuri: { nume: string; intrari: number; linii?: number; suma1?: number; suma2?: number }[] = [
    { nume: 'Conturi (GeneralLedgerAccounts)', intrari: dto.Conturi?.length ?? 0 },
    { nume: 'Clienți', intrari: rezumat.NumarClienti ?? 0 },
    { nume: 'Furnizori', intrari: rezumat.NumarFurnizori ?? 0 },
    { nume: 'Coduri de taxă', intrari: dto.Taxe?.length ?? 0 },
    { nume: 'Unități de măsură', intrari: dto.Unitati?.length ?? 0 },
    { nume: 'Produse', intrari: rezumat.NumarProduse ?? 0 },
    { nume: 'Tipuri de analiză', intrari: dto.TipuriAnaliza?.length ?? 0 },
    {
      nume: `Jurnale (${dto.Jurnale?.length ?? 0}) — tranzacții`,
      intrari: rezumat.Tranzactii ?? 0,
      linii: rezumat.LiniiGl ?? 0,
      suma1: rezumat.TotalDebit,
      suma2: rezumat.TotalCredit,
    },
    {
      nume: 'Facturi emise',
      intrari: rezumat.NumarFacturiEmise ?? 0,
      suma1: rezumat.NetTotalEmise,
      suma2: rezumat.GrossTotalEmise,
    },
    {
      nume: 'Facturi primite',
      intrari: rezumat.NumarFacturiPrimite ?? 0,
      suma1: rezumat.NetTotalPrimite,
      suma2: rezumat.GrossTotalPrimite,
    },
    { nume: 'Plăți', intrari: rezumat.NumarPlati ?? 0, suma2: rezumat.TotalPlati },
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

// Nimic nu se pierde (62f): ce n-a putut intra în fișier apare aici cu cauza.
function Neincluse({ lista }: { lista: Neinclus[] }) {
  if (lista.length === 0) return null;
  const afisate = lista.slice(0, NEINCLUSE_AFISATE);
  return (
    <div className="d300__neincluse">
      <h3>Neincluse în declarație ({numar(lista.length)})</h3>
      <p className="indiciu">
        Rânduri, documente sau solduri ale perioadei care nu au unde să cadă în structura D406 —
        deci cifrele lor <strong>nu</strong> se regăsesc în secțiunile de sus, dar intră în cusăturile
        de mai sus ca termen separat.
      </p>
      <table className="tabel-mic">
        <thead>
          <tr>
            <th>Cauză</th>
            <th>Secțiune</th>
            <th>Sens</th>
            <th>Document</th>
            <th>Cont</th>
            <th>Repartitor</th>
            <th>Bază</th>
            <th>TVA</th>
            <th>Debit</th>
            <th>Credit</th>
            <th>Rânduri</th>
          </tr>
        </thead>
        <tbody>
          {afisate.map((n, i) => (
            <tr key={`${n.Cauza}|${n.DocumentId ?? ''}|${n.ContId ?? ''}|${n.RepartitorId ?? ''}|${i}`}>
              <td>{labelEnum('CauzaNeincludere', n.Cauza)}</td>
              <td>{n.Sectiune}</td>
              <td>{labelEnum('SensTva', n.Sens)}</td>
              <td>{[n.DocumentTip, n.DocumentNumar].filter(Boolean).join(' ')}</td>
              <td>{n.ContSimbol}</td>
              <td>{n.RepartitorDenumire}</td>
              <td className="num">{n.Baza == null ? '' : bani(n.Baza)}</td>
              <td className="num">{n.Tva == null ? '' : bani(n.Tva)}</td>
              <td className="num">{n.Debit == null ? '' : bani(n.Debit)}</td>
              <td className="num">{n.Credit == null ? '' : bani(n.Credit)}</td>
              <td className="num">{numar(n.Randuri ?? 0)}</td>
            </tr>
          ))}
        </tbody>
      </table>
      {lista.length > afisate.length && (
        <p className="indiciu">
          Afișate primele {numar(afisate.length)} din {numar(lista.length)} — restul sunt în răspunsul
          JSON al proiecției, cu aceleași cauze.
        </p>
      )}
    </div>
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

function numar(v: number): string {
  return v.toLocaleString('ro-RO');
}
