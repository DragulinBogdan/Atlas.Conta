#nullable enable
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

/// <summary>Consumul unei linii, evaluat pe soldul cubului; <paramref name="SoldCitit"/> e nenul la prima atingere a unității.</summary>
public sealed record ConsumEvaluat(LinieOperand Linie, N.Unitate Unitate, decimal Cantitate, decimal Valoare, N.Sold? SoldCitit);

public sealed class DeclarantAsamblare : IDeclarant {
    public static readonly DeclarantAsamblare Instanta = new();
    DeclarantAsamblare() { }

    /// <summary>
    /// Consumurile evaluate în secvența liniilor (090 j). Refuză numai evaluarea însăși:
    /// lotul, contul lui de stoc, soldul. Nu judecă produsele și nici balansarea.
    /// </summary>
    public IReadOnlyList<ConsumEvaluat> Consum(Operand operand, N.Rotunjire rotunjire, ICollection<N.Refuz> refuzuri) {
        ArgumentNullException.ThrowIfNull(operand);
        ArgumentNullException.ThrowIfNull(rotunjire);
        ArgumentNullException.ThrowIfNull(refuzuri);
        var solduri = new Dictionary<CheieLotFapt, N.Sold>();
        var evaluate = new List<ConsumEvaluat>();
        foreach (var l in operand.Linii.Where(l => l.Transformare?.Rol == N.RolTransformare.Consum)) {
            if (l.Lot is not { } lot) {
                refuzuri.Add(new(CoduriRefuz.LotLipsa, "Linia cere un lot.", l.Id));
                continue;
            }
            if (lot.ContImplicitId is not Guid cont) {
                refuzuri.Add(new(CoduriRefuz.ContStocLipsa, "Lotul cere cont de stoc.", l.Id));
                continue;
            }
            var cheie = new CheieLotFapt(lot.Id, cont, lot.ProdusId, operand.Document.Predator.Id);
            var unitate = new N.Unitate(lot.Id, N.FelUnitate.Lot, cont, null, lot.ProdusId, lot.Data);
            N.Sold? citit = null;
            if (!solduri.TryGetValue(cheie, out var sold))
                citit = sold = operand.SolduriLoturi.GetValueOrDefault(cheie) ?? N.Sold.Zero;
            var q = Math.Abs(l.Cantitate);
            decimal c;
            try { c = N.Evaluare.Iesire(sold, q, rotunjire); }
            catch (N.RefuzException e) { refuzuri.Add(e.Refuz with { Linie = l.Id }); continue; }
            solduri[cheie] = sold with { Credit = sold.Credit + c, Cantitate = sold.Cantitate - q };
            evaluate.Add(new(l, unitate, q, c, citit));
        }
        return evaluate;
    }

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

        var consum = Consum(operand, rotunjire, refuzuri).ToDictionary(c => c.Linie.Id);
        if (refuzuri.Count > 0) return null;

        var decizii = new List<N.Decizie>();
        var ipoteze = new List<N.Ipoteza> { operand.PerioadaDeschisa, operand.VersiunePolitica };
        var valori = new List<ValoriLinie>();
        foreach (var l in linii) {
            if (consum.TryGetValue(l.Id, out var evaluat)) {
                if (evaluat.SoldCitit is { } citit)
                    ipoteze.Add(new N.SoldUnitateCitit(evaluat.Unitate, citit));
                decizii.Add(new N.ValoareIesire(l.Id, evaluat.Unitate, evaluat.Cantitate, evaluat.Valoare));
                valori.Add(new(l, evaluat.Unitate, evaluat.Cantitate, evaluat.Valoare, false));
            }
            else {
                var lot = l.Lot!;
                var unitate = new N.Unitate(lot.Id, N.FelUnitate.Lot, lot.ContImplicitId!.Value,
                    null, lot.ProdusId, lot.Data);
                var q = Math.Abs(l.Cantitate);
                var p = rotunjire.Bani(q * l.Transformare!.PretProdus!.Value);
                if (p <= 0m)
                    refuzuri.Add(new(CoduriRefuz.AsamblareProdusNepozitiv,
                        $"Valoarea produsului este {p}; trebuie să fie pozitivă.", l.Id));
                valori.Add(new(l, unitate, q, p, true));
            }
            decizii.Add(new N.ContRezolvat(l.Id, valori[^1].Unitate.Cont, "TipMaterial"));
        }
        if (refuzuri.Count > 0) return null;
        var totalC = valori.Where(l => !l.Produs).Sum(l => l.Valoare);
        var totalP = valori.Where(l => l.Produs).Sum(l => l.Valoare);
        if (totalP != totalC) {
            refuzuri.Add(new(CoduriRefuz.AsamblareNebalansata,
                $"Consum C={totalC}; produse P={totalP}; diferență P−C={totalP - totalC}. Redistribuiți valoarea.", null));
            return null;
        }

        var feluri = valori.GroupBy(l => l.Unitate.Cont).ToDictionary(g => g.Key,
            g => g.Where(l => l.Produs).Sum(l => l.Valoare) == g.Where(l => !l.Produs).Sum(l => l.Valoare)
                ? N.FelTranzactie.Transfer : N.FelTranzactie.Operare);
        var transformari = valori.Select(l => new N.Transformare(new N.Capat {
                Cont = l.Unitate.Cont, Unitate = l.Unitate, Produs = l.Unitate.Produs,
                Gestiune = doc.Predator.Id, Analiza = l.Linie.Analiza,
            }, l.Produs ? N.RolTransformare.Produs : N.RolTransformare.Consum,
            feluri[l.Unitate.Cont], l.Cantitate, l.Valoare, new N.Cauza(doc.Id, l.Linie.Id))).ToList();
        return new N.Declaratie(doc.Id, doc.DataInregistrare, [], [], transformari, decizii, ipoteze);
    }

    sealed record ValoriLinie(LinieOperand Linie, N.Unitate Unitate, decimal Cantitate, decimal Valoare, bool Produs);
}
