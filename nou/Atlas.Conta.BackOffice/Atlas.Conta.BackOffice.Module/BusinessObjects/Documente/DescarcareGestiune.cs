using Atlas.Conta.BackOffice.Module.UI;
using DevExpress.ExpressApp.DC;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// DSC (design P2 §3): descărcarea de gestiune — marfa IESE din patrimoniu
// (−1 pe predator, fără registru pereche; ≠ BonConsum, unde consumul rămâne pe
// responsabil, +Consum pe primitor). Predator = gestiunea de descărcare,
// primitor = clientul (partenerul de pe FCL) — marfa pleacă fizic la client.
// De regulă autogenerat din FacturaIesire (conex, DescarcareService), dar DSC-ul
// cules MANUAL rămâne legal (document normal de ieșire din gestiune).
[TipDetaliu(typeof(DescarcareGestiuneDetaliu))]
public class DescarcareGestiune : Document {
    // Ambele dimensiuni rămân pe gestiune (predatorul) — precedentul Decont 32c:
    // soldul 371/345 se ține per gestiune; clientul trăiește pe rândurile FCL și
    // pe link-ul DocumentSursa, nu pe dimensiunile registrului.
    public override Guid RepartitorImplicitCredit() => PredatorId;

    // Valoare = COST (preț lot × cantitate — pattern BTR/BCS, prețul nu se culege).
    // ValoareTva 0: TVA-ul vânzării e integral pe FCL (P1), DSC nu poartă TVA.
    public override void PregatesteOperare(DevExpress.ExpressApp.IObjectSpace os) {
        foreach (var d in Detalii.Where(d => d.LotId != null)) {
            var lot = os.GetObjectByKey<Lot>(d.LotId.Value);
            d.Valoare = Scara.RotunjesteBani(d.Cantitate * lot.PretUnitar);
            d.ValoareTva = 0;
        }
    }

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        if (os.GetObjectByKey<Repartitor>(PredatorId) is not Gestiune)
            erori.Add("Predatorul descărcării de gestiune trebuie să fie o gestiune.");
        // Fără validare pe primitor: DSC-ul cules manual e legal cu orice primitor
        // (clientul se materializează pe FCL, nu ține de ieșirea din gestiune).
        foreach (var d in Detalii) {
            if (d is not DescarcareGestiuneDetaliu)
                erori.Add("Linia descărcării trebuie culeasă ca linie de descărcare, nu ca detaliu generic.");
            if (d.LotId == null)
                erori.Add("Fiecare linie de descărcare referă un lot (ieșirea e pe lot — decizia 13).");
            if (d.Cantitate <= 0)
                erori.Add("Cantitatea descărcată trebuie să fie pozitivă.");
        }

        // Fără regulă de contare de cost per Tip, linia ar mișca stocul fără să
        // posteze NIMIC în contabilitate (motorul sare linia fără regulă) —
        // refuz explicit (review P2 defect 1). Coerența Tip ↔ produsul lotului:
        // altfel costul contează pe conturile Tipului greșit (defect 4).
        var tipDsc = Motor.MotorOperare.GasesteTipDocument(os, this);
        var tipuriCuRegula = os.GetObjectsQuery<RegulaContare>()
            .Where(r => r.TipDocumentId == tipDsc.ID && r.TipMaterialId != null)
            .Select(r => r.TipMaterialId.Value).ToList();
        var idsLot = Detalii.Where(d => d.LotId != null).Select(d => d.LotId.Value).Distinct().ToList();
        var infoLot = os.GetObjectsQuery<Lot>()
            .Where(l => idsLot.Contains(l.ID))
            .Select(l => new { l.ID, l.ProdusId, l.Produs.TipMaterialId })
            .ToDictionary(l => l.ID, l => (l.ProdusId, l.TipMaterialId));
        foreach (var d in Detalii) {
            if (!tipuriCuRegula.Contains(d.TipMaterialId))
                erori.Add("Linia descărcării nu are regulă de contare de cost pentru Tipul ei (6xx = cont de stoc) — adăugați rândul de politică (sau rulați updater-ul).");
            if (d.LotId != null && infoLot.TryGetValue(d.LotId.Value, out var lot)
                    && lot.TipMaterialId != null && lot.TipMaterialId != d.TipMaterialId)
                erori.Add("Lotul liniei aparține unui produs cu alt Tip decât Tipul liniei.");
        }

        // Integritatea trasabilității (review P2 defect 2): LinieSursa e liberă
        // ca FK — validăm la operare că referă o linie a facturii-sursă a
        // ACESTUI document (altfel un DSC străin ar otrăvi acoperirea altei FCL)
        // și că lotul descărcat aparține produsului liniei-sursă.
        var liniiCuSursa = Detalii.OfType<DescarcareGestiuneDetaliu>()
            .Where(d => d.LinieSursaId != null).ToList();
        if (liniiCuSursa.Count > 0) {
            if (DocumentSursaId == null)
                erori.Add("Liniile cu linie-sursă cer documentul-sursă (descărcarea acoperă o factură de ieșire).");
            else {
                var idsSursa = liniiCuSursa.Select(d => d.LinieSursaId.Value).Distinct().ToList();
                var surse = os.GetObjectsQuery<FacturaIesireDetaliu>()
                    .Where(s => idsSursa.Contains(s.ID))
                    .Select(s => new { s.ID, s.DocumentId, s.ProdusId })
                    .ToDictionary(s => s.ID, s => (s.DocumentId, s.ProdusId));
                foreach (var d in liniiCuSursa) {
                    if (!surse.TryGetValue(d.LinieSursaId.Value, out var sursa) || sursa.DocumentId != DocumentSursaId)
                        erori.Add("Linia-sursă a descărcării trebuie să fie o linie a facturii-sursă a documentului.");
                    else if (d.LotId != null && infoLot.TryGetValue(d.LotId.Value, out var lot2)
                            && sursa.ProdusId != null && lot2.ProdusId != sursa.ProdusId)
                        erori.Add("Lotul liniei de descărcare nu aparține produsului liniei-sursă.");
                }
            }
        }
    }
}

public class DescarcareGestiuneDetaliu : DocumentDetaliu {
    // Trasabilitatea acoperirii per linie FCL (design §3): FK REAL spre
    // DocumentDetaliu — cross-document (linia sursă e o FacturaIesireDetaliu),
    // deci NU creează ciclul de inserție al sensului linie↔lot. Restul = baza
    // pură (lot, cantitate, valoare).
    public virtual Guid? LinieSursaId { get; set; }
    public virtual DocumentDetaliu LinieSursa { get; set; }

    // DIM-2 (decizia 54c, inventar §2): primită prin clonă din linia FCL sursă.
    public virtual Guid? CodEconomicId { get; set; }
    [XafDisplayName("Cod economic")]
    public virtual CodEconomic CodEconomic { get; set; }

    public override Dimensiuni DimensiuniCulese() => new() { CodEconomicId = CodEconomicId };
    public override void PreiaDimensiuni(Dimensiuni s) => CodEconomicId = s.CodEconomicId;
}
