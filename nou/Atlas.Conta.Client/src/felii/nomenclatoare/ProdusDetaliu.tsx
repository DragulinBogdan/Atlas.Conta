import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { ConfirmareInline } from '../../nucleu/ConfirmareInline';
import { Formular } from '../../nucleu/formular';
import { CampText } from '../../nucleu/campuri';
import { Lookup } from '../../nucleu/Lookup';
import { PanouErori } from '../../nucleu/PanouErori';
import { campMeta } from '../../nucleu/campMeta';
import { eroriDin } from '../../nucleu/http';
import type { components } from '../../generated/api-types';
import { ShellNomenclator, type ComandaNomenclator } from './ShellNomenclator';
import { corpScriere, creeazaRand, invalideaza, modificaRand, stergeRand, useRand } from './api';

// Ecranul de produs (73-r6): identitatea plus cele două câmpuri cerute de
// SAF-T — `CodNc` și unitatea de măsură UN/ECE.
//
// `UM` (string liber) rămâne lângă `UnitateMasura` (nomenclator), fiindcă sunt
// două lucruri diferite: primul e ce scrie pe factură, al doilea e codul cerut
// de fișier. Unificarea lor e 73-r5, o decizie de model, nu de ecran.
//
// ═══ `CodNc`: validare de FORMĂ în client, fond pe server ═══
// Regula XAF `[RuleRegularExpression]` nu rulează pe API (55b) — dar gardianul
// O ARE (D16-D2): un PATCH cu „ABC" iese 422. Validarea de aici nu-l
// înlocuiește și nu-l dublează ca AUTORITATE: e afordanță, ca să nu afli la
// salvare ce se vede la tastare. Verdictul rămâne al serverului.

type Produs = components['schemas']['Produs'];

const CAMPURI = ['Cod', 'Denumire', 'UM', 'TipMaterialId', 'UnitateMasuraId', 'CodNc'] as const;
type Formularul = Partial<Pick<Produs, (typeof CAMPURI)[number]>>;

const FORMA_COD_NC = /^\d{8}$/;

function dinCitit(p: Produs): Formularul {
  const f: Record<string, unknown> = {};
  for (const c of CAMPURI) f[c] = (p as Record<string, unknown>)[c] ?? undefined;
  return f as Formularul;
}

export function ProdusDetaliu() {
  const { id } = useParams();
  const nou = id === 'nou' || id === undefined;
  const navigheaza = useNavigate();
  const cache = useQueryClient();

  const citit = useRand<Produs>('Produs', nou ? undefined : id);
  const [valoare, setValoare] = useState<Formularul>({});
  const [erori, setErori] = useState<string[]>([]);
  const [mesaje, setMesaje] = useState<string[]>([]);
  const [deSters, setDeSters] = useState(false);

  useEffect(() => {
    if (citit.data) setValoare(dinCitit(citit.data));
  }, [citit.data]);

  // Structurala clientului: forma codului NC. Un mesaj, aceeași listă ca a
  // serverului — panoul de erori are un singur mod de randare.
  const structurale = useMemo(() => {
    const cod = valoare.CodNc;
    return cod && !FORMA_COD_NC.test(cod)
      ? [`„${campMeta('Produs', 'CodNc', 'Produs').caption}” are exact 8 cifre (sau rămâne gol) — acum are „${cod}”.`]
      : [];
  }, [valoare.CodNc]);

  const salvare = useMutation({
    mutationFn: async () => {
      const corp = corpScriere(valoare, [...CAMPURI]);
      if (nou) {
        const creat = await creeazaRand<Produs>('Produs', corp);
        return String(creat.ID);
      }
      await modificaRand('Produs', id!, corp);
      return id!;
    },
    onSuccess: (idSalvat) => {
      setErori([]);
      setMesaje(['Salvat.']);
      invalideaza(cache, 'Produs', idSalvat);
      if (nou) navigheaza(`/produse/${idSalvat}`, { replace: true });
      else void citit.refetch();
    },
    onError: (e) => { setMesaje([]); setErori(eroriDin(e)); },
  });

  const stergere = useMutation({
    mutationFn: () => stergeRand('Produs', id!),
    onSuccess: () => {
      // Navigarea ÎNAINTEA invalidării, în ordinea asta: invalidarea ar reface
      // citirea rândului cât timp ecranul e încă montat, iar rândul tocmai a
      // dispărut — un `GET …(id)` ⇒ 404 inutil, pe drum spre listă. Demontat,
      // n-are cine reface.
      navigheaza('/produse', { replace: true });
      invalideaza(cache, 'Produs', id);
    },
    onError: (e) => { setDeSters(false); setMesaje([]); setErori(eroriDin(e)); },
  });

  const ocupat = salvare.isPending || stergere.isPending;

  function salveaza() {
    if (structurale.length > 0) {
      setMesaje([]);
      setErori([]);
      return;
    }
    salvare.mutate();
  }

  const comenzi: ComandaNomenclator[] = [
    { eticheta: 'Salvează', disponibila: !ocupat && structurale.length === 0, primara: true, ruleaza: salveaza },
    { eticheta: 'Șterge', disponibila: !nou && !ocupat, ruleaza: () => setDeSters(true) },
  ];

  return (
    <ShellNomenclator
      citire={citit}
      titlu={nou ? 'Produs nou' : (citit.data?.Denumire ?? 'Produs')}
      comenzi={comenzi}
      erori={erori}
      mesaje={mesaje}
      ocupat={ocupat}
      confirmare={deSters ? (
        <ConfirmareInline
          intrebare={<>Se șterge produsul <b>{citit.data?.Denumire ?? ''}</b>. Loturile și documentele
            care îl referă rămân; nomenclatorul nu-l mai oferă.</>}
          verb="Șterge"
          ocupat={ocupat}
          onConfirma={() => stergere.mutate()}
          onRenunta={() => setDeSters(false)}
        />
      ) : undefined}
      rezultat={<PanouErori erori={structurale} titlu="De corectat" />}
    >
      <Formular tip="Produs" schema="Produs" valoare={valoare} onSchimba={setValoare} aratErori>
        <div className="grila-campuri">
          <CampText<Formularul> camp="Cod" />
          <CampText<Formularul> camp="Denumire" />
          <CampText<Formularul> camp="UM" eticheta="UM (text pe document)" />
          <Lookup<Formularul> camp="TipMaterialId" entitate="TipMaterial" mod="remote" afisare={codSiDenumire} />
          {/* Nomenclatorul UN/ECE are ~120 de rânduri, dar se caută pe cod („H87")
              și pe denumire deopotrivă — deci REMOTE, pe coloana `Cautare`. */}
          <Lookup<Formularul>
            camp="UnitateMasuraId"
            entitate="UnitateMasura"
            mod="remote"
            afisare={codSiDenumire}
          />
          <CampText<Formularul> camp="CodNc" />
        </div>
        <p className="indiciu">
          Codul NC (Nomenclatura Combinată, 8 cifre) merge în SAF-T ca `ProductCommodityCode`;
          gol e permis. Unitatea de măsură SAF-T e alta decât textul de pe document.
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
