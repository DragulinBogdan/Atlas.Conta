import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// Dev = Vite dev server cu PROXY server-side către WebApi (43e): browserul
// vorbește doar cu originul Vite, deci CORS nu există nici în dev, nici în
// producție (unde SPA-ul e servit static de același host). `secure: false`
// pentru certificatul de dezvoltare self-signed al Kestrel.
const TINTA = 'https://localhost:5001';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': { target: TINTA, changeOrigin: true, secure: false },
    },
  },
});
