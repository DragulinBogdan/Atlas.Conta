using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using DevExtreme.AspNet.Data;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed class ScenariiFisa(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "FISA", 2021) {
    protected override void Executa() {
        FacturaScena N(int luna, int zi, string debit, string credit, decimal valoare) {
            var n = Nota(new(An, luna, zi), new LinieNtcScena(debit, credit, valoare));
            Opereaza(n.Id); return n;
        }
        var initial = N(1, 2, Serviciu, ContFurnizor, 100);
        Comanda(os => {
            Atlas.Conta.BackOffice.Module.Cub.Citiri.Invarianti.Verifica(os);
            Verifica("SC-CIT-23", "cubul acoperă registrul: invarianții trec", true);
            var db = ((EFCoreObjectSpace)os).DbContext;
            using var tx = db.Database.BeginTransaction();
            db.Database.ExecuteSqlInterpolated($"DELETE FROM \"Postare\" WHERE \"DocumentId\" = {initial.Id}");
            Refuza("SC-CIT-23", () => Atlas.Conta.BackOffice.Module.Cub.Citiri.Invarianti.Verifica(os), "CITIRE_ISTORIC_INCOMPLET");
            tx.Rollback();
        });
        var debit = N(2, 3, Serviciu, ContFurnizor, 60);
        var credit = N(2, 5, ContFurnizor, Serviciu, 25);
        var acelasi = N(2, 7, Serviciu, Serviciu, 40);
        var invers = N(2, 9, ContFurnizor, Serviciu, 10);
        Storneaza(invers.Id, new(An, 2, 11));
        var start = new DateOnly(An, 2, 1); var end = new DateOnly(An, 2, 12);
        Comanda(os => {
            var sursa = ContabilProiectii.FisaCont(os, Cont(Serviciu), start, end);
            var toate = sursa.ToList();
            Verifica("SC-CIT-18", "cumul 160, 135, 175/95, 135, 125, 135; identități distincte",
                toate.Count == 6 && toate[0].SoldCurent == 160 && toate[1].SoldCurent == 135
                && toate[2].SoldCurent is 175 or 95 && toate[3].SoldCurent == 135
                && toate[4].SoldCurent == 125 && toate[5].SoldCurent == 135
                && toate.Select(r => (r.Id, r.Spatiu)).Distinct().Count() == 6
                && toate[5] is { Credit: -10, Storno: true });
            var pagini = new List<FisaContRand>();
            foreach (var skip in new[] { 0, 2, 4 }) {
                var opt = new DataSourceLoadOptionsBase { Skip = skip, Take = 2 };
                OrdineLista.AplicaOrdineImplicita(opt, ContabilProiectii.OrdineFisa());
                pagini.AddRange(DataSourceLoader.Load(sursa, opt).data.Cast<FisaContRand>());
            }
            Verifica("SC-CIT-19", "paginile reproduc ordinea și soldurile integrale",
                pagini.Select(r => (r.Id, r.Spatiu, r.SoldCurent))
                    .SequenceEqual(toate.Select(r => (r.Id, r.Spatiu, r.SoldCurent))));
            var filtru = new DataSourceLoadOptionsBase {
                Filter = new object[] { "Credit", "=", 25m }, Take = 10
            };
            var filtrata = DataSourceLoader.Load(sursa, filtru).data.Cast<FisaContRand>().Single();
            Verifica("SC-CIT-19", "filtrarea grilei păstrează soldul 135", filtrata.SoldCurent == 135);
            var zi = new DateOnly(An, 2, 7);
            var oZi = ContabilProiectii.FisaCont(os, Cont(Serviciu), zi, zi).ToList();
            Verifica("SC-CIT-19", "o singură zi: două postări și final 135", oZi.Count == 2 && oZi[^1].SoldCurent == 135);
            var integral = ContabilProiectii.FisaCont(os, Cont(Serviciu), DateOnly.MinValue, end).ToList();
            var balantaIntegrala = ContabilProiectii.Balanta(os, DateOnly.MinValue, end)
                .Single(r => r.ContId == Cont(Serviciu));
            Verifica("SC-CIT-30", "data minimă: inițial zero și final 135 în fișă și balanță",
                integral.Count == 7 && integral[^1].SoldCurent == 135
                && balantaIntegrala.InitialDebit == 0 && balantaIntegrala.InitialCredit == 0
                && balantaIntegrala.SoldFinalDebit == 135);
            var jurnal = ContabilProiectii.RegistruJurnal(os, start, end).ToList();
            Verifica("SC-CIT-20", "jurnal: 10 postări, debit și credit 125",
                jurnal.Count == 10 && jurnal.Sum(r => r.Debit) == 125 && jurnal.Sum(r => r.Credit) == 125
                && jurnal.Select(r => (r.Id, r.Spatiu)).Distinct().Count() == 10);
            var ids = new[] { initial.Id, debit.Id, credit.Id, acelasi.Id, invers.Id };
            Verifica("SC-CIT-20", "fără perioadă: și cele două postări inițiale",
                ContabilProiectii.RegistruJurnal(os).Count(r => r.DocumentId != null && ids.Contains(r.DocumentId.Value)) == 12);
            ContabilProiectii.CompleteazaTipDocument(os, jurnal);
            Verifica("SC-CIT-22", "documente reale NTC și numere păstrate",
                jurnal.All(r => r.DocumentTip == "NTC" && !string.IsNullOrEmpty(r.DocumentNumar)));
        });
        var multi = Nota(new(An, 2, 13), new LinieNtcScena(Serviciu, Stoc, 20),
            new LinieNtcScena(ContFurnizor, Stoc, 30));
        Opereaza(multi.Id);
        Comanda(os => {
            var db = ((EFCoreObjectSpace)os).DbContext;
            using var tx = db.Database.BeginTransaction();
            db.Database.ExecuteSqlInterpolated($"DELETE FROM \"Postare\" WHERE \"DocumentId\" = {multi.Id} AND \"LinieId\" = {multi.Linii[0].Id}");
            Refuza("SC-CIT-34", () => Atlas.Conta.BackOffice.Module.Cub.Citiri.Invarianti.Verifica(os), "CITIRE_ISTORIC_INCOMPLET");
            tx.Rollback();
        });
        Comanda(os => {
            var db = ((EFCoreObjectSpace)os).DbContext;
            using var tx = db.Database.BeginTransaction();
            db.Database.ExecuteSqlInterpolated($"UPDATE \"Postare\" SET \"Valoare\" = \"Valoare\" + 1 WHERE \"DocumentId\" = {initial.Id} AND \"Cont\" = {Cont(Serviciu)}");
            Refuza("SC-CIT-34", () => Atlas.Conta.BackOffice.Module.Cub.Citiri.Invarianti.Verifica(os), "CITIRE_CUB_DEZECHILIBRAT");
            tx.Rollback();
        });
        Verifica("SC-CIT-21", "două contrapartide, fără ID unic inventat", CuSpatiu(os => {
            var r = ContabilProiectii.FisaCont(os, Cont(Stoc), new(An, 2, 13), new(An, 2, 13)).ToList();
            return r.Count == 2 && r.All(p => p.ContrapartidaId == null
                && p.ContrapartidaSimbol == string.Join(", ", new[] { Serviciu, ContFurnizor }.Order()))
                && r[^1].SoldCurent == -50;
        }));
        var anulata = N(2, 15, Serviciu, ContFurnizor, 15);
        Verifica("SC-CIT-22", "nota operată are două postări", CuSpatiu(os =>
            ContabilProiectii.RegistruJurnal(os).Count(r => r.DocumentId == anulata.Id) == 2));
        Anuleaza(anulata.Id);
        Verifica("SC-CIT-22", "anularea elimină postările din ambele rapoarte", CuSpatiu(os =>
            !ContabilProiectii.RegistruJurnal(os).Any(r => r.DocumentId == anulata.Id)
            && !ContabilProiectii.FisaCont(os, Cont(Serviciu), start, new(An, 2, 28)).Any(r => r.DocumentId == anulata.Id)));
    }
}
