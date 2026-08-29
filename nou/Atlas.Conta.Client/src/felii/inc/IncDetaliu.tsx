import { Lookup } from '../../nucleu/Lookup';
import { LaturaContrapartida } from '../trz/LaturaContrapartida';
import { TrezorerieDetaliu } from '../trz/TrezorerieDetaliu';
import type { TrzWrite } from '../trz/api';
import { inc, RUTA, TIP_ANTET } from './api';

// Ecranul de ÎNCASARE — oglinda plății (31a), cu laturile inversate:
//
//   predator = PLĂTITORUL (partener, angajat — restituirea de avans — sau, la
//              viramentul intern, un al doilea CONT PROPRIU: F7-D1),
//   primitor = CONTUL PROPRIU în care intră banul.
//
// Contrapartida e deci PREDATORUL — exact latura pe care o normalizează ramura
// INC a proiecției de rest (F3-D4) și tot ea spune, prin felul ei, dacă
// documentul e un picior de virament (F7-D8).
export function IncDetaliu() {
  return (
    <TrezorerieDetaliu
      api={inc}
      ruta={RUTA}
      cheieCache="inc"
      tip={TIP_ANTET}
      titluNou="Încasare — nouă"
      titluExistent={(numar) => `Încasare ${numar}`}
      campContrapartida="PredatorId"
      laturi={
        <>
          <LaturaContrapartida<TrzWrite> camp="PredatorId" eticheta="Plătitor" />
          <Lookup<TrzWrite>
            camp="PrimitorId"
            eticheta="Cont propriu (în care se încasează)"
            entitate="ContPropriu"
            mod="local"
            cauta={['Cautare', 'Iban']}
          />
        </>
      }
    />
  );
}
