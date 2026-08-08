// JWT în `sessionStorage` (D11). Nu e „store global de stare" — e ambientul
// sesiunii de browser, citit de un singur loc (wrapper-ul de fetch) și scris de
// două (login/logout). Nicio dată de domeniu nu trece pe aici.

const CHEIE = 'atlas.conta.jwt';

export function token(): string | null {
  return sessionStorage.getItem(CHEIE);
}

export function esteAutentificat(): boolean {
  return token() != null;
}

export function stergeToken(): void {
  sessionStorage.removeItem(CHEIE);
}

// Endpoint-ul XAF întoarce tokenul ca TEXT BRUT (nu JSON, nu `{token:…}`) —
// verificat pe host-ul viu. `JSON.parse` ar pica; ghilimelele eventuale se taie.
export async function autentifica(utilizator: string, parola: string): Promise<void> {
  const raspuns = await fetch('/api/Authentication/Authenticate', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ UserName: utilizator, Password: parola }),
  });
  if (!raspuns.ok)
    throw new Error(raspuns.status === 401 ? 'Utilizator sau parolă incorecte.' : `Autentificare eșuată (${raspuns.status}).`);
  const brut = (await raspuns.text()).trim();
  const valoare = brut.startsWith('"') && brut.endsWith('"') ? brut.slice(1, -1) : brut;
  if (!valoare)
    throw new Error('Serverul nu a întors niciun token.');
  sessionStorage.setItem(CHEIE, valoare);
}
