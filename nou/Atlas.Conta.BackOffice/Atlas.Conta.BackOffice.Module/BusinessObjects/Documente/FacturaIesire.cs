using System.ComponentModel.DataAnnotations.Schema;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// FCT IESIRE (07): pur creanță (4111 = 7xx), fără registru de stoc; numerotare
// proprie (serie fiscală) prin politică; scadența are default de politică (+30).
public class FacturaIesire : Document, IDocumentCuScadenta {
    public virtual DateOnly? DataScadenta { get; set; }
}

public class FacturaIesireDetaliu : DocumentDetaliu {
    public virtual string Descriere { get; set; }
    // Familia LIVRARE, un singur set de valori (07) — fără dubla familie legacy.
    public virtual decimal PretUnitar { get; set; }
    public virtual decimal CotaTva { get; set; }

    [NotMapped] public decimal PretLivrare => PretUnitar;
    [NotMapped] public decimal ValoareLivrare => PretLivrare * Cantitate;
    [NotMapped] public decimal TvaLivrare => ValoareLivrare * CotaTva / 100m;

    public void RecalculeazaValoare() => Valoare = ValoareLivrare + TvaLivrare;
}
