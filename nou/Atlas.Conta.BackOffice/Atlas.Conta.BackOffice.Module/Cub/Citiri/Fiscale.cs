using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub.Citiri;

public sealed class FaptFiscal {
    public Guid TranzactieId { get; set; }
    public Guid DocumentId { get; set; }
    public Guid DocumentFiscalId { get; set; }
    public Guid DetaliuId { get; set; }
    public Guid TipTvaId { get; set; }
    public Guid? PartenerId { get; set; }
    public SensTva Sens { get; set; }
    public RegimTva Regim { get; set; }
    public decimal Cota { get; set; }
    public bool DeImport { get; set; }
    public int PerioadaD300 { get; set; }
    public int PerioadaD394 { get; set; }
    public int PerioadaAn { get; set; }
    public int PerioadaLuna { get; set; }
    public DateOnly Data { get; set; }
    public DateOnly DataDocument { get; set; }
    public DateOnly DataExigibilitate { get; set; }
    public DateOnly? DataPrimire { get; set; }
    public DateOnly DataInregistrare { get; set; }
    public DateTime ScrisLa { get; set; }
    public bool Storno { get; set; }
    public bool InversaTehnica { get; set; }
    public bool RegularizareD300 { get; set; }
    public decimal Baza { get; set; }
    public decimal Tva { get; set; }
    public decimal Autocolectare { get; set; }
}

public static class Fiscale {
    public const string IzolareInsuficienta = "CITIRE_IZOLARE_INSUFICIENTA";

    /// <summary>Tranzacția de citire a unei declarații; cu <paramref name="cereIzolare"/>, o ambiantă sub RepeatableRead se refuză.</summary>
    public static IDbContextTransaction DeschideCitirea(IObjectSpace os, bool cereIzolare = false) {
        var db = ((EFCoreObjectSpace)os).DbContext.Database;
        if (db.CurrentTransaction is not { } ambianta)
            return db.BeginTransaction(IsolationLevel.RepeatableRead);
        if (cereIzolare && ambianta.GetDbTransaction().IsolationLevel
                is not (IsolationLevel.RepeatableRead or IsolationLevel.Serializable or IsolationLevel.Snapshot))
            throw new InvalidOperationException(
                $"{IzolareInsuficienta}: citirea declarației cere cel puțin RepeatableRead, tranzacția existentă are "
                + $"{ambianta.GetDbTransaction().IsolationLevel}.");
        return null;
    }

    public static string Versiune(IObjectSpace os, FormularFiscal formular, DateOnly deLa, DateOnly panaLa) {
        if (deLa.Day != 1 || panaLa != deLa.AddMonths(1).AddDays(-1)) return null;
        var fapte = IntreLuni(Fapte(os), deLa, panaLa, formular)
            .OrderBy(f => f.TranzactieId).ThenBy(f => f.DocumentId).ThenBy(f => f.DetaliuId)
            .ThenBy(f => f.TipTvaId).ThenBy(f => f.PartenerId).ThenBy(f => f.Sens).ToArray();
        return Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new {
            Formular = formular, Perioada = deLa.Year * 100 + deLa.Month, Fapte = fapte,
        })));
    }

    public static IQueryable<Postare> Postari(IObjectSpace os) =>
        os.GetObjectsQuery<Postare>().Where(p => p.TipTvaId != null);

    public static IQueryable<FaptFiscal> Fapte(IObjectSpace os) => Postari(os)
        .GroupBy(p => new {
            p.TranzactieId, p.DocumentId, p.DocumentFiscalId, p.LinieId, p.TipTvaId,
            p.Partener, p.SensTva, p.RegimTva, p.CotaTva, p.DeImport, p.PerioadaDeclarare,
            p.PerioadaD394, p.Data, p.DataDocument, p.DataExigibilitate, p.DataPrimire,
            p.DataInregistrare, p.Tranzactie.ScrisLa, p.Tranzactie.Fel,
            p.InversaTehnica, p.RegularizareD300,
        })
        .Select(g => new FaptFiscal {
            TranzactieId = g.Key.TranzactieId, DocumentId = g.Key.DocumentId.Value,
            DocumentFiscalId = g.Key.DocumentFiscalId.Value, DetaliuId = g.Key.LinieId.Value,
            TipTvaId = g.Key.TipTvaId.Value, PartenerId = g.Key.Partener,
            Sens = (SensTva)g.Key.SensTva.Value, Regim = (RegimTva)g.Key.RegimTva.Value,
            Cota = g.Key.CotaTva.Value, DeImport = g.Key.DeImport.Value,
            PerioadaD300 = g.Key.PerioadaDeclarare.Value, PerioadaD394 = g.Key.PerioadaD394.Value,
            PerioadaAn = g.Key.PerioadaDeclarare.Value / 100, PerioadaLuna = g.Key.PerioadaDeclarare.Value % 100,
            Data = g.Key.Data, DataDocument = g.Key.DataDocument.Value,
            DataExigibilitate = g.Key.DataExigibilitate.Value, DataPrimire = g.Key.DataPrimire,
            DataInregistrare = g.Key.DataInregistrare.Value, ScrisLa = g.Key.ScrisLa,
            Storno = g.Key.Fel == N.FelTranzactie.Storno,
            InversaTehnica = g.Key.InversaTehnica, RegularizareD300 = g.Key.RegularizareD300,
            Baza = g.Sum(p => p.RolTva == N.RolTva.Baza ? p.Valoare : 0m),
            Tva = g.Sum(p => p.RolTva == N.RolTva.Taxa ? p.Valoare : 0m),
            Autocolectare = g.Sum(p => p.RolTva == N.RolTva.Autocolectare ? p.Valoare : 0m),
        });

    public static IQueryable<FaptFiscal> IntreLuni(IQueryable<FaptFiscal> fapte,
            DateOnly? deLa, DateOnly? panaLa, FormularFiscal formular = FormularFiscal.D300) {
        if (deLa is { } de) {
            var perioada = de.Year * 100 + de.Month;
            fapte = formular == FormularFiscal.D394
                ? fapte.Where(f => f.PerioadaD394 >= perioada) : fapte.Where(f => f.PerioadaD300 >= perioada);
        }
        if (panaLa is { } la) {
            var perioada = la.Year * 100 + la.Month;
            fapte = formular == FormularFiscal.D394
                ? fapte.Where(f => f.PerioadaD394 <= perioada) : fapte.Where(f => f.PerioadaD300 <= perioada);
        }
        return fapte;
    }
}
