#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

public sealed class DeclarantReturFurnizor : IDeclarantCuDiferente {
    public static readonly DeclarantReturFurnizor Instanta = new();
    public string SursaValoareDeclarata => SurseValoare.Linie;
    DeclarantReturFurnizor() { }
    public bool ConteazaPrinReguli => true;
    public PoliticaProfil PoliticaCeruta => PoliticaProfil.Contare;

    public N.Declaratie? Declara(Operand operand, N.Rotunjire rotunjire, ICollection<N.Refuz> refuzuri) {
        ArgumentNullException.ThrowIfNull(operand);
        ArgumentNullException.ThrowIfNull(rotunjire);
        ArgumentNullException.ThrowIfNull(refuzuri);
        if (operand.Linii.Count == 0)
            refuzuri.Add(new(CoduriRefuz.LiniiLipsa, "Returul cere cel puțin o linie.", null));
        var contari = new ContareLinie?[operand.Linii.Count];
        var fiscale = new TipTvaFapt?[operand.Linii.Count];
        for (var i = 0; i < operand.Linii.Count; i++) {
            var l = operand.Linii[i];
            if (l.Lot is null)
                refuzuri.Add(new(CoduriRefuz.LotLipsa, "Returul descarcă lotul original.", l.Id));
            if (l.Cantitate == 0m)
                refuzuri.Add(new(CoduriRefuz.CantitateZero, "Cantitatea returnată nu poate fi zero.", l.Id));
            if (l.Lot?.TipMaterialId is Guid tipLot && tipLot != l.TipMaterialId)
                refuzuri.Add(new(CoduriRefuz.ProdusAltTip, "Lotul aparține altui Tip decât linia.", l.Id));
            contari[i] = Contari.Rezolva(operand, l, refuzuri);
            if (operand.PoliticaTva is null || l.TipTvaId is not Guid id) continue;
            if (!operand.TipuriTva.TryGetValue(id, out var tip))
                refuzuri.Add(new(CoduriRefuz.TipTvaLipsa, "Tipul de TVA nu poate fi rezolvat.", l.Id));
            else if (tip.Regim == RegimTva.Capitalizat)
                refuzuri.Add(new(CoduriRefuz.TvaCapitalizat, "TVA capitalizat nu este admis pe retur.", l.Id));
            else fiscale[i] = tip;
        }
        if (refuzuri.Count > 0) return null;
        var taxa = Fiscal.Taxa(operand, fiscale, rotunjire, refuzuri);
        if (refuzuri.Count > 0) return null;
        var doc = operand.Document;
        var miscari = new List<N.Miscare>();
        var decizii = new List<N.Decizie>();
        var partide = new Dictionary<Guid, N.UnitateSold>();
        var consumate = new List<N.VersiunePolitica?>();
        var ramas = new Dictionary<CheieLotFapt, (decimal Cantitate, decimal Valoare)>();
        for (var i = 0; i < operand.Linii.Count; i++) {
            var l = operand.Linii[i]; var lot = l.Lot!; var contare = contari[i]!.Value;
            Contari.Decide(contare, l.Id, decizii);
            var intern = new N.Capat {
                Cont = contare.ContDebit, Repartitor = doc.Predator.Coordonata, Produs = lot.ProdusId,
                Unitate = new N.UnitateSold(lot.Id, N.FelUnitate.Lot, contare.ContDebit, null, lot.ProdusId, lot.Data),
                Analiza = Contari.Analiza(l.Analiza, contare.Regula.OverrideDebit, contare.Regula.Comun),
            };
            var tert = new N.Capat {
                Cont = contare.ContCredit, Contrapondere = N.Contrapondere.Tert, Produs = lot.ProdusId,
                Analiza = Contari.Analiza(l.Analiza, contare.Regula.OverrideCredit, contare.Regula.Comun),
            };
            var stoc = intern;
            if (fiscale[i] is { } tip) intern = Fiscal.CuFapt(operand, intern, tip, N.RolTva.Baza);
            // T-D6: nota fiscală păstrează q × prețul lotului; reziduul golirii trece pe politica de diferențe (T-r2).
            miscari.Add(new(tert, intern, l.Cantitate, 0m, l.Valoare, new(doc.Id, l.Id)));
            var corectie = lot.CotaCorectie(l.Cantitate);
            if (corectie != 0m)
                miscari.Add(new(tert, stoc, 0m, 0m, corectie, new(doc.Id, l.Id)));
            var cheie = new CheieLotFapt(lot.Id, contare.ContDebit, lot.ProdusId, doc.Predator.Id);
            var (cantitate, valoare) = ramas.TryGetValue(cheie, out var inainte) ? inainte
                : operand.SolduriLoturi.GetValueOrDefault(cheie) is { } sold ? (sold.Cantitate, sold.Debit - sold.Credit) : (0m, 0m);
            (cantitate, valoare) = (cantitate + l.Cantitate, valoare + l.Valoare + corectie);
            var reziduu = 0m;
            if (l.Cantitate < 0m && cantitate == 0m && valoare != 0m) {
                var cauza = valoare > 0m ? CauzaDiferentei.Neimputabila : CauzaDiferentei.Plus;
                var politici = operand.PoliticiDiferenta.Where(p => p.Clasa == l.ClasaId && p.Cauza == cauza).ToList();
                if (politici.Count != 1 || !operand.Conturi.ContainsKey(politici[0].Cont)) {
                    refuzuri.Add(new(CoduriRefuz.ReziduuPoliticaLipsa,
                        $"Golirea lotului lasă un reziduu de {valoare}; clasa și cauza „{cauza}” cer o singură politică de diferență.", l.Id));
                    continue;
                }
                var politica = politici[0];
                consumate.Add(PoliticiConsumate.Versiunea(politica));
                var rezultat = new N.Capat { Cont = politica.Cont, Repartitor = doc.Predator.Coordonata, Analiza = stoc.Analiza };
                miscari.Add(valoare > 0m ? new(stoc, rezultat, 0m, 0m, valoare, new(doc.Id, l.Id))
                    : new(rezultat, stoc, 0m, 0m, -valoare, new(doc.Id, l.Id)));
                decizii.Add(new N.ContRezolvat(l.Id, politica.Cont, $"Reziduu la golirea lotului: {cauza}"));
                (reziduu, valoare) = (valoare, 0m);
            }
            ramas[cheie] = (cantitate, valoare);
            if (l.Cantitate < 0m)
                decizii.Add(new N.ValoareDeclarata(l.Id, intern.Unitate!, -l.Cantitate, -l.Valoare, SurseValoare.Linie, -corectie, reziduu));
            Partide.Numeste(operand, intern.Cont, doc.Primitor.Id, l.Id, partide, decizii);
            Partide.Numeste(operand, tert.Cont, doc.Primitor.Id, l.Id, partide, decizii);
            var impozit = Fiscal.Impozitul(operand, l, fiscale[i], taxa, DirectieTva.Deductibil, refuzuri);
            if (impozit is not null) {
                miscari.Add(impozit);
                Partide.Numeste(operand, impozit.DeLa.Cont, doc.Primitor.Id, l.Id, partide, decizii);
                Partide.Numeste(operand, impozit.La.Cont, doc.Primitor.Id, l.Id, partide, decizii);
            }
        }
        if (refuzuri.Count > 0) return null;
        return new(doc.Id, doc.DataInregistrare, [.. miscari.Select(m => m with {
            DeLa = Terti.Capat(operand, m.DeLa, doc.Primitor, partide),
            La = Terti.Capat(operand, m.La, doc.Primitor, partide),
        })], decizii, PoliticiConsumate.Ipoteze(operand, decizii, [PoliticiConsumate.Fiscala(operand, fiscale), .. consumate]));
    }
}
