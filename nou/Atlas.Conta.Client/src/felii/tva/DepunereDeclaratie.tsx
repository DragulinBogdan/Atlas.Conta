import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import type { components } from '../../generated/api-types';
import { ia, posteaza, eroriDin } from '../../nucleu/http';
import { PanouErori } from '../../nucleu/PanouErori';

export function DepunereDeclaratie({ formular, dataStart, dataEnd, exportat, reincarca }: {
  formular: 'D300' | 'D394'; dataStart: string; dataEnd: string;
  exportat?: { VersiuneExportata?: string | null };
  reincarca: () => Promise<{ data?: { VersiuneExportata?: string | null }; error: Error | null }>;
}) {
  const [versiune, setVersiune] = useState('');
  const cache = useQueryClient();
  const luna = dataStart.slice(0, 7);
  const lunaExacta = dataStart.endsWith('-01') && dataEnd.slice(0, 7) === luna
    && Number(dataEnd.slice(8)) === new Date(Number(dataStart.slice(0, 4)), Number(dataStart.slice(5, 7)), 0).getDate();
  const ruta = `/api/depuneri-declaratii/${formular}/${dataStart.slice(0, 4)}/${Number(dataStart.slice(5, 7))}`;
  const citit = useQuery({ queryKey: ['depunere', ruta], enabled: lunaExacta,
    queryFn: () => ia<components['schemas']['DepunereDeclaratieDto']>(ruta), retry: false });
  const confirma = useMutation({ mutationFn: () => posteaza(ruta, { VersiuneExportata: versiune }),
    onSuccess: async () => {
      setVersiune('');
      await cache.invalidateQueries({ queryKey: ['depunere'] });
      await cache.invalidateQueries({ queryKey: [formular.toLowerCase()] });
    } });
  const exporta = useMutation({ mutationFn: async () => {
    const rezultat = await reincarca();
    if (rezultat.error) throw rezultat.error;
    const date = rezultat.data;
    if (!date?.VersiuneExportata) return;
    const url = URL.createObjectURL(new Blob([JSON.stringify(date, null, 2)], { type: 'application/json' }));
    const link = document.createElement('a');
    link.href = url; link.download = `${formular}-${luna}.json`; link.click();
    URL.revokeObjectURL(url);
    setVersiune(date.VersiuneExportata);
    confirma.reset();
  } });
  if (!lunaExacta || !citit.data) return null;
  return <div className="d300__neincluse">
    <p>{citit.data.ConfirmataLa
      ? `Depunere confirmată: ${citit.data.VersiuneExportata}, ${new Date(citit.data.ConfirmataLa).toLocaleString('ro-RO')}.`
      : 'Depunerea nu este confirmată. Închiderea contabilă nu confirmă depunerea declarației.'}</p>
    {citit.data.PoateConfirma && <form onSubmit={e => { e.preventDefault(); confirma.mutate(); }}>
      <button type="button" className="buton" onClick={() => exporta.mutate()} disabled={exporta.isPending || !exportat?.VersiuneExportata}>
        Exportă datele {formular} (JSON)
      </button>{' '}
      <p>Export de date cu amprentă; nu este XML ANAF. Confirmați numai după depunerea declarației corespunzătoare.</p>
      {versiune && <p>Versiunea exportată: <code>{versiune}</code></p>}
      <button type="submit" className="buton" disabled={confirma.isPending || exporta.isPending || !versiune.trim()}>
        Confirmă depunerea {formular}
      </button>
    </form>}
    <PanouErori erori={exporta.error ? eroriDin(exporta.error) : []} />
    <PanouErori erori={confirma.error ? eroriDin(confirma.error) : []} />
  </div>;
}
