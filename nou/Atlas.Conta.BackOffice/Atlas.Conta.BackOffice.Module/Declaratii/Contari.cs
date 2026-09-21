#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

/// <summary>Regula de contare a liniei, cu cele două conturi deja rezolvate.</summary>
readonly record struct ContareLinie(
    RegulaContareFapt Regula,
    Guid ContDebit,
    Guid ContCredit,
    SursaRezolvata SursaDebit,
    SursaRezolvata SursaCredit);

/// <summary>
/// Rezolvările comune ale declaranților, pe funcțiile pure existente ale
/// motorului: contul fiecărei laturi și coalesce-ul dimensiunilor.
/// </summary>
static class Contari {
    public static ContareLinie? Rezolva(Operand operand, LinieOperand linie, ICollection<N.Refuz> refuzuri) {
        if (Potrivire.Contare(operand.ReguliContare, linie.Fapt).Castigator is not { } regula) {
            refuzuri.Add(new N.Refuz(CoduriRefuz.RegulaContareLipsa,
                "Linia nu are regulă de contare pe acest tip de document.", linie.Id));
            return null;
        }
        var debit = Potrivire.Cont(
            regula.SursaContDebit, regula.ContDebitId, linie.ContImplicitTipId, operand.Laturi);
        var credit = Potrivire.Cont(
            regula.SursaContCredit, regula.ContCreditId, linie.ContImplicitTipId, operand.Laturi);
        if (debit.ContId is not Guid contDebit) {
            refuzuri.Add(new N.Refuz(CoduriRefuz.RegulaContareLipsa,
                $"Contul debitor nu se poate rezolva (sursă {regula.SursaContDebit}).", linie.Id));
            return null;
        }
        if (credit.ContId is not Guid contCredit) {
            refuzuri.Add(new N.Refuz(CoduriRefuz.RegulaContareLipsa,
                $"Contul creditor nu se poate rezolva (sursă {regula.SursaContCredit}).", linie.Id));
            return null;
        }
        return new ContareLinie(regula, contDebit, contCredit, debit.Sursa, credit.Sursa);
    }

    // B-D8 pct. 8: același coalesce ca motorul vechi, pe funcția pură existentă —
    // repartitorul și materialul nu sunt axe de `Analiza` (sunt `Gestiune`/`Produs`).
    public static N.Analiza Analiza(N.Analiza aLiniei, Dimensiuni? overrideLatura, Dimensiuni? comun) {
        var rezolvate = DimensiuniResolver.Rezolva(
            new Dimensiuni {
                CodFunctionalId = aLiniei.CodFunctional,
                CodEconomicId = aLiniei.CodEconomic,
                SursaFinantareId = aLiniei.SursaFinantare,
                UnitateId = aLiniei.UnitateOrganizatorica,
                ProiectId = aLiniei.Proiect,
                CentruCostId = aLiniei.CentruCost,
            },
            overrideLatura,
            comun);
        return new N.Analiza(rezolvate.CodFunctionalId, rezolvate.CodEconomicId, rezolvate.SursaFinantareId,
            rezolvate.UnitateId, rezolvate.ProiectId, rezolvate.CentruCostId);
    }
}
