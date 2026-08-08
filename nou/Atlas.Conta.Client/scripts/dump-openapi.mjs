// Dump-ul `openapi.json` din HOST-UL VIU (43d, pasul (a) al pipeline-ului).
// Artefactul se COMITE; regenerarea e explicită (`pnpm gen:openapi`), exact
// disciplina migrațiilor: canonic e ce e comis, unealta doar reproduce.
//
// Precondiție: host-ul rulează în Development (Swagger e activ doar acolo):
//   dotnet run --project nou/.../Atlas.Conta.BackOffice.WebApi --launch-profile Atlas.Conta.BackOffice.WebApi
import { writeFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { dirname, join } from 'node:path';

const URL_SWAGGER = process.env.SWAGGER_URL ?? 'https://localhost:5001/swagger/v1/swagger.json';
const iesire = join(dirname(fileURLToPath(import.meta.url)), '..', 'src', 'generated', 'openapi.json');

// Certificatul de dezvoltare Kestrel e self-signed — la fel ca `secure:false`
// din proxy-ul Vite. Doar pentru acest script, doar în dev.
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

const raspuns = await fetch(URL_SWAGGER);
if (!raspuns.ok) {
  console.error(`Swagger indisponibil (${raspuns.status}) la ${URL_SWAGGER} — pornește host-ul WebApi în Development.`);
  process.exit(1);
}
// Re-serializat cu indentare stabilă: diff-ul trebuie să fie citibil.
const document = await raspuns.json();
writeFileSync(iesire, JSON.stringify(document, null, 2) + '\n', 'utf8');
console.log(`openapi.json scris: ${iesire}`);
