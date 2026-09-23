#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

public sealed class DeclarantDvi : IDeclarant {
    public static readonly DeclarantDvi Instanta = new();
    DeclarantDvi() { }

    public N.Declaratie? Declara(Operand operand, N.Rotunjire rotunjire, ICollection<N.Refuz> refuzuri) {
        ArgumentNullException.ThrowIfNull(operand);
        ArgumentNullException.ThrowIfNull(rotunjire);
        ArgumentNullException.ThrowIfNull(refuzuri);
        if (operand.Linii.Count == 0)
            refuzuri.Add(new(CoduriRefuz.LiniiLipsa, "Declarația vamală cere cel puțin o linie.", null));
        if (operand.PoliticaTva is null)
            refuzuri.Add(new(CoduriRefuz.PoliticaTvaLipsa, "Declarația vamală cere politică de TVA.", null));
        else if (operand.PoliticaTva.Directie != DirectieTva.Deductibil)
            refuzuri.Add(new(CoduriRefuz.DirectieTvaNepotrivita, "Declarația vamală cere TVA deductibil.", null));
        var tipuri = new TipTvaFapt?[operand.Linii.Count];
        for (var i = 0; i < operand.Linii.Count; i++) {
            var l = operand.Linii[i];
            if (l.Valoare <= 0m)
                refuzuri.Add(new(CoduriRefuz.ValoareNepozitiva, "Baza vamală trebuie să fie pozitivă.", l.Id));
            if (l.TipTvaId is not Guid id || !operand.TipuriTva.TryGetValue(id, out var tip)) {
                refuzuri.Add(new(CoduriRefuz.TipTvaLipsa, "Linia cere un tip de TVA de import.", l.Id));
                continue;
            }
            if (!tip.DeImport || tip.Cota <= 0m || tip.Regim is not (RegimTva.Normal or RegimTva.TaxareInversa))
                refuzuri.Add(new(CoduriRefuz.TvaImportNepotrivit, "Linia cere TVA de import cu cotă pozitivă.", l.Id));
            if (tip.ContTvaDeductibilId is null)
                refuzuri.Add(new(CoduriRefuz.ContTvaLipsa, "Baza vamală cere contul de TVA deductibil.", l.Id));
            tipuri[i] = tip;
        }
        if (refuzuri.Count > 0) return null;
        var taxa = Fiscal.Taxa(operand, tipuri, rotunjire, refuzuri);
        if (refuzuri.Count > 0) return null;
        var doc = operand.Document;
        var miscari = new List<N.Miscare>();
        var decizii = new List<N.Decizie>();
        var partide = new Dictionary<Guid, N.Unitate>();
        for (var i = 0; i < operand.Linii.Count; i++) {
            var l = operand.Linii[i]; var tip = tipuri[i]!.Value;
            var impozit = Fiscal.Impozitul(operand, l, tip, taxa, DirectieTva.Deductibil, refuzuri);
            if (impozit is not null) {
                Partide.Numeste(operand, impozit.DeLa.Cont, doc.Predator.Id, l.Id, partide, decizii);
                Partide.Numeste(operand, impozit.La.Cont, doc.Predator.Id, l.Id, partide, decizii);
                miscari.Add(impozit with {
                    DeLa = Partide.CuPartida(impozit.DeLa, doc.Predator.Id, partide),
                    La = Partide.CuPartida(impozit.La, doc.Predator.Id, partide),
                });
            }
            var ancora = new N.Capat {
                Cont = tip.ContTvaDeductibilId!.Value, Carte = N.Carte.Fiscal,
                Gestiune = Fiscal.GestiuneaInterna(operand), Analiza = Contari.Analiza(l.Analiza, null, null),
            };
            miscari.Add(new(ancora, Fiscal.CuFapt(operand, ancora, tip, N.RolTva.Baza),
                0m, 0m, l.Valoare, new(doc.Id, l.Id)));
        }
        return refuzuri.Count > 0 ? null : new(doc.Id, doc.DataInregistrare, miscari, decizii,
            [operand.PerioadaDeschisa, operand.VersiunePolitica]);
    }
}
