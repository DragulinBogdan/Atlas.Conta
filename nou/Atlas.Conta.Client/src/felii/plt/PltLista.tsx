import { TrezorerieLista } from '../trz/TrezorerieLista';
import { plt, RUTA, TIP_ANTET } from './api';

// Laturile plății, numite în vocabularul feliei: predatorul E contul propriu
// din care iese banul, primitorul E beneficiarul.
export function PltLista() {
  return (
    <TrezorerieLista
      api={plt}
      ruta={RUTA}
      titlu="Plăți"
      tip={TIP_ANTET}
      capPredator="Cont propriu"
      capPrimitor="Beneficiar"
    />
  );
}
