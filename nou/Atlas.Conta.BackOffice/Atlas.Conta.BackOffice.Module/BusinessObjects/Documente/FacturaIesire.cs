using System.ComponentModel.DataAnnotations.Schema;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// FCT IESIRE (07): pur creanță (411 = 7xx), fără registru de stoc; numerotare
// proprie (serie fiscală) prin politică; scadența are default de politică (+30).
public class FacturaIesire : Document, IDocumentCuScadenta {
    public virtual DateOnly? DataScadenta { get; set; }

    // P2 (design §4): gestiunea din care se descarcă marfa. O singură gestiune
    // per factură la P2 (magazinul online are un depozit); obligatorie doar când
    // există linii de stoc — validarea vine la pasul 2. Descărcarea (DSC conex)
    // se generează din acest header + loturile/produsele liniilor.
    public virtual Guid? GestiuneDescarcareId { get; set; }
    public virtual Gestiune GestiuneDescarcare { get; set; }

    // Pe FCL TVA-ul se CALCULEAZĂ (spre deosebire de FCT, unde valoarea culeasă
    // de pe factura furnizorului bate rotunjirea) — design §3.
    public override void PregatesteOperare(DevExpress.ExpressApp.IObjectSpace os) {
        var tipuri = Motor.TvaService.IncarcaTipuri(os, Detalii);
        foreach (var d in Detalii.OfType<FacturaIesireDetaliu>())
            Motor.TvaService.CalculeazaValori(d, d.PretUnitar * d.Cantitate, tipuri);
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

public class FacturaIesireDetaliu : DocumentDetaliu {
    public virtual string Descriere { get; set; }
    // Familia LIVRARE, un singur set de valori (07) — fără dubla familie legacy.
    // Cota și regimul vin din TipTva (bază, P1).
    public virtual decimal PretUnitar { get; set; }

    // P2 (design §4): identitatea liniei de stoc e PRODUSUL (poziția din site ↔
    // produs) — cheia pickingului la culegere/generare. Testul apartenenței
    // (decizia 2): produsul nu apare în formule de stoc/reguli contabile (stocul
    // lucrează pe Lot) ⇒ derivată, nu bază. Schema rămâne nullable (aceeași
    // derivată poartă și liniile de servicii); obligatoriu pe liniile de stoc
    // prin validare — pasul 2. LotId de pe bază = rafinarea specifică opțională
    // (pin), prioritară la picking.
    public virtual Guid? ProdusId { get; set; }
    public virtual Produs Produs { get; set; }

    [NotMapped] public decimal ValoareLivrare => PretUnitar * Cantitate;
}
