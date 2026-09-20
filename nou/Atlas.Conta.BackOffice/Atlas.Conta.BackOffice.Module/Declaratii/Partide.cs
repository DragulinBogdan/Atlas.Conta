#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

/// <summary>Partida documentului-sursă pe un cont, cu soldul ei citit.</summary>
readonly record struct PartidaSursa(N.Unitate Unitate, N.Sold Sold);

/// <summary>
/// Partidele terților, comune oricărui declarant cu terț: se deschid DOAR pe
/// conturile cu <c>RolTert</c> (B-D8 pct. 10) și se nominalizează pe partida
/// documentului-sursă, în limita a ce ține EA (TR-D2a).
/// </summary>
static class Partide {
    public static bool ARolTert(Operand operand, Guid cont) =>
        operand.Conturi.GetValueOrDefault(cont)?.RolTert is not (null or RolTertCont.Niciunul);

    public static N.Unitate Proprie(Operand operand, Guid cont, Guid partener) =>
        N.Unitate.DeschidePartida(
            cont, partener, operand.Document.Id, operand.Document.DataInregistrare);

    /// <summary>
    /// Partida sursei pe contul cerut și soldul ei REAL: restul documentului-sursă
    /// e altceva decât ce ține partida lui pe un cont anume (MAJOR-1). Fără rând pe
    /// cont soldul e zero — citirea rămâne consemnată ca ipoteză.
    /// </summary>
    public static PartidaSursa? Sursa(Operand operand, Guid cont, Guid partener) {
        if (operand.Document is not { Autogenerat: true, DocumentSursaId: Guid sursaId })
            return null;
        var net = 0m;
        foreach (var (alContului, sold) in operand.PartideSursa)
            if (alContului == cont)
                net = sold;
        return new PartidaSursa(
            N.Unitate.DeschidePartida(cont, partener, sursaId,
                operand.DataInregistrareSursa ?? operand.Document.DataInregistrare),
            net >= 0m ? new N.Sold(net, 0m, 0m, 0m) : new N.Sold(0m, -net, 0m, 0m));
    }

    /// <summary>
    /// 090h: UNA per cont de terț, deschisă de document; pe conturile fără
    /// <c>RolTert</c> (profilul bugetar) postarea rămâne fără unitate și fără partener.
    /// </summary>
    public static void Numeste(Operand operand, Guid cont, Guid partener, Guid linie,
            Dictionary<Guid, N.Unitate> partide, List<N.Decizie> decizii) {
        if (partide.ContainsKey(cont) || !ARolTert(operand, cont))
            return;
        var partida = Proprie(operand, cont, partener);
        partide.Add(cont, partida);
        decizii.Add(new N.PartidaDeschisa(linie, partida));
    }

    public static N.Capat CuPartida(
            N.Capat tert, Guid partener, IReadOnlyDictionary<Guid, N.Unitate> partide) {
        ArgumentNullException.ThrowIfNull(tert);
        return partide.TryGetValue(tert.Cont, out var partida)
            ? tert with { Partener = partener, Unitate = partida }
            : tert;
    }
}
