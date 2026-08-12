// Eticheta unui LOT citit prin OData — vocabular de NUCLEU, nu al unei felii
// (F6-D9: a treia și a patra utilizare au cerut extracția).
//
// Oglinda lui `ApiProiectii.EtichetaLot` (care e, la rândul lui, oglinda lui
// `Lot.Eticheta` — [NotMapped]): aceleași reguli, aceeași formă. Copia există
// fiindcă lookup-urile citesc lotul prin OData, unde proprietatea calculată nu
// ajunge (restanța 40d); cusătura e documentată în toate trei. Fixul de fond ar
// fi o coloană persistată sau o proiecție proprie de lookup-uri.
//
// Consumatori: pinul de lot al FCL, lotul descărcat pe BTR/BCS, lotul de minus
// al LDI. Toate cer aceeași etichetă — de aceea e una singură.

export function etichetaLot(element: Record<string, unknown>): string {
  if (!element) return '';
  const produs = element.Produs as Record<string, unknown> | null | undefined;
  const denumire = produs?.Denumire == null ? '(produs nedefinit)' : String(produs.Denumire);
  const data = ziLocala(element.Data);
  const pret = element.PretUnitar == null ? 0 : Number(element.PretUnitar);
  if (data == null)
    return denumire;
  // Lotul născut la culegere și nefinalizat încă de motor (25c): fără dată reală
  // și fără preț — se spune, nu se inventează.
  if (data === '01.01.0001' && pret === 0)
    return `${denumire} (în culegere)`;
  return `${denumire} · ${data} · ${pret.toLocaleString('ro-RO', { minimumFractionDigits: 2, maximumFractionDigits: 4 })}`;
}

// `Edm.Date` iese din ODataStore fie ca `Date` (deserializat), fie ca string ISO.
// Nicio conversie de fus: se citesc componentele locale ale obiectului sau se
// taie stringul.
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
