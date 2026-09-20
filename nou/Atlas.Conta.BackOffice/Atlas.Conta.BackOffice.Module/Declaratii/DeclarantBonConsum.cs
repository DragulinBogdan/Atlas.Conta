#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

/// <summary>
/// BCS (B-D4): o mișcare per linie — lotul iese din gestiunea predatoare pe
/// contul de stoc al regulii și intră pe contul de cheltuială al locului de
/// consum, evaluat pe raportul curent al lotului.
/// </summary>
public sealed class DeclarantBonConsum : IDeclarant {
    public static readonly DeclarantBonConsum Instanta = new();

    const string PredatorNepotrivit = "PREDATOR_NEPOTRIVIT";
    const string PrimitorNepotrivit = "PRIMITOR_NEPOTRIVIT";
    const string LiniiLipsa = "LINII_LIPSA";
    const string LotLipsa = "LOT_LIPSA";
    const string CantitateNepozitiva = "CANTITATE_NEPOZITIVA";
    const string RegulaContareLipsa = "REGULA_CONTARE_LIPSA";

    DeclarantBonConsum() { }

    readonly record struct Contare(
        RegulaContareFapt Regula,
        Guid ContDebit,
        Guid ContCredit,
        SursaRezolvata SursaDebit,
        SursaRezolvata SursaCredit);

    public N.Declaratie? Declara(Operand operand, N.Rotunjire rotunjire, ICollection<N.Refuz> refuzuri) {
        ArgumentNullException.ThrowIfNull(operand);
        ArgumentNullException.ThrowIfNull(rotunjire);
        ArgumentNullException.ThrowIfNull(refuzuri);

        var doc = operand.Document;
        if (doc.Predator.Fel != FelRepartitor.Gestiune)
            refuzuri.Add(new N.Refuz(PredatorNepotrivit,
                "Predatorul bonului de consum trebuie să fie o gestiune.", null));
        if (doc.Primitor.Fel == FelRepartitor.Partener
                || !doc.Primitor.Calitati.HasFlag(CalitateRepartitor.LocConsum))
            refuzuri.Add(new N.Refuz(PrimitorNepotrivit,
                "Primitorul trebuie să fie un loc de consum intern (calitatea LocConsum).", null));
        if (operand.Linii.Count == 0)
            refuzuri.Add(new N.Refuz(LiniiLipsa,
                "Bonul de consum se cere cu cel puțin o linie.", null));

        var contari = new Contare?[operand.Linii.Count];
        for (var i = 0; i < operand.Linii.Count; i++) {
            var linie = operand.Linii[i];
            if (linie.Lot is null)
                refuzuri.Add(new N.Refuz(LotLipsa,
                    "Fiecare linie de consum referă un lot.", linie.Id));
            if (linie.Cantitate <= 0m)
                refuzuri.Add(new N.Refuz(CantitateNepozitiva,
                    "Cantitatea consumată trebuie să fie pozitivă.", linie.Id));
            contari[i] = Conturi(operand, linie, refuzuri);
        }
        if (refuzuri.Count > 0)
            return null;

        var miscari = new List<N.Miscare>(operand.Linii.Count);
        var decizii = new List<N.Decizie>();
        var ipoteze = new List<N.Ipoteza>();
        var solduri = new Dictionary<Guid, N.Sold>();
        for (var i = 0; i < operand.Linii.Count; i++) {
            var linie = operand.Linii[i];
            var contare = contari[i]!.Value;
            var lot = linie.Lot!;
            var iesit = new N.Unitate(
                lot.Id, N.FelUnitate.Lot, contare.ContCredit, null, lot.ProdusId, lot.Data);
            if (!solduri.TryGetValue(lot.Id, out var sold)) {
                sold = operand.SolduriLoturi.GetValueOrDefault(lot.Id) ?? N.Sold.Zero;
                solduri[lot.Id] = sold;
                ipoteze.Add(new N.SoldUnitateCitit(iesit, sold));
            }
            // N-D7/N-r3: raportul CURENT al lotului, în secvența liniilor.
            var valoare = N.Evaluare.Iesire(sold, linie.Cantitate, rotunjire);
            solduri[lot.Id] = new N.Sold(
                sold.Debit, sold.Credit + valoare, sold.Cantitate - linie.Cantitate, sold.ValoareValuta);

            decizii.Add(new N.ContRezolvat(linie.Id, contare.ContDebit, contare.SursaDebit.ToString()));
            decizii.Add(new N.ContRezolvat(linie.Id, contare.ContCredit, contare.SursaCredit.ToString()));
            decizii.Add(new N.ValoareIesire(linie.Id, iesit, linie.Cantitate, valoare));

            miscari.Add(new N.Miscare(
                new N.Capat {
                    Cont = contare.ContCredit,
                    Gestiune = doc.Predator.Id,
                    Produs = lot.ProdusId,
                    Unitate = iesit,
                    Analiza = Analiza(linie.Analiza, contare.Regula.OverrideCredit, contare.Regula.Comun),
                },
                new N.Capat {
                    Cont = contare.ContDebit,
                    Gestiune = doc.Primitor.Id,
                    Produs = lot.ProdusId,
                    // C5 cere unitatea pe contul postării.
                    Unitate = iesit with { Cont = contare.ContDebit },
                    Analiza = Analiza(linie.Analiza, contare.Regula.OverrideDebit, contare.Regula.Comun),
                },
                linie.Cantitate,
                0m,
                valoare,
                new N.Cauza(doc.Id, linie.Id)));
        }
        ipoteze.Add(operand.PerioadaDeschisa);
        ipoteze.Add(operand.VersiunePolitica);
        return new N.Declaratie(doc.Id, doc.DataInregistrare, miscari, decizii, ipoteze);
    }

    static Contare? Conturi(Operand operand, LinieOperand linie, ICollection<N.Refuz> refuzuri) {
        if (Potrivire.Contare(operand.ReguliContare, linie.Fapt).Castigator is not { } regula) {
            refuzuri.Add(new N.Refuz(RegulaContareLipsa,
                "Linia nu are regulă de contare pe acest tip de document.", linie.Id));
            return null;
        }
        var debit = Potrivire.Cont(
            regula.SursaContDebit, regula.ContDebitId, linie.ContImplicitTipId, operand.Laturi);
        var credit = Potrivire.Cont(
            regula.SursaContCredit, regula.ContCreditId, linie.ContImplicitTipId, operand.Laturi);
        if (debit.ContId is not Guid contDebit) {
            refuzuri.Add(new N.Refuz(RegulaContareLipsa,
                $"Contul debitor nu se poate rezolva (sursă {regula.SursaContDebit}).", linie.Id));
            return null;
        }
        if (credit.ContId is not Guid contCredit) {
            refuzuri.Add(new N.Refuz(RegulaContareLipsa,
                $"Contul creditor nu se poate rezolva (sursă {regula.SursaContCredit}).", linie.Id));
            return null;
        }
        return new Contare(regula, contDebit, contCredit, debit.Sursa, credit.Sursa);
    }

    // B-D8 pct. 8: același coalesce ca motorul vechi, pe funcția pură existentă —
    // repartitorul și materialul nu sunt axe de `Analiza` (sunt `Gestiune`/`Produs`).
    static N.Analiza Analiza(N.Analiza aLiniei, Dimensiuni overrideLatura, Dimensiuni comun) {
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
