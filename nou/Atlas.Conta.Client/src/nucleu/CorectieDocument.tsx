import { useState } from 'react';
import { Link, useNavigate } from 'react-router';
import { useQueryClient } from '@tanstack/react-query';
import { DateBox, SelectBox } from 'devextreme-react';
import type { components } from '../generated/api-types';
import { PanouErori } from './PanouErori';
import { eroriDin, posteaza } from './http';
import { labelEnum, valoriEnum } from './campMeta';
import { rutaTip } from './stingeri';
import { azi, izolataZi } from './zi';

// F27-D6 — corecția unui document operat, pe ORICE ecran de document: butonul
// cu dialogul lui (data + motivul) și banda „Corectează pe …". Componentă a
// NUCLEULUI, nu a unei felii: comanda e a BAZEI (`api/documente/{id}/corecteaza`,
// documentul se rezolvă polimorf), deci un exemplar per felie ar fi fost 15
// copii ale aceleiași reguli.
//
// Ce NU face: nu decide DACĂ se poate corecta. Butonul apare pe documentul
// OPERAT, iar tot ce poate refuza comanda (perioada stornării închisă,
// dependenți, împerecheri, original deja corectat) rămâne al serverului, cu
// fraza lui — clientul nu re-derivă gardienii motorului (42e/43b).
//
// După succes NAVIGHEAZĂ la draftul nou: corecția e „storno + document nou", iar
// documentul pe care stă operatorul tocmai a devenit `Stornat`. Ruta vine din
// `TipCod` prin `rutaTip` (vocabular închis, în nucleu), nu dintr-un `toLowerCase`
// pe un cod necunoscut.

type Scheme = components['schemas'];
export type CorectieInfo = Scheme['CorectieDto'];
type CorectieRezultat = Scheme['CorectieRezultatDto'];

export function CorectieDocument(props: {
  id?: string | null;
  stare?: string | null;
  corectie?: CorectieInfo | null;
}) {
  const { id, stare, corectie } = props;
  const navigheaza = useNavigate();
  const cache = useQueryClient();
  const [deschis, setDeschis] = useState(false);
  const [data, setData] = useState<string | undefined>(azi());
  const [motiv, setMotiv] = useState('EroareMateriala');
  const [erori, setErori] = useState<string[]>([]);
  const [ocupat, setOcupat] = useState(false);

  async function corecteaza() {
    if (!id || !data) return;
    setErori([]);
    setOcupat(true);
    try {
      const rezultat = await posteaza<CorectieRezultat>(
        `/api/documente/${id}/corecteaza`, { Data: data, Motiv: motiv });
      setDeschis(false);
      // Originalul e acum stornat și există un document nou: nicio listă și
      // niciun detaliu deschis nu mai e la zi.
      await cache.invalidateQueries();
      const ruta = rezultat.CorectieId ? rutaTip(rezultat.TipCod, rezultat.CorectieId) : null;
      if (ruta) navigheaza(ruta);
    }
    catch (e) {
      setErori(eroriDin(e));
    }
    finally {
      setOcupat(false);
    }
  }

  const original = corectie?.OriginalId ? rutaTip(tipDinRuta(), corectie.OriginalId) : null;
  return (
    <>
      {corectie && (
        <div className="panou panou--atentie">
          <div className="panou__titlu">Document de corecție</div>
          <div>
            Corectează pe {original
              ? <Link to={original}>{corectie.Eticheta}</Link>
              : <strong>{corectie.Eticheta}</strong>}
            {' '}— {labelEnum('MotivCorectie', corectie.Motiv)}
          </div>
        </div>
      )}

      {id && stare === 'Operat' && !deschis && (
        <div className="cerere-data">
          <button type="button" className="buton" onClick={() => setDeschis(true)}>Corectează…</button>
          <span>Stornează documentul la data aleasă și deschide un draft nou cu aceeași culegere.</span>
        </div>
      )}

      {id && stare === 'Operat' && deschis && (
        <div className="cerere-data">
          <label className="camp__eticheta">Data corecției</label>
          <DateBox
            type="date"
            displayFormat="dd.MM.yyyy"
            value={data ?? null}
            onValueChanged={(e) => setData(izolataZi(e.value))}
          />
          <label className="camp__eticheta">Motivul corecției</label>
          <SelectBox
            dataSource={valoriEnum('MotivCorectie')}
            valueExpr="valoare"
            displayExpr="label"
            value={motiv}
            onValueChanged={(e) => setMotiv(String(e.value))}
          />
          <button
            type="button"
            className="buton buton--primar"
            disabled={!data || ocupat}
            onClick={() => void corecteaza()}
          >
            Corectează
          </button>
          <button type="button" className="buton" onClick={() => { setDeschis(false); setErori([]); }}>
            Renunță
          </button>
        </div>
      )}

      <PanouErori erori={erori} titlu="Corecția a fost refuzată" />
    </>
  );
}

// Ruta originalului nu se poate deduce din DTO (legătura poartă eticheta, nu
// codul tipului) — dar originalul e ÎNTOTDEAUNA de același tip concret cu
// corecția (invariantul F27-D6), deci ruta lui e chiar ecranul curent.
function tipDinRuta(): string | null {
  const segment = window.location.pathname.split('/').filter((s) => s.length > 0)[0];
  return segment ? segment.toUpperCase() : null;
}
