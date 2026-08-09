import { Lookup } from '../../nucleu/Lookup';
import { LaturaContrapartida } from '../trz/LaturaContrapartida';
import { TrezorerieDetaliu } from '../trz/TrezorerieDetaliu';
import type { TrzWrite } from '../trz/api';
import { inc, RUTA, TIP_ANTET } from './api';

// Ecranul de ÎNCASARE — oglinda plății (31a), cu laturile inversate:
//
//   predator = PLĂTITORUL (partener sau angajat — restituirea de avans),
//   primitor = CONTUL PROPRIU în care intră banul.
//
// Contrapartida pentru panoul de stingeri e deci PREDATORUL — exact latura pe
// care o normalizează ramura INC a proiecției de rest (F3-D4).
export function IncDetaliu() {
  return (
    <TrezorerieDetaliu
      api={inc}
      ruta={RUTA}
      cheieCache="inc"
      tip={TIP_ANTET}
      titluNou="Încasare — nouă"
      titluExistent={(numar) => `Încasare ${numar}`}
      contrapartida={(doc) => doc.PredatorId}
      laturi={
        <>
          <LaturaContrapartida<TrzWrite> camp="PredatorId" eticheta="Plătitor" />
          <Lookup<TrzWrite>
            camp="PrimitorId"
            eticheta="Cont propriu (în care se încasează)"
            entitate="ContPropriu"
            mod="local"
            cauta={['Cod', 'Denumire', 'Iban']}
          />
        </>
      }
    />
  );
}
