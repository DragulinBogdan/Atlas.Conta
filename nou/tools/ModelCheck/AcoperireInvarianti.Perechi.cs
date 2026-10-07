using DevExpress.ExpressApp;
using Microsoft.EntityFrameworkCore;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

static partial class AcoperireInvarianti {
    // Fiecare abatere aprinde o singură disjuncție a predicatului perechii; restul formei rămâne validă.
    static bool StricaPereche(IObjectSpace os, DbContext db, string abatere, N.FelTranzactie fel = N.FelTranzactie.Operare) {
        var tinta = os.GetObjectsQuery<C.Postare>()
            .Where(p => p.Pereche != null && p.Tranzactie.Fel == fel)
            .OrderBy(p => p.ID).Select(p => new { p.ID, p.Spatiu, p.DocumentId, p.LinieId, p.TranzactieId, p.Pereche }).FirstOrDefault();
        if (tinta == null) return false;
        var rand = db.Set<C.Postare>().Where(p => p.ID == tinta.ID && p.Spatiu == tinta.Spatiu);
        switch (abatere) {
            case "ordinal":
                db.Set<C.Postare>().Where(p => p.TranzactieId == tinta.TranzactieId && p.Pereche == tinta.Pereche)
                    .ExecuteUpdate(s => s.SetProperty(p => p.Pereche, 0)); break;
            case "cauza":
                var alta = os.GetObjectsQuery<C.Postare>().Where(p => p.LinieId != null && p.LinieId != tinta.LinieId)
                    .OrderBy(p => p.ID).Select(p => p.LinieId).FirstOrDefault();
                if (alta == null) return false;
                rand.ExecuteUpdate(s => s.SetProperty(p => p.LinieId, alta)); break;
            case "document":
                var altul = os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId != null && p.DocumentId != tinta.DocumentId)
                    .OrderBy(p => p.ID).Select(p => p.DocumentId).FirstOrDefault();
                if (altul == null) return false;
                rand.ExecuteUpdate(s => s.SetProperty(p => p.DocumentId, altul)); break;
            case "valoare": rand.ExecuteUpdate(s => s.SetProperty(p => p.Valoare, p => p.Valoare + 1m)); break;
            case "valuta": rand.ExecuteUpdate(s => s.SetProperty(p => p.ValoareValuta, p => p.ValoareValuta + 1m)); break;
            case "cantitate": rand.ExecuteUpdate(s => s.SetProperty(p => p.Cantitate, p => p.Cantitate + 1m)); break;
            case "latura":
                var pereche = os.GetObjectsQuery<C.Postare>()
                    .Where(p => p.TranzactieId == tinta.TranzactieId && p.Pereche == tinta.Pereche && p.ID != tinta.ID)
                    .Select(p => new { p.Latura, p.Valoare, p.ValoareValuta }).Single();
                // Operarea ajunge pe aceeași latură cu valori opuse; transferul, pe laturi opuse cu valori egale.
                var operare = fel == N.FelTranzactie.Operare;
                var latura = operare ? pereche.Latura
                    : pereche.Latura == N.Latura.Debit ? N.Latura.Credit : N.Latura.Debit;
                var semn = operare ? -1m : 1m;
                rand.ExecuteUpdate(s => s.SetProperty(p => p.Latura, latura)
                    .SetProperty(p => p.Valoare, semn * pereche.Valoare)
                    .SetProperty(p => p.ValoareValuta, semn * pereche.ValoareValuta)); break;
            default: throw new ArgumentOutOfRangeException(nameof(abatere));
        }
        return true;
    }

    // O postare a deschiderii primește ordinal și o contrapartidă pe aceeași latură, cu sumele opuse.
    static bool PerecheInDeschidere(IObjectSpace os, DbContext db) {
        var tinta = os.GetObjectsQuery<C.Postare>()
            .Where(p => p.Pereche == null && p.Tranzactie.Fel == N.FelTranzactie.Deschidere)
            .OrderBy(p => p.ID).Select(p => new { p.ID, p.Spatiu }).FirstOrDefault();
        if (tinta == null) return false;
        var id = Guid.NewGuid();
        db.Database.ExecuteSqlInterpolated($"""
            INSERT INTO "Postare"
            SELECT (jsonb_populate_record(NULL::"Postare", to_jsonb(p) ||
                jsonb_build_object('ID', {id}, 'Pereche', 1, 'Cantitate', -p."Cantitate",
                    'Valoare', -p."Valoare", 'ValoareValuta', -p."ValoareValuta"))).*
            FROM "Postare" p WHERE "ID" = {tinta.ID} AND "Spatiu" = {(int)tinta.Spatiu}
            """);
        db.Set<C.Postare>().Where(p => p.ID == tinta.ID && p.Spatiu == tinta.Spatiu)
            .ExecuteUpdate(s => s.SetProperty(p => p.Pereche, 1));
        return true;
    }

    static bool PerecheLangaTransformare(IObjectSpace os, DbContext db, decimal cantitate) {
        var contraponderi = os.GetObjectsQuery<C.Postare>().Where(C.Citiri.Transformare.Contrapondere);
        var tinta = os.GetObjectsQuery<C.Postare>()
            .Where(p => p.Tranzactie.Fel == N.FelTranzactie.Transfer && p.Pereche == null
                && p.FelUnitate == N.FelUnitate.Lot && p.Unitate != null
                && contraponderi.Any(c => c.TranzactieId == p.TranzactieId && c.DocumentId == p.DocumentId && c.LinieId == p.LinieId))
            .OrderBy(p => p.ID).Select(p => new { p.ID, p.Spatiu, p.TranzactieId }).FirstOrDefault();
        if (tinta == null) return false;
        var ordinal = (os.GetObjectsQuery<C.Postare>().Where(p => p.TranzactieId == tinta.TranzactieId)
            .Max(p => p.Pereche) ?? 0) + 1;
        foreach (var semn in new[] { -1m, 1m }) {
            var id = Guid.NewGuid();
            var q = semn * cantitate;
            db.Database.ExecuteSqlInterpolated($"""
                INSERT INTO "Postare"
                SELECT (jsonb_populate_record(NULL::"Postare", to_jsonb(p) ||
                    jsonb_build_object('ID', {id}, 'Pereche', {ordinal}, 'Cantitate', {q}, 'Valoare', {semn}))).*
                FROM "Postare" p WHERE "ID" = {tinta.ID} AND "Spatiu" = {(int)tinta.Spatiu}
                """);
        }
        if (Refuz(os, numaiPerechi: true) is { } refuz)
            throw new InvalidOperationException("Martorul cu transformare și pereche validă: " + refuz);
        db.Set<C.Postare>().Where(p => p.TranzactieId == tinta.TranzactieId && p.Pereche == ordinal)
            .ExecuteUpdate(s => s.SetProperty(p => p.Pereche, (int?)null));
        return true;
    }
}
