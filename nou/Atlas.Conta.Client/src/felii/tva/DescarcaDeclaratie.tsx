import { useState } from 'react';
import { descarcaFisier, eroriDin } from '../../nucleu/http';
import { PanouErori } from '../../nucleu/PanouErori';

type Refuz = { Cod?: string | null; Mesaj?: string | null };
type Bifa = { cheie: string; eticheta: string };

/** Antetele exportului XML, păstrate la confirmarea depunerii (122 i). */
export type FisierExportat = { VersiuneExportata: string; Sha256Fisier: string; ParametriFisier: string };

// Fișierul de depus (E2-D8): butonul stă inactiv cât proiecția are refuzuri, iar refuzul cererii vine în același panou.
export function DescarcaDeclaratie({ formular, cale, refuzuri, incarcat, bife, valori, seteaza, descarcat }: {
  formular: 'D300' | 'D394';
  cale: string;
  refuzuri: Refuz[];
  incarcat: boolean;
  bife: readonly Bifa[];
  valori: Record<string, string>;
  seteaza: (v: Record<string, string>) => void;
  descarcat?: (fisier: FisierExportat | undefined) => void;
}) {
  const [descarcare, setDescarcare] = useState<{ activa: boolean; erori: string[]; cerere: string }>(
    { activa: false, erori: [], cerere: '' });
  const motive = refuzuri.map((r) => `${r.Cod}: ${r.Mesaj}`);

  async function descarca() {
    setDescarcare({ activa: true, erori: [], cerere: cale });
    try {
      const antete = await descarcaFisier(cale, `${formular}.xml`, 'application/xml, application/json');
      const [versiune, sha, parametri] = ['X-Versiune-Exportata', 'X-Sha256-Fisier', 'X-Parametri-Fisier']
        .map((a) => antete.get(a));
      descarcat?.(versiune && sha && parametri
        ? { VersiuneExportata: versiune, Sha256Fisier: sha, ParametriFisier: parametri } : undefined);
      setDescarcare({ activa: false, erori: [], cerere: cale });
    }
    catch (e) {
      setDescarcare({ activa: false, erori: eroriDin(e), cerere: cale });
    }
  }

  return (
    <div className="declaratie-xml">
      <div className="bara-raport">
        {bife.map(({ cheie, eticheta }) => (
          <label key={cheie} className="bara-raport__camp">
            <input
              type="checkbox"
              checked={valori[cheie] === '1'}
              onChange={(e) => seteaza({ [cheie]: e.target.checked ? '1' : '0' })}
            />
            <span className="camp__eticheta">{eticheta}</span>
          </label>
        ))}
        <button
          type="button"
          className="buton buton--primar"
          disabled={!incarcat || descarcare.activa || motive.length > 0}
          title={motive.length > 0 ? 'Fișierul se refuză până la rezolvarea cauzelor de mai jos' : undefined}
          onClick={descarca}
        >
          {descarcare.activa ? 'Se generează…' : `Descarcă XML ${formular}`}
        </button>
      </div>
      <PanouErori erori={descarcare.cerere === cale ? descarcare.erori : []} titlu="Fișierul nu s-a putut genera" />
      <PanouErori erori={motive} titlu="Fișierul se refuză până la rezolvarea acestor cauze" />
    </div>
  );
}
