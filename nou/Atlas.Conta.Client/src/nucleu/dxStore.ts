import { createStore } from 'devextreme-aspnet-data-nojquery';
import { token } from './auth';

// Pachetul `-nojquery` e varianta pentru SPA-uri (React/Angular/Vue) a
// `DevExtreme.AspNet.Data`: bundle-ul CJS clasic cere `jquery` necondiționat, iar
// aici nu există jQuery. Același protocol `DataSourceLoader`, același API.
//
// Grilele de CITIRE sunt remote prin construcție (43c): `DataSourceLoader`
// server-side, `{data,totalCount}` pe sârmă, nimic materializat în afara
// paginii. Aceeași disciplină ca „TS nu calculează niciodată un sold": filtrul,
// sortarea, paginarea și agregatele stau pe server.
export function storeRemote(loadUrl: string, key: string | string[] = 'Id') {
  return createStore({
    key,
    loadUrl,
    onBeforeSend: (_operatie, setari) => {
      setari.headers = { ...(setari.headers ?? {}), Authorization: `Bearer ${token() ?? ''}` };
    },
  });
}
