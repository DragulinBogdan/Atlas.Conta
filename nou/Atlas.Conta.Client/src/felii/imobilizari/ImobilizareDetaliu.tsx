import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { useNavigate, useParams } from 'react-router';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { CampShell } from '../../nucleu/CampShell';
import { ConfirmareInline } from '../../nucleu/ConfirmareInline';
import { Formular, eroriStructurale } from '../../nucleu/formular';
import { CampText } from '../../nucleu/campuri';
import { Lookup } from '../../nucleu/Lookup';
import { PanouErori } from '../../nucleu/PanouErori';
import { campMeta, labelEnum } from '../../nucleu/campMeta';
import { eroriDin } from '../../nucleu/http';
import { ziLocala } from '../../nucleu/zi';
import { codSiDenumire } from '../politici/comune';
import { ShellNomenclator, type ComandaNomenclator } from '../nomenclatoare/ShellNomenclator';
import { corpScriere, creeazaRand, invalideaza, modificaRand, stergeRand, useRand } from '../nomenclatoare/api';
import { PanouFisa } from './PanouFisa';
import { TIP, type Imobilizare } from './api';

// Fișa e nomenclator subțire: identitate, loc, clasificare. `Stare`,
// `DataPunereInFunctiune` și `DataIesire` le scrie motorul la operarea PIF / CAS
// (F26-D3), deci se afișează, nu intră în corpul scrierii.

const CAMPURI = [
  'NumarInventar', 'Denumire', 'TipMaterialId', 'ClasificareId',
  'LocId', 'CentruCostId', 'CodEconomicId', 'ResponsabilId',
] as const;

type Formularul = Partial<Pick<Imobilizare, (typeof CAMPURI)[number]>>;

// Clasa F: doar tipurile de material de natura „Imobilizare” pot purta o fișă.
const DOAR_IMOBILIZARI: unknown[] = ['Clasa.Natura', '=', 'Imobilizare'];

function dinCitit(f: Imobilizare): Formularul {
  const valori: Record<string, unknown> = {};
  for (const c of CAMPURI) valori[c] = (f as Record<string, unknown>)[c] ?? undefined;
  return valori as Formularul;
}

export function ImobilizareDetaliu() {
  const { id } = useParams();
  const nou = id === 'nou' || id === undefined;
  const navigheaza = useNavigate();
  const cache = useQueryClient();

  const citit = useRand<Imobilizare>(TIP, nou ? undefined : id);
  const [valoare, setValoare] = useState<Formularul>({});
  const [aratErori, setAratErori] = useState(false);
  const [erori, setErori] = useState<string[]>([]);
  const [mesaje, setMesaje] = useState<string[]>([]);
  const [deSters, setDeSters] = useState(false);

  useEffect(() => {
    if (citit.data) setValoare(dinCitit(citit.data));
  }, [citit.data]);

  const structurale = useMemo(() => [
    ...eroriStructurale(TIP, TIP, valoare as Record<string, unknown>, [...CAMPURI]),
    // Schema lasă denumirea nullable, fișa fără ea n-ar avea etichetă în niciun
    // mesaj al motorului.
    ...(valoare.Denumire?.trim() ? [] : [`„${campMeta(TIP, 'Denumire', TIP).caption}” este obligatoriu.`]),
  ], [valoare]);

  const salvare = useMutation({
    mutationFn: async () => {
      const corp = corpScriere(valoare, [...CAMPURI]);
      if (nou) {
        const creat = await creeazaRand<Imobilizare>(TIP, corp);
        return String(creat.ID);
      }
      await modificaRand(TIP, id!, corp);
      return id!;
    },
    onSuccess: (idSalvat) => {
      setErori([]);
      setMesaje(['Salvat.']);
      invalideaza(cache, TIP, idSalvat);
      if (nou) navigheaza(`/imobilizari/${idSalvat}`, { replace: true });
      else void citit.refetch();
    },
    onError: (e) => { setMesaje([]); setErori(eroriDin(e)); },
  });

  const stergere = useMutation({
    mutationFn: () => stergeRand(TIP, id!),
    onSuccess: () => {
      navigheaza('/imobilizari', { replace: true });
      invalideaza(cache, TIP, id);
    },
    onError: (e) => { setDeSters(false); setMesaje([]); setErori(eroriDin(e)); },
  });

  const ocupat = salvare.isPending || stergere.isPending;
  const doc = citit.data;

  function salveaza() {
    setAratErori(true);
    if (structurale.length > 0) { setErori([]); setMesaje([]); return; }
    salvare.mutate();
  }

  const comenzi: ComandaNomenclator[] = [
    { eticheta: nou ? 'Creează' : 'Salvează', disponibila: !ocupat, primara: true, ruleaza: salveaza },
    // Butonul rămâne oferit și pe o fișă pusă în funcțiune: refuzul e al
    // gardianului (422), cu motivul lui, nu al ecranului.
    { eticheta: 'Șterge', disponibila: !nou && !ocupat, ruleaza: () => setDeSters(true) },
    { eticheta: 'Înapoi la listă', disponibila: !ocupat, ruleaza: () => navigheaza('/imobilizari') },
  ];

  return (
    <ShellNomenclator
      citire={citit}
      titlu={nou ? 'Fișă de imobilizare nouă' : (doc?.NumarInventar || doc?.Denumire || 'Fișă de imobilizare')}
      comenzi={comenzi}
      erori={erori}
      mesaje={mesaje}
      ocupat={ocupat}
      confirmare={deSters ? (
        <ConfirmareInline
          intrebare={<>Se șterge fișa <b>{doc?.NumarInventar || doc?.Denumire || ''}</b>. O fișă
            care a fost mișcată de un document nu se șterge — motorul o refuză.</>}
          verb="Șterge fișa"
          ocupat={ocupat}
          onConfirma={() => stergere.mutate()}
          onRenunta={() => setDeSters(false)}
        />
      ) : undefined}
      rezultat={aratErori ? <PanouErori erori={structurale} titlu="De completat înainte de salvare" /> : undefined}
    >
      <Formular tip={TIP} schema={TIP} valoare={valoare} onSchimba={setValoare} aratErori={aratErori}>
        <div className="grila-campuri">
          <CampText<Formularul> camp="NumarInventar" />
          <CampText<Formularul> camp="Denumire" obligatoriu />
          <Lookup<Formularul>
            camp="TipMaterialId"
            entitate="TipMaterial"
            mod="remote"
            expand={['Clasa']}
            filtru={DOAR_IMOBILIZARI}
            afisare={codSiDenumire}
          />
          <Lookup<Formularul>
            camp="ClasificareId"
            entitate="ClasificareImobilizari"
            mod="remote"
            afisare={codSiDenumire}
          />
          <Lookup<Formularul> camp="LocId" entitate="Repartitor" mod="remote" afisare={codSiDenumire} />
          <Lookup<Formularul> camp="CentruCostId" entitate="Repartitor" mod="remote" afisare={codSiDenumire} />
          <Lookup<Formularul> camp="CodEconomicId" entitate="CodEconomic" mod="local" afisare={codSiDenumire} />
          <Lookup<Formularul> camp="ResponsabilId" entitate="Angajat" mod="local" afisare={codSiDenumire} />
        </div>
      </Formular>

      {!nou && (
        <div className="grila-campuri">
          <Static membru="Stare" valoare={labelEnum('StareImobilizare', doc?.Stare)} />
          <Static membru="DataPunereInFunctiune" valoare={ziLocala(doc?.DataPunereInFunctiune)} />
          <Static membru="DataIesire" valoare={ziLocala(doc?.DataIesire)} />
        </div>
      )}

      <p className="indiciu">
        Fișa e nomenclator: starea și datele de punere în funcțiune / ieșire le scrie motorul, la
        operarea documentelor PIF și CAS. Tipul (contul) se schimbă doar cât fișa e Nouă — după
        punerea în funcțiune el a intrat deja în politica de amortizare și în note.
      </p>

      {!nou && <PanouFisa id={id!} />}
    </ShellNomenclator>
  );
}

// Câmp de AFIȘARE cu caption din metadata, pentru membrii scriși de motor.
function Static(props: { membru: string; valoare: ReactNode }) {
  const meta = { ...campMeta(TIP, props.membru, TIP), obligatoriu: false };
  return (
    <CampShell meta={meta}>
      <div className="valoare-statica">
        {props.valoare == null || props.valoare === '' ? '—' : props.valoare}
      </div>
    </CampShell>
  );
}
