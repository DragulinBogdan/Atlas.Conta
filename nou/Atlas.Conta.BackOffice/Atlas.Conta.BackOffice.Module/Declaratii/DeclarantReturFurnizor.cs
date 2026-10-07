#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

public sealed class DeclarantReturFurnizor : IDeclarant {
    public static readonly DeclarantReturFurnizor Instanta = new();
    public string SursaValoareDeclarata => SurseValoare.Linie;
    DeclarantReturFurnizor() { }
    public bool ConteazaPrinReguli => true;
    public PoliticaProfil PoliticaCeruta => PoliticaProfil.Contare;

    public N.Declaratie? Declara(Operand operand, N.Rotunjire rotunjire, ICollection<N.Refuz> refuzuri) {
        ArgumentNullException.ThrowIfNull(operand);
        ArgumentNullException.ThrowIfNull(rotunjire);
        ArgumentNullException.ThrowIfNull(refuzuri);
        if (operand.Linii.Count == 0)
            refuzuri.Add(new(CoduriRefuz.LiniiLipsa, "Returul cere cel puțin o linie.", null));
        var contari = new ContareLinie?[operand.Linii.Count];
        var fiscale = new TipTvaFapt?[operand.Linii.Count];
        for (var i = 0; i < operand.Linii.Count; i++) {
            var l = operand.Linii[i];
            if (l.Lot is null)
                refuzuri.Add(new(CoduriRefuz.LotLipsa, "Returul descarcă lotul original.", l.Id));
            if (l.Cantitate == 0m)
                refuzuri.Add(new(CoduriRefuz.CantitateZero, "Cantitatea returnată nu poate fi zero.", l.Id));
            if (l.Lot?.TipMaterialId is Guid tipLot && tipLot != l.TipMaterialId)
                refuzuri.Add(new(CoduriRefuz.ProdusAltTip, "Lotul aparține altui Tip decât linia.", l.Id));
            contari[i] = Contari.Rezolva(operand, l, refuzuri);
            if (operand.PoliticaTva is null || l.TipTvaId is not Guid id) continue;
            if (!operand.TipuriTva.TryGetValue(id, out var tip))
                refuzuri.Add(new(CoduriRefuz.TipTvaLipsa, "Tipul de TVA nu poate fi rezolvat.", l.Id));
            else if (tip.Regim == RegimTva.Capitalizat)
                refuzuri.Add(new(CoduriRefuz.TvaCapitalizat, "TVA capitalizat nu este admis pe retur.", l.Id));
            else fiscale[i] = tip;
        }
        if (refuzuri.Count > 0) return null;
        var taxa = Fiscal.Taxa(operand, fiscale, rotunjire, refuzuri);
        if (refuzuri.Count > 0) return null;
        var doc = operand.Document;
        var miscari = new List<N.Miscare>();
        var decizii = new List<N.Decizie>();
        var partide = new Dictionary<Guid, N.Unitate>();
        for (var i = 0; i < operand.Linii.Count; i++) {
            var l = operand.Linii[i]; var lot = l.Lot!; var contare = contari[i]!.Value;
            Contari.Decide(contare, l.Id, decizii);
            var intern = new N.Capat {
                Cont = contare.ContDebit, Gestiune = doc.Predator.Id, Produs = lot.ProdusId,
                Unitate = new N.Unitate(lot.Id, N.FelUnitate.Lot, contare.ContDebit, null, lot.ProdusId, lot.Data),
                Analiza = Contari.Analiza(l.Analiza, contare.Regula.OverrideDebit, contare.Regula.Comun),
            };
            var tert = new N.Capat {
                Cont = contare.ContCredit, Gestiune = N.GestiuniVirtuale.Furnizor, Produs = lot.ProdusId,
                Analiza = Contari.Analiza(l.Analiza, contare.Regula.OverrideCredit, contare.Regula.Comun),
            };
            if (fiscale[i] is { } tip) intern = Fiscal.CuFapt(operand, intern, tip, N.RolTva.Baza);
            // T-D6: nota fiscală păstrează q × prețul lotului, inclusiv la golire.
            // Nu se aplică Evaluare.Iesire; eventualul reziduu rămâne explicit.
            miscari.Add(new(tert, intern, l.Cantitate, 0m, l.Valoare, new(doc.Id, l.Id)));
            if (l.Cantitate < 0m)
                decizii.Add(new N.ValoareDeclarata(l.Id, intern.Unitate!, -l.Cantitate, -l.Valoare, SurseValoare.Linie));
            Partide.Numeste(operand, intern.Cont, doc.Primitor.Id, l.Id, partide, decizii);
            Partide.Numeste(operand, tert.Cont, doc.Primitor.Id, l.Id, partide, decizii);
            var impozit = Fiscal.Impozitul(operand, l, fiscale[i], taxa, DirectieTva.Deductibil, refuzuri);
            if (impozit is not null) {
                miscari.Add(impozit);
                Partide.Numeste(operand, impozit.DeLa.Cont, doc.Primitor.Id, l.Id, partide, decizii);
                Partide.Numeste(operand, impozit.La.Cont, doc.Primitor.Id, l.Id, partide, decizii);
            }
        }
        if (refuzuri.Count > 0) return null;
        return new(doc.Id, doc.DataInregistrare, [.. miscari.Select(m => m with {
            DeLa = Terti.Capat(operand, m.DeLa, doc.Primitor.Id, partide),
            La = Terti.Capat(operand, m.La, doc.Primitor.Id, partide),
        })], decizii, PoliticiConsumate.Ipoteze(operand, decizii, PoliticiConsumate.Fiscala(operand, fiscale)));
    }
}
