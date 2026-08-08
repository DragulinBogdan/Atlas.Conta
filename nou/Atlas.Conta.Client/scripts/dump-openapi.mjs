// Normalizarea + scrierea artefactului `openapi.json` (43d, pasul (a) al
// pipeline-ului). Artefactul se COMITE; regenerarea e explicită, exact
// disciplina migrațiilor: canonic e ce e comis, unealta doar reproduce.
//
// Două SURSE, o singură normalizare (F2-D7). Normalizarea trăiește aici, nu în
// generator, tocmai ca artefactul să fie identic indiferent pe ce cale a fost
// produs — altfel „driftul" ar semnala diferența dintre generatoare, nu dintre
// contracte (`swagger tofile` scrie `[ ]` și fără newline final, `fetch`+
// `JSON.stringify` scriu `[]` cu newline).
//
//   * OFFLINE (implicit, `pnpm gen:openapi` → `scripts/gen-openapi.ps1`):
//     `swagger tofile` pe assembly-ul WebApi, fără host viu și fără bază.
//     Fișierul brut ajunge aici prin `SWAGGER_FILE`.
//   * HOST VIU (`pnpm gen:openapi:host`), rămas ca plasă:
//     `SWAGGER_URL` (implicit https://localhost:5001), host-ul pornit în
//     Development (Swagger e activ doar acolo).
import { readFileSync, writeFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { dirname, join } from 'node:path';

const FISIER_SWAGGER = process.env.SWAGGER_FILE;
const URL_SWAGGER = process.env.SWAGGER_URL ?? 'https://localhost:5001/swagger/v1/swagger.json';
const iesire = join(dirname(fileURLToPath(import.meta.url)), '..', 'src', 'generated', 'openapi.json');

let document;
if (FISIER_SWAGGER) {
  document = JSON.parse(readFileSync(FISIER_SWAGGER, 'utf8'));
}
else {
  // Certificatul de dezvoltare Kestrel e self-signed — la fel ca `secure:false`
  // din proxy-ul Vite. Doar pentru acest script, doar în dev.
  process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';
  const raspuns = await fetch(URL_SWAGGER);
  if (!raspuns.ok) {
    console.error(`Swagger indisponibil (${raspuns.status}) la ${URL_SWAGGER} — pornește host-ul WebApi în Development.`);
    process.exit(1);
  }
  document = await raspuns.json();
}

// Re-serializat cu indentare stabilă: diff-ul trebuie să fie citibil.
writeFileSync(iesire, JSON.stringify(document, null, 2) + '\n', 'utf8');
console.log(`openapi.json scris: ${iesire}`);
