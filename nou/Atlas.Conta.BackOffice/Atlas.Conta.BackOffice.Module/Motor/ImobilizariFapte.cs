using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using DevExpress.ExpressApp;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Motor;

internal static class ImobilizariFapte {
    public static Operand Completeaza(IObjectSpace os, Operand operand) {
        var linii = operand.Linii.Where(l => l.Imobilizare != null).ToList();
        if (linii.Count == 0) return operand;
        var ids = linii.Select(l => l.Imobilizare.Fisa).Distinct().ToList();
        var fise = os.GetObjectsQuery<Imobilizare>().Where(f => ids.Contains(f.ID))
            .Select(f => new { f.ID, f.TipMaterialId, f.TipMaterial.ContImplicitId, f.LocId, f.DataPunereInFunctiune })
            .ToList();
        var tipuri = fise.Select(f => f.TipMaterialId).Distinct().ToList();
        var politici = os.GetObjectsQuery<PoliticaAmortizare>().Where(p => tipuri.Contains(p.TipMaterialId))
            .Select(p => new { p.ID, p.TipMaterialId, p.ContAmortizareId, p.ContCheltuialaAmortizareId, p.ContCheltuialaCedareId, p.OptimisticLockField })
            .ToDictionary(p => p.TipMaterialId);
        var situatii = C.Citiri.Imobilizari.Randuri(os, ids, operand.Document.DataInregistrare)
            .Where(r => r.DocumentId != operand.Document.Id).GroupBy(r => r.Rand.ImobilizareId)
            .ToDictionary(g => g.Key, g => AmortizareService.Situatie(g.Select(r => r.Rand), operand.Document.DataInregistrare));
        var conturiFise = C.Citiri.Imobilizari.Conturi(os, ids, operand.Document.DataInregistrare, operand.Document.Id);
        var locuri = Fapte.Repartitori(os, [.. fise.Select(f => f.LocId).Distinct()]);
        var rezultat = fise.ToDictionary(f => f.ID, f => {
            var p = politici.GetValueOrDefault(f.TipMaterialId);
            var s = situatii.GetValueOrDefault(f.ID);
            var conturi = conturiFise.GetValueOrDefault(f.ID);
            return new FisaFapt(f.ID, conturi?.Activ ?? f.ContImplicitId ?? Guid.Empty,
                conturi?.Amortizare ?? p?.ContAmortizareId ?? Guid.Empty,
                p?.ContCheltuialaAmortizareId ?? Guid.Empty, p?.ContCheltuialaCedareId ?? Guid.Empty,
                Fapte.Coordonata(locuri, f.LocId), f.DataPunereInFunctiune ?? operand.Document.Data,
                s?.ValoareFiscala ?? 0m, s?.AmortizareFiscala ?? 0m) {
                    Politica = p == null ? null : new N.VersiunePolitica(nameof(PoliticaAmortizare), p.ID, p.OptimisticLockField),
                };
        });
        if (!linii.Any(l => l.Imobilizare is PifCules { Fel: not FelLiniePif.Revizuire }))
            return operand with { Fise = rezultat };

        var conturi = rezultat.Values.SelectMany(f => new[] { f.Cont, f.ContAmortizare }).Distinct().ToList();
        var lipsa = conturi.Where(c => c != Guid.Empty && !operand.Conturi.ContainsKey(c)).ToList();
        if (lipsa.Count > 0) {
            var conturiOperand = operand.Conturi.ToDictionary();
            foreach (var (id, cont) in Fapte.Conturi(os, lipsa))
                conturiOperand[id] = cont;
            operand = operand with { Conturi = conturiOperand };
        }
        var randuri = C.Citiri.Imobilizari.PozitiiCitite(os, conturi, operand.Document.Id);
        var data = operand.Document.DataInregistrare;
        decimal Minim(IEnumerable<(DateOnly Data, decimal Delta)> delte) {
            decimal sold = 0m, minim = decimal.MaxValue;
            foreach (var zi in delte.GroupBy(d => d.Data <= data ? data : d.Data).OrderBy(g => g.Key)) {
                sold += zi.Sum(d => d.Delta);
                minim = Math.Min(minim, sold);
            }
            return minim == decimal.MaxValue ? 0m : minim;
        }
        var repartitori = Fapte.Repartitori(os, [.. randuri.Select(r => r.Repartitor).OfType<Guid>().Distinct()]);
        var postari = randuri.Select(r => new { Rand = r, Capat = new N.Capat {
            Cont = r.Cont, Repartitor = r.Repartitor is Guid id ? Fapte.Coordonata(repartitori, id) : null,
            Produs = r.Produs, Valuta = r.Valuta,
            Analiza = new(r.CodFunctional, r.CodEconomic, r.SursaFinantare, r.UnitateOrganizatorica, r.Proiect, r.CentruCost),
        }}).ToList();
        var disponibile = postari.GroupBy(p => p.Capat).Select(g => new DisponibilFapt(g.Key,
            Minim(g.Select(p => (p.Rand.Data, p.Rand.Latura == N.Sens.Debit ? p.Rand.Valoare : -p.Rand.Valoare))),
            Minim(g.Select(p => (p.Rand.Data, p.Rand.Latura == N.Sens.Credit ? p.Rand.Valoare : -p.Rand.Valoare)))))
            .ToList();
        var suporturi = postari.Where(p => !p.Rand.Stornata && p.Rand.Data <= data && p.Rand.Valoare > 0m
                && p.Rand.Fel is N.FelTranzactie.Operare or N.FelTranzactie.Deschidere)
            .Select(p => new SuportFapt(new(p.Rand.ID, p.Rand.Spatiu), p.Rand.LinieId, p.Capat, p.Rand.Latura,
                Minim(randuri.Where(r => r.ID == p.Rand.ID || r.SuportId == p.Rand.ID || r.InversaDinId == p.Rand.ID)
                    .Select(r => (r.Data, r.Valoare))), p.Rand.Data, p.Rand.FaraLinieSursa)).ToList();
        return operand with { Fise = rezultat, Suporturi = suporturi, DisponibilNominalizare = disponibile };
    }
}
