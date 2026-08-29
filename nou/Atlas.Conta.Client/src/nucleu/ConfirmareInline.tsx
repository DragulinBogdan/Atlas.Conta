import type { ReactNode } from 'react';

// Confirmarea unei acțiuni ireversibile, INLINE (57f). `window.confirm` a fost
// respins la smoke F3 din două motive care nu se repară: dialogul nativ
// BLOCHEAZĂ renderer-ul (spinner-ul comenzii îngheață sub el) și nu se poate
// stiliza, deci nu poate spune CE se pierde — un „OK/Cancel" fără context.
//
// Forma e cea deja folosită de `PanouStingeri` și de distribuirea ASM: un rând
// `.cerere-data` cu întrebarea, verbul acțiunii pe butonul primar și „Renunță".
// Componenta NU cunoaște acțiunea: primește ce să întrebe și ce să cheme.
//
// Ce NU face: nu ține stare (cine o montează decide când există) și n-are slot
// de formular. Cererile care culeg un PARAMETRU (data stornării, suma unei
// stingeri) rămân blocuri proprii, cu `DateBox`/`NumberBox` — un slot generic
// de formular pentru două cazuri ar fi o abstracție mai mare decât cazurile.
export function ConfirmareInline(props: {
  intrebare: ReactNode;
  // Verbul acțiunii, pe butonul primar („Șterge", „Distribuie"). Nu „OK":
  // butonul trebuie să spună ce face, nu că a fost citit.
  verb: string;
  ocupat?: boolean;
  onConfirma: () => void;
  onRenunta: () => void;
}) {
  const { intrebare, verb, ocupat = false, onConfirma, onRenunta } = props;
  return (
    <div className="cerere-data">
      <span>{intrebare}</span>
      <button type="button" className="buton buton--primar" disabled={ocupat} onClick={onConfirma}>
        {verb}
      </button>
      <button type="button" className="buton" onClick={onRenunta}>Renunță</button>
    </div>
  );
}
