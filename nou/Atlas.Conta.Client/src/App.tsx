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
import { PltLista } from './felii/plt/PltLista';
import { PltDetaliu } from './felii/plt/PltDetaliu';
import { IncLista } from './felii/inc/IncLista';
import { IncDetaliu } from './felii/inc/IncDetaliu';
import { SoldStoc } from './felii/stoc/SoldStoc';

// URL-ul E starea globală (43c): deep-linking și refresh gratis, fără store de
// sincronizat. Ruta statică `/…/nou` e declarată ÎNAINTEA celei parametrice.
// NIR-ul n-are rută `/nou`: în felia asta nu se culege din client (F2-D3).
export function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route element={<Cadru />}>
        <Route path="/fct" element={<FctLista />} />
        <Route path="/fct/nou" element={<FctDetaliu />} />
        <Route path="/fct/:id" element={<FctDetaliu />} />
        <Route path="/nir" element={<NirLista />} />
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
        <Route path="/btr" element={<BtrLista />} />
        <Route path="/btr/nou" element={<BtrDetaliu />} />
        <Route path="/btr/:id" element={<BtrDetaliu />} />
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
        <NavLink to="/btr">Note de transfer</NavLink>
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
