import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router';
import { autentifica } from '../nucleu/auth';
import { PanouErori } from '../nucleu/PanouErori';

export function Login() {
  const navigheaza = useNavigate();
  const [utilizator, setUtilizator] = useState('Admin');
  const [parola, setParola] = useState('');
  const [erori, setErori] = useState<string[]>([]);
  const [ocupat, setOcupat] = useState(false);

  async function intra(e: FormEvent) {
    e.preventDefault();
    setErori([]);
    setOcupat(true);
    try {
      await autentifica(utilizator, parola);
      navigheaza('/btr', { replace: true });
    }
    catch (ex) {
      setErori([ex instanceof Error ? ex.message : String(ex)]);
    }
    finally {
      setOcupat(false);
    }
  }

  return (
    <div className="login">
      <form className="login__card" onSubmit={intra}>
        <h1>Atlas Conta</h1>
        <label>
          Utilizator
          <input value={utilizator} onChange={(e) => setUtilizator(e.target.value)} autoComplete="username" />
        </label>
        <label>
          Parolă
          <input type="password" value={parola} onChange={(e) => setParola(e.target.value)} autoComplete="current-password" />
        </label>
        <PanouErori erori={erori} />
        <button type="submit" className="buton buton--primar" disabled={ocupat}>
          {ocupat ? 'Se autentifică…' : 'Intră'}
        </button>
      </form>
    </div>
  );
}
