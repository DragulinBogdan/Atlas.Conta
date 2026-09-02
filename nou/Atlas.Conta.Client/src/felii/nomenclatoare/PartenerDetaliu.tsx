import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { ConfirmareInline } from '../../nucleu/ConfirmareInline';
import { CampShell } from '../../nucleu/CampShell';
import { Formular } from '../../nucleu/formular';
import { CampBifa, CampSelectie, CampText } from '../../nucleu/campuri';
import { Lookup } from '../../nucleu/Lookup';
import { campMeta } from '../../nucleu/campMeta';
import { EroareIndisponibil, eroriDin, posteaza } from '../../nucleu/http';
import type { components } from '../../generated/api-types';
import { ShellNomenclator, type ComandaNomenclator } from './ShellNomenclator';
import { corpScriere, creeazaRand, invalideaza, modificaRand, stergeRand, useRand } from './api';

// Ecranul de partener (72-r9) — primul ecran de nomenclator cu formular.
//
// Trei lucruri îi sunt PROPRII, toate din felia 15:
//
//  1. **Județul e al adreselor din România** (72b). Gardianul refuză județ pe
//     `Tara != RO` (SAF-T validează `Region` contra ISO 3166-2:RO), deci
//     lookup-ul e AFORDANȚĂ: se dezactivează, iar la schimbarea țării din RO
//     valoarea se golește — un câmp activ care duce garantat la 422 e o capcană.
//     Refuzul rămâne al serverului; ecranul doar nu te împinge în el.
//  2. **Timbrul ANAF e server-owned** (`DataSincronizareAnaf`, `InactivFiscal`,
//     72a): se AFIȘEAZĂ, nu se culege. Un PATCH pe ele iese 422 (probat) — deci
//     nici nu pleacă: `corpScriere` compune corpul din câmpurile formularului,
//     iar ele nu sunt în listă.
//  3. **Sincronizarea din ANAF e o COMANDĂ pe partener** (72e), nu o salvare:
//     `POST api/parteneri/{id}/sincronizeaza-anaf`. Raportul ei — ce s-a
//     schimbat, ce diferă, ce n-a putut fi decis — se arată ÎNTREG. „Gol se
//     umple, diferit se raportează, canonicul bate" (72d): fără `suprascrie`,
//     ANAF-ul completează doar câmpurile goale și RAPORTEAZĂ diferențele; cu
//     `suprascrie`, le înlocuiește. De aceea a doua e o comandă separată, cu
//     confirmare — nu o bifă lângă prima.

type Partener = components['schemas']['Partener'];
type Sincronizare = components['schemas']['SincronizareAnafDto'];

// Câmpurile pe care le stăpânește formularul. Ce nu e aici nu poate pleca pe
// sârmă: `ID`, `Cautare` (coloană generată), `Calitati`/`ContImplicitId`
// (nefolosite de rolul de partener — rolul e dat de poziția pe document, 16) și
// cele două server-owned de mai sus.
const CAMPURI = [
  'Cod', 'Denumire', 'CodFiscal', 'RegistruComert', 'TipPersoana', 'Tara',
  'InregistratTva', 'TvaLaIncasare', 'Activ',
  'Strada', 'Numar', 'DetaliiAdresa', 'Localitate', 'CodPostal', 'JudetId',
] as const;

type Formularul = Partial<Pick<Partener, (typeof CAMPURI)[number]>>;

function gol(): Formularul {
  // `Tara` implicit RO: setterul serverului face oricum gol ⇒ RO, iar valoarea
  // vizibilă de la început e cea care decide dacă județul e activ.
  return { Tara: 'RO', TipPersoana: 'Juridica', Activ: true, InregistratTva: false, TvaLaIncasare: false };
}

function dinCitit(p: Partener): Formularul {
  const f: Record<string, unknown> = {};
  for (const c of CAMPURI) f[c] = (p as Record<string, unknown>)[c] ?? undefined;
  return f as Formularul;
}

export function PartenerDetaliu() {
  const { id } = useParams();
  const nou = id === 'nou' || id === undefined;
  const navigheaza = useNavigate();
  const cache = useQueryClient();

  const citit = useRand<Partener>('Partener', nou ? undefined : id);
  const [valoare, setValoare] = useState<Formularul>(gol);
  const [erori, setErori] = useState<string[]>([]);
  const [mesaje, setMesaje] = useState<string[]>([]);
  // O SINGURĂ cerere în așteptare (F20-D4): ștergerea și suprascrierea din ANAF
  // se exclud, deci împart o stare, nu două booleene care pot fi ambele true.
  const [confirmare, setConfirmare] = useState<'sterge' | 'suprascrie' | null>(null);
  const [raport, setRaport] = useState<Sincronizare | null>(null);
  // Ultima sincronizare căzută pe 503, ca butonul „Reia" să știe CE reia
  // (suprascrie sau nu). `null` = nimic de reluat.
  const [deReluat, setDeReluat] = useState<boolean | null>(null);

  useEffect(() => {
    if (citit.data) setValoare(dinCitit(citit.data));
  }, [citit.data]);

  const areJudet = valoare.Tara === 'RO';

  function schimba(v: Formularul) {
    // Țara plecată din RO ⇒ județul se golește ODATĂ cu ea, în aceeași scriere.
    // Golirea pleacă pe sârmă ca `null` (`corpScriere`), nu ca tăcere: un PATCH
    // fără câmp ar fi lăsat județul pe server și refuzul ar fi venit la salvare.
    setValoare(v.Tara === 'RO' ? v : { ...v, JudetId: undefined });
  }

  const salvare = useMutation({
    mutationFn: async () => {
      const corp = corpScriere(valoare, [...CAMPURI]);
      if (nou) {
        const creat = await creeazaRand<Partener>('Partener', corp);
        return String(creat.ID);
      }
      await modificaRand('Partener', id!, corp);
      return id!;
    },
    onSuccess: (idSalvat) => {
      setErori([]);
      setMesaje(['Salvat.']);
      invalideaza(cache, 'Partener', idSalvat);
      if (nou) navigheaza(`/parteneri/${idSalvat}`, { replace: true });
      else void citit.refetch();
    },
    onError: (e) => { setMesaje([]); setErori(eroriDin(e)); },
  });

  const stergere = useMutation({
    mutationFn: () => stergeRand('Partener', id!),
    onSuccess: () => {
      // Navigarea ÎNAINTEA invalidării, în ordinea asta: invalidarea ar reface
      // citirea rândului cât timp ecranul e încă montat, iar rândul tocmai a
      // dispărut — un `GET …(id)` ⇒ 404 inutil, pe drum spre listă. Demontat,
      // n-are cine reface.
      navigheaza('/parteneri', { replace: true });
      invalideaza(cache, 'Partener', id);
    },
    onError: (e) => { setConfirmare(null); setMesaje([]); setErori(eroriDin(e)); },
  });

  const sincronizare = useMutation({
    mutationFn: (suprascrie: boolean) =>
      posteaza<Sincronizare>(`/api/parteneri/${id}/sincronizeaza-anaf?suprascrie=${suprascrie}`),
    onMutate: () => { setConfirmare(null); setErori([]); setMesaje([]); setRaport(null); setDeReluat(null); },
    onSuccess: (r) => {
      setRaport(r);
      // Sincronizarea SCRIE pe partener: rândul citit e vechi, iar etichetele lui
      // din lookup-urile deschise la fel.
      invalideaza(cache, 'Partener', id);
      void citit.refetch();
    },
    onError: (e, suprascrie) => {
      setErori(eroriDin(e));
      // 503 = registrul n-a răspuns ACUM (72e). Nu e un refuz: reîncercarea are
      // sens, iar butonul apare doar aici. Orice altă eroare rămâne fără „Reia".
      if (e instanceof EroareIndisponibil) setDeReluat(suprascrie);
    },
  });

  const ocupat = salvare.isPending || stergere.isPending || sincronizare.isPending;
  const comenzi: ComandaNomenclator[] = [
    { eticheta: 'Salvează', disponibila: !ocupat, primara: true, ruleaza: () => salvare.mutate() },
    {
      eticheta: sincronizare.isPending ? 'Sincronizez…' : 'Sincronizează din ANAF',
      // Comanda e pe partenerul SALVAT: ruta cere id-ul lui, iar serviciul scrie
      // pe rândul comis. Pe `/nou` nu există ce sincroniza.
      disponibila: !nou && !ocupat,
      ruleaza: () => sincronizare.mutate(false),
    },
    {
      eticheta: 'Suprascrie din ANAF…',
      disponibila: !nou && !ocupat,
      ruleaza: () => setConfirmare('suprascrie'),
    },
    { eticheta: 'Șterge', disponibila: !nou && !ocupat, ruleaza: () => setConfirmare('sterge') },
  ];

  return (
    <ShellNomenclator
      citire={citit}
      titlu={nou ? 'Partener nou' : (citit.data?.Denumire ?? 'Partener')}
      sumar={!nou && citit.data?.InactivFiscal ? <span className="sumar__stare">Inactiv fiscal</span> : undefined}
      comenzi={comenzi}
      erori={erori}
      mesaje={mesaje}
      ocupat={ocupat}
      confirmare={confirmare == null ? undefined : (
        <ConfirmareInline
          intrebare={confirmare === 'sterge'
            ? <>Se șterge partenerul <b>{citit.data?.Denumire ?? ''}</b>. Documentele care îl referă rămân, dar
              nomenclatorul nu-l mai oferă.</>
            : <>Se cer datele de la ANAF și se ÎNLOCUIESC valorile culese care diferă (denumire, adresă).
              Fără suprascriere, ANAF completează doar câmpurile goale și raportează diferențele.</>}
          verb={confirmare === 'sterge' ? 'Șterge' : 'Suprascrie'}
          ocupat={ocupat}
          onConfirma={() => (confirmare === 'sterge' ? stergere.mutate() : sincronizare.mutate(true))}
          onRenunta={() => setConfirmare(null)}
        />
      )}
      rezultat={
        <>
          {deReluat !== null && (
            <div className="cerere-data">
              <span>Registrul ANAF n-a răspuns. Cererea era bună — se poate relua.</span>
              <button
                type="button"
                className="buton buton--primar"
                disabled={ocupat}
                onClick={() => sincronizare.mutate(deReluat)}
              >
                Reia
              </button>
              <button type="button" className="buton" onClick={() => setDeReluat(null)}>Renunță</button>
            </div>
          )}
          {raport && <RaportAnaf raport={raport} />}
        </>
      }
    >
      <Formular tip="Partener" schema="Partener" valoare={valoare} onSchimba={schimba} aratErori>
        <div className="grila-campuri">
          <CampText<Formularul> camp="Cod" />
          <CampText<Formularul> camp="Denumire" />
          <CampText<Formularul> camp="CodFiscal" eticheta="Cod fiscal" />
          <CampText<Formularul> camp="RegistruComert" eticheta="Nr. registrul comerțului" />
          <CampSelectie<Formularul> camp="TipPersoana" enumerare="TipPersoana" />
          {/* Cod ISO-2; setterul serverului normalizează (trim + majuscule, gol
              ⇒ RO), iar gardianul refuză „ROM"/„Germania". */}
          <CampText<Formularul> camp="Tara" />
          <CampBifa<Formularul> camp="InregistratTva" />
          <CampBifa<Formularul> camp="TvaLaIncasare" />
          <CampBifa<Formularul> camp="Activ" />
        </div>

        <h3 className="nomenclator__titlu-grup">Adresă</h3>
        <div className="grila-campuri">
          <CampText<Formularul> camp="Strada" />
          <CampText<Formularul> camp="Numar" />
          <CampText<Formularul> camp="DetaliiAdresa" />
          <CampText<Formularul> camp="Localitate" />
          <CampText<Formularul> camp="CodPostal" />
          {areJudet
            ? <Lookup<Formularul> camp="JudetId" entitate="Judet" mod="local" afisare={codSiDenumire} />
            : (
              <CampShell meta={{ ...campMeta('Partener', 'JudetId', 'Partener'), obligatoriu: false }}>
                <div className="valoare-statica">
                  — județul e al adreselor din România (țara e „{valoare.Tara ?? ''}")
                </div>
              </CampShell>
            )}
        </div>

        <h3 className="nomenclator__titlu-grup">Registrul ANAF</h3>
        <div className="grila-campuri">
          {/* Se AFIȘEAZĂ, nu se culeg: le scrie doar serviciul de sincronizare
              (72a). Un câmp read-only care arată ce spune registrul e altceva
              decât un câmp gri pe care operatorul crede că-l poate forța. */}
          {/* `obligatoriu: false` explicit: schema OData le dă non-nullable
              (`DateTime?` e nullable, dar `bool` nu), iar `CampShell` ar pune
              asteriscul de „se cere". Pe un câmp care se AFIȘEAZĂ, asteriscul ar
              cere ceva ce operatorul n-are cum să dea. */}
          <CampShell meta={{ ...campMeta('Partener', 'DataSincronizareAnaf', 'Partener'), obligatoriu: false }}>
            <div className="valoare-statica">{dataOra(citit.data?.DataSincronizareAnaf) || '— niciodată'}</div>
          </CampShell>
          <CampShell meta={{ ...campMeta('Partener', 'InactivFiscal', 'Partener'), obligatoriu: false }}>
            <div className="valoare-statica">{citit.data?.InactivFiscal ? 'Da' : 'Nu'}</div>
          </CampShell>
        </div>
      </Formular>
    </ShellNomenclator>
  );
}

// ── raportul sincronizării ──────────────────────────────────────────────────

// `Camp` e NUMELE proprietății (convenția din `ParteneriDtos.cs`): serverul
// trimite identitatea câmpului, clientul îi pune eticheta operatorului. Un
// server care ar fi trimis textul tradus ar fi mutat captions-urile în două
// locuri.
const etichetaCamp = (camp: string | null | undefined) =>
  camp ? campMeta('Partener', camp, 'Partener').caption : '';

function RaportAnaf({ raport }: { raport: Sincronizare }) {
  const modificari = raport.Modificari ?? [];
  const diferente = raport.Diferente ?? [];
  const avertismente = raport.Avertismente ?? [];

  return (
    <div className="panou panou--succes">
      <div className="panou__titlu">
        Sincronizare ANAF{raport.Cui != null ? ` — CUI ${raport.Cui}` : ''}
      </div>
      {!raport.Gasit && (
        <p>
          CUI-ul nu figurează în registrul interogat (`notFound`) — nu e o eroare, e răspunsul
          registrului. Nimic nu s-a schimbat, iar partenerul rămâne fără timbru de sincronizare.
        </p>
      )}
      {raport.Gasit && modificari.length === 0 && diferente.length === 0 && (
        <p>Nimic de schimbat: datele culese coincid cu registrul (sau registrul nu are ce completa).</p>
      )}

      {modificari.length > 0 && (
        <>
          <div className="panou__titlu">Modificate</div>
          <table className="tabel-mic">
            <thead><tr><th>Câmp</th><th>Vechi</th><th>Nou</th></tr></thead>
            <tbody>
              {modificari.map((m, i) => (
                <tr key={i}>
                  <td>{etichetaCamp(m.Camp)}</td>
                  <td>{m.Vechi}</td>
                  <td>{m.Nou}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </>
      )}

      {diferente.length > 0 && (
        <>
          <div className="panou__titlu">Diferite — RAPORTATE, nu schimbate</div>
          <table className="tabel-mic">
            <thead><tr><th>Câmp</th><th>Cules</th><th>ANAF</th></tr></thead>
            <tbody>
              {diferente.map((d, i) => (
                <tr key={i}>
                  <td>{etichetaCamp(d.Camp)}</td>
                  <td>{d.Cules}</td>
                  <td>{d.Anaf}</td>
                </tr>
              ))}
            </tbody>
          </table>
          <p className="indiciu">
            „Gol se umple, diferit se raportează, canonicul bate": ca să le înlocuiți cu valorile
            ANAF, folosiți „Suprascrie din ANAF…".
          </p>
        </>
      )}

      {avertismente.length > 0 && (
        <>
          <div className="panou__titlu">Atenționări</div>
          <ul className="panou__lista">{avertismente.map((a, i) => <li key={i}>{a}</li>)}</ul>
        </>
      )}
    </div>
  );
}

function codSiDenumire(e: Record<string, unknown>): string {
  const cod = e?.Cod == null ? '' : String(e.Cod);
  const denumire = e?.Denumire == null ? '' : String(e.Denumire);
  return cod && denumire ? `${cod} — ${denumire}` : cod || denumire;
}

// Timbrul vine ca ISO din server; se afișează în forma cu care lucrează
// operatorul. Formatarea e a AFIȘĂRII — valoarea nu se atinge.
function dataOra(iso: string | null | undefined): string {
  if (!iso) return '';
  const d = new Date(iso);
  return Number.isNaN(d.getTime()) ? String(iso) : d.toLocaleString('ro-RO');
}
