import { stergeToken, token } from './auth';

// Singurul loc care vorbește cu API-ul. Traduce cele TREI feluri de răspuns pe
// care contractul le distinge (spike D2):
//   • 401           → sesiune expirată: tokenul moare, UI-ul cade pe /login;
//   • 422 EroriDto  → REFUZ DE DOMENIU — date, nu excepție de transport: exact
//                     mesajele gardienilor motorului, afișate ca listă;
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
    stergeToken();
    throw new EroareSesiune();
  }
  if (raspuns.status === 422) {
    const corp = (await raspuns.json().catch(() => null)) as { Erori?: string[] | null } | null;
    throw new EroareDomeniu(corp?.Erori ?? ['Operațiune refuzată de server.']);
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

export async function sterge(cale: string): Promise<void> {
  await trimite(cale, { method: 'DELETE' }, false);
}

// Mesajul de afișat pentru orice eroare prinsă de UI, ca listă (aceeași formă
// ca `Erori[]` — panoul de erori are un singur mod de randare).
export function eroriDin(e: unknown): string[] {
  if (e instanceof EroareDomeniu) return e.erori;
  if (e instanceof Error) return [e.message];
  return [String(e)];
}
