using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Atlas.Conta.BackOffice.ModelCheck;

// Curățenia de scenă: ștergere FIZICĂ prin SQL brut, în ordinea dată de
// apelant — curățenia nu e probă, e infrastructură (F13-D2).
//
// Cascada e a purjei, nu a schemei (104g: `Cascade` numai în agregat): înainte
// de un pas se șterg, recursiv, rândurile legate prin FK OBLIGATORIU — cele care
// nu pot exista fără părinte. Referințele opționale rămân gardul: un rând din
// afara scenei care le ține oprește purja zgomotos.
//
// Se citesc doar Id-urile (`Select(x => x.ID)`), niciodată entitățile: obiectele
// materializate ar rămâne în change tracker-ul EF după purjă, iar o scenă care
// recreează ACELAȘI Id determinist ar pica pe „another instance with the same key
// value is already being tracked".
sealed class Purja(IObjectSpace os) {
    readonly List<(Type Tip, List<Guid> Ids)> pasi = [];

    public Purja Adauga<T>(IQueryable<T> interogare) where T : class, ICuCheie {
        var ids = interogare.Select(x => x.ID).Distinct().ToList();
        if (ids.Count > 0)
            pasi.Add((typeof(T), ids));
        return this;
    }

    // Comoditate pentru cazurile în care apelantul are deja obiectele în mână
    // (un draft creat de scenă, un `Detalii.ToList()`): tot Id-uri se purjează.
    public Purja Adauga<T>(IEnumerable<T> obiecte) where T : class, ICuCheie {
        var ids = obiecte.Select(x => x.ID).Distinct().ToList();
        if (ids.Count > 0)
            pasi.Add((typeof(T), ids));
        return this;
    }

    public Purja Adauga<T>(T obiect) where T : class, ICuCheie => Adauga([obiect]);

    // Cubul (S-D1) nu derivă din `EntitateConta`: aceeași purjă fizică, cu cheile date.
    public Purja AdaugaCheie<T>(IEnumerable<Guid> ids) where T : class {
        var distincte = ids.Distinct().ToList();
        if (distincte.Count > 0)
            pasi.Add((typeof(T), distincte));
        return this;
    }

    // Regulă de folosire (review F13, defect 6): purja detașează DOAR tipurile
    // purjate explicit; dependenții luați de cascadă (`RegistruTva`,
    // `Imperecheri`) rămân în tracker dacă scena i-a încărcat
    // înainte — un commit ulterior pe același OS ar da
    // `DbUpdateConcurrencyException`. Deci: purja la ÎNCEPUTUL scenei, pe OS
    // proaspăt, sau la sfârșit, pe un OS care nu se mai folosește.
    public void Executa() {
        if (pasi.Count == 0)
            return;
        var ctx = ((EFCoreObjectSpace)os).DbContext;
        // Obiectele culese/create de scenă pot fi încă urmărite; un `DELETE` pe la
        // spatele lui EF ar lăsa în tracker rânduri fantomă. Detașarea e înainte de
        // SQL, ca identity map-ul să fie liber pentru Id-urile care se recreează.
        foreach (var (tip, ids) in pasi)
            foreach (var intrare in ctx.ChangeTracker.Entries()
                         .Where(e => tip.IsInstanceOfType(e.Entity)).ToList())
                if (Cheia(intrare) is Guid id && ids.Contains(id))
                    intrare.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        foreach (var (tip, ids) in pasi) {
            StergeDependentii(ctx, ctx.Model.FindEntityType(tip)!.GetRootType(), ids, 0);
            var (tabela, coloanaId) = TabelaRadacina(ctx, tip);
            // Cubul atârnă de `Documente` prin FK: purja documentului îl ia cu ea. // S-D8
            if (tabela == "Documente")
                foreach (var alCubului in new[] { "Postare", "Tranzactie" }) {
                    var alCubuluiSql = $"DELETE FROM \"{alCubului}\" WHERE \"DocumentId\" = ANY(@p0)";
                    ctx.Database.ExecuteSqlRaw(alCubuluiSql, [ids.ToArray()]);
                }
            // Numele de tabelă/coloană vin din modelul EF (nu din date), Id-urile
            // rămân PARAMETRU (`uuid[]`) — SQL brut, dar nu concatenare de valori.
            var sql = $"DELETE FROM \"{tabela}\" WHERE \"{coloanaId}\" = ANY(@p0)";
            try {
                ctx.Database.ExecuteSqlRaw(sql, [ids.ToArray()]);
            }
            catch (Exception e) when (EsteViolareFk(e)) {
                // Ordinea contează și ÎN INTERIORUL unui pas, iar apelantul n-are cum
                // s-o știe: `DescarcariGestiuneDetalii.LinieSursaId` referă altă linie
                // din ACEEAȘI tabelă cu `ON DELETE RESTRICT` (38c: linia-sursă a
                // facturii), iar `RESTRICT` se verifică pe rând, nu la finalul
                // instrucțiunii ca `NO ACTION`. Reluarea în PASE rezolvă orice lanț
                // finit: fiecare pasă șterge ce a rămas fără dependenți, până când o
                // pasă nu mai mișcă nimic (atunci reziduul e o legătură REALĂ, spre
                // date din afara scenei — și atunci se aruncă zgomotos).
                var ramase = new List<Guid>(ids);
                while (ramase.Count > 0) {
                    var inainte = ramase.Count;
                    foreach (var id in ramase.ToList())
                        try {
                            ctx.Database.ExecuteSqlRaw(sql, [new[] { id }]);
                            ramase.Remove(id);
                        }
                        catch (Exception ex) when (EsteViolareFk(ex)) { }
                    if (ramase.Count == inainte)
                        throw new InvalidOperationException(
                            $"Purja nu poate șterge {ramase.Count} rând(uri) din \"{tabela}\" ({tip.Name}): "
                            + "ceva din AFARA scenei le mai referă. Prima cheie: " + ramase[0], e);
                }
            }
        }
        pasi.Clear();
    }

    // Dependenții prin FK obligatoriu ai rândurilor date, în adâncime, înaintea părintelui.
    static void StergeDependentii(DbContext ctx, IEntityType radacina, List<Guid> ids, int adancime) {
        if (adancime > 8)
            throw new InvalidOperationException($"Purja: lanț de dependențe prea adânc sub {radacina.Name}.");
        var tipuri = radacina.GetDerivedTypesInclusive().ToHashSet();
        foreach (var fk in ctx.Model.GetEntityTypes().SelectMany(e => e.GetDeclaredForeignKeys())) {
            if (!fk.IsRequired || !tipuri.Contains(fk.PrincipalEntityType)
                || !typeof(EntitateConta).IsAssignableFrom(fk.DeclaringEntityType.ClrType)
                || fk.DeclaringEntityType.GetRootType() == radacina)
                continue;
            var (tabela, coloanaId) = TabelaRadacina(ctx, fk.DeclaringEntityType.ClrType);
            var coloanaFk = fk.Properties[0].GetColumnName(
                StoreObjectIdentifier.Table(tabela, fk.DeclaringEntityType.GetRootType().GetSchema()));
            var copii = ctx.Database.SqlQueryRaw<Guid>(
                $"SELECT \"{coloanaId}\" AS \"Value\" FROM \"{tabela}\" WHERE \"{coloanaFk}\" = ANY(@p0)",
                [ids.ToArray()]).ToList();
            if (copii.Count == 0)
                continue;
            StergeDependentii(ctx, fk.DeclaringEntityType.GetRootType(), copii, adancime + 1);
            if (tabela == "Documente")
                foreach (var alCubului in new[] { "Postare", "Tranzactie" })
                    ctx.Database.ExecuteSqlRaw(
                        $"DELETE FROM \"{alCubului}\" WHERE \"DocumentId\" = ANY(@p0)", [copii.ToArray()]);
            ctx.Database.ExecuteSqlRaw($"DELETE FROM \"{tabela}\" WHERE \"{coloanaId}\" = ANY(@p0)", [copii.ToArray()]);
        }
    }

    static Guid? Cheia(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry intrare) =>
        intrare.Entity is ICuCheie cuCheie
            ? cuCheie.ID
            : intrare.Metadata.FindPrimaryKey()?.Properties is [{ } cheie]
                && intrare.Property(cheie.Name).CurrentValue is Guid id
                ? id
                : null;

    static bool EsteViolareFk(Exception e) {
        for (var x = e; x != null; x = x.InnerException)
            // 23503 = foreign_key_violation (`NO ACTION`), 23001 = restrict_violation
            // (`ON DELETE RESTRICT` — verificat pe rând, nu la finalul instrucțiunii).
            if (x is Npgsql.PostgresException pg && pg.SqlState is "23503" or "23001")
                return true;
        return false;
    }

    static (string Tabela, string ColoanaId) TabelaRadacina(DbContext ctx, Type tip) {
        var entitate = ctx.Model.FindEntityType(tip)
            ?? throw new InvalidOperationException($"Tipul {tip.Name} nu e în modelul EF.");
        var radacina = entitate.GetRootType();
        var tabela = radacina.GetTableName()
            ?? throw new InvalidOperationException($"Tipul {radacina.Name} n-are tabelă.");
        var cheie = radacina.FindPrimaryKey()
            ?? throw new InvalidOperationException($"Tipul {radacina.Name} n-are cheie primară.");
        var identificator = StoreObjectIdentifier.Table(tabela, radacina.GetSchema());
        var coloana = cheie.Properties[0].GetColumnName(identificator) ?? "ID";
        return (tabela, coloana);
    }
}
