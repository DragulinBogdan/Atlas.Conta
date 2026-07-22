namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Detaliul acestor trei tipuri = baza pură + validare (testul bazei §6).

// NIR (02): singura intrare în stoc a lanțului de cumpărare (+1 primitor);
// liniile lui creează Loturile. De regulă autogenerat din FacturaIntrare (conex).
public class NIR : Document {
}

// BCS (03): −magazie (predator) / +consum (primitor); valoarea vine din lot.
public class BonConsum : Document {
}

// BTR (04): −predator/+primitor pe același tip de stoc; lotul își schimbă
// gestiunea, prețul rămâne al lotului. Primul vertical slice al motorului.
public class NotaTransfer : Document, IDocumentCuPV {
    public virtual string NumarPV { get; set; }
    public virtual DateOnly? DataPV { get; set; }

    // Valoarea liniei = prețul lotului × cantitate (04: formulă fără re-aplicare
    // de TVA — prețul lotului e deja valoarea de registru per unitate).
    public override void PregatesteOperare(DevExpress.ExpressApp.IObjectSpace os) {
        foreach (var d in Detalii.Where(d => d.LotId != null)) {
            var lot = os.GetObjectByKey<Lot>(d.LotId.Value);
            d.Valoare = d.Cantitate * lot.PretUnitar;
        }
    }

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        if (os.GetObjectByKey<Repartitor>(PredatorId) is not Gestiune
            || os.GetObjectByKey<Repartitor>(PrimitorId) is not Gestiune)
            erori.Add("Transferul se face între două gestiuni.");
        else if (PredatorId == PrimitorId)
            erori.Add("Gestiunea sursă și cea destinație trebuie să difere.");
        foreach (var d in Detalii) {
            if (d.LotId == null)
                erori.Add("Fiecare linie de transfer referă un lot.");
            if (d.Cantitate <= 0)
                erori.Add("Cantitatea transferată trebuie să fie pozitivă.");
        }
    }
}
