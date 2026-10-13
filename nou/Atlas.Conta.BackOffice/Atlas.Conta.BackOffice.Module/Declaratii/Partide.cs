#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

/// <summary>Partida documentului-sursă pe un cont, cu soldul ei citit.</summary>
readonly record struct PartidaSursa(N.UnitateSold Unitate, N.Sold Sold);

/// <summary>Deschiderea și nominalizarea partidelor pe conturile cu urmărire activă.</summary>
static class Partide {
    public static bool Urmareste(Operand operand, Guid cont) =>
        operand.Conturi.GetValueOrDefault(cont)?.UrmarestePartide == true;

    public static N.UnitateSold Proprie(Operand operand, Guid cont, Guid partener) =>
        N.UnitateSold.DeschidePartida(
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
            Cub.IdentitatiPartide.Gaseste(operand.UnitatiSursa, sursaId, cont, partener)
            ?? N.UnitateSold.DeschidePartida(cont, partener, sursaId,
                operand.DataInregistrareSursa ?? operand.Document.DataInregistrare),
            net >= 0m ? new N.Sold(net, 0m, 0m, 0m) : new N.Sold(0m, -net, 0m, 0m));
    }

    public static void Numeste(Operand operand, Guid cont, Guid partener, Guid linie,
            Dictionary<Guid, N.UnitateSold> partide, List<N.Decizie> decizii) {
        if (partide.ContainsKey(cont) || !Urmareste(operand, cont))
            return;
        var partida = Proprie(operand, cont, partener);
        partide.Add(cont, partida);
        decizii.Add(new N.PartidaDeschisa(linie, partida));
    }

}

/// <summary>
/// Capătul de terț (D9-A10): partida când contul o urmărește, terțul când contul
/// declară feluri de repartitor; prezența și felul le judecă gardul. Capătul care
/// are deja repartitor rămâne neatins.
/// </summary>
static class Terti {
    public static N.Capat Capat(Operand operand, N.Capat capat, RepartitorFapt tert, N.UnitateSold? partida) {
        ArgumentNullException.ThrowIfNull(capat);
        ArgumentNullException.ThrowIfNull(tert);
        // C111-Q14: locul intern pe care titularul îl scoate din repartitor rămâne pe postare.
        if (partida is not null)
            return capat with {
                Repartitor = tert.Coordonata, Unitate = partida,
                Loc = capat.Loc ?? (capat.Repartitor is { EsteIntern: true } loc ? loc.Id : null),
            };
        return capat.Repartitor is null && DeclaraRepartitor(operand, capat)
            ? capat with { Repartitor = tert.Coordonata }
            : capat;
    }

    /// <summary>Partida se pune pe capătul care n-are deja unitate: lotul recepției nu e partidă.</summary>
    public static N.Capat Capat(
            Operand operand, N.Capat capat, RepartitorFapt tert, IReadOnlyDictionary<Guid, N.UnitateSold> partide) {
        ArgumentNullException.ThrowIfNull(capat);
        return Capat(operand, capat, tert, capat.Unitate is null ? partide.GetValueOrDefault(capat.Cont) : null);
    }

    static bool DeclaraRepartitor(Operand operand, N.Capat capat) =>
        capat.Carte == N.Carte.Contabil
        && operand.Conturi.GetValueOrDefault(capat.Cont)?.DeclaraRepartitor == true;
}
