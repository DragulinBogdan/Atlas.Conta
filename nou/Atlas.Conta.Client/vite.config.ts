import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// Dev = Vite dev server cu PROXY server-side către WebApi (43e): browserul
// vorbește doar cu originul Vite, deci CORS nu există nici în dev, nici în
// producție (unde SPA-ul e servit static de același host). `secure: false`
// pentru certificatul de dezvoltare self-signed al Kestrel.
// Hostul de dev poate rula pe alt port decât cel al utilizatorului (5001);
// `VITE_TINTA_API` îl mută fără să atingă fișierul. `process` se declară local
// (config-ul rulează în Node, dar proiectul n-are `@types/node` — o dependență
// întreagă pentru o singură variabilă n-ar fi meritat).
declare const process: { env: Record<string, string | undefined> };

const TINTA = process.env.VITE_TINTA_API ?? 'https://localhost:5001';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': { target: TINTA, changeOrigin: true, secure: false },
    },
  },
});
