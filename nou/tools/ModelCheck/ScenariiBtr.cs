using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using N = Atlas.Conta.Nucleu;
using C = Atlas.Conta.BackOffice.Module.Cub;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed class ScenariiBtr(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "BTR", 2004) {
    RandScena[] Randuri(FacturaScena d, int i, decimal q, decimal v) {
        var l = d.Linii[i];
        return [new(Cont(Stoc), N.Latura.Debit, -v, -q, Magazie, l.Lot, l.Produs, Linie: l.Id, Spatiu: N.Spatiu.Stoc),
            new(Cont(Stoc), N.Latura.Debit, v, q, Destinatie, l.Lot, l.Produs, Linie: l.Id, Spatiu: N.Spatiu.Stoc)];
    }

    void Solduri(string id, LinieScena l, DateOnly data, decimal qs, decimal vs, decimal qt, decimal vt) {
        SoldLot(id, l.Lot!.Value, Magazie, data, qs, vs);
        SoldLot(id, l.Lot.Value, Destinatie, data, qt, vt);
    }

    void Capete(Guid doc, Guid lot) => Comanda(os => {
        decimal Cub(Guid gestiune) => os.GetObjectsQuery<C.Postare>()
            .Where(p => p.DocumentId == doc && p.Unitate == lot && p.Gestiune == gestiune).Sum(p => (decimal?)p.Cantitate) ?? 0m;
        Verifica("SC-CIT-105", "transferul are ambele capete în cub: −4 pe sursă și +4 pe destinație",
            Cub(Magazie) == -4 && Cub(Destinatie) == 4);
        var db = ((EFCoreObjectSpace)os).DbContext;
        foreach (var (caz, gestiune) in new[] { ("destinației", Destinatie), ("sursei", Magazie) }) {
            using var tx = db.Database.BeginTransaction();
            db.Database.ExecuteSqlInterpolated(
                $"UPDATE \"Postare\" SET \"Cantitate\" = 0 WHERE \"DocumentId\" = {doc} AND \"Gestiune\" = {gestiune}");
            Refuza("SC-CIT-105/fără capătul " + caz, () => C.Citiri.Invarianti.Verifica(os), C.Citiri.Invarianti.TransferNeconservat);
            tx.Rollback();
        }
    });

    protected override void Executa() {
        var lot = Receptioneaza(new LinieFctScena(10, 10)).Linii[0];
        var d = Iesire(true, (lot, 4));
        Verifica("SC-BTR-01", "dry-run acceptat", CuSpatiu(os => ComenziDocument.Sistem(os).Valideaza(d.Id)).Count == 0);
        FaraEfecte("SC-BTR-01", d.Id); Opereaza(d.Id);
        Postari("SC-BTR-01", d.Id, N.FelTranzactie.Transfer, Ianuarie, Randuri(d, 0, 4, 40));
        Verifica("SC-BTR-01", "numai Transfer, fără tranzacție Operare", CuSpatiu(os =>
            os.GetObjectsQuery<C.Tranzactie>().Count(t => t.DocumentId == d.Id) == 1));
        Solduri("SC-BTR-01", lot, Ianuarie, 6, 60, 4, 40);
        Capete(d.Id, lot.Lot!.Value);
        var data = new DateOnly(An, 1, 20);
        Storneaza(d.Id, data);
        Postari("SC-BTR-03", d.Id, N.FelTranzactie.Transfer, Ianuarie, Randuri(d, 0, 4, 40));
        Postari("SC-BTR-03", d.Id, N.FelTranzactie.Storno, data, Randuri(d, 0, -4, -40));
        Verifica("SC-CIT-05", "BTR și inversa Transfer nu apar în citirea contabilă", CuSpatiu(os =>
            !C.Citiri.Contabil.Postari(os).Any(p => p.DocumentId == d.Id)));
        TransferPeCititori("SC-CIT-100", "Loturi.Postari", 2, 2, 4, d.Id);
        ListareTransfer("SC-CIT-107", "Loturi.Postari", d.Id);
        ProvenientaScrisa(d.Id);
        CostCitire(d.Id);
        ProvenientaInvalida(d.Id);
        Solduri("SC-BTR-03", lot, data, 10, 100, 0, 0);
        var amprenta = Amprenta(d.Id);
        Refuza("SC-BTR-03", () => Storneaza(d.Id, data), "Operat");
        Verifica("SC-BTR-03", "repetarea nu scrie", Amprenta(d.Id) == amprenta);
        var a = Iesire(true, (lot, 4)); Opereaza(a.Id); Anuleaza(a.Id);
        FaraEfecte("SC-BTR-05", a.Id); Solduri("SC-BTR-05", lot, data, 10, 100, 0, 0);
        Multiple();
        var insuf = Iesire(true, (lot, 11));
        RefuzDeclaratie("SC-BTR-08", insuf.Id, "STOC_INSUFICIENT");
        Refuza("SC-BTR-08", () => Opereaza(insuf.Id), "STOC_INSUFICIENT");
        FaraEfecte("SC-BTR-08", insuf.Id);
        Dependenti();
        var p = Iesire(true, (Receptioneaza(new LinieFctScena(10, 10)).Linii[0], 4)); Opereaza(p.Id);
        var c = Iesire(true, (Receptioneaza(new LinieFctScena(10, 10)).Linii[0], 4)); Opereaza(c.Id);
        InchideIanuarie();
        Refuza("SC-BTR-04", () => Anuleaza(p.Id), "închis");
        Storneaza(p.Id, Februarie);
        Postari("SC-BTR-04", p.Id, N.FelTranzactie.Storno, Februarie, Randuri(p, 0, -4, -40));
        Solduri("SC-BTR-04", p.Linii[0], new(An, 1, 31), 6, 60, 4, 40);
        Solduri("SC-BTR-04", p.Linii[0], Februarie, 10, 100, 0, 0);
        var nou = Corecteaza(c.Id); FaraEfecte("SC-BTR-06", nou);
        FacturaScena corectie;
        using (var os = Deschide()) {
            var doc = os.GetObjectsQuery<NotaTransfer>().Single(d => d.ID == nou);
            Verifica("SC-BTR-06", "corecție legată, motiv și date", doc.CorecteazaId == c.Id
                && doc.MotivCorectie == MotivCorectie.EroareMateriala && doc.Data == Ianuarie && doc.DataInregistrare == Februarie);
            var l = os.GetObjectsQuery<DocumentDetaliu>().Single(l => l.DocumentId == nou);
            l.Cantitate = 3; corectie = new(nou, [new(l.ID, l.LotId, c.Linii[0].Produs)]); os.CommitChanges();
        }
        Opereaza(nou);
        Postari("SC-BTR-06", c.Id, N.FelTranzactie.Transfer, Ianuarie, Randuri(c, 0, 4, 40));
        Postari("SC-BTR-06", c.Id, N.FelTranzactie.Storno, Februarie, Randuri(c, 0, -4, -40));
        Postari("SC-BTR-06", nou, N.FelTranzactie.Transfer, Februarie, Randuri(corectie, 0, 3, 30));
        Solduri("SC-BTR-06", c.Linii[0], Februarie, 7, 70, 3, 30);
    }

    void ProvenientaScrisa(Guid document) {
        using var os = Deschide();
        C.Citiri.Invarianti.VerificaProvenienta(os);
        var originale = os.GetObjectsQuery<C.Postare>()
            .Where(p => p.DocumentId == document && p.Tranzactie.Fel == N.FelTranzactie.Transfer)
            .Select(p => new { p.ID, p.Spatiu }).ToArray();
        var inverse = os.GetObjectsQuery<C.Postare>()
            .Where(p => p.DocumentId == document && p.Tranzactie.Fel == N.FelTranzactie.Storno)
            .Select(p => new { p.InversaDinId, p.InversaDinSpatiu }).ToArray();
        Verifica("SC-CIT-07", "fiecare inversă poartă exact originea ei Transfer", inverse.Length == originale.Length
            && originale.All(o => inverse.Count(i => i.InversaDinId == o.ID && i.InversaDinSpatiu == o.Spatiu) == 1));
        Verifica("SC-CIT-07", "BTR rămâne exclus din rulajele contabile", !C.Citiri.Contabil.Postari(os).Any(p => p.DocumentId == document));
    }

    void CostCitire(Guid document) {
        using var os = Deschide();
        var sql = NumaratorSql.Instanta;
        sql.Reseteaza();
        C.Citiri.Invarianti.VerificaProvenienta(os);
        var inainte = C.Citiri.Contabil.Postari(os).Count(p => p.DocumentId == document);
        Verifica("SC-CIT-09", "diagnostic + citire: două comenzi SQL", sql.Numar == 2);
        sql.Reseteaza();
        var citire = C.Citiri.Contabil.Postari(os).Where(p => p.DocumentId == document);
        Verifica("SC-CIT-09", "compunerea citirii nu execută SQL", sql.Numar == 0);
        var dupa = citire.Count();
        Verifica("SC-CIT-09", "citirea execută o singură comandă SQL", sql.Numar == 1);
        Verifica("SC-CIT-09", "A/B pe aceeași bază: zero rulaje BTR", inainte == 0 && dupa == 0);
    }

    void ProvenientaInvalida(Guid document) {
        foreach (var defect in new[] { "origine lipsă", "id inexistent", "spațiu greșit", "origine Storno", "proveniență validă" }) {
            using var os = Deschide();
            var db = ((EFCoreObjectSpace)os).DbContext;
            using var tranzactie = db.Database.BeginTransaction();
            var inverse = os.GetObjectsQuery<C.Postare>()
                .Where(p => p.DocumentId == document && p.Tranzactie.Fel == N.FelTranzactie.Storno)
                .Select(p => new { p.ID, p.Spatiu, p.InversaDinId, p.InversaDinSpatiu }).ToArray();
            var tinta = inverse[0];
            Guid? origine = defect == "origine lipsă" ? null : defect == "id inexistent" ? Guid.NewGuid()
                : defect == "origine Storno" ? inverse[1].ID : tinta.InversaDinId!.Value;
            N.Spatiu? spatiu = defect == "origine lipsă" ? null : defect == "spațiu greșit" ? N.Spatiu.Contabil
                : defect == "origine Storno" ? inverse[1].Spatiu : tinta.InversaDinSpatiu!.Value;
            db.Database.ExecuteSqlInterpolated($"""
                UPDATE "Postare" SET "InversaDinId" = {origine}, "InversaDinSpatiu" = {(short?)spatiu}
                WHERE "ID" = {tinta.ID} AND "Spatiu" = {(short)tinta.Spatiu}
                """);
            var valid = defect == "proveniență validă";
            if (valid) C.Citiri.Invarianti.VerificaProvenienta(os);
            else Refuza("SC-CIT-10", () => C.Citiri.Invarianti.VerificaProvenienta(os), "CITIRE_PROVENIENTA_LIPSA: 1");
            Verifica("SC-CIT-10", $"{defect}: inversa BTR nu intră în contabil",
                !C.Citiri.Contabil.Postari(os).Any(p => p.ID == tinta.ID && p.Spatiu == tinta.Spatiu));
            tranzactie.Rollback();
        }
    }

    void Multiple() {
        var f = Receptioneaza(new LinieFctScena(10, 10), new LinieFctScena(5, 20));
        var d = Iesire(true, (f.Linii[0], 2), (f.Linii[1], 3)); Opereaza(d.Id);
        Postari("SC-BTR-02", d.Id, N.FelTranzactie.Transfer, Ianuarie, [.. Randuri(d, 0, 2, 20), .. Randuri(d, 1, 3, 60)]);
        Solduri("SC-BTR-02", f.Linii[0], Ianuarie, 8, 80, 2, 20);
        Solduri("SC-BTR-02", f.Linii[1], Ianuarie, 2, 40, 3, 60);
        var mic = Receptioneaza(new LinieFctScena(3, 0.333333m)).Linii[0];
        var r = Iesire(true, (mic, 1), (mic, 2)); Opereaza(r.Id);
        Postari("SC-BTR-07", r.Id, N.FelTranzactie.Transfer, Ianuarie,
            [.. Randuri(r, 0, 1, 0.33m), .. Randuri(r, 1, 2, 0.67m)]);
        Solduri("SC-BTR-07", mic, Ianuarie, 0, 0, 3, 1);
    }

    void Dependenti() {
        var lot = Receptioneaza(new LinieFctScena(10, 10)).Linii[0];
        var d = Iesire(true, (lot, 4)); Opereaza(d.Id);
        var bcs = Consum(lot.Lot!.Value, 1, Destinatie); Opereaza(bcs);
        var inainte = Amprenta(d.Id) + Amprenta(bcs);
        Refuza("SC-X-08", () => Storneaza(d.Id, new(An, 1, 20)), "STOC_INSUFICIENT");
        Verifica("SC-X-08", "lanțul rămâne intact", Amprenta(d.Id) + Amprenta(bcs) == inainte);
        Solduri("SC-X-08", lot, new(An, 1, 20), 6, 60, 3, 30);
    }
}
