/// <reference types="vite/client" />

// Variabilele de mediu ale clientului. Doar prefixul `VITE_` ajunge în bundle
// (regula Vite) — deci nimic care nu poate fi public nu are ce căuta aici.
interface ImportMetaEnv {
  // Cheia de licență DevExtreme (F20-D9). Se pune în `.env.local`, care e
  // gitignored: e a utilizatorului, nu a repo-ului. Absentă ⇒ watermark de
  // trial, nu eroare.
  readonly VITE_DEVEXTREME_LICENSE?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
