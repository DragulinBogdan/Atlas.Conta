#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

public sealed class DeclarantDiferenteInventar : IDeclarantCuDiferente {
    public static readonly DeclarantDiferenteInventar Instanta = new();
    DeclarantDiferenteInventar() { }
    public bool ConteazaPrinReguli => true;

    public N.Declaratie? Declara(Operand operand, N.Rotunjire rotunjire, ICollection<N.Refuz> refuzuri) {
        ArgumentNullException.ThrowIfNull(operand);
        ArgumentNullException.ThrowIfNull(rotunjire);
        ArgumentNullException.ThrowIfNull(refuzuri);
        var doc = operand.Document;
        var proprii = operand.Linii.Select(l => l.Id).ToHashSet();
        var miscari = new List<N.Miscare>();
        var decizii = new List<N.Decizie>();
        var ipoteze = new List<N.Ipoteza>();
        var reguliStoc = new List<N.VersiunePolitica>();
        var partide = new HashSet<Guid>();
        var solduri = new Dictionary<CheieLotFapt, N.Sold>();
        if (operand.Linii.Count == 0)
            refuzuri.Add(new(CoduriRefuz.LiniiLipsa, "Lista de inventar cere cel puțin o linie.", null));
        foreach (var culeasa in operand.Linii) {
            var initial = refuzuri.Count;
            void Refuza(string cod, string mesaj) => refuzuri.Add(new(cod, mesaj, culeasa.Id));
            if (culeasa.DiferentaInventar?.Directie is not (DirectieDiferenta.Plus or DirectieDiferenta.Minus)) {
                Refuza(CoduriRefuz.InventarStructuraInvalida, "Linia cere direcția Plus sau Minus.");
                continue;
            }
            var plus = culeasa.DiferentaInventar.Directie == DirectieDiferenta.Plus;
            var q = Math.Abs(culeasa.Cantitate);
            var linie = culeasa with { Cantitate = plus ? q : -q };
            if (q == 0m) Refuza(CoduriRefuz.CantitateNepozitiva, "Cantitatea absolută trebuie să fie pozitivă.");
            if (linie.Natura != NaturaClasa.Stoc)
                Refuza(CoduriRefuz.NaturaNepotrivita, "Diferența de inventar cere natura Stoc.");
            if (linie.Lot is not { } lot) {
                Refuza(CoduriRefuz.LotLipsa, "Linia de inventar cere un lot.");
                continue;
            }
            if (lot.TipMaterialId != linie.TipMaterialId || linie.TipProdusCulesId is Guid tip && tip != linie.TipMaterialId
                    || linie.ProdusId is Guid produs && produs != lot.ProdusId)
                Refuza(CoduriRefuz.ProdusAltTip, "Tipul și produsul liniei trebuie să corespundă lotului.");
            if (plus) {
                if (lot.LinieIntrareId != linie.Id || lot.GestiuneId != doc.Predator.Id)
                    Refuza(CoduriRefuz.InventarStructuraInvalida, "Plusul cere lot propriu în gestiunea inventariată.");
                if (linie.DiferentaInventar!.PretEvaluare is not > 0m)
                    Refuza(CoduriRefuz.ValoareNepozitiva, "Plusul cere preț de evaluare pozitiv.");
            }
            else if (lot.LinieIntrareId is Guid origine && proprii.Contains(origine))
                Refuza(CoduriRefuz.InventarStructuraInvalida, "Minusul nu poate consuma un lot propriu documentului.");
            // 126: minusul fără cauză e neimputabil; plusul are o singură cauză.
            var cauza = plus ? CauzaDiferentei.Plus : culeasa.DiferentaInventar.Cauza ?? CauzaDiferentei.Neimputabila;
            if (plus ? culeasa.DiferentaInventar.Cauza is not (null or CauzaDiferentei.Plus)
                    : cauza is not (CauzaDiferentei.Perisabilitate or CauzaDiferentei.Imputabila or CauzaDiferentei.Neimputabila))
                Refuza(CoduriRefuz.InventarCauza, "Cauza diferenței nu este permisă pe această direcție a inventarului.");
            var imputat = cauza == CauzaDiferentei.Imputabila && culeasa.DiferentaInventar.Imputat is Guid cine
                ? operand.Repartitori.GetValueOrDefault(cine) : null;
            if (cauza == CauzaDiferentei.Imputabila && imputat?.Fel is not (N.FelRepartitor.Partener or N.FelRepartitor.Angajat))
                Refuza(CoduriRefuz.InventarCauza, "Lipsa imputabilă cere partenerul sau angajatul imputat pe linie.");
            // 126: cheltuiala și plusul rămân pe derivarea tipului din regula de contare; imputarea vine din politică.
            var politici = cauza == CauzaDiferentei.Imputabila
                ? operand.PoliticiDiferenta.Where(p => p.Clasa == linie.ClasaId && p.Cauza == cauza).ToList() : null;
            if (politici is { Count: not 1 })
                Refuza(CoduriRefuz.InventarPolitica, "Lipsa imputabilă cere o singură politică de diferență pe clasă.");
            var stoc = Potrivire.Stoc(operand.ReguliStoc, linie.Fapt).SelectMany(p => p.Reguli).ToArray();
            if (stoc.Length != 1 || stoc[0].Latura != LaturaDocument.Predator || stoc[0].Semn != 1
                    || stoc[0].TipStoc is not (TipStoc.Magazie or TipStoc.Marfuri or TipStoc.Folosinta))
                Refuza(CoduriRefuz.InventarStocNeacoperit, "Lista cere o singură regulă Magazie/Marfuri/Folosinta, +1 pe gestiunea inventariată.");
            var contare = Contari.Rezolva(operand, linie, refuzuri);
            if (refuzuri.Count != initial) continue;
            reguliStoc.Add(PoliticiConsumate.Versiunea(stoc[0]));
            var cont = contare!.Value;
            var contDiferenta = plus ? cont.ContCredit : cont.ContDebit;
            if (politici?[0] is { } politica) {
                reguliStoc.Add(PoliticiConsumate.Versiunea(politica));
                contDiferenta = imputat?.Fel == N.FelRepartitor.Angajat ? politica.ContPersonal ?? politica.Cont : politica.Cont;
                if (!operand.Conturi.ContainsKey(contDiferenta) || !Partide.Urmareste(operand, contDiferenta)) {
                    Refuza(CoduriRefuz.InventarPolitica, "Imputarea cere un cont existent, cu urmărire pe partide.");
                    continue;
                }
            }
            var contStoc = plus ? cont.ContDebit : cont.ContCredit;
            if (contStoc != lot.ContImplicitId) {
                Refuza(CoduriRefuz.ContStocLipsa, "Contul de stoc rezolvat trebuie să fie contul lotului.");
                continue;
            }
            var cheie = new CheieLotFapt(lot.Id, contStoc, lot.ProdusId, doc.Predator.Id);
            var unitate = new N.UnitateSold(lot.Id, N.FelUnitate.Lot, contStoc, null, lot.ProdusId, lot.Data);
            decimal v;
            if (plus) {
                v = rotunjire.Bani(q * linie.DiferentaInventar!.PretEvaluare!.Value);
            }
            else {
                if (!solduri.TryGetValue(cheie, out var sold)) {
                    sold = operand.SolduriLoturi.GetValueOrDefault(cheie) ?? N.Sold.Zero;
                    ipoteze.Add(new N.SoldUnitateCitit(unitate, sold));
                }
                try { v = N.Evaluare.Iesire(sold, q, rotunjire); }
                catch (N.RefuzException e) { Refuza(e.Refuz.Cod, e.Refuz.Mesaj); continue; }
                solduri[cheie] = sold with { Credit = sold.Credit + v, Cantitate = sold.Cantitate - q };
                decizii.Add(new N.ValoareIesire(linie.Id, unitate, q, v));
            }
            var real = new N.Capat { Cont = contStoc, Repartitor = doc.Predator.Coordonata, Produs = lot.ProdusId,
                Unitate = unitate, Analiza = Contari.Analiza(linie.Analiza,
                    plus ? cont.Regula.OverrideDebit : cont.Regula.OverrideCredit, cont.Regula.Comun) };
            N.UnitateSold? partida = imputat != null && Partide.Urmareste(operand, contDiferenta)
                ? Partide.Proprie(operand, contDiferenta, imputat.Id) : null;
            if (partida != null && partide.Add(partida.Id)) decizii.Add(new N.PartidaDeschisa(linie.Id, partida));
            var virtuala = new N.Capat { Cont = contDiferenta,
                Contrapondere = plus ? N.Contrapondere.Inventar : N.Contrapondere.Consum,
                Repartitor = imputat == null ? null : operand.Repartitor(imputat.Id), Unitate = partida,
                Produs = lot.ProdusId, Analiza = Contari.Analiza(linie.Analiza,
                    plus ? cont.Regula.OverrideCredit : cont.Regula.OverrideDebit, cont.Regula.Comun) };
            miscari.Add(new(plus ? virtuala : real, plus ? real : virtuala, q, 0m, v, new(doc.Id, linie.Id)));
            if (politici == null) Contari.Decide(cont, linie.Id, decizii);
            else {
                decizii.Add(new N.ContRezolvat(linie.Id, contStoc, (plus ? cont.SursaDebit : cont.SursaCredit).ToString(), cont.Regula.Id));
                decizii.Add(new N.ContRezolvat(linie.Id, contDiferenta, $"Diferență: {cauza}"));
            }
        }
        if (refuzuri.Count > 0) return null;
        ipoteze.AddRange(PoliticiConsumate.Ipoteze(operand, decizii, [.. reguliStoc]));
        return new(doc.Id, doc.DataInregistrare, miscari, decizii, ipoteze);
    }
}
