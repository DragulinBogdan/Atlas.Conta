using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using DevExpress.Persistent.BaseImpl.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Atlas.Conta.BackOffice.ModelCheck;

// F13-D2 — curățenia de scenă, după ce ModelCheck a primit interceptorul de
// ștergere amânată (`UseDeferredDeletion` pe `DbContextOptionsBuilder`).
//
// De ce nu mai merge `os.Delete` la curățenie: din momentul în care harness-ul
// șterge ca host-ul, `os.Delete` nu mai emite `DELETE`, ci
// `UPDATE … SET "GCRecord" = 1`. Rândul rămâne în tabelă — invizibil pentru
// interogări (filtrul global `GCRecord = 0` pus de suprasarcina pe `ModelBuilder`),
// dar prezent pentru BAZĂ: cheia primară rămâne ocupată (scenele care își dau
// Id-uri DETERMINISTE — fișa, `fa510000-…` — pică la a doua rulare cu violare de
// PK) și cascada fizică prin FK nu se mai declanșează (dependenții rămân, doar
// ei nu mai sunt nici măcar marcați).
//
// Regula feliei: **curățenia nu e probă, e infrastructură** — deci ștergere
// FIZICĂ, prin SQL brut, în ordinea FK dată de apelant (dependenții întâi);
// `os.Delete` rămâne DOAR acolo unde ștergerea logică e chiar obiectul probei
// (F5: maparea D300; proba 57f: linia de draft invizibilă la Committing).
//
// Două detalii pe care se sprijină tot mecanismul:
//   • **`IgnoreQueryFilters`** — culegerea rândurilor de purjat ocolește filtrul
//     global, altfel curățenia n-ar putea vedea (deci nici curăța) reziduul
//     lăsat de rulările anterioare: exact rândurile care ocupă PK-urile. Cu el,
//     baza se auto-vindecă la prima rulare, fără intervenție manuală.
//   • **rădăcina TPT** — purja se dă pe tabela RĂDĂCINĂ a ierarhiei
//     (`NotaTransfer` → `Documente`), fiindcă FK-urile derivatelor spre bază sunt
//     `ON DELETE CASCADE`: un `DELETE` pe `Documente` ia cu el rândul derivatei,
//     detaliile, `RegistruTva` și `Imperecheri`. Ce NU cascadează (`NO ACTION`:
//     `RegistruContabil`, `RegistruStoc`) se dă explicit, ÎNAINTEA documentelor —
//     de-aia ordinea pașilor e a apelantului, nu a helper-ului.
//
// Se citesc doar Id-urile (`Select(x => x.ID)`), niciodată entitățile: obiectele
// materializate ar rămâne în change tracker-ul EF după purjă, iar o scenă care
// recreează ACELAȘI Id determinist ar pica pe „another instance with the same key
// value is already being tracked".
sealed class Purja(IObjectSpace os) {
    readonly List<(Type Tip, List<Guid> Ids)> pasi = [];

    public Purja Adauga<T>(IQueryable<T> interogare) where T : BaseObject {
        var ids = interogare.IgnoreQueryFilters().Select(x => x.ID).Distinct().ToList();
        if (ids.Count > 0)
            pasi.Add((typeof(T), ids));
        return this;
    }

    // Comoditate pentru cazurile în care apelantul are deja obiectele în mână
    // (un draft creat de scenă, un `Detalii.ToList()`): tot Id-uri se purjează.
    public Purja Adauga<T>(IEnumerable<T> obiecte) where T : BaseObject {
        var ids = obiecte.Select(x => x.ID).Distinct().ToList();
        if (ids.Count > 0)
            pasi.Add((typeof(T), ids));
        return this;
    }

    public Purja Adauga<T>(T obiect) where T : BaseObject => Adauga([obiect]);

    public void Executa() {
        if (pasi.Count == 0)
            return;
        var ctx = ((EFCoreObjectSpace)os).DbContext;
        // Obiectele culese/create de scenă pot fi încă urmărite; un `DELETE` pe la
        // spatele lui EF ar lăsa în tracker rânduri fantomă. Detașarea e înainte de
        // SQL, ca identity map-ul să fie liber pentru Id-urile care se recreează.
        foreach (var (tip, ids) in pasi)
            foreach (var intrare in ctx.ChangeTracker.Entries().Where(e =>
                         tip.IsInstanceOfType(e.Entity) && e.Entity is BaseObject b && ids.Contains(b.ID)).ToList())
                intrare.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        foreach (var (tip, ids) in pasi) {
            var (tabela, coloanaId) = TabelaRadacina(ctx, tip);
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
