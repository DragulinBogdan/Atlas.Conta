import type { ReactNode } from 'react';
import type { CampMeta } from './campMeta';

// `CampShell` (43a): label din caption, marker de obligativitate, slot de
// control, slot de eroare. Editorii tipizați sunt FEȚE SUBȚIRI peste el — un
// editor nou înseamnă doar un control de input, nu un layout nou și nici
// stringuri noi (mesajele sunt șabloane pe felul regulii, în `formular.tsx`).
export function CampShell(props: { meta: CampMeta; eroare?: string | null; children: ReactNode }) {
  const { meta, eroare, children } = props;
  return (
    <div className={`camp${eroare ? ' camp--eroare' : ''}`}>
      <label className="camp__eticheta">
        {meta.caption}
        {meta.obligatoriu && <span className="camp__obligatoriu" title="obligatoriu"> *</span>}
      </label>
      <div className="camp__control">{children}</div>
      <div className="camp__eroare">{eroare}</div>
    </div>
  );
}
