import { NavLink, Navigate, Outlet, Route, Routes, useNavigate } from 'react-router';
import { esteAutentificat, stergeToken } from './nucleu/auth';
import { Login } from './pagini/Login';
import { BtrLista } from './felii/btr/BtrLista';
import { BtrDetaliu } from './felii/btr/BtrDetaliu';
import { FctLista } from './felii/fct/FctLista';
import { FctDetaliu } from './felii/fct/FctDetaliu';
import { NirLista } from './felii/nir/NirLista';
import { NirDetaliu } from './felii/nir/NirDetaliu';
import { FclLista } from './felii/fcl/FclLista';
import { FclDetaliu } from './felii/fcl/FclDetaliu';
import { DscLista } from './felii/dsc/DscLista';
import { DscDetaliu } from './felii/dsc/DscDetaliu';
import { BcsLista } from './felii/bcs/BcsLista';
import { BcsDetaliu } from './felii/bcs/BcsDetaliu';
import { LdiLista } from './felii/ldi/LdiLista';
import { LdiDetaliu } from './felii/ldi/LdiDetaliu';
import { PltLista } from './felii/plt/PltLista';
import { PltDetaliu } from './felii/plt/PltDetaliu';
import { IncLista } from './felii/inc/IncLista';
import { IncDetaliu } from './felii/inc/IncDetaliu';
import { DecLista } from './felii/dec/DecLista';
import { DecDetaliu } from './felii/dec/DecDetaliu';
import { SoldStoc } from './felii/stoc/SoldStoc';

// URL-ul E starea globală (43c): deep-linking și refresh gratis, fără store de
// sincronizat. Ruta statică `/…/nou` e declarată ÎNAINTEA celei parametrice.
export function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route element={<Cadru />}>
        <Route path="/fct" element={<FctLista />} />
        <Route path="/fct/nou" element={<FctDetaliu />} />
        <Route path="/fct/:id" element={<FctDetaliu />} />
        <Route path="/nir" element={<NirLista />} />
        {/* Recepția fără factură se culege manual (F5): NIR-ul are rută `/nou`
            de la felia 5, pe lângă clona conexă născută de operarea facturii. */}
        <Route path="/nir/nou" element={<NirDetaliu />} />
        <Route path="/nir/:id" element={<NirDetaliu />} />
        <Route path="/fcl" element={<FclLista />} />
        <Route path="/fcl/nou" element={<FclDetaliu />} />
        <Route path="/fcl/:id" element={<FclDetaliu />} />
        {/* DSC n-are rută `/nou`: descărcarea se naște din operarea facturii sau
            din comanda de backorder, niciodată din culegere (F4-D2). */}
        <Route path="/dsc" element={<DscLista />} />
        <Route path="/dsc/:id" element={<DscDetaliu />} />
        <Route path="/plt" element={<PltLista />} />
        <Route path="/plt/nou" element={<PltDetaliu />} />
        <Route path="/plt/:id" element={<PltDetaliu />} />
        <Route path="/inc" element={<IncLista />} />
        <Route path="/inc/nou" element={<IncDetaliu />} />
        <Route path="/inc/:id" element={<IncDetaliu />} />
        <Route path="/dec" element={<DecLista />} />
        <Route path="/dec/nou" element={<DecDetaliu />} />
        <Route path="/dec/:id" element={<DecDetaliu />} />
        <Route path="/btr" element={<BtrLista />} />
        <Route path="/btr/nou" element={<BtrDetaliu />} />
        <Route path="/btr/:id" element={<BtrDetaliu />} />
        <Route path="/bcs" element={<BcsLista />} />
        <Route path="/bcs/nou" element={<BcsDetaliu />} />
        <Route path="/bcs/:id" element={<BcsDetaliu />} />
        <Route path="/ldi" element={<LdiLista />} />
        <Route path="/ldi/nou" element={<LdiDetaliu />} />
        <Route path="/ldi/:id" element={<LdiDetaliu />} />
        <Route path="/stoc" element={<SoldStoc />} />
        <Route path="*" element={<Navigate to="/fct" replace />} />
      </Route>
    </Routes>
  );
}

function Cadru() {
  const navigheaza = useNavigate();
  if (!esteAutentificat())
    return <Navigate to="/login" replace />;

  return (
    <div className="cadru">
      <nav className="cadru__meniu">
        <span className="cadru__marca">Atlas Conta</span>
        <NavLink to="/fct">Facturi intrare</NavLink>
        <NavLink to="/nir">NIR-uri</NavLink>
        <NavLink to="/fcl">Facturi ieșire</NavLink>
        <NavLink to="/dsc">Descărcări</NavLink>
        <NavLink to="/plt">Plăți</NavLink>
        <NavLink to="/inc">Încasări</NavLink>
        <NavLink to="/dec">Deconturi</NavLink>
        <NavLink to="/btr">Note de transfer</NavLink>
        <NavLink to="/bcs">Bonuri de consum</NavLink>
        <NavLink to="/ldi">Diferențe inventar</NavLink>
        <NavLink to="/stoc">Sold stoc</NavLink>
        <button
          type="button"
          className="buton buton--mic"
          onClick={() => { stergeToken(); navigheaza('/login', { replace: true }); }}
        >
          Ieșire
        </button>
      </nav>
      <main className="cadru__continut"><Outlet /></main>
    </div>
  );
}
