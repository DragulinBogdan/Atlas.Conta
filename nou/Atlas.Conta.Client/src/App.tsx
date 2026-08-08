import { NavLink, Navigate, Outlet, Route, Routes, useNavigate } from 'react-router';
import { esteAutentificat, stergeToken } from './nucleu/auth';
import { Login } from './pagini/Login';
import { BtrLista } from './felii/btr/BtrLista';
import { BtrDetaliu } from './felii/btr/BtrDetaliu';
import { SoldStoc } from './felii/stoc/SoldStoc';

// URL-ul E starea globală (43c): deep-linking și refresh gratis, fără store de
// sincronizat. Ruta statică `/btr/nou` e declarată ÎNAINTEA celei parametrice.
export function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route element={<Cadru />}>
        <Route path="/btr" element={<BtrLista />} />
        <Route path="/btr/nou" element={<BtrDetaliu />} />
        <Route path="/btr/:id" element={<BtrDetaliu />} />
        <Route path="/stoc" element={<SoldStoc />} />
        <Route path="*" element={<Navigate to="/btr" replace />} />
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
