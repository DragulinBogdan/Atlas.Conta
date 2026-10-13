using System.Diagnostics;
using System.Text.Json;
using System.Security.Cryptography;
using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using P = Atlas.Conta.BackOffice.Module.Cub.Citiri.Partide;
using C = Atlas.Conta.BackOffice.Module.Cub;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed class ScenaD9bScriere(Func<IObjectSpace> deschide, Action<string, bool> check, string director) {
    const string Marcaj = "E2E-SC-D9B-SCRIERE", Serviciu = "628", ContFurnizor = "401";
    static readonly DateOnly Ianuarie = new(2077, 1, 5);
    readonly List<Guid> documente = [];
    Guid loc;
    T CuSpatiu<T>(Func<IObjectSpace, T> actiune) { using var os = deschide(); return actiune(os); }
    void Opereaza(Guid id) => CuSpatiu(os => ComenziDocument.Sistem(os).Opereaza(id));
    void Anuleaza(Guid id) => CuSpatiu(os => ComenziDocument.Sistem(os).AnuleazaOperarea(id));
    void Verifica(string cod, string mesaj, bool rezultat) {
        check(cod + ": " + mesaj, rezultat);
        if (!rezultat) throw new InvalidOperationException(mesaj);
    }

    public void Ruleaza() {
        var pregatit = false; var reusit = false;
        var inainte = Fapte();
        try {
            using (var os = deschide()) {
                var l = os.GetObjectsQuery<UnitateInterna>().SingleOrDefault(x => x.Cod == Marcaj + "-LOC");
                var inceput = new DateOnly(2077, 1, 1); var sfarsit = new DateOnly(2077, 12, 31);
                var anul = os.GetObjectsQuery<Document>().Where(d => d.DataInregistrare >= inceput && d.DataInregistrare <= sfarsit).ToArray();
                if (l != null) {
                    if (anul.Length != 2 || anul.Any(d => d.PredatorId != l.ID || d.Stare != StareDocument.Draft))
                        throw new InvalidOperationException("Reluarea cere numai cele două drafturi ale referinței.");
                    loc = l.ID;
                }
                else {
                    if (os.GetObjectsQuery<PerioadaFiscala>().Any(p => p.An == 2077) || anul.Length != 0)
                        throw new InvalidOperationException(Marcaj + " cere anul 2077 liber.");
                    var p = os.CreateObject<PerioadaFiscala>(); p.An = 2077; p.Luna = 1;
                    l = os.CreateObject<UnitateInterna>(); l.Cod = Marcaj + "-LOC"; l.Denumire = l.Cod;
                    l.Calitati = CalitateRepartitor.LocConsum; loc = l.ID;
                    os.CommitChanges(); pregatit = true;
                }
            }
            Executa();
            var dupa = Fapte();
            Verifica("D9B-SCRIERE", "faptele și originile preexistente sunt restaurate", inainte == dupa);
            File.WriteAllText(Path.Combine(director, "scriere-mediu.json"), JsonSerializer.Serialize(new {
                FapteInainte = inainte, FapteDupa = dupa,
                ModelCheckSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(ScenaD9bScriere).Assembly.Location))),
                ModuleSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(C.Materializare).Assembly.Location)))
            }));
            reusit = true;
        }
        finally {
            if (!reusit && pregatit) {
                using var os = deschide(); var pj = new Purja(os);
                ProbeCub.Purjeaza(pj, os, documente);
                pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(x => documente.Contains(x.DocumentId)));
                pj.Adauga(os.GetObjectsQuery<Document>().Where(x => documente.Contains(x.ID)));
                pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(x => x.Cod.StartsWith(Marcaj)));
                pj.Adauga(os.GetObjectsQuery<PerioadaFiscala>().Where(x => x.An == 2077)); pj.Executa();
            }
        }
    }

    string Fapte() => CuSpatiu(os => {
        using var c = new NpgsqlConnection(((EFCoreObjectSpace)os).DbContext.Database.GetConnectionString()); c.Open();
        using var cmd = new NpgsqlCommand("""
            select json_build_object('baza', current_database(),
                'schema', (select string_agg("MigrationId", ',' order by "MigrationId") from "__EFMigrationsHistory"),
                'postari', (select md5(string_agg(md5(row_to_json(p)::text), '' order by p."ID")) from "Postare" p),
                'origini', (select md5(string_agg(md5(row_to_json(o)::text), '' order by o."ID")) from "OriginiPartide" o))::text
            """, c) { CommandTimeout = 600 };
        return (string)cmd.ExecuteScalar();
    });

    FacturaScena Nota(DateOnly data, LinieNtcScena[] linii) => CuSpatiu(os => {
        var n = os.CreateObject<NotaContabila>(); n.Data = data; n.DataInregistrare = data; n.PredatorId = loc; n.PrimitorId = loc;
        var conturi = os.GetObjectsQuery<Cont>().ToDictionary(c => c.Simbol, c => c.ID);
        var tip = os.GetObjectsQuery<TipMaterial>().Single(t => t.Cod == "TRZ").ID;
        for (var i = 0; i < linii.Length; i++) {
            var spec = linii[i]; var l = os.CreateObject<NotaContabilaDetaliu>(); l.Document = n;
            l.Pozitie = i + 1; l.TipMaterialId = tip; l.ContDebitId = conturi[spec.Debit]; l.ContCreditId = conturi[spec.Credit];
            l.RepartitorCreditId = spec.RepartitorCredit; l.Valoare = spec.Valoare;
        }
        os.CommitChanges(); documente.Add(n.ID); return new FacturaScena(n.ID, []);
    });

    void Executa() {
        foreach (var cate in new[] { 2, 51 }) {
            var existent = CuSpatiu(os => os.GetObjectsQuery<NotaContabila>().Where(n => n.PredatorId == loc && n.Detalii.Count == cate)
                .Select(n => (Guid?)n.ID).SingleOrDefault());
            var titulari = CuSpatiu(os => {
                if (existent != null) return os.GetObjectsQuery<NotaContabilaDetaliu>().Where(l => l.DocumentId == existent)
                    .OrderBy(l => l.Pozitie).Select(l => l.RepartitorCreditId.Value).ToArray();
                var ids = Enumerable.Range(0, cate).Select(i => {
                    var p = os.CreateObject<Partener>(); p.Cod = $"{Marcaj}-{cate}-{i}"; p.Denumire = p.Cod; return p.ID;
                }).ToArray();
                os.CommitChanges(); return ids;
            });
            var n = existent is Guid existentId ? new FacturaScena(existentId, [])
                : Nota(Ianuarie, titulari.Select(id => new LinieNtcScena(Serviciu, ContFurnizor, 100, RepartitorCredit: id)).ToArray());
            var masuri = new List<object>();
            for (var i = 0; i < 6; i++) {
                try {
                    var alocat = GC.GetTotalAllocatedBytes(true); var ceas = Stopwatch.StartNew();
                    var sql = CapturaSql.Masoara(() => Opereaza(n.Id));
                    var ms = ceas.Elapsed.TotalMilliseconds; alocat = GC.GetTotalAllocatedBytes(true) - alocat;
                    Verifica("D9B-SCRIERE", $"{cate} partide × 100, runda {i}", CuSpatiu(os =>
                        P.Origini(os).Count(x => x.DocumentId == n.Id) == cate
                        && P.Solduri(os, Ianuarie).Where(x => titulari.Contains(x.PartenerId)).Sum(x => x.Credit - x.Debit) == cate * 100
                        && os.GetObjectsQuery<C.Postare>().Count(x => x.DocumentId == n.Id) == 2 * cate
                        && os.GetObjectsQuery<C.Tranzactie>().Count(x => x.DocumentId == n.Id) == 1));
                    masuri.Add(new { Runda = i, Ms = ms, Comenzi = sql.Count, Alocati = alocat,
                        Sql = sql.Select(c => c.Text).ToArray() });
                }
                finally {
                    if (CuSpatiu(os => os.GetObjectByKey<Document>(n.Id).Stare == StareDocument.Operat)) Anuleaza(n.Id);
                }
                Verifica("D9B-SCRIERE", "starea de pornire restaurată: Draft fără postări proprii", CuSpatiu(os =>
                    os.GetObjectByKey<Document>(n.Id).Stare == StareDocument.Draft
                    && !os.GetObjectsQuery<C.Postare>().Any(x => x.DocumentId == n.Id)
                    && !os.GetObjectsQuery<C.Tranzactie>().Any(x => x.DocumentId == n.Id)));
            }
            File.WriteAllText(Path.Combine(director, $"scriere-{cate}.json"), JsonSerializer.Serialize(new { Document = n.Id, Partide = cate, Masuri = masuri }));
        }
    }
}
