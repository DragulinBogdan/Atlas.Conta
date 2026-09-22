#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

/// <summary>
/// FCL (T-D4): venitul per linie pe regula de vânzare a Tipului, creanța pe contul
/// de terț al debitului și taxa COLECTATĂ per linie; zero postări de stoc —
/// descărcarea de gestiune e document propriu (DSC). Fiecare cont cu rol de terț
/// atins de linii primește o partidă (creanța, iar pe regularizarea de avans 419).
/// </summary>
public sealed class DeclarantFacturaIesire : IDeclarant {
    public static readonly DeclarantFacturaIesire Instanta = new();

    DeclarantFacturaIesire() { }

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
            var aleLiniei = new List<N.Miscare> { Venitul(operand, linie, contari[i]!.Value, tipuri[i], decizii) };
            if (Fiscal.Impozitul(operand, linie, tipuri[i], taxa, DirectieTva.Colectat, refuzuri) is { } impozit)
                aleLiniei.Add(impozit);
            // S-D16: partidă pe FIECARE cont cu `RolTert` al liniei — creanța și avansul.
            foreach (var miscare in aleLiniei) {
                Partide.Numeste(operand, miscare.DeLa.Cont, doc.Primitor.Id, linie.Id, partide, decizii);
                Partide.Numeste(operand, miscare.La.Cont, doc.Primitor.Id, linie.Id, partide, decizii);
            }
            miscari.AddRange(aleLiniei);
        }
        if (refuzuri.Count > 0)
            return null;
        return new N.Declaratie(
            doc.Id,
            doc.DataInregistrare,
            [.. miscari.Select(m => m with {
                DeLa = Partide.CuPartida(m.DeLa, doc.Primitor.Id, partide),
                La = Partide.CuPartida(m.La, doc.Primitor.Id, partide),
            })],
            decizii,
            [operand.PerioadaDeschisa, operand.VersiunePolitica]);
    }

    static void Antetul(DocumentFapt doc, Operand operand, ICollection<N.Refuz> refuzuri) {
        if (operand.Linii.Count == 0)
            refuzuri.Add(new N.Refuz(CoduriRefuz.LiniiLipsa,
                "Factura de ieșire se cere cu cel puțin o linie.", null));
    }

    // T-D4: FCL n-are recepție și n-are stoc — linia de natură `Stoc` e linie de VENIT
    // prin regula de vânzare a Tipului, deci nu cere nici lot, nici produs.
    static TipTvaFapt? Linia(Operand operand, LinieOperand linie, ContareLinie?[] contari, int indice,
            ICollection<N.Refuz> refuzuri) {
        if (linie.Cantitate <= 0m)
            refuzuri.Add(new N.Refuz(CoduriRefuz.CantitateNepozitiva,
                "Cantitatea fiecărei linii de factură trebuie să fie pozitivă.", linie.Id));
        var tipProdus = linie.TipProdusCulesId ?? linie.Lot?.TipMaterialId;
        if (tipProdus is Guid alProdusului && alProdusului != linie.TipMaterialId)
            refuzuri.Add(new N.Refuz(CoduriRefuz.ProdusAltTip,
                "Produsul liniei aparține altui Tip decât Tipul liniei.", linie.Id));
        contari[indice] = Contari.Rezolva(operand, linie, refuzuri);
        if (operand.PoliticaTva is null || linie.TipTvaId is not Guid tipTva)
            return null;
        if (!operand.TipuriTva.TryGetValue(tipTva, out var tip)) {
            refuzuri.Add(new N.Refuz(CoduriRefuz.TipTvaLipsa,
                "Tipul de TVA al liniei nu mai există în nomenclator.", linie.Id));
            return null;
        }
        return tip;
    }

    static N.Miscare Venitul(Operand operand, LinieOperand linie, ContareLinie contare, TipTvaFapt? tip,
            List<N.Decizie> decizii) {
        var doc = operand.Document;
        decizii.Add(new N.ContRezolvat(linie.Id, contare.ContDebit, contare.SursaDebit.ToString()));
        decizii.Add(new N.ContRezolvat(linie.Id, contare.ContCredit, contare.SursaCredit.ToString()));
        var intern = new N.Capat {
            Cont = contare.ContCredit,
            Gestiune = doc.Predator.Id,
            Produs = linie.Lot?.ProdusId,
            Analiza = Contari.Analiza(linie.Analiza, contare.Regula.OverrideCredit, contare.Regula.Comun),
        };
        var tert = new N.Capat {
            Cont = contare.ContDebit,
            Produs = linie.Lot?.ProdusId,
            Analiza = Contari.Analiza(linie.Analiza, contare.Regula.OverrideDebit, contare.Regula.Comun),
        };
        return new N.Miscare(
            tip is { } fiscal ? Fiscal.CuFapt(operand, intern, fiscal, N.RolTva.Baza) : intern,
            tert,
            0m,
            0m,
            linie.Valoare,
            new N.Cauza(doc.Id, linie.Id));
    }
}
