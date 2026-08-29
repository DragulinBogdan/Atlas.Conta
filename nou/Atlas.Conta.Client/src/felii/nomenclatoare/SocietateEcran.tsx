import { useEffect, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { CampShell } from '../../nucleu/CampShell';
import { Formular } from '../../nucleu/formular';
import { CampBifa, CampOptiuni, CampText } from '../../nucleu/campuri';
import { Lookup } from '../../nucleu/Lookup';
import { campMeta, nomenclator } from '../../nucleu/campMeta';
import { eroriDin, ia } from '../../nucleu/http';
import type { components } from '../../generated/api-types';
import { ShellNomenclator, type ComandaNomenclator } from './ShellNomenclator';
import { corpScriere, creeazaRand, invalideaza, modificaRand } from './api';

// Societatea raportoare (73-r6): UN SINGUR rând, deci un singur ecran — fără
// listă și fără `/nou`. Gardianul refuză al doilea rând (D16-D1: „societatea
// care raportează" e unică prin definiție; două rânduri ar face `IdSocietate`
// ambiguu), deci ecranul nici nu oferă butonul.
//
// Citirea e `GET api/odata/Societate?$top=1`: existența rândului decide verbul
// — `POST` dacă nu există, `PATCH` dacă există. Seed-ul îl creează GOL (73a), nu
// îl rescrie: pe o bază proaspătă ecranul găsește un rând cu totul necompletat,
// și asta e starea corectă, nu o eroare.
//
// Ce nu e aici: adresa are ACELEAȘI câmpuri și aceleași lungimi ca la partener
// (`AdresaSaft.Lungimi`, 73a) — lungimile vin din schema OpenAPI a entității,
// nu dintr-o copie în TS.

type Societate = components['schemas']['Societate'];
type BazaContabila = { Cod: string; Descriere: string };

const CAMPURI = [
  'Denumire', 'CodFiscal', 'RegistruComert', 'InregistratTva', 'Tara',
  'Strada', 'Numar', 'DetaliiAdresa', 'Localitate', 'CodPostal', 'JudetId',
  'ContactNume', 'ContactPrenume', 'Telefon', 'Email',
  'ContBancarId', 'BazaContabila', 'RaporteazaCnp',
] as const;

type Formularul = Partial<Pick<Societate, (typeof CAMPURI)[number]>>;

function dinCitit(s: Societate): Formularul {
  const f: Record<string, unknown> = {};
  for (const c of CAMPURI) f[c] = (s as Record<string, unknown>)[c] ?? undefined;
  return f as Formularul;
}

export function SocietateEcran() {
  const cache = useQueryClient();
  const [valoare, setValoare] = useState<Formularul>({ Tara: 'RO' });
  const [erori, setErori] = useState<string[]>([]);
  const [mesaje, setMesaje] = useState<string[]>([]);

  const citit = useQuery({
    queryKey: ['nomenclator-rand', 'Societate', 'unicul'],
    queryFn: () => ia<{ value?: Societate[] }>('/api/odata/Societate?$top=1'),
  });
  const randul = citit.data?.value?.[0];

  useEffect(() => {
    if (randul) setValoare(dinCitit(randul));
  }, [randul]);

  const areJudet = valoare.Tara === 'RO';

  function schimba(v: Formularul) {
    // Aceeași regulă ca la partener (72b): județul e al adreselor din România.
    setValoare(v.Tara === 'RO' ? v : { ...v, JudetId: undefined });
  }

  const salvare = useMutation({
    mutationFn: async () => {
      const corp = corpScriere(valoare, [...CAMPURI]);
      if (randul?.ID) {
        await modificaRand('Societate', String(randul.ID), corp);
        return;
      }
      await creeazaRand<Societate>('Societate', corp);
    },
    onSuccess: () => {
      setErori([]);
      setMesaje(['Salvat.']);
      invalideaza(cache, 'Societate', 'unicul');
      void citit.refetch();
    },
    onError: (e) => { setMesaje([]); setErori(eroriDin(e)); },
  });

  const comenzi: ComandaNomenclator[] = [
    {
      eticheta: 'Salvează',
      disponibila: !salvare.isPending && !citit.isLoading,
      primara: true,
      ruleaza: () => salvare.mutate(),
    },
  ];

  // Lista e LEGE ÎN COD pe server (`Societate.BazeContabile`, 73d/74b) și ajunge
  // în client prin `metadata.json` (F20-D6), nu printr-o interogare: n-are ușă
  // OData fiindcă nu e entitate. Codul e ce merge în fișier
  // (`TaxAccountingBasis`); descrierea e pentru operator.
  const baze = nomenclator<BazaContabila>('BazeContabile')
    .map((b) => ({ valoare: b.Cod, label: `${b.Cod} — ${b.Descriere}` }));

  return (
    <ShellNomenclator
      titlu="Societatea raportoare"
      comenzi={comenzi}
      erori={erori}
      mesaje={mesaje}
      ocupat={salvare.isPending}
      sumar={randul == null && !citit.isLoading
        ? <span className="sumar__stare">Niciun rând — se creează la prima salvare</span>
        : undefined}
    >
      <Formular tip="Societate" schema="Societate" valoare={valoare} onSchimba={schimba} aratErori>
        <div className="grila-campuri">
          <CampText<Formularul> camp="Denumire" />
          <CampText<Formularul> camp="CodFiscal" />
          <CampText<Formularul> camp="RegistruComert" />
          <CampBifa<Formularul> camp="InregistratTva" />
          <CampText<Formularul> camp="Tara" />
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
              <CampShell meta={{ ...campMeta('Societate', 'JudetId', 'Societate'), obligatoriu: false }}>
                <div className="valoare-statica">
                  — județul e al adreselor din România (țara e „{valoare.Tara ?? ''}")
                </div>
              </CampShell>
            )}
        </div>

        <h3 className="nomenclator__titlu-grup">Contact</h3>
        <div className="grila-campuri">
          <CampText<Formularul> camp="ContactNume" />
          <CampText<Formularul> camp="ContactPrenume" />
          <CampText<Formularul> camp="Telefon" />
          <CampText<Formularul> camp="Email" />
        </div>

        <h3 className="nomenclator__titlu-grup">Raportare</h3>
        <div className="grila-campuri">
          {/* Doar conturile proprii marcate ca bancare: `PaymentMethod`-ul din
              SAF-T se citește din contul societății, iar o casierie n-are IBAN. */}
          <Lookup<Formularul>
            camp="ContBancarId"
            entitate="ContPropriu"
            mod="local"
            filtru={['EsteBanca', '=', true]}
            afisare={codSiDenumire}
          />
          <CampOptiuni<Formularul>
            camp="BazaContabila"
            optiuni={baze}
            substitut="Alegeți baza contabilă"
            textFaraDate="Lista lipsește din metadata.json"
          />
          <CampBifa<Formularul> camp="RaporteazaCnp" />
        </div>
        <p className="indiciu">
          „Raportează CNP-ul persoanelor fizice": bifat, persoanele fizice ies în SAF-T cu tipul de
          identificare al CNP-ului; debifat, ies ca „04" cu un cod intern — datele personale nu
          părăsesc baza. Alegerea e a societății, nu a fișierului.
        </p>
      </Formular>
    </ShellNomenclator>
  );
}

function codSiDenumire(e: Record<string, unknown>): string {
  const cod = e?.Cod == null ? '' : String(e.Cod);
  const denumire = e?.Denumire == null ? '' : String(e.Denumire);
  return cod && denumire ? `${cod} — ${denumire}` : cod || denumire;
}
