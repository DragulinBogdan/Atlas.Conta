using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api.Imo;

// Fișa și registrul imobilizărilor (F26-D10).
// CONTRACT DE APELANT: ambele rulează pe ușa NON-SECURED, cu verdictul de acces luat
// înainte, pe cea securizată (F22-D5, 73g).
public static class ImobilizariApply {

    public static FisaImobilizareDto Fisa(IObjectSpace os, Guid id, DateOnly laData) {
        var h = os.GetObjectsQuery<Imobilizare>()
            .Where(f => f.ID == id)
            .Select(f => new {
                f.ID, f.NumarInventar, f.Denumire, f.Stare,
                f.DataPunereInFunctiune, f.DataIesire,
                TipMaterialCod = f.TipMaterial.Cod, TipMaterialDenumire = f.TipMaterial.Denumire,
                ClasificareCod = f.Clasificare.Cod, ClasificareDenumire = f.Clasificare.Denumire,
                DurataMinAni = (int?)f.Clasificare.DurataMinAni,
                DurataMaxAni = (int?)f.Clasificare.DurataMaxAni,
                LocDenumire = f.Loc.Denumire, CentruCostDenumire = f.CentruCost.Denumire,
                CodEconomicCod = f.CodEconomic.Cod, ResponsabilNume = f.Responsabil.Denumire
            })
            .FirstOrDefault();
        if (h == null)
            return null;

        var randuri = Randuri(os, [id], laData);
        var situatie = AmortizareService.Situatie(randuri.Select(r => r.Rand), laData);
        var coduriTip = ApiProiectii.CoduriTip(os, randuri.Select(r => r.DocumentId).Distinct().ToList());

        return new FisaImobilizareDto {
            Id = h.ID, NumarInventar = h.NumarInventar, Denumire = h.Denumire,
            TipMaterialCod = h.TipMaterialCod, TipMaterialDenumire = h.TipMaterialDenumire,
            ClasificareCod = h.ClasificareCod, ClasificareDenumire = h.ClasificareDenumire,
            LocDenumire = h.LocDenumire, CentruCostDenumire = h.CentruCostDenumire,
            CodEconomicCod = h.CodEconomicCod, ResponsabilNume = h.ResponsabilNume,
            Stare = h.Stare.ToString(),
            DataPunereInFunctiune = h.DataPunereInFunctiune, DataIesire = h.DataIesire,
            DurataFiscalaMinLuni = h.DurataMinAni * 12,
            DurataFiscalaMaxLuni = h.DurataMaxAni * 12,
            LaData = laData,
            Situatie = Proiecteaza(situatie),
            Randuri = randuri
                .OrderBy(r => r.Rand.Data).ThenBy(r => r.Rand.Fel).ThenBy(r => r.Rand.ID)
                .Select(r => new RandImobilizareDto {
                    Id = r.Rand.ID, Data = r.Rand.Data, Fel = r.Rand.Fel.ToString(), Storno = r.Rand.Storno,
                    DocumentId = r.DocumentId, DocumentNumar = r.DocumentNumar,
                    DocumentTip = coduriTip.GetValueOrDefault(r.DocumentId),
                    Valoare = r.Rand.Valoare, ValoareFiscala = r.Rand.ValoareFiscala,
                    Amortizare = r.Rand.Amortizare, AmortizareFiscala = r.Rand.AmortizareFiscala,
                    AmortizareDeductibila = r.Rand.AmortizareDeductibila, Luni = r.Rand.Luni,
                    Metoda = r.Rand.Metoda?.ToString(), DurataLuni = r.Rand.DurataLuni,
                    ValoareReziduala = r.Rand.ValoareReziduala,
                    MetodaFiscala = r.Rand.MetodaFiscala?.ToString(),
                    DurataFiscalaLuni = r.Rand.DurataFiscalaLuni,
                    CategorieFiscala = r.Rand.CategorieFiscala?.ToString(),
                    UtilizareExclusiva = r.Rand.UtilizareExclusiva
                })
                .ToList()
        };
    }

    public static RegistruImobilizariDto Registru(IObjectSpace os, DateOnly laData) {
        var fise = os.GetObjectsQuery<Imobilizare>()
            .Where(f => f.DataPunereInFunctiune != null)
            .Select(f => new {
                f.ID, f.NumarInventar, f.Denumire, f.Stare,
                TipMaterialCod = f.TipMaterial.Cod, LocDenumire = f.Loc.Denumire
            })
            .ToList();

        // 42c: o singură citire pentru toate fișele, apoi `Situatie` în memorie.
        var perFisa = Randuri(os, fise.Select(f => f.ID).ToList(), laData)
            .GroupBy(r => r.Rand.ImobilizareId)
            .ToDictionary(g => g.Key, g => g.Select(r => r.Rand).ToList());

        var linii = new List<RandRegistruImobilizariDto>();
        foreach (var f in fise.OrderBy(f => f.NumarInventar, StringComparer.Ordinal)) {
            var s = AmortizareService.Situatie(perFisa.GetValueOrDefault(f.ID) ?? [], laData);
            linii.Add(new RandRegistruImobilizariDto {
                ImobilizareId = f.ID, NumarInventar = f.NumarInventar, Denumire = f.Denumire,
                TipMaterialCod = f.TipMaterialCod, LocDenumire = f.LocDenumire,
                Stare = f.Stare.ToString(),
                Brut = s.Valoare, BrutFiscal = s.ValoareFiscala,
                AmortizareCumulata = s.Amortizare, AmortizareFiscalaCumulata = s.AmortizareFiscala,
                DeductibilCumulat = s.AmortizareDeductibila,
                NetContabil = s.NetContabil, NetFiscal = s.NetFiscal,
                Luni = s.Luni
            });
        }

        return new RegistruImobilizariDto {
            LaData = laData,
            Linii = linii,
            TotalBrut = linii.Sum(l => l.Brut),
            TotalBrutFiscal = linii.Sum(l => l.BrutFiscal),
            TotalAmortizareCumulata = linii.Sum(l => l.AmortizareCumulata),
            TotalAmortizareFiscalaCumulata = linii.Sum(l => l.AmortizareFiscalaCumulata),
            TotalDeductibilCumulat = linii.Sum(l => l.DeductibilCumulat),
            TotalNetContabil = linii.Sum(l => l.NetContabil),
            TotalNetFiscal = linii.Sum(l => l.NetFiscal)
        };
    }

    static SituatieImobilizareDto Proiecteaza(SituatieImobilizare s) => new() {
        Valoare = s.Valoare, ValoareFiscala = s.ValoareFiscala,
        Amortizare = s.Amortizare, AmortizareFiscala = s.AmortizareFiscala,
        AmortizareDeductibila = s.AmortizareDeductibila,
        NetContabil = s.NetContabil, NetFiscal = s.NetFiscal,
        Luni = s.Luni,
        Metoda = s.Metoda?.ToString(), DurataLuni = s.DurataLuni,
        ValoareReziduala = s.ValoareReziduala,
        MetodaFiscala = s.MetodaFiscala?.ToString(), DurataFiscalaLuni = s.DurataFiscalaLuni,
        CategorieFiscala = s.CategorieFiscala?.ToString(), UtilizareExclusiva = s.UtilizareExclusiva,
        DataUltimEveniment = s.DataUltimEveniment
    };

    sealed record RandCuDocument(RandRegistru Rand, Guid DocumentId, string DocumentNumar);

    static List<RandCuDocument> Randuri(IObjectSpace os, List<Guid> fise, DateOnly panaLa) {
        if (fise.Count == 0)
            return [];
        return os.GetObjectsQuery<RegistruImobilizari>()
            .Where(r => fise.Contains(r.ImobilizareId) && r.Data <= panaLa)
            .Select(r => new {
                r.ID, r.ImobilizareId, r.Data, r.Fel, r.Storno, r.DetaliuId,
                r.Valoare, r.ValoareFiscala, r.Amortizare, r.AmortizareFiscala,
                r.AmortizareDeductibila, r.Luni,
                r.Metoda, r.DurataLuni, r.ValoareReziduala,
                r.MetodaFiscala, r.DurataFiscalaLuni, r.CategorieFiscala, r.UtilizareExclusiva,
                r.DocumentId, DocumentNumar = r.Document.Numar
            })
            .ToList()
            .Select(r => new RandCuDocument(
                new RandRegistru(r.ID, r.ImobilizareId, r.Data, r.Fel, r.Storno, r.DetaliuId,
                    r.Valoare, r.ValoareFiscala, r.Amortizare, r.AmortizareFiscala,
                    r.AmortizareDeductibila, r.Luni, r.Metoda, r.DurataLuni, r.ValoareReziduala,
                    r.MetodaFiscala, r.DurataFiscalaLuni, r.CategorieFiscala, r.UtilizareExclusiva),
                r.DocumentId, r.DocumentNumar))
            .ToList();
    }
}
