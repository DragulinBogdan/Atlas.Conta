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
            .Select(p => new { p.TipMaterialId, p.ContAmortizareId, p.ContCheltuialaAmortizareId, p.ContCheltuialaCedareId })
            .ToDictionary(p => p.TipMaterialId);
        var situatii = C.Citiri.Imobilizari.Randuri(os, ids, operand.Document.DataInregistrare)
            .Where(r => r.DocumentId != operand.Document.Id).GroupBy(r => r.Rand.ImobilizareId)
            .ToDictionary(g => g.Key, g => AmortizareService.Situatie(g.Select(r => r.Rand), operand.Document.DataInregistrare));
        var conturiFise = C.Citiri.Imobilizari.Conturi(os, ids, operand.Document.DataInregistrare, operand.Document.Id);
        var rezultat = fise.ToDictionary(f => f.ID, f => {
            var p = politici.GetValueOrDefault(f.TipMaterialId);
            var s = situatii.GetValueOrDefault(f.ID);
            var conturi = conturiFise.GetValueOrDefault(f.ID);
            return new FisaFapt(f.ID, conturi?.Activ ?? f.ContImplicitId ?? Guid.Empty,
                conturi?.Amortizare ?? p?.ContAmortizareId ?? Guid.Empty,
                p?.ContCheltuialaAmortizareId ?? Guid.Empty, p?.ContCheltuialaCedareId ?? Guid.Empty,
                f.LocId, f.DataPunereInFunctiune ?? operand.Document.Data,
                s?.ValoareFiscala ?? 0m, s?.AmortizareFiscala ?? 0m);
        });
        if (!linii.Any(l => l.Imobilizare is PifCules { Fel: not FelLiniePif.Revizuire }))
            return operand with { Fise = rezultat };

        var conturi = rezultat.Values.SelectMany(f => new[] { f.Cont, f.ContAmortizare }).Distinct().ToList();
        var lipsa = conturi.Where(c => c != Guid.Empty && !operand.Conturi.ContainsKey(c)).ToList();
        if (lipsa.Count > 0) {
            var conturiOperand = operand.Conturi.ToDictionary();
            foreach (var c in os.GetObjectsQuery<Cont>().Where(c => lipsa.Contains(c.ID))
                    .Select(c => new { c.ID, c.Simbol, c.UrmarestePartide }).ToList())
                conturiOperand[c.ID] = new(c.ID, c.Simbol, c.UrmarestePartide);
            operand = operand with { Conturi = conturiOperand };
        }
        var note = os.GetObjectsQuery<NotaContabila>().Where(d => d.Stare == StareDocument.Operat).Select(d => d.ID);
        var stornate = os.GetObjectsQuery<Document>().Where(d => d.Stare == StareDocument.Stornat).Select(d => d.ID);
        var randuri = os.GetObjectsQuery<C.Postare>()
            .Where(p => conturi.Contains(p.Cont) && p.Spatiu == N.Spatiu.Contabil && p.Carte == N.Carte.Contabil
                && p.Unitate == null && p.DocumentId != operand.Document.Id)
            .Select(p => new { p.ID, p.Spatiu, p.Tranzactie.Fel, p.LinieId, p.Data, p.Latura, p.Valoare,
                p.Cont, p.Partener, p.Gestiune, p.Produs, p.Valuta, p.CodFunctional, p.CodEconomic,
                p.SursaFinantare, p.UnitateOrganizatorica, p.Proiect, p.CentruCost, p.SuportId, p.InversaDinId,
                Stornata = p.DocumentId != null && stornate.Contains(p.DocumentId.Value),
                FaraLinieSursa = p.Tranzactie.Fel == N.FelTranzactie.Deschidere
                    || (p.DocumentId != null && note.Contains(p.DocumentId.Value)) })
            .ToList();
        var data = operand.Document.DataInregistrare;
        decimal Minim(IEnumerable<(DateOnly Data, decimal Delta)> delte) {
            decimal sold = 0m, minim = decimal.MaxValue;
            foreach (var zi in delte.GroupBy(d => d.Data <= data ? data : d.Data).OrderBy(g => g.Key)) {
                sold += zi.Sum(d => d.Delta);
                minim = Math.Min(minim, sold);
            }
            return minim == decimal.MaxValue ? 0m : minim;
        }
        var postari = randuri.Select(r => new { Rand = r, Capat = new N.Capat {
            Cont = r.Cont, Partener = r.Partener, Gestiune = r.Gestiune, Produs = r.Produs, Valuta = r.Valuta,
            Analiza = new(r.CodFunctional, r.CodEconomic, r.SursaFinantare, r.UnitateOrganizatorica, r.Proiect, r.CentruCost),
        }}).ToList();
        var disponibile = postari.GroupBy(p => p.Capat).Select(g => new DisponibilFapt(g.Key,
            Minim(g.Select(p => (p.Rand.Data, p.Rand.Latura == N.Latura.Debit ? p.Rand.Valoare : -p.Rand.Valoare))),
            Minim(g.Select(p => (p.Rand.Data, p.Rand.Latura == N.Latura.Credit ? p.Rand.Valoare : -p.Rand.Valoare)))))
            .ToList();
        var suporturi = postari.Where(p => !p.Rand.Stornata && p.Rand.Data <= data && p.Rand.Valoare > 0m
                && p.Rand.Fel is N.FelTranzactie.Operare or N.FelTranzactie.Deschidere)
            .Select(p => new SuportFapt(new(p.Rand.ID, p.Rand.Spatiu), p.Rand.LinieId, p.Capat, p.Rand.Latura,
                Minim(randuri.Where(r => r.ID == p.Rand.ID || r.SuportId == p.Rand.ID || r.InversaDinId == p.Rand.ID)
                    .Select(r => (r.Data, r.Valoare))), p.Rand.Data, p.Rand.FaraLinieSursa)).ToList();
        return operand with { Fise = rezultat, Suporturi = suporturi, DisponibilNominalizare = disponibile };
    }
}
