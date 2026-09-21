#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

/// <summary>
/// BCS (B-D4): o mișcare per linie — lotul iese din gestiunea predatoare pe
/// contul de stoc al regulii și intră pe contul de cheltuială al locului de
/// consum, evaluat pe raportul curent al lotului.
/// </summary>
public sealed class DeclarantBonConsum : IDeclarant {
    public static readonly DeclarantBonConsum Instanta = new();

    DeclarantBonConsum() { }

    public N.Declaratie? Declara(Operand operand, N.Rotunjire rotunjire, ICollection<N.Refuz> refuzuri) {
        ArgumentNullException.ThrowIfNull(operand);
        ArgumentNullException.ThrowIfNull(rotunjire);
        ArgumentNullException.ThrowIfNull(refuzuri);

        var doc = operand.Document;
        if (doc.Predator.Fel != FelRepartitor.Gestiune)
            refuzuri.Add(new N.Refuz(CoduriRefuz.PredatorNepotrivit,
                "Predatorul bonului de consum trebuie să fie o gestiune.", null));
        if (doc.Primitor.Fel == FelRepartitor.Partener
                || !doc.Primitor.Calitati.HasFlag(CalitateRepartitor.LocConsum))
            refuzuri.Add(new N.Refuz(CoduriRefuz.PrimitorNepotrivit,
                "Primitorul trebuie să fie un loc de consum intern (calitatea LocConsum).", null));
        if (operand.Linii.Count == 0)
            refuzuri.Add(new N.Refuz(CoduriRefuz.LiniiLipsa,
                "Bonul de consum se cere cu cel puțin o linie.", null));

        var contari = new ContareLinie?[operand.Linii.Count];
        for (var i = 0; i < operand.Linii.Count; i++) {
            var linie = operand.Linii[i];
            if (linie.Lot is null)
                refuzuri.Add(new N.Refuz(CoduriRefuz.LotLipsa,
                    "Fiecare linie de consum referă un lot.", linie.Id));
            if (linie.Cantitate <= 0m)
                refuzuri.Add(new N.Refuz(CoduriRefuz.CantitateNepozitiva,
                    "Cantitatea consumată trebuie să fie pozitivă.", linie.Id));
            contari[i] = Contari.Rezolva(operand, linie, refuzuri);
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
            decimal valoare;
            try {
                // N-D7/N-r3: raportul CURENT al lotului, în secvența liniilor.
                valoare = N.Evaluare.Iesire(sold, linie.Cantitate, rotunjire);
            }
            catch (N.RefuzException e) {
                // Refuzul e al liniei, nu al documentului: iterarea continuă ca să
                // iasă TOATE refuzurile, nu primul (B-D2, MINOR-1).
                refuzuri.Add(new N.Refuz(e.Refuz.Cod, e.Refuz.Mesaj, linie.Id));
                continue;
            }
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
                    Analiza = Contari.Analiza(linie.Analiza, contare.Regula.OverrideCredit, contare.Regula.Comun),
                },
                new N.Capat {
                    Cont = contare.ContDebit,
                    Gestiune = doc.Primitor.Id,
                    Produs = lot.ProdusId,
                    // C5 cere unitatea pe contul postării.
                    Unitate = iesit with { Cont = contare.ContDebit },
                    Analiza = Contari.Analiza(linie.Analiza, contare.Regula.OverrideDebit, contare.Regula.Comun),
                },
                linie.Cantitate,
                0m,
                valoare,
                new N.Cauza(doc.Id, linie.Id)));
        }
        if (refuzuri.Count > 0)
            return null;
        ipoteze.Add(operand.PerioadaDeschisa);
        ipoteze.Add(operand.VersiunePolitica);
        return new N.Declaratie(doc.Id, doc.DataInregistrare, miscari, decizii, ipoteze);
    }
}
