using System.ComponentModel.DataAnnotations.Schema;
using Atlas.Conta.BackOffice.Module.UI;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// FCT IESIRE (07): pur creanță (411 = 7xx), fără registru de stoc; numerotare
// proprie (serie fiscală) prin politică; scadența are default de politică (+30).
// Layout-ul DetailView-ului: `[DetailViewLayout]` (GATE XAF D12), etichetele
// grupurilor în `LayoutDocumenteUpdater`.
[TipDetaliu(typeof(FacturaIesireDetaliu))]
public class FacturaIesire : Document, IDocumentCuScadenta {
    [XafDisplayName("Scadență")]
    [DetailViewLayout(GrupLayout.Livrare, GrupLayout.OrdineLivrare)]
    public virtual DateOnly? DataScadenta { get; set; }

    // P2 (design §4): gestiunea din care se descarcă marfa. O singură gestiune
    // per factură la P2 (magazinul online are un depozit); obligatorie doar când
    // există linii de stoc — validarea vine la pasul 2. Descărcarea (DSC conex)
    // se generează din acest header + loturile/produsele liniilor.
    public virtual Guid? GestiuneDescarcareId { get; set; }
    [XafDisplayName("Gestiune de descărcare")]
    [DetailViewLayout(GrupLayout.Livrare, GrupLayout.OrdineLivrare)]
    public virtual Gestiune GestiuneDescarcare { get; set; }

    // TVA-ul se calculează din cotă, DAR o `ValoareTva` nenulă culeasă se
    // păstrează — regula 36a, uniformizată pe FCT/FCL/DEC (decizia 48b): pe
    // facturarea proprie rotunjirea aparține documentului emis (e-Factura,
    // agregarea retailului), nu recalculului nostru.
    public override void PregatesteOperare(DevExpress.ExpressApp.IObjectSpace os) {
        var tipuri = Motor.TvaService.IncarcaTipuri(os, Detalii);
        foreach (var d in Detalii.OfType<FacturaIesireDetaliu>())
            Motor.TvaService.CalculeazaValori(d, d.PretUnitar * d.Cantitate, tipuri, pastreazaTvaCules: true);
    }

    // Descărcarea de gestiune (P2 §5): la operarea FCL se generează DSC-ul conex
    // (spargere pe loturi din liniile de stoc). Serviciu propriu, NU clona
    // PoliticaConex; motorul îl marchează la fel ca orice copil autogenerat.
    public override Document GenereazaSecundar(DevExpress.ExpressApp.IObjectSpace os) =>
        Motor.DescarcareService.Genereaza(os, this, Data);

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        // Combo-ul viu legacy (defa 47): intern → extern. Emitentul e predator,
        // clientul e primitor — creanța se particularizează prin ContImplicit.
        if (os.GetObjectByKey<Repartitor>(PredatorId) is Partener)
            erori.Add("Predatorul facturii de ieșire este emitentul — un repartitor intern, nu un partener.");
        if (os.GetObjectByKey<Repartitor>(PrimitorId) is not Partener)
            erori.Add("Primitorul facturii de ieșire trebuie să fie un partener (client).");

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
        var idsTip = Detalii.Select(d => d.TipMaterialId).Distinct().ToList();
        var infoTip = os.GetObjectsQuery<TipMaterial>()
            .Where(t => idsTip.Contains(t.ID))
            .Select(t => new { t.ID, t.ClasaId, t.Clasa.Natura })
            .ToDictionary(t => t.ID, t => (t.ClasaId, t.Natura));
        var liniiStoc = Detalii.OfType<FacturaIesireDetaliu>()
            .Where(d => infoTip.GetValueOrDefault(d.TipMaterialId).Natura == NaturaClasa.Stoc)
            .ToList();

        foreach (var d in liniiStoc)
            if (d.ProdusId == null)
                erori.Add("Linia de stoc a facturii de ieșire cere produsul (identitatea liniei) — alegeți-l.");
        if (liniiStoc.Count > 0 && GestiuneDescarcareId == null)
            erori.Add("Factura de ieșire cu linii de stoc cere gestiunea de descărcare.");

        if (liniiStoc.Count > 0) {
            // Fără regulă de contare per Tip pe FCL, linia de stoc ar cădea pe
            // genericul de servicii și ar posta creditul pe contul de STOC al
            // Tipului la preț de vânzare — exact bug-ul corectat de derivarea de
            // vânzare (design §6). Un Tip nou creat între updater-e nu are încă
            // rândul derivat: refuz explicit, nu postare greșită silențioasă
            // (filozofia 30b — fără fallback = eroare clară; review P2 defect 1).
            var tipFcl = Motor.MotorOperare.GasesteTipDocument(os, this);
            var tipuriCuRegula = os.GetObjectsQuery<RegulaContare>()
                .Where(r => r.TipDocumentId == tipFcl.ID && r.TipMaterialId != null)
                .Select(r => r.TipMaterialId.Value).ToList();
            // Identitatea dublă a liniei (Tip + Produs) trebuie să fie coerentă:
            // un produs de alt Tip ar conta pe conturile Tipului greșit deși
            // stocul se mișcă pe lotul produsului (review P2 defect 4).
            var idsProdus = liniiStoc.Where(d => d.ProdusId != null)
                .Select(d => d.ProdusId.Value).Distinct().ToList();
            var tipPerProdus = os.GetObjectsQuery<Produs>()
                .Where(p => idsProdus.Contains(p.ID))
                .Select(p => new { p.ID, p.TipMaterialId })
                .ToDictionary(p => p.ID, p => p.TipMaterialId);
            foreach (var d in liniiStoc) {
                if (!tipuriCuRegula.Contains(d.TipMaterialId))
                    erori.Add("Linia de stoc nu are regulă de contare de vânzare pentru Tipul ei — adăugați rândul de politică (sau rulați updater-ul).");
                if (d.ProdusId != null && tipPerProdus.TryGetValue(d.ProdusId.Value, out var tipProdus)
                        && tipProdus != null && tipProdus != d.TipMaterialId)
                    erori.Add("Produsul liniei de stoc aparține altui Tip decât Tipul liniei — corectați Tipul sau produsul.");
            }
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
            var reguliDsc = os.GetObjectsQuery<RegulaStoc>()
                .Where(r => r.TipDocumentId == tipDsc.ID && r.Latura == LaturaDocument.Predator && r.Semn < 0)
                .ToList();

            foreach (var d in pinuri) {
                var lotId = d.LotId.Value;
                if (d.ProdusId != null && produsPerLot.TryGetValue(lotId, out var prodLot) && prodLot != d.ProdusId)
                    erori.Add("Lotul ales pe linia de stoc nu aparține produsului liniei.");
                if (reguliDsc.Count == 0 || GestiuneDescarcareId == null)
                    continue;
                var tipStoc = Motor.DescarcareService.TipStocPentruClasa(
                    reguliDsc, infoTip.GetValueOrDefault(d.TipMaterialId).ClasaId);
                if (tipStoc != null && Motor.StocService.Sold(os,
                        new Motor.CheieStoc(lotId, GestiuneDescarcareId.Value, tipStoc.Value), Data) <= 0)
                    erori.Add($"Lotul ales nu are sold în gestiunea de descărcare — întâi transfer (BTR).");
            }
        }
    }
}

public class FacturaIesireDetaliu : DocumentDetaliu, ILinieCuPretUnitar {
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

    [NotMapped]
    [XafDisplayName("Valoare livrare")]
    public decimal ValoareLivrare => PretUnitar * Cantitate;
}
