#nullable enable
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

public sealed class DeclarantNotaContabila : IDeclarant {
    public static readonly DeclarantNotaContabila Instanta = new(PoliticaProfil.Niciuna);
    public static readonly DeclarantNotaContabila InchidereTva = new(PoliticaProfil.InchidereTva);
    DeclarantNotaContabila(PoliticaProfil politica) => PoliticaCeruta = politica;
    public PoliticaProfil PoliticaCeruta { get; }

    public N.Declaratie? Declara(Operand operand, N.Rotunjire rotunjire, ICollection<N.Refuz> refuzuri) {
        ArgumentNullException.ThrowIfNull(operand);
        ArgumentNullException.ThrowIfNull(rotunjire);
        ArgumentNullException.ThrowIfNull(refuzuri);
        if (operand.Linii.Count == 0)
            refuzuri.Add(new(CoduriRefuz.LiniiLipsa, "Nota contabilă cere cel puțin o linie.", null));
        foreach (var l in operand.Linii) {
            if (l.ContDebitId is not Guid d || !operand.Conturi.ContainsKey(d)
                || l.ContCreditId is not Guid c || !operand.Conturi.ContainsKey(c))
                refuzuri.Add(new(CoduriRefuz.ContExplicitLipsa, "Linia cere ambele conturi explicite.", l.Id));
            if (l.Valoare == 0m)
                refuzuri.Add(new(CoduriRefuz.ValoareZero, "Valoarea liniei trebuie să fie nenulă.", l.Id));
            foreach (var id in new[] { l.RepartitorDebitId, l.RepartitorCreditId }.OfType<Guid>())
                if (!operand.Repartitori.TryGetValue(id, out var r) || r.Parte is null)
                    refuzuri.Add(new(CoduriRefuz.RepartitorExplicitLipsa, "Repartitorul explicit nu poate fi rezolvat.", l.Id));
        }
        if (refuzuri.Count > 0) return null;

        var miscari = new List<N.Miscare>();
        var decizii = new List<N.Decizie>();
        var ipoteze = new List<N.Ipoteza> { operand.PerioadaDeschisa };
        var solduri = operand.PartideDisponibile.ToDictionary(p => p.Unitate.Id, p => Math.Sign(p.Sold.Net) * p.Disponibil);
        var citite = new HashSet<Guid>();
        var deschise = new HashSet<Guid>();
        foreach (var l in operand.Linii) {
            var debit = Nominalizeaza(l, l.ContDebitId!.Value, l.RepartitorDebitId,
                operand.Document.Predator, N.Latura.Debit);
            var credit = Nominalizeaza(l, l.ContCreditId!.Value, l.RepartitorCreditId,
                operand.Document.Primitor, N.Latura.Credit);
            var i = 0; var j = 0;
            var rd = debit[0].Suma; var rc = credit[0].Suma;
            while (i < debit.Count && j < credit.Count) {
                var suma = Math.Min(rd, rc);
                miscari.Add(new(credit[j].Capat, debit[i].Capat, 0m, 0m,
                    Math.Sign(l.Valoare) * suma, new(operand.Document.Id, l.Id)));
                rd -= suma; rc -= suma;
                if (rd == 0 && ++i < debit.Count) rd = debit[i].Suma;
                if (rc == 0 && ++j < credit.Count) rc = credit[j].Suma;
            }
        }
        return new(operand.Document.Id, operand.Document.DataInregistrare, miscari, decizii, ipoteze);

        List<(N.Capat Capat, decimal Suma)> Nominalizeaza(LinieOperand linie, Guid cont,
                Guid? repartitorId, RepartitorFapt implicitul, N.Latura latura) {
            var repartitor = repartitorId is Guid id ? operand.Repartitori[id] : implicitul;
            var externul = repartitorId != null && repartitor.Parte == Parte.Extern;
            var capat = new N.Capat {
                Cont = cont, Analiza = linie.Analiza,
                Gestiune = repartitor.Parte is Parte.Intern or Parte.Propriu ? repartitor.Id : null,
                Partener = externul ? repartitor.Id : null,
            };
            var cerere = Math.Abs(linie.Valoare);
            decizii.Add(new N.ContRezolvat(linie.Id, cont, "explicit"));
            if (!externul || !Partide.Urmareste(operand, cont)) return [(capat, cerere)];
            var sens = (latura == N.Latura.Debit ? 1 : -1) * Math.Sign(linie.Valoare);
            var candidati = new List<N.Disponibil>();
            foreach (var p in operand.PartideDisponibile.Where(p =>
                         p.Unitate.Cont == cont && p.Unitate.Partener == repartitor.Id)) {
                if (citite.Add(p.Unitate.Id)) ipoteze.Add(new N.SoldUnitateCitit(p.Unitate, p.Sold));
                var rest = -sens * solduri[p.Unitate.Id];
                if (rest > 0m) candidati.Add(new(p.Unitate, rest, p.Origine));
            }
            var alocare = N.Fifo.Nominalizeaza(cerere, candidati);
            var rezultat = new List<(N.Capat Capat, decimal Suma)>();
            foreach (var a in alocare.Alocari) {
                rezultat.Add((capat with { Unitate = a.Unitate, Partener = a.Unitate.Partener }, a.Masura));
                solduri[a.Unitate.Id] += sens * a.Masura;
                decizii.Add(new N.AlocareFifo(linie.Id, a.Unitate, a.Masura));
            }
            if (alocare.Ramas > 0m) {
                var proprie = Partide.Proprie(operand, cont, repartitor.Id);
                rezultat.Add((capat with { Unitate = proprie }, alocare.Ramas));
                if (deschise.Add(proprie.Id)) decizii.Add(new N.PartidaDeschisa(linie.Id, proprie));
            }
            return rezultat;
        }
    }
}
