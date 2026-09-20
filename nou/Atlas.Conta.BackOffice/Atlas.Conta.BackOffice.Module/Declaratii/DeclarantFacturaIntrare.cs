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
        var taxa = Taxa(operand, tipuri, rotunjire, refuzuri, out var culese);
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
            if (Impozitul(operand, linie, tipuri[i], taxa, culese, refuzuri) is { } impozit)
                aleLiniei.Add(impozit);
            foreach (var miscare in aleLiniei)
                Numeste(operand, miscare.DeLa.Cont, linie, partide, decizii);
            miscari.AddRange(aleLiniei);
        }
        if (refuzuri.Count > 0)
            return null;
        return new N.Declaratie(
            doc.Id,
            doc.DataInregistrare,
            [.. miscari.Select(m => m with { DeLa = CuPartida(operand, m.DeLa, partide) })],
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
    // care nu e în operand; contrapartida vine de pe regula naturii care postează pe
    // factură (același furnizor, același fallback), altfel din politica de TVA.
    static RezolvareCont? Contrapartida(Operand operand, ICollection<N.Refuz> refuzuri) {
        foreach (var regula in operand.ReguliContare) {
            if (regula.NaturaFiltru is not (NaturaClasa.Serviciu or NaturaClasa.Cheltuiala))
                continue;
            var alRegulii = Potrivire.Cont(regula.SursaContCredit, regula.ContCreditId, null, operand.Laturi);
            if (alRegulii.ContId != null)
                return alRegulii;
        }
        if (operand.PoliticaTva is { } politica) {
            var alPoliticii = Potrivire.Cont(
                politica.SursaContrapartida, politica.ContrapartidaFallbackId, null, operand.Laturi);
            if (alPoliticii.ContId != null)
                return alPoliticii;
        }
        refuzuri.Add(new N.Refuz(CoduriRefuz.RegulaContareLipsa,
            "Recepția n-are cont de furnizor: nicio regulă de contare pe Serviciu/Cheltuiala și "
            + "nicio politică de TVA nu-l dau.", null));
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
        var fiscal = tip is { } t ? CuFapt(operand, intern, t, N.RolTva.Baza) : intern;
        if (tip is not { Regim: RegimTva.Capitalizat } capitalizat) {
            yield return new N.Miscare(tert, fiscal, cantitate, 0m, linie.Valoare, cauza);
            yield break;
        }
        var baza = rotunjire.Bani(linie.Valoare / (1m + (capitalizat.Cota / 100m)));
        var taxa = linie.Valoare - baza;
        yield return new N.Miscare(tert, fiscal, cantitate, 0m, baza, cauza);
        if (taxa != 0m)
            yield return new N.Miscare(
                tert, CuFapt(operand, intern, capitalizat, N.RolTva.Taxa), 0m, 0m, taxa, cauza);
    }

    static N.Miscare? Impozitul(Operand operand, LinieOperand linie, TipTvaFapt? tip, N.TaxaDocument? taxa,
            IReadOnlyCollection<(N.RegimTva Regim, decimal Cota)> culese, ICollection<N.Refuz> refuzuri) {
        if (tip is not { } fiscal || taxa is null
                || fiscal.Regim is not (RegimTva.Normal or RegimTva.TaxareInversa))
            return null;
        var politica = operand.PoliticaTva!;
        // F13-D1: autolichidarea e a BENEFICIARULUI; pe livrare taxarea inversă n-are taxă.
        if (fiscal.Regim == RegimTva.TaxareInversa && politica.Directie != DirectieTva.Deductibil)
            return null;
        if (politica.Directie != DirectieTva.Deductibil) {
            refuzuri.Add(new N.Refuz(CoduriRefuz.DirectieTvaNepotrivita,
                "Factura de intrare deduce: politica de TVA a tipului o declară colectată.", linie.Id));
            return null;
        }
        var valoare = culese.Contains(Cheia(fiscal)) ? linie.ValoareTva : taxa.PerLinie.GetValueOrDefault(linie.Id);
        if (valoare == 0m)
            return null;
        if (fiscal.ContTvaDeductibilId is not Guid contDeductibil) {
            refuzuri.Add(new N.Refuz(CoduriRefuz.ContTvaLipsa,
                $"Tipul de TVA {fiscal.Cod} n-are contul de TVA deductibilă.", linie.Id));
            return null;
        }
        var analiza = Contari.Analiza(linie.Analiza, null, null);
        var intern = CuFapt(operand, new N.Capat {
            Cont = contDeductibil,
            Gestiune = operand.Document.Primitor.Id,
            Produs = linie.Lot?.ProdusId,
            Analiza = analiza,
        }, fiscal, N.RolTva.Taxa);
        Guid? contTert = fiscal.Regim == RegimTva.TaxareInversa
            ? fiscal.ContTvaColectatId
            : Potrivire.Cont(politica.SursaContrapartida, politica.ContrapartidaFallbackId, null,
                operand.Laturi).ContId;
        if (contTert is not Guid cont) {
            refuzuri.Add(new N.Refuz(
                fiscal.Regim == RegimTva.TaxareInversa ? CoduriRefuz.ContTvaLipsa : CoduriRefuz.RegulaContareLipsa,
                "Contrapartida rândului de TVA nu se poate rezolva.", linie.Id));
            return null;
        }
        return new N.Miscare(
            new N.Capat { Cont = cont, Produs = linie.Lot?.ProdusId, Analiza = analiza },
            intern,
            0m,
            0m,
            valoare,
            new N.Cauza(operand.Document.Id, linie.Id));
    }

    // 090j: taxa CULEASĂ e autoritară pe cota ei, dar se validează contra celei
    // decise pe document; cota fără nicio taxă culeasă o primește pe a nucleului (N-r4).
    static N.TaxaDocument? Taxa(Operand operand, IReadOnlyList<TipTvaFapt?> tipuri, N.Rotunjire rotunjire,
            ICollection<N.Refuz> refuzuri, out IReadOnlyCollection<(N.RegimTva Regim, decimal Cota)> culese) {
        var chei = new List<(N.RegimTva Regim, decimal Cota)>();
        var date = new List<(N.RegimTva Regim, decimal Cota)>();
        var sume = new Dictionary<(N.RegimTva Regim, decimal Cota), decimal>();
        var linii = new List<N.LinieTva>();
        for (var i = 0; i < operand.Linii.Count; i++) {
            if (tipuri[i] is not { } tip)
                continue;
            var linie = operand.Linii[i];
            var cheie = Cheia(tip);
            linii.Add(new N.LinieTva(linie.Id, linie.Valoare, cheie.Regim, cheie.Cota));
            if (!chei.Contains(cheie))
                chei.Add(cheie);
            sume[cheie] = sume.GetValueOrDefault(cheie) + linie.ValoareTva;
            if (linie.ValoareTva != 0m && !date.Contains(cheie))
                date.Add(cheie);
        }
        culese = date;
        if (linii.Count == 0)
            return null;
        var taxa = N.Tva.PeDocument(linii, (N.DirectieTva)(int)operand.PoliticaTva!.Directie, rotunjire);
        foreach (var cheie in chei)
            if (date.Contains(cheie)
                    && N.Tva.ValideazaData(sume[cheie], taxa.PerCota[cheie], operand.TolerantaTaxa) is { } refuz)
                refuzuri.Add(refuz);
        return taxa;
    }

    // B-D8 pct. 5: faptul fiscal e atribut al postării interne — codul, perioada
    // declarării și partenerul pe care D394 îl cere pe faptă.
    static N.Capat CuFapt(Operand operand, N.Capat capat, TipTvaFapt tip, N.RolTva rol) => capat with {
        CodTva = new N.CodTva(tip.Id, Sensul(operand), rol),
        PerioadaDeclarare = operand.PerioadaDeclarare,
        Partener = operand.PoliticaTva!.SursaContrapartida switch {
            SursaCont.RepartitorPredator => operand.Document.Predator.Id,
            SursaCont.RepartitorPrimitor => operand.Document.Primitor.Id,
            _ => null,
        },
    };

    // 090h: UNA per cont de terț, deschisă de factură; pe conturile fără `RolTert`
    // (profilul bugetar) postarea rămâne fără unitate și fără partener (B-D8 pct. 10).
    static void Numeste(Operand operand, Guid cont, LinieOperand linie,
            Dictionary<Guid, N.Unitate> partide, List<N.Decizie> decizii) {
        if (partide.ContainsKey(cont)
                || operand.Conturi.GetValueOrDefault(cont)?.RolTert is null or RolTertCont.Niciunul)
            return;
        var partida = N.Unitate.DeschidePartida(cont, operand.Document.Predator.Id,
            operand.Document.Id, operand.Document.DataInregistrare);
        partide.Add(cont, partida);
        decizii.Add(new N.PartidaDeschisa(linie.Id, partida));
    }

    static N.Capat CuPartida(Operand operand, N.Capat tert, IReadOnlyDictionary<Guid, N.Unitate> partide) =>
        partide.TryGetValue(tert.Cont, out var partida)
            ? tert with { Partener = operand.Document.Predator.Id, Unitate = partida }
            : tert;

    static (N.RegimTva Regim, decimal Cota) Cheia(TipTvaFapt tip) => ((N.RegimTva)(int)tip.Regim, tip.Cota);

    static N.SensTva Sensul(Operand operand) =>
        operand.PoliticaTva!.Directie == DirectieTva.Colectat ? N.SensTva.Livrare : N.SensTva.Achizitie;
}
