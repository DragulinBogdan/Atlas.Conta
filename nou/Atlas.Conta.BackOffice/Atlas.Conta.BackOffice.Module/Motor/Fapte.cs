using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Motor;

// Singura ortografie a mapării entitate → fapt (F24-D5): impură (citește), fără
// nicio decizie — deciziile stau în `Potrivire`. Tot ce se citește aici e pe
// FK-uri și proiecții, fără navigații lazy (25b).
internal static class Fapte {
    public static List<RegulaContareFapt> ReguliContare(IObjectSpace os, Guid tipDocumentId) =>
        os.GetObjectsQuery<RegulaContare>()
            .Where(r => r.TipDocumentId == tipDocumentId)
            .ToList()
            .Select(r => new RegulaContareFapt(r.ID, r.TipMaterialId, r.NaturaFiltru, r.SemnFiltru,
                r.PastreazaSemn, r.SursaContDebit, r.ContDebitId, r.SursaContCredit, r.ContCreditId,
                r.DinSeed, r.DimensiuniComun(), r.DimensiuniOverrideDebit(), r.DimensiuniOverrideCredit()))
            .ToList();

    public static List<RegulaStocFapt> ReguliStoc(IObjectSpace os, Guid tipDocumentId) =>
        os.GetObjectsQuery<RegulaStoc>()
            .Where(r => r.TipDocumentId == tipDocumentId)
            .Select(r => new { r.ID, r.Latura, r.ClasaId, r.TipStoc, r.Semn, r.DinSeed })
            .ToList()
            .Select(r => new RegulaStocFapt(r.ID, r.Latura, r.ClasaId, r.TipStoc, r.Semn, r.DinSeed))
            .ToList();

    public static PoliticaConexFapt? Conex(IObjectSpace os, Guid tipDocumentId) {
        var politica = os.FirstOrDefault<PoliticaConex>(p => p.TipDocumentSursaId == tipDocumentId);
        return politica == null
            ? null
            : new PoliticaConexFapt(politica.ID, politica.TipDocumentTintaId,
                politica.InverseazaLaturi, politica.NaturaFiltru);
    }

    public static List<PoliticaTvaImplicitFapt> PoliticiTvaImplicit(IObjectSpace os, Guid tipDocumentId) =>
        os.GetObjectsQuery<PoliticaTvaImplicit>()
            .Where(r => r.TipDocumentId == tipDocumentId)
            .Select(r => new { r.ID, r.ClasaFiscala, r.ValabilDeLa, r.TipTvaId, r.DinSeed })
            .ToList()
            .Select(r => new PoliticaTvaImplicitFapt(r.ID, r.ClasaFiscala, r.ValabilDeLa, r.TipTvaId, r.DinSeed))
            .ToList();

    public static Dictionary<Guid, TipTvaFapt> TipuriTva(IObjectSpace os, IReadOnlyList<Guid> ids) =>
        ids.Count == 0
            ? []
            : os.GetObjectsQuery<TipTva>()
                .Where(t => ids.Contains(t.ID))
                .Select(t => new TipTvaFapt(t.ID, t.Cod, t.Regim, t.Activ))
                .ToDictionary(t => t.Id);

    public static Dictionary<Guid, (Guid ClasaId, NaturaClasa Natura, string Denumire, Guid? ContImplicitId)>
        ClaseTip(IObjectSpace os, IEnumerable<Guid> idsTip) {
        var ids = idsTip.Distinct().ToList();
        return os.GetObjectsQuery<TipMaterial>()
            .Where(t => ids.Contains(t.ID))
            .Select(t => new { t.ID, t.ClasaId, t.Clasa.Natura, t.Denumire, t.ContImplicitId })
            .ToDictionary(t => t.ID, t => (t.ClasaId, t.Natura, t.Denumire, t.ContImplicitId));
    }

    public static LinieFapt Linie(DocumentDetaliu d,
            IReadOnlyDictionary<Guid, (Guid ClasaId, NaturaClasa Natura, string Denumire, Guid? ContImplicitId)> claseTip) {
        var gasit = claseTip.TryGetValue(d.TipMaterialId, out var info);
        return new LinieFapt(d.TipMaterialId, gasit ? info.ClasaId : null, gasit ? info.Natura : null,
            Math.Sign(d.Cantitate), d.LotId, gasit ? info.ContImplicitId : null);
    }

    public static LaturiFapt Laturi(Repartitor predator, Repartitor primitor) =>
        new(predator?.ContImplicitId, primitor?.ContImplicitId);
}
