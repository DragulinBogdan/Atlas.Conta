import type { components } from '../../generated/api-types';
import { ia, posteaza } from '../../nucleu/http';

// Perioadele n-au agregat de cules (F27-D1/D2): nu e un document, e un LANȚ cu
// două comenzi. De aceea aici nu există `WriteDto`, `antetGol` sau `spreWrite` —
// verbele feliei sunt lanțul, verificarea (raport, nu scrie nimic), închiderea
// (cu cheile acceptate conștient), redeschiderea (cu motiv) și istoricul.
//
// Nimic nu se calculează aici (42c): severitatea fiecărei constatări vine gata
// din politică, iar „se poate închide” e verdictul serverului, nu o condiție
// reconstruită în TS din lista de constatări.

type Scheme = components['schemas'];

export type Perioada = Scheme['PerioadaDto'];
export type ConstatareInchidere = Scheme['ConstatareInchidereDto'];
export type InchidereRezultat = Scheme['InchiderePerioadaRezultatDto'];
export type IstoricPerioada = Scheme['IstoricPerioadaDto'];

const BAZA = '/api/perioade';

export const perioade = {
  lant: () => ia<Perioada[]>(BAZA),

  verificare: (an: number, luna: number) =>
    ia<ConstatareInchidere[]>(`${BAZA}/${an}/${luna}/verificare`),

  istoric: (an: number, luna: number) =>
    ia<IstoricPerioada[]>(`${BAZA}/${an}/${luna}/istoric`),

  inchide: (an: number, luna: number, acceptate: string[]) =>
    posteaza<InchidereRezultat>(`${BAZA}/${an}/${luna}/inchide`, { Acceptate: acceptate }),

  redeschide: (an: number, luna: number, motiv: string) =>
    posteaza<InchidereRezultat>(`${BAZA}/${an}/${luna}/redeschide`, { Motiv: motiv }),
};

export function etichetaPerioada(p: { An?: number; Luna?: number }): string {
  return `${String(p.Luna ?? 0).padStart(2, '0')}/${p.An ?? 0}`;
}
