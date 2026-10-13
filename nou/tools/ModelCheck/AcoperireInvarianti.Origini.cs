using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using Microsoft.EntityFrameworkCore;
using C = Atlas.Conta.BackOffice.Module.Cub;

namespace Atlas.Conta.BackOffice.ModelCheck;

static partial class AcoperireInvarianti {
    static bool OrigineFaraSuport(IObjectSpace os, DbContext db) {
        var document = os.GetObjectsQuery<Document>().Where(d => d.Stare == StareDocument.Draft
            && !os.GetObjectsQuery<C.Postare>().Any(p => p.DocumentId == d.ID)
            && !os.GetObjectsQuery<C.OriginePartidaPersistata>().Any(o => o.DocumentId == d.ID))
            .Select(d => (Guid?)d.ID).FirstOrDefault();
        var coord = os.GetObjectsQuery<C.OriginePartidaPersistata>()
            .Select(o => new { o.ContId, o.PartenerId }).FirstOrDefault();
        if (document == null || coord == null) return false;
        var id = C.Citiri.Partide.Identitate(document.Value, coord.ContId, coord.PartenerId);
        db.Database.ExecuteSqlInterpolated($"""
            INSERT INTO "OriginiPartide" ("ID", "ContId", "PartenerId", "DocumentId")
            VALUES ({id}, {coord.ContId}, {coord.PartenerId}, {document})
            """);
        return true;
    }

    static bool MutaOrigine(IObjectSpace os, DbContext db, string fel) {
        var q = os.GetObjectsQuery<C.OriginePartidaPersistata>();
        var initiala = fel is "ordinal" or "referinta";
        var id = q.Where(o => initiala ? o.DocumentId == null : o.DocumentId != null)
            .Select(o => (Guid?)o.ID).FirstOrDefault();
        if (id == null) return false;
        if (fel == "lipsa") db.Database.ExecuteSqlInterpolated($"DELETE FROM \"OriginiPartide\" WHERE \"ID\" = {id}");
        if (fel == "ordinal") db.Database.ExecuteSqlInterpolated($"UPDATE \"OriginiPartide\" SET \"OrdinalInitial\" = \"OrdinalInitial\" + 10000 WHERE \"ID\" = {id}");
        if (fel == "referinta") db.Database.ExecuteSqlInterpolated($"UPDATE \"OriginiPartide\" SET \"ReferintaInitiala\" = {Guid.NewGuid()} WHERE \"ID\" = {id}");
        if (fel == "straina") {
            var origine = q.Single(o => o.ID == id);
            var alt = os.GetObjectsQuery<Document>().Where(d => d.ID != origine.DocumentId
                && !q.Any(o => o.DocumentId == d.ID && o.ContId == origine.ContId && o.PartenerId == origine.PartenerId))
                .Select(d => (Guid?)d.ID).FirstOrDefault();
            if (alt == null) return false;
            db.Database.ExecuteSqlInterpolated($"UPDATE \"OriginiPartide\" SET \"DocumentId\" = {alt} WHERE \"ID\" = {id}");
        }
        return true;
    }
}
