using System.ComponentModel.DataAnnotations.Schema;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// FCT (01): predator = Partener (furnizor), primitor = Gestiune; nu mișcă stoc —
// intrarea o face NIR-ul conex. Import extern (Tethys) = cale de primă clasă.
public class FacturaIntrare : Document, IDocumentCuScadenta, IDocumentCuPV {
    public virtual DateOnly? DataScadenta { get; set; }
    public virtual string NumarPV { get; set; }
    public virtual DateOnly? DataPV { get; set; }
    public virtual string CodCpv { get; set; }
    public virtual string TethysId { get; set; }

    public virtual string Valuta { get; set; }
    public virtual decimal? Curs { get; set; }

    // Fostul grup DECONT_* — parametrii generării documentului conex Plata;
    // câmpuri persistate (testul bazei §7.3).
    public virtual bool GenereazaPlata { get; set; }
    public virtual Guid? PlataContPropriuId { get; set; }
    public virtual ContPropriu PlataContPropriu { get; set; }
    public virtual string PlataNumar { get; set; }
    public virtual DateOnly? PlataData { get; set; }
    public virtual TipInstrumentPlata? PlataTipInstrument { get; set; }
    public virtual bool GenereazaChitanta { get; set; }
    public virtual string ChitantaNumar { get; set; }
    public virtual DateOnly? ChitantaData { get; set; }

    // Lanțul de valori trăiește pe derivată (testul bazei §3): capătul lui
    // (Valoare = VALOARE_RECEPTIE_TVA) se materializează la operare.
    public override void PregatesteOperare(DevExpress.ExpressApp.IObjectSpace os) {
        foreach (var d in Detalii.OfType<FacturaIntrareDetaliu>())
            d.RecalculeazaValoare();
    }

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        // Numărul facturii e al furnizorului — se culege, nu se generează
        // (FCT nu are politică de numerotare).
        if (string.IsNullOrWhiteSpace(Numar))
            erori.Add("Factura de intrare poartă numărul furnizorului — se completează la culegere.");
        if (os.GetObjectByKey<Repartitor>(PredatorId) is not Partener)
            erori.Add("Predatorul facturii de intrare trebuie să fie un partener (furnizor).");
        if (os.GetObjectByKey<Repartitor>(PrimitorId) is not Gestiune)
            erori.Add("Primitorul facturii de intrare trebuie să fie o gestiune.");

        var idsTip = Detalii.Select(d => d.TipMaterialId).Distinct().ToList();
        var naturi = os.GetObjectsQuery<TipMaterial>()
            .Where(t => idsTip.Contains(t.ID))
            .Select(t => new { t.ID, t.Clasa.Natura })
            .ToDictionary(t => t.ID, t => t.Natura);
        foreach (var d in Detalii) {
            // Regula hardcodată legacy (00 §10), păstrată: fără cantități negative.
            if (d.Cantitate <= 0)
                erori.Add("Cantitatea fiecărei linii de factură trebuie să fie pozitivă.");
            // Clasificația bugetară obligatorie (01): angajament SAU cod economic.
            if (d.AngajamentId == null && d.Dimensiuni.CodEconomicId == null)
                erori.Add("Fiecare linie cere clasificație bugetară: angajament sau cod economic.");
            // Liniile purtătoare de stoc și-au creat lotul la culegere (25c) —
            // el pleacă pe NIR-ul conex, care face singurul +1 în registru.
            if (naturi.GetValueOrDefault(d.TipMaterialId) == NaturaClasa.Stoc && d.LotId == null)
                erori.Add("Liniile de stoc ale facturii își creează lotul la culegere (alegeți produsul).");
        }
    }
}

public class FacturaIntrareDetaliu : DocumentDetaliu, ILinieCuAtributeLot {
    // Lanțul de valori trăiește pe derivată (testul bazei §3); doar capătul
    // (Valoare din bază) intră în registre.
    public virtual decimal PretUnitar { get; set; }
    public virtual decimal CotaTva { get; set; }

    [NotMapped] public decimal PretReceptie => PretUnitar;
    [NotMapped] public decimal PretReceptieTva => PretReceptie * (1 + CotaTva / 100m);
    [NotMapped] public decimal ValoareReceptie => PretReceptie * Cantitate;
    [NotMapped] public decimal TvaReceptie => ValoareReceptie * CotaTva / 100m;

    public virtual string CodCpv { get; set; }

    // Atribute de lot culese la intrare; motorul le copiază pe Lot la operare.
    public virtual DateOnly? DataExpirare { get; set; }
    public virtual string LotFabricatie { get; set; }

    public void RecalculeazaValoare() => Valoare = ValoareReceptie + TvaReceptie;
}
