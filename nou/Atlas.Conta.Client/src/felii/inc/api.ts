import { apiTrezorerie } from '../trz/api';

// Felia ÎNCASARE — geamăna plății, cu laturile inversate. Contractul e al
// nucleului `felii/trz` (F3-D1).
export const inc = apiTrezorerie('/api/inc');

export const TIP_ANTET = 'Incasare';

export const RUTA = '/inc';
