import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { Link, useNavigate, useParams } from 'react-router';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Column, DataGrid } from 'devextreme-react/data-grid';
import { DocumentShell, type Comanda } from '../../nucleu/DocumentShell';
import { ConfirmareInline } from '../../nucleu/ConfirmareInline';
import { Formular, eroriStructurale } from '../../nucleu/formular';
import { CampData, CampOptiuni, CampSelectie, CampText } from '../../nucleu/campuri';
import { PanouErori } from '../../nucleu/PanouErori';
import { PanouStingeri } from '../../nucleu/PanouStingeri';
import { campMeta, labelEnum } from '../../nucleu/campMeta';
import { eroriDin } from '../../nucleu/http';
import { useSonda } from '../../nucleu/odata';
import { rutaTip } from '../../nucleu/stingeri';
import { ziLocala } from '../../nucleu/zi';
import {
  antetGol, linieGoala, spreWrite,
  SCHEMA_ANTET, SCHEMA_LINIE, TIP_LINIE,
  type ApiTrezorerie, type CandidatPereche, type DocumentCopil,
  type TrzLinieRead, type TrzLinieWrite, type TrzRead, type TrzWrite,
} from './api';
import { TrezorerieEditorLinie } from './TrezorerieEditorLinie';

// Ecranul de document al trezoreriei — forma comună a celor două felii (același
// argument ca pe server: `TrezorerieApply` e generic, controllerele sunt
// subțiri). IDENTITATEA rămâne în felie și vine ca props: ruta, titlurile,
// captions-urile laturilor și **JSX-ul laturilor** — cine e contul propriu și
// cine e contrapartida se scrie acolo, în cod, nu se derivă aici.
//
// Peste șablonul BTR/FCT, felia asta aduce:
//   • `Numar` NU se culege (server-owned — PoliticaNumerotare): se AFIȘEAZĂ;
//   • `Autogenerat` + `DocumentSursa` — plata născută din factură (31e) e un
//     document normal, dar operatorul trebuie să vadă de unde vine;
//   • **panoul de STINGERI** pe document OPERAT: trezoreria e stingătorul
//     „clasic" (31d), deci rolul ei e `stinge`;
//   • **modul VIRAMENT INTERN** (F7-D8): când contrapartida e un al doilea CONT
//     PROPRIU, documentul nu mai e o plată/încasare către un terț, ci un picior
//     al transferului 581 — nu stinge nimic (`CapacitateStingere` e null pe el)
//     și naște la operare latura pereche. Ecranul își schimbă forma: Tipul
//     implicit al liniei, panoul de stingeri înlocuit de panoul perechii;
//   • **latura pereche DECLARATĂ** (F8-D12): pe un virament în culegere,
//     operatorul poate arăta piciorul EXISTENT în loc să lase motorul să
//     genereze unul — singurul mod de a închide gaura 64k (două picioare culese
//     manual generau, tăcut, un al treilea document și dublau postarea).
//     „(niciuna)" rămâne default-ul: generarea automată E comportamentul normal.

const CAMPURI_ANTET: (keyof TrzWrite & string)[] = ['Data', 'PredatorId', 'PrimitorId'];
const capLinie = (m: string) => campMeta(TIP_LINIE, m, SCHEMA_LINIE).caption;

// Tipul tehnic cu care se precompletează linia nouă, per mod (F7-D8/31a).
const COD_TIP_TRZ = 'TRZ';
const COD_TIP_VIRAMENT = 'VIR';

export function TrezorerieDetaliu(props: {
  api: ApiTrezorerie;
  ruta: string;
  // Cheia de cache a feliei (`plt`/`inc`) — invalidarea listei după orice
  // comandă.
  cheieCache: string;
  // Tipul de METADATA (`Plata`/`Incasare`) — sursa captions-urilor.
  tip: string;
  titluNou: string;
  titluExistent: (numar: string) => string;
  // Laturile, scrise în felie: contul propriu + contrapartida, în ordinea
  // fluxului real (PLT: din ce cont → cui; INC: de la cine → în ce cont).
  laturi: ReactNode;
  // CÂMPUL contrapartidei (PLT: primitorul; INC: predatorul) — nu valoarea:
  // shell-ul are nevoie și de cea SALVATĂ (candidații panoului de stingeri), și
  // de cea în curs de culegere (modul virament se citește din formular, sursa
  // de adevăr, nu din comutatorul de fel al laturii).
  campContrapartida: 'PredatorId' | 'PrimitorId';
}) {
  const { api, ruta, cheieCache, tip, titluNou, titluExistent, laturi, campContrapartida } = props;
  const { id } = useParams();
  const nou = id === 'nou' || id === undefined;
  const navigheaza = useNavigate();
  const cache = useQueryClient();

  const [agregat, setAgregat] = useState<TrzWrite>(antetGol);
  const [modificat, setModificat] = useState(false);
  const [aratErori, setAratErori] = useState(false);
  const [erori, setErori] = useState<string[]>([]);
  const [mesaje, setMesaje] = useState<string[]>([]);
  // Ștergerea draftului cere confirmare INLINE (F20-D4), nu `window.confirm`:
  // dialogul nativ blochează renderer-ul și nu poate spune CE se pierde.
  // Starea e a feliei; locul de randare îl dă `DocumentShell`.
  const [deSters, setDeSters] = useState(false);
  const [inEditare, setInEditare] = useState<TrzLinieWrite | null>(null);
  const [indiceEditat, setIndiceEditat] = useState<number | null>(null);

  const citit = useQuery({
    queryKey: [cheieCache, id],
    queryFn: () => api.citeste(id!),
    enabled: !nou,
  });

  // ReadDto proaspăt ⇒ formularul se re-seed-uiește. O singură direcție,
  // server → agregat, la fiecare recitire.
  useEffect(() => {
    if (citit.data) {
      setAgregat(spreWrite(citit.data));
      setModificat(false);
    }
  }, [citit.data]);

  const doc = citit.data;
  const poateEdita = nou || (doc?.PoateEdita ?? false);
  const linii = agregat.Linii ?? [];

  // ═══ Modul VIRAMENT: două surse, fiecare autoritară pe ce știe ═══
  // Pe documentul SALVAT decide SERVERUL (`EsteVirament` — predicatul domeniului,
  // AMBELE laturi conturi proprii); clientul nu-l reface din felul repartitorilor.
  // Pe culegerea NEsalvată serverul n-are ce spune încă, deci se sondează
  // formularul — dar cu EXACT aceeași definiție: AMBELE laturi în setul
  // `ContPropriu`. O sondă doar pe contrapartidă ar intra în modul virament pe un
  // draft cu laturile inversate (plată DE LA un partener CĂTRE casă), unde
  // contrapartida chiar e cont propriu fără ca documentul să fie virament, iar
  // mesajul de refuz al ecranului ar ieși mincinos. Cache-ul `useSonda` e pe
  // (entitate, id) și e comun cu selectorul de fel al laturii ⇒ a doua sondă e
  // ieftină. Cât timp nu știm (răspuns nevenit sau sondă eșuată — `undefined`),
  // modul rămâne cel obișnuit: default-ul care nu minte.
  const campContPropriu = campContrapartida === 'PredatorId' ? 'PrimitorId' : 'PredatorId';
  const culege = nou || modificat;
  const contrapartidaContPropriu = useSonda('ContPropriu', agregat[campContrapartida], culege);
  const propriuContPropriu = useSonda('ContPropriu', agregat[campContPropriu], culege);
  const esteVirament = culege
    ? contrapartidaContPropriu === true && propriuContPropriu === true
    : (doc?.EsteVirament ?? false);

  // ═══ Candidații de latură pereche (F8-D12) ═══
  // Mulțime calculată de SERVER (tip OPUS, aceleași laturi, fără pereche
  // definitivă), nu un nomenclator: de asta e `useQuery` + `CampOptiuni`, nu
  // `Lookup`. Se recitește când se schimbă laturile — ele SUNT criteriul.
  // Numai pe draft: pe un document ne-editabil câmpul nu se randează deloc, iar
  // starea reală a perechii o spune `PanouVirament` din ReadDto (`Pereche`).
  const candidatiActivi = esteVirament && poateEdita
    && agregat.PredatorId != null && agregat.PrimitorId != null;
  const candidati = useQuery({
    queryKey: [cheieCache, 'candidati-pereche', agregat.PredatorId, agregat.PrimitorId, id ?? 'nou'],
    queryFn: () => api.candidatiPereche(agregat.PredatorId!, agregat.PrimitorId!, nou ? undefined : id),
    enabled: candidatiActivi,
  });

  const optiuniPereche = useMemo(
    () => (candidati.data ?? []).map((c) => ({
      valoare: String(c.Id),
      label: etichetaCandidat(c),
    })),
    [candidati.data]);

  const structurale = useMemo(
    () => [
      ...eroriStructurale(tip, SCHEMA_ANTET, agregat as Record<string, unknown>, CAMPURI_ANTET),
      ...(linii.length === 0 ? ['Documentul nu are nicio linie.'] : []),
    ],
    [tip, agregat, linii.length]);

  function raporteaza(promisiune: Promise<string[]>) {
    setErori([]);
    setMesaje([]);
    return promisiune
      .then((m) => setMesaje(m))
      .catch((e) => setErori(eroriDin(e)));
  }

  const salvare = useMutation({
    mutationFn: async () => (nou ? api.creeaza(agregat) : api.actualizeaza(id!, agregat)),
    onSuccess: (salvat) => {
      setErori([]);
      setMesaje(['Salvat.']);
      setModificat(false);
      cache.invalidateQueries({ queryKey: [cheieCache] });
      if (nou) navigheaza(`${ruta}/${salvat.Id}`, { replace: true });
      else setAgregat(spreWrite(salvat));
    },
    onError: (e) => { setMesaje([]); setErori(eroriDin(e)); },
  });

  function salveaza() {
    setAratErori(true);
    if (structurale.length > 0) {
      setErori([]);
      setMesaje([]);
      return;
    }
    salvare.mutate();
  }

  async function comanda(rulare: () => Promise<{ Mesaje?: string[] | null; StareNoua?: string | null }>) {
    setErori([]);
    setMesaje([]);
    try {
      const rezultat = await rulare();
      setMesaje([`Stare nouă: ${labelEnum('StareDocument', rezultat.StareNoua)}`, ...(rezultat.Mesaje ?? [])]);
      await cache.invalidateQueries({ queryKey: [cheieCache] });
      // Operarea unui VIRAMENT naște (sau șterge, la anulare) latura pereche —
      // un document de tipul OPUS. Lista lui și, mai ales, candidații de pereche
      // ai altor picioare nu mai sunt de încredere; cheile ambelor felii de
      // trezorerie se invalidează, indiferent pe care dintre ele suntem.
      await cache.invalidateQueries({ queryKey: ['plt'] });
      await cache.invalidateQueries({ queryKey: ['inc'] });
      // Operarea unei plăți autogenerate creează imperecherea automată (31e), iar
      // anularea/stornarea o desface — panoul de stingeri al documentului (și al
      // facturii-sursă, dacă e deschisă în altă filă) nu mai e de încredere.
      await cache.invalidateQueries({ queryKey: ['stingeri'] });
      await cache.invalidateQueries({ queryKey: ['fct'] });
    }
    catch (e) {
      setErori(eroriDin(e));
    }
  }

  async function stergeDocumentul() {
    setDeSters(false);
    setErori([]);
    setMesaje([]);
    try {
      await api.sterge(id!);
      await cache.invalidateQueries({ queryKey: [cheieCache] });
      navigheaza(ruta);
    }
    catch (e) {
      setErori(eroriDin(e));
    }
  }

  const comenzi: Comanda[] = [
    { eticheta: nou ? 'Creează' : 'Salvează', disponibila: poateEdita, primara: true, ruleaza: salveaza },
    {
      eticheta: 'Verifică',
      disponibila: !nou && !modificat,
      ruleaza: () => void raporteaza(api.valideaza(id!).then((e) => {
        if (e.length > 0) { setErori(e); return []; }
        return ['Documentul trece toți gardienii motorului.'];
      })),
    },
    { eticheta: 'Operează', disponibila: (doc?.PoateOpera ?? false) && !modificat, ruleaza: () => void comanda(() => api.opereaza(id!)) },
    // `PoateAnula`/`PoateStorna` sunt ONESTE (F3-D2): țin cont de imperecheri —
    // un document stins nu se anulează până nu se șterge legătura din panou.
    { eticheta: 'Anulează operarea', disponibila: doc?.PoateAnula ?? false, ruleaza: () => void comanda(() => api.anuleaza(id!)) },
    {
      eticheta: 'Stornează',
      disponibila: doc?.PoateStorna ?? false,
      cereData: { eticheta: 'Data stornării' },
      ruleaza: (data) => { if (data) void comanda(() => api.storneaza(id!, data)); },
    },
    { eticheta: 'Șterge', disponibila: !nou && poateEdita, ruleaza: () => setDeSters(true) },
    { eticheta: 'Înapoi la listă', disponibila: true, ruleaza: () => navigheaza(ruta) },
  ];

  function schimbaAntet(v: TrzWrite) {
    // Legătura de pereche APARȚINE laturilor pentru care a fost aleasă: candidații
    // sunt exact „picioarele pe ACELEAȘI laturi" (F7-D1), iar motorul refuză la
    // operare o țintă cu alte laturi (F8-D8 punctul 4). Schimbarea unei laturi
    // stinge deci alegerea — pattern-ul pinului de lot al FCL (58e). Comparația se
    // face AICI, unde `agregat` e încă valoarea DINAINTEA schimbării.
    const laturiSchimbate = v.PredatorId !== agregat.PredatorId || v.PrimitorId !== agregat.PrimitorId;
    setAgregat(laturiSchimbate ? { ...v, LaturaPerecheId: null } : v);
    setModificat(true);
  }

  function salveazaLinie(linie: TrzLinieWrite) {
    const urmatoare = [...linii];
    if (indiceEditat == null) urmatoare.push(linie);
    else urmatoare[indiceEditat] = linie;
    setAgregat({ ...agregat, Linii: urmatoare });
    setModificat(true);
    setInEditare(null);
    setIndiceEditat(null);
  }

  function stergeLinie(indice: number) {
    setAgregat({ ...agregat, Linii: linii.filter((_, i) => i !== indice) });
    setModificat(true);
  }

  return (
    <DocumentShell
      citire={citit}
      titlu={nou ? titluNou : titluExistent(doc?.Numar ?? '')}
      sumar={<Sumar doc={doc} modificat={modificat || nou} esteVirament={esteVirament} />}
      comenzi={comenzi}
      confirmare={deSters && (
        <ConfirmareInline
          intrebare="Ștergeți definitiv acest draft, cu tot cu liniile lui?"
          verb="Șterge documentul"
          onConfirma={() => void stergeDocumentul()}
          onRenunta={() => setDeSters(false)}
        />
      )}
      inchideConfirmarea={() => setDeSters(false)}
      erori={erori}
      mesaje={mesaje}
      // Pe virament grupul conex ESTE latura pereche, iar despre ea vorbește
      // `PanouVirament` (inclusiv avertismentul de draft autogenerat) — două
      // panouri despre aceeași legătură ar fi doar zgomot.
      rezultatExtra={esteVirament ? undefined : <Provenienta doc={doc} />}
      ocupat={salvare.isPending || citit.isFetching}
      antet={
        <>
          {aratErori && <PanouErori erori={structurale} titlu="De completat înainte de salvare" />}
          <Formular
            tip={tip}
            schema={SCHEMA_ANTET}
            valoare={agregat}
            onSchimba={schimbaAntet}
            readOnly={!poateEdita}
            aratErori={aratErori}
          >
            <div className="grila-campuri">
              <CampData<TrzWrite> camp="Data" />
              {laturi}
              {/* Instrumentul e ENUM pe sârmă ca STRING (F3-D1): valorile vin din
                  metadata, nu dintr-o listă ținută în client. */}
              <CampSelectie<TrzWrite> camp="TipInstrument" enumerare="TipInstrumentPlata" />
              <CampText<TrzWrite> camp="NumarExtras" />
              <CampData<TrzWrite> camp="DataExtras" />

              {/* Latura pereche DECLARATĂ (F8-D12) — doar în modul virament și
                  doar cât documentul se poate scrie. Pe un document operat
                  câmpul dispare, iar starea reală o spune `PanouVirament` din
                  ReadDto (`Pereche` — link propriu SAU cine mă arată): un
                  SelectBox care nu conține valoarea salvată ar afișa gol pe o
                  legătură existentă.
                  „(niciuna)" NU e o opțiune de listă, ci absența valorii:
                  generarea automată rămâne comportamentul normal, iar butonul de
                  golire o readuce. */}
              {candidatiActivi && (
                <div>
                  <CampOptiuni<TrzWrite>
                    camp="LaturaPerecheId"
                    optiuni={optiuniPereche}
                    substitut="(niciuna) — se generează automat la operare"
                    textFaraDate={candidati.isFetching
                      ? 'Se caută…'
                      : 'Niciun picior compatibil — la operare se generează unul'}
                  />
                  <p className="indiciu">
                    Alegeți piciorul EXISTENT al aceluiași transfer, dacă l-ați cules deja: legat,
                    documentul nu mai generează nimic la operare. Lăsat gol, motorul creează singur
                    latura pereche.
                  </p>
                </div>
              )}
            </div>
          </Formular>
        </>
      }
      linii={
        <>
          <div className="linii__bara">
            <h3>Linii</h3>
            <button
              type="button"
              className="buton"
              disabled={!poateEdita || inEditare != null}
              onClick={() => { setIndiceEditat(null); setInEditare(linieGoala()); }}
            >
              Adaugă linie
            </button>
          </div>

          {/* Grilă READONLY peste liniile agregatului local (43c). Codurile de
              dimensiune sunt etichete server-owned: apar după salvare. */}
          <DataGrid
            dataSource={linii.map((l, i) => ({ ...l, __indice: i, ...etichete(doc?.Linii, l) }))}
            keyExpr="__indice"
            showBorders
            columnAutoWidth
            onRowClick={(e) => {
              if (!poateEdita) return;
              const rand = e.data as TrzLinieWrite & { __indice: number };
              setIndiceEditat(rand.__indice);
              setInEditare(linii[rand.__indice]);
            }}
          >
            <Column dataField="TipMaterialCod" caption="Cod tip" />
            <Column dataField="TipMaterialDenumire" caption={capLinie('TipMaterialId')} />
            <Column dataField="Valoare" caption={capLinie('Valoare')} dataType="number" format="#,##0.00" alignment="right" />
            <Column dataField="CodEconomicCod" caption={capLinie('CodEconomicId')} />
            <Column dataField="SursaFinantareCod" caption={capLinie('SursaFinantareId')} />
            <Column dataField="CodFunctionalCod" caption={capLinie('CodFunctionalId')} />
            <Column dataField="ProiectCod" caption={capLinie('ProiectId')} />
            <Column dataField="AngajamentCod" caption={capLinie('AngajamentId')} />
            <Column
              caption=""
              width={90}
              cellRender={(c) => (
                <button
                  type="button"
                  className="buton buton--mic"
                  disabled={!poateEdita}
                  onClick={(ev) => { ev.stopPropagation(); stergeLinie((c.data as { __indice: number }).__indice); }}
                >
                  Șterge
                </button>
              )}
            />
          </DataGrid>

          {inEditare && (
            // `key` = poziția editată: schimbarea liniei REMONTEAZĂ editorul (și
            // cu el precompletarea TRZ a liniei noi).
            <TrezorerieEditorLinie
              key={indiceEditat ?? 'linie-noua'}
              linie={inEditare}
              readOnly={!poateEdita}
              codTipImplicit={esteVirament ? COD_TIP_VIRAMENT : COD_TIP_TRZ}
              onSalveaza={salveazaLinie}
              onRenunta={() => { setInEditare(null); setIndiceEditat(null); }}
            />
          )}
        </>
      }
      // Sub linii stă ce ATÂRNĂ de document: stingerile pe plata/încasarea
      // obișnuită, indiciul perechii pe virament. Cele două se EXCLUD, fiindcă
      // viramentul nu stinge și nu e stins (F7-D5: `CapacitateStingere` e null
      // pe el, deci orice stingere oferită aici ar fi refuzată de server).
      subsol={esteVirament
        ? <PanouVirament doc={doc} />
        : doc && doc.Id && doc.Stare === 'Operat'
          ? (
            <PanouStingeri
              documentId={doc.Id}
              contrapartidaId={doc[campContrapartida]}
              // Trezoreria e STINGĂTORUL (31d): plata stinge factura.
              rol="stinge"
              // Trezoreria stinge creanțe/datorii, nu alte documente de
              // trezorerie (F3-D6a; INC↔PLT — avans↔regularizare — merge la
              // momentul 581, aditiv).
              tipuriCandidate={['FCT', 'FCL', 'DEC']}
              onSchimbare={() => void cache.invalidateQueries({ queryKey: [cheieCache] })}
            />
          )
          : undefined}
    />
  );
}

// Etichetele server-owned ale unei linii: se caută în ReadDto după `Id`. Linia
// nouă nu are încă niciuna — corect: nu le inventăm în TS.
function etichete(citite: TrzLinieRead[] | null | undefined, linie: TrzLinieWrite) {
  const g = citite?.find((c) => c.Id === linie.Id);
  return {
    TipMaterialCod: g?.TipMaterialCod ?? '',
    TipMaterialDenumire: g?.TipMaterialDenumire ?? '',
    CodEconomicCod: g?.CodEconomicCod ?? '',
    SursaFinantareCod: g?.SursaFinantareCod ?? '',
    CodFunctionalCod: g?.CodFunctionalCod ?? '',
    ProiectCod: g?.ProiectCod ?? '',
    AngajamentCod: g?.AngajamentCod ?? '',
  };
}

// De unde vine documentul și ce a mai născut el: plata autogenerată (31e) are
// factura-sursă, iar un document de trezorerie poate avea la rândul lui copii.
// Link, nu redirect: operatorul decide când trece pe celălalt ecran.
function Provenienta(props: { doc?: TrzRead }) {
  const doc = props.doc;
  const copii = doc?.Copii ?? [];
  if (!doc || (!doc.DocumentSursaId && copii.length === 0)) return null;
  // Ruta sursei prin `rutaTip` + TIPUL din ReadDto (D-6b): azi sursa e factura
  // (plata autogenerată 31e), dar la transferul 581 va fi PLT/INC — clientul nu
  // mai presupune `/fct/`. Tip fără felie de client = text, nu link mort.
  const rutaSursa = doc.DocumentSursaId ? rutaTip(doc.DocumentSursaTip, doc.DocumentSursaId) : null;
  const etichetaSursa = doc.DocumentSursaNumar || 'documentul sursă';
  return (
    <div className="panou panou--succes">
      <div className="panou__titlu">Grup conex</div>
      <ul className="panou__lista">
        {doc.DocumentSursaId && (
          <li>
            Generat din{' '}
            {rutaSursa ? <Link to={rutaSursa}>{etichetaSursa}</Link> : <span>{etichetaSursa}</span>}
            {doc.Autogenerat ? ' (autogenerat de motor)' : ''}
            {/* D-5b: draftul autogenerat e artefact al operării sursei — anularea
                ei îl ȘTERGE cu tot cu modificările manuale (26d/31e). */}
            {doc.Autogenerat && doc.Stare === 'Draft' && (
              <div className="indiciu">
                Draft autogenerat: dacă anulați operarea documentului-sursă, draftul se șterge
                — inclusiv modificările făcute aici. Re-operarea sursei îl regenerează curat.
              </div>
            )}
          </li>
        )}
        {copii.map((c: DocumentCopil) => (
          <li key={c.Id}>
            <Legatura copil={c} /> — {labelEnum('StareDocument', c.Stare)}{c.Autogenerat ? ', autogenerat' : ''}
          </li>
        ))}
      </ul>
    </div>
  );
}

// Ce ține locul panoului de stingeri pe un VIRAMENT INTERN (F7-D8): explicația
// mecanismului + drumul spre celălalt picior.
//
// Sursa e `Pereche` din ReadDto (F8-D11/F8-D12) — starea REZOLVATĂ de server
// („linkul meu SAU cine mă arată", `DocumentTrezorerie.PerecheId`). Înlocuiește
// deducerea din `Copii[]`/`DocumentSursa` a feliei 7, care vedea DOAR perechea
// generată de motor: latura DECLARATĂ manual (F8-D6) n-are `DocumentSursaId` și
// nu apare printre copii, deci vechea formă ar fi spus „latura pereche lipsește"
// exact pe cazul pentru care felia 8 există. O singură definiție, a serverului.
//
// Rutarea trece prin `rutaTip` (61a) — vocabular închis, fără `/plt/` hardcodat.
function PanouVirament(props: { doc?: TrzRead }) {
  const doc = props.doc;
  const pereche = doc?.Pereche;
  const rutaPereche = pereche?.Id ? rutaTip(pereche.Tip, pereche.Id) : null;
  const etichetaPereche = `${pereche?.Tip ?? ''} ${pereche?.Numar ?? ''}`.trim() || 'celălalt picior';
  // „Perechea ȚINE?" e o întrebare de DOMENIU (o pereche STORNATĂ are registrele
  // inversate, deci 581 e din nou deschis) — de asta vine gata calculată de la
  // server (`PerecheActiva`), nu se deduce aici din `Pereche.Stare`.
  const perecheActiva = doc?.PerecheActiva ?? false;
  // Cine ține legătura: linkul e al documentului care-l DECLARĂ (F8-D6). Al
  // treilea caz — perechea AUTOGENERATĂ căreia i s-a golit linkul: atunci nu
  // arată nimeni spre nimeni, iar relația e cea de grup conex.
  const provenienta = doc?.LaturaPerecheId
    ? ' (declarată de acest document)'
    : doc?.Copii?.some((c) => c.Id === pereche?.Id)
      ? ' (generată la operarea acestui document)'
      : doc?.DocumentSursaId && doc.DocumentSursaId === pereche?.Id
        ? ' (acest document s-a generat la operarea celuilalt)'
        : ' (celălalt document arată spre acesta)';
  return (
    <div className="panou">
      <div className="panou__titlu">Virament intern</div>
      <p className="indiciu">
        Virament intern — la operare se generează automat latura pereche, dacă nu ați declarat-o
        deja. Documentul nu stinge și nu poate fi stins: contul de tranzit (581) se închide singur
        când ambele picioare sunt operate.
      </p>
      {pereche && (
        <ul className="panou__lista">
          <li>
            Latura pereche:{' '}
            {rutaPereche ? <Link to={rutaPereche}>{etichetaPereche}</Link> : <span>{etichetaPereche}</span>}
            {' '}— {labelEnum('StareDocument', pereche.Stare)}
            {/* Care capăt ține legătura: e util la ștergere (linkul e al
                documentului care-l DECLARĂ — F8-D6, o singură parte scrisă). */}
            {provenienta}
          </li>
        </ul>
      )}
      {/* Latura pereche se poate ȘTERGE cât e Draft (ștergerea nu se refuză —
          operatorul poate să fi cules deja piciorul celălalt manual, iar calea
          manuală reproduce exact același document). Ce NU se acceptă e tăcerea:
          un virament operat fără pereche ACTIVĂ lasă 581 deschis, deci starea se
          spune, nu se ghicește din absența unei linii în listă.

          Ramificarea e pe `PerecheActiva`, nu pe existența liniei: o pereche
          STORNATĂ se vede în listă (o poți deschide), dar nu ține — registrele ei
          sunt inversate. Sfatul diferă și el: acolo unde perechea lipsește cu
          totul, culegerea manuală a piciorului celălalt e o cale validă; unde a
          fost stornată, re-operarea o REGENEREAZĂ (gardianul de anulare nu se
          uită la pointerii stornați), deci ăla e drumul scurt. */}
      {doc?.Stare === 'Operat' && !perecheActiva && (
        <div className="indiciu">
          {pereche
            ? `Latura pereche (${etichetaPereche}) a fost STORNATĂ: registrele ei sunt inversate,
               deci contul de tranzit 581 e din nou deschis. `
            : `Latura pereche lipsește (a fost ștearsă sau nu s-a generat): contul de tranzit 581
               rămâne deschis. `}
          {/* Remediul depinde de PROVENIENȚA acestui picior: generarea e a
              documentului „mamă" (gardul `Autogenerat`), deci pe unul generat
              automat sfatul „re-operați" n-ar face nimic. */}
          {doc.Autogenerat
            ? `Piciorul acesta a fost generat automat, deci re-operarea LUI nu regenerează nimic:
               reluați de pe celălalt document (anulare + re-operare) sau culegeți manual piciorul
               care lipsește.`
            : `Anulați operarea și re-operați documentul — perechea se regenerează — sau culegeți
               manual piciorul celălalt și legați-l.`}
        </div>
      )}
      {/* Același avertisment ca pe orice draft autogenerat (D-5b): artefact al
          operării sursei, deci anularea ei îl șterge cu tot cu modificări. */}
      {doc?.Autogenerat && doc.Stare === 'Draft' && (
        <div className="indiciu">
          Draft autogenerat: dacă anulați operarea celuilalt picior, draftul se șterge —
          inclusiv modificările făcute aici. Re-operarea lui îl regenerează curat.
        </div>
      )}
    </div>
  );
}

// Eticheta unui candidat de latură pereche: NUMĂR + dată + total + stare —
// exact cele patru lucruri care disting două viramente altfel identice între
// aceleași conturi (ambiguitatea 64k pe care o tranșează operatorul).
//
// `PerecheDraftNumar` completat = ținta are deja un draft care o arată, iar
// legarea va fi REFUZATĂ la operare (F8-D8 punctul 5) cât timp el există;
// remediul îl spune serverul, clientul doar îl arată.
//
// Excepția `esteAles`: după salvare, draftul care blochează candidatul ALES e
// chiar documentul curent (endpoint-ul aplică `exclusId` doar pe candidați, nu
// și pe căutarea drafturilor blocante) — un „blocat de …" pe propria alegere ar
// fi o minciună. Restul candidaților îl păstrează.
// Eticheta candidatului: date ale serverului, puse cap la cap. Indiciul
// „blocat de draftul X" vine tot de la server (`PerecheDraftNumar`) — inclusiv
// absența lui pe candidatul ALES de documentul curent, care își e propriul
// pointer Draft și care se exclude în proiecție (`exclusId`), nu aici.
function etichetaCandidat(c: CandidatPereche): string {
  const numar = c.Numar?.trim() || '(fără număr)';
  const data = ziLocala(c.Data) ?? '';
  const total = (c.Total ?? 0).toFixed(2);
  const stare = labelEnum('StareDocument', c.Stare);
  const baza = `${numar} · ${data} · ${total} · ${stare}`;
  return c.PerecheDraftNumar
    ? `${baza} — blocat de draftul ${c.PerecheDraftNumar} (ștergeți-l întâi)`
    : baza;
}

function Legatura(props: { copil: DocumentCopil }) {
  const { Id, Tip, Numar } = props.copil;
  const ruta = Id ? rutaTip(Tip, Id) : null;
  const eticheta = `${Tip ?? ''} ${Numar ?? ''}`.trim();
  return ruta ? <Link to={ruta}>{eticheta}</Link> : <span>{eticheta}</span>;
}

// `Total` = brutul redus de SERVER; `Rest` vine tot din ReadDto (F3-D2) — TS nu
// scade nimic. La editare nesalvată nu există: baza s-a schimbat.
//
// „Rest de stins" NU se arată pe virament: un picior de virament are `Ramas`
// egal cu totalul pe veci (nu se stinge niciodată — F7-D5/F7-D7), deci numărul
// ar fi corect aritmetic și mincinos ca înțeles. Aceeași regulă ca ascunderea
// panoului de stingeri, la același capitol: pe virament stingerea nu există.
function Sumar(props: { doc?: TrzRead; modificat: boolean; esteVirament: boolean }) {
  const { doc, modificat, esteVirament } = props;
  return (
    <div className="sumar">
      <span className="sumar__stare">{labelEnum('StareDocument', doc?.Stare) || 'nesalvat'}</span>
      <span className="sumar__total">
        Total: {modificat || doc?.Total == null
          ? <em title="Totalul îl calculează serverul la salvare.">— recalculat la salvare</em>
          : doc.Total.toFixed(2)}
      </span>
      {!modificat && !esteVirament && doc?.Stare === 'Operat' && doc.Ramas != null && (
        <span>Rest de stins: <strong>{doc.Ramas.toFixed(2)}</strong></span>
      )}
      {esteVirament && <span className="sumar__stare">virament intern</span>}
      {doc?.Numar && <span>Număr: {doc.Numar}</span>}
    </div>
  );
}
