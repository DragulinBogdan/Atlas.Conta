// Formatarea cifrelor pentru TEXT (în grile se folosește `format="#,##0.00"`).
// Cifrele vin gata calculate de pe server (42c); aici doar se afișează.
export function bani(v: number | null | undefined): string {
  if (v == null) return '—';
  return v.toLocaleString('ro-RO', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}
