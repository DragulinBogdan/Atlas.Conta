using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

static class ProbeNirOperand {
    public static void Ruleaza(Action<string, bool> check) {
        var data = new DateOnly(2016, 1, 5);
        var gest = Guid.NewGuid(); var tip = Guid.NewGuid(); var stoc = Guid.NewGuid(); var furnizor = Guid.NewGuid();
        var doc = new DocumentFapt(Guid.NewGuid(), "NIR", Guid.NewGuid(), data, data, null, false, null,
            new(Guid.NewGuid(), FelRepartitor.Partener, null, default),
            new(gest, FelRepartitor.Gestiune, null, default), null, null, null);
        var id = Guid.NewGuid(); var produs = Guid.NewGuid();
        var lot = new LotFapt(Guid.NewGuid(), produs, tip, stoc, data, 0m) { GestiuneId = gest, LinieIntrareId = id };
        var linie = new LinieOperand(id, tip, Guid.NewGuid(), NaturaClasa.Stoc, stoc, lot.Id, lot, 6m, 999m, 0m,
            null, null, produs, tip, null, null, null, null, N.Analiza.Fara, null) { PretUnitar = 12.5m };
        RegulaContareFapt regula = new(Guid.NewGuid(), null, NaturaClasa.Stoc, 1, false,
            SursaCont.TipMaterial, null, SursaCont.Explicit, furnizor, true, null, null, null);
        Operand Operand(params LinieOperand[] linii) => new(doc, linii, [regula], [], null,
            new Dictionary<Guid, TipTvaFapt>(), new Dictionary<Guid, ContFapt>(),
            new Dictionary<CheieLotFapt, N.Sold>(), null, [], null, null, null, new(2016, 1));
        foreach (var conventie in new[] { MidpointRounding.ToEven, MidpointRounding.AwayFromZero }) {
            N.Contract Contract(Operand o) {
                var refuzuri = new List<N.Refuz>(); var rot = new N.Rotunjire(conventie);
                var declaratie = DeclarantNir.Instanta.Declara(o, rot, refuzuri);
                return declaratie == null ? N.Contract.Refuza(refuzuri, [], [], rot.JumatatiDeBan) : N.Motor.Opereaza(declaratie, rot);
            }
            void Refuza(string nume, Operand o, string cod) {
                var c = Contract(o);
                check($"SC-NIR-12/{conventie}/{nume}: cod {cod}, fără tranzacții", !c.EsteAcceptat
                    && c.Refuzuri.Any(r => r.Cod == cod) && c.Tranzactii.Count == 0);
            }
            void Valoare(string nume, LinieOperand l, decimal valoare) {
                var c = Contract(Operand(l));
                check($"SC-NIR-17/{conventie}/{nume}: recepție 6/{valoare}", c.EsteAcceptat
                    && c.Tranzactii.Count == 1 && c.Tranzactii[0].Postari.Count == 2
                    && c.Tranzactii[0].Postari.Single(p => p.Coordonate.Cont == stoc).Valoare == valoare);
            }
            Valoare("lot propriu cu preț cules", linie, 75m);
            Valoare("linie de bază, valoare culeasă", linie with { PretUnitar = null, Valoare = 42m }, 42m);
            Valoare("lot străin, prețul lotului", linie with { Lot = lot with { LinieIntrareId = Guid.NewGuid(), PretUnitar = 3m } }, 18m);
            Refuza("contare", Operand(linie) with { ReguliContare = [] }, CoduriRefuz.RegulaContareLipsa);
            Refuza("cont", Operand(linie with { ContImplicitTipId = null }), CoduriRefuz.RegulaContareLipsa);
            Refuza("contul lotului", Operand(linie with { Lot = lot with { ContImplicitId = Guid.NewGuid() } }), CoduriRefuz.ContStocLipsa);
            Refuza("produs", Operand(linie with { ProdusId = Guid.NewGuid() }), CoduriRefuz.ProdusAltTip);
            Refuza("gol", Operand(), CoduriRefuz.LiniiLipsa);
            Refuza("atomic", Operand(linie, linie with { Id = Guid.NewGuid(), Cantitate = -1 }), CoduriRefuz.CantitateNepozitiva);
        }
    }
}
