using System.ComponentModel.DataAnnotations.Schema;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// FCT IESIRE (07): pur creanță (411 = 7xx), fără registru de stoc; numerotare
// proprie (serie fiscală) prin politică; scadența are default de politică (+30).
public class FacturaIesire : Document, IDocumentCuScadenta {
    public virtual DateOnly? DataScadenta { get; set; }

    // Pe FCL TVA-ul se CALCULEAZĂ (spre deosebire de FCT, unde valoarea culeasă
    // de pe factura furnizorului bate rotunjirea) — design §3.
    public override void PregatesteOperare(DevExpress.ExpressApp.IObjectSpace os) {
        var tipuri = Motor.TvaService.IncarcaTipuri(os, Detalii);
        foreach (var d in Detalii.OfType<FacturaIesireDetaliu>())
            Motor.TvaService.CalculeazaValori(d, d.PretUnitar * d.Cantitate, tipuri);
    }

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        // Combo-ul viu legacy (defa 47): intern → extern. Emitentul e predator,
        // clientul e primitor — creanța se particularizează prin ContImplicit.
        if (os.GetObjectByKey<Repartitor>(PredatorId) is Partener)
            erori.Add("Predatorul facturii de ieșire este emitentul — un repartitor intern, nu un partener.");
        if (os.GetObjectByKey<Repartitor>(PrimitorId) is not Partener)
            erori.Add("Primitorul facturii de ieșire trebuie să fie un partener (client).");

        // Refuzul liniilor de stoc (07: în acest profil facturarea nu descarcă
        // gestiune) a migrat în PoliticaValidare.NaturaInterzisa (30a → 3d).
        // Fără cerință de clasificație bugetară: veniturile sunt exceptate de la
        // obligativitatea angajamentului (regula hardcodată legacy, 00 §10) —
        // FCL pur și simplu nu are rând de politică cu CereClasificatieBugetara.
        foreach (var d in Detalii)
            if (d.Cantitate <= 0)
                erori.Add("Cantitatea fiecărei linii de factură trebuie să fie pozitivă.");
    }
}

public class FacturaIesireDetaliu : DocumentDetaliu {
    public virtual string Descriere { get; set; }
    // Familia LIVRARE, un singur set de valori (07) — fără dubla familie legacy.
    // Cota și regimul vin din TipTva (bază, P1).
    public virtual decimal PretUnitar { get; set; }

    [NotMapped] public decimal ValoareLivrare => PretUnitar * Cantitate;
}
