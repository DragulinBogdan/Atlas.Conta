#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

/// <summary>
/// FCT (B-D6): fiecare linie postează netul pe regula ei; linia de stoc e
/// RECEPȚIA facturii (TR-D3) — lotul intră în gestiunea primitoare de pe capătul
/// virtual al furnizorului. Taxa se decide per document × cotă și se postează
/// per linie, iar terțul are o singură partidă pe fiecare cont al lui.
/// </summary>
public sealed class DeclarantFacturaIntrare : IDeclarant {
    public static readonly DeclarantFacturaIntrare Instanta = new();

    DeclarantFacturaIntrare() { }
    public bool ConteazaPrinReguli => true;

    public N.Declaratie? Declara(Operand operand, N.Rotunjire rotunjire, ICollection<N.Refuz> refuzuri) {
        ArgumentNullException.ThrowIfNull(operand);
        ArgumentNullException.ThrowIfNull(rotunjire);
        ArgumentNullException.ThrowIfNull(refuzuri);

        var doc = operand.Document;
        Antetul(doc, operand, refuzuri);

        var contari = new ContareLinie?[operand.Linii.Count];
        var tipuri = new TipTvaFapt?[operand.Linii.Count];
        for (var i = 0; i < operand.Linii.Count; i++)
            tipuri[i] = Linia(operand, operand.Linii[i], contari, i, refuzuri);

        var taxa = Fiscal.Taxa(operand, tipuri, rotunjire, refuzuri);
        if (refuzuri.Count > 0)
            return null;

        var miscari = new List<N.Miscare>(operand.Linii.Count);
        var decizii = new List<N.Decizie>();
        var partide = new Dictionary<Guid, N.Unitate>();
        for (var i = 0; i < operand.Linii.Count; i++) {
            var linie = operand.Linii[i];
            var (intern, tert, cantitate) = Netul(operand, linie, contari[i]!.Value, decizii);
            var aleLiniei = Netele(operand, linie, intern, tert, cantitate, tipuri[i], rotunjire).ToList();
            if (Fiscal.Impozitul(operand, linie, tipuri[i], taxa, DirectieTva.Deductibil, refuzuri) is { } impozit)
                aleLiniei.Add(impozit);
            foreach (var miscare in aleLiniei) {
                Partide.Numeste(operand, miscare.DeLa.Cont, doc.Predator.Id, linie.Id, partide, decizii);
                Partide.Numeste(operand, miscare.La.Cont, doc.Predator.Id, linie.Id, partide, decizii);
            }
            miscari.AddRange(aleLiniei);
        }
        if (refuzuri.Count > 0)
            return null;
        return new N.Declaratie(
            doc.Id,
            doc.DataInregistrare,
            [.. miscari.Select(m => m with {
                DeLa = Terti.Capat(operand, m.DeLa, doc.Predator.Id, partide),
                La = Terti.Capat(operand, m.La, doc.Predator.Id, partide),
            })],
            decizii,
            PoliticiConsumate.Ipoteze(operand, decizii, PoliticiConsumate.Fiscala(operand, tipuri)));
    }

    static void Antetul(DocumentFapt doc, Operand operand, ICollection<N.Refuz> refuzuri) {
        if (string.IsNullOrWhiteSpace(doc.Numar))
            refuzuri.Add(new N.Refuz(CoduriRefuz.NumarLipsa,
                "Factura de intrare poartă numărul furnizorului.", null));
        if (operand.Linii.Count == 0)
            refuzuri.Add(new N.Refuz(CoduriRefuz.LiniiLipsa,
                "Factura de intrare se cere cu cel puțin o linie.", null));
    }

    static TipTvaFapt? Linia(Operand operand, LinieOperand linie, ContareLinie?[] contari, int indice,
            ICollection<N.Refuz> refuzuri) {
        if (linie.Cantitate <= 0m)
            refuzuri.Add(new N.Refuz(CoduriRefuz.CantitateNepozitiva,
                "Cantitatea fiecărei linii de factură trebuie să fie pozitivă.", linie.Id));
        var tipProdus = linie.TipProdusCulesId ?? linie.Lot?.TipMaterialId;
        if (tipProdus is Guid alProdusului && alProdusului != linie.TipMaterialId)
            refuzuri.Add(new N.Refuz(CoduriRefuz.ProdusAltTip,
                "Produsul liniei aparține altui Tip decât Tipul liniei.", linie.Id));
        var receptie = linie.Natura == NaturaClasa.Stoc;
        if (receptie && linie.Lot is null)
            refuzuri.Add(new N.Refuz(CoduriRefuz.LotLipsa,
                "Liniile de stoc ale facturii își creează lotul la culegere.", linie.Id));
        else {
            contari[indice] = Contari.Rezolva(operand, linie, refuzuri);
            if (receptie && contari[indice] is { } contare && contare.ContDebit != linie.Lot!.ContImplicitId)
                refuzuri.Add(new N.Refuz(CoduriRefuz.ContStocLipsa,
                    "Contul de recepție trebuie să fie contul lotului.", linie.Id));
        }
        if (operand.PoliticaTva is null || linie.TipTvaId is not Guid tipTva)
            return null;
        if (!operand.TipuriTva.TryGetValue(tipTva, out var tip)) {
            refuzuri.Add(new N.Refuz(CoduriRefuz.TipTvaLipsa,
                "Tipul de TVA al liniei nu mai există în nomenclator.", linie.Id));
            return null;
        }
        return tip;
    }

    static (N.Capat Intern, N.Capat Tert, decimal Cantitate) Netul(
            Operand operand, LinieOperand linie, ContareLinie contare, List<N.Decizie> decizii) {
        Contari.Decide(contare, linie.Id, decizii);
        var receptionat = linie.Natura == NaturaClasa.Stoc ? linie.Lot : null;
        return (
            new N.Capat {
                Cont = contare.ContDebit,
                Gestiune = operand.Document.Primitor.Id,
                Produs = linie.Lot?.ProdusId,
                Unitate = receptionat is null ? null : new N.Unitate(
                    receptionat.Id, N.FelUnitate.Lot, contare.ContDebit, null, receptionat.ProdusId, receptionat.Data),
                Analiza = Contari.Analiza(linie.Analiza, contare.Regula.OverrideDebit, contare.Regula.Comun),
            },
            new N.Capat {
                Cont = contare.ContCredit,
                // N-D4: cantitatea vine din afara evidenței, pe gestiunea structurală.
                Gestiune = receptionat is null ? null : N.GestiuniVirtuale.Furnizor,
                Produs = linie.Lot?.ProdusId,
                Analiza = Contari.Analiza(linie.Analiza, contare.Regula.OverrideCredit, contare.Regula.Comun),
            },
            receptionat is null ? 0m : linie.Cantitate);
    }

    // Capitalizatul e BRUT pe linie, dar jurnalul îl desface în bază + taxă,
    // iar jurnalul e proiecția pe `CodTva` (090a):
    // netul devine DOUĂ mișcări pe același cont de cost, cu Σ neschimbată.
    static IEnumerable<N.Miscare> Netele(Operand operand, LinieOperand linie, N.Capat intern, N.Capat tert,
            decimal cantitate, TipTvaFapt? tip, N.Rotunjire rotunjire) {
        var cauza = new N.Cauza(operand.Document.Id, linie.Id);
        var fiscal = tip is { } t ? Fiscal.CuFapt(operand, intern, t, N.RolTva.Baza) : intern;
        if (tip is not { Regim: RegimTva.Capitalizat } capitalizat) {
            yield return new N.Miscare(tert, fiscal, cantitate, 0m, linie.Valoare, cauza);
            yield break;
        }
        var baza = rotunjire.Bani(linie.Valoare / (1m + (capitalizat.Cota / 100m)));
        var taxa = linie.Valoare - baza;
        yield return new N.Miscare(tert, fiscal, cantitate, 0m, baza, cauza);
        if (taxa != 0m)
            yield return new N.Miscare(
                tert, Fiscal.CuFapt(operand, intern, capitalizat, N.RolTva.Taxa), 0m, 0m, taxa, cauza);
    }
}
