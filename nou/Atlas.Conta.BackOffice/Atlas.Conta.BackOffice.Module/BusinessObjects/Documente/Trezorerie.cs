using Atlas.Conta.BackOffice.Module.UI;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Decizia 17 + 09/31: plățile/încasările sunt tipuri de document; BREGISTRU
// dispare — registrul de casă/bancă e registrul contabil al acestor documente.
// Laturile: ContPropriu ↔ Partener/Angajat; liniile sunt DEFALCAREA sumei
// (echivalentul BREG_P): Valoare culeasă direct + Dimensiuni, fără stoc/lot.
// Tipul liniei: tehnic „TRZ" la culegere manuală; liniile autogenerate din
// factură păstrează Tipul liniei sursă (poartă informația de defalcare).
public abstract class DocumentTrezorerie : Document {
    [XafDisplayName("Instrument de plată")]
    public virtual TipInstrumentPlata TipInstrument { get; set; }
    [XafDisplayName("Număr extras")]
    public virtual string NumarExtras { get; set; }
    [XafDisplayName("Dată extras")]
    public virtual DateOnly? DataExtras { get; set; }

    // Legătura EXPLICITĂ de pereche a viramentului (F8-D6; gaura 64k): „acest
    // picior e perechea documentului X". Se scrie o SINGURĂ parte, deliberat —
    // pe COPIL la generare (motorul) sau pe latura culeasă MANUAL (operatorul,
    // arătând spre piciorul existent). Cealaltă parte e Operată, iar gardianul
    // de Committing (55a) refuză orice scriere pe documentele ne-Draft: o
    // legătură bidirecțională ar cere o ușă non-secured pentru un simplu link.
    public virtual Guid? LaturaPerecheId { get; set; }
    [XafDisplayName("Latura pereche")]
    public virtual DocumentTrezorerie LaturaPereche { get; set; }

    // Citirea e DERIVATĂ și SIMETRICĂ: „am pereche?" e adevărat și când eu arăt
    // spre altcineva, și când altcineva mă arată pe mine. Are însă DOUĂ roluri,
    // separate deliberat (fix review D1) — o singură metodă răspundea la două
    // întrebări diferite și mințea la una:
    //
    //   • `PerecheId` — DESCRIPTIVĂ, „ce ARĂT operatorului": orice pereche,
    //     inclusiv una STORNATĂ (a existat, o vede pe ecran, o poate deschide).
    //     Consumator: `Pereche` din ReadDto (F8-D11).
    //   • `PerecheActivaId` — DECIZIONALĂ, „perechea care ȚINE": capătul
    //     `Stornat` NU contează NICIODATĂ — registrele lui sunt inversate, deci
    //     581 e redeschis și perechea nu s-a produs. Consumatori: suprimarea
    //     generării (F8-D7) și filtrele de candidați (F8-D10/F8-D11).
    //
    // Noțiunea, o singură dată: o legătură contează dacă documentul de la
    // celălalt capăt e `Draft` (intenție în curs — suprimă un al doilea draft)
    // sau `Operat` (piciorul real). `Stornat` nu contează.
    //
    // Gardianul de anulare/storno (`MotorOperare.VerificaFaraLaturaPerecheOperata`)
    // rămâne pe `== Operat` și NU e o inconsecvență: el apără REGISTRE (doar un
    // pointer cu registre proprii poate dubla postarea la re-operarea țintei),
    // pe când `PerecheActivaId` apără DRAFTURI (un draft în lucru e deja
    // intenția celui de-al doilea picior, deci nu se mai generează unul).
    public Guid? PerecheId(DevExpress.ExpressApp.IObjectSpace os) =>
        CautaPereche(os, doarActiva: false);
    public Guid? PerecheActivaId(DevExpress.ExpressApp.IObjectSpace os) =>
        CautaPereche(os, doarActiva: true);

    // Trei fapte, în ordinea în care le crede operatorul: (1) linkul MEU,
    // (2) cine mă arată pe MINE, (3) grupul conex AUTOGENERAT — perechea
    // generată de motor, în ambele sensuri.
    //
    // (3) e fix de review (D2): `LaturaPerecheId` e câmp CULES, deci pe draftul
    // autogenerat operatorul îl poate GOLI cu un click. Datele rămân corecte
    // (gardul `Autogenerat` din `GenereazaSecundar` ține), dar o citire numai pe
    // link ar declara apoi „latura pereche lipsește, 581 rămâne deschis" despre
    // un document care o ARE — iar sfatul „culegeți manual piciorul celălalt" ar
    // produce chiar dubla postare pe care felia o închide. Serverul are faptul
    // (copilul e deja în `Copii[]`), deci panoul nu are cum să mintă.
    //
    // Confuzia cu plata autogenerată dintr-o FACTURĂ (31e) e exclusă în ambele
    // sensuri: în (3a) căutăm COPII de tip `DocumentTrezorerie` ai unui document
    // de trezorerie — plata generată de o factură are `DocumentSursa` = factura,
    // deci nu apare niciodată aici; în (3b) — unde riscul chiar EXISTĂ, fiindcă
    // acea plată e `Autogenerat` cu sursă — filtrul cere ca sursa să fie un
    // document de TREZORERIE cu AMBELE laturi conturi proprii (predicatul
    // `EsteVirament`), iar o factură nu e nici măcar `DocumentTrezorerie`.
    Guid? CautaPereche(DevExpress.ExpressApp.IObjectSpace os, bool doarActiva) {
        var id = ID;
        // (1) Linkul propriu.
        if (LaturaPerecheId is Guid propriu) {
            if (!doarActiva)
                return propriu;
            var stare = os.GetObjectsQuery<DocumentTrezorerie>()
                .Where(x => x.ID == propriu)
                .Select(x => (StareDocument?)x.Stare).FirstOrDefault();
            if (stare != null && stare != StareDocument.Stornat)
                return propriu;
            // Capăt STORNAT: legătura nu mai ține — se caută mai departe (poate
            // mă arată altcineva, iar re-operarea poate regenera perechea).
        }
        // (2) Cine mă arată pe mine.
        //
        // Ordonarea nu e cosmetică: după storno + re-operare există DOI pointeri
        // (cel stornat rămâne, cel nou se naște), iar varianta descriptivă i-ar
        // lua pe oricare — adică panoul putea arăta piciorul MORT lângă un
        // `PerecheActiva = true` despre altul. Cel viu are prioritate; cel
        // stornat se arată doar dacă e tot ce există.
        var aratat = os.GetObjectsQuery<DocumentTrezorerie>()
            .Where(x => x.LaturaPerecheId == id
                && (!doarActiva || x.Stare != StareDocument.Stornat))
            .OrderBy(x => x.Stare == StareDocument.Stornat ? 1 : 0)
            .Select(x => (Guid?)x.ID).FirstOrDefault();
        if (aratat != null)
            return aratat;
        // (3a) Copilul AUTOGENERAT al meu (linkul lui a fost golit) — aceeași
        // ordonare, din același motiv (re-operarea lasă în urmă copilul stornat).
        var copil = os.GetObjectsQuery<DocumentTrezorerie>()
            .Where(x => x.DocumentSursaId == id && x.Autogenerat
                && x.Predator is ContPropriu && x.Primitor is ContPropriu
                && (!doarActiva || x.Stare != StareDocument.Stornat))
            .OrderBy(x => x.Stare == StareDocument.Stornat ? 1 : 0)
            .Select(x => (Guid?)x.ID).FirstOrDefault();
        if (copil != null)
            return copil;
        // (3b) Oglinda lui (3a): EU sunt copilul autogenerat, iar linkul meu a
        // fost golit — atunci perechea mea e SURSA. Fără ea minciuna de la (3)
        // rămâne întreagă, doar citită de pe celălalt ecran: piciorul generat,
        // operat, cu linkul golit, ar spune „latura pereche lipsește… anulați și
        // re-operați" — iar re-operarea LUI nu regenerează nimic (e `Autogenerat`).
        if (!Autogenerat || DocumentSursaId is not Guid sursaId)
            return null;
        return os.GetObjectsQuery<DocumentTrezorerie>()
            .Where(x => x.ID == sursaId
                && x.Predator is ContPropriu && x.Primitor is ContPropriu
                && (!doarActiva || x.Stare != StareDocument.Stornat))
            .Select(x => (Guid?)x.ID).FirstOrDefault();
    }

    // Contrapartida (latura care NU e contul propriu) — cheia invariantului de
    // imperechere: plata stinge doar documente pe care apare același repartitor.
    // Metodă, nu proprietate: nu e stare, iar XAF n-are ce căuta pe ea (XAF0033).
    public abstract Guid GetContrapartidaId();

    // Piciorul PROPRIU al documentului (latura care NU e contrapartidă):
    // predatorul la Plata, primitorul la Incasare. Derivat din contrapartidă,
    // ca să existe o singură declarație de direcție per tip.
    public Guid GetContPropriuId() => PredatorId == GetContrapartidaId() ? PrimitorId : PredatorId;

    // Viramentul intern (F7-D1, transferul 581): contrapartida NU e un
    // partener/angajat, ci al DOILEA cont propriu. Discriminarea e a datelor
    // (tipul repartitorului de pe latură), nu a unui tip de document nou —
    // header, linii și ciclu de viață sunt identice cu ale unei plăți normale.
    //
    // Se cer AMBELE laturi conturi proprii, nu doar contrapartida: un draft cu
    // laturile inversate (plată de la un partener CĂTRE casă) are contrapartida
    // ContPropriu fără să fie virament, iar predicatul e consumat de căi care
    // rulează ÎNAINTE de validare (affordance-ul din API, dimensiunile notei) —
    // „gardul care tace devine capcană" (lecțiile F5/F6).
    public bool EsteVirament(DevExpress.ExpressApp.IObjectSpace os) =>
        os.GetObjectByKey<Repartitor>(PredatorId) is ContPropriu
        && os.GetObjectByKey<Repartitor>(PrimitorId) is ContPropriu;

    // Trezoreria e stingătorul „clasic" (31d): o SINGURĂ contrapartidă (latura
    // non-ContPropriu), cu plafonul = totalul brut al plății/încasării. Cum
    // toate stingerile ei merg către aceeași contrapartidă, plafonul per
    // contrapartidă e identic cu plafonul global de dinainte — comportamentul
    // trezoreriei nu se schimbă.
    //
    // Viramentul NU stinge și nu poate fi stins (F7-D5): banii nu părăsesc
    // patrimoniul, deci nicio datorie/creanță nu se închide, iar contrapartida
    // unei plăți normale (partener/angajat) nu apare niciodată pe laturile lui.
    // `null` = „tipul nu stinge nimic", exact contractul bazei — de aici motorul
    // știe să nu creeze imperecherea automată a laturii pereche.
    //
    // F19-D16: plafonul e pe UN SINGUR sens — opusul soldului pe care documentul
    // îl lasă el însuși (plata debitează 401, deci stinge DATORII, dar rămâne un
    // avans = creanță). O contrapartidă × un sens ⇒ nicio ambiguitate și niciun
    // grup de plafon nou: comportamentul trezoreriei rămâne identic.
    public override IReadOnlyDictionary<Guid, PlafonStingere> CapacitateStingere(DevExpress.ExpressApp.IObjectSpace os) =>
        EsteVirament(os)
            ? null
            : new Dictionary<Guid, PlafonStingere> {
                [GetContrapartidaId()] = default(PlafonStingere)
                    .Adauga(SensPropriu().Opus(), Motor.ImperechereService.Total(os, ID))
            };

    // Soldul pe care documentul îl LASĂ pe contul contrapartidei: plata = avans
    // dat (creanță), încasarea = avans primit (datorie). Declarat o singură dată
    // per tip — din el ies AMBELE jumătăți ale rolului (plafonul de mai sus și
    // `SensDeStins` de mai jos), ca ele să nu poată devia una de alta.
    // METODĂ, nu proprietate: pe o clasă persistentă XAF tratează orice
    // proprietate virtuală ca pe un câmp de model (XAF0033).
    protected abstract SensStingere SensPropriu();

    public override SensStingere? SensDeStins(DevExpress.ExpressApp.IObjectSpace os) => SensPropriu();

    // Cealaltă jumătate a lui F7-D5, IMPUSĂ nu doar afirmată: viramentul nu
    // închide nicio datorie/creanță, deci nu poate sta nici pe rolul de document
    // STINS. Argumentul „contrapartida unei plăți normale nu apare pe laturile
    // lui" nu ține: `NotaContabila` aduce contrapartide ARBITRARE din
    // repartitorii expliciți ai liniilor (49a), deci o notă cu repartitorul =
    // contul propriu al viramentului ar trece invariantul de contrapartidă și ar
    // stinge un picior — blocându-i definitiv anularea/stornarea, iar clientul
    // ascunde panoul de stingeri pe virament (F7-D8) ⇒ operatorul n-ar vedea DE CE.
    public override bool PoateFiStins(DevExpress.ExpressApp.IObjectSpace os) => !EsteVirament(os);

    // Dimensiunea Repartitor a notei (F7-D5b): pe virament AMBELE laturi
    // primesc contul propriu AL PICIORULUI. Default-ul bazei (debit←Predator,
    // credit←Primitor) pune pe fiecare rând contrapartida laturii — corect când
    // contrapartida e un partener, dar la un virament între două conturi pe
    // același sintetic (două bănci, ambele 5121 — analiticul se derivă din
    // dimensiuni, decizia 10) ieșirea lui A s-ar atribui lui B și invers:
    // soldul per cont propriu ar ieși exact inversat. Precedentul mecanismului:
    // Decont (32c) și DescarcareGestiune (37a).
    public override Guid RepartitorImplicitDebit(DevExpress.ExpressApp.IObjectSpace os) =>
        EsteVirament(os) ? GetContPropriuId() : base.RepartitorImplicitDebit(os);
    public override Guid RepartitorImplicitCredit(DevExpress.ExpressApp.IObjectSpace os) =>
        EsteVirament(os) ? GetContPropriuId() : base.RepartitorImplicitCredit(os);

    // Latura pereche a viramentului (F7-D4): Plata → Incasare, Incasare → Plata.
    // Contract, nu `is`/`switch` pe tip în clasa de bază (invariantul II).
    protected abstract DocumentTrezorerie CreeazaPereche(DevExpress.ExpressApp.IObjectSpace os);

    // Tipul CLR al laturii pereche (F8-D8, punctul 3): Plata → Incasare,
    // Incasare → Plata. Există separat de `CreeazaPereche` fiindcă validarea are
    // nevoie de tip fără să instanțieze nimic — și fiindcă `is`/`switch` pe tip
    // în clasa de bază ar rupe invariantul II („motorul nu cunoaște frunzele").
    public abstract Type TipLaturaPereche();

    // Cele două picioare ale tranzitului 581 sunt confirmate de documente
    // diferite, la date diferite (foaia de vărsământ azi, extrasul mâine), deci
    // viramentul e o PERECHE, nu un document unic: la operarea primului picior
    // motorul generează draftul celuilalt. Nu e alegere de profil (convenția
    // tranzitului implică două picioare, punct) ⇒ hook de tip, nu PoliticaConex
    // — care ar cere două rânduri simetrice și n-are gard contra recursiei.
    public override Document GenereazaSecundar(DevExpress.ExpressApp.IObjectSpace os) {
        // Gardul de recursie, local și explicit: latura pereche e ea însăși un
        // virament, deci fără el operarea ei ar genera un al treilea document,
        // la infinit.
        //
        // A doua jumătate (F8-D7) e la fel de load-bearing: generarea se suprimă
        // și când CINEVA MĂ ARATĂ PE MINE ca pereche, nu doar când eu arăt spre
        // altcineva. Cazul concret: ambele picioare culese manual ÎNAINTE de
        // operare, cu legătura pusă pe al doilea — la operarea PRIMULUI (care
        // n-are nimic în `LaturaPerecheId`) s-ar genera un al treilea document,
        // adică gaura 64k mutată cu o zi mai devreme și la fel de tăcută.
        //
        // Gardul `Autogenerat` rămâne pe lângă link: două motive independente
        // pentru același refuz, niciunul redundant (copilul poate fi ȘTERS și
        // recules manual — atunci link-ul dispare, marcajul nu).
        //
        // Criteriul e perechea ACTIVĂ (fix review D1): una STORNATĂ are
        // registrele inversate, deci 581 e redeschis și perechea nu s-a produs —
        // re-operarea trebuie s-o REGENEREZE, nu s-o considere făcută.
        if (Autogenerat || !EsteVirament(os) || PerecheActivaId(os) != null)
            return null;

        var pereche = CreeazaPereche(os);
        pereche.Data = Data;
        // Legătura se scrie pe COPIL (F8-D6): el e Draft, deci partea scriibilă.
        pereche.LaturaPerecheId = ID;
        pereche.TipInstrument = TipInstrument;
        // Laturile se copiază CA ATARE, NEinversate (F7-D1): predator = contul
        // sursă, primitor = contul destinație pe ambele documente; direcția o
        // poartă tipul (Plata = banii pleacă, Incasare = banii sosesc).
        pereche.PredatorId = PredatorId;
        pereche.PrimitorId = PrimitorId;
        // NU se copiază: Numar (server-owned, se asignează din seria proprie la
        // operarea perechii) și NumarExtras/DataExtras (fiecare picior are
        // extrasul lui).
        foreach (var s in Detalii) {
            var d = os.CreateObject<DocumentTrezorerieDetaliu>();
            d.Document = pereche;
            d.TipMaterialId = s.TipMaterialId;
            d.Valoare = s.Valoare;
            // `ValoareTva` intră în `Document.Total` (Σ Valoare + ValoareTva —
            // P1/36a), deci se clonează ODATĂ cu valoarea: pe calea API e mereu 0
            // (WriteDto-ul trezoreriei nu-l atinge), dar în UI-ul XAF câmpul e
            // vizibil și editabil pe linia de trezorerie — fără clonare perechea
            // ar ieși cu alt total decât sursa, fără niciun semnal.
            d.ValoareTva = s.ValoareTva;
            d.AngajamentId = s.AngajamentId;
            d.PreiaDimensiuni(s.DimensiuniCulese());
        }
        return pereche;
    }

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        foreach (var d in Detalii)
            if (d.Valoare <= 0)
                erori.Add("Fiecare linie poartă o valoare pozitivă (defalcarea sumei plătite/încasate).");

        ValideazaLaturaPereche(os, erori);

        // Virament către sine: cele două picioare ar posta 581 = 581 pe același
        // cont propriu, iar dimensiunea implicită n-ar mai distinge nimic.
        if (PredatorId != Guid.Empty && PredatorId == PrimitorId)
            erori.Add("Predatorul și primitorul sunt același repartitor — un virament cere două conturi proprii diferite.");

        // Cuplajul laturi ↔ natura liniilor (F7-D3), verificat în AMBELE
        // sensuri. Fără el, un virament cules din greșeală cu Tipul obișnuit de
        // trezorerie ar cădea pe regula GENERICĂ a tipului și ar posta
        // „cont destinație = cont sursă" pe FIECARE picior — dublă postare
        // tăcută. Cuplajul se formulează pe `NaturaClasa` (cod), nu pe coduri de
        // Clasă/Tip sau simboluri de cont: zero scurgere de profil în clasă.
        //
        // Naturile se preîncarcă pe FK-uri (25b — apelantul nu garantează lazy
        // loading), exact ca în motor.
        var idsTip = Detalii.Select(d => d.TipMaterialId).Distinct().ToList();
        var naturi = os.GetObjectsQuery<TipMaterial>()
            .Where(t => idsTip.Contains(t.ID))
            .Select(t => new { t.ID, t.Clasa.Natura })
            .ToDictionary(t => t.ID, t => t.Natura);
        // Tipul lipsă (linie neculeasă) e refuzat deja de baza `Document` — aici
        // nu e virament, deci cade natural pe ramura „linie care nu e virament".
        bool EsteLinieVirament(DocumentDetaliu d) =>
            naturi.TryGetValue(d.TipMaterialId, out var n) && n == NaturaClasa.Virament;

        var esteVirament = EsteVirament(os);
        var liniiVirament = Detalii.Where(EsteLinieVirament).ToList();
        if (esteVirament && liniiVirament.Count != Detalii.Count)
            erori.Add("Contrapartida e un cont propriu (virament intern) — toate liniile trebuie să fie de virament.");
        if (!esteVirament && liniiVirament.Count > 0)
            erori.Add("Liniile de virament intern cer contrapartidă cont propriu (casă/bancă) — corectați latura sau Tipul liniilor.");

        // Oglinda refuzului de pe DSC/FCL (38c): fără regulă de contare potrivită
        // pe Tip sau pe natura Virament, potrivirea ar cădea la final pe regula
        // GENERICĂ a tipului (TipMaterial null + NaturaFiltru null) și ar posta
        // din nou „destinație = sursă" pe ambele picioare, fără niciun zgomot.
        if (liniiVirament.Count > 0) {
            var tipDoc = Motor.MotorOperare.GasesteTipDocument(os, this);
            var reguli = os.GetObjectsQuery<RegulaContare>()
                .Where(r => r.TipDocumentId == tipDoc.ID)
                .Select(r => new { r.TipMaterialId, r.NaturaFiltru, r.SemnFiltru }).ToList();
            foreach (var d in liniiVirament) {
                // OGLINDA trebuie să rămână FIDELĂ potrivirii din motor, altfel
                // gardul devine decor: motorul filtrează ÎNTÂI pe `SemnFiltru`
                // (`MotorOperare`: `r.SemnFiltru == null || r.SemnFiltru == semn`),
                // și abia din supraviețuitori alege Tip exact → NaturaFiltru →
                // generic. Pe trezorerie `Cantitate` e 0, deci semnul e 0 și
                // ORICE rând cu SemnFiltru ±1 iese din joc. `RegulaContare` e dată
                // editabilă în XAF: un semn pus din greșeală pe rândul VIR ar
                // trece de un gard care ignoră semnul, iar motorul ar cădea pe
                // regula generică a tipului — exact dubla postare tăcută pe care
                // gardul există s-o prevină.
                var semn = Math.Sign(d.Cantitate);
                var candidati = reguli.Where(r => r.SemnFiltru == null || r.SemnFiltru == semn);
                if (!candidati.Any(r => r.TipMaterialId == d.TipMaterialId
                        || (r.TipMaterialId == null && r.NaturaFiltru == NaturaClasa.Virament)))
                    erori.Add("Linia de virament nu are regulă de contare potrivită (cont de tranzit = cont propriu) — "
                        + "adăugați rândul de politică (sau rulați updater-ul).");
            }
        }
    }

    // Legătura de pereche NU e obligatorie (generarea acoperă cazul normal —
    // F7-D4); dacă e prezentă, se validează INTEGRAL (F8-D8). Fiecare verificare
    // apără exact un fel de dublă postare sau de triplete: legătura suprimă
    // generarea (F8-D7) și blochează anularea țintei (F8-D9), deci o legătură
    // greșită e o armă, nu o notă informativă.
    //
    // Verificările lucrează pe FK-uri + IObjectSpace, fără navigații lazy în
    // enumerare (25b).
    void ValideazaLaturaPereche(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        if (LaturaPerecheId == null)
            return;

        // (6) Self-link: s-ar suprima propria generare și documentul s-ar bloca
        //     singur la anulare (gardianul F8-D9 s-ar vedea pe el însuși).
        if (LaturaPerecheId == ID) {
            erori.Add("Latura pereche nu poate fi documentul însuși — alegeți celălalt picior al viramentului.");
            return;
        }

        // (1) Cine declară legătura e el însuși virament. O plată obișnuită cu
        //     link ar suprima nimic (nu generează oricum), dar ar bloca anularea
        //     unui document nevinovat prin gardianul F8-D9.
        if (!EsteVirament(os))
            erori.Add("Latura pereche există doar la viramentul intern (ambele laturi conturi proprii) — "
                + "ștergeți legătura sau corectați laturile.");

        var tinta = os.GetObjectByKey<DocumentTrezorerie>(LaturaPerecheId.Value);
        if (tinta == null) {
            erori.Add("Documentul indicat ca latură pereche nu există (a fost șters?) — ștergeți legătura.");
            return;
        }

        // (2) Ținta e virament — aceeași definiție, o singură dată (`EsteVirament`).
        if (!tinta.EsteVirament(os))
            erori.Add($"Documentul {Eticheta(tinta)} nu e un virament intern (laturile lui nu sunt două conturi proprii) — "
                + "nu poate fi latura pereche.");

        // (3) Tipul OPUS, prin contract (fără `is`/`switch` pe tip în bază):
        //     două plăți „pereche" ar posta ieșirea de două ori, iar 581 n-ar
        //     mai reveni la zero niciodată.
        if (!TipLaturaPereche().IsInstanceOfType(tinta))
            erori.Add($"Latura pereche a acestui document trebuie să fie de tipul opus "
                + $"({(TipLaturaPereche() == typeof(Plata) ? "plată" : "încasare")}) — "
                + $"{Eticheta(tinta)} nu e.");

        // (4) ACELEAȘI laturi (F7-D1: cele două picioare stau pe aceleași laturi,
        //     direcția o poartă tipul). Laturi diferite = alt transfer, iar
        //     tranzitul 581 ar rămâne deschis pe amândouă.
        if (tinta.PredatorId != PredatorId || tinta.PrimitorId != PrimitorId)
            erori.Add($"Latura pereche trebuie să aibă exact aceleași conturi (predator/primitor) ca acest document — "
                + $"{Eticheta(tinta)} are alte laturi.");

        // (4b) Ținta nu e o latură GENERATĂ a altui virament. Ea aparține deja
        //      transferului ei prin grupul conex (`DocumentSursa`), iar linkul —
        //      fiind câmp CULES — poate fi golit de pe ea cu un click, ceea ce ar
        //      face-o să pară liberă. Fără refuzul ăsta, un al treilea document
        //      s-ar putea declara perechea ei: la operare ambele picioare de
        //      intrare postează, iar 581 iese dublat — exact gaura pe care felia
        //      o închide. Afordanța (lista de candidați, avertismentul) o exclude
        //      deja; aici e autoritatea.
        if (tinta.Autogenerat && tinta.DocumentSursaId != null)
            erori.Add($"Documentul {Eticheta(tinta)} e latura pereche GENERATĂ a altui virament — "
                + "aparține transferului lui. Alegeți piciorul cules manual sau ștergeți acel draft.");

        // (5) Ținta nu e deja legată — de nimeni, nici măcar de MINE. Cazul
        //     „altcineva" apără de triplete: două picioare de intrare declarate
        //     pereche ale aceleiași ieșiri s-ar opera amândouă și ar dubla
        //     postarea, tăcut.
        //
        //     Cazul RECIPROC (A→B și B→A) e refuzat ca amendament la F8-D8,
        //     scos de review-ul pasului 1: legătura e unilaterală prin
        //     construcție (F8-D6 — cealaltă parte e Operată, iar gardianul de
        //     Committing refuză scrierea pe ea), iar dublarea ei e o capcană cu
        //     ieșire zero: după operarea ambelor picioare fiecare l-ar bloca pe
        //     celălalt la anulare/storno (F8-D9), și nici linkul nu se mai poate
        //     șterge — documentele nu mai sunt Draft.
        //
        //     NUANȚA (fix review D1): o legătură al cărei celălalt capăt e
        //     STORNAT nu mai blochează pe nimeni. Registrele lui sunt inversate,
        //     deci perechea nu s-a produs — iar refuzul ar fi fost o FUNDĂTURĂ:
        //     remediul („ștergeți acea legătură") e imposibil pe un document
        //     stornat, care nu se mai editează și nu se mai șterge.
        if (tinta.LaturaPerecheId == ID)
            erori.Add($"Documentul {Eticheta(tinta)} vă declară DEJA ca latură pereche — "
                + "legătura se ține pe o singură parte: e de ajuns să ștergeți UNA dintre "
                + "cele două legături (de pildă pe aceasta).");
        else if (tinta.LaturaPerecheId is Guid idTert
                && StareDocumentului(os, idTert) != StareDocument.Stornat)
            erori.Add($"Documentul {Eticheta(tinta)} e deja declarat perechea altui document — "
                + "un virament are exact două picioare.");
        var idTinta = tinta.ID;
        var altPointer = os.GetObjectsQuery<DocumentTrezorerie>()
            .Where(x => x.LaturaPerecheId == idTinta && x.ID != ID
                && x.Stare != StareDocument.Stornat)
            .Select(x => new { x.Numar, x.Data, x.Stare, x.Autogenerat })
            .FirstOrDefault();
        if (altPointer != null) {
            var etichetaPointer = Eticheta(altPointer.Numar, altPointer.Data);
            // Cazul cel mai frecvent al refuzului ăstuia NU e o legătură greșită a
            // altcuiva, ci draftul pe care sistemul l-a generat singur la operarea
            // țintei (64k): „e deja perechea altui document" l-ar lăsa pe operator
            // să caute un vinovat care nu există. Numim artefactul și dăm ordinea
            // corectă — el e cel care dispare, nu legătura pe care o culege acum.
            erori.Add(altPointer.Stare == StareDocument.Draft && altPointer.Autogenerat
                ? $"Documentul {Eticheta(tinta)} are deja o latură pereche GENERATĂ automat "
                    + $"({etichetaPointer}), încă în lucru — ștergeți acel draft, apoi legați acest document."
                : $"Documentul {Eticheta(tinta)} e deja arătat ca pereche de alt document "
                    + $"({etichetaPointer}) — ștergeți acea legătură sau alegeți alt picior.");
        }
    }

    // Avertismentul CONSULTATIV (F8-D10), nu un refuz: două viramente identice
    // între aceleași conturi, în aceeași zi, sunt perfect legitime (64k), deci
    // niciun criteriu de CONȚINUT nu poate distinge „al doilea picior al lui X"
    // de „un al doilea virament". Ce se poate face onest e să-i ARĂTĂM
    // operatorului picioarele candidate în clipa în care tocmai a generat unul nou.
    public override IReadOnlyList<string> MesajeDupaOperare(DevExpress.ExpressApp.IObjectSpace os) {
        try {
            // Doar cazul „am generat singur perechea": legătura DECLARATĂ (a mea
            // sau a altcuiva spre mine) a suprimat generarea, deci n-am ce sfat
            // să dau. `Autogenerat` = sunt eu latura generată — la fel.
            if (LaturaPerecheId != null || Autogenerat || !EsteVirament(os))
                return Array.Empty<string>();
            var id = ID;
            var amGenerat = os.GetObjectsQuery<DocumentTrezorerie>()
                .Any(x => x.LaturaPerecheId == id && x.Autogenerat);
            if (!amGenerat)
                return Array.Empty<string>();

            // Candidații: picioare OPERATE, pe ACELEAȘI laturi, fără pereche.
            // Fiind pe aceleași laturi ca mine (și eu sunt virament), sunt
            // viramente prin construcție — nu se re-interoghează tipul
            // repartitorilor.
            //
            // Criteriul e „fără pereche OPERATĂ", NU „fără pereche" — și distincția
            // e chiar scenariul 64k, altfel mesajul e mort exact acolo unde a fost
            // scris: piciorul de ieșire operat și-a GENERAT un draft care-l arată,
            // deci un filtru pe „e arătat de cineva" l-ar scoate din listă tocmai
            // când operatorul culege manual celălalt picior. Contabil, 581 se
            // închide doar când al doilea picior e OPERAT; un draft e o intenție.
            //
            // Doar pointer-ul OPERAT scoate candidatul definitiv (fix review D1:
            // înainte era „nu e Draft", deci și cel STORNAT — al cărui capăt are
            // registrele inversate, deci n-a produs nicio pereche, și pe care
            // remediul nu-l poate atinge). Cel Draft îl lasă în listă, dar e NUMIT:
            // F8-D8 punctul 5 refuză legarea cât timp el arată spre țintă, iar un
            // sfat care nu se poate executa e mai rău decât tăcerea.
            var pred = PredatorId;
            var prim = PrimitorId;
            var cuPerecheOperata = os.GetObjectsQuery<DocumentTrezorerie>()
                .Where(x => x.LaturaPerecheId != null && x.Stare == StareDocument.Operat)
                .Select(x => x.LaturaPerecheId.Value);
            var posibili = os.GetObjectsQuery<DocumentTrezorerie>()
                .Where(x => x.PredatorId == pred && x.PrimitorId == prim
                    && x.Stare == StareDocument.Operat
                    && x.LaturaPerecheId == null && x.ID != id
                    && !cuPerecheOperata.Contains(x.ID)
                    // O latură GENERATĂ de motor aparține deja transferului ei
                    // prin grupul conex, chiar cu linkul golit (câmpul e cules) —
                    // n-are ce căuta printre picioarele „descoperite". Geamănul
                    // predicatului e în `CandidatiPereche`; autoritatea, în
                    // `ValideazaLaturaPereche`.
                    && !(x.Autogenerat && x.DocumentSursaId != null));
            // Tipul OPUS, filtrat ÎN SQL (sub TPT: LEFT JOIN + IS NOT NULL), dar
            // tot din CONTRACTUL domeniului: `Expression.TypeIs` peste
            // `TipLaturaPereche()` — nici `is` pe tip în clasa de bază
            // (invariantul II), nici materializarea tuturor viramentelor dintre
            // cele două conturi ca să le filtrăm în memorie (hook-ul rulează la
            // FIECARE operare de virament; geamănul din `CandidatiPereche` merge
            // de mult pe proiecție + plafon).
            var x0 = System.Linq.Expressions.Expression.Parameter(typeof(DocumentTrezorerie), "x");
            posibili = posibili.Where(
                System.Linq.Expressions.Expression.Lambda<Func<DocumentTrezorerie, bool>>(
                    System.Linq.Expressions.Expression.TypeIs(x0, TipLaturaPereche()), x0));

            // Plafon +1 ca să știm dacă mai sunt (fără un al doilea COUNT).
            const int plafon = 5;
            var candidati = posibili
                .OrderByDescending(x => x.Data).ThenByDescending(x => x.ID)
                .Select(x => new { x.ID, x.Numar, x.Data })
                .Take(plafon + 1).ToList();
            if (candidati.Count == 0)
                return Array.Empty<string>();

            var ids = candidati.Select(c => c.ID).ToList();
            var drafturiBlocante = os.GetObjectsQuery<DocumentTrezorerie>()
                .Where(x => x.LaturaPerecheId != null && ids.Contains(x.LaturaPerecheId.Value)
                    && x.Stare == StareDocument.Draft)
                .Select(x => new { Tinta = x.LaturaPerecheId.Value, x.Numar, x.Data })
                .ToList();

            string Descrie(Guid idCandidat, string numar, DateOnly data) {
                var blocant = drafturiBlocante.FirstOrDefault(p => p.Tinta == idCandidat);
                return blocant == null
                    ? Eticheta(numar, data)
                    : $"{Eticheta(numar, data)} (blocat de draftul {Eticheta(blocant.Numar, blocant.Data)} — ștergeți-l întâi)";
            }

            var lista = string.Join(", ",
                candidati.Take(plafon).Select(c => Descrie(c.ID, c.Numar, c.Data)));
            if (candidati.Count > plafon)
                lista += " …și altele";
            return new[] {
                $"S-a generat latura pereche a viramentului, dar există deja picioare operate compatibile: {lista}. "
                + "Dacă acesta e piciorul lor, anulați operarea, ștergeți draftul generat și alegeți «latura pereche»."
            };
        }
        catch (Exception) {
            // Contractul hook-ului (F8-D10): informarea nu are voie să strice o
            // operare deja COMISĂ. Fără mesaj > cu excepție.
            return Array.Empty<string>();
        }
    }

    // Starea unui document, citită PLAT (25b — nicio entitate materializată doar
    // ca să-i aflăm starea). `null` = nu există.
    static StareDocument? StareDocumentului(DevExpress.ExpressApp.IObjectSpace os, Guid id) =>
        os.GetObjectsQuery<DocumentTrezorerie>()
            .Where(x => x.ID == id).Select(x => (StareDocument?)x.Stare).FirstOrDefault();

    static string Eticheta(Document doc) => Eticheta(doc.Numar, doc.Data);

    // Varianta pe câmpuri PLATE: mesajele care numesc un ALT document îl citesc
    // dintr-o proiecție, nu dintr-o navigație materializată (25b).
    static string Eticheta(string numar, DateOnly data) =>
        string.IsNullOrWhiteSpace(numar) ? $"({data:dd.MM.yyyy})" : $"{numar} din {data:dd.MM.yyyy}";
}

// Predator = ContPropriu (sursa banilor), primitor = beneficiarul.
// [TipDetaliu] se declară pe fiecare derivată (atributul e Inherited=false).
[TipDetaliu(typeof(DocumentTrezorerieDetaliu))]
public class Plata : DocumentTrezorerie {
    public override Guid GetContrapartidaId() => PrimitorId;

    // Plata debitează contul contrapartidei ⇒ stinge DATORII (FCT, DEC, avansul
    // primit); ea însăși rămâne un avans dat = CREANȚĂ față de contrapartidă,
    // stinsă de o încasare de regularizare (lanțul 31d).
    protected override SensStingere SensPropriu() => SensStingere.Creanta;

    protected override DocumentTrezorerie CreeazaPereche(DevExpress.ExpressApp.IObjectSpace os) =>
        os.CreateObject<Incasare>();
    public override Type TipLaturaPereche() => typeof(Incasare);

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        if (os.GetObjectByKey<Repartitor>(PredatorId) is not ContPropriu)
            erori.Add("Predatorul plății este contul propriu (casă/bancă) din care se plătește.");
        // Contrapartida acceptă și un cont propriu (F7-D3): viramentul intern
        // e o plată către al doilea cont propriu, nu un tip de document aparte.
        if (os.GetObjectByKey<Repartitor>(PrimitorId) is not (Partener or Angajat or ContPropriu))
            erori.Add("Primitorul plății este beneficiarul — un partener, un angajat (avans) sau un cont propriu (virament intern).");
    }
}

// Predator = plătitorul, primitor = ContPropriu (destinația banilor).
[TipDetaliu(typeof(DocumentTrezorerieDetaliu))]
public class Incasare : DocumentTrezorerie {
    public override Guid GetContrapartidaId() => PredatorId;

    // Oglinda plății: încasarea creditează contrapartida ⇒ stinge CREANȚE (FCL,
    // avansul dat); ea însăși rămâne un avans primit = DATORIE.
    protected override SensStingere SensPropriu() => SensStingere.Datorie;

    protected override DocumentTrezorerie CreeazaPereche(DevExpress.ExpressApp.IObjectSpace os) =>
        os.CreateObject<Plata>();
    public override Type TipLaturaPereche() => typeof(Plata);

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        // Contrapartida acceptă și un cont propriu (F7-D3) — vezi Plata.
        if (os.GetObjectByKey<Repartitor>(PredatorId) is not (Partener or Angajat or ContPropriu))
            erori.Add("Predatorul încasării este plătitorul — un partener, un angajat sau un cont propriu (virament intern).");
        if (os.GetObjectByKey<Repartitor>(PrimitorId) is not ContPropriu)
            erori.Add("Primitorul încasării este contul propriu (casă/bancă) în care se încasează.");
    }
}

// DIM-2 (decizia 54e, inventar §2, Î1/Î2): frunza UNICĂ a defalcării PLT+INC —
// semantica liniei e identică (defalcarea sumei, 31a), iar plata autogenerată
// clonează dimensiunile liniilor FCT, deci frunza poartă reuniunea FCT.
// Obligativitatea e politică per profil (PoliticaValidare/DimensiuniObligatorii,
// decizia 54d): la bugetar cerute ca date, la privat opționale — nimic hardcodat.
public class DocumentTrezorerieDetaliu : DocumentDetaliu {
    public virtual Guid? CodEconomicId { get; set; }
    [XafDisplayName("Cod economic")]
    public virtual CodEconomic CodEconomic { get; set; }

    public virtual Guid? SursaFinantareId { get; set; }
    [XafDisplayName("Sursă de finanțare")]
    public virtual SursaFinantare SursaFinantare { get; set; }

    public virtual Guid? CodFunctionalId { get; set; }
    [XafDisplayName("Cod funcțional")]
    public virtual CodFunctional CodFunctional { get; set; }

    public virtual Guid? ProiectId { get; set; }
    [XafDisplayName("Proiect")]
    public virtual Proiect Proiect { get; set; }

    public override Dimensiuni DimensiuniCulese() => new() {
        CodEconomicId = CodEconomicId, SursaFinantareId = SursaFinantareId,
        CodFunctionalId = CodFunctionalId, ProiectId = ProiectId
    };
    public override void PreiaDimensiuni(Dimensiuni s) {
        CodEconomicId = s.CodEconomicId; SursaFinantareId = s.SursaFinantareId;
        CodFunctionalId = s.CodFunctionalId; ProiectId = s.ProiectId;
    }
}

// Decizia 17: stingerea — m2m stingător↔document cu sume parțiale
// (GEST_DECONTARI). Nu e document (fără ciclu Draft/Operat): un rând e o
// legătură între două documente DEJA operate. Invarianții
// (motor/ImperechereService): ambele Operat, Σ imperecheri ≤ totalul
// documentului stins, contrapartida stingătorului apare pe el; `ramas` e
// calcul, nu coloană. Autogenerat = creată de motor la operarea unei plăți
// autogenerate (plata automată din factură — 00 §7).
//
// `DocumentStingator` e tipat `Document`, nu `DocumentTrezorerie` (decizia 48b
// — compensarea): rolul de stingător e declarat POLIMORF, prin
// `Document.CapacitateStingere`, iar azi îl poartă trezoreria și nota
// contabilă. Tipul FK-ului nu mai face filtrarea — o face validarea.
[NavigationItem("Documente")]
public class Imperechere : BaseObject {
    public virtual Guid DocumentStingatorId { get; set; }
    public virtual Document DocumentStingator { get; set; }
    public virtual Guid DocumentId { get; set; }
    public virtual Document Document { get; set; }
    public virtual decimal Suma { get; set; }
    // Marcaj de proveniență (creată de motor la plata autogenerată) — nu se culege
    // de operator, deci read-only în UI.
    [ModelDefault("AllowEdit", "False")]
    public virtual bool Autogenerat { get; set; }
}
