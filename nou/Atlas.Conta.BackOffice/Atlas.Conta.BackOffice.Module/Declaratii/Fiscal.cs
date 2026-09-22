#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

/// <summary>
/// Faptul fiscal al unui document cu TVA, comun oricărui declarant: taxa decisă
/// pe document × cotă (090j), taxa culeasă ca autoritate PER LINIE, atributele
/// fiscale ale postării (B-D8 pct. 5) și mișcarea de taxă pe direcția politicii.
/// </summary>
static class Fiscal {
    /// <summary>
    /// Taxa nucleului per document × cotă, validată contra celei culese CÂND politica
    /// declară o toleranță: Σ per cotă a valorii ALESE pe fiecare linie, cu toleranța
    /// per linie înmulțită cu liniile cotei (MEDIU-4). Fără toleranță (S-D15) taxa
    /// culeasă e autoritară fără gard. <c>null</c> = documentul n-are linie cu TVA.
    /// </summary>
    public static N.TaxaDocument? Taxa(Operand operand, IReadOnlyList<TipTvaFapt?> tipuri,
            N.Rotunjire rotunjire, ICollection<N.Refuz> refuzuri) {
        ArgumentNullException.ThrowIfNull(operand);
        ArgumentNullException.ThrowIfNull(tipuri);
        ArgumentNullException.ThrowIfNull(refuzuri);
        var linii = new List<N.LinieTva>();
        var aleLor = new List<(LinieOperand Linie, (N.RegimTva Regim, decimal Cota) Cheie)>();
        var chei = new List<(N.RegimTva Regim, decimal Cota)>();
        for (var i = 0; i < operand.Linii.Count; i++) {
            if (tipuri[i] is not { } tip)
                continue;
            var linie = operand.Linii[i];
            var cheie = Cheia(tip);
            linii.Add(new N.LinieTva(linie.Id, linie.Valoare, cheie.Regim, cheie.Cota));
            aleLor.Add((linie, cheie));
            if (!chei.Contains(cheie))
                chei.Add(cheie);
        }
        if (linii.Count == 0)
            return null;
        var taxa = N.Tva.PeDocument(linii, (N.DirectieTva)(int)operand.PoliticaTva!.Directie, rotunjire);
        if (operand.TolerantaTaxa is not decimal toleranta)
            return taxa;
        foreach (var cheie in chei) {
            var aleCheii = aleLor.Where(a => a.Cheie == cheie).ToList();
            var refuz = N.Tva.ValideazaData(
                aleCheii.Sum(a => Valoarea(a.Linie, taxa)),
                taxa.PerCota[cheie],
                toleranta * aleCheii.Count);
            if (refuz is not null)
                refuzuri.Add(refuz);
        }
        return taxa;
    }

    /// <summary>
    /// 090j: taxa CULEASĂ e autoritară, PER LINIE ca azi (<c>pastreazaTvaCules</c>);
    /// linia lăsată la zero o primește pe a nucleului (N-r4).
    /// </summary>
    public static decimal Valoarea(LinieOperand linie, N.TaxaDocument taxa) {
        ArgumentNullException.ThrowIfNull(linie);
        ArgumentNullException.ThrowIfNull(taxa);
        return linie.ValoareTva != 0m ? linie.ValoareTva : taxa.PerLinie.GetValueOrDefault(linie.Id);
    }

    /// <summary>
    /// B-D8 pct. 5: faptul fiscal e atribut al postării interne — codul, perioada
    /// declarării și partenerul pe care D394 îl cere pe faptă.
    /// </summary>
    public static N.Capat CuFapt(Operand operand, N.Capat capat, TipTvaFapt tip, N.RolTva rol) {
        ArgumentNullException.ThrowIfNull(operand);
        ArgumentNullException.ThrowIfNull(capat);
        return capat with {
            CodTva = new N.CodTva(tip.Id, Sensul(operand), rol),
            PerioadaDeclarare = operand.PerioadaDeclarare,
            Partener = operand.PoliticaTva!.SursaContrapartida switch {
                SursaCont.RepartitorPredator => operand.Document.Predator.Id,
                SursaCont.RepartitorPrimitor => operand.Document.Primitor.Id,
                _ => null,
            },
        };
    }

    /// <summary>
    /// Gestiunea internă a documentului: latura OPUSĂ contrapartidei politicii de TVA.
    /// </summary>
    public static Guid GestiuneaInterna(Operand operand) {
        ArgumentNullException.ThrowIfNull(operand);
        return operand.PoliticaTva?.SursaContrapartida == SursaCont.RepartitorPrimitor
            ? operand.Document.Predator.Id
            : operand.Document.Primitor.Id;
    }

    /// <summary>
    /// Mișcarea de taxă a liniei: contul de TVA al direcției contra contrapartidei
    /// politicii; la taxare inversă contrapartida e contul colectat (4426 = 4427).
    /// </summary>
    public static N.Miscare? Impozitul(Operand operand, LinieOperand linie, TipTvaFapt? tip,
            N.TaxaDocument? taxa, DirectieTva asteptata, ICollection<N.Refuz> refuzuri) {
        ArgumentNullException.ThrowIfNull(operand);
        ArgumentNullException.ThrowIfNull(linie);
        ArgumentNullException.ThrowIfNull(refuzuri);
        if (tip is not { } fiscal || taxa is null
                || fiscal.Regim is not (RegimTva.Normal or RegimTva.TaxareInversa))
            return null;
        var politica = operand.PoliticaTva!;
        // F13-D1: autolichidarea e a BENEFICIARULUI; pe livrare taxarea inversă n-are taxă.
        if (fiscal.Regim == RegimTva.TaxareInversa && politica.Directie != DirectieTva.Deductibil)
            return null;
        if (politica.Directie != asteptata) {
            refuzuri.Add(new N.Refuz(CoduriRefuz.DirectieTvaNepotrivita,
                $"Documentul cere TVA {asteptata}: politica de TVA a tipului o declară {politica.Directie}.",
                linie.Id));
            return null;
        }
        var valoare = Valoarea(linie, taxa);
        if (valoare == 0m)
            return null;
        var alDirectiei = asteptata == DirectieTva.Deductibil
            ? fiscal.ContTvaDeductibilId
            : fiscal.ContTvaColectatId;
        if (alDirectiei is not Guid contTva) {
            refuzuri.Add(new N.Refuz(CoduriRefuz.ContTvaLipsa,
                $"Tipul de TVA {fiscal.Cod} n-are contul de TVA {asteptata}.", linie.Id));
            return null;
        }
        var analiza = Contari.Analiza(linie.Analiza, null, null);
        var intern = CuFapt(operand, new N.Capat {
            Cont = contTva,
            Gestiune = GestiuneaInterna(operand),
            Produs = linie.Lot?.ProdusId,
            Analiza = analiza,
        }, fiscal, N.RolTva.Taxa);
        Guid? alTertului = fiscal.Regim == RegimTva.TaxareInversa
            ? fiscal.ContTvaColectatId
            : Potrivire.Cont(politica.SursaContrapartida, politica.ContrapartidaFallbackId, null,
                operand.Laturi).ContId;
        if (alTertului is not Guid cont) {
            refuzuri.Add(new N.Refuz(
                fiscal.Regim == RegimTva.TaxareInversa ? CoduriRefuz.ContTvaLipsa : CoduriRefuz.RegulaContareLipsa,
                "Contrapartida rândului de TVA nu se poate rezolva.", linie.Id));
            return null;
        }
        var contrapartida = new N.Capat { Cont = cont, Produs = linie.Lot?.ProdusId, Analiza = analiza };
        // T-D4: pe `Colectat` taxa e CREDITUL (4427) contra contrapartidei debitoare (4111).
        var (deLa, la) = asteptata == DirectieTva.Colectat
            ? (intern, contrapartida)
            : (contrapartida, intern);
        return new N.Miscare(deLa, la, 0m, 0m, valoare, new N.Cauza(operand.Document.Id, linie.Id));
    }

    public static (N.RegimTva Regim, decimal Cota) Cheia(TipTvaFapt tip) =>
        ((N.RegimTva)(int)tip.Regim, tip.Cota);

    static N.SensTva Sensul(Operand operand) =>
        operand.PoliticaTva!.Directie == DirectieTva.Colectat ? N.SensTva.Livrare : N.SensTva.Achizitie;
}
