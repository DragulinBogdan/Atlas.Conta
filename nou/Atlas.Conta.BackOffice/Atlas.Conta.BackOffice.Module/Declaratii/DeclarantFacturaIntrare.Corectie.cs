#nullable enable
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

public sealed partial class DeclarantFacturaIntrare {
    // CV-D1: factura corectivă împarte valoarea fiecărei linii pe beneficiarii lotului ei.
    static N.Declaratie? DeclaraCorectia(Operand operand, ICollection<N.Refuz> refuzuri) {
        var doc = operand.Document;
        var miscari = new List<N.Miscare>();
        var decizii = new List<N.Decizie>();
        var solduri = new List<N.Ipoteza>();
        var partide = new Dictionary<Guid, N.UnitateSold>();
        var loturi = new HashSet<Guid>();
        foreach (var linie in operand.Linii) {
            if (Structura(linie, loturi) is { } abatere) {
                refuzuri.Add(new N.Refuz(CoduriRefuz.CvStructura, abatere, linie.Id));
                continue;
            }
            if (linie.Lot is not { } lot) {
                refuzuri.Add(new N.Refuz(CoduriRefuz.LotLipsa, "Linia de corecție referă lotul corectat.", linie.Id));
                continue;
            }
            if (linie.Valoare == 0m) {
                refuzuri.Add(new N.Refuz(CoduriRefuz.ValoareZero, "Linia de corecție se cere cu valoare nenulă.", linie.Id));
                continue;
            }
            if (Contari.Rezolva(operand, linie, refuzuri, semn: 1) is not { } contare)
                continue;
            if (linie.CorectieValorica!.Reper is not { } reper) {
                refuzuri.Add(new N.Refuz(CoduriRefuz.CvLotNeeligibil,
                    "Lotul nu are linie de intrare sau cont de stoc.", linie.Id));
                continue;
            }
            IReadOnlyList<N.AlocareLot> alocari;
            try {
                alocari = N.CorectieValorica.Aloca(
                    N.CorectieValorica.Beneficiari(reper, linie.CorectieValorica.Fapte, doc.DataInregistrare), linie.Valoare);
            }
            catch (N.CorectieValoricaRefuzata e) {
                refuzuri.Add(CoduriRefuz.Refuz(e, linie.Id));
                continue;
            }
            decizii.Add(new N.ContRezolvat(linie.Id, contare.ContCredit, contare.SursaCredit.ToString(), contare.Regula.Id));
            Partide.Numeste(operand, contare.ContCredit, doc.Predator.Id, linie.Id, partide, decizii);
            var tert = Terti.Capat(operand, new N.Capat {
                Cont = contare.ContCredit,
                Contrapondere = N.Contrapondere.Tert,
                Produs = lot.ProdusId,
                Analiza = Contari.Analiza(linie.Analiza, contare.Regula.OverrideCredit, contare.Regula.Comun),
            }, doc.Predator, partide);
            var cauza = new N.Cauza(doc.Id, linie.Id);
            foreach (var alocare in alocari) {
                decizii.Add(N.CorectieValorica.Decizia(alocare, linie.Id));
                if (alocare.Valoare != 0m)
                    miscari.Add(N.CorectieValorica.Miscarea(alocare, tert, cauza));
            }
            solduri.Add(new N.SoldUnitateCitit(
                new N.UnitateSold(lot.Id, N.FelUnitate.Lot, reper.ContStoc, null, lot.ProdusId, lot.Data),
                linie.CorectieValorica.Fapte
                    .Where(f => f.Postare.Coordonate is { Unitate: { Fel: N.FelUnitate.Lot } unitate } c && unitate.Id == lot.Id
                        && c.Cont == reper.ContStoc && c.Repartitor?.Id == reper.Gestiune)
                    .Aggregate(N.Sold.Zero, (sold, f) => sold + N.Sold.Din(f.Postare))));
        }
        if (refuzuri.Count > 0)
            return null;
        return new N.Declaratie(doc.Id, doc.DataInregistrare, miscari, decizii,
            [.. PoliticiConsumate.Ipoteze(operand, decizii), .. solduri]);
    }

    static string? Structura(LinieOperand linie, HashSet<Guid> loturi) =>
        linie.CorectieValorica is null ? "Factura cu linii de corecție are numai linii de corecție."
        : linie.Cantitate != 0m ? "Linia de corecție nu poartă cantitate."
        : linie.TipTvaId is not null || linie.ValoareTva != 0m ? "Corecția de valoare cu TVA nu este acoperită."
        : linie.LotId is Guid lot && !loturi.Add(lot) ? "Lotul apare pe mai multe linii de corecție ale documentului."
        : linie.Lot?.TipMaterialId is Guid tip && tip != linie.TipMaterialId ? "Linia de corecție are tipul produsului lotului."
        : null;
}
