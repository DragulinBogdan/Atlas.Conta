import { useCallback, useMemo } from 'react';
import { useSearchParams } from 'react-router';

// „URL-ul E starea globală" (43c) — până acum afirmat, niciodată folosit: rutele
// purtau doar identitatea documentului, iar niciun ecran n-avea parametri. Felia
// de raportare e prima care are (perioadă, mod, cont), și îi are pe TOATE trei
// ecranele — de aceea helper-ul stă în nucleu, nu într-o felie.
//
// Ce rezolvă, concret: deep-link („balanța pe februarie, analitic"), refresh
// fără pierdere de context, și drill-down-ul balanță → fișă care PĂSTREAZĂ
// perioada. Fără el fiecare ecran și-ar fi ținut filtrele în `useState` și
// nicio verigă n-ar fi fost partajabilă.
//
// Ce NU e: un store. Nu ține stare proprie, nu sincronizează nimic — citește și
// scrie `?…`, atât. Starea de FORMULAR rămâne unde e (43c: agregatul per felie);
// aici intră doar parametrii care merită să fie într-un link.

// Datele contabile sunt STRING-uri ISO peste tot în client (`zi.ts` — nicio
// conversie de fus n-are voie să le atingă), deci o „dată" e un `string`: nu
// există un al treilea fel de valoare de reprezentat.
export type ValoareStare = string | boolean;

// Implicitul `false` s-ar infera ca TIPUL literal `false`, iar starea citită din
// URL ar deveni ne-atribuibilă („`boolean` nu se poate atribui lui `false`").
// Felul parametrului e boolean, nu valoarea lui implicită — de aceea booleenele
// se lărgesc; string-urile rămân cum sunt date.
type Stare<T> = { [K in keyof T]: T[K] extends boolean ? boolean : T[K] };

// Citire+scriere tipată, cu valorile implicite ca SCHEMĂ: felul fiecărui
// parametru se deduce din tipul implicitului (`boolean` ⇒ se parsează, altfel
// string brut), deci nu există o a doua declarație de tipuri care să dividă.
//
// Convenția de scriere: valoarea egală cu implicitul IESE din URL. Link-urile
// rămân scurte și, mai important, „implicit" are o singură reprezentare —
// altfel `?analitic=false` și absența lui ar fi două stări identice scrise
// diferit. Consecința pe care apelantul trebuie s-o știe: un parametru care
// TREBUIE să poată fi golit (perioada opțională a jurnalului) își declară
// implicitul `''`, nu o valoare calculată.
export function useUrlStare<T extends Record<string, ValoareStare>>(implicite: T) {
  const [params, setParams] = useSearchParams();

  // `implicite` e scris inline în JSX (obiect nou la fiecare randare), deci
  // cheia de identitate e CONȚINUTUL lui — aceeași disciplină ca `expand`/
  // `filtru` din `Lookup`.
  const cheieImplicite = JSON.stringify(implicite);
  const cheieParams = params.toString();

  const stare = useMemo(() => {
    const rezultat = { ...implicite } as Record<string, ValoareStare>;
    for (const [cheie, implicit] of Object.entries(implicite)) {
      const brut = params.get(cheie);
      if (brut === null) continue;
      rezultat[cheie] = typeof implicit === 'boolean' ? brut === 'true' : brut;
    }
    return rezultat as Stare<T>;
    // eslint-disable-next-line react-hooks/exhaustive-deps -- `implicite`/`params` intră prin cheile de conținut.
  }, [cheieImplicite, cheieParams]);

  const seteaza = useCallback((patch: Partial<Stare<T>>) => {
    // `replace: true`: schimbarea unui filtru nu e o NAVIGARE — altfel butonul
    // „înapoi" ar derula fiecare tastă din caseta de dată în loc să întoarcă
    // ecranul precedent.
    setParams((precedent) => {
      const urmator = new URLSearchParams(precedent);
      const implicitePatch = JSON.parse(cheieImplicite) as Record<string, ValoareStare>;
      for (const [cheie, valoare] of Object.entries(patch)) {
        if (valoare === undefined || valoare === implicitePatch[cheie]) urmator.delete(cheie);
        else urmator.set(cheie, String(valoare));
      }
      return urmator;
    }, { replace: true });
  }, [setParams, cheieImplicite]);

  return [stare, seteaza] as const;
}

// Construirea unui link către alt ecran cu parametri (drill-down balanță →
// fișă). Aici parametrii se scriu TOȚI cei dați — nu există „implicit" comun
// între două ecrane diferite, deci omisiunea ar fi o presupunere. Valorile
// goale/absente se sar.
export function urlCu(cale: string, params: Record<string, ValoareStare | null | undefined>): string {
  const q = new URLSearchParams();
  for (const [cheie, valoare] of Object.entries(params)) {
    if (valoare === undefined || valoare === null || valoare === '') continue;
    q.set(cheie, String(valoare));
  }
  const sir = q.toString();
  return sir ? `${cale}?${sir}` : cale;
}
