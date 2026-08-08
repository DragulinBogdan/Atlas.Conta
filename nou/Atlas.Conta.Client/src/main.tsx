import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { locale, loadMessages } from 'devextreme/localization';
import roMessages from 'devextreme/localization/messages/ro.json';

import 'devextreme/dist/css/dx.light.css';
import './stil.css';
import { App } from './App';

// Localizarea completă a shell-ului e explicit în afara spike-ului; ce e gratis
// (mesajele DevExtreme + locale-ul de formatare) se pune acum.
loadMessages(roMessages);
locale('ro');

// Cache-ul de citire (43c, felul (1)). Nu e store de aplicație: cheia e
// interogarea, invalidarea o fac comenzile, nimic de domeniu nu se „ține" aici.
const cache = new QueryClient({
  defaultOptions: { queries: { refetchOnWindowFocus: false, retry: false } },
});

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={cache}>
      <BrowserRouter>
        <App />
      </BrowserRouter>
    </QueryClientProvider>
  </StrictMode>,
);
