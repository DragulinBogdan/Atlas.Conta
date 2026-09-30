using System.ComponentModel.DataAnnotations.Schema;
using Atlas.Conta.BackOffice.Module.UI;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// FCT IESIRE (07): pur creanță (411 = 7xx), fără registru de stoc; numerotare
// proprie (serie fiscală) prin politică; scadența are default de politică (+30).
// Layout-ul DetailView-ului: declarat în `ContaUiBaseline` cu `.Layout(...)`
// (GATE XAF D12), grupul propriu nested în containerul `Antet`.
[TipDetaliu(typeof(FacturaIesireDetaliu))]
[GardContare(NaturaClasa.Stoc, NivelContare.TipMaterialExact,
    "Linia de stoc nu are regulă de contare de vânzare pentru Tipul ei — adăugați rândul de politică (sau rulați updater-ul).")]
public class FacturaIesire : Document, IDocumentCuScadenta, IDocumentFiscal {
    [DevExpress.ExpressApp.DC.XafDisplayName("Exigibilitate TVA")]
    public virtual DateOnly? DataExigibilitate { get; set; }

    public override Declaratii.ContractLaturi Laturi() =>
        new(Declaratii.Latura.Interna, Declaratii.Latura.Externa);

    public override Declaratii.IDeclarant Declarant() => Declaratii.DeclarantFacturaIesire.Instanta;

    // Rolul de STINS (F19-D16): factura clientului lasă un sold DEBITOR pe 4111 —
    // se stinge creditând contrapartida (încasarea, jumătatea de credit a notei).
    public override SensStingere? SensDeStins(DevExpress.ExpressApp.IObjectSpace os) =>
        SensStingere.Creanta;

    [XafDisplayName("Scadență")]
    public virtual DateOnly? DataScadenta { get; set; }

    // P2 (design §4): gestiunea din care se descarcă marfa. O singură gestiune
    // per factură la P2 (magazinul online are un depozit); obligatorie doar când
    // există linii de stoc — validarea vine la pasul 2. Descărcarea (DSC conex)
    // se generează din acest header + loturile/produsele liniilor.
    public virtual Guid? GestiuneDescarcareId { get; set; }
    [XafDisplayName("Gestiune de descărcare")]
    public virtual Gestiune GestiuneDescarcare { get; set; }

    public override void PregatesteOperare(DevExpress.ExpressApp.IObjectSpace os) =>
        CalculeazaValori(os, Detalii, pastreazaTvaCules: true);

    // Cele două verificări ieftine scurt-circuitează proiecția resturilor (F4-D4).
    public override void ContribuieRegim(DevExpress.ExpressApp.IObjectSpace os, Api.RegimDocument.Constructor regim) =>
        regim.Decide(Api.RegimDocument.GenereazaDescarcarea,
            !regim.Operat ? "Descărcarea se generează pe o factură operată."
            : GestiuneDescarcareId == null ? "Factura nu are gestiune de descărcare."
            : !Motor.DescarcareService.RestNedescarcat(os, this).Any(r => r.RestNeacoperit > 0)
                ? "Factura nu are rest nedescărcat."
            : null);

    public override bool CuTva() => true;
    public override IReadOnlySet<string> IntrariBaza() => intrariBaza;
    static readonly IReadOnlySet<string> intrariBaza = IntrariBazaCu(nameof(FacturaIesireDetaliu.PretUnitar));

    public override decimal? BazaLinie(DevExpress.ExpressApp.IObjectSpace os, DocumentDetaliu linie) =>
        linie is FacturaIesireDetaliu d ? d.PretUnitar * d.Cantitate : null;

    // Descărcarea de gestiune (P2 §5): la operarea FCL se generează DSC-ul conex
    // (spargere pe loturi din liniile de stoc). Serviciu propriu, NU clona
    // PoliticaConex; motorul îl marchează la fel ca orice copil autogenerat.
    public override Document GenereazaSecundar(DevExpress.ExpressApp.IObjectSpace os) {
        var dsc = Motor.DescarcareService.Genereaza(os, this, Data, DataInregistrare);
        // F27-D4: descărcarea intră în evidență odată cu factura care o naște.
        if (dsc != null)
            dsc.DataInregistrare = DataInregistrare;
        return dsc;
    }

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        // Refuzul liniilor de stoc la BUGETAR (07: facturarea nu descarcă
        // gestiune) trăiește în PoliticaValidare.NaturaInterzisa (30a → 3d); la
        // PRIVAT (P2) liniile de stoc sunt permise și dictează descărcarea.
        // Fără cerință de clasificație bugetară: veniturile sunt exceptate de la
        // obligativitatea angajamentului (regula hardcodată legacy, 00 §10) —
        // FCL pur și simplu nu are rând de politică cu CereClasificatieBugetara.
        foreach (var d in Detalii)
            if (d.Cantitate <= 0)
                erori.Add("Cantitatea fiecărei linii de factură trebuie să fie pozitivă.");

        // Liniile FCL se culeg pe tipul derivat — o linie de bază DocumentDetaliu
        // ar ocoli complet General!+Specific? (review P2 defect 7): fără produs,
        // fără descărcare, fără rest urmăribil.
        foreach (var d in Detalii)
            if (d is not FacturaIesireDetaliu)
                erori.Add("Linia facturii de ieșire trebuie culeasă ca linie de factură de ieșire, nu ca detaliu generic.");

        // P2 (design §4): culegerea de stoc — General! (produsul e identitatea
        // liniei) + Specific? (lotul e pinul opțional). Totul pe proiecții (25b).
        var claseTip = Motor.Fapte.ClaseTip(os, Detalii.Select(d => d.TipMaterialId));
        var liniiStoc = Detalii.OfType<FacturaIesireDetaliu>()
            .Where(d => claseTip.GetValueOrDefault(d.TipMaterialId).Natura == NaturaClasa.Stoc)
            .ToList();

        foreach (var d in liniiStoc)
            if (d.ProdusId == null)
                erori.Add("Linia de stoc a facturii de ieșire cere produsul (identitatea liniei) — alegeți-l.");
        if (liniiStoc.Count > 0 && GestiuneDescarcareId == null)
            erori.Add("Factura de ieșire cu linii de stoc cere gestiunea de descărcare.");

        if (liniiStoc.Count > 0) {
            // Identitatea dublă a liniei (Tip + Produs) trebuie să fie coerentă:
            // un produs de alt Tip ar conta pe conturile Tipului greșit deși
            // stocul se mișcă pe lotul produsului (review P2 defect 4).
            var idsProdus = liniiStoc.Where(d => d.ProdusId != null)
                .Select(d => d.ProdusId.Value).Distinct().ToList();
            var tipPerProdus = os.GetObjectsQuery<Produs>()
                .Where(p => idsProdus.Contains(p.ID))
                .Select(p => new { p.ID, p.TipMaterialId })
                .ToDictionary(p => p.ID, p => p.TipMaterialId);
            foreach (var d in liniiStoc)
                if (d.ProdusId != null && tipPerProdus.TryGetValue(d.ProdusId.Value, out var tipProdus)
                        && tipProdus != null && tipProdus != d.TipMaterialId)
                    erori.Add("Produsul liniei de stoc aparține altui Tip decât Tipul liniei — corectați Tipul sau produsul.");
        }

        // Pin-urile (LotId cules): lotul aparține produsului liniei; iar cu DSC
        // activ (reguli de stoc DSC — profilul privat), lotul are sold în
        // gestiunea de descărcare (altfel: întâi transfer BTR). Fără reguli DSC
        // (bugetar) verificarea de sold se sare — liniile de stoc sunt oricum
        // refuzate declarativ acolo.
        var pinuri = liniiStoc.Where(d => d.LotId != null).ToList();
        if (pinuri.Count > 0) {
            var idsLotPin = pinuri.Select(d => d.LotId.Value).Distinct().ToList();
            var produsPerLot = os.GetObjectsQuery<Lot>()
                .Where(l => idsLotPin.Contains(l.ID))
                .Select(l => new { l.ID, l.ProdusId })
                .ToDictionary(l => l.ID, l => l.ProdusId);

            var tipDsc = Motor.MotorOperare.GasesteTipDocument(os, nameof(DescarcareGestiune));
            var reguliDsc = Motor.Fapte.ReguliStoc(os, tipDsc.ID)
                .Where(r => r.Latura == LaturaDocument.Predator && r.Semn < 0)
                .ToList();
            var conturi = reguliDsc.Count > 0 && GestiuneDescarcareId != null
                ? Motor.DescarcareService.ConturiStoc(os, this) : new Dictionary<Guid, Guid>();
            var disponibile = Cub.Citiri.Loturi.Cumulate(os, Cub.Citiri.CitireCumul.Integrala, DataInregistrare)
                .Where(s => idsLotPin.Contains(s.LotId) && s.GestiuneId == GestiuneDescarcareId
                    && s.Cantitate > 0m).ToList();

            foreach (var d in pinuri) {
                var lotId = d.LotId.Value;
                if (d.ProdusId != null && produsPerLot.TryGetValue(lotId, out var prodLot) && prodLot != d.ProdusId)
                    erori.Add("Lotul ales pe linia de stoc nu aparține produsului liniei.");
                if (reguliDsc.Count == 0 || GestiuneDescarcareId == null)
                    continue;
                var potrivit = Motor.Potrivire.Stoc(reguliDsc, Motor.Fapte.Linie(d, claseTip))
                    .FirstOrDefault(p => p.Latura == LaturaDocument.Predator);
                if (potrivit is { Reguli.Count: > 0 } && !disponibile.Any(s =>
                        s.LotId == lotId && s.ProdusId == d.ProdusId && s.ContId == conturi[d.ID]))
                    erori.Add($"Lotul ales nu are sold în gestiunea de descărcare — întâi transfer (BTR).");
            }
        }
    }
}

public class FacturaIesireDetaliu : DocumentDetaliu, ILinieCuPretUnitar, ILinieCuAvans {
    public virtual Guid? LinieAvansId { get; set; }
    [XafDisplayName("Linia avansului"), DataSourceProperty(nameof(AvansuriDisponibile))]
    public virtual DocumentDetaliu LinieAvans { get; set; }
    [NotMapped, System.ComponentModel.Browsable(false)]
    public IEnumerable<DocumentDetaliu> AvansuriDisponibile => Culegere.AvansuriCulegere.Candidati(ObjectSpace, Document);

    public virtual string Descriere { get; set; }
    // Familia LIVRARE, un singur set de valori (07) — fără dubla familie legacy.
    // Cota și regimul vin din TipTva (bază, P1).
    [XafDisplayName("Preț unitar")]
    public virtual decimal PretUnitar { get; set; }

    // P2 (design §4): identitatea liniei de stoc e PRODUSUL (poziția din site ↔
    // produs) — cheia pickingului la culegere/generare. Testul apartenenței
    // (decizia 2): produsul nu apare în formule de stoc/reguli contabile (stocul
    // lucrează pe Lot) ⇒ derivată, nu bază. Schema rămâne nullable (aceeași
    // derivată poartă și liniile de servicii); obligatoriu pe liniile de stoc
    // prin validare — pasul 2. LotId de pe bază = rafinarea specifică opțională
    // (pin), prioritară la picking.
    public virtual Guid? ProdusId { get; set; }
    // Catalog de produse (potențial mare): lookup standard (SmartLookup revertat,
    // decizia 40d/gate).
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    public virtual Produs Produs { get; set; }

    // F23-D2 — produsul cules, prin contractul bazei (`ProdusCules`), ca
    // implicitul de TVA să nu întrebe frunza prin `is` sau prin reflecție.
    public override Guid? ProdusCules() => ProdusId;

    [NotMapped]
    [XafDisplayName("Valoare livrare")]
    public decimal ValoareLivrare => PretUnitar * Cantitate;

    // DIM-2 (decizia 54c, inventar §2): dimensiunea culeasă pe linia FCL
    // (veniturile 751/750 cer E la bugetar); DSC o primește prin clonă.
    public virtual Guid? CodEconomicId { get; set; }
    [XafDisplayName("Cod economic")]
    public virtual CodEconomic CodEconomic { get; set; }

    public override Dimensiuni DimensiuniCulese() => new() { CodEconomicId = CodEconomicId };
    public override void PreiaDimensiuni(Dimensiuni s) => CodEconomicId = s.CodEconomicId;
}
