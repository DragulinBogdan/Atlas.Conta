import type { components } from '../../generated/api-types';
import { labelEnum } from '../../nucleu/campMeta';

// Puntea D300 → D394 și operațiunile excluse din D394 (123 a, c): informare, nu refuz.
type Punte = components['schemas']['D394Punte'];
type Exclus = components['schemas']['D394Exclus'];

const fmt = new Intl.NumberFormat('ro-RO', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const bani = (v?: number | null) => (v ?? 0) === 0 ? '' : fmt.format(v!);
const cheie = (r: { SensTva?: string | null; TipTvaId?: string; Cota?: number }) => `${r.SensTva}|${r.TipTvaId}|${r.Cota}`;

export function PunteD300D394({ punte, excluse }: { punte: Punte[]; excluse: Exclus[] }) {
  const cuDiferenta = punte.filter((r) => (r.BazaDiferenta ?? 0) !== 0 || (r.TvaDiferenta ?? 0) !== 0);
  const egale = punte.length - cuDiferenta.length;
  return (
    <>
      {excluse.length > 0 && (
        <div className="d300__neincluse">
          <h3>Excluse din declarație</h3>
          <p className="indiciu">
            Operațiuni pe care formularul 394 nu le cere (scutite, neimpozabile, intracomunitare, import),
            excluse prin politica <em>MapareD394</em>, cu motivul. Nu opresc fișierul; se regăsesc în D300.
          </p>
          <table className="tabel-mic">
            <thead>
              <tr><th>Sens</th><th>Tip TVA</th><th>Cotă</th><th>Motiv</th><th>Bază</th><th>TVA</th></tr>
            </thead>
            <tbody>
              {excluse.map((x) => (
                <tr key={cheie(x) + x.Motiv}>
                  <td>{labelEnum('SensTva', x.SensTva ?? '')}</td>
                  <td title={x.TipTvaDenumire ?? ''}>{x.TipTvaCod}</td>
                  <td className="num">{x.Cota}%</td>
                  <td>{x.Motiv}</td>
                  <td className="num">{bani(x.Baza)}</td>
                  <td className="num">{bani(x.Tva)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {cuDiferenta.length > 0 && (
        <div className="d394__avertismente">
          <h3>Puntea D300 → D394</h3>
          <p className="indiciu">
            Aceleași fapte fiscale, citite pe perioada fiecărei declarații: D300 pe înregistrare și exigibilitate,
            D394 pe primire și emisie. Pe fiecare rând, D300 minus ce e doar în perioada D300, plus ce e doar
            în perioada D394, dă declaratul, exclusul și neinclusul din D394. Coloanele intermediare sunt baze.
            {egale === 1 && ' Încă un rând are aceeași cifră în ambele declarații.'}
            {egale > 1 && ` Celelalte ${egale} rânduri au aceeași cifră în ambele declarații.`}
          </p>
          <table className="tabel-mic">
            <thead>
              <tr>
                <th>Sens</th><th>Tip TVA</th><th>Cotă</th><th>D300</th><th>Doar în perioada D300</th>
                <th>Doar în perioada D394</th><th>Exclus</th><th>Neinclus</th><th>Declarat D394</th>
                <th>Diferență bază</th><th>Diferență TVA</th><th>Documente decalate</th>
              </tr>
            </thead>
            <tbody>
              {cuDiferenta.map((r) => (
                <tr key={cheie(r)}>
                  <td>{labelEnum('SensTva', r.SensTva ?? '')}</td>
                  <td title={r.TipTvaDenumire ?? ''}>{r.TipTvaCod}</td>
                  <td className="num">{r.Cota}%</td>
                  <td className="num">{bani(r.BazaD300)}</td>
                  <td className="num">{bani(r.BazaDoarD300)}</td>
                  <td className="num">{bani(r.BazaDoarD394)}</td>
                  <td className="num">{bani(r.BazaExclusa)}</td>
                  <td className="num">{bani(r.BazaNeinclusa)}</td>
                  <td className="num">{bani(r.BazaDeclarata)}</td>
                  <td className="num"><strong>{bani(r.BazaDiferenta)}</strong></td>
                  <td className="num"><strong>{bani(r.TvaDiferenta)}</strong></td>
                  <td>
                    {(r.DocumenteDecalate ?? []).join('; ')}
                    {(r.NrDocumenteDecalate ?? 0) > (r.DocumenteDecalate ?? []).length
                      && ` și încă ${(r.NrDocumenteDecalate ?? 0) - (r.DocumenteDecalate ?? []).length}`}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </>
  );
}
