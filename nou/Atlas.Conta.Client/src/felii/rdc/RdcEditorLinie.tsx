import { useState } from 'react';
import { Formular, eroriStructurale } from '../../nucleu/formular';
import { CampNumar } from '../../nucleu/campuri';
import { Lookup } from '../../nucleu/Lookup';
import { PanouErori } from '../../nucleu/PanouErori';
import { campMeta } from '../../nucleu/campMeta';
import { etichetaLot } from '../../nucleu/lot';
import { precompleteazaTip } from '../../nucleu/etichete';
import { SCHEMA_LINIE, TIP_LINIE, type RdcLinieWrite } from './api';

// Liniile de draft se editează cu VOCABULARUL `Camp*`, nu în grilă (43c): grila
// doar afișează. Editorul nu vorbește cu serverul — CRUD-ul per linie a fost
// respins în designul API (agregatul se trimite întreg).
//
// ═══ Rolul liniei: prezență pe sârmă, comutator pe ecran ═══
// În model rolul E prezența lui `LotId` (F19-D12) — venitul stornat n-are lot,
// marfa care revine îl are pe cel ORIGINAL. Pentru operator asta nu înseamnă
// nimic, așa că AICI, la graniță, prezența se traduce în două cuvinte („Venit" /
// „Marfă returnată") și se traduce înapoi la salvare. Modelul rămâne neatins:
// nu există enum de rol nici pe sârmă, nici în DTO.
//
// ═══ Și capcana pe care comutatorul NU are voie s-o întindă ═══
// Serverul REFUZĂ schimbarea de rol pe o linie EXISTENTĂ, în ambele sensuri
// (riscul 5 al contractului, închis în `ReturClientApply`): o conversie ar trebui
// să fie completă — TVA, natura Tipului, valoarea, cantitatea pro-formă — iar pe
// jumătate ar lăsa în document o linie care nu e niciunul din cele două lucruri.
// Deci comutatorul e ACTIV DOAR PE LINII NOI. Pe o linie salvată rolul se
// afișează ca FAPT, cu calea de lucru scrisă lângă el: se șterge linia și se
// culege din nou pe rolul dorit — operație pe care agregatul o exprimă deja.
// Un comutator activ care duce garantat la 422 ar fi o capcană, nu o afordanță.
//
// Ce NU face editorul: nu calculează nicio valoare (costul mărfii returnate e al
// LOTULUI, îl scrie serverul), nu verifică natura Tipului (venit vs stoc — refuz
// al motorului, cu mesajul lui) și nu verifică soldul.

export type Rol = 'venit' | 'marfa';

// Traducerea PREZENȚEI în rol. Trăiește aici, lângă traducerea inversă din
// `spreLinie`, ca să existe o singură definiție a corespondenței — grila
// documentului o refolosește, nu o rescrie.
export function rolLiniei(linie: { LotId?: string | null }): Rol {
  return linie.LotId == null ? 'venit' : 'marfa';
}

// Rolul cu care se DESCHIDE editorul. Diferă de `rolLiniei` într-un singur
// punct, dar unul care contează: o linie BLANK („Adaugă linie") arată pe sârmă
// exact ca o linie de venit — n-are lot —, iar dacă am deschide-o pe „venit"
// rolul ar fi ales în locul operatorului. Deci: lot ⇒ marfă; fără lot dar cu
// Tipul cules (linie salvată, sau linie nouă reintrată în editare — Tipul e
// obligatoriu pe ambele roluri, deci există pe orice linie ajunsă în agregat)
// ⇒ venit; nimic ⇒ NULL, îl alege operatorul.
function rolInitial(linie: RdcLinieWrite): Rol | null {
  const blank = linie.Id == null && linie.LotId == null && linie.TipMaterialId == null;
  return blank ? null : rolLiniei(linie);
}

export const ETICHETA_ROL: Record<Rol, string> = {
  venit: 'Venit (venitul stornat)',
  marfa: 'Marfă returnată (revine pe lot)',
};

// Câmpurile verificate structural, per ROL: fiecare rol are alt set, iar schema
// le dă pe toate nullable (o linie e ori una, ori alta — niciunul dintre câmpuri
// nu poate fi obligatoriu structural pe DTO).
const CAMPURI_COMUNE: (keyof RdcLinieWrite & string)[] = ['TipMaterialId'];

// Etichetele CULESE la selecție, pentru grila documentului (mecanismul 61b):
// liniile nesalvate n-au încă ReadDto, dar răspunsul OData al selecției e DEJA
// în client — nu se inventează nimic în TS, se reține ce a afișat lookup-ul.
export type EticheteCulese = {
  TipMaterialCod?: string;
  TipMaterialDenumire?: string;
  LotEticheta?: string;
  TipTvaCod?: string;
};

const text = (v: unknown) => (v == null ? '' : String(v));

export function RdcEditorLinie(props: {
  linie: RdcLinieWrite;
  // Ce a calculat SERVERUL pentru linia asta (ReadDto) — doar pentru afișare.
  valoareTvaCitita?: number | null;
  readOnly: boolean;
  onSalveaza: (l: RdcLinieWrite, etichete: EticheteCulese) => void;
  onRenunta: () => void;
}) {
  const { readOnly, onSalveaza, onRenunta } = props;
  // `existenta` = linia e SALVATĂ pe server (are `Id`) — acolo rolul e imuabil,
  // fiindcă Apply refuză schimbarea lui. O linie adăugată local în agregat, dar
  // încă netrimisă, rămâne comutabilă: agregatul o creează de la zero la primul
  // PUT, oricare i-ar fi rolul. Rolul de pornire îl dă `rolInitial` (blank ⇒
  // niciunul: o linie necerută n-are voie să treacă drept ceva — același
  // principiu ca direcția fără membru 0 a ASM).
  const existenta = props.linie.Id != null;
  const [rol, setRol] = useState<Rol | null>(() => rolInitial(props.linie));
  const [linie, setLinie] = useState<RdcLinieWrite>(() => ({
    ...props.linie,
    ValoareTva: props.linie.ValoareTva ?? props.valoareTvaCitita ?? undefined,
  }));
  const [etichete, setEtichete] = useState<EticheteCulese>({});
  // Override-ul de TVA nu e un flag global de formular: e starea UNUI câmp, iar
  // singura sursă de adevăr e „valoarea lui s-a schimbat de când s-a deschis
  // editorul" (același mecanism ca la FCT/FCL/RLF).
  const [tvaAtins, setTvaAtins] = useState(props.linie.ValoareTva != null);
  const [aratErori, setAratErori] = useState(false);

  const venit = rol === 'venit';
  const marfa = rol === 'marfa';

  const structurale = [
    ...eroriStructurale(TIP_LINIE, SCHEMA_LINIE, linie as Record<string, unknown>, CAMPURI_COMUNE),
    ...cerut('Felul liniei', rol != null),
    // Pe rolul de marfă lotul e OBLIGATORIU — și e chiar rolul: golirea lui n-ar
    // fi „un lot lipsă", ar fi o schimbare de rol, pe care serverul o refuză.
    ...(marfa ? cerutCamp('LotId', linie.LotId != null) : []),
  ];

  function schimba(v: RdcLinieWrite) {
    if (v.ValoareTva !== linie.ValoareTva)
      setTvaAtins(true);
    setLinie(v);
  }

  // Comutarea rolului (doar pe linii NOI) GOLEȘTE câmpurile celuilalt rol —
  // oglinda golirii persistate din `ReturClientApply`, unde linia de cost își
  // pierde identitatea fiscală, iar linia de venit n-are lot. Câmpurile rolului
  // inactiv nici nu se randează: un câmp read-only care arată o valoare pe care
  // serverul o aruncă ar fi tot o minciună.
  function comutaRolul(nou: Rol) {
    if (nou === rol) return;
    setRol(nou);
    setTvaAtins(false);
    if (nou === 'venit') {
      setEtichete((prev) => ({ ...prev, LotEticheta: '' }));
      setLinie((prev) => ({ ...prev, LotId: null, Cantitate: 0 }));
    }
    else {
      setEtichete((prev) => ({ ...prev, TipTvaCod: '' }));
      setLinie((prev) => ({ ...prev, Valoare: 0, TipTvaId: null, ValoareTva: undefined }));
    }
  }

  // Traducerea INVERSĂ, la graniță: rolul ales devine forma pe care o înțelege
  // modelul. Nimic nu se strecoară de pe celălalt rol.
  function spreLinie(): RdcLinieWrite {
    if (venit) {
      return {
        ...linie,
        LotId: null,
        // Cantitatea liniei de venit e pro-formă (0 devine 1 la operare, 32d) —
        // nu se culege, deci nu se inventează aici: se duce cum a venit.
        Valoare: linie.Valoare ?? 0,
        ValoareTva: tvaAtins ? linie.ValoareTva ?? null : null,
      };
    }
    return {
      ...linie,
      // Costul e al LOTULUI: `Valoare` culeasă n-are ce căuta pe rolul ăsta
      // (serverul o recalculează oricum din prețul lotului).
      Valoare: 0,
      // FĂRĂ identitate fiscală (F19-D7): serverul persistă `TipTvaId = null` și
      // `ValoareTva = 0`. Trimitem exact asta, ca payload-ul să spună același
      // lucru ca modelul.
      TipTvaId: null,
      ValoareTva: null,
    };
  }

  function confirma() {
    setAratErori(true);
    if (structurale.length > 0)
      return;
    onSalveaza(spreLinie(), etichete);
  }

  return (
    <div className="editor-linie">
      <Formular
        tip={TIP_LINIE}
        schema={SCHEMA_LINIE}
        valoare={linie}
        onSchimba={schimba}
        readOnly={readOnly}
        aratErori={aratErori}
      >
        <div className="grila-campuri">
          <div className="camp">
            <label className="camp__eticheta">
              Felul liniei
              {!existenta && <span className="camp__obligatoriu" title="obligatoriu"> *</span>}
            </label>
            {existenta
              ? (
                  <div className="camp__control">
                    <div className="valoare-statica">{rol ? ETICHETA_ROL[rol] : '—'}</div>
                  </div>
                )
              : (
                  <div className="camp__control camp__comutator">
                    {(['venit', 'marfa'] as Rol[]).map((r) => (
                      <button
                        key={r}
                        type="button"
                        className={rol === r ? 'buton buton--primar buton--mic' : 'buton buton--mic'}
                        disabled={readOnly}
                        onClick={() => comutaRolul(r)}
                      >
                        {ETICHETA_ROL[r]}
                      </button>
                    ))}
                  </div>
                )}
            <div className="camp__eroare" />
            <p className="indiciu">
              {existenta
                ? 'Felul unei linii salvate nu se mai schimbă — venitul poartă valoarea și TVA-ul, marfa '
                  + 'poartă lotul și cantitatea. Ca să treceți linia pe celălalt fel: ștergeți-o din '
                  + 'document și adăugați una nouă pe felul dorit.'
                : 'Venit = venitul stornat de pe factura originală (fără lot, cu TVA). Marfă returnată = '
                  + 'marfa care revine pe lotul ORIGINAL (cu lot și cantitate, fără TVA). Se alege o '
                  + 'singură dată: pe o linie salvată felul nu se mai schimbă.'}
            </p>
          </div>

          {rol != null && (
            <Lookup<RdcLinieWrite>
              camp="TipMaterialId"
              entitate="TipMaterial"
              mod="local"
              eticheta={venit ? 'Tip de venit (cont/clasă)' : 'Tip de stoc (cont/clasă)'}
              afisare={codSiDenumire}
              laSelectie={(t) => setEtichete((prev) => ({
                ...prev, TipMaterialCod: text(t?.Cod), TipMaterialDenumire: text(t?.Denumire),
              }))}
            />
          )}

          {venit && (
            <>
              <div>
                {/* Singura valoare CULEASĂ din tot documentul: venitul stornat,
                    adică prețul de vânzare de pe factura originală. POZITIV —
                    semnul minus îl pune operarea. */}
                <CampNumar<RdcLinieWrite> camp="Valoare" zecimale={2} />
                <p className="indiciu">
                  Venitul stornat — prețul de vânzare de pe factura originală, pozitiv.
                </p>
              </div>
              <Lookup<RdcLinieWrite>
                camp="TipTvaId"
                entitate="TipTva"
                mod="local"
                afisare={etichetaTipTva}
                laSelectie={(t) => setEtichete((prev) => ({ ...prev, TipTvaCod: text(t?.Cod) }))}
              />
              <div>
                <CampNumar<RdcLinieWrite> camp="ValoareTva" zecimale={2} />
                <p className="indiciu">
                  {tvaAtins
                    ? 'Suprascris manual — se trimite ca atare.'
                    : 'Calculat de server din cotă; modificați doar dacă factura originală diferă.'}
                </p>
              </div>
            </>
          )}

          {marfa && (
            <>
              <div>
                {/* Lotul ORIGINAL al livrării, nefiltrat (F6-D8, precedentul
                    BTR/BCS/LDI): locația curentă a unui lot e soldul din
                    registru, nu nașterea lui. Din el iese costul cu care revine
                    marfa — de aceea rolul ăsta n-are editor de valoare. */}
                <Lookup<RdcLinieWrite>
                  camp="LotId"
                  entitate="Lot"
                  mod="remote"
                  obligatoriu
                  expand={['Produs']}
                  afisare={etichetaLot}
                  cauta="Produs.Denumire"
                  sortare="Data"
                  laSelectie={(l) => {
                    // Tipul se precompletează din PRODUSUL lotului
                    // (`$expand=Produs` aduce `Produs.TipMaterialId` — un Guid,
                    // fără `$expand` imbricat), doar când e gol. Id-ul și
                    // eticheta lui se scriu într-o SINGURĂ decizie (F20-D3).
                    const p = l?.Produs as Record<string, unknown> | null | undefined;
                    setEtichete((prev) => ({ ...prev, LotEticheta: l ? etichetaLot(l) : '' }));
                    precompleteazaTip({
                      linie,
                      tipId: p?.TipMaterialId == null ? undefined : String(p.TipMaterialId),
                      setLinie,
                      setEtichete,
                    });
                  }}
                />
                <p className="indiciu">
                  Lotul ORIGINAL al livrării — marfa revine pe el, la prețul lui. Costul îl scrie serverul.
                </p>
              </div>
              <div>
                <CampNumar<RdcLinieWrite> camp="Cantitate" />
                <p className="indiciu">
                  Cantitatea returnată, POZITIVĂ. Semnul minus îl pune operarea.
                </p>
              </div>
            </>
          )}
        </div>
      </Formular>

      {aratErori && <PanouErori erori={structurale} titlu="Completați linia" />}

      <div className="editor-linie__comenzi">
        <button type="button" className="buton buton--primar" disabled={readOnly} onClick={confirma}>
          {props.linie.Id ? 'Actualizează linia' : 'Adaugă linia'}
        </button>
        <button type="button" className="buton" onClick={onRenunta}>Renunță</button>
      </div>
    </div>
  );
}

// Obligativitatea CONDIȚIONATĂ de rol, cu ȘABLONUL de mesaj al nucleului.
// `cerutCamp` ia caption-ul din metadata; `cerut` primește numele scris în
// felie, pentru ce nu e câmp de DTO (felul liniei).
function cerut(nume: string, indeplinit: boolean): string[] {
  return indeplinit ? [] : [`„${nume}” este obligatoriu.`];
}

function cerutCamp(membru: string, indeplinit: boolean): string[] {
  return cerut(campMeta(TIP_LINIE, membru, SCHEMA_LINIE).caption, indeplinit);
}

// Nomenclatoarele au `DefaultProperty = Denumire`, dar operatorul caută pe cod —
// afișăm amândouă. Fără `Cod`, două tipuri omonime sunt indistinctibile.
function codSiDenumire(element: Record<string, unknown>): string {
  if (!element) return '';
  const cod = element.Cod == null ? '' : String(element.Cod);
  const denumire = element.Denumire == null ? '' : String(element.Denumire);
  return cod && denumire ? `${cod} — ${denumire}` : cod || denumire;
}

function etichetaTipTva(element: Record<string, unknown>): string {
  if (!element) return '';
  const cota = element.Cota == null ? null : Number(element.Cota);
  return `${codSiDenumire(element)}${cota == null ? '' : ` (${cota}%)`}`;
}
