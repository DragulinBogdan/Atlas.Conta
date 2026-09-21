import type { components } from '../../generated/api-types';
import { ia } from '../../nucleu/http';
import { urlCu } from '../../nucleu/urlStare';

// Fișa `Imobilizare` e nomenclator pe ușa OData (F2-D4), deci felia n-are verbe
// de scriere: cele două rute REST de aici sunt proiecții peste registru, cu
// situația, banda catalogului și totalurile calculate pe server (42c).

type Scheme = components['schemas'];

export type Imobilizare = Scheme['Imobilizare'];
export type ClasificareImobilizari = Scheme['ClasificareImobilizari'];
export type FisaImobilizareDto = Scheme['FisaImobilizareDto'];
export type SituatieImobilizareDto = Scheme['SituatieImobilizareDto'];
export type RandImobilizareDto = Scheme['RandImobilizareDto'];
export type RegistruImobilizariDto = Scheme['RegistruImobilizariDto'];
export type RandRegistruImobilizariDto = Scheme['RandRegistruImobilizariDto'];

// Entitatea XAF = numele setului OData = numele schemei OpenAPI.
export const TIP = 'Imobilizare';

const BAZA = '/api/imobilizari';

export const imobilizari = {
  // `laData` absent ⇒ serverul ia ziua de azi; trimis GOL ar pleca pe sârmă ca
  // dată invalidă, de aceea `urlCu` sare valorile vide.
  fisa: (id: string, laData?: string) =>
    ia<FisaImobilizareDto>(urlCu(`${BAZA}/${id}/fisa`, { laData })),

  registru: (laData?: string) =>
    ia<RegistruImobilizariDto>(urlCu(`${BAZA}/registru`, { laData })),
};
