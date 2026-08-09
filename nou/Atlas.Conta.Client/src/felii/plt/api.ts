import { apiTrezorerie } from '../trz/api';

// Felia PLATĂ = ruta + identitatea. Contractul (DTO-uri, `spreWrite`, verbe) e
// al nucleului `felii/trz` — pe server e la fel: `TrezorerieApply<Plata>` cu un
// controller subțire pe `api/plt` (F3-D1).
export const plt = apiTrezorerie('/api/plt');

// Tipul de METADATA (captions). Schemele OpenAPI sunt comune — vezi `trz/api`.
export const TIP_ANTET = 'Plata';

export const RUTA = '/plt';
