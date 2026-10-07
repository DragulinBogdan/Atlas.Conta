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
            LinieOperand Linie(Guid cont, bool produs, decimal pret, decimal qSold = 0, decimal vSold = 0) {
                var id = Guid.NewGuid(); var tip = Guid.NewGuid(); var p = Guid.NewGuid();
                var lot = new LotFapt(Guid.NewGuid(), p, tip, cont, new(2014, 1, 1), pret) {
                    LinieIntrareId = produs ? id : Guid.NewGuid(), GestiuneId = gest,
                };
                if (!produs) solduri[new(lot.Id, cont, p, gest)] = new(vSold, 0, qSold, 0);
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
                null, [], null, null, null, new(2014, 1));
            N.Declaratie Declara(Operand o, out List<N.Refuz> refuzuri) {
                refuzuri = [];
                return DeclarantAsamblare.Instanta.Declara(o, new(conventie), refuzuri);
            }
            var ca = Linie(a, false, 10m, 3, 29.94m);
            var cb = Linie(b, false, 1m, 2, 2.04m);
            var p1 = Linie(b, true, 10.99m); var p2 = Linie(b, true, .01m);
            var o = Operand(ca, cb, p1, p2);
            var d = Declara(o, out var erori);
            check($"SC-ASM-22/{conventie}: ΣC = 9,98 + 1,02 = ΣP = 10,99 + 0,01 trece fără redistribuire; niciun grup balansat, o singură Operare",
                d != null && erori.Count == 0
                && d.Transformari.Select(t => t.Valoare).SequenceEqual([9.98m, 1.02m, 10.99m, .01m])
                && d.Transformari.All(t => t.Fel == N.FelTranzactie.Operare)
                && N.Motor.Opereaza(d, new(conventie)) is { EsteAcceptat: true, Tranzactii: [{ Fel: N.FelTranzactie.Operare }] });
            var repetat = Declara(o, out _);
            check($"SC-ASM-22/{conventie}: operand identic, declarație identică", d == repetat);
            check($"SC-ASM-13/{conventie}: lotul propriu identifică produsul și fără câmp de culegere",
                Declara(Operand(ca, cb, p1 with { ProdusId = null }, p2), out _) != null);
            check($"SC-ASM-13/{conventie}: produs cules diferit de lot, refuz",
                Declara(Operand(ca, cb, p1 with { ProdusId = Guid.NewGuid() }, p2), out var nepotrivit) == null
                && nepotrivit.Any(r => r.Cod == CoduriRefuz.AsamblareStructuraInvalida));
            foreach (var cost in new[] { 9.99m, 9m }) {
                var c = Linie(a, false, 10, 2, cost * 2);
                var primul = Linie(a, true, 9.99m); var ultimul = Linie(a, true, .01m);
                var operand = Operand(c, primul, ultimul);
                var invalid = Declara(operand, out var refuzuri);
                check($"SC-ASM-24/{conventie}: P = 10 contra C = {cost}, refuz de dezechilibru, fără altă decizie",
                    invalid == null && refuzuri is [{ Cod: CoduriRefuz.AsamblareNebalansata, Linie: null }]);
                var evaluare = new List<N.Refuz>();
                var consum = DeclarantAsamblare.Instanta.Consum(operand, new(conventie), evaluare);
                check($"SC-ASM-24/{conventie}: evaluarea consumului ({cost}) e disponibilă înaintea gardului de balansare",
                    evaluare.Count == 0 && consum is [{ Valoare: var evaluat, SoldCitit: not null }] && evaluat == cost && consum[0].Linie.Id == c.Id);
            }
            var cPlin = Linie(a, false, 10, 1, 10);
            var infim = Linie(a, true, .004m);
            check($"SC-ASM-24/{conventie}: produsul cules care se rotunjește la zero e refuzat pe valoarea culeasă",
                Declara(Operand(cPlin, infim), out var nepozitiv) == null
                && nepozitiv.Any(r => r.Cod == CoduriRefuz.AsamblareProdusNepozitiv && r.Linie == infim.Id));
            var faraCont = ca with { Lot = ca.Lot with { ContImplicitId = null } };
            check($"SC-ASM-13/{conventie}: cont de lot absent", Declara(Operand(faraCont, p1), out var lipsa) == null
                && lipsa.Any(r => r.Cod == CoduriRefuz.ContStocLipsa));
        }
    }
}
