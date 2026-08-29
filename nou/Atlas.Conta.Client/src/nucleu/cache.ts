import { QueryClient } from '@tanstack/react-query';

// Cache-ul de citire (43c, felul (1)). Nu e store de aplicație: cheia e
// interogarea, invalidarea o fac comenzile, nimic de domeniu nu se „ține" aici.
//
// De ce stă într-un MODUL, nu în `main.tsx` (F20-D2): din felia 20 cache-ul are
// și consumatori din AFARA arborelui React — `byKey`-ul store-ului OData
// (`nucleu/odata.ts`) rulează în componenta DevExtreme, unde nu există
// `useQueryClient`. Un al doilea `QueryClient` acolo ar însemna două cache-uri
// care nu se văd: exact ce trebuia să dispară. Un singur obiect, importat și de
// `main.tsx` (care îl dă providerului), și de conductele imperative.
export const cache = new QueryClient({
  defaultOptions: { queries: { refetchOnWindowFocus: false, retry: false } },
});
