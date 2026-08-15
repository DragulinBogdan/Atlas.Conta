// Datele contabile circulă pe sârmă ca `DateOnly` ISO („2026-08-08") și se
// păstrează ca STRING în agregat — nicio conversie de fus orar nu are voie să le
// atingă (vezi `campuri.tsx`). `azi()` e singura fabrică de dată din client:
// documentul nou și data implicită a stornării pleacă de aici.
export function azi(): string {
  const d = new Date();
  return `${d.getFullYear()}-${`${d.getMonth() + 1}`.padStart(2, '0')}-${`${d.getDate()}`.padStart(2, '0')}`;
}

// `Date` (ce dă `DateBox`) → zi ISO, fără oră și fără fus. Un singur loc, folosit
// de `CampData` și de cererea de dată a shell-ului.
export function izolataZi(v: unknown): string | undefined {
  if (!v) return undefined;
  const d = v instanceof Date ? v : new Date(String(v));
  if (Number.isNaN(d.getTime())) return undefined;
  const luna = `${d.getMonth() + 1}`.padStart(2, '0');
  const zi = `${d.getDate()}`.padStart(2, '0');
  return `${d.getFullYear()}-${luna}-${zi}`;
}

// Zi ISO (sau `Date` deserializat de ODataStore) → „dd.MM.yyyy", pentru AFIȘARE
// în text (etichete de lookup, opțiuni de SelectBox) — acolo unde nu există un
// widget cu `displayFormat`. Nicio conversie de fus: se citesc componentele
// locale ale obiectului sau se taie stringul.
export function ziLocala(v: unknown): string | null {
  if (v == null) return null;
  if (v instanceof Date) {
    const zi = `${v.getDate()}`.padStart(2, '0');
    const luna = `${v.getMonth() + 1}`.padStart(2, '0');
    return `${zi}.${luna}.${v.getFullYear()}`;
  }
  const text = String(v).slice(0, 10);
  const parti = text.split('-');
  return parti.length === 3 ? `${parti[2]}.${parti[1]}.${parti[0]}` : text;
}
