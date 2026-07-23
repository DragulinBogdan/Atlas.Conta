namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// DSC (design P2 §3): descărcarea de gestiune — marfa IESE din patrimoniu
// (−1 pe predator, fără registru pereche; ≠ BonConsum, unde consumul rămâne pe
// responsabil, +Consum pe primitor). Predator = gestiunea de descărcare,
// primitor = clientul (partenerul de pe FCL) — marfa pleacă fizic la client.
// De regulă autogenerat din FacturaIesire (conex, DescarcareService), dar DSC-ul
// cules MANUAL rămâne legal (document normal de ieșire din gestiune).
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
            d.Valoare = d.Cantitate * lot.PretUnitar;
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
            if (d.LotId == null)
                erori.Add("Fiecare linie de descărcare referă un lot (ieșirea e pe lot — decizia 13).");
            if (d.Cantitate <= 0)
                erori.Add("Cantitatea descărcată trebuie să fie pozitivă.");
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
}
