import { useEffect, useMemo, useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { PanouErori } from '../../nucleu/PanouErori';
import { eroriDin } from '../../nucleu/http';
import { labelEnum } from '../../nucleu/campMeta';
import { useUrlStare } from '../../nucleu/urlStare';
import { perioade, etichetaPerioada, type ConstatareInchidere, type Perioada } from './api';

// Lanțul perioadelor și cele două comenzi ale lui (F27-D1/D2) — o consolă, nu o
// listă cu buton „Nouă”: perioada nu se culege, iar `Inchisa` nu e o bifă de
// editat, e rezultatul unei comenzi verificate.
//
// Tiparul e cel al consolei ITV: parametrul (luna aleasă) sus, VERDICTUL
// serverului dedesubt, comanda la capăt. Diferența: aici verdictul nu e un
// singur motiv, e o LISTĂ de constatări, iar avertismentele se acceptă pe cheie.
// Nimic nu se decide în TS — severitatea vine din politică, iar „se poate
// închide” e verdictul comenzii, nu o condiție reconstruită din listă.

const LUNI = [
  'ianuarie', 'februarie', 'martie', 'aprilie', 'mai', 'iunie',
  'iulie', 'august', 'septembrie', 'octombrie', 'noiembrie', 'decembrie',
];

export function Perioade() {
  const cache = useQueryClient();
  const acum = new Date();
  // Luna e STARE DE URL (43c): „închiderea pe ianuarie” trebuie să fie un link.
  const [stare, seteaza] = useUrlStare({
    an: String(acum.getFullYear()),
    luna: String(acum.getMonth() + 1),
  });
  const an = Number(stare.an);
  const luna = Number(stare.luna);

  const lant = useQuery({ queryKey: ['perioade', 'lant'], queryFn: () => perioade.lant() });
  const verificare = useQuery({
    queryKey: ['perioade', 'verificare', an, luna],
    queryFn: () => perioade.verificare(an, luna),
  });
  const istoric = useQuery({
    queryKey: ['perioade', 'istoric', an, luna],
    queryFn: () => perioade.istoric(an, luna),
  });

  const [acceptate, setAcceptate] = useState<Record<string, boolean>>({});
  const [motiv, setMotiv] = useState('');
  const [ocupat, setOcupat] = useState(false);
  const cerereCurenta = `${an}-${luna}`;
  // Rezultatul poartă și CEREREA pentru care s-a produs (tiparul ITV): altfel
  // refuzul lunii ianuarie ar rămâne pe ecran după trecerea pe februarie.
  const [rezultat, setRezultat] = useState<{ erori: string[]; mesaje: string[]; cerere: string }>(
    { erori: [], mesaje: [], cerere: '' });
  const alRandului = rezultat.cerere === cerereCurenta;

  // Bifele sunt ale lunii afișate: la schimbarea ei pornesc de la zero, ca o
  // acceptare dată pe altă lună să nu se strecoare în comanda de acum.
  useEffect(() => { setAcceptate({}); setMotiv(''); }, [cerereCurenta]);

  const constatari = verificare.data ?? [];
  const blocante = constatari.filter((c) => c.Severitate === 'Blocant');
  const avertismente = constatari.filter((c) => c.Severitate === 'Avertisment');
  const toateAcceptate = avertismente.every((c) => acceptate[c.Cheie ?? ''] === true);

  const perioadaCurenta = useMemo(
    () => (lant.data ?? []).find((p) => p.An === an && p.Luna === luna),
    [lant.data, an, luna]);

  async function comanda(actiune: () => Promise<unknown>, reusita: string) {
    const cerere = cerereCurenta;
    setOcupat(true);
    setRezultat({ erori: [], mesaje: [], cerere });
    try {
      await actiune();
      await cache.invalidateQueries({ queryKey: ['perioade'] });
      setRezultat({ erori: [], mesaje: [reusita], cerere });
    }
    catch (e) {
      setRezultat({ erori: eroriDin(e), mesaje: [], cerere });
    }
    finally {
      setOcupat(false);
    }
  }

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>Perioade fiscale</h2>
      </div>

      <div className="bara-raport">
        <label className="bara-raport__camp">
          <span className="camp__eticheta">An</span>
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
        <button
          type="button"
          className="buton"
          disabled={verificare.isFetching}
          onClick={() => void verificare.refetch()}
        >
          {verificare.isFetching ? 'Se verifică…' : 'Verifică'}
        </button>
        <button
          type="button"
          className="buton buton--primar"
          disabled={ocupat || perioadaCurenta?.Inchisa !== false
            || blocante.length > 0 || !toateAcceptate}
          onClick={() => void comanda(
            () => perioade.inchide(an, luna, avertismente.map((c) => c.Cheie ?? '')),
            `Perioada ${String(luna).padStart(2, '0')}/${an} a fost închisă.`)}
        >
          {ocupat ? 'Se lucrează…' : 'Închide'}
        </button>
      </div>

      <PanouErori
        erori={verificare.error ? eroriDin(verificare.error) : []}
        titlu="Luna nu se poate verifica"
      />
      <PanouErori erori={alRandului ? rezultat.erori : []} titlu="Refuzat de server" />
      <PanouErori erori={alRandului ? rezultat.mesaje : []} titlu="Rezultat" fel="succes" />

      <section className="itv__sectiune">
        <h3>{`Verificarea lunii ${String(luna).padStart(2, '0')}/${an}`}</h3>
        {verificare.isPending
          ? <p className="indiciu">Se verifică…</p>
          : <Constatari
              blocante={blocante}
              avertismente={avertismente}
              acceptate={acceptate}
              bifeaza={(cheie, val) => setAcceptate((a) => ({ ...a, [cheie]: val }))}
            />}
      </section>

      {perioadaCurenta?.Inchisa === true && (
        <section className="itv__sectiune">
          <h3>Redeschidere</h3>
          <p className="indiciu">
            Se redeschide doar ULTIMA perioadă închisă, cu motiv scris. Redeschiderea șterge
            soldurile materializate ale lunii și partidele ei deschise, apoi le reconstruiește pe
            cele ale lunii dinainte.
          </p>
          <div className="bara-raport">
            <label className="bara-raport__camp bara-raport__camp--lat">
              <span className="camp__eticheta">Motiv</span>
              <input
                type="text"
                value={motiv}
                onChange={(e) => setMotiv(e.target.value)}
                placeholder="De ce se redeschide luna"
              />
            </label>
            <button
              type="button"
              className="buton"
              disabled={ocupat || motiv.trim().length === 0}
              onClick={() => void comanda(
                () => perioade.redeschide(an, luna, motiv),
                `Perioada ${String(luna).padStart(2, '0')}/${an} a fost redeschisă.`)}
            >
              Redeschide
            </button>
          </div>
        </section>
      )}

      <section className="itv__sectiune">
        <h3>Istoricul lunii</h3>
        {(istoric.data ?? []).length === 0
          ? <p className="indiciu">Luna n-a fost încă închisă niciodată.</p>
          : (
            <table className="tabel-mic">
              <thead>
                <tr><th>Când</th><th>Fel</th><th>Cine</th><th>Motiv</th><th>Acceptări</th></tr>
              </thead>
              <tbody>
                {(istoric.data ?? []).map((r, i) => (
                  <tr key={`${r.La}-${i}`}>
                    <td>{r.La ? new Date(r.La).toLocaleString('ro-RO') : ''}</td>
                    <td>{labelEnum('FelInchiderePerioada', r.Fel) || r.Fel}</td>
                    <td>{r.De ?? ''}</td>
                    <td>{r.Motiv ?? ''}</td>
                    <td>{(r.Acceptari ?? []).join(', ')}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
      </section>

      <section className="itv__sectiune">
        <h3>Lanțul</h3>
        {lant.isError && <PanouErori erori={eroriDin(lant.error)} titlu="Lanțul nu se poate citi" />}
        <table className="tabel-mic">
          <thead>
            <tr><th>Perioada</th><th>Închisă</th><th>Închisă la</th><th>Închisă prima oară</th><th /></tr>
          </thead>
          <tbody>
            {(lant.data ?? []).map((p: Perioada) => (
              <tr key={`${p.An}-${p.Luna}`} className={p.An === an && p.Luna === luna ? 'rand--activ' : ''}>
                <td>{etichetaPerioada(p)}</td>
                <td>{p.Inchisa ? 'da' : 'nu'}</td>
                <td>{p.InchisaLa ? new Date(p.InchisaLa).toLocaleString('ro-RO') : ''}</td>
                <td>{p.InchisaPrimaOara ? new Date(p.InchisaPrimaOara).toLocaleString('ro-RO') : ''}</td>
                <td>
                  <button
                    type="button"
                    className="buton buton--mic"
                    onClick={() => seteaza({ an: String(p.An), luna: String(p.Luna) })}
                  >
                    Alege
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>

      <p className="indiciu">
        Perioadele se închid în LANȚ, în ordine: o lună nu se închide cât timp precedenta e
        deschisă, iar o lună care nu există e tratată ca închisă. Blocantele nu se pot accepta;
        avertismentele se acceptă pe cheie, iar cheile acceptate rămân scrise în istoric.
        Severitatea fiecărui fel de constatare e politică editabilă, în „Politici → Închidere de
        perioadă”.
      </p>
    </div>
  );
}

function Constatari({ blocante, avertismente, acceptate, bifeaza }: {
  blocante: ConstatareInchidere[];
  avertismente: ConstatareInchidere[];
  acceptate: Record<string, boolean>;
  bifeaza: (cheie: string, valoare: boolean) => void;
}) {
  if (blocante.length === 0 && avertismente.length === 0)
    return <p className="indiciu">Nicio constatare: luna se poate închide.</p>;

  return (
    <>
      {blocante.length > 0 && (
        <>
          <p className="panou panou--atentie">
            <strong>{blocante.length} blocante.</strong>{' '}
            Un blocant nu se acceptă — se rezolvă.
          </p>
          <table className="tabel-mic">
            <thead><tr><th>Fel</th><th>Obiect</th><th>Constatare</th></tr></thead>
            <tbody>
              {blocante.map((c) => (
                <tr key={c.Cheie}>
                  <td>{labelEnum('FelConstatareInchidere', c.Fel) || c.Fel}</td>
                  <td>{c.ObiectEticheta ?? ''}</td>
                  <td className="verificare__mesaj">{c.Text ?? ''}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </>
      )}

      {avertismente.length > 0 && (
        <>
          <p className="indiciu">
            {avertismente.length} avertismente. Închiderea cere ca fiecare să fie acceptat explicit.
          </p>
          <table className="tabel-mic">
            <thead><tr><th>Accept</th><th>Fel</th><th>Obiect</th><th>Constatare</th></tr></thead>
            <tbody>
              {avertismente.map((c) => (
                <tr key={c.Cheie}>
                  <td>
                    <input
                      type="checkbox"
                      checked={acceptate[c.Cheie ?? ''] === true}
                      onChange={(e) => bifeaza(c.Cheie ?? '', e.target.checked)}
                      aria-label={`Acceptă ${c.Cheie}`}
                    />
                  </td>
                  <td>{labelEnum('FelConstatareInchidere', c.Fel) || c.Fel}</td>
                  <td>{c.ObiectEticheta ?? ''}</td>
                  <td className="verificare__mesaj">{c.Text ?? ''}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </>
      )}
    </>
  );
}

function ani(anCurent: number): number[] {
  const lista: number[] = [];
  for (let a = anCurent + 1; a >= 2020; a--)
    lista.push(a);
  return lista;
}
