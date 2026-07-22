using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Microsoft.EntityFrameworkCore;

// Validare model EF + (dacă baza există) verificare migrații/seed.
// Aceeași țintă ca appsettings.json: Postgres localhost:5444.
var opts = new DbContextOptionsBuilder<BackOfficeEFCoreDbContext>()
    .UseNpgsql("Host=localhost;Port=5444;Username=postgres;Password=postgres;Database=Atlas.Conta.BackOffice")
    .UseChangeTrackingProxies()
    .Options;

using var ctx = new BackOfficeEFCoreDbContext(opts);
Console.WriteLine($"Model OK: {ctx.Model.GetEntityTypes().Count()} entity types");

if (!await ctx.Database.CanConnectAsync()) {
    Console.WriteLine("Baza nu există încă — doar validare de model.");
    return;
}

var pending = (await ctx.Database.GetPendingMigrationsAsync()).ToList();
var applied = (await ctx.Database.GetAppliedMigrationsAsync()).ToList();
Console.WriteLine($"Migrații aplicate: {applied.Count}; în așteptare: {pending.Count}"
    + (pending.Count > 0 ? $" ({string.Join(", ", pending)})" : ""));

Console.WriteLine($"TipuriDocument:  {await ctx.TipuriDocument.CountAsync()}");
Console.WriteLine($"ClaseProduse:    {await ctx.ClaseProduse.CountAsync()}");
Console.WriteLine($"TipuriMaterial:  {await ctx.TipuriMaterial.CountAsync()}");
Console.WriteLine($"Conturi:         {await ctx.Conturi.CountAsync()} (din care cu defalcare: {await ctx.Conturi.CountAsync(c => c.DimensiuniObligatorii != DimensiuneFlags.Niciuna)})");
Console.WriteLine($"Repartitori:     {await ctx.Repartitori.CountAsync()}");
Console.WriteLine($"PerioadeFiscale: {await ctx.PerioadeFiscale.CountAsync()}");
Console.WriteLine($"ReguliStoc:      {await ctx.ReguliStoc.CountAsync()}");
Console.WriteLine($"ReguliContare:   {await ctx.ReguliContare.CountAsync()}");

// Garda pentru limitarea owned + table sharing: cu navigația REQUIRED, un rând
// cu toate dimensiunile null trebuie să materializeze obiect gol, nu null.
var regula = await ctx.ReguliContare.FirstOrDefaultAsync();
if (regula != null)
    Console.WriteLine($"Owned all-null → DimensiuniComun {(regula.DimensiuniComun == null ? "NULL (BUG!)" : "instanță OK")}");

// Round-trip owned sub strategia de notificări (INotify* implementat manual pe
// Dimensiuni — instanța `new()` nu e proxy EF): insert cu FK pe dimensiune,
// update pe instanța materializată (proxy), apoi cleanup.
var tipDoc = await ctx.TipuriDocument.FirstAsync();
var repartitor = await ctx.Repartitori.FirstAsync();
var proba = ctx.CreateProxy<RegulaContare>(); // owner-ul TREBUIE proxy (ca în XAF)
proba.TipDocumentId = tipDoc.ID;
proba.DimensiuniComun = new Dimensiuni { RepartitorId = repartitor.ID };
ctx.ReguliContare.Add(proba);
await ctx.SaveChangesAsync();
ctx.ChangeTracker.Clear();

var recitita = await ctx.ReguliContare.SingleAsync(r => r.ID == proba.ID);
Console.WriteLine($"Owned insert → RepartitorId {(recitita.DimensiuniComun.RepartitorId == repartitor.ID ? "persistat OK" : "PIERDUT (BUG!)")}");

recitita.DimensiuniComun.RepartitorId = null;
recitita.DimensiuniComun.UnitateId = null; // no-op: null → null, nu trebuie să strice detectarea
await ctx.SaveChangesAsync();
ctx.ChangeTracker.Clear();

recitita = await ctx.ReguliContare.SingleAsync(r => r.ID == proba.ID);
Console.WriteLine($"Owned update → RepartitorId {(recitita.DimensiuniComun.RepartitorId == null ? "curățat OK" : "NEDETECTAT (BUG!)")}");

ctx.ReguliContare.Remove(recitita);
await ctx.SaveChangesAsync();
