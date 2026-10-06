#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

public sealed class DeclarantReturClient : IDeclarant {
    public static readonly DeclarantReturClient Instanta = new();
    public string SursaValoareDeclarata => SurseValoare.Linie;
    DeclarantReturClient() { }
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
            var cost = l.LotId != null;
            if (l.Natura != (cost ? NaturaClasa.Stoc : NaturaClasa.Serviciu))
                refuzuri.Add(new(CoduriRefuz.NaturaNepotrivita, "Returul cere Tip de stoc pe cost și Tip de venit pe linia fără lot.", l.Id));
            if (cost && l.Lot is null)
                refuzuri.Add(new(CoduriRefuz.LotLipsa, "Costul returnat cere lotul original.", l.Id));
            if (cost && l.Cantitate == 0m)
                refuzuri.Add(new(CoduriRefuz.CantitateZero, "Cantitatea returnată nu poate fi zero.", l.Id));
            if (!cost && l.Valoare == 0m)
                refuzuri.Add(new(CoduriRefuz.ValoareZero, "Venitul returnat cere o valoare nenulă.", l.Id));
            if (l.Lot?.TipMaterialId is Guid tipLot && tipLot != l.TipMaterialId)
                refuzuri.Add(new(CoduriRefuz.ProdusAltTip, "Lotul aparține altui Tip decât linia.", l.Id));
            contari[i] = Contari.Rezolva(operand, l, refuzuri);
            if (cost || operand.PoliticaTva is null || l.TipTvaId is not Guid id) continue;
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
            var l = operand.Linii[i]; var contare = contari[i]!.Value;
            decizii.Add(new N.ContRezolvat(l.Id, contare.ContDebit, contare.SursaDebit.ToString()));
            decizii.Add(new N.ContRezolvat(l.Id, contare.ContCredit, contare.SursaCredit.ToString()));
            var credit = new N.Capat {
                Cont = contare.ContCredit, Gestiune = doc.Primitor.Id,
                Produs = l.Lot?.ProdusId,
                Analiza = Contari.Analiza(l.Analiza, contare.Regula.OverrideCredit, contare.Regula.Comun),
            };
            var debit = new N.Capat {
                Cont = contare.ContDebit, Produs = l.Lot?.ProdusId,
                Analiza = Contari.Analiza(l.Analiza, contare.Regula.OverrideDebit, contare.Regula.Comun),
            };
            if (l.Lot is { } lot) {
                credit = credit with { Unitate = new N.Unitate(lot.Id, N.FelUnitate.Lot,
                    contare.ContCredit, null, lot.ProdusId, lot.Data) };
                debit = debit with { Gestiune = N.GestiuniVirtuale.Client, Partener = doc.Predator.Id };
                miscari.Add(new(credit, debit, l.Cantitate, 0m, l.Valoare, new(doc.Id, l.Id)));
                if (l.Cantitate > 0m)
                    decizii.Add(new N.ValoareDeclarata(l.Id, credit.Unitate!, l.Cantitate, l.Valoare, SurseValoare.Linie));
            }
            else {
                if (fiscale[i] is { } tip) credit = Fiscal.CuFapt(operand, credit, tip, N.RolTva.Baza);
                miscari.Add(new(credit, debit, 0m, 0m, l.Valoare, new(doc.Id, l.Id)));
                Partide.Numeste(operand, debit.Cont, doc.Predator.Id, l.Id, partide, decizii);
                var impozit = Fiscal.Impozitul(operand, l, fiscale[i], taxa, DirectieTva.Colectat, refuzuri);
                if (impozit is not null) {
                    miscari.Add(impozit);
                    Partide.Numeste(operand, impozit.La.Cont, doc.Predator.Id, l.Id, partide, decizii);
                }
            }
        }
        if (refuzuri.Count > 0) return null;
        return new(doc.Id, doc.DataInregistrare, [.. miscari.Select(m => m with {
            DeLa = Partide.CuPartida(m.DeLa, doc.Predator.Id, partide),
            La = Partide.CuPartida(m.La, doc.Predator.Id, partide),
        })], decizii, [operand.PerioadaDeschisa, operand.VersiunePolitica]);
    }
}
