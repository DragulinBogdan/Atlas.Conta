import { TrezorerieLista } from '../trz/TrezorerieLista';
import { inc, RUTA, TIP_ANTET } from './api';

// Oglinda plății: predatorul E plătitorul, primitorul E contul propriu în care
// intră banul.
export function IncLista() {
  return (
    <TrezorerieLista
      api={inc}
      ruta={RUTA}
      titlu="Încasări"
      tip={TIP_ANTET}
      capPredator="Plătitor"
      capPrimitor="Cont propriu"
    />
  );
}
