import { expiraSesiunea, token } from './auth';

// Singurul loc care vorbește cu API-ul. Traduce cele PATRU feluri de răspuns pe
// care contractul le distinge (spike D2, extins de F22-D1/D8):
//   • 401              → sesiune expirată: tokenul moare, UI-ul cade pe /login;
//   • 4xx cu `Erori[]` → MESAJ DE ARĂTAT, nu excepție de transport. Aceeași
//                        formă (`EroriDto`) pentru toate cele patru cauze pe
//                        care serverul le distinge prin STATUS: 400 binding sau
//                        refuzul unei proiecții (F13-D3), 404 „subiectul nu e
//                        vizibil", 403 „nu ai dreptul", 422 refuz de DOMENIU
//                        (F22-D4). Ordinea pe sârmă (401 → 400 → 404 → 403 →
//                        422) e a SERVERULUI: autorizarea vine înaintea
//                        domeniului. Clientul n-o rejoacă și nu ramifică pe
//                        status — arată fraza pe care a primit-o;
//   • 503              → serviciul din AMONTE nu răspunde acum (reluarea are sens);
//   • orice altceva    → eroare tehnică.
// Clientul NU interpretează erorile de domeniu (43 §2 — „zero motor de reguli
// în TS"): le arată.

export class EroareSesiune extends Error {
  constructor() {
    super('Sesiune expirată. Autentificați-vă din nou.');
    this.name = 'EroareSesiune';
  }
}

export class EroareDomeniu extends Error {
  readonly erori: string[];
  // Statusul ales de server, INFORMATIV (F22-D8): util în consolă și în
  // rapoartele de probă, dar niciun ecran nu ramifică pe el. Ar fi exact
  // motorul de reguli în TS pe care 43b îl interzice — și ar fi și inutil:
  // motivul e deja în `erori`, în română, scris de singura ușă care îl știe.
  readonly status?: number;
  constructor(erori: string[], status?: number) {
    super(erori[0] ?? 'Operațiune refuzată.');
    this.name = 'EroareDomeniu';
    this.erori = erori;
    this.status = status;
  }
}

// 503 = serviciul din AMONTE nu răspunde ACUM (felia 15: registrul ANAF pică
// tranzitoriu — 5xx, timeout, rețea). E o a treia specie, distinctă de refuzul
// de domeniu: cererea era bună, iar reîncercarea are sens — exact ce nu e
// adevărat despre un 422. Fără clasa asta clientul ar fi văzut un `Error`
// generic („POST … → 503") și n-ar fi putut oferi butonul de reluare fără să
// citească textul mesajului, adică fără să ghicească.
export class EroareIndisponibil extends Error {
  readonly erori: string[];
  constructor(erori: string[]) {
    super(erori[0] ?? 'Serviciul nu răspunde acum. Reîncercați.');
    this.name = 'EroareIndisponibil';
    this.erori = erori;
  }
}

// `EroriDto` de pe sârmă: `{ "Erori": ["…"] }` (PascalCase). Întoarce `null`
// pentru orice altceva — corp gol, corp non-JSON, JSON fără `Erori` sau cu
// `Erori` goale — ca apelantul să cadă pe plasa lui. E singura ortografie a
// formei: o folosesc și `trimite` (pe `Response`), și `dxStore.ts` (pe
// `responseText`-ul lui `XMLHttpRequest`).
export function eroriDinCorp(text: string | null | undefined): string[] | null {
  if (!text) return null;
  try {
    const corp = JSON.parse(text) as { Erori?: string[] | null } | null;
    const erori = corp?.Erori ?? null;
    return erori !== null && erori.length > 0 ? erori : null;
  } catch {
    return null;
  }
}

// Statusurile pe care serverul le motivează cu `EroriDto` (F22-D4): 400
// binding/proiecție, 403 și 404 acces, 422 domeniu. 401 și 503 au conducta lor.
export const STATUSURI_CU_ERORI: readonly number[] = [400, 403, 404, 422];

function antete(cuCorp: boolean): HeadersInit {
  const h: Record<string, string> = { Accept: 'application/json' };
  const t = token();
  if (t) h.Authorization = `Bearer ${t}`;
  if (cuCorp) h['Content-Type'] = 'application/json';
  return h;
}

async function trimite(cale: string, init: RequestInit, cuCorp: boolean): Promise<Response> {
  const raspuns = await fetch(cale, { ...init, headers: { ...antete(cuCorp), ...(init.headers ?? {}) } });
  if (raspuns.status === 401) {
    expiraSesiunea();
    throw new EroareSesiune();
  }
  // Serviciul din amonte, indisponibil ACUM. Corpul e tot `EroriDto` (motivul
  // real vine de la server, nu se inventează aici); ce adaugă clientul e doar
  // FELUL erorii, ca ecranul să poată oferi „Reia".
  if (raspuns.status === 503) {
    const erori = eroriDinCorp(await raspuns.text().catch(() => null));
    throw new EroareIndisponibil(erori ?? ['Serviciul nu răspunde acum. Reîncercați.']);
  }
  // Cele patru statusuri motivate cu `EroriDto` (F22-D4), tratate LA FEL și
  // INDIFERENT de cale: 400 = cerere malformată sau refuzul motivat al unei
  // proiecții (F13-D3: `InvalidModelStateResponseFactory` traduce `ModelState`
  // în „{câmp}: {mesaj}"; `ValidationProblemDetails`, al doilea shape de 400, a
  // dispărut de pe sârmă); 404 = „înregistrarea nu există sau nu e vizibilă
  // pentru utilizatorul curent" (inexistent și invizibil sunt DELIBERAT
  // nedistinse — altfel API-ul ar fi un oracol de existență pentru rândurile pe
  // care securitatea le ascunde); 403 = „nu aveți dreptul de a … „X”"; 422 =
  // refuz de domeniu. Distincția de STATUS rămâne a serverului; pentru client
  // toate patru sunt un mesaj de arătat, nu o excepție de transport.
  //
  // Ce a MURIT aici (F22-D8): ramura `cale.startsWith('/api/odata/')` care
  // inventa în TS textele „Nu aveți drept de scriere pe acest nomenclator" /
  // „Rândul nu există sau nu aveți drept să-l vedeți". Erau o ghicitoare
  // necesară cât timp ușa OData răspundea `text/plain` englezesc — și una care
  // mințea: pe 403 spunea „scriere" chiar când refuzul era de citire. De la
  // felia 22 fraza vine de la server, în română, pe TOATE ușile (REST și
  // `api/odata/*` deopotrivă, prin `RefuzOdataFilter`), deci calea nu mai e un
  // criteriu — sursa mesajului e cea care îl știe.
  //
  // Condiția pe `Erori[]` NEGOL rămâne PLASĂ: un 403/404/400 fără corp JSON
  // (proxy, rută servită de altcineva decât API-ul, un `Forbid()` cu corp gol
  // scăpat undeva) trebuie să rămână eroare tehnică — altfel un mesaj inventat
  // ar înlocui tăcut singurul indiciu util, statusul.
  if (STATUSURI_CU_ERORI.includes(raspuns.status)) {
    const erori = eroriDinCorp(await raspuns.text().catch(() => null));
    if (erori) throw new EroareDomeniu(erori, raspuns.status);
    if (raspuns.status === 422) throw new EroareDomeniu(['Operațiune refuzată de server.'], 422);
  }
  if (!raspuns.ok)
    throw new Error(`${init.method ?? 'GET'} ${cale} → ${raspuns.status} ${raspuns.statusText}`);
  return raspuns;
}

async function corp<T>(raspuns: Response): Promise<T> {
  if (raspuns.status === 204) return undefined as T;
  const text = await raspuns.text();
  return (text ? JSON.parse(text) : undefined) as T;
}

export async function ia<T>(cale: string): Promise<T> {
  return corp<T>(await trimite(cale, { method: 'GET' }, false));
}

export async function pune<T>(cale: string, date: unknown): Promise<T> {
  return corp<T>(await trimite(cale, { method: 'PUT', body: JSON.stringify(date) }, true));
}

export async function posteaza<T>(cale: string, date?: unknown): Promise<T> {
  return corp<T>(await trimite(
    cale,
    date === undefined ? { method: 'POST' } : { method: 'POST', body: JSON.stringify(date) },
    date !== undefined));
}

// PATCH — modificarea PARȚIALĂ a unei entități (F20-D7/D8). Verbul apare abia
// cu ecranele de nomenclator: documentele se scriu ca AGREGAT ÎNTREG (PUT header
// + linii, 42d), unde un patch parțial ar fi contrazis reconcilierea
// server-side. Pe OData e invers — nomenclatorul n-are agregat, iar corpul unui
// PATCH e chiar delta pe care o cere `DataController.Patch` (`Delta<TEntity>`):
// câmpurile absente rămân NEATINSE. De aceea aici absența unui câmp NU e golire
// deliberată (convenția 56 e a documentelor, a lui PUT); golirea se scrie
// explicit, cu `null`.
//
// Măsurat pe host: `PATCH api/odata/Partener({id})` ⇒ **204 No Content**, fără
// `Prefer` și fără `If-Match`; refuzul gardianului ⇒ `422 {Erori:[…]}` (F20-D7),
// tradus de `trimite` exact ca pe REST. Corpul gol al lui 204 e tratat de
// `corp<T>`, deci apelantul primește `undefined` — recitirea e a lui.
export async function modifica<T>(cale: string, date: unknown): Promise<T> {
  return corp<T>(await trimite(cale, { method: 'PATCH', body: JSON.stringify(date) }, true));
}

export async function sterge(cale: string): Promise<void> {
  await trimite(cale, { method: 'DELETE' }, false);
}

// ═══ Descărcarea unui FIȘIER (felia 16, D16-D6) ═══
// De ce nu un simplu `<a href={cale} download>`, cum spera contractul: sesiunea e
// un JWT din `sessionStorage`, trimis ca antet `Authorization` — o navigare de
// browser NU poartă antete, deci link-ul ar fi primit 401 și ar fi descărcat…
// pagina de eroare. (Nici un token în query string nu e o variantă: ar ajunge în
// istoric, în log-urile serverului și în Referer.)
//
// Deci aceeași conductă ca orice cerere — cu 401/422/400 traduse identic, ceea
// ce e chiar câștigul: 403-ul de pe fișierul SAF-T și 422-ul profilului bugetar
// ajung în `PanouErori`, nu într-un tab alb. Corpul se ia ca `blob` (adică
// TOTUL în memoria paginii — costul asumat al autentificării prin antet;
// fișierul unei luni e de ordinul MB) și se salvează printr-un `<a download>`
// sintetic peste un `blob:` URL.
export async function descarcaFisier(cale: string, numeImplicit: string, accept?: string): Promise<void> {
  const raspuns = await trimite(cale, { method: 'GET', headers: accept ? { Accept: accept } : {} }, false);
  const blob = await raspuns.blob();
  const nume = numeDinDispozitie(raspuns.headers.get('Content-Disposition')) ?? numeImplicit;
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = nume;
  document.body.appendChild(a);
  a.click();
  a.remove();
  // Revocarea imediată taie descărcarea în unele browsere (salvarea începe după
  // click, asincron); un minut e mult peste orice fereastră reală.
  setTimeout(() => URL.revokeObjectURL(url), 60_000);
}

// `attachment; filename="SAF-T_12345674_2025-09.xml"` → numele. Serverul e
// singurul care știe cum se cheamă fișierul (CUI-ul societății e al lui), deci
// numele lui BATE implicitul clientului.
function numeDinDispozitie(antet: string | null): string | null {
  if (!antet) return null;
  const utf8 = /filename\*=UTF-8''([^;]+)/i.exec(antet);
  if (utf8) return decodeURIComponent(utf8[1].trim());
  const simplu = /filename="?([^";]+)"?/i.exec(antet);
  return simplu ? simplu[1].trim() : null;
}

// Mesajul de afișat pentru orice eroare prinsă de UI, ca listă (aceeași formă
// ca `Erori[]` — panoul de erori are un singur mod de randare).
export function eroriDin(e: unknown): string[] {
  if (e instanceof EroareDomeniu || e instanceof EroareIndisponibil) return e.erori;
  if (e instanceof Error) return [e.message];
  return [String(e)];
}
