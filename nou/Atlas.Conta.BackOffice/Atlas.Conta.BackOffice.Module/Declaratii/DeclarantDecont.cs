#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

public sealed class DeclarantDecont : IDeclarant {
    public static readonly DeclarantDecont Instanta = new();
    DeclarantDecont() { }
    public bool ConteazaPrinReguli => true;

    public N.Declaratie? Declara(Operand operand, N.Rotunjire rotunjire, ICollection<N.Refuz> refuzuri) {
        var miscari = new List<N.Miscare>();
        var decizii = new List<N.Decizie>();
        var partide = new Dictionary<(Guid, Guid), N.Unitate>();
        var tipuri = new TipTvaFapt?[operand.Linii.Count];
        if (operand.Linii.Count == 0)
            refuzuri.Add(new(CoduriRefuz.LiniiLipsa, "Decontul cere cel puțin o linie.", null));
        for (var i = 0; i < operand.Linii.Count; i++) {
            var l = operand.Linii[i];
            if (l.Valoare <= 0)
                refuzuri.Add(new(CoduriRefuz.ValoareNepozitiva, "Cheltuiala justificată trebuie să fie pozitivă.", l.Id));
            if (operand.PoliticaTva != null && l.TipTvaId is Guid id) {
                if (!operand.TipuriTva.TryGetValue(id, out var tip))
                    refuzuri.Add(new(CoduriRefuz.TipTvaLipsa, "Tipul TVA nu există.", l.Id));
                else tipuri[i] = tip;
            }
        }
        var taxa = Fiscal.Taxa(operand, tipuri, rotunjire, refuzuri);
        if (refuzuri.Count > 0) return null;
        for (var i = 0; i < operand.Linii.Count; i++) {
            var l = operand.Linii[i];
            var regula = Potrivire.Contare(operand.ReguliContare, l.Fapt).Castigator;
            if (regula is null) {
                refuzuri.Add(new(CoduriRefuz.RegulaContareLipsa, "Linia de decont cere regulă de contare.", l.Id));
                continue;
            }
            var debit = l.ContDebitId ?? Potrivire.Cont(regula.Value.SursaContDebit,
                regula.Value.ContDebitId, l.ContImplicitTipId, operand.Laturi).ContId;
            var credit = l.ContCreditId ?? Potrivire.Cont(regula.Value.SursaContCredit,
                regula.Value.ContCreditId, l.ContImplicitTipId, operand.Laturi).ContId;
            if (debit is not Guid d || credit is not Guid c || !operand.Conturi.ContainsKey(d) || !operand.Conturi.ContainsKey(c)) {
                refuzuri.Add(new(CoduriRefuz.ContExplicitLipsa, "Conturile liniei nu pot fi rezolvate.", l.Id));
                continue;
            }
            var rd = l.RepartitorDebitId ?? regula.Value.OverrideDebit?.RepartitorId
                ?? regula.Value.Comun?.RepartitorId ?? operand.Document.Predator.Id;
            var rc = l.RepartitorCreditId ?? regula.Value.OverrideCredit?.RepartitorId
                ?? regula.Value.Comun?.RepartitorId ?? operand.Document.Predator.Id;
            var capD = Capat(d, rd, Contari.Analiza(l.Analiza, regula.Value.OverrideDebit, regula.Value.Comun), l.Id);
            var capC = Capat(c, rc, Contari.Analiza(l.Analiza, regula.Value.OverrideCredit, regula.Value.Comun), l.Id);
            var baza = tipuri[i] is { } tipBaza ? Fiscal.CuFapt(operand, capD, tipBaza, N.RolTva.Baza) : capD;
            var cauza = new N.Cauza(operand.Document.Id, l.Id);
            if (tipuri[i] is { Regim: RegimTva.Capitalizat } cap) {
                var net = rotunjire.Bani(l.Valoare / (1m + cap.Cota / 100m));
                miscari.Add(new(capC, baza, 0, 0, net, cauza));
                if (l.Valoare != net) miscari.Add(new(capC, Fiscal.CuFapt(operand, capD, cap, N.RolTva.Taxa),
                    0, 0, l.Valoare - net, cauza));
            }
            else miscari.Add(new(capC, baza, 0, 0, l.Valoare, cauza));
            if (Fiscal.Impozitul(operand, l, tipuri[i], taxa, DirectieTva.Deductibil, refuzuri) is { } impozit)
                miscari.Add(impozit with { DeLa = Capat(impozit.DeLa.Cont, operand.Document.Predator.Id,
                    impozit.DeLa.Analiza, l.Id) with {
                        CodTva = impozit.DeLa.CodTva, ReperFiscal = impozit.DeLa.ReperFiscal,
                        PerioadaDeclarare = impozit.DeLa.PerioadaDeclarare,
                    } });
        }
        return refuzuri.Count > 0 ? null : new(operand.Document.Id, operand.Document.DataInregistrare,
            miscari, decizii, [operand.PerioadaDeschisa, operand.VersiunePolitica]);

        N.Capat Capat(Guid cont, Guid repartitor, N.Analiza analiza, Guid linie) {
            decizii.Add(new N.ContRezolvat(linie, cont, "decont"));
            if (!operand.Repartitori.TryGetValue(repartitor, out var r) || r.Parte == null) {
                refuzuri.Add(new(CoduriRefuz.RepartitorExplicitLipsa, "Repartitorul liniei nu poate fi rezolvat.", linie));
                return new() { Cont = cont, Analiza = analiza };
            }
            N.Unitate? unitate = null;
            if (r.Parte == Parte.Extern && Partide.Urmareste(operand, cont)) {
                if (!partide.TryGetValue((cont, repartitor), out unitate)) {
                    unitate = Partide.Proprie(operand, cont, repartitor);
                    partide.Add((cont, repartitor), unitate);
                    decizii.Add(new N.PartidaDeschisa(linie, unitate));
                }
            }
            return new() { Cont = cont, Analiza = analiza, Unitate = unitate,
                Partener = r.Parte == Parte.Extern ? r.Id : null,
                Gestiune = r.Parte is Parte.Intern or Parte.Propriu ? r.Id : null };
        }
    }
}
