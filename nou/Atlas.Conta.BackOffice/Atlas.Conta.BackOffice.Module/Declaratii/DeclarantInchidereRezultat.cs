#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

/// <summary>
/// Închiderea conturilor de rezultat (E3-D3): fiecare linie mută o cheie-sursă întreagă; repartitorii
/// laturilor se iau literal de pe linie, iar destinația păstrează din analiză doar coordonatele declarate.
/// </summary>
public sealed class DeclarantInchidereRezultat : IDeclarant {
    public static readonly DeclarantInchidereRezultat Instanta = new();
    DeclarantInchidereRezultat() { }
    public PoliticaProfil PoliticaCeruta => PoliticaProfil.InchidereConturi;

    public N.Declaratie? Declara(Operand operand, N.Rotunjire rotunjire, ICollection<N.Refuz> refuzuri) {
        ArgumentNullException.ThrowIfNull(operand);
        ArgumentNullException.ThrowIfNull(refuzuri);
        if (operand.Linii.Count == 0)
            refuzuri.Add(new(CoduriRefuz.LiniiLipsa, "Închiderea cere cel puțin o linie.", null));
        foreach (var l in operand.Linii) {
            if (l.ContDebitId is not Guid d || !operand.Conturi.ContainsKey(d)
                || l.ContCreditId is not Guid c || !operand.Conturi.ContainsKey(c))
                refuzuri.Add(new(CoduriRefuz.ContExplicitLipsa, "Linia cere ambele conturi explicite.", l.Id));
            if (l.Valoare <= 0m)
                refuzuri.Add(new(CoduriRefuz.ValoareNepozitiva, "Linia închiderii mută un sold pozitiv.", l.Id));
            if (l.Inchidere is null)
                refuzuri.Add(new(CoduriRefuz.InchidereStructuraInvalida, "Linia nu poartă latura sursei.", l.Id));
            foreach (var id in new[] { l.RepartitorDebitId, l.RepartitorCreditId }.OfType<Guid>())
                if (!operand.Repartitori.ContainsKey(id))
                    refuzuri.Add(new(CoduriRefuz.RepartitorExplicitLipsa, "Repartitorul explicit nu poate fi rezolvat.", l.Id));
        }
        if (refuzuri.Count > 0) return null;

        var miscari = new List<N.Miscare>();
        var decizii = new List<N.Decizie>();
        foreach (var l in operand.Linii) {
            var sursaPeDebit = l.Inchidere!.SursaPeDebit;
            var debit = Capat(l, l.ContDebitId!.Value, l.RepartitorDebitId, sursaPeDebit);
            var credit = Capat(l, l.ContCreditId!.Value, l.RepartitorCreditId, !sursaPeDebit);
            decizii.Add(new N.ContRezolvat(l.Id, debit.Cont, "explicit"));
            decizii.Add(new N.ContRezolvat(l.Id, credit.Cont, "explicit"));
            miscari.Add(new(credit, debit, 0m, 0m, l.Valoare, new(operand.Document.Id, l.Id)));
        }
        return new(operand.Document.Id, operand.Document.DataInregistrare, miscari, decizii, [operand.PerioadaDeschisa]);

        N.Capat Capat(LinieOperand linie, Guid cont, Guid? repartitorId, bool sursa) {
            var pastrate = sursa ? (DimensiuneFlags)~0 : linie.Inchidere!.Pastrate;
            bool Pastreaza(DimensiuneFlags f) => (pastrate & f) == f;
            var a = linie.Analiza;
            return new N.Capat {
                Cont = cont,
                Repartitor = repartitorId is Guid id ? operand.Repartitori[id].Coordonata : null,
                Produs = Pastreaza(DimensiuneFlags.Material) ? linie.ProdusId : null,
                Analiza = new(
                    Pastreaza(DimensiuneFlags.CodFunctional) ? a.CodFunctional : null,
                    Pastreaza(DimensiuneFlags.CodEconomic) ? a.CodEconomic : null,
                    Pastreaza(DimensiuneFlags.SursaFinantare) ? a.SursaFinantare : null,
                    Pastreaza(DimensiuneFlags.Unitate) ? a.UnitateOrganizatorica : null,
                    Pastreaza(DimensiuneFlags.Proiect) ? a.Proiect : null,
                    Pastreaza(DimensiuneFlags.CentruCost) ? a.CentruCost : null),
            };
        }
    }
}
