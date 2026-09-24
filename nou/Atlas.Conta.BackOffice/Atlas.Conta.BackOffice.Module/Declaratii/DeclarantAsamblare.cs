#nullable enable
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

public sealed class DeclarantAsamblare : IDeclarant {
    public static readonly DeclarantAsamblare Instanta = new();
    DeclarantAsamblare() { }

    public N.Declaratie? Declara(Operand operand, N.Rotunjire rotunjire, ICollection<N.Refuz> refuzuri) {
        ArgumentNullException.ThrowIfNull(operand);
        ArgumentNullException.ThrowIfNull(rotunjire);
        ArgumentNullException.ThrowIfNull(refuzuri);
        var doc = operand.Document;
        var linii = operand.Linii;
        var proprii = linii.Select(l => l.Id).ToHashSet();
        if (!linii.Any(l => l.Transformare?.Rol == N.RolTransformare.Consum)
                || !linii.Any(l => l.Transformare?.Rol == N.RolTransformare.Produs))
            refuzuri.Add(new(CoduriRefuz.AsamblareStructuraInvalida, "Asamblarea cere consum și produs.", null));
        foreach (var l in linii) {
            if (l.Transformare?.Rol is not (N.RolTransformare.Consum or N.RolTransformare.Produs))
                refuzuri.Add(new(CoduriRefuz.AsamblareStructuraInvalida, "Linia cere rol consum/produs.", l.Id));
            if (l.Cantitate == 0m)
                refuzuri.Add(new(CoduriRefuz.CantitateNepozitiva, "Cantitatea absolută trebuie să fie pozitivă.", l.Id));
            if (l.Lot is not { } lot) {
                refuzuri.Add(new(CoduriRefuz.LotLipsa, "Linia cere un lot.", l.Id));
                continue;
            }
            if (lot.ContImplicitId is null)
                refuzuri.Add(new(CoduriRefuz.ContStocLipsa, "Lotul cere cont de stoc.", l.Id));
            if (lot.TipMaterialId != l.TipMaterialId)
                refuzuri.Add(new(CoduriRefuz.AsamblareStructuraInvalida, "Tipul liniei diferă de cel al lotului.", l.Id));
            if (l.Transformare?.Rol == N.RolTransformare.Produs) {
                if (lot.LinieIntrareId != l.Id || lot.GestiuneId != doc.Predator.Id
                        || (l.ProdusId is Guid produsCules && produsCules != lot.ProdusId)
                        || l.Transformare.PretProdus is not > 0m)
                    refuzuri.Add(new(CoduriRefuz.AsamblareStructuraInvalida,
                        "Produsul cere lot propriu în gestiunea de asamblare și preț pozitiv.", l.Id));
            }
            else if (lot.LinieIntrareId is Guid intrare && proprii.Contains(intrare))
                refuzuri.Add(new(CoduriRefuz.AsamblareStructuraInvalida, "Consumul nu poate referi lotul propriu documentului.", l.Id));
        }
        if (refuzuri.Count > 0) return null;

        var decizii = new List<N.Decizie>();
        var ipoteze = new List<N.Ipoteza> { operand.PerioadaDeschisa, operand.VersiunePolitica };
        var solduriCub = new Dictionary<Guid, N.Sold>();
        var solduriRegistru = new Dictionary<Guid, N.Sold>();
        var valori = new List<ValoriLinie>();
        foreach (var l in linii) {
            var lot = l.Lot!;
            var unitate = new N.Unitate(lot.Id, N.FelUnitate.Lot, lot.ContImplicitId!.Value,
                null, lot.ProdusId, lot.Data);
            var q = Math.Abs(l.Cantitate);
            var produs = l.Transformare!.Rol == N.RolTransformare.Produs;
            decimal r = 0m, c = 0m, p = 0m;
            if (produs) p = rotunjire.Bani(q * l.Transformare.PretProdus!.Value);
            else {
                if (!solduriCub.TryGetValue(lot.Id, out var sold)) {
                    sold = operand.SolduriLoturi.GetValueOrDefault(lot.Id) ?? N.Sold.Zero;
                    solduriRegistru[lot.Id] = sold;
                    ipoteze.Add(new N.SoldUnitateCitit(unitate, sold));
                }
                try { c = N.Evaluare.Iesire(sold, q, rotunjire); }
                catch (N.RefuzException e) { refuzuri.Add(e.Refuz with { Linie = l.Id }); continue; }
                var registru = solduriRegistru[lot.Id];
                r = q == registru.Cantitate ? registru.Net : rotunjire.Bani(q * lot.PretUnitar);
                solduriCub[lot.Id] = sold with { Credit = sold.Credit + c, Cantitate = sold.Cantitate - q };
                solduriRegistru[lot.Id] = registru with { Credit = registru.Credit + r, Cantitate = registru.Cantitate - q };
                decizii.Add(new N.ValoareIesire(l.Id, unitate, q, c));
            }
            valori.Add(new(l, unitate, q, r, c, p, produs));
            decizii.Add(new N.ContRezolvat(l.Id, unitate.Cont, "TipMaterial"));
        }
        if (refuzuri.Count > 0) return null;
        var totalR = valori.Sum(l => l.R);
        var totalP = valori.Sum(l => l.P);
        if (totalP != totalR) {
            refuzuri.Add(new(CoduriRefuz.AsamblareNebalansata,
                $"Consum R={totalR}; produse P={totalP}; diferență P−R={totalP - totalR}. Redistribuiți valoarea.", null));
            return null;
        }

        var grupuri = valori.GroupBy(l => l.Unitate.Cont).ToList();
        var feluri = grupuri.ToDictionary(g => g.Key,
            g => g.Sum(l => l.P) == g.Sum(l => l.R) ? N.FelTranzactie.Transfer : N.FelTranzactie.Operare);
        var tintaOperare = valori.LastOrDefault(l => l.Produs && feluri[l.Unitate.Cont] == N.FelTranzactie.Operare);
        var ajustari = new Dictionary<Guid, decimal>();
        foreach (var g in grupuri) {
            var r = g.Sum(l => l.R);
            var c = g.Sum(l => l.C);
            var delta = c - r;
            var tinta = g.LastOrDefault(l => l.Produs)
                ?? (feluri[g.Key] == N.FelTranzactie.Operare ? tintaOperare : null);
            if (tinta is null) {
                if (delta != 0m)
                    refuzuri.Add(new(CoduriRefuz.AsamblareDeltaFaraAncora,
                        $"Cont {g.Key}: Δ={delta}, fără produs eligibil pentru absorbție.", null));
                continue;
            }
            ajustari[tinta.Linie.Id] = ajustari.GetValueOrDefault(tinta.Linie.Id) + delta;
            decizii.Add(new N.AbsorbtieEvaluare(doc.Id, tinta.Linie.Id, g.Key, r, c, tinta.P, delta));
        }
        var transformari = new List<N.Transformare>();
        foreach (var l in valori) {
            var valoare = l.Produs ? l.P + ajustari.GetValueOrDefault(l.Linie.Id) : l.C;
            if (l.Produs && valoare <= 0m)
                refuzuri.Add(new(CoduriRefuz.AsamblareProdusNepozitiv,
                    $"Valoarea produsului după absorbție este {valoare}; trebuie să fie pozitivă.", l.Linie.Id));
            transformari.Add(new(new N.Capat {
                Cont = l.Unitate.Cont, Unitate = l.Unitate, Produs = l.Unitate.Produs,
                Gestiune = doc.Predator.Id, Analiza = l.Linie.Analiza,
            }, l.Produs ? N.RolTransformare.Produs : N.RolTransformare.Consum,
                feluri[l.Unitate.Cont], l.Q, valoare, new N.Cauza(doc.Id, l.Linie.Id)));
        }
        return refuzuri.Count > 0 ? null
            : new N.Declaratie(doc.Id, doc.DataInregistrare, [], [], transformari, decizii, ipoteze);
    }

    sealed record ValoriLinie(LinieOperand Linie, N.Unitate Unitate, decimal Q,
        decimal R, decimal C, decimal P, bool Produs);
}
