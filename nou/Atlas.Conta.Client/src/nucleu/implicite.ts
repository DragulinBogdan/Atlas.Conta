import { useEffect, useRef, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { ia } from './http';
import type { components } from '../generated/api-types';

// Implicitele de culegere (F23-D6) — mecanica, o SINGURĂ dată, în nucleu.
//
// Ce face: pe o linie NOUĂ cu tipul de TVA gol, întreabă serverul ce ar aplica
// EL și precompletează exact asta. Ce nu face: nu decide nimic. Regimul e al
// partenerului, cota e a produsului, ordinea treptelor e a serviciului — zero
// calcul în TS (43b), inclusiv zero „dacă partenerul e UE atunci scutit".
//
// De ce merită să existe, dat fiind că PUT-ul aplică oricum același serviciu pe
// liniile care ajung goale: operatorul vede tipul ÎNAINTE de salvare, cu motivul
// lui, și îl poate schimba. Fără asta, cifra apărea abia după „Salvează", ca și
// cum ar fi ghicit-o cineva. Cele două nu pot diverge fiindcă e aceeași funcție
// pe server (`ImpliciteService.TipTva`), chemată o dată prin ruta asta și o dată
// prin `TvaService.AplicaTipTvaImplicit`.
//
// Ce NU se schimbă din 56: tăcerea are semantică. Pe o linie EXISTENTĂ absența
// tipului în payload rămâne GOLIRE deliberată (round-trip), iar precompletarea
// nu se declanșează niciodată acolo. Iar pe linia nouă, odată aplicat, tipul nu
// se mai rescrie: o golire făcută de operator RĂMÂNE goală (77c).

export type RezultatImplicit = components['schemas']['RezultatImplicitDto'];

export type CerereImplicit = {
  // Codul ancorei: FCT, FCL, DEC, RLF, RDC.
  tipDocument: string;
  // Partenerul DOCUMENTULUI — el poartă regimul. Latura diferă per felie
  // (furnizorul pe intrări, clientul pe ieșiri), deci o dă apelantul.
  partenerId?: string | null;
  // Produsul LINIEI — el poartă cota. Feliile fără produs pe linie (DEC, RLF,
  // RDC) nu-l trimit deloc.
  produsId?: string | null;
  // Data DOCUMENTULUI, nu ziua de azi: rândurile de politică au valabilitate.
  data?: string | null;
};

// Parametrii ABSENȚI nu se trimit: un `partenerId=` gol ar fi fost un GUID
// invalid pe sârmă, iar `data=` gol ar fi însemnat altceva decât „azi".
function cale(cerere: CerereImplicit): string {
  const parametri = new URLSearchParams({ tipDocument: cerere.tipDocument });
  if (cerere.partenerId) parametri.set('partenerId', cerere.partenerId);
  if (cerere.produsId) parametri.set('produsId', cerere.produsId);
  if (cerere.data) parametri.set('data', cerere.data);
  return `/api/implicite/tip-tva?${parametri.toString()}`;
}

export function cereImplicitTipTva(cerere: CerereImplicit): Promise<RezultatImplicit> {
  return ia<RezultatImplicit>(cale(cerere));
}

// Precompletarea, ca hook. Întoarce MOTIVUL serverului (textul de sub câmp) —
// singurul lucru de care are nevoie ecranul după ce valoarea s-a aplicat.
//
// `activa` e condiția feliei („linie nouă, câmpul e gol, iar rolul liniei chiar
// poartă TVA"), nu una reconstruită aici. Cheia interogării poartă TOȚI
// parametrii: două linii cu produse diferite sunt două întrebări diferite.
//
// Update FUNCȚIONAL, prin `aplica`: `setLinie(prev => prev.TipTvaId ? prev : …)`
// se scrie în felie, fiindcă acolo e starea. Aici stă doar garda care contează:
// un id se aplică O SINGURĂ dată. Fără ea, o golire făcută de operator ar
// reactiva `activa` și implicitul s-ar întoarce peste el — exact ce interzice
// 77c.
export function usePrecompletareTipTva(
  cerere: CerereImplicit,
  activa: boolean,
  aplica: (tipTvaId: string, cod: string | null) => void,
): string | null {
  const raspuns = useQuery({
    queryKey: ['implicit-tip-tva', cerere.tipDocument,
      cerere.partenerId ?? null, cerere.produsId ?? null, cerere.data ?? null],
    queryFn: () => cereImplicitTipTva(cerere),
    enabled: activa,
    // Politicile nu se schimbă în viața unui editor de linie deschis.
    staleTime: 5 * 60_000,
  });

  const [motiv, setMotiv] = useState<string | null>(null);
  const aplicat = useRef<string | null>(null);
  // `aplica` e o funcție nouă la fiecare randare (closure peste starea liniei);
  // ca dependență ar fi rulat efectul la nesfârșit. Se ține ultima, prin ref.
  const ultima = useRef(aplica);
  ultima.current = aplica;

  const rezultat = raspuns.data;
  useEffect(() => {
    const id = rezultat?.TipTvaId ?? null;
    if (!activa || !id || aplicat.current === id)
      return;
    aplicat.current = id;
    ultima.current(id, rezultat?.TipTvaCod ?? null);
    setMotiv(rezultat?.Motiv ?? null);
  }, [activa, rezultat]);

  return motiv;
}
