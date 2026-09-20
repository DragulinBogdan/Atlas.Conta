#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

/// <summary>
/// FCT (B-D6): linia de stoc e RECEPȚIA facturii (TR-D3) — lotul intră în
/// gestiunea primitoare de pe capătul virtual al furnizorului; celelalte naturi
/// postează netul pe regula lor. Taxa se decide per document × cotă și se
/// postează per linie, iar terțul are o singură partidă pe fiecare cont al lui.
/// </summary>
public sealed class DeclarantFacturaIntrare : IDeclarant {
    public static readonly DeclarantFacturaIntrare Instanta = new();

    DeclarantFacturaIntrare() { }

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

        var receptia = operand.Linii.Any(l => l.Natura == NaturaClasa.Stoc)
            ? Contrapartida(operand, refuzuri)
            : null;
        var taxa = Fiscal.Taxa(operand, tipuri, rotunjire, refuzuri);
        if (refuzuri.Count > 0)
            return null;

        var miscari = new List<N.Miscare>(operand.Linii.Count);
        var decizii = new List<N.Decizie>();
        var partide = new Dictionary<Guid, N.Unitate>();
        for (var i = 0; i < operand.Linii.Count; i++) {
            var linie = operand.Linii[i];
            var (intern, tert, cantitate) = linie.Natura == NaturaClasa.Stoc
                ? Receptia(operand, linie, receptia!.Value, decizii)
                : Netul(operand, linie, contari[i]!.Value, decizii);
            var aleLiniei = Netele(operand, linie, intern, tert, cantitate, tipuri[i], rotunjire).ToList();
            if (Fiscal.Impozitul(operand, linie, tipuri[i], taxa, DirectieTva.Deductibil, refuzuri) is { } impozit)
                aleLiniei.Add(impozit);
            foreach (var miscare in aleLiniei)
                Partide.Numeste(operand, miscare.DeLa.Cont, doc.Predator.Id, linie.Id, partide, decizii);
            miscari.AddRange(aleLiniei);
        }
        if (refuzuri.Count > 0)
            return null;
        return new N.Declaratie(
            doc.Id,
            doc.DataInregistrare,
            [.. miscari.Select(m => m with { DeLa = Partide.CuPartida(m.DeLa, doc.Predator.Id, partide) })],
            decizii,
            [operand.PerioadaDeschisa, operand.VersiunePolitica]);
    }

    static void Antetul(DocumentFapt doc, Operand operand, ICollection<N.Refuz> refuzuri) {
        if (string.IsNullOrWhiteSpace(doc.Numar))
            refuzuri.Add(new N.Refuz(CoduriRefuz.NumarLipsa,
                "Factura de intrare poartă numărul furnizorului.", null));
        if (doc.Predator.Fel != FelRepartitor.Partener)
            refuzuri.Add(new N.Refuz(CoduriRefuz.PredatorNepotrivit,
                "Predatorul facturii de intrare trebuie să fie un partener (furnizor).", null));
        if (doc.Primitor.Fel != FelRepartitor.Gestiune)
            refuzuri.Add(new N.Refuz(CoduriRefuz.PrimitorNepotrivit,
                "Primitorul facturii de intrare trebuie să fie o gestiune.", null));
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
        if (linie.Natura == NaturaClasa.Stoc) {
            if (linie.Lot is null)
                refuzuri.Add(new N.Refuz(CoduriRefuz.LotLipsa,
                    "Liniile de stoc ale facturii își creează lotul la culegere.", linie.Id));
            else if (linie.ContImplicitTipId is null)
                refuzuri.Add(new N.Refuz(CoduriRefuz.RegulaContareLipsa,
                    "Tipul liniei de stoc n-are cont implicit, deci recepția n-are cont.", linie.Id));
        }
        else
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

    // TR-D3: recepția e a facturii, dar regula ei de contare e a NIR-ului conex,
    // care nu e în operand; contrapartida e cea DECLARATĂ de politica de TVA a
    // tipului, iar regula naturii care postează pe factură rămâne rezerva (MINOR-5).
    static RezolvareCont? Contrapartida(Operand operand, ICollection<N.Refuz> refuzuri) {
        if (operand.PoliticaTva is { } politica) {
            var alPoliticii = Potrivire.Cont(
                politica.SursaContrapartida, politica.ContrapartidaFallbackId, null, operand.Laturi);
            if (alPoliticii.ContId != null)
                return alPoliticii;
        }
        foreach (var regula in operand.ReguliContare) {
            if (regula.NaturaFiltru is not (NaturaClasa.Serviciu or NaturaClasa.Cheltuiala))
                continue;
            var alRegulii = Potrivire.Cont(regula.SursaContCredit, regula.ContCreditId, null, operand.Laturi);
            if (alRegulii.ContId != null)
                return alRegulii;
        }
        refuzuri.Add(new N.Refuz(CoduriRefuz.RegulaContareLipsa,
            "Recepția n-are cont de furnizor: nicio politică de TVA și nicio regulă de contare pe "
            + "Serviciu/Cheltuiala nu-l dau.", null));
        return null;
    }

    static (N.Capat Intern, N.Capat Tert, decimal Cantitate) Receptia(
            Operand operand, LinieOperand linie, RezolvareCont contrapartida, List<N.Decizie> decizii) {
        var lot = linie.Lot!;
        var contStoc = linie.ContImplicitTipId!.Value;
        var contTert = contrapartida.ContId!.Value;
        // Recepția n-are regulă proprie: rămâne coalesce-ul liniei, ca pe NIR.
        var analiza = Contari.Analiza(linie.Analiza, null, null);
        decizii.Add(new N.ContRezolvat(linie.Id, contStoc, SursaRezolvata.TipMaterial.ToString()));
        decizii.Add(new N.ContRezolvat(linie.Id, contTert, contrapartida.Sursa.ToString()));
        return (
            new N.Capat {
                Cont = contStoc,
                Gestiune = operand.Document.Primitor.Id,
                Produs = lot.ProdusId,
                Unitate = new N.Unitate(lot.Id, N.FelUnitate.Lot, contStoc, null, lot.ProdusId, lot.Data),
                Analiza = analiza,
            },
            new N.Capat {
                Cont = contTert,
                // N-D4: cantitatea vine din afara evidenței, pe gestiunea structurală.
                Gestiune = N.GestiuniVirtuale.Furnizor,
                Produs = lot.ProdusId,
                Analiza = analiza,
            },
            linie.Cantitate);
    }

    static (N.Capat Intern, N.Capat Tert, decimal Cantitate) Netul(
            Operand operand, LinieOperand linie, ContareLinie contare, List<N.Decizie> decizii) {
        decizii.Add(new N.ContRezolvat(linie.Id, contare.ContDebit, contare.SursaDebit.ToString()));
        decizii.Add(new N.ContRezolvat(linie.Id, contare.ContCredit, contare.SursaCredit.ToString()));
        return (
            new N.Capat {
                Cont = contare.ContDebit,
                Gestiune = operand.Document.Primitor.Id,
                Produs = linie.Lot?.ProdusId,
                Analiza = Contari.Analiza(linie.Analiza, contare.Regula.OverrideDebit, contare.Regula.Comun),
            },
            new N.Capat {
                Cont = contare.ContCredit,
                Produs = linie.Lot?.ProdusId,
                Analiza = Contari.Analiza(linie.Analiza, contare.Regula.OverrideCredit, contare.Regula.Comun),
            },
            0m);
    }

    // Capitalizatul e BRUT pe linie, dar jurnalul îl desface în bază + taxă
    // (`RegistruTvaService.Cifre`), iar jurnalul e proiecția pe `CodTva` (090a):
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
