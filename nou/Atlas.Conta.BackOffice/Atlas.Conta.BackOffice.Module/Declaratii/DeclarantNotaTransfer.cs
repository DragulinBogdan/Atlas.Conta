#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

/// <summary>
/// BTR (T-D2): o mutare per linie — lotul își schimbă gestiunea pe contul lui de
/// stoc, evaluat pe raportul curent al gestiunii predatoare. Contul nu se schimbă,
/// deci declarația e numai `Mutari` și motorul o închide într-un `Transfer`.
/// </summary>
public sealed class DeclarantNotaTransfer : IDeclarant {
    public static readonly DeclarantNotaTransfer Instanta = new();

    DeclarantNotaTransfer() { }

    public N.Declaratie? Declara(Operand operand, N.Rotunjire rotunjire, ICollection<N.Refuz> refuzuri) {
        ArgumentNullException.ThrowIfNull(operand);
        ArgumentNullException.ThrowIfNull(rotunjire);
        ArgumentNullException.ThrowIfNull(refuzuri);

        var doc = operand.Document;
        if (doc.Predator.Fel != FelRepartitor.Gestiune)
            refuzuri.Add(new N.Refuz(CoduriRefuz.PredatorNepotrivit,
                "Transferul se face între două gestiuni.", null));
        if (doc.Primitor.Fel != FelRepartitor.Gestiune)
            refuzuri.Add(new N.Refuz(CoduriRefuz.PrimitorNepotrivit,
                "Transferul se face între două gestiuni.", null));
        if (doc.Predator.Id == doc.Primitor.Id)
            refuzuri.Add(new N.Refuz(CoduriRefuz.GestiuniIdentice,
                "Gestiunea sursă și cea destinație trebuie să difere.", null));
        if (operand.Linii.Count == 0)
            refuzuri.Add(new N.Refuz(CoduriRefuz.LiniiLipsa,
                "Nota de transfer se cere cu cel puțin o linie.", null));

        foreach (var linie in operand.Linii) {
            if (linie.Lot is null)
                refuzuri.Add(new N.Refuz(CoduriRefuz.LotLipsa,
                    "Fiecare linie de transfer referă un lot.", linie.Id));
            else if (linie.Lot.ContImplicitId is null)
                refuzuri.Add(new N.Refuz(CoduriRefuz.ContStocLipsa,
                    "Lotul transferat n-are cont de stoc pe tipul lui de material.", linie.Id));
            if (linie.Cantitate <= 0m)
                refuzuri.Add(new N.Refuz(CoduriRefuz.CantitateNepozitiva,
                    "Cantitatea transferată trebuie să fie pozitivă.", linie.Id));
        }
        if (refuzuri.Count > 0)
            return null;

        var mutari = new List<N.Mutare>(operand.Linii.Count);
        var decizii = new List<N.Decizie>();
        var ipoteze = new List<N.Ipoteza>();
        var solduri = new Dictionary<Guid, N.Sold>();
        foreach (var linie in operand.Linii) {
            var lot = linie.Lot!;
            var cont = lot.ContImplicitId!.Value;
            var unitate = new N.Unitate(lot.Id, N.FelUnitate.Lot, cont, null, lot.ProdusId, lot.Data);
            if (!solduri.TryGetValue(lot.Id, out var sold)) {
                sold = operand.SolduriLoturi.GetValueOrDefault(lot.Id) ?? N.Sold.Zero;
                solduri[lot.Id] = sold;
                ipoteze.Add(new N.SoldUnitateCitit(unitate, sold));
            }
            decimal valoare;
            try {
                // N-D7: raportul CURENT al lotului în gestiunea predatoare, în secvența liniilor.
                valoare = N.Evaluare.Iesire(sold, linie.Cantitate, rotunjire);
            }
            catch (N.RefuzException e) {
                refuzuri.Add(new N.Refuz(e.Refuz.Cod, e.Refuz.Mesaj, linie.Id));
                continue;
            }
            solduri[lot.Id] = new N.Sold(
                sold.Debit, sold.Credit + valoare, sold.Cantitate - linie.Cantitate, sold.ValoareValuta);

            decizii.Add(new N.ContRezolvat(linie.Id, cont, SursaCont.TipMaterial.ToString()));
            decizii.Add(new N.ValoareIesire(linie.Id, unitate, linie.Cantitate, valoare));

            var capat = new N.Capat {
                Cont = cont,
                Gestiune = doc.Predator.Id,
                Produs = lot.ProdusId,
                Unitate = unitate,
            };
            mutari.Add(new N.Mutare(
                capat,
                capat with { Gestiune = doc.Primitor.Id },
                N.Latura.Debit,
                linie.Cantitate,
                0m,
                valoare,
                new N.Cauza(doc.Id, linie.Id)));
        }
        if (refuzuri.Count > 0)
            return null;
        ipoteze.Add(operand.PerioadaDeschisa);
        ipoteze.Add(operand.VersiunePolitica);
        return new N.Declaratie(doc.Id, doc.DataInregistrare, [], mutari, decizii, ipoteze);
    }
}
