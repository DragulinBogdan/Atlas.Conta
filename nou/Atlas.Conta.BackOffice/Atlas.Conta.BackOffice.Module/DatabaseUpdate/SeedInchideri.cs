using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.DatabaseUpdate;

/// <summary>Rândurile închiderii conturilor de rezultat, comune profilurilor (E3-D4).</summary>
internal static class SeedInchideri {
    public sealed record RandInchidereConturi(int Ordine, string Sursa, string Destinatie, MomentInchidere Moment,
        bool PastreazaRepartitor = false, DimensiuneFlags Pastrate = DimensiuneFlags.Niciuna);

    /// <summary>Numerotarea IRZ și rândurile închiderii conturilor (E3-D4); cheia rândului e sursa × momentul.</summary>
    internal static void Conturi(IObjectSpace os, params RandInchidereConturi[] randuri) {
        ContaSeeder.SeedNumerotare(os, "IRZ", "IRZ-");
        foreach (var r in randuri) {
            var sursa = os.FirstOrDefault<Cont>(c => c.Simbol == r.Sursa)
                ?? throw new InvalidOperationException($"Seed-ul închiderii conturilor cere contul {r.Sursa}.");
            var destinatie = os.FirstOrDefault<Cont>(c => c.Simbol == r.Destinatie)
                ?? throw new InvalidOperationException($"Seed-ul închiderii conturilor cere contul {r.Destinatie}.");
            ContaSeeder.Aliniaza<PoliticaInchidereConturi>(os, $"{r.Sursa}/{r.Moment}",
                p => p.ContSursaId == sursa.ID && p.Moment == r.Moment, p => {
                    p.Ordine = r.Ordine;
                    p.Moment = r.Moment;
                    p.ContSursaId = sursa.ID;
                    p.ContDestinatieId = destinatie.ID;
                    p.PastreazaRepartitor = r.PastreazaRepartitor;
                    p.CoordonatePastrate = r.Pastrate;
                });
        }
    }
}
