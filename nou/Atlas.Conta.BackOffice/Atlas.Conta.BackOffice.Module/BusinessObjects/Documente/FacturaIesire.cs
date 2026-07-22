using System.ComponentModel.DataAnnotations.Schema;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// FCT IESIRE (07): pur creanță (411 = 7xx), fără registru de stoc; numerotare
// proprie (serie fiscală) prin politică; scadența are default de politică (+30).
public class FacturaIesire : Document, IDocumentCuScadenta {
    public virtual DateOnly? DataScadenta { get; set; }

    public override void PregatesteOperare(DevExpress.ExpressApp.IObjectSpace os) {
        foreach (var d in Detalii.OfType<FacturaIesireDetaliu>())
            d.RecalculeazaValoare();
    }

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        // Combo-ul viu legacy (defa 47): intern → extern. Emitentul e predator,
        // clientul e primitor — creanța se particularizează prin ContImplicit.
        if (os.GetObjectByKey<Repartitor>(PredatorId) is Partener)
            erori.Add("Predatorul facturii de ieșire este emitentul — un repartitor intern, nu un partener.");
        if (os.GetObjectByKey<Repartitor>(PrimitorId) is not Partener)
            erori.Add("Primitorul facturii de ieșire trebuie să fie un partener (client).");

        var idsTip = Detalii.Select(d => d.TipMaterialId).Distinct().ToList();
        var naturi = os.GetObjectsQuery<TipMaterial>()
            .Where(t => idsTip.Contains(t.ID))
            .Select(t => new { t.ID, t.Clasa.Natura })
            .ToDictionary(t => t.ID, t => t.Natura);
        foreach (var d in Detalii) {
            if (d.Cantitate <= 0)
                erori.Add("Cantitatea fiecărei linii de factură trebuie să fie pozitivă.");
            // Inventar 07: în acest profil facturarea nu descarcă gestiune —
            // vânzarea de bunuri din stoc ar fi document/reguli proprii, de
            // decis la seed când apare nevoia (validarea declarativă la 3d).
            if (naturi.GetValueOrDefault(d.TipMaterialId) == NaturaClasa.Stoc)
                erori.Add("Factura de ieșire nu descarcă gestiune — liniile de stoc nu sunt permise (folosiți tipuri de venit).");
        }
        // Fără cerință de clasificație bugetară: veniturile sunt exceptate de la
        // obligativitatea angajamentului (regula hardcodată legacy, 00 §10).
    }
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
