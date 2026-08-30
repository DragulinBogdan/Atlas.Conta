import { useState } from 'react';
import { NavLink, Navigate, Outlet, Route, Routes, useNavigate } from 'react-router';
import Drawer from 'devextreme-react/drawer';
import { esteAutentificat, stergeToken } from './nucleu/auth';
import { cache } from './nucleu/cache';
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
import { NtcLista } from './felii/ntc/NtcLista';
import { NtcDetaliu } from './felii/ntc/NtcDetaliu';
import { AsmLista } from './felii/asm/AsmLista';
import { AsmDetaliu } from './felii/asm/AsmDetaliu';
import { RlfLista } from './felii/rlf/RlfLista';
import { RlfDetaliu } from './felii/rlf/RlfDetaliu';
import { RdcLista } from './felii/rdc/RdcLista';
import { RdcDetaliu } from './felii/rdc/RdcDetaliu';
import { SoldStoc } from './felii/stoc/SoldStoc';
import { Balanta } from './felii/raportare/Balanta';
import { BalantaPlan } from './felii/raportare/BalantaPlan';
import { FisaCont } from './felii/raportare/FisaCont';
import { RegistruJurnal } from './felii/raportare/RegistruJurnal';
import { JurnalCumparari, JurnalVanzari } from './felii/tva/JurnalTva';
import { DecontTva } from './felii/tva/DecontTva';
import { D300 } from './felii/tva/D300';
import { D394 } from './felii/tva/D394';
import { Saft } from './felii/tva/Saft';
import { Parteneri } from './felii/nomenclatoare/Parteneri';
import { PartenerDetaliu } from './felii/nomenclatoare/PartenerDetaliu';
import { Produse } from './felii/nomenclatoare/Produse';
import { ProdusDetaliu } from './felii/nomenclatoare/ProdusDetaliu';
import { SocietateEcran } from './felii/nomenclatoare/SocietateEcran';
import { PoliticiMiscareSaft } from './felii/nomenclatoare/PoliticiMiscareSaft';

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
        {/* Nota contabilă (felia 19): postare EXPLICITĂ, fără regulă de contare
            — și calea de lucru a compensării (48b), cu panoul de stingeri
            grupat pe (contrapartidă × sens). */}
        <Route path="/ntc" element={<NtcLista />} />
        <Route path="/ntc/nou" element={<NtcDetaliu />} />
        <Route path="/ntc/:id" element={<NtcDetaliu />} />
        <Route path="/asm" element={<AsmLista />} />
        <Route path="/asm/nou" element={<AsmDetaliu />} />
        <Route path="/asm/:id" element={<AsmDetaliu />} />
        {/* Retururile (felia 19): storno cu valori NEGATIVE pe corespondența
            originală, culese POZITIV. Nu sunt stingători și nu au panou de
            stingeri (F19-D11) — compensarea cu factura originală se face prin
            nota contabilă. */}
        <Route path="/rlf" element={<RlfLista />} />
        <Route path="/rlf/nou" element={<RlfDetaliu />} />
        <Route path="/rlf/:id" element={<RlfDetaliu />} />
        <Route path="/rdc" element={<RdcLista />} />
        <Route path="/rdc/nou" element={<RdcDetaliu />} />
        <Route path="/rdc/:id" element={<RdcDetaliu />} />
        <Route path="/stoc" element={<SoldStoc />} />
        {/* Raportarea pe registre (felia 9). Parametrii (perioadă, mod, cont)
            trăiesc în query string, nu în cale: sunt STARE, nu identitate — un
            raport e util fiindcă e partajabil ca link (43c). Fișa n-are intrare
            proprie de meniu: se ajunge la ea din balanță, cu perioada păstrată,
            dar are selector de cont ca să se poată schimba contul pe loc. */}
        <Route path="/balanta" element={<Balanta />} />
        <Route path="/balanta-plan" element={<BalantaPlan />} />
        <Route path="/fisa-cont" element={<FisaCont />} />
        <Route path="/jurnal" element={<RegistruJurnal />} />
        {/* Jurnalele de TVA (felia 11): aceeași proiecție pe laturi diferite,
            deci rute proprii — nu un ecran cu comutator. Sunt două rapoarte
            distincte, iar „jurnalul de cumpărări pe februarie" trebuie să fie un
            link. `/decont-tva` rămâne SCHELETUL — cifrele perioadei per tip de
            TVA, cu codurile SAF-T; formularul propriu-zis, cu rândurile și
            totalurile lui, e `/d300` (felia 12). Nici unul nu produce declarația
            (35c): fișierul XML e altă unealtă. */}
        <Route path="/jurnal-cumparari" element={<JurnalCumparari />} />
        <Route path="/jurnal-vanzari" element={<JurnalVanzari />} />
        <Route path="/decont-tva" element={<DecontTva />} />
        <Route path="/d300" element={<D300 />} />
        <Route path="/d394" element={<D394 />} />
        {/* SAF-T (felia 16): al treilea formular peste registre — și primul care
            produce un FIȘIER. Perioada e o LUNĂ (`an`+`luna`), nu un interval:
            `SelectionCriteria` din D406 numără luni. */}
        <Route path="/saft" element={<Saft />} />
        {/* Nomenclatoarele (felia 20, F20-D8): primele ecrane care scriu prin
            OData, nu prin REST — o entitate plată n-are agregat, deci n-are ce
            reconcilia un controller de felie. `Societate` n-are listă și n-are
            `/nou`: e un singur rând prin definiție (gardianul refuză al doilea).
            Politica de mișcare SAF-T e DOAR de citit (56). */}
        <Route path="/parteneri" element={<Parteneri />} />
        <Route path="/parteneri/nou" element={<PartenerDetaliu />} />
        <Route path="/parteneri/:id" element={<PartenerDetaliu />} />
        <Route path="/produse" element={<Produse />} />
        <Route path="/produse/nou" element={<ProdusDetaliu />} />
        <Route path="/produse/:id" element={<ProdusDetaliu />} />
        <Route path="/societate" element={<SocietateEcran />} />
        <Route path="/politici/miscare-saft" element={<PoliticiMiscareSaft />} />
        <Route path="*" element={<Navigate to="/fct" replace />} />
      </Route>
    </Routes>
  );
}

// Meniul, în panoul Drawer-ului: grupuri VERTICALE în locul barei care se rupea
// pe rânduri (smoke F20 D1 a prins grupurile ieșind din viewport la 1416 px, iar
// lista mai crește — ITV, politici). `NavLink`-urile rămân ale react-router:
// template-urile devextreme-react se randează în același arbore React, deci
// contextul router-ului și clasa `active` trec neatinse.
function Meniu() {
  return (
    <nav className="meniu">
      <span className="meniu__grup">Documente</span>
      <NavLink to="/fct">Facturi intrare</NavLink>
      <NavLink to="/nir">NIR-uri</NavLink>
      <NavLink to="/fcl">Facturi ieșire</NavLink>
      <NavLink to="/dsc">Descărcări</NavLink>
      <NavLink to="/btr">Note de transfer</NavLink>
      <NavLink to="/bcs">Bonuri de consum</NavLink>
      <NavLink to="/ldi">Diferențe inventar</NavLink>
      <NavLink to="/asm">Asamblări</NavLink>
      <NavLink to="/ntc">Note contabile</NavLink>
      <NavLink to="/rlf">Retururi la furnizor</NavLink>
      <NavLink to="/rdc">Retururi de la client</NavLink>
      <span className="meniu__grup">Trezorerie</span>
      <NavLink to="/plt">Plăți</NavLink>
      <NavLink to="/inc">Încasări</NavLink>
      <NavLink to="/dec">Deconturi</NavLink>
      <span className="meniu__grup">Rapoarte</span>
      <NavLink to="/stoc">Sold stoc</NavLink>
      <NavLink to="/balanta">Balanță</NavLink>
      <NavLink to="/balanta-plan">Balanță pe plan</NavLink>
      <NavLink to="/jurnal">Registru-jurnal</NavLink>
      <span className="meniu__grup">TVA și declarații</span>
      <NavLink to="/jurnal-cumparari">Jurnal cumpărări</NavLink>
      <NavLink to="/jurnal-vanzari">Jurnal vânzări</NavLink>
      <NavLink to="/decont-tva">Decont TVA</NavLink>
      <NavLink to="/d300">D300</NavLink>
      <NavLink to="/d394">D394</NavLink>
      <NavLink to="/saft">SAF-T</NavLink>
      <span className="meniu__grup">Nomenclatoare</span>
      <NavLink to="/parteneri">Parteneri</NavLink>
      <NavLink to="/produse">Produse</NavLink>
      <NavLink to="/societate">Societate</NavLink>
      <span className="meniu__grup">Politici</span>
      <NavLink to="/politici/miscare-saft">Mișcări SAF-T</NavLink>
    </nav>
  );
}

const randeazaMeniu = () => <Meniu />;

function Cadru() {
  const navigheaza = useNavigate();
  // Starea deschis/închis e EFEMERIDĂ locală (43c): nu merită un link — două
  // ferestre pe același raport pot avea legitim meniul în stări diferite.
  const [meniuDeschis, setMeniuDeschis] = useState(true);
  if (!esteAutentificat())
    return <Navigate to="/login" replace />;

  return (
    <div className="cadru">
      <header className="cadru__bara">
        <button
          type="button"
          className="buton buton--mic"
          aria-label="Meniu"
          onClick={() => setMeniuDeschis((d) => !d)}
        >
          ☰
        </button>
        <span className="cadru__marca">Atlas Conta</span>
        <button
          type="button"
          className="buton buton--mic cadru__iesire"
          // Ieșirea e navigare SPA, deci contextul JS supraviețuiește: fără `clear()`
          // cache-ul (`staleTime: Infinity` pe nomenclatoare, F20-D2) ar servi
          // următorului utilizator din același tab datele celui dinainte (review F2).
          onClick={() => { stergeToken(); cache.clear(); navigheaza('/login', { replace: true }); }}
        >
          Ieșire
        </button>
      </header>
      <div className="cadru__corp">
        <Drawer
          opened={meniuDeschis}
          openedStateMode="shrink"
          revealMode="slide"
          position="left"
          height="100%"
          render={randeazaMeniu}
        >
          <main className="cadru__continut"><Outlet /></main>
        </Drawer>
      </div>
    </div>
  );
}
