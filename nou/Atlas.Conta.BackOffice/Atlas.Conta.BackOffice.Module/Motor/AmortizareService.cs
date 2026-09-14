using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Motor;

/// <summary>Situația unei fișe la o dată: sumele registrului + parametrii ultimului eveniment.</summary>
public sealed record SituatieImobilizare(
    decimal Valoare, decimal ValoareFiscala,
    decimal Amortizare, decimal AmortizareFiscala, decimal AmortizareDeductibila,
    int Luni,
    MetodaAmortizare? Metoda, int? DurataLuni, decimal? ValoareReziduala,
    MetodaAmortizare? MetodaFiscala, int? DurataFiscalaLuni,
    CategorieFiscala? CategorieFiscala, bool? UtilizareExclusiva) {
    public decimal NetContabil => Valoare - Amortizare;
    public decimal NetFiscal => ValoareFiscala - AmortizareFiscala;
}

// Singura aritmetică a amortizării (F26-D7); pasul 1 poartă doar citirea registrului.
public static class AmortizareService {
    static readonly FelMiscareImobilizare[] Evenimente = [
        FelMiscareImobilizare.Intrare, FelMiscareImobilizare.Modernizare,
        FelMiscareImobilizare.Revizuire, FelMiscareImobilizare.Reevaluare,
    ];

    public static SituatieImobilizare Situatie(IObjectSpace os, Guid imobilizareId, DateOnly laData) {
        var randuri = os.GetObjectsQuery<RegistruImobilizari>()
            .Where(r => r.ImobilizareId == imobilizareId && r.Data <= laData)
            .Select(r => new {
                r.ID, r.Data, r.Fel, r.Storno, r.DetaliuId,
                r.Valoare, r.ValoareFiscala, r.Amortizare, r.AmortizareFiscala,
                r.AmortizareDeductibila, r.Luni,
                r.Metoda, r.DurataLuni, r.ValoareReziduala,
                r.MetodaFiscala, r.DurataFiscalaLuni, r.CategorieFiscala, r.UtilizareExclusiva,
            })
            .ToList();

        // Storno-ul copiază parametrii, deci evenimentul stornat iese și din rezolvarea lor (F26-D2).
        var stornate = randuri.Where(r => r.Storno).Select(r => r.DetaliuId).ToHashSet();
        var evenimente = randuri
            .Where(r => !r.Storno && Evenimente.Contains(r.Fel) && !stornate.Contains(r.DetaliuId))
            .OrderBy(r => r.Data).ThenBy(r => r.ID)
            .ToList();

        return new SituatieImobilizare(
            randuri.Sum(r => r.Valoare), randuri.Sum(r => r.ValoareFiscala),
            randuri.Sum(r => r.Amortizare), randuri.Sum(r => r.AmortizareFiscala),
            randuri.Sum(r => r.AmortizareDeductibila), randuri.Sum(r => r.Luni),
            Coalesce(evenimente, r => r.Metoda), Coalesce(evenimente, r => r.DurataLuni),
            Coalesce(evenimente, r => r.ValoareReziduala), Coalesce(evenimente, r => r.MetodaFiscala),
            Coalesce(evenimente, r => r.DurataFiscalaLuni), Coalesce(evenimente, r => r.CategorieFiscala),
            Coalesce(evenimente, r => r.UtilizareExclusiva));
    }

    // Coalesce ÎNAPOI: null pe un eveniment înseamnă „neschimbat” (F26-D2).
    static T? Coalesce<TRand, T>(List<TRand> evenimente, Func<TRand, T?> citeste) where T : struct {
        for (var i = evenimente.Count - 1; i >= 0; i--)
            if (citeste(evenimente[i]) is T valoare)
                return valoare;
        return null;
    }
}
