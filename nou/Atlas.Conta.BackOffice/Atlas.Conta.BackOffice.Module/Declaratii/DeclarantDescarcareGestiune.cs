#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

/// <summary>
/// DSC (T-D4): o mișcare per linie — lotul iese din gestiunea predatoare pe contul
/// de stoc al regulii, evaluat pe raportul curent, iar costul intră pe gestiunea
/// virtuală a clientului, marfa părăsind patrimoniul. Descărcarea are aceeași formă
/// cu sau fără factura care o naște: nu nominalizează nimic al ei.
/// </summary>
public sealed class DeclarantDescarcareGestiune : IDeclarant {
    public static readonly DeclarantDescarcareGestiune Instanta = new();

    DeclarantDescarcareGestiune() { }

    public N.Declaratie? Declara(Operand operand, N.Rotunjire rotunjire, ICollection<N.Refuz> refuzuri) {
        ArgumentNullException.ThrowIfNull(operand);
        ArgumentNullException.ThrowIfNull(rotunjire);
        ArgumentNullException.ThrowIfNull(refuzuri);

        var doc = operand.Document;
        if (operand.Linii.Count == 0)
            refuzuri.Add(new N.Refuz(CoduriRefuz.LiniiLipsa,
                "Descărcarea de gestiune se cere cu cel puțin o linie.", null));

        var contari = new ContareLinie?[operand.Linii.Count];
        for (var i = 0; i < operand.Linii.Count; i++) {
            var linie = operand.Linii[i];
            if (linie.Lot is null)
                refuzuri.Add(new N.Refuz(CoduriRefuz.LotLipsa,
                    "Fiecare linie de descărcare referă un lot.", linie.Id));
            if (linie.Cantitate <= 0m)
                refuzuri.Add(new N.Refuz(CoduriRefuz.CantitateNepozitiva,
                    "Cantitatea descărcată trebuie să fie pozitivă.", linie.Id));
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
                    // N-D4: marfa părăsește patrimoniul — capătul de cost e al clientului.
                    Gestiune = N.GestiuniVirtuale.Client,
                    // T-D13 (g): terțul nominalizat pe capătul extern.
                    Partener = doc.Primitor.Parte == Parte.Extern ? doc.Primitor.Id : null,
                    Produs = lot.ProdusId,
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
