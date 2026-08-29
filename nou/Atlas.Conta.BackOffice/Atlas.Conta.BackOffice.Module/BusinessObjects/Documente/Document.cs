using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Sensul stingerii (F19-D16): ce FEL de sold se închide. Aceeași axă pe ambele
// capete ale imperecherii — plafonul stingătorului e defalcat pe ea, iar
// documentul stins declară pe care jumătate o consumă (`Document.SensDeStins`).
//
// Nu e persistat nicăieri: e o funcție a tipului de document, calculată la
// fiecare validare. De aceea nu are migrație și nu are coloană pe `Imperechere`.
public enum SensStingere {
    // Stingătorul DEBITEAZĂ contul contrapartidei ⇒ închide un sold creditor
    // (ce datorăm): plata, jumătatea de DEBIT a notei de compensare.
    [XafDisplayName("Datorie")] Datorie,
    // Stingătorul CREDITEAZĂ contul contrapartidei ⇒ închide un sold debitor
    // (ce ni se datorează): încasarea, jumătatea de CREDIT a notei.
    [XafDisplayName("Creanță")] Creanta
}

public static class SensStingereExtensii {
    // Sensul OPUS: soldul pe care un document îl LASĂ pe contul contrapartidei e
    // opusul celui pe care îl STINGE (plata debitează 401 ⇒ stinge datorii, dar
    // rămâne ea însăși un avans = creanță). Un singur loc unde se scrie relația,
    // ca cele două jumătăți ale trezoreriei să nu poată devia una de alta.
    public static SensStingere Opus(this SensStingere sens) =>
        sens == SensStingere.Datorie ? SensStingere.Creanta : SensStingere.Datorie;
}

// Plafonul unei contrapartide, DEFALCAT PE SENS (F19-D16). Înainte era un
// singur `decimal`: nota 401 = 4111 de 60 pe X aduna 120 într-o cheie și îi
// consuma INTEGRAL pe două documente de aceeași natură. Tavanul de 120 era
// corect (60 datorie + 60 creanță = exact cele două stingeri legitime) — lipsea
// regula că fiecare jumătate se consumă pe latura ei.
public readonly record struct PlafonStingere(decimal Datorie, decimal Creanta) {
    public decimal this[SensStingere sens] =>
        sens == SensStingere.Datorie ? Datorie : Creanta;

    public PlafonStingere Adauga(SensStingere sens, decimal valoare) =>
        sens == SensStingere.Datorie
            ? this with { Datorie = Datorie + valoare }
            : this with { Creanta = Creanta + valoare };

    // Sensurile pe care plafonul chiar OFERĂ ceva. Un plafon integral zero
    // întoarce totuși `Datorie`, ca alegerea să rămână determinist unică:
    // stingerea cade oricum pe mesajul de plafon depășit, exact ca înainte.
    public IReadOnlyList<SensStingere> Sensuri {
        get {
            var l = new List<SensStingere>(2);
            if (Datorie != 0m) l.Add(SensStingere.Datorie);
            if (Creanta != 0m) l.Add(SensStingere.Creanta);
            if (l.Count == 0) l.Add(SensStingere.Datorie);
            return l;
        }
    }
}

// Nucleul generic (deciziile 1, 2, 22). Motoarele de stoc și contare consumă
// DOAR această clasă și DocumentDetaliu — orice câmp de aici e justificat de o
// formulă de stoc, o regulă contabilă sau un motor transversal (testul bazei).
//
// Ciclul de viață se distinge VIZUAL (GATE XAF D12): Draft = neutru (starea de
// lucru), Operat = verde bold (registrele există), Stornat = gri tăiat (rândurile
// au fost inversate). Roșul rămâne al erorilor de validare, nu al stornării.
[NavigationItem("Documente")]
[Appearance("Document_Stare_Operat", AppearanceItemType.ViewItem, "Stare = 'Operat'",
    TargetItems = nameof(Stare), FontColor = "Green", FontStyle = DevExpress.Drawing.DXFontStyle.Bold)]
[Appearance("Document_Stare_Stornat", AppearanceItemType.ViewItem, "Stare = 'Stornat'",
    TargetItems = nameof(Stare), FontColor = "Gray", FontStyle = DevExpress.Drawing.DXFontStyle.Strikeout)]
public abstract class Document : BaseObject {
    [XafDisplayName("Număr")]
    public virtual string Numar { get; set; }
    [XafDisplayName("Dată")]
    public virtual DateOnly Data { get; set; }

    // Validare de CULEGERE (context Save al pipeline-ului UI XAF): FK-urile
    // Predator/PrimitorId sunt NOT NULL în schemă, dar Guid.Empty NU e null —
    // regula stă pe NAVIGAȚIE, ca să blocheze commit-ul înainte ca INSERT-ul cu
    // FK invalid să ajungă în Postgres. ATENȚIE: pe calea Operează motorul
    // rulează în ObjectSpace-ul View-ului, deci commit-ul operării trece TOT
    // prin aceste reguli (obiectele create de motor — conex/plată/registre — au
    // FK-urile setate, iar navigația se rezolvă prin fixup/lazy-load); culegerea
    // se comite separat, ÎNAINTE de motor (DocumentOperareController.Executa).
    // Doar căile standalone (ModelCheck/Migrare/seed — EFCoreObjectSpaceProvider
    // fără controllere) sunt în afara regulilor.
    // Repartitori (sute la migrare) — nomenclator mare: lookup standard
    // (SmartLookup revertat, decizia 40d/gate).
    public virtual Guid PredatorId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Predator (de la)")]
    [RuleRequiredField("Document_Predator_Necesar", DefaultContexts.Save,
        CustomMessageTemplate = "Predatorul (de la cine) este obligatoriu.")]
    public virtual Repartitor Predator { get; set; }
    public virtual Guid PrimitorId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Primitor (către)")]
    [RuleRequiredField("Document_Primitor_Necesar", DefaultContexts.Save,
        CustomMessageTemplate = "Primitorul (către cine) este obligatoriu.")]
    public virtual Repartitor Primitor { get; set; }

    // Decizia 14: Draft → Operat → (Stornat); la operare motorul scrie registrele.
    // Cele patru câmpuri de mai jos sunt ALE MOTORULUI (GATE XAF D8): read-only în
    // UI ÎNTOTDEAUNA, pe orice cale (DetailView + editare inline în ListView) —
    // altfel operatorul putea trece un draft pe „Operat" cu mâna, fără registre.
    [ModelDefault("AllowEdit", "False")]
    public virtual StareDocument Stare { get; set; }
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Data operării")]
    public virtual DateTime? DataOperare { get; set; }

    // Decizia 17: legătura conex sursă→generat (ex. FacturaIntrare → NIR);
    // anularea operează pe tot grupul (00 §8).
    public virtual Guid? DocumentSursaId { get; set; }
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Document sursă")]
    public virtual Document DocumentSursa { get; set; }
    [ModelDefault("AllowEdit", "False")]
    public virtual bool Autogenerat { get; set; }

    [DevExpress.ExpressApp.DC.Aggregated]
    public virtual ObservableCollection<DocumentDetaliu> Detalii { get; set; } = new();

    // Explicit BRUT (P1, design §3): imperecherea și plata autogenerată sting
    // brutul. La regimurile capitalizate ValoareTva e 0, deci rămâne exact
    // vechiul Σ Valoare — invarianții ImperechereService nu se schimbă.
    // VIRTUAL (FAZA 1C §7): ReturClient poartă linii pe două roluri (venit +
    // cost) și totalul lui = doar liniile de venit — brutul care ajustează
    // creanța; costul e mișcare internă venit↔stoc, nu creanță.
    //
    // Pe DetailView e câmpul cu care operatorul confruntă hârtia ÎNAINTE de
    // operare (GATE XAF D5); în ListView-urile root al celor două ecrane de
    // felie e ascuns prin baseline — enumerarea Detalii per rând = N+1
    // (disciplina de hot-path, 35d).
    [NotMapped]
    [XafDisplayName("Total (brut)")]
    public virtual decimal Total => Detalii.Sum(d => d.Valoare + d.ValoareTva);

    // Geamănul SERVER-SIDE al lui Total (review advers 1C-a): liniile care
    // constituie creanța/datoria stinsă de imperechere. ImperechereService nu
    // poate enumera navigația Detalii (apelanții nu garantează lazy loading),
    // deci filtrează query-ul prin hook-ul ăsta — orice derivată care
    // suprascrie Total trebuie să țină cele două filtre în oglindă.
    public virtual IQueryable<DocumentDetaliu> LiniiCreanta(IQueryable<DocumentDetaliu> linii) => linii;

    // Hooks polimorfe consumate DOAR de motorul de operare (decizia 14).
    // Primesc IObjectSpace și lucrează pe FK-uri, nu pe navigații — contextul
    // apelant (UI, harness, viitorul Web API) nu garantează lazy loading.
    // Lanțul de valori aparține derivatei (testul bazei §3): aici derivata
    // materializează `Valoare` pe linii înainte de scrierea registrelor
    // (ex. NotaTransfer/BonConsum: preț lot × cantitate).
    public virtual void PregatesteOperare(DevExpress.ExpressApp.IObjectSpace os) { }

    // Convenția 00 §5 (dimensiunea Repartitor default pe notă: debit←Predator,
    // credit←Primitor) devine default POLIMORF — ultimul nivel al coalesce-ului
    // din motor. Decont o ajustează: creditul (contul de avans 542) urmărește
    // titularul, nu primitorul justificării.
    // Primesc `IObjectSpace` ca toate celelalte hook-uri ale motorului (vezi
    // nota de mai sus): trezoreria are nevoie de el ca să distingă viramentul
    // intern după TIPUL repartitorului de pe latură (F7-D5b).
    public virtual Guid RepartitorImplicitDebit(DevExpress.ExpressApp.IObjectSpace os) => PredatorId;
    public virtual Guid RepartitorImplicitCredit(DevExpress.ExpressApp.IObjectSpace os) => PrimitorId;

    // Gestiunea în care se NASC loturile culese pe liniile documentului
    // (F5-D2) — hook polimorf consumat de `LoturiCulegereService`, pe FK-uri +
    // IObjectSpace ca toate hook-urile motorului (25b: apelanții nu garantează
    // lazy loading).
    //
    // Default = latura PRIMITOARE, adevărată pe ambele tipuri de INTRARE care
    // culeg azi produse: FCT (Partener → Gestiune) și NIR (Partener →
    // Gestiune). Tipurile la care marfa apare pe latura PREDATOARE (plusul de
    // inventar — gestiunea inventariată, 28d; produsele Asamblării, 46d) vor
    // face override când intră în scopul culegerii (restanța 53i); hook-ul e
    // ancora pentru ele, nu se scrie nimic speculativ acum.
    public virtual Gestiune GestiuneLoturiCulese(DevExpress.ExpressApp.IObjectSpace os) =>
        PrimitorId != Guid.Empty ? os.GetObjectByKey<Repartitor>(PrimitorId) as Gestiune : null;

    // Rolul de STINGĂTOR în imperechere (decizia 31d, extinsă de 48b —
    // compensarea): contrapartidele pe care documentul le poate stinge, fiecare
    // cu PLAFONUL ei. `null` = tipul NU stinge nimic (majoritatea — facturile
    // stau doar pe rolul de document stins); dicționar gol = tipul stinge, dar
    // documentul ăsta n-are contrapartidă (refuz zgomotos în validare).
    // Invariantul rămâne cel din 31d: contrapartida trebuie să apară pe laturile
    // documentului stins, iar Σ stingerilor către ea ≤ plafon. Plafonul e per
    // contrapartidă (nu pe tot documentul) fiindcă nota de compensare e DUBLĂ
    // prin construcție: 401 = 4111 pe partenerul X stinge X lei de datorie ȘI
    // X lei de creanță — un plafon global ar refuza a doua stingere legitimă.
    // Hook pe FK-uri + IObjectSpace (25b): apelanții nu garantează lazy loading.
    // De la F19-D16 plafonul e DEFALCAT PE SENS (`PlafonStingere`): cheia rămâne
    // contrapartida, dar fiecare jumătate se consumă pe latura ei. Cheia
    // NESCHIMBATĂ (Guid) e deliberată — apelanții care întreabă doar „apare
    // contrapartida asta?" rămân neatinși.
    public virtual IReadOnlyDictionary<Guid, PlafonStingere> CapacitateStingere(DevExpress.ExpressApp.IObjectSpace os) => null;

    // Cealaltă jumătate a rolului: `CapacitateStingere` spune „pot STINGE",
    // asta spune „pot fi STINS". Default `true` — orice document cu rest e
    // candidat de stins (facturi, deconturi, plăți în lanțul avans↔
    // regularizare). Există separat fiindcă cele două NU se implică reciproc:
    // un stingător al cărui `CapacitateStingere` e null nu e prin asta protejat
    // de rolul de stins — `ImperechereService` alege contrapartida din
    // capacitățile STINGĂTORULUI, iar acelea pot fi arbitrare (`NotaContabila`
    // le ia din repartitorii expliciți ai liniilor), deci pot cădea pe laturile
    // ORICĂRUI document. Fără hook, „documentul ăsta nu se stinge" ar fi doar o
    // afirmație din comentarii. Hook pe FK-uri + IObjectSpace (25b).
    public virtual bool PoateFiStins(DevExpress.ExpressApp.IObjectSpace os) => true;

    // A TREIA jumătate a rolului (F19-D16): CE FEL de sold poartă documentul pe
    // contul contrapartidei, deci din ce jumătate a plafonului se stinge.
    // `Datorie` = poartă un sold CREDITOR (îi datorăm) și se stinge DEBITÂND
    // contrapartida — FCT, INC (avans primit), DEC, RDC. `Creanta` = sold
    // DEBITOR (ni se datorează), se stinge CREDITÂND — FCL, PLT (avans dat), RLF.
    //
    // `null` = tipul NU declară. Default DELIBERAT, nu accidental: motorul nu
    // ghicește niciodată o jumătate: dacă stingătorul oferă exact un sens față de
    // contrapartida aleasă (toată trezoreria — o singură contrapartidă, un
    // singur sens) comportamentul e IDENTIC cu cel de dinainte de F19-D16; dacă
    // oferă două (nota de compensare 401 = 4111), alegerea e AMBIGUĂ și se
    // refuză zgomotos în loc să cadă pe „prima" jumătate. Simetric, pe consum
    // (`ImperechereService.AsignatFataDe`) un document nedeclarat se scade din
    // AMBELE sensuri — conservator: nu deschide plafon.
    // Hook pe FK-uri + IObjectSpace (25b), ca surorile lui.
    public virtual SensStingere? SensDeStins(DevExpress.ExpressApp.IObjectSpace os) => null;

    // Documentul SECUNDAR (00 §7 — plata automată legacy, decizia 31): spre
    // deosebire de conexul din PoliticaConex (clonă filtrată pe natură, trăiește
    // în motor), secundarul se construiește din date CULESE pe derivată
    // (grupul DECONT_* al facturii), deci generarea e hook de tip. Motorul îl
    // marchează Autogenerat + DocumentSursa și îl tratează ca pe orice copil
    // al grupului conex (ștergere la anulare, refuz cât e operat).
    public virtual Document GenereazaSecundar(DevExpress.ExpressApp.IObjectSpace os) => null;

    // Informări pentru operator DUPĂ o operare reușită (F8-D10) — NU e o cale de
    // refuz: `OperareApi.Opereaza` le adaugă în `OperareRezultat.Mesaje`, lângă
    // mesajul documentului conex, iar ambele tiere (API + controllerul XAF) le
    // afișează pe calea existentă. Se apelează DUPĂ commit-ul motorului, deci
    // vede lumea finală (inclusiv copiii tocmai generați).
    //
    // Contract: hook-ul NU are voie să arunce. Un mesaj lipsă e o pierdere de
    // confort; o excepție aici ar transforma o operare CU REGISTRE DEJA COMISE
    // într-un eșec aparent pentru apelant. Implementările prind și tac.
    public virtual IReadOnlyList<string> MesajeDupaOperare(DevExpress.ExpressApp.IObjectSpace os) =>
        Array.Empty<string>();

    // Invariantele proprii tipului, verificate de motor înainte de operare.
    // Baza impune doar ce cere orice document; obligativitățile per tip se
    // adaugă în override (validarea declarativă completă vine la 3c/3d).
    public virtual void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        if (Detalii.Count == 0)
            erori.Add("Documentul nu are nicio linie.");
        if (PredatorId == Guid.Empty || PrimitorId == Guid.Empty)
            erori.Add("Predatorul și primitorul sunt obligatorii.");
        // Oglinda NOT NULL-ului din schemă, în motor (nu doar în regula UI de
        // Save): un refuz aici vine ÎNAINTE de materializare (33d) — doar FK-ul
        // scalar, fără navigații lazy în enumerare (25b).
        if (Detalii.Any(d => d.TipMaterialId == Guid.Empty))
            erori.Add("Toate liniile trebuie să aibă Tipul (contul/clasa) completat.");
    }
}

// Bază concretă: NIR/BonConsum/NotaTransfer o folosesc direct (testul bazei §6);
// derivate de detaliu există doar unde schema diferă.
public class DocumentDetaliu : BaseObject {
    [Browsable(false)]
    public virtual Guid DocumentId { get; set; }
    public virtual Document Document { get; set; }

    // Cheia contării; Clasa (cheia regulilor de stoc) = TipMaterial.Clasa.
    // Validare de culegere pe NAVIGAȚIE (ca Predator/Primitor pe header —
    // vezi nota de acolo despre calea Operează): TipMaterialId e NOT NULL,
    // iar o linie culeasă fără tip ar produce un INSERT cu FK invalid.
    public virtual Guid TipMaterialId { get; set; }
    // Nomenclator mare de tipuri: lookup standard (SmartLookup revertat,
    // decizia 40d/gate).
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Tip (cont/clasă)")]
    [RuleRequiredField("DocumentDetaliu_TipMaterial_Necesar", DefaultContexts.Save,
        CustomMessageTemplate = "Tipul (contul/clasa) liniei este obligatoriu.")]
    public virtual TipMaterial TipMaterial { get; set; }

    // Cheia stocului (decizia 13); null pe tipurile fără stoc.
    public virtual Guid? LotId { get; set; }
    public virtual Lot Lot { get; set; }

    // SEMNATĂ — limbajul motorului e semn × cantitate; LDI poartă direcția în semn.
    public virtual decimal Cantitate { get; set; }

    // Valoarea de postare în registre (stoc valoric + note) — capătul lanțului
    // de valori; derivatele o calculează (testul bazei §3). Cu TVA structural
    // (P1): net la regim deductibil, brut la Capitalizat — semantica „o singură
    // valoare de postare a rândului principal" (22a) nu se schimbă.
    public virtual decimal Valoare { get; set; }

    // P1 (design §3): postarea 4426/4427 e regulă contabilă generică → testul
    // apartenenței (decizia 2) pune TVA-ul pe BAZĂ, nu pe interfață. Null =
    // linie fără semantică de TVA (BTR/BCS/LDI/NIR/PLT/INC — neschimbate).
    public virtual Guid? TipTvaId { get; set; }
    [XafDisplayName("Tip TVA")]
    public virtual TipTva TipTva { get; set; }
    // A doua valoare de postare, cu destinație fixă (conturile de TVA din
    // TipTva + PoliticaTva); 0 la regimurile care nu postează separat.
    [XafDisplayName("Valoare TVA")]
    public virtual decimal ValoareTva { get; set; }

    // Ancoră spre execuția bugetară (modul separat) — testul bazei §7.1.
    public virtual Guid? AngajamentId { get; set; }
    public virtual Angajament Angajament { get; set; }

    // DIM-2 (decizia 54c): dimensiunile sunt caracteristică de FRUNZĂ — baza nu
    // culege nimic, contractul întoarce value object-ul gol. Frunzele care culeg
    // (FCT, FCL, LDI, DEC, NTC, NIR, DSC, trezoreria) fac override pe pereche;
    // motorul consumă doar contractul (valoarea, nu coloana).
    public virtual Dimensiuni DimensiuniCulese() => new();

    // Perechea de scriere — folosită DOAR de clonările motorului (conexul,
    // plata autogenerată, descărcarea): copiere frunză→frunză prin contract.
    public virtual void PreiaDimensiuni(Dimensiuni sursa) { }

    // Decizia 25c: lotul se naște LA CULEGERE pe linia de intrare (NIR manual,
    // FacturaIntrare pentru lanțul conex, plus de inventar, producție) — baza nu
    // poartă ProdusId, deci produsul ales intră direct pe Lot. Motorul îl
    // finalizează la operare (PretUnitar = Valoare/Cantitate, Data, atribute).
    public Lot CreeazaLot(DevExpress.ExpressApp.IObjectSpace os, Produs produs, Gestiune gestiune) {
        var lot = os.CreateObject<Lot>();
        lot.Produs = produs;
        lot.Gestiune = gestiune;
        lot.LinieIntrareId = ID;
        Lot = lot;
        return lot;
    }
}
