import { Lookup } from '../../nucleu/Lookup';
import { LaturaContrapartida } from '../trz/LaturaContrapartida';
import { TrezorerieDetaliu } from '../trz/TrezorerieDetaliu';
import type { TrzWrite } from '../trz/api';
import { plt, RUTA, TIP_ANTET } from './api';

// Ecranul de PLATĂ. Tot ce e formă comună stă în `TrezorerieDetaliu`; aici stă
// IDENTITATEA — direcția banului, scrisă ca JSX, nu ca descriptor (43a):
//
//   predator = CONTUL PROPRIU din care se plătește (casă/bancă/trezorerie),
//   primitor = BENEFICIARUL (partener, angajat — avansul 542, 31a — sau, la
//              viramentul intern, un al doilea CONT PROPRIU: F7-D1).
//
// Contrapartida e deci PRIMITORUL — aceeași latură pe care o normalizează
// proiecția de rest pentru ramura PLT (F3-D4) și tot ea spune, prin felul ei,
// dacă documentul e un picior de virament (F7-D8).
export function PltDetaliu() {
  return (
    <TrezorerieDetaliu
      api={plt}
      ruta={RUTA}
      cheieCache="plt"
      tip={TIP_ANTET}
      titluNou="Plată — nouă"
      titluExistent={(numar) => `Plată ${numar}`}
      campContrapartida="PrimitorId"
      laturi={
        <>
          {/* Conturile proprii sunt câteva (casierii + conturi bancare) ⇒ local. */}
          <Lookup<TrzWrite>
            camp="PredatorId"
            eticheta="Cont propriu (din care se plătește)"
            entitate="ContPropriu"
            mod="local"
            cauta={['Cod', 'Denumire', 'Iban']}
          />
          <LaturaContrapartida<TrzWrite> camp="PrimitorId" eticheta="Beneficiar" />
        </>
      }
    />
  );
}
