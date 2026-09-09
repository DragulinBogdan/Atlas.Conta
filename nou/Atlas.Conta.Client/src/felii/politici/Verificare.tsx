import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { labelEnum } from '../../nucleu/campMeta';
import { PanouErori } from '../../nucleu/PanouErori';
import { eroriDin, ia } from '../../nucleu/http';
import type { components } from '../../generated/api-types';

// Raportul de verificare a profilului (F23-D8): ce s-a abătut de la pachetul de
// seed și ce a rămas neconfigurat. Seed-ul e cel care ARUNCĂ; raportul e cel
// care ARATĂ.
//
// Două lucruri de reținut despre ruta asta, măsurate:
//   • Verdictul se calculează pe ușa NON-SECURED, ca cifra să fie a BAZEI. Pe
//     ușa filtrată raportul nu ieșea gol, ieșea FALS („tipul N21 nu există în
//     bază", pentru cine nu-l vede). De aceea ruta cere dreptul de CITIRE pe tot
//     ce citește, iar cine nu-l are primește 403 — nu o listă goală, care ar fi
//     o afirmație mincinoasă despre profil.
//   • Lista goală e, dimpotrivă, o afirmație ADEVĂRATĂ: profilul e cel al
//     seed-ului, nimic manual, nicio referință ruptă.
//
// Zero interpretare în TS: felul, tabelul, cheia și mesajul vin gata din DTO;
// gruparea e doar prezentare.

type Constatare = components['schemas']['ConstatareProfilDto'];

export function Verificare() {
  const raport = useQuery({
    queryKey: ['politici', 'verificare'],
    queryFn: () => ia<Constatare[]>('/api/politici/verificare'),
  });

  const grupuri = useMemo(() => {
    const harta = new Map<string, Constatare[]>();
    for (const c of raport.data ?? []) {
      const fel = c.Fel ?? '';
      const lista = harta.get(fel);
      if (lista) lista.push(c);
      else harta.set(fel, [c]);
    }
    return [...harta.entries()];
  }, [raport.data]);

  return (
    <div className="ecran">
      <div className="ecran__bara">
        <h2>Verificarea profilului</h2>
        <button
          type="button"
          className="buton"
          disabled={raport.isFetching}
          onClick={() => void raport.refetch()}
        >
          {raport.isFetching ? 'Se verifică…' : 'Reverifică'}
        </button>
      </div>

      {raport.isError && <PanouErori erori={eroriDin(raport.error)} titlu="Refuzat de server" />}

      {!raport.isError && raport.isPending && <p className="indiciu">Se citește…</p>}

      {!raport.isError && !raport.isPending && grupuri.length === 0 && (
        <p className="indiciu">Profilul nu are constatări.</p>
      )}

      {grupuri.map(([fel, constatari]) => (
        <section key={fel} className="verificare__grup">
          <h3>{labelEnum('FelConstatare', fel)} <span className="verificare__contor">({constatari.length})</span></h3>
          <table className="tabel-mic">
            <thead>
              <tr><th>Tabel</th><th>Cheie</th><th>Mesaj</th></tr>
            </thead>
            <tbody>
              {constatari.map((c, i) => (
                <tr key={`${c.Tabel}-${c.Cheie}-${i}`}>
                  <td>{c.Tabel ?? ''}</td>
                  <td>{c.Cheie ?? ''}</td>
                  <td className="verificare__mesaj">{c.Mesaj ?? ''}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </section>
      ))}

      <p className="indiciu">
        „Rând creat sau editat manual" nu e o eroare: e semnalul că rândul s-a abătut de la
        pachetul de seed și că un re-seed nu-l va mai corecta. Referințele spre rânduri șterse și
        tipurile de TVA inactive referite ca implicit sunt însă defecte de configurare — motorul
        le va sări tăcut la culegere.
      </p>
    </div>
  );
}
