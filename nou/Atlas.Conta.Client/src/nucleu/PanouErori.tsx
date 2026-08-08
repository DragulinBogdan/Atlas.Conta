// Panoul `Erori[]` (43a): singurul mod în care clientul afișează verdictul
// serverului. Aceeași formă pentru dry-run (`POST .../valideaza`), pentru 422
// și pentru orice excepție prinsă — un singur loc de randare, zero interpretare.
export function PanouErori(props: { erori: string[]; titlu?: string; fel?: 'eroare' | 'succes' }) {
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
