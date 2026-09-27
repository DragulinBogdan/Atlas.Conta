using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

static class ProbeAsmOperand {
    public static void Ruleaza(Action<string, bool> check) {
        foreach (var conventie in new[] { MidpointRounding.ToEven, MidpointRounding.AwayFromZero }) {
            var gest = Guid.NewGuid(); var a = Guid.NewGuid(); var b = Guid.NewGuid();
            var solduri = new Dictionary<CheieLotFapt, N.Sold>();
            var registru = new Dictionary<Guid, N.Sold>();
            LinieOperand Linie(Guid cont, bool produs, decimal pret, decimal qSold = 0, decimal vSold = 0) {
                var id = Guid.NewGuid(); var tip = Guid.NewGuid(); var p = Guid.NewGuid();
                var lot = new LotFapt(Guid.NewGuid(), p, tip, cont, new(2014, 1, 1), pret) {
                    LinieIntrareId = produs ? id : Guid.NewGuid(), GestiuneId = gest,
                };
                if (!produs) { solduri[new(lot.Id, cont, p, gest)] = new(vSold, 0, qSold, 0); registru[lot.Id] = new(vSold, 0, qSold, 0); }
                return new(id, tip, null, NaturaClasa.Stoc, cont, lot.Id, lot, 1, 0, 0,
                    null, null, p, tip, null, null, null, null, N.Analiza.Fara, null) {
                    Transformare = new(produs ? N.RolTransformare.Produs : N.RolTransformare.Consum, produs ? pret : null),
                };
            }
            var data = new DateOnly(2014, 1, 5);
            var repartitor = new RepartitorFapt(gest, FelRepartitor.Gestiune, null, default);
            var doc = new DocumentFapt(Guid.NewGuid(), "ASM", Guid.NewGuid(), data, data, null,
                false, null, repartitor, repartitor, null, null, null);
            Operand Operand(params LinieOperand[] linii) => new(doc, linii, [], [], null,
                new Dictionary<Guid, TipTvaFapt>(), new Dictionary<Guid, ContFapt>(), solduri,
                null, [], null, null, null, new(2014, 1), new("probă", data)) { SolduriLoturiRegistru = registru };
            N.Declaratie Declara(Operand o, out List<N.Refuz> refuzuri) {
                refuzuri = [];
                return DeclarantAsamblare.Instanta.Declara(o, new(conventie), refuzuri);
            }
            var ca = Linie(a, false, 10m, 3, 29.94m);
            var cb = Linie(b, false, 1m, 2, 2.04m);
            var p1 = Linie(b, true, 10.99m); var p2 = Linie(b, true, .01m);
            var o = Operand(ca, cb, p1, p2);
            var d = Declara(o, out var erori);
            check($"SC-ASM-22/{conventie}: Δ −0,02 și +0,02 se acumulează înaintea gardului", d != null
                && erori.Count == 0 && d.Transformari.Last().Valoare == .01m
                && d.Decizii.OfType<N.AbsorbtieEvaluare>().Select(x => x.Delta).SequenceEqual([-.02m, .02m])
                && N.Motor.Opereaza(d, new(conventie)).EsteAcceptat);
            var repetat = Declara(o, out _);
            check($"SC-ASM-22/{conventie}: operand identic, declarație identică", d == repetat);
            check($"SC-ASM-13/{conventie}: lotul propriu identifică produsul și fără câmp de culegere",
                Declara(Operand(ca, cb, p1 with { ProdusId = null }, p2), out _) != null);
            check($"SC-ASM-13/{conventie}: produs cules diferit de lot, refuz",
                Declara(Operand(ca, cb, p1 with { ProdusId = Guid.NewGuid() }, p2), out var nepotrivit) == null
                && nepotrivit.Any(r => r.Cod == CoduriRefuz.AsamblareStructuraInvalida));
            if (d != null) {
                var tranzactii = N.Motor.Opereaza(d, new(conventie)).Tranzactii;
                check($"SC-ASM-16/{conventie}: Comparabil exclude numai cele patru contraponderi", Comparabil.Proiecteaza(tranzactii).Count == 4);
                var virtuala = tranzactii.SelectMany(t => t.Postari).First(Comparabil.EsteContrapondereTransformare);
                check($"SC-ASM-16/{conventie}: o contrapondere cu valoare nu este ascunsă",
                    !Comparabil.EsteContrapondereTransformare(virtuala with { Valoare = .01m }));
            }
            foreach (var cost in new[] { 9.99m, 9m }) {
                var c = Linie(a, false, 10, 2, cost * 2);
                var primul = Linie(a, true, 9.99m); var ultimul = Linie(a, true, .01m);
                var invalid = Declara(Operand(c, primul, ultimul), out var refuzuri);
                check($"SC-ASM-24/{conventie}: produs final {cost - 9.99m}, refuz atomic", invalid == null
                    && refuzuri.Any(r => r.Cod == CoduriRefuz.AsamblareProdusNepozitiv && r.Linie == ultimul.Id));
            }
            var faraCont = ca with { Lot = ca.Lot with { ContImplicitId = null } };
            check($"SC-ASM-13/{conventie}: cont de lot absent", Declara(Operand(faraCont, p1), out var lipsa) == null
                && lipsa.Any(r => r.Cod == CoduriRefuz.ContStocLipsa));
        }
        Diagnostic(check);
    }

    static void Diagnostic(Action<string, bool> check) {
        var cheie = new DiagnosticValoriStoc.Cheie(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var d1 = Guid.NewGuid(); var d2 = Guid.NewGuid();
        DiagnosticValoriStoc.Fapt F(bool cub, int zi, Guid doc, decimal q, decimal v, bool storno = false) =>
            new(cheie, new(2014, 1, zi), doc, Guid.NewGuid(), storno, q, v, cub, "probă");
        DiagnosticValoriStoc.Fapt[] istoric = [F(true, 1, d1, 1, 3.34m), F(false, 1, d1, 1, 3.33m),
            F(true, 2, d2, -1, -3.33m), F(false, 2, d2, -1, -3.33m),
            F(true, 3, d2, 1, 3.33m, true), F(false, 3, d2, 1, 3.33m, true),
            F(true, 4, d1, -1, -3.34m, true), F(false, 4, d1, -1, -3.33m, true)];
        var p = DiagnosticValoriStoc.Calculeaza(istoric, new(2014, 1, 2)).Single();
        check("SC-ASM-18/diagnostic: Δ istoric inversat exact, D inițial păstrat, fără filtrarea stornourilor",
            p.Initial == .01m && p.Delta == 0 && p.Vcub == 0 && p.Vregistru == 0
            && p.Qcub == 0 && p.Qregistru == 0 && !p.Incomplet
            && p.Initial + p.Intrari - p.Iesiri == 0);
        var incomplet = DiagnosticValoriStoc.Calculeaza(istoric.Where(f => f.Document != d1 || !f.Cub)).Single();
        check("SC-ASM/diagnostic: istoric lipsă separat de diferența valorică", incomplet.Incomplet);
        var necunoscut = DiagnosticValoriStoc.Calculeaza(istoric.Select(f => f.Document == d2 && f.Cub && !f.Storno
            ? f with { V = -3.32m } : f)).Single();
        check("SC-ASM/diagnostic: diferența neexplicată rămâne vizibilă, fără toleranță N-r3",
            !necunoscut.Incomplet && necunoscut.Delta == .01m && necunoscut.Reziduu);
        var raport = new DiagnosticValoriStoc.Raport(new(2014, 1, 31), [necunoscut], 0, 0, 0);
        check("SC-ASM/diagnostic: abaterea valorică rămâne raport, exit-ul aparține (a)–(g)",
            raport.AreAbateri && raport.Linii().Any(l => l.Contains("DIFERENȚĂ NEEXPLICATĂ"))
            && ReconciliereCub.CodIesire([]) == 0
            && ReconciliereCub.CodIesire([new("(b)", "probă", 1, 0)]) == 1);
        var magazie = new DiagnosticValoriStoc.CheieMiscare(cheie.Lot, cheie.Gestiune, d1, new(2014, 1, 1), false);
        var custodie = magazie with { Document = d2 };
        var faraRegistru = magazie with { Document = Guid.NewGuid() };
        var folosinta = magazie with { Document = Guid.NewGuid() };
        var excluse = DiagnosticValoriStoc.InAfaraDomeniului([
            (magazie, TipStoc.Magazie), (magazie, TipStoc.Folosinta), (custodie, TipStoc.Custodie), (folosinta, TipStoc.Folosinta)]);
        check("SC-ASM/diagnostic: domeniu comun; mișcarea externă exclusă, cea fizică sau fără corespondent păstrată",
            excluse.SetEquals([custodie]) && !excluse.Contains(faraRegistru) && !excluse.Contains(folosinta));
    }
}
