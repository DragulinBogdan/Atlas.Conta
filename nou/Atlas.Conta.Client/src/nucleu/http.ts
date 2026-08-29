import { expiraSesiunea, token } from './auth';

// Singurul loc care vorbește cu API-ul. Traduce cele TREI feluri de răspuns pe
// care contractul le distinge (spike D2):
//   • 401           → sesiune expirată: tokenul moare, UI-ul cade pe /login;
//   • 422 EroriDto  → REFUZ DE DOMENIU — date, nu excepție de transport: exact
//                     mesajele gardienilor motorului, afișate ca listă;
//   • 400 EroriDto  → cerere malformată, motivată în ACEEAȘI formă (F13-D3):
//                     fie refuzul unei proiecții, fie eșecul de model binding;
//   • orice altceva → eroare tehnică.
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
  constructor(erori: string[]) {
    super(erori[0] ?? 'Operațiune refuzată.');
    this.name = 'EroareDomeniu';
    this.erori = erori;
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
  // 422 = refuz de DOMENIU (gardienii motorului). 400 = cerere malformată — și
  // de la F13-D3 e ÎNTOTDEAUNA `EroriDto`, oricare i-ar fi originea: refuzul
  // motivat al unei proiecții („«dataStart» nu poate fi după «dataEnd»") și
  // eșecul de model binding trec acum prin aceeași formă
  // (`InvalidModelStateResponseFactory` din `Startup.cs` traduce `ModelState`
  // în „{câmp}: {mesaj}"; `ValidationProblemDetails`, al doilea shape de 400,
  // a dispărut de pe sârmă). Distincția de STATUS rămâne a serverului; pentru
  // client, un corp cu `Erori[]` e un mesaj de arătat, nu o excepție de
  // transport.
  //
  // Ramura rămâne condiționată de `Erori[]` NEGOL — nu fiindcă serverul ar mai
  // trimite 400-uri fără corp, ci ca PLASĂ: un 400 non-JSON (proxy, rută
  // inexistentă servită de altcineva decât API-ul) trebuie să rămână eroare
  // tehnică, altfel „Operațiune refuzată de server" ar înlocui tăcut singurul
  // indiciu util, statusul.
  if (raspuns.status === 422 || raspuns.status === 400) {
    const corp = (await raspuns.json().catch(() => null)) as { Erori?: string[] | null } | null;
    const erori = corp?.Erori ?? null;
    if (erori !== null && erori.length > 0) throw new EroareDomeniu(erori);
    if (raspuns.status === 422) throw new EroareDomeniu(['Operațiune refuzată de server.']);
  }
  // Serviciul din amonte, indisponibil ACUM. Corpul e tot `EroriDto` (motivul
  // real vine de la server, nu se inventează aici); ce adaugă clientul e doar
  // FELUL erorii, ca ecranul să poată oferi „Reia".
  if (raspuns.status === 503) {
    const corp = (await raspuns.json().catch(() => null)) as { Erori?: string[] | null } | null;
    throw new EroareIndisponibil(corp?.Erori ?? ['Serviciul nu răspunde acum. Reîncercați.']);
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
