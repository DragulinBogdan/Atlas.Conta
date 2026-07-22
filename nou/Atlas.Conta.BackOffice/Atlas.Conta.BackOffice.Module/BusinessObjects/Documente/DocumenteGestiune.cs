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
}
