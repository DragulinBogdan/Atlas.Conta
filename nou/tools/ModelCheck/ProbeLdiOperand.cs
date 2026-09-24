using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

static class ProbeLdiOperand {
    public static void Ruleaza(Action<string, bool> check) {
        var data = new DateOnly(2015, 1, 5);
        var gest = Guid.NewGuid(); var tip = Guid.NewGuid(); var clasa = Guid.NewGuid();
        var stoc = Guid.NewGuid(); var cost = Guid.NewGuid(); var venit = Guid.NewGuid();
        var doc = new DocumentFapt(Guid.NewGuid(), "LDI", Guid.NewGuid(), data, data, null, false, null,
            new(gest, FelRepartitor.Gestiune, null, default),
            new(Guid.NewGuid(), FelRepartitor.UnitateInterna, null, CalitateRepartitor.Comisie), null, null, null);
        LinieOperand Linie(bool plus) {
            var id = Guid.NewGuid(); var produs = Guid.NewGuid();
            var lot = new LotFapt(Guid.NewGuid(), produs, tip, stoc, data, plus ? 0m : 10m) {
                GestiuneId = gest, LinieIntrareId = plus ? id : Guid.NewGuid(),
            };
            return new(id, tip, clasa, NaturaClasa.Stoc, stoc, lot.Id, lot, 1m, 0m, 0m, null, null,
                plus ? produs : null, plus ? tip : null, null, null, null, null, N.Analiza.Fara, null) {
                DiferentaInventar = new(plus ? DirectieDiferenta.Plus : DirectieDiferenta.Minus, plus ? 20m : null),
            };
        }
        var p = Linie(true); var m = Linie(false);
        RegulaContareFapt[] contare = [new(Guid.NewGuid(), null, NaturaClasa.Stoc, 1, false,
            SursaCont.TipMaterial, null, SursaCont.Explicit, venit, true, null, null, null),
            new(Guid.NewGuid(), tip, null, -1, false, SursaCont.Explicit, cost,
                SursaCont.TipMaterial, null, true, null, null, null)];
        RegulaStocFapt regula = new(Guid.NewGuid(), LaturaDocument.Predator, null, TipStoc.Magazie, 1, true);
        Operand Operand(params LinieOperand[] linii) => new(doc, linii, contare, [regula], null,
            new Dictionary<Guid, TipTvaFapt>(), new Dictionary<Guid, ContFapt>(),
            new Dictionary<Guid, N.Sold> { [m.Lot!.Id] = new(50m, 0m, 5m, 0m) }, null, [], null, null, null,
            new(2015, 1), new("probă", data));
        foreach (var conventie in new[] { MidpointRounding.ToEven, MidpointRounding.AwayFromZero }) {
            N.Contract Contract(Operand o) {
                var refuzuri = new List<N.Refuz>(); var rot = new N.Rotunjire(conventie);
                var declaratie = DeclarantDiferenteInventar.Instanta.Declara(o, rot, refuzuri);
                return declaratie == null ? N.Contract.Refuza(refuzuri, [], [], rot.JumatatiDeBan) : N.Motor.Opereaza(declaratie, rot);
            }
            void Refuza(string nume, Operand o, string cod) {
                var c = Contract(o);
                check($"SC-LDI-14/{conventie}/{nume}: cod {cod}, fără tranzacții", !c.EsteAcceptat
                    && c.Refuzuri.Any(r => r.Cod == cod) && c.Tranzactii.Count == 0);
            }
            var valid = Contract(Operand(p, m));
            check($"SC-LDI-01/02/{conventie}: plus 20, minus 10, o Operare și patru postări",
                valid.EsteAcceptat && valid.Tranzactii.Count == 1
                && valid.Tranzactii[0].Postari.Count == 4
                && valid.Tranzactii[0].Postari.Single(x => x.Coordonate.Cont == venit).Valoare == 20
                && valid.Tranzactii[0].Postari.Single(x => x.Coordonate.Cont == cost).Valoare == 10);
            Refuza("tip", Operand(p with { TipMaterialId = Guid.NewGuid() }), CoduriRefuz.ProdusAltTip);
            Refuza("produs", Operand(p with { ProdusId = Guid.NewGuid() }), CoduriRefuz.ProdusAltTip);
            Refuza("natura", Operand(p with { Natura = NaturaClasa.Serviciu }), CoduriRefuz.NaturaNepotrivita);
            Refuza("cont", Operand(p with { ContImplicitTipId = null }), CoduriRefuz.RegulaContareLipsa);
            Refuza("contare", Operand(p) with { ReguliContare = [] }, CoduriRefuz.RegulaContareLipsa);
            Refuza("stoc", Operand(p) with { ReguliStoc = [] }, CoduriRefuz.InventarStocNeacoperit);
            Refuza("custodie", Operand(p) with { ReguliStoc = [regula with { TipStoc = TipStoc.Custodie }] }, CoduriRefuz.InventarStocNeacoperit);
            Refuza("stoc multiplu", Operand(p) with { ReguliStoc = [regula, regula with { Id = Guid.NewGuid() }] }, CoduriRefuz.InventarStocNeacoperit);
            Refuza("gol", Operand(), CoduriRefuz.LiniiLipsa);
        }
    }
}
