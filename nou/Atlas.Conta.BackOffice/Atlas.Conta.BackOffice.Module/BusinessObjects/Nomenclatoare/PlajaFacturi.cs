using System.ComponentModel.DataAnnotations;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

/// <summary>Plaja de facturi alocată prin decizie internă, declarată în D394 I.2 (E2-D7).</summary>
[NavigationItem("Configurare")]
[XafDefaultProperty(nameof(Serie))]
public class PlajaFacturi : Nomenclator {
    [MaxLength(20)]
    [XafDisplayName("Serie")]
    public virtual string Serie { get; set; }

    [XafDisplayName("Număr inițial")]
    public virtual long NumarInitial { get; set; } = 1;

    [XafDisplayName("Număr final")]
    public virtual long NumarFinal { get; set; }

    [XafDisplayName("Data deciziei")]
    public virtual DateOnly DataDecizie { get; set; }

    /// <summary>Seria fără spații, separatori finali și diferențe de majuscule; cheia de potrivire și de suprapunere.</summary>
    public static string SerieNormalizata(string serie) => (serie ?? "").Trim().TrimEnd(' ', '-', '/', '.', '_').ToUpperInvariant();
}
