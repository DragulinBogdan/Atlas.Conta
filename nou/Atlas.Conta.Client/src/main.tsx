import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router';
import { QueryClientProvider } from '@tanstack/react-query';
import { licenseKey } from './devextreme-license';
import config from 'devextreme/core/config';
import { locale, loadMessages } from 'devextreme/localization';
import roMessages from 'devextreme/localization/messages/ro.json';

import 'devextreme/dist/css/dx.light.css';
import './stil.css';
import { App } from './App';
import { cache } from './nucleu/cache';

config({ licenseKey });

// Localizarea completă a shell-ului e explicit în afara spike-ului; ce e gratis
// (mesajele DevExtreme + locale-ul de formatare) se pune acum.
loadMessages(roMessages);
locale('ro');

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={cache}>
      <BrowserRouter>
        <App />
      </BrowserRouter>
    </QueryClientProvider>
  </StrictMode>,
);
