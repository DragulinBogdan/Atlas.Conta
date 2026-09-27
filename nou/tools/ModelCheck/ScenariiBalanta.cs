using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using DevExtreme.AspNet.Data;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed class ScenariiBalanta(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "BALANTA", 2022) {
    protected override void CurataNomenclatoare(IObjectSpace os, Purja purja) {
        var db = ((EFCoreObjectSpace)os).DbContext;
        db.Database.ExecuteSql($"UPDATE \"Conturi\" SET \"ParinteId\" = NULL WHERE \"Simbol\" LIKE {Marcaj + "%"}");
        purja.Adauga(os.GetObjectsQuery<Cont>().Where(c => c.Simbol.StartsWith(Marcaj)));
    }

    protected override void Executa() {
        using var os = Deschide();
        Cont C(string cod, Cont parinte = null) {
            var c = os.CreateObject<Cont>(); c.Simbol = Marcaj + cod; c.Denumire = c.Simbol;
            c.Parinte = parinte; return c;
        }
        var a = C("-A"); var b = C("-B"); var zero = C("-ZERO"); var initial = C("-INITIAL");
        var nul = C("-NUL"); var ascuns = C("-ASCUNS"); var grup = C("-GRUP");
        var copil = C("-COPIL", grup); var nepot = C("-NEPOT", copil);
        var creditor = C("-CREDIT", grup); var orfan = C("-ORFAN", ascuns); var x = C("-X");
        var economic = Economic is Guid idEconomic ? os.GetObjectByKey<CodEconomic>(idEconomic) : os.CreateObject<CodEconomic>();
        economic.Cod = Marcaj; economic.Denumire = Marcaj;
        os.CommitChanges();
        Guid N(int luna, int zi, Cont debit, Cont credit, decimal valoare,
                Guid? pd = null, Guid? pc = null, bool analiza = false) {
            var id = CuSpatiu(o => {
                var doc = o.CreateObject<NotaContabila>(); doc.Data = new(An, luna, zi);
                doc.PredatorId = Loc; doc.PrimitorId = Loc;
                var l = o.CreateObject<NotaContabilaDetaliu>(); l.Document = doc;
                l.TipMaterialId = Tip(o, "TRZ"); l.ContDebitId = debit.ID; l.ContCreditId = credit.ID;
                l.RepartitorDebitId = pd; l.RepartitorCreditId = pc; l.Valoare = valoare;
                if (analiza) l.CodEconomicId = economic.ID;
                o.CommitChanges(); return doc.ID;
            });
            Opereaza(id); return id;
        }
        var start = new DateOnly(An, 2, 1); var end = new DateOnly(An, 2, 20);
        N(1, 5, a, b, 100, Furnizor, Furnizor);
        N(1, 5, b, a, 300, Client, Client);
        N(1, 7, initial, b, 70, Furnizor, Furnizor);
        N(2, 1, a, zero, 50, Furnizor, Furnizor, analiza: true);
        N(2, 20, zero, a, 50, Furnizor, Furnizor);
        N(2, 10, b, a, 40, Furnizor, Client);
        N(2, 21, a, b, 999, Furnizor, Furnizor);
        N(1, 15, nul, b, 200, null, Furnizor);
        N(2, 5, nul, b, 30, null, Furnizor);
        N(2, 6, nul, b, 11, Furnizor, Furnizor);
        N(2, 12, ascuns, b, 17, Furnizor, Furnizor);
        N(2, 2, copil, x, 12); N(2, 3, nepot, x, 4); N(2, 4, x, creditor, 30);
        N(2, 5, grup, x, 1); N(2, 7, orfan, x, 5);

        var sintetica = ContabilProiectii.Balanta(os, start, end).ToList();
        BalantaRand R(Cont c) => sintetica.Single(r => r.ContId == c.ID);
        Verifica("SC-CIT-26", "granițe și netare: inițial C 200, final C 240",
            R(a) is { InitialDebit: 100, InitialCredit: 300, RulajDebit: 50, RulajCredit: 90,
                SoldInitialCredit: 200, SoldFinalCredit: 240, SoldFinalDebit: 0 });
        Verifica("SC-CIT-26", "numai inițial 70 și rulaje nete zero rămân în raport",
            R(initial) is { InitialDebit: 70, RulajDebit: 0, RulajCredit: 0, SoldFinalDebit: 70 }
            && R(zero) is { RulajDebit: 50, RulajCredit: 50, SoldFinalDebit: 0, SoldFinalCredit: 0 });
        Verifica("SC-CIT-26", "partidă dublă",
            sintetica.Sum(r => r.RulajDebit) == sintetica.Sum(r => r.RulajCredit)
            && sintetica.Sum(r => r.InitialDebit) == sintetica.Sum(r => r.InitialCredit));
        var analitica = ContabilProiectii.Balanta(os, start, end, true).ToList();
        Verifica("SC-CIT-26", "netare distinctă per partener: D 100 și C 340",
            analitica.Single(r => r.ContId == a.ID && r.RepartitorId == Furnizor).SoldFinalDebit == 100
            && analitica.Single(r => r.ContId == a.ID && r.RepartitorId == Client).SoldFinalCredit == 340
            && sintetica.All(r => r.RepartitorId == null));
        Verifica("SC-CIT-26", "analiticul reconstituie toate rulajele sintetice",
            sintetica.All(r => analitica.Where(t => t.ContId == r.ContId).Sum(t => t.RulajDebit) == r.RulajDebit
                && analitica.Where(t => t.ContId == r.ContId).Sum(t => t.RulajCredit) == r.RulajCredit));
        var ianuarie = ContabilProiectii.Balanta(os, new(An, 1, 1), new(An, 1, 31)).ToList();
        Verifica("SC-CIT-26", "continuitate la granița lunii", ianuarie.All(i =>
            sintetica.Single(r => r.ContId == i.ContId) is var f
            && i.SoldFinalDebit - i.SoldFinalCredit == f.SoldInitialDebit - f.SoldInitialCredit));
        var fn = ContabilProiectii.FisaCont(os, nul.ID, start, end, repartitorNul: true).ToList();
        var fa = ContabilProiectii.FisaCont(os, nul.ID, start, end, repartitorId: Furnizor).ToList();
        var ft = ContabilProiectii.FisaCont(os, nul.ID, start, end).ToList();
        Verifica("SC-CIT-27", "fără partener 230, cu partener 11, sintetic 241",
            fn.Count == 1 && fn[0].SoldCurent == 230 && fa.Count == 1 && fa[0].SoldCurent == 11
            && ft.Count == 2 && ft[^1].SoldCurent == 241 && fn[0].Id != fa[0].Id);
        var filtru = ContabilProiectii.Balanta(os, start, end, codEconomicId: economic.ID).ToList();
        Verifica("SC-CIT-27", "filtru de analiză înaintea agregării: D/C 50", filtru.Count == 2
            && filtru.Single(r => r.ContId == a.ID) is { RulajDebit: 50, RulajCredit: 0, InitialDebit: 0 });
        var partener = ContabilProiectii.Balanta(os, start, end, repartitorId: Client).ToList();
        Verifica("SC-CIT-27", "filtru pe partener aplicat pe latură", partener.Count == 2
            && partener.Single(r => r.ContId == a.ID) is { InitialCredit: 300, RulajCredit: 40, RulajDebit: 0 }
            && partener.Single(r => r.ContId == b.ID) is { InitialDebit: 300, RulajDebit: 0 });
        foreach (var analitic in new[] { false, true }) {
            var sursa = ContabilProiectii.Balanta(os, start, end, analitic);
            List<BalantaRand> Pagina(int skip, int take) {
                var opt = new DataSourceLoadOptionsBase { Skip = skip, Take = take };
                OrdineLista.AplicaOrdineImplicita(opt, ContabilProiectii.OrdineBalanta(analitic));
                return DataSourceLoader.Load(sursa, opt).data.Cast<BalantaRand>().ToList();
            }
            var tot = Pagina(0, 1000); var pagini = new List<BalantaRand>();
            for (var skip = 0; skip < tot.Count; skip += 2) pagini.AddRange(Pagina(skip, 2));
            Verifica("SC-CIT-27", $"paginare cu cheie compusă, analitic={analitic}",
                tot.Select(r => (r.ContId, r.RepartitorId)).SequenceEqual(pagini.Select(r => (r.ContId, r.RepartitorId))));
        }

        var db = ((EFCoreObjectSpace)os).DbContext;
        using var tx = db.Database.BeginTransaction();
        AscundereControlata.Ascunde(db, "Conturi", ascuns.ID);
        var faraEticheta = ContabilProiectii.Balanta(os, start, end).ToList();
        Verifica("SC-CIT-29", "eticheta lipsă nu elimină suma 17", faraEticheta.Count == sintetica.Count
            && faraEticheta.Single(r => r.ContId == ascuns.ID) is { ContSimbol: null, RulajDebit: 17 }
            && ContabilProiectii.FisaCont(os, ascuns.ID, start, end).Single().SoldCurent == 17);
        var plan = ContabilProiectii.BalantaPlan(os, start, end);
        Verifica("SC-CIT-28", "pliul netează la nod: D 17/C 30, net C 13",
            plan.Single(r => r.ContId == grup.ID) is { RulajDebit: 17, RulajCredit: 30, SoldFinalCredit: 13 }
            && plan.Single(r => r.ContId == copil.ID) is { RulajDebit: 16, Nivel: 1 }
            && plan.Single(r => r.ContId == nepot.ID) is { RulajDebit: 4, Nivel: 2 });
        Verifica("SC-CIT-29", "copilul fără părinte vizibil devine rădăcină",
            plan.Single(r => r.ContId == orfan.ID) is { ParinteId: null, Nivel: 0, RulajDebit: 5 });
        var radacini = plan.Where(r => r.ParinteId == null).ToList();
        var limitat = ContabilProiectii.BalantaPlan(os, start, end, nivelMaxim: 1);
        Verifica("SC-CIT-28", "rădăcinile și adâncimea limitată păstrează sumele",
            radacini.Sum(r => r.RulajDebit) == sintetica.Sum(r => r.RulajDebit)
            && radacini.Sum(r => r.RulajCredit) == sintetica.Sum(r => r.RulajCredit)
            && limitat.Count == radacini.Count && limitat.All(r => !r.AreCopii)
            && limitat.Single(r => r.ContId == grup.ID).SoldFinalCredit == 13);
        db.Database.ExecuteSql($"UPDATE \"Conturi\" SET \"ParinteId\" = {nepot.ID} WHERE \"ID\" = {grup.ID}");
        Verifica("SC-CIT-28", "ciclul din nomenclator nu blochează citirea",
            ContabilProiectii.BalantaPlan(os, start, end).Single(r => r.ContId == creditor.ID).RulajCredit == 30);
        tx.Rollback();
    }
}
