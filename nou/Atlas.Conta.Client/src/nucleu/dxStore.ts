import { createStore } from 'devextreme-aspnet-data-nojquery';
import { expiraSesiunea, token } from './auth';
import { eroriDinCorp, STATUSURI_CU_ERORI } from './http';

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
    // Sesiunea expirată: aceeași reacție ca pe `http.ts` (vezi `expiraSesiunea`).
    // Fără asta grila arăta „No data" la 401 — indistinct de un filtru fără
    // rezultate.
    //
    // Restul refuzurilor motivate (F22-D4/D8): 400 binding/proiecție, 403/404
    // acces, 422 domeniu — toate cu corp `EroriDto`. `e.error` E SCRIIBIL și e
    // chiar mesajul care ajunge la grilă: `devextreme-aspnet-data-nojquery`
    // face, în `send`, `var e = {xhr, error}; onAjaxError(e); error = e.error;`
    // și respinge Deferred-ul cu valoarea REZULTATĂ (index.js), deci ce punem
    // aici e ce vede operatorul. Fără rescriere ar vedea JSON-ul BRUT:
    // `getErrorMessageFromXhr` caută, într-un corp `application/json`, prima
    // proprietate de tip STRING — iar `Erori` e un ARRAY, deci funcția cade pe
    // `responseText`-ul întreg, cu ghilimele și paranteze cu tot.
    //
    // Statusul nu se traduce și nu se ramifică (43b): mesajul serverului e
    // singurul lucru afișat. Un corp fără `Erori[]` lasă mesajul implicit.
    onAjaxError: (e) => {
      const xhr = e.xhr as XMLHttpRequest | undefined;
      if (xhr?.status === 401) {
        expiraSesiunea();
        return;
      }
      if (!xhr || !STATUSURI_CU_ERORI.includes(xhr.status)) return;
      const erori = eroriDinCorp(xhr.responseText);
      if (erori) e.error = erori.join('\n');
    },
  });
}
