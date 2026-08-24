using System.ComponentModel.DataAnnotations.Schema;
using Atlas.Conta.BackOffice.Module.UI;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// FCT (01): predator = Partener (furnizor), primitor = Gestiune; nu mișcă stoc —
// intrarea o face NIR-ul conex. Import extern (Tethys) = cale de primă clasă.
//
// Layout-ul DetailView-ului (GATE XAF D12) se declară în `ContaUiBaseline`
// (`.Layout(...)`, grupurile proprii nested în containerul `Antet`).
// `TethysId` (id de import) și grupul CHITANTA_* (câmpuri moarte — 31e) se
// ascund din view-uri prin baseline; rămân în schemă (reactivarea la fluxul BF
// e aditivă), deci nu primesc grup de layout.
[TipDetaliu(typeof(FacturaIntrareDetaliu))]
public class FacturaIntrare : Document, IDocumentCuScadenta, IDocumentCuPV {
    [XafDisplayName("Scadență")]
    public virtual DateOnly? DataScadenta { get; set; }
    [XafDisplayName("Număr PV")]
    public virtual string NumarPV { get; set; }
    [XafDisplayName("Dată PV")]
    public virtual DateOnly? DataPV { get; set; }
    [XafDisplayName("Cod CPV")]
    public virtual string CodCpv { get; set; }
    public virtual string TethysId { get; set; }

    [XafDisplayName("Valută")]
    public virtual string Valuta { get; set; }
    public virtual decimal? Curs { get; set; }

    // Fostul grup DECONT_* — parametrii generării documentului conex Plata;
    // câmpuri persistate (testul bazei §7.3).
    [XafDisplayName("Generează plata")]
    public virtual bool GenereazaPlata { get; set; }
    public virtual Guid? PlataContPropriuId { get; set; }
    [XafDisplayName("Cont propriu (din care se plătește)")]
    public virtual ContPropriu PlataContPropriu { get; set; }
    [XafDisplayName("Număr plată")]
    public virtual string PlataNumar { get; set; }
    [XafDisplayName("Dată plată")]
    public virtual DateOnly? PlataData { get; set; }
    [XafDisplayName("Instrument de plată")]
    public virtual TipInstrumentPlata? PlataTipInstrument { get; set; }
    public virtual bool GenereazaChitanta { get; set; }
    public virtual string ChitantaNumar { get; set; }
    public virtual DateOnly? ChitantaData { get; set; }

    // Lanțul de valori trăiește pe derivată (testul bazei §3): capătul lui
    // (Valoare + ValoareTva, după regimul TipTva — P1) se materializează la
    // operare. TVA-ul cules manual pe linie se păstrează (factura furnizorului
    // bate rotunjirea noastră — design §3).
    public override void PregatesteOperare(DevExpress.ExpressApp.IObjectSpace os) {
        var tipuri = Motor.TvaService.IncarcaTipuri(os, Detalii);
        // Latura fiscală a tipului (F13-D1) — o dată per document, nu per linie.
        var directie = Motor.TvaService.DirectiePentru(os, this);
        foreach (var d in Detalii.OfType<FacturaIntrareDetaliu>())
            Motor.TvaService.CalculeazaValori(d, d.PretUnitar * d.Cantitate, tipuri, directie, pastreazaTvaCules: true);
    }

    // Plata automată (00 §7, decizia 31): grupul DECONT_* cules → draft Plata
    // autogenerat. Header din câmpurile culese; liniile clonează DEFALCAREA
    // fiecărei linii de factură (valoare + dimensiuni + angajament — echivalentul
    // BREG_P/GEST_DEFALCARE_DECONTARI), cu Tipul sursă păstrat ca informație.
    // La operarea plății, motorul creează imperecherea automată pe total.
    // GenereazaChitanta (încasarea-dovadă a BF) rămâne neactivat — nu are cont
    // propriu cules; se tratează la fluxul BF, aditiv.
    public override Document GenereazaSecundar(DevExpress.ExpressApp.IObjectSpace os) {
        if (!GenereazaPlata)
            return null;
        var plata = os.CreateObject<Plata>();
        plata.Data = PlataData ?? Data;
        plata.Numar = PlataNumar;
        plata.TipInstrument = PlataTipInstrument ?? TipInstrumentPlata.OrdinPlata;
        plata.PredatorId = PlataContPropriuId ?? Guid.Empty;
        plata.PrimitorId = PredatorId;
        foreach (var s in Detalii) {
            // DIM-2: defalcarea se naște pe frunza trezoreriei — altfel
            // PreiaDimensiuni ar fi no-op și plata ar pierde dimensiunile.
            var d = os.CreateObject<DocumentTrezorerieDetaliu>();
            d.Document = plata;
            d.TipMaterialId = s.TipMaterialId;
            // Plata stinge BRUTUL (design §3): defalcarea clonată per linie e
            // Valoare + ValoareTva; linia de plată nu are semantică proprie de
            // TVA (TipTva rămâne null).
            d.Valoare = s.Valoare + s.ValoareTva;
            d.AngajamentId = s.AngajamentId;
            d.PreiaDimensiuni(s.DimensiuniCulese());
        }
        return plata;
    }

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        // Numărul facturii e al furnizorului — se culege, nu se generează
        // (FCT nu are politică de numerotare).
        if (string.IsNullOrWhiteSpace(Numar))
            erori.Add("Factura de intrare poartă numărul furnizorului — se completează la culegere.");
        if (GenereazaPlata && PlataContPropriuId == null)
            erori.Add("Generarea plății cere contul propriu (casă/bancă) din care se plătește.");
        if (os.GetObjectByKey<Repartitor>(PredatorId) is not Partener)
            erori.Add("Predatorul facturii de intrare trebuie să fie un partener (furnizor).");
        if (os.GetObjectByKey<Repartitor>(PrimitorId) is not Gestiune)
            erori.Add("Primitorul facturii de intrare trebuie să fie o gestiune.");

        // Liniile FCT se culeg pe tipul derivat — o linie de bază DocumentDetaliu
        // ar ocoli lanțul de valori (fără PretUnitar, Valoare culeasă direct, fără
        // recalcul de TVA) și mecanismul lotului (fără Produs). Oglinda refuzului
        // de pe FCL (review P2 defect 7), devenită necesară odată cu ProdusId pe
        // derivată (GATE XAF D1).
        foreach (var d in Detalii)
            if (d is not FacturaIntrareDetaliu)
                erori.Add("Linia facturii de intrare trebuie culeasă ca linie de factură de intrare, nu ca detaliu generic.");

        var idsTip = Detalii.Select(d => d.TipMaterialId).Distinct().ToList();
        var naturi = os.GetObjectsQuery<TipMaterial>()
            .Where(t => idsTip.Contains(t.ID))
            .Select(t => new { t.ID, t.Clasa.Natura })
            .ToDictionary(t => t.ID, t => t.Natura);
        // Clasificația bugetară per linie a migrat în PoliticaValidare (29b →
        // 3d): regulă de profil, aplicată de motor înaintea acestui hook.
        foreach (var d in Detalii) {
            // Regula hardcodată legacy (00 §10), păstrată: fără cantități negative.
            if (d.Cantitate <= 0)
                erori.Add("Cantitatea fiecărei linii de factură trebuie să fie pozitivă.");
            // Liniile purtătoare de stoc și-au creat lotul la culegere (25c) —
            // el pleacă pe NIR-ul conex, care face singurul +1 în registru.
            if (naturi.GetValueOrDefault(d.TipMaterialId) == NaturaClasa.Stoc && d.LotId == null)
                erori.Add("Liniile de stoc ale facturii își creează lotul la culegere (alegeți produsul).");
        }

        // Identitatea dublă a liniei (Tip + Produs) trebuie să fie coerentă —
        // oglinda validării de pe FCL (review P2 defect 4): un produs de alt Tip
        // ar conta pe conturile Tipului greșit, iar lotul născut de linie ar
        // ajunge în registrul altui Tip decât cel postat. Totul pe proiecții
        // (25b): navigațiile nu se ating în enumerare.
        var idsProdus = Detalii.OfType<FacturaIntrareDetaliu>()
            .Where(d => d.ProdusId != null).Select(d => d.ProdusId.Value).Distinct().ToList();
        if (idsProdus.Count > 0) {
            var tipPerProdus = os.GetObjectsQuery<Produs>()
                .Where(p => idsProdus.Contains(p.ID))
                .Select(p => new { p.ID, p.TipMaterialId })
                .ToDictionary(p => p.ID, p => p.TipMaterialId);
            foreach (var d in Detalii.OfType<FacturaIntrareDetaliu>()) {
                if (d.ProdusId != null && tipPerProdus.TryGetValue(d.ProdusId.Value, out var tipProdus)
                        && tipProdus != null && tipProdus != d.TipMaterialId)
                    erori.Add("Produsul liniei aparține altui Tip decât Tipul liniei — corectați Tipul sau produsul.");
            }
        }
    }
}

public class FacturaIntrareDetaliu : DocumentDetaliu, ILinieCuAtributeLot, ILinieCuPretUnitar, ILinieCareNasteLot {
    // Lanțul de valori trăiește pe derivată (testul bazei §3); capetele lui
    // (Valoare + ValoareTva din bază) intră în registre. Cota și regimul vin
    // din TipTva (bază, P1) — fosta CotaTva de pe derivată era redundantă.
    [XafDisplayName("Preț unitar")]
    public virtual decimal PretUnitar { get; set; }

    [NotMapped]
    [XafDisplayName("Valoare recepție")]
    public decimal ValoareReceptie => PretUnitar * Cantitate;

    [XafDisplayName("Cod CPV")]
    public virtual string CodCpv { get; set; }

    // GATE XAF (D1): produsul liniei de STOC — mecanismul prin care lotul se naște
    // la culegere (decizia 25c: baza nu poartă ProdusId, deci produsul ales de
    // operator intră direct pe Lot prin CreeazaLot). Oglinda lui
    // FacturaIesireDetaliu.ProdusId (37d): identitatea liniei de stoc e produsul;
    // testul apartenenței (decizia 2) îl ține pe derivată — stocul lucrează pe Lot.
    // Schema rămâne nullable (aceeași derivată poartă și liniile de servicii);
    // obligatoriu pe liniile de stoc prin validare.
    public virtual Guid? ProdusId { get; set; }
    // Catalog de produse (potențial mare).
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    public virtual Produs Produs { get; set; }

    // Atribute de lot culese la intrare; motorul le copiază pe Lot la operare.
    [XafDisplayName("Dată expirare")]
    public virtual DateOnly? DataExpirare { get; set; }
    [XafDisplayName("Lot fabricație")]
    public virtual string LotFabricatie { get; set; }

    // DIM-2 (decizia 54c, inventar §2): dimensiunile culese pe linia FCT —
    // FK-uri explicite pe frunză; NIR (clona conexă) și plata autogenerată
    // le primesc prin contractul DimensiuniCulese/PreiaDimensiuni.
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
