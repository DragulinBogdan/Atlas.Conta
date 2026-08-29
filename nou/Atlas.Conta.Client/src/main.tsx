import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router';
import { QueryClientProvider } from '@tanstack/react-query';
import config from 'devextreme/core/config';
import { locale, loadMessages } from 'devextreme/localization';
import roMessages from 'devextreme/localization/messages/ro.json';

import 'devextreme/dist/css/dx.light.css';
import './stil.css';
import { App } from './App';
import { cache } from './nucleu/cache';

// Licența DevExtreme (F20-D9). Cheia e a UTILIZATORULUI, nu a repo-ului: intră
// prin `.env.local` (gitignored), documentată în `.env.example`. Fără ea build-ul
// merge și aplicația funcționează — apare doar watermark-ul de trial, tolerat pe
// dev. Apelul stă ÎNAINTE de orice import care instanțiază un widget, fiindcă
// verificarea se face la crearea componentei.
config({ licenseKey: import.meta.env.VITE_DEVEXTREME_LICENSE ?? '' });

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
