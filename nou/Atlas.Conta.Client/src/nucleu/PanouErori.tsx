// Panoul `Erori[]` (43a): singurul mod în care clientul afișează verdictul
// serverului. Aceeași formă pentru dry-run (`POST .../valideaza`), pentru 422
// și pentru orice excepție prinsă — un singur loc de randare, zero interpretare.
//
// `fel = 'atentie'` (felia 12) e a treia formă: `Avertismente[]` — o listă care
// NU e un refuz și nici o confirmare. D300 le produce (perioadă proiectată pe
// formularul altui an, TVA ajuns pe un rând fără coloană de TVA); afișate ca
// erori ar fi speriat degeaba, afișate ca `indiciu` s-ar fi pierdut în text.
export function PanouErori(props: { erori: string[]; titlu?: string; fel?: 'eroare' | 'succes' | 'atentie' }) {
  const { erori, titlu, fel = 'eroare' } = props;
  if (erori.length === 0) return null;
  return (
    <div className={`panou panou--${fel}`}>
      {titlu && <div className="panou__titlu">{titlu}</div>}
      <ul className="panou__lista">
        {erori.map((e, i) => <li key={i}>{e}</li>)}
      </ul>
    </div>
  );
}
